using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

/// <summary>
/// Port From:   ICU4C v2.1 : Collate/CollationDummyTest
/// Source File: $ICU4CRoot/source/test/intltest/allcoll.cpp
///              $ICU4CRoot/source/test/cintltst/callcoll.c
/// </summary>
namespace ICU4N.Dev.Test.Collate
{
    public class CollationDummyTest : TestFmwk
    {
        //testSourceCases[][] and testTargetCases[][], testCases[][] are ported from the file callcoll.c in icu4c
        private static char[][] testSourceCases = {
            new char[] {(char)0x61, (char)0x62, (char)0x27, (char)0x63},
            new char[] {(char)0x63, (char)0x6f, (char)0x2d, (char)0x6f, (char)0x70},
            new char[] {(char)0x61, (char)0x62},
            new char[] {(char)0x61, (char)0x6d, (char)0x70, (char)0x65, (char)0x72, (char)0x73, (char)0x61, (char)0x64},
            new char[] {(char)0x61, (char)0x6c, (char)0x6c},
            new char[] {(char)0x66, (char)0x6f, (char)0x75, (char)0x72},
            new char[] {(char)0x66, (char)0x69, (char)0x76, (char)0x65},
            new char[] {(char)0x31},
            new char[] {(char)0x31},
            new char[] {(char)0x31},                                            //  10
            new char[] {(char)0x32},
            new char[] {(char)0x32},
            new char[] {(char)0x48, (char)0x65, (char)0x6c, (char)0x6c, (char)0x6f},
            new char[] {(char)0x61, (char)0x3c, (char)0x62},
            new char[] {(char)0x61, (char)0x3c, (char)0x62},
            new char[] {(char)0x61, (char)0x63, (char)0x63},
            new char[] {(char)0x61, (char)0x63, (char)0x48, (char)0x63},  //  simple test
            new char[] {(char)0x70, (char)0x00EA, (char)0x63, (char)0x68, (char)0x65},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x62, (char)0x63},                                  //  20
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x00E6, (char)0x63},
            new char[] {(char)0x61, (char)0x63, (char)0x48, (char)0x63},  //  primary test
            new char[] {(char)0x62, (char)0x6c, (char)0x61, (char)0x63, (char)0x6b},
            new char[] {(char)0x66, (char)0x6f, (char)0x75, (char)0x72},
            new char[] {(char)0x66, (char)0x69, (char)0x76, (char)0x65},
            new char[] {(char)0x31},
            new char[] {(char)0x61, (char)0x62, (char)0x63},                                        //  30
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x62, (char)0x63, (char)0x48},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x63, (char)0x48, (char)0x63},                              //  34
            new char[] {(char)0x61, (char)0x63, (char)0x65, (char)0x30},
            new char[] {(char)0x31, (char)0x30},
            new char[] {(char)0x70, (char)0x00EA,(char)0x30}                                    // 37
        };

        private static char[][] testTargetCases = {
            new char[] {(char)0x61, (char)0x62, (char)0x63, (char)0x27},
            new char[] {(char)0x43, (char)0x4f, (char)0x4f, (char)0x50},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x26},
            new char[] {(char)0x26},
            new char[] {(char)0x34},
            new char[] {(char)0x35},
            new char[] {(char)0x6f, (char)0x6e, (char)0x65},
            new char[] {(char)0x6e, (char)0x6e, (char)0x65},
            new char[] {(char)0x70, (char)0x6e, (char)0x65},                                  //  10
            new char[] {(char)0x74, (char)0x77, (char)0x6f},
            new char[] {(char)0x75, (char)0x77, (char)0x6f},
            new char[] {(char)0x68, (char)0x65, (char)0x6c, (char)0x6c, (char)0x4f},
            new char[] {(char)0x61, (char)0x3c, (char)0x3d, (char)0x62},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},  //  simple test
            new char[] {(char)0x70, (char)0x00E9, (char)0x63, (char)0x68, (char)0x00E9},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x42, (char)0x43},                                  //  20
            new char[] {(char)0x61, (char)0x62, (char)0x63, (char)0x68},
            new char[] {(char)0x61, (char)0x62, (char)0x64},
            new char[] {(char)0x00E4, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x00C6, (char)0x63},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},  //  primary test
            new char[] {(char)0x62, (char)0x6c, (char)0x61, (char)0x63, (char)0x6b, (char)0x2d, (char)0x62, (char)0x69, (char)0x72, (char)0x64},
            new char[] {(char)0x34},
            new char[] {(char)0x35},
            new char[] {(char)0x6f, (char)0x6e, (char)0x65},
            new char[] {(char)0x61, (char)0x62, (char)0x63},
            new char[] {(char)0x61, (char)0x42, (char)0x63},                                  //  30
            new char[] {(char)0x61, (char)0x62, (char)0x63, (char)0x68},
            new char[] {(char)0x61, (char)0x62, (char)0x64},
            new char[] {(char)0x61, (char)0x43, (char)0x48, (char)0x63},                                //  34
            new char[] {(char)0x61, (char)0x63, (char)0x65, (char)0x30},
            new char[] {(char)0x31, (char)0x30},
            new char[] {(char)0x70, (char)0x00EB,(char)0x30}                                    // 37
        };

        private static char[][] testCases = {
            new char[] {(char)0x61},
            new char[] {(char)0x41},
            new char[] {(char)0x00e4},
            new char[] {(char)0x00c4},
            new char[] {(char)0x61, (char)0x65},
            new char[] {(char)0x61, (char)0x45},
            new char[] {(char)0x41, (char)0x65},
            new char[] {(char)0x41, (char)0x45},
            new char[] {(char)0x00e6},
            new char[] {(char)0x00c6},
            new char[] {(char)0x62},
            new char[] {(char)0x63},
            new char[] {(char)0x7a}
        };

        int[] results = {
            -1,
            -1, //Collator::GREATER,
            -1,
            -1,
            -1,
            -1,
            -1,
            1,
            1,
            -1,                                     //  10
            1,
            -1,
            1,
            1,
            -1,
            -1,
            -1,
        //  test primary > 17
            0,
            0,
            0,                                    //  20
            -1,
            -1,
            0,
            0,
            0,
            -1,
        //  test secondary > 26
            0,
            0,
            0,
            0,
            0,                                    //  30
            0,
            -1,
            0,                                     //  34
            0,
            0,
            -1
        };

        internal readonly int MAX_TOKEN_LEN = 16;

        private RuleBasedCollator myCollation;

        public CollationDummyTest()
        {
        }

        [SetUp]
        public void Init()
        {
            String ruleset = "& C < ch, cH, Ch, CH & Five, 5 & Four, 4 & one, 1 & Ampersand; '&' & Two, 2 ";
            // String ruleset = "& Four, 4";
            myCollation = new RuleBasedCollator(ruleset);
        }

        // perform test with strength tertiary
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = (Collator.Tertiary);
            for (i = 0; i < 17; i++)
            {
                DoTest(myCollation, testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // perform test with strength PRIMARY
        [Test]
        public void TestPrimary()
        {
            // problem in strcollinc for unfinshed contractions
            myCollation.Strength = (Collator.Primary);
            for (int i = 17; i < 26; i++)
            {
                DoTest(myCollation, testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        //perform test with strength SECONDARY
        [Test]
        public void TestSecondary()
        {
            int i;
            myCollation.Strength = (Collator.Secondary);
            for (i = 26; i < 34; i++)
            {
                DoTest(myCollation, testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        // perform extra tests
        [Test]
        public void TestExtra()
        {
            int i, j;
            myCollation.Strength = (Collator.Tertiary);
            for (i = 0; i < testCases.Length - 1; i++)
            {
                for (j = i + 1; j < testCases.Length; j += 1)
                {
                    DoTest(myCollation, testCases[i], testCases[j], -1);
                }
            }
        }

        [Test]
        public void TestIdentical()
        {
            int i;
            myCollation.Strength = (Collator.Identical);
            for (i = 34; i < 37; i++)
            {
                DoTest(myCollation, testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        [Test]
        public void TestJB581()
        {
            String source = "THISISATEST.";
            String target = "Thisisatest.";
            Collator coll = null;
            try
            {
                coll = Collator.GetInstance(new CultureInfo("en") /*Locale.ENGLISH*/);
            }
            catch (Exception e)
            {
                Errln("ERROR: Failed to create the collator for : en_US\n");
                return;
            }

            int result = coll.Compare(source, target);
            // result is 1, secondary differences only for ignorable space characters
            if (result != 1)
            {
                Errln("Comparing two strings with only secondary differences in C failed.\n");
                return;
            }

            // To compare them with just primary differences
            coll.Strength = (Collator.Primary);
            result = coll.Compare(source, target);
            // result is 0
            if (result != 0)
            {
                Errln("Comparing two strings with no differences in C failed.\n");
                return;
            }

            // Now, do the same comparison with keys
            CollationKey sourceKeyOut, targetKeyOut;
            sourceKeyOut = coll.GetCollationKey(source);
            targetKeyOut = coll.GetCollationKey(target);
            result = sourceKeyOut.CompareTo(targetKeyOut);
            if (result != 0)
            {
                Errln("Comparing two strings with sort keys in C failed.\n");
                return;
            }
        }

        //TestSurrogates() is ported from cintltst/callcoll.c

        /**
        * Tests surrogate support.
        */
        [Test]
        public void TestSurrogates()
        {
            String rules = "&z<'\ud800\udc00'<'\ud800\udc0a\u0308'<A";
            String[] source = {"z",
                           "\uD800\uDC00",
                           "\ud800\udc0a\u0308",
                           "\ud800\udc02"
        };

            String[] target = {"\uD800\uDC00",
                           "\ud800\udc0a\u0308",
                           "A",
                           "\ud800\udc03"
        };

            // this test is to verify the supplementary sort key order in the english
            // collator
            Collator enCollation;
            try
            {
                enCollation = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);
            }
            catch (Exception e)
            {
                Errln("ERROR: Failed to create the collator for ENGLISH");
                return;
            }

            myCollation.Strength = (Collator.Tertiary);
            int count = 0;
            // logln("start of english collation supplementary characters test\n");
            while (count < 2)
            {
                DoTest(enCollation, source[count], target[count], -1);
                count++;
            }
            DoTest(enCollation, source[count], target[count], 1);

            // logln("start of tailored collation supplementary characters test\n");
            count = 0;
            Collator newCollation;
            try
            {
                newCollation = new RuleBasedCollator(rules);
            }
            catch (Exception e)
            {
                Errln("ERROR: Failed to create the collator for rules");
                return;
            }

            // tests getting collation elements for surrogates for tailored rules
            while (count < 4)
            {
                DoTest(newCollation, source[count], target[count], -1);
                count++;
            }

            // tests that \uD801\uDC01 still has the same value, not changed
            CollationKey enKey = enCollation.GetCollationKey(source[3]);
            CollationKey newKey = newCollation.GetCollationKey(source[3]);
            int keyResult = enKey.CompareTo(newKey);
            if (keyResult != 0)
            {
                Errln("Failed : non-tailored supplementary characters should have the same value\n");
            }
        }

        private static readonly bool SUPPORT_VARIABLE_TOP_RELATION = false;
        //TestVariableTop() is ported from cintltst/callcoll.c
        /**
        * Tests the [variable top] tag in rule syntax. Since the default [alternate]
        * tag has the value shifted, any codepoints before [variable top] should give
        * a primary ce of 0.
        */
        [Test]
        public void TestVariableTop()
        {
            /*
             * Starting with ICU 53, setting the variable top via a pseudo relation string
             * is not supported any more.
             * It was replaced by the [maxVariable symbol] setting.
             * See ICU tickets #9958 and #8032.
             */
            if (!SUPPORT_VARIABLE_TOP_RELATION) { return; }
            String rule = "&z = [variable top]";
            Collator myColl;
            Collator enColl;
            char[] source = new char[1];
            char ch;
            int[] expected = { 0 };

            try
            {
                enColl = Collator.GetInstance(new CultureInfo("en")  /* Locale.ENGLISH */);
            }
            catch (Exception e)
            {
                Errln("ERROR: Failed to create the collator for ENGLISH");
                return;
            }

            try
            {
                myColl = new RuleBasedCollator(rule);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator with rules:" + rule);
                return;
            }
            enColl.Strength = (Collator.Primary);
            myColl.Strength = (Collator.Primary);

            ((RuleBasedCollator)enColl).IsAlternateHandlingShifted = (true);
            ((RuleBasedCollator)myColl).IsAlternateHandlingShifted = (true);

            if (((RuleBasedCollator)enColl).IsAlternateHandlingShifted != true)
            {
                Errln("ERROR: ALTERNATE_HANDLING value can not be set to SHIFTED\n");
            }

            // space is supposed to be a variable
            CollationKey key = enColl.GetCollationKey(" ");
            byte[] result = key.ToByteArray();

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != expected[i])
                {
                    Errln("ERROR: SHIFTED alternate does not return 0 for primary of space\n");
                    break;
                }
            }

            ch = 'a';
            while (ch < 'z')
            {
                source[0] = ch;
                key = myColl.GetCollationKey(new String(source));
                result = key.ToByteArray();

                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] != expected[i])
                    {
                        Errln("ERROR: SHIFTED alternate does not return 0 for primary of space\n");
                        break;
                    }
                }
                ch++;
            }
        }

        [Test]
        public void TestJB1401()
        {
            Collator myCollator = null;
            char[] NFD_UnsafeStartChars = {
            (char)0x0f73,          // Tibetan Vowel Sign II
            (char)0x0f75,          // Tibetan Vowel Sign UU
            (char)0x0f81,          // Tibetan Vowel Sign Reversed II
            (char)0
        };
            int i;

            try
            {
                myCollator = Collator.GetInstance(new CultureInfo("en") /* Locale.ENGLISH */);
            }
            catch (Exception e)
            {
                Errln("ERROR: Failed to create the collator for ENGLISH");
                return;
            }
            myCollator.Decomposition = (Collator.CanonicalDecomposition);
            for (i = 0; ; i++)
            {
                // Get the next funny character to be tested, and set up the
                // three test strings X, Y, Z, consisting of an A-grave + test char,
                // in original form, NFD, and then NFC form.
                char c = NFD_UnsafeStartChars[i];
                if (c == 0) { break; }

                String x = "\u00C0" + c;       // \u00C0 is A Grave
                String y;
                String z;

                try
                {
                    y = Normalizer.Decompose(x, false);
                    z = Normalizer.Decompose(y, true);
                }
                catch (Exception e)
                {
                    Errln("ERROR: Failed to normalize test of character" + c);
                    return;
                }

                // Collation test.  All three strings should be equal.
                // doTest does both strcoll and sort keys, with params in both orders.
                DoTest(myCollator, x, y, 0);
                DoTest(myCollator, x, z, 0);
                DoTest(myCollator, y, z, 0);

                // Run collation element iterators over the three strings.  Results should be same for each.

                {
                    CollationElementIterator ceiX, ceiY, ceiZ;
                    int ceX, ceY, ceZ;
                    int j;
                    try
                    {
                        ceiX = ((RuleBasedCollator)myCollator).GetCollationElementIterator(x);
                        ceiY = ((RuleBasedCollator)myCollator).GetCollationElementIterator(y);
                        ceiZ = ((RuleBasedCollator)myCollator).GetCollationElementIterator(z);
                    }
                    catch (Exception e)
                    {
                        Errln("ERROR: getCollationElementIterator failed");
                        return;
                    }

                    for (j = 0; ; j++)
                    {
                        try
                        {
                            ceX = ceiX.Next();
                            ceY = ceiY.Next();
                            ceZ = ceiZ.Next();
                        }
                        catch (Exception e)
                        {
                            Errln("ERROR: CollationElementIterator.next failed for iteration " + j);
                            break;
                        }

                        if (ceX != ceY || ceY != ceZ)
                        {
                            Errln("ERROR: ucol_next failed for iteration " + j);
                            break;
                        }
                        if (ceX == CollationElementIterator.NULLORDER)
                        {
                            break;
                        }
                    }
                }
            }
        }

        // main test method called with different strengths,
        // tests comparison of custum collation with different strengths

        private void DoTest(Collator collation, char[] source, char[] target, int result)
        {
            String s = new String(source);
            String t = new String(target);
            DoTestVariant(collation, s, t, result);
            if (result == -1)
            {
                DoTestVariant(collation, t, s, 1);
            }
            else if (result == 1)
            {
                DoTestVariant(collation, t, s, -1);
            }
            else
            {
                DoTestVariant(collation, t, s, 0);
            }
        }

        // main test method called with different strengths,
        // tests comparison of custum collation with different strengths

        private void DoTest(Collator collation, String s, String t, int result)
        {
            DoTestVariant(collation, s, t, result);
            if (result == -1)
            {
                DoTestVariant(collation, t, s, 1);
            }
            else if (result == 1)
            {
                DoTestVariant(collation, t, s, -1);
            }
            else
            {
                DoTestVariant(collation, t, s, 0);
            }
        }

        private void DoTestVariant(Collator collation, String source, String target, int result)
        {
            int compareResult = collation.Compare(source, target);
            CollationKey srckey, tgtkey;
            srckey = collation.GetCollationKey(source);
            tgtkey = collation.GetCollationKey(target);
            int keyResult = srckey.CompareTo(tgtkey);
            if (compareResult != result)
            {
                Errln("String comparison failed in variant test\n");
            }
            if (keyResult != result)
            {
                Errln("Collation key comparison failed in variant test\n");
            }
        }
    }
}
