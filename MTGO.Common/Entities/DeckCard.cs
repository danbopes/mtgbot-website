using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGO.Common.Entities
{
    public class DeckCard
    {
        public virtual int Id { get; protected set; }
        public virtual Card Card { get; set; }
        public virtual int Quantity { get; set; }
    }
}
