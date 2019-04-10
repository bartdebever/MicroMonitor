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
        private string _queue = string.Empty;

        /// <inheritdoc />
        public RabbitMqProducer()
        {
        }

        /// <inheritdoc />
        public RabbitMqProducer(string userName, string password, string hostName, string virtualHost = "/")
            : base(userName, password, hostName, virtualHost)
        {
        }

        /// <summary>
        /// Declares the queue that the messages will be sent to.
        /// </summary>
        /// <param name="queue">The queue the messages will be sent to.</param>
        public void BindQueue(string queue, bool autoDelete = false)
        {
            this._queue = queue;

            // Declare a non exclusive, self-deleting queue.
            Channel.QueueDeclare(queue, false, false, autoDelete);

            if (!string.IsNullOrWhiteSpace(Exchange))
            {
                Channel.QueueBind(queue, Exchange, queue);
            }

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

            Channel.BasicPublish(Exchange, _queue, newProperties, body);

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
