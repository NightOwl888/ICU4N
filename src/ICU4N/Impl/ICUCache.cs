using System;

namespace ICU4N.Impl
{
    public static class ICUCache
    {
        // Type of reference holding the Map instance
        public const int Soft = 0;
        public const int Weak = 1;

        // NULL object, which may be used for a cache key
        public static readonly object Null = new object();
    }

    public interface IICUCache<TKey, TValue>
    {
        void Clear();

        bool TryGetValue(TKey key, out TValue value);

        TValue GetOrAdd(TKey key, TValue value);

        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
    }
}
