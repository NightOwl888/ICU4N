using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Lang
{
    [Obsolete("This API is ICU internal only.")]
    internal class CharSequences // ICU4N specific - marked internal rather than public because of ICharSequence
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

        /**
         * Find the longest n such that a[aIndex,n] = b[bIndex,n], and n is on a character boundary.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int MatchAfter(ICharSequence a, ICharSequence b, int aIndex, int bIndex)
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

        /**
         * Count the code point length. Unpaired surrogates count as 1.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public int CodePointLength(ICharSequence s)
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

        /**
         * Utility function for comparing codepoint to string without generating new
         * string.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(int codepoint, ICharSequence other)
        {
            if (other == null)
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

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals(ICharSequence other, int codepoint)
        {
            return Equals(codepoint, other);
        }

        /**
         * Utility to compare a string to a code point.
         * Same results as turning the code point into a string (with the [ugly] new StringBuilder().appendCodePoint(codepoint).toString())
         * and comparing, but much faster (no object creation). 
         * Actually, there is one difference; a null compares as less.
         * Note that this (=String) order is UTF-16 order -- *not* code point order.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(ICharSequence str, int codePoint)
        {
            if (codePoint < Character.MIN_CODE_POINT || codePoint > Character.MAX_CODE_POINT)
            {
                throw new ArgumentException();
            }
            int stringLength = str.Length;
            if (stringLength == 0)
            {
                return -1;
            }
            char firstChar = str[0];
            int offset = codePoint - Character.MIN_SUPPLEMENTARY_CODE_POINT;

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
            char lead = (char)((int)((uint)offset >> 10) + Character.MIN_HIGH_SURROGATE);
            int result = firstChar - lead;
            if (result != 0)
            {
                return result;
            }
            if (stringLength > 1)
            {
                char trail = (char)((offset & 0x3ff) + Character.MIN_LOW_SURROGATE);
                result = str[1] - trail;
                if (result != 0)
                {
                    return result;
                }
            }
            return stringLength - 2;
        }

        /**
         * Utility to compare a string to a code point.
         * Same results as turning the code point into a string and comparing, but much faster (no object creation). 
         * Actually, there is one difference; a null compares as less.
         * Note that this (=String) order is UTF-16 order -- *not* code point order.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(int codepoint, ICharSequence a)
        {
            int result = Compare(a, codepoint);
            return result > 0 ? -1 : result < 0 ? 1 : 0; // Reverse the order.
        }

        /**
         * Return the value of the first code point, if the string is exactly one code point. Otherwise return Integer.MAX_VALUE.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int GetSingleCodePoint(ICharSequence s)
        {
            int length = s.Length;
            if (length < 1 || length > 2)
            {
                return int.MaxValue;
            }
            int result = Character.CodePointAt(s, 0);
            return (result < 0x10000) == (length == 1) ? result : int.MaxValue;
        }

        /**
         * Utility function for comparing objects that may be null
         * string.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals<T>(T a, T b) where T : class
        {
            return a == null ? b == null
                    : b == null ? false
                            : a.Equals(b);
        }

        /**
         * Utility for comparing the contents of CharSequences
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int Compare(ICharSequence a, ICharSequence b)
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

        /**
         * Utility for comparing the contents of CharSequences
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static bool EqualsChars(ICharSequence a, ICharSequence b)
        {
            // do length test first for fast path
            return a.Length == b.Length && Compare(a, b) == 0;
        }

        /**
         * Are we on a character boundary?
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static bool OnCharacterBoundary(ICharSequence s, int i)
        {
            return i <= 0
            || i >= s.Length
            || !char.IsHighSurrogate(s[i - 1])
            || !char.IsLowSurrogate(s[i]);
        }

        /**
         * Find code point in string.
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int indexOf(ICharSequence s, int codePoint)
        {
            int cp;
            for (int i = 0; i < s.Length; i += Character.CharCount(cp))
            {
                cp = Character.CodePointAt(s, i);
                if (cp == codePoint)
                {
                    return i;
                }
            }
            return -1;
        }

        /**
         * Utility function for simplified, more robust loops, such as:
         * <pre>
         *   for (int codePoint : CharSequences.codePoints(string)) {
         *     doSomethingWith(codePoint);
         *   }
         * </pre>
         * 
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static int[] CodePoints(ICharSequence s)
        {
            int[] result = new int[s.Length]; // in the vast majority of cases, the length is the same
            int j = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                char cp = s[i];
                if (cp >= 0xDC00 && cp <= 0xDFFF && i != 0)
                { // hand-code for speed
                    char last = (char)result[j - 1];
                    if (last >= 0xD800 && last <= 0xDBFF)
                    {
                        // Note: j-1 is safe, because j can only be zero if i is zero. But i!=0 in this block.
                        result[j - 1] = Character.ToCodePoint(last, cp);
                        continue;
                    }
                }
                result[j++] = cp;
            }
            if (j == result.Length)
            {
                return result;
            }
            int[] shortResult = new int[j];
            System.Array.Copy(result, 0, shortResult, 0, j);
            return shortResult;
        }

        private CharSequences()
        {
        }
    }
}
