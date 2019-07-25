using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/LotusCollationKoreanTest
/// Source File: $ICU4CRoot/source/test/intltest/lcukocol.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class LotusCollationKoreanTest : TestFmwk
    {
        private static char[][] testSourceCases = {
            new char[] {(char)0xac00}
        };

        private static char[][] testTargetCases = {
            new char[] {(char)0xac01}
        };

        private static int[] results = {
            -1
        };

        private Collator myCollation;

        public LotusCollationKoreanTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new CultureInfo("ko") /* Locale.KOREAN */);
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
        }

        // performs test with strength TERIARY
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = (Collator.Tertiary);
            for (i = 0; i < 1; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // main test routine, tests rules specific to "Korean" locale
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
