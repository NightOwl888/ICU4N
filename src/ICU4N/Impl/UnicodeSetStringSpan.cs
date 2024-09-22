using ICU4N.Text;
using J2N;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        private readonly object syncLock = new object();

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
        public bool NeedsStringSpanUTF16 => someRelevant;

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

        /// <summary>
        /// Spans a string.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        public int Span(string s, int start, SpanCondition spanCondition)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return Span(s.AsSpan(), start, spanCondition);
        }

        /// <summary>
        /// Spans a string.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        public int Span(ReadOnlySpan<char> s, int start, SpanCondition spanCondition)
        {
            if (spanCondition == SpanCondition.NotContained)
            {
                return SpanNot(s, start);
            }
            int spanLimit = spanSet.Span(s, start, SpanCondition.Contained);
            if (spanLimit == s.Length)
            {
                return spanLimit;
            }
            return SpanWithStrings(s, start, spanLimit, spanCondition);
        }

        /// <summary>
        /// Synchronized method for complicated spans using the offsets.
        /// Avoids synchronization for simple cases.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="spanLimit">= SpanSet.Span(s, start, Contained)</param>
        /// <param name="spanCondition"></param>
        /// <returns></returns>
        private int SpanWithStrings(ReadOnlySpan<char> s, int start, int spanLimit,
            SpanCondition spanCondition)
        {
            lock (syncLock)
            {
                // Consider strings; they may overlap with the span.
                int initSize = 0;
                if (spanCondition == SpanCondition.Contained)
                {
                    // Use offset list to try all possibilities.
                    initSize = maxLength16;
                }
                offsets.SetMaxLength(initSize);
                int length = s.Length;
                int pos = spanLimit, rest = length - spanLimit;
                int spanLength = spanLimit - start;
                int i, stringsLength = strings.Count;
                for (; ; )
                {
                    if (spanCondition == SpanCondition.Contained)
                    {
                        for (i = 0; i < stringsLength; ++i)
                        {
                            int overlap = spanLengths[i];
                            if (overlap == ALL_CP_CONTAINED)
                            {
                                continue; // Irrelevant string.
                            }
                            string str = strings[i];

                            int length16 = str.Length;

                            // Try to match this string at pos-overlap..pos.
                            if (overlap >= LONG_SPAN)
                            {
                                overlap = length16;
                                // While contained: No point matching fully inside the code point span.
                                overlap = str.OffsetByCodePoints(overlap, -1); // Length of the string minus the last code
                                                                               // point.
                            }
                            if (overlap > spanLength)
                            {
                                overlap = spanLength;
                            }
                            int inc = length16 - overlap; // Keep overlap+inc==length16.
                            for (; ; )
                            {
                                if (inc > rest)
                                {
                                    break;
                                }
                                // Try to match if the increment is not listed already.
                                if (!offsets.ContainsOffset(inc) && Matches16CPB(s, pos - overlap, length, str, length16))
                                {
                                    if (inc == rest)
                                    {
                                        return length; // Reached the end of the string.
                                    }
                                    offsets.AddOffset(inc);
                                }
                                if (overlap == 0)
                                {
                                    break;
                                }
                                --overlap;
                                ++inc;
                            }
                        }
                    }
                    else /* SIMPLE */
                    {
                        int maxInc = 0, maxOverlap = 0;
                        for (i = 0; i < stringsLength; ++i)
                        {
                            int overlap = spanLengths[i];
                            // For longest match, we do need to try to match even an all-contained string
                            // to find the match from the earliest start.

                            string str = strings[i];

                            int length16 = str.Length;

                            // Try to match this string at pos-overlap..pos.
                            if (overlap >= LONG_SPAN)
                            {
                                overlap = length16;
                                // Longest match: Need to match fully inside the code point span
                                // to find the match from the earliest start.
                            }
                            if (overlap > spanLength)
                            {
                                overlap = spanLength;
                            }
                            int inc = length16 - overlap; // Keep overlap+inc==length16.
                            for (; ; )
                            {
                                if (inc > rest || overlap < maxOverlap)
                                {
                                    break;
                                }
                                // Try to match if the string is longer or starts earlier.
                                if ((overlap > maxOverlap || /* redundant overlap==maxOverlap && */inc > maxInc)
                                        && Matches16CPB(s, pos - overlap, length, str, length16))
                                {
                                    maxInc = inc; // Longest match from earliest start.
                                    maxOverlap = overlap;
                                    break;
                                }
                                --overlap;
                                ++inc;
                            }
                        }

                        if (maxInc != 0 || maxOverlap != 0)
                        {
                            // Longest-match algorithm, and there was a string match.
                            // Simply continue after it.
                            pos += maxInc;
                            rest -= maxInc;
                            if (rest == 0)
                            {
                                return length; // Reached the end of the string.
                            }
                            spanLength = 0; // Match strings from after a string match.
                            continue;
                        }
                    }
                    // Finished trying to match all strings at pos.

                    if (spanLength != 0 || pos == 0)
                    {
                        // The position is after an unlimited code point span (spanLength!=0),
                        // not after a string match.
                        // The only position where spanLength==0 after a span is pos==0.
                        // Otherwise, an unlimited code point span is only tried again when no
                        // strings match, and if such a non-initial span fails we stop.
                        if (offsets.IsEmpty)
                        {
                            return pos; // No strings matched after a span.
                        }
                        // Match strings from after the next string match.
                    }
                    else
                    {
                        // The position is after a string match (or a single code point).
                        if (offsets.IsEmpty)
                        {
                            // No more strings matched after a previous string match.
                            // Try another code point span from after the last string match.
                            spanLimit = spanSet.Span(s, pos, SpanCondition.Contained);
                            spanLength = spanLimit - pos;
                            if (spanLength == rest || // Reached the end of the string, or
                                    spanLength == 0 // neither strings nor span progressed.
                            )
                            {
                                return spanLimit;
                            }
                            pos += spanLength;
                            rest -= spanLength;
                            continue; // spanLength>0: Match strings from after a span.
                        }
                        else
                        {
                            // Try to match only one code point from after a string match if some
                            // string matched beyond it, so that we try all possible positions
                            // and don't overshoot.
                            spanLength = SpanOne(spanSet, s, pos, rest);
                            if (spanLength > 0)
                            {
                                if (spanLength == rest)
                                {
                                    return length; // Reached the end of the string.
                                }
                                // Match strings after this code point.
                                // There cannot be any increments below it because UnicodeSet strings
                                // contain multiple code points.
                                pos += spanLength;
                                rest -= spanLength;
                                offsets.Shift(spanLength);
                                spanLength = 0;
                                continue; // Match strings from after a single code point.
                            }
                            // Match strings from after the next string match.
                        }
                    }
                    int ignoredOutCount;
                    int minOffset = offsets.PopMinimum(out ignoredOutCount);
                    pos += minOffset;
                    rest -= minOffset;
                    spanLength = 0; // Match strings from after a string match.
                }
            }
        }

        /// <summary>
        /// Spans a string and counts the smallest number of set elements on any path across the span.
        /// </summary>
        /// <remarks>
        /// For proper counting, we cannot ignore strings that are fully contained in code point spans.
        /// <para/>
        /// If the set does not have any fully-contained strings, then we could optimize this
        /// like Span(), but such sets are likely rare, and this is at least still linear.
        /// </remarks>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <param name="outCount">The count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        public int SpanAndCount(string s, int start, SpanCondition spanCondition,
            out int outCount) // ICU4N TODO: API - Would this be more useful if we returned length instead of limit?
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return SpanAndCount(s.AsSpan(), start, spanCondition, out outCount);
        }

        /// <summary>
        /// Spans a string and counts the smallest number of set elements on any path across the span.
        /// </summary>
        /// <remarks>
        /// For proper counting, we cannot ignore strings that are fully contained in code point spans.
        /// <para/>
        /// If the set does not have any fully-contained strings, then we could optimize this
        /// like Span(), but such sets are likely rare, and this is at least still linear.
        /// </remarks>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <param name="outCount">The count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        public int SpanAndCount(ReadOnlySpan<char> s, int start, SpanCondition spanCondition,
            out int outCount) // ICU4N TODO: API - Would this be more useful if we returned length instead of limit?
        {
            if (spanCondition == SpanCondition.NotContained)
            {
                return SpanNot(s, start, out outCount);
            }
            // Consider strings; they may overlap with the span,
            // and they may result in a smaller count that with just code points.
            if (spanCondition == SpanCondition.Contained)
            {
                return SpanContainedAndCount(s, start, out outCount);
            }
            // SIMPLE (not synchronized, does not use offsets)
            int stringsLength = strings.Count;
            int length = s.Length;
            int pos = start;
            int rest = length - start;
            int count = 0;
            while (rest != 0)
            {
                // Try to match the next code point.
                int cpLength = SpanOne(spanSet, s, pos, rest);
                int maxInc = (cpLength > 0) ? cpLength : 0;
                // Try to match all of the strings.
                for (int i = 0; i < stringsLength; ++i)
                {
                    string str = strings[i];
                    int length16 = str.Length;
                    if (maxInc < length16 && length16 <= rest &&
                            Matches16CPB(s, pos, length, str, length16))
                    {
                        maxInc = length16;
                    }
                }
                // We are done if there is no match beyond pos.
                if (maxInc == 0)
                {
                    outCount = count;
                    return pos;
                }
                // Continue from the longest match.
                ++count;
                pos += maxInc;
                rest -= maxInc;
            }
            outCount = count;
            return pos;
        }

        private int SpanContainedAndCount(ReadOnlySpan<char> s, int start, out int outCount)
        {
            lock (syncLock)
            {
                // Use offset list to try all possibilities.
                offsets.SetMaxLength(maxLength16);
                int stringsLength = strings.Count;
                int length = s.Length;
                int pos = start;
                int rest = length - start;
                int count = 0;
                while (rest != 0)
                {
                    // Try to match the next code point.
                    int cpLength = SpanOne(spanSet, s, pos, rest);
                    if (cpLength > 0)
                    {
                        offsets.AddOffsetAndCount(cpLength, count + 1);
                    }
                    // Try to match all of the strings.
                    for (int i = 0; i < stringsLength; ++i)
                    {
                        string str = strings[i];
                        int length16 = str.Length;
                        // Note: If the strings were sorted by length, then we could also
                        // avoid trying to match if there is already a match of the same length.
                        if (length16 <= rest && !offsets.HasCountAtOffset(length16, count + 1) &&
                                Matches16CPB(s, pos, length, str, length16))
                        {
                            offsets.AddOffsetAndCount(length16, count + 1);
                        }
                    }
                    // We are done if there is no match beyond pos.
                    if (offsets.IsEmpty)
                    {
                        outCount = count;
                        return pos;
                    }
                    // Continue from the nearest match.
                    int minOffset = offsets.PopMinimum(out outCount);
                    count = outCount;
                    pos += minOffset;
                    rest -= minOffset;
                }
                outCount = count;
                return pos;
            }
        }

        /// <summary>
        /// Span a string backwards.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="length"></param>
        /// <param name="spanCondition">The span condition</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        public int SpanBack(string s, int length, SpanCondition spanCondition)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return SpanBack(s.AsSpan(), length, spanCondition);
        }

        /// <summary>
        /// Span a string backwards.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="length"></param>
        /// <param name="spanCondition">The span condition</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        public int SpanBack(ReadOnlySpan<char> s, int length, SpanCondition spanCondition)
        {
            lock (syncLock)
            {
                if (spanCondition == SpanCondition.NotContained)
                {
                    return SpanNotBack(s, length);
                }
                int pos = spanSet.SpanBack(s, length, SpanCondition.Contained);
                if (pos == 0)
                {
                    return 0;
                }
                int spanLength = length - pos;

                // Consider strings; they may overlap with the span.
                int initSize = 0;
                if (spanCondition == SpanCondition.Contained)
                {
                    // Use offset list to try all possibilities.
                    initSize = maxLength16;
                }
                offsets.SetMaxLength(initSize);
                int i, stringsLength = strings.Count;
                int spanBackLengthsOffset = 0;
                if (all)
                {
                    spanBackLengthsOffset = stringsLength;
                }
                for (; ; )
                {
                    if (spanCondition == SpanCondition.Contained)
                    {
                        for (i = 0; i < stringsLength; ++i)
                        {
                            int overlap = spanLengths[spanBackLengthsOffset + i];
                            if (overlap == ALL_CP_CONTAINED)
                            {
                                continue; // Irrelevant string.
                            }
                            string str = strings[i];

                            int length16 = str.Length;

                            // Try to match this string at pos-(length16-overlap)..pos-length16.
                            if (overlap >= LONG_SPAN)
                            {
                                overlap = length16;
                                // While contained: No point matching fully inside the code point span.
                                int len1 = 0;
                                len1 = str.OffsetByCodePoints(0, 1);
                                overlap -= len1; // Length of the string minus the first code point.
                            }
                            if (overlap > spanLength)
                            {
                                overlap = spanLength;
                            }
                            int dec = length16 - overlap; // Keep dec+overlap==length16.
                            for (; ; )
                            {
                                if (dec > pos)
                                {
                                    break;
                                }
                                // Try to match if the decrement is not listed already.
                                if (!offsets.ContainsOffset(dec) && Matches16CPB(s, pos - dec, length, str, length16))
                                {
                                    if (dec == pos)
                                    {
                                        return 0; // Reached the start of the string.
                                    }
                                    offsets.AddOffset(dec);
                                }
                                if (overlap == 0)
                                {
                                    break;
                                }
                                --overlap;
                                ++dec;
                            }
                        }
                    }
                    else /* SIMPLE */
                    {
                        int maxDec = 0, maxOverlap = 0;
                        for (i = 0; i < stringsLength; ++i)
                        {
                            int overlap = spanLengths[spanBackLengthsOffset + i];
                            // For longest match, we do need to try to match even an all-contained string
                            // to find the match from the latest end.

                            string str = strings[i];

                            int length16 = str.Length;

                            // Try to match this string at pos-(length16-overlap)..pos-length16.
                            if (overlap >= LONG_SPAN)
                            {
                                overlap = length16;
                                // Longest match: Need to match fully inside the code point span
                                // to find the match from the latest end.
                            }
                            if (overlap > spanLength)
                            {
                                overlap = spanLength;
                            }
                            int dec = length16 - overlap; // Keep dec+overlap==length16.
                            for (; ; )
                            {
                                if (dec > pos || overlap < maxOverlap)
                                {
                                    break;
                                }
                                // Try to match if the string is longer or ends later.
                                if ((overlap > maxOverlap || /* redundant overlap==maxOverlap && */dec > maxDec)
                                        && Matches16CPB(s, pos - dec, length, str, length16))
                                {
                                    maxDec = dec; // Longest match from latest end.
                                    maxOverlap = overlap;
                                    break;
                                }
                                --overlap;
                                ++dec;
                            }
                        }

                        if (maxDec != 0 || maxOverlap != 0)
                        {
                            // Longest-match algorithm, and there was a string match.
                            // Simply continue before it.
                            pos -= maxDec;
                            if (pos == 0)
                            {
                                return 0; // Reached the start of the string.
                            }
                            spanLength = 0; // Match strings from before a string match.
                            continue;
                        }
                    }
                    // Finished trying to match all strings at pos.

                    if (spanLength != 0 || pos == length)
                    {
                        // The position is before an unlimited code point span (spanLength!=0),
                        // not before a string match.
                        // The only position where spanLength==0 before a span is pos==length.
                        // Otherwise, an unlimited code point span is only tried again when no
                        // strings match, and if such a non-initial span fails we stop.
                        if (offsets.IsEmpty)
                        {
                            return pos; // No strings matched before a span.
                        }
                        // Match strings from before the next string match.
                    }
                    else
                    {
                        // The position is before a string match (or a single code point).
                        if (offsets.IsEmpty)
                        {
                            // No more strings matched before a previous string match.
                            // Try another code point span from before the last string match.
                            int oldPos = pos;
                            pos = spanSet.SpanBack(s, oldPos, SpanCondition.Contained);
                            spanLength = oldPos - pos;
                            if (pos == 0 || // Reached the start of the string, or
                                    spanLength == 0 // neither strings nor span progressed.
                            )
                            {
                                return pos;
                            }
                            continue; // spanLength>0: Match strings from before a span.
                        }
                        else
                        {
                            // Try to match only one code point from before a string match if some
                            // string matched beyond it, so that we try all possible positions
                            // and don't overshoot.
                            spanLength = SpanOneBack(spanSet, s, pos);
                            if (spanLength > 0)
                            {
                                if (spanLength == pos)
                                {
                                    return 0; // Reached the start of the string.
                                }
                                // Match strings before this code point.
                                // There cannot be any decrements below it because UnicodeSet strings
                                // contain multiple code points.
                                pos -= spanLength;
                                offsets.Shift(spanLength);
                                spanLength = 0;
                                continue; // Match strings from before a single code point.
                            }
                            // Match strings from before the next string match.
                        }
                    }
                    int ignoredOutCount;
                    pos -= offsets.PopMinimum(out ignoredOutCount);
                    spanLength = 0; // Match strings from before a string match.
                }
            }
        }


        // ICU4N specific wrapper method to call SpanNot with no
        // output count.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SpanNot(ReadOnlySpan<char> s, int start)
        {
            return SpanNot(s, start, false, out _);
        }

        // ICU4N specific wrapper method to call SpanNot with an
        // output count.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SpanNot(ReadOnlySpan<char> s, int start, out int outCount)
        {
            return SpanNot(s, start, true, out outCount);
        }

        /// <summary>
        /// Algorithm for <c>SpanNot()==Span(SpanCondition.Contained)</c>
        /// </summary>
        /// <remarks>
        /// Theoretical algorithm:
        /// - Iterate through the string, and at each code point boundary:
        ///   + If the code point there is in the set, then return with the current position.
        ///   + If a set string matches at the current position, then return with the current position.
        /// <para/>
        /// Optimized implementation:
        /// <para/>
        /// (Same assumption as for Span() above.)
        /// <para/>
        /// Create and cache a spanNotSet which contains
        /// all of the single code points of the original set but none of its strings.
        /// For each set string add its initial code point to the spanNotSet.
        /// (Also add its final code point for <see cref="SpanNotBack(ReadOnlySpan{char}, int)"/>.)
        /// <para/>
        /// - Loop:
        ///   + Do spanLength=spanNotSet.Span(SpanCondition.Contained).
        ///   + If the current code point is in the original set, then return the current position.
        ///   + If any set string matches at the current position, then return the current position.
        ///   + If there is no match at the current position, neither for the code point
        ///     there nor for any set string, then skip this code point and continue the loop.
        ///     This happens for set-string-initial code points that were added to spanNotSet
        ///     when there is not actually a match for such a set string.
        /// </remarks>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="includeCount">If true, the number of code points across the span are 
        /// retrieved and returnd through <paramref name="outCount"/>.</param>
        /// <param name="outCount">Receives the number of code points across the span.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        // ICU4N specific - added includeCount parameter, made outCount an out parameter (rather than
        // an object)
        private int SpanNot(ReadOnlySpan<char> s, int start, bool includeCount, out int outCount)
        {
            outCount = default(int);
            int length = s.Length;
            int pos = start, rest = length - start;
            int stringsLength = strings.Count;
            int count = 0;
            do
            {
                // Span until we find a code point from the set,
                // or a code point that starts or ends some string.
                int spanLimit;
                if (!includeCount)
                {
                    spanLimit = spanNotSet.Span(s, pos, SpanCondition.NotContained);
                }
                else
                {
#pragma warning disable 612, 618
                    spanLimit = spanNotSet.SpanAndCount(s, pos, SpanCondition.NotContained, out outCount);
#pragma warning restore 612, 618
                    outCount = count = count + outCount;
                }
                if (spanLimit == length)
                {
                    return length; // Reached the end of the string.
                }
                pos = spanLimit;
                rest = length - spanLimit;

                // Check whether the current code point is in the original set,
                // without the string starts and ends.
                int cpLength = SpanOne(spanSet, s, pos, rest);
                if (cpLength > 0)
                {
                    return pos; // There is a set element at pos.
                }

                // Try to match the strings at pos.
                for (int i = 0; i < stringsLength; ++i)
                {
                    if (spanLengths[i] == ALL_CP_CONTAINED)
                    {
                        continue; // Irrelevant string.
                    }
                    string str = strings[i];

                    int length16 = str.Length;
                    if (length16 <= rest && Matches16CPB(s, pos, length, str, length16))
                    {
                        return pos; // There is a set element at pos.
                    }
                }

                // The span(while not contained) ended on a string start/end which is
                // not in the original set. Skip this code point and continue.
                // cpLength<0
                pos -= cpLength;
                rest += cpLength;
                ++count;
            } while (rest != 0);
            if (includeCount)
            {
                outCount = count;
            }
            return length; // Reached the end of the string.
        }

        private int SpanNotBack(ReadOnlySpan<char> s, int length)
        {
            int pos = length;
            int i, stringsLength = strings.Count;
            do
            {
                // Span until we find a code point from the set,
                // or a code point that starts or ends some string.
                pos = spanNotSet.SpanBack(s, pos, SpanCondition.NotContained);
                if (pos == 0)
                {
                    return 0; // Reached the start of the string.
                }

                // Check whether the current code point is in the original set,
                // without the string starts and ends.
                int cpLength = SpanOneBack(spanSet, s, pos);
                if (cpLength > 0)
                {
                    return pos; // There is a set element at pos.
                }

                // Try to match the strings at pos.
                for (i = 0; i < stringsLength; ++i)
                {
                    // Use spanLengths rather than a spanLengths pointer because
                    // it is easier and we only need to know whether the string is irrelevant
                    // which is the same in either array.
                    if (spanLengths[i] == ALL_CP_CONTAINED)
                    {
                        continue; // Irrelevant string.
                    }
                    string str = strings[i];

                    int length16 = str.Length;
                    if (length16 <= pos && Matches16CPB(s, pos - length16, length, str, length16))
                    {
                        return pos; // There is a set element at pos.
                    }
                }

                // The span(while not contained) ended on a string start/end which is
                // not in the original set. Skip this code point and continue.
                // cpLength<0
                pos += cpLength;
            } while (pos != 0);
            return 0; // Reached the start of the string.
        }

        internal static byte MakeSpanLengthByte(int spanLength) // ICU4N specific - changed return type to byte (since we have unsigned byte in .NET)
        {
            // 0xfe==UnicodeSetStringSpan::LONG_SPAN
            return spanLength < LONG_SPAN ? (byte)spanLength : LONG_SPAN;
        }

        // Compare strings without any argument checks. Requires length>0.
        private static bool Matches16(ReadOnlySpan<char> s, int start, string t, int length)
        {
            int end = start + length;
            while (length-- > 0)
            {
                if (s[--end] != t[length])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compare 16-bit Unicode strings (which may be malformed UTF-16)
        /// at code point boundaries.
        /// That is, each edge of a match must not be in the middle of a surrogate pair.
        /// </summary>
        /// <param name="s">The string to match in.</param>
        /// <param name="start">The start index of <paramref name="s"/>.</param>
        /// <param name="limit">The limit of the subsequence of <paramref name="s"/> being spanned.</param>
        /// <param name="t">The substring to be matched in <paramref name="s"/>.</param>
        /// <param name="tlength">The length of <paramref name="t"/>.</param>
        /// <returns></returns>
        internal static bool Matches16CPB(ReadOnlySpan<char> s, int start, int limit, string t, int tlength)
        {
            return Matches16(s, start, t, tlength)
                    && !(0 < start && char.IsHighSurrogate(s[start - 1]) &&
                            char.IsLowSurrogate(s[start]))
                    && !((start + tlength) < limit && char.IsHighSurrogate(s[start + tlength - 1]) &&
                            char.IsLowSurrogate(s[start + tlength]));
        }

        /// <summary>
        /// Does the set contain the next code point?
        /// If so, return its length; otherwise return its negative length.
        /// </summary>
        internal static int SpanOne(UnicodeSet set, ReadOnlySpan<char> s, int start, int length)
        {
            char c = s[start];
            if (c >= 0xd800 && c <= 0xdbff && length >= 2)
            {
                char c2 = s[start + 1];
                if (UTF16.IsTrailSurrogate(c2))
                {
                    int supplementary = Character.ToCodePoint(c, c2);
                    return set.Contains(supplementary) ? 2 : -2;
                }
            }
            return set.Contains(c) ? 1 : -1;
        }

        internal static int SpanOneBack(UnicodeSet set, ReadOnlySpan<char> s, int length)
        {
            char c = s[length - 1];
            if (c >= 0xdc00 && c <= 0xdfff && length >= 2)
            {
                char c2 = s[length - 2];
                if (UTF16.IsLeadSurrogate(c2))
                {
                    int supplementary = Character.ToCodePoint(c2, c);
                    return set.Contains(supplementary) ? 2 : -2;
                }
            }
            return set.Contains(c) ? 1 : -1;
        }

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

            public bool IsEmpty => length == 0;

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
