using System;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    /// <summary>
    /// RabbitMQ's build in correlationId seems buggy so using this instead.
    /// </summary>
    public abstract class CorrelationMessage
    {
        public string CorrelationId { get; set; }
    }
}
