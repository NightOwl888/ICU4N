using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

//
// Port From:   ICU4C v2.1 : Collate/CollationSpanishTest
// Source File: $ICU4CRoot/source/test/intltest/escoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationSpanishTest : TestFmwk
    {
        private static char[][] testSourceCases = {
            new char[] {(char)0x61, (char)0x6c, (char)0x69, (char)0x61, (char)0x73},
            new char[] {(char)0x45, (char)0x6c, (char)0x6c, (char)0x69, (char)0x6f, (char)0x74},
            new char[] {(char)0x48, (char)0x65, (char)0x6c, (char)0x6c, (char)0x6f},
            new char[] {(char)0x61, (char)0x63, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x63, (char)0x63},
            new char[] {(char)0x61, (char)0x6c, (char)0x69, (char)0x61, (char)0x73},
            new char[] {(char)0x61, (char)0x63, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x63, (char)0x63},
            new char[] {(char)0x48, (char)0x65, (char)0x6c, (char)0x6c, (char)0x6f},
        };

        private static char[][] testTargetCases =  {
            new char[] {(char)0x61, (char)0x6c, (char)0x6c, (char)0x69, (char)0x61, (char)0x73},
            new char[] {(char)0x45, (char)0x6d, (char)0x69, (char)0x6f, (char)0x74},
            new char[] {(char)0x68, (char)0x65, (char)0x6c, (char)0x6c, (char)0x4f},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x6c, (char)0x6c, (char)0x69, (char)0x61, (char)0x73},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},
            new char[] {(char)0x68, (char)0x65, (char)0x6c, (char)0x6c, (char)0x4f},
        };

        private static int[] results = {
            -1,
            -1,
            1,
            -1,
            -1,
            // test primary > 5
            -1,
            0,
            -1,
            0
        };

        //static public Collator myCollation = Collator.getInstance(new Locale("es", "ES"));

        private Collator myCollation = null;

        public CollationSpanishTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new CultureInfo("es-ES"));
        }

        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = (Collator.Tertiary);
            for (i = 0; i < 5; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        [Test]
        public void TestPrimary()
        {
            int i;
            myCollation.Strength = (Collator.Primary);
            for (i = 5; i < 9; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // amin test routine, tests rules specific to the spanish locale
        private void doTest(char[] source, char[] target, int result)
        {
            String s = new String(source);
            String t = new String(target);
            int compareResult = myCollation.Compare(s, t);
            CollationKey sortKey1, sortKey2;
            sortKey1 = myCollation.GetCollationKey(s);
            sortKey2 = myCollation.GetCollationKey(t);
            int keyResult = sortKey1.CompareTo(sortKey2);
            reportCResult(s, t, sortKey1, sortKey2, compareResult, keyResult, compareResult, result);
        }

        private void reportCResult(String source, String target, CollationKey sourceKey, CollationKey targetKey,
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
