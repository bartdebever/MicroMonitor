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
        private readonly HashSet<string> _authenticatedTokens;

        private RabbitMqReceiver _replyReceiver;

        private RabbitMqProducer _authSender;

        public AuthenticationFlow()
        {
            _authenticatedTokens = new HashSet<string>();
            SetupSender();
            SetupReceiver();

            _replyReceiver.Run();
        }

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

        private static void LogMessage(string message)
        {
            var messageObject = JsonConvert.DeserializeObject<LoggingMessage>(message);
            Log.Information("{Sender} from group {Group}: {Body}", messageObject.Sender, messageObject.Group, messageObject.Body);
        }

        private void SetupSender()
        {
            _authSender = new RabbitMqProducer();
            _authSender.Connect();
            _authSender.BindQueue(StaticQueues.IsAuthenticated);
        }

        private void SetupReceiver()
        {
            _replyReceiver = new RabbitMqReceiver();
            _replyReceiver.Connect();
            _replyReceiver.BindQueue(StaticQueues.IsAuthenticatedReply);
            _replyReceiver.DeclareReceived(ConsumerOnReceived);
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            var isAuthMessage = JsonConvert.DeserializeObject<IsAuthenticatedMessage>(message);

            if (!isAuthMessage.IsAuthenticated)
            {
                Log.Warning("Request without Authentication");
                return;
            }

            _authenticatedTokens.Add(isAuthMessage.Token);

            LogMessage(isAuthMessage.LogMessage);
        }
    }
}
