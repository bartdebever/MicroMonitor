using System;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    public class IsAuthenticatedMessage
    {
        public string Token { get; set; }

        public string LogMessage { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}
