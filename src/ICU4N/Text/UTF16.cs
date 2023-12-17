using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Standalone utility class providing UTF16 character conversions and indexing conversions.
    /// </summary>
    /// <remarks>
    /// Code that uses strings alone rarely need modification. By design, UTF-16 does not allow overlap,
    /// so searching for strings is a safe operation. Similarly, concatenation is always safe.
    /// Substringing is safe if the start and end are both on UTF-32 boundaries. In normal code, the
    /// values for start and end are on those boundaries, since they arose from operations like
    /// searching. If not, the nearest UTF-32 boundaries can be determined using <see cref="Bounds(string, int)"/>.
    /// <para/>
    /// <strong>Examples:</strong>
    /// <para/>
    /// The following examples illustrate use of some of these methods.
    /// <code>
    /// // iteration forwards: Original
    /// for (int i = 0; i &lt; s.Length; ++i)
    /// {
    ///     char ch = s[i];
    ///     DoSomethingWith(ch);
    /// }
    /// 
    /// // iteration forwards: Changes for UTF-32
    /// int ch;
    /// for (int i = 0; i &lt; s.Length; i += UTF16.GetCharCount(ch))
    /// {
    ///     ch = UTF16.CharAt(s, i);
    ///     DoSomethingWith(ch);
    /// }
    /// 
    /// // iteration backwards: Original
    /// for (int i = s.Length - 1; i &gt;= 0; --i)
    /// {
    ///     char ch = s[i];
    ///     DoSomethingWith(ch);
    /// }
    /// 
    /// // iteration backwards: Changes for UTF-32
    /// int ch;
    /// for (int i = s.Length - 1; i &gt; 0; i -= UTF16.GetCharCount(ch))
    /// {
    ///     ch = UTF16.CharAt(s, i);
    ///     DoSomethingWith(ch);
    /// }
    /// </code>
    /// <strong>Notes:</strong>
    /// <list type="bullet">
    ///     <item><description>
    ///         <strong>Naming:</strong> For clarity, High and Low surrogates are called <c>Lead</c>
    ///         and <c>Trail</c> in the API, which gives a better sense of their ordering in a string.
    ///         <c>offset16</c> and <c>offset32</c> are used to distinguish offsets to UTF-16
    ///         boundaries vs offsets to UTF-32 boundaries. <c>int char32</c> is used to contain UTF-32
    ///         characters, as opposed to <c>char16</c>, which is a UTF-16 code unit.
    ///     </description></item>
    ///     <item><description>
    ///         <strong>Roundtripping Offsets:</strong> You can always roundtrip from a UTF-32 offset to a
    ///         UTF-16 offset and back. Because of the difference in structure, you can roundtrip from a UTF-16
    ///         offset to a UTF-32 offset and back if and only if <c>Bounds(string, offset16) != TRAIL</c>.
    ///     </description></item>
    ///     <item><description>
    ///         <strong>Exceptions:</strong> The error checking will throw an exception if indices are out
    ///         of bounds. Other than than that, all methods will behave reasonably, even if unmatched surrogates
    ///         or out-of-bounds UTF-32 values are present. <see cref="UChar.IsLegal(int)"/> can be used to
    ///         check for validity if desired.
    ///     </description></item>
    ///     <item><description>
    ///         <strong>Unmatched Surrogates:</strong> If the string contains unmatched surrogates, then
    ///         these are counted as one UTF-32 value. This matches their iteration behavior, which is vital. It
    ///         also matches common display practice as missing glyphs (see the Unicode Standard Section 5.4,
    ///         5.5).
    ///     </description></item>
    ///     <item><description>
    ///         <strong>Optimization:</strong> The method implementations may need optimization if the
    ///         compiler doesn't fold static final methods. Since surrogate pairs will form an exceeding small
    ///         percentage of all the text in the world, the singleton case should always be optimized for.
    ///     </description></item>
    /// </list>
    /// </remarks>
    /// <author>Mark Davis, with help from Markus Scherer</author>
    /// <stable>ICU 2.1</stable>
    public static partial class UTF16
    {
        // public variables ---------------------------------------------------

        /// <summary>
        /// Value returned in <see cref="Bounds(string, int)"/>.
        /// These values are chosen specifically so that it actually represents the position of the
        /// character [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)]
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SingleCharBoundary = 1, LeadSurrogateBoundary = 2,
                TrailSurrogateBoundary = 5; // ICU4N TODO: API - make enum ?

        /// <summary>
        /// The lowest Unicode code point value.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int CodePointMinValue = 0;

        /// <summary>
        /// The highest Unicode code point value (scalar value) according to the Unicode Standard.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int CodePointMaxValue = 0x10ffff;

        /// <summary>
        /// The minimum value for Supplementary code points
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SupplementaryMinValue = 0x10000;

        /// <summary>
        /// Lead surrogate minimum value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int LeadSurrogateMinValue = 0xD800;

        /// <summary>
        /// Trail surrogate minimum value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int TrailSurrogateMinValue = 0xDC00;

        /// <summary>
        /// Lead surrogate maximum value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int LeadSurrogateMaxValue = 0xDBFF;

        /// <summary>
        /// Trail surrogate maximum value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int TrailSurrogateMaxValue = 0xDFFF;

        /// <summary>
        /// Surrogate minimum value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SurrogateMinValue = LeadSurrogateMinValue;

        /// <summary>
        /// Maximum surrogate value
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SurrogateMaxValue = TrailSurrogateMaxValue;

        /// <summary>
        /// Lead surrogate bitmask
        /// </summary>
        private const int LeadSurrogateBitmask = unchecked((int)0xFFFFFC00);

        /// <summary>
        /// Trail surrogate bitmask
        /// </summary>
        private const int TrailSurrogateBitmask = unchecked((int)0xFFFFFC00);

        /// <summary>
        /// Surrogate bitmask
        /// </summary>
        private const int SurrogateBitmask = unchecked((int)0xFFFFF800);

        /// <summary>
        /// Lead surrogate bits
        /// </summary>
        private const int LeadSurrogateBits = 0xD800;

        /// <summary>
        /// Trail surrogate bits
        /// </summary>
        private const int TrailSurrogateBits = 0xDC00;

        /// <summary>
        /// Surrogate bits
        /// </summary>
        private const int SurrogateBits = 0xD800;

        // constructor --------------------------------------------------------

        // ICU4N: Class made static rather than having private constructor


        // /CLOVER:ON
        // public method ------------------------------------------------------

        // ICU4N specific - These methods were combined into one and moved to UTF16.generated.tt
        // - CharAt(string source, int offset16)
        // - _charAt(string source, int offset16, char single)
        // - CharAt(char[] source, int offset16)
        // - _charAt(string source, int offset16, char single)
        // - CharAt(ICharSequence source, int offset16)
        // - _charAt(ICharSequence source, int offset16, char single)
        // - CharAt(StringBuilder source, int offset16)

        /// <summary>
        /// Extract a single UTF-32 value from a substring. Used when iterating forwards or backwards
        /// (with <see cref="UTF16.GetCharCount(int)"/>, as well as random access. If a validity check is
        /// required, use <see cref="UChar.IsLegal(int)"/>
        /// on the return value. If the char retrieved is part of a surrogate pair, its supplementary
        /// character will be returned. If a complete supplementary character is not found the incomplete
        /// character will be returned.
        /// </summary>
        /// <param name="source">Array of UTF-16 chars.</param>
        /// <param name="start">Offset to substring in the source array for analyzing.</param>
        /// <param name="limit">Offset to substring in the source array for analyzing.</param>
        /// <param name="offset16">UTF-16 offset relative to start.</param>
        /// <returns>UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
        /// of that codepoint are the same as in <see cref="Bounds(char[], int, int, int)"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if offset16 is not within the range of start and limit.</exception>
        /// <stable>ICU 2.1</stable>
        public static int CharAt(char[] source, int start, int limit, int offset16)
        {
            offset16 += start;
            if (offset16 < start || offset16 >= limit)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }

            char single = source[offset16];
            if (!IsSurrogate(single))
            {
                return single;
            }

            // Convert the UTF-16 surrogate pair if necessary.
            // For simplicity in usage, and because the frequency of pairs is
            // low, look both directions.
            if (single <= LeadSurrogateMaxValue)
            {
                offset16++;
                if (offset16 >= limit)
                {
                    return single;
                }
                char trail = source[offset16];
                if (IsTrailSurrogate(trail))
                {
                    return Character.ToCodePoint(single, trail);
                }
            }
            else
            { // IsTrailSurrogate(single), so
                if (offset16 == start)
                {
                    return single;
                }
                offset16--;
                char lead = source[offset16];
                if (IsLeadSurrogate(lead))
                    return Character.ToCodePoint(lead, single);
            }
            return single; // return unmatched surrogate
        }

        /// <summary>
        /// Extract a single UTF-32 value from a string. Used when iterating forwards or backwards (with
        /// <see cref="UTF16.GetCharCount(int)"/>, as well as random access. If a validity check is
        /// required, use <see cref="UChar.IsLegal(int)"/> on the return value. If the char 
        /// retrieved is part of a surrogate pair, its supplementary
        /// character will be returned. If a complete supplementary character is not found the incomplete
        /// character will be returned.
        /// </summary>
        /// <param name="source">UTF-16 chars string buffer.</param>
        /// <param name="offset16">UTF-16 offset to the start of the character.</param>
        /// <returns>UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
        /// of that codepoint are the same as in <see cref="Bounds(char[], int, int, int)"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if offset16 is not within the range of start and limit.</exception>
        /// <stable>ICU 2.1</stable>
        public static int CharAt(IReplaceable source, int offset16)
        {
            if (offset16 < 0 || offset16 >= source.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }

            char single = source[offset16];
            if (!IsSurrogate(single))
            {
                return single;
            }

            // Convert the UTF-16 surrogate pair if necessary.
            // For simplicity in usage, and because the frequency of pairs is
            // low, look both directions.

            if (single <= LeadSurrogateMaxValue)
            {
                ++offset16;
                if (source.Length != offset16)
                {
                    char trail = source[offset16];
                    if (IsTrailSurrogate(trail))
                        return Character.ToCodePoint(single, trail);
                }
            }
            else
            {
                --offset16;
                if (offset16 >= 0)
                {
                    // single is a trail surrogate so
                    char lead = source[offset16];
                    if (IsLeadSurrogate(lead))
                    {
                        return Character.ToCodePoint(lead, single);
                    }
                }
            }
            return single; // return unmatched surrogate
        }

        /// <summary>
        /// Determines how many chars this <paramref name="char32"/> requires. If a validity check is required, use 
        /// <see cref="UChar.IsLegal(int)"/> on <paramref name="char32"/> before calling.
        /// </summary>
        /// <param name="char32">The input codepoint.</param>
        /// <returns>2 if is in supplementary space, otherwise 1.</returns>
        /// <stable>ICU 2.1</stable>
        public static int GetCharCount(int char32)
        {
            if (char32 < SupplementaryMinValue)
            {
                return 1;
            }
            return 2;
        }

        /// <summary>
        /// Returns the type of the boundaries around the char at <paramref name="offset16"/>. Used for random access.
        /// </summary>
        /// <param name="source">Text to analyze.</param>
        /// <param name="offset16">UTF-16 offset.</param>
        /// <returns>
        /// <list type="bullet">
        ///     <item><description><see cref="SingleCharBoundary"/> : a single char; the bounds are [offset16, offset16+1]</description></item>
        ///     <item><description>
        ///         <see cref="LeadSurrogateBoundary"/> : a surrogate pair starting at offset16; the bounds
        ///         are [offset16, offset16 + 2]
        ///     </description></item>
        ///     <item><description>
        ///         <see cref="TrailSurrogateBoundary"/> : a surrogate pair starting at offset16 - 1; the
        ///         bounds are [offset16 - 1, offset16 + 1]
        ///     </description></item>
        /// </list>
        /// For bit-twiddlers, the return values for these are chosen so that the boundaries
        /// can be gotten by: [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)].
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Bounds(string source, int offset16)
        {
            char ch = source[offset16];
            if (IsSurrogate(ch))
            {
                if (IsLeadSurrogate(ch))
                {
                    if (++offset16 < source.Length && IsTrailSurrogate(source[offset16]))
                    {
                        return LeadSurrogateBoundary;
                    }
                }
                else
                {
                    // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= 0 && IsLeadSurrogate(source[offset16]))
                    {
                        return TrailSurrogateBoundary;
                    }
                }
            }
            return SingleCharBoundary;
        }

        /// <summary>
        /// Returns the type of the boundaries around the char at <paramref name="offset16"/>. Used for random access.
        /// </summary>
        /// <param name="source">String buffer to analyze.</param>
        /// <param name="offset16">UTF16 offset.</param>
        /// <returns>
        /// <list type="bullet">
        ///     <item><description><see cref="SingleCharBoundary"/> : a single char; the bounds are [offset16, offset16+1]</description></item>
        ///     <item><description>
        ///         <see cref="LeadSurrogateBoundary"/> : a surrogate pair starting at offset16; the bounds
        ///         are [offset16, offset16 + 2]
        ///     </description></item>
        ///     <item><description>
        ///         <see cref="TrailSurrogateBoundary"/> : a surrogate pair starting at offset16 - 1; the
        ///         bounds are [offset16 - 1, offset16 + 1]
        ///     </description></item>
        /// </list>
        /// For bit-twiddlers, the return values for these are chosen so that the boundaries
        /// can be gotten by: [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)].
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Bounds(StringBuilder source, int offset16)
        {
            char ch = source[offset16];
            if (IsSurrogate(ch))
            {
                if (IsLeadSurrogate(ch))
                {
                    if (++offset16 < source.Length && IsTrailSurrogate(source[offset16]))
                    {
                        return LeadSurrogateBoundary;
                    }
                }
                else
                {
                    // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= 0 && IsLeadSurrogate(source[offset16]))
                    {
                        return TrailSurrogateBoundary;
                    }
                }
            }
            return SingleCharBoundary;
        }

        /// <summary>
        /// Returns the type of the boundaries around the char at <paramref name="offset16"/>. Used for random access. Note
        /// that the boundaries are determined with respect to the subarray, hence the char array
        /// {0xD800, 0xDC00} has the result <see cref="SingleCharBoundary"/> for start = offset16 = 0 and limit = 1.
        /// </summary>
        /// <param name="source">Char array to analyze.</param>
        /// <param name="start">Offset to substring in the source array for analyzing.</param>
        /// <param name="limit">Offset to substring in the source array for analyzing.</param>
        /// <param name="offset16">UTF16 offset relative to start.</param>
        /// <returns>
        /// <list type="bullet">
        ///     <item><description><see cref="SingleCharBoundary"/> : a single char; the bounds are [offset16, offset16+1]</description></item>
        ///     <item><description>
        ///         <see cref="LeadSurrogateBoundary"/> : a surrogate pair starting at offset16; the bounds
        ///         are [offset16, offset16 + 2]
        ///     </description></item>
        ///     <item><description>
        ///         <see cref="TrailSurrogateBoundary"/> : a surrogate pair starting at offset16 - 1; the
        ///         bounds are [offset16 - 1, offset16 + 1]
        ///     </description></item>
        /// </list>
        /// For bit-twiddlers, the return values for these are chosen so that the boundaries
        /// can be gotten by: [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)].
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Bounds(char[] source, int start, int limit, int offset16)
        {
            offset16 += start;
            if (offset16 < start || offset16 >= limit)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }
            char ch = source[offset16];
            if (IsSurrogate(ch))
            {
                if (IsLeadSurrogate(ch))
                {
                    ++offset16;
                    if (offset16 < limit && IsTrailSurrogate(source[offset16]))
                    {
                        return LeadSurrogateBoundary;
                    }
                }
                else
                { // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= start && IsLeadSurrogate(source[offset16]))
                    {
                        return TrailSurrogateBoundary;
                    }
                }
            }
            return SingleCharBoundary;
        }

        /// <summary>
        /// Determines whether the code value is a surrogate.
        /// </summary>
        /// <param name="char16">The input character.</param>
        /// <returns>true if the input character is a surrogate.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsSurrogate(char char16)
        {
            return (char16 & SurrogateBitmask) == SurrogateBits;
        }

        /// <summary>
        /// Determines whether the character is a trail surrogate.
        /// </summary>
        /// <param name="char16">The input character.</param>
        /// <returns>true if the input character is a trail surrogate.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsTrailSurrogate(char char16)
        {
            return (char16 & TrailSurrogateBitmask) == TrailSurrogateBits;
        }

        /// <summary>
        /// Determines whether the character is a lead surrogate.
        /// </summary>
        /// <param name="char16">The input character.</param>
        /// <returns>true if the input character is a lead surrogate.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLeadSurrogate(char char16)
        {
            return (char16 & LeadSurrogateBitmask) == LeadSurrogateBits;
        }

        /// <summary>
        /// Returns the lead surrogate. If a validity check is required, use
        /// <see cref="UChar.IsLegal(int)"/> on <paramref name="char32"/> before calling.
        /// </summary>
        /// <param name="char32">The input character.</param>
        /// <returns>Lead surrogate if the <c>GetCharCount(ch)</c> is 2;
        /// and 0 otherwise (note: 0 is not a valid lead surrogate).</returns>
        /// 
        public static char GetLeadSurrogate(int char32)
        {
            if (char32 >= SupplementaryMinValue)
            {
                return (char)(LeadSurrogateOffset + (char32 >> LeadSurrogateShift));
            }
            return (char)0;
        }

        /// <summary>
        /// Returns the trail surrogate. If a validity check is required, use
        /// <see cref="UChar.IsLegal(int)"/> on <paramref name="char32"/> before calling.
        /// </summary>
        /// <param name="char32">The input character.</param>
        /// <returns>The trail surrogate if the <c>GetCharCount(ch)</c> is 2;
        /// otherwise the character itself.</returns>
        /// <stable>ICU 2.1</stable>
        public static char GetTrailSurrogate(int char32)
        {
            if (char32 >= SupplementaryMinValue)
            {
                return (char)(TrailSurrogateMinValue + (char32 & TrailSurrogateMask));
            }
            return (char)char32;
        }

        /// <summary>
        /// Convenience method corresponding to <c>char + ""</c>. Returns a one or two char string
        /// containing the UTF-32 value in UTF16 format. If a validity check is required, use
        /// <see cref="UChar.IsLegal(int)"/> on <paramref name="char32"/> before calling.
        /// </summary>
        /// <param name="char32">The input character.</param>
        /// <returns>String value of <paramref name="char32"/> in UTF16 format.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="char32"/> is a invalid codepoint.</exception>
        /// <stable>ICU 2.1</stable>
        public static string ValueOf(int char32)
        {
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Illegal codepoint");
            }
            return ToString(char32);
        }

        /// <summary>
        /// Convenience method corresponding to <c>(codepoint at <paramref name="offset16"/>) + ""</c>. Returns a one or
        /// two char string containing the UTF-32 value in UTF16 format. If <paramref name="offset16"/> indexes a surrogate
        /// character, the whole supplementary codepoint will be returned. If a validity check is
        /// required, use <see cref="UChar.IsLegal(int)"/> on the codepoint at 
        /// <paramref name="offset16"/> before calling. The result returned will be a newly created string
        /// obtained by calling <c><paramref name="source"/>.Substring(..)</c> with the appropriate index and length.
        /// </summary>
        /// <param name="source">The input string.</param>
        /// <param name="offset16">The UTF16 index to the codepoint in source.</param>
        /// <returns>String value of the codepoint at <paramref name="offset16"/> in UTF16 format.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ValueOf(string source, int offset16)
        {
            switch (Bounds(source, offset16))
            {
                case LeadSurrogateBoundary:
                    return source.Substring(offset16, 2); // ICU4N: offset16 + 2 - offset16 = 2
                case TrailSurrogateBoundary:
                    return source.Substring(offset16 - 1, 2); // ICU4N: (offset16 + 1) - (offset16 - 1) = 2
                default:
                    return source.Substring(offset16, 1); // ICU4N: offset16 + 1 - offset16 = 1
            }
        }

        /// <summary>
        /// Convenience method corresponding to <c>(codepoint at <paramref name="offset16"/>) + ""</c>. Returns a
        /// one or two char string containing the UTF-32 value in UTF16 format. If <paramref name="offset16"/> indexes a
        /// surrogate character, the whole supplementary codepoint will be returned. If a validity check
        /// is required, use <see cref="UChar.IsLegal(int)"/> on the codepoint at 
        /// <paramref name="offset16"/> before calling. The result returned will be a newly created string
        /// obtained by calling <c><paramref name="source"/>.Substring(..)</c> with the appropriate index and length.
        /// </summary>
        /// <param name="source">The input string builder.</param>
        /// <param name="offset16">The UTF16 index to the codepoint in source.</param>
        /// <returns>String value of the codepoint at <paramref name="offset16"/> in UTF16 format.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ValueOf(StringBuilder source, int offset16)
        {
            switch (Bounds(source, offset16))
            {
                case LeadSurrogateBoundary:
                    return source.ToString(offset16, 2); // offset16 + 2 - offset16 = 2
                case TrailSurrogateBoundary:
                    return source.ToString(offset16 - 1, 2); // (offset16 + 1) - (offset16 - 1) = 2
                default:
                    return source.ToString(offset16, 1); // offset16 + 1 - offset16 = 1
            }
        }

        /// <summary>
        /// Convenience method. Returns a one or two char string containing the UTF-32 value in UTF16
        /// format. If <paramref name="offset16"/> indexes a surrogate character, the whole supplementary codepoint will be
        /// returned, except when either the leading or trailing surrogate character lies out of the
        /// specified subarray. In the latter case, only the surrogate character within bounds will be
        /// returned. If a validity check is required, use <see cref="UChar.IsLegal(int)"/>
        /// on the codepoint at <paramref name="offset16"/> before calling. The result returned will 
        /// be a newly created string containing the relevant characters.
        /// </summary>
        /// <param name="source">The input char array.</param>
        /// <param name="start">Start index of the subarray.</param>
        /// <param name="limit">End index of the subarray.</param>
        /// <param name="offset16">The UTF16 index to the codepoint in source relative to start.</param>
        /// <returns>String value of the codepoint at <paramref name="offset16"/> in UTF16 format.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ValueOf(char[] source, int start, int limit, int offset16)
        {
            switch (Bounds(source, start, limit, offset16))
            {
                case LeadSurrogateBoundary:
                    return new string(source, start + offset16, 2);
                case TrailSurrogateBoundary:
                    return new string(source, start + offset16 - 1, 2);
                default:
                    return new string(source, start + offset16, 1);
            }
        }

        /// <summary>
        /// Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
        /// the <see cref="UTF16"/> class description for notes on roundtripping.
        /// </summary>
        /// <param name="source">The UTF-16 string.</param>
        /// <param name="offset32">UTF-32 offset.</param>
        /// <returns>UTF-16 offset.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset32"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindOffsetFromCodePoint(string source, int offset32)
        {
            char ch;
            int size = source.Length, result = 0, count = offset32;
            if (offset32 < 0 || offset32 > size)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            while (result < size && count > 0)
            {
                ch = source[result];
                if (IsLeadSurrogate(ch) && ((result + 1) < size)
                        && IsTrailSurrogate(source[result + 1]))
                {
                    result++;
                }

                count--;
                result++;
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            return result;
        }

        /// <summary>
        /// Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
        /// the <see cref="UTF16"/> class description for notes on roundtripping.
        /// </summary>
        /// <param name="source">The UTF-16 string buffer.</param>
        /// <param name="offset32">UTF-32 offset.</param>
        /// <returns>UTF-16 offset.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset32"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindOffsetFromCodePoint(StringBuilder source, int offset32)
        {
            char ch;
            int size = source.Length, result = 0, count = offset32;
            if (offset32 < 0 || offset32 > size)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            while (result < size && count > 0)
            {
                ch = source[result];
                if (IsLeadSurrogate(ch) && ((result + 1) < size)
                        && IsTrailSurrogate(source[result + 1]))
                {
                    result++;
                }

                count--;
                result++;
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            return result;
        }

        /// <summary>
        /// Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
        /// the <see cref="UTF16"/> class description for notes on roundtripping.
        /// </summary>
        /// <param name="source">The UTF-16 char array whose substring is to be analyzed.</param>
        /// <param name="start">Offset of the substring to be analyzed.</param>
        /// <param name="limit">Offset of the substring to be analyzed.</param>
        /// <param name="offset32">UTF-32 offset relative to <paramref name="start"/>.</param>
        /// <returns>UTF-16 offset relative to <paramref name="start"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset32"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindOffsetFromCodePoint(char[] source, int start, int limit, int offset32)
        {
            char ch;
            int result = start, count = offset32;
            if (offset32 > limit - start)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            while (result < limit && count > 0)
            {
                ch = source[result];
                if (IsLeadSurrogate(ch) && ((result + 1) < limit)
                        && IsTrailSurrogate(source[result + 1]))
                {
                    result++;
                }

                count--;
                result++;
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(offset32));
            }
            return result - start;
        }

        /// <summary>
        /// Returns the UTF-32 offset corresponding to the first UTF-32 boundary at or after the given
        /// UTF-16 offset. Used for random access. See the <see cref="UTF16"/> class description for
        /// notes on roundtripping.
        /// <para/>
        /// <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
        /// of the <strong>lead</strong> of the pair is returned. </i>
        /// <para/>
        /// To find the UTF-32 length of a string, use:
        /// <code>
        /// len32 = UTF16.CountCodePoint(source, source.Length);
        /// </code>
        /// </summary>
        /// <param name="source">Text to analyze.</param>
        /// <param name="offset16">UTF-16 offset &lt; source text length.</param>
        /// <returns>UTF-32 offset.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindCodePointOffset(string source, int offset16)
        {
            if (offset16 < 0 || offset16 > source.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }

            int result = 0;
            char ch;
            bool hadLeadSurrogate = false;

            for (int i = 0; i < offset16; ++i)
            {
                ch = source[i];
                if (hadLeadSurrogate && IsTrailSurrogate(ch))
                {
                    hadLeadSurrogate = false; // count valid trail as zero
                }
                else
                {
                    hadLeadSurrogate = IsLeadSurrogate(ch);
                    ++result; // count others as 1
                }
            }

            if (offset16 == source.Length)
            {
                return result;
            }

            // end of source being the less significant surrogate character
            // shift result back to the start of the supplementary character
            if (hadLeadSurrogate && (IsTrailSurrogate(source[offset16])))
            {
                result--;
            }

            return result;
        }

        /// <summary>
        /// Returns the UTF-32 offset corresponding to the first UTF-32 boundary at the given UTF-16
        /// offset. Used for random access. See the <see cref="UTF16"/> class description for notes on
        /// roundtripping.
        /// <para/>
        /// <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
        /// of the <strong>lead</strong> of the pair is returned. </i>
        /// <para/>
        /// To find the UTF-32 length of a string, use:
        /// <code>
        /// len32 = UTF16.CountCodePoint(source);
        /// </code>
        /// </summary>
        /// <param name="source">Text to analyze.</param>
        /// <param name="offset16">UTF-16 offset &lt; source text length.</param>
        /// <returns>UTF-32 offset.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindCodePointOffset(StringBuilder source, int offset16)
        {
            if (offset16 < 0 || offset16 > source.Length)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }

            int result = 0;
            char ch;
            bool hadLeadSurrogate = false;

            for (int i = 0; i < offset16; ++i)
            {
                ch = source[i];
                if (hadLeadSurrogate && IsTrailSurrogate(ch))
                {
                    hadLeadSurrogate = false; // count valid trail as zero
                }
                else
                {
                    hadLeadSurrogate = IsLeadSurrogate(ch);
                    ++result; // count others as 1
                }
            }

            if (offset16 == source.Length)
            {
                return result;
            }

            // end of source being the less significant surrogate character
            // shift result back to the start of the supplementary character
            if (hadLeadSurrogate && (IsTrailSurrogate(source[offset16])))
            {
                result--;
            }

            return result;
        }

        /// <summary>
        /// Returns the UTF-32 offset corresponding to the first UTF-32 boundary at the given UTF-16
        /// offset. Used for random access. See the <see cref="UTF16"/> class description for notes on
        /// roundtripping.
        /// <para/>
        /// <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
        /// of the <strong>lead</strong> of the pair is returned. </i>
        /// <para/>
        /// To find the UTF-32 length of a substring, use:
        /// <code>
        /// len32 = UTF16.CountCodePoint(source, start, limit);
        /// </code>
        /// </summary>
        /// <param name="source">Text to analyze.</param>
        /// <param name="start">Offset of the substring.</param>
        /// <param name="limit">Offset of the substring.</param>
        /// <param name="offset16">UTF-16 relative to <paramref name="start"/>.</param>
        /// <returns>UTF-32 offset relative to <paramref name="start"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is not within 
        /// the range of <paramref name="start"/> and <paramref name="limit"/>.</exception>
        /// <stable>ICU 2.1</stable>
        public static int FindCodePointOffset(char[] source, int start, int limit, int offset16)
        {
            offset16 += start;
            if (offset16 > limit)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }

            int result = 0;
            char ch;
            bool hadLeadSurrogate = false;

            for (int i = start; i < offset16; ++i)
            {
                ch = source[i];
                if (hadLeadSurrogate && IsTrailSurrogate(ch))
                {
                    hadLeadSurrogate = false; // count valid trail as zero
                }
                else
                {
                    hadLeadSurrogate = IsLeadSurrogate(ch);
                    ++result; // count others as 1
                }
            }

            if (offset16 == limit)
            {
                return result;
            }

            // end of source being the less significant surrogate character
            // shift result back to the start of the supplementary character
            if (hadLeadSurrogate && (IsTrailSurrogate(source[offset16])))
            {
                result--;
            }

            return result;
        }

        /// <summary>
        /// Append a single UTF-32 value to the end of a <see cref="StringBuilder"/>. If a validity check is required,
        /// use <see cref="UChar.IsLegal(int)"/> on <paramref name="char32"/> before calling.
        /// </summary>
        /// <param name="target">The buffer to append to.</param>
        /// <param name="char32">Value to append.</param>
        /// <returns>The updated <see cref="StringBuilder"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="char32"/> does not 
        /// lie within the range of the Unicode codepoints.</exception>
        /// <stable>ICU 2.1</stable>
        public static StringBuilder Append(StringBuilder target, int char32)
        {
            // Check for irregular values
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Illegal codepoint: " + string.Format("{0:X4}", char32));
            }

            // Write the UTF-16 values
            if (char32 >= SupplementaryMinValue)
            {
                target.Append(GetLeadSurrogate(char32));
                target.Append(GetTrailSurrogate(char32));
            }
            else
            {
                target.Append((char)char32);
            }
            return target;
        }

        /// <summary>
        /// Cover JDK 1.5 APIs. Append the code point to the buffer and return the buffer as a
        /// convenience.
        /// </summary>
        /// <param name="target">The buffer to append to.</param>
        /// <param name="cp">The code point to append.</param>
        /// <returns>The updated <see cref="StringBuilder"/>.</returns>
        /// <exception cref="ArgumentException">If cp is not a valid code point.</exception>
        /// <stable>ICU 3.0</stable>
        public static StringBuilder AppendCodePoint(StringBuilder target, int cp)
        {
            return Append(target, cp);
        }

        /// <summary>
        /// Adds a codepoint to offset16 position of the argument char array.
        /// </summary>
        /// <param name="target">Char array to be append with the new code point.</param>
        /// <param name="limit">UTF16 offset which the codepoint will be appended.</param>
        /// <param name="char32">Code point to be appended.</param>
        /// <returns>Offset after <paramref name="char32"/> in the array.</returns>
        /// <exception cref="ArgumentException">Thrown if there is not enough space for the append, or when 
        /// <paramref name="char32"/> does not lie within the range of the Unicode codepoints.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Append(char[] target, int limit, int char32)
        {
            // Check for irregular values
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Illegal codepoint");
            }
            // Write the UTF-16 values
            if (char32 >= SupplementaryMinValue)
            {
                target[limit++] = GetLeadSurrogate(char32);
                target[limit++] = GetTrailSurrogate(char32);
            }
            else
            {
                target[limit++] = (char)char32;
            }
            return limit;
        }

        /// <summary>
        /// Number of codepoints in a UTF16 string.
        /// </summary>
        /// <param name="source">UTF16 string.</param>
        /// <returns>Number of codepoint in string.</returns>
        /// <stable>ICU 2.1</stable>
        public static int CountCodePoint(string source)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, source.Length);
        }

        /// <summary>
        /// Number of codepoints in a UTF16 string buffer.
        /// </summary>
        /// <param name="source">UTF16 string buffer.</param>
        /// <returns>Number of codepoint in string.</returns>
        /// <stable>ICU 2.1</stable>
        public static int CountCodePoint(StringBuilder source)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, source.Length);
        }

        /// <summary>
        /// Number of codepoints in a UTF16 char array substring.
        /// </summary>
        /// <param name="source">UTF16 char array.</param>
        /// <param name="start">Offset of the substring.</param>
        /// <param name="limit">Offset of the substring.</param>
        /// <returns>Number of codepoint in the substring.</returns>
        /// <exception cref="IndexOutOfRangeException">If start and limit are not valid.</exception>
        /// <stable>ICU 2.1</stable>
        public static int CountCodePoint(char[] source, int start, int limit)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, start, limit, limit - start);
        }

        /// <summary>
        /// Set a code point into a UTF16 position. Adjusts target according if we are replacing a
        /// non-supplementary codepoint with a supplementary and vice versa.
        /// </summary>
        /// <param name="target">Target <see cref="StringBuilder"/>.</param>
        /// <param name="offset16">UTF16 position to insert into.</param>
        /// <param name="char32">Code point.</param>
        /// <stable>ICU 2.1</stable>
        public static void SetCharAt(StringBuilder target, int offset16, int char32)
        {
            int count = 1;
            char single = target[offset16];

            if (IsSurrogate(single))
            {
                // pairs of the surrogate with offset16 at the lead char found
                if (IsLeadSurrogate(single) && (target.Length > offset16 + 1)
                        && IsTrailSurrogate(target[offset16 + 1]))
                {
                    count++;
                }
                else
                {
                    // pairs of the surrogate with offset16 at the trail char
                    // found
                    if (IsTrailSurrogate(single) && (offset16 > 0)
                            && IsLeadSurrogate(target[offset16 - 1]))
                    {
                        offset16--;
                        count++;
                    }
                }
            }
            target.Replace(offset16, count, ValueOf(char32)); // ICU4N: Corrected 2nd parameter
        }

        /// <summary>
        /// Set a code point into a UTF16 position in a char array. Adjusts target according if we are
        /// replacing a non-supplementary codepoint with a supplementary and vice versa.
        /// </summary>
        /// <param name="target">Target char array.</param>
        /// <param name="limit">Numbers of valid chars in <paramref name="target"/>, different from target.Length. Limit counts the
        /// number of chars in target that represents a string, not the size of array target.</param>
        /// <param name="offset16">UTF16 position to insert into.</param>
        /// <param name="char32">Code point.</param>
        /// <returns>New number of chars in target that represents a string.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="offset16"/> is out of range.</exception>
        /// <stable>ICU 2.1</stable>
        public static int SetCharAt(char[] target, int limit, int offset16, int char32)
        {
            if (offset16 >= limit)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }
            int count = 1;
            char single = target[offset16];

            if (IsSurrogate(single))
            {
                // pairs of the surrogate with offset16 at the lead char found
                if (IsLeadSurrogate(single) && (target.Length > offset16 + 1)
                        && IsTrailSurrogate(target[offset16 + 1]))
                {
                    count++;
                }
                else
                {
                    // pairs of the surrogate with offset16 at the trail char
                    // found
                    if (IsTrailSurrogate(single) && (offset16 > 0)
                            && IsLeadSurrogate(target[offset16 - 1]))
                    {
                        offset16--;
                        count++;
                    }
                }
            }

            string str = ValueOf(char32);
            int result = limit;
            int strlength = str.Length;
            target[offset16] = str[0];
            if (count == strlength)
            {
                if (count == 2)
                {
                    target[offset16 + 1] = str[1];
                }
            }
            else
            {
                // this is not exact match in space, we'll have to do some
                // shifting
                System.Array.Copy(target, offset16 + count, target, offset16 + strlength, limit
                        - (offset16 + count));
                if (count < strlength)
                {
                    // char32 is a supplementary character trying to squeeze into
                    // a non-supplementary space
                    target[offset16 + 1] = str[1];
                    result++;
                    if (result < target.Length)
                    {
                        target[result] = (char)0;
                    }
                }
                else
                {
                    // char32 is a non-supplementary character trying to fill
                    // into a supplementary space
                    result--;
                    target[result] = (char)0;
                }
            }
            return result;
        }

        /// <summary>
        /// Shifts <paramref name="offset16"/> by the argument number of codepoints.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="offset16">UTF16 position to shift.</param>
        /// <param name="shift32">Number of codepoints to shift.</param>
        /// <returns>New shifted <paramref name="offset16"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">If the new <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int MoveCodePointOffset(string source, int offset16, int shift32)
        {
            int result = offset16;
            int size = source.Length;
            int count;
            char ch;
            if (offset16 < 0 || offset16 > size)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }
            if (shift32 > 0)
            {
                if (shift32 + offset16 > size)
                {
                    throw new IndexOutOfRangeException(nameof(offset16));
                }
                count = shift32;
                while (result < size && count > 0)
                {
                    ch = source[result];
                    if (IsLeadSurrogate(ch) && ((result + 1) < size)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        result++;
                    }
                    count--;
                    result++;
                }
            }
            else
            {
                if (offset16 + shift32 < 0)
                {
                    throw new IndexOutOfRangeException(nameof(offset16));
                }
                for (count = -shift32; count > 0; count--)
                {
                    result--;
                    if (result < 0)
                    {
                        break;
                    }
                    ch = source[result];
                    if (IsTrailSurrogate(ch) && result > 0
                            && IsLeadSurrogate(source[result - 1]))
                    {
                        result--;
                    }
                }
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(shift32));
            }
            return result;
        }

        /// <summary>
        /// Shifts <paramref name="offset16"/> by the argument number of codepoints.
        /// </summary>
        /// <param name="source">Source string buffer.</param>
        /// <param name="offset16">UTF16 position to shift.</param>
        /// <param name="shift32">Number of codepoints to shift.</param>
        /// <returns>New shifted <paramref name="offset16"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">If the new <paramref name="offset16"/> is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        public static int MoveCodePointOffset(StringBuilder source, int offset16, int shift32)
        {
            int result = offset16;
            int size = source.Length;
            int count;
            char ch;
            if (offset16 < 0 || offset16 > size)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }
            if (shift32 > 0)
            {
                if (shift32 + offset16 > size)
                {
                    throw new IndexOutOfRangeException(nameof(offset16));
                }
                count = shift32;
                while (result < size && count > 0)
                {
                    ch = source[result];
                    if (IsLeadSurrogate(ch) && ((result + 1) < size)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        result++;
                    }
                    count--;
                    result++;
                }
            }
            else
            {
                if (offset16 + shift32 < 0)
                {
                    throw new IndexOutOfRangeException(nameof(offset16));
                }
                for (count = -shift32; count > 0; count--)
                {
                    result--;
                    if (result < 0)
                    {
                        break;
                    }
                    ch = source[result];
                    if (IsTrailSurrogate(ch) && result > 0
                            && IsLeadSurrogate(source[result - 1]))
                    {
                        result--;
                    }
                }
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(shift32));
            }
            return result;
        }

        /// <summary>
        /// Shifts <paramref name="offset16"/> by the argument number of codepoints within a subarray.
        /// </summary>
        /// <param name="source">Char array.</param>
        /// <param name="start">Position of the subarray to be performed on.</param>
        /// <param name="limit">Position of the subarray to be performed on.</param>
        /// <param name="offset16">UTF16 position to shift relative to <paramref name="start"/>.</param>
        /// <param name="shift32">Number of codepoints to shift.</param>
        /// <returns>New shifted <paramref name="offset16"/> relative to start.</returns>
        /// <exception cref="IndexOutOfRangeException">If the new <paramref name="offset16"/> is out of bounds with respect to the subarray or the
        /// subarray bounds are out of range.</exception>
        /// <stable>ICU 2.1</stable>
        public static int MoveCodePointOffset(char[] source, int start, int limit, int offset16,
                int shift32)
        {
            int size = source.Length;
            int count;
            char ch;
            int result = offset16 + start;
            if (start < 0 || limit < start)
            {
                throw new IndexOutOfRangeException(nameof(start));
            }
            if (limit > size)
            {
                throw new IndexOutOfRangeException(nameof(limit));
            }
            if (offset16 < 0 || result > limit)
            {
                throw new IndexOutOfRangeException(nameof(offset16));
            }
            if (shift32 > 0)
            {
                if (shift32 + result > size)
                {
                    throw new IndexOutOfRangeException(nameof(result));
                }
                count = shift32;
                while (result < limit && count > 0)
                {
                    ch = source[result];
                    if (IsLeadSurrogate(ch) && (result + 1 < limit)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        result++;
                    }
                    count--;
                    result++;
                }
            }
            else
            {
                if (result + shift32 < start)
                {
                    throw new IndexOutOfRangeException(nameof(result));
                }
                for (count = -shift32; count > 0; count--)
                {
                    result--;
                    if (result < start)
                    {
                        break;
                    }
                    ch = source[result];
                    if (IsTrailSurrogate(ch) && result > start && IsLeadSurrogate(source[result - 1]))
                    {
                        result--;
                    }
                }
            }
            if (count != 0)
            {
                throw new IndexOutOfRangeException(nameof(shift32));
            }
            result -= start;
            return result;
        }

        /// <summary>
        /// Inserts <paramref name="char32"/> codepoint into target at the argument offset16. If the offset16 is in the
        /// middle of a supplementary codepoint, <paramref name="char32"/> will be inserted after the supplementary
        /// codepoint. The length of target increases by one if codepoint is non-supplementary, 2
        /// otherwise.
        /// <para/>
        /// The overall effect is exactly as if the argument were converted to a string by
        /// appending an empty string to a <see cref="char"/> and the characters in that string 
        /// were then inserted into target at the position indicated by <paramref name="offset16"/>.
        /// <para/>
        /// The <paramref name="offset16"/> argument must be greater than or equal to 0, and less than or equal to the length
        /// of source.
        /// </summary>
        /// <param name="target">String buffer to insert to.</param>
        /// <param name="offset16">Offset which <paramref name="char32"/> will be inserted in.</param>
        /// <param name="char32">Codepoint to be inserted.</param>
        /// <returns>A reference to <paramref name="target"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="offset16"/> is invalid.</exception>
        /// <stable>ICU 2.1</stable>
        public static StringBuilder Insert(StringBuilder target, int offset16, int char32)
        {
            string str = ValueOf(char32);
            if (offset16 != target.Length && Bounds(target, offset16) == TrailSurrogateBoundary)
            {
                offset16++;
            }
            target.Insert(offset16, str);
            return target;
        }

        /// <summary>
        /// Inserts <paramref name="char32"/> codepoint into target at the argument <paramref name="offset16"/>. 
        /// If the <paramref name="offset16"/> is in the middle of a supplementary codepoint, 
        /// <paramref name="char32"/> will be inserted after the supplementary
        /// codepoint. Limit increases by one if codepoint is non-supplementary, 2 otherwise.
        /// <para/>
        /// The overall effect is exactly as if the argument were converted to a string by the appending an empty
        /// string to a <see cref="char"/> and the characters in that string were then inserted into target at the
        /// position indicated by <paramref name="offset16"/>.
        /// <para/>
        /// The offset argument must be greater than or equal to 0, and less than or equal to the <paramref name="limit"/>.
        /// </summary>
        /// <param name="target">Char array to insert to.</param>
        /// <param name="limit">End index of the char array, limit &lt;= <paramref name="target"/>.Length.</param>
        /// <param name="offset16">Offset which <paramref name="char32"/> will be inserted in.</param>
        /// <param name="char32">Codepoint to be inserted.</param>
        /// <returns>New limit size.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="offset16"/> is invalid.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Insert(char[] target, int limit, int offset16, int char32)
        {
            string str = ValueOf(char32);
            if (offset16 != limit && Bounds(target, 0, limit, offset16) == TrailSurrogateBoundary)
            {
                offset16++;
            }
            int size = str.Length;
            if (limit + size > target.Length)
            {
                throw new IndexOutOfRangeException("offset16 + size");
            }
            System.Array.Copy(target, offset16, target, offset16 + size, limit - offset16);
            target[offset16] = str[0];
            if (size == 2)
            {
                target[offset16 + 1] = str[1];
            }
            return limit + size;
        }

        /// <summary>
        /// Removes the codepoint at the specified position in this target (shortening target by 1
        /// character if the codepoint is a non-supplementary, 2 otherwise).
        /// </summary>
        /// <param name="target">String buffer to remove codepoint from.</param>
        /// <param name="offset16">Offset which the codepoint will be removed.</param>
        /// <returns>A reference to target.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="offset16"/> is invalid.</exception>
        /// <stable>ICU 2.1</stable>
        public static StringBuilder Delete(StringBuilder target, int offset16)
        {
            int count = 1;
            switch (Bounds(target, offset16))
            {
                case LeadSurrogateBoundary:
                    count++;
                    break;
                case TrailSurrogateBoundary:
                    count++;
                    offset16--;
                    break;
            }
            target.Delete(offset16, count); // ICU4N: Corrected 2nd parameter of Delete
            return target;
        }

        /// <summary>
        /// Removes the codepoint at the specified position in this target (shortening target by 1
        /// character if the codepoint is a non-supplementary, 2 otherwise).
        /// </summary>
        /// <param name="target">String buffer to remove codepoint from.</param>
        /// <param name="limit">End index of the char array, limit &lt;= <paramref name="target"/>.Length.</param>
        /// <param name="offset16">Offset which the codepoint will be removed.</param>
        /// <returns>A new limit size.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="offset16"/> is invalid.</exception>
        /// <stable>ICU 2.1</stable>
        public static int Delete(char[] target, int limit, int offset16)
        {
            int count = 1;
            switch (Bounds(target, 0, limit, offset16))
            {
                case LeadSurrogateBoundary:
                    count++;
                    break;
                case TrailSurrogateBoundary:
                    count++;
                    offset16--;
                    break;
            }
            System.Array.Copy(target, offset16 + count, target, offset16, limit - (offset16 + count));
            target[limit - count] = (char)0;
            return limit - count;
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument codepoint. I.e., the smallest index <c>i</c> such that
        /// <c>UTF16.CharAt(source, i) == char32</c> is true.
        /// <para/>
        /// If no such character occurs in this string, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", 'a') returns 0</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", 0x10000) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="char32">Codepoint to search for.</param>
        /// <returns>The index of the first occurrence of the codepoint in the argument Unicode string, or
        /// -1 if the codepoint does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int IndexOf(string source, int char32)
        {
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LeadSurrogateMinValue
                    || (char32 > TrailSurrogateMaxValue && char32 < SupplementaryMinValue))
            {
                return source.IndexOf((char)char32);
            }
            // surrogate
            if (char32 < SupplementaryMinValue)
            {
                int result = source.IndexOf((char)char32);
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return IndexOf(source, char32, result + 1);
                    }
                    // trail surrogate
                    if (result > 0 && IsLeadSurrogate(source[result - 1]))
                    {
                        return IndexOf(source, char32, result + 1);
                    }
                }
                return result;
            }
            // supplementary
            string char32str = ToString(char32);
            return source.IndexOf(char32str, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument string <paramref name="str"/>. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity. Hence if the <paramref name="str"/>
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before <paramref name="str"/> found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// If no such string <paramref name="str"/> occurs in this <paramref name="source"/>, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", "ab") returns 0</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00") returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800") returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <returns>the index of the first occurrence of the codepoint in the argument Unicode string, or
        /// -1 if the codepoint does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int IndexOf(string source, string str)
        {
            return IndexOf(source, str, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument string <paramref name="str"/>. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity. Hence if the <paramref name="str"/>
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before <paramref name="str"/> found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// If no such string <paramref name="str"/> occurs in this <paramref name="source"/>, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", "ab") returns 0</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00") returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800") returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <returns>the index of the first occurrence of the codepoint in the argument Unicode string, or
        /// -1 if the codepoint does not occur.</returns>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <stable>ICU4N 60.1</stable>
        public static int IndexOf(string source, string str, StringComparison comparisonType)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.IndexOf(str, comparisonType);
            }

            int result = source.IndexOf(str, comparisonType);
            int resultEnd = result + strLength;
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[resultEnd + 1]))
                {
                    return IndexOf(source, str, resultEnd + 1, comparisonType);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return IndexOf(source, str, resultEnd + 1, comparisonType);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument codepoint. I.e., the smallest index i such that:
        /// <para/>
        /// (UTF16.CharAt(source, i) == char32 &amp;&amp; i &gt;= <paramref name="startIndex"/>) is true.
        /// <para/>
        /// If no such character occurs in this string, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", 'a', 1) returns -1</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", 0x10000, 1) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", 0xd800, 1) returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="char32">Codepoint to search for.</param>
        /// <param name="startIndex">The index to start the search from.</param>
        /// <returns>The index of the first occurrence of the codepoint in the argument Unicode string at
        /// or after <paramref name="startIndex"/>, or -1 if the codepoint does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int IndexOf(string source, int char32, int startIndex)
        {
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
            // non-surrogate bmp
            if (char32 < LeadSurrogateMinValue
                    || (char32 > TrailSurrogateMaxValue && char32 < SupplementaryMinValue))
            {
                return source.IndexOf((char)char32, startIndex);
            }
            // surrogate
            if (char32 < SupplementaryMinValue)
            {
                int result = source.IndexOf((char)char32, startIndex);
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return IndexOf(source, char32, result + 1);
                    }
                    // trail surrogate
                    if (result > 0 && IsLeadSurrogate(source[result - 1]))
                    {
                        return IndexOf(source, char32, result + 1);
                    }
                }
                return result;
            }
            // supplementary
            string char32str = ToString(char32);
            return source.IndexOf(char32str, startIndex, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument string <paramref name="str"/>. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity.e Hence if the <paramref name="str"/>
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before <paramref name="str"/> found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// If no such string <paramref name="str"/> occurs in this <paramref name="source"/>, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", "ab", 0) returns 0</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00", 0) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00", 2) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800", 0) returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="startIndex">The index to start the search from.</param>
        /// <returns>The index of the first occurrence of the codepoint in the argument Unicode string, or
        /// -1 if the codepoint does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int IndexOf(string source, string str, int startIndex)
        {
            return IndexOf(source, str, startIndex, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the first occurrence of
        /// the argument string <paramref name="str"/>. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity.e Hence if the <paramref name="str"/>
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before <paramref name="str"/> found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// If no such string <paramref name="str"/> occurs in this <paramref name="source"/>, then -1 is returned.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.IndexOf("abc", "ab", 0) returns 0</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00", 0) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800\udc00", 2) returns 3</description></item>
        ///     <item><description>UTF16.IndexOf("abc\ud800\udc00", "\ud800", 0) returns -1</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="startIndex">The index to start the search from.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The index of the first occurrence of the codepoint in the argument Unicode string, or
        /// -1 if the codepoint does not occur.</returns>
        /// <stable>ICU4N 60.1</stable>
        public static int IndexOf(string source, string str, int startIndex, StringComparison comparisonType)
        {
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.IndexOf(str, startIndex, comparisonType);
            }

            int result = source.IndexOf(str, startIndex, comparisonType);
            int resultEnd = result + strLength;
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[resultEnd]))
                {
                    return IndexOf(source, str, resultEnd + 1, comparisonType);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return IndexOf(source, str, resultEnd + 1, comparisonType);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument codepoint. I.e., the index returned is the largest value i such that:
        /// UTF16.CharAt(source, i) == char32 is true.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", 'a') returns 0</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="char32">Codepoint to search for.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int LastIndexOf(string source, int char32)
        {
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LeadSurrogateMinValue
                    || (char32 > TrailSurrogateMaxValue && char32 < SupplementaryMinValue))
            {
                return source.LastIndexOf((char)char32);
            }
            // surrogate
            if (char32 < SupplementaryMinValue)
            {
                int result = source.LastIndexOf((char)char32);
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return LastIndexOf(source, char32, Math.Max(result - 1, 0)); // ICU4N: Corrected 3rd parameter
                    }
                    // trail surrogate
                    if (result > 0 && IsLeadSurrogate(source[result - 1]))
                    {
                        return LastIndexOf(source, char32, result - 1);
                    }
                }
                return result;
            }
            // supplementary
            string char32str = ToString(char32);
            return source.LastIndexOf(char32str, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument string str. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity.e Hence if the str
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before str found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends str.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", "a") returns 0</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        public static int LastIndexOf(string source, string str)
        {
            return LastIndexOf(source, str, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument string str. This method is implemented based on codepoints, hence a "lead
        /// surrogate character + trail surrogate character" is treated as one entity.e Hence if the str
        /// starts with trail surrogate character at index 0, a source with a leading a surrogate
        /// character before str found at in source will not have a valid match. Vice versa for lead
        /// surrogates that ends str.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", "a") returns 0</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <stable>ICU4N 60.1</stable>
        public static int LastIndexOf(string source, string str, StringComparison comparisonType)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.LastIndexOf(str, comparisonType);
            }

            int result = source.LastIndexOf(str, comparisonType);
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[result + strLength + 1]))
                {
                    return LastIndexOf(source, str, Math.Max(result - 1, 0), comparisonType); // ICU4N: Corrected 3rd parameter
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return LastIndexOf(source, str, result - 1, comparisonType); 
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument codepoint, where the result is less than or equals to <paramref name="startIndex"/>.
        /// </summary>
        /// <remarks>
        /// This method is implemented based on codepoints, hence a single surrogate character will not
        /// match a supplementary character.
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character starting at the specified index.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", 'c', 2) returns 2</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc", 'c', 1) returns -1</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000, 5) returns 3.</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000, 3) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// <para/>
        /// Note this method is similar to the ICU4J lastIndexOf() implementation in that it does not throw an <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="startIndex"/> is negative or >= the length of <paramref name="source"/>.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="char32">Codepoint to search for.</param>
        /// <param name="startIndex">The index to start the search from. There is no restriction on the value of
        /// fromIndex. If it is greater than or equal to the length of this string, it has the
        /// same effect as if it were equal to one less than the length of this string: this
        /// entire string may be searched. If it is negative, it has the same effect as if it
        /// were -1: -1 is returned.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <stable>ICU 2.6</stable>
        internal static int SafeLastIndexOf(string source, int char32, int startIndex) // ICU4N TODO: API - make public ?
        {
            return LastIndexOf(source, char32, Math.Max(Math.Min(startIndex, source.Length - 1), 0));
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument codepoint, where the result is less than or equals to <paramref name="startIndex"/>.
        /// </summary>
        /// <remarks>
        /// This method is implemented based on codepoints, hence a single surrogate character will not
        /// match a supplementary character.
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character starting at the specified index.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", 'c', 2) returns 2</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc", 'c', 1) returns -1</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000, 5) throws <see cref="ArgumentOutOfRangeException"/>.</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0x10000, 3) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", 0xd800) returns -1</description></item>
        /// </list>
        /// <para/>
        /// Note this method differs from the ICU4J implementation in that it throws an <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="startIndex"/> is negative or >= the length of <paramref name="source"/>.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="char32">Codepoint to search for.</param>
        /// <param name="startIndex">The index to start the search from. 
        /// The search proceeds from startIndex toward the beginning of <paramref name="source"/>.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="startIndex"/> is negative or greater than 
        /// <paramref name="source"/>.Length - 1.</exception>
        /// <stable>ICU4N 60.1</stable>
        public static int LastIndexOf(string source, int char32, int startIndex)
        {
            if (char32 < CodePointMinValue || char32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
            // non-surrogate bmp
            if (char32 < LeadSurrogateMinValue
                    || (char32 > TrailSurrogateMaxValue && char32 < SupplementaryMinValue))
            {
                // ICU4N: There appears to be a bug in .NET - the overloads that accept char
                // throw an ArgumentOutOfRangeException for source.Length, but the ones that
                // accept string accept source.Length.
                return source.LastIndexOf((char)char32, Math.Min(startIndex, source.Length - 1));
            }
            // surrogate
            if (char32 < SupplementaryMinValue)
            {
                int result = source.LastIndexOf((char)char32, Math.Min(startIndex, source.Length - 1));
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return LastIndexOf(source, char32, Math.Max(result - 1, 0)); // ICU4N: Corrected 3rd parameter
                    }
                    // trail surrogate
                    if (result > 0 && IsLeadSurrogate(source[result - 1]))
                    {
                        return LastIndexOf(source, char32, result - 1);
                    }
                }
                return result;
            }
            // supplementary
            string char32str = ToString(char32);
            return source.LastIndexOf(char32str, startIndex, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument string <paramref name="str"/>, where the result is less than or equals to <paramref name="startIndex"/>.
        /// </summary>
        /// <remarks>
        /// This method is implemented based on codepoints, hence a "lead surrogate character + trail
        /// surrogate character" is treated as one entity. Hence if the <paramref name="str"/> starts with trail surrogate
        /// character at index 0, a source with a leading a surrogate character before <paramref name="str"/> found at in
        /// source will not have a valid match. Vice versa for lead surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 2) returns 2</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 1) returns -1</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 5) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 3) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800", 4) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// <para/>
        /// Note this method is similar to the ICU4J implementation in that it does not throw an <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="startIndex"/> is negative or >= the length of <paramref name="source"/>.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="startIndex">The index to start the search from.  There is no restriction on the value of
        /// fromIndex. If it is greater than or equal to the length of this string, it has the
        /// same effect as if it were equal to one less than the length of this string: this
        /// entire string may be searched. If it is negative, it has the same effect as if it
        /// were -1: -1 is returned.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <stable>ICU4N 60.1</stable>
        internal static int SafeLastIndexOf(string source, string str, int startIndex) // ICU4N TODO: Make public ?
        {
            return LastIndexOf(source, str, Math.Max(Math.Min(startIndex, source.Length - 1), 0), StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument string <paramref name="str"/>, where the result is less than or equals to <paramref name="startIndex"/>.
        /// </summary>
        /// <remarks>
        /// This method is implemented based on codepoints, hence a "lead surrogate character + trail
        /// surrogate character" is treated as one entity. Hence if the <paramref name="str"/> starts with trail surrogate
        /// character at index 0, a source with a leading a surrogate character before <paramref name="str"/> found at in
        /// source will not have a valid match. Vice versa for lead surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 2) returns 2</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 1) returns -1</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 5) throws <see cref="ArgumentOutOfRangeException"/>.</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 3) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800", 4) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// <para/>
        /// Note this method differs from the ICU4J implementation in that it throws an <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="startIndex"/> is negative or >= the length of <paramref name="source"/>.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="startIndex">The index to start the search from. 
        /// The search proceeds from <paramref name="startIndex"/> toward the beginning of <paramref name="source"/>.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="startIndex"/> is negative or greater than 
        /// <paramref name="source"/>.Length - 1.</exception>
        /// <stable>ICU4N 60.1</stable>
        public static int LastIndexOf(string source, string str, int startIndex)
        {
            return LastIndexOf(source, str, startIndex, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns the index within the argument UTF16 format Unicode string of the last occurrence of
        /// the argument string <paramref name="str"/>, where the result is less than or equals to <paramref name="startIndex"/>.
        /// </summary>
        /// <remarks>
        /// This method is implemented based on codepoints, hence a "lead surrogate character + trail
        /// surrogate character" is treated as one entity. Hence if the <paramref name="str"/> starts with trail surrogate
        /// character at index 0, a source with a leading a surrogate character before <paramref name="str"/> found at in
        /// source will not have a valid match. Vice versa for lead surrogates that ends <paramref name="str"/>.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 2) returns 2</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc", "c", 1) returns -1</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 5) throws <see cref="ArgumentOutOfRangeException"/>.</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800\udc00", 3) returns 3</description></item>
        ///     <item><description>UTF16.LastIndexOf("abc\ud800\udc00", "\ud800", 4) returns -1</description></item>
        /// </list>
        /// <para/>
        /// <paramref name="source"/> is searched backwards starting at the last character.
        /// <para/>
        /// Note this method differs from the ICU4J implementation in that it throws an <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="startIndex"/> is negative or >= the length of <paramref name="source"/>.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string that will be searched.</param>
        /// <param name="str">UTF16 format Unicode string to search for.</param>
        /// <param name="startIndex">The index to start the search from. 
        /// The search proceeds from <paramref name="startIndex"/> toward the beginning of <paramref name="source"/>.</param>
        /// <returns>The index of the last occurrence of the codepoint in source, or -1 if the codepoint
        /// does not occur.</returns>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="startIndex"/> is negative or greater than 
        /// <paramref name="source"/>.Length - 1.</exception>
        /// <stable>ICU4N 60.1</stable>
        public static int LastIndexOf(string source, string str, int startIndex, StringComparison comparisonType)
        {
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.LastIndexOf(str, startIndex, comparisonType);
            }

            int result = source.LastIndexOf(str, startIndex, comparisonType);
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[result + strLength]))
                {
                    return LastIndexOf(source, str, Math.Max(result - 1, 0), comparisonType); // ICU4N: Corrected 3rd parameter
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return LastIndexOf(source, str, result - 1, comparisonType);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new UTF16 format Unicode string resulting from replacing all occurrences of
        /// <paramref name="oldChar32"/> in source with <paramref name="newChar32"/>. If the character 
        /// <paramref name="oldChar32"/> does not occur in the UTF16 format Unicode string source, then 
        /// source will be returned. Otherwise, a new string object is created that represents a codepoint 
        /// sequence identical to the codepoint sequence represented by source, except that every occurrence 
        /// of <paramref name="oldChar32"/> is replaced by an occurrence of <paramref name="newChar32"/>.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <code>
        /// UTF16.Replace("mesquite in your cellar", 'e', 'o'); // returns "mosquito in your collar"
        /// UTF16.Replace("JonL", 'q', 'x'); // returns "JonL" (no change)
        /// UTF16.Replace("Supplementary character \ud800\udc00", 0x10000, '!'); // returns "Supplementary character !"
        /// UTF16.Replace("Supplementary character \ud800\udc00", 0xd800, '!'); // returns "Supplementary character \ud800\udc00"
        /// </code>
        /// <para/>
        /// Note this method is provided as support to jdk 1.3, which does not support supplementary
        /// characters to its fullest.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string which the codepoint replacements will be based on.</param>
        /// <param name="oldChar32">Non-zero old codepoint to be replaced.</param>
        /// <param name="newChar32">The new codepoint to replace <paramref name="oldChar32"/>.</param>
        /// <returns>new string derived from source by replacing every occurrence of <paramref name="oldChar32"/> with
        /// <paramref name="newChar32"/>, unless when no <paramref name="oldChar32"/> is found in source then source will be returned.</returns>
        /// <stable>ICU 2.6</stable>
        public static string Replace(string source, int oldChar32, int newChar32)
        {
            if (oldChar32 <= 0 || oldChar32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument oldChar32 is not a valid codepoint");
            }
            if (newChar32 <= 0 || newChar32 > CodePointMaxValue)
            {
                throw new ArgumentException("Argument newChar32 is not a valid codepoint");
            }

            int index = IndexOf(source, oldChar32);
            if (index == -1)
            {
                return source;
            }
            string newChar32Str = ToString(newChar32);
            int oldChar32Size = 1;
            int newChar32Size = newChar32Str.Length;
            StringBuilder result = new StringBuilder(source);
            int resultIndex = index;

            if (oldChar32 >= SupplementaryMinValue)
            {
                oldChar32Size = 2;
            }

            while (index != -1)
            {
                //int endResultIndex = resultIndex + oldChar32Size;
                result.Replace(resultIndex, oldChar32Size, newChar32Str); // ICU4N: Corrected 2nd parameter
                int lastEndIndex = index + oldChar32Size;
                index = IndexOf(source, oldChar32, lastEndIndex);
                resultIndex += newChar32Size + index - lastEndIndex;
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns a new UTF16 format Unicode string resulting from replacing all occurrences of <paramref name="oldStr"/>
        /// in <paramref name="source"/> with <paramref name="newStr"/>. If the string <paramref name="oldStr"/> does not 
        /// occur in the UTF16 format Unicode string <paramref name="source"/>, then source will be returned. Otherwise, a new 
        /// string object is created that represents a codepoint sequence identical to the codepoint sequence 
        /// represented by <paramref name="source"/>, except that every occurrence of <paramref name="oldStr"/> is replaced by 
        /// an occurrence of <paramref name="newStr"/>.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <code>
        /// UTF16.Replace("mesquite in your cellar", "e", "o"); // returns "mosquito in your collar"
        /// UTF16.Replace("mesquite in your cellar", "mesquite", "cat"); // returns "cat in your cellar"
        /// UTF16.Replace("JonL", "q", "x"); // returns "JonL" (no change)
        /// UTF16.Replace("Supplementary character \ud800\udc00", "\ud800\udc00", '!'); // returns "Supplementary character !"
        /// UTF16.Replace("Supplementary character \ud800\udc00", "\ud800", '!'); // returns "Supplementary character \ud800\udc00"
        /// </code>
        /// <para/>
        /// Note this method is provided as support to jdk 1.3, which does not support supplementary
        /// characters to its fullest.
        /// </remarks>
        /// <param name="source">UTF16 format Unicode string which the replacements will be based on.</param>
        /// <param name="oldStr">Non-zero-length string to be replaced.</param>
        /// <param name="newStr">The new string to replace <paramref name="oldStr"/>.</param>
        /// <returns>New string derived from <paramref name="source"/> by replacing every occurrence of 
        /// <paramref name="oldStr"/> with <paramref name="newStr"/>.
        /// When no <paramref name="oldStr"/> is found in <paramref name="source"/>, then source will be returned.
        /// </returns>
        /// <stable>ICU 2.6</stable>
        public static string Replace(string source, string oldStr, string newStr)
        {
            int index = IndexOf(source, oldStr);
            if (index == -1)
            {
                return source;
            }
            int oldStrSize = oldStr.Length;
            int newStrSize = newStr.Length;
            StringBuilder result = new StringBuilder(source);
            int resultIndex = index;

            while (index != -1)
            {
                //int endResultIndex = resultIndex + oldStrSize;
                result.Replace(resultIndex, oldStrSize, newStr); // ICU4N: Corrected 2nd parameter
                int lastEndIndex = index + oldStrSize;
                index = IndexOf(source, oldStr, lastEndIndex);
                resultIndex += newStrSize + index - lastEndIndex;
            }
            return result.ToString();
        }

        /// <summary>
        /// Reverses a UTF16 format Unicode string and replaces source's content with it. This method
        /// will reverse surrogate characters correctly, instead of blindly reversing every character.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <code>
        /// UTF16.Reverse(new StringBuilder( "Supplementary characters \ud800\udc00\ud801\udc01")) // returns "\ud801\udc01\ud800\udc00 sretcarahc yratnemelppuS"
        /// </code>
        /// </remarks>
        /// <param name="source">The source <see cref="StringBuilder"/> that contains UTF16 format Unicode string to be reversed.</param>
        /// <returns>A modified source with reversed UTF16 format Unicode string.</returns>
        /// <stable>ICU 2.6</stable>
        public static StringBuilder Reverse(StringBuilder source)
        {
            int length = source.Length;
            StringBuilder result = new StringBuilder(length);
            for (int i = length; i-- > 0;)
            {
                char ch = source[i];
                if (IsTrailSurrogate(ch) && i > 0)
                {
                    char ch2 = source[i - 1];
                    if (IsLeadSurrogate(ch2))
                    {
                        result.Append(ch2);
                        result.Append(ch);
                        --i;
                        continue;
                    }
                }
                result.Append(ch);
            }
            return result;
        }

        /// <summary>
        /// Check if the string contains more Unicode code points than a certain <paramref name="number"/>. This is more
        /// efficient than counting all code points in the entire string and comparing that <paramref name="number"/> with a
        /// threshold. This function may not need to scan the string at all if the length is within a
        /// certain range, and never needs to count more than '<paramref name="number"/> + 1' code points. Logically
        /// equivalent to (UTF16.CountCodePoint(s) &gt; <paramref name="number"/>). A Unicode code point may occupy either one or two
        /// code units.
        /// </summary>
        /// <param name="source">The input string.</param>
        /// <param name="number">The number of code points in the string is compared against the '<paramref name="number"/>' parameter.</param>
        /// <returns>Boolean value for whether the string contains more Unicode code points than '<paramref name="number"/>'.</returns>
        /// <stable>ICU 2.4</stable>
        public static bool HasMoreCodePointsThan(string source, int number)
        {
            if (number < 0)
            {
                return true;
            }
            if (source == null)
            {
                return false;
            }
            int length = source.Length;

            // length >= 0 known
            // source contains at least (length + 1) / 2 code points: <= 2
            // chars per cp
            if (((length + 1) >> 1) > number)
            {
                return true;
            }

            // check if source does not even contain enough chars
            int maxsupplementary = length - number;
            if (maxsupplementary <= 0)
            {
                return false;
            }

            // there are maxsupplementary = length - number more chars than
            // asked-for code points

            // count code points until they exceed and also check that there are
            // no more than maxsupplementary supplementary code points (char pairs)
            int start = 0;
            while (true)
            {
                if (length == 0)
                {
                    return false;
                }
                if (number == 0)
                {
                    return true;
                }
                if (IsLeadSurrogate(source[start++]) && start != length
                        && IsTrailSurrogate(source[start]))
                {
                    start++;
                    if (--maxsupplementary <= 0)
                    {
                        // too many pairs - too few code points
                        return false;
                    }
                }
                --number;
            }
        }

        /// <summary>
        /// Check if the sub-range of char array, from argument <paramref name="start"/> to <paramref name="limit"/>, contains more Unicode
        /// code points than a certain <paramref name="number"/>. This is more efficient than counting all code points in
        /// the entire char array range and comparing that number with a threshold. This function may not
        /// need to scan the char array at all if <paramref name="start"/> and <paramref name="limit"/> is within a certain range, and never
        /// needs to count more than '<paramref name="number"/> + 1' code points. Logically equivalent to
        /// (UTF16.CountCodePoint(source, start, limit) &gt; <paramref name="number"/>). A Unicode code point may occupy either one
        /// or two code units.
        /// </summary>
        /// <param name="source">Array of UTF-16 chars.</param>
        /// <param name="start">Offset to substring in the <paramref name="source"/> array for analyzing.</param>
        /// <param name="limit">Offset to substring in the <paramref name="source"/> array for analyzing.</param>
        /// <param name="number">The number of code points in the string is compared against the '<paramref name="number"/>' parameter.</param>
        /// <returns>Boolean value for whether the string contains more Unicode code points than '<paramref name="number"/>'.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="limit"/> &lt; <paramref name="start"/>.</exception>
        /// <stable>ICU 2.4</stable>
        public static bool HasMoreCodePointsThan(char[] source, int start, int limit, int number)
        {
            int length = limit - start;
            if (length < 0 || start < 0 || limit < 0)
            {
                throw new IndexOutOfRangeException(
                        "Start and limit indexes should be non-negative and start <= limit");
            }
            if (number < 0)
            {
                return true;
            }
            if (source == null)
            {
                return false;
            }

            // length >= 0 known
            // source contains at least (length + 1) / 2 code points: <= 2
            // chars per cp
            if (((length + 1) >> 1) > number)
            {
                return true;
            }

            // check if source does not even contain enough chars
            int maxsupplementary = length - number;
            if (maxsupplementary <= 0)
            {
                return false;
            }

            // there are maxsupplementary = length - number more chars than
            // asked-for code points

            // count code points until they exceed and also check that there are
            // no more than maxsupplementary supplementary code points (char pairs)
            while (true)
            {
                if (length == 0)
                {
                    return false;
                }
                if (number == 0)
                {
                    return true;
                }
                if (IsLeadSurrogate(source[start++]) && start != limit
                        && IsTrailSurrogate(source[start]))
                {
                    start++;
                    if (--maxsupplementary <= 0)
                    {
                        // too many pairs - too few code points
                        return false;
                    }
                }
                --number;
            }
        }

        /// <summary>
        /// Check if the string buffer contains more Unicode code points than a certain <paramref name="number"/>. This is
        /// more efficient than counting all code points in the entire string buffer and comparing that
        /// number with a threshold. This function may not need to scan the string buffer at all if the
        /// length is within a certain range, and never needs to count more than '<paramref name="number"/> + 1' code
        /// points. Logically equivalent to (UTF16.CountCodePoint(s) &gt; <paramref name="number"/>). A Unicode code point may
        /// occupy either one or two code units.
        /// </summary>
        /// <param name="source">The input string buffer.</param>
        /// <param name="number">The number of code points in the string buffer is compared against the '<paramref name="number"/>' parameter.</param>
        /// <returns>Boolean value for whether the string buffer contains more Unicode code points than '<paramref name="number"/>'.</returns>
        /// <stable>ICU 2.4</stable>
        public static bool HasMoreCodePointsThan(StringBuilder source, int number)
        {
            if (number < 0)
            {
                return true;
            }
            if (source == null)
            {
                return false;
            }
            int length = source.Length;

            // length >= 0 known
            // source contains at least (length + 1) / 2 code points: <= 2
            // chars per cp
            if (((length + 1) >> 1) > number)
            {
                return true;
            }

            // check if source does not even contain enough chars
            int maxsupplementary = length - number;
            if (maxsupplementary <= 0)
            {
                return false;
            }

            // there are maxsupplementary = length - number more chars than
            // asked-for code points

            // count code points until they exceed and also check that there are
            // no more than maxsupplementary supplementary code points (char pairs)
            int start = 0;
            while (true)
            {
                if (length == 0)
                {
                    return false;
                }
                if (number == 0)
                {
                    return true;
                }
                if (IsLeadSurrogate(source[start++]) && start != length
                        && IsTrailSurrogate(source[start]))
                {
                    start++;
                    if (--maxsupplementary <= 0)
                    {
                        // too many pairs - too few code points
                        return false;
                    }
                }
                --number;
            }
        }

        /// <summary>
        /// Create a string from an array of <paramref name="codePoints"/>.
        /// </summary>
        /// <param name="codePoints">The code point array.</param>
        /// <param name="offset">The start of the text in the code point array.</param>
        /// <param name="count">The number of code points.</param>
        /// <returns>A string representing the code points between <paramref name="offset"/> and <paramref name="count"/>.</returns>
        /// <exception cref="ArgumentException">If an invalid code point is encountered.</exception>
        /// <exception cref="IndexOutOfRangeException">If the <paramref name="offset"/> or <paramref name="count"/> are out of bounds.</exception>
        /// <stable>ICU 3.0</stable>
        public static string NewString(int[] codePoints, int offset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentException();
            }
            // ICU4N specific - refactored to eliminate the potential exceptions during normal operation, which
            // can significantly impact performance.
            int countThreashold = 1024; // If the number of chars exceeds this, we count them instead of allocating count * 2
            // as a first approximation, assume each codepoint 
            // is 2 characters (since it cannot be longer than this)
            int arrayLength = count * 2;
            // if we go over the threashold, count the number of 
            // chars we will need so we can allocate the precise amount of memory
            if (count > countThreashold)
            {
                arrayLength = 0;
                for (int r = offset, e = offset + count; r < e; ++r)
                {
                    arrayLength += codePoints[r] < 0x010000 ? 1 : 2;
                }
                if (arrayLength < 1)
                {
                    arrayLength = count * 2;
                }
            }
            // Initialize our array to our exact or oversized length.
            // It is now safe to assume we have enough space for all of the characters.
            char[] chars = new char[arrayLength];
            int w = 0;
            for (int r = offset, e = offset + count; r < e; ++r)
            {
                int cp = codePoints[r];
                if (cp < 0 || cp > 0x10ffff)
                {
                    throw new System.ArgumentException();
                }
                if (cp < 0x010000)
                {
                    chars[w++] = (char)cp;
                }
                else
                {
                    chars[w++] = (char)(LeadSurrogateOffset + (cp >> LeadSurrogateShift));
                    chars[w++] = (char)(TrailSurrogateMinValue + (cp & TrailSurrogateMask));
                }
            }
            return new string(chars, 0, w);
        }

        /// <summary>
        /// UTF16 string comparer class. Allows UTF16 string comparison to be done with the various
        /// modes.
        /// <list type="bullet">
        ///     <item><description>
        ///         Code point comparison or code unit comparison.
        ///     </description></item>
        ///     <item><description>
        ///         Case sensitive comparison, case insensitive comparison or case insensitive comparison
        ///         with special handling for character 'i'.
        ///     </description></item>
        /// </list>
        /// <para/>
        /// The code unit or code point comparison differ only when comparing supplementary code points
        /// (&#92;u10000..&#92;u10ffff) to BMP code points near the end of the BMP (i.e.,
        /// &#92;ue000..&#92;uffff). In code unit comparison, high BMP code points sort after
        /// supplementary code points because they are stored as pairs of surrogates which are at
        /// &#92;ud800..&#92;udfff.
        /// </summary>
        /// <seealso cref="FoldCaseDefault"/>
        /// <seealso cref="FoldCaseExcludeSpecialI"/>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - This design needs to be reworked to fit into the .NET world:
        // 1. Look into subclassing System.StringComparer
        // 2. Rather than "foldcaseoption", this should be made culture aware, so the "special i" casing works in the applicable cultures
        // 3. Look into making constants similar to System.StringComparer.Ordinal, System.StringComparer.OrdinalIgnoreCase, System.StringComparer.CurrentCulture, etc.
        // Note that it seems that the only feature available here is that is not available in .NET's StringComparer is the ability to use "codepointcompare" mode
        public sealed class StringComparer : IComparer<string> // ICU4N TODO: De-nest and rename UTF16StringComparer ?
        {
            // public constructor ------------------------------------------------

            /// <summary>
            /// Default constructor that does code unit comparison and case sensitive comparison.
            /// </summary>
            /// <stable>ICU 2.1</stable>
            public StringComparer()
                : this(false, false, FoldCaseDefault)
            {
            }

            /// <summary>
            /// Constructor that does comparison based on the argument options.
            /// </summary>
            /// <param name="codepointcompare">Flag to indicate true for code point comparison or false for code unit comparison.</param>
            /// <param name="ignorecase">False for case sensitive comparison, true for case-insensitive comparison.</param>
            /// <param name="foldcaseoption"><see cref="FoldCaseDefault"/> or <see cref="FoldCaseExcludeSpecialI"/>. This option is used only
            /// when ignorecase is set to true. If ignorecase is false, this option is
            /// ignored.
            /// </param>
            /// <seealso cref="FoldCaseDefault"/>
            /// <seealso cref="FoldCaseExcludeSpecialI"/>
            /// <exception cref="ArgumentException">If <paramref name="foldcaseoption"/> is out of range.</exception>
            /// <stable>ICU 2.4</stable>
            public StringComparer(bool codepointcompare, bool ignorecase, int foldcaseoption)
            {
                CodePointCompare = codepointcompare;
                m_ignoreCase_ = ignorecase;
                if (foldcaseoption < FoldCaseDefault || foldcaseoption > FoldCaseExcludeSpecialI)
                {
                    throw new ArgumentException("Invalid fold case option"); // ICU4N TODO: API - change to ArgumentOutOfRangeException
                }
                m_foldCase_ = foldcaseoption;
            }

            // public data member ------------------------------------------------

            /// <summary>
            /// Option value for case folding comparison:
            /// <para/>
            /// Comparison is case insensitive, strings are folded using default mappings defined in
            /// Unicode data file CaseFolding.txt, before comparison.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int FoldCaseDefault = (int)Globalization.FoldCase.Default; // ICU4N TODO: API Make enum and combine with UChar fold case 

            /// <summary>
            /// Option value for case folding:
            /// Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
            /// and dotless i appropriately for Turkic languages (tr, az).
            /// <para/>
            /// Comparison is case insensitive, strings are folded using modified mappings defined in
            /// Unicode data file CaseFolding.txt, before comparison.
            /// </summary>
            /// <seealso cref="UChar.FoldCaseExcludeSpecialI"/>
            /// <stable>ICU 2.4</stable>
            public const int FoldCaseExcludeSpecialI = (int)Globalization.FoldCase.ExcludeSpecialI; // ICU4N TODO: API Make enum and combine with UChar fold case 

            // public methods ----------------------------------------------------

            // public setters ----------------------------------------------------

            // ICU4N specific - SetCodePointCompare(bool) made into setter of CodePointCompare

            // ICU4N specific - Converted SetIgnoreCase(bool, int) into 2 separate property setters

            // public getters ----------------------------------------------------

            /// <summary>
            /// Gets or Sets the comparison mode to code point compare if flag is true. 
            /// Default comparison mode is set to code unit compare (false).
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public bool CodePointCompare
            {
                get => m_codePointCompare_ == Normalizer.COMPARE_CODE_POINT_ORDER;
                set
                {
                    if (value)
                    {
                        m_codePointCompare_ = Normalizer.COMPARE_CODE_POINT_ORDER;
                    }
                    else
                    {
                        m_codePointCompare_ = 0;
                    }
                }
            }

            /// <summary>
            /// Gets or sets whether <see cref="StringComparer"/> is in the case insensitive mode.
            /// </summary>
            /// <remarks>
            /// <b>true</b> if <see cref="StringComparer"/> performs case insensitive comparison, <b>false</b> otherwise
            /// </remarks>
            /// <stable>ICU 2.4</stable>
            public bool IgnoreCase 
            {
                get => m_ignoreCase_;
                set => m_ignoreCase_ = value;
            }

            /// <summary>
            /// Gets or sets the fold case options set in <see cref="StringComparer"/> to be used with case insensitive comparison.
            /// </summary>
            /// <remarks>
            /// <see cref="FoldCaseDefault"/> or <see cref="FoldCaseExcludeSpecialI"/>. This option is used only
            /// when ignorecase is set to true. If ignorecase is false, this option is
            /// ignored.
            /// </remarks>
            /// <seealso cref="FoldCaseDefault"/>
            /// <seealso cref="FoldCaseExcludeSpecialI"/>
            /// <stable>ICU 2.4</stable>
            public int IgnoreCaseOption
            {
                get => m_foldCase_;
                set
                {
                    if (value < FoldCaseDefault || value > FoldCaseExcludeSpecialI)
                    {
                        throw new ArgumentException("Invalid fold case option");
                    }
                    m_foldCase_ = value;
                }
            }

            // public other methods ----------------------------------------------

            /// <summary>
            /// Compare two strings depending on the options selected during construction.
            /// </summary>
            /// <param name="a">First source string.</param>
            /// <param name="b">Second source string.</param>
            /// <returns>0 returned if <paramref name="a"/> == <paramref name="b"/>. If <paramref name="a"/> &lt; <paramref name="b"/>, 
            /// a negative value is returned. Otherwise if <paramref name="a"/> &gt; <paramref name="b"/>, a positive value is returned.</returns>
            /// <exception cref="InvalidCastException">Thrown when either <paramref name="a"/> or <paramref name="b"/> is not a string object.</exception>
            /// <stable>ICU 4.4</stable>
            public int Compare(string a, string b)
            {
                if (Utility.SameObjects(a, b))
                {
                    return 0;
                }
                if (a == null)
                {
                    return -1;
                }
                if (b == null)
                {
                    return 1;
                }

                if (m_ignoreCase_)
                {
                    return CompareCaseInsensitive(a, b);
                }
                return CompareCaseSensitive(a, b);
            }

            // private data member ----------------------------------------------

            /// <summary>
            /// Code unit comparison flag. True if code unit comparison is required. False if code point
            /// comparison is required.
            /// </summary>
            private int m_codePointCompare_;

            /// <summary>
            /// Fold case comparison option.
            /// </summary>
            private int m_foldCase_;

            /// <summary>
            /// Flag indicator if ignore case is to be used during comparison
            /// </summary>
            private bool m_ignoreCase_;

            /// <summary>
            /// Code point order offset for surrogate characters
            /// </summary>
            private const int CODE_POINT_COMPARE_SURROGATE_OFFSET_ = 0x2800;

            // private method ---------------------------------------------------

            /// <summary>
            /// Compares case insensitive. This is a direct port of ICU4C, to make maintainence life
            /// easier.
            /// </summary>
            /// <param name="s1">First string to compare.</param>
            /// <param name="s2">Second string to compare.</param>
            /// <returns>-1 if <paramref name="s1"/> &lt; <paramref name="s2"/>, 0 if equal, 
            /// 1 if <paramref name="s1"/> &gt; <paramref name="s2"/>.</returns>
            private int CompareCaseInsensitive(string s1, string s2)
            {
                return Normalizer.CmpEquivFold(s1.AsCharSequence(), s2.AsCharSequence(), m_foldCase_ | m_codePointCompare_
                        | Normalizer.COMPARE_IGNORE_CASE);
            }

            /// <summary>
            /// Compares case sensitive. This is a direct port of ICU4C, to make maintainence life
            /// easier.
            /// </summary>
            /// <param name="s1">First string to compare.</param>
            /// <param name="s2">Second string to compare.</param>
            /// <returns>-1 if <paramref name="s1"/> &lt; <paramref name="s2"/>, 0 if equal, 
            /// 1 if <paramref name="s1"/> &gt; <paramref name="s2"/>.</returns>
            private int CompareCaseSensitive(string s1, string s2)
            {
                // compare identical prefixes - they do not need to be fixed up
                // limit1 = start1 + min(lenght1, length2)
                int length1 = s1.Length;
                int length2 = s2.Length;
                int minlength = length1;
                int result = 0;
                if (length1 < length2)
                {
                    result = -1;
                }
                else if (length1 > length2)
                {
                    result = 1;
                    minlength = length2;
                }

                char c1 = (char)0;
                char c2 = (char)0;
                int index = 0;
                for (; index < minlength; index++)
                {
                    c1 = s1[index];
                    c2 = s2[index];
                    // check pseudo-limit
                    if (c1 != c2)
                    {
                        break;
                    }
                }

                if (index == minlength)
                {
                    return result;
                }

                bool codepointcompare = m_codePointCompare_ == Normalizer.COMPARE_CODE_POINT_ORDER;
                // if both values are in or above the surrogate range, fix them up
                if (c1 >= LeadSurrogateMinValue && c2 >= LeadSurrogateMinValue
                        && codepointcompare)
                {
                    // subtract 0x2800 from BMP code points to make them smaller
                    // than supplementary ones
                    if ((c1 <= LeadSurrogateMaxValue && (index + 1) != length1 && IsTrailSurrogate(s1[index + 1]))
                            || (IsTrailSurrogate(c1) && index != 0 && IsLeadSurrogate(s1[index - 1])))
                    {
                        // part of a surrogate pair, leave >=d800
                    }
                    else
                    {
                        // BMP code point - may be surrogate code point - make
                        // < d800
                        //c1 -= CODE_POINT_COMPARE_SURROGATE_OFFSET_;
                        c1 = (char)(c1 - CODE_POINT_COMPARE_SURROGATE_OFFSET_);
                    }

                    if ((c2 <= LeadSurrogateMaxValue && (index + 1) != length2 && IsTrailSurrogate(s2[index + 1]))
                            || (IsTrailSurrogate(c2) && index != 0 && IsLeadSurrogate(s2[index - 1])))
                    {
                        // part of a surrogate pair, leave >=d800
                    }
                    else
                    {
                        // BMP code point - may be surrogate code point - make <d800
                        //c2 -= CODE_POINT_COMPARE_SURROGATE_OFFSET_;
                        c2 = (char)(c2 - CODE_POINT_COMPARE_SURROGATE_OFFSET_);
                    }
                }

                // now c1 and c2 are in UTF-32-compatible order
                return c1 - c2;
            }
        }

        // ICU4N specific - GetSingleCodePoint(ICharSequence s) moved to UTF16.generated.tt

        // ICU4N specific - CompareCodePoint(int codePoint, ICharSequence s) moved to UTF16.generated.tt


        // private data members -------------------------------------------------

        /// <summary>
        /// Shift value for lead surrogate to form a supplementary character.
        /// </summary>
        private const int LeadSurrogateShift = 10;

        /// <summary>
        /// Mask to retrieve the significant value from a trail surrogate.
        /// </summary>
        private const int TrailSurrogateMask = 0x3FF;

        /// <summary>
        /// Value that all lead surrogate starts with
        /// </summary>
        private const int LeadSurrogateOffset = LeadSurrogateMinValue
                - (SupplementaryMinValue >> LeadSurrogateShift);

        // private methods ------------------------------------------------------

        /// <summary>
        /// Converts argument code point and returns a string object representing the code point's value
        /// in UTF16 format.
        /// <para/>
        /// This method does not check for the validity of the codepoint, the results are not guaranteed
        /// if a invalid codepoint is passed as argument.
        /// <para/>
        /// The result is a string whose length is 1 for non-supplementary code points, 2 otherwise.
        /// </summary>
        /// <param name="ch">Code point.</param>
        /// <returns>String representation of the code point.</returns>
        private static string ToString(int ch)
        {
            if (ch < SupplementaryMinValue)
            {
                return "" + (char)ch;
            }

            // ICU4N: Both of the below alternatives were tried, but for some
            // reason this caused perfomance to degrade considerably.
            //return new string(GetLeadSurrogate(ch), GetTrailSurrogate(ch));
            //return char.ConvertFromUtf32(ch);

            StringBuilder result = new StringBuilder();
            result.Append(GetLeadSurrogate(ch));
            result.Append(GetTrailSurrogate(ch));
            return result.ToString();
        }
    }
}
