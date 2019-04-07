using System;
using System.Text;

using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;

using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

using Serilog;

namespace MicroMonitor.AuthenticationHub
{
    public class Program
    {
        private static RabbitMqReceiver _receiver;

        private static RabbitMqProducer _producer;


        public static void Main(string[] args)
        {
            ConfigureLogger();
            Log.Information("Starting MicroMonitor Authentication Provider");
            SetupProducer();
            SetupConsumer();

            _receiver.Run();
            Log.Information("Setup done, waiting for messages.");
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static void SetupProducer()
        {
            _producer = new RabbitMqProducer();
            _producer.Connect();
            _producer.BindQueue(StaticQueues.GetAuth);
        }

        private static void SetupConsumer()
        {
            _receiver = new RabbitMqReceiver();
            _receiver.Connect();
            _receiver.BindQueue(StaticQueues.RequestAuth);
            _receiver.DeclareReceived(ConsumerOnReceived);
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            // TODO check secret
            var token = TokenProducer.ProduceToken();

            var tokenObject = new StoredToken
                            {
                                Token = token,
                                AuthenticatedAt = DateTime.Now
                            };

            Log.Information("Creating new token.");
            using (var context = new MonitorContext())
            {
                context.Tokens.Add(tokenObject);
                context.SaveChanges();
            }

            var properties = new BasicProperties { CorrelationId = e.BasicProperties.MessageId };
            _producer.SendMessage(token, properties);
        }
    }
}
