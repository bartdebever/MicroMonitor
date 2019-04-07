using System;

using MicroMonitor.MessageQueueUtils;
using MicroMonitor.MessageQueueUtils.Messages;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var rabbitMqProducer = new RabbitMqProducer();
            rabbitMqProducer.Connect();
            rabbitMqProducer.BindQueue("MM_Log");
            Console.WriteLine("Producer");
            while (true)
            {
                var message = Console.ReadLine();

                if (message == "q")
                {
                    break;
                }

                var payload = new LoggingMessage { Sender = "Bort", Group = "FightCore", Body = message };

                rabbitMqProducer.SendObject(payload);
            }

            rabbitMqProducer.Disconnect();
            Console.WriteLine("Hello World!");
        }
    }
}
