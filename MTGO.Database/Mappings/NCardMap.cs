using FluentNHibernate.Mapping;
using MTGO.Database.Models;

namespace MTGO.Database.Mappings
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
            Map(x => x.PricingLow)
                .Column("pricing_low");
            Map(x => x.PricingMid)
                .Column("pricing_mid");
            Map(x => x.PricingHigh)
                .Column("pricing_high");
            Map(x => x.BackId)
                .Column("back_id");
            Map(x => x.Watermark)
                .Column("watermark");
            Map(x => x.NameCN)
                .Column("name_CN");
            Map(x => x.NameTW)
                .Column("name_TW");
            Map(x => x.NameFR)
                .Column("name_FR");
            Map(x => x.NameDE)
                .Column("name_DE");
            Map(x => x.NameIT)
                .Column("name_IT");
            Map(x => x.NameJP)
                .Column("name_JP");
            Map(x => x.NamePT)
                .Column("name_PT");
            Map(x => x.NameRU)
                .Column("name_RU");
            Map(x => x.NameES)
                .Column("name_ES");
            Map(x => x.NameKO)
                .Column("name_KO");
            Map(x => x.LegalityBlock)
                .Column("legality_Block");
            Map(x => x.LegalityStandard)
                .Column("legality_Standard");
            Map(x => x.LegalityExtended)
                .Column("legality_Extended");
            Map(x => x.LegalityModern)
                .Column("legality_Modern");
            Map(x => x.LegalityLegacy)
                .Column("legality_Legacy");
            Map(x => x.LegalityVintage)
                .Column("legality_Vintage");
            Map(x => x.LegalityHighlander)
                .Column("legality_Highlander");
            Map(x => x.LegalityFrenchCommander)
                .Column("legality_French_Commander");
            Map(x => x.LegalityCommander)
                .Column("legality_Commander");
            Map(x => x.LegalityPeasant)
                .Column("legality_Peasant");
            Map(x => x.LegalityPauper)
                .Column("legality_Pauper");

            References(x => x.NSet)
                .Column("`set`");
        }
    }
}
