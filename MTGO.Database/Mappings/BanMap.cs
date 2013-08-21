using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class BanMap : ClassMap<Ban>
    {
        public BanMap()
        {
            Id(x => x.Id);
            Map(x => x.IpAddress);
            Map(x => x.BanType).CustomType<BanType>();
            Map(x => x.Expires);
            Map(x => x.Reason);
            References(x => x.User);
        }
    }
}
