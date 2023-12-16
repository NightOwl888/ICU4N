using J2N.Text;
using System;
using System.Runtime.CompilerServices;
using System.Text;
#nullable enable

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        /// <summary>
        /// Compares a <see cref="ReadOnlySpan{Char}"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// <para/>
        /// This differs from <see cref="string.Equals(string, StringComparison)"/> in that it does not
        /// consider the <see cref="string"/> type to be part of the comparison - it will match for any character sequence
        /// that contains matching characters.
        /// <para/>
        /// The comparison is done using <see cref="StringComparison.Ordinal"/> comparison rules.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="charSequence">The character sequence to compare to.</param>
        /// <returns><c>true</c> if this <see cref="string"/> represents the same
        /// sequence of characters as the specified <paramref name="charSequence"/>; otherwise, <c>false</c>.</returns>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        public static bool ContentEquals(this string? text, ReadOnlySpan<char> charSequence) // ICU4N TODO: Move to J2N
        {
            if (text is null)
                return false;

            int len = charSequence.Length;
            if (len != text.Length)
                return false;
            if (len == 0 && text.Length == 0)
                return true; // since both are empty strings

            return charSequence.EqualsOrdinal(text.AsSpan());
        }

        /// <summary>
        /// Compares a <see cref="ReadOnlySpan{Char}"/> to this <see cref="string"/> to determine if
        /// their contents are equal.
        /// <para/>
        /// This differs from <see cref="string.Equals(string, StringComparison)"/> in that it does not
        /// consider the <see cref="string"/> type to be part of the comparison - it will match for any character sequence
        /// that contains matching characters.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="charSequence">The character sequence to compare to.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns><c>true</c> if this <see cref="string"/> represents the same
        /// sequence of characters as the specified <paramref name="charSequence"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="comparisonType"/> is not a <see cref="StringComparison"/> value.</exception>
        public static bool ContentEquals(this string? text, ReadOnlySpan<char> charSequence, StringComparison comparisonType) // ICU4N TODO: Move to J2N
        {
            if (text is null)
                return false;

            int len = charSequence.Length;
            if (len != text.Length)
                return false;
            if (len == 0 && text.Length == 0)
                return true; // since both are empty strings

            return charSequence.Equals(text.AsSpan(), comparisonType);
        }
#endif
    }
}
