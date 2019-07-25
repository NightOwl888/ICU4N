using ICU4N.Support;
using ICU4N.Support.Collections;
using System.Collections.Generic;

namespace ICU4N.Impl.Locale
{
    public abstract class LocaleObjectCache<K, V> where V : class
    {
        //private ConcurrentDictionary<K, CacheEntry> _map;
        private readonly IDictionary<K, CacheEntry> _map;
        private readonly ReferenceQueue<V> _queue = new ReferenceQueue<V>();

        public LocaleObjectCache()
            //: this(16, 16)
            : this(16)
        {
        }

        public LocaleObjectCache(int initialCapacity)
        {
            _map = new Dictionary<K, CacheEntry>(initialCapacity);
        }

        //public LocaleObjectCache(int initialCapacity, int concurrencyLevel)
        //{
        //    _map = new ConcurrentDictionary<K, CacheEntry>(concurrencyLevel, initialCapacity);
        //}

        public virtual V Get(K key)
        {
            V value = null;

            CleanStaleEntries();
            CacheEntry entry;
            lock (_map)
            {
                if (_map.TryGetValue(key, out entry) && entry != null)
                {
                    value = entry.Get();
                }
            }
            if (value == null)
            {
                key = NormalizeKey(key);
                V newVal = CreateObject(key);
                if (key == null || newVal == null)
                {
                    // subclass must return non-null key/value object
                    return null;
                }

                CacheEntry newEntry = new CacheEntry(key, newVal, _queue);

                while (value == null)
                {
                    CleanStaleEntries();
                    lock (_map)
                    {
                        if (!_map.TryGetValue(key, out entry)) // ICU4N TODO: Fix PutIfAbsent functionality and ConcurrentDictionary
                        {
                            value = newVal;
                            break;
                        }
                        else
                        {
                            _map[key] = newEntry;
                            value = entry.Get();
                        }
                    }

                    //entry = _map.PutIfAbsent(key, newEntry);
                    //if (entry == null)
                    //{
                    //    value = newVal;
                    //    break;
                    //}
                    //else
                    //{
                    //    value = entry.Get();
                    //}
                }
            }
            return value;
        }

        private void CleanStaleEntries()
        {
            CacheEntry entry;
            while ((entry = (CacheEntry)_queue.Poll()) != null)
            {
                lock (_map)
                {
                    _map.Remove(entry.Key);
                }
            }
        }

        protected abstract V CreateObject(K key);

        protected virtual K NormalizeKey(K key)
        {
            return key;
        }

        private class CacheEntry : SoftReference<V>
        {
            private K _key;

            internal CacheEntry(K key, V value, ReferenceQueue<V> queue)
                : base(value, queue)
            {
                _key = key;
            }

            internal virtual K Key
            {
                get { return _key; }
            }
        }
    }
}
