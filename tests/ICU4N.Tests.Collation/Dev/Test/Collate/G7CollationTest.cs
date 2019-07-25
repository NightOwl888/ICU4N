using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/G7CollationTest
/// Source File: $ICU4CRoot/source/test/intltest/g7coll.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class G7CollationTest : TestFmwk
    {
        private static String[] testCases = {
            "blackbirds", "Pat", "p\u00E9ch\u00E9", "p\u00EAche", "p\u00E9cher",
            "p\u00EAcher", "Tod", "T\u00F6ne", "Tofu", "blackbird", "Ton",
            "PAT", "black-bird", "black-birds", "pat", // 14
            // Additional tests
            "czar", "churo", "cat", "darn", "?",                                                                                /* 19 */
            "quick", "#", "&", "a-rdvark", "aardvark",                                                        /* 23 */
            "abbot", "co-p", "cop", "coop", "zebra"
        };

        private static int[][] results = {
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* en_US */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* en_GB */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* en_CA */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* fr_FR */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 3, 2, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* fr_CA */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* de_DE */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* it_IT */
            new int[] { 12, 13, 9, 0, 14, 1, 11, 2, 3, 4, 5, 6, 8, 10, 7, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 }, /* ja_JP */
            /* new table collation with rules "& Z < p, P"  loop to FIXEDTESTSET */
            new int[] { 12, 13, 9, 0, 6, 8, 10, 7, 14, 1, 11, 2, 3, 4, 5, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 },
            /* new table collation with rules "& C < ch , cH, Ch, CH " loop to TOTALTESTSET */
            new int[] { 19, 22, 21, 23, 24, 25, 12, 13, 9, 0, 17, 26, 28, 27, 15, 16, 18, 14, 1, 11, 2, 3, 4, 5, 20, 6, 8, 10, 7, 29 },
            /* new table collation with rules "& Question-mark ; ? & Hash-mark ; # & Ampersand ; '&'  " loop to TOTALTESTSET */
            new int[] { 23, 24, 25, 22, 12, 13, 9, 0, 17, 16, 26, 28, 27, 15, 18, 21, 14, 1, 11, 2, 3, 4, 5, 19, 20, 6, 8, 10, 7, 29 },
            /* analogous to Japanese rules " & aa ; a- & ee ; e- & ii ; i- & oo ; o- & uu ; u- " */  /* loop to TOTALTESTSET */
            new int[] { 19, 22, 21, 24, 23, 25, 12, 13, 9, 0, 17, 16, 28, 26, 27, 15, 18, 14, 1, 11, 2, 3, 4, 5, 20, 6, 8, 10, 7, 29 }
        };

        //private static readonly int MAX_TOKEN_LEN = 16;
        //private static readonly int TESTLOCALES = 12;
        private static readonly int FIXEDTESTSET = 15;
        private static readonly int TOTALTESTSET = 30;

        // perform test with added rules " & Z < p, P"
        [Test]
        public void TestDemo1()
        {
            Logln("Demo Test 1 : Create a new table collation with rules \"& Z < p, P\"");

            Collator col = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);


            String baseRules = ((RuleBasedCollator)col).GetRules();
            String newRules = " & Z < p, P";
            newRules = baseRules + newRules;
            RuleBasedCollator myCollation = null;
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + newRules);
                return;
            }

            int j, n;
            for (j = 0; j < FIXEDTESTSET; j++)
            {
                for (n = j + 1; n < FIXEDTESTSET; n++)
                {
                    DoTest(myCollation, testCases[results[8][j]], testCases[results[8][n]], -1);
                }
            }
        }


        // perorm test with added rules "& C < ch , cH, Ch, CH"
        [Test]
        public void TestDemo2()
        {
            Logln("Demo Test 2 : Create a new table collation with rules \"& C < ch , cH, Ch, CH\"");
            Collator col = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);


            String baseRules = ((RuleBasedCollator)col).GetRules();
            String newRules = "& C < ch , cH, Ch, CH";
            newRules = baseRules + newRules;
            RuleBasedCollator myCollation = null;
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + newRules);
                return;
            }

            int j, n;
            for (j = 0; j < TOTALTESTSET; j++)
            {
                for (n = j + 1; n < TOTALTESTSET; n++)
                {
                    DoTest(myCollation, testCases[results[9][j]], testCases[results[9][n]], -1);
                }
            }
        }


        // perform test with added rules
        // "& Question'-'mark ; '?' & Hash'-'mark ; '#' & Ampersand ; '&'"
        [Test]
        public void TestDemo3()
        {
            // Logln("Demo Test 3 : Create a new table collation with rules \"& Question'-'mark ; '?' & Hash'-'mark ; '#' & Ampersand ; '&'\"");
            Collator col = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);


            String baseRules = ((RuleBasedCollator)col).GetRules();
            String newRules = "& Question'-'mark ; '?' & Hash'-'mark ; '#' & Ampersand ; '&'";
            newRules = baseRules + newRules;
            RuleBasedCollator myCollation = null;
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + newRules);
                return;
            }

            int j, n;
            for (j = 0; j < TOTALTESTSET; j++)
            {
                for (n = j + 1; n < TOTALTESTSET; n++)
                {
                    DoTest(myCollation, testCases[results[10][j]], testCases[results[10][n]], -1);
                }
            }
        }


        // perform test with added rules
        // " & aa ; a'-' & ee ; e'-' & ii ; i'-' & oo ; o'-' & uu ; u'-' "
        [Test]
        public void TestDemo4()
        {
            Logln("Demo Test 4 : Create a new table collation with rules \" & aa ; a'-' & ee ; e'-' & ii ; i'-' & oo ; o'-' & uu ; u'-' \"");
            Collator col = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);

            String baseRules = ((RuleBasedCollator)col).GetRules();
            String newRules = " & aa ; a'-' & ee ; e'-' & ii ; i'-' & oo ; o'-' & uu ; u'-' ";
            newRules = baseRules + newRules;
            RuleBasedCollator myCollation = null;
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + newRules);
                return;
            }

            int j, n;
            for (j = 0; j < TOTALTESTSET; j++)
            {
                for (n = j + 1; n < TOTALTESTSET; n++)
                {
                    DoTest(myCollation, testCases[results[11][j]], testCases[results[11][n]], -1);
                }
            }
        }

        [Test]
        public void TestG7Data()
        {
            CultureInfo[] locales = {
                new CultureInfo("en-US") /* Locale.US */,
                new CultureInfo("en-GB") /* Locale.UK */,
                new CultureInfo("en-CA") /* Locale.CANADA */,
                new CultureInfo("fr-FR") /* Locale.FRANCE */,
                new CultureInfo("fr-CA") /* Locale.CANADA_FRENCH */,
                new CultureInfo("de-DE") /* Locale.GERMANY */,
                new CultureInfo("ja-JP") /* Locale.JAPAN */,
                new CultureInfo("it-IT") /* Locale.ITALY */
            };
            int i = 0, j = 0;
            for (i = 0; i < locales.Length; i++)
            {
                Collator myCollation = null;
                RuleBasedCollator tblColl1 = null;
                try
                {
                    myCollation = Collator.GetInstance(locales[i]);
                    tblColl1 = new RuleBasedCollator(((RuleBasedCollator)myCollation).GetRules());
                }
                catch (Exception foo)
                {
                    Warnln("Exception: " + foo.Message +
                          "; Locale : " + locales[i].DisplayName + " getRules failed");
                    continue;
                }
                for (j = 0; j < FIXEDTESTSET; j++)
                {
                    for (int n = j + 1; n < FIXEDTESTSET; n++)
                    {
                        DoTest(tblColl1, testCases[results[i][j]], testCases[results[i][n]], -1);
                    }
                }
                myCollation = null;
            }
        }


        // main test routine, tests comparisons for a set of strings against sets of expected results
        private void DoTest(Collator myCollation, String source, String target,
                            int result)
        {

            int compareResult = myCollation.Compare(source, target);
            CollationKey sortKey1, sortKey2;
            sortKey1 = myCollation.GetCollationKey(source);
            sortKey2 = myCollation.GetCollationKey(target);
            int keyResult = sortKey1.CompareTo(sortKey2);
            ReportCResult(source, target, sortKey1, sortKey2, compareResult,
                          keyResult, compareResult, result);
        }

        private void ReportCResult(String source, String target, CollationKey sourceKey, CollationKey targetKey,
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

            if (ok1 && ok2 && ok3 && !IsVerbose())
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
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
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
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
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
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
            }
        }
    }
}
