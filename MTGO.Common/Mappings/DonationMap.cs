using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class DonationMap : ClassMap<Donation>
    {
        public DonationMap()
        {
            Id(x => x.Id);
            Map(x => x.TxnId);
            Map(x => x.Email);
            Map(x => x.Username);
            Map(x => x.Amount);
            Map(x => x.DateTime);
        }
    }
}
