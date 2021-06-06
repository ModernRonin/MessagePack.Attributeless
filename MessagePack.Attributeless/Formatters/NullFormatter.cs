using MessagePack.Formatters;

namespace MessagePack.Attributeless.Formatters
{
    /// <summary>
    ///     Sometimes you want to not serialize a certain type at all. In these scenarios this formatter will be useful.
    ///     <para>
    ///         Beware that if you use this for an implementation of an interface, properties of the interface type
    ///         will still be serialized as null values, so there is some, if little, unnecessary data being serialized.
    ///     </para>
    /// </summary>
    public sealed class NullFormatter<T> : IMessagePackFormatter<T>
    {
        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => default;

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) { }
    }
}