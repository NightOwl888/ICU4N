using ICU4N.Impl;
using J2N.Text;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliteration rule used by
    /// <see cref="RuleBasedTransliterator"/>.
    /// <see cref="TransliterationRule"/> is an immutable object.
    /// </summary>
    /// <remarks>
    /// A rule consists of an input pattern and an output string.  When
    /// the input pattern is matched, the output string is emitted.  The
    /// input pattern consists of zero or more characters which are matched
    /// exactly (the key) and optional context.  Context must match if it
    /// is specified.  Context may be specified before the key, after the
    /// key, or both.  The key, preceding context, and following context
    /// may contain variables.  Variables represent a set of Unicode
    /// characters, such as the letters <i>a</i> through <i>z</i>.
    /// Variables are detected by looking up each character in a supplied
    /// variable list to see if it has been so defined.
    /// <para/>
    /// A rule may contain segments in its input string and segment
    /// references in its output string.  A segment is a substring of the
    /// input pattern, indicated by an offset and limit.  The segment may
    /// be in the preceding or following context.  It may not span a
    /// context boundary.  A segment reference is a special character in
    /// the output string that causes a segment of the input string (not
    /// the input pattern) to be copied to the output string.  The range of
    /// special characters that represent segment references is defined by
    /// <see cref="RuleBasedTransliterator.Data"/>.
    /// <para/>
    /// Example: The rule "([a-z]) . ([0-9]) > $2 . $1" will change the input
    /// string "abc.123" to "ab1.c23".
    /// <para/>
    /// Copyright &#169; IBM Corporation 1999.  All rights reserved.
    /// </remarks>
    /// <author>Alan Liu</author>
    internal class TransliterationRule
    {
        // TODO Eliminate the pattern and keyLength data members.  They
        // are used only by masks() and getIndexValue() which are called
        // only during build time, not during run-time.  Perhaps these
        // methods and pattern/keyLength can be isolated into a separate
        // object.

        /// <summary>
        /// The match that must occur before the <see cref="key"/>, or null if there is no
        /// preceding context.
        /// </summary>
        private StringMatcher anteContext;

        /// <summary>
        /// The matcher object for the key.  If null, then the key is empty.
        /// </summary>
        private StringMatcher key;

        /// <summary>
        /// The match that must occur after the <see cref="key"/>, or null if there is no
        /// following context.
        /// </summary>
        private StringMatcher postContext;

        /// <summary>
        /// The object that performs the replacement if the key,
        /// <see cref="anteContext"/>, and <see cref="postContext"/> are matched.  Never null.
        /// </summary>
        private IUnicodeReplacer output;

        /// <summary>
        /// The string that must be matched, consisting of the <see cref="anteContext"/>, <see cref="key"/>,
        /// and <see cref="postContext"/>, concatenated together, in that order.  Some components
        /// may be empty (zero length).
        /// </summary>
        /// <seealso cref="anteContextLength"/>
        /// <seealso cref="keyLength"/>
        private string pattern;

        /// <summary>
        /// An array of matcher objects corresponding to the input pattern
        /// segments.  If there are no segments this is null.  N.B. This is
        /// a <see cref="IUnicodeMatcher"/> for generality, but in practice it is always a
        /// <see cref="StringMatcher"/>.  In the future we may generalize this, but for
        /// now we sometimes cast down to <see cref="StringMatcher"/>.
        /// </summary>
        internal IUnicodeMatcher[] segments;

        /// <summary>
        /// The length of the string that must match before the key.  If
        /// zero, then there is no matching requirement before the key.
        /// Substring [0,anteContextLength) of pattern is the anteContext.
        /// </summary>
        private int anteContextLength;

        /// <summary>
        /// The length of the key.  Substring [anteContextLength,
        /// anteContextLength + keyLength) is the key.
        /// </summary>
        private int keyLength;

        /// <summary>
        /// Miscellaneous attributes.
        /// </summary>
        internal byte flags;

        /// <summary>
        /// Flag attributes.
        /// </summary>
        internal const int ANCHOR_START = 1;
        internal const int ANCHOR_END = 2;

        /// <summary>
        /// An alias pointer to the data for this rule.  The data provides
        /// lookup services for matchers and segments.
        /// </summary>
#pragma warning disable 612, 618
        private readonly RuleBasedTransliterator.Data data;
#pragma warning restore 612, 618

        /// <summary>
        /// Construct a new rule with the given <paramref name="input"/>, <paramref name="output"/> text, and other
        /// attributes.  A cursor position <paramref name="cursorPos"/> may be specified for the <paramref name="output"/> text.
        /// </summary>
        /// <param name="input">Input string, including key and optional ante and post context.</param>
        /// <param name="anteContextPos">Offset into <paramref name="input"/> to end of ante context, or -1 if
        /// none.  Must be &lt;= input.Length if not -1.</param>
        /// <param name="postContextPos">Offset into input to start of post context, or -1
        /// if none.  Must be &lt;= input.Length if not -1, and must be >=
        /// <paramref name="anteContextPos"/>.</param>
        /// <param name="output">Output string.</param>
        /// <param name="cursorPos">Offset into <paramref name="output"/> at which cursor is located, or -1 if
        /// none.  If less than zero, then the cursor is placed after the
        /// <paramref name="output"/>; that is, -1 is equivalent to <c>output.Length</c>.  If greater than
        /// <c>output.Length</c> then an exception is thrown.</param>
        /// <param name="cursorOffset">An offset to be added to <paramref name="cursorPos"/> to position the
        /// cursor either in the ante context, if &lt; 0, or in the post context, if &gt;
        /// 0.  For example, the rule "abc{def} > | @@@ xyz;" changes "def" to
        /// "xyz" and moves the cursor to before "a".  It would have a <paramref name="cursorOffset"/>
        /// of -3.</param>
        /// <param name="segs">Array of <see cref="IUnicodeMatcher"/> corresponding to input pattern
        /// segments, or null if there are none.</param>
        /// <param name="anchorStart"><c>true</c> if the the rule is anchored on the left to
        /// the context start.</param>
        /// <param name="anchorEnd"><c>true</c> if the rule is anchored on the right to the
        /// context limit.</param>
        /// <param name="theData"></param>
        public TransliterationRule(string input,
                                   int anteContextPos, int postContextPos,
                                   string output,
                                   int cursorPos, int cursorOffset,
                                   IUnicodeMatcher[] segs,
                                   bool anchorStart, bool anchorEnd,
#pragma warning disable 612, 618
                                   RuleBasedTransliterator.Data theData)
#pragma warning restore 612, 618
        {
            data = theData;

            // Do range checks only when warranted to save time
            if (anteContextPos < 0)
            {
                anteContextLength = 0;
            }
            else
            {
                if (anteContextPos > input.Length)
                {
                    throw new ArgumentException("Invalid ante context");
                }
                anteContextLength = anteContextPos;
            }
            if (postContextPos < 0)
            {
                keyLength = input.Length - anteContextLength;
            }
            else
            {
                if (postContextPos < anteContextLength ||
                    postContextPos > input.Length)
                {
                    throw new ArgumentException("Invalid post context");
                }
                keyLength = postContextPos - anteContextLength;
            }
            if (cursorPos < 0)
            {
                cursorPos = output.Length;
            }
            else if (cursorPos > output.Length)
            {
                throw new ArgumentException("Invalid cursor position");
            }

            // We don't validate the segments array.  The caller must
            // guarantee that the segments are well-formed (that is, that
            // all $n references in the output refer to indices of this
            // array, and that no array elements are null).
            this.segments = segs;

            pattern = input;
            flags = 0;
            if (anchorStart)
            {
                flags |= (byte)ANCHOR_START;
            }
            if (anchorEnd)
            {
                flags |= (byte)ANCHOR_END;
            }

            anteContext = null;
            if (anteContextLength > 0)
            {
                anteContext = new StringMatcher(pattern.Substring(0, anteContextLength), // ICU4N: Checked 2nd parameter
                                                0, data);
            }

            key = null;
            if (keyLength > 0)
            {
                key = new StringMatcher(pattern.Substring(anteContextLength, keyLength), // ICU4N: (anteContextLength + keyLength) - anteContextLength == keyLength
                                        0, data);
            }

            int postContextLength = pattern.Length - keyLength - anteContextLength;
            postContext = null;
            if (postContextLength > 0)
            {
                postContext = new StringMatcher(pattern.Substring(anteContextLength + keyLength),
                                                0, data);
            }

            this.output = new StringReplacer(output, cursorPos + cursorOffset, data);
        }

        /// <summary>
        /// Gets the preceding context length.  This property is needed to
        /// support <see cref="Transliterator.MaximumContextLength"/>.
        /// </summary>
        public virtual int AnteContextLength
            => anteContextLength + (((flags & ANCHOR_START) != 0) ? 1 : 0);

        /// <summary>
        /// Internal method.  Returns 8-bit index value for this rule.
        /// This is the low byte of the first character of the key,
        /// unless the first character of the key is a set.  If it's a
        /// set, or otherwise can match multiple keys, the index value is -1.
        /// </summary>
        internal int GetIndexValue()
        {
            if (anteContextLength == pattern.Length)
            {
                // A pattern with just ante context {such as foo)>bar} can
                // match any key.
                return -1;
            }
            int c = UTF16.CharAt(pattern, anteContextLength);
            return data.LookupMatcher(c) == null ? (c & 0xFF) : -1;
        }

        /// <summary>
        /// Internal method.  Returns true if this rule matches the given
        /// index value.  The index value is an 8-bit integer, 0..255,
        /// representing the low byte of the first character of the key.
        /// It matches this rule if it matches the first character of the
        /// key, or if the first character of the key is a set, and the set
        /// contains any character with a low byte equal to the index
        /// value.  If the rule contains only ante context, as in foo)>bar,
        /// then it will match any key.
        /// </summary>
        internal bool MatchesIndexValue(int v)
        {
            // Delegate to the key, or if there is none, to the postContext.
            // If there is neither then we match any key; return true.
            IUnicodeMatcher m = (key != null) ? key : postContext;
            return (m != null) ? m.MatchesIndexValue(v) : true;
        }

        /// <summary>
        /// Return true if this rule masks another rule.  If r1 masks <paramref name="r2"/> then
        /// r1 matches any input string that <paramref name="r2"/> matches.  If r1 masks <paramref name="r2"/> and <paramref name="r2"/> masks
        /// r1 then r1 == r2.  Examples: "a>x" masks "ab>y".  "a>x" masks "a[b]>y".
        /// "[c]a>x" masks "[dc]a>y".
        /// </summary>
        public virtual bool Masks(TransliterationRule r2)
        {
            /* Rule r1 masks rule r2 if the string formed of the
             * antecontext, key, and postcontext overlaps in the following
             * way:
             *
             * r1:      aakkkpppp
             * r2:     aaakkkkkpppp
             *            ^
             *
             * The strings must be aligned at the first character of the
             * key.  The length of r1 to the left of the alignment point
             * must be <= the length of r2 to the left; ditto for the
             * right.  The characters of r1 must equal (or be a superset
             * of) the corresponding characters of r2.  The superset
             * operation should be performed to check for UnicodeSet
             * masking.
             *
             * Anchors:  Two patterns that differ only in anchors only
             * mask one another if they are exactly equal, and r2 has
             * all the anchors r1 has (optionally, plus some).  Here Y
             * means the row masks the column, N means it doesn't.
             *
             *         ab   ^ab    ab$  ^ab$
             *   ab    Y     Y     Y     Y
             *  ^ab    N     Y     N     Y
             *   ab$   N     N     Y     Y
             *  ^ab$   N     N     N     Y
             *
             * Post context: {a}b masks ab, but not vice versa, since {a}b
             * matches everything ab matches, and {a}b matches {|a|}b but ab
             * does not.  Pre context is different (a{b} does not align with
             * ab).
             */

            /* LIMITATION of the current mask algorithm: Some rule
             * maskings are currently not detected.  For example,
             * "{Lu}]a>x" masks "A]a>y".  This can be added later. TODO
             */

            int len = pattern.Length;
            int left = anteContextLength;
            int left2 = r2.anteContextLength;
            int right = pattern.Length - left;
            int right2 = r2.pattern.Length - left2;

            // TODO Clean this up -- some logic might be combinable with the
            // next statement.

            // Test for anchor masking
            if (left == left2 && right == right2 &&
                keyLength <= r2.keyLength &&
                r2.pattern.RegionMatches(0, pattern, 0, len, StringComparison.Ordinal))
            {
                // The following boolean logic implements the table above
                return (flags == r2.flags) ||
                    (!((flags & ANCHOR_START) != 0) && !((flags & ANCHOR_END) != 0)) ||
                    (((r2.flags & ANCHOR_START) != 0) && ((r2.flags & ANCHOR_END) != 0));
            }

            return left <= left2 &&
                (right < right2 ||
                 (right == right2 && keyLength <= r2.keyLength)) &&
                r2.pattern.RegionMatches(left2 - left, pattern, 0, len, StringComparison.Ordinal);
        }

        internal static int PosBefore(IReplaceable str, int pos)
        {
            return (pos > 0) ?
                pos - UTF16.GetCharCount(str.Char32At(pos - 1)) :
                pos - 1;
        }

        internal static int PosAfter(IReplaceable str, int pos)
        {
            return (pos >= 0 && pos < str.Length) ?
                pos + UTF16.GetCharCount(str.Char32At(pos)) :
                pos + 1;
        }

        /// <summary>
        /// Attempt a match and replacement at the given <paramref name="position"/>.  Return
        /// the degree of match between this rule and the given <paramref name="text"/>text.  The
        /// degree of match may be mismatch, a partial match, or a full
        /// match.  A mismatch means at least one character of the <paramref name="text"/>
        /// does not match the context or key.  A partial match means some
        /// context and key characters match, but the text is not long
        /// enough to match all of them.  A full match means all context
        /// and key characters match.
        /// <para/>
        /// If a full match is obtained, perform a replacement, update <paramref name="position"/>,
        /// and return <see cref="MatchDegree.Match"/>.  Otherwise both <paramref name="text"/> 
        /// and <paramref name="position"/> are unchanged.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="position">The position indices.</param>
        /// <param name="incremental">If <c>true</c>, test for partial matches that may
        /// be completed by additional text inserted at <c>position.Limit</c>.</param>
        /// <returns>One of <see cref="MatchDegree.Mismatch"/>, <see cref="MatchDegree.PartialMatch"/>, or <see cref="MatchDegree.Match"/>. If
        /// <paramref name="incremental"/> is <c>false</c> then <see cref="MatchDegree.PartialMatch"/> will not be returned.</returns>
        public virtual MatchDegree MatchAndReplace(IReplaceable text,
                                   TransliterationPosition position,
                                   bool incremental)
        {
            // Matching and replacing are done in one method because the
            // replacement operation needs information obtained during the
            // match.  Another way to do this is to have the match method
            // create a match result struct with relevant offsets, and to pass
            // this into the replace method.

            // ============================ MATCH ===========================

            // Reset segment match data
            if (segments != null)
            {
                for (int i = 0; i < segments.Length; ++i)
                {
                    ((StringMatcher)segments[i]).ResetMatch();
                }
            }

            int keyLimit;
            int[] intRef = new int[1];

            // ------------------------ Ante Context ------------------------

            // A mismatch in the ante context, or with the start anchor,
            // is an outright U_MISMATCH regardless of whether we are
            // incremental or not.
            int oText; // offset into 'text'
            int minOText;

            // Note (1): We process text in 16-bit code units, rather than
            // 32-bit code points.  This works because stand-ins are
            // always in the BMP and because we are doing a literal match
            // operation, which can be done 16-bits at a time.

            int anteLimit = PosBefore(text, position.ContextStart);

            MatchDegree match;

            // Start reverse match at char before pos.start
            intRef[0] = PosBefore(text, position.Start);

            if (anteContext != null)
            {
                match = anteContext.Matches(text, intRef, anteLimit, false);
                if (match != MatchDegree.Match)
                {
                    return MatchDegree.Mismatch;
                }
            }

            oText = intRef[0];

            minOText = PosAfter(text, oText);

            // ------------------------ Start Anchor ------------------------

            if (((flags & ANCHOR_START) != 0) && oText != anteLimit)
            {
                return MatchDegree.Mismatch;
            }

            // -------------------- Key and Post Context --------------------

            intRef[0] = position.Start;

            if (key != null)
            {
                match = key.Matches(text, intRef, position.Limit, incremental);
                if (match != MatchDegree.Match)
                {
                    return match;
                }
            }

            keyLimit = intRef[0];

            if (postContext != null)
            {
                if (incremental && keyLimit == position.Limit)
                {
                    // The key matches just before pos.limit, and there is
                    // a postContext.  Since we are in incremental mode,
                    // we must assume more characters may be inserted at
                    // pos.limit -- this is a partial match.
                    return MatchDegree.PartialMatch;
                }

                match = postContext.Matches(text, intRef, position.ContextLimit, incremental);
                if (match != MatchDegree.Match)
                {
                    return match;
                }
            }

            oText = intRef[0];

            // ------------------------- Stop Anchor ------------------------

            if (((flags & ANCHOR_END)) != 0)
            {
                if (oText != position.ContextLimit)
                {
                    return MatchDegree.Mismatch;
                }
                if (incremental)
                {
                    return MatchDegree.PartialMatch;
                }
            }

            // =========================== REPLACE ==========================

            // We have a full match.  The key is between pos.start and
            // keyLimit.

            int newLength = output.Replace(text, position.Start, keyLimit, intRef);
            int lenDelta = newLength - (keyLimit - position.Start);
            int newStart = intRef[0];

            oText += lenDelta;
            position.Limit += lenDelta;
            position.ContextLimit += lenDelta;
            // Restrict new value of start to [minOText, min(oText, pos.limit)].
            position.Start = Math.Max(minOText, Math.Min(Math.Min(oText, position.Limit), newStart));
            return MatchDegree.Match;
        }

        /// <summary>
        /// Create a source string that represents this rule.  Append it to the
        /// given string.
        /// </summary>
        public virtual string ToRule(bool escapeUnprintable)
        {
            // int i;

            StringBuffer rule = new StringBuffer();

            // Accumulate special characters (and non-specials following them)
            // into quoteBuf.  Append quoteBuf, within single quotes, when
            // a non-quoted element must be inserted.
            StringBuffer quoteBuf = new StringBuffer();

            // Do not emit the braces '{' '}' around the pattern if there
            // is neither anteContext nor postContext.
            bool emitBraces =
                (anteContext != null) || (postContext != null);

            // Emit start anchor
            if ((flags & ANCHOR_START) != 0)
            {
                rule.Append('^');
            }

            // Emit the input pattern
            Utility.AppendToRule(rule, anteContext, escapeUnprintable, quoteBuf);

            if (emitBraces)
            {
                Utility.AppendToRule(rule, '{', true, escapeUnprintable, quoteBuf);
            }

            Utility.AppendToRule(rule, key, escapeUnprintable, quoteBuf);

            if (emitBraces)
            {
                Utility.AppendToRule(rule, '}', true, escapeUnprintable, quoteBuf);
            }

            Utility.AppendToRule(rule, postContext, escapeUnprintable, quoteBuf);

            // Emit end anchor
            if ((flags & ANCHOR_END) != 0)
            {
                rule.Append('$');
            }

            Utility.AppendToRule(rule, " > ", true, escapeUnprintable, quoteBuf);

            // Emit the output pattern

            Utility.AppendToRule(rule, output.ToReplacerPattern(escapeUnprintable),
                         true, escapeUnprintable, quoteBuf);

            Utility.AppendToRule(rule, ';', true, escapeUnprintable, quoteBuf);

            return rule.ToString();
        }

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        public override string ToString()
        {
            return '{' + ToRule(true) + '}';
        }

        /// <summary>
        /// Find the source and target sets, subject to the input filter.
        /// There is a known issue with filters containing multiple characters.
        /// </summary>
        // TODO: Problem: the rule is [{ab}]c > x
        // The filter is [a{bc}].
        // If the input is abc, then the rule will work.
        // However, following code applying the filter won't catch that case.
        internal void AddSourceTargetSet(UnicodeSet filter, UnicodeSet sourceSet, UnicodeSet targetSet, UnicodeSet revisiting)
        {
            int limit = anteContextLength + keyLength;
            UnicodeSet tempSource = new UnicodeSet();
            UnicodeSet temp = new UnicodeSet();

            // We need to walk through the pattern.
            // Iff some of the characters at ALL of the the positions are matched by the filter, then we add temp to toUnionTo
            for (int i = anteContextLength; i < limit;)
            {
                int ch = UTF16.CharAt(pattern, i);
                i += UTF16.GetCharCount(ch);
                IUnicodeMatcher matcher = data.LookupMatcher(ch);
                if (matcher == null)
                {
                    if (!filter.Contains(ch))
                    {
                        return;
                    }
                    tempSource.Add(ch);
                }
                else
                {
                    try
                    {
                        if (!filter.ContainsSome((UnicodeSet)matcher))
                        {
                            return;
                        }
                        matcher.AddMatchSetTo(tempSource);
                    }
                    catch (InvalidCastException)
                    { // if the matcher is not a UnicodeSet
                        temp.Clear();
                        matcher.AddMatchSetTo(temp);
                        if (!filter.ContainsSome(temp))
                        {
                            return;
                        }
                        tempSource.AddAll(temp);
                    }
                }
            }
            // if we made our way through the gauntlet, add to source/target
            sourceSet.AddAll(tempSource);
            output.AddReplacementSetTo(targetSet);
        }
    }
}
