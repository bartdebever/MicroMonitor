using System;
using System.Collections.Generic;
using System.Text;

using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;
using MicroMonitor.MessageQueueUtils.Storage;
using Newtonsoft.Json;
using NETCore.Encrypt;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace TestProject
{
    class Program
    {
        private const string IV = "JYFrNePrBqFm6MEL";
        private const string KEY = "kf9C224Knj3R3n8VVwJ8lI3QWUQJ1Exy";

        private static RabbitMqProducer authProducer;

        private static RabbitMqReceiver authReceiver;

        static void Main(string[] args)
        {
            Console.WriteLine("Test Client");
            Console.WriteLine("Waiting for services to start...\nPress enter when ready.");
            Console.ReadLine();
            authProducer = new RabbitMqProducer();
            authProducer.Connect();
            authProducer.BindQueue(StaticQueues.RequestAuth);

            authReceiver = new RabbitMqReceiver();
            authReceiver.Connect();
            authReceiver.BindQueue("TestApplication");
            authReceiver.DeclareReceived(OnAuthReceived);
            authReceiver.Run();

            var service = new Service();
            service.ApplicationId = "TestApplication";
            service.GroupId = "FightCore";

            var json = JsonConvert.SerializeObject(service);

            var encrypt = EncryptProvider.AESEncrypt(json, KEY, IV);

            authProducer.SendMessage(encrypt);

        }

        private static void OnAuthReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var token = Encoding.UTF8.GetString(body);

            var rabbitMqProducer = new RabbitMqProducer();
            rabbitMqProducer.Connect();

            rabbitMqProducer.BindQueue("MM_Log");
            Console.WriteLine($"Gained token: {token}, Start writing messages.");
            while (true)
            {
                var message = Console.ReadLine();

                if (message == "q")
                {
                    break;
                }

                var payload = new LoggingMessage { Sender = "TestApplication", Group = "FightCore", Body = message };
                var json = JsonConvert.SerializeObject(payload);
                var basicProperties =
                    new BasicProperties { Headers = new Dictionary<string, object> { { "token", token } } };
                rabbitMqProducer.SendMessage(json, basicProperties);
            }

            rabbitMqProducer.Disconnect();
        }
    }
}
