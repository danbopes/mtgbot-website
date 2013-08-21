using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id);
            Map(x => x.Admin);
            Map(x => x.Username);
            Map(x => x.SignupIpAddress);
            Map(x => x.Created);
            HasOne(x => x.Broadcaster);
            HasMany(x => x.Bans)
                .Inverse()
                .Cascade.All();
            HasMany(x => x.MtgoLinks)
                .Inverse()
                .Cascade.All();
        }
    }
}
