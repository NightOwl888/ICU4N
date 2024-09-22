using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    public interface IStringRangeAdder
    {
        /// <param name="start"></param>
        /// <param name="end">May be null, for adding single string.</param>
        void Add(string start, string end);
    }

    public class StringRange
    {
        private const int CharStackBufferSize = 32;
        private const int Int32StackBufferSize = 32;

        private static readonly bool DEBUG = false;

        private class Int32ArrayComparer : IComparer<int[]>
        {
            public int Compare(int[] o1, int[] o2)
            {
                int minIndex = Math.Min(o1.Length, o2.Length);
                for (int i = 0; i < minIndex; ++i)
                {
                    int diff = o1[i] - o2[i];
                    if (diff != 0)
                    {
                        return diff;
                    }
                }
                return o1.Length - o2.Length;
            }
        }

        public static readonly IComparer<int[]> ComparInt32Arrays = new Int32ArrayComparer();

        /// <summary>
        /// Compact the set of strings.
        /// </summary>
        /// <param name="source">Set of strings.</param>
        /// <param name="adder">Adds each pair to the output. See the <see cref="IStringRangeAdder"/> interface.</param>
        /// <param name="shorterPairs">Use abc-d instead of abc-abd.</param>
        /// <param name="moreCompact">Use a more compact form, at the expense of more processing. If false, source must be sorted.</param>
        public static void Compact(ISet<string> source, IStringRangeAdder adder, bool shorterPairs, bool moreCompact)
        {
            if (!moreCompact)
            {
                string start = null;
                string end = null;
                int lastCp = 0;
                int prefixLen = 0;
                foreach (string s in source)
                {
                    if (start != null)
                    { // We have something queued up
                        if (s.RegionMatches(0, start, 0, prefixLen, StringComparison.Ordinal))
                        {
                            int currentCp = s.CodePointAt(prefixLen);
                            if (currentCp == 1 + lastCp && s.Length == prefixLen + Character.CharCount(currentCp))
                            {
                                end = s;
                                lastCp = currentCp;
                                continue;
                            }
                        }
                        // We failed to find continuation. Add what we have and restart
                        adder.Add(start, end == null ? null
                            : !shorterPairs ? end
                                : end.Substring(prefixLen, end.Length - prefixLen)); // ICU4N: Corrected 2nd parameter
                    }
                    // new possible range
                    start = s;
                    end = null;
                    lastCp = s.CodePointBefore(s.Length);
                    prefixLen = s.Length - Character.CharCount(lastCp);
                }
                adder.Add(start, end == null ? null
                    : !shorterPairs ? end
                        : end.Substring(prefixLen, end.Length - prefixLen)); // ICU4N: Corrected 2nd parameter
            }
            else
            {
                // not a fast algorithm, but ok for now
                // TODO rewire to use the first (slower) algorithm to generate the ranges, then compact them from there.
                // first sort by lengths
                Relation<int, Ranges> lengthToArrays = Relation.Of(new SortedDictionary<int, ISet<Ranges>>(), typeof(SortedDictionary<int, ISet<Ranges>>));
                foreach (string s in source)
                {
                    Ranges item = new Ranges(s);
                    lengthToArrays.Put(item.Length, item);
                }
                // then compact items of each length and emit compacted sets
                foreach (var entry in lengthToArrays.KeyValues)
                {
                    LinkedList<Ranges> compacted = Compact(entry.Key, entry.Value);
                    foreach (Ranges ranges in compacted)
                    {
                        adder.Add(ranges.Start(), ranges.End(shorterPairs));
                    }
                }
            }
        }

        /// <summary>
        /// Faster but not as good compaction. Only looks at final codepoint.
        /// </summary>
        /// <param name="source">Set of strings.</param>
        /// <param name="adder">Adds each pair to the output. See the <see cref="IStringRangeAdder"/> interface.</param>
        /// <param name="shorterPairs">Use abc-d instead of abc-abd.</param>
        public static void Compact(ISet<string> source, IStringRangeAdder adder, bool shorterPairs)
        {
            Compact(source, adder, shorterPairs, false);
        }

        private static LinkedList<Ranges> Compact(int size, ISet<Ranges> inputRanges)
        {
            LinkedList<Ranges> ranges = new LinkedList<Ranges>(inputRanges);
            for (int i = size - 1; i >= 0; --i)
            {
                List<Ranges> toRemove = new List<Ranges>();
                Ranges last = null;
                foreach (Ranges item in ranges)
                {
                    if (last == null)
                    {
                        last = item;
                    }
                    else if (last.Merge(i, item))
                    {
                        //it.remove();
                        toRemove.Add(item);
                    }
                    else
                    {
                        last = item; // go to next
                    }
                }

                // Purge removable items
                foreach (var item in toRemove)
                    ranges.Remove(item);
            };
            return ranges;
        }

        internal sealed class Range : IComparable<Range>
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public Range(int min, int max)
            {
                this.Min = min;
                this.Max = max;
            }

            public override bool Equals(object obj)
            {
                return this == obj || (obj != null && obj is Range && CompareTo((Range)obj) == 0);
            }

            public int CompareTo(Range that)
            {
                int diff = Min - that.Min;
                if (diff != 0)
                {
                    return diff;
                }
                return Max - that.Max;
            }

            public override int GetHashCode()
            {
                return Max * 37 + Max;
            }

            public override string ToString()
            {
                using ValueStringBuilder result = new ValueStringBuilder(stackalloc char[8]);
                result.AppendCodePoint(Min);
                if (Max == Max)
                {
                    return result.ToString();
                }
                else
                {
                    result.Append('~');
                    result.AppendCodePoint(Max);
                    return result.ToString();
                }
            }
        }

        internal sealed class Ranges : IComparable<Ranges>
        {
            private readonly Range[] ranges;
            public Ranges(string s)
            {
                int[] arrayToReturnToPool = null;
                try
                {
                    Span<int> buffer = s.Length > Int32StackBufferSize
                        ? (arrayToReturnToPool = ArrayPool<int>.Shared.Rent(s.Length))
                        : stackalloc int[s.Length];
#pragma warning disable 612, 618
                    ReadOnlySpan<int> array = CharSequences.CodePoints(s, buffer);
#pragma warning restore 612, 618
                    ranges = new Range[array.Length];
                    for (int i = 0; i < array.Length; ++i)
                    {
                        ranges[i] = new Range(array[i], array[i]);
                    }
                }
                finally
                {
                    ArrayPool<int>.Shared.ReturnIfNotNull(arrayToReturnToPool);
                }
            }
            public bool Merge(int pivot, Ranges other)
            {
                // We merge items if the pivot is adjacent, and all later ranges are equal.
                for (int i = ranges.Length - 1; i >= 0; --i)
                {
                    if (i == pivot)
                    {
                        if (ranges[i].Max != other.ranges[i].Min - 1)
                        { // not adjacent
                            return false;
                        }
                    }
                    else
                    {
                        if (!ranges[i].Equals(other.ranges[i]))
                        {
                            return false;
                        }
                    }
                }
                if (DEBUG) Console.Out.Write("Merging: " + this + ", " + other);
                ranges[pivot].Max = other.ranges[pivot].Max;
                if (DEBUG) Console.Out.WriteLine(" => " + this);
                return true;
            }

            public string Start()
            {
                using ValueStringBuilder result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                for (int i = 0; i < ranges.Length; ++i)
                {
                    result.AppendCodePoint(ranges[i].Min);
                }
                return result.ToString();
            }
            public string End(bool mostCompact)
            {
                int firstDiff = FirstDifference();
                if (firstDiff == ranges.Length)
                {
                    return null;
                }
                using ValueStringBuilder result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                for (int i = mostCompact ? firstDiff : 0; i < ranges.Length; ++i)
                {
                    result.AppendCodePoint(ranges[i].Max);
                }
                return result.ToString();
            }
            public int FirstDifference()
            {
                for (int i = 0; i < ranges.Length; ++i)
                {
                    if (ranges[i].Min != ranges[i].Max)
                    {
                        return i;
                    }
                }
                return ranges.Length;
            }
            public int Length => ranges.Length;

            public int CompareTo(Ranges other)
            {
                int diff = ranges.Length - other.ranges.Length;
                if (diff != 0)
                {
                    return diff;
                }
                for (int i = 0; i < ranges.Length; ++i)
                {
                    diff = ranges[i].CompareTo(other.ranges[i]);
                    if (diff != 0)
                    {
                        return diff;
                    }
                }
                return 0;
            }

            public override string ToString()
            {
                string start = Start();
                string end = End(false);
                return end == null ? start : start + "~" + end;
            }
        }

        public static ICollection<string> Expand(string start, string end, bool requireSameLength, ICollection<string> output)
        {
            if (start == null || end == null)
            {
                throw new ICUException("Range must have 2 valid strings");
            }
            int[] startCpsArrayToReturnToPool = null;
            int[] endCpsArrayToReturnToPool = null;
            try
            {
                Span<int> startCpsBuffer = start.Length > Int32StackBufferSize
                    ? (startCpsArrayToReturnToPool = ArrayPool<int>.Shared.Rent(start.Length))
                    : stackalloc int[start.Length];
                Span<int> endCpsBuffer = end.Length > Int32StackBufferSize
                    ? (endCpsArrayToReturnToPool = ArrayPool<int>.Shared.Rent(end.Length))
                    : stackalloc int[end.Length];

#pragma warning disable 612, 618
                ReadOnlySpan<int> startCps = CharSequences.CodePoints(start, startCpsBuffer);
                ReadOnlySpan<int> endCps = CharSequences.CodePoints(end, endCpsBuffer);
#pragma warning restore 612, 618
                int startOffset = startCps.Length - endCps.Length;

                if (requireSameLength && startOffset != 0)
                {
                    throw new ICUException("Range must have equal-length strings");
                }
                else if (startOffset < 0)
                {
                    throw new ICUException("Range must have start-length ≥ end-length");
                }
                else if (endCps.Length == 0)
                {
                    throw new ICUException("Range must have end-length > 0");
                }

                ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                try
                {
                    for (int i = 0; i < startOffset; ++i)
                    {
                        builder.AppendCodePoint(startCps[i]);
                    }
                    Add(0, startOffset, startCps, endCps, ref builder, output);
                    return output;
                }
                finally
                {
                    builder.Dispose();
                }
            }
            finally
            {
                ArrayPool<int>.Shared.ReturnIfNotNull(startCpsArrayToReturnToPool);
                ArrayPool<int>.Shared.ReturnIfNotNull(endCpsArrayToReturnToPool);
            }
        }

        private static void Add(int endIndex, int startOffset, ReadOnlySpan<int> starts, ReadOnlySpan<int> ends, ref ValueStringBuilder builder, ICollection<string> output)
        {
            int start = starts[endIndex + startOffset];
            int end = ends[endIndex];
            if (start > end)
            {
                throw new ICUException("Range must have xᵢ ≤ yᵢ for each index i");
            }
            bool last = endIndex == ends.Length - 1;
            int startLen = builder.Length;
            for (int i = start; i <= end; ++i)
            {
                builder.AppendCodePoint(i);
                if (last)
                {
                    output.Add(builder.AsSpan().ToString());
                }
                else
                {
                    Add(endIndex + 1, startOffset, starts, ends, ref builder, output);
                }
                builder.Length = startLen;
            }
        }
    }
}
