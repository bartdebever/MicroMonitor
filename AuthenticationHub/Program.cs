using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;
using System;
using System.Text;

namespace MicroMonitor.AuthenticationHub
{
    public class Program
    {
        private static RabbitMqReceiver _receiver;

        private static RabbitMqProducer _producer;

        /// <summary>
        /// Main method ran when the program starts.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            ConfigureLogger();
            Log.Information("Starting MicroMonitor Authentication Provider");

            SetupProducer();
            SetupConsumer();

            _receiver.Run();
            Log.Information("Setup done, waiting for messages.");
        }

        /// <summary>
        /// Configures the Serilog logger.
        /// </summary>
        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        /// <summary>
        /// Sets up the producer to be used.
        /// </summary>
        private static void SetupProducer()
        {
            _producer = new RabbitMqProducer();
            _producer.Connect();
            _producer.BindQueue(StaticQueues.GetAuth);
        }

        /// <summary>
        /// Sets up the consumer to be used.
        /// </summary>
        private static void SetupConsumer()
        {
            _receiver = new RabbitMqReceiver();
            _receiver.Connect();
            _receiver.BindQueue(StaticQueues.RequestAuth);
            _receiver.DeclareReceived(ConsumerOnReceived);
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            // Read the body into a UTF8 string.
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            // TODO check secret

            // Generate a new authentication token.
            var token = TokenProducer.ProduceToken();

            SaveToken(new StoredToken(token));

            var properties = new BasicProperties
            {
                CorrelationId = e.BasicProperties.MessageId
            };

            _producer.SendMessage(token, properties);
        }

        /// <summary>
        /// Saves the token to the database.
        /// </summary>
        /// <param name="token">The token object wanting to be saved.</param>
        private static void SaveToken(StoredToken token)
        {
            Log.Information("Creating new token.");
            using (var context = new MonitorContext())
            {
                context.Tokens.Add(token);
                context.SaveChanges();
            }
        }
    }
}
