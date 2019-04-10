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
using System.Threading;

namespace TestProject
{
    class Program
    {
        private const string IV = "JYFrNePrBqFm6MEL";
        private const string KEY = "kf9C224Knj3R3n8VVwJ8lI3QWUQJ1Exy";

        private const string APPLICATIONID = "TestApplication";
        private const string GROUPID = "FightCore";

        private static RabbitMqProducer authProducer;

        private static RabbitMqReceiver authReceiver;

        private static RabbitMqProducer healthCheckProducer;

        private static Thread healthTread;

        static void Main(string[] args)
        {
            Console.WriteLine("Test Client");
            Console.WriteLine("Waiting for services to start...\nPress enter when ready.");
            Console.ReadLine();

            //SetupHealthCheck();

            healthTread = new Thread(SetupHealthCheck);

            authProducer = new RabbitMqProducer();
            authProducer.Connect();
            authProducer.BindQueue(StaticQueues.RequestAuth);

            authReceiver = new RabbitMqReceiver();
            authReceiver.Connect();
            authReceiver.BindQueue(APPLICATIONID);
            authReceiver.DeclareReceived(OnAuthReceived);
            authReceiver.Run();

            var service = new Service();
            service.ApplicationId = APPLICATIONID;
            service.GroupId = GROUPID;

            var json = JsonConvert.SerializeObject(service);

            var encrypt = EncryptProvider.AESEncrypt(json, KEY, IV);

            authProducer.SendMessage(encrypt);
            Console.WriteLine("Waiting for response.");

        }

        private static void OnAuthReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var token = Encoding.UTF8.GetString(body);

            var rabbitMqProducer = new RabbitMqProducer();
            rabbitMqProducer.Connect();

            rabbitMqProducer.BindQueue("MM_Log");
            Console.WriteLine($"Gained token: {token}, Start writing messages.");
            healthTread.Start();
            while (true)
            {
                var message = Console.ReadLine();

                if (message == "q")
                {
                    break;
                }

                var payload = new LoggingMessage { Sender = APPLICATIONID, Group = GROUPID, Body = message };
                var json = JsonConvert.SerializeObject(payload);
                var basicProperties =
                    new BasicProperties { Headers = new Dictionary<string, object> { { "token", token } } };
                rabbitMqProducer.SendMessage(json, basicProperties);
            }

            rabbitMqProducer.Disconnect();
        }

        private static void SetupHealthCheck()
        {

            healthCheckProducer = new RabbitMqProducer();
            healthCheckProducer.Connect();
            healthCheckProducer.BindQueue(StaticQueues.HealthCheckReply);

            var healthQueue = new RabbitMqReceiver();
            healthQueue.Connect();
            healthQueue.BindQueue(APPLICATIONID);
            healthQueue.DeclareReceived(OnHealthCheckReceived);
            healthQueue.Run();
        }

        private static void OnHealthCheckReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var json = Encoding.UTF8.GetString(body);

            Console.WriteLine("Healthcheck");
            // Normally you want to check the message for the action.
            // Don't care for now as this is the only message that will come in.
            var aggregatedMessage = JsonConvert.DeserializeObject<AggregationMessage<string>>(json);

            var message = new HealthMessage()
            {
                ApplicationId = APPLICATIONID,
                Health = 100,
                AggregationId = aggregatedMessage.AggregationId
            };

            healthCheckProducer.SendObject(message);
        }
    }
}
