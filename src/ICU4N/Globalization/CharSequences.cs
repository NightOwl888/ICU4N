using J2N;
using J2N.Numerics;
using System;
#nullable enable

namespace ICU4N.Globalization
{
    [Obsolete("This API is ICU internal only.")]
    internal static partial class CharSequences // ICU4N specific - marked internal, since the functionality is obsolete
    {
        // TODO
        // compareTo(a, b);
        // compareToIgnoreCase(a, b)
        // contentEquals(a, b)
        // contentEqualsIgnoreCase(a, b)

        // contains(a, b) => indexOf >= 0
        // endsWith(a, b)
        // startsWith(a, b)

        // lastIndexOf(a, b, fromIndex)
        // indexOf(a, ch, fromIndex)
        // lastIndexOf(a, ch, fromIndex);

        // s.trim() => UnicodeSet.trim(CharSequence s); return a subsequence starting with the first character not in the set to the last character not in the set.
        // add UnicodeSet.split(CharSequence s);

        /// <summary>
        /// Find the longest n such that a[aIndex,n] = b[bIndex,n], and n is on a character boundary.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int MatchAfter(string a, string b, int aIndex, int bIndex)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            return MatchAfter(a.AsSpan(), b.AsSpan(), aIndex, bIndex);
        }

        /// <summary>
        /// Find the longest n such that a[aIndex,n] = b[bIndex,n], and n is on a character boundary.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int MatchAfter(string a, ReadOnlySpan<char> b, int aIndex, int bIndex)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));

            return MatchAfter(a.AsSpan(), b, aIndex, bIndex);
        }

        /// <summary>
        /// Find the longest n such that a[aIndex,n] = b[bIndex,n], and n is on a character boundary.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int MatchAfter(ReadOnlySpan<char> a, string b, int aIndex, int bIndex)
        {
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            return MatchAfter(a, b.AsSpan(), aIndex, bIndex);
        }

        /// <summary>
        /// Find the longest n such that a[aIndex,n] = b[bIndex,n], and n is on a character boundary.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int MatchAfter(ReadOnlySpan<char> a, ReadOnlySpan<char> b, int aIndex, int bIndex)
        {
            int i = aIndex, j = bIndex;
            int alen = a.Length;
            int blen = b.Length;
            for (; i < alen && j < blen; ++i, ++j)
            {
                char ca = a[i];
                char cb = b[j];
                if (ca != cb)
                {
                    break;
                }
            }
            // if we failed a match make sure that we didn't match half a character
            int result = i - aIndex;
            if (result != 0 && !OnCharacterBoundary(a, i) && !OnCharacterBoundary(b, j))
            {
                --result; // backup
            }
            return result;
        }

        /// <summary>
        /// Count the code point length. Unpaired surrogates count as 1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int CodePointLength(string s)
        {
            return Character.CodePointCount(s, 0, s.Length);
            //        int length = s.length();
            //        int result = length;
            //        for (int i = 1; i < length; ++i) {
            //            char ch = s.charAt(i);
            //            if (0xDC00 <= ch && ch <= 0xDFFF) {
            //                char ch0 = s.charAt(i-1);
            //                if (0xD800 <= ch && ch <= 0xDbFF) {
            //                    --result;
            //                }
            //            }
            //        }
        }


        /// <summary>
        /// Count the code point length. Unpaired surrogates count as 1.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int CodePointLength(ReadOnlySpan<char> s)
        {
            return Character.CodePointCount(s);
            //        int length = s.length();
            //        int result = length;
            //        for (int i = 1; i < length; ++i) {
            //            char ch = s.charAt(i);
            //            if (0xDC00 <= ch && ch <= 0xDFFF) {
            //                char ch0 = s.charAt(i-1);
            //                if (0xD800 <= ch && ch <= 0xDbFF) {
            //                    --result;
            //                }
            //            }
            //        }
        }

        /// <summary>
        /// Utility function for comparing codepoint to string without generating new
        /// string.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(int codepoint, string other)
        {
            if (other is null)
            {
                return false;
            }
            switch (other.Length)
            {
                case 1: return codepoint == other[0];
                case 2: return codepoint > 0xFFFF && codepoint == Character.CodePointAt(other, 0);
                default: return false;
            }
        }

        /// <summary>
        /// Utility function for comparing codepoint to string without generating new
        /// string.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(int codepoint, ReadOnlySpan<char> other)
        {
            switch (other.Length)
            {
                case 1: return codepoint == other[0];
                case 2: return codepoint > 0xFFFF && codepoint == Character.CodePointAt(other, 0);
                default: return false;
            }
        }

        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(string other, int codepoint)
        {
            return Equals(codepoint, other);
        }

        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(ReadOnlySpan<char> other, int codepoint)
        {
            return Equals(codepoint, other);
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(string? str, int codePoint)
        {
            if (codePoint < Character.MinCodePoint || codePoint > Character.MaxCodePoint)
            {
                throw new ArgumentException();
            }
            if (string.IsNullOrEmpty(str))
            {
                return -1;
            }
            int stringLength = str!.Length;
            char firstChar = str[0];
            int offset = codePoint - Character.MinSupplementaryCodePoint;

            if (offset < 0)
            { // BMP codePoint
                int result2 = firstChar - codePoint;
                if (result2 != 0)
                {
                    return result2;
                }
                return stringLength - 1;
            }
            // non BMP
            char lead = (char)((offset.TripleShift(10)) + Character.MinHighSurrogate);
            int result = firstChar - lead;
            if (result != 0)
            {
                return result;
            }
            if (stringLength > 1)
            {
                char trail = (char)((offset & 0x3ff) + Character.MinLowSurrogate);
                result = str[1] - trail;
                if (result != 0)
                {
                    return result;
                }
            }
            return stringLength - 2;
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string (with the [ugly] new StringBuilder().AppendCodePoint(codepoint).ToString())
        /// and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(ReadOnlySpan<char> str, int codePoint)
        {
            if (codePoint < Character.MinCodePoint || codePoint > Character.MaxCodePoint)
            {
                throw new ArgumentException();
            }
            int stringLength = str.Length;
            if (stringLength == 0)
            {
                return -1;
            }
            char firstChar = str[0];
            int offset = codePoint - Character.MinSupplementaryCodePoint;

            if (offset < 0)
            { // BMP codePoint
                int result2 = firstChar - codePoint;
                if (result2 != 0)
                {
                    return result2;
                }
                return stringLength - 1;
            }
            // non BMP
            char lead = (char)((offset.TripleShift(10)) + Character.MinHighSurrogate);
            int result = firstChar - lead;
            if (result != 0)
            {
                return result;
            }
            if (stringLength > 1)
            {
                char trail = (char)((offset & 0x3ff) + Character.MinLowSurrogate);
                result = str[1] - trail;
                if (result != 0)
                {
                    return result;
                }
            }
            return stringLength - 2;
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(int codepoint, string? a)
        {
            int result = Compare(a, codepoint);
            return result > 0 ? -1 : result < 0 ? 1 : 0; // Reverse the order.
        }

        /// <summary>
        /// Utility to compare a string to a code point.
        /// Same results as turning the code point into a string and comparing, but much faster (no object creation).
        /// Actually, there is one difference; a null compares as less.
        /// Note that this (=String) order is UTF-16 order -- *not* code point order.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(int codepoint, ReadOnlySpan<char> a)
        {
            int result = Compare(a, codepoint);
            return result > 0 ? -1 : result < 0 ? 1 : 0; // Reverse the order.
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(string? s)
        {
            if (s is null) return int.MaxValue;
            int length = s.Length;
            if (length < 1 || length > 2)
            {
                return int.MaxValue;
            }
            int result = Character.CodePointAt(s, 0);
            return (result < 0x10000) == (length == 1) ? result : int.MaxValue;
        }

        /// <summary>
        /// Return the value of the first code point, if the string is exactly one code point. Otherwise return <see cref="int.MaxValue"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(ReadOnlySpan<char> s)
        {
            int length = s.Length;
            if (length < 1 || length > 2)
            {
                return int.MaxValue;
            }
            int result = Character.CodePointAt(s, 0);
            return (result < 0x10000) == (length == 1) ? result : int.MaxValue;
        }

        /// <summary>
        /// Utility function for comparing objects that may be null
        /// string.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals<T>(T? a, T? b) where T : class
        {
            return a == null ? b == null
                    : b == null ? false
                            : a.Equals(b);
        }

        /// <summary>
        /// Utility for comparing the contents of character sequences.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(string? a, string? b)
        {
            if (ReferenceEquals(a, b))
            {
                return 0;
            }
            if (a is null) return -1;
            if (b is null) return 1;

            int alength = a.Length;
            int blength = b.Length;
            int min = alength <= blength ? alength : blength;
            for (int i = 0; i < min; ++i)
            {
                int diff = a[i] - b[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return alength - blength;
        }


        /// <summary>
        /// Utility for comparing the contents of character sequences.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(string? a, ReadOnlySpan<char> b)
        {
            if (a is null) return -1;

            int alength = a.Length;
            int blength = b.Length;
            int min = alength <= blength ? alength : blength;
            for (int i = 0; i < min; ++i)
            {
                int diff = a[i] - b[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return alength - blength;
        }


        /// <summary>
        /// Utility for comparing the contents of character sequences.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(ReadOnlySpan<char> a, string? b)
        {
            if (b is null) return 1;

            int alength = a.Length;
            int blength = b.Length;
            int min = alength <= blength ? alength : blength;
            for (int i = 0; i < min; ++i)
            {
                int diff = a[i] - b[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return alength - blength;
        }


        /// <summary>
        /// Utility for comparing the contents of character sequences.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        {
            int alength = a.Length;
            int blength = b.Length;
            int min = alength <= blength ? alength : blength;
            for (int i = 0; i < min; ++i)
            {
                int diff = a[i] - b[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return alength - blength;
        }

        // ICU4N: Excluded EqualsChars() because it is redundant functionality with MemoryExtensions.Equals() or string.Equals() with StringComparison.Ordinal specified

        /// <summary>
        /// Are we on a character boundary?
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool OnCharacterBoundary(string s, int i)
        {
            if (s is null) return false;

            return i <= 0
                || i >= s.Length
                || !char.IsHighSurrogate(s[i - 1])
                || !char.IsLowSurrogate(s[i]);
        }

        /// <summary>
        /// Are we on a character boundary?
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool OnCharacterBoundary(ReadOnlySpan<char> s, int i)
        {
            return i <= 0
                || i >= s.Length
                || !char.IsHighSurrogate(s[i - 1])
                || !char.IsLowSurrogate(s[i]);
        }

        // ICU4N: Excluded IndexOf(CharSequence s, int codePoint) because it is a duplicate of J2N.Text.StringExtensions.IndexOf() and J2N.MemoryExtensions.IndexOf()

        /// <summary>
        /// Utility function for simplified, more robust loops, such as:
        /// <code>
        ///     foreach (int codePoint in CharSequences.CodePoints(str, stackalloc char[str.Length]))
        ///     {
        ///         DoSomethingWith(codePoint);
        ///     }
        /// </code>
        /// Note that the buffer length should be the same as the length of <paramref name="s"/>. The buffer can
        /// be allocated on the stack or using the array pool for reuse.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static ReadOnlySpan<int> CodePoints(string s, Span<int> buffer)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return CodePoints(s.AsSpan(), buffer);
        }

        /// <summary>
        /// Utility function for simplified, more robust loops, such as:
        /// <code>
        ///     foreach (int codePoint in CharSequences.CodePoints(str, stackalloc char[str.Length]))
        ///     {
        ///         DoSomethingWith(codePoint);
        ///     }
        /// </code>
        /// Note that the buffer length should be the same as the length of <paramref name="s"/>. The buffer can
        /// be allocated on the stack or using the array pool for reuse.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static ReadOnlySpan<int> CodePoints(ReadOnlySpan<char> s, Span<int> buffer) // in the vast majority of cases, the buffer length is the same
        {
            int j = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                char cp = s[i];
                if (cp >= 0xDC00 && cp <= 0xDFFF && i != 0)
                { // hand-code for speed
                    char last = (char)buffer[j - 1];
                    if (last >= 0xD800 && last <= 0xDBFF)
                    {
                        // Note: j-1 is safe, because j can only be zero if i is zero. But i!=0 in this block.
                        buffer[j - 1] = Character.ToCodePoint(last, cp);
                        continue;
                    }
                }
                buffer[j++] = cp;
            }
            return buffer.Slice(0, j);
        }

        // ICU4N: Removed constructor because the class is static in .NET
    }
}
