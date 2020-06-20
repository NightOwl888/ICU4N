using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using J2N;
using J2N.Text;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace ICU4N.Dev.Test.Rbbi
{
    /// <summary>
    /// Rule based break iterator data driven test.
    ///      Perform the tests from the file rbbitst.txt.
    ///      The test data file is common to both ICU4C and ICU4J.
    ///      See the data file for a description of the tests.
    /// </summary>
    public class RBBITestExtended : TestFmwk
    {
        public RBBITestExtended()
        {
        }



        internal class TestParams
        {
            internal BreakIterator bi;
            internal StringBuilder dataToBreak = new StringBuilder();
            internal int[] expectedBreaks = new int[4000];
            internal int[] srcLine = new int[4000];
            internal int[] srcCol = new int[4000];
            internal UCultureInfo currentLocale = new UCultureInfo("en_US");
        }


        [Test]
        [Ignore("ICU4N TODO: Fix this")]
        public void TestExtended()
        {
            TestParams tp = new TestParams();


            //
            //  Open and read the test data file.
            //
            StringBuilder testFileBuf = new StringBuilder();
            Stream @is = null;
            try
            {
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
                Assembly assembly = typeof(RBBITestExtended).GetTypeInfo().Assembly;
#else
                Assembly assembly = typeof(RBBITestExtended).Assembly;
#endif
                @is = assembly.GetManifestResourceStream("ICU4N.Dev.Test.Rbbi.rbbitst.txt");
                if (@is == null)
                {
                    Errln("Could not open test data file rbbitst.txt");
                    return;
                }
                StreamReader isr = new StreamReader(@is, Encoding.UTF8);
                try
                {
                    int c;
                    int count = 0;
                    for (; ; )
                    {
                        c = isr.Read();
                        if (c < 0)
                        {
                            break;
                        }
                        count++;
                        if (c == 0xFEFF && count == 1)
                        {
                            // BOM in the test data file. Discard it.
                            continue;
                        }

                        testFileBuf.AppendCodePoint(c);
                    }
                }
                finally
                {
                    isr.Dispose();
                }
            }
            catch (IOException e)
            {
                Errln(e.ToString());
                try
                {
                    @is.Dispose();
                }
                catch (IOException ignored)
                {
                }
                return;
            }

            String testString = testFileBuf.ToString();


            const int PARSE_COMMENT = 1;
            const int PARSE_TAG = 2;
            const int PARSE_DATA = 3;
            const int PARSE_NUM = 4;
            const int PARSE_RULES = 5;

            int parseState = PARSE_TAG;

            int savedState = PARSE_TAG;

            int lineNum = 1;
            int colStart = 0;
            int column = 0;
            int charIdx = 0;
            int i;

            int tagValue = 0;       // The numeric value of a <nnn> tag.

            StringBuilder rules = new StringBuilder();     // Holds rules from a <rules> ... </rules> block
            int rulesFirstLine = 0;              // Line number of the start of current <rules> block

            int len = testString.Length;

            for (charIdx = 0; charIdx < len;)
            {
                int c = testString.CodePointAt(charIdx);
                charIdx++;
                if (c == '\r' && charIdx < len && testString[charIdx] == '\n')
                {
                    // treat CRLF as a unit
                    c = '\n';
                    charIdx++;
                }
                if (c == '\n' || c == '\r')
                {
                    lineNum++;
                    colStart = charIdx;
                }
                column = charIdx - colStart + 1;

                switch (parseState)
                {
                    case PARSE_COMMENT:
                        if (c == 0x0a || c == 0x0d)
                        {
                            parseState = savedState;
                        }
                        break;

                    case PARSE_TAG:
                        {
                            if (c == '#')
                            {
                                parseState = PARSE_COMMENT;
                                savedState = PARSE_TAG;
                                break;
                            }
                            if (UChar.IsWhiteSpace(c))
                            {
                                break;
                            }
                            if (testString.StartsWith("<word>", charIdx - 1, StringComparison.Ordinal))
                            {
                                tp.bi = BreakIterator.GetWordInstance(tp.currentLocale);
                                charIdx += 5;
                                break;
                            }
                            if (testString.StartsWith("<char>", charIdx - 1, StringComparison.Ordinal))
                            {
                                tp.bi = BreakIterator.GetCharacterInstance(tp.currentLocale);
                                charIdx += 5;
                                break;
                            }
                            if (testString.StartsWith("<line>", charIdx - 1, StringComparison.Ordinal))
                            {
                                tp.bi = BreakIterator.GetLineInstance(tp.currentLocale);
                                charIdx += 5;
                                break;
                            }
                            if (testString.StartsWith("<sent>", charIdx - 1, StringComparison.Ordinal))
                            {
                                tp.bi = BreakIterator.GetSentenceInstance(tp.currentLocale);
                                charIdx += 5;
                                break;
                            }
                            if (testString.StartsWith("<title>", charIdx - 1, StringComparison.Ordinal))
                            {
                                tp.bi = BreakIterator.GetTitleInstance(tp.currentLocale);
                                charIdx += 6;
                                break;
                            }
                            if (testString.StartsWith("<rules>", charIdx - 1, StringComparison.Ordinal) ||
                                    testString.StartsWith("<badrules>", charIdx - 1, StringComparison.Ordinal))
                            {
                                charIdx = testString.IndexOf('>', charIdx) + 1;
                                parseState = PARSE_RULES;
                                rules.Length = (0);
                                rulesFirstLine = lineNum;
                                break;
                            }

                            if (testString.StartsWith("<locale ", charIdx - 1, StringComparison.Ordinal))
                            {
                                int closeIndex = testString.IndexOf('>', charIdx);
                                if (closeIndex < 0)
                                {
                                    Errln("line" + lineNum + ": missing close on <locale  tag.");
                                    break;
                                }
                                String localeName = testString.Substring(charIdx + 6, closeIndex - (charIdx + 6)); // ICU4N: Corrected 2nd parameter
                                localeName = localeName.Trim();
                                tp.currentLocale = new UCultureInfo(localeName);
                                charIdx = closeIndex + 1;
                                break;
                            }
                            if (testString.StartsWith("<data>", charIdx - 1, StringComparison.Ordinal))
                            {
                                parseState = PARSE_DATA;
                                charIdx += 5;
                                tp.dataToBreak.Length = (0);
                                tp.expectedBreaks.Fill(0);
                                tp.srcCol.Fill(0);
                                tp.srcLine.Fill(0);
                                break;
                            }

                            Errln("line" + lineNum + ": Tag expected in test file.");
                            return;
                            //parseState = PARSE_COMMENT;
                            //savedState = PARSE_DATA;
                        }

                    case PARSE_RULES:
                        if (testString.StartsWith("</rules>", charIdx - 1, StringComparison.Ordinal))
                        {
                            charIdx += 7;
                            parseState = PARSE_TAG;
                            try
                            {
                                tp.bi = new RuleBasedBreakIterator(rules.ToString());
                            }
                            catch (ArgumentException e)
                            {
                                Errln(String.Format("rbbitst.txt:{0}  Error creating break iterator from rules.  {1}", lineNum, e));
                            }
                        }
                        else if (testString.StartsWith("</badrules>", charIdx - 1, StringComparison.Ordinal))
                        {
                            charIdx += 10;
                            parseState = PARSE_TAG;
                            bool goodRules = true;
                            try
                            {
                                new RuleBasedBreakIterator(rules.ToString());
                            }
                            catch (ArgumentException e)
                            {
                                goodRules = false;
                            }
                            if (goodRules)
                            {
                                Errln(String.Format(
                                        "rbbitst.txt:{0}  Expected, but did not get, a failure creating break iterator from rules.",
                                        lineNum));
                            }
                        }
                        else
                        {
                            rules.AppendCodePoint(c);
                        }
                        break;

                    case PARSE_DATA:
                        if (c == '•')
                        {
                            int breakIdx = tp.dataToBreak.Length;
                            tp.expectedBreaks[breakIdx] = -1;
                            tp.srcLine[breakIdx] = lineNum;
                            tp.srcCol[breakIdx] = column;
                            break;
                        }

                        if (testString.StartsWith("</data>", charIdx - 1, StringComparison.Ordinal))
                        {
                            // Add final entry to mappings from break location to source file position.
                            //  Need one extra because last break position returned is after the
                            //    last char in the data, not at the last char.
                            int idx = tp.dataToBreak.Length;
                            tp.srcLine[idx] = lineNum;
                            tp.srcCol[idx] = column;

                            parseState = PARSE_TAG;
                            charIdx += 6;

                            // RUN THE TEST!
                            executeTest(tp);
                            break;
                        }

                        if (testString.StartsWith("\\N{", charIdx - 1, StringComparison.Ordinal))
                        {
                            int nameEndIdx = testString.IndexOf('}', charIdx);
                            if (nameEndIdx == -1)
                            {
                                Errln("Error in named character in test file at line " + lineNum +
                                        ", col " + column);
                            }
                            // Named character, e.g. \N{COMBINING GRAVE ACCENT}
                            // Get the code point from the name and insert it into the test data.
                            String charName = testString.Substring(charIdx + 2, nameEndIdx - (charIdx + 2)); // ICU4N: Corrected 2nd parameter
                            c = UChar.GetCharFromName(charName);
                            if (c == -1)
                            {
                                Errln("Error in named character in test file at line " + lineNum +
                                        ", col " + column);
                            }
                            else
                            {
                                // Named code point was recognized.  Insert it
                                //   into the test data.
                                tp.dataToBreak.AppendCodePoint(c);
                                for (i = tp.dataToBreak.Length - 1; i >= 0 && tp.srcLine[i] == 0; i--)
                                {
                                    tp.srcLine[i] = lineNum;
                                    tp.srcCol[i] = column;
                                }

                            }
                            if (nameEndIdx > charIdx)
                            {
                                charIdx = nameEndIdx + 1;
                            }
                            break;
                        }

                        if (testString.StartsWith("<>", charIdx - 1, StringComparison.Ordinal))
                        {
                            charIdx++;
                            int breakIdx = tp.dataToBreak.Length;
                            tp.expectedBreaks[breakIdx] = -1;
                            tp.srcLine[breakIdx] = lineNum;
                            tp.srcCol[breakIdx] = column;
                            break;
                        }

                        if (c == '<')
                        {
                            tagValue = 0;
                            parseState = PARSE_NUM;
                            break;
                        }

                        if (c == '#' && column == 3)
                        {   // TODO:  why is column off so far?
                            parseState = PARSE_COMMENT;
                            savedState = PARSE_DATA;
                            break;
                        }

                        if (c == '\\')
                        {
                            // Check for \ at end of line, a line continuation.
                            //     Advance over (discard) the newline
                            int cp = testString.CodePointAt(charIdx);
                            if (cp == '\r' && charIdx < len && testString.CodePointAt(charIdx + 1) == '\n')
                            {
                                // We have a CR LF
                                //  Need an extra increment of the input ptr to move over both of them
                                charIdx++;
                            }
                            if (cp == '\n' || cp == '\r')
                            {
                                lineNum++;
                                column = 0;
                                charIdx++;
                                colStart = charIdx;
                                break;
                            }

                            // Let unescape handle the back slash.
                            int[] charIdxAr = new int[1];
                            charIdxAr[0] = charIdx;
                            cp = Utility.UnescapeAt(testString, charIdxAr);
                            if (cp != -1)
                            {
                                // Escape sequence was recognized.  Insert the char
                                //   into the test data.
                                charIdx = charIdxAr[0];
                                tp.dataToBreak.AppendCodePoint(cp);
                                for (i = tp.dataToBreak.Length - 1; i >= 0 && tp.srcLine[i] == 0; i--)
                                {
                                    tp.srcLine[i] = lineNum;
                                    tp.srcCol[i] = column;
                                }

                                break;
                            }


                            // Not a recognized backslash escape sequence.
                            // Take the next char as a literal.
                            //  TODO:  Should this be an error?
                            c = testString.CodePointAt(charIdx);
                            charIdx = testString.OffsetByCodePoints(charIdx, 1);
                        }

                        // Normal, non-escaped data char.
                        tp.dataToBreak.AppendCodePoint(c);

                        // Save the mapping from offset in the data to line/column numbers in
                        //   the original input file.  Will be used for better error messages only.
                        //   If there's an expected break before this char, the slot in the mapping
                        //     vector will already be set for this char; don't overwrite it.
                        for (i = tp.dataToBreak.Length - 1; i >= 0 && tp.srcLine[i] == 0; i--)
                        {
                            tp.srcLine[i] = lineNum;
                            tp.srcCol[i] = column;
                        }
                        break;


                    case PARSE_NUM:
                        // We are parsing an expected numeric tag value, like <1234>,
                        //   within a chunk of data.
                        if (UChar.IsWhiteSpace(c))
                        {
                            break;
                        }

                        if (c == '>')
                        {
                            // Finished the number.  Add the info to the expected break data,
                            //   and switch parse state back to doing plain data.
                            parseState = PARSE_DATA;
                            if (tagValue == 0)
                            {
                                tagValue = -1;
                            }
                            int breakIdx = tp.dataToBreak.Length;
                            tp.expectedBreaks[breakIdx] = tagValue;
                            tp.srcLine[breakIdx] = lineNum;
                            tp.srcCol[breakIdx] = column;
                            break;
                        }

                        if (UChar.IsDigit(c))
                        {
                            tagValue = tagValue * 10 + UChar.Digit(c);
                            break;
                        }

                        Errln(String.Format("Syntax Error in rbbitst.txt at line {0}, col {1}", lineNum, column));
                        return;
                }
            }

            // Reached end of test file. Raise an error if parseState indicates that we are
            //   within a block that should have been terminated.
            if (parseState == PARSE_RULES)
            {
                Errln(String.Format("rbbitst.txt:{0} <rules> block beginning at line {1} is not closed.",
                    lineNum, rulesFirstLine));
            }
            if (parseState == PARSE_DATA)
            {
                Errln(String.Format("rbbitst.txt:{0} <data> block not closed.", lineNum));
            }
        }

        void executeTest(TestParams t)
        {
            // TODO: also rerun tests with a break iterator re-created from bi.getRules()
            //       and from bi.clone(). If in exhaustive mode only.
            int bp;
            int prevBP;
            int i;

            if (t.bi == null)
            {
                return;
            }

            t.bi.SetText(t.dataToBreak.ToString());
            //
            //  Run the iterator forward
            //
            prevBP = -1;
            for (bp = t.bi.First(); bp != BreakIterator.Done; bp = t.bi.Next())
            {
                if (prevBP == bp)
                {
                    // Fail for lack of forward progress.
                    Errln("Forward Iteration, no forward progress.  Break Pos=" + bp +
                            "  File line,col=" + t.srcLine[bp] + ", " + t.srcCol[bp]);
                    break;
                }

                // Check that there were we didn't miss an expected break between the last one
                //  and this one.
                for (i = prevBP + 1; i < bp; i++)
                {
                    if (t.expectedBreaks[i] != 0)
                    {
                        Errln("Forward Iteration, break expected, but not found.  Pos=" + i +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i]);
                    }
                }

                // Check that the break we did find was expected
                if (t.expectedBreaks[bp] == 0)
                {
                    Errln("Forward Iteration, break found, but not expected.  Pos=" + bp +
                            "  File line,col= " + t.srcLine[bp] + ", " + t.srcCol[bp]);
                }
                else
                {
                    // The break was expected.
                    //   Check that the {nnn} tag value is correct.
                    int expectedTagVal = t.expectedBreaks[bp];
                    if (expectedTagVal == -1)
                    {
                        expectedTagVal = 0;
                    }
                    int line = t.srcLine[bp];
                    int rs = (int)t.bi.RuleStatus;
                    if (rs != expectedTagVal)
                    {
                        Errln("Incorrect status for forward break.  Pos = " + bp +
                                ".  File line,col = " + line + ", " + t.srcCol[bp] + "\n" +
                              "          Actual, Expected status = " + rs + ", " + expectedTagVal);
                    }
                    int[] fillInArray = new int[4];
                    int numStatusVals = t.bi.GetRuleStatusVec(fillInArray);
                    assertTrue("", numStatusVals >= 1);
                    assertEquals("", expectedTagVal, fillInArray[0]);
                }


                prevBP = bp;
            }

            // Verify that there were no missed expected breaks after the last one found
            for (i = prevBP + 1; i < t.dataToBreak.Length + 1; i++)
            {
                if (t.expectedBreaks[i] != 0)
                {
                    Errln("Forward Iteration, break expected, but not found.  Pos=" + i +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i]);
                }
            }


            //
            //  Run the iterator backwards, verify that the same breaks are found.
            //
            prevBP = t.dataToBreak.Length + 2;  // start with a phony value for the last break pos seen.
            for (bp = t.bi.Last(); bp != BreakIterator.Done; bp = t.bi.Previous())
            {
                if (prevBP == bp)
                {
                    // Fail for lack of progress.
                    Errln("Reverse Iteration, no progress.  Break Pos=" + bp +
                            "File line,col=" + t.srcLine[bp] + " " + t.srcCol[bp]);
                    break;
                }

                // Check that we didn't miss an expected break between the last one
                //  and this one.  (UVector returns zeros for index out of bounds.)
                for (i = prevBP - 1; i > bp; i--)
                {
                    if (t.expectedBreaks[i] != 0)
                    {
                        Errln("Reverse Itertion, break expected, but not found.  Pos=" + i +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i]);
                    }
                }

                // Check that the break we did find was expected
                if (t.expectedBreaks[bp] == 0)
                {
                    Errln("Reverse Itertion, break found, but not expected.  Pos=" + bp +
                            "  File line,col= " + t.srcLine[bp] + ", " + t.srcCol[bp]);
                }
                else
                {
                    // The break was expected.
                    //   Check that the {nnn} tag value is correct.
                    int expectedTagVal = t.expectedBreaks[bp];
                    if (expectedTagVal == -1)
                    {
                        expectedTagVal = 0;
                    }
                    int line = t.srcLine[bp];
                    int rs = (int)t.bi.RuleStatus;
                    if (rs != expectedTagVal)
                    {
                        Errln("Incorrect status for reverse break.  Pos = " + bp +
                              "  File line,col= " + line + ", " + t.srcCol[bp] + "\n" +
                              "          Actual, Expected status = " + rs + ", " + expectedTagVal);
                    }
                }

                prevBP = bp;
            }

            // Verify that there were no missed breaks prior to the last one found
            for (i = prevBP - 1; i >= 0; i--)
            {
                if (t.expectedBreaks[i] != 0)
                {
                    Errln("Reverse Itertion, break expected, but not found.  Pos=" + i +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i]);
                }
            }
            // Check isBoundary()
            for (i = 0; i <= t.dataToBreak.Length; i++)
            {
                bool boundaryExpected = (t.expectedBreaks[i] != 0);
                bool boundaryFound = t.bi.IsBoundary(i);
                if (boundaryExpected != boundaryFound)
                {
                    Errln("isBoundary(" + i + ") incorrect.\n" +
                          "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i] +
                          "    Expected, Actual= " + boundaryExpected + ", " + boundaryFound);
                }
            }

            // Check following()
            for (i = 0; i <= t.dataToBreak.Length; i++)
            {
                int actualBreak = t.bi.Following(i);
                int expectedBreak = BreakIterator.Done;
                for (int j = i + 1; j < t.expectedBreaks.Length; j++)
                {
                    if (t.expectedBreaks[j] != 0)
                    {
                        expectedBreak = j;
                        break;
                    }
                }
                if (expectedBreak != actualBreak)
                {
                    Errln("following(" + i + ") incorrect.\n" +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i] +
                            "    Expected, Actual= " + expectedBreak + ", " + actualBreak);
                }
            }

            // Check preceding()
            for (i = t.dataToBreak.Length; i >= 0; i--)
            {
                int actualBreak = t.bi.Preceding(i);
                int expectedBreak = BreakIterator.Done;

                for (int j = i - 1; j >= 0; j--)
                {
                    if (t.expectedBreaks[j] != 0)
                    {
                        expectedBreak = j;
                        break;
                    }
                }
                if (expectedBreak != actualBreak)
                {
                    Errln("preceding(" + i + ") incorrect.\n" +
                            "  File line,col= " + t.srcLine[i] + ", " + t.srcCol[i] +
                            "    Expected, Actual= " + expectedBreak + ", " + actualBreak);
                }
            }

        }

    }
}
