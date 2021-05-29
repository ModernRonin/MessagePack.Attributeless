using System.Collections.Generic;

namespace MessagePack.Attributeless
{
    public interface IToKeyMapping<T>
    {
        IReadOnlyDictionary<T, int> Mappings { get; }
    }
}