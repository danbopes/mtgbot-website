using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGBotWebsite.TournamentLibrary
{
    public class TournPlayer : Player, ITournPlayer
    {
        //Points
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public int Points { get { return Wins * 3 + Ties * 1; } }
        public int DropRound { get; set; }
        public bool IsActive
        {
            get { return DropRound != 0; }
        }

        public TournPlayer(ITournPlayer player) : base(player.PlayerId, player.PlayerName, player.MtgoId, player.MtgoUsername)
        {
            Wins = player.Wins;
            Losses = player.Losses;
            Ties = player.Ties;
            DropRound = player.DropRound;
        }

        public TournPlayer(IPlayer player) : base(player)
        {
            Wins = 0;
            Losses = 0;
            Ties = 0;
            DropRound = 0;
        }
    }
}