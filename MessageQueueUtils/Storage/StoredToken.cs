using System;

namespace MicroMonitor.MessageQueueUtils.Storage
{
    public class StoredToken
    {
        public long Id { get; set; }

        public string Token { get; set; }

        public DateTime AuthenticatedAt { get; set; }
    }
}
