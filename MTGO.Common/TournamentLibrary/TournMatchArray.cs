using System.Collections.Generic;
using System.Linq;

namespace MTGO.Common.TournamentLibrary
{
    public class TournMatchArray : List<TournMatch>
    {
        public TournMatchArray()
        {
            
        }

        public TournMatchArray(IEnumerable<TournMatch> source)
        {
            foreach (var match in source)
            {
                AddMatch(match);
            }
        }

        public int AddMatch(TournMatch match)
        {
            Add(match);

            return Count;
        }

        public TournMatch[] GetByRound(int round)
        {
            return GetByRound(round, round);
        }

        public TournMatch[] GetByRound(int startRound, int endRound)
        {
            return this.Where(m => m.Round >= startRound && m.Round <= endRound).ToArray();
        }
    }
}