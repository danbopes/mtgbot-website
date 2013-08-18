using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Mappings.Mtgo
{
    public class MtgoPlayerMap : ClassMap<MtgoPlayer>
    {
        public MtgoPlayerMap()
        {
            Id(x => x.Id)
                .GeneratedBy.Assigned();

            Map(x => x.Username);
        }
    }
}
