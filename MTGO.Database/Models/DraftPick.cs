using System.Collections.Generic;

namespace MTGO.Database.Models
{
    public class DraftPick
    {
        public virtual int Id { get; protected set; }
        public virtual int DraftId { get; set; }
        public virtual int Pick { get; set; }
        public virtual int Pack { get; set; }
        public virtual Direction Direction { get; set; }
        public virtual int Time { get; set; }
        public virtual Card PickCard { get; set; }

        public virtual Draft Draft { get; protected set; }
        public virtual IList<Card> Picks { get; protected set; }
    }
}
