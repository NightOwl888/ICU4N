using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    public class ConformanceTest : TestFmwk
    {
        Normalizer normalizer;

        public ConformanceTest()
        {
            // Doesn't matter what the string and mode are; we'll change
            // them later as needed.
            normalizer = new Normalizer("", Normalizer.NFC, 0);
        }
        // more interesting conformance test cases, not in the unicode.org NormalizationTest.txt
        static String[] moreCases ={
            // Markus 2001aug30
            "0061 0332 0308;00E4 0332;0061 0332 0308;00E4 0332;0061 0332 0308; # Markus 0",

            // Markus 2001oct26 - test edge case for iteration: U+0f73.cc==0 but decomposition.lead.cc==129
            "0061 0301 0F73;00E1 0F71 0F72;0061 0F71 0F72 0301;00E1 0F71 0F72;0061 0F71 0F72 0301; # Markus 1"
        };

        /**
         * Test the conformance of Normalizer to
         * http://www.unicode.org/unicode/reports/tr15/conformance/Draft-TestSuite.txt.* http://www.unicode.org/Public/UNIDATA/NormalizationTest.txt
         * This file must be located at the path specified as TEST_SUITE_FILE.
         */
        [Test]
        public void TestConformance()
        {
            runConformance("unicode.NormalizationTest.txt", 0);
        }
        [Test]
        public void TestConformance_3_2()
        {
            runConformance("unicode.NormalizationTest-3.2.0.txt", Normalizer.UNICODE_3_2);
        }

        public void runConformance(String fileName, int options)
        {
            String line = null;
            String[]
            fields = new String[5];
            StringBuffer buf = new StringBuffer();
            int passCount = 0;
            int failCount = 0;
            UnicodeSet other = new UnicodeSet(0, 0x10ffff);
            int c = 0;
            TextReader input = null;
            try
            {
                input = TestUtil.GetDataReader(fileName);
                for (int count = 0; ; ++count)
                {
                    line = input.ReadLine();
                    if (line == null)
                    {
                        //read the extra test cases
                        if (count > moreCases.Length)
                        {
                            count = 0;
                        }
                        else if (count == moreCases.Length)
                        {
                            // all done
                            break;
                        }
                        line = moreCases[count++];
                    }
                    if (line.Length == 0) continue;

                    // Expect 5 columns of this format:
                    // 1E0C;1E0C;0044 0323;1E0C;0044 0323; # <comments>

                    // Skip comments
                    if (line[0] == '#' || line[0] == '@') continue;

                    // Parse out the fields
                    hexsplit(line, ';', fields, buf);

                    // Remove a single code point from the "other" UnicodeSet
                    if (fields[0].Length == UTF16.MoveCodePointOffset(fields[0], 0, 1))
                    {
                        c = UTF16.CharAt(fields[0], 0);
                        if (0xac20 <= c && c <= 0xd73f)
                        {
                            // not an exhaustive test run: skip most Hangul syllables
                            if (c == 0xac20)
                            {
                                other.Remove(0xac20, 0xd73f);
                            }
                            continue;
                        }
                        other.Remove(c);
                    }
                    if (checkConformance(fields, line, options))
                    {
                        ++passCount;
                    }
                    else
                    {
                        ++failCount;
                    }
                    if ((count % 1000) == 999)
                    {
                        Logln("Line " + (count + 1));
                    }
                }
            }
            catch (IOException ex)
            {
                ex.PrintStackTrace();
                throw new ArgumentException("Couldn't read file "
                  + ex.GetType().Name + " " + ex.ToString()
                  + " line = " + line
                  );
            }
            finally
            {
                if (input != null)
                {
                    try
                    {
                        input.Dispose();
                    }
                    catch (IOException ignored)
                    {
                    }
                }
            }

            if (failCount != 0)
            {
                Errln("Total: " + failCount + " lines failed, " +
                      passCount + " lines passed");
            }
            else
            {
                Logln("Total: " + passCount + " lines passed");
            }
        }

        /**
         * Verify the conformance of the given line of the Unicode
         * normalization (UTR 15) test suite file.  For each line,
         * there are five columns, corresponding to field[0]..field[4].
         *
         * The following invariants must be true for all conformant implementations
         *  c2 == NFC(c1) == NFC(c2) == NFC(c3)
         *  c3 == NFD(c1) == NFD(c2) == NFD(c3)
         *  c4 == NFKC(c1) == NFKC(c2) == NFKC(c3) == NFKC(c4) == NFKC(c5)
         *  c5 == NFKD(c1) == NFKD(c2) == NFKD(c3) == NFKD(c4) == NFKD(c5)
         *
         * @param field the 5 columns
         * @param line the source line from the test suite file
         * @return true if the test passes
         */
        private bool checkConformance(String[] field, String line, int options)
        {
            bool pass = true;
            StringBuffer buf = new StringBuffer(); // scratch
            String @out, fcd;
            int i = 0;
            for (i = 0; i < 5; ++i)
            {
                int fieldNum = i + 1;
                if (i < 3)
                {
                    pass &= checkNorm(Normalizer.NFC, options, field[i], field[1], fieldNum);
                    pass &= checkNorm(Normalizer.NFD, options, field[i], field[2], fieldNum);
                }
                pass &= checkNorm(Normalizer.NFKC, options, field[i], field[3], fieldNum);
                pass &= checkNorm(Normalizer.NFKD, options, field[i], field[4], fieldNum);
                cross(field[4] /*NFKD String*/, field[3]/*NFKC String*/, Normalizer.NFKC);
                cross(field[3] /*NFKC String*/, field[4]/*NFKD String*/, Normalizer.NFKD);

            }
            compare(field[1], field[2]);
            compare(field[0], field[1]);
            compare(field[0], field[2]);
            // test quick checks
            if (QuickCheckResult.No == Normalizer.QuickCheck(field[1], Normalizer.NFC, options))
            {
                Errln("Normalizer error: quickCheck(NFC(s), Normalizer.NFC) is Normalizer.NO");
                pass = false;
            }
            if (Normalizer.NO == Normalizer.QuickCheck(field[2], Normalizer.NFD, options))
            {
                Errln("Normalizer error: quickCheck(NFD(s), Normalizer.NFD) is Normalizer.NO");
                pass = false;
            }
            if (Normalizer.NO == Normalizer.QuickCheck(field[3], Normalizer.NFKC, options))
            {
                Errln("Normalizer error: quickCheck(NFKC(s), Normalizer.NFKC) is Normalizer.NO");
                pass = false;
            }
            if (Normalizer.NO == Normalizer.QuickCheck(field[4], Normalizer.NFKD, options))
            {
                Errln("Normalizer error: quickCheck(NFKD(s), Normalizer.NFKD) is Normalizer.NO");
                pass = false;
            }

            if (!Normalizer.IsNormalized(field[1], Normalizer.NFC, options))
            {
                Errln("Normalizer error: isNormalized(NFC(s), Normalizer.NFC) is false");
                pass = false;
            }
            if (!field[0].Equals(field[1]) && Normalizer.IsNormalized(field[0], Normalizer.NFC, options))
            {
                Errln("Normalizer error: isNormalized(s, Normalizer.NFC) is TRUE");
                pass = false;
            }
            if (!Normalizer.IsNormalized(field[3], Normalizer.NFKC, options))
            {
                Errln("Normalizer error: isNormalized(NFKC(s), Normalizer.NFKC) is false");
                pass = false;
            }
            if (!field[0].Equals(field[3]) && Normalizer.IsNormalized(field[0], Normalizer.NFKC, options))
            {
                Errln("Normalizer error: isNormalized(s, Normalizer.NFKC) is TRUE");
                pass = false;
            }
            // test api that takes a char[]
            if (!Normalizer.IsNormalized(field[1].ToCharArray(), 0, field[1].Length, Normalizer.NFC, options))
            {
                Errln("Normalizer error: isNormalized(NFC(s), Normalizer.NFC) is false");
                pass = false;
            }
            // test api that takes a codepoint
            if (!Normalizer.IsNormalized(UTF16.CharAt(field[1], 0), Normalizer.NFC, options))
            {
                Errln("Normalizer error: isNormalized(NFC(s), Normalizer.NFC) is false");
                pass = false;
            }
            // test FCD quick check and "makeFCD"
            fcd = Normalizer.Normalize(field[0], Normalizer.FCD);
            if (Normalizer.NO == Normalizer.QuickCheck(fcd, Normalizer.FCD, options))
            {
                Errln("Normalizer error: quickCheck(FCD(s), Normalizer.FCD) is Normalizer.NO");
                pass = false;
            }
            // check FCD return length
            {
                char[] fcd2 = new char[fcd.Length * 2];
                char[] src = field[0].ToCharArray();
                int fcdLen = Normalizer.Normalize(src, 0, src.Length, fcd2, fcd.Length, fcd2.Length, Normalizer.FCD, 0);
                if (fcdLen != fcd.Length)
                {
                    Errln("makeFCD did not return the correct length");
                }
            }
            if (Normalizer.NO == Normalizer.QuickCheck(fcd, Normalizer.FCD, options))
            {
                Errln("Normalizer error: quickCheck(FCD(s), Normalizer.FCD) is Normalizer.NO");
                pass = false;
            }
            if (Normalizer.NO == Normalizer.QuickCheck(field[2], Normalizer.FCD, options))
            {
                Errln("Normalizer error: quickCheck(NFD(s), Normalizer.FCD) is Normalizer.NO");
                pass = false;
            }

            if (Normalizer.NO == Normalizer.QuickCheck(field[4], Normalizer.FCD, options))
            {
                Errln("Normalizer error: quickCheck(NFKD(s), Normalizer.FCD) is Normalizer.NO");
                pass = false;
            }

            @out = iterativeNorm(new StringCharacterIterator(field[0]), Normalizer.FCD, buf, +1, options);
            @out = iterativeNorm(new StringCharacterIterator(field[0]), Normalizer.FCD, buf, -1, options);

            @out = iterativeNorm(new StringCharacterIterator(field[2]), Normalizer.FCD, buf, +1, options);
            @out = iterativeNorm(new StringCharacterIterator(field[2]), Normalizer.FCD, buf, -1, options);

            @out = iterativeNorm(new StringCharacterIterator(field[4]), Normalizer.FCD, buf, +1, options);
            @out = iterativeNorm(new StringCharacterIterator(field[4]), Normalizer.FCD, buf, -1, options);

            @out = Normalizer.Normalize(fcd, Normalizer.NFD);
            if (!@out.Equals(field[2]))
            {
                Errln("Normalizer error: NFD(FCD(s))!=NFD(s)");
                pass = false;
            }
            if (!pass)
            {
                Errln("FAIL: " + line);
            }
            if (field[0] != field[2])
            {
                // two strings that are canonically equivalent must test
                // equal under a canonical caseless match
                // see UAX #21 Case Mappings and Jitterbug 2021 and
                // Unicode Technical Committee meeting consensus 92-C31
                int rc;
                if ((rc = Normalizer.Compare(field[0], field[2], (options << Normalizer.COMPARE_NORM_OPTIONS_SHIFT) | Normalizer.COMPARE_IGNORE_CASE)) != 0)
                {
                    Errln("Normalizer.compare(original, NFD, case-insensitive) returned " + rc + " instead of 0 for equal");
                    pass = false;
                }
            }

            return pass;
        }

        private static int getModeNumber(Normalizer.Mode mode)
        {
            if (mode == Normalizer.NFD) { return 0; }
            if (mode == Normalizer.NFKD) { return 1; }
            if (mode == Normalizer.NFC) { return 2; }
            if (mode == Normalizer.NFKC) { return 3; }
            return -1;
        }
        private static readonly String[] kModeStrings = {
            "D", "KD", "C", "KC"
        };
        private static readonly String[] kMessages = {
            "c3!=D(c%d)", "c5!=KC(c%d)", "c2!=C(c%d)", "c4!=KC(c%d)" // ICU4N TODO: Change string format
        };

        bool checkNorm(Normalizer.Mode mode, int options,  // Normalizer2 norm2,
                String s, String exp, int field)
        {
            String modeString = kModeStrings[getModeNumber(mode)];
            String msg = String.Format(kMessages[getModeNumber(mode)], field);
            StringBuffer buf = new StringBuffer();
            String @out = Normalizer.Normalize(s, mode, options);
            if (!assertEqual(modeString, "", s, @out, exp, msg))
            {
                return false;
            }

            @out = iterativeNorm(s, mode, buf, +1, options);
            if (!assertEqual(modeString, "(+1)", s, @out, exp, msg))
            {
                return false;
            }

            @out = iterativeNorm(s, mode, buf, -1, options);
            if (!assertEqual(modeString, "(-1)", s, @out, exp, msg))
            {
                return false;
            }

            @out = iterativeNorm(new StringCharacterIterator(s), mode, buf, +1, options);
            if (!assertEqual(modeString, "(+1)", s, @out, exp, msg))
            {
                return false;
            }

            @out = iterativeNorm(new StringCharacterIterator(s), mode, buf, -1, options);
            if (!assertEqual(modeString, "(-1)", s, @out, exp, msg))
            {
                return false;
            }

            return true;
        }

        // two strings that are canonically equivalent must test
        // equal under a canonical caseless match
        // see UAX #21 Case Mappings and Jitterbug 2021 and
        // Unicode Technical Committee meeting consensus 92-C31
        private void compare(String s1, String s2)
        {
            if (s1.Length == 1 && s2.Length == 1)
            {
                if (Normalizer.Compare(UTF16.CharAt(s1, 0), UTF16.CharAt(s2, 0), Normalizer.COMPARE_IGNORE_CASE) != 0)
                {
                    Errln("Normalizer.compare(int,int) failed for s1: "
                            + Utility.Hex(s1) + " s2: " + Utility.Hex(s2));
                }
            }
            if (s1.Length == 1 && s2.Length > 1)
            {
                if (Normalizer.Compare(UTF16.CharAt(s1, 0), s2, Normalizer.COMPARE_IGNORE_CASE) != 0)
                {
                    Errln("Normalizer.compare(int,String) failed for s1: "
                            + Utility.Hex(s1) + " s2: " + Utility.Hex(s2));
                }
            }
            if (s1.Length > 1 && s2.Length > 1)
            {
                // TODO: Re-enable this tests after UTC fixes UAX 21
                if (Normalizer.Compare(s1.ToCharArray(), s2.ToCharArray(), Normalizer.COMPARE_IGNORE_CASE) != 0)
                {
                    Errln("Normalizer.compare(char[],char[]) failed for s1: "
                            + Utility.Hex(s1) + " s2: " + Utility.Hex(s2));
                }
            }
        }
        private void cross(String s1, String s2, Normalizer.Mode mode)
        {
            String result = Normalizer.Normalize(s1, mode);
            if (!result.Equals(s2))
            {
                Errln("cross test failed s1: " + Utility.Hex(s1) + " s2: "
                            + Utility.Hex(s2));
            }
        }
        /**
         * Do a normalization using the iterative API in the given direction.
         * @param buf scratch buffer
         * @param dir either +1 or -1
         */
        private String iterativeNorm(String str, Normalizer.Mode mode,
                                     StringBuffer buf, int dir, int options)
        {
            normalizer.SetText(str);
            normalizer.SetMode(mode);
            buf.Length = (0);
            normalizer.SetOption(-1, false);      // reset all options
            normalizer.SetOption(options, true);  // set desired options

            int ch;
            if (dir > 0)
            {
                for (ch = normalizer.MoveFirst(); ch != Normalizer.DONE;
                     ch = normalizer.MoveNext())
                {
                    buf.Append(UTF16.ValueOf(ch));
                }
            }
            else
            {
                for (ch = normalizer.MoveLast(); ch != Normalizer.DONE;
                     ch = normalizer.MovePrevious())
                {
                    buf.Insert(0, UTF16.ValueOf(ch));
                }
            }
            return buf.ToString();
        }

        /**
         * Do a normalization using the iterative API in the given direction.
         * @param str a Java StringCharacterIterator
         * @param buf scratch buffer
         * @param dir either +1 or -1
         */
        private String iterativeNorm(StringCharacterIterator str, Normalizer.Mode mode,
                                     StringBuffer buf, int dir, int options)
        {
            normalizer.SetText(str);
            normalizer.SetMode(mode);
            buf.Length = (0);
            normalizer.SetOption(-1, false);      // reset all options
            normalizer.SetOption(options, true);  // set desired options

            int ch;
            if (dir > 0)
            {
                for (ch = normalizer.MoveFirst(); ch != Normalizer.DONE;
                     ch = normalizer.MoveNext())
                {
                    buf.Append(UTF16.ValueOf(ch));
                }
            }
            else
            {
                for (ch = normalizer.MoveLast(); ch != Normalizer.DONE;
                     ch = normalizer.MovePrevious())
                {
                    buf.Insert(0, UTF16.ValueOf(ch));
                }
            }
            return buf.ToString();
        }

        /**
         * @param op name of normalization form, e.g., "KC"
         * @param op2 name of test case variant, e.g., "(-1)"
         * @param s string being normalized
         * @param got value received
         * @param exp expected value
         * @param msg description of this test
         * @returns true if got == exp
         */
        private bool assertEqual(String op, String op2, String s, String got,
                                    String exp, String msg)
        {
            if (exp.Equals(got))
            {
                return true;
            }
            Errln(("      " + msg + ": " + op + op2 + '(' + s + ")=" + Hex(got) +
                                 ", exp. " + Hex(exp)));
            return false;
        }

        /**
         * Split a string into pieces based on the given delimiter
         * character.  Then, parse the resultant fields from hex into
         * characters.  That is, "0040 0400;0C00;0899" -> new String[] {
         * "\u0040\u0400", "\u0C00", "\u0899" }.  The output is assumed to
         * be of the proper length already, and exactly output.Length
         * fields are parsed.  If there are too few an exception is
         * thrown.  If there are too many the extras are ignored.
         *
         * @param buf scratch buffer
         */
        private static void hexsplit(String s, char delimiter,
                                     String[] output, StringBuffer buf)
        {
            int i;
            int pos = 0;
            for (i = 0; i < output.Length; ++i)
            {
                int delim = s.IndexOf(delimiter, pos);
                if (delim < 0)
                {
                    throw new ArgumentException("Missing field in " + s);
                }
                // Our field is from pos..delim-1.
                buf.Length = (0);

                String toHex = s.Substring(pos, delim - pos); // ICU4N: Corrected 2nd parameter
                pos = delim;
                int index = 0;
                int len = toHex.Length;
                while (index < len)
                {
                    if (toHex[index] == ' ')
                    {
                        index++;
                    }
                    else
                    {
                        int spacePos = toHex.IndexOf(' ', index);
                        if (spacePos == -1)
                        {
                            appendInt(buf, toHex.Substring(index, len - index), s); // ICU4N: Corrected 2nd substring parameter
                            spacePos = len;
                        }
                        else
                        {
                            appendInt(buf, toHex.Substring(index, spacePos - index), s); // ICU4N: Corrected 2nd substring parameter
                        }
                        index = spacePos + 1;
                    }
                }

                if (buf.Length < 1)
                {
                    throw new ArgumentException("Empty field " + i + " in " + s);
                }
                output[i] = buf.ToString();
                ++pos; // Skip over delim
            }
        }
        public static void appendInt(StringBuffer buf, String strToHex, String s)
        {
            int hex = int.Parse(strToHex, NumberStyles.HexNumber);
            if (hex < 0)
            {
                throw new ArgumentException("Out of range hex " +
                                                    hex + " in " + s);
            }
            else if (hex > 0xFFFF)
            {
                buf.Append((char)((hex >> 10) + 0xd7c0));
                buf.Append((char)((hex & 0x3ff) | 0xdc00));
            }
            else
            {
                buf.Append((char)hex);
            }
        }

        // Specific tests for debugging.  These are generally failures
        // taken from the conformance file, but culled out to make
        // debugging easier.  These can be eliminated without affecting
        // coverage.
        [Ignore("Comment to run manually")]
        [Test]
        public void _hideTestCase6(/*int options*/)
        {
            _testOneLine("0385;0385;00A8 0301;0020 0308 0301;0020 0308 0301;", /*options*/ 0);
        }

        private void _testOneLine(String line, int options)
        {
            String[]
            fields = new String[5];
            StringBuffer buf = new StringBuffer();
            // Parse out the fields
            hexsplit(line, ';', fields, buf);
            checkConformance(fields, line, options);
        }


    }
}
