using MessageQueueUtils;
using System;

using RabbitMQ.Client.Events;
using System.Text;

using MessageQueueUtils.Messages;

using Newtonsoft.Json;

using Serilog;

namespace MessageQueueLoggingHub
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateLogger();

            Log.Debug("Starting new Receiver for queue: {0}", StaticQueues.LoggingQueue);
            var rabbitMqReceiver = new RabbitMqReceiver();
            rabbitMqReceiver.Connect();
            rabbitMqReceiver.BindQueue(StaticQueues.LoggingQueue);
            rabbitMqReceiver.DeclareReceived(ConsumerOnReceived);

            Log.Debug("Running RabbitMq Logger");
            rabbitMqReceiver.Run();
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            var messageObject = JsonConvert.DeserializeObject<LoggingMessage>(message);
            Log.Information("{Sender} from group {Group}: {Body}", messageObject.Sender, messageObject.Group, messageObject.Body);
        }

        /// <summary>
        /// Creates a new Serilog logger.
        /// </summary>
        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console().CreateLogger();
        }
    }
}
