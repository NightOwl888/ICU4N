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


        /// <summary> Expert:
        /// A string interner cache.
        /// This shouldn't be changed to an incompatible implementation after other APIs have been used.
        /// </summary>
        private static StringInterner interner =
#if NETSTANDARD1_6
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
