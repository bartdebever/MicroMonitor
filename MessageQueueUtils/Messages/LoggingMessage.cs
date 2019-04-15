using System;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    [Serializable]
    public class LoggingMessage
    {
        public LogLevel Level { get; set; } = LogLevel.Info;

        public string Sender { get; set; }

        public string Group { get; set; }

        public string Body { get; set; }
    }
}
