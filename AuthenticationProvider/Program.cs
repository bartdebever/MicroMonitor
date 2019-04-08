using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;
using System.Linq;
using System.Text;

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
            Log.Information("Setting up MicroMonitor Authentication Provider");
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
            _authProducer.BindQueue(StaticQueues.IsAuthenticatedReply);
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
            var message = Encoding.UTF8.GetString(body);

            // Gain authentication message from JSON Body.
            var isAuthenticatedMessage = JsonConvert.DeserializeObject<IsAuthenticatedMessage>(message);

            Log.Information("Checking authentication for token \"{Token}\"", isAuthenticatedMessage.Token);

            bool isAuth;
            // Check if it exists in the database.
            using (var context = new MonitorContext())
            {
                isAuth = context.Tokens.Any(x => x.Token == isAuthenticatedMessage.Token);
            }

            var properties = new BasicProperties
            {
                CorrelationId = e.BasicProperties.MessageId
            };

            isAuthenticatedMessage.IsAuthenticated = isAuth;
            
            // Send back a JSON message with the authentication status.
            var json = JsonConvert.SerializeObject(isAuthenticatedMessage);

            _authProducer.SendMessage(json, properties);
        }
    }
}
