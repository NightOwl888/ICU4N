using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StringBuffer = System.Text.StringBuilder;

/// <summary>
/// Port From:   ICU4C v2.1 : cintltest
/// Source File: $ICU4CRoot/source/test/cintltest/cmsccoll.c
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationMiscTest : TestFmwk
    {
        //private static final int NORM_BUFFER_TEST_LEN_ = 32;
        private sealed class Tester
        {
            internal int u;
            internal String NFC;
            internal String NFD;
        }

        private static bool hasCollationElements(CultureInfo locale)
        {
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuCollationBaseName, locale);
            if (rb != null)
            {
                try
                {
                    String collkey = rb.GetStringWithFallback("collations/default");
                    ICUResourceBundle elements = rb.GetWithFallback("collations/" + collkey);
                    if (elements != null)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                }
            }
            return false;
        }

        [Test]
        public void TestComposeDecompose()
        {
            Tester[] t = new Tester[0x30000];
            t[0] = new Tester();
            Logln("Testing UCA extensively\n");
            RuleBasedCollator coll;
            try
            {
                coll = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH */);
            }
            catch (Exception e)
            {
                Warnln("Error opening collator\n");
                return;
            }

            int noCases = 0;
            for (int u = 0; u < 0x30000; u++)
            {
                String comp = UTF16.ValueOf(u);
                int len = comp.Length;
                t[noCases].NFC = Normalizer.Normalize(u, NormalizerMode.NFC);
                t[noCases].NFD = Normalizer.Normalize(u, NormalizerMode.NFD);

                if (t[noCases].NFC.Length != t[noCases].NFD.Length
                    || (t[noCases].NFC.CompareToOrdinal(t[noCases].NFD) != 0)
                    || (len != t[noCases].NFD.Length)
                    || (comp.CompareToOrdinal(t[noCases].NFD) != 0))
                {
                    t[noCases].u = u;
                    if (len != t[noCases].NFD.Length
                        || (comp.CompareToOrdinal(t[noCases].NFD) != 0))
                    {
                        t[noCases].NFC = comp;
                    }
                    noCases++;
                    t[noCases] = new Tester();
                }
            }

            for (int u = 0; u < noCases; u++)
            {
                if (!coll.Equals(t[u].NFC, t[u].NFD))
                {
                    Errln("Failure: codePoint \\u" + (t[u].u).ToHexString()
                          + " fails TestComposeDecompose in the UCA");
                    CollationTest.DoTest(this, coll, t[u].NFC, t[u].NFD, 0);
                }
            }

            Logln("Testing locales, number of cases = " + noCases);
            CultureInfo[] loc = Collator.GetAvailableLocales();
            for (int i = 0; i < loc.Length; i++)
            {
                if (hasCollationElements(loc[i]))
                {
                    Logln("Testing locale " + loc[i].DisplayName);
                    coll = (RuleBasedCollator)Collator.GetInstance(loc[i]);
                    coll.Strength = (Collator.Identical);

                    for (int u = 0; u < noCases; u++)
                    {
                        if (!coll.Equals(t[u].NFC, t[u].NFD))
                        {
                            Errln("Failure: codePoint \\u"
                                  + (t[u].u).ToHexString()
                                  + " fails TestComposeDecompose for locale "
                                  + loc[i].DisplayName);
                            // this tests for the iterators too
                            CollationTest.DoTest(this, coll, t[u].NFC, t[u].NFD,
                                                 0);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestRuleOptions()
        {
            // values here are hardcoded and are correct for the current UCA when
            // the UCA changes, one might be forced to change these values.

            /*
             * These strings contain the last character before [variable top]
             * and the first and second characters (by primary weights) after it.
             * See FractionalUCA.txt. For example:
                [last variable [0C FE, 05, 05]] # U+10A7F OLD SOUTH ARABIAN NUMERIC INDICATOR
                [variable top = 0C FE]
                [first regular [0D 0A, 05, 05]] # U+0060 GRAVE ACCENT
               and
                00B4; [0D 0C, 05, 05]
             *
             * Note: Starting with UCA 6.0, the [variable top] collation element
             * is not the weight of any character or string,
             * which means that LAST_VARIABLE_CHAR_STRING sorts before [last variable].
             */
            String LAST_VARIABLE_CHAR_STRING = "\\U00010A7F";
            String FIRST_REGULAR_CHAR_STRING = "\\u0060";
            String SECOND_REGULAR_CHAR_STRING = "\\u00B4";

            /*
             * This string has to match the character that has the [last regular] weight
             * which changes with each UCA version.
             * See the bottom of FractionalUCA.txt which says something like
                [last regular [7A FE, 05, 05]] # U+1342E EGYPTIAN HIEROGLYPH AA032
             *
             * Note: Starting with UCA 6.0, the [last regular] collation element
             * is not the weight of any character or string,
             * which means that LAST_REGULAR_CHAR_STRING sorts before [last regular].
             */
            String LAST_REGULAR_CHAR_STRING = "\\U0001342E";

            String[] rules = {
                // cannot test this anymore, as [last primary ignorable] doesn't
                // have a  code point associated to it anymore
                // "&[before 3][last primary ignorable]<<<k",
                // - all befores here amount to zero
                /* "you cannot go before ...": The parser now sets an error for such nonsensical rules.
                "&[before 3][first tertiary ignorable]<<<a",
                "&[before 3][last tertiary ignorable]<<<a", */
                /*
                 * However, there is a real secondary ignorable (artificial addition in FractionalUCA.txt),
                 * and it *is* possible to "go before" that.
                 */
                "&[before 3][first secondary ignorable]<<<a",
                "&[before 3][last secondary ignorable]<<<a",
                // 'normal' befores
                /*
                 * Note: With a "SPACE first primary" boundary CE in FractionalUCA.txt,
                 * it is not possible to tailor &[first primary ignorable]<a or &[last primary ignorable]<a
                 * because there is no tailoring space before that boundary.
                 * Made the tests work by tailoring to a space instead.
                 */
                "&[before 3][first primary ignorable]<<<c<<<b &' '<a",  /* was &[first primary ignorable]<a */
                // we don't have a code point that corresponds to the last primary
                // ignorable
                "&[before 3][last primary ignorable]<<<c<<<b &' '<a",  /* was &[last primary ignorable]<a */
                "&[before 3][first variable]<<<c<<<b &[first variable]<a",
                "&[last variable]<a &[before 3][last variable]<<<c<<<b ",
                "&[first regular]<a &[before 1][first regular]<b",
                "&[before 1][last regular]<b &[last regular]<a",
                "&[before 1][first implicit]<b &[first implicit]<a",
                /* The current builder does not support tailoring to unassigned-implicit CEs (seems unnecessary, adds complexity).
                "&[before 1][last implicit]<b &[last implicit]<a", */
                "&[last variable]<z" +
                "&' '<x" +  /* was &[last primary ignorable]<x, see above */
                "&[last secondary ignorable]<<y&[last tertiary ignorable]<<<w&[top]<u",
            };
            String[][] data = {
                // {"k", "\u20e3"},
                /* "you cannot go before ...": The parser now sets an error for such nonsensical rules.
                new string[] {"\\u0000", "a"}, // you cannot go before first tertiary ignorable
                new string[] {"\\u0000", "a"}, // you cannot go before last tertiary ignorable */
                /*
                 * However, there is a real secondary ignorable (artificial addition in FractionalUCA.txt),
                 * and it *is* possible to "go before" that.
                 */
                new string[] {"\\u0000", "a"},
                new string[] {"\\u0000", "a"},
                /*
                 * Note: With a "SPACE first primary" boundary CE in FractionalUCA.txt,
                 * it is not possible to tailor &[first primary ignorable]<a or &[last primary ignorable]<a
                 * because there is no tailoring space before that boundary.
                 * Made the tests work by tailoring to a space instead.
                 */
                new string[] {"c", "b", "\\u0332", "a"},
                new string[] {"\\u0332", "\\u20e3", "c", "b", "a"},
                new string[] {"c", "b", "\\u0009", "a", "\\u000a"},
                new string[] {LAST_VARIABLE_CHAR_STRING, "c", "b", /* [last variable] */ "a", FIRST_REGULAR_CHAR_STRING},
                new string[] {"b", FIRST_REGULAR_CHAR_STRING, "a", SECOND_REGULAR_CHAR_STRING},
                // The character in the second ordering test string
                // has to match the character that has the [last regular] weight
                // which changes with each UCA version.
                // See the bottom of FractionalUCA.txt which says something like
                // [last regular [CE 27, 05, 05]] # U+1342E EGYPTIAN HIEROGLYPH AA032
                new string[] {LAST_REGULAR_CHAR_STRING, "b", /* [last regular] */ "a", "\\u4e00"},
                new string[] {"b", "\\u4e00", "a", "\\u4e01"},
                /* The current builder does not support tailoring to unassigned-implicit CEs (seems unnecessary, adds complexity).
                new string[] {"b", "\\U0010FFFD", "a"}, */
                new string[] {"\ufffb",  "w", "y", "\u20e3", "x", LAST_VARIABLE_CHAR_STRING, "z", "u"},
            };

            for (int i = 0; i < rules.Length; i++)
            {
                Logln(String.Format("rules[{0}] = \"{1}\"", i, rules[i]));
                genericRulesStarter(rules[i], data[i]);
            }
        }

        internal void genericRulesStarter(String rules, String[] s)
        {
            genericRulesStarterWithResult(rules, s, -1);
        }

        internal void genericRulesStarterWithResult(String rules, String[] s, int result)
        {

            RuleBasedCollator coll = null;
            try
            {
                coll = new RuleBasedCollator(rules);
                // Logln("Rules starter for " + rules);
                genericOrderingTestWithResult(coll, s, result);
            }
            catch (Exception e)
            {
                Warnln("Unable to open collator with rules " + rules + ": " + e);
            }
        }

        internal void genericRulesStarterWithOptionsAndResult(String rules, String[] s, String[] atts, Object[] attVals, int result)
        {
            RuleBasedCollator coll = null;
            try
            {
                coll = new RuleBasedCollator(rules);
                genericOptionsSetter(coll, atts, attVals);
                genericOrderingTestWithResult(coll, s, result);
            }
            catch (Exception e)
            {
                Warnln("Unable to open collator with rules " + rules);
            }
        }
        internal void genericOrderingTestWithResult(Collator coll, String[] s, int result)
        {
            String t1 = "";
            String t2 = "";

            for (int i = 0; i < s.Length - 1; i++)
            {
                for (int j = i + 1; j < s.Length; j++)
                {
                    t1 = Utility.Unescape(s[i]);
                    t2 = Utility.Unescape(s[j]);
                    // System.out.println(i + " " + j);
                    CollationTest.DoTest(this, (RuleBasedCollator)coll, t1, t2,
                                         result);
                }
            }
        }

        internal void reportCResult(String source, String target, CollationKey sourceKey, CollationKey targetKey,
                           int compareResult, int keyResult, int incResult, int expectedResult)
        {
            if (expectedResult < -1 || expectedResult > 1)
            {
                Errln("***** invalid call to reportCResult ****");
                return;
            }
            bool ok1 = (compareResult == expectedResult);
            bool ok2 = (keyResult == expectedResult);
            bool ok3 = (incResult == expectedResult);
            if (ok1 && ok2 && ok3 /* synwee to undo && !isVerbose()*/)
            {
                return;
            }
            else
            {
                String msg1 = ok1 ? "Ok: compare(\"" : "FAIL: compare(\"";
                String msg2 = "\", \"";
                String msg3 = "\") returned ";
                String msg4 = "; expected ";
                String sExpect = "";
                String sResult = "";
                sResult = CollationTest.AppendCompareResult(compareResult, sResult);
                sExpect = CollationTest.AppendCompareResult(expectedResult, sExpect);
                if (ok1)
                {
                    // Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
                msg1 = ok2 ? "Ok: key(\"" : "FAIL: key(\"";
                msg2 = "\").compareTo(key(\"";
                msg3 = "\")) returned ";
                sResult = CollationTest.AppendCompareResult(keyResult, sResult);
                if (ok2)
                {
                    // Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                    msg1 = "  ";
                    msg2 = " vs. ";
                    Errln(msg1 + CollationTest.Prettify(sourceKey) + msg2 + CollationTest.Prettify(targetKey));
                }
                msg1 = ok3 ? "Ok: incCompare(\"" : "FAIL: incCompare(\"";
                msg2 = "\", \"";
                msg3 = "\") returned ";
                sResult = CollationTest.AppendCompareResult(incResult, sResult);
                if (ok3)
                {
                    // Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
            }
        }

        [Test]
        public void TestBeforePrefixFailure()
        {
            String[] rules = {
                "&g <<< a&[before 3]\uff41 <<< x",
                "&\u30A7=\u30A7=\u3047=\uff6a&\u30A8=\u30A8=\u3048=\uff74&[before 3]\u30a7<<<\u30a9",
                "&[before 3]\u30a7<<<\u30a9&\u30A7=\u30A7=\u3047=\uff6a&\u30A8=\u30A8=\u3048=\uff74",
            };
            String[][] data = {
                new string[] {"x", "\uff41"},
                new string[] {"\u30a9", "\u30a7"},
                new string[] {"\u30a9", "\u30a7"},
            };

            for (int i = 0; i < rules.Length; i++)
            {
                genericRulesStarter(rules[i], data[i]);
            }
        }

        [Test]
        public void TestContractionClosure()
        {
            // Note: This was also ported to the data-driven test, see collationtest.txt.
            String[] rules = {
                "&b=\u00e4\u00e4",
                "&b=\u00C5",
            };
            String[][] data = {
                new string[] { "b", "\u00e4\u00e4", "a\u0308a\u0308", "\u00e4a\u0308", "a\u0308\u00e4" },
                new string[] { "b", "\u00C5", "A\u030A", "\u212B" },
            };

            for (int i = 0; i < rules.Length; i++)
            {
                genericRulesStarterWithResult(rules[i], data[i], 0);
            }
        }

        [Test]
        public void TestPrefixCompose()
        {
            String rule1 = "&\u30a7<<<\u30ab|\u30fc=\u30ac|\u30fc";

            String str = rule1;
            try
            {
                RuleBasedCollator coll = new RuleBasedCollator(str);
                Logln("rule:" + coll.GetRules());
            }
            catch (Exception e)
            {
                Warnln("Error open RuleBasedCollator rule = " + str);
            }
        }

        [Test]
        public void TestStrCollIdenticalPrefix()
        {
            String rule = "&\ud9b0\udc70=\ud9b0\udc71";
            String[] test = {
                "ab\ud9b0\udc70",
                "ab\ud9b0\udc71"
            };
            genericRulesStarterWithResult(rule, test, 0);
        }

        [Test]
        public void TestPrefix()
        {
            String[] rules = {
                "&z <<< z|a",
                "&z <<< z|   a",
                "[strength I]&a=\ud900\udc25&z<<<\ud900\udc25|a",
            };
            String[][] data = {
                new string[] {"zz", "za"},
                new string[] {"zz", "za"},
                new string[] {"aa", "az", "\ud900\udc25z", "\ud900\udc25a", "zz"},
            };

            for (int i = 0; i < rules.Length; i++)
            {
                genericRulesStarter(rules[i], data[i]);
            }
        }

        [Test]
        public void TestNewJapanese()
        {

            String[] test1 = {
                "\u30b7\u30e3\u30fc\u30ec",
                "\u30b7\u30e3\u30a4",
                "\u30b7\u30e4\u30a3",
                "\u30b7\u30e3\u30ec",
                "\u3061\u3087\u3053",
                "\u3061\u3088\u3053",
                "\u30c1\u30e7\u30b3\u30ec\u30fc\u30c8",
                "\u3066\u30fc\u305f",
                "\u30c6\u30fc\u30bf",
                "\u30c6\u30a7\u30bf",
                "\u3066\u3048\u305f",
                "\u3067\u30fc\u305f",
                "\u30c7\u30fc\u30bf",
                "\u30c7\u30a7\u30bf",
                "\u3067\u3048\u305f",
                "\u3066\u30fc\u305f\u30fc",
                "\u30c6\u30fc\u30bf\u30a1",
                "\u30c6\u30a7\u30bf\u30fc",
                "\u3066\u3047\u305f\u3041",
                "\u3066\u3048\u305f\u30fc",
                "\u3067\u30fc\u305f\u30fc",
                "\u30c7\u30fc\u30bf\u30a1",
                "\u3067\u30a7\u305f\u30a1",
                "\u30c7\u3047\u30bf\u3041",
                "\u30c7\u30a8\u30bf\u30a2",
                "\u3072\u3086",
                "\u3073\u3085\u3042",
                "\u3074\u3085\u3042",
                "\u3073\u3085\u3042\u30fc",
                "\u30d3\u30e5\u30a2\u30fc",
                "\u3074\u3085\u3042\u30fc",
                "\u30d4\u30e5\u30a2\u30fc",
                "\u30d2\u30e5\u30a6",
                "\u30d2\u30e6\u30a6",
                "\u30d4\u30e5\u30a6\u30a2",
                "\u3073\u3085\u30fc\u3042\u30fc",
                "\u30d3\u30e5\u30fc\u30a2\u30fc",
                "\u30d3\u30e5\u30a6\u30a2\u30fc",
                "\u3072\u3085\u3093",
                "\u3074\u3085\u3093",
                "\u3075\u30fc\u308a",
                "\u30d5\u30fc\u30ea",
                "\u3075\u3045\u308a",
                "\u3075\u30a5\u308a",
                "\u3075\u30a5\u30ea",
                "\u30d5\u30a6\u30ea",
                "\u3076\u30fc\u308a",
                "\u30d6\u30fc\u30ea",
                "\u3076\u3045\u308a",
                "\u30d6\u30a5\u308a",
                "\u3077\u3046\u308a",
                "\u30d7\u30a6\u30ea",
                "\u3075\u30fc\u308a\u30fc",
                "\u30d5\u30a5\u30ea\u30fc",
                "\u3075\u30a5\u308a\u30a3",
                "\u30d5\u3045\u308a\u3043",
                "\u30d5\u30a6\u30ea\u30fc",
                "\u3075\u3046\u308a\u3043",
                "\u30d6\u30a6\u30ea\u30a4",
                "\u3077\u30fc\u308a\u30fc",
                "\u3077\u30a5\u308a\u30a4",
                "\u3077\u3046\u308a\u30fc",
                "\u30d7\u30a6\u30ea\u30a4",
                "\u30d5\u30fd",
                "\u3075\u309e",
                "\u3076\u309d",
                "\u3076\u3075",
                "\u3076\u30d5",
                "\u30d6\u3075",
                "\u30d6\u30d5",
                "\u3076\u309e",
                "\u3076\u3077",
                "\u30d6\u3077",
                "\u3077\u309d",
                "\u30d7\u30fd",
                "\u3077\u3075",
            };

            String[] test2 = {
                "\u306f\u309d", // H\u309d
                "\u30cf\u30fd", // K\u30fd
                "\u306f\u306f", // HH
                "\u306f\u30cf", // HK
                "\u30cf\u30cf", // KK
                "\u306f\u309e", // H\u309e
                "\u30cf\u30fe", // K\u30fe
                "\u306f\u3070", // HH\u309b
                "\u30cf\u30d0", // KK\u309b
                "\u306f\u3071", // HH\u309c
                "\u30cf\u3071", // KH\u309c
                "\u30cf\u30d1", // KK\u309c
                "\u3070\u309d", // H\u309b\u309d
                "\u30d0\u30fd", // K\u309b\u30fd
                "\u3070\u306f", // H\u309bH
                "\u30d0\u30cf", // K\u309bK
                "\u3070\u309e", // H\u309b\u309e
                "\u30d0\u30fe", // K\u309b\u30fe
                "\u3070\u3070", // H\u309bH\u309b
                "\u30d0\u3070", // K\u309bH\u309b
                "\u30d0\u30d0", // K\u309bK\u309b
                "\u3070\u3071", // H\u309bH\u309c
                "\u30d0\u30d1", // K\u309bK\u309c
                "\u3071\u309d", // H\u309c\u309d
                "\u30d1\u30fd", // K\u309c\u30fd
                "\u3071\u306f", // H\u309cH
                "\u30d1\u30cf", // K\u309cK
                "\u3071\u3070", // H\u309cH\u309b
                "\u3071\u30d0", // H\u309cK\u309b
                "\u30d1\u30d0", // K\u309cK\u309b
                "\u3071\u3071", // H\u309cH\u309c
                "\u30d1\u30d1", // K\u309cK\u309c
            };

            String[] att = { "strength", };
            Object[] val = { new int?((int)Collator.Quaternary), };

            String[] attShifted = { "strength", "AlternateHandling" };
            Object[] valShifted = { new int?((int)Collator.Quaternary),
                                true };

            genericLocaleStarterWithOptions(new CultureInfo("ja") /* Locale.JAPANESE */, test1, att, val);
            genericLocaleStarterWithOptions(new CultureInfo("ja") /* Locale.JAPANESE */, test2, att, val);

            genericLocaleStarterWithOptions(new CultureInfo("ja") /* Locale.JAPANESE */, test1, attShifted,
                                            valShifted);
            genericLocaleStarterWithOptions(new CultureInfo("ja") /* Locale.JAPANESE */, test2, attShifted,
                                            valShifted);
        }

        internal void genericLocaleStarter(CultureInfo locale, String[] s)
        {
            RuleBasedCollator coll = null;
            try
            {
                coll = (RuleBasedCollator)Collator.GetInstance(locale);

            }
            catch (Exception e)
            {
                Warnln("Unable to open collator for locale " + locale);
                return;
            }
            // Logln("Locale starter for " + locale);
            genericOrderingTest(coll, s);
        }

        internal void genericLocaleStarterWithOptions(CultureInfo locale, String[] s, String[] attrs, Object[] values)
        {
            genericLocaleStarterWithOptionsAndResult(locale, s, attrs, values, -1);
        }

        private void genericOptionsSetter(RuleBasedCollator coll, String[] attrs, Object[] values)
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i].Equals("strength"))
                {
                    coll.Strength = (CollationStrength)(((int?)values[i]).Value);
                }
                else if (attrs[i].Equals("decomp"))
                {
                    coll.Decomposition = (NormalizationMode)(((int?)values[i]).Value);
                }
                else if (attrs[i].Equals("AlternateHandling"))
                {
                    coll.IsAlternateHandlingShifted = (((bool)values[i]
                                                      ));
                }
                else if (attrs[i].Equals("NumericCollation"))
                {
                    coll.IsNumericCollation = (((bool)values[i]));
                }
                else if (attrs[i].Equals("UpperFirst"))
                {
                    coll.IsUpperCaseFirst = (((bool)values[i]));
                }
                else if (attrs[i].Equals("LowerFirst"))
                {
                    coll.IsLowerCaseFirst = (((bool)values[i]));
                }
                else if (attrs[i].Equals("CaseLevel"))
                {
                    coll.IsCaseLevel = (((bool)values[i]));
                }
            }
        }

        internal void genericLocaleStarterWithOptionsAndResult(CultureInfo locale, String[] s, String[] attrs, Object[] values, int result)
        {
            RuleBasedCollator coll = null;
            try
            {
                coll = (RuleBasedCollator)Collator.GetInstance(locale);
            }
            catch (Exception e)
            {
                Warnln("Unable to open collator for locale " + locale);
                return;
            }
            // Logln("Locale starter for " +locale);

            // Logln("Setting attributes");
            genericOptionsSetter(coll, attrs, values);

            genericOrderingTestWithResult(coll, s, result);
        }

        internal void genericOrderingTest(Collator coll, String[] s)
        {
            genericOrderingTestWithResult(coll, s, -1);
        }

        [Test]
        public void TestNonChars()
        {
            String[] test = {
            "\u0000",  /* ignorable */
            "\uFFFE",  /* special merge-sort character with minimum non-ignorable weights */
            "\uFDD0", "\uFDEF",
            "\\U0001FFFE", "\\U0001FFFF",  /* UCA 6.0: noncharacters are treated like unassigned, */
            "\\U0002FFFE", "\\U0002FFFF",  /* not like ignorable. */
            "\\U0003FFFE", "\\U0003FFFF",
            "\\U0004FFFE", "\\U0004FFFF",
            "\\U0005FFFE", "\\U0005FFFF",
            "\\U0006FFFE", "\\U0006FFFF",
            "\\U0007FFFE", "\\U0007FFFF",
            "\\U0008FFFE", "\\U0008FFFF",
            "\\U0009FFFE", "\\U0009FFFF",
            "\\U000AFFFE", "\\U000AFFFF",
            "\\U000BFFFE", "\\U000BFFFF",
            "\\U000CFFFE", "\\U000CFFFF",
            "\\U000DFFFE", "\\U000DFFFF",
            "\\U000EFFFE", "\\U000EFFFF",
            "\\U000FFFFE", "\\U000FFFFF",
            "\\U0010FFFE", "\\U0010FFFF",
            "\uFFFF"  /* special character with maximum primary weight */
        };
            Collator coll = null;
            try
            {
                coll = Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("Unable to open collator");
                return;
            }
            // Logln("Test non characters");

            genericOrderingTestWithResult(coll, test, -1);
        }

        [Test]
        public void TestExtremeCompression()
        {
            String[] test = new String[4];

            for (int i = 0; i < 4; i++)
            {
                StringBuffer temp = new StringBuffer();
                for (int j = 0; j < 2047; j++)
                {
                    temp.Append('a');
                }
                temp.Append((char)('a' + i));
                test[i] = temp.ToString();
            }

            genericLocaleStarter(new CultureInfo("en-US"), test);
        }

        /**
         * Tests surrogate support.
         */
        [Test]
        public void TestSurrogates()
        {
            String[] test = {"z","\ud900\udc25", "\ud805\udc50", "\ud800\udc00y",
                         "\ud800\udc00r", "\ud800\udc00f", "\ud800\udc00",
                         "\ud800\udc00c", "\ud800\udc00b", "\ud800\udc00fa",
                         "\ud800\udc00fb", "\ud800\udc00a", "c", "b"};

            String rule = "&z < \ud900\udc25 < \ud805\udc50 < \ud800\udc00y "
                + "< \ud800\udc00r < \ud800\udc00f << \ud800\udc00 "
                + "< \ud800\udc00fa << \ud800\udc00fb < \ud800\udc00a "
                + "< c < b";
            genericRulesStarter(rule, test);
        }

        [Test]
        public void TestBocsuCoverage()
        {
            String test = "\u0041\u0441\u4441\\U00044441\u4441\u0441\u0041";
            Collator coll = Collator.GetInstance();
            coll.Strength = (Collator.Identical);
            CollationKey key = coll.GetCollationKey(test);
            Logln("source:" + key.SourceString);
        }

        [Test]
        public void TestCyrillicTailoring()
        {
            String[] test = {
                "\u0410b",
                "\u0410\u0306a",
                "\u04d0A"
            };

            // Most of the following are commented out because UCA 8.0
            // drops most of the Cyrillic contractions from the default order.
            // See CLDR ticket #7246 "root collation: remove Cyrillic contractions".

            // genericLocaleStarter(new Locale("en", ""), test);
            // genericRulesStarter("&\u0410 = \u0410", test);
            // genericRulesStarter("&Z < \u0410", test);
            genericRulesStarter("&\u0410 = \u0410 < \u04d0", test);
            genericRulesStarter("&Z < \u0410 < \u04d0", test);
            // genericRulesStarter("&\u0410 = \u0410 < \u0410\u0301", test);
            // genericRulesStarter("&Z < \u0410 < \u0410\u0301", test);
        }

        [Test]
        public void TestSuppressContractions()
        {
            String[] testNoCont2 = {
                "\u0410\u0302a",
                "\u0410\u0306b",
                "\u0410c"
            };
            String[] testNoCont = {
                "a\u0410",
                "A\u0410\u0306",
                "\uFF21\u0410\u0302"
            };

            genericRulesStarter("[suppressContractions [\u0400-\u047f]]", testNoCont);
            genericRulesStarter("[suppressContractions [\u0400-\u047f]]", testNoCont2);
        }

        [Test]
        public void TestCase()
        {
            String gRules = "\u0026\u0030\u003C\u0031\u002C\u2460\u003C\u0061\u002C\u0041";
            String[] testCase = {
                "1a", "1A", "\u2460a", "\u2460A"
            };
            int[][] caseTestResults = {
                new int[] { -1, -1, -1, 0, -1, -1, 0, 0, -1 },
                new int[] { 1, -1, -1, 0, -1, -1, 0, 0, 1 },
                new int[] { -1, -1, -1, 0, 1, -1, 0, 0, -1 },
                new int[] { 1, -1, 1, 0, -1, -1, 0, 0, 1 }

            };
            bool[][] caseTestAttributes = {
                new bool[] { false, false},
                new bool[] { true, false},
                new bool[] { false, true},
                new bool[] { true, true}
            };

            int i, j, k;
            Collator myCollation;
            try
            {
                myCollation = Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator ");
                return;
            }
            // Logln("Testing different case settings");
            myCollation.Strength = (Collator.Tertiary);

            for (k = 0; k < 4; k++)
            {
                if (caseTestAttributes[k][0] == true)
                {
                    // upper case first
                    ((RuleBasedCollator)myCollation).IsUpperCaseFirst = (true);
                }
                else
                {
                    // upper case first
                    ((RuleBasedCollator)myCollation).IsLowerCaseFirst = (true);
                }
                ((RuleBasedCollator)myCollation).IsCaseLevel = (
                                                              caseTestAttributes[k][1]);

                // Logln("Case first = " + caseTestAttributes[k][0] + ", Case level = " + caseTestAttributes[k][1]);
                for (i = 0; i < 3; i++)
                {
                    for (j = i + 1; j < 4; j++)
                    {
                        CollationTest.DoTest(this,
                                             (RuleBasedCollator)myCollation,
                                             testCase[i], testCase[j],
                                             caseTestResults[k][3 * i + j - 1]);
                    }
                }
            }
            try
            {
                myCollation = new RuleBasedCollator(gRules);
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
                return;
            }
            // Logln("Testing different case settings with custom rules");
            myCollation.Strength = (Collator.Tertiary);

            for (k = 0; k < 4; k++)
            {
                if (caseTestAttributes[k][0] == true)
                {
                    ((RuleBasedCollator)myCollation).IsUpperCaseFirst = (true);
                }
                else
                {
                    ((RuleBasedCollator)myCollation).IsUpperCaseFirst = (false);
                }
                ((RuleBasedCollator)myCollation).IsCaseLevel = (
                                                              caseTestAttributes[k][1]);
                for (i = 0; i < 3; i++)
                {
                    for (j = i + 1; j < 4; j++)
                    {
                        CollationTest.DoTest(this,
                                             (RuleBasedCollator)myCollation,
                                             testCase[i], testCase[j],
                                             caseTestResults[k][3 * i + j - 1]);
                    }
                }
            }

            {
                String[] lowerFirst = {
                    "h",
                    "H",
                    "ch",
                    "Ch",
                    "CH",
                    "cha",
                    "chA",
                    "Cha",
                    "ChA",
                    "CHa",
                    "CHA",
                    "i",
                    "I"
                };

                String[] upperFirst = {
                    "H",
                    "h",
                    "CH",
                    "Ch",
                    "ch",
                    "CHA",
                    "CHa",
                    "ChA",
                    "Cha",
                    "chA",
                    "cha",
                    "I",
                    "i"
                };
                // Logln("mixed case test");
                // Logln("lower first, case level off");
                genericRulesStarter("[caseFirst lower]&H<ch<<<Ch<<<CH", lowerFirst);
                // Logln("upper first, case level off");
                genericRulesStarter("[caseFirst upper]&H<ch<<<Ch<<<CH", upperFirst);
                // Logln("lower first, case level on");
                genericRulesStarter("[caseFirst lower][caseLevel on]&H<ch<<<Ch<<<CH", lowerFirst);
                // Logln("upper first, case level on");
                genericRulesStarter("[caseFirst upper][caseLevel on]&H<ch<<<Ch<<<CH", upperFirst);
            }
        }

        [Test]
        public void TestIncompleteCnt()
        {
            String[] cnt1 = {
                "AA",
                "AC",
                "AZ",
                "AQ",
                "AB",
                "ABZ",
                "ABQ",
                "Z",
                "ABC",
                "Q",
                "B"
            };

            String[] cnt2 = {
                "DA",
                "DAD",
                "DAZ",
                "MAR",
                "Z",
                "DAVIS",
                "MARK",
                "DAV",
                "DAVI"
            };
            RuleBasedCollator coll = null;
            String temp = " & Z < ABC < Q < B";
            try
            {
                coll = new RuleBasedCollator(temp);
            }
            catch (Exception e)
            {
                Warnln("fail to create RuleBasedCollator");
                return;
            }

            int size = cnt1.Length;
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    String t1 = cnt1[i];
                    String t2 = cnt1[j];
                    CollationTest.DoTest(this, coll, t1, t2, -1);
                }
            }

            temp = " & Z < DAVIS < MARK <DAV";
            try
            {
                coll = new RuleBasedCollator(temp);
            }
            catch (Exception e)
            {
                Warnln("fail to create RuleBasedCollator");
                return;
            }

            size = cnt2.Length;
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    String t1 = cnt2[i];
                    String t2 = cnt2[j];
                    CollationTest.DoTest(this, coll, t1, t2, -1);
                }
            }
        }

        [Test]
        public void TestBlackBird()
        {
            String[] shifted = {
                "black bird",
                "black-bird",
                "blackbird",
                "black Bird",
                "black-Bird",
                "blackBird",
                "black birds",
                "black-birds",
                "blackbirds"
            };
            int[] shiftedTert = {
                0,
                0,
                0,
                -1,
                0,
                0,
                -1,
                0,
                0
            };
            String[] nonignorable = {
                "black bird",
                "black Bird",
                "black birds",
                "black-bird",
                "black-Bird",
                "black-birds",
                "blackbird",
                "blackBird",
                "blackbirds"
            };
            int i = 0, j = 0;
            int size = 0;
            Collator coll = Collator.GetInstance(new CultureInfo("en-US"));
            //ucol_setAttribute(coll, UCOL_NORMALIZATION_MODE, UCOL_OFF, &status);
            //ucol_setAttribute(coll, UCOL_ALTERNATE_HANDLING, UCOL_NON_IGNORABLE, &status);
            ((RuleBasedCollator)coll).IsAlternateHandlingShifted = (false);
            size = nonignorable.Length;
            for (i = 0; i < size - 1; i++)
            {
                for (j = i + 1; j < size; j++)
                {
                    String t1 = nonignorable[i];
                    String t2 = nonignorable[j];
                    CollationTest.DoTest(this, (RuleBasedCollator)coll, t1, t2, -1);
                }
            }
            ((RuleBasedCollator)coll).IsAlternateHandlingShifted = (true);
            coll.Strength = (Collator.Quaternary);
            size = shifted.Length;
            for (i = 0; i < size - 1; i++)
            {
                for (j = i + 1; j < size; j++)
                {
                    String t1 = shifted[i];
                    String t2 = shifted[j];
                    CollationTest.DoTest(this, (RuleBasedCollator)coll, t1, t2, -1);
                }
            }
            coll.Strength = (Collator.Tertiary);
            size = shifted.Length;
            for (i = 1; i < size; i++)
            {
                String t1 = shifted[i - 1];
                String t2 = shifted[i];
                CollationTest.DoTest(this, (RuleBasedCollator)coll, t1, t2,
                                     shiftedTert[i]);
            }
        }

        [Test]
        public void TestFunkyA()
        {
            String[] testSourceCases = {
                "\u0041\u0300\u0301",
                "\u0041\u0300\u0316",
                "\u0041\u0300",
                "\u00C0\u0301",
                // this would work with forced normalization
                "\u00C0\u0316",
            };

            String[] testTargetCases = {
                "\u0041\u0301\u0300",
                "\u0041\u0316\u0300",
                "\u00C0",
                "\u0041\u0301\u0300",
                // this would work with forced normalization
                "\u0041\u0316\u0300",
            };

            int[] results = {
                1,
                0,
                0,
                1,
                0
            };

            Collator myCollation;
            try
            {
                myCollation = Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
                return;
            }
            // Logln("Testing some A letters, for some reason");
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
            myCollation.Strength = (Collator.Tertiary);
            for (int i = 0; i < 4; i++)
            {
                CollationTest.DoTest(this, (RuleBasedCollator)myCollation,
                                     testSourceCases[i], testTargetCases[i],
                                     results[i]);
            }
        }

        [Test]
        public void TestChMove()
        {
            String[] chTest = {
                "c",
                "C",
                "ca", "cb", "cx", "cy", "CZ",
                "c\u030C", "C\u030C",
                "h",
                "H",
                "ha", "Ha", "harly", "hb", "HB", "hx", "HX", "hy", "HY",
                "ch", "cH", "Ch", "CH",
                "cha", "charly", "che", "chh", "chch", "chr",
                "i", "I", "iarly",
                "r", "R",
                "r\u030C", "R\u030C",
                "s",
                "S",
                "s\u030C", "S\u030C",
                "z", "Z",
                "z\u030C", "Z\u030C"
            };
            Collator coll = null;
            try
            {
                coll = Collator.GetInstance(new CultureInfo("cs"));
            }
            catch (Exception e)
            {
                Warnln("Cannot create Collator");
                return;
            }
            int size = chTest.Length;
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    String t1 = chTest[i];
                    String t2 = chTest[j];
                    CollationTest.DoTest(this, (RuleBasedCollator)coll, t1, t2, -1);
                }
            }
        }

        [Test]
        public void TestImplicitTailoring()
        {
            String[] rules = {
                /* Tailor b and c before U+4E00. */
                "&[before 1]\u4e00 < b < c " +
                /* Now, before U+4E00 is c; put d and e after that. */
                "&[before 1]\u4e00 < d < e",
                "&\u4e00 < a <<< A < b <<< B",
                "&[before 1]\u4e00 < \u4e01 < \u4e02",
                "&[before 1]\u4e01 < \u4e02 < \u4e03",
            };
            String[][] cases = {
                new string[] { "b", "c", "d", "e", "\u4e00" },
                new string[] { "\u4e00", "a", "A", "b", "B", "\u4e01" },
                new string[] { "\u4e01", "\u4e02", "\u4e00" },
                new string[] { "\u4e02", "\u4e03", "\u4e01" },
            };

            int i = 0;

            for (i = 0; i < rules.Length; i++)
            {
                genericRulesStarter(rules[i], cases[i]);
            }
        }

        [Test]
        public void TestFCDProblem()
        {
            String s1 = "\u0430\u0306\u0325";
            String s2 = "\u04D1\u0325";
            Collator coll = null;
            try
            {
                coll = Collator.GetInstance();
            }
            catch (Exception e)
            {
                Warnln("Can't create collator");
                return;
            }

            coll.Decomposition = (Collator.NoDecomposition);
            CollationTest.DoTest(this, (RuleBasedCollator)coll, s1, s2, 0);
            coll.Decomposition = (Collator.CanonicalDecomposition);
            CollationTest.DoTest(this, (RuleBasedCollator)coll, s1, s2, 0);
        }

        [Test]
        public void TestEmptyRule()
        {
            String rulez = "";
            try
            {
                RuleBasedCollator coll = new RuleBasedCollator(rulez);
                Logln("rule:" + coll.GetRules());
            }
            catch (Exception e)
            {
                Warnln(e.ToString());
            }
        }

        /* superseded by TestBeforePinyin, since Chinese collation rules have changed */
        /*
        [Test]
        public void TestJ784() {
            String[] data = {
                "A", "\u0101", "\u00e1", "\u01ce", "\u00e0",
                "E", "\u0113", "\u00e9", "\u011b", "\u00e8",
                "I", "\u012b", "\u00ed", "\u01d0", "\u00ec",
                "O", "\u014d", "\u00f3", "\u01d2", "\u00f2",
                "U", "\u016b", "\u00fa", "\u01d4", "\u00f9",
                "\u00fc", "\u01d6", "\u01d8", "\u01da", "\u01dc"
            };
            genericLocaleStarter(new Locale("zh", ""), data);
        }
        */

        [Test]
        public void TestJ815()
        {
            String[] data = {
                "aa",
                "Aa",
                "ab",
                "Ab",
                "ad",
                "Ad",
                "ae",
                "Ae",
                "\u00e6",
                "\u00c6",
                "af",
                "Af",
                "b",
                "B"
            };
            genericLocaleStarter(new CultureInfo("fr"), data);
            genericRulesStarter("[backwards 2]&A<<\u00e6/e<<<\u00c6/E", data);
        }

        [Test]
        public void TestJ3087()
        {
            String[] rule = {
                    "&h<H&CH=\u0427",
                    /*
                     * The ICU 53 builder adheres to the principle that
                     * a rule is affected by previous rules but not following ones.
                     * Therefore, setting CH=\u0427 and then re-tailoring H makes CH != \u0427.
                    "&CH=\u0427&h<H", */
                    "&CH=\u0427"
            };
            RuleBasedCollator rbc = null;
            CollationElementIterator iter1;
            CollationElementIterator iter2;
            for (int i = 0; i < rule.Length; i++)
            {
                try
                {
                    rbc = new RuleBasedCollator(rule[i]);
                }
                catch (Exception e)
                {
                    Warnln(e.ToString());
                    continue;
                }
                iter1 = rbc.GetCollationElementIterator("CH");
                iter2 = rbc.GetCollationElementIterator("\u0427");
                int ce1 = CollationElementIterator.Ingorable;
                int ce2 = CollationElementIterator.Ingorable;
                // The ICU 53 builder code sets the uppercase flag only on the first CE.
                int mask = ~0;
                while (ce1 != CollationElementIterator.NullOrder
                       && ce2 != CollationElementIterator.NullOrder)
                {
                    ce1 = iter1.Next();
                    ce2 = iter2.Next();
                    if ((ce1 & mask) != (ce2 & mask))
                    {
                        Errln("Error generating RuleBasedCollator with the rule "
                              + rule[i]);
                        Errln("CH != \\u0427");
                    }
                    mask = ~0xc0;  // mask off case/continuation bits
                }
            }
        }

        [Test]
        public void TestUpperCaseFirst()
        {
            String[] data = {
                "I",
                "i",
                "Y",
                "y"
            };
            genericLocaleStarter(new CultureInfo("da"), data);
        }

        [Test]
        public void TestBefore()
        {
            String[] data = {
                "\u0101", "\u00e1", "\u01ce", "\u00e0", "A",
                "\u0113", "\u00e9", "\u011b", "\u00e8", "E",
                "\u012b", "\u00ed", "\u01d0", "\u00ec", "I",
                "\u014d", "\u00f3", "\u01d2", "\u00f2", "O",
                "\u016b", "\u00fa", "\u01d4", "\u00f9", "U",
                "\u01d6", "\u01d8", "\u01da", "\u01dc", "\u00fc"
            };
            genericRulesStarter(
                                "&[before 1]a<\u0101<\u00e1<\u01ce<\u00e0"
                                + "&[before 1]e<\u0113<\u00e9<\u011b<\u00e8"
                                + "&[before 1]i<\u012b<\u00ed<\u01d0<\u00ec"
                                + "&[before 1]o<\u014d<\u00f3<\u01d2<\u00f2"
                                + "&[before 1]u<\u016b<\u00fa<\u01d4<\u00f9"
                                + "&u<\u01d6<\u01d8<\u01da<\u01dc<\u00fc", data);
        }

        [Test]
        public void TestHangulTailoring()
        {
            String[] koreanData = {
                "\uac00", "\u4f3d", "\u4f73", "\u5047", "\u50f9", "\u52a0", "\u53ef", "\u5475",
                "\u54e5", "\u5609", "\u5ac1", "\u5bb6", "\u6687", "\u67b6", "\u67b7", "\u67ef",
                "\u6b4c", "\u73c2", "\u75c2", "\u7a3c", "\u82db", "\u8304", "\u8857", "\u8888",
                "\u8a36", "\u8cc8", "\u8dcf", "\u8efb", "\u8fe6", "\u99d5",
                "\u4EEE", "\u50A2", "\u5496", "\u54FF", "\u5777", "\u5B8A", "\u659D", "\u698E",
                "\u6A9F", "\u73C8", "\u7B33", "\u801E", "\u8238", "\u846D", "\u8B0C"
            };

            String rules =
                    "&\uac00 <<< \u4f3d <<< \u4f73 <<< \u5047 <<< \u50f9 <<< \u52a0 <<< \u53ef <<< \u5475 "
                    + "<<< \u54e5 <<< \u5609 <<< \u5ac1 <<< \u5bb6 <<< \u6687 <<< \u67b6 <<< \u67b7 <<< \u67ef "
                    + "<<< \u6b4c <<< \u73c2 <<< \u75c2 <<< \u7a3c <<< \u82db <<< \u8304 <<< \u8857 <<< \u8888 "
                    + "<<< \u8a36 <<< \u8cc8 <<< \u8dcf <<< \u8efb <<< \u8fe6 <<< \u99d5 "
                    + "<<< \u4EEE <<< \u50A2 <<< \u5496 <<< \u54FF <<< \u5777 <<< \u5B8A <<< \u659D <<< \u698E "
                    + "<<< \u6A9F <<< \u73C8 <<< \u7B33 <<< \u801E <<< \u8238 <<< \u846D <<< \u8B0C";

            String rlz = rules;

            Collator coll = null;
            try
            {
                coll = new RuleBasedCollator(rlz);
            }
            catch (Exception e)
            {
                Warnln("Unable to open collator with rules" + rules);
                return;
            }
            // Logln("Using start of korean rules\n");
            genericOrderingTest(coll, koreanData);

            // no such locale in icu4j
            // Logln("Using ko__LOTUS locale\n");
            // genericLocaleStarter(new Locale("ko__LOTUS", ""), koreanData);
        }

        [Test]
        public void TestIncrementalNormalize()
        {
            Collator coll = null;
            // Logln("Test 1 ....");
            {
                /* Test 1.  Run very long unnormalized strings, to force overflow of*/
                /*          most buffers along the way.*/

                try
                {
                    coll = Collator.GetInstance(new CultureInfo("en-US"));
                }
                catch (Exception e)
                {
                    Warnln("Cannot get default instance!");
                    return;
                }
                char baseA = (char)0x41;
                char[] ccMix = { (char)0x316, (char)0x321, (char)0x300 };
                int sLen;
                int i;
                StringBuffer strA = new StringBuffer();
                StringBuffer strB = new StringBuffer();

                coll.Decomposition = (Collator.CanonicalDecomposition);

                for (sLen = 1000; sLen < 1001; sLen++)
                {
                    strA.Delete(0, strA.Length);
                    strA.Append(baseA);
                    strB.Delete(0, strB.Length);
                    strB.Append(baseA);
                    for (i = 1; i < sLen; i++)
                    {
                        strA.Append(ccMix[i % 3]);
                        strB.Insert(1, ccMix[i % 3]);
                    }
                    coll.Strength = (Collator.Tertiary);   // Do test with default strength, which runs
                    CollationTest.DoTest(this, (RuleBasedCollator)coll,
                                         strA.ToString(), strB.ToString(), 0);    //   optimized functions in the impl
                    coll.Strength = (Collator.Identical);   // Do again with the slow, general impl.
                    CollationTest.DoTest(this, (RuleBasedCollator)coll,
                                         strA.ToString(), strB.ToString(), 0);
                }
            }
            /*  Test 2:  Non-normal sequence in a string that extends to the last character*/
            /*         of the string.  Checks a couple of edge cases.*/
            // Logln("Test 2 ....");
            {
                String strA = "AA\u0300\u0316";
                String strB = "A\u00c0\u0316";
                coll.Strength = (Collator.Tertiary);
                CollationTest.DoTest(this, (RuleBasedCollator)coll, strA, strB, 0);
            }
            /*  Test 3:  Non-normal sequence is terminated by a surrogate pair.*/
            // Logln("Test 3 ....");
            {
                String strA = "AA\u0300\u0316\uD800\uDC01";
                String strB = "A\u00c0\u0316\uD800\uDC00";
                coll.Strength = (Collator.Tertiary);
                CollationTest.DoTest(this, (RuleBasedCollator)coll, strA, strB, 1);
            }
            /*  Test 4:  Imbedded nulls do not terminate a string when length is specified.*/
            // Logln("Test 4 ....");
            /*
             * not a valid test since string are null-terminated in java{
             char strA[] = {0x41, 0x00, 0x42};
             char strB[] = {0x41, 0x00, 0x00};

             int result = coll.compare(new String(strA), new String(strB));
             if (result != 1) {
             Errln("ERROR 1 in test 4\n");
             }

             result = coll.compare(new String(strA, 0, 1), new String(strB, 0, 1));
             if (result != 0) {
             Errln("ERROR 1 in test 4\n");
             }

             CollationKey sortKeyA = coll.GetCollationKey(new String(strA));
             CollationKey sortKeyB = coll.GetCollationKey(new String(strB));

             int r = sortKeyA.compareTo(sortKeyB);
             if (r <= 0) {
             Errln("Error 4 in test 4\n");
             }

             coll.Strength=(Collator.IDENTICAL);
             sortKeyA = coll.GetCollationKey(new String(strA));
             sortKeyB = coll.GetCollationKey(new String(strB));

             r = sortKeyA.compareTo(sortKeyB);
             if (r <= 0) {
             Errln("Error 7 in test 4\n");
             }

             coll.Strength=(Collator.TERTIARY);
             }
            */
            /*  Test 5:  Null characters in non-normal source strings.*/
            // Logln("Test 5 ....");
            /*
             * not a valid test since string are null-terminated in java{
             {
             char strA[] = {0x41, 0x41, 0x300, 0x316, 0x00, 0x42,};
             char strB[] = {0x41, 0x41, 0x300, 0x316, 0x00, 0x00,};


             int result = coll.compare(new String(strA, 0, 6), new String(strB, 0, 6));
             if (result < 0) {
             Errln("ERROR 1 in test 5\n");
             }
             result = coll.compare(new String(strA, 0, 4), new String(strB, 0, 4));
             if (result != 0) {
             Errln("ERROR 2 in test 5\n");
             }

             CollationKey sortKeyA = coll.GetCollationKey(new String(strA));
             CollationKey sortKeyB = coll.GetCollationKey(new String(strB));
             int r = sortKeyA.compareTo(sortKeyB);
             if (r <= 0) {
             Errln("Error 4 in test 5\n");
             }

             coll.Strength=(Collator.IDENTICAL);

             sortKeyA = coll.GetCollationKey(new String(strA));
             sortKeyB = coll.GetCollationKey(new String(strB));
             r = sortKeyA.compareTo(sortKeyB);
             if (r <= 0) {
             Errln("Error 7 in test 5\n");
             }

             coll.Strength=(Collator.TERTIARY);
             }
            */
            /*  Test 6:  Null character as base of a non-normal combining sequence.*/
            // Logln("Test 6 ....");
            /*
             * not a valid test since string are null-terminated in java{
             {
             char strA[] = {0x41, 0x0, 0x300, 0x316, 0x41, 0x302,};
             char strB[] = {0x41, 0x0, 0x302, 0x316, 0x41, 0x300,};

             int result = coll.compare(new String(strA, 0, 5), new String(strB, 0, 5));
             if (result != -1) {
             Errln("Error 1 in test 6\n");
             }
             result = coll.compare(new String(strA, 0, 1), new String(strB, 0, 1));
             if (result != 0) {
             Errln("Error 2 in test 6\n");
             }
             }
            */
        }

        [Test]
        public void TestContraction()
        {
            String[] testrules = {
                "&A = AB / B",
                "&A = A\\u0306/\\u0306",
                "&c = ch / h",
            };
            String[] testdata = {
                "AB", "AB", "A\u0306", "ch"
            };
            String[] testdata2 = {
                "\u0063\u0067",
                "\u0063\u0068",
                "\u0063\u006C",
            };
            /*
             * These pairs of rule strings are not guaranteed to yield the very same mappings.
             * In fact, LDML 24 recommends an improved way of creating mappings
             * which always yields different mappings for such pairs. See
             * http://www.unicode.org/reports/tr35/tr35-33/tr35-collation.html#Orderings
            String[] testrules3 = {
                "&z < xyz &xyzw << B",
                "&z < xyz &xyz << B / w",
                "&z < ch &achm << B",
                "&z < ch &a << B / chm",
                "&\ud800\udc00w << B",
                "&\ud800\udc00 << B / w",
                "&a\ud800\udc00m << B",
                "&a << B / \ud800\udc00m",
            }; */

            RuleBasedCollator coll = null;
            for (int i = 0; i < testrules.Length; i++)
            {
                CollationElementIterator iter1 = null;
                int j = 0;
                // Logln("Rule " + testrules[i] + " for testing\n");
                String rule = testrules[i];
                try
                {
                    coll = new RuleBasedCollator(rule);
                }
                catch (Exception e)
                {
                    Warnln("Collator creation failed " + testrules[i]);
                    return;
                }
                try
                {
                    iter1 = coll.GetCollationElementIterator(testdata[i]);
                }
                catch (Exception e)
                {
                    Errln("Collation iterator creation failed\n");
                    return;
                }
                while (j < 2)
                {
                    CollationElementIterator iter2;
                    int ce;
                    try
                    {
                        iter2 = coll.GetCollationElementIterator(testdata[i][j] + "");

                    }
                    catch (Exception e)
                    {
                        Errln("Collation iterator creation failed\n");
                        return;
                    }
                    ce = iter2.Next();
                    while (ce != CollationElementIterator.NullOrder)
                    {
                        if (iter1.Next() != ce)
                        {
                            Errln("Collation elements in contraction split does not match\n");
                            return;
                        }
                        ce = iter2.Next();
                    }
                    j++;
                }
                if (iter1.Next() != CollationElementIterator.NullOrder)
                {
                    Errln("Collation elements not exhausted\n");
                    return;
                }
            }
            {
                String rule = "& a < b < c < ch < d & c = ch / h";
                try
                {
                    coll = new RuleBasedCollator(rule);
                }
                catch (Exception e)
                {
                    Errln("cannot create rulebased collator");
                    return;
                }

                if (coll.Compare(testdata2[0], testdata2[1]) != -1)
                {
                    Errln("Expected " + testdata2[0] + " < " + testdata2[1]);
                    return;
                }
                if (coll.Compare(testdata2[1], testdata2[2]) != -1)
                {
                    Errln("Expected " + testdata2[1] + " < " + testdata2[2]);
                    return;
                }
                /* see above -- for (int i = 0; i < testrules3.Length; i += 2) {
                    RuleBasedCollator          coll1, coll2;
                    CollationElementIterator iter1, iter2;
                    char               ch = 0x0042;
                    int            ce;
                    rule = testrules3[i];
                    try {
                        coll1 = new RuleBasedCollator(rule);
                    } catch (Exception e) {
                        Errln("Fail: cannot create rulebased collator, rule:" + rule);
                        return;
                    }
                    rule = testrules3[i + 1];
                    try {
                        coll2 = new RuleBasedCollator(rule);
                    } catch (Exception e) {
                        Errln("Collator creation failed " + testrules[i]);
                        return;
                    }
                    try {
                        iter1 = coll1.GetCollationElementIterator(String.valueOf(ch));
                        iter2 = coll2.GetCollationElementIterator(String.valueOf(ch));
                    } catch (Exception e) {
                        Errln("Collation iterator creation failed\n");
                        return;
                    }
                    ce = iter1.Next();

                    while (ce != CollationElementIterator.NULLORDER) {
                        if (ce != iter2.Next()) {
                            Errln("CEs does not match\n");
                            return;
                        }
                        ce = iter1.Next();
                    }
                    if (iter2.Next() != CollationElementIterator.NULLORDER) {
                        Errln("CEs not exhausted\n");
                        return;
                    }
                } */
            }
        }

        [Test]
        public void TestExpansion()
        {
            String[] testrules = {
                /*
                 * This seems to have tested that M was not mapped to an expansion.
                 * I believe the old builder just did that because it computed the extension CEs
                 * at the very end, which was a bug.
                 * Among other problems, it violated the core tailoring principle
                 * by making an earlier rule depend on a later one.
                 * And, of course, if M did not get an expansion, then it was primary different from K,
                 * unlike what the rule &K<<M says.
                "&J << K / B & K << M",
                 */
                "&J << K / B << M"
            };
            String[] testdata = {
                "JA", "MA", "KA", "KC", "JC", "MC",
            };

            Collator coll;
            for (int i = 0; i < testrules.Length; i++)
            {
                // Logln("Rule " + testrules[i] + " for testing\n");
                String rule = testrules[i];
                try
                {
                    coll = new RuleBasedCollator(rule);
                }
                catch (Exception e)
                {
                    Warnln("Collator creation failed " + testrules[i]);
                    return;
                }

                for (int j = 0; j < 5; j++)
                {
                    CollationTest.DoTest(this, (RuleBasedCollator)coll,
                                         testdata[j], testdata[j + 1], -1);
                }
            }
        }

        [Test]
        public void TestContractionEndCompare()
        {
            String rules = "&b=ch";
            String src = "bec";
            String tgt = "bech";
            Collator coll = null;
            try
            {
                coll = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Warnln("Collator creation failed " + rules);
                return;
            }
            CollationTest.DoTest(this, (RuleBasedCollator)coll, src, tgt, 1);
        }

        [Test]
        public void TestLocaleRuleBasedCollators()
        {
            if (TestFmwk.GetExhaustiveness() < 5)
            {
                // not serious enough to run this
                return;
            }
            CultureInfo[] locale = Collator.GetAvailableLocales();
            String prevrule = null;
            for (int i = 0; i < locale.Length; i++)
            {
                CultureInfo l = locale[i];
                try
                {
                    ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuCollationBaseName, l);
                    String collkey = rb.GetStringWithFallback("collations/default");
                    ICUResourceBundle elements = rb.GetWithFallback("collations/" + collkey);
                    if (elements == null)
                    {
                        continue;
                    }
                    String rule = null;
                    /*
                      Object[][] colldata = (Object[][])elements;
                      // %%CollationBin
                      if (colldata[0][1] instanceof byte[]){
                      rule = (String)colldata[1][1];
                      }
                      else {
                      rule = (String)colldata[0][1];
                      }
                    */
                    rule = elements.GetString("Sequence");

                    RuleBasedCollator col1 =
                        (RuleBasedCollator)Collator.GetInstance(l);
                    if (!rule.Equals(col1.GetRules()))
                    {
                        Errln("Rules should be the same in the RuleBasedCollator and Locale");
                    }
                    if (rule != null && rule.Length > 0
                        && !rule.Equals(prevrule))
                    {
                        RuleBasedCollator col2 = new RuleBasedCollator(rule);
                        if (!col1.Equals(col2))
                        {
                            Errln("Error creating RuleBasedCollator from " +
                                  "locale rules for " + l.ToString());
                        }
                    }
                    prevrule = rule;
                }
                catch (Exception e)
                {
                    Warnln("Error retrieving resource bundle for testing: " + e.ToString());
                }
            }
        }

        [Test]
        public void TestOptimize()
        {
            /* this is not really a test - just trying out
             * whether copying of UCA contents will fail
             * Cannot really test, since the functionality
             * remains the same.
             */
            String[] rules = {
                "[optimize [\\uAC00-\\uD7FF]]"
            };
            String[][] data = {
                new string[] { "a", "b"}
            };
            int i = 0;

            for (i = 0; i < rules.Length; i++)
            {
                genericRulesStarter(rules[i], data[i]);
            }
        }

        [Test]
        public void TestIdenticalCompare()
        {
            try
            {
                RuleBasedCollator coll
                    = new RuleBasedCollator("& \uD800\uDC00 = \uD800\uDC01");
                String strA = "AA\u0300\u0316\uD800\uDC01";
                String strB = "A\u00c0\u0316\uD800\uDC00";
                coll.Strength = (Collator.Identical);
                CollationTest.DoTest(this, coll, strA, strB, 1);
            }
            catch (Exception e)
            {
                Warnln(e.ToString());
            }
        }

        [Test]
        public void TestMergeSortKeys()
        {
            String[] cases = { "abc", "abcd", "abcde" };
            String prefix = "foo";
            String suffix = "egg";
            CollationKey[] mergedPrefixKeys = new CollationKey[cases.Length];
            CollationKey[] mergedSuffixKeys = new CollationKey[cases.Length];

            Collator coll = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH */);
            genericLocaleStarter(new CultureInfo("en")  /*Locale.ENGLISH */, cases);

            CollationStrength strength = Collator.Primary;
            while (strength <= Collator.Identical)
            {
                coll.Strength = (strength);
                CollationKey prefixKey = coll.GetCollationKey(prefix);
                CollationKey suffixKey = coll.GetCollationKey(suffix);
                for (int i = 0; i < cases.Length; i++)
                {
                    CollationKey key = coll.GetCollationKey(cases[i]);
                    mergedPrefixKeys[i] = prefixKey.Merge(key);
                    mergedSuffixKeys[i] = suffixKey.Merge(key);
                    if (mergedPrefixKeys[i].SourceString != null
                        || mergedSuffixKeys[i].SourceString != null)
                    {
                        Errln("Merged source string error: expected null");
                    }
                    if (i > 0)
                    {
                        if (mergedPrefixKeys[i - 1].CompareTo(mergedPrefixKeys[i])
                            >= 0)
                        {
                            Errln("Error while comparing prefixed keys @ strength "
                                  + strength);
                            Errln(CollationTest.Prettify(mergedPrefixKeys[i - 1]));
                            Errln(CollationTest.Prettify(mergedPrefixKeys[i]));
                        }
                        if (mergedSuffixKeys[i - 1].CompareTo(mergedSuffixKeys[i])
                            >= 0)
                        {
                            Errln("Error while comparing suffixed keys @ strength "
                                  + strength);
                            Errln(CollationTest.Prettify(mergedSuffixKeys[i - 1]));
                            Errln(CollationTest.Prettify(mergedSuffixKeys[i]));
                        }
                    }
                }
                if (strength == Collator.Quaternary)
                {
                    strength = Collator.Identical;
                }
                else
                {
                    strength++;
                }
            }
        }

        [Test]
        public void TestVariableTop()
        {
            // ICU 53+: The character must be in a supported reordering group,
            // and the variable top is pinned to the end of that group.
            // parseNextToken is not released as public so i create my own rules
            String rules = "& ' ' < b < c < de < fg & hi = j";
            try
            {
                RuleBasedCollator coll = new RuleBasedCollator(rules);
                String[] tokens = { " ", "b", "c", "de", "fg", "hi", "j", "ab" };
                coll.IsAlternateHandlingShifted = (true);
                for (int i = 0; i < tokens.Length; i++)
                {
                    int varTopOriginal = coll.VariableTop;
                    try
                    {
                        int varTop = coll.SetVariableTop(tokens[i]);
                        if (i > 4)
                        {
                            Errln("Token " + tokens[i] + " expected to fail");
                        }
                        if (varTop != coll.VariableTop)
                        {
                            Errln("Error setting and getting variable top");
                        }
                        CollationKey key1 = coll.GetCollationKey(tokens[i]);
                        for (int j = 0; j < i; j++)
                        {
                            CollationKey key2 = coll.GetCollationKey(tokens[j]);
                            if (key2.CompareTo(key1) < 0)
                            {
                                Errln("Setting variable top shouldn't change the comparison sequence");
                            }
                            byte[] sortorder = key2.ToByteArray();
                            if (sortorder.Length > 0
                                && (key2.ToByteArray())[0] > 1)
                            {
                                Errln("Primary sort order should be 0");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CollationElementIterator iter
                            = coll.GetCollationElementIterator(tokens[i]);
                        /*int ce =*/
                        iter.Next();
                        int ce2 = iter.Next();
                        if (ce2 == CollationElementIterator.NullOrder)
                        {
                            Errln("Token " + tokens[i] + " not expected to fail");
                        }
                        if (coll.VariableTop != varTopOriginal)
                        {
                            Errln("When exception is thrown variable top should "
                                  + "not be changed");
                        }
                    }
                    coll.VariableTop = (varTopOriginal);
                    if (varTopOriginal != coll.VariableTop)
                    {
                        Errln("Couldn't restore old variable top\n");
                    }
                }

                // Testing calling with error set
                try
                {
                    coll.SetVariableTop("");
                    Errln("Empty string should throw an IllegalArgumentException");
                }
                catch (ArgumentException e)
                {
                    Logln("PASS: Empty string failed as expected");
                }
                try
                {
                    coll.SetVariableTop(null);
                    Errln("Null string should throw an IllegalArgumentException");
                }
                catch (ArgumentException e)
                {
                    Logln("PASS: null string failed as expected");
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating RuleBasedCollator");
            }
        }

        // ported from cmsccoll.c
        [Test]
        public void TestVariableTopSetting()
        {
            int varTopOriginal = 0, varTop1, varTop2;
            Collator coll = Collator.GetInstance(ULocale.ROOT);

            String empty = "";
            String space = " ";
            String dot = ".";  /* punctuation */
            String degree = "\u00b0";  /* symbol */
            String dollar = "$";  /* currency symbol */
            String zero = "0";  /* digit */

            varTopOriginal = coll.VariableTop;
            Logln(String.Format("coll.getVariableTop(root) -> %08x", varTopOriginal));
            ((RuleBasedCollator)coll).IsAlternateHandlingShifted = (true);

            varTop1 = coll.SetVariableTop(space);
            varTop2 = coll.VariableTop;
            Logln(String.Format("coll.setVariableTop(space) -> {0:x8}", varTop1));
            if (varTop1 != varTop2 ||
                    !coll.Equals(empty, space) ||
                    coll.Equals(empty, dot) ||
                    coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(space, dot) >= 0)
            {
                Errln("coll.setVariableTop(space) did not work");
            }

            varTop1 = coll.SetVariableTop(dot);
            varTop2 = coll.VariableTop;
            Logln(String.Format("coll.setVariableTop(dot) -> {0:x8}", varTop1));
            if (varTop1 != varTop2 ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(dot, degree) >= 0)
            {
                Errln("coll.setVariableTop(dot) did not work");
            }

            varTop1 = coll.SetVariableTop(degree);
            varTop2 = coll.VariableTop;
            Logln(String.Format("coll.setVariableTop(degree) -> %08x", varTop1));
            if (varTop1 != varTop2 ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    !coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(degree, dollar) >= 0)
            {
                Errln("coll.setVariableTop(degree) did not work");
            }

            varTop1 = coll.SetVariableTop(dollar);
            varTop2 = coll.VariableTop;
            Logln(String.Format("coll.setVariableTop(dollar) -> {0:x8}", varTop1));
            if (varTop1 != varTop2 ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    !coll.Equals(empty, degree) ||
                    !coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(dollar, zero) >= 0)
            {
                Errln("coll.setVariableTop(dollar) did not work");
            }

            Logln("Testing setting variable top to contractions");
            try
            {
                coll.SetVariableTop("@P");
                Errln("Invalid contraction succeded in setting variable top!");
            }
            catch (Exception expected)
            {
            }

            Logln("Test restoring variable top");
            coll.VariableTop = (varTopOriginal);
            if (varTopOriginal != coll.VariableTop)
            {
                Errln("Couldn't restore old variable top");
            }
        }

        // ported from cmsccoll.c
        [Test]
        public void TestMaxVariable()
        {
            int oldMax, max;

            String empty = "";
            String space = " ";
            String dot = ".";  /* punctuation */
            String degree = "\u00b0";  /* symbol */
            String dollar = "$";  /* currency symbol */
            String zero = "0";  /* digit */

            Collator coll = Collator.GetInstance(ULocale.ROOT);

            oldMax = coll.MaxVariable;
            Logln(String.Format("coll.getMaxVariable(root) -> {0:x4}", (int)oldMax));
            ((RuleBasedCollator)coll).IsAlternateHandlingShifted = (true);

            coll.MaxVariable = (ReorderCodes.Space);
            max = coll.MaxVariable;
            Logln(String.Format("coll.setMaxVariable(space) -> {0:x4}", (int)max));
            if (max != ReorderCodes.Space ||
                    !coll.Equals(empty, space) ||
                    coll.Equals(empty, dot) ||
                    coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(space, dot) >= 0)
            {
                Errln("coll.setMaxVariable(space) did not work");
            }

            coll.MaxVariable = (ReorderCodes.Punctuation);
            max = coll.MaxVariable;
            Logln(String.Format("coll.setMaxVariable(punctuation) -> {0:x4}", (int)max));
            if (max != ReorderCodes.Punctuation ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(dot, degree) >= 0)
            {
                Errln("coll.setMaxVariable(punctuation) did not work");
            }

            coll.MaxVariable = (ReorderCodes.Symbol);
            max = coll.MaxVariable;
            Logln(String.Format("coll.setMaxVariable(symbol) -> {0:x4}", (int)max));
            if (max != ReorderCodes.Symbol ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    !coll.Equals(empty, degree) ||
                    coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(degree, dollar) >= 0)
            {
                Errln("coll.setMaxVariable(symbol) did not work");
            }

            coll.MaxVariable = (ReorderCodes.Currency);
            max = coll.MaxVariable;
            Logln(String.Format("coll.setMaxVariable(currency) -> {0:x4}", (int)max));
            if (max != ReorderCodes.Currency ||
                    !coll.Equals(empty, space) ||
                    !coll.Equals(empty, dot) ||
                    !coll.Equals(empty, degree) ||
                    !coll.Equals(empty, dollar) ||
                    coll.Equals(empty, zero) ||
                    coll.Compare(dollar, zero) >= 0)
            {
                Errln("coll.setMaxVariable(currency) did not work");
            }

            Logln("Test restoring maxVariable");
            coll.MaxVariable = (oldMax);
            if (oldMax != coll.MaxVariable)
            {
                Errln("Couldn't restore old maxVariable");
            }
        }

        [Test]
        public void TestUCARules()
        {
            try
            {
                // only root locale can have empty tailorings .. not English!
                RuleBasedCollator coll
                    = (RuleBasedCollator)Collator.GetInstance(CultureInfo.InvariantCulture /*new Locale("", "", "")*/);
                String rule
                    = coll.GetRules(false);
                if (!rule.Equals(""))
                {
                    Errln("Empty rule string should have empty rules " + rule);
                }
                rule = coll.GetRules(true);
                if (rule.Equals(""))
                {
                    Errln("UCA rule string should not be empty");
                }
                coll = new RuleBasedCollator(rule);
            }
            catch (Exception e)
            {
                Warnln(e.ToString());
            }
        }

        /**
         * Jitterbug 2726
         */
        [Test]
        public void TestShifted()
        {
            RuleBasedCollator collator = (RuleBasedCollator)Collator.GetInstance();
            collator.Strength = (Collator.Primary);
            collator.IsAlternateHandlingShifted = (true);
            CollationTest.DoTest(this, collator, " a", "a", 0); // works properly
            CollationTest.DoTest(this, collator, "a", "a ", 0); // inconsistent results
        }

        /**
         * Test for CollationElementIterator previous and next for the whole set of
         * unicode characters with normalization on.
         */
        [Test]
        public void TestNumericCollation()
        {
            String[] basicTestStrings = { "hello1", "hello2", "hello123456" };
            String[] preZeroTestStrings = {"avery1",
                                           "avery01",
                                           "avery001",
                                           "avery0001"};
            String[] thirtyTwoBitNumericStrings = {"avery42949672960",
                                                   "avery42949672961",
                                                   "avery42949672962",
                                                   "avery429496729610"};

            String[] supplementaryDigits = {"\uD835\uDFCE", // 0
                                            "\uD835\uDFCF", // 1
                                            "\uD835\uDFD0", // 2
                                            "\uD835\uDFD1", // 3
                                            "\uD835\uDFCF\uD835\uDFCE", // 10
                                            "\uD835\uDFCF\uD835\uDFCF", // 11
                                            "\uD835\uDFCF\uD835\uDFD0", // 12
                                            "\uD835\uDFD0\uD835\uDFCE", // 20
                                            "\uD835\uDFD0\uD835\uDFCF", // 21
                                            "\uD835\uDFD0\uD835\uDFD0" // 22
            };

            String[] foreignDigits = {"\u0661",
                                      "\u0662",
                                      "\u0663",
                                      "\u0661\u0660",
                                      "\u0661\u0662",
                                      "\u0661\u0663",
                                      "\u0662\u0660",
                                      "\u0662\u0662",
                                      "\u0662\u0663",
                                      "\u0663\u0660",
                                      "\u0663\u0662",
                                      "\u0663\u0663"
            };

            //Additional tests to cover bug reported in #9476
            String[] lastDigitDifferent ={"2004","2005",
                                     "110005", "110006",
                                     "11005", "11006",
                                     "100000000005","100000000006"};

            // Open our collator.
            RuleBasedCollator coll
                = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH */);
            String[] att = { "NumericCollation" };
            object[] val = { true };
            genericLocaleStarterWithOptions(new CultureInfo("en")  /*Locale.ENGLISH */, basicTestStrings, att,
                                            val);
            genericLocaleStarterWithOptions(new CultureInfo("en")  /*Locale.ENGLISH */,
                                            thirtyTwoBitNumericStrings, att, val);
            genericLocaleStarterWithOptions(new CultureInfo("en")  /*Locale.ENGLISH */, foreignDigits, att,
                                            val);
            genericLocaleStarterWithOptions(new CultureInfo("en")  /*Locale.ENGLISH */, supplementaryDigits,
                                            att, val);

            // Setting up our collator to do digits.
            coll.IsNumericCollation = (true);

            // Testing that prepended zeroes still yield the correct collation
            // behavior.
            // We expect that every element in our strings array will be equal.
            for (int i = 0; i < preZeroTestStrings.Length - 1; i++)
            {
                for (int j = i + 1; j < preZeroTestStrings.Length; j++)
                {
                    CollationTest.DoTest(this, coll, preZeroTestStrings[i],
                                         preZeroTestStrings[j], 0);
                }
            }

            //Testing that the behavior reported in #9476 is fixed
            //We expect comparisons between adjacent pairs will result in -1
            for (int i = 0; i < lastDigitDifferent.Length - 1; i = i + 2)
            {
                CollationTest.DoTest(this, coll, lastDigitDifferent[i], lastDigitDifferent[i + 1], -1);
            }


            //cover setNumericCollationDefault, getNumericCollation
            assertTrue("The Numeric Collation setting is on", coll.IsNumericCollation);
            coll.SetNumericCollationToDefault();
            Logln("After set Numeric to default, the setting is: " + coll.IsNumericCollation);
        }

        [Test]
        public void Test3249()
        {
            String rule = "&x < a &z < a";
            try
            {
                RuleBasedCollator coll = new RuleBasedCollator(rule);
                if (coll != null)
                {
                    Logln("Collator did not throw an exception");
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating RuleBasedCollator with " + rule + " failed");
            }
        }

        [Test]
        public void TestTibetanConformance()
        {
            String[] test = { "\u0FB2\u0591\u0F71\u0061", "\u0FB2\u0F71\u0061" };
            try
            {
                Collator coll = Collator.GetInstance();
                coll.Decomposition = (Collator.CanonicalDecomposition);
                if (coll.Compare(test[0], test[1]) != 0)
                {
                    Errln("Tibetan comparison error");
                }
                CollationTest.DoTest(this, (RuleBasedCollator)coll,
                                     test[0], test[1], 0);
            }
            catch (Exception e)
            {
                Warnln("Error creating UCA collator");
            }
        }

        [Test]
        public void TestJ3347()
        {
            try
            {
                Collator coll = Collator.GetInstance(new CultureInfo("fr") /* Locale.FRENCH */);
                ((RuleBasedCollator)coll).IsAlternateHandlingShifted = (true);
                if (coll.Compare("6", "!6") != 0)
                {
                    Errln("Jitterbug 3347 failed");
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating UCA collator");
            }
        }

        [Test]
        public void TestPinyinProblem()
        {
            String[] test = { "\u4E56\u4E56\u7761", "\u4E56\u5B69\u5B50" };
            // ICU4N: See: https://stackoverflow.com/questions/9416435/what-culture-code-should-i-use-for-pinyin#comment11937203_9421566
            genericLocaleStarter(new CultureInfo("zh-Hans")   /* new Locale("zh", "", "PINYIN") */, test);
        }

        /* supercedes TestJ784 */
        [Test]
        public void TestBeforePinyin()
        {
            String rules =
                "&[before 2]A << \u0101  <<< \u0100 << \u00E1 <<< \u00C1 << \u01CE <<< \u01CD << \u00E0 <<< \u00C0" +
                "&[before 2]e << \u0113 <<< \u0112 << \u00E9 <<< \u00C9 << \u011B <<< \u011A << \u00E8 <<< \u00C8" +
                "&[before 2] i << \u012B <<< \u012A << \u00ED <<< \u00CD << \u01D0 <<< \u01CF << \u00EC <<< \u00CC" +
                "&[before 2] o << \u014D <<< \u014C << \u00F3 <<< \u00D3 << \u01D2 <<< \u01D1 << \u00F2 <<< \u00D2" +
                "&[before 2]u << \u016B <<< \u016A << \u00FA <<< \u00DA << \u01D4 <<< \u01D3 << \u00F9 <<< \u00D9" +
                "&U << \u01D6 <<< \u01D5 << \u01D8 <<< \u01D7 << \u01DA <<< \u01D9 << \u01DC <<< \u01DB << \u00FC";

            String[] test = {
                "l\u0101",
                "la",
                "l\u0101n",
                "lan ",
                "l\u0113",
                "le",
                "l\u0113n",
                "len"
            };

            String[] test2 = {
                "x\u0101",
                "x\u0100",
                "X\u0101",
                "X\u0100",
                "x\u00E1",
                "x\u00C1",
                "X\u00E1",
                "X\u00C1",
                "x\u01CE",
                "x\u01CD",
                "X\u01CE",
                "X\u01CD",
                "x\u00E0",
                "x\u00C0",
                "X\u00E0",
                "X\u00C0",
                "xa",
                "xA",
                "Xa",
                "XA",
                "x\u0101x",
                "x\u0100x",
                "x\u00E1x",
                "x\u00C1x",
                "x\u01CEx",
                "x\u01CDx",
                "x\u00E0x",
                "x\u00C0x",
                "xax",
                "xAx"
            };
            /* TODO: port builder fixes to before */
            genericRulesStarter(rules, test);
            genericLocaleStarter(new CultureInfo("zh"), test);
            genericRulesStarter(rules, test2);
            genericLocaleStarter(new CultureInfo("zh"), test2);
        }

        [Test]
        public void TestUpperFirstQuaternary()
        {
            String[] tests = { "B", "b", "Bb", "bB" };
            String[] att = { "strength", "UpperFirst" };
            Object[] attVals = { new int?((int)Collator.Quaternary), true };
            genericLocaleStarterWithOptions(CultureInfo.InvariantCulture /* new Locale("root", "", "") */, tests, att, attVals);
        }

        [Test]
        public void TestJ4960()
        {
            String[] tests = { "\\u00e2T", "aT" };
            String[] att = { "strength", "CaseLevel" };
            Object[] attVals = { new int?((int)Collator.Primary), true };
            String[] tests2 = { "a", "A" };
            String rule = "&[first tertiary ignorable]=A=a";
            String[] att2 = { "CaseLevel" };
            Object[] attVals2 = { true };
            // Test whether we correctly ignore primary ignorables on case level when
            // we have only primary & case level
            genericLocaleStarterWithOptionsAndResult(CultureInfo.InvariantCulture /* new Locale("root", "") */, tests, att, attVals, 0);
            // Test whether ICU4J will make case level for sortkeys that have primary strength
            // and case level
            genericLocaleStarterWithOptions(CultureInfo.InvariantCulture /* new Locale("root", "") */, tests2, att, attVals);
            // Test whether completely ignorable letters have case level info (they shouldn't)
            genericRulesStarterWithOptionsAndResult(rule, tests2, att2, attVals2, 0);
        }

        [Test]
        public void TestJB5298()
        {
            ULocale[] locales = Collator.GetAvailableULocales();
            Logln("Number of collator locales returned : " + locales.Length);
            // double-check keywords
            String[] keywords = Collator.Keywords.ToArray();
            if (keywords.Length != 1 || !keywords[0].Equals("collation"))
            {
                throw new ArgumentException("internal collation error");
            }

            String[] values = Collator.GetKeywordValues("collation");
            Log("Collator.getKeywordValues returned: ");
            for (int i = 0; i < values.Length; i++)
            {
                Log(values[i] + ", ");
            }
            Logln("");
            Logln("Number of collation keyword values returned : " + values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].StartsWith("private-", StringComparison.Ordinal))
                {
                    Errln("Collator.getKeywordValues() returns private collation keyword: " + values[i]);
                }
            }

            var foundValues = new SortedSet<string>(values, StringComparer.Ordinal);

            for (int i = 0; i < locales.Length; ++i)
            {
                for (int j = 0; j < values.Length; ++j)
                {
                    ULocale tryLocale = values[j].Equals("standard")
                    ? locales[i] : new ULocale(locales[i] + "@collation=" + values[j]);
                    // only append if not standard
                    ULocale canon = Collator.GetFunctionalEquivalent("collation", tryLocale);
                    if (!canon.Equals(tryLocale))
                    {
                        continue; // has a different
                    }
                    else
                    {// functional equivalent, so skip
                        Logln(tryLocale + " : " + canon + ", ");
                    }
                    String can = canon.ToString();
                    int idx = can.IndexOf("@collation=", StringComparison.Ordinal);
                    String val = idx >= 0 ? can.Substring(idx + 11, can.Length - (idx + 11)) : ""; // ICU4N: Corrected 2nd substring parameter
                    if (val.Length > 0 && !foundValues.Contains(val))
                    {
                        Errln("Unknown collation found " + can);
                    }
                }
            }
            Logln(" ");
        }

        [Test]
        public void TestJ5367()
        {
            String[] test = { "a", "y" };
            String rules = "&Ny << Y &[first secondary ignorable] <<< a";
            genericRulesStarter(rules, test);
        }

        [Test]
        public void TestVI5913()
        {

            String[] rules = {
                    "&a < \u00e2 <<< \u00c2",
                    "&a < \u1FF3 ",  // OMEGA WITH YPOGEGRAMMENI
                    "&s < \u0161 ",  // &s < s with caron
                    /*
                     * Note: Just tailoring &z<ae^ does not work as expected:
                     * The UCA spec requires for discontiguous contractions that they
                     * extend an *existing match* by one combining mark at a time.
                     * Therefore, ae must be a contraction so that the builder finds
                     * discontiguous contractions for ae^, for example with an intervening underdot.
                     * Only then do we get the expected tail closure with a\u1EC7, a\u1EB9\u0302, etc.
                     */
                    "&x < ae &z < a\u00EA",  // &x < ae &z < a+e with circumflex
            };
            String[][] cases = {
                new string[] { "\u1EAC", "A\u0323\u0302", "\u1EA0\u0302", "\u00C2\u0323", },
                new string[] { "\u1FA2", "\u03C9\u0313\u0300\u0345", "\u1FF3\u0313\u0300",
                  "\u1F60\u0300\u0345", "\u1f62\u0345", "\u1FA0\u0300", },
                new string[] { "\u1E63\u030C", "s\u0323\u030C", "s\u030C\u0323"},
                new string[] { "a\u1EC7", //  a+ e with dot below and circumflex
                  "a\u1EB9\u0302", // a + e with dot below + combining circumflex
                  "a\u00EA\u0323", // a + e with circumflex + combining dot below
                }
            };


            for (int i = 0; i < rules.Length; i++)
            {

                RuleBasedCollator coll = null;
                try
                {
                    coll = new RuleBasedCollator(rules[i]);
                }
                catch (Exception e)
                {
                    Warnln("Unable to open collator with rules " + rules[i]);
                }

                Logln("Test case[" + i + "]:");
                CollationKey expectingKey = coll.GetCollationKey(cases[i][0]);
                for (int j = 1; j < cases[i].Length; j++)
                {
                    CollationKey key = coll.GetCollationKey(cases[i][j]);
                    if (key.CompareTo(expectingKey) != 0)
                    {
                        Errln("Error! Test case[" + i + "]:" + "source:" + key.SourceString);
                        Errln("expecting:" + CollationTest.Prettify(expectingKey) + "got:" + CollationTest.Prettify(key));
                    }
                    Logln("   Key:" + CollationTest.Prettify(key));
                }
            }


            RuleBasedCollator vi_vi = null;
            try
            {
                vi_vi = (RuleBasedCollator)Collator.GetInstance(
                                                          new CultureInfo("vi"));
                Logln("VI sort:");
                CollationKey expectingKey = vi_vi.GetCollationKey(cases[0][0]);
                for (int j = 1; j < cases[0].Length; j++)
                {
                    CollationKey key = vi_vi.GetCollationKey(cases[0][j]);
                    if (key.CompareTo(expectingKey) != 0)
                    {
                        // TODO (claireho): change the Logln to Errln after vi.res is up-to-date.
                        // Errln("source:" + key.SourceString);
                        // Errln("expecting:"+prettify(expectingKey)+ "got:"+  prettify(key));
                        Logln("Error!! in Vietnese sort - source:" + key.SourceString);
                        Logln("expecting:" + CollationTest.Prettify(expectingKey) + "got:" + CollationTest.Prettify(key));
                    }
                    // Logln("source:" + key.SourceString);
                    Logln("   Key:" + CollationTest.Prettify(key));
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating Vietnese collator");
                return;
            }

        }


        [Test]
        public void Test6179()
        {
            String[] rules = {
                    "&[last primary ignorable]<< a  &[first primary ignorable]<<b ",
                    "&[last secondary ignorable]<<< a &[first secondary ignorable]<<<b",
            };
            // defined in UCA5.1
            String firstPrimIgn = "\u0332";
            String lastPrimIgn = "\uD800\uDDFD";
            String firstVariable = "\u0009";
            byte[] secIgnKey = { 1, 1, 4, 0 };

            int i = 0;
            {

                RuleBasedCollator coll = null;
                try
                {
                    coll = new RuleBasedCollator(rules[i]);
                }
                catch (Exception e)
                {
                    Warnln("Unable to open collator with rules " + rules[i] + ": " + e);
                    return;
                }

                Logln("Test rule[" + i + "]" + rules[i]);

                CollationKey keyA = coll.GetCollationKey("a");
                Logln("Key for \"a\":" + CollationTest.Prettify(keyA));
                if (keyA.CompareTo(coll.GetCollationKey(lastPrimIgn)) <= 0)
                {
                    CollationKey key = coll.GetCollationKey(lastPrimIgn);
                    Logln("Collation key for 0xD800 0xDDFD: " + CollationTest.Prettify(key));
                    Errln("Error! String \"a\" must be greater than \uD800\uDDFD -" +
                          "[Last Primary Ignorable]");
                }
                if (keyA.CompareTo(coll.GetCollationKey(firstVariable)) >= 0)
                {
                    CollationKey key = coll.GetCollationKey(firstVariable);
                    Logln("Collation key for 0x0009: " + CollationTest.Prettify(key));
                    Errln("Error! String \"a\" must be less than 0x0009 - [First Variable]");
                }
                CollationKey keyB = coll.GetCollationKey("b");
                Logln("Key for \"b\":" + CollationTest.Prettify(keyB));
                if (keyB.CompareTo(coll.GetCollationKey(firstPrimIgn)) <= 0)
                {
                    CollationKey key = coll.GetCollationKey(firstPrimIgn);
                    Logln("Collation key for 0x0332: " + CollationTest.Prettify(key));
                    Errln("Error! String \"b\" must be greater than 0x0332 -" +
                          "[First Primary Ignorable]");
                }
                if (keyB.CompareTo(coll.GetCollationKey(firstVariable)) >= 0)
                {
                    CollationKey key = coll.GetCollationKey(firstVariable);
                    Logln("Collation key for 0x0009: " + CollationTest.Prettify(key));
                    Errln("Error! String \"b\" must be less than 0x0009 - [First Variable]");
                }
            }
            {
                i = 1;
                RuleBasedCollator coll = null;
                try
                {
                    coll = new RuleBasedCollator(rules[i]);
                }
                catch (Exception e)
                {
                    Warnln("Unable to open collator with rules " + rules[i]);
                }

                Logln("Test rule[" + i + "]" + rules[i]);

                CollationKey keyA = coll.GetCollationKey("a");
                Logln("Key for \"a\":" + CollationTest.Prettify(keyA));
                byte[] keyAInBytes = keyA.ToByteArray();
                for (int j = 0; j < keyAInBytes.Length && j < secIgnKey.Length; j++)
                {
                    if (keyAInBytes[j] != secIgnKey[j])
                    {
                        if ((char)keyAInBytes[j] <= (char)secIgnKey[j])
                        {
                            Logln("Error! String \"a\" must be greater than [Last Secondary Ignorable]");
                        }
                        break;
                    }
                }
                if (keyA.CompareTo(coll.GetCollationKey(firstVariable)) >= 0)
                {
                    Errln("Error! String \"a\" must be less than 0x0009 - [First Variable]");
                    CollationKey key = coll.GetCollationKey(firstVariable);
                    Logln("Collation key for 0x0009: " + CollationTest.Prettify(key));
                }
                CollationKey keyB = coll.GetCollationKey("b");
                Logln("Key for \"b\":" + CollationTest.Prettify(keyB));
                byte[] keyBInBytes = keyB.ToByteArray();
                for (int j = 0; j < keyBInBytes.Length && j < secIgnKey.Length; j++)
                {
                    if (keyBInBytes[j] != secIgnKey[j])
                    {
                        if ((char)keyBInBytes[j] <= (char)secIgnKey[j])
                        {
                            Errln("Error! String \"b\" must be greater than [Last Secondary Ignorable]");
                        }
                        break;
                    }
                }
                if (keyB.CompareTo(coll.GetCollationKey(firstVariable)) >= 0)
                {
                    CollationKey key = coll.GetCollationKey(firstVariable);
                    Logln("Collation key for 0x0009: " + CollationTest.Prettify(key));
                    Errln("Error! String \"b\" must be less than 0x0009 - [First Variable]");
                }
            }
        }

        [Test]
        public void TestUCAPrecontext()
        {
            String[] rules = {
                    "& \u00B7<a ",
                    "& L\u00B7 << a", // 'a' is an expansion.
            };
            String[] cases = {
                "\u00B7",
                "\u0387",
                "a",
                "l",
                "L\u0332",
                "l\u00B7",
                "l\u0387",
                "L\u0387",
                "la\u0387",
                "La\u00b7",
            };

            // Test en sort
            RuleBasedCollator en = null;

            Logln("EN sort:");
            try
            {
                en = (RuleBasedCollator)Collator.GetInstance(
                        new CultureInfo("en"));
                for (int j = 0; j < cases.Length; j++)
                {
                    CollationKey key = en.GetCollationKey(cases[j]);
                    if (j > 0)
                    {
                        CollationKey prevKey = en.GetCollationKey(cases[j - 1]);
                        if (key.CompareTo(prevKey) < 0)
                        {
                            Errln("Error! EN test[" + j + "]:source:" + cases[j] +
                            " is not >= previous test string.");
                        }
                    }
                    /*
                    if ( key.compareTo(expectingKey)!=0) {
                        Errln("Error! Test case["+i+"]:"+"source:" + key.SourceString);
                        Errln("expecting:"+prettify(expectingKey)+ "got:"+  prettify(key));
                    }
                    */
                    Logln("String:" + cases[j] + "   Key:" + CollationTest.Prettify(key));
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating English collator");
                return;
            }

            // Test ja sort
            RuleBasedCollator ja = null;
            Logln("JA sort:");
            try
            {
                ja = (RuleBasedCollator)Collator.GetInstance(
                        new CultureInfo("ja"));
                for (int j = 0; j < cases.Length; j++)
                {
                    CollationKey key = ja.GetCollationKey(cases[j]);
                    if (j > 0)
                    {
                        CollationKey prevKey = ja.GetCollationKey(cases[j - 1]);
                        if (key.CompareTo(prevKey) < 0)
                        {
                            Errln("Error! JA test[" + j + "]:source:" + cases[j] +
                            " is not >= previous test string.");
                        }
                    }
                    Logln("String:" + cases[j] + "   Key:" + CollationTest.Prettify(key));
                }
            }
            catch (Exception e)
            {
                Warnln("Error creating Japanese collator");
                return;
            }
            for (int i = 0; i < rules.Length; i++)
            {

                RuleBasedCollator coll = null;
                Logln("Tailoring rule:" + rules[i]);
                try
                {
                    coll = new RuleBasedCollator(rules[i]);
                }
                catch (Exception e)
                {
                    Warnln("Unable to open collator with rules " + rules[i]);
                    continue;
                }

                for (int j = 0; j < cases.Length; j++)
                {
                    CollationKey key = coll.GetCollationKey(cases[j]);
                    if (j > 0)
                    {
                        CollationKey prevKey = coll.GetCollationKey(cases[j - 1]);
                        if (i == 1 && j == 3)
                        {
                            if (key.CompareTo(prevKey) > 0)
                            {
                                Errln("Error! Rule:" + rules[i] + " test[" + j + "]:source:" +
                                cases[j] + " is not <= previous test string.");
                            }
                        }
                        else
                        {
                            if (key.CompareTo(prevKey) < 0)
                            {
                                Errln("Error! Rule:" + rules[i] + " test[" + j + "]:source:" +
                                cases[j] + " is not >= previous test string.");
                            }
                        }
                    }
                    Logln("String:" + cases[j] + "   Key:" + CollationTest.Prettify(key));
                }
            }
        }


        /**
         * Stores a test case for collation testing.
         */
        private class OneTestCase
        {
            /** The first value to compare.  **/
            public String m_source_;

            /** The second value to compare. **/
            public String m_target_;

            /**
             *  0 if the two values sort equal,
             * -1 if the first value sorts before the second
             *  1 if the first value sorts after the first
             */
            public int m_result_;

            public OneTestCase(String source, String target, int result)
            {
                m_source_ = source;
                m_target_ = target;
                m_result_ = result;
            }
        }

        /**
         * Convenient function to test collation rules.
         * @param testCases
         * @param rules Collation rules in ICU format.  All the strings in this
         *     array represent the same rule, expressed in different forms.
         */
        private void doTestCollation(
            OneTestCase[] testCases, String[] rules)
        {

            Collator myCollation;
            foreach (String rule in rules)
            {
                try
                {
                    myCollation = new RuleBasedCollator(rule);
                }
                catch (Exception e)
                {
                    Warnln("ERROR: in creation of rule based collator: " + e);
                    return;
                }

                myCollation.Decomposition = (Collator.CanonicalDecomposition);
                myCollation.Strength = (Collator.Tertiary);
                foreach (OneTestCase testCase in testCases)
                {
                    CollationTest.DoTest(this, (RuleBasedCollator)myCollation,
                                         testCase.m_source_,
                                         testCase.m_target_,
                                         testCase.m_result_);
                }
            }
        }

        // Test cases to check whether the rules equivalent to
        // "&a<b<c<d &b<<k<<l<<m &k<<<x<<<y<<<z &a=1=2=3" are working fine.
        private OneTestCase[] m_rangeTestCases_ = {
            //               Left                  Right             Result
            new OneTestCase( "\u0061",             "\u0062",             -1 ),  // "a" < "b"
            new OneTestCase( "\u0062",             "\u0063",             -1 ),  // "b" < "c"
            new OneTestCase( "\u0061",             "\u0063",             -1 ),  // "a" < "c"

            new OneTestCase( "\u0062",             "\u006b",             -1 ),  // "b" << "k"
            new OneTestCase( "\u006b",             "\u006c",             -1 ),  // "k" << "l"
            new OneTestCase( "\u0062",             "\u006c",             -1 ),  // "b" << "l"
            new OneTestCase( "\u0061",             "\u006c",             -1 ),  // "a" << "l"
            new OneTestCase( "\u0061",             "\u006d",             -1 ),  // "a" << "m"

            new OneTestCase( "\u0079",             "\u006d",             -1 ),  // "y" < "f"
            new OneTestCase( "\u0079",             "\u0067",             -1 ),  // "y" < "g"
            new OneTestCase( "\u0061",             "\u0068",             -1 ),  // "y" < "h"
            new OneTestCase( "\u0061",             "\u0065",             -1 ),  // "g" < "e"

            new OneTestCase( "\u0061",             "\u0031",              0 ),   // "a" == "1"
            new OneTestCase( "\u0061",             "\u0032",              0 ),   // "a" == "2"
            new OneTestCase( "\u0061",             "\u0033",              0 ),   // "a" == "3"
            new OneTestCase( "\u0061",             "\u0066",             -1 ),   // "a" < "f",
            new OneTestCase( "\u006c\u0061",       "\u006b\u0062",       -1 ),  // "la" < "kb"
            new OneTestCase( "\u0061\u0061\u0061", "\u0031\u0032\u0033",  0 ),  // "aaa" == "123"
            new OneTestCase( "\u0062",             "\u007a",             -1 ),  // "b" < "z"
            new OneTestCase( "\u0061\u007a\u0062", "\u0032\u0079\u006d", -1 ),  // "azm" < "2yc"
        };

        // Test cases to check whether the rules equivalent to
        // "&\ufffe<\uffff<\U00010000<\U00010001<\U00010002
        //  &\U00010000<<\U00020001<<\U00020002<<\U00020002
        //  &\U00020001=\U0003001=\U0004001=\U0004002
        //  &\U00040008<\U00030008<\UU00020008"
        // are working fine.
        private OneTestCase[] m_rangeTestCasesSupplemental_ = {
            //               Left                Right               Result
            new OneTestCase( "\u4e00",           "\ufffb",             -1 ),
            new OneTestCase( "\ufffb",           "\ud800\udc00",       -1 ),  // U+FFFB < U+10000
            new OneTestCase( "\ud800\udc00",    "\ud800\udc01",        -1 ),  // U+10000 < U+10001

            new OneTestCase( "\u4e00",           "\ud800\udc01",       -1 ),  // U+4E00 < U+10001
            new OneTestCase( "\ud800\udc01",    "\ud800\udc02",        -1 ),  // U+10001 < U+10002
            new OneTestCase( "\ud800\udc00",    "\ud840\udc02",        -1 ),  // U+10000 < U+10002
            new OneTestCase( "\u4e00",           "\u0d840\udc02",      -1 ),  // U+4E00 < U+10002

        };

        // Test cases in disjoint random code points.  To test only the compact syntax.
        // Rule:  &q<w<e<r &w<<t<<y<<u &t<<<i<<<o<<<p &o=a=s=d
        private OneTestCase[] m_qwertCollationTestCases_ = {
            new OneTestCase("q", "w" , -1),
            new OneTestCase("w", "e" , -1),

            new OneTestCase("y", "u" , -1),
            new OneTestCase("q", "u" , -1),

            new OneTestCase("t", "i" , -1),
            new OneTestCase("o", "p" , -1),

            new OneTestCase("y", "e" , -1),
            new OneTestCase("i", "u" , -1),

            new OneTestCase("quest", "were" , -1),
            new OneTestCase("quack", "quest", -1)
        };

        // Tests the compact list with ASCII codepoints.
        [Test]
        public void TestSameStrengthList()
        {
            String[] rules = new String[] {
                // Normal
                "&a<b<c<d &b<<k<<l<<m &k<<<x<<<y<<<z &y<f<g<h<e &a=1=2=3",

                // Lists
                "&a<*bcd &b<<*klm &k<<<*xyz &y<*fghe &a=*123",

                // Lists with quoted characters
                "&'\u0061'<*bcd &b<<*klm &k<<<*xyz &y<*f'\u0067\u0068'e &a=*123",
            };
            doTestCollation(m_rangeTestCases_, rules);
        }

        [Test]
        public void TestSameStrengthListQuoted()
        {
            String[] rules = new String[] {
                "&'\u0061'<*bcd &b<<*klm &k<<<*xyz &y<*f'\u0067\u0068'e &a=1=2=3",
                "&'\u0061'<*b'\u0063'd &b<<*klm &k<<<*xyz &'\u0079'<*fgh'\u0065' " +
                "&a=*'\u0031\u0032\u0033'",

                "&'\u0061'<*'\u0062'c'\u0064' &b<<*klm &k<<<*xyz  &y<*fghe " +
                "&a=*'\u0031\u0032\u0033'",
            };
            doTestCollation(m_rangeTestCases_, rules);
        }

        // Tests the compact list with ASCII codepoints in non-codepoint order.
        [Test]
        public void TestSameStrengthListQwerty()
        {
            String[] rules = new String[] {
                "&q<w<e<r &w<<t<<y<<u &t<<<i<<<o<<<p &o=a=s=d",   // Normal
                "&q<*wer &w<<*tyu &t<<<*iop &o=*asd",             // Lists
            };

            doTestCollation(m_qwertCollationTestCases_, rules);
        }

        // Tests the compact list with supplemental codepoints.
        [Test]
        public void TestSameStrengthListWithSupplementalCharacters()
        {
            String[] rules = new String[] {
                // ** Rule without compact list syntax **
                // \u4e00 < \ufffb < \U00010000    < \U00010001  < \U00010002
                "&\u4e00<\ufffb<'\ud800\udc00'<'\ud800\udc01'<'\ud800\udc02' " +
                // \U00010000    << \U00020001   << \U00020002       \U00020002
                "&'\ud800\udc00'<<'\ud840\udc01'<<'\ud840\udc02'<<'\ud840\udc02'  " +
                // \U00020001   = \U0003001    = \U0004001    = \U0004002
                "&'\ud840\udc01'='\ud880\udc01'='\ud8c0\udc01'='\ud8c0\udc02'",

                // ** Rule with compact list syntax **
                // \u4e00 <* \ufffb\U00010000  \U00010001
                "&\u4e00<*'\ufffb\ud800\udc00\ud800\udc01\ud800\udc02' " +
                // \U00010000   <<* \U00020001  \U00020002
                "&'\ud800\udc00'<<*'\ud840\udc01\ud840\udc02\ud840\udc03'  " +
                // \U00020001   =* \U0003001   \U0003002   \U0003003   \U0004001
                "&'\ud840\udc01'=*'\ud880\udc01\ud880\udc02\ud880\udc03\ud8c0\udc01' "

            };
            doTestCollation(m_rangeTestCasesSupplemental_, rules);
        }


        // Tests the compact range syntax with ASCII codepoints.
        [Test]
        public void TestSameStrengthListRanges()
        {
            String[] rules = new String[] {
                // Ranges
                "&a<*b-d &b<<*k-m &k<<<*x-z &y<*f-he &a=*1-3",

                // Ranges with quoted characters
                "&'\u0061'<*'\u0062'-'\u0064' &b<<*klm &k<<<*xyz " +
                "&'\u0079'<*'\u0066'-'\u0068e' &a=*123",
                "&'\u0061'<*'\u0062'-'\u0064' " +
                "&b<<*'\u006B'-m &k<<<*x-'\u007a' " +
                "&'\u0079'<*'\u0066'-h'\u0065' &a=*'\u0031\u0032\u0033'",
            };

            doTestCollation(m_rangeTestCases_, rules);
        }

        // Tests the compact range syntax with supplemental codepoints.
        [Test]
        public void TestSameStrengthListRangesWithSupplementalCharacters()
        {
            String[] rules = new String[] {
                // \u4e00 <* \ufffb\U00010000  \U00010001
                "&\u4e00<*'\ufffb'\ud800\udc00-'\ud800\udc02' " +
                // \U00010000   <<* \U00020001   - \U00020003
                "&'\ud800\udc00'<<*'\ud840\udc01'-'\ud840\udc03'  " +
                // \U00020001   =* \U0003001   \U0004001
                "&'\ud840\udc01'=*'\ud880\udc01'-'\ud880\udc03\ud8c0\udc01' "
            };
            doTestCollation(m_rangeTestCasesSupplemental_, rules);
        }

        // Tests the compact range syntax with special characters used as syntax characters in rules.
        [Test]
        public void TestSpecialCharacters()
        {
            String[] rules = new String[] {
                    // Normal
                    "&';'<'+'<','<'-'<'&'<'*'",

                    // List
                    "&';'<*'+,-&*'",

                    // Range
                    "&';'<*'+'-'-&*'",

                    "&'\u003b'<'\u002b'<'\u002c'<'\u002d'<'\u0026'<'\u002a'",

                    "&'\u003b'<*'\u002b\u002c\u002d\u0026\u002a'",
                    "&'\u003b'<*'\u002b\u002c\u002d\u0026\u002a'",
                    "&'\u003b'<*'\u002b'-'\u002d\u0026\u002a'",
                    "&'\u003b'<*'\u002b'-'\u002d\u0026\u002a'",
            };
            OneTestCase[] testCases = new OneTestCase[] {
                new OneTestCase("\u003b", "\u002b", -1), // ; < +
                new OneTestCase("\u002b", "\u002c", -1), // + < ,
                new OneTestCase("\u002c", "\u002d", -1), // , < -
                new OneTestCase("\u002d", "\u0026", -1), // - < &
            };
            doTestCollation(testCases, rules);
        }

        [Test]
        public void TestInvalidListsAndRanges()
        {
            String[] invalidRules = new String[] {
                // Range not in starred expression
                "&\u4e00<\ufffb-'\ud800\udc02'",

                // Range without start
                "&a<*-c",

                // Range without end
                "&a<*b-",

                // More than one hyphen
                "&a<*b-g-l",

                // Range in the wrong order
                "&a<*k-b",
            };
            foreach (String rule in invalidRules)
            {
                try
                {
                    Collator myCollation = new RuleBasedCollator(rule);
                    Warnln("ERROR: Creation of collator didn't fail for " + rule + " when it should.");
                    CollationTest.DoTest(this, (RuleBasedCollator)myCollation,
                            "x",
                            "y",
                            -1);

                }
                catch (Exception e)
                {
                    continue;
                }
                throw new ArgumentException("ERROR: Invalid collator with rule " + rule + " worked fine.");
            }
        }

        // This is the same example above with ' and space added.
        // They work a little different than expected.  Desired rules are commented out.
        [Test]
        public void TestQuoteAndSpace()
        {
            String[] rules = new String[] {
                    // These are working as expected.
                    "&';'<'+'<','<'-'<'&'<''<'*'<' '",

                    // List.  Desired rule is
                    // "&';'<*'+,-&''* '",
                    // but it doesn't work.  Instead, '' should be outside quotes as below.
                    "&';'<*'+,-&''''* '",

                    // Range.  Similar issues here as well.  The following are working.
                    //"&';'<*'+'-'-&''* '",
                    //"&';'<*'+'-'-&'\\u0027'* '",
                    "&';'<*'+'-'-&''''* '",
                    //"&';'<*'+'-'-&'\\u0027'* '",

                    // The following rules are not working.
                    // "&';'<'+'<','<'-'<'&'<\\u0027<'*'<' '",
                    //"&'\u003b'<'\u002b'<'\u002c'<'\u002d'<'\u0026'<'\u0027'<\u002a'<'\u0020'",
                    //"&'\u003b'<'\u002b'<'\u002c'<'\u002d'<'\u0026'<\\u0027<\u002a'<'\u0020'",
            };

            OneTestCase[] testCases = new OneTestCase[] {
                new OneTestCase("\u003b", "\u002b", -1), // ; < ,
                new OneTestCase("\u002b", "\u002c", -1), // ; < ,
                new OneTestCase("\u002c", "\u002d", -1), // , < -
                new OneTestCase("\u002d", "\u0026", -1), // - < &
                new OneTestCase("\u0026", "\u0027", -1), // & < '
                new OneTestCase("\u0027", "\u002a", -1), // ' < *
                // new OneTestCase("\u002a", "\u0020", -1), // * < <space>
            };
            doTestCollation(testCases, rules);
        }

        /*
         * Tests the method public bool equals(Object target) in CollationKey
         */
        [Test]
        public void TestCollationKeyEquals()
        {
            CollationKey ck = new CollationKey("", (byte[])null);

            // Tests when "if (!(target instanceof CollationKey))" is true
            if (ck.Equals(new Object()))
            {
                Errln("CollationKey.Equals() was not suppose to return false "
                        + "since it is comparing to a non Collation Key object.");
            }
            if (ck.Equals(""))
            {
                Errln("CollationKey.Equals() was not suppose to return false "
                        + "since it is comparing to a non Collation Key object.");
            }
            if (ck.Equals(0))
            {
                Errln("CollationKey.Equals() was not suppose to return false "
                        + "since it is comparing to a non Collation Key object.");
            }
            if (ck.Equals(0.0))
            {
                Errln("CollationKey.Equals() was not suppose to return false "
                        + "since it is comparing to a non Collation Key object.");
            }

            // Tests when "if (target == null)" is true
            if (ck.Equals((CollationKey)null))
            {
                Errln("CollationKey.Equals() was not suppose to return false "
                        + "since it is comparing to a null Collation Key object.");
            }
        }

        /*
         * Tests the method public int hashCode() in CollationKey
         */
        [Test]
        public void TestCollationKeyHashCode()
        {
            CollationKey ck = new CollationKey("", (byte[])null);

            // Tests when "if (m_key_ == null)" is true
            if (ck.GetHashCode() != 1)
            {
                Errln("CollationKey.hashCode() was suppose to return 1 "
                        + "when m_key is null due a null parameter in the " + "constructor.");
            }
        }

        /*
         * Tests the method public CollationKey getBound(int boundType, int noOfLevels)
         */
        [Test]
        public void TestGetBound()
        {
            CollationKey ck = new CollationKey("", (byte[])null);

            // Tests when "if (noOfLevels > Collator.PRIMARY)" is false
            // Tests when "default: " is true for "switch (boundType)"
            try
            {
                ck.GetBound((CollationKeyBoundMode)Enum.GetNames(typeof(CollationKeyBoundMode)).Length, (CollationStrength)(-1));
                Errln("CollationKey.getBound(int,int) was suppose to return an "
                        + "exception for an invalid boundType value.");
            }
            catch (Exception e)
            {
            }

            // Tests when "if (noOfLevels > 0)"
            byte[] b = { };
            CollationKey ck1 = new CollationKey("", b);
            try
            {
                ck1.GetBound((CollationKeyBoundMode)0, (CollationStrength)1);
                Errln("CollationKey.getBound(int,int) was suppose to return an "
                        + "exception a value of noOfLevels that exceeds expected.");
            }
            catch (Exception e)
            {
            }
        }

        /*
         * Tests the method public CollationKey merge(CollationKey source)
         */
        [Test]
        public void TestMerge()
        {
            byte[] b = { };
            CollationKey ck = new CollationKey("", b);

            // Tests when "if (source == null || source.getLength() == 0)" is true
            try
            {
                ck.Merge(null);
                Errln("Collationkey.Merge(CollationKey) was suppose to return " + "an exception for a null parameter.");
            }
            catch (Exception e)
            {
            }
            try
            {
                ck.Merge(ck);
                Errln("Collationkey.Merge(CollationKey) was suppose to return " + "an exception for a null parameter.");
            }
            catch (Exception e)
            {
            }
        }

        /* Test the method public int compareTo(RawCollationKey rhs) */
        [Test]
        public void TestRawCollationKeyCompareTo()
        {
            RawCollationKey rck = new RawCollationKey();
            byte[] b = { (byte)10, (byte)20 };
            RawCollationKey rck100 = new RawCollationKey(b, 2);

            if (rck.CompareTo(rck) != 0)
            {
                Errln("RawCollatonKey.compareTo(RawCollationKey) was suppose to return 0 " +
                        "for two idential RawCollationKey objects.");
            }

            if (rck.CompareTo(rck100) == 0)
            {
                Errln("RawCollatonKey.compareTo(RawCollationKey) was not suppose to return 0 " +
                        "for two different RawCollationKey objects.");
            }
        }

        /* Track7223: CollationElementIterator does not return correct order for Hungarian */
        [Test]
        public void TestHungarianTailoring()
        {
            String rules = "&DZ<dzs<<<Dzs<<<DZS" +
                                      "&G<gy<<<Gy<<<GY" +
                                      "&L<ly<<<Ly<<<LY" +
                                      "&N<ny<<<Ny<<<NY" +
                                      "&S<sz<<<Sz<<<SZ" +
                                      "&T<ty<<<Ty<<<TY" +
                                      "&Z<zs<<<Zs<<<ZS" +
                                      "&O<\u00f6<<<\u00d6<<\u0151<<<\u0150" +
                                      "&U<\u00fc<<<\u00dc<<\u0171<<<\u0171" +
                                      "&cs<<<ccs/cs" +
                                      "&Cs<<<Ccs/cs" +
                                      "&CS<<<CCS/CS" +
                                      "&dz<<<ddz/dz" +
                                      "&Dz<<<Ddz/dz" +
                                      "&DZ<<<DDZ/DZ" +
                                      "&dzs<<<ddzs/dzs" +
                                      "&Dzs<<<Ddzs/dzs" +
                                      "&DZS<<<DDZS/DZS" +
                                      "&gy<<<ggy/gy" +
                                      "&Gy<<<Ggy/gy" +
                                      "&GY<<<GGY/GY";
            RuleBasedCollator coll;
            try
            {
                String str1 = "ggy";
                String str2 = "GGY";
                coll = new RuleBasedCollator(rules);
                if (coll.Compare("ggy", "GGY") >= 0)
                {
                    Errln("TestHungarianTailoring.compare(" + str1 + "," + str2 +
                          ") was suppose to return -1 ");
                }
                CollationKey sortKey1 = coll.GetCollationKey(str1);
                CollationKey sortKey2 = coll.GetCollationKey(str2);
                if (sortKey1.CompareTo(sortKey2) >= 0)
                {
                    Errln("TestHungarianTailoring getCollationKey(\"" + str1 + "\") was suppose " +
                          "less than getCollationKey(\"" + str2 + "\").");
                    Errln("  getCollationKey(\"ggy\"):" + CollationTest.Prettify(sortKey1) +
                          "  getCollationKey(\"GGY\"):" + CollationTest.Prettify(sortKey2));
                }

                CollationElementIterator iter1 = coll.GetCollationElementIterator(str1);
                CollationElementIterator iter2 = coll.GetCollationElementIterator(str2);
                int ce1, ce2;
                while ((ce1 = iter1.Next()) != CollationElementIterator.NullOrder &&
                      (ce2 = iter2.Next()) != CollationElementIterator.NullOrder)
                {
                    if (ce1 > ce2)
                    {
                        Errln("TestHungarianTailoring.CollationElementIterator(" + str1 +
                            "," + str2 + ") was suppose to return -1 ");
                    }
                }
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
            }
        }

        [Test]
        public void TestImport()
        {
            try
            {
                RuleBasedCollator vicoll = (RuleBasedCollator)Collator.GetInstance(new ULocale("vi"));
                RuleBasedCollator escoll = (RuleBasedCollator)Collator.GetInstance(new ULocale("es"));
                RuleBasedCollator viescoll = new RuleBasedCollator(vicoll.GetRules() + escoll.GetRules());
                RuleBasedCollator importviescoll = new RuleBasedCollator("[import vi][import es]");

                UnicodeSet tailoredSet = viescoll.GetTailoredSet();
                UnicodeSet importTailoredSet = importviescoll.GetTailoredSet();

                if (!tailoredSet.Equals(importTailoredSet))
                {
                    Warnln("Tailored set not equal");
                }

                for (UnicodeSetIterator it = new UnicodeSetIterator(tailoredSet); it.Next();)
                {
                    String t = it.GetString();
                    CollationKey sk1 = viescoll.GetCollationKey(t);
                    CollationKey sk2 = importviescoll.GetCollationKey(t);
                    if (!sk1.Equals(sk2))
                    {
                        Warnln("Collation key's not equal for " + t);
                    }
                }

            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
            }
        }

        [Test]
        public void TestImportWithType()
        {
            try
            {
                RuleBasedCollator vicoll = (RuleBasedCollator)Collator.GetInstance(new ULocale("vi"));
                RuleBasedCollator decoll = (RuleBasedCollator)Collator.GetInstance(ULocale.ForLanguageTag("de-u-co-phonebk"));
                RuleBasedCollator videcoll = new RuleBasedCollator(vicoll.GetRules() + decoll.GetRules());
                RuleBasedCollator importvidecoll = new RuleBasedCollator("[import vi][import de-u-co-phonebk]");

                UnicodeSet tailoredSet = videcoll.GetTailoredSet();
                UnicodeSet importTailoredSet = importvidecoll.GetTailoredSet();

                if (!tailoredSet.Equals(importTailoredSet))
                {
                    Warnln("Tailored set not equal");
                }

                for (UnicodeSetIterator it = new UnicodeSetIterator(tailoredSet); it.Next();)
                {
                    String t = it.GetString();
                    CollationKey sk1 = videcoll.GetCollationKey(t);
                    CollationKey sk2 = importvidecoll.GetCollationKey(t);
                    if (!sk1.Equals(sk2))
                    {
                        Warnln("Collation key's not equal for " + t);
                    }
                }

            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
            }
        }

        /*
         * This test ensures that characters placed before a character in a different script have the same lead byte
         * in their collation key before and after script reordering.
         */
        [Test]
        public void TestBeforeRuleWithScriptReordering()
        {
            /* build collator */
            String rules = "&[before 1]\u03b1 < \u0e01";
            int[] reorderCodes = { UScript.Greek };
            int result;

            Collator myCollation = new RuleBasedCollator(rules);
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
            myCollation.Strength = (Collator.Tertiary);

            String @base = "\u03b1"; /* base */
            String before = "\u0e01"; /* ko kai */

            /* check collation results - before rule applied but not script reordering */
            result = myCollation.Compare(@base, before);
            if (!(result > 0))
            {
                Errln("Collation result not correct before script reordering.");
            }

            /* check the lead byte of the collation keys before script reordering */
            CollationKey baseKey = myCollation.GetCollationKey(@base);
            CollationKey beforeKey = myCollation.GetCollationKey(before);
            byte[] baseKeyBytes = baseKey.ToByteArray();
            byte[] beforeKeyBytes = beforeKey.ToByteArray();
            if (baseKeyBytes[0] != beforeKeyBytes[0])
            {
                Errln("Different lead byte for sort keys using before rule and before script reordering. base character lead byte = "
                        + baseKeyBytes[0] + ", before character lead byte = " + beforeKeyBytes[0]);
            }

            /* reorder the scripts */
            myCollation.SetReorderCodes(reorderCodes);

            /* check collation results - before rule applied and after script reordering */
            result = myCollation.Compare(@base, before);
            if (!(result > 0))
            {
                Errln("Collation result not correct after script reordering.");
            }

            /* check the lead byte of the collation keys after script reordering */
            baseKey = myCollation.GetCollationKey(@base);
            beforeKey = myCollation.GetCollationKey(before);
            baseKeyBytes = baseKey.ToByteArray();
            beforeKeyBytes = beforeKey.ToByteArray();
            if (baseKeyBytes[0] != beforeKeyBytes[0])
            {
                Errln("Different lead byte for sort keys using before rule and before script reordering. base character lead byte = "
                        + baseKeyBytes[0] + ", before character lead byte = " + beforeKeyBytes[0]);
            }
        }

        /*
         * Test that in a primary-compressed sort key all bytes except the first one are unchanged under script reordering.
         */
        [Test]
        public void TestNonLeadBytesDuringCollationReordering()
        {
            Collator myCollation;
            byte[] baseKey;
            byte[] reorderKey;
            int[] reorderCodes = { UScript.Greek };
            String testString = "\u03b1\u03b2\u03b3";

            /* build collator tertiary */
            myCollation = new RuleBasedCollator("");
            myCollation.Strength = (Collator.Tertiary);
            baseKey = myCollation.GetCollationKey(testString).ToByteArray();

            myCollation.SetReorderCodes(reorderCodes);
            reorderKey = myCollation.GetCollationKey(testString).ToByteArray();

            if (baseKey.Length != reorderKey.Length)
            {
                Errln("Key lengths not the same during reordering.\n");
            }

            for (int i = 1; i < baseKey.Length; i++)
            {
                if (baseKey[i] != reorderKey[i])
                {
                    Errln("Collation key bytes not the same at position " + i);
                }
            }

            /* build collator tertiary */
            myCollation = new RuleBasedCollator("");
            myCollation.Strength = (Collator.Quaternary);
            baseKey = myCollation.GetCollationKey(testString).ToByteArray();

            myCollation.SetReorderCodes(reorderCodes);
            reorderKey = myCollation.GetCollationKey(testString).ToByteArray();

            if (baseKey.Length != reorderKey.Length)
            {
                Errln("Key lengths not the same during reordering.\n");
            }

            for (int i = 1; i < baseKey.Length; i++)
            {
                if (baseKey[i] != reorderKey[i])
                {
                    Errln("Collation key bytes not the same at position " + i);
                }
            }
        }

        /*
         * Test reordering API.
         */
        [Test]
        public void TestReorderingAPI()
        {
            Collator myCollation;
            int[] reorderCodes = { UScript.Greek, UScript.Han, ReorderCodes.Punctuation };
            int[] duplicateReorderCodes = { UScript.Hiragana, UScript.Greek, ReorderCodes.Currency, UScript.Katakana };
            int[] reorderCodesStartingWithDefault = { ReorderCodes.Default, UScript.Greek, UScript.Han, ReorderCodes.Punctuation };
            int[] retrievedReorderCodes;
            String greekString = "\u03b1";
            String punctuationString = "\u203e";

            /* build collator tertiary */
            myCollation = new RuleBasedCollator("");
            myCollation.Strength = (Collator.Tertiary);

            /* set the reorderding */
            myCollation.SetReorderCodes(reorderCodes);

            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (!Arrays.Equals(reorderCodes, retrievedReorderCodes))
            {
                Errln("ERROR: retrieved reorder codes do not match set reorder codes.");
            }
            if (!(myCollation.Compare(greekString, punctuationString) < 0))
            {
                Errln("ERROR: collation result should have been less.");
            }

            /* clear the reordering */
            myCollation.SetReorderCodes(null);
            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (retrievedReorderCodes.Length != 0)
            {
                Errln("ERROR: retrieved reorder codes was not null.");
            }

            if (!(myCollation.Compare(greekString, punctuationString) > 0))
            {
                Errln("ERROR: collation result should have been greater.");
            }

            // do it again with an empty but non-null array

            /* set the reorderding */
            myCollation.SetReorderCodes(reorderCodes);

            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (!Arrays.Equals(reorderCodes, retrievedReorderCodes))
            {
                Errln("ERROR: retrieved reorder codes do not match set reorder codes.");
            }
            if (!(myCollation.Compare(greekString, punctuationString) < 0))
            {
                Errln("ERROR: collation result should have been less.");
            }

            /* clear the reordering */
            myCollation.SetReorderCodes(new int[] { });
            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (retrievedReorderCodes.Length != 0)
            {
                Errln("ERROR: retrieved reorder codes was not null.");
            }

            if (!(myCollation.Compare(greekString, punctuationString) > 0))
            {
                Errln("ERROR: collation result should have been greater.");
            }

            /* clear the reordering using [NONE] */
            myCollation.SetReorderCodes(new int[] { ReorderCodes.None });
            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (retrievedReorderCodes.Length != 0)
            {
                Errln("ERROR: [NONE] retrieved reorder codes was not null.");
            }

            bool gotException = false;
            /* set duplicates in the reorder codes */
            try
            {
                myCollation.SetReorderCodes(duplicateReorderCodes);
            }
            catch (ArgumentException e)
            {
                // expect exception on illegal arguments
                gotException = true;
            }
            if (!gotException)
            {
                Errln("ERROR: exception was not thrown for illegal reorder codes argument.");
            }

            /* set duplicate reorder codes */
            gotException = false;
            try
            {
                myCollation.SetReorderCodes(reorderCodesStartingWithDefault);
            }
            catch (ArgumentException e)
            {
                gotException = true;
            }
            if (!gotException)
            {
                Errln("ERROR: reorder codes following a 'default' code should have thrown an exception but did not.");
            }
        }

        /*
         * Test reordering API.
         */
        [Test]
        public void TestReorderingAPIWithRuleCreatedCollator()
        {
            Collator myCollation;
            String rules = "[reorder Hani Grek]";
            int[] rulesReorderCodes = { UScript.Han, UScript.Greek };
            int[] reorderCodes = { UScript.Greek, UScript.Han, ReorderCodes.Punctuation };
            int[] retrievedReorderCodes;


            /* build collator tertiary */
            myCollation = new RuleBasedCollator(rules);
            myCollation.Strength = (Collator.Tertiary);

            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (!Arrays.Equals(rulesReorderCodes, retrievedReorderCodes))
            {
                Errln("ERROR: retrieved reorder codes do not match set reorder codes.");
            }

            /* clear the reordering */
            myCollation.SetReorderCodes(null);
            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (retrievedReorderCodes.Length != 0)
            {
                Errln("ERROR: retrieved reorder codes was not null.");
            }

            /* set the reorderding */
            myCollation.SetReorderCodes(reorderCodes);

            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (!Arrays.Equals(reorderCodes, retrievedReorderCodes))
            {
                Errln("ERROR: retrieved reorder codes do not match set reorder codes.");
            }

            /* reset the reordering */
            myCollation.SetReorderCodes(ReorderCodes.Default);
            retrievedReorderCodes = myCollation.GetReorderCodes();
            if (!Arrays.Equals(rulesReorderCodes, retrievedReorderCodes))
            {
                Errln("ERROR: retrieved reorder codes do not match set reorder codes.");
            }
        }

        static bool containsExpectedScript(int[] scripts, int expectedScript)
        {
            for (int i = 0; i < scripts.Length; ++i)
            {
                if (expectedScript == scripts[i]) { return true; }
            }
            return false;
        }

        [Test]
        public void TestEquivalentReorderingScripts()
        {
            // Beginning with ICU 55, collation reordering moves single scripts
            // rather than groups of scripts,
            // except where scripts share a range and sort primary-equal.
            int[] expectedScripts = {
                    UScript.Hiragana,
                    UScript.Katakana,
                    UScript.KatakanaOrHiragana
            };

            int[] equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.Gothic);
            if (equivalentScripts.Length != 1 || equivalentScripts[0] != UScript.Gothic)
            {
                Errln(String.Format("ERROR/Gothic: retrieved equivalent scripts wrong: " +
                        "length expected 1, was = {0}; expected [{1}] was [{2}]",
                        equivalentScripts.Length, UScript.Gothic, equivalentScripts[0]));
            }

            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.Hiragana);
            if (equivalentScripts.Length != expectedScripts.Length)
            {
                Errln(String.Format("ERROR/Hiragana: retrieved equivalent script length wrong: " +
                        "expected {0}, was = {1}",
                        expectedScripts.Length, equivalentScripts.Length));
            }
            int prevScript = -1;
            for (int i = 0; i < equivalentScripts.Length; ++i)
            {
                int script = equivalentScripts[i];
                if (script <= prevScript)
                {
                    Errln("ERROR/Hiragana: equivalent scripts out of order at index " + i);
                }
                prevScript = script;
            }
            foreach (int code in expectedScripts)
            {
                if (!containsExpectedScript(equivalentScripts, code))
                {
                    Errln("ERROR/Hiragana: equivalent scripts do not contain " + code);
                }
            }

            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.Katakana);
            if (equivalentScripts.Length != expectedScripts.Length)
            {
                Errln(String.Format("ERROR/Katakana: retrieved equivalent script length wrong: " +
                        "expected {0}, was = {1}",
                        expectedScripts.Length, equivalentScripts.Length));
            }
            foreach (int code in expectedScripts)
            {
                if (!containsExpectedScript(equivalentScripts, code))
                {
                    Errln("ERROR/Katakana: equivalent scripts do not contain " + code);
                }
            }

            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.KatakanaOrHiragana);
            if (equivalentScripts.Length != expectedScripts.Length)
            {
                Errln(String.Format("ERROR/Hrkt: retrieved equivalent script length wrong: " +
                        "expected {0}, was = {1}",
                        expectedScripts.Length, equivalentScripts.Length));
            }

            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.Han);
            if (equivalentScripts.Length != 3)
            {
                Errln("ERROR/Hani: retrieved equivalent script length wrong: " +
                        "expected 3, was = " + equivalentScripts.Length);
            }
            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.SimplifiedHan);
            if (equivalentScripts.Length != 3)
            {
                Errln("ERROR/Hans: retrieved equivalent script length wrong: " +
                        "expected 3, was = " + equivalentScripts.Length);
            }
            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.TraditionalHan);
            if (equivalentScripts.Length != 3)
            {
                Errln("ERROR/Hant: retrieved equivalent script length wrong: " +
                        "expected 3, was = " + equivalentScripts.Length);
            }

            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.MeroiticCursive);
            if (equivalentScripts.Length != 2)
            {
                Errln("ERROR/Merc: retrieved equivalent script length wrong: " +
                        "expected 2, was = " + equivalentScripts.Length);
            }
            equivalentScripts = RuleBasedCollator.GetEquivalentReorderCodes(UScript.MeroiticHieroglyphs);
            if (equivalentScripts.Length != 2)
            {
                Errln("ERROR/Mero: retrieved equivalent script length wrong: " +
                        "expected 2, was = " + equivalentScripts.Length);
            }
        }

        [Test]
        public void TestGreekFirstReorderCloning()
        {
            String[] testSourceCases = {
                "\u0041",
                "\u03b1\u0041",
                "\u0061",
                "\u0041\u0061",
                "\u0391",
            };

            String[] testTargetCases = {
                "\u03b1",
                "\u0041\u03b1",
                "\u0391",
                "\u0391\u03b1",
                "\u0391",
            };

            int[] results = {
                1,
                -1,
                1,
                1,
                0
            };

            Collator originalCollation;
            Collator myCollation;
            String rules = "[reorder Grek]";
            try
            {
                originalCollation = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
                return;
            }
            try
            {
                myCollation = (Collator)originalCollation.Clone();
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of rule based collator");
                return;
            }
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
            myCollation.Strength = (Collator.Tertiary);
            for (int i = 0; i < testSourceCases.Length; i++)
            {
                CollationTest.DoTest(this, (RuleBasedCollator)myCollation,
                                     testSourceCases[i], testTargetCases[i],
                                     results[i]);
            }
        }

        /*
         * Utility function to test one collation reordering test case.
         * @param testcases Array of test cases.
         * @param n_testcases Size of the array testcases.
         * @param str_rules Array of rules.  These rules should be specifying the same rule in different formats.
         * @param n_rules Size of the array str_rules.
         */
        private void doTestOneReorderingAPITestCase(OneTestCase[] testCases, int[] reorderTokens)
        {
            Collator myCollation = Collator.GetInstance(ULocale.ENGLISH);
            myCollation.SetReorderCodes(reorderTokens);

            foreach (OneTestCase testCase in testCases)
            {
                CollationTest.DoTest(this, (RuleBasedCollator)myCollation,
                        testCase.m_source_,
                        testCase.m_target_,
                        testCase.m_result_);
            }
        }

        [Test]
        public void TestGreekFirstReorder()
        {
            String[] strRules = {
                "[reorder Grek]"
            };

            int[] apiRules = {
                UScript.Greek
            };

            OneTestCase[] privateUseCharacterStrings = {
                new OneTestCase("\u0391", "\u0391", 0),
                new OneTestCase("\u0041", "\u0391", 1),
                new OneTestCase("\u03B1\u0041", "\u03B1\u0391", 1),
                new OneTestCase("\u0060", "\u0391", -1),
                new OneTestCase("\u0391", "\ue2dc", -1),
                new OneTestCase("\u0391", "\u0060", 1),
            };

            /* Test rules creation */
            doTestCollation(privateUseCharacterStrings, strRules);

            /* Test collation reordering API */
            doTestOneReorderingAPITestCase(privateUseCharacterStrings, apiRules);
        }

        [Test]
        public void TestGreekLastReorder()
        {
            String[] strRules = {
                "[reorder Zzzz Grek]"
            };

            int[] apiRules = {
                UScript.Unknown, UScript.Greek
            };

            OneTestCase[] privateUseCharacterStrings = {
                new OneTestCase("\u0391", "\u0391", 0),
                new OneTestCase("\u0041", "\u0391", -1),
                new OneTestCase("\u03B1\u0041", "\u03B1\u0391", -1),
                new OneTestCase("\u0060", "\u0391", -1),
                new OneTestCase("\u0391", "\ue2dc", 1),
            };

            /* Test rules creation */
            doTestCollation(privateUseCharacterStrings, strRules);

            /* Test collation reordering API */
            doTestOneReorderingAPITestCase(privateUseCharacterStrings, apiRules);
        }

        [Test]
        public void TestNonScriptReorder()
        {
            String[] strRules = {
                "[reorder Grek Symbol DIGIT Latn Punct space Zzzz cURRENCy]"
            };

            int[] apiRules = {
                UScript.Greek, ReorderCodes.Symbol, ReorderCodes.Digit, UScript.Latin,
                ReorderCodes.Punctuation, ReorderCodes.Space, UScript.Unknown,
                ReorderCodes.Currency
            };

            OneTestCase[] privateUseCharacterStrings = {
                new OneTestCase("\u0391", "\u0041", -1),
                new OneTestCase("\u0041", "\u0391", 1),
                new OneTestCase("\u0060", "\u0041", -1),
                new OneTestCase("\u0060", "\u0391", 1),
                new OneTestCase("\u0024", "\u0041", 1),
            };

            /* Test rules creation */
            doTestCollation(privateUseCharacterStrings, strRules);

            /* Test collation reordering API */
            doTestOneReorderingAPITestCase(privateUseCharacterStrings, apiRules);
        }

        [Test]
        public void TestHaniReorder()
        {
            String[] strRules = {
                "[reorder Hani]"
            };
            int[] apiRules = {
                UScript.Han
            };

            OneTestCase[] privateUseCharacterStrings = {
                new OneTestCase("\u4e00", "\u0041", -1),
                new OneTestCase("\u4e00", "\u0060", 1),
                new OneTestCase("\uD86D\uDF40", "\u0041", -1),
                new OneTestCase("\uD86D\uDF40", "\u0060", 1),
                new OneTestCase("\u4e00", "\uD86D\uDF40", -1),
                new OneTestCase("\ufa27", "\u0041", -1),
                new OneTestCase("\uD869\uDF00", "\u0041", -1),
            };

            /* Test rules creation */
            doTestCollation(privateUseCharacterStrings, strRules);

            /* Test collation reordering API */
            doTestOneReorderingAPITestCase(privateUseCharacterStrings, apiRules);
        }

        [Test]
        public void TestHaniReorderWithOtherRules()
        {
            String[] strRules = {
                "[reorder Hani]  &b<a"
            };

            OneTestCase[] privateUseCharacterStrings = {
                new OneTestCase("\u4e00", "\u0041", -1),
                new OneTestCase("\u4e00", "\u0060", 1),
                new OneTestCase("\uD86D\uDF40", "\u0041", -1),
                new OneTestCase("\uD86D\uDF40", "\u0060", 1),
                new OneTestCase("\u4e00", "\uD86D\uDF40", -1),
                new OneTestCase("\ufa27", "\u0041", -1),
                new OneTestCase("\uD869\uDF00", "\u0041", -1),
                new OneTestCase("b", "a", -1),
            };

            /* Test rules creation */
            doTestCollation(privateUseCharacterStrings, strRules);
        }

        [Test]
        public void TestMultipleReorder()
        {
            String[] strRules = {
                "[reorder Grek Zzzz DIGIT Latn Hani]"
            };

            int[] apiRules = {
                UScript.Greek, UScript.Unknown, ReorderCodes.Digit, UScript.Latin, UScript.Han
            };

            OneTestCase[] collationTestCases = {
                new OneTestCase("\u0391", "\u0041", -1),
                new OneTestCase("\u0031", "\u0041", -1),
                new OneTestCase("u0041", "\u4e00", -1),
            };

            /* Test rules creation */
            doTestCollation(collationTestCases, strRules);

            /* Test collation reordering API */
            doTestOneReorderingAPITestCase(collationTestCases, apiRules);
        }

        [Test]
        public void TestFrozeness()
        {
            Collator myCollation = Collator.GetInstance(ULocale.CANADA);
            bool exceptionCaught = false;

            myCollation.Freeze();
            assertTrue("Collator not frozen.", myCollation.IsFrozen);

            try
            {
                myCollation.Strength = (Collator.Secondary);
            }
            catch (NotSupportedException e)
            {
                // expected
                exceptionCaught = true;
            }
            assertTrue("Frozen collator allowed change.", exceptionCaught);
            exceptionCaught = false;

            try
            {
                myCollation.SetReorderCodes(ReorderCodes.Default);
            }
            catch (NotSupportedException e)
            {
                // expected
                exceptionCaught = true;
            }
            assertTrue("Frozen collator allowed change.", exceptionCaught);
            exceptionCaught = false;

            try
            {
                myCollation.VariableTop = (12);
            }
            catch (NotSupportedException e)
            {
                // expected
                exceptionCaught = true;
            }
            assertTrue("Frozen collator allowed change.", exceptionCaught);
            exceptionCaught = false;

            Collator myClone = null;
            //try
            //{
            myClone = (Collator)myCollation.Clone();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    // should not happen - clone is implemented in Collator
            //    Errln("ERROR: unable to clone collator.");
            //}
            assertTrue("Clone not frozen as expected.", myClone.IsFrozen);

            myClone = myClone.CloneAsThawed();
            assertFalse("Clone not thawed as expected.", myClone.IsFrozen);
        }

        // Test case for Ticket#9409
        // Unknown collation type should be ignored, without printing stack trace
        [Test]
        public void TestUnknownCollationKeyword()
        {
            Collator coll1 = Collator.GetInstance(new ULocale("en_US@collation=bogus"));
            Collator coll2 = Collator.GetInstance(new ULocale("en_US"));
            assertEquals("Unknown collation keyword 'bogus' should be ignored", coll1, coll2);
        }
    }
}
