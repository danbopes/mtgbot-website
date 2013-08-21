using System.Collections.Generic;
using System.ServiceModel;
using MTGO.Services.Messages;
using MTGO.Tournaments.Messages;

namespace MTGO.Services
{
    [ServiceContract]
    public interface IBotService
    {
        [OperationContract]
        MTGORating GetRating(string username);

        [OperationContract]
        string StartCubeDraft(int id);

        [OperationContract]
        void UpdateCubeStatus(int id);

        [OperationContract]
        MTGOUser SendMessage(string username, string message);

        [OperationContract]
        void NotifyPairings(int id, IEnumerable<TournMatch> pairings);
    }
}
