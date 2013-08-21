using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class DonationMap : ClassMap<Donation>
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
