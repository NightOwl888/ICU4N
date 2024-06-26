﻿using J2N.Collections.Concurrent;
using System;

namespace ICU4N.Impl.Locale
{
    public abstract class LocaleObjectCache<TKey, TValue> where TValue : class
    {
        private readonly LurchTable<TKey, Lazy<TValue>> _map;

        public LocaleObjectCache()
            : this(16)
        {
        }

        public LocaleObjectCache(int initialCapacity)
        {
            // ICU4N: Since .NET doesn't have a memory-sensitive cache, we are using an LRU cache with a fixed size.
            // This ensures that the culture(s) that the application uses most stay near the top of the cache and
            // less used cultures get popped off of the bottom of the cache.
            _map = new LurchTable<TKey, Lazy<TValue>>(initialCapacity, LurchTableOrder.Access, limit: 64, comparer: null);
        }

        public virtual TValue Get(TKey key)
        {
            var result = _map.GetOrAdd(key, (key) => new Lazy<TValue>(() => CreateObject(key)));
            return result.Value;
        }

        protected abstract TValue CreateObject(TKey key);

        protected virtual TKey NormalizeKey(TKey key)
        {
            return key;
        }
    }
}
