using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class BanMap : ClassMap<Ban>
    {
        public BanMap()
        {
            Id(x => x.Id);
            Map(x => x.IpAddress);
            Map(x => x.BanType);
            Map(x => x.Expires);
            Map(x => x.Reason);
            References(x => x.User);
        }
    }
}
