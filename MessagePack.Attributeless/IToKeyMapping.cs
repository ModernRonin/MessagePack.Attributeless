using System.Collections.Generic;

namespace MessagePack.Attributeless
{
    interface IToKeyMapping<T>
    {
        IReadOnlyDictionary<T, int> Mappings { get; }
    }
}