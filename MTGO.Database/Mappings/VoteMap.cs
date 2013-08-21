using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class VoteMap : ClassMap<Vote>
    {
        public VoteMap()
        {
            Id(x => x.Id);

            References(x => x.User);
            References(x => x.DraftPick);
            References(x => x.Card);
        }
    }
}
