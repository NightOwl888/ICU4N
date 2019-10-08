using ICU4N.Support;
using System.Diagnostics;

namespace ICU4N.Impl
{
    /// <summary>
    /// Helper class for frozen <see cref="Text.UnicodeSet"/>s, implements <see cref="Contains(int)"/> and 
    /// <see cref="Span(string, int, Text.SpanCondition, out int)"/> optimized for BMP code points.
    /// <para/>
    /// Latin-1: Look up bytes.
    /// 2-byte characters: Bits organized vertically.
    /// 3-byte characters: Use zero/one/mixed data per 64-block in U+0000..U+FFFF, with mixed for illegal ranges.
    /// Supplementary characters: Binary search over
    /// the supplementary part of the parent set's inversion list.
    /// </summary>
    public sealed partial class BMPSet
    {
        public const int U16SurrogateOffset = ((0xd800 << 10) + 0xdc00 - 0x10000);

        /// <summary>
        /// One bool ('true' or 'false') per Latin-1 character.
        /// </summary>
        private bool[] latin1Contains;

        /// <summary>
        /// One bit per code point from U+0000..U+07FF. The bits are organized vertically; consecutive code points
        /// correspond to the same bit positions in consecutive table words. With code point parts lead=c{10..6}
        /// trail=c{5..0} it is set.Contains(c)==(table7FF[trail] bit lead)
        /// <para/>
        /// Bits for 0..FF are unused (0).
        /// </summary>
        private int[] table7FF;

        /// <summary>
        /// One bit per 64 BMP code points. The bits are organized vertically; consecutive 64-code point blocks
        /// correspond to the same bit position in consecutive table words. With code point parts lead=c{15..12}
        /// t1=c{11..6} test bits (lead+16) and lead in bmpBlockBits[t1]. If the upper bit is 0, then the lower bit
        /// indicates if Contains(c) for all code points in the 64-block. If the upper bit is 1, then the block is mixed
        /// and set.Contains(c) must be called.
        /// <para/>
        /// Bits for 0..7FF are unused (0).
        /// </summary>
        private int[] bmpBlockBits;

        /// <summary>
        /// Inversion list indexes for restricted binary searches in findCodePoint(), from FindCodePoint(U+0800, U+1000,
        /// U+2000, .., U+F000, U+10000). U+0800 is the first 3-byte-UTF-8 code point. Code points below U+0800 are
        /// always looked up in the bit tables. The last pair of indexes is for finding supplementary code points.
        /// </summary>
        private int[] list4kStarts;

        /// <summary>
        /// The inversion list of the parent set, for the slower Contains() implementation for mixed BMP blocks and for
        /// supplementary code points. The list is terminated with list[listLength-1]=0x110000.
        /// </summary>
        private readonly int[] list;
        private readonly int listLength; // length used; list may be longer to minimize reallocs

        public BMPSet(int[] parentList, int parentListLength)
        {
            list = parentList;
            listLength = parentListLength;
            latin1Contains = new bool[0x100];
            table7FF = new int[64];
            bmpBlockBits = new int[64];
            list4kStarts = new int[18];

            /*
             * Set the list indexes for binary searches for U+0800, U+1000, U+2000, .., U+F000, U+10000. U+0800 is the
             * first 3-byte-UTF-8 code point. Lower code points are looked up in the bit tables. The last pair of
             * indexes is for finding supplementary code points.
             */
            list4kStarts[0] = FindCodePoint(0x800, 0, listLength - 1);
            int i;
            for (i = 1; i <= 0x10; ++i)
            {
                list4kStarts[i] = FindCodePoint(i << 12, list4kStarts[i - 1], listLength - 1);
            }
            list4kStarts[0x11] = listLength - 1;

            InitBits();
        }

        public BMPSet(BMPSet otherBMPSet, int[] newParentList, int newParentListLength)
        {
            list = newParentList;
            listLength = newParentListLength;
            latin1Contains = (bool[])otherBMPSet.latin1Contains.Clone();
            table7FF = (int[])otherBMPSet.table7FF.Clone();
            bmpBlockBits = (int[])otherBMPSet.bmpBlockBits.Clone();
            list4kStarts = (int[])otherBMPSet.list4kStarts.Clone();
        }

        public bool Contains(int c)
        {
            if (c <= 0xff)
            {
                return (latin1Contains[c]);
            }
            else if (c <= 0x7ff)
            {
                return ((table7FF[c & 0x3f] & (1 << (c >> 6))) != 0);
            }
            else if (c < 0xd800 || (c >= 0xe000 && c <= 0xffff))
            {
                int lead = c >> 12;
                int twoBits = (bmpBlockBits[(c >> 6) & 0x3f] >> lead) & 0x10001;
                if (twoBits <= 1)
                {
                    // All 64 code points with the same bits 15..6
                    // are either in the set or not.
                    return (0 != twoBits);
                }
                else
                {
                    // Look up the code point in its 4k block of code points.
                    return ContainsSlow(c, list4kStarts[lead], list4kStarts[lead + 1]);
                }
            }
            else if (c <= 0x10ffff)
            {
                // surrogate or supplementary code point
                return ContainsSlow(c, list4kStarts[0xd], list4kStarts[0x11]);
            }
            else
            {
                // Out-of-range code points get false, consistent with long-standing
                // behavior of UnicodeSet.contains(c).
                return false;
            }
        }

        /// <summary>
        /// Set bits in a bit rectangle in "vertical" bit organization. start&lt;limit&lt;=0x800
        /// </summary>
        private static void Set32x64Bits(int[] table, int start, int limit)
        {
            Debug.Assert(64 == table.Length);
            int lead = start >> 6;  // Named for UTF-8 2-byte lead byte with upper 5 bits.
            int trail = start & 0x3f;  // Named for UTF-8 2-byte trail byte with lower 6 bits.

            // Set one bit indicating an all-one block.
            int bits = 1 << lead;
            if ((start + 1) == limit)
            { // Single-character shortcut.
                table[trail] |= bits;
                return;
            }

            int limitLead = limit >> 6;
            int limitTrail = limit & 0x3f;

            if (lead == limitLead)
            {
                // Partial vertical bit column.
                while (trail < limitTrail)
                {
                    table[trail++] |= bits;
                }
            }
            else
            {
                // Partial vertical bit column,
                // followed by a bit rectangle,
                // followed by another partial vertical bit column.
                if (trail > 0)
                {
                    do
                    {
                        table[trail++] |= bits;
                    } while (trail < 64);
                    ++lead;
                }
                if (lead < limitLead)
                {
                    bits = ~((1 << lead) - 1);
                    if (limitLead < 0x20)
                    {
                        bits &= (1 << limitLead) - 1;
                    }
                    for (trail = 0; trail < 64; ++trail)
                    {
                        table[trail] |= bits;
                    }
                }
                // limit<=0x800. If limit==0x800 then limitLead=32 and limitTrail=0.
                // In that case, bits=1<<limitLead == 1<<0 == 1
                // (because Java << uses only the lower 5 bits of the shift operand)
                // but the bits value is not used because trail<limitTrail is already false.
                bits = 1 << limitLead;
                for (trail = 0; trail < limitTrail; ++trail)
                {
                    table[trail] |= bits;
                }
            }
        }

        private void InitBits()
        {
            int start, limit;
            int listIndex = 0;

            // Set latin1Contains[].
            do
            {
                start = list[listIndex++];
                if (listIndex < listLength)
                {
                    limit = list[listIndex++];
                }
                else
                {
                    limit = 0x110000;
                }
                if (start >= 0x100)
                {
                    break;
                }
                do
                {
                    latin1Contains[start++] = true;
                } while (start < limit && start < 0x100);
            } while (limit <= 0x100);

            // Set table7FF[].
            while (start < 0x800)
            {
                Set32x64Bits(table7FF, start, limit <= 0x800 ? limit : 0x800);
                if (limit > 0x800)
                {
                    start = 0x800;
                    break;
                }

                start = list[listIndex++];
                if (listIndex < listLength)
                {
                    limit = list[listIndex++];
                }
                else
                {
                    limit = 0x110000;
                }
            }

            // Set bmpBlockBits[].
            int minStart = 0x800;
            while (start < 0x10000)
            {
                if (limit > 0x10000)
                {
                    limit = 0x10000;
                }

                if (start < minStart)
                {
                    start = minStart;
                }
                if (start < limit)
                { // Else: Another range entirely in a known mixed-value block.
                    if (0 != (start & 0x3f))
                    {
                        // Mixed-value block of 64 code points.
                        start >>= 6;
                        bmpBlockBits[start & 0x3f] |= 0x10001 << (start >> 6);
                        start = (start + 1) << 6; // Round up to the next block boundary.
                        minStart = start; // Ignore further ranges in this block.
                    }
                    if (start < limit)
                    {
                        if (start < (limit & ~0x3f))
                        {
                            // Multiple all-ones blocks of 64 code points each.
                            Set32x64Bits(bmpBlockBits, start >> 6, limit >> 6);
                        }

                        if (0 != (limit & 0x3f))
                        {
                            // Mixed-value block of 64 code points.
                            limit >>= 6;
                            bmpBlockBits[limit & 0x3f] |= 0x10001 << (limit >> 6);
                            limit = (limit + 1) << 6; // Round up to the next block boundary.
                            minStart = limit; // Ignore further ranges in this block.
                        }
                    }
                }

                if (limit == 0x10000)
                {
                    break;
                }

                start = list[listIndex++];
                if (listIndex < listLength)
                {
                    limit = list[listIndex++];
                }
                else
                {
                    limit = 0x110000;
                }
            }
        }

        /// <summary>
        /// Same as <see cref="Text.UnicodeSet.FindCodePoint(int)"/> except that the binary search is restricted for finding code
        /// points in a certain range.
        /// <para/>
        /// For restricting the search for finding in the range start..end, pass in lo=FindCodePoint(start) and
        /// hi=FindCodePoint(end) with 0&lt;=lo&lt;=hi&lt;len. FindCodePoint(c) defaults to lo=0 and hi=len-1.
        /// </summary>
        /// <param name="c">A character in a subrange of MIN_VALUE..MAX_VALUE.</param>
        /// <param name="lo">The lowest index to be returned.</param>
        /// <param name="hi">The highest index to be returned.</param>
        /// <returns>The smallest integer i in the range lo..hi, inclusive, such that c &lt; list[i].</returns>
        private int FindCodePoint(int c, int lo, int hi)
        {
            /* Examples:
                                               findCodePoint(c)
               set              list[]         c=0 1 3 4 7 8
               ===              ==============   ===========
               []               [110000]         0 0 0 0 0 0
               [\u0000-\u0003]  [0, 4, 110000]   1 1 1 2 2 2
               [\u0004-\u0007]  [4, 8, 110000]   0 0 0 1 1 2
               [:Any:]          [0, 110000]      1 1 1 1 1 1
             */

            // Return the smallest i such that c < list[i]. Assume
            // list[len - 1] == HIGH and that c is legal (0..HIGH-1).
            if (c < list[lo])
                return lo;
            // High runner test. c is often after the last range, so an
            // initial check for this condition pays off.
            if (lo >= hi || c >= list[hi - 1])
                return hi;
            // invariant: c >= list[lo]
            // invariant: c < list[hi]
            for (; ; )
            {
                int i = (lo + hi).TripleShift(1);
                if (i == lo)
                {
                    break; // Found!
                }
                else if (c < list[i])
                {
                    hi = i;
                }
                else
                {
                    lo = i;
                }
            }
            return hi;
        }

        private bool ContainsSlow(int c, int lo, int hi)
        {
            return (0 != (FindCodePoint(c, lo, hi) & 1));
        }
    }
}
