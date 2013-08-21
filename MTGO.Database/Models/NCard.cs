
namespace MTGO.Database.Models
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
        public virtual double? PricingLow { get; set; }
        public virtual double? PricingMid { get; set; }
        public virtual double? PricingHigh { get; set; }
        public virtual string BackId { get; set; }
        public virtual string Watermark { get; set; }
        public virtual string NameCN { get; set; }
        public virtual string NameTW { get; set; }
        public virtual string NameFR { get; set; }
        public virtual string NameDE { get; set; }
        public virtual string NameIT { get; set; }
        public virtual string NameJP { get; set; }
        public virtual string NamePT { get; set; }
        public virtual string NameRU { get; set; }
        public virtual string NameES { get; set; }
        public virtual string NameKO { get; set; }
        public virtual string LegalityBlock { get; set; }
        public virtual string LegalityStandard { get; set; }
        public virtual string LegalityExtended { get; set; }
        public virtual string LegalityModern { get; set; }
        public virtual string LegalityLegacy { get; set; }
        public virtual string LegalityVintage { get; set; }
        public virtual string LegalityHighlander { get; set; }
        public virtual string LegalityFrenchCommander { get; set; }
        public virtual string LegalityCommander { get; set; }
        public virtual string LegalityPeasant { get; set; }
        public virtual string LegalityPauper { get; set; }
    
        public virtual NSet NSet { get; set; }
    }
}
