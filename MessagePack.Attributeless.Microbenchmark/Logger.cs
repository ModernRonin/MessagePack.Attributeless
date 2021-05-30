using System;

namespace MessagePack.Attributeless.Microbenchmark
{
    static class Logger
    {
        public static void Error(string line)
        {
            using var color = new ConsoleColorChanger(ConsoleColor.DarkRed);
            Log(line);
        }

        public static void Log(string line) => Console.WriteLine(line);

        public static void Warning(string line)
        {
            using var color = new ConsoleColorChanger(ConsoleColor.DarkYellow);
            Log(line);
        }
    }
}