using ICU4N.Dev.Test;
using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ICU4N
{
    public class IcuNumberFormattingTest : TestFmwk
    {
        private const int CharStackBufferSize = 64;

#if FEATURE_SPAN
        [Test]
        public void TestTryFormatInt64_AgainstRbnfDecimalFormat_SpellOut()
        {
            long number = 123456789012345678;
            Span<char> destination = stackalloc char[CharStackBufferSize];

            foreach (UCultureInfo locale in NumberFormat.GetUCultures(UCultureTypes.AllCultures))
            {
                AssertTryFormatFor(number, locale, NumberPresentation.SpellOut, destination);
            }
        }

        [Test]
        public void TestTryFormatInt64_AgainstRbnfDecimalFormat_SpellOut_ar_EH()
        {
            long number = 123456789012345678;
            Span<char> destination = stackalloc char[CharStackBufferSize];

            AssertTryFormatFor(number, new UCultureInfo("ar_EH"), NumberPresentation.SpellOut, destination);
        }

        private void AssertTryFormatFor(long number, UCultureInfo locale, NumberPresentation presentation, Span<char> destination)
        {
            // NOTE: This method falls back wrong when in DEBUG mode because of the hack we added to
            // make the debugger work when satellite assemblies are loaded for unrecognized cultures.
            // It works in RELEASE mode. This is not a bug on our end.
            //var decimalFormat = NumberFormat.GetInstance(locale, NumberFormatStyle.NumberStyle);


            var culture = locale.ToCultureInfo();
            var rbnf = new RuleBasedNumberFormat(locale, presentation);
            var decimalFormat = rbnf.DecimalFormat;

            string expected = decimalFormat.Format(number);

            assertTrue($"TryFormatInt64 returned false for {locale}",
                IcuNumber.TryFormatInt64(number, locale.NumberFormat.NumberPattern.AsSpan(),
                    locale.NumberFormat, destination, out int charsWritten));

            string actual = destination.Slice(0, charsWritten).ToString();

            assertEquals($"bad format for {locale}", expected, actual);
        }



        public static IEnumerable<TestCaseData> Test_PluralFormatUnitTest_Data
        {
            get
            {
                string culture;
                string decimalPattern;
                string pluralPattern;
                PluralType pluralType;

                //// TestApplyPatternAndFormat()
                //culture = UCultureInfo.CurrentCulture.FullName;
                //pluralType = PluralType.Cardinal;
                //decimalPattern = null;

                //UCultureInfo ci = new UCultureInfo(culture);
                //for (int i = 0; i < 22; ++i)
                //{
                //    pluralPattern = "odd: n mod 2 is 1";
                //    yield return new TestCaseData(culture, i, pluralPattern, pluralType, decimalPattern, $"{i.ToString(ci.cultureData.decimalFormat, ci.ToCultureInfo())}", "Fully specified PluralFormat gave wrong results");

                //    pluralPattern = "odd{# is odd.} other{# is even.}";
                //    yield return new TestCaseData(culture, i, pluralPattern, pluralType, decimalPattern, $"{i.ToString(ci.cultureData.decimalFormat, ci.ToCultureInfo())}", "Fully specified PluralFormat gave wrong results");

                //    pluralPattern = "other{# is odd or even.}";
                //    yield return new TestCaseData(culture, i, pluralPattern, pluralType, decimalPattern, $"{i.ToString(ci.cultureData.decimalFormat, ci.ToCultureInfo())}", "Fully specified PluralFormat gave wrong results");
                //}


                // TestExtendedPluralFormat()
                string[] expecteds = {
                    "There are no widgets.",
                    "There is one widget.",
                    "There is a bling widget and one other widget.",
                    "There is a bling widget and 2 other widgets.",
                    "There is a bling widget and 3 other widgets.",
                    "Widgets, five (5-1=4) there be.",
                    "There is a bling widget and 5 other widgets.",
                    "There is a bling widget and 6 other widgets.",
                };

                culture = "en";
                pluralPattern = "offset:1.0 "
                            + "=0 {There are no widgets.} "
                            + "=1.0 {There is one widget.} "
                            + "=5 {Widgets, five (5-1=#) there be.} "
                            + "one {There is a bling widget and one other widget.} "
                            + "other {There is a bling widget and # other widgets.}";
                pluralType = PluralType.Cardinal;
                decimalPattern = null;

                for (int i = 0; i <= 7; ++i)
                {
                    yield return new TestCaseData(culture, i, pluralPattern, pluralType, decimalPattern, expecteds[i], "PluralFormat.format(value " + i + ")");
                }


                // TestOrdinalFormat()
                culture = "en";
                pluralPattern = "one{#st file}two{#nd file}few{#rd file}other{#th file}";
                pluralType = PluralType.Ordinal;
                decimalPattern = null;
                yield return new TestCaseData(culture, 321, pluralPattern, pluralType, decimalPattern, "321st file", "PluralFormat.format(321)");
                yield return new TestCaseData(culture, 22, pluralPattern, pluralType, decimalPattern, "22nd file", "PluralFormat.format(22)");
                yield return new TestCaseData(culture, 3, pluralPattern, pluralType, decimalPattern, "3rd file", "PluralFormat.format(3)");

                yield return new TestCaseData(culture, 456, pluralPattern, pluralType, decimalPattern, "456th file", "PluralFormat.format(456)");
                yield return new TestCaseData(culture, 111, pluralPattern, pluralType, decimalPattern, "111th file", "PluralFormat.format(111)");

                // TestDecimals()
                culture = "en";
                pluralPattern = "one{one meter}other{# meters}";
                pluralType = PluralType.Cardinal;
                decimalPattern = null;
                yield return new TestCaseData(culture, 1, pluralPattern, pluralType, decimalPattern, "one meter", "simple format(1)");
                yield return new TestCaseData(culture, 1.5, pluralPattern, pluralType, decimalPattern, "1.5 meters", "simple format(1.5)");

                culture = "en";
                pluralPattern = "offset:1 one{another meter}other{another # meters}";
                pluralType = PluralType.Cardinal;
                decimalPattern = "0.0";
                yield return new TestCaseData(culture, 1, pluralPattern, pluralType, decimalPattern, "another 0.0 meters", "offset-decimals format(1)");
                yield return new TestCaseData(culture, 2, pluralPattern, pluralType, decimalPattern, "another 1.0 meters", "offset-decimals format(2)");
                yield return new TestCaseData(culture, 2.5, pluralPattern, pluralType, decimalPattern, "another 1.5 meters", "offset-decimals format(2.5)");

                // TestNegative()
                culture = "en";
                pluralPattern = "one{# foot}other{# feet}";
                pluralType = PluralType.Cardinal;
                decimalPattern = null;
                yield return new TestCaseData(culture, -3, pluralPattern, pluralType, decimalPattern, "-3 feet", "locale=en, pattern=one{# foot}other{# feet}");
            }
        }

        [TestCaseSource("Test_PluralFormatUnitTest_Data")]
        public void Test_PluralFormatUnitTest(string culture, double number, string pluralPattern, PluralType pluralType, string decimalPattern, string expected, string assertMessage)
        {
            var locale = new UCultureInfo(culture);
            MessagePattern messagePattern = new MessagePattern();
            messagePattern.ParsePluralStyle(pluralPattern);
            string actual = IcuNumber.FormatPlural(number, decimalPattern, messagePattern, pluralType, locale.NumberFormat);

            assertEquals(assertMessage, expected, actual);
        }

#endif
    }
}
