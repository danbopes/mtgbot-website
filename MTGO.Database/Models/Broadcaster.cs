namespace MTGO.Database.Models
{
    public class Broadcaster
    {
        public virtual int Id { get; protected set; }
        public virtual string IrcSettings { get; set; }
        public virtual User User { get; set; }
    }
}
