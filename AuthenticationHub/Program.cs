using System;

using MessageQueueUtils;
using MessageQueueUtils.Messages;

namespace AuthenticationHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rabbitMqProducer = new RabbitMqProducer();
            rabbitMqProducer.Connect();
            rabbitMqProducer.DeclareQueue("MM_Log");
            Console.WriteLine("Producer");
            while (true)
            {
                var message = Console.ReadLine();

                if (message == "q")
                {
                    break;
                }

                var payload = new LoggingMessage();
                payload.Sender = "Bort";
                payload.Group = "FightCore";
                payload.Body = message;

                rabbitMqProducer.SendObject(payload);
            }

            rabbitMqProducer.Disconnect();
            Console.WriteLine("Hello World!");
        }
    }
}
