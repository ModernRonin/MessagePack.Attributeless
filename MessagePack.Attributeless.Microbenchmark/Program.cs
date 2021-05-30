using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bogus;
using JsonSubTypes;
using MessagePack.Resolvers;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;
using Newtonsoft.Json;
using static MessagePack.Attributeless.Microbenchmark.Logger;

namespace MessagePack.Attributeless.Microbenchmark
{
    class Program
    {
        const int Seed = 2326;

        static int Main(string[] args)
        {
#if DEBUG
            Logger.Warning(
                "You are running a DEBUG configuration - performance results in DEBUG may be misleading.");
#endif
            var parser = ParserFactory.Create("MessagePack.Attributeless Microbenchmark",
                "size and speed comparison of Attributeless vs Fully Attributed, Contractless and Typeless");
            var benchmark = parser.AddVerb<BenchmarkConfiguration>()
                .WithHelp("run benchmark")
                .Rename("benchmark");
            benchmark.Parameter(b => b.NumberOfRecords)
                .WithHelp("size of the array of complex objects to be used");
            benchmark.Parameter(b => b.Repetitions).WithHelp("how often to repeat the run");
            benchmark.Parameter(b => b.DontIncludeOthermethods)
                .WithLongName("only-attributeless")
                .WithShortName("oa")
                .WithHelp("don't benchmark other methods, only attributeless");
            benchmark.Parameter(b => b.DoIncludeJson)
                .WithShortName("j")
                .WithLongName("with-json")
                .WithHelp("run the benchmark with JSON, too, as a reference");
            var profiling = parser.AddVerb<Profile>()
                .WithHelp("run only attributeless with 100 records and 100 repetitions for profiling");
            profiling.Parameter(p => p.DontPromptForProfiler)
                .WithLongName("no-prompt")
                .WithShortName("np")
                .WithHelp("don't prompt you to attach the profiler");
            switch (parser.Parse(args))
            {
                case HelpResult help:
                    Console.WriteLine(help.Text);
                    return help.IsResultOfInvalidInput ? -1 : 0;
                case BenchmarkConfiguration options:
                    Run(options);
                    return 0;
                case Profile:
                    RunProfile(100, 100);
                    return 0;
            }

            return 0;
        }

        static void Run(BenchmarkConfiguration options)
        {
            var methods = new List<Func<int, int, Result>> {RunAttributeless};
            if (!options.DontIncludeOthermethods)
            {
                methods.Add(RunFullyAttributed);
                methods.Add(RunContractless);
                methods.Add(RunTypeless);
            }

            if (options.DoIncludeJson) methods.Add(RunJson);

            foreach (var method in methods)
            {
                method.Invoke(options.Repetitions, options.NumberOfRecords);
                Log("--------------------------------------------------------");
            }

            if (Debugger.IsAttached)
            {
                Log("Press <Enter> to exit...");
                Console.ReadLine();
            }
        }

        static Result RunAttributeless(int repetitions, int size) =>
            RunMethod(repetitions, size, "Attributeless", AttributelessSamples.Create, MessagePackSerializer
                .DefaultOptions.Configure()
                .GraphOf<AttributelessSamples.PersonWithPet>()
                .Build());

        static Result RunContractless(int repetitions, int size) =>
            RunMethod(repetitions, size, "Contractless", ContractlessSamples.Create,
                ContractlessStandardResolver.Options);

        static Result RunFullyAttributed(int repetitions, int size) =>
            RunMethod(repetitions, size, "Fully attributed", FullyAttributedSamples.Create,
                MessagePackSerializer.DefaultOptions);

        static Result RunJson(int repetitions, int size)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(JsonSubtypesConverterBuilder
                .Of<AttributelessSamples.IExtremity>("extremity")
                .RegisterSubtype<AttributelessSamples.Arm>("arm")
                .RegisterSubtype<AttributelessSamples.Leg>("leg")
                .RegisterSubtype<AttributelessSamples.Wing>("wing")
                .SerializeDiscriminatorProperty()
                .Build());
            settings.Converters.Add(JsonSubtypesConverterBuilder.Of<AttributelessSamples.IAnimal>("animal")
                .RegisterSubtype<AttributelessSamples.Mammal>("mammal")
                .RegisterSubtype<AttributelessSamples.Bird>("bird")
                .SerializeDiscriminatorProperty()
                .Build());
            Log($"Creating {size} input records");
            Randomizer.Seed = new Random(Seed);
            var input = AttributelessSamples.Create(size);

            Log("Running json...");
            Log("Warming up method...");
            var serialized = JsonConvert.SerializeObject(input, settings);
            long serializedSize = serialized.Length;
            JsonConvert.DeserializeObject<AttributelessSamples.PersonWithPet[]>(serialized, settings);

            Log($"Running {repetitions} serializations...");
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < repetitions; ++i) JsonConvert.SerializeObject(input, settings);

            watch.Stop();
            var serializeDuration = watch.Elapsed;

            Log($"Running {repetitions} deserializations...");
            watch.Restart();
            JsonConvert.SerializeObject(input, settings);
            for (var i = 0; i < repetitions; ++i)
            {
                var deserialized =
                    JsonConvert.DeserializeObject<AttributelessSamples.PersonWithPet[]>(serialized, settings);
            }

            watch.Stop();
            var deserializeDuration = watch.Elapsed;

            Log(
                $"Finished method JSON with a size of {serializedSize}, serialize-duration of {serializeDuration.TotalMilliseconds:0.}ms and deserialize-duration of {deserializeDuration.TotalMilliseconds:0.}ms for {repetitions} repetitions");
            return new Result("JSON", serializedSize, serializeDuration, deserializeDuration, repetitions);
        }

        static Result RunMethod<T>(int repetitions,
            int size,
            string name,
            Func<int, T[]> producer,
            MessagePackSerializerOptions options)
        {
            Log($"Method {name}");
            Log($"Creating {size} input records");
            Randomizer.Seed = new Random(Seed);
            var input = producer(size);
            return new Benchmark<T[]>(name, options, input).Run(repetitions);
        }

        static void RunProfile(int repetitions, int size)
        {
            var options = MessagePackSerializer
                .DefaultOptions.Configure()
                .GraphOf<AttributelessSamples.PersonWithPet>()
                .Build();
            Log($"Creating {size} input records");
            Randomizer.Seed = new Random(Seed);
            var input = AttributelessSamples.Create(size);
            Warning("Attach the profiler and press <Enter>");
            Console.ReadLine();
            new Benchmark<AttributelessSamples.PersonWithPet[]>("Attributeless", options, input).Run(
                repetitions);
            Log("Exiting...");
        }

        static Result RunTypeless(int repetitions, int size) =>
            RunMethod(repetitions, size, "Typeless", AttributelessSamples.Create,
                MessagePackSerializer.Typeless.DefaultOptions);
    }
}