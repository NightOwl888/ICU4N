using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable

namespace ICU4N
{
    /// <summary>
    /// Extensions to <see cref="ReadOnlySpan{T}"/>, <see cref="Span{T}"/>,
    /// <see cref="ReadOnlyMemory{T}"/>, and <see cref="Memory{T}"/>.
    /// </summary>
    internal static class MemoryExtensions
    {
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            return System.MemoryExtensions.IndexOf(span, value.AsSpan(), comparisonType);
        }


        /// <summary>
        /// Determines whether this <paramref name="span"/> and the specified <paramref name="other"/> span have the same characters
        /// when compared using the specified <paramref name="comparisonType"/> option.
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// </summary>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Equals(this ReadOnlySpan<char> span, string other, StringComparison comparisonType)
        {
            return System.MemoryExtensions.Equals(span, other.AsSpan(), comparisonType);
        }

        /// <summary>
        /// Determines whether the beginning of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the beginning of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool StartsWith(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            return System.MemoryExtensions.StartsWith(span, value.AsSpan(), comparisonType);
        }

        /// <summary>
        /// Determines whether the end of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool EndsWith(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            return System.MemoryExtensions.EndsWith(span, value.AsSpan(), comparisonType);
        }
#endif

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf(this ReadOnlySpan<char> span, string value, int startIndex, StringComparison comparisonType)
        {
            return IndexOf(span, value.AsSpan(), startIndex, comparisonType);
        }
#endif
        public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, int startIndex, StringComparison comparisonType)
        {
            ReadOnlySpan<char> slice = span.Slice(startIndex);
            int pos = System.MemoryExtensions.IndexOf(slice, value, comparisonType);
            return pos < 0 ? -1 : pos + startIndex;
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf(this ReadOnlySpan<char> span, string value, int startIndex, int count, StringComparison comparisonType)
        {
            return IndexOf(span, value.AsSpan(), startIndex, count, comparisonType);
        }
#endif

        public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, int startIndex, int count, StringComparison comparisonType)
        {
            ReadOnlySpan<char> slice = span.Slice(startIndex, count);
            int pos = System.MemoryExtensions.IndexOf(slice, value, comparisonType);
            return pos < 0 ? -1 : pos + startIndex;
        }

        public static int IndexOf(this ReadOnlySpan<char> span, char value, int startIndex)
        {
            ReadOnlySpan<char> slice = span.Slice(startIndex);
            int pos = System.MemoryExtensions.IndexOf(slice, value);
            return pos < 0 ? -1 : pos + startIndex;
        }

#if !FEATURE_MEMORYEXTENSIONS_LASTINDEXOF_COMPARISONTYPE

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static int LastIndexOf(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal)
            {
                return span.LastIndexOf(value.AsSpan());
            }

            // Hack for platforms older than .NET Core, since this overload didn't exist.
            // ICU4N TODO: Optimize (this is rarely used)
            return span.ToString().LastIndexOf(value, comparisonType);
        }

#endif

        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static int LastIndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal)
            {
                return span.LastIndexOf(value);
            }

            // Hack for platforms older than .NET Core, since this overload didn't exist.
            // ICU4N TODO: Optimize (this is rarely used)
            return span.ToString().LastIndexOf(value.ToString(), comparisonType);
        }

        private static void CheckStringComparison(StringComparison comparisonType)
        {
            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
                throw new ArgumentOutOfRangeException(nameof(comparisonType));
        }

#endif

        // ICU4N TODO: Move to J2N...?
        /// <summary>
        /// Compares the specified <see cref="ReadOnlySpan{Char}"/> to this string and compares the specified
        /// range of characters to determine if they are the same.
        /// </summary>
        /// <param name="text">This string.</param>
        /// <param name="thisStartIndex">The starting offset in this string.</param>
        /// <param name="other">The <see cref="ReadOnlySpan{Char}"/> to compare.</param>
        /// <param name="otherStartIndex">The starting offset in the specified string.</param>
        /// <param name="length">The number of characters to compare.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns><c>true</c> if the ranges of characters are equal, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparisonType"/> is not a <see cref="StringComparison"/> value.</exception>
        public static bool RegionMatches(this ReadOnlySpan<char> text, int thisStartIndex, ReadOnlySpan<char> other, int otherStartIndex, int length, StringComparison comparisonType) // KEEP OVERLOADS FOR ReadOnlySpan<char>, ICharSequence, char[], StringBuilder, and string IN SYNC
        {
            if (other.Length - otherStartIndex < length || otherStartIndex < 0)
                return false;
            if (thisStartIndex < 0 || text.Length - thisStartIndex < length)
                return false;
            if (length <= 0)
                return true;
            
            return text.Slice(thisStartIndex, length).Equals(other.Slice(otherStartIndex, length), comparisonType);
        }

        /// <summary>
        /// Sets the supplied <paramref name="reference"/> to the underlying <see cref="string"/> or <see cref="T:char[]"/>
        /// of this <see cref="ReadOnlyMemory{Char}"/>. This allows use of <see cref="ReadOnlyMemory{Char}"/> as a field
        /// of a struct or class without having the underlying <see cref="string"/> or <see cref="T:char[]"/> go out of scope.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlyMemory{Char}"/>.</param>
        /// <param name="reference">When this method returns successfully, the refernce will be set by ref to the
        /// underlying <see cref="string"/> or <see cref="T:char[]"/> of <paramref name="text"/>.</param>
        /// <returns><c>true</c> if the underlying reference could be retrieved; otherwise, <c>false</c>.
        /// Note that if the underlying memory is not a <see cref="string"/> or <see cref="T:char[]"/>,
        /// this method will always return <c>false</c>.</returns>
        public static bool TryGetReference(this ReadOnlyMemory<char> text, [MaybeNullWhen(false)] ref object? reference)
        {
            if (MemoryMarshal.TryGetString(text, out string? stringValue, out _, out _) && stringValue is not null)
            {
                reference = stringValue;
                return true;
            }
            else if (MemoryMarshal.TryGetArray(text, out ArraySegment<char> arraySegment) && arraySegment.Array is not null)
            {
                reference = arraySegment.Array;
                return true;
            }
            return false;
        }
    }
}
