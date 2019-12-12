using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// General test of UnicodeSet string span.
    /// </summary>
    public class UnicodeSetStringSpanTest : TestFmwk
    {
        // Simple test first, easier to debug.
        [Test]
        public void TestSimpleStringSpan()
        {
            String pattern = "[a{ab}{bc}]";
            String str = "abc";
            UnicodeSet set = new UnicodeSet(pattern);
            set.Complement();
            int pos = set.SpanBack(str, 3, SpanCondition.Simple);
            if (pos != 1)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}).SpanBack({1}) returns the wrong value pos {2} (!= 1)",
                        set.ToString(), str, pos));
            }
            pos = set.Span(str, SpanCondition.Simple);
            if (pos != 3)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}).Span({1}) returns the wrong value pos {2} (!= 3)",
                        set.ToString(), str, pos));
            }
            pos = set.Span(str, 1, SpanCondition.Simple);
            if (pos != 3)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}).Span({1}, 1) returns the wrong value pos {2} (!= 3)",
                        set.ToString(), str, pos));
            }
        }

        // test our slow implementation
        [Test]
        public void TestSimpleStringSpanSlow()
        {
            String pattern = "[a{ab}{bc}]";
            String str = "abc";
            UnicodeSet uset = new UnicodeSet(pattern);
            uset.Complement();
            UnicodeSetWithStrings set = new UnicodeSetWithStrings(uset);

            int length = ContainsSpanBackUTF16(set, str, 3, SpanCondition.Simple);
            if (length != 1)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}) containsSpanBackUTF16({1}) returns the wrong value length {2} (!= 1)",
                        set.ToString(), str, length));
            }
            length = ContainsSpanUTF16(set, str, SpanCondition.Simple);
            if (length != 3)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}) containsSpanUTF16({1}) returns the wrong value length {2} (!= 3)",
                        set.ToString(), str, length));
            }
            length = ContainsSpanUTF16(set, str.Substring(1), SpanCondition.Simple);
            if (length != 2)
            {
                Errln(string.Format("FAIL: UnicodeSet({0}) containsSpanUTF16({1}) returns the wrong value length {2} (!= 2)",
                        set.ToString(), str, length));
            }
        }

        // Test select patterns and strings, and test SIMPLE.
        [Test]
        public void TestSimpleStringSpanAndFreeze()
        {
            String pattern = "[x{xy}{xya}{axy}{ax}]";
            String str = "xx"
                    + "xyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxya" + "xx"
                    + "xyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxya" + "xx"
                    + "xyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxyaxy" + "aaaa";

            UnicodeSet set = new UnicodeSet(pattern);
            if (set.ContainsAll(str))
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").containsAll(" + str + ") should be FALSE");
            }

            // Remove trailing "aaaa".
            String string16 = str.Substring(0, str.Length - 4); // ICU4N: Checked 2nd parameter
            if (!set.ContainsAll(string16))
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").containsAll(" + str + "[:-4]) should be TRUE");
            }

            String s16 = "byayaxya";
            if (set.Span(s16.Substring(0, 8), SpanCondition.NotContained) != 4 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 7), SpanCondition.NotContained) != 4 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 6), SpanCondition.NotContained) != 4 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 5), SpanCondition.NotContained) != 5 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 4), SpanCondition.NotContained) != 4 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 3), SpanCondition.NotContained) != 3) // ICU4N: Checked 2nd substring parameter
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").Span(while not) returns the wrong value");
            }

            pattern = "[a{ab}{abc}{cd}]";
            set.ApplyPattern(pattern);
            s16 = "acdabcdabccd";
            if (set.Span(s16.Substring(0, 12), SpanCondition.Contained) != 12 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 12), SpanCondition.Simple) != 6 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(7), SpanCondition.Simple) != 5)
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").Span(while longest match) returns the wrong value");
            }
            set.Freeze();
            if (set.Span(s16.Substring(0, 12), SpanCondition.Contained) != 12 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(0, 12), SpanCondition.Simple) != 6 // ICU4N: Checked 2nd substring parameter
                || set.Span(s16.Substring(7), SpanCondition.Simple) != 5)
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").Span(while longest match) returns the wrong value");
            }

            pattern = "[d{cd}{bcd}{ab}]";
            set = set.CloneAsThawed();
            set.ApplyPattern(pattern).Freeze();
            s16 = "abbcdabcdabd";
            if (set.SpanBack(s16, 12, SpanCondition.Contained) != 0
                || set.SpanBack(s16, 12, SpanCondition.Simple) != 6
                || set.SpanBack(s16, 5, SpanCondition.Simple) != 0)
            {
                Errln("FAIL: UnicodeSet(" + pattern + ").SpanBack(while longest match) returns the wrong value");
            }
        }

        // more complex test. --------------------------------------------------------

        // Make the strings in a UnicodeSet easily accessible.
        private class UnicodeSetWithStrings
        {
            private UnicodeSet set;
            private ICollection<String> setStrings;
            private int stringsLength;

            public UnicodeSetWithStrings(UnicodeSet normalSet)
            {
                set = normalSet;
                setStrings = normalSet.Strings;
                stringsLength = setStrings.Count;
            }

            public UnicodeSet Set
            {
                get { return set; }
            }

            public virtual bool HasStrings
            {
                get { return (stringsLength > 0); }
            }

            public IEnumerable<String> Strings
            {
                get { return setStrings; }
            }
        }

        // Compare 16-bit Unicode strings (which may be malformed UTF-16)
        // at code point boundaries.
        // That is, each edge of a match must not be in the middle of a surrogate pair.
        internal static bool Matches16CPB(String s, int start, int limit, String t)
        {
            limit -= start;
            int length = t.Length;
            return t.Equals(s.Substring(start, length)) // ICU4N: (start + length) - start == length
                    && !(0 < start && UTF16.IsLeadSurrogate(s[start - 1]) &&
                                      UTF16.IsTrailSurrogate(s[start]))
                    && !(length < limit && UTF16.IsLeadSurrogate(s[start + length - 1]) &&
                                           UTF16.IsTrailSurrogate(s[start + length]));
        }

        // Implement span() with contains() for comparison.
        private static int ContainsSpanUTF16(UnicodeSetWithStrings set, String s,
                SpanCondition spanCondition)
        {
            UnicodeSet realSet = set.Set;
            int length = s.Length;
            if (!set.HasStrings)
            {
                bool spanContained = false;
                if (spanCondition != SpanCondition.NotContained)
                {
                    spanContained = true; // Pin to 0/1 values.
                }

                int c;
                int start = 0, prev;
                while ((prev = start) < length)
                {
                    c = s.CodePointAt(start);
                    start = s.OffsetByCodePoints(start, 1);
                    if (realSet.Contains(c) != spanContained)
                    {
                        break;
                    }
                }
                return prev;
            }
            else if (spanCondition == SpanCondition.NotContained)
            {
                int c;
                int start, next;
                for (start = next = 0; start < length;)
                {
                    c = s.CodePointAt(next);
                    next = s.OffsetByCodePoints(next, 1);
                    if (realSet.Contains(c))
                    {
                        break;
                    }
                    foreach (String str in set.Strings)
                    {
                        if (str.Length <= (length - start) && Matches16CPB(s, start, length, str))
                        {
                            // spanNeedsStrings=true;
                            return start;
                        }
                    }
                    start = next;
                }
                return start;
            }
            else /* CONTAINED or SIMPLE */
            {
                int c;
                int start, next, maxSpanLimit = 0;
                for (start = next = 0; start < length;)
                {
                    c = s.CodePointAt(next);
                    next = s.OffsetByCodePoints(next, 1);
                    if (!realSet.Contains(c))
                    {
                        next = start; // Do not span this single, not-contained code point.
                    }
                    foreach (String str in set.Strings)
                    {
                        if (str.Length <= (length - start) && Matches16CPB(s, start, length, str))
                        {
                            // spanNeedsStrings=true;
                            int matchLimit = start + str.Length;
                            if (matchLimit == length)
                            {
                                return length;
                            }
                            if (spanCondition == SpanCondition.Contained)
                            {
                                // Iterate for the shortest match at each position.
                                // Recurse for each but the shortest match.
                                if (next == start)
                                {
                                    next = matchLimit; // First match from start.
                                }
                                else
                                {
                                    if (matchLimit < next)
                                    {
                                        // Remember shortest match from start for iteration.
                                        int temp = next;
                                        next = matchLimit;
                                        matchLimit = temp;
                                    }
                                    // Recurse for non-shortest match from start.
                                    int spanLength = ContainsSpanUTF16(set, s.Substring(matchLimit),
                                            SpanCondition.Contained);
                                    if ((matchLimit + spanLength) > maxSpanLimit)
                                    {
                                        maxSpanLimit = matchLimit + spanLength;
                                        if (maxSpanLimit == length)
                                        {
                                            return length;
                                        }
                                    }
                                }
                            }
                            else /* spanCondition==SIMPLE */
                            {
                                if (matchLimit > next)
                                {
                                    // Remember longest match from start.
                                    next = matchLimit;
                                }
                            }
                        }
                    }
                    if (next == start)
                    {
                        break; // No match from start.
                    }
                    start = next;
                }
                if (start > maxSpanLimit)
                {
                    return start;
                }
                else
                {
                    return maxSpanLimit;
                }
            }
        }

        private static int ContainsSpanBackUTF16(UnicodeSetWithStrings set, String s, int length,
                SpanCondition spanCondition)
        {
            if (length == 0)
            {
                return 0;
            }
            UnicodeSet realSet = set.Set;
            if (!set.HasStrings)
            {
                bool spanContained = false;
                if (spanCondition != SpanCondition.NotContained)
                {
                    spanContained = true; // Pin to 0/1 values.
                }

                int c;
                int prev = length;
                do
                {
                    c = s.CodePointBefore(prev);
                    if (realSet.Contains(c) != spanContained)
                    {
                        break;
                    }
                    prev = s.OffsetByCodePoints(prev, -1);
                } while (prev > 0);
                return prev;
            }
            else if (spanCondition == SpanCondition.NotContained)
            {
                int c;
                int prev = length, length0 = length;
                do
                {
                    c = s.CodePointBefore(prev);
                    if (realSet.Contains(c))
                    {
                        break;
                    }
                    foreach (String str in set.Strings)
                    {
                        if (str.Length <= prev && Matches16CPB(s, prev - str.Length, length0, str))
                        {
                            // spanNeedsStrings=true;
                            return prev;
                        }
                    }
                    prev = s.OffsetByCodePoints(prev, -1);
                } while (prev > 0);
                return prev;
            }
            else /* SpanCondition.CONTAINED or SIMPLE */
            {
                int c;
                int prev = length, minSpanStart = length, length0 = length;
                do
                {
                    c = s.CodePointBefore(length);
                    length = s.OffsetByCodePoints(length, -1);
                    if (!realSet.Contains(c))
                    {
                        length = prev; // Do not span this single, not-contained code point.
                    }
                    foreach (String str in set.Strings)
                    {
                        if (str.Length <= prev && Matches16CPB(s, prev - str.Length, length0, str))
                        {
                            // spanNeedsStrings=true;
                            int matchStart = prev - str.Length;
                            if (matchStart == 0)
                            {
                                return 0;
                            }
                            if (spanCondition == SpanCondition.Contained)
                            {
                                // Iterate for the shortest match at each position.
                                // Recurse for each but the shortest match.
                                if (length == prev)
                                {
                                    length = matchStart; // First match from prev.
                                }
                                else
                                {
                                    if (matchStart > length)
                                    {
                                        // Remember shortest match from prev for iteration.
                                        int temp = length;
                                        length = matchStart;
                                        matchStart = temp;
                                    }
                                    // Recurse for non-shortest match from prev.
                                    int spanStart = ContainsSpanBackUTF16(set, s, matchStart,
                                            SpanCondition.Contained);
                                    if (spanStart < minSpanStart)
                                    {
                                        minSpanStart = spanStart;
                                        if (minSpanStart == 0)
                                        {
                                            return 0;
                                        }
                                    }
                                }
                            }
                            else /* spanCondition==SIMPLE */
                            {
                                if (matchStart < length)
                                {
                                    // Remember longest match from prev.
                                    length = matchStart;
                                }
                            }
                        }
                    }
                    if (length == prev)
                    {
                        break; // No match from prev.
                    }
                } while ((prev = length) > 0);
                if (prev < minSpanStart)
                {
                    return prev;
                }
                else
                {
                    return minSpanStart;
                }
            }
        }

        // spans to be performed and compared
        internal const int SPAN_UTF16 = 1;
        internal const int SPAN_UTF8 = 2;
        internal const int SPAN_UTFS = 3;

        internal const int SPAN_SET = 4;
        internal const int SPAN_COMPLEMENT = 8;
        internal const int SPAN_POLARITY = 0xc;

        internal const int SPAN_FWD = 0x10;
        internal const int SPAN_BACK = 0x20;
        internal const int SPAN_DIRS = 0x30;

        internal const int SPAN_CONTAINED = 0x100;
        internal const int SPAN_SIMPLE = 0x200;
        internal const int SPAN_CONDITION = 0x300;

        internal const int SPAN_ALL = 0x33f;

        internal static SpanCondition InvertSpanCondition(SpanCondition spanCondition, SpanCondition contained)
        {
            return spanCondition == SpanCondition.NotContained ? contained
                    : SpanCondition.NotContained;
        }

        /*
         * Count spans on a string with the method according to type and set the span limits. The set may be the complement
         * of the original. When using spanBack() and comparing with span(), use a span condition for the first spanBack()
         * according to the expected number of spans. Sets typeName to an empty string if there is no such type. Returns -1
         * if the span option is filtered out.
         */
        private static int GetSpans(UnicodeSetWithStrings set, bool isComplement, String s,
                int whichSpans, int type, String[] typeName, int[] limits, int limitsCapacity,
                int expectCount)
        {
            UnicodeSet realSet = set.Set;
            int start, count, i;
            SpanCondition spanCondition, firstSpanCondition, contained;
            bool isForward;

            int length = s.Length;
            if (type < 0 || 7 < type)
            {
                typeName[0] = null;
                return 0;
            }

            String[] typeNames16 = {
                "contains",
                "contains(LM)",
                "span",
                "span(LM)",
                "containsBack",
                "containsBack(LM)",
                "spanBack",
                "spanBack(LM)" };

            typeName[0] = typeNames16[type];

            // filter span options
            if (type <= 3)
            {
                // span forward
                if ((whichSpans & SPAN_FWD) == 0)
                {
                    return -1;
                }
                isForward = true;
            }
            else
            {
                // span backward
                if ((whichSpans & SPAN_BACK) == 0)
                {
                    return -1;
                }
                isForward = false;
            }
            if ((type & 1) == 0)
            {
                // use SpanCondition.CONTAINED
                if ((whichSpans & SPAN_CONTAINED) == 0)
                {
                    return -1;
                }
                contained = SpanCondition.Contained;
            }
            else
            {
                // use SIMPLE
                if ((whichSpans & SPAN_SIMPLE) == 0)
                {
                    return -1;
                }
                contained = SpanCondition.Simple;
            }

            // Default first span condition for going forward with an uncomplemented set.
            spanCondition = SpanCondition.NotContained;
            if (isComplement)
            {
                spanCondition = InvertSpanCondition(spanCondition, contained);
            }

            // First span condition for span(), used to terminate the spanBack() iteration.
            firstSpanCondition = spanCondition;

            // spanBack(): Its initial span condition is span()'s last span condition,
            // which is the opposite of span()'s first span condition
            // if we expect an even number of spans.
            // (The loop inverts spanCondition (expectCount-1) times
            // before the expectCount'th span() call.)
            // If we do not compare forward and backward directions, then we do not have an
            // expectCount and just start with firstSpanCondition.
            if (!isForward && (whichSpans & SPAN_FWD) != 0 && (expectCount & 1) == 0)
            {
                spanCondition = InvertSpanCondition(spanCondition, contained);
            }

            count = 0;
            switch (type)
            {
                case 0:
                case 1:
                    start = 0;
                    for (; ; )
                    {
                        start += ContainsSpanUTF16(set, s.Substring(start), spanCondition);
                        if (count < limitsCapacity)
                        {
                            limits[count] = start;
                        }
                        ++count;
                        if (start >= length)
                        {
                            break;
                        }
                        spanCondition = InvertSpanCondition(spanCondition, contained);
                    }
                    break;
                case 2:
                case 3:
                    start = 0;
                    for (; ; )
                    {
                        start = realSet.Span(s, start, spanCondition);
                        if (count < limitsCapacity)
                        {
                            limits[count] = start;
                        }
                        ++count;
                        if (start >= length)
                        {
                            break;
                        }
                        spanCondition = InvertSpanCondition(spanCondition, contained);
                    }
                    break;
                case 4:
                case 5:
                    for (; ; )
                    {
                        ++count;
                        if (count <= limitsCapacity)
                        {
                            limits[limitsCapacity - count] = length;
                        }
                        length = ContainsSpanBackUTF16(set, s, length, spanCondition);
                        if (length == 0 && spanCondition == firstSpanCondition)
                        {
                            break;
                        }
                        spanCondition = InvertSpanCondition(spanCondition, contained);
                    }
                    if (count < limitsCapacity)
                    {
                        for (i = count; i-- > 0;)
                        {
                            limits[i] = limits[limitsCapacity - count + i];
                        }
                    }
                    break;
                case 6:
                case 7:
                    for (; ; )
                    {
                        ++count;
                        if (count <= limitsCapacity)
                        {
                            limits[limitsCapacity - count] = length >= 0 ? length : s.Length;
                        }
                        length = realSet.SpanBack(s, length, spanCondition);
                        if (length == 0 && spanCondition == firstSpanCondition)
                        {
                            break;
                        }
                        spanCondition = InvertSpanCondition(spanCondition, contained);
                    }
                    if (count < limitsCapacity)
                    {
                        for (i = count; i-- > 0;)
                        {
                            limits[i] = limits[limitsCapacity - count + i];
                        }
                    }
                    break;
                default:
                    typeName = null;
                    return -1;
            }

            return count;
        }

        // sets to be tested; odd index=isComplement
        internal const int SLOW = 0;
        internal const int SLOW_NOT = 1;
        internal const int FAST = 2;
        internal const int FAST_NOT = 3;
        internal const int SET_COUNT = 4;

        internal static readonly String[] setNames = { "slow", "slow.not", "fast", "fast.not" };

        /*
         * Verify that we get the same results whether we look at text with contains(), span() or spanBack(), using unfrozen
         * or frozen versions of the set, and using the set or its complement (switching the spanConditions accordingly).
         * The latter verifies that set.Span(spanCondition) == set.complement().Span(!spanCondition).
         *
         * The expectLimits[] are either provided by the caller (with expectCount>=0) or returned to the caller (with an
         * input expectCount<0).
         */
        private void VerifySpan(UnicodeSetWithStrings[] sets, String s, int whichSpans,
                int[] expectLimits, int expectCount,
                String testName, int index)
        {
            int[] limits = new int[500];
            int limitsCount;
            int i, j;
            String[] typeName = new String[1];
            int type;

            for (i = 0; i < SET_COUNT; ++i)
            {
                if ((i & 1) == 0)
                {
                    // Even-numbered sets are original, uncomplemented sets.
                    if ((whichSpans & SPAN_SET) == 0)
                    {
                        continue;
                    }
                }
                else
                {
                    // Odd-numbered sets are complemented.
                    if ((whichSpans & SPAN_COMPLEMENT) == 0)
                    {
                        continue;
                    }
                }
                for (type = 0; ; ++type)
                {
                    limitsCount = GetSpans(sets[i], (0 != (i & 1)), s, whichSpans, type, typeName, limits,
                            limits.Length, expectCount);
                    if (typeName[0] == null)
                    {
                        break; // All types tried.
                    }
                    if (limitsCount < 0)
                    {
                        continue; // Span option filtered out.
                    }
                    if (expectCount < 0)
                    {
                        expectCount = limitsCount;
                        if (limitsCount > limits.Length)
                        {
                            Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.{3} span count={4} > {5} capacity - too many spans",
                                    testName, index, setNames[i], typeName[0], limitsCount, limits.Length));
                            return;
                        }
                        for (j = limitsCount; j-- > 0;)
                        {
                            expectLimits[j] = limits[j];
                        }
                    }
                    else if (limitsCount != expectCount)
                    {
                        Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.{3} span count={4} != {5}", testName, index, setNames[i],
                                typeName[0], limitsCount, expectCount));
                    }
                    else
                    {
                        for (j = 0; j < limitsCount; ++j)
                        {
                            if (limits[j] != expectLimits[j])
                            {
                                Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.{3} span count={4} limits[{5}]={6} != {7}", testName,
                                        index, setNames[i], typeName[0], limitsCount, j, limits[j], expectLimits[j]));
                                break;
                            }
                        }
                    }
                }
            }

            // Compare span() with containsAll()/containsNone(),
            // but only if we have expectLimits[] from the uncomplemented set.
            if ((whichSpans & SPAN_SET) != 0)
            {
                String s16 = s;
                String str;
                int prev = 0, limit, len;
                for (i = 0; i < expectCount; ++i)
                {
                    limit = expectLimits[i];
                    len = limit - prev;
                    if (len > 0)
                    {
                        str = s16.Substring(prev, len); // read-only alias // ICU4N: (prev + len) - prev == len
                        if (0 != (i & 1))
                        {
                            if (!sets[SLOW].Set.ContainsAll(str))
                            {
                                Errln(string.Format("FAIL:{0}[0x{1:x}].{2}.containsAll({3}..{4})==false contradicts span()",
                                        testName, index, setNames[SLOW], prev, limit));
                                return;
                            }
                            if (!sets[FAST].Set.ContainsAll(str))
                            {
                                Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.containsAll({3}..{4})==false contradicts span()",
                                        testName, index, setNames[FAST], prev, limit));
                                return;
                            }
                        }
                        else
                        {
                            if (!sets[SLOW].Set.ContainsNone(str))
                            {
                                Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.containsNone([3}..{4})==false contradicts span()",
                                        testName, index, setNames[SLOW], prev, limit));
                                return;
                            }
                            if (!sets[FAST].Set.ContainsNone(str))
                            {
                                Errln(string.Format("FAIL: {0}[0x{1:x}].{2}.containsNone({3}..{4})==false contradicts span()",
                                        testName, index, setNames[FAST], prev, limit));
                                return;
                            }
                        }
                    }
                    prev = limit;
                }
            }
        }

        // Specifically test either UTF-16 or UTF-8.
        private void VerifySpan(UnicodeSetWithStrings[] sets, String s, int whichSpans,
                String testName, int index)
        {
            int[] expectLimits = new int[500];
            int expectCount = -1;
            VerifySpan(sets, s, whichSpans, expectLimits, expectCount, testName, index);
        }

        // Test both UTF-16 and UTF-8 versions of span() etc. on the same sets and text,
        // unless either UTF is turned off in whichSpans.
        // Testing UTF-16 and UTF-8 together requires that surrogate code points
        // have the same contains(c) value as U+FFFD.
        private void VerifySpanBothUTFs(UnicodeSetWithStrings[] sets, String s16, int whichSpans,
                String testName, int index)
        {
            int[] expectLimits = new int[500];
            int expectCount;

            expectCount = -1; // Get expectLimits[] from verifySpan().

            if ((whichSpans & SPAN_UTF16) != 0)
            {
                VerifySpan(sets, s16, whichSpans, expectLimits, expectCount, testName, index);
            }
        }

        internal static int NextCodePoint(int c)
        {
            // Skip some large and boring ranges.
            switch (c)
            {
                case 0x3441:
                    return 0x4d7f;
                case 0x5100:
                    return 0x9f00;
                case 0xb040:
                    return 0xd780;
                case 0xe041:
                    return 0xf8fe;
                case 0x10100:
                    return 0x20000;
                case 0x20041:
                    return 0xe0000;
                case 0xe0101:
                    return 0x10fffd;
                default:
                    return c + 1;
            }
        }

        // Verify that all implementations represent the same set.
        private void VerifySpanContents(UnicodeSetWithStrings[] sets, int whichSpans, String testName)
        {
            StringBuffer s = new StringBuffer();
            int localWhichSpans;
            int c, first;
            for (first = c = 0; ; c = NextCodePoint(c))
            {
                if (c > 0x10ffff || s.Length > 1024)
                {
                    localWhichSpans = whichSpans;
                    VerifySpanBothUTFs(sets, s.ToString(), localWhichSpans, testName, first);
                    if (c > 0x10ffff)
                    {
                        break;
                    }
                    s.Delete(0, s.Length - 0); // ICU4N: Corrected 2nd parameter of Delete
                    first = c;
                }
                UTF16.Append(s, c);
            }
        }

        // Test with a particular, interesting string.
        // Specify length and try NUL-termination.
        internal static readonly char[] interestingStringChars = { (char)0x61, (char)0x62, (char)0x20, // Latin, space
                    (char)0x3b1, (char)0x3b2, (char)0x3b3, // Greek
                    (char)0xd900, // lead surrogate
                    (char)0x3000, (char)0x30ab, (char)0x30ad, // wide space, Katakana
                    (char)0xdc05, // trail surrogate
                    (char)0xa0, (char)0xac00, (char)0xd7a3, // nbsp, Hangul
                    (char)0xd900, (char)0xdc05, // unassigned supplementary
                    (char)0xd840, (char)0xdfff, (char)0xd860, (char)0xdffe, // Han supplementary
                    (char)0xd7a4, (char)0xdc05, (char)0xd900, (char)0x2028  // unassigned, surrogates in wrong order, LS
            };
        internal static String interestingString = new String(interestingStringChars);
        internal const String unicodeSet1 = "[[[:ID_Continue:]-[\\u30ab\\u30ad]]{\\u3000\\u30ab}{\\u3000\\u30ab\\u30ad}]";

        [Test]
        public void TestInterestingStringSpan()
        {
            UnicodeSet uset = new UnicodeSet(Utility.Unescape(unicodeSet1));
            SpanCondition spanCondition = SpanCondition.NotContained;
            int expect = 2;
            int start = 14;

            int c = 0xd840;
            bool contains = uset.Contains(c);
            if (false != contains)
            {
                Errln(string.Format("FAIL: UnicodeSet(unicodeSet1).Contains({0}) = true (expect false)",
                      c));
            }

            UnicodeSetWithStrings set = new UnicodeSetWithStrings(uset);
            int len = ContainsSpanUTF16(set, interestingString.Substring(start), spanCondition);
            if (expect != len)
            {
                Errln(string.Format("FAIL: containsSpanUTF16(unicodeSet1, \"{0}({1})\") = {2} (expect {3})",
                      interestingString, start, len, expect));
            }

            len = uset.Span(interestingString, start, spanCondition) - start;
            if (expect != len)
            {
                Errln(string.Format("FAIL: UnicodeSet(unicodeSet1).Span(\"{0}\", {1}) = {2} (expect {3})",
                      interestingString, start, len, expect));
            }
        }

        private void VerifySpanUTF16String(UnicodeSetWithStrings[] sets, int whichSpans, String testName)
        {
            if ((whichSpans & SPAN_UTF16) == 0)
            {
                return;
            }
            VerifySpan(sets, interestingString, (whichSpans & ~SPAN_UTF8), testName, 1);
        }

        // Take a set of span options and multiply them so that
        // each portion only has one of the options a, b and c.
        // If b==0, then the set of options is just modified with mask and a.
        // If b!=0 and c==0, then the set of options is just modified with mask, a and b.
        private static int AddAlternative(int[] whichSpans, int whichSpansCount, int mask, int a, int b, int c)
        {
            int s;
            int i;

            for (i = 0; i < whichSpansCount; ++i)
            {
                s = whichSpans[i] & mask;
                whichSpans[i] = s | a;
                if (b != 0)
                {
                    whichSpans[whichSpansCount + i] = s | b;
                    if (c != 0)
                    {
                        whichSpans[2 * whichSpansCount + i] = s | c;
                    }
                }
            }
            return b == 0 ? whichSpansCount : c == 0 ? 2 * whichSpansCount : 3 * whichSpansCount;
        }

        // They are not representable in UTF-8, and a leading trail surrogate
        // and a trailing lead surrogate must not match in the middle of a proper surrogate pair.
        // U+20001 == \\uD840\\uDC01
        // U+20400 == \\uD841\\uDC00
        internal const String patternWithUnpairedSurrogate =
        "[a\\U00020001\\U00020400{ab}{b\\uD840}{\\uDC00a}]";
        internal const String stringWithUnpairedSurrogate =
        "aaab\\U00020001ba\\U00020400aba\\uD840ab\\uD840\\U00020000b\\U00020000a\\U00020000\\uDC00a\\uDC00babbb";

        internal const String _63_a = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        internal const String _64_a = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        internal const String _63_b = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        internal const String _64_b = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        internal const String longPattern =
        "[a{" + _64_a + _64_a + _64_a + _64_a + "b}" + "{a" + _64_b + _64_b + _64_b + _64_b + "}]";

        [Test]
        public void TestStringWithUnpairedSurrogateSpan()
        {
            String str = Utility.Unescape(stringWithUnpairedSurrogate);
            UnicodeSet uset = new UnicodeSet(Utility.Unescape(patternWithUnpairedSurrogate));
            SpanCondition spanCondition = SpanCondition.NotContained;
            int start = 17;
            int expect = 5;

            UnicodeSetWithStrings set = new UnicodeSetWithStrings(uset);
            int len = ContainsSpanUTF16(set, str.Substring(start), spanCondition);
            if (expect != len)
            {
                Errln(string.Format("FAIL: containsSpanUTF16(patternWithUnpairedSurrogate, \"{0}({1})\") = {2} (expect {3})",
                      str, start, len, expect));
            }

            len = uset.Span(str, start, spanCondition) - start;
            if (expect != len)
            {
                Errln(string.Format("FAIL: UnicodeSet(patternWithUnpairedSurrogate).Span(\"{0}\", {1}) = {2} (expect {3})",
                      str, start, len, expect));
            }
        }

        [Test]
        public void TestSpan()
        {
            // "[...]" is a UnicodeSet pattern.
            // "*" performs tests on all Unicode code points and on a selection of
            // malformed UTF-8/16 strings.
            // "-options" limits the scope of testing for the current set.
            // By default, the test verifies that equivalent boundaries are found
            // for UTF-16 and UTF-8, going forward and backward,
            // alternating NOT_CONTAINED with
            // either CONTAINED or SIMPLE.
            // Single-character options:
            // 8 -- UTF-16 and UTF-8 boundaries may differ.
            // Cause: contains(U+FFFD) is inconsistent with contains(some surrogates),
            // or the set contains strings with unpaired surrogates
            // which do not translate to valid UTF-8.
            // c -- set.Span() and set.complement().Span() boundaries may differ.
            // Cause: Set strings are not complemented.
            // b -- span() and spanBack() boundaries may differ.
            // Cause: Strings in the set overlap, and spanBack(CONTAINED)
            // and spanBack(SIMPLE) are defined to
            // match with non-overlapping substrings.
            // For example, with a set containing "ab" and "ba",
            // span() of "aba" yields boundaries { 0, 2, 3 }
            // because the initial "ab" matches from 0 to 2,
            // while spanBack() yields boundaries { 0, 1, 3 }
            // because the final "ba" matches from 1 to 3.
            // l -- CONTAINED and SIMPLE boundaries may differ.
            // Cause: Strings in the set overlap, and a longer match may
            // require a sequence including non-longest substrings.
            // For example, with a set containing "ab", "abc" and "cd",
            // span(contained) of "abcd" spans the entire string
            // but span(longest match) only spans the first 3 characters.
            // Each "-options" first resets all options and then applies the specified options.
            // A "-" without options resets the options.
            // The options are also reset for each new set.
            // Other strings will be spanned.
            String[] testdata = {
                "[:ID_Continue:]",
                "*",
                "[:White_Space:]",
                "*",
                "[]",
                "*",
                "[\\u0000-\\U0010FFFF]",
                "*",
                "[\\u0000\\u0080\\u0800\\U00010000]",
                "*",
                "[\\u007F\\u07FF\\uFFFF\\U0010FFFF]",
                "*",
                unicodeSet1,
                "-c",
                "*",
                "[[[:ID_Continue:]-[\\u30ab\\u30ad]]{\\u30ab\\u30ad}{\\u3000\\u30ab\\u30ad}]",
                "-c",
                "*",

                // Overlapping strings cause overlapping attempts to match.
                "[x{xy}{xya}{axy}{ax}]",
                "-cl",

                // More repetitions of "xya" would take too long with the recursive
                // reference implementation.
                // containsAll()=false
                // test_string 0x14
                "xx" + "xyaxyaxyaxya" + // set.complement().Span(longest match) will stop here.
                        "xx" + // set.complement().Span(contained) will stop between the two 'x'es.
                        "xyaxyaxyaxya" + "xx" + "xyaxyaxyaxya" + // span() ends here.
                        "aaa",

                // containsAll()=true
                // test_string 0x15
                "xx" + "xyaxyaxyaxya" + "xx" + "xyaxyaxyaxya" + "xx" + "xyaxyaxyaxy",

                "-bc",
                // test_string 0x17
                "byayaxya", // span() -> { 4, 7, 8 } spanBack() -> { 5, 8 }
                "-c",
                "byayaxy", // span() -> { 4, 7 } complement.Span() -> { 7 }
                "byayax", // span() -> { 4, 6 } complement.Span() -> { 6 }
                "-",
                "byaya", // span() -> { 5 }
                "byay", // span() -> { 4 }
                "bya", // span() -> { 3 }

                // span(longest match) will not span the whole string.
                "[a{ab}{bc}]",
                "-cl",
                // test_string 0x21
                "abc",

                "[a{ab}{abc}{cd}]",
                "-cl",
                "acdabcdabccd",

                // spanBack(longest match) will not span the whole string.
                "[c{ab}{bc}]",
                "-cl",
                "abc",

                "[d{cd}{bcd}{ab}]",
                "-cl",
                "abbcdabcdabd",

                // Test with non-ASCII set strings - test proper handling of surrogate pairs
                // and UTF-8 trail bytes.
                // Copies of above test sets and strings, but transliterated to have
                // different code points with similar trail units.
                // Previous: a b c d
                // Unicode: 042B 30AB 200AB 204AB
                // UTF-16: 042B 30AB D840 DCAB D841 DCAB
                // UTF-8: D0 AB E3 82 AB F0 A0 82 AB F0 A0 92 AB
                "[\\u042B{\\u042B\\u30AB}{\\u042B\\u30AB\\U000200AB}{\\U000200AB\\U000204AB}]",
                "-cl",
                "\\u042B\\U000200AB\\U000204AB\\u042B\\u30AB\\U000200AB\\U000204AB\\u042B\\u30AB\\U000200AB\\U000200AB\\U000204AB",

                "[\\U000204AB{\\U000200AB\\U000204AB}{\\u30AB\\U000200AB\\U000204AB}{\\u042B\\u30AB}]",
                "-cl",
                "\\u042B\\u30AB\\u30AB\\U000200AB\\U000204AB\\u042B\\u30AB\\U000200AB\\U000204AB\\u042B\\u30AB\\U000204AB",

                // Stress bookkeeping and recursion.
                // The following strings are barely doable with the recursive
                // reference implementation.
                // The not-contained character at the end prevents an early exit from the span().
                "[b{bb}]",
                "-c",
                // test_string 0x33
                "bbbbbbbbbbbbbbbbbbbbbbbb-",
                // On complement sets, span() and spanBack() get different results
                // because b is not in the complement set and there is an odd number of b's
                // in the test string.
                "-bc",
                "bbbbbbbbbbbbbbbbbbbbbbbbb-",

                // Test with set strings with an initial or final code point span
                // longer than 254.
                longPattern,
                "-c",
                _64_a + _64_a + _64_a + _63_a + "b",
                _64_a + _64_a + _64_a + _64_a + "b",
                _64_a + _64_a + _64_a + _64_a + "aaaabbbb",
                "a" + _64_b + _64_b + _64_b + _63_b,
                "a" + _64_b + _64_b + _64_b + _64_b,
                "aaaabbbb" + _64_b + _64_b + _64_b + _64_b,

                // Test with strings containing unpaired surrogates.
                patternWithUnpairedSurrogate, "-8cl",
                stringWithUnpairedSurrogate };
            int i, j;
            int whichSpansCount = 1;
            int[] whichSpans = new int[96];
            for (i = whichSpans.Length; i-- > 0;)
            {
                whichSpans[i] = SPAN_ALL;
            }

            UnicodeSet[] sets = new UnicodeSet[SET_COUNT];
            UnicodeSetWithStrings[] sets_with_str = new UnicodeSetWithStrings[SET_COUNT];

            String testName = null;
            String testNameLimit;

            for (i = 0; i < testdata.Length; ++i)
            {
                String s = testdata[i];
                if (s[0] == '[')
                {
                    // Create new test sets from this pattern.
                    for (j = 0; j < SET_COUNT; ++j)
                    {
                        sets_with_str[j] = null;
                        sets[j] = null;
                    }
                    sets[SLOW] = new UnicodeSet(Utility.Unescape(s));
                    sets[SLOW_NOT] = new UnicodeSet(sets[SLOW]);
                    sets[SLOW_NOT].Complement();
                    // Intermediate set: Test cloning of a frozen set.
                    UnicodeSet fast = new UnicodeSet(sets[SLOW]);
                    fast.Freeze();
                    sets[FAST] = (UnicodeSet)fast.Clone();
                    fast = null;
                    UnicodeSet fastNot = new UnicodeSet(sets[SLOW_NOT]);
                    fastNot.Freeze();
                    sets[FAST_NOT] = (UnicodeSet)fastNot.Clone();
                    fastNot = null;

                    for (j = 0; j < SET_COUNT; ++j)
                    {
                        sets_with_str[j] = new UnicodeSetWithStrings(sets[j]);
                    }

                    testName = s + ':';
                    whichSpans[0] = SPAN_ALL;
                    whichSpansCount = 1;
                }
                else if (s[0] == '-')
                {
                    whichSpans[0] = SPAN_ALL;
                    whichSpansCount = 1;

                    for (j = 1; j < s.Length; j++)
                    {
                        switch (s[j])
                        {
                            case 'c':
                                whichSpansCount = AddAlternative(whichSpans, whichSpansCount, ~SPAN_POLARITY, SPAN_SET,
                                        SPAN_COMPLEMENT, 0);
                                break;
                            case 'b':
                                whichSpansCount = AddAlternative(whichSpans, whichSpansCount, ~SPAN_DIRS, SPAN_FWD, SPAN_BACK,
                                        0);
                                break;
                            case 'l':
                                // test CONTAINED FWD & BACK, and separately
                                // SIMPLE only FWD, and separately
                                // SIMPLE only BACK
                                whichSpansCount = AddAlternative(whichSpans, whichSpansCount, ~(SPAN_DIRS | SPAN_CONDITION),
                                        SPAN_DIRS | SPAN_CONTAINED, SPAN_FWD | SPAN_SIMPLE, SPAN_BACK | SPAN_SIMPLE);
                                break;
                            case '8':
                                whichSpansCount = AddAlternative(whichSpans, whichSpansCount, ~SPAN_UTFS, SPAN_UTF16,
                                        SPAN_UTF8, 0);
                                break;
                            default:
                                Errln(String.Format("FAIL: unrecognized span set option in \"{0}\"", testdata[i]));
                                break;
                        }
                    }
                }
                else if (s.Equals("*"))
                {
                    testNameLimit = "bad_string";
                    for (j = 0; j < whichSpansCount; ++j)
                    {
                        if (whichSpansCount > 1)
                        {
                            testNameLimit += String.Format("%%0x{0:x3}", whichSpans[j]);
                        }
                        VerifySpanUTF16String(sets_with_str, whichSpans[j], testName);
                    }

                    testNameLimit = "contents";
                    for (j = 0; j < whichSpansCount; ++j)
                    {
                        if (whichSpansCount > 1)
                        {
                            testNameLimit += String.Format("%%0x{0:x3}", whichSpans[j]);
                        }
                        VerifySpanContents(sets_with_str, whichSpans[j], testName);
                    }
                }
                else
                {
                    String str = Utility.Unescape(s);
                    testNameLimit = "test_string";
                    for (j = 0; j < whichSpansCount; ++j)
                    {
                        if (whichSpansCount > 1)
                        {
                            testNameLimit += String.Format("%%0x{0:x3}", whichSpans[j]);
                        }
                        VerifySpanBothUTFs(sets_with_str, str, whichSpans[j], testName, i);
                    }
                }
            }
        }

        [Test]
        public void TestSpanAndCount()
        {
            // a set with no strings
            UnicodeSet abc = new UnicodeSet('a', 'c');
            // a set with an "irrelevant" string (fully contained in the code point set)
            UnicodeSet crlf = new UnicodeSet().Add('\n').Add('\r').Add("\r\n");
            // a set with no "irrelevant" string but some interesting overlaps
            UnicodeSet ab_cd = new UnicodeSet().Add('a').Add("ab").Add("abc").Add("cd");
            String s = "ab\n\r\r\n" + UTF16.ValueOf(0x50000) + "abcde";
            int count = 0;
            assertEquals("abc span[8, 11[", 11,
                    abc.SpanAndCount(s, 8, SpanCondition.Simple, out count));
            assertEquals("abc count=3", 3, count);
            assertEquals("no abc span[2, 8[", 8,
                    abc.SpanAndCount(s, 2, SpanCondition.NotContained, out count));
            assertEquals("no abc count=5", 5, count);
            assertEquals("line endings span[2, 6[", 6,
                    crlf.SpanAndCount(s, 2, SpanCondition.Contained, out count));
            assertEquals("line endings count=3", 3, count);
            assertEquals("no ab+cd span[2, 8[", 8,
                    ab_cd.SpanAndCount(s, 2, SpanCondition.NotContained, out count));
            assertEquals("no ab+cd count=5", 5, count);
            assertEquals("ab+cd span[8, 12[", 12,
                    ab_cd.SpanAndCount(s, 8, SpanCondition.Contained, out count));
            assertEquals("ab+cd count=2", 2, count);
            assertEquals("1x abc span[8, 11[", 11,
                    ab_cd.SpanAndCount(s, 8, SpanCondition.Simple, out count));
            assertEquals("1x abc count=1", 1, count);

            abc.Freeze();
            crlf.Freeze();
            ab_cd.Freeze();
            assertEquals("abc span[8, 11[ (frozen)", 11,
                    abc.SpanAndCount(s, 8, SpanCondition.Simple, out count));
            assertEquals("abc count=3 (frozen)", 3, count);
            assertEquals("no abc span[2, 8[ (frozen)", 8,
                    abc.SpanAndCount(s, 2, SpanCondition.NotContained, out count));
            assertEquals("no abc count=5 (frozen)", 5, count);
            assertEquals("line endings span[2, 6[ (frozen)", 6,
                    crlf.SpanAndCount(s, 2, SpanCondition.Contained, out count));
            assertEquals("line endings count=3 (frozen)", 3, count);
            assertEquals("no ab+cd span[2, 8[ (frozen)", 8,
                    ab_cd.SpanAndCount(s, 2, SpanCondition.NotContained, out count));
            assertEquals("no ab+cd count=5 (frozen)", 5, count);
            assertEquals("ab+cd span[8, 12[ (frozen)", 12,
                    ab_cd.SpanAndCount(s, 8, SpanCondition.Contained, out count));
            assertEquals("ab+cd count=2 (frozen)", 2, count);
            assertEquals("1x abc span[8, 11[ (frozen)", 11,
                    ab_cd.SpanAndCount(s, 8, SpanCondition.Simple, out count));
            assertEquals("1x abc count=1 (frozen)", 1, count);
        }
    }
}
