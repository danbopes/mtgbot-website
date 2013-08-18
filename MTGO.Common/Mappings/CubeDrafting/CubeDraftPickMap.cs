using FluentNHibernate.Mapping;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Common.Mappings.CubeDrafting
{
    class CubeDraftPickMap : ClassMap<CubeDraftPick>
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
