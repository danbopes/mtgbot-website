using System;
using System.Linq;

namespace MTGO.Common.Models
{
    public partial class CubeDraft
    {
        public virtual int GetCurrentPick(int playerId)
        {
            /*var picks = CubeDraftPicks.Where(p => p.PlayerId == playerId).ToArray();

            return picks.Any() ? 0 : picks.Max(p => p.Pick);*/
            try
            {
                return CubeDraftPicks.Where(p => p.PlayerId == playerId).Max(p => p.Pick);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }
    }
}
