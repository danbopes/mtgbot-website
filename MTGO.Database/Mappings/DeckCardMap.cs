using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class DeckCardMap : ClassMap<DeckCard>
    {
        public DeckCardMap()
        {
            Id(x => x.Id);
            Map(x => x.Quantity);

            References(x => x.Card);
        }
    }
}
