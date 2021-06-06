using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public interface IPropertyFormatter : IPropertyToKeyMapping, IMessagePackFormatter { }
}