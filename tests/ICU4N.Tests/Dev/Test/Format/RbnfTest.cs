using ICU4N.Globalization;
using ICU4N.Text;
using J2N.Globalization;
using J2N.Numerics;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using StringBuffer = System.Text.StringBuilder;
using Double = J2N.Numerics.Double;
using ICU4N.Numerics;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// This does not test lenient parse mode, since testing the default implementation
    /// introduces a dependency on collation.  See RbnfLenientScannerTest.
    /// </summary>
    public class RbnfTest : TestFmwk
    {
        static String fracRules =
            "%main:\n" +
            // this rule formats the number if it's 1 or more.  It formats
            // the integral part using a DecimalFormat ("#,##0" puts
            // thousands separators in the right places) and the fractional
            // part using %%frac.  If there is no fractional part, it
            // just shows the integral part.
            "    x.0: <#,##0<[ >%%frac>];\n" +
            // this rule formats the number if it's between 0 and 1.  It
            // shows only the fractional part (0.5 shows up as "1/2," not
            // "0 1/2")
            "    0.x: >%%frac>;\n" +
            // the fraction rule set.  This works the same way as the one in the
            // preceding example: We multiply the fractional part of the number
            // being formatted by each rule's base value and use the rule that
            // produces the result closest to 0 (or the first rule that produces 0).
            // Since we only provide rules for the numbers from 2 to 10, we know
            // we'll get a fraction with a denominator between 2 and 10.
            // "<0<" causes the numerator of the fraction to be formatted
            // using numerals
            "%%frac:\n" +
            "    2: 1/2;\n" +
            "    3: <0</3;\n" +
            "    4: <0</4;\n" +
            "    5: <0</5;\n" +
            "    6: <0</6;\n" +
            "    7: <0</7;\n" +
            "    8: <0</8;\n" +
            "    9: <0</9;\n" +
            "   10: <0</10;\n";

        [Test]
        public void TestCoverage()
        {
            String durationInSecondsRules =
                // main rule set for formatting with words
                "%with-words:\n"
                // take care of singular and plural forms of "second"
                + "    0 seconds; 1 second; =0= seconds;\n"
                // use %%min to format values greater than 60 seconds
                + "    60/60: <%%min<[, >>];\n"
                // use %%hr to format values greater than 3,600 seconds
                // (the ">>>" below causes us to see the number of minutes
                // when when there are zero minutes)
                + "    3600/60: <%%hr<[, >>>];\n"
                // this rule set takes care of the singular and plural forms
                // of "minute"
                + "%%min:\n"
                + "    0 minutes; 1 minute; =0= minutes;\n"
                // this rule set takes care of the singular and plural forms
                // of "hour"
                + "%%hr:\n"
                + "    0 hours; 1 hour; =0= hours;\n"

                // main rule set for formatting in numerals
                + "%in-numerals:\n"
                // values below 60 seconds are shown with "sec."
                + "    =0= sec.;\n"
                // higher values are shown with colons: %%min-sec is used for
                // values below 3,600 seconds...
                + "    60: =%%min-sec=;\n"
                // ...and %%hr-min-sec is used for values of 3,600 seconds
                // and above
                + "    3600: =%%hr-min-sec=;\n"
                // this rule causes values of less than 10 minutes to show without
                // a leading zero
                + "%%min-sec:\n"
                + "    0: :=00=;\n"
                + "    60/60: <0<>>;\n"
                // this rule set is used for values of 3,600 or more.  Minutes are always
                // shown, and always shown with two digits
                + "%%hr-min-sec:\n"
                + "    0: :=00=;\n"
                + "    60/60: <00<>>;\n"
                + "    3600/60: <#,##0<:>>>;\n"
                // the lenient-parse rules allow several different characters to be used
                // as delimiters between hours, minutes, and seconds
                + "%%lenient-parse:\n"
                + "    & : = . = ' ' = -;\n";

            // extra calls to boost coverage numbers
            RuleBasedNumberFormat fmt0 = new RuleBasedNumberFormat(NumberPresentation.SpellOut);
            RuleBasedNumberFormat fmt1 = (RuleBasedNumberFormat)fmt0.Clone();
            RuleBasedNumberFormat fmt2 = new RuleBasedNumberFormat(NumberPresentation.SpellOut);
            if (!fmt0.Equals(fmt0))
            {
                Errln("self equality fails");
            }
            if (!fmt0.Equals(fmt1))
            {
                Errln("clone equality fails");
            }
            if (!fmt0.Equals(fmt2))
            {
                Errln("duplicate equality fails");
            }
            String str = fmt0.ToString();
            Logln(str);

            RuleBasedNumberFormat fmt3 = new RuleBasedNumberFormat(durationInSecondsRules);

            if (fmt0.Equals(fmt3))
            {
                Errln("nonequal fails");
            }
            if (!fmt3.Equals(fmt3))
            {
                Errln("self equal 2 fails");
            }
            str = fmt3.ToString();
            Logln(str);

            String[] names = fmt3.GetRuleSetNames();

            try
            {
                fmt3.SetDefaultRuleSet(null);
                fmt3.SetDefaultRuleSet("%%foo");
                Errln("sdrf %%foo didn't fail");
            }
            catch (Exception e)
            {
                Logln("Got the expected exception");
            }

            try
            {
                fmt3.SetDefaultRuleSet("%bogus");
                Errln("sdrf %bogus didn't fail");
            }
            catch (Exception e)
            {
                Logln("Got the expected exception");
            }

            try
            {
                str = fmt3.Format(2.3, names[0]);
                Logln(str);
                str = fmt3.Format(2.3, "%%foo");
                Errln("format double %%foo didn't fail");
            }
            catch (Exception e)
            {
                Logln("Got the expected exception");
            }

            try
            {
                str = fmt3.Format(123L, names[0]);
                Logln(str);
                str = fmt3.Format(123L, "%%foo");
                Errln("format double %%foo didn't fail");
            }
            catch (Exception e)
            {
                Logln("Got the expected exception");
            }

            RuleBasedNumberFormat fmt4 = new RuleBasedNumberFormat(fracRules, new CultureInfo("en"));
            RuleBasedNumberFormat fmt5 = new RuleBasedNumberFormat(fracRules, new CultureInfo("en"));
            str = fmt4.ToString();
            Logln(str);
            if (!fmt4.Equals(fmt5))
            {
                Errln("duplicate 2 equality failed");
            }
            str = fmt4.Format(123L);
            Logln(str);
            try
            {
                Number num = fmt4.Parse(str);
                Logln(num.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Errln("parse caught exception");
            }

            str = fmt4.Format(.000123);
            Logln(str);
            try
            {
                Number num = fmt4.Parse(str);
                Logln(num.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Errln("parse caught exception");
            }

            str = fmt4.Format(456.000123);
            Logln(str);
            try
            {
                Number num = fmt4.Parse(str);
                Logln(num.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Errln("parse caught exception");
            }
        }

        [Test]
        public void TestUndefinedSpellout()
        {
            CultureInfo greek = new CultureInfo("el");
            RuleBasedNumberFormat[] formatters = {
                new RuleBasedNumberFormat(greek, NumberPresentation.SpellOut),
                new RuleBasedNumberFormat(greek, NumberPresentation.Ordinal),
                new RuleBasedNumberFormat(greek, NumberPresentation.Duration),
            };

            String[] data = {
                "0",
                "1",
                "15",
                "20",
                "23",
                "73",
                "88",
                "100",
                "106",
                "127",
                "200",
                "579",
                "1,000",
                "2,000",
                "3,004",
                "4,567",
                "15,943",
                "105,000",
                "2,345,678",
                "-36",
                "-36.91215",
                "234.56789"
            };

            NumberFormat decFormat = NumberFormat.GetInstance(new CultureInfo("en-US"));
            for (int j = 0; j < formatters.Length; ++j)
            {
                NumberFormat formatter = formatters[j];
                Logln("formatter[" + j + "]");
                for (int i = 0; i < data.Length; ++i)
                {
                    try
                    {
                        String result = formatter.Format(decFormat.Parse(data[i]));
                        Logln("[" + i + "] " + data[i] + " ==> " + result);
                    }
                    catch (Exception e)
                    {
                        Errln("formatter[" + j + "], data[" + i + "] " + data[i] + " threw exception " + e.Message);
                    }
                }
            }
        }

        /**
         * Perform a simple spot check on the English spellout rules
         */
        [Test]
        public void TestEnglishSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("en-US"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "one" },
                new string[] { "15", "fifteen" },
                new string[] { "20", "twenty" },
                new string[] { "23", "twenty-three" },
                new string[] { "73", "seventy-three" },
                new string[] { "88", "eighty-eight" },
                new string[] { "100", "one hundred" },
                new string[] { "106", "one hundred six" },
                new string[] { "127", "one hundred twenty-seven" },
                new string[] { "200", "two hundred" },
                new string[] { "579", "five hundred seventy-nine" },
                new string[] { "1,000", "one thousand" },
                new string[] { "2,000", "two thousand" },
                new string[] { "3,004", "three thousand four" },
                new string[] { "4,567", "four thousand five hundred sixty-seven" },
                new string[] { "15,943", "fifteen thousand nine hundred forty-three" },
                new string[] { "2,345,678", "two million three hundred forty-five "
                        + "thousand six hundred seventy-eight" },
                new string[] { "-36", "minus thirty-six" },
                new string[] { "234.567", "two hundred thirty-four point five six seven" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the English ordinal-abbreviation rules
         */
        [Test]
        public void TestOrdinalAbbreviations()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("en-US"),
                    NumberPresentation.Ordinal);
            string[][] testData = new string[][] {
                new string[] { "1", "1st" },
                new string[] { "2", "2nd" },
                new string[] { "3", "3rd" },
                new string[] { "4", "4th" },
                new string[] { "7", "7th" },
                new string[] { "10", "10th" },
                new string[] { "11", "11th" },
                new string[] { "13", "13th" },
                new string[] { "20", "20th" },
                new string[] { "21", "21st" },
                new string[] { "22", "22nd" },
                new string[] { "23", "23rd" },
                new string[] { "24", "24th" },
                new string[] { "33", "33rd" },
                new string[] { "102", "102nd" },
                new string[] { "312", "312th" },
                new string[] { "12,345", "12,345th" }
            };

            doTest(formatter, testData, false);
        }

        /**
         * Perform a simple spot check on the duration-formatting rules
         */
        [Test]
        public void TestDurations()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("en-US"),
                   NumberPresentation.Duration);
            string[][] testData = new string[][] {
                new string[] { "3,600", "1:00:00" },             //move me and I fail
                new string[] { "0", "0 sec." },
                new string[] { "1", "1 sec." },
                new string[] { "24", "24 sec." },
                new string[] { "60", "1:00" },
                new string[] { "73", "1:13" },
                new string[] { "145", "2:25" },
                new string[] { "666", "11:06" },
                //            new string[] { "3,600", "1:00:00" },
                new string[] { "3,740", "1:02:20" },
                new string[] { "10,293", "2:51:33" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the Spanish spellout rules
         */
        [Test]
        public void TestSpanishSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("es-es"),
                NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "uno" },
                new string[] { "6", "seis" },
                new string[] { "16", "diecis\u00e9is" },
                new string[] { "20", "veinte" },
                new string[] { "24", "veinticuatro" },
                new string[] { "26", "veintis\u00e9is" },
                new string[] { "73", "setenta y tres" },
                new string[] { "88", "ochenta y ocho" },
                new string[] { "100", "cien" },
                new string[] { "106", "ciento seis" },
                new string[] { "127", "ciento veintisiete" },
                new string[] { "200", "doscientos" },
                new string[] { "579", "quinientos setenta y nueve" },
                new string[] { "1,000", "mil" },
                new string[] { "2,000", "dos mil" },
                new string[] { "3,004", "tres mil cuatro" },
                new string[] { "4,567", "cuatro mil quinientos sesenta y siete" },
                new string[] { "15,943", "quince mil novecientos cuarenta y tres" },
                new string[] { "2,345,678", "dos millones trescientos cuarenta y cinco mil "
                        + "seiscientos setenta y ocho"},
                new string[] { "-36", "menos treinta y seis" },
                new string[] { "234.567", "doscientos treinta y cuatro coma cinco seis siete" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the French spellout rules
         */
        [Test]
        public void TestFrenchSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("fr-FR"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "un" },
                new string[] { "15", "quinze" },
                new string[] { "20", "vingt" },
                new string[] { "21", "vingt-et-un" },
                new string[] { "23", "vingt-trois" },
                new string[] { "62", "soixante-deux" },
                new string[] { "70", "soixante-dix" },
                new string[] { "71", "soixante-et-onze" },
                new string[] { "73", "soixante-treize" },
                new string[] { "80", "quatre-vingts" },
                new string[] { "88", "quatre-vingt-huit" },
                new string[] { "100", "cent" },
                new string[] { "106", "cent six" },
                new string[] { "127", "cent vingt-sept" },
                new string[] { "200", "deux cents" },
                new string[] { "579", "cinq cent soixante-dix-neuf" },
                new string[] { "1,000", "mille" },
                new string[] { "1,123", "mille cent vingt-trois" },
                new string[] { "1,594", "mille cinq cent quatre-vingt-quatorze" },
                new string[] { "2,000", "deux mille" },
                new string[] { "3,004", "trois mille quatre" },
                new string[] { "4,567", "quatre mille cinq cent soixante-sept" },
                new string[] { "15,943", "quinze mille neuf cent quarante-trois" },
                new string[] { "2,345,678", "deux millions trois cent quarante-cinq mille "
                        + "six cent soixante-dix-huit" },
                new string[] { "-36", "moins trente-six" },
                new string[] { "234.567", "deux cent trente-quatre virgule cinq six sept" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the Swiss French spellout rules
         */
        [Test]
        public void TestSwissFrenchSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("fr-CH"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "un" },
                new string[] { "15", "quinze" },
                new string[] { "20", "vingt" },
                new string[] { "21", "vingt-et-un" },
                new string[] { "23", "vingt-trois" },
                new string[] { "62", "soixante-deux" },
                new string[] { "70", "septante" },
                new string[] { "71", "septante-et-un" },
                new string[] { "73", "septante-trois" },
                new string[] { "80", "huitante" },
                new string[] { "88", "huitante-huit" },
                new string[] { "100", "cent" },
                new string[] { "106", "cent six" },
                new string[] { "127", "cent vingt-sept" },
                new string[] { "200", "deux cents" },
                new string[] { "579", "cinq cent septante-neuf" },
                new string[] { "1,000", "mille" },
                new string[] { "1,123", "mille cent vingt-trois" },
                new string[] { "1,594", "mille cinq cent nonante-quatre" },
                new string[] { "2,000", "deux mille" },
                new string[] { "3,004", "trois mille quatre" },
                new string[] { "4,567", "quatre mille cinq cent soixante-sept" },
                new string[] { "15,943", "quinze mille neuf cent quarante-trois" },
                new string[] { "2,345,678", "deux millions trois cent quarante-cinq mille "
                        + "six cent septante-huit" },
                new string[] { "-36", "moins trente-six" },
                new string[] { "234.567", "deux cent trente-quatre virgule cinq six sept" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the Italian spellout rules
         */
        [Test]
        public void TestItalianSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("it"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "uno" },
                new string[] { "15", "quindici" },
                new string[] { "20", "venti" },
                new string[] { "23", "venti\u00ADtr\u00E9" },
                new string[] { "73", "settanta\u00ADtr\u00E9" },
                new string[] { "88", "ottant\u00ADotto" },
                new string[] { "100", "cento" },
                new string[] { "106", "cento\u00ADsei" },
                new string[] { "108", "cent\u00ADotto" },
                new string[] { "127", "cento\u00ADventi\u00ADsette" },
                new string[] { "181", "cent\u00ADottant\u00ADuno" },
                new string[] { "200", "due\u00ADcento" },
                new string[] { "579", "cinque\u00ADcento\u00ADsettanta\u00ADnove" },
                new string[] { "1,000", "mille" },
                new string[] { "2,000", "due\u00ADmila" },
                new string[] { "3,004", "tre\u00ADmila\u00ADquattro" },
                new string[] { "4,567", "quattro\u00ADmila\u00ADcinque\u00ADcento\u00ADsessanta\u00ADsette" },
                new string[] { "15,943", "quindici\u00ADmila\u00ADnove\u00ADcento\u00ADquaranta\u00ADtr\u00E9" },
                new string[] { "-36", "meno trenta\u00ADsei" },
                new string[] { "234.567", "due\u00ADcento\u00ADtrenta\u00ADquattro virgola cinque sei sette" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the German spellout rules
         */
        [Test]
        public void TestGermanSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("de-DE"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "1", "eins" },
                new string[] { "15", "f\u00fcnfzehn" },
                new string[] { "20", "zwanzig" },
                new string[] { "23", "drei\u00ADund\u00ADzwanzig" },
                new string[] { "73", "drei\u00ADund\u00ADsiebzig" },
                new string[] { "88", "acht\u00ADund\u00ADachtzig" },
                new string[] { "100", "ein\u00ADhundert" },
                new string[] { "106", "ein\u00ADhundert\u00ADsechs" },
                new string[] { "127", "ein\u00ADhundert\u00ADsieben\u00ADund\u00ADzwanzig" },
                new string[] { "200", "zwei\u00ADhundert" },
                new string[] { "579", "f\u00fcnf\u00ADhundert\u00ADneun\u00ADund\u00ADsiebzig" },
                new string[] { "1,000", "ein\u00ADtausend" },
                new string[] { "2,000", "zwei\u00ADtausend" },
                new string[] { "3,004", "drei\u00ADtausend\u00ADvier" },
                new string[] { "4,567", "vier\u00ADtausend\u00ADf\u00fcnf\u00ADhundert\u00ADsieben\u00ADund\u00ADsechzig" },
                new string[] { "15,943", "f\u00fcnfzehn\u00ADtausend\u00ADneun\u00ADhundert\u00ADdrei\u00ADund\u00ADvierzig" },
                new string[] { "2,345,678", "zwei Millionen drei\u00ADhundert\u00ADf\u00fcnf\u00ADund\u00ADvierzig\u00ADtausend\u00AD"
                        + "sechs\u00ADhundert\u00ADacht\u00ADund\u00ADsiebzig" }
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the Thai spellout rules
         */
        [Test]
        public void TestThaiSpellout()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("th-TH"),
                    NumberPresentation.SpellOut);
            string[][] testData = new string[][] {
                new string[] { "0", "\u0e28\u0e39\u0e19\u0e22\u0e4c" },
                new string[] { "1", "\u0e2b\u0e19\u0e36\u0e48\u0e07" },
                new string[] { "10", "\u0e2a\u0e34\u0e1a" },
                new string[] { "11", "\u0e2a\u0e34\u0e1a\u200b\u0e40\u0e2d\u0e47\u0e14" },
                new string[] { "21", "\u0e22\u0e35\u0e48\u200b\u0e2a\u0e34\u0e1a\u200b\u0e40\u0e2d\u0e47\u0e14" },
                new string[] { "101", "\u0e2b\u0e19\u0e36\u0e48\u0e07\u200b\u0e23\u0e49\u0e2d\u0e22\u200b\u0e2b\u0e19\u0e36\u0e48\u0e07" },
                new string[] { "1.234", "\u0e2b\u0e19\u0e36\u0e48\u0e07\u200b\u0e08\u0e38\u0e14\u200b\u0e2a\u0e2d\u0e07\u0e2a\u0e32\u0e21\u0e2a\u0e35\u0e48" },
                new string[] { "21.45", "\u0e22\u0e35\u0e48\u200b\u0e2a\u0e34\u0e1a\u200b\u0e40\u0e2d\u0e47\u0e14\u200b\u0e08\u0e38\u0e14\u200b\u0e2a\u0e35\u0e48\u0e2b\u0e49\u0e32" },
                new string[] { "22.45", "\u0e22\u0e35\u0e48\u200b\u0e2a\u0e34\u0e1a\u200b\u0e2a\u0e2d\u0e07\u200b\u0e08\u0e38\u0e14\u200b\u0e2a\u0e35\u0e48\u0e2b\u0e49\u0e32" },
                new string[] { "23.45", "\u0e22\u0e35\u0e48\u200b\u0e2a\u0e34\u0e1a\u200b\u0e2a\u0e32\u0e21\u200b\u0e08\u0e38\u0e14\u200b\u0e2a\u0e35\u0e48\u0e2b\u0e49\u0e32" },
                new string[] { "123.45", "\u0e2b\u0e19\u0e36\u0e48\u0e07\u200b\u0e23\u0e49\u0e2d\u0e22\u200b\u0e22\u0e35\u0e48\u200b\u0e2a\u0e34\u0e1a\u200b\u0e2a\u0e32\u0e21\u200b\u0e08\u0e38\u0e14\u200b\u0e2a\u0e35\u0e48\u0e2b\u0e49\u0e32" },
                new string[] { "12,345.678", "\u0E2B\u0E19\u0E36\u0E48\u0E07\u200b\u0E2B\u0E21\u0E37\u0E48\u0E19\u200b\u0E2A\u0E2D\u0E07\u200b\u0E1E\u0E31\u0E19\u200b\u0E2A\u0E32\u0E21\u200b\u0E23\u0E49\u0E2D\u0E22\u200b\u0E2A\u0E35\u0E48\u200b\u0E2A\u0E34\u0E1A\u200b\u0E2B\u0E49\u0E32\u200b\u0E08\u0E38\u0E14\u200b\u0E2B\u0E01\u0E40\u0E08\u0E47\u0E14\u0E41\u0E1B\u0E14" },
            };

            doTest(formatter, testData, true);
        }

        /**
         * Perform a simple spot check on the ordinal spellout rules
         */
        [Test]
        public void TestPluralRules()
        {
            String enRules = "%digits-ordinal:"
                    + "-x: −>>;"
                    + "0: =#,##0=$(ordinal,one{st}two{nd}few{rd}other{th})$;";
            RuleBasedNumberFormat enFormatter = new RuleBasedNumberFormat(enRules, new UCultureInfo("en"));
            string[][] enTestData = new string[][] {
                new string[] { "1", "1st" },
                new string[] { "2", "2nd" },
                new string[] { "3", "3rd" },
                new string[] { "4", "4th" },
                new string[] { "11", "11th" },
                new string[] { "12", "12th" },
                new string[] { "13", "13th" },
                new string[] { "14", "14th" },
                new string[] { "21", "21st" },
                new string[] { "22", "22nd" },
                new string[] { "23", "23rd" },
                new string[] { "24", "24th" },
            };

            doTest(enFormatter, enTestData, true);

            // This is trying to model the feminine form, but don't worry about the details too much.
            // We're trying to test the plural rules.
            String ruRules = "%spellout-numbering:"
                    + "-x: минус >>;"
                    + "x.x: [<< $(cardinal,one{целый}other{целых})$ ]>%%fractions-feminine>;"
                    + "0: ноль;"
                    + "1: один;"
                    + "2: два;"
                    + "3: три;"
                    + "4: четыре;"
                    + "5: пять;"
                    + "6: шесть;"
                    + "7: семь;"
                    + "8: восемь;"
                    + "9: девять;"
                    + "10: десять;"
                    + "11: одиннадцать;"
                    + "12: двенадцать;"
                    + "13: тринадцать;"
                    + "14: четырнадцать;"
                    + "15: пятнадцать;"
                    + "16: шестнадцать;"
                    + "17: семнадцать;"
                    + "18: восемнадцать;"
                    + "19: девятнадцать;"
                    + "20: двадцать[ >>];"
                    + "30: тридцать[ >>];"
                    + "40: сорок[ >>];"
                    + "50: пятьдесят[ >>];"
                    + "60: шестьдесят[ >>];"
                    + "70: семьдесят[ >>];"
                    + "80: восемьдесят[ >>];"
                    + "90: девяносто[ >>];"
                    + "100: сто[ >>];"
                    + "200: <<сти[ >>];"
                    + "300: <<ста[ >>];"
                    + "500: <<сот[ >>];"
                    + "1000: << $(cardinal,one{тысяча}few{тысячи}other{тысяч})$[ >>];"
                    + "1000000: << $(cardinal,one{миллион}few{миллионы}other{миллионов})$[ >>];"
                    + "%%fractions-feminine:"
                    + "10: <%spellout-numbering< $(cardinal,one{десятая}other{десятых})$;"
                    + "100: <%spellout-numbering< $(cardinal,one{сотая}other{сотых})$;";
            RuleBasedNumberFormat ruFormatter = new RuleBasedNumberFormat(ruRules, new UCultureInfo("ru"));
            string[][] ruTestData = new string[][] {
                new string[] { "1", "один" },
                new string[] { "100", "сто" },
                new string[] { "125", "сто двадцать пять" },
                new string[] { "399", "триста девяносто девять" },
                new string[] { "1,000", "один тысяча" },
                new string[] { "1,001", "один тысяча один" },
                new string[] { "2,000", "два тысячи" },
                new string[] { "2,001", "два тысячи один" },
                new string[] { "2,002", "два тысячи два" },
                new string[] { "3,333", "три тысячи триста тридцать три" },
                new string[] { "5,000", "пять тысяч" },
                new string[] { "11,000", "одиннадцать тысяч" },
                new string[] { "21,000", "двадцать один тысяча" },
                new string[] { "22,000", "двадцать два тысячи" },
                new string[] { "25,001", "двадцать пять тысяч один" },
                new string[] { "0.1", "один десятая" },
                new string[] { "0.2", "два десятых" },
                new string[] { "0.21", "двадцать один сотая" },
                new string[] { "0.22", "двадцать два сотых" },
                new string[] { "21.1", "двадцать один целый один десятая" },
                new string[] { "22.2", "двадцать два целых два десятых" },
            };

            doTest(ruFormatter, ruTestData, true);

            // Make sure there are no divide by 0 errors.
            String result = new RuleBasedNumberFormat(ruRules, new UCultureInfo("ru")).Format(21000);
            if (!"двадцать один тысяча".Equals(result, StringComparison.Ordinal))
            {
                Errln("Got " + result + " for 21000");
            }
        }

        /**
         * Perform a simple spot check on the parsing going into an infinite loop for alternate rules.
         */
        [Test]
        public void TestMultiplePluralRules()
        {
            // This is trying to model the feminine form, but don't worry about the details too much.
            // We're trying to test the plural rules where there are different prefixes.
            String ruRules = "%spellout-cardinal-feminine-genitive:"
                    + "-x: минус >>;"
                    + "x.x: << запятая >>;"
                    + "0: ноля;"
                    + "1: одной;"
                    + "2: двух;"
                    + "3: трех;"
                    + "4: четырех;"
                    + "5: пяти;"
                    + "6: шести;"
                    + "7: семи;"
                    + "8: восьми;"
                    + "9: девяти;"
                    + "10: десяти;"
                    + "11: одиннадцати;"
                    + "12: двенадцати;"
                    + "13: тринадцати;"
                    + "14: четырнадцати;"
                    + "15: пятнадцати;"
                    + "16: шестнадцати;"
                    + "17: семнадцати;"
                    + "18: восемнадцати;"
                    + "19: девятнадцати;"
                    + "20: двадцати[ >>];"
                    + "30: тридцати[ >>];"
                    + "40: сорока[ >>];"
                    + "50: пятидесяти[ >>];"
                    + "60: шестидесяти[ >>];"
                    + "70: семидесяти[ >>];"
                    + "80: восемидесяти[ >>];"
                    + "90: девяноста[ >>];"
                    + "100: ста[ >>];"
                    + "200: <<сот[ >>];"
                    + "1000: << $(cardinal,one{тысяча}few{тысячи}other{тысяч})$[ >>];"
                    + "1000000: =#,##0=;"
                    + "%spellout-cardinal-feminine:"
                    + "-x: минус >>;"
                    + "x.x: << запятая >>;"
                    + "0: ноль;"
                    + "1: одна;"
                    + "2: две;"
                    + "3: три;"
                    + "4: четыре;"
                    + "5: пять;"
                    + "6: шесть;"
                    + "7: семь;"
                    + "8: восемь;"
                    + "9: девять;"
                    + "10: десять;"
                    + "11: одиннадцать;"
                    + "12: двенадцать;"
                    + "13: тринадцать;"
                    + "14: четырнадцать;"
                    + "15: пятнадцать;"
                    + "16: шестнадцать;"
                    + "17: семнадцать;"
                    + "18: восемнадцать;"
                    + "19: девятнадцать;"
                    + "20: двадцать[ >>];"
                    + "30: тридцать[ >>];"
                    + "40: сорок[ >>];"
                    + "50: пятьдесят[ >>];"
                    + "60: шестьдесят[ >>];"
                    + "70: семьдесят[ >>];"
                    + "80: восемьдесят[ >>];"
                    + "90: девяносто[ >>];"
                    + "100: сто[ >>];"
                    + "200: <<сти[ >>];"
                    + "300: <<ста[ >>];"
                    + "500: <<сот[ >>];"
                    + "1000: << $(cardinal,one{тысяча}few{тысячи}other{тысяч})$[ >>];"
                    + "1000000: =#,##0=;";
            RuleBasedNumberFormat ruFormatter = new RuleBasedNumberFormat(ruRules, new UCultureInfo("ru"));
            try
            {
                Number result;
                if (1000 != (result = ruFormatter.Parse(ruFormatter.Format(1000))).ToDouble())
                {
                    Errln("RuleBasedNumberFormat did not return the correct value. Got: " + result);
                }
                if (1000 != (result = ruFormatter.Parse(ruFormatter.Format(1000, "%spellout-cardinal-feminine-genitive"))).ToDouble())
                {
                    Errln("RuleBasedNumberFormat did not return the correct value. Got: " + result);
                }
                if (1000 != (result = ruFormatter.Parse(ruFormatter.Format(1000, "%spellout-cardinal-feminine"))).ToDouble())
                {
                    Errln("RuleBasedNumberFormat did not return the correct value. Got: " + result);
                }
            }
            //catch (ParseException e)
            catch (FormatException e)
            {
                Errln(e.ToString());
            }
        }

        [Test]
        public void TestFractionalRuleSet()
        {
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(fracRules,
                    new CultureInfo("en"));

            string[][] testData = new string[][] {
                new string[] { "0", "0" },
                new string[] { "1", "1" },
                new string[] { "10", "10" },
                new string[] { ".1", "1/10" },
                new string[] { ".11", "1/9" },
                new string[] { ".125", "1/8" },
                new string[] { ".1428", "1/7" },
                new string[] { ".1667", "1/6" },
                new string[] { ".2", "1/5" },
                new string[] { ".25", "1/4" },
                new string[] { ".333", "1/3" },
                new string[] { ".5", "1/2" },
                new string[] { "1.1", "1 1/10" },
                new string[] { "2.11", "2 1/9" },
                new string[] { "3.125", "3 1/8" },
                new string[] { "4.1428", "4 1/7" },
                new string[] { "5.1667", "5 1/6" },
                new string[] { "6.2", "6 1/5" },
                new string[] { "7.25", "7 1/4" },
                new string[] { "8.333", "8 1/3" },
                new string[] { "9.5", "9 1/2" },
                new string[] { ".2222", "2/9" },
                new string[] { ".4444", "4/9" },
                new string[] { ".5555", "5/9" },
                new string[] { "1.2856", "1 2/7" }
            };
            doTest(formatter, testData, false); // exact values aren't parsable from fractions
        }

        [Test]
        public void TestSwedishSpellout()
        {
            CultureInfo locale = new CultureInfo("sv");
            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(locale,
                    NumberPresentation.SpellOut);

            string[][] testDataDefault = new string[][] {
                new string[] { "101", "ett\u00ADhundra\u00ADett" },
                new string[] { "123", "ett\u00ADhundra\u00ADtjugo\u00ADtre" },
                new string[] { "1,001", "et\u00ADtusen ett" },
                new string[] { "1,100", "et\u00ADtusen ett\u00ADhundra" },
                new string[] { "1,101", "et\u00ADtusen ett\u00ADhundra\u00ADett" },
                new string[] { "1,234", "et\u00ADtusen tv\u00e5\u00ADhundra\u00ADtrettio\u00ADfyra" },
                new string[] { "10,001", "tio\u00ADtusen ett" },
                new string[] { "11,000", "elva\u00ADtusen" },
                new string[] { "12,000", "tolv\u00ADtusen" },
                new string[] { "20,000", "tjugo\u00ADtusen" },
                new string[] { "21,000", "tjugo\u00ADet\u00ADtusen" },
                new string[] { "21,001", "tjugo\u00ADet\u00ADtusen ett" },
                new string[] { "200,000", "tv\u00e5\u00ADhundra\u00ADtusen" },
                new string[] { "201,000", "tv\u00e5\u00ADhundra\u00ADet\u00ADtusen" },
                new string[] { "200,200", "tv\u00e5\u00ADhundra\u00ADtusen tv\u00e5\u00ADhundra" },
                new string[] { "2,002,000", "tv\u00e5 miljoner tv\u00e5\u00ADtusen" },
                new string[] { "12,345,678", "tolv miljoner tre\u00ADhundra\u00ADfyrtio\u00ADfem\u00ADtusen sex\u00ADhundra\u00ADsjuttio\u00AD\u00e5tta" },
                new string[] { "123,456.789", "ett\u00ADhundra\u00ADtjugo\u00ADtre\u00ADtusen fyra\u00ADhundra\u00ADfemtio\u00ADsex komma sju \u00e5tta nio" },
                new string[] { "-12,345.678", "minus tolv\u00ADtusen tre\u00ADhundra\u00ADfyrtio\u00ADfem komma sex sju \u00e5tta" },
            };

            Logln("testing default rules");
            doTest(formatter, testDataDefault, true);

            string[][] testDataNeutrum = new string[][] {
                new string[] { "101", "ett\u00adhundra\u00adett" },
                new string[] { "1,001", "et\u00adtusen ett" },
                new string[] { "1,101", "et\u00adtusen ett\u00adhundra\u00adett" },
                new string[] { "10,001", "tio\u00adtusen ett" },
                new string[] { "21,001", "tjugo\u00adet\u00adtusen ett" }
            };

            formatter.SetDefaultRuleSet("%spellout-cardinal-neuter");
            Logln("testing neutrum rules");
            doTest(formatter, testDataNeutrum, true);

            string[][] testDataYear = new string[][] {
                new string[] { "101", "ett\u00adhundra\u00adett" },
                new string[] { "900", "nio\u00adhundra" },
                new string[] { "1,001", "et\u00adtusen ett" },
                new string[] { "1,100", "elva\u00adhundra" },
                new string[] { "1,101", "elva\u00adhundra\u00adett" },
                new string[] { "1,234", "tolv\u00adhundra\u00adtrettio\u00adfyra" },
                new string[] { "2,001", "tjugo\u00adhundra\u00adett" },
                new string[] { "10,001", "tio\u00adtusen ett" }
            };

            formatter.SetDefaultRuleSet("%spellout-numbering-year");
            Logln("testing year rules");
            doTest(formatter, testDataYear, true);
        }

        [Test]
        public void TestBigNumbers()
        {
            ICU4N.Numerics.BigMath.BigInteger bigI = ICU4N.Numerics.BigMath.BigInteger.Parse("1234567890", 10);
            StringBuffer buf = new StringBuffer();
            RuleBasedNumberFormat fmt = new RuleBasedNumberFormat(NumberPresentation.SpellOut);
            fmt.Format(bigI, buf, null);
            Logln("big int: " + buf.ToString());

            buf.Length = (0);
            ICU4N.Numerics.BigMath.BigDecimal bigD = new ICU4N.Numerics.BigMath.BigDecimal(bigI);
            fmt.Format(bigD, buf, null);
            Logln("big dec: " + buf.ToString());
        }

        [Test]
        public void TestTrailingSemicolon()
        {
            String thaiRules =
                "%default:\n" +
                "  -x: \u0e25\u0e1a>>;\n" +
                "  x.x: <<\u0e08\u0e38\u0e14>>>;\n" +
                "  \u0e28\u0e39\u0e19\u0e22\u0e4c; \u0e2b\u0e19\u0e36\u0e48\u0e07; \u0e2a\u0e2d\u0e07; \u0e2a\u0e32\u0e21;\n" +
                "  \u0e2a\u0e35\u0e48; \u0e2b\u0e49\u0e32; \u0e2b\u0e01; \u0e40\u0e08\u0e47\u0e14; \u0e41\u0e1b\u0e14;\n" +
                "  \u0e40\u0e01\u0e49\u0e32; \u0e2a\u0e34\u0e1a; \u0e2a\u0e34\u0e1a\u0e40\u0e2d\u0e47\u0e14;\n" +
                "  \u0e2a\u0e34\u0e1a\u0e2a\u0e2d\u0e07; \u0e2a\u0e34\u0e1a\u0e2a\u0e32\u0e21;\n" +
                "  \u0e2a\u0e34\u0e1a\u0e2a\u0e35\u0e48; \u0e2a\u0e34\u0e1a\u0e2b\u0e49\u0e32;\n" +
                "  \u0e2a\u0e34\u0e1a\u0e2b\u0e01; \u0e2a\u0e34\u0e1a\u0e40\u0e08\u0e47\u0e14;\n" +
                "  \u0e2a\u0e34\u0e1a\u0e41\u0e1b\u0e14; \u0e2a\u0e34\u0e1a\u0e40\u0e01\u0e49\u0e32;\n" +
                "  20: \u0e22\u0e35\u0e48\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  30: \u0e2a\u0e32\u0e21\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  40: \u0e2a\u0e35\u0e48\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  50: \u0e2b\u0e49\u0e32\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  60: \u0e2b\u0e01\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  70: \u0e40\u0e08\u0e47\u0e14\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  80: \u0e41\u0e1b\u0e14\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  90: \u0e40\u0e01\u0e49\u0e32\u0e2a\u0e34\u0e1a[>%%alt-ones>];\n" +
                "  100: <<\u0e23\u0e49\u0e2d\u0e22[>>];\n" +
                "  1000: <<\u0e1e\u0e31\u0e19[>>];\n" +
                "  10000: <<\u0e2b\u0e21\u0e37\u0e48\u0e19[>>];\n" +
                "  100000: <<\u0e41\u0e2a\u0e19[>>];\n" +
                "  1,000,000: <<\u0e25\u0e49\u0e32\u0e19[>>];\n" +
                "  1,000,000,000: <<\u0e1e\u0e31\u0e19\u0e25\u0e49\u0e32\u0e19[>>];\n" +
                "  1,000,000,000,000: <<\u0e25\u0e49\u0e32\u0e19\u0e25\u0e49\u0e32\u0e19[>>];\n" +
                "  1,000,000,000,000,000: =#,##0=;\n" +
                "%%alt-ones:\n" +
                "  \u0e28\u0e39\u0e19\u0e22\u0e4c;\n" +
                "  \u0e40\u0e2d\u0e47\u0e14;\n" +
                "  =%default=;\n ; ;; ";

            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(thaiRules, new CultureInfo("th-TH"));

            string[][] testData = new string[][] {
                new string[] { "0", "\u0e28\u0e39\u0e19\u0e22\u0e4c" },
                new string[] { "1", "\u0e2b\u0e19\u0e36\u0e48\u0e07" },
                new string[] { "123.45", "\u0e2b\u0e19\u0e36\u0e48\u0e07\u0e23\u0e49\u0e2d\u0e22\u0e22\u0e35\u0e48\u0e2a\u0e34\u0e1a\u0e2a\u0e32\u0e21\u0e08\u0e38\u0e14\u0e2a\u0e35\u0e48\u0e2b\u0e49\u0e32" }
            };

            doTest(formatter, testData, true);
        }

        [Test]
        public void TestSmallValues()
        {
            string[][] testData = new string[][] {
                new string[] { "0.001", "zero point zero zero one" },
                new string[] { "0.0001", "zero point zero zero zero one" },
                new string[] { "0.00001", "zero point zero zero zero zero one" },
                new string[] { "0.000001", "zero point zero zero zero zero zero one" },
                new string[] { "0.0000001", "zero point zero zero zero zero zero zero one" },
                new string[] { "0.00000001", "zero point zero zero zero zero zero zero zero one" },
                new string[] { "0.000000001", "zero point zero zero zero zero zero zero zero zero one" },
                new string[] { "0.0000000001", "zero point zero zero zero zero zero zero zero zero zero one" },
                new string[] { "0.00000000001", "zero point zero zero zero zero zero zero zero zero zero zero one" },
                new string[] { "0.000000000001", "zero point zero zero zero zero zero zero zero zero zero zero zero one" },
                new string[] { "0.0000000000001", "zero point zero zero zero zero zero zero zero zero zero zero zero zero one" },
                new string[] { "0.00000000000001", "zero point zero zero zero zero zero zero zero zero zero zero zero zero zero one" },
                new string[] { "0.000000000000001", "zero point zero zero zero zero zero zero zero zero zero zero zero zero zero zero one" },
                new string[] { "10,000,000.001", "ten million point zero zero one" },
                new string[] { "10,000,000.0001", "ten million point zero zero zero one" },
                new string[] { "10,000,000.00001", "ten million point zero zero zero zero one" },
                new string[] { "10,000,000.000001", "ten million point zero zero zero zero zero one" },
                new string[] { "10,000,000.0000001", "ten million point zero zero zero zero zero zero one" },
                new string[] { "10,000,000.00000001", "ten million point zero zero zero zero zero zero zero one" },
                new string[] { "10,000,000.000000002", "ten million point zero zero zero zero zero zero zero zero two" },
                new string[] { "10,000,000", "ten million" },
                new string[] { "1,234,567,890.0987654", "one billion two hundred thirty-four million five hundred sixty-seven thousand eight hundred ninety point zero nine eight seven six five four" },
                new string[] { "123,456,789.9876543", "one hundred twenty-three million four hundred fifty-six thousand seven hundred eighty-nine point nine eight seven six five four three" },
                new string[] { "12,345,678.87654321", "twelve million three hundred forty-five thousand six hundred seventy-eight point eight seven six five four three two one" },
                new string[] { "1,234,567.7654321", "one million two hundred thirty-four thousand five hundred sixty-seven point seven six five four three two one" },
                new string[] { "123,456.654321", "one hundred twenty-three thousand four hundred fifty-six point six five four three two one" },
                new string[] { "12,345.54321", "twelve thousand three hundred forty-five point five four three two one" },
                new string[] { "1,234.4321", "one thousand two hundred thirty-four point four three two one" },
                new string[] { "123.321", "one hundred twenty-three point three two one" },
                new string[] { "0.0000000011754944", "zero point zero zero zero zero zero zero zero zero one one seven five four nine four four" },
                new string[] { "0.000001175494351", "zero point zero zero zero zero zero one one seven five four nine four three five one" },
            };

            RuleBasedNumberFormat formatter = new RuleBasedNumberFormat(new CultureInfo("en-US"), NumberPresentation.SpellOut);
            doTest(formatter, testData, true);
        }

        [Test]
        public void TestRuleSetDisplayName()
        {
            Assume.That(!PlatformDetection.IsLinux, "LUCENENET TODO: On Linux, this test is failing for some unkown reason. Most likely, it is because the localizations array is not being processed correctly.");

            /*
             * Spellout rules for U.K. English.
             * This was borrowed from the rule sets for TestRuleSetDisplayName()
             */
            String ukEnglish =
                    "%simplified:\n"
                            + "    -x: minus >>;\n"
                            + "    x.x: << point >>;\n"
                            + "    zero; one; two; three; four; five; six; seven; eight; nine;\n"
                            + "    ten; eleven; twelve; thirteen; fourteen; fifteen; sixteen;\n"
                            + "        seventeen; eighteen; nineteen;\n"
                            + "    20: twenty[->>];\n"
                            + "    30: thirty[->>];\n"
                            + "    40: forty[->>];\n"
                            + "    50: fifty[->>];\n"
                            + "    60: sixty[->>];\n"
                            + "    70: seventy[->>];\n"
                            + "    80: eighty[->>];\n"
                            + "    90: ninety[->>];\n"
                            + "    100: << hundred[ >>];\n"
                            + "    1000: << thousand[ >>];\n"
                            + "    1,000,000: << million[ >>];\n"
                            + "    1,000,000,000,000: << billion[ >>];\n"
                            + "    1,000,000,000,000,000: =#,##0=;\n"
                            + "%alt-teens:\n"
                            + "    =%simplified=;\n"
                            + "    1000>: <%%alt-hundreds<[ >>];\n"
                            + "    10,000: =%simplified=;\n"
                            + "    1,000,000: << million[ >%simplified>];\n"
                            + "    1,000,000,000,000: << billion[ >%simplified>];\n"
                            + "    1,000,000,000,000,000: =#,##0=;\n"
                            + "%%alt-hundreds:\n"
                            + "    0: SHOULD NEVER GET HERE!;\n"
                            + "    10: <%simplified< thousand;\n"
                            + "    11: =%simplified= hundred>%%empty>;\n"
                            + "%%empty:\n"
                            + "    0:;"
                            + "%ordinal:\n"
                            + "    zeroth; first; second; third; fourth; fifth; sixth; seventh;\n"
                            + "        eighth; ninth;\n"
                            + "    tenth; eleventh; twelfth; thirteenth; fourteenth;\n"
                            + "        fifteenth; sixteenth; seventeenth; eighteenth;\n"
                            + "        nineteenth;\n"
                            + "    twentieth; twenty->>;\n"
                            + "    30: thirtieth; thirty->>;\n"
                            + "    40: fortieth; forty->>;\n"
                            + "    50: fiftieth; fifty->>;\n"
                            + "    60: sixtieth; sixty->>;\n"
                            + "    70: seventieth; seventy->>;\n"
                            + "    80: eightieth; eighty->>;\n"
                            + "    90: ninetieth; ninety->>;\n"
                            + "    100: <%simplified< hundredth; <%simplified< hundred >>;\n"
                            + "    1000: <%simplified< thousandth; <%simplified< thousand >>;\n"
                            + "    1,000,000: <%simplified< millionth; <%simplified< million >>;\n"
                            + "    1,000,000,000,000: <%simplified< billionth;\n"
                            + "        <%simplified< billion >>;\n"
                            + "    1,000,000,000,000,000: =#,##0=;"
                            + "%default:\n"
                            + "    -x: minus >>;\n"
                            + "    x.x: << point >>;\n"
                            + "    =%simplified=;\n"
                            + "    100: << hundred[ >%%and>];\n"
                            + "    1000: << thousand[ >%%and>];\n"
                            + "    100,000>>: << thousand[>%%commas>];\n"
                            + "    1,000,000: << million[>%%commas>];\n"
                            + "    1,000,000,000,000: << billion[>%%commas>];\n"
                            + "    1,000,000,000,000,000: =#,##0=;\n"
                            + "%%and:\n"
                            + "    and =%default=;\n"
                            + "    100: =%default=;\n"
                            + "%%commas:\n"
                            + "    ' and =%default=;\n"
                            + "    100: , =%default=;\n"
                            + "    1000: , <%default< thousand, >%default>;\n"
                            + "    1,000,000: , =%default=;"
                            + "%%lenient-parse:\n"
                            + "    & ' ' , ',' ;\n";
            UCultureInfo.CurrentCulture = new UCultureInfo("en-US");
            string[][] localizations = new string[][] {
                /* public rule sets*/
                    new string[] {"%simplified", "%default", "%ordinal"},
                /* display names in "en_US" locale*/
                    new string[] {"en_US", "Simplified", "Default", "Ordinal"},
                /* display names in "zh_Hans" locale*/
                    new string[] {"zh_Hans", "\u7B80\u5316", "\u7F3A\u7701",  "\u5E8F\u5217"},
                /* display names in a fake locale*/
                    new string[] {"foo_Bar_BAZ", "Simplified", "Default", "Ordinal"}
            };

            //Construct RuleBasedNumberFormat by rule sets and localizations list
            RuleBasedNumberFormat formatter
                    = new RuleBasedNumberFormat(ukEnglish, localizations, new UCultureInfo("en-US"));
            RuleBasedNumberFormat f2 = new RuleBasedNumberFormat(ukEnglish, localizations);
            assertTrue("Check the two formatters' equality", formatter.Equals(f2));

            //get displayName by name
            String[] ruleSetNames = formatter.GetRuleSetNames();
            for (int i = 0; i < ruleSetNames.Length; i++)
            {
                Logln("Rule set name: " + ruleSetNames[i]);
                String RSName_defLoc = formatter.GetRuleSetDisplayName(ruleSetNames[i]);
                assertEquals($"Display name in default locale ({UCultureInfo.CurrentUICulture.FullName}) for index {i}.", localizations[1][i + 1], RSName_defLoc);
                String RSName_loc = formatter.GetRuleSetDisplayName(ruleSetNames[i], new UCultureInfo("zh_Hans_CN"));
                assertEquals($"Display name in Chinese for index {i}.", localizations[2][i + 1], RSName_loc);
            }

            // getDefaultRuleSetName
            String defaultRS = formatter.DefaultRuleSetName;
            //you know that the default rule set is %simplified according to rule sets string ukEnglish
            assertEquals("getDefaultRuleSetName", "%simplified", defaultRS);

            //get locales of localizations
            UCultureInfo[] locales = formatter.GetRuleSetDisplayNameLocales();
            for (int i = 0; i < locales.Length; i++)
            {
                Logln(locales[i].FullName);
            }

            //get displayNames
            String[] RSNames_defLoc = formatter.GetRuleSetDisplayNames();
            for (int i = 0; i < RSNames_defLoc.Length; i++)
            {
                assertEquals("getRuleSetDisplayNames in default locale", localizations[1][i + 1], RSNames_defLoc[i]);
            }

            String[] RSNames_loc = formatter.GetRuleSetDisplayNames(new UCultureInfo("en_GB"));
            for (int i = 0; i < RSNames_loc.Length; i++)
            {
                assertEquals("getRuleSetDisplayNames in English", localizations[1][i + 1], RSNames_loc[i]);
            }

            RSNames_loc = formatter.GetRuleSetDisplayNames(new UCultureInfo("zh_Hans_CN"));
            for (int i = 0; i < RSNames_loc.Length; i++)
            {
                assertEquals("getRuleSetDisplayNames in Chinese", localizations[2][i + 1], RSNames_loc[i]);
            }

            RSNames_loc = formatter.GetRuleSetDisplayNames(new UCultureInfo("foo_Bar_BAZ"));
            for (int i = 0; i < RSNames_loc.Length; i++)
            {
                assertEquals("getRuleSetDisplayNames in fake locale", localizations[3][i + 1], RSNames_loc[i]);
            }
        }

        [Test]
        public void TestAllLocales()
        {
            StringBuilder errors = new StringBuilder();
            String[] names = {
                " (spellout) ",
                " (ordinal) "
                //" (duration) " // English only
            };
            double[] numbers = { 45.678, 1, 2, 10, 11, 100, 110, 200, 1000, 1111, -1111 };
            int count = numbers.Length;
            Random r = (count <= numbers.Length ? null : CreateRandom());

            foreach (UCultureInfo loc in NumberFormat.GetUCultures(UCultureTypes.AllCultures))
            {
                for (int j = 0; j < names.Length; ++j)
                {
                    RuleBasedNumberFormat fmt = new RuleBasedNumberFormat(loc, (NumberPresentation)j + 1);
                    if (!loc.Equals(fmt.ActualCulture))
                    {
                        // Skip the redundancy
                        break;
                    }

                    for (int c = 0; c < count; c++)
                    {
                        double n;
                        if (c < numbers.Length)
                        {
                            n = numbers[c];
                        }
                        else
                        {
                            n = (r.Next(10000) - 3000) / 16d;
                        }

                        String s = fmt.Format(n);
                        if (IsVerbose())
                        {
                            Logln(loc.FullName + names[j] + "success format: " + n + " -> " + s);
                        }

                        try
                        {
                            // RBNF parse is extremely slow when lenient option is enabled.
                            // non-lenient parse
                            fmt.LenientParseEnabled = false; ;
                            Number num = fmt.Parse(s);
                            if (IsVerbose())
                            {
                                Logln(loc.FullName + names[j] + "success parse: " + s + " -> " + num);
                            }
                            if (j != 0)
                            {
                                // TODO: Fix the ordinal rules.
                                continue;
                            }
                            if (n != num.ToDouble())
                            {
                                errors.Append("\n" + loc + names[j] + "got " + num + " expected " + n);
                            }
                        }
                        //catch (ParseException pe)
                        catch (FormatException pe)
                        {
                            String msg = loc.FullName + names[j] + "ERROR:" + pe.ToString();
                            Logln(msg);
                            errors.Append("\n" + msg);
                        }
                    }
                }
            }
            if (errors.Length > 0)
            {
                Errln(errors.ToString());
            }
        }

        void doTest(RuleBasedNumberFormat formatter, string[][] testData,
                    bool testParsing)
        {
            //        NumberFormat decFmt = NumberFormat.getInstance(Locale.US);
            NumberFormat decFmt = new DecimalFormat("#,###.################");
            try
            {
                for (int i = 0; i < testData.Length; i++)
                {
                    String number = testData[i][0];
                    String expectedWords = testData[i][1];
                    if (IsVerbose())
                    {
                        Logln("test[" + i + "] number: " + number + " target: " + expectedWords);
                    }
                    Number num = decFmt.Parse(number);
                    String actualWords = formatter.Format(num);

                    if (!actualWords.Equals(expectedWords))
                    {
                        Errln("Spot check format failed: for " + number + ", expected\n    "
                                + expectedWords + ", but got\n    " +
                                actualWords);
                    }
                    else if (testParsing)
                    {
                        String actualNumber = decFmt.Format(formatter
                                .Parse(actualWords));

                        if (!actualNumber.Equals(number))
                        {
                            Errln("Spot check parse failed: for " + actualWords +
                                    ", expected " + number + ", but got " +
                                    actualNumber);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Errln("Test failed with exception: " + e.ToString());
                Console.WriteLine(e.ToString());
                //e.printStackTrace();
                //Errln("Test failed with exception: " + e.toString());
            }
        }

        /* Tests the method
         *      public boolean equals(Object that)
         */
        [Test]
        public void TestEquals()
        {
            // Tests when "if (!(that instanceof RuleBasedNumberFormat))" is true
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat("dummy");
            if (rbnf.Equals("dummy") ||
                    rbnf.Equals('a') ||
                    rbnf.Equals(new Object()) ||
                    rbnf.Equals(-1) ||
                    rbnf.Equals(0) ||
                    rbnf.Equals(1) ||
                    rbnf.Equals(-1.0) ||
                    rbnf.Equals(0.0) ||
                    rbnf.Equals(1.0))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was suppose to " +
                        "be false for an invalid object.");
            }

            // Tests when
            // "if (!locale.equals(that2.locale) || lenientParse != that2.lenientParse)"
            // is true
            RuleBasedNumberFormat rbnf1 = new RuleBasedNumberFormat("dummy", new CultureInfo("en"));
            RuleBasedNumberFormat rbnf2 = new RuleBasedNumberFormat("dummy", new CultureInfo("jp"));
            RuleBasedNumberFormat rbnf3 = new RuleBasedNumberFormat("dummy", new CultureInfo("sp"));
            RuleBasedNumberFormat rbnf4 = new RuleBasedNumberFormat("dummy", new CultureInfo("fr"));

            if (rbnf1.Equals(rbnf2) || rbnf1.Equals(rbnf3) ||
                    rbnf1.Equals(rbnf4) || rbnf2.Equals(rbnf3) ||
                    rbnf2.Equals(rbnf4) || rbnf3.Equals(rbnf4))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was suppose to " +
                        "be false for an invalid object.");
            }

            if (!rbnf1.Equals(rbnf1))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was not suppose to " +
                        "be false for an invalid object.");
            }

            if (!rbnf2.Equals(rbnf2))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was not suppose to " +
                        "be false for an invalid object.");
            }

            if (!rbnf3.Equals(rbnf3))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was not suppose to " +
                        "be false for an invalid object.");
            }

            if (!rbnf4.Equals(rbnf4))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was not suppose to " +
                        "be false for an invalid object.");
            }

            RuleBasedNumberFormat rbnf5 = new RuleBasedNumberFormat("dummy", new CultureInfo("en"));
            RuleBasedNumberFormat rbnf6 = new RuleBasedNumberFormat("dummy", new CultureInfo("en"));

            if (!rbnf5.Equals(rbnf6))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was not suppose to " +
                        "be false for an invalid object.");
            }
            rbnf6.LenientParseEnabled = true;

            if (rbnf5.Equals(rbnf6))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was suppose to " +
                        "be false for an invalid object.");
            }

            // Tests when "if (!ruleSets[i].equals(that2.ruleSets[i]))" is true
            RuleBasedNumberFormat rbnf7 = new RuleBasedNumberFormat("not_dummy", new CultureInfo("en"));
            if (rbnf5.Equals(rbnf7))
            {
                Errln("RuleBasedNumberFormat.equals(Object that) was suppose to " +
                        "be false for an invalid object.");
            }
        }

        /* Tests the method
         *      public ULocale[] getRuleSetDisplayNameLocales()
         */
        [Test]
        public void TestGetRuleDisplayNameLocales()
        {
            // Tests when "if (ruleSetDisplayNames != null" is false
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat("dummy");
            rbnf.GetRuleSetDisplayNameLocales();
            if (rbnf.GetRuleSetDisplayNameLocales() != null)
            {
                Errln("RuleBasedNumberFormat.getRuleDisplayNameLocales() was suppose to " +
                        "return null.");
            }
        }

        /* Tests the method
         *      private String[] getNameListForLocale(ULocale loc)
         *      public String[] getRuleSetDisplayNames(ULocale loc)
         */
        [Test]
        public void TestGetNameListForLocale()
        {
            // Tests when "if (names != null)" is false and
            //  "if (loc != null && ruleSetDisplayNames != null)" is false
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat("dummy");
            rbnf.GetRuleSetDisplayNames(null);
            try
            {
                rbnf.GetRuleSetDisplayNames(null);
            }
            catch (Exception e)
            {
                Errln("RuleBasedNumberFormat.getRuleSetDisplayNames(ULocale loc) " +
                        "was not suppose to have an exception.");
            }
        }

        /* Tests the method
         *      public String getRuleSetDisplayName(String ruleSetName, ULocale loc)
         */
        [Test]
        public void TestGetRulesSetDisplayName()
        {
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat("dummy");
            //rbnf.getRuleSetDisplayName("dummy", new ULocale("en_US"));

            // Tests when "if (names != null) " is true

            // Tests when the method throws an exception
            try
            {
                rbnf.GetRuleSetDisplayName("", new UCultureInfo("en_US"));
                Errln("RuleBasedNumberFormat.getRuleSetDisplayName(String ruleSetName, ULocale loc) " +
                        "was suppose to have an exception.");
            }
            catch (Exception ignored) { }

            try
            {
                rbnf.GetRuleSetDisplayName("dummy", new UCultureInfo("en_US"));
                Errln("RuleBasedNumberFormat.getRuleSetDisplayName(String ruleSetName, ULocale loc) " +
                        "was suppose to have an exception.");
            }
            catch (Exception ignored) { }
        }

        /* Test the method
         *      public void process(StringBuffer buf, NFRuleSet ruleSet)
         */
        [Test]
        public void TestChineseProcess()
        {
            String ruleWithChinese =
                "%simplified:\n"
                + "    -x: minus >>;\n"
                + "    x.x: << point >>;\n"
                + "    zero; one; two; three; four; five; six; seven; eight; nine;\n"
                + "    ten; eleven; twelve; thirteen; fourteen; fifteen; sixteen;\n"
                + "        seventeen; eighteen; nineteen;\n"
                + "    20: twenty[->>];\n"
                + "    30: thirty[->>];\n"
                + "    40: forty[->>];\n"
                + "    50: fifty[->>];\n"
                + "    60: sixty[->>];\n"
                + "    70: seventy[->>];\n"
                + "    80: eighty[->>];\n"
                + "    90: ninety[->>];\n"
                + "    100: << hundred[ >>];\n"
                + "    1000: << thousand[ >>];\n"
                + "    1,000,000: << million[ >>];\n"
                + "    1,000,000,000,000: << billion[ >>];\n"
                + "    1,000,000,000,000,000: =#,##0=;\n"
                + "%alt-teens:\n"
                + "    =%simplified=;\n"
                + "    1000>: <%%alt-hundreds<[ >>];\n"
                + "    10,000: =%simplified=;\n"
                + "    1,000,000: << million[ >%simplified>];\n"
                + "    1,000,000,000,000: << billion[ >%simplified>];\n"
                + "    1,000,000,000,000,000: =#,##0=;\n"
                + "%%alt-hundreds:\n"
                + "    0: SHOULD NEVER GET HERE!;\n"
                + "    10: <%simplified< thousand;\n"
                + "    11: =%simplified= hundred>%%empty>;\n"
                + "%%empty:\n"
                + "    0:;"
                + "%accounting:\n"
                + "    \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c;\n"
                + "        \u842c; \u842c;\n"
                + "    \u842c; \u842c; \u842c; \u842c; \u842c;\n"
                + "        \u842c; \u842c; \u842c; \u842c;\n"
                + "        \u842c;\n"
                + "    twentieth; \u96f6|>>;\n"
                + "    30: \u96f6; \u96f6|>>;\n"
                + "    40: \u96f6; \u96f6|>>;\n"
                + "    50: \u96f6; \u96f6|>>;\n"
                + "    60: \u96f6; \u96f6|>>;\n"
                + "    70: \u96f6; \u96f6|>>;\n"
                + "    80: \u96f6; \u96f6|>>;\n"
                + "    90: \u96f6; \u96f6|>>;\n"
                + "    100: <%simplified< \u96f6; <%simplified< \u96f6 >>;\n"
                + "    1000: <%simplified< \u96f6; <%simplified< \u96f6 >>;\n"
                + "    1,000,000: <%simplified< \u96f6; <%simplified< \u96f6 >>;\n"
                + "    1,000,000,000,000: <%simplified< \u96f6;\n"
                + "        <%simplified< \u96f6 >>;\n"
                + "    1,000,000,000,000,000: =#,##0=;"
                + "%default:\n"
                + "    -x: minus >>;\n"
                + "    x.x: << point >>;\n"
                + "    =%simplified=;\n"
                + "    100: << hundred[ >%%and>];\n"
                + "    1000: << thousand[ >%%and>];\n"
                + "    100,000>>: << thousand[>%%commas>];\n"
                + "    1,000,000: << million[>%%commas>];\n"
                + "    1,000,000,000,000: << billion[>%%commas>];\n"
                + "    1,000,000,000,000,000: =#,##0=;\n"
                + "%%and:\n"
                + "    and =%default=;\n"
                + "    100: =%default=;\n"
                + "%%commas:\n"
                + "    ' and =%default=;\n"
                + "    100: , =%default=;\n"
                + "    1000: , <%default< thousand, >%default>;\n"
                + "    1,000,000: , =%default=;"
                + "%traditional:\n"
                + "    -x: \u3007| >>;\n"
                + "    x.x: << \u9ede >>;\n"
                + "    \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c;\n"
                + "    \u842c; \u842c; \u842c; \u842c; \u842c; \u842c; \u842c;\n"
                + "        \u842c; \u842c; \u842c;\n"
                + "    20: \u842c[->>];\n"
                + "    30: \u842c[->>];\n"
                + "    40: \u842c[->>];\n"
                + "    50: \u842c[->>];\n"
                + "    60: \u842c[->>];\n"
                + "    70: \u842c[->>];\n"
                + "    80: \u842c[->>];\n"
                + "    90: \u842c[->>];\n"
                + "    100: << \u842c[ >>];\n"
                + "    1000: << \u842c[ >>];\n"
                + "    1,000,000: << \u842c[ >>];\n"
                + "    1,000,000,000,000: << \u842c[ >>];\n"
                + "    1,000,000,000,000,000: =#,##0=;\n"
                + "%time:\n"
                + "    =0= sec.;\n"
                + "    60: =%%min-sec=;\n"
                + "    3600: =%%hr-min-sec=;\n"
                + "%%min-sec:\n"
                + "    0: *=00=;\n"
                + "    60/60: <0<>>;\n"
                + "%%hr-min-sec:\n"
                + "    0: *=00=;\n"
                + "    60/60: <00<>>;\n"
                + "    3600/60: <#,##0<:>>>;\n"
                + "%%post-process:ICU4N.Text.RbnfChinesePostProcessor\n";
            //+ "%%post-process:com.ibm.icu.text.RBNFChinesePostProcessor\n";

            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(ruleWithChinese, new UCultureInfo("zh"));
            String[] ruleNames = rbnf.GetRuleSetNames();
            try
            {
                // Test with "null" rules
                rbnf.Format(0.0, null);
                Errln("This was suppose to return an exception for a null format");
            }
            catch (Exception e) when (!(e is AssertionException)) { }
            for (int i = 0; i < ruleNames.Length; i++)
            {
                try
                {
                    rbnf.Format(-123450.6789, ruleNames[i]);
                }
                catch (Exception e)
                {
                    Errln("RBNFChinesePostProcessor was not suppose to return an exception " +
                            "when being formatted with parameters 0.0 and " + ruleNames[i]);
                }
            }
        }

        [Test]
        public void TestSetDecimalFormatSymbols()
        {
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(new CultureInfo("en"), NumberPresentation.Ordinal);

            DecimalFormatSymbols dfs = new DecimalFormatSymbols(new CultureInfo("en"));

            double number = 1001;

            String[] expected = { "1,001st", "1&001st" };

            String result = rbnf.Format(number);
            if (!result.Equals(expected[0]))
            {
                Errln("Format Error - Got: " + result + " Expected: " + expected[0]);
            }

            /* Set new symbol for testing */
            dfs.GroupingSeparator = ('&');
            rbnf.SetDecimalFormatSymbols(dfs);

            result = rbnf.Format(number);
            if (!result.Equals(expected[1]))
            {
                Errln("Format Error - Got: " + result + " Expected: " + expected[1]);
            }
        }

        class TextContextItem
        {
            public String locale;
            public NumberPresentation format;
            public DisplayContext context;
            public double value;
            public String expectedResult;
            // Simple constructor
            public TextContextItem(String loc, NumberPresentation fmt, DisplayContext ctxt, double val, String expRes)
            {
                locale = loc;
                format = fmt;
                context = ctxt;
                value = val;
                expectedResult = expRes;
            }
        }

        [Test]
        public void TestContext()
        {

            TextContextItem[] items = new TextContextItem[] {
                new TextContextItem( "sv", NumberPresentation.SpellOut, DisplayContext.CapitalizationForMiddleOfSentence,     123.45, "ett\u00ADhundra\u00ADtjugo\u00ADtre komma fyra fem" ),
                new TextContextItem( "sv", NumberPresentation.SpellOut, DisplayContext.CapitalizationForBeginningOfSentence,  123.45, "Ett\u00ADhundra\u00ADtjugo\u00ADtre komma fyra fem" ),
                new TextContextItem( "sv", NumberPresentation.SpellOut, DisplayContext.CapitalizationForUIListOrMenu,         123.45, "ett\u00ADhundra\u00ADtjugo\u00ADtre komma fyra fem" ),
                new TextContextItem( "sv", NumberPresentation.SpellOut, DisplayContext.CapitalizationForStandalone,           123.45, "ett\u00ADhundra\u00ADtjugo\u00ADtre komma fyra fem" ),
                new TextContextItem( "en", NumberPresentation.SpellOut, DisplayContext.CapitalizationForMiddleOfSentence,     123.45, "one hundred twenty-three point four five" ),
                new TextContextItem( "en", NumberPresentation.SpellOut, DisplayContext.CapitalizationForBeginningOfSentence,  123.45, "One hundred twenty-three point four five" ),
                new TextContextItem( "en", NumberPresentation.SpellOut, DisplayContext.CapitalizationForUIListOrMenu,         123.45, "One hundred twenty-three point four five" ),
                new TextContextItem( "en", NumberPresentation.SpellOut, DisplayContext.CapitalizationForStandalone,           123.45, "One hundred twenty-three point four five" ),
            };
            foreach (TextContextItem item in items)
            {
                UCultureInfo locale = new UCultureInfo(item.locale);
                RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(locale, item.format);
                rbnf.SetContext(item.context);
                String result = rbnf.Format(item.value, rbnf.DefaultRuleSetName);
                if (!result.Equals(item.expectedResult))
                {
                    Errln("Error for locale " + item.locale + ", context " + item.context + ", expected " + item.expectedResult + ", got " + result);
                }
                RuleBasedNumberFormat rbnfClone = (RuleBasedNumberFormat)rbnf.Clone();
                if (!rbnfClone.Equals(rbnf))
                {
                    Errln("Error for locale " + item.locale + ", context " + item.context + ", rbnf.clone() != rbnf");
                }
                else
                {
                    result = rbnfClone.Format(item.value, rbnfClone.DefaultRuleSetName);
                    if (!result.Equals(item.expectedResult))
                    {
                        Errln("Error with clone for locale " + item.locale + ", context " + item.context + ", expected " + item.expectedResult + ", got " + result);
                    }
                }
            }
        }

        [Test]
        public void TestInfinityNaN()
        {
            String enRules = "%default:"
                    + "-x: minus >>;"
                    + "Inf: infinite;"
                    + "NaN: not a number;"
                    + "0: =#,##0=;";
            RuleBasedNumberFormat enFormatter = new RuleBasedNumberFormat(enRules, new UCultureInfo("en"));
            string[][] enTestData = new string[][] {
                new string[] {"1", "1"},
                new string[] {"\u221E", "infinite"},
                new string[] {"-\u221E", "minus infinite"},
                new string[] {"NaN", "not a number"},

            };

            doTest(enFormatter, enTestData, true);

            // Test the default behavior when the rules are undefined.
            enRules = "%default:"
                    + "-x: ->>;"
                    + "0: =#,##0=;";
            enFormatter = new RuleBasedNumberFormat(enRules, new UCultureInfo("en"));
            string[][] enDefaultTestData = new string[][] {
                new string[] {"1", "1"},
                new string[] {"\u221E", "∞"},
                new string[] {"-\u221E", "-∞"},
                new string[] {"NaN", "NaN"},

            };

            doTest(enFormatter, enDefaultTestData, true);
        }

        [Test]
        public void TestVariableDecimalPoint()
        {
            String enRules = "%spellout-numbering:"
                    + "-x: minus >>;"
                    + "x.x: << point >>;"
                    + "x,x: << comma >>;"
                    + "0.x: xpoint >>;"
                    + "0,x: xcomma >>;"
                    + "0: zero;"
                    + "1: one;"
                    + "2: two;"
                    + "3: three;"
                    + "4: four;"
                    + "5: five;"
                    + "6: six;"
                    + "7: seven;"
                    + "8: eight;"
                    + "9: nine;";
            RuleBasedNumberFormat enFormatter = new RuleBasedNumberFormat(enRules, new UCultureInfo("en"));
            string[][] enTestPointData = new string[][] {
                        new string[] {"1.1", "one point one"},
                        new string[] {"1.23", "one point two three"},
                        new string[] {"0.4", "xpoint four"},
                };
            doTest(enFormatter, enTestPointData, true);
            DecimalFormatSymbols decimalFormatSymbols = new DecimalFormatSymbols(new UCultureInfo("en"));
            decimalFormatSymbols.DecimalSeparator = (',');
            enFormatter.SetDecimalFormatSymbols(decimalFormatSymbols);
            string[][] enTestCommaData = new string[][] {
                        new string[] {"1.1", "one comma one"},
                        new string[] {"1.23", "one comma two three"},
                        new string[] {"0.4", "xcomma four"},
                };
            doTest(enFormatter, enTestCommaData, true);
        }

        [Test]
        public void TestRounding()
        {
            RuleBasedNumberFormat enFormatter = new RuleBasedNumberFormat(new UCultureInfo("en"), NumberPresentation.SpellOut);
            string[][] enTestFullData = new string[][] {
                new string[] {"0", "zero"},
                new string[] {"0.4", "zero point four"},
                new string[] {"0.49", "zero point four nine"},
                new string[] {"0.5", "zero point five"},
                new string[] {"0.51", "zero point five one"},
                new string[] {"0.99", "zero point nine nine"},
                new string[] {"1", "one"},
                new string[] {"1.01", "one point zero one"},
                new string[] {"1.49", "one point four nine"},
                new string[] {"1.5", "one point five"},
                new string[] {"1.51", "one point five one"},
                new string[] {"450359962737049.6", "four hundred fifty trillion three hundred fifty-nine billion nine hundred sixty-two million seven hundred thirty-seven thousand forty-nine point six"}, // 2^52 / 10
                new string[] {"450359962737049.7", "four hundred fifty trillion three hundred fifty-nine billion nine hundred sixty-two million seven hundred thirty-seven thousand forty-nine point seven"}, // 2^52 + 1 / 10
            };
            doTest(enFormatter, enTestFullData, false);

            enFormatter.MaximumFractionDigits = 0;
            enFormatter.RoundingMode = (BigDecimal.RoundHalfEven).ToRoundingMode();
            string[][] enTestIntegerData = new string[][] {
                new string[] {"0", "zero"},
                new string[] {"0.4", "zero"},
                new string[] {"0.49", "zero"},
                new string[] {"0.5", "zero"},
                new string[] {"0.51", "one"},
                new string[] {"0.99", "one"},
                new string[] {"1", "one"},
                new string[] {"1.01", "one"},
                new string[] {"1.49", "one"},
                new string[] {"1.5", "two"},
                new string[] {"1.51", "two"},
            };
            doTest(enFormatter, enTestIntegerData, false);

            enFormatter.MaximumFractionDigits = 1;
            enFormatter.RoundingMode = (BigDecimal.RoundHalfEven).ToRoundingMode();
            string[][] enTestTwoDigitsData = new string[][] {
                new string[] {"0", "zero"},
                new string[] {"0.04", "zero"},
                new string[] {"0.049", "zero"},
                new string[] {"0.05", "zero"},
                new string[] {"0.051", "zero point one"},
                new string[] {"0.099", "zero point one"},
                new string[] {"10.11", "ten point one"},
                new string[] {"10.149", "ten point one"},
                new string[] {"10.15", "ten point two"},
                new string[] {"10.151", "ten point two"},
            };
            doTest(enFormatter, enTestTwoDigitsData, false);

            enFormatter.MaximumFractionDigits = 3;
            enFormatter.RoundingMode = (BigDecimal.RoundDown).ToRoundingMode();
            string[][] enTestThreeDigitsDownData = new string[][] {
                new string[] {"4.3", "four point three"}, // Not 4.299!
            };
            doTest(enFormatter, enTestThreeDigitsDownData, false);
        }

        [Test]
        public void TestLargeNumbers()
        {
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(new UCultureInfo("en-US"), NumberPresentation.SpellOut);

            string[][] enTestFullData = new string[][] {
                new string[] {"-9007199254740991", "minus nine quadrillion seven trillion one hundred ninety-nine billion two hundred fifty-four million seven hundred forty thousand nine hundred ninety-one"}, // Maximum precision in both a double and a long
                new string[] {"9007199254740991", "nine quadrillion seven trillion one hundred ninety-nine billion two hundred fifty-four million seven hundred forty thousand nine hundred ninety-one"}, // Maximum precision in both a double and a long
                new string[] {"-9007199254740992", "minus nine quadrillion seven trillion one hundred ninety-nine billion two hundred fifty-four million seven hundred forty thousand nine hundred ninety-two"}, // Only precisely contained in a long
                new string[] {"9007199254740992", "nine quadrillion seven trillion one hundred ninety-nine billion two hundred fifty-four million seven hundred forty thousand nine hundred ninety-two"}, // Only precisely contained in a long
                new string[] {"9999999999999998", "nine quadrillion nine hundred ninety-nine trillion nine hundred ninety-nine billion nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-eight"},
                new string[] {"9999999999999999", "nine quadrillion nine hundred ninety-nine trillion nine hundred ninety-nine billion nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-nine"},
                new string[] {"999999999999999999", "nine hundred ninety-nine quadrillion nine hundred ninety-nine trillion nine hundred ninety-nine billion nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-nine"},
                new string[] {"1000000000000000000", "1,000,000,000,000,000,000"}, // The rules don't go to 1 quintillion yet
                new string[] {"-9223372036854775809", "-9,223,372,036,854,775,809"}, // We've gone beyond 64-bit precision
                new string[] {"-9223372036854775808", "-9,223,372,036,854,775,808"}, // We've gone beyond +64-bit precision
                new string[] {"-9223372036854775807", "minus 9,223,372,036,854,775,807"}, // Minimum 64-bit precision
                new string[] {"-9223372036854775806", "minus 9,223,372,036,854,775,806"}, // Minimum 64-bit precision + 1
                new string[] {"9223372036854774111", "9,223,372,036,854,774,111"}, // Below 64-bit precision
                new string[] {"9223372036854774999", "9,223,372,036,854,774,999"}, // Below 64-bit precision
                new string[] {"9223372036854775000", "9,223,372,036,854,775,000"}, // Below 64-bit precision
                new string[] {"9223372036854775806", "9,223,372,036,854,775,806"}, // Maximum 64-bit precision - 1
                new string[] {"9223372036854775807", "9,223,372,036,854,775,807"}, // Maximum 64-bit precision
                new string[] {"9223372036854775808", "9,223,372,036,854,775,808"}, // We've gone beyond 64-bit precision. This can only be represented with BigDecimal.
            };
            doTest(rbnf, enTestFullData, false);
        }

        [Test]
        public void TestCompactDecimalFormatStyle()
        {
            // This is not a common use case, but we're testing it anyway.
            string numberPattern = "=###0.#####=;"
                    + "1000: <###0.00< K;"
                    + "1000000: <###0.00< M;"
                    + "1000000000: <###0.00< B;"
                    + "1000000000000: <###0.00< T;"
                    + "1000000000000000: <###0.00< Q;";
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(numberPattern, new UCultureInfo("en_US"));

            string[][] enTestFullData = new string[][] {
                new string[] {"1000", "1.00 K"},
                new string[] {"1234", "1.23 K"},
                new string[] {"999994", "999.99 K"},
                new string[] {"999995", "1000.00 K"},
                new string[] {"1000000", "1.00 M"},
                new string[] {"1200000", "1.20 M"},
                new string[] {"1200000000", "1.20 B"},
                new string[] {"1200000000000", "1.20 T"},
                new string[] {"1200000000000000", "1.20 Q"},
                new string[] {"4503599627370495", "4.50 Q"},
                new string[] {"4503599627370496", "4.50 Q"},
                new string[] {"8990000000000000", "8.99 Q"},
                new string[] {"9008000000000000", "9.00 Q"}, // Number doesn't precisely fit into a double
                new string[] {"9456000000000000", "9.00 Q"},  // Number doesn't precisely fit into a double
                new string[] {"10000000000000000", "10.00 Q"},  // Number doesn't precisely fit into a double
                new string[] {"9223372036854775807", "9223.00 Q"}, // Maximum 64-bit precision
                new string[] {"9223372036854775808", "9,223,372,036,854,775,808"}, // We've gone beyond 64-bit precision. This can only be represented with BigDecimal.
            };
            doTest(rbnf, enTestFullData, false);
        }

        private void assertEquals(String expected, String result)
        {
            if (!expected.Equals(result, StringComparison.Ordinal))
            {
                Errln("Expected: " + expected + " Got: " + result);
            }
        }

        [Test]
        public void TestRoundingUnrealNumbers()
        {
            RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(new UCultureInfo("en-US"), NumberPresentation.SpellOut);
            rbnf.RoundingMode = (BigDecimal.RoundHalfUp).ToRoundingMode();
            rbnf.MaximumFractionDigits = (3);
            assertEquals("zero point one", rbnf.Format(0.1));
            assertEquals("zero point zero zero one", rbnf.Format(0.0005));
            assertEquals("infinity", rbnf.Format(double.PositiveInfinity));
            assertEquals("not a number", rbnf.Format(double.NaN));
        }
    }
}
