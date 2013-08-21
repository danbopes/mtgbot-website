namespace MTGO.Database.Models.CubeDrafting
{
    public class CubeDraftCard
    {
        public virtual int Id { get; protected set; }
        public virtual int Location { get; set; }

        public virtual CubeDraft CubeDraft { get; set; }
        public virtual Card Card { get; set; }
    }
}
