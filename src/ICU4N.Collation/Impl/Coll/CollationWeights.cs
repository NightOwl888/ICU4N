using System;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Allocates n collation element weights between two exclusive limits.
    /// Used only internally by the collation tailoring builder.
    /// </summary>
    /// <since>2001mar08</since>
    /// <author>Markus W. Scherer</author>
    public sealed class CollationWeights
    {
        public CollationWeights() { }

        public void InitForPrimary(bool compressible)
        {
            middleLength = 1;
            minBytes[1] = Collation.MergeSeparatorByte + 1;
            maxBytes[1] = Collation.TRAIL_WEIGHT_BYTE;
            if (compressible)
            {
                minBytes[2] = Collation.PrimaryCompressionLowByte + 1;
                maxBytes[2] = Collation.PrimaryCompressionHighByte - 1;
            }
            else
            {
                minBytes[2] = 2;
                maxBytes[2] = 0xff;
            }
            minBytes[3] = 2;
            maxBytes[3] = 0xff;
            minBytes[4] = 2;
            maxBytes[4] = 0xff;
        }

        public void InitForSecondary()
        {
            // We use only the lower 16 bits for secondary weights.
            middleLength = 3;
            minBytes[1] = 0;
            maxBytes[1] = 0;
            minBytes[2] = 0;
            maxBytes[2] = 0;
            minBytes[3] = Collation.LevelSeparatorByte + 1;
            maxBytes[3] = 0xff;
            minBytes[4] = 2;
            maxBytes[4] = 0xff;
        }

        public void InitForTertiary()
        {
            // We use only the lower 16 bits for tertiary weights.
            middleLength = 3;
            minBytes[1] = 0;
            maxBytes[1] = 0;
            minBytes[2] = 0;
            maxBytes[2] = 0;
            // We use only 6 bits per byte.
            // The other bits are used for case & quaternary weights.
            minBytes[3] = Collation.LevelSeparatorByte + 1;
            maxBytes[3] = 0x3f;
            minBytes[4] = 2;
            maxBytes[4] = 0x3f;
        }

        /// <summary>
        /// Determine heuristically
        /// what ranges to use for a given number of weights between (excluding)
        /// two limits.
        /// </summary>
        /// <param name="lowerLimit">A collation element weight; the ranges will be filled to cover
        /// weights greater than this one.</param>
        /// <param name="upperLimit">A collation element weight; the ranges will be filled to cover
        /// weights less than this one.</param>
        /// <param name="n">The number of collation element weights w necessary such that
        /// lowerLimit&lt;w&lt;upperLimit in lexical order.</param>
        /// <returns><c>true</c> if it is possible to fit n elements between the limits.</returns>
        public bool AllocWeights(long lowerLimit, long upperLimit, int n)
        {
            // Call getWeightRanges() and then determine heuristically
            // which ranges to use for a given number of weights between (excluding)
            // two limits.
            // puts("");

            if (!GetWeightRanges(lowerLimit, upperLimit))
            {
                // printf("error: unable to get Weight ranges\n");
                return false;
            }

            /* try until we find suitably large ranges */
            for (; ; )
            {
                /* get the smallest number of bytes in a range */
                int minLength = ranges[0].Length;

                if (AllocWeightsInShortRanges(n, minLength)) { break; }

                if (minLength == 4)
                {
                    // printf("error: the maximum number of %ld weights is insufficient for n=%ld\n",
                    //       minLengthCount, n);
                    return false;
                }

                if (AllocWeightsInMinLengthRanges(n, minLength)) { break; }

                /* no good match, lengthen all minLength ranges and iterate */
                // printf("lengthen the short ranges from %ld bytes to %ld and iterate\n", minLength, minLength+1);
                for (int i = 0; i < rangeCount && ranges[i].Length == minLength; ++i)
                {
                    LengthenRange(ranges[i]);
                }
            }

            /* puts("final ranges:");
            for(int i=0; i<rangeCount; ++i) {
                printf("ranges[%ld] .start=0x%08lx .end=0x%08lx .length=%ld .count=%ld\n",
                      i, ranges[i].start, ranges[i].end, ranges[i].length, ranges[i].count);
            } */

            rangeIndex = 0;
            if (rangeCount < ranges.Length)
            {
                ranges[rangeCount] = null;  // force a crash when going out of bounds
            }
            return true;
        }

        /// <summary>
        /// Given a set of ranges calculated by <see cref="AllocWeights(long, long, int)"/>,
        /// iterate through the weights.
        /// The ranges are modified to keep the current iteration state.
        /// </summary>
        /// <returns>The next weight in the ranges, or 0xffffffff if there is none left.</returns>
        public long NextWeight()
        {
            if (rangeIndex >= rangeCount)
            {
                return 0xffffffffL;
            }
            else
            {
                /* get the next weight */
                WeightRange range = ranges[rangeIndex];
                long weight = range.Start;
                if (--range.Count == 0)
                {
                    /* this range is finished */
                    ++rangeIndex;
                }
                else
                {
                    /* increment the weight for the next value */
                    range.Start = IncWeight(weight, range.Length);
                    Debug.Assert(range.Start <= range.End);
                }

                return weight;
            }
        }

        /// <internal/>
        private sealed class WeightRange : IComparable<WeightRange>
        {
            internal long Start { get; set; }
            internal long End { get; set; }
            internal int Length { get; set; }
            internal int Count { get; set; }

            public int CompareTo(WeightRange other)
            {
                long l = Start;
                long r = other.Start;
                if (l < r)
                {
                    return -1;
                }
                else if (l > r)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /* helper functions for CE weights */

        public static int LengthOfWeight(long weight)
        {
            if ((weight & 0xffffff) == 0)
            {
                return 1;
            }
            else if ((weight & 0xffff) == 0)
            {
                return 2;
            }
            else if ((weight & 0xff) == 0)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        private static int GetWeightTrail(long weight, int length)
        {
            return (int)(weight >> (8 * (4 - length))) & 0xff;
        }

        private static long SetWeightTrail(long weight, int length, int trail)
        {
            length = 8 * (4 - length);
            return (weight & (0xffffff00L << length)) | ((long)trail << length);
        }

        private static int GetWeightByte(long weight, int idx)
        {
            return GetWeightTrail(weight, idx); /* same calculation */
        }

        private static long SetWeightByte(long weight, int idx, int b)
        {
            long mask; /* 0xffffffff except a 00 "hole" for the index-th byte */

            idx *= 8;
            if (idx < 32)
            {
                mask = 0xffffffffL >> idx;
            }
            else
            {
                // Do not use int>>32 because on some platforms that does not shift at all
                // while we need it to become 0.
                // PowerPC: 0xffffffff>>32 = 0           (wanted)
                // x86:     0xffffffff>>32 = 0xffffffff  (not wanted)
                //
                // ANSI C99 6.5.7 Bitwise shift operators:
                // "If the value of the right operand is negative
                // or is greater than or equal to the width of the promoted left operand,
                // the behavior is undefined."
                mask = 0;
            }
            idx = 32 - idx;
            mask |= 0xffffff00L << idx;
            return (weight & mask) | ((long)b << idx);
        }

        private static long TruncateWeight(long weight, int length)
        {
            return weight & (0xffffffffL << (8 * (4 - length)));
        }

        private static long IncWeightTrail(long weight, int length)
        {
            return weight + (1L << (8 * (4 - length)));
        }

        private static long DecWeightTrail(long weight, int length)
        {
            return weight - (1L << (8 * (4 - length)));
        }

        /// <returns>Number of usable byte values for byte <paramref name="idx"/>.</returns>
        private int CountBytes(int idx)
        {
            return maxBytes[idx] - minBytes[idx] + 1;
        }

        private long IncWeight(long weight, int length)
        {
            for (; ; )
            {
                int b = GetWeightByte(weight, length);
                if (b < maxBytes[length])
                {
                    return SetWeightByte(weight, length, b + 1);
                }
                else
                {
                    // Roll over, set this byte to the minimum and increment the previous one.
                    weight = SetWeightByte(weight, length, minBytes[length]);
                    --length;
                    Debug.Assert(length > 0);
                }
            }
        }

        private long IncWeightByOffset(long weight, int length, int offset)
        {
            for (; ; )
            {
                offset += GetWeightByte(weight, length);
                if (offset <= maxBytes[length])
                {
                    return SetWeightByte(weight, length, offset);
                }
                else
                {
                    // Split the offset between this byte and the previous one.
                    offset -= minBytes[length];
                    weight = SetWeightByte(weight, length, minBytes[length] + offset % CountBytes(length));
                    offset /= CountBytes(length);
                    --length;
                    Debug.Assert(length > 0);
                }
            }
        }

        private void LengthenRange(WeightRange range)
        {
            int length = range.Length + 1;
            range.Start = SetWeightTrail(range.Start, length, minBytes[length]);
            range.End = SetWeightTrail(range.End, length, maxBytes[length]);
            range.Count *= CountBytes(length);
            range.Length = length;
        }

        /// <summary>
        /// Takes two CE weights and calculates the
        /// possible ranges of weights between the two limits, excluding them.
        /// For weights with up to 4 bytes there are up to 2*4-1=7 ranges.
        /// </summary>
        private bool GetWeightRanges(long lowerLimit, long upperLimit)
        {
            Debug.Assert(lowerLimit != 0);
            Debug.Assert(upperLimit != 0);

            /* get the lengths of the limits */
            int lowerLength = LengthOfWeight(lowerLimit);
            int upperLength = LengthOfWeight(upperLimit);

            // printf("length of lower limit 0x%08lx is %ld\n", lowerLimit, lowerLength);
            // printf("length of upper limit 0x%08lx is %ld\n", upperLimit, upperLength);
            Debug.Assert(lowerLength >= middleLength);
            // Permit upperLength<middleLength: The upper limit for secondaries is 0x10000.

            if (lowerLimit >= upperLimit)
            {
                // printf("error: no space between lower & upper limits\n");
                return false;
            }

            /* check that neither is a prefix of the other */
            if (lowerLength < upperLength)
            {
                if (lowerLimit == TruncateWeight(upperLimit, lowerLength))
                {
                    // printf("error: lower limit 0x%08lx is a prefix of upper limit 0x%08lx\n", lowerLimit, upperLimit);
                    return false;
                }
            }
            /* if the upper limit is a prefix of the lower limit then the earlier test lowerLimit>=upperLimit has caught it */

            WeightRange[] lower = new WeightRange[5]; /* [0] and [1] are not used - this simplifies indexing */
            WeightRange middle = new WeightRange();
            WeightRange[] upper = new WeightRange[5];

            /*
             * With the limit lengths of 1..4, there are up to 7 ranges for allocation:
             * range     minimum length
             * lower[4]  4
             * lower[3]  3
             * lower[2]  2
             * middle    1
             * upper[2]  2
             * upper[3]  3
             * upper[4]  4
             *
             * We are now going to calculate up to 7 ranges.
             * Some of them will typically overlap, so we will then have to merge and eliminate ranges.
             */
            long weight = lowerLimit;
            for (int length = lowerLength; length > middleLength; --length)
            {
                int trail = GetWeightTrail(weight, length);
                if (trail < maxBytes[length])
                {
                    lower[length] = new WeightRange();
                    lower[length].Start = IncWeightTrail(weight, length);
                    lower[length].End = SetWeightTrail(weight, length, maxBytes[length]);
                    lower[length].Length = length;
                    lower[length].Count = maxBytes[length] - trail;
                }
                weight = TruncateWeight(weight, length - 1);
            }
            if (weight < 0xff000000L)
            {
                middle.Start = IncWeightTrail(weight, middleLength);
            }
            else
            {
                // Prevent overflow for primary lead byte FF
                // which would yield a middle range starting at 0.
                middle.Start = 0xffffffffL;  // no middle range
            }

            weight = upperLimit;
            for (int length = upperLength; length > middleLength; --length)
            {
                int trail = GetWeightTrail(weight, length);
                if (trail > minBytes[length])
                {
                    upper[length] = new WeightRange();
                    upper[length].Start = SetWeightTrail(weight, length, minBytes[length]);
                    upper[length].End = DecWeightTrail(weight, length);
                    upper[length].Length = length;
                    upper[length].Count = trail - minBytes[length];
                }
                weight = TruncateWeight(weight, length - 1);
            }
            middle.End = DecWeightTrail(weight, middleLength);

            /* set the middle range */
            middle.Length = middleLength;
            if (middle.End >= middle.Start)
            {
                middle.Count = (int)((middle.End - middle.Start) >> (8 * (4 - middleLength))) + 1;
            }
            else
            {
                /* no middle range, eliminate overlaps */
                for (int length = 4; length > middleLength; --length)
                {
                    if (lower[length] != null && upper[length] != null &&
                            lower[length].Count > 0 && upper[length].Count > 0)
                    {
                        // Note: The lowerEnd and upperStart weights are versions of
                        // lowerLimit and upperLimit (which are lowerLimit<upperLimit),
                        // truncated (still less-or-equal)
                        // and then with their last bytes changed to the
                        // maxByte (for lowerEnd) or minByte (for upperStart).
                        long lowerEnd = lower[length].End;
                        long upperStart = upper[length].Start;
                        bool merged = false;

                        if (lowerEnd > upperStart)
                        {
                            // These two lower and upper ranges collide.
                            // Since lowerLimit<upperLimit and lowerEnd and upperStart
                            // are versions with only their last bytes modified
                            // (and following ones removed/reset to 0),
                            // lowerEnd>upperStart is only possible
                            // if the leading bytes are equal
                            // and lastByte(lowerEnd)>lastByte(upperStart).
                            Debug.Assert(TruncateWeight(lowerEnd, length - 1) ==
                                    TruncateWeight(upperStart, length - 1));
                            // Intersect these two ranges.
                            lower[length].End = upper[length].End;
                            lower[length].Count =
                                    GetWeightTrail(lower[length].End, length) -
                                    GetWeightTrail(lower[length].Start, length) + 1;
                            // count might be <=0 in which case there is no room,
                            // and the range-collecting code below will ignore this range.
                            merged = true;
                        }
                        else if (lowerEnd == upperStart)
                        {
                            // Not possible, unless minByte==maxByte which is not allowed.
                            Debug.Assert(minBytes[length] < maxBytes[length]);
                        }
                        else /* lowerEnd<upperStart */
                        {
                            if (IncWeight(lowerEnd, length) == upperStart)
                            {
                                // Merge adjacent ranges.
                                lower[length].End = upper[length].End;
                                lower[length].Count += upper[length].Count;  // might be >countBytes
                                merged = true;
                            }
                        }
                        if (merged)
                        {
                            // Remove all shorter ranges.
                            // There was no room available for them between the ranges we just merged.
                            upper[length].Count = 0;
                            while (--length > middleLength)
                            {
                                lower[length] = upper[length] = null;
                            }
                            break;
                        }
                    }
                }
            }

            /* print ranges
            for(int length=4; length>=2; --length) {
                if(lower[length].count>0) {
                    printf("lower[%ld] .start=0x%08lx .end=0x%08lx .count=%ld\n", length, lower[length].start, lower[length].end, lower[length].count);
                }
            }
            if(middle.count>0) {
                printf("middle   .start=0x%08lx .end=0x%08lx .count=%ld\n", middle.start, middle.end, middle.count);
            }
            for(int length=2; length<=4; ++length) {
                if(upper[length].count>0) {
                    printf("upper[%ld] .start=0x%08lx .end=0x%08lx .count=%ld\n", length, upper[length].start, upper[length].end, upper[length].count);
                }
            } */

            /* copy the ranges, shortest first, into the result array */
            rangeCount = 0;
            if (middle.Count > 0)
            {
                ranges[0] = middle;
                rangeCount = 1;
            }
            for (int length = middleLength + 1; length <= 4; ++length)
            {
                /* copy upper first so that later the middle range is more likely the first one to use */
                if (upper[length] != null && upper[length].Count > 0)
                {
                    ranges[rangeCount++] = upper[length];
                }
                if (lower[length] != null && lower[length].Count > 0)
                {
                    ranges[rangeCount++] = lower[length];
                }
            }
            return rangeCount > 0;
        }

        private bool AllocWeightsInShortRanges(int n, int minLength)
        {
            // See if the first few minLength and minLength+1 ranges have enough weights.
            for (int i = 0; i < rangeCount && ranges[i].Length <= (minLength + 1); ++i)
            {
                if (n <= ranges[i].Count)
                {
                    // Use the first few minLength and minLength+1 ranges.
                    if (ranges[i].Length > minLength)
                    {
                        // Reduce the number of weights from the last minLength+1 range
                        // which might sort before some minLength ranges,
                        // so that we use all weights in the minLength ranges.
                        ranges[i].Count = n;
                    }
                    rangeCount = i + 1;
                    // printf("take first %ld ranges\n", rangeCount);

                    if (rangeCount > 1)
                    {
                        /* sort the ranges by weight values */
                        Array.Sort(ranges, 0, rangeCount);
                    }
                    return true;
                }
                n -= ranges[i].Count;  // still >0
            }
            return false;
        }

        private bool AllocWeightsInMinLengthRanges(int n, int minLength)
        {
            // See if the minLength ranges have enough weights
            // when we split one and lengthen the following ones.
            int count = 0;
            int minLengthRangeCount;
            for (minLengthRangeCount = 0;
                    minLengthRangeCount < rangeCount &&
                        ranges[minLengthRangeCount].Length == minLength;
                    ++minLengthRangeCount)
            {
                count += ranges[minLengthRangeCount].Count;
            }

            int nextCountBytes = CountBytes(minLength + 1);
            if (n > count * nextCountBytes) { return false; }

            // Use the minLength ranges. Merge them, and then split again as necessary.
            long start = ranges[0].Start;
            long end = ranges[0].End;
            for (int i = 1; i < minLengthRangeCount; ++i)
            {
                if (ranges[i].Start < start) { start = ranges[i].Start; }
                if (ranges[i].End > end) { end = ranges[i].End; }
            }

            // Calculate how to split the range between minLength (count1) and minLength+1 (count2).
            // Goal:
            //   count1 + count2 * nextCountBytes = n
            //   count1 + count2 = count
            // These turn into
            //   (count - count2) + count2 * nextCountBytes = n
            // and then into the following count1 & count2 computations.
            int count2 = (n - count) / (nextCountBytes - 1);  // number of weights to be lengthened
            int count1 = count - count2;  // number of minLength weights
            if (count2 == 0 || (count1 + count2 * nextCountBytes) < n)
            {
                // round up
                ++count2;
                --count1;
                Debug.Assert((count1 + count2 * nextCountBytes) >= n);
            }

            ranges[0].Start = start;

            if (count1 == 0)
            {
                // Make one long range.
                ranges[0].End = end;
                ranges[0].Count = count;
                LengthenRange(ranges[0]);
                rangeCount = 1;
            }
            else
            {
                // Split the range, lengthen the second part.
                // printf("split the range number %ld (out of %ld minLength ranges) by %ld:%ld\n",
                //       splitRange, rangeCount, count1, count2);

                // Next start = start + count1. First end = 1 before that.
                ranges[0].End = IncWeightByOffset(start, minLength, count1 - 1);
                ranges[0].Count = count1;

                if (ranges[1] == null)
                {
                    ranges[1] = new WeightRange();
                }
                ranges[1].Start = IncWeight(ranges[0].End, minLength);
                ranges[1].End = end;
                ranges[1].Length = minLength;  // +1 when lengthened
                ranges[1].Count = count2;  // *countBytes when lengthened
                LengthenRange(ranges[1]);
                rangeCount = 2;
            }
            return true;
        }

        private int middleLength;
        private int[] minBytes = new int[5];  // for byte 1, 2, 3, 4
        private int[] maxBytes = new int[5];
        private WeightRange[] ranges = new WeightRange[7];
        private int rangeIndex;
        private int rangeCount;
    }
}
