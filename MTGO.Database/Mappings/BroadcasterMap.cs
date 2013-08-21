using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class BroadcasterMap : ClassMap<Broadcaster>
    {
        public BroadcasterMap()
        {
            Id(x => x.Id);
            Map(x => x.IrcSettings).Length(16777215);
            HasOne(x => x.User)
                .Cascade.All();
        }
    }
}
