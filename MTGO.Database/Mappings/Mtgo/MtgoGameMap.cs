using FluentNHibernate.Mapping;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Mappings.Mtgo
{
    public class MtgoGameMap : ClassMap<MtgoGame>
    {
        public MtgoGameMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();
            Map(x => x.Player1Wins);
            Map(x => x.Player2Wins);
            References(x => x.Player1);
            References(x => x.Player2)
                .Nullable();
        }
    }
}
