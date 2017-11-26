using ICU4N.Impl;
using System;
using System.Text;

namespace ICU4N.Text
{
    internal class Quantifier : IUnicodeMatcher
    {
        private IUnicodeMatcher matcher;

        private int minCount;

        private int maxCount;

        /// <summary>
        /// Maximum count a quantifier can have.
        /// </summary>
        public const int MAX = int.MaxValue;

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
        public virtual int Matches(IReplaceable text,
                           int[] offset,
                           int limit,
                           bool incremental)
        {
            int start = offset[0];
            int count = 0;
            while (count < maxCount)
            {
                int pos = offset[0];
                int m = matcher.Matches(text, offset, limit, incremental);
                if (m == UnicodeMatcher.U_MATCH)
                {
                    ++count;
                    if (pos == offset[0])
                    {
                        // If offset has not moved we have a zero-width match.
                        // Don't keep matching it infinitely.
                        break;
                    }
                }
                else if (incremental && m == UnicodeMatcher.U_PARTIAL_MATCH)
                {
                    return UnicodeMatcher.U_PARTIAL_MATCH;
                }
                else
                {
                    break;
                }
            }
            if (incremental && offset[0] == limit)
            {
                return UnicodeMatcher.U_PARTIAL_MATCH;
            }
            if (count >= minCount)
            {
                return UnicodeMatcher.U_MATCH;
            }
            offset[0] = start;
            return UnicodeMatcher.U_MISMATCH;
        }

        /// <summary>
        /// Implement <see cref="IUnicodeMatcher"/> API.
        /// </summary>
        public virtual string ToPattern(bool escapeUnprintable)
        {
            StringBuilder result = new StringBuilder();
            result.Append(matcher.ToPattern(escapeUnprintable));
            if (minCount == 0)
            {
                if (maxCount == 1)
                {
                    return result.Append('?').ToString();
                }
                else if (maxCount == MAX)
                {
                    return result.Append('*').ToString();
                }
                // else fall through
            }
            else if (minCount == 1 && maxCount == MAX)
            {
                return result.Append('+').ToString();
            }
            result.Append('{');
            result.Append(Utility.Hex(minCount, 1));
            result.Append(',');
            if (maxCount != MAX)
            {
                result.Append(Utility.Hex(maxCount, 1));
            }
            result.Append('}');
            return result.ToString();
        }

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
