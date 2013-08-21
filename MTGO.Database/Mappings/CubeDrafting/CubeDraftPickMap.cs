using FluentNHibernate.Mapping;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Database.Mappings.CubeDrafting
{
    public class CubeDraftPickMap : ClassMap<CubeDraftPick>
    {
        public CubeDraftPickMap()
        {
            Id(x => x.Id);
            Map(x => x.PickNumber);
            References(x => x.Pick)
                .Nullable();
            References(x => x.CubeDraft);
            References(x => x.Player);
            HasManyToMany(x => x.Picks)
                .Cascade.All()
                .Table("CubeDraftPicksLink");
        }
    }
}
