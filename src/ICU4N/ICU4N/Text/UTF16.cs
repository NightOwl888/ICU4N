using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public sealed class UTF16
    {
        // public variables ---------------------------------------------------

        /**
         * Value returned in {@link #bounds(String, int) bounds()}.
         * These values are chosen specifically so that it actually represents the position of the
         * character [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)]
         *
         * @stable ICU 2.1
         */
        public const int SINGLE_CHAR_BOUNDARY = 1, LEAD_SURROGATE_BOUNDARY = 2,
                TRAIL_SURROGATE_BOUNDARY = 5;

        /**
         * The lowest Unicode code point value.
         *
         * @stable ICU 2.1
         */
        public static readonly int CODEPOINT_MIN_VALUE = 0;

        /**
         * The highest Unicode code point value (scalar value) according to the Unicode Standard.
         *
         * @stable ICU 2.1
         */
        public static readonly int CODEPOINT_MAX_VALUE = 0x10ffff;

        /**
         * The minimum value for Supplementary code points
         *
         * @stable ICU 2.1
         */
        public static readonly int SUPPLEMENTARY_MIN_VALUE = 0x10000;

        /**
         * Lead surrogate minimum value
         *
         * @stable ICU 2.1
         */
        public static readonly int LEAD_SURROGATE_MIN_VALUE = 0xD800;

        /**
         * Trail surrogate minimum value
         *
         * @stable ICU 2.1
         */
        public static readonly int TRAIL_SURROGATE_MIN_VALUE = 0xDC00;

        /**
         * Lead surrogate maximum value
         *
         * @stable ICU 2.1
         */
        public static readonly int LEAD_SURROGATE_MAX_VALUE = 0xDBFF;

        /**
         * Trail surrogate maximum value
         *
         * @stable ICU 2.1
         */
        public static readonly int TRAIL_SURROGATE_MAX_VALUE = 0xDFFF;

        /**
         * Surrogate minimum value
         *
         * @stable ICU 2.1
         */
        public static readonly int SURROGATE_MIN_VALUE = LEAD_SURROGATE_MIN_VALUE;

        /**
         * Maximum surrogate value
         *
         * @stable ICU 2.1
         */
        public static readonly int SURROGATE_MAX_VALUE = TRAIL_SURROGATE_MAX_VALUE;

        /**
         * Lead surrogate bitmask
         */
        private static readonly int LEAD_SURROGATE_BITMASK = unchecked((int)0xFFFFFC00);

        /**
         * Trail surrogate bitmask
         */
        private static readonly int TRAIL_SURROGATE_BITMASK = unchecked((int)0xFFFFFC00);

        /**
         * Surrogate bitmask
         */
        private static readonly int SURROGATE_BITMASK = unchecked((int)0xFFFFF800);

        /**
         * Lead surrogate bits
         */
        private static readonly int LEAD_SURROGATE_BITS = 0xD800;

        /**
         * Trail surrogate bits
         */
        private static readonly int TRAIL_SURROGATE_BITS = 0xDC00;

        /**
         * Surrogate bits
         */
        private static readonly int SURROGATE_BITS = 0xD800;

        // constructor --------------------------------------------------------

        // /CLOVER:OFF
        /**
         * Prevent instance from being created.
         */
        private UTF16()
        {
        }

        // /CLOVER:ON
        // public method ------------------------------------------------------

        /**
         * Extract a single UTF-32 value from a string. Used when iterating forwards or backwards (with
         * <code>UTF16.getCharCount()</code>, as well as random access. If a validity check is
         * required, use <code><a href="../lang/UCharacter.html#isLegal(char)">
         * UCharacter.isLegal()</a></code>
         * on the return value. If the char retrieved is part of a surrogate pair, its supplementary
         * character will be returned. If a complete supplementary character is not found the incomplete
         * character will be returned
         *
         * @param source Array of UTF-16 chars
         * @param offset16 UTF-16 offset to the start of the character.
         * @return UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
         *         of that codepoint are the same as in <code>bounds32()</code>.
         * @exception IndexOutOfBoundsException Thrown if offset16 is out of bounds.
         * @stable ICU 2.1
         */
        public static int CharAt(string source, int offset16)
        {
            char single = source[offset16];
            if (single < LEAD_SURROGATE_MIN_VALUE)
            {
                return single;
            }
            return _charAt(source, offset16, single);
        }

        private static int _charAt(string source, int offset16, char single)
        {
            if (single > TRAIL_SURROGATE_MAX_VALUE)
            {
                return single;
            }

            // Convert the UTF-16 surrogate pair if necessary.
            // For simplicity in usage, and because the frequency of pairs is
            // low, look both directions.

            if (single <= LEAD_SURROGATE_MAX_VALUE)
            {
                ++offset16;
                if (source.Length != offset16)
                {
                    char trail = source[offset16];
                    if (trail >= TRAIL_SURROGATE_MIN_VALUE && trail <= TRAIL_SURROGATE_MAX_VALUE)
                    {
                        return Character.ToCodePoint(single, trail);
                    }
                }
            }
            else
            {
                --offset16;
                if (offset16 >= 0)
                {
                    // single is a trail surrogate so
                    char lead = source[offset16];
                    if (lead >= LEAD_SURROGATE_MIN_VALUE && lead <= LEAD_SURROGATE_MAX_VALUE)
                    {
                        return Character.ToCodePoint(lead, single);
                    }
                }
            }
            return single; // return unmatched surrogate
        }

        /// <summary>
        /// Extract a single UTF-32 value from a string. Used when iterating forwards or backwards (with
        /// <c>UTF16.GetCharCount()</c>, as well as random access. If a validity check is
        /// required, use <c><a href="../lang/UCharacter.html#isLegal(char)">
        /// UCharacter.IsLegal()</a></c>
        /// on the return value. If the char retrieved is part of a surrogate pair, its supplementary
        /// character will be returned. If a complete supplementary character is not found the incomplete
        /// character will be returned.
        /// </summary>
        /// <param name="source">Array of UTF-16 chars</param>
        /// <param name="offset16">UTF-16 offset to the start of the character.</param>
        /// <returns>
        /// UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
        /// of that codepoint are the same as in <c>Bounds32()</c>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if offset16 is out of bounds.</exception>
        /// <stable>ICU 2.1</stable>
        internal static int CharAt(ICharSequence source, int offset16)
        {
            char single = source[offset16];
            if (single < UTF16.LEAD_SURROGATE_MIN_VALUE)
            {
                return single;
            }
            return _charAt(source, offset16, single);
        }

        private static int _charAt(ICharSequence source, int offset16, char single)
        {
            if (single > UTF16.TRAIL_SURROGATE_MAX_VALUE)
            {
                return single;
            }

            // Convert the UTF-16 surrogate pair if necessary.
            // For simplicity in usage, and because the frequency of pairs is
            // low, look both directions.

            if (single <= UTF16.LEAD_SURROGATE_MAX_VALUE)
            {
                ++offset16;
                if (source.Length != offset16)
                {
                    char trail = source[offset16];
                    if (trail >= UTF16.TRAIL_SURROGATE_MIN_VALUE
                            && trail <= UTF16.TRAIL_SURROGATE_MAX_VALUE)
                    {
                        return Character.ToCodePoint(single, trail);
                    }
                }
            }
            else
            {
                --offset16;
                if (offset16 >= 0)
                {
                    // single is a trail surrogate so
                    char lead = source[offset16];
                    if (lead >= UTF16.LEAD_SURROGATE_MIN_VALUE
                            && lead <= UTF16.LEAD_SURROGATE_MAX_VALUE)
                    {
                        return Character.ToCodePoint(lead, single);
                    }
                }
            }
            return single; // return unmatched surrogate
        }

        /**
         * Extract a single UTF-32 value from a string. Used when iterating forwards or backwards (with
         * <code>UTF16.getCharCount()</code>, as well as random access. If a validity check is
         * required, use <code><a href="../lang/UCharacter.html#isLegal(char)">UCharacter.isLegal()
         * </a></code>
         * on the return value. If the char retrieved is part of a surrogate pair, its supplementary
         * character will be returned. If a complete supplementary character is not found the incomplete
         * character will be returned
         *
         * @param source UTF-16 chars string buffer
         * @param offset16 UTF-16 offset to the start of the character.
         * @return UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
         *         of that codepoint are the same as in <code>bounds32()</code>.
         * @exception IndexOutOfBoundsException Thrown if offset16 is out of bounds.
         * @stable ICU 2.1
         */
        public static int CharAt(StringBuilder source, int offset16)
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

            if (single <= LEAD_SURROGATE_MAX_VALUE)
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

        /**
         * Extract a single UTF-32 value from a substring. Used when iterating forwards or backwards
         * (with <code>UTF16.getCharCount()</code>, as well as random access. If a validity check is
         * required, use <code><a href="../lang/UCharacter.html#isLegal(char)">UCharacter.isLegal()
         * </a></code>
         * on the return value. If the char retrieved is part of a surrogate pair, its supplementary
         * character will be returned. If a complete supplementary character is not found the incomplete
         * character will be returned
         *
         * @param source Array of UTF-16 chars
         * @param start Offset to substring in the source array for analyzing
         * @param limit Offset to substring in the source array for analyzing
         * @param offset16 UTF-16 offset relative to start
         * @return UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
         *         of that codepoint are the same as in <code>bounds32()</code>.
         * @exception IndexOutOfBoundsException Thrown if offset16 is not within the range of start and limit.
         * @stable ICU 2.1
         */
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
            if (single <= LEAD_SURROGATE_MAX_VALUE)
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

        /**
         * Extract a single UTF-32 value from a string. Used when iterating forwards or backwards (with
         * <code>UTF16.getCharCount()</code>, as well as random access. If a validity check is
         * required, use <code><a href="../lang/UCharacter.html#isLegal(char)">UCharacter.isLegal()
         * </a></code>
         * on the return value. If the char retrieved is part of a surrogate pair, its supplementary
         * character will be returned. If a complete supplementary character is not found the incomplete
         * character will be returned
         *
         * @param source UTF-16 chars string buffer
         * @param offset16 UTF-16 offset to the start of the character.
         * @return UTF-32 value for the UTF-32 value that contains the char at offset16. The boundaries
         *         of that codepoint are the same as in <code>bounds32()</code>.
         * @exception IndexOutOfBoundsException Thrown if offset16 is out of bounds.
         * @stable ICU 2.1
         */
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

            if (single <= LEAD_SURROGATE_MAX_VALUE)
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

        /**
         * Determines how many chars this char32 requires. If a validity check is required, use <code>
         * <a href="../lang/UCharacter.html#isLegal(char)">isLegal()</a></code>
         * on char32 before calling.
         *
         * @param char32 The input codepoint.
         * @return 2 if is in supplementary space, otherwise 1.
         * @stable ICU 2.1
         */
        public static int GetCharCount(int char32)
        {
            if (char32 < SUPPLEMENTARY_MIN_VALUE)
            {
                return 1;
            }
            return 2;
        }

        /**
         * Returns the type of the boundaries around the char at offset16. Used for random access.
         *
         * @param source Text to analyse
         * @param offset16 UTF-16 offset
         * @return
         *            <ul>
         *            <li> SINGLE_CHAR_BOUNDARY : a single char; the bounds are [offset16, offset16+1]
         *            <li> LEAD_SURROGATE_BOUNDARY : a surrogate pair starting at offset16; the bounds
         *            are [offset16, offset16 + 2]
         *            <li> TRAIL_SURROGATE_BOUNDARY : a surrogate pair starting at offset16 - 1; the
         *            bounds are [offset16 - 1, offset16 + 1]
         *            </ul>
         *            For bit-twiddlers, the return values for these are chosen so that the boundaries
         *            can be gotten by: [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)].
         * @exception IndexOutOfBoundsException If offset16 is out of bounds.
         * @stable ICU 2.1
         */
        public static int Bounds(string source, int offset16)
        {
            char ch = source[offset16];
            if (IsSurrogate(ch))
            {
                if (IsLeadSurrogate(ch))
                {
                    if (++offset16 < source.Length && IsTrailSurrogate(source[offset16]))
                    {
                        return LEAD_SURROGATE_BOUNDARY;
                    }
                }
                else
                {
                    // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= 0 && IsLeadSurrogate(source[offset16]))
                    {
                        return TRAIL_SURROGATE_BOUNDARY;
                    }
                }
            }
            return SINGLE_CHAR_BOUNDARY;
        }

        /**
         * Returns the type of the boundaries around the char at offset16. Used for random access.
         *
         * @param source String buffer to analyse
         * @param offset16 UTF16 offset
         * @return
         *            <ul>
         *            <li> SINGLE_CHAR_BOUNDARY : a single char; the bounds are [offset16, offset16 + 1]
         *            <li> LEAD_SURROGATE_BOUNDARY : a surrogate pair starting at offset16; the bounds
         *            are [offset16, offset16 + 2]
         *            <li> TRAIL_SURROGATE_BOUNDARY : a surrogate pair starting at offset16 - 1; the
         *            bounds are [offset16 - 1, offset16 + 1]
         *            </ul>
         *            For bit-twiddlers, the return values for these are chosen so that the boundaries
         *            can be gotten by: [offset16 - (value &gt;&gt; 2), offset16 + (value &amp; 3)].
         * @exception IndexOutOfBoundsException If offset16 is out of bounds.
         * @stable ICU 2.1
         */
        public static int Bounds(StringBuilder source, int offset16)
        {
            char ch = source[offset16];
            if (IsSurrogate(ch))
            {
                if (IsLeadSurrogate(ch))
                {
                    if (++offset16 < source.Length && IsTrailSurrogate(source[offset16]))
                    {
                        return LEAD_SURROGATE_BOUNDARY;
                    }
                }
                else
                {
                    // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= 0 && IsLeadSurrogate(source[offset16]))
                    {
                        return TRAIL_SURROGATE_BOUNDARY;
                    }
                }
            }
            return SINGLE_CHAR_BOUNDARY;
        }

        /**
         * Returns the type of the boundaries around the char at offset16. Used for random access. Note
         * that the boundaries are determined with respect to the subarray, hence the char array
         * {0xD800, 0xDC00} has the result SINGLE_CHAR_BOUNDARY for start = offset16 = 0 and limit = 1.
         *
         * @param source Char array to analyse
         * @param start Offset to substring in the source array for analyzing
         * @param limit Offset to substring in the source array for analyzing
         * @param offset16 UTF16 offset relative to start
         * @return
         *            <ul>
         *            <li> SINGLE_CHAR_BOUNDARY : a single char; the bounds are
         *            <li> LEAD_SURROGATE_BOUNDARY : a surrogate pair starting at offset16; the bounds
         *            are [offset16, offset16 + 2]
         *            <li> TRAIL_SURROGATE_BOUNDARY : a surrogate pair starting at offset16 - 1; the
         *            bounds are [offset16 - 1, offset16 + 1]
         *            </ul>
         *            For bit-twiddlers, the boundary values for these are chosen so that the boundaries
         *            can be gotten by: [offset16 - (boundvalue &gt;&gt; 2), offset16 + (boundvalue &amp; 3)].
         * @exception IndexOutOfBoundsException If offset16 is not within the range of start and limit.
         * @stable ICU 2.1
         */
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
                        return LEAD_SURROGATE_BOUNDARY;
                    }
                }
                else
                { // IsTrailSurrogate(ch), so
                    --offset16;
                    if (offset16 >= start && IsLeadSurrogate(source[offset16]))
                    {
                        return TRAIL_SURROGATE_BOUNDARY;
                    }
                }
            }
            return SINGLE_CHAR_BOUNDARY;
        }

        /**
         * Determines whether the code value is a surrogate.
         *
         * @param char16 The input character.
         * @return true If the input character is a surrogate.
         * @stable ICU 2.1
         */
        public static bool IsSurrogate(char char16)
        {
            return (char16 & SURROGATE_BITMASK) == SURROGATE_BITS;
        }

        /**
         * Determines whether the character is a trail surrogate.
         *
         * @param char16 The input character.
         * @return true If the input character is a trail surrogate.
         * @stable ICU 2.1
         */
        public static bool IsTrailSurrogate(char char16)
        {
            return (char16 & TRAIL_SURROGATE_BITMASK) == TRAIL_SURROGATE_BITS;
        }

        /**
         * Determines whether the character is a lead surrogate.
         *
         * @param char16 The input character.
         * @return true If the input character is a lead surrogate
         * @stable ICU 2.1
         */
        public static bool IsLeadSurrogate(char char16)
        {
            return (char16 & LEAD_SURROGATE_BITMASK) == LEAD_SURROGATE_BITS;
        }

        /**
         * Returns the lead surrogate. If a validity check is required, use
         * <code><a href="../lang/UCharacter.html#isLegal(char)">isLegal()</a></code> on char32
         * before calling.
         *
         * @param char32 The input character.
         * @return lead surrogate if the getCharCount(ch) is 2; <br>
         *         and 0 otherwise (note: 0 is not a valid lead surrogate).
         * @stable ICU 2.1
         */
        public static char GetLeadSurrogate(int char32)
        {
            if (char32 >= SUPPLEMENTARY_MIN_VALUE)
            {
                return (char)(LEAD_SURROGATE_OFFSET_ + (char32 >> LEAD_SURROGATE_SHIFT_));
            }
            return (char)0;
        }

        /**
         * Returns the trail surrogate. If a validity check is required, use
         * <code><a href="../lang/UCharacter.html#isLegal(char)">isLegal()</a></code> on char32
         * before calling.
         *
         * @param char32 The input character.
         * @return the trail surrogate if the getCharCount(ch) is 2; <br>
         *         otherwise the character itself
         * @stable ICU 2.1
         */
        public static char GetTrailSurrogate(int char32)
        {
            if (char32 >= SUPPLEMENTARY_MIN_VALUE)
            {
                return (char)(TRAIL_SURROGATE_MIN_VALUE + (char32 & TRAIL_SURROGATE_MASK_));
            }
            return (char)char32;
        }

        /**
         * Convenience method corresponding to String.valueOf(char). Returns a one or two char string
         * containing the UTF-32 value in UTF16 format. If a validity check is required, use
         * {@link com.ibm.icu.lang.UCharacter#isLegal(int)} on char32 before calling.
         *
         * @param char32 The input character.
         * @return string value of char32 in UTF16 format
         * @exception IllegalArgumentException Thrown if char32 is a invalid codepoint.
         * @stable ICU 2.1
         */
        public static string ValueOf(int char32)
        {
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Illegal codepoint");
            }
            return ToString(char32);
        }

        /**
         * Convenience method corresponding to String.valueOf(codepoint at offset16). Returns a one or
         * two char string containing the UTF-32 value in UTF16 format. If offset16 indexes a surrogate
         * character, the whole supplementary codepoint will be returned. If a validity check is
         * required, use {@link com.ibm.icu.lang.UCharacter#isLegal(int)} on the
         * codepoint at offset16 before calling. The result returned will be a newly created String
         * obtained by calling source.substring(..) with the appropriate indexes.
         *
         * @param source The input string.
         * @param offset16 The UTF16 index to the codepoint in source
         * @return string value of char32 in UTF16 format
         * @stable ICU 2.1
         */
        public static string ValueOf(string source, int offset16)
        {
            switch (Bounds(source, offset16))
            {
                case LEAD_SURROGATE_BOUNDARY:
                    return source.Substring(offset16, 2); // offset16 + 2 - offset16 = 2
                case TRAIL_SURROGATE_BOUNDARY:
                    return source.Substring(offset16 - 1, 2); // (offset16 + 1) - (offset16 - 1) = 2
                default:
                    return source.Substring(offset16, 1); // offset16 + 1 - offset16 = 1
            }
        }

        /**
         * Convenience method corresponding to StringBuffer.valueOf(codepoint at offset16). Returns a
         * one or two char string containing the UTF-32 value in UTF16 format. If offset16 indexes a
         * surrogate character, the whole supplementary codepoint will be returned. If a validity check
         * is required, use {@link com.ibm.icu.lang.UCharacter#isLegal(int)} on
         * the codepoint at offset16 before calling. The result returned will be a newly created String
         * obtained by calling source.substring(..) with the appropriate indexes.
         *
         * @param source The input string buffer.
         * @param offset16 The UTF16 index to the codepoint in source
         * @return string value of char32 in UTF16 format
         * @stable ICU 2.1
         */
        public static string ValueOf(StringBuilder source, int offset16)
        {
            switch (Bounds(source, offset16))
            {
                case LEAD_SURROGATE_BOUNDARY:
                    return source.ToString(offset16, 2); // offset16 + 2 - offset16 = 2
                case TRAIL_SURROGATE_BOUNDARY:
                    return source.ToString(offset16 - 1, 2); // (offset16 + 1) - (offset16 - 1) = 2
                default:
                    return source.ToString(offset16, 1); // offset16 + 1 - offset16 = 1
            }
        }

        /**
         * Convenience method. Returns a one or two char string containing the UTF-32 value in UTF16
         * format. If offset16 indexes a surrogate character, the whole supplementary codepoint will be
         * returned, except when either the leading or trailing surrogate character lies out of the
         * specified subarray. In the latter case, only the surrogate character within bounds will be
         * returned. If a validity check is required, use
         * {@link com.ibm.icu.lang.UCharacter#isLegal(int)} on the codepoint at
         * offset16 before calling. The result returned will be a newly created String containing the
         * relevant characters.
         *
         * @param source The input char array.
         * @param start Start index of the subarray
         * @param limit End index of the subarray
         * @param offset16 The UTF16 index to the codepoint in source relative to start
         * @return string value of char32 in UTF16 format
         * @stable ICU 2.1
         */
        public static string ValueOf(char[] source, int start, int limit, int offset16)
        {
            switch (Bounds(source, start, limit, offset16))
            {
                case LEAD_SURROGATE_BOUNDARY:
                    return new string(source, start + offset16, 2);
                case TRAIL_SURROGATE_BOUNDARY:
                    return new string(source, start + offset16 - 1, 2);
                default:
                    return new string(source, start + offset16, 1);
            }
        }

        /**
         * Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
         * the {@link UTF16 class description} for notes on roundtripping.
         *
         * @param source The UTF-16 string
         * @param offset32 UTF-32 offset
         * @return UTF-16 offset
         * @exception IndexOutOfBoundsException If offset32 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
         * the {@link UTF16 class description} for notes on roundtripping.
         *
         * @param source The UTF-16 string buffer
         * @param offset32 UTF-32 offset
         * @return UTF-16 offset
         * @exception IndexOutOfBoundsException If offset32 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Returns the UTF-16 offset that corresponds to a UTF-32 offset. Used for random access. See
         * the {@link UTF16 class description} for notes on roundtripping.
         *
         * @param source The UTF-16 char array whose substring is to be analysed
         * @param start Offset of the substring to be analysed
         * @param limit Offset of the substring to be analysed
         * @param offset32 UTF-32 offset relative to start
         * @return UTF-16 offset relative to start
         * @exception IndexOutOfBoundsException If offset32 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Returns the UTF-32 offset corresponding to the first UTF-32 boundary at or after the given
         * UTF-16 offset. Used for random access. See the {@link UTF16 class description} for
         * notes on roundtripping.<br>
         * <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
         * of the <strong>lead</strong> of the pair is returned. </i>
         * <p>
         * To find the UTF-32 length of a string, use:
         *
         * <pre>
         * len32 = countCodePoint(source, source.length());
         * </pre>
         *
         * @param source Text to analyse
         * @param offset16 UTF-16 offset &lt; source text length.
         * @return UTF-32 offset
         * @exception IndexOutOfBoundsException If offset16 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Returns the UTF-32 offset corresponding to the first UTF-32 boundary at the given UTF-16
         * offset. Used for random access. See the {@link UTF16 class description} for notes on
         * roundtripping.<br>
         * <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
         * of the <strong>lead</strong> of the pair is returned. </i>
         * <p>
         * To find the UTF-32 length of a string, use:
         *
         * <pre>
         * len32 = countCodePoint(source);
         * </pre>
         *
         * @param source Text to analyse
         * @param offset16 UTF-16 offset &lt; source text length.
         * @return UTF-32 offset
         * @exception IndexOutOfBoundsException If offset16 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Returns the UTF-32 offset corresponding to the first UTF-32 boundary at the given UTF-16
         * offset. Used for random access. See the {@link UTF16 class description} for notes on
         * roundtripping.<br>
         * <i>Note: If the UTF-16 offset is into the middle of a surrogate pair, then the UTF-32 offset
         * of the <strong>lead</strong> of the pair is returned. </i>
         * <p>
         * To find the UTF-32 length of a substring, use:
         *
         * <pre>
         * len32 = countCodePoint(source, start, limit);
         * </pre>
         *
         * @param source Text to analyse
         * @param start Offset of the substring
         * @param limit Offset of the substring
         * @param offset16 UTF-16 relative to start
         * @return UTF-32 offset relative to start
         * @exception IndexOutOfBoundsException If offset16 is not within the range of start and limit.
         * @stable ICU 2.1
         */
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

        /**
         * Append a single UTF-32 value to the end of a StringBuffer. If a validity check is required,
         * use {@link com.ibm.icu.lang.UCharacter#isLegal(int)} on char32 before
         * calling.
         *
         * @param target The buffer to append to
         * @param char32 Value to append.
         * @return the updated StringBuffer
         * @exception IllegalArgumentException Thrown when char32 does not lie within the range of the Unicode codepoints
         * @stable ICU 2.1
         */
        public static StringBuilder Append(StringBuilder target, int char32)
        {
            // Check for irregular values
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Illegal codepoint: " + string.Format("{0:X4}", char32));
            }

            // Write the UTF-16 values
            if (char32 >= SUPPLEMENTARY_MIN_VALUE)
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

        /**
         * Cover JDK 1.5 APIs. Append the code point to the buffer and return the buffer as a
         * convenience.
         *
         * @param target The buffer to append to
         * @param cp The code point to append
         * @return the updated StringBuffer
         * @throws IllegalArgumentException If cp is not a valid code point
         * @stable ICU 3.0
         */
        public static StringBuilder AppendCodePoint(StringBuilder target, int cp)
        {
            return Append(target, cp);
        }

        /**
         * Adds a codepoint to offset16 position of the argument char array.
         *
         * @param target Char array to be append with the new code point
         * @param limit UTF16 offset which the codepoint will be appended.
         * @param char32 Code point to be appended
         * @return offset after char32 in the array.
         * @exception IllegalArgumentException Thrown if there is not enough space for the append, or when char32 does not
         *                lie within the range of the Unicode codepoints.
         * @stable ICU 2.1
         */
        public static int Append(char[] target, int limit, int char32)
        {
            // Check for irregular values
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Illegal codepoint");
            }
            // Write the UTF-16 values
            if (char32 >= SUPPLEMENTARY_MIN_VALUE)
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

        /**
         * Number of codepoints in a UTF16 String
         *
         * @param source UTF16 string
         * @return number of codepoint in string
         * @stable ICU 2.1
         */
        public static int CountCodePoint(string source)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, source.Length);
        }

        /**
         * Number of codepoints in a UTF16 String buffer
         *
         * @param source UTF16 string buffer
         * @return number of codepoint in string
         * @stable ICU 2.1
         */
        public static int CountCodePoint(StringBuilder source)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, source.Length);
        }

        /**
         * Number of codepoints in a UTF16 char array substring
         *
         * @param source UTF16 char array
         * @param start Offset of the substring
         * @param limit Offset of the substring
         * @return number of codepoint in the substring
         * @exception IndexOutOfBoundsException If start and limit are not valid.
         * @stable ICU 2.1
         */
        public static int CountCodePoint(char[] source, int start, int limit)
        {
            if (source == null || source.Length == 0)
            {
                return 0;
            }
            return FindCodePointOffset(source, start, limit, limit - start);
        }

        /**
         * Set a code point into a UTF16 position. Adjusts target according if we are replacing a
         * non-supplementary codepoint with a supplementary and vice versa.
         *
         * @param target Stringbuffer
         * @param offset16 UTF16 position to insert into
         * @param char32 Code point
         * @stable ICU 2.1
         */
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
            target.Replace(offset16, offset16 + count, ValueOf(char32));
        }

        /**
         * Set a code point into a UTF16 position in a char array. Adjusts target according if we are
         * replacing a non-supplementary codepoint with a supplementary and vice versa.
         *
         * @param target char array
         * @param limit numbers of valid chars in target, different from target.length. limit counts the
         *            number of chars in target that represents a string, not the size of array target.
         * @param offset16 UTF16 position to insert into
         * @param char32 code point
         * @return new number of chars in target that represents a string
         * @exception IndexOutOfBoundsException if offset16 is out of range
         * @stable ICU 2.1
         */
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

        /**
         * Shifts offset16 by the argument number of codepoints
         *
         * @param source string
         * @param offset16 UTF16 position to shift
         * @param shift32 number of codepoints to shift
         * @return new shifted offset16
         * @exception IndexOutOfBoundsException if the new offset16 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Shifts offset16 by the argument number of codepoints
         *
         * @param source String buffer
         * @param offset16 UTF16 position to shift
         * @param shift32 Number of codepoints to shift
         * @return new shifted offset16
         * @exception IndexOutOfBoundsException If the new offset16 is out of bounds.
         * @stable ICU 2.1
         */
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

        /**
         * Shifts offset16 by the argument number of codepoints within a subarray.
         *
         * @param source Char array
         * @param start Position of the subarray to be performed on
         * @param limit Position of the subarray to be performed on
         * @param offset16 UTF16 position to shift relative to start
         * @param shift32 Number of codepoints to shift
         * @return new shifted offset16 relative to start
         * @exception IndexOutOfBoundsException If the new offset16 is out of bounds with respect to the subarray or the
         *                subarray bounds are out of range.
         * @stable ICU 2.1
         */
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

        /**
         * Inserts char32 codepoint into target at the argument offset16. If the offset16 is in the
         * middle of a supplementary codepoint, char32 will be inserted after the supplementary
         * codepoint. The length of target increases by one if codepoint is non-supplementary, 2
         * otherwise.
         * <p>
         * The overall effect is exactly as if the argument were converted to a string by the method
         * valueOf(char) and the characters in that string were then inserted into target at the
         * position indicated by offset16.
         * </p>
         * <p>
         * The offset argument must be greater than or equal to 0, and less than or equal to the length
         * of source.
         *
         * @param target String buffer to insert to
         * @param offset16 Offset which char32 will be inserted in
         * @param char32 Codepoint to be inserted
         * @return a reference to target
         * @exception IndexOutOfBoundsException Thrown if offset16 is invalid.
         * @stable ICU 2.1
         */
        public static StringBuilder Insert(StringBuilder target, int offset16, int char32)
        {
            string str = ValueOf(char32);
            if (offset16 != target.Length && Bounds(target, offset16) == TRAIL_SURROGATE_BOUNDARY)
            {
                offset16++;
            }
            target.Insert(offset16, str);
            return target;
        }

        /**
         * Inserts char32 codepoint into target at the argument offset16. If the offset16 is in the
         * middle of a supplementary codepoint, char32 will be inserted after the supplementary
         * codepoint. Limit increases by one if codepoint is non-supplementary, 2 otherwise.
         * <p>
         * The overall effect is exactly as if the argument were converted to a string by the method
         * valueOf(char) and the characters in that string were then inserted into target at the
         * position indicated by offset16.
         * </p>
         * <p>
         * The offset argument must be greater than or equal to 0, and less than or equal to the limit.
         *
         * @param target Char array to insert to
         * @param limit End index of the char array, limit &lt;= target.length
         * @param offset16 Offset which char32 will be inserted in
         * @param char32 Codepoint to be inserted
         * @return new limit size
         * @exception IndexOutOfBoundsException Thrown if offset16 is invalid.
         * @stable ICU 2.1
         */
        public static int Insert(char[] target, int limit, int offset16, int char32)
        {
            string str = ValueOf(char32);
            if (offset16 != limit && Bounds(target, 0, limit, offset16) == TRAIL_SURROGATE_BOUNDARY)
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

        /**
         * Removes the codepoint at the specified position in this target (shortening target by 1
         * character if the codepoint is a non-supplementary, 2 otherwise).
         *
         * @param target String buffer to remove codepoint from
         * @param offset16 Offset which the codepoint will be removed
         * @return a reference to target
         * @exception IndexOutOfBoundsException Thrown if offset16 is invalid.
         * @stable ICU 2.1
         */
        public static StringBuilder Delete(StringBuilder target, int offset16)
        {
            int count = 1;
            switch (Bounds(target, offset16))
            {
                case LEAD_SURROGATE_BOUNDARY:
                    count++;
                    break;
                case TRAIL_SURROGATE_BOUNDARY:
                    count++;
                    offset16--;
                    break;
            }
            target.Delete(offset16, offset16 + count);
            return target;
        }

        /**
         * Removes the codepoint at the specified position in this target (shortening target by 1
         * character if the codepoint is a non-supplementary, 2 otherwise).
         *
         * @param target String buffer to remove codepoint from
         * @param limit End index of the char array, limit &lt;= target.length
         * @param offset16 Offset which the codepoint will be removed
         * @return a new limit size
         * @exception IndexOutOfBoundsException Thrown if offset16 is invalid.
         * @stable ICU 2.1
         */
        public static int Delete(char[] target, int limit, int offset16)
        {
            int count = 1;
            switch (Bounds(target, 0, limit, offset16))
            {
                case LEAD_SURROGATE_BOUNDARY:
                    count++;
                    break;
                case TRAIL_SURROGATE_BOUNDARY:
                    count++;
                    offset16--;
                    break;
            }
            System.Array.Copy(target, offset16 + count, target, offset16, limit - (offset16 + count));
            target[limit - count] = (char)0;
            return limit - count;
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the first occurrence of
         * the argument codepoint. I.e., the smallest index <code>i</code> such that
         * <code>UTF16.charAt(source, i) ==
         * char32</code> is true.
         * <p>
         * If no such character occurs in this string, then -1 is returned.
         * </p>
         * <p>
         * Examples:<br>
         * UTF16.indexOf("abc", 'a') returns 0<br>
         * UTF16.indexOf("abc\ud800\udc00", 0x10000) returns 3<br>
         * UTF16.indexOf("abc\ud800\udc00", 0xd800) returns -1<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param char32 Codepoint to search for
         * @return the index of the first occurrence of the codepoint in the argument Unicode string, or
         *         -1 if the codepoint does not occur.
         * @stable ICU 2.6
         */
        public static int IndexOf(String source, int char32)
        {
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LEAD_SURROGATE_MIN_VALUE
                    || (char32 > TRAIL_SURROGATE_MAX_VALUE && char32 < SUPPLEMENTARY_MIN_VALUE))
            {
                return source.IndexOf((char)char32);
            }
            // surrogate
            if (char32 < SUPPLEMENTARY_MIN_VALUE)
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
            return source.IndexOf(char32str);
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the first occurrence of
         * the argument string str. This method is implemented based on codepoints, hence a "lead
         * surrogate character + trail surrogate character" is treated as one entity.e Hence if the str
         * starts with trail surrogate character at index 0, a source with a leading a surrogate
         * character before str found at in source will not have a valid match. Vice versa for lead
         * surrogates that ends str. See example below.
         * <p>
         * If no such string str occurs in this source, then -1 is returned.
         * </p>
         * <p>
         * Examples:<br>
         * UTF16.indexOf("abc", "ab") returns 0<br>
         * UTF16.indexOf("abc\ud800\udc00", "\ud800\udc00") returns 3<br>
         * UTF16.indexOf("abc\ud800\udc00", "\ud800") returns -1<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param str UTF16 format Unicode string to search for
         * @return the index of the first occurrence of the codepoint in the argument Unicode string, or
         *         -1 if the codepoint does not occur.
         * @stable ICU 2.6
         */
        public static int IndexOf(string source, string str)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.IndexOf(str);
            }

            int result = source.IndexOf(str);
            int resultEnd = result + strLength;
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[resultEnd + 1]))
                {
                    return IndexOf(source, str, resultEnd + 1);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return IndexOf(source, str, resultEnd + 1);
                }
            }
            return result;
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the first occurrence of
         * the argument codepoint. I.e., the smallest index i such that: <br>
         * (UTF16.charAt(source, i) == char32 &amp;&amp; i &gt;= fromIndex) is true.
         * <p>
         * If no such character occurs in this string, then -1 is returned.
         * </p>
         * <p>
         * Examples:<br>
         * UTF16.indexOf("abc", 'a', 1) returns -1<br>
         * UTF16.indexOf("abc\ud800\udc00", 0x10000, 1) returns 3<br>
         * UTF16.indexOf("abc\ud800\udc00", 0xd800, 1) returns -1<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param char32 Codepoint to search for
         * @param fromIndex The index to start the search from.
         * @return the index of the first occurrence of the codepoint in the argument Unicode string at
         *         or after fromIndex, or -1 if the codepoint does not occur.
         * @stable ICU 2.6
         */
        public static int IndexOf(string source, int char32, int fromIndex)
        {
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LEAD_SURROGATE_MIN_VALUE
                    || (char32 > TRAIL_SURROGATE_MAX_VALUE && char32 < SUPPLEMENTARY_MIN_VALUE))
            {
                return source.IndexOf((char)char32, fromIndex);
            }
            // surrogate
            if (char32 < SUPPLEMENTARY_MIN_VALUE)
            {
                int result = source.IndexOf((char)char32, fromIndex);
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
            return source.IndexOf(char32str, fromIndex);
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the first occurrence of
         * the argument string str. This method is implemented based on codepoints, hence a "lead
         * surrogate character + trail surrogate character" is treated as one entity.e Hence if the str
         * starts with trail surrogate character at index 0, a source with a leading a surrogate
         * character before str found at in source will not have a valid match. Vice versa for lead
         * surrogates that ends str. See example below.
         * <p>
         * If no such string str occurs in this source, then -1 is returned.
         * </p>
         * <p>
         * Examples:<br>
         * UTF16.indexOf("abc", "ab", 0) returns 0<br>
         * UTF16.indexOf("abc\ud800\udc00", "\ud800\udc00", 0) returns 3<br>
         * UTF16.indexOf("abc\ud800\udc00", "\ud800\udc00", 2) returns 3<br>
         * UTF16.indexOf("abc\ud800\udc00", "\ud800", 0) returns -1<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param str UTF16 format Unicode string to search for
         * @param fromIndex The index to start the search from.
         * @return the index of the first occurrence of the codepoint in the argument Unicode string, or
         *         -1 if the codepoint does not occur.
         * @stable ICU 2.6
         */
        public static int IndexOf(string source, string str, int fromIndex)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.IndexOf(str, fromIndex);
            }

            int result = source.IndexOf(str, fromIndex);
            int resultEnd = result + strLength;
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[resultEnd]))
                {
                    return IndexOf(source, str, resultEnd + 1);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return IndexOf(source, str, resultEnd + 1);
                }
            }
            return result;
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the last occurrence of
         * the argument codepoint. I.e., the index returned is the largest value i such that:
         * UTF16.charAt(source, i) == char32 is true.
         * <p>
         * Examples:<br>
         * UTF16.lastIndexOf("abc", 'a') returns 0<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", 0x10000) returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", 0xd800) returns -1<br>
         * </p>
         * <p>
         * source is searched backwards starting at the last character.
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param char32 Codepoint to search for
         * @return the index of the last occurrence of the codepoint in source, or -1 if the codepoint
         *         does not occur.
         * @stable ICU 2.6
         */
        public static int LastIndexOf(string source, int char32)
        {
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LEAD_SURROGATE_MIN_VALUE
                    || (char32 > TRAIL_SURROGATE_MAX_VALUE && char32 < SUPPLEMENTARY_MIN_VALUE))
            {
                return source.LastIndexOf((char)char32);
            }
            // surrogate
            if (char32 < SUPPLEMENTARY_MIN_VALUE)
            {
                int result = source.LastIndexOf((char)char32);
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return LastIndexOf(source, char32, result - 1);
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
            return source.LastIndexOf(char32str);
        }

        /**
         * Returns the index within the argument UTF16 format Unicode string of the last occurrence of
         * the argument string str. This method is implemented based on codepoints, hence a "lead
         * surrogate character + trail surrogate character" is treated as one entity.e Hence if the str
         * starts with trail surrogate character at index 0, a source with a leading a surrogate
         * character before str found at in source will not have a valid match. Vice versa for lead
         * surrogates that ends str. See example below.
         * <p>
         * Examples:<br>
         * UTF16.lastIndexOf("abc", "a") returns 0<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", "\ud800\udc00") returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", "\ud800") returns -1<br>
         * </p>
         * <p>
         * source is searched backwards starting at the last character.
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param str UTF16 format Unicode string to search for
         * @return the index of the last occurrence of the codepoint in source, or -1 if the codepoint
         *         does not occur.
         * @stable ICU 2.6
         */
        public static int LastIndexOf(string source, string str)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.LastIndexOf(str);
            }

            int result = source.LastIndexOf(str);
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[result + strLength + 1]))
                {
                    return LastIndexOf(source, str, result - 1);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return LastIndexOf(source, str, result - 1);
                }
            }
            return result;
        }

        /**
         * <p>
         * Returns the index within the argument UTF16 format Unicode string of the last occurrence of
         * the argument codepoint, where the result is less than or equals to fromIndex.
         * </p>
         * <p>
         * This method is implemented based on codepoints, hence a single surrogate character will not
         * match a supplementary character.
         * </p>
         * <p>
         * source is searched backwards starting at the last character starting at the specified index.
         * </p>
         * <p>
         * Examples:<br>
         * UTF16.lastIndexOf("abc", 'c', 2) returns 2<br>
         * UTF16.lastIndexOf("abc", 'c', 1) returns -1<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", 0x10000, 5) returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", 0x10000, 3) returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", 0xd800) returns -1<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param char32 Codepoint to search for
         * @param fromIndex the index to start the search from. There is no restriction on the value of
         *            fromIndex. If it is greater than or equal to the length of this string, it has the
         *            same effect as if it were equal to one less than the length of this string: this
         *            entire string may be searched. If it is negative, it has the same effect as if it
         *            were -1: -1 is returned.
         * @return the index of the last occurrence of the codepoint in source, or -1 if the codepoint
         *         does not occur.
         * @stable ICU 2.6
         */
        public static int LastIndexOf(string source, int char32, int fromIndex)
        {
            if (char32 < CODEPOINT_MIN_VALUE || char32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Argument char32 is not a valid codepoint");
            }
            // non-surrogate bmp
            if (char32 < LEAD_SURROGATE_MIN_VALUE
                    || (char32 > TRAIL_SURROGATE_MAX_VALUE && char32 < SUPPLEMENTARY_MIN_VALUE))
            {
                return source.LastIndexOf((char)char32, fromIndex);
            }
            // surrogate
            if (char32 < SUPPLEMENTARY_MIN_VALUE)
            {
                int result = source.LastIndexOf((char)char32, fromIndex);
                if (result >= 0)
                {
                    if (IsLeadSurrogate((char)char32) && (result < source.Length - 1)
                            && IsTrailSurrogate(source[result + 1]))
                    {
                        return LastIndexOf(source, char32, result - 1);
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
            return source.LastIndexOf(char32str, fromIndex);
        }

        /**
         * <p>
         * Returns the index within the argument UTF16 format Unicode string of the last occurrence of
         * the argument string str, where the result is less than or equals to fromIndex.
         * </p>
         * <p>
         * This method is implemented based on codepoints, hence a "lead surrogate character + trail
         * surrogate character" is treated as one entity. Hence if the str starts with trail surrogate
         * character at index 0, a source with a leading a surrogate character before str found at in
         * source will not have a valid match. Vice versa for lead surrogates that ends str.
         * </p>
         * See example below.
         * <p>
         * Examples:<br>
         * UTF16.lastIndexOf("abc", "c", 2) returns 2<br>
         * UTF16.lastIndexOf("abc", "c", 1) returns -1<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", "\ud800\udc00", 5) returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", "\ud800\udc00", 3) returns 3<br>
         * UTF16.lastIndexOf("abc\ud800\udc00", "\ud800", 4) returns -1<br>
         * </p>
         * <p>
         * source is searched backwards starting at the last character.
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string that will be searched
         * @param str UTF16 format Unicode string to search for
         * @param fromIndex the index to start the search from. There is no restriction on the value of
         *            fromIndex. If it is greater than or equal to the length of this string, it has the
         *            same effect as if it were equal to one less than the length of this string: this
         *            entire string may be searched. If it is negative, it has the same effect as if it
         *            were -1: -1 is returned.
         * @return the index of the last occurrence of the codepoint in source, or -1 if the codepoint
         *         does not occur.
         * @stable ICU 2.6
         */
        public static int LastIndexOf(string source, string str, int fromIndex)
        {
            int strLength = str.Length;
            // non-surrogate ends
            if (!IsTrailSurrogate(str[0]) && !IsLeadSurrogate(str[strLength - 1]))
            {
                return source.LastIndexOf(str, fromIndex);
            }

            int result = source.LastIndexOf(str, fromIndex);
            if (result >= 0)
            {
                // check last character
                if (IsLeadSurrogate(str[strLength - 1]) && (result < source.Length - 1)
                        && IsTrailSurrogate(source[result + strLength]))
                {
                    return LastIndexOf(source, str, result - 1);
                }
                // check first character which is a trail surrogate
                if (IsTrailSurrogate(str[0]) && result > 0
                        && IsLeadSurrogate(source[result - 1]))
                {
                    return LastIndexOf(source, str, result - 1);
                }
            }
            return result;
        }

        /**
         * Returns a new UTF16 format Unicode string resulting from replacing all occurrences of
         * oldChar32 in source with newChar32. If the character oldChar32 does not occur in the UTF16
         * format Unicode string source, then source will be returned. Otherwise, a new String object is
         * created that represents a codepoint sequence identical to the codepoint sequence represented
         * by source, except that every occurrence of oldChar32 is replaced by an occurrence of
         * newChar32.
         * <p>
         * Examples: <br>
         * UTF16.replace("mesquite in your cellar", 'e', 'o');<br>
         * returns "mosquito in your collar"<br>
         * UTF16.replace("JonL", 'q', 'x');<br>
         * returns "JonL" (no change)<br>
         * UTF16.replace("Supplementary character \ud800\udc00", 0x10000, '!'); <br>
         * returns "Supplementary character !"<br>
         * UTF16.replace("Supplementary character \ud800\udc00", 0xd800, '!'); <br>
         * returns "Supplementary character \ud800\udc00"<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string which the codepoint replacements will be based on.
         * @param oldChar32 Non-zero old codepoint to be replaced.
         * @param newChar32 The new codepoint to replace oldChar32
         * @return new String derived from source by replacing every occurrence of oldChar32 with
         *         newChar32, unless when no oldChar32 is found in source then source will be returned.
         * @stable ICU 2.6
         */
        public static string Replace(string source, int oldChar32, int newChar32)
        {
            if (oldChar32 <= 0 || oldChar32 > CODEPOINT_MAX_VALUE)
            {
                throw new ArgumentException("Argument oldChar32 is not a valid codepoint");
            }
            if (newChar32 <= 0 || newChar32 > CODEPOINT_MAX_VALUE)
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

            if (oldChar32 >= SUPPLEMENTARY_MIN_VALUE)
            {
                oldChar32Size = 2;
            }

            while (index != -1)
            {
                int endResultIndex = resultIndex + oldChar32Size;
                result.Replace(resultIndex, endResultIndex, newChar32Str);
                int lastEndIndex = index + oldChar32Size;
                index = IndexOf(source, oldChar32, lastEndIndex);
                resultIndex += newChar32Size + index - lastEndIndex;
            }
            return result.ToString();
        }

        /**
         * Returns a new UTF16 format Unicode string resulting from replacing all occurrences of oldStr
         * in source with newStr. If the string oldStr does not occur in the UTF16 format Unicode string
         * source, then source will be returned. Otherwise, a new String object is created that
         * represents a codepoint sequence identical to the codepoint sequence represented by source,
         * except that every occurrence of oldStr is replaced by an occurrence of newStr.
         * <p>
         * Examples: <br>
         * UTF16.replace("mesquite in your cellar", "e", "o");<br>
         * returns "mosquito in your collar"<br>
         * UTF16.replace("mesquite in your cellar", "mesquite", "cat");<br>
         * returns "cat in your cellar"<br>
         * UTF16.replace("JonL", "q", "x");<br>
         * returns "JonL" (no change)<br>
         * UTF16.replace("Supplementary character \ud800\udc00", "\ud800\udc00", '!'); <br>
         * returns "Supplementary character !"<br>
         * UTF16.replace("Supplementary character \ud800\udc00", "\ud800", '!'); <br>
         * returns "Supplementary character \ud800\udc00"<br>
         * </p>
         * Note this method is provided as support to jdk 1.3, which does not support supplementary
         * characters to its fullest.
         *
         * @param source UTF16 format Unicode string which the replacements will be based on.
         * @param oldStr Non-zero-length string to be replaced.
         * @param newStr The new string to replace oldStr
         * @return new String derived from source by replacing every occurrence of oldStr with newStr.
         *         When no oldStr is found in source, then source will be returned.
         * @stable ICU 2.6
         */
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
                int endResultIndex = resultIndex + oldStrSize;
                result.Replace(resultIndex, endResultIndex, newStr);
                int lastEndIndex = index + oldStrSize;
                index = IndexOf(source, oldStr, lastEndIndex);
                resultIndex += newStrSize + index - lastEndIndex;
            }
            return result.ToString();
        }

        /**
         * Reverses a UTF16 format Unicode string and replaces source's content with it. This method
         * will reverse surrogate characters correctly, instead of blindly reversing every character.
         * <p>
         * Examples:<br>
         * UTF16.reverse(new StringBuffer( "Supplementary characters \ud800\udc00\ud801\udc01"))<br>
         * returns "\ud801\udc01\ud800\udc00 sretcarahc yratnemelppuS".
         *
         * @param source The source StringBuffer that contains UTF16 format Unicode string to be reversed
         * @return a modified source with reversed UTF16 format Unicode string.
         * @stable ICU 2.6
         */
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

        /**
         * Check if the string contains more Unicode code points than a certain number. This is more
         * efficient than counting all code points in the entire string and comparing that number with a
         * threshold. This function may not need to scan the string at all if the length is within a
         * certain range, and never needs to count more than 'number + 1' code points. Logically
         * equivalent to (countCodePoint(s) &gt; number). A Unicode code point may occupy either one or two
         * code units.
         *
         * @param source The input string.
         * @param number The number of code points in the string is compared against the 'number'
         *            parameter.
         * @return boolean value for whether the string contains more Unicode code points than 'number'.
         * @stable ICU 2.4
         */
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

        /**
         * Check if the sub-range of char array, from argument start to limit, contains more Unicode
         * code points than a certain number. This is more efficient than counting all code points in
         * the entire char array range and comparing that number with a threshold. This function may not
         * need to scan the char array at all if start and limit is within a certain range, and never
         * needs to count more than 'number + 1' code points. Logically equivalent to
         * (countCodePoint(source, start, limit) &gt; number). A Unicode code point may occupy either one
         * or two code units.
         *
         * @param source Array of UTF-16 chars
         * @param start Offset to substring in the source array for analyzing
         * @param limit Offset to substring in the source array for analyzing
         * @param number The number of code points in the string is compared against the 'number'
         *            parameter.
         * @return boolean value for whether the string contains more Unicode code points than 'number'.
         * @exception IndexOutOfBoundsException Thrown when limit &lt; start
         * @stable ICU 2.4
         */
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

        /**
         * Check if the string buffer contains more Unicode code points than a certain number. This is
         * more efficient than counting all code points in the entire string buffer and comparing that
         * number with a threshold. This function may not need to scan the string buffer at all if the
         * length is within a certain range, and never needs to count more than 'number + 1' code
         * points. Logically equivalent to (countCodePoint(s) &gt; number). A Unicode code point may
         * occupy either one or two code units.
         *
         * @param source The input string buffer.
         * @param number The number of code points in the string buffer is compared against the 'number'
         *            parameter.
         * @return boolean value for whether the string buffer contains more Unicode code points than
         *         'number'.
         * @stable ICU 2.4
         */
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

        /**
         * Cover JDK 1.5 API. Create a String from an array of codePoints.
         *
         * @param codePoints The code array
         * @param offset The start of the text in the code point array
         * @param count The number of code points
         * @return a String representing the code points between offset and count
         * @throws IllegalArgumentException If an invalid code point is encountered
         * @throws IndexOutOfBoundsException If the offset or count are out of bounds.
         * @stable ICU 3.0
         */
        public static string NewString(int[] codePoints, int offset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentException();
            }
            char[] chars = new char[count];
            int w = 0;
            for (int r = offset, e = offset + count; r < e; ++r)
            {
                int cp = codePoints[r];
                if (cp < 0 || cp > 0x10ffff)
                {
                    throw new ArgumentException();
                }
                while (true)
                {
                    try
                    {
                        if (cp < 0x010000)
                        {
                            chars[w] = (char)cp;
                            w++;
                        }
                        else
                        {
                            chars[w] = (char)(LEAD_SURROGATE_OFFSET_ + (cp >> LEAD_SURROGATE_SHIFT_));
                            chars[w + 1] = (char)(TRAIL_SURROGATE_MIN_VALUE + (cp & TRAIL_SURROGATE_MASK_));
                            w += 2;
                        }
                        break;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        int newlen = (int)(Math.Ceiling((double)codePoints.Length * (w + 2)
                                / (r - offset + 1)));
                        char[] temp = new char[newlen];
                        System.Array.Copy(chars, 0, temp, 0, w);
                        chars = temp;
                    }
                }
            }
            return new string(chars, 0, w);
        }

        /**
         * <p>
         * UTF16 string comparator class. Allows UTF16 string comparison to be done with the various
         * modes
         * </p>
         * <ul>
         * <li> Code point comparison or code unit comparison
         * <li> Case sensitive comparison, case insensitive comparison or case insensitive comparison
         * with special handling for character 'i'.
         * </ul>
         * <p>
         * The code unit or code point comparison differ only when comparing supplementary code points
         * (&#92;u10000..&#92;u10ffff) to BMP code points near the end of the BMP (i.e.,
         * &#92;ue000..&#92;uffff). In code unit comparison, high BMP code points sort after
         * supplementary code points because they are stored as pairs of surrogates which are at
         * &#92;ud800..&#92;udfff.
         * </p>
         *
         * @see #FOLD_CASE_DEFAULT
         * @see #FOLD_CASE_EXCLUDE_SPECIAL_I
         * @stable ICU 2.1
         */
        public sealed class StringComparer : IComparer<string>
        {
            // public constructor ------------------------------------------------

            /**
             * Default constructor that does code unit comparison and case sensitive comparison.
             *
             * @stable ICU 2.1
             */
            public StringComparer()
                    : this(false, false, FOLD_CASE_DEFAULT)
            {
            }

            /**
             * Constructor that does comparison based on the argument options.
             *
             * @param codepointcompare Flag to indicate true for code point comparison or false for code unit
             *            comparison.
             * @param ignorecase False for case sensitive comparison, true for case-insensitive comparison
             * @param foldcaseoption FOLD_CASE_DEFAULT or FOLD_CASE_EXCLUDE_SPECIAL_I. This option is used only
             *            when ignorecase is set to true. If ignorecase is false, this option is
             *            ignored.
             * @see #FOLD_CASE_DEFAULT
             * @see #FOLD_CASE_EXCLUDE_SPECIAL_I
             * @throws IllegalArgumentException If foldcaseoption is out of range
             * @stable ICU 2.4
             */
            public StringComparer(bool codepointcompare, bool ignorecase, int foldcaseoption)
            {
                SetCodePointCompare(codepointcompare);
                m_ignoreCase_ = ignorecase;
                if (foldcaseoption < FOLD_CASE_DEFAULT || foldcaseoption > FOLD_CASE_EXCLUDE_SPECIAL_I)
                {
                    throw new ArgumentException("Invalid fold case option");
                }
                m_foldCase_ = foldcaseoption;
            }

            // public data member ------------------------------------------------

            /**
             * Option value for case folding comparison:
             *
             * <p>Comparison is case insensitive, strings are folded using default mappings defined in
             * Unicode data file CaseFolding.txt, before comparison.
             *
             * @stable ICU 2.4
             */
            public static readonly int FOLD_CASE_DEFAULT = 0;

            /**
             * Option value for case folding:
             * Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
             * and dotless i appropriately for Turkic languages (tr, az).
             *
             * <p>Comparison is case insensitive, strings are folded using modified mappings defined in
             * Unicode data file CaseFolding.txt, before comparison.
             *
             * @stable ICU 2.4
             * @see com.ibm.icu.lang.UCharacter#FOLD_CASE_EXCLUDE_SPECIAL_I
             */
            public static readonly int FOLD_CASE_EXCLUDE_SPECIAL_I = 1;

            // public methods ----------------------------------------------------

            // public setters ----------------------------------------------------

            /**
             * Sets the comparison mode to code point compare if flag is true. Otherwise comparison mode
             * is set to code unit compare
             *
             * @param flag True for code point compare, false for code unit compare
             * @stable ICU 2.4
             */
            public void SetCodePointCompare(bool flag)
            {
                if (flag)
                {
                    m_codePointCompare_ = Normalizer.COMPARE_CODE_POINT_ORDER;
                }
                else
                {
                    m_codePointCompare_ = 0;
                }
            }

            /**
             * Sets the Comparator to case-insensitive comparison mode if argument is true, otherwise
             * case sensitive comparison mode if set to false.
             *
             * @param ignorecase True for case-insitive comparison, false for case sensitive comparison
             * @param foldcaseoption FOLD_CASE_DEFAULT or FOLD_CASE_EXCLUDE_SPECIAL_I. This option is used only
             *            when ignorecase is set to true. If ignorecase is false, this option is
             *            ignored.
             * @see #FOLD_CASE_DEFAULT
             * @see #FOLD_CASE_EXCLUDE_SPECIAL_I
             * @stable ICU 2.4
             */
            public void SetIgnoreCase(bool ignorecase, int foldcaseoption)
            {
                m_ignoreCase_ = ignorecase;
                if (foldcaseoption < FOLD_CASE_DEFAULT || foldcaseoption > FOLD_CASE_EXCLUDE_SPECIAL_I)
                {
                    throw new ArgumentException("Invalid fold case option");
                }
                m_foldCase_ = foldcaseoption;
            }

            // public getters ----------------------------------------------------

            /**
             * Checks if the comparison mode is code point compare.
             *
             * @return true for code point compare, false for code unit compare
             * @stable ICU 2.4
             */
            public bool GetCodePointCompare()
            {
                return m_codePointCompare_ == Normalizer.COMPARE_CODE_POINT_ORDER;
            }

            /**
             * Checks if Comparator is in the case insensitive mode.
             *
             * @return true if Comparator performs case insensitive comparison, false otherwise
             * @stable ICU 2.4
             */
            public bool GetIgnoreCase()
            {
                return m_ignoreCase_;
            }

            /**
             * Gets the fold case options set in Comparator to be used with case insensitive comparison.
             *
             * @return either FOLD_CASE_DEFAULT or FOLD_CASE_EXCLUDE_SPECIAL_I
             * @see #FOLD_CASE_DEFAULT
             * @see #FOLD_CASE_EXCLUDE_SPECIAL_I
             * @stable ICU 2.4
             */
            public int GetIgnoreCaseOption()
            {
                return m_foldCase_;
            }

            // public other methods ----------------------------------------------

            /**
             * Compare two strings depending on the options selected during construction.
             *
             * @param a first source string.
             * @param b second source string.
             * @return 0 returned if a == b. If a &lt; b, a negative value is returned. Otherwise if a &gt; b,
             *         a positive value is returned.
             * @exception InvalidCastException thrown when either a or b is not a String object
             * @stable ICU 4.4
             */
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

            /**
             * Code unit comparison flag. True if code unit comparison is required. False if code point
             * comparison is required.
             */
            private int m_codePointCompare_;

            /**
             * Fold case comparison option.
             */
            private int m_foldCase_;

            /**
             * Flag indicator if ignore case is to be used during comparison
             */
            private bool m_ignoreCase_;

            /**
             * Code point order offset for surrogate characters
             */
            private static readonly int CODE_POINT_COMPARE_SURROGATE_OFFSET_ = 0x2800;

            // private method ---------------------------------------------------

            /**
             * Compares case insensitive. This is a direct port of ICU4C, to make maintainence life
             * easier.
             *
             * @param s1
             *            first string to compare
             * @param s2
             *            second string to compare
             * @return -1 is s1 &lt; s2, 0 if equals,
             */
            private int CompareCaseInsensitive(string s1, string s2)
            {
                return Normalizer.CmpEquivFold(s1.ToCharSequence(), s2.ToCharSequence(), m_foldCase_ | m_codePointCompare_
                        | Normalizer.COMPARE_IGNORE_CASE);
            }

            /**
             * Compares case sensitive. This is a direct port of ICU4C, to make maintainence life
             * easier.
             *
             * @param s1
             *            first string to compare
             * @param s2
             *            second string to compare
             * @return -1 is s1 &lt; s2, 0 if equals,
             */
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
                if (c1 >= LEAD_SURROGATE_MIN_VALUE && c2 >= LEAD_SURROGATE_MIN_VALUE
                        && codepointcompare)
                {
                    // subtract 0x2800 from BMP code points to make them smaller
                    // than supplementary ones
                    if ((c1 <= LEAD_SURROGATE_MAX_VALUE && (index + 1) != length1 && IsTrailSurrogate(s1[index + 1]))
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

                    if ((c2 <= LEAD_SURROGATE_MAX_VALUE && (index + 1) != length2 && IsTrailSurrogate(s2[index + 1]))
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

        /// <summary>
        /// Utility for getting a code point from a ICharSequence that contains exactly one code point.
        /// </summary>
        /// <param name="s">to test</param>
        /// <returns>The code point IF the string is non-null and consists of a single code point. Otherwise returns -1.</returns>
        /// <stable>ICU 54</stable>
        public static int GetSingleCodePoint(string s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Utility for getting a code point from a ICharSequence that contains exactly one code point.
        /// </summary>
        /// <param name="s">to test</param>
        /// <returns>The code point IF the string is non-null and consists of a single code point. Otherwise returns -1.</returns>
        /// <stable>ICU 54</stable>
        public static int GetSingleCodePoint(StringBuilder s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Utility for getting a code point from a ICharSequence that contains exactly one code point.
        /// </summary>
        /// <param name="s">to test</param>
        /// <returns>The code point IF the string is non-null and consists of a single code point. Otherwise returns -1.</returns>
        /// <stable>ICU 54</stable>
        public static int GetSingleCodePoint(char[] s)
        {
            return GetSingleCodePoint(s.ToCharSequence());
        }

        /// <summary>
        /// Utility for getting a code point from a ICharSequence that contains exactly one code point.
        /// </summary>
        /// <param name="s">to test</param>
        /// <returns>The code point IF the string is non-null and consists of a single code point. Otherwise returns -1.</returns>
        /// <stable>ICU 54</stable>
        internal static int GetSingleCodePoint(ICharSequence s)
        {
            if (s == null || s.Length == 0)
            {
                return -1;
            }
            else if (s.Length == 1)
            {
                return s[0];
            }
            else if (s.Length > 2)
            {
                return -1;
            }

            // at this point, len = 2
            int cp = Character.CodePointAt(s, 0);
            if (cp > 0xFFFF)
            { // is surrogate pair
                return cp;
            }
            return -1;
        }

        /// <summary>
        /// Utility for comparing a code point to a string without having to create a new string. Returns the same results
        /// as a code point comparison of UTF16.ValueOf(codePoint) and s.ToString(). More specifically, if
        /// <code>
        ///    sc = new StringComparer(true,false,0);
        ///    fast = UTF16.CompareCodePoint(codePoint, charSequence)
        ///    slower = sc.Compare(UTF16.ValueOf(codePoint), charSequence == null ? "" : charSequence.ToString())
        /// </code>
        /// then
        /// <code>
        ///    Math.Sign(fast) == Math.Sign(slower)
        /// </code>
        /// </summary>
        /// <param name="codePoint">CodePoint to test.</param>
        /// <param name="s">String to test.</param>
        /// <returns>Equivalent of code point comparator comparing two strings.</returns>
        /// <stable>ICU 54</stable>
        public static int CompareCodePoint(int codePoint, string s)
        {
            return CompareCodePoint(codePoint, s.ToCharSequence());
        }

        /// <summary>
        /// Utility for comparing a code point to a string without having to create a new string. Returns the same results
        /// as a code point comparison of UTF16.ValueOf(codePoint) and s.ToString(). More specifically, if
        /// <code>
        ///    sc = new StringComparer(true,false,0);
        ///    fast = UTF16.CompareCodePoint(codePoint, charSequence)
        ///    slower = sc.Compare(UTF16.ValueOf(codePoint), charSequence == null ? "" : charSequence.ToString())
        /// </code>
        /// then
        /// <code>
        ///    Math.Sign(fast) == Math.Sign(slower)
        /// </code>
        /// </summary>
        /// <param name="codePoint">CodePoint to test.</param>
        /// <param name="s">String to test.</param>
        /// <returns>Equivalent of code point comparator comparing two strings.</returns>
        /// <stable>ICU 54</stable>
        public static int CompareCodePoint(int codePoint, StringBuilder s)
        {
            return CompareCodePoint(codePoint, s.ToCharSequence());
        }

        /// <summary>
        /// Utility for comparing a code point to a string without having to create a new string. Returns the same results
        /// as a code point comparison of UTF16.ValueOf(codePoint) and s.ToString(). More specifically, if
        /// <code>
        ///    sc = new StringComparer(true,false,0);
        ///    fast = UTF16.CompareCodePoint(codePoint, charSequence)
        ///    slower = sc.Compare(UTF16.ValueOf(codePoint), charSequence == null ? "" : charSequence.ToString())
        /// </code>
        /// then
        /// <code>
        ///    Math.Sign(fast) == Math.Sign(slower)
        /// </code>
        /// </summary>
        /// <param name="codePoint">CodePoint to test.</param>
        /// <param name="s">String to test.</param>
        /// <returns>Equivalent of code point comparator comparing two strings.</returns>
        /// <stable>ICU 54</stable>
        public static int CompareCodePoint(int codePoint, char[] s)
        {
            return CompareCodePoint(codePoint, s.ToCharSequence());
        }

        /// <summary>
        /// Utility for comparing a code point to a string without having to create a new string. Returns the same results
        /// as a code point comparison of UTF16.ValueOf(codePoint) and s.ToString(). More specifically, if
        /// <code>
        ///    sc = new StringComparer(true,false,0);
        ///    fast = UTF16.CompareCodePoint(codePoint, charSequence)
        ///    slower = sc.Compare(UTF16.ValueOf(codePoint), charSequence == null ? "" : charSequence.ToString())
        /// </code>
        /// then
        /// <code>
        ///    Math.Sign(fast) == Math.Sign(slower)
        /// </code>
        /// </summary>
        /// <param name="codePoint">CodePoint to test.</param>
        /// <param name="s">String to test.</param>
        /// <returns>Equivalent of code point comparator comparing two strings.</returns>
        /// <stable>ICU 54</stable>
        internal static int CompareCodePoint(int codePoint, ICharSequence s)
        {
            if (s == null)
            {
                return 1;
            }
            int strLen = s.Length;
            if (strLen == 0)
            {
                return 1;
            }
            int second = Character.CodePointAt(s, 0);
            int diff = codePoint - second;
            if (diff != 0)
            {
                return diff;
            }
            return strLen == Character.CharCount(codePoint) ? 0 : -1;
        }

        // private data members -------------------------------------------------

        /**
         * Shift value for lead surrogate to form a supplementary character.
         */
        private static readonly int LEAD_SURROGATE_SHIFT_ = 10;

        /**
         * Mask to retrieve the significant value from a trail surrogate.
         */
        private static readonly int TRAIL_SURROGATE_MASK_ = 0x3FF;

        /**
         * Value that all lead surrogate starts with
         */
        private static readonly int LEAD_SURROGATE_OFFSET_ = LEAD_SURROGATE_MIN_VALUE
                - (SUPPLEMENTARY_MIN_VALUE >> LEAD_SURROGATE_SHIFT_);

        // private methods ------------------------------------------------------

        /**
         * <p>
         * Converts argument code point and returns a String object representing the code point's value
         * in UTF16 format.
         * </p>
         * <p>
         * This method does not check for the validity of the codepoint, the results are not guaranteed
         * if a invalid codepoint is passed as argument.
         * </p>
         * <p>
         * The result is a string whose length is 1 for non-supplementary code points, 2 otherwise.
         * </p>
         *
         * @param ch
         *            code point
         * @return string representation of the code point
         */
        private static string ToString(int ch)
        {
            if (ch < SUPPLEMENTARY_MIN_VALUE)
            {
                return new string((char)ch, 1);
            }

            StringBuilder result = new StringBuilder();
            result.Append(GetLeadSurrogate(ch));
            result.Append(GetTrailSurrogate(ch));
            return result.ToString();
        }
    }
}
