using ICU4N.Support;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
using Microsoft.Extensions.Caching.Memory;
#else
using System.Runtime.Caching;
#endif
using System;
using System.Collections.Concurrent;

namespace ICU4N.Impl
{
    public class SimpleCache<TKey, TValue> : IICUCache<TKey, TValue>
    {
        private const int DEFAULT_CAPACITY = 16;

        private volatile object cacheRef = null;
        private readonly int type = ICUCache.Soft;
        private readonly int capacity = DEFAULT_CAPACITY;

        private static readonly TimeSpan SlidingExpiration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
        private const int ConcurrencyLevel = 4;

        public SimpleCache()
        {
        }

        public SimpleCache(int cacheType)
            : this(cacheType, DEFAULT_CAPACITY)
        {
        }

        public SimpleCache(int cacheType, int initialCapacity)
        {
            if (cacheType == ICUCache.Weak)
            {
                type = cacheType;
            }
            if (initialCapacity > 0)
            {
                capacity = initialCapacity;
            }
        }

        public virtual void Clear()
        {
            cacheRef = null;
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return GetMap().TryGetValue(key, out value);
        }

        public virtual TValue GetOrAdd(TKey key, TValue value)
        {
            return GetMap().GetOrAdd(key, value);
        }

        public virtual TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return GetMap().GetOrAdd(key, valueFactory);
        }

        private ConcurrentDictionary<TKey, TValue> GetMap()
        {
            object @ref = cacheRef;
#pragma warning disable IDE0018 // Inline variable declaration
            ConcurrentDictionary<TKey, TValue> map = null;
#pragma warning restore IDE0018 // Inline variable declaration
            if (@ref is SoftReference<ConcurrentDictionary<TKey, TValue>> soft && soft.TryGetValue(out map) && map != null)
            {
                return map;
            }
#if FEATURE_TYPEDWEAKREFERENCE
            else if (@ref is WeakReference<ConcurrentDictionary<TKey, TValue>> weak && weak.TryGetTarget(out map) && map != null)
#else
            else if (@ref is WeakReference weak && (map = (ConcurrentDictionary<TKey, TValue>)weak.Target) != null)
#endif
            {
                return map;
            }

            map = new ConcurrentDictionary<TKey, TValue>(ConcurrencyLevel, capacity);
            if (type == ICUCache.Weak)
            {
#if FEATURE_TYPEDWEAKREFERENCE
                @ref = new WeakReference<ConcurrentDictionary<TKey, TValue>>(map);
#else
                @ref = new WeakReference(map);
#endif
            }
            else
            {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                @ref = new SoftReference<ConcurrentDictionary<TKey, TValue>>(map, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
#else
                @ref = new SoftReference<ConcurrentDictionary<TKey, TValue>>(map, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
#endif
            }
            cacheRef = @ref;
            return map;
        }
    }
}
