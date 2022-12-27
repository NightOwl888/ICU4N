using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using static ICU4N.Util.Currency;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Numerics
{
    using BigDecimal = BigMath.BigDecimal;
    using BigInteger = BigMath.BigInteger;


    /// <summary>
    /// A parser designed to convert an arbitrary human-generated string to its best representation as a
    /// number: a <see cref="long"/>, a <see cref="BigInteger"/>, or a <see cref="BigDecimal"/>.
    /// <para/>
    /// The parser may traverse multiple parse paths in the same strings if there is ambiguity. For
    /// example, the string "12,345.67" has two main interpretations: it could be "12.345" in a locale
    /// that uses '.' as the grouping separator, or it could be "12345.67" in a locale that uses ',' as
    /// the grouping separator. Since the second option has a longer parse path (consumes more of the
    /// input string), the parser will accept the second option.
    /// </summary>
    internal class Parser // ICU4N TODO: API - this was public in ICU4J
    {
        /// <summary>
        /// Controls the set of rules for parsing a string.
        /// </summary>
        public enum ParseMode
        {
            /// <summary>
            /// Lenient mode should be used if you want to accept malformed user input. It will use
            /// heuristics to attempt to parse through typographical errors in the string.
            /// </summary>
            Lenient,

            /// <summary>
            /// Strict mode should be used if you want to require that the input is well-formed. More
            /// specifically, it differs from lenient mode in the following ways:
            /// 
            /// <list type="bullet">
            ///     <item><description>Grouping widths must match the grouping settings. For example, "12,3,45" will fail if
            ///         the grouping width is 3, as in the pattern "#,##0".</description></item>
            ///     <item><description>The string must contain a complete prefix and suffix. For example, if the pattern is
            ///         "{#};(#)", then "{123}" or "(123)" would match, but "{123", "123}", and "123" would all
            ///         fail. (The latter strings would be accepted in lenient mode.)</description></item>
            ///     <item><description>Whitespace may not appear at arbitrary places in the string. In lenient mode,
            ///         whitespace is allowed to occur arbitrarily before and after prefixes and exponent
            ///         separators.</description></item>
            ///     <item><description>Leading grouping separators are not allowed, as in ",123".</description></item>
            ///     <item><description>Minus and plus signs can only appear if specified in the pattern. In lenient mode, a
            ///         plus or minus sign can always precede a number.</description></item>
            ///     <item><description>The set of characters that can be interpreted as a decimal or grouping separator is
            ///         smaller.</description></item>
            ///     <item><description><strong>If currency parsing is enabled,</strong> currencies must only appear where
            ///         specified in either the current pattern string or in a valid pattern string for the
            ///         current locale. For example, if the pattern is "¤0.00", then "$1.23" would match, but
            ///         "1.23$" would fail to match.</description></item>
            /// </list>
            /// </summary>
            Strict,

            /// <summary>
            /// Fast mode should be used in applications that don't require prefixes and suffixes to match.
            /// <para/>
            /// In addition to ignoring prefixes and suffixes, fast mode performs the following
            /// optimizations:
            /// 
            /// <list type="bullet">
            ///     <item><description>Ignores digit strings from <see cref="DecimalFormatSymbols"/> and only uses the code point's
            ///         Unicode digit property. If you are not using custom digit strings, this should not
            ///         cause a change in behavior.</description></item>
            ///     <item><description>Instead of traversing multiple possible parse paths, a "greedy" parsing strategy is
            ///         used, which might mean that fast mode won't accept strings that lenient or strict mode
            ///         would accept. Since prefix and suffix strings are ignored, this is not an issue unless
            ///         you are using custom symbols.</description></item>
            /// </list>
            /// </summary>
            Fast,
        }

        /// <summary>
        /// An enum containing the choices for strategy in parsing when choosing between grouping and
        /// decimal separators.
        /// </summary>
        public enum GroupingMode
        {
            /// <summary>
            /// Accept decimal equivalents as decimals, and if that fails, accept all equivalence classes
            /// (periods, commas, and whitespace-like) as grouping. This is a more lenient strategy.
            /// <para/>
            /// For example, if the formatter's current locale is <em>fr-FR</em>, then "1.234" will parse
            /// as 1234, even though <em>fr-FR</em> does not use a period as the grouping separator.
            /// </summary>
            Default,

            /// <summary>
            /// Accept decimal equivalents as decimals and grouping equivalents as grouping. This strategy is
            /// more strict.
            /// <para/>
            /// For example, if the formatter's current locale is <em>fr-FR</em>, then "1.234" will fail
            /// to parse since <em>fr-FR</em> does not use a period as the grouping separator.
            /// </summary>
            Restricted,
        }

        internal enum StateName
        {
            BeforePrefix,
            AfterPrefix,
            AfterIntegerDigit,
            AfterFractionDigit,
            AfterExponentSeparator,
            AfterExponentDigit,
            BeforeSuffix,
            BeforeSuffixSeenExponent,
            AfterSuffix,
            InsideCurrency,
            InsideDigit,
            InsideString,
            InsideAffixPattern,
        }

        // This set was decided after discussion with icu-design@. See ticket #13309.
        // Zs+TAB is "horizontal whitespace" according to UTS #18 (blank property).
        private static readonly UnicodeSet UNISET_WHITESPACE =
            new UnicodeSet("[[:Zs:][\\u0009]]").Freeze();

        // BiDi characters are skipped over and ignored at any point in the string, even in strict mode.
        private static readonly UnicodeSet UNISET_BIDI =
            new UnicodeSet("[[\\u200E\\u200F\\u061C]]").Freeze();

        // TODO: Re-generate these sets from the database. They probably haven't been updated in a while.
        private static readonly UnicodeSet UNISET_PERIOD_LIKE =
            new UnicodeSet("[.\\u2024\\u3002\\uFE12\\uFE52\\uFF0E\\uFF61]").Freeze();
        private static readonly UnicodeSet UNISET_STRICT_PERIOD_LIKE =
            new UnicodeSet("[.\\u2024\\uFE52\\uFF0E\\uFF61]").Freeze();
        private static readonly UnicodeSet UNISET_COMMA_LIKE =
            new UnicodeSet("[,\\u060C\\u066B\\u3001\\uFE10\\uFE11\\uFE50\\uFE51\\uFF0C\\uFF64]").Freeze();
        private static readonly UnicodeSet UNISET_STRICT_COMMA_LIKE =
            new UnicodeSet("[,\\u066B\\uFE10\\uFE50\\uFF0C]").Freeze();
        private static readonly UnicodeSet UNISET_OTHER_GROUPING_SEPARATORS =
            new UnicodeSet("[\\ '\\u00A0\\u066C\\u2000-\\u200A\\u2018\\u2019\\u202F\\u205F\\u3000\\uFF07]").Freeze();

        // For parse return value calculation.
        internal static readonly BigMath.BigDecimal MIN_LONG_AS_BIG_DECIMAL = new BigMath.BigDecimal(long.MinValue);
        internal static readonly BigMath.BigDecimal MAX_LONG_AS_BIG_DECIMAL = new BigMath.BigDecimal(long.MaxValue);

        internal enum SeparatorType
        {
            CommaLike,
            PeriodLike,
            OtherGrouping,
            Unknown,
        }

        internal static class SeparatorTypeUtil
        {
            internal static SeparatorType FromCp(int cp, ParseMode? mode)
            {
                if (mode == ParseMode.Fast)
                {
                    return SeparatorType.Unknown;
                }
                else if (mode == ParseMode.Strict)
                {
                    if (UNISET_STRICT_COMMA_LIKE.Contains(cp)) return SeparatorType.CommaLike;
                    if (UNISET_STRICT_PERIOD_LIKE.Contains(cp)) return SeparatorType.PeriodLike;
                    if (UNISET_OTHER_GROUPING_SEPARATORS.Contains(cp)) return SeparatorType.OtherGrouping;
                    return SeparatorType.Unknown;
                }
                else
                {
                    if (UNISET_COMMA_LIKE.Contains(cp)) return SeparatorType.CommaLike;
                    if (UNISET_PERIOD_LIKE.Contains(cp)) return SeparatorType.PeriodLike;
                    if (UNISET_OTHER_GROUPING_SEPARATORS.Contains(cp)) return SeparatorType.OtherGrouping;
                    return SeparatorType.Unknown;
                }
            }
        }


        internal enum DigitType
        {
            Integer,
            Fraction,
            Exponent,
        }

        /// <summary>
        /// Holds a snapshot in time of a single parse path. This includes the digits seen so far, the
        /// current state name, and other properties like the grouping separator used on this parse path,
        /// details about the exponent and negative signs, etc.
        /// </summary>
        internal class StateItem
        {
            // Parser state:
            // The "trailingChars" is used to keep track of how many characters from the end of the string
            // are ignorable and should be removed from the parse position should this item be accepted.
            // The "score" is used to help rank two otherwise equivalent parse paths. Currently, the only
            // function giving points to the score is prefix/suffix.
            internal StateName? name;
            internal int trailingCount;
            internal int score;

            // Numerical value:
            internal DecimalQuantity_DualStorageBCD fq = new DecimalQuantity_DualStorageBCD();
            internal int numDigits;
            internal int trailingZeros;
            internal int exponent;

            // Other items that we've seen:
            internal int groupingCp;
            internal long groupingWidths;
            internal string isoCode;
            internal bool sawNegative;
            internal bool sawNegativeExponent;
            internal bool sawCurrency;
            internal bool sawNaN;
            internal bool sawInfinity;
            internal AffixHolder affix;
            internal bool sawPrefix;
            internal bool sawSuffix;
            internal bool sawDecimalPoint;
            internal bool sawExponentDigit;

            // Data for intermediate parsing steps:
            internal StateName? returnTo1;
            internal StateName? returnTo2;
            // For string literals:
            internal string currentString;
            internal int currentOffset;
            internal bool currentTrailing;
            // For affix patterns:
            internal string currentAffixPattern;
            internal long currentStepwiseParserTag;
            // For currency:
            internal TextTrieMap<CurrencyStringInfo>.ParseState currentCurrencyTrieState;
            // For multi-code-point digits:
            internal TextTrieMap<byte>.ParseState currentDigitTrieState;
            internal DigitType? currentDigitType;

            // Identification for path tracing:
            internal readonly char id;
            internal string path;

            internal StateItem(char _id)
            {
                id = _id;
            }

            /// <summary>
            /// Clears the instance so that it can be re-used.
            /// </summary>
            /// <returns>Myself, for chaining.</returns>
            internal StateItem Clear()
            {
                // Parser state:
                name = StateName.BeforePrefix;
                trailingCount = 0;
                score = 0;

                // Numerical value:
                fq.Clear();
                numDigits = 0;
                trailingZeros = 0;
                exponent = 0;

                // Other items we've seen:
                groupingCp = -1;
                groupingWidths = 0L;
                isoCode = null;
                sawNegative = false;
                sawNegativeExponent = false;
                sawCurrency = false;
                sawNaN = false;
                sawInfinity = false;
                affix = null;
                sawPrefix = false;
                sawSuffix = false;
                sawDecimalPoint = false;
                sawExponentDigit = false;

                // Data for intermediate parsing steps:
                returnTo1 = null;
                returnTo2 = null;
                currentString = null;
                currentOffset = 0;
                currentTrailing = false;
                currentAffixPattern = null;
                currentStepwiseParserTag = 0L;
                currentCurrencyTrieState = null;
                currentDigitTrieState = null;
                currentDigitType = null;

                // Identification for path tracing:
                // id is constant and is not cleared
                path = "";

                return this;
            }

            /// <summary>
            /// Sets the internal value of this instance equal to another instance.
            /// <para/>
            /// <paramref name="newName"/> and <paramref name="other"/> are required as parameters to this
            /// function because every time a code point is consumed and a state item is copied, both of
            /// the corresponding fields should be updated; it would be an error if they weren't updated.
            /// </summary>
            /// <param name="other">The instance to copy from.</param>
            /// <param name="newName">The state name that the new copy should take on.</param>
            /// <param name="trailing">If positive, record this code point as trailing; if negative, reset the
            /// trailing count to zero.</param>
            /// <returns>Myself, for chaining.</returns>
            internal StateItem CopyFrom(StateItem other, StateName? newName, int trailing)
            {
                // Parser state:
                name = newName;
                score = other.score;

                // Either reset trailingCount or add the width of the current code point.
                trailingCount = (trailing < 0) ? 0 : other.trailingCount + Character.CharCount(trailing);

                // Numerical value:
                fq.CopyFrom(other.fq);
                numDigits = other.numDigits;
                trailingZeros = other.trailingZeros;
                exponent = other.exponent;

                // Other items we've seen:
                groupingCp = other.groupingCp;
                groupingWidths = other.groupingWidths;
                isoCode = other.isoCode;
                sawNegative = other.sawNegative;
                sawNegativeExponent = other.sawNegativeExponent;
                sawCurrency = other.sawCurrency;
                sawNaN = other.sawNaN;
                sawInfinity = other.sawInfinity;
                affix = other.affix;
                sawPrefix = other.sawPrefix;
                sawSuffix = other.sawSuffix;
                sawDecimalPoint = other.sawDecimalPoint;
                sawExponentDigit = other.sawExponentDigit;

                // Data for intermediate parsing steps:
                returnTo1 = other.returnTo1;
                returnTo2 = other.returnTo2;
                currentString = other.currentString;
                currentOffset = other.currentOffset;
                currentTrailing = other.currentTrailing;
                currentAffixPattern = other.currentAffixPattern;
                currentStepwiseParserTag = other.currentStepwiseParserTag;
                currentCurrencyTrieState = other.currentCurrencyTrieState;
                currentDigitTrieState = other.currentDigitTrieState;
                currentDigitType = other.currentDigitType;

                // Record source node if debugging
                if (DEBUGGING)
                {
                    path = other.path + other.id;
                }

                return this;
            }

            /// <summary>
            /// Adds a digit to the internal representation of this instance.
            /// </summary>
            /// <param name="digit">The digit that was read from the string.</param>
            /// <param name="type">Whether the digit occured after the decimal point.</param>
            internal void AppendDigit(byte digit, DigitType? type)
            {
                if (type == DigitType.Exponent)
                {
                    sawExponentDigit = true;
                    int newExponent = exponent * 10 + digit;
                    if (newExponent < exponent)
                    {
                        // overflow
                        exponent = int.MaxValue;
                    }
                    else
                    {
                        exponent = newExponent;
                    }
                }
                else
                {
                    numDigits++;
                    if (type == DigitType.Fraction && digit == 0)
                    {
                        trailingZeros++;
                    }
                    else if (type == DigitType.Fraction)
                    {
                        fq.AppendDigit(digit, trailingZeros, false);
                        trailingZeros = 0;
                    }
                    else
                    {
                        fq.AppendDigit(digit, 0, true);
                    }
                }
            }

            /// <summary>
            /// Gets whether or not this item contains a valid number.
            /// </summary>
            public bool HasNumber => numDigits > 0 || sawNaN || sawInfinity;

            /// <summary>
            /// Converts the internal digits from this instance into a <see cref="J2N.Numerics.Number"/>, preferring a <see cref="Long"/>, then a
            /// <see cref="BigInteger"/>, then a <see cref="BigDecimal"/>. A <see cref="Double"/> is used for NaN, infinity, and -0.0.
            /// </summary>
            /// <param name="properties"></param>
            /// <returns>The <see cref="J2N.Numerics.Number"/>. Never null.</returns>
            internal Number ToNumber(DecimalFormatProperties properties) // ICU4N TODO: Ideally, the decision about which type to parse into will be delegated to the user.
            {
                // Check for NaN, infinity, and -0.0
                if (sawNaN)
                {
                    return Double.GetInstance(double.NaN);
                }
                if (sawInfinity)
                {
                    if (sawNegative)
                    {
                        return Double.GetInstance(double.NegativeInfinity);
                    }
                    else
                    {
                        return Double.GetInstance(double.PositiveInfinity);
                    }
                }
                if (fq.IsZero && sawNegative)
                {
                    return Double.GetInstance(-0.0);
                }

                // Check for exponent overflow
                bool forceBigDecimal = properties.ParseToBigDecimal;
                if (exponent == int.MaxValue)
                {
                    if (sawNegativeExponent && sawNegative)
                    {
                        return Double.GetInstance(-0.0);
                    }
                    else if (sawNegativeExponent)
                    {
                        return Double.GetInstance(0.0);
                    }
                    else if (sawNegative)
                    {
                        return Double.GetInstance(double.NegativeInfinity);
                    }
                    else
                    {
                        return Double.GetInstance(double.PositiveInfinity);
                    }
                }
                else if (exponent > 1000)
                {
                    // BigDecimals can handle huge values better than BigIntegers.
                    forceBigDecimal = true;
                }

                // Multipliers must be applied in reverse.
                BigMath.BigDecimal multiplier = properties.Multiplier;
                if (properties.MagnitudeMultiplier != 0)
                {
                    if (multiplier == null) multiplier = BigMath.BigDecimal.One;
                    //multiplier = multiplier.ScaleByPowerOfTen(properties.MagnitudeMultiplier);
                    multiplier = BigMath.BigMath.ScaleByPowerOfTen(multiplier, properties.MagnitudeMultiplier);
                }
                int delta = (sawNegativeExponent ? -1 : 1) * exponent;

                // We need to use a math context in order to prevent non-terminating decimal expansions.
                // This is only used when dividing by the multiplier.
                BigMath.MathContext mc = RoundingUtils.GetMathContextOr34Digits(properties);

                // Construct the output number.
                // This is the only step during fast-mode parsing that incurs object creations.
                BigMath.BigDecimal result = fq.ToBigDecimal();
                if (sawNegative) result = -result;
                ////Deveel.Math.BigDecimal foo = new Deveel.Math.BigDecimal(9);

                result = BigMath.BigMath.ScaleByPowerOfTen(result, delta);
                if (multiplier != null)
                {
                    result = BigMath.BigMath.Divide(result, multiplier, mc);
                }
                result = BigMath.BigMath.StripTrailingZeros(result);


                //result = result.ScaleByPowerOfTen(delta);
                //if (multiplier != null)
                //{
                //    result = result.Divide(multiplier, mc);
                //}
                //result = result.StripTrailingZeros();
                if (forceBigDecimal || result.Scale > 0)
                {
                    return result;
                }
                else if (result.CompareTo(MIN_LONG_AS_BIG_DECIMAL) >= 0
                    && result.CompareTo(MAX_LONG_AS_BIG_DECIMAL) <= 0)
                {
                    return Long.GetInstance(result.ToInt64Exact());
                }
                else
                {
                    return result.ToBigIntegerExact();
                }
            }

            /// <summary>
            /// Converts the internal digits to a number, and also associates the number with the parsed
            /// currency.
            /// </summary>
            /// <param name="properties"></param>
            /// <returns>The CurrencyAmount. Never null.</returns>
            public CurrencyAmount ToCurrencyAmount(DecimalFormatProperties properties)
            {
                Debug.Assert(isoCode != null);
                J2N.Numerics.Number number = ToNumber(properties);
                Currency currency = Currency.GetInstance(isoCode);
                return new CurrencyAmount(number, currency);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                sb.Append(path);
                sb.Append("] ");
                //sb.Append(name.name());
                sb.Append(name?.ToString() ?? "null");
                if (name == StateName.InsideString)
                {
                    sb.Append("{");
                    sb.Append(currentString);
                    sb.Append(":");
                    sb.Append(currentOffset);
                    sb.Append("}");
                }
                if (name == StateName.InsideAffixPattern)
                {
                    sb.Append("{");
                    sb.Append(currentAffixPattern);
                    sb.Append(":");
                    sb.Append(AffixUtils.GetOffset(currentStepwiseParserTag) - 1);
                    sb.Append("}");
                }
                sb.Append(" ");
                sb.Append(fq.ToBigDecimal());
                sb.Append(" grouping:");
                sb.Append(groupingCp == -1 ? new char[] { '?' } : Character.ToChars(groupingCp));
                sb.Append(" widths:");
                sb.Append(groupingWidths.ToHexString());
                sb.Append(" seen:");
                sb.Append(sawNegative ? 1 : 0);
                sb.Append(sawNegativeExponent ? 1 : 0);
                sb.Append(sawNaN ? 1 : 0);
                sb.Append(sawInfinity ? 1 : 0);
                sb.Append(sawPrefix ? 1 : 0);
                sb.Append(sawSuffix ? 1 : 0);
                sb.Append(sawDecimalPoint ? 1 : 0);
                sb.Append(" trailing:");
                sb.Append(trailingCount);
                sb.Append(" score:");
                sb.Append(score);
                sb.Append(" affix:");
                sb.Append(affix);
                sb.Append(" currency:");
                sb.Append(isoCode);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Holds an ordered list of <see cref="StateItem"/> and other metadata about the string to be parsed.
        /// There are two internal arrays of <see cref="StateItem"/>, which are swapped back and forth in order
        /// to avoid object creations. The items in one array can be populated at the same time that items
        /// in the other array are being read from.
        /// </summary>
        internal class ParserState
        {

            // Basic ParserStateItem lists:
            internal StateItem[] items = new StateItem[16];
            internal StateItem[] prevItems = new StateItem[16];
            internal int length;
            internal int prevLength;

            // Properties and Symbols memory:
            internal DecimalFormatProperties properties;
            internal DecimalFormatSymbols symbols;
            internal ParseMode? mode;
            internal bool caseSensitive;
            internal bool parseCurrency;
            internal GroupingMode? groupingMode;

            // Other pre-computed fields:
            internal int decimalCp1;
            internal int decimalCp2;
            internal int groupingCp1;
            internal int groupingCp2;
            internal SeparatorType decimalType1;
            internal SeparatorType decimalType2;
            internal SeparatorType groupingType1;
            internal SeparatorType groupingType2;

            internal TextTrieMap<byte> digitTrie;
            internal ISet<AffixHolder> affixHolders = new HashSet<AffixHolder>();

            internal ParserState()
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new StateItem((char)('A' + i));
                    prevItems[i] = new StateItem((char)('A' + i));
                }
            }

            /// <summary>
            /// Clears the internal state in order to prepare for parsing a new string.
            /// </summary>
            /// <returns>Myself, for chaining.</returns>
            internal ParserState Clear()
            {
                length = 0;
                prevLength = 0;
                digitTrie = null;
                affixHolders.Clear();
                return this;
            }

            /// <summary>
            /// Swaps the internal arrays of <see cref="StateItem"/>. Sets the length of the primary list to zero,
            /// so that it can be appended to.
            /// </summary>
            internal void Swap()
            {
                StateItem[] temp = prevItems;
                prevItems = items;
                items = temp;
                prevLength = length;
                length = 0;
            }

            /// <summary>
            /// Swaps the internal arrays of <see cref="StateItem"/>. Sets the length of the primary list to the
            /// length of the previous list, so that it can be read from.
            /// </summary>
            internal void SwapBack()
            {
                StateItem[] temp = prevItems;
                prevItems = items;
                items = temp;
                length = prevLength;
                prevLength = 0;
            }

            /// <summary>
            /// Gets the next available <see cref="StateItem"/> from the primary list for writing. This method
            /// should be thought of like a list append method, except that there are no object creations
            /// taking place.
            /// <para/>
            /// It is the caller's responsibility to call either <see cref="StateItem.Clear()"/> or
            /// <see cref="StateItem.CopyFrom(StateItem, StateName?, int)"/> on the returned object.
            /// </summary>
            /// <returns>A dirty <see cref="StateItem"/>.</returns>
            internal StateItem GetNext()
            {
                if (length >= items.Length)
                {
                    // TODO: What to do here? Expand the array?
                    // This case is rare and would happen only with specially designed input.
                    // For now, just overwrite the last entry.
                    length = items.Length - 1;
                }
                StateItem item = items[length];
                length++;
                return item;
            }

            /// <summary>The index of the last inserted StateItem via a call to <see cref="GetNext()"/>.</summary>
            public int LastInsertedIndex
            {
                get
                {
                    Debug.Assert(length > 0);
                    return length - 1;
                }
            }

            /// <summary>
            /// Gets a <see cref="StateItem"/> from the primary list. Assumes that the item has already been added
            /// via a call to <see cref="GetNext()"/>.
            /// </summary>
            /// <param name="index">The index of the item to get.</param>
            /// <returns>The item.</returns>
            public StateItem GetItem(int index) // ICU4N TODO: API - Make into indexer?
            {
                Debug.Assert(index >= 0 && index < length);
                return items[index];
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<ParseState mode:");
                sb.Append(mode);
                sb.Append(" caseSensitive:");
                sb.Append(caseSensitive);
                sb.Append(" parseCurrency:");
                sb.Append(parseCurrency);
                sb.Append(" groupingMode:");
                sb.Append(groupingMode);
                sb.Append(" decimalCps:");
                sb.Append((char)decimalCp1);
                sb.Append((char)decimalCp2);
                sb.Append(" groupingCps:");
                sb.Append((char)groupingCp1);
                sb.Append((char)groupingCp2);
                sb.Append(" affixes:");
                sb.Append(affixHolders);
                sb.Append(">");
                return sb.ToString();
            }
        }

        /// <summary>
        /// A wrapper for affixes. Affixes can be string-based or pattern-based, and they can come from
        /// several sources, including the property bag and the locale paterns from CLDR data.
        /// </summary>
        internal class AffixHolder
        {
            internal readonly string p; // prefix
            internal readonly string s; // suffix
            internal readonly bool strings;
            internal readonly bool negative;

            internal static readonly AffixHolder EMPTY_POSITIVE = new AffixHolder("", "", true, false);
            internal static readonly AffixHolder EMPTY_NEGATIVE = new AffixHolder("", "", true, true);

            internal static void AddToState(ParserState state, DecimalFormatProperties properties)
            {
                AffixHolder pp = FromPropertiesPositivePattern(properties);
                AffixHolder np = FromPropertiesNegativePattern(properties);
                AffixHolder ps = FromPropertiesPositiveString(properties);
                AffixHolder ns = FromPropertiesNegativeString(properties);
                if (pp != null) state.affixHolders.Add(pp);
                if (ps != null) state.affixHolders.Add(ps);
                if (np != null) state.affixHolders.Add(np);
                if (ns != null) state.affixHolders.Add(ns);
            }

            internal static AffixHolder FromPropertiesPositivePattern(DecimalFormatProperties properties)
            {
                string ppp = properties.PositivePrefixPattern;
                string psp = properties.PositiveSuffixPattern;
                if (properties.SignAlwaysShown)
                {
                    // TODO: This logic is somewhat duplicated from MurkyModifier.
                    bool foundSign = false;
                    string npp = properties.NegativePrefixPattern;
                    string nsp = properties.NegativeSuffixPattern;
                    if (AffixUtils.ContainsType(npp, AffixUtils.Type.MinusSign))
                    {
                        foundSign = true;
                        ppp = AffixUtils.ReplaceType(npp, AffixUtils.Type.MinusSign, '+');
                    }
                    if (AffixUtils.ContainsType(nsp, AffixUtils.Type.MinusSign))
                    {
                        foundSign = true;
                        psp = AffixUtils.ReplaceType(nsp, AffixUtils.Type.MinusSign, '+');
                    }
                    if (!foundSign)
                    {
                        ppp = "+" + ppp;
                    }
                }
                return GetInstance(ppp, psp, false, false);
            }

            internal static AffixHolder FromPropertiesNegativePattern(DecimalFormatProperties properties)
            {
                string npp = properties.NegativePrefixPattern;
                string nsp = properties.NegativeSuffixPattern;
                if (npp == null && nsp == null)
                {
                    npp = properties.PositivePrefixPattern;
                    nsp = properties.PositiveSuffixPattern;
                    if (npp == null)
                    {
                        npp = "-";
                    }
                    else
                    {
                        npp = "-" + npp;
                    }
                }
                return GetInstance(npp, nsp, false, true);
            }

            internal static AffixHolder FromPropertiesPositiveString(DecimalFormatProperties properties)
            {
                string pp = properties.PositivePrefix;
                string ps = properties.PositiveSuffix;
                if (pp == null && ps == null) return null;
                return GetInstance(pp, ps, true, false);
            }

            internal static AffixHolder FromPropertiesNegativeString(DecimalFormatProperties properties)
            {
                string np = properties.NegativePrefix;
                string ns = properties.NegativeSuffix;
                if (np == null && ns == null) return null;
                return GetInstance(np, ns, true, true);
            }

            internal static AffixHolder GetInstance(string p, string s, bool strings, bool negative)
            {
                if (p == null && s == null) return negative ? EMPTY_NEGATIVE : EMPTY_POSITIVE;
                if (p == null) p = "";
                if (s == null) s = "";
                if (p.Length == 0 && s.Length == 0) return negative ? EMPTY_NEGATIVE : EMPTY_POSITIVE;
                return new AffixHolder(p, s, strings, negative);
            }

            internal AffixHolder(string pp, string sp, bool strings, bool negative)
            {
                this.p = pp;
                this.s = sp;
                this.strings = strings;
                this.negative = negative;
            }

            public override bool Equals(object other)
            {
                if (other == null) return false;
                if (this == other) return true;
                if (!(other is AffixHolder _other)) return false;
                if (!p.Equals(_other.p, StringComparison.Ordinal)) return false;
                if (!s.Equals(_other.s, StringComparison.Ordinal)) return false;
                if (strings != _other.strings) return false;
                if (negative != _other.negative) return false;
                return true;
            }

            public override int GetHashCode()
            {
                return p.GetHashCode() ^ s.GetHashCode();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append(p);
                sb.Append("|");
                sb.Append(s);
                sb.Append("|");
                sb.Append(strings ? 'S' : 'P');
                sb.Append("}");
                return sb.ToString();
            }
        }

        /// <summary>
        /// A class that holds information about all currency affix patterns for the locale. This allows
        /// the parser to accept currencies in any format that are valid for the locale.
        /// </summary>
        private class CurrencyAffixPatterns
        {
            private readonly ISet<AffixHolder> set = new HashSet<AffixHolder>();

            private static readonly ConcurrentDictionary<UCultureInfo, CurrencyAffixPatterns> currencyAffixPatterns =
                new ConcurrentDictionary<UCultureInfo, CurrencyAffixPatterns>();

            internal static void AddToState(UCultureInfo uloc, ParserState state)
            {
                CurrencyAffixPatterns value = currencyAffixPatterns.GetOrAdd(uloc, (key) => new CurrencyAffixPatterns(key));

                //CurrencyAffixPatterns value = currencyAffixPatterns.get(uloc);
                //if (value == null)
                //{
                //    // There can be multiple threads computing the same CurrencyAffixPatterns simultaneously,
                //    // but that scenario is harmless.
                //    CurrencyAffixPatterns newValue = new CurrencyAffixPatterns(uloc);
                //    currencyAffixPatterns.putIfAbsent(uloc, newValue);
                //    value = currencyAffixPatterns.get(uloc);
                //}
                state.affixHolders.UnionWith(value.set);
            }

            private CurrencyAffixPatterns(UCultureInfo uloc)
            {
                // Get the basic currency pattern.
                string pattern = NumberFormat.GetPatternForStyle(uloc, NumberFormatStyle.CurrencyStyle);
                AddPattern(pattern);

                // Get the currency plural patterns.
                // TODO: Update this after CurrencyPluralInfo is replaced.
                CurrencyPluralInfo pluralInfo = CurrencyPluralInfo.GetInstance(uloc);
                foreach (StandardPlural plural in Enum.GetValues(typeof(StandardPlural))) // ICU4N TODO: don't loop on Enum.GetValues - it is slow
                {
                    pattern = pluralInfo.GetCurrencyPluralPattern(plural.GetKeyword());
                    AddPattern(pattern);
                }
            }



            private static readonly ThreadLocal<DecimalFormatProperties> threadLocalProperties =
                new ThreadLocal<DecimalFormatProperties>(() => new DecimalFormatProperties()); // ICU4N TODO: Dispose()
            //    {
            //      @Override
            //          protected DecimalFormatProperties initialValue()
            //    {
            //        return new DecimalFormatProperties();
            //    }
            //};

            private void AddPattern(string pattern)
            {
                DecimalFormatProperties properties = threadLocalProperties.Value;
                try
                {
                    PatternStringParser.ParseToExistingProperties(pattern, properties);
                }
                catch (ArgumentException e)
                {
                    // This should only happen if there is a bug in CLDR data. Fail silently.
                }
                set.Add(AffixHolder.FromPropertiesPositivePattern(properties));
                set.Add(AffixHolder.FromPropertiesNegativePattern(properties));
            }
        }

        /// <summary>
        /// Makes a {@link TextTrieMap} for parsing digit strings. A trie is required only if the digit
        /// strings are longer than one code point. In order for this to be the case, the user would have
        /// needed to specify custom multi-character digits, like "(0)".
        /// </summary>
        /// <param name="digitStrings">The list of digit strings from <see cref="DecimalFormatSymbols"/>.</param>
        /// <returns>A trie, or <c>null</c> if a trie is not required.</returns>
        private static TextTrieMap<byte> MakeDigitTrie(string[] digitStrings)
        {
            bool requiresTrie = false;
            for (int i = 0; i < 10; i++)
            {
                string str = digitStrings[i];
                if (Character.CharCount(Character.CodePointAt(str, 0)) != str.Length)
                {
                    requiresTrie = true;
                    break;
                }
            }
            if (!requiresTrie) return null;

            // TODO: Consider caching the tries so they don't need to be re-created run to run.
            // (Low-priority since multi-character digits are rare in practice)
            TextTrieMap<byte> trieMap = new TextTrieMap<byte>(false);
            for (int i = 0; i < 10; i++)
            {
                trieMap.Put(digitStrings[i], (byte)i);
            }
            return trieMap;
        }

        protected static readonly ThreadLocal<ParserState> threadLocalParseState =
            new ThreadLocal<ParserState>(() => new ParserState()); // ICU4N TODO: Dispose()

        //    {
        //        @Override
        //        protected ParserState initialValue()
        //{
        //    return new ParserState();
        //}
        //      };

        protected static readonly ThreadLocal<ParsePosition> threadLocalParsePosition =
            new ThreadLocal<ParsePosition>(() => new ParsePosition(0)); // ICU4N TODO: Dispose()

        //    {
        //        @Override
        //        protected ParsePosition initialValue()
        //{
        //    return new ParsePosition(0);
        //}
        //      };

        /**
         * @internal
         * @deprecated This API is ICU internal only. 
         */

        // TODO: Remove this set from ScientificNumberFormat.
        [Obsolete("This API is ICU internal only.")]
        public static readonly UnicodeSet UNISET_PLUS =
          new UnicodeSet(
                  0x002B, 0x002B, 0x207A, 0x207A, 0x208A, 0x208A, 0x2795, 0x2795, 0xFB29, 0xFB29,
                  0xFE62, 0xFE62, 0xFF0B, 0xFF0B)
              .Freeze();

        /**
         * @internal
         * @deprecated This API is ICU internal only. TODO: Remove this set from ScientificNumberFormat.
         */
        // TODO: Remove this set from ScientificNumberFormat.
        [Obsolete("This API is ICU internal only.")]
        public static readonly UnicodeSet UNISET_MINUS =
          new UnicodeSet(
                  0x002D, 0x002D, 0x207B, 0x207B, 0x208B, 0x208B, 0x2212, 0x2212, 0x2796, 0x2796,
                  0xFE63, 0xFE63, 0xFF0D, 0xFF0D)
              .Freeze();

        public static Number Parse(string input, DecimalFormatProperties properties, DecimalFormatSymbols symbols)
        {
            ParsePosition ppos = threadLocalParsePosition.Value;
            ppos.Index = 0;
            return Parse(input, ppos, properties, symbols);
        }

        // TODO: DELETE ME once debugging is finished
        public static volatile bool DEBUGGING = false;

        /**
         * Implements an iterative parser that maintains a lists of possible states at each code point in
         * the string. At each code point in the string, the list of possible states is updated based on
         * the states coming from the previous code point. The parser stops when it reaches the end of the
         * string or when there are no possible parse paths remaining in the string.
         *
         * <p>TODO: This API is not fully flushed out. Right now this is internal-only.
         *
         * @param input The string to parse.
         * @param ppos A {@link ParsePosition} to hold the index at which parsing stopped.
         * @param properties A property bag, used only for determining the prefix/suffix strings and the
         *     padding character.
         * @param symbols A {@link DecimalFormatSymbols} object, used for determining locale-specific
         *     symbols for grouping/decimal separators, digit strings, and prefix/suffix substitutions.
         * @return A Number matching the parser's best interpretation of the string.
         */
        // ICU4N TODO: API Revisit making this a ReadOnlySpan<char> and accepting both string and ReadOnlySpan<char> on the API.
        public static Number Parse(
            string input,
            ParsePosition ppos,
            DecimalFormatProperties properties,
            DecimalFormatSymbols symbols)
        {
            StateItem best = ParseImpl(input, ppos, false, properties, symbols);
            return (best == null) ? null : best.ToNumber(properties);
        }

        public static CurrencyAmount ParseCurrency(
            string input, DecimalFormatProperties properties, DecimalFormatSymbols symbols) //throws ParseException
        {
            return ParseCurrency(input, null, properties, symbols);
        }

        // ICU4N TODO: API Revisit making this a ReadOnlySpan<char> and accepting both string and ReadOnlySpan<char> on the API.
        public static CurrencyAmount ParseCurrency(
            string input, ParsePosition ppos, DecimalFormatProperties properties, DecimalFormatSymbols symbols)
        // throws ParseException
        {
            if (ppos == null)
            {
                ppos = threadLocalParsePosition.Value;
                ppos.Index = 0;
                ppos.ErrorIndex = -1;
            }
            StateItem best = ParseImpl(input, ppos, true, properties, symbols);
            return (best == null) ? null : best.ToCurrencyAmount(properties);
        }

        private static StateItem ParseImpl(
            string input, // ICU4N TODO: API Revisit making this a ReadOnlySpan<char> and accepting both string and ReadOnlySpan<char> on the API.
            ParsePosition ppos,
            bool parseCurrency,
            DecimalFormatProperties properties,
            DecimalFormatSymbols symbols)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input), "All arguments are required for parse.");
            if (ppos is null)
                throw new ArgumentNullException(nameof(ppos), "All arguments are required for parse.");
            if (properties is null)
                throw new ArgumentNullException(nameof(properties), "All arguments are required for parse.");
            if (symbols is null)
                throw new ArgumentNullException(nameof(symbols), "All arguments are required for parse.");

            ParseMode? mode = properties.ParseMode;
            if (mode == null) mode = ParseMode.Lenient;
            bool integerOnly = properties.ParseIntegerOnly;
            bool ignoreExponent = properties.ParseNoExponent;
            bool ignoreGrouping = properties.GroupingSize < 0;

            // Set up the initial state
            ParserState state = threadLocalParseState.Value.Clear();
            state.properties = properties;
            state.symbols = symbols;
            state.mode = mode;
            state.parseCurrency = parseCurrency;
            state.groupingMode = properties.ParseGroupingMode;
            if (state.groupingMode == null) state.groupingMode = GroupingMode.Default;
            state.caseSensitive = properties.ParseCaseSensitive;
            state.decimalCp1 = Character.CodePointAt(symbols.DecimalSeparatorString, 0);
            state.decimalCp2 = Character.CodePointAt(symbols.MonetaryDecimalSeparatorString, 0);
            state.groupingCp1 = Character.CodePointAt(symbols.GroupingSeparatorString, 0);
            state.groupingCp2 = Character.CodePointAt(symbols.MonetaryGroupingSeparatorString, 0);
            state.decimalType1 = SeparatorTypeUtil.FromCp(state.decimalCp1, mode);
            state.decimalType2 = SeparatorTypeUtil.FromCp(state.decimalCp2, mode);
            state.groupingType1 = SeparatorTypeUtil.FromCp(state.groupingCp1, mode);
            state.groupingType2 = SeparatorTypeUtil.FromCp(state.groupingCp2, mode);
            StateItem initialStateItem = state.GetNext().Clear();
            initialStateItem.name = StateName.BeforePrefix;

            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
            {
                state.digitTrie = MakeDigitTrie(symbols.DigitStringsLocal);
                AffixHolder.AddToState(state, properties);
                if (parseCurrency)
                {
                    CurrencyAffixPatterns.AddToState(symbols.UCulture, state);
                }
            }

            if (DEBUGGING)
            {
                Console.Out.WriteLine("Parsing: " + input);
                Console.Out.WriteLine(properties);
                Console.Out.WriteLine(state);
            }

            // Start walking through the string, one codepoint at a time. Backtracking is not allowed. This
            // is to enforce linear runtime and prevent cases that could result in an infinite loop.
            int offset = ppos.Index;
            for (; offset < input.Length;)
            {
                int cp = Character.CodePointAt(input, offset);
                state.Swap();
                for (int i = 0; i < state.prevLength; i++)
                {
                    StateItem item = state.prevItems[i];
                    if (DEBUGGING)
                    {
                        Console.Out.WriteLine(":" + offset + item.id + " " + item);
                    }

                    // In the switch statement below, if you see a line like:
                    //    if (state.Length > 0 && mode == ParseMode.FAST) break;
                    // it is used for accelerating the fast parse mode. The check is performed only in the
                    // states BEFORE_PREFIX, AFTER_INTEGER_DIGIT, and AFTER_FRACTION_DIGIT, which are the
                    // most common states.

                    switch (item.name)
                    {
                        case StateName.BeforePrefix:
                            // Beginning of string
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptMinusOrPlusSign(cp, StateName.BeforePrefix, state, item, false);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            AcceptIntegerDigit(cp, StateName.AfterIntegerDigit, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptBidi(cp, StateName.BeforePrefix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptWhitespace(cp, StateName.BeforePrefix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptPadding(cp, StateName.BeforePrefix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptNan(cp, StateName.BeforePrefix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptInfinity(cp, StateName.BeforePrefix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            if (!integerOnly)
                            {
                                AcceptDecimalPoint(cp, StateName.AfterFractionDigit, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptPrefix(cp, StateName.AfterPrefix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                if (!ignoreGrouping)
                                {
                                    AcceptGrouping(cp, StateName.AfterIntegerDigit, state, item);
                                    if (state.length > 0 && mode == ParseMode.Fast) break;
                                }
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforePrefix, state, item);
                                }
                            }
                            break;

                        case StateName.AfterPrefix:
                            // Prefix is consumed
                            AcceptBidi(cp, StateName.AfterPrefix, state, item);
                            AcceptPadding(cp, StateName.AfterPrefix, state, item);
                            AcceptNan(cp, StateName.BeforeSuffix, state, item);
                            AcceptInfinity(cp, StateName.BeforeSuffix, state, item);
                            AcceptIntegerDigit(cp, StateName.AfterIntegerDigit, state, item);
                            if (!integerOnly)
                            {
                                AcceptDecimalPoint(cp, StateName.AfterFractionDigit, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.AfterPrefix, state, item);
                                if (!ignoreGrouping)
                                {
                                    AcceptGrouping(cp, StateName.AfterIntegerDigit, state, item);
                                }
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.AfterPrefix, state, item);
                                }
                            }
                            break;

                        case StateName.AfterIntegerDigit:
                            // Previous character was an integer digit (or grouping/whitespace)
                            AcceptIntegerDigit(cp, StateName.AfterIntegerDigit, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            if (!integerOnly)
                            {
                                AcceptDecimalPoint(cp, StateName.AfterFractionDigit, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            if (!ignoreGrouping)
                            {
                                AcceptGrouping(cp, StateName.AfterIntegerDigit, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            AcceptBidi(cp, StateName.BeforeSuffix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptPadding(cp, StateName.BeforeSuffix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            if (!ignoreExponent)
                            {
                                AcceptExponentSeparator(cp, StateName.AfterExponentSeparator, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptSuffix(cp, StateName.AfterSuffix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.BeforeSuffix, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.BeforeSuffix, state, item, false);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforeSuffix, state, item);
                                }
                            }
                            break;

                        case StateName.AfterFractionDigit:
                            // We encountered a decimal point
                            AcceptFractionDigit(cp, StateName.AfterFractionDigit, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptBidi(cp, StateName.BeforeSuffix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            AcceptPadding(cp, StateName.BeforeSuffix, state, item);
                            if (state.length > 0 && mode == ParseMode.Fast) break;
                            if (!ignoreExponent)
                            {
                                AcceptExponentSeparator(cp, StateName.AfterExponentSeparator, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptSuffix(cp, StateName.AfterSuffix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.BeforeSuffix, state, item);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.BeforeSuffix, state, item, false);
                                if (state.length > 0 && mode == ParseMode.Fast) break;
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforeSuffix, state, item);
                                }
                            }
                            break;

                        case StateName.AfterExponentSeparator:
                            AcceptBidi(cp, StateName.AfterExponentSeparator, state, item);
                            AcceptMinusOrPlusSign(cp, StateName.AfterExponentSeparator, state, item, true);
                            AcceptExponentDigit(cp, StateName.AfterExponentDigit, state, item);
                            break;

                        case StateName.AfterExponentDigit:
                            AcceptBidi(cp, StateName.BeforeSuffixSeenExponent, state, item);
                            AcceptPadding(cp, StateName.BeforeSuffixSeenExponent, state, item);
                            AcceptExponentDigit(cp, StateName.AfterExponentDigit, state, item);
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptSuffix(cp, StateName.AfterSuffix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.BeforeSuffixSeenExponent, state, item);
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.BeforeSuffix, state, item, false);
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforeSuffixSeenExponent, state, item);
                                }
                            }
                            break;

                        case StateName.BeforeSuffix:
                            // Accept whitespace, suffixes, and exponent separators
                            AcceptBidi(cp, StateName.BeforeSuffix, state, item);
                            AcceptPadding(cp, StateName.BeforeSuffix, state, item);
                            if (!ignoreExponent)
                            {
                                AcceptExponentSeparator(cp, StateName.AfterExponentSeparator, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptSuffix(cp, StateName.AfterSuffix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.BeforeSuffix, state, item);
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.BeforeSuffix, state, item, false);
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforeSuffix, state, item);
                                }
                            }
                            break;

                        case StateName.BeforeSuffixSeenExponent:
                            // Accept whitespace and suffixes but not exponent separators
                            AcceptBidi(cp, StateName.BeforeSuffixSeenExponent, state, item);
                            AcceptPadding(cp, StateName.BeforeSuffixSeenExponent, state, item);
                            if (mode == ParseMode.Lenient || mode == ParseMode.Strict)
                            {
                                AcceptSuffix(cp, StateName.AfterSuffix, state, item);
                            }
                            if (mode == ParseMode.Lenient || mode == ParseMode.Fast)
                            {
                                AcceptWhitespace(cp, StateName.BeforeSuffixSeenExponent, state, item);
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.BeforeSuffixSeenExponent, state, item, false);
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.BeforeSuffixSeenExponent, state, item);
                                }
                            }
                            break;

                        case StateName.AfterSuffix:
                            if ((mode == ParseMode.Lenient || mode == ParseMode.Fast) && parseCurrency)
                            {
                                // Continue traversing in case there is a currency symbol to consume
                                AcceptBidi(cp, StateName.AfterSuffix, state, item);
                                AcceptPadding(cp, StateName.AfterSuffix, state, item);
                                AcceptWhitespace(cp, StateName.AfterSuffix, state, item);
                                // TODO(sffc): AcceptMinusOrPlusSign(cp, StateName.AFTER_SUFFIX, state, item, false);
                                if (parseCurrency)
                                {
                                    AcceptCurrency(cp, StateName.AfterSuffix, state, item);
                                }
                            }
                            // Otherwise, do not Accept any more characters.
                            break;

                        case StateName.InsideCurrency:
                            AcceptCurrencyOffset(cp, state, item);
                            break;

                        case StateName.InsideDigit:
                            AcceptDigitTrieOffset(cp, state, item);
                            break;

                        case StateName.InsideString:
                            AcceptStringOffset(cp, state, item);
                            break;

                        case StateName.InsideAffixPattern:
                            AcceptAffixPatternOffset(cp, state, item);
                            break;
                    }
                }

                if (state.length == 0)
                {
                    // No parse paths continue past this point. We have found the longest parsable string
                    // from the input. Restore previous state without the offset and break.
                    state.SwapBack();
                    break;
                }

                offset += Character.CharCount(cp);
            }

            // Post-processing
            if (state.length == 0)
            {
                if (DEBUGGING)
                {
                    Console.Out.WriteLine("No matches found");
                    Console.Out.WriteLine("- - - - - - - - - -");
                }
                return null;
            }
            else
            {

                // Loop through the candidates.  "continue" skips a candidate as invalid.
                StateItem best = null;
                //outer:
                for (int i = 0; i < state.length; i++)
                {
                    StateItem item = state.items[i];

                    if (DEBUGGING)
                    {
                        Console.Out.WriteLine(":end " + item);
                    }

                    // Check that at least one digit was read.
                    if (!item.HasNumber)
                    {
                        if (DEBUGGING) Console.Out.WriteLine("-> rejected due to no number value");
                        continue;
                    }

                    if (mode == ParseMode.Strict)
                    {
                        // Perform extra checks for strict mode.
                        // We require that the affixes match.
                        bool sawPrefix = item.sawPrefix || (item.affix != null && item.affix.p == string.Empty);
                        bool sawSuffix = item.sawSuffix || (item.affix != null && item.affix.s == string.Empty);
                        bool hasEmptyAffix =
                            state.affixHolders.Contains(AffixHolder.EMPTY_POSITIVE)
                                || state.affixHolders.Contains(AffixHolder.EMPTY_NEGATIVE);
                        if (sawPrefix && sawSuffix)
                        {
                            // OK
                        }
                        else if (!sawPrefix && !sawSuffix && hasEmptyAffix)
                        {
                            // OK
                        }
                        else
                        {
                            // Has a prefix or suffix that doesn't match
                            if (DEBUGGING) Console.Out.WriteLine("-> rejected due to mismatched prefix/suffix");
                            continue;
                        }

                        // Check for scientific notation.
                        if (properties.MinimumExponentDigits > 0 && !item.sawExponentDigit)
                        {
                            if (DEBUGGING) Console.Out.WriteLine("-> reject due to lack of exponent");
                            continue;
                        }

                        // Check that grouping sizes are valid.
                        int grouping1 = properties.GroupingSize;
                        int grouping2 = properties.SecondaryGroupingSize;
                        grouping1 = grouping1 > 0 ? grouping1 : grouping2;
                        grouping2 = grouping2 > 0 ? grouping2 : grouping1;
                        long groupingWidths = item.groupingWidths;
                        int numGroupingRegions = 16 - groupingWidths.LeadingZeroCount() / 4;
                        // If the last grouping is zero, accept strings like "1," but reject string like "1,.23"
                        // Strip off multiple last-groupings to handle cases like "123,," or "123  "
                        while (numGroupingRegions > 1 && (groupingWidths & 0xf) == 0)
                        {
                            if (item.sawDecimalPoint)
                            {
                                if (DEBUGGING) Console.Out.WriteLine("-> rejected due to decimal point after grouping");
                                //continue outer;
                                goto outer_continue;
                            }
                            else
                            {
                                //groupingWidths >>>= 4;
                                groupingWidths = groupingWidths.TripleShift(4);
                                numGroupingRegions--;
                            }
                        }
                        if (grouping1 < 0)
                        {
                            // OK (no grouping data available)
                        }
                        else if (numGroupingRegions <= 1)
                        {
                            // OK (no grouping digits)
                        }
                        else if ((groupingWidths & 0xf) != grouping1)
                        {
                            // First grouping size is invalid
                            if (DEBUGGING) Console.Out.WriteLine("-> rejected due to first grouping violation");
                            continue;
                        }
                        else if (((groupingWidths.TripleShift((numGroupingRegions - 1) * 4)) & 0xf) > grouping2)
                        {
                            // String like "1234,567" where the highest grouping is too large
                            if (DEBUGGING) Console.Out.WriteLine("-> rejected due to final grouping violation");
                            continue;
                        }
                        else
                        {
                            for (int j = 1; j < numGroupingRegions - 1; j++)
                            {
                                if (((groupingWidths.TripleShift(j * 4)) & 0xf) != grouping2)
                                {
                                    // A grouping size somewhere in the middle is invalid
                                    if (DEBUGGING) Console.Out.WriteLine("-> rejected due to inner grouping violation");
                                    goto outer_continue;
                                }
                            }
                        }
                    }

                    // Optionally require that the presence of a decimal point matches the pattern.
                    if (properties.DecimalPatternMatchRequired
                        && item.sawDecimalPoint
                            != (properties.DecimalSeparatorAlwaysShown
                                || properties.MaximumFractionDigits != 0))
                    {
                        if (DEBUGGING) Console.Out.WriteLine("-> rejected due to decimal point violation");
                        continue;
                    }

                    // When parsing currencies, require that a currency symbol was found.
                    if (parseCurrency && !item.sawCurrency)
                    {
                        if (DEBUGGING) Console.Out.WriteLine("-> rejected due to lack of currency");
                        continue;
                    }

                    // If we get here, then this candidate is acceptable.
                    // Use the earliest candidate in the list, or the one with the highest score, or the
                    // one with the fewest trailing digits.
                    if (best == null)
                    {
                        best = item;
                    }
                    else if (item.score > best.score)
                    {
                        best = item;
                    }
                    else if (item.trailingCount < best.trailingCount)
                    {
                        best = item;
                    }
                outer_continue: { /* Intentionally blank */ }
                }

                if (DEBUGGING)
                {
                    Console.Out.WriteLine("- - - - - - - - - -");
                }

                if (best != null)
                {
                    ppos.Index = offset - best.trailingCount;
                    return best;
                }
                else
                {
                    ppos.ErrorIndex = offset;
                    return null;
                }
            }
        }

        /// <summary>
        /// If <paramref name="cp"/> is whitespace (as determined by the unicode set <see cref="UNISET_WHITESPACE"/>,
        /// copies <paramref name="item"/> to the new list in <paramref name="state"/> and sets its state name to
        /// <paramref name="nextName"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName">The new state name if the check passes.</param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptWhitespace(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            if (UNISET_WHITESPACE.Contains(cp))
            {
                state.GetNext().CopyFrom(item, nextName, cp);
            }
        }

        /// <summary>
        /// If <paramref name="cp"/> is a bidi control character (as determined by the unicode set 
        /// <see cref="UNISET_BIDI"/>, copies <paramref name="item"/> to the new list in <paramref name="state"/> and sets its
        /// state name to <paramref name="nextName"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName">The new state name if the check passes.</param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptBidi(int cp, StateName nextName, ParserState state, StateItem item)
        {
            if (UNISET_BIDI.Contains(cp))
            {
                state.GetNext().CopyFrom(item, nextName, cp);
            }
        }

        /// <summary>
        /// If <paramref name="cp"/> is a padding character (as determined by <see cref="DecimalFormatProperties.PadString"/>,
        /// copies <paramref name="item"/> to the new list in <paramref name="state"/> and sets its state name to
        /// <paramref name="nextName"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName">The new state name if the check passes.</param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptPadding(int cp, StateName nextName, ParserState state, StateItem item)
        {
            string padding = state.properties.PadString;
            if (padding == null || padding.Length == 0) return;
            int referenceCp = Character.CodePointAt(padding, 0);
            if (cp == referenceCp)
            {
                state.GetNext().CopyFrom(item, nextName, cp);
            }
        }

        private static void AcceptIntegerDigit(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            AcceptDigitHelper(cp, nextName, state, item, DigitType.Integer);
        }

        private static void AcceptFractionDigit(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            AcceptDigitHelper(cp, nextName, state, item, DigitType.Fraction);
        }

        private static void AcceptExponentDigit(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            AcceptDigitHelper(cp, nextName, state, item, DigitType.Exponent);
        }
        /**
         * If <code>cp</code> is a digit character (as determined by either {@link UCharacter#digit} or
         * {@link ParserState#digitCps}), copies <code>item</code> to the new list in <code>state</code>
         * and sets its state name to one determined by <code>type</code>. Also copies the digit into a
         * field in the new item determined by <code>type</code>.
         *
         * @param cp The code point to check.
         * @param nextName The state to set if a digit is accepted.
         * @param state The state object to update.
         * @param item The old state leading into the code point.
         * @param type The digit type, which determines the next state and the field into which to insert
         *     the digit.
         */
        /// <summary>
        /// If <paramref name="cp"/> is a digit character (as determined by either <see cref="UChar.Digit(int, int)"/> or
        /// <see cref="DecimalFormatSymbols.DigitStringsLocal"/>, copies <paramref name="item"/> to the new list in <paramref name="state"/>
        /// and sets its state name to one determined by <paramref name="type"/>. Also copies the digit into a
        /// field in the new item determined by <paramref name="type"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName">The state to set if a digit is accepted.</param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        /// <param name="type">The digit type, which determines the next state and the field into which to insert
        /// the digit.</param>
        private static void AcceptDigitHelper(
            int cp, StateName nextName, ParserState state, StateItem item, DigitType type)
        {
            // Check the Unicode digit character property
            sbyte digit = (sbyte)UChar.Digit(cp, 10);
            StateItem next = null;

            // Look for the digit:
            if (digit >= 0)
            {
                // Code point is a number
                next = state.GetNext().CopyFrom(item, nextName, -1);
            }

            // Do not perform the expensive string manipulations in fast mode.
            if (digit < 0 && (state.mode == ParseMode.Lenient || state.mode == ParseMode.Strict))
            {
                if (state.digitTrie == null)
                {
                    // Check custom digits, all of which are at most one code point
                    for (sbyte d = 0; d < 10; d++)
                    {
                        int referenceCp = Character.CodePointAt(state.symbols.DigitStringsLocal[d], 0);
                        if (cp == referenceCp)
                        {
                            digit = d;
                            next = state.GetNext().CopyFrom(item, nextName, -1);
                        }
                    }
                }
                else
                {
                    // Custom digits have more than one code point
                    AcceptDigitTrie(cp, nextName, state, item, type);
                }
            }

            // Save state
            RecordDigit(next, (byte)digit, type);
        }

        /// <summary>
        /// Helper function for <see cref="AcceptDigitHelper(int, StateName, ParserState, StateItem, DigitType)"/>
        /// and <see cref="AcceptDigitTrie(int, StateName, ParserState, StateItem, DigitType)"/> to save a complete
        /// digit in a state item and update grouping widths.
        /// </summary>
        /// <param name="next">The new <see cref="StateItem"/>.</param>
        /// <param name="digit">The digit to record.</param>
        /// <param name="type">The type of the digit to record (<see cref="DigitType.Integer"/>,
        /// <see cref="DigitType.Fraction"/>, or <see cref="DigitType.Exponent"/>)</param>
        private static void RecordDigit(StateItem next, byte digit, DigitType? type)
        {
            if (next == null) return;
            next.AppendDigit(digit, type);
            if (type == DigitType.Integer && (next.groupingWidths & 0xf) < 15)
            {
                next.groupingWidths++;
            }
        }

        /// <summary>
        /// If <paramref name="cp"/> is a sign (as determined by the unicode sets <see cref="UNISET_PLUS"/>
        /// and <see cref="UNISET_MINUS"/>, copies <paramref name="item"/> to the new list in <paramref name="state"/>.
        /// Loops back to the same state name.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName"></param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        /// <param name="exponent"></param>
        private static void AcceptMinusOrPlusSign(
            int cp, StateName nextName, ParserState state, StateItem item, bool exponent)
        {
            AcceptMinusSign(cp, nextName, null, state, item, exponent);
            AcceptPlusSign(cp, nextName, null, state, item, exponent);
        }

        private static long AcceptMinusSign(
            int cp,
            StateName? returnTo1,
            StateName? returnTo2,
            ParserState state,
            StateItem item,
            bool exponent)
        {
            if (UNISET_MINUS.Contains(cp))
            {
                StateItem next = state.GetNext().CopyFrom(item, returnTo1, -1);
                next.returnTo1 = returnTo2;
                if (exponent)
                {
                    next.sawNegativeExponent = true;
                }
                else
                {
                    next.sawNegative = true;
                }
                return 1L << state.LastInsertedIndex;
            }
            else
            {
                return 0L;
            }
        }

        private static long AcceptPlusSign(
            int cp,
            StateName? returnTo1,
            StateName? returnTo2,
            ParserState state,
            StateItem item,
            bool exponent)
        {
            if (UNISET_PLUS.Contains(cp))
            {
                StateItem next = state.GetNext().CopyFrom(item, returnTo1, -1);
                next.returnTo1 = returnTo2;
                return 1L << state.LastInsertedIndex;
            }
            else
            {
                return 0L;
            }
        }

        // ICU4N NOTE: The docs here are a best guess because the references in the original doc were broken and this class
        // was removed in ICU4J 61.
        /// <summary>
        /// If <paramref name="cp"/> is a grouping separator, copies <paramref name="item"/> to the new list in <paramref name="state"/>
        /// and loops back to the same state. Also accepts if <paramref name="cp"/> is the locale-specific grouping
        /// separator in <see cref="ParserState.groupingCp1"/> and <see cref="ParserState.groupingCp2"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName"></param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptGrouping(
            int cp, StateName? nextName, ParserState state, StateItem item)
        {
            // Do not accept mixed grouping separators in the same string.
            if (item.groupingCp == -1)
            {
                // First time seeing a grouping separator.
                SeparatorType cpType = SeparatorTypeUtil.FromCp(cp, state.mode);

                // Always accept if exactly the same as the locale grouping separator.
                if (cp != state.groupingCp1 && cp != state.groupingCp2)
                {
                    // Reject if not in one of the three primary equivalence classes.
                    if (cpType == SeparatorType.Unknown)
                    {
                        return;
                    }
                    if (state.groupingMode == GroupingMode.Restricted)
                    {
                        // Reject if not in the same class as the locale grouping separator.
                        if (cpType != state.groupingType1 || cpType != state.groupingType2)
                        {
                            return;
                        }
                    }
                    else
                    {
                        // Reject if in the same class as the decimal separator.
                        if (cpType == SeparatorType.CommaLike
                            && (state.decimalType1 == SeparatorType.CommaLike
                                || state.decimalType2 == SeparatorType.CommaLike))
                        {
                            return;
                        }
                        if (cpType == SeparatorType.PeriodLike
                            && (state.decimalType1 == SeparatorType.PeriodLike
                                || state.decimalType2 == SeparatorType.PeriodLike))
                        {
                            return;
                        }
                    }
                }

                // A match was found.
                StateItem next = state.GetNext().CopyFrom(item, nextName, cp);
                next.groupingCp = cp;
                next.groupingWidths <<= 4;
            }
            else
            {
                // Have already seen a grouping separator.
                if (cp == item.groupingCp)
                {
                    StateItem next = state.GetNext().CopyFrom(item, nextName, cp);
                    next.groupingWidths <<= 4;
                }
            }
        }

        // ICU4N NOTE: The docs here are a best guess because the references in the original doc were broken and this class
        // was removed in ICU4J 61.
        /// <summary>
        /// If <paramref name="cp"/> is a decimal, copies <paramref name="item"/> to the new list in <paramref name="state"/> and goes to
        /// <see cref="StateName.AfterFractionDigit"/>. Also accepts if <paramref name="cp"/> is the locale-specific decimal
        /// point in <see cref="ParserState.decimalCp1"/> and <see cref="ParserState.decimalCp2"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName"></param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptDecimalPoint(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            if (cp == item.groupingCp)
            {
                // Don't accept a decimal point that is the same as the grouping separator
                return;
            }

            SeparatorType cpType = SeparatorTypeUtil.FromCp(cp, state.mode);

            // We require that the decimal separator be in the same class as the locale.
            if (cpType != state.decimalType1 && cpType != state.decimalType2)
            {
                return;
            }

            // If in UNKNOWN or OTHER, require an exact match.
            if (cpType == SeparatorType.OtherGrouping || cpType == SeparatorType.Unknown)
            {
                if (cp != state.decimalCp1 && cp != state.decimalCp2)
                {
                    return;
                }
            }

            // A match was found.
            StateItem next = state.GetNext().CopyFrom(item, nextName, -1);
            next.sawDecimalPoint = true;
        }

        private static void AcceptNan(int cp, StateName nextName, ParserState state, StateItem item)
        {
            string nan = state.symbols.NaN;
            long added = AcceptString(cp, nextName, null, state, item, nan, 0, false);

            // Set state in the items that were added by the function call
            for (int i = added.TrailingZeroCount(); (1L << i) <= added; i++)
            {
                if (((1L << i) & added) != 0)
                {
                    state.GetItem(i).sawNaN = true;
                }
            }
        }

        private static void AcceptInfinity(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            string inf = state.symbols.Infinity;
            long added = AcceptString(cp, nextName, null, state, item, inf, 0, false);

            // Set state in the items that were added by the function call
            for (int i = added.TrailingZeroCount(); (1L << i) <= added; i++)
            {
                if (((1L << i) & added) != 0)
                {
                    state.GetItem(i).sawInfinity = true;
                }
            }
        }

        private static void AcceptExponentSeparator(
            int cp, StateName nextName, ParserState state, StateItem item)
        {
            string exp = state.symbols.ExponentSeparator;
            AcceptString(cp, nextName, null, state, item, exp, 0, true);
        }

        private static void AcceptPrefix(int cp, StateName nextName, ParserState state, StateItem item)
        {
            foreach (AffixHolder holder in state.affixHolders)
            {
                AcceptAffixHolder(cp, nextName, state, item, holder, true);
            }
        }

        private static void AcceptSuffix(int cp, StateName nextName, ParserState state, StateItem item)
        {
            if (item.affix != null)
            {
                AcceptAffixHolder(cp, nextName, state, item, item.affix, false);
            }
            else
            {
                foreach (AffixHolder holder in state.affixHolders)
                {
                    AcceptAffixHolder(cp, nextName, state, item, holder, false);
                }
            }
        }

        private static void AcceptAffixHolder(
            int cp,
            StateName nextName,
            ParserState state,
            StateItem item,
            AffixHolder holder,
            bool prefix)
        {
            if (holder == null) return;
            string str = prefix ? holder.p : holder.s;
            long added;
            if (holder.strings)
            {
                added = AcceptString(cp, nextName, null, state, item, str, 0, false);
            }
            else
            {
                added =
                    AcceptAffixPattern(cp, nextName, state, item, str, AffixUtils.NextToken(0, str));
            }
            // Record state in the added entries
            for (int i = added.TrailingZeroCount(); (1L << i) <= added; i++)
            {
                if (((1L << i) & added) != 0)
                {
                    StateItem next = state.GetItem(i);
                    next.affix = holder;
                    if (prefix) next.sawPrefix = true;
                    if (!prefix) next.sawSuffix = true;
                    if (holder.negative) next.sawNegative = true;
                    // 10 point reward for consuming a prefix/suffix:
                    next.score += 10;
                    // 1 point reward for positive holders (if there is ambiguity, we want to favor positive):
                    if (!holder.negative) next.score += 1;
                    // 5 point reward for affix holders that have an empty prefix or suffix (we won't see them again):
                    if (!next.sawPrefix && holder.p == string.Empty) next.score += 5;
                    if (!next.sawSuffix && holder.s == string.Empty) next.score += 5;
                }
            }
        }

        private static long AcceptStringOffset(int cp, ParserState state, StateItem item)
        {
            return AcceptString(
                cp,
                item.returnTo1,
                item.returnTo2,
                state,
                item,
                item.currentString,
                item.currentOffset,
                item.currentTrailing);
        }

        /// <summary>
        /// Accepts a code point if the code point is compatible with the string at the given offset.
        /// Handles runs of ignorable characters.
        /// <para/>
        /// This method will add either one or two <see cref="StateItem"/> to the <see cref="ParserState"/>.
        /// </summary>
        /// <param name="cp">The current code point, which will be checked for a match to the string.</param>
        /// <param name="ret1">The state to return to after reaching the end of the string.</param>
        /// <param name="ret2">The state to save in <c>returnTo1</c> after reaching the end of the string.</param>
        /// <param name="state">The current <see cref="ParserState"/>.</param>
        /// <param name="item">The current <see cref="StateItem"/>.</param>
        /// <param name="str">The string against which to check for a match.</param>
        /// <param name="offset">The number of chars into the string. Initial value should be 0.</param>
        /// <param name="trailing"><c>false</c> if this string is strong and should reset trailing count to zero when it
        /// is fully consumed.</param>
        /// <returns>A bitmask where the bits correspond to the items that were added. Set to 0L if no items
        /// were added.</returns>
        private static long AcceptString(
            int cp,
            StateName? ret1,
            StateName? ret2,
            ParserState state,
            StateItem item,
            string str,
            int offset,
            bool trailing)
        {
            if (str == null || str.Length == 0) return 0L;
            return AcceptStringOrAffixPatternWithIgnorables(
                cp, ret1, ret2, state, item, str, offset, trailing, true);
        }

        private static long AcceptStringNonIgnorable(
            int cp,
            StateName? ret1,
            StateName? ret2,
            ParserState state,
            StateItem item,
            string str,
            bool trailing,
            int referenceCp,
            long firstOffsetOrTag,
            long nextOffsetOrTag)
        {
            long added = 0L;
            int firstOffset = (int)firstOffsetOrTag;
            int nextOffset = (int)nextOffsetOrTag;
            if (CodePointEquals(referenceCp, cp, state))
            {
                if (firstOffset < str.Length)
                {
                    added |= AcceptStringHelper(cp, ret1, ret2, state, item, str, firstOffset, trailing);
                }
                if (nextOffset >= str.Length)
                {
                    added |= AcceptStringHelper(cp, ret1, ret2, state, item, str, nextOffset, trailing);
                }
                return added;
            }
            else
            {
                return 0L;
            }
        }

        /// <summary>
        /// Internal method that is used to step to the next code point of a string or exit the string if
        /// at the end.
        /// </summary>
        /// <param name="cp">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="returnTo1">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="returnTo2">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="state">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="item">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="str">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <param name="newOffset">The offset at which the next step should start. If past the end of the string,
        /// exit the string and return to the outer loop.</param>
        /// <param name="trailing">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>.</param>
        /// <returns>Bitmask containing one entry, the one that was added.</returns>
        private static long AcceptStringHelper(
            int cp,
            StateName? returnTo1,
            StateName? returnTo2,
            ParserState state,
            StateItem item,
            string str,
            int newOffset,
            bool trailing)
        {
            StateItem next = state.GetNext().CopyFrom(item, null, cp);
            next.score += 1; // reward for consuming a cp from string
            if (newOffset < str.Length)
            {
                // String has more code points.
                next.name = StateName.InsideString;
                next.returnTo1 = returnTo1;
                next.returnTo2 = returnTo2;
                next.currentString = str;
                next.currentOffset = newOffset;
                next.currentTrailing = trailing;
            }
            else
            {
                // We've reached the end of the string.
                next.name = returnTo1;
                if (!trailing) next.trailingCount = 0;
                next.returnTo1 = returnTo2;
                next.returnTo2 = null;
            }
            return 1L << state.LastInsertedIndex;
        }

        private static long AcceptAffixPatternOffset(int cp, ParserState state, StateItem item)
        {
            return AcceptAffixPattern(
                cp, item.returnTo1, state, item, item.currentAffixPattern, item.currentStepwiseParserTag);
        }

        /// <summary>
        /// Accepts a code point if the code point is compatible with the affix pattern at the offset
        /// encoded in the tag argument.
        /// </summary>
        /// <param name="cp">The current code point, which will be checked for a match to the string.</param>
        /// <param name="ret1">The state to return to after reaching the end of the string.</param>
        /// <param name="state">The current <see cref="ParserState"/>.</param>
        /// <param name="item">The current <see cref="StateItem"/>.</param>
        /// <param name="str">The string containing the affix pattern.</param>
        /// <param name="tag">The current state of the stepwise parser. Initial value should be 0L.</param>
        /// <returns>A bitmask where the bits correspond to the items that were added. Set to 0L if no items
        /// were added.</returns>
        private static long AcceptAffixPattern(
            int cp, StateName? ret1, ParserState state, StateItem item, string str, long tag)
        {
            if (str == null || str.Length == 0) return 0L;
            return AcceptStringOrAffixPatternWithIgnorables(
                cp, ret1, null, state, item, str, tag, false, false);
        }

        private static long AcceptAffixPatternNonIgnorable(
            int cp,
            StateName? returnTo,
            ParserState state,
            StateItem item,
            string str,
            int typeOrCp,
            long firstTag,
            long nextTag)
        {

            // Convert from the returned tag to a code point, string, or currency to check
            int resolvedCp = -1;
            string resolvedStr = null;
            bool resolvedMinusSign = false;
            bool resolvedPlusSign = false;
            bool resolvedCurrency = false;
            if (typeOrCp < 0)
            {
                // Symbol
                switch ((AffixUtils.Type)typeOrCp)
                {
                    case AffixUtils.Type.MinusSign:
                        resolvedMinusSign = true;
                        break;
                    case AffixUtils.Type.PlusSign:
                        resolvedPlusSign = true;
                        break;
                    case AffixUtils.Type.Percent:
                        resolvedStr = state.symbols.PercentString;
                        if (resolvedStr.Length != 1 || resolvedStr[0] != '%')
                        {
                            resolvedCp = '%'; // accept ASCII percent as well as locale percent
                        }
                        break;
                    case AffixUtils.Type.PerMille:
                        resolvedStr = state.symbols.PerMillString;
                        if (resolvedStr.Length != 1 || resolvedStr[0] != '‰')
                        {
                            resolvedCp = '‰'; // accept ASCII permille as well as locale permille
                        }
                        break;
                    case AffixUtils.Type.CurrencySymbol:
                    case AffixUtils.Type.CurrencyDouble:
                    case AffixUtils.Type.CurrencyTriple:
                    case AffixUtils.Type.CurrencyQuad:
                    case AffixUtils.Type.CurrencyQuint:
                    case AffixUtils.Type.CurrencyOverflow:
                        resolvedCurrency = true;
                        break;
                    default:
                        throw new InvalidOperationException();  // throw new AssertionError();
                }
            }
            else
            {
                resolvedCp = typeOrCp;
            }

            long added = 0L;
            if (resolvedCp >= 0 && CodePointEquals(cp, resolvedCp, state))
            {
                if (firstTag >= 0)
                {
                    added |= AcceptAffixPatternHelper(cp, returnTo, state, item, str, firstTag);
                }
                if (nextTag < 0)
                {
                    added |= AcceptAffixPatternHelper(cp, returnTo, state, item, str, nextTag);
                }
            }
            if (resolvedMinusSign)
            {
                if (firstTag >= 0)
                {
                    added |= AcceptMinusSign(cp, StateName.InsideAffixPattern, returnTo, state, item, false);
                }
                if (nextTag < 0)
                {
                    added |= AcceptMinusSign(cp, returnTo, null, state, item, false);
                }
                if (added == 0L)
                {
                    // Also attempt to accept custom minus sign string
                    string mss = state.symbols.MinusSignString;
                    int mssCp = Character.CodePointAt(mss, 0);
                    if (mss.Length != Character.CharCount(mssCp) || !UNISET_MINUS.Contains(mssCp))
                    {
                        resolvedStr = mss;
                    }
                }
            }
            if (resolvedPlusSign)
            {
                if (firstTag >= 0)
                {
                    added |= AcceptPlusSign(cp, StateName.InsideAffixPattern, returnTo, state, item, false);
                }
                if (nextTag < 0)
                {
                    added |= AcceptPlusSign(cp, returnTo, null, state, item, false);
                }
                if (added == 0L)
                {
                    // Also attempt to accept custom plus sign string
                    string pss = state.symbols.PlusSignString;
                    int pssCp = Character.CodePointAt(pss, 0);
                    if (pss.Length != Character.CharCount(pssCp) || !UNISET_MINUS.Contains(pssCp))
                    {
                        resolvedStr = pss;
                    }
                }
            }
            if (resolvedStr != null)
            {
                if (firstTag >= 0)
                {
                    added |=
                        AcceptString(
                            cp, StateName.InsideAffixPattern, returnTo, state, item, resolvedStr, 0, false);
                }
                if (nextTag < 0)
                {
                    added |= AcceptString(cp, returnTo, null, state, item, resolvedStr, 0, false);
                }
            }
            if (resolvedCurrency)
            {
                if (firstTag >= 0)
                {
                    added |= AcceptCurrency(cp, StateName.InsideAffixPattern, returnTo, state, item);
                }
                if (nextTag < 0)
                {
                    added |= AcceptCurrency(cp, returnTo, null, state, item);
                }
            }

            // Set state in the items that were added by the function calls
            for (int i = added.TrailingZeroCount(); (1L << i) <= added; i++)
            {
                if (((1L << i) & added) != 0)
                {
                    state.GetItem(i).currentAffixPattern = str;
                    state.GetItem(i).currentStepwiseParserTag = firstTag;
                }
            }
            return added;
        }

        /// <summary>
        /// Internal method that is used to step to the next token of a affix pattern or exit the affix
        /// pattern if at the end.
        /// </summary>
        /// <param name="cp">See <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, string, long)"/>.</param>
        /// <param name="returnTo">See <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, string, long)"/>.</param>
        /// <param name="state">See <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, string, long)"/>.</param>
        /// <param name="item">See <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, string, long)"/>.</param>
        /// <param name="str">See <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, string, long)"/>.</param>
        /// <param name="newTag">The tag corresponding to the next token in the affix pattern that should be
        /// recorded and consumed in a future call to <see cref="AcceptAffixPatternOffset(int, ParserState, StateItem)"/>.</param>
        /// <returns>Bitmask containing one entry, the one that was added.</returns>
        private static long AcceptAffixPatternHelper(
            int cp,
            StateName? returnTo,
            ParserState state,
            StateItem item,
            string str,
            long newTag)
        {
            StateItem next = state.GetNext().CopyFrom(item, null, cp);
            next.score += 1; // reward for consuming a cp from pattern
            if (newTag >= 0)
            {
                // Additional tokens in affix string.
                next.name = StateName.InsideAffixPattern;
                next.returnTo1 = returnTo;
                next.currentAffixPattern = str;
                next.currentStepwiseParserTag = newTag;
            }
            else
            {
                // Reached last token in affix string.
                next.name = returnTo;
                next.trailingCount = 0;
                next.returnTo1 = null;
            }
            return 1L << state.LastInsertedIndex;
        }

        /// <summary>
        /// Consumes tokens from a string or affix pattern following ICU's rules for handling of whitespace
        /// and bidi control characters (collectively called "ignorables"). The methods
        /// <see cref="AcceptStringHelper(int, StateName?, StateName?, ParserState, StateItem, string, int, bool)"/>,
        /// <see cref="AcceptAffixPatternHelper(int, StateName?, ParserState, StateItem, string, long)"/>,
        /// <see cref="AcceptStringNonIgnorable(int, StateName?, StateName?, ParserState, StateItem, string, bool, int, long, long)"/>
        /// and <see cref="AcceptAffixPatternNonIgnorable(int, StateName?, ParserState, StateItem, string, int, long, long)"/>
        /// will be called by this method to actually add parse paths.
        /// <para/>
        /// In the "NonIgnorable" functions, two arguments are passed: firstOffsetOrTag and
        /// nextOffsetOrTag. These two arguments should add parse paths according to the following rules:
        /// 
        /// <code>
        /// if (firstOffsetOrTag is valid or inside string boundary) {
        ///   // Add parse path going to firstOffsetOrTag
        /// }
        /// if (nextOffsetOrTag is invalid or beyond string boundary) {
        ///   // Add parse path leaving the string
        /// }
        /// </code>
        /// <para/>
        /// Note that there may be multiple parse paths added by these lines. This is important in order
        /// to properly handle runs of ignorables.
        /// </summary>
        /// <param name="cp">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="ret1">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="ret2">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// (affix pattern can pass null).</param>
        /// <param name="state">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="item">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="str">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="offsetOrTag">The current int offset for strings, or the current tag for affix patterns.</param>
        /// <param name="trailing">See <see cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// and <see cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>.</param>
        /// <param name="isString"><c>true</c> if the parameters correspond to a string; <c>false</c> if they correspond to an
        /// affix pattern.</param>
        /// <returns>A bitmask containing the entries that were added.</returns>
        private static long AcceptStringOrAffixPatternWithIgnorables(
            int cp,
            StateName? ret1,
            StateName? ret2 /* String only */,
            ParserState state,
            StateItem item,
            string str,
            long offsetOrTag /* offset for string; tag for affix pattern */,
            bool trailing /* String only */,
            bool isString)
        {

            // Runs of ignorables (whitespace and bidi control marks) can occur at the beginning, middle,
            // or end of the reference string, or a run across the entire string.
            //
            // - A run at the beginning or in the middle corresponds to a run of length *zero or more*
            //   in the input.
            // - A run at the end need to be matched exactly.
            // - A string that contains only ignorable characters also needs to be matched exactly.
            //
            // Because the behavior differs, we need logic here to determine which case we have.

            int typeOrCp =
                isString
                    ? Character.CodePointAt(str, (int)offsetOrTag)
                    : AffixUtils.GetTypeOrCp(offsetOrTag);

            if (IsIgnorable(typeOrCp, state))
            {
                // Look for the next nonignorable code point
                int nextTypeOrCp = typeOrCp;
                long prevOffsetOrTag;
                long nextOffsetOrTag = offsetOrTag;
                long firstOffsetOrTag = 0L;
                while (true)
                {
                    prevOffsetOrTag = nextOffsetOrTag;
                    nextOffsetOrTag =
                        isString
                            ? nextOffsetOrTag + Character.CharCount(nextTypeOrCp)
                            : AffixUtils.NextToken(nextOffsetOrTag, str);
                    if (firstOffsetOrTag == 0L) firstOffsetOrTag = nextOffsetOrTag;
                    if (isString ? nextOffsetOrTag >= str.Length : nextOffsetOrTag < 0)
                    {
                        // Integer.MIN_VALUE is an invalid value for either a type or a cp;
                        // use it to indicate the end of the string.
                        nextTypeOrCp = int.MinValue;
                        break;
                    }
                    nextTypeOrCp =
                        isString
                            ? Character.CodePointAt(str, (int)nextOffsetOrTag)
                            : AffixUtils.GetTypeOrCp(nextOffsetOrTag);
                    if (!IsIgnorable(nextTypeOrCp, state)) break;
                }

                if (nextTypeOrCp == int.MinValue)
                {
                    // Run at end or string that contains only ignorable characters.
                    if (CodePointEquals(cp, typeOrCp, state))
                    {
                        // Step forward and also exit the string if not at very end.
                        // RETURN
                        long added = 0L;
                        added |=
                            isString
                                ? AcceptStringHelper(
                                    cp, ret1, ret2, state, item, str, (int)firstOffsetOrTag, trailing)
                                : AcceptAffixPatternHelper(cp, ret1, state, item, str, firstOffsetOrTag);
                        if (firstOffsetOrTag != nextOffsetOrTag)
                        {
                            added |=
                                isString
                                    ? AcceptStringHelper(
                                        cp, ret1, ret2, state, item, str, (int)nextOffsetOrTag, trailing)
                                    : AcceptAffixPatternHelper(cp, ret1, state, item, str, nextOffsetOrTag);
                        }
                        return added;
                    }
                    else
                    {
                        // Code point does not exactly match the run at end.
                        // RETURN
                        return 0L;
                    }
                }
                else
                {
                    // Run at beginning or in middle.
                    if (IsIgnorable(cp, state))
                    {
                        // Consume the ignorable.
                        // RETURN
                        return isString
                            ? AcceptStringHelper(
                                cp, ret1, ret2, state, item, str, (int)prevOffsetOrTag, trailing)
                            : AcceptAffixPatternHelper(cp, ret1, state, item, str, prevOffsetOrTag);
                    }
                    else
                    {
                        // Go to nonignorable cp.
                        // FALL THROUGH
                    }
                }

                // Fall through to the nonignorable code point found above.
                Debug.Assert(nextTypeOrCp != int.MinValue);
                typeOrCp = nextTypeOrCp;
                offsetOrTag = nextOffsetOrTag;
            }
            Debug.Assert(!IsIgnorable(typeOrCp, state));

            {
                // Look for the next nonignorable code point after this nonignorable code point
                // to determine if we are at the end of the string.
                int nextTypeOrCp = typeOrCp;
                long nextOffsetOrTag = offsetOrTag;
                long firstOffsetOrTag = 0L;
                while (true)
                {
                    nextOffsetOrTag =
                        isString
                            ? nextOffsetOrTag + Character.CharCount(nextTypeOrCp)
                            : AffixUtils.NextToken(nextOffsetOrTag, str);
                    if (firstOffsetOrTag == 0L) firstOffsetOrTag = nextOffsetOrTag;
                    if (isString ? nextOffsetOrTag >= str.Length : nextOffsetOrTag < 0)
                    {
                        nextTypeOrCp = -1;
                        break;
                    }
                    nextTypeOrCp =
                        isString
                            ? Character.CodePointAt(str, (int)nextOffsetOrTag)
                            : AffixUtils.GetTypeOrCp(nextOffsetOrTag);
                    if (!IsIgnorable(nextTypeOrCp, state)) break;
                }

                // Nonignorable logic.
                return isString
                    ? AcceptStringNonIgnorable(
                        cp, ret1, ret2, state, item, str, trailing, typeOrCp, firstOffsetOrTag, nextOffsetOrTag)
                    : AcceptAffixPatternNonIgnorable(
                        cp, ret1, state, item, str, typeOrCp, firstOffsetOrTag, nextOffsetOrTag);
            }
        }

        /// <summary>
        /// This method can add up to four items to the new list in <paramref name="state"/>.
        /// <para/>
        /// If <paramref name="cp"/> is equal to any known ISO code or long name, copies <paramref name="item"/> to
        /// the new list in <paramref name="state"/> and sets its ISO code to the corresponding currency.
        /// <para/>
        /// If <paramref name="cp"/> is the first code point of any ISO code or long name having more them one
        /// code point in length, copies <paramref name="item"/> to the new list in <paramref name="state"/> along with
        /// an instance of <see cref="TextTrieMap{TValue}.ParseState"/> for tracking the following code points.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="nextName"></param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptCurrency(
            int cp, StateName? nextName, ParserState state, StateItem item)
        {
            AcceptCurrency(cp, nextName, null, state, item);
        }

        private static long AcceptCurrency(
            int cp, StateName? returnTo1, StateName? returnTo2, ParserState state, StateItem item)
        {
            if (item.sawCurrency) return 0L;
            long added = 0L;

            // Accept from local currency information
            String str1, str2;
            Currency currency = state.properties.Currency;
            if (currency != null)
            {
                str1 = currency.GetName(state.symbols.UCulture, CurrencyNameStyle.SymbolName, out bool _);
                str2 = currency.CurrencyCode;
                // TODO: Should we also accept long names? In currency mode, they are in the CLDR data.
            }
            else
            {
                currency = state.symbols.Currency;
                str1 = state.symbols.CurrencySymbol;
                str2 = state.symbols.InternationalCurrencySymbol;
            }
            added |= AcceptString(cp, returnTo1, returnTo2, state, item, str1, 0, false);
            added |= AcceptString(cp, returnTo1, returnTo2, state, item, str2, 0, false);
            for (int i = added.TrailingZeroCount(); (1L << i) <= added; i++)
            {
                if (((1L << i) & added) != 0)
                {
                    state.GetItem(i).sawCurrency = true;
                    state.GetItem(i).isoCode = str2;
                }
            }

            // Accept from CLDR data
            if (state.parseCurrency)
            {
                UCultureInfo uloc = state.symbols.UCulture;
                TextTrieMap<Currency.CurrencyStringInfo>.ParseState trie1 =
                    Currency.OpenParseState(uloc, cp, CurrencyNameStyle.LongName);
                TextTrieMap<Currency.CurrencyStringInfo>.ParseState trie2 =
                    Currency.OpenParseState(uloc, cp, CurrencyNameStyle.SymbolName);
                added |= AcceptCurrencyHelper(cp, returnTo1, returnTo2, state, item, trie1);
                added |= AcceptCurrencyHelper(cp, returnTo1, returnTo2, state, item, trie2);
            }

            return added;
        }

        /// <summary>
        /// If <paramref name="cp"/> is the next code point of any currency, copies <paramref name="item"/> to the new
        /// list in <paramref name="state"/> along with an instance of <see cref="TextTrieMap{TValue}.ParseState"/> for
        /// tracking the following code points.
        /// <para/>
        /// This method should only be called in a state following <see cref="AcceptCurrency(int, StateName?, ParserState, StateItem)"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <param name="state">The state object to update.</param>
        /// <param name="item">The old state leading into the code point.</param>
        private static void AcceptCurrencyOffset(int cp, ParserState state, StateItem item)
        {
            AcceptCurrencyHelper(
                cp, item.returnTo1, item.returnTo2, state, item, item.currentCurrencyTrieState);
        }

        private static long AcceptCurrencyHelper(
            int cp,
            StateName? returnTo1,
            StateName? returnTo2,
            ParserState state,
            StateItem item,
            TextTrieMap<Currency.CurrencyStringInfo>.ParseState trieState)
        {
            if (trieState == null) return 0L;
            trieState.Accept(cp);
            long added = 0L;
            IEnumerator<Currency.CurrencyStringInfo> currentMatches = trieState.GetCurrentMatches();
            if (currentMatches != null)
            {
                // Match on current code point
                // TODO: What should happen with multiple currency matches?
                StateItem next = state.GetNext().CopyFrom(item, returnTo1, -1);
                next.returnTo1 = returnTo2;
                next.returnTo2 = null;
                next.sawCurrency = true;
                currentMatches.MoveNext();
                next.isoCode = currentMatches.Current.ISOCode;
                added |= 1L << state.LastInsertedIndex;
            }
            if (!trieState.AtEnd)
            {
                // Prepare for matches on future code points
                StateItem next = state.GetNext().CopyFrom(item, StateName.InsideCurrency, -1);
                next.returnTo1 = returnTo1;
                next.returnTo2 = returnTo2;
                next.currentCurrencyTrieState = trieState;
                added |= 1L << state.LastInsertedIndex;
            }
            return added;
        }

        private static long AcceptDigitTrie(
            int cp, StateName nextName, ParserState state, StateItem item, DigitType type)
        {
            Debug.Assert(state.digitTrie != null);
            TextTrieMap<byte>.ParseState trieState = state.digitTrie.OpenParseState(cp);
            if (trieState == null) return 0L;
            return AcceptDigitTrieHelper(cp, nextName, state, item, type, trieState);
        }

        private static void AcceptDigitTrieOffset(int cp, ParserState state, StateItem item)
        {
            AcceptDigitTrieHelper(
                cp, item.returnTo1, state, item, item.currentDigitType, item.currentDigitTrieState);
        }

        private static long AcceptDigitTrieHelper(
            int cp,
            StateName? returnTo1,
            ParserState state,
            StateItem item,
            DigitType? type,
            TextTrieMap<byte>.ParseState trieState)
        {
            if (trieState == null) return 0L;
            trieState.Accept(cp);
            long added = 0L;
            IEnumerator<byte> currentMatches = trieState.GetCurrentMatches();
            if (currentMatches != null)
            {
                // Match on current code point
                currentMatches.MoveNext();
                byte digit = currentMatches.Current;
                StateItem next = state.GetNext().CopyFrom(item, returnTo1, -1);
                next.returnTo1 = null;
                RecordDigit(next, digit, type);
                added |= 1L << state.LastInsertedIndex;
            }
            if (!trieState.AtEnd)
            {
                // Prepare for matches on future code points
                StateItem next = state.GetNext().CopyFrom(item, StateName.InsideDigit, -1);
                next.returnTo1 = returnTo1;
                next.currentDigitTrieState = trieState;
                next.currentDigitType = type;
                added |= 1L << state.LastInsertedIndex;
            }
            return added;
        }

        /// <summary>
        /// Checks whether the two given code points are equal after applying case mapping as requested in
        /// the <see cref="ParserState"/>.
        /// </summary>
        /// <seealso cref="AcceptString(int, StateName?, StateName?, ParserState, StateItem, ICharSequence, int, bool)"/>
        /// <seealso cref="AcceptAffixPattern(int, StateName?, ParserState, StateItem, ICharSequence, long)"/>
        private static bool CodePointEquals(int cp1, int cp2, ParserState state)
        {
            if (!state.caseSensitive)
            {
                cp1 = UChar.FoldCase(cp1, true);
                cp2 = UChar.FoldCase(cp2, true);
            }
            return cp1 == cp2;
        }

        /// <summary>
        /// Checks whether the given code point is "ignorable" and should be skipped. BiDi control marks
        /// are always ignorable, and whitespace is ignorable in lenient mode.
        /// <para/>
        /// Returns <c>false</c> if <paramref name="cp"/> is negative.
        /// </summary>
        /// <param name="cp">The code point to test.</param>
        /// <param name="state">The current <see cref="ParserState"/>, used for determining strict mode.</param>
        /// <returns><c>true</c> if <paramref name="cp"/> is ignorable; <c>false</c> otherwise.</returns>
        private static bool IsIgnorable(int cp, ParserState state)
        {
            if (cp < 0) return false;
            if (UNISET_BIDI.Contains(cp)) return true;
            return state.mode == ParseMode.Lenient && UNISET_WHITESPACE.Contains(cp);
        }
    }
}
