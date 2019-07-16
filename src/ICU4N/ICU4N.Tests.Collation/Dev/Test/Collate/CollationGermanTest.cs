using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/CollationGermanTest
/// Source File: $ICU4CRoot/source/test/intltest/decoll.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationGermanTest : TestFmwk
    {
        private static char[][] testSourceCases = {
                new char[] {(char)0x47, (char)0x72, (char)0x00F6, (char)0x00DF, (char)0x65},
                new char[] {(char)0x61, (char)0x62, (char)0x63},
                new char[] {(char)0x54, (char)0x00F6, (char)0x6e, (char)0x65},
                new char[] {(char)0x54, (char)0x00F6, (char)0x6e, (char)0x65},
                new char[] {(char)0x54, (char)0x00F6, (char)0x6e, (char)0x65},
                new char[] {(char)0x61, (char)0x0308, (char)0x62, (char)0x63},
                new char[] {(char)0x00E4, (char)0x62, (char)0x63},
                new char[] {(char)0x00E4, (char)0x62, (char)0x63},
                new char[] {(char)0x53, (char)0x74, (char)0x72, (char)0x61, (char)0x00DF, (char)0x65},
                new char[] {(char)0x65, (char)0x66, (char)0x67},
                new char[] {(char)0x00E4, (char)0x62, (char)0x63},
                new char[] {(char)0x53, (char)0x74, (char)0x72, (char)0x61, (char)0x00DF, (char)0x65}
            };

        private static char[][] testTargetCases = {
                new char[] {(char)0x47, (char)0x72, (char)0x6f, (char)0x73, (char)0x73, (char)0x69, (char)0x73, (char)0x74},
                new char[] {(char)0x61, (char)0x0308, (char)0x62, (char)0x63},
                new char[] {(char)0x54, (char)0x6f, (char)0x6e},
                new char[] {(char)0x54, (char)0x6f, (char)0x64},
                new char[] {(char)0x54, (char)0x6f, (char)0x66, (char)0x75},
                new char[] {(char)0x41, (char)0x0308, (char)0x62, (char)0x63},
                new char[] {(char)0x61, (char)0x0308, (char)0x62, (char)0x63},
                new char[] {(char)0x61, (char)0x65, (char)0x62, (char)0x63},
                new char[] {(char)0x53, (char)0x74, (char)0x72, (char)0x61, (char)0x73, (char)0x73, (char)0x65},
                new char[] {(char)0x65, (char)0x66, (char)0x67},
                new char[] {(char)0x61, (char)0x65, (char)0x62, (char)0x63},
                new char[] {(char)0x53, (char)0x74, (char)0x72, (char)0x61, (char)0x73, (char)0x73, (char)0x65}
            };

        private static int[][] results =
            {
                //  Primary  Tertiary
                new int[] { -1,        -1 },
                new int[] { 0,         -1 },
                new int[] { 1,          1 },
                new int[] { 1,          1 },
                new int[] { 1,          1 },
                new int[] { 0,         -1 },
                new int[] { 0,          0 },
                new int[] { -1,        -1 },
                new int[] { 0,          1 },
                new int[] { 0,          0 },
                new int[] { -1,        -1 },
                new int[] { 0,          1 }
            };

        private Collator myCollation = null;

        public CollationGermanTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new CultureInfo("de") /* Locale.GERMAN */);
            if (myCollation == null)
            {
                Errln("ERROR: in creation of collator of GERMAN locale");
            }
        }

        // perform test with strength TERTIARY
        [Test]
        public void TestTertiary()
        {
            if (myCollation == null)
            {
                Errln("decoll: cannot start test, collator is null\n");
                return;
            }

            int i = 0;
            myCollation.Strength = (Collator.Tertiary);
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
            for (i = 0; i < 12; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i][1]);
            }
        }

        // perform test with strength SECONDARY
        //This method in icu4c has no implementation.
        [Test]
        public void TestSecondary()
        {
        }

        // perform test with strength PRIMARY
        [Test]
        public void TestPrimary()
        {
            if (myCollation == null)
            {
                Errln("decoll: cannot start test, collator is null\n");
                return;
            }
            int i;
            myCollation.Strength = (Collator.Primary);
            myCollation.Decomposition = (Collator.CanonicalDecomposition);
            for (i = 0; i < 12; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i][0]);
            }
        }


        //main test routine, tests rules specific to germa locale
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
