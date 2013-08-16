using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using MTGBotWebsite.Helpers;
using MTGBotWebsite.Infastructure;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.CSharp.RuntimeBinder;

namespace MTGBotWebsite.Hubs
{
    public class MtgHub : Hub
    {
        private readonly MainDbContext _db = new MainDbContext();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ProxyMessage(dynamic message)
        {
            try
            {
                if (!HttpContext.Current.Request.IsLocal)
                    return;

                var broadcasterName = (string)message.BroadcasterName;

                int mtgoDraftId = 0;
                try
                {
                    mtgoDraftId = (int) message.DraftId;
                }
                catch (RuntimeBinderException)
                {
                }

                int mtgoTournamentId = 0;
                try
                {
                    mtgoTournamentId = (int)message.TournamentId;
                }
                catch (RuntimeBinderException)
                {
                }


                Draft draft = null;

                if (string.IsNullOrEmpty(broadcasterName))
                {
                    throw new Exception("Broadcaster Name Required");
                }

                var broadcaster = _db.Broadcasters.SingleOrDefault(b => b.MtgoUsername == broadcasterName);

                if (broadcaster == null)
                {
                    throw new Exception("Invalid Broadcaster.");
                }

                dynamic clients = null;

                if (message.Action != "start_draft")
                {
                    if (mtgoDraftId > 0)
                        draft = _db.Drafts.SingleOrDefault(d => d.DraftId == mtgoDraftId && d.BroadcasterId == broadcaster.Id);
                    else if (mtgoTournamentId > 0)
                        draft = _db.Drafts.SingleOrDefault(d => d.TournamentId == mtgoTournamentId && d.BroadcasterId == broadcaster.Id);

                    if (draft == null)
                        throw new Exception(String.Format("Unable to find draft with draft_id='{0}'", mtgoDraftId));

                    clients = Clients.Group(String.Format("draft/{0}",
                        draft.Id
                    ));
                }

                log.DebugFormat("Message from mtgo client broadcaster='{0}', mtgoDraftId='{1}', mtgoTournamentId='{2}', action='{3}'", broadcaster.Name, mtgoDraftId, mtgoTournamentId, message.Action);

                switch ((string)message.Action)
                {
                    case "start_draft":
                        var players = ((Newtonsoft.Json.Linq.JArray)message.Players).ToObject<int[]>();
                        draft = new Draft
                        {
                            DraftId = mtgoDraftId,
                            TournamentId = mtgoTournamentId,
                            Players = string.Join(",", players),
                            DraftStatus = DraftStatus.Drafting,
                            BroadcasterId = broadcaster.Id
                        };
                        _db.Drafts.Add(draft);
                        _db.SaveChanges();
                        {
                            Pipe.SendMessage("{0}|StartDraft|{1}",
                                                          broadcaster.Name,
                                                          draft.Id);
                        }
                        //Clients.Caller.startDraft(draft.Id);
                        break;
                    case "open_pack":
                        draft.CurrentPack++;
                        _db.Entry(draft).State = EntityState.Modified;
                        _db.SaveChanges();
                        break;
                    case "pending_selection":
                        {
                            var dir = (Direction)message.Direction;
                            var time = (int)message.Time;
                            var picks = ((Newtonsoft.Json.Linq.JArray)message.Picks).ToObject<int[]>();
                            var currentPick = 16 - picks.Length;

                            int playerCount = draft.Players.Split(',').Length;
                            int i = playerCount;
                            List<object> previousPicks = new List<object>();

                            while (currentPick - i > 0)
                            {
                                var previousPick =
                                    _db.DraftPicks.SingleOrDefault(
                                        dp =>
                                        dp.DraftId == draft.Id && dp.Pack == draft.CurrentPack &&
                                        dp.Pick == currentPick - i);

                                if (previousPick != null)
                                {
                                    var previousPickCards = previousPick.Picks.Split(',').Select(p => Convert.ToInt32(p));
                                    previousPicks.Add(new
                                        {
                                            PickId = previousPick.PickId,
                                            Cards = _db.Cards.Include("CardSet").Where(c => previousPickCards.Contains(c.Id)).ToArray()
                                        });
                                }
                                i += playerCount;
                            }

                            var draftPick = new DraftPick
                            {
                                DraftId = draft.Id,
                                PickId = null,
                                Picks = string.Join(",", picks),
                                Direction = dir,
                                Pick = currentPick,
                                Pack = draft.CurrentPack,
                                Time = time
                            };
                            _db.DraftPicks.Add(draftPick);

                            log.DebugFormat("Writing draft pick: {0}", draftPick);

                            var cards = _db.Cards.Include("CardSet").Where(c => picks.Contains(c.Id)).ToArray();

                            _db.SaveChanges();

                            log.DebugFormat("Calling pendingSelection: {0}, {1}, {2}, {3}, {4}, {5}", draftPick.Id, previousPicks, cards, draft.CurrentPack, dir, time);

                            clients.pendingSelection(
                                draftPick.Id, previousPicks, cards, draft.CurrentPack, dir, time);
                        }
                        break;
                    case "draft_selection":
                        {
                            var draftPick = _db.DraftPicks.OrderByDescending(d => d.Id)
                              .SingleOrDefault(d => d.DraftId == draft.Id && d.PickId == null);
                            var pick = (int)message.Pick;

                            if (draftPick != null)
                            {
                                draftPick.PickId = pick;
                                _db.Entry(draftPick).State = EntityState.Modified;
                                _db.SaveChanges();
                            }
                            clients.draftSelection(_db.Cards.Include("CardSet").Single(p => p.Id == pick), draft.CurrentPack);
                        }
                        break;
                    case "draft_ended":
                        {
                            draft.DraftStatus = DraftStatus.Completed;
                            _db.Entry(draft).State = EntityState.Modified;
                            _db.SaveChanges();
                            Pipe.SendMessage("{0}|EndDraft|{1}",
                                                          broadcaster.Name,
                                                          draft.Id);
                        }
                        break;
                    case "submit_deck":
                        {
                            var cards = ((Newtonsoft.Json.Linq.JArray)message.Cards).ToObject<string[]>();
                            draft.FinalDeck = string.Join(",", cards);
                            _db.Entry(draft).State = EntityState.Modified;
                            _db.SaveChanges();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error on ProxyMessage: ", ex);
            }
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void SubscribeToDraft(int draftId)
        {
            var draft = _db.Drafts
                .Include("DraftPicks")
                .Include("Broadcaster")
                .Single(d => d.Id == draftId);

            var user = _db.Users.Find(Context.User.GetUserId());

            int i = 0;

            foreach (var pick in draft.DraftPicks.Where(p => p.PickId != null).OrderBy(p2 => p2.Id))
            {
                i++;

                Clients.Caller.draftSelection(_db.Cards.Include("CardSet").Single(c => c.Id == pick.PickId), Math.Ceiling((double) i/15));
            }

            var lastPick = draft.DraftPicks.OrderByDescending(p => p.Id).FirstOrDefault();

            if (lastPick != null)
            {
                var picks = lastPick.Picks.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                var cards = _db.Cards.Include("CardSet").Where(c => picks.Contains(c.Id)).ToList();
                var currentPick = 16 - picks.Length;

                int playerCount = draft.Players.Split(',').Length;
                i = playerCount;
                List<object> previousPicks = new List<object>();

                while (currentPick - i > 0)
                {
                    var previousPick =
                        _db.DraftPicks.SingleOrDefault(
                            dp =>
                            dp.DraftId == draftId && dp.Pack == draft.CurrentPack &&
                            dp.Pick == currentPick - i);
                    if (previousPick != null)
                    {
                        var previousPickCards = previousPick.Picks.Split(',').Select(p => Convert.ToInt32(p));
                        previousPicks.Add(new
                            {
                                PickId = previousPick.PickId,
                                Cards = _db.Cards.Include("CardSet").Where(c => previousPickCards.Contains(c.Id))
                            });
                    }
                    i += playerCount;
                }


                Clients.Caller.pendingSelection(
                    lastPick.Id, previousPicks, cards, draft.CurrentPack, (int) lastPick.Direction, lastPick.Time);
            }

            Groups.Add(Context.ConnectionId, string.Format("draft/{0}", draftId));

            if (user.TwitchUsername.ToLower() != draft.Broadcaster.Name.ToLower())
                return;

            Groups.Add(Context.ConnectionId, string.Format("broadcaster/{0}", draftId));

            if (lastPick == null) return;

            var votes = _db.Votes.Where(v => v.DraftPickId == lastPick.Id).ToList();
            foreach (var vote in votes)
            {
                Clients.Caller.addVote(vote.DraftPickId, null, vote.Card, vote.Username);
            }

            if (draft.DraftStatus != DraftStatus.Drafting)
            {
                Clients.Caller.draftCompleted();
            }
        }

        [AuthorizeClaim(MtgbotClaimTypes.Identifier)]
        public void SubmitVote(int draftId, int pickId, int cardId)
        {
            var draft = _db.Drafts.Find(draftId);

            if (draft == null) return;

            var user = _db.Users.Find(Context.User.GetUserId());

            var pick = _db.DraftPicks.Find(pickId);

            if (pick == null) return;

            if (!pick.Picks.Split(',').Select(x => Convert.ToInt32(x)).Contains(cardId))
                return;

            //TODO: Change this to userId
            var vote =
                _db.Votes.SingleOrDefault(
                    v => v.DraftPickId == pickId && v.Username == user.TwitchUsername);

            Card previousCard = null;
            if (vote != null)
            {
                previousCard = _db.Cards.Find(vote.CardId);
                vote.CardId = cardId;
                _db.Entry(vote).State = EntityState.Modified;
            }
            else
            {
                _db.Votes.Add(new Vote
                {
                    DraftPickId = pickId,
                    CardId = cardId,
                    Username = user.TwitchUsername
                });
            }

            _db.SaveChanges();

            Clients.Group(string.Format("broadcaster/{0}", draftId))
                   .addVote(pickId, previousCard, _db.Cards.Include("CardSet").Single(c => c.Id == cardId), user.TwitchUsername);


            //For broadcasters, submit the pick
            //if (username.ToLower() != draft.Broadcaster.Name.ToLower())
            //    return;

            //Sending the draft.DraftId (The mtgo client draftId)
            //Clients.Group("broadcasterclient/" + draft.Broadcaster.Id).takePick(cardId, draft.DraftId);
        }
    }
}