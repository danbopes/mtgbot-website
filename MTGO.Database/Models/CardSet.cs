using System.Collections.Generic;

namespace MTGO.Database.Models
{
    public class CardSet
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Short { get; set; }
        public virtual string GathererSet { get; set; }
        public virtual string MagicCardsInfoSet { get; set; }
        public virtual int MtgoBoosterId { get; set; }
        public virtual bool Completed { get; set; }

        public virtual IList<Card> Cards { get; protected set; }
    }
}
