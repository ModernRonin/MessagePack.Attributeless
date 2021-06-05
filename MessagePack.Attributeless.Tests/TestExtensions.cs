using System.IO;
using FluentAssertions;

namespace MessagePack.Attributeless.Tests
{
    public static class TestExtensions
    {
        public static T Roundtrip<T>(this MessagePackSerializerOptions self, T input)
        {
            using var stream = new MemoryStream();
            MessagePackSerializer.Serialize(stream, input, self);

            stream.Position = 0;
            return MessagePackSerializer.Deserialize<T>(stream, self);
        }

        public static void TestRoundtrip<T>(this MessagePackSerializerOptions self, T input)
        {
            var output = Roundtrip(self, input);

            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }
    }
}