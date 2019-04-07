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

namespace AuthenticationProvider
{
    class Program
    {
        private static RabbitMqProducer _authProducer;

        private static RabbitMqReceiver _authReceiver;

        static void Main(string[] args)
        {
            SetupAuthProducer();
            SetupAuthReceiver();

            _authReceiver.Run();
        }

        private static void SetupAuthProducer()
        {
            _authProducer = new RabbitMqProducer();
            _authProducer.Connect();
            _authProducer.BindQueue(StaticQueues.IsAuthenticatedReply);
        }

        private static void SetupAuthReceiver()
        {
            _authReceiver = new RabbitMqReceiver();
            _authReceiver.Connect();
            _authReceiver.BindQueue(StaticQueues.IsAuthenticated);
            _authReceiver.DeclareReceived(IsAuthenticatedOnReceived);
        }

        private static void IsAuthenticatedOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            var isAuthenticatedMessage = JsonConvert.DeserializeObject<IsAuthenticatedMessage>(message);

            Console.WriteLine("Checking authentication for {0}", isAuthenticatedMessage.Token);
            bool isAuth;
            using (var context = new MonitorContext())
            {
                isAuth = context.Tokens.Any(x => x.Token == isAuthenticatedMessage.Token);
            }

            var properties = new BasicProperties
            {
                CorrelationId = e.BasicProperties.MessageId
            };

            isAuthenticatedMessage.IsAuthenticated = isAuth;

            var json = JsonConvert.SerializeObject(isAuthenticatedMessage);

            _authProducer.SendMessage(json, properties);
        }
    }
}
