using MicroMonitor.Data;
using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Storage;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;
using System.Linq;
using System.Text;

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


            Console.WriteLine("Hello World!");
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

            bool isAuth;
            using (var context = new MonitorContext())
            {
                isAuth = context.Tokens.Any(x => x.Token == message);
            }

            var properties = new BasicProperties
            {
                CorrelationId = e.BasicProperties.MessageId
            };

            _authProducer.SendMessage(isAuth.ToString(), properties);
        }
    }
}
