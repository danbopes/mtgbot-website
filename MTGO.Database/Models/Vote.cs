namespace MTGO.Database.Models
{
    public class Vote
    {
        public virtual int Id { get; protected set; }

        public virtual User User { get; set; }
        public virtual DraftPick DraftPick { get; protected set; }
        public virtual Card Card { get; protected set; }
    }
}
