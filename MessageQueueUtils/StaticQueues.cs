namespace MicroMonitor.MessageQueueUtils
{
    public static class StaticQueues
    {
        public const string LoggingQueue = "MM_Log";

        public const string RequestAuth = "MM_Auth_Get";

        public const string GetAuth = "MM_Auth_Token";

        public const string IsAuthenticated = "MM_Auth_Check";

        public const string IsAuthenticatedReply = "MM_Auth_Reply";
    }
}
