using System;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    [Serializable]
    public class LoggingMessage
    {
        public string Sender { get; set; }

        public string Group { get; set; }

        public string Body { get; set; }
    }
}
