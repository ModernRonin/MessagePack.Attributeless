using AutoBogus;
using FluentAssertions;
using MessagePack.Attributeless.Formatters;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.Formatters;

[TestFixture]
public class ConfigurableKeyFormatterTests
{
    public class Something
    {
        public int Count { get; set; }
        public string Text { get; set; }
        public Samples.Person Who { get; set; }
    }

    static IEnumerable<TestCaseData> ContainsNullableCases
    {
        get
        {
            yield return new TestCaseData(new ContainsNullable()).SetName("{m} when all are null");
            yield return new TestCaseData(new ContainsNullable { Count = 13 }).SetName(
                "{m} when all non-user-defined is null");
            yield return new TestCaseData(new ContainsNullable { Side = Samples.Side.Right }).SetName(
                "{m} when all user-defined is null");
        }
    }

    [TestCaseSource(nameof(ContainsNullableCases))]
    public void Roundtrip_on_type_with_nullable_primitive_properties(ContainsNullable input)
    {
        var formatter = new ConfigurableKeyFormatter<ContainsNullable>();
        formatter.UseAutomaticKeys();

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new[] { formatter }, new[] { ContractlessStandardResolver.Instance }));

        options.TestRoundtrip(input);
    }

    public class ContainsNullable
    {
        public int? Count { get; set; }
        public Samples.Side? Side { get; set; }
    }

    public class Inside
    {
        public int Number { get; set; }
    }

    public class Outside
    {
        public Inside Nested { get; set; }
        public string Text { get; set; }
    }

    public class TypeWithIndexerProperties
    {
        readonly Dictionary<int, Inside> _numbers = new();

        public Inside this[int index]
        {
            get => _numbers[index];
            set => _numbers[index] = value;
        }

        public int SomeNumber { get; set; }
    }

    [Test]
    public void Deserialize_throws_if_it_encounters_an_unknown_key()
    {
        var formatter = new ConfigurableKeyFormatter<Samples.Address>();
        formatter.SetKeyFor(0, a => a.City);
        formatter.SetKeyFor(1, a => a.Country);
        formatter.SetKeyFor(2, a => a.ZipCode);
        formatter.SetKeyFor(3, a => a.StreetAddress);
        var options = makeOptions(formatter);

        using var stream = new MemoryStream();
        MessagePackSerializer.Serialize(stream, AutoFaker.Generate<Samples.Address>(), options);
        stream.Position = 0;

        formatter.SetKeyFor(7, a => a.Country);

        Action action = () => MessagePackSerializer.Deserialize<Samples.Address>(stream, options);

        action.Should()
            .ThrowExactly<MessagePackSerializationException>()
            .WithMessage("Failed to deserialize MessagePack.Attributeless.Tests.Samples+Address value.")
            .WithInnerException<MessagePackSerializationException>()
            .WithMessage(
                "Encountered unknown property key 1 for Address - was this serialized with a different configuration?");

        MessagePackSerializerOptions makeOptions(
            ConfigurableKeyFormatter<Samples.Address> configurableKeyFormatter)
        {
            return MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new[] { configurableKeyFormatter }, new[] { ContractlessStandardResolver.Instance }));
        }
    }

    [Test]
    public void Indexer_properties_are_ignored()
    {
        var formatter = new ConfigurableKeyFormatter<TypeWithIndexerProperties>();
        formatter.UseAutomaticKeys();

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new[] { formatter }, new[] { ContractlessStandardResolver.Instance }));

        var input = new TypeWithIndexerProperties
        {
            [13] = new(),
            [17] = new(),
            SomeNumber = 19
        };

        var output = options.Roundtrip(input);

        output.Should().BeEquivalentTo(new TypeWithIndexerProperties { SomeNumber = 19 });
    }

    [Test]
    public void Roundtrip_complex_object()
    {
        var addressFormatter = new ConfigurableKeyFormatter<Samples.Address>();
        addressFormatter.SetKeyFor(0, a => a.City);
        addressFormatter.SetKeyFor(1, a => a.Country);
        addressFormatter.SetKeyFor(2, a => a.ZipCode);
        addressFormatter.SetKeyFor(3, a => a.StreetAddress);
        var personFormatter = new ConfigurableKeyFormatter<Samples.Person>();
        personFormatter.SetKeyFor(0, p => p.Addresses);
        personFormatter.SetKeyFor(1, p => p.Birthday);
        personFormatter.SetKeyFor(2, p => p.Email);
        personFormatter.SetKeyFor(3, p => p.FirstName);
        personFormatter.SetKeyFor(4, p => p.LastName);

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new IMessagePackFormatter[]
                {
                    addressFormatter,
                    personFormatter,
                    new NativeDateTimeFormatter()
                }, new[] { ContractlessStandardResolver.Instance }));

        options.TestRoundtrip(Samples.MakePerson());
    }

    [Test]
    public void Roundtrip_complex_object_with_automatic_keys()
    {
        var addressFormatter = new ConfigurableKeyFormatter<Samples.Address>();
        var personFormatter = new ConfigurableKeyFormatter<Samples.Person>();
        addressFormatter.UseAutomaticKeys();
        personFormatter.UseAutomaticKeys();

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new IMessagePackFormatter[]
                {
                    addressFormatter,
                    personFormatter,
                    new NativeDateTimeFormatter()
                }, new[] { ContractlessStandardResolver.Instance }));

        options.TestRoundtrip(Samples.MakePerson());
    }

    [Test]
    public void Roundtrip_if_complex_properties_are_null()
    {
        var insideFormatter = new ConfigurableKeyFormatter<Inside>();
        insideFormatter.UseAutomaticKeys();
        var outsideFormatter = new ConfigurableKeyFormatter<Outside>();
        outsideFormatter.UseAutomaticKeys();

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new IMessagePackFormatter[]
                {
                    outsideFormatter,
                    insideFormatter
                }, new[] { ContractlessStandardResolver.Instance }));

        var input = new Outside { Text = "bla" };

        options.TestRoundtrip(input);
    }

    [Test]
    public void Roundtrip_if_some_properties_are_null()
    {
        var formatter = new ConfigurableKeyFormatter<Something>();
        formatter.UseAutomaticKeys();

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new[] { formatter }, new[] { ContractlessStandardResolver.Instance }));

        var input = new Something
        {
            Count = 13,
            Text = "bla"
        };

        options.TestRoundtrip(input);
    }

    [Test]
    public void Roundtrip_simple_object()
    {
        var formatter = new ConfigurableKeyFormatter<Samples.Address>();
        formatter.SetKeyFor(0, a => a.City);
        formatter.SetKeyFor(1, a => a.Country);
        formatter.SetKeyFor(2, a => a.ZipCode);
        formatter.SetKeyFor(3, a => a.StreetAddress);

        var options =
            MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                .Create(new[] { formatter }, new[] { ContractlessStandardResolver.Instance }));

        options.TestRoundtrip(new Samples.Address
        {
            ZipCode = "10000",
            Country = "USA",
            City = "NYC",
            StreetAddress = "1st Avenue 13"
        });
    }

    [Test]
    public void Roundtrip_with_builder_configuration()
    {
        var options = MessagePackSerializer.DefaultOptions.Configure()
            .AutoKeyed<Samples.Address>()
            .AutoKeyed<Samples.Person>()
            .AddNativeFormatters()
            .Build();

        options.TestRoundtrip(Samples.MakePerson());
    }
}