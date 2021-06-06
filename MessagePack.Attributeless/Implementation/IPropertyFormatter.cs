using MessagePack.Formatters;

namespace MessagePack.Attributeless.Implementation
{
    public interface IPropertyFormatter : IPropertyToKeyMapping, IMessagePackFormatter { }
}