using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MTGBotWebsite.Hubs;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite.CubeDrafts
{
    public class Drafter : IEquatable<Drafter>
    {
        public bool Equals(Drafter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PlayerId == other.PlayerId;
        }

        public override int GetHashCode()
        {
            return PlayerId;
        }

        //DraftId
        public int DraftId;

        //PlayerId in the Database
        public readonly int PlayerId;

        //PlayerName (Twitch username)
        public readonly string PlayerName;

        //Position in the Draft
        public readonly int Position;

        //SignalR client Id
        //public int ClientId;

        //Draft Picks
        public Card[] CurrentPicks;

        //Packs (to open)
        public readonly Pack[] Packs;

        //Queued Packs
        public readonly Queue<Card[]> QueuedPicks = new Queue<Card[]>(15);

        public readonly List<Card> Picks = new List<Card>(); 

        //CurrentPick
        public int CurrentPick = 1;

        private static readonly MainDbContext DB = new MainDbContext();
        private Object _lockObject = new Object();

        public Drafter(CubeDraft draft, CubeDraftPlayer player, int position, DraftCollection draftCollection)
        {
            DraftId = draft.Id;
            PlayerId = player.Id;
            PlayerName = player.MTGOUsername.TwitchUsername;
            Position = position;
            Packs = draftCollection.ToArray();
            QueuedPicks.Enqueue(Packs.Single(p => p.Number == 1).Cards);
        }

        public void OpenNextPack()
        {
            if (CurrentPicks.Length > 0)
                throw new InvalidOperationException("Cannot open next pack as invalid number of cards queued.");

            switch (CurrentPick)
            {
                case 16:
                    QueuedPicks.Enqueue(Packs.Single(p => p.Number == 2).Cards);
                    NotifyPick();
                    break;
                case 31:
                    QueuedPicks.Enqueue(Packs.Single(p => p.Number == 3).Cards);
                    NotifyPick();
                    break;
                default:
                    throw new InvalidOperationException("Cannot open next pack as invalid pick #.");
            }
        }

        public void NotifyPick()
        {
            //Already have a current pack?
            if (CurrentPicks != null)
                return;

            try
            {
                CurrentPicks = QueuedPicks.Dequeue();

                DB.CubeDraftPicks.Add(new CubeDraftPick
                    {
                        CubedraftId = DraftId,
                        Pick = CurrentPick,
                        PickId = null,
                        Picks = String.Join(",", CurrentPicks.Select(c => c.Id)),
                        PlayerId = PlayerId
                    });
                DB.SaveChanges();

                var hubContext = GlobalHost.ConnectionManager.GetHubContext<CubeHub>();

                if (hubContext == null)
                    return;

                //TODO: Add timer functionality

                hubContext.Clients.Group(String.Format("draft/{0}/players/{1}", DraftId, PlayerId)).PendingSelection(
                   CurrentPicks, -1
                );
            }
            //Queue is empty
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Selects a card from this drafter
        /// </summary>
        /// <param name="pickNumber"></param>
        /// <param name="pickId"></param>
        /// <returns>The picks</returns>
        public Card[] TakePick(int pickNumber, int pickId)
        {
            lock (_lockObject)
            {
                if ( pickNumber != CurrentPick )
                    throw new InvalidOperationException(String.Format("Invalid Pick Number. Expected '{0}' got '{1}'", CurrentPick, pickNumber));

                var pick = CurrentPicks.FirstOrDefault(p => p.Id == pickId);
                if (pick == null || !CurrentPicks.ToList().Remove(pick))
                    throw new InvalidOperationException(String.Format("Invalid Pick Id. Pickid '{0}' was not found in current pack", pickId));

                //Increment the current pick #
                CurrentPick++;
                Picks.Add(pick);

                //Update the database
                var draftPick = DB.CubeDraftPicks.Single(c => c.PlayerId == PlayerId && c.Pick == CurrentPick-1);

                draftPick.PickId = pickId;
                DB.Entry(draftPick).State = EntityState.Modified;
                DB.SaveChanges();

                //Check to see if there is a pack in the queue waiting to be sent
                NotifyPick();

                var ret = CurrentPicks;
                CurrentPicks = null;

                return ret;
            }
        }

        /*public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Drafter) obj);
        }*/
    }
}