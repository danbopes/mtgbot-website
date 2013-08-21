using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class MtgoLinkMap : ClassMap<MtgoLink>
    {
        public MtgoLinkMap()
        {
            Id(x => x.Id);
            Map(x => x.Confirmed);
            Map(x => x.ConfirmKey)
                .Nullable();

            References(x => x.User);
            References(x => x.Player);
        }
    }
}
