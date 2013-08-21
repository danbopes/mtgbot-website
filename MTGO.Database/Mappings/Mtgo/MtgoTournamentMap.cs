using FluentNHibernate.Mapping;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Mappings.Mtgo
{
    public class MtgoTournamentMap : ClassMap<MtgoTournament>
    {
        public MtgoTournamentMap()
        {
            Id(x => x.Id)
                .GeneratedBy.Assigned();

            Map(x => x.Token);
            Map(x => x.Description);
            Map(x => x.StartDate);
            HasMany(x => x.Matches);
        }
    }
}
