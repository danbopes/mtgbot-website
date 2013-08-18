using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    class DraftMap : ClassMap<Draft>
    {
        public DraftMap()
        {
            Id(x => x.Id);
            Map(x => x.DraftStatus);
            Map(x => x.CurrentPack);

            References(x => x.MtgoDraft);
            References(x => x.Broadcaster);

            HasMany(x => x.DraftPicks)
                .Cascade.Delete();

            HasMany(x => x.Deck)
                .Cascade.Delete();
        }
    }
}
