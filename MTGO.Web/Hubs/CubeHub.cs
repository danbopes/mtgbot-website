using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using MTGO.Common;
using MTGO.Common.Models;
using MTGO.Web.Helpers;
using MTGO.Web.Infastructure;
using MTGO.Web.Services;
using MTGO.Web.TournamentLibrary;
using Microsoft.AspNet.SignalR;
using log4net;

namespace MTGO.Web.Hubs
{
    public class CubeHub : Hub
    {
        private MainDbContext _db = new MainDbContext();

        private static readonly ConcurrentDictionary<string, int> ConnectedClients = new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<int, ICubeDraftManager> DraftManagers = new ConcurrentDictionary<int, ICubeDraftManager>();
        private static readonly ConcurrentDictionary<int, SwissTournament> TournamentManagers = new ConcurrentDictionary<int, SwissTournament>();
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DraftService _draftService;

        public CubeHub()
        {
            _draftService = new DraftService(_db);
        }

        private static IBotService BotService
        {
            get
            {
                return new ChannelFactory<IBotService>(
                    new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/MTGOBotService")
                ).CreateChannel();
            }
        }

        private SwissTournament GetTournament(int draftId)
        {
            if (TournamentManagers.TryAdd(draftId, new SwissTournament()))
            {
                var draft = _db.CubeDrafts
                    .Include("CubeDraftResults")
                    .Include("CubeDraftPlayers")
                    .Include("CubeDraftPlayers.MtgoLink")
                    .Single(d=> d.Id == draftId);

                if (draft != null)
                {
                    foreach (var player in draft.CubeDraftPlayers.Where(p => p.Confirmed))
                    {
                        var playerObj = new Player();
                        playerObj.FromDB(player);

                        TournamentManagers[draftId].AddPlayer(playerObj);
                    }

                    foreach (var result in draft.CubeDraftResults)
                    {
                        var match = new TournMatch();
                        match.FromDB(result);

                        TournamentManagers[draftId].AddMatch(match);
                    }
                }
            }

            return TournamentManagers[draftId];
        }

        private class CacheCheck
        {
            public DateTime Last = DateTime.Now;
            public int Count = 1;
        }

        public override Task OnConnected()
        {
            Log.DebugFormat("Connect from client='{0}'", Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnReconnected()
        {
            Log.DebugFormat("Reconnect from client='{0}'", Context.ConnectionId);

            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            Log.DebugFormat("Disconnect from client='{0}'", Context.ConnectionId);
            int ignored;
            ConnectedClients.TryRemove(Context.ConnectionId, out ignored);

            try
            {
                foreach (var player in DraftManagers.SelectMany(x => x.Value.Players))
                    player.ClientIds.Remove(Context.ConnectionId);
            }
            catch
            {
            }
            /*foreach (var player in ConnectedPlayers.Where(c => c.ConnectedClients.Contains(Context.ConnectionId)))
            {
                player.ConnectedClients.Remove(Context.ConnectionId);

                if (player.ConnectedClients.Count == 0)
                    ConnectedPlayers.Remove(player);
            }*/

            return base.OnDisconnected();
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public bool SubscribeToDraft(int draftId)
        {
            /*string user = (string) HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;*/

            var userId = Context.User.GetUserId();

            var user = _db.Users.Find(userId);

            Log.DebugFormat("Subscribe to Draft: userId='{0}', username='{1}', connectionId='{2}'", userId, user.TwitchUsername, Context.ConnectionId);

            var draft = _db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftCards")
                .Include("CubeDraftPlayers.CubeDraftPicks")
                .Include("CubeDraftPlayers.MtgoLink.User")
                .SingleOrDefault(d => d.Id == draftId);

            if (draft == null)
                return false;

            Log.Debug(String.Format("SubscribeToDraft from userName='{0}', connectionId='{1}', draftId='{2}'", user.TwitchUsername, Context.ConnectionId, draftId));

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", draftId));
            ConnectedClients.TryAdd(Context.ConnectionId, user.Id);

            if (draft.Broadcaster.Name.ToLower() == user.TwitchUsername.ToLower())
                Groups.Add(Context.ConnectionId, String.Format("draft/{0}/broadcaster", draftId));

            var cardIds = draft.CubeDraftCards.Select(c => c.CardId).ToArray();

            var cardObjects = _db.Cards.Include("CardSet").Where(c => cardIds.Contains(c.Id)).ToArray();

            Clients.Caller.addedCards(cardIds.Select(c => cardObjects.Single(co => co.Id == c)).ToArray());

            foreach ( var player in draft.CubeDraftPlayers )
                Clients.Caller.playerUpdated(new PlayerUpdatedModel(player, draft));

            if (draft.Status == CubeDraftStatus.InMatches || draft.Status == CubeDraftStatus.ProductHandOut ||
                draft.Status == CubeDraftStatus.ProductHandIn || draft.Status == CubeDraftStatus.Completed)
            {
                var tournament = GetTournament(draftId);

                Clients.Caller.newPairings(tournament.CurrentRound, tournament.Matches.GetByRound(tournament.CurrentRound));

                var standings = tournament.GetStandings();

                if (standings != null)
                    Clients.Caller.newStandings(standings);

                if (tournament.CurrentRound == 0 && draft.CubeDraftPlayers.Where(p => p.Confirmed).All(p => p.DeckBuilt) &&
                    draft.Broadcaster.Name.ToLower() == user.TwitchUsername.ToLower())
                    Clients.Caller.allDecksBuilt();

            }
            return true;
        }

        /// <summary>
        /// Called from the mtgo bot service, when cards are added to the bots collection
        /// </summary>
        /// <param name="draftId">The id of the draft</param>
        /// <param name="cards">The cards that have been added</param>
        public void AddedCards(int draftId, int[] cards)
        {
            //Verify local rSequest
            if (!HttpContext.Current.Request.IsLocal)
                return;

            Log.DebugFormat("Added cards to draftid='{0}', cards='{1}", draftId, String.Join(", ", cards));

            var cardObjects = _db.Cards.Include("CardSet").Where(c => cards.Contains(c.Id)).ToArray();

            Clients.Group(String.Format("draft/{0}/clients", draftId)).addedCards(cards.Select(c => cardObjects.Single(co => co.Id == c)).ToArray());
        }

        /// <summary>
        /// Called from the mtgo bot service, when the draft get's a new status
        /// </summary>
        /// <param name="draftId">The id of the draft</param>
        /// <param name="newStatus">The new cube draft status</param>
        public void StatusUpdate(int draftId, CubeDraftStatus newStatus)
        {
            //Verify local request
            if (!HttpContext.Current.Request.IsLocal)
                return;

            Log.DebugFormat("StatusUpdate draftId='{0}', newStatus='{1}'", draftId, newStatus);

            Clients.Group(String.Format("draft/{0}/clients", draftId)).statusUpdate(newStatus);
        }

        public void QueueUpdate(int draftId, PlayerQueueStatus[] queue)
        {
            //Verify local request
            if (!HttpContext.Current.Request.IsLocal)
                return;

            Log.DebugFormat("QueueUpdate draftId='{0}', newStatus='{1}'", draftId, queue);

            Clients.Group(String.Format("drafting/{0}/clients", draftId)).queueUpdate(queue);
        }

        public void MatchUpdate(int draftId, int round, int player1Id, int player2Id, int currentGame, int player1Wins, int player2Wins, int ties)
        {
            Log.DebugFormat("MatchUpdate draftId='{0}': {1}, {2}, {3}, {4}, {5}, {6}, {7}", draftId, round, player1Id, player2Id, currentGame, player1Wins, player2Wins, ties);
            
            //Verify local request
            if (!HttpContext.Current.Request.IsLocal)
                return;

            /*using (var memStm = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(serializedMatch);
                memStm.Write(data, 0, data.Length);
                memStm.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(TournMatch));
                var match = (TournMatch) deserializer.ReadObject(memStm);
                
                
            }*/

            var tournament = GetTournament(draftId);

            var match = tournament.UpdateMatch(round, player1Id, player2Id, currentGame, player1Wins, player2Wins, ties);

            Clients.Group(String.Format("draft/{0}/clients", draftId)).matchUpdate(match);

            if (tournament.OutstandingMatches) return;

            var standings = tournament.GetStandings();

            if (standings != null)
            {
                Clients.Group(String.Format("draft/{0}/clients", draftId)).newStandings(standings);
            }
        }

        public void TradeUpdate(int draftId, int mtgoId, bool trading)
        {
            if (!HttpContext.Current.Request.IsLocal)
                return;

            Log.DebugFormat("TradeUpdate draftId='{0}', mtgoId='{1}', trading='{2}'", draftId, mtgoId, trading);

            var draft = _db.CubeDrafts
                .Include("CubeDraftCards")
                .Include("CubeDraftPlayers.CubeDraftPicks")
                .Include("CubeDraftPlayers.MtgoLink.User")
                .Single(c => c.Id == draftId);

            if (draft == null)
                return;

            var player = draft.CubeDraftPlayers.Single(p => p.MtgoLink.MtgoId == mtgoId);

            var pvm = new PlayerUpdatedModel(player, draft, trading);

            if (draft.Status == CubeDraftStatus.InMatches && pvm.DeckStatus != "full")
            {
                var tournament = GetTournament(draftId);
                Log.DebugFormat("Dropping player: playerId='{0}', players='{1}'", player.Id, tournament.Players);

                tournament.DropPlayer(player.Id);
                player.DropRound = tournament.CurrentRound;
                _db.Entry(player).State = EntityState.Modified;
                _db.SaveChanges();
            }
            
            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(pvm);
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public bool CancelDraft(int draftId)
        {
            var userId = Context.User.GetUserId();

            var user = _db.Users.Find(userId);

            if (user == null)
                return false;

            var draft = _db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftCards")
                .Single(c => c.Id == draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return false;

            //Make sure they are a broadcaster
            _draftService.EnsureBroadcaster(draft, user);

            if (draft.CubeDraftCards.Count > 0)
            {
                draft.Status = CubeDraftStatus.ProductHandIn;
                _db.Entry(draft).State = EntityState.Modified;
                _db.SaveChanges();
                BotService.UpdateCubeStatus(draft.Id);
            }
            else
            {
                draft.Status = CubeDraftStatus.Completed;
                _db.Entry(draft).State = EntityState.Modified;
                _db.SaveChanges();
            }

            return true;
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void SubscribeToDrafting(int draftId)
        {
            var userId = Context.User.GetUserId();

            var user = _db.Users.Find(userId);

            ICubeDraftManager manager;

            var draft = _db.CubeDrafts
                           .Include("CubeDraftPicks")
                           .Include("CubeDraftPlayers.MtgoLink.User")
                           .Include("CubeDraftCards")
                           .Single(d => d.Id == draftId);

            Log.DebugFormat("Subscribe to Drafting: userId='{0}', username='{1}', connectionId='{2}'", userId, user.TwitchUsername, Context.ConnectionId);

            Groups.Add(Context.ConnectionId, String.Format("drafting/{0}/clients", draftId));

            if (draft.Status != CubeDraftStatus.Drafting && draft.Status != CubeDraftStatus.PreStart)
                return;

            if ( !DraftManagers.TryGetValue(draftId, out manager) )
                DraftManagers.TryAdd(draftId, new CubeDraftManager(ref _db, draft));

            DraftManagers[draftId].PlayerSubscribe(user, Context.ConnectionId);
        }


        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public HubResponse CreateDraft(string name, int roundLimits, bool requireWatchers)
        {
            var user = _db.Users.Find(Context.User.GetUserId());

            //Should never happen, but an additional check just in case
            var broadcaster = _db.Broadcasters.FirstOrDefault(b => b.Name == user.TwitchUsername);

            if (broadcaster == null)
                throw new Exception("Sorry, this feature is only for broadcasters on twitch.tv (With a valid mtgbot.tv client application).");

            var newCubeDraft = new CubeDraft
                {
                    BroadcasterId = broadcaster.Id,
                    Created = DateTime.Now,
                    Name = name,
                    RoundTime = roundLimits,
                    RequireWatchers = requireWatchers
                };
            try
            {
                _db.CubeDrafts.Add(newCubeDraft);
                _db.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                return HubResponse.ErrorResponse("There was a problem saving this cube draft. If this problem persists, please contact the developer.");
            }

            string username = null;

            
            try
            {
                username = BotService.StartCubeDraft(newCubeDraft.Id);
            }
            catch (Exception ex)
            {
                Log.Error("Error calling BotService.StartCubeDraft(" + newCubeDraft.Id + ")", ex);
            }

            if (username == null)
                return HubResponse.ErrorResponse("Unable to create draft. There is no bot available to support this draft. Please try again later.");

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", newCubeDraft.Id));
            return HubResponse.SuccessResponse(new {
                     Username = username,
                     DraftId = newCubeDraft.Id
                });
        }
        
        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public HubResponse EndTournament(int draftId)
        {
            var user = _db.Users.Find(Context.User.GetUserId());

            var draft = _db.CubeDrafts
                .Include("Broadcaster").Single(c => c.Id == draftId);

            _draftService.EndTournament(draft, user);

            Clients.Group(String.Format("draft/{0}/clients", draftId)).statusUpdate(CubeDraftStatus.ProductHandIn);

            BotService.UpdateCubeStatus(draft.Id);

            return HubResponse.SuccessResponse();
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public bool StartDraft(int draftId)
        {
            var user = _db.Users.Find(Context.User.GetUserId());

            if (user == null)
                return false;

            var draft = _db.CubeDrafts
                .Include("CubeDraftCards")
                .Include("CubeDraftPlayers.MtgoLink.User")
                .Include("CubeDraftPicks")
                .Include("Broadcaster").Single(c => c.Id == draftId);

            if (draft.Status != CubeDraftStatus.PreStart)
                return false;

            _draftService.EnsureBroadcaster(draft, user);
            //Make sure they are a broadcaster
            if (DraftManagers.TryAdd(draft.Id, new CubeDraftManager(ref _db, draft)))
            {
                Log.InfoFormat("Draft starting: draftId='{0}'", draftId);
                return true;
            }
            return false;
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public HubResponse PairNextRound(int draftId)
        {
            try
            {
                var user = _db.Users.Find(Context.User.GetUserId());

                var draft = _db.CubeDrafts
                    .Include("Broadcaster").Single(c => c.Id == draftId);

                _draftService.EnsureBroadcaster(draft, user);

                Log.DebugFormat("Attempting to pair next round for draftId='{0}'", draftId);
                var tournament = GetTournament(draftId);
                //TODO: Remove
                //tournament.Matches.Clear();
                //tournament.CurrentRound = 0;
                var pairings = tournament.PairNextRound();

                Log.DebugFormat("Parings {0}", String.Join(", ", pairings));

                foreach (var match in pairings)
                    match.ToDB(_db, draftId);

                try
                {
                    BotService.NotifyPairings(draftId, pairings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return
                        HubResponse.ErrorResponse("There was a problem notifiying players of the next round. Please try again later.");
                }

                Clients.Group(String.Format("draft/{0}/clients", draftId))
                       .newPairings(tournament.CurrentRound, pairings);

                return HubResponse.SuccessResponse();
            }
            catch (InvalidOperationException ex)
            {
                return HubResponse.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return HubResponse.ErrorResponse("An unhandled exception occurred: " + ex.Message);
            }
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void TakePick(int draftId, int pickNumber, int pickId)
        {
            var userId = Context.User.GetUserId();

            Log.DebugFormat("TakePick: draftId='{0}', pickNumber='{1}', pickId='{2}'", draftId, pickNumber, pickId);

            try
            {
                ICubeDraftManager draftManager;

                if (!DraftManagers.TryGetValue(draftId, out draftManager))
                    return;

                //var player = _db.CubeDraftPlayers.Single(p => p.MtgoLink.UserId == userId);

                draftManager.Pick((int)userId, pickNumber, pickId);
            }
            catch (Exception ex)
            {
                Log.Error("Error on TakePick", ex);
                throw;
            }
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void BuiltDeck(int draftId)
        {
            var userId = Context.User.GetUserId();

            var draft = _db.CubeDrafts
                .Include("CubeDraftCards")
                .Include("CubeDraftPlayers.CubeDraftPicks")
                           .Include("CubeDraftPlayers.MtgoLink.User")
                           .Single(d => d.Id == draftId);

            var player = draft.CubeDraftPlayers.Single(p => p.MtgoLink.UserId == userId);

            if (player.DeckBuilt) return;

            player.DeckBuilt = true;

            _db.Entry(player).State = EntityState.Modified;

            _db.SaveChanges();

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(new PlayerUpdatedModel(player, draft));

            if (draft.CubeDraftPlayers.Where(p => p.Confirmed).All(p => p.DeckBuilt) && (draft.Status == CubeDraftStatus.InMatches || draft.Status == CubeDraftStatus.ProductHandOut))
                Clients.Group(String.Format("draft/{0}/broadcaster", draftId)).allDecksBuilt();
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public int Signup(int draftId)
        {
            try
            {
                var userId = Context.User.GetUserId();
                var user = _db.Users
                    .Include("MtgoLinks")
                    .Single(u => u.Id == userId);

                Log.DebugFormat("Signup from userId='{0}', user='{1}'", userId, user.TwitchUsername);

                var draft = _db.CubeDrafts
                    .Include("CubeDraftCards")
                    .Include("CubeDraftPlayers.CubeDraftPicks")
                    .Include("CubeDraftPlayers.MtgoLink.User")
                    .SingleOrDefault(d => d.Id == draftId);


                if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                {
                    Log.Debug("Draft not in PreStart");
                    return 0;
                }

                var dbUser = user.MtgoLinks.FirstOrDefault();

                if (dbUser == null)
                {
                    Log.Debug("No link found, sending 1");
                    return 1;
                }

                if (!dbUser.Confirmed)
                {
                    Log.Debug("Link found, not confirmed, sending 2");
                    return 2;
                }

                var newPlayer = new CubeDraftPlayer
                {
                    Confirmed = false,
                    ProductGiven = false,
                    MtgoLinkId = dbUser.Id,
                    Position = 0,
                    RequireCollateral = 0,
                    Team = 0,
                    DropRound = -1
                };

                try
                {
                    draft.CubeDraftPlayers.Add(newPlayer);

                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Debug("Exception adding CubeDraftPlayer", ex);
                    draft.CubeDraftPlayers.Remove(newPlayer);
                    return 0;
                }

                Log.DebugFormat("Player Added: playerId='{0}', mtgoLinkId='{1}'. Sending 3", newPlayer.Id, newPlayer.MtgoLinkId);
                Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(new PlayerUpdatedModel(newPlayer, draft));

                return 3;
            }
            catch (Exception ex)
            {
                Log.Debug("Uncaught expcetion in Signup", ex);
                Log.DebugFormat("Inner Exception: {0}", ex.InnerException);
                throw;
            }
            
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public async Task<object> GetPlayerInfo(int draftId, int targetId)
        {
            var userId = Context.User.GetUserId();

            var user = _db.Users.Find(userId);

            var draft = _db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftPlayers.MtgoLink.User")
                .Single(c => c.Id == draftId);

            //Make sure they are a broadcaster
            _draftService.EnsureBroadcaster(draft, user);

            var targetPlayer = draft.CubeDraftPlayers.Single(p => p.Id == targetId);

            var info = await new LogParser(draft.Broadcaster.Name).FetchInfoAsync(targetPlayer.MtgoLink.User.TwitchUsername);

            //var player = _db.MtgoLink.First(u => u.UserId == userId && u.Confirmed);

            return new
                {
                    targetPlayer.Id,
                    targetPlayer.MtgoLink.User.TwitchUsername,
                    targetPlayer.MtgoLink.MtgoUsername,
                    Approved = targetPlayer.Confirmed,
                    RequireCollateral = targetPlayer.Confirmed ? targetPlayer.RequireCollateral : 0,
                    info.Joins,
                    info.MessageCount,
                    info.Last50Messages
                };
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void UpdatePlayer(int draftId, int playerId, bool approved, int collateral = 0)
        {
            var userId = Context.User.GetUserId();

            var user = _db.Users.Find(userId);

            if (user == null)
                return;

            var draft = _db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftCards")
                .Include("CubeDraftPlayers.CubeDraftPicks")
                .Include("CubeDraftPlayers.MtgoLink.User").Single(c => c.Id == draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return;

            //Make sure they are a broadcaster
            _draftService.EnsureBroadcaster(draft, user);

            if (!approved)
                collateral = 0;

            var player = draft.CubeDraftPlayers.Single(p => p.Id == playerId);

            player.Confirmed = approved;
            player.RequireCollateral = collateral;

            _db.Entry(player).State = EntityState.Modified;

            _db.SaveChanges();

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(new PlayerUpdatedModel(player, draft));
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public int LinkUsername(string mtgousername)
        {
            var userId = Context.User.GetUserId();
            var user = _db.Users
                .Include("MtgoLinks")
                .Single(u => u.Id == userId);

            var dbUser = user.MtgoLinks.FirstOrDefault();

            if (dbUser != null)
                return 0;

            var confirmKey = new Random().Next(1000, 9999);

            if (_db.MtgoLink.Any(u => u.MtgoUsername == mtgousername && u.Confirmed))
                return 2;

            var mtgoUser = BotService.SendMessage(mtgousername,
                                                  String.Format(
                                                      @"[sHat] Welcome to the MTGBot Cube Drafting System! Your confirmation code is '{0}'. If you did not initiate this request, please ignore this message and do nothing.",
                                                      confirmKey));
            if ( mtgoUser == null )
                return 1;


            _db.MtgoLink.Add(new MtgoLink
                {
                    ConfirmKey = confirmKey.ToString(CultureInfo.InvariantCulture),
                    Confirmed = false,
                    UserId = user.Id,
                    MtgoUsername = mtgoUser.Username,
                    MtgoId = mtgoUser.PlayerId
                });

            _db.SaveChanges();

            return 3;
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public int ConfirmUsername(int confirmKey)
        {
            var userId = Context.User.GetUserId();

            if (userId == null)
                return 0;

            //Check cache and throw error if user is trying too much
            var key = "confirmusername-" + userId;
            var cache = HttpRuntime.Cache[key];
            if (cache != null)
            {
                var cacheCheck = (CacheCheck)cache;

                if (DateTime.Now > cacheCheck.Last.AddMinutes(5))
                    cacheCheck = new CacheCheck();

                if (cacheCheck.Count > 2)
                    return 2;

                cacheCheck.Count++;
                cacheCheck.Last = DateTime.Now;
            }
            else
            {
                HttpRuntime.Cache.Add(key,
                                         new CacheCheck(),
                                         null,
                                         Cache.NoAbsoluteExpiration,
                                         TimeSpan.FromMinutes(5),
                                         CacheItemPriority.Low,
                                         null);
            }

            var dbUser = _db.MtgoLink.FirstOrDefault(l => l.UserId == userId);

            if (dbUser == null)
                return 0;

            if (dbUser.Confirmed)
                return 2;

            if (_db.MtgoLink.Any(u => u.MtgoUsername == dbUser.MtgoUsername && u.Confirmed))
                return 3;

            if (dbUser.ConfirmKey != confirmKey.ToString(CultureInfo.InvariantCulture))
                return 1;

            dbUser.Confirmed = true;
            dbUser.ConfirmKey = null;
            _db.Entry(dbUser).State = EntityState.Modified;
            _db.SaveChanges();

            return 4;
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public Guid? InitReadyCheck(int draftId)
        {
            var userId = Context.User.GetUserId();
            var user = _db.Users.Find(userId);

            if (user == null)
                return null;

            var draft = _db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftPlayers.MtgoLink")
                .SingleOrDefault(d => d.Id == draftId);

            if (draft == null)
                return null;

            _draftService.EnsureBroadcaster(draft, user);

            var guid = Guid.NewGuid();

            var approvedClientIds =
                draft.CubeDraftPlayers.Where(p => p.Confirmed && p.MtgoLink.UserId != userId)
                     .Select(p => p.MtgoLink.UserId)
                     .ToArray();

            var broadcasterPlayer =
                draft.CubeDraftPlayers.FirstOrDefault(p => p.Confirmed && p.MtgoLink.UserId == userId);

            foreach (var client in ConnectedClients.Where(c => approvedClientIds.Contains(c.Value)).Select(x => x.Key))
            {
                Clients.Client(client).readyCheck(guid);
                if ( broadcasterPlayer != null )
                    Clients.Client(client).readyCheckUpdate(guid, broadcasterPlayer.Id, true);
            }

            return guid;
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void ReadyCheck(Guid guid, int draftId, bool ready)
        {
            var userId = Context.User.GetUserId();

            Log.DebugFormat("ReadyCheckUpdate (Start): guid='{0}', user='{1}', ready='{2}'", guid, userId, ready);

            if (userId == null)
                return;

            var draft = _db.CubeDrafts
                .Include("CubeDraftPlayers.MtgoLink")
                .Single(d => d.Id == draftId);

            var player = draft.CubeDraftPlayers.Single(p => p.MtgoLink.UserId == userId);

            Log.DebugFormat("ReadyCheckUpdate: guid='{0}', playerId='{1}', ready='{2}'", guid, player.Id, ready);
            Clients.Group(String.Format("draft/{0}/clients", draftId)).readyCheckUpdate(guid, player.Id, ready);
        }
    }
}