using System;

namespace MTGO.Common.TournamentLibrary
{
    [Serializable]
    public class TournPlayer : Player, ITournPlayer
    {
        //Points
        public int Wins { get; set; }
        public int GameWins { get; set; }
        public int Losses { get; set; }
        public int GameLosses { get; set; }
        public int Ties { get; set; }
        public float TieBreaker1 { get; set; }
        public float TieBreaker2 { get; set; }
        public float TieBreaker3 { get; set; }
        public int Points { get { return Wins * 3 + Ties * 1; } }
        public int DropRound { get; set; }
        public bool IsActive
        {
            get { return DropRound != 0; }
        }

        public TournPlayer(ITournPlayer player) : base(player.PlayerId, player.PlayerName, player.MtgoId, player.MtgoUsername)
        {
            Wins = player.Wins;
            GameWins = player.GameWins;
            Losses = player.Losses;
            GameLosses = player.GameLosses;
            Ties = player.Ties;
            DropRound = player.DropRound;
        }

        public TournPlayer(IPlayer player) : base(player)
        {
            Wins = 0;
            Losses = 0;
            Ties = 0;
            DropRound = -1;
        }

        public TournPlayer()
        {
        }

        public override string ToString()
        {
            return String.Format("{0} (Points {1})", PlayerName, Points);
        }
    }
}