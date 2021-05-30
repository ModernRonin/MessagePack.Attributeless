using System;
using System.Diagnostics;
using MessagePack.Resolvers;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;

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
                RunTypeless,
                RunAttributeless
            };
            foreach (var method in methods)
            {
                method.Invoke(options.Repetitions, options.NumberOfRecords);
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

        static Result RunTypeless(int repetitions, int size) =>
            RunMethod(repetitions, size, "Typeless", AttributelessSamples.Create,
                MessagePackSerializer.Typeless.DefaultOptions);
    }
}