using ICU4N.Support.Text;
using ICU4N.TestFramework.Dev.Test;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ICU4N.Tests.Dev.Test.Rbbi
{
    /// <summary>
    /// API Test the RuleBasedBreakIterator class
    /// </summary>
    public class RBBIAPITest : TestFmwk
    {
        /**
     * Tests clone() and equals() methods of RuleBasedBreakIterator
     **/
        [Test]
        public void TestCloneEquals()
        {
            RuleBasedBreakIterator bi1 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator biequal = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator bi3 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator bi2 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);

            string testString = "Testing word break iterators's clone() and equals()";
            bi1.SetText(testString);
            bi2.SetText(testString);
            biequal.SetText(testString);

            bi3.SetText("hello");
            Logln("Testing equals()");
            Logln("Testing == and !=");
            if (!bi1.Equals(biequal) || bi1.Equals(bi2) || bi1.Equals(bi3))
                Errln("ERROR:1 RBBI's == and !- operator failed.");
            if (bi2.Equals(biequal) || bi2.Equals(bi1) || biequal.Equals(bi3))
                Errln("ERROR:2 RBBI's == and != operator  failed.");
            Logln("Testing clone()");
            RuleBasedBreakIterator bi1clone = (RuleBasedBreakIterator)bi1.Clone();
            RuleBasedBreakIterator bi2clone = (RuleBasedBreakIterator)bi2.Clone();
            if (!bi1clone.Equals(bi1)
                || !bi1clone.Equals(biequal)
                || bi1clone.Equals(bi3)
                || bi1clone.Equals(bi2))
                Errln("ERROR:1 RBBI's clone() method failed");

            if (bi2clone.Equals(bi1)
                || bi2clone.Equals(biequal)
                || bi2clone.Equals(bi3)
                || !bi2clone.Equals(bi2))
                Errln("ERROR:2 RBBI's clone() method failed");

            if (!bi1.GetText().Equals(bi1clone.GetText())
                || !bi2clone.GetText().Equals(bi2.GetText())
                || bi2clone.Equals(bi1clone))
                Errln("ERROR: RBBI's clone() method failed");
        }

        /**
         * Tests toString() method of RuleBasedBreakIterator
         **/
        [Test]
        public void TestToString()
        {
            RuleBasedBreakIterator bi1 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator bi2 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);
            Logln("Testing toString()");
            bi1.SetText("Hello there");
            RuleBasedBreakIterator bi3 = (RuleBasedBreakIterator)bi1.Clone();
            String temp = bi1.ToString();
            String temp2 = bi2.ToString();
            String temp3 = bi3.ToString();
            if (temp2.Equals(temp3) || temp.Equals(temp2) || !temp.Equals(temp3))
                Errln("ERROR: error in toString() method");
        }

        /**
         * Tests the method hashCode() of RuleBasedBreakIterator
         **/
        [Test]
        public void TestHashCode()
        {
            RuleBasedBreakIterator bi1 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator bi3 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            RuleBasedBreakIterator bi2 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);
            Logln("Testing hashCode()");
            bi1.SetText("Hash code");
            bi2.SetText("Hash code");
            bi3.SetText("Hash code");
            RuleBasedBreakIterator bi1clone = (RuleBasedBreakIterator)bi1.Clone();
            RuleBasedBreakIterator bi2clone = (RuleBasedBreakIterator)bi2.Clone();
            if (bi1.GetHashCode() != bi1clone.GetHashCode()
                || bi1.GetHashCode() != bi3.GetHashCode()
                || bi1clone.GetHashCode() != bi3.GetHashCode()
                || bi2.GetHashCode() != bi2clone.GetHashCode())
                Errln("ERROR: identical objects have different hashcodes");

            if (bi1.GetHashCode() == bi2.GetHashCode()
                || bi2.GetHashCode() == bi3.GetHashCode()
                || bi1clone.GetHashCode() == bi2clone.GetHashCode()
                || bi1clone.GetHashCode() == bi2.GetHashCode())
                Errln("ERROR: different objects have same hashcodes");
        }

        /**
          * Tests the methods getText() and setText() of RuleBasedBreakIterator
          **/
        [Test]
        public void TestGetSetText()
        {
            Logln("Testing getText setText ");
            String str1 = "first string.";
            String str2 = "Second string.";
            //RuleBasedBreakIterator charIter1 = (RuleBasedBreakIterator) BreakIterator.getCharacterInstance(Locale.getDefault());
            RuleBasedBreakIterator wordIter1 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);
            CharacterIterator text1 = new StringCharacterIterator(str1);
            //CharacterIterator text1Clone = (CharacterIterator) text1.Clone();
            //CharacterIterator text2 = new StringCharacterIterator(str2);
            wordIter1.SetText(str1);
            if (!wordIter1.GetText().Equals(text1))
                Errln("ERROR:1 error in setText or getText ");
            if (wordIter1.Current != 0)
                Errln("ERROR:1 setText did not set the iteration position to the beginning of the text, it is"
                       + wordIter1.Current + "\n");
            wordIter1.Next(2);
            wordIter1.SetText(str2);
            if (wordIter1.Current != 0)
                Errln("ERROR:2 setText did not reset the iteration position to the beginning of the text, it is"
                        + wordIter1.Current + "\n");

            // Test the CharSequence overload of setText() for a simple case.
            BreakIterator lineIter = BreakIterator.GetLineInstance(new CultureInfo("en"));
            ICharSequence csText = "Hello, World. ".ToCharSequence();
            // Expected Line Brks  ^      ^      ^
            //                     0123456789012345
            List<int> expected = new List<int>();
            expected.Add(0); expected.Add(7); expected.Add(14);
            lineIter.SetText(csText);
            for (int pos = lineIter.First(); pos != BreakIterator.DONE; pos = lineIter.Next())
            {
                assertTrue("", expected.Contains(pos));
            }
            assertEquals("", csText.Length, lineIter.Current);
        }

        /**
          * Testing the methods first(), next(), next(int) and following() of RuleBasedBreakIterator
          *   TODO:  Most of this test should be retired, rule behavior is much better covered by
          *          TestExtended, which is also easier to understand and maintain.
          **/
        [Test]
        public void TestFirstNextFollowing()
        {
            int p, q;
            String testString = "This is a word break. Isn't it? 2.25";
            Logln("Testing first() and next(), following() with custom rules");
            Logln("testing word iterator - string :- \"" + testString + "\"\n");
            RuleBasedBreakIterator wordIter1 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(CultureInfo.CurrentCulture);
            wordIter1.SetText(testString);
            p = wordIter1.First();
            if (p != 0)
                Errln("ERROR: first() returned" + p + "instead of 0");
            q = wordIter1.Next(9);
            doTest(testString, p, q, 20, "This is a word break");
            p = q;
            q = wordIter1.Next();
            doTest(testString, p, q, 21, ".");
            p = q;
            q = wordIter1.Next(3);
            doTest(testString, p, q, 28, " Isn't ");
            p = q;
            q = wordIter1.Next(2);
            doTest(testString, p, q, 31, "it?");
            q = wordIter1.Following(2);
            doTest(testString, 2, q, 4, "is");
            q = wordIter1.Following(22);
            doTest(testString, 22, q, 27, "Isn't");
            wordIter1.Last();
            p = wordIter1.Next();
            q = wordIter1.Following(wordIter1.Last());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: next()/following() at last position returned #"
                        + p + " and " + q + " instead of" + testString.Length + "\n");
            RuleBasedBreakIterator charIter1 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(CultureInfo.CurrentCulture);
            testString = "Write hindi here. ";
            Logln("testing char iter - string:- \"" + testString + "\"");
            charIter1.SetText(testString);
            p = charIter1.First();
            if (p != 0)
                Errln("ERROR: first() returned" + p + "instead of 0");
            q = charIter1.Next();
            doTest(testString, p, q, 1, "W");
            p = q;
            q = charIter1.Next(4);
            doTest(testString, p, q, 5, "rite");
            p = q;
            q = charIter1.Next(12);
            doTest(testString, p, q, 17, " hindi here.");
            p = q;
            q = charIter1.Next(-6);
            doTest(testString, p, q, 11, " here.");
            p = q;
            q = charIter1.Next(6);
            doTest(testString, p, q, 17, " here.");
            p = charIter1.Following(charIter1.Last());
            q = charIter1.Next(charIter1.Last());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: following()/next() at last position returned #"
                        + p + " and " + q + " instead of" + testString.Length);
            testString = "Hello! how are you? I'am fine. Thankyou. How are you doing? This  costs $20,00,000.";
            RuleBasedBreakIterator sentIter1 = (RuleBasedBreakIterator)BreakIterator.GetSentenceInstance(CultureInfo.CurrentCulture);
            Logln("testing sentence iter - String:- \"" + testString + "\"");
            sentIter1.SetText(testString);
            p = sentIter1.First();
            if (p != 0)
                Errln("ERROR: first() returned" + p + "instead of 0");
            q = sentIter1.Next();
            doTest(testString, p, q, 7, "Hello! ");
            p = q;
            q = sentIter1.Next(2);
            doTest(testString, p, q, 31, "how are you? I'am fine. ");
            p = q;
            q = sentIter1.Next(-2);
            doTest(testString, p, q, 7, "how are you? I'am fine. ");
            p = q;
            q = sentIter1.Next(4);
            doTest(testString, p, q, 60, "how are you? I'am fine. Thankyou. How are you doing? ");
            p = q;
            q = sentIter1.Next();
            doTest(testString, p, q, 83, "This  costs $20,00,000.");
            q = sentIter1.Following(1);
            doTest(testString, 1, q, 7, "ello! ");
            q = sentIter1.Following(10);
            doTest(testString, 10, q, 20, " are you? ");
            q = sentIter1.Following(20);
            doTest(testString, 20, q, 31, "I'am fine. ");
            p = sentIter1.Following(sentIter1.Last());
            q = sentIter1.Next(sentIter1.Last());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: following()/next() at last position returned #"
                        + p + " and " + q + " instead of" + testString.Length);
            testString = "Hello! how\r\n (are)\r you? I'am fine- Thankyou. foo\u00a0bar How, are, you? This, costs $20,00,000.";
            Logln("(UnicodeString)testing line iter - String:- \"" + testString + "\"");
            RuleBasedBreakIterator lineIter1 = (RuleBasedBreakIterator)BreakIterator.GetLineInstance(CultureInfo.CurrentCulture);
            lineIter1.SetText(testString);
            p = lineIter1.First();
            if (p != 0)
                Errln("ERROR: first() returned" + p + "instead of 0");
            q = lineIter1.Next();
            doTest(testString, p, q, 7, "Hello! ");
            p = q;
            p = q;
            q = lineIter1.Next(4);
            doTest(testString, p, q, 20, "how\r\n (are)\r ");
            p = q;
            q = lineIter1.Next(-4);
            doTest(testString, p, q, 7, "how\r\n (are)\r ");
            p = q;
            q = lineIter1.Next(6);
            doTest(testString, p, q, 30, "how\r\n (are)\r you? I'am ");
            p = q;
            q = lineIter1.Next();
            doTest(testString, p, q, 36, "fine- ");
            p = q;
            q = lineIter1.Next(2);
            doTest(testString, p, q, 54, "Thankyou. foo\u00a0bar ");
            q = lineIter1.Following(60);
            doTest(testString, 60, q, 64, "re, ");
            q = lineIter1.Following(1);
            doTest(testString, 1, q, 7, "ello! ");
            q = lineIter1.Following(10);
            doTest(testString, 10, q, 12, "\r\n");
            q = lineIter1.Following(20);
            doTest(testString, 20, q, 25, "you? ");
            p = lineIter1.Following(lineIter1.Last());
            q = lineIter1.Next(lineIter1.Last());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: following()/next() at last position returned #"
                        + p + " and " + q + " instead of" + testString.Length);
        }

        /**
         * Testing the methods last(), previous(), and preceding() of RuleBasedBreakIterator
         **/
        [Test]
        public void TestLastPreviousPreceding()
        {
            int p, q;
            String testString = "This is a word break. Isn't it? 2.25 dollars";
            Logln("Testing last(),previous(), preceding() with custom rules");
            Logln("testing word iteration for string \"" + testString + "\"");
            RuleBasedBreakIterator wordIter1 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(new CultureInfo("en"));
            wordIter1.SetText(testString);
            p = wordIter1.Last();
            if (p != testString.Length)
            {
                Errln("ERROR: last() returned" + p + "instead of" + testString.Length);
            }
            q = wordIter1.Previous();
            doTest(testString, p, q, 37, "dollars");
            p = q;
            q = wordIter1.Previous();
            doTest(testString, p, q, 36, " ");
            q = wordIter1.Preceding(25);
            doTest(testString, 25, q, 22, "Isn");
            p = q;
            q = wordIter1.Previous();
            doTest(testString, p, q, 21, " ");
            q = wordIter1.Preceding(20);
            doTest(testString, 20, q, 15, "break");
            p = wordIter1.Preceding(wordIter1.First());
            if (p != BreakIterator.DONE)
                Errln("ERROR: preceding()  at starting position returned #" + p + " instead of 0");
            testString = "Hello! how are you? I'am fine. Thankyou. How are you doing? This  costs $20,00,000.";
            Logln("testing sentence iter - String:- \"" + testString + "\"");
            RuleBasedBreakIterator sentIter1 = (RuleBasedBreakIterator)BreakIterator.GetSentenceInstance(CultureInfo.CurrentCulture);
            sentIter1.SetText(testString);
            p = sentIter1.Last();
            if (p != testString.Length)
                Errln("ERROR: last() returned" + p + "instead of " + testString.Length);
            q = sentIter1.Previous();
            doTest(testString, p, q, 60, "This  costs $20,00,000.");
            p = q;
            q = sentIter1.Previous();
            doTest(testString, p, q, 41, "How are you doing? ");
            q = sentIter1.Preceding(40);
            doTest(testString, 40, q, 31, "Thankyou.");
            q = sentIter1.Preceding(25);
            doTest(testString, 25, q, 20, "I'am ");
            sentIter1.First();
            p = sentIter1.Previous();
            q = sentIter1.Preceding(sentIter1.First());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: previous()/preceding() at starting position returned #"
                        + p + " and " + q + " instead of 0\n");
            testString = "Hello! how are you? I'am fine. Thankyou. How are you doing? This\n costs $20,00,000.";
            Logln("testing line iter - String:- \"" + testString + "\"");
            RuleBasedBreakIterator lineIter1 = (RuleBasedBreakIterator)BreakIterator.GetLineInstance(CultureInfo.CurrentCulture);
            lineIter1.SetText(testString);
            p = lineIter1.Last();
            if (p != testString.Length)
                Errln("ERROR: last() returned" + p + "instead of " + testString.Length);
            q = lineIter1.Previous();
            doTest(testString, p, q, 72, "$20,00,000.");
            p = q;
            q = lineIter1.Previous();
            doTest(testString, p, q, 66, "costs ");
            q = lineIter1.Preceding(40);
            doTest(testString, 40, q, 31, "Thankyou.");
            q = lineIter1.Preceding(25);
            doTest(testString, 25, q, 20, "I'am ");
            lineIter1.First();
            p = lineIter1.Previous();
            q = lineIter1.Preceding(sentIter1.First());
            if (p != BreakIterator.DONE || q != BreakIterator.DONE)
                Errln("ERROR: previous()/preceding() at starting position returned #"
                        + p + " and " + q + " instead of 0\n");
        }

        /**
         * Tests the method IsBoundary() of RuleBasedBreakIterator
         **/
        [Test]
        public void TestIsBoundary()
        {
            String testString1 = "Write here. \u092d\u0301\u0930\u0924 \u0938\u0941\u0902\u0926\u0930 a\u0301u";
            RuleBasedBreakIterator charIter1 = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance(new CultureInfo("en"));
            charIter1.SetText(testString1);
            int[] bounds1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 16, 17, 20, 21, 22, 23, 25, 26 };
            doBoundaryTest(charIter1, testString1, bounds1);
            RuleBasedBreakIterator wordIter2 = (RuleBasedBreakIterator)BreakIterator.GetWordInstance(new CultureInfo("en"));
            wordIter2.SetText(testString1);
            int[] bounds2 = { 0, 5, 6, 10, 11, 12, 16, 17, 22, 23, 26 };
            doBoundaryTest(wordIter2, testString1, bounds2);
        }

        /**
         *  Tests the rule status return value constants
         */
        [Test]
        public void TestRuleStatus()
        {
            BreakIterator bi = BreakIterator.GetWordInstance(ULocale.ENGLISH);

            bi.SetText("# ");
            assertEquals(null, bi.Next(), 1);
            assertTrue(null, bi.RuleStatus >= RuleBasedBreakIterator.WORD_NONE);
            assertTrue(null, bi.RuleStatus < RuleBasedBreakIterator.WORD_NONE_LIMIT);

            bi.SetText("3 ");
            assertEquals(null, bi.Next(), 1);
            assertTrue(null, bi.RuleStatus >= RuleBasedBreakIterator.WORD_NUMBER);
            assertTrue(null, bi.RuleStatus < RuleBasedBreakIterator.WORD_NUMBER_LIMIT);

            bi.SetText("a ");
            assertEquals(null, bi.Next(), 1);
            assertTrue(null, bi.RuleStatus >= RuleBasedBreakIterator.WORD_LETTER);
            assertTrue(null, bi.RuleStatus < RuleBasedBreakIterator.WORD_LETTER_LIMIT);


            bi.SetText("イ  ");
            assertEquals(null, bi.Next(), 1);
            assertTrue(null, bi.RuleStatus >= RuleBasedBreakIterator.WORD_KANA);
            // TODO: ticket #10261, Kana is not returning the correct status.
            // assertTrue(null, bi.getRuleStatus() < RuleBasedBreakIterator.WORD_KANA_LIMIT);
            // System.out.println("\n" + bi.getRuleStatus());

            bi.SetText("退 ");
            assertEquals(null, bi.Next(), 1);
            assertTrue(null, bi.RuleStatus >= RuleBasedBreakIterator.WORD_IDEO);
            assertTrue(null, bi.RuleStatus < RuleBasedBreakIterator.WORD_IDEO_LIMIT);
        }

        /**
         *  Tests the rule dump debug function.
         */
        [Test]
        public void TestRuledump()
        {
            RuleBasedBreakIterator bi = (RuleBasedBreakIterator)BreakIterator.GetCharacterInstance();
            MemoryStream bos = new MemoryStream();
            TextWriter @out = new StreamWriter(bos);
            bi.Dump(@out);
            assertTrue(null, bos.Length > 100);
        }

        //---------------------------------------------
        //Internal subroutines
        //---------------------------------------------

        /* Internal subroutine used by TestIsBoundary() */
        private void doBoundaryTest(BreakIterator bi, String text, int[] boundaries)
        {
            Logln("testIsBoundary():");
            int p = 0;
            bool isB;
            for (int i = 0; i < text.Length; i++)
            {
                isB = bi.IsBoundary(i);
                Logln("bi.isBoundary(" + i + ") -> " + isB);
                if (i == boundaries[p])
                {
                    if (!isB)
                        Errln("Wrong result from isBoundary() for " + i + ": expected true, got false");
                    p++;
                }
                else
                {
                    if (isB)
                        Errln("Wrong result from isBoundary() for " + i + ": expected false, got true");
                }
            }
        }

        /*Internal subroutine used for comparison of expected and acquired results */
        private void doTest(String testString, int start, int gotoffset, int expectedOffset, String expectedString)
        {
            String selected;
            String expected = expectedString;
            if (gotoffset != expectedOffset)
                Errln("ERROR:****returned #" + gotoffset + " instead of #" + expectedOffset);
            if (start <= gotoffset)
            {
                selected = testString.Substring(start, gotoffset - start); // ICU4N: corrected 2nd parameter
            }
            else
            {
                selected = testString.Substring(gotoffset, start - gotoffset); // ICU4N: corrected 2nd parameter
            }
            if (!selected.Equals(expected))
                Errln("ERROR:****selected \"" + selected + "\" instead of \"" + expected + "\"");
            else
                Logln("****selected \"" + selected + "\"");
        }

        [Test]
        public void TestGetTitleInstance()
        {
            BreakIterator bi = BreakIterator.GetTitleInstance(new CultureInfo("en-CA"));
            TestFmwk.assertNotEquals("Title instance break iterator not correctly instantiated", bi.First(), null);
            bi.SetText("Here is some Text");
            TestFmwk.assertEquals("Title instance break iterator not correctly instantiated", bi.First(), 0);
        }
    }
}
