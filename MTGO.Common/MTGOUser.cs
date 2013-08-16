using System;

namespace MTGO.Common
{
    [Serializable]
    public class MTGOUser
    {
        public int PlayerId { get; set; }
        public string Username { get; set; }
    }
}
