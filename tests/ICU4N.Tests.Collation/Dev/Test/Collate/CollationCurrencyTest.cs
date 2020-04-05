using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

//
// Port From:   ICU4C v2.1 : Collate/CollationCurrencyTest
// Source File: $ICU4CRoot/source/test/intltest/currcoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationCurrencyTest : TestFmwk
    {
        [Test]
        public void TestCurrency()
        {
            // All the currency symbols, in collation order
            char[][] currency = {
                    new char[] { (char)0x00A4 }, /*00A4; L; [14 36, 03, 03]    # [082B.0020.0002] # CURRENCY SIGN*/
                    new char[] { (char)0x00A2 }, /*00A2; L; [14 38, 03, 03]    # [082C.0020.0002] # CENT SIGN*/
                    new char[] { (char)0xFFE0 }, /*FFE0; L; [14 38, 03, 05]    # [082C.0020.0003] # FULLWIDTH CENT SIGN*/
                    new char[] { (char)0x0024 }, /*0024; L; [14 3A, 03, 03]    # [082D.0020.0002] # DOLLAR SIGN*/
                    new char[] { (char)0xFF04 }, /*FF04; L; [14 3A, 03, 05]    # [082D.0020.0003] # FULLWIDTH DOLLAR SIGN*/
                    new char[] { (char)0xFE69 }, /*FE69; L; [14 3A, 03, 1D]    # [082D.0020.000F] # SMALL DOLLAR SIGN*/
                    new char[] { (char)0x00A3 }, /*00A3; L; [14 3C, 03, 03]    # [082E.0020.0002] # POUND SIGN*/
                    new char[] { (char)0xFFE1 }, /*FFE1; L; [14 3C, 03, 05]    # [082E.0020.0003] # FULLWIDTH POUND SIGN*/
                    new char[] { (char)0x00A5 }, /*00A5; L; [14 3E, 03, 03]    # [082F.0020.0002] # YEN SIGN*/
                    new char[] { (char)0xFFE5 }, /*FFE5; L; [14 3E, 03, 05]    # [082F.0020.0003] # FULLWIDTH YEN SIGN*/
                    new char[] { (char)0x09F2 }, /*09F2; L; [14 40, 03, 03]    # [0830.0020.0002] # BENGALI RUPEE MARK*/
                    new char[] { (char)0x09F3 }, /*09F3; L; [14 42, 03, 03]    # [0831.0020.0002] # BENGALI RUPEE SIGN*/
                    new char[] { (char)0x0E3F }, /*0E3F; L; [14 44, 03, 03]    # [0832.0020.0002] # THAI CURRENCY SYMBOL BAHT*/
                    new char[] { (char)0x17DB }, /*17DB; L; [14 46, 03, 03]    # [0833.0020.0002] # KHMER CURRENCY SYMBOL RIEL*/
                    new char[] { (char)0x20A0 }, /*20A0; L; [14 48, 03, 03]    # [0834.0020.0002] # EURO-CURRENCY SIGN*/
                    new char[] { (char)0x20A1 }, /*20A1; L; [14 4A, 03, 03]    # [0835.0020.0002] # COLON SIGN*/
                    new char[] { (char)0x20A2 }, /*20A2; L; [14 4C, 03, 03]    # [0836.0020.0002] # CRUZEIRO SIGN*/
                    new char[] { (char)0x20A3 }, /*20A3; L; [14 4E, 03, 03]    # [0837.0020.0002] # FRENCH FRANC SIGN*/
                    new char[] { (char)0x20A4 }, /*20A4; L; [14 50, 03, 03]    # [0838.0020.0002] # LIRA SIGN*/
                    new char[] { (char)0x20A5 }, /*20A5; L; [14 52, 03, 03]    # [0839.0020.0002] # MILL SIGN*/
                    new char[] { (char)0x20A6 }, /*20A6; L; [14 54, 03, 03]    # [083A.0020.0002] # NAIRA SIGN*/
                    new char[] { (char)0x20A7 }, /*20A7; L; [14 56, 03, 03]    # [083B.0020.0002] # PESETA SIGN*/
                    new char[] { (char)0x20A9 }, /*20A9; L; [14 58, 03, 03]    # [083C.0020.0002] # WON SIGN*/
                    new char[] { (char)0xFFE6 }, /*FFE6; L; [14 58, 03, 05]    # [083C.0020.0003] # FULLWIDTH WON SIGN*/
                    new char[] { (char)0x20AA }, /*20AA; L; [14 5A, 03, 03]    # [083D.0020.0002] # NEW SHEQEL SIGN*/
                    new char[] { (char)0x20AB }, /*20AB; L; [14 5C, 03, 03]    # [083E.0020.0002] # DONG SIGN*/
                    new char[] { (char)0x20AC }, /*20AC; L; [14 5E, 03, 03]    # [083F.0020.0002] # EURO SIGN*/
                    new char[] { (char)0x20AD }, /*20AD; L; [14 60, 03, 03]    # [0840.0020.0002] # KIP SIGN*/
                    new char[] { (char)0x20AE }, /*20AE; L; [14 62, 03, 03]    # [0841.0020.0002] # TUGRIK SIGN*/
                    new char[] { (char)0x20AF } /*20AF; L; [14 64, 03, 03]    # [0842.0020.0002] # DRACHMA SIGN*/
                };

            int i, j;
            int expectedResult = 0;
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en") /*Locale.ENGLISH*/);

            // Compare each currency symbol against all the
            // currency symbols, including itself
            String source;
            String target;

            for (i = 0; i < currency.Length; i += 1)
            {
                for (j = 0; j < currency.Length; j += 1)
                {
                    source = new String(currency[i]);
                    target = new String(currency[j]);

                    if (i < j)
                    {
                        expectedResult = -1;
                    }
                    else if (i == j)
                    {
                        expectedResult = 0;
                    }
                    else
                    {
                        expectedResult = 1;
                    }

                    int compareResult = c.Compare(source, target);
                    CollationKey sourceKey = null;

                    sourceKey = c.GetCollationKey(source);

                    if (sourceKey == null)
                    {
                        Errln("Couldn't get collationKey for source");
                        continue;
                    }

                    CollationKey targetKey = null;
                    targetKey = c.GetCollationKey(target);
                    if (targetKey == null)
                    {
                        Errln("Couldn't get collationKey for source");
                        continue;
                    }

                    int keyResult = sourceKey.CompareTo(targetKey);

                    ReportCResult(source, target, sourceKey, targetKey, compareResult, keyResult, compareResult, expectedResult);
                }
            }
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
