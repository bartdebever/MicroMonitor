using System;
using System.Collections.Generic;
using System.Text;

using MicroMonitor.MessageQueueUtils.Storage;

using Microsoft.EntityFrameworkCore;

namespace MicroMonitor.Data
{
    public class MonitorContext : DbContext
    {
        public DbSet<Service> Services { get; set; }

        public MonitorContext() : base()
        {
        }

        public MonitorContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Rough hardcoded SQL string for now. Should be moved to configuration if this wasn't a PoC.
            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-BORT;Database=MicroMonitor;Trusted_Connection=True;MultipleActiveResultSets=true;");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>().HasAlternateKey(u => u.ApplicationId);
        }
    }
}
