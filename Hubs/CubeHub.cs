using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using MTGBotWebsite.Helpers;
using MTGBotWebsite.TournamentLibrary;
using MTGOLibrary;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.Hubs
{
    public class CubeHub : Hub
    {
        private static object LockObject = new object();
        private MainDbContext _db = new MainDbContext();

        private static readonly ConcurrentDictionary<string, string> ConnectedClients = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<int, ICubeDraftManager> DraftManagers = new ConcurrentDictionary<int, ICubeDraftManager>();

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

        private class CacheCheck
        {
            public DateTime Last = DateTime.Now;
            public int Count = 1;
        }

        public override Task OnDisconnected()
        {
            string ignored;
            ConnectedClients.TryRemove(Context.ConnectionId, out ignored);

            try
            {
                foreach (var player in DraftManagers.SelectMany(x => x.Value.Players))
                {
                    player.ClientIds.Remove(Context.ConnectionId);
                }
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

        public bool SubscribeToDraft(int draftId)
        {
            string user = (string) HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null)
                return false;

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", draftId));
            ConnectedClients.TryAdd(Context.ConnectionId, user);

            if (draft.Broadcaster.Name.ToLower() == user.ToLower())
                Groups.Add(Context.ConnectionId, String.Format("draft/{0}/broadcaster", draftId));

            var cardIds = draft.CubeDraftCards.Select(c => c.CardId).ToArray();

            var cardObjects = _db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();

            Clients.Caller.addedCards(cardIds.Select(c => cardObjects.Single(co => co.Id == c)).ToArray());

            //For security purposes, I don't want to send MtgoUsernames for signed up players
            foreach ( var player in draft.CubeDraftPlayers.Where(p => !p.Confirmed) )
                Clients.Caller.playerUpdated(new {
                    MTGOUsername = new {
                        player.MTGOUsername.TwitchUsername,
                        MtgoUsername = null as string
                    },
                    player.Confirmed,
                    RequireCollateral = 0
                });

            foreach (var player in draft.CubeDraftPlayers.Where(p => p.Confirmed))
                Clients.Caller.playerUpdated(player);



            return false;
        }

        /// <summary>
        /// Called from the mtgo bot service, when cards are added to the bots collection
        /// </summary>
        /// <param name="draftId"></param>
        /// <param name="cards"></param>
        public void AddedCards(int draftId, int[] cards)
        {
            //Verify local request
            if (!HttpContext.Current.Request.IsLocal)
                return;

            var cardObjects = _db.Cards.Where(c => cards.Contains(c.Id)).ToArray();

            Clients.Caller.addedCards(cards.Select(c => cardObjects.Single(co => co.Id == c)).ToArray());

            Clients.Group(String.Format("draft/{0}/clients", draftId)).addedCards(_db.Cards.Where(c => cards.Contains(c.Id)));
        }

        public bool CancelDraft(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return false;

            //Make sure they are a broadcaster
            //TODO: Is the .ToLower() required?
            if (draft.Broadcaster.Name.ToLower() != user.ToLower())
                return false;

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

        public void SubscribeToDrafting(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            //TODO: This can throw an exception, but too lazy to fix ATM
            DraftManagers[draftId].PlayerSubscribe(user, Context.ConnectionId);
        }

        public object CreateDraft(string name, int roundLimits, bool requireWatchers)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return new { Error = "An unknown error has occurred. Please refresh the page and try again." };

            //Should never happen, but an additional check just in case
            var broadcaster = _db.Broadcasters.FirstOrDefault(b => b.Name == user);

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
                return new { Error = "There was a problem saving this cube draft. If this problem persists, please contact the developer." };
            }

            string username = null;

            
            try
            {
                username = BotService.StartCubeDraft(newCubeDraft.Id);
            }
            catch (Exception)
            {
            }

            if (username == null)
                return new { Error = "Unable to create draft. There is no bot available to support this draft. Please try again later." };

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", newCubeDraft.Id));
            return new {Error = false, Username = username, DraftId = newCubeDraft.Id};
        }

        public bool StartDraft(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;
            
            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return false;

            //Make sure they are a broadcaster
            return draft.Broadcaster.Name.ToLower() == user.ToLower() && DraftManagers.TryAdd(draft.Id, new CubeDraftManager(ref _db, draft));
        }

        public void TakePick(int draftId, int pickNumber, int pickId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            ICubeDraftManager draftManager;

            if (!DraftManagers.TryGetValue(draftId, out draftManager))
                return;

            draftManager.Pick(user, pickNumber, pickId);
        }

        public int Signup(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return 0;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return 0;

            var dbUser = _db.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

            if (dbUser == null)
                return 1;

            if (!dbUser.Confirmed)
                return 2;

            var newPlayer = new CubeDraftPlayer
                {
                    Confirmed = false,
                    ProductGiven = false,
                    UserId = dbUser.Id,
                    Position = 0,
                    RequireCollateral = 0,
                    Team = 0
                };
            try
            {
                draft.CubeDraftPlayers.Add(newPlayer);

                _db.SaveChanges();
            }
            catch (Exception)
            {
                draft.CubeDraftPlayers.Remove(newPlayer);
                return 0;
            }

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(new
                {
                    MTGOUsername = new
                    {
                        newPlayer.MTGOUsername.TwitchUsername,
                        MtgoUsername = null as string
                    },
                    newPlayer.Confirmed,
                    RequireCollateral = 0
                });

            return 3;
        }

        public async Task<object> GetPlayerInfo(int draftId, string username)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null || username == null)
                return null;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null)
                return null;

            //Make sure they are a broadcaster
            if (draft.Broadcaster.Name.ToLower() != user.ToLower())
                return null;

            var task = new LogParser(draft.Broadcaster.Name).FetchInfoAsync(username);

            var player = _db.MTGOUsernames.First(u => u.TwitchUsername == username && u.Confirmed);
            var player2 = draft.CubeDraftPlayers.SingleOrDefault(u => u.UserId == player.Id);

            var info = await task;
            return new
                {
                    player.TwitchUsername,
                    player.MtgoUsername,
                    Approved = player2 != null && player2.Confirmed,
                    RequireCollateral = player2 != null ? player2.RequireCollateral : 0,
                    info.Joins,
                    info.MessageCount,
                    info.Last50Messages
                };
        }

        public void UpdatePlayer(int draftId, string username, bool approved, int collateral = 0)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null || draft.Status != CubeDraftStatus.PreStart)
                return;

            //Make sure they are a broadcaster
            if (draft.Broadcaster.Name.ToLower() != user.ToLower())
                return;

            if (!approved)
                collateral = 0;

            var player = draft.CubeDraftPlayers.First(p => p.MTGOUsername.TwitchUsername == username);

            player.Confirmed = approved;
            player.RequireCollateral = collateral;

            _db.Entry(player).State = EntityState.Modified;

            _db.SaveChanges();

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerUpdated(player);
        }

        public int LinkUsername(string mtgousername)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return 0;

            var dbUser = _db.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

            if (dbUser != null)
                return 0;

            var confirmKey = new Random().Next(1000, 9999);

            if (_db.MTGOUsernames.Any(u => u.MtgoUsername == mtgousername && u.Confirmed))
                return 2;

            var mtgoUser = BotService.SendMessage(mtgousername,
                                                  String.Format(
                                                      @"[sHat] Welcome to the MTGBot Cube Drafting System! Your confirmation code is '{0}'. If you did not initiate this request, please ignore this message and do nothing.",
                                                      confirmKey));
            if ( mtgoUser == null )
                return 1;


            _db.MTGOUsernames.Add(new MTGOUsername
                {
                    ConfirmKey = confirmKey.ToString(CultureInfo.InvariantCulture),
                    Confirmed = false,
                    MtgoUsername = mtgoUser.Username,
                    MtgoId = mtgoUser.PlayerId,
                    TwitchUsername = user
                });

            _db.SaveChanges();

            return 3;
        }

        public int ConfirmUsername(int confirmKey)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return 0;

            //Check cache and throw error if user is trying too much
            var key = "confirmusername-" + user;
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

            var dbUser = _db.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

            if (dbUser == null)
                return 0;

            if (dbUser.Confirmed)
                return 2;

            if (_db.MTGOUsernames.Any(u => u.MtgoUsername == dbUser.MtgoUsername && u.Confirmed))
                return 3;

            if (dbUser.ConfirmKey != confirmKey.ToString(CultureInfo.InvariantCulture))
                return 1;

            dbUser.Confirmed = true;
            dbUser.ConfirmKey = null;
            _db.Entry(dbUser).State = EntityState.Modified;
            _db.SaveChanges();

            return 4;
        }

        public Guid? InitReadyCheck(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return null;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null)
                return null;

            if (user.ToLower() != draft.Broadcaster.Name.ToLower())
                return null;

            var guid = Guid.NewGuid();

            var approvedClientNames =
                draft.CubeDraftPlayers.Where(p => p.Confirmed && p.MTGOUsername.TwitchUsername != user).Select(p => p.MTGOUsername.TwitchUsername).ToArray();

            foreach ( var client in ConnectedClients.Where(c => approvedClientNames.Contains(c.Value)).Select(x => x.Key))
                Clients.Client(client).readyCheck(guid);

            return guid;
        }

        public void ReadyCheck(Guid guid, int draftId, bool ready)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            var draft = _db.CubeDrafts.Find(draftId);

            if (draft == null)
                return;

            Clients.Group(String.Format("draft/{0}/broadcaster", draftId)).readyCheckUpdate(guid, user, ready);
        }
    }
}