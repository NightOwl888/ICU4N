using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A helper class used to count, replace, and trim character sequences based on <see cref="Text.UnicodeSet"/> matches.
    /// </summary>
    /// <remarks>
    /// An instance is immutable (and thus thread-safe) iff the source UnicodeSet is frozen.
    /// <para/>
    /// <b>Note:</b> The counting, deletion, and replacement depend on alternating a <see cref="Text.SpanCondition"/> with
    /// its inverse. That is, the code spans, then spans for the inverse, then spans, and so on.
    /// For the inverse, the following mapping is used:
    /// <list type="table">
    ///     <item><term><see cref="Text.SpanCondition.Simple"/></term><description><see cref="Text.SpanCondition.NotContained"/></description></item>
    ///     <item><term><see cref="Text.SpanCondition.Contained"/></term><description><see cref="Text.SpanCondition.NotContained"/></description></item>
    ///     <item><term><see cref="Text.SpanCondition.NotContained"/></term><description><see cref="Text.SpanCondition.Simple"/></description></item>
    /// </list>
    /// These are actually not complete inverses. However, the alternating works because there are no gaps.
    /// For example, with [a{ab}{bc}], you get the following behavior when scanning forward:
    /// <list type="table">
    ///     <item><term><see cref="Text.SpanCondition.Simple"/></term><description>xxx[ab]cyyy</description></item>
    ///     <item><term><see cref="Text.SpanCondition.Contained"/></term><description>xxx[abc]yyy</description></item>
    ///     <item><term><see cref="Text.SpanCondition.NotContained"/></term><description>[xxx]ab[cyyy]</description></item>
    /// </list>
    /// <para/>
    /// So here is what happens when you alternate:
    /// <list type="table">
    ///     <item><term><see cref="Text.SpanCondition.NotContained"/></term><description>|xxxabcyyy</description></item>
    ///     <item><term><see cref="Text.SpanCondition.Contained"/></term><description>xxx|abcyyy</description></item>
    ///     <item><term><see cref="Text.SpanCondition.NotContained"/></term><description>xxxabcyyy|</description></item>
    /// </list>
    /// <para/>
    /// The entire string is traversed.
    /// </remarks>
    /// <stable>ICU 54</stable>
    public partial class UnicodeSetSpanner
    {
        private const int CharStackBufferSize = 32;

        private readonly UnicodeSet unicodeSet;

        /// <summary>
        /// Create a spanner from a <see cref="Text.UnicodeSet"/>. For speed and safety, the <see cref="Text.UnicodeSet"/> should be frozen. However, this class
        /// can be used with a non-frozen version to avoid the cost of freezing.
        /// </summary>
        /// <param name="source">The original <see cref="Text.UnicodeSet"/>.</param>
        /// <stable>ICU 54</stable>
        public UnicodeSetSpanner(UnicodeSet source)
        {
            unicodeSet = source;
        }

        /// <summary>
        /// Gets the <see cref="Text.UnicodeSet"/> used for processing. It is frozen iff the original was.
        /// </summary>
        /// <stable>ICU 54</stable>
        public virtual UnicodeSet UnicodeSet => unicodeSet;


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        /// <stable>ICU 54</stable>
        public override bool Equals(object other)
        {
            return other is UnicodeSetSpanner && unicodeSet.Equals(((UnicodeSetSpanner)other).unicodeSet);
        }

        /// <summary>
        /// Gets a hash code that represents the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <stable>ICU 54</stable>
        public override int GetHashCode()
        {
            return unicodeSet.GetHashCode();
        }

        // ICU4N specific - de-nested CountMethod enum

        #region CountIn

        /// <summary>
        /// Returns the number of matching characters found in a character sequence, 
        /// counting by <see cref="CountMethod.MinElements"/> using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">The sequence to count characters in.</param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(string sequence)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return CountIn(sequence.AsSpan(), CountMethod.MinElements, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns the number of matching characters found in a character sequence, 
        /// counting by <see cref="CountMethod.MinElements"/> using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">The sequence to count characters in.</param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(ReadOnlySpan<char> sequence)
        {
            return CountIn(sequence, CountMethod.MinElements, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns the number of matching characters found in a character sequence, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">The sequence to count characters in.</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(string sequence, CountMethod countMethod)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return CountIn(sequence.AsSpan(), countMethod, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns the number of matching characters found in a character sequence, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">The sequence to count characters in.</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(ReadOnlySpan<char> sequence, CountMethod countMethod)
        {
            return CountIn(sequence, countMethod, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns the number of matching characters found in a character sequence.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// The sequence to count characters in.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <param name="spanCondition">the spanCondition to use. <see cref="SpanCondition.Simple"/> or <see cref="SpanCondition.Contained"/> 
        /// means only count the elements in the span; <see cref="SpanCondition.NotContained"/> is the reverse.
        /// <para/>
        /// <b>WARNING: </b> when a <see cref="Text.UnicodeSet"/> contains strings, there may be unexpected behavior in edge cases.
        /// </param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(string sequence, CountMethod countMethod, SpanCondition spanCondition)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return CountIn(sequence.AsSpan(), countMethod, spanCondition);
        }

        /// <summary>
        /// Returns the number of matching characters found in a character sequence.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// The sequence to count characters in.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <param name="spanCondition">the spanCondition to use. <see cref="SpanCondition.Simple"/> or <see cref="SpanCondition.Contained"/> 
        /// means only count the elements in the span; <see cref="SpanCondition.NotContained"/> is the reverse.
        /// <para/>
        /// <b>WARNING: </b> when a <see cref="Text.UnicodeSet"/> contains strings, there may be unexpected behavior in edge cases.
        /// </param>
        /// <returns>The count. Zero if there are none.</returns>
        /// <stable>ICU 54</stable>
        public virtual int CountIn(ReadOnlySpan<char> sequence, CountMethod countMethod, SpanCondition spanCondition)
        {
            int count = 0;
            int start = 0;
            SpanCondition skipSpan = spanCondition == SpanCondition.NotContained ? SpanCondition.Simple
                    : SpanCondition.NotContained;
            int length = sequence.Length;
            int spanCount = 0;
            while (start != length)
            {
                int endOfSpan = unicodeSet.Span(sequence, start, skipSpan);
                if (endOfSpan == length)
                {
                    break;
                }
                if (countMethod == CountMethod.WholeSpan)
                {
                    start = unicodeSet.Span(sequence, endOfSpan, spanCondition);
                    count += 1;
                }
                else
                {
#pragma warning disable 612, 618
                    start = unicodeSet.SpanAndCount(sequence, endOfSpan, spanCondition, out spanCount);
#pragma warning restore 612, 618
                    count += spanCount;
                }
            }
            return count;
        }

        #endregion CountIn

        #region DeleteFrom

        // ICU4N TODO: API - Create overloads that write to a Span<char> instead of returning string

        /// <summary>
        /// Delete all the matching spans in sequence, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string DeleteFrom(string sequence)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return ReplaceFrom(sequence.AsSpan(), ReadOnlySpan<char>.Empty, CountMethod.WholeSpan, SpanCondition.Simple);
        }

        /// <summary>
        /// Delete all the matching spans in sequence, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string DeleteFrom(ReadOnlySpan<char> sequence)
        {
            return ReplaceFrom(sequence, ReadOnlySpan<char>.Empty, CountMethod.WholeSpan, SpanCondition.Simple);
        }

        /// <summary>
        /// Delete all matching spans in sequence, according to the spanCondition.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// 
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="spanCondition">Specify whether to modify the matching spans 
        /// (<see cref="SpanCondition.Contained"/> or <see cref="SpanCondition.Simple"/>) 
        /// or the non-matching (<see cref="SpanCondition.NotContained"/>).</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string DeleteFrom(string sequence, SpanCondition spanCondition)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return ReplaceFrom(sequence.AsSpan(), ReadOnlySpan<char>.Empty, CountMethod.WholeSpan, spanCondition);
        }

        /// <summary>
        /// Delete all matching spans in sequence, according to the spanCondition.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// 
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="spanCondition">Specify whether to modify the matching spans 
        /// (<see cref="SpanCondition.Contained"/> or <see cref="SpanCondition.Simple"/>) 
        /// or the non-matching (<see cref="SpanCondition.NotContained"/>).</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string DeleteFrom(ReadOnlySpan<char> sequence, SpanCondition spanCondition)
        {
            return ReplaceFrom(sequence, ReadOnlySpan<char>.Empty, CountMethod.WholeSpan, spanCondition);
        }

        #endregion DeleteFrom

        #region ReplaceFrom

        // ICU4N TODO: API - Create overloads that write to a Span<char> instead of returning string

        /// <summary>
        /// Replace all matching spans in sequence by the replacement,
        /// counting by <see cref="CountMethod.MinElements"/> using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(string sequence, string replacement)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));
            if (replacement is null)
                throw new ArgumentNullException(nameof(replacement));

            return ReplaceFrom(sequence.AsSpan(), replacement.AsSpan(), CountMethod.MinElements, SpanCondition.Simple);
        }

        /// <summary>
        /// Replace all matching spans in sequence by the replacement,
        /// counting by <see cref="CountMethod.MinElements"/> using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(ReadOnlySpan<char> sequence, ReadOnlySpan<char> replacement)
        {
            return ReplaceFrom(sequence, replacement, CountMethod.MinElements, SpanCondition.Simple);
        }

        /// <summary>
        /// Replace all matching spans in sequence by replacement, according to the <see cref="CountMethod"/>, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(string sequence, string replacement, CountMethod countMethod)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));
            if (replacement is null)
                throw new ArgumentNullException(nameof(replacement));

            return ReplaceFrom(sequence.AsSpan(), replacement.AsSpan(), countMethod, SpanCondition.Simple);
        }

        /// <summary>
        /// Replace all matching spans in sequence by replacement, according to the <see cref="CountMethod"/>, using <see cref="SpanCondition.Simple"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(ReadOnlySpan<char> sequence, ReadOnlySpan<char> replacement, CountMethod countMethod)
        {
            return ReplaceFrom(sequence, replacement, countMethod, SpanCondition.Simple);
        }

        /// <summary>
        /// Replace all matching spans in sequence by replacement, according to the <paramref name="countMethod"/> and <paramref name="spanCondition"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <param name="spanCondition">specify whether to modify the matching spans (<see cref="SpanCondition.Contained"/> 
        /// or <see cref="SpanCondition.Simple"/>) or the non-matching (<see cref="SpanCondition.NotContained"/>)</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(string sequence, string replacement, CountMethod countMethod,
            SpanCondition spanCondition)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));
            if (replacement is null)
                throw new ArgumentNullException(nameof(replacement));

            return ReplaceFrom(sequence.AsSpan(), replacement.AsSpan(), countMethod, spanCondition);
        }

        /// <summary>
        /// Replace all matching spans in sequence by replacement, according to the <paramref name="countMethod"/> and <paramref name="spanCondition"/>.
        /// The code alternates spans; see the class doc for <see cref="UnicodeSetSpanner"/> for a note about boundary conditions.
        /// </summary>
        /// <param name="sequence">Character sequence to replace matching spans in.</param>
        /// <param name="replacement">Replacement sequence. To delete, use "".</param>
        /// <param name="countMethod">Whether to treat an entire span as a match, or individual elements as matches.</param>
        /// <param name="spanCondition">specify whether to modify the matching spans (<see cref="SpanCondition.Contained"/> 
        /// or <see cref="SpanCondition.Simple"/>) or the non-matching (<see cref="SpanCondition.NotContained"/>)</param>
        /// <returns>Modified string.</returns>
        /// <stable>ICU 54</stable>
        public virtual string ReplaceFrom(ReadOnlySpan<char> sequence, ReadOnlySpan<char> replacement, CountMethod countMethod,
            SpanCondition spanCondition)
        {
            SpanCondition copySpan = spanCondition == SpanCondition.NotContained ? SpanCondition.Simple
                    : SpanCondition.NotContained;
            bool remove = replacement.Length == 0;
            ValueStringBuilder result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            // TODO, we can optimize this to
            // avoid this allocation unless needed

            int length = sequence.Length;
            int spanCount = 0;
            for (int endCopy = 0; endCopy != length;)
            {
                int endModify;
                if (countMethod == CountMethod.WholeSpan)
                {
                    endModify = unicodeSet.Span(sequence, endCopy, spanCondition);
                }
                else
                {
#pragma warning disable 612, 618
                    endModify = unicodeSet.SpanAndCount(sequence, endCopy, spanCondition, out spanCount);
#pragma warning restore 612, 618
                }
                if (remove || endModify == 0)
                {
                    // do nothing
                }
                else if (countMethod == CountMethod.WholeSpan)
                {
                    result.Append(replacement);
                }
                else
                {
                    for (int i = spanCount; i > 0; --i)
                    {
                        result.Append(replacement);
                    }
                }
                if (endModify == length)
                {
                    break;
                }
                endCopy = unicodeSet.Span(sequence, endModify, copySpan);
                result.Append(sequence.Slice(endModify, endCopy - endModify)); // ICU4N: Corrected 2nd parameter
            }
            return result.ToString();
        }

        #endregion ReplaceFrom

        #region Trim

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start and
        /// end of the string, using <see cref="TrimOption.Both"/> and <see cref="SpanCondition.Simple"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab")
        /// </code>
        /// ... returns <c>"cat"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(string sequence)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return Trim(sequence.AsSpan(), TrimOption.Both, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start and
        /// end of the string, using <see cref="TrimOption.Both"/> and <see cref="SpanCondition.Simple"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab")
        /// </code>
        /// ... returns <c>"cat"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(ReadOnlySpan<char> sequence)
        {
            return Trim(sequence, TrimOption.Both, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start or
        /// end of the string, using the <paramref name="trimOption"/> and <see cref="SpanCondition.Simple"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab", TrimOption.Leading)
        /// </code>
        /// ... returns <c>"catbab"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <param name="trimOption"><see cref="TrimOption.Leading"/>, <see cref="TrimOption.Trailing"/>, 
        /// or <see cref="TrimOption.Both"/>.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(string sequence, TrimOption trimOption)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return Trim(sequence.AsSpan(), trimOption, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start or
        /// end of the string, using the <paramref name="trimOption"/> and <see cref="SpanCondition.Simple"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab", TrimOption.Leading)
        /// </code>
        /// ... returns <c>"catbab"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <param name="trimOption"><see cref="TrimOption.Leading"/>, <see cref="TrimOption.Trailing"/>, 
        /// or <see cref="TrimOption.Both"/>.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(ReadOnlySpan<char> sequence, TrimOption trimOption)
        {
            return Trim(sequence, trimOption, SpanCondition.Simple);
        }

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start or
        /// end of the string, depending on the <paramref name="trimOption"/> and <paramref name="spanCondition"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab", TrimOption.Leading, SpanCondition.Simple)
        /// </code>
        /// ... returns <c>"catbab"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <param name="trimOption"><see cref="TrimOption.Leading"/>, <see cref="TrimOption.Trailing"/>, 
        /// or <see cref="TrimOption.Both"/>.</param>
        /// <param name="spanCondition"><see cref="SpanCondition.Simple"/>, <see cref="SpanCondition.Contained"/> or 
        /// <see cref="SpanCondition.NotContained"/>.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(string sequence, TrimOption trimOption, SpanCondition spanCondition)
        {
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            return Trim(sequence.AsSpan(), trimOption, spanCondition);
        }

        /// <summary>
        /// Returns a trimmed sequence (using <see cref="ReadOnlySpan{Char}.Slice(int, int)"/>), that omits matching elements at the start or
        /// end of the string, depending on the <paramref name="trimOption"/> and <paramref name="spanCondition"/>. For example:
        /// <code>
        ///     new UnicodeSet("[ab]").Trim("abacatbab", TrimOption.Leading, SpanCondition.Simple)
        /// </code>
        /// ... returns <c>"catbab"</c>.
        /// </summary>
        /// <param name="sequence">The sequence to trim.</param>
        /// <param name="trimOption"><see cref="TrimOption.Leading"/>, <see cref="TrimOption.Trailing"/>, 
        /// or <see cref="TrimOption.Both"/>.</param>
        /// <param name="spanCondition"><see cref="SpanCondition.Simple"/>, <see cref="SpanCondition.Contained"/> or 
        /// <see cref="SpanCondition.NotContained"/>.</param>
        /// <returns>A subsequence.</returns>
        /// <stable>ICU 54</stable>
        public virtual ReadOnlySpan<char> Trim(ReadOnlySpan<char> sequence, TrimOption trimOption, SpanCondition spanCondition)
        {
            int endLeadContained, startTrailContained;
            int length = sequence.Length;
            if (trimOption != TrimOption.Trailing)
            {
                endLeadContained = unicodeSet.Span(sequence, spanCondition);
                if (endLeadContained == length)
                {
                    return ReadOnlySpan<char>.Empty;
                }
            }
            else
            {
                endLeadContained = 0;
            }
            if (trimOption != TrimOption.Leading)
            {
                startTrailContained = unicodeSet.SpanBack(sequence, spanCondition);
            }
            else
            {
                startTrailContained = length;
            }
            return endLeadContained == 0 && startTrailContained == length ?
                sequence :
                sequence.Slice(endLeadContained, startTrailContained - endLeadContained); // ICU4N: Corrected 2nd parameter
        }

        #endregion Trim

        // ICU4N specific - de-nested TrimOption enum
    }

    /// <summary>
    /// Options for <see cref="UnicodeSetSpanner.ReplaceFrom(ReadOnlySpan{char}, ReadOnlySpan{char}, CountMethod)"/> 
    /// and <see cref="UnicodeSetSpanner.CountIn(ReadOnlySpan{char}, CountMethod)"/> to control how to treat each matched span. 
    /// It is similar to whether one is replacing [abc] by x, or [abc]* by x.
    /// </summary>
    /// <stable>ICU 54</stable>
    public enum CountMethod
    {
        /// <summary>
        /// Collapse spans. That is, modify/count the entire matching span as a single item, instead of separate
        /// set elements.
        /// </summary>
        /// <stable>ICU 54</stable>
        WholeSpan,

        /// <summary>
        /// Use the smallest number of elements in the spanned range for counting and modification,
        /// based on the <see cref="SpanCondition"/>.
        /// If the set has no strings, this will be the same as the number of spanned code points.
        /// <para/>
        /// For example, in the string "abab" with <see cref="SpanCondition.Simple"/>:
        /// <list type="bullet">
        ///     <item><description>spanning with [ab] will count four <see cref="MinElements"/>.</description></item>
        ///     <item><description>spanning with [{ab}] will count two <see cref="MinElements"/>.</description></item>
        ///     <item><description>spanning with [ab{ab}] will also count two <see cref="MinElements"/>.</description></item>
        /// </list>
        /// </summary>
        /// <stable>ICU 54</stable>
        MinElements,
        // Note: could in the future have an additional option MAX_ELEMENTS
    }

    /// <summary>
    /// Options for the <see cref="UnicodeSetSpanner.Trim(ReadOnlySpan{char}, TrimOption, SpanCondition)"/> method.
    /// </summary>
    /// <stable>ICU 54</stable>
    public enum TrimOption
    {
        /// <summary>
        /// Trim leading spans.
        /// </summary>
        /// <stable>ICU 54</stable>
        Leading,
        /// <summary>
        /// Trim leading and trailing spans.
        /// </summary>
        /// <stable>ICU 54</stable>
        Both,
        /// <summary>
        /// Trim trailing spans.
        /// </summary>
        /// <stable>ICU 54</stable>
        Trailing
    }
}
