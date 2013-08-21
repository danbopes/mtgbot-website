using FluentNHibernate.Mapping;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Mappings.Mtgo
{
    public class MtgoPlayerMap : ClassMap<MtgoPlayer>
    {
        public MtgoPlayerMap()
        {
            Id(x => x.Id)
                .GeneratedBy.Assigned();

            Map(x => x.Username);
        }
    }
}
