using FluentNHibernate.Mapping;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Database.Mappings.CubeDrafting
{
    public class CubeDraftMap : ClassMap<CubeDraft>
    {
        public CubeDraftMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Created);
            Map(x => x.Status).CustomType<CubeDraftStatus>();
            Map(x => x.RoundTime);
            Map(x => x.RequireWatchers);
            Map(x => x.Timed);
            Map(x => x.BotName);
            References(x => x.Creator);
            HasMany(x => x.CubeDraftCards)
                .Cascade.Delete();
            HasMany(x => x.CubeDraftPicks)
                .Cascade.Delete();
            HasMany(x => x.CubeDraftPlayers)
                .Cascade.Delete();
            HasMany(x => x.CubeDraftResults)
                .Cascade.Delete();
        }
    }
}
