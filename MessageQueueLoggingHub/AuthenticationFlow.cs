using System;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Serilog;
using System.Collections.Generic;
using System.Text;
using Bogus;
using MicroMonitor.MessageQueueUtils.Storage;

namespace MicroMonitor.MessageQueueLoggingHub
{
    public class AuthenticationFlow
    {
        // A cache of the authenticated tokens.
        private readonly Dictionary<string, Service> _authenticatedTokens;

        private readonly Dictionary<string, string> _unprocessed;

        private RabbitMqReceiver _replyReceiver;

        private RabbitMqProducer _authSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFlow"/> class.
        /// </summary>
        public AuthenticationFlow()
        {
            _unprocessed = new Dictionary<string, string>();
            _authenticatedTokens = new Dictionary<string, Service>();
            _authSender = RabbitMqProducer.Create(StaticQueues.IsAuthenticated);
            var faker = new Faker();
            _replyReceiver = RabbitMqReceiver.Create(faker.Random.AlphaNumeric(15), ConsumerOnReceived, StaticQueues.IsAuthenticatedReply);

            _replyReceiver.Run();
        }

        /// <summary>
        /// Checks the cache if the token is already in memory and sends the message back.
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="token">The token received.</param>
        public void CheckAuthentication(string message, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (_authenticatedTokens.ContainsKey(token))
            {
                LogMessage(message);
                return;
            }


            var messageId = _authSender.SendMessage(token);

            _unprocessed.Add(messageId, message);
        }

        /// <summary>
        /// Logs the message if the authentication was correct.
        /// </summary>
        /// <param name="message"></param>
        private static void LogMessage(string message)
        {
            var messageObject = JsonConvert.DeserializeObject<LoggingMessage>(message);
            var template = "{Sender} from group {Group}: {Body}";
            var paramValues = new[] {messageObject.Sender, messageObject.Group, messageObject.Body};

            switch (messageObject.Level)
            {
                case LogLevel.Error:
                    Log.Error(template, paramValues);
                    break;
                case LogLevel.Warning:
                    Log.Warning(template, paramValues);
                    break;
                case LogLevel.Info:
                default:
                    Log.Information(template, paramValues);
                    break;
            }
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

            if (!_unprocessed.ContainsKey(isAuthMessage.CorrelationId))
            {
                return;
            }

            if (!isAuthMessage.IsAuthenticated)
            {
                Log.Warning("Request without Authentication");
                return;
            }

            if (!_authenticatedTokens.ContainsKey(isAuthMessage.Token))
            {
                // Add the token to the cache and log the message.
                _authenticatedTokens.Add(isAuthMessage.Token, isAuthMessage.Service);
            }

            if (!_unprocessed.ContainsKey(isAuthMessage.CorrelationId))
            {
                return;
            }

            var logMessage = _unprocessed[isAuthMessage.CorrelationId];
            _unprocessed.Remove(isAuthMessage.CorrelationId);

            LogMessage(logMessage);
        }
    }
}
