using System;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="UnicodeFilter"/> defines a protocol for selecting a
    /// subset of the full range (U+0000 to U+FFFF) of Unicode characters.
    /// Currently, filters are used in conjunction with classes like
    /// Transliterator to only process selected characters through a
    /// transformation.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    public abstract class UnicodeFilter : IUnicodeMatcher
    {
        /// <summary>
        /// Returns <c>true</c> for characters that are in the selected
        /// subset.  In other words, if a character is <b>to be
        /// filtered</b>, then <see cref="Contains(int)"/> returns
        /// <b><c>false</c></b>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public abstract bool Contains(int c);

        /// <summary>
        /// Default implementation of <see cref="IUnicodeMatcher.Matches(IReplaceable, int[], int, bool)"/> for Unicode
        /// filters.  Matches a single 16-bit code unit at offset.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual int Matches(IReplaceable text,
                       int[] offset,
                       int limit,
                       bool incremental)
        {
            int c;
            if (offset[0] < limit &&
                Contains(c = text.Char32At(offset[0])))
            {
                offset[0] += UTF16.GetCharCount(c);
                return UnicodeMatcher.U_MATCH;
            }
            if (offset[0] > limit && Contains(text.Char32At(offset[0])))
            {
                // Backup offset by 1, unless the preceding character is a
                // surrogate pair -- then backup by 2 (keep offset pointing at
                // the lead surrogate).
                --offset[0];
                if (offset[0] >= 0)
                {
                    offset[0] -= UTF16.GetCharCount(text.Char32At(offset[0])) - 1;
                }
                return UnicodeMatcher.U_MATCH;
            }
            if (incremental && offset[0] == limit)
            {
                return UnicodeMatcher.U_PARTIAL_MATCH;
            }
            return UnicodeMatcher.U_MISMATCH;
        }

        public abstract string ToPattern(bool escapeUnprintable);

        public abstract bool MatchesIndexValue(int v);

        public abstract void AddMatchSetTo(UnicodeSet toUnionTo);

        // TODO Remove this when the JDK property implements MemberDoc.isSynthetic
        /// <summary>
        /// (This should not be here; it is declared to make CheckTags
        /// happy.  .NET inserts a synthetic constructor and CheckTags
        /// can't tell that it's synthetic.)
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected UnicodeFilter() { }
    }
}
