namespace MicroMonitor.MessageQueueUtils
{
    public static class StaticQueues
    {
        /// <summary>
        /// Queue used to log messages.
        /// Should only be used by outside services.
        /// </summary>
        public const string LoggingQueue = "MM_Log";

        /// <summary>
        /// Queue to request an authentication key.
        /// Should only be used by outside services.
        /// </summary>
        public const string RequestAuth = "MM_Auth_Get";

        /// <summary>
        /// Queue used to receive the authentication key.
        /// Should only be used by outside services.
        /// </summary>
        public const string GetAuth = "MM_Auth_Token";

        /// <summary>
        /// Queue used to check a token's authentication.
        /// Should only be used by inside services.
        /// </summary>
        public const string IsAuthenticated = "MM_Auth_Check";

        /// <summary>
        /// Queue to receive the reply about the token's validity.
        /// Should only be used by inside services.
        /// </summary>
        public const string IsAuthenticatedReply = "MM_Auth_Reply";

        /// <summary>
        /// The queue that will receive the health check results.
        /// </summary>
        public const string HealthCheckReply = "MM_Health";
    }
}
