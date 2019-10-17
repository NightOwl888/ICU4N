using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using ICU4N.Text.Unicode;
using NUnit.Framework;
using System;
using System.Reflection;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Test JDK 1.5 cover APIs.
    /// </summary>
    public sealed class UCharacterSurrogateTest : TestFmwk
    {
        [Test]
        public void TestUnicodeBlockForName()
        {
            String[] names = {"Latin-1 Supplement",
                        "Optical Character Recognition",
                        "CJK Unified Ideographs Extension A",
                        "Supplemental Arrows-B",
                        "Supplemental arrows b",
                        "supp-lement-al arrowsb",
                        "Supplementary Private Use Area-B",
                        "supplementary_Private_Use_Area-b",
                        "supplementary_PRIVATE_Use_Area_b"};
            for (int i = 0; i < names.Length; ++i)
            {
                try
                {
                    UnicodeBlock b = UnicodeBlock
                            .ForName(names[i]);
                    Logln("found: " + b + " for name: " + names[i]);
                }
                catch (Exception e)
                {
                    Errln("could not find block for name: " + names[i]);
                    break;
                }
            }
        }

        [Test]
        public void TestIsValidCodePoint()
        {
            if (UChar.IsValidCodePoint(-1))
                Errln("-1");
            if (!UChar.IsValidCodePoint(0))
                Errln("0");
            if (!UChar.IsValidCodePoint(UChar.MaxCodePoint))
                Errln("0x10ffff");
            if (UChar.IsValidCodePoint(UChar.MaxCodePoint + 1))
                Errln("0x110000");
        }

        [Test]
        public void TestIsSupplementaryCodePoint()
        {
            if (UChar.IsSupplementaryCodePoint(-1))
                Errln("-1");
            if (UChar.IsSupplementaryCodePoint(0))
                Errln("0");
            if (UChar
                    .IsSupplementaryCodePoint(UChar.MinSupplementaryCodePoint - 1))
                Errln("0xffff");
            if (!UChar
                    .IsSupplementaryCodePoint(UChar.MinSupplementaryCodePoint))
                Errln("0x10000");
            if (!UChar.IsSupplementaryCodePoint(UChar.MaxCodePoint))
                Errln("0x10ffff");
            if (UChar.IsSupplementaryCodePoint(UChar.MaxCodePoint + 1))
                Errln("0x110000");
        }

        [Test]
        public void TestIsHighSurrogate()
        {
            if (UChar
                    .IsHighSurrogate((char)(UChar.MinHighSurrogate - 1)))
                Errln("0xd7ff");
            if (!UChar.IsHighSurrogate(UChar.MinHighSurrogate))
                Errln("0xd800");
            if (!UChar.IsHighSurrogate(UChar.MaxHighSurrogate))
                Errln("0xdbff");
            if (UChar
                    .IsHighSurrogate((char)(UChar.MaxHighSurrogate + 1)))
                Errln("0xdc00");
        }

        [Test]
        public void TestIsLowSurrogate()
        {
            if (UChar
                    .IsLowSurrogate((char)(UChar.MinLowSurrogate - 1)))
                Errln("0xdbff");
            if (!UChar.IsLowSurrogate(UChar.MinLowSurrogate))
                Errln("0xdc00");
            if (!UChar.IsLowSurrogate(UChar.MaxLowSurrogate))
                Errln("0xdfff");
            if (UChar
                    .IsLowSurrogate((char)(UChar.MaxLowSurrogate + 1)))
                Errln("0xe000");
        }

        [Test]
        public void TestIsSurrogatePair()
        {
            if (UChar.IsSurrogatePair(
                    (char)(UChar.MinHighSurrogate - 1),
                    UChar.MinLowSurrogate))
                Errln("0xd7ff,0xdc00");
            if (UChar.IsSurrogatePair(
                    (char)(UChar.MaxHighSurrogate + 1),
                    UChar.MinLowSurrogate))
                Errln("0xd800,0xdc00");
            if (UChar.IsSurrogatePair(UChar.MinHighSurrogate,
                    (char)(UChar.MinLowSurrogate - 1)))
                Errln("0xd800,0xdbff");
            if (UChar.IsSurrogatePair(UChar.MinHighSurrogate,
                    (char)(UChar.MaxLowSurrogate + 1)))
                Errln("0xd800,0xe000");
            if (!UChar.IsSurrogatePair(UChar.MinHighSurrogate,
                    UChar.MinLowSurrogate))
                Errln("0xd800,0xdc00");
        }

        [Test]
        public void TestCharCount()
        {
            UChar.CharCount(-1);
            UChar.CharCount(UChar.MaxCodePoint + 1);
            if (UChar.CharCount(UChar.MinSupplementaryCodePoint - 1) != 1)
                Errln("0xffff");
            if (UChar.CharCount(UChar.MinSupplementaryCodePoint) != 2)
                Errln("0x010000");
        }

        [Test]
        public void TestToCodePoint()
        {
            char[] pairs = {(char) (UChar.MinHighSurrogate + 0),
                (char) (UChar.MinLowSurrogate + 0),
                (char) (UChar.MinHighSurrogate + 1),
                (char) (UChar.MinLowSurrogate + 1),
                (char) (UChar.MinHighSurrogate + 2),
                (char) (UChar.MinLowSurrogate + 2),
                (char) (UChar.MaxHighSurrogate - 2),
                (char) (UChar.MaxLowSurrogate - 2),
                (char) (UChar.MaxHighSurrogate - 1),
                (char) (UChar.MaxLowSurrogate - 1),
                (char) (UChar.MaxHighSurrogate - 0),
                (char) (UChar.MaxLowSurrogate - 0),};
            for (int i = 0; i < pairs.Length; i += 2)
            {
                int cp = UChar.ToCodePoint(pairs[i], pairs[i + 1]);
                if (pairs[i] != UTF16.GetLeadSurrogate(cp)
                        || pairs[i + 1] != UTF16.GetTrailSurrogate(cp))
                {

                    Errln((pairs[i]).ToHexString() + ", " + pairs[i + 1]);
                    break;
                }
            }
        }

        [Test]
        public void TestCodePointAtBefore()
        {
            String s = "" + UChar.MinHighSurrogate + // isolated high
                    UChar.MinHighSurrogate + // pair
                    UChar.MinLowSurrogate + UChar.MinLowSurrogate; // isolated
                                                                                 // low
            char[] c = s.ToCharArray();
            int[] avalues = {
                UChar.MinHighSurrogate,
                UChar.ToCodePoint(UChar.MinHighSurrogate,
                        UChar.MinLowSurrogate),
                UChar.MinLowSurrogate, UChar.MinLowSurrogate};
            int[] bvalues = {
                UChar.MinHighSurrogate,
                UChar.MinHighSurrogate,
                UChar.ToCodePoint(UChar.MinHighSurrogate,
                        UChar.MinLowSurrogate),
                UChar.MinLowSurrogate,};
            StringBuffer b = new StringBuffer(s);
            for (int i = 0; i < avalues.Length; ++i)
            {
                if (UChar.CodePointAt(s, i) != avalues[i])
                    Errln("string at: " + i);
                if (UChar.CodePointAt(c, i) != avalues[i])
                    Errln("chars at: " + i);
                if (UChar.CodePointAt(b, i) != avalues[i])
                    Errln("stringbuffer at: " + i);

                if (UChar.CodePointBefore(s, i + 1) != bvalues[i])
                    Errln("string before: " + i);
                if (UChar.CodePointBefore(c, i + 1) != bvalues[i])
                    Errln("chars before: " + i);
                if (UChar.CodePointBefore(b, i + 1) != bvalues[i])
                    Errln("stringbuffer before: " + i);
            }

            //cover codePointAtBefore with limit
            Logln("Testing codePointAtBefore with limit ...");
            for (int i = 0; i < avalues.Length; ++i)
            {
                if (UChar.CodePointAt(c, i, 4) != avalues[i])
                    Errln("chars at: " + i);
                if (UChar.CodePointBefore(c, i + 1, 0) != bvalues[i])
                    Errln("chars before: " + i);
            }

        }

        [Test]
        public void TestToChars()
        {
            char[] chars = new char[3];
            int cp = UChar.ToCodePoint(UChar.MinHighSurrogate,
                    UChar.MinLowSurrogate);
            UChar.ToChars(cp, chars, 1);
            if (chars[1] != UChar.MinHighSurrogate
                    || chars[2] != UChar.MinLowSurrogate)
            {

                Errln("fail");
            }

            chars = UChar.ToChars(cp);
            if (chars[0] != UChar.MinHighSurrogate
                    || chars[1] != UChar.MinLowSurrogate)
            {

                Errln("fail");
            }
        }

        private class CodePointCountTest
        {
            internal String Str(String s, int start, int limit)
            {
                if (s == null)
                {
                    s = "";
                }
                return "codePointCount('" + Utility.Escape(s) + "' " + start
                        + ", " + limit + ")";
            }

            internal void Test(String s, int start, int limit, int expected)
            {
                int val1 = UChar.CodePointCount(s.ToCharArray(), start,
                        limit);
                int val2 = UChar.CodePointCount(s, start, limit);
                if (val1 != expected)
                {
                    Errln("char[] " + Str(s, start, limit) + "(" + val1
                            + ") != " + expected);
                }
                else if (val2 != expected)
                {
                    Errln("String " + Str(s, start, limit) + "(" + val2
                            + ") != " + expected);
                }
                else if (IsVerbose())
                {
                    Logln(Str(s, start, limit) + " == " + expected);
                }
            }

            internal void Fail(String s, int start, int limit, Type exc)
            {
                try
                {
                    UChar.CodePointCount(s, start, limit);
                    Errln("unexpected success " + Str(s, start, limit));
                }
                catch (Exception e)
                {
                    //if (!exc.GetTypeInfo().isInstance(e))
                    if (!exc.IsAssignableFrom(e.GetType()))
                    {
                        Warnln("bad exception " + Str(s, start, limit)
                                + e.GetType().Name);
                    }
                }
            }
        }

        [Test]
        public void TestCodePointCount()
        {


            CodePointCountTest test = new CodePointCountTest();
            test.Fail(null, 0, 1, typeof(ArgumentNullException));
            test.Fail("a", -1, 0, typeof(IndexOutOfRangeException));
            test.Fail("a", 1, 2, typeof(IndexOutOfRangeException));
            test.Fail("a", 1, 0, typeof(IndexOutOfRangeException));
            test.Test("", 0, 0, 0);
            test.Test("\ud800", 0, 1, 1);
            test.Test("\udc00", 0, 1, 1);
            test.Test("\ud800\udc00", 0, 1, 1);
            test.Test("\ud800\udc00", 1, 2, 1);
            test.Test("\ud800\udc00", 0, 2, 1);
            test.Test("\udc00\ud800", 0, 1, 1);
            test.Test("\udc00\ud800", 1, 2, 1);
            test.Test("\udc00\ud800", 0, 2, 2);
            test.Test("\ud800\ud800\udc00", 0, 2, 2);
            test.Test("\ud800\ud800\udc00", 1, 3, 1);
            test.Test("\ud800\ud800\udc00", 0, 3, 2);
            test.Test("\ud800\udc00\udc00", 0, 2, 1);
            test.Test("\ud800\udc00\udc00", 1, 3, 2);
            test.Test("\ud800\udc00\udc00", 0, 3, 2);
        }

        private class OffsetByCodePointsTest
        {
            internal String Str(String s, int start, int count, int index, int offset)
            {
                return "offsetByCodePoints('" + Utility.Escape(s) + "' "
                        + start + ", " + count + ", " + index + ", " + offset
                        + ")";
            }

            internal void Test(String s, int start, int count, int index, int offset,
                    int expected, bool flip)
            {
                char[] chars = s.ToCharArray();
                String strng = s.Substring(start, count); // ICU4N: (start + count) - start == count
                int val1 = UChar.OffsetByCodePoints(chars, start, count,
                        index, offset);
                int val2 = UChar.OffsetByCodePoints(strng, index - start,
                        offset)
                        + start;

                if (val1 != expected)
                {
                    TestFmwk.Errln("char[] " + Str(s, start, count, index, offset) + "("
                            + val1 + ") != " + expected);
                }
                else if (val2 != expected)
                {
                    TestFmwk.Errln("String " + Str(s, start, count, index, offset) + "("
                            + val2 + ") != " + expected);
                }
                else if (TestFmwk.IsVerbose())
                {
                    TestFmwk.Logln(Str(s, start, count, index, offset) + " == "
                            + expected);
                }

                if (flip)
                {
                    val1 = UChar.OffsetByCodePoints(chars, start, count,
                            expected, -offset);
                    val2 = UChar.OffsetByCodePoints(strng, expected
                            - start, -offset)
                            + start;
                    if (val1 != index)
                    {
                        TestFmwk.Errln("char[] "
                                + Str(s, start, count, expected, -offset) + "("
                                + val1 + ") != " + index);
                    }
                    else if (val2 != index)
                    {
                        TestFmwk.Errln("String "
                                + Str(s, start, count, expected, -offset) + "("
                                + val2 + ") != " + index);
                    }
                    else if (TestFmwk.IsVerbose())
                    {
                        TestFmwk.Logln(Str(s, start, count, expected, -offset) + " == "
                                + index);
                    }
                }
            }

            internal void Fail(char[] text, int start, int count, int index, int offset,
                    Type exc)
            {
                try
                {
                    UChar.OffsetByCodePoints(text, start, count, index,
                            offset);
                    Errln("unexpected success "
                            + Str(new String(text), start, count, index, offset));
                }
                catch (Exception e)
                {
                    //if (!exc.isInstance(e))
                    if (!exc.IsAssignableFrom(e.GetType()))
                    {
                        Errln("bad exception "
                                + Str(new String(text), start, count, index,
                                        offset) + e.GetType().Name);
                    }
                }
            }

            internal void Fail(String text, int index, int offset, Type exc)
            {
                try
                {
                    UChar.OffsetByCodePoints(text, index, offset);
                    Errln("unexpected success "
                            + Str(text, index, offset, 0, text.Length));
                }
                catch (Exception e)
                {
                    //if (!exc.isInstance(e))
                    if (!exc.IsAssignableFrom(e.GetType()))
                    {
                        Errln("bad exception "
                                + Str(text, 0, text.Length, index, offset)
                                + e.GetType().Name);
                    }
                }
            }
        }


        [Test]
        public void TestOffsetByCodePoints()
        {


            OffsetByCodePointsTest test = new OffsetByCodePointsTest();

            test.Test("\ud800\ud800\udc00", 0, 2, 0, 1, 1, true);

            test.Fail((char[])null, 0, 1, 0, 1, typeof(ArgumentNullException));
            test.Fail((String)null, 0, 1, typeof(ArgumentNullException));
            test.Fail("abc", -1, 0, typeof(IndexOutOfRangeException));
            test.Fail("abc", 4, 0, typeof(IndexOutOfRangeException));
            test.Fail("abc", 1, -2, typeof(IndexOutOfRangeException));
            test.Fail("abc", 2, 2, typeof(IndexOutOfRangeException));
            char[] abc = "abc".ToCharArray();
            test.Fail(abc, -1, 2, 0, 0, typeof(IndexOutOfRangeException));
            test.Fail(abc, 2, 2, 3, 0, typeof(IndexOutOfRangeException));
            test.Fail(abc, 1, -1, 0, 0, typeof(IndexOutOfRangeException));
            test.Fail(abc, 1, 1, 2, -2, typeof(IndexOutOfRangeException));
            test.Fail(abc, 1, 1, 1, 2, typeof(IndexOutOfRangeException));
            test.Fail(abc, 1, 2, 1, 3, typeof(IndexOutOfRangeException));
            test.Fail(abc, 0, 2, 2, -3, typeof(IndexOutOfRangeException));
            test.Test("", 0, 0, 0, 0, 0, false);
            test.Test("\ud800", 0, 1, 0, 1, 1, true);
            test.Test("\udc00", 0, 1, 0, 1, 1, true);

            String s = "\ud800\udc00";
            test.Test(s, 0, 1, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 1, 2, true);
            test.Test(s, 0, 2, 1, 1, 2, false);
            test.Test(s, 1, 1, 1, 1, 2, true);

            s = "\udc00\ud800";
            test.Test(s, 0, 1, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 2, 2, true);
            test.Test(s, 0, 2, 1, 1, 2, true);
            test.Test(s, 1, 1, 1, 1, 2, true);

            s = "\ud800\ud800\udc00";
            test.Test(s, 0, 1, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 2, 2, true);
            test.Test(s, 0, 2, 1, 1, 2, true);
            test.Test(s, 0, 3, 0, 1, 1, true);
            test.Test(s, 0, 3, 0, 2, 3, true);
            test.Test(s, 0, 3, 1, 1, 3, true);
            test.Test(s, 0, 3, 2, 1, 3, false);
            test.Test(s, 1, 1, 1, 1, 2, true);
            test.Test(s, 1, 2, 1, 1, 3, true);
            test.Test(s, 1, 2, 2, 1, 3, false);
            test.Test(s, 2, 1, 2, 1, 3, true);

            s = "\ud800\udc00\udc00";
            test.Test(s, 0, 1, 0, 1, 1, true);
            test.Test(s, 0, 2, 0, 1, 2, true);
            test.Test(s, 0, 2, 1, 1, 2, false);
            test.Test(s, 0, 3, 0, 1, 2, true);
            test.Test(s, 0, 3, 0, 2, 3, true);
            test.Test(s, 0, 3, 1, 1, 2, false);
            test.Test(s, 0, 3, 1, 2, 3, false);
            test.Test(s, 0, 3, 2, 1, 3, true);
            test.Test(s, 1, 1, 1, 1, 2, true);
            test.Test(s, 1, 2, 1, 1, 2, true);
            test.Test(s, 1, 2, 1, 2, 3, true);
            test.Test(s, 1, 2, 2, 1, 3, true);
            test.Test(s, 2, 1, 2, 1, 3, true);
        }
    }
}
