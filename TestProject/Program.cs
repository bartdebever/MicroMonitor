using System;
using System.Collections.Generic;
using System.Text;

using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;

using Newtonsoft.Json;

using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace TestProject
{
    class Program
    {
        private static RabbitMqProducer authProducer;

        private static RabbitMqReceiver authReceiver;

        static void Main(string[] args)
        {
            Console.WriteLine("Producer");
            Console.WriteLine("Waiting...");
            Console.ReadLine();
            authProducer = new RabbitMqProducer();
            authProducer.Connect();
            authProducer.BindQueue(StaticQueues.RequestAuth);

            authReceiver = new RabbitMqReceiver();
            authReceiver.Connect();
            authReceiver.BindQueue(StaticQueues.GetAuth);
            authReceiver.DeclareReceived(OnAuthReceived);
            authReceiver.Run();
            authProducer.SendMessage("Test");

        }

        private static void OnAuthReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var token = Encoding.UTF8.GetString(body);

            var rabbitMqProducer = new RabbitMqProducer();
            rabbitMqProducer.Connect();

            rabbitMqProducer.BindQueue("MM_Log");
            Console.WriteLine("Gained token");
            while (true)
            {
                var message = Console.ReadLine();

                if (message == "q")
                {
                    break;
                }

                var payload = new LoggingMessage { Sender = "Bort", Group = "FightCore", Body = message };
                var json = JsonConvert.SerializeObject(payload);
                var basicProperties =
                    new BasicProperties { Headers = new Dictionary<string, object> { { "token", token } } };
                rabbitMqProducer.SendMessage(json, basicProperties);
            }

            rabbitMqProducer.Disconnect();
        }
    }
}
