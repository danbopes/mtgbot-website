using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class CardSetMap : ClassMap<CardSet>
    {
        public CardSetMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();

            Map(x => x.Name);
            Map(x => x.Short);
            Map(x => x.GathererSet);
            Map(x => x.MagicCardsInfoSet);
            Map(x => x.MtgoBoosterId);
            Map(x => x.Completed);

            HasMany(x => x.Cards);
        }
    }
}
