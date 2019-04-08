using System;

namespace MicroMonitor.MessageQueueUtils.Storage
{
    public class StoredToken
    {
        public long Id { get; set; }

        public string Token { get; set; }

        public DateTime AuthenticatedAt { get; set; }

        public StoredToken()
        {
        }

        public StoredToken(string token) : this (token, DateTime.Now)
        {
            
        }

        public StoredToken(string token, DateTime authenticatedAt)
        {
            Token = token;
            AuthenticatedAt = authenticatedAt;
        }
    }
}
