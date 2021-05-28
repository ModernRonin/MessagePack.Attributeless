using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    [TestFixture]
    public class SubTypeFormatterTests
    {
        [TestCaseSource(typeof(Samples), nameof(Samples.ExtremityCases))]
        public void Roundtrip_of_interface(Samples.IExtremity input)
        {
            var formatter = new SubTypeFormatter<Samples.IExtremity>();
            formatter.RegisterSubType<Samples.Arm>(0);
            formatter.RegisterSubType<Samples.Leg>(1);
            formatter.RegisterSubType<Samples.Wing>(2);
            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new[] {formatter}, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.AnimalCases))]
        public void Roundtrip_of_nested_hierarchy(Samples.IAnimal input)
        {
            var extremityFormatter = new SubTypeFormatter<Samples.IExtremity>();
            extremityFormatter.RegisterSubType<Samples.Arm>(0);
            extremityFormatter.RegisterSubType<Samples.Leg>(1);
            extremityFormatter.RegisterSubType<Samples.Wing>(2);

            var animalFormatter = new SubTypeFormatter<Samples.IAnimal>();
            animalFormatter.RegisterSubType<Samples.Mammal>(0);
            animalFormatter.RegisterSubType<Samples.Bird>(1);

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[]
                    {
                        extremityFormatter,
                        animalFormatter
                    }, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.AnimalCases))]
        public void Roundtrip_of_nested_hierarchy_with_automatic_keys(Samples.IAnimal input)
        {
            var extremityFormatter = new SubTypeFormatter<Samples.IExtremity>();
            extremityFormatter.RegisterSubType<Samples.Arm>();
            extremityFormatter.RegisterSubType<Samples.Leg>();
            extremityFormatter.RegisterSubType<Samples.Wing>();

            var animalFormatter = new SubTypeFormatter<Samples.IAnimal>();
            animalFormatter.RegisterSubType<Samples.Mammal>();
            animalFormatter.RegisterSubType<Samples.Bird>();

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[]
                    {
                        extremityFormatter,
                        animalFormatter
                    }, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.AnimalCases))]
        public void Roundtrip_with_builder_configuration(Samples.IAnimal input)
        {
            var options = ContractlessStandardResolver.Options.Configure()
                .SubType<Samples.IExtremity, Samples.Arm>()
                .SubType<Samples.IExtremity, Samples.Leg>()
                .SubType<Samples.IExtremity, Samples.Wing>()
                .SubType<Samples.IAnimal, Samples.Mammal>()
                .SubType<Samples.IAnimal, Samples.Bird>()
                .Build();

            options.TestRoundtrip(input);
        }

        // TODO: test with compression, look at MessagePackSerializer.cs:223
    }
}