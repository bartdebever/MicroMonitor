using MicroMonitor.MessageQueueUtils;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace MicroMonitor.MessageQueueLoggingHub
{
    class Program
    {
        private static AuthenticationFlow _authenticationFlow;

        static void Main(string[] args)
        {
            _authenticationFlow = new AuthenticationFlow();
            CreateLogger();

            Log.Debug("Starting new Receiver for queue: {0}", StaticQueues.LoggingQueue);
            var rabbitMqReceiver = new RabbitMqReceiver();
            rabbitMqReceiver.Connect();
            rabbitMqReceiver.BindQueue(StaticQueues.LoggingQueue);
            rabbitMqReceiver.DeclareReceived(ConsumerOnReceived);

            Log.Debug("Running RabbitMq Logger");
            rabbitMqReceiver.Run();
        }

        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);
            if (e.BasicProperties.Headers == null || !e.BasicProperties.Headers.ContainsKey("token"))
            {
                Log.Warning("Request without headers");
                return;
            }

            var token = e.BasicProperties.Headers["token"];

            if (!(token is byte[] bytes))
            {
                return;
            }

            var stringToken = Encoding.UTF8.GetString(bytes);

            _authenticationFlow.CheckAuthentication(message, stringToken);

        }

        /// <summary>
        /// Creates a new Serilog logger.
        /// </summary>
        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                //TODO Add ElasticSearch with Info level.
                .WriteTo.Console().CreateLogger();
        }
    }
}
