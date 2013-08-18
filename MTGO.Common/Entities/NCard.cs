
namespace MTGO.Common.Entities
{
    public class NCard
    {
        public virtual string Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual string Set { get; set; }
        public virtual string Type { get; set; }
        public virtual string Rarity { get; set; }
        public virtual string Manacost { get; set; }
        public virtual string ConvertedManacost { get; set; }
        public virtual string Power { get; set; }
        public virtual string Toughness { get; set; }
        public virtual int? Loyalty { get; set; }
        public virtual string Ability { get; set; }
        public virtual string Flavor { get; set; }
        public virtual int? Variation { get; set; }
        public virtual string Artist { get; set; }
        public virtual string Number { get; set; }
        public virtual string Rating { get; set; }
        public virtual string Ruling { get; set; }
        public virtual string Color { get; set; }
        public virtual string GeneratedMana { get; set; }
        public virtual double? pricing_low { get; set; }
        public virtual double? pricing_mid { get; set; }
        public virtual double? pricing_high { get; set; }
        public virtual string back_id { get; set; }
        public virtual string watermark { get; set; }
        public virtual string name_CN { get; set; }
        public virtual string name_TW { get; set; }
        public virtual string name_FR { get; set; }
        public virtual string name_DE { get; set; }
        public virtual string name_IT { get; set; }
        public virtual string name_JP { get; set; }
        public virtual string name_PT { get; set; }
        public virtual string name_RU { get; set; }
        public virtual string name_ES { get; set; }
        public virtual string name_KO { get; set; }
        public virtual string legality_Block { get; set; }
        public virtual string legality_Standard { get; set; }
        public virtual string legality_Extended { get; set; }
        public virtual string legality_Modern { get; set; }
        public virtual string legality_Legacy { get; set; }
        public virtual string legality_Vintage { get; set; }
        public virtual string legality_Highlander { get; set; }
        public virtual string legality_French_Commander { get; set; }
        public virtual string legality_Commander { get; set; }
        public virtual string legality_Peasant { get; set; }
        public virtual string legality_Pauper { get; set; }
    
        public virtual NSet NSet { get; set; }
    }
}
