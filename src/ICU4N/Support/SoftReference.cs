using Microsoft.Extensions.Caching.Memory;

namespace ICU4N.Support
{
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
    internal class SoftReference<T>
    {
        private static readonly MemoryCache innerCache = new MemoryCache(new MemoryCacheOptions());

        public SoftReference(T value, MemoryCacheEntryOptions options)
        {
            innerCache.Set(this, value, options);
        }

        public bool TryGetValue(out T value)
        {
            return innerCache.TryGetValue(this, out value);
        }
    }
}
