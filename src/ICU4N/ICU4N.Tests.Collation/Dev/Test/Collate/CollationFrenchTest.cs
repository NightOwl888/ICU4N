using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/CollationFrenchTest
/// Source File: $ICU4CRoot/source/test/intltest/frcoll.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationFrenchTest : TestFmwk
    {
        private static char[][] testSourceCases = {
                new char[] {(char)0x0061/*'a'*/, (char)0x0062/*'b'*/, (char)0x0063/*'c'*/},
                new char[] {(char)0x0043/*'C'*/, (char)0x004f/*'O'*/, (char)0x0054/*'T'*/, (char)0x0045/*'E'*/},
                new char[] {(char)0x0063/*'c'*/, (char)0x006f/*'o'*/, (char)0x002d/*'-'*/, (char)0x006f/*'o'*/, (char)0x0070/*'p'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00EA, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00EA, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/, (char)0x0072/*'r'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00E9, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/, (char)0x0072/*'r'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00E9, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/, (char)0x0072/*'r'*/},
                new char[] {(char)0x0048/*'H'*/, (char)0x0065/*'e'*/, (char)0x006c/*'l'*/, (char)0x006c/*'l'*/, (char)0x006f/*'o'*/},
                new char[] {(char)0x01f1},
                new char[] {(char)0xfb00},
                new char[] {(char)0x01fa},
                new char[] {(char)0x0101}
            };

        private static char[][] testTargetCases = {
                new char[] {(char)0x0041/*'A'*/, (char)0x0042/*'B'*/, (char)0x0043/*'C'*/},
                new char[] {(char)0x0063/*'c'*/, (char)0x00f4, (char)0x0074/*'t'*/, (char)0x0065/*'e'*/},
                new char[] {(char)0x0043/*'C'*/, (char)0x004f/*'O'*/, (char)0x004f/*'O'*/, (char)0x0050/*'P'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00E9, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x00E9},
                new char[] {(char)0x0070/*'p'*/,  (char)0x00E9, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x00E9},
                new char[] {(char)0x0070/*'p'*/, (char)0x00EA, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/},
                new char[] {(char)0x0070/*'p'*/, (char)0x00EA, (char)0x0063/*'c'*/, (char)0x0068/*'h'*/, (char)0x0065/*'e'*/, (char)0x0072/*'r'*/},
                new char[] {(char)0x0068/*'h'*/, (char)0x0065/*'e'*/, (char)0x006c/*'l'*/, (char)0x006c/*'l'*/, (char)0x004f/*'O'*/},
                new char[] {(char)0x01ee},
                new char[] {(char)0x25ca},
                new char[] {(char)0x00e0},
                new char[] {(char)0x01df}
            };

        private static int[] results = {
                -1,
                -1,
                -1, /*Collator::GREATER,*/
                -1,
                1,
                1,
                -1,
                1,
               -1, /*Collator::GREATER,*/
                1,
                -1,
                -1
            };

        // 0x0300 is grave, 0x0301 is acute
        // the order of elements in this array must be different than the order in CollationEnglishTest
        private static char[][] testAcute = {
            /*00*/    new char[] {(char)0x0065/*'e'*/, (char)0x0065/*'e'*/},
            /*01*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0065/*'e'*/},
            /*02*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0301, (char)0x0065/*'e'*/},
            /*03*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0065/*'e'*/},
            /*04*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0300, (char)0x0065/*'e'*/},
            /*05*/    new char[] {(char)0x0065/*'e'*/, (char)0x0065/*'e'*/, (char)0x0301},
            /*06*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0301},
            /*07*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0301},
            /*08*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0301},
            /*09*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0301},
            /*0a*/    new char[] {(char)0x0065/*'e'*/, (char)0x0065/*'e'*/, (char)0x0300, (char)0x0301},
            /*0b*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0300, (char)0x0301},
            /*0c*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0300, (char)0x0301},
            /*0d*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0300, (char)0x0301},
            /*0e*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0300, (char)0x0301},
            /*0f*/    new char[] {(char)0x0065/*'e'*/, (char)0x0065/*'e'*/, (char)0x0300},
            /*10*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0300},
            /*11*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0300},
            /*12*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0300},
            /*13*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0300},
            /*14*/    new char[] {(char)0x0065/*'e'*/, (char)0x0065/*'e'*/, (char)0x0301, (char)0x0300},
            /*15*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0301, (char)0x0300},
            /*16*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0301, (char)0x0065/*'e'*/, (char)0x0301, (char)0x0300},
            /*17*/    new char[] {(char)0x0065/*'e'*/, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0301, (char)0x0300},
            /*18*/    new char[] {(char)0x0065/*'e'*/, (char)0x0301, (char)0x0300, (char)0x0065/*'e'*/, (char)0x0301, (char)0x0300}
            };

        private static char[][] testBugs = {
                new char[] {(char)0x0061/*'a'*/},
                new char[] {(char)0x0041/*'A'*/},
                new char[] {(char)0x0065/*'e'*/},
                new char[] {(char)0x0045/*'E'*/},
                new char[] {(char)0x00e9},
                new char[] {(char)0x00e8},
                new char[] {(char)0x00ea},
                new char[] {(char)0x00eb},
                new char[] {(char)0x0065/*'e'*/, (char)0x0061/*'a'*/},
                new char[] {(char)0x0078/*'x'*/}
            };


        private Collator myCollation = null;

        public CollationFrenchTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new CultureInfo("fr-CA"));
            //myCollation = Collator.GetInstance(new ULocale("fr", "CA", ""));
        }

        // perform tests with strength TERTIARY
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = CollationStrength.Tertiary; // Collator.TERTIARY);

            for (i = 0; i < 12; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // perform tests with strength SECONDARY
        [Test]
        public void TestSecondary()
        {
            //test acute and grave ordering
            int i = 0;
            int j;
            int expected;

            myCollation.Strength = CollationStrength.Secondary; //(Collator.SECONDARY);

            for (i = 0; i < testAcute.Length; i++)
            {
                for (j = 0; j < testAcute.Length; j++)
                {
                    if (i < j)
                    {
                        expected = -1;
                    }
                    else if (i == j)
                    {
                        expected = 0;
                    }
                    else
                    {
                        expected = 1;
                    }
                    doTest(testAcute[i], testAcute[j], expected);
                }
            }
        }

        // perform extra tests
        [Test]
        public void TestExtra()
        {
            int i, j;
            myCollation.Strength = CollationStrength.Tertiary; //(Collator.TERTIARY);
            for (i = 0; i < 9; i++)
            {
                for (j = i + 1; j < 10; j += 1)
                {
                    doTest(testBugs[i], testBugs[j], -1);
                }
            }
        }

        [Test]
        public void TestContinuationReordering()
        {
            String rule = "&0x2f00 << 0x2f01";
            try
            {
                RuleBasedCollator collator = new RuleBasedCollator(rule);
                collator.IsFrenchCollation = (true);
                CollationKey key1
                            = collator.GetCollationKey("a\u0325\u2f00\u2f01b\u0325");
                CollationKey key2
                            = collator.GetCollationKey("a\u0325\u2f01\u2f01b\u0325");
                if (key1.CompareTo(key2) >= 0)
                {
                    Errln("Error comparing continuation strings");
                }
            }
            catch (Exception e)
            {
                Errln(e.ToString());
            }
        }

        // main test routine, test rules specific to the french locale
        private void doTest(char[] source, char[] target, int result)
        {
            String s = new String(source);
            String t = new String(target);
            int compareResult = myCollation.Compare(s, t);
            CollationKey sortKey1, sortKey2;
            sortKey1 = myCollation.GetCollationKey(s);
            sortKey2 = myCollation.GetCollationKey(t);
            int keyResult = sortKey1.CompareTo(sortKey2);
            ReportCResult(s, t, sortKey1, sortKey2, compareResult, keyResult, compareResult, result);
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
