using System;
using System.Collections.Generic;
using System.Text;
using MicroMonitor.MessageQueueUtils.Storage;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    public class IsAuthenticatedMessage : CorrelationMessage
    {
        public string Token => Service.Token;

        public string LogMessage { get; set; }

        public Service Service { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}
