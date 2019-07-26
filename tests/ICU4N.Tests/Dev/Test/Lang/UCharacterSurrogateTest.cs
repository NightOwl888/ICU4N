using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Text;
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
                    UCharacter.UnicodeBlock b = UCharacter.UnicodeBlock
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
            if (UCharacter.IsValidCodePoint(-1))
                Errln("-1");
            if (!UCharacter.IsValidCodePoint(0))
                Errln("0");
            if (!UCharacter.IsValidCodePoint(UCharacter.MaxCodePoint))
                Errln("0x10ffff");
            if (UCharacter.IsValidCodePoint(UCharacter.MaxCodePoint + 1))
                Errln("0x110000");
        }

        [Test]
        public void TestIsSupplementaryCodePoint()
        {
            if (UCharacter.IsSupplementaryCodePoint(-1))
                Errln("-1");
            if (UCharacter.IsSupplementaryCodePoint(0))
                Errln("0");
            if (UCharacter
                    .IsSupplementaryCodePoint(UCharacter.MinSupplementaryCodePoint - 1))
                Errln("0xffff");
            if (!UCharacter
                    .IsSupplementaryCodePoint(UCharacter.MinSupplementaryCodePoint))
                Errln("0x10000");
            if (!UCharacter.IsSupplementaryCodePoint(UCharacter.MaxCodePoint))
                Errln("0x10ffff");
            if (UCharacter.IsSupplementaryCodePoint(UCharacter.MaxCodePoint + 1))
                Errln("0x110000");
        }

        [Test]
        public void TestIsHighSurrogate()
        {
            if (UCharacter
                    .IsHighSurrogate((char)(UCharacter.MinHighSurrogate - 1)))
                Errln("0xd7ff");
            if (!UCharacter.IsHighSurrogate(UCharacter.MinHighSurrogate))
                Errln("0xd800");
            if (!UCharacter.IsHighSurrogate(UCharacter.MaxHighSurrogate))
                Errln("0xdbff");
            if (UCharacter
                    .IsHighSurrogate((char)(UCharacter.MaxHighSurrogate + 1)))
                Errln("0xdc00");
        }

        [Test]
        public void TestIsLowSurrogate()
        {
            if (UCharacter
                    .IsLowSurrogate((char)(UCharacter.MinLowSurrogate - 1)))
                Errln("0xdbff");
            if (!UCharacter.IsLowSurrogate(UCharacter.MinLowSurrogate))
                Errln("0xdc00");
            if (!UCharacter.IsLowSurrogate(UCharacter.MaxLowSurrogate))
                Errln("0xdfff");
            if (UCharacter
                    .IsLowSurrogate((char)(UCharacter.MaxLowSurrogate + 1)))
                Errln("0xe000");
        }

        [Test]
        public void TestIsSurrogatePair()
        {
            if (UCharacter.IsSurrogatePair(
                    (char)(UCharacter.MinHighSurrogate - 1),
                    UCharacter.MinLowSurrogate))
                Errln("0xd7ff,0xdc00");
            if (UCharacter.IsSurrogatePair(
                    (char)(UCharacter.MaxHighSurrogate + 1),
                    UCharacter.MinLowSurrogate))
                Errln("0xd800,0xdc00");
            if (UCharacter.IsSurrogatePair(UCharacter.MinHighSurrogate,
                    (char)(UCharacter.MinLowSurrogate - 1)))
                Errln("0xd800,0xdbff");
            if (UCharacter.IsSurrogatePair(UCharacter.MinHighSurrogate,
                    (char)(UCharacter.MaxLowSurrogate + 1)))
                Errln("0xd800,0xe000");
            if (!UCharacter.IsSurrogatePair(UCharacter.MinHighSurrogate,
                    UCharacter.MinLowSurrogate))
                Errln("0xd800,0xdc00");
        }

        [Test]
        public void TestCharCount()
        {
            UCharacter.CharCount(-1);
            UCharacter.CharCount(UCharacter.MaxCodePoint + 1);
            if (UCharacter.CharCount(UCharacter.MinSupplementaryCodePoint - 1) != 1)
                Errln("0xffff");
            if (UCharacter.CharCount(UCharacter.MinSupplementaryCodePoint) != 2)
                Errln("0x010000");
        }

        [Test]
        public void TestToCodePoint()
        {
            char[] pairs = {(char) (UCharacter.MinHighSurrogate + 0),
                (char) (UCharacter.MinLowSurrogate + 0),
                (char) (UCharacter.MinHighSurrogate + 1),
                (char) (UCharacter.MinLowSurrogate + 1),
                (char) (UCharacter.MinHighSurrogate + 2),
                (char) (UCharacter.MinLowSurrogate + 2),
                (char) (UCharacter.MaxHighSurrogate - 2),
                (char) (UCharacter.MaxLowSurrogate - 2),
                (char) (UCharacter.MaxHighSurrogate - 1),
                (char) (UCharacter.MaxLowSurrogate - 1),
                (char) (UCharacter.MaxHighSurrogate - 0),
                (char) (UCharacter.MaxLowSurrogate - 0),};
            for (int i = 0; i < pairs.Length; i += 2)
            {
                int cp = UCharacter.ToCodePoint(pairs[i], pairs[i + 1]);
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
            String s = "" + UCharacter.MinHighSurrogate + // isolated high
                    UCharacter.MinHighSurrogate + // pair
                    UCharacter.MinLowSurrogate + UCharacter.MinLowSurrogate; // isolated
                                                                                 // low
            char[] c = s.ToCharArray();
            int[] avalues = {
                UCharacter.MinHighSurrogate,
                UCharacter.ToCodePoint(UCharacter.MinHighSurrogate,
                        UCharacter.MinLowSurrogate),
                UCharacter.MinLowSurrogate, UCharacter.MinLowSurrogate};
            int[] bvalues = {
                UCharacter.MinHighSurrogate,
                UCharacter.MinHighSurrogate,
                UCharacter.ToCodePoint(UCharacter.MinHighSurrogate,
                        UCharacter.MinLowSurrogate),
                UCharacter.MinLowSurrogate,};
            StringBuffer b = new StringBuffer(s);
            for (int i = 0; i < avalues.Length; ++i)
            {
                if (UCharacter.CodePointAt(s, i) != avalues[i])
                    Errln("string at: " + i);
                if (UCharacter.CodePointAt(c, i) != avalues[i])
                    Errln("chars at: " + i);
                if (UCharacter.CodePointAt(b, i) != avalues[i])
                    Errln("stringbuffer at: " + i);

                if (UCharacter.CodePointBefore(s, i + 1) != bvalues[i])
                    Errln("string before: " + i);
                if (UCharacter.CodePointBefore(c, i + 1) != bvalues[i])
                    Errln("chars before: " + i);
                if (UCharacter.CodePointBefore(b, i + 1) != bvalues[i])
                    Errln("stringbuffer before: " + i);
            }

            //cover codePointAtBefore with limit
            Logln("Testing codePointAtBefore with limit ...");
            for (int i = 0; i < avalues.Length; ++i)
            {
                if (UCharacter.CodePointAt(c, i, 4) != avalues[i])
                    Errln("chars at: " + i);
                if (UCharacter.CodePointBefore(c, i + 1, 0) != bvalues[i])
                    Errln("chars before: " + i);
            }

        }

        [Test]
        public void TestToChars()
        {
            char[] chars = new char[3];
            int cp = UCharacter.ToCodePoint(UCharacter.MinHighSurrogate,
                    UCharacter.MinLowSurrogate);
            UCharacter.ToChars(cp, chars, 1);
            if (chars[1] != UCharacter.MinHighSurrogate
                    || chars[2] != UCharacter.MinLowSurrogate)
            {

                Errln("fail");
            }

            chars = UCharacter.ToChars(cp);
            if (chars[0] != UCharacter.MinHighSurrogate
                    || chars[1] != UCharacter.MinLowSurrogate)
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
                int val1 = UCharacter.CodePointCount(s.ToCharArray(), start,
                        limit);
                int val2 = UCharacter.CodePointCount(s, start, limit);
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
                    UCharacter.CodePointCount(s, start, limit);
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
                int val1 = UCharacter.OffsetByCodePoints(chars, start, count,
                        index, offset);
                int val2 = UCharacter.OffsetByCodePoints(strng, index - start,
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
                    val1 = UCharacter.OffsetByCodePoints(chars, start, count,
                            expected, -offset);
                    val2 = UCharacter.OffsetByCodePoints(strng, expected
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
                    UCharacter.OffsetByCodePoints(text, start, count, index,
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
                    UCharacter.OffsetByCodePoints(text, index, offset);
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
