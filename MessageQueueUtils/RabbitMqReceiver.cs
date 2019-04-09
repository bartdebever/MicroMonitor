﻿using System;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroMonitor.MessageQueueUtils
{
    public class RabbitMqReceiver : RabbitMqConnectionProducer
    {
        private EventingBasicConsumer _consumer;

        private string _queue;

        /// <inheritdoc />
        public RabbitMqReceiver()
        {
        }

        /// <inheritdoc />
        public RabbitMqReceiver(string userName, string password, string hostName, string virtualHost = "/")
            : base(userName, password, hostName, virtualHost)
        {
        }

        public void BindExchange(string exchange)
        {
            Channel.ExchangeDeclare(exchange, ExchangeType.Fanout, false, false);
        }

        /// <summary>
        /// Binds the receiver to a particular queue.
        /// Creating the queue if it doesn't exist in the process.
        /// </summary>
        /// <param name="queue">The queue wanting to be bound to.</param>
        /// <param name="exchange">The exchange which the queue is based on.</param>
        public void BindQueue(string queue, string exchange = null)
        {
            this._queue = queue;

            // Declare a non exclusive, non self-deleting queue.

            Channel.QueueDeclare(queue, false, false, false);
            if (!string.IsNullOrWhiteSpace(exchange))
            {
                Channel.QueueBind(queue, exchange, queue);
            }


            _consumer = new EventingBasicConsumer(Channel);
        }

        /// <summary>
        /// Declare the method ran when receiving a message.
        /// </summary>
        /// <param name="fallback">The method wanting to be ran.</param>
        public void DeclareReceived(EventHandler<BasicDeliverEventArgs> fallback)
        {
            _consumer.Received += fallback;
        }

        /// <summary>
        /// Runs the consuming process.
        /// </summary>
        public void Run()
        {
            Channel.BasicConsume(this._queue, true, _consumer);
        }
    }
}
