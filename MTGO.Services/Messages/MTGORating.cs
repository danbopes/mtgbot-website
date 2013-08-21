using System;

namespace MTGO.Services.Messages
{
    [Serializable]
    public class MTGORating
    {
        public int Composite;
        public int Constructed;
        public int Limited;
        public string Username;

        public MTGORating(int composite, int constructed, int limited)
        {
            Composite = composite;
            Constructed = constructed;
            Limited = limited;
        }
    }
}
