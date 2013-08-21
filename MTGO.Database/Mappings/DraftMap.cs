using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class DraftMap : ClassMap<Draft>
    {
        public DraftMap()
        {
            Id(x => x.Id);
            Map(x => x.DraftStatus).CustomType<DraftStatus>();
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
