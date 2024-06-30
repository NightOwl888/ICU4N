using System;
using System.Collections.Generic;

namespace ICU4N.Support.Collections
{
    internal static partial class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            dictionary.TryGetValue(key, out TValue result);
            return result;
        }

        public static void PutAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            foreach (var kvp in collection)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }
    }
}
