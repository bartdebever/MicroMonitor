using System;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    public class AggregationMessage<T>
    {
        public string AggregationId { get; set; }

        public T Payload { get; set; }
    }
}
