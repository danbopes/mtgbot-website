using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTGO.Tournaments.Messages;

namespace MTGO.Tournaments.Helpers
{
    public static class TournMatchHelper 
    {
        public static void TournMatchArray(this IList<ITournMatch> matches)
        {
            foreach (var match in matches)
            {
                AddMatch(matches, match);
            }
        }

        public static int AddMatch(this IList<ITournMatch> matches, ITournMatch match)
        {
            matches.Add(match);
            return matches.Count;
        }

        public TournMatch[] GetByRound(this IList<ITournMatch> matches, int round)
        {
            return GetByRound(round, round);
        }

        public TournMatch[] GetByRound(this IList<ITournMatch> matches, int startRound, int endRound)
        {
            return matches.Where(m => m.Round >= startRound && m.Round <= endRound).ToArray();
        }
    }
}
