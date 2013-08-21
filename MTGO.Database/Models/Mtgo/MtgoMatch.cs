namespace MTGO.Database.Models.Mtgo
{
    public class MtgoMatch
    {
        public virtual int Id { get; set; }
        public virtual int Round { get; set; }
        public virtual MtgoTournament Tournament { get; protected set; }
    }
}
