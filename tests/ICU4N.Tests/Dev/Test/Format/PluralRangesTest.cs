using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Format
{
    /// <author>markdavis</author>
    public class PluralRangesTest : TestFmwk
    {
        [Test]
        public void TestLocaleData()
        {
            string[][] tests = new string[][] {
                new string[] {"de", "other", "one", "one"},
                new string[] {"xxx", "few", "few", "few" },
                new string[] {"de", "one", "other", "other"},
                new string[] {"de", "other", "one", "one"},
                new string[] {"de", "other", "other", "other"},
                new string[] {"ro", "one", "few", "few"},
                new string[] {"ro", "one", "other", "other"},
                new string[] {"ro", "few", "one", "few"},
            };
            foreach (String[] test in tests)
            {
                UCultureInfo locale = new UCultureInfo(test[0]);
                StandardPluralUtil.TryGetValue(test[1], out StandardPlural start);
                StandardPluralUtil.TryGetValue(test[2], out StandardPlural end);
                StandardPluralUtil.TryGetValue(test[3], out StandardPlural expected);
                PluralRanges pluralRanges = PluralRulesFactory.DefaultFactory.GetPluralRanges(locale);

                //StandardPlural start = StandardPlural.FromString(test[1]);
                //StandardPlural end = StandardPlural.fromString(test[2]);
                //StandardPlural expected = StandardPlural.fromString(test[3]);
                //Factory.getDefaultFactory().getPluralRanges(locale);

                StandardPlural actual = pluralRanges.Get(start, end);
                assertEquals("Deriving range category", expected, actual);
            }
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependencies MeasureFormat and FormatWidth")]
        public void TestRangePattern()
        {
            //    string[][] tests = new string[][] {
            //        new string[] {"de", "SHORT", "{0}–{1}"},
            //        new string[] {"ja", "NARROW", "{0}～{1}"},
            //    };
            //    foreach (String[] test in tests)
            //    {
            //        UCultureInfo ulocale = new UCultureInfo(test[0]);
            //        FormatWidth width = FormatWidth.valueOf(test[1]);
            //        String expected = test[2];
            //        String formatter = MeasureFormat.getRangeFormat(ulocale, width);
            //        String actual = SimpleFormatterImpl.FormatCompiledPattern(formatter, "{0}", "{1}");
            //        assertEquals(string.Format(StringFormatter.CurrentCulture, "range pattern {0}", test), expected, actual);
            //    }
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependencies MeasureFormat, MeasureUnit, Currency and FormatWidth")]
        public void TestFormatting()
        {
            //    object[][] tests = new object[][] {
            //        new object[]{0.0, 1.0, ULocale.FRANCE, FormatWidth.WIDE, MeasureUnit.FAHRENHEIT, "0–1 degré Fahrenheit"},
            //        new object[]{1.0, 2.0, ULocale.FRANCE, FormatWidth.WIDE, MeasureUnit.FAHRENHEIT, "1–2 degrés Fahrenheit"},
            //        new object[]{3.1, 4.25, ULocale.FRANCE, FormatWidth.SHORT, MeasureUnit.FAHRENHEIT, "3,1–4,25 °F"},
            //        new object[]{3.1, 4.25, ULocale.ENGLISH, FormatWidth.SHORT, MeasureUnit.FAHRENHEIT, "3.1–4.25°F"},
            //        new object[]{3.1, 4.25, ULocale.CHINESE, FormatWidth.WIDE, MeasureUnit.INCH, "3.1-4.25英寸"},
            //        new object[]{0.0, 1.0, ULocale.ENGLISH, FormatWidth.WIDE, MeasureUnit.INCH, "0–1 inches"},

            //        new object[]{0.0, 1.0, ULocale.ENGLISH, FormatWidth.NARROW, Currency.getInstance("EUR"), "€0.00–1.00"},
            //        new object[]{0.0, 1.0, ULocale.FRENCH, FormatWidth.NARROW, Currency.getInstance("EUR"), "0,00–1,00 €"},
            //        new object[]{0.0, 100.0, ULocale.FRENCH, FormatWidth.NARROW, Currency.getInstance("JPY"), "0–100\u00a0JPY"},

            //        new object[]{0.0, 1.0, ULocale.ENGLISH, FormatWidth.SHORT, Currency.getInstance("EUR"), "EUR0.00–1.00"},
            //        new object[]{0.0, 1.0, ULocale.FRENCH, FormatWidth.SHORT, Currency.getInstance("EUR"), "0,00–1,00\u00a0EUR"},
            //        new object[]{0.0, 100.0, ULocale.FRENCH, FormatWidth.SHORT, Currency.getInstance("JPY"), "0–100\u00a0JPY"},

            //        new object[]{0.0, 1.0, ULocale.ENGLISH, FormatWidth.WIDE, Currency.getInstance("EUR"), "0.00–1.00 euros"},
            //        new object[]{0.0, 1.0, ULocale.FRENCH, FormatWidth.WIDE, Currency.getInstance("EUR"), "0,00–1,00 euro"},
            //        new object[]{0.0, 2.0, ULocale.FRENCH, FormatWidth.WIDE, Currency.getInstance("EUR"), "0,00–2,00 euros"},
            //        new object[]{0.0, 100.0, ULocale.FRENCH, FormatWidth.WIDE, Currency.getInstance("JPY"), "0–100 yens japonais"},
            //    };
            //    int i = 0;
            //    foreach (Object[] test in tests)
            //    {
            //        ++i;
            //        double low = (double)test[0];
            //        double high = (double)test[1];
            //        ULocale locale = (ULocale)test[2];
            //        FormatWidth width = (FormatWidth)test[3];
            //        MeasureUnit unit = (MeasureUnit)test[4];
            //        Object expected = test[5];

            //        MeasureFormat mf = MeasureFormat.getInstance(locale, width);
            //        Object actual;
            //        try
            //        {
            //            actual = mf.formatMeasureRange(new Measure(low, unit), new Measure(high, unit));
            //        }
            //        catch (Exception e)
            //        {
            //            actual = e.GetType();
            //        }
            //        assertEquals(i + " Formatting unit", expected, actual);
            //    }
        }

        [Test]
        public void TestBasic()
        {
            PluralRanges a = new PluralRanges();
            a.Add(StandardPlural.One, StandardPlural.Other, StandardPlural.One);
            StandardPlural actual = a.Get(StandardPlural.One, StandardPlural.Other);
            assertEquals("range", StandardPlural.One, actual);
            a.Freeze();
            try
            {
                a.Add(StandardPlural.One, StandardPlural.One, StandardPlural.One);
                Errln("Failed to cause exception on frozen instance");
            }
            catch (NotSupportedException e)
            {
            }
        }
    }
}
