using J2N;
using System;

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

        // ICU4N specfic - moved all methods to CharSequences.generated.tt

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


        /// <summary>
        /// Utility function for comparing objects that may be null
        /// string.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals<T>(T a, T b) where T : class
        {
            return a == null ? b == null
                    : b == null ? false
                            : a.Equals(b);
        }

        // ICU4N: Removed constructor because the class is static in .NET
    }
}
