namespace MTGO.Common.TournamentLibrary
{
    public interface IPlayer
    {
        int PlayerId { get; }
        string PlayerName { get; }
        int MtgoId { get; }
        string MtgoUsername { get; }
    }
}