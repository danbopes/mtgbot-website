using System.Collections.Generic;

namespace MTGO.Database.Models
{
    public class NSet
    {
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string CodeMagiccards { get; set; }
        public virtual string Date { get; set; }
        public virtual string IsPromo { get; set; }
        public virtual IList<NCard> NCards { get; protected set; }

        public NSet()
        {
            NCards = new List<NCard>();
        }
    }
}
