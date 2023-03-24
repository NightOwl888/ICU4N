using ICU4N.Dev.Test;
using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;

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
                IcuNumber.TryFormatInt64(number, locale.NumberFormat.NumberPattern,
                    locale.NumberFormat, destination, out int charsWritten));

            string actual = destination.Slice(0, charsWritten).ToString();

            assertEquals($"bad format for {locale}", expected, actual);
        }

#endif
    }
}
