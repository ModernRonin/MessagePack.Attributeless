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
            Log("Warming up method..."); // to make sure whatever caches etc are initialized
            using (var stream = new MemoryStream())
            {
                MessagePackSerializer.Serialize(stream, _input, _options);
                serializedSize = stream.Length;
                stream.Position = 0;
                MessagePackSerializer.Deserialize<T>(stream, _options);
            }

            Log($"Running {repetitions} serializations...");
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < repetitions; ++i)
            {
                using var stream = new MemoryStream();
                MessagePackSerializer.Serialize(stream, _input, _options);
            }

            watch.Stop();
            var serializeDuration = watch.Elapsed;

            Log($"Running {repetitions} deserializations...");
            watch.Restart();
            using (var stream = new MemoryStream())
            {
                MessagePackSerializer.Serialize(stream, _input, _options);
                for (var i = 0; i < repetitions; ++i)
                {
                    stream.Position = 0;
                    var deserialized = MessagePackSerializer.Deserialize<T>(stream, _options);
                }
            }

            watch.Stop();
            var deserializeDuration = watch.Elapsed;

            Log(
                $"Finished method {_name} with a size of {serializedSize}, serialize-duration of {serializeDuration.TotalMilliseconds:0.}ms and deserialize-duration of {deserializeDuration.TotalMilliseconds:0.}ms for {repetitions} repetitions");
            return new Result(_name, serializedSize, serializeDuration, deserializeDuration, repetitions);
        }
    }
}