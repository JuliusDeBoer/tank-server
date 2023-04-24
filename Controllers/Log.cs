namespace TankServer.Controllers
{
    public static class Log
    {
        private const string dateFomrat = "HH:MM:ss:ff";

        public static void Info(string msg)
        {
            string time = DateTime.Now.ToString(dateFomrat);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[{time}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" INFO    ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(msg);
        }

        public static void Warn(string msg)
        {
            string time = DateTime.Now.ToString(dateFomrat);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[{time}]");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" WARNING ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(msg);
        }

        public static void Error(string msg)
        {
            string time = DateTime.Now.ToString(dateFomrat);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Error.Write($"[{time}]");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.Write(" ERROR   ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Error.WriteLine(msg);
        }

        public static void Info(Response response)
        {
            Info($"({response.Code}) {response.Message}");
        }

        public static void Warn(Response response)
        {
            Warn($"({response.Code}) {response.Message}");
        }

        public static void Error(Response response)
        {
            Error($"({response.Code}) {response.Message}");
        }
    }
}
