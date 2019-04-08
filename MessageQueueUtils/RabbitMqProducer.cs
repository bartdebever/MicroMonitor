using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using RabbitMQ.Client;

namespace MicroMonitor.MessageQueueUtils
{
    public class RabbitMqProducer : RabbitMqConnectionProducer
    {
        private string _queue;

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
        public void BindQueue(string queue)
        {
            this._queue = queue;

            // Declare a non exclusive, non self-deleting queue.
            Channel.QueueDeclare(queue, false, false, false);
        }

        /// <summary>
        /// Sends a new message to the queue.
        /// </summary>
        /// <param name="message">The message wanting to be sent.</param>
        /// <param name="properties">The optional properties.</param>
        public void SendMessage(string message, IBasicProperties properties = null)
        {
            var body = Encoding.UTF8.GetBytes(message);
            IBasicProperties newProperties = null;
            if (properties != null)
            {
                newProperties = Channel.CreateBasicProperties();
                newProperties.Headers = properties.Headers;
            }

            Channel.BasicPublish(string.Empty, _queue, newProperties, body);
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
