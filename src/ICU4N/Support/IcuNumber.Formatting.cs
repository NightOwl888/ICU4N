using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Globalization;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static ICU4N.Text.PluralRules;
#nullable enable

namespace ICU4N
{
    internal static partial class IcuNumber
    {
        private const int CharStackBufferSize = 32; // General numbers
        public const int PluralCharStackBufferSize = 64; // Plural formatting
        public const int RuleBasedCharStackBufferSize = 128; // Rule based


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
            short grouping2 = (short)((positive.groupingSizes >>> 16) & 0xffff);
            short grouping3 = (short)((positive.groupingSizes >>> 32) & 0xffff);

            int groupingSize = grouping1 < 0 ? 0 : grouping1;
            int secondaryGroupingSize = grouping3 != -1 ? (grouping2 < 0 ? 0 : grouping2) : 0;
            if (groupingSize == 0 || secondaryGroupingSize == 0)
                return new int[] { groupingSize };

            return new int[] { groupingSize, secondaryGroupingSize };
        }

        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe void FormatBigInteger(ref ValueStringBuilder sb, System.Numerics.BigInteger value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            Debug.Assert(info != null);

            var nfi = ToNumberFormatInfo(info!, numberGroupSizesOverride);
#if FEATURE_SPANFORMATTABLE
            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            if (value.TryFormat(tempFormatted, out int charsWrittenTemp, format, nfi))
            {
                AppendConvertedDigits(ref sb, new ReadOnlySpan<char>(pTempFormatted, charsWrittenTemp), info);
            }
            else
#endif
            {

                // NOTE: TryFormat above should have already thrown if any of the parameters are invalid,
                // so we don't try/catch here.

                // We didn't have enough buffer on the stack. Do it the slow way.
                string temp = value.ToString(format.ToString(), nfi);
                AppendConvertedDigits(ref sb, temp.AsSpan(), info!);
            }
        }

#if FEATURE_INT128
        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe void FormatInt128(ref ValueStringBuilder sb, Int128 value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            Debug.Assert(info != null);

            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            var nfi = ToNumberFormatInfo(info, numberGroupSizesOverride);
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
        }

        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe void FormatUInt128(ref ValueStringBuilder sb, UInt128 value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            Debug.Assert(info != null);

            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            var nfi = ToNumberFormatInfo(info, numberGroupSizesOverride);
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
        }
#endif


        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe string? FormatDouble(ref ValueStringBuilder sb, double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            Debug.Assert(info != null);

            if (!value.IsFinite())
            {
                if (double.IsNaN(value))
                {
                    return info!.NaNSymbol;
                }

                return value.IsNegative() ? string.Concat(info!.NegativeSign, info.PositiveInfinitySymbol) /*info.NegativeInfinitySymbol*/ : info!.PositiveInfinitySymbol;
            }

            var nfi = ToNumberFormatInfo(info!, numberGroupSizesOverride);
            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            if (J2N.Numerics.Double.TryFormat(value, tempFormatted, out int charsWrittenTemp, format, nfi))
            {
                AppendConvertedDigits(ref sb, new ReadOnlySpan<char>(pTempFormatted, charsWrittenTemp), info!);
            }
            else
            {
                // NOTE: TryFormat above should have already thrown if any of the parameters are invalid,
                // so we don't try/catch here.

                // We didn't have enough buffer on the stack. Do it the slow way.
                string temp = value.ToString(format.ToString(), nfi);
                AppendConvertedDigits(ref sb, temp.AsSpan(), info!);
            }
            return null;
        }

        /// <summary>Formats the specified value according to the specified format and info.</summary>
        /// <returns>
        /// Non-null if an existing string can be returned, in which case the builder will be unmodified.
        /// Null if no existing string was returned, in which case the formatted output is in the builder.
        /// </returns>
        private static unsafe string? FormatInt64(ref ValueStringBuilder sb, long value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            Debug.Assert(info != null);

            var nfi = ToNumberFormatInfo(info!, numberGroupSizesOverride);
            char* pTempFormatted = stackalloc char[CharStackBufferSize];
            Span<char> tempFormatted = new Span<char>(pTempFormatted, CharStackBufferSize);
            if (J2N.Numerics.Int64.TryFormat(value, tempFormatted, out int charsWrittenTemp, format, nfi))
            {
                AppendConvertedDigits(ref sb, new ReadOnlySpan<char>(pTempFormatted, charsWrittenTemp), info!);
            }
            else
            {
                // NOTE: TryFormat above should have already thrown if any of the parameters are invalid,
                // so we don't try/catch here.

                // We didn't have enough buffer on the stack. Do it the slow way.
                string temp = value.ToString(format.ToString(), nfi);
                AppendConvertedDigits(ref sb, temp.AsSpan(), info!);
            }
            return null;
        }

        private static void AppendConvertedDigits(ref ValueStringBuilder sb, ReadOnlySpan<char> formatted, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            if (info!.DigitSubstitution == UDigitShapes.None)
            {
                sb.Append(formatted);
                return;
            }

            string[] digits = info.NativeDigitsLocal;

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

        private static NumberFormatInfo ToNumberFormatInfo(UNumberFormatInfo info, int[]? numberGroupSizesOverride)
        {
            var nfi = new NumberFormatInfo();

            //nfi.NativeDigits = info.NativeDigitsLocal; // No effect in .NET
            //nfi.DigitSubstitution = (DigitShapes)info.DigitSubstitution; // No effect in .NET
            nfi.NumberGroupSeparator = info.NumberGroupSeparator;
            nfi.NumberDecimalSeparator = info.NumberDecimalSeparator;
            nfi.NumberGroupSizes = numberGroupSizesOverride ?? info.NumberGroupSizesLocal;

            nfi.NegativeSign = info.NegativeSign;
            nfi.PositiveSign = info.PositiveSign;

            // These were optimized out in FormatDecimal()
            //nfi.NaNSymbol = info.NaN;
            //nfi.PositiveInfinitySymbol = info.Infinity;
            //nfi.NegativeInfinitySymbol = nfi.NegativeSign + nfi.Infinity;
            return nfi;
        }

        public static string FormatInt64(long value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                return FormatInt64(ref sb, value, format, info, numberGroupSizesOverride) ?? sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryFormatInt64(long value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                string? s = FormatInt64(ref sb, value, format, info, numberGroupSizesOverride);
                return s != null ?
                    TryCopyTo(s, destination, out charsWritten) :
                    sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static string FormatDouble(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                return FormatDouble(ref sb, value, format, info, numberGroupSizesOverride) ?? sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryFormatDouble(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                string? s = FormatDouble(ref sb, value, format, info, numberGroupSizesOverride);
                return s != null ?
                    TryCopyTo(s, destination, out charsWritten) :
                    sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static string FormatPlural(double value, string? format, MessagePattern? messagePattern, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            PluralRules pluralRules = info!.CardinalPluralRules;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[PluralCharStackBufferSize]);
            try
            {
                FormatPlural(ref sb, value, format, messagePattern, pluralRules, info);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static string FormatPlural(double value, string? format, MessagePattern? messagePattern, PluralType pluralType, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            PluralRules pluralRules = pluralType == PluralType.Ordinal ? info!.OrdinalPluralRules : info!.CardinalPluralRules;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[PluralCharStackBufferSize]);
            try
            {
                FormatPlural(ref sb, value, format, messagePattern, pluralRules, info);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static void FormatPlural(ref ValueStringBuilder sb, double value, string? format, MessagePattern? messagePattern, PluralType pluralType, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            PluralRules pluralRules = pluralType == PluralType.Ordinal ? info!.OrdinalPluralRules : info!.CardinalPluralRules;
            FormatPlural(ref sb, value, format, messagePattern, pluralRules, info);
        }

        // format is the decimalFormat string for the current culture
        public static void FormatPlural(ref ValueStringBuilder sb, double value, string? format, MessagePattern? messagePattern, PluralRules pluralRules, UNumberFormatInfo info)
        {
            Debug.Assert(pluralRules != null);
            Debug.Assert(info != null);

            int[] numberGroupSizesOverride;
            // ICU4N TODO: Need to decide the best way to deal with format pattern
            if (string.IsNullOrEmpty(format))
            {
                format = info!.NumberPattern;
                numberGroupSizesOverride = info.decimalPatternProperties.GroupingSizes ?? UCultureData.Default.GetDecimalGroupSizes();
            }
            else
            {
                numberGroupSizesOverride = GetGroupingSizes(format!);
            }

            // If no pattern was applied, return the formatted number.
            if (messagePattern is null || messagePattern.PartCount == 0)
            {
                FormatDouble(ref sb, value, format.AsSpan(), info!, numberGroupSizesOverride);
                return;
            }

            double offset = messagePattern.GetPluralOffset(pluralStart: 0); // From ApplyPattern() method

            // Get the appropriate sub-message.
            // Select it based on the formatted number-offset.
            double numberMinusOffset = value - offset;
            string numberString;
            ValueStringBuilder temp = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (offset == 0)
                {
                    FormatDouble(ref temp, value, format.AsSpan(), info!, numberGroupSizesOverride); // ICU4N NOTE: This is how we might format decimal/BigDecimal at some point (just like in ICU4J)
                }
                else
                {
                    FormatDouble(ref temp, numberMinusOffset, format.AsSpan(), info!, numberGroupSizesOverride);
                }
#pragma warning disable 612, 618
                // ICU4N NOTE: This is how we get the values for 'v' and 'f'
                // for the current context. See: https://github.com/jeffijoe/messageformat.net/blob/master/src/Jeffijoe.MessageFormat/Formatting/Formatters/PluralContext.cs
                // and the docummentation for the Operand enum.

                numberString = temp.ToString();
            }
            finally
            {
                temp.Dispose();
            }
            string decimalString = numberString;

            if (!AreAsciiDigits(info!.NativeDigitsLocal))
            {
                // ICU4N TODO: This allocation (and the parse below) can be eliminated by returning the 'v' and 'f' values
                // from the above FormatDouble() operation prior to replacing the ASCII digits with native digits.

                // We need to make sure we have ascii digits to inspect here
                // both for the length and the value to parse.
                var asciiInfo = (UNumberFormatInfo)info.Clone();
                asciiInfo.nativeDigits = AsciiDigits;
                decimalString = FormatDouble(numberMinusOffset, format.AsSpan(), asciiInfo, numberGroupSizesOverride);
            }

            int dotIndex = decimalString.IndexOf('.');
            
            int v = 0;
            long f = 0;
            if (dotIndex != -1)
            {
                ReadOnlySpan<char> fractionSpan = decimalString.AsSpan(dotIndex + 1, decimalString.Length - dotIndex - 1);
                v = fractionSpan.Length;
                f = J2N.Numerics.Int64.Parse(fractionSpan, NumberStyle.None, CultureInfo.InvariantCulture);
            }
            IFixedDecimal dec = new FixedDecimal(numberMinusOffset, v, f);
#pragma warning restore 612, 618
            int partIndex = PluralFormat.FindSubMessage(messagePattern, 0, pluralRules, dec, value);

            // Replace syntactic # signs in the top level of this sub-message
            // (not in nested arguments) with the formatted number-offset.
            int prevIndex = messagePattern.GetPart(partIndex).Limit;
            string pattern = messagePattern.PatternString;
            while (true)
            {
                MessagePatternPart part = messagePattern.GetPart(++partIndex);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    sb.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    return;
                }
                else if (type == MessagePatternPartType.ReplaceNumber ||
                          // JDK compatibility mode: Remove SKIP_SYNTAX.
                          (type == MessagePatternPartType.SkipSyntax && messagePattern.JdkAposMode))
                {
                    sb.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    if (type == MessagePatternPartType.ReplaceNumber)
                    {
                        sb.Append(numberString);
                    }
                    prevIndex = part.Limit;
                }
                else if (type == MessagePatternPartType.ArgStart)
                {
                    sb.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    prevIndex = index;
                    partIndex = messagePattern.GetLimitPartIndex(partIndex);
                    index = messagePattern.GetPart(partIndex).Limit;
                    MessagePattern.AppendReducedApostrophes(pattern, prevIndex, index, ref sb);
                    prevIndex = index;
                }
            }
        }

#if FEATURE_INT128
        public static string FormatInt128RuleBased(Int128 value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatInt128RuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info);
        }

        public static string FormatInt128RuleBased(Int128 value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatInt128RuleBased(ref sb, value, rules, ruleSetName, info);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static void FormatInt128RuleBased(ref ValueStringBuilder sb, Int128 value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            if (value >= long.MinValue && value <= long.MaxValue)
            {
                FormatInt64RuleBased(ref sb, (long)value, rules, ruleSetName, info);
            }
            else
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                FormatInt128(ref sb, value, info.NumberPattern, info, info.decimalPatternProperties.GroupingSizes);
            }
        }

        public static bool TryFormatInt128RuleBased(Int128 value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatInt128RuleBased(value, destination, out charsWritten, rules, ruleSetName, info);
        }

        public static bool TryFormatInt128RuleBased(Int128 value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatInt128RuleBased(ref sb, value, rules, ruleSetName, info);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }



        public static string FormatUInt128RuleBased(UInt128 value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatUInt128RuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info);
        }

        public static string FormatUInt128RuleBased(UInt128 value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatUInt128RuleBased(ref sb, value, rules, ruleSetName, info);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static void FormatUInt128RuleBased(ref ValueStringBuilder sb, UInt128 value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            if (value <= long.MaxValue)
            {
                FormatInt64RuleBased(ref sb, (long)value, rules, ruleSetName, info);
            }
            else
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                FormatUInt128(ref sb, value, info.NumberPattern, info, info.decimalPatternProperties.GroupingSizes);
            }
        }

        public static bool TryFormatUInt128RuleBased(UInt128 value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatUInt128RuleBased(value, destination, out charsWritten, rules, ruleSetName, info);
        }

        public static bool TryFormatUInt128RuleBased(UInt128 value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatUInt128RuleBased(ref sb, value, rules, ruleSetName, info);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }
#endif
        public static string FormatUInt64RuleBased(ulong value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatUInt64RuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info!);
        }

        public static string FormatUInt64RuleBased(ulong value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatUInt64RuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        private static void FormatUInt64RuleBased(ref ValueStringBuilder sb, ulong value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);
            
#pragma warning disable IDE0004 // Cast is redundant (required for < net7.0 to compile)
            if (value <= (ulong)long.MaxValue)
#pragma warning restore IDE0004 // Cast is redundant (required for < net7.0 to compile)
            {
                FormatInt64RuleBased(ref sb, (long)value, rules!, ruleSetName, info!);
            }
            else
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                FormatBigInteger(ref sb, value, info!.NumberPattern.AsSpan(), info, info.decimalPatternProperties.GroupingSizes);
            }
        }

        public static bool TryFormatUInt64RuleBased(ulong value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatUInt64RuleBased(value, destination, out charsWritten, rules, ruleSetName, info!);
        }

        public static bool TryFormatUInt64RuleBased(ulong value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatUInt64RuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }




        public static string FormatBigIntegerRuleBased(System.Numerics.BigInteger value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatBigIntegerRuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info!);
        }

        public static string FormatBigIntegerRuleBased(System.Numerics.BigInteger value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatBigIntegerRuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        private static void FormatBigIntegerRuleBased(ref ValueStringBuilder sb, System.Numerics.BigInteger value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            if (value >= long.MinValue && value <= long.MaxValue)
            {
                FormatInt64RuleBased(ref sb, (long)value, rules!, ruleSetName, info!);
            }
            else
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                FormatBigInteger(ref sb, value, info!.NumberPattern.AsSpan(), info, info.decimalPatternProperties.GroupingSizes);
            }
        }

        public static bool TryFormatBigIntegerRuleBased(System.Numerics.BigInteger value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatBigIntegerRuleBased(value, destination, out charsWritten, rules, ruleSetName, info!);
        }

        public static bool TryFormatBigIntegerRuleBased(System.Numerics.BigInteger value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatBigIntegerRuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }


        public static string FormatDoubleRuleBased(double value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatDoubleRuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info!);
        }

        public static string FormatDoubleRuleBased(double value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatDoubleRuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static void FormatDoubleRuleBased(ref ValueStringBuilder sb, double value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules!.DefaultRuleSet : rules!.FindRuleSet(ruleSetName);
            FormatDoubleRuleBased(ref sb, value, rules, ruleSet, info!);
        }

        public static void FormatDoubleRuleBased(ref ValueStringBuilder sb, double value, NumberFormatRules rules, NumberFormatRuleSet ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);
            Debug.Assert(ruleSet != null);

            rules!.Format(ref sb, value, ruleSet, info);
            AdjustForContext(ref sb, info!);
        }

        public static bool TryFormatDoubleRuleBased(double value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatDoubleRuleBased(value, destination, out charsWritten, rules, ruleSetName, info!);
        }

        public static bool TryFormatDoubleRuleBased(double value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules!.DefaultRuleSet : rules!.FindRuleSet(ruleSetName);
            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatDoubleRuleBased(ref sb, value, rules, ruleSet, info!);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }



        public static string FormatInt64RuleBased(long value, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            return FormatInt64RuleBased(value, rules: presentation.ToNumberFormatRules(info), ruleSetName, info!);
        }

        public static string FormatInt64RuleBased(long value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatInt64RuleBased(ref sb, value, rules!, ruleSetName, info!);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static void FormatInt64RuleBased(ref ValueStringBuilder sb, long value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules!.DefaultRuleSet : rules!.FindRuleSet(ruleSetName);
            FormatInt64RuleBased(ref sb, value, rules, ruleSet, info!);
        }

        public static void FormatInt64RuleBased(ref ValueStringBuilder sb, long value, NumberFormatRules rules, NumberFormatRuleSet ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);
            Debug.Assert(ruleSet != null);

            rules!.Format(ref sb, value, ruleSet, info);
            AdjustForContext(ref sb, info!);
        }

        public static bool TryFormatInt64RuleBased(long value, Span<char> destination, out int charsWritten, NumberPresentation presentation, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            NumberFormatRules rules = presentation.ToNumberFormatRules(info);
            return TryFormatInt64RuleBased(value, destination, out charsWritten, rules, ruleSetName, info!);
        }

        public static bool TryFormatInt64RuleBased(long value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules!.DefaultRuleSet : rules!.FindRuleSet(ruleSetName);
            var sb = new ValueStringBuilder(stackalloc char[RuleBasedCharStackBufferSize]);
            try
            {
                FormatInt64RuleBased(ref sb, value, rules, ruleSet, info!);
                return sb.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                sb.Dispose();
            }
        }

        private static void AdjustForContext(ref ValueStringBuilder sb, UNumberFormatInfo info)
        {
            if (info.Capitalization == Capitalization.None || sb.Length == 0 || !UChar.IsLower(CodePointAt(sb.AsSpan(0, 2), 0)))
            {
                return;
            }

            if (info.Capitalization == Capitalization.ForBeginningOfSentence ||
                (info.Capitalization == Capitalization.ForUIListOrMenu && info.capitalizationForListOrMenu) ||
                (info.Capitalization == Capitalization.ForStandalone && info.capitalizationForStandAlone))
            {
                // ICU4N TODO: We could save heap allocations if we pass through the Span<char> or ValueStringBuilder to accomplish the capitalization in place.

                // ICU4N TODO: use threadlocal here so we can reuse this instance?
                BreakIterator capitalizationBrkIter = (BreakIterator)info.SentenceBreakIterator.Clone(); // Clone to the current thread

                // ICU4N TODO: We use arraypool to move the chars to the heap so we can utilize BreakIterator.
                // Ideally, we could pass in delegates (for next(), prev(), etc) and stack allocated state
                // (to track the break iteration) so we don't have to move this to the heap. But we need to break
                // apart the components of RuleBasedBreakIterator to accomplish that.
                int length = sb.Length;
                char[] buffer = ArrayPool<char>.Shared.Rent(length);
                try
                {
                    sb.AsSpan().CopyTo(buffer); // Do not call sb.TryCopyTo() because we don't want to Dispose() the ValueStringBuilder yet.
                    sb.Length = 0; // Replace the entire input with the capitalized text
                    capitalizationBrkIter.SetText(buffer.AsMemory(0, length));
                    ValueStringBuilder titleCaseStringBuilder = length <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                        : new ValueStringBuilder(length);
                    try
                    {
                        CaseMapImpl.ToTitle(info.CaseLocale, UChar.TitleCaseNoLowerCase | UChar.TitleCaseNoBreakAdjustment,
                            capitalizationBrkIter, src: buffer.AsSpan(0, length), ref titleCaseStringBuilder, edits: null);
                        titleCaseStringBuilder.AsSpan().CopyTo(sb.AppendSpan(titleCaseStringBuilder.Length));
                    }
                    finally
                    {
                        titleCaseStringBuilder.Dispose();
                    }
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(buffer);
                }
            }
        }

        private static int CodePointAt(this ReadOnlySpan<char> seq, int index) // ICU4N TODO: Move to J2N
        {
            int len = seq.Length;
            if (index < 0 || index >= len)
                throw new ArgumentOutOfRangeException(nameof(index));

            char high = seq[index++];
            if (index >= len)
                return high;
            char low = seq[index];
            if (char.IsSurrogatePair(high, low))
                return Character.ToCodePoint(high, low);
            return high;
        }

        private static bool TryCopyTo(string source, Span<char> destination, out int charsWritten)
        {
            Debug.Assert(source != null);

            if (source.AsSpan().TryCopyTo(destination))
            {
                charsWritten = source!.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }

        private static string[] AsciiDigits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

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
