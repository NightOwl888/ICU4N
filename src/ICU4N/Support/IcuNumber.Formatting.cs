using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Support.Text;
using J2N.Numerics;
using System;
using System.Diagnostics;
using System.Globalization;
#nullable enable

namespace ICU4N
{
    internal static partial class IcuNumber
    {
        private const int CharStackBufferSize = 32;


        ///// Parses the given pattern string and overwrites the settings specified in the pattern string.
        ///// The properties corresponding to the following setters are overwritten, either with their
        ///// default values or with the value specified in the pattern string:
        ///// 
        ///// <list type="bullet">
        /////     <item><description><see cref="DecimalSeparatorAlwaysShown"/></description></item>
        /////     <item><description><see cref="ExponentSignAlwaysShown"/></description></item>
        /////     <item><description><see cref="FormatWidth"/></description></item>
        /////     <item><description><see cref="GroupingSize"/></description></item>
        /////     <item><description><see cref="Multiplier"/>  (percent/permille)</description></item>
        /////     <item><description><see cref="MaximumFractionDigits"/></description></item>
        /////     <item><description><see cref="MaximumIntegerDigits"/></description></item>
        /////     <item><description><see cref="MaximumSignificantDigits"/></description></item>
        /////     <item><description><see cref="MinimumExponentDigits"/></description></item>
        /////     <item><description><see cref="MinimumFractionDigits"/></description></item>
        /////     <item><description><see cref="MinimumIntegerDigits"/></description></item>
        /////     <item><description><see cref="MinimumSignificantDigits"/></description></item>
        /////     <item><description><see cref="PadPosition"/></description></item>
        /////     <item><description><see cref="PadCharacter"/></description></item>
        /////     <item><description><see cref="RoundingIncrement"/></description></item>
        /////     <item><description><see cref="SecondaryGroupingSize"/></description></item>
        ///// </list>
        ///// All other settings remain untouched.

        // ICU4N TODO: This is a temporary workaround. This corresponds with GroupingSize/SecondaryGroupingSize.
        // We ought to be able to set all of the above properties on UNumberFormatInfo.
        // But, in .NET we need to convert these to a pattern string for the formatter to understand them (when possible).
        internal static int[] GetGroupingSizes(string pattern)
        {
            // This is how the group sizes are determined in ICU - need to deconstruct.
            PatternStringParser.ParsedPatternInfo patternInfo = PatternStringParser.ParseToPatternInfo(pattern);
            PatternStringParser.ParsedSubpatternInfo positive = patternInfo.positive;
            // Grouping settings
            short grouping1 = (short)(positive.groupingSizes & 0xffff);
            short grouping2 = (short)((positive.groupingSizes.TripleShift(16)) & 0xffff);
            short grouping3 = (short)((positive.groupingSizes.TripleShift(32)) & 0xffff);

            int groupingSize = grouping1 < 0 ? 0 : grouping1;
            int secondaryGroupingSize = grouping3 != -1 ? (grouping2 < 0 ? 0 : grouping2) : 0;
            if (groupingSize == 0 || secondaryGroupingSize == 0)
                return new int[] { groupingSize };

            return new int[] { groupingSize, secondaryGroupingSize };
        }

#if FEATURE_SPAN
        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe string? FormatDouble(ref ValueStringBuilder sb, double value, ReadOnlySpan<char> format, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            if (!double.IsFinite(value))
            {
                if (double.IsNaN(value))
                {
                    return info.NaNSymbol;
                }

                return double.IsNegative(value) ? string.Concat(info.NegativeSign, info.PositiveInfinitySymbol) /*info.NegativeInfinitySymbol*/ : info.PositiveInfinitySymbol;
            }

            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            var nfi = ToNumberFormatInfo(info);
            if (value.TryFormat(tempFormatted, out int charsWrittenTemp, format, nfi))
            {
                AppendConvertedDigits(ref sb, new ReadOnlySpan<char>(pTempFormatted, charsWrittenTemp), info);
            }
            else
            {
                // NOTE: TryFormat above should have already thrown if any of the parameters are invalid,
                // so we don't try/catch here.

                // We didn't have enough buffer on the stack. Do it the slow way.
                string temp = value.ToString(new string(format), nfi);
                AppendConvertedDigits(ref sb, temp, info);
            }
            return null;
        }

        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe string? FormatInt64(ref ValueStringBuilder sb, long value, ReadOnlySpan<char> format, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            var nfi = ToNumberFormatInfo(info);
            if (value.TryFormat(tempFormatted, out int charsWrittenTemp, format, nfi))
            {
                AppendConvertedDigits(ref sb, new ReadOnlySpan<char>(pTempFormatted, charsWrittenTemp), info);
            }
            else
            {
                // NOTE: TryFormat above should have already thrown if any of the parameters are invalid,
                // so we don't try/catch here.

                // We didn't have enough buffer on the stack. Do it the slow way.
                string temp = value.ToString(new string(format), nfi);
                AppendConvertedDigits(ref sb, temp, info);
            }
            return null;
        }

        private static void AppendConvertedDigits(ref ValueStringBuilder sb, ReadOnlySpan<char> formatted, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            if (info.DigitSubstitution == UDigitShapes.None)
            {
                sb.Append(formatted);
                return;
            }

            string[] digits = info.NativeDigits; // Clones array locally

            // .NET formatters always return ASCII digits, so if they are
            // specified, we can do a single operation.
            if (AreAsciiDigits(digits))
            {
                sb.Append(formatted);
                return;
            }

            foreach (char ch in formatted)
            {
                if (IsAsciiDigit(ch))
                {
                    sb.Append(digits[ch - 48]);
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }

        private static NumberFormatInfo ToNumberFormatInfo(UNumberFormatInfo info)
        {
            var nfi = new NumberFormatInfo();

            //nfi.NativeDigits = info.NativeDigitsLocal; // No effect in .NET
            //nfi.DigitSubstitution = (DigitShapes)info.DigitSubstitution; // No effect in .NET
            nfi.NumberGroupSeparator = info.NumberGroupSeparator;
            nfi.NumberDecimalSeparator = info.NumberDecimalSeparator;
            nfi.NumberGroupSizes = info.NumberGroupSizesLocal;

            nfi.NegativeSign = info.NegativeSign;
            nfi.PositiveSign = info.PositiveSign;

            // These were optimized out in FormatDecimal()
            //nfi.NaNSymbol = info.NaN;
            //nfi.PositiveInfinitySymbol = info.Infinity;
            //nfi.NegativeInfinitySymbol = nfi.NegativeSign + nfi.Infinity;
            return nfi;
        }

        public static string FormatInt64(long value, string? format, UNumberFormatInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            return FormatInt64(ref sb, value, format, info) ?? sb.ToString();
        }

        public static bool TryFormatInt64(long value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            string? s = FormatInt64(ref sb, value, format, info);
            return s != null ?
                TryCopyTo(s, destination, out charsWritten) :
                sb.TryCopyTo(destination, out charsWritten);
        }

        public static string FormatDouble(double value, string? format, UNumberFormatInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            return FormatDouble(ref sb, value, format, info) ?? sb.ToString();
        }

        public static bool TryFormatDouble(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            string? s = FormatDouble(ref sb, value, format, info);
            return s != null ?
                TryCopyTo(s, destination, out charsWritten) :
                sb.TryCopyTo(destination, out charsWritten);
        }


        private static bool TryCopyTo(string source, Span<char> destination, out int charsWritten)
        {
            Debug.Assert(source != null);

            if (source.AsSpan().TryCopyTo(destination))
            {
                charsWritten = source.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }
#endif

        private static bool AreAsciiDigits(string[] digits)
        {
            if (digits.Length != 10)
                return false;
            return digits[0] == "0" && digits[1] == "1" && digits[2] == "2" && digits[3] == "3" && digits[4] == "4" &&
                digits[5] == "5" && digits[6] == "6" && digits[7] == "7" && digits[8] == "8" && digits[9] == "9";
        }

        /// <summary>Indicates whether a character is categorized as an ASCII digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>true if <paramref name="c"/> is an ASCII digit; otherwise, false.</returns>
        /// <remarks>
        /// This determines whether the character is in the range '0' through '9', inclusive.
        /// </remarks>
        private static bool IsAsciiDigit(char c) => IsBetween(c, '0', '9');

        /// <summary>Indicates whether a character is within the specified inclusive range.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <param name="minInclusive">The lower bound, inclusive.</param>
        /// <param name="maxInclusive">The upper bound, inclusive.</param>
        /// <returns>true if <paramref name="c"/> is within the specified range; otherwise, false.</returns>
        /// <remarks>
        /// The method does not validate that <paramref name="maxInclusive"/> is greater than or equal
        /// to <paramref name="minInclusive"/>.  If <paramref name="maxInclusive"/> is less than
        /// <paramref name="minInclusive"/>, the behavior is undefined.
        /// </remarks>
        private static bool IsBetween(char c, char minInclusive, char maxInclusive) =>
            (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);
    }
}
