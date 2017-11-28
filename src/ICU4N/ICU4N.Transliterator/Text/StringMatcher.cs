using ICU4N.Impl;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// An object that matches a fixed input string, implementing the
    /// <see cref="IUnicodeMatcher"/> API.  This object also implements the
    /// <see cref="IUnicodeReplacer"/> API, allowing it to emit the matched text as
    /// output.  Since the match text may contain flexible match elements,
    /// such as <see cref="UnicodeSet"/>s, the emitted text is not the match pattern, but
    /// instead a substring of the actual matched text.  Following
    /// convention, the output text is the leftmost match seen up to this
    /// point.
    /// </summary>
    /// <remarks>
    /// A StringMatcher may represent a segment, in which case it has a
    /// positive segment number.  This affects how the matcher converts
    /// itself to a pattern but does not otherwise affect its function.
    /// <para/>
    /// A StringMatcher that is not a segment should not be used as a
    /// <see cref="IUnicodeReplacer"/>.
    /// </remarks>
    internal class StringMatcher : IUnicodeMatcher, IUnicodeReplacer
    {
        /// <summary>
        /// The text to be matched.
        /// </summary>
        private string pattern;

        /// <summary>
        /// Start offset, in the match text, of the <em>rightmost</em>
        /// match.
        /// </summary>
        private int matchStart;

        /// <summary>
        /// Limit offset, in the match text, of the <em>rightmost</em>
        /// match.
        /// </summary>
        private int matchLimit;

        /// <summary>
        /// The segment number, 1-based, or 0 if not a segment.
        /// </summary>
        private int segmentNumber;

        /// <summary>
        /// Context object that maps stand-ins to matcher and replacer
        /// objects.
        /// </summary>
        private readonly RuleBasedTransliterator.Data data;

        /// <summary>
        /// Construct a matcher that matches the given pattern string.
        /// </summary>
        /// <param name="theString">The pattern to be matched, possibly containing
        /// stand-ins that represent nested <see cref="IUnicodeMatcher"/> objects.</param>
        /// <param name="segmentNum">The segment number from 1..n, or 0 if this is
        /// not a segment.</param>
        /// <param name="theData">Context object mapping stand-ins to <see cref="IUnicodeMatcher"/> objects.</param>
        public StringMatcher(string theString,
                             int segmentNum,
                             RuleBasedTransliterator.Data theData)
        {
            data = theData;
            pattern = theString;
            matchStart = matchLimit = -1;
            segmentNumber = segmentNum;
        }

        /// <summary>
        /// Construct a matcher that matches a substring of the given
        /// pattern string.
        /// </summary>
        /// <param name="theString">The pattern to be matched, possibly containing
        /// stand-ins that represent nested <see cref="IUnicodeMatcher"/> objects.</param>
        /// <param name="start">First character of <paramref name="theString"/> to be matched.</param>
        /// <param name="limit">Index after the last character of <paramref name="theString"/> to be matched.</param>
        /// <param name="segmentNum">The segment number from 1..n, or 0 if this is not a segment.</param>
        /// <param name="theData">context object mapping stand-ins to <see cref="IUnicodeMatcher"/> objects.</param>
        public StringMatcher(string theString,
                             int start,
                             int limit,
                             int segmentNum,
                             RuleBasedTransliterator.Data theData)
            : this(theString.Substring(start, limit - start), segmentNum, theData) // ICU4N: Corrected 2nd substring parameter
        {
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/>
        /// </summary>
        public virtual int Matches(IReplaceable text,
                           int[] offset,
                           int limit,
                           bool incremental)
        {
            // Note (1): We process text in 16-bit code units, rather than
            // 32-bit code points.  This works because stand-ins are
            // always in the BMP and because we are doing a literal match
            // operation, which can be done 16-bits at a time.
            int i;
            int[] cursor = new int[] { offset[0] };
            if (limit < cursor[0])
            {
                // Match in the reverse direction
                for (i = pattern.Length - 1; i >= 0; --i)
                {
                    char keyChar = pattern[i]; // OK; see note (1) above
                    IUnicodeMatcher subm = data.LookupMatcher(keyChar);
                    if (subm == null)
                    {
                        if (cursor[0] > limit &&
                            keyChar == text[cursor[0]])
                        { // OK; see note (1) above
                            --cursor[0];
                        }
                        else
                        {
                            return UnicodeMatcher.U_MISMATCH;
                        }
                    }
                    else
                    {
                        int m =
                            subm.Matches(text, cursor, limit, incremental);
                        if (m != UnicodeMatcher.U_MATCH)
                        {
                            return m;
                        }
                    }
                }
                // Record the match position, but adjust for a normal
                // forward start, limit, and only if a prior match does not
                // exist -- we want the rightmost match.
                if (matchStart < 0)
                {
                    matchStart = cursor[0] + 1;
                    matchLimit = offset[0] + 1;
                }
            }
            else
            {
                for (i = 0; i < pattern.Length; ++i)
                {
                    if (incremental && cursor[0] == limit)
                    {
                        // We've reached the context limit without a mismatch and
                        // without completing our match.
                        return UnicodeMatcher.U_PARTIAL_MATCH;
                    }
                    char keyChar = pattern[i]; // OK; see note (1) above
                    IUnicodeMatcher subm = data.LookupMatcher(keyChar);
                    if (subm == null)
                    {
                        // Don't need the cursor < limit check if
                        // incremental is true (because it's done above); do need
                        // it otherwise.
                        if (cursor[0] < limit &&
                            keyChar == text[cursor[0]])
                        { // OK; see note (1) above
                            ++cursor[0];
                        }
                        else
                        {
                            return UnicodeMatcher.U_MISMATCH;
                        }
                    }
                    else
                    {
                        int m =
                            subm.Matches(text, cursor, limit, incremental);
                        if (m != UnicodeMatcher.U_MATCH)
                        {
                            return m;
                        }
                    }
                }
                // Record the match position
                matchStart = offset[0];
                matchLimit = cursor[0];
            }

            offset[0] = cursor[0];
            return UnicodeMatcher.U_MATCH;
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/>
        /// </summary>
        public virtual string ToPattern(bool escapeUnprintable)
        {
            StringBuffer result = new StringBuffer();
            StringBuffer quoteBuf = new StringBuffer();
            if (segmentNumber > 0)
            { // i.e., if this is a segment
                result.Append('(');
            }
            for (int i = 0; i < pattern.Length; ++i)
            {
                char keyChar = pattern[i]; // OK; see note (1) above
                IUnicodeMatcher m = data.LookupMatcher(keyChar);
                if (m == null)
                {
                    Utility.AppendToRule(result, keyChar, false, escapeUnprintable, quoteBuf);
                }
                else
                {
                    Utility.AppendToRule(result, m.ToPattern(escapeUnprintable),
                                         true, escapeUnprintable, quoteBuf);
                }
            }
            if (segmentNumber > 0)
            { // i.e., if this is a segment
                result.Append(')');
            }
            // Flush quoteBuf out to result
            Utility.AppendToRule(result, -1,
                                 true, escapeUnprintable, quoteBuf);
            return result.ToString();
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/>
        /// </summary>
        public virtual bool MatchesIndexValue(int v)
        {
            if (pattern.Length == 0)
            {
                return true;
            }
            int c = UTF16.CharAt(pattern, 0);
            IUnicodeMatcher m = data.LookupMatcher(c);
            return (m == null) ? ((c & 0xFF) == v) : m.MatchesIndexValue(v);
        }

        /// <summary>
        /// Implementation of <see cref="IUnicodeMatcher"/> API.  Union the set of all
        /// characters that may be matched by this object into the given
        /// set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the source characters.</param>
        public virtual void AddMatchSetTo(UnicodeSet toUnionTo)
        {
            int ch;
            for (int i = 0; i < pattern.Length; i += UTF16.GetCharCount(ch))
            {
                ch = UTF16.CharAt(pattern, i);
                IUnicodeMatcher matcher = data.LookupMatcher(ch);
                if (matcher == null)
                {
                    toUnionTo.Add(ch);
                }
                else
                {
                    matcher.AddMatchSetTo(toUnionTo);
                }
            }
        }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual int Replace(IReplaceable text,
                           int start,
                           int limit,
                           int[] cursor)
        {
            int outLen = 0;

            // Copy segment with out-of-band data
            int dest = limit;
            // If there was no match, that means that a quantifier
            // matched zero-length.  E.g., x (a)* y matched "xy".
            if (matchStart >= 0)
            {
                if (matchStart != matchLimit)
                {
                    text.Copy(matchStart, matchLimit, dest);
                    outLen = matchLimit - matchStart;
                }
            }

            text.Replace(start, limit, ""); // delete original text

            return outLen;
        }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual string ToReplacerPattern(bool escapeUnprintable)
        {
            // assert(segmentNumber > 0);
            StringBuffer rule = new StringBuffer("$");
            Utility.AppendNumber(rule, segmentNumber, 10, 1);
            return rule.ToString();
        }

        /// <summary>
        /// Remove any match data.  This must be called before performing a
        /// set of matches with this segment.
        /// </summary>
        public virtual void ResetMatch()
        {
            matchStart = matchLimit = -1;
        }

        /// <summary>
        /// Union the set of all characters that may output by this object
        /// into the given set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the output characters.</param>
        public virtual void AddReplacementSetTo(UnicodeSet toUnionTo)
        {
            // The output of this replacer varies; it is the source text between
            // matchStart and matchLimit.  Since this varies depending on the
            // input text, we can't compute it here.  We can either do nothing
            // or we can add ALL characters to the set.  It's probably more useful
            // to do nothing.
        }
    }
}
