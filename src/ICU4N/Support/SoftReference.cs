using System.Globalization;
using System.Threading;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
using Microsoft.Extensions.Caching.Memory;
#else
using System.Runtime.Caching;
#endif

namespace ICU4N.Support
{
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
    /// <summary>
    /// A reference that can be configured to be kept in memory based on <see cref="MemoryCacheEntryOptions"/>.
    /// <para/>
    /// While not really a "soft" reference like the one in Java that is memory-sensitive, <see cref="SoftReference{T}"/>
    /// uses <see cref="MemoryCache"/> to store/evict a cached value based on a caching policy supplied
    /// in the constructor. The instance itself is the key to the cache, so the user must take care not
    /// to use the same value for multiple <see cref="SoftReference{T}"/> instances.
    /// <para/>
    /// When the cached item is no longer available, <see cref="TryGetValue(out T)"/> will return <c>false</c>.
    /// To reload, a new <see cref="SoftReference{T}"/> instance must be created to replace the expired cache
    /// reference.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
#else
    /// <summary>
    /// A reference that can be configured to be kept in memory based on <see cref="CacheItemPolicy"/>.
    /// <para/>
    /// While not really a "soft" reference like the one in Java that is memory-sensitive, <see cref="SoftReference{T}"/>
    /// uses <see cref="MemoryCache"/> to store/evict a cached value based on a caching policy supplied
    /// in the constructor. The instance itself is the key to the cache, so the user must take care not
    /// to use the same value for multiple <see cref="SoftReference{T}"/> instances.
    /// <para/>
    /// When the cached item is no longer available, <see cref="TryGetValue(out T)"/> will return <c>false</c>.
    /// To reload, a new <see cref="SoftReference{T}"/> instance must be created to replace the expired cache
    /// reference.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
#endif
    internal class SoftReference<T>
    {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
        private static readonly MemoryCache innerCache = new MemoryCache(new MemoryCacheOptions());
#else
        private static readonly MemoryCache innerCache = new MemoryCache("ICU4N");
        private static int lastId = 0;
        private int id;
#endif

        public SoftReference(T value,
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
            MemoryCacheEntryOptions options
#else
            CacheItemPolicy options
#endif
        )
        {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
            innerCache.Set(this, value, options);
#else
            id = Interlocked.Increment(ref lastId);
            innerCache.Set(id.ToString(CultureInfo.InvariantCulture), value, options);
#endif
        }

        public bool TryGetValue(out T value)
        {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
            return innerCache.TryGetValue(this, out value);
#else
            value = (T) innerCache.Get(id.ToString(CultureInfo.InvariantCulture));
            return value != null;
#endif
        }
    }
}
