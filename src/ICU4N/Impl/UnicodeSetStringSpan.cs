using ICU4N.Support.Text;
using ICU4N.Text;
using System.Collections.Generic;
using System.Diagnostics;
using static ICU4N.Text.UnicodeSet;

namespace ICU4N.Impl
{
    /// <summary>
    /// Implement <see cref="Span(string, int, SpanCondition)"/> etc. for a set with strings.
    /// Avoid recursion because of its exponential complexity.
    /// Instead, try multiple paths at once and track them with an IndexList.
    /// </summary>
    public partial class UnicodeSetStringSpan
    {
        // ICU4N TODO: API - make [Flags] enum ?
        /*
         * Which span() variant will be used? The object is either built for one variant and used once,
         * or built for all and may be used many times.
         */
        public const int WithCount = 0x40;  // spanAndCount() may be called
        public const int Forward = 0x20;
        public const int Backward = 0x10;
        // public const int Utf16      = 8;
        public const int Contained = 2;
        public const int NotContained = 1;

        public const int All = 0x7f;

        public const int ForwardUtf16Contained = Forward | /* Utf16 | */    Contained;
        public const int ForwardUtf16NotContained = Forward | /* Utf16 | */NotContained;
        public const int BackwardUtf16Contained = Backward | /* Utf16 | */    Contained;
        public const int BackwardUtf16NotContained = Backward | /* Utf16 | */NotContained;

        /**
         * Special spanLength short values. (since .NET has unsigned byte type, we don't need to use short like in Java)
         * All code points in the string are contained in the parent set.
         */
        internal const byte ALL_CP_CONTAINED = 0xff;
        /// <summary>The spanLength is >=0xfe.</summary>
        internal const byte LONG_SPAN = (ALL_CP_CONTAINED - 1);

        /// <summary>Set for <see cref="Span(string, int, SpanCondition)"/>. Same as parent but without strings.</summary>
        private UnicodeSet spanSet;

        /// <summary>
        /// Set for Span(not contained).
        /// Same as <see cref="spanSet"/>, plus characters that start or end strings.
        /// </summary>
        private UnicodeSet spanNotSet;

        /// <summary>The strings of the parent set.</summary>
        private IList<string> strings;

        /// <summary>The lengths of <see cref="Span(string, int, SpanCondition)"/>, 
        /// <see cref="SpanBack(string, int, SpanCondition)"/> etc. for each string.</summary>
        private short[] spanLengths;

        /// <summary>Maximum lengths of relevant strings.</summary>
        private readonly int maxLength16;

        /// <summary>Are there strings that are not fully contained in the code point set?</summary>
        private bool someRelevant;

        /// <summary>Set up for all variants of <see cref="SpanBack(string, int, SpanCondition)"/>?</summary>
        private bool all;

        /// <summary>Span helper</summary>
        private OffsetList offsets;

        /// <summary>
        /// Constructs for all variants of <see cref="Span(string, int, SpanCondition)"/>, or only for any one variant.
        /// Initializes as little as possible, for single use.
        /// </summary>
        public UnicodeSetStringSpan(UnicodeSet set, IList<string> setStrings, int which)
        {
            spanSet = new UnicodeSet(0, 0x10ffff);
            // TODO: With Java 6, just take the parent set's strings as is,
            // as a NavigableSet<String>, rather than as an ArrayList copy of the set of strings.
            // Then iterate via the first() and higher() methods.
            // (We do not want to create multiple Iterator objects in each span().)
            // See ICU ticket #7454.
            strings = setStrings;
            all = (which == All);
            spanSet.RetainAll(set);
            if (0 != (which & NotContained))
            {
                // Default to the same sets.
                // addToSpanNotSet() will create a separate set if necessary.
                spanNotSet = spanSet;
            }
            offsets = new OffsetList();

            // Determine if the strings even need to be taken into account at all for span() etc.
            // If any string is relevant, then all strings need to be used for
            // span(longest match) but only the relevant ones for span(while contained).
            // TODO: Possible optimization: Distinguish CONTAINED vs. LONGEST_MATCH
            // and do not store UTF-8 strings if !thisRelevant and CONTAINED.
            // (Only store irrelevant UTF-8 strings for LONGEST_MATCH where they are relevant after all.)
            // Also count the lengths of the UTF-8 versions of the strings for memory allocation.
            int stringsLength = strings.Count;

            int i, spanLength;
            int maxLength16 = 0;
            someRelevant = false;
            for (i = 0; i < stringsLength; ++i)
            {
                string str = strings[i];
                int length16 = str.Length;
                spanLength = spanSet.Span(str, SpanCondition.Contained);
                if (spanLength < length16)
                { // Relevant string.
                    someRelevant = true;
                }
                if (/* (0 != (which & UTF16)) && */ length16 > maxLength16)
                {
                    maxLength16 = length16;
                }
            }
            this.maxLength16 = maxLength16;
            if (!someRelevant && (which & WithCount) == 0)
            {
                return;
            }

            // Freeze after checking for the need to use strings at all because freezing
            // a set takes some time and memory which are wasted if there are no relevant strings.
            if (all)
            {
                spanSet.Freeze();
            }

            int spanBackLengthsOffset;

            // Allocate a block of meta data.
            int allocSize;
            if (all)
            {
                // 2 sets of span lengths
                allocSize = stringsLength * (2);
            }
            else
            {
                allocSize = stringsLength; // One set of span lengths.
            }
            spanLengths = new short[allocSize];

            if (all)
            {
                // Store span lengths for all span() variants.
                spanBackLengthsOffset = stringsLength;
            }
            else
            {
                // Store span lengths for only one span() variant.
                spanBackLengthsOffset = 0;
            }

            // Set the meta data and spanNotSet and write the UTF-8 strings.

            for (i = 0; i < stringsLength; ++i)
            {
                string str = strings[i];
                int length16 = str.Length;
                spanLength = spanSet.Span(str, SpanCondition.Contained);
                if (spanLength < length16)
                { // Relevant string.
                    if (true /* 0 != (which & UTF16) */)
                    {
                        if (0 != (which & Contained))
                        {
                            if (0 != (which & Forward))
                            {
                                spanLengths[i] = MakeSpanLengthByte(spanLength);
                            }
                            if (0 != (which & Backward))
                            {
                                spanLength = length16
                                        - spanSet.SpanBack(str, length16, SpanCondition.Contained);
                                spanLengths[spanBackLengthsOffset + i] = MakeSpanLengthByte(spanLength);
                            }
                        }
                        else /* not CONTAINED, not all, but NOT_CONTAINED */
                        {
                            spanLengths[i] = spanLengths[spanBackLengthsOffset + i] = 0; // Only store a relevant/irrelevant
                                                                                         // flag.
                        }
                    }
                    if (0 != (which & NotContained))
                    {
                        // Add string start and end code points to the spanNotSet so that
                        // a span(while not contained) stops before any string.
                        int c;
                        if (0 != (which & Forward))
                        {
                            c = str.CodePointAt(0);
                            AddToSpanNotSet(c);
                        }
                        if (0 != (which & Backward))
                        {
                            c = str.CodePointBefore(length16);
                            AddToSpanNotSet(c);
                        }
                    }
                }
                else
                { // Irrelevant string.
                    if (all)
                    {
                        spanLengths[i] = spanLengths[spanBackLengthsOffset + i] = ALL_CP_CONTAINED;
                    }
                    else
                    {
                        // All spanXYZLengths pointers contain the same address.
                        spanLengths[i] = ALL_CP_CONTAINED;
                    }
                }
            }

            // Finish.
            if (all)
            {
                spanNotSet.Freeze();
            }
        }

        /// <summary>
        /// Constructs a copy of an existing UnicodeSetStringSpan.
        /// Assumes which==<see cref="All"/> for a frozen set.
        /// </summary>
        public UnicodeSetStringSpan(UnicodeSetStringSpan otherStringSpan,
                IList<string> newParentSetStrings)
        {
            spanSet = otherStringSpan.spanSet;
            strings = newParentSetStrings;
            maxLength16 = otherStringSpan.maxLength16;
            someRelevant = otherStringSpan.someRelevant;
            all = true;
            if (Utility.SameObjects(otherStringSpan.spanNotSet, otherStringSpan.spanSet))
            {
                spanNotSet = spanSet;
            }
            else
            {
                spanNotSet = (UnicodeSet)otherStringSpan.spanNotSet.Clone();
            }
            offsets = new OffsetList();

            spanLengths = (short[])otherStringSpan.spanLengths.Clone();
        }

        /// <summary>
        /// Do the strings need to be checked in <see cref="Span(string, int, SpanCondition)"/> etc.?
        /// Returns true if strings need to be checked (call <see cref="Span(string, int, SpanCondition)"/> here),
        /// false if not (use a BMPSet for best performance).
        /// </summary>
        public bool NeedsStringSpanUTF16
        {
            get { return someRelevant; }
        }

        /// <summary>For fast <see cref="UnicodeSet.Contains(int)"/>.</summary>
        public bool Contains(int c)
        {
            return spanSet.Contains(c);
        }

        /// <summary>
        /// Adds a starting or ending string character to the <see cref="spanNotSet"/>
        /// so that a character span ends before any string.
        /// </summary>
        private void AddToSpanNotSet(int c)
        {
            if (Utility.SameObjects(spanNotSet, null) || Utility.SameObjects(spanNotSet, spanSet))
            {
                if (spanSet.Contains(c))
                {
                    return; // Nothing to do.
                }
                spanNotSet = spanSet.CloneAsThawed();
            }
            spanNotSet.Add(c);
        }

        /*
         * Note: In span() when spanLength==0
         * (after a string match, or at the beginning after an empty code point span)
         * and in spanNot() and spanNotUTF8(),
         * string matching could use a binary search because all string matches are done
         * from the same start index.
         *
         * For UTF-8, this would require a comparison function that returns UTF-16 order.
         *
         * This optimization should not be necessary for normal UnicodeSets because most sets have no strings, and most sets
         * with strings have very few very short strings. For cases with many strings, it might be better to use a different
         * API and implementation with a DFA (state machine).
         */

        /*
         * Algorithm for span(SpanCondition.CONTAINED)
         *
         * Theoretical algorithm:
         * - Iterate through the string, and at each code point boundary:
         *   + If the code point there is in the set, then remember to continue after it.
         *   + If a set string matches at the current position, then remember to continue after it.
         *   + Either recursively span for each code point or string match, or recursively span
         *     for all but the shortest one and iteratively continue the span with the shortest local match.
         *   + Remember the longest recursive span (the farthest end point).
         *   + If there is no match at the current position,
         *     neither for the code point there nor for any set string,
         *     then stop and return the longest recursive span length.
         *
         * Optimized implementation:
         *
         * (We assume that most sets will have very few very short strings.
         * A span using a string-less set is extremely fast.)
         *
         * Create and cache a spanSet which contains all of the single code points of the original set
         * but none of its strings.
         *
         * - Start with spanLength=spanSet.span(SpanCondition.CONTAINED).
         * - Loop:
         *   + Try to match each set string at the end of the spanLength.
         *     ~ Set strings that start with set-contained code points
         *       must be matched with a partial overlap
         *       because the recursive algorithm would have tried to match them at every position.
         *     ~ Set strings that entirely consist of set-contained code points
         *       are irrelevant for span(SpanCondition.CONTAINED)
         *       because the recursive algorithm would continue after them anyway and
         *       find the longest recursive match from their end.
         *     ~ Rather than recursing, note each end point of a set string match.
         *   + If no set string matched after spanSet.span(),
         *     then return with where the spanSet.span() ended.
         *   + If at least one set string matched after spanSet.span(),
         *     then pop the shortest string match end point and continue the loop,
         *     trying to match all set strings from there.
         *   + If at least one more set string matched after a previous string match, then test if the
         *     code point after the previous string match is also contained in the set.
         *     Continue the loop with the shortest end point of
         *     either this code point or a matching set string.
         *   + If no more set string matched after a previous string match,
         *     then try another spanLength=spanSet.span(SpanCondition.CONTAINED).
         *     Stop if spanLength==0, otherwise continue the loop.
         *
         * By noting each end point of a set string match, the function visits each string position at most once and
         * finishes in linear time.
         *
         * The recursive algorithm may visit the same string position many times
         * if multiple paths lead to it and finishes in exponential time.
         */

        /*
         * Algorithm for span(SIMPLE)
         *
         * Theoretical algorithm:
         * - Iterate through the string, and at each code point boundary:
         *   + If the code point there is in the set, then remember to continue after it.
         *   + If a set string matches at the current position, then remember to continue after it.
         *   + Continue from the farthest match position and ignore all others.
         *   + If there is no match at the current position, then stop and return the current position.
         *
         * Optimized implementation:
         *
         * (Same assumption and spanSet as above.)
         *
         * - Start with spanLength=spanSet.span(SpanCondition.CONTAINED).
         * - Loop:
         *   + Try to match each set string at the end of the spanLength.
         *     ~ Set strings that start with set-contained code points
         *       must be matched with a partial overlap
         *       because the standard algorithm would have tried to match them earlier.
         *     ~ Set strings that entirely consist of set-contained code points
         *       must be matched with a full overlap because the longest-match algorithm
         *       would hide set string matches that end earlier.
         *       Such set strings need not be matched earlier inside the code point span
         *       because the standard algorithm would then have
         *       continued after the set string match anyway.
         *     ~ Remember the longest set string match (farthest end point)
         *       from the earliest starting point.
         *   + If no set string matched after spanSet.span(),
         *     then return with where the spanSet.span() ended.
         *   + If at least one set string matched,
         *     then continue the loop after the longest match from the earliest position.
         *   + If no more set string matched after a previous string match,
         *     then try another spanLength=spanSet.span(SpanCondition.CONTAINED).
         *     Stop if spanLength==0, otherwise continue the loop.
         */

        // ICU4N specific - Span(ICharSequence s, int start, SpanCondition spanCondition) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanWithStrings(ICharSequence s, int start, int spanLimit, 
        //    SpanCondition spanCondition) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanAndCount(ICharSequence s, int start, SpanCondition spanCondition,
        //    out int outCount) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanContainedAndCount(ICharSequence s, int start, out int outCount) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanBack(ICharSequence s, int length, SpanCondition spanCondition) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanNot(ICharSequence s, int start, bool includeCount, out int outCount) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanNotBack(ICharSequence s, int length) moved to UnicodeSetStringSpanExtension.tt



        internal static byte MakeSpanLengthByte(int spanLength) // ICU4N specific - changed return type to byte (since we have unsigned byte in .NET)
        {
            // 0xfe==UnicodeSetStringSpan::LONG_SPAN
            return spanLength < LONG_SPAN ? (byte)spanLength : LONG_SPAN;
        }

        // ICU4N specific - Matches16(ICharSequence s, int start, string t, int length) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - Matches16CPB(ICharSequence s, int start, int limit, string t, int tlength) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanOne(UnicodeSet set, ICharSequence s, int start, int length) moved to UnicodeSetStringSpanExtension.tt

        // ICU4N specific - SpanOneBack(UnicodeSet set, ICharSequence s, int length) moved to UnicodeSetStringSpanExtension.tt


        /// <summary>
        /// Helper class for <see cref="UnicodeSetStringSpan"/>.
        /// </summary>
        /// <remarks>
        /// List of offsets from the current position from where to try matching
        /// a code point or a string.
        /// Stores offsets rather than indexes to simplify the code and use the same list
        /// for both increments (in <see cref="Span(string, int, SpanCondition)"/>) and decrements (in <see cref="SpanBack(string, int, SpanCondition)"/>).
        /// 
        /// <para/>Assumption: The maximum offset is limited, and the offsets that are stored at any one time
        /// are relatively dense, that is,
        /// there are normally no gaps of hundreds or thousands of offset values.
        /// 
        /// <para/>This class optionally also tracks the minimum non-negative count for each position,
        /// intended to count the smallest number of elements of any path leading to that position.
        /// 
        /// <para/>The implementation uses a circular buffer of count integers,
        /// each indicating whether the corresponding offset is in the list,
        /// and its path element count.
        /// This avoids inserting into a sorted list of offsets (or absolute indexes)
        /// and physically moving part of the list.
        /// 
        /// <para/>Note: In principle, the caller should <see cref="SetMaxLength(int)"/> to
        /// the maximum of the max string length and U16_LENGTH/U8_LENGTH
        /// to account for "long" single code points.
        /// 
        /// <para/>Note: An earlier version did not track counts and stored only byte flags.
        /// With boolean flags, if maxLength were guaranteed to be no more than 32 or 64,
        /// the list could be stored as bit flags in a single integer.
        /// Rather than handling a circular buffer with a start list index,
        /// the integer would simply be shifted when lower offsets are removed.
        /// UnicodeSet does not have a limit on the lengths of strings.
        /// </remarks>
        private sealed class OffsetList
        {
            private int[] list;
            private int length;
            private int start;

            public OffsetList()
            {
                list = new int[16];  // default size
            }

            public void SetMaxLength(int maxLength)
            {
                if (maxLength > list.Length)
                {
                    list = new int[maxLength];
                }
                Clear();
            }

            public void Clear()
            {
                for (int i = list.Length; i-- > 0;)
                {
                    list[i] = 0;
                }
                start = length = 0;
            }

            public bool IsEmpty
            {
                get { return (length == 0); }
            }

            /// <summary>
            /// Reduces all stored offsets by delta, used when the current position moves by delta.
            /// There must not be any offsets lower than delta.
            /// If there is an offset equal to delta, it is removed.
            /// </summary>
            /// <param name="delta">[1..maxLength]</param>
            public void Shift(int delta)
            {
                int i = start + delta;
                if (i >= list.Length)
                {
                    i -= list.Length;
                }
                if (list[i] != 0)
                {
                    list[i] = 0;
                    --length;
                }
                start = i;
            }

            /// <summary>
            /// Adds an offset. The list must not contain it yet.
            /// </summary>
            /// <param name="offset">[1..maxLength]</param>
            public void AddOffset(int offset)
            {
                int i = start + offset;
                if (i >= list.Length)
                {
                    i -= list.Length;
                }
                Debug.Assert(list[i] == 0);
                list[i] = 1;
                ++length;
            }

            /// <summary>
            /// Adds an offset and updates its count.
            /// The list may already contain the offset.
            /// </summary>
            /// <param name="offset">[1..maxLength]</param>
            /// <param name="count"></param>
            public void AddOffsetAndCount(int offset, int count)
            {
                Debug.Assert(count > 0);
                int i = start + offset;
                if (i >= list.Length)
                {
                    i -= list.Length;
                }
                if (list[i] == 0)
                {
                    list[i] = count;
                    ++length;
                }
                else if (count < list[i])
                {
                    list[i] = count;
                }
            }

            /// <param name="offset">[1..maxLength]</param>
            public bool ContainsOffset(int offset)
            {
                int i = start + offset;
                if (i >= list.Length)
                {
                    i -= list.Length;
                }
                return list[i] != 0;
            }

            /// <param name="offset">[1..maxLength]</param>
            /// <param name="count"></param>
            public bool HasCountAtOffset(int offset, int count)
            {
                int i = start + offset;
                if (i >= list.Length)
                {
                    i -= list.Length;
                }
                int oldCount = list[i];
                return oldCount != 0 && oldCount <= count;
            }

            /// <summary>
            /// Finds the lowest stored offset from a non-empty list, removes it,
            /// and reduces all other offsets by this minimum.
            /// </summary>
            /// <param name="outCount"></param>
            /// <returns>min=[1..maxLength]</returns>
            public int PopMinimum(out int outCount)
            {
                // Look for the next offset in list[start+1..list.length-1].
                int i = start, result;
                while (++i < list.Length)
                {
                    int count2 = list[i];
                    if (count2 != 0)
                    {
                        list[i] = 0;
                        --length;
                        result = i - start;
                        start = i;
                        outCount = count2;
                        return result;
                    }
                }
                // i==list.length

                // Wrap around and look for the next offset in list[0..start].
                // Since the list is not empty, there will be one.
                result = list.Length - start;
                i = 0;
                int count;
                while ((count = list[i]) == 0)
                {
                    ++i;
                }
                list[i] = 0;
                --length;
                start = i;
                outCount = count;
                return result + i;
            }
        }
    }
}
