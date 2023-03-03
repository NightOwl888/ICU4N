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
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("en-US"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the duration-formatting rules
         */
        [Test]
        public void TestDurationsRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("en-US"),
                            NumberPresentation.Duration);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Spanish spellout rules
         */
        [Test]
        public void TestSpanishSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("es-es"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the French spellout rules
         */
        [Test]
        public void TestFrenchSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("fr-FR"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Swiss French spellout rules
         */
        [Test]
        public void TestSwissFrenchSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("fr-CH"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Italian spellout rules
         */
        [Test]
        public void TestItalianSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("it-IT"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -999999, 999999);
        }

        /**
         * Perform an exhaustive round-trip test on the German spellout rules
         */
        [Test]
        public void TestGermanSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("de-DE"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Swedish spellout rules
         */
        [Test]
        public void TestSwedishSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("sv-SE"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Dutch spellout rules
         */
        [Test]
        public void TestDutchSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("nl-NL"),
                            NumberPresentation.SpellOut);

            doTest(formatter, -12345678, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Japanese spellout rules
         */
        [Test]
        public void TestJapaneseSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("ja-JP"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Russian spellout rules
         */
        [Test]
        [Ignore("ICU4N TODO: This test is very slow (4.5 min). In Java, this takes 15 seconds.")]
        [Timeout(400000)]
        public void TestRussianSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("ru-RU"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Greek spellout rules
         */
        [Test]
        public void TestGreekSpelloutRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("el-GR"),
                            NumberPresentation.SpellOut);

            doTest(formatter, 0, 12345678);
        }

        /**
         * Perform an exhaustive round-trip test on the Hebrew numbering system rules
         */
        [Test]
        public void TestHebrewNumberingRT()
        {
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("he-IL"),
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
            RuleBasedNumberFormat formatter
                            = new RuleBasedNumberFormat(new CultureInfo("en"),
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
    }
}
