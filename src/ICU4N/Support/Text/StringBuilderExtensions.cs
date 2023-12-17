using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// Extensions to <see cref="StringBuilder"/>.
    /// </summary>
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Convenience method to wrap a <see cref="StringBuilder"/> in an
        /// <see cref="StringBuilderCharSequence"/> adapter class so it can be
        /// used with the <see cref="IAppendable"/> interface.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <returns>An <see cref="StringBuilderCharSequence"/>.</returns>
        internal static IAppendable AsAppendable(this StringBuilder text)
        {
            return new StringBuilderCharSequence(text);
        }

#if FEATURE_SPAN
        /// <summary>
        /// Appends the given <see cref="ReadOnlySpan{Char}"/> to this <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="charSequence">The <see cref="ReadOnlySpan{Char}"/> to append.</param>
        /// <returns>This <see cref="StringBuilder"/>, for chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <c>null</c>.</exception>
        public static StringBuilder Append(this StringBuilder text, ReadOnlySpan<char> charSequence) // ICU4N TODO: Move to J2N
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

#if FEATURE_STRINGBUILDER_APPEND_CHARPTR
            unsafe
            {
                fixed (char* seq = charSequence)
                {
                    text.Append(seq, charSequence.Length);
                }
            }
#else
            text.EnsureCapacity(text.Length + charSequence.Length);
            foreach (var ch in charSequence)
            {
                text.Append(ch);
            }
#endif
            return text;
        }
#endif
    }
}
