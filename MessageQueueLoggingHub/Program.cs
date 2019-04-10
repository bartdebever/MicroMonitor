using System;
using MicroMonitor.MessageQueueUtils;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace MicroMonitor.MessageQueueLoggingHub
{
    public class Program
    {
        private static AuthenticationFlow _authenticationFlow;

        /// <summary>
        /// Main method ran when executing the program.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            _authenticationFlow = new AuthenticationFlow();
            CreateLogger(false);

            Log.Debug("Starting new Receiver for queue: {0}", StaticQueues.LoggingQueue);
            var rabbitMqReceiver = new RabbitMqReceiver();
            rabbitMqReceiver.Connect();
            rabbitMqReceiver.BindQueue(StaticQueues.LoggingQueue);
            rabbitMqReceiver.DeclareReceived(ConsumerOnReceived);

            Log.Debug("Running RabbitMq Logger");
            rabbitMqReceiver.Run();
        }

        /// <summary>
        /// Method executed when the consumer receives a message.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments containing the message.</param>
        private static void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);
            // Check if the headers are not null and contain the token.
            if (e.BasicProperties.Headers == null || !e.BasicProperties.Headers.ContainsKey("token"))
            {
                Log.Warning("Request without headers");
                return;
            }

            // Extract the token, convert it to a byte array and read it.
            var token = e.BasicProperties.Headers["token"];

            if (!(token is byte[] bytes))
            {
                return;
            }

            var stringToken = Encoding.UTF8.GetString(bytes);

            // Continue to the authentication flow section.
            _authenticationFlow.CheckAuthentication(message, stringToken);

        }

        /// <summary>
        /// Creates a new Serilog logger.
        /// </summary>
        private static void CreateLogger(bool useElasticSearch)
        {
            var elasticOptions = new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                MinimumLogEventLevel = LogEventLevel.Information
            };

            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console();

            if (useElasticSearch)
            {
                config = config.WriteTo.Elasticsearch(elasticOptions);
            }

            Log.Logger = config.CreateLogger();

        }
    }
}
