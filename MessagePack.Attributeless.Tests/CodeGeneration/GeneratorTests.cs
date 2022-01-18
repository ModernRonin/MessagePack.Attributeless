using ApprovalTests;
using FluentAssertions;
using MessagePack.Attributeless.CodeGeneration;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.CodeGeneration;

[TestFixture]
public class GeneratorTests
{
    class BadType
    {
        public TestActionAttribute UnconfiguredType { get; set; }
    }

    [Test]
    public void Does_not_throw_for_valid_configuration()
    {
        var config = MessagePackSerializer.DefaultOptions.Configure()
            .GraphOf<Samples.IAnimal>()
            .Configuration;

        var underTest = new Generator();

        var action = () => underTest.Generate(config);

        action.Should().NotThrow();
    }

    [Test]
    public void Generate_concrete_type_with_enum()
    {
        var config = MessagePackSerializer.DefaultOptions.Configure()
            .GraphOf<Samples.Leg>()
            .Configuration;

        var result = new Generator().Generate(config);

        Approvals.Verify(result);
    }

    [Test]
    public void Generate_type_hierarchy()
    {
        var config = MessagePackSerializer.DefaultOptions.Configure()
            .GraphOf<Samples.IExtremity>()
            .Configuration;

        var result = new Generator().Generate(config);

        Approvals.Verify(result);
    }

    [Test]
    public void Throws_for_invalid_configuration()
    {
        var config = MessagePackSerializer.DefaultOptions.Configure()
            .GraphOf<BadType>()
            .Configuration;

        var underTest = new Generator();

        var action = () => underTest.Generate(config);

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "configuration is incomplete - the following types are neither mapped nor handled by MessagePack natively nor enums: NUnit.Framework.TestActionAttribute");
    }
}