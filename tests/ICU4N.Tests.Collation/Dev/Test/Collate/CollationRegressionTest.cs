using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

//
// Port From:   ICU4C v2.1 : collate/CollationRegressionTest
// Source File: $ICU4CRoot/source/test/intltest/regcoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationRegressionTest : TestFmwk
    {
        // @bug 4048446
        //
        // CollationElementIterator.Reset() doesn't work
        //
        [Test]
        public void Test4048446()
        {
            String test1 = "XFILE What subset of all possible test cases has the highest probability of detecting the most errors?";
            //final String test2 = "Xf_ile What subset of all possible test cases has the lowest probability of detecting the least errors?";
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            CollationElementIterator i1 = en_us.GetCollationElementIterator(test1);
            CollationElementIterator i2 = en_us.GetCollationElementIterator(test1);

            if (i1 == null || i2 == null)
            {
                Errln("Could not create CollationElementIterator's");
                return;
            }

            while (i1.Next() != CollationElementIterator.NullOrder)
            {
                //
            }

            i1.Reset();
            assertEqual(i1, i2);
        }

        internal void assertEqual(CollationElementIterator i1, CollationElementIterator i2)
        {
            int c1, c2, count = 0;

            do
            {
                c1 = i1.Next();
                c2 = i2.Next();

                if (c1 != c2)
                {
                    String msg = "";
                    String msg1 = "    ";

                    msg += msg1 + count;
                    msg += ": strength(0x" + (c1).ToHexString();
                    msg += ") != strength(0x" + (c2).ToHexString();
                    msg += ")";
                    Errln(msg);
                    break;
                }
                count += 1;
            } while (c1 != CollationElementIterator.NullOrder);
        }

        // @bug 4051866
        //
        // Collator -> rules -> Collator round-trip broken for expanding characters
        //
        [Test]
        public void Test4051866()
        {
            String rules = "&n < o & oe ,o\u3080& oe ,\u1530 ,O& OE ,O\u3080& OE ,\u1520< p ,P";

            // Build a collator containing expanding characters
            RuleBasedCollator c1 = null;

            try
            {
                c1 = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + rules);
                return;
            }

            // Build another using the rules from  the first
            RuleBasedCollator c2 = null;
            try
            {
                c2 = new RuleBasedCollator(c1.GetRules());
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + rules);
                return;
            }

            // Make sure they're the same
            if (!(c1.GetRules().Equals(c2.GetRules())))
            {
                Errln("Rules are not equal");
            }
        }

        // @bug 4053636
        //
        // Collator thinks "black-bird" == "black"
        //
        [Test]
        public void Test4053636()
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            if (en_us.Equals("black_bird", "black"))
            {
                Errln("black-bird == black");
            }
        }

        // @bug 4054238
        //
        // CollationElementIterator will not work correctly if the associated
        // Collator object's mode is changed
        //
        [Test]
        public void Test4054238(/* char* par */)
        {
            char[] chars3 = { (char)0x61, (char)0x00FC, (char)0x62, (char)0x65, (char)0x63, (char)0x6b, (char)0x20, (char)0x47, (char)0x72, (char)0x00F6, (char)0x00DF, (char)0x65, (char)0x20, (char)0x4c, (char)0x00FC, (char)0x62, (char)0x63, (char)0x6b, (char)0 };
            String test3 = new String(chars3);
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);

            // NOTE: The Java code uses en_us to create the CollationElementIterators
            // but I'm pretty sure that's wrong, so I've changed this to use c.
            c.Decomposition = (Collator.NoDecomposition);
            CollationElementIterator i1 = c.GetCollationElementIterator(test3);
            Logln("Offset:" + i1.GetOffset());
        }

        // @bug 4054734
        //
        // Collator::IDENTICAL documented but not implemented
        //
        [Test]
        public void Test4054734(/* char* par */)
        {

            //Here's the original Java:

            String[] decomp = {
                "\u0001",   "<",    "\u0002",
                "\u0001",   "=",    "\u0001",
                "A\u0001",  ">",    "~\u0002",      // Ensure A and ~ are not compared bitwise
                "\u00C0",   "=",    "A\u0300",      // Decomp should make these equal
            };

            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Identical);
            c.Decomposition = (Collator.CanonicalDecomposition);
            CompareArray(c, decomp);
        }

        internal void CompareArray(Collator c, String[] tests)
        {

            int expectedResult = 0;

            for (int i = 0; i < tests.Length; i += 3)
            {
                String source = tests[i];
                String comparison = tests[i + 1];
                String target = tests[i + 2];

                if (comparison.Equals("<"))
                {
                    expectedResult = -1;
                }
                else if (comparison.Equals(">"))
                {
                    expectedResult = 1;
                }
                else if (comparison.Equals("="))
                {
                    expectedResult = 0;
                }
                else
                {
                    Errln("Bogus comparison string \"" + comparison + "\"");
                }

                int compareResult = 0;

                Logln("i = " + i);
                Logln(source);
                Logln(target);
                try
                {
                    compareResult = c.Compare(source, target);
                }
                catch (Exception e)
                {
                    Errln(e.ToString());
                }

                CollationKey sourceKey = null, targetKey = null;
                try
                {
                    sourceKey = c.GetCollationKey(source);
                }
                catch (Exception e)
                {
                    Errln("Couldn't get collationKey for source");
                    continue;
                }

                try
                {
                    targetKey = c.GetCollationKey(target);
                }
                catch (Exception e)
                {
                    Errln("Couldn't get collationKey for target");
                    continue;
                }

                int keyResult = sourceKey.CompareTo(targetKey);
                ReportCResult(source, target, sourceKey, targetKey, compareResult, keyResult, compareResult, expectedResult);
            }
        }

        internal void ReportCResult(String source, String target, CollationKey sourceKey, CollationKey targetKey,
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

        // @bug 4054736
        //
        // Full Decomposition mode not implemented
        //
        [Test]
        public void Test4054736(/* char* par */)
        {
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);

            c.Strength = (Collator.Secondary);
            c.Decomposition = (Collator.NoDecomposition);

            String[] tests = { "\uFB4F", "\u003d", "\u05D0\u05DC" };  // Alef-Lamed vs. Alef, Lamed
            CompareArray(c, tests);
        }

        // @bug 4058613
        //
        // Collator::createInstance() causes an ArrayIndexOutofBoundsException for Korean
        //
        [Test]
        public void Test4058613(/* char* par */)
        {
            // Creating a default collator doesn't work when Korean is the default
            // locale
            CultureInfo oldDefault = CultureInfo.CurrentCulture;
#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
            CultureInfo.CurrentCulture = new CultureInfo("ko");
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ko");
#endif

            Collator c = null;

            c = Collator.GetInstance(new CultureInfo("en-US"));

            if (c == null)
            {
                Errln("Could not create a Korean collator");
#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
                CultureInfo.CurrentCulture = oldDefault;
#else
                System.Threading.Thread.CurrentThread.CurrentCulture = oldDefault;
#endif
                return;
            }

            // Since the fix to this bug was to turn off decomposition for Korean collators,
            // ensure that's what we got
            if (c.Decomposition != Collator.NoDecomposition)
            {
                Errln("Decomposition is not set to NO_DECOMPOSITION for Korean collator");
            }

#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
            CultureInfo.CurrentCulture = oldDefault;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = oldDefault;
#endif
        }

        // @bug 4059820
        //
        // RuleBasedCollator.getRules does not return the exact pattern as input
        // for expanding character sequences
        //
        [Test]
        public void Test4059820(/* char* par */)
        {
            RuleBasedCollator c = null;
            String rules = "&9 < a < b , c/a < d < z";
            try
            {
                c = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Errln("Failure building a collator.");
                return;
            }

            if (c.GetRules().IndexOf("c/a", StringComparison.Ordinal) == -1)
            {
                Errln("returned rules do not contain 'c/a'");
            }
        }

        // @bug 4060154
        //
        // MergeCollation::fixEntry broken for "& H < \u0131, \u0130, i, I"
        //
        [Test]
        public void Test4060154(/* char* par */)
        {
            String rules = "&f < g, G < h, H < i, I < j, J & H < \u0131, \u0130, i, I";

            RuleBasedCollator c = null;
            try
            {
                c = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                //System.out.println(e);
                Errln("failure building collator:" + e);
                return;
            }

            c.Decomposition = (Collator.NoDecomposition);

            String[] tertiary = {
            "A",        "<",    "B",
            "H",        "<",    "\u0131",
            "H",        "<",    "I",
            "\u0131",   "<",    "\u0130",
            "\u0130",   "<",    "i",
            "\u0130",   ">",    "H",
        };

            c.Strength = (Collator.Tertiary);
            CompareArray(c, tertiary);

            String[] secondary = {
            "H",        "<",    "I",
            "\u0131",   "=",    "\u0130",
        };

            c.Strength = (Collator.Primary);
            CompareArray(c, secondary);
        }

        // @bug 4062418
        //
        // Secondary/Tertiary comparison incorrect in French Secondary
        //
        [Test]
        public void Test4062418(/* char* par */)
        {
            RuleBasedCollator c = null;
            try
            {
                c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("fr-CA") /* Locale.CANADA_FRENCH */);
            }
            catch (Exception e)
            {
                Errln("Failed to create collator for Locale.CANADA_FRENCH");
                return;
            }
            c.Strength = (Collator.Secondary);

            String[] tests = {
                "p\u00eache",    "<",    "p\u00e9ch\u00e9",    // Comparing accents from end, p\u00e9ch\u00e9 is greater
        };

            CompareArray(c, tests);
        }

        // @bug 4065540
        //
        // Collator::compare() method broken if either string contains spaces
        //
        [Test]
        public void Test4065540(/* char* par */)
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            if (en_us.Compare("abcd e", "abcd f") == 0)
            {
                Errln("'abcd e' == 'abcd f'");
            }
        }

        // @bug 4066189
        //
        // Unicode characters need to be recursively decomposed to get the
        // correct result. For example,
        // u1EB1 -> \u0103 + \u0300 -> a + \u0306 + \u0300.
        //
        [Test]
        public void Test4066189(/* char* par */)
        {
            String test1 = "\u1EB1";
            String test2 = "\u0061\u0306\u0300";

            // NOTE: The java code used en_us to create the
            // CollationElementIterator's. I'm pretty sure that
            // was wrong, so I've change the code to use c1 and c2
            RuleBasedCollator c1 = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c1.Decomposition = (Collator.CanonicalDecomposition);
            CollationElementIterator i1 = c1.GetCollationElementIterator(test1);

            RuleBasedCollator c2 = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c2.Decomposition = (Collator.NoDecomposition);
            CollationElementIterator i2 = c2.GetCollationElementIterator(test2);

            assertEqual(i1, i2);
        }

        // @bug 4066696
        //
        // French secondary collation checking at the end of compare iteration fails
        //
        [Test]
        public void Test4066696(/* char* par */)
        {
            RuleBasedCollator c = null;
            try
            {
                c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("fr-CA") /* Locale.CANADA_FRENCH */);
            }
            catch (Exception e)
            {
                Errln("Failure creating collator for Locale.CANADA_FRENCH");
                return;
            }
            c.Strength = (Collator.Secondary);

            String[] tests = {
            "\u00e0",   ">",     "\u01fa",       // a-grave <  A-ring-acute
        };
            CompareArray(c, tests);
        }

        // @bug 4076676
        //
        // Bad canonicalization of same-class combining characters
        //
        [Test]
        public void Test4076676(/* char* par */)
        {
            // These combining characters are all in the same class, so they should not
            // be reordered, and they should compare as unequal.
            String s1 = "\u0041\u0301\u0302\u0300";
            String s2 = "\u0041\u0302\u0300\u0301";

            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);

            if (c.Compare(s1, s2) == 0)
            {
                Errln("Same-class combining chars were reordered");
            }
        }

        // @bug 4078588
        //
        // RuleBasedCollator breaks on "< a < bb" rule
        //
        [Test]
        public void Test4078588(/* char *par */)
        {
            RuleBasedCollator rbc = null;
            try
            {
                rbc = new RuleBasedCollator("&9 < a < bb");
            }
            catch (Exception e)
            {
                Errln("Failed to create RuleBasedCollator.");
                return;
            }

            int result = rbc.Compare("a", "bb");

            if (result >= 0)
            {
                Errln("Compare(a,bb) returned " + result + "; expected -1");
            }
        }

        // @bug 4079231
        //
        // RuleBasedCollator::operator==(NULL) throws NullPointerException
        //
        [Test]
        public void Test4079231(/* char* par */)
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            try
            {
                if (en_us.Equals(null))
                {
                    Errln("en_us.Equals(null) returned true");
                }
            }
            catch (Exception e)
            {
                Errln("en_us.Equals(null) threw " + e.ToString());
            }
        }

        // @bug 4081866
        //
        // Combining characters in different classes not reordered properly.
        //
        [Test]
        public void Test4081866(/* char* par */)
        {
            // These combining characters are all in different classes,
            // so they should be reordered and the strings should compare as equal.
            String s1 = "\u0041\u0300\u0316\u0327\u0315";
            String s2 = "\u0041\u0327\u0316\u0315\u0300";

            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);

            // Now that the default collators are set to NO_DECOMPOSITION
            // (as a result of fixing bug 4114077), we must set it explicitly
            // when we're testing reordering behavior.  -- lwerner, 5/5/98
            c.Decomposition = (Collator.CanonicalDecomposition);
            if (c.Compare(s1, s2) != 0)
            {
                Errln("Combining chars were not reordered");
            }
        }

        // @bug 4087241
        //
        // string comparison errors in Scandinavian collators
        //
        [Test]
        public void Test4087241(/* char* par */)
        {
            CultureInfo da_DK = new CultureInfo("da-DK");
            RuleBasedCollator c = null;
            try
            {
                c = (RuleBasedCollator)Collator.GetInstance(da_DK);
            }
            catch (Exception e)
            {
                Errln("Failed to create collator for da_DK locale");
                return;
            }
            c.Strength = (Collator.Secondary);
            String[] tests = {
            "\u007a",       "\u003c", "\u00E6",            // z        < ae
            "\u0061\u0308", "\u003c", "\u0061\u030A",      // a-umlaut < a-ring
            "\u0059",       "\u003c", "\u0075\u0308",      // Y        < u-umlaut
        };
            CompareArray(c, tests);
        }

        // @bug 4087243
        //
        // CollationKey takes ignorable strings into account when it shouldn't
        //
        [Test]
        public void Test4087243(/* char* par */)
        {
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);
            String[] tests = {
            "\u0031\u0032\u0033", "\u003d", "\u0031\u0032\u0033\u0001"    // 1 2 3  =  1 2 3 ctrl-A
        };
            CompareArray(c, tests);
        }

        // @bug 4092260
        //
        // Mu/micro conflict
        // Micro symbol and greek lowercase letter Mu should sort identically
        //
        [Test]
        public void Test4092260(/* char* par */)
        {
            CultureInfo el = new CultureInfo("el");
            Collator c = null;
            try
            {
                c = Collator.GetInstance(el);
            }
            catch (Exception e)
            {
                Errln("Failed to create collator for el locale.");
                return;
            }
            // These now have tertiary differences in UCA
            c.Strength = (Collator.Secondary);
            String[] tests = {
            "\u00B5", "\u003d", "\u03BC",
        };
            CompareArray(c, tests);
        }

        // @bug 4095316
        //
        [Test]
        public void Test4095316(/* char* par */)
        {
            CultureInfo el_GR = new CultureInfo("el-GR");
            Collator c = null;
            try
            {
                c = Collator.GetInstance(el_GR);
            }
            catch (Exception e)
            {
                Errln("Failed to create collator for el_GR locale");
                return;
            }
            // These now have tertiary differences in UCA
            //c->setStrength(Collator::TERTIARY);
            //c->setAttribute(UCOL_STRENGTH, UCOL_SECONDARY, status);
            c.Strength = (Collator.Secondary);
            String[] tests = {
            "\u03D4", "\u003d", "\u03AB",
        };
            CompareArray(c, tests);
        }

        // @bug 4101940
        //
        [Test]
        public void Test4101940(/* char* par */)
        {
            RuleBasedCollator c = null;
            String rules = "&9 < a < b";
            String nothing = "";
            try
            {
                c = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Errln("Failed to create RuleBasedCollator");
                return;
            }
            CollationElementIterator i = c.GetCollationElementIterator(nothing);
            i.Reset();
            if (i.Next() != CollationElementIterator.NullOrder)
            {
                Errln("next did not return NULLORDER");
            }
        }

        // @bug 4103436
        //
        // Collator::compare not handling spaces properly
        //
        [Test]
        public void Test4103436(/* char* par */)
        {
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);
            String[] tests = {
            "\u0066\u0069\u006c\u0065", "\u003c", "\u0066\u0069\u006c\u0065\u0020\u0061\u0063\u0063\u0065\u0073\u0073",
            "\u0066\u0069\u006c\u0065", "\u003c", "\u0066\u0069\u006c\u0065\u0061\u0063\u0063\u0065\u0073\u0073",
        };
            CompareArray(c, tests);
        }

        // @bug 4114076
        //
        // Collation not Unicode conformant with Hangul syllables
        //
        [Test]
        public void Test4114076(/* char* par */)
        {
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);

            //
            // With Canonical decomposition, Hangul syllables should get decomposed
            // into Jamo, but Jamo characters should not be decomposed into
            // conjoining Jamo
            //
            String[] test1 = {
            "\ud4db", "\u003d", "\u1111\u1171\u11b6"
        };

            c.Decomposition = (Collator.CanonicalDecomposition);
            CompareArray(c, test1);

            // From UTR #15:
            // *In earlier versions of Unicode, jamo characters like ksf
            //  had compatibility mappings to kf + sf. These mappings were
            //  removed in Unicode 2.1.9 to ensure that Hangul syllables are maintained.)
            // That is, the following test is obsolete as of 2.1.9

            //obsolete-    // With Full decomposition, it should go all the way down to
            //obsolete-    // conjoining Jamo characters.
            //obsolete-    //
            //obsolete-    static const UChar test2[][CollationRegressionTest::MAX_TOKEN_LEN] =
            //obsolete-    {
            //obsolete-        {0xd4db, 0}, {0x3d, 0}, {0x1111, 0x116e, 0x1175, 0x11af, 0x11c2, 0}
            //obsolete-    };
            //obsolete-
            //obsolete-    c->setDecomposition(Normalizer::DECOMP_COMPAT);
            //obsolete-    compareArray(*c, test2, ARRAY_LENGTH(test2));
        }

        // @bug 4114077
        //
        // Collation with decomposition off doesn't work for Europe
        //
        [Test]
        public void Test4114077(/* char* par */)
        {
            // Ensure that we get the same results with decomposition off
            // as we do with it on....
            RuleBasedCollator c = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            c.Strength = (Collator.Tertiary);
            String[] test1 = {
            "\u00C0",                         "\u003d", "\u0041\u0300",            // Should be equivalent
            "\u0070\u00ea\u0063\u0068\u0065", "\u003e", "\u0070\u00e9\u0063\u0068\u00e9",
            "\u0204",                         "\u003d", "\u0045\u030F",
            "\u01fa",                         "\u003d", "\u0041\u030a\u0301",    // a-ring-acute -> a-ring, acute
                                                    //   -> a, ring, acute
            "\u0041\u0300\u0316",             "\u003c", "\u0041\u0316\u0300"        // No reordering --> unequal
        };

            c.Decomposition = (Collator.NoDecomposition);
            CompareArray(c, test1);

            String[] test2 = {
            "\u0041\u0300\u0316", "\u003d", "\u0041\u0316\u0300"      // Reordering --> equal
        };

            c.Decomposition = (Collator.CanonicalDecomposition);
            CompareArray(c, test2);
        }

        // @bug 4124632
        //
        // Collator::getCollationKey was hanging on certain character sequences
        //
        [Test]
        public void Test4124632(/* char* par */)
        {
            Collator coll = null;
            try
            {
                coll = Collator.GetInstance(new CultureInfo("ja-JP") /* Locale.JAPAN */);
            }
            catch (Exception e)
            {
                Errln("Failed to create collator for Locale::JAPAN");
                return;
            }
            String test = "\u0041\u0308\u0062\u0063";
            CollationKey key;
            try
            {
                key = coll.GetCollationKey(test);
                Logln(key.SourceString);
            }
            catch (Exception e)
            {
                Errln("CollationKey creation failed.");
            }
        }

        // @bug 4132736
        //
        // sort order of french words with multiple accents has errors
        //
        [Test]
        public void Test4132736(/* char* par */)
        {
            Collator c = null;
            try
            {
                c = Collator.GetInstance(new CultureInfo("fr-CA") /* Locale.CANADA_FRENCH */);
                c.Strength = (Collator.Tertiary);
            }
            catch (Exception e)
            {
                Errln("Failed to create a collator for Locale.CANADA_FRENCH");
            }

            String[] test1 = {
                    "\u0065\u0300\u0065\u0301", "\u003c", "\u0065\u0301\u0065\u0300",
                    "\u0065\u0300\u0301",       "\u003c", "\u0065\u0301\u0300",
                };
            CompareArray(c, test1);
        }

        // @bug 4133509
        //
        // The sorting using java.text.CollationKey is not in the exact order
        //
        [Test]
        public void Test4133509(/* char* par */)
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            String[] test1 = {
                    "\u0045\u0078\u0063\u0065\u0070\u0074\u0069\u006f\u006e", "\u003c", "\u0045\u0078\u0063\u0065\u0070\u0074\u0069\u006f\u006e\u0049\u006e\u0049\u006e\u0069\u0074\u0069\u0061\u006c\u0069\u007a\u0065\u0072\u0045\u0072\u0072\u006f\u0072",
                    "\u0047\u0072\u0061\u0070\u0068\u0069\u0063\u0073",       "\u003c", "\u0047\u0072\u0061\u0070\u0068\u0069\u0063\u0073\u0045\u006e\u0076\u0069\u0072\u006f\u006e\u006d\u0065\u006e\u0074",
                    "\u0053\u0074\u0072\u0069\u006e\u0067",                   "\u003c", "\u0053\u0074\u0072\u0069\u006e\u0067\u0042\u0075\u0066\u0066\u0065\u0072",
                };

            CompareArray(en_us, test1);
        }

        // @bug 4139572
        //
        // getCollationKey throws exception for spanish text
        // Cannot reproduce this bug on 1.2, however it DOES fail on 1.1.6
        //
        [Test]
        public void Test4139572(/* char* par */)
        {
            //
            // Code pasted straight from the bug report
            // (and then translated to C++ ;-)
            //
            // create spanish locale and collator
            CultureInfo l = new CultureInfo("es-es");
            Collator col = null;
            try
            {
                col = Collator.GetInstance(l);
            }
            catch (Exception e)
            {
                Errln("Failed to create a collator for es_es locale.");
                return;
            }
            CollationKey key = null;
            // this spanish phrase kills it!
            try
            {
                key = col.GetCollationKey("Nombre De Objeto");
                Logln("source:" + key.SourceString);
            }
            catch (Exception e)
            {
                Errln("Error creating CollationKey for \"Nombre De Ojbeto\"");
            }
        }

        // @bug 4141640
        //
        // Support for Swedish gone in 1.1.6 (Can't create Swedish collator)
        //
        [Test]
        public void Test4141640(/* char* par */)
        {
            //
            // Rather than just creating a Swedish collator, we might as well
            // try to instantiate one for every locale available on the system
            // in order to prevent this sort of bug from cropping up in the future
            //
            CultureInfo[] locales = Collator.GetCultures(UCultureTypes.AllCultures);

            for (int i = 0; i < locales.Length; i += 1)
            {
                Collator c = null;
                try
                {
                    c = Collator.GetInstance(locales[i]);
                    Logln("source: " + c.Strength);
                }
                catch (Exception e)
                {
                    String msg = "";
                    msg += "Could not create collator for locale ";
                    msg += locales[i].DisplayName;
                    Errln(msg);
                }
            }
        }

        private void CheckListOrder(String[] sortedList, Collator c)
        {
            // this function uses the specified Collator to make sure the
            // passed-in list is already sorted into ascending order
            for (int i = 0; i < sortedList.Length - 1; i++)
            {
                if (c.Compare(sortedList[i], sortedList[i + 1]) >= 0)
                {
                    Errln("List out of order at element #" + i + ": "
                            + sortedList[i] + " >= "
                            + sortedList[i + 1]);
                }
            }
        }

        [Test]
        public void Test4171974()
        {
            // test French accent ordering more thoroughly
            /*String[] frenchList = {
                "\u0075\u0075",     // u u
                "\u00fc\u0075",     // u-umlaut u
                "\u01d6\u0075",     // u-umlaut-macron u
                "\u016b\u0075",     // u-macron u
                "\u1e7b\u0075",     // u-macron-umlaut u
                "\u0075\u00fc",     // u u-umlaut
                "\u00fc\u00fc",     // u-umlaut u-umlaut
                "\u01d6\u00fc",     // u-umlaut-macron u-umlaut
                "\u016b\u00fc",     // u-macron u-umlaut
                "\u1e7b\u00fc",     // u-macron-umlaut u-umlaut
                "\u0075\u01d6",     // u u-umlaut-macron
                "\u00fc\u01d6",     // u-umlaut u-umlaut-macron
                "\u01d6\u01d6",     // u-umlaut-macron u-umlaut-macron
                "\u016b\u01d6",     // u-macron u-umlaut-macron
                "\u1e7b\u01d6",     // u-macron-umlaut u-umlaut-macron
                "\u0075\u016b",     // u u-macron
                "\u00fc\u016b",     // u-umlaut u-macron
                "\u01d6\u016b",     // u-umlaut-macron u-macron
                "\u016b\u016b",     // u-macron u-macron
                "\u1e7b\u016b",     // u-macron-umlaut u-macron
                "\u0075\u1e7b",     // u u-macron-umlaut
                "\u00fc\u1e7b",     // u-umlaut u-macron-umlaut
                "\u01d6\u1e7b",     // u-umlaut-macron u-macron-umlaut
                "\u016b\u1e7b",     // u-macron u-macron-umlaut
                "\u1e7b\u1e7b"      // u-macron-umlaut u-macron-umlaut
            };
            Collator french = Collator.GetInstance(Locale.FRENCH);

            Logln("Testing French order...");
            checkListOrder(frenchList, french);

            Logln("Testing French order without decomposition...");
            french.setDecomposition(Collator.NO_DECOMPOSITION);
            checkListOrder(frenchList, french);*/

            String[] englishList = {
                    "\u0075\u0075",     // u u
                    "\u0075\u00fc",     // u u-umlaut
                    "\u0075\u01d6",     // u u-umlaut-macron
                    "\u0075\u016b",     // u u-macron
                    "\u0075\u1e7b",     // u u-macron-umlaut
                    "\u00fc\u0075",     // u-umlaut u
                    "\u00fc\u00fc",     // u-umlaut u-umlaut
                    "\u00fc\u01d6",     // u-umlaut u-umlaut-macron
                    "\u00fc\u016b",     // u-umlaut u-macron
                    "\u00fc\u1e7b",     // u-umlaut u-macron-umlaut
                    "\u01d6\u0075",     // u-umlaut-macron u
                    "\u01d6\u00fc",     // u-umlaut-macron u-umlaut
                    "\u01d6\u01d6",     // u-umlaut-macron u-umlaut-macron
                    "\u01d6\u016b",     // u-umlaut-macron u-macron
                    "\u01d6\u1e7b",     // u-umlaut-macron u-macron-umlaut
                    "\u016b\u0075",     // u-macron u
                    "\u016b\u00fc",     // u-macron u-umlaut
                    "\u016b\u01d6",     // u-macron u-umlaut-macron
                    "\u016b\u016b",     // u-macron u-macron
                    "\u016b\u1e7b",     // u-macron u-macron-umlaut
                    "\u1e7b\u0075",     // u-macron-umlaut u
                    "\u1e7b\u00fc",     // u-macron-umlaut u-umlaut
                    "\u1e7b\u01d6",     // u-macron-umlaut u-umlaut-macron
                    "\u1e7b\u016b",     // u-macron-umlaut u-macron
                    "\u1e7b\u1e7b"      // u-macron-umlaut u-macron-umlaut
                };
            Collator english = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);

            Logln("Testing English order...");
            CheckListOrder(englishList, english);

            Logln("Testing English order without decomposition...");
            english.Decomposition = (Collator.NoDecomposition);
            CheckListOrder(englishList, english);
        }

        [Test]
        public void Test4179216()
        {
            // you can position a CollationElementIterator in the middle of
            // a contracting character sequence, yielding a bogus collation
            // element
            RuleBasedCollator coll = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            coll = new RuleBasedCollator(coll.GetRules()
                    + " & C < ch , cH , Ch , CH < cat < crunchy");
            String testText = "church church catcatcher runcrunchynchy";
            CollationElementIterator iter = coll.GetCollationElementIterator(
                    testText);

            // test that the "ch" combination works properly
            iter.SetOffset(4);
            int elt4 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.Reset();
            int elt0 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(5);
            int elt5 = CollationElementIterator.PrimaryOrder(iter.Next());

            // Compares and prints only 16-bit primary weights.
            if (elt4 != elt0 || elt5 != elt0)
            {
                Errln(String.Format("The collation elements at positions 0 (0x{0:x4}), " +
                        "4 (0x{1:x4}), and 5 (0x{2:x4}) don't match.",
                        elt0, elt4, elt5));
            }

            // test that the "cat" combination works properly
            iter.SetOffset(14);
            int elt14 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(15);
            int elt15 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(16);
            int elt16 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(17);
            int elt17 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(18);
            int elt18 = CollationElementIterator.PrimaryOrder(iter.Next());

            iter.SetOffset(19);
            int elt19 = CollationElementIterator.PrimaryOrder(iter.Next());

            // Compares and prints only 16-bit primary weights.
            if (elt14 != elt15 || elt14 != elt16 || elt14 != elt17
                    || elt14 != elt18 || elt14 != elt19)
            {
                Errln(String.Format("\"cat\" elements don't match: elt14 = 0x{0:x4}, " +
                        "elt15 = 0x{1:x4}, elt16 = 0x{2:x4}, elt17 = 0x{3:x4}, " +
                        "elt18 = 0x{4:x4}, elt19 = 0x{5:x4}",
                        elt14, elt15, elt16, elt17, elt18, elt19));
            }

            // now generate a complete list of the collation elements,
            // first using next() and then using setOffset(), and
            // make sure both interfaces return the same set of elements
            iter.Reset();

            int elt = iter.Next();
            int count = 0;
            while (elt != CollationElementIterator.NullOrder)
            {
                ++count;
                elt = iter.Next();
            }

            String[] nextElements = new String[count];
            String[] setOffsetElements = new String[count];
            int lastPos = 0;

            iter.Reset();
            elt = iter.Next();
            count = 0;
            while (elt != CollationElementIterator.NullOrder)
            {
                nextElements[count++] = testText.Substring(lastPos, iter.GetOffset() - lastPos); // ICU4N: Corrected 2nd parameter
                lastPos = iter.GetOffset();
                elt = iter.Next();
            }
            count = 0;
            for (int i = 0; i < testText.Length;)
            {
                iter.SetOffset(i);
                lastPos = iter.GetOffset();
                elt = iter.Next();
                setOffsetElements[count++] = testText.Substring(lastPos, iter.GetOffset() - lastPos); // ICU4N: Corrected 2nd parameter
                i = iter.GetOffset();
            }
            for (int i = 0; i < nextElements.Length; i++)
            {
                if (nextElements[i].Equals(setOffsetElements[i]))
                {
                    Logln(nextElements[i]);
                }
                else
                {
                    Errln("Error: next() yielded " + nextElements[i] + ", but setOffset() yielded "
                        + setOffsetElements[i]);
                }
            }
        }

        [Test]
        public void Test4216006()
        {
            // rule parser barfs on "<\u00e0=a\u0300", and on other cases
            // where the same token (after normalization) appears twice in a row
            bool caughtException = false;
            try
            {
                new RuleBasedCollator("\u00e0<a\u0300");
            }
            catch (FormatException e)
            {
                caughtException = true;
            }
            if (!caughtException)
            {
                throw new Exception("\"a<a\" collation sequence didn't cause parse error!");
            }

            RuleBasedCollator collator = new RuleBasedCollator("&a<\u00e0=a\u0300");
            //commented by Kevin 2003/10/21
            //for "FULL_DECOMPOSITION is not supported here." in ICU4J DOC
            //collator.setDecomposition(Collator.FULL_DECOMPOSITION);
            collator.Strength = (Collator.Identical);

            String[] tests = {
            "a\u0300", "=", "\u00e0",
            "\u00e0",  "=", "a\u0300"
        };

            CompareArray(collator, tests);
        }

        // CollationElementIterator.previous broken for expanding char sequences
        //
        [Test]
        public void Test4179686()
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            // Create a collator with a few expanding character sequences in it....
            RuleBasedCollator coll = new RuleBasedCollator(en_us.GetRules()
                                                            + " & ae ; \u00e4 & AE ; \u00c4"
                                                            + " & oe ; \u00f6 & OE ; \u00d6"
                                                            + " & ue ; \u00fc & UE ; \u00dc");

            String text = "T\u00f6ne"; // o-umlaut

            CollationElementIterator iter = coll.GetCollationElementIterator(text);
            List<object> elements = new List<object>();
            int elem;

            // Iterate forward and collect all of the elements into a Vector
            while ((elem = iter.Next()) != CollationElementIterator.NullOrder)
            {
                elements.Add(elem);
            }

            // Now iterate backward and make sure they're the same
            iter.Reset();
            int index = elements.Count - 1;
            while ((elem = iter.Previous()) != CollationElementIterator.NullOrder)
            {
                int expect = (int)elements[index];

                if (elem != expect)
                {
                    Errln("Mismatch at index " + index
                          + ": got " + Convert.ToString(elem, 16)
                          + ", expected " + Convert.ToString(expect, 16));
                }
                index--;
            }
        }

        [Test]
        public void Test4244884()
        {
            RuleBasedCollator coll = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            coll = new RuleBasedCollator(coll.GetRules()
                        + " & C < ch , cH , Ch , CH < cat < crunchy");

            String[] testStrings = new String[] {
            "car",
            "cave",
            "clamp",
            "cramp",
            "czar",
            "church",
            "catalogue",
            "crunchy",
            "dog"
        };

            for (int i = 1; i < testStrings.Length; i++)
            {
                if (coll.Compare(testStrings[i - 1], testStrings[i]) >= 0)
                {
                    Errln("error: \"" + testStrings[i - 1]
                        + "\" is greater than or equal to \"" + testStrings[i]
                        + "\".");
                }
            }
        }

        //  CollationElementIterator set doesn't work propertly with next/prev
        [Test]
        public void Test4663220()
        {
            RuleBasedCollator collator = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);
            StringCharacterIterator stringIter = new StringCharacterIterator("fox");
            CollationElementIterator iter = collator.GetCollationElementIterator(stringIter);

            int[] elements_next = new int[3];
            Logln("calling next:");
            for (int i = 0; i < 3; ++i)
            {
                Logln("[" + i + "] " + (elements_next[i] = iter.Next()));
            }

            int[] elements_fwd = new int[3];
            Logln("calling set/next:");
            for (int i = 0; i < 3; ++i)
            {
                iter.SetOffset(i);
                Logln("[" + i + "] " + (elements_fwd[i] = iter.Next()));
            }

            for (int i = 0; i < 3; ++i)
            {
                if (elements_next[i] != elements_fwd[i])
                {
                    Errln("mismatch at position " + i +
                    ": " + elements_next[i] +
                    " != " + elements_fwd[i]);
                }
            }
        }

        // Fixing the infinite loop for surrogates
        [Test]
        public void Test8484()
        {
            String s = "\u9FE1\uCEF3\u2798\uAAB6\uDA7C";
            Collator coll = Collator.GetInstance();
            CollationKey collKey = coll.GetCollationKey(s);
            Logln("Pass: " + collKey.ToString() + " generated OK.");
        }

        [Test]
        public void TestBengaliSortKey()
        {
            char[] rules = {
        (char)0x26, (char)0x9fa, (char)0x3c, (char)0x98c, (char)0x3c, (char)0x9e1, (char)0x3c, (char)0x98f, (char)0x3c, (char)0x990, (char)0x3c, (char)0x993,
                (char)0x3c, (char)0x994, (char)0x3c, (char)0x9bc, (char)0x3c, (char)0x982, (char)0x3c, (char)0x983, (char)0x3c, (char)0x981, (char)0x3c, (char)0x9b0, (char)0x3c,
                (char)0x9b8, (char)0x3c, (char)0x9b9, (char)0x3c, (char)0x9bd, (char)0x3c, (char)0x9be, (char)0x3c, (char)0x9bf, (char)0x3c, (char)0x9c8, (char)0x3c, (char)0x9cb,
                (char)0x3d, (char)0x9cb };

            Collator col = new RuleBasedCollator(new String(rules));

            String str1 = "\u09be";
            String str2 = "\u0b70";

            int result = col.Compare(str1, str2);
            Console.Out.Flush();

            if (result >= 0)
            {
                Errln("\nERROR: result is " + result + " , wanted negative.");
                Errln(PrintKey(col, str1).ToString());
                Errln(PrintKey(col, str2).ToString());
            }
            else
            {
                Logln("Pass: result is OK.");
            }
        }

        private static StringBuilder PrintKey(Collator col, String str1)
        {
            StringBuilder sb = new StringBuilder();
            CollationKey sortk1 = col.GetCollationKey(str1);
            byte[] bytes = sortk1.ToByteArray();
            for (int i = 0; i < str1.Length; i++)
            {
                sb.Append("\\u" + (str1[i]).ToHexString());
            }
            Console.Out.Write(": ");
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(" 0x" + ((bytes[i]) & 0xff).ToHexString());
            }
            sb.Append("\n");
            return sb;
        }

        /*
         * Test case for ticket#8624
         * Bad collation key with upper first option.
         */
        [Test]
        public void TestCaseFirstCompression()
        {
            RuleBasedCollator col = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US") /* Locale.US */);

            // Default
            CaseFirstCompressionSub(col, "default");

            // Upper first
            col.IsUpperCaseFirst = (true);
            CaseFirstCompressionSub(col, "upper first");

            // Lower first
            col.IsLowerCaseFirst = (true);
            CaseFirstCompressionSub(col, "lower first");
        }

        [Test]
        public void TestTrailingComment()
        {
            // ICU ticket #8070:
            // Check that the rule parser handles a comment without terminating end-of-line.
            RuleBasedCollator coll = new RuleBasedCollator("&c<b#comment1\n<a#comment2");
            assertTrue("c<b", coll.Compare("c", "b") < 0);
            assertTrue("b<a", coll.Compare("b", "a") < 0);
        }

        [Test]
        public void TestBeforeWithTooStrongAfter()
        {
            // ICU ticket #9959:
            // Forbid rules with a before-reset followed by a stronger relation.
            try
            {
                new RuleBasedCollator("&[before 2]x<<q<p");
                Errln("should forbid before-2-reset followed by primary relation");
            }
            catch (Exception expected)
            {
            }
            try
            {
                new RuleBasedCollator("&[before 3]x<<<q<<s<p");
                Errln("should forbid before-3-reset followed by primary or secondary relation");
            }
            catch (Exception expected)
            {
            }
        }

        /*
         * Compare two strings - "aaa...A" and "aaa...a" with
         * Collation#compare and CollationKey#compareTo, called from
         * TestCaseFirstCompression.
         */
        private void CaseFirstCompressionSub(RuleBasedCollator col, String opt)
        {
            int maxLength = 50;

            StringBuilder buf1 = new StringBuilder();
            StringBuilder buf2 = new StringBuilder();
            String str1, str2;

            for (int n = 1; n <= maxLength; n++)
            {
                buf1.Length = (0);
                buf2.Length = (0);

                for (int i = 0; i < n - 1; i++)
                {
                    buf1.Append('a');
                    buf2.Append('a');
                }
                buf1.Append('A');
                buf2.Append('a');

                str1 = buf1.ToString();
                str2 = buf2.ToString();

                CollationKey key1 = col.GetCollationKey(str1);
                CollationKey key2 = col.GetCollationKey(str2);

                int cmpKey = key1.CompareTo(key2);
                int cmpCol = col.Compare(str1, str2);

                if ((cmpKey < 0 && cmpCol >= 0) || (cmpKey > 0 && cmpCol <= 0) || (cmpKey == 0 && cmpCol != 0))
                {
                    Errln("Inconsistent comparison(" + opt + "): str1=" + str1 + ", str2=" + str2 + ", cmpKey=" + cmpKey + " , cmpCol=" + cmpCol);
                }
            }
        }

        /* RuleBasedCollator not subclassable
         * @bug 4146160
        //
        // RuleBasedCollator doesn't use createCollationElementIterator internally
        //
        [Test]
        public void Test4146160() {
            //
            // Use a custom collator class whose createCollationElementIterator
            // methods increment a count....
            //
            RuleBasedCollator en_us = (RuleBasedCollator) Collator.GetInstance(new CultureInfo("en-US") /* Locale.US *//*);
            My4146160Collator.count = 0;
            My4146160Collator mc = null;
            try {
                mc = new My4146160Collator(en_us);
            } catch (Exception e) {
                Errln("Failed to create a My4146160Collator.");
                return;
            }

            CollationKey key = null;
            try {
                key = mc.getCollationKey("1");
            } catch (Exception e) {
                Errln("Failure to get a CollationKey from a My4146160Collator.");
                return;
            }

            if (My4146160Collator.count < 1) {
                Errln("My4146160Collator.getCollationElementIterator not called for getCollationKey");
            }

            My4146160Collator.count = 0;
            mc.Compare("1", "2");

            if (My4146160Collator.count < 1) {
                Errln("My4146160Collator.getCollationElementIterator not called for compare");
            }
        }*/
    }

    /* RuleBasedCollator not subclassable
     * class My4146160Collator extends RuleBasedCollator {
        static int count = 0;

        public My4146160Collator(RuleBasedCollator rbc) throws Exception {
            super(rbc.GetRules());
        }

        public CollationElementIterator getCollationElementIterator(String text) {
            count += 1;
            return super.GetCollationElementIterator(text);
        }

        public CollationElementIterator getCollationElementIterator(java.text.CharacterIterator text) {
            count += 1;
            return super.GetCollationElementIterator(text);
        }
    }
    */
}

