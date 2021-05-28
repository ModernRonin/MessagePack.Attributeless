using System.Collections.Generic;

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
    }
}