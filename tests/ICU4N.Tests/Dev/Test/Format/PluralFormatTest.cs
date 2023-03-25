using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using Integer = J2N.Numerics.Int32;

namespace ICU4N.Dev.Test.Format
{
    /// <author>tschumann (Tim Schumann)</author>
    public class PluralFormatTest : TestFmwk
    {
        private void helperTestRules(String localeIDs, String testPattern, IDictionary<Integer, String> changes)
        {
            String[] locales = Utility.Split(localeIDs, ',');

            // Create example outputs for all supported locales.
            /*
            System.out.println("\n" + localeIDs);
            String lastValue = (String) changes.get(Integer.GetInstance(0));
            int  lastNumber = 0;

            for (int i = 1; i < 199; ++i) {
                if (changes.get(Integer.GetInstance(i)) != null) {
                    if (lastNumber == i-1) {
                        System.out.println(lastNumber + ": " + lastValue);
                    } else {
                        System.out.println(lastNumber + "... " + (i-1) + ": " + lastValue);
                    }
                    lastNumber = i;
                    lastValue = (String) changes.get(Integer.GetInstance(i));
                }
            }
            System.out.println(lastNumber + "..." + 199 + ": " + lastValue);
            */
            Log("test pattern: '" + testPattern + "'");
            for (int i = 0; i < locales.Length; ++i)
            {
                try
                {
                    PluralFormat plf = new PluralFormat(new UCultureInfo(locales[i]), testPattern);
                    Log("plf: " + plf);
                    String expected = changes[Integer.GetInstance(0)];
                    for (int n = 0; n < 200; ++n)
                    {
                        if (changes.TryGetValue(n, out string value) && value != null)
                        {
                            expected = value;
                        }
                        assertEquals("Locale: " + locales[i] + ", number: " + n,
                                     expected, plf.Format(n));
#if FEATURE_SPAN
                        // Tests the new IcuNumber class
                        TestIcuNumber_PluralFormat(locales[i], n, testPattern, null, expected, "Locale: " + locales[i] + ", number: " + n);
#endif
                    }
                }
                catch (ArgumentException e)
                {
                    Errln(e.ToString() + " locale: " + locales[i] + " pattern: '" + testPattern + "' " + J2N.Time.CurrentTimeMilliseconds());
                }
            }
        }

#if FEATURE_SPAN
        public void TestIcuNumber_PluralFormat(string culture, int number, string pluralPattern, string decimalPattern, string expected, string assertMessage)
        {
            var locale = new UCultureInfo(culture);
            MessagePattern messagePattern = new MessagePattern();
            messagePattern.ParsePluralStyle(pluralPattern);
            PluralRules pluralRules = PluralRules.GetInstance(locale);
            string actual = IcuNumber.FormatPlural(number, decimalPattern, messagePattern, pluralRules, locale.NumberFormat);

            assertEquals(assertMessage, expected, actual);
        }
#endif

        [Test]
        public void TestOneFormLocales()
        {
            String localeIDs = "ja,ko,tr,vi";
            String testPattern = @"other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestSingular1Locales()
        {
            String localeIDs = "bem,da,de,el,en,eo,es,et,fi,fo,he,it,nb,nl,nn,no,pt_PT,sv,af,bg,ca,eu,fur,fy,ha,ku,lb,ml," +
                "nah,ne,om,or,pap,ps,so,sq,sw,ta,te,tk,ur,mn,gsw,rm";
            String testPattern = @"one{one} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestSingular01Locales()
        {
            String localeIDs = "ff,fr,kab,gu,mr,pa,pt,zu,bn";
            String testPattern = @"one{one} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "one";
            changes[Integer.GetInstance(2)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestZeroSingularLocales()
        {
            String localeIDs = "lv";
            String testPattern = @"zero{zero} one{one} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "zero";
            changes[Integer.GetInstance(1)] = "one";
            for (int i = 2; i < 20; ++i)
            {
                if (i < 10)
                {
                    changes[Integer.GetInstance(i)] = "other";
                }
                else
                {
                    changes[Integer.GetInstance(i)] = "zero";
                }
                changes[Integer.GetInstance(i * 10)] = "zero";
                if (i == 11)
                {
                    changes[Integer.GetInstance(i * 10 + 1)] = "zero";
                    changes[Integer.GetInstance(i * 10 + 2)] = "zero";
                }
                else
                {
                    changes[Integer.GetInstance(i * 10 + 1)] = "one";
                    changes[Integer.GetInstance(i * 10 + 2)] = "other";
                }
            }
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestSingularDual()
        {
            String localeIDs = "ga";
            String testPattern = @"one{one} two{two} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "two";
            changes[Integer.GetInstance(3)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestSingularZeroSome()
        {
            String localeIDs = "ro";
            String testPattern = @"few{few} one{one} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "few";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "few";
            changes[Integer.GetInstance(20)] = "other";
            changes[Integer.GetInstance(101)] = "few";
            changes[Integer.GetInstance(120)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestSpecial12_19()
        {
            String localeIDs = "lt";
            String testPattern = @"one{one} few{few} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "few";
            changes[Integer.GetInstance(10)] = "other";
            for (int i = 2; i < 20; ++i)
            {
                if (i == 11)
                {
                    continue;
                }
                changes[Integer.GetInstance(i * 10 + 1)] = "one";
                changes[Integer.GetInstance(i * 10 + 2)] = "few";
                changes[Integer.GetInstance((i + 1) * 10)] = "other";
            }
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestPaucalExcept11_14()
        {
            String localeIDs = "hr,sr,uk";
            String testPattern = @"one{one} few{few} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "few";
            changes[Integer.GetInstance(5)] = "other";
            for (int i = 2; i < 20; ++i)
            {
                if (i == 11)
                {
                    continue;
                }
                changes[Integer.GetInstance(i * 10 + 1)] = "one";
                changes[Integer.GetInstance(i * 10 + 2)] = "few";
                changes[Integer.GetInstance(i * 10 + 5)] = "other";
            }
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestPaucalRu()
        {
            String localeIDs = "ru";
            String testPattern = @"one{one} many{many} other{other}";
            var changes = new Dictionary<Integer, string>();
            for (int i = 0; i < 200; i += 10)
            {
                if (i == 10 || i == 110)
                {
                    put(i, 0, 9, "many", changes);
                    continue;
                }
                put(i, 0, "many", changes);
                put(i, 1, "one", changes);
                put(i, 2, 4, "other", changes);
                put(i, 5, 9, "many", changes);
            }
            helperTestRules(localeIDs, testPattern, changes);
        }

        public void put<T>(int @base, int start, int end, T value, IDictionary<Integer, T> m)
        {
            for (int i = start; i <= end; ++i)
            {
                if (m.ContainsKey(@base + i))
                {
                    throw new ArgumentException();
                }
                m[@base + i] = value;
            }
        }

        public void put<T>(int @base, int start, T value, IDictionary<Integer, T> m)
        {
            put(@base, start, start, value, m);
        }

        [Test]
        public void TestSingularPaucal()
        {
            String localeIDs = "cs,sk";
            String testPattern = @"one{one} few{few} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "few";
            changes[Integer.GetInstance(5)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestPaucal1_234()
        {
            String localeIDs = "pl";
            String testPattern = @"one{one} few{few} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "few";
            changes[Integer.GetInstance(5)] = "other";
            for (int i = 2; i < 20; ++i)
            {
                if (i == 11)
                {
                    continue;
                }
                changes[Integer.GetInstance(i * 10 + 2)] = "few";
                changes[Integer.GetInstance(i * 10 + 5)] = "other";
            }
            helperTestRules(localeIDs, testPattern, changes);
        }

        [Test]
        public void TestPaucal1_2_34()
        {
            String localeIDs = "sl";
            String testPattern = @"one{one} two{two} few{few} other{other}";
            var changes = new Dictionary<Integer, string>();
            changes[Integer.GetInstance(0)] = "other";
            changes[Integer.GetInstance(1)] = "one";
            changes[Integer.GetInstance(2)] = "two";
            changes[Integer.GetInstance(3)] = "few";
            changes[Integer.GetInstance(5)] = "other";
            changes[Integer.GetInstance(101)] = "one";
            changes[Integer.GetInstance(102)] = "two";
            changes[Integer.GetInstance(103)] = "few";
            changes[Integer.GetInstance(105)] = "other";
            helperTestRules(localeIDs, testPattern, changes);
        }

        /* Tests the method public PluralRules getPluralRules() */
        [Test]
        public void TestGetPluralRules()
        {
            CurrencyPluralInfo cpi = new CurrencyPluralInfo();
            try
            {
                var _ = cpi.PluralRules;
            }
            catch (Exception e)
            {
                Errln("CurrencyPluralInfo.getPluralRules() was not suppose to " + "return an exception.");
            }
        }

        /* Tests the method public ULocale getLocale() */
        [Test]
        public void TestGetLocale()
        {
            CurrencyPluralInfo cpi = new CurrencyPluralInfo(new UCultureInfo("en_US"));
            if (!cpi.Culture.Equals(new UCultureInfo("en_US")))
            {
                Errln("CurrencyPluralInfo.getLocale() was suppose to return true " + "when passing the same ULocale");
            }
            if (cpi.Culture.Equals(new UCultureInfo("jp_JP")))
            {
                Errln("CurrencyPluralInfo.getLocale() was not suppose to return true " + "when passing a different ULocale");
            }
        }

        /* Tests the method public void setLocale(ULocale loc) */
        [Test]
        public void TestSetLocale()
        {
            CurrencyPluralInfo cpi = new CurrencyPluralInfo();
            cpi.Culture = new UCultureInfo("en_US");
            if (!cpi.Culture.Equals(new UCultureInfo("en_US")))
            {
                Errln("CurrencyPluralInfo.setLocale() was suppose to return true when passing the same ULocale");
            }
            if (cpi.Culture.Equals(new UCultureInfo("jp_JP")))
            {
                Errln("CurrencyPluralInfo.setLocale() was not suppose to return true when passing a different ULocale");
            }
        }

        /* Tests the method public boolean equals(Object a) */
        [Test]
        public void TestEquals()
        {
            CurrencyPluralInfo cpi = new CurrencyPluralInfo();
            if (cpi.Equals(0))
            {
                Errln("CurrencyPluralInfo.equals(Object) was not suppose to return true when comparing to an invalid object for integer 0.");
            }
            if (cpi.Equals(0.0))
            {
                Errln("CurrencyPluralInfo.equals(Object) was not suppose to return true when comparing to an invalid object for float 0.");
            }
            if (cpi.Equals("0"))
            {
                Errln("CurrencyPluralInfo.equals(Object) was not suppose to return true when comparing to an invalid object for string 0.");
            }
        }

        /* Test for http://bugs.icu-project.org/trac/ticket/13151 */
        [Test]
        public void TestFractionRounding()
        {
            NumberFormat nf = NumberFormat.GetInstance(new CultureInfo("en"));
            nf.MaximumFractionDigits = (0);
            PluralFormat pf = new PluralFormat(new UCultureInfo("en"), @"one{#kg}other{#kgs}");
            pf.SetNumberFormat(nf);
            assertEquals("1.2kg", "1kg", pf.Format(1.2));
        }
    }
}
