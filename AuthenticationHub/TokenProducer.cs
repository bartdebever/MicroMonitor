using Bogus;

namespace MicroMonitor.AuthenticationHub
{
    /// <summary>
    /// Simple helper class to produce random tokens.
    /// </summary>
    public static class TokenProducer
    {
        private static Faker _faker = new Faker();

        private const int TOKEN_LENGTH = 20;
        
        /// <summary>
        /// Creates a random token of a given length.
        /// </summary>
        /// <returns>A random string token.</returns>
        public static string ProduceToken()
        {
            return _faker.Random.AlphaNumeric(TOKEN_LENGTH);
        }
    }
}
