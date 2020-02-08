using System;
using System.Collections.Concurrent;

namespace ICU4N.Impl
{
    /// <summary>
    /// Generic, thread-safe cache implementation.
    /// </summary>
    /// <remarks>
    /// To use, pass a delegate that implements the <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> method.
    /// </remarks>
    /// <typeparam name="K">Cache lookup key type.</typeparam>
    /// <typeparam name="V">Cache instance value type (must not be a <see cref="CacheValue{V}"/>).</typeparam>
    /// <typeparam name="D">Data type for creating a new instance value.</typeparam>
    /// <seealso cref="Cache{K, V, D}"/>
    /// <seealso cref="CacheBase{TKey, TValue, TData}"/>
    /// <author>Shad Storhaug</author>
    public class AnonymousCache<K, V, D> : Cache<K, V, D> where V : class
    {
        private readonly Func<K, D, V> createInstance;
        public AnonymousCache(Func<K,D,V> createInstance)
        {
            this.createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
        }

        protected override V CreateInstance(K key, D data)
        {
            return createInstance(key, data);
        }
    }

    /// <summary>
    /// Generic, thread-safe cache implementation.
    /// </summary>
    /// <remarks>
    /// To use, instantiate a subclass which implements the <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> method,
    /// and call Get() with the key and the data. The Get() call will use the data
    /// only if it needs to call <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/>, otherwise the data is ignored.
    /// <para/>
    /// The cache holds on to its map of keys to CacheValues forever.
    /// <para/>
    /// A value can be <c>null</c> if <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> returns <c>null</c>.
    /// In this case, it must do so consistently for the same key and data.
    /// </remarks>
    /// <typeparam name="TKey">Cache lookup key type.</typeparam>
    /// <typeparam name="TValue">Cache instance value type (must not be a <see cref="CacheValue{V}"/>).</typeparam>
    /// <typeparam name="TData">Data type for creating a new instance value.</typeparam>
    /// <author>Markus Scherer, Mark Davis</author>
    // ICU4N: .NET has no "SoftRefernce", so we are always using strong references here.
    // The below is greatly simplified by using a ConcurrentDictionary to do most of the heavy lifting.
    public abstract class Cache<TKey, TValue, TData> : CacheBase<TKey, TValue, TData> where TValue : class
    {
        private readonly ConcurrentDictionary<TKey, object> map = new ConcurrentDictionary<TKey, object>();

        public override TValue GetInstance(TKey key, TData data)
        {
            object mapValue = map.GetOrAdd(key, (k) =>
            {
                // We had never cached an instance for this key.
                return CreateInstance(key, data) ?? (object)CacheValue<TValue>.GetInstance(null);
            });
            if (!(mapValue is CacheValue<TValue> cv))
            {
                // The value was stored directly.
                return (TValue)mapValue;
            }
            if (cv.IsNull)
            {
                return null;
            }
            return cv.Get(); // Always a strong ref - no need to check for null and re-cache here
        }
    }
}
