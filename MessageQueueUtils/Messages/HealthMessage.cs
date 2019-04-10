using System;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueUtils.Messages
{
    public class HealthMessage
    {
        public string AggregationId { get; set; }

        public string ApplicationId { get; set; }

        public double Health { get; set; }
    }
}
