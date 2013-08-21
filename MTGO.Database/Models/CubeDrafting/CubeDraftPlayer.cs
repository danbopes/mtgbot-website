using System.Collections.Generic;

namespace MTGO.Database.Models.CubeDrafting
{
    public class CubeDraftPlayer
    {
        public CubeDraftPlayer()
        {
            CubeDraftPicks = new HashSet<CubeDraftPick>();
        }
    
        public virtual int Id { get; set; }
        public virtual bool Confirmed { get; set; }
        public virtual int Team { get; set; }
        public virtual int RequireCollateral { get; set; }
        public virtual bool ProductGiven { get; set; }
        public virtual int Position { get; set; }
        public virtual int DropRound { get; set; }
        public virtual bool DeckBuilt { get; set; }

        public virtual CubeDraft CubeDraft { get; set; }
        public virtual MtgoLink MtgoLink { get; set; }
        public virtual ICollection<CubeDraftPick> CubeDraftPicks { get; set; }
    }
}
