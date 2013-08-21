using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGO.Database.Models.CubeDrafting
{
    public class CubeDraft
    {
        public CubeDraft()
        {
            CubeDraftCards = new List<CubeDraftCard>();
            CubeDraftPlayers = new List<CubeDraftPlayer>();
            CubeDraftPicks = new List<CubeDraftPick>();
            CubeDraftResults = new List<CubeDraftResult>();
            Created = DateTime.Now;
        }
    
        public virtual int Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual CubeDraftStatus Status { get; set; }
        public virtual int RoundTime { get; set; }
        public virtual bool RequireWatchers { get; set; }
        public virtual bool Timed { get; set; }
        public virtual string BotName { get; set; }
    
        public virtual User Creator { get; set; }
        public virtual IList<CubeDraftCard> CubeDraftCards { get; protected set; }
        public virtual IList<CubeDraftPlayer> CubeDraftPlayers { get; protected set; }
        public virtual IList<CubeDraftPick> CubeDraftPicks { get; protected set; }
        public virtual IList<CubeDraftResult> CubeDraftResults { get; protected set; }

        public virtual int GetCurrentPick(int playerId)
        {
            try
            {
                return CubeDraftPicks.Where(pick => pick.Player.Id == playerId).Max(pick => pick.PickNumber);
            }
            catch (InvalidCastException)
            {
                return 0;
            }
        }
    }
}
