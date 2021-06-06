using MessagePack.Formatters;

namespace MessagePack.Attributeless.Formatters
{
    public sealed class NullFormatter<T> : IMessagePackFormatter<T>
    {
        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => default;

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) { }
    }
}