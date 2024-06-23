using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Text;

namespace ICU4N.Dev.Test.Util
{
    public partial class SimpleFormatterTest : TestFmwk
    {
        /**
         * Constructor
         */
        public SimpleFormatterTest()
        {
        }

        // public methods -----------------------------------------------

        [Test]
        public void TestWithNoArguments()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("This doesn''t have templates '{0}");
            Span<char> buffer = stackalloc char[64];
            assertEquals(
                    "getArgumentLimit",
                    0,
                    fmt.ArgumentLimit);
            assertEquals(
                    "format",
                    "This doesn't have templates {0}",
                    fmt.Format("unused"));
            assertEquals(
                    "format",
                    "This doesn't have templates {0}",
                    fmt.TryFormat(buffer, out int charsLength, "unused") ? buffer.Slice(0, charsLength).ToString() : string.Empty); // ICU4N specific
            assertEquals(
                    "format with values=null",
                    "This doesn't have templates {0}",
                    fmt.Format((string[])null));
            assertEquals(
                    "format with values=null",
                    "This doesn't have templates {0}",
                    fmt.TryFormat(buffer, out charsLength, (string[])null) ? buffer.Slice(0, charsLength).ToString() : string.Empty); // ICU4N specific
            assertEquals(
                    "toString",
                    "This doesn't have templates {0}",
                    fmt.ToString());
            int[] offsets = new int[1];
            assertEquals(
                    "formatAndAppend",
                    "This doesn't have templates {0}",
                    fmt.FormatAndAppend(new StringBuilder(), offsets).ToString());
            assertEquals(
                    "offsets[0]",
                    -1,
                    offsets[0]);
            assertEquals(
                    "formatAndAppend with values=null",
                    "This doesn't have templates {0}",
                    fmt.FormatAndAppend(new StringBuilder(), null, (string[])null).ToString());
            assertEquals(
                    "formatAndReplace with values=null",
                    "This doesn't have templates {0}",
                    fmt.FormatAndReplace(new StringBuilder(), null, (string[])null).ToString());
        }

        [Test]
        public void TestSyntaxErrors()
        {
            try
            {
                SimpleFormatter.Compile("{}");
                fail("Syntax error did not yield an exception.");
            }
            catch (ArgumentException expected)
            {
            }
            try
            {
                SimpleFormatter.Compile("{12d");
                fail("Syntax error did not yield an exception.");
            }
            catch (ArgumentException expected)
            {
            }
        }

        [Test]
        public void TestOneArgument()
        {
            string expected = "1 meter";
            SimpleFormatter fmt = SimpleFormatter.Compile("{0} meter");
            assertEquals("TestOneArgument Format",
                    expected,
                    fmt.Format("1"));

            // ICU4N specific
            Span<char> buffer = stackalloc char[32];
            assertTrue("TestOneArgument TryFormat result",
                fmt.TryFormat(buffer, out int charsLength, "1"));
            assertEquals("TestOneArgument TryFormat",
                    expected,
                    buffer.Slice(0, charsLength).ToString());
        }

        [Test]
        public void TestBigArgument()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("a{20}c");
            assertEquals("{20} count", 21, fmt.ArgumentLimit);
            string[] values = new string[21];
            values[20] = "b";
            assertEquals("{20}=b", "abc", fmt.Format(values));
        }

        [Test]
        public void TestGetTextWithNoArguments()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("Templates {1}{2} and {3} are here.");
            string expected = "Templates  and  are here.";
            assertEquals(
                    "",
                    expected,
                    fmt.GetTextWithNoArguments());

            // ICU4N specific
            Span<char> buffer = stackalloc char[40];
            assertTrue("", fmt.TryGetTextWithNoArguments(buffer, out int charsLength));
            assertEquals(
                    "",
                    expected,
                    buffer.Slice(0, charsLength).ToString());
        }

        [Test]
        public void TestTooFewArgumentValues()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile(
                    "Templates {2}{1} and {4} are out of order.");
            Span<char> buffer = stackalloc char[64];
            try
            {
                fmt.Format("freddy", "tommy", "frog", "leg");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }
            try // ICU4N specific
            {
                fmt.TryFormat(buffer, out int charsLength, "freddy", "tommy", "frog", "leg");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }
            try
            {
                fmt.FormatAndAppend(
                        new StringBuilder(), null, "freddy", "tommy", "frog", "leg");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }
            try
            {
                fmt.FormatAndReplace(
                        new StringBuilder(), null, "freddy", "tommy", "frog", "leg");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }
        }

        [Test]
        public void TestWithArguments()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile(
                    "Templates {2}{1} and {4} are out of order.");
            assertEquals(
                    "getArgumentLimit",
                    5,
                    fmt.ArgumentLimit);
            assertEquals(
                    "toString",
                    "Templates {2}{1} and {4} are out of order.",
                    fmt.ToString());
            int[] offsets = new int[6];
            assertEquals(
                     "format",
                     "123456: Templates frogtommy and {0} are out of order.",
                     fmt.FormatAndAppend(
                             new StringBuilder("123456: "),
                             offsets,
                             "freddy", "tommy", "frog", "leg", "{0}").ToString());

            int[] expectedOffsets = { -1, 22, 18, -1, 32, -1 };
            verifyOffsets(expectedOffsets, offsets);
        }

        [Test]
        public void TestFormatUseAppendToAsArgument()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile(
                    "Arguments {0} and {1}");
            //StringBuilder appendTo = new StringBuilder("previous:");
            //try
            //{
            //    fmt.FormatAndAppend(appendTo, null, appendTo.ToString(), "frog");
            //    fail("IllegalArgumentException expected.");
            //}
            //catch (ArgumentException e)
            //{
            //    // expected.
            //}

            // ICU4N: We can only have a "same object" situation when referring to the same
            // memory location.
            // We don't have a SimpleFormatter.FormatAndAppend() method for this test because ValueStringBuilder
            // is internal and we don't use the SimpleFormatter instance internally, anyway. Instead,
            // we use SimpleFormatterImpl.
            Span<char> appendTo = stackalloc char[32];
            "previous:".AsSpan().CopyTo(appendTo);
            try
            {
                fmt.TryFormat(appendTo, out int charsLength, appendTo, "frog".AsSpan());
                fail("IllegalArgumentException expected.");
            }
            catch (ArgumentException e)
            {
                // expected.
            }
        }

        [Test]
        public void TestFormatReplaceNoOptimization()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("{2}, {0}, {1} and {3}");
            int[] offsets = new int[4];
            StringBuilder result = new StringBuilder("original");
            assertEquals(
                     "format",
                     "frog, original, freddy and by",
                     fmt.FormatAndReplace(
                             result,
                             offsets,
                             result.ToString(), "freddy", "frog", "by").ToString());

            int[] expectedOffsets = { 6, 16, 0, 27 };
            verifyOffsets(expectedOffsets, offsets);
        }


        [Test]
        public void TestFormatReplaceNoOptimizationLeadingText()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("boo {2}, {0}, {1} and {3}");
            int[] offsets = new int[4];
            StringBuilder result = new StringBuilder("original");
            assertEquals(
                     "format",
                     "boo original, freddy, frog and by",
                     fmt.FormatAndReplace(
                             result,
                             offsets,
                             "freddy", "frog", result.ToString(), "by").ToString());

            int[] expectedOffsets = { 14, 22, 4, 31 };
            verifyOffsets(expectedOffsets, offsets);
        }

        [Test]
        public void TestFormatReplaceOptimization()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("{2}, {0}, {1} and {3}");
            int[] offsets = new int[4];
            StringBuilder result = new StringBuilder("original");
            assertEquals(
                     "format",
                     "original, freddy, frog and by",
                     fmt.FormatAndReplace(
                             result,
                             offsets,
                             "freddy", "frog", result.ToString(), "by").ToString());

            int[] expectedOffsets = { 10, 18, 0, 27 };
            verifyOffsets(expectedOffsets, offsets);
        }

        [Test]
        public void TestFormatReplaceOptimizationNoOffsets()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile("{2}, {0}, {1} and {3}");
            StringBuilder result = new StringBuilder("original");
            assertEquals(
                     "format",
                     "original, freddy, frog and by",
                     fmt.FormatAndReplace(
                             result,
                             null,
                             "freddy", "frog", result.ToString(), "by").ToString());

        }

        [Test]
        public void TestFormatReplaceNoOptimizationNoOffsets()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile(
                    "Arguments {0} and {1}");
            StringBuilder result = new StringBuilder("previous:");
            assertEquals(
                    "",
                    "Arguments previous: and frog",
                    fmt.FormatAndReplace(result, null, result.ToString(), "frog").ToString());
        }

        [Test]
        public void TestFormatReplaceNoOptimizationLeadingArgumentUsedTwice()
        {
            SimpleFormatter fmt = SimpleFormatter.Compile(
                    "{2}, {0}, {1} and {3} {2}");
            StringBuilder result = new StringBuilder("original");
            int[] offsets = new int[4];
            assertEquals(
                    "",
                    "original, freddy, frog and by original",
                    fmt.FormatAndReplace(
                            result,
                            offsets,
                            "freddy", "frog", result.ToString(), "by").ToString());
            int[] expectedOffsets = { 10, 18, 30, 27 };
            verifyOffsets(expectedOffsets, offsets);
        }

        [Test]
        public void TestQuotingLikeMessageFormat()
        {
            string pattern = "{0} don't can''t '{5}''}{a' again '}'{1} to the '{end";
            SimpleFormatter spf = SimpleFormatter.Compile(pattern);
            MessageFormat mf = new MessageFormat(pattern, UCultureInfo.InvariantCulture);
            String expected = "X don't can't {5}'}{a again }Y to the {end";
            assertEquals("MessageFormat", expected, mf.Format(new Object[] { "X", "Y" }));
            assertEquals("SimpleFormatter", expected, spf.Format("X", "Y"));
        }

        private void verifyOffsets(int[] expected, int[] actual)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                if (expected[i] != actual[i])
                {
                    Errln("Expected " + expected[i] + ", got " + actual[i]);
                }
            }
        }
    }
}
