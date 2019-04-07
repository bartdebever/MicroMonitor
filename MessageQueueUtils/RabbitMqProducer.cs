using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace MessageQueueUtils
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
        public void DeclareQueue(string queue)
        {
            this._queue = queue;

            // Declare a non exclusive, non self-deleting queue.
            Channel.QueueDeclare(queue, false, false, false);
        }

        /// <summary>
        /// Sends a text message to the queue.
        /// </summary>
        /// <param name="message">The message provided</param>
        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            Channel.BasicPublish(string.Empty, _queue, null, body);
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
