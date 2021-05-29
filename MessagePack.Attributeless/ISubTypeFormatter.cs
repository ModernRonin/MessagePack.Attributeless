using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public interface ISubTypeFormatter : IMessagePackFormatter, ISubTypeToKeyMapping { }
}