using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class VoteMap : ClassMap<Vote>
    {
        public VoteMap()
        {
            Id(x => x.Id);

            References(x => x.User);
            References(x => x.DraftPick);
            References(x => x.Card);
        }
    }
}
