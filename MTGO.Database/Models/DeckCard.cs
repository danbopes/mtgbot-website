namespace MTGO.Database.Models
{
    public class DeckCard
    {
        public virtual int Id { get; protected set; }
        public virtual Card Card { get; set; }
        public virtual int Quantity { get; set; }
    }
}
