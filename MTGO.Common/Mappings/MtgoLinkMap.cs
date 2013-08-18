using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Mappings
{
    class MtgoLinkMap : ClassMap<MtgoLink>
    {
        public MtgoLinkMap()
        {
            Id(x => x.Id);
            Map(x => x.Confirmed);
            Map(x => x.ConfirmKey)
                .Nullable();

            References(x => x.User);
            References(x => x.Player);
        }
    }
}
