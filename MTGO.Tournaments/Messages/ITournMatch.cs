using System;
using MTGO.Common.TournamentLibrary;

namespace MTGO.Tournaments.Messages
{
    public interface ITournMatch
    {
        ITournPlayer Player1 { get; }
        ITournPlayer Player2 { get; }
        bool ReportedResult { get; }
        void ReportResult(IPlayer player, int wins, int losses, int ties);
    }
}