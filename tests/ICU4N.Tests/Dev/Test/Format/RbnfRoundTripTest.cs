using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Dev.Test.Format
{
    public class RbnfRoundTripTest : TestFmwk
    {
        /**
         * Perform an exhaustive round-trip test on the English spellout rules
         */
        [Test]
        public void TestEnglishSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("en-US"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the duration-formatting rules
         */
        [Test]
        public void TestDurationsRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("en-US"),
                            NumberPresentation.Duration);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Spanish spellout rules
         */
        [Test]
        public void TestSpanishSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("es-es"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the French spellout rules
         */
        [Test]
        public void TestFrenchSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("fr-FR"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Swiss French spellout rules
         */
        [Test]
        public void TestSwissFrenchSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("fr-CH"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Italian spellout rules
         */
        [Test]
        public void TestItalianSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("it-IT"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -999999, 999999);
        }

        /**
         * Perform an exhaustive round-trip test on the German spellout rules
         */
        [Test]
        public void TestGermanSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("de-DE"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Swedish spellout rules
         */
        [Test]
        public void TestSwedishSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("sv-SE"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Dutch spellout rules
         */
        [Test]
        public void TestDutchSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("nl-NL"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Japanese spellout rules
         */
        [Test]
        public void TestJapaneseSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("ja-JP"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Russian spellout rules
         */
        [Test]
        [Ignore("ICU4N TODO: This test is very slow (> 5 min). In Java, this takes 15 seconds.")]
        [Timeout(600000)]
        public void TestRussianSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("ru-RU"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Greek spellout rules
         */
        [Test]
        public void TestGreekSpelloutRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("el-GR"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Hebrew numbering system rules
         */
        [Test]
        public void TestHebrewNumberingRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("he-IL"),
                            NumberPresentation.NumberingSystem);

            formatter.SetDefaultRuleSet("%hebrew");
            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the English roman numeral numbering system rules
         */
        [Test]
        public void TestEnglishNumberingRT()
        {
            RbnfFormattterSettings formatter
                            = new RbnfFormattterSettings(new CultureInfo("en"),
                            NumberPresentation.NumberingSystem);

            formatter.SetDefaultRuleSet("%roman-upper");
            doTest(formatter, 0, 12345678);
        }

        void doTest(RuleBasedNumberFormat formatter, long lowLimit,
                long highLimit)
        {
            try
            {
                long count = 0;
                long increment = 1;
                for (long i = lowLimit; i <= highLimit; i += increment)
                {
                    if (count % 1000 == 0)
                        Logln(i.ToString(CultureInfo.InvariantCulture));

                    if (Math.Abs(i) < 5000)
                        increment = 1;
                    else if (Math.Abs(i) < 500000)
                        increment = 2737;
                    else
                        increment = 267437;

                    string text = formatter.Format(i);
                    long rt = formatter.Parse(text).ToInt64();

                    if (rt != i)
                    {
                        Errln("Round-trip failed: " + i + " -> " + text +
                                        " -> " + rt);
                    }

                    ++count;
                }

                if (lowLimit < 0)
                {
                    double d = 1.234;
                    while (d < 1000)
                    {
                        string text = formatter.Format(d);
                        double rt = formatter.Parse(text).ToDouble();

                        if (rt != d)
                        {
                            Errln("Round-trip failed: " + d + " -> " + text +
                                            " -> " + rt);
                        }
                        d *= 10;
                    }
                }
            }
            catch (Exception e)
            {
                Errln("Test failed with exception: " + e.ToString());
                //e.printStackTrace();
            }
        }

        void doTest(RbnfFormattterSettings formatterSettings, long lowLimit,
                long highLimit)
        {
            RuleBasedNumberFormat formatter = formatterSettings.formatter;
            try
            {
                long count = 0;
                long increment = 1;
                for (long i = lowLimit; i <= highLimit; i += increment)
                {
                    if (count % 1000 == 0)
                        Logln(i.ToString(CultureInfo.InvariantCulture));

                    if (Math.Abs(i) < 5000)
                        increment = 1;
                    else if (Math.Abs(i) < 500000)
                        increment = 2737;
                    else
                        increment = 267437;

                    string text = formatter.Format(i);
                    long rt = formatter.Parse(text).ToInt64();

                    if (rt != i)
                    {
                        Errln("Round-trip failed: " + i + " -> " + text +
                                        " -> " + rt);
                    }

                    ++count;
                }

                if (lowLimit < 0)
                {
                    double d = 1.234;
                    while (d < 1000)
                    {
                        string text = formatter.Format(d);
                        double rt = formatter.Parse(text).ToDouble();

                        if (rt != d)
                        {
                            Errln("Round-trip failed: " + d + " -> " + text +
                                            " -> " + rt);
                        }
                        d *= 10;
                    }
                }
            }
            catch (Exception e)
            {
                Errln("Test failed with exception: " + e.ToString());
                //e.printStackTrace();
            }

            try
            {
                long count = 0;
                long increment = 1;
                for (long i = lowLimit; i <= highLimit; i += increment)
                {
                    if (count % 1000 == 0)
                        Logln(i.ToString(CultureInfo.InvariantCulture));

                    if (Math.Abs(i) < 5000)
                        increment = 1;
                    else if (Math.Abs(i) < 500000)
                        increment = 2737;
                    else
                        increment = 267437;

                    string text = formatterSettings.FormatWithIcuNumber(i);
                    long rt = formatter.Parse(text).ToInt64();

                    if (rt != i)
                    {
                        Errln("Round-trip failed: " + i + " -> " + text +
                                        " -> " + rt);
                    }

                    ++count;
                }

                if (lowLimit < 0)
                {
                    double d = 1.234;
                    while (d < 1000)
                    {
                        string text = formatterSettings.FormatWithIcuNumber(d);
                        double rt = formatter.Parse(text).ToDouble();

                        if (rt != d)
                        {
                            Errln("Round-trip failed: " + d + " -> " + text +
                                            " -> " + rt);
                        }
                        d *= 10;
                    }
                }
            }
            catch (Exception e)
            {
                Errln("Test failed with exception: " + e.ToString());
                //e.printStackTrace();
            }
        }




    }


}
