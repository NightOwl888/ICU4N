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
    }
}
