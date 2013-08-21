using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
{
    public class NSetMap : ClassMap<NSet>
    {
        public NSetMap()
        {
            Id(x => x.Code)
                .Column("code");
            Map(x => x.Name)
                .Column("name");
            Map(x => x.CodeMagiccards)
                .Column("code_magiccards");
            Map(x => x.Date)
                .Column("date");
            Map(x => x.IsPromo)
                .Column("is_promo");

            HasMany(x => x.NCards)
                .KeyColumn("set");
        }
    }
}
