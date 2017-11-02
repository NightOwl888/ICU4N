using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections;
using ICU4N.Support;
using System.Linq;
using ICU4N.Support.Collections;

namespace ICU4N.Text
{
    public class UnicodeSet : UnicodeFilter, IEnumerable<string>, IComparable<UnicodeSet>, IFreezable<UnicodeSet>
    {
        private static readonly object syncLock = new object();

        /**
     * Constant for the empty set.
     * @stable ICU 4.8
     */
        public static readonly UnicodeSet EMPTY = new UnicodeSet().Freeze();
        /**
         * Constant for the set of all code points. (Since UnicodeSets can include strings, does not include everything that a UnicodeSet can.)
         * @stable ICU 4.8
         */
        public static readonly UnicodeSet ALL_CODE_POINTS = new UnicodeSet(0, 0x10FFFF).Freeze();

        private static XSymbolTable XSYMBOL_TABLE = null; // for overriding the the function processing

        private static readonly int LOW = 0x000000; // LOW <= all valid values. ZERO for codepoints
        private static readonly int HIGH = 0x110000; // HIGH > all valid values. 10000 for code units.
                                                     // 110000 for codepoints

        /**
         * Minimum value that can be stored in a UnicodeSet.
         * @stable ICU 2.0
         */
        public static readonly int MIN_VALUE = LOW;

        /**
         * Maximum value that can be stored in a UnicodeSet.
         * @stable ICU 2.0
         */
        public static readonly int MAX_VALUE = HIGH - 1;

        private int len;      // length used; list may be longer to minimize reallocs
        private int[] list;   // MUST be terminated with HIGH
        private int[] rangeList; // internal buffer
        private int[] buffer; // internal buffer

        // NOTE: normally the field should be of type SortedSet; but that is missing a public clone!!
        // is not private so that UnicodeSetIterator can get access
        private SortedSet<string> strings = new SortedSet<string>(StringComparer.Ordinal);

        /**
         * The pattern representation of this set.  This may not be the
         * most economical pattern.  It is the pattern supplied to
         * applyPattern(), with variables substituted and whitespace
         * removed.  For sets constructed without applyPattern(), or
         * modified using the non-pattern API, this string will be null,
         * indicating that toPattern() must generate a pattern
         * representation from the inversion list.
         */
        private string pat = null;

        private static readonly int START_EXTRA = 16;         // initial storage. Must be >= 0
        private static readonly int GROW_EXTRA = START_EXTRA; // extra amount for growth. Must be >= 0

        // Special property set IDs
        private static readonly String ANY_ID = "ANY";   // [\u0000-\U0010FFFF]
        private static readonly String ASCII_ID = "ASCII"; // [\u0000-\u007F]
        private static readonly String ASSIGNED = "Assigned"; // [:^Cn:]

        /**
         * A set of all characters _except_ the second through last characters of
         * certain ranges.  These ranges are ranges of characters whose
         * properties are all exactly alike, e.g. CJK Ideographs from
         * U+4E00 to U+9FA5.
         */
        private static UnicodeSet[] INCLUSIONS = null;

        private volatile BMPSet bmpSet; // The set is frozen if bmpSet or stringSpan is not null.
        private volatile UnicodeSetStringSpan stringSpan;
        //----------------------------------------------------------------
        // Public API
        //----------------------------------------------------------------
#pragma warning disable 612, 618

        /**
         * Constructs an empty set.
         * @stable ICU 2.0
         */
        public UnicodeSet()
        {
            list = new int[1 + START_EXTRA];
            list[len++] = HIGH;
        }

        /**
         * Constructs a copy of an existing set.
         * @stable ICU 2.0
         */
        public UnicodeSet(UnicodeSet other)
        {
            Set(other);
        }

        /**
         * Constructs a set containing the given range. If <code>end &gt;
         * start</code> then an empty set is created.
         *
         * @param start first character, inclusive, of range
         * @param end last character, inclusive, of range
         * @stable ICU 2.0
         */
        public UnicodeSet(int start, int end)
            : this()
        {
            Complement(start, end);
        }

        /**
         * Quickly constructs a set from a set of ranges &lt;s0, e0, s1, e1, s2, e2, ..., sn, en&gt;.
         * There must be an even number of integers, and they must be all greater than zero,
         * all less than or equal to Character.MAX_CODE_POINT.
         * In each pair (..., si, ei, ...) it must be true that si &lt;= ei
         * Between adjacent pairs (...ei, sj...), it must be true that ei+1 &lt; sj
         * @param pairs pairs of character representing ranges
         * @stable ICU 4.4
         */
        public UnicodeSet(params int[] pairs)
        {
            if ((pairs.Length & 1) != 0)
            {
                throw new ArgumentException("Must have even number of integers");
            }
            list = new int[pairs.Length + 1]; // don't allocate extra space, because it is likely that this is a fixed set.
            len = list.Length;
            int last = -1; // used to ensure that the results are monotonically increasing.
            int i = 0;
            while (i < pairs.Length)
            {
                // start of pair
                int start = pairs[i];
                if (last >= start)
                {
                    throw new ArgumentException("Must be monotonically increasing.");
                }
                list[i++] = last = start;
                // end of pair
                int end = pairs[i] + 1;
                if (last >= end)
                {
                    throw new ArgumentException("Must be monotonically increasing.");
                }
                list[i++] = last = end;
            }
            list[i] = HIGH; // terminate
        }
#pragma warning restore 612, 618

        /**
         * Constructs a set from the given pattern.  See the class description
         * for the syntax of the pattern language.  Whitespace is ignored.
         * @param pattern a string specifying what characters are in the set
         * @exception java.lang.ArgumentException if the pattern contains
         * a syntax error.
         * @stable ICU 2.0
         */
        public UnicodeSet(string pattern)
            : this()
        {
            ApplyPattern(pattern, null, null, IGNORE_SPACE);
        }

        /**
         * Constructs a set from the given pattern.  See the class description
         * for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param ignoreWhitespace if true, ignore Unicode Pattern_White_Space characters
         * @exception java.lang.ArgumentException if the pattern contains
         * a syntax error.
         * @stable ICU 2.0
         */
        public UnicodeSet(string pattern, bool ignoreWhitespace)
            : this()
        {
            ApplyPattern(pattern, null, null, ignoreWhitespace ? IGNORE_SPACE : 0);
        }

        /**
         * Constructs a set from the given pattern.  See the class description
         * for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param options a bitmask indicating which options to apply.
         * Valid options are IGNORE_SPACE and CASE.
         * @exception java.lang.ArgumentException if the pattern contains
         * a syntax error.
         * @stable ICU 3.8
         */
        public UnicodeSet(string pattern, int options)
            : this()
        {
            ApplyPattern(pattern, null, null, options);
        }

        /**
         * Constructs a set from the given pattern.  See the class description
         * for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param pos on input, the position in pattern at which to start parsing.
         * On output, the position after the last character parsed.
         * @param symbols a symbol table mapping variables to char[] arrays
         * and chars to UnicodeSets
         * @exception java.lang.ArgumentException if the pattern
         * contains a syntax error.
         * @stable ICU 2.0
         */
        public UnicodeSet(string pattern, ParsePosition pos, ISymbolTable symbols)
            : this()
        {
            ApplyPattern(pattern, pos, symbols, IGNORE_SPACE);
        }

        /**
         * Constructs a set from the given pattern.  See the class description
         * for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param pos on input, the position in pattern at which to start parsing.
         * On output, the position after the last character parsed.
         * @param symbols a symbol table mapping variables to char[] arrays
         * and chars to UnicodeSets
         * @param options a bitmask indicating which options to apply.
         * Valid options are IGNORE_SPACE and CASE.
         * @exception java.lang.ArgumentException if the pattern
         * contains a syntax error.
         * @stable ICU 3.2
         */
        public UnicodeSet(string pattern, ParsePosition pos, ISymbolTable symbols, int options)
            : this()
        {
            ApplyPattern(pattern, pos, symbols, options);
        }


        /**
         * Return a new set that is equivalent to this one.
         * @stable ICU 2.0
         */
        public object Clone()
        {
            if (IsFrozen)
            {
                return this;
            }
            UnicodeSet result = new UnicodeSet(this);
            result.bmpSet = this.bmpSet;
            result.stringSpan = this.stringSpan;
            return result;
        }

        /**
         * Make this object represent the range <code>start - end</code>.
         * If <code>end &gt; start</code> then this object is set to an
         * an empty range.
         *
         * @param start first character in the set, inclusive
         * @param end last character in the set, inclusive
         * @stable ICU 2.0
         */
        public UnicodeSet Set(int start, int end)
        {
            CheckFrozen();
            Clear();
            Complement(start, end);
            return this;
        }

        /**
         * Make this object represent the same set as <code>other</code>.
         * @param other a <code>UnicodeSet</code> whose value will be
         * copied to this object
         * @stable ICU 2.0
         */
        public UnicodeSet Set(UnicodeSet other)
        {
            CheckFrozen();
            list = (int[])other.list.Clone();
            len = other.len;
            pat = other.pat;
            strings = new SortedSet<string>(other.strings, StringComparer.Ordinal);
            return this;
        }

        /**
         * Modifies this set to represent the set specified by the given pattern.
         * See the class description for the syntax of the pattern language.
         * Whitespace is ignored.
         * @param pattern a string specifying what characters are in the set
         * @exception java.lang.ArgumentException if the pattern
         * contains a syntax error.
         * @stable ICU 2.0
         */
        public UnicodeSet ApplyPattern(string pattern)
        {
            CheckFrozen();
            return ApplyPattern(pattern, null, null, IGNORE_SPACE);
        }

        /**
         * Modifies this set to represent the set specified by the given pattern,
         * optionally ignoring whitespace.
         * See the class description for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param ignoreWhitespace if true then Unicode Pattern_White_Space characters are ignored
         * @exception java.lang.ArgumentException if the pattern
         * contains a syntax error.
         * @stable ICU 2.0
         */
        public UnicodeSet ApplyPattern(string pattern, bool ignoreWhitespace)
        {
            CheckFrozen();
            return ApplyPattern(pattern, null, null, ignoreWhitespace ? IGNORE_SPACE : 0);
        }

        /**
         * Modifies this set to represent the set specified by the given pattern,
         * optionally ignoring whitespace.
         * See the class description for the syntax of the pattern language.
         * @param pattern a string specifying what characters are in the set
         * @param options a bitmask indicating which options to apply.
         * Valid options are IGNORE_SPACE and CASE.
         * @exception java.lang.ArgumentException if the pattern
         * contains a syntax error.
         * @stable ICU 3.8
         */
        public UnicodeSet ApplyPattern(String pattern, int options)
        {
            CheckFrozen();
            return ApplyPattern(pattern, null, null, options);
        }

        /**
         * Return true if the given position, in the given pattern, appears
         * to be the start of a UnicodeSet pattern.
         * @stable ICU 2.0
         */
        public static bool ResemblesPattern(String pattern, int pos)
        {
            return ((pos + 1) < pattern.Length &&
                    pattern[pos] == '[') ||
                    ResemblesPropertyPattern(pattern, pos);
        }

        /**
         * TODO: create Appendable version of UTF16.append(buf, c),
         * maybe in new class Appendables?
         * @throws IOException
         */
        private static void AppendCodePoint(StringBuilder app, int c)
        {
            Debug.Assert(0 <= c && c <= 0x10ffff);
            try
            {
                if (c <= 0xffff)
                {
                    app.Append((char)c);
                }
                else
                {
                    app.Append(UTF16.GetLeadSurrogate(c)).Append(UTF16.GetTrailSurrogate(c));
                }
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        /**
         * TODO: create class Appendables?
         * @throws IOException
         */
        private static void Append(StringBuilder app, ICharSequence s)
        {
            try
            {
                app.Append(s);
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        /**
         * Append the <code>toPattern()</code> representation of a
         * string to the given <code>Appendable</code>.
         */
        private static StringBuilder AppendToPat(StringBuilder buf, string s, bool escapeUnprintable)
        {
            int cp;
            for (int i = 0; i < s.Length; i += Character.CharCount(cp))
            {
                cp = s.CodePointAt(i);
                AppendToPat(buf, cp, escapeUnprintable);
            }
            return buf;
        }

        /**
         * Append the <code>toPattern()</code> representation of a
         * character to the given <code>Appendable</code>.
         */
        private static StringBuilder AppendToPat(StringBuilder buf, int c, bool escapeUnprintable)
        {
            try
            {
                if (escapeUnprintable && Utility.IsUnprintable(c))
                {
                    // Use hex escape notation (<backslash>uxxxx or <backslash>Uxxxxxxxx) for anything
                    // unprintable
                    if (Utility.EscapeUnprintable(buf, c))
                    {
                        return buf;
                    }
                }
                // Okay to let ':' pass through
                switch (c)
                {
                    case '[': // SET_OPEN:
                    case ']': // SET_CLOSE:
                    case '-': // HYPHEN:
                    case '^': // COMPLEMENT:
                    case '&': // INTERSECTION:
                    case '\\': //BACKSLASH:
                    case '{':
                    case '}':
                    case '$':
                    case ':':
                        buf.Append('\\');
                        break;
                    default:
                        // Escape whitespace
                        if (PatternProps.IsWhiteSpace(c))
                        {
                            buf.Append('\\');
                        }
                        break;
                }
                AppendCodePoint(buf, c);
                return buf;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        /**
         * Returns a string representation of this set.  If the result of
         * calling this function is passed to a UnicodeSet constructor, it
         * will produce another set that is equal to this one.
         * @stable ICU 2.0
         */
        public override string ToPattern(bool escapeUnprintable)
        {
            if (pat != null && !escapeUnprintable)
            {
                return pat;
            }
            StringBuilder result = new StringBuilder();
            return ToPattern(result, escapeUnprintable).ToString();
        }

        /**
         * Append a string representation of this set to result.  This will be
         * a cleaned version of the string passed to applyPattern(), if there
         * is one.  Otherwise it will be generated.
         */
        private StringBuilder ToPattern(StringBuilder result,
                bool escapeUnprintable)
        {
            if (pat == null)
            {
                return AppendNewPattern(result, escapeUnprintable, true);
            }
            try
            {
                if (!escapeUnprintable)
                {
                    result.Append(pat);
                    return result;
                }
                bool oddNumberOfBackslashes = false;
                for (int i = 0; i < pat.Length;)
                {
                    int c = pat.CodePointAt(i);
                    i += Character.CharCount(c);
                    if (Utility.IsUnprintable(c))
                    {
                        // If the unprintable character is preceded by an odd
                        // number of backslashes, then it has been escaped
                        // and we omit the last backslash.
                        Utility.EscapeUnprintable(result, c);
                        oddNumberOfBackslashes = false;
                    }
                    else if (!oddNumberOfBackslashes && c == '\\')
                    {
                        // Temporarily withhold an odd-numbered backslash.
                        oddNumberOfBackslashes = true;
                    }
                    else
                    {
                        if (oddNumberOfBackslashes)
                        {
                            result.Append('\\');
                        }
                        AppendCodePoint(result, c);
                        oddNumberOfBackslashes = false;
                    }
                }
                if (oddNumberOfBackslashes)
                {
                    result.Append('\\');
                }
                return result;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        /**
         * Generate and append a string representation of this set to result.
         * This does not use this.pat, the cleaned up copy of the string
         * passed to applyPattern().
         * @param result the buffer into which to generate the pattern
         * @param escapeUnprintable escape unprintable characters if true
         * @stable ICU 2.0
         */
        public StringBuilder GeneratePattern(StringBuilder result, bool escapeUnprintable)
        {
            return GeneratePattern(result, escapeUnprintable, true);
        }

        /**
         * Generate and append a string representation of this set to result.
         * This does not use this.pat, the cleaned up copy of the string
         * passed to applyPattern().
         * @param includeStrings if false, doesn't include the strings.
         * @stable ICU 3.8
         */
        public StringBuilder GeneratePattern(StringBuilder result,
                bool escapeUnprintable, bool includeStrings)
        {
            return AppendNewPattern(result, escapeUnprintable, includeStrings);
        }

        private StringBuilder AppendNewPattern(
                StringBuilder result, bool escapeUnprintable, bool includeStrings)
        {
            try
            {
                result.Append('[');

                int count = GetRangeCount();

                // If the set contains at least 2 intervals and includes both
                // MIN_VALUE and MAX_VALUE, then the inverse representation will
                // be more economical.
                if (count > 1 &&
                        GetRangeStart(0) == MIN_VALUE &&
                        GetRangeEnd(count - 1) == MAX_VALUE)
                {

                    // Emit the inverse
                    result.Append('^');

                    for (int i = 1; i < count; ++i)
                    {
                        int start = GetRangeEnd(i - 1) + 1;
                        int end = GetRangeStart(i) - 1;
                        AppendToPat(result, start, escapeUnprintable);
                        if (start != end)
                        {
                            if ((start + 1) != end)
                            {
                                result.Append('-');
                            }
                            AppendToPat(result, end, escapeUnprintable);
                        }
                    }
                }

                // Default; emit the ranges as pairs
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        int start = GetRangeStart(i);
                        int end = GetRangeEnd(i);
                        AppendToPat(result, start, escapeUnprintable);
                        if (start != end)
                        {
                            if ((start + 1) != end)
                            {
                                result.Append('-');
                            }
                            AppendToPat(result, end, escapeUnprintable);
                        }
                    }
                }

                if (includeStrings && strings.Count > 0)
                {
                    foreach (string s in strings)
                    {
                        result.Append('{');
                        AppendToPat(result, s, escapeUnprintable);
                        result.Append('}');
                    }
                }
                result.Append(']');
                return result;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        /**
         * Returns the number of elements in this set (its cardinality)
         * Note than the elements of a set may include both individual
         * codepoints and strings.
         *
         * @return the number of elements in this set (its cardinality).
         * @stable ICU 2.0
         */
        public int Count // ICU4N TODO: Not the best candidate for a property...
        {
            get
            {
                int n = 0;
                int count = GetRangeCount();
                for (int i = 0; i < count; ++i)
                {
                    n += GetRangeEnd(i) - GetRangeStart(i) + 1;
                }
                return n + strings.Count;
            }
        }

        /**
         * Returns <tt>true</tt> if this set contains no elements.
         *
         * @return <tt>true</tt> if this set contains no elements.
         * @stable ICU 2.0
         */
        public bool IsEmpty()
        {
            return len == 1 && strings.Count == 0;
        }

        /**
         * Implementation of UnicodeMatcher API.  Returns <tt>true</tt> if
         * this set contains any character whose low byte is the given
         * value.  This is used by <tt>RuleBasedTransliterator</tt> for
         * indexing.
         * @stable ICU 2.0
         */
        public override bool MatchesIndexValue(int v)
        {
            /* The index value v, in the range [0,255], is contained in this set if
             * it is contained in any pair of this set.  Pairs either have the high
             * bytes equal, or unequal.  If the high bytes are equal, then we have
             * aaxx..aayy, where aa is the high byte.  Then v is contained if xx <=
             * v <= yy.  If the high bytes are unequal we have aaxx..bbyy, bb>aa.
             * Then v is contained if xx <= v || v <= yy.  (This is identical to the
             * time zone month containment logic.)
             */
            for (int i = 0; i < GetRangeCount(); ++i)
            {
                int low = GetRangeStart(i);
                int high = GetRangeEnd(i);
                if ((low & ~0xFF) == (high & ~0xFF))
                {
                    if ((low & 0xFF) <= v && v <= (high & 0xFF))
                    {
                        return true;
                    }
                }
                else if ((low & 0xFF) <= v || v <= (high & 0xFF))
                {
                    return true;
                }
            }
            if (strings.Count != 0)
            {
                foreach (string s in strings)
                {
                    //if (s.Length() == 0) {
                    //    // Empty strings match everything
                    //    return true;
                    //}
                    // assert(s.Length() != 0); // We enforce this elsewhere
                    int c = UTF16.CharAt(s, 0);
                    if ((c & 0xFF) == v)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * Implementation of UnicodeMatcher.matches().  Always matches the
         * longest possible multichar string.
         * @stable ICU 2.0
         */
        public override int Matches(IReplaceable text,
                int[] offset,
                int limit,
                bool incremental)
        {

            if (offset[0] == limit)
            {
                // Strings, if any, have length != 0, so we don't worry
                // about them here.  If we ever allow zero-length strings
                // we much check for them here.
                if (Contains(UnicodeMatcher.ETHER))
                {
                    return incremental ? UnicodeMatcher.U_PARTIAL_MATCH : UnicodeMatcher.U_MATCH;
                }
                else
                {
                    return UnicodeMatcher.U_MISMATCH;
                }
            }
            else
            {
                if (strings.Count != 0)
                { // try strings first

                    // might separate forward and backward loops later
                    // for now they are combined

                    // TODO Improve efficiency of this, at least in the forward
                    // direction, if not in both.  In the forward direction we
                    // can assume the strings are sorted.

                    bool forward = offset[0] < limit;

                    // firstChar is the leftmost char to match in the
                    // forward direction or the rightmost char to match in
                    // the reverse direction.
                    char firstChar = text[offset[0]];

                    // If there are multiple strings that can match we
                    // return the longest match.
                    int highWaterLength = 0;

                    foreach (string trial in strings)
                    {
                        //if (trial.Length() == 0) {
                        //    return U_MATCH; // null-string always matches
                        //}
                        // assert(trial.Length() != 0); // We ensure this elsewhere

                        char c = trial[forward ? 0 : trial.Length - 1];

                        // Strings are sorted, so we can optimize in the
                        // forward direction.
                        if (forward && c > firstChar) break;
                        if (c != firstChar) continue;

                        int length = MatchRest(text, offset[0], limit, trial);

                        if (incremental)
                        {
                            int maxLen = forward ? limit - offset[0] : offset[0] - limit;
                            if (length == maxLen)
                            {
                                // We have successfully matched but only up to limit.
                                return UnicodeMatcher.U_PARTIAL_MATCH;
                            }
                        }

                        if (length == trial.Length)
                        {
                            // We have successfully matched the whole string.
                            if (length > highWaterLength)
                            {
                                highWaterLength = length;
                            }
                            // In the forward direction we know strings
                            // are sorted so we can bail early.
                            if (forward && length < highWaterLength)
                            {
                                break;
                            }
                            continue;
                        }
                    }

                    // We've checked all strings without a partial match.
                    // If we have full matches, return the longest one.
                    if (highWaterLength != 0)
                    {
                        offset[0] += forward ? highWaterLength : -highWaterLength;
                        return UnicodeMatcher.U_MATCH;
                    }
                }
                return base.Matches(text, offset, limit, incremental);
            }
        }

        /**
         * Returns the longest match for s in text at the given position.
         * If limit > start then match forward from start+1 to limit
         * matching all characters except s.charAt(0).  If limit &lt; start,
         * go backward starting from start-1 matching all characters
         * except s.charAt(s.Length()-1).  This method assumes that the
         * first character, text.charAt(start), matches s, so it does not
         * check it.
         * @param text the text to match
         * @param start the first character to match.  In the forward
         * direction, text.charAt(start) is matched against s.charAt(0).
         * In the reverse direction, it is matched against
         * s.charAt(s.Length()-1).
         * @param limit the limit offset for matching, either last+1 in
         * the forward direction, or last-1 in the reverse direction,
         * where last is the index of the last character to match.
         * @return If part of s matches up to the limit, return |limit -
         * start|.  If all of s matches before reaching the limit, return
         * s.Length().  If there is a mismatch between s and text, return
         * 0
         */
        private static int MatchRest(IReplaceable text, int start, int limit, string s)
        {
            int maxLen;
            int slen = s.Length;
            if (start < limit)
            {
                maxLen = limit - start;
                if (maxLen > slen) maxLen = slen;
                for (int i = 1; i < maxLen; ++i)
                {
                    if (text[start + i] != s[i]) return 0;
                }
            }
            else
            {
                maxLen = start - limit;
                if (maxLen > slen) maxLen = slen;
                --slen; // <=> slen = s.Length() - 1;
                for (int i = 1; i < maxLen; ++i)
                {
                    if (text[start - i] != s[slen - i]) return 0;
                }
            }
            return maxLen;
        }

        /// <summary>
        /// Tests whether the text matches at the offset. If so, returns the end of the longest substring that it matches. If not, returns -1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public virtual int MatchesAt(string text, int offset)
        {
            return MatchesAt(text.ToCharSequence(), offset);
        }

        /// <summary>
        /// Tests whether the text matches at the offset. If so, returns the end of the longest substring that it matches. If not, returns -1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public virtual int MatchesAt(StringBuilder text, int offset)
        {
            return MatchesAt(text.ToCharSequence(), offset);
        }

        /// <summary>
        /// Tests whether the text matches at the offset. If so, returns the end of the longest substring that it matches. If not, returns -1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public virtual int MatchesAt(char[] text, int offset)
        {
            return MatchesAt(text.ToCharSequence(), offset);
        }

        /// <summary>
        /// Tests whether the text matches at the offset. If so, returns the end of the longest substring that it matches. If not, returns -1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        internal virtual int MatchesAt(ICharSequence text, int offset)
        {
            int lastLen = -1;

            if (strings.Count != 0)
            {
                char firstChar = text[offset];
                string trial = null;
                // find the first string starting with firstChar
                //Iterator<string> it = strings.iterator();
                using (var it = strings.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        trial = it.Current;
                        char firstStringChar = trial[0];
                        if (firstStringChar < firstChar) continue;
                        if (firstStringChar > firstChar) goto strings_break;
                    }

                    // now keep checking string until we get the longest one
                    for (; ; )
                    {
                        int tempLen = MatchesAt(text, offset, trial.ToCharSequence());
                        if (lastLen > tempLen) goto strings_break;
                        lastLen = tempLen;
                        if (!it.MoveNext()) break;
                        trial = it.Current;
                    }
                }
            }
            strings_break: { }

            if (lastLen < 2)
            {
                int cp = UTF16.CharAt(text, offset);
                if (Contains(cp)) lastLen = UTF16.GetCharCount(cp);
            }

            return offset + lastLen;
        }

        /**
         * Does one string contain another, starting at a specific offset?
         * @param text text to match
         * @param offsetInText offset within that text
         * @param substring substring to match at offset in text
         * @return -1 if match fails, otherwise other.Length()
         */
        // Note: This method was moved from CollectionUtilities
        private static int MatchesAt(ICharSequence text, int offsetInText, ICharSequence substring)
        {
            int len = substring.Length;
            int textLength = text.Length;
            if (textLength + offsetInText > len)
            {
                return -1;
            }
            int i = 0;
            for (int j = offsetInText; i < len; ++i, ++j)
            {
                char pc = substring[i];
                char tc = text[j];
                if (pc != tc) return -1;
            }
            return i;
        }

        /**
         * Implementation of UnicodeMatcher API.  Union the set of all
         * characters that may be matched by this object into the given
         * set.
         * @param toUnionTo the set into which to union the source characters
         * @stable ICU 2.2
         */
        public override void AddMatchSetTo(UnicodeSet toUnionTo)
        {
            toUnionTo.AddAll(this);
        }

        /**
         * Returns the index of the given character within this set, where
         * the set is ordered by ascending code point.  If the character
         * is not in this set, return -1.  The inverse of this method is
         * <code>charAt()</code>.
         * @return an index from 0..size()-1, or -1
         * @stable ICU 2.0
         */
        public int IndexOf(int c)
        {
            if (c < MIN_VALUE || c > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }
            int i = 0;
            int n = 0;
            for (; ; )
            {
                int start = list[i++];
                if (c < start)
                {
                    return -1;
                }
                int limit = list[i++];
                if (c < limit)
                {
                    return n + c - start;
                }
                n += limit - start;
            }
        }

        /**
         * Returns the character at the given index within this set, where
         * the set is ordered by ascending code point.  If the index is
         * out of range, return -1.  The inverse of this method is
         * <code>indexOf()</code>.
         * @param index an index from 0..size()-1
         * @return the character at the given index, or -1.
         * @stable ICU 2.0
         */
        public int CharAt(int index)
        {
            if (index >= 0)
            {
                // len2 is the largest even integer <= len, that is, it is len
                // for even values and len-1 for odd values.  With odd values
                // the last entry is UNICODESET_HIGH.
                int len2 = len & ~1;
                for (int i = 0; i < len2;)
                {
                    int start = list[i++];
                    int count = list[i++] - start;
                    if (index < count)
                    {
                        return start + index;
                    }
                    index -= count;
                }
            }
            return -1;
        }

        /**
         * Adds the specified range to this set if it is not already
         * present.  If this set already contains the specified range,
         * the call leaves this set unchanged.  If <code>end &gt; start</code>
         * then an empty range is added, leaving the set unchanged.
         *
         * @param start first character, inclusive, of range to be added
         * to this set.
         * @param end last character, inclusive, of range to be added
         * to this set.
         * @stable ICU 2.0
         */
        public UnicodeSet Add(int start, int end)
        {
            CheckFrozen();
            return AddUnchecked(start, end);
        }

        /**
         * Adds all characters in range (uses preferred naming convention).
         * @param start The index of where to start on adding all characters.
         * @param end The index of where to end on adding all characters.
         * @return a reference to this object
         * @stable ICU 4.4
         */
        public UnicodeSet AddAll(int start, int end)
        {
            CheckFrozen();
            return AddUnchecked(start, end);
        }

        // for internal use, after checkFrozen has been called
        private UnicodeSet AddUnchecked(int start, int end)
        {
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start < end)
            {
                Add(Range(start, end), 2, 0);
            }
            else if (start == end)
            {
                Add(start);
            }
            return this;
        }

        //    /**
        //     * Format out the inversion list as a string, for debugging.  Uncomment when
        //     * needed.
        //     */
        //    public final String dump() {
        //        StringBuffer buf = new StringBuffer("[");
        //        for (int i=0; i<len; ++i) {
        //            if (i != 0) buf.append(", ");
        //            int c = list[i];
        //            //if (c <= 0x7F && c != '\n' && c != '\r' && c != '\t' && c != ' ') {
        //            //    buf.append((char) c);
        //            //} else {
        //                buf.append("U+").append(Utility.Hex(c, (c<0x10000)?4:6));
        //            //}
        //        }
        //        buf.append("]");
        //        return buf.toString();
        //    }

        /**
         * Adds the specified character to this set if it is not already
         * present.  If this set already contains the specified character,
         * the call leaves this set unchanged.
         * @stable ICU 2.0
         */
        public UnicodeSet Add(int c)
        {
            CheckFrozen();
            return AddUnchecked(c);
        }

        // for internal use only, after checkFrozen has been called
        private UnicodeSet AddUnchecked(int c)
        {
            if (c < MIN_VALUE || c > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }

            // find smallest i such that c < list[i]
            // if odd, then it is IN the set
            // if even, then it is OUT of the set
            int i = FindCodePoint(c);

            // already in set?
            if ((i & 1) != 0) return this;

            // HIGH is 0x110000
            // assert(list[len-1] == HIGH);

            // empty = [HIGH]
            // [start_0, limit_0, start_1, limit_1, HIGH]

            // [..., start_k-1, limit_k-1, start_k, limit_k, ..., HIGH]
            //                             ^
            //                             list[i]

            // i == 0 means c is before the first range
            // TODO: Is the "list[i]-1" a typo? Even if you pass MAX_VALUE into
            //      add_unchecked, the maximum value that "c" will be compared to
            //      is "MAX_VALUE-1" meaning that "if (c == MAX_VALUE)" will
            //      never be reached according to this logic.
            if (c == list[i] - 1)
            {
                // c is before start of next range
                list[i] = c;
                // if we touched the HIGH mark, then add a new one
                if (c == MAX_VALUE)
                {
                    EnsureCapacity(len + 1);
                    list[len++] = HIGH;
                }
                if (i > 0 && c == list[i - 1])
                {
                    // collapse adjacent ranges

                    // [..., start_k-1, c, c, limit_k, ..., HIGH]
                    //                     ^
                    //                     list[i]
                    System.Array.Copy(list, i + 1, list, i - 1, len - i - 1);
                    len -= 2;
                }
            }

            else if (i > 0 && c == list[i - 1])
            {
                // c is after end of prior range
                list[i - 1]++;
                // no need to chcek for collapse here
            }

            else
            {
                // At this point we know the new char is not adjacent to
                // any existing ranges, and it is not 10FFFF.


                // [..., start_k-1, limit_k-1, start_k, limit_k, ..., HIGH]
                //                             ^
                //                             list[i]

                // [..., start_k-1, limit_k-1, c, c+1, start_k, limit_k, ..., HIGH]
                //                             ^
                //                             list[i]

                // Don't use ensureCapacity() to save on copying.
                // NOTE: This has no measurable impact on performance,
                // but it might help in some usage patterns.
                if (len + 2 > list.Length)
                {
                    int[] temp = new int[len + 2 + GROW_EXTRA];
                    if (i != 0) System.Array.Copy(list, 0, temp, 0, i);
                    System.Array.Copy(list, i, temp, i + 2, len - i);
                    list = temp;
                }
                else
                {
                    System.Array.Copy(list, i, list, i + 2, len - i);
                }

                list[i] = c;
                list[i + 1] = c + 1;
                len += 2;
            }

            pat = null;
            return this;
        }

        /// <summary>
        /// Adds the specified multicharacter to this set if it is not already
        /// present.  If this set already contains the multicharacter,
        /// the call leaves this set unchanged.
        /// Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Add(string s)
        {
            return Add(s.ToCharSequence());
        }

        /// <summary>
        /// Adds the specified multicharacter to this set if it is not already
        /// present.  If this set already contains the multicharacter,
        /// the call leaves this set unchanged.
        /// Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Add(StringBuilder s)
        {
            return Add(s.ToCharSequence());
        }

        /// <summary>
        /// Adds the specified multicharacter to this set if it is not already
        /// present.  If this set already contains the multicharacter,
        /// the call leaves this set unchanged.
        /// Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Add(char[] s)
        {
            return Add(s.ToCharSequence());
        }

        /// <summary>
        /// Adds the specified multicharacter to this set if it is not already
        /// present.  If this set already contains the multicharacter,
        /// the call leaves this set unchanged.
        /// Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Add(ICharSequence s)
        {
            CheckFrozen();
            int cp = GetSingleCP(s);
            if (cp < 0)
            {
                strings.Add(s.ToString());
                pat = null;
            }
            else
            {
                AddUnchecked(cp, cp);
            }
            return this;
        }

        /**
         * Utility for getting code point from single code point CharSequence.
         * See the public UTF16.getSingleCodePoint()
         * @return a code point IF the string consists of a single one.
         * otherwise returns -1.
         * @param s to test
         */
        private static int GetSingleCP(ICharSequence s)
        {
            if (s.Length < 1)
            {
                throw new ArgumentException("Can't use zero-length strings in UnicodeSet");
            }
            if (s.Length > 2) return -1;
            if (s.Length == 1) return s[0];

            // at this point, len = 2
            int cp = UTF16.CharAt(s, 0);
            if (cp > 0xFFFF)
            { // is surrogate pair
                return cp;
            }
            return -1;
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet AddAll(string s) // ICU4N TODO: Change to UnionWith?
        {
            return AddAll(s.ToCharSequence());
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet AddAll(StringBuilder s) // ICU4N TODO: Change to UnionWith?
        {
            return AddAll(s.ToCharSequence());
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet AddAll(char[] s) // ICU4N TODO: Change to UnionWith?
        {
            return AddAll(s.ToCharSequence());
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet AddAll(ICharSequence s) // ICU4N TODO: Change to UnionWith?
        {
            CheckFrozen();
            int cp;
            for (int i = 0; i < s.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(s, i);
                AddUnchecked(cp, cp);
            }
            return this;
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RetainAll(string s) // ICU4N TODO: Change to IntersectWith?
        {
            return RetainAll(s.ToCharSequence());
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RetainAll(StringBuilder s) // ICU4N TODO: Change to IntersectWith?
        {
            return RetainAll(s.ToCharSequence());
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RetainAll(char[] s) // ICU4N TODO: Change to IntersectWith?
        {
            return RetainAll(s.ToCharSequence());
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet RetainAll(ICharSequence s) // ICU4N TODO: Change to IntersectWith?
        {
            return RetainAll(FromAll(s));
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet ComplementAll(string s)
        {
            return ComplementAll(s.ToCharSequence());
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet ComplementAll(StringBuilder s)
        {
            return ComplementAll(s.ToCharSequence());
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet ComplementAll(char[] s)
        {
            return ComplementAll(s.ToCharSequence());
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet ComplementAll(ICharSequence s)
        {
            return ComplementAll(FromAll(s));
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RemoveAll(string s)  // ICU4N TODO: Change to ExceptWith?
        {
            return RemoveAll(s.ToCharSequence());
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RemoveAll(StringBuilder s)  // ICU4N TODO: Change to ExceptWith?
        {
            return RemoveAll(s.ToCharSequence());
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet RemoveAll(char[] s)  // ICU4N TODO: Change to ExceptWith?
        {
            return RemoveAll(s.ToCharSequence());
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet RemoveAll(ICharSequence s)  // ICU4N TODO: Change to ExceptWith?
        {
            return RemoveAll(FromAll(s));
        }

        /// <summary>
        /// Remove all strings from this UnicodeSet
        /// </summary>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 4.2</stable>
        public UnicodeSet RemoveAllStrings()
        {
            CheckFrozen();
            if (strings.Count != 0)
            {
                strings.Clear();
                pat = null;
            }
            return this;
        }

        /// <summary>
        /// Makes a set from a multicharacter string. Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given string.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet From(string s)
        {
            return From(s.ToCharSequence());
        }

        /// <summary>
        /// Makes a set from a multicharacter string. Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given string.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet From(StringBuilder s)
        {
            return From(s.ToCharSequence());
        }

        /// <summary>
        /// Makes a set from a multicharacter string. Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given string.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet From(char[] s)
        {
            return From(s.ToCharSequence());
        }

        /// <summary>
        /// Makes a set from a multicharacter string. Thus "ch" =&gt; {"ch"}
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given string.</returns>
        /// <stable>ICU 2.0</stable>
        internal static UnicodeSet From(ICharSequence s)
        {
            return new UnicodeSet().Add(s);
        }

        /// <summary>
        /// Makes a set from each of the characters in the string. Thus "ch" =&gt; {"c", "h"}
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given characters.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet FromAll(string s)
        {
            return new UnicodeSet().AddAll(s);
        }

        /// <summary>
        /// Makes a set from each of the characters in the string. Thus "ch" =&gt; {"c", "h"}
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given characters.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet FromAll(StringBuilder s)
        {
            return new UnicodeSet().AddAll(s);
        }

        /// <summary>
        /// Makes a set from each of the characters in the string. Thus "ch" =&gt; {"c", "h"}
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given characters.</returns>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet FromAll(char[] s)
        {
            return new UnicodeSet().AddAll(s);
        }

        /// <summary>
        /// Makes a set from each of the characters in the string. Thus "ch" =&gt; {"c", "h"}
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>A newly created set containing the given characters.</returns>
        /// <stable>ICU 2.0</stable>
        internal static UnicodeSet FromAll(ICharSequence s)
        {
            return new UnicodeSet().AddAll(s);
        }

        /**
         * Retain only the elements in this set that are contained in the
         * specified range.  If <code>end &gt; start</code> then an empty range is
         * retained, leaving the set empty.
         *
         * @param start first character, inclusive, of range to be retained
         * to this set.
         * @param end last character, inclusive, of range to be retained
         * to this set.
         * @stable ICU 2.0
         */
        public UnicodeSet Retain(int start, int end)
        {
            CheckFrozen();
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Retain(Range(start, end), 2, 0);
            }
            else
            {
                Clear();
            }
            return this;
        }

        /**
         * Retain the specified character from this set if it is present.
         * Upon return this set will be empty if it did not contain c, or
         * will only contain c if it did contain c.
         * @param c the character to be retained
         * @return this object, for chaining
         * @stable ICU 2.0
         */
        public UnicodeSet Retain(int c)
        {
            return Retain(c, c);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Retain(string cs)
        {
            return Retain(cs.ToCharSequence());
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Retain(StringBuilder cs)
        {
            return Retain(cs.ToCharSequence());
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Retain(char[] cs)
        {
            return Retain(cs.ToCharSequence());
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Retain(ICharSequence cs)
        {
            int cp = GetSingleCP(cs);
            if (cp < 0)
            {
                string s = cs.ToString();
                bool isIn = strings.Contains(s);
                if (isIn && Count == 1)
                {
                    return this;
                }
                Clear();
                strings.Add(s);
                pat = null;
            }
            else
            {
                Retain(cp, cp);
            }
            return this;
        }

        /**
         * Removes the specified range from this set if it is present.
         * The set will not contain the specified range once the call
         * returns.  If <code>end &gt; start</code> then an empty range is
         * removed, leaving the set unchanged.
         *
         * @param start first character, inclusive, of range to be removed
         * from this set.
         * @param end last character, inclusive, of range to be removed
         * from this set.
         * @stable ICU 2.0
         */
        public UnicodeSet Remove(int start, int end)
        {
            CheckFrozen();
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Retain(Range(start, end), 2, 2);
            }
            return this;
        }

        /**
         * Removes the specified character from this set if it is present.
         * The set will not contain the specified character once the call
         * returns.
         * @param c the character to be removed
         * @return this object, for chaining
         * @stable ICU 2.0
         */
        public UnicodeSet Remove(int c)
        {
            return Remove(c, c);
        }

        /// <summary>
        /// Removes the specified string from this set if it is present.
        /// The set will not contain the specified string once the call
        /// returns.
        /// </summary>
        /// <param name="s">The string to be removed.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Remove(string s)
        {
            return Remove(s.ToCharSequence());
        }

        /// <summary>
        /// Removes the specified string from this set if it is present.
        /// The set will not contain the specified string once the call
        /// returns.
        /// </summary>
        /// <param name="s">The string to be removed.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Remove(StringBuilder s)
        {
            return Remove(s.ToCharSequence());
        }

        /// <summary>
        /// Removes the specified string from this set if it is present.
        /// The set will not contain the specified string once the call
        /// returns.
        /// </summary>
        /// <param name="s">The string to be removed.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Remove(char[] s)
        {
            return Remove(s.ToCharSequence());
        }

        /// <summary>
        /// Removes the specified string from this set if it is present.
        /// The set will not contain the specified string once the call
        /// returns.
        /// </summary>
        /// <param name="s">The string to be removed.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Remove(ICharSequence s)
        {
            int cp = GetSingleCP(s);
            if (cp < 0)
            {
                strings.Remove(s.ToString());
                pat = null;
            }
            else
            {
                Remove(cp, cp);
            }
            return this;
        }

        /**
         * Complements the specified range in this set.  Any character in
         * the range will be removed if it is in this set, or will be
         * added if it is not in this set.  If <code>end &gt; start</code>
         * then an empty range is complemented, leaving the set unchanged.
         *
         * @param start first character, inclusive, of range to be removed
         * from this set.
         * @param end last character, inclusive, of range to be removed
         * from this set.
         * @stable ICU 2.0
         */
        public UnicodeSet Complement(int start, int end)
        {
            CheckFrozen();
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Xor(Range(start, end), 2, 0);
            }
            pat = null;
            return this;
        }

        /**
         * Complements the specified character in this set.  The character
         * will be removed if it is in this set, or will be added if it is
         * not in this set.
         * @stable ICU 2.0
         */
        public UnicodeSet Complement(int c)
        {
            return Complement(c, c);
        }

        /**
         * This is equivalent to
         * <code>complement(MIN_VALUE, MAX_VALUE)</code>.
         * @stable ICU 2.0
         */
        public UnicodeSet Complement()
        {
            CheckFrozen();
            if (list[0] == LOW)
            {
                System.Array.Copy(list, 1, list, 0, len - 1);
                --len;
            }
            else
            {
                EnsureCapacity(len + 1);
                System.Array.Copy(list, 0, list, 1, len);
                list[0] = LOW;
                ++len;
            }
            pat = null;
            return this;
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Complement(string s)
        {
            return Complement(s.ToCharSequence());
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Complement(StringBuilder s)
        {
            return Complement(s.ToCharSequence());
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Complement(char[] s)
        {
            return Complement(s.ToCharSequence());
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Complement(ICharSequence s)
        {
            CheckFrozen();
            int cp = GetSingleCP(s);
            if (cp < 0)
            {
                string s2 = s.ToString();
                if (strings.Contains(s2))
                {
                    strings.Remove(s2);
                }
                else
                {
                    strings.Add(s2);
                }
                pat = null;
            }
            else
            {
                Complement(cp, cp);
            }
            return this;
        }

        /**
         * Returns true if this set contains the given character.
         * @param c character to be checked for containment
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public override bool Contains(int c)
        {
            if (c < MIN_VALUE || c > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }
            if (bmpSet != null)
            {
                return bmpSet.Contains(c);
            }
            if (stringSpan != null)
            {
                return stringSpan.Contains(c);
            }

            /*
            // Set i to the index of the start item greater than ch
            // We know we will terminate without length test!
            int i = -1;
            while (true) {
                if (c < list[++i]) break;
            }
             */

            int i = FindCodePoint(c);

            return ((i & 1) != 0); // return true if odd
        }

        /**
         * Returns the smallest value i such that c < list[i].  Caller
         * must ensure that c is a legal value or this method will enter
         * an infinite loop.  This method performs a binary search.
         * @param c a character in the range MIN_VALUE..MAX_VALUE
         * inclusive
         * @return the smallest integer i in the range 0..len-1,
         * inclusive, such that c < list[i]
         */
        private int FindCodePoint(int c)
        {
            /* Examples:
                                               findCodePoint(c)
               set              list[]         c=0 1 3 4 7 8
               ===              ==============   ===========
               []               [110000]         0 0 0 0 0 0
               [\u0000-\u0003]  [0, 4, 110000]   1 1 1 2 2 2
               [\u0004-\u0007]  [4, 8, 110000]   0 0 0 1 1 2
               [:all:]          [0, 110000]      1 1 1 1 1 1
             */

            // Return the smallest i such that c < list[i].  Assume
            // list[len - 1] == HIGH and that c is legal (0..HIGH-1).
            if (c < list[0]) return 0;
            // High runner test.  c is often after the last range, so an
            // initial check for this condition pays off.
            if (len >= 2 && c >= list[len - 2]) return len - 1;
            int lo = 0;
            int hi = len - 1;
            // invariant: c >= list[lo]
            // invariant: c < list[hi]
            for (; ; )
            {
                int i = (int)((uint)(lo + hi) >> 1);
                if (i == lo) return hi;
                if (c < list[i])
                {
                    hi = i;
                }
                else
                {
                    lo = i;
                }
            }
        }

        //    //----------------------------------------------------------------
        //    // Unrolled binary search
        //    //----------------------------------------------------------------
        //
        //    private int validLen = -1; // validated value of len
        //    private int topOfLow;
        //    private int topOfHigh;
        //    private int power;
        //    private int deltaStart;
        //
        //    private void validate() {
        //        if (len <= 1) {
        //            throw new ArgumentException("list.len==" + len + "; must be >1");
        //        }
        //
        //        // find greatest power of 2 less than or equal to len
        //        for (power = exp2.Length-1; power > 0 && exp2[power] > len; power--) {}
        //
        //        // assert(exp2[power] <= len);
        //
        //        // determine the starting points
        //        topOfLow = exp2[power] - 1;
        //        topOfHigh = len - 1;
        //        deltaStart = exp2[power-1];
        //        validLen = len;
        //    }
        //
        //    private static final int exp2[] = {
        //        0x1, 0x2, 0x4, 0x8,
        //        0x10, 0x20, 0x40, 0x80,
        //        0x100, 0x200, 0x400, 0x800,
        //        0x1000, 0x2000, 0x4000, 0x8000,
        //        0x10000, 0x20000, 0x40000, 0x80000,
        //        0x100000, 0x200000, 0x400000, 0x800000,
        //        0x1000000, 0x2000000, 0x4000000, 0x8000000,
        //        0x10000000, 0x20000000 // , 0x40000000 // no unsigned int in Java
        //    };
        //
        //    /**
        //     * Unrolled lowest index GT.
        //     */
        //    private final int leastIndexGT(int searchValue) {
        //
        //        if (len != validLen) {
        //            if (len == 1) return 0;
        //            validate();
        //        }
        //        int temp;
        //
        //        // set up initial range to search. Each subrange is a power of two in length
        //        int high = searchValue < list[topOfLow] ? topOfLow : topOfHigh;
        //
        //        // Completely unrolled binary search, folhighing "Programming Pearls"
        //        // Each case deliberately falls through to the next
        //        // Logically, list[-1] < all_search_values && list[count] > all_search_values
        //        // although the values -1 and count are never actually touched.
        //
        //        // The bounds at each point are low & high,
        //        // where low == high - delta*2
        //        // so high - delta is the midpoint
        //
        //        // The invariant AFTER each line is that list[low] < searchValue <= list[high]
        //
        //        switch (power) {
        //        //case 31: if (searchValue < list[temp = high-0x40000000]) high = temp; // no unsigned int in Java
        //        case 30: if (searchValue < list[temp = high-0x20000000]) high = temp;
        //        case 29: if (searchValue < list[temp = high-0x10000000]) high = temp;
        //
        //        case 28: if (searchValue < list[temp = high- 0x8000000]) high = temp;
        //        case 27: if (searchValue < list[temp = high- 0x4000000]) high = temp;
        //        case 26: if (searchValue < list[temp = high- 0x2000000]) high = temp;
        //        case 25: if (searchValue < list[temp = high- 0x1000000]) high = temp;
        //
        //        case 24: if (searchValue < list[temp = high-  0x800000]) high = temp;
        //        case 23: if (searchValue < list[temp = high-  0x400000]) high = temp;
        //        case 22: if (searchValue < list[temp = high-  0x200000]) high = temp;
        //        case 21: if (searchValue < list[temp = high-  0x100000]) high = temp;
        //
        //        case 20: if (searchValue < list[temp = high-   0x80000]) high = temp;
        //        case 19: if (searchValue < list[temp = high-   0x40000]) high = temp;
        //        case 18: if (searchValue < list[temp = high-   0x20000]) high = temp;
        //        case 17: if (searchValue < list[temp = high-   0x10000]) high = temp;
        //
        //        case 16: if (searchValue < list[temp = high-    0x8000]) high = temp;
        //        case 15: if (searchValue < list[temp = high-    0x4000]) high = temp;
        //        case 14: if (searchValue < list[temp = high-    0x2000]) high = temp;
        //        case 13: if (searchValue < list[temp = high-    0x1000]) high = temp;
        //
        //        case 12: if (searchValue < list[temp = high-     0x800]) high = temp;
        //        case 11: if (searchValue < list[temp = high-     0x400]) high = temp;
        //        case 10: if (searchValue < list[temp = high-     0x200]) high = temp;
        //        case  9: if (searchValue < list[temp = high-     0x100]) high = temp;
        //
        //        case  8: if (searchValue < list[temp = high-      0x80]) high = temp;
        //        case  7: if (searchValue < list[temp = high-      0x40]) high = temp;
        //        case  6: if (searchValue < list[temp = high-      0x20]) high = temp;
        //        case  5: if (searchValue < list[temp = high-      0x10]) high = temp;
        //
        //        case  4: if (searchValue < list[temp = high-       0x8]) high = temp;
        //        case  3: if (searchValue < list[temp = high-       0x4]) high = temp;
        //        case  2: if (searchValue < list[temp = high-       0x2]) high = temp;
        //        case  1: if (searchValue < list[temp = high-       0x1]) high = temp;
        //        }
        //
        //        return high;
        //    }
        //
        //    // For debugging only
        //    public int len() {
        //        return len;
        //    }
        //
        //    //----------------------------------------------------------------
        //    //----------------------------------------------------------------

        /**
         * Returns true if this set contains every character
         * of the given range.
         * @param start first character, inclusive, of the range
         * @param end last character, inclusive, of the range
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public bool Contains(int start, int end)
        {
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            //int i = -1;
            //while (true) {
            //    if (start < list[++i]) break;
            //}
            int i = FindCodePoint(start);
            return ((i & 1) != 0 && end < list[i]);
        }

        /// <summary>
        /// Returns <tt>true</tt> if this set contains the given
        /// multicharacter string.
        /// </summary>
        /// <param name="s">String to be checked for containment.</param>
        /// <returns><tt>true</tt> if this set contains the specified string.</returns>
        /// <stable>ICU 2.0</stable>
        public bool Contains(string s)
        {
            return Contains(s.ToCharSequence());
        }

        /// <summary>
        /// Returns <tt>true</tt> if this set contains the given
        /// multicharacter string.
        /// </summary>
        /// <param name="s">String to be checked for containment.</param>
        /// <returns><tt>true</tt> if this set contains the specified string.</returns>
        /// <stable>ICU 2.0</stable>
        public bool Contains(StringBuilder s)
        {
            return Contains(s.ToCharSequence());
        }

        /// <summary>
        /// Returns <tt>true</tt> if this set contains the given
        /// multicharacter string.
        /// </summary>
        /// <param name="s">String to be checked for containment.</param>
        /// <returns><tt>true</tt> if this set contains the specified string.</returns>
        /// <stable>ICU 2.0</stable>
        public bool Contains(char[] s)
        {
            return Contains(s.ToCharSequence());
        }

        /// <summary>
        /// Returns <tt>true</tt> if this set contains the given
        /// multicharacter string.
        /// </summary>
        /// <param name="s">String to be checked for containment.</param>
        /// <returns><tt>true</tt> if this set contains the specified string.</returns>
        /// <stable>ICU 2.0</stable>
        internal bool Contains(ICharSequence s)
        {
            int cp = GetSingleCP(s);
            if (cp < 0)
            {
                return strings.Contains(s.ToString());
            }
            else
            {
                return Contains(cp);
            }
        }

        /**
         * Returns true if this set contains all the characters and strings
         * of the given set.
         * @param b set to be checked for containment
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public bool ContainsAll(UnicodeSet b) // ICU4N TODO: API change to IsSupersetOf ?
        {
            // The specified set is a subset if all of its pairs are contained in
            // this set. This implementation accesses the lists directly for speed.
            // TODO: this could be faster if size() were cached. But that would affect building speed
            // so it needs investigation.
            int[] listB = b.list;
            bool needA = true;
            bool needB = true;
            int aPtr = 0;
            int bPtr = 0;
            int aLen = len - 1;
            int bLen = b.len - 1;
            int startA = 0, startB = 0, limitA = 0, limitB = 0;
            while (true)
            {
                // double iterations are such a pain...
                if (needA)
                {
                    if (aPtr >= aLen)
                    {
                        // ran out of A. If B is also exhausted, then break;
                        if (needB && bPtr >= bLen)
                        {
                            break;
                        }
                        return false;
                    }
                    startA = list[aPtr++];
                    limitA = list[aPtr++];
                }
                if (needB)
                {
                    if (bPtr >= bLen)
                    {
                        // ran out of B. Since we got this far, we have an A and we are ok so far
                        break;
                    }
                    startB = listB[bPtr++];
                    limitB = listB[bPtr++];
                }
                // if B doesn't overlap and is greater than A, get new A
                if (startB >= limitA)
                {
                    needA = true;
                    needB = false;
                    continue;
                }
                // if B is wholy contained in A, then get a new B
                if (startB >= startA && limitB <= limitA)
                {
                    needA = false;
                    needB = true;
                    continue;
                }
                // all other combinations mean we fail
                return false;
            }

            if (!strings.IsSupersetOf(b.strings)) return false;
            return true;
        }

        //    /**
        //     * Returns true if this set contains all the characters and strings
        //     * of the given set.
        //     * @param c set to be checked for containment
        //     * @return true if the test condition is met
        //     * @stable ICU 2.0
        //     */
        //    public bool containsAllOld(UnicodeSet c) {
        //        // The specified set is a subset if all of its pairs are contained in
        //        // this set.  It's possible to code this more efficiently in terms of
        //        // direct manipulation of the inversion lists if the need arises.
        //        int n = c.getRangeCount();
        //        for (int i=0; i<n; ++i) {
        //            if (!contains(c.getRangeStart(i), c.getRangeEnd(i))) {
        //                return false;
        //            }
        //        }
        //        if (!strings.containsAll(c.strings)) return false;
        //        return true;
        //    }

        /**
         * Returns true if there is a partition of the string such that this set contains each of the partitioned strings.
         * For example, for the Unicode set [a{bc}{cd}]<br>
         * containsAll is true for each of: "a", "bc", ""cdbca"<br>
         * containsAll is false for each of: "acb", "bcda", "bcx"<br>
         * @param s string containing characters to be checked for containment
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public bool ContainsAll(string s)
        {
            int cp;
            for (int i = 0; i < s.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(s, i);
                if (!Contains(cp))
                {
                    if (strings.Count == 0)
                    {
                        return false;
                    }
                    return ContainsAll(s, 0);
                }
            }
            return true;
        }

        /**
         * Recursive routine called if we fail to find a match in containsAll, and there are strings
         * @param s source string
         * @param i point to match to the end on
         * @return true if ok
         */
        private bool ContainsAll(string s, int i)
        {
            if (i >= s.Length)
            {
                return true;
            }
            int cp = UTF16.CharAt(s, i);
            if (Contains(cp) && ContainsAll(s, i + UTF16.GetCharCount(cp)))
            {
                return true;
            }
            foreach (string setStr in strings)
            {
                if (s.Substring(i).StartsWith(setStr, StringComparison.Ordinal) && ContainsAll(s, i + setStr.Length))
                {
                    return true;
                }
            }
            return false;

        }

        /**
         * Get the Regex equivalent for this UnicodeSet
         * @return regex pattern equivalent to this UnicodeSet
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //@Deprecated
        public string GetRegexEquivalent()
        {
            if (strings.Count == 0)
            {
                return ToString();
            }
            StringBuilder result = new StringBuilder("(?:");
            AppendNewPattern(result, true, false);
            foreach (string s in strings)
            {
                result.Append('|');
                AppendToPat(result, s, true);
            }
            return result.Append(")").ToString();
        }

        /**
         * Returns true if this set contains none of the characters
         * of the given range.
         * @param start first character, inclusive, of the range
         * @param end last character, inclusive, of the range
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public bool ContainsNone(int start, int end)
        {
            if (start < MIN_VALUE || start > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MIN_VALUE || end > MAX_VALUE)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            int i = -1;
            while (true)
            {
                if (start < list[++i]) break;
            }
            return ((i & 1) == 0 && end < list[i]);
        }

        /**
         * Returns true if none of the characters or strings in this UnicodeSet appears in the string.
         * For example, for the Unicode set [a{bc}{cd}]<br>
         * containsNone is true for: "xy", "cb"<br>
         * containsNone is false for: "a", "bc", "bcd"<br>
         * @param b set to be checked for containment
         * @return true if the test condition is met
         * @stable ICU 2.0
         */
        public bool ContainsNone(UnicodeSet b)
        {
            // The specified set is a subset if some of its pairs overlap with some of this set's pairs.
            // This implementation accesses the lists directly for speed.
            int[] listB = b.list;
            bool needA = true;
            bool needB = true;
            int aPtr = 0;
            int bPtr = 0;
            int aLen = len - 1;
            int bLen = b.len - 1;
            int startA = 0, startB = 0, limitA = 0, limitB = 0;
            while (true)
            {
                // double iterations are such a pain...
                if (needA)
                {
                    if (aPtr >= aLen)
                    {
                        // ran out of A: break so we test strings
                        break;
                    }
                    startA = list[aPtr++];
                    limitA = list[aPtr++];
                }
                if (needB)
                {
                    if (bPtr >= bLen)
                    {
                        // ran out of B: break so we test strings
                        break;
                    }
                    startB = listB[bPtr++];
                    limitB = listB[bPtr++];
                }
                // if B is higher than any part of A, get new A
                if (startB >= limitA)
                {
                    needA = true;
                    needB = false;
                    continue;
                }
                // if A is higher than any part of B, get new B
                if (startA >= limitB)
                {
                    needA = false;
                    needB = true;
                    continue;
                }
                // all other combinations mean we fail
                return false;
            }

            if (!SortedSetRelation.HasRelation(strings, SortedSetRelation.DISJOINT, b.strings)) return false;
            return true;
        }

        //    /**
        //     * Returns true if none of the characters or strings in this UnicodeSet appears in the string.
        //     * For example, for the Unicode set [a{bc}{cd}]<br>
        //     * containsNone is true for: "xy", "cb"<br>
        //     * containsNone is false for: "a", "bc", "bcd"<br>
        //     * @param c set to be checked for containment
        //     * @return true if the test condition is met
        //     * @stable ICU 2.0
        //     */
        //    public bool containsNoneOld(UnicodeSet c) {
        //        // The specified set is a subset if all of its pairs are contained in
        //        // this set.  It's possible to code this more efficiently in terms of
        //        // direct manipulation of the inversion lists if the need arises.
        //        int n = c.getRangeCount();
        //        for (int i=0; i<n; ++i) {
        //            if (!containsNone(c.getRangeStart(i), c.getRangeEnd(i))) {
        //                return false;
        //            }
        //        }
        //        if (!SortedSetRelation.hasRelation(strings, SortedSetRelation.DISJOINT, c.strings)) return false;
        //        return true;
        //    }

        /// <summary>
        /// Returns true if this set contains none of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool ContainsNone(string s)
        {
            return ContainsNone(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains none of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool ContainsNone(StringBuilder s)
        {
            return ContainsNone(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains none of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool ContainsNone(char[] s)
        {
            return ContainsNone(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains none of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal virtual bool ContainsNone(ICharSequence s)
        {
            return Span(s, SpanCondition.NOT_CONTAINED) == s.Length;
        }

        /**
         * Returns true if this set contains one or more of the characters
         * in the given range.
         * @param start first character, inclusive, of the range
         * @param end last character, inclusive, of the range
         * @return true if the condition is met
         * @stable ICU 2.0
         */
        public bool ContainsSome(int start, int end)
        {
            return !ContainsNone(start, end);
        }

        /**
         * Returns true if this set contains one or more of the characters
         * and strings of the given set.
         * @param s set to be checked for containment
         * @return true if the condition is met
         * @stable ICU 2.0
         */
        public bool ContainsSome(UnicodeSet s)
        {
            return !ContainsNone(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public bool ContainsSome(string s)
        {
            return ContainsSome(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public bool ContainsSome(StringBuilder s)
        {
            return ContainsSome(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public bool ContainsSome(char[] s)
        {
            return ContainsSome(s.ToCharSequence());
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal bool ContainsSome(ICharSequence s)
        {
            return !ContainsNone(s);
        }


        /**
         * Adds all of the elements in the specified set to this set if
         * they're not already present.  This operation effectively
         * modifies this set so that its value is the <i>union</i> of the two
         * sets.  The behavior of this operation is unspecified if the specified
         * collection is modified while the operation is in progress.
         *
         * @param c set whose elements are to be added to this set.
         * @stable ICU 2.0
         */
        public UnicodeSet AddAll(UnicodeSet c) // ICU4N TODO: API Rename UnionWith? Or maybe just make an additional function to satisfy the ISet<T> contract ?
        {
            CheckFrozen();
            Add(c.list, c.len, 0);
            strings.UnionWith(c.strings);
            return this;
        }

        /**
         * Retains only the elements in this set that are contained in the
         * specified set.  In other words, removes from this set all of
         * its elements that are not contained in the specified set.  This
         * operation effectively modifies this set so that its value is
         * the <i>intersection</i> of the two sets.
         *
         * @param c set that defines which elements this set will retain.
         * @stable ICU 2.0
         */
        public UnicodeSet RetainAll(UnicodeSet c)
        {
            CheckFrozen();
            Retain(c.list, c.len, 0);
            strings.IntersectWith(c.strings);
            return this;
        }

        /**
         * Removes from this set all of its elements that are contained in the
         * specified set.  This operation effectively modifies this
         * set so that its value is the <i>asymmetric set difference</i> of
         * the two sets.
         *
         * @param c set that defines which elements will be removed from
         *          this set.
         * @stable ICU 2.0
         */
        public UnicodeSet RemoveAll(UnicodeSet c) // ICU4N TODO: API rename ExceptWith or make extra ExceptWith method that calls this
        {
            CheckFrozen();
            Retain(c.list, c.len, 2);
            strings.ExceptWith(c.strings);
            return this;
        }

        /**
         * Complements in this set all elements contained in the specified
         * set.  Any character in the other set will be removed if it is
         * in this set, or will be added if it is not in this set.
         *
         * @param c set that defines which elements will be complemented from
         *          this set.
         * @stable ICU 2.0
         */
        public UnicodeSet ComplementAll(UnicodeSet c)
        {
            CheckFrozen();
            Xor(c.list, c.len, 0);
            SortedSetRelation.DoOperation(strings, SortedSetRelation.COMPLEMENTALL, c.strings);
            return this;
        }

        /**
         * Removes all of the elements from this set.  This set will be
         * empty after this call returns.
         * @stable ICU 2.0
         */
        public UnicodeSet Clear()
        {
            CheckFrozen();
            list[0] = HIGH;
            len = 1;
            pat = null;
            strings.Clear();
            return this;
        }

        /**
         * Iteration method that returns the number of ranges contained in
         * this set.
         * @see #getRangeStart
         * @see #getRangeEnd
         * @stable ICU 2.0
         */
        public int GetRangeCount()
        {
            return len / 2;
        }

        /**
         * Iteration method that returns the first character in the
         * specified range of this set.
         * @exception ArrayIndexOutOfBoundsException if index is outside
         * the range <code>0..getRangeCount()-1</code>
         * @see #getRangeCount
         * @see #getRangeEnd
         * @stable ICU 2.0
         */
        public int GetRangeStart(int index)
        {
            return list[index * 2];
        }

        /**
         * Iteration method that returns the last character in the
         * specified range of this set.
         * @exception ArrayIndexOutOfBoundsException if index is outside
         * the range <code>0..getRangeCount()-1</code>
         * @see #getRangeStart
         * @see #getRangeEnd
         * @stable ICU 2.0
         */
        public int GetRangeEnd(int index)
        {
            return (list[index * 2 + 1] - 1);
        }

        /**
         * Reallocate this objects internal structures to take up the least
         * possible space, without changing this object's value.
         * @stable ICU 2.0
         */
        public UnicodeSet Compact()
        {
            CheckFrozen();
            if (len != list.Length)
            {
                int[] temp = new int[len];
                System.Array.Copy(list, 0, temp, 0, len);
                list = temp;
            }
            rangeList = null;
            buffer = null;
            return this;
        }

        /**
         * Compares the specified object with this set for equality.  Returns
         * <tt>true</tt> if the specified object is also a set, the two sets
         * have the same size, and every member of the specified set is
         * contained in this set (or equivalently, every member of this set is
         * contained in the specified set).
         *
         * @param o Object to be compared for equality with this set.
         * @return <tt>true</tt> if the specified Object is equal to this set.
         * @stable ICU 2.0
         */
        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (this == o)
            {
                return true;
            }
            try
            {
                UnicodeSet that = (UnicodeSet)o;
                if (len != that.len) return false;
                for (int i = 0; i < len; ++i)
                {
                    if (list[i] != that.list[i]) return false;
                }
                if (!strings.Equals(that.strings)) return false;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /**
         * Returns the hash code value for this set.
         *
         * @return the hash code value for this set.
         * @see java.lang.Object#hashCode()
         * @stable ICU 2.0
         */
        public override int GetHashCode()
        {
            int result = len;
            for (int i = 0; i < len; ++i)
            {
                result *= 1000003;
                result += list[i];
            }
            return result;
        }

        /**
         * Return a programmer-readable string representation of this object.
         * @stable ICU 2.0
         */
        public override string ToString()
        {
            return ToPattern(true);
        }

        //----------------------------------------------------------------
        // Implementation: Pattern parsing
        //----------------------------------------------------------------

        /**
         * Parses the given pattern, starting at the given position.  The character
         * at pattern.charAt(pos.getIndex()) must be '[', or the parse fails.
         * Parsing continues until the corresponding closing ']'.  If a syntax error
         * is encountered between the opening and closing brace, the parse fails.
         * Upon return from a successful parse, the ParsePosition is updated to
         * point to the character following the closing ']', and an inversion
         * list for the parsed pattern is returned.  This method
         * calls itself recursively to parse embedded subpatterns.
         *
         * @param pattern the string containing the pattern to be parsed.  The
         * portion of the string from pos.getIndex(), which must be a '[', to the
         * corresponding closing ']', is parsed.
         * @param pos upon entry, the position at which to being parsing.  The
         * character at pattern.charAt(pos.getIndex()) must be a '['.  Upon return
         * from a successful parse, pos.getIndex() is either the character after the
         * closing ']' of the parsed pattern, or pattern.Length() if the closing ']'
         * is the last character of the pattern string.
         * @return an inversion list for the parsed substring
         * of <code>pattern</code>
         * @exception java.lang.ArgumentException if the parse fails.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //@Deprecated
        public UnicodeSet ApplyPattern(string pattern,
                ParsePosition pos,
                ISymbolTable symbols,
                int options)
        {

            // Need to build the pattern in a temporary string because
            // _applyPattern calls add() etc., which set pat to empty.
            bool parsePositionWasNull = pos == null;
            if (parsePositionWasNull)
            {
                pos = new ParsePosition(0);
            }

            StringBuilder rebuiltPat = new StringBuilder();
            RuleCharacterIterator chars =
                    new RuleCharacterIterator(pattern, symbols, pos);
            ApplyPattern(chars, symbols, rebuiltPat, options);
            if (chars.InVariable)
            {
                SyntaxError(chars, "Extra chars in variable value");
            }
            pat = rebuiltPat.ToString();
            if (parsePositionWasNull)
            {
                int i = pos.Index;

                // Skip over trailing whitespace
                if ((options & IGNORE_SPACE) != 0)
                {
                    i = PatternProps.SkipWhiteSpace(pattern, i);
                }

                if (i != pattern.Length)
                {
                    throw new ArgumentException("Parse of \"" + pattern +
                            "\" failed at " + i);
                }
            }
            return this;
        }

        // Add constants to make the applyPattern() code easier to follow.

        private const int LAST0_START = 0,
                LAST1_RANGE = 1,
                LAST2_SET = 2;

        private const int MODE0_NONE = 0,
                MODE1_INBRACKET = 1,
                MODE2_OUTBRACKET = 2;

        private const int SETMODE0_NONE = 0,
                SETMODE1_UNICODESET = 1,
                SETMODE2_PROPERTYPAT = 2,
                SETMODE3_PREPARSED = 3;

        /**
         * Parse the pattern from the given RuleCharacterIterator.  The
         * iterator is advanced over the parsed pattern.
         * @param chars iterator over the pattern characters.  Upon return
         * it will be advanced to the first character after the parsed
         * pattern, or the end of the iteration if all characters are
         * parsed.
         * @param symbols symbol table to use to parse and dereference
         * variables, or null if none.
         * @param rebuiltPat the pattern that was parsed, rebuilt or
         * copied from the input pattern, as appropriate.
         * @param options a bit mask of zero or more of the following:
         * IGNORE_SPACE, CASE.
         */
        private void ApplyPattern(RuleCharacterIterator chars, ISymbolTable symbols,
                StringBuilder rebuiltPat, int options)
        {

            // Syntax characters: [ ] ^ - & { }

            // Recognized special forms for chars, sets: c-c s-s s&s

            int opts = RuleCharacterIterator.PARSE_VARIABLES |
                    RuleCharacterIterator.PARSE_ESCAPES;
            if ((options & IGNORE_SPACE) != 0)
            {
                opts |= RuleCharacterIterator.SKIP_WHITESPACE;
            }

            StringBuilder patBuf = new StringBuilder(), buf = null;
            bool usePat = false;
            UnicodeSet scratch = null;
            Object backup = null;

            // mode: 0=before [, 1=between [...], 2=after ]
            // lastItem: 0=none, 1=char, 2=set
            int lastItem = LAST0_START, lastChar = 0, mode = MODE0_NONE;
            char op = (char)0;

            bool invert = false;

            Clear();
            String lastString = null;

            while (mode != MODE2_OUTBRACKET && !chars.AtEnd)
            {
                //Eclipse stated the following is "dead code"
                /*
                if (false) {
                    // Debugging assertion
                    if (!((lastItem == 0 && op == 0) ||
                            (lastItem == 1 && (op == 0 || op == '-')) ||
                            (lastItem == 2 && (op == 0 || op == '-' || op == '&')))) {
                        throw new ArgumentException();
                    }
                }*/

                int c = 0;
                bool literal = false;
                UnicodeSet nested = null;

                // -------- Check for property pattern

                // setMode: 0=none, 1=unicodeset, 2=propertypat, 3=preparsed
                int setMode = SETMODE0_NONE;
                if (ResemblesPropertyPattern(chars, opts))
                {
                    setMode = SETMODE2_PROPERTYPAT;
                }

                // -------- Parse '[' of opening delimiter OR nested set.
                // If there is a nested set, use `setMode' to define how
                // the set should be parsed.  If the '[' is part of the
                // opening delimiter for this pattern, parse special
                // strings "[", "[^", "[-", and "[^-".  Check for stand-in
                // characters representing a nested set in the symbol
                // table.

                else
                {
                    // Prepare to backup if necessary
                    backup = chars.GetPos(backup);
                    c = chars.Next(opts);
                    literal = chars.IsEscaped;

                    if (c == '[' && !literal)
                    {
                        if (mode == MODE1_INBRACKET)
                        {
                            chars.SetPos(backup); // backup
                            setMode = SETMODE1_UNICODESET;
                        }
                        else
                        {
                            // Handle opening '[' delimiter
                            mode = MODE1_INBRACKET;
                            patBuf.Append('[');
                            backup = chars.GetPos(backup); // prepare to backup
                            c = chars.Next(opts);
                            literal = chars.IsEscaped;
                            if (c == '^' && !literal)
                            {
                                invert = true;
                                patBuf.Append('^');
                                backup = chars.GetPos(backup); // prepare to backup
                                c = chars.Next(opts);
                                literal = chars.IsEscaped;
                            }
                            // Fall through to handle special leading '-';
                            // otherwise restart loop for nested [], \p{}, etc.
                            if (c == '-')
                            {
                                literal = true;
                                // Fall through to handle literal '-' below
                            }
                            else
                            {
                                chars.SetPos(backup); // backup
                                continue;
                            }
                        }
                    }
                    else if (symbols != null)
                    {
                        IUnicodeMatcher m = symbols.LookupMatcher(c); // may be null
                        if (m != null)
                        {
                            try
                            {
                                nested = (UnicodeSet)m;
                                setMode = SETMODE3_PREPARSED;
                            }
                            catch (InvalidCastException e)
                            {
                                SyntaxError(chars, "Syntax error");
                            }
                        }
                    }
                }

                // -------- Handle a nested set.  This either is inline in
                // the pattern or represented by a stand-in that has
                // previously been parsed and was looked up in the symbol
                // table.

                if (setMode != SETMODE0_NONE)
                {
                    if (lastItem == LAST1_RANGE)
                    {
                        if (op != 0)
                        {
                            SyntaxError(chars, "Char expected after operator");
                        }
                        AddUnchecked(lastChar, lastChar);
                        AppendToPat(patBuf, lastChar, false);
                        lastItem = LAST0_START;
                        op = (char)0;
                    }

                    if (op == '-' || op == '&')
                    {
                        patBuf.Append(op);
                    }

                    if (nested == null)
                    {
                        if (scratch == null) scratch = new UnicodeSet();
                        nested = scratch;
                    }
                    switch (setMode)
                    {
                        case SETMODE1_UNICODESET:
                            nested.ApplyPattern(chars, symbols, patBuf, options);
                            break;
                        case SETMODE2_PROPERTYPAT:
                            chars.SkipIgnored(opts);
                            nested.ApplyPropertyPattern(chars, patBuf, symbols);
                            break;
                        case SETMODE3_PREPARSED: // `nested' already parsed
                            nested.ToPattern(patBuf, false);
                            break;
                    }

                    usePat = true;

                    if (mode == MODE0_NONE)
                    {
                        // Entire pattern is a category; leave parse loop
                        Set(nested);
                        mode = MODE2_OUTBRACKET;
                        break;
                    }

                    switch (op)
                    {
                        case '-':
                            RemoveAll(nested);
                            break;
                        case '&':
                            RetainAll(nested);
                            break;
                        case (char)0:
                            AddAll(nested);
                            break;
                    }

                    op = (char)0;
                    lastItem = LAST2_SET;

                    continue;
                }

                if (mode == MODE0_NONE)
                {
                    SyntaxError(chars, "Missing '['");
                }

                // -------- Parse special (syntax) characters.  If the
                // current character is not special, or if it is escaped,
                // then fall through and handle it below.

                if (!literal)
                {
                    switch (c)
                    {
                        case ']':
                            if (lastItem == LAST1_RANGE)
                            {
                                AddUnchecked(lastChar, lastChar);
                                AppendToPat(patBuf, lastChar, false);
                            }
                            // Treat final trailing '-' as a literal
                            if (op == '-')
                            {
                                AddUnchecked(op, op);
                                patBuf.Append(op);
                            }
                            else if (op == '&')
                            {
                                SyntaxError(chars, "Trailing '&'");
                            }
                            patBuf.Append(']');
                            mode = MODE2_OUTBRACKET;
                            continue;
                        case '-':
                            if (op == 0)
                            {
                                if (lastItem != LAST0_START)
                                {
                                    op = (char)c;
                                    continue;
                                }
                                else if (lastString != null)
                                {
                                    op = (char)c;
                                    continue;
                                }
                                else
                                {
                                    // Treat final trailing '-' as a literal
                                    AddUnchecked(c, c);
                                    c = chars.Next(opts);
                                    literal = chars.IsEscaped;
                                    if (c == ']' && !literal)
                                    {
                                        patBuf.Append("-]");
                                        mode = MODE2_OUTBRACKET;
                                        continue;
                                    }
                                }
                            }
                            SyntaxError(chars, "'-' not after char, string, or set");
                            break;
                        case '&':
                            if (lastItem == LAST2_SET && op == 0)
                            {
                                op = (char)c;
                                continue;
                            }
                            SyntaxError(chars, "'&' not after set");
                            break;
                        case '^':
                            SyntaxError(chars, "'^' not after '['");
                            break;
                        case '{':
                            if (op != 0 && op != '-')
                            {
                                SyntaxError(chars, "Missing operand after operator");
                            }
                            if (lastItem == LAST1_RANGE)
                            {
                                AddUnchecked(lastChar, lastChar);
                                AppendToPat(patBuf, lastChar, false);
                            }
                            lastItem = LAST0_START;
                            if (buf == null)
                            {
                                buf = new StringBuilder();
                            }
                            else
                            {
                                buf.Length = 0;
                            }
                            bool ok = false;
                            while (!chars.AtEnd)
                            {
                                c = chars.Next(opts);
                                literal = chars.IsEscaped;
                                if (c == '}' && !literal)
                                {
                                    ok = true;
                                    break;
                                }
                                AppendCodePoint(buf, c);
                            }
                            if (buf.Length < 1 || !ok)
                            {
                                SyntaxError(chars, "Invalid multicharacter string");
                            }
                            // We have new string. Add it to set and continue;
                            // we don't need to drop through to the further
                            // processing
                            string curString = buf.ToString();
                            if (op == '-')
                            {
#pragma warning disable 612, 618
                                int lastSingle = CharSequences.GetSingleCodePoint((lastString == null ? "" : lastString).ToCharSequence());
                                int curSingle = CharSequences.GetSingleCodePoint(curString.ToCharSequence());
#pragma warning restore 612, 618
                                if (lastSingle != int.MaxValue && curSingle != int.MaxValue)
                                {
                                    Add(lastSingle, curSingle);
                                }
                                else
                                {
                                    try
                                    {
                                        StringRange.Expand(lastString, curString, true, strings);
                                    }
                                    catch (Exception e)
                                    {
                                        SyntaxError(chars, e.Message);
                                    }
                                }
                                lastString = null;
                                op = (char)0;
                            }
                            else
                            {
                                Add(curString.ToCharSequence());
                                lastString = curString;
                            }
                            patBuf.Append('{');
                            AppendToPat(patBuf, curString, false);
                            patBuf.Append('}');
                            continue;
                        case SymbolTable.SYMBOL_REF:
                            //         symbols  nosymbols
                            // [a-$]   error    error (ambiguous)
                            // [a$]    anchor   anchor
                            // [a-$x]  var "x"* literal '$'
                            // [a-$.]  error    literal '$'
                            // *We won't get here in the case of var "x"
                            backup = chars.GetPos(backup);
                            c = chars.Next(opts);
                            literal = chars.IsEscaped;
                            bool anchor = (c == ']' && !literal);
                            if (symbols == null && !anchor)
                            {
                                c = SymbolTable.SYMBOL_REF;
                                chars.SetPos(backup);
                                break; // literal '$'
                            }
                            if (anchor && op == 0)
                            {
                                if (lastItem == LAST1_RANGE)
                                {
                                    AddUnchecked(lastChar, lastChar);
                                    AppendToPat(patBuf, lastChar, false);
                                }
                                AddUnchecked(UnicodeMatcher.ETHER);
                                usePat = true;
                                patBuf.Append(SymbolTable.SYMBOL_REF).Append(']');
                                mode = MODE2_OUTBRACKET;
                                continue;
                            }
                            SyntaxError(chars, "Unquoted '$'");
                            break;
                        default:
                            break;
                    }
                }

                // -------- Parse literal characters.  This includes both
                // escaped chars ("\u4E01") and non-syntax characters
                // ("a").

                switch (lastItem)
                {
                    case LAST0_START:
                        if (op == '-' && lastString != null)
                        {
                            SyntaxError(chars, "Invalid range");
                        }
                        lastItem = LAST1_RANGE;
                        lastChar = c;
                        lastString = null;
                        break;
                    case LAST1_RANGE:
                        if (op == '-')
                        {
                            if (lastString != null)
                            {
                                SyntaxError(chars, "Invalid range");
                            }
                            if (lastChar >= c)
                            {
                                // Don't allow redundant (a-a) or empty (b-a) ranges;
                                // these are most likely typos.
                                SyntaxError(chars, "Invalid range");
                            }
                            AddUnchecked(lastChar, c);
                            AppendToPat(patBuf, lastChar, false);
                            patBuf.Append(op);
                            AppendToPat(patBuf, c, false);
                            lastItem = LAST0_START;
                            op = (char)0;
                        }
                        else
                        {
                            AddUnchecked(lastChar, lastChar);
                            AppendToPat(patBuf, lastChar, false);
                            lastChar = c;
                        }
                        break;
                    case LAST2_SET:
                        if (op != 0)
                        {
                            SyntaxError(chars, "Set expected after operator");
                        }
                        lastChar = c;
                        lastItem = LAST1_RANGE;
                        break;
                }
            }

            if (mode != MODE2_OUTBRACKET)
            {
                SyntaxError(chars, "Missing ']'");
            }

            chars.SkipIgnored(opts);

            /**
             * Handle global flags (invert, case insensitivity).  If this
             * pattern should be compiled case-insensitive, then we need
             * to close over case BEFORE COMPLEMENTING.  This makes
             * patterns like /[^abc]/i work.
             */
            if ((options & CASE) != 0)
            {
                CloseOver(CASE);
            }
            if (invert)
            {
                Complement();
            }

            // Use the rebuilt pattern (pat) only if necessary.  Prefer the
            // generated pattern.
            if (usePat)
            {
                Append(rebuiltPat, patBuf.ToCharSequence());
            }
            else
            {
                AppendNewPattern(rebuiltPat, false, true);
            }
        }

        private static void SyntaxError(RuleCharacterIterator chars, string msg)
        {
            throw new ArgumentException("Error: " + msg + " at \"" +
                    Utility.Escape(chars.ToString()) +
                    '"');
        }

        /**
         * Add the contents of the UnicodeSet (as strings) into a collection.
         * @param target collection to add into
         * @stable ICU 4.4
         */
        public ICollection<string> AddAllTo(ICollection<string> target)
        {
            return AddAllTo(this, target);
        }


        /**
         * Add the contents of the UnicodeSet (as strings) into a collection.
         * @param target collection to add into
         * @stable ICU 4.4
         */
        public string[] AddAllTo(string[] target)
        {
            return AddAllTo(this, target);
        }

        /**
         * Add the contents of the UnicodeSet (as strings) into an array.
         * @stable ICU 4.4
         */
        public static string[] ToArray(UnicodeSet set)
        {
            return AddAllTo(set, new string[set.Count]);
        }

        /**
         * Add the contents of the collection (as strings) into this UnicodeSet.
         * The collection must not contain null.
         * @param source the collection to add
         * @return a reference to this object
         * @stable ICU 4.4
         */
        public UnicodeSet Add<T>(IEnumerable<T> source)
        {
            return AddAll(source);
        }

        /**
         * Add a collection (as strings) into this UnicodeSet.
         * Uses standard naming convention.
         * @param source collection to add into
         * @return a reference to this object
         * @stable ICU 4.4
         */
        public UnicodeSet AddAll<T>(IEnumerable<T> source)
        {
            CheckFrozen();
            foreach (object o in source)
            {
                Add(o.ToString());
            }
            return this;
        }

        //----------------------------------------------------------------
        // Implementation: Utility methods
        //----------------------------------------------------------------

        private void EnsureCapacity(int newLen)
        {
            if (newLen <= list.Length) return;
            int[] temp = new int[newLen + GROW_EXTRA];
            System.Array.Copy(list, 0, temp, 0, len);
            list = temp;
        }

        private void EnsureBufferCapacity(int newLen)
        {
            if (buffer != null && newLen <= buffer.Length) return;
            buffer = new int[newLen + GROW_EXTRA];
        }

        /**
         * Assumes start <= end.
         */
        private int[] Range(int start, int end)
        {
            if (rangeList == null)
            {
                rangeList = new int[] { start, end + 1, HIGH };
            }
            else
            {
                rangeList[0] = start;
                rangeList[1] = end + 1;
            }
            return rangeList;
        }

        //----------------------------------------------------------------
        // Implementation: Fundamental operations
        //----------------------------------------------------------------

        // polarity = 0, 3 is normal: x xor y
        // polarity = 1, 2: x xor ~y == x === y

        private UnicodeSet Xor(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b;
            // TODO: Based on the call hierarchy, polarity of 1 or 2 is never used
            //      so the following if statement will not be called.
            ///CLOVER:OFF
            if (polarity == 1 || polarity == 2)
            {
                b = LOW;
                if (other[j] == LOW)
                { // skip base if already LOW
                    ++j;
                    b = other[j];
                }
                ///CLOVER:ON
            }
            else
            {
                b = other[j++];
            }
            // simplest of all the routines
            // sort the values, discarding identicals!
            while (true)
            {
                if (a < b)
                {
                    buffer[k++] = a;
                    a = list[i++];
                }
                else if (b < a)
                {
                    buffer[k++] = b;
                    b = other[j++];
                }
                else if (a != HIGH)
                { // at this point, a == b
                  // discard both values!
                    a = list[i++];
                    b = other[j++];
                }
                else
                { // DONE!
                    buffer[k++] = HIGH;
                    len = k;
                    break;
                }
            }
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        // polarity = 0 is normal: x union y
        // polarity = 2: x union ~y
        // polarity = 1: ~x union y
        // polarity = 3: ~x union ~y

        private UnicodeSet Add(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b = other[j++];
            // change from xor is that we have to check overlapping pairs
            // polarity bit 1 means a is second, bit 2 means b is.
            //main:
            while (true)
            {
                switch (polarity)
                {
                    case 0: // both first; take lower if unequal
                        if (a < b)
                        { // take a
                          // Back up over overlapping ranges in buffer[]
                            if (k > 0 && a <= buffer[k - 1])
                            {
                                // Pick latter end value in buffer[] vs. list[]
                                a = Max(list[i], buffer[--k]);
                            }
                            else
                            {
                                // No overlap
                                buffer[k++] = a;
                                a = list[i];
                            }
                            i++; // Common if/else code factored out
                            polarity ^= 1;
                        }
                        else if (b < a)
                        { // take b
                            if (k > 0 && b <= buffer[k - 1])
                            {
                                b = Max(other[j], buffer[--k]);
                            }
                            else
                            {
                                buffer[k++] = b;
                                b = other[j];
                            }
                            j++;
                            polarity ^= 2;
                        }
                        else
                        { // a == b, take a, drop b
                            if (a == HIGH) goto main_break;
                            // This is symmetrical; it doesn't matter if
                            // we backtrack with a or b. - liu
                            if (k > 0 && a <= buffer[k - 1])
                            {
                                a = Max(list[i], buffer[--k]);
                            }
                            else
                            {
                                // No overlap
                                buffer[k++] = a;
                                a = list[i];
                            }
                            i++;
                            polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 3: // both second; take higher if unequal, and drop other
                        if (b <= a)
                        { // take a
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a;
                        }
                        else
                        { // take b
                            if (b == HIGH) goto main_break;
                            buffer[k++] = b;
                        }
                        a = list[i++]; polarity ^= 1;   // factored common code
                        b = other[j++]; polarity ^= 2;
                        break;
                    case 1: // a second, b first; if b < a, overlap
                        if (a < b)
                        { // no overlap, take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // OVERLAP, drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 2: // a first, b second; if a < b, overlap
                        if (b < a)
                        { // no overlap, take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else if (a < b)
                        { // OVERLAP, drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                }
            }
            main_break: { }
            buffer[k++] = HIGH;    // terminate
            len = k;
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        // polarity = 0 is normal: x intersect y
        // polarity = 2: x intersect ~y == set-minus
        // polarity = 1: ~x intersect y
        // polarity = 3: ~x intersect ~y

        private UnicodeSet Retain(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b = other[j++];
            // change from xor is that we have to check overlapping pairs
            // polarity bit 1 means a is second, bit 2 means b is.
            //main:
            while (true)
            {
                switch (polarity)
                {
                    case 0: // both first; drop the smaller
                        if (a < b)
                        { // drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, take one, drop other
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 3: // both second; take lower if unequal
                        if (a < b)
                        { // take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, take one, drop other
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 1: // a second, b first;
                        if (a < b)
                        { // NO OVERLAP, drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // OVERLAP, take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 2: // a first, b second; if a < b, overlap
                        if (b < a)
                        { // no overlap, drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else if (a < b)
                        { // OVERLAP, take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                }
            }
            main_break: { }
            buffer[k++] = HIGH;    // terminate
            len = k;
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        private static int Max(int a, int b)
        {
            return (a > b) ? a : b;
        }

        //----------------------------------------------------------------
        // Generic filter-based scanning code
        //----------------------------------------------------------------

        private interface IFilter
        {
            bool Contains(int codePoint);
        }

        private class NumericValueFilter : IFilter
        {
            public double Value { get; set; }
            internal NumericValueFilter(double value)
            {
                this.Value = value;
            }

            public bool Contains(int ch)
            {
                return UCharacter.GetUnicodeNumericValue(ch) == Value;
            }
        }

        private class GeneralCategoryMaskFilter : IFilter
        {
            public int Mask { get; set; }
            internal GeneralCategoryMaskFilter(int mask) { this.Mask = mask; }

            public bool Contains(int ch)
            {
                return ((1 << (int)UCharacter.GetType(ch)) & Mask) != 0;
            }
        }

        private class IntPropertyFilter : IFilter
        {
            public int Prop { get; set; }
            public int Value { get; set; }
            internal IntPropertyFilter(int prop, int value)
            {
                this.Prop = prop;
                this.Value = value;
            }
            public bool Contains(int ch)
            {
                return UCharacter.GetIntPropertyValue(ch, (UnicodeProperty)Prop) == Value;
            }
        }

        private class ScriptExtensionsFilter : IFilter
        {
            public int Script { get; set; }
            internal ScriptExtensionsFilter(int script) { this.Script = script; }

            public bool Contains(int c)
            {
                return UScript.HasScript(c, Script);
            }
        }

        // VersionInfo for unassigned characters
        private static readonly VersionInfo NO_VERSION = VersionInfo.GetInstance(0, 0, 0, 0);

        private class VersionFilter : IFilter
        {
            private VersionInfo version;
            internal VersionFilter(VersionInfo version) { this.version = version; }

            public bool Contains(int ch)
            {
                VersionInfo v = UCharacter.GetAge(ch);
                // Reference comparison ok; VersionInfo caches and reuses
                // unique objects.
                return !Utility.SameObjects(v, NO_VERSION) &&
                        v.CompareTo(version) <= 0;
            }
        }

        private static UnicodeSet GetInclusions(int src)
        {
            lock (syncLock)
            {
                if (INCLUSIONS == null)
                {
                    INCLUSIONS = new UnicodeSet[UCharacterProperty.SRC_COUNT];
                }
                if (INCLUSIONS[src] == null)
                {
                    UnicodeSet incl = new UnicodeSet();
                    switch (src)
                    {
                        case UCharacterProperty.SRC_CHAR:
                            UCharacterProperty.INSTANCE.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_PROPSVEC:
                            UCharacterProperty.INSTANCE.upropsvec_addPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CHAR_AND_PROPSVEC:
                            UCharacterProperty.INSTANCE.AddPropertyStarts(incl);
                            UCharacterProperty.INSTANCE.upropsvec_addPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CASE_AND_NORM:
                            Norm2AllModes.GetNFCInstance().impl.AddPropertyStarts(incl);
                            UCaseProps.INSTANCE.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFC:
                            Norm2AllModes.GetNFCInstance().impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFKC:
                            Norm2AllModes.GetNFKCInstance().impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFKC_CF:
                            Norm2AllModes.GetNFKC_CFInstance().impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFC_CANON_ITER:
                            Norm2AllModes.GetNFCInstance().impl.AddCanonIterPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CASE:
                            UCaseProps.INSTANCE.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_BIDI:
                            UBiDiProps.INSTANCE.AddPropertyStarts(incl);
                            break;
                        default:
                            throw new InvalidOperationException("UnicodeSet.getInclusions(unknown src " + src + ")");
                    }
                    INCLUSIONS[src] = incl;
                }
                return INCLUSIONS[src];
            }
        }

        /**
         * Generic filter-based scanning code for UCD property UnicodeSets.
         */
        private UnicodeSet ApplyFilter(IFilter filter, int src)
        {
            // Logically, walk through all Unicode characters, noting the start
            // and end of each range for which filter.contain(c) is
            // true.  Add each range to a set.
            //
            // To improve performance, use an inclusions set which
            // encodes information about character ranges that are known
            // to have identical properties.
            // getInclusions(src) contains exactly the first characters of
            // same-value ranges for the given properties "source".

            Clear();

            int startHasProperty = -1;
            UnicodeSet inclusions = GetInclusions(src);
            int limitRange = inclusions.GetRangeCount();

            for (int j = 0; j < limitRange; ++j)
            {
                // get current range
                int start = inclusions.GetRangeStart(j);
                int end = inclusions.GetRangeEnd(j);

                // for all the code points in the range, process
                for (int ch = start; ch <= end; ++ch)
                {
                    // only add to the unicodeset on inflection points --
                    // where the hasProperty value changes to false
                    if (filter.Contains(ch))
                    {
                        if (startHasProperty < 0)
                        {
                            startHasProperty = ch;
                        }
                    }
                    else if (startHasProperty >= 0)
                    {
                        AddUnchecked(startHasProperty, ch - 1);
                        startHasProperty = -1;
                    }
                }
            }
            if (startHasProperty >= 0)
            {
                AddUnchecked(startHasProperty, 0x10FFFF);
            }

            return this;
        }


        /**
         * Remove leading and trailing Pattern_White_Space and compress
         * internal Pattern_White_Space to a single space character.
         */
        private static string MungeCharName(string source)
        {
            source = PatternProps.TrimWhiteSpace(source);
            StringBuilder buf = null;
            for (int i = 0; i < source.Length; ++i)
            {
                char ch = source[i];
                if (PatternProps.IsWhiteSpace(ch))
                {
                    if (buf == null)
                    {
                        buf = new StringBuilder().Append(source, 0, i);
                    }
                    else if (buf[buf.Length - 1] == ' ')
                    {
                        continue;
                    }
                    ch = ' '; // convert to ' '
                }
                if (buf != null)
                {
                    buf.Append(ch);
                }
            }
            return buf == null ? source : buf.ToString();
        }

        //----------------------------------------------------------------
        // Property set API
        //----------------------------------------------------------------

        /**
         * Modifies this set to contain those code points which have the
         * given value for the given binary or enumerated property, as
         * returned by UCharacter.getIntPropertyValue.  Prior contents of
         * this set are lost.
         *
         * @param prop a property in the range
         * UProperty.BIN_START..UProperty.BIN_LIMIT-1 or
         * UProperty.INT_START..UProperty.INT_LIMIT-1 or.
         * UProperty.MASK_START..UProperty.MASK_LIMIT-1.
         *
         * @param value a value in the range
         * UCharacter.getIntPropertyMinValue(prop)..
         * UCharacter.getIntPropertyMaxValue(prop), with one exception.
         * If prop is UProperty.GENERAL_CATEGORY_MASK, then value should not be
         * a UCharacter.getType() result, but rather a mask value produced
         * by logically ORing (1 &lt;&lt; UCharacter.getType()) values together.
         * This allows grouped categories such as [:L:] to be represented.
         *
         * @return a reference to this set
         *
         * @stable ICU 2.4
         */
        public UnicodeSet ApplyIntPropertyValue(int prop, int value)
        {
            CheckFrozen();
            if (prop == (int)UnicodeProperty.GENERAL_CATEGORY_MASK)
            {
                ApplyFilter(new GeneralCategoryMaskFilter(value), UCharacterProperty.SRC_CHAR);
            }
            else if (prop == (int)UnicodeProperty.SCRIPT_EXTENSIONS)
            {
                ApplyFilter(new ScriptExtensionsFilter(value), UCharacterProperty.SRC_PROPSVEC);
            }
            else
            {
                ApplyFilter(new IntPropertyFilter(prop, value), UCharacterProperty.INSTANCE.GetSource(prop));
            }
            return this;
        }



        /**
         * Modifies this set to contain those code points which have the
         * given value for the given property.  Prior contents of this
         * set are lost.
         *
         * @param propertyAlias a property alias, either short or long.
         * The name is matched loosely.  See PropertyAliases.txt for names
         * and a description of loose matching.  If the value string is
         * empty, then this string is interpreted as either a
         * General_Category value alias, a Script value alias, a binary
         * property alias, or a special ID.  Special IDs are matched
         * loosely and correspond to the following sets:
         *
         * "ANY" = [\\u0000-\\U0010FFFF],
         * "ASCII" = [\\u0000-\\u007F].
         *
         * @param valueAlias a value alias, either short or long.  The
         * name is matched loosely.  See PropertyValueAliases.txt for
         * names and a description of loose matching.  In addition to
         * aliases listed, numeric values and canonical combining classes
         * may be expressed numerically, e.g., ("nv", "0.5") or ("ccc",
         * "220").  The value string may also be empty.
         *
         * @return a reference to this set
         *
         * @stable ICU 2.4
         */
        public UnicodeSet ApplyPropertyAlias(string propertyAlias, string valueAlias)
        {
            return ApplyPropertyAlias(propertyAlias, valueAlias, null);
        }

        /**
         * Modifies this set to contain those code points which have the
         * given value for the given property.  Prior contents of this
         * set are lost.
         * @param propertyAlias A string of the property alias.
         * @param valueAlias A string of the value alias.
         * @param symbols if not null, then symbols are first called to see if a property
         * is available. If true, then everything else is skipped.
         * @return this set
         * @stable ICU 3.2
         */
        public UnicodeSet ApplyPropertyAlias(string propertyAlias,
                string valueAlias, ISymbolTable symbols)
        {
            CheckFrozen();
            UnicodeProperty p;
            UnicodeProperty v;
            bool invert = false;

            if (symbols != null
                    && (symbols is XSymbolTable)
                        && ((XSymbolTable)symbols).ApplyPropertyAlias(propertyAlias, valueAlias, this))
            {
                return this;
            }

            if (XSYMBOL_TABLE != null)
            {
                if (XSYMBOL_TABLE.ApplyPropertyAlias(propertyAlias, valueAlias, this))
                {
                    return this;
                }
            }

            if (valueAlias.Length > 0)
            {
                p = (UnicodeProperty)UCharacter.GetPropertyEnum(propertyAlias);

                // Treat gc as gcm
                if (p == UnicodeProperty.GENERAL_CATEGORY)
                {
                    p = UnicodeProperty.GENERAL_CATEGORY_MASK;
                }

#pragma warning disable 612, 618
                if ((p >= UnicodeProperty.BINARY_START && p < UnicodeProperty.BINARY_LIMIT) ||
                        (p >= UnicodeProperty.INT_START && p < UnicodeProperty.INT_LIMIT) ||
                        (p >= UnicodeProperty.MASK_START && p < UnicodeProperty.MASK_LIMIT))
#pragma warning restore 612, 618
                {
                    try
                    {
                        v = (UnicodeProperty)UCharacter.GetPropertyValueEnum(p, valueAlias);
                    }
                    catch (ArgumentException e)
                    {
                        // Handle numeric CCC
                        if (p == UnicodeProperty.CANONICAL_COMBINING_CLASS ||
                                p == UnicodeProperty.LEAD_CANONICAL_COMBINING_CLASS ||
                                p == UnicodeProperty.TRAIL_CANONICAL_COMBINING_CLASS)
                        {
                            v = (UnicodeProperty)int.Parse(PatternProps.TrimWhiteSpace(valueAlias), CultureInfo.InvariantCulture);
                            // Anything between 0 and 255 is valid even if unused.
                            if (v < 0 || (int)v > 255) throw e;
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }

                else
                {
                    switch (p)
                    {
                        case UnicodeProperty.NUMERIC_VALUE:
                            {
                                double value = double.Parse(PatternProps.TrimWhiteSpace(valueAlias), CultureInfo.InvariantCulture);
                                ApplyFilter(new NumericValueFilter(value), UCharacterProperty.SRC_CHAR);
                                return this;
                            }
                        case UnicodeProperty.NAME:
                            {
                                // Must munge name, since
                                // UCharacter.charFromName() does not do
                                // 'loose' matching.
                                String buf = MungeCharName(valueAlias);
                                int ch = UCharacter.GetCharFromExtendedName(buf);
                                if (ch == -1)
                                {
                                    throw new ArgumentException("Invalid character name");
                                }
                                Clear();
                                AddUnchecked(ch);
                                return this;
                            }
#pragma warning disable 612, 618
                        case UnicodeProperty.UNICODE_1_NAME:
                            // ICU 49 deprecates the Unicode_1_Name property APIs.
                            throw new ArgumentException("Unicode_1_Name (na1) not supported");
#pragma warning restore 612, 618
                        case UnicodeProperty.AGE:
                            {
                                // Must munge name, since
                                // VersionInfo.getInstance() does not do
                                // 'loose' matching.
                                VersionInfo version = VersionInfo.GetInstance(MungeCharName(valueAlias));
                                ApplyFilter(new VersionFilter(version), UCharacterProperty.SRC_PROPSVEC);
                                return this;
                            }
                        case UnicodeProperty.SCRIPT_EXTENSIONS:
                            v = (UnicodeProperty)UCharacter.GetPropertyValueEnum(UnicodeProperty.SCRIPT, valueAlias);
                            // fall through to calling applyIntPropertyValue()
                            break;
                        default:
                            // p is a non-binary, non-enumerated property that we
                            // don't support (yet).
                            throw new ArgumentException("Unsupported property");
                    }
                }
            }

            else
            {
                // valueAlias is empty.  Interpret as General Category, Script,
                // Binary property, or ANY or ASCII.  Upon success, p and v will
                // be set.
                UPropertyAliases pnames = UPropertyAliases.INSTANCE;
                p = UnicodeProperty.GENERAL_CATEGORY_MASK;
                v = (UnicodeProperty)pnames.GetPropertyValueEnum((int)p, propertyAlias);
#pragma warning disable 612, 618
                if (v == UnicodeProperty.UNDEFINED)
                {
                    p = UnicodeProperty.SCRIPT;
                    v = (UnicodeProperty)pnames.GetPropertyValueEnum((int)p, propertyAlias);
                    if (v == UnicodeProperty.UNDEFINED)
                    {
                        p = (UnicodeProperty)pnames.GetPropertyEnum(propertyAlias);
                        if (p == UnicodeProperty.UNDEFINED)
                        {
                            p = (UnicodeProperty)(-1);
                        }
                        if (p >= UnicodeProperty.BINARY_START && p < UnicodeProperty.BINARY_LIMIT)
#pragma warning restore 612, 618
                        {
                            v = (UnicodeProperty)1;
                        }
                        else if ((int)p == -1)
                        {
                            if (0 == UPropertyAliases.Compare(ANY_ID, propertyAlias))
                            {
                                Set(MIN_VALUE, MAX_VALUE);
                                return this;
                            }
                            else if (0 == UPropertyAliases.Compare(ASCII_ID, propertyAlias))
                            {
                                Set(0, 0x7F);
                                return this;
                            }
                            else if (0 == UPropertyAliases.Compare(ASSIGNED, propertyAlias))
                            {
                                // [:Assigned:]=[:^Cn:]
                                p = UnicodeProperty.GENERAL_CATEGORY_MASK;
                                v = (UnicodeProperty)(1 << UCharacter.UNASSIGNED);
                                invert = true;
                            }
                            else
                            {
                                // Property name was never matched.
                                throw new ArgumentException("Invalid property alias: " + propertyAlias + "=" + valueAlias);
                            }
                        }
                        else
                        {
                            // Valid propery name, but it isn't binary, so the value
                            // must be supplied.
                            throw new ArgumentException("Missing property value");
                        }
                    }
                }
            }

            ApplyIntPropertyValue((int)p, (int)v);
            if (invert)
            {
                Complement();
            }

            return this;
        }

        //----------------------------------------------------------------
        // Property set patterns
        //----------------------------------------------------------------

        /**
         * Return true if the given position, in the given pattern, appears
         * to be the start of a property set pattern.
         */
        private static bool ResemblesPropertyPattern(string pattern, int pos)
        {
            // Patterns are at least 5 characters long
            if ((pos + 5) > pattern.Length)
            {
                return false;
            }

            // Look for an opening [:, [:^, \p, or \P
            return pattern.RegionMatches(pos, "[:", 0, 2) ||
                    pattern.RegionMatches(true, pos, "\\p", 0, 2) ||
                    pattern.RegionMatches(pos, "\\N", 0, 2);
        }

        /**
         * Return true if the given iterator appears to point at a
         * property pattern.  Regardless of the result, return with the
         * iterator unchanged.
         * @param chars iterator over the pattern characters.  Upon return
         * it will be unchanged.
         * @param iterOpts RuleCharacterIterator options
         */
        private static bool ResemblesPropertyPattern(RuleCharacterIterator chars,
                int iterOpts)
        {
            bool result = false;
            iterOpts &= ~RuleCharacterIterator.PARSE_ESCAPES;
            Object pos = chars.GetPos(null);
            int c = chars.Next(iterOpts);
            if (c == '[' || c == '\\')
            {
                int d = chars.Next(iterOpts & ~RuleCharacterIterator.SKIP_WHITESPACE);
                result = (c == '[') ? (d == ':') :
                    (d == 'N' || d == 'p' || d == 'P');
            }
            chars.SetPos(pos);
            return result;
        }

        /**
         * Parse the given property pattern at the given parse position.
         * @param symbols TODO
         */
        private UnicodeSet ApplyPropertyPattern(string pattern, ParsePosition ppos, ISymbolTable symbols)
        {
            int pos = ppos.Index;

            // On entry, ppos should point to one of the following locations:

            // Minimum length is 5 characters, e.g. \p{L}
            if ((pos + 5) > pattern.Length)
            {
                return null;
            }

            bool posix = false; // true for [:pat:], false for \p{pat} \P{pat} \N{pat}
            bool isName = false; // true for \N{pat}, o/w false
            bool invert = false;

            // Look for an opening [:, [:^, \p, or \P
            if (pattern.RegionMatches(pos, "[:", 0, 2))
            {
                posix = true;
                pos = PatternProps.SkipWhiteSpace(pattern, (pos + 2));
                if (pos < pattern.Length && pattern[pos] == '^')
                {
                    ++pos;
                    invert = true;
                }
            }
            else if (pattern.RegionMatches(true, pos, "\\p", 0, 2) ||
                  pattern.RegionMatches(pos, "\\N", 0, 2))
            {
                char c = pattern[pos + 1];
                invert = (c == 'P');
                isName = (c == 'N');
                pos = PatternProps.SkipWhiteSpace(pattern, (pos + 2));
                if (pos == pattern.Length || pattern[pos++] != '{')
                {
                    // Syntax error; "\p" or "\P" not followed by "{"
                    return null;
                }
            }
            else
            {
                // Open delimiter not seen
                return null;
            }

            // Look for the matching close delimiter, either :] or }
            int close = pattern.IndexOf(posix ? ":]" : "}", pos);
            if (close < 0)
            {
                // Syntax error; close delimiter missing
                return null;
            }

            // Look for an '=' sign.  If this is present, we will parse a
            // medium \p{gc=Cf} or long \p{GeneralCategory=Format}
            // pattern.
            int equals = pattern.IndexOf('=', pos);
            string propName, valueName;
            if (equals >= 0 && equals < close && !isName)
            {
                // Equals seen; parse medium/long pattern
                propName = pattern.Substring(pos, equals - pos);
                valueName = pattern.Substring(equals + 1, close - (equals + 1));
            }

            else
            {
                // Handle case where no '=' is seen, and \N{}
                propName = pattern.Substring(pos, close - pos);
                valueName = "";

                // Handle \N{name}
                if (isName)
                {
                    // This is a little inefficient since it means we have to
                    // parse "na" back to UProperty.NAME even though we already
                    // know it's UProperty.NAME.  If we refactor the API to
                    // support args of (int, String) then we can remove
                    // "na" and make this a little more efficient.
                    valueName = propName;
                    propName = "na";
                }
            }

            ApplyPropertyAlias(propName, valueName, symbols);

            if (invert)
            {
                Complement();
            }

            // Move to the limit position after the close delimiter
            ppos.Index = close + (posix ? 2 : 1);

            return this;
        }

        /**
         * Parse a property pattern.
         * @param chars iterator over the pattern characters.  Upon return
         * it will be advanced to the first character after the parsed
         * pattern, or the end of the iteration if all characters are
         * parsed.
         * @param rebuiltPat the pattern that was parsed, rebuilt or
         * copied from the input pattern, as appropriate.
         * @param symbols TODO
         */
        private void ApplyPropertyPattern(RuleCharacterIterator chars,
                StringBuilder rebuiltPat, ISymbolTable symbols)
        {
            string patStr = chars.Lookahead();
            ParsePosition pos = new ParsePosition(0);
            ApplyPropertyPattern(patStr, pos, symbols);
            if (pos.Index == 0)
            {
                SyntaxError(chars, "Invalid property pattern");
            }
            chars.Jumpahead(pos.Index);
            Append(rebuiltPat, patStr.Substring(0, pos.Index - 0).ToCharSequence());
        }

        //----------------------------------------------------------------
        // Case folding API
        //----------------------------------------------------------------

        /**
         * Bitmask for constructor and applyPattern() indicating that
         * white space should be ignored.  If set, ignore Unicode Pattern_White_Space characters,
         * unless they are quoted or escaped.  This may be ORed together
         * with other selectors.
         * @stable ICU 3.8
         */
        public static readonly int IGNORE_SPACE = 1;

        /**
         * Bitmask for constructor, applyPattern(), and closeOver()
         * indicating letter case.  This may be ORed together with other
         * selectors.
         *
         * Enable case insensitive matching.  E.g., "[ab]" with this flag
         * will match 'a', 'A', 'b', and 'B'.  "[^ab]" with this flag will
         * match all except 'a', 'A', 'b', and 'B'. This performs a full
         * closure over case mappings, e.g. U+017F for s.
         *
         * The resulting set is a superset of the input for the code points but
         * not for the strings.
         * It performs a case mapping closure of the code points and adds
         * full case folding strings for the code points, and reduces strings of
         * the original set to their full case folding equivalents.
         *
         * This is designed for case-insensitive matches, for example
         * in regular expressions. The full code point case closure allows checking of
         * an input character directly against the closure set.
         * Strings are matched by comparing the case-folded form from the closure
         * set with an incremental case folding of the string in question.
         *
         * The closure set will also contain single code points if the original
         * set contained case-equivalent strings (like U+00DF for "ss" or "Ss" etc.).
         * This is not necessary (that is, redundant) for the above matching method
         * but results in the same closure sets regardless of whether the original
         * set contained the code point or a string.
         * @stable ICU 3.8
         */
        public static readonly int CASE = 2;

        /**
         * Alias for UnicodeSet.CASE, for ease of porting from C++ where ICU4C
         * also has both USET_CASE and USET_CASE_INSENSITIVE (see uset.h).
         * @see #CASE
         * @stable ICU 3.4
         */
        public static readonly int CASE_INSENSITIVE = 2;

        /**
         * Bitmask for constructor, applyPattern(), and closeOver()
         * indicating letter case.  This may be ORed together with other
         * selectors.
         *
         * Enable case insensitive matching.  E.g., "[ab]" with this flag
         * will match 'a', 'A', 'b', and 'B'.  "[^ab]" with this flag will
         * match all except 'a', 'A', 'b', and 'B'. This adds the lower-,
         * title-, and uppercase mappings as well as the case folding
         * of each existing element in the set.
         * @stable ICU 3.4
         */
        public static readonly int ADD_CASE_MAPPINGS = 4;

        //  add the result of a full case mapping to the set
        //  use str as a temporary string to avoid constructing one
        private static void AddCaseMapping(UnicodeSet set, int result, StringBuilder full)
        {
            if (result >= 0)
            {
                if (result > UCaseProps.MAX_STRING_LENGTH)
                {
                    // add a single-code point case mapping
                    set.Add(result);
                }
                else
                {
                    // add a string case mapping from full with length result
                    set.Add(full.ToString());
                    full.Length = 0;
                }
            }
            // result < 0: the code point mapped to itself, no need to add it
            // see UCaseProps
        }

        /**
         * Close this set over the given attribute.  For the attribute
         * CASE, the result is to modify this set so that:
         *
         * 1. For each character or string 'a' in this set, all strings
         * 'b' such that foldCase(a) == foldCase(b) are added to this set.
         * (For most 'a' that are single characters, 'b' will have
         * b.Length() == 1.)
         *
         * 2. For each string 'e' in the resulting set, if e !=
         * foldCase(e), 'e' will be removed.
         *
         * Example: [aq\u00DF{Bc}{bC}{Fi}] =&gt; [aAqQ\u00DF\uFB01{ss}{bc}{fi}]
         *
         * (Here foldCase(x) refers to the operation
         * UCharacter.foldCase(x, true), and a == b actually denotes
         * a.equals(b), not pointer comparison.)
         *
         * @param attribute bitmask for attributes to close over.
         * Currently only the CASE bit is supported.  Any undefined bits
         * are ignored.
         * @return a reference to this set.
         * @stable ICU 3.8
         */
        public UnicodeSet CloseOver(int attribute)
        {
            CheckFrozen();
            if ((attribute & (CASE | ADD_CASE_MAPPINGS)) != 0)
            {
                UCaseProps csp = UCaseProps.INSTANCE;
                UnicodeSet foldSet = new UnicodeSet(this);
                ULocale root = ULocale.ROOT;

                // start with input set to guarantee inclusion
                // CASE: remove strings because the strings will actually be reduced (folded);
                //       therefore, start with no strings and add only those needed
                if ((attribute & CASE) != 0)
                {
                    foldSet.strings.Clear();
                }

                int n = GetRangeCount();
                int result;
                StringBuilder full = new StringBuilder();

                for (int i = 0; i < n; ++i)
                {
                    int start = GetRangeStart(i);
                    int end = GetRangeEnd(i);

                    if ((attribute & CASE) != 0)
                    {
                        // full case closure
                        for (int cp = start; cp <= end; ++cp)
                        {
                            csp.AddCaseClosure(cp, foldSet);
                        }
                    }
                    else
                    {
                        // add case mappings
                        // (does not add long s for regular s, or Kelvin for k, for example)
                        for (int cp = start; cp <= end; ++cp)
                        {
                            result = csp.ToFullLower(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullTitle(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullUpper(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullFolding(cp, full, 0);
                            AddCaseMapping(foldSet, result, full);
                        }
                    }
                }
                if (strings.Count > 0)
                {
                    if ((attribute & CASE) != 0)
                    {
                        foreach (String s in strings)
                        {
                            string str = UCharacter.FoldCase(s, 0);
                            if (!csp.AddStringCaseClosure(str, foldSet))
                            {
                                foldSet.Add(str); // does not map to code points: add the folded string itself
                            }
                        }
                    }
                    else
                    {
                        BreakIterator bi = BreakIterator.GetWordInstance(root);
                        foreach (string str in strings)
                        {
                            // TODO: call lower-level functions
                            foldSet.Add(UCharacter.ToLower(root, str));
                            foldSet.Add(UCharacter.ToTitleCase(root, str, bi));
                            foldSet.Add(UCharacter.ToUpper(root, str));
                            foldSet.Add(UCharacter.FoldCase(str, 0));
                        }
                    }
                }
                Set(foldSet);
            }
            return this;
        }

        /**
         * Internal class for customizing UnicodeSet parsing of properties.
         * TODO: extend to allow customizing of codepoint ranges
         * @draft ICU3.8 (retain)
         * @provisional This API might change or be removed in a future release.
         * @author medavis
         */
        abstract public class XSymbolTable : ISymbolTable
        {
            /**
             * Default constructor
             * @draft ICU3.8 (retain)
             * @provisional This API might change or be removed in a future release.
             */
            public XSymbolTable() { }
            /**
             * Supplies default implementation for SymbolTable (no action).
             * @draft ICU3.8 (retain)
             * @provisional This API might change or be removed in a future release.
             */
            public virtual IUnicodeMatcher LookupMatcher(int i)
            {
                return null;
            }

            /**
             * Override the interpretation of the sequence [:propertyName=propertyValue:] (and its negated and Perl-style
             * variant). The propertyName and propertyValue may be existing Unicode aliases, or may not be.
             * <p>
             * This routine will be called whenever the parsing of a UnicodeSet pattern finds such a
             * propertyName+propertyValue combination.
             *
             * @param propertyName
             *            the name of the property
             * @param propertyValue
             *            the name of the property value
             * @param result UnicodeSet value to change
             *            a set to which the characters having the propertyName+propertyValue are to be added.
             * @return returns true if the propertyName+propertyValue combination is to be overridden, and the characters
             *         with that property have been added to the UnicodeSet, and returns false if the
             *         propertyName+propertyValue combination is not recognized (in which case result is unaltered).
             * @draft ICU3.8 (retain)
             * @provisional This API might change or be removed in a future release.
             */
            public virtual bool ApplyPropertyAlias(string propertyName, string propertyValue, UnicodeSet result)
            {
                return false;
            }
            /**
             * Supplies default implementation for SymbolTable (no action).
             * @draft ICU3.8 (retain)
             * @provisional This API might change or be removed in a future release.
             */
            public virtual char[] Lookup(string s)
            {
                return null;
            }
            /**
             * Supplies default implementation for SymbolTable (no action).
             * @draft ICU3.8 (retain)
             * @provisional This API might change or be removed in a future release.
             */
            public virtual string ParseReference(string text, ParsePosition pos, int limit)
            {
                return null;
            }
        }

        /**
         * Is this frozen, according to the Freezable interface?
         *
         * @return value
         * @stable ICU 3.8
         */
        public virtual bool IsFrozen // ICU4N TODO: This does not implement IFreezable... but the comment suggests it should?
        {
            get { return (bmpSet != null || stringSpan != null); }
        }

        /**
         * Freeze this class, according to the Freezable interface.
         *
         * @return this
         * @stable ICU 4.4
         */
        public UnicodeSet Freeze()
        {
            if (!IsFrozen)
            {
                // Do most of what compact() does before freezing because
                // compact() will not work when the set is frozen.
                // Small modification: Don't shrink if the savings would be tiny (<=GROW_EXTRA).

                // Delete buffer first to defragment memory less.
                buffer = null;
                if (list.Length > (len + GROW_EXTRA))
                {
                    // Make the capacity equal to len or 1.
                    // We don't want to realloc of 0 size.
                    int capacity = (len == 0) ? 1 : len;
                    int[] oldList = list;
                    list = new int[capacity];
                    for (int i = capacity; i-- > 0;)
                    {
                        list[i] = oldList[i];
                    }
                }

                // Optimize contains() and span() and similar functions.
                if (strings.Count > 0)
                {
                    stringSpan = new UnicodeSetStringSpan(this, new List<string>(strings), UnicodeSetStringSpan.ALL);
                }
                if (stringSpan == null || !stringSpan.NeedsStringSpanUTF16)
                {
                    // Optimize for code point spans.
                    // There are no strings, or
                    // all strings are irrelevant for span() etc. because
                    // all of each string's code points are contained in this set.
                    // However, fully contained strings are relevant for spanAndCount(),
                    // so we create both objects.
                    bmpSet = new BMPSet(list, len);
                }
            }
            return this;
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The length of the span.</returns>
        /// <stable>ICU 4.4</stable>
        public int Span(string s, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The length of the span.</returns>
        /// <stable>ICU 4.4</stable>
        public int Span(StringBuilder s, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The length of the span.</returns>
        /// <stable>ICU 4.4</stable>
        public int Span(char[] s, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The length of the span.</returns>
        /// <stable>ICU 4.4</stable>
        internal int Span(ICharSequence s, SpanCondition spanCondition)
        {
            return Span(s, 0, spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <list type="bullet">
        ///     <item><description>If the start index is less than 0, span will start from 0.</description></item>
        ///     <item><description>If the start index is greater than the string length, span returns the string length.</description></item>
        /// </list>
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which ends the span (i.e. exclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int Span(string s, int start, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), start, spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <list type="bullet">
        ///     <item><description>If the start index is less than 0, span will start from 0.</description></item>
        ///     <item><description>If the start index is greater than the string length, span returns the string length.</description></item>
        /// </list>
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which ends the span (i.e. exclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int Span(StringBuilder s, int start, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), start, spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <list type="bullet">
        ///     <item><description>If the start index is less than 0, span will start from 0.</description></item>
        ///     <item><description>If the start index is greater than the string length, span returns the string length.</description></item>
        /// </list>
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which ends the span (i.e. exclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int Span(char[] s, int start, SpanCondition spanCondition)
        {
            return Span(s.ToCharSequence(), start, spanCondition);
        }

        /// <summary>
        /// Span a string using this UnicodeSet.
        /// <list type="bullet">
        ///     <item><description>If the start index is less than 0, span will start from 0.</description></item>
        ///     <item><description>If the start index is greater than the string length, span returns the string length.</description></item>
        /// </list>
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="start">The start index that the span begins.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which ends the span (i.e. exclusive).</returns>
        /// <stable>ICU 4.4</stable>
        internal virtual int Span(ICharSequence s, int start, SpanCondition spanCondition)
        {
            int end = s.Length;
            if (start < 0)
            {
                start = 0;
            }
            else if (start >= end)
            {
                return end;
            }
            if (bmpSet != null)
            {
                // Frozen set without strings, or no string is relevant for span().
                return bmpSet.Span(s, start, spanCondition, null);
            }
            if (stringSpan != null)
            {
                return stringSpan.Span(s, start, spanCondition);
            }
            else if (strings.Count > 0)
            {
                int which = spanCondition == SpanCondition.NOT_CONTAINED ? UnicodeSetStringSpan.FWD_UTF16_NOT_CONTAINED
                        : UnicodeSetStringSpan.FWD_UTF16_CONTAINED;
                UnicodeSetStringSpan strSpan = new UnicodeSetStringSpan(this, new List<string>(strings), which);
                if (strSpan.NeedsStringSpanUTF16)
                {
                    return strSpan.Span(s, start, spanCondition);
                }
            }

            return SpanCodePointsAndCount(s, start, spanCondition, null);
        }

        /// <summary>
        /// Same as <see cref="Span(string, SpanCondition)"/> but also counts the smallest number of set elements on any path across the span.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="spanCondition"></param>
        /// <param name="outCount">An output-only object (must not be null) for returning the count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        [Obsolete("This API is ICU internal only.")]
        public virtual int SpanAndCount(string s, int start, SpanCondition spanCondition, OutputInt outCount) // ICU4N TODO: API - change to use out parameter everywhere OutputInt is used ?
        {
            return SpanAndCount(s.ToCharSequence(), start, spanCondition, outCount);
        }

        /// <summary>
        /// Same as <see cref="Span(StringBuilder, SpanCondition)"/> but also counts the smallest number of set elements on any path across the span.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="spanCondition"></param>
        /// <param name="outCount">An output-only object (must not be null) for returning the count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        [Obsolete("This API is ICU internal only.")]
        public virtual int SpanAndCount(StringBuilder s, int start, SpanCondition spanCondition, OutputInt outCount) // ICU4N TODO: API - change to use out parameter everywhere OutputInt is used ?
        {
            return SpanAndCount(s.ToCharSequence(), start, spanCondition, outCount);
        }

        /// <summary>
        /// Same as <see cref="Span(char[], SpanCondition)"/> but also counts the smallest number of set elements on any path across the span.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="spanCondition"></param>
        /// <param name="outCount">An output-only object (must not be null) for returning the count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        [Obsolete("This API is ICU internal only.")]
        public virtual int SpanAndCount(char[] s, int start, SpanCondition spanCondition, OutputInt outCount) // ICU4N TODO: API - change to use out parameter everywhere OutputInt is used ?
        {
            return SpanAndCount(s.ToCharSequence(), start, spanCondition, outCount);
        }

        /// <summary>
        /// Same as <see cref="Span(ICharSequence, SpanCondition)"/> but also counts the smallest number of set elements on any path across the span.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="spanCondition"></param>
        /// <param name="outCount">An output-only object (must not be null) for returning the count.</param>
        /// <returns>The limit (exclusive end) of the span.</returns>
        [Obsolete("This API is ICU internal only.")]
        internal virtual int SpanAndCount(ICharSequence s, int start, SpanCondition spanCondition, OutputInt outCount)
        {
            if (outCount == null)
            {
                throw new ArgumentException("outCount must not be null");
            }
            int end = s.Length;
            if (start < 0)
            {
                start = 0;
            }
            else if (start >= end)
            {
                return end;
            }
            if (stringSpan != null)
            {
                // We might also have bmpSet != null,
                // but fully-contained strings are relevant for counting elements.
                return stringSpan.SpanAndCount(s, start, spanCondition, outCount);
            }
            else if (bmpSet != null)
            {
                return bmpSet.Span(s, start, spanCondition, outCount);
            }
            else if (strings.Count > 0)
            {
                int which = spanCondition == SpanCondition.NOT_CONTAINED ? UnicodeSetStringSpan.FWD_UTF16_NOT_CONTAINED
                        : UnicodeSetStringSpan.FWD_UTF16_CONTAINED;
                which |= UnicodeSetStringSpan.WITH_COUNT;
                UnicodeSetStringSpan strSpan = new UnicodeSetStringSpan(this, new List<string>(strings), which);
                return strSpan.SpanAndCount(s, start, spanCondition, outCount);
            }

            return SpanCodePointsAndCount(s, start, spanCondition, outCount);
        }

#pragma warning disable 612, 618
        private int SpanCodePointsAndCount(ICharSequence s, int start,
                SpanCondition spanCondition, OutputInt outCount)
#pragma warning restore 612, 618
        {
            // Pin to 0/1 values.
            bool spanContained = (spanCondition != SpanCondition.NOT_CONTAINED);

            int c;
            int next = start;
            int length = s.Length;
            int count = 0;
            do
            {
                c = Character.CodePointAt(s, next);
                if (spanContained != Contains(c))
                {
                    break;
                }
                ++count;
                next += Character.CharCount(c);
            } while (next < length);
            if (outCount != null)
            {
#pragma warning disable 612, 618
                outCount.Value = count;
#pragma warning restore 612, 618
            }
            return next;
        }

        /// <summary>
        /// Span a string backwards (from the end) using this <see cref="UnicodeSet"/>.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(string s, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the end) using this <see cref="UnicodeSet"/>.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(StringBuilder s, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the end) using this <see cref="UnicodeSet"/>.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(char[] s, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the end) using this <see cref="UnicodeSet"/>.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        internal virtual int SpanBack(ICharSequence s, SpanCondition spanCondition)
        {
            return SpanBack(s, s.Length, spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the <paramref name="fromIndex"/>) using this <see cref="UnicodeSet"/>.
        /// If the <paramref name="fromIndex"/> is less than 0, SpanBack will return 0.
        /// If <paramref name="fromIndex"/> is greater than the string length, SpanBack will start from the string length.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="fromIndex">The index of the char (exclusive) that the string should be spanned backwards.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(string s, int fromIndex, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), fromIndex, spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the <paramref name="fromIndex"/>) using this <see cref="UnicodeSet"/>.
        /// If the <paramref name="fromIndex"/> is less than 0, SpanBack will return 0.
        /// If <paramref name="fromIndex"/> is greater than the string length, SpanBack will start from the string length.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="fromIndex">The index of the char (exclusive) that the string should be spanned backwards.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(StringBuilder s, int fromIndex, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), fromIndex, spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the <paramref name="fromIndex"/>) using this <see cref="UnicodeSet"/>.
        /// If the <paramref name="fromIndex"/> is less than 0, SpanBack will return 0.
        /// If <paramref name="fromIndex"/> is greater than the string length, SpanBack will start from the string length.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="fromIndex">The index of the char (exclusive) that the string should be spanned backwards.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        public virtual int SpanBack(char[] s, int fromIndex, SpanCondition spanCondition)
        {
            return SpanBack(s.ToCharSequence(), fromIndex, spanCondition);
        }

        /// <summary>
        /// Span a string backwards (from the <paramref name="fromIndex"/>) using this <see cref="UnicodeSet"/>.
        /// If the <paramref name="fromIndex"/> is less than 0, SpanBack will return 0.
        /// If <paramref name="fromIndex"/> is greater than the string length, SpanBack will start from the string length.
        /// <para/>
        /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
        /// </summary>
        /// <param name="s">The string to be spanned.</param>
        /// <param name="fromIndex">The index of the char (exclusive) that the string should be spanned backwards.</param>
        /// <param name="spanCondition">The span condition.</param>
        /// <returns>The string index which starts the span (i.e. inclusive).</returns>
        /// <stable>ICU 4.4</stable>
        internal virtual int SpanBack(ICharSequence s, int fromIndex, SpanCondition spanCondition)
        {
            if (fromIndex <= 0)
            {
                return 0;
            }
            if (fromIndex > s.Length)
            {
                fromIndex = s.Length;
            }
            if (bmpSet != null)
            {
                // Frozen set without strings, or no string is relevant for spanBack().
                return bmpSet.SpanBack(s, fromIndex, spanCondition);
            }
            if (stringSpan != null)
            {
                return stringSpan.SpanBack(s, fromIndex, spanCondition);
            }
            else if (strings.Count > 0)
            {
                int which = (spanCondition == SpanCondition.NOT_CONTAINED)
                        ? UnicodeSetStringSpan.BACK_UTF16_NOT_CONTAINED
                                : UnicodeSetStringSpan.BACK_UTF16_CONTAINED;
                UnicodeSetStringSpan strSpan = new UnicodeSetStringSpan(this, new List<string>(strings), which);
                if (strSpan.NeedsStringSpanUTF16)
                {
                    return strSpan.SpanBack(s, fromIndex, spanCondition);
                }
            }

            // Pin to 0/1 values.
            bool spanContained = (spanCondition != SpanCondition.NOT_CONTAINED);

            int c;
            int prev = fromIndex;
            do
            {
                c = Character.CodePointBefore(s, prev);
                if (spanContained != Contains(c))
                {
                    break;
                }
                prev -= Character.CharCount(c);
            } while (prev > 0);
            return prev;
        }

        /**
         * Clone a thawed version of this class, according to the Freezable interface.
         * @return the clone, not frozen
         * @stable ICU 4.4
         */
        public UnicodeSet CloneAsThawed()
        {
            UnicodeSet result = new UnicodeSet(this);
            Debug.Assert(!result.IsFrozen);
            return result;
        }

        // internal function
        private void CheckFrozen()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Attempt to modify frozen object");
            }
        }

        // ************************
        // Additional methods for integration with Generics and Collections
        // ************************

        /**
         * A struct-like class used for iteration through ranges, for faster iteration than by String.
         * Read about the restrictions on usage in {@link UnicodeSet#ranges()}.
         *
         * @stable ICU 54
         */
        public class EntryRange
        {
            /**
             * The starting code point of the range.
             *
             * @stable ICU 54
             */
            public int Codepoint { get; set; }
            /**
             * The ending code point of the range
             *
             * @stable ICU 54
             */
            public int CodepointEnd { get; set; }


            internal EntryRange()
            {
            }

            /**
             * {@inheritDoc}
             *
             * @stable ICU 54
             */
            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                return (
                        Codepoint == CodepointEnd ? AppendToPat(b, Codepoint, false)
                                : AppendToPat(AppendToPat(b, Codepoint, false).Append('-'), CodepointEnd, false))
                                .ToString();
            }
        }

        /**
         * Provide for faster iteration than by String. Returns an Iterable/Iterator over ranges of code points.
         * The UnicodeSet must not be altered during the iteration.
         * The EntryRange instance is the same each time; the contents are just reset.
         *
         * <p><b>Warning: </b>To iterate over the full contents, you have to also iterate over the strings.
         *
         * <p><b>Warning: </b>For speed, UnicodeSet iteration does not check for concurrent modification.
         * Do not alter the UnicodeSet while iterating.
         *
         * <pre>
         * // Sample code
         * for (EntryRange range : us1.ranges()) {
         *     // do something with code points between range.codepoint and range.codepointEnd;
         * }
         * for (String s : us1.strings()) {
         *     // do something with each string;
         * }
         * </pre>
         *
         * @stable ICU 54
         */
        public IEnumerable<EntryRange> Ranges
        {
            get { return new EntryRangeEnumerable(this); }
        }

        private class EntryRangeEnumerable : IEnumerable<EntryRange>
        {
            internal readonly UnicodeSet outerInstance;

            internal EntryRangeEnumerable(UnicodeSet outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public IEnumerator<EntryRange> GetEnumerator()
            {
                return new EntryRangeEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new EntryRangeEnumerator(this);
            }
        }

        private class EntryRangeEnumerator : IEnumerator<EntryRange>
        {
            private int pos;
            private EntryRange result = new EntryRange();
            private readonly EntryRangeEnumerable outerInstance;

            internal EntryRangeEnumerator(EntryRangeEnumerable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public EntryRange Current
            {
                get { return result; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public bool MoveNext()
            {
                if (!HasNext())
                    return false;
                return Next() != null;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private bool HasNext()
            {
                return pos < outerInstance.outerInstance.len - 1;
            }

            private EntryRange Next()
            {
                if (pos < outerInstance.outerInstance.len - 1)
                {
                    result.Codepoint = outerInstance.outerInstance.list[pos++];
                    result.CodepointEnd = outerInstance.outerInstance.list[pos++] - 1;
                }
                else
                {
                    //throw new NoSuchElementException();
                    return null;
                }
                return result;
            }
            // ICU4N NOTE: Remove not supported in .NET
        }


        /**
         * Returns a string iterator. Uses the same order of iteration as {@link UnicodeSetIterator}.
         * <p><b>Warning: </b>For speed, UnicodeSet iteration does not check for concurrent modification.
         * Do not alter the UnicodeSet while iterating.
         * @see java.util.Set#iterator()
         * @stable ICU 4.4
         */
        public virtual IEnumerator<String> GetEnumerator()
        {
            return new UnicodeSetEnumerator2(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Cover for string iteration.
        private class UnicodeSetEnumerator2 : IEnumerator<string>
        {
            // Invariants:
            // sourceList != null then sourceList[item] is a valid character
            // sourceList == null then delegates to stringIterator
            private int[] sourceList;
            private int len;
            private int item;
            private int current;
            private int limit;
            private ISet<string> sourceStrings;
            private IEnumerator<string> stringIterator;
            private char[] buffer;

            private string currentElement = null;

            internal UnicodeSetEnumerator2(UnicodeSet source)
            {
                // set according to invariants
                len = source.len - 1;
                if (len > 0)
                {
                    sourceStrings = source.strings;
                    sourceList = source.list;
                    current = sourceList[item++];
                    limit = sourceList[item++];
                }
                else
                {
                    stringIterator = source.strings.GetEnumerator();
                    sourceList = null;
                }
            }

            public string Current
            {
                get { return currentElement; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public bool MoveNext()
            {
                if (sourceList == null)
                {
                    bool hasNext = stringIterator.MoveNext();
                    if (!hasNext)
                        return false;
                }

                int codepoint = current++;
                // we have the codepoint we need, but we may need to adjust the state
                if (current >= limit)
                {
                    if (item >= len)
                    {
                        stringIterator = sourceStrings.GetEnumerator();
                        sourceList = null;
                    }
                    else
                    {
                        current = sourceList[item++];
                        limit = sourceList[item++];
                    }
                }
                // Now return. Single code point is easy
                if (codepoint <= 0xFFFF)
                {
                    currentElement = new string(new char[] { (char)codepoint });
                    return true;
                }
                // But Java lacks a valueOfCodePoint, so we handle ourselves for speed
                // allocate a buffer the first time, to make conversion faster.
                if (buffer == null)
                {
                    buffer = new char[2];
                }
                // compute ourselves, to save tests and calls
                int offset = codepoint - Character.MIN_SUPPLEMENTARY_CODE_POINT;
                buffer[0] = (char)((int)((uint)(offset >> 10)) + Character.MIN_HIGH_SURROGATE);
                buffer[1] = (char)((offset & 0x3ff) + Character.MIN_LOW_SURROGATE);
                currentElement = new string(buffer);
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            ///* (non-Javadoc)
            // * @see java.util.Iterator#hasNext()
            // */
            //private bool HasNext()
            //{
            //    return sourceList != null || stringIterator.HasNext();
            //}

            ///* (non-Javadoc)
            // * @see java.util.Iterator#next()
            // */
            //private string Next()
            //{
            //    if (sourceList == null)
            //    {
            //        return stringIterator.next();
            //    }
            //    int codepoint = current++;
            //    // we have the codepoint we need, but we may need to adjust the state
            //    if (current >= limit)
            //    {
            //        if (item >= len)
            //        {
            //            stringIterator = sourceStrings.iterator();
            //            sourceList = null;
            //        }
            //        else
            //        {
            //            current = sourceList[item++];
            //            limit = sourceList[item++];
            //        }
            //    }
            //    // Now return. Single code point is easy
            //    if (codepoint <= 0xFFFF)
            //    {
            //        return String.valueOf((char)codepoint);
            //    }
            //    // But Java lacks a valueOfCodePoint, so we handle ourselves for speed
            //    // allocate a buffer the first time, to make conversion faster.
            //    if (buffer == null)
            //    {
            //        buffer = new char[2];
            //    }
            //    // compute ourselves, to save tests and calls
            //    int offset = codepoint - Character.MIN_SUPPLEMENTARY_CODE_POINT;
            //    buffer[0] = (char)((offset >>> 10) + Character.MIN_HIGH_SURROGATE);
            //    buffer[1] = (char)((offset & 0x3ff) + Character.MIN_LOW_SURROGATE);
            //    return String.valueOf(buffer);
            //}

            ///* (non-Javadoc)
            // * @see java.util.Iterator#remove()
            // */
            //@Override
            //    public void remove()
            //{
            //    throw new UnsupportedOperationException();
            //}
        }

        /**
         * @see #containsAll(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        public bool ContainsAll<T>(IEnumerable<T> collection) where T : class
        {
            foreach (T o in collection)
            {
                var csq = o.ConvertToCharSequence();
                if (csq != null)
                {
                    if (!Contains(csq))
                    {
                        return false;
                    }
                }
                else
                {
                    throw new ArgumentException("Only string, StringBuilder, char[], and ICharSequence types are allowed.");
                }
            }
            return true;
        }

        /**
         * @see #containsNone(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        public bool ContainsNone<T>(IEnumerable<T> collection) where T : class
        {
            foreach (T o in collection)
            {
                var csq = o.ConvertToCharSequence();
                if (csq != null)
                {
                    if (Contains(csq))
                    {
                        return false;
                    }
                }
                else
                {
                    throw new ArgumentException("Only string, StringBuilder, char[], and ICharSequence types are allowed.");
                }
            }
            return true;
        }

        /**
         * @see #containsAll(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        public bool ContainsSome<T>(IEnumerable<T> collection) where T : class
        {
            return !ContainsNone(collection);
        }

        /**
         * @see #addAll(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        // See ticket #11395, this is safe.
        public UnicodeSet AddAll<T>(params T[] collection) where T : class
        {
            CheckFrozen();
            foreach (T str in collection)
            {
                var csq = str.ConvertToCharSequence();
                if (csq != null)
                {
                    Add(csq);
                }
                else
                {
                    throw new ArgumentException("Only string, StringBuilder, char[], and ICharSequence types are allowed.");
                }
            }
            return this;
        }


        /**
         * @see #removeAll(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        public UnicodeSet RemoveAll<T>(IEnumerable<T> collection) where T : class
        {
            CheckFrozen();
            foreach (T o in collection)
            {
                var csq = o.ConvertToCharSequence();
                if (csq != null)
                {
                    Remove(csq);
                }
                else
                {
                    throw new ArgumentException("Only string, StringBuilder, char[], and ICharSequence types are allowed.");
                }

            }
            return this;
        }

        /**
         * @see #retainAll(com.ibm.icu.text.UnicodeSet)
         * @stable ICU 4.4
         */
        public UnicodeSet RetainAll<T>(IEnumerable<T> collection) where T : class
        {
            CheckFrozen();
            // TODO optimize
            UnicodeSet toRetain = new UnicodeSet();
            toRetain.AddAll(collection);
            RetainAll(toRetain);
            return this;
        }

        /**
         * Comparison style enums used by {@link UnicodeSet#compareTo(UnicodeSet, ComparisonStyle)}.
         * @stable ICU 4.4
         */
        public enum ComparisonStyle
        {
            /**
             * @stable ICU 4.4
             */
            SHORTER_FIRST,
            /**
             * @stable ICU 4.4
             */
            LEXICOGRAPHIC,
            /**
             * @stable ICU 4.4
             */
            LONGER_FIRST
        }

        /**
         * Compares UnicodeSets, where shorter come first, and otherwise lexigraphically
         * (according to the comparison of the first characters that differ).
         * @see java.lang.Comparable#compareTo(java.lang.Object)
         * @stable ICU 4.4
         */
        public virtual int CompareTo(UnicodeSet o)
        {
            return CompareTo(o, ComparisonStyle.SHORTER_FIRST);
        }
        /**
         * Compares UnicodeSets, in three different ways.
         * @see java.lang.Comparable#compareTo(java.lang.Object)
         * @stable ICU 4.4
         */
        public virtual int CompareTo(UnicodeSet o, ComparisonStyle style)
        {
            if (style != ComparisonStyle.LEXICOGRAPHIC)
            {
                int diff = Count - o.Count;
                if (diff != 0)
                {
                    return (diff < 0) == (style == ComparisonStyle.SHORTER_FIRST) ? -1 : 1;
                }
            }
            int result;
            for (int i = 0; ; ++i)
            {
                if (0 != (result = list[i] - o.list[i]))
                {
                    // if either list ran out, compare to the last string
                    if (list[i] == HIGH)
                    {
                        if (strings.Count == 0) return 1;
                        string item = strings.First();
                        return Compare(item.ToCharSequence(), o.list[i]);
                    }
                    if (o.list[i] == HIGH)
                    {
                        if (o.strings.Count == 0) return -1;
                        string item = o.strings.First();
                        int compareResult = Compare(item.ToCharSequence(), list[i]);
                        return compareResult > 0 ? -1 : compareResult < 0 ? 1 : 0; // Reverse the order.
                    }
                    // otherwise return the result if even index, or the reversal if not
                    return (i & 1) == 0 ? result : -result;
                }
                if (list[i] == HIGH)
                {
                    break;
                }
            }
            return Compare(strings, o.strings);
        }

        /**
         * @stable ICU 4.4
         */
        public virtual int CompareTo(IEnumerable<string> other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(string str, int codePoint)
        {
            return Compare(str.ToCharSequence(), codePoint);
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(StringBuilder str, int codePoint)
        {
            return Compare(str.ToCharSequence(), codePoint);
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(char[] str, int codePoint)
        {
            return Compare(str.ToCharSequence(), codePoint);
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        internal static int Compare(ICharSequence str, int codePoint)
        {
#pragma warning disable 612, 618
            return CharSequences.Compare(str, codePoint);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(int codePoint, string str)
        {
            return Compare(codePoint, str.ToCharSequence());
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(int codePoint, StringBuilder str)
        {
            return Compare(codePoint, str.ToCharSequence());
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static int Compare(int codePoint, char[] str)
        {
            return Compare(codePoint, str.ToCharSequence());
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        internal static int Compare(int codePoint, ICharSequence str)
        {
#pragma warning disable 612, 618
            return -CharSequences.Compare(str, codePoint);
#pragma warning restore 612, 618
        }


        /**
         * Utility to compare two iterables. Warning: the ordering in iterables is important. For Collections that are ordered,
         * like Lists, that is expected. However, Sets in Java violate Leibniz's law when it comes to iteration.
         * That means that sets can't be compared directly with this method, unless they are TreeSets without
         * (or with the same) comparator. Unfortunately, it is impossible to reliably detect in Java whether subclass of
         * Collection satisfies the right criteria, so it is left to the user to avoid those circumstances.
         * @stable ICU 4.4
         */
        public static int Compare<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IComparable<T>
        {
#pragma warning disable 612, 618
            return Compare(collection1.GetEnumerator(), collection2.GetEnumerator());
#pragma warning restore 612, 618
        }

        /**
         * Utility to compare two iterators. Warning: the ordering in iterables is important. For Collections that are ordered,
         * like Lists, that is expected. However, Sets in Java violate Leibniz's law when it comes to iteration.
         * That means that sets can't be compared directly with this method, unless they are TreeSets without
         * (or with the same) comparator. Unfortunately, it is impossible to reliably detect in Java whether subclass of
         * Collection satisfies the right criteria, so it is left to the user to avoid those circumstances.
         * @internal
         * @deprecated 
         */
        [Obsolete("This API is ICU internal only.")]
        public static int Compare<T>(IEnumerator<T> first, IEnumerator<T> other) where T : IComparable<T>
        {
            while (true)
            {
                if (!first.MoveNext())
                {
                    return other.MoveNext() ? -1 : 0;
                }
                else if (!other.MoveNext())
                {
                    return 1;
                }
                T item1 = first.Current;
                T item2 = other.Current;
                int result = item1.CompareTo(item2);
                if (result != 0)
                {
                    return result;
                }
            }
        }


        /**
         * Utility to compare two collections, optionally by size, and then lexicographically.
         * @stable ICU 4.4
         */
        public static int Compare<T>(ICollection<T> collection1, ICollection<T> collection2, ComparisonStyle style) where T : IComparable<T>
        {
            if (style != ComparisonStyle.LEXICOGRAPHIC)
            {
                int diff = collection1.Count - collection2.Count;
                if (diff != 0)
                {
                    return (diff < 0) == (style == ComparisonStyle.SHORTER_FIRST) ? -1 : 1;
                }
            }
            return Compare(collection1, collection2);
        }

        /**
         * Utility for adding the contents of an iterable to a collection.
         * @stable ICU 4.4
         */
        public static U AddAllTo<T, U>(IEnumerable<T> source, U target) where U : ICollection<T>
        {
            foreach (T item in source)
            {
                target.Add(item);
            }
            return target;
        }

        /**
         * Utility for adding the contents of an iterable to a collection.
         * @stable ICU 4.4
         */
        public static T[] AddAllTo<T>(IEnumerable<T> source, T[] target)
        {
            int i = 0;
            foreach (T item in source)
            {
                target[i++] = item;
            }
            return target;
        }

        /**
         * For iterating through the strings in the set. Example:
         * <pre>
         * for (String key : myUnicodeSet.strings()) {
         *   doSomethingWith(key);
         * }
         * </pre>
         * @stable ICU 4.4
         */
        public ICollection<string> Strings
        {
            get { return strings.ToUnmodifiableSet(); }
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(string s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(StringBuilder s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(char[] s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        internal static int GetSingleCodePoint(ICharSequence s)
        {
            return CharSequences.GetSingleCodePoint(s);
        }

        /**
         * Simplify the ranges in a Unicode set by merging any ranges that are only separated by characters in the dontCare set.
         * For example, the ranges: \\u2E80-\\u2E99\\u2E9B-\\u2EF3\\u2F00-\\u2FD5\\u2FF0-\\u2FFB\\u3000-\\u303E change to \\u2E80-\\u303E
         * if the dontCare set includes unassigned characters (for a particular version of Unicode).
         * @param dontCare Set with the don't-care characters for spanning
         * @return the input set, modified
         * @internal
         * @deprecated 
         */
        [Obsolete("This API is ICU internal only.")]
        public UnicodeSet AddBridges(UnicodeSet dontCare)
        {
            UnicodeSet notInInput = new UnicodeSet(this).Complement();
            for (UnicodeSetIterator it = new UnicodeSetIterator(notInInput); it.NextRange();)
            {
                if (it.Codepoint != 0 && it.Codepoint != UnicodeSetIterator.IS_STRING && it.CodepointEnd != 0x10FFFF && dontCare.Contains(it.Codepoint, it.CodepointEnd))
                {
                    Add(it.Codepoint, it.CodepointEnd);
                }
            }
            return this;
        }

        /// <summary>
        /// Find the first index at or after <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the first place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, length is returned.
        /// </summary>
        [Obsolete("This API is ICU internal only.Use span instead.")]
        public virtual int FindIn(string value, int fromIndex, bool findNot)
        {
            return FindIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the first index at or after <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the first place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, length is returned.
        /// </summary>
        [Obsolete("This API is ICU internal only.Use span instead.")]
        public virtual int FindIn(StringBuilder value, int fromIndex, bool findNot)
        {
            return FindIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the first index at or after <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the first place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, length is returned.
        /// </summary>
        [Obsolete("This API is ICU internal only.Use span instead.")]
        public virtual int FindIn(char[] value, int fromIndex, bool findNot)
        {
            return FindIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the first index at or after <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the first place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, length is returned.
        /// </summary>
        [Obsolete("This API is ICU internal only.Use span instead.")]
        internal virtual int FindIn(ICharSequence value, int fromIndex, bool findNot)
        {
            //TODO add strings, optimize, using ICU4C algorithms
            int cp;
            for (; fromIndex < value.Length; fromIndex += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(value, fromIndex);
                if (Contains(cp) != findNot)
                {
                    break;
                }
            }
            return fromIndex;
        }

        /// <summary>
        /// Find the last index before <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the last place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, -1 is returned.
        /// BEFORE index is not in the <see cref="UnicodeSet"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only. Use spanBack instead.")]
        public virtual int FindLastIn(string value, int fromIndex, bool findNot)
        {
            return FindLastIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the last index before <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the last place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, -1 is returned.
        /// BEFORE index is not in the <see cref="UnicodeSet"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only. Use spanBack instead.")]
        public virtual int FindLastIn(StringBuilder value, int fromIndex, bool findNot)
        {
            return FindLastIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the last index before <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the last place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, -1 is returned.
        /// BEFORE index is not in the <see cref="UnicodeSet"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only. Use spanBack instead.")]
        public virtual int FindLastIn(char[] value, int fromIndex, bool findNot)
        {
            return FindLastIn(value.ToCharSequence(), fromIndex, findNot);
        }

        /// <summary>
        /// Find the last index before <paramref name="fromIndex"/> where the <see cref="UnicodeSet"/> matches at that index.
        /// If <paramref name="findNot"/> is true, then reverse the sense of the match: find the last place where the <see cref="UnicodeSet"/> doesn't match.
        /// If there is no match, -1 is returned.
        /// BEFORE index is not in the <see cref="UnicodeSet"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only. Use spanBack instead.")]
        internal virtual int FindLastIn(ICharSequence value, int fromIndex, bool findNot)
        {
            //TODO add strings, optimize, using ICU4C algorithms
            int cp;
            fromIndex -= 1;
            for (; fromIndex >= 0; fromIndex -= UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(value, fromIndex);
                if (Contains(cp) != findNot)
                {
                    break;
                }
            }
            return fromIndex < 0 ? -1 : fromIndex;
        }

        /// <summary>
        /// Strips code points from source. If matches is true, script all that match <i>this</i>. If matches is false, then strip all that <i>don't</i> match.
        /// </summary>
        /// <param name="source">The source of the <see cref="ICharSequence"/> to strip from.</param>
        /// <param name="matches">A bool to either strip all that matches or don't match with the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>The string after it has been stripped.</returns>
        [Obsolete("This API is ICU internal only. Use replaceFrom.")]
        public virtual string StripFrom(string source, bool matches)
        {
            return StripFrom(source.ToCharSequence(), matches);
        }

        /// <summary>
        /// Strips code points from source. If matches is true, script all that match <i>this</i>. If matches is false, then strip all that <i>don't</i> match.
        /// </summary>
        /// <param name="source">The source of the <see cref="ICharSequence"/> to strip from.</param>
        /// <param name="matches">A bool to either strip all that matches or don't match with the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>The string after it has been stripped.</returns>
        [Obsolete("This API is ICU internal only. Use replaceFrom.")]
        public virtual string StripFrom(StringBuilder source, bool matches)
        {
            return StripFrom(source.ToCharSequence(), matches);
        }

        /// <summary>
        /// Strips code points from source. If matches is true, script all that match <i>this</i>. If matches is false, then strip all that <i>don't</i> match.
        /// </summary>
        /// <param name="source">The source of the <see cref="ICharSequence"/> to strip from.</param>
        /// <param name="matches">A bool to either strip all that matches or don't match with the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>The string after it has been stripped.</returns>
        [Obsolete("This API is ICU internal only. Use replaceFrom.")]
        public virtual string StripFrom(char[] source, bool matches)
        {
            return StripFrom(source.ToCharSequence(), matches);
        }

        /// <summary>
        /// Strips code points from source. If matches is true, script all that match <i>this</i>. If matches is false, then strip all that <i>don't</i> match.
        /// </summary>
        /// <param name="source">The source of the <see cref="ICharSequence"/> to strip from.</param>
        /// <param name="matches">A bool to either strip all that matches or don't match with the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>The string after it has been stripped.</returns>
        [Obsolete("This API is ICU internal only. Use replaceFrom.")]
        internal virtual string StripFrom(ICharSequence source, bool matches)
        {
            StringBuilder result = new StringBuilder();
            for (int pos = 0; pos < source.Length;)
            {
                int inside = FindIn(source, pos, !matches);
                result.Append(source.SubSequence(pos, inside));
                pos = FindIn(source, inside, matches); // get next start
            }
            return result.ToString();
        }

        /**
         * Argument values for whether span() and similar functions continue while the current character is contained vs.
         * not contained in the set.
         * <p>
         * The functionality is straightforward for sets with only single code points, without strings (which is the common
         * case):
         * <ul>
         * <li>CONTAINED and SIMPLE work the same.
         * <li>CONTAINED and SIMPLE are inverses of NOT_CONTAINED.
         * <li>span() and spanBack() partition any string the
         * same way when alternating between span(NOT_CONTAINED) and span(either "contained" condition).
         * <li>Using a
         * complemented (inverted) set and the opposite span conditions yields the same results.
         * </ul>
         * When a set contains multi-code point strings, then these statements may not be true, depending on the strings in
         * the set (for example, whether they overlap with each other) and the string that is processed. For a set with
         * strings:
         * <ul>
         * <li>The complement of the set contains the opposite set of code points, but the same set of strings.
         * Therefore, complementing both the set and the span conditions may yield different results.
         * <li>When starting spans
         * at different positions in a string (span(s, ...) vs. span(s+1, ...)) the ends of the spans may be different
         * because a set string may start before the later position.
         * <li>span(SIMPLE) may be shorter than
         * span(CONTAINED) because it will not recursively try all possible paths. For example, with a set which
         * contains the three strings "xy", "xya" and "ax", span("xyax", CONTAINED) will return 4 but span("xyax",
         * SIMPLE) will return 3. span(SIMPLE) will never be longer than span(CONTAINED).
         * <li>With either "contained" condition, span() and spanBack() may partition a string in different ways. For example,
         * with a set which contains the two strings "ab" and "ba", and when processing the string "aba", span() will yield
         * contained/not-contained boundaries of { 0, 2, 3 } while spanBack() will yield boundaries of { 0, 1, 3 }.
         * </ul>
         * Note: If it is important to get the same boundaries whether iterating forward or backward through a string, then
         * either only span() should be used and the boundaries cached for backward operation, or an ICU BreakIterator could
         * be used.
         * <p>
         * Note: Unpaired surrogates are treated like surrogate code points. Similarly, set strings match only on code point
         * boundaries, never in the middle of a surrogate pair.
         *
         * @stable ICU 4.4
         */
        public enum SpanCondition
        {
            /**
             * Continues a span() while there is no set element at the current position.
             * Increments by one code point at a time.
             * Stops before the first set element (character or string).
             * (For code points only, this is like while contains(current)==false).
             * <p>
             * When span() returns, the substring between where it started and the position it returned consists only of
             * characters that are not in the set, and none of its strings overlap with the span.
             *
             * @stable ICU 4.4
             */
            NOT_CONTAINED,

            /**
             * Spans the longest substring that is a concatenation of set elements (characters or strings).
             * (For characters only, this is like while contains(current)==true).
             * <p>
             * When span() returns, the substring between where it started and the position it returned consists only of set
             * elements (characters or strings) that are in the set.
             * <p>
             * If a set contains strings, then the span will be the longest substring for which there
             * exists at least one non-overlapping concatenation of set elements (characters or strings).
             * This is equivalent to a POSIX regular expression for <code>(OR of each set element)*</code>.
             * (Java/ICU/Perl regex stops at the first match of an OR.)
             *
             * @stable ICU 4.4
             */
            CONTAINED,

            /**
             * Continues a span() while there is a set element at the current position.
             * Increments by the longest matching element at each position.
             * (For characters only, this is like while contains(current)==true).
             * <p>
             * When span() returns, the substring between where it started and the position it returned consists only of set
             * elements (characters or strings) that are in the set.
             * <p>
             * If a set only contains single characters, then this is the same as CONTAINED.
             * <p>
             * If a set contains strings, then the span will be the longest substring with a match at each position with the
             * longest single set element (character or string).
             * <p>
             * Use this span condition together with other longest-match algorithms, such as ICU converters
             * (ucnv_getUnicodeSet()).
             *
             * @stable ICU 4.4
             */
            SIMPLE,

            /**
             * One more than the last span condition.
             *
             * @stable ICU 4.4
             */
            CONDITION_COUNT
        }

        /**
         * Get the default symbol table. Null means ordinary processing. For internal use only.
         * @return the symbol table
         * @internal
         * @deprecated 
         */
        [Obsolete("This API is ICU internal only.")]
        public static XSymbolTable DefaultXSymbolTable
        {
            get { return XSYMBOL_TABLE; }
        }

        /**
         * Set the default symbol table. Null means ordinary processing. For internal use only. Will affect all subsequent parsing
         * of UnicodeSets.
         * <p>
         * WARNING: If this function is used with a UnicodeProperty, and the
         * Unassigned characters (gc=Cn) are different than in ICU other than in ICU, you MUST call
         * {@code UnicodeProperty.ResetCacheProperties} afterwards. If you then call {@code UnicodeSet.setDefaultXSymbolTable}
         * with null to clear the value, you MUST also call {@code UnicodeProperty.ResetCacheProperties}.
         *
         * @param xSymbolTable the new default symbol table.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static void SetDefaultXSymbolTable(XSymbolTable xSymbolTable) // ICU4N NOTE: Has side-effect, so isn't a good property candidate
        {
            INCLUSIONS = null; // If the properties override inclusions, these have to be regenerated.
            XSYMBOL_TABLE = xSymbolTable;
        }
    }
}
