using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class BroadcasterMap : ClassMap<Broadcaster>
    {
        public BroadcasterMap()
        {
            Id(x => x.Id);
            Map(x => x.IrcSettings);
            HasOne(x => x.User)
                .Cascade.All();
        }
    }
}
