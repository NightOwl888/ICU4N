using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <author>ram</author>
    public class TestIDNARef : TestFmwk
    {
        private StringPrepParseException unassignedException = new StringPrepParseException("", StringPrepErrorType.UnassignedError);

        [Test]
        public void TestToUnicode()
        {
            try
            {
                for (int i = 0; i < TestData.asciiIn.Length; i++)
                {
                    // test StringBuffer toUnicode
                    DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNAReference.DEFAULT, null);
                    DoTestToUnicode(TestData.asciiIn[i], new String(TestData.unicodeIn[i]), IDNAReference.ALLOW_UNASSIGNED, null);
                    //doTestToUnicode(TestData.asciiIn[i],new String(TestData.unicodeIn[i]),IDNAReference.USE_STD3_RULES, null);
                    //doTestToUnicode(TestData.asciiIn[i],new String(TestData.unicodeIn[i]),IDNAReference.USE_STD3_RULES|IDNAReference.ALLOW_UNASSIGNED, null);

                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }

        [Test]
        public void TestToASCII()
        {
            try
            {
                for (int i = 0; i < TestData.asciiIn.Length; i++)
                {
                    // test StringBuffer toUnicode
                    DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNAReference.DEFAULT, null);
                    DoTestToASCII(new String(TestData.unicodeIn[i]), TestData.asciiIn[i], IDNAReference.ALLOW_UNASSIGNED, null);
                    //doTestToUnicode(TestData.asciiIn[i],new String(TestData.unicodeIn[i]),IDNAReference.USE_STD3_RULES, null);
                    //doTestToUnicode(TestData.asciiIn[i],new String(TestData.unicodeIn[i]),IDNAReference.USE_STD3_RULES|IDNAReference.ALLOW_UNASSIGNED, null);

                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }

        [Test]
        public void TestIDNToASCII()
        {
            try
            {
                for (int i = 0; i < TestData.domainNames.Length; i++)
                {
                    DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.DEFAULT, null);
                    DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.ALLOW_UNASSIGNED, null);
                    DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.USE_STD3_RULES, null);
                    DoTestIDNToASCII(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.ALLOW_UNASSIGNED | IDNAReference.USE_STD3_RULES, null);
                }

                for (int i = 0; i < TestData.domainNames1Uni.Length; i++)
                {
                    DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNAReference.DEFAULT, null);
                    DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNAReference.ALLOW_UNASSIGNED, null);
                    DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNAReference.USE_STD3_RULES, null);
                    DoTestIDNToASCII(TestData.domainNames1Uni[i], TestData.domainNamesToASCIIOut[i], IDNAReference.ALLOW_UNASSIGNED | IDNAReference.USE_STD3_RULES, null);

                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }
        [Test]
        public void TestIDNToUnicode()
        {
            try
            {
                for (int i = 0; i < TestData.domainNames.Length; i++)
                {
                    DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.DEFAULT, null);
                    DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.ALLOW_UNASSIGNED, null);
                    DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.USE_STD3_RULES, null);
                    DoTestIDNToUnicode(TestData.domainNames[i], TestData.domainNames[i], IDNAReference.ALLOW_UNASSIGNED | IDNAReference.USE_STD3_RULES, null);
                }
                for (int i = 0; i < TestData.domainNamesToASCIIOut.Length; i++)
                {
                    DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNAReference.DEFAULT, null);
                    DoTestIDNToUnicode(TestData.domainNamesToASCIIOut[i], TestData.domainNamesToUnicodeOut[i], IDNAReference.ALLOW_UNASSIGNED, null);
                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }

        private void DoTestToUnicode(String src, String expected, IDNA2003Options options, Object expectedException)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestToUnicode.");
                return;
            }

            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNAReference.ConvertToUnicode(src, options);
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

                StringBuffer @out = IDNAReference.ConvertToUnicode(inBuf, options);
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
                StringBuffer @out = IDNAReference.ConvertToUnicode(inIter, options);
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

        private void DoTestIDNToUnicode(String src, String expected, IDNA2003Options options, Object expectedException)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestIDNToUnicode.");
                return;
            }

            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNAReference.ConvertIDNToUnicode(src, options);
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
                StringBuffer @out = IDNAReference.ConvertIDNToUnicode(inBuf, options);
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
                StringBuffer @out = IDNAReference.ConvertIDNToUnicode(inIter, options);
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
        private void DoTestToASCII(String src, String expected, IDNA2003Options options, Object expectedException)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestToASCII.");
                return;
            }

            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNAReference.ConvertToASCII(src, options);
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
                StringBuffer @out = IDNAReference.ConvertToASCII(inBuf, options);
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
                StringBuffer @out = IDNAReference.ConvertToASCII(inIter, options);
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
        private void DoTestIDNToASCII(String src, String expected, IDNA2003Options options, Object expectedException)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestIDNToASCII.");
                return;
            }

            StringBuffer inBuf = new StringBuffer(src);
            UCharacterIterator inIter = UCharacterIterator.GetInstance(src);
            try
            {

                StringBuffer @out = IDNAReference.ConvertIDNToASCII(src, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToIDNAReferenceASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToIDNAReferenceASCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToIDNAReferenceASCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }
            try
            {
                StringBuffer @out = IDNAReference.ConvertIDNtoASCII(inBuf, options);
                if (expected != null && @out != null && !@out.ToString().Equals(expected))
                {
                    Errln("convertToIDNAReferenceASCII did not return expected result with options : " + options +
                          " Expected: " + expected + " Got: " + @out);
                }
                if (expectedException != null && !unassignedException.Equals(expectedException))
                {
                    Errln("convertToIDNAReferenceSCII did not get the expected exception. The operation succeeded!");
                }
            }
            catch (StringPrepParseException ex)
            {
                if (expectedException == null || !ex.Equals(expectedException))
                {
                    Errln("convertToIDNAReferenceSCII did not get the expected exception for source: " + src + " Got:  " + ex.ToString());
                }
            }

            try
            {
                StringBuffer @out = IDNAReference.ConvertIDNtoASCII(inIter, options);
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
            try
            {
                for (int i = 0; i < TestData.conformanceTestCases.Length; i++)
                {

                    TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                    if (testCase.expected != null)
                    {
                        //Test toASCII
                        DoTestToASCII(testCase.input, testCase.output, IDNAReference.DEFAULT, testCase.expected);
                        DoTestToASCII(testCase.input, testCase.output, IDNAReference.ALLOW_UNASSIGNED, testCase.expected);
                    }
                    //Test toUnicode
                    //doTestToUnicode(testCase.input,testCase.output,IDNAReference.DEFAULT,testCase.expected);
                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }
        [Test]
        public void TestNamePrepConformance()
        {
            try
            {
                NamePrepTransform namePrep = NamePrepTransform.GetInstance();
                if (!namePrep.IsReady)
                {
                    Logln("Transliterator is not available on this environment.");
                    return;
                }
                for (int i = 0; i < TestData.conformanceTestCases.Length; i++)
                {
                    TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                    UCharacterIterator iter = UCharacterIterator.GetInstance(testCase.input);
                    try
                    {
                        StringBuffer output = namePrep.Prepare(iter, NamePrepTransform.NONE);
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
                        StringBuffer output = namePrep.Prepare(iter, NamePrepTransform.ALLOW_UNASSIGNED);
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
            catch (TypeInitializationException e)
            {
                Warnln("Could not load NamePrepTransformData");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }

        }
        [Test]
        public void TestErrorCases()
        {
            try
            {
                for (int i = 0; i < TestData.errorCases.Length; i++)
                {
                    TestData.ErrorCase errCase = TestData.errorCases[i];
                    if (errCase.testLabel == true)
                    {
                        // Test ToASCII
                        DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.DEFAULT, errCase.expected);
                        DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.ALLOW_UNASSIGNED, errCase.expected);
                        if (errCase.useSTD3ASCIIRules)
                        {
                            DoTestToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.USE_STD3_RULES, errCase.expected);
                        }
                    }
                    if (errCase.useSTD3ASCIIRules != true)
                    {

                        // Test IDNToASCII
                        DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.DEFAULT, errCase.expected);
                        DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.ALLOW_UNASSIGNED, errCase.expected);

                    }
                    else
                    {
                        DoTestIDNToASCII(new String(errCase.unicode), errCase.ascii, IDNAReference.USE_STD3_RULES, errCase.expected);
                    }

                    //TestToUnicode
                    if (errCase.testToUnicode == true)
                    {
                        if (errCase.useSTD3ASCIIRules != true)
                        {
                            // Test IDNToUnicode
                            DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNAReference.DEFAULT, errCase.expected);
                            DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNAReference.ALLOW_UNASSIGNED, errCase.expected);

                        }
                        else
                        {
                            DoTestIDNToUnicode(errCase.ascii, new String(errCase.unicode), IDNAReference.USE_STD3_RULES, errCase.expected);
                        }
                    }
                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }
        private void DoTestCompare(String s1, String s2, bool isEqual)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestCompare.");
                return;
            }

            try
            {
                int retVal = IDNAReference.Compare(s1, s2, IDNA2003Options.Default);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNAReference.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA2003Options.Default);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
                retVal = IDNAReference.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA2003Options.Default);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
                Errln("Unexpected exception thrown by IDNAReference.compare");
            }

            try
            {
                int retVal = IDNAReference.Compare(s1, s2, IDNA2003Options.AllowUnassigned);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                          " s2: " + Prettify(s2));
                }
                retVal = IDNAReference.Compare(new StringBuffer(s1), new StringBuffer(s2), IDNA2003Options.AllowUnassigned);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
                retVal = IDNAReference.Compare(UCharacterIterator.GetInstance(s1), UCharacterIterator.GetInstance(s2), IDNA2003Options.AllowUnassigned);
                if (isEqual == true && retVal != 0)
                {
                    Errln("Did not get the expected result for s1: " + Prettify(s1) +
                         " s2: " + Prettify(s2));
                }
            }
            catch (Exception e)
            {
                Errln("Unexpected exception thrown by IDNAReference.compare");
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
            try
            {
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
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }

        //  test and ascertain
        //  func(func(func(src))) == func(src)
        private void DoTestChainingToASCII(String source)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestChainingToASCII.");
                return;
            }

            StringBuffer expected;
            StringBuffer chained;

            // test convertIDNToASCII
            expected = IDNAReference.ConvertIDNToASCII(source, IDNAReference.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNAReference.ConvertIDNtoASCII(chained, IDNAReference.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertIDNToASCII");
            }
            // test convertIDNToA
            expected = IDNAReference.ConvertToASCII(source, IDNAReference.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNAReference.ConvertToASCII(chained, IDNAReference.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertToASCII");
            }

        }
        //  test and ascertain
        //  func(func(func(src))) == func(src)
        public void DoTestChainingToUnicode(String source)
        {

            if (!IDNAReference.IsReady)
            {
                Logln("Transliterator is not available on this environment.  Skipping doTestChainingToUnicode.");
                return;
            }

            StringBuffer expected;
            StringBuffer chained;

            // test convertIDNToUnicode
            expected = IDNAReference.ConvertIDNToUnicode(source, IDNAReference.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNAReference.ConvertIDNToUnicode(chained, IDNAReference.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertIDNToUnicode");
            }
            // test convertIDNToA
            expected = IDNAReference.ConvertToUnicode(source, IDNAReference.DEFAULT);
            chained = expected;
            for (int i = 0; i < 4; i++)
            {
                chained = IDNAReference.ConvertToUnicode(chained, IDNAReference.DEFAULT);
            }
            if (!expected.ToString().Equals(chained.ToString()))
            {
                Errln("Chaining test failed for convertToUnicode");
            }

        }
        [Test]
        public void TestChaining()
        {
            try
            {
                for (int i = 0; i < TestData.unicodeIn.Length; i++)
                {
                    DoTestChainingToASCII(new String(TestData.unicodeIn[i]));
                }
                for (int i = 0; i < TestData.asciiIn.Length; i++)
                {
                    DoTestChainingToUnicode(TestData.asciiIn[i]);
                }
            }
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
        }
        [Test]
        public void TestRootLabelSeparator()
        {
            String www = "www.";
            String com = ".com."; /*root label separator*/
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
            try
            {
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
            catch (TypeInitializationException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }
            catch (TypeLoadException ex)
            {
                Warnln("Could not load NamePrepTransform data");
            }

        }
    }
}
