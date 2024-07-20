using ICU4N.Impl;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Text
{
    internal class Quantifier : IUnicodeMatcher
    {
        private const int CharStackBufferSize = 64;

        private IUnicodeMatcher matcher;

        private int minCount;

        private int maxCount;

        /// <summary>
        /// Maximum count a quantifier can have.
        /// </summary>
        public const int MaxCount = int.MaxValue;

        public Quantifier(IUnicodeMatcher theMatcher,
                          int theMinCount, int theMaxCount)
        {
            if (theMatcher == null || theMinCount < 0 || theMaxCount < 0 || theMinCount > theMaxCount)
            {
                throw new ArgumentException();
            }
            matcher = theMatcher;
            minCount = theMinCount;
            maxCount = theMaxCount;
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/> API.
        /// </summary>
        public virtual MatchDegree Matches(IReplaceable text,
                           ref int offset,
                           int limit,
                           bool incremental)
        {
            int start = offset;
            int count = 0;
            while (count < maxCount)
            {
                int pos = offset;
                MatchDegree m = matcher.Matches(text, ref offset, limit, incremental);
                if (m == MatchDegree.Match)
                {
                    ++count;
                    if (pos == offset)
                    {
                        // If offset has not moved we have a zero-width match.
                        // Don't keep matching it infinitely.
                        break;
                    }
                }
                else if (incremental && m == MatchDegree.PartialMatch)
                {
                    return MatchDegree.PartialMatch;
                }
                else
                {
                    break;
                }
            }
            if (incremental && offset == limit)
            {
                return MatchDegree.PartialMatch;
            }
            if (count >= minCount)
            {
                return MatchDegree.Match;
            }
            offset = start;
            return MatchDegree.Mismatch;
        }

#nullable enable

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/> API.
        /// </summary>
        public virtual string ToPattern(bool escapeUnprintable)
        {
            ValueStringBuilder result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToPattern(escapeUnprintable, ref result);
                return result.ToString();
            }
            finally
            {
                result.Dispose();
            }
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/> API.
        /// </summary>
        public virtual bool TryToPattern(bool escapeUnprintable, Span<char> destination, out int charsLength)
        {
            ValueStringBuilder result = new ValueStringBuilder(destination);
            try
            {
                ToPattern(escapeUnprintable, ref result);
                return result.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                result.Dispose();
            }
        }

        internal void ToPattern(bool escapeUnprintable, ref ValueStringBuilder result)
        {
            char[]? matcherPatternArray = null;
            try
            {
                Span<char> matcherPattern = stackalloc char[CharStackBufferSize];
                if (!matcher.TryToPattern(escapeUnprintable, matcherPattern, out int matcherPatternLength))
                {
                    // Not enough buffer, use the array pool
                    matcherPattern = matcherPatternArray = ArrayPool<char>.Shared.Rent(matcherPatternLength);
                    bool success = matcher.TryToPattern(escapeUnprintable, matcherPattern, out matcherPatternLength);
                    Debug.Assert(success); // Unexpected
                }
                result.Append(matcherPattern.Slice(0, matcherPatternLength));
            }
            finally
            {
                if (matcherPatternArray is not null)
                    ArrayPool<char>.Shared.Return(matcherPatternArray);
            }
            if (minCount == 0)
            {
                if (maxCount == 1)
                {
                    result.Append('?');
                    return;
                }
                else if (maxCount == MaxCount)
                {
                    result.Append('*');
                    return;
                }
                // else fall through
            }
            else if (minCount == 1 && maxCount == MaxCount)
            {
                result.Append('+');
                return;
            }
            result.Append('{');
            result.AppendFormatHex(minCount, 1);
            result.Append(',');
            if (maxCount != MaxCount)
            {
                result.AppendFormatHex(maxCount, 1);
            }
            result.Append('}');
        }

#nullable restore

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/> API.
        /// </summary>
        public virtual bool MatchesIndexValue(int v)
        {
            return (minCount == 0) || matcher.MatchesIndexValue(v);
        }

        /// <summary>
        /// Implementation of <see cref="IUnicodeMatcher"/> API.  Union the set of all
        /// characters that may be matched by this object into the given
        /// set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the source characters.</param>
        public virtual void AddMatchSetTo(UnicodeSet toUnionTo)
        {
            if (maxCount > 0)
            {
                matcher.AddMatchSetTo(toUnionTo);
            }
        }
    }
}
