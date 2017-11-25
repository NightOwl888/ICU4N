using ICU4N.Impl.Coll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Options that used in the API <see cref="CollationKey.GetBound(int, int)"/> for getting a
    /// CollationKey based on the bound mode requested.
    /// </summary>
    /// <stable>ICU 2.6</stable>
    // do not change the values assigned to the members of this enum.
    // Underlying code depends on them having these numbers
    public enum CollationKeyBoundMode
    {
        /// <summary>
        /// Lower bound
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Lower = 0,
        /// <summary>
        /// Upper bound that will match strings of exact size.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Upper = 1,
        /// <summary>
        /// Upper bound that will match all the strings that have the same
        /// initial substring as the given string.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        UpperLong = 2,
    }

    public sealed class CollationKey : IComparable<CollationKey>, IComparable
    {
        // public inner classes -------------------------------------------------

        ///**
        // * Options that used in the API CollationKey.getBound() for getting a
        // * CollationKey based on the bound mode requested.
        // * @stable ICU 2.6
        // */
        //public static final class BoundMode
        //{
        //    /*
        //     * do not change the values assigned to the members of this enum.
        //     * Underlying code depends on them having these numbers
        //     */

        //    /**
        //     * Lower bound
        //     * @stable ICU 2.6
        //     */
        //    public static final int LOWER = 0;

        //    /**
        //     * Upper bound that will match strings of exact size
        //     * @stable ICU 2.6
        //     */
        //    public static final int UPPER = 1;

        //    /**
        //     * Upper bound that will match all the strings that have the same
        //     * initial substring as the given string
        //     * @stable ICU 2.6
        //     */
        //    public static final int UPPER_LONG = 2;

        //    /**
        //     * One more than the highest normal BoundMode value.
        //     * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
        //     */
        //    @Deprecated
        //    public static final int COUNT = 3;

        //    /**
        //     * Private Constructor
        //     */
        //    ///CLOVER:OFF
        //    private BoundMode() { }
        //    ///CLOVER:ON
        //}

        // public constructor ---------------------------------------------------

        /**
         * CollationKey constructor.
         * This constructor is given public access, unlike the JDK version, to
         * allow access to users extending the Collator class. See
         * {@link Collator#getCollationKey(String)}.
         * @param source string this CollationKey is to represent
         * @param key array of bytes that represent the collation order of argument
         *            source terminated by a null
         * @see Collator
         * @stable ICU 2.8
         */
        public CollationKey(string source, byte[] key)
            : this(source, key, -1)
        {
        }

        /**
         * Private constructor, takes a length argument so it need not be lazy-evaluated.
         * There must be a 00 byte at key[length] and none before.
         */
        private CollationKey(string source, byte[] key, int length)
        {
            m_source_ = source;
            m_key_ = key;
            m_hashCode_ = 0;
            m_length_ = length;
        }

        /**
         * CollationKey constructor that forces key to release its internal byte
         * array for adoption. key will have a null byte array after this
         * construction.
         * @param source string this CollationKey is to represent
         * @param key RawCollationKey object that represents the collation order of
         *            argument source.
         * @see Collator
         * @see RawCollationKey
         * @stable ICU 2.8
         */
        public CollationKey(string source, RawCollationKey key)
        {
            m_source_ = source;
            m_length_ = key.Count - 1;
            m_key_ = key.ReleaseBytes();
            Debug.Assert(m_key_[m_length_] == 0);
            m_hashCode_ = 0;
        }

        // public getters -------------------------------------------------------

        /**
         * Return the source string that this CollationKey represents.
         * @return source string that this CollationKey represents
         * @stable ICU 2.8
         */
        public string SourceString
        {
            get { return m_source_; }
        }

        /**
         * Duplicates and returns the value of this CollationKey as a sequence
         * of big-endian bytes terminated by a null.
         *
         * <p>If two CollationKeys can be legitimately compared, then one can
         * compare the byte arrays of each to obtain the same result, e.g.
         * <pre>
         * byte key1[] = collationkey1.toByteArray();
         * byte key2[] = collationkey2.toByteArray();
         * int key, targetkey;
         * int i = 0;
         * do {
         *       key = key1[i] &amp; 0xFF;
         *     targetkey = key2[i] &amp; 0xFF;
         *     if (key &lt; targetkey) {
         *         System.out.println("String 1 is less than string 2");
         *         return;
         *     }
         *     if (targetkey &lt; key) {
         *         System.out.println("String 1 is more than string 2");
         *     }
         *     i ++;
         * } while (key != 0 &amp;&amp; targetKey != 0);
         *
         * System.out.println("Strings are equal.");
         * </pre>
         *
         * @return CollationKey value in a sequence of big-endian byte bytes
         *         terminated by a null.
         * @stable ICU 2.8
         */
        public byte[] ToByteArray()
        {
            int length = GetLength() + 1;
            byte[] result = new byte[length];
            System.Array.Copy(m_key_, 0, result, 0, length);
            return result;
        }

        // public other methods -------------------------------------------------

        /// <summary>
        /// Compare this CollationKey to another CollationKey.  The
        /// collation rules of the Collator that created this key are
        /// applied.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Comparison between CollationKeys
        /// created by different Collators might return incorrect
        /// results.  See class documentation.
        /// </remarks>
        /// <param name="target">Target <see cref="CollationKey"/>.</param>
        /// <returns>
        /// An integer value.  If the value is less than zero this CollationKey
        /// is less than than target, if the value is zero they are equal, and
        /// if the value is greater than zero this CollationKey is greater
        /// than target.
        /// </returns>
        /// <exception cref="ArgumentNullException">is thrown if argument is null.</exception>
        /// <seealso cref="Collator.Compare(string, string)"/>
        /// <stable>ICU 2.8</stable>
        public int CompareTo(CollationKey target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            for (int i = 0; ; ++i)
            {
                int l = m_key_[i] & 0xff;
                int r = target.m_key_[i] & 0xff;
                if (l < r)
                {
                    return -1;
                }
                else if (l > r)
                {
                    return 1;
                }
                else if (l == 0)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Compare this CollationKey to another CollationKey.  The
        /// collation rules of the Collator that created this key are
        /// applied.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Comparison between CollationKeys
        /// created by different Collators might return incorrect
        /// results.  See class documentation.
        /// </remarks>
        /// <param name="target">Target <see cref="CollationKey"/>.</param>
        /// <returns>
        /// An integer value.  If the value is less than zero this CollationKey
        /// is less than than target, if the value is zero they are equal, and
        /// if the value is greater than zero this CollationKey is greater
        /// than target.
        /// </returns>
        /// <exception cref="ArgumentNullException">is thrown if argument is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="other"/> cannot be cast to <see cref="CollationKey"/>.</exception>
        /// <seealso cref="Collator.Compare(string, string)"/>
        /// <stable>ICU4N 60.1</stable>
        // ICU4N specific overload to handle non-generic IComparable
        public int CompareTo(object other)
        {
            if (other is CollationKey)
                return CompareTo((CollationKey)other);
            throw new ArgumentException("'other' must be a CollationKey.");
        }

        /**
         * Compare this CollationKey and the specified Object for
         * equality.  The collation rules of the Collator that created
         * this key are applied.
         *
         * <p>See note in compareTo(CollationKey) for warnings about
         * possible incorrect results.
         *
         * @param target the object to compare to.
         * @return true if the two keys compare as equal, false otherwise.
         * @see #compareTo(CollationKey)
         * @exception ClassCastException is thrown when the argument is not
         *            a CollationKey.  NullPointerException is thrown when the argument
         *            is null.
         * @stable ICU 2.8
         */
        public override bool Equals(object target)
        {
            if (!(target is CollationKey)) {
                return false;
            }

            return Equals((CollationKey)target);
        }

        /**
         * Compare this CollationKey and the argument target CollationKey for
         * equality.
         * The collation
         * rules of the Collator object which created these objects are applied.
         * <p>
         * See note in compareTo(CollationKey) for warnings of incorrect results
         *
         * @param target the CollationKey to compare to.
         * @return true if two objects are equal, false otherwise.
         * @exception NullPointerException is thrown when the argument is null.
         * @stable ICU 2.8
         */
        public bool Equals(CollationKey target)
        {
            if (this == target)
            {
                return true;
            }
            if (target == null)
            {
                return false;
            }
            CollationKey other = target;
            int i = 0;
            while (true)
            {
                if (m_key_[i] != other.m_key_[i])
                {
                    return false;
                }
                if (m_key_[i] == 0)
                {
                    break;
                }
                i++;
            }
            return true;
        }

        /**
         * Returns a hash code for this CollationKey. The hash value is calculated
         * on the key itself, not the String from which the key was created. Thus
         * if x and y are CollationKeys, then x.hashCode(x) == y.hashCode()
         * if x.equals(y) is true. This allows language-sensitive comparison in a
         * hash table.
         *
         * @return the hash value.
         * @stable ICU 2.8
         */
        public override int GetHashCode()
        {
            if (m_hashCode_ == 0)
            {
                if (m_key_ == null)
                {
                    m_hashCode_ = 1;
                }
                else
                {
                    int size = m_key_.Length >> 1;
                    StringBuilder key = new StringBuilder(size);
                    int i = 0;
                    while (m_key_[i] != 0 && m_key_[i + 1] != 0)
                    {
                        key.Append((char)((m_key_[i] << 8) | (0xff & m_key_[i + 1])));
                        i += 2;
                    }
                    if (m_key_[i] != 0)
                    {
                        key.Append((char)(m_key_[i] << 8));
                    }
                    m_hashCode_ = key.ToString().GetHashCode();
                }
            }
            return m_hashCode_;
        }

        /**
         * Produces a bound for the sort order of a given collation key and a
         * strength level. This API does not attempt to find a bound for the
         * CollationKey String representation, hence null will be returned in its
         * place.
         * <p>
         * Resulting bounds can be used to produce a range of strings that are
         * between upper and lower bounds. For example, if bounds are produced
         * for a sortkey of string "smith", strings between upper and lower
         * bounds with primary strength would include "Smith", "SMITH", "sMiTh".
         * <p>
         * There are two upper bounds that can be produced. If BoundMode.UPPER
         * is produced, strings matched would be as above. However, if a bound
         * is produced using BoundMode.UPPER_LONG is used, the above example will
         * also match "Smithsonian" and similar.
         * <p>
         * For more on usage, see example in test procedure
         * <a href="http://source.icu-project.org/repos/icu/icu4j/trunk/src/com/ibm/icu/dev/test/collator/CollationAPITest.java">
         * src/com/ibm/icu/dev/test/collator/CollationAPITest/TestBounds.
         * </a>
         * <p>
         * Collation keys produced may be compared using the <TT>compare</TT> API.
         * @param boundType Mode of bound required. It can be BoundMode.LOWER, which
         *              produces a lower inclusive bound, BoundMode.UPPER, that
         *              produces upper bound that matches strings of the same
         *              length or BoundMode.UPPER_LONG that matches strings that
         *              have the same starting substring as the source string.
         * @param noOfLevels Strength levels required in the resulting bound
         *                 (for most uses, the recommended value is PRIMARY). This
         *                 strength should be less than the maximum strength of
         *                 this CollationKey.
         *                 See users guide for explanation on the strength levels a
         *                 collation key can have.
         * @return the result bounded CollationKey with a valid sort order but
         *         a null String representation.
         * @exception IllegalArgumentException thrown when the strength level
         *            requested is higher than or equal to the strength in this
         *            CollationKey.
         *            In the case of an Exception, information
         *            about the maximum strength to use will be returned in the
         *            Exception. The user can then call getBound() again with the
         *            appropriate strength.
         * @see CollationKey
         * @see CollationKey.BoundMode
         * @see Collator#PRIMARY
         * @see Collator#SECONDARY
         * @see Collator#TERTIARY
         * @see Collator#QUATERNARY
         * @see Collator#IDENTICAL
         * @stable ICU 2.6
         */
        public CollationKey GetBound(CollationKeyBoundMode boundType, CollationStrength noOfLevels) 
        {
            // Scan the string until we skip enough of the key OR reach the end of
            // the key
            int offset = 0;
            CollationStrength keystrength = CollationStrength.Primary;

            if (noOfLevels > CollationStrength.Primary)
            {
                while (offset < m_key_.Length && m_key_[offset] != 0)
                {
                    if (m_key_[offset++]
                            == Collation.LEVEL_SEPARATOR_BYTE)
                    {
                        keystrength++;
                        noOfLevels--;
                        if (noOfLevels == CollationStrength.Primary
                            || offset == m_key_.Length || m_key_[offset] == 0)
                        {
                            offset--;
                            break;
                        }
                    }
                }
            }

            if (noOfLevels > 0)
            {
                throw new ArgumentException(
                                      "Source collation key has only "
                                      + keystrength
                                      + " strength level. Call getBound() again "
                                      + " with noOfLevels < " + keystrength);
            }

            // READ ME: this code assumes that the values for BoundMode variables
            // will not change. They are set so that the enum value corresponds to
            // the number of extra bytes each bound type needs.
            byte[] resultkey = new byte[offset + (int)boundType + 1];
            System.Array.Copy(m_key_, 0, resultkey, 0, offset);
            switch (boundType)
            {
                case CollationKeyBoundMode.Lower: // = 0
                                      // Lower bound just gets terminated. No extra bytes
                    break;
                case CollationKeyBoundMode.Upper: // = 1
                                      // Upper bound needs one extra byte
                    resultkey[offset++] = 2;
                    break;
                case CollationKeyBoundMode.UpperLong: // = 2
                                           // Upper long bound needs two extra bytes
                    resultkey[offset++] = (byte)0xFF;
                    resultkey[offset++] = (byte)0xFF;
                    break;
                default:
                    throw new ArgumentException(
                                                    "Illegal boundType argument");
            }
            resultkey[offset] = 0;
            return new CollationKey(null, resultkey, offset);
        }


        /**
         * Merges this CollationKey with another.
         * The levels are merged with their corresponding counterparts
         * (primaries with primaries, secondaries with secondaries etc.).
         * Between the values from the same level a separator is inserted.
         *
         * <p>This is useful, for example, for combining sort keys from first and last names
         * to sort such pairs.
         * See http://www.unicode.org/reports/tr10/#Merging_Sort_Keys
         *
         * <p>The recommended way to achieve "merged" sorting is by
         * concatenating strings with U+FFFE between them.
         * The concatenation has the same sort order as the merged sort keys,
         * but merge(getSortKey(str1), getSortKey(str2)) may differ from getSortKey(str1 + '\uFFFE' + str2).
         * Using strings with U+FFFE may yield shorter sort keys.
         *
         * <p>For details about Sort Key Features see
         * http://userguide.icu-project.org/collation/api#TOC-Sort-Key-Features
         *
         * <p>It is possible to merge multiple sort keys by consecutively merging
         * another one with the intermediate result.
         *
         * <p>Only the sort key bytes of the CollationKeys are merged.
         * This API does not attempt to merge the
         * String representations of the CollationKeys, hence null will be returned
         * as the result's String representation.
         *
         * <p>Example (uncompressed):
         * <pre>191B1D 01 050505 01 910505 00
         * 1F2123 01 050505 01 910505 00</pre>
         * will be merged as
         * <pre>191B1D 02 1F2123 01 050505 02 050505 01 910505 02 910505 00</pre>
         *
         * @param source CollationKey to merge with
         * @return a CollationKey that contains the valid merged sort keys
         *         with a null String representation,
         *         i.e. <tt>new CollationKey(null, merged_sort_keys)</tt>
         * @exception IllegalArgumentException thrown if source CollationKey
         *            argument is null or of 0 length.
         * @stable ICU 2.6
         */
        public CollationKey Merge(CollationKey source)
        {
            // check arguments
            if (source == null || source.GetLength() == 0)
            {
                throw new ArgumentException(
                          "CollationKey argument can not be null or of 0 length");
            }

            // 1 byte extra for the 02 separator at the end of the copy of this sort key,
            // and 1 more for the terminating 00.
            byte[] result = new byte[GetLength() + source.GetLength() + 2];

            // merge the sort keys with the same number of levels
            int rindex = 0;
            int index = 0;
            int sourceindex = 0;
            while (true)
            {
                // copy level from src1 not including 00 or 01
                // unsigned issues
                while (m_key_[index] < 0 || m_key_[index] >= MERGE_SEPERATOR_)
                {
                    result[rindex++] = m_key_[index++];
                }

                // add a 02 merge separator
                result[rindex++] = MERGE_SEPERATOR_;

                // copy level from src2 not including 00 or 01
                while (source.m_key_[sourceindex] < 0
                       || source.m_key_[sourceindex] >= MERGE_SEPERATOR_)
                {
                    result[rindex++] = source.m_key_[sourceindex++];
                }

                // if both sort keys have another level, then add a 01 level
                // separator and continue
                if (m_key_[index] == Collation.LEVEL_SEPARATOR_BYTE
                    && source.m_key_[sourceindex]
                            == Collation.LEVEL_SEPARATOR_BYTE)
                {
                    ++index;
                    ++sourceindex;
                    result[rindex++] = Collation.LEVEL_SEPARATOR_BYTE;
                }
                else
                {
                    break;
                }
            }

            // here, at least one sort key is finished now, but the other one
            // might have some contents left from containing more levels;
            // that contents is just appended to the result
            int remainingLength;
            if ((remainingLength = m_length_ - index) > 0)
            {
                System.Array.Copy(m_key_, index, result, rindex, remainingLength);
                rindex += remainingLength;
            }
            else if ((remainingLength = source.m_length_ - sourceindex) > 0)
            {
                System.Array.Copy(source.m_key_, sourceindex, result, rindex, remainingLength);
                rindex += remainingLength;
            }
            result[rindex] = 0;

            Debug.Assert( rindex == result.Length - 1);
            return new CollationKey(null, result, rindex);
        }

        // private data members -------------------------------------------------

        /**
         * Sequence of bytes that represents the sort key
         */
        private byte[] m_key_;

        /**
         * Source string this CollationKey represents
         */
        private string m_source_;

        /**
         * Hash code for the key
         */
        private int m_hashCode_;
        /**
         * Gets the length of this CollationKey
         */
        private int m_length_;
        /**
         * Collation key merge seperator
         */
        private static readonly byte MERGE_SEPERATOR_ = 2;

        // private methods ------------------------------------------------------

        /**
         * Gets the length of the CollationKey
         * @return length of the CollationKey
         */
        private int GetLength()
        {
            if (m_length_ >= 0)
            {
                return m_length_;
            }
            int length = m_key_.Length;
            for (int index = 0; index < length; index++)
            {
                if (m_key_[index] == 0)
                {
                    length = index;
                    break;
                }
            }
            m_length_ = length;
            return m_length_;
        }
    }
}
