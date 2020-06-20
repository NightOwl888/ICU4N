using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Globalization;

//
// Port From:   ICU4C v2.1 : Collate/CollationKanaTest
// Source File: $ICU4CRoot/source/test/intltest/jacoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationKanaTest : TestFmwk
    {
        private static char[][] testSourceCases = {
            new char[] {(char)0xff9E},
            new char[] {(char)0x3042},
            new char[] {(char)0x30A2},
            new char[] {(char)0x3042, (char)0x3042},
            new char[] {(char)0x30A2, (char)0x30FC},
            new char[] {(char)0x30A2, (char)0x30FC, (char)0x30C8}                               /*  6 */
        };

        private static char[][] testTargetCases = {
            new char[] {(char)0xFF9F},
            new char[] {(char)0x30A2},
            new char[] {(char)0x3042, (char)0x3042},
            new char[] {(char)0x30A2, (char)0x30FC},
            new char[] {(char)0x30A2, (char)0x30FC, (char)0x30C8},
            new char[] {(char)0x3042, (char)0x3042, (char)0x3068}                              /*  6 */
        };

        private static int[] results = {
            -1,
            0,   //Collator::LESS, /* Katakanas and Hiraganas are equal on tertiary level(ICU 2.0)*/
            -1,
            1, // Collator::LESS, /* Prolonged sound mark sorts BEFORE equivalent vowel (ICU 2.0)*/
            -1,
            -1,    //Collator::GREATER /* Prolonged sound mark sorts BEFORE equivalent vowel (ICU 2.0)*//*  6 */
        };

        private static char[][] testBaseCases = {
            new char[] {(char)0x30AB},
            new char[] {(char)0x30AB, (char)0x30AD},
            new char[] {(char)0x30AD},
            new char[] {(char)0x30AD, (char)0x30AD}
        };

        private static char[][] testPlainDakutenHandakutenCases = {
            new char[] {(char)0x30CF, (char)0x30AB},
            new char[] {(char)0x30D0, (char)0x30AB},
            new char[] {(char)0x30CF, (char)0x30AD},
            new char[] {(char)0x30D0, (char)0x30AD}
        };

        private static char[][] testSmallLargeCases = {
            new char[] {(char)0x30C3, (char)0x30CF},
            new char[] {(char)0x30C4, (char)0x30CF},
            new char[] {(char)0x30C3, (char)0x30D0},
            new char[] {(char)0x30C4, (char)0x30D0}
        };

        private static char[][] testKatakanaHiraganaCases = {
            new char[] {(char)0x3042, (char)0x30C3},
            new char[] {(char)0x30A2, (char)0x30C3},
            new char[] {(char)0x3042, (char)0x30C4},
            new char[] {(char)0x30A2, (char)0x30C4}
        };

        private static char[][] testChooonKigooCases = {
            /*0*/ new char[] {(char)0x30AB, (char)0x30FC, (char)0x3042},
            /*1*/ new char[] {(char)0x30AB, (char)0x30FC, (char)0x30A2},
            /*2*/ new char[] {(char)0x30AB, (char)0x30A4, (char)0x3042},
            /*3*/ new char[] {(char)0x30AB, (char)0x30A4, (char)0x30A2},
            /*6*/ new char[] {(char)0x30AD, (char)0x30FC, (char)0x3042}, /* Prolonged sound mark sorts BEFORE equivalent vowel (ICU 2.0)*/
            /*7*/ new char[] {(char)0x30AD, (char)0x30FC, (char)0x30A2}, /* Prolonged sound mark sorts BEFORE equivalent vowel (ICU 2.0)*/
            /*4*/ new char[] {(char)0x30AD, (char)0x30A4, (char)0x3042},
            /*5*/ new char[] {(char)0x30AD, (char)0x30A4, (char)0x30A2}
        };

        private Collator myCollation = null;

        public CollationKanaTest()
        {
        }

        [SetUp]
        public void init()
        {
            if (myCollation == null)
            {
                myCollation = Collator.GetInstance(new CultureInfo("ja") /* Locale.JAPANESE */);
            }
        }

        // performs test with strength TERIARY
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = (Collator.Tertiary);

            for (i = 0; i < 6; i++)
            {
                doTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        /* Testing base letters */
        [Test]
        public void TestBase()
        {
            int i;
            myCollation.Strength = (Collator.Primary);
            for (i = 0; i < 3; i++)
            {
                doTest(testBaseCases[i], testBaseCases[i + 1], -1);
            }
        }

        /* Testing plain, Daku-ten, Handaku-ten letters */
        [Test]
        public void TestPlainDakutenHandakuten()
        {
            int i;
            myCollation.Strength = (Collator.Secondary);
            for (i = 0; i < 3; i++)
            {
                doTest(testPlainDakutenHandakutenCases[i], testPlainDakutenHandakutenCases[i + 1], -1);
            }
        }

        /*
        * Test Small, Large letters
        */
        [Test]
        public void TestSmallLarge()
        {
            int i;
            myCollation.Strength = (Collator.Tertiary);

            for (i = 0; i < 3; i++)
            {
                doTest(testSmallLargeCases[i], testSmallLargeCases[i + 1], -1);
            }
        }

        /*
        * Test Katakana, Hiragana letters
        */
        [Test]
        public void TestKatakanaHiragana()
        {
            int i;
            myCollation.Strength = (Collator.Quaternary);
            for (i = 0; i < 3; i++)
            {
                doTest(testKatakanaHiraganaCases[i], testKatakanaHiraganaCases[i + 1], -1);
            }
        }

        /*
        * Test Choo-on kigoo
        */
        [Test]
        public void TestChooonKigoo()
        {
            int i;
            myCollation.Strength = (Collator.Quaternary);
            for (i = 0; i < 7; i++)
            {
                doTest(testChooonKigooCases[i], testChooonKigooCases[i + 1], -1);
            }
        }

        /*
         * Test common Hiragana and Katakana characters (e.g. 0x3099) (ticket:6140)
         */
        [Test]
        public void TestCommonCharacters()
        {
            char[] tmp1 = { (char)0x3058, (char)0x30B8 };
            char[] tmp2 = { (char)0x3057, (char)0x3099, (char)0x30B7, (char)0x3099 };
            CollationKey key1, key2;
            int result;
            String string1 = new String(tmp1);
            String string2 = new String(tmp2);
            RuleBasedCollator rb = (RuleBasedCollator)Collator.GetInstance(new UCultureInfo("ja"));
            rb.Strength = (Collator.Quaternary);
            rb.IsAlternateHandlingShifted = (false);

            result = rb.Compare(string1, string2);

            key1 = rb.GetCollationKey(string1);
            key2 = rb.GetCollationKey(string2);

            if (result != 0 || !key1.Equals(key2))
            {
                Errln("Failed Hiragana and Katakana common characters test. Expected results to be equal.");
            }

        }
        // main test routine, tests rules specific to "Kana" locale
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
