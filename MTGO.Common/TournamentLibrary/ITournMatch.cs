using System;

namespace MTGO.Common.TournamentLibrary
{
    public interface ITournMatch : IComparable
    {
        ITournPlayer Player1 { get; }
        ITournPlayer Player2 { get; }
        bool ReportedResult { get; }
        void ReportResult(IPlayer player, int wins, int losses, int ties);
    }
}