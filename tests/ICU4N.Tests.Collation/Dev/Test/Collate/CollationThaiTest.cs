using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

//
// Port From:   ICU4C v2.1 : collate/CollationRegressionTest
// Source File: $ICU4CRoot/source/test/intltest/regcoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationThaiTest : TestFmwk
    {
        internal readonly int MAX_FAILURES_TO_SHOW = -1;

        /**
         * Odd corner conditions taken from "How to Sort Thai Without Rewriting Sort",
         * by Doug Cooper, http://seasrc.th.net/paper/thaisort.zip
         */
        [Test]
        public void TestCornerCases()
        {
            String[] TESTS = {
            // Shorter words precede longer
            "\u0e01",                               "<",    "\u0e01\u0e01",

            // Tone marks are considered after letters (i.e. are primary ignorable)
            "\u0e01\u0e32",                        "<",    "\u0e01\u0e49\u0e32",

            // ditto for other over-marks
            "\u0e01\u0e32",                        "<",    "\u0e01\u0e32\u0e4c",

            // commonly used mark-in-context order.
            // In effect, marks are sorted after each syllable.
            "\u0e01\u0e32\u0e01\u0e49\u0e32",   "<",    "\u0e01\u0e48\u0e32\u0e01\u0e49\u0e32",

            // Hyphens and other punctuation follow whitespace but come before letters
            "\u0e01\u0e32",                        "=",    "\u0e01\u0e32-",
            "\u0e01\u0e32-",                       "<",    "\u0e01\u0e32\u0e01\u0e32",

            // Doubler follows an indentical word without the doubler
            "\u0e01\u0e32",                        "=",    "\u0e01\u0e32\u0e46",
            "\u0e01\u0e32\u0e46",                 "<",    "\u0e01\u0e32\u0e01\u0e32",

            // \u0e45 after either \u0e24 or \u0e26 is treated as a single
            // combining character, similar to "c < ch" in traditional spanish.
            // TODO: beef up this case
            "\u0e24\u0e29\u0e35",                 "<",    "\u0e24\u0e45\u0e29\u0e35",
            "\u0e26\u0e29\u0e35",                 "<",    "\u0e26\u0e45\u0e29\u0e35",

            // Vowels reorder, should compare \u0e2d and \u0e34
            "\u0e40\u0e01\u0e2d",                 "<",    "\u0e40\u0e01\u0e34",

            // Tones are compared after the rest of the word (e.g. primary ignorable)
            "\u0e01\u0e32\u0e01\u0e48\u0e32",   "<",    "\u0e01\u0e49\u0e32\u0e01\u0e32",

            // Periods are ignored entirely
            "\u0e01.\u0e01.",                      "<",    "\u0e01\u0e32",
        };

            RuleBasedCollator coll = null;
            try
            {
                coll = GetThaiCollator();
            }
            catch (Exception e)
            {
                Warnln("could not construct Thai collator");
                return;
            }
            CompareArray(coll, TESTS);
        }

        internal void CompareArray(RuleBasedCollator c, String[] tests)
        {
            for (int i = 0; i < tests.Length; i += 3)
            {
                int expect = 0;
                if (tests[i + 1].Equals("<"))
                {
                    expect = -1;
                }
                else if (tests[i + 1].Equals(">"))
                {
                    expect = 1;
                }
                else if (tests[i + 1].Equals("="))
                {
                    expect = 0;
                }
                else
                {
                    // expect = Integer.decode(tests[i+1]).intValue();
                    Errln("Error: unknown operator " + tests[i + 1]);
                    return;
                }
                String s1 = tests[i];
                String s2 = tests[i + 2];
                CollationTest.DoTest(this, c, s1, s2, expect);
            }
        }

        internal int Sign(int i)
        {
            if (i < 0) return -1;
            if (i > 0) return 1;
            return 0;
        }

        /**
         * Read the external dictionary file, which is already in proper
         * sorted order, and confirm that the collator compares each line as
         * preceding the following line.
         */
        [Test]
        public void TestDictionary()
        {
            RuleBasedCollator coll = null;
            try
            {
                coll = GetThaiCollator();
            }
            catch (Exception e)
            {
                Warnln("could not construct Thai collator");
                return;
            }

            // Read in a dictionary of Thai words
            int line = 0;
            int failed = 0;
            int wordCount = 0;
            TextReader @in = null;
            try
            {
                String fileName = "riwords.txt";
                @in = TestUtil.GetDataReader(fileName, "UTF-8");

                //
                // Loop through each word in the dictionary and compare it to the previous
                // word. They should be in sorted order.
                //
                String lastWord = "";
                String word = @in.ReadLine();
                while (word != null)
                {
                    line++;

                    // Skip comments and blank lines
                    if (word.Length == 0 || word[0] == 0x23)
                    {
                        word = @in.ReadLine();
                        continue;
                    }

                    // Show the first 8 words being compared, so we can see what's happening
                    ++wordCount;
                    if (wordCount <= 8)
                    {
                        Logln("Word " + wordCount + ": " + word);
                    }

                    if (lastWord.Length > 0)
                    {
                        // CollationTest.doTest isn't really set up to handle situations where
                        // the result can be equal or greater than the previous, so have to skip for now.
                        // Not a big deal, since we're still testing to make sure everything sorts out
                        // right, just not looking at the colation keys in detail...
                        // CollationTest.doTest(this, coll, lastWord, word, -1);
                        int result = coll.Compare(lastWord, word);

                        if (result > 0)
                        {
                            failed++;
                            if (MAX_FAILURES_TO_SHOW < 0 || failed <= MAX_FAILURES_TO_SHOW)
                            {
                                String msg = "--------------------------------------------\n" + line + " compare("
                                        + lastWord + ", " + word + ") returned " + result + ", expected -1\n";
                                CollationKey k1, k2;
                                k1 = coll.GetCollationKey(lastWord);
                                k2 = coll.GetCollationKey(word);
                                msg += "key1: " + CollationTest.Prettify(k1) + "\n" + "key2: " + CollationTest.Prettify(k2);
                                Errln(msg);
                            }
                        }
                    }
                    lastWord = word;
                    word = @in.ReadLine();
                }
            }
            catch (IOException e)
            {
                Errln("IOException " + e.ToString());
            }
            finally
            {
                if (@in == null)
                {
                    Errln("Error: could not open test file. Aborting test.");
                }
                else
                {
                    try
                    {
                        @in.Dispose();
                    }
                    catch (IOException ignored)
                    {
                    }
                }
            }

            // ICU4N: We can't return in a finally block, so we have to do it here under
            // the same condition
            if (@in == null)
            {
                return;
            }


            if (failed != 0)
            {
                if (failed > MAX_FAILURES_TO_SHOW)
                {
                    Errln("Too many failures; only the first " +
                          MAX_FAILURES_TO_SHOW + " failures were shown");
                }
                Errln("Summary: " + failed + " of " + (line - 1) +
                      " comparisons failed");
            }

            Logln("Words checked: " + wordCount);
        }

        [Test]
        public void TestInvalidThai()
        {
            String[] tests = { "\u0E44\u0E01\u0E44\u0E01",
                           "\u0E44\u0E01\u0E01\u0E44",
                           "\u0E01\u0E44\u0E01\u0E44",
                           "\u0E01\u0E01\u0E44\u0E44",
                           "\u0E44\u0E44\u0E01\u0E01",
                           "\u0E01\u0E44\u0E44\u0E01",
                         };

            RuleBasedCollator collator;
            StrCmp comparator;
            try
            {
                collator = GetThaiCollator();
                comparator = new StrCmp();
            }
            catch (Exception e)
            {
                Warnln("could not construct Thai collator");
                return;
            }

            Array.Sort(tests, comparator);

            for (int i = 0; i < tests.Length; i++)
            {
                for (int j = i + 1; j < tests.Length; j++)
                {
                    if (collator.Compare(tests[i], tests[j]) > 0)
                    {
                        // inconsistency ordering found!
                        Errln("Inconsistent ordering between strings " + i
                              + " and " + j);
                    }
                }
                CollationElementIterator iterator
                    = collator.GetCollationElementIterator(tests[i]);
                CollationTest.BackAndForth(this, iterator);
            }
        }

        [Test]
        public void TestReordering()
        {
            String[] tests = {
                "\u0E41c\u0301",      "=", "\u0E41\u0107", // composition
                "\u0E41\uD835\uDFCE", "<", "\u0E41\uD835\uDFCF", // supplementaries
                "\u0E41\uD834\uDD5F", "=", "\u0E41\uD834\uDD58\uD834\uDD65", // supplementary composition decomps to supplementary
                "\u0E41\uD87E\uDC02", "=", "\u0E41\u4E41", // supplementary composition decomps to BMP
                "\u0E41\u0301",       "=", "\u0E41\u0301", // unsafe (just checking backwards iteration)
                "\u0E41\u0301\u0316", "=", "\u0E41\u0316\u0301",

                "abc\u0E41c\u0301",      "=", "abc\u0E41\u0107", // composition
                "abc\u0E41\uD834\uDC00", "<", "abc\u0E41\uD834\uDC01", // supplementaries
                "abc\u0E41\uD834\uDD5F", "=", "abc\u0E41\uD834\uDD58\uD834\uDD65", // supplementary composition decomps to supplementary
                "abc\u0E41\uD87E\uDC02", "=", "abc\u0E41\u4E41", // supplementary composition decomps to BMP
                "abc\u0E41\u0301",       "=", "abc\u0E41\u0301", // unsafe (just checking backwards iteration)
                "abc\u0E41\u0301\u0316", "=", "abc\u0E41\u0316\u0301",

                "\u0E41c\u0301abc",      "=", "\u0E41\u0107abc", // composition
                "\u0E41\uD834\uDC00abc", "<", "\u0E41\uD834\uDC01abc", // supplementaries
                "\u0E41\uD834\uDD5Fabc", "=", "\u0E41\uD834\uDD58\uD834\uDD65abc", // supplementary composition decomps to supplementary
                "\u0E41\uD87E\uDC02abc", "=", "\u0E41\u4E41abc", // supplementary composition decomps to BMP
                "\u0E41\u0301abc",       "=", "\u0E41\u0301abc", // unsafe (just checking backwards iteration)
                "\u0E41\u0301\u0316abc", "=", "\u0E41\u0316\u0301abc",

                "abc\u0E41c\u0301abc",      "=", "abc\u0E41\u0107abc", // composition
                "abc\u0E41\uD834\uDC00abc", "<", "abc\u0E41\uD834\uDC01abc", // supplementaries
                "abc\u0E41\uD834\uDD5Fabc", "=", "abc\u0E41\uD834\uDD58\uD834\uDD65abc", // supplementary composition decomps to supplementary
                "abc\u0E41\uD87E\uDC02abc", "=", "abc\u0E41\u4E41abc", // supplementary composition decomps to BMP
                "abc\u0E41\u0301abc",       "=", "abc\u0E41\u0301abc", // unsafe (just checking backwards iteration)
                "abc\u0E41\u0301\u0316abc", "=", "abc\u0E41\u0316\u0301abc",
            };

            RuleBasedCollator collator;
            try
            {
                collator = GetThaiCollator();
            }
            catch (Exception e)
            {
                Warnln("could not construct Thai collator");
                return;
            }
            CompareArray(collator, tests);

            String rule = "& c < ab";
            String[] testcontraction = { "\u0E41ab", ">", "\u0E41c" };
            try
            {
                collator = new RuleBasedCollator(rule);
            }
            catch (Exception e)
            {
                Errln("Error: could not construct collator with rule " + rule);
                return;
            }
            CompareArray(collator, testcontraction);
        }

        // private inner class -------------------------------------------------

        private sealed class StrCmp : IComparer<String>
        {
            public int Compare(String string1, String string2)
            {
                return collator.Compare(string1, string2);
            }

            internal StrCmp()
            {
                collator = GetThaiCollator();
            }

            Collator collator;
        }

        // private data members ------------------------------------------------

        private static RuleBasedCollator m_collator_;

        // private methods -----------------------------------------------------

        private static RuleBasedCollator GetThaiCollator()
        {
            if (m_collator_ == null)
            {
                m_collator_ = (RuleBasedCollator)Collator.GetInstance(
                                                    new CultureInfo("th-TH"));
            }
            return m_collator_;
        }
    }
}
