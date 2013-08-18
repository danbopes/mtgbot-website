using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Mappings.Mtgo
{
    class MtgoDraftMap : ClassMap<MtgoDraft>
    {
        public MtgoDraftMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();
            Map(x => x.Token);
        }
    }
}
