using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
#if !NETSTANDARD
using System.Threading;
#endif
using StringBuffer = System.Text.StringBuilder;

//Regression testing of RuleBasedBreakIterator
//
//  TODO:  These tests should be mostly retired.
//          Much of the test data that was originally here was removed when the RBBI rules
//            were updated to match the Unicode boundary TRs, and the data was found to be invalid.
//          Much of the remaining data has been moved into the rbbitst.txt test data file,
//            which is common between ICU4C and ICU4J.  The remaining test data should also be moved,
//            or simply retired if it is no longer interesting.

namespace ICU4N.Dev.Test.Rbbi
{
    public class RBBITest : TestFmwk
    {
        public RBBITest()
        {
        }



        [Test]
        public void TestThaiDictionaryBreakIterator()
        {
            int position;
            int index;
            int[] result = { 1, 2, 5, 10, 11, 12, 11, 10, 5, 2, 1, 0 };
            char[] ctext = {
               (char)0x0041, (char)0x0020,
               (char)0x0E01, (char)0x0E32, (char)0x0E23, (char)0x0E17, (char)0x0E14, (char)0x0E25, (char)0x0E2D, (char)0x0E07,
               (char)0x0020, (char)0x0041
               };
            String text = new String(ctext);

            ULocale locale = ULocale.CreateCanonical("th");
            BreakIterator b = BreakIterator.GetWordInstance(locale);

            b.SetText(text);

            index = 0;
            // Test forward iteration
            while ((position = b.Next()) != BreakIterator.Done)
            {
                if (position != result[index++])
                {
                    Errln("Error with ThaiDictionaryBreakIterator forward iteration test at " + position + ".\nShould have been " + result[index - 1]);
                }
            }

            // Test backward iteration
            while ((position = b.Previous()) != BreakIterator.Done)
            {
                if (position != result[index++])
                {
                    Errln("Error with ThaiDictionaryBreakIterator backward iteration test at " + position + ".\nShould have been " + result[index - 1]);
                }
            }

            //Test invalid sequence and spaces
            char[] text2 = {
               (char)0x0E01, (char)0x0E39, (char)0x0020, (char)0x0E01, (char)0x0E34, (char)0x0E19, (char)0x0E01, (char)0x0E38, (char)0x0E49, (char)0x0E07, (char)0x0020, (char)0x0E1B,
               (char)0x0E34, (char)0x0E49, (char)0x0E48, (char)0x0E07, (char)0x0E2D, (char)0x0E22, (char)0x0E39, (char)0x0E48, (char)0x0E43, (char)0x0E19,
               (char)0x0E16, (char)0x0E49, (char)0x0E33
       };
            int[] expectedWordResult = {
               2, 3, 6, 10, 11, 15, 17, 20, 22
       };
            int[] expectedLineResult = {
               3, 6, 11, 15, 17, 20, 22
       };
            BreakIterator brk = BreakIterator.GetWordInstance(new ULocale("th"));
            brk.SetText(new String(text2));
            position = index = 0;
            while ((position = brk.Next()) != BreakIterator.Done && position < text2.Length)
            {
                if (position != expectedWordResult[index++])
                {
                    Errln("Incorrect break given by thai word break iterator. Expected: " + expectedWordResult[index - 1] + " Got: " + position);
                }
            }

            brk = BreakIterator.GetLineInstance(new ULocale("th"));
            brk.SetText(new String(text2));
            position = index = 0;
            while ((position = brk.Next()) != BreakIterator.Done && position < text2.Length)
            {
                if (position != expectedLineResult[index++])
                {
                    Errln("Incorrect break given by thai line break iterator. Expected: " + expectedLineResult[index - 1] + " Got: " + position);
                }
            }
            // Improve code coverage
            if (brk.Preceding(expectedLineResult[1]) != expectedLineResult[0])
            {
                Errln("Incorrect preceding position.");
            }
            if (brk.Following(expectedLineResult[1]) != expectedLineResult[2])
            {
                Errln("Incorrect following position.");
            }
            int[] fillInArray = new int[2];
            if (((RuleBasedBreakIterator)brk).GetRuleStatusVec(fillInArray) != 1 || fillInArray[0] != 0)
            {
                Errln("Error: Since getRuleStatusVec is not supported in DictionaryBasedBreakIterator, it should return 1 and fillInArray[0] == 0.");
            }
        }

        internal class TBItem
        {
            private int type;
            private ULocale locale;
            private String text;
            private int[] expectOffsets;
            internal TBItem(int typ, ULocale loc, String txt, int[] eOffs)
            {
                type = typ;
                locale = loc;
                text = txt;
                expectOffsets = eOffs;
            }
            private const int maxOffsetCount = 128;
            private bool offsetsMatchExpected(int[] foundOffsets, int foundOffsetsLength)
            {
                if (foundOffsetsLength != expectOffsets.Length)
                {
                    return false;
                }
                for (int i = 0; i < foundOffsetsLength; i++)
                {
                    if (foundOffsets[i] != expectOffsets[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            private String formatOffsets(int[] offsets, int length)
            {
                StringBuffer buildString = new StringBuffer(4 * maxOffsetCount);
                for (int i = 0; i < length; i++)
                {
                    buildString.Append(" " + offsets[i]);
                }
                return buildString.ToString();
            }

            public void doTest()
            {
                BreakIterator brkIter;
                switch (type)
                {
                    case BreakIterator.KIND_CHARACTER: brkIter = BreakIterator.GetCharacterInstance(locale); break;
                    case BreakIterator.KIND_WORD: brkIter = BreakIterator.GetWordInstance(locale); break;
                    case BreakIterator.KIND_LINE: brkIter = BreakIterator.GetLineInstance(locale); break;
                    case BreakIterator.KIND_SENTENCE: brkIter = BreakIterator.GetSentenceInstance(locale); break;
                    default: Errln("Unsupported break iterator type " + type); return;
                }
                brkIter.SetText(text);
                int[] foundOffsets = new int[maxOffsetCount];
                int offset, foundOffsetsCount = 0;
                // do forwards iteration test
                while (foundOffsetsCount < maxOffsetCount && (offset = brkIter.Next()) != BreakIterator.Done)
                {
                    foundOffsets[foundOffsetsCount++] = offset;
                }
                if (!offsetsMatchExpected(foundOffsets, foundOffsetsCount))
                {
                    // log error for forwards test
                    String textToDisplay = (text.Length <= 16) ? text : text.Substring(0, 16 - 0); // ICU4N: Checked 2nd parameter
                    Errln("For type " + type + " " + locale + ", text \"" + textToDisplay + "...\"" +
                            "; expect " + expectOffsets.Length + " offsets:" + formatOffsets(expectOffsets, expectOffsets.Length) +
                            "; found " + foundOffsetsCount + " offsets fwd:" + formatOffsets(foundOffsets, foundOffsetsCount));
                }
                else
                {
                    // do backwards iteration test
                    --foundOffsetsCount; // back off one from the end offset
                    while (foundOffsetsCount > 0)
                    {
                        offset = brkIter.Previous();
                        if (offset != foundOffsets[--foundOffsetsCount])
                        {
                            // log error for backwards test
                            String textToDisplay = (text.Length <= 16) ? text : text.Substring(0, 16 - 0); // ICU4N: Checked 2nd parameter
                            Errln("For type " + type + " " + locale + ", text \"" + textToDisplay + "...\"" +
                                    "; expect " + expectOffsets.Length + " offsets:" + formatOffsets(expectOffsets, expectOffsets.Length) +
                                    "; found rev offset " + offset + " where expect " + foundOffsets[foundOffsetsCount]);
                            break;
                        }
                    }
                }
            }
        }

        // TODO: Move these test cases to rbbitst.txt if they aren't there already, then remove this test. It is redundant.
        [Test]
        public void TestTailoredBreaks()
        {

            // KIND_SENTENCE "el"
            string elSentText = "\u0391\u03B2, \u03B3\u03B4; \u0395 \u03B6\u03B7\u037E \u0398 \u03B9\u03BA. " +
                                          "\u039B\u03BC \u03BD\u03BE! \u039F\u03C0, \u03A1\u03C2? \u03A3";
            int[] elSentTOffsets = { 8, 14, 20, 27, 35, 36 };
            int[] elSentROffsets = { 20, 27, 35, 36 };
            // KIND_CHARACTER "th"
            string thCharText = "\u0E01\u0E23\u0E30\u0E17\u0E48\u0E2D\u0E21\u0E23\u0E08\u0E19\u0E32 " +
                                          "(\u0E2A\u0E38\u0E0A\u0E32\u0E15\u0E34-\u0E08\u0E38\u0E11\u0E32\u0E21\u0E32\u0E28) " +
                                          "\u0E40\u0E14\u0E47\u0E01\u0E21\u0E35\u0E1B\u0E31\u0E0D\u0E2B\u0E32 ";
            int[] thCharTOffsets = { 1, 2, 3, 5, 6, 7, 8, 9, 10, 11,
                                        12, 13, 15, 16, 17, 19, 20, 22, 23, 24, 25, 26, 27, 28,
                                        29, 30, 32, 33, 35, 37, 38, 39, 40, 41 };
            //starting in Unicode 6.1, root behavior should be the same as Thai above
            //final int[]  thCharROffsets = { 1,    3, 5, 6, 7, 8, 9,     11,
            //                                12, 13, 15,     17, 19, 20, 22,     24,     26, 27, 28,
            //                                29,     32, 33, 35, 37, 38,     40, 41 };

            TBItem[] tests = {
            new TBItem(BreakIterator.KIND_SENTENCE,  new ULocale("el"),          elSentText,   elSentTOffsets   ),
            new TBItem(BreakIterator.KIND_SENTENCE, ULocale.ROOT, elSentText, elSentROffsets   ),
            new TBItem(BreakIterator.KIND_CHARACTER, new ULocale("th"),          thCharText,   thCharTOffsets   ),
            new TBItem(BreakIterator.KIND_CHARACTER, ULocale.ROOT, thCharText, thCharTOffsets   ),
        };
            for (int iTest = 0; iTest < tests.Length; iTest++)
            {
                tests[iTest].doTest();
            }
        }

        /* Tests the method public Object clone() */
        [Test]
        public void TestClone()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            try
            {
                rbbi.SetText((CharacterIterator)null);
                if (((RuleBasedBreakIterator)rbbi.Clone()).Text != null)
                    Errln("RuleBasedBreakIterator.clone() was suppose to return "
                            + "the same object because fText is set to null.");
            }
            catch (Exception e)
            {
                Errln("RuleBasedBreakIterator.clone() was not suppose to return " + "an exception.");
            }
        }

        /*
         * Tests the method public boolean equals(Object that)
         */
        [Test]
        public void TestEquals()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            RuleBasedBreakIterator rbbi1 = new RuleBasedBreakIterator(".;");

            // TODO: Tests when "if (fRData != other.fRData && (fRData == null || other.fRData == null))" is true

            // Tests when "if (fText == null || other.fText == null)" is true
            rbbi.SetText((CharacterIterator)null);
            if (rbbi.Equals(rbbi1))
            {
                Errln("RuleBasedBreakIterator.equals(Object) was not suppose to return "
                        + "true when the other object has a null fText.");
            }

            // Tests when "if (fText == null && other.fText == null)" is true
            rbbi1.SetText((CharacterIterator)null);
            if (!rbbi.Equals(rbbi1))
            {
                Errln("RuleBasedBreakIterator.equals(Object) was not suppose to return "
                        + "false when both objects has a null fText.");
            }

            // Tests when an exception occurs
            if (rbbi.Equals(0))
            {
                Errln("RuleBasedBreakIterator.equals(Object) was suppose to return " + "false when comparing to integer 0.");
            }
            if (rbbi.Equals(0.0))
            {
                Errln("RuleBasedBreakIterator.equals(Object) was suppose to return " + "false when comparing to float 0.0.");
            }
            if (rbbi.Equals("0"))
            {
                Errln("RuleBasedBreakIterator.equals(Object) was suppose to return "
                        + "false when comparing to string '0'.");
            }
        }

        /*
         * Tests the method public int first()
         */
        [Test]
        public void TestFirst()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            // Tests when "if (fText == null)" is true
            rbbi.SetText((CharacterIterator)null);
            assertEquals("RuleBasedBreakIterator.First()", BreakIterator.Done, rbbi.First());

            rbbi.SetText("abc");
            assertEquals("RuleBasedBreakIterator.First()", 0, rbbi.First());
            assertEquals("RuleBasedBreakIterator.Next()", 1, rbbi.Next());
        }

        /*
         * Tests the method public int last()
         */
        [Test]
        public void TestLast()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            // Tests when "if (fText == null)" is true
            rbbi.SetText((CharacterIterator)null);
            if (rbbi.Last() != BreakIterator.Done)
            {
                Errln("RuleBasedBreakIterator.Last() was supposed to return "
                        + "BreakIterator.Done when the object has a null fText.");
            }
        }

        /*
         * Tests the method public int following(int offset)
         */
        [Test]
        public void TestFollowing()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            // Tests when "else if (offset < fText.getBeginIndex())" is true
            rbbi.SetText("dummy");
            if (rbbi.Following(-1) != 0)
            {
                Errln("RuleBasedBreakIterator.following(-1) was suppose to return "
                        + "0 when the object has a fText of dummy.");
            }
        }

        /*
         * Tests the method public int preceding(int offset)
         */
        [Test]
        public void TestPreceding()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            // Tests when "if (fText == null || offset > fText.getEndIndex())" is true
            rbbi.SetText((CharacterIterator)null);
            if (rbbi.Preceding(-1) != BreakIterator.Done)
            {
                Errln("RuleBasedBreakIterator.Preceding(-1) was suppose to return "
                        + "0 when the object has a fText of null.");
            }

            // Tests when "else if (offset < fText.getBeginIndex())" is true
            rbbi.SetText("dummy");
            if (rbbi.Preceding(-1) != 0)
            {
                Errln("RuleBasedBreakIterator.Preceding(-1) was suppose to return "
                        + "0 when the object has a fText of dummy.");
            }
        }

        /* Tests the method public int current() */
        [Test]
        public void TestCurrent()
        {
            RuleBasedBreakIterator rbbi = new RuleBasedBreakIterator(".;");
            // Tests when "(fText != null) ? fText.getIndex() : BreakIterator.Done" is true and false
            rbbi.SetText((CharacterIterator)null);
            if (rbbi.Current != BreakIterator.Done)
            {
                Errln("RuleBasedBreakIterator.Current was suppose to return "
                        + "BreakIterator.Done when the object has a fText of null.");
            }
            rbbi.SetText("dummy");
            if (rbbi.Current != 0)
            {
                Errln("RuleBasedBreakIterator.Current was suppose to return "
                        + "0 when the object has a fText of dummy.");
            }
        }

        [Test]
        public void TestBug7547()
        {
            try
            {
                new RuleBasedBreakIterator("");
                fail("TestBug7547: RuleBasedBreakIterator constructor failed to throw an exception with empty rules.");
            }
            catch (ArgumentException e)
            {
                // expected exception with empty rules.
            }
            catch (Exception e)
            {
                fail("TestBug7547: Unexpected exception while creating RuleBasedBreakIterator: " + e);
            }
        }

        [Test]
        public void TestBug12797()
        {
            String rules = "!!chain; !!forward; $v=b c; a b; $v; !!reverse; .*;";
            RuleBasedBreakIterator bi = new RuleBasedBreakIterator(rules);

            bi.SetText("abc");
            bi.First();
            assertEquals("Rule chaining test", 3, bi.Next());
        }

        internal class WorkerThread : ThreadJob
        {
            private readonly string dataToBreak;
            private readonly RuleBasedBreakIterator bi;
            private readonly AssertionException[] assertErr;

            public WorkerThread(string dataToBreak, RuleBasedBreakIterator bi, AssertionException[] assertErr)
            {
                this.dataToBreak = dataToBreak;
                this.bi = bi;
                this.assertErr = assertErr;
            }

            public override void Run()
            {
                try
                {
                    RuleBasedBreakIterator localBI = (RuleBasedBreakIterator)bi.Clone();
                    localBI.SetText(dataToBreak);
                    for (int loop = 0; loop < 100; loop++)
                    {
                        int nextExpectedBreak = 0;
                        for (int actualBreak = localBI.First(); actualBreak != BreakIterator.Done;
                                actualBreak = localBI.Next(), nextExpectedBreak += 4)
                        {
                            assertEquals("", nextExpectedBreak, actualBreak);
                        }
                        assertEquals("", dataToBreak.Length + 4, nextExpectedBreak);
                    }
                }
                catch (AssertionException e)
                {
                    assertErr[0] = e;
                }
            }
        }


        [Test]
        public void TestBug12873()
        {
            // Bug with RuleBasedBreakIterator's internal structure for recording potential look-ahead
            // matches not being cloned when a break iterator is cloned. This resulted in usage
            // collisions if the original break iterator and its clone were used concurrently.

            // The Line Break rules for Regional Indicators make use of look-ahead rules, and
            // show the bug. 1F1E6 = \uD83C\uDDE6 = REGIONAL INDICATOR SYMBOL LETTER A
            // Regional indicators group into pairs, expect breaks after two code points, which
            // is after four 16 bit code units.

            string dataToBreak = "\uD83C\uDDE6\uD83C\uDDE6\uD83C\uDDE6\uD83C\uDDE6\uD83C\uDDE6\uD83C\uDDE6";
            RuleBasedBreakIterator bi = (RuleBasedBreakIterator)BreakIterator.GetLineInstance();
            AssertionException[] assertErr = new AssertionException[1];  // saves an error found from within a thread



            List<ThreadJob> threads = new List<ThreadJob>();
            for (int n = 0; n < 4; ++n)
            {
                threads.Add(new WorkerThread(dataToBreak, bi, assertErr));
            }
            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
#if !NETCOREAPP1_0
                try {
#endif
                thread.Join();
#if !NETCOREAPP1_0
            } catch (ThreadInterruptedException e) {
                fail(e.ToString());
            }
#endif
            }

            // JUnit wont see failures from within the worker threads, so
            // check again if one occurred.
            if (assertErr[0] != null)
            {
                throw assertErr[0];
            }
        }

        [Test]
        public void TestBreakAllChars()
        {
            // Make a "word" from each code point, separated by spaces.
            // For dictionary based breaking, runs the start-of-range
            // logic with all possible dictionary characters.
            StringBuilder sb = new StringBuilder();
            for (int c = 0; c < 0x110000; ++c)
            {
                sb.AppendCodePoint(c);
                sb.AppendCodePoint(c);
                sb.AppendCodePoint(c);
                sb.AppendCodePoint(c);
                sb.Append(' ');
            }
            String s = sb.ToString();

            for (int breakKind = BreakIterator.KIND_CHARACTER; breakKind <= BreakIterator.KIND_TITLE; ++breakKind)
            {
                RuleBasedBreakIterator bi =
                        (RuleBasedBreakIterator)BreakIterator.GetBreakInstance(ULocale.ENGLISH, breakKind);
                bi.SetText(s);
                int lastb = -1;
                for (int b = bi.First(); b != BreakIterator.Done; b = bi.Next())
                {
                    assertTrue("(lastb < b) : (" + lastb + " < " + b + ")", lastb < b);
                }
            }
        }

        [Test]
        public void TestBug12918()
        {
            // This test triggered an assertion failure in ICU4C, in dictbe.cpp
            // The equivalent code in ICU4J is structured slightly differently,
            // and does not appear vulnerable to the same issue.
            //
            // \u3325 decomposes with normalization, then the CJK dictionary
            // finds a break within the decomposition.

            String crasherString = "\u3325\u4a16";
            BreakIterator iter = BreakIterator.GetWordInstance(ULocale.ENGLISH);
            iter.SetText(crasherString);
            iter.First();
            int pos = 0;
            int lastPos = -1;
            while ((pos = iter.Next()) != BreakIterator.Done)
            {
                assertTrue("", pos > lastPos);
            }
        }

        [Test]
        public void TestBug12519()
        {
            RuleBasedBreakIterator biEn = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(ULocale.ENGLISH);
            RuleBasedBreakIterator biFr = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(ULocale.FRANCE);
            assertEquals("", ULocale.ENGLISH, biEn.GetLocale(ULocale.VALID_LOCALE));
            assertEquals("", ULocale.FRENCH, biFr.GetLocale(ULocale.VALID_LOCALE));
            assertEquals("Locales do not participate in BreakIterator equality.", biEn, biFr);

            RuleBasedBreakIterator cloneEn = (RuleBasedBreakIterator)biEn.Clone();
            assertEquals("", biEn, cloneEn);
            assertEquals("", ULocale.ENGLISH, cloneEn.GetLocale(ULocale.VALID_LOCALE));

            RuleBasedBreakIterator cloneFr = (RuleBasedBreakIterator)biFr.Clone();
            assertEquals("", biFr, cloneFr);
            assertEquals("", ULocale.FRENCH, cloneFr.GetLocale(ULocale.VALID_LOCALE));
        }
    }
}
