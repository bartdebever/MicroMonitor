using System;
using System.Collections.Generic;
using System.Text;

using MicroMonitor.MessageQueueUtils.Storage;

using Microsoft.EntityFrameworkCore;

namespace MicroMonitor.Data
{
    public class MonitorContext : DbContext
    {
        public DbSet<StoredToken> Tokens { get; set; }
    }
}
