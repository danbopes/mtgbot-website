using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class CardMap : ClassMap<Card>
    {
        public CardMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();

            Map(x => x.Name);
            Map(x => x.MagicCardsInfoId);
            Map(x => x.Foil);
            Map(x => x.CardTextureNumber);
            Map(x => x.Rarity);
            Map(x => x.ManaCost);
            Map(x => x.CMC);
            Map(x => x.Color);

            References(x => x.CardSet);
        }
    }
}
