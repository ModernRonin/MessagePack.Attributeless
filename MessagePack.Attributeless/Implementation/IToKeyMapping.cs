using System.Collections.Generic;

namespace MessagePack.Attributeless.Implementation
{
    public interface IToKeyMapping<T>
    {
        IReadOnlyDictionary<T, int> Mappings { get; }
    }
}