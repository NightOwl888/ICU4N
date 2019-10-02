using ICU4N.Util;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// Simple class wrapper to store the internal byte representation of a 
    /// <see cref="CollationKey"/>. Unlike the <see cref="CollationKey"/>, this class do not contain information 
    /// on the source string the sort order represents. <see cref="RawCollationKey"/> is mutable 
    /// and users can reuse its objects with the method in 
    /// <see cref="RuleBasedCollator.GetRawCollationKey(string, RawCollationKey)"/>.
    /// </summary>
    /// <remarks>
    /// Please refer to the documentation on <see cref="CollationKey"/> for a detail description
    /// on the internal byte representation. Note the internal byte representation 
    /// is always null-terminated.
    /// <para/>
    /// <code>
    /// // Example of use:
    /// string str[] = {.....};
    /// RuleBasedCollator collator = (RuleBasedCollator)Collator.GetInstance();
    /// RawCollationKey key = new RawCollationKey(128);
    /// for (int i = 0; i &lt; str.length; i++)
    /// {
    ///     collator.GetRawCollationKey(str[i], key);
    ///     // do something with key.Bytes
    /// }
    /// </code>
    /// <para/>
    /// <strong>Note:</strong> Comparison between <see cref="RawCollationKey"/>s created by
    /// different <see cref="Collator"/>s might return incorrect results.
    /// See <see cref="Collator"/> documentation for details.
    /// </remarks>
    /// <stable>ICU 2.8</stable>
    /// <seealso cref="RuleBasedCollator"/>
    /// <seealso cref="CollationKey"/>
    // ICU4N TODO: API Update above code sample when Collator.GetInstance() is made generic
    public sealed class RawCollationKey : ByteArrayWrapper
    {
        // public constructors --------------------------------------------------

        /// <summary>
        /// Default constructor, internal byte array is null and its size set to 0.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public RawCollationKey()
        {
        }

        /// <summary>
        /// RawCollationKey created with an empty internal byte array of length 
        /// capacity. Size of the internal byte array will be set to 0.
        /// </summary>
        /// <param name="capacity">Length of internal byte array.</param>
        /// <stable>ICU 2.8</stable>
        public RawCollationKey(int capacity)
        {
            Bytes = new byte[capacity];
        }

        /// <summary>
        /// <see cref="RawCollationKey"/> created, adopting bytes as the internal byte array.
        /// <para/>
        /// Size of the internal byte array will be set to 0.
        /// </summary>
        /// <param name="bytes">Byte array to be adopted by <see cref="RawCollationKey"/>.</param>
        /// <stable>ICU 2.8</stable>
        public RawCollationKey(byte[] bytes)
        {
            this.Bytes = bytes;
        }

        /// <summary>
        /// Construct a <see cref="RawCollationKey"/> from a byte array and size.
        /// </summary>
        /// <param name="bytesToAdopt">The byte array to adopt.</param>
        /// <param name="size">The length of valid data in the byte array.</param>
        /// <exception cref="IndexOutOfRangeException">If bytesToAdopt == null and size != 0, or
        /// size &lt; 0, or size &gt; bytesToAdopt.length.</exception>
        /// <stable>ICU 2.8</stable>
        public RawCollationKey(byte[] bytesToAdopt, int size)
            : base(bytesToAdopt, size)
        {
        }

        /// <summary>
        /// Compare this <see cref="RawCollationKey"/> to another, which must not be null.  This overrides
        /// the inherited implementation to ensure the returned values are -1, 0, or 1.
        /// </summary>
        /// <param name="rhs">The <see cref="RawCollationKey"/> to compare to.</param>
        /// <returns>-1, 0, or 1 as this compares less than, equal to, or
        /// greater than rhs.</returns>
        /// <stable>ICU 4.4</stable>
        public override int CompareTo(ByteArrayWrapper rhs)
        {
            int result = base.CompareTo(rhs);
            return result < 0 ? -1 : result == 0 ? 0 : 1;
        }
    }
}
