using ICU4N.Text;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Error condition test of Transliterator
    /// </summary>
    public class ErrorTest : TestFmwk
    {
        [Test]
        public void TestTransliteratorErrors()
        {
            String trans = "Latin-Greek";
            String bogusID = "LATINGREEK-GREEKLATIN";
            String newID = "Bogus-Latin";
            String newIDRules = "zzz > Z; f <> ph";
            String bogusRules = "a } [b-g m-p ";
            ReplaceableString testString =
                new ReplaceableString("A quick fox jumped over the lazy dog.");
            String insertString = "cats and dogs";
            int stoppedAt = 0, len;
            TransliterationPosition pos = new TransliterationPosition();

            Transliterator t =
                Transliterator.GetInstance(trans, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: construction of Latin-Greek");
                return;
            }
            len = testString.Length;
            stoppedAt = t.Transliterate(testString, 0, 100);
            if (stoppedAt != -1)
            {
                Errln("FAIL: Out of bounds check failed (1).");
            }
            else if (testString.Length != len)
            {
                testString =
                    new ReplaceableString("A quick fox jumped over the lazy dog.");
                Errln("FAIL: Transliterate fails and the target string was modified.");
            }
            stoppedAt = t.Transliterate(testString, 100, testString.Length - 1);
            if (stoppedAt != -1)
            {
                Errln("FAIL: Out of bounds check failed (2).");
            }
            else if (testString.Length != len)
            {
                testString =
                    new ReplaceableString("A quick fox jumped over the lazy dog.");
                Errln("FAIL: Transliterate fails and the target string was modified.");
            }
            pos.Start = 100;
            pos.Limit = testString.Length;
            try
            {
                t.Transliterate(testString, pos);
                Errln("FAIL: Start offset is out of bounds, error not reported.");
            }
            catch (ArgumentException e)
            {
                Logln("Start offset is out of bounds and detected.");
            }
            pos.Limit = 100;
            pos.Start = 0;

            try
            {
                t.Transliterate(testString, pos);
                Errln("FAIL: Limit offset is out of bounds, error not reported.\n");
            }
            catch (ArgumentException e)
            {
                Logln("Start offset is out of bounds and detected.");
            }
            len = pos.ContextLimit = testString.Length;
            pos.ContextStart = 0;
            pos.Limit = len - 1;
            pos.Start = 5;
            try
            {
                t.Transliterate(testString, pos, insertString);
                if (len == pos.Limit)
                {
                    Errln("FAIL: Test insertion with string: the transliteration position limit didn't change as expected.");
                }
            }
            catch (ArgumentException e)
            {
                Errln("Insertion test with string failed for some reason.");
            }
            pos.ContextStart = 0;
            pos.ContextLimit = testString.Length;
            pos.Limit = testString.Length - 1;
            pos.Start = 5;
            try
            {
                t.Transliterate(testString, pos, 0x0061);
                if (len == pos.Limit)
                {
                    Errln("FAIL: Test insertion with character: the transliteration position limit didn't change as expected.");
                }
            }
            catch (ArgumentException e)
            {
                Errln("FAIL: Insertion test with UTF-16 code point failed for some reason.");
            }
            len = pos.Limit = testString.Length;
            pos.ContextStart = 0;
            pos.ContextLimit = testString.Length - 1;
            pos.Start = 5;
            try
            {
                t.Transliterate(testString, pos, insertString);
                Errln("FAIL: Out of bounds check failed (3).");
                if (testString.Length != len)
                {
                    Errln("FAIL: The input string was modified though the offsets were out of bounds.");
                }
            }
            catch (ArgumentException e)
            {
                Logln("Insertion test with out of bounds indexes.");
            }
            Transliterator t1 = null;
            try
            {
                t1 = Transliterator.GetInstance(bogusID, Transliterator.Forward);
                if (t1 != null)
                {
                    Errln("FAIL: construction of bogus ID \"LATINGREEK-GREEKLATIN\"");
                }
            }
            catch (ArgumentException e)
            {
            }

            //try { // unneeded - Exception cannot be thrown
            Transliterator t2 =
                Transliterator.CreateFromRules(
                    newID,
                    newIDRules,
                    Transliterator.Forward);
            try
            {
                Transliterator t3 = t2.GetInverse();
                Errln("FAIL: The newID transliterator was not registered so createInverse should fail.");
                if (t3 != null)
                {
                    Errln("FAIL: The newID transliterator was not registered so createInverse should fail.");
                }
            }
            catch (Exception e)
            {
            }
            //} catch (Exception e) { }
            try
            {
                Transliterator t4 =
                    Transliterator.CreateFromRules(
                        newID,
                        bogusRules,
                        Transliterator.Forward);
                if (t4 != null)
                {
                    Errln("FAIL: The rules is malformed but error was not reported.");
                }
            }
            catch (Exception e)
            {
            }
        }

        [Test]
        public void TestUnicodeSetErrors()
        {
            String badPattern = "[[:L:]-[0x0300-0x0400]";
            UnicodeSet set = new UnicodeSet();
            //String result;

            if (!set.IsEmpty)
            {
                Errln("FAIL: The default ctor of UnicodeSet created a non-empty object.");
            }
            try
            {
                set.ApplyPattern(badPattern);
                Errln("FAIL: Applied a bad pattern to the UnicodeSet object okay.");
            }
            catch (ArgumentException e)
            {
                Logln("Test applying with the bad pattern.");
            }
            try
            {
                new UnicodeSet(badPattern);
                Errln("FAIL: Created a UnicodeSet based on bad patterns.");
            }
            catch (ArgumentException e)
            {
                Logln("Test constructing with the bad pattern.");
            }
        }

        //    public void TestUniToHexErrors() {
        //        Transliterator t = null;
        //        try {
        //            t = new UnicodeToHexTransliterator("", true, null);
        //            if (t != null) {
        //                Errln("FAIL: Created a UnicodeToHexTransliterator with an empty pattern.");
        //            }
        //        } catch (ArgumentException e) {
        //        }
        //        try {
        //            t = new UnicodeToHexTransliterator("\\x", true, null);
        //            if (t != null) {
        //                Errln("FAIL: Created a UnicodeToHexTransliterator with a bad pattern.");
        //            }
        //        } catch (ArgumentException e) {
        //        }
        //        t = new UnicodeToHexTransliterator();
        //        try {
        //            ((UnicodeToHexTransliterator) t).applyPattern("\\x");
        //            Errln("FAIL: UnicodeToHexTransliterator::applyPattern succeeded with a bad pattern.");
        //        } catch (Exception e) {
        //        }
        //    }

        [Test]
        public void TestRBTErrors()
        {

            String rules = "ab>y";
            String id = "MyRandom-YReverse";
            String goodPattern = "[[:L:]&[\\u0000-\\uFFFF]]"; /* all BMP letters */
            UnicodeSet set = null;
            try
            {
                set = new UnicodeSet(goodPattern);
                try
                {
                    Transliterator t =
                        Transliterator.CreateFromRules(id, rules, Transliterator.Reverse);
                    t.Filter = (set);
                    Transliterator.RegisterType(id, t.GetType(), null);
                    Transliterator.Unregister(id);
                    try
                    {
                        Transliterator.GetInstance(id, Transliterator.Reverse);
                        Errln("FAIL: construction of unregistered ID should have failed.");
                    }
                    catch (ArgumentException e)
                    {
                    }
                }
                catch (ArgumentException e)
                {
                    Errln("FAIL: Was not able to create a good RBT to test registration.");
                }
            }
            catch (ArgumentException e)
            {
                Errln("FAIL: Was not able to create a good UnicodeSet based on valid patterns.");
                return;
            }
        }

        //    public void TestHexToUniErrors() {
        //        Transliterator t = null;
        //        //try { // unneeded - exception cannot be thrown
        //        t = new HexToUnicodeTransliterator("", null);
        //        //} catch (Exception e) {
        //        //    Errln("FAIL: Could not create a HexToUnicodeTransliterator with an empty pattern.");
        //        //}
        //        try {
        //            t = new HexToUnicodeTransliterator("\\x", null);
        //            Errln("FAIL: Created a HexToUnicodeTransliterator with a bad pattern.");
        //        } catch (ArgumentException e) {
        //        }
        //
        //        t = new HexToUnicodeTransliterator();
        //        try {
        //            ((HexToUnicodeTransliterator) t).applyPattern("\\x");
        //            Errln("FAIL: HexToUnicodeTransliterator::applyPattern succeeded with a bad pattern.");
        //        } catch (ArgumentException e) {
        //        }
        //    }
    }
}
