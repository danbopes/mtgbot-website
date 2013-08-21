using System.Collections.Generic;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Models
{
    public class Draft
    {
        public Draft()
        {
            CurrentPack = 0;
            DraftPicks = new List<DraftPick>();
            Deck = new List<DeckCard>();
        }
    
        public virtual int Id { get; protected set; }
        public virtual DraftStatus DraftStatus { get; set; }
        public virtual int CurrentPack { get; set; }

        public virtual MtgoDraft MtgoDraft { get; set; }
        public virtual Broadcaster Broadcaster { get; set; }
        public virtual IList<DraftPick> DraftPicks { get; protected set; }
        public virtual IList<DeckCard> Deck { get; protected set; }
    }
}
