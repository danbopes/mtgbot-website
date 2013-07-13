using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using MTGBotWebsite.CubeDrafts;
using MTGBotWebsite.Helpers;
using MTGOLibrary;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.Hubs
{
    public class CubeHub : Hub
    {
        private static ICubeDraftManager _draftManager;
        private readonly MainDbContext DB = new MainDbContext();

        //private static IBotService 

        private class CacheCheck
        {
            public DateTime Last = DateTime.Now;
            public int Count = 1;
        }

        public bool SubscribeToDraft(int draftId)
        {
            string user = (string) HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", draftId));

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return false;

            var cubeDraftCards = draft.CubeDraftCards.Select(cdc => cdc.CardId).ToArray();
            Clients.Caller.addedCards(DB.Cards.Where(c => cubeDraftCards.Contains(c.Id)).ToArray());

            foreach ( var player in draft.CubeDraftPlayers.Where(p => !p.Confirmed) )
                Clients.Caller.playerSignup(player.MTGOUsername.TwitchUsername);

            foreach (var player in draft.CubeDraftPlayers.Where(p => p.Confirmed))
                Clients.Caller.playerApproved(player);

            return false;
        }

        /// <summary>
        /// Called from the mtgo bot service, when cards are added to the bots collection
        /// </summary>
        /// <param name="draftId"></param>
        /// <param name="cards"></param>
        public void AddedCards(int draftId, int[] cards)
        {
            Clients.Group(String.Format("draft/{0}/clients", draftId)).addedCards(DB.Cards.Where(c => cards.Contains(c.Id)));
        }

        public void SubscribeToDrafting(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            var player = DB.CubeDraftPlayers.FirstOrDefault(p => p.MTGOUsername.TwitchUsername == user && p.CubedraftId == draftId && p.Confirmed);

            if (player == null)
                return;

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/players/{1}", draftId, player.Id));

            var drafter = _draftManager.Players.FirstOrDefault(p => p.PlayerId == player.Id);

            if (drafter == null) return;

            drafter.NotifyPick();
        }

        public object CreateDraft(string name, int roundLimits, bool requireWatchers)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return new { Error = "An unknown error has occurred. Please refresh the page and try again." };

            //Should never happen, but an additional check just in case
            var broadcaster = DB.Broadcasters.FirstOrDefault(b => b.Name == user);

            if (broadcaster == null)
                throw new Exception("Sorry, this feature is only for broadcasters on twitch.tv (With a valid mtgbot.tv client application).");

            var newCubeDraft = new CubeDraft
                {
                    BroadcasterId = broadcaster.Id,
                    Name = name,
                    RoundTime = roundLimits,
                    RequireWatchers = requireWatchers
                };
            try
            {
                DB.CubeDrafts.Add(newCubeDraft);

                DB.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                return new { Error = "There was a problem saving this cube draft. If this problem persists, please contact the developer." };
            }

            string username = null;

            var _botService = new ChannelFactory<IBotService>(
                    new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/MTGOBotService")
                ).CreateChannel();
            try
            {
                username = _botService.StartCubeDraft(newCubeDraft.Id);
            }
            catch (Exception)
            {
            }

            if (username == null)
                return new { Error = "Unable to create draft. There is no bot available to support this draft. Please try again later." };

            Groups.Add(Context.ConnectionId, String.Format("draft/{0}/clients", newCubeDraft.Id));
            return new {Error = false, Username = username};
        }

        public bool StartDraft(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return false;

            return false;
        }

        public int Signup(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return 0;

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return 0;

            var dbUser = DB.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

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

                DB.SaveChanges();
            }
            catch (Exception)
            {
                draft.CubeDraftPlayers.Remove(newPlayer);
                return 0;
            }

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerSignup(user);

            return 3;
        }

        public async Task<object> GetPlayerInfo(int draftId, string username)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return null;

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return null;

            //Make sure they are a broadcaster
            if (draft.Broadcaster.Name.ToLower() != user.ToLower())
                return null;

            var task = new LogParser(draft.Broadcaster.Name).FetchInfoAsync(username);

            var player = DB.MTGOUsernames.First(u => u.TwitchUsername == username && u.Confirmed);

            var info = await task;
            return new
                {
                    player.TwitchUsername,
                    player.MtgoUsername,
                    info.Joins,
                    info.MessageCount,
                    info.Last50Messages
                };
        }

        public void ApprovePlayer(int draftId, string username)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return;

            //Make sure they are a broadcaster
            if (draft.Broadcaster.Name.ToLower() != user.ToLower())
                return;

            var player = draft.CubeDraftPlayers.First(p => p.MTGOUsername.TwitchUsername == username && !p.Confirmed);

            player.Confirmed = true;

            DB.Entry(player).State = EntityState.Modified;

            DB.SaveChanges();

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerApproved(player);
        }

        public int LinkUsername(string mtgousername)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return 0;

            var dbUser = DB.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

            if (dbUser != null)
                return 0;

            var confirmKey = new Random().Next(1000, 9999);

            #if !debug
            var botService = new ChannelFactory<IBotService>(
                    new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/MTGOBotService")
                ).CreateChannel();

            if (!botService.SendMessage(mtgousername,
                                        String.Format(
                                            @"Welcome to the MTGBot Cube Drafting System! Your confirm code is '{0}'. If you did not initiate this request, please ignore this message and do nothing.",
                                            confirmKey)))
                return 1;
            #endif

            if (DB.MTGOUsernames.Any(u => u.MtgoUsername == mtgousername && u.Confirmed))
                return 2;

            DB.MTGOUsernames.Add(new MTGOUsername
                {
                    ConfirmKey = confirmKey.ToString(CultureInfo.InvariantCulture),
                    Confirmed = false,
                    MtgoUsername = mtgousername,
                    TwitchUsername = user
                });

            DB.SaveChanges();

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

            var dbUser = DB.MTGOUsernames.FirstOrDefault(c => c.TwitchUsername == user);

            if (dbUser == null)
                return 0;

            if (dbUser.Confirmed)
                return 2;

            if (DB.MTGOUsernames.Any(u => u.MtgoUsername == dbUser.MtgoUsername && u.Confirmed))
                return 3;

            if (dbUser.ConfirmKey != confirmKey.ToString(CultureInfo.InvariantCulture))
                return 1;

            dbUser.Confirmed = true;
            dbUser.ConfirmKey = null;
            DB.Entry(dbUser).State = EntityState.Modified;
            DB.SaveChanges();

            return 4;
        }

        public Guid? InitReadyCheck(int draftId)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return null;

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return null;

            if (user.ToLower() != draft.Broadcaster.Name.ToLower())
                return null;

            var guid = Guid.NewGuid();
            Clients.OthersInGroup(String.Format("draft/{0}/approved_clients", draftId)).readyCheck(guid);

            return guid;
        }

        public void ReadyCheck(Guid guid, int draftId, bool ready)
        {
            string user = (string)HttpContext.Current.Session["user_name"];

            if (user == null)
                return;

            var draft = DB.CubeDrafts.Find(draftId);

            if (draft == null)
                return;

            Clients.Group(String.Format("draft/{0}/broadcaster", draftId)).readyCheckUpdate(guid, user, ready);
        }
    }
}