using System.Collections.Generic;

namespace ICU4N.Impl
{
    public abstract class SoftCache<K, V, D> : CacheBase<K, V, D> where V : class
    {
        //private ConcurrentDictionary<K, object> map = new ConcurrentDictionary<K, object>();
        private IDictionary<K, object> map = new Dictionary<K, object>();

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
            lock (map)
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
                lock (map)
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
