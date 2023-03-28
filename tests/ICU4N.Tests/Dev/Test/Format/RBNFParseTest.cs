using ICU4N.Globalization;
using ICU4N.Text;
using J2N.Numerics;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Dev.Test.Format
{
    public class RBNFParseTest : TestFmwk
    {
        [Test]
        public void TestParse()
        {

            // these rules make no sense but behave rationally
            string[] okrules = {
                "random text",
                "%foo:bar",
                "%foo: bar",
                "0:",
                "0::",
                "%%foo:;",
                "-",
                "-1",
                "-:",
                ".",
                ".1",
                "[",
                "]",
                "[]",
                "[foo]",
                "[[]",
                "[]]",
                "[[]]",
                "[][]",
                "<",
                ">",
                "=",
                "==",
                "===",
                "=foo=",
            };

            string[] exceptrules = {
                "",
                ";",
                ";;",
                ":",
                "::",
                ":1",
                ":;",
                ":;:;",
                "<<",
                "<<<",
                "10:;9:;",
                ">>",
                ">>>",
                "10:", // formatting any value with a one's digit will fail
                "11: << x", // formating a multiple of 10 causes rollback rule to fail
                "%%foo: 0 foo; 10: =%%bar=; %%bar: 0: bar; 10: =%%foo=;",
            };

            string[][] allrules = {
                okrules,
                exceptrules,
            };

            for (int j = 0; j < allrules.Length; ++j)
            {
                string[] tests = allrules[j];
                bool except = tests == exceptrules;
                for (int i = 0; i < tests.Length; ++i)
                {
                    Logln("----------");
                    Logln("rules: '" + tests[i] + "'");
                    bool caughtException = false;
                    try
                    {
                        RuleBasedNumberFormat fmt = new RuleBasedNumberFormat(tests[i], new CultureInfo("en-US"));
                        Logln("1.23: " + fmt.Format(20));
                        Logln("-123: " + fmt.Format(-123));
                        Logln(".123: " + fmt.Format(.123));
                        Logln(" 123: " + fmt.Format(123));
                    }
                    catch (Exception e)
                    {
                        if (!except)
                        {
                            Errln("Unexpected exception: " + e.Message);
                        }
                        else
                        {
                            caughtException = true;
                        }
                    }
                    if (except && !caughtException)
                    {
                        Errln("expected exception but didn't get one!");
                    }
                }
            }
        }

        private void parseFormat(RuleBasedNumberFormat rbnf, string s, string target)
        {
            try
            {
                Number n = rbnf.Parse(s);
                string t = rbnf.Format(n);
                assertEquals(rbnf.ActualCulture + ": " + s + " : " + n, target, t);
            }
            catch (FormatException e)
            {
                fail("exception:" + e);
            }
        }

        private void parseList(RuleBasedNumberFormat rbnf_en, RuleBasedNumberFormat rbnf_fr, string[][] lists)
        {
            for (int i = 0; i < lists.Length; ++i)
            {
                string[] list = lists[i];
                string s = list[0];
                string target_en = list[1];
                string target_fr = list[2];

                parseFormat(rbnf_en, s, target_en);
                parseFormat(rbnf_fr, s, target_fr);
            }
        }

        [Test]
        public void TestLenientParse()
        {
            RuleBasedNumberFormat rbnf_en, rbnf_fr;

            // TODO: this still passes, but setLenientParseMode should have no effect now.
            // Did it ever test what it was supposed to?
            rbnf_en = new RuleBasedNumberFormat(new CultureInfo("en"), NumberPresentation.SpellOut);
            rbnf_en.LenientParseEnabled = (true);
            rbnf_fr = new RuleBasedNumberFormat(new CultureInfo("fr"), NumberPresentation.SpellOut);
            rbnf_fr.LenientParseEnabled = (true);

            Number n = rbnf_en.Parse("1,2 million");
            Logln(n.ToString());

            string[][] lists = {
                new string[] { "1,2", "twelve", "un virgule deux" },
                new string[] { "1,2 million", "twelve million", "un virgule deux" },
                new string[] { "1,2 millions", "twelve million", "un million deux cent mille" },
                new string[] { "1.2", "one point two", "douze" },
                new string[] { "1.2 million", "one million two hundred thousand", "douze" },
                new string[] { "1.2 millions", "one million two hundred thousand", "douze millions" },
            };

            //Locale.setDefault(Locale.FRANCE);
            base.CurrentCulture = new CultureInfo("fr-FR");
            Logln("Default locale:" + CultureInfo.CurrentCulture);
            Logln("rbnf_en:" + rbnf_en.DefaultRuleSetName);
            Logln("rbnf_fr:" + rbnf_en.DefaultRuleSetName);
            parseList(rbnf_en, rbnf_fr, lists);

            //Locale.setDefault(Locale.US);
            base.CurrentCulture = new CultureInfo("en-US");
            Logln("Default locale:" + CultureInfo.CurrentCulture);
            Logln("rbnf_en:" + rbnf_en.DefaultRuleSetName);
            Logln("rbnf_fr:" + rbnf_en.DefaultRuleSetName);
            parseList(rbnf_en, rbnf_fr, lists);
        }
    }
}
