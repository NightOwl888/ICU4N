using J2N;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// A comparer that orders <see cref="string"/> objects in the same way as
    /// Java's <c>String.CASE_INSENSITIVE_COMPARATOR</c> parameter. This 
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class CaseInsensitiveComparer : IComparer<string>
    {
        public static CaseInsensitiveComparer Default { get; } = new CaseInsensitiveComparer();

        public int Compare(string x, string y)
        {
            if (x is null)
                return (y is null) ? 0 : 1;
            if (y is null)
                return -1;

            int result;
            int xl = x.Length;
            int yl = y.Length;
            int end = Math.Min(xl, yl);
            int c1, c2;
            for (int i = 0; i < end; i++)
            {
                if ((c1 = x[i]) == (c2 = y[i]))
                    continue;
                c1 = CompareValue(c1);
                c2 = CompareValue(c2);
                if ((result = c1 - c2) != 0)
                {
                    return result;
                }
            }
            return xl - yl;
        }

        // Optimized for ASCII
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareValue(int ch)
        {
            if (ch < 128)
            {
                if ('A' <= ch && ch <= 'Z')
                {
                    return (ch + ('a' - 'A'));
                }
                return ch;
            }
            return Character.ToLower(Character.ToUpper(ch, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
    }
}
