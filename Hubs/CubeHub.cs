using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using MTGBotWebsite.CubeDrafts;
using MTGOLibrary;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.Hubs
{
    public class CubeHub : Hub
    {
        private static ICubeDraftManager _draftManager;
        private static MainDbContext DB = new MainDbContext();

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

            foreach ( var player in draft.CubeDraftPlayers.Where(p => !p.Confirmed) )
                Clients.Caller.playerSignup(player.MTGOUsername.TwitchUsername);

            foreach (var player in draft.CubeDraftPlayers.Where(p => p.Confirmed))
                Clients.Caller.playerApproved(player.MTGOUsername.TwitchUsername);

            return false;
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

            try
            {
                draft.CubeDraftPlayers.Add(new CubeDraftPlayer
                {
                    Confirmed = false,
                    ProductGiven = false,
                    UserId = dbUser.Id,
                    Position = 0,
                    RequireCollateral = 0,
                    Team = 0
                });

                DB.SaveChanges();
            }
            catch (Exception)
            {
                return 0;
            }

            Clients.Group(String.Format("draft/{0}/clients", draftId)).playerSignup(user);

            return 3;
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