using Bogus;

namespace MicroMonitor.AuthenticationHub
{
    public class TokenProducer
    {
        private static Faker _faker = new Faker();

        private const int TOKEN_LENGTH = 20;

        public static string ProduceToken()
        {
            return _faker.Random.AlphaNumeric(TOKEN_LENGTH);
        }
    }
}
