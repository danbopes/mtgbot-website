using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using MTGO.Common.Entities;

namespace MTGO.Common.Mappings
{
    public class NCardMap : ClassMap<NCard>
    {
        public NCardMap()
        {
            Id(x => x.Id)
                .Column("id")
                .GeneratedBy
                .Assigned();
            Map(x => x.Name)
                .Column("name");
            Map(x => x.Type)
                .Column("type");
            Map(x => x.Rarity)
                .Column("rarity");
            Map(x => x.Manacost)
                .Column("manacost");
            Map(x => x.ConvertedManacost)
                .Column("converted_manacost");
            Map(x => x.Power)
                .Column("power");
            Map(x => x.Toughness)
                .Column("toughness");
            Map(x => x.Loyalty)
                .Column("loyalty");
            Map(x => x.Ability)
                .Column("ability");
            Map(x => x.Flavor)
                .Column("flavor");
            Map(x => x.Variation)
                .Column("variation");
            Map(x => x.Artist)
                .Column("artist");
            Map(x => x.Number)
                .Column("number");
            Map(x => x.Rating)
                .Column("rating");
            Map(x => x.Ruling)
                .Column("ruling");
            Map(x => x.Color)
                .Column("color");
            Map(x => x.GeneratedMana)
                .Column("generated_mana");
            Map(x => x.pricing_low)
                .Column("pricing_low");
            Map(x => x.pricing_mid)
                .Column("pricing_mid");
            Map(x => x.pricing_high)
                .Column("pricing_high");
            Map(x => x.back_id)
                .Column("back_id");
            Map(x => x.watermark)
                .Column("watermark");
            Map(x => x.name_CN)
                .Column("name_CN");
            Map(x => x.name_TW)
                .Column("name_TW");
            Map(x => x.name_FR)
                .Column("name_FR");
            Map(x => x.name_DE)
                .Column("name_DE");
            Map(x => x.name_IT)
                .Column("name_IT");
            Map(x => x.name_JP)
                .Column("name_JP");
            Map(x => x.name_PT)
                .Column("name_PT");
            Map(x => x.name_RU)
                .Column("name_RU");
            Map(x => x.name_ES)
                .Column("name_ES");
            Map(x => x.name_KO)
                .Column("name_KO");
            Map(x => x.legality_Block)
                .Column("legality_Block");
            Map(x => x.legality_Standard)
                .Column("legality_Standard");
            Map(x => x.legality_Extended)
                .Column("legality_Extended");
            Map(x => x.legality_Modern)
                .Column("legality_Modern");
            Map(x => x.legality_Legacy)
                .Column("legality_Legacy");
            Map(x => x.legality_Vintage)
                .Column("legality_Vintage");
            Map(x => x.legality_Highlander)
                .Column("legality_Highlander");
            Map(x => x.legality_French_Commander)
                .Column("legality_French_Commander");
            Map(x => x.legality_Commander)
                .Column("legality_Commander");
            Map(x => x.legality_Peasant)
                .Column("legality_Peasant");
            Map(x => x.legality_Pauper)
                .Column("legality_Pauper");

            References(x => x.NSet)
                .Column("`set`");
        }
    }
}
