using FluentNHibernate.Mapping;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Common.Mappings.CubeDrafting
{
    class CubeDraftCardMap : ClassMap<CubeDraftCard>
    {
        public CubeDraftCardMap()
        {
            Id(x => x.Id);
            Map(x => x.Location);
            HasOne(x => x.Card);
            References(x => x.CubeDraft);
        }
    }
}
