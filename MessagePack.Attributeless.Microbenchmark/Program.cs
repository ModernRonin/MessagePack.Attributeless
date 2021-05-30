using System;
using MessagePack.Resolvers;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;

namespace MessagePack.Attributeless.Microbenchmark
{
    public class Options
    {
        public int NumberOfRecords { get; set; } = 1000;
        public int Repetitions { get; set; } = 100;
    }

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
            parser.DefaultVerb<Options>();
            switch (parser.Parse(args))
            {
                case HelpResult help:
                    Console.WriteLine(help.Text);
                    return help.IsResultOfInvalidInput ? -1 : 0;
                case Options options:
                    Run(options);
                    return 0;
            }

            return 0;
        }

        static void Run(Options options)
        {
            var methods = new Func<int, int, Result>[]
            {
                RunFullyAttributed,
                RunContractless,
                RunAttributeless
            };
            foreach (var method in methods)
            {
                method.Invoke(options.Repetitions, options.NumberOfRecords);
                Logger.Log("--------------------------------------------------------");
            }

            Logger.Log("Press <Enter> to exit...");
            Console.ReadLine();
        }

        static Result RunAttributeless(int repetitions, int size)
        {
            var name = "Attributeless";
            Logger.Log($"Method {name}");
            Logger.Log($"Creating {size} input records");
            Logger.Log("Setting up messagepack");
            var input = AttributelessSamples.Create(size);
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<AttributelessSamples.PersonWithPet>()
                .Build();
            return new Benchmark<AttributelessSamples.PersonWithPet[]>(name, options, input)
                .Run(repetitions);
        }

        static Result RunContractless(int repetitions, int size)
        {
            var name = "Contractless";
            Logger.Log($"Method {name}");
            Logger.Log($"Creating {size} input records");
            Logger.Log("Setting up messagepack");
            var input = ContractlessSamples.Create(size);
            var options = ContractlessStandardResolver.Options;
            return new Benchmark<ContractlessSamples.PersonWithPet[]>(name, options, input).Run(repetitions);
        }

        static Result RunFullyAttributed(int repetitions, int size)
        {
            var name = "Fully attributed";
            Logger.Log($"Method {name}");
            Logger.Log($"Creating {size} input records");
            Logger.Log("Setting up messagepack");
            var input = FullyAttributedSamples.Create(size);
            var options = MessagePackSerializer.DefaultOptions;
            return new Benchmark<FullyAttributedSamples.PersonWithPet[]>(name, options, input)
                .Run(repetitions);
        }
    }
}