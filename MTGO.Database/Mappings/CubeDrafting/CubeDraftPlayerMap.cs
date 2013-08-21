using FluentNHibernate.Mapping;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Database.Mappings.CubeDrafting
{
    public class CubeDraftPlayerMap : ClassMap<CubeDraftPlayer>
    {
        public CubeDraftPlayerMap()
        {
            Id(x => x.Id);
            Map(x => x.Confirmed);
            Map(x => x.Team);
            Map(x => x.RequireCollateral);
            Map(x => x.ProductGiven);
            Map(x => x.Position);
            Map(x => x.DropRound);
            Map(x => x.DeckBuilt);
            References(x => x.CubeDraft);
            References(x => x.MtgoLink);
            HasMany(x => x.CubeDraftPicks)
                .Cascade.Delete();
        }
    }
}
