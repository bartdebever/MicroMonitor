using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;
using System.Linq;
using System.Text;
using Bogus;
using MicroMonitor.MessageQueueUtils.Messages;

using Newtonsoft.Json;

using Serilog;

namespace AuthenticationProvider
{
    public class Program
    {
        private static RabbitMqProducer _authProducer;

        private static RabbitMqReceiver _authReceiver;

        /// <summary>
        /// Main method ran when executing the program.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            ConfigureLogger();
            Log.Information("Setting up MicroMonitor Authentication Token Provider");
            SetupAuthProducer();
            SetupAuthReceiver();

            _authReceiver.Run();
            Log.Information("Setup complete, waiting for request.");
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
        /// Sets up the authentication producer for use.
        /// </summary>
        private static void SetupAuthProducer()
        {
            _authProducer = new RabbitMqProducer();
            _authProducer.Connect();
            _authProducer.BindExchange(StaticQueues.IsAuthenticatedReply);
        }

        /// <summary>
        /// Sets up the authentication receiver for use.
        /// </summary>
        private static void SetupAuthReceiver()
        {
            _authReceiver = new RabbitMqReceiver();
            _authReceiver.Connect();
            _authReceiver.BindQueue(StaticQueues.IsAuthenticated);
            _authReceiver.DeclareReceived(IsAuthenticatedOnReceived);
        }

        /// <summary>
        /// Method called when a new message is in.
        /// </summary>
        /// <param name="sender">The object which sent the message.</param>
        /// <param name="e">The arguments containing the message.</param>
        private static void IsAuthenticatedOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var token = Encoding.UTF8.GetString(body);

           
            Log.Information("Checking authentication for token \"{Token}\"", token);

            Service service;
            // Check if it exists in the database.
            using (var context = new MonitorContext())
            {
                service = context.Services.FirstOrDefault(x => x.Token == token);
            }

            var isAuthenticatedMessage = new IsAuthenticatedMessage
            {
                CorrelationId = e.BasicProperties.MessageId, IsAuthenticated = service != null, Service = service
            };

            // Send back a JSON message with the authentication status.
            var json = JsonConvert.SerializeObject(isAuthenticatedMessage);

            _authProducer.SendMessage(json);
        }
    }
}
