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
    internal static class MemoryExtensions // ICU4N TODO: Clean up
    {
#if !FEATURE_MEMORYEXTENSIONS_READONLYSPAN_EQUALS

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static int IndexOf(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal)
            {
                //return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
                return span.IndexOf(value.AsSpan());
            }

            throw new NotImplementedException();

            //switch (comparisonType)
            //{
            //    case StringComparison.CurrentCulture:
            //    case StringComparison.CurrentCultureIgnoreCase:
            //        return CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    case StringComparison.InvariantCulture:
            //    case StringComparison.InvariantCultureIgnoreCase:
            //        return CompareInfo.Invariant.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    default:
            //        Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
            //        return Ordinal.IndexOfOrdinalIgnoreCase(span, value);
            //}
        }


        /// <summary>
        /// Determines whether this <paramref name="span"/> and the specified <paramref name="other"/> span have the same characters
        /// when compared using the specified <paramref name="comparisonType"/> option.
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// </summary>
        public static bool Equals(this ReadOnlySpan<char> span, string other, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return EqualsOrdinal(span, other.AsSpan());

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return EqualsOrdinalIgnoreCase(span, other);
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the beginning of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the beginning of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool StartsWith(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    //return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    //return CompareInfo.Invariant.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return span.StartsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return span.StartsWithOrdinalIgnoreCase(value);
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the start of the span.
        /// </summary>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public unsafe static bool StartsWith<T>(this ReadOnlySpan<T> span, string value) where T : IEquatable<T>
        {
            int valueLength = value.Length;
            //if (RuntimeHelpers.IsBitwiseEquatable<T>())
            //{
            //    nuint size = (nuint)Unsafe.SizeOf<T>();
            //    return valueLength <= span.Length &&
            //    SpanHelpers.SequenceEqual(
            //        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)),
            //        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
            //        ((nuint)valueLength) * size);  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
            //}

            fixed (char* pValue = value)
                return valueLength <= span.Length && span.Slice(0, valueLength).SequenceEqual(new ReadOnlySpan<T>(pValue, valueLength)); //SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), valueLength);
        }

                /// <summary>
        /// Determines whether the end of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool EndsWith(this ReadOnlySpan<char> span, string value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    //return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    //return CompareInfo.Invariant.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return span.EndsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return span.EndsWithOrdinalIgnoreCase(value);
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the span.
        /// </summary>
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public unsafe static bool EndsWith<T>(this ReadOnlySpan<T> span, string value) where T : IEquatable<T>
        {
            int spanLength = span.Length;
            int valueLength = value.Length;
            //if (RuntimeHelpers.IsBitwiseEquatable<T>())
            //{
            //    nuint size = (nuint)Unsafe.SizeOf<T>();
            //    return valueLength <= spanLength &&
            //    SpanHelpers.SequenceEqual(
            //        ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), spanLength - valueLength)),
            //        ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)),
            //        ((nuint)valueLength) * size);  // If this multiplication overflows, the Span we got overflows the entire address range. There's no happy outcome for this api in such a case so we choose not to take the overhead of checking.
            //}

            fixed (char* pValue = value)
                return valueLength <= spanLength &&
                    span.Slice(spanLength - valueLength).SequenceEqual(new ReadOnlySpan<T>(pValue, valueLength));
            //SpanHelpers.SequenceEqual(
            //    ref Unsafe.Add(ref MemoryMarshal.GetReference(span), spanLength - valueLength),
            //    ref MemoryMarshal.GetReference(value),
            //    valueLength);
        }
#endif

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            if (comparisonType == StringComparison.Ordinal)
            {
                //return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
                return span.IndexOf(value);
            }

            throw new NotImplementedException();

            //switch (comparisonType)
            //{
            //    case StringComparison.CurrentCulture:
            //    case StringComparison.CurrentCultureIgnoreCase:
            //        return CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    case StringComparison.InvariantCulture:
            //    case StringComparison.InvariantCultureIgnoreCase:
            //        return CompareInfo.Invariant.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    default:
            //        Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
            //        return Ordinal.IndexOfOrdinalIgnoreCase(span, value);
            //}
        }

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
                //return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
                return span.LastIndexOf(value);
            }

            throw new NotImplementedException();

            //switch (comparisonType)
            //{
            //    case StringComparison.CurrentCulture:
            //    case StringComparison.CurrentCultureIgnoreCase:
            //        return CultureInfo.CurrentCulture.CompareInfo.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    case StringComparison.InvariantCulture:
            //    case StringComparison.InvariantCultureIgnoreCase:
            //        return CompareInfo.Invariant.IndexOf(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));

            //    default:
            //        Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
            //        return Ordinal.IndexOfOrdinalIgnoreCase(span, value);
            //}
        }

        /// <summary>
        /// Determines whether this <paramref name="span"/> and the specified <paramref name="other"/> span have the same characters
        /// when compared using the specified <paramref name="comparisonType"/> option.
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// </summary>
        public static bool Equals(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return EqualsOrdinal(span, other);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return EqualsOrdinalIgnoreCase(span, other);
                    throw new NotImplementedException();
            }
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool EqualsOrdinal(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
                return false;
            if (value.Length == 0)  // span.Length == value.Length == 0
                return true;
            return span.SequenceEqual(value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal static bool EqualsOrdinalIgnoreCase(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        //{
        //    if (span.Length != value.Length)
        //        return false;
        //    if (value.Length == 0)  // span.Length == value.Length == 0
        //        return true;
        //    return Ordinal.EqualsIgnoreCase(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), span.Length);
        //}

        /// <summary>
        /// Determines whether the beginning of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the beginning of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    //return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    //return CompareInfo.Invariant.IsPrefix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return span.StartsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return span.StartsWithOrdinalIgnoreCase(value);
                    throw new NotImplementedException();
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal static bool StartsWithOrdinalIgnoreCase(this ReadOnlySpan<char> span, ReadOnlySpan<char> value)
        //    => value.Length <= span.Length
        //    && Ordinal.EqualsIgnoreCase(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), value.Length);


        /// <summary>
        /// Determines whether the end of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            CheckStringComparison(comparisonType);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    //return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    //return CompareInfo.Invariant.IsSuffix(span, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
                    throw new NotImplementedException();

                case StringComparison.Ordinal:
                    return span.EndsWith(value);

                default:
                    Debug.Assert(comparisonType == StringComparison.OrdinalIgnoreCase);
                    //return span.EndsWithOrdinalIgnoreCase(value);
                    throw new NotImplementedException();
            }
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
