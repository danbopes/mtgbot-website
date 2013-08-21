using System.Collections.Generic;
using System.Linq;
using MTGO.Tournaments.Messages;

namespace MTGO.Tournaments.Helpers
{
    public static class PlayerHelper
    {
        public static void TournPlayerArray(this IList<ITournPlayer> players)
        {
            foreach (var player in players)
            {
                AddPlayer(players, new TournPlayer(player));
            }
        }
        
        public static int AddPlayer(this IList<ITournPlayer> players, ITournPlayer player)
        {
            if (FindById(players, player.PlayerId) == null)
                players.Add(player);

            return players.Count;
        }

        public static ITournPlayer FindById(this IList<ITournPlayer> players, int id)
        {
            return players.FirstOrDefault(p => p.PlayerId == id);
        }

        //public string PlayerToList(this IList<ITournPlayer> players)
        //{
        //    return String.Join(", ", this);
        //}
    }
}
