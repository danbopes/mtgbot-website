namespace MTGO.Database.Models
{
    public class Card
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string MagicCardsInfoId { get; set; }
        public virtual bool Foil { get; set; }
        public virtual int CardTextureNumber { get; set; }
        public virtual string Rarity { get; set; }
        public virtual string ManaCost { get; set; }
        public virtual int CMC { get; set; }
        public virtual string Color { get; set; }
    
        public virtual CardSet CardSet { get; set; }
    }
}
