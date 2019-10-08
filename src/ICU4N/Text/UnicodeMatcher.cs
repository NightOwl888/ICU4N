namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="IUnicodeMatcher"/> defines a protocol for objects that can
    /// match a range of characters in a <see cref="IReplaceable"/> string.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    public interface IUnicodeMatcher
    {
        /// <summary>
        /// Return a <see cref="MatchDegree"/> value indicating the degree of match for
        /// the given text at the given offset.  Zero, one, or more
        /// characters may be matched.
        /// </summary>
        /// <remarks>
        /// Matching in the forward direction is indicated by <paramref name="limit"/> &gt;
        /// <paramref name="offset"/>.  Characters from <paramref name="offset"/> forwards to <paramref name="limit"/>-1 will be
        /// considered for matching.
        /// <para/>
        /// Matching in the reverse direction is indicated by <paramref name="limit"/> &lt;
        /// <paramref name="offset"/>.  Characters from <paramref name="offset"/> backwards to <paramref name="limit"/>+1 will be
        /// considered for matching.
        /// <para/>
        /// If <paramref name="limit"/> == <paramref name="offset"/> then the only match possible is a zero
        /// character match (which subclasses may implement if desired).
        /// <para/>
        /// If <see cref="MatchDegree.Match"/> is returned, then as a side effect, advance the
        /// <paramref name="offset"/> parameter to the limit of the matched substring.  In the
        /// forward direction, this will be the index of the last matched
        /// character plus one.  In the reverse direction, this will be the
        /// index of the last matched character minus one.
        /// </remarks>
        /// <param name="text">The text to be matched.</param>
        /// <param name="offset">Offset on input, the index into text at which to begin
        /// matching.  On output, the limit of the matched text.  The
        /// number of matched characters is the output value of <paramref name="offset"/>
        /// minus the input value.  Offset should always point to the
        /// HIGH SURROGATE (leading code unit) of a pair of surrogates,
        /// both on entry and upon return.
        /// </param>
        /// <param name="limit">The limit index of text to be matched.  Greater
        /// than <paramref name="offset"/> for a forward direction match, less than <paramref name="offset"/> for
        /// a backward direction match.  The last character to be
        /// considered for matching will be <c>text[limit-1]</c> in the
        /// forward direction or <c>text[limit+1]</c> in the backward
        /// direction.
        /// </param>
        /// <param name="incremental">If TRUE, then assume further characters may
        /// be inserted at <paramref name="limit"/> and check for partial matching.  Otherwise
        /// assume the text as given is complete.
        /// </param>
        /// <returns>a match degree value indicating a full match, a partial
        /// match, or a mismatch.  If incremental is FALSE then
        /// <see cref="MatchDegree.PartialMatch"/> should never be returned.</returns>
        /// <stable>ICU 2.0</stable>
        MatchDegree Matches(IReplaceable text,
                                    int[] offset,
                                    int limit,
                                    bool incremental);

        /// <summary>
        /// Returns a string representation of this matcher.  If the result of
        /// calling this function is passed to the appropriate parser, it
        /// will produce another matcher that is equal to this one.
        /// </summary>
        /// <param name="escapeUnprintable">if TRUE then convert unprintable
        /// character to their hex escape representations, \\uxxxx or
        /// \\Uxxxxxxxx.  Unprintable characters are those other than
        /// U+000A, U+0020..U+007E.
        /// </param>
        /// <stable>ICU 2.0</stable>
        string ToPattern(bool escapeUnprintable);

        /// <summary>
        /// Returns TRUE if this matcher will match a character c, where c
        /// &amp; 0xFF == v, at offset, in the forward direction (with limit &gt;
        /// offset).  This is used by <c>RuleBasedTransliterator</c> for
        /// indexing.
        /// </summary>
        /// <remarks>
        /// Note:  This API uses an <see cref="int"/> even though the value will be
        /// restricted to 8 bits in order to avoid complications with
        /// signedness (bytes convert to ints in the range -128..127).
        /// </remarks>
        /// <stable>ICU 2.0</stable>
        bool MatchesIndexValue(int v); // ICU4N TODO: API - convert param to byte (in .NET bytes are not signed)

        /// <summary>
        /// Union the set of all characters that may be matched by this object
        /// into the given set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the source characters.</param>
        /// <stable>ICU 2.2</stable>
        void AddMatchSetTo(UnicodeSet toUnionTo);
    }

    /// <summary>
    /// Constants for <see cref="IUnicodeMatcher"/>.
    /// </summary>
    internal static class UnicodeMatcher
    {
        /// <summary>
        /// The character at index i, where i &lt; contextStart || i &gt;= contextLimit,
        /// is <see cref="Ether"/>.  This allows explicit matching by rules and <see cref="UnicodeSet"/>s
        /// of text outside the context.  In traditional terms, this allows anchoring
        /// at the start and/or end.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        internal const char Ether = '\uFFFF';
    }

    /// <summary>
    /// Constants returned by <see cref="IUnicodeMatcher.Matches(IReplaceable, int[], int, bool)"/>
    /// indicating the degree of match.
    /// </summary>
    /// <remarks>
    /// Ported from icu4c/source/common/unicode/unimatch.h
    /// </remarks>
    /// <stable>ICU 2.4</stable>
    public enum MatchDegree
    {
        /// <summary>
        /// Indicates a mismatch between the text and the <see cref="IUnicodeMatcher"/>.  
        /// The text contains a character which does not match, or the text does not contain
        /// all desired characters for a non-incremental match.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Mismatch = 0,

        /// <summary>
        /// Indicates a partial match between the text and the <see cref="IUnicodeMatcher"/>.  This value is
        /// only returned for incremental match operations.  All characters
        /// of the text match, but more characters are required for a
        /// complete match.  Alternatively, for variable-length matchers,
        /// all characters of the text match, and if more characters were
        /// supplied at limit, they might also match.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        PartialMatch = 1,

        /// <summary>
        /// Indicates a complete match between the text and the <see cref="IUnicodeMatcher"/>.  For an
        /// incremental variable-length match, this value is returned if
        /// the given text matches, and it is known that additional
        /// characters would not alter the extent of the match.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Match = 2,
    }
}
