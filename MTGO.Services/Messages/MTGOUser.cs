using System;

namespace MTGO.Services.Messages
{
    [Serializable]
    public class MTGOUser
    {
        public int PlayerId { get; set; }
        public string Username { get; set; }
    }
}
