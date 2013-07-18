using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGBotWebsite.TournamentLibrary
{
    public class TournPlayerArray : List<ITournPlayer>
    {
        public TournPlayerArray(IEnumerable<ITournPlayer> source)
        {
            foreach (var player in source)
            {
                AddPlayer(new TournPlayer(player));
            }
        }

        public int AddPlayer(ITournPlayer player)
        {
            if ( FindById(player.PlayerId) == null )
                Add(new TournPlayer(player));

            return base.Count;
        }

        public ITournPlayer FindById(int id)
        {
            //int num = BinarySearch()
            return this.FirstOrDefault(p => p.PlayerId == id);
        }
    }
}