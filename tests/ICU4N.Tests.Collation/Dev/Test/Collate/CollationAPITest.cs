using ICU4N.Impl;
using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using CollatorFactory = ICU4N.Text.Collator.CollatorFactory;
using StringBuffer = System.Text.StringBuilder;

/// <summary>
/// Port From:   ICU4C v2.1 : collate/CollationAPITest
/// Source File: $ICU4CRoot/source/test/intltest/apicoll.cpp
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationAPITest : TestFmwk
    {
        /**
         * This tests the collation key related APIs.
         * - constructor/destructor
         * - Collator.getCollationKey
         * - == and != operators
         * - comparison between collation keys
         * - creating collation key with a byte array and vice versa
         */
        [Test]
        public void TestCollationKey()
        {
            Logln("testing CollationKey begins...");
            Collator col = Collator.GetInstance();
            col.Strength = (Collator.Tertiary);

            String test1 = "Abcda";
            String test2 = "abcda";

            Logln("Testing weird arguments");
            CollationKey sortk1 = col.GetCollationKey("");
            // key gets reset here
            byte[] bytes = sortk1.ToByteArray();
            DoAssert(bytes.Length == 3 && bytes[0] == 1 && bytes[1] == 1
                     && bytes[2] == 0,
                     "Empty string should return a collation key with empty levels");

            // Most control codes and CGJ are completely ignorable.
            // A string with only completely ignorables must compare equal to an empty string.
            CollationKey sortkIgnorable = col.GetCollationKey("\u0001\u034f");
            DoAssert(sortkIgnorable != null && sortkIgnorable.ToByteArray().Length == 3,
                     "Completely ignorable string should return a collation key with empty levels");
            DoAssert(sortkIgnorable.CompareTo(sortk1) == 0,
                     "Completely ignorable string should compare equal to empty string");

            // bogus key returned here
            sortk1 = col.GetCollationKey(null);
            DoAssert(sortk1 == null, "Error code should return bogus collation key");

            Logln("Use tertiary comparison level testing ....");
            sortk1 = col.GetCollationKey(test1);
            CollationKey sortk2 = col.GetCollationKey(test2);
            DoAssert((sortk1.CompareTo(sortk2)) > 0, "Result should be \"Abcda\" >>> \"abcda\"");

            CollationKey sortkNew;
            sortkNew = sortk1;
            DoAssert(!(sortk1.Equals(sortk2)), "The sort keys should be different");
            DoAssert((sortk1.GetHashCode() != sortk2.GetHashCode()), "sort key hashCode() failed");
            DoAssert((sortk1.Equals(sortkNew)), "The sort keys assignment failed");
            DoAssert((sortk1.GetHashCode() == sortkNew.GetHashCode()), "sort key hashCode() failed");

            // port from apicoll
            try
            {
                col = Collator.GetInstance();
            }
            catch (Exception e)
            {
                Errln("Collator.GetInstance() failed");
            }
            if (col.Strength != Collator.Tertiary)
            {
                Errln("Default collation did not have tertiary strength");
            }

            // Need to use identical strength
            col.Strength = (Collator.Identical);

            CollationKey key1 = col.GetCollationKey(test1);
            CollationKey key2 = col.GetCollationKey(test2);
            CollationKey key3 = col.GetCollationKey(test2);

            DoAssert(key1.CompareTo(key2) > 0,
                     "Result should be \"Abcda\" > \"abcda\"");
            DoAssert(key2.CompareTo(key1) < 0,
                    "Result should be \"abcda\" < \"Abcda\"");
            DoAssert(key2.CompareTo(key3) == 0,
                    "Result should be \"abcda\" ==  \"abcda\"");

            byte[] key2identical = key2.ToByteArray();

            Logln("Use secondary comparision level testing ...");
            col.Strength = (Collator.Secondary);

            key1 = col.GetCollationKey(test1);
            key2 = col.GetCollationKey(test2);
            key3 = col.GetCollationKey(test2);

            DoAssert(key1.CompareTo(key2) == 0,
                    "Result should be \"Abcda\" == \"abcda\"");
            DoAssert(key2.CompareTo(key3) == 0,
                    "Result should be \"abcda\" ==  \"abcda\"");

            byte[] tempkey = key2.ToByteArray();
            byte[] subkey2compat = new byte[tempkey.Length];
            System.Array.Copy(key2identical, 0, subkey2compat, 0, tempkey.Length);
            subkey2compat[subkey2compat.Length - 1] = 0;
            DoAssert(Arrays.Equals(tempkey, subkey2compat),
                     "Binary format for 'abcda' sortkey different for secondary strength!");

            Logln("testing sortkey ends...");
        }

        [Test]
        public void TestRawCollationKey()
        {
            // testing constructors
            RawCollationKey key = new RawCollationKey();
            if (key.Bytes != null || key.Length != 0)
            {
                Errln("Empty default constructor expected to leave the bytes null "
                      + "and size 0");
            }
            byte[] array = new byte[128];
            key = new RawCollationKey(array);
            if (key.Bytes != array || key.Length != 0)
            {
                Errln("Constructor taking an array expected to adopt it and "
                      + "retaining its size 0");
            }
            try
            {
                key = new RawCollationKey(array, 129);
                Errln("Constructor taking an array and a size > array.Length "
                      + "expected to throw an exception");
            }
            catch (IndexOutOfRangeException e)
            {
                Logln("PASS: Constructor failed as expected");
            }
            try
            {
                key = new RawCollationKey(array, -1);
                Errln("Constructor taking an array and a size < 0 "
                      + "expected to throw an exception");
            }
            catch (IndexOutOfRangeException e)
            {
                Logln("PASS: Constructor failed as expected");
            }
            key = new RawCollationKey(array, array.Length >> 1);
            if (key.Bytes != array || key.Length != (array.Length >> 1))
            {
                Errln("Constructor taking an array and a size, "
                      + "expected to adopt it and take the size specified");
            }
            key = new RawCollationKey(10);
            if (key.Bytes == null || key.Bytes.Length != 10 || key.Length != 0)
            {
                Errln("Constructor taking a specified capacity expected to "
                      + "create a new internal byte array with length 10 and "
                      + "retain size 0");
            }
        }

        internal void DoAssert(bool conditions, String message)
        {
            if (!conditions)
            {
                Errln(message);
            }
        }

        /**
         * This tests the comparison convenience methods of a collator object.
         * - greater than
         * - greater than or equal to
         * - equal to
         */
        [Test]
        public void TestCompare()
        {
            Logln("The compare tests begin : ");
            Collator col = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);

            String test1 = "Abcda";
            String test2 = "abcda";
            Logln("Use tertiary comparison level testing ....");

            DoAssert((!col.Equals(test1, test2)), "Result should be \"Abcda\" != \"abcda\"");
            DoAssert((col.Compare(test1, test2) > 0), "Result should be \"Abcda\" >>> \"abcda\"");

            col.Strength = (Collator.Secondary);
            Logln("Use secondary comparison level testing ....");

            DoAssert((col.Equals(test1, test2)), "Result should be \"Abcda\" == \"abcda\"");
            DoAssert((col.Compare(test1, test2) == 0), "Result should be \"Abcda\" == \"abcda\"");

            col.Strength = (Collator.Primary);
            Logln("Use primary comparison level testing ....");

            DoAssert((col.Equals(test1, test2)), "Result should be \"Abcda\" == \"abcda\"");
            DoAssert((col.Compare(test1, test2) == 0), "Result should be \"Abcda\" == \"abcda\"");
            Logln("The compare tests end.");
        }

        /**
        * Tests decomposition setting
        */
        [Test]
        public void TestDecomposition()
        {
            Collator en_US = null, el_GR = null, vi_VN = null;

            en_US = Collator.GetInstance(new CultureInfo("en-US"));
            el_GR = Collator.GetInstance(new CultureInfo("el-GR"));
            vi_VN = Collator.GetInstance(new CultureInfo("vi-VN"));


            // there is no reason to have canonical decomposition in en_US OR default locale */
            if (vi_VN.Decomposition != Collator.CanonicalDecomposition)
            {
                Errln("vi_VN collation did not have cannonical decomposition for normalization!");
            }

            if (el_GR.Decomposition != Collator.CanonicalDecomposition)
            {
                Errln("el_GR collation did not have cannonical decomposition for normalization!");
            }

            if (en_US.Decomposition != Collator.NoDecomposition)
            {
                Errln("en_US collation had cannonical decomposition for normalization!");
            }
        }

        /**
         * This tests the duplication of a collator object.
         */
        [Test]
        public void TestDuplicate()
        {
            //Clone does not be implemented
            Collator col1 = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);

            // Collator col2 = (Collator)col1.clone();
            // doAssert(col1.Equals(col2), "Cloned object is not equal to the orginal");
            String ruleset = "&9 < a, A < b, B < c, C < d, D, e, E";
            RuleBasedCollator col3 = null;
            try
            {
                col3 = new RuleBasedCollator(ruleset);
            }
            catch (Exception e)
            {
                Errln("Failure creating RuleBasedCollator with rule: \"" + ruleset + "\"\n" + e);
                return;
            }
            DoAssert(!col1.Equals(col3), "Cloned object is equal to some dummy");
            col3 = (RuleBasedCollator)col1;
            DoAssert(col1.Equals(col3), "Copied object is not equal to the orginal");

        }

        /**
         * This tests the CollationElementIterator related APIs.
         * - creation of a CollationElementIterator object
         * - == and != operators
         * - iterating forward
         * - reseting the iterator index
         * - requesting the order properties(primary, secondary or tertiary)
         */
        [Test]
        public void TestElemIter()
        {
            // Logln("testing sortkey begins...");
            Collator col = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);


            String testString1 = "XFILE What subset of all possible test cases has the highest probability of detecting the most errors?";
            String testString2 = "Xf_ile What subset of all possible test cases has the lowest probability of detecting the least errors?";
            // Logln("Constructors and comparison testing....");
            CollationElementIterator iterator1 = ((RuleBasedCollator)col).GetCollationElementIterator(testString1);

            CharacterIterator chariter = new StringCharacterIterator(testString1);
            // copy ctor
            CollationElementIterator iterator2 = ((RuleBasedCollator)col).GetCollationElementIterator(chariter);
            UCharacterIterator uchariter = UCharacterIterator.GetInstance(testString2);
            CollationElementIterator iterator3 = ((RuleBasedCollator)col).GetCollationElementIterator(uchariter);

            int offset = 0;
            offset = iterator1.GetOffset();
            if (offset != 0)
            {
                Errln("Error in getOffset for collation element iterator");
                return;
            }
            iterator1.SetOffset(6);
            iterator1.SetOffset(0);
            int order1, order2, order3;

            order1 = iterator1.Next();
            DoAssert(!(iterator1.Equals(iterator2)), "The first iterator advance failed");
            order2 = iterator2.Next();

            // ICU4N specific - in .NET it is not possible to "catch" a Debug.Assert,
            // so we must skip this test (or actually design a hash code)

            //// Code coverage for dummy "not designed" hashCode() which does "assert false".
            //try
            //{
            //    iterator1.GetHashCode();  // We don't expect any particular value.
            //}
            //catch (InvalidOperationException ignored)
            //{
            //    // Expected to be thrown if assertions are enabled.
            //}

            // In ICU 52 and earlier we had iterator1.Equals(iterator2)
            // but in ICU 53 this fails because the iterators differ (String vs. CharacterIterator).
            // doAssert((iterator1.Equals(iterator2)), "The second iterator advance failed");
            DoAssert(iterator1.GetOffset() == iterator2.GetOffset(), "The second iterator advance failed");
            DoAssert((order1 == order2), "The order result should be the same");
            order3 = iterator3.Next();

            DoAssert((CollationElementIterator.PrimaryOrder(order1) ==
                CollationElementIterator.PrimaryOrder(order3)), "The primary orders should be the same");
            DoAssert((CollationElementIterator.SecondaryOrder(order1) ==
                CollationElementIterator.SecondaryOrder(order3)), "The secondary orders should be the same");
            DoAssert((CollationElementIterator.TertiaryOrder(order1) ==
                CollationElementIterator.TertiaryOrder(order3)), "The tertiary orders should be the same");

            order1 = iterator1.Next();
            order3 = iterator3.Next();

            DoAssert((CollationElementIterator.PrimaryOrder(order1) ==
                CollationElementIterator.PrimaryOrder(order3)), "The primary orders should be identical");
            DoAssert((CollationElementIterator.TertiaryOrder(order1) !=
                CollationElementIterator.TertiaryOrder(order3)), "The tertiary orders should be different");

            order1 = iterator1.Next();
            order3 = iterator3.Next();
            // invalid test wrong in UCA
            // doAssert((CollationElementIterator.SecondaryOrder(order1) !=
            //    CollationElementIterator.SecondaryOrder(order3)), "The secondary orders should not be the same");

            DoAssert((order1 != CollationElementIterator.NullOrder), "Unexpected end of iterator reached");

            iterator1.Reset();
            iterator2.Reset();
            iterator3.Reset();
            order1 = iterator1.Next();

            DoAssert(!(iterator1.Equals(iterator2)), "The first iterator advance failed");

            order2 = iterator2.Next();

            // In ICU 52 and earlier we had iterator1.Equals(iterator2)
            // but in ICU 53 this fails because the iterators differ (String vs. CharacterIterator).
            // doAssert((iterator1.Equals(iterator2)), "The second iterator advance failed");
            DoAssert(iterator1.GetOffset() == iterator2.GetOffset(), "The second iterator advance failed");
            DoAssert((order1 == order2), "The order result should be the same");

            order3 = iterator3.Next();

            DoAssert((CollationElementIterator.PrimaryOrder(order1) ==
                CollationElementIterator.PrimaryOrder(order3)), "The primary orders should be the same");
            DoAssert((CollationElementIterator.SecondaryOrder(order1) ==
                CollationElementIterator.SecondaryOrder(order3)), "The secondary orders should be the same");
            DoAssert((CollationElementIterator.TertiaryOrder(order1) ==
                CollationElementIterator.TertiaryOrder(order3)), "The tertiary orders should be the same");

            order1 = iterator1.Next();
            order2 = iterator2.Next();
            order3 = iterator3.Next();

            DoAssert((CollationElementIterator.PrimaryOrder(order1) ==
                CollationElementIterator.PrimaryOrder(order3)), "The primary orders should be identical");
            DoAssert((CollationElementIterator.TertiaryOrder(order1) !=
                CollationElementIterator.TertiaryOrder(order3)), "The tertiary orders should be different");

            order1 = iterator1.Next();
            order3 = iterator3.Next();

            // obsolete invalid test, removed
            // doAssert((CollationElementIterator.SecondaryOrder(order1) !=
            //    CollationElementIterator.SecondaryOrder(order3)), "The secondary orders should not be the same");
            DoAssert((order1 != CollationElementIterator.NullOrder), "Unexpected end of iterator reached");
            DoAssert(!(iterator2.Equals(iterator3)), "The iterators should be different");
            Logln("testing CollationElementIterator ends...");
        }

        /**
         * This tests the hashCode method of a collator object.
         */
        [Test]
        public void TestHashCode()
        {
            Logln("hashCode tests begin.");
            Collator col1 = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);

            Collator col2 = null;
            CultureInfo dk = new CultureInfo("da-DK");
            try
            {
                col2 = Collator.GetInstance(dk);
            }
            catch (Exception e)
            {
                Errln("Danish collation creation failed.");
                return;
            }

            Collator col3 = null;
            try
            {
                col3 = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);
            }
            catch (Exception e)
            {
                Errln("2nd default collation creation failed.");
                return;
            }

            Logln("Collator.GetHashCode() testing ...");

            DoAssert(col1.GetHashCode() != col2.GetHashCode(), "Hash test1 result incorrect");
            DoAssert(!(col1.GetHashCode() == col2.GetHashCode()), "Hash test2 result incorrect");
            DoAssert(col1.GetHashCode() == col3.GetHashCode(), "Hash result not equal");

            Logln("hashCode tests end.");

            String test1 = "Abcda";
            String test2 = "abcda";

            CollationKey sortk1, sortk2, sortk3;

            sortk1 = col3.GetCollationKey(test1);
            sortk2 = col3.GetCollationKey(test2);
            sortk3 = col3.GetCollationKey(test2);

            DoAssert(sortk1.GetHashCode() != sortk2.GetHashCode(), "Hash test1 result incorrect");
            DoAssert(sortk2.GetHashCode() == sortk3.GetHashCode(), "Hash result not equal");
        }

        /**
         * This tests the properties of a collator object.
         * - constructor
         * - factory method getInstance
         * - compare and getCollationKey
         * - get/set decomposition mode and comparison level
         */
        [Test]
        public void TestProperty()
        {
            /*
              All the collations have the same version in an ICU
              version.
              ICU 2.0 currVersionArray = {0x18, 0xC0, 0x02, 0x02};
              ICU 2.1 currVersionArray = {0x19, 0x00, 0x03, 0x03};
              ICU 2.8 currVersionArray = {0x29, 0x80, 0x00, 0x04};
            */
            Logln("The property tests begin : ");
            Logln("Test ctors : ");
            Collator col = Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);

            Logln("Test getVersion");
            // Check for a version greater than some value rather than equality
            // so that we need not update the expected version each time.
            VersionInfo expectedVersion = VersionInfo.GetInstance(0x31, 0xC0, 0x00, 0x05);  // from ICU 4.4/UCA 5.2
            DoAssert(col.GetVersion().CompareTo(expectedVersion) >= 0, "Expected minimum version " + expectedVersion.ToString() + " got " + col.GetVersion().ToString());

            Logln("Test getUCAVersion");
            // Assume that the UCD and UCA versions are the same,
            // rather than hardcoding (and updating each time) a particular UCA version.
            VersionInfo ucdVersion = UChar.UnicodeVersion;
            VersionInfo ucaVersion = col.GetUCAVersion();
            DoAssert(ucaVersion.Equals(ucdVersion),
                    "Expected UCA version " + ucdVersion.ToString() + " got " + col.GetUCAVersion().ToString());

            DoAssert((col.Compare("ab", "abc") < 0), "ab < abc comparison failed");
            DoAssert((col.Compare("ab", "AB") < 0), "ab < AB comparison failed");
            DoAssert((col.Compare("blackbird", "black-bird") > 0), "black-bird > blackbird comparison failed");
            DoAssert((col.Compare("black bird", "black-bird") < 0), "black bird > black-bird comparison failed");
            DoAssert((col.Compare("Hello", "hello") > 0), "Hello > hello comparison failed");

            Logln("Test ctors ends.");

            Logln("testing Collator.Strength method ...");
            DoAssert((col.Strength == Collator.Tertiary), "collation object has the wrong strength");
            DoAssert((col.Strength != Collator.Primary), "collation object's strength is primary difference");

            Logln("testing Collator.setStrength() method ...");
            col.Strength = (Collator.Secondary);
            DoAssert((col.Strength != Collator.Tertiary), "collation object's strength is secondary difference");
            DoAssert((col.Strength != Collator.Primary), "collation object's strength is primary difference");
            DoAssert((col.Strength == Collator.Secondary), "collation object has the wrong strength");

            Logln("testing Collator.setDecomposition() method ...");
            col.Decomposition = Collator.NoDecomposition;
            DoAssert((col.Decomposition != Collator.CanonicalDecomposition), "Decomposition mode != Collator.CANONICAL_DECOMPOSITION");
            DoAssert((col.Decomposition == Collator.NoDecomposition), "Decomposition mode = Collator.NO_DECOMPOSITION");


            RuleBasedCollator rcol = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("da-DK"));
            DoAssert(rcol.GetRules().Length != 0, "da_DK rules does not have length 0");

            try
            {
                col = Collator.GetInstance(new CultureInfo("fr")  /*Locale.FRENCH*/);
            }
            catch (Exception e)
            {
                Errln("Creating French collation failed.");
                return;
            }

            col.Strength = (Collator.Primary);
            Logln("testing Collator.Strength method again ...");
            DoAssert((col.Strength != Collator.Tertiary), "collation object has the wrong strength");
            DoAssert((col.Strength == Collator.Primary), "collation object's strength is not primary difference");

            Logln("testing French Collator.setStrength() method ...");
            col.Strength = (Collator.Tertiary);
            DoAssert((col.Strength == Collator.Tertiary), "collation object's strength is not tertiary difference");
            DoAssert((col.Strength != Collator.Primary), "collation object's strength is primary difference");
            DoAssert((col.Strength != Collator.Secondary), "collation object's strength is secondary difference");

        }

        [Test]
        public void TestJunkCollator()
        {
            Logln("Create junk collation: ");
            // ICU4N TODO: We can't create unknown cultures in .NET
            // so, using invariant culture here.
            //Locale abcd = new Locale("ab", "CD", "");
            var abcd = CultureInfo.InvariantCulture;

            Collator junk = Collator.GetInstance(abcd);
            Collator col = Collator.GetInstance();


            String colrules = ((RuleBasedCollator)col).GetRules();
            String junkrules = ((RuleBasedCollator)junk).GetRules();
            DoAssert(colrules == junkrules || colrules.Equals(junkrules),
                       "The default collation should be returned.");
            Collator frCol = null;
            try
            {
                frCol = Collator.GetInstance(new CultureInfo("fr-CA") /*Locale.CANADA_FRENCH*/);
            }
            catch (Exception e)
            {
                Errln("Creating fr_CA collator failed.");
                return;
            }

            DoAssert(!(frCol.Equals(junk)), "The junk is the same as the fr_CA collator.");
            Logln("Collator property test ended.");

        }

        /**
        * This tests the RuleBasedCollator
        * - constructor/destructor
        * - getRules
        */
        [Test]
        public void TestRuleBasedColl()
        {
            RuleBasedCollator col1 = null, col2 = null, col3 = null, col4 = null;

            String ruleset1 = "&9 < a, A < b, B < c, C; ch, cH, Ch, CH < d, D, e, E";
            String ruleset2 = "&9 < a, A < b, B < c, C < d, D, e, E";
            String ruleset3 = "&";

            try
            {
                col1 = new RuleBasedCollator(ruleset1);
            }
            catch (Exception e)
            {
                // only first error needs to be a warning since we exit function
                Warnln("RuleBased Collator creation failed.");
                return;
            }

            try
            {
                col2 = new RuleBasedCollator(ruleset2);
            }
            catch (Exception e)
            {
                Errln("RuleBased Collator creation failed.");
                return;
            }

            try
            {
                // empty rules fail
                col3 = new RuleBasedCollator(ruleset3);
                Errln("Failure: Empty rules for the collator should fail");
                return;
            }
            catch (MissingManifestResourceException e)
            {
                Warnln(e.ToString());
            }
            catch (Exception e)
            {
                Logln("PASS: Empty rules for the collator failed as expected");
            }

            //Locale locale = new Locale("aa", "AA");
            // ICU4N TODO: We can't create unknown cultures in .NET
            var locale = CultureInfo.InvariantCulture;
            try
            {
                col3 = (RuleBasedCollator)Collator.GetInstance(locale);
            }
            catch (Exception e)
            {
                Errln("Fallback Collator creation failed.: %s");
                return;
            }

            try
            {
                col3 = (RuleBasedCollator)Collator.GetInstance();
            }
            catch (Exception e)
            {
                Errln("Default Collator creation failed.: %s");
                return;
            }

            String rule1 = col1.GetRules();
            String rule2 = col2.GetRules();
            String rule3 = col3.GetRules();

            DoAssert(!rule1.Equals(rule2), "Default collator getRules failed");
            DoAssert(!rule2.Equals(rule3), "Default collator getRules failed");
            DoAssert(!rule1.Equals(rule3), "Default collator getRules failed");

            try
            {
                col4 = new RuleBasedCollator(rule2);
            }
            catch (Exception e)
            {
                Errln("RuleBased Collator creation failed.");
                return;
            }

            String rule4 = col4.GetRules();
            DoAssert(rule2.Equals(rule4), "Default collator getRules failed");
            // tests that modifier ! is always ignored
            String exclamationrules = "!&a<b";
            // java does not allow ! to be the start of the rule
            String thaistr = "\u0e40\u0e01\u0e2d";
            try
            {
                RuleBasedCollator col5 = new RuleBasedCollator(exclamationrules);
                RuleBasedCollator encol = (RuleBasedCollator)
                                            Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/);
                CollationElementIterator col5iter
                                       = col5.GetCollationElementIterator(thaistr);
                CollationElementIterator encoliter
                                       = encol.GetCollationElementIterator(
                                                                          thaistr);
                while (true)
                {
                    // testing with en since thai has its own tailoring
                    int ce = col5iter.Next();
                    int ce2 = encoliter.Next();
                    if (ce2 != ce)
                    {
                        Errln("! modifier test failed");
                    }
                    if (ce == CollationElementIterator.NullOrder)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Errln("RuleBased Collator creation failed for ! modifier.");
                return;
            }
        }

        /**
        * This tests the RuleBasedCollator
        * - getRules
        */
        [Test]
        public void TestRules()
        {
            RuleBasedCollator coll = (RuleBasedCollator)Collator.GetInstance(CultureInfo.InvariantCulture  /*new Locale("", "", "")*/); //root
                                                                                                                                        // Logln("PASS: RuleBased Collator creation passed");


            String rules = coll.GetRules();
            if (rules != null && rules.Length != 0)
            {
                Errln("Root tailored rules failed");
            }
        }

        [Test]
        public void TestSafeClone()
        {
            String test1 = "abCda";
            String test2 = "abcda";

            // one default collator & two complex ones
            RuleBasedCollator[] someCollators = {
            (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en")  /*Locale.ENGLISH*/),
            (RuleBasedCollator)Collator.GetInstance(new CultureInfo("ko-KR")  /*Locale.KOREA*/),
            (RuleBasedCollator)Collator.GetInstance(new CultureInfo("ja-JP")  /*Locale.JAPAN*/)
        };
            RuleBasedCollator[] someClonedCollators = new RuleBasedCollator[3];

            // change orig & clone & make sure they are independent

            for (int index = 0; index < someCollators.Length; index++)
            {
                //try
                //{
                someClonedCollators[index]
                            = (RuleBasedCollator)someCollators[index].Clone();
                //}
                //catch (CloneNotSupportedException e)
                //{
                //    Errln("Error cloning collator");
                //}

                someClonedCollators[index].Strength = (Collator.Tertiary);
                someCollators[index].Strength = (Collator.Primary);
                someClonedCollators[index].IsCaseLevel = (false);
                someCollators[index].IsCaseLevel = (false);

                DoAssert(someClonedCollators[index].Compare(test1, test2) > 0,
                         "Result should be \"abCda\" >>> \"abcda\" ");
                DoAssert(someCollators[index].Compare(test1, test2) == 0,
                         "Result should be \"abCda\" == \"abcda\" ");
            }
        }

        [Test]
        public void TestGetTailoredSet()
        {
            Logln("testing getTailoredSet...");
            String[] rules = {
            "&a < \u212b",
            "& S < \u0161 <<< \u0160",
        };
            String[][] data = {
            new string[] { "\u212b", "A\u030a", "\u00c5" },
            new string[] { "\u0161", "s\u030C", "\u0160", "S\u030C" }
        };

            int i = 0, j = 0;

            RuleBasedCollator coll;
            UnicodeSet set;

            for (i = 0; i < rules.Length; i++)
            {
                try
                {
                    Logln("Instantiating a collator from " + rules[i]);
                    coll = new RuleBasedCollator(rules[i]);
                    set = coll.GetTailoredSet();
                    Logln("Got set: " + set.ToPattern(true));
                    if (set.Count < data[i].Length)
                    {
                        Errln("Tailored set size smaller (" + set.Count + ") than expected (" + data[i].Length + ")");
                    }
                    for (j = 0; j < data[i].Length; j++)
                    {
                        Logln("Checking to see whether " + data[i][j] + " is in set");
                        if (!set.Contains(data[i][j]))
                        {
                            Errln("Tailored set doesn't contain " + data[i][j] + "... It should");
                        }
                    }
                }
                catch (Exception e)
                {
                    Warnln("Couldn't open collator with rules " + rules[i]);
                }
            }
        }

        private class TestCollator : Collator
        {
            public override bool Equals(Object that)
            {
                return this == that;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override int Compare(String source, String target)
            {
                return source.CompareToOrdinal(target);
            }

            public override CollationKey GetCollationKey(String source)
            {
                return new CollationKey(source,
                          GetRawCollationKey(source, new RawCollationKey()));
            }

            public override RawCollationKey GetRawCollationKey(String source,
                                                      RawCollationKey key)
            {
                byte[] temp1 = source.GetBytes(Encoding.UTF8);
                byte[] temp2 = new byte[temp1.Length + 1];
                System.Array.Copy(temp1, 0, temp2, 0, temp1.Length);
                temp2[temp1.Length] = 0;
                if (key == null)
                {
                    key = new RawCollationKey();
                }
                key.Bytes = temp2;
                key.Length = temp2.Length;
                return key;
            }

#pragma warning disable 672
            public override int VariableTop
#pragma warning restore 672
            {
                get { return 0; }
                set
                {
                    if (IsFrozen)
                    {
                        throw new NotSupportedException("Attempt to modify frozen object");
                    }
                }
            }

#pragma warning disable 672
            public override int SetVariableTop(string str)
#pragma warning restore 672
            {
                if (IsFrozen)
                {
                    throw new NotSupportedException("Attempt to modify frozen object");
                }

                return 0;
            }

            public override VersionInfo GetVersion()
            {
                return VersionInfo.GetInstance(0);
            }

            public override VersionInfo GetUCAVersion()
            {
                return VersionInfo.GetInstance(0);
            }
        }

        /**
         * Simple test to see if Collator is subclassable.
         * Also test coverage of base class methods that are overridden by RuleBasedCollator.
         */
        [Test]
        public void TestSubClass()
        {


            Collator col1 = new TestCollator();
            Collator col2 = new TestCollator();
            if (col1.Equals(col2))
            {
                Errln("2 different instance of TestCollator should fail");
            }
            if (col1.GetHashCode() != col2.GetHashCode())
            {
                Errln("Every TestCollator has the same hashcode");
            }
            String abc = "abc";
            String bcd = "bcd";
            if (col1.Compare(abc, bcd) != abc.CompareToOrdinal(bcd))
            {
                Errln("TestCollator compare should be the same as the default " +
                      "string comparison");
            }
            CollationKey key = col1.GetCollationKey(abc);
            byte[] temp1 = abc.GetBytes(Encoding.UTF8);
            byte[] temp2 = new byte[temp1.Length + 1];
            System.Array.Copy(temp1, 0, temp2, 0, temp1.Length);
            temp2[temp1.Length] = 0;
            if (!Arrays.Equals(key.ToByteArray(), temp2)
                    || !key.SourceString.Equals(abc))
            {
                Errln("TestCollator collationkey API is returning wrong values");
            }
            UnicodeSet set = col1.GetTailoredSet();
            if (!set.Equals(new UnicodeSet(0, 0x10FFFF)))
            {
                Errln("Error getting default tailored set");
            }

            // Base class code coverage.
            // Most of these methods are dummies;
            // they are overridden by any subclass that supports their features.

            assertEquals("compare(strings as Object)", 0,
                    col1.Compare(new StringBuilder("abc"), new StringBuffer("abc")));

            col1.Strength = (Collator.Secondary);
            assertNotEquals("getStrength()", Collator.Primary, col1.Strength);

            // setStrength2() is @internal and returns this.
            // The base class getStrength() always returns the same value,
            // since the base class does not have a field to store the strength.
            assertNotEquals("setStrength2().Strength", Collator.Primary,
                    col1.SetStrength2(Collator.Identical).Strength);

            // (base class).setDecomposition() may or may not be implemented.
            try
            {
                col1.Decomposition = (Collator.CanonicalDecomposition);
            }
            catch (NotSupportedException expected)
            {
            }
            assertNotEquals("getDecomposition()", -1, col1.Decomposition);  // don't care about the value

            // (base class).setMaxVariable() may or may not be implemented.
            try
            {
                col1.MaxVariable = (ReorderCodes.Currency);
            }
            catch (NotSupportedException expected)
            {
            }
            assertNotEquals("getMaxVariable()", -1, col1.MaxVariable);  // don't care about the value

            // (base class).setReorderCodes() may or may not be implemented.
            try
            {
                col1.SetReorderCodes(0, 1, 2);
            }
            catch (NotSupportedException expected)
            {
            }
            try
            {
                col1.GetReorderCodes();
            }
            catch (NotSupportedException expected)
            {
            }

            assertFalse("getDisplayName()", Collator.GetDisplayName(new CultureInfo("de")) == string.Empty);
            assertFalse("getDisplayName()", Collator.GetDisplayName(new CultureInfo("de"), new CultureInfo("it")) == string.Empty);

            assertNotEquals("getLocale()", ULocale.GERMAN, col1.GetLocale(ULocale.ACTUAL_LOCALE));

            // Cover Collator.setLocale() which is only package-visible.
            Object token = Collator.RegisterInstance(new TestCollator(), new ULocale("de-Japn-419"));
            Collator.Unregister(token);

            // Freezable default implementations. freeze() may or may not be implemented.
            assertFalse("not yet frozen", col2.IsFrozen);
            try
            {
                col2.Freeze();
                assertTrue("now frozen", col2.IsFrozen);
            }
            catch (NotSupportedException expected)
            {
            }
            try
            {
                col2.Strength = (Collator.Primary);
                if (col2.IsFrozen)
                {
                    fail("(frozen Collator).setStrength() should throw an exception");
                }
            }
            catch (NotSupportedException expected)
            {
            }
            try
            {
                Collator col3 = col2.CloneAsThawed();
                assertFalse("!cloneAsThawed().isFrozen()", col3.IsFrozen);
            }
            catch (NotSupportedException expected)
            {
            }
        }

        /**
         * Simple test the collator setter and getters.
         * Similar to C++ apicoll.cpp TestAttribute().
         */
        [Test]
        public void TestSetGet()
        {
            RuleBasedCollator collator = (RuleBasedCollator)Collator.GetInstance();
            NormalizationMode decomp = collator.Decomposition;
            CollationStrength strength = collator.Strength;
            bool alt = collator.IsAlternateHandlingShifted;
            bool caselevel = collator.IsCaseLevel;
            bool french = collator.IsFrenchCollation;
            bool hquart = collator.IsHiraganaQuaternary;
            bool lowercase = collator.IsLowerCaseFirst;
            bool uppercase = collator.IsUpperCaseFirst;

            collator.Decomposition = (Collator.CanonicalDecomposition);
            if (collator.Decomposition != Collator.CanonicalDecomposition)
            {
                Errln("Setting decomposition failed");
            }
            collator.Strength = (Collator.Quaternary);
            if (collator.Strength != Collator.Quaternary)
            {
                Errln("Setting strength failed");
            }
            collator.IsAlternateHandlingShifted = (!alt);
            if (collator.IsAlternateHandlingShifted == alt)
            {
                Errln("Setting alternate handling failed");
            }
            collator.IsCaseLevel = (!caselevel);
            if (collator.IsCaseLevel == caselevel)
            {
                Errln("Setting case level failed");
            }
            collator.IsFrenchCollation = (!french);
            if (collator.IsFrenchCollation == french)
            {
                Errln("Setting french collation failed");
            }
            collator.IsHiraganaQuaternary = (!hquart);
            if (collator.IsHiraganaQuaternary != hquart)
            {
                Errln("Setting hiragana quartenary worked but should be a no-op since ICU 50");
            }
            collator.IsLowerCaseFirst = (!lowercase);
            if (collator.IsLowerCaseFirst == lowercase)
            {
                Errln("Setting lower case first failed");
            }
            collator.IsUpperCaseFirst = (!uppercase);
            if (collator.IsUpperCaseFirst == uppercase)
            {
                Errln("Setting upper case first failed");
            }
            collator.SetDecompositionToDefault();
            if (collator.Decomposition != decomp)
            {
                Errln("Setting decomposition default failed");
            }
            collator.SetStrengthToDefault();
            if (collator.Strength != strength)
            {
                Errln("Setting strength default failed");
            }
            collator.SetAlternateHandlingToDefault();
            if (collator.IsAlternateHandlingShifted != alt)
            {
                Errln("Setting alternate handling default failed");
            }
            collator.SetCaseLevelToDefault();
            if (collator.IsCaseLevel != caselevel)
            {
                Errln("Setting case level default failed");
            }
            collator.SetFrenchCollationToDefault();
            if (collator.IsFrenchCollation != french)
            {
                Errln("Setting french handling default failed");
            }
            collator.SetHiraganaQuaternaryToDefault();
            if (collator.IsHiraganaQuaternary != hquart)
            {
                Errln("Setting Hiragana Quartenary default failed");
            }
            collator.SetCaseFirstToDefault();
            if (collator.IsLowerCaseFirst != lowercase
                || collator.IsUpperCaseFirst != uppercase)
            {
                Errln("Setting case first handling default failed");
            }
        }

        [Test]
        public void TestVariableTopSetting()
        {
            // Use the root collator, not the default collator.
            // This test fails with en_US_POSIX which tailors the dollar sign after 'A'.
            RuleBasedCollator coll = (RuleBasedCollator)Collator.GetInstance(ULocale.ROOT);

            int oldVarTop = coll.VariableTop;

            // ICU 53+: The character must be in a supported reordering group,
            // and the variable top is pinned to the end of that group.
            try
            {
                coll.SetVariableTop("A");
                Errln("setVariableTop(letter) did not detect illegal argument");
            }
            catch (ArgumentException expected)
            {
            }

            // dollar sign (currency symbol)
            int newVarTop = coll.SetVariableTop("$");

            if (newVarTop != coll.VariableTop)
            {
                Errln("setVariableTop(dollar sign) != following getVariableTop()");
            }

            string dollar = "$";
            string euro = "\u20AC";
            int newVarTop2 = coll.SetVariableTop(euro);
            assertEquals("setVariableTop(Euro sign) == following getVariableTop()",
                         newVarTop2, coll.VariableTop);
            assertEquals("setVariableTop(Euro sign) == setVariableTop(dollar sign) (should pin to top of currency group)",
                         newVarTop2, newVarTop);

            coll.IsAlternateHandlingShifted = (true);
            assertEquals("empty==dollar", 0, coll.Compare("", dollar));  // UCOL_EQUAL
            assertEquals("empty==euro", 0, coll.Compare("", euro));  // UCOL_EQUAL
            assertEquals("dollar<zero", -1, coll.Compare(dollar, "0"));  // UCOL_LESS

            coll.VariableTop = (oldVarTop);

            int newerVarTop = coll.SetVariableTop("$");

            if (newVarTop != newerVarTop)
            {
                Errln("Didn't set vartop properly from String!\n");
            }
        }

        [Test]
        public void TestMaxVariable()
        {
            RuleBasedCollator coll = (RuleBasedCollator)Collator.GetInstance(ULocale.ROOT);

            try
            {
                coll.MaxVariable = (ReorderCodes.Others);
                Errln("setMaxVariable(others) did not detect illegal argument");
            }
            catch (ArgumentException expected)
            {
            }

            coll.MaxVariable = (ReorderCodes.Currency);

            if (ReorderCodes.Currency != coll.MaxVariable)
            {
                Errln("setMaxVariable(currency) != following getMaxVariable()");
            }

            coll.IsAlternateHandlingShifted = (true);
            assertEquals("empty==dollar", 0, coll.Compare("", "$"));  // UCOL_EQUAL
            assertEquals("empty==euro", 0, coll.Compare("", "\u20AC"));  // UCOL_EQUAL
            assertEquals("dollar<zero", -1, coll.Compare("$", "0"));  // UCOL_LESS
        }

        [Test]
        [Ignore("ICU4N TOOD: Fix this")]
        public void TestGetLocale()
        {
            String rules = "&a<x<y<z";

            Collator coll = Collator.GetInstance(new ULocale("root"));
            ULocale locale = coll.GetLocale(ULocale.ACTUAL_LOCALE);
            if (!locale.Equals(ULocale.ROOT))
            {
                Errln("Collator.GetInstance(\"root\").getLocale(actual) != ULocale.ROOT; " +
                      "getLocale().getName() = \"" + locale.GetName() + "\"");
            }

            coll = Collator.GetInstance(new ULocale(""));
            locale = coll.GetLocale(ULocale.ACTUAL_LOCALE);
            if (!locale.Equals(ULocale.ROOT))
            {
                Errln("Collator.GetInstance(\"\").getLocale(actual) != ULocale.ROOT; " +
                      "getLocale().getName() = \"" + locale.GetName() + "\"");
            }

            int i = 0;

            string[][] testStruct = {
                // requestedLocale, validLocale, actualLocale
                // Note: ULocale.ROOT.getName() == "" not "root".
                new string[] { "de_DE", "de", "" },
                new string[] { "sr_RS", "sr_Cyrl_RS", "sr" },
                new string[] { "en_US_CALIFORNIA", "en_US", "" },
                new string[] { "fr_FR_NONEXISTANT", "fr", "" },
                // pinyin is the default, therefore suppressed.
                new string[] { "zh_CN", "zh_Hans_CN", "zh" },
                // zh_Hant has default=stroke but the data is in zh.
                new string[] { "zh_TW", "zh_Hant_TW", "zh@collation=stroke" },
                new string[] { "zh_TW@collation=pinyin", "zh_Hant_TW@collation=pinyin", "zh" },
                new string[] { "zh_CN@collation=stroke", "zh_Hans_CN@collation=stroke", "zh@collation=stroke" },
                // yue/yue_Hant aliased to zh_Hant, yue_Hans aliased to zh_Hans.
                new string[] { "yue", "zh_Hant", "zh@collation=stroke" },
                new string[] { "yue_HK", "zh_Hant", "zh@collation=stroke" },
                new string[] { "yue_Hant", "zh_Hant", "zh@collation=stroke" },
                new string[] { "yue_Hant_HK", "zh_Hant", "zh@collation=stroke" },
                new string[] { "yue@collation=pinyin", "zh_Hant@collation=pinyin", "zh" },
                new string[] { "yue_HK@collation=pinyin", "zh_Hant@collation=pinyin", "zh" },
                new string[] { "yue_CN", "zh_Hans", "zh" },
                new string[] { "yue_Hans", "zh_Hans", "zh" },
                new string[] { "yue_Hans_CN", "zh_Hans", "zh" },
                new string[] { "yue_Hans@collation=stroke", "zh_Hans@collation=stroke", "zh@collation=stroke" },
                new string[] { "yue_CN@collation=stroke", "zh_Hans@collation=stroke", "zh@collation=stroke" }
            };

            /* test opening collators for different locales */
            for (i = 0; i < testStruct.Length; i++)
            {
                String requestedLocale = testStruct[i][0];
                String validLocale = testStruct[i][1];
                String actualLocale = testStruct[i][2];
                try
                {
                    coll = Collator.GetInstance(new ULocale(requestedLocale));
                }
                catch (Exception e)
                {
                    Errln(String.Format("Failed to open collator for {0} with {1}", requestedLocale, e));
                    continue;
                }
                // Note: C++ getLocale() recognizes ULOC_REQUESTED_LOCALE
                // which does not exist in Java.
                locale = coll.GetLocale(ULocale.VALID_LOCALE);
                if (!locale.Equals(new ULocale(validLocale)))
                {
                    Errln(String.Format("[Coll {0}]: Error in valid locale, expected {1}, got {2}",
                          requestedLocale, validLocale, locale.GetName()));
                }
                locale = coll.GetLocale(ULocale.ACTUAL_LOCALE);
                if (!locale.Equals(new ULocale(actualLocale)))
                {
                    Errln(String.Format("[Coll {0}]: Error in actual locale, expected {1}, got {2}",
                          requestedLocale, actualLocale, locale.GetName()));
                }
                // If we open a collator for the actual locale, we should get an equivalent one again.
                Collator coll2;
                try
                {
                    coll2 = Collator.GetInstance(locale);
                }
                catch (Exception e)
                {
                    Errln(String.Format("Failed to open collator for actual locale \"{0}\" with {1}",
                            locale.GetName(), e));
                    continue;
                }
                ULocale actual2 = coll2.GetLocale(ULocale.ACTUAL_LOCALE);
                if (!actual2.Equals(locale))
                {
                    Errln(String.Format("[Coll actual \"{0}\"]: Error in actual locale, got different one: \"{1}\"",
                          locale.GetName(), actual2.GetName()));
                }
                if (!coll2.Equals(coll))
                {
                    Errln(String.Format("[Coll actual \"{0}\"]: Got different collator than before",
                            locale.GetName()));
                }
            }

            /* completely non-existent locale for collator should get a root collator */
            {
                try
                {
                    coll = Collator.GetInstance(new ULocale("blahaha"));
                }
                catch (Exception e)
                {
                    Errln("Failed to open collator with " + e);
                    return;
                }
                ULocale valid = coll.GetLocale(ULocale.VALID_LOCALE);
                String name = valid.GetName();
                if (name.Length != 0 && !name.Equals("root"))
                {
                    Errln("Valid locale for nonexisting locale collator is \"" + name + "\" not root");
                }
                ULocale actual = coll.GetLocale(ULocale.ACTUAL_LOCALE);
                name = actual.GetName();
                if (name.Length != 0 && !name.Equals("root"))
                {
                    Errln("Actual locale for nonexisting locale collator is \"" + name + "\" not root");
                }
            }

            /* collator instantiated from rules should have all locales null */
            try
            {
                coll = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Errln("RuleBasedCollator(" + rules + ") failed: " + e);
                return;
            }
            locale = coll.GetLocale(ULocale.VALID_LOCALE);
            if (locale != null)
            {
                Errln(String.Format("For collator instantiated from rules, valid locale {0} is not bogus",
                        locale.GetName()));
            }
            locale = coll.GetLocale(ULocale.ACTUAL_LOCALE);
            if (locale != null)
            {
                Errln(String.Format("For collator instantiated from rules, actual locale {0} is not bogus",
                        locale.GetName()));
            }
        }

        [Test]
        public void TestBounds()
        {
            Collator coll = Collator.GetInstance(new CultureInfo("sh")); //("sh", ""));

            String[] test = { "John Smith", "JOHN SMITH",
                          "john SMITH", "j\u00F6hn sm\u00EFth",
                          "J\u00F6hn Sm\u00EFth", "J\u00D6HN SM\u00CFTH",
                          "john smithsonian", "John Smithsonian",
        };

            String[] testStr = {
                          "\u010CAKI MIHALJ",
                          "\u010CAKI MIHALJ",
                          "\u010CAKI PIRO\u0160KA",
                          "\u010CABAI ANDRIJA",
                          "\u010CABAI LAJO\u0160",
                          "\u010CABAI MARIJA",
                          "\u010CABAI STEVAN",
                          "\u010CABAI STEVAN",
                          "\u010CABARKAPA BRANKO",
                          "\u010CABARKAPA MILENKO",
                          "\u010CABARKAPA MIROSLAV",
                          "\u010CABARKAPA SIMO",
                          "\u010CABARKAPA STANKO",
                          "\u010CABARKAPA TAMARA",
                          "\u010CABARKAPA TOMA\u0160",
                          "\u010CABDARI\u0106 NIKOLA",
                          "\u010CABDARI\u0106 ZORICA",
                          "\u010CABI NANDOR",
                          "\u010CABOVI\u0106 MILAN",
                          "\u010CABRADI AGNEZIJA",
                          "\u010CABRADI IVAN",
                          "\u010CABRADI JELENA",
                          "\u010CABRADI LJUBICA",
                          "\u010CABRADI STEVAN",
                          "\u010CABRDA MARTIN",
                          "\u010CABRILO BOGDAN",
                          "\u010CABRILO BRANISLAV",
                          "\u010CABRILO LAZAR",
                          "\u010CABRILO LJUBICA",
                          "\u010CABRILO SPASOJA",
                          "\u010CADE\u0160 ZDENKA",
                          "\u010CADESKI BLAGOJE",
                          "\u010CADOVSKI VLADIMIR",
                          "\u010CAGLJEVI\u0106 TOMA",
                          "\u010CAGOROVI\u0106 VLADIMIR",
                          "\u010CAJA VANKA",
                          "\u010CAJI\u0106 BOGOLJUB",
                          "\u010CAJI\u0106 BORISLAV",
                          "\u010CAJI\u0106 RADOSLAV",
                          "\u010CAK\u0160IRAN MILADIN",
                          "\u010CAKAN EUGEN",
                          "\u010CAKAN EVGENIJE",
                          "\u010CAKAN IVAN",
                          "\u010CAKAN JULIJAN",
                          "\u010CAKAN MIHAJLO",
                          "\u010CAKAN STEVAN",
                          "\u010CAKAN VLADIMIR",
                          "\u010CAKAN VLADIMIR",
                          "\u010CAKAN VLADIMIR",
                          "\u010CAKARA ANA",
                          "\u010CAKAREVI\u0106 MOMIR",
                          "\u010CAKAREVI\u0106 NEDELJKO",
                          "\u010CAKI \u0160ANDOR",
                          "\u010CAKI AMALIJA",
                          "\u010CAKI ANDRA\u0160",
                          "\u010CAKI LADISLAV",
                          "\u010CAKI LAJO\u0160",
                          "\u010CAKI LASLO" };

            CollationKey[] testKey = new CollationKey[testStr.Length];
            for (int i = 0; i < testStr.Length; i++)
            {
                testKey[i] = coll.GetCollationKey(testStr[i]);
            }

            Array.Sort(testKey);
            for (int i = 0; i < testKey.Length - 1; i++)
            {
                CollationKey lower
                               = testKey[i].GetBound(CollationKeyBoundMode.Lower,
                                                     Collator.Secondary);
                for (int j = i + 1; j < testKey.Length; j++)
                {
                    CollationKey upper
                               = testKey[j].GetBound(CollationKeyBoundMode.Upper,
                                                     Collator.Secondary);
                    for (int k = i; k <= j; k++)
                    {
                        if (lower.CompareTo(testKey[k]) > 0)
                        {
                            Errln("Problem with lower bound at i = " + i + " j = "
                                  + j + " k = " + k);
                        }
                        if (upper.CompareTo(testKey[k]) <= 0)
                        {
                            Errln("Problem with upper bound at i = " + i + " j = "
                                  + j + " k = " + k);
                        }
                    }
                }
            }

            for (int i = 0; i < test.Length; i++)
            {
                CollationKey key = coll.GetCollationKey(test[i]);
                CollationKey lower = key.GetBound(CollationKeyBoundMode.Lower,
                                                  Collator.Secondary);
                CollationKey upper = key.GetBound(CollationKeyBoundMode.UpperLong,
                                                  Collator.Secondary);
                for (int j = i + 1; j < test.Length; j++)
                {
                    key = coll.GetCollationKey(test[j]);
                    if (lower.CompareTo(key) > 0)
                    {
                        Errln("Problem with lower bound i = " + i + " j = " + j);
                    }
                    if (upper.CompareTo(key) <= 0)
                    {
                        Errln("Problem with upper bound i = " + i + " j = " + j);
                    }
                }
            }
        }

        [Test]
        public void TestGetAll()
        {
            CultureInfo[] list = Collator.GetAvailableLocales();
            int errorCount = 0;
            for (int i = 0; i < list.Length; ++i)
            {
                Log("Locale name: ");
                Log(CollectionUtil.ToString(list[i]));
                Log(" , the display name is : ");
                Logln(list[i].DisplayName);
                try
                {
                    Logln("     ...... Or display as: " + Collator.GetDisplayName(list[i]));
                    Logln("     ...... and display in Chinese: " +
                          Collator.GetDisplayName(list[i], new CultureInfo("zh") /* Locale.CHINA */));
                }
                catch (MissingManifestResourceException ex)
                {
                    errorCount++;
                    Logln("could not get displayName for " + list[i]);
                }
            }
            if (errorCount > 0)
            {
                Warnln("Could not load the locale data.");
            }
        }

        private bool DoSetsTest(UnicodeSet @ref, UnicodeSet set, String inSet, String outSet)
        {
            bool ok = true;
            set.Clear();
            set.ApplyPattern(inSet);

            if (!@ref.ContainsAll(set))
            {
                Err("Some stuff from " + inSet + " is not present in the set.\nMissing:" +
                    set.RemoveAll(@ref).ToPattern(true) + "\n");
                ok = false;
            }

            set.Clear();
            set.ApplyPattern(outSet);
            if (!@ref.ContainsNone(set))
            {
                Err("Some stuff from " + outSet + " is present in the set.\nUnexpected:" +
                    set.RetainAll(@ref).ToPattern(true) + "\n");
                ok = false;
            }
            return ok;
        }

        // capitst.c/TestGetContractionsAndUnsafes()
        [Test]
        public void TestGetContractions()
        {
            /*        static struct {
             const char* locale;
             const char* inConts;
             const char* outConts;
             const char* inExp;
             const char* outExp;
             const char* unsafeCodeUnits;
             const char* safeCodeUnits;
             }
             */
            String[][] tests = {
       new string[] {
            "ru",
                    "[{\u0418\u0306}{\u0438\u0306}]",
                    "[\u0439\u0457]",
                    "[\u00e6]",
                    "[ae]",
                    "[\u0418\u0438]",
                    "[aAbBxv]"
                },
                new string[] {
            "uk",
                    "[{\u0406\u0308}{\u0456\u0308}{\u0418\u0306}{\u0438\u0306}]",
                    "[\u0407\u0419\u0439\u0457]",
                    "[\u00e6]",
                    "[ae]",
                    "[\u0406\u0456\u0418\u0438]",
                    "[aAbBxv]"
                },
                new string[] {
            "sh",
                    "[{C\u0301}{C\u030C}{C\u0341}{DZ\u030C}{Dz\u030C}{D\u017D}{D\u017E}{lj}{nj}]",
                    "[{\u309d\u3099}{\u30fd\u3099}]",
                    "[\u00e6]",
                    "[a]",
                    "[nlcdzNLCDZ]",
                    "[jabv]"
                },
                new string[] {
            "ja",
                    /*
                     * The "collv2" builder omits mappings if the collator maps their
                     * character sequences to the same CEs.
                     * For example, it omits Japanese contractions for NFD forms
                     * of the voiced iteration mark (U+309E = U+309D + U+3099), such as
                     * {\u3053\u3099\u309D\u3099}{\u3053\u309D\u3099}
                     * {\u30B3\u3099\u30FD\u3099}{\u30B3\u30FD\u3099}.
                     * It does add mappings for the precomposed forms.
                     */
                    "[{\u3053\u3099\u309D}{\u3053\u3099\u309E}{\u3053\u3099\u30FC}" +
                     "{\u3053\u309D}{\u3053\u309E}{\u3053\u30FC}" +
                     "{\u30B3\u3099\u30FC}{\u30B3\u3099\u30FD}{\u30B3\u3099\u30FE}" +
                     "{\u30B3\u30FC}{\u30B3\u30FD}{\u30B3\u30FE}]",
                    "[{\u30FD\u3099}{\u309D\u3099}{\u3053\u3099}{\u30B3\u3099}{lj}{nj}]",
                    "[\u30FE\u00e6]",
                    "[a]",
                    "[\u3099]",
                    "[]"
                }
    };

            RuleBasedCollator coll = null;
            int i = 0;
            UnicodeSet conts = new UnicodeSet();
            UnicodeSet exp = new UnicodeSet();
            UnicodeSet set = new UnicodeSet();

            for (i = 0; i < tests.Length; i++)
            {
                Logln("Testing locale: " + tests[i][0]);
                coll = (RuleBasedCollator)Collator.GetInstance(new ULocale(tests[i][0]));
                coll.GetContractionsAndExpansions(conts, exp, true);
                bool ok = true;
                Logln("Contractions " + conts.Count + ":\n" + conts.ToPattern(true));
                ok &= DoSetsTest(conts, set, tests[i][1], tests[i][2]);
                Logln("Expansions " + exp.Count + ":\n" + exp.ToPattern(true));
                ok &= DoSetsTest(exp, set, tests[i][3], tests[i][4]);
                if (!ok)
                {
                    // In case of failure, log the rule string for better diagnostics.
                    String rules = coll.GetRules(false);
                    Logln("Collation rules (getLocale()=" +
                            coll.GetLocale(ULocale.ACTUAL_LOCALE).ToString() + "): " +
                            Utility.Escape(rules));
                }

                // No unsafe set in ICU4J
                //noConts = ucol_getUnsafeSet(coll, conts, &status);
                //doSetsTest(conts, set, tests[i][5], tests[i][6]);
                //log_verbose("Unsafes "+conts.size()+":\n"+conts.toPattern(true)+"\n");
            }
        }
        private static readonly String bigone = "One";
        private static readonly String littleone = "one";

        [Test]
        public void TestClone()
        {
            Logln("\ninit c0");
            RuleBasedCollator c0 = (RuleBasedCollator)Collator.GetInstance();
            c0.Strength = (Collator.Tertiary);
            Dump("c0", c0);

            Logln("\ninit c1");
            RuleBasedCollator c1 = (RuleBasedCollator)Collator.GetInstance();
            c1.Strength = (Collator.Tertiary);
            c1.IsUpperCaseFirst = (!c1.IsUpperCaseFirst);
            Dump("c0", c0);
            Dump("c1", c1);
            //try
            //{
            Logln("\ninit c2");
            RuleBasedCollator c2 = (RuleBasedCollator)c1.Clone();
            c2.IsUpperCaseFirst = (!c2.IsUpperCaseFirst);
            Dump("c0", c0);
            Dump("c1", c1);
            Dump("c2", c2);
            if (c1.Equals(c2))
            {
                Errln("The cloned objects refer to same data");
            }
            //}
            //catch (CloneNotSupportedException ex)
            //{
            //    Errln("Could not clone the collator");
            //}
        }

        private void Dump(String msg, RuleBasedCollator c)
        {
            Logln(msg + " " + c.Compare(bigone, littleone) +
                               " s: " + c.Strength +
                               " u: " + c.IsUpperCaseFirst);
        }

        [Test]
        public void TestIterNumeric()
        {  // misnomer for Java, but parallel with C++ test
           // Regression test for ticket #9915.
           // The collation code sometimes masked the continuation marker away
           // but later tested the result for isContinuation().
           // This test case failed because the third bytes of the computed numeric-collation primaries
           // were permutated with the script reordering table.
           // It should have been possible to reproduce this with the root collator
           // and characters with appropriate 3-byte primary weights.
           // The effectiveness of this test depends completely on the collation elements
           // and on the implementation code.
            RuleBasedCollator coll = new RuleBasedCollator("[reorder Hang Hani]");
            coll.IsNumericCollation = (true);
            int result = coll.Compare("40", "72");
            assertTrue("40<72", result < 0);
        }

        /*
         * Tests the method public void setStrength(int newStrength)
         */
        [Test]
        public void TestSetStrength()
        {
            // Tests when if ((newStrength != PRIMARY) && ... ) is true
            int[] cases = { -1, 4, 5 };
            for (int i = 0; i < cases.Length; i++)
            {
                try
                {
                    // Assuming -1 is not one of the values
                    Collator c = Collator.GetInstance();
                    c.Strength = (CollationStrength)(cases[i]);
                    Errln("Collator.setStrength(int) is suppose to return "
                            + "an exception for an invalid newStrength value of " + cases[i]);
                }
                catch (Exception e)
                {
                }
            }
        }

        /*
         * Tests the method public void setDecomposition(int decomposition)
         */
        [Test]
        public void TestSetDecomposition()
        {
            // Tests when if ((decomposition != NO_DECOMPOSITION) && ...) is true
            int[] cases = { 0, 1, 14, 15, 18, 19 };
            for (int i = 0; i < cases.Length; i++)
            {
                try
                {
                    // Assuming -1 is not one of the values
                    Collator c = Collator.GetInstance();
                    c.Decomposition = (NormalizationMode)(cases[i]);
                    Errln("Collator.setDecomposition(int) is suppose to return "
                            + "an exception for an invalid decomposition value of " + cases[i]);
                }
                catch (Exception e)
                {
                }
            }
        }

        // The following class override public Collator createCollator(Locale loc)
        private class TestCreateCollator0 : CollatorFactory
        {
            public override ICollection<String> GetSupportedLocaleIDs()
            {
                return new HashSet<String>();
            }

            public TestCreateCollator0()
                    : base()
            {
            }

            public override Collator CreateCollator(ULocale c)
            {
                return null;
            }
        }

        // The following class override public Collator createCollator(ULocale loc)
        private class TestCreateCollator1 : CollatorFactory
        {
            public override ICollection<String> GetSupportedLocaleIDs()
            {
                return new HashSet<String>();
            }

            public TestCreateCollator1()
                    : base()
            {
            }

            public override Collator CreateCollator(CultureInfo c)
            {
                return null;
            }

            public override bool Visible
            {
                get { return false; }
            }
        }

        /*
         * Tests the class CollatorFactory
         */
        [Test]
        public void TestCreateCollator()
        {



            /*
             * Tests the method public Collator createCollator(Locale loc) using TestCreateCollator1 class
             */
            try
            {
                TestCreateCollator0 tcc = new TestCreateCollator0();
                tcc.CreateCollator(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Errln("Collator.createCollator(Locale) was not suppose to " + "return an exception.");
            }

            /*
             * Tests the method public Collator createCollator(ULocale loc) using TestCreateCollator1 class
             */
            try
            {
                TestCreateCollator1 tcc = new TestCreateCollator1();
                tcc.CreateCollator(new ULocale("en_US"));
            }
            catch (Exception e)
            {
                Errln("Collator.createCollator(ULocale) was not suppose to " + "return an exception.");
            }

            /*
             * Tests the method public String getDisplayName(Locale objectLocale, Locale displayLocale) using TestCreateCollator1 class
             */
            try
            {
                TestCreateCollator0 tcc = new TestCreateCollator0();
                tcc.GetDisplayName(new CultureInfo("en-US"), new CultureInfo("jp-JP"));
            }
            catch (Exception e)
            {
                Errln("Collator.getDisplayName(Locale,Locale) was not suppose to return an exception.");
            }

            /*
             * Tests the method public String getDisplayName(ULocale objectLocale, ULocale displayLocale) using TestCreateCollator1 class
             */
            try
            {
                TestCreateCollator1 tcc = new TestCreateCollator1();
                tcc.GetDisplayName(new ULocale("en_US"), new ULocale("jp_JP"));
            }
            catch (Exception e)
            {
                Errln("Collator.getDisplayName(ULocale,ULocale) was not suppose to return an exception.");
            }
        }
        /* Tests the method
         * public static final String[] getKeywordValues(String keyword)
         */
        [Test]
        public void TestGetKeywordValues()
        {
            // Tests when "if (!keyword.Equals(KEYWORDS[0]))" is true
            String[] cases = { "", "dummy" };
            for (int i = 0; i < cases.Length; i++)
            {
                try
                {
                    // ICU4N: Not sure why there was an instance being created here, only to call the static method, but that won't work in .NET
                    //Collator c = Collator.GetInstance();
                    String[] s = Collator.GetKeywordValues(cases[i]);
                    Errln("Collator.getKeywordValues(String) is suppose to return " +
                            "an exception for an invalid keyword.");
                }
                catch (Exception e) { }
            }
        }

        [Test]
        public void TestBadKeywords()
        {
            // Test locale IDs with errors.
            // Valid locale IDs are tested via data-driven tests.
            // Note: ICU4C tests with a bogus Locale. There is no such thing in ICU4J.

            // Unknown value.
            String localeID = "it-u-ks-xyz";
            try
            {
                Collator.GetInstance(new ULocale(localeID));
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected");
            }
            catch (ArgumentException expected)
            {
            }
            catch (Exception other)
            {
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected - " + other);
            }

            // Unsupported attributes.
            localeID = "it@colHiraganaQuaternary=true";
            try
            {
                Collator.GetInstance(new ULocale(localeID));
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected");
            }
            catch (NotSupportedException expected)
            {
            }
            catch (Exception other)
            {
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected - " + other);
            }

            localeID = "it-u-vt-u24";
            try
            {
                Collator.GetInstance(new ULocale(localeID));
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected");
            }
            catch (NotSupportedException expected)
            {
            }
            catch (Exception other)
            {
                Errln("Collator.GetInstance(" + localeID + ") did not fail as expected - " + other);
            }
        }

        [Test]
        public void TestGapTooSmall()
        {
            // Try to tailor >20k characters into a too-small primary gap between symbols
            // that have 3-byte primary weights.
            // In FractionalUCA.txt:
            // 263A; [0C BA D0, 05, 05]  # Zyyy So  [084A.0020.0002]  * WHITE SMILING FACE
            // 263B; [0C BA D7, 05, 05]  # Zyyy So  [084B.0020.0002]  * BLACK SMILING FACE
            try
            {
                new RuleBasedCollator("&☺<*\u4E00-\u9FFF");
                Errln("no exception for primary-gap overflow");
            }
            catch (NotSupportedException e)
            {
                assertTrue("exception message mentions 'gap'", e.Message.Contains("gap"));
            }
            catch (Exception e)
            {
                Errln("unexpected exception for primary-gap overflow: " + e);
            }

            // CLDR 32/ICU 60 FractionalUCA.txt makes room at the end of the symbols range
            // for several 2-byte primaries, or a large number of 3-byters.
            // The reset point is primary-before what should be
            // the special currency-first-primary contraction,
            // which is hopefully fairly stable, but not guaranteed stable.
            // In FractionalUCA.txt:
            // FDD1 20AC; [0D 70 02, 05, 05]  # CURRENCY first primary
            try
            {
                Collator coll = new RuleBasedCollator("&[before 1]\uFDD1€<*\u4E00-\u9FFF");
                assertTrue("tailored Han before currency", coll.Compare("\u4E00", "$") < 0);
            }
            catch (Exception e)
            {
                Errln("unexpected exception for tailoring many characters at the end of symbols: " + e);
            }
        }
    }
}
