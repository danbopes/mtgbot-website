using FluentNHibernate.Mapping;
using MTGO.Common.Entities;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Mappings.Mtgo
{
    class MtgoTournamentMap : ClassMap<MtgoTournament>
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
