#if NET40
    using System.Runtime.Caching;
#else
    using Microsoft.Extensions.Caching.Memory;
#endif

namespace ICU4N.Support
{
    #if NET40
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
#else    
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
#endif    
    internal class SoftReference<T>
    {
#if NET40
        private static readonly MemoryCache innerCache = new MemoryCache("ICU4N");
        private static int lastId = 0;
        private int id;
#else
        private static readonly MemoryCache innerCache = new MemoryCache(new MemoryCacheOptions());
#endif
        
        public SoftReference(T value, 
#if NET40
            CacheItemPolicy options
#else
            MemoryCacheEntryOptions options
#endif       
        )
        {
#if NET40
            id = lastId++;
            innerCache.Set(id.ToString(), value, options);
#else
            innerCache.Set(this, value, options);
#endif
        }

        public bool TryGetValue(out T value)
        {
#if NET40
            value = (T) innerCache.Get(id.ToString());
            return value != null;
#else
            return innerCache.TryGetValue(this, out value);
#endif
        }
    }
}
