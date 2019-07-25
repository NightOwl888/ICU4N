using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal static class CharArrayExtensions
    {
        /// <summary>
        /// Convenience method to wrap a string in a <see cref="CharArrayCharSequence"/>
        /// so a <see cref="T:char[]"/> can be used as <see cref="ICharSequence"/> in .NET.
        /// </summary>
        internal static ICharSequence ToCharSequence(this char[] text)
        {
            return new CharArrayCharSequence(text);
        }

        internal static ICharSequence SubSequence(this char[] text, int start, int end)
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

            int len = end - start;
            char[] result = new char[len];
            System.Array.Copy(text, start, result, 0, len);
            return result.ToCharSequence();
        }
    }
}
