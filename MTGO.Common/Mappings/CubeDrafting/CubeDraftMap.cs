using FluentNHibernate.Mapping;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Common.Mappings.CubeDrafting
{
    class CubeDraftMap : ClassMap<CubeDraft>
    {
        public CubeDraftMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Created);
            Map(x => x.Status);
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
