using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    /// <summary> Simple lockless and memory barrier free String intern cache that is guaranteed
    /// to return the same String instance as String.intern() does.
    /// </summary>
    public class SimpleStringInterner : StringInterner
    {

        internal /*private*/ class Entry
        {
            internal /*private*/ string str;
            internal /*private*/ int hash;
            internal /*private*/ Entry next;
            internal Entry(string str, int hash, Entry next)
            {
                this.str = str;
                this.hash = hash;
                this.next = next;
            }
        }

        private Entry[] cache;
        private int maxChainLength;

        /// <param name="tableSize"> Size of the hash table, should be a power of two.
        /// </param>
        /// <param name="maxChainLength"> Maximum length of each bucket, after which the oldest item inserted is dropped.
        /// </param>
        public SimpleStringInterner(int tableSize, int maxChainLength)
        {
            cache = new Entry[System.Math.Max(1, NextHighestPowerOfTwo(tableSize))];
            this.maxChainLength = System.Math.Max(2, maxChainLength);
        }

        /// <summary>
        /// Returns the next highest power of two, or the current value if it's already a power of two or zero </summary>
        private static int NextHighestPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        // @Override
        public override string Intern(string s)
        {
            int h = s.GetHashCode();
            // In the future, it may be worth augmenting the string hash
            // if the lower bits need better distribution.
            int slot = h & (cache.Length - 1);

            Entry first = this.cache[slot];
            Entry nextToLast = null;

            int chainLength = 0;

            for (Entry e = first; e != null; e = e.next)
            {
                if (e.hash == h && (ReferenceEquals(e.str, s) || string.CompareOrdinal(e.str, s) == 0))
                {
                    // if (e.str == s || (e.hash == h && e.str.compareTo(s)==0)) {
                    return e.str;
                }

                chainLength++;
                if (e.next != null)
                {
                    nextToLast = e;
                }
            }

            // insertion-order cache: add new entry at head

#if !NETSTANDARD1_3
            s = string.Intern(s);
#endif

            this.cache[slot] = new Entry(s, h, first);
            if (chainLength >= maxChainLength)
            {
                // prune last entry
                nextToLast.next = null;
            }
            return s;
        }
    }
}
