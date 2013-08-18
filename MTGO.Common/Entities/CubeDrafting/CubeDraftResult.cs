namespace MTGO.Common.Entities.CubeDrafting
{
    public class CubeDraftResult
    {
        public virtual int Id { get; set; }
        public virtual int Round { get; set; }
        public virtual int Player1Wins { get; set; }
        public virtual int Player2Wins { get; set; }
        public virtual int Ties { get; set; }
        public virtual int CurrentGame { get; set; }

        public virtual CubeDraft Draft { get; set; }
        public virtual CubeDraftPlayer Player1 { get; set; }
        public virtual CubeDraftPlayer Player2 { get; set; }
    }
}
