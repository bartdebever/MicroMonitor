using System;
using System.Collections.Generic;
using Bogus;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;
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
            var incomingQueues = new List<string>();
            var outgoingQueues = new List<string>();
            var faker = new Faker();
            _scatterGatter = new RabbitMqScatterGatter(outgoingQueues, incomingQueues) {EndingAction = LogStatus};
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
        private static void LogStatus(List<double> values)
        {
            foreach (var percentage in values)
            {
                Log.Information("X Reported back: {percentage}", percentage);
            }
        }
    }
}
