using System;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueUtils.Storage
{
    public class Service
    {
        public long Id { get; set; }

        public string ApplicationId { get; set; }

        public string GroupId { get; set; }

        public string Token { get; set; }
    }
}
