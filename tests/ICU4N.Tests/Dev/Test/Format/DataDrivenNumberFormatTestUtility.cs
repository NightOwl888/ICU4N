using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// A collection of methods to run the data driven number format test suite.
    /// </summary>
    internal class DataDrivenNumberFormatTestUtility : TestFmwk
    {
        /**
         * Base class for code under test.
         */
        public abstract class CodeUnderTest
        {

            /**
             * Returns the ID of the code under test. This ID is used to identify
             * tests that are known to fail for this particular code under test.
             * This implementation returns null which means that by default all
             * tests should work with this code under test.
             * @return 'J' means ICU4J, 'K' means JDK
             */
            public virtual char? Id => null;

            /**
             *  Runs a single formatting test. On success, returns null.
             *  On failure, returns the error. This implementation just returns null.
             *  Subclasses should override.
             *  @param tuple contains the parameters of the format test.
             */
            public virtual string Format(DataDrivenNumberFormatTestData tuple)
            {
                if (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal)) return "fail";
                return null;
            }

            /**
             *  Runs a single toPattern test. On success, returns null.
             *  On failure, returns the error. This implementation just returns null.
             *  Subclasses should override.
             *  @param tuple contains the parameters of the format test.
             */
            public virtual string ToPattern(DataDrivenNumberFormatTestData tuple)
            {
                if (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal)) return "fail";
                return null;
            }

            /**
             *  Runs a single parse test. On success, returns null.
             *  On failure, returns the error. This implementation just returns null.
             *  Subclasses should override.
             *  @param tuple contains the parameters of the format test.
             */
            public virtual string Parse(DataDrivenNumberFormatTestData tuple)
            {
                if (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal)) return "fail";
                return null;
            }

            /**
             *  Runs a single parse currency test. On success, returns null.
             *  On failure, returns the error. This implementation just returns null.
             *  Subclasses should override.
             *  @param tuple contains the parameters of the format test.
             */
            public virtual string ParseCurrency(DataDrivenNumberFormatTestData tuple)
            {
                if (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal)) return "fail";
                return null;
            }

            /**
             * Runs a single select test. On success, returns null.
             *  On failure, returns the error. This implementation just returns null.
             *  Subclasses should override.
             * @param tuple contains the parameters of the format test.
             */
            public virtual string Select(DataDrivenNumberFormatTestData tuple)
            {
                if (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal)) return "fail";
                return null;
            }
        }

        private enum RunMode
        {
            SKIP_KNOWN_FAILURES,
            INCLUDE_KNOWN_FAILURES
        }

        private readonly CodeUnderTest codeUnderTest;
        private string fileLine = null;
        private int fileLineNumber = 0;
        private string fileTestName = "";
        private DataDrivenNumberFormatTestData tuple = new DataDrivenNumberFormatTestData();

        /**
         * Runs all the tests in the data driven test suite against codeUnderTest.
         * @param fileName The name of the test file. A relative file name under
         *   com/ibm/icu/dev/data such as "data.txt"
         * @param codeUnderTest the code under test
         */

        public static void runSuite(
                string fileName, CodeUnderTest codeUnderTest)
        {
            new DataDrivenNumberFormatTestUtility(codeUnderTest)
                    .run(fileName, RunMode.SKIP_KNOWN_FAILURES);
        }

        /**
         * Runs every format test in data driven test suite including those
         * that are known to fail.  If a test is supposed to fail but actually
         * passes, an error is printed.
         *
         * @param fileName The name of the test file. A relative file name under
         *   com/ibm/icu/dev/data such as "data.txt"
         * @param codeUnderTest the code under test
         */
        public static void runFormatSuiteIncludingKnownFailures(
                string fileName, CodeUnderTest codeUnderTest)
        {
            new DataDrivenNumberFormatTestUtility(codeUnderTest)
                    .run(fileName, RunMode.INCLUDE_KNOWN_FAILURES);
        }

        private DataDrivenNumberFormatTestUtility(
                CodeUnderTest codeUnderTest)
        {
            this.codeUnderTest = codeUnderTest;
        }

        private void run(string fileName, RunMode runMode)
        {
            char? codeUnderTestIdObj = codeUnderTest.Id;
            char codeUnderTestId =
                    codeUnderTestIdObj == null ? (char)0 : char.ToUpperInvariant(codeUnderTestIdObj.Value);
            TextReader @in = null;
            try
            {
                @in = TestUtil.GetDataReader("numberformattestspecification.txt", "UTF-8");
                // read first line and remove BOM if present
                readLine(@in);
                if (fileLine != null && fileLine[0] == '\uFEFF')
                {
                    fileLine = fileLine.Substring(1);
                }

                int state = 0;
                IList<string> columnValues;
                IList<string> columnNames = null;
                while (true)
                {
                    if (fileLine == null || fileLine.Length == 0)
                    {
                        if (!readLine(@in))
                        {
                            break;
                        }
                        if (fileLine.Length == 0 && state == 2)
                        {
                            state = 0;
                        }
                        continue;
                    }
                    if (fileLine.StartsWith("//", StringComparison.Ordinal))
                    {
                        fileLine = null;
                        continue;
                    }
                    // Initial setup of test.
                    if (state == 0)
                    {
                        if (fileLine.StartsWith("test ", StringComparison.Ordinal))
                        {
                            fileTestName = fileLine;
                            tuple = new DataDrivenNumberFormatTestData();
                        }
                        else if (fileLine.StartsWith("set ", StringComparison.Ordinal))
                        {
                            if (!setTupleField())
                            {
                                return;
                            }
                        }
                        else if (fileLine.StartsWith("begin", StringComparison.Ordinal))
                        {
                            state = 1;
                        }
                        else
                        {
                            showError("Unrecognized verb.");
                            return;
                        }
                        // column specification
                    }
                    else if (state == 1)
                    {
                        columnNames = splitBy((char)0x09);
                        state = 2;
                        // run the tests
                    }
                    else
                    {
                        int columnNamesSize = columnNames.Count;
                        columnValues = splitBy(columnNamesSize, (char)0x09);
                        int columnValuesSize = columnValues.Count;
                        for (int i = 0; i < columnValuesSize; ++i)
                        {
                            if (!setField(columnNames[i], columnValues[i]))
                            {
                                return;
                            }
                        }
                        for (int i = columnValuesSize; i < columnNamesSize; ++i)
                        {
                            if (!clearField(columnNames[i]))
                            {
                                return;
                            }
                        }
                        if (runMode == RunMode.INCLUDE_KNOWN_FAILURES || !breaks(codeUnderTestId))
                        {
                            String errorMessage;
                            Exception err = null;
                            bool shouldFail = (tuple.output != null && tuple.output.Equals("fail", StringComparison.Ordinal))
                                    ? !breaks(codeUnderTestId)
                                    : breaks(codeUnderTestId);
                            try
                            {
                                errorMessage = isPass(tuple);
                            }
                            catch (Exception e)
                            {
                                err = e;
                                errorMessage = "Exception: " + e + ": " + e.InnerException;
                            }
                            if (shouldFail && errorMessage == null)
                            {
                                showError("Expected failure, but passed");
                            }
                            else if (!shouldFail && errorMessage != null)
                            {
                                if (err != null)
                                {
                                    //ByteArrayOutputStream os = new ByteArrayOutputStream();
                                    //PrintStream ps = new PrintStream(os);
                                    //err.printStackTrace(ps);
                                    string stackTrace = err.StackTrace;
                                    showError(errorMessage + "     Stack trace: " + stackTrace.Substring(0, 500)); // ICU4N: Checked 2nd arg
                                }
                                else
                                {
                                    showError(errorMessage);
                                }
                            }
                        }
                    }
                    fileLine = null;
                }
            }
            catch (Exception e)
            {
                //ByteArrayOutputStream os = new ByteArrayOutputStream();
                //PrintStream ps = new PrintStream(os);
                //e.printStackTrace(ps);
                String stackTrace = e.StackTrace;
                showError("MAJOR ERROR: " + e.ToString() + "     Stack trace: " + stackTrace.Substring(0, 500));
            }
            finally
            {
                try
                {
                    if (@in != null)
                    {
                        @in.Dispose();
                    }
                }
                catch (IOException e)
                {
                    //e.printStackTrace();
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private bool breaks(char code)
        {
            string breaks = tuple.breaks == null ? "" : tuple.breaks;
            return (breaks.ToUpperInvariant().IndexOf(code) != -1);
        }

        private static bool isSpace(char c)
        {
            return (c == 0x09 || c == 0x20 || c == 0x3000);
        }

        private bool setTupleField()
        {
            IList<string> parts = splitBy(3, (char)0x20);
            if (parts.Count < 3)
            {
                showError("Set expects 2 parameters");
                return false;
            }
            return setField(parts[1], parts[2]);
        }

        private bool setField(string name, string value)
        {
            try
            {
                tuple.setField(name, Utility.Unescape(value));
                return true;
            }
            catch (Exception e)
            {
                showError("No such field: " + name + ", or bad value: " + value);
                return false;
            }
        }

        private bool clearField(string name)
        {
            try
            {
                tuple.clearField(name);
                return true;
            }
            catch (Exception e)
            {
                showError("Field cannot be clared: " + name);
                return false;
            }
        }

        private void showError(String message)
        {
            //TestFmwk.Errln(String.Format("line %d: %s\n%s\n%s", fileLineNumber, Utility.Escape(message), fileTestName, fileLine));
            TestFmwk.Errln(String.Format("line {0}: {1}\n{2}\n{3}", fileLineNumber, Utility.Escape(message), fileTestName, fileLine));
        }

        private IList<string> splitBy(char delimiter)
        {
            return splitBy(int.MaxValue, delimiter);
        }

        private IList<string> splitBy(int max, char delimiter)
        {
            List<string> result = new List<string>();
            int colIdx = 0;
            int colStart = 0;
            int len = fileLine.Length;
            for (int idx = 0; colIdx < max - 1 && idx < len; ++idx)
            {
                char ch = fileLine[idx];
                if (ch == delimiter)
                {
                    result.Add(
                            fileLine.Substring(colStart, idx - colStart)); // ICU4N: Corrected 2nd arg
                    ++colIdx;
                    colStart = idx + 1;
                }
            }
            result.Add(fileLine.Substring(colStart, len - colStart)); // ICU4N: Corrected 2nd arg
            return result;
        }

        private bool readLine(TextReader @in) //throws IOException
        {
            string line = @in.ReadLine();
            if (line == null)
            {
                fileLine = null;
                return false;
            }
            ++fileLineNumber;
            // Strip trailing comments and spaces
            int idx = line.Length;
            for (; idx > 0; idx--)
            {
                if (!isSpace(line[idx - 1]))
                {
                    break;
                }
            }
            fileLine = idx == 0 ? "" : line;
            return true;
        }

        private string isPass(DataDrivenNumberFormatTestData tuple)
        {
            StringBuilder result = new StringBuilder();
            if (tuple.format != null && tuple.output != null)
            {
                string errorMessage = codeUnderTest.Format(tuple);
                if (errorMessage != null)
                {
                    result.Append(errorMessage);
                }
            }
            else if (tuple.toPattern != null || tuple.toLocalizedPattern != null)
            {
                string errorMessage = codeUnderTest.ToPattern(tuple);
                if (errorMessage != null)
                {
                    result.Append(errorMessage);
                }
            }
            else if (tuple.parse != null && tuple.output != null && tuple.outputCurrency != null)
            {
                string errorMessage = codeUnderTest.ParseCurrency(tuple);
                if (errorMessage != null)
                {
                    result.Append(errorMessage);
                }
            }
            else if (tuple.parse != null && tuple.output != null)
            {
                string errorMessage = codeUnderTest.Parse(tuple);
                if (errorMessage != null)
                {
                    result.Append(errorMessage);
                }
            }
            else if (tuple.plural != null)
            {
                string errorMessage = codeUnderTest.Select(tuple);
                if (errorMessage != null)
                {
                    result.Append(errorMessage);
                }
            }
            else
            {
                result.Append("Unrecognized test type.");
            }
            if (result.Length > 0)
            {
                result.Append(": ");
                result.Append(tuple);
                return result.ToString();
            }
            return null;
        }
    }
}
