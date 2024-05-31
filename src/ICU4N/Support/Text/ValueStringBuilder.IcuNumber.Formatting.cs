using ICU4N.Globalization;
using ICU4N.Text;
using System;
using System.Runtime.CompilerServices;
#nullable enable

namespace ICU4N.Text
{
    // ICU4N TODO: Fix the below logic after updating IcuNumber to correctly return charsLength instead of charsWritten,
    // since that will tell us exactly how much we need allocated to succeed and we will not have to resort to
    // a heap allocation on the retry.
    internal ref partial struct ValueStringBuilder
    {
        private const int CharStackBufferSize = 32;

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void AppendFormat(long value, string? format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
            => AppendFormat(value, format.AsSpan(), info, numberGroupSizesOverride);
#endif

        internal void AppendFormat(long value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            if (IcuNumber.TryFormatInt64(value, format, info, _chars.Slice(_pos), out int charsWritten, numberGroupSizesOverride))
            {
                _pos += charsWritten;
                UpdateMaxLength();
            }
            else
            {
                Span<char> buffer = stackalloc char[CharStackBufferSize];
                if (IcuNumber.TryFormatInt64(value, format, info, buffer, out charsWritten, numberGroupSizesOverride))
                {
                    int pos = _pos;
                    if (pos > _chars.Length - charsWritten)
                    {
                        Grow(charsWritten);
                    }

                    buffer.CopyTo(_chars.Slice(_pos));
                    _pos += charsWritten;
                    UpdateMaxLength();
                }
                else
                {
                    Append(IcuNumber.FormatInt64(value, format, info!, numberGroupSizesOverride));
                }
            }
        }


#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void AppendFormat(double value, string? format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
            => AppendFormat(value, format.AsSpan(), info, numberGroupSizesOverride);
#endif

        internal void AppendFormat(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            if (IcuNumber.TryFormatDouble(value, format, info!, _chars.Slice(_pos), out int charsWritten, numberGroupSizesOverride))
            {
                _pos += charsWritten;
                UpdateMaxLength();
            }
            else
            {
                Span<char> buffer = stackalloc char[CharStackBufferSize];
                if (IcuNumber.TryFormatDouble(value, format, info, buffer, out charsWritten, numberGroupSizesOverride))
                {
                    int pos = _pos;
                    if (pos > _chars.Length - charsWritten)
                    {
                        Grow(charsWritten);
                    }

                    buffer.CopyTo(_chars.Slice(_pos));
                    _pos += charsWritten;
                    UpdateMaxLength();
                }
                else
                {
                    Append(IcuNumber.FormatDouble(value, format, info, numberGroupSizesOverride));
                }
            }
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void InsertFormat(int index, long value, string? format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
            => InsertFormat(index, value, format.AsSpan(), info, numberGroupSizesOverride);
#endif

        internal void InsertFormat(int index, long value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            Span<char> buffer = stackalloc char[CharStackBufferSize];
            if (IcuNumber.TryFormatInt64(value, format, info!, buffer, out int charsWritten, numberGroupSizesOverride))
            {
                int pos = _pos;
                if (pos > _chars.Length - charsWritten)
                {
                    Grow(charsWritten);
                }

                int remaining = pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + charsWritten));
                buffer.Slice(0, charsWritten).CopyTo(_chars.Slice(index));
                _pos += charsWritten;
                UpdateMaxLength();
            }
            else
            {
                Insert(index, IcuNumber.FormatInt64(value, format, info, numberGroupSizesOverride));
            }
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void InsertFormat(int index, double value, string? format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
            => InsertFormat(index, value, format.AsSpan(), info, numberGroupSizesOverride);
#endif

        internal void InsertFormat(int index, double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            Span<char> buffer = stackalloc char[CharStackBufferSize];
            if (IcuNumber.TryFormatDouble(value, format, info!, buffer, out int charsWritten, numberGroupSizesOverride))
            {
                int pos = _pos;
                if (pos > _chars.Length - charsWritten)
                {
                    Grow(charsWritten);
                }

                int remaining = pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + charsWritten));
                buffer.Slice(0, charsWritten).CopyTo(_chars.Slice(index));
                _pos += charsWritten;
                UpdateMaxLength();
            }
            else
            {
                Insert(index, IcuNumber.FormatDouble(value, format, info, numberGroupSizesOverride));
            }
        }

        internal void InsertFormatPlural(int index, double value, string? format, MessagePattern? messagePattern, PluralType pluralType, UNumberFormatInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[IcuNumber.PluralCharStackBufferSize]);
            try
            {
                IcuNumber.FormatPlural(ref sb, value, format, messagePattern, pluralType, info);

                int pos = _pos;
                if (pos > _chars.Length - sb.Length)
                {
                    Grow(sb.Length);
                }

                int remaining = pos - index;
                _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + sb.Length));
                sb.AsSpan().CopyTo(_chars.Slice(index));
                _pos += sb.Length;
                UpdateMaxLength();
            }
            finally
            {
                sb.Dispose();
            }
        }
    }
}
