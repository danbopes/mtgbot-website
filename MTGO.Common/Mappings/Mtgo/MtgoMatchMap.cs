using FluentNHibernate.Mapping;
using MTGO.Common.Entities;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Mappings.Mtgo
{
    class MtgoMatchMap : ClassMap<MtgoMatch>
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
