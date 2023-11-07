using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Numerics.BigMath;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Numerics;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Double = J2N.Numerics.Double;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Dev.Test.Format
{
    public class NumberFormatDataDrivenTest : TestFmwk
    {
        private static UCultureInfo EN = new UCultureInfo("en");

        private static Number toNumber(String s)
        {
            if (s.Equals("NaN", StringComparison.Ordinal))
            {
                return Double.GetInstance(double.NaN);
            }
            else if (s.Equals("-Inf", StringComparison.Ordinal))
            {
                return Double.GetInstance(double.NegativeInfinity);
            }
            else if (s.Equals("Inf", StringComparison.Ordinal))
            {
                return Double.GetInstance(double.PositiveInfinity);
            }
            return ICU4N.Numerics.BigMath.BigDecimal.Parse(s, CultureInfo.InvariantCulture);
        }

#if FEATURE_IKVM

        private static java.lang.Number toJdkNumber(String s)
        {
            if (s.Equals("NaN", StringComparison.Ordinal))
            {
                return java.lang.Double.valueOf(double.NaN);
            }
            else if (s.Equals("-Inf", StringComparison.Ordinal))
            {
                return java.lang.Double.valueOf(double.NegativeInfinity);
            }
            else if (s.Equals("Inf", StringComparison.Ordinal))
            {
                return java.lang.Double.valueOf(double.PositiveInfinity);
            }
            return new java.math.BigDecimal(s);
        }

#endif

        //// ICU4N TODO: Missing dependency DecimalFormat_ICU58
        //private class ICU58CodeUnderTestAnonymousClass : DataDrivenNumberFormatTestUtility.CodeUnderTest
        //{
        //    public override char? Id => 'J';

        //    public override string Format(DataDrivenNumberFormatTestData tuple)
        //    {
        //        DecimalFormat_ICU58 fmt = createDecimalFormat(tuple);
        //        String actual = fmt.format(toNumber(tuple.format));
        //        String expected = tuple.output;
        //        if (!expected.Equals(actual, StringComparison.Ordinal))
        //        {
        //            return "Expected " + expected + ", got " + actual;
        //        }
        //        return null;
        //    }

        //    public override string ToPattern(DataDrivenNumberFormatTestData tuple)
        //    {
        //        DecimalFormat_ICU58 fmt = createDecimalFormat(tuple);
        //        StringBuilder result = new StringBuilder();
        //        if (tuple.toPattern != null)
        //        {
        //            String expected = tuple.toPattern;
        //            String actual = fmt.toPattern();
        //            if (!expected.Equals(actual, StringComparison.Ordinal))
        //            {
        //                result.Append("Expected toPattern=" + expected + ", got " + actual);
        //            }
        //        }
        //        if (tuple.toLocalizedPattern != null)
        //        {
        //            String expected = tuple.toLocalizedPattern;
        //            String actual = fmt.toLocalizedPattern();
        //            if (!expected.Equals(actual, StringComparison.Ordinal))
        //            {
        //                result.Append("Expected toLocalizedPattern=" + expected + ", got " + actual);
        //            }
        //        }
        //        return result.Length == 0 ? null : result.ToString();
        //    }

        //    public override string Parse(DataDrivenNumberFormatTestData tuple)
        //    {
        //        DecimalFormat_ICU58 fmt = createDecimalFormat(tuple);
        //        ParsePosition ppos = new ParsePosition(0);
        //        Number actual = fmt.parse(tuple.parse, ppos);
        //        if (ppos.Index == 0)
        //        {
        //            return "Parse failed; got " + actual + ", but expected " + tuple.output;
        //        }
        //        if (tuple.output.Equals("fail", StringComparison.Ordinal))
        //        {
        //            return null;
        //        }
        //        Number expected = toNumber(tuple.output);
        //        // number types cannot be compared, this is the best we can do.
        //        if (expected.ToDouble() != actual.ToDouble()
        //            && !double.IsNaN(expected.ToDouble())
        //            && !double.IsNaN(expected.ToDouble()))
        //        {
        //            return "Expected: " + expected + ", got: " + actual;
        //        }
        //        return null;
        //    }

        //    public override string ParseCurrency(DataDrivenNumberFormatTestData tuple)
        //    {
        //        DecimalFormat_ICU58 fmt = createDecimalFormat(tuple);
        //        ParsePosition ppos = new ParsePosition(0);
        //        CurrencyAmount currAmt = fmt.parseCurrency(tuple.parse, ppos);
        //        if (ppos.Index == 0)
        //        {
        //            return "Parse failed; got " + currAmt + ", but expected " + tuple.output;
        //        }
        //        if (tuple.output.Equals("fail", StringComparison.Ordinal))
        //        {
        //            return null;
        //        }
        //        Number expected = toNumber(tuple.output);
        //        Number actual = currAmt.Number;
        //        // number types cannot be compared, this is the best we can do.
        //        if (expected.ToDouble() != actual.ToDouble()
        //            && !double.IsNaN(expected.ToDouble())
        //            && !double.IsNaN(expected.ToDouble()))
        //        {
        //            return "Expected: " + expected + ", got: " + actual;
        //        }

        //        if (!tuple.outputCurrency.Equals(currAmt.Currency.ToString()))
        //        {
        //            return "Expected currency: " + tuple.outputCurrency + ", got: " + currAmt.Currency;
        //        }
        //        return null;
        //    }

        //    /**
        //     * @param tuple
        //     * @return
        //     */
        //    private DecimalFormat_ICU58 createDecimalFormat(DataDrivenNumberFormatTestData tuple)
        //    {

        //        DecimalFormat_ICU58 fmt =
        //            new DecimalFormat_ICU58(
        //                tuple.pattern == null ? "0" : tuple.pattern,
        //                new DecimalFormatSymbols(tuple.locale == null ? EN : tuple.locale));
        //        adjustDecimalFormat(tuple, fmt);
        //        return fmt;
        //    }
        //    /**
        //     * @param tuple
        //     * @param fmt
        //     */
        //    private void adjustDecimalFormat(
        //        DataDrivenNumberFormatTestData tuple, DecimalFormat_ICU58 fmt)
        //    {
        //        if (tuple.minIntegerDigits != null)
        //        {
        //            fmt.setMinimumIntegerDigits(tuple.minIntegerDigits);
        //        }
        //        if (tuple.maxIntegerDigits != null)
        //        {
        //            fmt.setMaximumIntegerDigits(tuple.maxIntegerDigits);
        //        }
        //        if (tuple.minFractionDigits != null)
        //        {
        //            fmt.setMinimumFractionDigits(tuple.minFractionDigits);
        //        }
        //        if (tuple.maxFractionDigits != null)
        //        {
        //            fmt.setMaximumFractionDigits(tuple.maxFractionDigits);
        //        }
        //        if (tuple.currency != null)
        //        {
        //            fmt.setCurrency(tuple.currency);
        //        }
        //        if (tuple.minGroupingDigits != null)
        //        {
        //            // Oops we don't support this.
        //        }
        //        if (tuple.useSigDigits != null)
        //        {
        //            fmt.setSignificantDigitsUsed(tuple.useSigDigits != 0);
        //        }
        //        if (tuple.minSigDigits != null)
        //        {
        //            fmt.setMinimumSignificantDigits(tuple.minSigDigits);
        //        }
        //        if (tuple.maxSigDigits != null)
        //        {
        //            fmt.setMaximumSignificantDigits(tuple.maxSigDigits);
        //        }
        //        if (tuple.useGrouping != null)
        //        {
        //            fmt.setGroupingUsed(tuple.useGrouping != 0);
        //        }
        //        if (tuple.multiplier != null)
        //        {
        //            fmt.setMultiplier(tuple.multiplier);
        //        }
        //        if (tuple.roundingIncrement != null)
        //        {
        //            fmt.setRoundingIncrement(tuple.roundingIncrement.ToDouble());
        //        }
        //        if (tuple.formatWidth != null)
        //        {
        //            fmt.setFormatWidth(tuple.formatWidth);
        //        }
        //        if (tuple.padCharacter != null && tuple.padCharacter.Length > 0)
        //        {
        //            fmt.setPadCharacter(tuple.padCharacter[0]);
        //        }
        //        if (tuple.useScientific != null)
        //        {
        //            fmt.setScientificNotation(tuple.useScientific != 0);
        //        }
        //        if (tuple.grouping != null)
        //        {
        //            fmt.setGroupingSize(tuple.grouping);
        //        }
        //        if (tuple.grouping2 != null)
        //        {
        //            fmt.setSecondaryGroupingSize(tuple.grouping2);
        //        }
        //        if (tuple.roundingMode != null)
        //        {
        //            fmt.setRoundingMode(tuple.roundingMode);
        //        }
        //        if (tuple.currencyUsage != null)
        //        {
        //            fmt.setCurrencyUsage(tuple.currencyUsage);
        //        }
        //        if (tuple.minimumExponentDigits != null)
        //        {
        //            fmt.setMinimumExponentDigits(tuple.minimumExponentDigits.ToByte());
        //        }
        //        if (tuple.exponentSignAlwaysShown != null)
        //        {
        //            fmt.setExponentSignAlwaysShown(tuple.exponentSignAlwaysShown != 0);
        //        }
        //        if (tuple.decimalSeparatorAlwaysShown != null)
        //        {
        //            fmt.setDecimalSeparatorAlwaysShown(tuple.decimalSeparatorAlwaysShown != 0);
        //        }
        //        if (tuple.padPosition != null)
        //        {
        //            fmt.setPadPosition(tuple.padPosition);
        //        }
        //        if (tuple.positivePrefix != null)
        //        {
        //            fmt.setPositivePrefix(tuple.positivePrefix);
        //        }
        //        if (tuple.positiveSuffix != null)
        //        {
        //            fmt.setPositiveSuffix(tuple.positiveSuffix);
        //        }
        //        if (tuple.negativePrefix != null)
        //        {
        //            fmt.setNegativePrefix(tuple.negativePrefix);
        //        }
        //        if (tuple.negativeSuffix != null)
        //        {
        //            fmt.setNegativeSuffix(tuple.negativeSuffix);
        //        }
        //        if (tuple.localizedPattern != null)
        //        {
        //            fmt.applyLocalizedPattern(tuple.localizedPattern);
        //        }
        //        int lenient = tuple.lenient == null ? 1 : tuple.lenient.ToInt32();
        //        fmt.setParseStrict(lenient == 0);
        //        if (tuple.parseIntegerOnly != null)
        //        {
        //            fmt.setParseIntegerOnly(tuple.parseIntegerOnly != 0);
        //        }
        //        if (tuple.parseCaseSensitive != null)
        //        {
        //            // Not supported.
        //        }
        //        if (tuple.decimalPatternMatchRequired != null)
        //        {
        //            fmt.setDecimalPatternMatchRequired(tuple.decimalPatternMatchRequired != 0);
        //        }
        //        if (tuple.parseNoExponent != null)
        //        {
        //            // Oops, not supported for now
        //        }
        //    }
        //}

        //private DataDrivenNumberFormatTestUtility.CodeUnderTest ICU58 = new ICU58CodeUnderTestAnonymousClass();


#if FEATURE_IKVM
        private class JDKCodeUnderTestAnonymousClass : DataDrivenNumberFormatTestUtility.CodeUnderTest
        {
            public override char? Id => 'K';

            public override string Format(DataDrivenNumberFormatTestData tuple)
            {
                java.text.DecimalFormat fmt = createDecimalFormat(tuple);
                String actual = fmt.format(toJdkNumber(tuple.format));
                String expected = tuple.output;
                if (!expected.Equals(actual, StringComparison.Ordinal))
                {
                    return "Expected " + expected + ", got " + actual;
                }
                return null;
            }

            public override string ToPattern(DataDrivenNumberFormatTestData tuple)
            {
                java.text.DecimalFormat fmt = createDecimalFormat(tuple);
                StringBuilder result = new StringBuilder();
                if (tuple.toPattern != null)
                {
                    string expected = tuple.toPattern;
                    string actual = fmt.toPattern();
                    if (!expected.Equals(actual, StringComparison.Ordinal))
                    {
                        result.Append("Expected toPattern=" + expected + ", got " + actual);
                    }
                }
                if (tuple.toLocalizedPattern != null)
                {
                    String expected = tuple.toLocalizedPattern;
                    String actual = fmt.toLocalizedPattern();
                    if (!expected.Equals(actual, StringComparison.Ordinal))
                    {
                        result.Append("Expected toLocalizedPattern=" + expected + ", got " + actual);
                    }
                }
                return result.Length == 0 ? null : result.ToString();
            }

            public override string Parse(DataDrivenNumberFormatTestData tuple)
            {
                java.text.DecimalFormat fmt = createDecimalFormat(tuple);
                java.text.ParsePosition ppos = new java.text.ParsePosition(0);
                java.lang.Number actual = fmt.parse(tuple.parse, ppos);
                if (ppos.getIndex() == 0)
                {
                    return "Parse failed; got " + actual + ", but expected " + tuple.output;
                }
                if (tuple.output.Equals("fail", StringComparison.Ordinal))
                {
                    return null;
                }
                Number expected = toNumber(tuple.output);
                // number types cannot be compared, this is the best we can do.
                if (expected.ToDouble() != actual.doubleValue()
                    && !double.IsNaN(expected.ToDouble())
                    && !double.IsNaN(expected.ToDouble()))
                {
                    return "Expected: " + expected + ", got: " + actual;
                }
                return null;
            }

            /**
             * @param tuple
             * @return
             */
            private java.text.DecimalFormat createDecimalFormat(DataDrivenNumberFormatTestData tuple)
            {
                java.text.DecimalFormat fmt =
                    new java.text.DecimalFormat(
                        tuple.pattern == null ? "0" : tuple.pattern,
                        new java.text.DecimalFormatSymbols(
                            toLocale((tuple.locale == null ? EN : tuple.locale))));
                adjustDecimalFormat(tuple, fmt);
                return fmt;
            }

            private java.util.Locale toLocale(UCultureInfo uCultureInfo) // ICU4N: Convert UCultureInfo to java.util.Locale
            {
                string localeId = uCultureInfo.ToString();
#if FEATURE_SPAN
                using var parser = new LocaleIDParser(stackalloc char[32], localeId);
#else
                using var parser = new LocaleIDParser(localeID);
#endif

                string language = parser.GetLanguage();
                string country = parser.GetCountry();
                string variant = parser.GetVariant();
                return new java.util.Locale(language, country, variant);
            }

            /**
             * @param tuple
             * @param fmt
             */
            private void adjustDecimalFormat(
                DataDrivenNumberFormatTestData tuple, java.text.DecimalFormat fmt)
            {
                if (tuple.minIntegerDigits != null)
                {
                    fmt.setMinimumIntegerDigits(tuple.minIntegerDigits);
                }
                if (tuple.maxIntegerDigits != null)
                {
                    fmt.setMaximumIntegerDigits(tuple.maxIntegerDigits);
                }
                if (tuple.minFractionDigits != null)
                {
                    fmt.setMinimumFractionDigits(tuple.minFractionDigits);
                }
                if (tuple.maxFractionDigits != null)
                {
                    fmt.setMaximumFractionDigits(tuple.maxFractionDigits);
                }
                if (tuple.currency != null)
                {
                    fmt.setCurrency(java.util.Currency.getInstance(tuple.currency.ToString()));
                }
                if (tuple.minGroupingDigits != null)
                {
                    // Oops we don't support this.
                }
                if (tuple.useSigDigits != null)
                {
                    // Oops we don't support this
                }
                if (tuple.minSigDigits != null)
                {
                    // Oops we don't support this
                }
                if (tuple.maxSigDigits != null)
                {
                    // Oops we don't support this
                }
                if (tuple.useGrouping != null)
                {
                    fmt.setGroupingUsed(tuple.useGrouping != 0);
                }
                if (tuple.multiplier != null)
                {
                    fmt.setMultiplier(tuple.multiplier);
                }
                if (tuple.roundingIncrement != null)
                {
                    // Not supported
                }
                if (tuple.formatWidth != null)
                {
                    // Not supported
                }
                if (tuple.padCharacter != null && tuple.padCharacter.Length > 0)
                {
                    // Not supported
                }
                if (tuple.useScientific != null)
                {
                    // Not supported
                }
                if (tuple.grouping != null)
                {
                    fmt.setGroupingSize(tuple.grouping);
                }
                if (tuple.grouping2 != null)
                {
                    // Not supported
                }
                if (tuple.roundingMode != null)
                {
                    // Not supported
                }
                if (tuple.currencyUsage != null)
                {
                    // Not supported
                }
                if (tuple.minimumExponentDigits != null)
                {
                    // Not supported
                }
                if (tuple.exponentSignAlwaysShown != null)
                {
                    // Not supported
                }
                if (tuple.decimalSeparatorAlwaysShown != null)
                {
                    fmt.setDecimalSeparatorAlwaysShown(tuple.decimalSeparatorAlwaysShown != 0);
                }
                if (tuple.padPosition != null)
                {
                    // Not supported
                }
                if (tuple.positivePrefix != null)
                {
                    fmt.setPositivePrefix(tuple.positivePrefix);
                }
                if (tuple.positiveSuffix != null)
                {
                    fmt.setPositiveSuffix(tuple.positiveSuffix);
                }
                if (tuple.negativePrefix != null)
                {
                    fmt.setNegativePrefix(tuple.negativePrefix);
                }
                if (tuple.negativeSuffix != null)
                {
                    fmt.setNegativeSuffix(tuple.negativeSuffix);
                }
                if (tuple.localizedPattern != null)
                {
                    fmt.applyLocalizedPattern(tuple.localizedPattern);
                }

                // lenient parsing not supported by JDK
                if (tuple.parseIntegerOnly != null)
                {
                    fmt.setParseIntegerOnly(tuple.parseIntegerOnly != 0);
                }
                if (tuple.parseCaseSensitive != null)
                {
                    // Not supported.
                }
                if (tuple.decimalPatternMatchRequired != null)
                {
                    // Oops, not supported
                }
                if (tuple.parseNoExponent != null)
                {
                    // Oops, not supported for now
                }
            }
        }

        private DataDrivenNumberFormatTestUtility.CodeUnderTest JDK = new JDKCodeUnderTestAnonymousClass();

#endif

        static void propertiesFromTuple(DataDrivenNumberFormatTestData tuple, DecimalFormatProperties properties)
        {
            if (tuple.minIntegerDigits != null)
            {
                properties.MinimumIntegerDigits = (tuple.minIntegerDigits);
            }
            if (tuple.maxIntegerDigits != null)
            {
                properties.MaximumIntegerDigits = (tuple.maxIntegerDigits);
            }
            if (tuple.minFractionDigits != null)
            {
                properties.MinimumFractionDigits = (tuple.minFractionDigits);
            }
            if (tuple.maxFractionDigits != null)
            {
                properties.MaximumFractionDigits = (tuple.maxFractionDigits);
            }
            if (tuple.currency != null)
            {
                properties.Currency = (tuple.currency);
            }
            if (tuple.minGroupingDigits != null)
            {
                properties.MinimumGroupingDigits = (tuple.minGroupingDigits);
            }
            if (tuple.useSigDigits != null)
            {
                // TODO
            }
            if (tuple.minSigDigits != null)
            {
                properties.MinimumSignificantDigits = (tuple.minSigDigits);
            }
            if (tuple.maxSigDigits != null)
            {
                properties.MaximumSignificantDigits = (tuple.maxSigDigits);
            }
            if (tuple.useGrouping != null && tuple.useGrouping == 0)
            {
                properties.GroupingSize = (-1);
                properties.SecondaryGroupingSize = (-1);
            }
            if (tuple.multiplier != null)
            {
                properties.Multiplier = (new ICU4N.Numerics.BigMath.BigDecimal(tuple.multiplier));
            }
            if (tuple.roundingIncrement != null)
            {
                properties.RoundingIncrement = (ICU4N.Numerics.BigMath.BigDecimal.Parse(tuple.roundingIncrement.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
            }
            if (tuple.formatWidth != null)
            {
                properties.FormatWidth = (tuple.formatWidth);
            }
            if (tuple.padCharacter != null && tuple.padCharacter.Length > 0)
            {
                properties.PadString = (tuple.padCharacter.ToString());
            }
            if (tuple.useScientific != null)
            {
                properties.MinimumExponentDigits = (
                    tuple.useScientific != 0 ? 1 : -1);
            }
            if (tuple.grouping != null)
            {
                properties.GroupingSize = (tuple.grouping);
            }
            if (tuple.grouping2 != null)
            {
                properties.SecondaryGroupingSize = (tuple.grouping2);
            }
            if (tuple.roundingMode != null)
            {
                properties.RoundingMode = ((ICU4N.Numerics.BigMath.RoundingMode?)tuple.roundingMode);
            }
            if (tuple.currencyUsage != null)
            {
                properties.CurrencyUsage = (tuple.currencyUsage);
            }
            if (tuple.minimumExponentDigits != null)
            {
                properties.MinimumExponentDigits = (tuple.minimumExponentDigits.ToByte());
            }
            if (tuple.exponentSignAlwaysShown != null)
            {
                properties.ExponentSignAlwaysShown = (tuple.exponentSignAlwaysShown != 0);
            }
            if (tuple.decimalSeparatorAlwaysShown != null)
            {
                properties.DecimalSeparatorAlwaysShown = (tuple.decimalSeparatorAlwaysShown != 0);
            }
            if (tuple.padPosition != null)
            {
                properties.PadPosition = (tuple.padPosition.HasValue ? tuple.padPosition.Value.ToNew() : default);
            }
            if (tuple.positivePrefix != null)
            {
                properties.PositivePrefix = (tuple.positivePrefix);
            }
            if (tuple.positiveSuffix != null)
            {
                properties.PositiveSuffix = (tuple.positiveSuffix);
            }
            if (tuple.negativePrefix != null)
            {
                properties.NegativePrefix = (tuple.negativePrefix);
            }
            if (tuple.negativeSuffix != null)
            {
                properties.NegativeSuffix = (tuple.negativeSuffix);
            }
            if (tuple.localizedPattern != null)
            {
                DecimalFormatSymbols symbols = DecimalFormatSymbols.GetInstance(tuple.locale);
                String converted = PatternStringUtils.ConvertLocalized(tuple.localizedPattern, symbols, false);
                PatternStringParser.ParseToExistingProperties(converted, properties);
            }
            if (tuple.lenient != null)
            {
                properties.ParseMode = (tuple.lenient == 0 ? Parser.ParseMode.Strict : Parser.ParseMode.Lenient);
            }
            if (tuple.parseIntegerOnly != null)
            {
                properties.ParseIntegerOnly = (tuple.parseIntegerOnly != 0);
            }
            if (tuple.parseCaseSensitive != null)
            {
                properties.ParseCaseSensitive = (tuple.parseCaseSensitive != 0);
            }
            if (tuple.decimalPatternMatchRequired != null)
            {
                properties.DecimalPatternMatchRequired = (tuple.decimalPatternMatchRequired != 0);
            }
            if (tuple.parseNoExponent != null)
            {
                properties.ParseNoExponent = (tuple.parseNoExponent != 0);
            }
        }

        private class ICU60CodeUnderTestAnonymousClass : DataDrivenNumberFormatTestUtility.CodeUnderTest
        {
            public override char? Id => 'Q';

            public override string Format(DataDrivenNumberFormatTestData tuple)
            {
                String pattern = (tuple.pattern == null) ? "0" : tuple.pattern;
                UCultureInfo locale = (tuple.locale == null) ? new UCultureInfo("en") : tuple.locale;
                DecimalFormatProperties properties =
                    PatternStringParser.ParseToProperties(
                        pattern,
                        tuple.currency != null
                            ? PatternStringParser.IGNORE_ROUNDING_ALWAYS
                            : PatternStringParser.IGNORE_ROUNDING_NEVER);
                propertiesFromTuple(tuple, properties);
                DecimalFormatSymbols symbols = DecimalFormatSymbols.GetInstance(locale);
                LocalizedNumberFormatter fmt = NumberFormatter.FromDecimalFormat(properties, symbols, null).Culture(locale);
                Number number = toNumber(tuple.format);
                String expected = tuple.output;
                String actual = fmt.Format(number).ToString();
                if (!expected.Equals(actual, StringComparison.Ordinal))
                {
                    return "Expected \"" + expected + "\", got \"" + actual + "\"";
                }
                return null;
            }
        }

        /**
         * Formatting, but no other features.
         */
        private DataDrivenNumberFormatTestUtility.CodeUnderTest ICU60 = new ICU60CodeUnderTestAnonymousClass();


        private class DecimalFormatPropertySetter : DecimalFormat.IPropertySetter
        {
            private readonly DecimalFormatProperties properties;
            public DecimalFormatPropertySetter(DecimalFormatProperties properties)
            {
                this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
            }

            public void Set(DecimalFormatProperties props)
            {
                props.CopyFrom(properties);
            }
        }

        private class ICU60OtherCodeUnderTestAnonymousClass : DataDrivenNumberFormatTestUtility.CodeUnderTest
        {
            public override char? Id => 'S';

            /**
             * Runs a single toPattern test. On success, returns null. On failure, returns the error. This implementation
             * just returns null. Subclasses should override.
             *
             * @param tuple
             *            contains the parameters of the format test.
             */
            public override string ToPattern(DataDrivenNumberFormatTestData tuple)
            {
                String pattern = (tuple.pattern == null) ? "0" : tuple.pattern;
                DecimalFormatProperties properties;
                DecimalFormat df;
                try
                {
                    properties = PatternStringParser.ParseToProperties(
                            pattern,
                            tuple.currency != null ? PatternStringParser.IGNORE_ROUNDING_ALWAYS
                                    : PatternStringParser.IGNORE_ROUNDING_NEVER);
                    propertiesFromTuple(tuple, properties);
                    // TODO: Use PatternString.propertiesToString() directly. (How to deal with CurrencyUsage?)
                    df = new DecimalFormat();
                    df.SetProperties(new DecimalFormatPropertySetter(properties));
                }
                catch (ArgumentException e)
                {
                    //e.printStackTrace();
                    Console.WriteLine(e.ToString());
                    return e.ToString();
                }

                if (tuple.toPattern != null)
                {
                    String expected = tuple.toPattern;
                    String actual = df.ToPattern();
                    if (!expected.Equals(actual, StringComparison.Ordinal))
                    {
                        return "Expected toPattern='" + expected + "'; got '" + actual + "'";
                    }
                }
                if (tuple.toLocalizedPattern != null)
                {
                    String expected = tuple.toLocalizedPattern;
                    String actual = PatternStringUtils.PropertiesToPatternString(properties);
                    if (!expected.Equals(actual, StringComparison.Ordinal))
                    {
                        return "Expected toLocalizedPattern='" + expected + "'; got '" + actual + "'";
                    }
                }
                return null;
            }

            /**
             * Runs a single parse test. On success, returns null. On failure, returns the error. This implementation just
             * returns null. Subclasses should override.
             *
             * @param tuple
             *            contains the parameters of the format test.
             */
            public override string Parse(DataDrivenNumberFormatTestData tuple)
            {
                String pattern = (tuple.pattern == null) ? "0" : tuple.pattern;
                DecimalFormatProperties properties;
                ParsePosition ppos = new ParsePosition(0);
                Number actual;
                try
                {
                    properties = PatternStringParser.ParseToProperties(
                            pattern,
                            tuple.currency != null ? PatternStringParser.IGNORE_ROUNDING_ALWAYS
                                    : PatternStringParser.IGNORE_ROUNDING_NEVER);
                    propertiesFromTuple(tuple, properties);
                    actual = Parser.Parse(tuple.parse, ppos, properties, DecimalFormatSymbols.GetInstance(tuple.locale));
                }
                catch (ArgumentException e)
                {
                    return "parse exception: " + e.Message;
                }
                if (actual == null && ppos.Index != 0)
                {
                    throw new AssertionException("Error: value is null but parse position is not zero");
                }
                if (ppos.Index == 0)
                {
                    return "Parse failed; got " + actual + ", but expected " + tuple.output;
                }
                if (tuple.output.Equals("NaN", StringComparison.Ordinal))
                {
                    if (!double.IsNaN(actual.ToDouble()))
                    {
                        return "Expected NaN, but got: " + actual;
                    }
                    return null;
                }
                else if (tuple.output.Equals("Inf", StringComparison.Ordinal))
                {
                    if (!double.IsInfinity(actual.ToDouble()) || JCG.Comparer<double>.Default.Compare(actual.ToDouble(), 0.0) < 0)
                    {
                        return "Expected Inf, but got: " + actual;
                    }
                    return null;
                }
                else if (tuple.output.Equals("-Inf", StringComparison.Ordinal))
                {
                    if (!double.IsInfinity(actual.ToDouble()) || JCG.Comparer<double>.Default.Compare(actual.ToDouble(), 0.0) > 0)
                    {
                        return "Expected -Inf, but got: " + actual;
                    }
                    return null;
                }
                else if (tuple.output.Equals("fail", StringComparison.Ordinal))
                {
                    return null;
                }
                else if (ICU4N.Numerics.BigMath.BigDecimal.Parse(tuple.output).CompareTo(ICU4N.Numerics.BigMath.BigDecimal.Parse(actual.ToString(CultureInfo.InvariantCulture))) != 0)
                {
                    return "Expected: " + tuple.output + ", got: " + actual;
                }
                else
                {
                    return null;
                }
            }

            /**
             * Runs a single parse currency test. On success, returns null. On failure, returns the error. This
             * implementation just returns null. Subclasses should override.
             *
             * @param tuple
             *            contains the parameters of the format test.
             */
            public override string ParseCurrency(DataDrivenNumberFormatTestData tuple)
            {
                String pattern = (tuple.pattern == null) ? "0" : tuple.pattern;
                DecimalFormatProperties properties;
                ParsePosition ppos = new ParsePosition(0);
                CurrencyAmount actual;
                try
                {
                    properties = PatternStringParser.ParseToProperties(
                            pattern,
                            tuple.currency != null ? PatternStringParser.IGNORE_ROUNDING_ALWAYS
                                    : PatternStringParser.IGNORE_ROUNDING_NEVER);
                    propertiesFromTuple(tuple, properties);
                    actual = Parser
                            .ParseCurrency(tuple.parse, ppos, properties, DecimalFormatSymbols.GetInstance(tuple.locale));
                }
                catch (ParseException e)
                {
                    //e.PrintStackTrace();
                    Console.WriteLine(e.ToString());
                    return "parse exception: " + e.Message;
                }
                if (ppos.Index == 0 || actual.Currency.CurrencyCode.Equals("XXX", StringComparison.Ordinal))
                {
                    return "Parse failed; got " + actual + ", but expected " + tuple.output;
                }
                ICU4N.Numerics.BigMath.BigDecimal expectedNumber = ICU4N.Numerics.BigMath.BigDecimal.Parse(tuple.output, CultureInfo.InvariantCulture);
                if (expectedNumber.CompareTo(ICU4N.Numerics.BigMath.BigDecimal.Parse(actual.Number.ToString(CultureInfo.InvariantCulture))) != 0)
                {
                    return "Wrong number: Expected: " + expectedNumber + ", got: " + actual;
                }
                String expectedCurrency = tuple.outputCurrency;
                if (!expectedCurrency.Equals(actual.Currency.ToString()))
                {
                    return "Wrong currency: Expected: " + expectedCurrency + ", got: " + actual;
                }
                return null;
            }

            /**
             * Runs a single select test. On success, returns null. On failure, returns the error. This implementation just
             * returns null. Subclasses should override.
             *
             * @param tuple
             *            contains the parameters of the format test.
             */
            public override string Select(DataDrivenNumberFormatTestData tuple)
            {
                return null;
            }
        }

        /**
         * All features except formatting.
         */
        private DataDrivenNumberFormatTestUtility.CodeUnderTest ICU60_Other = new ICU60OtherCodeUnderTestAnonymousClass();


        [Test]
        [Ignore("ICU4N TODO: Finish implementation - either use IKVM to fake this or do a port of the formatter from ICU58.")]
        public void TestDataDrivenICU58()
        {
            // ICU4N TODO:
            // Android can't access DecimalFormat_ICU58 for testing (ticket #13283).
            //if (TestUtil.getJavaVendor() == TestUtil.JavaVendor.Android) return;

            //DataDrivenNumberFormatTestUtility.runFormatSuiteIncludingKnownFailures(
            //    "numberformattestspecification.txt", ICU58);
            throw new NotImplementedException();
        }

#if FEATURE_IKVM
        // Note: This test case is really questionable. Depending on Java version,
        // something may or may not work. However the test data assumes a specific
        // Java runtime version. We should probably disable this test case - #13372
        [Test]
        public void TestDataDrivenJDK()
        {
            // ICU4N TODO:
            // Android implements java.text.DecimalFormat with ICU4J (ticket #13322).
            // Oracle/OpenJDK 9's behavior is not exactly same with Oracle/OpenJDK 8.
            // Some test cases failed on 8 work well, while some other test cases
            // fail on 9, but worked on 8. Skip this test case if Java version is not 8.
            ////org.junit.Assume.assumeTrue(
            ////        TestUtil.getJavaVendor() != TestUtil.JavaVendor.Android
            ////        && TestUtil.getJavaVersion() < 9);

            DataDrivenNumberFormatTestUtility.runFormatSuiteIncludingKnownFailures(
                "numberformattestspecification.txt", JDK);
        }
#endif

        [Test]
        public void TestDataDrivenICULatest_Format()
        {
            DataDrivenNumberFormatTestUtility.runFormatSuiteIncludingKnownFailures(
                "numberformattestspecification.txt", ICU60);
        }

        [Test]
        public void TestDataDrivenICULatest_Other()
        {
            DataDrivenNumberFormatTestUtility.runFormatSuiteIncludingKnownFailures(
                "numberformattestspecification.txt", ICU60_Other);
        }
    }
}
