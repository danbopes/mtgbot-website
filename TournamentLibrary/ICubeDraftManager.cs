using System.Collections.Generic;
using MTGOLibrary.Models;

namespace MTGBotWebsite.TournamentLibrary
{
    public interface ICubeDraftManager
    {
        List<Drafter> Players { get; set; }
        void StartDraft();
        void PlayerSubscribe(User username, string connectionId);
        void PlayerUnSubscribe(User username, string connectionId);
        void Recover();
        void Pick(int playerId, int pickNumber, int pickId);
    }
}