using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class DraftPickMap : ClassMap<DraftPick>
    {
        public DraftPickMap()
        {
            Id(x => x.Id);
            Map(x => x.Pick);
            Map(x => x.Pack);
            Map(x => x.Direction).CustomType<Direction>();
            Map(x => x.Time);

            References(x => x.Draft);
            References(x => x.PickCard)
                .Nullable();

            HasManyToMany(x => x.Picks)
                .Cascade.Delete();
        }
    }
}
