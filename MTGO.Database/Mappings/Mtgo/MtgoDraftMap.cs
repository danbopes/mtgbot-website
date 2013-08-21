using FluentNHibernate.Mapping;
using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Mappings.Mtgo
{
    public class MtgoDraftMap : ClassMap<MtgoDraft>
    {
        public MtgoDraftMap()
        {
            Id(x => x.Id)
                .GeneratedBy
                .Assigned();
            Map(x => x.Token);
        }
    }
}
