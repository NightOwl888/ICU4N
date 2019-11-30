using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Collections
{
    /// <summary>
    /// This class provides supporting methods of java.util.BitSet
    /// that are not present in System.Collections.BitArray.
    /// </summary>
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Returns the value of the bit with the specified index. The value
        /// is <c>true</c> if the bit with the <paramref name="index"/>
        /// is currently set in this <see cref="BitArray"/>; otherwise, the result
        /// is <c>false</c>.
        /// <para/>
        /// Usage Note: This method is safe to use on indicies that are beyond the
        /// <see cref="BitArray.Length"/> of this <see cref="BitArray"/>.
        /// </summary>
        /// <param name="bits">This <see cref="BitArray"/>.</param>
        /// <param name="index">The bit index.</param>
        /// <returns>The value of the bit with the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="bits"/> is <c>null</c>.</exception>
        public static bool SafeGet(this BitArray bits, int index)
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} may not be negative");

            return index < bits.Length && bits.Get(index);
        }

        /// <summary>
        /// Sets the bit at the specified index to the specified value.
        /// Expands the <see cref="BitArray.Length"/> of this <see cref="BitArray"/>
        /// if there aren't enough available bits.
        /// <para/>
        /// Usage Note: This method is safe to use on indicies that are beyond the
        /// <see cref="BitArray.Length"/> of this <see cref="BitArray"/>.
        /// </summary>
        /// <param name="bits">This <see cref="BitArray"/>.</param>
        /// <param name="index">A bit index.</param>
        /// <param name="value">A boolean value to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is less than zero.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="bits"/> is <c>null</c>.</exception>
        public static void SafeSet(this BitArray bits, int index, bool value)
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} may not be negative");

            if (index >= bits.Length)
                bits.Length = index + 1;

            bits.Set(index, value);
        }


        /// <summary>
        /// Returns the number of bits set to <c>true</c> in this <see cref="BitArray"/>.
        /// </summary>
        /// <param name="bits">This <see cref="BitArray"/>.</param>
        /// <returns>The number of bits set to true in this <see cref="BitArray"/>.</returns>
        public static int Cardinality(this BitArray bits)
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            int count = 0;

#if NETSTANDARD1_3
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    count++;
            }
#else
            int bitsCount = bits.Count;
            int[] ints = new int[(bitsCount >> 5) + 1];
            int intsCount = ints.Length;

            bits.CopyTo(ints, 0);

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[intsCount - 1] &= ~(-1 << (bitsCount % 32));

            for (int i = 0; i < intsCount; i++)
            {
                int c = ints[i];

                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c -= (c >> 1) & 0x55555555;
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }

                count += c;
            }
#endif
            return count;
        }
    }
}
