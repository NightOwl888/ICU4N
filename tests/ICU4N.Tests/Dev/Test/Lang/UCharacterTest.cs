using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Text.Unicode;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Testing class for UCharacter
    /// Mostly following the test cases for ICU
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>nov 04 2000</since>
    public sealed class UCharacterTest : TestFmwk
    {
        // private variables =============================================

        /**
         * Expected Unicode version.
         */
        private readonly VersionInfo VERSION_ = VersionInfo.GetInstance(10);

        // constructor ===================================================

        /**
        * Constructor
        */
        public UCharacterTest()
        {
        }

        // public methods ================================================

        /**
        * Testing the letter and number determination in UCharacter
        */
        [Test]
        public void TestLetterNumber()
        {
            for (int i = 0x0041; i < 0x005B; i++)
                if (!UChar.IsLetter(i))
                    Errln("FAIL \\u" + Hex(i) + " expected to be a letter");

            for (int i = 0x0660; i < 0x066A; i++)
                if (UChar.IsLetter(i))
                    Errln("FAIL \\u" + Hex(i) + " expected not to be a letter");

            for (int i = 0x0660; i < 0x066A; i++)
                if (!UChar.IsDigit(i))
                    Errln("FAIL \\u" + Hex(i) + " expected to be a digit");

            for (int i = 0x0041; i < 0x005B; i++)
                if (!UChar.IsLetterOrDigit(i))
                    Errln("FAIL \\u" + Hex(i) + " expected not to be a digit");

            for (int i = 0x0660; i < 0x066A; i++)
                if (!UChar.IsLetterOrDigit(i))
                    Errln("FAIL \\u" + Hex(i) +
                        "expected to be either a letter or a digit");

            /*
             * The following checks work only starting from Unicode 4.0.
             * Check the version number here.
             */
            VersionInfo version = UChar.UnicodeVersion;
            if (version.Major < 4 || version.Equals(VersionInfo.GetInstance(4, 0, 1)))
            {
                return;
            }



            /*
             * Sanity check:
             * Verify that exactly the digit characters have decimal digit values.
             * This assumption is used in the implementation of u_digit()
             * (which checks nt=de)
             * compared with the parallel java.lang.Character.digit()
             * (which checks Nd).
             *
             * This was not true in Unicode 3.2 and earlier.
             * Unicode 4.0 fixed discrepancies.
             * Unicode 4.0.1 re-introduced problems in this area due to an
             * unintentionally incomplete last-minute change.
             */
            String digitsPattern = "[:Nd:]";
            String decimalValuesPattern = "[:Numeric_Type=Decimal:]";

            UnicodeSet digits, decimalValues;

            digits = new UnicodeSet(digitsPattern);
            decimalValues = new UnicodeSet(decimalValuesPattern);


            CompareUSets(digits, decimalValues, "[:Nd:]", "[:Numeric_Type=Decimal:]", true);


        }

        /**
        * Tests for space determination in UCharacter
        */
        [Test]
        public void TestSpaces()
        {
            int[] spaces = { 0x0020, 0x00a0, 0x2000, 0x2001, 0x2005 };
            int[] nonspaces = { 0x0061, 0x0062, 0x0063, 0x0064, 0x0074 };
            int[] whitespaces = { 0x2008, 0x2009, 0x200a, 0x001c, 0x000c /* ,0x200b */}; // 0x200b was "Zs" in Unicode 4.0, but it is "Cf" in Unicode 4.1
            int[] nonwhitespaces = { 0x0061, 0x0062, 0x003c, 0x0028, 0x003f, 0x00a0, 0x2007, 0x202f, 0xfefe, 0x200b };

            int size = spaces.Length;
            for (int i = 0; i < size; i++)
            {
                if (!UChar.IsSpaceChar(spaces[i]))
                {
                    Errln("FAIL \\u" + Hex(spaces[i]) +
                        " expected to be a space character");
                    break;
                }

                if (UChar.IsSpaceChar(nonspaces[i]))
                {
                    Errln("FAIL \\u" + Hex(nonspaces[i]) +
                    " expected not to be space character");
                    break;
                }

                if (!UChar.IsWhiteSpace(whitespaces[i]))
                {
                    Errln("FAIL \\u" + Hex(whitespaces[i]) +
                            " expected to be a white space character");
                    break;
                }
                if (UChar.IsWhiteSpace(nonwhitespaces[i]))
                {
                    Errln("FAIL \\u" + Hex(nonwhitespaces[i]) +
                                " expected not to be a space character");
                    break;
                }
                Logln("Ok    \\u" + Hex(spaces[i]) + " and \\u" +
                      Hex(nonspaces[i]) + " and \\u" + Hex(whitespaces[i]) +
                      " and \\u" + Hex(nonwhitespaces[i]));
            }

            int[] patternWhiteSpace = {0x9, 0xd, 0x20, 0x85,
                                0x200e, 0x200f, 0x2028, 0x2029};
            int[] nonPatternWhiteSpace = {0x8, 0xe, 0x21, 0x86, 0xa0, 0xa1,
                                   0x1680, 0x1681, 0x180e, 0x180f,
                                   0x1FFF, 0x2000, 0x200a, 0x200b,
                                   0x2010, 0x202f, 0x2030, 0x205f,
                                   0x2060, 0x3000, 0x3001};
            for (int i = 0; i < patternWhiteSpace.Length; i++)
            {
                if (!PatternProps.IsWhiteSpace(patternWhiteSpace[i]))
                {
                    Errln("\\u" + Utility.Hex(patternWhiteSpace[i], 4)
                          + " expected to be a Pattern_White_Space");
                }
            }
            for (int i = 0; i < nonPatternWhiteSpace.Length; i++)
            {
                if (PatternProps.IsWhiteSpace(nonPatternWhiteSpace[i]))
                {
                    Errln("\\u" + Utility.Hex(nonPatternWhiteSpace[i], 4)
                          + " expected to be a non-Pattern_White_Space");
                }
            }

            // TODO: propose public API for constants like uchar.h's U_GC_*_MASK
            // (http://bugs.icu-project.org/trac/ticket/7461)
            int GC_Z_MASK =
                (1 << UUnicodeCategory.SpaceSeparator.ToInt32()) |
                (1 << UUnicodeCategory.LineSeparator.ToInt32()) |
                (1 << UUnicodeCategory.ParagraphSeparator.ToInt32());

            // UCharacter.isWhitespace(c) should be the same as Character.isWhitespace().
            // This uses Logln() because Character.isWhitespace() differs between Java versions, thus
            // it is not necessarily an error if there is a difference between
            // particular Java and ICU versions.
            // However, you need to run tests with -v to see the output.
            // Also note that, at least as of Unicode 5.2,
            // there are no supplementary white space characters.
            for (int c = 0; c <= 0xffff; ++c)
            {
                bool j = char.IsWhiteSpace((char)c);
                bool i = UChar.IsWhiteSpace(c);
                bool u = UChar.IsUWhiteSpace(c);
                bool z = (UChar.GetIntPropertyValue(c, UProperty.General_Category_Mask) &
                             GC_Z_MASK) != 0;
                if (j != i)
                {
                    Logln(String.Format(
                        "isWhitespace(U+{0:x4}) difference: JDK {1} ICU {2} Unicode WS {3} Z Separator {4}",
                        c, j, i, u, z));
                }
                else if (j || i || u || z)
                {
                    Logln(String.Format(
                        "isWhitespace(U+{0:x4}) FYI:        JDK {1} ICU {2} Unicode WS {3} Z Separator {4}",
                        c, j, i, u, z));
                }
            }
            for (char c = (char)0; c <= 0xff; ++c)
            {
                bool j = J2N.Character.IsSpace(c);
                bool i = UChar.IsSpace(c);
                bool z = (UChar.GetIntPropertyValue(c, UProperty.General_Category_Mask) &
                             GC_Z_MASK) != 0;
                if (j != i)
                {
                    Logln(String.Format(
                        "isSpace(U+{0:x4}) difference: JDK {1} ICU {2} Z Separator {3}",
                        (int)c, j, i, z));
                }
                else if (j || i || z)
                {
                    Logln(String.Format(
                        "isSpace(U+{0:x4}) FYI:        JDK {1} ICU {2} Z Separator {3}",
                        (int)c, j, i, z));
                }
            }
        }

        /**
         * Test various implementations of Pattern_Syntax &amp; Pattern_White_Space.
         */
        [Test]
        public void TestPatternProperties()
        {
            UnicodeSet syn_pp = new UnicodeSet();
            UnicodeSet syn_prop = new UnicodeSet("[:Pattern_Syntax:]");
            UnicodeSet syn_list = new UnicodeSet(
                "[!-/\\:-@\\[-\\^`\\{-~" +
                "\u00A1-\u00A7\u00A9\u00AB\u00AC\u00AE\u00B0\u00B1\u00B6\u00BB\u00BF\u00D7\u00F7" +
                "\u2010-\u2027\u2030-\u203E\u2041-\u2053\u2055-\u205E\u2190-\u245F\u2500-\u2775" +
                "\u2794-\u2BFF\u2E00-\u2E7F\u3001-\u3003\u3008-\u3020\u3030\uFD3E\uFD3F\uFE45\uFE46]");
            UnicodeSet ws_pp = new UnicodeSet();
            UnicodeSet ws_prop = new UnicodeSet("[:Pattern_White_Space:]");
            UnicodeSet ws_list = new UnicodeSet("[\\u0009-\\u000D\\ \\u0085\\u200E\\u200F\\u2028\\u2029]");
            UnicodeSet syn_ws_pp = new UnicodeSet();
            UnicodeSet syn_ws_prop = new UnicodeSet(syn_prop).AddAll(ws_prop);
            for (int c = 0; c <= 0xffff; ++c)
            {
                if (PatternProps.IsSyntax(c))
                {
                    syn_pp.Add(c);
                }
                if (PatternProps.IsWhiteSpace(c))
                {
                    ws_pp.Add(c);
                }
                if (PatternProps.IsSyntaxOrWhiteSpace(c))
                {
                    syn_ws_pp.Add(c);
                }
            }
            CompareUSets(syn_pp, syn_prop,
                         "PatternProps.isSyntax()", "[:Pattern_Syntax:]", true);
            CompareUSets(syn_pp, syn_list,
                         "PatternProps.isSyntax()", "[Pattern_Syntax ranges]", true);
            CompareUSets(ws_pp, ws_prop,
                         "PatternProps.isWhiteSpace()", "[:Pattern_White_Space:]", true);
            CompareUSets(ws_pp, ws_list,
                         "PatternProps.isWhiteSpace()", "[Pattern_White_Space ranges]", true);
            CompareUSets(syn_ws_pp, syn_ws_prop,
                         "PatternProps.isSyntaxOrWhiteSpace()",
                         "[[:Pattern_Syntax:][:Pattern_White_Space:]]", true);
        }

        /**
        * Tests for defined and undefined characters
        */
        [Test]
        public void TestDefined()
        {
            int[] undefined = { 0xfff1, 0xfff7, 0xfa6e };
            int[] defined = { 0x523E, 0x004f88, 0x00fffd };

            int size = undefined.Length;
            for (int i = 0; i < size; i++)
            {
                if (UChar.IsDefined(undefined[i]))
                {
                    Errln("FAIL \\u" + Hex(undefined[i]) +
                                " expected not to be defined");
                    break;
                }
                if (!UChar.IsDefined(defined[i]))
                {
                    Errln("FAIL \\u" + Hex(defined[i]) + " expected defined");
                    break;
                }
            }
        }

        /**
        * Tests for base characters and their cellwidth
        */
        [Test]
        public void TestBase()
        {
            int[] @base = { 0x0061, 0x000031, 0x0003d2 };
            int[] nonbase = { 0x002B, 0x000020, 0x00203B };
            int size = @base.Length;
            for (int i = 0; i < size; i++)
            {
                if (UChar.IsBaseForm(nonbase[i]))
                {
                    Errln("FAIL \\u" + Hex(nonbase[i]) +
                                " expected not to be a base character");
                    break;
                }
                if (!UChar.IsBaseForm(@base[i]))
                {
                    Errln("FAIL \\u" + Hex(@base[i]) +
                          " expected to be a base character");
                    break;
                }
            }
        }

        /**
        * Tests for digit characters
        */
        [Test]
        public void TestDigits()
        {
            int[] digits = { 0x0030, 0x000662, 0x000F23, 0x000ED5, 0x002160 };

            //special characters not in the properties table
            int[] digits2 = {0x3007, 0x004e00, 0x004e8c, 0x004e09, 0x0056d8,
                         0x004e94, 0x00516d, 0x4e03, 0x00516b, 0x004e5d};
            int[] nondigits = { 0x0010, 0x000041, 0x000122, 0x0068FE };

            int[] digitvalues = { 0, 2, 3, 5, 1 };
            int[] digitvalues2 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            int size = digits.Length;
            for (int i = 0; i < size; i++)
            {
                if (UChar.IsDigit(digits[i]) &&
                    UChar.Digit(digits[i]) != digitvalues[i])
                {
                    Errln("FAIL \\u" + Hex(digits[i]) +
                            " expected digit with value " + digitvalues[i]);
                    break;
                }
            }
            size = nondigits.Length;
            for (int i = 0; i < size; i++)
                if (UChar.IsDigit(nondigits[i]))
                {
                    Errln("FAIL \\u" + Hex(nondigits[i]) + " expected nondigit");
                    break;
                }

            size = digits2.Length;
            for (int i = 0; i < 10; i++)
            {
                if (UChar.IsDigit(digits2[i]) &&
                    UChar.Digit(digits2[i]) != digitvalues2[i])
                {
                    Errln("FAIL \\u" + Hex(digits2[i]) +
                        " expected digit with value " + digitvalues2[i]);
                    break;
                }
            }
        }

        /**
        *  Tests for numeric characters
        */
        [Test]
        public void TestNumeric()
        {
            if (UChar.GetNumericValue(0x00BC) != -2)
            {
                Errln("Numeric value of 0x00BC expected to be -2");
            }

            for (int i = '0'; i < '9'; i++)
            {
                int n1 = UChar.GetNumericValue(i);
                double n2 = UChar.GetUnicodeNumericValue(i);
                if (n1 != n2 || n1 != (i - '0'))
                {
                    Errln("Numeric value of " + (char)i + " expected to be " +
                          (i - '0'));
                }
            }
            for (int i = 'A'; i < 'F'; i++)
            {
                int n1 = UChar.GetNumericValue(i);
                double n2 = UChar.GetUnicodeNumericValue(i);
                if (n2 != UChar.NoNumericValue || n1 != (i - 'A' + 10))
                {
                    Errln("Numeric value of " + (char)i + " expected to be " +
                          (i - 'A' + 10));
                }
            }
            for (int i = 0xFF21; i < 0xFF26; i++)
            {
                // testing full wideth latin characters A-F
                int n1 = UChar.GetNumericValue(i);
                double n2 = UChar.GetUnicodeNumericValue(i);
                if (n2 != UChar.NoNumericValue || n1 != (i - 0xFF21 + 10))
                {
                    Errln("Numeric value of " + (char)i + " expected to be " +
                          (i - 0xFF21 + 10));
                }
            }
            // testing han numbers
            int[] han = {0x96f6, 0, 0x58f9, 1, 0x8cb3, 2, 0x53c3, 3,
                     0x8086, 4, 0x4f0d, 5, 0x9678, 6, 0x67d2, 7,
                     0x634c, 8, 0x7396, 9, 0x5341, 10, 0x62fe, 10,
                     0x767e, 100, 0x4f70, 100, 0x5343, 1000, 0x4edf, 1000,
                     0x824c, 10000, 0x5104, 100000000};
            for (int i = 0; i < han.Length; i += 2)
            {
                if (UChar.GetHanNumericValue(han[i]) != han[i + 1])
                {
                    Errln("Numeric value of \\u" +
                          (han[i]).ToHexString() + " expected to be " +
                          han[i + 1]);
                }
            }
        }

        /**
        * Tests for version
        */
        [Test]
        public void TestVersion()
        {
            if (!UChar.UnicodeVersion.Equals(VERSION_))
                Errln("FAIL expected: " + VERSION_ + " got: " + UChar.UnicodeVersion);
        }

        /**
        * Tests for control characters
        */
        [Test]
        public void TestISOControl()
        {
            int[] control = { 0x001b, 0x000097, 0x000082 };
            int[] noncontrol = { 0x61, 0x000031, 0x0000e2 };

            int size = control.Length;
            for (int i = 0; i < size; i++)
            {
                if (!UChar.IsISOControl(control[i]))
                {
                    Errln("FAIL 0x" + (control[i]).ToHexString() +
                            " expected to be a control character");
                    break;
                }
                if (UChar.IsISOControl(noncontrol[i]))
                {
                    Errln("FAIL 0x" + (noncontrol[i]).ToHexString() +
                            " expected to be not a control character");
                    break;
                }

                Logln("Ok    0x" + (control[i]).ToHexString() + " and 0x" +
                        (noncontrol[i]).ToHexString());
            }
        }

        /**
         * Test Supplementary
         */
        [Test]
        public void TestSupplementary()
        {
            for (int i = 0; i < 0x10000; i++)
            {
                if (UChar.IsSupplementary(i))
                {
                    Errln("Codepoint \\u" + (i).ToHexString() +
                          " is not supplementary");
                }
            }
            for (int i = 0x10000; i < 0x10FFFF; i++)
            {
                if (!UChar.IsSupplementary(i))
                {
                    Errln("Codepoint \\u" + (i).ToHexString() +
                          " is supplementary");
                }
            }
        }

        /**
         * Test mirroring
         */
        [Test]
        public void TestMirror()
        {
            if (!(UChar.IsMirrored(0x28) && UChar.IsMirrored(0xbb) &&
                  UChar.IsMirrored(0x2045) && UChar.IsMirrored(0x232a)
                  && !UChar.IsMirrored(0x27) &&
                  !UChar.IsMirrored(0x61) && !UChar.IsMirrored(0x284)
                  && !UChar.IsMirrored(0x3400)))
            {
                Errln("isMirrored() does not work correctly");
            }

            if (!(UChar.GetMirror(0x3c) == 0x3e &&
                  UChar.GetMirror(0x5d) == 0x5b &&
                  UChar.GetMirror(0x208d) == 0x208e &&
                  UChar.GetMirror(0x3017) == 0x3016 &&

                  UChar.GetMirror(0xbb) == 0xab &&
                  UChar.GetMirror(0x2215) == 0x29F5 &&
                  UChar.GetMirror(0x29F5) == 0x2215 && /* large delta between the code points */

                  UChar.GetMirror(0x2e) == 0x2e &&
                  UChar.GetMirror(0x6f3) == 0x6f3 &&
                  UChar.GetMirror(0x301c) == 0x301c &&
                  UChar.GetMirror(0xa4ab) == 0xa4ab &&

                  /* see Unicode Corrigendum #6 at http://www.unicode.org/versions/corrigendum6.html */
                  UChar.GetMirror(0x2018) == 0x2018 &&
                  UChar.GetMirror(0x201b) == 0x201b &&
                  UChar.GetMirror(0x301d) == 0x301d))
            {
                Errln("getMirror() does not work correctly");
            }

            /* verify that Bidi_Mirroring_Glyph roundtrips */
            UnicodeSet set = new UnicodeSet("[:Bidi_Mirrored:]");
            UnicodeSetIterator iter = new UnicodeSetIterator(set);
            int start, end, c2, c3;
            while (iter.NextRange() && (start = iter.Codepoint) >= 0)
            {
                end = iter.CodepointEnd;
                do
                {
                    c2 = UChar.GetMirror(start);
                    c3 = UChar.GetMirror(c2);
                    if (c3 != start)
                    {
                        Errln("getMirror() does not roundtrip: U+" + Hex(start) + "->U+" + Hex(c2) + "->U+" + Hex(c3));
                    }
                    c3 = UChar.GetBidiPairedBracket(start);
                    if (UChar.GetIntPropertyValue(start, UProperty.Bidi_Paired_Bracket_Type) == BidiPairedBracketType.None)
                    {
                        if (c3 != start)
                        {
                            Errln("u_getBidiPairedBracket(U+" + Hex(start) + ") != self for bpt(c)==None");
                        }
                    }
                    else
                    {
                        if (c3 != c2)
                        {
                            Errln("u_getBidiPairedBracket(U+" + Hex(start) + ") != U+" + Hex(c2) + " = bmg(c)'");
                        }
                    }
                } while (++start <= end);
            }

            // verify that Unicode Corrigendum #6 reverts mirrored status of the following
            if (UChar.IsMirrored(0x2018) ||
                UChar.IsMirrored(0x201d) ||
                UChar.IsMirrored(0x201f) ||
                UChar.IsMirrored(0x301e))
            {
                Errln("Unicode Corrigendum #6 conflict, one or more of 2018/201d/201f/301e has mirrored property");
            }
        }

        /**
        * Tests for printable characters
        */
        [Test]
        public void TestPrint()
        {
            int[] printable = { 0x0042, 0x00005f, 0x002014 };
            int[] nonprintable = { 0x200c, 0x00009f, 0x00001b };

            int size = printable.Length;
            for (int i = 0; i < size; i++)
            {
                if (!UChar.IsPrintable(printable[i]))
                {
                    Errln("FAIL \\u" + Hex(printable[i]) +
                        " expected to be a printable character");
                    break;
                }
                if (UChar.IsPrintable(nonprintable[i]))
                {
                    Errln("FAIL \\u" + Hex(nonprintable[i]) +
                            " expected not to be a printable character");
                    break;
                }
                Logln("Ok    \\u" + Hex(printable[i]) + " and \\u" +
                        Hex(nonprintable[i]));
            }

            // test all ISO 8 controls
            for (int ch = 0; ch <= 0x9f; ++ch)
            {
                if (ch == 0x20)
                {
                    // skip ASCII graphic characters and continue with DEL
                    ch = 0x7f;
                }
                if (UChar.IsPrintable(ch))
                {
                    Errln("Fail \\u" + Hex(ch) +
                        " is a ISO 8 control character hence not printable\n");
                }
            }

            /* test all Latin-1 graphic characters */
            for (int ch = 0x20; ch <= 0xff; ++ch)
            {
                if (ch == 0x7f)
                {
                    ch = 0xa0;
                }
                if (!UChar.IsPrintable(ch)
                    && ch != 0x00AD/* Unicode 4.0 changed the defintion of soft hyphen to be a Cf*/)
                {
                    Errln("Fail \\u" + Hex(ch) +
                          " is a Latin-1 graphic character\n");
                }
            }
        }

        /**
        * Testing for identifier characters
        */
        [Test]
        public void TestIdentifier()
        {
            int[] unicodeidstart = { 0x0250, 0x0000e2, 0x000061 };
            int[] nonunicodeidstart = { 0x2000, 0x00000a, 0x002019 };
            int[] unicodeidpart = { 0x005f, 0x000032, 0x000045 };
            int[] nonunicodeidpart = { 0x2030, 0x0000a3, 0x000020 };
            int[] idignore = { 0x0006, 0x0010, 0x206b };
            int[] nonidignore = { 0x0075, 0x0000a3, 0x000061 };

            int size = unicodeidstart.Length;
            for (int i = 0; i < size; i++)
            {
                if (!UChar.IsUnicodeIdentifierStart(unicodeidstart[i]))
                {
                    Errln("FAIL \\u" + Hex(unicodeidstart[i]) +
                        " expected to be a unicode identifier start character");
                    break;
                }
                if (UChar.IsUnicodeIdentifierStart(nonunicodeidstart[i]))
                {
                    Errln("FAIL \\u" + Hex(nonunicodeidstart[i]) +
                            " expected not to be a unicode identifier start " +
                            "character");
                    break;
                }
                if (!UChar.IsUnicodeIdentifierPart(unicodeidpart[i]))
                {
                    Errln("FAIL \\u" + Hex(unicodeidpart[i]) +
                        " expected to be a unicode identifier part character");
                    break;
                }
                if (UChar.IsUnicodeIdentifierPart(nonunicodeidpart[i]))
                {
                    Errln("FAIL \\u" + Hex(nonunicodeidpart[i]) +
                            " expected not to be a unicode identifier part " +
                            "character");
                    break;
                }
                if (!UChar.IsIdentifierIgnorable(idignore[i]))
                {
                    Errln("FAIL \\u" + Hex(idignore[i]) +
                            " expected to be a ignorable unicode character");
                    break;
                }
                if (UChar.IsIdentifierIgnorable(nonidignore[i]))
                {
                    Errln("FAIL \\u" + Hex(nonidignore[i]) +
                        " expected not to be a ignorable unicode character");
                    break;
                }
                Logln("Ok    \\u" + Hex(unicodeidstart[i]) + " and \\u" +
                        Hex(nonunicodeidstart[i]) + " and \\u" +
                        Hex(unicodeidpart[i]) + " and \\u" +
                        Hex(nonunicodeidpart[i]) + " and \\u" +
                        Hex(idignore[i]) + " and \\u" + Hex(nonidignore[i]));
            }
        }

        /**
        * Tests for the character types, direction.<br/>
        * This method reads in UnicodeData.txt file for testing purposes. A
        * default path is provided relative to the src path, however the user
        * could set a system property to change the directory path.<br/>
        * e.g. java -DUnicodeData="data_directory_path"
        * com.ibm.icu.dev.test.lang.UCharacterTest
        */
        [Test]
        public void TestUnicodeData()
        {
            // this is the 2 char category types used in the UnicodeData file
            String TYPE =
                "LuLlLtLmLoMnMeMcNdNlNoZsZlZpCcCfCoCsPdPsPePcPoSmScSkSoPiPf";

            // directorionality types used in the UnicodeData file
            // padded by spaces to make each type size 4
            String DIR =
                "L   R   EN  ES  ET  AN  CS  B   S   WS  ON  LRE LRO AL  RLE RLO PDF NSM BN  FSI LRI RLI PDI ";

            Normalizer2 nfc = Normalizer2.GetNFCInstance();
            Normalizer2 nfkc = Normalizer2.GetNFKCInstance();

            TextReader input = null;
            try
            {
                input = TestUtil.GetDataReader("unicode/UnicodeData.txt");
                int numErrors = 0;

                for (; ; )
                {
                    String s = input.ReadLine();
                    if (s == null)
                    {
                        break;
                    }
                    if (s.Length < 4 || s.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    String[] fields = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Debug.Assert((fields.Length == 15), "Number of fields is " + fields.Length + ": " + s);

                    int ch = int.Parse(fields[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    // testing the general category
                    int type = TYPE.IndexOf(fields[2], StringComparison.Ordinal);
                    if (type < 0)
                        type = 0;
                    else
                        type = (type >> 1) + 1;
                    if (UChar.GetUnicodeCategory(ch).ToInt32() != type)
                    {
                        Errln("FAIL \\u" + Hex(ch) + " expected type " + type);
                        break;
                    }

                    if (UChar.GetIntPropertyValue(ch,
                               UProperty.General_Category_Mask) != (1 << type))
                    {
                        Errln("error: getIntPropertyValue(\\u" +
                              (ch).ToHexString() +
                              ", UProperty.GENERAL_CATEGORY_MASK) != " +
                              "getMask(getType(ch))");
                    }

                    // testing combining class
                    int cc = int.Parse(fields[3], CultureInfo.InvariantCulture);
                    if (UChar.GetCombiningClass(ch) != cc)
                    {
                        Errln("FAIL \\u" + Hex(ch) + " expected combining " +
                                "class " + cc);
                        break;
                    }
                    if (nfkc.GetCombiningClass(ch) != cc)
                    {
                        Errln("FAIL \\u" + Hex(ch) + " expected NFKC combining " +
                                "class " + cc);
                        break;
                    }

                    // testing the direction
                    String d = fields[4];
                    if (d.Length == 1)
                        d = d + "   ";

                    int dir = DIR.IndexOf(d, StringComparison.Ordinal) >> 2;
                    if (UChar.GetDirection(ch).ToInt32() != dir)
                    {
                        Errln("FAIL \\u" + Hex(ch) +
                            " expected direction " + dir + " but got " + UChar.GetDirection(ch).ToInt32());
                        break;
                    }

                    byte bdir = (byte)dir;
                    if (UChar.GetDirectionality(ch) != bdir)
                    {
                        Errln("FAIL \\u" + Hex(ch) +
                            " expected directionality " + bdir + " but got " +
                            UChar.GetDirectionality(ch));
                        break;
                    }

                    /* get Decomposition_Type & Decomposition_Mapping, field 5 */
                    int dt;
                    if (fields[5].Length == 0)
                    {
                        /* no decomposition, except UnicodeData.txt omits Hangul syllable decompositions */
                        if (ch == 0xac00 || ch == 0xd7a3)
                        {
                            dt = DecompositionType.Canonical;
                        }
                        else
                        {
                            dt = DecompositionType.None;
                        }
                    }
                    else
                    {
                        d = fields[5];
                        dt = -1;
                        if (d[0] == '<')
                        {
                            int end = d.IndexOf('>', 1);
                            if (end >= 0)
                            {
                                dt = UChar.GetPropertyValueEnum(UProperty.Decomposition_Type, d.Substring(1, end - 1));// ICU4N: Corrected 2nd parameter
                                while (d[++end] == ' ') { }  // skip spaces
                                d = d.Substring(end);
                            }
                        }
                        else
                        {
                            dt = DecompositionType.Canonical;
                        }
                    }
                    String dm;
                    if (dt > DecompositionType.None)
                    {
                        if (ch == 0xac00)
                        {
                            dm = "\u1100\u1161";
                        }
                        else if (ch == 0xd7a3)
                        {
                            dm = "\ud788\u11c2";
                        }
                        else
                        {
                            String[] dmChars = Regex.Split(d, " +");
                            StringBuilder dmb = new StringBuilder(dmChars.Length);
                            foreach (String dmc in dmChars)
                            {
                                dmb.AppendCodePoint(Convert.ToInt32(dmc, 16));
                            }
                            dm = dmb.ToString();
                        }
                    }
                    else
                    {
                        dm = null;
                    }
                    if (dt < 0)
                    {
                        Errln(String.Format("error in UnicodeData.txt: syntax error in U+{0:X4} decomposition field", ch));
                        return;
                    }
                    int i = UChar.GetIntPropertyValue(ch, UProperty.Decomposition_Type);
                    assertEquals(
                            String.Format("error: UCharacter.getIntPropertyValue(U+{0:X4}, UProperty.DECOMPOSITION_TYPE) is wrong", ch),
                            dt, i);
                    /* Expect Decomposition_Mapping=nfkc.getRawDecomposition(c). */
                    String mapping = nfkc.GetRawDecomposition(ch);
                    assertEquals(
                            String.Format("error: nfkc.getRawDecomposition(U+{0:X4}) is wrong", ch),
                            dm, mapping);
                    /* For canonical decompositions only, expect Decomposition_Mapping=nfc.getRawDecomposition(c). */
                    if (dt != DecompositionType.Canonical)
                    {
                        dm = null;
                    }
                    mapping = nfc.GetRawDecomposition(ch);
                    assertEquals(
                            String.Format("error: nfc.getRawDecomposition(U+{0:X4}) is wrong", ch),
                            dm, mapping);
                    /* recompose */
                    if (dt == DecompositionType.Canonical
                            && !UChar.HasBinaryProperty(ch, UProperty.Full_Composition_Exclusion))
                    {
                        int a = dm.CodePointAt(0);
                        int b = dm.CodePointBefore(dm.Length);
                        int composite = nfc.ComposePair(a, b);
                        assertEquals(
                                String.Format(
                                        "error: nfc U+{0:X4} decomposes to U+{1:X4}+U+{2:X4} " +
                                        "but does not compose back (instead U+{3:X4})",
                                        ch, a, b, composite),
                                ch, composite);
                        /*
                         * Note: NFKC has fewer round-trip mappings than NFC,
                         * so we can't just test nfkc.composePair(a, b) here without further data.
                         */
                    }

                    // testing iso comment
                    try
                    {
                        String isocomment = fields[11];
                        String comment = UChar.GetISOComment(ch);
                        if (comment == null)
                        {
                            comment = "";
                        }
                        if (!comment.Equals(isocomment))
                        {
                            Errln("FAIL \\u" + Hex(ch) +
                                " expected iso comment " + isocomment);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.IndexOf("unames.icu", StringComparison.Ordinal) >= 0)
                        {
                            numErrors++;
                        }
                        else
                        {
                            throw; // ICU4N: CA2200: Rethrow to preserve stack details
                        }
                    }

                    String upper = fields[12];
                    int tempchar = ch;
                    if (upper.Length > 0)
                    {
                        tempchar = Convert.ToInt32(upper, 16);
                    }
                    int resultCp = UChar.ToUpper(ch);
                    if (resultCp != tempchar)
                    {
                        Errln("FAIL \\u" + Utility.Hex(ch, 4)
                                + " expected uppercase \\u"
                                + Utility.Hex(tempchar, 4)
                                + " but got \\u"
                                + Utility.Hex(resultCp, 4));
                        break;
                    }

                    String lower = fields[13];
                    tempchar = ch;
                    if (lower.Length > 0)
                    {
                        tempchar = Convert.ToInt32(lower, 16);
                    }
                    if (UChar.ToLower(ch) != tempchar)
                    {
                        Errln("FAIL \\u" + Utility.Hex(ch, 4)
                                + " expected lowercase \\u"
                                + Utility.Hex(tempchar, 4));
                        break;
                    }



                    String title = fields[14];
                    tempchar = ch;
                    if (title.Length > 0)
                    {
                        tempchar = Convert.ToInt32(title, 16);
                    }
                    if (UChar.ToTitleCase(ch) != tempchar)
                    {
                        Errln("FAIL \\u" + Utility.Hex(ch, 4)
                                + " expected titlecase \\u"
                                + Utility.Hex(tempchar, 4));
                        break;
                    }
                }
                if (numErrors > 0)
                {
                    Warnln("Could not find unames.icu");
                }
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
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

            if (UnicodeBlock.Of(0x0041)
                                            != UnicodeBlock.Basic_Latin
                || UChar.GetIntPropertyValue(0x41, UProperty.Block)
                                  != UnicodeBlock.Basic_Latin.ID)
            {
                Errln("UCharacter.UnicodeBlock.of(\\u0041) property failed! "
                        + "Expected : "
                        + UnicodeBlock.Basic_Latin.ID + " got "
                        + UnicodeBlock.Of(0x0041));
            }

            // sanity check on repeated properties
            for (int ch = 0xfffe; ch <= 0x10ffff;)
            {
                UUnicodeCategory type = UChar.GetUnicodeCategory(ch);
                if (UChar.GetIntPropertyValue(ch,
                                                   UProperty.General_Category_Mask)
                    != (1 << type.ToInt32()))
                {
                    Errln("error: UCharacter.getIntPropertyValue(\\u"
                          + (ch).ToHexString()
                          + ", UProperty.GENERAL_CATEGORY_MASK) != "
                          + "getMask(getType())");
                }
                if (type != UUnicodeCategory.OtherNotAssigned)
                {
                    Errln("error: UCharacter.getType(\\u" + Utility.Hex(ch, 4)
                            + " != UCharacterCategory.UNASSIGNED (returns "
                            + UChar.GetUnicodeCategory(ch).AsString()
                            + ")");
                }
                if ((ch & 0xffff) == 0xfffe)
                {
                    ++ch;
                }
                else
                {
                    ch += 0xffff;
                }
            }

            // test that PUA is not "unassigned"
            for (int ch = 0xe000; ch <= 0x10fffd;)
            {
                int type = UChar.GetUnicodeCategory(ch).ToInt32();
                if (UChar.GetIntPropertyValue(ch,
                                                   UProperty.General_Category_Mask)
                    != (1 << type))
                {
                    Errln("error: UCharacter.getIntPropertyValue(\\u"
                          + (ch).ToHexString()
                          + ", UProperty.GENERAL_CATEGORY_MASK) != "
                          + "getMask(getType())");
                }

                if (type == UUnicodeCategory.OtherNotAssigned.ToInt32())
                {
                    Errln("error: UCharacter.getType(\\u"
                            + Utility.Hex(ch, 4)
                            + ") == UCharacterCategory.UNASSIGNED");
                }
                else if (type != UUnicodeCategory.PrivateUse.ToInt32())
                {
                    Logln("PUA override: UCharacter.getType(\\u"
                          + Utility.Hex(ch, 4) + ")=" + type);
                }
                if (ch == 0xf8ff)
                {
                    ch = 0xf0000;
                }
                else if (ch == 0xffffd)
                {
                    ch = 0x100000;
                }
                else
                {
                    ++ch;
                }
            }
        }


        /**
        * Test for the character names
        */
        [Test]
        public void TestNames()
        {
            try
            {
                int length = UCharacterName.Instance.MaxCharNameLength;
                if (length < 83)
                { // Unicode 3.2 max char name length
                    Errln("getMaxCharNameLength()=" + length + " is too short");
                }

                int[] c = {0x0061,                //LATIN SMALL LETTER A
                       0x000284,              //LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK
                       0x003401,              //CJK UNIFIED IDEOGRAPH-3401
                       0x007fed,              //CJK UNIFIED IDEOGRAPH-7FED
                       0x00ac00,              //HANGUL SYLLABLE GA
                       0x00d7a3,              //HANGUL SYLLABLE HIH
                       0x00d800, 0x00dc00,    //LINEAR B SYLLABLE B008 A
                       0xff08,                //FULLWIDTH LEFT PARENTHESIS
                       0x00ffe5,              //FULLWIDTH YEN SIGN
                       0x00ffff,              //null
                       0x0023456              //CJK UNIFIED IDEOGRAPH-23456
                       };
                String[] name = {
                             "LATIN SMALL LETTER A",
                             "LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK",
                             "CJK UNIFIED IDEOGRAPH-3401",
                             "CJK UNIFIED IDEOGRAPH-7FED",
                             "HANGUL SYLLABLE GA",
                             "HANGUL SYLLABLE HIH",
                             "",
                             "",
                             "FULLWIDTH LEFT PARENTHESIS",
                             "FULLWIDTH YEN SIGN",
                             "",
                             "CJK UNIFIED IDEOGRAPH-23456"
                             };
                String[] oldname = {"", "", "",
                            "",
                            "", "", "", "", "", "",
                            "", ""};
                String[] extendedname = {"LATIN SMALL LETTER A",
                                 "LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK",
                                 "CJK UNIFIED IDEOGRAPH-3401",
                                 "CJK UNIFIED IDEOGRAPH-7FED",
                                 "HANGUL SYLLABLE GA",
                                 "HANGUL SYLLABLE HIH",
                                 "<lead surrogate-D800>",
                                 "<trail surrogate-DC00>",
                                 "FULLWIDTH LEFT PARENTHESIS",
                                 "FULLWIDTH YEN SIGN",
                                 "<noncharacter-FFFF>",
                                 "CJK UNIFIED IDEOGRAPH-23456"};

                int size = c.Length;
                String str;
                int uc;

                for (int i = 0; i < size; i++)
                {
                    // modern Unicode character name
                    str = UChar.GetName(c[i]);
                    if ((str == null && name[i].Length > 0) ||
                        (str != null && !str.Equals(name[i])))
                    {
                        Errln("FAIL \\u" + Hex(c[i]) + " expected name " +
                                name[i]);
                        break;
                    }

                    // 1.0 Unicode character name
                    str = UChar.GetName1_0(c[i]);
                    if ((str == null && oldname[i].Length > 0) ||
                        (str != null && !str.Equals(oldname[i])))
                    {
                        Errln("FAIL \\u" + Hex(c[i]) + " expected 1.0 name " +
                                oldname[i]);
                        break;
                    }

                    // extended character name
                    str = UChar.GetExtendedName(c[i]);
                    if (str == null || !str.Equals(extendedname[i]))
                    {
                        Errln("FAIL \\u" + Hex(c[i]) + " expected extended name " +
                                extendedname[i]);
                        break;
                    }

                    // retrieving unicode character from modern name
                    uc = UChar.GetCharFromName(name[i]);
                    if (uc != c[i] && name[i].Length != 0)
                    {
                        Errln("FAIL " + name[i] + " expected character \\u" +
                              Hex(c[i]));
                        break;
                    }

                    //retrieving unicode character from 1.0 name
                    uc = UChar.GetCharFromName1_0(oldname[i]);
                    if (uc != c[i] && oldname[i].Length != 0)
                    {
                        Errln("FAIL " + oldname[i] + " expected 1.0 character \\u" +
                              Hex(c[i]));
                        break;
                    }

                    //retrieving unicode character from 1.0 name
                    uc = UChar.GetCharFromExtendedName(extendedname[i]);
                    if (uc != c[i] && i != 0 && (i == 1 || i == 6))
                    {
                        Errln("FAIL " + extendedname[i] +
                              " expected extended character \\u" + Hex(c[i]));
                        break;
                    }
                }

                // test getName works with mixed-case names (new in 2.0)
                if (0x61 != UChar.GetCharFromName("LATin smALl letTER A"))
                {
                    Errln("FAIL: 'LATin smALl letTER A' should result in character "
                          + "U+0061");
                }

                if (TestFmwk.GetExhaustiveness() >= 5)
                {
                    // extra testing different from icu
                    for (int i = UChar.MinValue; i < UChar.MaxValue; i++)
                    {
                        str = UChar.GetName(i);
                        if (str != null && UChar.GetCharFromName(str) != i)
                        {
                            Errln("FAIL \\u" + Hex(i) + " " + str +
                                                " retrieval of name and vice versa");
                            break;
                        }
                    }
                }

                // Test getCharNameCharacters
                if (TestFmwk.GetExhaustiveness() >= 10)
                {
                    bool[] map = new bool[256];

                    UnicodeSet set = new UnicodeSet(1, 0); // empty set
                    UnicodeSet dumb = new UnicodeSet(1, 0); // empty set

                    // uprv_getCharNameCharacters() will likely return more lowercase
                    // letters than actual character names contain because
                    // it includes all the characters in lowercased names of
                    // general categories, for the full possible set of extended names.
                    UCharacterName.Instance.GetCharNameCharacters(set);

                    // build set the dumb (but sure-fire) way
                    map.Fill(false);

                    int maxLength = 0;
                    for (int cp = 0; cp < 0x110000; ++cp)
                    {
                        String n = UChar.GetExtendedName(cp);
                        int len = n.Length;
                        if (len > maxLength)
                        {
                            maxLength = len;
                        }

                        for (int i = 0; i < len; ++i)
                        {
                            char ch = n[i];
                            if (!map[ch & 0xff])
                            {
                                dumb.Add(ch);
                                map[ch & 0xff] = true;
                            }
                        }
                    }

                    length = UCharacterName.Instance.MaxCharNameLength;
                    if (length != maxLength)
                    {
                        Errln("getMaxCharNameLength()=" + length
                              + " differs from the maximum length " + maxLength
                              + " of all extended names");
                    }

                    // compare the sets.  Where is my uset_equals?!!
                    bool ok = true;
                    for (int i = 0; i < 256; ++i)
                    {
                        if (set.Contains(i) != dumb.Contains(i))
                        {
                            if (0x61 <= i && i <= 0x7a // a-z
                                && set.Contains(i) && !dumb.Contains(i))
                            {
                                // ignore lowercase a-z that are in set but not in dumb
                                ok = true;
                            }
                            else
                            {
                                ok = false;
                                break;
                            }
                        }
                    }

                    String pattern1 = set.ToPattern(true);
                    String pattern2 = dumb.ToPattern(true);

                    if (!ok)
                    {
                        Errln("FAIL: getCharNameCharacters() returned " + pattern1
                              + " expected " + pattern2
                              + " (too many lowercase a-z are ok)");
                    }
                    else
                    {
                        Logln("Ok: getCharNameCharacters() returned " + pattern1);
                    }
                }
                // improve code coverage
                String expected = "LATIN SMALL LETTER A|LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK|" +
                                  "CJK UNIFIED IDEOGRAPH-3401|CJK UNIFIED IDEOGRAPH-7FED|HANGUL SYLLABLE GA|" +
                                  "HANGUL SYLLABLE HIH|LINEAR B SYLLABLE B008 A|FULLWIDTH LEFT PARENTHESIS|" +
                                  "FULLWIDTH YEN SIGN|" +
                                  "null|" + // getName returns null because 0xFFFF does not have a name, but has an extended name!
                                  "CJK UNIFIED IDEOGRAPH-23456";
                String separator = "|";
                String source = Utility.ValueOf(c);
                String result = UChar.GetName(source, separator);
                if (!result.Equals(expected))
                {
                    Errln("UCharacter.getName did not return the expected result.\n\t Expected: " + expected + "\n\t Got: " + result);
                }

            }
            catch (ArgumentException e)
            {
                if (e.Message.IndexOf("unames.icu", StringComparison.Ordinal) >= 0)
                {
                    Warnln("Could not find unames.icu");
                }
                else
                {
                    throw e;
                }
            }

        }

        [Test]
        public void TestUCharFromNameUnderflow()
        {
            // Ticket #10889: Underflow crash when there is no dash.
            int c = UChar.GetCharFromExtendedName("<NO BREAK SPACE>");
            if (c >= 0)
            {
                Errln("UCharacter.getCharFromExtendedName(<NO BREAK SPACE>) = U+" + Hex(c) +
                        " but should fail (-1)");
            }

            // Test related edge cases.
            c = UChar.GetCharFromExtendedName("<-00a0>");
            if (c >= 0)
            {
                Errln("UCharacter.getCharFromExtendedName(<-00a0>) = U+" + Hex(c) +
                        " but should fail (-1)");
            }

            c = UChar.GetCharFromExtendedName("<control->");
            if (c >= 0)
            {
                Errln("UCharacter.getCharFromExtendedName(<control->) = U+" + Hex(c) +
                        " but should fail (-1)");
            }

            c = UChar.GetCharFromExtendedName("<control-111111>");
            if (c >= 0)
            {
                Errln("UCharacter.getCharFromExtendedName(<control-111111>) = U+" + Hex(c) +
                        " but should fail (-1)");
            }
        }

        /**
        * Testing name iteration
        */
        [Test]
        public void TestNameIteration()
        {
            try
            {
                IValueEnumerator iterator = UChar.GetExtendedNameEnumerator();
                ValueEnumeratorElement old = new ValueEnumeratorElement();
                // testing subrange
                iterator.SetRange(-10, -5);
                if (iterator.MoveNext())
                {
                    Errln("Fail, expected iterator to return false when range is set outside the meaningful range");
                }
                iterator.SetRange(0x110000, 0x111111);
                if (iterator.MoveNext())
                {
                    Errln("Fail, expected iterator to return false when range is set outside the meaningful range");
                }
                try
                {
                    iterator.SetRange(50, 10);
                    Errln("Fail, expected exception when encountered invalid range");
                }
                catch (Exception e)
                {
                }

                iterator.SetRange(-10, 10);
                if (!iterator.MoveNext() || iterator.Current.Integer != 0)
                {
                    Errln("Fail, expected iterator to return 0 when range start limit is set outside the meaningful range");
                }

                iterator.SetRange(0x10FFFE, 0x200000);
                int last = 0;
                while (iterator.MoveNext())
                {
                    last = iterator.Current.Integer;
                }
                if (last != 0x10FFFF)
                {
                    Errln("Fail, expected iterator to return 0x10FFFF when range end limit is set outside the meaningful range");
                }

                iterator = UChar.GetNameEnumerator();
                iterator.SetRange(0xF, 0x45);
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Integer <= old.Integer)
                    {
                        Errln("FAIL next returned a less codepoint \\u" +
                            (iterator.Current.Integer).ToHexString() + " than \\u" +
                            (old.Integer).ToHexString());
                        break;
                    }
                    if (!UChar.GetName(iterator.Current.Integer).Equals(iterator.Current.Value))
                    {
                        Errln("FAIL next codepoint \\u" +
                            (iterator.Current.Integer).ToHexString() +
                            " does not have the expected name " +
                            UChar.GetName(iterator.Current.Integer) +
                            " instead have the name " + (String)iterator.Current.Value);
                        break;
                    }
                    old.Integer = iterator.Current.Integer;
                }

                iterator.Reset();
                iterator.MoveNext();
                if (iterator.Current.Integer != 0x20)
                {
                    Errln("FAIL reset in iterator");
                }

                iterator.SetRange(0, 0x110000);
                old.Integer = 0;
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Integer != 0 && iterator.Current.Integer <= old.Integer)
                    {
                        Errln("FAIL next returned a less codepoint \\u" +
                            (iterator.Current.Integer).ToHexString() + " than \\u" +
                            (old.Integer).ToHexString());
                        break;
                    }
                    if (!UChar.GetName(iterator.Current.Integer).Equals(iterator.Current.Value))
                    {
                        Errln("FAIL next codepoint \\u" +
                                (iterator.Current.Integer).ToHexString() +
                                " does not have the expected name " +
                                UChar.GetName(iterator.Current.Integer) +
                                " instead have the name " + (String)iterator.Current.Value);
                        break;
                    }
                    for (int i = old.Integer + 1; i < iterator.Current.Integer; i++)
                    {
                        if (UChar.GetName(i) != null)
                        {
                            Errln("FAIL between codepoints are not null \\u" +
                                    (old.Integer).ToHexString() + " and " +
                                    (iterator.Current.Integer).ToHexString() + " has " +
                                    (i).ToHexString() + " with a name " +
                                    UChar.GetName(i));
                            break;
                        }
                    }
                    old.Integer = iterator.Current.Integer;
                }

                iterator = UChar.GetExtendedNameEnumerator();
                old.Integer = 0;
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Integer != 0 && iterator.Current.Integer != old.Integer)
                    {
                        Errln("FAIL next returned a codepoint \\u" +
                                (iterator.Current.Integer).ToHexString() +
                                " different from \\u" +
                                (old.Integer).ToHexString());
                        break;
                    }
                    if (!UChar.GetExtendedName(iterator.Current.Integer).Equals(
                                                                  iterator.Current.Value))
                    {
                        Errln("FAIL next codepoint \\u" +
                            (iterator.Current.Integer).ToHexString() +
                            " name should be "
                            + UChar.GetExtendedName(iterator.Current.Integer) +
                            " instead of " + (String)iterator.Current.Value);
                        break;
                    }
                    old.Integer++;
                }
                iterator = UChar.GetName1_0Enumerator();
                old.Integer = 0;
                while (iterator.MoveNext())
                {
                    Logln((iterator.Current.Integer).ToHexString() + " " +
                                                            (String)iterator.Current.Value);
                    if (iterator.Current.Integer != 0 && iterator.Current.Integer <= old.Integer)
                    {
                        Errln("FAIL next returned a less codepoint \\u" +
                            (iterator.Current.Integer).ToHexString() + " than \\u" +
                            (old.Integer).ToHexString());
                        break;
                    }
                    if (!iterator.Current.Value.Equals(UChar.GetName1_0(
                                                                iterator.Current.Integer)))
                    {
                        Errln("FAIL next codepoint \\u" +
                                (iterator.Current.Integer).ToHexString() +
                                " name cannot be null");
                        break;
                    }
                    for (int i = old.Integer + 1; i < iterator.Current.Integer; i++)
                    {
                        if (UChar.GetName1_0(i) != null)
                        {
                            Errln("FAIL between codepoints are not null \\u" +
                                (old.Integer).ToHexString() + " and " +
                                (iterator.Current.Integer).ToHexString() + " has " +
                                (i).ToHexString() + " with a name " +
                                UChar.GetName1_0(i));
                            break;
                        }
                    }
                    old.Integer = iterator.Current.Integer;
                }
            }
            catch (Exception e)
            {
                // !!! wouldn't preflighting be simpler?  This looks like
                // it is effectively be doing that.  It seems that for every
                // true error the code will call errln, which will throw the error, which
                // this will catch, which this will then rethrow the error.  Just seems
                // cumbersome.
                if (e.Message.IndexOf("unames.icu", StringComparison.Ordinal) >= 0)
                {
                    Warnln("Could not find unames.icu");
                }
                else
                {
                    Errln(e.ToString());
                }
            }
        }

        /**
        * Testing the for illegal characters
        */
        [Test]
        public void TestIsLegal()
        {
            int[] illegal = {0xFFFE, 0x00FFFF, 0x005FFFE, 0x005FFFF, 0x0010FFFE,
                         0x0010FFFF, 0x110000, 0x00FDD0, 0x00FDDF, 0x00FDE0,
                         0x00FDEF, 0xD800, 0xDC00, -1};
            int[] legal = {0x61, 0x00FFFD, 0x0010000, 0x005FFFD, 0x0060000,
                       0x0010FFFD, 0xFDCF, 0x00FDF0};
            for (int count = 0; count < illegal.Length; count++)
            {
                if (UChar.IsLegal(illegal[count]))
                {
                    Errln("FAIL \\u" + Hex(illegal[count]) +
                            " is not a legal character");
                }
            }

            for (int count = 0; count < legal.Length; count++)
            {
                if (!UChar.IsLegal(legal[count]))
                {
                    Errln("FAIL \\u" + Hex(legal[count]) +
                                                       " is a legal character");
                }
            }

            String illegalStr = "This is an illegal string ";
            String legalStr = "This is a legal string ";

            for (int count = 0; count < illegal.Length; count++)
            {
                StringBuffer str = new StringBuffer(illegalStr);
                if (illegal[count] < 0x10000)
                {
                    str.Append((char)illegal[count]);
                }
                else
                {
                    char lead = UTF16.GetLeadSurrogate(illegal[count]);
                    char trail = UTF16.GetTrailSurrogate(illegal[count]);
                    str.Append(lead);
                    str.Append(trail);
                }
                if (UChar.IsLegal(str.ToString()))
                {
                    Errln("FAIL " + Hex(str.ToString()) +
                          " is not a legal string");
                }
            }

            for (int count = 0; count < legal.Length; count++)
            {
                StringBuffer str = new StringBuffer(legalStr);
                if (legal[count] < 0x10000)
                {
                    str.Append((char)legal[count]);
                }
                else
                {
                    char lead = UTF16.GetLeadSurrogate(legal[count]);
                    char trail = UTF16.GetTrailSurrogate(legal[count]);
                    str.Append(lead);
                    str.Append(trail);
                }
                if (!UChar.IsLegal(str.ToString()))
                {
                    Errln("FAIL " + Hex(str.ToString()) + " is a legal string");
                }
            }
        }

        /**
         * Test getCodePoint
         */
        [Test]
        public void TestCodePoint()
        {
            int ch = 0x10000;
            for (char i = (char)0xD800; i < 0xDC00; i++)
            {
                for (char j = (char)0xDC00; j <= 0xDFFF; j++)
                {
                    if (UChar.ConvertToUtf32(i, j) != ch)
                    {
                        Errln("Error getting codepoint for surrogate " +
                              "characters \\u"
                              + (i).ToHexString() + " \\u" +
                              (j).ToHexString());
                    }
                    ch++;
                }
            }
            try
            {
                UChar.ConvertToUtf32((char)0xD7ff, (char)0xDC00);
                Errln("Invalid surrogate characters should not form a " +
                      "supplementary");
            }
            catch (Exception e)
            {
            }
            for (char i = (char)0; i < 0xFFFF; i++)
            {
                if (i == 0xFFFE ||
                    (i >= 0xD800 && i <= 0xDFFF) ||
                    (i >= 0xFDD0 && i <= 0xFDEF))
                {
                    // not a character
                    try
                    {
                        UChar.ConvertToUtf32(i);
                        Errln("Not a character is not a valid codepoint");
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    if (UChar.ConvertToUtf32(i) != i)
                    {
                        Errln("A valid codepoint should return itself");
                    }
                }
            }
        }

        /**
        * This method is a little different from the type test in icu4c.
        * But combined with testUnicodeData, they basically do the same thing.
*/
        [Test]
        public void TestIteration()
        {
            int limit = 0;
            int prevtype = -1;
            int shouldBeDir;
            int[][] test ={new int[] {0x41, UUnicodeCategory.UppercaseLetter.ToInt32()},
                        new int[] {0x308, UUnicodeCategory.NonSpacingMark.ToInt32()},
                        new int[] {0xfffe, UUnicodeCategory.OtherNotAssigned.ToInt32()},
                        new int[] {0xe0041, UUnicodeCategory.Format.ToInt32()},
                        new int[] {0xeffff, UUnicodeCategory.OtherNotAssigned.ToInt32()}};

            // default Bidi classes for unassigned code points, from the DerivedBidiClass.txt header
            int[][] defaultBidi ={
                new int[] { 0x0590, UCharacterDirection.LeftToRight.ToInt32() },
                new int[] { 0x0600, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x07C0, UCharacterDirection.RightToLeftArabic.ToInt32() },
                new int[] { 0x0860, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x0870, UCharacterDirection.RightToLeftArabic.ToInt32() },  // Unicode 10 changes U+0860..U+086F from R to AL.
                new int[] { 0x08A0, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x0900, UCharacterDirection.RightToLeftArabic.ToInt32() },  /* Unicode 6.1 changes U+08A0..U+08FF from R to AL */
                new int[] { 0x20A0, UCharacterDirection.LeftToRight.ToInt32() },
                new int[] { 0x20D0, UCharacterDirection.EuropeanNumberTerminator.ToInt32() },  /* Unicode 6.3 changes the currency symbols block U+20A0..U+20CF to default to ET not L */
                new int[] { 0xFB1D, UCharacterDirection.LeftToRight.ToInt32() },
                new int[] { 0xFB50, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0xFE00, UCharacterDirection.RightToLeftArabic.ToInt32() },
                new int[] { 0xFE70, UCharacterDirection.LeftToRight.ToInt32() },
                new int[] { 0xFF00, UCharacterDirection.RightToLeftArabic.ToInt32() },
                new int[] { 0x10800, UCharacterDirection.LeftToRight.ToInt32() },
                new int[] { 0x11000, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x1E800, UCharacterDirection.LeftToRight.ToInt32() },  /* new default-R range in Unicode 5.2: U+1E800 - U+1EFFF */
                new int[] { 0x1EE00, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x1EF00, UCharacterDirection.RightToLeftArabic.ToInt32() },  /* Unicode 6.1 changes U+1EE00..U+1EEFF from R to AL */
                new int[] { 0x1F000, UCharacterDirection.RightToLeft.ToInt32() },
                new int[] { 0x110000, UCharacterDirection.LeftToRight.ToInt32() }
            };

            IRangeValueEnumerator iterator = UChar.GetTypeEnumerator();
            RangeValueEnumeratorElement result;
            while (iterator.MoveNext())
            {
                result = iterator.Current;
                if (result.Start != limit)
                {
                    Errln("UCharacterIteration failed: Ranges not continuous " +
                            "0x" + (result.Start).ToHexString());
                }

                limit = result.Limit;
                if (result.Value == prevtype)
                {
                    Errln("Type of the next set of enumeration should be different");
                }
                prevtype = result.Value;

                for (int i = result.Start; i < limit; i++)
                {
                    int temptype = UChar.GetUnicodeCategory(i).ToInt32();
                    if (temptype != result.Value)
                    {
                        Errln("UCharacterIteration failed: Codepoint \\u" +
                                (i).ToHexString() + " should be of type " +
                                temptype + " not " + result.Value);
                    }
                }

                for (int i = 0; i < test.Length; ++i)
                {
                    if (result.Start <= test[i][0] && test[i][0] < result.Limit)
                    {
                        if (result.Value != test[i][1])
                        {
                            Errln("error: getTypes() has range ["
                                  + (result.Start).ToHexString() + ", "
                                  + (result.Limit).ToHexString()
                                  + "] with type " + result.Value
                                  + " instead of ["
                                  + (test[i][0]).ToHexString() + ", "
                                  + (test[i][1]).ToHexString());
                        }
                    }
                }

                // LineBreak.txt specifies:
                //   #  - Assigned characters that are not listed explicitly are given the value
                //   #    "AL".
                //   #  - Unassigned characters are given the value "XX".
                //
                // PUA characters are listed explicitly with "XX".
                // Verify that no assigned character has "XX".
                if (result.Value != UUnicodeCategory.OtherNotAssigned.ToInt32()
                    && result.Value != UUnicodeCategory.PrivateUse.ToInt32())
                {
                    int c = result.Start;
                    while (c < result.Limit)
                    {
                        if (0 == UChar.GetIntPropertyValue(c,
                                                    UProperty.Line_Break))
                        {
                            Logln("error UProperty.LINE_BREAK(assigned \\u"
                                  + Utility.Hex(c, 4) + ")=XX");
                        }
                        ++c;
                    }
                }

                /*
                 * Verify default Bidi classes.
                 * See DerivedBidiClass.txt, especially for unassigned code points.
                 */
                if (result.Value == UUnicodeCategory.OtherNotAssigned.ToInt32()
                    || result.Value == UUnicodeCategory.PrivateUse.ToInt32())
                {
                    int c = result.Start;
                    for (int i = 0; i < defaultBidi.Length && c < result.Limit;
                         ++i)
                    {
                        if (c < defaultBidi[i][0])
                        {
                            while (c < result.Limit && c < defaultBidi[i][0])
                            {
                                // TODO change to public UCharacter.isNonCharacter(c) once it's available
                                if (UCharacterUtility.IsNonCharacter(c) || UChar.HasBinaryProperty(c, UProperty.Default_Ignorable_Code_Point))
                                {
                                    shouldBeDir = UCharacterDirection.BoundaryNeutral.ToInt32();
                                }
                                else
                                {
                                    shouldBeDir = defaultBidi[i][1];
                                }


                                if (UChar.GetDirection(c).ToInt32() != shouldBeDir
                                    || UChar.GetIntPropertyValue(c,
                                                              UProperty.Bidi_Class)
                                       != shouldBeDir)
                                {
                                    Errln("error: getDirection(unassigned/PUA "
                                          + (c).ToHexString()
                                          + ") should be "
                                          + shouldBeDir);
                                }
                                ++c;
                            }
                        }
                    }
                }
            }

            iterator.Reset();
            if (iterator.MoveNext() == false || iterator.Current.Start != 0)
            {
                Console.Out.WriteLine("result " + iterator.Current.Start);
                Errln("UCharacterIteration reset() failed");
            }
        }

        /**
         * Testing getAge
         */
        [Test]
        public void TestGetAge()
        {
            int[] ages = {0x41,    1, 1, 0, 0,
                      0xffff,  1, 1, 0, 0,
                      0x20ab,  2, 0, 0, 0,
                      0x2fffe, 2, 0, 0, 0,
                      0x20ac,  2, 1, 0, 0,
                      0xfb1d,  3, 0, 0, 0,
                      0x3f4,   3, 1, 0, 0,
                      0x10300, 3, 1, 0, 0,
                      0x220,   3, 2, 0, 0,
                      0xff60,  3, 2, 0, 0};
            for (int i = 0; i < ages.Length; i += 5)
            {
                VersionInfo age = UChar.GetAge(ages[i]);
                if (age != VersionInfo.GetInstance(ages[i + 1], ages[i + 2],
                                                   ages[i + 3], ages[i + 4]))
                {
                    Errln("error: getAge(\\u" + (ages[i]).ToHexString() +
                          ") == " + age.ToString() + " instead of " +
                          ages[i + 1] + "." + ages[i + 2] + "." + ages[i + 3] +
                          "." + ages[i + 4]);
                }
            }

            int[] valid_tests = {
                UChar.MinValue, UChar.MinValue+1,
                UChar.MaxValue-1, UChar.MaxValue};
            int[] invalid_tests = {
                UChar.MinValue-1, UChar.MinValue-2,
                UChar.MaxValue+1, UChar.MaxValue+2};

            for (int i = 0; i < valid_tests.Length; i++)
            {
                try
                {
                    UChar.GetAge(valid_tests[i]);
                }
                catch (Exception e)
                {
                    Errln("UCharacter.getAge(int) was not suppose to have " +
                            "an exception. Value passed: " + valid_tests[i]);
                }
            }

            for (int i = 0; i < invalid_tests.Length; i++)
            {
                try
                {
                    UChar.GetAge(invalid_tests[i]);
                    Errln("UCharacter.getAge(int) was suppose to have " +
                            "an exception. Value passed: " + invalid_tests[i]);
                }
                catch (Exception e)
                {
                }
            }
        }

        /**
         * Test binary non core properties
         */
        [Test]
        public void TestAdditionalProperties()
        {
            int FALSE = 0;
            int TRUE = 1;
            // test data for hasBinaryProperty()
            int[][] props = { // code point, property
                new int[] { 0x0627, (int)UProperty.Alphabetic, 1 },
                new int[] { 0x1034a, (int)UProperty.Alphabetic, 1 },
                new int[] { 0x2028, (int)UProperty.Alphabetic, 0 },

                new int[] { 0x0066, (int)UProperty.ASCII_Hex_Digit, 1 },
                new int[] { 0x0067, (int)UProperty.ASCII_Hex_Digit, 0 },

                new int[] { 0x202c, (int)UProperty.Bidi_Control, 1 },
                new int[] { 0x202f, (int)UProperty.Bidi_Control, 0 },

                new int[] { 0x003c, (int)UProperty.Bidi_Mirrored, 1 },
                new int[] { 0x003d, (int)UProperty.Bidi_Mirrored, 0 },

                /* see Unicode Corrigendum #6 at http://www.unicode.org/versions/corrigendum6.html */
                new int[] { 0x2018, (int)UProperty.Bidi_Mirrored, 0 },
                new int[] { 0x201d, (int)UProperty.Bidi_Mirrored, 0 },
                new int[] { 0x201f, (int)UProperty.Bidi_Mirrored, 0 },
                new int[] { 0x301e, (int)UProperty.Bidi_Mirrored, 0 },

                new int[] { 0x058a, (int)UProperty.Dash, 1 },
                new int[] { 0x007e, (int)UProperty.Dash, 0 },

                new int[] { 0x0c4d, (int)UProperty.Diacritic, 1 },
                new int[] { 0x3000, (int)UProperty.Diacritic, 0 },

                new int[] { 0x0e46, (int)UProperty.Extender, 1 },
                new int[] { 0x0020, (int)UProperty.Extender, 0 },

                new int[] { 0xfb1d, (int)UProperty.Full_Composition_Exclusion, 1 },
                new int[] { 0x1d15f, (int)UProperty.Full_Composition_Exclusion, 1 },
                new int[] { 0xfb1e, (int)UProperty.Full_Composition_Exclusion, 0 },

                new int[] { 0x110a, (int)UProperty.NFD_Inert, 1 },      /* Jamo L */
                new int[] { 0x0308, (int)UProperty.NFD_Inert, 0 },

                new int[] { 0x1164, (int)UProperty.NFKD_Inert, 1 },     /* Jamo V */
                new int[] { 0x1d79d, (int)UProperty.NFKD_Inert, 0 },   /* math compat version of xi */

                new int[] { 0x0021, (int)UProperty.NFC_Inert, 1 },      /* ! */
                new int[] { 0x0061, (int)UProperty.NFC_Inert, 0 },     /* a */
                new int[] { 0x00e4, (int)UProperty.NFC_Inert, 0 },     /* a-umlaut */
                new int[] { 0x0102, (int)UProperty.NFC_Inert, 0 },     /* a-breve */
                new int[] { 0xac1c, (int)UProperty.NFC_Inert, 0 },     /* Hangul LV */
                new int[] { 0xac1d, (int)UProperty.NFC_Inert, 1 },      /* Hangul LVT */

                new int[] { 0x1d79d, (int)UProperty.NFKC_Inert, 0 },   /* math compat version of xi */
                new int[] { 0x2a6d6, (int)UProperty.NFKC_Inert, 1 },    /* Han, last of CJK ext. B */

                new int[] { 0x00e4, (int)UProperty.Segment_Starter, 1 },
                new int[] { 0x0308, (int)UProperty.Segment_Starter, 0 },
                new int[] { 0x110a, (int)UProperty.Segment_Starter, 1 }, /* Jamo L */
                new int[] { 0x1164, (int)UProperty.Segment_Starter, 0 },/* Jamo V */
                new int[] { 0xac1c, (int)UProperty.Segment_Starter, 1 }, /* Hangul LV */
                new int[] { 0xac1d, (int)UProperty.Segment_Starter, 1 }, /* Hangul LVT */

                new int[] { 0x0044, (int)UProperty.Hex_Digit, 1 },
                new int[] { 0xff46, (int)UProperty.Hex_Digit, 1 },
                new int[] { 0x0047, (int)UProperty.Hex_Digit, 0 },

                new int[] { 0x30fb, (int)UProperty.Hyphen, 1 },
                new int[] { 0xfe58, (int)UProperty.Hyphen, 0 },

                new int[] { 0x2172, (int)UProperty.ID_Continue, 1 },
                new int[] { 0x0307, (int)UProperty.ID_Continue, 1 },
                new int[] { 0x005c, (int)UProperty.ID_Continue, 0 },

                new int[] { 0x2172, (int)UProperty.ID_Start, 1 },
                new int[] { 0x007a, (int)UProperty.ID_Start, 1 },
                new int[] { 0x0039, (int)UProperty.ID_Start, 0 },

                new int[] { 0x4db5, (int)UProperty.Ideographic, 1 },
                new int[] { 0x2f999, (int)UProperty.Ideographic, 1 },
                new int[] { 0x2f99, (int)UProperty.Ideographic, 0 },

                new int[] { 0x200c, (int)UProperty.Join_Control, 1 },
                new int[] { 0x2029, (int)UProperty.Join_Control, 0 },

                new int[] { 0x1d7bc, (int)UProperty.Lowercase, 1 },
                new int[] { 0x0345, (int)UProperty.Lowercase, 1 },
                new int[] { 0x0030, (int)UProperty.Lowercase, 0 },

                new int[] { 0x1d7a9, (int)UProperty.Math, 1 },
                new int[] { 0x2135, (int)UProperty.Math, 1 },
                new int[] { 0x0062, (int)UProperty.Math, 0 },

                new int[] { 0xfde1, (int)UProperty.Noncharacter_Code_Point, 1 },
                new int[] { 0x10ffff, (int)UProperty.Noncharacter_Code_Point, 1 },
                new int[] { 0x10fffd, (int)UProperty.Noncharacter_Code_Point, 0 },

                new int[] { 0x0022, (int)UProperty.Quotation_Mark, 1 },
                new int[] { 0xff62, (int)UProperty.Quotation_Mark, 1 },
                new int[] { 0xd840, (int)UProperty.Quotation_Mark, 0 },

                new int[] { 0x061f, (int)UProperty.Terminal_Punctuation, 1 },
                new int[] { 0xe003f, (int)UProperty.Terminal_Punctuation, 0 },

                new int[] { 0x1d44a, (int)UProperty.Uppercase, 1 },
                new int[] { 0x2162, (int)UProperty.Uppercase, 1 },
                new int[] { 0x0345, (int)UProperty.Uppercase, 0 },

                new int[] { 0x0020, (int)UProperty.White_Space, 1 },
                new int[] { 0x202f, (int)UProperty.White_Space, 1 },
                new int[] { 0x3001, (int)UProperty.White_Space, 0 },

                new int[] { 0x0711, (int)UProperty.XID_Continue, 1 },
                new int[] { 0x1d1aa, (int)UProperty.XID_Continue, 1 },
                new int[] { 0x007c, (int)UProperty.XID_Continue, 0 },

                new int[] { 0x16ee, (int)UProperty.XID_Start, 1 },
                new int[] { 0x23456, (int)UProperty.XID_Start, 1 },
                new int[] { 0x1d1aa, (int)UProperty.XID_Start, 0 },

                /*
                 * Version break:
                 * The following properties are only supported starting with the
                 * Unicode version indicated in the second field.
                 */
                new int[] { -1, 0x320, 0 },

                new int[] { 0x180c, (int)UProperty.Default_Ignorable_Code_Point, 1 },
                new int[] { 0xfe02, (int)UProperty.Default_Ignorable_Code_Point, 1 },
                new int[] { 0x1801, (int)UProperty.Default_Ignorable_Code_Point, 0 },

                new int[] { 0x0149, (int)UProperty.Deprecated, 1 },         /* changed in Unicode 5.2 */
                new int[] { 0x0341, (int)UProperty.Deprecated, 0 },        /* changed in Unicode 5.2 */
                new int[] { 0xe0001, (int)UProperty.Deprecated, 1 },       /* Changed from Unicode 5 to 5.1 */
                new int[] { 0xe0100, (int)UProperty.Deprecated, 0 },

                new int[] { 0x00a0, (int)UProperty.Grapheme_Base, 1 },
                new int[] { 0x0a4d, (int)UProperty.Grapheme_Base, 0 },
                new int[] { 0xff9d, (int)UProperty.Grapheme_Base, 1 },
                new int[] { 0xff9f, (int)UProperty.Grapheme_Base, 0 },      /* changed from Unicode 3.2 to 4  and again 5 to 5.1 */

                new int[] { 0x0300, (int)UProperty.Grapheme_Extend, 1 },
                new int[] { 0xff9d, (int)UProperty.Grapheme_Extend, 0 },
                new int[] { 0xff9f, (int)UProperty.Grapheme_Extend, 1 },   /* changed from Unicode 3.2 to 4 and again 5 to 5.1 */
                new int[] { 0x0603, (int)UProperty.Grapheme_Extend, 0 },

                new int[] { 0x0a4d, (int)UProperty.Grapheme_Link, 1 },
                new int[] { 0xff9f, (int)UProperty.Grapheme_Link, 0 },

                new int[] { 0x2ff7, (int)UProperty.IDS_Binary_Operator, 1 },
                new int[] { 0x2ff3, (int)UProperty.IDS_Binary_Operator, 0 },

                new int[] { 0x2ff3, (int)UProperty.IDS_Trinary_Operator, 1 },
                new int[] { 0x2f03, (int)UProperty.IDS_Trinary_Operator, 0 },

                new int[] { 0x0ec1, (int)UProperty.Logical_Order_Exception, 1 },
                new int[] { 0xdcba, (int)UProperty.Logical_Order_Exception, 0 },

                new int[] { 0x2e9b, (int)UProperty.Radical, 1 },
                new int[] { 0x4e00, (int)UProperty.Radical, 0 },

                new int[] { 0x012f, (int)UProperty.Soft_Dotted, 1 },
                new int[] { 0x0049, (int)UProperty.Soft_Dotted, 0 },

                new int[] { 0xfa11, (int)UProperty.Unified_Ideograph, 1 },
                new int[] { 0xfa12, (int)UProperty.Unified_Ideograph, 0 },

                new int[] { -1, 0x401, 0 }, /* version break for Unicode 4.0.1 */

                new int[] { 0x002e, (int)UProperty.STerm, 1 },
                new int[] { 0x0061, (int)UProperty.STerm, 0 },

                new int[] { 0x180c, (int)UProperty.Variation_Selector, 1 },
                new int[] { 0xfe03, (int)UProperty.Variation_Selector, 1 },
                new int[] { 0xe01ef, (int)UProperty.Variation_Selector, 1 },
                new int[] { 0xe0200, (int)UProperty.Variation_Selector, 0 },

                /* enum/integer type properties */
                /* test default Bidi classes for unassigned code points */
                new int[] { 0x0590, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x05cf, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x05ed, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x07f2, (int)UProperty.Bidi_Class, (int)UCharacterDirection.DirNonSpacingMark }, /* Nko, new in Unicode 5.0 */
                new int[] { 0x07fe, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft }, /* unassigned R */
                new int[] { 0x089f, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0xfb37, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0xfb42, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x10806, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x10909, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x10fe4, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },

                new int[] { 0x061d, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0x063f, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0x070e, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0x0775, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0xfbc2, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0xfd90, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0xfefe, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },

                new int[] { 0x02AF, (int)UProperty.Block, UnicodeBlock.IPA_Extensions.ID },
                new int[] { 0x0C4E, (int)UProperty.Block, UnicodeBlock.Telugu.ID },
                new int[] { 0x155A, (int)UProperty.Block, UnicodeBlock.Unified_Canadian_Aboriginal_Syllabics.ID },
                new int[] { 0x1717, (int)UProperty.Block, UnicodeBlock.Tagalog.ID },
                new int[] { 0x1900, (int)UProperty.Block, UnicodeBlock.Limbu.ID },
                new int[] { 0x1CBF, (int)UProperty.Block, UnicodeBlock.NoBlock.ID},
                new int[] { 0x3040, (int)UProperty.Block, UnicodeBlock.Hiragana.ID},
                new int[] { 0x1D0FF, (int)UProperty.Block, UnicodeBlock.Byzantine_Musical_Symbols.ID},
                new int[] { 0x50000, (int)UProperty.Block, UnicodeBlock.NoBlock.ID },
                new int[] { 0xEFFFF, (int)UProperty.Block, UnicodeBlock.NoBlock.ID },
                new int[] { 0x10D0FF, (int)UProperty.Block, UnicodeBlock.Supplementary_Private_Use_Area_B.ID },

                /* (int)UProperty.CANONICAL_COMBINING_CLASS tested for assigned characters in TestUnicodeData() */
                new int[] { 0xd7d7, (int)UProperty.Canonical_Combining_Class, 0 },

                new int[] { 0x00A0, (int)UProperty.Decomposition_Type, DecompositionType.NoBreak },
                new int[] { 0x00A8, (int)UProperty.Decomposition_Type, DecompositionType.Compat },
                new int[] { 0x00bf, (int)UProperty.Decomposition_Type, DecompositionType.None },
                new int[] { 0x00c0, (int)UProperty.Decomposition_Type, DecompositionType.Canonical },
                new int[] { 0x1E9B, (int)UProperty.Decomposition_Type, DecompositionType.Canonical },
                new int[] { 0xBCDE, (int)UProperty.Decomposition_Type, DecompositionType.Canonical },
                new int[] { 0xFB5D, (int)UProperty.Decomposition_Type, DecompositionType.Medial },
                new int[] { 0x1D736, (int)UProperty.Decomposition_Type, DecompositionType.Font },
                new int[] { 0xe0033, (int)UProperty.Decomposition_Type, DecompositionType.None },

                new int[] { 0x0009, (int)UProperty.East_Asian_Width, EastAsianWidth.Neutral },
                new int[] { 0x0020, (int)UProperty.East_Asian_Width, EastAsianWidth.Narrow },
                new int[] { 0x00B1, (int)UProperty.East_Asian_Width, EastAsianWidth.Ambiguous },
                new int[] { 0x20A9, (int)UProperty.East_Asian_Width, EastAsianWidth.HalfWidth },
                new int[] { 0x2FFB, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0x3000, (int)UProperty.East_Asian_Width, EastAsianWidth.FullWidth },
                new int[] { 0x35bb, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0x58bd, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0xD7A3, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0xEEEE, (int)UProperty.East_Asian_Width, EastAsianWidth.Ambiguous },
                new int[] { 0x1D198, (int)UProperty.East_Asian_Width, EastAsianWidth.Neutral },
                new int[] { 0x20000, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0x2F8C7, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0x3a5bd, (int)UProperty.East_Asian_Width, EastAsianWidth.Wide },
                new int[] { 0x5a5bd, (int)UProperty.East_Asian_Width, EastAsianWidth.Neutral },
                new int[] { 0xFEEEE, (int)UProperty.East_Asian_Width, EastAsianWidth.Ambiguous },
                new int[] { 0x10EEEE, (int)UProperty.East_Asian_Width, EastAsianWidth.Ambiguous },

                /* (int)UProperty.GENERAL_CATEGORY tested for assigned characters in TestUnicodeData() */
                new int[] { 0xd7c7, (int)UProperty.General_Category, 0 },
                new int[] { 0xd7d7, (int)UProperty.General_Category, UUnicodeCategory.OtherLetter.ToInt32() },     /* changed in Unicode 5.2 */

                new int[] { 0x0444, (int)UProperty.Joining_Group, JoiningGroup.NoJoiningGroup },
                new int[] { 0x0639, (int)UProperty.Joining_Group, JoiningGroup.Ain },
                new int[] { 0x072A, (int)UProperty.Joining_Group, JoiningGroup.DalathRish },
                new int[] { 0x0647, (int)UProperty.Joining_Group, JoiningGroup.Heh },
                new int[] { 0x06C1, (int)UProperty.Joining_Group, JoiningGroup.HehGoal },

                new int[] { 0x200C, (int)UProperty.Joining_Type, JoiningType.NonJoining },
                new int[] { 0x200D, (int)UProperty.Joining_Type, JoiningType.JoinCausing },
                new int[] { 0x0639, (int)UProperty.Joining_Type, JoiningType.DualJoining },
                new int[] { 0x0640, (int)UProperty.Joining_Type, JoiningType.JoinCausing },
                new int[] { 0x06C3, (int)UProperty.Joining_Type, JoiningType.RightJoining },
                new int[] { 0x0300, (int)UProperty.Joining_Type, JoiningType.Transparent },
                new int[] { 0x070F, (int)UProperty.Joining_Type, JoiningType.Transparent },
                new int[] { 0xe0033, (int)UProperty.Joining_Type, JoiningType.Transparent },

                /* TestUnicodeData() verifies that no assigned character has "XX" (unknown) */
                new int[] { 0xe7e7, (int)UProperty.Line_Break, LineBreak.Unknown },
                new int[] { 0x10fffd, (int)UProperty.Line_Break, LineBreak.Unknown },
                new int[] { 0x0028, (int)UProperty.Line_Break, LineBreak.OpenPunctuation },
                new int[] { 0x232A, (int)UProperty.Line_Break, LineBreak.ClosePunctuation },
                new int[] { 0x3401, (int)UProperty.Line_Break, LineBreak.Ideographic },
                new int[] { 0x4e02, (int)UProperty.Line_Break, LineBreak.Ideographic },
                new int[] { 0x20004, (int)UProperty.Line_Break, LineBreak.Ideographic },
                new int[] { 0xf905, (int)UProperty.Line_Break, LineBreak.Ideographic },
                new int[] { 0xdb7e, (int)UProperty.Line_Break, LineBreak.Surrogate },
                new int[] { 0xdbfd, (int)UProperty.Line_Break, LineBreak.Surrogate },
                new int[] { 0xdffc, (int)UProperty.Line_Break, LineBreak.Surrogate },
                new int[] { 0x2762, (int)UProperty.Line_Break, LineBreak.Exclamation },
                new int[] { 0x002F, (int)UProperty.Line_Break, LineBreak.BreakSymbols },
                new int[] { 0x1D49C, (int)UProperty.Line_Break, LineBreak.Alphabetic },
                new int[] { 0x1731, (int)UProperty.Line_Break, LineBreak.Alphabetic },

                /* (int)UProperty.NUMERIC_TYPE tested in TestNumericProperties() */

                /* (int)UProperty.SCRIPT tested in TestUScriptCodeAPI() */

                new int[] { 0x10ff, (int)UProperty.Hangul_Syllable_Type, 0 },
                new int[] { 0x1100, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },
                new int[] { 0x1111, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },
                new int[] { 0x1159, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },
                new int[] { 0x115a, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },     /* changed in Unicode 5.2 */
                new int[] { 0x115e, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },     /* changed in Unicode 5.2 */
                new int[] { 0x115f, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },

                new int[] { 0xa95f, (int)UProperty.Hangul_Syllable_Type, 0 },
                new int[] { 0xa960, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },     /* changed in Unicode 5.2 */
                new int[] { 0xa97c, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LeadingJamo },     /* changed in Unicode 5.2 */
                new int[] { 0xa97d, (int)UProperty.Hangul_Syllable_Type, 0 },

                new int[] { 0x1160, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },
                new int[] { 0x1161, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },
                new int[] { 0x1172, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },
                new int[] { 0x11a2, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },
                new int[] { 0x11a3, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },       /* changed in Unicode 5.2 */
                new int[] { 0x11a7, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },       /* changed in Unicode 5.2 */

                new int[] { 0xd7af, (int)UProperty.Hangul_Syllable_Type, 0 },
                new int[] { 0xd7b0, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },       /* changed in Unicode 5.2 */
                new int[] { 0xd7c6, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.VowelJamo },       /* changed in Unicode 5.2 */
                new int[] { 0xd7c7, (int)UProperty.Hangul_Syllable_Type, 0 },

                new int[] { 0x11a8, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },
                new int[] { 0x11b8, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },
                new int[] { 0x11c8, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },
                new int[] { 0x11f9, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },
                new int[] { 0x11fa, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },    /* changed in Unicode 5.2 */
                new int[] { 0x11ff, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },    /* changed in Unicode 5.2 */
                new int[] { 0x1200, (int)UProperty.Hangul_Syllable_Type, 0 },

                new int[] { 0xd7ca, (int)UProperty.Hangul_Syllable_Type, 0 },
                new int[] { 0xd7cb, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },    /* changed in Unicode 5.2 */
                new int[] { 0xd7fb, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.TrailingJamo },    /* changed in Unicode 5.2 */
                new int[] { 0xd7fc, (int)UProperty.Hangul_Syllable_Type, 0 },

                new int[] { 0xac00, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvSyllable },
                new int[] { 0xac1c, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvSyllable },
                new int[] { 0xc5ec, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvSyllable },
                new int[] { 0xd788, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvSyllable },

                new int[] { 0xac01, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvtSyllable },
                new int[] { 0xac1b, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvtSyllable },
                new int[] { 0xac1d, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvtSyllable },
                new int[] { 0xc5ee, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvtSyllable },
                new int[] { 0xd7a3, (int)UProperty.Hangul_Syllable_Type, HangulSyllableType.LvtSyllable },

                new int[] { 0xd7a4, (int)UProperty.Hangul_Syllable_Type, 0 },

                new int[] { -1, 0x410, 0 }, /* version break for Unicode 4.1 */

                new int[] { 0x00d7, (int)UProperty.Pattern_Syntax, 1 },
                new int[] { 0xfe45, (int)UProperty.Pattern_Syntax, 1 },
                new int[] { 0x0061, (int)UProperty.Pattern_Syntax, 0 },

                new int[] { 0x0020, (int)UProperty.Pattern_White_Space, 1 },
                new int[] { 0x0085, (int)UProperty.Pattern_White_Space, 1 },
                new int[] { 0x200f, (int)UProperty.Pattern_White_Space, 1 },
                new int[] { 0x00a0, (int)UProperty.Pattern_White_Space, 0 },
                new int[] { 0x3000, (int)UProperty.Pattern_White_Space, 0 },

                new int[] { 0x1d200, (int)UProperty.Block, UnicodeBlock.Ancient_Greek_Musical_Notation_ID },
                new int[] { 0x2c8e,  (int)UProperty.Block, UnicodeBlock.Coptic_ID },
                new int[] { 0xfe17,  (int)UProperty.Block, UnicodeBlock.Vertical_Forms_ID },

                new int[] { 0x1a00,  (int)UProperty.Script, UScript.Buginese },
                new int[] { 0x2cea,  (int)UProperty.Script, UScript.Coptic },
                new int[] { 0xa82b,  (int)UProperty.Script, UScript.SylotiNagri },
                new int[] { 0x103d0, (int)UProperty.Script, UScript.OldPersian },

                new int[] { 0xcc28, (int)UProperty.Line_Break, LineBreak.H2 },
                new int[] { 0xcc29, (int)UProperty.Line_Break, LineBreak.H3 },
                new int[] { 0xac03, (int)UProperty.Line_Break, LineBreak.H3 },
                new int[] { 0x115f, (int)UProperty.Line_Break, LineBreak.Jl },
                new int[] { 0x11aa, (int)UProperty.Line_Break, LineBreak.Jt },
                new int[] { 0x11a1, (int)UProperty.Line_Break, LineBreak.Jv },

                new int[] { 0xb2c9, (int)UProperty.Grapheme_Cluster_Break, GraphemeClusterBreak.Lvt },
                new int[] { 0x036f, (int)UProperty.Grapheme_Cluster_Break, GraphemeClusterBreak.Extend },
                new int[] { 0x0000, (int)UProperty.Grapheme_Cluster_Break, GraphemeClusterBreak.Control },
                new int[] { 0x1160, (int)UProperty.Grapheme_Cluster_Break, GraphemeClusterBreak.V },

                new int[] { 0x05f4, (int)UProperty.Word_Break, WordBreak.MidLetter },
                new int[] { 0x4ef0, (int)UProperty.Word_Break, WordBreak.Other },
                new int[] { 0x19d9, (int)UProperty.Word_Break, WordBreak.Numeric },
                new int[] { 0x2044, (int)UProperty.Word_Break, WordBreak.MidNum },

                new int[] { 0xfffd, (int)UProperty.Sentence_Break, SentenceBreak.Other },
                new int[] { 0x1ffc, (int)UProperty.Sentence_Break, SentenceBreak.Upper },
                new int[] { 0xff63, (int)UProperty.Sentence_Break, SentenceBreak.Close },
                new int[] { 0x2028, (int)UProperty.Sentence_Break, SentenceBreak.Sep },

                new int[] { -1, 0x520, 0 }, /* version break for Unicode 5.2 */

                /* unassigned code points in new default Bidi R blocks */
                new int[] { 0x1ede4, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },
                new int[] { 0x1efe4, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeft },

                /* test some script codes >127 */
                new int[] { 0xa6e6,  (int)UProperty.Script, UScript.Bamum },
                new int[] { 0xa4d0,  (int)UProperty.Script, UScript.Lisu },
                new int[] { 0x10a7f,  (int)UProperty.Script, UScript.OldSouthArabian },

                new int[] { -1, 0x600, 0 }, /* version break for Unicode 6.0 */

                /* value changed in Unicode 6.0 */
                new int[] { 0x06C3, (int)UProperty.Joining_Group, JoiningGroup.TehMarbutaGoal },

                new int[] { -1, 0x610, 0 }, /* version break for Unicode 6.1 */

                /* unassigned code points in new/changed default Bidi AL blocks */
                new int[] { 0x08ba, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },
                new int[] { 0x1eee4, (int)UProperty.Bidi_Class, (int)UCharacterDirection.RightToLeftArabic },

                new int[] { -1, 0x630, 0 }, /* version break for Unicode 6.3 */

                /* unassigned code points in the currency symbols block now default to ET */
                new int[] { 0x20C0, (int)UProperty.Bidi_Class, (int)UCharacterDirection.EuropeanNumberTerminator },
                new int[] { 0x20CF, (int)UProperty.Bidi_Class, (int)UCharacterDirection.EuropeanNumberTerminator },

                /* new property in Unicode 6.3 */
                new int[] { 0x0027, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.None },
                new int[] { 0x0028, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.Open },
                new int[] { 0x0029, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.Close },
                new int[] { 0xFF5C, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.None },
                new int[] { 0xFF5B, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.Open },
                new int[] { 0xFF5D, (int)UProperty.Bidi_Paired_Bracket_Type, BidiPairedBracketType.Close },

                new int[] { -1, 0x700, 0 }, /* version break for Unicode 7.0 */

                /* new character range with Joining_Group values */
                new int[] { 0x10ABF, (int)UProperty.Joining_Group, JoiningGroup.NoJoiningGroup },
                new int[] { 0x10AC0, (int)UProperty.Joining_Group, JoiningGroup.ManichaeanAleph },
                new int[] { 0x10AC1, (int)UProperty.Joining_Group, JoiningGroup.ManichaeanBeth },
                new int[] { 0x10AEF, (int)UProperty.Joining_Group, JoiningGroup.ManichaeanHundred },
                new int[] { 0x10AF0, (int)UProperty.Joining_Group, JoiningGroup.NoJoiningGroup },

                new int[] { -1, 0xa00, 0 },  // version break for Unicode 10

                new int[] { 0x1F1E5, (int)UProperty.Regional_Indicator, FALSE },
                new int[] { 0x1F1E7, (int)UProperty.Regional_Indicator, TRUE },
                new int[] { 0x1F1FF, (int)UProperty.Regional_Indicator, TRUE },
                new int[] { 0x1F200, (int)UProperty.Regional_Indicator, FALSE },

                new int[] { 0x0600, (int)UProperty.Prepended_Concatenation_Mark, TRUE },
                new int[] { 0x0606, (int)UProperty.Prepended_Concatenation_Mark, FALSE },
                new int[] { 0x110BD, (int)UProperty.Prepended_Concatenation_Mark, TRUE },

                /* undefined (int)UProperty values */
                new int[] { 0x61, 0x4a7, 0 },
                new int[] { 0x234bc, 0x15ed, 0 }
            };


            if (UChar.GetIntPropertyMinValue(UProperty.Dash) != 0
                || UChar.GetIntPropertyMinValue(UProperty.Bidi_Class) != 0
                || UChar.GetIntPropertyMinValue(UProperty.Block) != 0  /* j2478 */
                || UChar.GetIntPropertyMinValue(UProperty.Script) != 0 /* JB#2410 */
                || UChar.GetIntPropertyMinValue((UProperty)0x2345) != 0)
            {
                Errln("error: UCharacter.getIntPropertyMinValue() wrong");
            }

            if (UChar.GetIntPropertyMaxValue(UProperty.Dash) != 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.DASH) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.ID_Continue) != 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.ID_CONTINUE) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UPropertyConstants.Binary_Limit - 1) != 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.BINARY_LIMIT-1) wrong\n");
            }

            if (UChar.GetIntPropertyMaxValue(UProperty.Bidi_Class) != UCharacterDirectionExtensions.CharDirectionCount.ToInt32() - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.BIDI_CLASS) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Block) != UnicodeBlock.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.BLOCK) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Line_Break) != LineBreak.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.LINE_BREAK) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Script) != UScript.CodeLimit - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.SCRIPT) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Numeric_Type) != NumericType.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.NUMERIC_TYPE) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.General_Category) != UUnicodeCategoryExtensions.CharCategoryCount - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.GENERAL_CATEGORY) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Hangul_Syllable_Type) != HangulSyllableType.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.HANGUL_SYLLABLE_TYPE) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Grapheme_Cluster_Break) != GraphemeClusterBreak.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.GRAPHEME_CLUSTER_BREAK) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Sentence_Break) != SentenceBreak.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.SENTENCE_BREAK) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Word_Break) != WordBreak.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.WORD_BREAK) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Bidi_Paired_Bracket_Type) != BidiPairedBracketType.Count - 1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.BIDI_PAIRED_BRACKET_TYPE) wrong\n");
            }
            /*JB#2410*/
            if (UChar.GetIntPropertyMaxValue((UProperty)0x2345) != -1)
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(0x2345) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Decomposition_Type) != (DecompositionType.Count - 1))
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.DECOMPOSITION_TYPE) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Joining_Group) != (JoiningGroup.Count - 1))
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.JOINING_GROUP) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.Joining_Type) != (JoiningType.Count - 1))
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.JOINING_TYPE) wrong\n");
            }
            if (UChar.GetIntPropertyMaxValue(UProperty.East_Asian_Width) != (EastAsianWidth.Count - 1))
            {
                Errln("error: UCharacter.getIntPropertyMaxValue(UProperty.EAST_ASIAN_WIDTH) wrong\n");
            }

            VersionInfo version = UChar.UnicodeVersion;

            // test hasBinaryProperty()
            for (int i = 0; i < props.Length; ++i)
            {
                int which = props[i][1];
                if (props[i][0] < 0)
                {
                    if (version.CompareTo(VersionInfo.GetInstance(which >> 8,
                                                              (which >> 4) & 0xF,
                                                              which & 0xF,
                                                              0)) < 0)
                    {
                        break;
                    }
                    continue;
                }
                String whichName;
                try
                {
                    whichName = UChar.GetPropertyName((UProperty)which, NameChoice.Long);
                }
                catch (ArgumentException e)
                {
                    // There are intentionally invalid property integer values ("which").
                    // Catch and ignore the exception from getPropertyName().
                    whichName = "undefined UProperty value";
                }
                bool expect = true;
                if (props[i][2] == 0)
                {
                    expect = false;
                }
                if ((UProperty)which < UPropertyConstants.Int_Start)
                {
                    if (UChar.HasBinaryProperty(props[i][0], (UProperty)which)
                        != expect)
                    {
                        Errln("error: UCharacter.hasBinaryProperty(U+" +
                                Utility.Hex(props[i][0], 4) + ", " +
                              whichName + ") has an error, expected=" + expect);
                    }
                }

                int retVal = UChar.GetIntPropertyValue(props[i][0], (UProperty)which);
                if (retVal != props[i][2])
                {
                    Errln("error: UCharacter.getIntPropertyValue(U+" +
                          Utility.Hex(props[i][0], 4) +
                          ", " + whichName + ") is wrong, expected="
                          + props[i][2] + " actual=" + retVal);
                }

                // test separate functions, too
                switch ((UProperty)which)
                {
                    case UProperty.Alphabetic:
                        if (UChar.IsUAlphabetic(props[i][0]) != expect)
                        {
                            Errln("error: UCharacter.isUAlphabetic(\\u" +
                                  (props[i][0]).ToHexString() +
                                  ") is wrong expected " + props[i][2]);
                        }
                        break;
                    case UProperty.Lowercase:
                        if (UChar.IsULower(props[i][0]) != expect)
                        {
                            Errln("error: UCharacter.isULowercase(\\u" +
                                  (props[i][0]).ToHexString() +
                                  ") is wrong expected " + props[i][2]);
                        }
                        break;
                    case UProperty.Uppercase:
                        if (UChar.IsUUpper(props[i][0]) != expect)
                        {
                            Errln("error: UCharacter.isUUppercase(\\u" +
                                  (props[i][0]).ToHexString() +
                                  ") is wrong expected " + props[i][2]);
                        }
                        break;
                    case UProperty.White_Space:
                        if (UChar.IsUWhiteSpace(props[i][0]) != expect)
                        {
                            Errln("error: UCharacter.isUWhiteSpace(\\u" +
                                  (props[i][0]).ToHexString() +
                                  ") is wrong expected " + props[i][2]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        [Test]
        public void TestNumericProperties()
        {
            // see UnicodeData.txt, DerivedNumericValues.txt
            double[][] values = {
                // Code point, numeric type, numeric value.
                // If a fourth value is specified, it is the getNumericValue().
                // Otherwise it is expected to be the same as the getUnicodeNumericValue(),
                // where UCharacter.NO_NUMERIC_VALUE is turned into -1.
                // getNumericValue() returns -2 if the code point has a value
                // which is not a non-negative integer. (This is mostly auto-converted to -2.)
                new double[] { 0x0F33, NumericType.Numeric, -1.0 / 2.0 },
                new double[] { 0x0C66, NumericType.Decimal, 0 },
                new double[] { 0x96f6, NumericType.Numeric, 0 },
                new double[] { 0xa833, NumericType.Numeric, 1.0 / 16.0 },
                new double[] { 0x2152, NumericType.Numeric, 1.0 / 10.0 },
                new double[] { 0x2151, NumericType.Numeric, 1.0 / 9.0 },
                new double[] { 0x1245f, NumericType.Numeric, 1.0 / 8.0 },
                new double[] { 0x2150, NumericType.Numeric, 1.0 / 7.0 },
                new double[] { 0x2159, NumericType.Numeric, 1.0 / 6.0 },
                new double[] { 0x09f6, NumericType.Numeric, 3.0 / 16.0 },
                new double[] { 0x2155, NumericType.Numeric, 1.0 / 5.0 },
                new double[] { 0x00BD, NumericType.Numeric, 1.0 / 2.0 },
                new double[] { 0x0031, NumericType.Decimal, 1.0 },
                new double[] { 0x4e00, NumericType.Numeric, 1.0 },
                new double[] { 0x58f1, NumericType.Numeric, 1.0 },
                new double[] { 0x10320, NumericType.Numeric, 1.0 },
                new double[] { 0x0F2B, NumericType.Numeric, 3.0 / 2.0 },
                new double[] { 0x00B2, NumericType.Digit, 2.0 }, /* Unicode 4.0 change */
                new double[] { 0x5f10, NumericType.Numeric, 2.0 },
                new double[] { 0x1813, NumericType.Decimal, 3.0 },
                new double[] { 0x5f0e, NumericType.Numeric, 3.0 },
                new double[] { 0x2173, NumericType.Numeric, 4.0 },
                new double[] { 0x8086, NumericType.Numeric, 4.0 },
                new double[] { 0x278E, NumericType.Digit, 5.0 },
                new double[] { 0x1D7F2, NumericType.Decimal, 6.0 },
                new double[] { 0x247A, NumericType.Digit, 7.0 },
                new double[] { 0x7396, NumericType.Numeric, 9.0 },
                new double[] { 0x1372, NumericType.Numeric, 10.0 },
                new double[] { 0x216B, NumericType.Numeric, 12.0 },
                new double[] { 0x16EE, NumericType.Numeric, 17.0 },
                new double[] { 0x249A, NumericType.Numeric, 19.0 },
                new double[] { 0x303A, NumericType.Numeric, 30.0 },
                new double[] { 0x5345, NumericType.Numeric, 30.0 },
                new double[] { 0x32B2, NumericType.Numeric, 37.0 },
                new double[] { 0x1375, NumericType.Numeric, 40.0 },
                new double[] { 0x10323, NumericType.Numeric, 50.0 },
                new double[] { 0x0BF1, NumericType.Numeric, 100.0 },
                new double[] { 0x964c, NumericType.Numeric, 100.0 },
                new double[] { 0x217E, NumericType.Numeric, 500.0 },
                new double[] { 0x2180, NumericType.Numeric, 1000.0 },
                new double[] { 0x4edf, NumericType.Numeric, 1000.0 },
                new double[] { 0x2181, NumericType.Numeric, 5000.0 },
                new double[] { 0x137C, NumericType.Numeric, 10000.0 },
                new double[] { 0x4e07, NumericType.Numeric, 10000.0 },
                new double[] { 0x12432, NumericType.Numeric, 216000.0 },
                new double[] { 0x12433, NumericType.Numeric, 432000.0 },
                new double[] { 0x4ebf, NumericType.Numeric, 100000000.0 },
                new double[] { 0x5146, NumericType.Numeric, 1000000000000.0 },
                new double[] { -1, NumericType.None, UChar.NoNumericValue },
                new double[] { 0x61, NumericType.None, UChar.NoNumericValue, 10.0 },
                new double[] { 0x3000, NumericType.None, UChar.NoNumericValue },
                new double[] { 0xfffe, NumericType.None, UChar.NoNumericValue },
                new double[] { 0x10301, NumericType.None, UChar.NoNumericValue },
                new double[] { 0xe0033, NumericType.None, UChar.NoNumericValue },
                new double[] { 0x10ffff, NumericType.None, UChar.NoNumericValue },
                new double[] { 0x110000, NumericType.None, UChar.NoNumericValue }
            };

            for (int i = 0; i < values.Length; ++i)
            {
                int c = (int)values[i][0];
                int type = UChar.GetIntPropertyValue(c,
                                                          UProperty.Numeric_Type);
                double nv = UChar.GetUnicodeNumericValue(c);

                if (type != values[i][1])
                {
                    Errln("UProperty.NUMERIC_TYPE(\\u" + Utility.Hex(c, 4)
                           + ") = " + type + " should be " + (int)values[i][1]);
                }
                if (0.000001 <= Math.Abs(nv - values[i][2]))
                {
                    Errln("UCharacter.getUnicodeNumericValue(\\u" + Utility.Hex(c, 4)
                            + ") = " + nv + " should be " + values[i][2]);
                }

                // Test getNumericValue() as well.
                // It can only return the subset of numeric values that are
                // non-negative and fit into an int.
                int expectedInt;
                if (values[i].Length == 3)
                {
                    if (values[i][2] == UChar.NoNumericValue)
                    {
                        expectedInt = -1;
                    }
                    else
                    {
                        expectedInt = (int)values[i][2];
                        if (expectedInt < 0 || expectedInt != values[i][2])
                        {
                            // The numeric value is not a non-negative integer.
                            expectedInt = -2;
                        }
                    }
                }
                else
                {
                    expectedInt = (int)values[i][3];
                }
                int nvInt = UChar.GetNumericValue(c);
                if (nvInt != expectedInt)
                {
                    Errln("UCharacter.getNumericValue(\\u" + Utility.Hex(c, 4)
                            + ") = " + nvInt + " should be " + expectedInt);
                }
            }
        }

        /**
         * Test the property values API.  See JB#2410.
         */
        [Test]
        public void TestPropertyValues()
        {
            UProperty i, p;
            int min, max;

            /* Min should be 0 for everything. */
            /* Until JB#2478 is fixed, the one exception is UProperty.BLOCK. */
            for (p = UPropertyConstants.Int_Start; p < UPropertyConstants.Int_Limit; ++p)
            {
                min = UChar.GetIntPropertyMinValue(p);
                if (min != 0)
                {
                    if (p == UProperty.Block)
                    {
                        /* This is okay...for now.  See JB#2487.
                           TODO Update this for JB#2487. */
                    }
                    else
                    {
                        String name;
                        name = UChar.GetPropertyName(p, NameChoice.Long);
                        Errln("FAIL: UCharacter.getIntPropertyMinValue(" + name + ") = " +
                              min + ", exp. 0");
                    }
                }
            }

            if (UChar.GetIntPropertyMinValue(UProperty.General_Category_Mask)
                != 0
                || UChar.GetIntPropertyMaxValue(
                                                   UProperty.General_Category_Mask)
                   != -1)
            {
                Errln("error: UCharacter.getIntPropertyMin/MaxValue("
                      + "UProperty.GENERAL_CATEGORY_MASK) is wrong");
            }

            /* Max should be -1 for invalid properties. */
            max = UChar.GetIntPropertyMaxValue((UProperty)(-1));
            if (max != -1)
            {
                Errln("FAIL: UCharacter.getIntPropertyMaxValue(-1) = " +
                      max + ", exp. -1");
            }

            /* Script should return 0 for an invalid code point. If the API
               throws an exception then that's fine too. */
            for (i = 0; (int)i < 2; ++i)
            {
                try
                {
                    int script = 0;
                    String desc = null;
                    switch ((int)i)
                    {
                        case 0:
                            script = UScript.GetScript(-1);
                            desc = "UScript.getScript(-1)";
                            break;
                        case 1:
                            script = UChar.GetIntPropertyValue(-1, UProperty.Script);
                            desc = "UCharacter.getIntPropertyValue(-1, UProperty.SCRIPT)";
                            break;
                    }
                    if (script != 0)
                    {
                        Errln("FAIL: " + desc + " = " + script + ", exp. 0");
                    }
                }
                catch (ArgumentException e) { }
            }
        }

        [Test]
        public void TestBidiPairedBracketType()
        {
            // BidiBrackets-6.3.0.txt says:
            //
            // The set of code points listed in this file was originally derived
            // using the character properties General_Category (gc), Bidi_Class (bc),
            // Bidi_Mirrored (Bidi_M), and Bidi_Mirroring_Glyph (bmg), as follows:
            // two characters, A and B, form a pair if A has gc=Ps and B has gc=Pe,
            // both have bc=ON and Bidi_M=Y, and bmg of A is B. Bidi_Paired_Bracket
            // maps A to B and vice versa, and their Bidi_Paired_Bracket_Type
            // property values are Open and Close, respectively.
            UnicodeSet bpt = new UnicodeSet("[:^bpt=n:]");
            assertTrue("bpt!=None is not empty", !bpt.IsEmpty);
            // The following should always be true.
            UnicodeSet mirrored = new UnicodeSet("[:Bidi_M:]");
            UnicodeSet other_neutral = new UnicodeSet("[:bc=ON:]");
            assertTrue("bpt!=None is a subset of Bidi_M", mirrored.ContainsAll(bpt));
            assertTrue("bpt!=None is a subset of bc=ON", other_neutral.ContainsAll(bpt));
            // The following are true at least initially in Unicode 6.3.
            UnicodeSet bpt_open = new UnicodeSet("[:bpt=o:]");
            UnicodeSet bpt_close = new UnicodeSet("[:bpt=c:]");
            UnicodeSet ps = new UnicodeSet("[:Ps:]");
            UnicodeSet pe = new UnicodeSet("[:Pe:]");
            assertTrue("bpt=Open is a subset of Ps", ps.ContainsAll(bpt_open));
            assertTrue("bpt=Close is a subset of Pe", pe.ContainsAll(bpt_close));
        }

        [Test]
        public void TestEmojiProperties()
        {
            assertFalse("space is not Emoji", UChar.HasBinaryProperty(0x20, UProperty.Emoji));
            assertTrue("shooting star is Emoji", UChar.HasBinaryProperty(0x1F320, UProperty.Emoji));
            UnicodeSet emoji = new UnicodeSet("[:Emoji:]");
            assertTrue("lots of Emoji", emoji.Count > 700);

            assertTrue("shooting star is Emoji_Presentation",
                    UChar.HasBinaryProperty(0x1F320, UProperty.Emoji_Presentation));
            assertTrue("Fitzpatrick 6 is Emoji_Modifier",
                    UChar.HasBinaryProperty(0x1F3FF, UProperty.Emoji_Modifier));
            assertTrue("happy person is Emoji_Modifier_Base",
                    UChar.HasBinaryProperty(0x1F64B, UProperty.Emoji_Modifier_Base));
            assertTrue("asterisk is Emoji_Component",
                    UChar.HasBinaryProperty(0x2A, UProperty.Emoji_Component));
        }

        [Test]
        public void TestIsBMP()
        {
            int[] ch = { 0x0, -1, 0xffff, 0x10ffff, 0xff, 0x1ffff };
            bool[] flag = { true, false, true, false, true, false };
            for (int i = 0; i < ch.Length; i++)
            {
                if (UChar.IsBMP(ch[i]) != flag[i])
                {
                    Errln("Fail: \\u" + Utility.Hex(ch[i], 8)
                          + " failed at UCharacter.isBMP");
                }
            }
        }

        private bool ShowADiffB(UnicodeSet a, UnicodeSet b,
                                            String a_name, String b_name,
                                            bool expect,
                                            bool diffIsError)
        {
            int i, start, end;
            bool equal = true;
            for (i = 0; i < a.RangeCount; ++i)
            {
                start = a.GetRangeStart(i);
                end = a.GetRangeEnd(i);
                if (expect != b.Contains(start, end))
                {
                    equal = false;
                    while (start <= end)
                    {
                        if (expect != b.Contains(start))
                        {
                            if (diffIsError)
                            {
                                if (expect)
                                {
                                    Errln("error: " + a_name + " contains " + Hex(start) + " but " + b_name + " does not");
                                }
                                else
                                {
                                    Errln("error: " + a_name + " and " + b_name + " both contain " + Hex(start) + " but should not intersect");
                                }
                            }
                            else
                            {
                                if (expect)
                                {
                                    Logln("info: " + a_name + " contains " + Hex(start) + "but " + b_name + " does not");
                                }
                                else
                                {
                                    Logln("info: " + a_name + " and " + b_name + " both contain " + Hex(start) + " but should not intersect");
                                }
                            }
                        }
                        ++start;
                    }
                }
            }
            return equal;
        }
        private bool ShowAMinusB(UnicodeSet a, UnicodeSet b,
                                            String a_name, String b_name,
                                            bool diffIsError)
        {

            return ShowADiffB(a, b, a_name, b_name, true, diffIsError);
        }

        private bool ShowAIntersectB(UnicodeSet a, UnicodeSet b,
                                                String a_name, String b_name,
                                                bool diffIsError)
        {
            return ShowADiffB(a, b, a_name, b_name, false, diffIsError);
        }

        private bool CompareUSets(UnicodeSet a, UnicodeSet b,
                                             String a_name, String b_name,
                                             bool diffIsError)
        {
            return
                ShowAMinusB(a, b, a_name, b_name, diffIsError) &&
                ShowAMinusB(b, a, b_name, a_name, diffIsError);
        }

        /* various tests for consistency of UCD data and API behavior */
        [Test]
        public void TestConsistency()
        {
            UnicodeSet set1, set2, set3, set4;

            int start, end;
            int i, length;

            String hyphenPattern = "[:Hyphen:]";
            String dashPattern = "[:Dash:]";
            String lowerPattern = "[:Lowercase:]";
            String formatPattern = "[:Cf:]";
            String alphaPattern = "[:Alphabetic:]";

            /*
             * It used to be that UCD.html and its precursors said
             * "Those dashes used to mark connections between pieces of words,
             *  plus the Katakana middle dot."
             *
             * Unicode 4 changed 00AD Soft Hyphen to Cf and removed it from Dash
             * but not from Hyphen.
             * UTC 94 (2003mar) decided to leave it that way and to change UCD.html.
             * Therefore, do not show errors when testing the Hyphen property.
             */
            Logln("Starting with Unicode 4, inconsistencies with [:Hyphen:] are\n"
                        + "known to the UTC and not considered errors.\n");

            set1 = new UnicodeSet(hyphenPattern);
            set2 = new UnicodeSet(dashPattern);

            /* remove the Katakana middle dot(s) from set1 */
            set1.Remove(0x30fb);
            set2.Remove(0xff65); /* halfwidth variant */
            ShowAMinusB(set1, set2, "[:Hyphen:]", "[:Dash:]", false);


            /* check that Cf is neither Hyphen nor Dash nor Alphabetic */
            set3 = new UnicodeSet(formatPattern);
            set4 = new UnicodeSet(alphaPattern);

            ShowAIntersectB(set3, set1, "[:Cf:]", "[:Hyphen:]", false);
            ShowAIntersectB(set3, set2, "[:Cf:]", "[:Dash:]", true);
            ShowAIntersectB(set3, set4, "[:Cf:]", "[:Alphabetic:]", true);
            /*
             * Check that each lowercase character has "small" in its name
             * and not "capital".
             * There are some such characters, some of which seem odd.
             * Use the verbose flag to see these notices.
             */
            set1 = new UnicodeSet(lowerPattern);

            for (i = 0; ; ++i)
            {
                //               try{
                //                   length=set1.getItem(set1, i, &start, &end, NULL, 0, &errorCode);
                //               }catch(Exception e){
                //                   break;
                //               }
                start = set1.GetRangeStart(i);
                end = set1.GetRangeEnd(i);
                length = i < set1.RangeCount ? set1.RangeCount : 0;
                if (length != 0)
                {
                    break; /* done with code points, got a string or -1 */
                }

                while (start <= end)
                {
                    String name = UChar.GetName(start);

                    if ((name.IndexOf("SMALL", StringComparison.Ordinal) < 0 || name.IndexOf("CAPITAL", StringComparison.Ordinal) < -1) &&
                        name.IndexOf("SMALL CAPITAL", StringComparison.Ordinal) == -1
                    )
                    {
                        Logln("info: [:Lowercase:] contains U+" + Hex(start) + " whose name does not suggest lowercase: " + name);
                    }
                    ++start;
                }
            }


            /*
             * Test for an example that unorm_getCanonStartSet() delivers
             * all characters that compose from the input one,
             * even in multiple steps.
             * For example, the set for "I" (0049) should contain both
             * I-diaeresis (00CF) and I-diaeresis-acute (1E2E).
             * In general, the set for the middle such character should be a subset
             * of the set for the first.
             */
            Normalizer2 norm2 = Normalizer2.GetNFDInstance();
            set1 = new UnicodeSet();
            Norm2AllModes.GetNFCInstance().Impl.
                EnsureCanonIterData().GetCanonStartSet(0x49, set1);
            set2 = new UnicodeSet();

            /* enumerate all characters that are plausible to be latin letters */
            for (start = 0xa0; start < 0x2000; ++start)
            {
                String decomp = norm2.Normalize(UTF16.ValueOf(start));
                if (decomp.Length > 1 && decomp[0] == 0x49)
                {
                    set2.Add(start);
                }
            }

            CompareUSets(set1, set2,
                         "[canon start set of 0049]", "[all c with canon decomp with 0049]",
                         false);

        }

        [Test]
        public void TestCoverage()
        {
            //cover forDigit
            char ch1 = UChar.ForDigit(7, 11);
            assertEquals("UCharacter.forDigit ", "7", ch1 + "");
            char ch2 = UChar.ForDigit(17, 20);
            assertEquals("UCharacter.forDigit ", "h", ch2 + "");

            // ICU4N: Doesn't apply in .NET
            ////Jitterbug 4451, for coverage
            //for (int i = 0x0041; i < 0x005B; i++)
            //{
            //    if (!UCharacter.IsJavaLetter(i))
            //        Errln("FAIL \\u" + Hex(i) + " expected to be a letter");
            //    if (!UCharacter.IsJavaIdentifierStart(i))
            //        Errln("FAIL \\u" + Hex(i) + " expected to be a Java identifier start character");
            //    if (!UCharacter.IsJavaLetterOrDigit(i))
            //        Errln("FAIL \\u" + Hex(i) + " expected not to be a Java letter");
            //    if (!UCharacter.IsJavaIdentifierPart(i))
            //        Errln("FAIL \\u" + Hex(i) + " expected to be a Java identifier part character");
            //}
            char[] spaces = { '\t', '\n', '\f', '\r', ' ' };
            for (int i = 0; i < spaces.Length; i++)
            {
                if (!UChar.IsSpace(spaces[i]))
                    Errln("FAIL \\u" + Hex(spaces[i]) + " expected to be a Java space");
            }
        }

        [Test]
        public void TestBlockData()
        {
            Type ubc = typeof(UnicodeBlock);

            for (int b = 1; b < UnicodeBlock.Count; b += 1)
            {
                UnicodeBlock blk = UnicodeBlock.GetInstance(b);
                int id = blk.ID;
                String name = blk.ToString();

                if (id != b)
                {
                    Errln("UCharacter.UnicodeBlock.GetInstance(" + b + ") returned a block with id = " + id);
                }

                try
                {
                    if ((int)ubc.GetField(name + "_ID").GetValue(blk) != b)
                    {
                        Errln("UCharacter.UnicodeBlock.GetInstance(" + b + ") returned a block with a name of " + name +
                              " which does not match the block id.");
                    }
                }
                catch (Exception e)
                {
                    Errln("Couldn't get the id name for id " + b);
                }
            }
        }

        /*
         * The following method tests
         *      public static UnicodeBlock getInstance(int id)
         */
        [Test]
        public void TestGetInstance()
        {
            // Testing values for invalid and valid ID
            int[] invalid_test = { -1, -10, -100 };
            for (int i = 0; i < invalid_test.Length; i++)
            {
                if (UnicodeBlock.Invalid_Code != UnicodeBlock.GetInstance(invalid_test[i]))
                {
                    Errln("UCharacter.UnicodeBlock.GetInstance(invalid_test[i]) was " +
                            "suppose to return UCharacter.UnicodeBlock.INVALID_CODE. Got " +
                            UnicodeBlock.GetInstance(invalid_test[i]) + ". Expected " +
                            UnicodeBlock.Invalid_Code);
                }
            }
        }

        /*
         * The following method tests
         *      public static UnicodeBlock of(int ch)
         */
        [Test]
        public void TestOf()
        {
            if (UnicodeBlock.Invalid_Code != UnicodeBlock.Of(UTF16.CodePointMaxValue + 1))
            {
                Errln("UCharacter.UnicodeBlock.of(UTF16.CODEPOINT_MAX_VALUE+1) was " +
                        "suppose to return UCharacter.UnicodeBlock.INVALID_CODE. Got " +
                        UnicodeBlock.Of(UTF16.CodePointMaxValue + 1) + ". Expected " +
                        UnicodeBlock.Invalid_Code);
            }
        }

        /*
         * The following method tests
         *      public static final UnicodeBlock forName(String blockName)
         */
        [Test]
        public void TestForName()
        {
            //UCharacter.UnicodeBlock.forName("");
            //Tests when "if (b == null)" is true
        }

        /*
         * The following method tests
         *      public static int getNumericValue(int ch)
         */
        [Test]
        public void TestGetNumericValue()
        {
            // The following tests the else statement when
            //      if(numericType<NumericType.COUNT) is false
            // The following values were obtained by testing all values from
            //      UTF16.CODEPOINT_MIN_VALUE to UTF16.CODEPOINT_MAX_VALUE inclusively
            //      to obtain the value to go through the else statement.
            int[] valid_values =
                {3058,3442,4988,8558,8559,8574,8575,8576,8577,8578,8583,8584,19975,
             20159,20191,20740,20806,21315,33836,38433,65819,65820,65821,65822,
             65823,65824,65825,65826,65827,65828,65829,65830,65831,65832,65833,
             65834,65835,65836,65837,65838,65839,65840,65841,65842,65843,65861,
             65862,65863,65868,65869,65870,65875,65876,65877,65878,65899,65900,
             65901,65902,65903,65904,65905,65906,66378,68167};

            int[] results =
                {1000,1000,10000,500,1000,500,1000,1000,5000,10000,50000,100000,
             10000,100000000,1000,100000000,-2,1000,10000,1000,300,400,500,
             600,700,800,900,1000,2000,3000,4000,5000,6000,7000,8000,9000,
             10000,20000,30000,40000,50000,60000,70000,80000,90000,500,5000,
             50000,500,1000,5000,500,1000,10000,50000,300,500,500,500,500,500,
             1000,5000,900,1000};

            if (valid_values.Length != results.Length)
            {
                Errln("The valid_values array and the results array need to be " +
                        "the same length.");
            }
            else
            {
                for (int i = 0; i < valid_values.Length; i++)
                {
                    try
                    {
                        if (UChar.GetNumericValue(valid_values[i]) != results[i])
                        {
                            Errln("UCharacter.getNumericValue(i) returned a " +
                                    "different value from the expected result. " +
                                    "Got " + UChar.GetNumericValue(valid_values[i]) +
                                    "Expected" + results[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        Errln("UCharacter.getNumericValue(int) returned an exception " +
                                "with the parameter value");
                    }
                }
            }
        }

        /*
         * The following method tests
         *      public static double getUnicodeNumericValue(int ch)
         */
        // The following tests covers if(mant==0), else if(mant > 9), and default
        [Test]
        public void TestGetUnicodeNumericValue()
        {
            /*  The code coverage for if(mant==0), else if(mant > 9), and default
             *  could not be covered even with input values from UTF16.CODEPOINT_MIN_VALUE
             *  to UTF16.CODEPOINT_MAX_VALUE. I also tested from UTF16.CODEPOINT_MAX_VALUE to
             *  Integer.MAX_VALUE and didn't recieve any code coverage there too.
             *  Therefore, the code could either be dead code or meaningless.
             */
        }

        /*
         * The following method tests
         *      public static String toString(int ch)
         */
        [Test]
        public void TestToString()
        {
            int[] valid_tests = {
                UChar.MinValue, UChar.MinValue+1,
                UChar.MaxValue-1, UChar.MaxValue};
            int[] invalid_tests = {
                UChar.MinValue-1, UChar.MinValue-2,
                UChar.MaxValue+1, UChar.MaxValue+2};

            for (int i = 0; i < valid_tests.Length; i++)
            {
                if (UChar.ConvertFromUtf32(valid_tests[i]) == null)
                {
                    Errln("UCharacter.toString(int) was not suppose to return " +
                    "null because it was given a valid parameter. Value passed: " +
                    valid_tests[i] + ". Got null.");
                }
            }

            for (int i = 0; i < invalid_tests.Length; i++)
            {
                if (UChar.ConvertFromUtf32(invalid_tests[i]) != null)
                {
                    Errln("UCharacter.toString(int) was suppose to return " +
                    "null because it was given an invalid parameter. Value passed: " +
                    invalid_tests[i] + ". Got: " + UChar.ConvertFromUtf32(invalid_tests[i]));
                }
            }
        }

        /*
         * The following method tests
         *      public static int getCombiningClass(int ch)
         */
        [Test]
        public void TestGetCombiningClass()
        {
            int[] valid_tests = {
                UChar.MinValue, UChar.MinValue+1,
                UChar.MaxValue-1, UChar.MaxValue};
            int[] invalid_tests = {
                UChar.MinValue-1, UChar.MinValue-2,
                UChar.MaxValue+1, UChar.MaxValue+2};

            for (int i = 0; i < valid_tests.Length; i++)
            {
                try
                {
                    UChar.GetCombiningClass(valid_tests[i]);
                }
                catch (Exception e)
                {
                    Errln("UCharacter.getCombiningClass(int) was not supposed to have " +
                            "an exception. Value passed: " + valid_tests[i]);
                }
            }

            for (int i = 0; i < invalid_tests.Length; i++)
            {
                try
                {
                    assertEquals("getCombiningClass(out of range)",
                                 0, UChar.GetCombiningClass(invalid_tests[i]));
                }
                catch (Exception e)
                {
                    Errln("UCharacter.getCombiningClass(int) was not supposed to have " +
                            "an exception. Value passed: " + invalid_tests[i]);
                }
            }
        }

        /*
         * The following method tests
         *      public static String getName(int ch)
         */
        [Test]
        public void TestGetName()
        {
            // Need to test on other "one characters" for the getName() method
            String[] data = { "a", "z" };
            String[] results = { "LATIN SMALL LETTER A", "LATIN SMALL LETTER Z" };
            if (data.Length != results.Length)
            {
                Errln("The data array and the results array need to be " +
                        "the same length.");
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (UChar.GetName(data[i], "").CompareToOrdinal(results[i]) != 0)
                    {
                        Errln("UCharacter.getName(String, String) was suppose " +
                                "to have the same result for the data in the parameter. " +
                                "Value passed: " + data[i] + ". Got: " +
                                UChar.GetName(data[i], "") + ". Expected: " +
                                results[i]);
                    }
                }
            }
        }

        /*
         * The following method tests
         *      public static String getISOComment(int ch)
         */
        [Test]
        public void TestGetISOComment()
        {
            int[] invalid_tests = {
                UChar.MinValue-1, UChar.MinValue-2,
                UChar.MaxValue+1, UChar.MaxValue+2};

            for (int i = 0; i < invalid_tests.Length; i++)
            {
                if (UChar.GetISOComment(invalid_tests[i]) != null)
                {
                    Errln("UCharacter.getISOComment(int) was suppose to return " +
                    "null because it was given an invalid parameter. Value passed: " +
                    invalid_tests[i] + ". Got: " + UChar.GetISOComment(invalid_tests[i]));
                }
            }
        }

        /*
         * The following method tests
         *      public void setLimit(int lim)
         */
        [Test]
        public void TestSetLimit()
        {
            // TODO: Tests when "if(0<=lim && lim<=s.Length)" is false
        }

        /*
         * The following method tests
         *      public int nextCaseMapCP()
         */
        [Test]
        public void TestNextCaseMapCP()
        {
            // TODO: Tests when "if(UTF16.LEAD_SURROGATE_MIN_VALUE<=c || c<=UTF16.TRAIL_SURROGATE_MAX_VALUE)" is false
            /* TODO: Tests when "if( c<=UTF16.LEAD_SURROGATE_MAX_VALUE && cpLimit<limit &&
             * UTF16.TRAIL_SURROGATE_MIN_VALUE<=(c2=s.charAt(cpLimit)) && c2<=UTF16.TRAIL_SURROGATE_MAX_VALUE)" is false
             */
        }

        /*
         * The following method tests
         *      public void reset(int direction)
         */
        [Test]
        public void TestReset()
        {
            // The method reset() is never called by another function
            // TODO: Tests when "else if(direction<0)" is false
        }

        /*
         * The following methods test
         *      public static String toTitleCase(Locale locale, String str, BreakIterator breakiter)
         */
        [Test]
        public void TestToTitleCaseCoverage()
        {
            //Calls the function "toTitleCase(Locale locale, String str, BreakIterator breakiter)"
            String[] locale = { "en", "fr", "zh", "ko", "ja", "it", "de", "" };
            for (int i = 0; i < locale.Length; i++)
            {
                UChar.ToTitleCase(new CultureInfo(locale[i]), "", null);
            }

            // Calls the function "string ToTitleCase(UCultureInfo locale, string str, BreakIterator titleIter, int options)"
            // Tests when "if (locale == null)" is true
            UChar.ToTitleCase(locale:(UCultureInfo)null, str: "", titleIter: null, options: 0);

            // TODO: Tests when "if(index==BreakIterator.Done || index>srcLength)" is true
            // TODO: Tests when "while((c=iter.nextCaseMapCP())>=0 && UCaseProps.NONE==gCsp.getType(c))" is false
            // TODO: Tests when "if(prev<titleStart)" is false
            // TODO: Tests when "if(c<=0xffff)" is false
            // TODO: Tests when "if(c<=0xffff)" is false
            // TODO: Tests when "if(titleLimit<index)" is false
            // TODO: Tests when "else if((nc=iter.nextCaseMapCP())>=0)" is false
        }

        [Test]
        public void TestToTitleCase_Locale_String_BreakIterator_I()
        {
            String titleCase = UChar.ToTitleCase(new CultureInfo("nl"), "ijsland", null,
                    UChar.FoldCaseDefault);
            assertEquals("Wrong title casing", "IJsland", titleCase);
        }

        [Test]
        public void TestToTitleCase_String_BreakIterator_en()
        {
            String titleCase = UChar.ToTitleCase(new CultureInfo("en"), "ijsland", null);
            assertEquals("Wrong title casing", "Ijsland", titleCase);
        }
        /*
         * The following method tests
         *      public static string ToUpperCase(UCultureInfo locale, string str)
         */
        [Test]
        public void TestToUpperCase()
        {
            // TODO: Tests when "while((c=iter.nextCaseMapCP())>=0)" is false
        }

        /*
         * The following method tests
         *      public static string ToLowerCase(UCultureInfo locale, string str)
         */
        [Test]
        public void TestToLowerCase()
        {
            // Test when locale is null
            String[] cases = {"","a","A","z","Z","Dummy","DUMMY","dummy","a z","A Z",
                "'","\"","0","9","0a","a0","*","~!@#$%^&*()_+"};
            for (int i = 0; i < cases.Length; i++)
            {
                try
                {
                    UChar.ToLower((UCultureInfo)null, cases[i]);
                }
                catch (Exception e)
                {
                    Errln("UCharacter.toLowerCase was not suppose to return an " +
                            "exception for input of null and string: " + cases[i]);
                }
            }
            // TODO: Tests when "while((c=iter.nextCaseMapCP())>=0)" is false
        }

        /*
         * The following method tests
         *      public static int getHanNumericValue(int ch)
         */
        [Test]
        public void TestGetHanNumericValue()
        {
            int[] valid = {
                0x3007, //IDEOGRAPHIC_NUMBER_ZERO_
                0x96f6, //CJK_IDEOGRAPH_COMPLEX_ZERO_
                0x4e00, //CJK_IDEOGRAPH_FIRST_
                0x58f9, //CJK_IDEOGRAPH_COMPLEX_ONE_
                0x4e8c, //CJK_IDEOGRAPH_SECOND_
                0x8cb3, //CJK_IDEOGRAPH_COMPLEX_TWO_
                0x4e09, //CJK_IDEOGRAPH_THIRD_
                0x53c3, //CJK_IDEOGRAPH_COMPLEX_THREE_
                0x56db, //CJK_IDEOGRAPH_FOURTH_
                0x8086, //CJK_IDEOGRAPH_COMPLEX_FOUR_
                0x4e94, //CJK_IDEOGRAPH_FIFTH_
                0x4f0d, //CJK_IDEOGRAPH_COMPLEX_FIVE_
                0x516d, //CJK_IDEOGRAPH_SIXTH_
                0x9678, //CJK_IDEOGRAPH_COMPLEX_SIX_
                0x4e03, //CJK_IDEOGRAPH_SEVENTH_
                0x67d2, //CJK_IDEOGRAPH_COMPLEX_SEVEN_
                0x516b, //CJK_IDEOGRAPH_EIGHTH_
                0x634c, //CJK_IDEOGRAPH_COMPLEX_EIGHT_
                0x4e5d, //CJK_IDEOGRAPH_NINETH_
                0x7396, //CJK_IDEOGRAPH_COMPLEX_NINE_
                0x5341, //CJK_IDEOGRAPH_TEN_
                0x62fe, //CJK_IDEOGRAPH_COMPLEX_TEN_
                0x767e, //CJK_IDEOGRAPH_HUNDRED_
                0x4f70, //CJK_IDEOGRAPH_COMPLEX_HUNDRED_
                0x5343, //CJK_IDEOGRAPH_THOUSAND_
                0x4edf, //CJK_IDEOGRAPH_COMPLEX_THOUSAND_
                0x824c, //CJK_IDEOGRAPH_TEN_THOUSAND_
                0x5104, //CJK_IDEOGRAPH_HUNDRED_MILLION_
        };

            int[] invalid = { -5, -2, -1, 0 };

            int[] results = {0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9,10,10,100,100,
                1000,1000,10000,100000000};

            if (valid.Length != results.Length)
            {
                Errln("The arrays valid and results are suppose to be the same length " +
                        "to test getHanNumericValue(int ch).");
            }
            else
            {
                for (int i = 0; i < valid.Length; i++)
                {
                    if (UChar.GetHanNumericValue(valid[i]) != results[i])
                    {
                        Errln("UCharacter.getHanNumericValue does not return the " +
                                "same result as expected. Passed value: " + valid[i] +
                                ". Got: " + UChar.GetHanNumericValue(valid[i]) +
                                ". Expected: " + results[i]);
                    }
                }
            }

            for (int i = 0; i < invalid.Length; i++)
            {
                if (UChar.GetHanNumericValue(invalid[i]) != -1)
                {
                    Errln("UCharacter.getHanNumericValue does not return the " +
                            "same result as expected. Passed value: " + invalid[i] +
                            ". Got: " + UChar.GetHanNumericValue(invalid[i]) +
                            ". Expected: -1");
                }
            }
        }

        /*
         * The following method tests
         *      public static bool hasBinaryProperty(int ch, int property)
         */
        [Test]
        public void TestHasBinaryProperty()
        {
            // Testing when "if (ch < MIN_VALUE || ch > MAX_VALUE)" is true
            int[] invalid = {
                UChar.MinValue-1, UChar.MinValue-2,
                UChar.MaxValue+1, UChar.MaxValue+2};
            int[] valid = {
                UChar.MinValue, UChar.MinValue+1,
                UChar.MaxValue, UChar.MaxValue-1};

            for (int i = 0; i < invalid.Length; i++)
            {
                try
                {
                    if (UChar.HasBinaryProperty(invalid[i], (UProperty)1))
                    {
                        Errln("UCharacter.hasBinaryProperty(ch, property) should return " +
                                "false for out-of-range code points but " +
                                "returns true for " + invalid[i]);
                    }
                }
                catch (Exception e)
                {
                    Errln("UCharacter.hasBinaryProperty(ch, property) should not " +
                            "throw an exception for any input. Value passed: " +
                            invalid[i]);
                }
            }

            for (int i = 0; i < valid.Length; i++)
            {
                try
                {
                    UChar.HasBinaryProperty(valid[i], (UProperty)1);
                }
                catch (Exception e)
                {
                    Errln("UCharacter.hasBinaryProperty(ch, property) should not " +
                            "throw an exception for any input. Value passed: " +
                            valid[i]);
                }
            }
        }

        /*
         * The following method tests
         *      public static int getIntPropertyValue(int ch, int type)
         */
        [Test]
        public void TestGetIntPropertyValue()
        {
            /* Testing UCharacter.getIntPropertyValue(ch, type) */
            // Testing when "if (type < UProperty.BINARY_START)" is true
            int[] negative_cases = { -100, -50, -10, -5, -2, -1 };
            for (int i = 0; i < negative_cases.Length; i++)
            {
                if (UChar.GetIntPropertyValue(0, (UProperty)negative_cases[i]) != 0)
                {
                    Errln("UCharacter.getIntPropertyValue(ch, type) was suppose to return 0 " +
                            "when passing a negative value of " + negative_cases[i]);

                }
            }

            // Testing when "if(ch<NormalizerImpl.JAMO_L_BASE)" is true
            for (int i = Hangul.JamoLBase - 5; i < Hangul.JamoLBase; i++)
            {
                if (UChar.GetIntPropertyValue(i, UProperty.Hangul_Syllable_Type) != 0)
                {
                    Errln("UCharacter.getIntPropertyValue(ch, type) was suppose to return 0 " +
                            "when passing ch: " + i + "and type of Property.HANGUL_SYLLABLE_TYPE");

                }
            }

            // Testing when "else if((ch-=NormalizerImpl.HANGUL_BASE)<0)" is true
            for (int i = Hangul.HangulBase - 5; i < Hangul.HangulBase; i++)
            {
                if (UChar.GetIntPropertyValue(i, UProperty.Hangul_Syllable_Type) != 0)
                {
                    Errln("UCharacter.getIntPropertyValue(ch, type) was suppose to return 0 " +
                            "when passing ch: " + i + "and type of Property.HANGUL_SYLLABLE_TYPE");

                }
            }
        }

        /*
         * The following method tests
         *      public static int getIntPropertyMaxValue(int type)
         */
        [Test]
        public void TestGetIntPropertyMaxValue()
        {
            /* Testing UCharacter.getIntPropertyMaxValue(type) */
            // Testing when "else if (type < UProperty.INT_START)" is true
            UProperty[] cases = {UPropertyConstants.Binary_Limit, UPropertyConstants.Binary_Limit+1,
                UPropertyConstants.Int_Start-2, UPropertyConstants.Int_Start-1};
            for (int i = 0; i < cases.Length; i++)
            {
                if (UChar.GetIntPropertyMaxValue(cases[i]) != -1)
                {
                    Errln("UCharacter.getIntPropertyMaxValue was suppose to return -1 " +
                            "but got " + UChar.GetIntPropertyMaxValue(cases[i]));
                }
            }

            // TODO: Testing when the case statment reaches "default"
            // After testing between values of UProperty.INT_START and
            // UProperty.INT_LIMIT are covered, none of the values reaches default.
        }

        /*
         * The following method tests
         *      public static final int codePointAt(CharSequence seq, int index)
         *      public static final int codePointAt(char[] text, int index, int limit)
         */
        [Test]
        public void TestCodePointAt()
        {

            // {LEAD_SURROGATE_MIN_VALUE,
            //  LEAD_SURROGATE_MAX_VALUE, LEAD_SURROGATE_MAX_VALUE-1
            String[] cases = { "\uD800", "\uDBFF", "\uDBFE" };
            int[] result = { 55296, 56319, 56318 };
            for (int i = 0; i < cases.Length; i++)
            {
                /* Testing UCharacter.CodePointAt(seq, index) */
                // Testing when "if (index < seq.Length)" is false
                if (UChar.CodePointAt(cases[i], 0) != result[i])
                    Errln("UCharacter.CodePointAt(CharSequence ...) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointAt(cases[i], 0));

                /* Testing UCharacter.CodePointAt(text, index) */
                // Testing when "if (index < text.Length)" is false
                if (UChar.CodePointAt(cases[i].ToCharArray(), 0) != result[i])
                    Errln("UCharacter.CodePointAt(char[] ...) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointAt(cases[i].ToCharArray(), 0));

                /* Testing UCharacter.CodePointAt(text, index, limit) */
                // Testing when "if (index < limit)" is false
                if (UChar.CodePointAt(cases[i].ToCharArray(), 0, 1) != result[i])
                    Errln("UCharacter.CodePointAt(char[], int, int) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointAt(cases[i].ToCharArray(), 0, 1));
            }

            /* Testing UCharacter.CodePointAt(text, index, limit) */
            // Testing when "if (index >= limit || limit > text.Length)" is true
            char[] empty_text = { };
            char[] one_char_text = { 'a' };
            char[] reg_text = { 'd', 'u', 'm', 'm', 'y' };
            int[] limitCases = { 2, 3, 5, 10, 25 };

            // When index >= limit
            for (int i = 0; i < limitCases.Length; i++)
            {
                try
                {
                    UChar.CodePointAt(reg_text, 100, limitCases[i]);
                    Errln("UCharacter.codePointAt was suppose to return an exception " +
                            "but got " + UChar.CodePointAt(reg_text, 100, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(reg_text) + ", Start: " +
                            100 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }
            }

            // When limit > text.Length
            for (int i = 0; i < limitCases.Length; i++)
            {
                try
                {
                    UChar.CodePointAt(empty_text, 0, limitCases[i]);
                    Errln("UCharacter.codePointAt was suppose to return an exception " +
                            "but got " + UChar.CodePointAt(empty_text, 0, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(empty_text) + ", Start: " +
                            0 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }

                try
                {
                    UChar.CodePointCount(one_char_text, 0, limitCases[i]);
                    Errln("UCharacter.codePointCount was suppose to return an exception " +
                            "but got " + UChar.CodePointCount(one_char_text, 0, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(one_char_text) + ", Start: " +
                            0 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }
            }
        }

        /*
         * The following method tests
         *      public static final int codePointBefore(CharSequence seq, int index)
         *      public static final int codePointBefore(char[] text, int index)
         *      public static final int codePointBefore(char[] text, int index, int limit)
         */
        [Test]
        public void TestCodePointBefore()
        {
            // {TRAIL_SURROGATE_MIN_VALUE,
            //  TRAIL_SURROGATE_MAX_VALUE, TRAIL_SURROGATE_MAX_VALUE -1
            String[] cases = { "\uDC00", "\uDFFF", "\uDDFE" };
            int[] result = { 56320, 57343, 56830 };
            for (int i = 0; i < cases.Length; i++)
            {
                /* Testing UCharacter.CodePointBefore(seq, index) */
                // Testing when "if (index > 0)" is false
                if (UChar.CodePointBefore(cases[i], 1) != result[i])
                    Errln("UCharacter.CodePointBefore(CharSequence ...) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointBefore(cases[i], 1));

                /* Testing UCharacter.CodePointBefore(text, index) */
                // Testing when "if (index > 0)" is false
                if (UChar.CodePointBefore(cases[i].ToCharArray(), 1) != result[i])
                    Errln("UCharacter.CodePointBefore(char[] ...) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointBefore(cases[i].ToCharArray(), 1));

                /* Testing UCharacter.CodePointBefore(text, index, limit) */
                // Testing when "if (index > limit)" is false
                if (UChar.CodePointBefore(cases[i].ToCharArray(), 1, 0) != result[i])
                    Errln("UCharacter.CodePointBefore(char[], int, int) did not return as expected. " +
                            "Passed value: " + cases[i] + ". Expected: " +
                            result[i] + ". Got: " +
                            UChar.CodePointBefore(cases[i].ToCharArray(), 1, 0));
            }

            /* Testing UCharacter.CodePointBefore(text, index, limit) */
            char[] dummy = { 'd', 'u', 'm', 'm', 'y' };
            // Testing when "if (index <= limit || limit < 0)" is true
            int[] negative_cases = { -100, -10, -5, -2, -1 };
            int[] index_cases = { 0, 1, 2, 5, 10, 100 };

            for (int i = 0; i < negative_cases.Length; i++)
            {
                try
                {
                    UChar.CodePointBefore(dummy, 10000, negative_cases[i]);
                    Errln("UCharacter.CodePointBefore(text, index, limit) was suppose to return an exception " +
                            "when the parameter limit of " + negative_cases[i] + " is a negative number.");
                }
                catch (Exception e) { }
            }

            for (int i = 0; i < index_cases.Length; i++)
            {
                try
                {
                    UChar.CodePointBefore(dummy, index_cases[i], 101);
                    Errln("UCharacter.CodePointBefore(text, index, limit) was suppose to return an exception " +
                            "when the parameter index of " + index_cases[i] + " is a negative number.");
                }
                catch (Exception e) { }
            }
        }

        /*
         * The following method tests
         *      public static final int toChars(int cp, char[] dst, int dstIndex)
         *      public static final char[] toChars(int cp)
         */
        [Test]
        public void TestToChars()
        {
            int[] positive_cases = { 1, 2, 5, 10, 100 };
            char[] dst = { 'a' };

            /* Testing UCharacter.toChars(cp, dst, dstIndex) */
            for (int i = 0; i < positive_cases.Length; i++)
            {
                // Testing negative values when cp < 0 for if (cp >= 0)
                try
                {
                    UChar.ToChars(-1 * positive_cases[i], dst, 0);
                    Errln("UCharacter.toChars(int,char[],int) was suppose to return an exception " +
                            "when the parameter " + (-1 * positive_cases[i]) + " is a negative number.");
                }
                catch (Exception e)
                {
                }

                // Testing when "if (cp < MIN_SUPPLEMENTARY_CODE_POINT)" is true
                if (UChar.ToChars(UChar.MinSupplementaryCodePoint - positive_cases[i], dst, 0) != 1)
                {
                    Errln("UCharacter.toChars(int,char[],int) was suppose to return a value of 1. Got: " +
                            UChar.ToChars(UChar.MinSupplementaryCodePoint - positive_cases[i], dst, 0));
                }

                // Testing when "if (cp < MIN_SUPPLEMENTARY_CODE_POINT)" is false and
                //     when "if (cp <= MAX_CODE_POINT)" is false
                try
                {
                    UChar.ToChars(UChar.MaxCodePoint + positive_cases[i], dst, 0);
                    Errln("UCharacter.toChars(int,char[],int) was suppose to return an exception " +
                            "when the parameter " + (UChar.MaxCodePoint + positive_cases[i]) +
                            " is a large number.");
                }
                catch (Exception e)
                {
                }
            }


            /* Testing UCharacter.toChars(cp)*/
            for (int i = 0; i < positive_cases.Length; i++)
            {
                // Testing negative values when cp < 0 for if (cp >= 0)
                try
                {
                    UChar.ToChars(-1 * positive_cases[i]);
                    Errln("UCharacter.toChars(cint) was suppose to return an exception " +
                            "when the parameter " + positive_cases[i] + " is a negative number.");
                }
                catch (Exception e)
                {
                }

                // Testing when "if (cp < MIN_SUPPLEMENTARY_CODE_POINT)" is true
                if (UChar.ToChars(UChar.MinSupplementaryCodePoint - positive_cases[i]).Length <= 0)
                {
                    Errln("UCharacter.toChars(int) was suppose to return some result result when the parameter " +
                            (UChar.MinSupplementaryCodePoint - positive_cases[i]) + "is passed.");
                }

                // Testing when "if (cp < MIN_SUPPLEMENTARY_CODE_POINT)" is false and
                //     when "if (cp <= MAX_CODE_POINT)" is false
                try
                {
                    UChar.ToChars(UChar.MaxCodePoint + positive_cases[i]);
                    Errln("UCharacter.toChars(int) was suppose to return an exception " +
                            "when the parameter " + positive_cases[i] + " is a large number.");
                }
                catch (Exception e)
                {
                }
            }
        }

        /*
         * The following method tests
         *      public static int codePointCount(CharSequence text, int start, int limit)
         *      public static int codePointCount(char[] text, int start, int limit)
         */
        [Test]
        public void TestCodePointCount()
        {
            // The following tests the first if statement to make it true:
            //  if (start < 0 || limit < start || limit > text.Length)
            //  which will throw an exception.
            char[] empty_text = { };
            char[] one_char_text = { 'a' };
            char[] reg_text = { 'd', 'u', 'm', 'm', 'y' };
            int[] invalid_startCases = { -1, -2, -5, -10, -100 };
            int[] limitCases = { 2, 3, 5, 10, 25 };

            // When start < 0
            for (int i = 0; i < invalid_startCases.Length; i++)
            {
                try
                {
                    UChar.CodePointCount(reg_text, invalid_startCases[i], 1);
                    Errln("UCharacter.codePointCount was suppose to return an exception " +
                            "but got " + UChar.CodePointCount(reg_text, invalid_startCases[i], 1) +
                            ". The following passed parameters were Text: " + new string(reg_text) + ", Start: " +
                            invalid_startCases[i] + ", Limit: " + 1 + ".");
                }
                catch (Exception e)
                {
                }
            }

            // When limit < start
            for (int i = 0; i < limitCases.Length; i++)
            {
                try
                {
                    UChar.CodePointCount(reg_text, 100, limitCases[i]);
                    Errln("UCharacter.codePointCount was suppose to return an exception " +
                            "but got " + UChar.CodePointCount(reg_text, 100, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(reg_text) + ", Start: " +
                            100 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }
            }

            // When limit > text.Length
            for (int i = 0; i < limitCases.Length; i++)
            {
                try
                {
                    UChar.CodePointCount(empty_text, 0, limitCases[i]);
                    Errln("UCharacter.codePointCount was suppose to return an exception " +
                            "but got " + UChar.CodePointCount(empty_text, 0, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(empty_text) + ", Start: " +
                            0 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }

                try
                {
                    UChar.CodePointCount(one_char_text, 0, limitCases[i]);
                    Errln("UCharacter.codePointCount was suppose to return an exception " +
                            "but got " + UChar.CodePointCount(one_char_text, 0, limitCases[i]) +
                            ". The following passed parameters were Text: " + new string(one_char_text) + ", Start: " +
                            0 + ", Limit: " + limitCases[i] + ".");
                }
                catch (Exception e)
                {
                }
            }
        }

        /*
         * The following method tests
         *      private static int getEuropeanDigit(int ch)
         * The method needs to use the method "digit" in order to access the
         * getEuropeanDigit method.
         */
        [Test]
        public void TestGetEuropeanDigit()
        {
            //The number retrieved from 0xFF41 to 0xFF5A is due to
            //  exhaustive testing from UTF16.CODEPOINT_MIN_VALUE to
            //  UTF16.CODEPOINT_MAX_VALUE return a value of -1.

            int[] radixResult = {
                10,11,12,13,14,15,16,17,18,19,20,21,22,
                23,24,25,26,27,28,29,30,31,32,33,34,35};
            // Invalid and too-small-for-these-digits radix values.
            int[] radixCase1 = { 0, 1, 5, 10, 100 };
            // Radix values that work for at least some of the "digits".
            int[] radixCase2 = { 12, 16, 20, 36 };

            for (int i = 0xFF41; i <= 0xFF5A; i++)
            {
                for (int j = 0; j < radixCase1.Length; j++)
                {
                    if (UChar.Digit(i, radixCase1[j]) != -1)
                    {
                        Errln("UCharacter.digit(int,int) was supposed to return -1 for radix " + radixCase1[j]
                                + ". Value passed: U+" + (i).ToHexString() + ". Got: " + UChar.Digit(i, radixCase1[j]));
                    }
                }
                for (int j = 0; j < radixCase2.Length; j++)
                {
                    int radix = radixCase2[j];
                    int expected = (radixResult[i - 0xFF41] < radix) ? radixResult[i - 0xFF41] : -1;
                    int actual = UChar.Digit(i, radix);
                    if (actual != expected)
                    {
                        Errln("UCharacter.digit(int,int) was supposed to return " +
                                expected + " for radix " + radix +
                                ". Value passed: U+" + (i).ToHexString() + ". Got: " + actual);
                        break;
                    }
                }
            }
        }

        /* Tests the method
         *      private static final int getProperty(int ch)
         * from public static int getType(int ch)
         */
        [Test]
        public void TestGetProperty()
        {
            int[] cases = { UTF16.CodePointMaxValue + 1, UTF16.CodePointMaxValue + 2 };
            for (int i = 0; i < cases.Length; i++)
                if (UChar.GetUnicodeCategory(cases[i]).ToInt32() != 0)
                    Errln("UCharacter.getType for testing UCharacter.getProperty "
                            + "did not return 0 for passed value of " + cases[i] +
                            " but got " + UChar.GetUnicodeCategory(cases[i]).ToInt32());
        }

        private class MyXSymbolTable : XSymbolTable { }

        /* Tests the class
         *      abstract public static class XSymbolTable implements SymbolTable
         */
        [Test]
        public void TestXSymbolTable()
        {

            MyXSymbolTable st = new MyXSymbolTable();

            // Tests "public UnicodeMatcher lookupMatcher(int i)"
            if (st.LookupMatcher(0) != null)
                Errln("XSymbolTable.lookupMatcher(int i) was suppose to return null.");

            // Tests "public bool applyPropertyAlias(String propertyName, String propertyValue, UnicodeSet result)"
            if (st.ApplyPropertyAlias("", "", new UnicodeSet()) != false)
                Errln("XSymbolTable.applyPropertyAlias(String propertyName, String propertyValue, UnicodeSet result) was suppose to return false.");

            // Tests "public char[] lookup(String s)"
            if (st.Lookup("") != null)
                Errln("XSymbolTable.lookup(String s) was suppose to return null.");

            // Tests "public String parseReference(String text, ParsePosition pos, int limit)"
            if (st.ParseReference("", null, 0) != null)
                Errln("XSymbolTable.parseReference(String text, ParsePosition pos, int limit) was suppose to return null.");
        }

        /* Tests the method
         *      public bool isFrozen()
         */
        [Test]
        public void TestIsFrozen()
        {
            UnicodeSet us = new UnicodeSet();
            if (us.IsFrozen != false)
                Errln("Unicode.isFrozen() was suppose to return false.");

            us.Freeze();
            if (us.IsFrozen != true)
                Errln("Unicode.isFrozen() was suppose to return true.");
        }

        /* Tests the methods
         *      public static String getNameAlias() and
         *      public static String getCharFromNameAlias()
         */
        [Test]
        public void TestNameAliasing()
        {
            int input = '\u01a2';
            String alias = UChar.GetNameAlias(input);
            assertEquals("Wrong name alias", "LATIN CAPITAL LETTER GHA", alias);
            int output = UChar.GetCharFromNameAlias(alias);
            assertEquals("alias for '" + input + "'", input, output);
        }
    }
}
