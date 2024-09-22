using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using NUnit.Framework;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <author>ram</author>
    public class TestIDNA : TestFmwk
    {
        private StringPrepFormatException unassignedException = new StringPrepFormatException("", StringPrepErrorType.UnassignedError);

        [Test]
        public void TestToUnicode()
        {
            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                // test StringBuffer toUnicode
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.Default, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.AllowUnassigned, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.UseSTD3Rules, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.UseSTD3Rules | IDNA2003Options.AllowUnassigned, null);

            }
        }

        [Test]
        public void TestToASCII()
        {
            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                // test StringBuffer toUnicode
                DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNA2003Options.Default, null);
                DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNA2003Options.AllowUnassigned, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.UseSTD3Rules, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA2003Options.UseSTD3Rules | IDNA2003Options.AllowUnassigned, null);

            }
        }

        [Test]
        public void TestIDNToASCII()
        {
            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.Default, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.AllowUnassigned, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.UseSTD3Rules, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.AllowUnassigned | IDNA2003Options.UseSTD3Rules, null);
            }

            for (int i = 0; i < TestData.domainNames1Uni.Length; i++)
            {
                DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNA2003Options.Default, null);
                DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNA2003Options.AllowUnassigned, null);
            }
        }
        [Test]
        public void TestIDNToUnicode()
        {
            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.Default, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.AllowUnassigned, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.UseSTD3Rules, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA2003Options.AllowUnassigned | IDNA2003Options.UseSTD3Rules, null);
            }
            for (int i = 0; i < TestData.domainNamesToASCIIOut.Length; i++)
            {
                DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNA2003Options.Default, null);
                DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNA2003Options.AllowUnassigned, null);
            }
        }

        private void DoTestToUnicode(String src, String expected, IDNA2003Options options, Object expectedException)
        {
            // ICU4N: Factored out UCharacterIterator and StringBuffer overloads. In .NET, it is better
            // to return string than StringBuilder, since there is no way to get the string
            // out of a StringBuilder without an allocation. Instead, we added an overload
            // that outputs to a Span<char>.
            try
            {
                string @out = IDNA.ConvertToUnicode(src, options);
                if (expected != null && @out != null && !@out.Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepFormatException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception for source: " + Prettify(src) + " Got:  " + ex.ToString());
                }
            }

            {
                Span<char> outBuf = stackalloc char[src.Length + 32];
                int outBufLength = 0;
                StringPrepErrorType errorType = (StringPrepErrorType)(-1);
                bool success = IDNA.TryConvertToUnicode(src.AsSpan(), outBuf, out outBufLength, options, out errorType);
                if (success)
                {
                    string @out = outBuf.Slice(0, outBufLength).ToString();
                    if (expected != null && @out != null && !@out.ToString().Equals(expected))
                    {
                        Errln("TryConvertToUnicode did not return expected result with options : " + options +
                              " Expected: " + Prettify(expected) + " Got: " + @out);
                    }
                    if (expectedException != null && !unassignedException.Equals(expectedException))
                    {
                        Errln("TryConvertToUnicode did not get the expected exception. The operation succeeded!");
                    }
                }
                else
                {
                    if (expectedException == null || ((StringPrepFormatException)expectedException).Error != errorType)
                    {
                        Errln("TryConvertToUnicode did not get the expected exception for source: " + src + " Got:  " + errorType.ToString());
                    }
                }
            }
        }

        private void DoTestIDNToUnicode(String src, String expected, IDNA2003Options options, Object expectedException)
        {
            // ICU4N: Factored out UCharacterIterator and StringBuffer overloads. In .NET, it is better
            // to return string than StringBuilder, since there is no way to get the string
            // out of a StringBuilder without an allocation. Instead, we added an overload
            // that outputs to a Span<char>.

            try
            {
                string @out = IDNA.ConvertIDNToUnicode(src, options);
                if (expected != null && @out != null && !@out.Equals(expected))
                {
                    Errln("ConvertIDNToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("ConvertIDNToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepFormatException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("ConvertIDNToUnicode did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }

            {
                Span<char> outBuf = stackalloc char[src.Length + 32];
                int outBufLength = 0;
                StringPrepErrorType errorType = (StringPrepErrorType)(-1);
                bool success = IDNA.TryConvertIDNToUnicode(src.AsSpan(), outBuf, out outBufLength, options, out errorType);
                if (success)
                {
                    string @out = outBuf.Slice(0, outBufLength).ToString();
                    if (expected != null && @out != null && !@out.Equals(expected))
                    {
                        Errln("TryConvertIDNToUnicode did not return expected result with options : " + options +
                              " Expected: " + Prettify(expected) + " Got: " + @out);
                    }
                    if (expectedException != null && !unassignedException.Equals(expectedException))
                    {
                        Errln("TryConvertIDNToUnicode did not get the expected exception. The operation succeeded!");
                    }
                }
                else
                {
                    if (expectedException == null || ((StringPrepFormatException)expectedException).Error != errorType)
                    {
                        Errln("TryConvertIDNToUnicode did not get the expected exception for source: " + src + " Got:  " + errorType.ToString());
                    }
                }
            }
        }

        // NOTE: Passing null for expectedException is for a "real" test. The postive case is skipped during
        // the TestErrorCases test. Also, that test is passing in full domain names instead of labels,
        // so we don't expect the expected variable to match the result in those cases.
        private void DoTestToASCII(String src, String expected, IDNA2003Options options, Object expectedException)
        {
            // ICU4N: Factored out UCharacterIterator and StringBuffer overloads. In .NET, it is better
            // to return string than StringBuilder, since there is no way to get the string
            // out of a StringBuilder without an allocation. Instead, we added an overload
            // that outputs to a Span<char>.
            try
            {
                string @out = IDNA.ConvertToASCII(src, options);
                if (!unassignedException.Equals(expectedException) && expected != null && @out != null && !@out.Equals(expected.ToLowerInvariant()))
                {
                    Errln("ConvertToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("ConvertToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepFormatException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("ConvertToASCII did not get the expected exception for source: " + src + "\n Got:  " + ex.ToString() + "\n Expected: " + ex.ToString());
                }
            }
            {
                Span<char> outBuf = stackalloc char[src.Length + 32];
                StringPrepErrorType errorType = (StringPrepErrorType)(-1);
                bool success = IDNA.TryConvertToASCII(src.AsSpan(), outBuf, out int outBufLength, options, out errorType);
                if (success)
                {
                    string @out = outBuf.Slice(0, outBufLength).ToString();
                    if (!unassignedException.Equals(expectedException) && expected != null && @out != null && !@out.Equals(expected.ToLowerInvariant()))
                    {
                        Errln("TryConvertToASCII did not return expected result with options : " + options +
                              " Expected: " + Prettify(expected) + " Got: " + @out);
                    }
                    if (expectedException != null && !unassignedException.Equals(expectedException))
                    {
                        Errln("TryConvertToASCII did not get the expected exception. The operation succeeded!");
                    }
                }
                else
                {
                    if (expectedException == null || ((StringPrepFormatException)expectedException).Error != errorType)
                    {
                        Errln("TryConvertToASCII did not get the expected exception for source: " + src + " Got:  " + errorType.ToString());
                    }
                }
            }
        }
        private void DoTestIDNToASCII(String src, String expected, IDNA2003Options options, Object expectedException)
        {
            // ICU4N: Factored out UCharacterIterator and StringBuffer overloads. In .NET, it is better
            // to return string than StringBuilder, since there is no way to get the string
            // out of a StringBuilder without an allocation. Instead, we added an overload
            // that outputs to a Span<char>.
            try
            {
                string @out = IDNA.ConvertIDNToASCII(src, options);
                if (expected != null && @out != null && !@out.Equals(expected))
                {
                    Errln("ConvertIDNToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("ConvertIDNToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepFormatException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("ConvertIDNToASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
            {
                Span<char> outBuf = stackalloc char[src.Length + 32];
                int outBufLength = 0;
                StringPrepErrorType errorType = (StringPrepErrorType)(-1);
                bool success = IDNA.TryConvertIDNToASCII(src.AsSpan(), outBuf, out outBufLength, options, out errorType);

                if (success)
                {
                    string @out = outBuf.Slice(0, outBufLength).ToString();
                    if (expected != null && @out != null && !@out.ToString().Equals(expected))
                    {
                        Errln("TryConvertIDNToASCII did not return expected result with options : " + options +
                              " Expected: " + Prettify(expected) + " Got: " + @out);
                    }
                    if (expectedException != null && !unassignedException.Equals(expectedException))
                    {
                        Errln("TryConvertIDNToASCII did not get the expected exception. The operation succeeded!");
                    }
                }
                else
                {
                    if (expectedException == null || ((StringPrepFormatException)expectedException).Error != errorType)
                    {
                        Errln("TryConvertIDNToASCII did not get the expected exception for source: " + src + " Got:  " + errorType.ToString());
                    }
                }
            }
        }
        [Test]
        public void TestConformance()
        {
            for (int i = 0; i < TestData.conformanceTestCases.Length; i++)
            {

                TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                if (testCase.expected != null)
                {
                    //Test toASCII
                    DoTestToASCII(testCase.input, testCase.output, IDNA2003Options.Default, testCase.expected);
                    DoTestToASCII(testCase.input, testCase.output, IDNA2003Options.AllowUnassigned, testCase.expected);
                }
                //Test toUnicode
                //doTestToUnicode(testCase.input,testCase.output,IDNAOptions.Default,testCase.expected);
            }
        }
        [Test]
        public void TestNamePrepConformance()
        {
            Text.StringPrep namePrep = Text.StringPrep.GetInstance(StringPrepProfile.Rfc3491NamePrep);
            Span<char> outputBuf = stackalloc char[256];
            for (int i = 0; i < TestData.conformanceTestCases.Length; i++)
            {
                TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                try
                {
                    string output = namePrep.Prepare(testCase.input, StringPrepOptions.Default);
                    if (testCase.output != null && output != null && !testCase.output.Equals(output))
                    {
                        Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                              " Got: " + Prettify(output));
                    }
                    if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception. The operation succeeded!");
                    }
                }
                catch (StringPrepFormatException ex)
                {
                    if (testCase.expected == null || !ex.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + ex.ToString());
                    }
                }

                try
                {
                    string output = namePrep.Prepare(testCase.input, StringPrepOptions.AllowUnassigned);
                    if (testCase.output != null && output != null && !testCase.output.Equals(output))
                    {
                        Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                              " Got: " + Prettify(output));
                    }
                    if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception. The operation succeeded!");
                    }
                }
                catch (StringPrepFormatException ex)
                {
                    if (testCase.expected == null || !ex.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + ex.ToString());
                    }
                }

                {
                    bool success = namePrep.TryPrepare(testCase.input.AsSpan(), outputBuf, out int charsLength, StringPrepOptions.Default, out StringPrepErrorType errorType);
                    if (success)
                    {
                        string output = outputBuf.Slice(0, charsLength).ToString();
                        if (testCase.output != null && output != null && !testCase.output.Equals(output))
                        {
                            Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                                  " Got: " + Prettify(output));
                        }
                        if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                        {
                            Errln("Did not get the expected exception. The operation succeeded!");
                        }
                    }
                    else
                    {
                        if (testCase.expected == null || errorType != ((StringPrepFormatException)testCase.expected).Error)
                        {
                            Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + errorType.ToString());
                        }
                    }
                }

                {
                    bool success = namePrep.TryPrepare(testCase.input.AsSpan(), outputBuf, out int charsLength, StringPrepOptions.AllowUnassigned, out StringPrepErrorType errorType);
                    if (success)
                    {
                        string output = outputBuf.Slice(0, charsLength).ToString();
                        if (testCase.output != null && output != null && !testCase.output.Equals(output))
                        {
                            Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                                  " Got: " + Prettify(output));
                        }
                        if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                        {
                            Errln("Did not get the expected exception. The operation succeeded!");
                        }
                    }
                    else
                    {
                        if (testCase.expected == null || errorType != ((StringPrepFormatException)testCase.expected).Error)
                        {
                            Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + errorType.ToString());
                        }
                    }
                }
            }

        }

        [Test]
        public void TestErrorCases()
        {
            for (int i = 0; i < TestData.errorCases.Length; i++)
            {
                TestData.ErrorCase errCase = TestData.errorCases[i];
                if (errCase.testLabel == true)
                {
                    // Test ToASCII
                    DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.Default, errCase.expected);
                    DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.AllowUnassigned, errCase.expected);
                    if (errCase.useSTD3ASCIIRules)
                    {
                        DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.UseSTD3Rules, errCase.expected);
                    }
                }
                if (errCase.useSTD3ASCIIRules != true)
                {

                    // Test IDNToASCII
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.Default, errCase.expected);
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.AllowUnassigned, errCase.expected);
                }
                else
                {
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA2003Options.UseSTD3Rules, errCase.expected);
                }
                //TestToUnicode
                if (errCase.testToUnicode == true)
                {
                    if (errCase.useSTD3ASCIIRules != true)
                    {
                        // Test IDNToUnicode
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA2003Options.Default, errCase.expected);
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA2003Options.AllowUnassigned, errCase.expected);

                    }
                    else
                    {
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA2003Options.UseSTD3Rules, errCase.expected);
                    }
                }
            }
        }
        private void DoTestCompare(String s1, String s2, bool isEqual)
        {
            try
            {
                int retVal = IDNA.Compare(s1, s2, IDNA2003Options.Default);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(s1.AsSpan(), s2.AsSpan(), IDNA2003Options.Default);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }

                // ICU4N: Factored out StringBuffer and UCharacterIterator overloads

                //retVal = IDNA.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA2003Options.Default);
                //if (isEqual == true && retVal != 0)
                //{
                //    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                //         " s2: " + Prettify(s2));
                //}
                //retVal = IDNA.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA2003Options.Default);
                //if (isEqual == true && retVal != 0)
                //{
                //    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                //         " s2: " + Prettify(s2));
                //}
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
                Errln("Unexpected exception thrown by IDNA.compare");
            }

            try
            {
                int retVal = IDNA.Compare(s1, s2, IDNA2003Options.AllowUnassigned);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(s1.AsSpan(), s2.AsSpan(), IDNA2003Options.AllowUnassigned);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                // ICU4N: Factored out StringBuffer and UCharacterIterator overloads

                //retVal = IDNA.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA2003Options.AllowUnassigned);
                //if (isEqual == true && retVal != 0)
                //{
                //    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                //         " s2: " + Prettify(s2));
                //}
                //retVal = IDNA.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA2003Options.AllowUnassigned);
                //if (isEqual == true && retVal != 0)
                //{
                //    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                //         " s2: " + Prettify(s2));
                //}
            }
            catch (Exception e)
            {
                Errln("Unexpected exception thrown by IDNA.compare");
            }
        }
        [Test]
        public void TestCompare()
        {
            String www = "www.";
            String com = ".com";
            StringBuffer source = new StringBuffer(www);
            StringBuffer uni0 = new StringBuffer(www);
            StringBuffer uni1 = new StringBuffer(www);
            StringBuffer ascii0 = new StringBuffer(www);
            StringBuffer ascii1 = new StringBuffer(www);

            uni0.Append(TestData.unicodeIn[0]);
            uni0.Append(com);

            uni1.Append(TestData.unicodeIn[1]);
            uni1.Append(com);

            ascii0.Append(TestData.asciiIn[0]);
            ascii0.Append(com);

            ascii1.Append(TestData.asciiIn[1]);
            ascii1.Append(com);

            for (int i = 0; i < TestData.unicodeIn.Length; i++)
            {

                // for every entry in unicodeIn array
                // prepend www. and append .com
                source.Length = (4);
                source.Append(TestData.unicodeIn[i]);
                source.Append(com);

                // a) compare it with itself
                DoTestCompare(source.ToString(), source.ToString(), true);

                // b) compare it with asciiIn equivalent
                DoTestCompare(source.ToString(), www + TestData.asciiIn[i] + com, true);

                // c) compare it with unicodeIn not equivalent
                if (i == 0)
                {
                    DoTestCompare(source.ToString(), uni1.ToString(), false);
                }
                else
                {
                    DoTestCompare(source.ToString(), uni0.ToString(), false);
                }
                // d) compare it with asciiIn not equivalent
                if (i == 0)
                {
                    DoTestCompare(source.ToString(), ascii1.ToString(), false);
                }
                else
                {
                    DoTestCompare(source.ToString(), ascii0.ToString(), false);
                }

            }
        }

        //  test and ascertain
        //  func(func(func(src))) == func(src)
        private void DoTestChainingToASCII(String source)
        {
            string expected;
            string chained;

            // test convertIDNToASCII
            expected = IDNA.ConvertIDNToASCII(source, IDNA2003Options.Default);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertIDNToASCII(chained, IDNA2003Options.Default);
            }
            if (!expected.Equals(chained))
            {
                Errln("Chaining test failed for convertIDNToASCII");
            }
            // test convertIDNToA
            expected = IDNA.ConvertToASCII(source, IDNA2003Options.Default);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertToASCII(chained, IDNA2003Options.Default);
            }
            if (!expected.Equals(chained))
            {
                Errln("Chaining test failed for convertToASCII");
            }
        }

        //  test and ascertain
        //  func(func(func(src))) == func(src)
        private void DoTestChainingToUnicode(String source)
        {
            string expected;
            string chained;

            // test convertIDNToUnicode
            expected = IDNA.ConvertIDNToUnicode(source, IDNA2003Options.Default);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertIDNToUnicode(chained, IDNA2003Options.Default);
            }
            if (!expected.Equals(chained))
            {
                Errln("Chaining test failed for convertIDNToUnicode");
            }
            // test convertIDNToA
            expected = IDNA.ConvertToUnicode(source, IDNA2003Options.Default);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertToUnicode(chained, IDNA2003Options.Default);
            }
            if (!expected.Equals(chained))
            {
                Errln("Chaining test failed for convertToUnicode");
            }
        }

        [Test]
        public void TestChaining()
        {
            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                DoTestChainingToUnicode(TestData.asciiIn[i]);
            }
            for (int i = 0; i < TestData.unicodeIn.Length; i++)
            {
                DoTestChainingToASCII(new String(TestData.unicodeIn[i]));
            }
        }


        /* IDNA RFC Says:
        A label is an individual part of a domain name.  Labels are usually
        shown separated by dots; for example, the domain name
        "www.example.com" is composed of three labels: "www", "example", and
        "com".  (The zero-length root label described in [STD13], which can
        be explicit as in "www.example.com." or implicit as in
        "www.example.com", is not considered a label in this specification.)
        */
        [Test]
        public void TestRootLabelSeparator()
        {
            String www = "www.";
            String com = ".com."; //root label separator
            StringBuffer source = new StringBuffer(www);
            StringBuffer uni0 = new StringBuffer(www);
            StringBuffer uni1 = new StringBuffer(www);
            StringBuffer ascii0 = new StringBuffer(www);
            StringBuffer ascii1 = new StringBuffer(www);

            uni0.Append(TestData.unicodeIn[0]);
            uni0.Append(com);

            uni1.Append(TestData.unicodeIn[1]);
            uni1.Append(com);

            ascii0.Append(TestData.asciiIn[0]);
            ascii0.Append(com);

            ascii1.Append(TestData.asciiIn[1]);
            ascii1.Append(com);

            for (int i = 0; i < TestData.unicodeIn.Length; i++)
            {

                // for every entry in unicodeIn array
                // prepend www. and append .com
                source.Length = (4);
                source.Append(TestData.unicodeIn[i]);
                source.Append(com);

                // a) compare it with itself
                DoTestCompare(source.ToString(), source.ToString(), true);

                // b) compare it with asciiIn equivalent
                DoTestCompare(source.ToString(), www + TestData.asciiIn[i] + com, true);

                // c) compare it with unicodeIn not equivalent
                if (i == 0)
                {
                    DoTestCompare(source.ToString(), uni1.ToString(), false);
                }
                else
                {
                    DoTestCompare(source.ToString(), uni0.ToString(), false);
                }
                // d) compare it with asciiIn not equivalent
                if (i == 0)
                {
                    DoTestCompare(source.ToString(), ascii1.ToString(), false);
                }
                else
                {
                    DoTestCompare(source.ToString(), ascii0.ToString(), false);
                }

            }

        }


        private const int loopCount = 100;
        private const int maxCharCount = 15;
        // private const int maxCodePoint = 0x10ffff;
        private Random random = null;

        /**
         * Return a random integer i where 0 &lt;= i &lt; n.
         * A special function that gets random codepoints from planes 0,1,2 and 14
         */
        private int RandUni()
        {
            int retVal = (int)(random.Next() & 0x3FFFF);
            if (retVal >= 0x30000)
            {
                retVal += 0xB0000;
            }
            return retVal;
        }

        private int Randi(int n)
        {
            return (random.Next(0x7fff) % (n + 1));
        }

        private StringBuffer GetTestSource(StringBuffer fillIn)
        {
            // use uniform seed value from the framework
            if (random == null)
            {
                random = CreateRandom();
            }
            int i = 0;
            int charCount = (Randi(maxCharCount) + 1);
            while (i < charCount)
            {
                int codepoint = RandUni();
                if (codepoint == 0x0000)
                {
                    continue;
                }
                UTF16.Append(fillIn, codepoint);
                i++;
            }
            return fillIn;

        }

        // TODO(#13294): turned off because monkey test fails approx 1 in 3 times.
        [Ignore("TODO(#13294): turned off because monkey test fails approx 1 in 3 times.")]
        [Test]
        public void MonkeyTest()
        {
            StringBuffer source = new StringBuffer();
            /* do the monkey test   */
            for (int i = 0; i < loopCount; i++)
            {
                source.Length = (0);
                GetTestSource(source);
                DoTestCompareReferenceImpl(source);
            }

            // test string with embedded null
            source.Append("\\u0000\\u2109\\u3E1B\\U000E65CA\\U0001CAC5");

            source = new StringBuffer(Utility.Unescape(source.ToString()));
            DoTestCompareReferenceImpl(source);

            //StringBuffer src = new StringBuffer(Utility.Unescape("\\uDEE8\\U000E228C\\U0002EE8E\\U000E6350\\U00024DD9\u4049\\U000E0DE4\\U000E448C\\U0001869B\\U000E3380\\U00016A8E\\U000172D5\\U0001C408\\U000E9FB5"));
            //doTestCompareReferenceImpl(src);

            //test deletion of code points
            source = new StringBuffer(Utility.Unescape("\\u043f\\u00AD\\u034f\\u043e\\u0447\\u0435\\u043c\\u0443\\u0436\\u0435\\u043e\\u043d\\u0438\\u043d\\u0435\\u0433\\u043e\\u0432\\u043e\\u0440\\u044f\\u0442\\u043f\\u043e\\u0440\\u0443\\u0441\\u0441\\u043a\\u0438"));
            StringBuffer expected = new StringBuffer("xn--b1abfaaepdrnnbgefbadotcwatmq2g4l");
            DoTestCompareReferenceImpl(source);
            DoTestToASCII(source.ToString(), expected.ToString(), IDNA2003Options.Default, null);
        }

        private StringBuffer _doTestCompareReferenceImpl(StringBuffer src, bool toASCII, IDNA2003Options options)
        {
            String refIDNAName = toASCII ? "IDNAReference.convertToASCII" : "IDNAReference.convertToUnicode";
            String uIDNAName = toASCII ? "IDNA.convertToASCII" : "IDNA.convertToUnicode";

            Logln("Comparing " + refIDNAName + " with " + uIDNAName + " for input: "
                    + Prettify(src) + " with options: " + options);

            StringBuffer exp = null;
            int expStatus = -1;
            try
            {
                exp = toASCII ? IDNAReference.ConvertToASCII(src, options) : IDNAReference.ConvertToUnicode(src, options);
            }
            catch (StringPrepFormatException e)
            {
                expStatus = (int)e.Error;
            }

            string got = null;
            int gotStatus = -1;
            try
            {
                // ICU4N: Factored out StringBuffer overloads. In .NET, it is better
                // to return string than StringBuilder, since there is no way to get the string
                // out of a StringBuilder without an allocation. Instead, we added an overload
                // that outputs to a Span<char>.
                got = toASCII ? IDNA.ConvertToASCII(src.ToString(), options) : IDNA.ConvertToUnicode(src.ToString(), options);
            }
            catch (StringPrepFormatException e)
            {
                gotStatus = (int)e.Error;
            }

            if (expStatus != gotStatus)
            {
                Errln("Did not get the expected status while comparing " + refIDNAName + " with " + uIDNAName
                        + " Expected: " + expStatus
                        + " Got: " + gotStatus
                        + " for Source: " + Prettify(src)
                        + " Options: " + options);
            }
            else
            {
                // now we know that both implementation yielded same status
                if (gotStatus == -1)
                {
                    // compare the outputs
                    if (!got.ToString().Equals(exp.ToString()))
                    {
                        Errln("Did not get the expected output while comparing " + refIDNAName + " with " + uIDNAName
                                + " Expected: " + exp
                                + " Got: " + got
                                + " for Source: " + Prettify(src)
                                + " Options: " + options);
                    }
                }
                else
                {
                    Logln("Got the same error while comparing " + refIDNAName + " with " + uIDNAName
                            + " for input: " + Prettify(src) + " with options: " + options);
                }
            }

            return exp;
        }

        private void DoTestCompareReferenceImpl(StringBuffer src)
        {
            // test toASCII
            StringBuffer asciiLabel = _doTestCompareReferenceImpl(src, true, IDNA2003Options.AllowUnassigned);
            _doTestCompareReferenceImpl(src, true, IDNA2003Options.Default);
            _doTestCompareReferenceImpl(src, true, IDNA2003Options.UseSTD3Rules);
            _doTestCompareReferenceImpl(src, true, IDNA2003Options.UseSTD3Rules | IDNA2003Options.AllowUnassigned);

            if (asciiLabel != null)
            {
                // test toUnicode
                _doTestCompareReferenceImpl(src, false, IDNA2003Options.AllowUnassigned);
                _doTestCompareReferenceImpl(src, false, IDNA2003Options.Default);
                _doTestCompareReferenceImpl(src, false, IDNA2003Options.UseSTD3Rules);
                _doTestCompareReferenceImpl(src, false, IDNA2003Options.UseSTD3Rules | IDNA2003Options.AllowUnassigned);
            }
        }

        [Test]
        public void TestJB4490()
        {
            String[] @in = new String[]{
                "\u00F5\u00dE\u00dF\u00dD",
                "\uFB00\uFB01"
               };
            for (int i = 0; i < @in.Length; i++)
            {
                try
                {
                    String ascii = IDNA.ConvertToASCII(@in[i], IDNA2003Options.Default).ToString();
                    try
                    {
                        String unicode = IDNA.ConvertToUnicode(ascii, IDNA2003Options.Default).ToString();
                        Logln("result " + unicode);
                    }
                    catch (StringPrepFormatException ex)
                    {
                        Errln("Unexpected exception for convertToUnicode: " + ex.ToString());
                    }
                }
                catch (StringPrepFormatException ex)
                {
                    Errln("Unexpected exception for convertToASCII: " + ex.ToString());
                }
            }
        }
        [Test]
        public void TestJB4475()
        {
            String[] @in = new String[]{
                        "TEST",
                        "test"
                       };
            for (int i = 0; i < @in.Length; i++)
            {

                try
                {
                    String ascii = IDNA.ConvertToASCII(@in[i], IDNA2003Options.Default).ToString();
                    if (!ascii.Equals(@in[i]))
                    {
                        Errln("Did not get the expected string for convertToASCII. Expected: " + @in[i] + " Got: " + ascii);
                    }
                }
                catch (StringPrepFormatException ex)
                {
                    Errln("Unexpected exception: " + ex.ToString());
                }
            }

        }

        [Test]
        public void TestDebug()
        {
            try
            {
                String src = "\u00ED4dn";
                String uni = IDNA.ConvertToUnicode(src, IDNA2003Options.Default).ToString();
                if (!uni.Equals(src))
                {
                    Errln("Did not get the expected result. Expected: " + Prettify(src) + " Got: " + uni);
                }
            }
            catch (StringPrepFormatException ex)
            {
                Logln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                String ascii = IDNA.ConvertToASCII("\u00AD", IDNA2003Options.Default).ToString();
                if (ascii != null)
                {
                    Errln("Did not get the expected exception");
                }
            }
            catch (StringPrepFormatException ex)
            {
                Logln("Got the expected exception: " + ex.ToString());
            }
        }
        [Test]
        public void TestJB5273()
        {
            String INVALID_DOMAIN_NAME = "xn--m\u00FCller.de";
            try
            {
                IDNA.ConvertIDNToUnicode(INVALID_DOMAIN_NAME, IDNA2003Options.Default);
                IDNA.ConvertIDNToUnicode(INVALID_DOMAIN_NAME, IDNA2003Options.UseSTD3Rules);

            }
            catch (StringPrepFormatException ex)
            {
                Errln("Unexpected exception: " + ex.ToString());
            }
            catch (IndexOutOfRangeException ex)
            {
                Errln("Got an ArrayIndexOutOfBoundsException calling convertIDNToUnicode(\"" + INVALID_DOMAIN_NAME + "\")");
            }

            String domain = "xn--m\u00FCller.de";
            try
            {
                IDNA.ConvertIDNToUnicode(domain, IDNA2003Options.Default);
            }
            catch (StringPrepFormatException ex)
            {
                Logln("Got the expected exception. " + ex.ToString());
            }
            catch (Exception ex)
            {
                Errln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertIDNToUnicode(domain, IDNA2003Options.UseSTD3Rules);
            }
            catch (StringPrepFormatException ex)
            {
                Logln("Got the expected exception. " + ex.ToString());
            }
            catch (Exception ex)
            {
                Errln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode("xn--m\u00FCller", IDNA2003Options.Default);
            }
            catch (Exception ex)
            {
                Errln("ToUnicode operation failed! " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode("xn--m\u00FCller", IDNA2003Options.UseSTD3Rules);
            }
            catch (Exception ex)
            {
                Errln("ToUnicode operation failed! " + ex.ToString());
            }
            try
            {
                IDNA.ConvertIDNToUnicode("xn--m\u1234ller", IDNA2003Options.UseSTD3Rules);
            }
            catch (StringPrepFormatException ex)
            {
                Errln("ToUnicode operation failed! " + ex.ToString());
            }
        }

        [Test]
        public void TestLength()
        {
            String ul = "my_very_very_very_very_very_very_very_very_very_very_very_very_very_long_and_incredibly_uncreative_domain_label";

            /* this unicode string is longer than MAX_LABEL_BUFFER_SIZE and produces an
               IDNA prepared string (including xn--)that is exactly 63 bytes long */
            String ul1 = "\uC138\uACC4\uC758\uBAA8\uB4E0\uC0AC\uB78C\uB4E4\uC774" +
                        "\uD55C\uAD6D\uC5B4\uB97C\uC774\u00AD\u034F\u1806\u180B" +
                        "\u180C\u180D\u200B\u200C\u200D\u2060\uFE00\uFE01\uFE02" +
                        "\uFE03\uFE04\uFE05\uFE06\uFE07\uFE08\uFE09\uFE0A\uFE0B" +
                        "\uFE0C\uFE0D\uFE0E\uFE0F\uFEFF\uD574\uD55C\uB2E4\uBA74" +
                        "\uC138\u0041\u00AD\u034F\u1806\u180B\u180C\u180D\u200B" +
                        "\u200C\u200D\u2060\uFE00\uFE01\uFE02\uFE03\uFE04\uFE05" +
                        "\uFE06\uFE07\uFE08\uFE09\uFE0A\uFE0B\uFE0C\uFE0D\uFE0E" +
                        "\uFE0F\uFEFF\u00AD\u034F\u1806\u180B\u180C\u180D\u200B" +
                        "\u200C\u200D\u2060\uFE00\uFE01\uFE02\uFE03\uFE04\uFE05" +
                        "\uFE06\uFE07\uFE08\uFE09\uFE0A\uFE0B\uFE0C\uFE0D\uFE0E" +
                        "\uFE0F\uFEFF\u00AD\u034F\u1806\u180B\u180C\u180D\u200B" +
                        "\u200C\u200D\u2060\uFE00\uFE01\uFE02\uFE03\uFE04\uFE05" +
                        "\uFE06\uFE07\uFE08\uFE09\uFE0A\uFE0B\uFE0C\uFE0D\uFE0E" +
                        "\uFE0F\uFEFF";
            try
            {
                IDNA.ConvertToASCII(ul, IDNA2003Options.Default);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepFormatException ex)
            {
                if (ex.Error != StringPrepErrorType.LabelTooLongError)
                {
                    Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToASCII(ul, IDNAOptions.Default) Succeeded");
                }
            }
            try
            {
                IDNA.ConvertToASCII(ul1, IDNA2003Options.Default);
            }
            catch (StringPrepFormatException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode(ul1, IDNA2003Options.Default);
            }
            catch (StringPrepFormatException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode(ul, IDNA2003Options.Default);
            }
            catch (StringPrepFormatException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }

            String idn = "my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.ibm.com";
            try
            {
                IDNA.ConvertIDNToASCII(idn, IDNA2003Options.Default);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepFormatException ex)
            {
                if (ex.Error != StringPrepErrorType.DomainNameTooLongError)
                {
                    Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToASCII(idn, IDNAOptions.Default) Succeeded");
                }
            }
            try
            {
                IDNA.ConvertIDNToUnicode(idn, IDNA2003Options.Default);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepFormatException ex)
            {
                if (ex.Error != StringPrepErrorType.DomainNameTooLongError)
                {
                    Errln("IDNA.convertToUnicode failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToUnicode(idn, IDNAOptions.Default) Succeeded");
                }
            }
        }

        /* Tests the method public static StringBuffer convertToASCII(String src, int options) */
        [Test]
        public void TestConvertToASCII()
        {
            try
            {
                if (!IDNA.ConvertToASCII("dummy", 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.ConvertToASCII(String,int) was suppose to return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.ConvertToASCII(String,int) was not suppose to return an exception.");
            }
            Span<char> result = stackalloc char[10];
            bool success = IDNA.TryConvertToASCII("dummy".AsSpan(), result, out int resultLength, 0, out _);
            if (success)
            {
                if (!result.Slice(0, resultLength).ToString().Equals("dummy"))
                {
                    Errln("IDNA.TryConvertToASCII(ReadOnlySpan<char>, Span<char>, out int, IDNA2003Options, out StringPrepErrorType) was suppose to " + "return the same string passed.");
                }
            }
            else
            {
                Errln("IDNA.TryConvertToASCII was not suppose to return an error.");
            }
        }

        /*
         * Tests the method public static StringBuffer convertIDNToASCII(UCharacterIterator src, int options), method public
         * static StringBuffer public static StringBuffer convertIDNToASCII(StringBuffer src, int options), public static
         * StringBuffer convertIDNToASCII(UCharacterIterator src, int options)
         */
        [Test]
        public void TestConvertIDNToASCII()
        {
            try
            {
                //UCharacterIterator uci = UCharacterIterator.GetInstance("dummy");
                //if (!IDNA.ConvertIDNToASCII(uci, 0).ToString().Equals("dummy"))
                //{
                //    Errln("IDNA.convertIDNToASCII(UCharacterIterator, int) was suppose to "
                //            + "return the same string passed.");
                //}
                if (!IDNA.ConvertIDNToASCII("dummy", 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToASCII(string, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
                if (!IDNA.ConvertIDNToASCII("dummy".AsSpan(), 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToASCII(string, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.ConvertIDNToASCII was not suppose to return an exception.");
            }
            Span<char> result = stackalloc char[10];
            bool success = IDNA.TryConvertIDNToASCII("dummy".AsSpan(), result, out int resultLength, 0, out _);
            if (success)
            {
                if (!result.Slice(0, resultLength).ToString().Equals("dummy"))
                {
                    Errln("IDNA.TryConvertIDNToASCII(ReadOnlySpan<char>, Span<char>, out int, IDNA2003Options, out StringPrepErrorType) was suppose to " + "return the same string passed.");
                }
            }
            else
            {
                Errln("IDNA.TryConvertIDNToASCII was not suppose to return an error.");
            }
        }

        /*
         * Tests the method public static StringBuffer convertToUnicode(String src, int options), public static StringBuffer
         * convertToUnicode(StringBuffer src, int options)
         */
        [Test]
        public void TestConvertToUnicode()
        {
            try
            {
                if (!IDNA.ConvertToUnicode("dummy", 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertToUnicode(String, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
                //if (!IDNA.ConvertToUnicode(new StringBuffer("dummy"), 0).ToString().Equals("dummy"))
                //{
                //    Errln("IDNA.ConvertToUnicode(StringBuffer, int) was suppose to " + "return the same string passed.");
                //}
                if (!IDNA.ConvertToUnicode("dummy".AsSpan(), 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertToUnicode(ReadOnlySpan<chr>, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.ConvertToUnicode was not suppose to return an exception.");
            }
            Span<char> result = stackalloc char[10];
            bool success = IDNA.TryConvertToUnicode("dummy".AsSpan(), result, out int resultLength, 0, out _);
            if (success)
            {
                if (!result.Slice(0, resultLength).ToString().Equals("dummy"))
                {
                    Errln("IDNA.TryConvertToUnicode(ReadOnlySpan<char>, Span<char>, out int, IDNA2003Options, out StringPrepErrorType) was suppose to " + "return the same string passed.");
                }
            }
            else
            {
                Errln("IDNA.TryConvertToUnicode was not suppose to return an error.");
            }
        }

        /* Tests the method public static StringBuffer convertIDNToUnicode(UCharacterIterator src, int options) */
        [Test]
        public void TestConvertIDNToUnicode()
        {
            try
            {
                // ICU4N: Factored out UCharacterIterator and StringBuffer overloads. In .NET, it is better
                // to return string than StringBuilder, since there is no way to get the string
                // out of a StringBuilder without an allocation. Instead, we added an overload
                // that outputs to a Span<char>.
                if (!IDNA.ConvertIDNToUnicode("dummy", 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToUnicode(string, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
                if (!IDNA.ConvertIDNToUnicode("dummy".AsSpan(), 0).Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToUnicode(ReadOnlySpan<char>, IDNA2003Options) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.convertIDNToUnicode was not suppose to return an exception.");
            }
            Span<char> result = stackalloc char[10];
            bool success = IDNA.TryConvertIDNToUnicode("dummy".AsSpan(), result, out int resultLength, 0, out _);
            if (success)
            {
                if (!result.Slice(0, resultLength).ToString().Equals("dummy"))
                {
                    Errln("IDNA.TryConvertIDNToUnicode(ReadOnlySpan<char>, Span<char>, out int, IDNA2003Options, out StringPrepErrorType) was suppose to " + "return the same string passed.");
                }
            }
            else
            {
                Errln("IDNA.TryConvertIDNToUnicode was not suppose to return an error.");
            }
        }

        /* Tests the method public static int compare */
        [Test]
        public void TestIDNACompare()
        {
            // Testing the method public static int compare(String s1, String s2, int options)
            try
            {
                IDNA.Compare((String)null, (String)null, 0);
                Errln("IDNA.Compare((String)null,(String)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare((String)null, "dummy", 0);
                Errln("IDNA.Compare((String)null,'dummy') was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare("dummy", (String)null, 0);
                Errln("IDNA.Compare('dummy',(String)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                if (IDNA.Compare("dummy", "dummy", 0) != 0)
                {
                    Errln("IDNA.Compare('dummy','dummy') was suppose to return a 0.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.Compare('dummy','dummy') was not suppose to return an exception.");
            }

            // Testing the method public static int Compare(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, IDNA2003Options options)

            try
            {
                if (IDNA.Compare("dummy".AsSpan(), "dummy".AsSpan(), 0) != 0)
                {
                    Errln("IDNA.Compare('dummy','dummy') was suppose to return a 0.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.Compare('dummy','dummy') was not suppose to return an exception.");
            }

            // ICU4N: Factored out StringBuffer and UCharacterIterator overloads

            //// Testing the method public static int compare(StringBuffer s1, StringBuffer s2, int options)
            //try
            //{
            //    IDNA.Compare((StringBuffer)null, (StringBuffer)null, 0);
            //    Errln("IDNA.Compare((StringBuffer)null,(StringBuffer)null) was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    IDNA.Compare((StringBuffer)null, new StringBuffer("dummy"), 0);
            //    Errln("IDNA.Compare((StringBuffer)null,'dummy') was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    IDNA.Compare(new StringBuffer("dummy"), (StringBuffer)null, 0);
            //    Errln("IDNA.Compare('dummy',(StringBuffer)null) was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    if (IDNA.Compare(new StringBuffer("dummy"), new StringBuffer("dummy"), 0) != 0)
            //    {
            //        Errln("IDNA.Compare(new StringBuffer('dummy'),new StringBuffer('dummy')) was suppose to return a 0.");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Errln("IDNA.Compare(new StringBuffer('dummy'),new StringBuffer('dummy')) was not suppose to return an exception.");
            //}

            //// Testing the method public static int compare(UCharacterIterator s1, UCharacterIterator s2, int options)
            //UCharacterIterator uci = UCharacterIterator.GetInstance("dummy");
            //try
            //{
            //    IDNA.Compare((UCharacterIterator)null, (UCharacterIterator)null, 0);
            //    Errln("IDNA.Compare((UCharacterIterator)null,(UCharacterIterator)null) was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    IDNA.Compare((UCharacterIterator)null, uci, 0);
            //    Errln("IDNA.Compare((UCharacterIterator)null,UCharacterIterator) was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    IDNA.Compare(uci, (UCharacterIterator)null, 0);
            //    Errln("IDNA.Compare(UCharacterIterator,(UCharacterIterator)null) was suppose to return an exception.");
            //}
            //catch (Exception e)
            //{
            //}

            //try
            //{
            //    if (IDNA.Compare(uci, uci, 0) != 0)
            //    {
            //        Errln("IDNA.Compare(UCharacterIterator('dummy'),UCharacterIterator('dummy')) was suppose to return a 0.");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Errln("IDNA.Compare(UCharacterIterator('dummy'),UCharacterIterator('dummy')) was not suppose to return an exception.");
            //}
        }

        [Test] // ICU4N specific
        public void TestTryConvertToASCII_BufferOverflow()
        {
            Span<char> longBuffer = stackalloc char[256];
            Span<char> shortBuffer = stackalloc char[4];

            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                DoTest(TestData.unicodeIn[i], longBuffer, shortBuffer);
            }

            static void DoTest(ReadOnlySpan<char> src, Span<char> longBuffer, Span<char> shortBuffer)
            {
                bool success = IDNA.TryConvertToASCII(src, longBuffer, out int longBufferLength, IDNA2003Options.Default, out _);
                if (!success)
                {
                    Errln("IDNA.TryConvertToASCII was not suppose to return an exception when there is a long enough buffer.");
                }
                success = IDNA.TryConvertToASCII(src, shortBuffer, out int shortBufferLength, IDNA2003Options.Default, out StringPrepErrorType errorType);
                if (success || errorType != StringPrepErrorType.BufferOverflowError)
                {
                    Errln($"IDNA.TryConvertToASCII was suppose to return a {StringPrepErrorType.BufferOverflowError} when the buffer is too short.");
                }
                if (shortBufferLength < longBufferLength)
                {
                    Errln($"IDNA.TryConvertToASCII was suppose to return a buffer size large enough to fit the text.");
                }
            }
        }

        [Test] // ICU4N specific
        public void TestTryConvertToUnicode_BufferOverflow()
        {
            Span<char> longBuffer = stackalloc char[256];
            Span<char> shortBuffer = stackalloc char[4];

            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                DoTest(TestData.asciiIn[i], longBuffer, shortBuffer);
            }

            static void DoTest(string src, Span<char> longBuffer, Span<char> shortBuffer)
            {
                bool success = IDNA.TryConvertToUnicode(src.AsSpan(), longBuffer, out int longBufferLength, IDNA2003Options.Default, out _);
                if (!success)
                {
                    Errln("IDNA.TryConvertToUnicode was not suppose to return an exception when there is a long enough buffer.");
                }
                success = IDNA.TryConvertToUnicode(src.AsSpan(), shortBuffer, out int shortBufferLength, IDNA2003Options.Default, out StringPrepErrorType errorType);
                if (success || errorType != StringPrepErrorType.BufferOverflowError)
                {
                    Errln($"IDNA.TryConvertToUnicode was suppose to return a {StringPrepErrorType.BufferOverflowError} when the buffer is too short.");
                }
                if (shortBufferLength < longBufferLength)
                {
                    Errln($"IDNA.TryConvertToUnicode was suppose to return a buffer size large enough to fit the text.");
                }
            }
        }

        [Test] // ICU4N specific
        public void TestTryConvertIDNToASCII_BufferOverflow()
        {
            Span<char> longBuffer = stackalloc char[256];
            Span<char> shortBuffer = stackalloc char[4];

            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTest(TestData.domainNames[i], longBuffer, shortBuffer);
            }
            for (int i = 0; i < TestData.domainNames1Uni.Length; i++)
            {
                DoTest(TestData.domainNames1Uni[i], longBuffer, shortBuffer);
            }

            static void DoTest(string src, Span<char> longBuffer, Span<char> shortBuffer)
            {
                bool success = IDNA.TryConvertIDNToASCII(src.AsSpan(), longBuffer, out int longBufferLength, IDNA2003Options.Default, out _);
                if (!success)
                {
                    Errln("IDNA.TryConvertIDNToASCII was not suppose to return an exception when there is a long enough buffer.");
                }
                success = IDNA.TryConvertIDNToASCII(src.AsSpan(), shortBuffer, out int shortBufferLength, IDNA2003Options.Default, out StringPrepErrorType errorType);
                if (success || errorType != StringPrepErrorType.BufferOverflowError)
                {
                    Errln($"IDNA.TryConvertIDNToASCII was suppose to return a {StringPrepErrorType.BufferOverflowError} when the buffer is too short.");
                }
                if (shortBufferLength < longBufferLength)
                {
                    Errln($"IDNA.TryConvertIDNToASCII was suppose to return a buffer size large enough to fit the text.");
                }
            }
        }

        [Test] // ICU4N specific
        public void TestTryConvertIDNToUnicode_BufferOverflow()
        {
            Span<char> longBuffer = stackalloc char[256];
            Span<char> shortBuffer = stackalloc char[4];

            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTest(TestData.domainNames[i], longBuffer, shortBuffer);
            }
            for (int i = 0; i < TestData.domainNamesToASCIIOut.Length; i++)
            {
                DoTest(TestData.domainNamesToASCIIOut[i], longBuffer, shortBuffer);
            }

            static void DoTest(string src, Span<char> longBuffer, Span<char> shortBuffer)
            {
                bool success = IDNA.TryConvertIDNToUnicode(src.AsSpan(), longBuffer, out int longBufferLength, IDNA2003Options.Default, out _);
                if (!success)
                {
                    Errln("IDNA.TryConvertIDNToUnicode was not suppose to return an exception when there is a long enough buffer.");
                }
                success = IDNA.TryConvertIDNToUnicode(src.AsSpan(), shortBuffer, out int shortBufferLength, IDNA2003Options.Default, out StringPrepErrorType errorType);
                if (success || errorType != StringPrepErrorType.BufferOverflowError)
                {
                    Errln($"IDNA.TryConvertIDNToUnicode was suppose to return a {StringPrepErrorType.BufferOverflowError} when the buffer is too short.");
                }
                if (shortBufferLength < longBufferLength)
                {
                    Errln($"IDNA.TryConvertIDNToUnicode was suppose to return a buffer size large enough to fit the text.");
                }
            }
        }
    }
}
