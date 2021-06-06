using System;
using System.IO;
using FluentAssertions;
using MessagePack.Attributeless.Formatters;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.Formatters
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

        [Test]
        public void Deserialize_throws_if_it_encounters_an_unknown_key()
        {
            var originalFormatter = new SubTypeFormatter<Samples.IExtremity>();
            originalFormatter.RegisterSubType<Samples.Arm>(0);
            originalFormatter.RegisterSubType<Samples.Leg>(1);
            originalFormatter.RegisterSubType<Samples.Wing>(2);
            var options = makeOptions(originalFormatter);

            using var stream = new MemoryStream();
            MessagePackSerializer.Serialize<Samples.IExtremity>(stream, new Samples.Leg(), options);
            stream.Position = 0;

            var changedFormatter = new SubTypeFormatter<Samples.IExtremity>();
            changedFormatter.RegisterSubType<Samples.Arm>(0);
            changedFormatter.RegisterSubType<Samples.Leg>(10);
            changedFormatter.RegisterSubType<Samples.Wing>(20);
            options = makeOptions(changedFormatter);

            Action action = () => MessagePackSerializer.Deserialize<Samples.IExtremity>(stream, options);

            action.Should()
                .ThrowExactly<MessagePackSerializationException>()
                .WithMessage(
                    "Failed to deserialize MessagePack.Attributeless.Tests.Samples+IExtremity value.")
                .WithInnerException<MessagePackSerializationException>()
                .WithMessage(
                    "Encountered unknown type key 1 for IExtremity - was this serialized with a differrent configuration?");

            MessagePackSerializerOptions makeOptions(SubTypeFormatter<Samples.IExtremity> formatter)
            {
                return MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[] {formatter},
                        new[] {ContractlessStandardResolver.Instance}));
            }
        }

        [Test]
        public void Serialize_throws_if_it_encounters_an_unmapped_subtype()
        {
            var formatter = new SubTypeFormatter<Samples.IExtremity>();
            formatter.RegisterSubType<Samples.Arm>(0);
            formatter.RegisterSubType<Samples.Leg>(1);
            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[] {formatter},
                        new[] {ContractlessStandardResolver.Instance}));

            using var stream = new MemoryStream();
            Action action = () =>
                MessagePackSerializer.Serialize<Samples.IExtremity>(new Samples.Wing(), options);

            action.Should()
                .ThrowExactly<MessagePackSerializationException>()
                .WithMessage("Failed to serialize MessagePack.Attributeless.Tests.Samples+IExtremity value.")
                .WithInnerException<MessagePackSerializationException>()
                .WithMessage("Missing configuration for subtype Wing of IExtremity");
        }
    }
}