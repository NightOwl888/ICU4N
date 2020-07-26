using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Dev.Test.Collate
{
    public class Counter<T> : IEnumerable<T>, IComparable<Counter<T>>
    {
        IDictionary<T, RWLong> map;
        IComparer<T> comparer;

        public Counter()
            : this(null)
        {
        }

        public Counter(IComparer<T> comparer)
        {
            if (this.comparer != null)
            {
                this.comparer = comparer;
                map = new SortedDictionary<T, RWLong>(this.comparer);
            }
            else
            {
                //map = new LinkedHashMap<T, RWLong>();
                map = new Dictionary<T, RWLong>();
            }
        }

        public sealed class RWLong : IComparable<RWLong>
        {
            // the uniqueCount ensures that two different RWIntegers will always be different
            static int uniqueCount;
            public long value;
            private int forceUnique;
            internal RWLong()
            {
                lock (typeof(RWLong))
                { // make thread-safe
                    forceUnique = uniqueCount++;
                }
            }

            public int CompareTo(RWLong that)
            {
                if (that.value < value) return -1;
                if (that.value > value) return 1;
                if (this == that) return 0;
                lock (this)
                { // make thread-safe
                    if (that.forceUnique < forceUnique) return -1;
                }
                return 1; // the forceUnique values must be different, so this is the only remaining case
            }
            public override string ToString()
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public Counter<T> Add(T obj, long countValue)
        {
            if (!map.TryGetValue(obj, out RWLong count))
                map[obj] = count = new RWLong();

            count.value += countValue;
            return this;
        }

        public long GetCount(T obj)
        {
            return Get(obj);
        }

        public long Get(T obj)
        {
            return !map.TryGetValue(obj, out RWLong count) ? 0 : count.value;
        }

        public Counter<T> Clear()
        {
            map.Clear();
            return this;
        }

        public long GetTotal()
        {
            long count = 0;
            foreach (var pair in map)
            {
                count += pair.Value.value;
            }
            return count;
        }

        public int ItemCount
        {
            get { return this.Count; }
        }

        private class Entry
        {
            internal RWLong count;
            internal T value;
            internal int uniqueness;
            public Entry(RWLong count, T value, int uniqueness)
            {
                this.count = count;
                this.value = value;
                this.uniqueness = uniqueness;
            }
        }

        private class EntryComparer : IComparer<Entry>
        {
            int countOrdering;
            IComparer<T> byValue;

            public EntryComparer(bool ascending, IComparer<T> byValue)
            {
                countOrdering = ascending ? 1 : -1;
                this.byValue = byValue;
            }
            public int Compare(Entry o1, Entry o2)
            {
                if (o1.count.value < o2.count.value) return -countOrdering;
                if (o1.count.value > o2.count.value) return countOrdering;
                if (byValue != null)
                {
                    return byValue.Compare(o1.value, o2.value);
                }
                return o1.uniqueness - o2.uniqueness;
            }
        }

        public ICollection<T> GetKeysetSortedByCount(bool ascending)
        {
            return GetKeysetSortedByCount(ascending, null);
        }

        public ICollection<T> GetKeysetSortedByCount(bool ascending, IComparer<T> byValue)
        {
            ISet<Entry> count_key = new SortedSet<Entry>(new EntryComparer(ascending, byValue));
            int counter = 0;
            foreach (T key in map.Keys)
            {
                count_key.Add(new Entry(map[key], key, counter++));
            }
            //Set<T> result = new LinkedHashSet<T>();
            IList<T> result = new List<T>();
            foreach (Entry entry in count_key)
            {
                result.Add(entry.value);
            }
            return result;
        }

        public ICollection<T> GetKeysetSortedByKey()
        {
            ISet<T> s = new SortedSet<T>(comparer);
            s.UnionWith(map.Keys);
            return s;
        }

        //public Map<T,RWInteger> getKeyToKey() {
        //Map<T,RWInteger> result = new HashMap<T,RWInteger>();
        //Iterator<T> it = map.keySet().iterator();
        //while (it.hasNext()) {
        //Object key = it.next();
        //result.put(key, key);
        //}
        //return result;
        //}

        public ICollection<T> Keys
        {
            get { return map.Keys; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return map.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDictionary<T, RWLong> GetMap()
        {
            return map; // older code was protecting map, but not the integer values.
        }

        public int Count
        {
            get { return map.Count; }
        }

        public override String ToString()
        {
            return map.ToString();
        }

        public Counter<T> UnionWith(ICollection<T> keys, int delta)
        {
            foreach (T key in keys)
            {
                Add(key, delta);
            }
            return this;
        }

        public Counter<T> UnionWith(Counter<T> keys)
        {
            foreach (T key in keys)
            {
                Add(key, keys.GetCount(key));
            }
            return this;
        }

        public int CompareTo(Counter<T> o)
        {
            using (IEnumerator<T> i = map.Keys.GetEnumerator())
            using (IEnumerator<T> j = o.map.Keys.GetEnumerator())
            {
                while (true)
                {
                    bool goti = i.MoveNext();
                    bool gotj = j.MoveNext();
                    if (!goti || !gotj)
                    {
                        return goti ? 1 : gotj ? -1 : 0;
                    }
                    T ii = i.Current;
                    T jj = i.Current;
                    int result = ((IComparable<T>)ii).CompareTo(jj);
                    if (result != 0)
                    {
                        return result;
                    }
                    long iv = map[ii].value;
                    long jv = o.map[jj].value;
                    if (iv != jv) return iv < jv ? -1 : 0;
                }
            }
        }

        public Counter<T> Increment(T key)
        {
            return Add(key, 1);
        }

        public bool ContainsKey(T key)
        {
            return map.ContainsKey(key);
        }

        public override bool Equals(Object o)
        {
            return map.Equals(o);
        }

        public override int GetHashCode()
        {
            return map.GetHashCode();
        }

        // ICU4N: Use Count == 0
        //public boolean isEmpty()
        //{
        //    return map.isEmpty();
        //}

        public Counter<T> Remove(T key)
        {
            map.Remove(key);
            return this;
        }

        //public RWLong put(T key, RWLong value) {
        //    return map.put(key, value);
        //}
        //
        //public void putAll(Map<? extends T, ? extends RWLong> t) {
        //    map.putAll(t);
        //}
        //
        //public Set<java.util.Map.Entry<T, Long>> entrySet() {
        //    return map.entrySet();
        //}
        //
        //public Collection<RWLong> values() {
        //    return map.values();
        //}
    }
}
