using System;

namespace MTGO.Common
{
    public enum QueueStatus
    {
        Waiting = 0,
        Trading = 1,
        Skipped = 2,
        Recieved = 3
    }

    public class PlayerQueueStatus
    {
        public int PlayerId { get; set; }
        public QueueStatus Status { get; set; }
        public TimeSpan Timer { get; set; }
    }
}
