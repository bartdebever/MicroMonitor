using RabbitMQ.Client;

namespace MicroMonitor.MessageQueueUtils
{
    public abstract class RabbitMqConnectionProducer
    {
        private readonly ConnectionFactory _connectionFactory;

        protected IModel Channel;

        protected IConnection Connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqReceiver"/> class.
        /// </summary>
        /// <param name="userName">The username used to connect to the server.</param>
        /// <param name="password">The password used to connect to the server.</param>
        /// <param name="hostName">The hostname of the server.</param>
        /// <param name="virtualHost">The virtualhost of the server.</param>
        protected RabbitMqConnectionProducer(string userName, string password, string hostName, string virtualHost = "/")
        {
            _connectionFactory = new ConnectionFactory
                                    {
                                        UserName = userName,
                                        Password = password,
                                        HostName = hostName,
                                        VirtualHost = virtualHost
                                    };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqReceiver" /> class.
        /// Uses the default localhost credentials of RabbitMQ.
        /// </summary>
        protected RabbitMqConnectionProducer() : this("guest", "guest", "localhost")
        {
        }

        /// <summary>
        /// Opens the connection to the provided RabbitMQ Server.
        /// </summary>
        public void Connect()
        {
            Connection = _connectionFactory.CreateConnection();
            Channel = Connection.CreateModel();
        }

        /// <summary>
        /// Disconnects from the RabbitMQ Server.
        /// </summary>
        public void Disconnect()
        {
            if (Channel == null || Connection == null)
            {
                return;
            }

            Channel.Close();
            Connection.Close();
        }
    }
}
