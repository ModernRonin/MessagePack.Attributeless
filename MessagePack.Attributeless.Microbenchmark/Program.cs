using System;
using System.Collections.Generic;
using System.Diagnostics;
using MessagePack.Resolvers;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;

namespace MessagePack.Attributeless.Microbenchmark
{
    class Program
    {
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
                    RunProfile();
                    return 0;
            }

            return 0;
        }

        static void Run(BenchmarkConfiguration benchmarkConfiguration)
        {
            var methods = new List<Func<int, int, Result>> {RunAttributeless};
            if (!benchmarkConfiguration.DontIncludeOthermethods)
            {
                methods.Add(RunFullyAttributed);
                methods.Add(RunContractless);
                methods.Add(RunTypeless);
            }

            foreach (var method in methods)
            {
                method.Invoke(benchmarkConfiguration.Repetitions, benchmarkConfiguration.NumberOfRecords);
                Logger.Log("--------------------------------------------------------");
            }

            if (Debugger.IsAttached)
            {
                Logger.Log("Press <Enter> to exit...");
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

        static Result RunMethod<T>(int repetitions,
            int size,
            string name,
            Func<int, T[]> producer,
            MessagePackSerializerOptions options)
        {
            Logger.Log($"Method {name}");
            Logger.Log($"Creating {size} input records");
            var input = producer(size);
            return new Benchmark<T[]>(name, options, input).Run(repetitions);
        }

        static void RunProfile()
        {
            var options = MessagePackSerializer
                .DefaultOptions.Configure()
                .GraphOf<AttributelessSamples.PersonWithPet>()
                .Build();
            Logger.Log($"Creating {100} input records");
            var input = AttributelessSamples.Create(100);
            Logger.Warning("Attach the profiler and press <Enter>");
            Console.ReadLine();
            new Benchmark<AttributelessSamples.PersonWithPet[]>("Attributeless", options, input).Run(100);
            Logger.Log("Exiting...");
        }

        static Result RunTypeless(int repetitions, int size) =>
            RunMethod(repetitions, size, "Typeless", AttributelessSamples.Create,
                MessagePackSerializer.Typeless.DefaultOptions);
    }
}