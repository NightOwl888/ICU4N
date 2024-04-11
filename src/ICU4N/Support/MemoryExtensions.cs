using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable

namespace ICU4N
{
    internal static class MemoryExtensions // ICU4N TODO: Clean up
    {
#if FEATURE_SPAN

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

#endif
    }
}
