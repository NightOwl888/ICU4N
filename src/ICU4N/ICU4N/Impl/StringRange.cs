using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    public class StringRange
    {
        private static readonly bool DEBUG = false;

        public interface IAdder // ICU4N TODO: API - de-nest ?
        {
            /// <param name="start"></param>
            /// <param name="end">May be null, for adding single string.</param>
            void Add(string start, string end);
        }

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

        public static readonly IComparer<int[]> COMPARE_INT_ARRAYS = new Int32ArrayComparer(); // ICU4N TODO: API - rename to follow .NET Conventions

        /// <summary>
        /// Compact the set of strings.
        /// </summary>
        /// <param name="source">Set of strings.</param>
        /// <param name="adder">Adds each pair to the output. See the <see cref="IAdder"/> interface.</param>
        /// <param name="shorterPairs">Use abc-d instead of abc-abd.</param>
        /// <param name="moreCompact">Use a more compact form, at the expense of more processing. If false, source must be sorted.</param>
        public static void Compact(ISet<string> source, IAdder adder, bool shorterPairs, bool moreCompact)
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
                        if (s.RegionMatches(0, start, 0, prefixLen))
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
                throw new NotImplementedException();
                // ICU4N TODO: Finish implementation
                //        // not a fast algorithm, but ok for now
                //        // TODO rewire to use the first (slower) algorithm to generate the ranges, then compact them from there.
                //        // first sort by lengths
                //        Relation<int, Ranges> lengthToArrays = Relation.of(new TreeMap<int, Set<Ranges>>(), TreeSet.class);
                //        for (String s : source) {
                //            Ranges item = new Ranges(s);
                //lengthToArrays.put(item.size(), item);
                //        }
                //// then compact items of each length and emit compacted sets
                //foreach (var entry in lengthToArrays.keyValuesSet())
                //{
                //    LinkedList<Ranges> compacted = compact(entry.getKey(), entry.getValue());
                //    for (Ranges ranges : compacted)
                //    {
                //        adder.add(ranges.start(), ranges.end(shorterPairs));
                //    }
                //}
            }
        }

        /// <summary>
        /// Faster but not as good compaction. Only looks at final codepoint.
        /// </summary>
        /// <param name="source">Set of strings.</param>
        /// <param name="adder">Adds each pair to the output. See the <see cref="IAdder"/> interface.</param>
        /// <param name="shorterPairs">Use abc-d instead of abc-abd.</param>
        public static void Compact(ISet<string> source, IAdder adder, bool shorterPairs)
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
                StringBuilder result = new StringBuilder().AppendCodePoint(Min);
                return Max == Max ? result.ToString() : result.Append('~').AppendCodePoint(Max).ToString();
            }
        }

        internal sealed class Ranges : IComparable<Ranges>
        {
            private readonly Range[] ranges;
            public Ranges(string s)
            {
                int[] array = CharSequences.CodePoints(s);
                ranges = new Range[array.Length];
                for (int i = 0; i < array.Length; ++i)
                {
                    ranges[i] = new Range(array[i], array[i]);
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
                StringBuilder result = new StringBuilder();
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
                StringBuilder result = new StringBuilder();
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
            public int Length
            {
                get { return ranges.Length; }
            }

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
            int[] startCps = CharSequences.CodePoints(start);
            int[] endCps = CharSequences.CodePoints(end);
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

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < startOffset; ++i)
            {
                builder.AppendCodePoint(startCps[i]);
            }
            Add(0, startOffset, startCps, endCps, builder, output);
            return output;
        }

        private static void Add(int endIndex, int startOffset, int[] starts, int[] ends, StringBuilder builder, ICollection<string> output)
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
                    output.Add(builder.ToString());
                }
                else
                {
                    Add(endIndex + 1, startOffset, starts, ends, builder, output);
                }
                builder.Length = startLen;
            }
        }
    }
}
