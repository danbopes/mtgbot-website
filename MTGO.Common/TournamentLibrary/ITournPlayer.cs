namespace MTGO.Common.TournamentLibrary
{
    public interface ITournPlayer : IPlayer
    {
        int Wins { get; set; }
        int GameWins { get; set; }
        int Losses { get; set; }
        int GameLosses { get; set; }
        int Ties { get; set; }
        float TieBreaker1 { get; set; }
        float TieBreaker2 { get; set; }
        float TieBreaker3 { get; set; }
        int Points { get; }
        int DropRound { get; set; }
        bool IsActive { get; }
    }
}