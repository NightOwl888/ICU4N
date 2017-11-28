using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : collate/CollationMonkeyTest
/// Source File: $ICU4CRoot/source/test/intltest/mnkytst.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    /// <summary>
    /// CollationMonkeyTest is a third level test class.  This tests the random
    /// substrings of the default test strings to verify if the compare and
    /// sort key algorithm works correctly.  For example, any string is always
    /// less than the string itself appended with any character.
    /// </summary>
    public class CollationMonkeyTest : TestFmwk
    {
        private String source = "-abcdefghijklmnopqrstuvwxyz#&^$@";

        [Test]
        public void TestCollationKey()
        {
            if (source.Length == 0)
            {
                Errln("CollationMonkeyTest.TestCollationKey(): source is empty - ICU_DATA not set or data missing?");
                return;
            }
            Collator myCollator;
            try
            {
                myCollator = Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of collator of ENGLISH locale");
                return;
            }

            Random rand = CreateRandom(); // use test framework's random seed
            int s = rand.Next(0x7fff) % source.Length;
            int t = rand.Next(0x7fff) % source.Length;
            int slen = Math.Abs(rand.Next(0x7fff) % source.Length - source.Length) % source.Length;
            int tlen = Math.Abs(rand.Next(0x7fff) % source.Length - source.Length) % source.Length;
            String subs = source.Substring(Math.Min(s, slen), Math.Min(s + slen, source.Length) - Math.Min(s, slen)); // ICU4N: Corrected 2nd parameter
            String subt = source.Substring(Math.Min(t, tlen), Math.Min(t + tlen, source.Length) - Math.Min(t, tlen)); // ICU4N: Corrected 2nd parameter

            CollationKey collationKey1, collationKey2;

            myCollator.Strength = (Collator.TERTIARY);
            collationKey1 = myCollator.GetCollationKey(subs);
            collationKey2 = myCollator.GetCollationKey(subt);
            int result = collationKey1.CompareTo(collationKey2);  // Tertiary
            int revResult = collationKey2.CompareTo(collationKey1);  // Tertiary
            report(subs, subt, result, revResult);

            myCollator.Strength = (Collator.SECONDARY);
            collationKey1 = myCollator.GetCollationKey(subs);
            collationKey2 = myCollator.GetCollationKey(subt);
            result = collationKey1.CompareTo(collationKey2);  // Secondary
            revResult = collationKey2.CompareTo(collationKey1);   // Secondary
            report(subs, subt, result, revResult);

            myCollator.Strength = (Collator.PRIMARY);
            collationKey1 = myCollator.GetCollationKey(subs);
            collationKey2 = myCollator.GetCollationKey(subt);
            result = collationKey1.CompareTo(collationKey2);  // Primary
            revResult = collationKey2.CompareTo(collationKey1);   // Primary
            report(subs, subt, result, revResult);

            String msg = "";
            String addOne = subs + (0xE000).ToString(CultureInfo.InvariantCulture);

            collationKey1 = myCollator.GetCollationKey(subs);
            collationKey2 = myCollator.GetCollationKey(addOne);
            result = collationKey1.CompareTo(collationKey2);
            if (result != -1)
            {
                msg += "CollationKey(";
                msg += subs;
                msg += ") .LT. CollationKey(";
                msg += addOne;
                msg += ") Failed.";
                Errln(msg);
            }

            msg = "";
            result = collationKey2.CompareTo(collationKey1);
            if (result != 1)
            {
                msg += "CollationKey(";
                msg += addOne;
                msg += ") .GT. CollationKey(";
                msg += subs;
                msg += ") Failed.";
                Errln(msg);
            }
        }

        // perform monkey tests using Collator.compare
        [Test]
        public void TestCompare()
        {
            if (source.Length == 0)
            {
                Errln("CollationMonkeyTest.TestCompare(): source is empty - ICU_DATA not set or data missing?");
                return;
            }

            Collator myCollator;
            try
            {
                myCollator = Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of collator of ENGLISH locale");
                return;
            }

            /* Seed the random-number generator with current time so that
             * the numbers will be different every time we run.
             */

            Random rand = CreateRandom(); // use test framework's random seed
            int s = rand.Next(0x7fff) % source.Length;
            int t = rand.Next(0x7fff) % source.Length;
            int slen = Math.Abs(rand.Next(0x7fff) % source.Length - source.Length) % source.Length;
            int tlen = Math.Abs(rand.Next(0x7fff) % source.Length - source.Length) % source.Length;
            String subs = source.Substring(Math.Min(s, slen), Math.Min(s + slen, source.Length) - Math.Min(s, slen)); // ICU4N: Corrected 2nd parameter
            String subt = source.Substring(Math.Min(t, tlen), Math.Min(t + tlen, source.Length) - Math.Min(t, tlen)); // ICU4N: Corrected 2nd parameter

            myCollator.Strength = (Collator.TERTIARY);
            int result = myCollator.Compare(subs, subt);  // Tertiary
            int revResult = myCollator.Compare(subt, subs);  // Tertiary
            report(subs, subt, result, revResult);

            myCollator.Strength = (Collator.SECONDARY);
            result = myCollator.Compare(subs, subt);  // Secondary
            revResult = myCollator.Compare(subt, subs);  // Secondary
            report(subs, subt, result, revResult);

            myCollator.Strength = (Collator.PRIMARY);
            result = myCollator.Compare(subs, subt);  // Primary
            revResult = myCollator.Compare(subt, subs);  // Primary
            report(subs, subt, result, revResult);

            String msg = "";
            String addOne = subs + (0xE000).ToString(CultureInfo.InvariantCulture);

            result = myCollator.Compare(subs, addOne);
            if (result != -1)
            {
                msg += "Test : ";
                msg += subs;
                msg += " .LT. ";
                msg += addOne;
                msg += " Failed.";
                Errln(msg);
            }

            msg = "";
            result = myCollator.Compare(addOne, subs);
            if (result != 1)
            {
                msg += "Test : ";
                msg += addOne;
                msg += " .GT. ";
                msg += subs;
                msg += " Failed.";
                Errln(msg);
            }
        }

        void report(String s, String t, int result, int revResult)
        {
            if (revResult != -result)
            {
                String msg = "";
                msg += s;
                msg += " and ";
                msg += t;
                msg += " round trip comparison failed";
                msg += " (result " + result + ", reverse Result " + revResult + ")";
                Errln(msg);
            }
        }

        [Test]
        public void TestRules()
        {
            String[] testSourceCases = {
            "\u0061\u0062\u007a",
            "\u0061\u0062\u007a",
        };

            String[] testTargetCases = {
            "\u0061\u0062\u00e4",
            "\u0061\u0062\u0061\u0308",
        };

            int i = 0;
            Logln("Demo Test 1 : Create a new table collation with rules \"& z < 0x00e4\"");
            Collator col = Collator.GetInstance(new CultureInfo("en-US"));
            String baseRules = ((RuleBasedCollator)col).GetRules();
            String newRules = " & z < ";
            newRules = baseRules + newRules + (0x00e4).ToString(CultureInfo.InvariantCulture);
            RuleBasedCollator myCollation = null;
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Warnln("Demo Test 1 Table Collation object creation failed.");
                return;
            }

            for (i = 0; i < 2; i++)
            {
                doTest(myCollation, testSourceCases[i], testTargetCases[i], -1);
            }
            Logln("Demo Test 2 : Create a new table collation with rules \"& z < a 0x0308\"");
            newRules = "";
            newRules = baseRules + " & z < a" + (0x0308).ToString(CultureInfo.InvariantCulture);
            try
            {
                myCollation = new RuleBasedCollator(newRules);
            }
            catch (Exception e)
            {
                Errln("Demo Test 1 Table Collation object creation failed.");
                return;
            }
            for (i = 0; i < 2; i++)
            {
                doTest(myCollation, testSourceCases[i], testTargetCases[i], -1);
            }
        }

        void doTest(RuleBasedCollator myCollation, String mysource, String target, int result)
        {
            int compareResult = myCollation.Compare(source, target);
            CollationKey sortKey1, sortKey2;

            try
            {
                sortKey1 = myCollation.GetCollationKey(source);
                sortKey2 = myCollation.GetCollationKey(target);
            }
            catch (Exception e)
            {
                Errln("SortKey generation Failed.\n");
                return;
            }
            int keyResult = sortKey1.CompareTo(sortKey2);
            reportCResult(mysource, target, sortKey1, sortKey2, compareResult, keyResult, compareResult, result);
        }

        public void reportCResult(String src, String target, CollationKey sourceKey, CollationKey targetKey,
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
                    Logln(msg1 + src + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + src + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
                msg1 = ok2 ? "Ok: key(\"" : "FAIL: key(\"";
                msg2 = "\").compareTo(key(\"";
                msg3 = "\")) returned ";
                sResult = CollationTest.AppendCompareResult(keyResult, sResult);
                if (ok2)
                {
                    Logln(msg1 + src + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + src + msg2 + target + msg3 + sResult + msg4 + sExpect);
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
                    Logln(msg1 + src + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + src + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
            }
        }
    }
}
