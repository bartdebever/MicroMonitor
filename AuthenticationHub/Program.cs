using System.Text;

using MicroMonitor.MessageQueueUtils;

using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace MicroMonitor.AuthenticationHub
{
    public class Program
    {
        private static RabbitMqReceiver receiver;

        private static RabbitMqProducer producer;

        public static void Main(string[] args)
        {
            SetupProducer();
            SetupConsumer();
            receiver.Run();
        }

        private static void SetupProducer()
        {
            producer = new RabbitMqProducer();
            producer.Connect();
            producer.BindQueue(StaticQueues.GetAuth);
        }

        private static void SetupConsumer()
        {
            receiver = new RabbitMqReceiver();
            receiver.Connect();
            receiver.BindQueue(StaticQueues.RequestAuth);
            receiver.DeclareReceived(ConsumerOnReceived);
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);

            // TODO check secret
            var token = TokenProducer.ProduceToken();
            var properties = new BasicProperties { CorrelationId = e.BasicProperties.MessageId };
            producer.SendMessage(token, properties);
        }
    }
}
