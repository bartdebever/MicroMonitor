using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;
using MicroMonitor.MessageQueueUtils.Storage;
using Serilog;

namespace MicroMonitor.HeathChecks
{
    public class Program
    {
        private static RabbitMqScatterGatter _scatterGatter;

        /// <summary>
        /// Runs the main health checking logic.
        /// </summary>
        /// <param name="args">Arguments provided by the commandline.</param>
        public static void Main(string[] args)
        {
            var services = GetServices();

            var outgoingQueues = services.Select(x => x.ApplicationId);

            var faker = new Faker();
            _scatterGatter = new RabbitMqScatterGatter(outgoingQueues, StaticQueues.HealthCheckReply) {EndingAction = LogStatus};
            var message = new AggregationMessage<string>
            {
                Payload = "Please report health check",
                // Generate a pseudorandom Aggregation Id for all the messages.
                AggregationId = faker.Random.AlphaNumeric(10)
            };
            
            _scatterGatter.Run(message);
        }

        /// <summary>
        /// Method that will happen once all values are in.
        /// </summary>
        /// <param name="values"></param>
        private static void LogStatus(Dictionary<string, double> values)
        {
            foreach (var servicePercentagePairs in values)
            {
                Log.Information("{service} Reported back: {percentage}", servicePercentagePairs.Key, servicePercentagePairs.Value);
            }
        }

        /// <summary>
        /// Gets the services from the database.
        /// </summary>
        /// <returns>The services out of the database.</returns>
        private static List<Service> GetServices()
        {
            using (var context = new MonitorContext())
            {
                return context.Services.ToList();
            }
        }
    }
}
