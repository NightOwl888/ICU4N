using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/CollationFinnishTest
/// Source File: $ICU4CRoot/source/test/intltest/ficoll.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationFinnishTest : TestFmwk
    {
        private static char[][] testSourceCases = {
                new char[] {(char)0x77, (char)0x61, (char)0x74},
                new char[] {(char)0x76, (char)0x61, (char)0x74},
                new char[] {(char)0x61, (char)0x00FC, (char)0x62, (char)0x65, (char)0x63, (char)0x6b},
                new char[] {(char)0x4c, (char)0x00E5, (char)0x76, (char)0x69},
                new char[] {(char)0x77, (char)0x61, (char)0x74}
            };

        private static char[][] testTargetCases =  {
                new char[] {(char)0x76, (char)0x61, (char)0x74},
                new char[] {(char)0x77, (char)0x61, (char)0x79},
                new char[] {(char)0x61, (char)0x78, (char)0x62, (char)0x65, (char)0x63, (char)0x6b},
                new char[] {(char)0x4c, (char)0x00E4, (char)0x77, (char)0x65},
                new char[] {(char)0x76, (char)0x61, (char)0x74}
            };

        private static int[] results = {
                1,
                -1,
                1,
                -1,
                // test primary > 4
                1,  // v < w per cldrbug 6615
            };

        private Collator myCollation = null;

        public CollationFinnishTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new ULocale("fi_FI@collation=standard"));
        }


        // perform tests with strength PRIMARY
        [Test]
        public void TestPrimary()
        {
            int i = 0;
            myCollation.Strength = (Collator.PRIMARY);
            for (i = 4; i < 5; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // perform test with strength TERTIARY
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = (Collator.TERTIARY);
            for (i = 0; i < 4; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // main test routine, tests rules specific to the finish locale
        private void DoTest(char[] source, char[] target, int result)
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
