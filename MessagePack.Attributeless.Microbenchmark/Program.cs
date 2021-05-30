using System;
using MessagePack.Resolvers;

namespace MessagePack.Attributeless.Microbenchmark
{
    class Program
    {
        static void Main()
        {
#if DEBUG
            Logger.Warning(
                "You are running a DEBUG configuration - performance results in DEBUG may be misleading.");
#endif
            var methods = new Func<int, int, Result>[]
            {
                RunFullyAttributed,
                RunContractless,
                RunAttributeless
            };
            const int repetitions = 10;
            const int size = 100;
            foreach (var method in methods)
            {
                method.Invoke(repetitions, size);
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