using MessagePack.Attributeless.Tests;
using NUnit.Framework;

namespace MessagePack.Attributeless.CompileTime.IntegrationTests;

[SerializeGraph(typeof(Samples.IAnimal))]
// ReSharper disable once PartialTypeWithSinglePart
public partial class MySerializer
{
    public T Deserialize<T>(Stream stream) => MessagePackSerializer.Deserialize<T>(stream, Options);

    public void SerializeTo<T>(T unserialized, Stream stream) =>
        MessagePackSerializer.Serialize(stream, unserialized, Options);

    public MessagePackSerializerOptions Options { get; }
}

[TestFixture]
public class SourceGeneratorTests
{
    [Test]
    public void Show()
    {
        //Assert.Fail(Debug.Diagnostics);
    }
}