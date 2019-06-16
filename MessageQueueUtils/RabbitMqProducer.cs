using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Newtonsoft.Json;

using RabbitMQ.Client;

namespace MicroMonitor.MessageQueueUtils
{
    public class RabbitMqProducer : RabbitMqConnectionProducer
    {
        /// <inheritdoc />
        public RabbitMqProducer()
        {
        }

        /// <inheritdoc />
        public RabbitMqProducer(string userName, string password, string hostName, string virtualHost = "/")
            : base(userName, password, hostName, virtualHost)
        {
        }

        public static RabbitMqProducer Create(string queue, string exchange = "", bool autoDelete = false)
        {
            var producer = new RabbitMqProducer();
            producer.Connect();
            if (!string.IsNullOrWhiteSpace(exchange))
            {
                producer.BindExchange(exchange);
            }

            if (!string.IsNullOrWhiteSpace(queue))
            {
                producer.BindQueue(queue, autoDelete);
            }


            return producer;
        }

        /// <summary>
        /// Sends a new message to the queue.
        /// </summary>
        /// <param name="message">The message wanting to be sent.</param>
        /// <param name="properties">The optional properties.</param>
        public string SendMessage(string message, IBasicProperties properties = null)
        {
            var faker = new Faker();

            var body = Encoding.UTF8.GetBytes(message);
            var newProperties = Channel.CreateBasicProperties();
            newProperties.MessageId = faker.Random.AlphaNumeric(20);
            if (properties != null)
            {
                newProperties.Headers = properties.Headers;
            }

            Channel.BasicPublish(Exchange, Queue, newProperties, body);

            return newProperties.MessageId;
        }

        /// <summary>
        /// Converts an object to JSON and sends it to the queue.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="payload">The object being send.</param>
        public void SendObject<T>(T payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            this.SendMessage(json);
        }
    }
}
