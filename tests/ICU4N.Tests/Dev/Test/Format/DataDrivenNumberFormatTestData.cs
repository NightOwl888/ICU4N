using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integer = J2N.Numerics.Int32;
using Double = J2N.Numerics.Double;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// A representation of a single NumberFormat specification test from a data driven test file.
    /// <para/>
    /// The purpose of this class is to hide the details of the data driven test file from the
    /// main testing code.
    /// <para/>
    /// This class contains fields describing an attribute of the test that may or may
    /// not be set. The name of each attribute corresponds to the name used in the
    /// data driven test file.
    /// <para/>
    /// <b>Adding new attributes</b>
    /// <para/>
    /// Each attribute name is lower case. Moreover, for each attribute there is also a
    /// setXXX method for that attribute that is used to initialize the attribute from a
    /// string value read from the data file. For example, there is a setLocale(String) method
    /// for the locale attribute and a setCurrency(String) method for the currency attribute.
    /// In general, for an attribute named abcd, the setter will be setAbcd(String).
    /// This naming rule must be strictly followed or else the test runner will not know how to
    /// initialize instances of this class.
    /// <para/>
    /// In addition each attribute is listed in the fieldOrdering static array which specifies
    /// The order that attributes are printed whenever there is a test failure.
    /// <para/>
    /// To add a new attribute, first create a public field for it.
    /// Next, add the attribute name to the fieldOrdering array.
    /// Finally, create a setter method for it.
    /// </summary>
    /// <author>rocketman</author>
    internal class DataDrivenNumberFormatTestData
    {
        /**
         * The locale.
         */
        public UCultureInfo locale = null;

        /**
         * The currency.
         */
        public Currency currency = null;

        /**
         * The pattern to initialize the formatter, for example 0.00"
         */
        public string pattern = null;

        /**
         * The value to format as a string. For example 1234.5 would be "1234.5"
         */
        public string format = null;

        /**
         * The formatted value.
         */
        public string output = null;

        /**
         * Field for arbitrary comments.
         */
        public string comment = null;

        public Integer minIntegerDigits = null;
        public Integer maxIntegerDigits = null;
        public Integer minFractionDigits = null;
        public Integer maxFractionDigits = null;
        public Integer minGroupingDigits = null;
        public Integer useSigDigits = null;
        public Integer minSigDigits = null;
        public Integer maxSigDigits = null;
        public Integer useGrouping = null;
        public Integer multiplier = null;
        public Double roundingIncrement = null;
        public Integer formatWidth = null;
        public string padCharacter = null;
        public Integer useScientific = null;
        public Integer grouping = null;
        public Integer grouping2 = null;
        public RoundingMode? roundingMode = null;
        public CurrencyUsage? currencyUsage = null;
        public Integer minimumExponentDigits = null;
        public Integer exponentSignAlwaysShown = null;
        public Integer decimalSeparatorAlwaysShown = null;
        public PadPosition? padPosition = null;
        public string positivePrefix = null;
        public string positiveSuffix = null;
        public string negativePrefix = null;
        public string negativeSuffix = null;
        public string localizedPattern = null;
        public string toPattern = null;
        public string toLocalizedPattern = null;
        public NumberFormatStyle? style = null;
        public string parse = null;
        public Integer lenient = null;
        public string plural = null;
        public Integer parseIntegerOnly = null;
        public Integer decimalPatternMatchRequired = null;
        public Integer parseCaseSensitive = null;
        public Integer parseNoExponent = null;
        public string outputCurrency = null;



        /**
         * nothing or empty means that test ought to work for both C and JAVA;
         * "C" means test is known to fail in C. "J" means test is known to fail in JAVA.
         * "CJ" means test is known to fail for both languages.
         */
        public string breaks = null;

        private static IDictionary<String, int> roundingModeMap =
                new Dictionary<String, int>()
                {
                    {"ceiling", (int)BigDecimal.RoundCeiling },
                    { "floor", (int)BigDecimal.RoundFloor },
                    { "down", (int)BigDecimal.RoundDown },
                    {"up", (int)BigDecimal.RoundUp },
                    {"halfEven", (int)BigDecimal.RoundHalfEven },
                    {"halfDown", (int)BigDecimal.RoundHalfDown },
                    {"halfUp", (int)BigDecimal.RoundHalfUp },
                    { "unnecessary", (int)BigDecimal.RoundUnnecessary },
                };

        //    static {
        //    roundingModeMap.put("ceiling", BigDecimal.ROUND_CEILING);
        //    roundingModeMap.put("floor", BigDecimal.ROUND_FLOOR);
        //    roundingModeMap.put("down", BigDecimal.ROUND_DOWN);
        //    roundingModeMap.put("up", BigDecimal.ROUND_UP);
        //    roundingModeMap.put("halfEven", BigDecimal.ROUND_HALF_EVEN);
        //    roundingModeMap.put("halfDown", BigDecimal.ROUND_HALF_DOWN);
        //    roundingModeMap.put("halfUp", BigDecimal.ROUND_HALF_UP);
        //    roundingModeMap.put("unnecessary", BigDecimal.ROUND_UNNECESSARY);
        //}

        private static IDictionary<String, CurrencyUsage> currencyUsageMap =
                new Dictionary<String, CurrencyUsage>()
                {
                    { "standard", CurrencyUsage.Standard },
                    { "cash", CurrencyUsage.Cash },
                };

        //static {
        //    currencyUsageMap.put("standard", Currency.CurrencyUsage.STANDARD);
        //    currencyUsageMap.put("cash", Currency.CurrencyUsage.CASH);
        //}

        private static IDictionary<String, PadPosition> padPositionMap =
                new Dictionary<String, PadPosition>
                {
                    {"beforePrefix", PadPosition.BeforePrefix },
                    {"afterPrefix", PadPosition.AfterPrefix },
                    {"beforeSuffix", PadPosition.BeforeSuffix },
                    {"afterSuffix", PadPosition.AfterSuffix },
                };


        //static
        //{
        //    // TODO: Fix so that it doesn't depend on DecimalFormat.
        //    padPositionMap.put("beforePrefix", DecimalFormat.PAD_BEFORE_PREFIX);
        //    padPositionMap.put("afterPrefix", DecimalFormat.PAD_AFTER_PREFIX);
        //    padPositionMap.put("beforeSuffix", DecimalFormat.PAD_BEFORE_SUFFIX);
        //    padPositionMap.put("afterSuffix", DecimalFormat.PAD_AFTER_SUFFIX);
        //}

        private static IDictionary<String, NumberFormatStyle> formatStyleMap =
                new Dictionary<String, NumberFormatStyle>
                {
                    { "decimal", NumberFormatStyle.NumberStyle },
                    { "currency", NumberFormatStyle.CurrencyStyle },
                    { "percent", NumberFormatStyle.PercentStyle },
                    { "scientific", NumberFormatStyle.ScientificStyle },
                    { "currencyIso", NumberFormatStyle.ISOCurrencyStyle },
                    { "currencyPlural", NumberFormatStyle.PluralCurrencyStyle },
                    { "currencyAccounting", NumberFormatStyle.AccountingCurrencyStyle },
                    { "cashCurrency", NumberFormatStyle.CashCurrencyStyle },
                };

        //static
        //{
        //    formatStyleMap.put("decimal", NumberFormat.NUMBERSTYLE);
        //    formatStyleMap.put("currency", NumberFormat.CURRENCYSTYLE);
        //    formatStyleMap.put("percent", NumberFormat.PERCENTSTYLE);
        //    formatStyleMap.put("scientific", NumberFormat.SCIENTIFICSTYLE);
        //    formatStyleMap.put("currencyIso", NumberFormat.ISOCURRENCYSTYLE);
        //    formatStyleMap.put("currencyPlural", NumberFormat.PLURALCURRENCYSTYLE);
        //    formatStyleMap.put("currencyAccounting", NumberFormat.ACCOUNTINGCURRENCYSTYLE);
        //    formatStyleMap.put("cashCurrency", NumberFormat.CASHCURRENCYSTYLE);
        //}

        // Add any new fields here. On test failures, fields are printed in the same order they
        // appear here.
        private static String[] fieldOrdering = {
            "locale",
            "currency",
            "pattern",
            "format",
            "output",
            "comment",
            "minIntegerDigits",
            "maxIntegerDigits",
            "minFractionDigits",
            "maxFractionDigits",
            "minGroupingDigits",
            "breaks",
            "useSigDigits",
            "minSigDigits",
            "maxSigDigits",
            "useGrouping",
            "multiplier",
            "roundingIncrement",
            "formatWidth",
            "padCharacter",
            "useScientific",
            "grouping",
            "grouping2",
            "roundingMode",
            "currencyUsage",
            "minimumExponentDigits",
            "exponentSignAlwaysShown",
            "decimalSeparatorAlwaysShown",
            "padPosition",
            "positivePrefix",
            "positiveSuffix",
            "negativePrefix",
            "negativeSuffix",
            "localizedPattern",
            "toPattern",
            "toLocalizedPattern",
            "style",
            "parse",
            "lenient",
            "plural",
            "parseIntegerOnly",
            "decimalPatternMatchRequired",
            "parseNoExponent",
            "outputCurrency"
        };

        static DataDrivenNumberFormatTestData()
        {
            HashSet<string> set = new HashSet<string>();
            foreach (string s in fieldOrdering)
            {
                if (!set.Add(s))
                {
                    throw new Exception(s + "is a duplicate field.");
                }
            }
        }

        private static T fromString<T>(IDictionary<string, T> map, string key)
        {
            if (!map.TryGetValue(key, out T value) || value == null)
            {
                throw new ArgumentException("Bad value: " + key);
            }
            return value;
        }

        // start field setters.
        // add setter for each new field in this block.

        public void setLocale(string value)
        {
            locale = new UCultureInfo(value);
        }

        public void setCurrency(string value)
        {
            currency = Currency.GetInstance(value);
        }

        public void setPattern(string value)
        {
            pattern = value;
        }

        public void setFormat(string value)
        {
            format = value;
        }

        public void setOutput(string value)
        {
            output = value;
        }

        public void setComment(string value)
        {
            comment = value;
        }

        public void setMinIntegerDigits(string value)
        {
            minIntegerDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMaxIntegerDigits(string value)
        {
            maxIntegerDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMinFractionDigits(string value)
        {
            minFractionDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMaxFractionDigits(string value)
        {
            maxFractionDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMinGroupingDigits(string value)
        {
            minGroupingDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setBreaks(string value)
        {
            breaks = value;
        }

        public void setUseSigDigits(string value)
        {
            useSigDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMinSigDigits(string value)
        {
            minSigDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMaxSigDigits(string value)
        {
            maxSigDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setUseGrouping(string value)
        {
            useGrouping = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setMultiplier(string value)
        {
            multiplier = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setRoundingIncrement(string value)
        {
            roundingIncrement = Double.GetInstance(Double.Parse(value, CultureInfo.InvariantCulture)); // ICU4N TODO: Accuracy
        }

        public void setFormatWidth(string value)
        {
            formatWidth = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setPadCharacter(string value)
        {
            padCharacter = value;
        }

        public void setUseScientific(string value)
        {
            useScientific = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setGrouping(string value)
        {
            grouping = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setGrouping2(string value)
        {
            grouping2 = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setRoundingMode(string value)
        {
            roundingMode = (RoundingMode?)fromString(roundingModeMap, value);
        }

        public void setCurrencyUsage(string value)
        {
            currencyUsage = fromString(currencyUsageMap, value);
        }

        public void setMinimumExponentDigits(string value)
        {
            minimumExponentDigits = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setExponentSignAlwaysShown(string value)
        {
            exponentSignAlwaysShown = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setDecimalSeparatorAlwaysShown(string value)
        {
            decimalSeparatorAlwaysShown = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setPadPosition(string value)
        {
            padPosition = fromString(padPositionMap, value);
        }

        public void setPositivePrefix(string value)
        {
            positivePrefix = value;
        }

        public void setPositiveSuffix(string value)
        {
            positiveSuffix = value;
        }

        public void setNegativePrefix(string value)
        {
            negativePrefix = value;
        }

        public void setNegativeSuffix(string value)
        {
            negativeSuffix = value;
        }

        public void setLocalizedPattern(string value)
        {
            localizedPattern = value;
        }

        public void setToPattern(string value)
        {
            toPattern = value;
        }

        public void setToLocalizedPattern(string value)
        {
            toLocalizedPattern = value;
        }

        public void setStyle(string value)
        {
            style = fromString(formatStyleMap, value);
        }

        public void setParse(string value)
        {
            parse = value;
        }

        public void setLenient(string value)
        {
            lenient = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setPlural(string value)
        {
            plural = value;
        }

        public void setParseIntegerOnly(string value)
        {
            parseIntegerOnly = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setParseCaseSensitive(string value)
        {
            parseCaseSensitive = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setDecimalPatternMatchRequired(string value)
        {
            decimalPatternMatchRequired = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setParseNoExponent(string value)
        {
            parseNoExponent = Integer.GetInstance(Integer.Parse(value, CultureInfo.InvariantCulture));
        }

        public void setOutputCurrency(string value)
        {
            outputCurrency = value;
        }

        // end field setters.

        // start of field clearers
        // Add clear methods that can be set in one test and cleared
        // in the next i.e the breaks field.

        public void clearBreaks()
        {
            breaks = null;
        }

        public void clearUseGrouping()
        {
            useGrouping = null;
        }

        public void clearGrouping2()
        {
            grouping2 = null;
        }

        public void clearGrouping()
        {
            grouping = null;
        }

        public void clearMinGroupingDigits()
        {
            minGroupingDigits = null;
        }

        public void clearUseScientific()
        {
            useScientific = null;
        }

        public void clearDecimalSeparatorAlwaysShown()
        {
            decimalSeparatorAlwaysShown = null;
        }

        // end field clearers

        public void setField(string fieldName, string valueString)
        // throws NoSuchMethodException
        {
            MethodInfo m = GetType().GetMethod(
                        fieldToSetter(fieldName), new Type[] { typeof(string) });
            //try
            //{
            m.Invoke(this, new object[] { valueString });
            //}
            //catch (IllegalAccessException e)
            //{
            //    throw new RuntimeException(e);
            //}
            //catch (InvocationTargetException e)
            //{
            //    throw new RuntimeException(e);
            //}
        }

        public void clearField(string fieldName)
        //throws NoSuchMethodException
        {
            MethodInfo m = GetType().GetMethod(fieldToClearer(fieldName));
            //try {
            m.Invoke(this, new object[0]);
            //} catch (IllegalAccessException e) {
            //    throw new RuntimeException(e);
            //} catch (InvocationTargetException e) {
            //    throw new RuntimeException(e);
            //}
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("{");
            bool first = true;
            foreach (string fieldName in fieldOrdering)
            {
                //try
                //{
                FieldInfo field = GetType().GetField(fieldName);
                object optionalValue = field.GetValue(this);
                if (optionalValue == null)
                {
                    continue;
                }
                if (!first)
                {
                    result.Append(", ");
                }
                first = false;
                result.Append(fieldName);
                result.Append(": ");
                result.Append(optionalValue);
                //}
                //catch (NoSuchFieldException e)
                //{
                //    throw new RuntimeException(e);
                //}
                //catch (SecurityException e)
                //{
                //    throw new RuntimeException(e);
                //}
                //catch (IllegalAccessException e)
                //{
                //    throw new RuntimeException(e);
                //}
            }
            result.Append("}");
            return result.ToString();
        }

        private static string fieldToSetter(string fieldName)
        {
            return "set"
                    + char.ToUpperInvariant(fieldName[0])
                    + fieldName.Substring(1);
        }

        private static string fieldToClearer(string fieldName)
        {
            return "clear"
                    + char.ToUpperInvariant(fieldName[0])
                    + fieldName.Substring(1);
        }
    }
}
