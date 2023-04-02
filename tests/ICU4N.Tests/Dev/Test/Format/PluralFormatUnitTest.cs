using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Collections.Generic;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using Integer = J2N.Numerics.Int32;
using Long = J2N.Numerics.Int64;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Format
{
    /// <author>tschumann (Tim Schumann)</author>
    public class PluralFormatUnitTest : TestFmwk
    {
        [Test]
        public void TestConstructor()
        {
            // Test correct formatting of numbers.
            PluralFormat[] plFmts = new PluralFormat[10];
            plFmts[0] = new PluralFormat();
            plFmts[0].ApplyPattern("other{#}");
            plFmts[1] = new PluralFormat(PluralRules.Default);
            plFmts[1].ApplyPattern("other{#}");
            plFmts[2] = new PluralFormat(PluralRules.Default, "other{#}");
            plFmts[3] = new PluralFormat("other{#}");
            plFmts[4] = new PluralFormat(UCultureInfo.CurrentCulture);
            plFmts[4].ApplyPattern("other{#}");
            plFmts[5] = new PluralFormat(UCultureInfo.CurrentCulture, PluralRules.Default);
            plFmts[5].ApplyPattern("other{#}");
            plFmts[6] = new PluralFormat(UCultureInfo.CurrentCulture,
                    PluralRules.Default,
                    "other{#}");
            plFmts[7] = new PluralFormat(UCultureInfo.CurrentCulture, "other{#}");

            // Constructors with Java Locale
            plFmts[8] = new PluralFormat(CultureInfo.CurrentCulture);
            plFmts[8].ApplyPattern("other{#}");
            plFmts[9] = new PluralFormat(CultureInfo.CurrentCulture, PluralRules.Default);
            plFmts[9].ApplyPattern("other{#}");

            // These plural formats should produce the same output as a
            // NumberFormat for the default locale.
            NumberFormat numberFmt = NumberFormat.GetInstance(UCultureInfo.CurrentCulture);
            for (int n = 1; n < 13; n++)
            {
                String result = numberFmt.Format(n);
                for (int k = 0; k < plFmts.Length; ++k)
                {
                    TestFmwk.assertEquals("PluralFormat's output is not as expected",
                            result, plFmts[k].Format(n));
                }
            }
            // Test some bigger numbers.
            // Coverage: Use the format(Object, ...) version.
            StringBuffer sb = new StringBuffer();
            FieldPosition ignore = new FieldPosition(-1);
            for (int n = 100; n < 113; n++)
            {
                String result = numberFmt.Format(n * n);
                for (int k = 0; k < plFmts.Length; ++k)
                {
                    sb.Delete(0, sb.Length); // ICU4N: Checked 2nd parameter
                    String pfResult = plFmts[k].Format(Long.GetInstance(n * n), sb, ignore).ToString();
                    TestFmwk.assertEquals("PluralFormat's output is not as expected", result, pfResult);
                }
            }
        }

        [Test]
        public void TestEquals()
        {
            // There is neither clone() nor a copy constructor.
            PluralFormat de_fee_1 = new PluralFormat(new UCultureInfo("de"), PluralType.Cardinal, "other{fee}");
            PluralFormat de_fee_2 = new PluralFormat(new UCultureInfo("de"), PluralType.Cardinal, "other{fee}");
            PluralFormat de_fi = new PluralFormat(new UCultureInfo("de"), PluralType.Cardinal, "other{fi}");
            PluralFormat fr_fee = new PluralFormat(new UCultureInfo("fr"), PluralType.Cardinal, "other{fee}");
            assertTrue("different de_fee objects", de_fee_1 != de_fee_2);
            assertTrue("equal de_fee objects", de_fee_1.Equals(de_fee_2));
            assertFalse("different pattern strings", de_fee_1.Equals(de_fi));
            assertFalse("different locales", de_fee_1.Equals(fr_fee));
        }

        [Test]
        public void TestApplyPatternAndFormat()
        {
            // Create rules for testing.
            PluralRules oddAndEven = PluralRules.CreateRules("odd: n mod 2 is 1");
            {
                // Test full specified case for testing RuleSet
                PluralFormat plfOddAndEven = new PluralFormat(oddAndEven);
                plfOddAndEven.ApplyPattern("odd{# is odd.} other{# is even.}");

                // Test fall back to other.
                PluralFormat plfOddOrEven = new PluralFormat(oddAndEven);
                plfOddOrEven.ApplyPattern("other{# is odd or even.}");

                NumberFormat numberFormat =
                        NumberFormat.GetInstance(UCultureInfo.CurrentCulture);
                for (int i = 0; i < 22; ++i)
                {
                    assertEquals("Fallback to other gave wrong results",
                            numberFormat.Format(i) + " is odd or even.",
                            plfOddOrEven.Format(i));
                    assertEquals("Fully specified PluralFormat gave wrong results",
                            numberFormat.Format(i) + ((i % 2 == 1) ? " is odd."
                                    : " is even."),
                                    plfOddAndEven.Format(i));
                }

                // ICU 4.8 does not check for duplicate keywords any more.
                PluralFormat pf = new PluralFormat(new UCultureInfo("en"), oddAndEven,
                        "odd{foo} odd{bar} other{foobar}");
                assertEquals("should use first occurrence of the 'odd' keyword", "foo", pf.Format(1));
                pf.ApplyPattern("odd{foo} other{bar} other{foobar}");
                assertEquals("should use first occurrence of the 'other' keyword", "bar", pf.Format(2));
                // This sees the first "other" before calling the PluralSelector which then selects "other".
                pf.ApplyPattern("other{foo} odd{bar} other{foobar}");
                assertEquals("should use first occurrence of the 'other' keyword", "foo", pf.Format(2));
            }
            // omit other keyword.
            try
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("odd{foo}");
                Errln("Not defining plural case other should result in an " +
                        "exception but did not.");
            }
            catch (ArgumentException e) { }

            // ICU 4.8 does not check for unknown keywords any more.
            {
                PluralFormat pf = new PluralFormat(new UCultureInfo("en"), oddAndEven, "otto{foo} other{bar}");
                assertEquals("should ignore unknown keywords", "bar", pf.Format(1));
            }

            // Test invalid keyword.
            try
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("*odd{foo} other{bar}");
                Errln("Defining a message for an invalid keyword should result in " +
                        "an exception but did not.");
            }
            catch (ArgumentException e) { }

            // Test invalid syntax
            //   -- comma between keyword{message} clauses
            //   -- space in keywords
            //   -- keyword{message1}{message2}
            try
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("odd{foo},other{bar}");
                Errln("Separating keyword{message} items with other characters " +
                        "than space should provoke an exception but did not.");
            }
            catch (ArgumentException e) { }
            try
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("od d{foo} other{bar}");
                Errln("Spaces inside keywords should provoke an exception but " +
                        "did not.");
            }
            catch (ArgumentException e) { }
            try
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("odd{foo}{foobar}other{foo}");
                Errln("Defining multiple messages after a keyword should provoke " +
                        "an exception but did not.");
            }
            catch (ArgumentException e) { }

            // Check that nested format is preserved.
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("odd{The number {0, number, #.#0} is odd.}" +
                        "other{The number {0, number, #.#0} is even.}");
                for (int i = 1; i < 3; ++i)
                {
                    assertEquals("format did not preserve a nested format string.",
                            ((i % 2 == 1) ?
                                    "The number {0, number, #.#0} is odd."
                                    : "The number {0, number, #.#0} is even."),
                                    plFmt.Format(i));
                }

            }
            // Check that a pound sign in curly braces is preserved.
            {
                PluralFormat plFmt = new PluralFormat(oddAndEven);
                plFmt.ApplyPattern("odd{The number {1,number,#} is odd.}" +
                        "other{The number {2,number,#} is even.}");
                for (int i = 1; i < 3; ++i)
                {
                    assertEquals("format did not preserve # inside curly braces.",
                            ((i % 2 == 1) ? "The number {1,number,#} is odd."
                                    : "The number {2,number,#} is even."),
                                    plFmt.Format(i));
                }

            }
        }


        [Test]
        public void TestSamples()
        {
            IDictionary<UCultureInfo, ISet<UCultureInfo>> same = new LinkedDictionary<UCultureInfo, ISet<UCultureInfo>>();
            foreach (UCultureInfo locale in PluralRules.GetUCultures())
            {
                UCultureInfo otherLocale = PluralRules.GetFunctionalEquivalent(locale);
                if (!same.TryGetValue(otherLocale, out ISet<UCultureInfo> others)) same[otherLocale] = others = new LinkedHashSet<UCultureInfo>();
                others.Add(locale);
                continue;
            }
            foreach (UCultureInfo locale0 in same.Keys)
            {
                PluralRules rules = PluralRules.GetInstance(locale0);
                String localeName = locale0.ToString().Length == 0 ? "root" : locale0.ToString();
                Logln(localeName + "\t=\t" + same[locale0]);
                Logln(localeName + "\ttoString\t" + rules.ToString());
                ICollection<string> keywords = rules.Keywords;
                foreach (string keyword in keywords)
                {
                    ICollection<double> list = rules.GetSamples(keyword);
                    if (list.Count == 0)
                    {
                        // if there aren't any integer samples, get the decimal ones.
                        list = rules.GetSamples(keyword, PluralRulesSampleType.Decimal);
                    }

                    if (list == null || list.Count == 0)
                    {
                        Errln("Empty list for " + localeName + " : " + keyword);
                    }
                    else
                    {
                        Logln("\t" + localeName + " : " + keyword + " ; " + list);
                    }
                }
            }
        }

        [Test]
        public void TestSetLocale()
        {
            // Create rules for testing.
            PluralRules oddAndEven = PluralRules.CreateRules("odd__: n mod 2 is 1");

            PluralFormat plFmt = new PluralFormat(oddAndEven);
            plFmt.ApplyPattern("odd__{odd} other{even}");
            plFmt.SetCulture(new UCultureInfo("en"));

            // Check that pattern gets deleted.
            NumberFormat nrFmt = NumberFormat.GetInstance(new UCultureInfo("en"));
            assertEquals("pattern was not resetted by setLocale() call.",
                    nrFmt.Format(5),
                    plFmt.Format(5));

            // Check that rules got updated.
            plFmt.ApplyPattern("odd__{odd} other{even}");
            assertEquals("SetLocale should reset rules but did not.", "even", plFmt.Format(1));

            plFmt.ApplyPattern("one{one} other{not one}");
            for (int i = 0; i < 20; ++i)
            {
                assertEquals("Wrong ruleset loaded by setLocale()",
                        ((i == 1) ? "one" : "not one"),
                        plFmt.Format(i));
            }
        }

        [Test]
        public void TestParse()
        {
            PluralFormat plFmt = new PluralFormat("other{test}");
            try
            {
                plFmt.Parse("test", new ParsePosition(0));
                Errln("parse() should throw an UnsupportedOperationException but " +
                        "did not");
            }
            catch (NotSupportedException e)
            {
            }

            plFmt = new PluralFormat("other{test}");
            try
            {
                plFmt.ParseObject("test", new ParsePosition(0));
                Errln("parse() should throw an UnsupportedOperationException but " +
                        "did not");
            }
            catch (NotSupportedException e)
            {
            }
        }

        [Test]
        public void TestPattern()
        {
            Object[] args = { "acme", null };

            {
                // ICU 4.8 PluralFormat does not trim() its pattern any more.
                // None of the other *Format classes do.
                String pat = "  one {one ''widget} other {# widgets}  ";
                PluralFormat pf = new PluralFormat(pat);
                assertEquals("should not trim() the pattern", pat, pf.ToPattern());
            }

            MessageFormat pfmt = new MessageFormat("The disk ''{0}'' contains {1, plural,  one {one ''''{1, number, #.0}'''' widget} other {# widgets}}.");
            Logln("");
            for (int i = 0; i < 3; ++i)
            {
                args[1] = Integer.GetInstance(i);
                Logln(pfmt.Format(args));
            }
            /* ICU 4.8 returns null instead of a choice/plural/select Format object
             * (because it does not create an object for any "complex" argument).
            PluralFormat pf = (PluralFormat)pfmt.getFormatsByArgumentIndex()[1];
            logln(pf.toPattern());
             */
            Logln(pfmt.ToPattern());
            MessageFormat pfmt2 = new MessageFormat(pfmt.ToPattern());
            assertEquals("message formats are equal", pfmt, pfmt2);
        }

        [Test]
        public void TestExtendedPluralFormat()
        {
            String[] targets = {
                "There are no widgets.",
                "There is one widget.",
                "There is a bling widget and one other widget.",
                "There is a bling widget and 2 other widgets.",
                "There is a bling widget and 3 other widgets.",
                "Widgets, five (5-1=4) there be.",
                "There is a bling widget and 5 other widgets.",
                "There is a bling widget and 6 other widgets.",
            };
            String pluralStyle =
                    "offset:1.0 "
                            + "=0 {There are no widgets.} "
                            + "=1.0 {There is one widget.} "
                            + "=5 {Widgets, five (5-1=#) there be.} "
                            + "one {There is a bling widget and one other widget.} "
                            + "other {There is a bling widget and # other widgets.}";
            PluralFormat pf = new PluralFormat(new UCultureInfo("en"), pluralStyle);
            MessageFormat mf = new MessageFormat("{0,plural," + pluralStyle + "}", new UCultureInfo("en"));
            Integer[] args = new Integer[1];
            for (int i = 0; i <= 7; ++i)
            {
                String result = pf.Format(i);
                assertEquals("PluralFormat.format(value " + i + ")", targets[i], result);
                args[0] = i;
                result = mf.Format(args);
                assertEquals("MessageFormat.format(value " + i + ")", targets[i], result);
            }

            // Try explicit values after keywords.
            pf.ApplyPattern("other{zz}other{yy}one{xx}one{ww}=1{vv}=1{uu}");
            assertEquals("should find first matching *explicit* value", "vv", pf.Format(1));
        }

        [Test]
        public void TestExtendedPluralFormatParsing()
        {
            String[] failures = {
                "offset:1..0 =0 {Foo}",
                "offset:1.0 {Foo}",
                "=0= {Foo}",
                "=0 {Foo} =0.0 {Bar}",
                " = {Foo}",
        };
            foreach (String fmt in failures)
            {
                try
                {
                    new PluralFormat(fmt);
                    fail("expected exception when parsing '" + fmt + "'");
                }
                catch (ArgumentException e)
                {
                    // ok
                }
            }
        }

        [Test]
        public void TestOrdinalFormat()
        {
            String pattern = "one{#st file}two{#nd file}few{#rd file}other{#th file}";
            PluralFormat pf = new PluralFormat(new UCultureInfo("en"), PluralType.Ordinal, pattern);
            assertEquals("PluralFormat.format(321)", "321st file", pf.Format(321));
            assertEquals("PluralFormat.format(22)", "22nd file", pf.Format(22));
            assertEquals("PluralFormat.format(3)", "3rd file", pf.Format(3));

            // Code coverage: Use the other new-for-PluralType constructor as well.
            pf = new PluralFormat(new UCultureInfo("en"), PluralType.Ordinal);
            pf.ApplyPattern(pattern);
            assertEquals("PluralFormat.format(456)", "456th file", pf.Format(456));
            assertEquals("PluralFormat.format(111)", "111th file", pf.Format(111));

            // Code coverage: Use Locale not ULocale.
            pf = new PluralFormat(new CultureInfo("en"), PluralType.Ordinal);
            pf.ApplyPattern(pattern);
            assertEquals("PluralFormat.format(456)", "456th file", pf.Format(456));
            assertEquals("PluralFormat.format(111)", "111th file", pf.Format(111));
        }

        [Test]
        public void TestDecimals()
        {
            // Simple number replacement.
            PluralFormat pf = new PluralFormat(new UCultureInfo("en"), "one{one meter}other{# meters}");
            assertEquals("simple format(1)", "one meter", pf.Format(1));
            assertEquals("simple format(1.5)", "1.5 meters", pf.Format(1.5));
            PluralFormat pf2 = new PluralFormat(new UCultureInfo("en"),
                    "offset:1 one{another meter}other{another # meters}");
            pf2.SetNumberFormat(new DecimalFormat("0.0", new DecimalFormatSymbols(new UCultureInfo("en"))));
            assertEquals("offset-decimals format(1)", "another 0.0 meters", pf2.Format(1));
            assertEquals("offset-decimals format(2)", "another 1.0 meters", pf2.Format(2));
            assertEquals("offset-decimals format(2.5)", "another 1.5 meters", pf2.Format(2.5));
        }

        [Test]
        public void TestNegative()
        {
            PluralFormat pluralFormat = new PluralFormat(new UCultureInfo("en"), "one{# foot}other{# feet}");
            String actual = pluralFormat.Format(-3);
            assertEquals(pluralFormat.ToString(), "-3 feet", actual);
        }
    }
}
