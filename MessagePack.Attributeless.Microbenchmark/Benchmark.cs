using System;
using System.Diagnostics;
using System.IO;
using static MessagePack.Attributeless.Microbenchmark.Logger;

namespace MessagePack.Attributeless.Microbenchmark
{
    class Benchmark<T>
    {
        readonly T _input;
        readonly string _name;
        readonly MessagePackSerializerOptions _options;

        public Benchmark(string name, MessagePackSerializerOptions options, T input)
        {
            _name = name;
            _options = options;
            _input = input;
        }

        public Result Run(int repetitions)
        {
            Log($"Running {_name}...");
            long serializedSize;
            using (var stream = new MemoryStream())
            {
                MessagePackSerializer.Serialize(stream, _input, _options);
                serializedSize = stream.Length;
                stream.Position = 0;
                var roundTripped = MessagePackSerializer.Deserialize<T>(stream, _options);
                // the following is just to ensure that the optimizer cannot
                // throw away the method call; actually, if we compared
                // roundtripped to the input, it would not match
                // unless we added native datetime resolvers, but this is
                // not relevant for the benchmark so we leave it out
                if (roundTripped == null)
                {
                    Error("Roundtrip not successful");
                    return new Result(_name, -1, TimeSpan.Zero, -1);
                }
            }

            Log($"Running {repetitions} iterations...");
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < repetitions; ++i)
            {
                using (var stream = new MemoryStream())
                {
                    MessagePackSerializer.Serialize(stream, _input, _options);
                    stream.Position = 0;
                    MessagePackSerializer.Deserialize<T>(stream, _options);
                }
            }

            watch.Stop();
            Log(
                $"Finished method {_name} with a size of {serializedSize} and duration of {watch.ElapsedMilliseconds:0.}ms for {repetitions} repetitions");
            return new Result(_name, serializedSize, watch.Elapsed, repetitions);
        }
    }
}