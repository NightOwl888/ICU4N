using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Numerics;
using System;
using System.Diagnostics;
using System.Globalization;
using static ICU4N.Text.PluralRules;
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
        private static unsafe void FormatBigInteger(ref ValueStringBuilder sb, System.Numerics.BigInteger value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
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
        private static unsafe string? FormatDouble(ref ValueStringBuilder sb, double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride)
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

        public static string FormatInt64(long value, string? format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            return FormatInt64(ref sb, value, format, info, numberGroupSizesOverride) ?? sb.ToString();
        }

        public static bool TryFormatInt64(long value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            string? s = FormatInt64(ref sb, value, format, info, numberGroupSizesOverride);
            return s != null ?
                TryCopyTo(s, destination, out charsWritten) :
                sb.TryCopyTo(destination, out charsWritten);
        }

        public static string FormatDouble(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            return FormatDouble(ref sb, value, format, info, numberGroupSizesOverride) ?? sb.ToString();
        }

        public static bool TryFormatDouble(double value, ReadOnlySpan<char> format, UNumberFormatInfo info, Span<char> destination, out int charsWritten, int[]? numberGroupSizesOverride = null)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            string? s = FormatDouble(ref sb, value, format, info, numberGroupSizesOverride);
            return s != null ?
                TryCopyTo(s, destination, out charsWritten) :
                sb.TryCopyTo(destination, out charsWritten);
        }


        public static string FormatPlural(double value, string? format, MessagePattern? messagePattern, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            PluralRules pluralRules = info.CardinalPluralRules;
            return FormatPlural(value, format, messagePattern, pluralRules, info);
        }

        public static string FormatPlural(double value, string? format, MessagePattern? messagePattern, PluralType pluralType, UNumberFormatInfo info)
        {
            Debug.Assert(info != null);

            PluralRules pluralRules = pluralType == PluralType.Ordinal ? info.OrdinalPluralRules : info.CardinalPluralRules;
            return FormatPlural(value, format, messagePattern, pluralRules, info);
        }

        // format is the decimalFormat string for the current culture
        public static string FormatPlural(double value, string? format, MessagePattern? messagePattern, PluralRules pluralRules, UNumberFormatInfo info)
        {
            Debug.Assert(pluralRules != null);
            Debug.Assert(info != null);

            int[] numberGroupSizesOverride;
            // ICU4N TODO: Need to decide the best way to deal with format pattern
            if (string.IsNullOrEmpty(format))
            {
                format = info.NumberPattern;
                numberGroupSizesOverride = info.decimalPatternProperties.GroupingSizes ?? UCultureData.Default.GetDecimalGroupSizes();
            }
            else
            {
                numberGroupSizesOverride = GetGroupingSizes(format);
            }

            // If no pattern was applied, return the formatted number.
            if (messagePattern is null || messagePattern.PartCount == 0)
                return FormatDouble(value, format, info, numberGroupSizesOverride);

            double offset = messagePattern.GetPluralOffset(pluralStart: 0); // From ApplyPattern() method

            // Get the appropriate sub-message.
            // Select it based on the formatted number-offset.
            double numberMinusOffset = value - offset;
            string numberString;

            if (offset == 0)
            {
                numberString = FormatDouble(value, format, info, numberGroupSizesOverride);
            }
            else
            {
                numberString = FormatDouble(numberMinusOffset, format, info, numberGroupSizesOverride);
            }
#pragma warning disable 612, 618
            // ICU4N NOTE: This is how we get the values for 'v' and 'f'
            // for the current context. See: https://github.com/jeffijoe/messageformat.net/blob/master/src/Jeffijoe.MessageFormat/Formatting/Formatters/PluralContext.cs
            // and the docummentation for the Operand enum.

            string decimalString;
            if (AreAsciiDigits(info.NativeDigitsLocal))
            {
                decimalString = numberString;
            }
            else
            {
                // We need to make sure we have ascii digits to inspect here
                // both for the length and the value to parse.
                var asciiInfo = (UNumberFormatInfo)info.Clone();
                asciiInfo.nativeDigits = AsciiDigits;
                decimalString = FormatDouble(numberMinusOffset, format, asciiInfo, numberGroupSizesOverride);
            }

            int dotIndex = decimalString.IndexOf('.');
            
            int v = 0;
            long f = 0;
            if (dotIndex != -1)
            {
                ReadOnlySpan<char> fractionSpan = decimalString.AsSpan(dotIndex + 1, decimalString.Length - dotIndex - 1);
                v = fractionSpan.Length;
                f = long.Parse(fractionSpan, NumberStyles.None, CultureInfo.InvariantCulture);
            }
            IFixedDecimal dec = new FixedDecimal(numberMinusOffset, v, f);
#pragma warning restore 612, 618
            int partIndex = PluralFormat.FindSubMessage(messagePattern, 0, pluralRules, dec, value);

            // Replace syntactic # signs in the top level of this sub-message
            // (not in nested arguments) with the formatted number-offset.

            ValueStringBuilder result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            int prevIndex = messagePattern.GetPart(partIndex).Limit;
            string pattern = messagePattern.PatternString;
            while (true)
            {
                MessagePatternPart part = messagePattern.GetPart(++partIndex);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    return result.ToString();
                }
                else if (type == MessagePatternPartType.ReplaceNumber ||
                          // JDK compatibility mode: Remove SKIP_SYNTAX.
                          (type == MessagePatternPartType.SkipSyntax && messagePattern.JdkAposMode))
                {
                    result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    if (type == MessagePatternPartType.ReplaceNumber)
                    {
                        result.Append(numberString);
                    }
                    prevIndex = part.Limit;
                }
                else if (type == MessagePatternPartType.ArgStart)
                {
                    result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                    prevIndex = index;
                    partIndex = messagePattern.GetLimitPartIndex(partIndex);
                    index = messagePattern.GetPart(partIndex).Limit;
                    MessagePattern.AppendReducedApostrophes(pattern, prevIndex, index, ref result);
                    prevIndex = index;
                }
            }
        }



        public static string FormatBigIntegerRuleBased(System.Numerics.BigInteger value, NumberFormatRules rules, string? ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            FormatBigIntegerRuleBased(ref sb, value, rules, ruleSet, info);
            return sb.ToString();
        }

        private static void FormatBigIntegerRuleBased(ref ValueStringBuilder sb, System.Numerics.BigInteger value, NumberFormatRules rules, string? ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            if (value >= long.MinValue && value <= long.MaxValue)
            {
                FormatInt64RuleBased(ref sb, (long)value, rules, ruleSet, info);
            }
            else
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                FormatBigInteger(ref sb, value, info.NumberPattern, info, info.decimalPatternProperties.GroupingSizes);
            }
        }

        public static string FormatDoubleRuleBased(double value, NumberFormatRules rules, string? ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            FormatDoubleRuleBased(ref sb, value, rules, ruleSet, info);
            return sb.ToString();
        }

        private static void FormatDoubleRuleBased(ref ValueStringBuilder sb, double value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            // ICU4N TODO: Need to validate ruleset name is not a private set and that it exists (in callers, don't throw here)
            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules.DefaultRuleSet : rules.FindRuleSet(ruleSetName, throwIfNotFound: false);
            rules.Format(ref sb, value, ruleSet, info);
            
            if (info.Capitalization == Capitalization.BeginningOfSentence ||
                (info.Capitalization == Capitalization.UIListOrMenu && info.capitalizationForListOrMenu) ||
                (info.Capitalization == Capitalization.Standalone && info.capitalizationForStandAlone))
            {
                //// ICU4N TODO: use threadlocal here so we can reuse this instance?
                //BreakIterator capitalizationBrkIter = (BreakIterator)info.SentenceBreakIterator.Clone(); // Clone to the current thread
                //string temp = new string(sb.AsSpan());
                //sb.Length = 0;
                //sb.Append(UChar.ToTitleCase()) // ICU4N TODO: Factor out locale from ToTitleCase() and move to UCultureData.
            }
        }

        public static string FormatInt64RuleBased(long value, NumberFormatRules rules, string? ruleSet,  UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            FormatInt64RuleBased(ref sb, value, rules, ruleSet, info);
            return sb.ToString();
        }

        private static void FormatInt64RuleBased(ref ValueStringBuilder sb, long value, NumberFormatRules rules, string? ruleSetName, UNumberFormatInfo info)
        {
            Debug.Assert(rules != null);
            Debug.Assert(info != null);

            // ICU4N TODO: Need to validate ruleset name is not a private set and that it exists (in callers, don't throw here)
            NumberFormatRuleSet ruleSet = ruleSetName is null ? rules.DefaultRuleSet : rules.FindRuleSet(ruleSetName, throwIfNotFound: false);
            rules.Format(ref sb, value, ruleSet, info);
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
