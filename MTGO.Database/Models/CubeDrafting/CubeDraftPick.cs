using System.Collections.Generic;

namespace MTGO.Database.Models.CubeDrafting
{
    public class CubeDraftPick
    {
        public virtual int Id { get; protected set; }
        public virtual int PickNumber { get; set; }
        public virtual CubeDraft CubeDraft { get; set; }
        public virtual CubeDraftPlayer Player { get; set; }
        public virtual Card Pick { get; set; }

        public virtual IList<Card> Picks { get; protected set; }

        public CubeDraftPick()
        {
            Picks = new List<Card>();
        }
    }
}
