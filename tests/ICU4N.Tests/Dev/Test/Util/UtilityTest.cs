using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.IO;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ICU4N.Dev.Test.Util
{
    /// <summary>
    /// Test of internal Utility class
    /// </summary>
    public class UtilityTest : TestFmwk
    {
        [Test]
        public void TestUnescape()
        {
            string input =
                "Sch\\u00f6nes Auto: \\u20ac 11240.\\fPrivates Zeichen: \\U00102345\\e\\cC\\n \\x1b\\x{263a}";

            //string expect =
            //    "Sch\u00F6nes Auto: \u20AC 11240.\u000CPrivates Zeichen: \uDBC8\uDF45\u001B\u0003\012 \u001B\u263A";

            // ICU4N specific - converted \012 (octal) to \x000a (hex) because .NET does not support octal strings.
            string expect =
                "Sch\u00F6nes Auto: \u20AC 11240.\u000CPrivates Zeichen: \uDBC8\uDF45\u001B\u0003\x000a \u001B\u263A";

            string result = Utility.Unescape(input);
            if (!result.Equals(expect))
            {
                Errln("FAIL: Utility.unescape() returned " + result + ", exp. " + expect);
            }
        }

        [Test]
        public void TestFormat()
        {
            string[] data = {
                "the quick brown fox jumps over the lazy dog",
                // result of this conversion will exceed the original length and
                // cause a newline to be inserted
                "testing space , quotations \"",
                "testing weird supplementary characters \ud800\udc00",
                "testing control characters \u0001 and line breaking!! \n are we done yet?"
            };
            string[] result = {
                "        \"the quick brown fox jumps over the lazy dog\"",
                "        \"testing space , quotations \\042\"",
                "        \"testing weird supplementary characters \\uD800\\uDC00\"",
                "        \"testing control characters \\001 and line breaking!! \\n are we done ye\"+"
                         + Utility.LINE_SEPARATOR + "        \"t?\""
            };
            string[] result1 = {
                "\"the quick brown fox jumps over the lazy dog\"",
                "\"testing space , quotations \\042\"",
                "\"testing weird supplementary characters \\uD800\\uDC00\"",
                "\"testing control characters \\001 and line breaking!! \\n are we done yet?\""
            };

            for (int i = 0; i < data.Length; i++)
            {
                assertEquals("formatForSource(\"" + data[i] + "\")",
                             result[i], Utility.FormatForSource(data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                assertEquals("format1ForSource(\"" + data[i] + "\")",
                             result1[i], Utility.Format1ForSource(data[i]));
            }
        }

        [Test]
        public void TestHighBit()
        {
            int[] data = { -1, -1276, 0, 0xFFFF, 0x1234 };
            sbyte[] result = { -1, -1, -1, 15, 12 };
            for (int i = 0; i < data.Length; i++)
            {
                if (Utility.HighBit(data[i]) != result[i])
                {
                    Errln("Fail: Highest bit of \\u"
                          + (data[i]).ToHexString() + " should be "
                          + result[i]);
                }
            }
        }

        [Test]
        public void TestCompareUnsigned()
        {
            int[] data = {0, 1, unchecked((int)0x8fffffff), -1, int.MaxValue,
                      int.MinValue, 2342423, -2342423};
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    if (Utility.CompareUnsigned(data[i], data[j])
                        != compareLongUnsigned(data[i], data[j]))
                    {
                        Errln("Fail: Unsigned comparison failed with " + data[i]
                              + " " + data[i + 1]);
                    }
                }
            }
        }

        // This test indends to test the utility class ByteArrayWrapper
        // Seems that the class is somewhat incomplete, for example
        //      - getHashCode(Object) is weird
        //      - PatternMatch feature(search part of array within the whole one) lacks
        [Test]
        public void TestByteArrayWrapper()
        {
            byte[] ba = { 0x00, 0x01, 0x02 };
            sbyte[] bb = { 0x00, 0x01, 0x02, -1 };

            ByteBuffer buffer = ByteBuffer.Wrap(ba);
            ByteArrayWrapper x = new ByteArrayWrapper(buffer);

            ByteArrayWrapper y = new ByteArrayWrapper(ba, 3);
            ByteArrayWrapper z = new ByteArrayWrapper((byte[])(Array)bb, 3);


            if (!y.ToString().Equals("00 01 02"))
            {
                Errln("FAIL: test toString : Failed!");
            }

            // test equality
            if (!x.Equals(y) || !x.Equals(z))
                Errln("FAIL: test (operator ==): Failed!");
            if (x.GetHashCode() != y.GetHashCode())
                Errln("FAIL: identical objects have different hash codes.");

            // test non-equality
            y = new ByteArrayWrapper((byte[])(Array)bb, 4);
            if (x.Equals(y))
                Errln("FAIL: test (operator !=): Failed!");

            // test sign of unequal comparison
            if ((x.CompareTo(y) > 0) != (y.CompareTo(x) < 0))
            {
                Errln("FAIL: comparisons not opposite sign");
            }
        }

        private int compareLongUnsigned(int x, int y)
        {
            long x1 = x & 0xFFFFFFFFL;
            long y1 = y & 0xFFFFFFFFL;
            if (x1 < y1)
            {
                return -1;
            }
            else if (x1 > y1)
            {
                return 1;
            }
            return 0;
        }
        [Test]
        public void TestUnicodeSet()
        {
            string[] array = new string[] { "a", "b", "c", "{de}" };
            List<string> list = array.ToList();
            ISet<string> aset = new HashSet<string>(list);
            Logln(" *** The source set's size is: " + aset.Count);
            //The size reads 4
            UnicodeSet set = new UnicodeSet();
            set.Clear();
            set.AddAll(aset);
            Logln(" *** After addAll, the UnicodeSet size is: " + set.Count);
            //The size should also read 4, but 0 is seen instead

        }

        [Test]
        public void TestAssert()
        {
            try
            {
                ICU4N.Impl.Assert.Assrt(false);
                Errln("FAIL: Assert.assrt(false)");
            }
            catch (InvalidOperationException e)
            {
                if (e.Message.Equals("assert failed"))
                {
                    Logln("Assert.assrt(false) works");
                }
                else
                {
                    Errln("FAIL: Assert.assrt(false) returned " + e.Message);
                }
            }
            try
            {
                ICU4N.Impl.Assert.Assrt("Assert message", false);
                Errln("FAIL: Assert.assrt(false)");
            }
            catch (InvalidOperationException e)
            {
                if (e.Message.Equals("assert 'Assert message' failed"))
                {
                    Logln("Assert.assrt(false) works");
                }
                else
                {
                    Errln("FAIL: Assert.assrt(false) returned " + e.Message);
                }
            }
            try
            {
                ICU4N.Impl.Assert.Fail("Assert message");
                Errln("FAIL: Assert.fail");
            }
            catch (InvalidOperationException e)
            {
                if (e.Message.Equals("failure 'Assert message'"))
                {
                    Logln("Assert.fail works");
                }
                else
                {
                    Errln("FAIL: Assert.fail returned " + e.Message);
                }
            }
            try
            {
                ICU4N.Impl.Assert.Fail(new InvalidFormatException());
                Errln("FAIL: Assert.fail with an exception");
            }
            catch (InvalidOperationException e)
            {
                Logln("Assert.fail works");
            }
        }

        [Test]
        public void TestCaseInsensitiveString()
        {
            CaseInsensitiveString str1 = new CaseInsensitiveString("ThIs is A tEst");
            CaseInsensitiveString str2 = new CaseInsensitiveString("This IS a test");
            if (!str1.Equals(str2)
                || !str1.ToString().Equals(str1.String)
                || str1.ToString().Equals(str2.ToString()))
            {
                Errln("FAIL: str1(" + str1 + ") != str2(" + str2 + ")");
            }
        }


        [Test]
        [Ignore("ICU4N: This test only checks to see if the stack trace can be read in the test framework, however no tests depend on this functionality, and it fails to work when compiler optimizations are enabled.")]
        [MethodImpl(MethodImplOptions.NoInlining)] // ICU4N NOTE: This attribute is required for whatever method calls TestFmwk.SourceLocation() to work in Release mode in .NET Standard 1.x
        public void TestSourceLocation()
        {
            string here = TestFmwk.SourceLocation();
            string there = CheckSourceLocale();
            string hereAgain = TestFmwk.SourceLocation();
            assertTrue("here < there < hereAgain", here.CompareToOrdinal(there) < 0 && there.CompareToOrdinal(hereAgain) < 0);
        }

        public string CheckSourceLocale()
        {
            return TestFmwk.SourceLocation();
        }
    }
}
