using System;
using System.Collections;
using System.Linq;
using ApprovalTests;
using FluentAssertions;
using MessagePack.Formatters;
using MoreLinq;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class MessagePackSerializerOptionsBuilderTests
    {
        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_with_completely_manual_configuration_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .AutoKeyed<Samples.Address>()
                .AutoKeyed<Samples.Person>()
                .AddNativeFormatters()
                .SubType<Samples.IExtremity, Samples.Arm>()
                .SubType<Samples.IExtremity, Samples.Leg>()
                .SubType<Samples.IExtremity, Samples.Wing>()
                .SubType<Samples.IAnimal, Samples.Mammal>()
                .SubType<Samples.IAnimal, Samples.Bird>()
                .AutoKeyed<Samples.PersonWithPet>()
                .Build();

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_with_GraphOf_configuration_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Build();

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void GraphOf_configuration_ignoring_a_type_altogether_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Ignore<Samples.Bird>()
                .Build();

            var output = options.Roundtrip(input);

            if (input.Pet is Samples.Bird) input.Pet = default;
            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void GraphOf_configuration_ignoring_a_type_altogether_even_if_nested_with(
            Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Ignore<Samples.Leg>()
                .Build();

            var output = options.Roundtrip(input);

            input.Pet.Extremities = input.Pet.Extremities.Where(e => e is not Samples.Leg).ToArray();
            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

        class UppercasingStringFormatter : IMessagePackFormatter<string>
        {
            public string Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
                MessagePackSerializer.Deserialize<string>(ref reader, options);

            public void Serialize(ref MessagePackWriter writer,
                string value,
                MessagePackSerializerOptions options)
            {
                MessagePackSerializer.Serialize(typeof(string), ref writer, value.ToUpperInvariant(),
                    options);
            }
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void GraphOf_configuration_with_type_override_for_non_user_type_with(
            Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .OverrideFormatter<string, UppercasingStringFormatter>()
                .Build();

            var output = options.Roundtrip(input);

            setAllStringPropertiesToUppercase(input);
            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());

            void setAllStringPropertiesToUppercase(object obj)
            {
                if (obj is IEnumerable enumerable and not null)
                {
                    foreach (var element in enumerable) setAllStringPropertiesToUppercase(element);
                    return;
                }

                var type = obj.GetType();
                var (stringProperties, otherProperties) = type
                    .GetProperties()
                    .Where(p => p.CanWrite)
                    .Partition(p => p.PropertyType == typeof(string));
                stringProperties.ForEach(
                    p => p.SetValue(obj, p.GetValue(obj)?.ToString()?.ToUpperInvariant()));
                foreach (var p in otherProperties) setAllStringPropertiesToUppercase(p.GetValue(obj));
            }
        }

        class OverridingExtremityFormatter : IMessagePackFormatter<Samples.IExtremity>
        {
            public Samples.IExtremity Deserialize(ref MessagePackReader reader,
                MessagePackSerializerOptions options)
            {
                WasUsed = true;
                var (code, val, side) = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                Samples.IExtremity result = code switch
                {
                    0 => new Samples.Arm {NumberOfFingers = (byte) val},
                    1 => new Samples.Leg {NumberOfToes = (byte) val},
                    2 => new Samples.Wing {Span = val},
                    _ => throw new InvalidOperationException("couldn't deserialize")
                };
                result.Side = (Samples.Side) side;
                return result;
            }

            public void Serialize(ref MessagePackWriter writer,
                Samples.IExtremity value,
                MessagePackSerializerOptions options)
            {
                WasUsed = true;
                var (code, val) = value switch
                {
                    Samples.Arm arm => (0, arm.NumberOfFingers),
                    Samples.Leg leg => (1, (int) leg.NumberOfToes),
                    Samples.Wing wing => (2, wing.Span),
                    _ => throw new InvalidOperationException("couldn't serialize")
                };
                writer.Write(code);
                writer.Write(val);
                writer.Write((int) value.Side);
            }

            public static bool WasUsed { get; set; }
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void GraphOf_configuration_with_type_override_for_user_type_with(
            Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .OverrideFormatter<Samples.IExtremity, OverridingExtremityFormatter>()
                .Build();

            OverridingExtremityFormatter.WasUsed = false;

            options.TestRoundtrip(input);
            OverridingExtremityFormatter.WasUsed.Should().BeTrue();
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_ignoring_individual_properties_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Ignore<Samples.Address, string>(a => a.City)
                .Ignore<Samples.Address, string>(a => a.Country)
                .Build();

            var output = options.Roundtrip(input);

            input.Human.Addresses.ForEach(a =>
            {
                a.City = default;
                a.Country = default;
            });
            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_ignoring_properties_by_predicate_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Ignore<Samples.Address>(pi => pi.Name.StartsWith("C"))
                .Build();

            var output = options.Roundtrip(input);

            input.Human.Addresses.ForEach(a =>
            {
                a.City = default;
                a.Country = default;
            });
            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_with_AllSubTypesOf_configuration_with(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .AllSubTypesOf<Samples.IExtremity>()
                .AllSubTypesOf<Samples.IAnimal>()
                .AutoKeyed<Samples.PersonWithPet>()
                .AutoKeyed<Samples.Address>()
                .AutoKeyed<Samples.Person>()
                .AddNativeFormatters()
                .Build();

            options.TestRoundtrip(input);
        }

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_with_compression(Samples.PersonWithPet input)
        {
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression
                    .Lz4BlockArray)
                .Configure()
                .GraphOf<Samples.PersonWithPet>()
                .AddNativeFormatters()
                .Build();

            options.TestRoundtrip(input);
        }

        public interface IInterface
        {
            string Name { get; set; }
        }

        public class Implementation : IInterface
        {
            public string Name { get; set; }
        }

        public class ContainsInterfaceProperty
        {
            public int Count { get; set; }
            public IInterface Prop { get; set; }
        }

        [Test]
        public void Checksum_is_LIKELY_to_be_different_for_a_modified_configuration()
        {
            var oldVersion = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .Validation.Checksum;
            // the same, but leaving out Arm
            var newVersion = MessagePackSerializer.DefaultOptions.Configure()
                .AutoKeyed<Samples.Address>()
                .AutoKeyed<Samples.Person>()
                .AddNativeFormatters()
                .SubType<Samples.IExtremity, Samples.Leg>()
                .SubType<Samples.IExtremity, Samples.Wing>()
                .SubType<Samples.IAnimal, Samples.Mammal>()
                .SubType<Samples.IAnimal, Samples.Bird>()
                .AutoKeyed<Samples.PersonWithPet>()
                .Validation.Checksum;

            newVersion.Should().NotEqual(oldVersion);
        }

        [Test]
        public void Checksum_is_the_same_for_the_same_configuration()
        {
            var oldVersion = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .Validation.Checksum;
            var newVersion = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.PersonWithPet>()
                .Validation.Checksum;

            newVersion.Should().Equal(oldVersion);
        }

        [Test]
        public void KeyTable()
        {
            var builder = MessagePackSerializer.DefaultOptions.Configure()
                .AutoKeyed<Samples.Address>()
                .AutoKeyed<Samples.Person>()
                .AddNativeFormatters()
                .SubType<Samples.IExtremity, Samples.Arm>()
                .SubType<Samples.IExtremity, Samples.Leg>()
                .SubType<Samples.IExtremity, Samples.Wing>()
                .SubType<Samples.IAnimal, Samples.Mammal>()
                .SubType<Samples.IAnimal, Samples.Bird>()
                .AutoKeyed<Samples.PersonWithPet>();
            var keytable = builder.Validation.KeyTable;
            // not Environment.NewLine to prevent issues between the platform where the approved file was saved being different from the one the test is executed on 
            var asText = string.Join('\n', keytable);
            Approvals.Verify(asText);
        }

        [Test]
        public void Roundtrip_if_an_interface_property_is_null()
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<ContainsInterfaceProperty>()
                .Build();

            var input = new ContainsInterfaceProperty {Count = 13};
            options.TestRoundtrip(input);
        }

        [Test]
        public void Roundtrip_with_GraphOf_configuration_with_abstract_root()
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.AnAnimal>()
                .Build();

            var input = Samples.AnimalCases.Cast<Samples.AnAnimal>().First();
            options.TestRoundtrip(input);
        }

        [Test]
        public void Roundtrip_with_GraphOf_configuration_with_concrete_root()
        {
            var options = MessagePackSerializer.DefaultOptions.Configure()
                .GraphOf<Samples.Person>()
                .AddNativeFormatters()
                .Build();

            var input = Samples.MakePerson();
            options.TestRoundtrip(input);
        }

        [Test]
        public void Roundtrip_with_GraphOf_configuration_with_interface_root()
        {
            var options = MessagePackSerializer.DefaultOptions.Configure().GraphOf<Samples.IAnimal>().Build();

            var input = Samples.AnimalCases.First();
            options.TestRoundtrip(input);
        }
    }
}