using J2N.Text;
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
    }
}
