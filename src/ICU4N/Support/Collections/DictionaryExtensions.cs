using System.Collections.Generic;

namespace ICU4N.Support.Collections
{
    internal static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue result);
            return result;
        }

        public static void PutAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> kvps)
        {
            foreach (var kvp in kvps)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }
    }
}
