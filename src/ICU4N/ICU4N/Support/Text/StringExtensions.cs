using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support.Text
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Convenience method to wrap a string in a <see cref="StringCharSequence"/>
        /// so a <see cref="string"/> can be used as <see cref="ICharSequence"/> in .NET.
        /// </summary>
        internal static ICharSequence ToCharSequence(this string text)
        {
            return new StringCharSequence(text);
        }

        public static byte[] GetBytes(this string str, Encoding enc)
        {
            return enc.GetBytes(str);
        }

        /// <summary>
        /// This method mimics the Java String.compareTo(String) method in that it
        /// <list type="number">
        /// <item><description>Compares the strings using lexographic sorting rules</description></item>
        /// <item><description>Performs a culture-insensitive comparison</description></item>
        /// </list>
        /// This method is a convenience to replace the .NET CompareTo method 
        /// on all strings, provided the logic does not expect specific values
        /// but is simply comparing them with <c>&gt;</c> or <c>&lt;</c>.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">The string to compare with.</param>
        /// <returns>
        /// An integer that indicates the lexical relationship between the two comparands.
        /// Less than zero indicates the comparison value is greater than the current string.
        /// Zero indicates the strings are equal.
        /// Greater than zero indicates the comparison value is less than the current string.
        /// </returns>
        public static int CompareToOrdinal(this string str, string value)
        {
            return string.CompareOrdinal(str, value);
        }

        public static int CodePointBefore(this string seq, int index)
        {
            return Character.CodePointBefore(seq, index);
        }

        public static int CodePointAt(this string str, int index)
        {
            return Character.CodePointAt(str, index);
        }

        /// <summary>
        /// Returns the number of Unicode code points in the specified text
        /// range of this <see cref="string"/>. The text range begins at the
        /// specified <paramref name="beginIndex"/> and extends to the
        /// <see cref="char"/> at index <c>endIndex - 1</c>. Thus the
        /// length (in <see cref="char"/>s) of the text range is
        /// <c>endIndex-beginIndex</c>. Unpaired surrogates within
        /// the text range count as one code point each.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="beginIndex">the index to the first <see cref="char"/> of the text range.</param>
        /// <param name="endIndex">the index after the last <see cref="char"/> of the text range.</param>
        /// <returns>the number of Unicode code points in the specified text range</returns>
        /// <exception cref="IndexOutOfRangeException">if the <paramref name="beginIndex"/> is negative, or
        /// <paramref name="endIndex"/> is larger than the length of this <see cref="string"/>, or
        /// <paramref name="beginIndex"/> is larger than <paramref name="endIndex"/>.</exception>
        public static int CodePointCount(this string str, int beginIndex, int endIndex)
        {
            if (beginIndex < 0 || endIndex > str.Length || beginIndex > endIndex)
            {
                throw new IndexOutOfRangeException();
            }
            return Character.CodePointCountImpl(str.ToCharArray(), beginIndex, endIndex - beginIndex);
        }

        /// <summary>
        /// Returns the index within this string of the first occurrence of
        /// the specified character. If a character with value
        /// <paramref name="ch"/> occurs in the character sequence represented by
        /// this <see cref="string"/> object, then the index (in Unicode
        /// code units) of the first such occurrence is returned. For
        /// values of <paramref name="ch"/> in the range from 0 to 0xFFFF
        /// (inclusive), this is the smallest value <i>k</i> such that:
        /// <code>
        ///     this[(<i>k</i>] == ch
        /// </code>
        /// is true. For other values of <paramref name="ch"/>, it is the
        /// smallest value <i>k</i> such that:
        /// <code>
        ///     this.CodePointAt(<i>k</i>) == ch
        /// </code>
        /// is true. In either case, if no such character occurs in this
        /// string, then <c>-1</c> is returned.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="ch">A character (Unicode code point).</param>
        /// <returns>The index of the first occurrence of the character in the
        /// character sequence represented by this object, or
        /// <c>-1</c> if the character does not occur.
        /// </returns>
        public static int IndexOf(this string text, int ch)
        {
            return IndexOf(text, ch, 0);
        }

        /// <summary>
        /// Returns the index within this string of the first occurrence of the
        /// specified character, starting the search at the specified index.
        /// </summary>
        /// <remarks>
        /// If a character with value <paramref name="ch"/> occurs in the
        /// character sequence represented by this <see cref="string"/>
        /// object at an index no smaller than <paramref name="fromIndex"/>, then
        /// the index of the first such occurrence is returned. For values
        /// of <paramref name="ch"/> in the range from 0 to 0xFFFF (inclusive),
        /// this is the smallest value <i>k</i> such that:
        /// <code>
        ///     (this[<i>k</i>] == ch) && (<i>k</i> &gt;= fromIndex)
        /// </code>
        /// is true. For other values of <code>ch</code>, it is the
        /// smallest value <i>k</i> such that:
        /// <code>
        ///     (this.CodePointAt(<i>k</i>) == ch) && (<i>k</i> &gt;= fromIndex)
        /// </code>
        /// is true. In either case, if no such character occurs in this
        /// string at or after position <paramref name="fromIndex"/>, then
        /// <c>-1</c> is returned.
        /// <para/>
        /// There is no restriction on the value of <paramref name="fromIndex"/>. If it
        /// is negative, it has the same effect as if it were zero: this entire
        /// string may be searched. If it is greater than the length of this
        /// string, it has the same effect as if it were equal to the length of
        /// this string: <c>-1</c> is returned.
        /// <para/>
        /// All indices are specified in <c>char</c> values
        /// (Unicode code units).
        /// </remarks>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="ch">A character (Unicode code point).</param>
        /// <param name="fromIndex">The index to start the search from.</param>
        /// <returns>The index of the first occurrence of the character in the
        /// character sequence represented by this object that is greater
        /// than or equal to <paramref name="fromIndex"/>, or <c>-1</c>
        /// if the character does not occur.
        /// </returns>
        public static int IndexOf(this string text, int ch, int fromIndex)
        {
            if (fromIndex < 0)
            {
                fromIndex = 0;
            }
            else if (fromIndex >= text.Length)
            {
                // Note: fromIndex might be near -1>>>1.
                return -1;
            }

            if (ch < Character.MIN_SUPPLEMENTARY_CODE_POINT)
            {
                // handle most cases here (ch is a BMP code point or a
                // negative value (invalid code point))
                int max = text.Length;
                for (int i = fromIndex; i < max; i++)
                {
                    if (text[i] == ch)
                    {
                        return i;
                    }
                }
                return -1;
            }
            else
            {
                return IndexOfSupplementary(text, ch, fromIndex);
            }
        }

        /// <summary>
        /// Handles (rare) calls of indexOf with a supplementary character.
        /// </summary>
        private static int IndexOfSupplementary(this string text, int ch, int fromIndex)
        {
            if (Character.IsValidCodePoint(ch))
            {
                string pair = char.ConvertFromUtf32(ch);
                char hi = pair[0];
                char lo = pair[1];
                int max = text.Length - 1;
                for (int i = fromIndex; i < max; i++)
                {
                    if (text[i] == hi && text[i + 1] == lo)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int OffsetByCodePoints(this string seq, int index,
                                         int codePointOffset)
        {
            return Character.OffsetByCodePoints(seq, index, codePointOffset);
        }

        /// <summary>
        /// Compares the specified string to this string and compares the specified
        /// range of characters to determine if they are the same.
        /// </summary>
        /// <param name="seq">This string.</param>
        /// <param name="thisStart">the starting offset in this string.</param>
        /// <param name="str">the string to compare.</param>
        /// <param name="start">the starting offset in the specified string.</param>
        /// <param name="length">the number of characters to compare.</param>
        /// <returns><c>true</c> if the ranges of characters are equal, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="str"/> is <c>null</c></exception>
        internal static bool RegionMatches(this string seq, int thisStart, string str, int start,
            int length)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length - start < length || start < 0)
            {
                return false;
            }
            if (thisStart < 0 || seq.Length - thisStart < length)
            {
                return false;
            }
            if (length <= 0)
            {
                return true;
            }
            int o1 = /*offset +*/ thisStart, o2 = /*str.offset +*/ start;
            for (int i = 0; i < length; ++i)
            {
                if (seq[o1 + i] != str[o2 + i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares the specified string to this string and compares the specified
        /// range of characters to determine if they are the same. When ignoreCase is
        /// true, the case of the characters is ignored during the comparison.
        /// </summary>
        /// <param name="seq">This string.</param>
        /// <param name="ignoreCase">Specifies if case should be ignored.</param>
        /// <param name="thisStart">The starting offset in this string.</param>
        /// <param name="str">The string to compare.</param>
        /// <param name="start">The starting offset in the specified string.</param>
        /// <param name="length">The number of characters to compare.</param>
        /// <returns><c>true</c> if the ranges of characters are equal, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="str"/> is <c>null</c>.</exception>
        public static bool RegionMatches(this string seq, bool ignoreCase, int thisStart,
            string str, int start, int length)
        {
            return RegionMatches(seq, null, ignoreCase, thisStart, str, start, length);
        }

        /// <summary>
        /// Compares the specified string to this string and compares the specified
        /// range of characters to determine if they are the same. When ignoreCase is
        /// true, the case of the characters is ignored during the comparison.
        /// </summary>
        /// <param name="seq">This string.</param>
        /// <param name="culture">The culture to use when correcting case for comparison (only applies if <paramref name="ignoreCase"/> is true).</param>
        /// <param name="ignoreCase">Specifies if case should be ignored.</param>
        /// <param name="thisStart">The starting offset in this string.</param>
        /// <param name="str">The string to compare.</param>
        /// <param name="start">The starting offset in the specified string.</param>
        /// <param name="length">The number of characters to compare.</param>
        /// <returns><c>true</c> if the ranges of characters are equal, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="str"/> is <c>null</c>.</exception>
        public static bool RegionMatches(this string seq, CultureInfo culture, bool ignoreCase, int thisStart,
            string str, int start, int length)
        {
            if (!ignoreCase)
            {
                return RegionMatches(seq, thisStart, str, start, length);
            }

            if (str != null)
            {
                if (thisStart < 0 || length > seq.Length - thisStart)
                {
                    return false;
                }
                if (start < 0 || length > str.Length - start)
                {
                    return false;
                }

                int end = thisStart + length;
                char c1, c2;
                string target = str;
                var textInfo = culture == null ? CultureInfo.CurrentCulture.TextInfo : culture.TextInfo;
                while (thisStart < end)
                {
                    if ((c1 = seq[thisStart++]) != (c2 = target[start++])
                            && textInfo.ToUpper(c1) != textInfo.ToUpper(c2)
                            // Required for unicode that we test both cases
                            && textInfo.ToLower(c1) != textInfo.ToLower(c2))
                    {
                        return false;
                    }
                }
                return true;
            }
            throw new ArgumentNullException();
        }

        internal static ICharSequence SubSequence(this string text, int start, int end)
        {
            // From Apache Harmony String class
            if (start == 0 && end == text.Length)
            {
                return text.ToCharSequence();
            }
            if (start < 0)
            {
                throw new IndexOutOfRangeException(nameof(start));
            }
            else if (start > end)
            {
                throw new IndexOutOfRangeException("end - start");
            }
            else if (end > text.Length)
            {
                throw new IndexOutOfRangeException(nameof(end));
            }

            return text.Substring(start, end - start).ToCharSequence();
        }

        /// <summary>
        /// Compares the specified string to this string, starting at the specified
        /// offset, to determine if the specified string is a prefix.
        /// </summary>
        /// <param name="text">This string.</param>
        /// <param name="prefix">the string to look for.</param>
        /// <param name="start">the starting offset.</param>
        /// <returns><c>true</c> if the specified string occurs in this string at the specified offset, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="prefix"/> is <c>null</c>.</exception>
        public static bool StartsWith(this string text, string prefix, int start)
        {
            return RegionMatches(text, start, prefix, 0, prefix.Length);
        }

        /// <summary>
        /// Compares a <see cref="string"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="cs">The character sequence to compare to.</param>
        /// <returns></returns>
        public static bool ContentEquals(this string text, string cs)
        {
            int len = cs.Length;

            if (len != text.Length)
            {
                return false;
            }

            if (len == 0 && text.Length == 0)
            {
                return true; // since both are empty strings
            }

            return RegionMatches(text, 0, cs.ToString(), 0, len);
        }

        /// <summary>
        /// Compares a <see cref="StringBuilder"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="cs">The character sequence to compare to.</param>
        /// <returns></returns>
        public static bool ContentEquals(this string text, StringBuilder cs)
        {
            int len = cs.Length;

            if (len != text.Length)
            {
                return false;
            }

            if (len == 0 && text.Length == 0)
            {
                return true; // since both are empty strings
            }

            return RegionMatches(text, 0, cs.ToString(), 0, len);
        }

        /// <summary>
        /// Compares a <see cref="T:char[]"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="cs">The character sequence to compare to.</param>
        /// <returns></returns>
        public static bool ContentEquals(this string text, char[] cs)
        {
            int len = cs.Length;

            if (len != text.Length)
            {
                return false;
            }

            if (len == 0 && text.Length == 0)
            {
                return true; // since both are empty strings
            }

            return RegionMatches(text, 0, cs.ToString(), 0, len);
        }

        /// <summary>
        /// Compares a <see cref="ICharSequence"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="cs">The character sequence to compare to.</param>
        /// <returns></returns>
        public static bool ContentEquals(this string text, ICharSequence cs)
        {
            int len = cs.Length;

            if (len != text.Length)
            {
                return false;
            }

            if (len == 0 && text.Length == 0)
            {
                return true; // since both are empty strings
            }

            return RegionMatches(text, 0, cs.ToString(), 0, len);
        }

        /// <summary> Expert:
        /// A string interner cache.
        /// This shouldn't be changed to an incompatible implementation after other APIs have been used.
        /// </summary>
        private static StringInterner interner =
#if NETSTANDARD1_3
            new SimpleStringInterner(1024, 8);
#else
            new StringInterner();
#endif

        /// <summary>
        /// Searches an internal table of strings for a string equal to this string.
        /// If the string is not in the table, it is added. Returns the string
        /// contained in the table which is equal to this string. The same string
        /// object is always returned for strings which are equal.
        /// </summary>
        /// <returns>The interned string equal to this string.</returns>
        public static string Intern(this string s)
        {
            return interner.Intern(s);
        }
    }
}
