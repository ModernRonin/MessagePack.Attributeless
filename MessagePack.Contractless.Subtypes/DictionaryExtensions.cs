using System.Collections.Generic;
using System.Linq;

namespace MessagePack.Contractless.Subtypes
{
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     polyfill because we are on netstandard2.0
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> tuple,
            out TKey key,
            out TValue value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, TValue> self) =>
            self.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
    }
}