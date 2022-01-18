using System.Linq;
using FluentAssertions;
using MessagePack.Attributeless.CodeGeneration;
using MessagePack.Attributeless.Implementation;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.CodeGeneration;

[TestFixture]
public class GenerationExtensionsTests
{
    [Test]
    public void AllTypes_of_PropertyMappedFormatterCollection()
    {
        var input = new PropertyMappedFormatterCollection
        {
            typeof(Samples.Arm),
            typeof(Samples.Leg),
            typeof(Samples.Wing)
        };

        var result = input.AllTypes().ToArray();

        result.Should()
        .BeEquivalentTo(new[]
        {
            typeof(Samples.Arm),
            typeof(Samples.Leg),
            typeof(Samples.Wing),
            typeof(Samples.Side),
            typeof(byte),
            typeof(int)
        });
    }

    [Test]
    public void AllTypes_of_SubTypeMappedFormatterCollection()
    {
        var input = new SubTypeMappedFormatterCollection
        {
            { typeof(Samples.IAnimal), typeof(Samples.Bird) },
            { typeof(Samples.IAnimal), typeof(Samples.Mammal) },
            { typeof(Samples.IExtremity), typeof(Samples.Arm) },
            { typeof(Samples.IExtremity), typeof(Samples.Leg) },
            { typeof(Samples.IExtremity), typeof(Samples.Wing) }
        };

        var result = input.AllTypes().ToArray();

        result.Should()
            .BeEquivalentTo(new[]
            {
                typeof(Samples.IAnimal),
                typeof(Samples.IExtremity),
                typeof(Samples.Bird),
                typeof(Samples.Mammal),
                typeof(Samples.Arm),
                typeof(Samples.Leg),
                typeof(Samples.Wing)
            });
    }
}