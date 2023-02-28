using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Number = J2N.Numerics.Number;
using Integer = J2N.Numerics.Int32;
using Long = J2N.Numerics.Int64;
using Double = J2N.Numerics.Double;
using Float = J2N.Numerics.Single;
using J2N;
using ICU4N.Support.Text;
using StringBuffer = System.Text.StringBuilder;
using J2N.Globalization;
using System.Threading;
using J2N.Threading;
using System.Reflection;

namespace ICU4N.Dev.Test.Format
{
    using BigDecimal = ICU4N.Numerics.BigDecimal;
    using MathContext = ICU4N.Numerics.MathContext;
    using BigInteger = ICU4N.Numerics.BigMath.BigInteger;
    using static ICU4N.Text.DecimalFormat;

    public class NumberFormatTest : TestFmwk
    {
        [Test]
        public void TestRoundingScientific10542()
        {
            DecimalFormat format =
                    new DecimalFormat("0.00E0");

            int[] roundingModes = {
              (int)BigDecimal.RoundCeiling,
              (int)BigDecimal.RoundDown,
              (int)BigDecimal.RoundFloor,
              (int)BigDecimal.RoundHalfDown,
              (int)BigDecimal.RoundHalfEven,
              (int)BigDecimal.RoundHalfUp,
              (int)BigDecimal.RoundUp };
            string[] descriptions = {
                "Round Ceiling",
                "Round Down",
                "Round Floor",
                "Round half down",
                "Round half even",
                "Round half up",
                "Round up"};

            double[] values = { -0.003006, -0.003005, -0.003004, 0.003014, 0.003015, 0.003016 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            string[][] expected = {
                new string[] {"-3.00E-3", "-3.00E-3", "-3.00E-3", "3.02E-3", "3.02E-3", "3.02E-3"},
                new string[] {"-3.00E-3", "-3.00E-3", "-3.00E-3", "3.01E-3", "3.01E-3", "3.01E-3"},
                new string[] {"-3.01E-3", "-3.01E-3", "-3.01E-3", "3.01E-3", "3.01E-3", "3.01E-3"},
                new string[] {"-3.01E-3", "-3.00E-3", "-3.00E-3", "3.01E-3", "3.01E-3", "3.02E-3"},
                new string[] {"-3.01E-3", "-3.00E-3", "-3.00E-3", "3.01E-3", "3.02E-3", "3.02E-3"},
                new string[] {"-3.01E-3", "-3.01E-3", "-3.00E-3", "3.01E-3", "3.02E-3", "3.02E-3"},
                new string[] {"-3.01E-3", "-3.01E-3", "-3.01E-3", "3.02E-3", "3.02E-3", "3.02E-3"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { -3006.0, -3005, -3004, 3014, 3015, 3016 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"-3.00E3", "-3.00E3", "-3.00E3", "3.02E3", "3.02E3", "3.02E3"},
                new string[] {"-3.00E3", "-3.00E3", "-3.00E3", "3.01E3", "3.01E3", "3.01E3"},
                new string[] {"-3.01E3", "-3.01E3", "-3.01E3", "3.01E3", "3.01E3", "3.01E3"},
                new string[] {"-3.01E3", "-3.00E3", "-3.00E3", "3.01E3", "3.01E3", "3.02E3"},
                new string[] {"-3.01E3", "-3.00E3", "-3.00E3", "3.01E3", "3.02E3", "3.02E3"},
                new string[] {"-3.01E3", "-3.01E3", "-3.00E3", "3.01E3", "3.02E3", "3.02E3"},
                new string[] {"-3.01E3", "-3.01E3", "-3.01E3", "3.02E3", "3.02E3", "3.02E3"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { 0.0, -0.0 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"},
                new string[] {"0.00E0", "-0.00E0"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { 1e25, 1e25 + 1e15, 1e25 - 1e15 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"1.00E25", "1.01E25", "1.00E25"},
                new string[] {"1.00E25", "1.00E25", "9.99E24"},
                new string[] {"1.00E25", "1.00E25", "9.99E24"},
                new string[] {"1.00E25", "1.00E25", "1.00E25"},
                new string[] {"1.00E25", "1.00E25", "1.00E25"},
                new string[] {"1.00E25", "1.00E25", "1.00E25"},
                new string[] {"1.00E25", "1.01E25", "1.00E25"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { -1e25, -1e25 + 1e15, -1e25 - 1e15 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"-1.00E25", "-9.99E24", "-1.00E25"},
                new string[] {"-1.00E25", "-9.99E24", "-1.00E25"},
                new string[] {"-1.00E25", "-1.00E25", "-1.01E25"},
                new string[] {"-1.00E25", "-1.00E25", "-1.00E25"},
                new string[] {"-1.00E25", "-1.00E25", "-1.00E25"},
                new string[] {"-1.00E25", "-1.00E25", "-1.00E25"},
                new string[] {"-1.00E25", "-1.00E25", "-1.01E25"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { 1e-25, 1e-25 + 1e-35, 1e-25 - 1e-35 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"1.00E-25", "1.01E-25", "1.00E-25"},
                new string[] {"1.00E-25", "1.00E-25", "9.99E-26"},
                new string[] {"1.00E-25", "1.00E-25", "9.99E-26"},
                new string[] {"1.00E-25", "1.00E-25", "1.00E-25"},
                new string[] {"1.00E-25", "1.00E-25", "1.00E-25"},
                new string[] {"1.00E-25", "1.00E-25", "1.00E-25"},
                new string[] {"1.00E-25", "1.01E-25", "1.00E-25"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
            values = new double[] { -1e-25, -1e-25 + 1e-35, -1e-25 - 1e-35 };
            // The order of these expected values correspond to the order of roundingModes and the order of values.
            expected = new string[][]{
                new string[] {"-1.00E-25", "-9.99E-26", "-1.00E-25"},
                new string[] {"-1.00E-25", "-9.99E-26", "-1.00E-25"},
                new string[] {"-1.00E-25", "-1.00E-25", "-1.01E-25"},
                new string[] {"-1.00E-25", "-1.00E-25", "-1.00E-25"},
                new string[] {"-1.00E-25", "-1.00E-25", "-1.00E-25"},
                new string[] {"-1.00E-25", "-1.00E-25", "-1.00E-25"},
                new string[] {"-1.00E-25", "-1.00E-25", "-1.01E-25"}};
            verifyRounding(format, values, expected, roundingModes, descriptions);
        }

        private void verifyRounding(DecimalFormat format, double[] values, string[][] expected, int[] roundingModes,
                string[] descriptions)
        {
            for (int i = 0; i < roundingModes.Length; i++)
            {
                format.RoundingMode = (Numerics.BigMath.RoundingMode)(roundingModes[i]);
                for (int j = 0; j < values.Length; j++)
                {
                    assertEquals(descriptions[i] + " " + values[j], expected[i][j], format.Format(values[j]));
                }
            }
        }

        [Test]
        public void Test10419RoundingWith0FractionDigits()
        {
            object[][] data = new object[][]{
                new object[]{BigDecimal.RoundCeiling, 1.488, "2"},
                new object[]{BigDecimal.RoundDown, 1.588, "1"},
                new object[]{BigDecimal.RoundFloor, 1.588, "1"},
                new object[]{BigDecimal.RoundHalfDown, 1.5, "1"},
                new object[]{BigDecimal.RoundHalfEven, 2.5, "2"},
                new object[]{BigDecimal.RoundHalfUp, 2.5, "3"},
                new object[]{BigDecimal.RoundUp, 1.5, "2"},
        };
            NumberFormat nff = NumberFormat.GetNumberInstance(new UCultureInfo("en"));
            nff.MaximumFractionDigits = (0);
            foreach (object[] item in data)
            {
                nff.RoundingMode = (((Numerics.BigMath.RoundingMode)item[0]));
                assertEquals("Test10419", item[2], nff.Format(item[1]));
            }
        }

        [Test]
        public void TestParseNegativeWithFaLocale()
        {
            DecimalFormat parser = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("fa"));
            try
            {
                double value = parser.Parse("-0,5").ToDouble();
                assertEquals("Expect -0.5", -0.5, value);
            }
            catch (FormatException e)
            {
                TestFmwk.Errln("Parsing -0.5 should have succeeded.");
            }
        }

        [Test]
        public void TestParseNegativeWithAlternativeMinusSign()
        {
            DecimalFormat parser = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            try
            {
                double value = parser.Parse("\u208B0.5").ToDouble();
                assertEquals("Expect -0.5", -0.5, value);
            }
            catch (FormatException e)
            {
                TestFmwk.Errln("Parsing -0.5 should have succeeded.");
            }
        }

        // Test various patterns
        [Test]
        public void TestPatterns()
        {

            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            string[] pat = { "#.#", "#.", ".#", "#" };
            int pat_length = pat.Length;
            string[] newpat = { "0.#", "0.", "#.0", "0" };
            string[] num = { "0", "0.", ".0", "0" };
            for (int i = 0; i < pat_length; ++i)
            {
                DecimalFormat fmt = new DecimalFormat(pat[i], sym);
                String newp = fmt.ToPattern();
                if (!newp.Equals(newpat[i], StringComparison.Ordinal))
                    Errln("FAIL: Pattern " + pat[i] + " should transmute to " + newpat[i] +
                            "; " + newp + " seen instead");

                String s = ((NumberFormat)fmt).Format(0);
                if (!s.Equals(num[i], StringComparison.Ordinal))
                {
                    Errln("FAIL: Pattern " + pat[i] + " should format zero as " + num[i] +
                            "; " + s + " seen instead");
                    Logln("Min integer digits = " + fmt.MinimumIntegerDigits);
                }
                // BigInteger 0 - ticket#4731
                s = ((NumberFormat)fmt).Format(BigInteger.Zero);
                if (!s.Equals(num[i], StringComparison.Ordinal))
                {
                    Errln("FAIL: Pattern " + pat[i] + " should format BigInteger zero as " + num[i] +
                            "; " + s + " seen instead");
                    Logln("Min integer digits = " + fmt.MinimumIntegerDigits);
                }
            }
        }

        // Test exponential pattern
        [Test]
        public void TestExponential()
        {

            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            string[] pat = { "0.####E0", "00.000E00", "##0.######E000", "0.###E0;[0.###E0]" };
            int pat_length = pat.Length;

            double[] val = { 0.01234, 123456789, 1.23e300, -3.141592653e-271 };
            int val_length = val.Length;
            string[] valFormat = {
                // 0.####E0
                "1.234E-2", "1.2346E8", "1.23E300", "-3.1416E-271",
                // 00.000E00
                "12.340E-03", "12.346E07", "12.300E299", "-31.416E-272",
                // ##0.######E000
                "12.34E-003", "123.4568E006", "1.23E300", "-314.1593E-273",
                // 0.###E0;[0.###E0]
                "1.234E-2", "1.235E8", "1.23E300", "[3.142E-271]" };
            /*double valParse[] =
                {
                    0.01234, 123460000, 1.23E300, -3.1416E-271,
                    0.01234, 123460000, 1.23E300, -3.1416E-271,
                    0.01234, 123456800, 1.23E300, -3.141593E-271,
                    0.01234, 123500000, 1.23E300, -3.142E-271,
                };*/ //The variable is never used

            int[] lval = { 0, -1, 1, 123456789 };
            int lval_length = lval.Length;
            string[] lvalFormat = {
                // 0.####E0
                "0E0", "-1E0", "1E0", "1.2346E8",
                // 00.000E00
                "00.000E00", "-10.000E-01", "10.000E-01", "12.346E07",
                // ##0.######E000
                "0E000", "-1E000", "1E000", "123.4568E006",
                // 0.###E0;[0.###E0]
                "0E0", "[1E0]", "1E0", "1.235E8" };
            int[] lvalParse =
                {
                0, -1, 1, 123460000,
                0, -1, 1, 123460000,
                0, -1, 1, 123456800,
                0, -1, 1, 123500000,
            };
            int ival = 0, ilval = 0;
            for (int p = 0; p < pat_length; ++p)
            {
                DecimalFormat fmt = new DecimalFormat(pat[p], sym);
                Logln("Pattern \"" + pat[p] + "\" -toPattern-> \"" + fmt.ToPattern() + "\"");
                int v;
                for (v = 0; v < val_length; ++v)
                {
                    String s;
                    s = ((NumberFormat)fmt).Format(val[v]);
                    Logln(" " + val[v] + " -format-> " + s);
                    if (!s.Equals(valFormat[v + ival], StringComparison.Ordinal))
                        Errln("FAIL: Expected " + valFormat[v + ival]);

                    ParsePosition pos = new ParsePosition(0);
                    double a = fmt.Parse(s, pos).ToDouble();
                    if (pos.Index == s.Length)
                    {
                        Logln("  -parse-> " + a.ToString(CultureInfo.InvariantCulture));
                        // Use epsilon comparison as necessary
                    }
                    else
                        Errln("FAIL: Partial parse (" + pos.Index + " chars) -> " + a);
                }
                for (v = 0; v < lval_length; ++v)
                {
                    String s;
                    s = ((NumberFormat)fmt).Format(lval[v]);
                    Logln(" " + lval[v] + "L -format-> " + s);
                    if (!s.Equals(lvalFormat[v + ilval], StringComparison.Ordinal))
                        Errln("ERROR: Expected " + lvalFormat[v + ilval] + " Got: " + s);

                    ParsePosition pos = new ParsePosition(0);
                    long a = 0;
                    Number A = fmt.Parse(s, pos);
                    if (A != null)
                    {
                        a = A.ToInt64();
                        if (pos.Index == s.Length)
                        {
                            Logln("  -parse-> " + a);
                            if (a != lvalParse[v + ilval])
                                Errln("FAIL: Expected " + lvalParse[v + ilval]);
                        }
                        else
                            Errln("FAIL: Partial parse (" + pos.Index + " chars) -> " + Long.ToString(a, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Errln("Fail to parse the string: " + s);
                    }
                }
                ival += val_length;
                ilval += lval_length;
            }
        }

        // Test the handling of quotes
        [Test]
        public void TestQuotes()
        {

            StringBuffer pat;
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            pat = new StringBuffer("a'fo''o'b#");
            DecimalFormat fmt = new DecimalFormat(pat.ToString(), sym);
            String s = ((NumberFormat)fmt).Format(123);
            Logln("Pattern \"" + pat + "\"");
            Logln(" Format 123 . " + s);
            if (!s.Equals("afo'ob123", StringComparison.Ordinal))
                Errln("FAIL: Expected afo'ob123");

            s = "";
            pat = new StringBuffer("a''b#");
            fmt = new DecimalFormat(pat.ToString(), sym);
            s = ((NumberFormat)fmt).Format(123);
            Logln("Pattern \"" + pat + "\"");
            Logln(" Format 123 . " + s);
            if (!s.Equals("a'b123", StringComparison.Ordinal))
                Errln("FAIL: Expected a'b123");
        }

        [Test]
        public void TestParseCurrencyTrailingSymbol()
        {
            // see sun bug 4709840
            NumberFormat fmt = NumberFormat.GetCurrencyInstance(new CultureInfo("de-DE"));
            float val = 12345.67f;
            String str = fmt.Format(val);
            Logln("val: " + val + " str: " + str);
            try
            {
                Number num = fmt.Parse(str);
                Logln("num: " + num);
            }
            catch (FormatException e)
            {
                Errln("parse of '" + str + "' threw exception: " + e);
            }
        }

        /**
         * Test the handling of the currency symbol in patterns.
         **/
        [Test]
        public void TestCurrencySign()
        {
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            StringBuffer pat = new StringBuffer("");
            char currency = (char)0x00A4;
            // "\xA4#,##0.00;-\xA4#,##0.00"
            pat.Append(currency).Append("#,##0.00;-").Append(currency).Append("#,##0.00");
            DecimalFormat fmt = new DecimalFormat(pat.ToString(), sym);
            String s = ((NumberFormat)fmt).Format(1234.56);
            pat = new StringBuffer();
            Logln("Pattern \"" + fmt.ToPattern() + "\"");
            Logln(" Format " + 1234.56 + " . " + s);
            assertEquals("symbol, pos", "$1,234.56", s);

            s = ((NumberFormat)fmt).Format(-1234.56);
            Logln(" Format " + (-1234.56).ToString(CultureInfo.InvariantCulture) + " . " + s);
            assertEquals("symbol, neg", "-$1,234.56", s);

            pat.Length = (0);
            // "\xA4\xA4 #,##0.00;\xA4\xA4 -#,##0.00"
            pat.Append(currency).Append(currency).Append(" #,##0.00;").Append(currency).Append(currency).Append(" -#,##0.00");
            fmt = new DecimalFormat(pat.ToString(), sym);
            s = ((NumberFormat)fmt).Format(1234.56);
            Logln("Pattern \"" + fmt.ToPattern() + "\"");
            Logln(" Format " + 1234.56.ToString(CultureInfo.InvariantCulture) + " . " + s);
            assertEquals("name, pos", "USD 1,234.56", s);

            s = ((NumberFormat)fmt).Format(-1234.56);
            Logln(" Format " + (-1234.56).ToString(CultureInfo.InvariantCulture) + " . " + s);
            assertEquals("name, neg", "USD -1,234.56", s);
        }

        [Test]
        public void TestSpaceParsing()
        {
            // the data are:
            // the string to be parsed, parsed position, parsed error index
            string[][] DATA = {
                new string[] {"$124", "4", "-1"},
                new string[] {"$124 $124", "4", "-1"},
                new string[] {"$124 ", "4", "-1"},
                new string[] {"$124  ", "4", "-1"},
                new string[] {"$ 124 ", "5", "-1"},
                new string[] {"$\u00A0124 ", "5", "-1"},
                new string[] {" $ 124 ", "6", "-1"},
                new string[] {"124$", "3", "-1"},
                new string[] {"124 $", "3", "-1"},
                new string[] {"$124\u200A", "4", "-1"},
                new string[] {"$\u200A124", "5", "-1"},
        };
            NumberFormat foo = NumberFormat.GetCurrencyInstance();
            for (int i = 0; i < DATA.Length; ++i)
            {
                ParsePosition parsePosition = new ParsePosition(0);
                String stringToBeParsed = DATA[i][0];
                int parsedPosition = Integer.Parse(DATA[i][1], radix: 10);
                int errorIndex = Integer.Parse(DATA[i][2], radix: 10);
                try
                {
                    Number result = foo.Parse(stringToBeParsed, parsePosition);
                    if (parsePosition.Index != parsedPosition ||
                            parsePosition.ErrorIndex != errorIndex)
                    {
                        Errln("FAILED parse " + stringToBeParsed + "; parse position: " + parsePosition.Index + "; error position: " + parsePosition.ErrorIndex);
                    }
                    if (parsePosition.ErrorIndex == -1 &&
                            result.ToDouble() != 124)
                    {
                        Errln("FAILED parse " + stringToBeParsed + "; value " + result.ToDouble());
                    }
                }
                catch (Exception e)
                {
                    Errln("FAILED " + e.ToString());
                }
            }
        }

        [Test]
        public void TestSpaceParsingStrict()
        {
            // All trailing grouping separators should be ignored in strict mode, not just the first.
            object[][] cases = {
                new object[] {"123 ", 3, -1},
                new object[] {"123  ", 3, -1},
                new object[] {"123  ,", 3, -1},
                new object[] {"123,", 3, -1},
                new object[] {"123, ", 3, -1},
                new object[] {"123,,", 3, -1},
                new object[] {"123,, ", 3, -1},
                new object[] {"123 ,", 3, -1},
                new object[] {"123, ", 3, -1},
                new object[] {"123, 456", 3, -1},
                new object[] {"123  456", 0, 8} // TODO: Does this behavior make sense?
        };
            DecimalFormat df = new DecimalFormat("#,###");
            df.ParseStrict = (true);
            foreach (object[] cas in cases)
            {
                string input = (string)cas[0];
                int expectedIndex = (int)cas[1];
                int expectedErrorIndex = (int)cas[2];
                ParsePosition ppos = new ParsePosition(0);
                df.Parse(input, ppos);
                assertEquals("Failed on index: '" + input + "'", expectedIndex, ppos.Index);
                assertEquals("Failed on error: '" + input + "'", expectedErrorIndex, ppos.ErrorIndex);
            }
        }

        [Test]
        public void TestMultiCurrencySign()
        {
            string[][] DATA = {
                // the fields in the following test are:
                // locale,
                // currency pattern (with negative pattern),
                // currency number to be formatted,
                // currency format using currency symbol name, such as "$" for USD,
                // currency format using currency ISO name, such as "USD",
                // currency format using plural name, such as "US dollars".
                // for US locale
                new string[] {"en_US", "\u00A4#,##0.00;-\u00A4#,##0.00", "1234.56", "$1,234.56", "USD 1,234.56", "US dollars 1,234.56"},
                new string[] {"en_US", "\u00A4#,##0.00;-\u00A4#,##0.00", "-1234.56", "-$1,234.56", "-USD 1,234.56", "-US dollars 1,234.56"},
                new string[] {"en_US", "\u00A4#,##0.00;-\u00A4#,##0.00", "1", "$1.00", "USD 1.00", "US dollars 1.00"},
                // for CHINA locale
                new string[] {"zh_CN", "\u00A4#,##0.00;(\u00A4#,##0.00)", "1234.56", "\uFFE51,234.56", "CNY 1,234.56", "\u4EBA\u6C11\u5E01 1,234.56"},
                new string[] {"zh_CN", "\u00A4#,##0.00;(\u00A4#,##0.00)", "-1234.56", "(\uFFE51,234.56)", "(CNY 1,234.56)", "(\u4EBA\u6C11\u5E01 1,234.56)"},
                new string[] {"zh_CN", "\u00A4#,##0.00;(\u00A4#,##0.00)", "1", "\uFFE51.00", "CNY 1.00", "\u4EBA\u6C11\u5E01 1.00"}
        };

            String doubleCurrencyStr = "\u00A4\u00A4";
            String tripleCurrencyStr = "\u00A4\u00A4\u00A4";

            for (int i = 0; i < DATA.Length; ++i)
            {
                String locale = DATA[i][0];
                String pat = DATA[i][1];
                Double numberToBeFormat = Double.Parse(DATA[i][2], CultureInfo.InvariantCulture);
                DecimalFormatSymbols sym = new DecimalFormatSymbols(new UCultureInfo(locale));
                for (int j = 1; j <= 3; ++j)
                {
                    // j represents the number of currency sign in the pattern.
                    if (j == 2)
                    {
                        pat = pat.Replace("\u00A4", doubleCurrencyStr);
                    }
                    else if (j == 3)
                    {
                        pat = pat.Replace("\u00A4\u00A4", tripleCurrencyStr);
                    }
                    DecimalFormat fmt = new DecimalFormat(pat, sym);
                    String s = ((NumberFormat)fmt).Format(numberToBeFormat);
                    // DATA[i][3] is the currency format result using a
                    // single currency sign.
                    // DATA[i][4] is the currency format result using
                    // double currency sign.
                    // DATA[i][5] is the currency format result using
                    // triple currency sign.
                    // DATA[i][j+2] is the currency format result using
                    // 'j' number of currency sign.
                    String currencyFormatResult = DATA[i][2 + j];
                    if (!s.Equals(currencyFormatResult, StringComparison.Ordinal))
                    {
                        Errln("FAIL format: Expected " + currencyFormatResult + " but got " + s);
                    }
                    try
                    {
                        // mix style parsing
                        for (int k = 3; k <= 4; ++k)
                        {
                            // DATA[i][3] is the currency format result using a
                            // single currency sign.
                            // DATA[i][4] is the currency format result using
                            // double currency sign.
                            // DATA[i][5] is the currency format result using
                            // triple currency sign.
                            // ICU 59: long name parsing requires currency mode.
                            String oneCurrencyFormat = DATA[i][k];
                            if (fmt.Parse(oneCurrencyFormat).ToDouble() !=
                                    numberToBeFormat.ToDouble())
                            {
                                Errln("FAILED parse " + oneCurrencyFormat);
                            }
                        }
                    }
                    catch (FormatException e)
                    {
                        Errln("FAILED, DecimalFormat parse currency: " + e.ToString());
                    }
                }
            }
        }


        [Test]
        [Ignore("ICU4N TODO: Missing dependency MeasureFormat")]
        public void TestCurrencyFormatForMixParsing()
        {
            //    MeasureFormat curFmt = MeasureFormat.getCurrencyFormat(new UCultureInfo("en_US"));
            //    string[] formats = {
            //        "$1,234.56",  // string to be parsed
            //        "USD1,234.56",
            //        "US dollars1,234.56",
            //        "1,234.56 US dollars"
            //};
            //    try
            //    {
            //        for (int i = 0; i < formats.Length; ++i)
            //        {
            //            String stringToBeParsed = formats[i];
            //            CurrencyAmount parsedVal = (CurrencyAmount)curFmt.parseObject(stringToBeParsed);
            //            Number val = parsedVal.Number;
            //            if (!val.Equals(BigDecimal.Parse("1234.56")))
            //            {
            //                Errln("FAIL: getCurrencyFormat of default locale (en_US) failed roundtripping the number. val=" + val);
            //            }
            //            if (!parsedVal.getCurrency().equals(Currency.GetInstance("USD")))
            //            {
            //                Errln("FAIL: getCurrencyFormat of default locale (en_US) failed roundtripping the currency");
            //            }
            //        }
            //    }
            //    catch (FormatException e)
            //    {
            //        Errln("parse FAILED: " + e.ToString());
            //    }
        }

        [Test]
        public void TestDecimalFormatCurrencyParse()
        {
            // new CultureInfo("en-US")
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            StringBuffer pat = new StringBuffer("");
            char currency = (char)0x00A4;
            // "\xA4#,##0.00;-\xA4#,##0.00"
            pat.Append(currency).Append(currency).Append(currency).Append("#,##0.00;-").Append(currency).Append(currency).Append(currency).Append("#,##0.00");
            DecimalFormat fmt = new DecimalFormat(pat.ToString(), sym);
            string[][] DATA = {
                // the data are:
                // string to be parsed, the parsed result (number)
                new string[] {"$1.00", "1"},
                new string[] {"USD1.00", "1"},
                new string[] {"1.00 US dollar", "1"},
                new string[] {"$1,234.56", "1234.56"},
                new string[] {"USD1,234.56", "1234.56"},
                new string[] {"1,234.56 US dollar", "1234.56"},
            };
            try
            {
                for (int i = 0; i < DATA.Length; ++i)
                {
                    string stringToBeParsed = DATA[i][0];
                    double parsedResult = Double.Parse(DATA[i][1], CultureInfo.InvariantCulture);
                    Number num = fmt.Parse(stringToBeParsed);
                    if (num.ToDouble() != parsedResult)
                    {
                        Errln("FAIL parse: Expected " + parsedResult);
                    }
                }
            }
            catch (FormatException e)
            {
                Errln("FAILED, DecimalFormat parse currency: " + e.ToString());
            }
        }

        /**
         * Test localized currency patterns.
         */
        [Test]
        public void TestCurrency()
        {
            string[] DATA = {
                "fr", "CA", "", "1,50\u00a0$",
                "de", "DE", "", "1,50\u00a0\u20AC",
                "de", "DE", "PREEURO", "1,50\u00a0DM",
                "fr", "FR", "", "1,50\u00a0\u20AC",
                "fr", "FR", "PREEURO", "1,50\u00a0F",
            };

            for (int i = 0; i < DATA.Length; i += 4)
            {
                // ICU4N: .NET doesn't support PREEURO, so using ICU for that case instead
                NumberFormat fmt;
                string localeString;
                if (DATA[i + 2] == string.Empty)
                {
                    localeString = string.Concat(DATA[i], "-", DATA[i + 1]);
                    CultureInfo locale = new CultureInfo(localeString);
                    fmt = NumberFormat.GetCurrencyInstance(locale);
                }
                else
                {
                    localeString = string.Concat(DATA[i], "-", DATA[i + 1], "-", DATA[i + 2]);
                    UCultureInfo locale = new UCultureInfo(localeString);
                    fmt = NumberFormat.GetCurrencyInstance(locale);
                }

                String s = fmt.Format(1.50);
                if (s.Equals(DATA[i + 3], StringComparison.Ordinal))
                {
                    Logln("Ok: 1.50 x " + localeString + " => " + s);
                }
                else
                {
                    Logln("FAIL: 1.50 x " + localeString + " => " + s +
                            ", expected " + DATA[i + 3]);
                }
            }

            // format currency with CurrencyAmount
            for (int i = 0; i < DATA.Length; i += 4)
            {
                // ICU4N: .NET doesn't support PREEURO, so using ICU for that case instead
                Currency curr;
                NumberFormat fmt;
                string localeString;
                string currencyName;
                if (DATA[i + 2] == string.Empty)
                {
                    localeString = string.Concat(DATA[i], "-", DATA[i + 1]);
                    CultureInfo locale = new CultureInfo(localeString);
                    curr = Currency.GetInstance(locale);
                    currencyName = curr.GetName(locale, CurrencyNameStyle.LongName, out bool _);
                    fmt = NumberFormat.GetCurrencyInstance(locale);
                }
                else
                {
                    localeString = string.Concat(DATA[i], "-", DATA[i + 1], "-", DATA[i + 2]);
                    UCultureInfo locale = new UCultureInfo(localeString);
                    curr = Currency.GetInstance(locale);
                    currencyName = curr.GetName(locale, CurrencyNameStyle.LongName, out bool _);
                    fmt = NumberFormat.GetCurrencyInstance(locale);
                }


                //CultureInfo locale = new CultureInfo(DATA[i], DATA[i + 1], DATA[i + 2]);

                //Currency curr = Currency.GetInstance(locale);
                //Logln("\nName of the currency is: " + curr.GetName(locale, CurrencyNameStyle.LongName, out bool _));
                Logln("\nName of the currency is: " + currencyName);
                CurrencyAmount cAmt = new CurrencyAmount((Number)Double.GetInstance(1.5), curr); // ICU4N TODO: Investigate why this constructor overload fails to be called
                Logln("CurrencyAmount object's hashCode is: " + cAmt.GetHashCode()); //cover hashCode

                //NumberFormat fmt = NumberFormat.GetCurrencyInstance(locale);
                String sCurr = fmt.Format(cAmt);
                if (sCurr.Equals(DATA[i + 3], StringComparison.Ordinal))
                {
                    Logln("Ok: 1.50 x " + localeString + " => " + sCurr);
                }
                else
                {
                    Errln("FAIL: 1.50 x " + localeString + " => " + sCurr +
                            ", expected " + DATA[i + 3]);
                }
            }

            // ICU4N TODO: Missing dependency MeasureFormat

            ////Cover MeasureFormat.getCurrencyFormat()
            //UCultureInfo save = UCultureInfo.CurrentCulture;
            //UCultureInfo.CurrentCulture = new UCultureInfo("en-US");
            //MeasureFormat curFmt = MeasureFormat.getCurrencyFormat();
            //String strBuf = curFmt.Format(new CurrencyAmount(Float.GetInstance(1234.56), Currency.GetInstance("USD")));

            //try
            //{
            //    CurrencyAmount parsedVal = (CurrencyAmount)curFmt.parseObject(strBuf);
            //    Number val = parsedVal.getNumber();
            //    if (!val.Equals(BigDecimal.Parse("1234.56", CultureInfo.InvariantCulture)))
            //    {
            //        Errln("FAIL: getCurrencyFormat of default locale (en_US) failed roundtripping the number. val=" + val);
            //    }
            //    if (!parsedVal.getCurrency().equals(Currency.GetInstance("USD")))
            //    {
            //        Errln("FAIL: getCurrencyFormat of default locale (en_US) failed roundtripping the currency");
            //    }
            //}
            //catch (FormatException e)
            //{
            //    Errln("FAIL: " + e.Message);
            //}
            //UCultureInfo.CurrentCulture = (save);
        }


        [Test]
        [Ignore("ICU4N TODO: Didn't implement JDK currency conversion because we don't have Currency in .NET")]
        public void TestJavaCurrencyConversion()
        {
            //    java.util.Currency gbpJava = java.util.Currency.getInstance("GBP");
            //    Currency gbpIcu = Currency.GetInstance("GBP");
            //    assertEquals("ICU should equal API value", gbpIcu, Currency.fromJavaCurrency(gbpJava));
            //    assertEquals("Java should equal API value", gbpJava, gbpIcu.toJavaCurrency());
            //    // Test CurrencyAmount constructors
            //    CurrencyAmount ca1 = new CurrencyAmount(123.45, gbpJava);
            //    CurrencyAmount ca2 = new CurrencyAmount(123.45, gbpIcu);
            //    assertEquals("CurrencyAmount from both Double constructors should be equal", ca1, ca2);
            //    // Coverage for the Number constructor
            //    ca1 = new CurrencyAmount(BigDecimal.Parse("543.21", CultureInfo.InvariantCulture), gbpJava);
            //    ca2 = new CurrencyAmount(BigDecimal.Parse("543.21", CultureInfo.InvariantCulture), gbpIcu);
            //    assertEquals("CurrencyAmount from both Number constructors should be equal", ca1, ca2);
        }


        [Test]
        public void TestCurrencyIsoPluralFormat()
        {
            string[][] DATA = {
                // the data are:
                // locale,
                // currency amount to be formatted,
                // currency ISO code to be formatted,
                // format result using CURRENCYSTYLE,
                // format result using ISOCURRENCYSTYLE,
                // format result using PLURALCURRENCYSTYLE,
                new string[] {"en_US", "1", "USD", "$1.00", "USD 1.00", "1.00 US dollars"},
                new string[] {"en_US", "1234.56", "USD", "$1,234.56", "USD 1,234.56", "1,234.56 US dollars"},
                new string[] {"en_US", "-1234.56", "USD", "-$1,234.56", "-USD 1,234.56", "-1,234.56 US dollars"},
                new string[] {"zh_CN", "1", "USD", "US$1.00", "USD 1.00", "1.00 美元"},
                new string[] {"zh_CN", "1234.56", "USD", "US$1,234.56", "USD 1,234.56", "1,234.56 美元"},
                new string[] {"zh_CN", "1", "CNY", "￥1.00", "CNY 1.00", "1.00 人民币"},
                new string[] {"zh_CN", "1234.56", "CNY", "￥1,234.56", "CNY 1,234.56", "1,234.56 人民币"},
                new string[] {"ru_RU", "1", "RUB", "1,00 \u20BD", "1,00 RUB", "1,00 российского рубля"},
                new string[] {"ru_RU", "2", "RUB", "2,00 \u20BD", "2,00 RUB", "2,00 российского рубля"},
                new string[] {"ru_RU", "5", "RUB", "5,00 \u20BD", "5,00 RUB", "5,00 российского рубля"},
                // test locale without currency information
                new string[] {"root", "-1.23", "USD", "-US$ 1.23", "-USD 1.23", "-1.23 USD"},
                new string[] {"root@numbers=latn", "-1.23", "USD", "-US$ 1.23", "-USD 1.23", "-1.23 USD"}, // ensure that the root locale is still used with modifiers
                new string[] {"root@numbers=arab", "-1.23", "USD", "\u061C-\u0661\u066B\u0662\u0663\u00A0US$", "\u061C-\u0661\u066B\u0662\u0663\u00A0USD", "\u061C-\u0661\u066B\u0662\u0663 USD"}, // ensure that the root locale is still used with modifiers
                new string[] {"es_AR", "1", "INR", "INR\u00A01,00", "INR\u00A01,00", "1,00 rupia india"},
                new string[] {"ar_EG", "1", "USD", "١٫٠٠\u00A0US$", "١٫٠٠\u00A0USD", "١٫٠٠ دولار أمريكي"},
            };

            for (int i = 0; i < DATA.Length; ++i)
            {
                for (int k = (int)NumberFormatStyle.CurrencyStyle;
                        k <= (int)NumberFormatStyle.PluralCurrencyStyle;
                        ++k)
                {
                    // k represents currency format style.
                    if (k != (int)NumberFormatStyle.CurrencyStyle &&
                            k != (int)NumberFormatStyle.ISOCurrencyStyle &&
                            k != (int)NumberFormatStyle.PluralCurrencyStyle)
                    {
                        continue;
                    }
                    string localeString = DATA[i][0];
                    Double numberToBeFormat = Double.Parse(DATA[i][1], CultureInfo.InvariantCulture);
                    string currencyISOCode = DATA[i][2];
                    UCultureInfo locale = new UCultureInfo(localeString);
                    NumberFormat numFmt = NumberFormat.GetInstance(locale, (NumberFormatStyle)k);
                    numFmt.Currency = Currency.GetInstance(currencyISOCode);
                    string strBuf = numFmt.Format(numberToBeFormat);
                    int resultDataIndex = k - 1;
                    if (k == (int)NumberFormatStyle.CurrencyStyle)
                    {
                        resultDataIndex = k + 2;
                    }
                    // DATA[i][resultDataIndex] is the currency format result
                    // using 'k' currency style.
                    string formatResult = DATA[i][resultDataIndex];
                    if (!strBuf.Equals(formatResult, StringComparison.Ordinal))
                    {
                        Errln("FAIL: localeID: " + localeString + ", expected(" + formatResult.Length + "): \"" + formatResult + "\", actual(" + strBuf.Length + "): \"" + strBuf + "\"");
                    }
                    // test parsing, and test parsing for all currency formats.
                    for (int j = 3; j < 6; ++j)
                    {
                        // DATA[i][3] is the currency format result using
                        // CURRENCYSTYLE formatter.
                        // DATA[i][4] is the currency format result using
                        // ISOCURRENCYSTYLE formatter.
                        // DATA[i][5] is the currency format result using
                        // PLURALCURRENCYSTYLE formatter.
                        String oneCurrencyFormatResult = DATA[i][j];
                        CurrencyAmount val = numFmt.ParseCurrency(oneCurrencyFormatResult, null);
                        if (val.Number.ToDouble() != numberToBeFormat.ToDouble())
                        {
                            Errln("FAIL: getCurrencyFormat of locale " + localeString + " failed roundtripping the number. val=" + val + "; expected: " + numberToBeFormat);
                        }
                    }
                }
            }
        }


        [Test]
        public void TestMiscCurrencyParsing()
        {
            string[][] DATA = {
                // each has: string to be parsed, parsed position, error position
                new string[] {"1.00 ", "4", "-1", "0", "5"},
                new string[] {"1.00 UAE dirha", "4", "-1", "0", "14"},
                new string[] {"1.00 us dollar", "4", "-1", "14", "-1"},
                new string[] {"1.00 US DOLLAR", "4", "-1", "14", "-1"},
                new string[] {"1.00 usd", "4", "-1", "8", "-1"},
                new string[] {"1.00 USD", "4", "-1", "8", "-1"},
            };
            UCultureInfo locale = new UCultureInfo("en_US");
            for (int i = 0; i < DATA.Length; ++i)
            {
                string stringToBeParsed = DATA[i][0];
                int parsedPosition = Integer.Parse(DATA[i][1], radix: 10);
                int errorIndex = Integer.Parse(DATA[i][2], radix: 10);
                int currParsedPosition = Integer.Parse(DATA[i][3], radix: 10);
                int currErrorIndex = Integer.Parse(DATA[i][4], radix: 10);
                NumberFormat numFmt = NumberFormat.GetInstance(locale, NumberFormatStyle.CurrencyStyle);
                ParsePosition parsePosition = new ParsePosition(0);
                Number val = numFmt.Parse(stringToBeParsed, parsePosition);
                if (parsePosition.Index != parsedPosition ||
                        parsePosition.ErrorIndex != errorIndex)
                {
                    Errln("FAIL: parse failed on case " + i + ". expected position: " + parsedPosition + "; actual: " + parsePosition.Index);
                    Errln("FAIL: parse failed on case " + i + ". expected error position: " + errorIndex + "; actual: " + parsePosition.ErrorIndex);
                }
                if (parsePosition.ErrorIndex == -1 &&
                        val.ToDouble() != 1.00)
                {
                    Errln("FAIL: parse failed. expected 1.00, actual:" + val);
                }
                parsePosition = new ParsePosition(0);
                CurrencyAmount amt = numFmt.ParseCurrency(stringToBeParsed, parsePosition);
                if (parsePosition.Index != currParsedPosition ||
                        parsePosition.ErrorIndex != currErrorIndex)
                {
                    Errln("FAIL: parseCurrency failed on case " + i + ". expected error position: " + currErrorIndex + "; actual: " + parsePosition.ErrorIndex);
                    Errln("FAIL: parseCurrency failed on case " + i + ". expected position: " + currParsedPosition + "; actual: " + parsePosition.Index);
                }
                if (parsePosition.ErrorIndex == -1 &&
                        amt.Number.ToDouble() != 1.00)
                {
                    Errln("FAIL: parseCurrency failed. expected 1.00, actual:" + val);
                }
            }
        }

        private class ParseCurrencyItem
        {
            private readonly String localeString;
            private readonly String descrip;
            private readonly String currStr;
            private readonly int numExpectPos;
            private readonly int numExpectVal;
            private readonly int curExpectPos;
            private readonly int curExpectVal;
            private readonly String curExpectCurr;

            internal ParseCurrencyItem(String locStr, String desc, String curr, int numExPos, int numExVal, int curExPos, int curExVal, String curExCurr)
            {
                localeString = locStr;
                descrip = desc;
                currStr = curr;
                numExpectPos = numExPos;
                numExpectVal = numExVal;
                curExpectPos = curExPos;
                curExpectVal = curExVal;
                curExpectCurr = curExCurr;
            }
            public String getLocaleString() { return localeString; }
            public String getDescrip() { return descrip; }
            public String getCurrStr() { return currStr; }
            public int getNumExpectPos() { return numExpectPos; }
            public int getNumExpectVal() { return numExpectVal; }
            public int getCurExpectPos() { return curExpectPos; }
            public int getCurExpectVal() { return curExpectVal; }
            public String getCurExpectCurr() { return curExpectCurr; }
        }

        [Test]
        public void TestParseCurrency()
        {

            // Note: In cases where the number occurs before the currency sign, non-currency mode will parse the number
            // and stop when it reaches the currency symbol.
            ParseCurrencyItem[] parseCurrencyItems = {
                new ParseCurrencyItem( "en_US", "dollars2", "$2.00",            5,  2,  5,  2,  "USD" ),
                new ParseCurrencyItem( "en_US", "dollars4", "$4",               2,  4,  2,  4,  "USD" ),
                new ParseCurrencyItem( "en_US", "dollars9", "9\u00A0$",         1,  9,  3,  9,  "USD" ),
                new ParseCurrencyItem( "en_US", "pounds3",  "\u00A33.00",       0,  0,  5,  3,  "GBP" ),
                new ParseCurrencyItem( "en_US", "pounds5",  "\u00A35",          0,  0,  2,  5,  "GBP" ),
                new ParseCurrencyItem( "en_US", "pounds7",  "7\u00A0\u00A3",    1,  7,  3,  7,  "GBP" ),
                new ParseCurrencyItem( "en_US", "euros8",   "\u20AC8",          0,  0,  2,  8,  "EUR" ),

                new ParseCurrencyItem( "en_GB", "pounds3",  "\u00A33.00",       5,  3,  5,  3,  "GBP" ),
                new ParseCurrencyItem( "en_GB", "pounds5",  "\u00A35",          2,  5,  2,  5,  "GBP" ),
                new ParseCurrencyItem( "en_GB", "pounds7",  "7\u00A0\u00A3",    1,  7,  3,  7,  "GBP" ),
                new ParseCurrencyItem( "en_GB", "euros4",   "4,00\u00A0\u20AC", 4,400,  6,400,  "EUR" ),
                new ParseCurrencyItem( "en_GB", "euros6",   "6\u00A0\u20AC",    1,  6,  3,  6,  "EUR" ),
                new ParseCurrencyItem( "en_GB", "euros8",   "\u20AC8",          0,  0,  2,  8,  "EUR" ),
                new ParseCurrencyItem( "en_GB", "dollars4", "US$4",             0,  0,  4,  4,  "USD" ),

                new ParseCurrencyItem( "fr_FR", "euros4",   "4,00\u00A0\u20AC", 6,  4,  6,  4,  "EUR" ),
                new ParseCurrencyItem( "fr_FR", "euros6",   "6\u00A0\u20AC",    3,  6,  3,  6,  "EUR" ),
                new ParseCurrencyItem( "fr_FR", "euros8",   "\u20AC8",          0,  0,  2,  8,  "EUR" ),
                new ParseCurrencyItem( "fr_FR", "dollars2", "$2.00",            0,  0,  0,  0,  ""    ),
                new ParseCurrencyItem( "fr_FR", "dollars4", "$4",               0,  0,  0,  0,  ""    ),
            };
            foreach (ParseCurrencyItem item in parseCurrencyItems)
            {
                String localeString = item.getLocaleString();
                UCultureInfo uloc = new UCultureInfo(localeString);
                NumberFormat fmt = null;
                try
                {
                    fmt = NumberFormat.GetCurrencyInstance(uloc);
                }
                catch (Exception e)
                {
                    Errln("NumberFormat.getCurrencyInstance fails for locale " + localeString);
                    continue;
                }
                String currStr = item.getCurrStr();
                ParsePosition parsePos = new ParsePosition(0);

                Number numVal = fmt.Parse(currStr, parsePos);
                if (parsePos.Index != item.getNumExpectPos() || (numVal != null && numVal.ToInt32() != item.getNumExpectVal()))
                {
                    if (numVal != null)
                    {
                        Errln("NumberFormat.getCurrencyInstance parse " + localeString + "/" + item.getDescrip() +
                                ", expect pos/val " + item.getNumExpectPos() + "/" + item.getNumExpectVal() +
                                ", get " + parsePos.Index + "/" + numVal.ToInt32());
                    }
                    else
                    {
                        Errln("NumberFormat.getCurrencyInstance parse " + localeString + "/" + item.getDescrip() +
                                ", expect pos/val " + item.getNumExpectPos() + "/" + item.getNumExpectVal() +
                                ", get " + parsePos.Index + "/(NULL)");
                    }
                }

                parsePos.Index = (0);
                int curExpectPos = item.getCurExpectPos();
                CurrencyAmount currAmt = fmt.ParseCurrency(currStr, parsePos);
                if (parsePos.Index != curExpectPos || (currAmt != null && (currAmt.Number.ToInt32() != item.getCurExpectVal() ||
                        currAmt.Currency.CurrencyCode.CompareToOrdinal(item.getCurExpectCurr()) != 0)))
                {
                    if (currAmt != null)
                    {
                        Errln("NumberFormat.getCurrencyInstance parseCurrency " + localeString + "/" + item.getDescrip() +
                                ", expect pos/val/curr " + curExpectPos + "/" + item.getCurExpectVal() + "/" + item.getCurExpectCurr() +
                                ", get " + parsePos.Index + "/" + currAmt.Number.ToInt32() + "/" + currAmt.Currency.CurrencyCode);
                    }
                    else
                    {
                        Errln("NumberFormat.getCurrencyInstance parseCurrency " + localeString + "/" + item.getDescrip() +
                                ", expect pos/val/curr " + curExpectPos + "/" + item.getCurExpectVal() + "/" + item.getCurExpectCurr() +
                                ", get " + parsePos.Index + "/(NULL)");
                    }
                }
            }
        }

        [Test]
        public void TestParseCurrencyWithWhitespace()
        {
            DecimalFormat df = new DecimalFormat("#,##0.00 ¤¤");
            ParsePosition ppos = new ParsePosition(0);
            df.ParseCurrency("1.00 us denmark", ppos);
            assertEquals("Expected to fail on 'us denmark' string", 9, ppos.ErrorIndex);
        }

        [Test]
        public void TestParseCurrPatternWithDecStyle()
        {
            String currpat = "¤#,##0.00";
            String parsetxt = "x0y$";
            DecimalFormat decfmt = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en_US"), NumberFormatStyle.NumberStyle);
            decfmt.ApplyPattern(currpat);
            ParsePosition ppos = new ParsePosition(0);
            Number value = decfmt.Parse(parsetxt, ppos);
            if (ppos.Index != 0)
            {
                Errln("DecimalFormat.parse expected to fail but got ppos " + ppos.Index + ", value " + value);
            }
        }

        /**
         * Test the Currency object handling, new as of ICU 2.2.
         */
        [Test]
        public void TestCurrencyObject()
        {
            NumberFormat fmt =
                    NumberFormat.GetCurrencyInstance(new CultureInfo("en-US"));

            expectCurrency(fmt, null, 1234.56, "$1,234.56");

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("fr-FR")),
                    1234.56, "\u20AC1,234.56"); // Euro

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("ja-JP")),
                    1234.56, "\u00A51,235"); // Yen

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("fr-CH")), // ICU4N TODO: Is this a valid test case in .NET?
                    1234.56, "CHF 1,234.56"); // no more 0.05 rounding here, see cldrbug 5548

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("en-US")),
                    1234.56, "$1,234.56");

            fmt = NumberFormat.GetCurrencyInstance(new CultureInfo("fr-FR"));

            expectCurrency(fmt, null, 1234.56, "1 234,56 \u20AC");

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("ja-JP")),
                    1234.56, "1 235 JPY"); // Yen

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("fr-CH")), // ICU4N TODO: Is this a valid test case in .NET?
                    1234.56, "1 234,56 CHF"); // no more rounding here, see cldrbug 5548

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("en-US")),
                    1234.56, "1 234,56 $US");

            expectCurrency(fmt, Currency.GetInstance(new CultureInfo("fr-FR")),
                    1234.56, "1 234,56 \u20AC"); // Euro
        }

        [Test]
        public void TestCompatibleCurrencies()
        {
            NumberFormat fmt =
                    NumberFormat.GetCurrencyInstance(new CultureInfo("en-US"));
            expectParseCurrency(fmt, Currency.GetInstance(new CultureInfo("ja-JP")), "\u00A51,235"); // Yen half-width
            expectParseCurrency(fmt, Currency.GetInstance(new CultureInfo("ja-JP")), "\uFFE51,235"); // Yen full-wdith
        }

        [Test]
        public void TestCurrencyPatterns()
        {
            int i;
            Random rnd = new Random(2017);
            CultureInfo[] locs = NumberFormat.GetCultures(UCultureTypes.AllCultures);
            for (i = 0; i < locs.Length; ++i)
            {
                if (rnd.NextDouble() < 0.9)
                {
                    // Check a random subset for speed:
                    // Otherwise, this test takes a large fraction of the entire time.
                    continue;
                }
                NumberFormat nf = NumberFormat.GetCurrencyInstance(locs[i]);
                // Make sure currency formats do not have a variable number
                // of fraction digits
                int min = nf.MinimumFractionDigits;
                int max = nf.MaximumFractionDigits;
                if (min != max)
                {
                    String a = nf.Format(1.0);
                    String b = nf.Format(1.125);
                    Errln("FAIL: " + locs[i] +
                            " min fraction digits != max fraction digits; " +
                            "x 1.0 => " + a +
                            "; x 1.125 => " + b);
                }

                // Make sure EURO currency formats have exactly 2 fraction digits
                if (nf is DecimalFormat)
                {
                    Currency curr = ((DecimalFormat)nf).Currency;
                    if (curr != null && "EUR".Equals(curr.CurrencyCode, StringComparison.Ordinal))
                    {
                        if (min != 2 || max != 2)
                        {
                            String a = nf.Format(1.0);
                            Errln("FAIL: " + locs[i] +
                                    " is a EURO format but it does not have 2 fraction digits; " +
                                    "x 1.0 => " +
                                    a);
                        }
                    }
                }
            }
        }

        /**
         * Do rudimentary testing of parsing.
         */
        [Test]
        public void TestParse()
        {
            String arg = "0.0";
            DecimalFormat format = new DecimalFormat("00");
            double aNumber = 0L;
            try
            {
                aNumber = format.Parse(arg).ToDouble();
            }
            catch (ParseException e)
            {
                Console.Out.WriteLine(e);
            }
            Logln("parse(" + arg + ") = " + aNumber);
        }

        /**
         * Test proper rounding by the format method.
         */
        [Test]
        public void TestRounding487()
        {

            NumberFormat nf = NumberFormat.GetInstance();
            roundingTest(nf, 0.00159999, 4, "0.0016");
            roundingTest(nf, 0.00995, 4, "0.01");

            roundingTest(nf, 12.3995, 3, "12.4");

            roundingTest(nf, 12.4999, 0, "12");
            roundingTest(nf, -19.5, 0, "-20");

        }

        /**
         * Test the functioning of the secondary grouping value.
         */
        [Test]
        public void TestSecondaryGrouping()
        {

            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat f = new DecimalFormat("#,##,###", US);

            expect(f, 123456789L, "12,34,56,789");
            expectPat(f, "#,##,##0");
            f.ApplyPattern("#,###");

            f.SecondaryGroupingSize = (4);
            expect(f, 123456789L, "12,3456,789");
            expectPat(f, "#,####,##0");
            NumberFormat g = NumberFormat.GetInstance(new CultureInfo("hi-IN"));

            String @out = "";
            long l = 1876543210L;
            @out = g.Format(l);

            // expect "1,87,65,43,210", but with Hindi digits
            //         01234567890123
            bool ok = true;
            if (@out.Length != 14)
            {
                ok = false;
            }
            else
            {
                for (int i = 0; i < @out.Length; ++i)
                {
                    bool expectGroup = false;
                    switch (i)
                    {
                        case 1:
                        case 4:
                        case 7:
                        case 10:
                            expectGroup = true;
                            break;
                    }
                    // Later -- fix this to get the actual grouping
                    // character from the resource bundle.
                    bool isGroup = (@out[i] == 0x002C);
                    if (isGroup != expectGroup)
                    {
                        ok = false;
                        break;
                    }
                }
            }
            if (!ok)
            {
                Errln("FAIL  Expected " + l + " x hi_IN . \"1,87,65,43,210\" (with Hindi digits), got \""
                        + @out + "\"");
            }
            else
            {
                Logln("Ok    " + l + " x hi_IN . \"" + @out + "\"");
            }
        }

        /*
         * Internal test utility.
         */
        private void roundingTest(NumberFormat nf, double x, int maxFractionDigits, String expected)
        {
            nf.MaximumFractionDigits = (maxFractionDigits);
            String @out = nf.Format(x);
            Logln(x + " formats with " + maxFractionDigits + " fractional digits to " + @out);
            if (!@out.Equals(expected, StringComparison.Ordinal))
                Errln("FAIL: Expected " + expected);
        }

        /**
         * Upgrade to alphaWorks
         */
        [Test]
        public void TestExponent()
        {
            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat fmt1 = new DecimalFormat("0.###E0", US);
            DecimalFormat fmt2 = new DecimalFormat("0.###E+0", US);
            int n = 1234;
            expect2(fmt1, n, "1.234E3");
            expect2(fmt2, n, "1.234E+3");
            expect(fmt1, "1.234E+3", n); // Either format should parse "E+3"

        }

        /**
         * Upgrade to alphaWorks
         */
        [Test]
        public void TestScientific()
        {

            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));

            // Test pattern round-trip
            string[] PAT = { "#E0", "0.####E0", "00.000E00", "##0.####E000", "0.###E0;[0.###E0]" };
            int PAT_length = PAT.Length;
            int[] DIGITS = {
                // min int, max int, min frac, max frac
                0, 1, 0, 0, // "#E0"
                1, 1, 0, 4, // "0.####E0"
                2, 2, 3, 3, // "00.000E00"
                1, 3, 0, 4, // "##0.####E000"
                1, 1, 0, 3, // "0.###E0;[0.###E0]"
        };
            for (int i = 0; i < PAT_length; ++i)
            {
                String pat = PAT[i];
                DecimalFormat df = new DecimalFormat(pat, US);
                String pat2 = df.ToPattern();
                if (pat.Equals(pat2, StringComparison.Ordinal))
                {
                    Logln("Ok   Pattern rt \"" + pat + "\" . \"" + pat2 + "\"");
                }
                else
                {
                    Errln("FAIL Pattern rt \"" + pat + "\" . \"" + pat2 + "\"");
                }
                // Make sure digit counts match what we expect
                if (i == 0) continue; // outputs to 1,1,0,0 since at least one min digit is required.
                if (df.MinimumIntegerDigits != DIGITS[4 * i]
                        || df.MaximumIntegerDigits != DIGITS[4 * i + 1]
                                || df.MinimumFractionDigits != DIGITS[4 * i + 2]
                                        || df.MaximumFractionDigits != DIGITS[4 * i + 3])
                {
                    Errln("FAIL \"" + pat + "\" min/max int; min/max frac = "
                            + df.MinimumIntegerDigits + "/"
                            + df.MaximumIntegerDigits + ";"
                            + df.MinimumFractionDigits + "/"
                            + df.MaximumFractionDigits + ", expect "
                            + DIGITS[4 * i] + "/"
                            + DIGITS[4 * i + 1] + ";"
                            + DIGITS[4 * i + 2] + "/"
                            + DIGITS[4 * i + 3]);
                }
            }

            expect2(new DecimalFormat("#E0", US), 12345.0, "1.2345E4");
            expect(new DecimalFormat("0E0", US), 12345.0, "1E4");

            // pattern of NumberFormat.getScientificInstance(Locale.US) = "0.######E0" not "#E0"
            // so result = 1.234568E4 not 1.2345678901E4
            //when the pattern problem is finalized, delete comment mark'//'
            //of the following code
            expect2(NumberFormat.GetScientificInstance(new CultureInfo("en-US")), 12345.678901, "1.2345678901E4");
            Logln("Testing NumberFormat.getScientificInstance(ULocale) ...");
            expect2(NumberFormat.GetScientificInstance(new UCultureInfo("en-US")), 12345.678901, "1.2345678901E4");

            expect(new DecimalFormat("##0.###E0", US), 12345.0, "12.34E3");
            expect(new DecimalFormat("##0.###E0", US), 12345.00001, "12.35E3");
            expect2(new DecimalFormat("##0.####E0", US), 12345, "12.345E3");

            // pattern of NumberFormat.getScientificInstance(Locale.US) = "0.######E0" not "#E0"
            // so result = 1.234568E4 not 1.2345678901E4
            expect2(NumberFormat.GetScientificInstance(new CultureInfo("fr-FR")), 12345.678901, "1,2345678901E4");
            Logln("Testing NumberFormat.getScientificInstance(ULocale) ...");
            expect2(NumberFormat.GetScientificInstance(new UCultureInfo("fr-FR")), 12345.678901, "1,2345678901E4");

            expect(new DecimalFormat("##0.####E0", US), 789.12345e-9, "789.12E-9");
            expect2(new DecimalFormat("##0.####E0", US), 780.0e-9, "780E-9"); // 780.e-9 in Java
            expect(new DecimalFormat(".###E0", US), 45678.0, ".457E5");
            expect2(new DecimalFormat(".###E0", US), 0, ".0E0");
            /*
            expect(new DecimalFormat[] { new DecimalFormat("#E0", US),
                                         new DecimalFormat("##E0", US),
                                         new DecimalFormat("####E0", US),
                                         new DecimalFormat("0E0", US),
                                         new DecimalFormat("00E0", US),
                                         new DecimalFormat("000E0", US),
                                       },
                   new Long(45678000),
                   new string[] { "4.5678E7",
                                  "45.678E6",
                                  "4567.8E4",
                                  "5E7",
                                  "46E6",
                                  "457E5",
                                }
                   );
            !
            ! Unroll this test into individual tests below...
            !
             */
            expect2(new DecimalFormat("#E0", US), 45678000, "4.5678E7");
            expect2(new DecimalFormat("##E0", US), 45678000, "45.678E6");
            expect2(new DecimalFormat("####E0", US), 45678000, "4567.8E4");
            expect(new DecimalFormat("0E0", US), 45678000, "5E7");
            expect(new DecimalFormat("00E0", US), 45678000, "46E6");
            expect(new DecimalFormat("000E0", US), 45678000, "457E5");
            /*
            expect(new DecimalFormat("###E0", US, status),
                   new Object[] { new Double(0.0000123), "12.3E-6",
                                  new Double(0.000123), "123E-6",
                                  new Double(0.00123), "1.23E-3",
                                  new Double(0.0123), "12.3E-3",
                                  new Double(0.123), "123E-3",
                                  new Double(1.23), "1.23E0",
                                  new Double(12.3), "12.3E0",
                                  new Double(123), "123E0",
                                  new Double(1230), "1.23E3",
                                 });
            !
            ! Unroll this test into individual tests below...
            !
             */
            expect2(new DecimalFormat("###E0", US), 0.0000123, "12.3E-6");
            expect2(new DecimalFormat("###E0", US), 0.000123, "123E-6");
            expect2(new DecimalFormat("###E0", US), 0.00123, "1.23E-3");
            expect2(new DecimalFormat("###E0", US), 0.0123, "12.3E-3");
            expect2(new DecimalFormat("###E0", US), 0.123, "123E-3");
            expect2(new DecimalFormat("###E0", US), 1.23, "1.23E0");
            expect2(new DecimalFormat("###E0", US), 12.3, "12.3E0");
            expect2(new DecimalFormat("###E0", US), 123.0, "123E0");
            expect2(new DecimalFormat("###E0", US), 1230.0, "1.23E3");
            /*
            expect(new DecimalFormat("0.#E+00", US, status),
                   new Object[] { new Double(0.00012), "1.2E-04",
                                  new Long(12000),     "1.2E+04",
                                 });
            !
            ! Unroll this test into individual tests below...
            !
             */
            expect2(new DecimalFormat("0.#E+00", US), 0.00012, "1.2E-04");
            expect2(new DecimalFormat("0.#E+00", US), 12000, "1.2E+04");
        }

        /**
         * Upgrade to alphaWorks
         */
        [Test]
        public void TestPad()
        {

            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            expect2(new DecimalFormat("*^##.##", US), 0, "^^^^0");
            expect2(new DecimalFormat("*^##.##", US), -1.3, "^-1.3");
            expect2(
                    new DecimalFormat("##0.0####E0*_ 'g-m/s^2'", US),
                    0,
                    "0.0E0______ g-m/s^2");
            expect(
                    new DecimalFormat("##0.0####E0*_ 'g-m/s^2'", US),
                    1.0 / 3,
                    "333.333E-3_ g-m/s^2");
            expect2(new DecimalFormat("##0.0####*_ 'g-m/s^2'", US), 0, "0.0______ g-m/s^2");
            expect(
                    new DecimalFormat("##0.0####*_ 'g-m/s^2'", US),
                    1.0 / 3,
                    "0.33333__ g-m/s^2");

            // Test padding before a sign
            String formatStr = "*x#,###,###,##0.0#;*x(###,###,##0.0#)";
            expect2(new DecimalFormat(formatStr, US), -10, "xxxxxxxxxx(10.0)");
            expect2(new DecimalFormat(formatStr, US), -1000, "xxxxxxx(1,000.0)");
            expect2(new DecimalFormat(formatStr, US), -1000000, "xxx(1,000,000.0)");
            expect2(new DecimalFormat(formatStr, US), -100.37, "xxxxxxxx(100.37)");
            expect2(new DecimalFormat(formatStr, US), -10456.37, "xxxxx(10,456.37)");
            expect2(new DecimalFormat(formatStr, US), -1120456.37, "xx(1,120,456.37)");
            expect2(new DecimalFormat(formatStr, US), -112045600.37, "(112,045,600.37)");
            expect2(new DecimalFormat(formatStr, US), -1252045600.37, "(1,252,045,600.37)");

            expect2(new DecimalFormat(formatStr, US), 10, "xxxxxxxxxxxx10.0");
            expect2(new DecimalFormat(formatStr, US), 1000, "xxxxxxxxx1,000.0");
            expect2(new DecimalFormat(formatStr, US), 1000000, "xxxxx1,000,000.0");
            expect2(new DecimalFormat(formatStr, US), 100.37, "xxxxxxxxxx100.37");
            expect2(new DecimalFormat(formatStr, US), 10456.37, "xxxxxxx10,456.37");
            expect2(new DecimalFormat(formatStr, US), 1120456.37, "xxxx1,120,456.37");
            expect2(new DecimalFormat(formatStr, US), 112045600.37, "xx112,045,600.37");
            expect2(new DecimalFormat(formatStr, US), 10252045600.37, "10,252,045,600.37");

            // Test padding between a sign and a number
            String formatStr2 = "#,###,###,##0.0#*x;(###,###,##0.0#*x)";
            expect2(new DecimalFormat(formatStr2, US), -10, "(10.0xxxxxxxxxx)");
            expect2(new DecimalFormat(formatStr2, US), -1000, "(1,000.0xxxxxxx)");
            expect2(new DecimalFormat(formatStr2, US), -1000000, "(1,000,000.0xxx)");
            expect2(new DecimalFormat(formatStr2, US), -100.37, "(100.37xxxxxxxx)");
            expect2(new DecimalFormat(formatStr2, US), -10456.37, "(10,456.37xxxxx)");
            expect2(new DecimalFormat(formatStr2, US), -1120456.37, "(1,120,456.37xx)");
            expect2(new DecimalFormat(formatStr2, US), -112045600.37, "(112,045,600.37)");
            expect2(new DecimalFormat(formatStr2, US), -1252045600.37, "(1,252,045,600.37)");

            expect2(new DecimalFormat(formatStr2, US), 10, "10.0xxxxxxxxxxxx");
            expect2(new DecimalFormat(formatStr2, US), 1000, "1,000.0xxxxxxxxx");
            expect2(new DecimalFormat(formatStr2, US), 1000000, "1,000,000.0xxxxx");
            expect2(new DecimalFormat(formatStr2, US), 100.37, "100.37xxxxxxxxxx");
            expect2(new DecimalFormat(formatStr2, US), 10456.37, "10,456.37xxxxxxx");
            expect2(new DecimalFormat(formatStr2, US), 1120456.37, "1,120,456.37xxxx");
            expect2(new DecimalFormat(formatStr2, US), 112045600.37, "112,045,600.37xx");
            expect2(new DecimalFormat(formatStr2, US), 10252045600.37, "10,252,045,600.37");

            //testing the setPadCharacter(UnicodeString) and getPadCharacterString()
            DecimalFormat fmt = new DecimalFormat("#", US);
            char padString = 'P';
            fmt.PadCharacter = (padString);
            expectPad(fmt, "*P##.##", PadPosition.BeforePrefix, 5, padString);
            fmt.PadCharacter = ('^');
            expectPad(fmt, "*^#", PadPosition.BeforePrefix, 1, '^');
            //commented untill implementation is complete
            /*  fmt.setPadCharacter((UnicodeString)"^^^");
              expectPad(fmt, "*^^^#", DecimalFormat.kPadBeforePrefix, 3, (UnicodeString)"^^^");
              padString.remove();
              padString.append((UChar)0x0061);
              padString.append((UChar)0x0302);
              fmt.setPadCharacter(padString);
              UChar patternChars[]={0x002a, 0x0061, 0x0302, 0x0061, 0x0302, 0x0023, 0x0000};
              UnicodeString pattern(patternChars);
              expectPad(fmt, pattern , DecimalFormat.kPadBeforePrefix, 4, padString);
             */

            // Test multi-char padding sequence specified via pattern
            expect2(new DecimalFormat("*'😃'####.00", US), 1.1, "😃😃😃1.10");
        }

        /**
         * Upgrade to alphaWorks
         */
        [Test]
        public void TestPatterns2()
        {
            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat fmt = new DecimalFormat("#", US);

            char hat = (char)0x005E; /*^*/

            expectPad(fmt, "*^#", PadPosition.BeforePrefix, 1, hat);
            expectPad(fmt, "$*^#", PadPosition.AfterPrefix, 2, hat);
            expectPad(fmt, "#*^", PadPosition.BeforeSuffix, 1, hat);
            expectPad(fmt, "#$*^", PadPosition.AfterSuffix, 2, hat);
            expectPad(fmt, "$*^$#", (PadPosition)(-1));
            expectPad(fmt, "#$*^$", (PadPosition)(-1));
            expectPad(fmt, "'pre'#,##0*x'post'", PadPosition.BeforeSuffix, 12, (char)0x0078 /*x*/);
            expectPad(fmt, "''#0*x", PadPosition.BeforeSuffix, 3, (char)0x0078 /*x*/);
            expectPad(fmt, "'I''ll'*a###.##", PadPosition.AfterPrefix, 10, (char)0x0061 /*a*/);

            fmt.ApplyPattern("AA#,##0.00ZZ");
            fmt.PadCharacter = (hat);

            fmt.FormatWidth = (10);

            fmt.PadPosition = PadPosition.BeforePrefix;// (DecimalFormat.PAD_BEFORE_PREFIX);
            expectPat(fmt, "*^AA#,##0.00ZZ");

            //fmt.setPadPosition(DecimalFormat.PAD_BEFORE_SUFFIX);
            fmt.PadPosition = PadPosition.BeforeSuffix;
            expectPat(fmt, "AA#,##0.00*^ZZ");

            //fmt.setPadPosition(DecimalFormat.PAD_AFTER_SUFFIX);
            fmt.PadPosition = PadPosition.AfterSuffix;
            expectPat(fmt, "AA#,##0.00ZZ*^");

            //            12  3456789012
            String exp = "AA*^#,##0.00ZZ";
            fmt.FormatWidth = (12);
            //fmt.setPadPosition(DecimalFormat.PAD_AFTER_PREFIX);
            fmt.PadPosition = PadPosition.AfterPrefix;
            expectPat(fmt, exp);

            fmt.FormatWidth = (13);
            //              12  34567890123
            expectPat(fmt, "AA*^##,##0.00ZZ");

            fmt.FormatWidth = (14);
            //              12  345678901234
            expectPat(fmt, "AA*^###,##0.00ZZ");

            fmt.FormatWidth = (15);
            //              12  3456789012345
            expectPat(fmt, "AA*^####,##0.00ZZ"); // This is the interesting case

            // The new implementation produces "AA*^#####,##0.00ZZ", which is functionally equivalent
            // to what the old implementation produced, "AA*^#,###,##0.00ZZ"
            fmt.FormatWidth = (16);
            //              12  34567890123456
            //expectPat(fmt, "AA*^#,###,##0.00ZZ");
            expectPat(fmt, "AA*^#####,##0.00ZZ");
        }

        private class TestFactory : SimpleNumberFormatFactory
        {
            private readonly NumberFormat currencyStyle;

            internal TestFactory(UCultureInfo SRC_LOC, UCultureInfo SWAP_LOC)
                : base(SRC_LOC, true)
            {
                currencyStyle = NumberFormat.GetIntegerInstance(SWAP_LOC);
            }

            public override NumberFormat CreateFormat(UCultureInfo loc, int formatType)
            {
                if (formatType == FormatCurrency)
                {
                    return currencyStyle;
                }
                return null;
            }
        }

        [Test]
        public void TestRegistration()
        {
            UCultureInfo SRC_LOC = new UCultureInfo("fr-FR"); // ULocale.FRANCE;
            UCultureInfo SWAP_LOC = new UCultureInfo("en-US");



            NumberFormat f0 = NumberFormat.GetIntegerInstance(SWAP_LOC);
            NumberFormat f1 = NumberFormat.GetIntegerInstance(SRC_LOC);
            NumberFormat f2 = NumberFormat.GetCurrencyInstance(SRC_LOC);
            Object key = NumberFormat.RegisterFactory(new TestFactory(SRC_LOC, SWAP_LOC));
            NumberFormat f3 = NumberFormat.GetCurrencyInstance(SRC_LOC);
            NumberFormat f4 = NumberFormat.GetIntegerInstance(SRC_LOC);
            NumberFormat.Unregister(key); // restore for other tests
            NumberFormat f5 = NumberFormat.GetCurrencyInstance(SRC_LOC);

            float n = 1234.567f;
            Logln("f0 swap int: " + f0.Format(n));
            Logln("f1 src int: " + f1.Format(n));
            Logln("f2 src cur: " + f2.Format(n));
            Logln("f3 reg cur: " + f3.Format(n));
            Logln("f4 reg int: " + f4.Format(n));
            Logln("f5 unreg cur: " + f5.Format(n));

            if (!f3.Format(n).Equals(f0.Format(n), StringComparison.Ordinal))
            {
                Errln("registered service did not match");
            }
            if (!f4.Format(n).Equals(f1.Format(n), StringComparison.Ordinal))
            {
                Errln("registered service did not inherit");
            }
            if (!f5.Format(n).Equals(f2.Format(n), StringComparison.Ordinal))
            {
                Errln("unregistered service did not match original");
            }
        }

        [Test]
        public void TestScientific2()
        {
            // jb 2552
            DecimalFormat fmt = (DecimalFormat)NumberFormat.GetCurrencyInstance();
            Number num = Double.GetInstance(12.34);
            expect(fmt, num, "$12.34");
            fmt.UseScientificNotation = (true);
            expect(fmt, num, "$1.23E1");
            fmt.UseScientificNotation = (false);
            expect(fmt, num, "$12.34");
        }

        [Test]
        public void TestScientificGrouping()
        {
            // jb 2552
            DecimalFormat fmt = new DecimalFormat("###.##E0");
            expect(fmt, .01234, "12.3E-3");
            expect(fmt, .1234, "123E-3");
            expect(fmt, 1.234, "1.23E0");
            expect(fmt, 12.34, "12.3E0");
            expect(fmt, 123.4, "123E0");
            expect(fmt, 1234, "1.23E3");
        }

        // additional coverage tests

        // sigh, can't have static inner classes, why not?

        internal sealed class PI : Number
        {
            /**
             * For serialization
             */
            private static readonly long serialVersionUID = -305601227915602172L;

            public PI() { }

            public override int ToInt32() { return (int)Math.PI; }

            public override long ToInt64() { return (long)Math.PI; }

            public override float ToSingle() { return (float)Math.PI; }

            public override double ToDouble() { return Math.PI; }

            public override byte ToByte() { return (byte)Math.PI; }

            public override short ToInt16() { return (short)Math.PI; }

            public override string ToString(string format, IFormatProvider provider)
            {
                return Math.PI.ToString(format, provider);
            }

            public static readonly Number INSTANCE = new PI();
        }

        [Test]
        public void TestCoverage()
        {
            NumberFormat fmt = NumberFormat.GetNumberInstance(); // default locale
            Logln(fmt.Format(BigInteger.Parse("1234567890987654321234567890987654321", radix: 10)));

            fmt = NumberFormat.GetScientificInstance(); // default locale

            Logln(fmt.Format(PI.INSTANCE));

            try
            {
                Logln(fmt.Format("12345"));
                Errln("numberformat of string did not throw exception");
            }
            catch (Exception e)
            {
                Logln("PASS: numberformat of string failed as expected");
            }

            int hash = fmt.GetHashCode();
            Logln("hash code " + hash);

            Logln("compare to string returns: " + fmt.Equals(""));

            // For ICU 2.6 - alan
            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat df = new DecimalFormat("'*&'' '\u00A4' ''&*' #,##0.00", US);
            df.Currency = (Currency.GetInstance("INR"));
            expect2(df, 1.0, "*&' \u20B9 '&* 1.00");
            expect2(df, -2.0, "-*&' \u20B9 '&* 2.00");
            df.ApplyPattern("#,##0.00 '*&'' '\u00A4' ''&*'");
            expect2(df, 2.0, "2.00 *&' \u20B9 '&*");
            expect2(df, -1.0, "-1.00 *&' \u20B9 '&*");

            Numerics.BigMath.BigDecimal r;

            r = df.RoundingIncrement;
            if (r != null)
            {
                Errln("FAIL: rounding = " + r + ", expect null");
            }

            if (df.UseScientificNotation)
            {
                Errln("FAIL: isScientificNotation = true, expect false");
            }

            // Create a new instance to flush out currency info
            df = new DecimalFormat("0.00000", US);
            df.UseScientificNotation = (true);
            if (!df.UseScientificNotation)
            {
                Errln("FAIL: isScientificNotation = false, expect true");
            }
            df.MinimumExponentDigits = ((byte)2);
            if (df.MinimumExponentDigits != 2)
            {
                Errln("FAIL: getMinimumExponentDigits = " +
                        df.MinimumExponentDigits + ", expect 2");
            }
            df.ExponentSignAlwaysShown = (true);
            if (!df.ExponentSignAlwaysShown)
            {
                Errln("FAIL: isExponentSignAlwaysShown = false, expect true");
            }
            df.SecondaryGroupingSize = (0);
            if (df.SecondaryGroupingSize != 0)
            {
                Errln("FAIL: getSecondaryGroupingSize = " +
                        df.SecondaryGroupingSize + ", expect 0");
            }
            expect2(df, 3.14159, "3.14159E+00");

            // DecimalFormatSymbols#getInstance
            DecimalFormatSymbols decsym1 = DecimalFormatSymbols.GetInstance();
            DecimalFormatSymbols decsym2 = new DecimalFormatSymbols();
            if (!decsym1.Equals(decsym2))
            {
                Errln("FAIL: DecimalFormatSymbols returned by getInstance()" +
                        "does not match new DecimalFormatSymbols().");
            }
            decsym1 = DecimalFormatSymbols.GetInstance(new CultureInfo("ja-JP"));
            decsym2 = DecimalFormatSymbols.GetInstance(new UCultureInfo("ja-JP"));
            if (!decsym1.Equals(decsym2))
            {
                Errln("FAIL: DecimalFormatSymbols returned by getInstance(Locale.JAPAN)" +
                        "does not match the one returned by getInstance(ULocale.JAPAN).");
            }

            // DecimalFormatSymbols#getAvailableLocales/#getAvailableULocales
            CultureInfo[] allLocales = DecimalFormatSymbols.GetCultures(UCultureTypes.AllCultures);
            if (allLocales.Length == 0)
            {
                Errln("FAIL: Got a empty list for DecimalFormatSymbols.getAvailableLocales");
            }
            else
            {
                Logln("PASS: " + allLocales.Length +
                        " available locales returned by DecimalFormatSymbols.getAvailableLocales");
            }
            UCultureInfo[] allULocales = DecimalFormatSymbols.GetUCultures(UCultureTypes.AllCultures);
            if (allULocales.Length == 0)
            {
                Errln("FAIL: Got a empty list for DecimalFormatSymbols.getAvailableLocales");
            }
            else
            {
                Logln("PASS: " + allULocales.Length +
                        " available locales returned by DecimalFormatSymbols.getAvailableULocales");
            }
        }

        [Test]
        public void TestLocalizedPatternSymbolCoverage()
        {
            string[] standardPatterns = { "#,##0.05+%;#,##0.05-%", "* @@@E0‰" };
            string[] standardPatterns58 = { "#,##0.05+%;#,##0.05-%", "* @@@E0‰;* -@@@E0‰" };
            string[] localizedPatterns = { "▰⁖▰▰໐⁘໐໕†⁜⁙▰⁖▰▰໐⁘໐໕‡⁜", "⁂ ⁕⁕⁕⁑⁑໐‱" };
            string[] localizedPatterns58 = { "▰⁖▰▰໐⁘໐໕+⁜⁙▰⁖▰▰໐⁘໐໕‡⁜", "⁂ ⁕⁕⁕⁑⁑໐‱⁙⁂ ‡⁕⁕⁕⁑⁑໐‱" };

            DecimalFormatSymbols dfs = new DecimalFormatSymbols();
            dfs.GroupingSeparator = ('⁖');
            dfs.DecimalSeparator = ('⁘');
            dfs.PatternSeparator = ('⁙');
            dfs.Digit = ('▰');
            dfs.ZeroDigit = ('໐');
            dfs.SignificantDigit = ('⁕');
            dfs.PlusSign = ('†');
            dfs.MinusSign = ('‡');
            dfs.Percent = ('⁜');
            dfs.PerMill = ('‱');
            dfs.ExponentSeparator = ("⁑⁑"); // tests multi-char sequence
            dfs.PadEscape = ('⁂');

            for (int i = 0; i < 2; i++)
            {
                String standardPattern = standardPatterns[i];
                String standardPattern58 = standardPatterns58[i];
                String localizedPattern = localizedPatterns[i];
                String localizedPattern58 = localizedPatterns58[i];

                DecimalFormat df1 = new DecimalFormat("#", dfs);
                df1.ApplyPattern(standardPattern);
                DecimalFormat df2 = new DecimalFormat("#", dfs);
                df2.ApplyLocalizedPattern(localizedPattern);
                assertEquals("DecimalFormat instances should be equal",
                        df1, df2);
                assertEquals("toPattern should match on localizedPattern instance",
                        standardPattern, df2.ToPattern());
                assertEquals("toLocalizedPattern should match on standardPattern instance",
                        localizedPattern, df1.ToLocalizedPattern());

                // ICU4N TODO: finish Android implementation?
                // Android can't access DecimalFormat_ICU58 for testing (ticket #13283).
                //if (TestUtil.getJavaVendor() == TestUtil.JavaVendor.Android) continue;


                // ICU4N TODO: Missing dependency DecimalFormat_ICU58

                //// Note: ICU 58 does not support plus signs in patterns
                //// Note: ICU 58 always prints the negative part of scientific notation patterns,
                ////       even when the negative part is not necessary
                //DecimalFormat_ICU58 df3 = new DecimalFormat_ICU58("#", dfs);
                //df3.applyPattern(standardPattern); // Reading standardPattern is OK
                //DecimalFormat_ICU58 df4 = new DecimalFormat_ICU58("#", dfs);
                //df4.applyLocalizedPattern(localizedPattern58);
                //// Note: DecimalFormat#equals() is broken on ICU 58
                //assertEquals("toPattern should match on ICU58 localizedPattern instance",
                //        standardPattern58, df4.ToPattern());
                //assertEquals("toLocalizedPattern should match on ICU58 standardPattern instance",
                //        localizedPattern58, df3.ToLocalizedPattern());
            }
        }

        [Test]
        public void TestParseNull() //throws ParseException
        {
            DecimalFormat df = new DecimalFormat();
            try
            {
                df.Parse(null);
                fail("df.Parse(null) didn't throw an exception");
            }
            catch (ArgumentNullException e) { } // ICU4N: Changed exception type to ArgumentNullException
            try
            {
                df.Parse(null, null);
                fail("df.Parse(null) didn't throw an exception");
            }
            catch (ArgumentNullException e) { } // ICU4N: Changed exception type to ArgumentNullException
            try
            {
                df.ParseCurrency(null, null);
                fail("df.Parse(null) didn't throw an exception");
            }
            catch (ArgumentNullException e) { } // ICU4N: Changed exception type to ArgumentNullException
        }

        [Test]
        public void TestWhiteSpaceParsing()
        {
            DecimalFormatSymbols US = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat fmt = new DecimalFormat("a  b#0c  ", US);
            int n = 1234;
            expect(fmt, "a b1234c ", n);
            expect(fmt, "a   b1234c   ", n);
            expect(fmt, "ab1234", n);

            fmt.ApplyPattern("a b #");
            expect(fmt, "ab1234", n);
            expect(fmt, "ab  1234", n);
            expect(fmt, "a b1234", n);
            expect(fmt, "a   b1234", n);
            expect(fmt, " a b 1234", n);

            // Horizontal whitespace is allowed, but not vertical whitespace.
            expect(fmt, "\ta\u00A0b\u20001234", n);
            expect(fmt, "a   \u200A    b1234", n);
            expectParseException(fmt, "\nab1234", n);
            expectParseException(fmt, "a    \n   b1234", n);
            expectParseException(fmt, "a    \u0085   b1234", n);
            expectParseException(fmt, "a    \u2028   b1234", n);

            // Test all characters in the UTS 18 "blank" set stated in the API docstring.
            UnicodeSet blanks = new UnicodeSet("[[:Zs:][\\u0009]]").Freeze();
            foreach (string space in blanks)
            {
                string str = "a  " + space + "  b1234";
                expect(fmt, str, n);
            }

            // Test that other whitespace characters do not work
            UnicodeSet otherWhitespace = new UnicodeSet("[[:whitespace:]]").ExceptWith(blanks).Freeze();
            foreach (string space in otherWhitespace)
            {
                string str = "a  " + space + "  b1234";
                expectParseException(fmt, str, n);
            }
        }

        /**
         * Test currencies whose display name is a ChoiceFormat.
         */
        [Test]
        public void TestComplexCurrency()
        {
            //  CLDR No Longer uses complex currency symbols.
            //  Skipping this test.
            //        Locale loc = new Locale("kn", "IN", "");
            //        NumberFormat fmt = NumberFormat.GetCurrencyInstance(loc);

            //        expect2(fmt, 1.0, "Re.\u00a01.00");
            //        expect(fmt, 1.001, "Re.\u00a01.00"); // tricky
            //        expect2(fmt, 12345678.0, "Rs.\u00a01,23,45,678.00");
            //        expect2(fmt, 0.5, "Rs.\u00a00.50");
            //        expect2(fmt, -1.0, "-Re.\u00a01.00");
            //        expect2(fmt, -10.0, "-Rs.\u00a010.00");
        }

        [Test]
        public void TestCurrencyKeyword()
        {
            UCultureInfo locale = new UCultureInfo("th_TH@currency=QQQ");
            NumberFormat format = NumberFormat.GetCurrencyInstance(locale);
            String result = format.Format(12.34f);
            if (!"QQQ 12.34".Equals(result, StringComparison.Ordinal))
            {
                Errln("got unexpected currency: " + result);
            }
        }

        private class TestNumberingSystemItem
        {
            internal readonly String localeName;
            internal readonly double value;
            internal readonly bool isRBNF;
            internal readonly String expectedResult;

            internal TestNumberingSystemItem(String loc, double val, bool rbnf, String exp)
            {
                localeName = loc;
                value = val;
                isRBNF = rbnf;
                expectedResult = exp;
            }
        }

        /**
         * Test alternate numbering systems
         */
        [Test]
        // ICU4N NOTE: This test fails when not loading the zh-TW satellite assembly. However,
        // currently debugging breaks when it is loaded, so we have it disabled in DEBUG mode.
#if DEBUG
        [Ignore("ICU4N NOTE: This test fails when not loading the zh-TW satellite assembly.")]
#endif
        public void TestNumberingSystems()
        {


            TestNumberingSystemItem[] DATA = {
                new TestNumberingSystemItem("en_US@numbers=thai", 1234.567, false, "\u0e51,\u0e52\u0e53\u0e54.\u0e55\u0e56\u0e57"),
                new TestNumberingSystemItem("en_US@numbers=thai", 1234.567, false, "\u0E51,\u0E52\u0E53\u0E54.\u0E55\u0E56\u0E57"),
                new TestNumberingSystemItem("en_US@numbers=hebr", 5678.0, true, "\u05D4\u05F3\u05EA\u05E8\u05E2\u05F4\u05D7"),
                new TestNumberingSystemItem("en_US@numbers=arabext", 1234.567, false, "\u06F1\u066c\u06F2\u06F3\u06F4\u066b\u06F5\u06F6\u06F7"),
                new TestNumberingSystemItem("de_DE@numbers=foobar", 1234.567, false, "1.234,567"),
                new TestNumberingSystemItem("ar_EG", 1234.567, false, "\u0661\u066c\u0662\u0663\u0664\u066b\u0665\u0666\u0667"),
                new TestNumberingSystemItem("th_TH@numbers=traditional", 1234.567, false, "\u0E51,\u0E52\u0E53\u0E54.\u0E55\u0E56\u0E57"), // fall back to native per TR35
                new TestNumberingSystemItem("ar_MA", 1234.567, false, "1.234,567"),
                new TestNumberingSystemItem("en_US@numbers=hanidec", 1234.567, false, "\u4e00,\u4e8c\u4e09\u56db.\u4e94\u516d\u4e03"),
                new TestNumberingSystemItem("ta_IN@numbers=native", 1234.567, false, "\u0BE7,\u0BE8\u0BE9\u0BEA.\u0BEB\u0BEC\u0BED"),
                new TestNumberingSystemItem("ta_IN@numbers=traditional", 1235.0, true, "\u0BF2\u0BE8\u0BF1\u0BE9\u0BF0\u0BEB"),
                new TestNumberingSystemItem("ta_IN@numbers=finance", 1234.567, false, "1,234.567"), // fall back to default per TR35
                new TestNumberingSystemItem("zh_TW@numbers=native", 1234.567, false, "\u4e00,\u4e8c\u4e09\u56db.\u4e94\u516d\u4e03"),
                new TestNumberingSystemItem("zh_TW@numbers=traditional", 1234.567, true, "\u4E00\u5343\u4E8C\u767E\u4E09\u5341\u56DB\u9EDE\u4E94\u516D\u4E03"),
                new TestNumberingSystemItem("zh_TW@numbers=finance", 1234.567, true, "\u58F9\u4EDF\u8CB3\u4F70\u53C3\u62FE\u8086\u9EDE\u4F0D\u9678\u67D2"),
                new TestNumberingSystemItem("en_US@numbers=mathsanb", 1234.567, false, "𝟭,𝟮𝟯𝟰.𝟱𝟲𝟳"), // ticket #13286
        };


            foreach (TestNumberingSystemItem item in DATA)
            {
                UCultureInfo loc = new UCultureInfo(item.localeName);
                NumberFormat fmt = NumberFormat.GetInstance(loc);
                if (item.isRBNF)
                {
                    expect3(fmt, item.value, item.expectedResult);
                }
                else
                {
                    expect2(fmt, item.value, item.expectedResult);
                }
            }
        }

        // Coverage tests for methods not being called otherwise.
        [Test]
        public void TestNumberingSystemCoverage()
        {
            // Test getAvaliableNames
            string[] availableNames = NumberingSystem.GetAvailableNames();
            if (availableNames == null || availableNames.Length <= 0)
            {
                Errln("ERROR: NumberingSystem.getAvailableNames() returned a null or empty array.");
            }
            else
            {
                bool latnFound = false;
                foreach (string name in availableNames)
                {
                    if ("latn".Equals(name, StringComparison.Ordinal))
                    {
                        latnFound = true;
                        break;
                    }
                }

                if (!latnFound)
                {
                    Errln("ERROR: 'latn' numbering system not found on NumberingSystem.getAvailableNames().");
                }
            }

            // Test NumberingSystem.GetInstance()
            NumberingSystem ns1 = NumberingSystem.GetInstance();
            if (ns1 == null || ns1.IsAlgorithmic)
            {
                Errln("ERROR: NumberingSystem.GetInstance() returned a null or invalid NumberingSystem");
            }

            // Test NumberingSystem.GetInstance(int,bool,String)
            /* Parameters used: the ones used in the default constructor
             * radix = 10;
             * algorithmic = false;
             * desc = "0123456789";
             */
            NumberingSystem ns2 = NumberingSystem.GetInstance(10, false, "0123456789");
            if (ns2 == null || ns2.IsAlgorithmic)
            {
                Errln("ERROR: NumberingSystem.GetInstance(int,bool,String) returned a null or invalid NumberingSystem");
            }

            // Test NumberingSystem.GetInstance(Locale)
            NumberingSystem ns3 = NumberingSystem.GetInstance(new CultureInfo("en"));
            if (ns3 == null || ns3.IsAlgorithmic)
            {
                Errln("ERROR: NumberingSystem.GetInstance(Locale) returned a null or invalid NumberingSystem");
            }
        }

        [Test]
        [Ignore("Undefined cultures (und-PH) are not supported in .NET.")]
        public void Test6816()
        {
            Currency cur1 = Currency.GetInstance(new CultureInfo("und-PH"));

            NumberFormat nfmt = NumberFormat.GetCurrencyInstance(new CultureInfo("und-PH"));
            DecimalFormatSymbols decsym = ((DecimalFormat)nfmt).GetDecimalFormatSymbols();
            Currency cur2 = decsym.Currency;

            if (!cur1.CurrencyCode.Equals("PHP", StringComparison.Ordinal) || !cur2.CurrencyCode.Equals("PHP", StringComparison.Ordinal))
            {
                Errln("FAIL: Currencies should match PHP: cur1 = " + cur1.CurrencyCode + "; cur2 = " + cur2.CurrencyCode);
            }

        }

        private class FormatTask
        {
            DecimalFormat fmt;
            StringBuffer buf;
            bool inc;
            float num;

            internal FormatTask(DecimalFormat fmt, int index)
            {
                this.fmt = fmt;
                this.buf = new StringBuffer();
                this.inc = (index & 0x1) == 0;
                this.num = inc ? 0 : 10000;
            }

            public void Run()
            {
                if (inc)
                {
                    while (num < 10000)
                    {
                        buf.Append(fmt.Format(num) + "\n");
                        num += 3.14159f;
                    }
                }
                else
                {
                    while (num > 0)
                    {
                        buf.Append(fmt.Format(num) + "\n");
                        num -= 3.14159f;
                    }
                }
            }

            public string Result => buf.ToString();
        }


        [Test]
        public void TestThreadedFormat()
        {
            DecimalFormat fmt = new DecimalFormat("0.####");
            FormatTask[] formatTasks = new FormatTask[8];
            Action[] tasks = new Action[8];
            for (int i = 0; i < tasks.Length; ++i)
            {
                formatTasks[i] = new FormatTask(fmt, i);
                tasks[i] = formatTasks[i].Run;
            }

            TestUtil.RunUntilDone(tasks);

            for (int i = 2; i < formatTasks.Length; i++)
            {
                String str1 = formatTasks[i].Result;
                String str2 = formatTasks[i - 2].Result;
                if (!str1.Equals(str2))
                {
                    Console.Out.WriteLine("mismatch at " + i);
                    Console.Out.WriteLine(str1);
                    Console.Out.WriteLine(str2);
                    Errln("decimal format thread mismatch");

                    break;
                }
                str1 = str2;
            }
        }

        [Test]
        public void TestPerMill()
        {
            DecimalFormat fmt = new DecimalFormat("###.###\u2030");
            assertEquals("0.4857 x ###.###\u2030",
                    "485.7\u2030", fmt.Format(0.4857));

            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en"));
            sym.PerMill = ('m');
            DecimalFormat fmt2 = new DecimalFormat("", sym);
            fmt2.ApplyLocalizedPattern("###.###m");
            assertEquals("0.4857 x ###.###m",
                    "485.7m", fmt2.Format(0.4857));
        }

        [Test]
        public void TestIllegalPatterns()
        {
            // Test cases:
            // Prefix with "-:" for illegal patterns
            // Prefix with "+:" for legal patterns
            string[] DATA = {
                // Unquoted special characters in the suffix are illegal
                "-:000.000|###",
                "+:000.000'|###'",
        };
            for (int i = 0; i < DATA.Length; ++i)
            {
                string pat = DATA[i];
                bool valid = pat[0] == '+';
                pat = pat.Substring(2);
                Exception e = null;
                try
                {
                    // locale doesn't matter here
                    new DecimalFormat(pat);
                }
                catch (FormatException e1) // ICU4N: Changed from ArgumentException to FormatException, since this is a parse error
                {
                    e = e1;
                }
                //catch (ArgumentException e1)
                //{
                //    e = e1;
                //}
                //catch (IndexOutOfBoundsException e1) // ICU4N TODO: Do we need this?
                //{
                //    e = e1;
                //}
                String msg = (e == null) ? "success" : e.Message;
                if ((e == null) == valid)
                {
                    Logln("Ok: pattern \"" + pat + "\": " + msg);
                }
                else
                {
                    Errln("FAIL: pattern \"" + pat + "\" should have " +
                            (valid ? "succeeded" : "failed") + "; got " + msg);
                }
            }
        }

        /**
         * Parse a CurrencyAmount using the given NumberFormat, with
         * the 'delim' character separating the number and the currency.
         */
        private static CurrencyAmount parseCurrencyAmount(String str, NumberFormat fmt,
                char delim)
        // throws ParseException
        {
            int i = str.IndexOf(delim);
            return new CurrencyAmount(fmt.Parse(str.Substring(0, i)), // ICU4N: Checked 2nd arg
                    Currency.GetInstance(str.Substring(i + 1)));
        }

        /**
         * Return an integer representing the next token from this
         * iterator.  The integer will be an index into the given list, or
         * -1 if there are no more tokens, or -2 if the token is not on
         * the list.
         */
        private static int keywordIndex(string tok)
        {
            for (int i = 0; i < KEYWORDS.Length; ++i)
            {
                if (tok.Equals(KEYWORDS[i], StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        private static readonly string[] KEYWORDS = {
    /*0*/
    "ref=", // <reference pattern to parse numbers>
        /*1*/ "loc=", // <locale for formats>
        /*2*/ "f:",   // <pattern or '-'> <number> <exp. string>
        /*3*/ "fp:",  // <pattern or '-'> <number> <exp. string> <exp. number>
        /*4*/ "rt:",  // <pattern or '-'> <(exp.) number> <(exp.) string>
        /*5*/ "p:",   // <pattern or '-'> <string> <exp. number>
        /*6*/ "perr:", // <pattern or '-'> <invalid string>
        /*7*/ "pat:", // <pattern or '-'> <exp. toPattern or '-' or 'err'>
        /*8*/ "fpc:", // <loc or '-'> <curr.amt> <exp. string> <exp. curr.amt>
        /*9*/ "strict=", // true or false
    };

        //@SuppressWarnings("resource")  // InputStream is will be closed by the ResourceReader.
        [Test]
        [Ignore("ICU4N TODO: Missing dependencies ResourceReader, TokenIterator, MeasureFormat")]
        public void TestCases()
        {
            //String caseFileName = "NumberFormatTestCases.txt";
            ////java.io.InputStream is = NumberFormatTest.class.getResourceAsStream(caseFileName);
            //Stream @is = typeof(NumberFormatTest).FindAndGetManifestResourceStream(caseFileName);

            //ResourceReader reader = new ResourceReader(@is, caseFileName, "utf-8");
            //TokenIterator tokens = new TokenIterator(reader);

            //CultureInfo loc = new CultureInfo("en-US");
            //DecimalFormat @ref = null, fmt = null;
            //MeasureFormat mfmt = null;
            //String pat = null, str = null, mloc = null;
            //bool strict = false;

            //try
            //{
            //    for (; ; )
            //    {
            //        String tok = tokens.next();
            //        if (tok == null)
            //        {
            //            break;
            //        }
            //        String where = "(" + tokens.getLineNumber() + ") ";
            //        int cmd = keywordIndex(tok);
            //        switch (cmd)
            //        {
            //            case 0:
            //                // ref= <reference pattern>
            //                @ref = new DecimalFormat(tokens.next(),
            //            new DecimalFormatSymbols(new CultureInfo("en-US")));
            //                @ref.ParseStrict = (strict);
            //                Logln("Setting reference pattern to:\t" + @ref);
            //                break;
            //            case 1:
            //                // loc= <locale>
            //                loc = LocaleUtility.GetLocaleFromName(tokens.next());
            //                pat = ((DecimalFormat)NumberFormat.GetInstance(loc)).ToPattern();
            //                Logln("Setting locale to:\t" + loc + ", \tand pattern to:\t" + pat);
            //                break;
            //            case 2: // f:
            //            case 3: // fp:
            //            case 4: // rt:
            //            case 5: // p:
            //                tok = tokens.next();
            //                if (!tok.Equals("-", StringComparison.Ordinal))
            //                {
            //                    pat = tok;
            //                }
            //                try
            //                {
            //                    fmt = new DecimalFormat(pat, new DecimalFormatSymbols(loc));
            //                    fmt.ParseStrict = (strict);
            //                }
            //                catch (ArgumentException iae)
            //                {
            //                    Errln(where + "Pattern \"" + pat + '"');
            //                    Console.WriteLine(iae.ToString());
            //                    //iae.printStackTrace();
            //                    tokens.next(); // consume remaining tokens
            //                                   //tokens.next();
            //                    if (cmd == 3) tokens.next();
            //                    continue;
            //                }
            //                str = null;
            //                try
            //                {
            //                    if (cmd == 2 || cmd == 3 || cmd == 4)
            //                    {
            //                        // f: <pattern or '-'> <number> <exp. string>
            //                        // fp: <pattern or '-'> <number> <exp. string> <exp. number>
            //                        // rt: <pattern or '-'> <number> <string>
            //                        String num = tokens.next();
            //                        str = tokens.next();
            //                        Number n = @ref.Parse(num);
            //                        assertEquals(where + '"' + pat + "\".Format(" + num + ")",
            //                                str, fmt.Format(n));
            //                        if (cmd == 3)
            //                        { // fp:
            //                            n = @ref.Parse(tokens.next());
            //                        }
            //                        if (cmd != 2)
            //                        { // != f:
            //                            assertEquals(where + '"' + pat + "\".Parse(\"" + str + "\")",
            //                                    n, fmt.Parse(str));
            //                        }
            //                    }
            //                    // p: <pattern or '-'> <string to parse> <exp. number>
            //                    else
            //                    {
            //                        str = tokens.next();
            //                        String expstr = tokens.next();
            //                        Number parsed = fmt.Parse(str);
            //                        Number exp = @ref.Parse(expstr);
            //                        assertEquals(where + '"' + pat + "\".Parse(\"" + str + "\")",
            //                                exp, parsed);
            //                    }
            //                }
            //                catch (FormatException e)
            //                {
            //                    Errln(where + '"' + pat + "\".Parse(\"" + str +
            //                            "\") threw an exception");
            //                    Console.WriteLine(e.ToString());
            //                    //e.printStackTrace();
            //                }
            //                break;
            //            case 6:
            //                // perr: <pattern or '-'> <invalid string>
            //                Errln("Under construction");
            //                return;
            //            case 7:
            //                // pat: <pattern> <exp. toPattern, or '-' or 'err'>
            //                String testpat = tokens.next();
            //                String exppat = tokens.next();
            //                bool err = exppat.Equals("err", StringComparison.Ordinal);
            //                if (testpat.Equals("-", StringComparison.Ordinal))
            //                {
            //                    if (err)
            //                    {
            //                        Errln("Invalid command \"pat: - err\" at " + tokens.describePosition());
            //                        continue;
            //                    }
            //                    testpat = pat;
            //                }
            //                if (exppat.Equals("-", StringComparison.Ordinal)) exppat = testpat;
            //                try
            //                {
            //                    DecimalFormat f = null;
            //                    if (testpat == pat)
            //                    { // [sic]
            //                        f = fmt;
            //                    }
            //                    else
            //                    {
            //                        f = new DecimalFormat(testpat);
            //                        f.ParseStrict = (strict);
            //                    }
            //                    if (err)
            //                    {
            //                        Errln(where + "Invalid pattern \"" + testpat +
            //                                "\" was accepted");
            //                    }
            //                    else
            //                    {
            //                        assertEquals(where + '"' + testpat + "\".toPattern()",
            //                                exppat, f.ToPattern());
            //                    }
            //                }
            //                catch (ArgumentException iae2)
            //                {
            //                    if (err)
            //                    {
            //                        Logln("Ok: " + where + "Invalid pattern \"" + testpat +
            //                                "\" threw an exception");
            //                    }
            //                    else
            //                    {
            //                        Errln(where + "Valid pattern \"" + testpat +
            //                                "\" threw an exception");
            //                        Console.WriteLine(iae2.ToString());
            //                        //iae2.printStackTrace();
            //                    }
            //                }
            //                break;
            //            case 8: // fpc:
            //                tok = tokens.next();
            //                if (!tok.Equals("-", StringComparison.Ordinal))
            //                {
            //                    mloc = tok;
            //                    UCultureInfo l = new UCultureInfo(mloc);
            //                    try
            //                    {
            //                        mfmt = MeasureFormat.getCurrencyFormat(l);
            //                    }
            //                    catch (ArgumentException iae)
            //                    {
            //                        Errln(where + "Loc \"" + tok + '"');
            //                        Console.WriteLine(iae.ToString());
            //                        //iae.printStackTrace();
            //                        tokens.next(); // consume remaining tokens
            //                        tokens.next();
            //                        tokens.next();
            //                        continue;
            //                    }
            //                }
            //                str = null;
            //                try
            //                {
            //                    // fpc: <loc or '-'> <curr.amt> <exp. string> <exp. curr.amt>
            //                    String currAmt = tokens.next();
            //                    str = tokens.next();
            //                    CurrencyAmount target = parseCurrencyAmount(currAmt, @ref, '/');
            //                    String formatResult = mfmt.Format(target);
            //                    assertEquals(where + "getCurrencyFormat(" + mloc + ").Format(" + currAmt + ")",
            //                            str, formatResult);
            //                    target = parseCurrencyAmount(tokens.next(), @ref, '/');
            //                    CurrencyAmount parseResult = (CurrencyAmount)mfmt.parseObject(str);
            //                    assertEquals(where + "getCurrencyFormat(" + mloc + ").Parse(\"" + str + "\")",
            //                            target, parseResult);
            //                }
            //                catch (FormatException e)
            //                {
            //                    Errln(where + '"' + pat + "\".Parse(\"" + str +
            //                            "\") threw an exception");
            //                    Console.WriteLine(e.ToString());
            //                    //e.printStackTrace();
            //                }
            //                break;
            //            case 9: // strict= true or false
            //                strict = "true".Equals(tokens.next(), StringComparison.OrdinalIgnoreCase);
            //                Logln("Setting strict to:\t" + strict);
            //                break;
            //            case -1:
            //                Errln("Unknown command \"" + tok + "\" at " + tokens.describePosition());
            //                return;
            //        }
            //    }
            //}
            //catch (System.IO.IOException e)
            //{
            //    throw;
            //    //throw new RuntimeException(e);
            //}
            //finally
            //{
            //    try
            //    {
            //        reader.Dispose();
            //    }
            //    catch (System.IO.IOException ignored)
            //    {
            //    }
            //}
        }

        [Test]
        public void TestFieldPositionDecimal()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            nf.PositivePrefix = "FOO";
            nf.PositiveSuffix = "BA";
            StringBuffer buffer = new StringBuffer();
            FieldPosition fp = new FieldPosition(NumberFormatField.DecimalSeparator);
            nf.Format(35.47, buffer, fp);
            assertEquals("35.47", "FOO35.47BA", buffer.ToString());
            assertEquals("fp begin", 5, fp.BeginIndex);
            assertEquals("fp end", 6, fp.EndIndex);
        }

        [Test]
        public void TestFieldPositionInteger()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            nf.PositivePrefix = "FOO";
            nf.PositiveSuffix = "BA";
            StringBuffer buffer = new StringBuffer();
            FieldPosition fp = new FieldPosition(NumberFormatField.Integer);
            nf.Format(35.47, buffer, fp);
            assertEquals("35.47", "FOO35.47BA", buffer.ToString());
            assertEquals("fp begin", 3, fp.BeginIndex);
            assertEquals("fp end", 5, fp.EndIndex);
        }

        [Test]
        public void TestFieldPositionFractionButInteger()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            nf.PositivePrefix = "FOO";
            nf.PositiveSuffix = "BA";
            StringBuffer buffer = new StringBuffer();
            FieldPosition fp = new FieldPosition(NumberFormatField.Fraction);
            nf.Format(35, buffer, fp);
            assertEquals("35", "FOO35BA", buffer.ToString());
            assertEquals("fp begin", 5, fp.BeginIndex);
            assertEquals("fp end", 5, fp.EndIndex);
        }

        [Test]
        public void TestFieldPositionFraction()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            nf.PositivePrefix = "FOO";
            nf.PositiveSuffix = "BA";
            StringBuffer buffer = new StringBuffer();
            FieldPosition fp = new FieldPosition(NumberFormatField.Fraction);
            nf.Format(35.47, buffer, fp);
            assertEquals("35.47", "FOO35.47BA", buffer.ToString());
            assertEquals("fp begin", 6, fp.BeginIndex);
            assertEquals("fp end", 8, fp.EndIndex);
        }

        [Test]
        public void TestFieldPositionCurrency()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetCurrencyInstance(new CultureInfo("en-US"));
            double amount = 35.47;
            double negAmount = -34.567;
            FieldPosition cp = new FieldPosition(NumberFormatField.Currency);

            StringBuffer buffer0 = new StringBuffer();
            nf.Format(amount, buffer0, cp);
            assertEquals("$35.47", "$35.47", buffer0.ToString());
            assertEquals("cp begin", 0, cp.BeginIndex);
            assertEquals("cp end", 1, cp.EndIndex);

            StringBuffer buffer01 = new StringBuffer();
            nf.Format(negAmount, buffer01, cp);
            assertEquals("-$34.57", "-$34.57", buffer01.ToString());
            assertEquals("cp begin", 1, cp.BeginIndex);
            assertEquals("cp end", 2, cp.EndIndex);

            nf.Currency = Currency.GetInstance(new CultureInfo("fr-FR"));
            StringBuffer buffer1 = new StringBuffer();
            nf.Format(amount, buffer1, cp);
            assertEquals("€35.47", "€35.47", buffer1.ToString());
            assertEquals("cp begin", 0, cp.BeginIndex);
            assertEquals("cp end", 1, cp.EndIndex);

            nf.Currency = Currency.GetInstance(new CultureInfo("fr-ch"));
            StringBuffer buffer2 = new StringBuffer();
            nf.Format(amount, buffer2, cp);
            assertEquals("CHF 35.47", "CHF 35.47", buffer2.ToString());
            assertEquals("cp begin", 0, cp.BeginIndex);
            assertEquals("cp end", 3, cp.EndIndex);

            StringBuffer buffer20 = new StringBuffer();
            nf.Format(negAmount, buffer20, cp);
            assertEquals("-CHF 34.57", "-CHF 34.57", buffer20.ToString());
            assertEquals("cp begin", 1, cp.BeginIndex);
            assertEquals("cp end", 4, cp.EndIndex);

            nf = (DecimalFormat)NumberFormat.GetCurrencyInstance(new CultureInfo("fr-FR"));
            StringBuffer buffer3 = new StringBuffer();
            nf.Format(amount, buffer3, cp);
            assertEquals("35,47 €", "35,47 €", buffer3.ToString());
            assertEquals("cp begin", 6, cp.BeginIndex);
            assertEquals("cp end", 7, cp.EndIndex);

            StringBuffer buffer4 = new StringBuffer();
            nf.Format(negAmount, buffer4, cp);
            assertEquals("-34,57 €", "-34,57 €", buffer4.ToString());
            assertEquals("cp begin", 7, cp.BeginIndex);
            assertEquals("cp end", 8, cp.EndIndex);

            nf.Currency = Currency.GetInstance(new CultureInfo("fr-ch"));
            StringBuffer buffer5 = new StringBuffer();
            nf.Format(negAmount, buffer5, cp);
            assertEquals("-34,57 CHF", "-34,57 CHF", buffer5.ToString());
            assertEquals("cp begin", 7, cp.BeginIndex);
            assertEquals("cp end", 10, cp.EndIndex);

            NumberFormat plCurrencyFmt = NumberFormat.GetInstance(new CultureInfo("fr-ch"), NumberFormatStyle.PluralCurrencyStyle);
            StringBuffer buffer6 = new StringBuffer();
            plCurrencyFmt.Format(negAmount, buffer6, cp);
            assertEquals("-34.57 francs suisses", "-34.57 francs suisses", buffer6.ToString());
            assertEquals("cp begin", 7, cp.BeginIndex);
            assertEquals("cp end", 21, cp.EndIndex);

            // Positive value with PLURALCURRENCYSTYLE.
            plCurrencyFmt = NumberFormat.GetInstance(new CultureInfo("ja-ch"), NumberFormatStyle.PluralCurrencyStyle);
            StringBuffer buffer7 = new StringBuffer();
            plCurrencyFmt.Format(amount, buffer7, cp);
            assertEquals("35.47 スイス フラン", "35.47 スイス フラン", buffer7.ToString());
            assertEquals("cp begin", 6, cp.BeginIndex);
            assertEquals("cp end", 13, cp.EndIndex);

            // PLURALCURRENCYSTYLE for non-ASCII.
            plCurrencyFmt = NumberFormat.GetInstance(new CultureInfo("ja-de"), NumberFormatStyle.PluralCurrencyStyle);
            StringBuffer buffer8 = new StringBuffer();
            plCurrencyFmt.Format(negAmount, buffer8, cp);
            assertEquals("-34.57 ユーロ", "-34.57 ユーロ", buffer8.ToString());
            assertEquals("cp begin", 7, cp.BeginIndex);
            assertEquals("cp end", 10, cp.EndIndex);

            nf = (DecimalFormat)NumberFormat.GetCurrencyInstance(new CultureInfo("ja-JP"));
            nf.Currency = Currency.GetInstance(new CultureInfo("ja-jp"));
            StringBuffer buffer9 = new StringBuffer();
            nf.Format(negAmount, buffer9, cp);
            assertEquals("-￥35", "-￥35", buffer9.ToString());
            assertEquals("cp begin", 1, cp.BeginIndex);
            assertEquals("cp end", 2, cp.EndIndex);

            // Negative value with PLURALCURRENCYSTYLE.
            plCurrencyFmt = NumberFormat.GetInstance(new CultureInfo("ja-ch"), NumberFormatStyle.PluralCurrencyStyle);
            StringBuffer buffer10 = new StringBuffer();
            plCurrencyFmt.Format(negAmount, buffer10, cp);
            assertEquals("-34.57 スイス フラン", "-34.57 スイス フラン", buffer10.ToString());
            assertEquals("cp begin", 7, cp.BeginIndex);
            assertEquals("cp end", 14, cp.EndIndex);

            // Nagative value with PLURALCURRENCYSTYLE, Arabic digits.
            nf = (DecimalFormat)NumberFormat.GetCurrencyInstance(new CultureInfo("ar-eg"));
            plCurrencyFmt = NumberFormat.GetInstance(new CultureInfo("ar-eg"), NumberFormatStyle.PluralCurrencyStyle);
            StringBuffer buffer11 = new StringBuffer();
            plCurrencyFmt.Format(negAmount, buffer11, cp);
            assertEquals("؜-٣٤٫٥٧ جنيه مصري", "؜-٣٤٫٥٧ جنيه مصري", buffer11.ToString());
            assertEquals("cp begin", 8, cp.BeginIndex);
            assertEquals("cp end", 17, cp.EndIndex);
        }

        [Test]
        public void TestRounding()
        {
            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            if (false)
            { // for debugging specific value
                nf.RoundingMode = Numerics.BigMath.RoundingMode.HalfUp;// (BigDecimal.ROUND_HALF_UP);
                checkRounding(nf, BigDecimal.Parse("300.0300000000", CultureInfo.InvariantCulture), 0, BigDecimal.Parse("0.020000000", CultureInfo.InvariantCulture));
            }
            // full tests
            int[] roundingIncrements = { 1, 2, 5, 20, 50, 100 };
            int[] testValues = { 0, 300 };
            for (int j = 0; j < testValues.Length; ++j)
            {
                for (int mode = (int)Numerics.BigMath.RoundingMode.Up; mode < (int)Numerics.BigMath.RoundingMode.HalfEven; ++mode) // ICU4N TODO: This will need to change if the RoundingMode values change
                {
                    nf.RoundingMode = (Numerics.BigMath.RoundingMode)(mode);
                    for (int increment = 0; increment < roundingIncrements.Length; ++increment)
                    {
                        BigDecimal @base = new BigDecimal(testValues[j]);
                        BigDecimal rInc = new BigDecimal(roundingIncrements[increment]);
                        checkRounding(nf, @base, 20, rInc);
                        rInc = BigDecimal.Parse("1.000000000", CultureInfo.InvariantCulture) / (rInc);
                        checkRounding(nf, @base, 20, rInc);
                    }
                }
            }
        }

        private class TestRoundingPatternItem
        {
            internal String pattern;
            internal BigDecimal roundingIncrement;
            internal double testCase;
            internal String expected;

            internal TestRoundingPatternItem(String pattern, BigDecimal roundingIncrement, double testCase, String expected)
            {
                this.pattern = pattern;
                this.roundingIncrement = roundingIncrement;
                this.testCase = testCase;
                this.expected = expected;
            }
        }

        [Test]
        public void TestRoundingPattern()
        {


            TestRoundingPatternItem[] tests = {
                new TestRoundingPatternItem("##0.65", BigDecimal.Parse("0.65", CultureInfo.InvariantCulture), 1.234, "1.30"),
                new TestRoundingPatternItem("#50", BigDecimal.Parse("50", CultureInfo.InvariantCulture), 1230, "1250")
        };

            DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en"));
            String result;
            for (int i = 0; i < tests.Length; i++)
            {
                df.ApplyPattern(tests[i].pattern);

                result = df.Format(tests[i].testCase);

                if (!tests[i].expected.Equals(result, StringComparison.Ordinal))
                {
                    Errln("String Pattern Rounding Test Failed: Pattern: \"" + tests[i].pattern + "\" Number: " + tests[i].testCase + " - Got: " + result + " Expected: " + tests[i].expected);
                }

                df.SetRoundingIncrement(tests[i].roundingIncrement);

                result = df.Format(tests[i].testCase);

                if (!tests[i].expected.Equals(result, StringComparison.Ordinal))
                {
                    Errln("BigDecimal Rounding Test Failed: Pattern: \"" + tests[i].pattern + "\" Number: " + tests[i].testCase + " - Got: " + result + " Expected: " + tests[i].expected);
                }
            }
        }

        [Test]
        public void TestBigDecimalRounding()
        {
            String figure = "50.000000004";
            Double dbl = Double.Parse(figure, CultureInfo.InvariantCulture);
            BigDecimal dec = BigDecimal.Parse(figure, CultureInfo.InvariantCulture);

            DecimalFormat f = (DecimalFormat)NumberFormat.GetInstance();
            f.ApplyPattern("00.00######");

            assertEquals("double format", "50.00", f.Format(dbl));
            assertEquals("bigdec format", "50.00", f.Format(dec));

            int maxFracDigits = f.MaximumFractionDigits;
            BigDecimal roundingIncrement = BigDecimal.Parse("1", CultureInfo.InvariantCulture).MovePointLeft(maxFracDigits);

            f.SetRoundingIncrement(roundingIncrement);
            f.RoundingMode = Numerics.BigMath.RoundingMode.Down; // (BigDecimal.ROUND_DOWN);
            assertEquals("Rounding down", f.Format(dbl), f.Format(dec));

            f.SetRoundingIncrement(roundingIncrement);
            f.RoundingMode = Numerics.BigMath.RoundingMode.HalfUp; //(BigDecimal.ROUND_HALF_UP);
            assertEquals("Rounding half up", f.Format(dbl), f.Format(dec));
        }

        void checkRounding(DecimalFormat nf, BigDecimal @base, int iterations, BigDecimal increment)
        {
            nf.SetRoundingIncrement(increment.ToBigDecimal());
            BigDecimal lastParsed = new BigDecimal(int.MinValue); // used to make sure that rounding is monotonic
            for (int i = -iterations; i <= iterations; ++i)
            {
                BigDecimal iValue = @base.Add(increment.Multiply(new BigDecimal(i)).MovePointLeft(1));
                BigDecimal smallIncrement = BigDecimal.Parse("0.00000001", CultureInfo.InvariantCulture);
                if (iValue.Sign != 0)
                {
                    smallIncrement.Multiply(iValue); // scale unless zero
                }
                // we not only test the value, but some values in a small range around it.
                lastParsed = checkRound(nf, iValue - (smallIncrement), lastParsed);
                lastParsed = checkRound(nf, iValue, lastParsed);
                lastParsed = checkRound(nf, iValue + (smallIncrement), lastParsed);
            }
        }

        private BigDecimal checkRound(DecimalFormat nf, BigDecimal iValue, BigDecimal lastParsed)
        {
            String formatedBigDecimal = nf.Format(iValue);
            String formattedDouble = nf.Format(iValue.ToDouble());
            if (!equalButForTrailingZeros(formatedBigDecimal, formattedDouble))
            {

                Errln("Failure at: " + iValue + " (" + iValue.ToDouble() + ")"
                        + ",\tRounding-mode: " + roundingModeNames[(int)nf.RoundingMode]
                                + ",\tRounding-increment: " + nf.RoundingIncrement
                                + ",\tdouble: " + formattedDouble
                                + ",\tBigDecimal: " + formatedBigDecimal);

            }
            else
            {
                Logln("Value: " + iValue
                        + ",\tRounding-mode: " + roundingModeNames[(int)nf.RoundingMode]
                                + ",\tRounding-increment: " + nf.RoundingIncrement
                                + ",\tdouble: " + formattedDouble
                                + ",\tBigDecimal: " + formatedBigDecimal);
            }
            try
            {
                // Number should have compareTo(...)
                BigDecimal parsed = toBigDecimal(nf.Parse(formatedBigDecimal));
                if (lastParsed.CompareTo(parsed) > 0)
                {
                    Errln("Rounding wrong direction!: " + lastParsed + " > " + parsed);
                }
                lastParsed = parsed;
            }
            catch (FormatException e)
            {
                Errln("Parse Failure with: " + formatedBigDecimal);
            }
            return lastParsed;
        }

        static BigDecimal toBigDecimal(Number number)
        {
            return number is BigDecimal bigDecimal ? bigDecimal
                        : number is BigInteger bigInteger ? new BigDecimal(bigInteger)
                : number is ICU4N.Numerics.BigMath.BigDecimal bigDecimal2 ? new BigDecimal(bigDecimal2)
                        : number is Double ? new BigDecimal(number.ToDouble())
                : number is Float ? new BigDecimal(number.ToSingle())
                        : new BigDecimal(number.ToInt64());
        }

        static string[] roundingModeNames = {
        "ROUND_UP", "ROUND_DOWN", "ROUND_CEILING", "ROUND_FLOOR",
        "ROUND_HALF_UP", "ROUND_HALF_DOWN", "ROUND_HALF_EVEN",
        "ROUND_UNNECESSARY"
    };

        private static bool equalButForTrailingZeros(string formatted1, string formatted2)
        {
            if (formatted1.Length == formatted2.Length) return formatted1.Equals(formatted2, StringComparison.Ordinal);
            return stripFinalZeros(formatted1).Equals(stripFinalZeros(formatted2), StringComparison.Ordinal);
        }

        private static string stripFinalZeros(string formatted)
        {
            int len1 = formatted.Length;
            char ch;
            while (len1 > 0 && ((ch = formatted[len1 - 1]) == '0' || ch == '.')) --len1;
            if (len1 == 1 && ((ch = formatted[len1 - 1]) == '-')) --len1;
            return formatted.Substring(0, len1); // ICU4N: Checked 2nd arg
        }

        //------------------------------------------------------------------
        // Support methods
        //------------------------------------------------------------------

        /** Format-Parse test */
        internal void expect2(NumberFormat fmt, Number n, string exp)
        {
            // Don't round-trip format test, since we explicitly do it
            expect(fmt, n, exp, false);
            expect(fmt, exp, n);
        }
        /** Format-Parse test */
        internal void expect3(NumberFormat fmt, Number n, String exp)
        {
            // Don't round-trip format test, since we explicitly do it
            expect_rbnf(fmt, n, exp, false);
            expect_rbnf(fmt, exp, n);
        }

        /** Format-Parse test (convenience) */
        internal void expect2(NumberFormat fmt, double n, String exp)
        {
            expect2(fmt, (Number)Double.GetInstance(n), exp);
        }
        /** RBNF Format-Parse test (convenience) */
        internal void expect3(NumberFormat fmt, double n, String exp)
        {
            expect3(fmt, (Number)Double.GetInstance(n), exp);
        }

        /** Format-Parse test (convenience) */
        internal void expect2(NumberFormat fmt, long n, String exp)
        {
            expect2(fmt, (Number)Long.GetInstance(n), exp);
        }
        /** RBNF Format-Parse test (convenience) */
        internal void expect3(NumberFormat fmt, long n, String exp)
        {
            expect3(fmt, (Number)Long.GetInstance(n), exp);
        }

        /** Format test */
        internal void expect(NumberFormat fmt, Number n, String exp, bool rt)
        {
            StringBuffer saw = new StringBuffer();
            FieldPosition pos = new FieldPosition(0);
            fmt.Format(n, saw, pos);
            String pat = ((DecimalFormat)fmt).ToPattern();
            if (saw.ToString().Equals(exp, StringComparison.Ordinal))
            {
                Logln("Ok   " + n + " x " +
                        pat + " = \"" +
                        saw + "\"");
                // We should be able to round-trip the formatted string =>
                // number => string (but not the other way around: number
                // => string => number2, might have number2 != number):
                if (rt)
                {
                    try
                    {
                        Number n2 = fmt.Parse(exp);
                        StringBuffer saw2 = new StringBuffer();
                        fmt.Format(n2, saw2, pos);
                        if (!saw2.ToString().Equals(exp, StringComparison.Ordinal))
                        {
                            Errln("expect() format test rt, locale " + fmt.ValidCulture +
                                    ", FAIL \"" + exp + "\" => " + n2 + " => \"" + saw2 + '"');
                        }
                    }
                    catch (FormatException e)
                    {
                        Errln("expect() format test rt, locale " + fmt.ValidCulture +
                                ", " + e.Message);
                        return;
                    }
                }
            }
            else
            {
                Errln("expect() format test, locale " + fmt.ValidCulture +
                        ", FAIL " + n + " x " + pat + " = \"" + saw + "\", expected \"" + exp + "\"");
            }
        }
        /** RBNF format test */
        internal void expect_rbnf(NumberFormat fmt, Number n, string exp, bool rt)
        {
            StringBuffer saw = new StringBuffer();
            FieldPosition pos = new FieldPosition(0);
            fmt.Format(n, saw, pos);
            if (saw.ToString().Equals(exp, StringComparison.Ordinal))
            {
                Logln("Ok   " + n + " = \"" +
                        saw + "\"");
                // We should be able to round-trip the formatted string =>
                // number => string (but not the other way around: number
                // => string => number2, might have number2 != number):
                if (rt)
                {
                    try
                    {
                        Number n2 = fmt.Parse(exp);
                        StringBuffer saw2 = new StringBuffer();
                        fmt.Format(n2, saw2, pos);
                        if (!saw2.ToString().Equals(exp, StringComparison.Ordinal))
                        {
                            Errln("expect_rbnf() format test rt, locale " + fmt.ValidCulture +
                                    ", FAIL \"" + exp + "\" => " + n2 + " => \"" + saw2 + '"');
                        }
                    }
                    catch (FormatException e)
                    {
                        Errln("expect_rbnf() format test rt, locale " + fmt.ValidCulture +
                                ", " + e.Message);
                        return;
                    }
                }
            }
            else
            {
                Errln("expect_rbnf() format test, locale " + fmt.ValidCulture +
                        ", FAIL " + n + " = \"" + saw + "\", expected \"" + exp + "\"");
            }
        }

        /** Format test (convenience) */
        internal void expect(NumberFormat fmt, Number n, string exp)
        {
            expect(fmt, n, exp, true);
        }

        /** Format test (convenience) */
        internal void expect(NumberFormat fmt, double n, string exp)
        {
            expect(fmt, (Number)Double.GetInstance(n), exp);
        }

        /** Format test (convenience) */
        internal void expect(NumberFormat fmt, long n, string exp)
        {
            expect(fmt, (Number)Long.GetInstance(n), exp);
        }

        /** Parse test */
        internal void expect(NumberFormat fmt, string str, Number n)
        {
            Number num = null;
            try
            {
                num = fmt.Parse(str);
            }
            catch (FormatException e)
            {
                Errln(e.Message);
                return;
            }
            string pat = ((DecimalFormat)fmt).ToPattern();
            // A little tricky here -- make sure Double(12345.0) and
            // Long(12345) match.
            if (num.Equals(n) || num.ToDouble() == n.ToDouble())
            {
                Logln("Ok   \"" + str + "\" x " +
                        pat + " = " +
                        num);
            }
            else
            {
                Errln("expect() parse test, locale " + fmt.ValidCulture +
                        ", FAIL \"" + str + "\" x " + pat + " = " + num + ", expected " + n);
            }
        }

        /** RBNF Parse test */
        internal void expect_rbnf(NumberFormat fmt, string str, Number n)
        {
            Number num = null;
            try
            {
                num = fmt.Parse(str);
            }
            catch (FormatException e)
            {
                Errln(e.Message);
                return;
            }
            // A little tricky here -- make sure Double(12345.0) and
            // Long(12345) match.
            if (num.Equals(n) || num.ToDouble() == n.ToDouble())
            {
                Logln("Ok   \"" + str + " = " +
                        num);
            }
            else
            {
                Errln("expect_rbnf() parse test, locale " + fmt.ValidCulture +
                        ", FAIL \"" + str + " = " + num + ", expected " + n);
            }
        }

        /** Parse test (convenience) */
        internal void expect(NumberFormat fmt, string str, double n)
        {
            expect(fmt, str, (Number)Double.GetInstance(n));
        }

        /** Parse test (convenience) */
        internal void expect(NumberFormat fmt, string str, long n)
        {
            expect(fmt, str, (Number)Long.GetInstance(n));
        }

        /** Parse test (convenience) */
        internal void expectParseException(DecimalFormat fmt, string str, double n)
        {
            expectParseException(fmt, str, (Number)Double.GetInstance(n));
        }

        /** Parse test (convenience) */
        internal void expectParseException(DecimalFormat fmt, string str, long n)
        {
            expectParseException(fmt, str, (Number)Long.GetInstance(n));
        }


        /** Parse test */
        internal void expectParseException(DecimalFormat fmt, string str, Number n)
        {
            Number num = null;
            try
            {
                num = fmt.Parse(str);
                Errln("Expected failure, but passed: " + n + " on " + fmt.ToPattern() + " -> " + num);
            }
            catch (FormatException e)
            {
            }
        }

        private void expectCurrency(NumberFormat nf, Currency curr,
                double value, string @string)
        {
            DecimalFormat fmt = (DecimalFormat)nf;
            if (curr != null)
            {
                fmt.Currency = (curr);
            }
            string s = fmt.Format(value).Replace('\u00A0', ' ');

            if (s.Equals(@string))
            {
                Logln("Ok: " + value + " x " + curr + " => " + s);
            }
            else
            {
                Errln("FAIL: " + value + " x " + curr + " => " + s +
                        ", expected " + @string);
            }
        }

        internal void expectPad(DecimalFormat fmt, string pat, PadPosition pos)
        {
            expectPad(fmt, pat, pos, 0, (char)0);
        }

        internal void expectPad(DecimalFormat fmt, string pat, PadPosition pos, int width, char pad)
        {
            PadPosition apos = (PadPosition)0;
            int awidth = 0;
            char apadStr;
            try
            {
                fmt.ApplyPattern(pat);
                apos = fmt.PadPosition;
                awidth = fmt.FormatWidth;
                apadStr = fmt.PadCharacter;
            }
            catch (Exception e)
            {
                apos = (PadPosition)(-1);
                awidth = width;
                apadStr = pad;
            }

            if (apos == pos && awidth == width && apadStr == pad)
            {
                Logln("Ok   \"" + pat + "\" pos="
                        + apos + (((int)pos == -1) ? "" : " width=" + awidth + " pad=" + apadStr));
            }
            else
            {
                Errln("FAIL \"" + pat + "\" pos=" + apos + " width="
                        + awidth + " pad=" + apadStr + ", expected "
                        + pos + " " + width + " " + pad);
            }
        }

        internal void expectPat(DecimalFormat fmt, string exp)
        {
            string pat = fmt.ToPattern();
            if (pat.Equals(exp, StringComparison.Ordinal))
            {
                Logln("Ok   \"" + pat + "\"");
            }
            else
            {
                Errln("FAIL \"" + pat + "\", expected \"" + exp + "\"");
            }
        }


        private void expectParseCurrency(NumberFormat fmt, Currency expected, string text)
        {
            ParsePosition pos = new ParsePosition(0);
            CurrencyAmount currencyAmount = fmt.ParseCurrency(text, pos);
            assertTrue("Parse of " + text + " should have succeeded.", pos.Index > 0);
            assertEquals("Currency should be correct.", expected, currencyAmount.Currency);
        }

        [Test]
        public void TestJB3832()
        {
            UCultureInfo locale = new UCultureInfo("pt_PT@currency=PTE");
            NumberFormat format = NumberFormat.GetCurrencyInstance(locale);
            Currency curr = Currency.GetInstance(locale);
            Logln("\nName of the currency is: " + curr.GetName(locale, CurrencyNameStyle.LongName, out bool _));
            CurrencyAmount cAmt = new CurrencyAmount(1150.50, curr);
            Logln("CurrencyAmount object's hashCode is: " + cAmt.GetHashCode()); //cover hashCode
            String str = format.Format(cAmt);
            String expected = "1,150$50\u00a0\u200b";
            if (!expected.Equals(str, StringComparison.Ordinal))
            {
                Errln("Did not get the expected output Expected: " + expected + " Got: " + str);
            }
        }

        [Test]
        public void TestScientificWithGrouping()
        {
            // Grouping separators are not allowed in the pattern, but we can enable them via the API.
            DecimalFormat df = new DecimalFormat("###0.000E0");
            df.IsGroupingUsed = (true);
            expect2(df, 123, "123.0E0");
            expect2(df, 1234, "1,234E0");
            expect2(df, 12340, "1.234E4");
        }

        [Test]
        public void TestStrictParse()
        {
            string[] pass = {
                "0",           // single zero before end of text is not leading
                "0 ",          // single zero at end of number is not leading
                "0.",          // single zero before period (or decimal, it's ambiguous) is not leading
                "0,",          // single zero before comma (not group separator) is not leading
                "0.0",         // single zero before decimal followed by digit is not leading
                "0. ",         // same as above before period (or decimal) is not leading
                "0.100,5",     // comma stops parse of decimal (no grouping)
                ".00",         // leading decimal is ok, even with zeros
                "1234567",     // group separators are not required
                "12345, ",     // comma not followed by digit is not a group separator, but end of number
                "1,234, ",     // if group separator is present, group sizes must be appropriate
                "1,234,567",   // ...secondary too
                "0E",          // an exponent not followed by zero or digits is not an exponent
                "00",          // leading zero before zero - used to be error - see ticket #7913
                "012",         // leading zero before digit - used to be error - see ticket #7913
                "0,456",       // leading zero before group separator - used to be error - see ticket #7913
                "999,999",     // see ticket #6863
                "-99,999",     // see ticket #6863
                "-999,999",    // see ticket #6863
                "-9,999,999",  // see ticket #6863
        };
            string[] fail = {
                "1,2",       // wrong number of digits after group separator
                ",0",        // leading group separator before zero
                ",1",        // leading group separator before digit
                ",.02",      // leading group separator before decimal
                "1,.02",     // group separator before decimal
                "1,,200",    // multiple group separators
                "1,45",      // wrong number of digits in primary group
                "1,45 that", // wrong number of digits in primary group
                "1,45.34",   // wrong number of digits in primary group
                "1234,567",  // wrong number of digits in secondary group
                "12,34,567", // wrong number of digits in secondary group
                "1,23,456,7890", // wrong number of digits in primary and secondary groups
        };

            DecimalFormat nf = (DecimalFormat)NumberFormat.GetInstance(new CultureInfo("en"));
            runStrictParseBatch(nf, pass, fail);

            string[] scientificPass = {
                "0E2",      // single zero before exponent is ok
                "1234E2",   // any number of digits before exponent is ok
                "1,234E",   // an exponent string not followed by zero or digits is not an exponent
                "00E2",     // leading zeroes now allowed in strict mode - see ticket #
        };
            string[] scientificFail = {
        };

            nf = (DecimalFormat)NumberFormat.GetInstance(new CultureInfo("en"));
            runStrictParseBatch(nf, scientificPass, scientificFail);

            string[] mixedPass = {
                "12,34,567",
                "12,34,567,",
                "12,34,567, that",
                "12,34,567 that",
        };
            string[] mixedFail = {
                "12,34,56",
                "12,34,56,",
                "12,34,56, that ",
                "12,34,56 that",
        };

            nf = new DecimalFormat("#,##,##0.#");
            runStrictParseBatch(nf, mixedPass, mixedFail);
        }

        void runStrictParseBatch(DecimalFormat nf, string[] pass, string[] fail)
        {
            nf.ParseStrict = (false);
            runStrictParseTests("should pass", nf, pass, true);
            runStrictParseTests("should also pass", nf, fail, true);
            nf.ParseStrict = (true);
            runStrictParseTests("should still pass", nf, pass, true);
            runStrictParseTests("should fail", nf, fail, false);
        }

        void runStrictParseTests(string msg, DecimalFormat nf, string[] tests, bool pass)
        {
            Logln("");
            Logln("pattern: '" + nf.ToPattern() + "'");
            Logln(msg);
            for (int i = 0; i < tests.Length; ++i)
            {
                string str = tests[i];
                ParsePosition pp = new ParsePosition(0);
                Number n = nf.Parse(str, pp);
                string formatted = n != null ? nf.Format(n) : "null";
                string err = pp.ErrorIndex == -1 ? "" : "(error at " + pp.ErrorIndex + ")";
                if ((err.Length == 0) != pass)
                {
                    Errln("'" + str + "' parsed '" +
                            str.Substring(0, pp.Index) + // ICU4N: Checked 2nd arg
                            "' returned " + n + " formats to '" +
                            formatted + "' " + err);
                }
                else
                {
                    if (err.Length > 0)
                    {
                        err = "got expected " + err;
                    }
                    Logln("'" + str + "' parsed '" +
                            str.Substring(0, pp.Index) + // ICU4N: Checked 2nd arg
                            "' returned " + n + " formats to '" +
                            formatted + "' " + err);
                }
            }
        }

        [Test]
        public void TestJB5251()
        {
            //save default locale
            UCultureInfo defaultLocale = UCultureInfo.CurrentCulture;
            UCultureInfo.CurrentCulture = new UCultureInfo("qr_QR");
            try
            {
                NumberFormat.GetInstance();
            }
            catch (Exception e)
            {
                Errln("Numberformat threw exception for non-existent locale. It should use the default.");
            }
            //reset default locale
            UCultureInfo.CurrentCulture = (defaultLocale);
        }

        [Test]
        public void TestParseReturnType()
        {
            string[] defaultLong = {
                "123",
                "123.0",
                "0.0",
                "-9223372036854775808", // Min Long
                "9223372036854775807" // Max Long
        };

            string[] defaultNonLong = {
                "12345678901234567890",
                "9223372036854775808",
                "-9223372036854775809"
        };

            string[] doubles = {
                "-0.0",
                "NaN",
                "\u221E"    // Infinity
        };

            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat nf = new DecimalFormat("#.#", sym);

            if (nf.ParseToBigDecimal)
            {
                Errln("FAIL: isParseDecimal() must return false by default");
            }

            // isParseBigDecimal() is false
            for (int i = 0; i < defaultLong.Length; i++)
            {
                try
                {
                    Number n = nf.Parse(defaultLong[i]);
                    if (!(n is Long))
                    {
                        Errln("FAIL: parse does not return Long instance");
                    }
                }
                catch (FormatException e)
                {
                    Errln("parse of '" + defaultLong[i] + "' threw exception: " + e);
                }
            }
            for (int i = 0; i < defaultNonLong.Length; i++)
            {
                try
                {
                    Number n = nf.Parse(defaultNonLong[i]);
                    // For backwards compatibility with this test, BigDecimal is checked.
                    if ((n is Long) || (n is BigDecimal))
                    {
                        Errln("FAIL: parse returned a Long or a BigDecimal");
                    }
                }
                catch (FormatException e)
                {
                    Errln("parse of '" + defaultNonLong[i] + "' threw exception: " + e);
                }
            }
            // parse results for doubls must be always Double
            for (int i = 0; i < doubles.Length; i++)
            {
                try
                {
                    Number n = nf.Parse(doubles[i]);
                    if (!(n is Double))
                    {
                        Errln("FAIL: parse does not return Double instance");
                    }
                }
                catch (FormatException e)
                {
                    Errln("parse of '" + doubles[i] + "' threw exception: " + e);
                }
            }

            // force this DecimalFormat to return BigDecimal
            nf.ParseToBigDecimal = true;// setParseBigDecimal(true);
            if (!nf.ParseToBigDecimal)
            {
                Errln("FAIL: isParseBigDecimal() must return true");
            }

            // isParseBigDecimal() is true
            for (int i = 0; i < defaultLong.Length + defaultNonLong.Length; i++)
            {
                String input = (i < defaultLong.Length) ? defaultLong[i] : defaultNonLong[i - defaultLong.Length];
                try
                {
                    Number n = nf.Parse(input);
                    if (!(n is BigDecimal))
                    {
                        Errln("FAIL: parse does not return BigDecimal instance");
                    }
                }
                catch (FormatException e)
                {
                    Errln("parse of '" + input + "' threw exception: " + e);
                }
            }
            // parse results for doubls must be always Double
            for (int i = 0; i < doubles.Length; i++)
            {
                try
                {
                    Number n = nf.Parse(doubles[i]);
                    if (!(n is Double))
                    {
                        Errln("FAIL: parse does not return Double instance");
                    }
                }
                catch (FormatException e)
                {
                    Errln("parse of '" + doubles[i] + "' threw exception: " + e);
                }
            }
        }

        [Test]
        public void TestNonpositiveMultiplier()
        {
            DecimalFormat df = new DecimalFormat("0");

            // test zero multiplier

            try
            {
                df.Multiplier = (0);

                // bad
                Errln("DecimalFormat.setMultiplier(0) did not throw an IllegalArgumentException");
            }
            catch (ArgumentException ex)
            {
                // good
            }

            // test negative multiplier

            try
            {
                df.Multiplier = (-1);

                if (df.Multiplier != -1)
                {
                    Errln("DecimalFormat.setMultiplier(-1) did not change the multiplier to -1");
                    return;
                }

                // good
            }
            catch (ArgumentException ex)
            {
                // bad
                Errln("DecimalFormat.setMultiplier(-1) threw an IllegalArgumentException");
                return;
            }

            expect(df, "1122.123", -1122.123);
            expect(df, "-1122.123", 1122.123);
            expect(df, "1.2", -1.2);
            expect(df, "-1.2", 1.2);

            expect2(df, long.MaxValue, (-BigInteger.GetInstance(long.MaxValue)).ToString(CultureInfo.InvariantCulture));
            expect2(df, long.MinValue, (-BigInteger.GetInstance(long.MinValue)).ToString(CultureInfo.InvariantCulture));
            expect2(df, long.MaxValue / 2, (-BigInteger.GetInstance(long.MaxValue / 2)).ToString(CultureInfo.InvariantCulture));
            expect2(df, long.MinValue / 2, (-BigInteger.GetInstance(long.MinValue / 2)).ToString(CultureInfo.InvariantCulture));

            expect2(df, BigDecimal.GetInstance(long.MaxValue), (-BigDecimal.GetInstance(long.MaxValue)).ToString(CultureInfo.InvariantCulture));
            expect2(df, BigDecimal.GetInstance(long.MinValue), (-BigDecimal.GetInstance(long.MinValue)).ToString(CultureInfo.InvariantCulture));

            expect2(df, ICU4N.Numerics.BigMath.BigDecimal.GetInstance(long.MaxValue), (-ICU4N.Numerics.BigMath.BigDecimal.GetInstance(long.MaxValue)).ToString(CultureInfo.InvariantCulture));
            expect2(df, ICU4N.Numerics.BigMath.BigDecimal.GetInstance(long.MinValue), (-ICU4N.Numerics.BigMath.BigDecimal.GetInstance(long.MinValue)).ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestJB5358()
        {
            int numThreads = 10;
            String numstr = "12345";
            double expected = 12345;
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat fmt = new DecimalFormat("#.#", sym);
            IList<string> errors = new List<string>();

            ParseThreadJB5358[] threads = new ParseThreadJB5358[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                threads[i] = new ParseThreadJB5358((DecimalFormat)fmt.Clone(), numstr, expected, errors);
                threads[i].Start();
            }
            for (int i = 0; i < numThreads; i++)
            {
                try
                {
                    threads[i].Join();
                }
                catch (ThreadInterruptedException ie)
                {
                    //ie.printStackTrace();
                    Console.Out.WriteLine(ie.ToString());
                }
            }
            if (errors.Count != 0)
            {
                StringBuffer errBuf = new StringBuffer();
                for (int i = 0; i < errors.Count; i++)
                {
                    errBuf.Append(errors[i]);
                    errBuf.Append("\n");
                }
                Errln("FAIL: " + errBuf);
            }
        }

        private class ParseThreadJB5358 : ThreadJob
        {
            private readonly DecimalFormat decfmt;
            private readonly string numstr;
            private readonly double expect;
            private readonly IList<string> errors;

            public ParseThreadJB5358(DecimalFormat decfmt, string numstr, double expect, IList<string> errors)
            {
                this.decfmt = decfmt;
                this.numstr = numstr;
                this.expect = expect;
                this.errors = errors;
            }

            public override void Run()
            {
                for (int i = 0; i < 10000; i++)
                {
                    try
                    {
                        Number n = decfmt.Parse(numstr);
                        if (n.ToDouble() != expect)
                        {
                            lock (errors)
                            {
                                errors.Add("Bad parse result - expected:" + expect + " actual:" + n.ToDouble());
                            }
                        }
                    }
                    catch (Exception t)
                    {
                        lock (errors)
                        {
                            errors.Add(t.GetType().FullName + " - " + t.Message);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestSetCurrency()
        {
            DecimalFormatSymbols decf1 = DecimalFormatSymbols.GetInstance(new UCultureInfo("en-US"));
            DecimalFormatSymbols decf2 = DecimalFormatSymbols.GetInstance(new UCultureInfo("en-US"));
            decf2.CurrencySymbol = "UKD";
            DecimalFormat format1 = new DecimalFormat("000.000", decf1);
            DecimalFormat format2 = new DecimalFormat("000.000", decf2);
            Currency euro = Currency.GetInstance("EUR");
            format1.Currency = (euro);
            format2.Currency = (euro);
            assertEquals("Reset with currency symbol", format1, format2);
        }

        /*
         * Testing the method public StringBuffer format(Object number, ...)
         */
        [Test]
        public void TestFormat()
        {
            NumberFormat nf = NumberFormat.GetInstance();
            StringBuffer sb = new StringBuffer("dummy");
            FieldPosition fp = new FieldPosition(0);

            // Tests when "if (number instanceof Long)" is true
            try
            {
                nf.Format(Long.GetInstance(Long.Parse("0", CultureInfo.InvariantCulture)), sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "return an exception for a Long object. Error: " + e);
            }

            // Tests when "else if (number instanceof BigInteger)" is true
            try
            {
                nf.Format((object)BigInteger.Parse("0", radix: 10), sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "return an exception for a BigInteger object. Error: " + e);
            }

            // Tests when "else if (number instanceof java.math.BigDecimal)" is true
            try
            {
                nf.Format((Object)ICU4N.Numerics.BigMath.BigDecimal.Parse("0", CultureInfo.InvariantCulture), sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "return an exception for a java.math.BigDecimal object. Error: " + e);
            }

            // Tests when "else if (number instanceof com.ibm.icu.math.BigDecimal)" is true
            try
            {
                nf.Format((Object)ICU4N.Numerics.BigDecimal.Parse("0", CultureInfo.InvariantCulture), sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "return an exception for a com.ibm.icu.math.BigDecimal object. Error: " + e);
            }

            // Tests when "else if (number instanceof CurrencyAmount)" is true
            try
            {
                CurrencyAmount ca = new CurrencyAmount(0.0, Currency.GetInstance(new UCultureInfo("en_US")));
                nf.Format((Object)ca, sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "return an exception for a CurrencyAmount object. Error: " + e);
            }

            // Tests when "else if (number instanceof Number)" is true
            try
            {
                nf.Format(0.0, sb, fp);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.Format(Object number, ...) was not suppose to "
                        + "to return an exception for a Number object. Error: " + e);
            }

            // Tests when "else" is true
            try
            {
                nf.Format(new object(), sb, fp);
                Errln("NumberFormat.Format(Object number, ...) was suppose to "
                        + "return an exception for an invalid object.");
            }
            catch (Exception e)
            {
            }

            try
            {
                nf.Format("dummy", sb, fp);
                Errln("NumberFormat.Format(Object number, ...) was suppose to "
                        + "return an exception for an invalid object.");
            }
            catch (Exception e)
            {
            }
        }

        /*
         * Coverage tests for the implementation of abstract format methods not being called otherwise
         */
        [Test]
        public void TestFormatAbstractImplCoverage()
        {
            NumberFormat df = DecimalFormat.GetInstance(new CultureInfo("en"));
            NumberFormat cdf = CompactDecimalFormat.GetInstance(new CultureInfo("en"), CompactDecimalFormat.CompactStyle.Short);
            NumberFormat rbf = new RuleBasedNumberFormat(new UCultureInfo("en"), NumberPresentation.SpellOut);

            /*
             *  Test  NumberFormat.Format(BigDecimal,StringBuffer,FieldPosition)
             */
            StringBuffer sb = new StringBuffer();
            string result = df.Format(new BigDecimal(2000.43), sb, new FieldPosition(0)).ToString();
            if (!"2,000.43".Equals(result))
            {
                Errln("DecimalFormat failed. Expected: 2,000.43 - Actual: " + result);
            }

            sb.Clear();
            result = cdf.Format(new BigDecimal(2000.43), sb, new FieldPosition(0)).ToString();
            if (!"2K".Equals(result))
            {
                Errln("DecimalFormat failed. Expected: 2K - Actual: " + result);
            }

            sb.Clear();
            result = rbf.Format(new BigDecimal(2000.43), sb, new FieldPosition(0)).ToString();
            if (!"two thousand point four three".Equals(result))
            {
                Errln("DecimalFormat failed. Expected: 'two thousand point four three' - Actual: '" + result + "'");
            }
        }

        /*
         * Tests the method public final static NumberFormat getInstance(int style) public static NumberFormat
         * getInstance(Locale inLocale, int style) public static NumberFormat getInstance(ULocale desiredLocale, int choice)
         */
        [Test]
        public void TestGetInstance()
        {
            // Tests "public final static NumberFormat getInstance(int style)"
            int maxStyle = (int)NumberFormatStyle.StandardCurrencyStyle; // NumberFormat.STANDARDCURRENCYSTYLE;

            int[] invalid_cases = { (int)NumberFormatStyle.NumberStyle - 1, (int)NumberFormatStyle.NumberStyle - 2,
                maxStyle + 1, maxStyle + 2 };

            for (int i = (int)NumberFormatStyle.NumberStyle; i < maxStyle; i++)
            {
                try
                {
                    NumberFormat.GetInstance((NumberFormatStyle)i);
                }
                catch (Exception e)
                {
                    Errln("NumberFormat.GetInstance(int style) was not suppose to "
                            + "return an exception for passing value of " + i);
                }
            }

            for (int i = 0; i < invalid_cases.Length; i++)
            {
                try
                {
                    NumberFormat.GetInstance((NumberFormatStyle)invalid_cases[i]);
                    Errln("NumberFormat.GetInstance(int style) was suppose to "
                            + "return an exception for passing value of " + invalid_cases[i]);
                }
                catch (Exception e)
                {
                }
            }

            // Tests "public static NumberFormat getInstance(Locale inLocale, int style)"
            // ICU4N: Note that ICU4J has a bug here - they are testing an invalid locale. It should be ja_JP, not jp_JP.
            //string[] localeCases = { "en_US", "fr_FR", "de_DE", "jp_JP" };
            string[] localeCases = { "en-US", "fr-FR", "de-DE", "ja-JP" };

            for (int i = (int)NumberFormatStyle.NumberStyle; i < maxStyle; i++)
            {
                for (int j = 0; j < localeCases.Length; j++)
                {
                    try
                    {
                        NumberFormat.GetInstance(new CultureInfo(localeCases[j]), (NumberFormatStyle)i);
                    }
                    catch (Exception e)
                    {
                        Errln("NumberFormat.GetInstance(Locale inLocale, int style) was not suppose to "
                                + "return an exception for passing value of " + localeCases[j] + ", " + i);
                    }
                }
            }

            // Tests "public static NumberFormat getInstance(ULocale desiredLocale, int choice)"
            // Tests when "if (choice < NUMBERSTYLE || choice > PLURALCURRENCYSTYLE)" is true
            for (int i = 0; i < invalid_cases.Length; i++)
            {
                try
                {
                    NumberFormat.GetInstance((UCultureInfo)null, (NumberFormatStyle)invalid_cases[i]);
                    Errln("NumberFormat.GetInstance(ULocale inLocale, int choice) was not suppose to "
                            + "return an exception for passing value of " + invalid_cases[i]);
                }
                catch (Exception e)
                {
                }
            }
        }


        /*
         * The following class allows the method public NumberFormat createFormat(Locale loc, int formatType) to be
         * tested.
         */
        private class TestFactory_X : NumberFormatFactory
        {
            internal TestFactory_X() { }

            public override ICollection<string> GetSupportedLocaleNames()
            {
                return null;
            }

            public override NumberFormat CreateFormat(UCultureInfo loc, int formatType)
            {
                return null;
            }
        }

        /*
         * The following class allows the method public NumberFormat createFormat(ULocale loc, int formatType) to be
         * tested.
         */
        private class TestFactory_X1 : NumberFormatFactory
        {
            internal TestFactory_X1() { }

            public override ICollection<string> GetSupportedLocaleNames()
            {
                return null;
            }

            public override NumberFormat CreateFormat(CultureInfo loc, int formatType)
            {
                return null;
            }
        }

        /*
         * Tests the class public static abstract class NumberFormatFactory
         */
        [Test]
        public void TestNumberFormatFactory()
        {


            TestFactory_X tf = new TestFactory_X();
            TestFactory_X1 tf1 = new TestFactory_X1();

            /*
             * Tests the method public bool visible()
             */
            if (tf.Visible != true)
            {
                Errln("NumberFormatFactory.visible() was suppose to return true.");
            }

            /*
             * Tests the method public NumberFormat createFormat(Locale loc, int formatType)
             */
            if (tf.CreateFormat(new CultureInfo(""), 0) != null)
            {
                Errln("NumberFormatFactory.createFormat(Locale loc, int formatType) " + "was suppose to return null");
            }

            /*
             * Tests the method public NumberFormat createFormat(ULocale loc, int formatType)
             */
            if (tf1.CreateFormat(new UCultureInfo(""), 0) != null)
            {
                Errln("NumberFormatFactory.createFormat(ULocale loc, int formatType) " + "was suppose to return null");
            }
        }


        private class TestSimpleNumberFormatFactoryClass : SimpleNumberFormatFactory
        {
            /*
             * Tests the method public SimpleNumberFormatFactory(Locale locale)
             */
            internal TestSimpleNumberFormatFactoryClass() : base(new CultureInfo(""))
            {
            }
        }

        /*
         * Tests the class public static abstract class SimpleNumberFormatFactory extends NumberFormatFactory
         */
        [Test]
        public void TestSimpleNumberFormatFactory()
        {

            TestSimpleNumberFormatFactoryClass tsnff = new TestSimpleNumberFormatFactoryClass();
        }



        //@SuppressWarnings("serial")
        private class TestGetAvailableLocalesClass : NumberFormat
        {
#if FEATURE_FIELDPOSITION
            public
#else
            internal
#endif
                override StringBuffer Format(double number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

#if FEATURE_FIELDPOSITION
            public
#else
            internal
#endif
                override StringBuffer Format(long number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(BigInteger number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(Numerics.BigMath.BigDecimal number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(BigDecimal number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

            public override Number Parse(string text, ParsePosition parsePosition)
            {
                return null;
            }
        }



        /*
         * Tests the method public static ULocale[] getAvailableLocales()
         */
        //@SuppressWarnings("static-access")
        [Test]
        public void TestGetAvailableLocales()
        {
            // Tests when "if (shim == null)" is true


            try
            {
                TestGetAvailableLocalesClass test = new TestGetAvailableLocalesClass();
                //test.getAvailableLocales();
                NumberFormat.GetCultures(UCultureTypes.AllCultures);
            }
            catch (Exception e)
            {
                Errln("NumberFormat.getAvailableLocales() was not suppose to "
                        + "return an exception when getting getting available locales.");
            }
        }

        /*
         * Tests the method public void setMinimumIntegerDigits(int newValue)
         */
        [Test]
        public void TestSetMinimumIntegerDigits()
        {
            NumberFormat nf = NumberFormat.GetInstance();
            // For valid array, it is displayed as {min value, max value}
            // Tests when "if (minimumIntegerDigits > maximumIntegerDigits)" is true
            int[][] cases = { new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 2, 1 }, new int[] { 10, 0 } };
            int[] expectedMax = { 1, 1, 0, 0, 1, 0 };
            if (cases.Length != expectedMax.Length)
            {
                Errln("Can't continue test case method TestSetMinimumIntegerDigits "
                        + "since the test case arrays are unequal.");
            }
            else
            {
                for (int i = 0; i < cases.Length; i++)
                {
                    nf.MinimumIntegerDigits = (cases[i][0]);
                    nf.MaximumIntegerDigits = (cases[i][1]);
                    if (nf.MaximumIntegerDigits != expectedMax[i])
                    {
                        Errln("NumberFormat.setMinimumIntegerDigits(int newValue "
                                + "did not return an expected result for parameter " + cases[i][0] + " and " + cases[i][1]
                                        + " and expected " + expectedMax[i] + " but got " + nf.MaximumIntegerDigits);
                    }
                }
            }
        }

        //@SuppressWarnings("serial")
        private class TestRoundingModeClass : NumberFormat
        {

#if FEATURE_FIELDPOSITION
            public
#else
            internal
#endif
                override StringBuffer Format(double number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }


#if FEATURE_FIELDPOSITION
            public
#else
            internal
#endif
                override StringBuffer Format(long number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }


#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(BigInteger number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }


#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(Numerics.BigMath.BigDecimal number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
            public
#else
            internal
#endif
                override StringBuffer Format(BigDecimal number, StringBuffer toAppendTo, FieldPosition pos)
            {
                return null;
            }

            public override Number Parse(string text, ParsePosition parsePosition)
            {
                return null;
            }
        }

        /*
         * Tests the method public int getRoundingMode() public void setRoundingMode(int roundingMode)
         */
        [Test]
        public void TestRoundingMode()
        {

            TestRoundingModeClass tgrm = new TestRoundingModeClass();

            // Tests the function 'public void setRoundingMode(int roundingMode)'
            try
            {
                tgrm.RoundingMode = (Numerics.BigMath.RoundingMode)(0);
                Errln("NumberFormat.setRoundingMode(int) was suppose to return an exception");
            }
            catch (Exception e)
            {
            }

            // Tests the function 'public int getRoundingMode()'
            try
            {
                var _ = tgrm.RoundingMode;
                Errln("NumberFormat.getRoundingMode() was suppose to return an exception");
            }
            catch (Exception e)
            {
            }
        }

        /*
         * Testing lenient decimal/grouping separator parsing
         */
        [Test]
        public void TestLenientSymbolParsing()
        {
            DecimalFormat fmt = new DecimalFormat();
            DecimalFormatSymbols sym = new DecimalFormatSymbols();

            expect(fmt, "12\u300234", 12.34);

            // Ticket#7345 - case 1
            // Even strict parsing, the decimal separator set in the symbols
            // should be successfully parsed.

            sym.DecimalSeparator = ('\u3002');

            // non-strict
            fmt.SetDecimalFormatSymbols(sym);

            // strict - failed before the fix for #7345
            fmt.ParseStrict = (true);
            expect(fmt, "23\u300245", 23.45);
            fmt.ParseStrict = (false);


            // Ticket#7345 - case 2
            // Decimal separator variants other than DecimalFormatSymbols.decimalSeparator
            // should not hide the grouping separator DecimalFormatSymbols.groupingSeparator.
            sym.DecimalSeparator = ('.');
            sym.GroupingSeparator = (',');
            fmt.SetDecimalFormatSymbols(sym);

            expect(fmt, "1,234.56", 1234.56);

            sym.GroupingSeparator = ('\uFF61');
            fmt.SetDecimalFormatSymbols(sym);

            expect(fmt, "2\uFF61345.67", 2345.67);

            // Ticket#7128
            //
            sym.GroupingSeparator = (',');
            fmt.SetDecimalFormatSymbols(sym);

            // ICU4N TODO: Implement this setting?
            String skipExtSepParse = ICUConfig.DecimalFormat_SkipExtendedSeparatorParsing; // Get("com.ibm.icu.text.DecimalFormat.SkipExtendedSeparatorParsing", "false");
            if (skipExtSepParse.Equals("true", StringComparison.Ordinal))
            {
                // When the property SkipExtendedSeparatorParsing is true,
                // DecimalFormat does not use the extended equivalent separator
                // data and only uses the one in DecimalFormatSymbols.
                expect(fmt, "23 456", 23);
            }
            else
            {
                // Lenient separator parsing is enabled by default.
                // A space character below is interpreted as a
                // group separator, even ',' is used as grouping
                // separator in the symbols.
                expect(fmt, "12 345", 12345);
            }
        }

        /*
         * Testing currency driven max/min fraction digits problem
         * reported by ticket#7282
         */
        [Test]
        public void TestCurrencyFractionDigits()
        {
            double value = 99.12345;

            // Create currency instance
            NumberFormat cfmt = NumberFormat.GetCurrencyInstance(new UCultureInfo("ja_JP"));
            String text1 = cfmt.Format(value);

            // Reset the same currency and format the test value again
            cfmt.Currency = (cfmt.Currency);
            String text2 = cfmt.Format(value);

            // output1 and output2 must be identical
            if (!text1.Equals(text2, StringComparison.Ordinal))
            {
                Errln("NumberFormat.Format() should return the same result - text1="
                        + text1 + " text2=" + text2);
            }
        }

        /*
         * Testing rounding to negative zero problem
         * reported by ticket#7609
         */
        [Test]
        public void TestNegZeroRounding()
        {

            DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance();
            df.RoundingMode = Numerics.BigMath.RoundingMode.HalfUp; // (MathContext.ROUND_HALF_UP);
            df.MinimumFractionDigits = (1);
            df.MaximumFractionDigits = (1);
            String text1 = df.Format(-0.01);

            df.SetRoundingIncrement(0.1);
            String text2 = df.Format(-0.01);

            // output1 and output2 must be identical
            if (!text1.Equals(text2, StringComparison.Ordinal))
            {
                Errln("NumberFormat.Format() should return the same result - text1="
                        + text1 + " text2=" + text2);
            }

        }

        [Test]
        public void TestCurrencyAmountCoverage()
        {
            CurrencyAmount ca, cb;

            try
            {
                ca = new CurrencyAmount(null, (Currency)null);
                Errln("NullPointerException should have been thrown.");
            }
            catch (ArgumentNullException ex)
            {
            }
            try
            {
                ca = new CurrencyAmount((Number)Integer.GetInstance(0), (Currency)null);
                Errln("NullPointerException should have been thrown.");
            }
            catch (ArgumentNullException ex)
            {
            }

            ca = new CurrencyAmount((Number)Integer.GetInstance(0), Currency.GetInstance(new UCultureInfo("ja_JP")));
            cb = new CurrencyAmount((Number)Integer.GetInstance(1), Currency.GetInstance(new UCultureInfo("ja_JP")));
            if (ca.Equals(null))
            {
                Errln("Comparison should return false.");
            }
            if (!ca.Equals(ca))
            {
                Errln("Comparision should return true.");
            }
            if (ca.Equals(cb))
            {
                Errln("Comparison should return false.");
            }
        }

        [Test]
        public void TestExponentParse()
        {
            ParsePosition parsePos = new ParsePosition(0);
            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat fmt = new DecimalFormat("#####", symbols);
            Number result = fmt.Parse("5.06e-27", parsePos);
            if (result.ToDouble() != 5.06E-27 || parsePos.Index != 8)
            {
                Errln("ERROR: ERROR: parse failed - expected 5.06E-27, 8; got " + result.ToDouble() + ", " + parsePos.Index);
            }
        }

        [Test]
        public void TestExplicitParents()
        {
            // We use these for testing because decimal and grouping separators will be inherited from es_419
            // starting with CLDR 2.0
            string[] DATA = {
                "es", "CO", "", "1.250,75",
                "es", "ES", "", "1.250,75",
                "es", "GQ", "", "1.250,75",
                "es", "MX", "", "1,250.75",
                "es", "US", "", "1,250.75",
                "es", "VE", "", "1.250,75",

        };

            for (int i = 0; i < DATA.Length; i += 4)
            {


                CultureInfo locale = new CultureInfo(string.Concat(DATA[i], "-", DATA[i + 1]) /*, DATA[i + 2]*/);
                NumberFormat fmt = NumberFormat.GetInstance(locale);
                String s = fmt.Format(1250.75);
                if (s.Equals(DATA[i + 3], StringComparison.Ordinal))
                {
                    Logln("Ok: 1250.75 x " + locale + " => " + s);
                }
                else
                {
                    Errln("FAIL: 1250.75 x " + locale + " => " + s +
                            ", expected " + DATA[i + 3]);
                }
            }
        }

        /*
         * Test case for #9240
         * ICU4J 49.1 DecimalFormat did not clone the internal object holding
         * formatted text attribute information properly. Therefore, DecimalFormat
         * created by cloning may return incorrect results or may throw an exception
         * when formatToCharacterIterator is invoked from multiple threads.
         */
        [Test]
        public void TestFormatToCharacterIteratorThread()
        {
            int COUNT = 10;

            DecimalFormat fmt1 = new DecimalFormat("#0");
            DecimalFormat fmt2 = (DecimalFormat)fmt1.Clone();

            int[] res1 = new int[COUNT];
            int[] res2 = new int[COUNT];

            ThreadJob t1 = new FormatCharItrTestThread(fmt1, 1, res1);
            ThreadJob t2 = new FormatCharItrTestThread(fmt2, 100, res2);

            t1.Start();
            t2.Start();

            try
            {
                t1.Join();
                t2.Join();
            }
            catch (ThreadInterruptedException e)
            {
                //TODO
            }

            int val1 = res1[0];
            int val2 = res2[0];

            for (int i = 0; i < COUNT; i++)
            {
                if (res1[i] != val1)
                {
                    Errln("Inconsistent first run limit in test thread 1");
                }
                if (res2[i] != val2)
                {
                    Errln("Inconsistent first run limit in test thread 2");
                }
            }
        }

        /*
         * This feature had to do with a limitation in DigitList.java that no longer exists in the
         * new implementation.
         *
        [Test]
        public void TestParseMaxDigits() {
            DecimalFormat fmt = new DecimalFormat();
            String number = "100000000000";
            int newParseMax = number.Length - 1;

            fmt.setParseMaxDigits(-1);

            // Default value is 1000
            if (fmt.getParseMaxDigits() != 1000) {
                Errln("Fail valid value checking in setParseMaxDigits.");
            }

            try {
                if (fmt.Parse(number).ToDouble() == Float.POSITIVE_INFINITY) {
                    Errln("Got Infinity but should NOT when parsing number: " + number);
                }

                fmt.setParseMaxDigits(newParseMax);

                if (fmt.Parse(number).ToDouble() != Float.POSITIVE_INFINITY) {
                    Errln("Did not get Infinity but should when parsing number: " + number);
                }
            } catch (FormatException ex) {

            }
        }
        */

        private class FormatCharItrTestThread : ThreadJob
        {
            private readonly NumberFormat fmt;
            private readonly int num;
            private readonly int[] result;

            internal FormatCharItrTestThread(NumberFormat fmt, int num, int[] result)
            {
                this.fmt = fmt;
                this.num = num;
                this.result = result;
            }

            public override void Run()
            {
                for (int i = 0; i < result.Length; i++)
                {
                    AttributedCharacterIterator acitr = fmt.FormatToCharacterIterator(Integer.GetInstance(num));
                    acitr.First();
                    result[i] = acitr.GetRunLimit();
                }
            }
        }

        [Test]
        public void TestRoundingBehavior()
        {
            object[][] TEST_CASES = {
        new object[] {
            new UCultureInfo("en-US"),                             // ULocale - null for default locale
                    "#.##",                                 // Pattern
                    Integer.GetInstance((int)BigDecimal.RoundDown), // Rounding Mode or null (implicit)
                    Double.GetInstance(0.0d),                   // Rounding increment, Double or BigDecimal, or null (implicit)
                    Double.GetInstance(123.4567d),              // Input value, Long, Double, BigInteger or BigDecimal
                    "123.45"                                // Expected result, null for exception
                },
                new object[] {
            new UCultureInfo("en-US"),
                    "#.##",
                    null,
                    Double.GetInstance(0.1d),
                    Double.GetInstance(123.4567d),
                    "123.5"
                },
                new object[] {
            new UCultureInfo("en-US"),
                    "#.##",
                    Integer.GetInstance((int)BigDecimal.RoundDown),
                    Double.GetInstance(0.1d),
                    Double.GetInstance(123.4567d),
                    "123.4"
                },
                new object[] {
            new UCultureInfo("en-US"),
                    "#.##",
                    Integer.GetInstance((int)BigDecimal.RoundUnnecessary),
                    null,
                    Double.GetInstance(123.4567d),
                    null
                },
                new object[] {
            new UCultureInfo("en-US"),
                    "#.##",
                    Integer.GetInstance((int)BigDecimal.RoundDown),
                    null,
                    Long.GetInstance(1234),
                    "1234"
                },
        };

            int testNum = 1;

            foreach (object[] testCase in TEST_CASES)
            {
                // 0: locale
                // 1: pattern
                UCultureInfo locale = testCase[0] == null ? UCultureInfo.CurrentCulture : (UCultureInfo)testCase[0];
                String pattern = (String)testCase[1];

                DecimalFormat fmt = new DecimalFormat(pattern, DecimalFormatSymbols.GetInstance(locale));

                // 2: rounding mode
                Integer roundingMode = null;
                if (testCase[2] != null)
                {
                    roundingMode = (Integer)testCase[2];
                    fmt.RoundingMode = (Numerics.BigMath.RoundingMode)(roundingMode.ToInt32());
                }

                // 3: rounding increment
                if (testCase[3] != null)
                {
                    if (testCase[3] is Double dbl)
                    {
                        fmt.SetRoundingIncrement(dbl);
                    }
                    else if (testCase[3] is BigDecimal bigDecimal)
                    {
                        fmt.SetRoundingIncrement(bigDecimal);
                    }
                    else if (testCase[3] is Numerics.BigMath.BigDecimal bigDecimal2)
                    {
                        fmt.SetRoundingIncrement(bigDecimal2);
                    }
                }

                // 4: input number
                String s = null;
                bool bException = false;
                try
                {
                    s = fmt.Format(testCase[4]);
                }
                catch (ArithmeticException e) // ICU4N TODO: Check this exception type
                {
                    bException = true;
                }

                if (bException)
                {
                    if (testCase[5] != null)
                    {
                        Errln("Test case #" + testNum + ": ArithmeticException was thrown.");
                    }
                }
                else
                {
                    if (testCase[5] == null)
                    {
                        Errln("Test case #" + testNum +
                                ": ArithmeticException must be thrown, but got formatted result: " +
                                s);
                    }
                    else
                    {
                        assertEquals("Test case #" + testNum, testCase[5], s);
                    }
                }

                testNum++;
            }
        }

        [Test]
        public void TestSignificantDigits()
        {
            double[] input = {
                0, 0,
                123, -123,
                12345, -12345,
                123.45, -123.45,
                123.44501, -123.44501,
                0.001234, -0.001234,
                0.00000000123, -0.00000000123,
                0.0000000000000000000123, -0.0000000000000000000123,
                1.2, -1.2,
                0.0000000012344501, -0.0000000012344501,
                123445.01, -123445.01,
                12344501000000000000000000000000000.0, -12344501000000000000000000000000000.0,
        };
            string[] expected = {
                "0.00", "0.00",
                "123", "-123",
                "12345", "-12345",
                "123.45", "-123.45",
                "123.45", "-123.45",
                "0.001234", "-0.001234",
                "0.00000000123", "-0.00000000123",
                "0.0000000000000000000123", "-0.0000000000000000000123",
                "1.20", "-1.20",
                "0.0000000012345", "-0.0000000012345",
                "123450", "-123450",
                "12345000000000000000000000000000000", "-12345000000000000000000000000000000",
        };
            DecimalFormat numberFormat =
                    (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en-US"));
            numberFormat.AreSignificantDigitsUsed = (true);
            numberFormat.MinimumSignificantDigits = (3);
            numberFormat.MaximumSignificantDigits = (5);
            numberFormat.IsGroupingUsed = (false);
            for (int i = 0; i < input.Length; i++)
            {
                assertEquals("TestSignificantDigits", expected[i], numberFormat.Format(input[i]));
            }
        }

        [Test]
        public void TestBug9936()
        {
            DecimalFormat numberFormat =
                    (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en-US"));
            assertFalse("", numberFormat.AreSignificantDigitsUsed);

            numberFormat.AreSignificantDigitsUsed = (true);
            assertTrue("", numberFormat.AreSignificantDigitsUsed);

            numberFormat.AreSignificantDigitsUsed = (false);
            assertFalse("", numberFormat.AreSignificantDigitsUsed);

            numberFormat.MinimumSignificantDigits = (3);
            assertTrue("", numberFormat.AreSignificantDigitsUsed);

            numberFormat.AreSignificantDigitsUsed = (false);
            numberFormat.MaximumSignificantDigits = (6);
            assertTrue("", numberFormat.AreSignificantDigitsUsed);
        }

        [Test]
        public void TestShowZero()
        {
            DecimalFormat numberFormat =
                    (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en-US"));
            numberFormat.AreSignificantDigitsUsed = (true);
            numberFormat.MaximumSignificantDigits = (3);
            assertEquals("TestShowZero", "0", numberFormat.Format(0.0));
        }

        [Test]
        public void TestCurrencyPlurals()
        {
            string[][] tests = {
                new string[] {"en", "USD", "1", "1 US dollar"},
                new string[] {"en", "USD", "1.0", "1.0 US dollars"},
                new string[] {"en", "USD", "1.00", "1.00 US dollars"},
                new string[] {"en", "USD", "1.99", "1.99 US dollars"},
                new string[] {"en", "AUD", "1", "1 Australian dollar"},
                new string[] {"en", "AUD", "1.00", "1.00 Australian dollars"},
                new string[] {"sl", "USD", "1", "1 ameri\u0161ki dolar"},
                new string[] {"sl", "USD", "2", "2 ameri\u0161ka dolarja"},
                new string[] {"sl", "USD", "3", "3 ameri\u0161ki dolarji"},
                new string[] {"sl", "USD", "5", "5 ameriških dolarjev"},
                new string[] {"fr", "USD", "1.99", "1,99 dollar des États-Unis"},
                new string[] {"ru", "RUB", "1", "1 \u0440\u043E\u0441\u0441\u0438\u0439\u0441\u043A\u0438\u0439 \u0440\u0443\u0431\u043B\u044C"},
                new string[] {"ru", "RUB", "2", "2 \u0440\u043E\u0441\u0441\u0438\u0439\u0441\u043A\u0438\u0445 \u0440\u0443\u0431\u043B\u044F"},
                new string[] {"ru", "RUB", "5", "5 \u0440\u043E\u0441\u0441\u0438\u0439\u0441\u043A\u0438\u0445 \u0440\u0443\u0431\u043B\u0435\u0439"},
        };
            foreach (String[] test in tests)
            {
                DecimalFormat numberFormat = (DecimalFormat)DecimalFormat.GetInstance(new UCultureInfo(test[0]), NumberFormatStyle.PluralCurrencyStyle);
                numberFormat.Currency = (Currency.GetInstance(test[1]));
                double number = Double.Parse(test[2], CultureInfo.InvariantCulture);
                int dotPos = test[2].IndexOf('.');
                int decimals = dotPos < 0 ? 0 : test[2].Length - dotPos - 1;
                int digits = dotPos < 0 ? test[2].Length : test[2].Length - 1;
                numberFormat.MaximumFractionDigits = (decimals);
                numberFormat.MinimumFractionDigits = (decimals);
                String actual = numberFormat.Format(number);
                assertEquals(test[0] + "\t" + test[1] + "\t" + test[2], test[3], actual);
                numberFormat.MaximumSignificantDigits = (digits);
                numberFormat.MinimumSignificantDigits = (digits);
                actual = numberFormat.Format(number);
                assertEquals(test[0] + "\t" + test[1] + "\t" + test[2], test[3], actual);
            }
        }

        [Test]
        public void TestCustomCurrencySignAndSeparator()
        {
            DecimalFormatSymbols custom = new DecimalFormatSymbols(new UCultureInfo("en-US"));

            custom.CurrencySymbol = ("*");
            custom.MonetaryGroupingSeparator = ('^');
            custom.MonetaryDecimalSeparator = (':');

            DecimalFormat fmt = new DecimalFormat("\u00A4 #,##0.00", custom);

            string numstr = "* 1^234:56";
            expect2(fmt, 1234.56, numstr);
        }

        private class SignsAndMarksItem
        {
            public String locale;
            public bool lenient;
            public String numString;
            public double value;
            // Simple constructor
            public SignsAndMarksItem(String loc, bool lnt, String numStr, double val)
            {
                locale = loc;
                lenient = lnt;
                numString = numStr;
                value = val;
            }
        }

        [Test]
        public void TestParseSignsAndMarks()
        {

            SignsAndMarksItem[] items = {
            // *** Note, ICU4J lenient number parsing does not handle arbitrary whitespace, but can
            // treat some whitespace as a grouping separator. The cases marked *** below depend
            // on isGroupingUsed() being set for the locale, which in turn depends on grouping
            // separators being present in the decimalFormat pattern for the locale (& num sys).
            //
            //                    locale                lenient numString                               value
            new SignsAndMarksItem("en", false, "12", 12),
            new SignsAndMarksItem("en", true, "12", 12),
            new SignsAndMarksItem("en", false, "-23", -23),
            new SignsAndMarksItem("en", true, "-23", -23),
            new SignsAndMarksItem("en", true, "- 23", -23), // ***
            new SignsAndMarksItem("en", false, "\u200E-23", -23),
            new SignsAndMarksItem("en", true, "\u200E-23", -23),
            new SignsAndMarksItem("en", true, "\u200E- 23", -23), // ***

            new SignsAndMarksItem("en@numbers=arab", false, "\u0663\u0664", 34),
            new SignsAndMarksItem("en@numbers=arab", true, "\u0663\u0664", 34),
            new SignsAndMarksItem("en@numbers=arab", false, "-\u0664\u0665", -45),
            new SignsAndMarksItem("en@numbers=arab", true, "-\u0664\u0665", -45),
            new SignsAndMarksItem("en@numbers=arab", true, "- \u0664\u0665", -45), // ***
            new SignsAndMarksItem("en@numbers=arab", false, "\u200F-\u0664\u0665", -45),
            new SignsAndMarksItem("en@numbers=arab", true, "\u200F-\u0664\u0665", -45),
            new SignsAndMarksItem("en@numbers=arab", true, "\u200F- \u0664\u0665", -45), // ***

            new SignsAndMarksItem("en@numbers=arabext", false, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("en@numbers=arabext", true, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("en@numbers=arabext", false, "-\u06F6\u06F7", -67),
            new SignsAndMarksItem("en@numbers=arabext", true, "-\u06F6\u06F7", -67),
            new SignsAndMarksItem("en@numbers=arabext", true, "- \u06F6\u06F7", -67), // ***
            new SignsAndMarksItem("en@numbers=arabext", false, "\u200E-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("en@numbers=arabext", true, "\u200E-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("en@numbers=arabext", true, "\u200E-\u200E \u06F6\u06F7", -67), // ***

            new SignsAndMarksItem("he", false, "12", 12),
            new SignsAndMarksItem("he", true, "12", 12),
            new SignsAndMarksItem("he", false, "-23", -23),
            new SignsAndMarksItem("he", true, "-23", -23),
            new SignsAndMarksItem("he", true, "- 23", -23), // ***
            new SignsAndMarksItem("he", false, "\u200E-23", -23),
            new SignsAndMarksItem("he", true, "\u200E-23", -23),
            new SignsAndMarksItem("he", true, "\u200E- 23", -23), // ***

            new SignsAndMarksItem("ar", false, "\u0663\u0664", 34),
            new SignsAndMarksItem("ar", true, "\u0663\u0664", 34),
            new SignsAndMarksItem("ar", false, "-\u0664\u0665", -45),
            new SignsAndMarksItem("ar", true, "-\u0664\u0665", -45),
            new SignsAndMarksItem("ar", true, "- \u0664\u0665", -45), // ***
            new SignsAndMarksItem("ar", false, "\u200F-\u0664\u0665", -45),
            new SignsAndMarksItem("ar", true, "\u200F-\u0664\u0665", -45),
            new SignsAndMarksItem("ar", true, "\u200F- \u0664\u0665", -45), // ***

            new SignsAndMarksItem("ar_MA", false, "12", 12),
            new SignsAndMarksItem("ar_MA", true, "12", 12),
            new SignsAndMarksItem("ar_MA", false, "-23", -23),
            new SignsAndMarksItem("ar_MA", true, "-23", -23),
            new SignsAndMarksItem("ar_MA", true, "- 23", -23), // ***
            new SignsAndMarksItem("ar_MA", false, "\u200E-23", -23),
            new SignsAndMarksItem("ar_MA", true, "\u200E-23", -23),
            new SignsAndMarksItem("ar_MA", true, "\u200E- 23", -23), // ***

            new SignsAndMarksItem("fa", false, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("fa", true, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("fa", false, "\u2212\u06F6\u06F7", -67),
            new SignsAndMarksItem("fa", true, "\u2212\u06F6\u06F7", -67),
            new SignsAndMarksItem("fa", true, "\u2212 \u06F6\u06F7", -67), // ***
            new SignsAndMarksItem("fa", false, "\u200E\u2212\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("fa", true, "\u200E\u2212\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("fa", true, "\u200E\u2212\u200E \u06F6\u06F7", -67), // ***

            new SignsAndMarksItem("ps", false, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("ps", true, "\u06F5\u06F6", 56),
            new SignsAndMarksItem("ps", false, "-\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "-\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "- \u06F6\u06F7", -67), // ***
            new SignsAndMarksItem("ps", false, "\u200E-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "\u200E-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "\u200E-\u200E \u06F6\u06F7", -67), // ***
            new SignsAndMarksItem("ps", false, "-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "-\u200E\u06F6\u06F7", -67),
            new SignsAndMarksItem("ps", true, "-\u200E \u06F6\u06F7", -67), // ***
        };
            foreach (SignsAndMarksItem item in items)
            {
                UCultureInfo locale = new UCultureInfo(item.locale);
                NumberFormat numfmt = NumberFormat.GetInstance(locale);
                if (numfmt != null)
                {
                    numfmt.ParseStrict = (!item.lenient);
                    ParsePosition ppos = new ParsePosition(0);
                    Number num = numfmt.Parse(item.numString, ppos);
                    if (num != null && ppos.Index == item.numString.Length)
                    {
                        double parsedValue = num.ToDouble();
                        if (parsedValue != item.value)
                        {
                            Errln("FAIL: locale " + item.locale + ", lenient " + item.lenient + ", parse of \"" + item.numString + "\" gives value " + parsedValue);
                        }
                    }
                    else
                    {
                        Errln("FAIL: locale " + item.locale + ", lenient " + item.lenient + ", parse of \"" + item.numString + "\" gives position " + ppos.Index);
                    }
                }
                else
                {
                    Errln("FAIL: NumberFormat.getInstance for locale " + item.locale);
                }
            }
        }

        [Test]
        public void TestContext()
        {
            // just a minimal sanity check for now
            NumberFormat nfmt = NumberFormat.GetInstance();
            DisplayContext context = nfmt.GetContext(DisplayContextType.Capitalization);
            if (context != DisplayContext.CapitalizationNone)
            {
                Errln("FAIL: Initial NumberFormat.getContext() is not CAPITALIZATION_NONE");
            }
            nfmt.SetContext(DisplayContext.CapitalizationForStandalone);
            context = nfmt.GetContext(DisplayContextType.Capitalization);
            if (context != DisplayContext.CapitalizationForStandalone)
            {
                Errln("FAIL: NumberFormat.getContext() does not return the value set, CAPITALIZATION_FOR_STANDALONE");
            }
        }

        [Test]
        public void TestAccountingCurrency()
        {
            string[][] tests = {
                //locale              num         curr fmt per loc     curr std fmt         curr acct fmt        rt
                new string[] {"en_US",             "1234.5",   "$1,234.50",         "$1,234.50",         "$1,234.50",         "true"},
                new string[] {"en_US@cf=account",  "1234.5",   "$1,234.50",         "$1,234.50",         "$1,234.50",         "true"},
                new string[] {"en_US",             "-1234.5",  "-$1,234.50",        "-$1,234.50",        "($1,234.50)",       "true"},
                new string[] {"en_US@cf=standard", "-1234.5",  "-$1,234.50",        "-$1,234.50",        "($1,234.50)",       "true"},
                new string[] {"en_US@cf=account",  "-1234.5",  "($1,234.50)",       "-$1,234.50",        "($1,234.50)",       "true"},
                new string[] {"en_US",             "0",        "$0.00",             "$0.00",             "$0.00",             "true"},
                new string[] {"en_US",             "-0.2",     "-$0.20",            "-$0.20",            "($0.20)",           "true"},
                new string[] {"en_US@cf=standard", "-0.2",     "-$0.20",            "-$0.20",            "($0.20)",           "true"},
                new string[] {"en_US@cf=account",  "-0.2",     "($0.20)",           "-$0.20",            "($0.20)",           "true"},
                new string[] {"ja_JP",             "10000",    "￥10,000",          "￥10,000",          "￥10,000",          "true" },
                new string[] {"ja_JP",             "-1000.5",  "-￥1,000",          "-￥1,000",          "(￥1,000)",         "false"},
                new string[] {"ja_JP@cf=account",  "-1000.5",  "(￥1,000)",         "-￥1,000",          "(￥1,000)",         "false"},
                new string[] {"de_DE",             "-23456.7", "-23.456,70\u00A0€", "-23.456,70\u00A0€", "-23.456,70\u00A0€", "true" },
        };
            foreach (string[] data in tests)
            {
                UCultureInfo loc = new UCultureInfo(data[0]);
                Double num = Double.Parse(data[1], CultureInfo.InvariantCulture);
                String fmtPerLocExpected = data[2];
                String fmtStandardExpected = data[3];
                String fmtAccountExpected = data[4];
                bool rt = bool.Parse(data[5]);

                NumberFormat fmtPerLoc = NumberFormat.GetInstance(loc, NumberFormatStyle.CurrencyStyle);
                expect(fmtPerLoc, (Number)num, fmtPerLocExpected, rt);

                NumberFormat fmtStandard = NumberFormat.GetInstance(loc, NumberFormatStyle.StandardCurrencyStyle);
                expect(fmtStandard, (Number)num, fmtStandardExpected, rt);

                NumberFormat fmtAccount = NumberFormat.GetInstance(loc, NumberFormatStyle.AccountingCurrencyStyle);
                expect(fmtAccount, (Number)num, fmtAccountExpected, rt);
            }
        }

        [Test]
        public void TestCurrencyUsage()
        {
            // the 1st one is checking setter/getter, while the 2nd one checks for getInstance
            // compare the Currency and Currency Cash Digits
            // Note that as of CLDR 26:
            // * TWD switches from 0 decimals to 2; PKR still has 0, so change test to that
            // * CAD rounds to .05 in the cash style only.
            for (int i = 0; i < 2; i++)
            {
                String original_expected = "PKR 124";
                DecimalFormat custom = null;
                if (i == 0)
                {
                    custom = (DecimalFormat)DecimalFormat.GetInstance(new UCultureInfo("en_US@currency=PKR"),
                            NumberFormatStyle.CurrencyStyle);

                    String original = custom.Format(123.567);
                    assertEquals("Test Currency Context", original_expected, original);

                    // test the getter
                    assertEquals("Test Currency Context Purpose", custom.CurrencyUsage,
                            CurrencyUsage.Standard);
                    custom.CurrencyUsage = (CurrencyUsage.Cash);
                    assertEquals("Test Currency Context Purpose", custom.CurrencyUsage, CurrencyUsage.Cash);
                }
                else
                {
                    custom = (DecimalFormat)DecimalFormat.GetInstance(new UCultureInfo("en_US@currency=PKR"),
                            NumberFormatStyle.CashCurrencyStyle);

                    // test the getter
                    assertEquals("Test Currency Context Purpose", custom.CurrencyUsage, CurrencyUsage.Cash);
                }

                String cash_currency = custom.Format(123.567);
                String cash_currency_expected = "PKR 124";
                assertEquals("Test Currency Context", cash_currency_expected, cash_currency);
            }

            // the 1st one is checking setter/getter, while the 2nd one checks for getInstance
            // compare the Currency and Currency Cash Rounding
            for (int i = 0; i < 2; i++)
            {
                String original_rounding_expected = "CA$123.57";
                DecimalFormat fmt = null;
                if (i == 0)
                {
                    fmt = (DecimalFormat)DecimalFormat.GetInstance(new UCultureInfo("en_US@currency=CAD"),
                           NumberFormatStyle.CurrencyStyle);

                    String original_rounding = fmt.Format(123.566);
                    assertEquals("Test Currency Context", original_rounding_expected, original_rounding);

                    fmt.CurrencyUsage = (CurrencyUsage.Cash);
                }
                else
                {
                    fmt = (DecimalFormat)DecimalFormat.GetInstance(new UCultureInfo("en_US@currency=CAD"),
                           NumberFormatStyle.CashCurrencyStyle);
                }

                String cash_rounding_currency = fmt.Format(123.567);
                String cash__rounding_currency_expected = "CA$123.55";
                assertEquals("Test Currency Context", cash__rounding_currency_expected, cash_rounding_currency);
            }

            // the 1st one is checking setter/getter, while the 2nd one checks for getInstance
            // Test the currency change
            for (int i = 0; i < 2; i++)
            {
                DecimalFormat fmt2 = null;
                if (i == 1)
                {
                    fmt2 = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en_US@currency=JPY"),
                           NumberFormatStyle.CurrencyStyle);
                    fmt2.CurrencyUsage = (CurrencyUsage.Cash);
                }
                else
                {
                    fmt2 = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en_US@currency=JPY"),
                           NumberFormatStyle.CashCurrencyStyle);
                }

                fmt2.Currency = (Currency.GetInstance("PKR"));
                String PKR_changed = fmt2.Format(123.567);
                String PKR_changed_expected = "PKR 124";
                assertEquals("Test Currency Context", PKR_changed_expected, PKR_changed);
            }
        }

        [Test]
        public void TestCurrencyWithMinMaxFractionDigits()
        {
            DecimalFormat df = new DecimalFormat();
            df.ApplyPattern("¤#,##0.00");
            df.Currency = (Currency.GetInstance("USD"));
            assertEquals("Basic currency format fails", "$1.23", df.Format(1.234));
            df.MaximumFractionDigits = (4);
            assertEquals("Currency with max fraction == 4", "$1.234", df.Format(1.234));
            df.MinimumFractionDigits = (4);
            assertEquals("Currency with min fraction == 4", "$1.2340", df.Format(1.234));
        }

        [Test]
        public void TestParseRequiredDecimalPoint()
        {

            string[] testPattern = { "00.####", "00.0", "00" };

            String value2Parse = "99";
            String value2ParseWithDecimal = "99.9";
            double parseValue = 99;
            double parseValueWithDecimal = 99.9;
            DecimalFormat parser = new DecimalFormat();
            double result;
            bool hasDecimalPoint;
            for (int i = 0; i < testPattern.Length; i++)
            {
                parser.ApplyPattern(testPattern[i]);
                hasDecimalPoint = testPattern[i].Contains(".");

                parser.DecimalPatternMatchRequired = (false);
                try
                {
                    result = parser.Parse(value2Parse).ToDouble();
                    assertEquals("wrong parsed value", parseValue, result);
                }
                catch (FormatException e)
                {
                    TestFmwk.Errln("Parsing " + value2Parse + " should have succeeded with " + testPattern[i] +
                                " and isDecimalPointMatchRequired set to: " + parser.DecimalPatternMatchRequired);
                }
                try
                {
                    result = parser.Parse(value2ParseWithDecimal).ToDouble();
                    assertEquals("wrong parsed value", parseValueWithDecimal, result);
                }
                catch (FormatException e)
                {
                    TestFmwk.Errln("Parsing " + value2ParseWithDecimal + " should have succeeded with " + testPattern[i] +
                                " and isDecimalPointMatchRequired set to: " + parser.DecimalPatternMatchRequired);
                }

                parser.DecimalPatternMatchRequired = (true);
                try
                {
                    result = parser.Parse(value2Parse).ToDouble();
                    if (hasDecimalPoint)
                    {
                        TestFmwk.Errln("Parsing " + value2Parse + " should NOT have succeeded with " + testPattern[i] +
                                " and isDecimalPointMatchRequired set to: " + parser.DecimalPatternMatchRequired);
                    }
                }
                catch (FormatException e)
                {
                    // OK, should fail
                }
                try
                {
                    result = parser.Parse(value2ParseWithDecimal).ToDouble();
                    if (!hasDecimalPoint)
                    {
                        TestFmwk.Errln("Parsing " + value2ParseWithDecimal + " should NOT have succeeded with " + testPattern[i] +
                                " and isDecimalPointMatchRequired set to: " + parser.DecimalPatternMatchRequired);
                    }
                }
                catch (FormatException e)
                {
                    // OK, should fail
                }
            }
        }

        [Test]
        public void TestCurrFmtNegSameAsPositive()
        {
            DecimalFormatSymbols decfmtsym = DecimalFormatSymbols.GetInstance(new CultureInfo("en-US"));
            decfmtsym.MinusSign = ('\u200B'); // ZERO WIDTH SPACE, in ICU4J cannot set to empty string
            DecimalFormat decfmt = new DecimalFormat("\u00A4#,##0.00;-\u00A4#,##0.00", decfmtsym);
            String currFmtResult = decfmt.Format(-100.0);
            if (!currFmtResult.Equals("\u200B$100.00", StringComparison.Ordinal))
            {
                Errln("decfmt.toPattern results wrong, expected \u200B$100.00, got " + currFmtResult);
            }
        }

        [Test]
        public void TestNumberFormatTestDataToString()
        {
            new DataDrivenNumberFormatTestData().ToString();
        }

        // Testing for Issue 11805.
        [Test]
        public void TestFormatToCharacterIteratorIssue11805()
        {
            double number = -350.76;
            DecimalFormat dfUS = (DecimalFormat)DecimalFormat.GetCurrencyInstance(new CultureInfo("en-US"));
            String strUS = dfUS.Format(number);
            ICollection<AttributedCharacterIteratorAttribute> resultUS = dfUS.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative US Results: " + strUS, 5, resultUS.Count);

            // For each test, add assert that all the fields are present and in the right spot.
            // TODO: Add tests for identify and position of each field, as in IntlTestDecimalFormatAPIC.

            DecimalFormat dfDE = (DecimalFormat)DecimalFormat.GetCurrencyInstance(new CultureInfo("de-DE"));
            String strDE = dfDE.Format(number);
            ICollection<AttributedCharacterIteratorAttribute> resultDE = dfDE.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative DE Results: " + strDE, 5, resultDE.Count);

            DecimalFormat dfIN = (DecimalFormat)DecimalFormat.GetCurrencyInstance(new CultureInfo("hi-in"));
            String strIN = dfIN.Format(number);
            ICollection<AttributedCharacterIteratorAttribute> resultIN = dfIN.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative IN Results: " + strIN, 5, resultIN.Count);

            DecimalFormat dfJP = (DecimalFormat)DecimalFormat.GetCurrencyInstance(new CultureInfo("ja-JP"));
            String strJP = dfJP.Format(number);
            ICollection<AttributedCharacterIteratorAttribute> resultJP = dfJP.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative JA Results: " + strJP, 3, resultJP.Count);

            DecimalFormat dfGB = (DecimalFormat)DecimalFormat.GetCurrencyInstance(new CultureInfo("en-gb"));
            String strGB = dfGB.Format(number);
            ICollection<AttributedCharacterIteratorAttribute> resultGB = dfGB.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative GB Results: " + strGB, 5, resultGB.Count);

            DecimalFormat dfPlural = (DecimalFormat)NumberFormat.GetInstance(new CultureInfo("en-gb"),
                    NumberFormatStyle.PluralCurrencyStyle);
            strGB = dfPlural.Format(number);
            resultGB = dfPlural.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative GB Results: " + strGB, 5, resultGB.Count);

            strGB = dfPlural.Format(1);
            resultGB = dfPlural.FormatToCharacterIterator((Integer)1).GetAllAttributeKeys();
            assertEquals("Negative GB Results: " + strGB, 4, resultGB.Count);

            // Test output with unit value.
            DecimalFormat auPlural = (DecimalFormat)NumberFormat.GetInstance(new CultureInfo("en-au"),
                    NumberFormatStyle.PluralCurrencyStyle);
            String strAU = auPlural.Format(1L);
            ICollection<AttributedCharacterIteratorAttribute> resultAU =
            auPlural.FormatToCharacterIterator((Long)1L).GetAllAttributeKeys();
            assertEquals("Unit AU Result: " + strAU, 4, resultAU.Count);

            // Verify Permille fields.
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-gb"));
            DecimalFormat dfPermille = new DecimalFormat("####0.##\u2030", sym);
            strGB = dfPermille.Format(number);
            resultGB = dfPermille.FormatToCharacterIterator((Double)number).GetAllAttributeKeys();
            assertEquals("Negative GB Permille Results: " + strGB, 3, resultGB.Count);
        }

        // Testing for Issue 11808.
        [Test]
        public void TestRoundUnnecessarytIssue11808()
        {
            DecimalFormat df = (DecimalFormat)DecimalFormat.GetInstance();
            StringBuffer result = new StringBuffer("");
            df.RoundingMode = Numerics.BigMath.RoundingMode.Unnecessary; // BigDecimal.RoundUnnecessary; // (BigDecimal.ROUND_UNNECESSARY);
            df.ApplyPattern("00.0#E0");

            try
            {
                df.Format(99999.0, result, new FieldPosition(0));
                fail("Missing ArithmeticException for double: " + result);
            }
            catch (ArithmeticException expected)
            {
                // The exception should be thrown, since rounding is needed.
            }

            try
            {
                result = df.Format(99999, result, new FieldPosition(0));
                fail("Missing ArithmeticException for int: " + result);
            }
            catch (ArithmeticException expected)
            {
                // The exception should be thrown, since rounding is needed.
            }

            try
            {
                result = df.Format(BigInteger.Parse("999999", radix: 10), result, new FieldPosition(0));
                fail("Missing ArithmeticException for BigInteger: " + result);
            }
            catch (ArithmeticException expected)
            {
                // The exception should be thrown, since rounding is needed.
            }

            try
            {
                result = df.Format(BigDecimal.Parse("99999", CultureInfo.InvariantCulture), result, new FieldPosition(0));
                fail("Missing ArithmeticException for BigDecimal: " + result);
            }
            catch (ArithmeticException expected)
            {
                // The exception should be thrown, since rounding is needed.
            }

            try
            {
                result = df.Format(BigDecimal.Parse("-99999", CultureInfo.InvariantCulture), result, new FieldPosition(0));
                fail("Missing ArithmeticException for BigDecimal: " + result);
            }
            catch (ArithmeticException expected)
            {
                // The exception should be thrown, since rounding is needed.
            }
        }

        // Testing for Issue 11735.
        [Test]
        public void TestNPEIssue11735()
        {
            DecimalFormat fmt = new DecimalFormat("0", new DecimalFormatSymbols(new UCultureInfo("en")));
            ParsePosition ppos = new ParsePosition(0);
            assertEquals("Currency symbol missing in parse. Expect null result.",
                    fmt.ParseCurrency("53.45", ppos), null);
        }

        private void CompareAttributedCharacterFormatOutput(AttributedCharacterIterator iterator,
            IList<FieldContainer> expected, String formattedOutput)
        {

            List<FieldContainer> result = new List<FieldContainer>();
            while (iterator.Index != iterator.EndIndex)
            {
                int start = iterator.GetRunStart();
                int end = iterator.GetRunLimit();
                var it = iterator.GetAttributes().Keys.GetEnumerator();
                it.MoveNext();
                AttributedCharacterIteratorAttribute attribute = (AttributedCharacterIteratorAttribute)it.Current;
                // For positions with both INTEGER and GROUPING attributes, we want the GROUPING attribute.
                if (it.MoveNext() && attribute.Equals(NumberFormatField.Integer))
                {
                    attribute = (AttributedCharacterIteratorAttribute)it.Current;
                }
                Object value = iterator.GetAttribute(attribute);
                result.Add(new FieldContainer(start, end, attribute, value));
                iterator.SetIndex(end);
            }
            assertEquals("Comparing vector length for " + formattedOutput,
                expected.Count, result.Count);

            //if (!expected.containsAll(result))
            if (result.Except(expected).Any())
            {
                // Print information on the differences.
                for (int i = 0; i < expected.Count; i++)
                {
                    Console.Out.WriteLine("     expected[" + i + "] =" +
                        expected[i].start + " " +
                        expected[i].end + " " +
                        expected[i].attribute + " " +
                        expected[i].value);
                    Console.Out.WriteLine(" result[" + i + "] =" +
                result[i].start + " " +
                result[i].end + " " +
                result[i].attribute + " " +
                result[i].value);
                }
            }
            //assertTrue("Comparing vector results for " + formattedOutput, expected.containsAll(result));
            assertTrue("Comparing vector results for " + formattedOutput, !result.Except(expected).Any());
        }

        // Testing for Issue 11914, missing FieldPositions for some field types.
        [Test]
        public void TestNPEIssue11914()
        {
            // First test: Double value with grouping separators.
            List<FieldContainer> v1 = new List<FieldContainer>(7);
            v1.Add(new FieldContainer(0, 3, NumberFormatField.Integer));
            v1.Add(new FieldContainer(3, 4, NumberFormatField.GroupingSeparator));
            v1.Add(new FieldContainer(4, 7, NumberFormatField.Integer));
            v1.Add(new FieldContainer(7, 8, NumberFormatField.GroupingSeparator));
            v1.Add(new FieldContainer(8, 11, NumberFormatField.Integer));
            v1.Add(new FieldContainer(11, 12, NumberFormatField.DecimalSeparator));
            v1.Add(new FieldContainer(12, 15, NumberFormatField.Fraction));

            Number number = Double.GetInstance(123456789.9753);
            UCultureInfo usLoc = new UCultureInfo("en-US");
            DecimalFormatSymbols US = new DecimalFormatSymbols(usLoc);

            NumberFormat outFmt = NumberFormat.GetNumberInstance(usLoc);
            String numFmtted = outFmt.Format(number);
            AttributedCharacterIterator iterator =
                    outFmt.FormatToCharacterIterator(number);
            CompareAttributedCharacterFormatOutput(iterator, v1, numFmtted);

            // Second test: Double with scientific notation formatting.
            List<FieldContainer> v2 = new List<FieldContainer>(7);
            v2.Add(new FieldContainer(0, 1, NumberFormatField.Integer));
            v2.Add(new FieldContainer(1, 2, NumberFormatField.DecimalSeparator));
            v2.Add(new FieldContainer(2, 5, NumberFormatField.Fraction));
            v2.Add(new FieldContainer(5, 6, NumberFormatField.ExponentSymbol));
            v2.Add(new FieldContainer(6, 7, NumberFormatField.ExponentSign));
            v2.Add(new FieldContainer(7, 8, NumberFormatField.Exponent));
            DecimalFormat fmt2 = new DecimalFormat("0.###E+0", US);

            numFmtted = fmt2.Format(number);
            iterator = fmt2.FormatToCharacterIterator(number);
            CompareAttributedCharacterFormatOutput(iterator, v2, numFmtted);

            // Third test. BigInteger with grouping separators.
            List<FieldContainer> v3 = new List<FieldContainer>(7);
            v3.Add(new FieldContainer(0, 1, NumberFormatField.Sign));
            v3.Add(new FieldContainer(1, 2, NumberFormatField.Integer));
            v3.Add(new FieldContainer(2, 3, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(3, 6, NumberFormatField.Integer));
            v3.Add(new FieldContainer(6, 7, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(7, 10, NumberFormatField.Integer));
            v3.Add(new FieldContainer(10, 11, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(11, 14, NumberFormatField.Integer));
            v3.Add(new FieldContainer(14, 15, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(15, 18, NumberFormatField.Integer));
            v3.Add(new FieldContainer(18, 19, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(19, 22, NumberFormatField.Integer));
            v3.Add(new FieldContainer(22, 23, NumberFormatField.GroupingSeparator));
            v3.Add(new FieldContainer(23, 26, NumberFormatField.Integer));
            BigInteger bigNumberInt = BigInteger.Parse("-1234567890246813579", radix: 10);
            String fmtNumberBigInt = outFmt.Format(bigNumberInt);

            iterator = outFmt.FormatToCharacterIterator(bigNumberInt);
            CompareAttributedCharacterFormatOutput(iterator, v3, fmtNumberBigInt);

            // Fourth test: BigDecimal with exponential formatting.
            List<FieldContainer> v4 = new List<FieldContainer>(7);
            v4.Add(new FieldContainer(0, 1, NumberFormatField.Sign));
            v4.Add(new FieldContainer(1, 2, NumberFormatField.Integer));
            v4.Add(new FieldContainer(2, 3, NumberFormatField.DecimalSeparator));
            v4.Add(new FieldContainer(3, 6, NumberFormatField.Fraction));
            v4.Add(new FieldContainer(6, 7, NumberFormatField.ExponentSymbol));
            v4.Add(new FieldContainer(7, 8, NumberFormatField.ExponentSign));
            v4.Add(new FieldContainer(8, 9, NumberFormatField.Exponent));

            Numerics.BigMath.BigDecimal numberBigD = new Numerics.BigMath.BigDecimal(-123456789);
            String fmtNumberBigDExp = fmt2.Format(numberBigD);

            iterator = fmt2.FormatToCharacterIterator(numberBigD);
            CompareAttributedCharacterFormatOutput(iterator, v4, fmtNumberBigDExp);

        }

#if FEATURE_IKVM

        // Test that the decimal is shown even when there are no fractional digits
        [Test]
        public void Test11621()// throws Exception
        {
            String pat = "0.##E0";

            DecimalFormatSymbols icuSym = new DecimalFormatSymbols(new CultureInfo("en-US"));
            DecimalFormat icuFmt = new DecimalFormat(pat, icuSym);
            icuFmt.DecimalSeparatorAlwaysShown = (true);
            String icu = ((NumberFormat)icuFmt).Format(299792458);

            java.text.DecimalFormatSymbols jdkSym = new java.text.DecimalFormatSymbols(java.util.Locale.US);
            java.text.DecimalFormat jdkFmt = new java.text.DecimalFormat(pat, jdkSym);
            jdkFmt.setDecimalSeparatorAlwaysShown(true);
            String jdk = ((java.text.NumberFormat)jdkFmt).format(299792458);

            assertEquals("ICU and JDK placement of decimal in exponent", jdk, icu);
        }

#endif

        private void checkFormatWithField(String testInfo, Formatter format, Object @object,
            String expected, FormatField field, int begin, int end)
        {
            StringBuffer buffer = new StringBuffer();
            FieldPosition pos = new FieldPosition(field);
            format.Format(@object, buffer, pos);

            assertEquals("Test " + testInfo + ": incorrect formatted text", expected, buffer.ToString());

            if (begin != pos.BeginIndex || end != pos.EndIndex)
            {
                assertEquals("Index mismatch", field + " " + begin + ".." + end,
                    pos.FieldAttribute + " " + pos.BeginIndex + ".." + pos.EndIndex);
            }
        }

        [Test]
        public void TestMissingFieldPositionsCurrency()
        {
            DecimalFormat formatter = (DecimalFormat)NumberFormat.GetCurrencyInstance(new UCultureInfo("en-US"));
            Number number = Double.GetInstance(92314587.66);
            String result = "$92,314,587.66";

            checkFormatWithField("currency", formatter, number, result,
                NumberFormatField.Currency, 0, 1);
            checkFormatWithField("integer", formatter, number, result,
                NumberFormatField.Integer, 1, 11);
            checkFormatWithField("grouping separator", formatter, number, result,
                NumberFormatField.GroupingSeparator, 3, 4);
            checkFormatWithField("decimal separator", formatter, number, result,
                NumberFormatField.DecimalSeparator, 11, 12);
            checkFormatWithField("fraction", formatter, number, result,
                NumberFormatField.Fraction, 12, 14);
        }

        [Test]
        public void TestMissingFieldPositionsNegativeDouble()
        {
            // test for exponential fields with double
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            Number number = Double.GetInstance(-12345678.90123);
            DecimalFormat formatter = new DecimalFormat("0.#####E+00", us_symbols);
            String numFmtted = formatter.Format(number);

            checkFormatWithField("sign", formatter, number, numFmtted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", formatter, number, numFmtted,
                NumberFormatField.Integer, 1, 2);
            checkFormatWithField("decimal separator", formatter, number, numFmtted,
                NumberFormatField.DecimalSeparator, 2, 3);
            checkFormatWithField("exponent symbol", formatter, number, numFmtted,
                NumberFormatField.ExponentSymbol, 8, 9);
            checkFormatWithField("exponent sign", formatter, number, numFmtted,
                NumberFormatField.ExponentSign, 9, 10);
            checkFormatWithField("exponent", formatter, number, numFmtted,
                NumberFormatField.Exponent, 10, 12);
        }

        [Test]
        public void TestMissingFieldPositionsPerCent()
        {
            // Check PERCENT
            DecimalFormat percentFormat = (DecimalFormat)NumberFormat.GetPercentInstance(new UCultureInfo("en-US"));
            Number number = Double.GetInstance(-0.986);
            String numberFormatted = percentFormat.Format(number);
            checkFormatWithField("sign", percentFormat, number, numberFormatted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", percentFormat, number, numberFormatted,
                NumberFormatField.Integer, 1, 3);
            checkFormatWithField("percent", percentFormat, number, numberFormatted,
                NumberFormatField.Percent, 3, 4);
        }

        [Test]
        public void TestMissingFieldPositionsPerCentPattern()
        {
            // Check PERCENT with more digits
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat fmtPercent = new DecimalFormat("0.#####%", us_symbols);
            Number number = Double.GetInstance(-0.986);
            String numFmtted = fmtPercent.Format(number);

            checkFormatWithField("sign", fmtPercent, number, numFmtted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", fmtPercent, number, numFmtted,
                NumberFormatField.Integer, 1, 3);
            checkFormatWithField("decimal separator", fmtPercent, number, numFmtted,
                NumberFormatField.DecimalSeparator, 3, 4);
            checkFormatWithField("fraction", fmtPercent, number, numFmtted,
                NumberFormatField.Fraction, 4, 5);
            checkFormatWithField("percent", fmtPercent, number, numFmtted,
                NumberFormatField.Percent, 5, 6);
        }

        [Test]
        public void TestMissingFieldPositionsPerMille()
        {
            // Check PERMILLE
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat fmtPerMille = new DecimalFormat("0.######‰", us_symbols);
            Number numberPermille = Double.GetInstance(-0.98654);
            String numFmtted = fmtPerMille.Format(numberPermille);

            checkFormatWithField("sign", fmtPerMille, numberPermille, numFmtted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", fmtPerMille, numberPermille, numFmtted,
                NumberFormatField.Integer, 1, 4);
            checkFormatWithField("decimal separator", fmtPerMille, numberPermille, numFmtted,
                NumberFormatField.DecimalSeparator, 4, 5);
            checkFormatWithField("fraction", fmtPerMille, numberPermille, numFmtted,
                NumberFormatField.Fraction, 5, 7);
            checkFormatWithField("permille", fmtPerMille, numberPermille, numFmtted,
                NumberFormatField.PerMille, 7, 8);
        }

        [Test]
        public void TestMissingFieldPositionsNegativeBigInt()
        {
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat formatter = new DecimalFormat("0.#####E+0", us_symbols);
            Number number = BigDecimal.Parse("-123456789987654321", CultureInfo.InvariantCulture);
            String bigDecFmtted = formatter.Format(number);

            checkFormatWithField("sign", formatter, number, bigDecFmtted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", formatter, number, bigDecFmtted,
                NumberFormatField.Integer, 1, 2);
            checkFormatWithField("decimal separator", formatter, number, bigDecFmtted,
                NumberFormatField.DecimalSeparator, 2, 3);
            checkFormatWithField("exponent symbol", formatter, number, bigDecFmtted,
                NumberFormatField.ExponentSymbol, 8, 9);
            checkFormatWithField("exponent sign", formatter, number, bigDecFmtted,
                NumberFormatField.ExponentSign, 9, 10);
            checkFormatWithField("exponent", formatter, number, bigDecFmtted,
                NumberFormatField.Exponent, 10, 12);
        }

        [Test]
        public void TestMissingFieldPositionsNegativeLong()
        {
            Number number = Long.GetInstance(Long.Parse("-123456789987654321", CultureInfo.InvariantCulture));
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat formatter = new DecimalFormat("0.#####E+0", us_symbols);
            String longFmtted = formatter.Format(number);

            checkFormatWithField("sign", formatter, number, longFmtted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", formatter, number, longFmtted,
                NumberFormatField.Integer, 1, 2);
            checkFormatWithField("decimal separator", formatter, number, longFmtted,
                NumberFormatField.DecimalSeparator, 2, 3);
            checkFormatWithField("exponent symbol", formatter, number, longFmtted,
                NumberFormatField.ExponentSymbol, 8, 9);
            checkFormatWithField("exponent sign", formatter, number, longFmtted,
                NumberFormatField.ExponentSign, 9, 10);
            checkFormatWithField("exponent", formatter, number, longFmtted,
                NumberFormatField.Exponent, 10, 12);
        }

        [Test]
        public void TestMissingFieldPositionsPositiveBigDec()
        {
            // Check complex positive;negative pattern.
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat fmtPosNegSign = new DecimalFormat("+0.####E+00;-0.#######E+0", us_symbols);
            Number positiveExp = Double.GetInstance(Double.Parse("9876543210", CultureInfo.InvariantCulture));
            String posExpFormatted = fmtPosNegSign.Format(positiveExp);

            checkFormatWithField("sign", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.Integer, 1, 2);
            checkFormatWithField("decimal separator", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.DecimalSeparator, 2, 3);
            checkFormatWithField("fraction", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.Fraction, 3, 7);
            checkFormatWithField("exponent symbol", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.ExponentSymbol, 7, 8);
            checkFormatWithField("exponent sign", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.ExponentSign, 8, 9);
            checkFormatWithField("exponent", fmtPosNegSign, positiveExp, posExpFormatted,
                NumberFormatField.Exponent, 9, 11);
        }

        [Test]
        public void TestMissingFieldPositionsNegativeBigDec()
        {
            // Check complex positive;negative pattern.
            DecimalFormatSymbols us_symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));
            DecimalFormat fmtPosNegSign = new DecimalFormat("+0.####E+00;-0.#######E+0", us_symbols);
            Number negativeExp = BigDecimal.Parse("-0.000000987654321083", CultureInfo.InvariantCulture);
            String negExpFormatted = fmtPosNegSign.Format(negativeExp);

            checkFormatWithField("sign", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.Sign, 0, 1);
            checkFormatWithField("integer", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.Integer, 1, 2);
            checkFormatWithField("decimal separator", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.DecimalSeparator, 2, 3);
            checkFormatWithField("fraction", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.Fraction, 3, 7);
            checkFormatWithField("exponent symbol", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.ExponentSymbol, 7, 8);
            checkFormatWithField("exponent sign", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.ExponentSign, 8, 9);
            checkFormatWithField("exponent", fmtPosNegSign, negativeExp, negExpFormatted,
                NumberFormatField.Exponent, 9, 11);
        }

        [Test]
        public void TestStringSymbols()
        {
            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new UCultureInfo("en-US"));

            // Attempt digits with multiple code points.
            string[] customDigits = { "(0)", "(1)", "(2)", "(3)", "(4)", "(5)", "(6)", "(7)", "(8)", "(9)" };
            symbols.DigitStrings = (customDigits);
            DecimalFormat fmt = new DecimalFormat("#,##0.0#", symbols);
            expect2(fmt, 1234567.89, "(1),(2)(3)(4),(5)(6)(7).(8)(9)");

            // Scientific notation should work.
            fmt.ApplyPattern("@@@E0");
            expect2(fmt, 1230000, "(1).(2)(3)E(6)");

            // Grouping and decimal with multiple code points are not supported during parsing.
            symbols.DecimalSeparatorString = ("~~");
            symbols.GroupingSeparatorString = ("^^");
            fmt.SetDecimalFormatSymbols(symbols);
            fmt.ApplyPattern("#,##0.0#");
            assertEquals("Custom decimal and grouping separator string with multiple characters",
                    "(1)^^(2)(3)(4)^^(5)(6)(7)~~(8)(9)", fmt.Format(1234567.89));

            // Digits starting at U+1D7CE MATHEMATICAL BOLD DIGIT ZERO
            // These are all single code points, so parsing will work.
            for (int i = 0; i < 10; i++) customDigits[i] = new string(Character.ToChars(0x1D7CE + i));
            symbols.DigitStrings = (customDigits);
            symbols.DecimalSeparatorString = ("😁");
            symbols.GroupingSeparatorString = ("😎");
            fmt.SetDecimalFormatSymbols(symbols);
            expect2(fmt, 1234.56, "𝟏😎𝟐𝟑𝟒😁𝟓𝟔");
        }

        [Test]
        public void TestArabicCurrencyPatternInfo()
        {
            UCultureInfo arLocale = new UCultureInfo("ar");

            DecimalFormatSymbols symbols = new DecimalFormatSymbols(arLocale);
            String currSpacingPatn = symbols.GetPatternForCurrencySpacing(CurrencySpacingPattern.CurrencyMatch, true);
            if (currSpacingPatn == null || currSpacingPatn.Length == 0)
            {
                Errln("locale ar, getPatternForCurrencySpacing returns null or 0-length string");
            }

            DecimalFormat currAcctFormat = (DecimalFormat)NumberFormat.GetInstance(arLocale, NumberFormatStyle.AccountingCurrencyStyle);
            String currAcctPatn = currAcctFormat.ToPattern();
            if (currAcctPatn == null || currAcctPatn.Length == 0)
            {
                Errln("locale ar, toPattern for ACCOUNTINGCURRENCYSTYLE returns null or 0-length string");
            }
        }

        [Test]
        public void TestMinMaxOverrides()
        /*throws IllegalAccessException, IllegalArgumentException, InvocationTargetException,
            NoSuchMethodException, SecurityException*/
        {
            Type[] baseClasses = { typeof(NumberFormat), typeof(NumberFormat), typeof(DecimalFormat) };
            string[] names = { "Integer", "Fraction", "Significant" };
            for (int i = 0; i < 3; i++)
            {
                DecimalFormat df = new DecimalFormat();
                Type @base = baseClasses[i];
                String name = names[i];
                MethodInfo getMinimum = @base.GetMethod("get_Minimum" + name + "Digits");
                MethodInfo setMinimum = @base.GetMethod("set_Minimum" + name + "Digits", new Type[] { typeof(int) });
                MethodInfo getMaximum = @base.GetMethod("get_Maximum" + name + "Digits");
                MethodInfo setMaximum = @base.GetMethod("set_Maximum" + name + "Digits", new Type[] { typeof(int) });


                // Check max overrides min
                setMinimum.Invoke(df, new object[] { 2 });
                assertEquals(name + " getMin A", 2, getMinimum.Invoke(df, new object[0]));
                setMaximum.Invoke(df, new object[] { 3 });
                assertEquals(name + " getMin B", 2, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax B", 3, getMaximum.Invoke(df, new object[0]));
                setMaximum.Invoke(df, new object[] { 2 });
                assertEquals(name + " getMin C", 2, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax C", 2, getMaximum.Invoke(df, new object[0]));
                setMaximum.Invoke(df, new object[] { 1 });
                assertEquals(name + " getMin D", 1, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax D", 1, getMaximum.Invoke(df, new object[0]));

                // Check min overrides max
                setMaximum.Invoke(df, new object[] { 2 });
                assertEquals(name + " getMax E", 2, getMaximum.Invoke(df, new object[0]));
                setMinimum.Invoke(df, new object[] { 1 });
                assertEquals(name + " getMin F", 1, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax F", 2, getMaximum.Invoke(df, new object[0]));
                setMinimum.Invoke(df, new object[] { 2 });
                assertEquals(name + " getMin G", 2, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax G", 2, getMaximum.Invoke(df, new object[0]));
                setMinimum.Invoke(df, new object[] { 3 });
                assertEquals(name + " getMin H", 3, getMinimum.Invoke(df, new object[0]));
                assertEquals(name + " getMax H", 3, getMaximum.Invoke(df, new object[0]));
            }
        }

        [Test]
        [Ignore("ICU4N TODO: This test fails due to the inaccuracy of BigDecimal.AproxPrecision(). See: https://github.com/openjdk/jdk8u-dev/blob/987c7384267be18fe86d3bd2514d389a5d62306c/jdk/src/share/classes/java/math/BigDecimal.java#L3869-L3886")]
        public void TestSetMathContext() //throws ParseException
        {
            Numerics.BigMath.MathContext fourDigits = new Numerics.BigMath.MathContext(4);
            Numerics.BigMath.MathContext unlimitedCeiling = new Numerics.BigMath.MathContext(0, Numerics.BigMath.RoundingMode.Ceiling);

            // Test rounding
            DecimalFormat df = new DecimalFormat();
            assertEquals("Default format", "9,876.543", df.Format(9876.5432));
            df.MathContext = (fourDigits);
            assertEquals("Format with fourDigits", "9,877", df.Format(9876.5432));
            df.MathContext = (unlimitedCeiling);
            assertEquals("Format with unlimitedCeiling", "9,876.544", df.Format(9876.5432));

            // Test multiplication
            df = new DecimalFormat("0.000%");
            assertEquals("Default multiplication", "12.001%", df.Format(0.120011));
            df.MathContext = (fourDigits);
            assertEquals("Multiplication with fourDigits", "12.000%", df.Format(0.120011));
            df.MathContext = (unlimitedCeiling);
            assertEquals("Multiplication with unlimitedCeiling", "12.002%", df.Format(0.120011));

            // Test simple division
            df = new DecimalFormat("0%");
            assertEquals("Default division", 0.12001, df.Parse("12.001%").ToDouble());
            df.MathContext = (fourDigits);
            assertEquals("Division with fourDigits", 0.12, df.Parse("12.001%").ToDouble());
            df.MathContext = (unlimitedCeiling);
            assertEquals("Division with unlimitedCeiling", 0.12001, df.Parse("12.001%").ToDouble());

            // Test extreme division
            df = new DecimalFormat();
            df.Multiplier = (1000000007); // prime number
            String hugeNumberString = "9876543212345678987654321234567898765432123456789"; // 49 digits
            BigInteger huge34Digits = BigInteger.Parse("9876543143209876985185182338271622000000", radix: 10);
            BigInteger huge4Digits = BigInteger.Parse("9877000000000000000000000000000000000000", radix: 10);
            assertEquals("Default extreme division", huge34Digits, df.Parse(hugeNumberString));
            df.MathContext = (fourDigits);
            assertEquals("Extreme division with fourDigits", huge4Digits, df.Parse(hugeNumberString));
            df.MathContext = (unlimitedCeiling);
            try
            {
                df.Parse(hugeNumberString);
                fail("Extreme division with unlimitedCeiling should throw ArithmeticException");
            }
            catch (ArithmeticException e)
            {
                // expected
            }
        }

        [Test]
        public void Test10436()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance(new CultureInfo("en"));
            df.RoundingMode = Numerics.BigMath.RoundingMode.Ceiling; //(MathContext.ROUND_CEILING);
            df.MinimumFractionDigits = (0);
            df.MaximumFractionDigits = (0);
            assertEquals("-.99 should round toward infinity", "-0", df.Format(-0.99));
        }

        [Test]
        public void Test10765()
        {
            NumberFormat fmt = NumberFormat.GetInstance(new UCultureInfo("en"));
            fmt.MinimumIntegerDigits = (10);
            FieldPosition pos = new FieldPosition(NumberFormatField.GroupingSeparator);
            StringBuffer sb = new StringBuffer();
            fmt.Format(1234567, sb, pos);
            assertEquals("Should have multiple grouping separators", "0,001,234,567", sb.ToString());
            assertEquals("FieldPosition should report the first occurence", 1, pos.BeginIndex);
            assertEquals("FieldPosition should report the first occurence", 2, pos.EndIndex);
        }

        [Test]
        public void Test10997()
        {
            NumberFormat fmt = NumberFormat.GetCurrencyInstance(new UCultureInfo("en-US"));
            fmt.MinimumFractionDigits = (4);
            fmt.MaximumFractionDigits = (4);
            String str1 = fmt.Format(new CurrencyAmount(123.45, Currency.GetInstance("USD")));
            String str2 = fmt.Format(new CurrencyAmount(123.45, Currency.GetInstance("EUR")));
            assertEquals("minFrac 4 should be respected in default currency", "$123.4500", str1);
            assertEquals("minFrac 4 should be respected in different currency", "€123.4500", str2);
        }

        [Test]
        public void Test11020()
        {
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new UCultureInfo("fr-FR"));
            DecimalFormat fmt = new DecimalFormat("0.05E0", sym);
            String result = fmt.Format(12301.2).Replace('\u00a0', ' ');
            assertEquals("Rounding increment should be applied after magnitude scaling", "1,25E4", result);
        }

        [Test]
        public void Test11025()
        {
            String pattern = "¤¤ **####0.00";
            DecimalFormatSymbols sym = new DecimalFormatSymbols(new UCultureInfo("fr-FR"));
            DecimalFormat fmt = new DecimalFormat(pattern, sym);
            String result = fmt.Format(433.0);
            assertEquals("Number should be padded to 11 characters", "EUR *433,00", result);
        }

        [Test]
        public void Test11640()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance();
            df.ApplyPattern("¤¤¤ 0");
            String result = df.PositivePrefix;
            assertEquals("Triple-currency should give long name on getPositivePrefix", "US dollar ", result);
        }

        [Test]
        public void Test11645()
        {
            String pattern = "#,##0.0#";
            DecimalFormat fmt = (DecimalFormat)NumberFormat.GetInstance();
            fmt.ApplyPattern(pattern);
            DecimalFormat fmtCopy;

            int newMultiplier = 37;
            fmtCopy = (DecimalFormat)fmt.Clone();
            assertNotEquals("Value before setter", fmtCopy.Multiplier, newMultiplier);
            fmtCopy.Multiplier = (newMultiplier);
            assertEquals("Value after setter", fmtCopy.Multiplier, newMultiplier);
            fmtCopy.ApplyPattern(pattern);
            assertEquals("Value after applyPattern", fmtCopy.Multiplier, newMultiplier);
            assertFalse("multiplier", fmt.Equals(fmtCopy));

            Numerics.BigMath.RoundingMode newRoundingMode = Numerics.BigMath.RoundingMode.Ceiling; // RoundingMode.CEILING.ordinal();
            fmtCopy = (DecimalFormat)fmt.Clone();
            assertNotEquals("Value before setter", fmtCopy.RoundingMode, newRoundingMode);
            fmtCopy.RoundingMode = (newRoundingMode);
            assertEquals("Value after setter", fmtCopy.RoundingMode, newRoundingMode);
            fmtCopy.ApplyPattern(pattern);
            assertEquals("Value after applyPattern", fmtCopy.RoundingMode, newRoundingMode);
            assertFalse("roundingMode", fmt.Equals(fmtCopy));

            Currency newCurrency = Currency.GetInstance("EAT");
            fmtCopy = (DecimalFormat)fmt.Clone();
            assertNotEquals("Value before setter", fmtCopy.Currency, newCurrency);
            fmtCopy.Currency = (newCurrency);
            assertEquals("Value after setter", fmtCopy.Currency, newCurrency);
            fmtCopy.ApplyPattern(pattern);
            assertEquals("Value after applyPattern", fmtCopy.Currency, newCurrency);
            assertFalse("currency", fmt.Equals(fmtCopy));

            CurrencyUsage newCurrencyUsage = CurrencyUsage.Cash;
            fmtCopy = (DecimalFormat)fmt.Clone();
            assertNotEquals("Value before setter", fmtCopy.CurrencyUsage, newCurrencyUsage);
            fmtCopy.CurrencyUsage = (CurrencyUsage.Cash);
            assertEquals("Value after setter", fmtCopy.CurrencyUsage, newCurrencyUsage);
            fmtCopy.ApplyPattern(pattern);
            assertEquals("Value after applyPattern", fmtCopy.CurrencyUsage, newCurrencyUsage);
            assertFalse("currencyUsage", fmt.Equals(fmtCopy));
        }

        [Test]
        public void Test11646()
        {
            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new UCultureInfo("en_US"));
            String pattern = "\u00a4\u00a4\u00a4 0.00 %\u00a4\u00a4";
            DecimalFormat fmt = new DecimalFormat(pattern, symbols);

            // Test equality with affixes. set affix methods can't capture special
            // characters which is why equality should fail.
            {
                DecimalFormat fmtCopy = (DecimalFormat)fmt.Clone();
                assertEquals("", fmt, fmtCopy);
                fmtCopy.PositivePrefix = (fmtCopy.PositivePrefix);
                assertNotEquals("", fmt, fmtCopy);
            }
            {
                DecimalFormat fmtCopy = (DecimalFormat)fmt.Clone();
                assertEquals("", fmt, fmtCopy);
                fmtCopy.PositiveSuffix = (fmtCopy.PositiveSuffix);
                assertNotEquals("", fmt, fmtCopy);
            }
            {
                DecimalFormat fmtCopy = (DecimalFormat)fmt.Clone();
                assertEquals("", fmt, fmtCopy);
                fmtCopy.NegativePrefix = (fmtCopy.NegativePrefix);
                assertNotEquals("", fmt, fmtCopy);
            }
            {
                DecimalFormat fmtCopy = (DecimalFormat)fmt.Clone();
                assertEquals("", fmt, fmtCopy);
                fmtCopy.NegativeSuffix = (fmtCopy.NegativeSuffix);
                assertNotEquals("", fmt, fmtCopy);
            }
        }

        [Test]
        public void Test11648()
        {
            DecimalFormat df = new DecimalFormat("0.00");
            df.UseScientificNotation = (true);
            String pat = df.ToPattern();
            assertEquals("A valid scientific notation pattern should be produced", "0.00E0", pat);
        }

        [Test]
        public void Test11649()
        {
            String pattern = "\u00a4\u00a4\u00a4 0.00";
            DecimalFormat fmt = new DecimalFormat(pattern);
            fmt.Currency = (Currency.GetInstance("USD"));
            assertEquals("Triple currency sign should format long name", "US dollars 12.34", fmt.Format(12.34));

            String newPattern = fmt.ToPattern();
            assertEquals("Should produce a valid pattern", pattern, newPattern);

            DecimalFormat fmt2 = new DecimalFormat(newPattern);
            fmt2.Currency = (Currency.GetInstance("USD"));
            assertEquals("Triple currency sign pattern should round-trip", "US dollars 12.34", fmt2.Format(12.34));

            String quotedPattern = "\u00a4\u00a4'\u00a4' 0.00";
            DecimalFormat fmt3 = new DecimalFormat(quotedPattern);
            assertEquals("Should be treated as double currency sign", "USD\u00a4 12.34", fmt3.Format(12.34));

            String outQuotedPattern = fmt3.ToPattern();
            assertEquals("Double currency sign with quoted sign should round-trip", quotedPattern, outQuotedPattern);
        }

        [Test]
        public void Test11686()
        {
            DecimalFormat df = new DecimalFormat();
            df.PositiveSuffix = ("0K");
            df.NegativeSuffix = ("0N");
            expect2(df, 123, "1230K");
            expect2(df, -123, "-1230N");
        }

        [Test]
        public void Test11839()
        {
            DecimalFormatSymbols dfs = new DecimalFormatSymbols(new UCultureInfo("en"));
            dfs.MinusSignString = ("a∸");
            dfs.PlusSignString = ("b∔"); //  ∔  U+2214 DOT PLUS
            DecimalFormat df = new DecimalFormat("0.00+;0.00-", dfs);
            String result = df.Format(-1.234);
            assertEquals("Locale-specific minus sign should be used", "1.23a∸", result);
            result = df.Format(1.234);
            assertEquals("Locale-specific plus sign should be used", "1.23b∔", result);
            // Test round-trip with parse
            expect2(df, -456, "456.00a∸");
            expect2(df, 456, "456.00b∔");
        }

        [Test]
        public void Test12753()
        {
            UCultureInfo locale = new UCultureInfo("en-US");
            DecimalFormatSymbols symbols = DecimalFormatSymbols.GetInstance(locale);
            symbols.DecimalSeparator = ('*');
            DecimalFormat df = new DecimalFormat("0.00", symbols);
            df.DecimalPatternMatchRequired = (true);
            try
            {
                df.Parse("123");
                fail("Parsing integer succeeded even though setDecimalPatternMatchRequired was set");
            }
            catch (FormatException e)
            {
                // Parse failed (expected)
            }
        }

        [Test]
        public void Test12962()
        {
            String pat = "**0.00";
            DecimalFormat df = new DecimalFormat(pat);
            String newPat = df.ToPattern();
            assertEquals("Format width changed upon calling applyPattern", pat.Length, newPat.Length);
        }

        [Test]
        public void Test10354()
        {
            DecimalFormatSymbols dfs = new DecimalFormatSymbols();
            dfs.NaN = ("");
            DecimalFormat df = new DecimalFormat();
            df.SetDecimalFormatSymbols(dfs);
            try
            {
                df.FormatToCharacterIterator(Double.GetInstance(double.NaN));
                // pass
            }
            catch (ArgumentException e)
            {
                throw new AssertionException(e.ToString(), e);
            }
        }

        [Test]
        public void Test11913()
        {
            NumberFormat df = DecimalFormat.GetInstance();
            String result = df.Format(BigDecimal.Parse("1.23456789E400", CultureInfo.InvariantCulture));
            assertEquals("Should format more than 309 digits", "12,345,678", result.Substring(0, 10)); // ICU4N Checked 2nd arg
            assertEquals("Should format more than 309 digits", 534, result.Length);
        }

        [Test]
        public void Test12045()
        {
            if (logKnownIssue("12045", "XSU is missing from fr")) { return; }

            NumberFormat nf = NumberFormat.GetInstance(new UCultureInfo("fr"), NumberFormatStyle.PluralCurrencyStyle);
            ParsePosition ppos = new ParsePosition(0);
            try
            {
                CurrencyAmount result = nf.ParseCurrency("2,34 XSU", ppos);
                assertEquals("Parsing should succeed on XSU",
                             new CurrencyAmount(2.34, Currency.GetInstance("XSU")), result);
                // pass
            }
            catch (Exception e)
            {
                //throw new AssertionError("Should have been able to parse XSU", e);
                throw new AssertionException("Should have been able to parse XSU: " + e.Message, e);
            }
        }

        [Test]
        public void Test11739()
        {
            NumberFormat nf = NumberFormat.GetCurrencyInstance(new UCultureInfo("sr_BA"));
            ((DecimalFormat)nf).ApplyPattern("#,##0.0 ¤¤¤");
            ParsePosition ppos = new ParsePosition(0);
            CurrencyAmount result = nf.ParseCurrency("1.500 амерички долар", ppos);
            assertEquals("Should parse to 1500 USD", new CurrencyAmount(1500, Currency.GetInstance("USD")), result);
        }

        [Test]
        public void Test11647()
        {
            DecimalFormat df = new DecimalFormat();
            df.ApplyPattern("¤¤¤¤#");
            String actual = df.Format(123);
            assertEquals("Should replace 4 currency signs with U+FFFD", "\uFFFD123", actual);
        }

        [Test]
        public void Test12567()
        {
            DecimalFormat df1 = (DecimalFormat)NumberFormat.GetInstance(NumberFormatStyle.PluralCurrencyStyle);
            DecimalFormat df2 = (DecimalFormat)NumberFormat.GetInstance(NumberFormatStyle.NumberStyle);
            df2.Currency = (df1.Currency);
            df2.CurrencyPluralInfo = (df1.CurrencyPluralInfo);
            df1.ApplyPattern("0.00");
            df2.ApplyPattern("0.00");
            assertEquals("df1 == df2", df1, df2);
            assertEquals("df2 == df1", df2, df1);
            df2.PositivePrefix = ("abc");
            assertNotEquals("df1 != df2", df1, df2);
            assertNotEquals("df2 != df1", df2, df1);
        }

        [Test]
        public void Test13055()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetPercentInstance();
            df.MaximumFractionDigits = (0);
            df.RoundingMode = Numerics.BigMath.RoundingMode.HalfEven; //(BigDecimal.ROUND_HALF_EVEN);
            assertEquals("Should round percent toward even number", "216%", df.Format(2.155));
        }

        [Test]
        public void Test13056()
        {
            DecimalFormat df = new DecimalFormat("#,##0");
            assertEquals("Primary grouping should return 3", 3, df.GroupingSize);
            assertEquals("Secondary grouping should return 0", 0, df.SecondaryGroupingSize);
            df.SecondaryGroupingSize = (3);
            assertEquals("Primary grouping should still return 3", 3, df.GroupingSize);
            assertEquals("Secondary grouping should still return 0", 0, df.SecondaryGroupingSize);
            df.GroupingSize = (4);
            assertEquals("Primary grouping should return 4", 4, df.GroupingSize);
            assertEquals("Secondary should remember explicit setting and return 3", 3, df.SecondaryGroupingSize);
        }

        [Test]
        public void Test13074()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetCurrencyInstance(new UCultureInfo("bg-BG"));
            String result = df.Format(987654.321);
            assertEquals("Locale 'bg' should not use monetary grouping", "987654,32 лв.", result);
        }

        [Test]
        public void Test13088and13162()
        {
            UCultureInfo loc = new UCultureInfo("fa");
            String pattern1 = "%\u00A0#,##0;%\u00A0-#,##0";
            double num = -12.34;
            DecimalFormatSymbols symbols = DecimalFormatSymbols.GetInstance(loc);
            // If the symbols ever change in locale data, please call the setters so that this test
            // continues to use the old symbols.
            // The fa percent symbol does change in CLDR 32, so....
            symbols.PercentString = ("‎٪");
            assertEquals("Checking for expected symbols", "‎−", symbols.MinusSignString);
            assertEquals("Checking for expected symbols", "‎٪", symbols.PercentString);
            DecimalFormat numfmt = new DecimalFormat(pattern1, symbols);
            expect2(numfmt, num, "‎٪ ‎−۱٬۲۳۴");
            String pattern2 = "%#,##0;%-#,##0";
            numfmt = new DecimalFormat(pattern2, symbols);
            expect2(numfmt, num, "‎٪‎−۱٬۲۳۴");
        }

        [Test]
        public void Test13113_MalformedPatterns()
        {
            string[][] cases = {
                new string[] {"'", "quoted literal"},
                new string[] {"ab#c'd", "quoted literal"},
                new string[] {"ab#c*", "unquoted literal"},
                new string[] {"0#", "# cannot follow 0"},
                new string[] {".#0", "0 cannot follow #"},
                new string[] {"@0", "Cannot mix @ and 0"},
                new string[] {"0@", "Cannot mix 0 and @"},
                new string[] {"#x#", "unquoted special character"},
                new string[] {"@#@", "# inside of a run of @"},
        };
            foreach (string[] cas in cases)
            {
                try
                {
                    new DecimalFormat(cas[0]);
                    fail("Should have thrown on malformed pattern");
                }
                catch (FormatException ex) // ICU4N: Changed from ArgumentException to FormatException, since this is a parse error
                {
                    assertTrue("Exception should contain \"Malformed pattern\": " + ex.Message,
                            ex.Message.Contains("Malformed pattern"));
                    assertTrue("Exception should contain \"" + cas[1] + "\"" + ex.Message,
                            ex.Message.Contains(cas[1]));
                }
            }
        }

        [Test]
        public void Test13118()
        {
            DecimalFormat df = new DecimalFormat("@@@");
            df.UseScientificNotation = (true);
            for (double d = 12345.67; d > 1e-6; d /= 10)
            {
                String result = df.Format(d);
                assertEquals("Should produce a string of expected length on " + d,
                        d > 1 ? 6 : 7, result.Length);
            }
        }

        [Test]
        public void Test13148()
        {
            if (logKnownIssue("13148", "Currency separators used in non-currency parsing")) return;
            DecimalFormat fmt = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("en-ZA"));
            DecimalFormatSymbols symbols = fmt.GetDecimalFormatSymbols();
            symbols.DecimalSeparator = ('.');
            symbols.GroupingSeparator = (',');
            fmt.SetDecimalFormatSymbols(symbols);
            ParsePosition ppos = new ParsePosition(0);
            Number number = fmt.Parse("300,000", ppos);
            assertEquals("Should parse to 300000 using non-monetary separators: " + ppos, 300000L, number);
        }

        [Test]
        public void Test13289()
        {
            DecimalFormat df = new DecimalFormat("#00.0#E0");
            String result = df.Format(0.00123);
            assertEquals("Should ignore scientific minInt if maxInt>minInt", "1.23E-3", result);
        }

        [Test]
        public void Test13310()
        {
            // Note: if minInt > 8, then maxInt can be greater than 8.
            assertEquals("Should not throw an assertion error",
                    "100000007.6E-1",
                    new DecimalFormat("000000000.0#E0").Format(10000000.76d));
        }

        [Test]
        public void Test13391() //throws ParseException
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance(new UCultureInfo("ccp"));
            df.ParseStrict = (true);
            String expected = "\uD804\uDD37\uD804\uDD38,\uD804\uDD39\uD804\uDD3A\uD804\uDD3B";
            assertEquals("Should produce expected output in ccp", expected, df.Format(12345));
            Number result = df.Parse(expected);
            assertEquals("Should parse to 12345 in ccp", 12345, result.ToInt64());

            df = (DecimalFormat)NumberFormat.GetScientificInstance(new UCultureInfo("ccp"));
            df.ParseStrict = (true);
            String expectedScientific = "\uD804\uDD37.\uD804\uDD39E\uD804\uDD38";
            assertEquals("Should produce expected scientific output in ccp",
                    expectedScientific, df.Format(130));
            Number resultScientific = df.Parse(expectedScientific);
            assertEquals("Should parse scientific to 130 in ccp",
                    130, resultScientific.ToInt64());
        }

        [Test]
        public void testPercentZero()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetPercentInstance();
            String actual = df.Format(0);
            assertEquals("Should have one zero digit", "0%", actual);
        }

        [Test]
        public void testCurrencyZeroRounding()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetCurrencyInstance();
            df.MaximumFractionDigits = (0);
            String actual = df.Format(0);
            assertEquals("Should have zero fraction digits", "$0", actual);
        }

        [Test]
        public void testCustomCurrencySymbol()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetCurrencyInstance();
            df.Currency = (Currency.GetInstance("USD"));
            DecimalFormatSymbols symbols = df.GetDecimalFormatSymbols();
            symbols.CurrencySymbol = ("#");
            df.SetDecimalFormatSymbols(symbols);
            String actual = df.Format(123);
            assertEquals("Should use '#' instad of '$'", "# 123.00", actual);
        }

        [Test]
        [Ignore("ICU4N TODO: Serialization")]
        public void TestBasicSerializationRoundTrip() /* throws IOException, ClassNotFoundException */
        {
            //    DecimalFormat df0 = new DecimalFormat("A-**#####,#00.00b¤");

            //    // Write to byte stream
            //    ByteArrayOutputStream baos = new ByteArrayOutputStream();
            //    ObjectOutputStream oos = new ObjectOutputStream(baos);
            //    oos.writeObject(df0);
            //    oos.flush();
            //    baos.close();
            //    byte[] bytes = baos.toByteArray();

            //    // Read from byte stream
            //    ObjectInputStream ois = new ObjectInputStream(new ByteArrayInputStream(bytes));
            //    Object obj = ois.readObject();
            //    ois.close();
            //    DecimalFormat df1 = (DecimalFormat)obj;

            //    // Test equality
            //    assertEquals("Did not round-trip through serialization", df0, df1);

            //    // Test basic functionality
            //    String str0 = df0.Format(12345.67);
            //    String str1 = df1.Format(12345.67);
            //    assertEquals("Serialized formatter does not produce same output", str0, str1);
        }

        [Test]
        public void testGetSetCurrency()
        {
            DecimalFormat df = new DecimalFormat("¤#");
            assertEquals("Currency should start out null", null, df.Currency);
            Currency curr = Currency.GetInstance("EUR");
            df.Currency = (curr);
            assertEquals("Currency should equal EUR after set", curr, df.Currency);
            String result = df.Format(123);
            assertEquals("Currency should format as expected in EUR", "€123.00", result);
        }

        [Test]
        public void testRoundingModeSetters()
        {
            DecimalFormat df1 = new DecimalFormat();
            DecimalFormat df2 = new DecimalFormat();

            df1.RoundingMode = Numerics.BigMath.RoundingMode.Ceiling; // (java.math.BigDecimal.ROUND_CEILING);
            assertNotEquals("Rounding mode was set to a non-default", df1, df2);
            df2.RoundingMode = Numerics.BigMath.RoundingMode.Ceiling; // (com.ibm.icu.math.BigDecimal.ROUND_CEILING); // ICU4N TODO: Make this into a set method?
            assertEquals("Rounding mode from icu.math and java.math should be the same", df1, df2);
            df2.RoundingMode = Numerics.BigMath.RoundingMode.Ceiling; // (java.math.RoundingMode.CEILING.ordinal());
            assertEquals("Rounding mode ordinal from java.math.RoundingMode should be the same", df1, df2);
        }

        [Test]
        public void testCurrencySignificantDigits()
        {
            UCultureInfo locale = new UCultureInfo("en-US");
            DecimalFormat df = (DecimalFormat)NumberFormat.GetCurrencyInstance(locale);
            df.MaximumSignificantDigits = (2);
            String result = df.Format(1234);
            assertEquals("Currency rounding should obey significant digits", "$1,200", result);
        }

        [Test]
        public void testParseStrictScientific()
        {
            // See ticket #13057
            DecimalFormat df = (DecimalFormat)NumberFormat.GetScientificInstance();
            df.ParseStrict = (true);
            ParsePosition ppos = new ParsePosition(0);
            Number result0 = df.Parse("123E4", ppos);
            assertEquals("Should accept number with exponent", 1230000L, result0.ToInt64());
            assertEquals("Should consume the whole number", 5, ppos.Index);
            ppos.Index = (0);
            result0 = df.Parse("123", ppos);
            assertNull("Should reject number without exponent", result0);
            ppos.Index = (0);
            CurrencyAmount result1 = df.ParseCurrency("USD123", ppos);
            assertNull("Should reject currency without exponent", result1);
        }

        [Test]
        public void testParseLenientScientific()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetScientificInstance();
            ParsePosition ppos = new ParsePosition(0);
            Number result0 = df.Parse("123E", ppos);
            assertEquals("Should parse the number in lenient mode", 123L, result0.ToInt64());
            assertEquals("Should stop before the E", 3, ppos.Index);
            DecimalFormatSymbols dfs = df.GetDecimalFormatSymbols();
            dfs.ExponentSeparator = ("EE");
            df.SetDecimalFormatSymbols(dfs);
            ppos.Index = (0);
            result0 = df.Parse("123EE", ppos);
            assertEquals("Should parse the number in lenient mode", 123L, result0.ToInt64());
            assertEquals("Should stop before the EE", 3, ppos.Index);
        }

        [Test]
        public void testParseAcceptAsciiPercentPermilleFallback()
        {
            UCultureInfo loc = new UCultureInfo("ar");
            DecimalFormat df = (DecimalFormat)NumberFormat.GetPercentInstance(loc);
            ParsePosition ppos = new ParsePosition(0);
            Number result = df.Parse("42%", ppos);
            assertEquals("Should parse as 0.42 even in ar", BigDecimal.Parse("0.42", CultureInfo.InvariantCulture), result);
            assertEquals("Should consume the entire string even in ar", 3, ppos.Index);
            // TODO: Is there a better way to make a localized permille formatter?
            df.ApplyPattern(df.ToPattern().Replace("%", "‰"));
            ppos.Index = (0);
            result = df.Parse("42‰", ppos);
            assertEquals("Should parse as 0.042 even in ar", BigDecimal.Parse("0.042", CultureInfo.InvariantCulture), result);
            assertEquals("Should consume the entire string even in ar", 3, ppos.Index);
        }

        [Test]
        public void testParseSubtraction()
        {
            // TODO: Is this a case we need to support? It prevents us from automatically parsing
            // minus signs that appear after the number, like  in "12-" vs "-12".
            DecimalFormat df = new DecimalFormat();
            String str = "12 - 5";
            ParsePosition ppos = new ParsePosition(0);
            Number n1 = df.Parse(str, ppos);
            Number n2 = df.Parse(str, ppos);
            assertEquals("Should parse 12 and -5", 7, n1.ToInt32() + n2.ToInt32());
        }

        [Test]
        public void testSetPrefixDefaultSuffix()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetPercentInstance();
            df.PositivePrefix = ("+");
            assertEquals("Should have manual plus sign and auto percent sign", "+100%", df.Format(1));
        }

        [Test]
        public void testMultiCodePointPaddingInPattern()
        {
            DecimalFormat df = new DecimalFormat("a*'நி'###0b");
            String result = df.Format(12);
            assertEquals("Multi-codepoint padding should not be split", "aநிநி12b", result);
            df = new DecimalFormat("a*😁###0b");
            result = df.Format(12);
            assertEquals("Single-codepoint padding should not be split", "a😁😁12b", result);
            df = new DecimalFormat("a*''###0b");
            result = df.Format(12);
            assertEquals("Quote should be escapable in padding syntax", "a''12b", result);
        }

        [Test]
        public void testParseAmbiguousAffixes()
        {
            BigDecimal positive = BigDecimal.Parse("0.0567", CultureInfo.InvariantCulture);
            BigDecimal negative = BigDecimal.Parse("-0.0567", CultureInfo.InvariantCulture);
            DecimalFormat df = new DecimalFormat();
            df.ParseToBigDecimal = (true);

            string[] patterns = { "+0.00%;-0.00%", "+0.00%;0.00%", "0.00%;-0.00%" };
            string[] inputs = { "+5.67%", "-5.67%", "5.67%" };
            bool[][] expectedPositive = {
                new bool[] { true, false, true },
                new bool[] { true, false, false },
                new bool[] { true, false, true }
        };

            for (int i = 0; i < patterns.Length; i++)
            {
                String pattern = patterns[i];
                df.ApplyPattern(pattern);
                for (int j = 0; j < inputs.Length; j++)
                {
                    String input = inputs[j];
                    ParsePosition ppos = new ParsePosition(0);
                    Number actual = df.Parse(input, ppos);
                    BigDecimal expected = expectedPositive[i][j] ? positive : negative;
                    String message = "Pattern " + pattern + " with input " + input;
                    assertEquals(message, expected, actual);
                    assertEquals(message, input.Length, ppos.Index);
                }
            }
        }

        [Test]
        public void testParseIgnorables()
        {
            // Also see the test case "test parse ignorables" in numberformattestspecification.txt
            DecimalFormatSymbols dfs = DecimalFormatSymbols.GetInstance();
            dfs.PercentString = ("\u200E%\u200E");
            DecimalFormat df = new DecimalFormat("0 %;-0a", dfs);
            ParsePosition ppos = new ParsePosition(0);
            Number result = df.Parse("42\u200E%\u200E ", ppos);
            assertEquals("Should parse as percentage", BigDecimal.Parse("0.42", CultureInfo.InvariantCulture), result);
            assertEquals("Should consume the trailing bidi since it is in the symbol", 5, ppos.Index);
            ppos.Index = (0);
            result = df.Parse("-42a\u200E ", ppos);
            assertEquals("Should parse as percent", BigDecimal.Parse("-0.42", CultureInfo.InvariantCulture), result);
            assertEquals("Should not consume the trailing bidi or whitespace", 4, ppos.Index);

            // A few more cases based on the docstring:
            expect(df, "42%", 0.42);
            expect(df, "42 %", 0.42);
            expect(df, "42   %", 0.42);
            expect(df, "42\u00A0%", 0.42);
        }

        [Test]
        public void testCustomCurrencyUsageOverridesPattern()
        {
            DecimalFormat df = new DecimalFormat("#,##0.###");
            expect2(df, 1234, "1,234");
            df.CurrencyUsage = (CurrencyUsage.Standard);
            expect2(df, 1234, "1,234.00");
            df.CurrencyUsage = (null);
            expect2(df, 1234, "1,234");
        }

        [Test]
        public void testCurrencyUsageFractionOverrides() // ICU4N TODO: This test is very slow
        {
            NumberFormat df = DecimalFormat.GetCurrencyInstance(new UCultureInfo("en-US"));
            expect2(df, 35.0, "$35.00");
            df.MinimumFractionDigits = (3);
            expect2(df, 35.0, "$35.000");
            df.MaximumFractionDigits = (3);
            expect2(df, 35.0, "$35.000");
            df.MinimumFractionDigits = (-1);
            expect2(df, 35.0, "$35");
            df.MaximumFractionDigits = (-1);
            expect2(df, 35.0, "$35.00");
        }

        [Test]
        public void testParseVeryVeryLargeExponent()
        {
            DecimalFormat df = new DecimalFormat();
            ParsePosition ppos = new ParsePosition(0);

            object[][] cases = {
                new object[] {"1.2E+1234567890", Double.GetInstance(double.PositiveInfinity) },
                new object[] {"1.2E+999999999", BigDecimal.Parse("1.2E+999999999", CultureInfo.InvariantCulture)},
                new object[] {"1.2E+1000000000", Double.GetInstance(double.PositiveInfinity) },
                new object[] {"-1.2E+999999999", BigDecimal.Parse("-1.2E+999999999", CultureInfo.InvariantCulture) },
                new object[] {"-1.2E+1000000000", Double.GetInstance(double.NegativeInfinity) },
                new object[] {"1.2E-999999999", BigDecimal.Parse("1.2E-999999999", CultureInfo.InvariantCulture) },
                new object[] {"1.2E-1000000000", Double.GetInstance(0.0) },
                new object[] {"-1.2E-999999999", BigDecimal.Parse("-1.2E-999999999", CultureInfo.InvariantCulture) },
                new object[] {"-1.2E-1000000000", Double.GetInstance(-0.0) },

            };

            foreach (object[] cas in cases)
            {
                ppos.Index = (0);
                string input = (string)cas[0];
                Number expected = (Number)cas[1];
                Number actual = df.Parse(input, ppos);
                assertEquals(input, expected, actual);
            }
        }

        [Test]
        public void testStringMethodsNPE()
        {
            string[] npeMethods = {
                            "ApplyLocalizedPattern",
                            "ApplyPattern",
                            "set_NegativePrefix",
                            "set_NegativeSuffix",
                            "set_PositivePrefix",
                            "set_PositiveSuffix"
                    };
            foreach (string npeMethod in npeMethods)
            {
                DecimalFormat df = new DecimalFormat();
                try
                {
                    //DecimalFormat.class.getDeclaredMethod(npeMethod, String.class).invoke(df, (String)null);
                    typeof(DecimalFormat).GetMethod(npeMethod, new Type[] { typeof(string) }).Invoke(df, new object[] { (string)null });
                    fail("NullPointerException not thrown in method " + npeMethod);
                }
                catch (TargetInvocationException e)
                {
                    assertTrue("Exception should be NullPointerException in method " + npeMethod,
                            e.InnerException is ArgumentNullException);
                }
                catch (Exception e)
                {
                    // Other reflection exceptions
                    //throw new AssertionError("Reflection error in method " + npeMethod + ": " + e.getMessage());
                    throw new AssertionException("Reflection error in method " + npeMethod + ": " + e.Message);
                }
            }

            // Also test the constructors
            try
            {
                new DecimalFormat(null);
                fail("NullPointerException not thrown in 1-parameter constructor");
            }
            catch (ArgumentNullException e)
            {
                // Expected
            }
            try
            {
                new DecimalFormat(null, new DecimalFormatSymbols());
                fail("NullPointerException not thrown in 2-parameter constructor");
            }
            catch (ArgumentNullException e)
            {
                // Expected
            }
            try
            {
                new DecimalFormat(null, new DecimalFormatSymbols(), CurrencyPluralInfo.GetInstance(), 0);
                fail("NullPointerException not thrown in 4-parameter constructor");
            }
            catch (ArgumentNullException e)
            {
                // Expected
            }
        }

        [Test]
        public void testParseGroupingMode()
        {
            UCultureInfo[] locales = {         // GROUPING   DECIMAL
                new UCultureInfo("en-US"), // comma      period
                new UCultureInfo("fr-FR"), // space      comma
                new UCultureInfo("de-CH"), // apostrophe period
                new UCultureInfo("es-PY")  // period     comma
            };
            string[] inputs = {
                "12,345.67",
                "12 345,67",
                "12'345.67",
                "12.345,67",
                "12,345",
                "12 345",
                "12'345",
                "12.345"
            };
            BigDecimal[] outputs = {
                BigDecimal.Parse("12345.67", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345.67", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345.67", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345.67", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345", CultureInfo.InvariantCulture),
                BigDecimal.Parse("12345", CultureInfo.InvariantCulture)
            };
            int[][] expecteds = {
                // 0 => works in neither default nor restricted
                // 1 => works in default but not restricted
                // 2 => works in restricted but not default (should not happen)
                // 3 => works in both default and restricted
                //
                // C=comma, P=period, S=space, A=apostrophe
                // C+P    S+C    A+P    P+C    C-only  S-only   A-only   P-only
                new int[] {  3,     0,     1,     0,     3,      1,       1,       0  }, // => en-US
                new int[] {  0,     3,     0,     1,     0,      3,       3,       1  }, // => fr-FR
                new int[] {  1,     0,     3,     0,     1,      3,       3,       0  }, // => de-CH
                new int[] {  0,     1,     0,     3,     0,      1,       1,       3  }  // => es-PY
            };

            for (int i = 0; i < locales.Length; i++)
            {
                UCultureInfo loc = locales[i];
                DecimalFormat df = (DecimalFormat)NumberFormat.GetInstance(loc);
                df.ParseToBigDecimal = (true);
                for (int j = 0; j < inputs.Length; j++)
                {
                    String input = inputs[j];
                    BigDecimal output = outputs[j];
                    int expected = expecteds[i][j];

                    // TODO(sffc): Uncomment after ICU 60 API proposal
                    //df.setParseGroupingMode(null);
                    //assertEquals("Getter should return null", null, df.getParseGroupingMode());
                    ParsePosition ppos = new ParsePosition(0);
                    Number result = df.Parse(input, ppos);
                    bool actualNull = output.Equals(result) && (ppos.Index == input.Length);
                    assertEquals("Locale " + loc + ", string \"" + input + "\", DEFAULT, "
                            + "actual result: " + result + " (ppos: " + ppos.Index + ")",
                            (expected & 1) != 0, actualNull);

                    // TODO(sffc): Uncomment after ICU 60 API proposal
                    //df.setParseGroupingMode(GroupingMode.DEFAULT);
                    //assertEquals("Getter should return new value", GroupingMode.DEFAULT, df.getParseGroupingMode());
                    //ppos = new ParsePosition(0);
                    //result = df.Parse(input, ppos);
                    //bool actualDefault = output.equals(result) && (ppos.Index == input.Length);
                    //assertEquals("Result from null should be the same as DEFAULT", actualNull, actualDefault);

                    // TODO(sffc): Uncomment after ICU 60 API proposal
                    //df.setParseGroupingMode(GroupingMode.RESTRICTED);
                    //assertEquals("Getter should return new value", GroupingMode.RESTRICTED, df.getParseGroupingMode());
                    //ppos = new ParsePosition(0);
                    //result = df.Parse(input, ppos);
                    //bool actualRestricted = output.equals(result) && (ppos.Index == input.Length);
                    //assertEquals("Locale " + loc + ", string \"" + input + "\", RESTRICTED, "
                    //        + "actual result: " + result + " (ppos: " + ppos.Index + ")",
                    //        (expected & 2) != 0, actualRestricted);
                }
            }
        }

        [Test]
        public void testParseNoExponent() //throws ParseException
        {
            DecimalFormat df = new DecimalFormat();
            assertEquals("Parse no exponent has wrong default", false, df.ParseNoExponent);
            Number result1 = df.Parse("123E4");
            df.ParseNoExponent = (true);
            assertEquals("Parse no exponent getter is broken", true, df.ParseNoExponent);
            Number result2 = df.Parse("123E4");
            assertEquals("Exponent did not parse before setParseNoExponent", result1, Long.GetInstance(1230000));
            assertEquals("Exponent parsed after setParseNoExponent", result2, Long.GetInstance(123));
        }

        [Test]
        public void testMinimumGroupingDigits()
        {
            string[][] allExpected = {
                new string[] {"123", "123"},
                new string[] {"1,230", "1230"},
                new string[] {"12,300", "12,300"},
                new string[] {"1,23,000", "1,23,000"}
        };

            DecimalFormat df = new DecimalFormat("#,##,##0");
            assertEquals("Minimum grouping digits has wrong default", 1, df.MinimumGroupingDigits);

            for (int l = 123, i = 0; l <= 123000; l *= 10, i++)
            {
                df.MinimumGroupingDigits = (1);
                assertEquals("Minimum grouping digits getter is broken", 1, df.MinimumGroupingDigits);
                String actual = df.Format(l);
                assertEquals("Output is wrong for 1, " + i, allExpected[i][0], actual);
                df.MinimumGroupingDigits = (2);
                assertEquals("Minimum grouping digits getter is broken", 2, df.MinimumGroupingDigits);
                actual = df.Format(l);
                assertEquals("Output is wrong for 2, " + i, allExpected[i][1], actual);
            }
        }

        [Test]
        public void testParseCaseSensitive()
        {
            string[] patterns = { "a#b", "A#B" };
            string[] inputs = { "a500b", "A500b", "a500B", "a500e10b", "a500E10b" };
            int[][] expectedParsePositions = {
                new int[] {5, 5, 5, 8, 8}, // case insensitive, pattern 0
                new int[] {5, 0, 4, 4, 8}, // case sensitive, pattern 0
                new int[] {5, 5, 5, 8, 8}, // case insensitive, pattern 1
                new int[] {0, 4, 0, 0, 0}, // case sensitive, pattern 1
        };

            for (int p = 0; p < patterns.Length; p++)
            {
                String pat = patterns[p];
                DecimalFormat df = new DecimalFormat(pat);
                assertEquals("parseCaseSensitive default is wrong", false, df.ParseCaseSensitive);
                for (int i = 0; i < inputs.Length; i++)
                {
                    String inp = inputs[i];
                    df.ParseCaseSensitive = (false);
                    assertEquals("parseCaseSensitive getter is broken", false, df.ParseCaseSensitive);
                    ParsePosition actualInsensitive = new ParsePosition(0);
                    df.Parse(inp, actualInsensitive);
                    assertEquals("Insensitive, pattern " + p + ", input " + i,
                            expectedParsePositions[p * 2][i], actualInsensitive.Index);
                    df.ParseCaseSensitive = (true);
                    assertEquals("parseCaseSensitive getter is broken", true, df.ParseCaseSensitive);
                    ParsePosition actualSensitive = new ParsePosition(0);
                    df.Parse(inp, actualSensitive);
                    assertEquals("Sensitive, pattern " + p + ", input " + i,
                            expectedParsePositions[p * 2 + 1][i], actualSensitive.Index);
                }
            }
        }

        [Test]
        public void testPlusSignAlwaysShown() //throws ParseException
        {
            double[] numbers = { 0.012, 5.78, 0, -0.012, -5.78 };
            UCultureInfo[] locs = { new UCultureInfo("en-US"), new UCultureInfo("ar-EG"), new UCultureInfo("es-CL") };
            string[][][] expecteds = {
                // en-US
                new string[][] {
                    // decimal
                    new string[] { "+0.012", "+5.78", "+0", "-0.012", "-5.78" },
                    // currency
                    new string[] { "+$0.01", "+$5.78", "+$0.00", "-$0.01", "-$5.78" }
                },
                // ar-EG (interesting because the plus sign string starts with \u061C)
                new string[][] {
                    // decimal
                    new string[] {
                        "\u061C+\u0660\u066B\u0660\u0661\u0662", // "؜+٠٫٠١٢"
                        "\u061C+\u0665\u066B\u0667\u0668", // "؜+٥٫٧٨"
                        "\u061C+\u0660", // "؜+٠"
                        "\u061C-\u0660\u066B\u0660\u0661\u0662", // "؜-٠٫٠١٢"
                        "\u061C-\u0665\u066B\u0667\u0668", // "؜-٥٫٧٨"
                    },
                    // currency (\062C.\0645.\200F is the currency sign in ar for EGP)
                    new string[] {
                        "\u061C+\u0660\u066B\u0660\u0661\u00A0\u062C.\u0645.\u200F",
                        "\u061C+\u0665\u066B\u0667\u0668\u00A0\u062C.\u0645.\u200F",
                        "\u061C+\u0660\u066B\u0660\u0660\u00A0\u062C.\u0645.\u200F",
                        "\u061C-\u0660\u066B\u0660\u0661\u00A0\u062C.\u0645.\u200F",
                        "\u061C-\u0665\u066B\u0667\u0668\u00A0\u062C.\u0645.\u200F"
                    }
                },
                // es-CL (interesting because of position of sign in currency)
                new string[][] {
                    // decimal
                    new string[] { "+0,012", "+5,78", "+0", "-0,012", "-5,78" },
                    // currency (note: rounding for es-CL's currency, CLP, is 0 fraction digits)
                    new string[] { "$+0", "$+6", "$+0", "$-0", "$-6" }
                }
            };

            for (int i = 0; i < locs.Length; i++)
            {
                UCultureInfo loc = locs[i];
                DecimalFormat df1 = (DecimalFormat)NumberFormat.GetNumberInstance(loc);
                assertFalse("Default should be false", df1.SignAlwaysShown);
                df1.SignAlwaysShown = (true);
                assertTrue("Getter should now return true", df1.SignAlwaysShown);
                DecimalFormat df2 = (DecimalFormat)NumberFormat.GetCurrencyInstance(loc);
                assertFalse("Default should be false", df2.SignAlwaysShown);
                df2.SignAlwaysShown = (true);
                assertTrue("Getter should now return true", df2.SignAlwaysShown);
                for (int j = 0; j < 2; j++)
                {
                    DecimalFormat df = (j == 0) ? df1 : df2;
                    for (int k = 0; k < numbers.Length; k++)
                    {
                        double d = numbers[k];
                        String exp = expecteds[i][j][k];
                        String act = df.Format(d);
                        assertEquals("Locale " + loc + ", type " + j + ", " + d, exp, act);
                        BigDecimal parsedExp = BigDecimal.GetInstance(d);
                        if (j == 1)
                        {
                            // Currency-round expected parse output
                            int scale = (i == 2) ? 0 : 2;
                            parsedExp = parsedExp.SetScale(scale, BigDecimal.RoundHalfEven);
                        }
                        Number parsedNum = df.Parse(exp);
                        BigDecimal parsedAct = (parsedNum.GetType() == typeof(BigDecimal))
                                    ? (BigDecimal)parsedNum
                                    : BigDecimal.GetInstance(parsedNum.ToDouble());
                        assertEquals(
                                "Locale " + loc + ", type " + j + ", " + d + ", " + parsedExp + " => " + parsedAct,
                                0, parsedExp.CompareTo(parsedAct));
                    }
                }
            }
        }

        internal class PropertySetterAnonymousClass : IPropertySetter
        {
            private readonly PluralRules rules;
            public PropertySetterAnonymousClass(PluralRules rules)
            {
                this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            }

            public void Set(Numerics.DecimalFormatProperties props)
            {
                props.PluralRules = rules;
            }
        }

        [Test]
        public void TestCurrencyPluralInfoAndCustomPluralRules() // throws ParseException
        {
            DecimalFormatSymbols symbols = DecimalFormatSymbols.GetInstance(new UCultureInfo("en"));
            PluralRules rules = PluralRules.ParseDescription("one: n is 1; few: n in 2..4");
            CurrencyPluralInfo info = CurrencyPluralInfo.GetInstance(new UCultureInfo("en"));
            info.SetCurrencyPluralPattern("one", "0 qwerty");
            info.SetCurrencyPluralPattern("few", "0 dvorak");
            DecimalFormat df = new DecimalFormat("#", symbols, info, NumberFormatStyle.CurrencyStyle);
            df.Currency = (Currency.GetInstance("USD"));
            df.SetProperties(new PropertySetterAnonymousClass(rules));

            //df.SetProperties(new PropertySetter(){
            //            @Override
            //            public void set(DecimalFormatProperties props)
            //{
            //    props.setPluralRules(rules);
            //}
            //        });

            assertEquals("Plural one", "1.00 qwerty", df.Format(1));
            assertEquals("Plural few", "3.00 dvorak", df.Format(3));
            assertEquals("Plural other", "5.80 US dollars", df.Format(5.8));
        }

        [Test]
        public void TestNarrowCurrencySymbols()
        {
            DecimalFormat df = (DecimalFormat)NumberFormat.GetCurrencyInstance(new UCultureInfo("en-CA") /*ULocale.CANADA*/);
            df.Currency = (Currency.GetInstance("USD"));
            expect2(df, 123.45, "US$123.45");
            String pattern = df.ToPattern();
            pattern = pattern.Replace("¤", "¤¤¤¤¤");
            df.ApplyPattern(pattern);
            // Note: Narrow currency is not parseable because of ambiguity.
            assertEquals("Narrow currency symbol for USD in en_CA is $",
                    "$123.45", df.Format(123.45));
        }

    }
}
