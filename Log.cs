namespace Tanks
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
            Console.Write($"[{time}]");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" ERROR   ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(msg);
        }
    }
}
