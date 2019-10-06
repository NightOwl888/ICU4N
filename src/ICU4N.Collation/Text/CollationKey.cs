using ICU4N.Impl.Coll;
using System;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Options that used in the API <see cref="CollationKey.GetBound(CollationKeyBoundMode, CollationStrength)"/> for getting a
    /// <see cref="CollationKey"/> based on the bound mode requested.
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

    /// <summary>
    /// A <see cref="CollationKey"/> represents a <see cref="string"/>
    /// under the rules of a specific <see cref="Collator"/> object.
    /// Comparing two <see cref="CollationKey"/>s returns the
    /// relative order of the <see cref="string"/>s they represent.
    /// </summary>
    /// <remarks>
    /// Since the rule set of <see cref="Collator"/>s can differ, the
    /// sort orders of the same string under two different
    /// <see cref="Collator"/>s might differ.  Hence comparing
    /// <see cref="CollationKey"/>s generated from different
    /// <see cref="Collator"/>s can give incorrect results.
    /// <para/>
    /// Both the method
    /// <see cref="CollationKey.CompareTo(CollationKey)"/> and the method
    /// <see cref="Collator.Compare(string, string)"/> compare two strings
    /// and returns their relative order.  The performance characteristics
    /// of these two approaches can differ.
    /// Note that collation keys are often less efficient than simply doing comparison.
    /// For more details, see the ICU User Guide.
    /// <para/>
    /// During the construction of a <see cref="CollationKey"/>, the
    /// entire source string is examined and processed into a series of
    /// bits terminated by a null, that are stored in the <see cref="CollationKey"/>.
    /// When <see cref="CollationKey.CompareTo(CollationKey)"/> executes, it
    /// performs bitwise comparison on the bit sequences.  This can incur
    /// startup cost when creating the <see cref="CollationKey"/>, but once
    /// the key is created, binary comparisons are fast.  This approach is
    /// recommended when the same strings are to be compared over and over
    /// again.
    /// <para/>
    /// On the other hand, implementations of
    /// <see cref="Collator.Compare(string, string)"/> can examine and
    /// process the strings only until the first characters differing in
    /// order.  This approach is recommended if the strings are to be
    /// compared only once.
    /// <para/>
    /// More information about the composition of the bit sequence can
    /// be found in the
    /// <a href="http://www.icu-project.org/userguide/Collate_ServiceArchitecture.html">
    /// user guide</a>.
    /// <para/>
    /// The following example shows how <see cref="CollationKey"/>s can be used
    /// to sort a list of <see cref="string"/>s.
    /// 
    /// <code>
    /// // Create an array of CollationKeys for the Strings to be sorted.
    /// Collator myCollator = Collator.GetInstance();
    /// CollationKey[] keys = new CollationKey[3];
    /// keys[0] = myCollator.GetCollationKey("Tom");
    /// keys[1] = myCollator.GetCollationKey("Dick");
    /// keys[2] = myCollator.GetCollationKey("Harry");
    /// Sort( keys );
    /// 
    /// //...
    /// 
    /// // Inside body of sort routine, compare keys this way
    /// if( keys[i].CompareTo( keys[j] ) &gt; 0 )
    ///     // swap keys[i] and keys[j]
    ///     
    /// //...
    /// 
    /// // Finally, when we've returned from sort.
    /// Console.WriteLine(keys[0].SourceString);
    /// Console.WriteLine(keys[1].SourceString);
    /// Console.WriteLine(keys[2].SourceString);
    /// </code>
    /// <para/>
    /// This class is not subclassable
    /// </remarks>
    /// <seealso cref="Collator"/>
    /// <seealso cref="RuleBasedCollator"/>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.8</stable>
    public sealed class CollationKey : IComparable<CollationKey>, IComparable
    {
        // public inner classes -------------------------------------------------

        // ICU4N specific - boundmode de-nested, made into Enum, and renamed CollationKeyBoundMode

        // public constructor ---------------------------------------------------

        /// <summary>
        /// <see cref="CollationKey"/> constructor.
        /// This constructor is given public access, unlike the <c>System.Globalization.SortKey</c>, to
        /// allow access to users extending the <see cref="Collator"/> class. See
        /// <see cref="Collator.GetCollationKey(string)"/>.
        /// </summary>
        /// <param name="source">String this <see cref="CollationKey"/> is to represent.</param>
        /// <param name="key">Array of bytes that represent the collation order of argument
        /// source terminated by a null.</param>
        /// <seealso cref="Collator"/>
        /// <stable>ICU 2.8</stable>
        public CollationKey(string source, byte[] key)
            : this(source, key, -1)
        {
        }

        /// <summary>
        /// Private constructor, takes a <paramref name="length"/> argument so it need not be lazy-evaluated.
        /// There must be a 00 byte at <paramref name="key"/>[<paramref name="length"/>] and none before.
        /// </summary>
        private CollationKey(string source, byte[] key, int length)
        {
            m_source_ = source;
            m_key_ = key;
            m_hashCode_ = 0;
            m_length_ = length;
        }

        /// <summary>
        /// <see cref="CollationKey"/> constructor that forces key to release its internal byte
        /// array for adoption. key will have a null byte array after this
        /// construction.
        /// </summary>
        /// <param name="source">String this <see cref="CollationKey"/> is to represent.</param>
        /// <param name="key"><see cref="RawCollationKey"/> object that represents the collation order of
        /// argument source.</param>
        /// <stable>ICU 2.8</stable>
        public CollationKey(string source, RawCollationKey key)
        {
            m_source_ = source;
            m_length_ = key.Length - 1;
            m_key_ = key.ReleaseBytes();
            Debug.Assert(m_key_[m_length_] == 0);
            m_hashCode_ = 0;
        }

        // public getters -------------------------------------------------------

        /// <summary>
        /// Gets the source string that this <see cref="CollationKey"/> represents.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public string SourceString
        {
            get { return m_source_; }
        }

        /// <summary>
        /// Duplicates and returns the value of this <see cref="CollationKey"/> as a sequence
        /// of big-endian bytes terminated by a null.
        /// </summary>
        /// <remarks>
        /// If two <see cref="CollationKey"/>s can be legitimately compared, then one can
        /// compare the byte arrays of each to obtain the same result, e.g.
        /// <code>
        /// byte key1[] = collationkey1.ToByteArray();
        /// byte key2[] = collationkey2.ToByteArray();
        /// int key, targetkey;
        /// int i = 0;
        /// do
        /// {
        ///     key = key1[i] &amp; 0xFF;
        ///     targetkey = key2[i] &amp; 0xFF;
        ///     if (key &lt; targetkey)
        ///     {
        ///         Console.WriteLine("String 1 is less than string 2");
        ///         return;
        ///     }
        ///     if (targetkey &lt; key)
        ///     {
        ///         Console.WriteLine("String 1 is more than string 2");
        ///     }
        ///     i++;
        /// } while (key != 0 &amp;&amp; targetKey != 0);
        /// 
        /// Console.WriteLine("Strings are equal.");
        /// </code>
        /// </remarks>
        /// <returns>
        /// <see cref="CollationKey"/> value in a sequence of big-endian byte bytes
        /// terminated by a null.
        /// </returns>
        /// <stable>ICU 2.8</stable>
        public byte[] ToByteArray()
        {
            int length = GetLength() + 1;
            byte[] result = new byte[length];
            System.Array.Copy(m_key_, 0, result, 0, length);
            return result;
        }

        // public other methods -------------------------------------------------

        /// <summary>
        /// Compare this <see cref="CollationKey"/> to another <see cref="CollationKey"/>.  The
        /// collation rules of the <see cref="Collator"/> that created this key are
        /// applied.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Comparison between <see cref="CollationKey"/>s
        /// created by different <see cref="Collator"/>s might return incorrect
        /// results.  See class documentation.
        /// </remarks>
        /// <param name="target">Target <see cref="CollationKey"/>.</param>
        /// <returns>
        /// An integer value.  If the value is less than zero this CollationKey
        /// is less than than target, if the value is zero they are equal, and
        /// if the value is greater than zero this <see cref="CollationKey"/> is greater
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
        /// Compare this <see cref="CollationKey"/> to another <see cref="CollationKey"/>.  The
        /// collation rules of the <see cref="Collator"/> that created this key are
        /// applied.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Comparison between <see cref="CollationKey"/>s
        /// created by different <see cref="Collator"/>s might return incorrect
        /// results.  See class documentation.
        /// </remarks>
        /// <param name="other">Target <see cref="CollationKey"/>.</param>
        /// <returns>
        /// An integer value.  If the value is less than zero this CollationKey
        /// is less than than target, if the value is zero they are equal, and
        /// if the value is greater than zero this <see cref="CollationKey"/> is greater
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

        /// <summary>
        /// Compare this <see cref="CollationKey"/> and the specified <see cref="object"/> for
        /// equality.  The collation rules of the <see cref="Collator"/> that created
        /// this key are applied.
        /// <para/>
        /// See note in <see cref="CompareTo(CollationKey)"/> for warnings about
        /// possible incorrect results.
        /// </summary>
        /// <param name="target">The object to compare to.</param>
        /// <returns>true if the two keys compare as equal, false otherwise.</returns>
        /// <seealso cref="CompareTo(CollationKey)"/>
        /// <seealso cref="CompareTo(object)"/>
        public override bool Equals(object target) // ICU4N specific - no exceptions thrown (if null or not a CollationKey, returns false)
        {
            if (!(target is CollationKey)) {
                return false;
            }

            return Equals((CollationKey)target);
        }

        /// <summary>
        /// Compare this <see cref="CollationKey"/> and the argument target <see cref="CollationKey"/> for
        /// equality.
        /// The collation
        /// rules of the <see cref="Collator"/> object which created these objects are applied.
        /// <para/>
        /// See note in <see cref="CompareTo(CollationKey)"/> for warnings of incorrect results
        /// </summary>
        /// <param name="target">The <see cref="CollationKey"/> to compare to.</param>
        /// <returns>true if two objects are equal, false otherwise.</returns>
        /// <stable>ICU 2.8</stable>
        public bool Equals(CollationKey target) // ICU4N specific - no exceptions thrown (if null or not a CollationKey, returns false)
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

        /// <summary>
        /// Returns a hash code for this CollationKey. The hash value is calculated
        /// on the key itself, not the string from which the key was created. Thus
        /// if x and y are <see cref="CollationKey"/>s, then x.GetHashCode(x) == y.GetHashCode()
        /// if x.Equals(y) is true. This allows language-sensitive comparison in a
        /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>The hash value.</returns>
        /// <stable>ICU 2.8</stable>
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

        /// <summary>
        /// Produces a bound for the sort order of a given collation key and a
        /// strength level. This API does not attempt to find a bound for the
        /// <see cref="CollationKey"/> string representation, hence null will be returned in its
        /// place.
        /// </summary>
        /// <remarks>
        /// Resulting bounds can be used to produce a range of strings that are
        /// between upper and lower bounds. For example, if bounds are produced
        /// for a sortkey of string "smith", strings between upper and lower
        /// bounds with primary strength would include "Smith", "SMITH", "sMiTh".
        /// <para/>
        /// There are two upper bounds that can be produced. If <see cref="CollationKeyBoundMode.Upper"/>
        /// is produced, strings matched would be as above. However, if a bound
        /// is produced using <see cref="CollationKeyBoundMode.UpperLong"/> is used, the above example will
        /// also match "Smithsonian" and similar.
        /// <para/>
        /// For more on usage, see example in test procedure
        /// <a href="http://source.icu-project.org/repos/icu/icu4j/trunk/src/com/ibm/icu/dev/test/collator/CollationAPITest.java">
        /// src/com/ibm/icu/dev/test/collator/CollationAPITest/TestBounds.
        /// </a>
        /// <para/>
        /// Collation keys produced may be compared using the <see cref="CollationKey.CompareTo(CollationKey)"/> API.
        /// </remarks>
        /// <param name="boundType">
        /// Mode of bound required. It can be <see cref="CollationKeyBoundMode.Lower"/>, which
        /// produces a lower inclusive bound, <see cref="CollationKeyBoundMode.Upper"/>, that
        /// produces upper bound that matches strings of the same
        /// length or <see cref="CollationKeyBoundMode.UpperLong"/> that matches strings that
        /// have the same starting substring as the source string.
        /// </param>
        /// <param name="noOfLevels">
        /// Strength levels required in the resulting bound
        /// (for most uses, the recommended value is <see cref="CollationStrength.Primary"/>). This
        /// strength should be less than the maximum strength of
        /// this <see cref="CollationKey"/>.
        /// See users guide for explanation on the strength levels a
        /// collation key can have.
        /// </param>
        /// <returns>
        /// The result bounded <see cref="CollationKey"/> with a valid sort order but
        /// a null string representation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// thrown when the strength level
        /// requested is higher than or equal to the strength in this
        /// <see cref="CollationKey"/>.
        /// In the case of an Exception, information
        /// about the maximum strength to use will be returned in the
        /// Exception. The user can then call <see cref="GetBound(CollationKeyBoundMode, CollationStrength)"/> again with the
        /// appropriate strength.
        /// </exception>
        /// <seealso cref="CollationKey"/>
        /// <seealso cref="CollationKeyBoundMode"/>
        /// <seealso cref="CollationStrength.Primary"/>
        /// <seealso cref="CollationStrength.Secondary"/>
        /// <seealso cref="CollationStrength.Tertiary"/>
        /// <seealso cref="CollationStrength.Quaternary"/>
        /// <seealso cref="CollationStrength.Identical"/>
        /// <stable>ICU 2.6</stable>
        // ICU4N TODO: Update documentation to point to .NET CollationAPITest class
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
                            == Collation.LevelSeparatorByte)
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
                                      + " strength level. Call GetBound() again "
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

        /// <summary>
        /// Merges this <see cref="CollationKey"/> with another.
        /// The levels are merged with their corresponding counterparts
        /// (primaries with primaries, secondaries with secondaries etc.).
        /// Between the values from the same level a separator is inserted.
        /// </summary>
        /// <remarks>
        /// This is useful, for example, for combining sort keys from first and last names
        /// to sort such pairs.
        /// See <a href="http://www.unicode.org/reports/tr10/#Merging_Sort_Keys">http://www.unicode.org/reports/tr10/#Merging_Sort_Keys</a>.
        /// <para/>
        /// The recommended way to achieve "merged" sorting is by
        /// concatenating strings with U+FFFE between them.
        /// The concatenation has the same sort order as the merged sort keys,
        /// but Merge(GetCollationKey(str1), GetCollationKey(str2)) may differ from GetCollationKey(str1 + '\uFFFE' + str2).
        /// Using strings with U+FFFE may yield shorter sort keys.
        /// <para/>
        /// For details about Sort Key Features see
        /// <a href="http://userguide.icu-project.org/collation/api#TOC-Sort-Key-Features">http://userguide.icu-project.org/collation/api#TOC-Sort-Key-Features</a>.
        /// <para/>
        /// It is possible to merge multiple sort keys by consecutively merging
        /// another one with the intermediate result.
        /// <para/>
        /// Only the sort key bytes of the <see cref="CollationKey"/>s are merged.
        /// This API does not attempt to merge the
        /// string representations of the <see cref="CollationKey"/>s, hence null will be returned
        /// as the result's string representation.
        /// <para/>
        /// Example (uncompressed):
        /// <code>
        /// 191B1D 01 050505 01 910505 00
        /// 1F2123 01 050505 01 910505 00
        /// </code>
        /// will be merged as
        /// <code>
        /// 191B1D 02 1F2123 01 050505 02 050505 01 910505 02 910505 00
        /// </code>
        /// </remarks>
        /// <param name="source"><see cref="CollationKey"/> to merge with.</param>
        /// <returns>A <see cref="CollationKey"/> that contains the valid merged sort keys
        /// with a null String representation,
        /// i.e. <c>new CollationKey(null, merged_sort_keys)</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if source <see cref="CollationKey"/>
        /// argument is null or of 0 length.</exception>
        /// <stable>ICU 2.6</stable>
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
                if (m_key_[index] == Collation.LevelSeparatorByte
                    && source.m_key_[sourceindex]
                            == Collation.LevelSeparatorByte)
                {
                    ++index;
                    ++sourceindex;
                    result[rindex++] = Collation.LevelSeparatorByte;
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

        /// <summary>
        /// Sequence of bytes that represents the sort key
        /// </summary>
        private byte[] m_key_;

        /// <summary>
        /// Source string this <see cref="CollationKey"/> represents
        /// </summary>
        private string m_source_;

        /// <summary>
        /// Hash code for the key
        /// </summary>
        private int m_hashCode_;
        /// <summary>
        /// Gets the length of this <see cref="CollationKey"/>
        /// </summary>
        private int m_length_;
        /// <summary>
        /// Collation key merge seperator
        /// </summary>
        private static readonly byte MERGE_SEPERATOR_ = 2;

        // private methods ------------------------------------------------------

        /// <summary>
        /// Gets the length of the <see cref="CollationKey"/>.
        /// </summary>
        /// <returns>Length of the <see cref="CollationKey"/>.</returns>
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
