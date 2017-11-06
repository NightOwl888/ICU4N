using System;
using System.Collections.Generic;
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
        private static readonly long serialVersionUID = 7997698588986878753L;

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

        /**
         * Create a new {@code BitSet} with size equal to 64 bits.
         * 
         * @see #clear(int)
         * @see #set(int)
         * @see #clear()
         * @see #clear(int, int)
         * @see #set(int, boolean)
         * @see #set(int, int)
         * @see #set(int, int, boolean)
         */
        public BitSet()
        {
            bits = new long[1];
            actualArrayLength = 0;
            isLengthActual = true;
        }

        /**
         * Create a new {@code BitSet} with size equal to nbits. If nbits is not a
         * multiple of 64, then create a {@code BitSet} with size nbits rounded to
         * the next closest multiple of 64.
         * 
         * @param nbits
         *            the size of the bit set.
         * @throws NegativeArraySizeException
         *             if {@code nbits} is negative.
         * @see #clear(int)
         * @see #set(int)
         * @see #clear()
         * @see #clear(int, int)
         * @see #set(int, boolean)
         * @see #set(int, int)
         * @see #set(int, int, boolean)
         */
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

        /**
         * Private constructor called from get(int, int) method
         * 
         * @param bits
         *            the size of the bit set
         */
        private BitSet(long[] bits, bool needClear, int actualArrayLength,
                bool isLengthActual)
        {
            this.bits = bits;
            this.needClear = needClear;
            this.actualArrayLength = actualArrayLength;
            this.isLengthActual = isLengthActual;
        }

        /**
         * Creates a copy of this {@code BitSet}.
         * 
         * @return a copy of this {@code BitSet}.
         */
        public virtual object Clone()
        {
            BitSet clone = (BitSet)base.MemberwiseClone();
            clone.bits = (long[])bits.Clone();
            return clone;
        }

        /**
         * Compares the argument to this {@code BitSet} and returns whether they are
         * equal. The object must be an instance of {@code BitSet} with the same
         * bits set.
         * 
         * @param obj
         *            the {@code BitSet} object to compare.
         * @return a {@code boolean} indicating whether or not this {@code BitSet} and
         *         {@code obj} are equal.
         * @see #hashCode
         */
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

        /**
         * Increase the size of the internal array to accommodate {@code pos} bits.
         * The new array max index will be a multiple of 64.
         * 
         * @param len
         *            the index the new array needs to be able to access.
         */
        private void GrowLength(int len)
        {
            long[] tempBits = new long[Math.Max(len, bits.Length * 2)];
            System.Array.Copy(bits, 0, tempBits, 0, this.actualArrayLength);
            bits = tempBits;
        }

        /**
         * Computes the hash code for this {@code BitSet}. If two {@code BitSet}s are equal
         * the have to return the same result for {@code hashCode()}.
         * 
         * @return the {@code int} representing the hash code for this bit
         *         set.
         * @see #equals
         * @see java.util.Hashtable
         */
        public override int GetHashCode()
        {
            long x = 1234;
            for (int i = 0, length = actualArrayLength; i < length; i++)
            {
                x ^= bits[i] * (i + 1);
            }
            return (int)((x >> 32) ^ x);
        }

        /**
         * Retrieves the bit at index {@code pos}. Grows the {@code BitSet} if
         * {@code pos > size}.
         * 
         * @param pos
         *            the index of the bit to be retrieved.
         * @return {@code true} if the bit at {@code pos} is set,
         *         {@code false} otherwise.
         * @throws IndexOutOfBoundsException
         *             if {@code pos} is negative.
         * @see #clear(int)
         * @see #set(int)
         * @see #clear()
         * @see #clear(int, int)
         * @see #set(int, boolean)
         * @see #set(int, int)
         * @see #set(int, int, boolean)
         */
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

        /**
         * Retrieves the bits starting from {@code pos1} to {@code pos2} and returns
         * back a new bitset made of these bits. Grows the {@code BitSet} if
         * {@code pos2 > size}.
         * 
         * @param pos1
         *            beginning position.
         * @param pos2
         *            ending position.
         * @return new bitset of the range specified.
         * @throws IndexOutOfBoundsException
         *             if {@code pos1} or {@code pos2} is negative, or if
         *             {@code pos2} is smaller than {@code pos1}.
         * @see #get(int)
         */
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

        /**
         * Sets the bit at index {@code pos} to 1. Grows the {@code BitSet} if
         * {@code pos > size}.
         * 
         * @param pos
         *            the index of the bit to set.
         * @throws IndexOutOfBoundsException
         *             if {@code pos} is negative.
         * @see #clear(int)
         * @see #clear()
         * @see #clear(int, int)
         */
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

        /**
         * Sets the bit at index {@code pos} to {@code val}. Grows the
         * {@code BitSet} if {@code pos > size}.
         * 
         * @param pos
         *            the index of the bit to set.
         * @param val
         *            value to set the bit.
         * @throws IndexOutOfBoundsException
         *             if {@code pos} is negative.
         * @see #set(int)
         */
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

        /**
         * Sets the bits starting from {@code pos1} to {@code pos2}. Grows the
         * {@code BitSet} if {@code pos2 > size}.
         * 
         * @param pos1
         *            beginning position.
         * @param pos2
         *            ending position.
         * @throws IndexOutOfBoundsException
         *             if {@code pos1} or {@code pos2} is negative, or if
         *             {@code pos2} is smaller than {@code pos1}.
         * @see #set(int)
         */
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

        /**
         * Sets the bits starting from {@code pos1} to {@code pos2} to the given
         * {@code val}. Grows the {@code BitSet} if {@code pos2 > size}.
         * 
         * @param pos1
         *            beginning position.
         * @param pos2
         *            ending position.
         * @param val
         *            value to set these bits.
         * @throws IndexOutOfBoundsException
         *             if {@code pos1} or {@code pos2} is negative, or if
         *             {@code pos2} is smaller than {@code pos1}.
         * @see #set(int,int)
         */
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

        /**
         * Clears all the bits in this {@code BitSet}.
         * 
         * @see #clear(int)
         * @see #clear(int, int)
         */
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

        /**
         * Clears the bit at index {@code pos}. Grows the {@code BitSet} if
         * {@code pos > size}.
         * 
         * @param pos
         *            the index of the bit to clear.
         * @throws IndexOutOfBoundsException
         *             if {@code pos} is negative.
         * @see #clear(int, int)
         */
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

        /**
         * Clears the bits starting from {@code pos1} to {@code pos2}. Grows the
         * {@code BitSet} if {@code pos2 > size}.
         * 
         * @param pos1
         *            beginning position.
         * @param pos2
         *            ending position.
         * @throws IndexOutOfBoundsException
         *             if {@code pos1} or {@code pos2} is negative, or if
         *             {@code pos2} is smaller than {@code pos1}.
         * @see #clear(int)
         */
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

        /**
         * Flips the bit at index {@code pos}. Grows the {@code BitSet} if
         * {@code pos > size}.
         * 
         * @param pos
         *            the index of the bit to flip.
         * @throws IndexOutOfBoundsException
         *             if {@code pos} is negative.
         * @see #flip(int, int)
         */
        public void flip(int pos)
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

        /**
         * Flips the bits starting from {@code pos1} to {@code pos2}. Grows the
         * {@code BitSet} if {@code pos2 > size}.
         * 
         * @param pos1
         *            beginning position.
         * @param pos2
         *            ending position.
         * @throws IndexOutOfBoundsException
         *             if {@code pos1} or {@code pos2} is negative, or if
         *             {@code pos2} is smaller than {@code pos1}.
         * @see #flip(int)
         */
        public void flip(int pos1, int pos2)
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

        /**
         * Checks if these two {@code BitSet}s have at least one bit set to true in the same
         * position.
         * 
         * @param bs
         *            {@code BitSet} used to calculate the intersection.
         * @return {@code true} if bs intersects with this {@code BitSet},
         *         {@code false} otherwise.
         */
        public bool Intersects(BitSet bs)
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

        /**
         * Performs the logical AND of this {@code BitSet} with another
         * {@code BitSet}. The values of this {@code BitSet} are changed accordingly.
         * 
         * @param bs
         *            {@code BitSet} to AND with.
         * @see #or
         * @see #xor
         */
        public void And(BitSet bs)
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

        /**
         * Clears all bits in the receiver which are also set in the parameter
         * {@code BitSet}. The values of this {@code BitSet} are changed accordingly.
         * 
         * @param bs
         *            {@code BitSet} to ANDNOT with.
         */
        public void AndNot(BitSet bs)
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

        /**
         * Performs the logical OR of this {@code BitSet} with another {@code BitSet}.
         * The values of this {@code BitSet} are changed accordingly.
         *
         * @param bs
         *            {@code BitSet} to OR with.
         * @see #xor
         * @see #and
         */
        public void Or(BitSet bs)
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

        /**
         * Performs the logical XOR of this {@code BitSet} with another {@code BitSet}.
         * The values of this {@code BitSet} are changed accordingly.
         *
         * @param bs
         *            {@code BitSet} to XOR with.
         * @see #or
         * @see #and
         */
        public void Xor(BitSet bs)
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

        /**
         * Returns the number of bits this {@code BitSet} has.
         * 
         * @return the number of bits contained in this {@code BitSet}.
         * @see #length
         */
        public int Count
        {
            get { return bits.Length << OFFSET; }
        }

        /**
         * Returns the number of bits up to and including the highest bit set.
         * 
         * @return the length of the {@code BitSet}.
         */
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

        /**
         * Returns a string containing a concise, human-readable description of the
         * receiver.
         * 
         * @return a comma delimited list of the indices of all bits that are set.
         */
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

        /**
         * Returns the position of the first bit that is {@code true} on or after {@code pos}.
         * 
         * @param pos
         *            the starting position (inclusive).
         * @return -1 if there is no bits that are set to {@code true} on or after {@code pos}.
         */
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

        /**
         * Returns the position of the first bit that is {@code false} on or after {@code pos}.
         * 
         * @param pos
         *            the starting position (inclusive).
         * @return the position of the next bit set to {@code false}, even if it is further
         *         than this {@code BitSet}'s size.
         */
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

        /**
         * Returns true if all the bits in this {@code BitSet} are set to false.
         * 
         * @return {@code true} if the {@code BitSet} is empty,
         *         {@code false} otherwise.
         */
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

        /**
         * Returns the number of bits that are {@code true} in this {@code BitSet}.
         * 
         * @return the number of {@code true} bits in the set.
         */
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
