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

        public MonitorContext() : base()
        {
        }

        public MonitorContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-BORT;Database=MicroMonitor;Trusted_Connection=True;MultipleActiveResultSets=true;");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
