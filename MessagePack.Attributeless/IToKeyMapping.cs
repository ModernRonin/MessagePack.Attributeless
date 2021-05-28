using System.Collections.Generic;

namespace MessagePack.Contractless.Subtypes
{
    interface IToKeyMapping<T>
    {
        IReadOnlyDictionary<T, int> Mappings { get; }
    }
}