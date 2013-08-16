using System.ServiceModel;
using MTGO.Common.TournamentLibrary;

namespace MTGO.Common
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
        void NotifyPairings(int id, TournMatchArray pairings);
    }
}
