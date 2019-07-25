using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// Simple class for handling serialized USet/UnicodeSet structures
    /// without object creation. See ICU4C icu/source/common/uset.c.
    /// </summary>
    public sealed class USerializedSet
    {
        /// <summary>
        /// Fill in the given serialized set object.
        /// </summary>
        /// <param name="src">Pointer to start of array.</param>
        /// <param name="srcStart">Pointer to start of serialized data (length value).</param>
        /// <returns>true if the given array is valid, otherwise false.</returns>
        public bool GetSet(char[] src, int srcStart)
        {
            // leave most argument checking up to Java exceptions
            array = null;
            arrayOffset = bmpLength = length = 0;

            length = src[srcStart++];

            if ((length & 0x8000) != 0)
            {
                /* there are supplementary values */
                length &= 0x7fff;
                if (src.Length < (srcStart + 1 + length))
                {
                    length = 0;
                    throw new IndexOutOfRangeException();
                }
                bmpLength = src[srcStart++];
            }
            else
            {
                /* only BMP values */
                if (src.Length < (srcStart + length))
                {
                    length = 0;
                    throw new IndexOutOfRangeException();
                }
                bmpLength = length;
            }
            array = new char[length];
            System.Array.Copy(src, srcStart, array, 0, length);
            //arrayOffset=srcStart;
            return true;
        }

        /// <summary>
        /// Set the <see cref="USerializedSet"/> to contain the given character (and nothing
        /// else).
        /// </summary>
        public void SetToOne(int c)
        {
            if (0x10ffff < c)
            {
                return;
            }

            if (c < 0xffff)
            {
                bmpLength = length = 2;
                array[0] = (char)c;
                array[1] = (char)(c + 1);
            }
            else if (c == 0xffff)
            {
                bmpLength = 1;
                length = 3;
                array[0] = (char)0xffff;
                array[1] = (char)1;
                array[2] = (char)0;
            }
            else if (c < 0x10ffff)
            {
                bmpLength = 0;
                length = 4;
                array[0] = (char)(c >> 16);
                array[1] = (char)c;
                ++c;
                array[2] = (char)(c >> 16);
                array[3] = (char)c;
            }
            else /* c==0x10ffff */
            {
                bmpLength = 0;
                length = 2;
                array[0] = (char)0x10;
                array[1] = (char)0xffff;
            }
        }

        /// <summary>
        /// Returns a range of characters contained in the given serialized
        /// set.
        /// </summary>
        /// <param name="rangeIndex">A non-negative integer in the range <c>0..GetSerializedRangeCount()-1</c>.</param>
        /// <param name="range">Variable to receive the data in the range.</param>
        /// <returns>true if rangeIndex is valid, otherwise false.</returns>
        public bool GetRange(int rangeIndex, int[] range)
        {
            if (rangeIndex < 0)
            {
                return false;
            }
            if (array == null)
            {
                array = new char[8];
            }
            if (range == null || range.Length < 2)
            {
                throw new ArgumentException();
            }
            rangeIndex *= 2; /* address start/limit pairs */
            if (rangeIndex < bmpLength)
            {
                range[0] = array[rangeIndex++];
                if (rangeIndex < bmpLength)
                {
                    range[1] = array[rangeIndex] - 1;
                }
                else if (rangeIndex < length)
                {
                    range[1] = ((((int)array[rangeIndex]) << 16) | array[rangeIndex + 1]) - 1;
                }
                else
                {
                    range[1] = 0x10ffff;
                }
                return true;
            }
            else
            {
                rangeIndex -= bmpLength;
                rangeIndex *= 2; /* address pairs of pairs of units */
                int suppLength = length - bmpLength;
                if (rangeIndex < suppLength)
                {
                    int offset = arrayOffset + bmpLength;
                    range[0] = (((int)array[offset + rangeIndex]) << 16) | array[offset + rangeIndex + 1];
                    rangeIndex += 2;
                    if (rangeIndex < suppLength)
                    {
                        range[1] = ((((int)array[offset + rangeIndex]) << 16) | array[offset + rangeIndex + 1]) - 1;
                    }
                    else
                    {
                        range[1] = 0x10ffff;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the current <see cref="USerializedSet"/> contains the given
        /// character.
        /// </summary>
        /// <param name="c">The character to test for.</param>
        /// <returns>true if set contains c.</returns>
        public bool Contains(int c)
        {
            if (c > 0x10ffff)
            {
                return false;
            }

            if (c <= 0xffff)
            {
                int i;
                /* find c in the BMP part */
                for (i = 0; i < bmpLength && (char)c >= array[i]; ++i) { }
                return ((i & 1) != 0);
            }
            else
            {
                int i;
                /* find c in the supplementary part */
                char high = (char)(c >> 16), low = (char)c;
                for (i = bmpLength;
                    i < length && (high > array[i] || (high == array[i] && low >= array[i + 1]));
                    i += 2) { }

                /* count pairs of 16-bit units even per BMP and check if the number of pairs is odd */
                return (((i + bmpLength) & 2) != 0);
            }
        }

        /// <summary>
        /// Returns the number of disjoint ranges of characters contained in
        /// the given serialized set.  Ignores any strings contained in the set.
        /// </summary>
        /// <returns>A non-negative integer counting the character ranges contained in set.</returns>
        public int CountRanges()
        {
            return (bmpLength + (length - bmpLength) / 2 + 1) / 2;
        }

        private char[] array = new char[8];
        private int arrayOffset, bmpLength, length;
    }
}
