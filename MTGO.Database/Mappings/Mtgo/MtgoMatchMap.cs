using FluentNHibernate.Mapping;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Mappings.Mtgo
{
    public class MtgoMatchMap : ClassMap<MtgoMatch>
    {
        public MtgoMatchMap()
        {
            Id(x => x.Id)
                .GeneratedBy.Assigned();

            Map(x => x.Round);
            References(x => x.Tournament);
        }
    }
}
