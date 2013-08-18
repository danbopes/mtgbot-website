using System;

namespace MTGO.Common.Entities.Mtgo
{
    public class MtgoGame
    {
        public virtual int Id { get; set; }
        public virtual MtgoPlayer Player1 { get; set; }
        public virtual MtgoPlayer Player2 { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual int Player1Wins { get; set; }
        public virtual int Player2Wins { get; set; }
    }
}
