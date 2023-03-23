using ICU4N.Dev.Test;
using ICU4N.Text;
using NUnit.Framework;
using System;

namespace ICU4N.Globalization
{
    public class UNumberFormatInfoTest : TestFmwk
    {
        [Test]
        public void TestAllCultures()
        {
            foreach (var culture in UCultureInfo.GetCultures(UCultureTypes.AllCultures))
            {
                DecimalFormatSymbols expected = new DecimalFormatSymbols(culture);
                IDecimalFormatSymbols actual = culture.NumberFormat;

                try
                {
                    assertDecimalFormatSymbolsEqual(expected, actual);
                }
                catch (Exception ex)
                {
                    fail(culture.ToString() + Environment.NewLine + ex.ToString());
                }
            }
        }

        [Test]
        public void Test_en_FI()
        {
            var culture = new UCultureInfo("en_FI");
            DecimalFormatSymbols expected = new DecimalFormatSymbols(culture);
            IDecimalFormatSymbols actual = culture.NumberFormat;

            assertDecimalFormatSymbolsEqual(expected, actual);
        }

        [Test]
        public void TestArabic()
        {
            var culture = new UCultureInfo("ar");
            DecimalFormatSymbols expected = new DecimalFormatSymbols(culture);
            IDecimalFormatSymbols actual = culture.NumberFormat;

            assertDecimalFormatSymbolsEqual(expected, actual);
        }

        [Test]
        // ICU4N specific - check 
        public void TestInvariantCulture()
        {
            DecimalFormatSymbols expected = new DecimalFormatSymbols(UCultureInfo.InvariantCulture);
            IDecimalFormatSymbols actual = new UNumberFormatInfo();

            assertDecimalFormatSymbolsEqual(expected, actual);
        }

        private void assertDecimalFormatSymbolsEqual(DecimalFormatSymbols expected, IDecimalFormatSymbols actual)
        {
            assertEquals("invalid CodePointZero", expected.CodePointZero, actual.CodePointZero);
            //assertEquals("invalid CurrencyPattern", expected.CurrencyPattern, actual.CurrencyPattern); // ICU4N TODO: Need to account for this somehow - it is only used in NumberFormat.GetInstance() but it uses culture data. Invariant default is null.
            //assertEquals("invalid CurrencySymbol", expected.CurrencySymbol, actual.CurrencySymbol); // ICU4N TODO: Need to lookup currency settings
            assertEquals("invalid DecimalSeparator", expected.DecimalSeparator, actual.DecimalSeparator);
            assertEquals("invalid DecimalSeparatorString", expected.DecimalSeparatorString, actual.DecimalSeparatorString);
            assertEquals("invalid Digit", expected.Digit, actual.Digit);
            assertEquals("invalid Digits", expected.Digits, actual.Digits);
            assertEquals("invalid DigitStrings", expected.DigitStrings, actual.DigitStrings);
            assertEquals("invalid DigitStringsLocal", expected.DigitStringsLocal, actual.DigitStringsLocal);
            assertEquals("invalid ExponentMultiplicationSign", expected.ExponentMultiplicationSign, actual.ExponentMultiplicationSign);
            assertEquals("invalid ExponentSeparator", expected.ExponentSeparator, actual.ExponentSeparator);

            //assertCurrencySpacingPatternsEqual(expected, actual); // ICU4N TODO: Need to lookup currency settings

            assertEquals("invalid GroupingSeparator", expected.GroupingSeparator, actual.GroupingSeparator);
            assertEquals("invalid GroupingSeparatorString", expected.GroupingSeparatorString, actual.GroupingSeparatorString);
            assertEquals("invalid Infinity", expected.Infinity, actual.Infinity);
            //assertEquals("invalid InternationalCurrencySymbol", expected.InternationalCurrencySymbol, actual.InternationalCurrencySymbol); // ICU4N TODO: Need to lookup currency settings
            assertEquals("invalid MinusSign", expected.MinusSign, actual.MinusSign);
            assertEquals("invalid MinusSignString", expected.MinusSignString, actual.MinusSignString);
            //assertEquals("invalid MonetaryDecimalSeparator", expected.MonetaryDecimalSeparator, actual.MonetaryDecimalSeparator); // ICU4N TODO: Need to lookup currency settings
            //assertEquals("invalid MonetaryDecimalSeparatorString", expected.MonetaryDecimalSeparatorString, actual.MonetaryDecimalSeparatorString); // ICU4N TODO: Need to lookup currency settings
            //assertEquals("invalid MonetaryGroupingSeparator", expected.MonetaryGroupingSeparator, actual.MonetaryGroupingSeparator); // ICU4N TODO: Need to lookup currency settings
            //assertEquals("invalid MonetaryGroupingSeparatorString", expected.MonetaryGroupingSeparatorString, actual.MonetaryGroupingSeparatorString); // ICU4N TODO: Need to lookup currency settings
            assertEquals("invalid NaN", expected.NaN, actual.NaN);
            assertEquals("invalid PadEscape", expected.PadEscape, actual.PadEscape);
            assertEquals("invalid PatternSeparator", expected.PatternSeparator, actual.PatternSeparator);
            assertEquals("invalid Percent", expected.Percent, actual.Percent);
            assertEquals("invalid PercentString", expected.PercentString, actual.PercentString);
            assertEquals("invalid PerMill", expected.PerMill, actual.PerMill);
            assertEquals("invalid PerMillString", expected.PerMillString, actual.PerMillString);
            assertEquals("invalid PlusSign", expected.PlusSign, actual.PlusSign);
            assertEquals("invalid PlusSignString", expected.PlusSignString, actual.PlusSignString);
            assertEquals("invalid SignificantDigit", expected.SignificantDigit, actual.SignificantDigit);
            assertEquals("invalid ZeroDigit", expected.ZeroDigit, actual.ZeroDigit);
        }

        private void assertCurrencySpacingPatternsEqual(DecimalFormatSymbols expected, IDecimalFormatSymbols actual)
        {
            assertEquals("invalid CurrencyMatch prefix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.CurrencyMatch, beforeCurrency: true),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.CurrencyMatch, beforeCurrency: true));
            assertEquals("invalid CurrencyMatch suffix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.CurrencyMatch, beforeCurrency: false),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.CurrencyMatch, beforeCurrency: false));

            assertEquals("invalid SurroundingMatch prefix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.SurroundingMatch, beforeCurrency: true),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.SurroundingMatch, beforeCurrency: true));
            assertEquals("invalid SurroundingMatch suffix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.SurroundingMatch, beforeCurrency: false),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.SurroundingMatch, beforeCurrency: false));

            assertEquals("invalid InsertBetween prefix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.InsertBetween, beforeCurrency: true),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.InsertBetween, beforeCurrency: true));
            assertEquals("invalid InsertBetween suffix",
                expected.GetPatternForCurrencySpacing(CurrencySpacingPattern.InsertBetween, beforeCurrency: false),
                actual.GetPatternForCurrencySpacing(CurrencySpacingPattern.InsertBetween, beforeCurrency: false));
        }
    }
}
