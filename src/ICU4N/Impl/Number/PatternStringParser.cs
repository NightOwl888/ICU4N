using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Numerics.BigMath;
using J2N;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Numerics.Padder;

namespace ICU4N.Numerics
{
    /// <summary>
    /// Implements a recursive descent parser for decimal format patterns.
    /// </summary>
    internal class PatternStringParser // ICU4N TODO: API - this was public in ICU4J
    {
        // ICU4N TODO: API - make into enum?
        public const int IGNORE_ROUNDING_NEVER = 0; // ICU4N TODO: API - Naming
        public const int IGNORE_ROUNDING_IF_CURRENCY = 1; // ICU4N TODO: API - Naming
        public const int IGNORE_ROUNDING_ALWAYS = 2; // ICU4N TODO: API - Naming

        /**
         * Runs the recursive descent parser on the given pattern string, returning a data structure with raw information
         * about the pattern string.
         *
         * <para/>
         * To obtain a more useful form of the data, consider using {@link #parseToProperties} instead.
         *
         * @param patternString
         *            The LDML decimal format pattern (Excel-style pattern) to parse.
         * @return The results of the parse.
         */
        public static ParsedPatternInfo ParseToPatternInfo(string patternString)
        {
            ParserState state = new ParserState(patternString);
            ParsedPatternInfo result = new ParsedPatternInfo(patternString);
            ConsumePattern(state, result);
            return result;
        }

        /**
         * Parses a pattern string into a new property bag.
         *
         * @param pattern
         *            The pattern string, like "#,##0.00"
         * @param ignoreRounding
         *            Whether to leave out rounding information (minFrac, maxFrac, and rounding increment) when parsing the
         *            pattern. This may be desirable if a custom rounding mode, such as CurrencyUsage, is to be used
         *            instead. One of {@link PatternStringParser#IGNORE_ROUNDING_ALWAYS},
         *            {@link PatternStringParser#IGNORE_ROUNDING_IF_CURRENCY}, or
         *            {@link PatternStringParser#IGNORE_ROUNDING_NEVER}.
         * @return A property bag object.
         * @throws IllegalArgumentException
         *             If there is a syntax error in the pattern string.
         */
        public static DecimalFormatProperties ParseToProperties(string pattern, int ignoreRounding) // ICU4N TODO: Make TryParseToProperties
        {
            DecimalFormatProperties properties = new DecimalFormatProperties();
            ParseToExistingPropertiesImpl(pattern, properties, ignoreRounding);
            return properties;
        }

        public static DecimalFormatProperties ParseToProperties(string pattern) // ICU4N TODO: Make TryParseToProperties
        {
            return ParseToProperties(pattern, PatternStringParser.IGNORE_ROUNDING_NEVER);
        }

        // ICU4N: Added to store only the deconstructed string in NumberFormatSubstitution and UCultureData so we can pass this
        // info into the number formatting pipeline based on the string in a rule or default pattern for DecimalFormat.
        // ICU4J simply throws out the string and uses these properties only (along with many others, but until we break free
        // from the .NET formatter, we need the string.
        internal static NumberPatternStringProperties ParseToPatternStringProperties(string pattern, int ignoreRounding)
        {
            DecimalFormatProperties properties = new DecimalFormatProperties();
            return ParseToPatternStringProperties(pattern, properties, ignoreRounding);
        }

        internal static NumberPatternStringProperties ParseToPatternStringProperties(string pattern, DecimalFormatProperties reuse, int ignoreRounding)
        {
            ParseToExistingPropertiesImpl(pattern, reuse, ignoreRounding);
            return new NumberPatternStringProperties(reuse);
        }

        /**
         * Parses a pattern string into an existing property bag. All properties that can be encoded into a pattern string
         * will be overwritten with either their default value or with the value coming from the pattern string. Properties
         * that cannot be encoded into a pattern string, such as rounding mode, are not modified.
         *
         * @param pattern
         *            The pattern string, like "#,##0.00"
         * @param properties
         *            The property bag object to overwrite.
         * @param ignoreRounding
         *            See {@link #parseToProperties(String pattern, int ignoreRounding)}.
         * @throws IllegalArgumentException
         *             If there was a syntax error in the pattern string.
         */
        public static void ParseToExistingProperties(string pattern, DecimalFormatProperties properties,
                int ignoreRounding) // ICU4N TODO: Make TryParse version of this.
        {
            ParseToExistingPropertiesImpl(pattern, properties, ignoreRounding);
        }

        public static void ParseToExistingProperties(string pattern, DecimalFormatProperties properties) // ICU4N TODO: Make TryParse version of this.
        {
            ParseToExistingProperties(pattern, properties, PatternStringParser.IGNORE_ROUNDING_NEVER);
        }

        /**
         * Contains raw information about the parsed decimal format pattern string.
         */
        public class ParsedPatternInfo : IAffixPatternProvider
        {
            public string pattern;
            public ParsedSubpatternInfo positive;
            public ParsedSubpatternInfo negative;

            internal ParsedPatternInfo(string pattern)
            {
                this.pattern = pattern;
            }

            char IAffixPatternProvider.this[AffixPatternProviderFlags flags, int index]
            {
                get
                {
                    long endpoints = GetEndpoints(flags);
                    int left = (int)(endpoints & 0xffffffff);
                    int right = (int)(endpoints.TripleShift(32));
                    if (index < 0 || index >= right - left)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return pattern[left + index];
                }
            }

            public virtual int Length(AffixPatternProviderFlags flags)
            {
                return GetLengthFromEndpoints(GetEndpoints(flags));
            }


            public static int GetLengthFromEndpoints(long endpoints)
            {
                int left = (int)(endpoints & 0xffffffff);
                int right = (int)(endpoints.TripleShift(32));
                return right - left;
            }

            public virtual string GetString(AffixPatternProviderFlags flags)
            {
                long endpoints = GetEndpoints(flags);
                int left = (int)(endpoints & 0xffffffff);
                int right = (int)(endpoints.TripleShift(32));
                if (left == right)
                {
                    return "";
                }
                return pattern.Substring(left, right - left); // ICU4N: Corrected 2nd arg
            }

            public virtual ReadOnlySpan<char> AsSpan(AffixPatternProviderFlags flags) // ICU4N: Added so we don't need to rely on ICharSequence
            {
                long endpoints = GetEndpoints(flags);
                int left = (int)(endpoints & 0xffffffff);
                int right = (int)(endpoints.TripleShift(32));
                if (left == right)
                {
                    return ReadOnlySpan<char>.Empty;
                }
                return pattern.AsSpan(left, right - left); // ICU4N: Corrected 2nd arg
            }

            private long GetEndpoints(AffixPatternProviderFlags flags)
            {
                bool prefix = (flags & AffixPatternProviderFlags.Prefix) != 0;
                bool isNegative = (flags & AffixPatternProviderFlags.NegativeSubpattern) != 0;
                bool padding = (flags & AffixPatternProviderFlags.Padding) != 0;
                if (isNegative && padding)
                {
                    return negative.paddingEndpoints;
                }
                else if (padding)
                {
                    return positive.paddingEndpoints;
                }
                else if (prefix && isNegative)
                {
                    return negative.prefixEndpoints;
                }
                else if (prefix)
                {
                    return positive.prefixEndpoints;
                }
                else if (isNegative)
                {
                    return negative.suffixEndpoints;
                }
                else
                {
                    return positive.suffixEndpoints;
                }
            }

            public virtual bool PositiveHasPlusSign => positive.hasPlusSign;

            public virtual bool HasNegativeSubpattern => negative != null;

            public virtual bool NegativeHasMinusSign => negative.hasMinusSign;

            public virtual bool HasCurrencySign => positive.hasCurrencySign || (negative != null && negative.hasCurrencySign);


            public virtual bool ContainsSymbolType(AffixUtils.Type type)
            {
                return AffixUtils.ContainsType(pattern, type);
            }


        }

        public class ParsedSubpatternInfo
        {
            public long groupingSizes = 0x0000ffffffff0000L;
            public int integerLeadingHashSigns = 0;
            public int integerTrailingHashSigns = 0;
            public int integerNumerals = 0;
            public int integerAtSigns = 0;
            public int integerTotal = 0; // for convenience
            public int fractionNumerals = 0;
            public int fractionHashSigns = 0;
            public int fractionTotal = 0; // for convenience
            public bool hasDecimal = false;
            public int widthExceptAffixes = 0;
            public PadPosition? paddingLocation = null;
            public DecimalQuantity_DualStorageBCD rounding = null;
            public bool exponentHasPlusSign = false;
            public int exponentZeros = 0;
            public bool hasPercentSign = false;
            public bool hasPerMilleSign = false;
            public bool hasCurrencySign = false;
            public bool hasMinusSign = false;
            public bool hasPlusSign = false;

            public long prefixEndpoints = 0;
            public long suffixEndpoints = 0;
            public long paddingEndpoints = 0;
        }

        ///////////////////////////////////////////////////
        // BEGIN RECURSIVE DESCENT PARSER IMPLEMENTATION //
        ///////////////////////////////////////////////////

        /** An internal class used for tracking the cursor during parsing of a pattern string. */
        private class ParserState
        {
            internal readonly string pattern;
            internal int offset;

            internal ParserState(string pattern)
            {
                this.pattern = pattern;
                this.offset = 0;
            }

            internal virtual int Peek()
            {
                if (offset == pattern.Length)
                {
                    return -1;
                }
                else
                {
                    return pattern.CodePointAt(offset);
                }
            }

            internal int Next()
            {
                int codePoint = Peek();
                offset += Character.CharCount(codePoint);
                return codePoint;
            }

            internal FormatException ToParseException(string message)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Malformed pattern for ICU DecimalFormat: \"");
                sb.Append(pattern);
                sb.Append("\": ");
                sb.Append(message);
                sb.Append(" at position ");
                sb.Append(offset);
                return new FormatException(sb.ToString());
            }
        }

        private static void ConsumePattern(ParserState state, ParsedPatternInfo result)
        {
            // pattern := subpattern (';' subpattern)?
            result.positive = new ParsedSubpatternInfo();
            ConsumeSubpattern(state, result.positive);
            if (state.Peek() == ';')
            {
                state.Next(); // consume the ';'
                              // Don't consume the negative subpattern if it is empty (trailing ';')
                if (state.Peek() != -1)
                {
                    result.negative = new ParsedSubpatternInfo();
                    ConsumeSubpattern(state, result.negative);
                }
            }
            if (state.Peek() != -1)
            {
                throw state.ToParseException("Found unquoted special character");
            }
        }

        private static void ConsumeSubpattern(ParserState state, ParsedSubpatternInfo result)
        {
            // subpattern := literals? number exponent? literals?
            ConsumePadding(state, result, PadPosition.BeforePrefix);
            result.prefixEndpoints = ConsumeAffix(state, result);
            ConsumePadding(state, result, PadPosition.AfterPrefix);
            ConsumeFormat(state, result);
            ConsumeExponent(state, result);
            ConsumePadding(state, result, PadPosition.BeforeSuffix);
            result.suffixEndpoints = ConsumeAffix(state, result);
            ConsumePadding(state, result, PadPosition.AfterSuffix);
        }

        private static void ConsumePadding(ParserState state, ParsedSubpatternInfo result, PadPosition paddingLocation)
        {
            if (state.Peek() != '*')
            {
                return;
            }
            if (result.paddingLocation != null)
            {
                throw state.ToParseException("Cannot have multiple pad specifiers");
            }
            result.paddingLocation = paddingLocation;
            state.Next(); // consume the '*'
            result.paddingEndpoints |= (uint)state.offset;
            ConsumeLiteral(state);
            result.paddingEndpoints |= ((long)state.offset) << 32;
        }

        private static long ConsumeAffix(ParserState state, ParsedSubpatternInfo result)
        {
            // literals := { literal }
            long endpoints = state.offset;
            //outer:
            while (true)
            {
                switch (state.Peek())
                {
                    case '#':
                    case '@':
                    case ';':
                    case '*':
                    case '.':
                    case ',':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case -1:
                        // Characters that cannot appear unquoted in a literal
                        //break outer;
                        goto outer_break;

                    case '%':
                        result.hasPercentSign = true;
                        break;

                    case '‰':
                        result.hasPerMilleSign = true;
                        break;

                    case '¤':
                        result.hasCurrencySign = true;
                        break;

                    case '-':
                        result.hasMinusSign = true;
                        break;

                    case '+':
                        result.hasPlusSign = true;
                        break;
                }
                ConsumeLiteral(state);
            }
        outer_break: { /* Intentionally blank */ }
            endpoints |= ((long)state.offset) << 32;
            return endpoints;
        }

        private static void ConsumeLiteral(ParserState state)
        {
            if (state.Peek() == -1)
            {
                throw state.ToParseException("Expected unquoted literal but found EOL");
            }
            else if (state.Peek() == '\'')
            {
                state.Next(); // consume the starting quote
                while (state.Peek() != '\'')
                {
                    if (state.Peek() == -1)
                    {
                        throw state.ToParseException("Expected quoted literal but found EOL");
                    }
                    else
                    {
                        state.Next(); // consume a quoted character
                    }
                }
                state.Next(); // consume the ending quote
            }
            else
            {
                // consume a non-quoted literal character
                state.Next();
            }
        }

        private static void ConsumeFormat(ParserState state, ParsedSubpatternInfo result)
        {
            ConsumeIntegerFormat(state, result);
            if (state.Peek() == '.')
            {
                state.Next(); // consume the decimal point
                result.hasDecimal = true;
                result.widthExceptAffixes += 1;
                ConsumeFractionFormat(state, result);
            }
        }

        private static void ConsumeIntegerFormat(ParserState state, ParsedSubpatternInfo result)
        {
            //outer: 
            while (true)
            {
                switch (state.Peek())
                {
                    case ',':
                        result.widthExceptAffixes += 1;
                        result.groupingSizes <<= 16;
                        break;

                    case '#':
                        if (result.integerNumerals > 0)
                        {
                            throw state.ToParseException("# cannot follow 0 before decimal point");
                        }
                        result.widthExceptAffixes += 1;
                        result.groupingSizes += 1;
                        if (result.integerAtSigns > 0)
                        {
                            result.integerTrailingHashSigns += 1;
                        }
                        else
                        {
                            result.integerLeadingHashSigns += 1;
                        }
                        result.integerTotal += 1;
                        break;

                    case '@':
                        if (result.integerNumerals > 0)
                        {
                            throw state.ToParseException("Cannot mix 0 and @");
                        }
                        if (result.integerTrailingHashSigns > 0)
                        {
                            throw state.ToParseException("Cannot nest # inside of a run of @");
                        }
                        result.widthExceptAffixes += 1;
                        result.groupingSizes += 1;
                        result.integerAtSigns += 1;
                        result.integerTotal += 1;
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (result.integerAtSigns > 0)
                        {
                            throw state.ToParseException("Cannot mix @ and 0");
                        }
                        result.widthExceptAffixes += 1;
                        result.groupingSizes += 1;
                        result.integerNumerals += 1;
                        result.integerTotal += 1;
                        if (state.Peek() != '0' && result.rounding == null)
                        {
                            result.rounding = new DecimalQuantity_DualStorageBCD();
                        }
                        if (result.rounding != null)
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            result.rounding.AppendDigit((byte)(state.Peek() - '0'), 0, true);
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                        break;

                    default:
                        //break outer;
                        goto outer_break;
                }
                state.Next(); // consume the symbol
            }
        outer_break: { /* Intentionally blank */ }

            // Disallow patterns with a trailing ',' or with two ',' next to each other
            short grouping1 = (short)(result.groupingSizes & 0xffff);
            short grouping2 = (short)((result.groupingSizes.TripleShift(16)) & 0xffff);
            short grouping3 = (short)((result.groupingSizes.TripleShift(32)) & 0xffff);
            if (grouping1 == 0 && grouping2 != -1)
            {
                throw state.ToParseException("Trailing grouping separator is invalid");
            }
            if (grouping2 == 0 && grouping3 != -1)
            {
                throw state.ToParseException("Grouping width of zero is invalid");
            }
        }

        private static void ConsumeFractionFormat(ParserState state, ParsedSubpatternInfo result)
        {
            int zeroCounter = 0;
            while (true)
            {
                switch (state.Peek())
                {
                    case '#':
                        result.widthExceptAffixes += 1;
                        result.fractionHashSigns += 1;
                        result.fractionTotal += 1;
                        zeroCounter++;
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (result.fractionHashSigns > 0)
                        {
                            throw state.ToParseException("0 cannot follow # after decimal point");
                        }
                        result.widthExceptAffixes += 1;
                        result.fractionNumerals += 1;
                        result.fractionTotal += 1;
                        if (state.Peek() == '0')
                        {
                            zeroCounter++;
                        }
                        else
                        {
                            if (result.rounding == null)
                            {
                                result.rounding = new DecimalQuantity_DualStorageBCD();
                            }
#pragma warning disable CS0618 // Type or member is obsolete
                            result.rounding.AppendDigit((byte)(state.Peek() - '0'), zeroCounter, false);
#pragma warning restore CS0618 // Type or member is obsolete
                            zeroCounter = 0;
                        }
                        break;

                    default:
                        return;
                }
                state.Next(); // consume the symbol
            }
        }

        private static void ConsumeExponent(ParserState state, ParsedSubpatternInfo result)
        {
            if (state.Peek() != 'E')
            {
                return;
            }
            if ((result.groupingSizes & 0xffff0000L) != 0xffff0000L)
            {
                throw state.ToParseException("Cannot have grouping separator in scientific notation");
            }
            state.Next(); // consume the E
            result.widthExceptAffixes++;
            if (state.Peek() == '+')
            {
                state.Next(); // consume the +
                result.exponentHasPlusSign = true;
                result.widthExceptAffixes++;
            }
            while (state.Peek() == '0')
            {
                state.Next(); // consume the 0
                result.exponentZeros += 1;
                result.widthExceptAffixes++;
            }
        }

        ///////////////////////////////////////////////////
        /// END RECURSIVE DESCENT PARSER IMPLEMENTATION ///
        ///////////////////////////////////////////////////

        private static void ParseToExistingPropertiesImpl(string pattern, DecimalFormatProperties properties, int ignoreRounding)
        {
            if (pattern == null || pattern.Length == 0)
            {
                // Backwards compatibility requires that we reset to the default values.
                // TODO: Only overwrite the properties that "saveToProperties" normally touches?
                properties.Clear();
                return;
            }

            // TODO: Use thread locals here?
            ParsedPatternInfo patternInfo = ParseToPatternInfo(pattern);
            PatternInfoToProperties(properties, patternInfo, ignoreRounding);
        }

        /** Finalizes the temporary data stored in the ParsedPatternInfo to the Properties. */
        private static void PatternInfoToProperties(DecimalFormatProperties properties, ParsedPatternInfo patternInfo,
                int _ignoreRounding)
        {
            // Translate from PatternParseResult to Properties.
            // Note that most data from "negative" is ignored per the specification of DecimalFormat.

            ParsedSubpatternInfo positive = patternInfo.positive;

            bool ignoreRounding;
            if (_ignoreRounding == PatternStringParser.IGNORE_ROUNDING_NEVER)
            {
                ignoreRounding = false;
            }
            else if (_ignoreRounding == PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY)
            {
                ignoreRounding = positive.hasCurrencySign;
            }
            else
            {
                Debug.Assert(_ignoreRounding == PatternStringParser.IGNORE_ROUNDING_ALWAYS);
                ignoreRounding = true;
            }

            // Grouping settings
            short grouping1 = (short)(positive.groupingSizes & 0xffff);
            short grouping2 = (short)((positive.groupingSizes.TripleShift(16)) & 0xffff);
            short grouping3 = (short)((positive.groupingSizes.TripleShift(32)) & 0xffff);
            if (grouping2 != -1)
            {
                properties.GroupingSize = grouping1;
            }
            else
            {
                properties.GroupingSize = -1;
            }
            if (grouping3 != -1)
            {
                properties.SecondaryGroupingSize = grouping2;
            }
            else
            {
                properties.SecondaryGroupingSize = -1;
            }

            // For backwards compatibility, require that the pattern emit at least one min digit.
            int minInt, minFrac;
            if (positive.integerTotal == 0 && positive.fractionTotal > 0)
            {
                // patterns like ".##"
                minInt = 0;
                minFrac = Math.Max(1, positive.fractionNumerals);
            }
            else if (positive.integerNumerals == 0 && positive.fractionNumerals == 0)
            {
                // patterns like "#.##"
                minInt = 1;
                minFrac = 0;
            }
            else
            {
                minInt = positive.integerNumerals;
                minFrac = positive.fractionNumerals;
            }

            // Rounding settings
            // Don't set basic rounding when there is a currency sign; defer to CurrencyUsage
            if (positive.integerAtSigns > 0)
            {
                properties.MinimumFractionDigits = -1;
                properties.MaximumFractionDigits = -1;
                properties.RoundingIncrement = null;
                properties.MinimumSignificantDigits = positive.integerAtSigns;
                properties.MaximumSignificantDigits = positive.integerAtSigns + positive.integerTrailingHashSigns;
            }
            else if (positive.rounding != null)
            {
                if (!ignoreRounding)
                {
                    properties.MinimumFractionDigits = minFrac;
                    properties.MaximumFractionDigits = positive.fractionTotal;
                    properties.RoundingIncrement = ICU4N.Numerics.BigMath.BigDecimal.SetScale(positive.rounding.ToBigDecimal(), positive.fractionNumerals);
                }
                else
                {
                    properties.MinimumFractionDigits = -1;
                    properties.MaximumFractionDigits = -1;
                    properties.RoundingIncrement = null;
                }
                properties.MinimumSignificantDigits = -1;
                properties.MaximumSignificantDigits = -1;
            }
            else
            {
                if (!ignoreRounding)
                {
                    properties.MinimumFractionDigits = minFrac;
                    properties.MaximumFractionDigits = positive.fractionTotal;
                    properties.RoundingIncrement = null;
                }
                else
                {
                    properties.MinimumFractionDigits = -1;
                    properties.MaximumFractionDigits = -1;
                    properties.RoundingIncrement = null;
                }
                properties.MinimumSignificantDigits = -1;
                properties.MaximumSignificantDigits = -1;
            }

            // If the pattern ends with a '.' then force the decimal point.
            if (positive.hasDecimal && positive.fractionTotal == 0)
            {
                properties.DecimalSeparatorAlwaysShown = true;
            }
            else
            {
                properties.DecimalSeparatorAlwaysShown = false;
            }

            // Scientific notation settings
            if (positive.exponentZeros > 0)
            {
                properties.ExponentSignAlwaysShown = positive.exponentHasPlusSign;
                properties.MinimumExponentDigits = positive.exponentZeros;
                if (positive.integerAtSigns == 0)
                {
                    // patterns without '@' can define max integer digits, used for engineering notation
                    properties.MinimumIntegerDigits = positive.integerNumerals;
                    properties.MaximumIntegerDigits = positive.integerTotal;
                }
                else
                {
                    // patterns with '@' cannot define max integer digits
                    properties.MinimumIntegerDigits = 1;
                    properties.MaximumIntegerDigits = -1;
                }
            }
            else
            {
                properties.ExponentSignAlwaysShown = false;
                properties.MinimumExponentDigits = -1;
                properties.MinimumIntegerDigits = minInt;
                properties.MaximumIntegerDigits = -1;
            }

            // Compute the affix patterns (required for both padding and affixes)
            string posPrefix = patternInfo.GetString(AffixPatternProviderFlags.Prefix);
            string posSuffix = patternInfo.GetString(0);

            // Padding settings
            if (positive.paddingLocation != null)
            {
                // The width of the positive prefix and suffix templates are included in the padding
                int paddingWidth = positive.widthExceptAffixes + AffixUtils.EstimateLength(posPrefix)
                        + AffixUtils.EstimateLength(posSuffix);
                properties.FormatWidth = paddingWidth;
                string rawPaddingString = patternInfo.GetString(AffixPatternProviderFlags.Padding);
                if (rawPaddingString.Length == 1)
                {
                    properties.PadString = rawPaddingString;
                }
                else if (rawPaddingString.Length == 2)
                {
                    if (rawPaddingString[0] == '\'')
                    {
                        properties.PadString = "'";
                    }
                    else
                    {
                        properties.PadString = rawPaddingString;
                    }
                }
                else
                {
                    properties.PadString = rawPaddingString.Substring(1, rawPaddingString.Length - 2); // ICU4N: Corrected 2nd arg
                }
                Debug.Assert(positive.paddingLocation != null);
                properties.PadPosition = positive.paddingLocation;
            }
            else
            {
                properties.FormatWidth = -1;
                properties.PadString = null;
                properties.PadPosition = null;
            }

            // Set the affixes
            // Always call the setter, even if the prefixes are empty, especially in the case of the
            // negative prefix pattern, to prevent default values from overriding the pattern.
            properties.PositivePrefixPattern = posPrefix;
            properties.PositiveSuffixPattern = posSuffix;
            if (patternInfo.negative != null)
            {
                properties.NegativePrefixPattern = (patternInfo
                        .GetString(AffixPatternProviderFlags.NegativeSubpattern | AffixPatternProviderFlags.Prefix));
                properties.NegativeSuffixPattern = (patternInfo.GetString(AffixPatternProviderFlags.NegativeSubpattern));
            }
            else
            {
                properties.NegativePrefixPattern = null;
                properties.NegativeSuffixPattern = null;
            }

            // Set the magnitude multiplier
            if (positive.hasPercentSign)
            {
                properties.MagnitudeMultiplier = 2;
            }
            else if (positive.hasPerMilleSign)
            {
                properties.MagnitudeMultiplier = 3;
            }
            else
            {
                properties.MagnitudeMultiplier = 0;
            }
        }
    }

#nullable enable
    /// <summary>
    /// A deconstructed pattern string. Contains all of the properties in a pattern string that can be used
    /// to reconstitute a pattern string.
    /// </summary>
    // ICU4N specific
    internal struct NumberPatternStringProperties
    {
        private bool decimalSeparatorAlwaysShown;
        private bool exponentSignAlwaysShown;
        private int? formatWidth;
        private int[]? groupingSizes;
        private int magnitudeMultiplier;
        private int? maximumFractionDigits;
        private int? minimumFractionDigits;
        private int? maximumIntegerDigits;
        private int? minimumIntegerDigits;
        private int? maximumSignificantDigits;
        private int? minimumSignificantDigits;
        private int? minimumExponentDigits;
        private Padder.PadPosition? padPosition;
        private string? padString;
        private BigMath.BigDecimal? roundingIncrement;

        public bool DecimalSeparatorAlwaysShown => decimalSeparatorAlwaysShown;
        public bool ExponentSignAlwaysShown => exponentSignAlwaysShown;
        public int? FormatWidth => formatWidth;
        public int[]? GroupingSizes => groupingSizes;
        public int MagnitudeMultiplier => magnitudeMultiplier;
        public int? MaximumFractionDigits => maximumFractionDigits;
        public int? MinimumFractionDigits => minimumFractionDigits;
        public int? MaximumIntegerDigits => maximumIntegerDigits;
        public int? MinimumIntegerDigits => minimumIntegerDigits;
        public int? MaximumSignificantDigits => maximumSignificantDigits;
        public int? MinimumSignificantDigits => minimumSignificantDigits;
        public int? MinimumExponentDigits => minimumExponentDigits;
        public Padder.PadPosition? PadPosition => padPosition;
        public string? PadString => padString;
        public BigMath.BigDecimal? RoundingIncrement => roundingIncrement;
        public NumberPatternStringProperties(DecimalFormatProperties properties)
        {
            decimalSeparatorAlwaysShown = properties.DecimalSeparatorAlwaysShown;
            exponentSignAlwaysShown = properties.ExponentSignAlwaysShown;
            formatWidth = ConvertNegativeOneToNull(properties.FormatWidth);
            int groupingSize = properties.GroupingSize; // defaults to -1
            int secondaryGroupingSize = properties.SecondaryGroupingSize; // defaults to -1
            if (groupingSize != -1)
            {
                if (secondaryGroupingSize != -1)
                    groupingSizes = new int[] { groupingSize, secondaryGroupingSize };
                else
                    groupingSizes = new int[] { groupingSize };
            }
            else
                groupingSizes = null;
            magnitudeMultiplier = properties.MagnitudeMultiplier; // defaults to 0
            maximumFractionDigits = ConvertNegativeOneToNull(properties.MaximumFractionDigits);
            minimumFractionDigits = ConvertNegativeOneToNull(properties.MinimumFractionDigits);
            maximumIntegerDigits = ConvertNegativeOneToNull(properties.MaximumIntegerDigits);
            minimumIntegerDigits = ConvertNegativeOneToNull(properties.MinimumIntegerDigits);
            maximumSignificantDigits = ConvertNegativeOneToNull(properties.MaximumSignificantDigits);
            minimumSignificantDigits = ConvertNegativeOneToNull(properties.MinimumSignificantDigits);
            minimumExponentDigits = ConvertNegativeOneToNull(properties.MinimumExponentDigits);
            padPosition = properties.PadPosition;
            padString = properties.PadString;
            roundingIncrement = properties.RoundingIncrement;
        }

        private static int? ConvertNegativeOneToNull(int value)
            => value == -1 ? null : value;

        // currentSetting should typically come from UNumberFormatInfo
        // contextProperties should be the culture data prior to any changes the user made.
        public static int GetEffectiveMaximumFractionDigits(int currentSetting, ref NumberPatternStringProperties localPatternProperties, ref NumberPatternStringProperties contextProperties)
        {
            return currentSetting == (contextProperties.maximumFractionDigits ?? -1) // -1 is just here to force a non-match. It has no other meaning and it should never happen.
                ? localPatternProperties.maximumFractionDigits ?? contextProperties.maximumFractionDigits ?? currentSetting
                : currentSetting; // If the current setting was changed by the user, it is authoritive.
        }

        public static int[] GetEffectiveGroupingSizes(int[] currentSetting, ref NumberPatternStringProperties localPatternProperties, ref NumberPatternStringProperties contextProperties)
        {
            return currentSetting == (contextProperties.groupingSizes ?? new int[0]) // new int[0] is just here to force a non-match. It has no other meaning and it should never happen.
                ? localPatternProperties.groupingSizes ?? contextProperties.groupingSizes ?? currentSetting
                : currentSetting; // If the current setting was changed by the user, it is authoritive.
        }

    }
#nullable restore
}
