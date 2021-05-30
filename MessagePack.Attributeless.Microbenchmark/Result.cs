using System;

namespace MessagePack.Attributeless.Microbenchmark
{
    record Result(string Name, long Size, TimeSpan Duration, int Repetitions);
}