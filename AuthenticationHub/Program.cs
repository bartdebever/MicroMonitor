using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;
using System;
using System.Text;
using Newtonsoft.Json;
using NETCore.Encrypt;

namespace MicroMonitor.AuthenticationHub
{
    public class Program
    {
        private static RabbitMqReceiver _receiver;

        // For testing purposes only.
        private const string IV = "JYFrNePrBqFm6MEL";
        private const string KEY = "kf9C224Knj3R3n8VVwJ8lI3QWUQJ1Exy";

        /// <summary>
        /// Main method ran when the program starts.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            ConfigureLogger();
            Log.Information("Starting MicroMonitor Authentication Token Creator");
            _receiver = RabbitMqReceiver.Create(StaticQueues.RequestAuth, ConsumerOnReceived);

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

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            // Read the body into a UTF8 string.
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            string json;
            try
            {
                json = EncryptProvider.AESDecrypt(message, KEY, IV);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new ArgumentNullException(nameof(json));
                }
            }
            catch
            {
                Log.Warning("Invalid attempt to send message, wrong encryption Key and IV");
                return;
            }

            var service = JsonConvert.DeserializeObject<Service>(json);

            // Generate a new authentication token.
            var token = TokenProducer.ProduceToken();

            service.Token = token;

            SaveService(service);

            var producer = RabbitMqProducer.Create(service.ApplicationId);

            producer.SendMessage(token);
        }

        /// <summary>
        /// Saves the service to the database.
        /// </summary>
        /// <param name="service">The service object wanting to be saved.</param>
        private static void SaveService(Service service)
        {
            Log.Information("Creating new service for {applicationId}", service.ApplicationId);
            using (var context = new MonitorContext())
            {
                context.Services.Add(service);
                context.SaveChanges();
            }
        }
    }
}
