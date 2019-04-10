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

        private RabbitMqReceiver _receiver;

        private Dictionary<string, double> _responses;

        private int _responsesReceived = 0;

        private Timer _timer;

        private string _aggregationId;

        /// <summary>
        /// Action that will be performed at the end of the timer or if all responses are in.
        /// </summary>
        public Action<Dictionary<string, double>> EndingAction { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqScatterGatter"/> class.
        /// Prepares all producers, receivers and the response list.
        /// </summary>
        /// <param name="outgoingQueues">The queues wanting to sent messages out to.</param>
        /// <param name="incomingQueue">The queue where responses will be coming back to.</param>
        public RabbitMqScatterGatter(IEnumerable<string> outgoingQueues, string incomingQueue)
        {
            _timer = new Timer();
            _responses = new Dictionary<string, double>();
            _producers = new List<RabbitMqProducer>();


            foreach (var outgoingQueue in outgoingQueues)
            {
                var producer = new RabbitMqProducer();
                producer.Connect();
                producer.BindQueue(outgoingQueue);
                _producers.Add(producer);

                // Outgoing queue is the applicationId so this can be used for the key as well.
                _responses.Add(outgoingQueue, 0);
            }

            _receiver = new RabbitMqReceiver();
            _receiver.Connect();
            _receiver.BindQueue(incomingQueue);
            _receiver.DeclareReceived(ConsumerReceived);
        }

        /// <summary>
        /// Runs the actual Gather and Scatter algorithm.
        /// </summary>
        /// <typeparam name="T">The type of payload wanting to be sent.</typeparam>
        /// <param name="message">The message wanting to be sent to all queues.</param>
        public void Run<T>(AggregationMessage<T> message) where T : class
        {
            _aggregationId = message.AggregationId;

            foreach (var rabbitMqProducer in _producers)
            {
                try
                {
                    rabbitMqProducer.SendObject(message);
                }
                catch
                {
                    // Negate the failed sends.
                    //TODO: Later store this somewhere and return it back to the user.
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

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var message = JsonConvert.DeserializeObject<HealthMessage>(json);

            // Check if there is a message, with the needed cirteria and the right aggregationId
            if (message == null || message.ApplicationId == null || message.AggregationId != _aggregationId)
            {
                return;
            }

            if(_responses.ContainsKey(message.ApplicationId))
            {
                _responses[message.ApplicationId] = message.Health;
                _responsesReceived++;
            }

            // If we don't have all responses yet, return.
            if (_responses.Count != _producers.Count)
            {
                return;
            }
            _timer.Stop();

            EndingAction.Invoke(_responses);
        }
    }
}
