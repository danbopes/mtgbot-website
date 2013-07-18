using System;

namespace MTGBotWebsite.TournamentLibrary
{
    public interface IPlayer : IComparable
    {
        int PlayerId { get; }
        string PlayerName { get; }
        int MtgoId { get; }
        string MtgoUsername { get; }
    }
}