using System;

namespace MessagePack.Attributeless.Microbenchmark
{
    record Result(string Name,
        long Size,
        TimeSpan SerializeDuration,
        TimeSpan DeserializeDuration,
        int Repetitions);
}