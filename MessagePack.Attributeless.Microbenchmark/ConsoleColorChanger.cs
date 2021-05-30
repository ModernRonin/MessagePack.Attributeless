using System;

namespace MessagePack.Attributeless.Microbenchmark
{
    class ConsoleColorChanger : IDisposable
    {
        readonly ConsoleColor _oldColor;

        public ConsoleColorChanger(ConsoleColor newColor) =>
            (_oldColor, Console.ForegroundColor) = (Console.ForegroundColor, newColor);

        public void Dispose() => Console.ForegroundColor = _oldColor;
    }
}