using System;
using System.Collections.Generic;

namespace ICU4N.Impl
{
    /// <summary>
    /// Generic, thread-safe cache implementation.
    /// </summary>
    /// <remarks>
    /// To use, pass a delegate that implements the <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> method.
    /// </remarks>
    /// <typeparam name="K">Cache lookup key type</typeparam>
    /// <typeparam name="V">Cache instance value type (must not be a CacheValue)</typeparam>
    /// <typeparam name="D">Data type for creating a new instance value</typeparam>
    /// <seealso cref="SoftCache{K, V, D}"/>
    /// <seealso cref="CacheBase{TKey, TValue, TData}"/>
    /// <author>Shad Storhaug</author>
    public class AnonymousSoftCache<K, V, D> : SoftCache<K, V, D> where V : class
    {
        private readonly Func<K, D, V> createInstance;
        public AnonymousSoftCache(Func<K,D,V> createInstance)
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
    /// When caching instances while the CacheValue "strength" is <see cref="CacheValueStrength.Soft"/>,
    /// the runtime can later release these instances once they are not used any more at all.
    /// If such an instance is then requested again, the <see cref="GetInstance(K, D)"/> method 
    /// will call <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> again and reset the CacheValue.
    /// The cache holds on to its map of keys to CacheValues forever.
    /// <para/>
    /// A value can be null if <see cref="CacheBase{TKey, TValue, TData}.CreateInstance(TKey, TData)"/> returns null.
    /// In this case, it must do so consistently for the same key and data.
    /// </remarks>
    /// <typeparam name="K">Cache lookup key type</typeparam>
    /// <typeparam name="V">Cache instance value type (must not be a CacheValue)</typeparam>
    /// <typeparam name="D">Data type for creating a new instance value</typeparam>
    /// <author>Markus Scherer, Mark Davis</author>
    public abstract class SoftCache<K, V, D> : CacheBase<K, V, D> where V : class
    {
        //private ConcurrentDictionary<K, object> map = new ConcurrentDictionary<K, object>();
        private IDictionary<K, object> map = new Dictionary<K, object>();
        private object syncLock = new object();

        public override V GetInstance(K key, D data)
        {
            // We synchronize twice, once in the ConcurrentHashMap and
            // once in valueRef.resetIfCleared(value),
            // because we prefer the fine-granularity locking of the ConcurrentHashMap
            // over coarser locking on the whole cache instance.
            // We use a CacheValue (a second level of indirection) because
            // ConcurrentHashMap.putIfAbsent() never replaces the key's value, and if it were
            // a simple Reference we would not be able to reset its value after it has been cleared.
            // (And ConcurrentHashMap.put() always replaces the value, which we don't want either.)
            object mapValue = null;
            lock (syncLock)
                map.TryGetValue(key, out mapValue);
            if (mapValue != null)
            {
                if (!(mapValue is CacheValue<V>))
                {
                    // The value was stored directly.
                    return (V)mapValue;
                }
                CacheValue<V> cv = (CacheValue<V>)mapValue;
                if (cv.IsNull)
                {
                    return null;
                }
                V value = cv.Get();
                if (value != null)
                {
                    return value;
                }
                // The instance has been evicted, its Reference cleared.
                // Create and set a new instance.
                value = CreateInstance(key, data);
                return cv.ResetIfCleared(value);
            }
            else /* valueRef == null */
            {
                // We had never cached an instance for this key.
                V value = CreateInstance(key, data);
                mapValue = (value != null && CacheValue<V>.FutureInstancesWillBeStrong) ?
                        value : (object)CacheValue<V>.GetInstance(value);
                object temp;
                lock (syncLock)
                {
                    // ICU4N TODO: use PutIfAbsent logic from elsewhere to utilize ConcurrentDictionary...?
                    if (!map.TryGetValue(key, out temp))
                    {
                        // put if absent
                        map[key] = mapValue;
                    }
                    mapValue = temp;
                }
                if (mapValue == null)
                {
                    // Normal "put": Our new value is now cached.
                    return value;
                }

                // Race condition: Another thread beat us to putting a CacheValue
                // into the map. Return its value, but just in case the garbage collector
                // was aggressive, we also offer our new instance for caching.
                if (!(mapValue is CacheValue<V>))
                {
                    // The value was stored directly.
                    return (V)mapValue;
                }
                CacheValue<V> cv = (CacheValue<V>)mapValue;
                return cv.ResetIfCleared(value);
            }
        }
    }
}
