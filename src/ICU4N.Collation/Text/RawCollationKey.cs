using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public sealed class RawCollationKey : ByteArrayWrapper
    {
        // public constructors --------------------------------------------------

        /**
         * Default constructor, internal byte array is null and its size set to 0.
         * @stable ICU 2.8
         */
        public RawCollationKey()
        {
        }

        /**
         * RawCollationKey created with an empty internal byte array of length 
         * capacity. Size of the internal byte array will be set to 0.
         * @param capacity length of internal byte array
         * @stable ICU 2.8
         */
        public RawCollationKey(int capacity)
        {
            Bytes = new byte[capacity];
        }

        /**
         * RawCollationKey created, adopting bytes as the internal byte array.
         * Size of the internal byte array will be set to 0.
         * @param bytes byte array to be adopted by RawCollationKey
         * @stable ICU 2.8
         */
        public RawCollationKey(byte[] bytes)
        {
            this.Bytes = bytes;
        }

        /**
         * Construct a RawCollationKey from a byte array and size.
         * @param bytesToAdopt the byte array to adopt
         * @param size the length of valid data in the byte array
         * @throws IndexOutOfBoundsException if bytesToAdopt == null and size != 0, or
         * size &lt; 0, or size &gt; bytesToAdopt.length.
         * @stable ICU 2.8
         */
        public RawCollationKey(byte[] bytesToAdopt, int size)
            : base(bytesToAdopt, size)
        {
        }

        /**
         * Compare this RawCollationKey to another, which must not be null.  This overrides
         * the inherited implementation to ensure the returned values are -1, 0, or 1.
         * @param rhs the RawCollationKey to compare to.
         * @return -1, 0, or 1 as this compares less than, equal to, or
         * greater than rhs.
         * @throws ClassCastException if the other object is not a RawCollationKey.
         * @stable ICU 4.4
         */
        public override int CompareTo(ByteArrayWrapper rhs)
        {
            int result = base.CompareTo(rhs);
            return result < 0 ? -1 : result == 0 ? 0 : 1;
        }
    }
}
