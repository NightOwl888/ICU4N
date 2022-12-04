using ICU4N.Globalization;
using ICU4N.Text;
using J2N;
using J2N.Collections;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Dev.Test.Format
{
    public class IntlTestDecimalFormatSymbols : TestFmwk
    {
        // Test the API of DecimalFormatSymbols; primarily a simple get/set set.
        [Test]
        public void TestSymbols()
        {
            DecimalFormatSymbols fr = new DecimalFormatSymbols(new CultureInfo("fr"));
            DecimalFormatSymbols en = new DecimalFormatSymbols(new CultureInfo("en"));

            if (en.Equals(fr))
            {
                Errln("ERROR: English DecimalFormatSymbols equal to French");
            }

            // just do some VERY basic tests to make sure that get/set work

            if (!en.Culture.Equals(new CultureInfo("en")))
            {
                Errln("ERROR: getLocale failed");
            }
            if (!en.UCulture.Equals(new UCultureInfo("en")))
            {
                Errln("ERROR: getULocale failed");
            }

            if (!en.Culture.Equals(new CultureInfo("en")))
            {
                Errln("ERROR: getLocale failed");
            }
            if (!en.UCulture.Equals(new UCultureInfo("en")))
            {
                Errln("ERROR: getULocale failed");
            }

            char zero = en.ZeroDigit;
            fr.ZeroDigit = (zero);
            if (fr.ZeroDigit != en.ZeroDigit)
            {
                Errln("ERROR: get/set ZeroDigit failed");
            }

            String[] digits = en.DigitStrings;
            fr.DigitStrings = (digits);
            if (!ArrayEqualityComparer<string>.OneDimensional.Equals(fr.DigitStrings, en.DigitStrings))
            {
                Errln("ERROR: get/set DigitStrings failed");
            }

            char sigDigit = en.SignificantDigit;
            fr.SignificantDigit = (sigDigit);
            if (fr.SignificantDigit != en.SignificantDigit)
            {
                Errln("ERROR: get/set SignificantDigit failed");
            }

            // ICU4N TODO: Currency
            //Currency currency = Currency.getInstance("USD");
            //fr.setCurrency(currency);
            //if (!fr.getCurrency().equals(currency))
            //{
            //    Errln("ERROR: get/set Currency failed");
            //}

            char group = en.GroupingSeparator;
            fr.GroupingSeparator = (group);
            if (fr.GroupingSeparator != en.GroupingSeparator)
            {
                Errln("ERROR: get/set GroupingSeparator failed");
            }

            String groupStr = en.GroupingSeparatorString;
            fr.GroupingSeparatorString = (groupStr);
            if (!fr.GroupingSeparatorString.Equals(en.GroupingSeparatorString, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set GroupingSeparatorString failed");
            }

            char @decimal = en.DecimalSeparator;
            fr.DecimalSeparator = (@decimal);
            if (fr.DecimalSeparator != en.DecimalSeparator)
            {
                Errln("ERROR: get/set DecimalSeparator failed");
            }

            String decimalStr = en.DecimalSeparatorString;
            fr.DecimalSeparatorString = (decimalStr);
            if (!fr.DecimalSeparatorString.Equals(en.DecimalSeparatorString))
            {
                Errln("ERROR: get/set DecimalSeparatorString failed");
            }

            char monetaryGroup = en.MonetaryGroupingSeparator;
            fr.MonetaryGroupingSeparator = (monetaryGroup);
            if (fr.MonetaryGroupingSeparator != en.MonetaryGroupingSeparator)
            {
                Errln("ERROR: get/set MonetaryGroupingSeparator failed");
            }

            String monetaryGroupStr = en.MonetaryGroupingSeparatorString;
            fr.MonetaryGroupingSeparatorString = (monetaryGroupStr);
            if (!fr.MonetaryGroupingSeparatorString.Equals(en.MonetaryGroupingSeparatorString, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set MonetaryGroupingSeparatorString failed");
            }

            char monetaryDecimal = en.MonetaryDecimalSeparator;
            fr.MonetaryDecimalSeparator = (monetaryDecimal);
            if (fr.MonetaryDecimalSeparator != en.MonetaryDecimalSeparator)
            {
                Errln("ERROR: get/set MonetaryDecimalSeparator failed");
            }

            String monetaryDecimalStr = en.MonetaryDecimalSeparatorString;
            fr.MonetaryDecimalSeparatorString = (monetaryDecimalStr);
            if (!fr.MonetaryDecimalSeparatorString.Equals(en.MonetaryDecimalSeparatorString, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set MonetaryDecimalSeparatorString failed");
            }

            char perMill = en.PerMill;
            fr.PerMill = (perMill);
            if (fr.PerMill != en.PerMill)
            {
                Errln("ERROR: get/set PerMill failed");
            }

            String perMillStr = en.PerMillString;
            fr.PerMillString = (perMillStr);
            if (!fr.PerMillString.Equals(en.PerMillString))
            {
                Errln("ERROR: get/set PerMillString failed");
            }

            char percent = en.Percent;
            fr.Percent = (percent);
            if (fr.Percent != en.Percent)
            {
                Errln("ERROR: get/set Percent failed");
            }

            String percentStr = en.PercentString;
            fr.PercentString = (percentStr);
            if (!fr.PercentString.Equals(en.PercentString))
            {
                Errln("ERROR: get/set PercentString failed");
            }

            char digit = en.Digit;
            fr.Digit = (digit);
            if (fr.Digit != en.Digit)
            {
                Errln("ERROR: get/set Digit failed");
            }

            char patternSeparator = en.PatternSeparator;
            fr.PatternSeparator = (patternSeparator);
            if (fr.PatternSeparator != en.PatternSeparator)
            {
                Errln("ERROR: get/set PatternSeparator failed");
            }

            String infinity = en.Infinity;
            fr.Infinity = (infinity);
            String infinity2 = fr.Infinity;
            if (!infinity.Equals(infinity2, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set Infinity failed");
            }

            String nan = en.NaN;
            fr.NaN = (nan);
            String nan2 = fr.NaN;
            if (!nan.Equals(nan2, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set NaN failed");
            }

            char minusSign = en.MinusSign;
            fr.MinusSign = (minusSign);
            if (fr.MinusSign != en.MinusSign)
            {
                Errln("ERROR: get/set MinusSign failed");
            }

            String minusSignStr = en.MinusSignString;
            fr.MinusSignString = (minusSignStr);
            if (!fr.MinusSignString.Equals(en.MinusSignString, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set MinusSignString failed");
            }

            char plusSign = en.PlusSign;
            fr.PlusSign = (plusSign);
            if (fr.PlusSign != en.PlusSign)
            {
                Errln("ERROR: get/set PlusSign failed");
            }

            String plusSignStr = en.PlusSignString;
            fr.PlusSignString = (plusSignStr);
            if (!fr.PlusSignString.Equals(en.PlusSignString, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set PlusSignString failed");
            }

            char padEscape = en.PadEscape;
            fr.PadEscape = (padEscape);
            if (fr.PadEscape != en.PadEscape)
            {
                Errln("ERROR: get/set PadEscape failed");
            }

            String exponential = en.ExponentSeparator;
            fr.ExponentSeparator = (exponential);
            if (fr.ExponentSeparator != en.ExponentSeparator)
            {
                Errln("ERROR: get/set Exponential failed");
            }

            String exponentMultiplicationSign = en.ExponentMultiplicationSign;
            fr.ExponentMultiplicationSign = (exponentMultiplicationSign);
            if (fr.ExponentMultiplicationSign != en.ExponentMultiplicationSign)
            {
                Errln("ERROR: get/set ExponentMultiplicationSign failed");
            }

            // Test CurrencySpacing.
            // In CLDR 1.7, only root.txt has CurrencySpacing data. This data might
            // be different between en and fr in the future.
            for (int i = DecimalFormatSymbols.CURRENCY_SPC_CURRENCY_MATCH; i <= DecimalFormatSymbols.CURRENCY_SPC_INSERT; i++)
            {
                if (en.GetPatternForCurrencySpacing(i, true) !=
                    fr.GetPatternForCurrencySpacing(i, true))
                {
                    Errln("ERROR: get currency spacing item:" + i + " before the currency");
                    if (en.GetPatternForCurrencySpacing(i, false) !=
                        fr.GetPatternForCurrencySpacing(i, false))
                    {
                        Errln("ERROR: get currency spacing item:" + i + " after currency");
                    }
                }
            }

            String dash = "-";
            en.SetPatternForCurrencySpacing(DecimalFormatSymbols.CURRENCY_SPC_INSERT, true, dash);
            if (dash != en.GetPatternForCurrencySpacing(DecimalFormatSymbols.CURRENCY_SPC_INSERT, true))
            {
                Errln("ERROR: set currency spacing pattern for before currency.");
            }

            //DecimalFormatSymbols foo = new DecimalFormatSymbols(); //The variable is never used

            en = (DecimalFormatSymbols)fr.Clone();

            if (!en.Equals(fr))
            {
                Errln("ERROR: Clone failed");
            }
        }

        [Test]
        public void TestCoverage()
        {
            DecimalFormatSymbols df = new DecimalFormatSymbols();
            DecimalFormatSymbols df2 = (DecimalFormatSymbols)df.Clone();
            if (!df.Equals(df2) || df.GetHashCode() != df2.GetHashCode())
            {
                Errln("decimal format symbols clone, equals, or hashCode failed");
            }
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependency DecimalFormat")]
        public void TestPropagateZeroDigit()
        {
            //DecimalFormatSymbols dfs = new DecimalFormatSymbols();
            //dfs.ZeroDigit = ('\u1040');
            //DecimalFormat df = new DecimalFormat("0");
            //df.SetDecimalFormatSymbols(dfs);
            //assertEquals("Should propagate char with number property zero",
            //        '\u1041', dfs.Digits[1]);
            //assertEquals("Should propagate char with number property zero",
            //        "\u1044\u1040\u1041\u1042\u1043", df.Format(40123));
            //dfs.ZeroDigit = ('a');
            //df.SetDecimalFormatSymbols(dfs);
            //assertEquals("Should propagate char WITHOUT number property zero",
            //        'b', dfs.Digits[1]);
            //assertEquals("Should propagate char WITHOUT number property zero",
            //        "eabcd", df.Format(40123));
        }

        [Test]
        public void TestDigitSymbols()
        {
            char defZero = '0';
            char[] defDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string[] defDigitStrings = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] osmanyaDigitStrings = new string[] {
                "\uD801\uDCA0", "\uD801\uDCA1", "\uD801\uDCA2", "\uD801\uDCA3", "\uD801\uDCA4",
                "\uD801\uDCA5", "\uD801\uDCA6", "\uD801\uDCA7", "\uD801\uDCA8", "\uD801\uDCA9"
            };
            String[] differentDigitStrings = { "0", "b", "3", "d", "5", "ff", "7", "h", "9", "j" };

            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new CultureInfo("en"));

            symbols.DigitStrings = (osmanyaDigitStrings);
            if (!ArrayEqualityComparer<string>.OneDimensional.Equals(symbols.DigitStrings, osmanyaDigitStrings))
            {
                Errln("ERROR: Osmanya digits (supplementary) should be set");
            }
            if (Character.CodePointAt(osmanyaDigitStrings[0], 0) != symbols.CodePointZero)
            {
                Errln("ERROR: Code point zero be Osmanya code point zero");
            }
            if (defZero != symbols.ZeroDigit)
            {
                Errln("ERROR: Zero digit should be 0");
            }
            if (!ArrayEqualityComparer<char>.OneDimensional.Equals(symbols.Digits, defDigits))
            {
                Errln("ERROR: Char digits should be Latin digits");
            }

            symbols.DigitStrings = (differentDigitStrings);
            if (!ArrayEqualityComparer<string>.OneDimensional.Equals(symbols.DigitStrings, differentDigitStrings))
            {
                Errln("ERROR: Different digits should be set");
            }
            if (-1 != symbols.CodePointZero)
            {
                Errln("ERROR: Code point zero should be invalid");
            }

            // Reset digits to Latin
            symbols.ZeroDigit = (defZero);
            if (!ArrayEqualityComparer<string>.OneDimensional.Equals(symbols.DigitStrings, defDigitStrings))
            {
                Errln("ERROR: Latin digits should be set" + symbols.DigitStrings[0]);
            }
            if (defZero != symbols.CodePointZero)
            {
                Errln("ERROR: Code point zero be ASCII 0");
            }
        }

        [Test]
        public void TestNumberingSystem()
        {
            object[][] cases = new object[][] {
                new object[] {"en", "latn", "1,234.56", ';'},
                new object[] {"en", "arab", "١٬٢٣٤٫٥٦", '؛'},
                new object[] {"en", "mathsanb", "𝟭,𝟮𝟯𝟰.𝟱𝟲", ';'},
                new object[] {"en", "mymr", "၁,၂၃၄.၅၆", ';'},
                new object[] {"my", "latn", "1,234.56", ';'},
                new object[] {"my", "arab", "١٬٢٣٤٫٥٦", '؛'},
                new object[] {"my", "mathsanb", "𝟭,𝟮𝟯𝟰.𝟱𝟲", ';'},
                new object[] {"my", "mymr", "၁,၂၃၄.၅၆", '၊'},
                new object[] {"en@numbers=thai", "mymr", "၁,၂၃၄.၅၆", ';'}, // conflicting numbering system
            };

            foreach (Object[] cas in cases)
            {
                UCultureInfo loc = new UCultureInfo((string)cas[0]);
                NumberingSystem ns = NumberingSystem.GetInstanceByName((string)cas[1]);
                String expectedFormattedNumberString = (string)cas[2];
                char expectedPatternSeparator = (char)cas[3];

                DecimalFormatSymbols dfs = DecimalFormatSymbols.ForNumberingSystem(loc, ns);
                // ICU4N TODO: DecimalFormat
                //DecimalFormat df = new DecimalFormat("#,##0.##", dfs);
                //String actual1 = df.Format(1234.56);
                //assertEquals("1234.56 with " + loc + " and " + ns.Name,
                //        expectedFormattedNumberString, actual1);
                // The pattern separator is something that differs by numbering system in my@numbers=mymr.
                char actual2 = dfs.PatternSeparator;
                assertEquals("Pattern separator with " + loc + " and " + ns.Name,
                        expectedPatternSeparator, actual2);

                // Coverage for JDK Locale overload
                DecimalFormatSymbols dfs2 = DecimalFormatSymbols.ForNumberingSystem(loc.ToCultureInfo(), ns);
                assertEquals("JDK Locale and ICU Locale should produce the same object", dfs, dfs2);
            }
        }
    }
}
