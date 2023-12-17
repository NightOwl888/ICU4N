using J2N.Text;
using System;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// Extensions to <see cref="IAppendable"/>.
    /// </summary>
    internal static class AppendableExtensions
    {
#if FEATURE_SPAN
        /// <summary>
        /// Appends the given <see cref="ReadOnlySpan{Char}"/> to this <see cref="IAppendable"/>.
        /// </summary>
        /// <param name="text">This <see cref="IAppendable"/>.</param>
        /// <param name="charSequence">The <see cref="ReadOnlySpan{Char}"/> to append.</param>
        /// <returns>This <see cref="IAppendable"/>, for chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> or its underlying value is <c>null</c>.</exception>
        public static IAppendable Append(this IAppendable text, ReadOnlySpan<char> charSequence) // ICU4N TODO: Move to J2N...or perhaps add to IAppendable interface
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (text is StringBuilderCharSequence stringBuilder)
            {
                if (stringBuilder.Value is null)
                    throw new ArgumentNullException(nameof(text));

                stringBuilder.Value.Append(charSequence);
            }
            else
            {
                // ICU4N NOTE: No way to ensure capacity, so it may
                // be more efficient to allocate up front here in some cases.
                foreach (var ch in charSequence)
                {
                    text.Append(ch);
                }
            }
            return text;
        }
#endif

    }
}
