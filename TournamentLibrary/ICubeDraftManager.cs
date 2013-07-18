using System.Collections.Generic;

namespace MTGBotWebsite.TournamentLibrary
{
    public interface ICubeDraftManager
    {
        List<Drafter> Players { get; set; }
        void StartDraft();
        void PlayerSubscribe(string username, string connectionId);
        void PlayerUnSubscribe(string username, string connectionId);
        void Recover();
        void Pick(string drafterName, int pickNumber, int pickId);
    }
}