using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Text;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Character = J2N.Character;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Rbbi
{
    public class BreakIteratorTest : TestFmwk
    {
        private BreakIterator characterBreak;
        private BreakIterator wordBreak;
        private BreakIterator lineBreak;
        private BreakIterator sentenceBreak;
        private BreakIterator titleBreak;

        public BreakIteratorTest()
        {

        }

        [SetUp]
        public void Init()
        {
            characterBreak = BreakIterator.GetCharacterInstance();
            wordBreak = BreakIterator.GetWordInstance();
            lineBreak = BreakIterator.GetLineInstance();
            //Logln("Creating sentence iterator...");
            sentenceBreak = BreakIterator.GetSentenceInstance();
            //Logln("Finished creating sentence iterator...");
            titleBreak = BreakIterator.GetTitleInstance();
        }
        //=========================================================================
        // general test subroutines
        //=========================================================================

        private List<String> _testFirstAndNext(BreakIterator bi, String text)
        {
            int p = bi.First();
            int lastP = p;
            List<String> result = new List<String>();

            if (p != 0)
                Errln("first() returned " + p + " instead of 0");
            while (p != BreakIterator.Done)
            {
                p = bi.Next();
                if (p != BreakIterator.Done)
                {
                    if (p <= lastP)
                        Errln("next() failed to move forward: next() on position "
                                        + lastP + " yielded " + p);

                    result.Add(text.Substring(lastP, p - lastP)); // ICU4N: Corrected 2nd substring parameter
                }
                else
                {
                    if (lastP != text.Length)
                        Errln("next() returned DONE prematurely: offset was "
                                        + lastP + " instead of " + text.Length);
                }
                lastP = p;
            }
            return result;
        }

        private List<String> _testLastAndPrevious(BreakIterator bi, String text)
        {
            int p = bi.Last();
            int lastP = p;
            List<String> result = new List<String>();

            if (p != text.Length)
                Errln("last() returned " + p + " instead of " + text.Length);
            while (p != BreakIterator.Done)
            {
                p = bi.Previous();
                if (p != BreakIterator.Done)
                {
                    if (p >= lastP)
                        Errln("previous() failed to move backward: previous() on position "
                                        + lastP + " yielded " + p);

                    result.Insert(0, text.Substring(p, lastP - p)); // ICU4N: Corrected 2nd substring parameter
                }
                else
                {
                    if (lastP != 0)
                        Errln("previous() returned DONE prematurely: offset was "
                                        + lastP + " instead of 0");
                }
                lastP = p;
            }
            return result;
        }

        private void compareFragmentLists(String f1Name, String f2Name, List<String> f1, List<String> f2)
        {
            int p1 = 0;
            int p2 = 0;
            String s1;
            String s2;
            int t1 = 0;
            int t2 = 0;

            while (p1 < f1.Count && p2 < f2.Count)
            {
                s1 = f1[p1];
                s2 = f2[p2];
                t1 += s1.Length;
                t2 += s2.Length;

                if (s1.Equals(s2))
                {
                    debugLogln("   >" + s1 + "<");
                    ++p1;
                    ++p2;
                }
                else
                {
                    int tempT1 = t1;
                    int tempT2 = t2;
                    int tempP1 = p1;
                    int tempP2 = p2;

                    while (tempT1 != tempT2 && tempP1 < f1.Count && tempP2 < f2.Count)
                    {
                        while (tempT1 < tempT2 && tempP1 < f1.Count)
                        {
                            tempT1 += (f1[tempP1]).Length;
                            ++tempP1;
                        }
                        while (tempT2 < tempT1 && tempP2 < f2.Count)
                        {
                            tempT2 += (f2[tempP2]).Length;
                            ++tempP2;
                        }
                    }
                    Logln("*** " + f1Name + " has:");
                    while (p1 <= tempP1 && p1 < f1.Count)
                    {
                        s1 = f1[p1];
                        t1 += s1.Length;
                        debugLogln(" *** >" + s1 + "<");
                        ++p1;
                    }
                    Logln("***** " + f2Name + " has:");
                    while (p2 <= tempP2 && p2 < f2.Count)
                    {
                        s2 = f2[p2];
                        t2 += s2.Length;
                        debugLogln(" ***** >" + s2 + "<");
                        ++p2;
                    }
                    Errln("Discrepancy between " + f1Name + " and " + f2Name);
                }
            }
        }

        private void _testFollowing(BreakIterator bi, String text, int[] boundaries)
        {
            Logln("testFollowing():");
            int p = 2;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == boundaries[p])
                    ++p;

                int b = bi.Following(i);
                Logln("bi.following(" + i + ") -> " + b);
                if (b != boundaries[p])
                    Errln("Wrong result from following() for " + i + ": expected " + boundaries[p]
                                    + ", got " + b);
            }
        }

        private void _testPreceding(BreakIterator bi, String text, int[] boundaries)
        {
            Logln("testPreceding():");
            int p = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                int b = bi.Preceding(i);
                Logln("bi.preceding(" + i + ") -> " + b);
                if (b != boundaries[p])
                    Errln("Wrong result from preceding() for " + i + ": expected " + boundaries[p]
                                    + ", got " + b);

                if (i == boundaries[p + 1])
                    ++p;
            }
        }

        private void _testIsBoundary(BreakIterator bi, String text, int[] boundaries)
        {
            Logln("testIsBoundary():");
            int p = 1;
            bool isB;
            for (int i = 0; i <= text.Length; i++)
            {
                isB = bi.IsBoundary(i);
                Logln("bi.isBoundary(" + i + ") -> " + isB);

                if (i == boundaries[p])
                {
                    if (!isB)
                        Errln("Wrong result from isBoundary() for " + i + ": expected true, got false");
                    ++p;
                }
                else
                {
                    if (isB)
                        Errln("Wrong result from isBoundary() for " + i + ": expected false, got true");
                }
            }
        }

        private void doOtherInvariantTest(BreakIterator tb, String testChars)
        {
            StringBuffer work = new StringBuffer("a\r\na");
            int errorCount = 0;

            // a break should never occur between CR and LF
            for (int i = 0; i < testChars.Length; i++)
            {
                work[0] = testChars[i];
                for (int j = 0; j < testChars.Length; j++)
                {
                    work[3] = testChars[j];
                    tb.SetText(work.ToString());
                    for (int k = tb.First(); k != BreakIterator.Done; k = tb.Next())
                        if (k == 2)
                        {
                            Errln("Break between CR and LF in string U+" +
                                    (work[0]).ToHexString() + ", U+d U+a U+" +
                                    (work[3]).ToHexString());
                            errorCount++;
                            if (errorCount >= 75)
                                return;
                        }
                }
            }

            // a break should never occur before a non-spacing mark, unless it's preceded
            // by a line terminator
            work.Length = (0);
            work.Append("aaaa");
            for (int i = 0; i < testChars.Length; i++)
            {
                char c = testChars[i];
                if (c == '\n' || c == '\r' || c == '\u2029' || c == '\u2028' || c == '\u0003')
                    continue;
                work[1] = c;
                for (int j = 0; j < testChars.Length; j++)
                {
                    c = testChars[j];
                    if (Character.GetType(c) != UnicodeCategory.NonSpacingMark && Character.GetType(c)
                            != UnicodeCategory.EnclosingMark)
                        continue;
                    work[2] = c;
                    tb.SetText(work.ToString());
                    for (int k = tb.First(); k != BreakIterator.Done; k = tb.Next())
                        if (k == 2)
                        {
                            Errln("Break between U+" + ((work[1])).ToHexString()
                                    + " and U+" + ((work[2])).ToHexString());
                            errorCount++;
                            if (errorCount >= 75)
                                return;
                        }
                }
            }
        }

        public void debugLogln(String s)
        {
            string zeros = "0000";
            string temp;
            StringBuffer @out = new StringBuffer();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= ' ' && c < '\u007f')
                    @out.Append(c);
                else
                {
                    @out.Append("\\u");
                    temp = (c).ToHexString();
                    @out.Append(zeros.Substring(0, (4 - temp.Length) - 0)); // ICU4N: Checked 2nd substring parameter
                    @out.Append(temp);
                }
            }
            Logln(@out.ToString());
        }

        //=========================================================================
        // tests
        //=========================================================================


        /*
         * @bug 4153072
         */
        [Test]
        public void TestBug4153072()
        {
            BreakIterator iter = BreakIterator.GetWordInstance();
            String str = "...Hello, World!...";
            int begin = 3;
            int end = str.Length - 3;
            // not used boolean gotException = false;


            iter.SetText(new StringCharacterEnumerator(str, begin, Math.Max(end - begin, 0), begin));
            for (int index = -1; index < begin + 1; ++index)
            {
                try
                {
                    iter.IsBoundary(index);
                    if (index < begin)
                        Errln("Didn't get exception with offset = " + index +
                                        " and begin index = " + begin);
                }
                catch (ArgumentException e)
                {
                    if (index >= begin)
                        Errln("Got exception with offset = " + index +
                                        " and begin index = " + begin);
                }
            }
        }


        private const string cannedTestChars
        = "\u0000\u0001\u0002\u0003\u0004 !\"#$%&()+-01234<=>ABCDE[]^_`abcde{}|\u00a0\u00a2"
        + "\u00a3\u00a4\u00a5\u00a6\u00a7\u00a8\u00a9\u00ab\u00ad\u00ae\u00af\u00b0\u00b2\u00b3"
        + "\u00b4\u00b9\u00bb\u00bc\u00bd\u02b0\u02b1\u02b2\u02b3\u02b4\u0300\u0301\u0302\u0303"
        + "\u0304\u05d0\u05d1\u05d2\u05d3\u05d4\u0903\u093e\u093f\u0940\u0949\u0f3a\u0f3b\u2000"
        + "\u2001\u2002\u200c\u200d\u200e\u200f\u2010\u2011\u2012\u2028\u2029\u202a\u203e\u203f"
        + "\u2040\u20dd\u20de\u20df\u20e0\u2160\u2161\u2162\u2163\u2164";

        [Test]
        public void TestSentenceInvariants()
        {
            BreakIterator e = BreakIterator.GetSentenceInstance();
            doOtherInvariantTest(e, cannedTestChars + ".,\u3001\u3002\u3041\u3042\u3043\ufeff");
        }

        [Test]
        
        public void TestGetAvailableLocales()
        {
            CultureInfo[] locList = BreakIterator.GetCultures(UCultureTypes.AllCultures);

            if (locList.Length == 0)
                Errln("GetCultures() returned an empty list!");
            // I have no idea how to test this function...

            UCultureInfo[] ulocList = BreakIterator.GetUCultures(UCultureTypes.AllCultures);
            if (ulocList.Length == 0)
            {
                Errln("GetUCultures() returned an empty list!");
            }
            else
            {
                Logln("GetUCultures() returned " + ulocList.Length + " locales");
            }
            foreach (var specificCulture in BreakIterator.GetCultures(UCultureTypes.SpecificCultures))
            {
                assertFalse($"Expected a specific culture, got '{specificCulture.Name}'", specificCulture.IsNeutralCulture);
            }
            foreach (var neutralCulture in BreakIterator.GetCultures(UCultureTypes.NeutralCultures))
            {
                assertTrue($"Expected a neutral culture, got '{neutralCulture.Name}'", neutralCulture.IsNeutralCulture);
            }
        }


        /**
         * @bug 4068137
         */
        [Test]
        public void TestEndBehavior()
        {
            String testString = "boo.";
            BreakIterator wb = BreakIterator.GetWordInstance();
            wb.SetText(testString);

            if (wb.First() != 0)
                Errln("Didn't get break at beginning of string.");
            if (wb.Next() != 3)
                Errln("Didn't get break before period in \"boo.\"");
            if (wb.Current != 4 && wb.Next() != 4)
                Errln("Didn't get break at end of string.");
        }

        // The Following two tests are ported from ICU4C 1.8.1 [Richard/GCL]
        /**
         * Port From:   ICU4C v1.8.1 : textbounds : IntlTestTextBoundary
         * Source File: $ICU4CRoot/source/test/intltest/ittxtbd.cpp
         **/
        /**
         * test methods preceding, following and isBoundary
         **/
        [Test]
        public void TestPreceding()
        {
            String words3 = "aaa bbb ccc";
            BreakIterator e = BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);
            e.SetText(words3);
            int t = e.First();
            int p1 = e.Next();
            int p2 = e.Next();
            int p3 = e.Next();
            int p4 = e.Next();

            int f = e.Following(p2 + 1);
            int p = e.Preceding(p2 + 1);
            if (f != p3)
                Errln("IntlTestTextBoundary::TestPreceding: f!=p3");
            if (p != p2)
                Errln("IntlTestTextBoundary::TestPreceding: p!=p2");

            if (p1 + 1 != p2)
                Errln("IntlTestTextBoundary::TestPreceding: p1+1!=p2");

            if (p3 + 1 != p4)
                Errln("IntlTestTextBoundary::TestPreceding: p3+1!=p4");

            if (!e.IsBoundary(p2) || e.IsBoundary(p2 + 1) || !e.IsBoundary(p3))
            {
                Errln("IntlTestTextBoundary::TestPreceding: isBoundary err");
            }
        }

        /**
         * Ticket#5615
         */
        [Test]
        public void TestT5615()
        {
            UCultureInfo[] ulocales = BreakIterator.GetUCultures(UCultureTypes.AllCultures);
            int type = 0;
            UCultureInfo loc = null;
            try
            {
                for (int i = 0; i < ulocales.Length; i++)
                {
                    loc = ulocales[i];
                    for (type = 0; type < 5 /* 5 = BreakIterator.KIND_COUNT */; ++type)
                    {
                        BreakIterator brk = BreakIterator.GetBreakInstance(loc, type);
                        if (brk == null)
                        {
                            Errln("ERR: Failed to create an instance type: " + type + " / locale: " + loc);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Errln("ERR: Failed to create an instance type: " + type + " / locale: " + loc + " / exception: " + e.ToString());
            }
        }

        /**
         * At present, Japanese doesn't have exceptions.
         * However, this still should not fail.
         */
        [Test]
        public void TestFilteredJapanese()
        {
            UCultureInfo loc = new UCultureInfo("ja");
            BreakIterator brk = FilteredBreakIteratorBuilder
                    .GetInstance(loc)
                    .WrapIteratorWithFilter(BreakIterator.GetSentenceInstance(loc));
            brk.SetText("ＯＫです。");
            assertEquals("Starting point", 0, brk.Current);
            assertEquals("Next point", 5, brk.Next());
            assertEquals("Last point", BreakIterator.Done, brk.Next());
        }

        /*
         * Test case for Ticket#10721. BreakIterator factory method should throw NPE
         * when specified locale is null.
         */
        [Test]
        public void TestNullLocale()
        {
            CultureInfo loc = null;
            UCultureInfo uloc = null;

            BreakIterator brk;

            // Character
            try
            {
                brk = BreakIterator.GetCharacterInstance(loc);
                Errln("GetCharacterInstance((CultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
            try
            {
                brk = BreakIterator.GetCharacterInstance(uloc);
                Errln("GetCharacterInstance((UCultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }

            // Line
            try
            {
                brk = BreakIterator.GetLineInstance(loc);
                Errln("GetLineInstance((CultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
            try
            {
                brk = BreakIterator.GetLineInstance(uloc);
                Errln("GetLineInstance((UCultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }

            // Sentence
            try
            {
                brk = BreakIterator.GetSentenceInstance(loc);
                Errln("GetSentenceInstance((CultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
            try
            {
                brk = BreakIterator.GetSentenceInstance(uloc);
                Errln("GetSentenceInstance((UCultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }

            // Title
            try
            {
                brk = BreakIterator.GetTitleInstance(loc);
                Errln("GetTitleInstance((CultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
            try
            {
                brk = BreakIterator.GetTitleInstance(uloc);
                Errln("GetTitleInstance((UCultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }

            // Word
            try
            {
                brk = BreakIterator.GetWordInstance(loc);
                Errln("GetWordInstance((CultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
            try
            {
                brk = BreakIterator.GetWordInstance(uloc);
                Errln("GetWordInstance((UCultureInfo)null) did not throw NPE.");
            }
            catch (ArgumentNullException e) { /* OK */ }
        }

        /**
         * Test FilteredBreakIteratorBuilder newly introduced
         */
        [Test]
        public void TestFilteredBreakIteratorBuilder()
        {
            FilteredBreakIteratorBuilder builder;
            BreakIterator baseBI;
            BreakIterator filteredBI;

            String text = "In the meantime Mr. Weston arrived with his small ship, which he had now recovered. Capt. Gorges, who informed the Sgt. here that one purpose of his going east was to meet with Mr. Weston, took this opportunity to call him to account for some abuses he had to lay to his charge."; // (William Bradford, public domain. http://catalog.hathitrust.org/Record/008651224 ) - edited.
            String ABBR_MR = "Mr.";
            String ABBR_CAPT = "Capt.";

            {
                Logln("Constructing empty builder\n");
                builder = FilteredBreakIteratorBuilder.GetEmptyInstance();

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("en"));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                assertDefaultBreakBehavior(filteredBI, text);
            }

            {
                Logln("Constructing empty builder\n");
                builder = FilteredBreakIteratorBuilder.GetEmptyInstance();

                Logln("Adding Mr. as an exception\n");

                assertEquals("2.1 suppressBreakAfter", true, builder.SuppressBreakAfter(ABBR_MR));
                assertEquals("2.2 suppressBreakAfter", false, builder.SuppressBreakAfter(ABBR_MR));
                assertEquals("2.3 unsuppressBreakAfter", true, builder.UnsuppressBreakAfter(ABBR_MR));
                assertEquals("2.4 unsuppressBreakAfter", false, builder.UnsuppressBreakAfter(ABBR_MR));
                assertEquals("2.5 suppressBreakAfter", true, builder.SuppressBreakAfter(ABBR_MR));

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("en"));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                Logln("Testing:");
                filteredBI.SetText(text);
                assertEquals("2nd next", 84, filteredBI.Next());
                assertEquals("2nd next", 90, filteredBI.Next());
                assertEquals("2nd next", 278, filteredBI.Next());
                filteredBI.First();
            }


            {
                Logln("Constructing empty builder\n");
                builder = FilteredBreakIteratorBuilder.GetEmptyInstance();

                Logln("Adding Mr. and Capt as an exception\n");
                assertEquals("3.1 suppressBreakAfter", true, builder.SuppressBreakAfter(ABBR_MR));
                assertEquals("3.2 suppressBreakAfter", true, builder.SuppressBreakAfter(ABBR_CAPT));

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("en"));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                Logln("Testing:");
                filteredBI.SetText(text);
                assertEquals("3rd next", 84, filteredBI.Next());
                assertEquals("3rd next", 278, filteredBI.Next());
                filteredBI.First();
            }

            {
                Logln("Constructing English builder\n");
                builder = FilteredBreakIteratorBuilder.GetInstance(new UCultureInfo("en"));

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("en"));

                Logln("unsuppressing 'Capt'");
                assertEquals("1st suppressBreakAfter", true, builder.UnsuppressBreakAfter(ABBR_CAPT));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                if (filteredBI != null)
                {
                    Logln("Testing:");
                    filteredBI.SetText(text);
                    assertEquals("4th next", 84, filteredBI.Next());
                    assertEquals("4th next", 90, filteredBI.Next());
                    assertEquals("4th next", 278, filteredBI.Next());
                    filteredBI.First();
                }
            }

            {
                Logln("Constructing English builder\n");
                builder = FilteredBreakIteratorBuilder.GetInstance(new UCultureInfo("en"));

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("en"));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                if (filteredBI != null)
                {
                    assertEnglishBreakBehavior(filteredBI, text);
                }
            }

            {
                Logln("Constructing English @ss=standard\n");
                filteredBI = BreakIterator.GetSentenceInstance(UCultureInfo.GetCultureInfoByIetfLanguageTag("en-US-u-ss-standard"));

                if (filteredBI != null)
                {
                    assertEnglishBreakBehavior(filteredBI, text);
                }
            }

            {
                Logln("Constructing Afrikaans @ss=standard - should be == default\n");
                filteredBI = BreakIterator.GetSentenceInstance(UCultureInfo.GetCultureInfoByIetfLanguageTag("af-u-ss-standard"));

                assertDefaultBreakBehavior(filteredBI, text);
            }

            {
                Logln("Constructing Japanese @ss=standard - should be == default\n");
                filteredBI = BreakIterator.GetSentenceInstance(UCultureInfo.GetCultureInfoByIetfLanguageTag("ja-u-ss-standard"));

                assertDefaultBreakBehavior(filteredBI, text);
            }
            {
                Logln("Constructing tfg @ss=standard - should be == default\n");
                filteredBI = BreakIterator.GetSentenceInstance(UCultureInfo.GetCultureInfoByIetfLanguageTag("tfg-u-ss-standard"));

                assertDefaultBreakBehavior(filteredBI, text);
            }

            {
                Logln("Constructing French builder");
                builder = FilteredBreakIteratorBuilder.GetInstance(new UCultureInfo("fr"));

                Logln("Constructing base BI\n");
                baseBI = BreakIterator.GetSentenceInstance(new CultureInfo("fr"));

                Logln("Building new BI\n");
                filteredBI = builder.WrapIteratorWithFilter(baseBI);

                if (filteredBI != null)
                {
                    assertFrenchBreakBehavior(filteredBI, text);
                }
            }
        }

        /**
         * @param filteredBI
         * @param text
         */
        private void assertFrenchBreakBehavior(BreakIterator filteredBI, String text)
        {
            Logln("Testing French behavior:");
            filteredBI.SetText(text);
            assertEquals("6th next", 20, filteredBI.Next());
            assertEquals("6th next", 84, filteredBI.Next());
            filteredBI.First();
        }

        /**
         * @param filteredBI
         * @param text
         */
        private void assertEnglishBreakBehavior(BreakIterator filteredBI, String text)
        {
            Logln("Testing English filtered behavior:");
            filteredBI.SetText(text);

            assertEquals("5th next", 84, filteredBI.Next());
            assertEquals("5th next", 278, filteredBI.Next());
            filteredBI.First();
        }

        /**
         * @param filteredBI
         * @param text
         */
        private void assertDefaultBreakBehavior(BreakIterator filteredBI, String text)
        {
            Logln("Testing Default Behavior:");
            filteredBI.SetText(text);
            assertEquals("1st next", 20, filteredBI.Next());
            assertEquals("1st next", 84, filteredBI.Next());
            assertEquals("1st next", 90, filteredBI.Next());
            assertEquals("1st next", 181, filteredBI.Next());
            assertEquals("1st next", 278, filteredBI.Next());
            filteredBI.First();
        }




        // ICU4N specific - test for concurrency problems with dictionary-based
        // breakiterators
        [Test]
        public void TestConcurrency()
        {
            int numThreads = 8;
            char[] chars = new char[] {
                        (char)4160,
                        (char)4124,
                        (char)4097,
                        (char)4177,
                        (char)4113,
                        (char)32,
                        (char)10671,
                    };
            string contents = new string(chars);

            var proto = BreakIterator.GetWordInstance(new UCultureInfo("th"));

            var iter = (BreakIterator)proto.Clone();
            iter.SetText(contents);
            int br;
            var breaks = new List<int>();
            while ((br = iter.Next()) != BreakIterator.Done)
            {
                breaks.Add(br);
            }



            CountdownEvent startingGun = new CountdownEvent(1);
            ThreadAnonymousHelper[] threads = new ThreadAnonymousHelper[numThreads];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new ThreadAnonymousHelper(startingGun, proto, contents, breaks.ToArray());

                threads[i].Start();
            }
            startingGun.Signal();
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
        }

        private class ThreadAnonymousHelper : ThreadJob
        {
            private readonly CountdownEvent startingGun;
            private readonly BreakIterator proto;
            private readonly string contents;
            private readonly int[] expected;

            public ThreadAnonymousHelper(CountdownEvent startingGun, BreakIterator proto, string contents, int[] expected)
            {
                this.startingGun = startingGun;
                this.proto = proto;
                this.contents = contents;
                this.expected = expected;
            }

            public override void Run()
            {
                try
                {
                    startingGun.Wait();
                    //long tokenCount = 0;

                    var iter = (BreakIterator)proto.Clone();

                    //string contents = "英 เบียร์ ビール ເບຍ abc";
                    for (int i = 0; i < 1000; i++)
                    {
                        
                        iter.SetText(contents);
                        int br;
                        var actual = new List<int>();
                        while ((br = iter.Next()) != BreakIterator.Done)
                        {
                            actual.Add(br);
                        }

                        Assert.IsTrue(J2N.Collections.ArrayEqualityComparer<int>.OneDimensional.Equals(expected, actual.ToArray()));


                        //Tokenizer tokenizer = new ICUTokenizer(new StringReader(contents));
                        //tokenizer.Reset();
                        //while (tokenizer.IncrementToken())
                        //{
                        //    tokenCount++;
                        //}
                        //tokenizer.End();

                        //if (Verbose)
                        //{
                        //    System.Console.Out.WriteLine(tokenCount);
                        //}
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }
            }
        }
    }
}
