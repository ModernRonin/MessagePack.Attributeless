using MessagePack.Formatters;

namespace MessagePack.Attributeless.Implementation
{
    public interface ISubTypeFormatter : IMessagePackFormatter, ISubTypeToKeyMapping { }
}