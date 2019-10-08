using System;
using System.Text;

namespace ICU4N.Support.Collections
{
    /// <summary>
    /// The <see cref="BitSet"/> class implements a bit field. Each element in a
    /// <see cref="BitSet"/> can be on(1) or off(0). A <see cref="BitSet"/> is created with a
    /// given size and grows if this size is exceeded. Growth is always rounded to a
    /// 64 bit boundary.
    /// </summary>
    public class BitSet // ICU4N TODO: Make serializable
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        private static readonly int OFFSET = 6;

        private static readonly int ELM_SIZE = 1 << OFFSET;

        private static readonly int RIGHT_BITS = ELM_SIZE - 1;

        private static readonly long[] TWO_N_ARRAY = new long[] { 0x1L, 0x2L, 0x4L,
            0x8L, 0x10L, 0x20L, 0x40L, 0x80L, 0x100L, 0x200L, 0x400L, 0x800L,
            0x1000L, 0x2000L, 0x4000L, 0x8000L, 0x10000L, 0x20000L, 0x40000L,
            0x80000L, 0x100000L, 0x200000L, 0x400000L, 0x800000L, 0x1000000L,
            0x2000000L, 0x4000000L, 0x8000000L, 0x10000000L, 0x20000000L,
            0x40000000L, 0x80000000L, 0x100000000L, 0x200000000L, 0x400000000L,
            0x800000000L, 0x1000000000L, 0x2000000000L, 0x4000000000L,
            0x8000000000L, 0x10000000000L, 0x20000000000L, 0x40000000000L,
            0x80000000000L, 0x100000000000L, 0x200000000000L, 0x400000000000L,
            0x800000000000L, 0x1000000000000L, 0x2000000000000L,
            0x4000000000000L, 0x8000000000000L, 0x10000000000000L,
            0x20000000000000L, 0x40000000000000L, 0x80000000000000L,
            0x100000000000000L, 0x200000000000000L, 0x400000000000000L,
            0x800000000000000L, 0x1000000000000000L, 0x2000000000000000L,
            0x4000000000000000L, unchecked((long)0x8000000000000000L) };

        private long[] bits;


        private bool needClear; // non-serializable

        private int actualArrayLength; // non-serializable

        private bool isLengthActual; // non-serializable

        /// <summary>
        /// Create a new <see cref="BitSet"/> with size equal to 64 bits.
        /// </summary>
        /// <seealso cref="Clear(int)"/>
        /// <seealso cref="Set(int)"/>
        /// <seealso cref="Clear()"/>
        /// <seealso cref="Clear(int, int)"/>
        /// <seealso cref="Set(int, bool)"/>
        /// <seealso cref="Set(int, int)"/>
        /// <seealso cref="Set(int, int, bool)"/>
        public BitSet()
        {
            bits = new long[1];
            actualArrayLength = 0;
            isLengthActual = true;
        }

        /// <summary>
        /// Create a new <see cref="BitSet"/> with size equal to nbits. If nbits is not a
        /// multiple of 64, then create a <see cref="BitSet"/> with size nbits rounded to
        /// the next closest multiple of 64.
        /// </summary>
        /// <param name="nbits">The size of the bit set.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="nbits"/> is negative.</exception>
        /// <seealso cref="Clear(int)"/>
        /// <seealso cref="Set(int)"/>
        /// <seealso cref="Clear()"/>
        /// <seealso cref="Clear(int, int)"/>
        /// <seealso cref="Set(int, bool)"/>
        /// <seealso cref="Set(int, int)"/>
        /// <seealso cref="Set(int, int, bool)"/>
        public BitSet(int nbits)
        {
            if (nbits < 0)
            {
                throw new IndexOutOfRangeException("nbits may not be negative");
            }
            bits = new long[(nbits >> OFFSET) + ((nbits & RIGHT_BITS) > 0 ? 1 : 0)];
            actualArrayLength = 0;
            isLengthActual = true;
        }

        /// <summary>
        /// Private constructor called from <see cref="Get(int, int)"/> method.
        /// </summary>
        /// <param name="bits">The size of the bit set.</param>
        /// <param name="needClear"></param>
        /// <param name="actualArrayLength"></param>
        /// <param name="isLengthActual"></param>
        private BitSet(long[] bits, bool needClear, int actualArrayLength,
                bool isLengthActual)
        {
            this.bits = bits;
            this.needClear = needClear;
            this.actualArrayLength = actualArrayLength;
            this.isLengthActual = isLengthActual;
        }

        /// <summary>
        /// Creates a copy of this <see cref="BitSet"/>.
        /// </summary>
        /// <returns>A copy of this <see cref="BitSet"/>.</returns>
        public virtual object Clone()
        {
            BitSet clone = (BitSet)base.MemberwiseClone();
            clone.bits = (long[])bits.Clone();
            return clone;
        }

        /// <summary>
        /// Compares the argument to this <see cref="BitSet"/> and returns whether they are
        /// equal. The object must be an instance of <see cref="BitSet"/> with the same
        /// bits set.
        /// </summary>
        /// <param name="obj">The <see cref="BitSet"/> object to compare.</param>
        /// <returns>A <see cref="bool"/> indicating whether or not this <see cref="BitSet"/> and
        /// <paramref name="obj"/> are equal.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is BitSet)
            {
                long[] bsBits = ((BitSet)obj).bits;
                int length1 = this.actualArrayLength, length2 = ((BitSet)obj).actualArrayLength;
                if (this.isLengthActual && ((BitSet)obj).isLengthActual
                        && length1 != length2)
                {
                    return false;
                }
                // If one of the BitSets is larger than the other, check to see if
                // any of its extra bits are set. If so return false.
                if (length1 <= length2)
                {
                    for (int i = 0; i < length1; i++)
                    {
                        if (bits[i] != bsBits[i])
                        {
                            return false;
                        }
                    }
                    for (int i = length1; i < length2; i++)
                    {
                        if (bsBits[i] != 0)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < length2; i++)
                    {
                        if (bits[i] != bsBits[i])
                        {
                            return false;
                        }
                    }
                    for (int i = length2; i < length1; i++)
                    {
                        if (bits[i] != 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Increase the size of the internal array to accommodate <paramref name="len"/> bits.
        /// The new array max index will be a multiple of 64.
        /// </summary>
        /// <param name="len">The index the new array needs to be able to access.</param>
        private void GrowLength(int len)
        {
            long[] tempBits = new long[Math.Max(len, bits.Length * 2)];
            System.Array.Copy(bits, 0, tempBits, 0, this.actualArrayLength);
            bits = tempBits;
        }

        /// <summary>
        /// Computes the hash code for this <see cref="BitSet"/>. If two <see cref="BitSet"/>s are equal
        /// the have to return the same result for <see cref="GetHashCode()"/>.
        /// </summary>
        /// <returns>The <see cref="int"/> representing the hash code for this bit
        /// set.</returns>
        /// <seealso cref="Equals(object)"/>
        public override int GetHashCode()
        {
            long x = 1234;
            for (int i = 0, length = actualArrayLength; i < length; i++)
            {
                x ^= bits[i] * (i + 1);
            }
            return (int)((x >> 32) ^ x);
        }

        /// <summary>
        /// Retrieves the bit at index <paramref name="pos"/>. Grows the <see cref="BitSet"/> if
        /// <paramref name="pos"/> &gt; size.
        /// </summary>
        /// <param name="pos">The index of the bit to be retrieved.</param>
        /// <returns><c>true</c> if the bit at <paramref name="pos"/> is set,
        /// <c>false</c> otherwise.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos"/> is negative.</exception>
        /// <seealso cref="Clear(int)"/>
        /// <seealso cref="Set(int)"/>
        /// <seealso cref="Clear()"/>
        /// <seealso cref="Clear(int, int)"/>
        /// <seealso cref="Set(int, bool)"/>
        /// <seealso cref="Set(int, int)"/>
        /// <seealso cref="Set(int, int, bool)"/>
        public bool Get(int pos)
        {
            if (pos < 0)
            {
                // Negative index specified
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            int arrayPos = pos >> OFFSET;
            if (arrayPos < actualArrayLength)
            {
                return (bits[arrayPos] & TWO_N_ARRAY[pos & RIGHT_BITS]) != 0;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the bits starting from <paramref name="pos1"/> to <paramref name="pos2"/> and returns
        /// back a new bitset made of these bits. Grows the <see cref="BitSet"/> if
        /// <paramref name="pos2"/> &gt; size.
        /// </summary>
        /// <param name="pos1">Beginning position.</param>
        /// <param name="pos2">Ending position.</param>
        /// <returns>New bitset of the range specified.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos1"/> or <paramref name="pos2"/> is negative, or if
        /// <paramref name="pos2"/> is smaller than <paramref name="pos1"/>.</exception>
        /// <seealso cref="Get(int)"/>
        public BitSet Get(int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos2 < pos1)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            int last = actualArrayLength << OFFSET;
            if (pos1 >= last || pos1 == pos2)
            {
                return new BitSet(0);
            }
            if (pos2 > last)
            {
                pos2 = last;
            }

            int idx1 = pos1 >> OFFSET;
            int idx2 = (pos2 - 1) >> OFFSET;
            long factor1 = (~0L) << (pos1 & RIGHT_BITS);
            long factor2 = (int)((uint)((~0L) >> (ELM_SIZE - (pos2 & RIGHT_BITS))));

            if (idx1 == idx2)
            {
                long result = (int)((uint)(bits[idx1] & (factor1 & factor2)) >> (pos1 % ELM_SIZE));
                if (result == 0)
                {
                    return new BitSet(0);
                }
                return new BitSet(new long[] { result }, needClear, 1, true);
            }
            long[] newbits = new long[idx2 - idx1 + 1];
            // first fill in the first and last indexes in the new bitset
            newbits[0] = bits[idx1] & factor1;
            newbits[newbits.Length - 1] = bits[idx2] & factor2;

            // fill in the in between elements of the new bitset
            for (int i = 1; i < idx2 - idx1; i++)
            {
                newbits[i] = bits[idx1 + i];
            }

            // shift all the elements in the new bitset to the right by pos1
            // % ELM_SIZE
            int numBitsToShift = pos1 & RIGHT_BITS;
            int actualLen = newbits.Length;
            if (numBitsToShift != 0)
            {
                for (int i = 0; i < newbits.Length; i++)
                {
                    // shift the current element to the right regardless of
                    // sign
                    newbits[i] = (int)((uint)newbits[i] >> (numBitsToShift));

                    // apply the last x bits of newbits[i+1] to the current
                    // element
                    if (i != newbits.Length - 1)
                    {
                        newbits[i] |= newbits[i + 1] << (ELM_SIZE - (numBitsToShift));
                    }
                    if (newbits[i] != 0)
                    {
                        actualLen = i + 1;
                    }
                }
            }
            return new BitSet(newbits, needClear, actualLen,
                    newbits[actualLen - 1] != 0);
        }

        /// <summary>
        /// Sets the bit at index <paramref name="pos"/> to 1. Grows the <see cref="BitSet"/> if
        /// <paramref name="pos"/> &gt; size.
        /// </summary>
        /// <param name="pos">The index of the bit to set.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos"/> is negative.</exception>
        /// <seealso cref="Clear(int)"/>
        /// <seealso cref="Clear()"/>
        /// <seealso cref="Clear(int, int)"/>
        public void Set(int pos)
        {
            if (pos < 0)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            int len = (pos >> OFFSET) + 1;
            if (len > bits.Length)
            {
                GrowLength(len);
            }
            bits[len - 1] |= TWO_N_ARRAY[pos & RIGHT_BITS];
            if (len > actualArrayLength)
            {
                actualArrayLength = len;
                isLengthActual = true;
            }
            NeedClear();
        }

        /// <summary>
        /// Sets the bit at index <paramref name="pos"/> to <paramref name="val"/>. Grows the
        /// <see cref="BitSet"/> if <paramref name="pos"/> &gt; size.
        /// </summary>
        /// <param name="pos">The index of the bit to set.</param>
        /// <param name="val">Value to set the bit.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos"/> is negative.</exception>
        /// <seealso cref="Set(int)"/>
        public void Set(int pos, bool val)
        {
            if (val)
            {
                Set(pos);
            }
            else
            {
                Clear(pos);
            }
        }

        /// <summary>
        /// Sets the bits starting from <paramref name="pos1"/> to <paramref name="pos2"/>. Grows the
        /// <see cref="BitSet"/> if <paramref name="pos2"/> &gt; size.
        /// </summary>
        /// <param name="pos1">Beginning position.</param>
        /// <param name="pos2">Ending position.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos1"/> or <paramref name="pos2"/> is negative, or if
        /// <paramref name="pos2"/> is smaller than <paramref name="pos1"/>.</exception>
        /// <seealso cref="Set(int)"/>
        public void Set(int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos2 < pos1)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            if (pos1 == pos2)
            {
                return;
            }
            int len2 = ((pos2 - 1) >> OFFSET) + 1;
            if (len2 > bits.Length)
            {
                GrowLength(len2);
            }

            int idx1 = pos1 >> OFFSET;
            int idx2 = (pos2 - 1) >> OFFSET;
            long factor1 = (~0L) << (pos1 & RIGHT_BITS);
            long factor2 = (long)((ulong)((~0L) >> (ELM_SIZE - (pos2 & RIGHT_BITS))));

            if (idx1 == idx2)
            {
                bits[idx1] |= (factor1 & factor2);
            }
            else
            {
                bits[idx1] |= factor1;
                bits[idx2] |= factor2;
                for (int i = idx1 + 1; i < idx2; i++)
                {
                    bits[i] |= (~0L);
                }
            }
            if (idx2 + 1 > actualArrayLength)
            {
                actualArrayLength = idx2 + 1;
                isLengthActual = true;
            }
            NeedClear();
        }

        private void NeedClear()
        {
            this.needClear = true;
        }

        /// <summary>
        /// Sets the bits starting from <paramref name="pos1"/> to <paramref name="pos2"/> to the given
        /// <paramref name="val"/>. Grows the <see cref="BitSet"/> if <paramref name="pos2"/> &gt; size.
        /// </summary>
        /// <param name="pos1">Beginning position.</param>
        /// <param name="pos2">Ending position.</param>
        /// <param name="val">Value to set these bits.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos1"/> or <paramref name="pos2"/> is negative, or if
        /// <paramref name="pos2"/> is smaller than <paramref name="pos1"/>.</exception>
        /// <seealso cref="Set(int, int)"/>
        public void Set(int pos1, int pos2, bool val)
        {
            if (val)
            {
                Set(pos1, pos2);
            }
            else
            {
                Clear(pos1, pos2);
            }
        }

        /// <summary>
        /// Clears all the bits in this <see cref="BitSet"/>.
        /// </summary>
        /// <seealso cref="Clear(int)"/>
        /// <seealso cref="Clear(int, int)"/>
        public void Clear()
        {
            if (needClear)
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = 0L;
                }
                actualArrayLength = 0;
                isLengthActual = true;
                needClear = false;
            }
        }

        /// <summary>
        /// Clears the bit at index <paramref name="pos"/>. Grows the <see cref="BitSet"/> if
        /// <paramref name="pos"/> &gt; size.
        /// </summary>
        /// <param name="pos">The index of the bit to clear.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos"/> is negative.</exception>
        /// <seealso cref="Clear(int, int)"/>
        public void Clear(int pos)
        {
            if (pos < 0)
            {
                // Negative index specified
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            if (!needClear)
            {
                return;
            }
            int arrayPos = pos >> OFFSET;
            if (arrayPos < actualArrayLength)
            {
                bits[arrayPos] &= ~(TWO_N_ARRAY[pos & RIGHT_BITS]);
                if (bits[actualArrayLength - 1] == 0)
                {
                    isLengthActual = false;
                }
            }
        }

        /// <summary>
        /// Clears the bits starting from <paramref name="pos1"/> to <paramref name="pos2"/>. Grows the
        /// <see cref="BitSet"/> if <paramref name="pos2"/> &gt; size;
        /// </summary>
        /// <param name="pos1">Beginning position.</param>
        /// <param name="pos2">Ending position.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos1"/> or <paramref name="pos2"/> is negative, or if
        /// <paramref name="pos2"/> is smaller than <paramref name="pos1"/>.</exception>
        /// <seealso cref="Clear(int)"/>
        public void Clear(int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos2 < pos1)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            if (!needClear)
            {
                return;
            }
            int last = (actualArrayLength << OFFSET);
            if (pos1 >= last || pos1 == pos2)
            {
                return;
            }
            if (pos2 > last)
            {
                pos2 = last;
            }

            int idx1 = pos1 >> OFFSET;
            int idx2 = (pos2 - 1) >> OFFSET;
            long factor1 = (~0L) << (pos1 & RIGHT_BITS);
            long factor2 = (long)((ulong)((~0L) >> (ELM_SIZE - (pos2 & RIGHT_BITS))));

            if (idx1 == idx2)
            {
                bits[idx1] &= ~(factor1 & factor2);
            }
            else
            {
                bits[idx1] &= ~factor1;
                bits[idx2] &= ~factor2;
                for (int i = idx1 + 1; i < idx2; i++)
                {
                    bits[i] = 0L;
                }
            }
            if ((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0))
            {
                isLengthActual = false;
            }
        }

        /// <summary>
        /// Flips the bit at index <paramref name="pos"/>. Grows the <see cref="BitSet"/> if
        /// <paramref name="pos"/> &gt; size.
        /// </summary>
        /// <param name="pos">The index of the bit to flip.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos"/> is negative.</exception>
        /// <seealso cref="Flip(int, int)"/>
        public void Flip(int pos)
        {
            if (pos < 0)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            int len = (pos >> OFFSET) + 1;
            if (len > bits.Length)
            {
                GrowLength(len);
            }
            bits[len - 1] ^= TWO_N_ARRAY[pos & RIGHT_BITS];
            if (len > actualArrayLength)
            {
                actualArrayLength = len;
            }
            isLengthActual = !((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0));
            NeedClear();
        }

        /// <summary>
        /// Flips the bits starting from <paramref name="pos1"/> to <paramref name="pos2"/>. Grows the
        ///  <see cref="BitSet"/> if <paramref name="pos2"/> &gt; size.
        /// </summary>
        /// <param name="pos1">Beginning position.</param>
        /// <param name="pos2">Ending position.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="pos1"/> or <paramref name="pos2"/> is negative, or if
        /// <paramref name="pos2"/> is smaller than <paramref name="pos1"/>.</exception>
        /// <seealso cref="Flip(int)"/>
        public void Flip(int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos2 < pos1)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            if (pos1 == pos2)
            {
                return;
            }
            int len2 = ((pos2 - 1) >> OFFSET) + 1;
            if (len2 > bits.Length)
            {
                GrowLength(len2);
            }

            int idx1 = pos1 >> OFFSET;
            int idx2 = (pos2 - 1) >> OFFSET;
            long factor1 = (~0L) << (pos1 & RIGHT_BITS);
            long factor2 = (long)((ulong)((~0L) >> (ELM_SIZE - (pos2 & RIGHT_BITS))));

            if (idx1 == idx2)
            {
                bits[idx1] ^= (factor1 & factor2);
            }
            else
            {
                bits[idx1] ^= factor1;
                bits[idx2] ^= factor2;
                for (int i = idx1 + 1; i < idx2; i++)
                {
                    bits[i] ^= (~0L);
                }
            }
            if (len2 > actualArrayLength)
            {
                actualArrayLength = len2;
            }
            isLengthActual = !((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0));
            NeedClear();
        }

        /// <summary>
        /// Checks if these two <see cref="BitSet"/>s have at least one bit set to true in the same
        /// position.
        /// </summary>
        /// <param name="bs"><see cref="BitSet"/> used to calculate the intersection.</param>
        /// <returns><c>true</c> if bs intersects with this <see cref="BitSet"/>,
        /// <c>false</c> otherwise.</returns>
        public bool Intersects(BitSet bs) // ICU4N TODO: API - Make a member of ISet<T>?
        {
            long[] bsBits = bs.bits;
            int length1 = actualArrayLength, length2 = bs.actualArrayLength;

            if (length1 <= length2)
            {
                for (int i = 0; i < length1; i++)
                {
                    if ((bits[i] & bsBits[i]) != 0L)
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < length2; i++)
                {
                    if ((bits[i] & bsBits[i]) != 0L)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Performs the logical AND of this <see cref="BitSet"/> with another
        /// <see cref="BitSet"/>. The values of this <see cref="BitSet"/> are changed accordingly.
        /// </summary>
        /// <param name="bs"><see cref="BitSet"/> to AND with.</param>
        /// <seealso cref="Or(BitSet)"/>
        /// <seealso cref="Xor(BitSet)"/>
        public void And(BitSet bs) // ICU4N TODO: API - Make a member of ISet<T>?
        {
            long[] bsBits = bs.bits;
            if (!needClear)
            {
                return;
            }
            int length1 = actualArrayLength, length2 = bs.actualArrayLength;
            if (length1 <= length2)
            {
                for (int i = 0; i < length1; i++)
                {
                    bits[i] &= bsBits[i];
                }
            }
            else
            {
                for (int i = 0; i < length2; i++)
                {
                    bits[i] &= bsBits[i];
                }
                for (int i = length2; i < length1; i++)
                {
                    bits[i] = 0;
                }
                actualArrayLength = length2;
            }
            isLengthActual = !((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0));
        }

        /// <summary>
        /// Clears all bits in the receiver which are also set in the <paramref name="bs"/> parameter.
        /// The values of this <see cref="BitSet"/> are changed accordingly.
        /// </summary>
        /// <param name="bs"><see cref="BitSet"/> to ANDNOT with.</param>
        public void AndNot(BitSet bs) // ICU4N TODO: API - Make a member of ISet<T>?
        {
            long[] bsBits = bs.bits;
            if (!needClear)
            {
                return;
            }
            int range = actualArrayLength < bs.actualArrayLength ? actualArrayLength
                    : bs.actualArrayLength;
            for (int i = 0; i < range; i++)
            {
                bits[i] &= ~bsBits[i];
            }

            if (actualArrayLength < range)
            {
                actualArrayLength = range;
            }
            isLengthActual = !((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0));
        }

        /// <summary>
        /// Performs the logical OR of this <see cref="BitSet"/> with another <see cref="BitSet"/>.
        /// The values of this <see cref="BitSet"/> are changed accordingly.
        /// </summary>
        /// <param name="bs"><see cref="BitSet"/> to OR with.</param>
        /// <seealso cref="Xor(BitSet)"/>
        /// <seealso cref="And(BitSet)"/>
        public void Or(BitSet bs) // ICU4N TODO: API - Make a member of ISet<T>?
        {
            int bsActualLen = bs.GetActualArrayLength();
            if (bsActualLen > bits.Length)
            {
                long[] tempBits = new long[bsActualLen];
                System.Array.Copy(bs.bits, 0, tempBits, 0, bs.actualArrayLength);
                for (int i = 0; i < actualArrayLength; i++)
                {
                    tempBits[i] |= bits[i];
                }
                bits = tempBits;
                actualArrayLength = bsActualLen;
                isLengthActual = true;
            }
            else
            {
                long[] bsBits = bs.bits;
                for (int i = 0; i < bsActualLen; i++)
                {
                    bits[i] |= bsBits[i];
                }
                if (bsActualLen > actualArrayLength)
                {
                    actualArrayLength = bsActualLen;
                    isLengthActual = true;
                }
            }
            NeedClear();
        }

        /// <summary>
        /// Performs the logical XOR of this <see cref="BitSet"/> with another <see cref="BitSet"/>.
        /// The values of this <see cref="BitSet"/> are changed accordingly.
        /// </summary>
        /// <param name="bs"><see cref="BitSet"/> to XOR with.</param>
        /// <seealso cref="Or(BitSet)"/>
        /// <seealso cref="And(BitSet)"/>
        public void Xor(BitSet bs) // ICU4N TODO: API - Make a member of ISet<T>?
        {
            int bsActualLen = bs.GetActualArrayLength();
            if (bsActualLen > bits.Length)
            {
                long[] tempBits = new long[bsActualLen];
                System.Array.Copy(bs.bits, 0, tempBits, 0, bs.actualArrayLength);
                for (int i = 0; i < actualArrayLength; i++)
                {
                    tempBits[i] ^= bits[i];
                }
                bits = tempBits;
                actualArrayLength = bsActualLen;
                isLengthActual = !((actualArrayLength > 0) && (bits[actualArrayLength - 1] == 0));
            }
            else
            {
                long[] bsBits = bs.bits;
                for (int i = 0; i < bsActualLen; i++)
                {
                    bits[i] ^= bsBits[i];
                }
                if (bsActualLen > actualArrayLength)
                {
                    actualArrayLength = bsActualLen;
                    isLengthActual = true;
                }
            }
            NeedClear();
        }

        /// <summary>
        /// Gets the number of bits this <see cref="BitSet"/> has.
        /// </summary>
        /// <seealso cref="GetLength()"/>
        public int Count
        {
            get { return bits.Length << OFFSET; }
        }

        /// <summary>
        /// Returns the number of bits up to and including the highest bit set.
        /// </summary>
        /// <returns>The length of the <see cref="BitSet"/>.</returns>
        public int GetLength()
        {
            int idx = actualArrayLength - 1;
            while (idx >= 0 && bits[idx] == 0)
            {
                --idx;
            }
            actualArrayLength = idx + 1;
            if (idx == -1)
            {
                return 0;
            }
            int i = ELM_SIZE - 1;
            long val = bits[idx];
            while ((val & (TWO_N_ARRAY[i])) == 0 && i > 0)
            {
                i--;
            }
            return (idx << OFFSET) + i + 1;
        }

        private int GetActualArrayLength()
        {
            if (isLengthActual)
            {
                return actualArrayLength;
            }
            int idx = actualArrayLength - 1;
            while (idx >= 0 && bits[idx] == 0)
            {
                --idx;
            }
            actualArrayLength = idx + 1;
            isLengthActual = true;
            return actualArrayLength;
        }

        /// <summary>
        /// Returns a string containing a concise, human-readable description of the
        /// receiver.
        /// </summary>
        /// <returns>A comma delimited list of the indices of all bits that are set.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(bits.Length / 2);
            int bitCount = 0;
            sb.Append('{');
            bool comma = false;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == 0)
                {
                    bitCount += ELM_SIZE;
                    continue;
                }
                for (int j = 0; j < ELM_SIZE; j++)
                {
                    if (((bits[i] & (TWO_N_ARRAY[j])) != 0))
                    {
                        if (comma)
                        {
                            sb.Append(", "); //$NON-NLS-1$
                        }
                        sb.Append(bitCount);
                        comma = true;
                    }
                    bitCount++;
                }
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Returns the position of the first bit that is <c>true</c> on or after <paramref name="pos"/>.
        /// </summary>
        /// <param name="pos">The starting position (inclusive).</param>
        /// <returns>-1 if there is no bits that are set to <c>true</c> on or after <paramref name="pos"/>.</returns>
        public virtual int NextSetBit(int pos)
        {
            if (pos < 0)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            if (pos >= actualArrayLength << OFFSET)
            {
                return -1;
            }

            int idx = pos >> OFFSET;
            // first check in the same bit set element
            if (bits[idx] != 0L)
            {
                for (int j = pos & RIGHT_BITS; j < ELM_SIZE; j++)
                {
                    if (((bits[idx] & (TWO_N_ARRAY[j])) != 0))
                    {
                        return (idx << OFFSET) + j;
                    }
                }

            }
            idx++;
            while (idx < actualArrayLength && bits[idx] == 0L)
            {
                idx++;
            }
            if (idx == actualArrayLength)
            {
                return -1;
            }

            // we know for sure there is a bit set to true in this element
            // since the bitset value is not 0L
            for (int j = 0; j < ELM_SIZE; j++)
            {
                if (((bits[idx] & (TWO_N_ARRAY[j])) != 0))
                {
                    return (idx << OFFSET) + j;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the position of the first bit that is <c>false</c> on or after <paramref name="pos"/>.
        /// </summary>
        /// <param name="pos">The starting position (inclusive).</param>
        /// <returns>the position of the next bit set to <c>false</c>, even if it is further
        /// than this <see cref="BitSet"/>'s size.</returns>
        public virtual int NextClearBit(int pos)
        {
            if (pos < 0)
            {
                throw new IndexOutOfRangeException("Position may not be negative"); //$NON-NLS-1$
            }

            int length = actualArrayLength;
            int bssize = length << OFFSET;
            if (pos >= bssize)
            {
                return pos;
            }

            int idx = pos >> OFFSET;
            // first check in the same bit set element
            if (bits[idx] != (~0L))
            {
                for (int j = pos % ELM_SIZE; j < ELM_SIZE; j++)
                {
                    if (((bits[idx] & (TWO_N_ARRAY[j])) == 0))
                    {
                        return idx * ELM_SIZE + j;
                    }
                }
            }
            idx++;
            while (idx < length && bits[idx] == (~0L))
            {
                idx++;
            }
            if (idx == length)
            {
                return bssize;
            }

            // we know for sure there is a bit set to true in this element
            // since the bitset value is not 0L
            for (int j = 0; j < ELM_SIZE; j++)
            {
                if (((bits[idx] & (TWO_N_ARRAY[j])) == 0))
                {
                    return (idx << OFFSET) + j;
                }
            }

            return bssize;
        }

        /// <summary>
        /// Returns true if all the bits in this <see cref="BitSet"/> are set to false.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="BitSet"/> is empty, <c>false</c> otherwise.</returns>
        public virtual bool IsEmpty()
        {
            if (!needClear)
            {
                return true;
            }
            int length = bits.Length;
            for (int idx = 0; idx < length; idx++)
            {
                if (bits[idx] != 0L)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the number of bits that are <c>true</c> in this <see cref="BitSet"/>.
        /// </summary>
        /// <returns>The number of bits that are <c>true</c> in the set.</returns>
        public virtual int Cardinality()
        {
            if (!needClear)
            {
                return 0;
            }
            int count = 0;
            int length = bits.Length;
            // FIXME: need to test performance, if still not satisfied, change it to
            // 256-bits table based
            for (int idx = 0; idx < length; idx++)
            {
                count += Pop(bits[idx] & 0xffffffffL);
                count += (int)((uint)Pop(bits[idx] >> 32));
            }
            return count;
        }

        private int Pop(long x)
        {
            x = x - (((long)((ulong)x >> 1)) & 0x55555555);
            x = (x & 0x33333333) + (long)((ulong)((x >> 2)) & 0x33333333);
            x = (x + (long)((ulong)(x >> 4))) & 0x0f0f0f0f;
            x = x + (long)((ulong)(x >> 8));
            x = x + (long)((ulong)(x >> 16));
            return (int)x & 0x0000003f;
        }

        //private void ReadObject(ObjectInputStream ois)
        //{
        //    ois.defaultReadObject();
        //    this.isLengthActual = false;
        //    this.actualArrayLength = bits.length;
        //    this.needClear = this.getActualArrayLength() != 0;
        //}
    }
}
