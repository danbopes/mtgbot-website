using System;

namespace MTGBotWebsite.TournamentLibrary
{
    public interface ITournPlayer : IPlayer, IComparable
    {
        int Wins { get; set; }
        int Losses { get; set; }
        int Ties { get; set; }
        int Points { get; }
        int DropRound { get; set; }
        bool IsActive { get; }
    }
}