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
        private StringPrepParseException unassignedException = new StringPrepParseException("", StringPrepParseException.UNASSIGNED_ERROR);

        [Test]
        public void TestToUnicode()
        {
            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                // test StringBuffer toUnicode
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.DEFAULT, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.ALLOW_UNASSIGNED, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.USE_STD3_RULES, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.USE_STD3_RULES | IDNA.ALLOW_UNASSIGNED, null);

            }
        }

        [Test]
        public void TestToASCII()
        {
            for (int i = 0; i < TestData.asciiIn.Length; i++)
            {
                // test StringBuffer toUnicode
                DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNA.DEFAULT, null);
                DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNA.ALLOW_UNASSIGNED, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.USE_STD3_RULES, null);
                DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNA.USE_STD3_RULES | IDNA.ALLOW_UNASSIGNED, null);

            }
        }

        [Test]
        public void TestIDNToASCII()
        {
            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA.DEFAULT, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA.ALLOW_UNASSIGNED, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA.USE_STD3_RULES, null);
                DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNA.ALLOW_UNASSIGNED | IDNA.USE_STD3_RULES, null);
            }

            for (int i = 0; i < TestData.domainNames1Uni.Length; i++)
            {
                DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNA.DEFAULT, null);
                DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNA.ALLOW_UNASSIGNED, null);
            }
        }
        [Test]
        public void TestIDNToUnicode()
        {
            for (int i = 0; i < TestData.domainNames.Length; i++)
            {
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA.DEFAULT, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA.ALLOW_UNASSIGNED, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA.USE_STD3_RULES, null);
                DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNA.ALLOW_UNASSIGNED | IDNA.USE_STD3_RULES, null);
            }
            for (int i = 0; i < TestData.domainNamesToASCIIOut.Length; i++)
            {
                DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNA.DEFAULT, null);
                DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNA.ALLOW_UNASSIGNED, null);
            }
        }

        private void DoTestToUnicode(String src, String expected, int options, Object expectedException)
        {
            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNA.ConvertToUnicode(src, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception for source: " + Prettify(src) + " Got:  " + ex.ToString());
                }
            }
            try
            {

                StringBuffer @out = IDNA.ConvertToUnicode(inBuf, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception for source: " + Prettify(src) + " Got:  " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNA.ConvertToUnicode(inIter, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("Did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("Did not get the expected exception for source: " + Prettify(src) + " Got:  " + ex.ToString());
                }
            }
        }

        private void DoTestIDNToUnicode(String src, String expected, int options, Object expectedException)
        {
            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNA.ConvertIDNToUnicode(src, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("convertToUnicode did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
            try
            {
                StringBuffer @out = IDNA.ConvertIDNToUnicode(inBuf, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToUnicode did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("convertToUnicode did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNA.ConvertIDNToUnicode(inIter, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToUnicode did not return expected result with options : " + options +
                          " Expected: " + Prettify(expected) + " Got: " + Prettify(@out));
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("Did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("Did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
        }
        private void DoTestToASCII(String src, String expected, int options, Object expectedException)
        {
            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNA.ConvertToASCII(src, options);
                if (!unassignedException.Equals(expectedException) && expected != null && @out != null && expected != null && @out != null && !@out.ToString().Equals(expected.ToLowerInvariant()))
                {
                    Errln("convertToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("convertToASCII did not get the expected exception for source: " + src + "\n Got:  " + ex.ToString() + "\n Expected: " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNA.ConvertToASCII(inBuf, options);
                if (!unassignedException.Equals(expectedException) && expected != null && @out != null && expected != null && @out != null && !@out.ToString().Equals(expected.ToLowerInvariant()))
                {
                    Errln("convertToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("convertToASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNA.ConvertToASCII(inIter, options);
                if (!unassignedException.Equals(expectedException) && expected != null && @out != null && expected != null && @out != null && !@out.ToString().Equals(expected.ToLowerInvariant()))
                {
                    Errln("convertToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !expectedException.Equals(ex))
                {
                    Errln("convertToASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
        }
        private void DoTestIDNToASCII(String src, String expected, int options, Object expectedException)
        {
            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNA.ConvertIDNToASCII(src, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToIDNASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToIDNASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToIDNASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
            try
            {
                StringBuffer @out = IDNA.ConvertIDNToASCII(inBuf, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToIDNASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToIDNASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToIDNASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNA.ConvertIDNToASCII(inIter, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertIDNToASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }

                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertIDNToASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertIDNToASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
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
                    DoTestToASCII(testCase.input, testCase.output, IDNA.DEFAULT, testCase.expected);
                    DoTestToASCII(testCase.input, testCase.output, IDNA.ALLOW_UNASSIGNED, testCase.expected);
                }
                //Test toUnicode
                //doTestToUnicode(testCase.input,testCase.output,IDNA.DEFAULT,testCase.expected);
            }
        }
        [Test]
        public void TestNamePrepConformance()
        {
            Text.StringPrep namePrep = Text.StringPrep.GetInstance(Text.StringPrep.RFC3491_NAMEPREP);
            for (int i = 0; i < TestData.conformanceTestCases.Length; i++)
            {
                TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                UCharacterIterator iter = UCharacterIterator.GetInstance(testCase.input);
                try
                {
                    StringBuffer output = namePrep.Prepare(iter, Text.StringPrep.DEFAULT);
                    if (testCase.output != null && output != null && !testCase.output.Equals(output.ToString()))
                    {
                        Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                              " Got: " + Prettify(output));
                    }
                    if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception. The operation succeeded!");
                    }
                }
                catch (StringPrepParseException ex)
                {
                    if (testCase.expected == null || !ex.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + ex.ToString());
                    }
                }

                try
                {
                    iter.SetToStart();
                    StringBuffer output = namePrep.Prepare(iter, Text.StringPrep.ALLOW_UNASSIGNED);
                    if (testCase.output != null && output != null && !testCase.output.Equals(output.ToString()))
                    {
                        Errln("Did not get the expected output. Expected: " + Prettify(testCase.output) +
                              " Got: " + Prettify(output));
                    }
                    if (testCase.expected != null && !unassignedException.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception. The operation succeeded!");
                    }
                }
                catch (StringPrepParseException ex)
                {
                    if (testCase.expected == null || !ex.Equals(testCase.expected))
                    {
                        Errln("Did not get the expected exception for source: " + testCase.input + " Got:  " + ex.ToString());
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
                    DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA.DEFAULT, errCase.expected);
                    DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA.ALLOW_UNASSIGNED, errCase.expected);
                    if (errCase.useSTD3ASCIIRules)
                    {
                        DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNA.USE_STD3_RULES, errCase.expected);
                    }
                }
                if (errCase.useSTD3ASCIIRules != true)
                {

                    // Test IDNToASCII
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA.DEFAULT, errCase.expected);
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA.ALLOW_UNASSIGNED, errCase.expected);

                }
                else
                {
                    DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNA.USE_STD3_RULES, errCase.expected);
                }
                //TestToUnicode
                if (errCase.testToUnicode == true)
                {
                    if (errCase.useSTD3ASCIIRules != true)
                    {
                        // Test IDNToUnicode
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA.DEFAULT, errCase.expected);
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA.ALLOW_UNASSIGNED, errCase.expected);

                    }
                    else
                    {
                        DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNA.USE_STD3_RULES, errCase.expected);
                    }
                }
            }
        }
        private void DoTestCompare(String s1, String s2, bool isEqual)
        {
            try
            {
                int retVal = IDNA.Compare(s1, s2, IDNA.DEFAULT);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA.DEFAULT);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA.DEFAULT);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
                Errln("Unexpected exception thrown by IDNA.compare");
            }

            try
            {
                int retVal = IDNA.Compare(s1, s2, IDNA.ALLOW_UNASSIGNED);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA.ALLOW_UNASSIGNED);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
                retVal = IDNA.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA.ALLOW_UNASSIGNED);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
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
            StringBuffer expected;
            StringBuffer chained;

            // test convertIDNToASCII
            expected = IDNA.ConvertIDNToASCII(source, IDNA.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertIDNToASCII(chained, IDNA.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertIDNToASCII");
            }
            // test convertIDNToA
            expected = IDNA.ConvertToASCII(source, IDNA.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertToASCII(chained, IDNA.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertToASCII");
            }
        }

        //  test and ascertain
        //  func(func(func(src))) == func(src)
        private void DoTestChainingToUnicode(String source)
        {
            StringBuffer expected;
            StringBuffer chained;

            // test convertIDNToUnicode
            expected = IDNA.ConvertIDNToUnicode(source, IDNA.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertIDNToUnicode(chained, IDNA.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertIDNToUnicode");
            }
            // test convertIDNToA
            expected = IDNA.ConvertToUnicode(source, IDNA.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNA.ConvertToUnicode(chained, IDNA.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
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


        private static readonly int loopCount = 100;
        private static readonly int maxCharCount = 15;
        // private static final int maxCodePoint = 0x10ffff;
        private Random random = null;

        /**
         * Return a random integer i where 0 <= i < n.
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
            DoTestToASCII(source.ToString(), expected.ToString(), IDNA.DEFAULT, null);
        }

        private StringBuffer _doTestCompareReferenceImpl(StringBuffer src, bool toASCII, int options)
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
            catch (StringPrepParseException e)
            {
                expStatus = e.Error;
            }

            StringBuffer got = null;
            int gotStatus = -1;
            try
            {
                got = toASCII ? IDNA.ConvertToASCII(src, options) : IDNA.ConvertToUnicode(src, options);
            }
            catch (StringPrepParseException e)
            {
                gotStatus = e.Error;
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
            StringBuffer asciiLabel = _doTestCompareReferenceImpl(src, true, IDNA.ALLOW_UNASSIGNED);
            _doTestCompareReferenceImpl(src, true, IDNA.DEFAULT);
            _doTestCompareReferenceImpl(src, true, IDNA.USE_STD3_RULES);
            _doTestCompareReferenceImpl(src, true, IDNA.USE_STD3_RULES | IDNA.ALLOW_UNASSIGNED);

            if (asciiLabel != null)
            {
                // test toUnicode
                _doTestCompareReferenceImpl(src, false, IDNA.ALLOW_UNASSIGNED);
                _doTestCompareReferenceImpl(src, false, IDNA.DEFAULT);
                _doTestCompareReferenceImpl(src, false, IDNA.USE_STD3_RULES);
                _doTestCompareReferenceImpl(src, false, IDNA.USE_STD3_RULES | IDNA.ALLOW_UNASSIGNED);
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
                    String ascii = IDNA.ConvertToASCII(@in[i], IDNA.DEFAULT).ToString();
                    try
                    {
                        String unicode = IDNA.ConvertToUnicode(ascii, IDNA.DEFAULT).ToString();
                        Logln("result " + unicode);
                    }
                    catch (StringPrepParseException ex)
                    {
                        Errln("Unexpected exception for convertToUnicode: " + ex.ToString());
                    }
                }
                catch (StringPrepParseException ex)
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
                    String ascii = IDNA.ConvertToASCII(@in[i], IDNA.DEFAULT).ToString();
                    if (!ascii.Equals(@in[i]))
                    {
                        Errln("Did not get the expected string for convertToASCII. Expected: " + @in[i] + " Got: " + ascii);
                    }
                }
                catch (StringPrepParseException ex)
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
                String uni = IDNA.ConvertToUnicode(src, IDNA.DEFAULT).ToString();
                if (!uni.Equals(src))
                {
                    Errln("Did not get the expected result. Expected: " + Prettify(src) + " Got: " + uni);
                }
            }
            catch (StringPrepParseException ex)
            {
                Logln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                String ascii = IDNA.ConvertToASCII("\u00AD", IDNA.DEFAULT).ToString();
                if (ascii != null)
                {
                    Errln("Did not get the expected exception");
                }
            }
            catch (StringPrepParseException ex)
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
                IDNA.ConvertIDNToUnicode(INVALID_DOMAIN_NAME, IDNA.DEFAULT);
                IDNA.ConvertIDNToUnicode(INVALID_DOMAIN_NAME, IDNA.USE_STD3_RULES);

            }
            catch (StringPrepParseException ex)
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
                IDNA.ConvertIDNToUnicode(domain, IDNA.DEFAULT);
            }
            catch (StringPrepParseException ex)
            {
                Logln("Got the expected exception. " + ex.ToString());
            }
            catch (Exception ex)
            {
                Errln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertIDNToUnicode(domain, IDNA.USE_STD3_RULES);
            }
            catch (StringPrepParseException ex)
            {
                Logln("Got the expected exception. " + ex.ToString());
            }
            catch (Exception ex)
            {
                Errln("Unexpected exception: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode("xn--m\u00FCller", IDNA.DEFAULT);
            }
            catch (Exception ex)
            {
                Errln("ToUnicode operation failed! " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode("xn--m\u00FCller", IDNA.USE_STD3_RULES);
            }
            catch (Exception ex)
            {
                Errln("ToUnicode operation failed! " + ex.ToString());
            }
            try
            {
                IDNA.ConvertIDNToUnicode("xn--m\u1234ller", IDNA.USE_STD3_RULES);
            }
            catch (StringPrepParseException ex)
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
                IDNA.ConvertToASCII(ul, IDNA.DEFAULT);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepParseException ex)
            {
                if (ex.Error != StringPrepParseException.LABEL_TOO_LONG_ERROR)
                {
                    Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToASCII(ul, IDNA.DEFAULT) Succeeded");
                }
            }
            try
            {
                IDNA.ConvertToASCII(ul1, IDNA.DEFAULT);
            }
            catch (StringPrepParseException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode(ul1, IDNA.DEFAULT);
            }
            catch (StringPrepParseException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }
            try
            {
                IDNA.ConvertToUnicode(ul, IDNA.DEFAULT);
            }
            catch (StringPrepParseException ex)
            {
                Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
            }

            String idn = "my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.my_very_very_long_and_incredibly_uncreative_domain_label.ibm.com";
            try
            {
                IDNA.ConvertIDNToASCII(idn, IDNA.DEFAULT);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepParseException ex)
            {
                if (ex.Error != StringPrepParseException.DOMAIN_NAME_TOO_LONG_ERROR)
                {
                    Errln("IDNA.convertToASCII failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToASCII(idn, IDNA.DEFAULT) Succeeded");
                }
            }
            try
            {
                IDNA.ConvertIDNToUnicode(idn, IDNA.DEFAULT);
                Errln("IDNA.convertToUnicode did not fail!");
            }
            catch (StringPrepParseException ex)
            {
                if (ex.Error != StringPrepParseException.DOMAIN_NAME_TOO_LONG_ERROR)
                {
                    Errln("IDNA.convertToUnicode failed with error: " + ex.ToString());
                }
                else
                {
                    Logln("IDNA.ConvertToUnicode(idn, IDNA.DEFAULT) Succeeded");
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
                UCharacterIterator uci = UCharacterIterator.GetInstance("dummy");
                if (!IDNA.ConvertIDNToASCII(uci, 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.convertIDNToASCII(UCharacterIterator, int) was suppose to "
                            + "return the same string passed.");
                }
                if (!IDNA.ConvertIDNToASCII(new StringBuffer("dummy"), 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.convertIDNToASCII(StringBuffer, int) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.convertIDNToASCII was not suppose to return an exception.");
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
                if (!IDNA.ConvertToUnicode("dummy", 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.ConvertToUnicode(String, int) was suppose to " + "return the same string passed.");
                }
                if (!IDNA.ConvertToUnicode(new StringBuffer("dummy"), 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.ConvertToUnicode(StringBuffer, int) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.convertToUnicode was not suppose to return an exception.");
            }
        }

        /* Tests the method public static StringBuffer convertIDNToUnicode(UCharacterIterator src, int options) */
        [Test]
        public void TestConvertIDNToUnicode()
        {
            try
            {
                UCharacterIterator uci = UCharacterIterator.GetInstance("dummy");
                if (!IDNA.ConvertIDNToUnicode(uci, 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToUnicode(UCharacterIterator, int) was suppose to "
                            + "return the same string passed.");
                }
                if (!IDNA.ConvertIDNToUnicode(new StringBuffer("dummy"), 0).ToString().Equals("dummy"))
                {
                    Errln("IDNA.ConvertIDNToUnicode(StringBuffer, int) was suppose to " + "return the same string passed.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.convertIDNToUnicode was not suppose to return an exception.");
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

            // Testing the method public static int compare(StringBuffer s1, StringBuffer s2, int options)
            try
            {
                IDNA.Compare((StringBuffer)null, (StringBuffer)null, 0);
                Errln("IDNA.Compare((StringBuffer)null,(StringBuffer)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare((StringBuffer)null, new StringBuffer("dummy"), 0);
                Errln("IDNA.Compare((StringBuffer)null,'dummy') was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare(new StringBuffer("dummy"), (StringBuffer)null, 0);
                Errln("IDNA.Compare('dummy',(StringBuffer)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                if (IDNA.Compare(new StringBuffer("dummy"), new StringBuffer("dummy"), 0) != 0)
                {
                    Errln("IDNA.Compare(new StringBuffer('dummy'),new StringBuffer('dummy')) was suppose to return a 0.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.Compare(new StringBuffer('dummy'),new StringBuffer('dummy')) was not suppose to return an exception.");
            }

            // Testing the method public static int compare(UCharacterIterator s1, UCharacterIterator s2, int options)
            UCharacterIterator uci = UCharacterIterator.GetInstance("dummy");
            try
            {
                IDNA.Compare((UCharacterIterator)null, (UCharacterIterator)null, 0);
                Errln("IDNA.Compare((UCharacterIterator)null,(UCharacterIterator)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare((UCharacterIterator)null, uci, 0);
                Errln("IDNA.Compare((UCharacterIterator)null,UCharacterIterator) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                IDNA.Compare(uci, (UCharacterIterator)null, 0);
                Errln("IDNA.Compare(UCharacterIterator,(UCharacterIterator)null) was suppose to return an exception.");
            }
            catch (Exception e)
            {
            }

            try
            {
                if (IDNA.Compare(uci, uci, 0) != 0)
                {
                    Errln("IDNA.Compare(UCharacterIterator('dummy'),UCharacterIterator('dummy')) was suppose to return a 0.");
                }
            }
            catch (Exception e)
            {
                Errln("IDNA.Compare(UCharacterIterator('dummy'),UCharacterIterator('dummy')) was not suppose to return an exception.");
            }
        }
    }
}
