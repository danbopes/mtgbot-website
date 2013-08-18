using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class DraftPickMap : ClassMap<DraftPick>
    {
        public DraftPickMap()
        {
            Id(x => x.Id);
            Map(x => x.Pick);
            Map(x => x.Pack);
            Map(x => x.Direction);
            Map(x => x.Time);

            References(x => x.Draft);
            References(x => x.PickCard)
                .Nullable();

            HasManyToMany(x => x.Picks)
                .Cascade.Delete();
        }
    }
}
