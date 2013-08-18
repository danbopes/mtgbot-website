using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class DeckCardMap : ClassMap<DeckCard>
    {
        public DeckCardMap()
        {
            Id(x => x.Id);
            Map(x => x.Quantity);

            References(x => x.Card);
        }
    }
}
