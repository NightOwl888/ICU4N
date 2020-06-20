using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using StringBuffer = System.Text.StringBuilder;

//
// Port From:   ICU4C v2.1 : collate/CollationIteratorTest
// Source File: $ICU4CRoot/source/test/intltest/itercoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationIteratorTest : TestFmwk
    {
        String test1 = "What subset of all possible test cases?";
        String test2 = "has the highest probability of detecting";

        /*
         * @bug 4157299
         */
        [Test]
        public void TestClearBuffers(/* char* par */)
        {
            RuleBasedCollator c = null;
            try
            {
                c = new RuleBasedCollator("&a < b < c & ab = d");
            }
            catch (Exception e)
            {
                Warnln("Couldn't create a RuleBasedCollator.");
                return;
            }

            String source = "abcd";
            CollationElementIterator i = c.GetCollationElementIterator(source);
            int e0 = 0;
            try
            {
                e0 = i.Next();    // save the first collation element
            }
            catch (Exception e)
            {
                Errln("call to i.Next() failed.");
                return;
            }

            try
            {
                i.SetOffset(3);        // go to the expanding character
            }
            catch (Exception e)
            {
                Errln("call to i.setOffset(3) failed.");
                return;
            }

            try
            {
                i.Next();                // but only use up half of it
            }
            catch (Exception e)
            {
                Errln("call to i.Next() failed.");
                return;
            }

            try
            {
                i.SetOffset(0);        // go back to the beginning
            }
            catch (Exception e)
            {
                Errln("call to i.setOffset(0) failed. ");
            }

            {
                int e = 0;
                try
                {
                    e = i.Next();    // and get this one again
                }
                catch (Exception ee)
                {
                    Errln("call to i.Next() failed. ");
                    return;
                }

                if (e != e0)
                {
                    Errln("got 0x" + (e).ToHexString() + ", expected 0x" + (e0).ToHexString());
                }
            }
        }

        /** @bug 4108762
         * Test for getMaxExpansion()
         */
        [Test]
        public void TestMaxExpansion(/* char* par */)
        {
            int unassigned = 0xEFFFD;
            String rule = "&a < ab < c/aba < d < z < ch";
            RuleBasedCollator coll = null;
            try
            {
                coll = new RuleBasedCollator(rule);
            }
            catch (Exception e)
            {
                Warnln("Fail to create RuleBasedCollator");
                return;
            }
            char ch = (char)0;
            String str = ch + "";

            CollationElementIterator iter = coll.GetCollationElementIterator(str);

            while (ch < 0xFFFF)
            {
                int count = 1;
                ch++;
                str = ch + "";
                iter.SetText(str);
                int order = iter.Previous();

                // thai management
                if (order == 0)
                {
                    order = iter.Previous();
                }

                while (iter.Previous() != CollationElementIterator.NullOrder)
                {
                    count++;
                }

                if (iter.GetMaxExpansion(order) < count)
                {
                    Errln("Failure at codepoint " + ch + ", maximum expansion count < " + count);
                }
            }

            // testing for exact max expansion
            ch = (char)0;
            while (ch < 0x61)
            {
                str = ch + "";
                iter.SetText(str);
                int order = iter.Previous();

                if (iter.GetMaxExpansion(order) != 1)
                {
                    Errln("Failure at codepoint 0x" + (ch).ToHexString()
                          + " maximum expansion count == 1");
                }
                ch++;
            }

            ch = (char)0x63;
            str = ch + "";
            iter.SetText(str);
            int temporder = iter.Previous();

            if (iter.GetMaxExpansion(temporder) != 3)
            {
                Errln("Failure at codepoint 0x" + (ch).ToHexString()
                                      + " maximum expansion count == 3");
            }

            ch = (char)0x64;
            str = ch + "";
            iter.SetText(str);
            temporder = iter.Previous();

            if (iter.GetMaxExpansion(temporder) != 1)
            {
                Errln("Failure at codepoint 0x" + (ch).ToHexString()
                                      + " maximum expansion count == 1");
            }

            str = UChar.ConvertFromUtf32(unassigned);
            iter.SetText(str);
            temporder = iter.Previous();

            if (iter.GetMaxExpansion(temporder) != 2)
            {
                Errln("Failure at codepoint 0x" + (ch).ToHexString()
                                      + " maximum expansion count == 2");
            }


            // testing jamo
            ch = (char)0x1165;
            str = ch + "";
            iter.SetText(str);
            temporder = iter.Previous();

            if (iter.GetMaxExpansion(temporder) > 3)
            {
                Errln("Failure at codepoint 0x" + (ch).ToHexString()
                                              + " maximum expansion count < 3");
            }

            // testing special jamo &a<\u1165
            rule = "\u0026\u0071\u003c\u1165\u002f\u0071\u0071\u0071\u0071";

            try
            {
                coll = new RuleBasedCollator(rule);
            }
            catch (Exception e)
            {
                Errln("Fail to create RuleBasedCollator");
                return;
            }
            iter = coll.GetCollationElementIterator(str);

            temporder = iter.Previous();

            if (iter.GetMaxExpansion(temporder) != 6)
            {
                Errln("Failure at codepoint 0x" + (ch).ToHexString()
                                             + " maximum expansion count == 6");
            }
        }

        /**
         * Test for getOffset() and setOffset()
         */
        [Test]
        public void TestOffset(/* char* par */)
        {
            RuleBasedCollator en_us;
            try
            {
                en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of collator of ENGLISH locale");
                return;
            }

            CollationElementIterator iter = en_us.GetCollationElementIterator(test1);
            // testing boundaries
            iter.SetOffset(0);
            if (iter.Previous() != CollationElementIterator.NullOrder)
            {
                Errln("Error: After setting offset to 0, we should be at the end "
                      + "of the backwards iteration");
            }
            iter.SetOffset(test1.Length);
            if (iter.Next() != CollationElementIterator.NullOrder)
            {
                Errln("Error: After setting offset to the end of the string, we "
                      + "should be at the end of the forwards iteration");
            }

            // Run all the way through the iterator, then get the offset
            int[] orders = CollationTest.GetOrders(iter);
            Logln("orders.Length = " + orders.Length);

            int offset = iter.GetOffset();

            if (offset != test1.Length)
            {
                String msg1 = "offset at end != length: ";
                String msg2 = " vs ";
                Errln(msg1 + offset + msg2 + test1.Length);
            }

            // Now set the offset back to the beginning and see if it works
            CollationElementIterator pristine = en_us.GetCollationElementIterator(test1);

            try
            {
                iter.SetOffset(0);
            }
            catch (Exception e)
            {
                Errln("setOffset failed.");
            }
            assertEqual(iter, pristine);

            // setting offset in the middle of a contraction
            String contraction = "change";
            RuleBasedCollator tailored = null;
            try
            {
                tailored = new RuleBasedCollator("& a < ch");
            }
            catch (Exception e)
            {
                Errln("Error: in creation of Spanish collator");
                return;
            }
            iter = tailored.GetCollationElementIterator(contraction);
            int[] order = CollationTest.GetOrders(iter);
            iter.SetOffset(1); // sets offset in the middle of ch
            int[] order2 = CollationTest.GetOrders(iter);
            if (!ArrayEqualityComparer<int>.OneDimensional.Equals(order, order2))
            {
                Errln("Error: setting offset in the middle of a contraction should be the same as setting it to the start of the contraction");
            }
            contraction = "peache";
            iter = tailored.GetCollationElementIterator(contraction);
            iter.SetOffset(3);
            order = CollationTest.GetOrders(iter);
            iter.SetOffset(4); // sets offset in the middle of ch
            order2 = CollationTest.GetOrders(iter);
            if (!ArrayEqualityComparer<int>.OneDimensional.Equals(order, order2))
            {
                Errln("Error: setting offset in the middle of a contraction should be the same as setting it to the start of the contraction");
            }
            // setting offset in the middle of a surrogate pair
            String surrogate = "\ud800\udc00str";
            iter = tailored.GetCollationElementIterator(surrogate);
            order = CollationTest.GetOrders(iter);
            iter.SetOffset(1); // sets offset in the middle of surrogate
            order2 = CollationTest.GetOrders(iter);
            if (!ArrayEqualityComparer<int>.OneDimensional.Equals(order, order2))
            {
                Errln("Error: setting offset in the middle of a surrogate pair should be the same as setting it to the start of the surrogate pair");
            }
            surrogate = "simple\ud800\udc00str";
            iter = tailored.GetCollationElementIterator(surrogate);
            iter.SetOffset(6);
            order = CollationTest.GetOrders(iter);
            iter.SetOffset(7); // sets offset in the middle of surrogate
            order2 = CollationTest.GetOrders(iter);
            if (!ArrayEqualityComparer<int>.OneDimensional.Equals(order, order2))
            {
                Errln("Error: setting offset in the middle of a surrogate pair should be the same as setting it to the start of the surrogate pair");
            }
            // TODO: try iterating halfway through a messy string.
        }



        void assertEqual(CollationElementIterator i1, CollationElementIterator i2)
        {
            int c1, c2, count = 0;
            do
            {
                c1 = i1.Next();
                c2 = i2.Next();
                if (c1 != c2)
                {
                    Errln("    " + count + ": strength(0x" +
                        (c1).ToHexString() + ") != strength(0x" + (c2).ToHexString() + ")");
                    break;
                }
                count += 1;
            } while (c1 != CollationElementIterator.NullOrder);
            CollationTest.BackAndForth(this, i1);
            CollationTest.BackAndForth(this, i2);
        }

        /**
         * Test for CollationElementIterator.previous()
         *
         * @bug 4108758 - Make sure it works with contracting characters
         *
         */
        [Test]
        public void TestPrevious(/* char* par */)
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
            CollationElementIterator iter = en_us.GetCollationElementIterator(test1);

            // A basic test to see if it's working at all
            CollationTest.BackAndForth(this, iter);

            // Test with a contracting character sequence
            String source;
            RuleBasedCollator c1 = null;
            try
            {
                c1 = new RuleBasedCollator("&a,A < b,B < c,C, d,D < z,Z < ch,cH,Ch,CH");
            }
            catch (Exception e)
            {
                Errln("Couldn't create a RuleBasedCollator with a contracting sequence.");
                return;
            }

            source = "abchdcba";
            iter = c1.GetCollationElementIterator(source);
            CollationTest.BackAndForth(this, iter);

            // Test with an expanding character sequence
            RuleBasedCollator c2 = null;
            try
            {
                c2 = new RuleBasedCollator("&a < b < c/abd < d");
            }
            catch (Exception e)
            {
                Errln("Couldn't create a RuleBasedCollator with an expanding sequence.");
                return;
            }

            source = "abcd";
            iter = c2.GetCollationElementIterator(source);
            CollationTest.BackAndForth(this, iter);

            // Now try both
            RuleBasedCollator c3 = null;
            try
            {
                c3 = new RuleBasedCollator("&a < b < c/aba < d < z < ch");
            }
            catch (Exception e)
            {
                Errln("Couldn't create a RuleBasedCollator with both an expanding and a contracting sequence.");
                return;
            }

            source = "abcdbchdc";
            iter = c3.GetCollationElementIterator(source);
            CollationTest.BackAndForth(this, iter);

            source = "\u0e41\u0e02\u0e41\u0e02\u0e27abc";
            Collator c4 = null;
            try
            {
                c4 = Collator.GetInstance(new CultureInfo("th-TH"));
            }
            catch (Exception e)
            {
                Errln("Couldn't create a collator");
                return;
            }

            iter = ((RuleBasedCollator)c4).GetCollationElementIterator(source);
            CollationTest.BackAndForth(this, iter);

            source = "\u0061\u30CF\u3099\u30FC";
            Collator c5 = null;
            try
            {
                c5 = Collator.GetInstance(new CultureInfo("ja-JP"));
            }
            catch (Exception e)
            {
                Errln("Couldn't create Japanese collator\n");
                return;
            }
            iter = ((RuleBasedCollator)c5).GetCollationElementIterator(source);

            CollationTest.BackAndForth(this, iter);
        }



        /**
         * Test for setText()
         */
        [Test]
        public void TestSetText(/* char* par */)
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
            CollationElementIterator iter1 = en_us.GetCollationElementIterator(test1);
            CollationElementIterator iter2 = en_us.GetCollationElementIterator(test2);

            // Run through the second iterator just to exercise it
            int c = iter2.Next();
            int i = 0;

            while (++i < 10 && c != CollationElementIterator.NullOrder)
            {
                try
                {
                    c = iter2.Next();
                }
                catch (Exception e)
                {
                    Errln("iter2.Next() returned an error.");
                    break;
                }
            }

            // Now set it to point to the same string as the first iterator
            try
            {
                iter2.SetText(test1);
            }
            catch (Exception e)
            {
                Errln("call to iter2->setText(test1) failed.");
                return;
            }
            assertEqual(iter1, iter2);

            iter1.Reset();
            //now use the overloaded setText(ChracterIterator&, UErrorCode) function to set the text
            CharacterIterator chariter = new StringCharacterIterator(test1);
            try
            {
                iter2.SetText(chariter);
            }
            catch (Exception e)
            {
                Errln("call to iter2->setText(chariter(test1)) failed.");
                return;
            }
            assertEqual(iter1, iter2);

            iter1.Reset();
            //now use the overloaded setText(ChracterIterator&, UErrorCode) function to set the text
            UCharacterIterator uchariter = UCharacterIterator.GetInstance(test1);
            try
            {
                iter2.SetText(uchariter);
            }
            catch (Exception e)
            {
                Errln("call to iter2->setText(uchariter(test1)) failed.");
                return;
            }
            assertEqual(iter1, iter2);
        }

        /**
         * Test for CollationElementIterator previous and next for the whole set of
         * unicode characters.
         */
        [Test]
        public void TestUnicodeChar()
        {
            RuleBasedCollator en_us = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
            CollationElementIterator iter;
            char codepoint;
            StringBuffer source = new StringBuffer();
            source.Append("\u0e4d\u0e4e\u0e4f");
            // source.append("\u04e8\u04e9");
            iter = en_us.GetCollationElementIterator(source.ToString());
            // A basic test to see if it's working at all
            CollationTest.BackAndForth(this, iter);
            for (codepoint = (char)1; codepoint < 0xFFFE;)
            {
                source.Delete(0, source.Length - 0); // ICU4N: Corrected 2nd parameter of Delete
                while (codepoint % 0xFF != 0)
                {
                    if (UChar.IsDefined(codepoint))
                    {
                        source.Append(codepoint);
                    }
                    codepoint++;
                }

                if (UChar.IsDefined(codepoint))
                {
                    source.Append(codepoint);
                }

                if (codepoint != 0xFFFF)
                {
                    codepoint++;
                }
                /*if (codepoint >= 0x04fc) {
                    System.out.println("codepoint " + Integer.toHexString(codepoint));
                    String str = source.substring(230, 232);
                    System.out.println(com.ibm.icu.impl.Utility.escape(str));
                    System.out.println("codepoint " + Integer.toHexString(codepoint)
                                       + "length " + str.Length);
                    iter = en_us.GetCollationElementIterator(str);
                    CollationTest.BackAndForth(this, iter);
                }
                */
                iter = en_us.GetCollationElementIterator(source.ToString());
                // A basic test to see if it's working at all
                CollationTest.BackAndForth(this, iter);
            }
        }

        /**
         * Test for CollationElementIterator previous and next for the whole set of
         * unicode characters with normalization on.
         */
        [Test]
        public void TestNormalizedUnicodeChar()
        {
            // thai should have normalization on
            RuleBasedCollator th_th = null;
            try
            {
                th_th = (RuleBasedCollator)Collator.GetInstance(
                                                           new CultureInfo("th-TH"));
            }
            catch (Exception e)
            {
                Warnln("Error creating Thai collator");
                return;
            }
            StringBuffer source = new StringBuffer();
            source.Append('\uFDFA');
            CollationElementIterator iter
                            = th_th.GetCollationElementIterator(source.ToString());
            CollationTest.BackAndForth(this, iter);
            for (char codepoint = (char)0x1; codepoint < 0xfffe;)
            {
                source.Delete(0, source.Length - 0); // ICU4N: Corrected 2nd parameter of Delete
                while (codepoint % 0xFF != 0)
                {
                    if (UChar.IsDefined(codepoint))
                    {
                        source.Append(codepoint);
                    }
                    codepoint++;
                }

                if (UChar.IsDefined(codepoint))
                {
                    source.Append(codepoint);
                }

                if (codepoint != 0xFFFF)
                {
                    codepoint++;
                }

                /*if (((int)codepoint) >= 0xfe00) {
                    String str = source.substring(185, 190);
                    System.out.println(com.ibm.icu.impl.Utility.escape(str));
                    System.out.println("codepoint "
                                       + Integer.toHexString(codepoint)
                                       + "length " + str.Length);
                    iter = th_th.GetCollationElementIterator(str);
                    CollationTest.BackAndForth(this, iter);
                */
                iter = th_th.GetCollationElementIterator(source.ToString());
                // A basic test to see if it's working at all
                CollationTest.BackAndForth(this, iter);
            }
        }

        /**
        * Testing the discontiguous contractions
        */
        [Test]
        public void TestDiscontiguous()
        {
            String rulestr = "&z < AB < X\u0300 < ABC < X\u0300\u0315";
            String[] src = {"ADB", "ADBC", "A\u0315B", "A\u0315BC",
                        // base character blocked
                        "XD\u0300", "XD\u0300\u0315",
                        // non blocking combining character
                        "X\u0319\u0300", "X\u0319\u0300\u0315",
                        // blocking combining character
                        "X\u0314\u0300", "X\u0314\u0300\u0315",
                        // contraction prefix
                        "ABDC", "AB\u0315C","X\u0300D\u0315",
                        "X\u0300\u0319\u0315", "X\u0300\u031A\u0315",
                        // ends not with a contraction character
                        "X\u0319\u0300D", "X\u0319\u0300\u0315D",
                        "X\u0300D\u0315D", "X\u0300\u0319\u0315D",
                        "X\u0300\u031A\u0315D"
        };
            String[] tgt = {// non blocking combining character
                        "A D B", "A D BC", "A \u0315 B", "A \u0315 BC",
                        // base character blocked
                        "X D \u0300", "X D \u0300\u0315",
                        // non blocking combining character
                        "X\u0300 \u0319", "X\u0300\u0315 \u0319",
                        // blocking combining character
                        "X \u0314 \u0300", "X \u0314 \u0300\u0315",
                        // contraction prefix
                        "AB DC", "AB \u0315 C","X\u0300 D \u0315",
                        "X\u0300\u0315 \u0319", "X\u0300 \u031A \u0315",
                        // ends not with a contraction character
                        "X\u0300 \u0319D", "X\u0300\u0315 \u0319D",
                        "X\u0300 D\u0315D", "X\u0300\u0315 \u0319D",
                        "X\u0300 \u031A\u0315D"
        };
            int count = 0;
            try
            {
                RuleBasedCollator coll = new RuleBasedCollator(rulestr);
                CollationElementIterator iter
                                            = coll.GetCollationElementIterator("");
                CollationElementIterator resultiter
                                            = coll.GetCollationElementIterator("");
                while (count < src.Length)
                {
                    iter.SetText(src[count]);
                    int s = 0;
                    while (s < tgt[count].Length)
                    {
                        int e = tgt[count].IndexOf(' ', s);
                        if (e < 0)
                        {
                            e = tgt[count].Length;
                        }
                        String resultstr = tgt[count].Substring(s, e - s); // ICU4N: Corrected 2nd parameter
                        resultiter.SetText(resultstr);
                        int ce = resultiter.Next();
                        while (ce != CollationElementIterator.NullOrder)
                        {
                            if (ce != iter.Next())
                            {
                                Errln("Discontiguos contraction test mismatch at"
                                      + count);
                                return;
                            }
                            ce = resultiter.Next();
                        }
                        s = e + 1;
                    }
                    iter.Reset();
                    CollationTest.BackAndForth(this, iter);
                    count++;
                }
            }
            catch (Exception e)
            {
                Warnln("Error running discontiguous tests " + e.ToString());
            }
        }

        /**
        * Test the incremental normalization
        */
        [Test]
        public void TestNormalization()
        {
            String rules = "&a < \u0300\u0315 < A\u0300\u0315 < \u0316\u0315B < \u0316\u0300\u0315";
            String[] testdata = {"\u1ED9", "o\u0323\u0302",
                            "\u0300\u0315", "\u0315\u0300",
                            "A\u0300\u0315B", "A\u0315\u0300B",
                            "A\u0316\u0315B", "A\u0315\u0316B",
                            "\u0316\u0300\u0315", "\u0315\u0300\u0316",
                            "A\u0316\u0300\u0315B", "A\u0315\u0300\u0316B",
                            "\u0316\u0315\u0300", "A\u0316\u0315\u0300B"};
            RuleBasedCollator coll = null;
            try
            {
                coll = new RuleBasedCollator(rules);
                coll.Decomposition = NormalizationMode.CanonicalDecomposition; //(Collator.CANONICAL_DECOMPOSITION);
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of collator using rules " + rules);
                return;
            }

            CollationElementIterator iter = coll.GetCollationElementIterator("testing");
            for (int count = 0; count < testdata.Length; count++)
            {
                iter.SetText(testdata[count]);
                CollationTest.BackAndForth(this, iter);
            }
        }

        internal class TSCEItem
        {
            private String localeString;
            private int[] offsets;
            internal TSCEItem(String locStr, int[] offs)
            {
                localeString = locStr;
                offsets = offs;
            }
            public String LocaleString { get { return localeString; } }
            public int[] GetOffsets() { return offsets; }
        }

        /**
         * TestSearchCollatorElements tests iterator behavior (forwards and backwards) with
         * normalization on AND jamo tailoring, among other things.
         *
         * Note: This test is sensitive to changes of the root collator,
         * for example whether the ae-ligature maps to three CEs (as in the DUCET)
         * or to two CEs (as in the CLDR 24 FractionalUCA.txt).
         * It is also sensitive to how those CEs map to the iterator's 32-bit CE encoding.
         * For example, the DUCET's artificial secondary CE in the ae-ligature
         * may map to two 32-bit iterator CEs (as it did until ICU 52).
         */
        [Test]
        public void TestSearchCollatorElements()
        {
            String tsceText =
                " \uAC00" +              // simple LV Hangul
                " \uAC01" +              // simple LVT Hangul
                " \uAC0F" +              // LVTT, last jamo expands for search
                " \uAFFF" +              // LLVVVTT, every jamo expands for search
                " \u1100\u1161\u11A8" +  // 0xAC01 as conjoining jamo
                " \u3131\u314F\u3131" +  // 0xAC01 as compatibility jamo
                " \u1100\u1161\u11B6" +  // 0xAC0F as conjoining jamo; last expands for search
                " \u1101\u1170\u11B6" +  // 0xAFFF as conjoining jamo; all expand for search
                " \u00E6" +              // small letter ae, expands
                " \u1E4D" +              // small letter o with tilde and acute, decomposes
                " ";

            int[] rootStandardOffsets = {
                    0,  1,2,
                    2,  3,4,4,
                    4,  5,6,6,
                    6,  7,8,8,
                    8,  9,10,11,
                    12, 13,14,15,
                    16, 17,18,19,
                    20, 21,22,23,
                    24, 25,26,  /* plus another 1-2 offset=26 if ae-ligature maps to three CEs */
                    26, 27,28,28,
                    28,
                    29
                };

            int[] rootSearchOffsets = {
                    0,  1,2,
                    2,  3,4,4,
                    4,  5,6,6,6,
                    6,  7,8,8,8,8,8,8,
                    8,  9,10,11,
                    12, 13,14,15,
                    16, 17,18,19,20,
                    20, 21,22,22,23,23,23,24,
                    24, 25,26,  /* plus another 1-2 offset=26 if ae-ligature maps to three CEs */
                    26, 27,28,28,
                    28,
                    29
                };


            TSCEItem[] tsceItems = {
                new TSCEItem( "root", rootStandardOffsets ),
                new TSCEItem( "root@collation=search", rootSearchOffsets   ),
            };

            foreach (TSCEItem tsceItem in tsceItems)
            {
                String localeString = tsceItem.LocaleString;
                UCultureInfo uloc = new UCultureInfo(localeString);
                RuleBasedCollator col = null;
                try
                {
                    col = (RuleBasedCollator)Collator.GetInstance(uloc);
                }
                catch (Exception e)
                {
                    Errln("Error: in locale " + localeString + ", err in Collator.getInstance");
                    continue;
                }
                CollationElementIterator uce = col.GetCollationElementIterator(tsceText);
                int[] offsets = tsceItem.GetOffsets();
                int ioff, noff = offsets.Length;
                int offset, element;

                ioff = 0;
                do
                {
                    offset = uce.GetOffset();
                    element = uce.Next();
                    Logln(String.Format("({0}) offset={1:d2}  ce={2:x8}\n", tsceItem.LocaleString, offset, element));
                    if (element == 0)
                    {
                        Errln("Error: in locale " + localeString + ", CEIterator next() returned element 0");
                    }
                    if (ioff < noff)
                    {
                        if (offset != offsets[ioff])
                        {
                            Errln("Error: in locale " + localeString + ", expected CEIterator next()->getOffset " + offsets[ioff] + ", got " + offset);
                            //ioff = noff;
                            //break;
                        }
                        ioff++;
                    }
                    else
                    {
                        Errln("Error: in locale " + localeString + ", CEIterator next() returned more elements than expected");
                    }
                } while (element != CollationElementIterator.NullOrder);
                if (ioff < noff)
                {
                    Errln("Error: in locale " + localeString + ", CEIterator next() returned fewer elements than expected");
                }

                // backwards test
                uce.SetOffset(tsceText.Length);
                ioff = noff;
                do
                {
                    offset = uce.GetOffset();
                    element = uce.Previous();
                    if (element == 0)
                    {
                        Errln("Error: in locale " + localeString + ", CEIterator previous() returned element 0");
                    }
                    if (ioff > 0)
                    {
                        ioff--;
                        if (offset != offsets[ioff])
                        {
                            Errln("Error: in locale " + localeString + ", expected CEIterator previous()->getOffset " + offsets[ioff] + ", got " + offset);
                            //ioff = 0;
                            //break;
                        }
                    }
                    else
                    {
                        Errln("Error: in locale " + localeString + ", CEIterator previous() returned more elements than expected");
                    }
                } while (element != CollationElementIterator.NullOrder);
                if (ioff > 0)
                {
                    Errln("Error: in locale " + localeString + ", CEIterator previous() returned fewer elements than expected");
                }
            }
        }
    }
}
