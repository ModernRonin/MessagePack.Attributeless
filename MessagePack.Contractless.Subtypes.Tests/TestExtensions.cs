using System.IO;
using FluentAssertions;

namespace MessagePack.Contractless.Subtypes.Tests
{
    public static class TestExtensions
    {
        public static void TestRoundtrip<T>(this MessagePackSerializerOptions self, T input)
        {
            using var stream = new MemoryStream();
            MessagePackSerializer.Serialize(stream, input, self);

            stream.Position = 0;
            var output = MessagePackSerializer.Deserialize<T>(stream, self);

            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

    }
}