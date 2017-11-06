using ICU4N.Support.Text;
using ICU4N.Util;
using System.Text;
using static ICU4N.Text.UnicodeSet;

namespace ICU4N.Text
{
    internal /* public */ class UnicodeSetSpanner // ICU4N TODO: API This class should be public and the ICharSequence members need to be overloaded for string, StringBuilder and char[]
    {
        private readonly UnicodeSet unicodeSet;

        /**
         * Create a spanner from a UnicodeSet. For speed and safety, the UnicodeSet should be frozen. However, this class
         * can be used with a non-frozen version to avoid the cost of freezing.
         * 
         * @param source
         *            the original UnicodeSet
         *
         * @stable ICU 54
         */
        public UnicodeSetSpanner(UnicodeSet source)
        {
            unicodeSet = source;
        }

        /**
         * Returns the UnicodeSet used for processing. It is frozen iff the original was.
         * 
         * @return the construction set.
         *
         * @stable ICU 54
         */
        public virtual UnicodeSet UnicodeSet
        {
            get { return unicodeSet; }
        }


        /**
         * {@inheritDoc}
         * 
         * @stable ICU 54
         */
        public override bool Equals(object other)
        {
            return other is UnicodeSetSpanner && unicodeSet.Equals(((UnicodeSetSpanner)other).unicodeSet);
        }

        /**
         * {@inheritDoc}
         * 
         * @stable ICU 54
         */
        public override int GetHashCode()
        {
            return unicodeSet.GetHashCode();
        }

        /**
         * Options for replaceFrom and countIn to control how to treat each matched span. 
         * It is similar to whether one is replacing [abc] by x, or [abc]* by x.
         * 
         * @stable ICU 54
         */
        public enum CountMethod // ICU4N TODO: De-nest
        {
            /**
             * Collapse spans. That is, modify/count the entire matching span as a single item, instead of separate
             * set elements.
             *
             * @stable ICU 54
             */
            WHOLE_SPAN,
            /**
             * Use the smallest number of elements in the spanned range for counting and modification,
             * based on the {@link UnicodeSet.SpanCondition}.
             * If the set has no strings, this will be the same as the number of spanned code points.
             * <p>For example, in the string "abab" with SpanCondition.SIMPLE:
             * <ul>
             * <li>spanning with [ab] will count four MIN_ELEMENTS.</li>
             * <li>spanning with [{ab}] will count two MIN_ELEMENTS.</li>
             * <li>spanning with [ab{ab}] will also count two MIN_ELEMENTS.</li>
             * </ul>
             *
             * @stable ICU 54
             */
            MIN_ELEMENTS,
            // Note: could in the future have an additional option MAX_ELEMENTS
        }

        /**
         * Returns the number of matching characters found in a character sequence, 
         * counting by CountMethod.MIN_ELEMENTS using SpanCondition.SIMPLE.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            the sequence to count characters in
         * @return the count. Zero if there are none.
         * 
         * @stable ICU 54
         */
        public virtual int CountIn(ICharSequence sequence)
        {
            return CountIn(sequence, CountMethod.MIN_ELEMENTS, SpanCondition.SIMPLE);
        }

        /**
         * Returns the number of matching characters found in a character sequence, using SpanCondition.SIMPLE.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            the sequence to count characters in
         * @param countMethod
         *            whether to treat an entire span as a match, or individual elements as matches
         * @return the count. Zero if there are none.
         * 
         * @stable ICU 54
         */
        public virtual int CountIn(ICharSequence sequence, CountMethod countMethod)
        {
            return CountIn(sequence, countMethod, SpanCondition.SIMPLE);
        }

        /**
         * Returns the number of matching characters found in a character sequence.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            the sequence to count characters in
         * @param countMethod
         *            whether to treat an entire span as a match, or individual elements as matches
         * @param spanCondition
         *            the spanCondition to use. SIMPLE or CONTAINED means only count the elements in the span;
         *            NOT_CONTAINED is the reverse.
         *            <br><b>WARNING: </b> when a UnicodeSet contains strings, there may be unexpected behavior in edge cases.
         * @return the count. Zero if there are none.
         * 
         * @stable ICU 54
         */
        public virtual int CountIn(ICharSequence sequence, CountMethod countMethod, SpanCondition spanCondition)
        {
            int count = 0;
            int start = 0;
            SpanCondition skipSpan = spanCondition == SpanCondition.NOT_CONTAINED ? SpanCondition.SIMPLE
                    : SpanCondition.NOT_CONTAINED;
            int length = sequence.Length;
            OutputInt spanCount = null;
            while (start != length)
            {
                int endOfSpan = unicodeSet.Span(sequence, start, skipSpan);
                if (endOfSpan == length)
                {
                    break;
                }
                if (countMethod == CountMethod.WHOLE_SPAN)
                {
                    start = unicodeSet.Span(sequence, endOfSpan, spanCondition);
                    count += 1;
                }
                else
                {
                    if (spanCount == null)
                    {
                        spanCount = new OutputInt();
                    }
                    start = unicodeSet.SpanAndCount(sequence, endOfSpan, spanCondition, spanCount);
                    count += spanCount.Value;
                }
            }
            return count;
        }

        /**
         * Delete all the matching spans in sequence, using SpanCondition.SIMPLE
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            charsequence to replace matching spans in.
         * @return modified string.
         * 
         * @stable ICU 54
         */
        public virtual string DeleteFrom(ICharSequence sequence)
        {
            return ReplaceFrom(sequence, "".ToCharSequence(), CountMethod.WHOLE_SPAN, SpanCondition.SIMPLE);
        }

        /**
         * Delete all matching spans in sequence, according to the spanCondition.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            charsequence to replace matching spans in.
         * @param spanCondition
         *            specify whether to modify the matching spans (CONTAINED or SIMPLE) or the non-matching (NOT_CONTAINED)
         * @return modified string.
         * 
         * @stable ICU 54
         */
        public virtual string DeleteFrom(ICharSequence sequence, SpanCondition spanCondition)
        {
            return ReplaceFrom(sequence, "".ToCharSequence(), CountMethod.WHOLE_SPAN, spanCondition);
        }

        /**
         * Replace all matching spans in sequence by the replacement,
         * counting by CountMethod.MIN_ELEMENTS using SpanCondition.SIMPLE.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            charsequence to replace matching spans in.
         * @param replacement
         *            replacement sequence. To delete, use ""
         * @return modified string.
         * 
         * @stable ICU 54
         */
        public virtual string ReplaceFrom(ICharSequence sequence, ICharSequence replacement)
        {
            return ReplaceFrom(sequence, replacement, CountMethod.MIN_ELEMENTS, SpanCondition.SIMPLE);
        }

        /**
         * Replace all matching spans in sequence by replacement, according to the CountMethod, using SpanCondition.SIMPLE.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * 
         * @param sequence
         *            charsequence to replace matching spans in.
         * @param replacement
         *            replacement sequence. To delete, use ""
         * @param countMethod
         *            whether to treat an entire span as a match, or individual elements as matches
         * @return modified string.
         * 
         * @stable ICU 54
         */
        public virtual string ReplaceFrom(ICharSequence sequence, ICharSequence replacement, CountMethod countMethod)
        {
            return ReplaceFrom(sequence, replacement, countMethod, SpanCondition.SIMPLE);
        }

        /**
         * Replace all matching spans in sequence by replacement, according to the countMethod and spanCondition.
         * The code alternates spans; see the class doc for {@link UnicodeSetSpanner} for a note about boundary conditions.
         * @param sequence
         *            charsequence to replace matching spans in.
         * @param replacement
         *            replacement sequence. To delete, use ""
         * @param countMethod 
         *            whether to treat an entire span as a match, or individual elements as matches
         * @param spanCondition
         *            specify whether to modify the matching spans (CONTAINED or SIMPLE) or the non-matching
         *            (NOT_CONTAINED)
         * @return modified string.
         * 
         * @stable ICU 54
         */
        public virtual string ReplaceFrom(ICharSequence sequence, ICharSequence replacement, CountMethod countMethod,
                SpanCondition spanCondition)
        {
            SpanCondition copySpan = spanCondition == SpanCondition.NOT_CONTAINED ? SpanCondition.SIMPLE
                    : SpanCondition.NOT_CONTAINED;
            bool remove = replacement.Length == 0;
            StringBuilder result = new StringBuilder();
            // TODO, we can optimize this to
            // avoid this allocation unless needed

            int length = sequence.Length;
            OutputInt spanCount = null;
            for (int endCopy = 0; endCopy != length;)
            {
                int endModify;
                if (countMethod == CountMethod.WHOLE_SPAN)
                {
                    endModify = unicodeSet.Span(sequence, endCopy, spanCondition);
                }
                else
                {
                    if (spanCount == null)
                    {
                        spanCount = new OutputInt();
                    }
                    endModify = unicodeSet.SpanAndCount(sequence, endCopy, spanCondition, spanCount);
                }
                if (remove || endModify == 0)
                {
                    // do nothing
                }
                else if (countMethod == CountMethod.WHOLE_SPAN)
                {
                    result.Append(replacement);
                }
                else
                {
                    for (int i = spanCount.Value; i > 0; --i)
                    {
                        result.Append(replacement);
                    }
                }
                if (endModify == length)
                {
                    break;
                }
                endCopy = unicodeSet.Span(sequence, endModify, copySpan);
                result.Append(sequence.SubSequence(endModify, endCopy));
            }
            return result.ToString();
        }

        /**
         * Options for the trim() method
         * 
         * @stable ICU 54
         */
        public enum TrimOption
        {
            /**
             * Trim leading spans.
             * 
             * @stable ICU 54
             */
            LEADING,
            /**
             * Trim leading and trailing spans.
             * 
             * @stable ICU 54
             */
            BOTH,
            /**
             * Trim trailing spans.
             * 
             * @stable ICU 54
             */
            TRAILING
        }

        /**
         * Returns a trimmed sequence (using CharSequence.subsequence()), that omits matching elements at the start and
         * end of the string, using TrimOption.BOTH and SpanCondition.SIMPLE. For example:
         * 
         * <pre>
         * {@code
         * 
         *   new UnicodeSet("[ab]").trim("abacatbab")}
         * </pre>
         * 
         * ... returns {@code "cat"}.
         * @param sequence
         *            the sequence to trim
         * @return a subsequence
         * 
         * @stable ICU 54
         */
        public ICharSequence Trim(ICharSequence sequence)
        {
            return Trim(sequence, TrimOption.BOTH, SpanCondition.SIMPLE);
        }

        /**
         * Returns a trimmed sequence (using CharSequence.subsequence()), that omits matching elements at the start or
         * end of the string, using the trimOption and SpanCondition.SIMPLE. For example:
         * 
         * <pre>
         * {@code
         * 
         *   new UnicodeSet("[ab]").trim("abacatbab", TrimOption.LEADING)}
         * </pre>
         * 
         * ... returns {@code "catbab"}.
         * 
         * @param sequence
         *            the sequence to trim
         * @param trimOption
         *            LEADING, TRAILING, or BOTH
         * @return a subsequence
         * 
         * @stable ICU 54
         */
        public virtual ICharSequence Trim(ICharSequence sequence, TrimOption trimOption)
        {
            return Trim(sequence, trimOption, SpanCondition.SIMPLE);
        }

        /**
         * Returns a trimmed sequence (using CharSequence.subsequence()), that omits matching elements at the start or
         * end of the string, depending on the trimOption and spanCondition. For example:
         * 
         * <pre>
         * {@code
         * 
         *   new UnicodeSet("[ab]").trim("abacatbab", TrimOption.LEADING, SpanCondition.SIMPLE)}
         * </pre>
         * 
         * ... returns {@code "catbab"}.
         * 
         * @param sequence
         *            the sequence to trim
         * @param trimOption
         *            LEADING, TRAILING, or BOTH
         * @param spanCondition
         *            SIMPLE, CONTAINED or NOT_CONTAINED
         * @return a subsequence
         * 
         * @stable ICU 54
         */
        public virtual ICharSequence Trim(ICharSequence sequence, TrimOption trimOption, SpanCondition spanCondition)
        {
            int endLeadContained, startTrailContained;
            int length = sequence.Length;
            if (trimOption != TrimOption.TRAILING)
            {
                endLeadContained = unicodeSet.Span(sequence, spanCondition);
                if (endLeadContained == length)
                {
                    return "".ToCharSequence();
                }
            }
            else
            {
                endLeadContained = 0;
            }
            if (trimOption != TrimOption.LEADING)
            {
                startTrailContained = unicodeSet.SpanBack(sequence, spanCondition);
            }
            else
            {
                startTrailContained = length;
            }
            return endLeadContained == 0 && startTrailContained == length ? sequence : sequence.SubSequence(
                    endLeadContained, startTrailContained);
        }
    }
}
