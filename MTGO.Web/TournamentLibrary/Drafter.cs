using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MTGO.Web.Hubs;

namespace MTGO.Web.TournamentLibrary
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

        //UserId int the Database
        public readonly int UserId;

        //PlayerName (Twitch username)
        public readonly string PlayerName;

        //Position in the Draft
        public readonly int Position;

        //SignalR client Ids
        public HashSet<string> ClientIds = new HashSet<string>();

        //Draft Picks
        public List<Card> CurrentPicks;

        //Packs (to open)
        public readonly DraftCollection Packs;

        //Queued Packs
        public readonly Queue<List<Card>> QueuedPicks = new Queue<List<Card>>(15);

        public readonly List<Card> Picks = new List<Card>(); 

        //CurrentPick
        public int CurrentPick = 1;

        //PickExpires
        public DateTime CurrentPickExpires = DateTime.MaxValue;

        private readonly MainDbContext _db = new MainDbContext();
        private static readonly IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<CubeHub>();

        private Object _lockObject = new Object();
        private Object _notifyLockObject = new Object();
        private bool _isTimed;

        public override string ToString()
        {
            return PlayerName;
        }

        public Drafter(CubeDraft draft, CubeDraftPlayer player, int position, DraftCollection draftCollection)
        {
            QueuedPicks.Enqueue(draftCollection.GetNextPack().Cards.ToList());
            DraftId = draft.Id;
            PlayerId = player.Id;
            UserId = player.MtgoLink.UserId;
            PlayerName = player.MtgoLink.User.TwitchUsername;
            Position = position;
            Packs = draftCollection;
            _isTimed = draft.Timed;
        }

        public Drafter(CubeDraft draft, CubeDraftPlayer player, int position, DraftCollection draftCollection, List<Card> picks)
        {
            if ( picks == null )
                picks = new List<Card>();

            DraftId = draft.Id;
            PlayerId = player.Id;
            UserId = player.MtgoLink.UserId;
            PlayerName = player.MtgoLink.User.TwitchUsername;
            Position = position;
            _isTimed = draft.Timed;
            Packs = draftCollection;
            if ( picks.Count == 0 )
                QueuedPicks.Enqueue(draftCollection.GetNextPack().Cards.ToList());

            Picks = picks;
            CurrentPick = picks.Count + 1;
        }

        public void OpenNextPack()
        {
            lock (_lockObject)
            {
                if (CurrentPicks != null)
                {
                    Console.WriteLine("Current Picks Count: " + CurrentPicks.Count + " (" + String.Join(", ", CurrentPicks) + ")");
                    throw new InvalidOperationException("Cannot open next pack as invalid number of cards queued.");
                }

                var pack = Packs.GetNextPack();

                if (pack == null)
                    throw new InvalidOperationException("No packs to open");

                QueuedPicks.Enqueue(pack.Cards.ToList());
                NotifyPick();
            }
        }

        //When recovered, needs to push the currentpicks without inserting a new entry
        public void FixQueuedPicks()
        {
            CurrentPicks = QueuedPicks.Dequeue();
        }

        public void NotifyPick(bool force = false)
        {
            lock (_notifyLockObject)
            {
                //Already have a current pack?
                if (CurrentPicks != null)
                {
                    if (!force) return;

                    var timer = _isTimed ? (CurrentPickExpires-DateTime.UtcNow).TotalSeconds : -1;
                    foreach (var clientId in ClientIds)
                        HubContext.Clients.Client(clientId).pendingSelection(CurrentPick, CurrentPicks, timer);

                    return;
                }

                try
                {
                    CurrentPicks = QueuedPicks.Dequeue();

                    var newDraftPick = new CubeDraftPick
                        {
                            CubedraftId = DraftId,
                            Pick = CurrentPick,
                            PickId = null,
                            Picks = String.Join(",", CurrentPicks.Select(c => c.Id)),
                            PlayerId = PlayerId
                        };

                    _db.CubeDraftPicks.Add(newDraftPick);
                    _db.SaveChanges();

                    var currentPick = CurrentPick;
                    var timer = _isTimed ? CurrentPicks.Count * 5 : -1;
                    CurrentPickExpires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(CurrentPicks.Count * 5));

                    foreach (var clientId in ClientIds)
                        HubContext.Clients.Client(clientId).pendingSelection(CurrentPick, CurrentPicks, timer);

                    if (timer > -1)
                    {
                        //TODO: Most likely very inefficient to keep all these threads going. Let's find a cleaner way
                        Task.Delay(timer * 1000 + 3000).ContinueWith(res =>
                        {
                            if (currentPick == CurrentPick)
                                TakePick(currentPick, CurrentPicks.PickRandom().Id);
                        });
                    }
                }
                //Queue is empty
                catch (InvalidOperationException)
                {
                }
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
                if (pick == null || !CurrentPicks.Remove(pick))
                    throw new InvalidOperationException(String.Format("Invalid Pick Id. Pickid '{0}' was not found in current pack", pickId));

                //Increment the current pick #
                CurrentPick++;
                Picks.Add(pick);

                //Update the database
                var draftPick = _db.CubeDraftPicks.Single(c => c.CubedraftId == DraftId && c.PlayerId == PlayerId && c.Pick == CurrentPick-1);

                draftPick.PickId = pickId;
                _db.Entry(draftPick).State = EntityState.Modified;
                _db.SaveChanges();

                foreach (var clientId in ClientIds)
                    HubContext.Clients.Client(clientId).draftSelection(CurrentPick, pick);

                var ret = CurrentPicks;
                CurrentPicks = null;

                //Check to see if there is a pack in the queue waiting to be sent
                NotifyPick();

                return ret.ToArray();
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