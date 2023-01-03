using ICU4N.Globalization;
using ICU4N.Text;
using J2N.Numerics;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using Integer = J2N.Numerics.Int32;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// Performs regression test for MessageFormat
    /// </summary>
    public class NumberFormatRegressionTest : TestFmwk
    {
        /**
         * alphaWorks upgrade
         */
        [Test]
        public void Test4161100()
        {
            NumberFormat nf = NumberFormat.GetInstance(new CultureInfo("en-US"));
            nf.MinimumFractionDigits = (1);
            nf.MaximumFractionDigits = (1);
            double a = -0.09;
            String s = nf.Format(a);
            Logln(a + " x " +
                  ((DecimalFormat)nf).ToPattern() + " = " + s);
            if (!s.Equals("-0.1", StringComparison.Ordinal))
            {
                Errln("FAIL");
            }
        }

        // ICU4N TODO: Depends on DateFormat

        ///**
        // * DateFormat should call setIntegerParseOnly(TRUE) on adopted
        // * NumberFormat objects.
        // */
        //[Test]
        //public void TestJ691()
        //{

        //    CultureInfo loc = new CultureInfo("fr-CH");

        //    // set up the input date string & expected output
        //    String udt = "11.10.2000";
        //    String exp = "11.10.00";

        //    // create a Calendar for this locale
        //    Calendar cal = Calendar.GetInstance(loc);

        //    // create a NumberFormat for this locale
        //    NumberFormat nf = NumberFormat.GetInstance(loc);

        //    // *** Here's the key: We don't want to have to do THIS:
        //    //nf.setParseIntegerOnly(true);
        //    // or this (with changes to fr_CH per cldrbug:9370):
        //    //nf.setGroupingUsed(false);
        //    // so they are done in DateFormat.setNumberFormat

        //    // create the DateFormat
        //    DateFormat df = DateFormat.GetDateInstance(DateFormat.SHORT, loc);

        //    df.setCalendar(cal);
        //    df.setNumberFormat(nf);

        //    // set parsing to lenient & parse
        //    Date ulocdat = new Date();
        //    df.setLenient(true);
        //    try
        //    {
        //        ulocdat = df.Parse(udt);
        //    }
        //    catch (java.text.ParseException pe)
        //    {
        //        Errln(pe.getMessage());
        //    }
        //    // format back to a string
        //    String outString = df.Format(ulocdat);

        //    if (!outString.Equals(exp, StringComparison.Ordinal))
        //    {
        //        Errln("FAIL: " + udt + " => " + outString);
        //    }
        //}

        /**
         * Test getIntegerInstance();
         */
        [Test]
        public void Test4408066()
        {

            NumberFormat nf1 = NumberFormat.GetIntegerInstance();
            NumberFormat nf2 = NumberFormat.GetIntegerInstance(new CultureInfo("zh-CN"));

            //test isParseIntegerOnly
            if (!nf1.ParseIntegerOnly || !nf2.ParseIntegerOnly)
            {
                Errln("Failed : Integer Number Format Instance should set setParseIntegerOnly(true)");
            }

            //Test format
            {
                double[] data = {
                    -3.75, -2.5, -1.5,
                    -1.25, 0,    1.0,
                    1.25,  1.5,  2.5,
                    3.75,  10.0, 255.5
                };
                String[] expected = {
                    "-4", "-2", "-2",
                    "-1", "0",  "1",
                    "1",  "2",  "2",
                    "4",  "10", "256"
                };

                for (int i = 0; i < data.Length; ++i)
                {
                    String result = nf1.Format(data[i]);
                    if (!result.Equals(expected[i], StringComparison.Ordinal))
                    {
                        Errln("Failed => Source: " + data[i].ToString(CultureInfo.InvariantCulture)
                            + ";Formatted : " + result
                            + ";but expectted: " + expected[i]);
                    }
                }
            }
            //Test parse, Parsing should stop at "."
            {
                String[] data = {
                    "-3.75", "-2.5", "-1.5",
                    "-1.25", "0",    "1.0",
                    "1.25",  "1.5",  "2.5",
                    "3.75",  "10.0", "255.5"
                };
                long[] expected = {
                    -3, -2, -1,
                    -1, 0,  1,
                    1,  1,  2,
                    3,  10, 255
                };

                for (int i = 0; i < data.Length; ++i)
                {
                    Number n = null;
                    try
                    {
                        n = nf1.Parse(data[i]);
                    }
                    catch (FormatException e)
                    {
                        Errln("Failed: " + e.Message);
                    }
                    if (!(n is Long) || (n is Integer))
                    {
                        Errln("Failed: Integer Number Format should parse string to Long/Integer");
                    }
                    if (n.ToInt64() != expected[i])
                    {
                        Errln("Failed=> Source: " + data[i]
                            + ";result : " + n.ToString(CultureInfo.InvariantCulture)
                            + ";expected :" + expected[i].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        // ICU4N TODO: Serialization

        ////Test New serialized DecimalFormat(2.0) read old serialized forms of DecimalFormat(1.3.1.1)
        //[Test]
        //public void TestSerialization() //throws IOException
        //{
        //    byte[][] contents = NumberFormatSerialTestData.getContent();
        //    double data = 1234.56;
        //    String []
        //    expected = {
        //        "1,234.56", "$1,234.56", "1.23456E3", "1,234.56"};
        //    for (int i = 0; i < 4; ++i) {
        //        Stream ois = new MemoryStream(contents[i]);
        //        try
        //        {
        //            NumberFormat format = (NumberFormat)ois.readObject();
        //            String result = format.Format(data);
        //            assertEquals("Deserialization new version should read old version", expected[i], result);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.ToString());
        //            Warnln("FAIL: " + e);
        //        }
        //    }
        //}

        /*
         * Test case for JB#5509, strict parsing issue
         */
        [Test]
        public void TestJB5509()
        {
            String[] data = {
                "1,2",
                "1.2",
                "1,2.5",
                "1,23.5",
                "1,234.5",
                "1,234",
                "1,234,567",
                "1,234,567.8",
                "1,234,5",
                "1,234,5.6",
                "1,234,56.7"
            };
            bool[] expected = { // false for expected parse failure
                false,
                true,
                false,
                false,
                true,
                true,
                true,
                true,
                false,
                false,
                false,
                false
            };

            DecimalFormat df = new DecimalFormat("#,##0.###", new DecimalFormatSymbols(new UCultureInfo("en_US")));
            df.ParseStrict = (true);
            for (int i = 0; i < data.Length; i++)
            {
                try
                {
                    df.Parse(data[i]);
                    if (!expected[i])
                    {
                        Errln("Failed: ParseException must be thrown for string " + data[i]);
                    }
                }
                catch (FormatException pe)
                {
                    if (expected[i])
                    {
                        Errln("Failed: ParseException must not be thrown for string " + data[i]);
                    }
                }
            }
        }

        /*
         * Test case for ticket#5698 - parsing extremely large/small values
         */
        [Test]
        public void TestT5698()
        {
            String[] data = {
                "12345679E66666666666666666",
                "-12345679E66666666666666666",
                ".1E2147483648", // exponent > max int
                ".1E2147483647", // exponent == max int
                ".1E-2147483648", // exponent == min int
                ".1E-2147483649", // exponent < min int
                "1.23E350", // value > max double
                "1.23E300", // value < max double
                "-1.23E350", // value < min double
                "-1.23E300", // value > min double
                "4.9E-324", // value = smallest non-zero double
                "1.0E-325", // 0 < value < smallest non-zero positive double0
                "-1.0E-325", // 0 > value > largest non-zero negative double
            };
            double[] expected = {
                double.PositiveInfinity,
                double.NegativeInfinity,
                double.PositiveInfinity,
                double.PositiveInfinity,
                0.0,
                0.0,
                double.PositiveInfinity,
                1.23e300d,
                double.NegativeInfinity,
                -1.23e300d,
                4.9e-324d,
                0.0,
                -0.0,
            };

            NumberFormat nfmt = NumberFormat.GetInstance();

            for (int i = 0; i < data.Length; i++)
            {
                try
                {
                    Number n = nfmt.Parse(data[i]);
                    if (expected[i] != n.ToDouble())
                    {
                        Errln("Failed: Parsed result for " + data[i] + ": "
                                + n.ToDouble() + " / expected: " + expected[i]);
                    }
                }
                catch (FormatException pe)
                {
                    Errln("Failed: ParseException is thrown for " + data[i]);
                }
            }
        }
        [Test]
        public void TestSurrogatesParsing()
        { // Test parsing of numbers that use digits from the supplemental planes.
            String[] data = {
            "1\ud801\udca2,3\ud801\udca45.67", //
                "\ud801\udca1\ud801\udca2,\ud801\udca3\ud801\udca4\ud801\udca5.\ud801\udca6\ud801\udca7\ud801\udca8", //
                "\ud835\udfd2.\ud835\udfd7E-\ud835\udfd1",
                "\ud835\udfd3.8E-0\ud835\udfd0"
                };
            double[] expected = {
                12345.67,
                12345.678,
                0.0049,
                0.058
            };

            NumberFormat nfmt = NumberFormat.GetInstance();

            for (int i = 0; i < data.Length; i++)
            {
                try
                {
                    Number n = nfmt.Parse(data[i]);
                    if (expected[i] != n.ToDouble())
                    {
                        Errln("Failed: Parsed result for " + data[i] + ": "
                                + n.ToDouble() + " / expected: " + expected[i]);
                    }
                }
                catch (FormatException pe)
                {
                    Errln("Failed: ParseException is thrown for " + data[i]);
                }
            }
        }

        void checkNBSPPatternRtNum(String testcase, NumberFormat nf, double myNumber)
        {
            String myString = nf.Format(myNumber);

            double aNumber;
            try
            {
                aNumber = nf.Parse(myString).ToDouble();
            }
            catch (FormatException e)
            {
                // TODO Auto-generated catch block
                Errln("FAIL: " + testcase + " - failed to parse. " + e.ToString());
                return;
            }
            if (Math.Abs(aNumber - myNumber) > .001)
            {
                Errln("FAIL: " + testcase + ": formatted " + myNumber + ", parsed into " + aNumber + "\n");
            }
            else
            {
                Logln("PASS: " + testcase + ": formatted " + myNumber + ", parsed into " + aNumber + "\n");
            }
        }

        void checkNBSPPatternRT(String testcase, NumberFormat nf)
        {
            checkNBSPPatternRtNum(testcase, nf, 12345.0); // ICU4N: Changed literal because a literal ending with "." is not valid in C#
            checkNBSPPatternRtNum(testcase, nf, -12345.0);
        }

        [Test]
        public void TestNBSPInPattern()
        {
            NumberFormat nf = null;
            String testcase;


            testcase = "ar_AE UNUM_CURRENCY";
            nf = NumberFormat.GetCurrencyInstance(new UCultureInfo("ar_AE"));
            checkNBSPPatternRT(testcase, nf);
            // if we don't have CLDR 1.6 data, bring out the problem anyways

            String SPECIAL_PATTERN = "\u00A4\u00A4'\u062f.\u0625.\u200f\u00a0'###0.00";
            testcase = "ar_AE special pattern: " + SPECIAL_PATTERN;
            nf = new DecimalFormat();
            ((DecimalFormat)nf).ApplyPattern(SPECIAL_PATTERN);
            checkNBSPPatternRT(testcase, nf);

        }

        /*
         * Test case for #9293
         * Parsing currency in strict mode
         */
        [Test]
        public void TestT9293()
        {
            NumberFormat fmt = NumberFormat.GetCurrencyInstance();
            fmt.ParseStrict = (true);

            int val = 123456;
            String txt = fmt.Format(123456);

            ParsePosition pos = new ParsePosition(0);
            Number num = fmt.Parse(txt, pos);

            if (pos.ErrorIndex >= 0)
            {
                Errln("FAIL: Parsing " + txt + " - error index: " + pos.ErrorIndex);
            }
            else if (val != num.ToInt32())
            {
                Errln("FAIL: Parsed result: " + num + " - expected: " + val);
            }
        }

        [Test]
        public void TestAffixesNoCurrency()
        {
            UCultureInfo locale = new UCultureInfo("en");
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(locale, NumberFormatStyle.PluralCurrencyStyle);
            assertEquals(
                "Positive suffix should contain the single currency sign when no currency is set",
                " \u00A4",
                nf.PositiveSuffix);
        }
    }
}
