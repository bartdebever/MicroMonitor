using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MessageQueueUtils.Messages
{
    [Serializable]
    public class LoggingMessage
    {
        public string Sender { get; set; }

        public string Group { get; set; }

        public string Body { get; set; }
    }
}
