using System;
using System.Runtime.CompilerServices;

namespace ICU4N.Support.Text
{
    internal static class StringExtensions
    {
        public static int CompareToOrdinalIgnoreCase(this string str, string value)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(str, value);
        }

#if FEATURE_SPAN
        /// <summary>Copies the contents of this string into the destination span.</summary>
        /// <param name="s">This string.</param>
        /// <param name="destination">The span into which to copy this string's contents.</param>
        /// <exception cref="ArgumentException">If <paramref name="destination"/> is too short.</exception>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void CopyTo(this string s, Span<char> destination) // ICU4N TODO: Move to J2N?
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            if ((uint)s.Length <= (uint)destination.Length)
            {
                s.AsSpan().CopyTo(destination);
            }
            else
            {
                throw new ArgumentException("Destination is too short.", nameof(destination)); // Argument_DestinationTooShort
            }
        }

        /// <summary>Copies the contents of this string into the destination span.</summary>
        /// <param name="s">This string.</param>
        /// <param name="destination">The span into which to copy this string's contents.</param>
        /// <returns>true if the data was copied; false if the destination was too short to fit the contents of the string.</returns>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryCopyTo(this string s, Span<char> destination) // ICU4N TODO: Move to J2N?
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            bool retVal = false;
            if ((uint)s.Length <= (uint)destination.Length)
            {
                retVal = s.AsSpan(0, s.Length).TryCopyTo(destination);
            }
            return retVal;
        }
#endif
    }
}
