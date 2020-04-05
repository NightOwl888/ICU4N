using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using J2N.Collections.Generic.Extensions;
using J2N.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Util
{
    /// <summary>
    /// Class for mapping Unicode characters and strings to values, optimized for single code points, 
    /// where ranges of code points have the same value.
    /// Much smaller storage than using <see cref="Dictionary{TKey, TValue}"/>, and much faster and more compact than
    /// a list of <see cref="UnicodeSet"/>s. The API design mimics <see cref="T:IDictionary{String, TValue}"/> but can't extend it due to some
    /// necessary changes (much as <see cref="UnicodeSet"/> mimics <see cref="T:ISet{string}"/>). Note that nulls are not permitted as values;
    /// that is, a Put(x,null) is the same as Remove(x).
    /// <para/>
    /// At this point "" is also not allowed as a key, although that may change.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>markdavis</author>
    public sealed class UnicodeMap<T> : IFreezable<UnicodeMap<T>>, IStringTransform, IEnumerable<string>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
        where T : class
    {
        /**
     * For serialization
     */
        //private static final long serialVersionUID = -6540936876295804105L;
        internal static readonly bool ASSERTIONS = false;
        internal const long GROWTH_PERCENT = 200; // 100 is no growth!
        internal const long GROWTH_GAP = 10; // extra bump!

        private int length;
        // two parallel arrays to save memory. Wish Java had structs.
        private int[] transitions;
        /* package private */
        internal T[] values;

        //private LinkedHashSet<T> availableValues = new LinkedHashSet<T>();
        private List<T> availableValues = new List<T>();
        private /* transient */ bool staleAvailableValues;

        private /* transient */  bool errorOnReset;
        private volatile /* transient */  bool locked;
        private int lastIndex;
        private SortedDictionary<string, T> stringMap;

        //{ clear();
        //}

        public UnicodeMap()
        {
            Clear();
        }

        public UnicodeMap(UnicodeMap<T> other)
        {
            Clear();
            this.PutAll(other);
        }

        public UnicodeMap<T> Clear()
        {
            if (locked)
            {
                throw new NotSupportedException("Attempt to modify locked object");
            }
            length = 2;
            transitions = new int[] { 0, 0x110000, 0, 0, 0, 0, 0, 0, 0, 0 };
            values = new T[10];

            availableValues.Clear();
            staleAvailableValues = false;

            errorOnReset = false;
            lastIndex = 0;
            stringMap = null;
            return this;
        }

        /* Boilerplate */
        public override bool Equals(object other)
        {
            if (other == null) return false;
            try
            {
                UnicodeMap<T> that = (UnicodeMap<T>)other;
                if (length != that.length) return false;
                for (int i = 0; i < length - 1; ++i)
                {
                    if (transitions[i] != that.transitions[i]) return false;
                    if (!AreEqual(values[i], that.values[i])) return false;
                }
                return true;
            }
            catch (InvalidCastException e)
            {
                return false;
            }
        }

        public static bool AreEqual(object a, object b)
        {
            if (a == b) return true;
            if (a == null || b == null) return false;
            return a.Equals(b);
        }

        public override int GetHashCode()
        {
            int result = length;
            // TODO might want to abbreviate this for speed.
            for (int i = 0; i < length - 1; ++i)
            {
                result = 37 * result + transitions[i];
                result = 37 * result;
                if (values[i] != null)
                {
                    result += values[i].GetHashCode();
                }
            }
            if (stringMap != null)
            {
                result = 37 * result + stringMap.GetHashCode();
            }
            return result;
        }

        /**
         * Standard clone. Warning, as with Collections, does not do deep clone.
         */
        public UnicodeMap<T> CloneAsThawed()
        {
            UnicodeMap<T> that = new UnicodeMap<T>();
            that.length = length;
            that.transitions = (int[])transitions.Clone();
            that.values = (T[])values.Clone();
            //that.availableValues = new LinkedHashSet<T>(availableValues);
            that.availableValues = new List<T>(availableValues);
            that.locked = false;
            that.stringMap = stringMap == null ? null : new SortedDictionary<String, T>(stringMap);
            return that;
        }

        /* for internal consistency checking */

        void _checkInvariants()
        {
            if (length < 2
                    || length > transitions.Length
                    || transitions.Length != values.Length)
            {
                throw new ArgumentException("Invariant failed: Lengths bad");
            }
            for (int i = 1; i < length - 1; ++i)
            {
                if (AreEqual(values[i - 1], values[i]))
                {
                    throw new ArgumentException("Invariant failed: values shared at "
                            + "\t" + Utility.Hex(i - 1) + ": <" + values[i - 1] + ">"
                            + "\t" + Utility.Hex(i) + ": <" + values[i] + ">"
                    );
                }
            }
            if (transitions[0] != 0 || transitions[length - 1] != 0x110000)
            {
                throw new ArgumentException("Invariant failed: bounds set wrong");
            }
            for (int i = 1; i < length - 1; ++i)
            {
                if (transitions[i - 1] >= transitions[i])
                {
                    throw new ArgumentException("Invariant failed: not monotonic"
                            + "\t" + Utility.Hex(i - 1) + ": " + transitions[i - 1]
                                                                           + "\t" + Utility.Hex(i) + ": " + transitions[i]
                    );
                }
            }
        }

        /**
         * Finds an index such that inversionList[i] &lt;= codepoint &lt; inversionList[i+1]
         * Assumes that 0 &lt;= codepoint &lt;= 0x10FFFF
         * @param codepoint
         * @return the index
         */
        private int _findIndex(int c)
        {
            int lo = 0;
            int hi = length - 1;
            int i = (lo + hi).TripleShift(1);
            // invariant: c >= list[lo]
            // invariant: c < list[hi]
            while (i != lo)
            {
                if (c < transitions[i])
                {
                    hi = i;
                }
                else
                {
                    lo = i;
                }
                i = (lo + hi).TripleShift(1);
            }
            if (ASSERTIONS) _checkFind(c, lo);
            return lo;
        }

        private void _checkFind(int codepoint, int value)
        {
            int other = __findIndex(codepoint);
            if (other != value)
            {
                throw new ArgumentException("Invariant failed: binary search"
                        + "\t" + Utility.Hex(codepoint) + ": " + value
                        + "\tshould be: " + other);
            }
        }

        private int __findIndex(int codepoint)
        {
            for (int i = length - 1; i > 0; --i)
            {
                if (transitions[i] <= codepoint) return i;
            }
            return 0;
        }

        /*
         * Try indexed lookup

        static final int SHIFT = 8;
        int[] starts = new int[0x10FFFF>>SHIFT]; // lowest transition index where codepoint>>x can be found
        boolean startsValid = false;
        private int findIndex(int codepoint) {
            if (!startsValid) {
                int start = 0;
                for (int i = 1; i < length; ++i) {

                }
            }
            for (int i = length-1; i > 0; --i) {
               if (transitions[i] <= codepoint) return i;
           }
           return 0;
       }
         */

        /**
         * Remove the items from index through index+count-1.
         * Logically reduces the size of the internal arrays.
         * @param index
         * @param count
         */
        private void _removeAt(int index, int count)
        {
            for (int i = index + count; i < length; ++i)
            {
                transitions[i - count] = transitions[i];
                values[i - count] = values[i];
            }
            length -= count;
        }
        /**
         * Add a gap from index to index+count-1.
         * The values there are undefined, and must be set.
         * Logically grows arrays to accomodate. Actual growth is limited
         * @param index
         * @param count
         */
        private void _insertGapAt(int index, int count)
        {
            int newLength = length + count;
            int[] oldtransitions = transitions;
            T[] oldvalues = values;
            if (newLength > transitions.Length)
            {
                int allocation = (int)(GROWTH_GAP + (newLength * GROWTH_PERCENT) / 100);
                transitions = new int[allocation];
                values = new T[allocation];
                for (int i = 0; i < index; ++i)
                {
                    transitions[i] = oldtransitions[i];
                    values[i] = oldvalues[i];
                }
            }
            for (int i = length - 1; i >= index; --i)
            {
                transitions[i + count] = oldtransitions[i];
                values[i + count] = oldvalues[i];
            }
            length = newLength;
        }

        /**
         * Associates code point with value. Removes any previous association.
         * All code that calls this MUST check for frozen first!
         * @param codepoint
         * @param value
         * @return this, for chaining
         */
        private UnicodeMap<T> _put(int codepoint, T value)
        {
            // Warning: baseIndex is an invariant; must
            // be defined such that transitions[baseIndex] < codepoint
            // at end of this routine.
            int baseIndex;
            if (transitions[lastIndex] <= codepoint
                    && codepoint < transitions[lastIndex + 1])
            {
                baseIndex = lastIndex;
            }
            else
            {
                baseIndex = _findIndex(codepoint);
            }
            int limitIndex = baseIndex + 1;
            // cases are (a) value is already set
            if (AreEqual(values[baseIndex], value)) return this;
            if (locked)
            {
                throw new NotSupportedException("Attempt to modify locked object");
            }
            if (errorOnReset && values[baseIndex] != null)
            {
                throw new NotSupportedException("Attempt to reset value for " + Utility.Hex(codepoint)
                        + " when that is disallowed. Old: " + values[baseIndex] + "; New: " + value);
            }

            // adjust the available values
            staleAvailableValues = true;
            if (!availableValues.Contains(value)) availableValues.Add(value); // add if not there already      

            int baseCP = transitions[baseIndex];
            int limitCP = transitions[limitIndex];
            // we now start walking through the difference case,
            // based on whether we are at the start or end of range
            // and whether the range is a single character or multiple

            if (baseCP == codepoint)
            {
                // CASE: At very start of range
                bool connectsWithPrevious =
                    baseIndex != 0 && AreEqual(value, values[baseIndex - 1]);

                if (limitCP == codepoint + 1)
                {
                    // CASE: Single codepoint range
                    bool connectsWithFollowing =
                        baseIndex < length - 2 && AreEqual(value, values[limitIndex]); // was -1

                    if (connectsWithPrevious)
                    {
                        // A1a connects with previous & following, so remove index
                        if (connectsWithFollowing)
                        {
                            _removeAt(baseIndex, 2);
                        }
                        else
                        {
                            _removeAt(baseIndex, 1); // extend previous
                        }
                        --baseIndex; // fix up
                    }
                    else if (connectsWithFollowing)
                    {
                        _removeAt(baseIndex, 1); // extend following backwards
                        transitions[baseIndex] = codepoint;
                    }
                    else
                    {
                        // doesn't connect on either side, just reset
                        values[baseIndex] = value;
                    }
                }
                else if (connectsWithPrevious)
                {
                    // A.1: start of multi codepoint range
                    // if connects
                    ++transitions[baseIndex]; // extend previous
                }
                else
                {
                    // otherwise insert new transition
                    transitions[baseIndex] = codepoint + 1; // fix following range
                    _insertGapAt(baseIndex, 1);
                    values[baseIndex] = value;
                    transitions[baseIndex] = codepoint;
                }
            }
            else if (limitCP == codepoint + 1)
            {
                // CASE: at end of range        
                // if connects, just back up range
                bool connectsWithFollowing =
                    baseIndex < length - 2 && AreEqual(value, values[limitIndex]); // was -1

                if (connectsWithFollowing)
                {
                    --transitions[limitIndex];
                    return this;
                }
                else
                {
                    _insertGapAt(limitIndex, 1);
                    transitions[limitIndex] = codepoint;
                    values[limitIndex] = value;
                }
            }
            else
            {
                // CASE: in middle of range
                // insert gap, then set the new range
                _insertGapAt(++baseIndex, 2);
                transitions[baseIndex] = codepoint;
                values[baseIndex] = value;
                transitions[baseIndex + 1] = codepoint + 1;
                values[baseIndex + 1] = values[baseIndex - 1]; // copy lower range values
            }
            lastIndex = baseIndex; // store for next time
            return this;
        }

        private UnicodeMap<T> _putAll(int startCodePoint, int endCodePoint, T value)
        {
            // TODO optimize
            for (int i = startCodePoint; i <= endCodePoint; ++i)
            {
                _put(i, value);
                if (ASSERTIONS) _checkInvariants();
            }
            return this;
        }

        /**
         * Sets the codepoint value.
         * @param codepoint
         * @param value
         * @return this (for chaining)
         */
        public UnicodeMap<T> Put(int codepoint, T value)
        {
            if (codepoint < 0 || codepoint > 0x10FFFF)
            {
                throw new ArgumentException("Codepoint out of range: " + codepoint);
            }
            _put(codepoint, value);
            if (ASSERTIONS) _checkInvariants();
            return this;
        }

        /**
         * Sets the codepoint value.
         * @param codepoint
         * @param value
         * @return this (for chaining)
         */
        public UnicodeMap<T> Put(string str, T value)
        {
            int v = UnicodeSet.GetSingleCodePoint(str);
            if (v == int.MaxValue)
            {
                if (locked)
                {
                    throw new NotSupportedException("Attempt to modify locked object");
                }
                if (value != null)
                {
                    if (stringMap == null)
                    {
                        stringMap = new SortedDictionary<string, T>(StringComparer.Ordinal);
                    }
                    stringMap[str] = value;
                    staleAvailableValues = true;
                }
                else if (stringMap != null)
                {
                    if (stringMap.Remove(str))
                    {
                        staleAvailableValues = true;
                    }
                }
                return this;
            }
            return Put(v, value);
        }

        /**
         * Adds bunch o' codepoints; otherwise like put.
         * @param codepoints
         * @param value
         * @return this (for chaining)
         */
        public UnicodeMap<T> PutAll(UnicodeSet codepoints, T value)
        {
            UnicodeSetIterator it = new UnicodeSetIterator(codepoints);
            while (it.NextRange())
            {
                if (it.String == null)
                {
                    _putAll(it.Codepoint, it.CodepointEnd, value);
                }
                else
                {
                    Put(it.String, value);
                }
            }
            return this;
        }

        /**
         * Adds bunch o' codepoints; otherwise like add.
         * @param startCodePoint
         * @param endCodePoint
         * @param value
         * @return this (for chaining)
         */
        public UnicodeMap<T> PutAll(int startCodePoint, int endCodePoint, T value)
        {
            if (locked)
            {
                throw new NotSupportedException("Attempt to modify locked object");
            }
            if (startCodePoint < 0 || endCodePoint > 0x10FFFF)
            {
                throw new ArgumentException("Codepoint out of range: "
                        + Utility.Hex(startCodePoint) + ".." + Utility.Hex(endCodePoint));
            }
            return _putAll(startCodePoint, endCodePoint, value);
        }

        /**
         * Add all the (main) values from a UnicodeMap
         * @param unicodeMap the property to add to the map
         * @return this (for chaining)
         */
        public UnicodeMap<T> PutAll(UnicodeMap<T> unicodeMap)
        {
            for (int i = 0; i < unicodeMap.length; ++i)
            {
                T value = unicodeMap.values[i];
                if (value != null)
                {
                    _putAll(unicodeMap.transitions[i], unicodeMap.transitions[i + 1] - 1, value);
                }
                if (ASSERTIONS) _checkInvariants();
            }
            if (unicodeMap.stringMap != null && unicodeMap.stringMap.Count > 0)
            {
                if (stringMap == null)
                {
                    stringMap = new SortedDictionary<string, T>();
                }
                stringMap.PutAll(unicodeMap.stringMap);
            }
            return this;
        }

        /**
         * Add all the (main) values from a Unicode property
         * @param prop the property to add to the map
         * @return this (for chaining)
         */
        public UnicodeMap<T> PutAllFiltered(UnicodeMap<T> prop, UnicodeSet filter)
        {
            // TODO optimize
            for (UnicodeSetIterator it = new UnicodeSetIterator(filter); it.Next();)
            {
                if (it.Codepoint != UnicodeSetIterator.IsString)
                {
                    T value = prop.GetValue(it.Codepoint);
                    if (value != null)
                    {
                        _put(it.Codepoint, value);
                    }
                }
            }
            // now do the strings
            foreach (string key in filter.Strings)
            {
                T value = prop.Get(key);
                if (value != null)
                {
                    Put(key, value);
                }
            }
            return this;
        }

        /**
         * Set the currently unmapped Unicode code points to the given value.
         * @param value the value to set
         * @return this (for chaining)
         */
        public UnicodeMap<T> SetMissing(T value)
        {
            // fast path, if value not yet present
            if (!GetAvailableValues().Contains(value))
            {
                staleAvailableValues = true;
                availableValues.Add(value);
                for (int i = 0; i < length; ++i)
                {
                    if (values[i] == null) values[i] = value;
                }
                return this;
            }
            else
            {
                return PutAll(KeySet(null), value);
            }
        }
        /**
         * Returns the keyset consisting of all the keys that would produce the given value. Deposits into
         * result if it is not null. Remember to clear if you just want
         * the new values.
         */
        public UnicodeSet KeySet(T value, UnicodeSet result)
        {
            if (result == null) result = new UnicodeSet();
            for (int i = 0; i < length - 1; ++i)
            {
                if (AreEqual(value, values[i]))
                {
                    result.Add(transitions[i], transitions[i + 1] - 1);
                }
            }
            if (value != null && stringMap != null)
            {
                foreach (string key in stringMap.Keys)
                {
                    T newValue = stringMap.Get(key);
                    if (value.Equals(newValue))
                    {
                        result.Add((string)key);
                    }
                }
            }
            return result;
        }

        /**
         * Returns the keyset consisting of all the keys that would produce the given value.
         * the new values.
         */
        public UnicodeSet KeySet(T value)
        {
            return KeySet(value, null);
        }

        /**
         * Returns the keyset consisting of all the keys that would produce (non-null) values.
         */
        public UnicodeSet KeySet()
        {
            UnicodeSet result = new UnicodeSet();
            for (int i = 0; i < length - 1; ++i)
            {
                if (values[i] != null)
                {
                    result.Add(transitions[i], transitions[i + 1] - 1);
                }
            }
            if (stringMap != null)
            {
                result.AddAll(stringMap.Keys);
            }
            return result;
        }

        /**
         * Returns the list of possible values. Deposits each non-null value into
         * result. Creates result if it is null. Remember to clear result if
         * you are not appending to existing collection.
         * @param result
         * @return result
         */
        //public U Values<U>(U result) where U : ICollection<T>
        public ICollection<T> Values(ICollection<T> result)

        {
            if (staleAvailableValues)
            {
                // collect all the current values
                // retain them in the availableValues
                ISet<T> temp = new HashSet<T>();
                for (int i = 0; i < length - 1; ++i)
                {
                    if (values[i] != null) temp.Add(values[i]);
                }
                //availableValues.RetainAll(temp);
                //foreach (var value in availableValues)
                //{
                //    if (!temp.Contains(value))
                //        availableValues.Remove(value);
                //}
                int keep = 0, availableCount = availableValues.Count;
                for (int i = 0; i < availableCount; i++)
                {
                    var value = availableValues[i];
                    if (temp.Contains(value))
                    {
                        if (i != keep)
                        {
                            // If this is not an element we want to toss,
                            // move it to the next position in the list.
                            availableValues[keep] = value;
                        }
                        keep++;
                    }
                }
                if (availableCount - keep > 0)
                {
                    // Remove everything else at once. This is several orders of magnitude
                    // faster than removing elements one at a time.
                    // Reference: https://stackoverflow.com/a/621836
                    availableValues.RemoveRange(keep, availableCount - keep);
                }

                if (stringMap != null)
                {
                    //availableValues.addAll(stringMap.values());
                    foreach (var value in stringMap.Values)
                    {
                        if (!availableValues.Contains(value))
                            availableValues.Add(value);
                    }
                }
                staleAvailableValues = false;
            }
            if (result == null)
            {
                //result = (U)new LinkedHashSet<T>(availableValues.size());
                result = new List<T>(availableValues.Count);
            }
            //result.addAll(availableValues);
            //result.AddRange(availableValues);
            foreach (var value in availableValues)
                result.Add(value);

            return result;
        }

        /**
         * Convenience method
         */
        public ICollection<T> Values()
        {
            return GetAvailableValues(null);
        }
        /**
         * Gets the value associated with a given code point.
         * Returns null, if there is no such value.
         * @param codepoint
         * @return the value
         */
        public T Get(int codepoint)
        {
            if (codepoint < 0 || codepoint > 0x10FFFF)
            {
                throw new ArgumentException("Codepoint out of range: " + codepoint);
            }
            return values[_findIndex(codepoint)];
        }

        /**
         * Gets the value associated with a given code point.
         * Returns null, if there is no such value.
         * @param codepoint
         * @return the value
         */
        public T Get(string value)
        {
            if (UTF16.HasMoreCodePointsThan(value, 1))
            {
                if (stringMap == null)
                {
                    return null;
                }
                return stringMap.Get(value);
            }
            return GetValue(UTF16.CharAt(value, 0));
        }


        /**
         * Change a new string from the source string according to the mappings.
         * For each code point cp, if getValue(cp) is null, append the character, otherwise append getValue(cp).toString()
         * TODO: extend to strings
         * @param source
         * @return
         */
        public string Transform(string source)
        {
            StringBuffer result = new StringBuffer();
            int cp;
            for (int i = 0; i < source.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(source, i);
                T mResult = GetValue(cp);
                if (mResult != null)
                {
                    result.Append(mResult);
                }
                else
                {
                    UTF16.Append(result, cp);
                }
            }
            return result.ToString();
        }

        /**
         * Used to add complex values, where the value isn't replaced but in some sense composed
         * @author markdavis
         */
        public abstract class Composer
        {
            /**
             * This will be called with either a string or a code point. The result is the new value for that item.
             * If the codepoint is used, the string is null; if the string is used, the codepoint is -1.
             * @param a
             * @param b
             */
            public abstract T Compose(int codePoint, string str, T a, T b);
        }

        public UnicodeMap<T> ComposeWith(UnicodeMap<T> other, Composer composer)
        {
            foreach (T value in other.GetAvailableValues())
            {
                UnicodeSet set = other.KeySet(value);
                ComposeWith(set, value, composer);
            }
            return this;
        }

        public UnicodeMap<T> ComposeWith(UnicodeSet set, T value, Composer composer)
        {
            for (UnicodeSetIterator it = new UnicodeSetIterator(set); it.Next();)
            {
                int i = it.Codepoint;
                if (i == UnicodeSetIterator.IsString)
                {
                    string s = it.String;
                    T v1 = GetValue(s);
                    T v3 = composer.Compose(-1, s, v1, value);
                    if (!ReferenceEquals(v1, v3) && (v1 == null || !v1.Equals(v3)))
                    {
                        Put(s, v3);
                    }
                }
                else
                {
                    T v1 = GetValue(i);
                    T v3 = composer.Compose(i, null, v1, value);
                    if (!ReferenceEquals(v1, v3) && (v1 == null || !v1.Equals(v3)))
                    {
                        Put(i, v3);
                    }
                }
            }
            return this;
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(IComparer<T> collected)
        {
            StringBuffer result = new StringBuffer();
            if (collected == null)
            {
                for (int i = 0; i < length - 1; ++i)
                {
                    T value = values[i];
                    if (value == null) continue;
                    int start = transitions[i];
                    int end = transitions[i + 1] - 1;
                    result.Append(Utility.Hex(start));
                    if (start != end) result.Append("-").Append(Utility.Hex(end));
                    result.Append("=").Append(value.ToString()).Append("\n");
                }
                if (stringMap != null)
                {
                    foreach (string s in stringMap.Keys)
                    {
                        result.Append(Utility.Hex(s)).Append("=").Append(stringMap.Get(s).ToString()).Append("\n");
                    }
                }
            }
            else
            {
                var set = Values(new SortedSet<T>(collected));
                foreach (T value in set)
                {
                    UnicodeSet s = KeySet(value);
                    result.Append(value).Append("=").Append(s.ToString()).Append("\n");
                }
            }
            return result.ToString();
        }
        /**
         * @return Returns the errorOnReset value.
         */
        public bool ErrorOnReset
        {
            get { return errorOnReset; }
            set { errorOnReset = value; }
        }
        ///**
        // * Puts the UnicodeMap into a state whereby new mappings are accepted, but changes to old mappings cause an exception.
        // * @param errorOnReset The errorOnReset to set.
        // */
        //public UnicodeMap<T> setErrorOnReset(bool errorOnReset)
        //{
        //    this.errorOnReset = errorOnReset;
        //    return this;
        //}

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.util.Freezable#isFrozen()
         */
        public bool IsFrozen
        {
            get
            {
                // TODO Auto-generated method stub
                return locked;
            }
        }

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.util.Freezable#lock()
         */
        public UnicodeMap<T> Freeze()
        {
            locked = true;
            return this;
        }

        /**
         * Utility to find the maximal common prefix of two strings.
         * TODO: fix supplemental support
         */
        public static int FindCommonPrefix(string last, string s)
        {
            int minLen = Math.Min(last.Length, s.Length);
            for (int i = 0; i < minLen; ++i)
            {
                if (last[i] != s[i]) return i;
            }
            return minLen;
        }

        /**
         * Get the number of ranges; used for getRangeStart/End. The ranges together cover all of the single-codepoint keys in the UnicodeMap. Other keys can be gotten with getStrings().
         */
        public int RangeCount
        {
            get { return length - 1; }
        }

        /**
         * Get the start of a range. All code points between start and end are in the UnicodeMap's keyset.
         */
        public int GetRangeStart(int range)
        {
            return transitions[range];
        }

        /**
         * Get the start of a range. All code points between start and end are in the UnicodeMap's keyset.
         */
        public int GetRangeEnd(int range)
        {
            return transitions[range + 1] - 1;
        }

        /**
         * Get the value for the range.
         */
        public T GetRangeValue(int range)
        {
            return values[range];
        }

        /**
         * Get the strings that are not in the ranges. Returns null if there are none.
         * @return
         */
        public ICollection<string> GetNonRangeStrings()
        {
            if (stringMap == null || stringMap.Count == 0)
            {
                return null;
            }
            return stringMap.Keys.AsReadOnly();
        }

        internal const bool DEBUG_WRITE = false;

        /* (non-Javadoc)
         * @see java.util.Map#containsKey(java.lang.Object)
         */
        public bool ContainsKey(String key)
        {
            return GetValue(key) != null;
        }

        /* (non-Javadoc)
         * @see java.util.Map#containsKey(java.lang.Object)
         */
        public bool ContainsKey(int key)
        {
            return GetValue(key) != null;
        }

        /* (non-Javadoc)
         * @see java.util.Map#containsValue(java.lang.Object)
         */
        public bool ContainsValue(T value)
        {
            // TODO Optimize
            return GetAvailableValues().Contains(value);
        }

        /* (non-Javadoc)
         * @see java.util.Map#isEmpty()
         */
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /* (non-Javadoc)
         * @see java.util.Map#putAll(java.util.Map)
         */
        public UnicodeMap<T> PutAll(IDictionary<string, T> map)
        {
            foreach (string key in map.Keys)
            {
                Put(key, map.Get(key));
            }
            return this;
        }

        /**
         * Utility for extracting map
         * @deprecated
         */
        public UnicodeMap<T> PutAllIn(IDictionary<string, T> map)
        {
            foreach (string key in KeySet())
            {
                map[key] = Get(key);
            }
            return this;
        }

        /**
         * Utility for extracting map
         */
        public IDictionary<string, T> PutAllInto(IDictionary<string, T> map)
        {
            foreach (EntryRange entry in GetEntryRanges())
            {
                if (entry.String != null)
                {
                    break;
                }
                for (int cp = entry.Codepoint; cp <= entry.CodepointEnd; ++cp)
                {
                    map[UTF16.ValueOf(cp)] = entry.Value;
                }
            }
            map.PutAll(stringMap);
            return map;
        }

        /**
         * Utility for extracting map
         */
        public IDictionary<int, T> PutAllCodepointsInto(IDictionary<int, T> map)
        {
            foreach (EntryRange entry in GetEntryRanges())
            {
                if (entry.String != null)
                {
                    break;
                }
                for (int cp = entry.Codepoint; cp <= entry.CodepointEnd; ++cp)
                {
                    map[cp] = entry.Value;
                }
            }
            return map;
        }

        /* (non-Javadoc)
         * @see java.util.Map#remove(java.lang.Object)
         */
        public UnicodeMap<T> Remove(string key)
        {
            return Put(key, null);
        }

        /* (non-Javadoc)
         * @see java.util.Map#remove(java.lang.Object)
         */
        public UnicodeMap<T> Remove(int key)
        {
            return Put(key, null);
        }

        /* (non-Javadoc)
         * @see java.util.Map#size()
         */
        public int Count

        {
            get
            {
                int result = stringMap == null ? 0 : stringMap.Count;
                for (int i = 0; i < length - 1; ++i)
                {
                    T value = values[i];
                    if (value == null) continue;
                    result += transitions[i + 1] - transitions[i];
                }
                return result;
            }
        }

        /* (non-Javadoc)
         * @see java.util.Map#entrySet()
         */
        public IEnumerable<KeyValuePair<string, T>> EntrySet()
        {
            return new EntrySetX(this);
        }

        private class EntrySetX : IEnumerable<KeyValuePair<string, T>>
        {

            internal readonly UnicodeMap<T> outerInstance;

            public EntrySetX(UnicodeMap<T> outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                return new IteratorX(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string ToString()
            {
                StringBuffer b = new StringBuffer();
                foreach (var item in this)
                {
                    b.Append(item.ToString()).Append(' ');
                }
                return b.ToString();
            }


        }

        private class IteratorX : IEnumerator<KeyValuePair<string, T>>
        {
            private readonly EntrySetX outerInstance;
            internal IEnumerator<string> iterator; // = KeySet().iterator();
            private KeyValuePair<string, T> current;

            public IteratorX(EntrySetX outerInstance)
            {
                this.outerInstance = outerInstance;

                iterator = outerInstance.outerInstance.KeySet().GetEnumerator();
            }

            public KeyValuePair<string, T> Current => current;

            object IEnumerator.Current => current;

            public void Dispose()
            {
                iterator.Dispose();
            }

            public bool MoveNext()
            {
                bool hasNext = iterator.MoveNext();
                if (hasNext)
                {
                    string key = iterator.Current;
                    current = new KeyValuePair<string, T>(key, outerInstance.outerInstance.Get(key));
                }
                else
                {
                    current = default(KeyValuePair<string, T>);
                }
                return hasNext;
            }

            public void Reset()
            {
                iterator.Reset();
            }

            ////* (non-Javadoc)
            // * @see java.util.Iterator#hasNext()
            // */
            //public bool HasNext()
            //{
            //    return iterator.hasNext();
            //}

            ////* (non-Javadoc)
            // * @see java.util.Iterator#next()
            // */
            //public Entry<String, T> next()
            //{
            //    String key = iterator.next();
            //    return new ImmutableEntry(key, get(key));
            //}

            ////* (non-Javadoc)
            // * @see java.util.Iterator#remove()
            // */
            //public void remove()
            //{
            //    throw new UnsupportedOperationException();
            //}

        }

        /**
         * Struct-like class used to iterate over a UnicodeMap in a for loop. 
         * If the value is a string, then codepoint == codepointEnd == -1. Otherwise the string is null;
         * Caution: The contents may change during the iteration!
         */
        public class EntryRange
        {
            public int Codepoint { get; set; }
            public int CodepointEnd { get; set; }
            public string String { get; set; }
            public T Value { get; set; }

            public override string ToString()
            {
                return (String != null ? Utility.Hex(String)
                        : Utility.Hex(Codepoint) + (Codepoint == CodepointEnd ? "" : ".." + Utility.Hex(CodepointEnd)))
                        + "=" + Value;
            }
        }

        /**
         * Returns an Iterable over EntryRange, designed for efficient for loops over UnicodeMaps. 
         * Caution: For efficiency, the EntryRange may be reused, so the EntryRange may change on each iteration!
         * The value is guaranteed never to be null. The entryRange.string values (non-null) are after all the ranges. 
         * @return entry range, for for loops
         */
        public IEnumerable<EntryRange> GetEntryRanges()
        {
            return new EntryRanges(this);
        }

        private class EntryRanges : IEnumerable<EntryRange>, IEnumerator<EntryRange>
        {
            private int pos;
            //private EntryRange result = new EntryRange();
            private int lastRealRange;
            private IEnumerator<KeyValuePair<string, T>> stringIterator;
            private readonly UnicodeMap<T> outerInstance;
            private EntryRange current = new EntryRange();

            public EntryRanges(UnicodeMap<T> outerInstance)
            {
                this.outerInstance = outerInstance;

                lastRealRange = outerInstance.values[outerInstance.length - 2] == null ? outerInstance.length - 2 : outerInstance.length - 1;
                stringIterator = outerInstance.stringMap == null ? (IEnumerator<KeyValuePair<string, T>>)null : outerInstance.stringMap.GetEnumerator();
            }



            public IEnumerator<EntryRange> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            public EntryRange Current => current;

            object IEnumerator.Current => current;

            public void Dispose()
            {
                if (stringIterator != null)
                    stringIterator.Dispose();
            }

            public bool MoveNext()
            {
                if (pos < lastRealRange || (stringIterator != null && stringIterator.MoveNext()))
                {
                    // a range may be null, but then the next one must not be(except the final range)
                    if (pos < lastRealRange)
                    {
                        T temp = outerInstance.values[pos];
                        if (temp == null)
                        {
                            temp = outerInstance.values[++pos];
                        }
                        current.Codepoint = outerInstance.transitions[pos];
                        current.CodepointEnd = outerInstance.transitions[pos + 1] - 1;
                        current.String = null;
                        current.Value = temp;
                        ++pos;
                    }
                    else
                    {
                        KeyValuePair<string, T> entry = stringIterator.Current;
                        current.Codepoint = current.CodepointEnd = -1;
                        current.String = entry.Key;
                        current.Value = entry.Value;
                    }
                    return true;
                }
                current = null;
                return false;
            }

            public void Reset()
            {
                stringIterator.Reset();
            }


            //public bool HasNext()
            //{
            //    return pos < lastRealRange || (stringIterator != null && stringIterator.hasNext());
            //}
            //public EntryRange<T> next()
            //{
            //    // a range may be null, but then the next one must not be (except the final range)
            //    if (pos < lastRealRange)
            //    {
            //        T temp = values[pos];
            //        if (temp == null)
            //        {
            //            temp = values[++pos];
            //        }
            //        result.codepoint = transitions[pos];
            //        result.codepointEnd = transitions[pos + 1] - 1;
            //        result.string = null;
            //        result.value = temp;
            //        ++pos;
            //    }
            //    else
            //    {
            //        Entry<String, T> entry = stringIterator.next();
            //        result.codepoint = result.codepointEnd = -1;
            //        result.string = entry.getKey();
            //        result.value = entry.getValue();
            //    }
            //    return result;
            //}
            //public void remove()
            //{
            //    throw new NotSupportedException();
            //}
        }

        /* (non-Javadoc)
         * @see java.lang.Iterable#iterator()
         */
        public IEnumerator<string> GetEnumerator()
        {
            return KeySet().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /**
         * Old form for compatibility
         */
        public T GetValue(string key)
        {
            return Get(key);
        }

        /**
         * Old form for compatibility
         */
        public T GetValue(int key)
        {
            // TODO Auto-generated method stub
            return Get(key);
        }

        /**
         * Old form for compatibility
         */
        public ICollection<T> GetAvailableValues()
        {
            return Values();
        }

        /**
         * Old form for compatibility
         */
        public ICollection<T> GetAvailableValues(ICollection<T> result)
        {
            return Values(result);
        }

        /**
         * Old form for compatibility
         */
        public UnicodeSet GetSet(T value)
        {
            return KeySet(value);
        }

        /**
         * Old form for compatibility
         */
        public UnicodeSet GetSet(T value, UnicodeSet result)
        {
            return KeySet(value, result);
        }

        // This is to support compressed serialization. It works; just commented out for now as we shift to Generics
        // TODO Fix once generics are cleaned up.
        //    // TODO Fix to serialize more than just strings.
        //    // Only if all the items are strings will we do the following compression
        //    // Otherwise we'll just use Java Serialization, bulky as it is
        //    public void writeExternal(ObjectOutput out1) throws IOException {
        //        DataOutputCompressor sc = new DataOutputCompressor(out1);
        //        // if all objects are strings
        //        Collection<T> availableVals = getAvailableValues();
        //        boolean allStrings = allAreString(availableVals);
        //        sc.writeBoolean(allStrings);
        //        Map object_index = new LinkedHashMap();
        //        if (allAreString(availableVals)) {
        //            sc.writeStringSet(new TreeSet(availableVals), object_index);
        //        } else {
        //            sc.writeCollection(availableVals, object_index);           
        //        }
        //        sc.writeUInt(length);
        //        int lastTransition = -1;
        //        int lastValueNumber = 0;
        //        if (DEBUG_WRITE) System.out.println("Trans count: " + length);
        //        for (int i = 0; i < length; ++i) {
        //            int valueNumber = ((Integer)object_index.get(values[i])).intValue();
        //            if (DEBUG_WRITE) System.out.println("Trans: " + transitions[i] + ",\t" + valueNumber);
        //
        //            int deltaTransition = transitions[i] - lastTransition;
        //            lastTransition = transitions[i];
        //            int deltaValueNumber = valueNumber - lastValueNumber;
        //            lastValueNumber = valueNumber;
        //
        //            deltaValueNumber <<= 1; // make room for one bit
        //            boolean canCombine = deltaTransition == 1;
        //            if (canCombine) deltaValueNumber |= 1;
        //            sc.writeInt(deltaValueNumber);
        //            if (DEBUG_WRITE) System.out.println("deltaValueNumber: " + deltaValueNumber);
        //            if (!canCombine) {
        //                sc.writeUInt(deltaTransition);
        //                if (DEBUG_WRITE) System.out.println("deltaTransition: " + deltaTransition);
        //            }
        //        }
        //        sc.flush();
        //    }
        //
        //    /**
        //     * 
        //     */
        //    private boolean allAreString(Collection<T> availableValues2) {
        //        //if (true) return false;
        //        for (Iterator<T> it = availableValues2.iterator(); it.hasNext();) {
        //            if (!(it.next() instanceof String)) return false;
        //        }
        //        return true;
        //    }
        //
        //    public void readExternal(ObjectInput in1) throws IOException, ClassNotFoundException {
        //        DataInputCompressor sc = new DataInputCompressor(in1);
        //        boolean allStrings = sc.readBoolean();
        //        T[] valuesList;
        //        availableValues = new LinkedHashSet();
        //        if (allStrings) {
        //            valuesList = sc.readStringSet(availableValues);
        //        } else {
        //            valuesList = sc.readCollection(availableValues);            
        //        }
        //        length = sc.readUInt();
        //        transitions = new int[length];
        //        if (DEBUG_WRITE) System.out.println("Trans count: " + length);
        //        values = (T[]) new Object[length];
        //        int currentTransition = -1;
        //        int currentValue = 0;
        //        int deltaTransition;
        //        for (int i = 0; i < length; ++i) {
        //            int temp = sc.readInt();
        //            if (DEBUG_WRITE) System.out.println("deltaValueNumber: " + temp);
        //            boolean combined = (temp & 1) != 0;
        //            temp >>= 1;
        //        values[i] = valuesList[currentValue += temp];
        //        if (!combined) {
        //            deltaTransition = sc.readUInt();
        //            if (DEBUG_WRITE) System.out.println("deltaTransition: " + deltaTransition);
        //        } else {
        //            deltaTransition = 1;
        //        }
        //        transitions[i] = currentTransition += deltaTransition; // delta value
        //        if (DEBUG_WRITE) System.out.println("Trans: " + transitions[i] + ",\t" + currentValue);
        //        }
        //    }

        public UnicodeMap<T> RemoveAll(UnicodeSet set)
        {
            return PutAll(set, null);
        }

        public UnicodeMap<T> RemoveAll(UnicodeMap<T> reference)
        {
            return RemoveRetainAll(reference, true);
        }

        public UnicodeMap<T> RetainAll(UnicodeSet set)
        {
            UnicodeSet toNuke = new UnicodeSet();
            // TODO Optimize
            foreach (EntryRange ae in GetEntryRanges())
            {
                if (ae.String != null)
                {
                    if (!set.Contains(ae.String))
                    {
                        toNuke.Add(ae.String);
                    }
                }
                else
                {
                    for (int i = ae.Codepoint; i <= ae.CodepointEnd; ++i)
                    {
                        if (!set.Contains(i))
                        {
                            toNuke.Add(i);
                        }
                    }
                }
            }
            return PutAll(toNuke, null);
        }

        public UnicodeMap<T> RetainAll(UnicodeMap<T> reference)
        {
            return RemoveRetainAll(reference, false);
        }

        private UnicodeMap<T> RemoveRetainAll(UnicodeMap<T> reference, bool remove)
        {
            UnicodeSet toNuke = new UnicodeSet();
            // TODO Optimize
            foreach (EntryRange ae in GetEntryRanges())
            {
                if (ae.String != null)
                {
                    if (ae.Value.Equals(reference.Get(ae.String)) == remove)
                    {
                        toNuke.Add(ae.String);
                    }
                }
                else
                {
                    for (int i = ae.Codepoint; i <= ae.CodepointEnd; ++i)
                    {
                        if (ae.Value.Equals(reference.Get(i)) == remove)
                        {
                            toNuke.Add(i);
                        }
                    }
                }
            }
            return PutAll(toNuke, null);
        }

        /**
         * Returns the keys that consist of multiple code points.
         * @return
         */
        public ICollection<string> StringKeys()
        {
            return GetNonRangeStrings();
        }

        /**
         * Gets the inverse of this map, adding to the target. Like putAllIn
         * @return
         */
        public IDictionary<T, UnicodeSet> AddInverseTo(IDictionary<T, UnicodeSet> target)
        {
            foreach (T value in Values())
            {
                UnicodeSet uset = GetSet(value);
                target[value] = uset;
            }
            return target;
        }

        /**
         * Freeze an inverse map.
         * @param target
         * @return
         */
        public static IDictionary<T, UnicodeSet> Freeze(IDictionary<T, UnicodeSet> target)
        {
            foreach (UnicodeSet entry in target.Values)
            {
                entry.Freeze();
            }
            return target.AsReadOnly();
        }

        /**
         * @param target
         * @return
         */
        public UnicodeMap<T> PutAllInverse(IDictionary<T, UnicodeSet> source)
        {
            foreach (var entry in source)
            {
                PutAll(entry.Value, entry.Key);
            }
            return this;
        }
    }
}
