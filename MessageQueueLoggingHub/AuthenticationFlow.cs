using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Serilog;
using System.Collections.Generic;
using System.Text;

namespace MicroMonitor.MessageQueueLoggingHub
{
    public class AuthenticationFlow
    {
        // A cache of the authenticated tokens.
        private readonly HashSet<string> _authenticatedTokens;

        private RabbitMqReceiver _replyReceiver;

        private RabbitMqProducer _authSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFlow"/> class.
        /// </summary>
        public AuthenticationFlow()
        {
            _authenticatedTokens = new HashSet<string>();
            SetupSender();
            SetupReceiver();

            _replyReceiver.Run();
        }

        /// <summary>
        /// Checks the cache if the token is already in memory and sends the message back.
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="token">The token received.</param>
        public void CheckAuthentication(string message, string token)
        {
            if (_authenticatedTokens.Contains(token))
            {
                LogMessage(message);
                return;
            }

            var isAuthenticatedMessage = new IsAuthenticatedMessage
            {
                Token = token,
                LogMessage = message
            };

            _authSender.SendObject(isAuthenticatedMessage);
        }

        /// <summary>
        /// Logs the message if the authentication was correct.
        /// </summary>
        /// <param name="message"></param>
        private static void LogMessage(string message)
        {
            var messageObject = JsonConvert.DeserializeObject<LoggingMessage>(message);
            Log.Information("{Sender} from group {Group}: {Body}", 
                messageObject.Sender, messageObject.Group, messageObject.Body);
        }

        /// <summary>
        /// Sets up the authentication producer.
        /// Sends a request to the authentication service to ask if the token is correct.
        /// </summary>
        private void SetupSender()
        {
            _authSender = new RabbitMqProducer();
            _authSender.Connect();
            _authSender.BindQueue(StaticQueues.IsAuthenticated);
        }

        /// <summary>
        /// Sets up the receiver who will catch the authentication service's reply.
        /// </summary>
        private void SetupReceiver()
        {
            _replyReceiver = new RabbitMqReceiver();
            _replyReceiver.Connect();
            _replyReceiver.BindQueue(StaticQueues.IsAuthenticatedReply);
            _replyReceiver.DeclareReceived(ConsumerOnReceived);
        }

        /// <summary>
        /// The method called when receiving a message.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The event arguments containing the message.</param>
        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            // Deserialize the JSON and check the result.
            var isAuthMessage = JsonConvert.DeserializeObject<IsAuthenticatedMessage>(message);

            if (!isAuthMessage.IsAuthenticated)
            {
                Log.Warning("Request without Authentication");
                return;
            }

            // Add the token to the cache and log the message.
            _authenticatedTokens.Add(isAuthMessage.Token);

            LogMessage(isAuthMessage.LogMessage);
        }
    }
}
