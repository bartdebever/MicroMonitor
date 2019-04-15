using System;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroMonitor.MessageQueueUtils
{
    public class RabbitMqReceiver : RabbitMqConnectionProducer
    {
        private EventingBasicConsumer _consumer;

        private AsyncEventingBasicConsumer _asyncConsumer;

        /// <inheritdoc />
        public RabbitMqReceiver()
        {
        }

        /// <inheritdoc />
        public RabbitMqReceiver(string userName, string password, string hostName, string virtualHost = "/")
            : base(userName, password, hostName, virtualHost)
        {
        }

        public static RabbitMqReceiver Create(string queue, EventHandler<BasicDeliverEventArgs> callback, string exchange = "", bool autoDelete = false)
        {
            var receiver = new RabbitMqReceiver();
            receiver.Connect();
            if (!string.IsNullOrWhiteSpace(exchange))
            {
                receiver.BindExchange(exchange);
            }

            receiver.BindQueue(queue, autoDelete);
            receiver.DeclareReceived(callback);

            return receiver;
        }

        /// <summary>
        /// Binds the receiver to a particular queue.
        /// Creating the queue if it doesn't exist in the process.
        /// </summary>
        /// <param name="queue">The queue wanting to be bound to.</param>
        /// <param name="autoDelete">If the queue should be automatically deleted.</param>
        public override void BindQueue(string queue, bool autoDelete = false)
        {
            base.BindQueue(queue, autoDelete);

            _consumer = new EventingBasicConsumer(Channel);
            _asyncConsumer = new AsyncEventingBasicConsumer(Channel);
        }

        /// <summary>
        /// Declare the method ran when receiving a message.
        /// </summary>
        /// <param name="callback">The method wanting to be ran.</param>
        public void DeclareReceived(EventHandler<BasicDeliverEventArgs> callback)
        {
            _consumer.Received += callback;
        }

        /// <summary>
        /// Declares the async method ran when receiving a message.
        /// </summary>
        /// <param name="callback">The method wanting to be ran.</param>
        public void DeclareReceivedAsync(AsyncEventHandler<BasicDeliverEventArgs> callback)
        {
            _asyncConsumer.Received += callback;
        }

        /// <summary>
        /// Runs the consuming process.
        /// </summary>
        public void Run()
        {
            Channel.BasicConsume(Queue, true, _consumer);
        }
    }
}
