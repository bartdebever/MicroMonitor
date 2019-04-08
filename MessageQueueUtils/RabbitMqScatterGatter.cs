using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using MicroMonitor.MessageQueueUtils.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace MicroMonitor.MessageQueueUtils
{
    public class RabbitMqScatterGatter
    {
        private List<RabbitMqProducer> _producers;

        private List<RabbitMqReceiver> _receivers;

        private List<double> _responses;

        private Timer _timer;

        /// <summary>
        /// Action that will be performed at the end of the timer or if all responses are in.
        /// </summary>
        public Action<List<double>> EndingAction { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqScatterGatter"/> class.
        /// Prepares all producers, receivers and the response list.
        /// </summary>
        /// <param name="outgoingQueues">The queues wanting to sent messages out to.</param>
        /// <param name="intcomingQueues">The queues where responses will be coming back to.</param>
        public RabbitMqScatterGatter(IEnumerable<string> outgoingQueues, IEnumerable<string> intcomingQueues)
        {
            _timer = new Timer();
            _responses = new List<double>();
            _producers = new List<RabbitMqProducer>();
            

            foreach (var outgoingQueue in outgoingQueues)
            {
                var producer = new RabbitMqProducer();
                producer.Connect();
                producer.BindQueue(outgoingQueue);
                _producers.Add(producer);
            }

            _receivers = new List<RabbitMqReceiver>();

            foreach (var incomingQueue in intcomingQueues)
            {
                var receiver = new RabbitMqReceiver();
                receiver.Connect();
                receiver.BindQueue(incomingQueue);
                receiver.DeclareReceived(ConsumerReceived);

                _receivers.Add(receiver);
            }
        }

        /// <summary>
        /// Runs the actual Gather and Scatter algorithm.
        /// </summary>
        /// <typeparam name="T">The type of payload wanting to be sent.</typeparam>
        /// <param name="message">The message wanting to be sent to all queues.</param>
        public void Run<T>(AggregationMessage<T> message) where T : class
        {
            foreach (var rabbitMqProducer in _producers)
            {
                try
                {
                    rabbitMqProducer.SendObject(message);
                }
                catch
                {

                }
            }

            _timer.Interval = 60d;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Method that will be called when the timer has ticked for a minute.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments by the</param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            EndingAction.Invoke(_responses);
        }

        /// <summary>
        /// Method that will be called when the receiver, receives a message.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="args">The argument for the event.</param>
        private void ConsumerReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var json = Encoding.UTF8.GetString(body);

            var message = JsonConvert.DeserializeObject<AggregationMessage<double>>(json);
            if (message == null)
            {
                return;
            }

            _responses.Add(message.Payload);

            if (_responses.Count == _producers.Count)
            {
                _timer.Stop();
                EndingAction.Invoke(_responses);
            }
        }
    }
}
