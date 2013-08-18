using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class CardSetMap : ClassMap<CardSet>
    {
        public CardSetMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();

            Map(x => x.Name);
            Map(x => x.Short);
            Map(x => x.GathererSet);
            Map(x => x.MagicCardsInfoSet);
            Map(x => x.MtgoBoosterId);
            Map(x => x.Completed);

            HasMany(x => x.Cards);
        }
    }
}
