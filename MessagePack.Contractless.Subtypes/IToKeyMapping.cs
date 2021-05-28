using System.Linq;

namespace MessagePack.Contractless.Subtypes
{
    interface IToKeyMapping<T>
    {
        ILookup<T, int> Mappings { get; }
    }
}