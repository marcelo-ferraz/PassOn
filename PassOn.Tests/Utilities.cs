namespace PassOn.Tests
{
    internal class Utilities
    {
        private static Random random = new Random();

        public static string RandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable
                .Range(1, length)
                .Select(_ => chars[random.Next(chars.Length)]
            ).ToArray());
        }

        public static int NextRandomInt(int min = 0, int max = 100)
        {
            return random.Next(min, max);
        }
    }
}
