using FluentNHibernate.Mapping;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Database.Mappings.CubeDrafting
{
    public class CubeDraftCardMap : ClassMap<CubeDraftCard>
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
