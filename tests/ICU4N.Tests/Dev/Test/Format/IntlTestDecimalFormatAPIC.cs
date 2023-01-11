using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringBuffer = System.Text.StringBuilder;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;
using Integer = J2N.Numerics.Int32;
using J2N.Numerics;

namespace ICU4N.Dev.Test.Format
{
    // This is an API test, not a unit test.  It doesn't test very many cases, and doesn't
    // try to test the full functionality.  It just calls each function in the class and
    // verifies that it works on a basic level.
    public class IntlTestDecimalFormatAPIC : TestFmwk
    {
        // This test checks various generic API methods in DecimalFormat to achieve 100% API coverage.
        [Test]
        public void TestAPI()
        {

            Logln("DecimalFormat API test---");
            Logln("");
            //Locale.setDefault(Locale.ENGLISH);
            base.CurrentCulture = new CultureInfo("en");

            // ======= Test constructors

            Logln("Testing DecimalFormat constructors");

            DecimalFormat def = new DecimalFormat();

            String pattern = "#,##0.# FF";
            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new CultureInfo("fr"));
            CurrencyPluralInfo infoInput = new CurrencyPluralInfo(new UCultureInfo("fr"));

            DecimalFormat pat = null;
            try
            {
                pat = new DecimalFormat(pattern);
            }
            catch (ArgumentException e)
            {
                Errln("ERROR: Could not create DecimalFormat (pattern)");
            }

            DecimalFormat cust1 = null;
            try
            {
                cust1 = new DecimalFormat(pattern, symbols);
            }
            catch (ArgumentException e)
            {
                Errln("ERROR: Could not create DecimalFormat (pattern, symbols)");
            }

            //@SuppressWarnings("unused")
            DecimalFormat cust2 = null;
            try
            {
                cust2 = new DecimalFormat(pattern, symbols, infoInput, NumberFormatStyle.PluralCurrencyStyle);
            }
            catch (ArgumentException e)
            {
                Errln("ERROR: Could not create DecimalFormat (pattern, symbols, infoInput, style)");
            }


            // ======= Test clone(), assignment, and equality

            Logln("Testing clone() and equality operators");

            UFormat clone = (UFormat)def.Clone();
            if (!def.Equals(clone))
            {
                Errln("ERROR: Clone() failed");
            }

            // ======= Test various format() methods

            Logln("Testing various format() methods");

            //        final double d = -10456.0037; // this appears as -10456.003700000001 on NT
            //        final double d = -1.04560037e-4; // this appears as -1.0456003700000002E-4 on NT
            double d = -10456.00370000000000; // this works!
            long l = 100000000;
            Logln("" + d.ToString(CultureInfo.InvariantCulture) + " is the double value");

            StringBuffer res1 = new StringBuffer();
            StringBuffer res2 = new StringBuffer();
            StringBuffer res3 = new StringBuffer();
            StringBuffer res4 = new StringBuffer();
            FieldPosition pos1 = new FieldPosition(0);
            FieldPosition pos2 = new FieldPosition(0);
            FieldPosition pos3 = new FieldPosition(0);
            FieldPosition pos4 = new FieldPosition(0);

            res1 = def.Format(d, res1, pos1);
            Logln("" + Double.ToString(d, CultureInfo.InvariantCulture) + " formatted to " + res1);

            res2 = pat.Format(l, res2, pos2);
            Logln("" + l + " formatted to " + res2);

            res3 = cust1.Format(d, res3, pos3);
            Logln("" + Double.ToString(d, CultureInfo.InvariantCulture) + " formatted to " + res3);

            res4 = cust1.Format(l, res4, pos4);
            Logln("" + l + " formatted to " + res4);

            // ======= Test parse()

            Logln("Testing parse()");

            String text = "-10,456.0037";
            ParsePosition pos = new ParsePosition(0);
            String patt = "#,##0.#";
            pat.ApplyPattern(patt);
            double d2 = pat.Parse(text, pos).ToDouble();
            if (d2 != d)
            {
                Errln(
                    "ERROR: Roundtrip failed (via parse(" + Double.ToString(d2, CultureInfo.InvariantCulture) + " != " + Double.ToString(d, CultureInfo.InvariantCulture) + ")) for " + text);
            }
            Logln(text + " parsed into " + (long)d2);

            // ======= Test getters and setters

            Logln("Testing getters and setters");

            DecimalFormatSymbols syms = pat.GetDecimalFormatSymbols();
            def.SetDecimalFormatSymbols(syms);
            if (!pat.GetDecimalFormatSymbols().Equals(def.GetDecimalFormatSymbols()))
            {
                Errln("ERROR: set DecimalFormatSymbols() failed");
            }

            String posPrefix;
            pat.PositivePrefix = ("+");
            posPrefix = pat.PositivePrefix;
            Logln("Positive prefix (should be +): " + posPrefix);
            assertEquals("ERROR: setPositivePrefix() failed", "+", posPrefix);

            String negPrefix;
            pat.NegativePrefix = ("-");
            negPrefix = pat.NegativePrefix;
            Logln("Negative prefix (should be -): " + negPrefix);
            assertEquals("ERROR: setNegativePrefix() failed", "-", negPrefix);

            String posSuffix;
            pat.PositiveSuffix = ("_");
            posSuffix = pat.PositiveSuffix;
            Logln("Positive suffix (should be _): " + posSuffix);
            assertEquals("ERROR: setPositiveSuffix() failed", "_", posSuffix);

            String negSuffix;
            pat.NegativeSuffix = ("~");
            negSuffix = pat.NegativeSuffix;
            Logln("Negative suffix (should be ~): " + negSuffix);
            assertEquals("ERROR: setNegativeSuffix() failed", "~", negSuffix);

            long multiplier = 0;
            pat.Multiplier = (8);
            multiplier = pat.Multiplier;
            Logln("Multiplier (should be 8): " + multiplier);
            if (multiplier != 8)
            {
                Errln("ERROR: setMultiplier() failed");
            }

            int groupingSize = 0;
            pat.GroupingSize = (2);
            groupingSize = pat.GroupingSize;
            Logln("Grouping size (should be 2): " + (long)groupingSize);
            if (groupingSize != 2)
            {
                Errln("ERROR: setGroupingSize() failed");
            }

            pat.DecimalSeparatorAlwaysShown = (true);
            bool tf = pat.DecimalSeparatorAlwaysShown;
            Logln(
                "DecimalSeparatorIsAlwaysShown (should be true) is " + (tf ? "true" : "false"));
            if (tf != true)
            {
                Errln("ERROR: setDecimalSeparatorAlwaysShown() failed");
            }

            String funkyPat;
            funkyPat = pat.ToPattern();
            Logln("Pattern is " + funkyPat);

            String locPat;
            locPat = pat.ToLocalizedPattern();
            Logln("Localized pattern is " + locPat);

            pat.CurrencyPluralInfo = (infoInput);
            if (!infoInput.Equals(pat.CurrencyPluralInfo))
            {
                Errln("ERROR: set/get CurrencyPluralInfo() failed");
            }


            pat.CurrencyPluralInfo = (infoInput);
            if (!infoInput.Equals(pat.CurrencyPluralInfo))
            {
                Errln("ERROR: set/get CurrencyPluralInfo() failed");
            }

            // ======= Test applyPattern()

            Logln("Testing applyPattern()");

            String p1 = "#,##0.0#;(#,##0.0#)";
            Logln("Applying pattern " + p1);
            pat.ApplyPattern(p1);
            String s2;
            s2 = pat.ToPattern();
            Logln("Extracted pattern is " + s2);
            if (!s2.Equals(p1, StringComparison.Ordinal))
            {
                Errln("ERROR: toPattern() result did not match pattern applied: " + p1 + " vs " + s2);
            }

            String p2 = "#,##0.0# FF;(#,##0.0# FF)";
            Logln("Applying pattern " + p2);
            pat.ApplyLocalizedPattern(p2);
            String s3;
            s3 = pat.ToLocalizedPattern();
            Logln("Extracted pattern is " + s3);
            assertEquals("ERROR: toLocalizedPattern() result did not match pattern applied", p2, s3);

            // ======= Test getStaticClassID()

            //        Logln("Testing instanceof()");

            //        try {
            //           NumberFormat test = new DecimalFormat();

            //            if (! (test instanceof DecimalFormat)) {
            //                Errln("ERROR: instanceof failed");
            //            }
            //        }
            //        catch (Exception e) {
            //            Errln("ERROR: Couldn't create a DecimalFormat");
            //        }

        }

        [Test]
        public void TestRounding()
        {
            double Roundingnumber = 2.55;
            double Roundingnumber1 = -2.55;
            //+2.55 results   -2.55 results
            double[] result = {
                3, -3,
                2, -2,
                3, -2,
                2, -3,
                3, -3,
                3, -3,
                3, -3
            };
            DecimalFormat pat = new DecimalFormat();
            String s = "";
            s = pat.ToPattern();
            Logln("pattern = " + s);
            int mode;
            int i = 0;
            String message;
            String resultStr;
            for (mode = 0; mode < 7; mode++)
            {
                pat.RoundingMode = (Numerics.BigMath.RoundingMode)(mode);
                if ((int)pat.RoundingMode != mode)
                {
                    Errln(
                         "SetRoundingMode or GetRoundingMode failed for mode=" + mode);
                }

                //for +2.55 with RoundingIncrement=1.0
                pat.SetRoundingIncrement(Numerics.BigMath.BigDecimal.One);
                resultStr = pat.Format(Roundingnumber);
                message = "round(" + Roundingnumber
                        + "," + mode + ",FALSE) with RoundingIncrement=1.0==>";
                verify(message, resultStr, result[i++]);
                message = "";
                resultStr = "";

                //for -2.55 with RoundingIncrement=1.0
                resultStr = pat.Format(Roundingnumber1);
                message = "round(" + Roundingnumber1
                        + "," + mode + ",FALSE) with RoundingIncrement=1.0==>";
                verify(message, resultStr, result[i++]);
                message = "";
                resultStr = "";
            }
        }

        [Test]
        public void testFormatToCharacterIterator()
        {

            Number number = Double.GetInstance(350.76);
            Number negativeNumber = Double.GetInstance(-350.76);

            CultureInfo us = new CultureInfo("en-US"); //Locale.US;

            // test number instance
            t_Format(1, number, NumberFormat.GetNumberInstance(us),
                    getNumberVectorUS());

            // test percent instance
            t_Format(3, number, NumberFormat.GetPercentInstance(us),
                    getPercentVectorUS());

            // test permille pattern
            DecimalFormat format = new DecimalFormat("###0.##\u2030");
            t_Format(4, number, format, getPermilleVector());

            // test exponential pattern with positive exponent
            format = new DecimalFormat("00.0#E0");
            t_Format(5, number, format, getPositiveExponentVector());

            // test exponential pattern with negative exponent
            format = new DecimalFormat("0000.0#E0");
            t_Format(6, number, format, getNegativeExponentVector());

            // test currency instance with US Locale
            t_Format(7, number, NumberFormat.GetCurrencyInstance(us),
                    getPositiveCurrencyVectorUS());

            // test negative currency instance with US Locale
            t_Format(8, negativeNumber, NumberFormat.GetCurrencyInstance(us),
                    getNegativeCurrencyVectorUS());

            // test multiple grouping separators
            number = Long.GetInstance(100300400);
            t_Format(11, number, NumberFormat.GetNumberInstance(us),
                    getNumberVector2US());

            // test 0
            number = Long.GetInstance(0);
            t_Format(12, number, NumberFormat.GetNumberInstance(us),
                    getZeroVector());
        }

        private static List<FieldContainer> getNumberVectorUS()
        {
            List<FieldContainer> v = new List<FieldContainer>(3);
            v.Add(new FieldContainer(0, 3, NumberFormatField.Integer));
            v.Add(new FieldContainer(3, 4, NumberFormatField.DecimalSeparator));
            v.Add(new FieldContainer(4, 6, NumberFormatField.Fraction));
            return v;
        }

        //    private static Vector getPositiveCurrencyVectorTR() {
        //        Vector v = new Vector();
        //        v.Add(new FieldContainer(0, 3, NumberFormat.Field.INTEGER));
        //        v.Add(new FieldContainer(4, 6, NumberFormat.Field.CURRENCY));
        //        return v;
        //    }
        //
        //    private static Vector getNegativeCurrencyVectorTR() {
        //        Vector v = new Vector();
        //        v.Add(new FieldContainer(0, 1, NumberFormat.Field.SIGN));
        //        v.Add(new FieldContainer(1, 4, NumberFormat.Field.INTEGER));
        //        v.Add(new FieldContainer(5, 7, NumberFormat.Field.CURRENCY));
        //        return v;
        //    }

        private static IList<FieldContainer> getPositiveCurrencyVectorUS()
        {
            List<FieldContainer> v = new List<FieldContainer>(4);
            v.Add(new FieldContainer(0, 1, NumberFormatField.Currency));
            v.Add(new FieldContainer(1, 4, NumberFormatField.Integer));
            v.Add(new FieldContainer(4, 5, NumberFormatField.DecimalSeparator));
            v.Add(new FieldContainer(5, 7, NumberFormatField.Fraction));
            return v;
        }

        private static IList<FieldContainer> getNegativeCurrencyVectorUS()
        {
            List<FieldContainer> v = new List<FieldContainer>(4);
            // SIGN added with fix for issue 11805.
            v.Add(new FieldContainer(0, 1, NumberFormatField.Sign));
            v.Add(new FieldContainer(1, 2, NumberFormatField.Currency));
            v.Add(new FieldContainer(2, 5, NumberFormatField.Integer));
            v.Add(new FieldContainer(5, 6, NumberFormatField.DecimalSeparator));
            v.Add(new FieldContainer(6, 8, NumberFormatField.Fraction));
            return v;
        }

        private static IList<FieldContainer> getPercentVectorUS()
        {
            List<FieldContainer> v = new List<FieldContainer>(5);
            v.Add(new FieldContainer(0, 2, NumberFormatField.Integer));
            v.Add(new FieldContainer(2, 3, NumberFormatField.Integer));
            v.Add(new FieldContainer(2, 3, NumberFormatField.GroupingSeparator));
            v.Add(new FieldContainer(3, 6, NumberFormatField.Integer));
            v.Add(new FieldContainer(6, 7, NumberFormatField.Percent));
            return v;
        }

        private static IList<FieldContainer> getPermilleVector()
        {
            List<FieldContainer> v = new List<FieldContainer>(2);
            v.Add(new FieldContainer(0, 6, NumberFormatField.Integer));
            v.Add(new FieldContainer(6, 7, NumberFormatField.PerMille));
            return v;
        }

        private static IList<FieldContainer> getNegativeExponentVector()
        {
            List<FieldContainer> v = new List<FieldContainer>(6);
            v.Add(new FieldContainer(0, 4, NumberFormatField.Integer));
            v.Add(new FieldContainer(4, 5, NumberFormatField.DecimalSeparator));
            v.Add(new FieldContainer(5, 6, NumberFormatField.Fraction));
            v.Add(new FieldContainer(6, 7, NumberFormatField.ExponentSymbol));
            v.Add(new FieldContainer(7, 8, NumberFormatField.ExponentSign));
            v.Add(new FieldContainer(8, 9, NumberFormatField.Exponent));
            return v;
        }

        private static IList<FieldContainer> getPositiveExponentVector()
        {
            List<FieldContainer> v = new List<FieldContainer>(5);
            v.Add(new FieldContainer(0, 2, NumberFormatField.Integer));
            v.Add(new FieldContainer(2, 3, NumberFormatField.DecimalSeparator));
            v.Add(new FieldContainer(3, 5, NumberFormatField.Fraction));
            v.Add(new FieldContainer(5, 6, NumberFormatField.ExponentSymbol));
            v.Add(new FieldContainer(6, 7, NumberFormatField.Exponent));
            return v;
        }

        private static IList<FieldContainer> getNumberVector2US()
        {
            List<FieldContainer> v = new List<FieldContainer>(7);
            v.Add(new FieldContainer(0, 3, NumberFormatField.Integer));
            v.Add(new FieldContainer(3, 4, NumberFormatField.GroupingSeparator));
            v.Add(new FieldContainer(3, 4, NumberFormatField.Integer));
            v.Add(new FieldContainer(4, 7, NumberFormatField.Integer));
            v.Add(new FieldContainer(7, 8, NumberFormatField.GroupingSeparator));
            v.Add(new FieldContainer(7, 8, NumberFormatField.Integer));
            v.Add(new FieldContainer(8, 11, NumberFormatField.Integer));
            return v;
        }

        private static IList<FieldContainer> getZeroVector()
        {
            List<FieldContainer> v = new List<FieldContainer>(1);
            v.Add(new FieldContainer(0, 1, NumberFormatField.Integer));
            return v;
        }

        private void t_Format(int count, Object obj, Formatter format,
                IList<FieldContainer> expectedResults)
        {
            List<FieldContainer> results = findFields(format.FormatToCharacterIterator(obj));
            assertTrue("Test " + count
                    + ": Format returned incorrect CharacterIterator for "
                    + format.Format(obj), compare(results, expectedResults));
        }

        /**
         * compares two vectors regardless of the order of their elements
         */
        private static bool compare<T>(IList<T> vector1, IList<T> vector2)
        {
            return vector1.Count == vector2.Count && !vector2.Except(vector1).Any(); //vector1.containsAll(vector2);
        }

        /**
         * finds attributes with regards to char index in this
         * AttributedCharacterIterator, and puts them in a vector
         *
         * @param iterator
         * @return a vector, each entry in this vector are of type FieldContainer ,
         *         which stores start and end indexes and an attribute this range
         *         has
         */
        private static List<FieldContainer> findFields(AttributedCharacterIterator iterator)
        {
            List<FieldContainer> result = new List<FieldContainer>();
            while (iterator.Index != iterator.EndIndex)
            {
                int start = iterator.GetRunStart();
                int end = iterator.GetRunLimit();

                var it = iterator.GetAttributes().Keys.GetEnumerator();
                while (it.MoveNext())
                {
                    AttributedCharacterIteratorAttribute attribute = (AttributedCharacterIteratorAttribute)it
                            .Current;
                    Object value = iterator.GetAttribute(attribute);
                    result.Add(new FieldContainer(start, end, attribute, value));
                    // System.out.println(start + " " + end + ": " + attribute + ",
                    // " + value );
                    // System.out.println("v.Add(new FieldContainer(" + start +"," +
                    // end +"," + attribute+ "," + value+ "));");
                }
                iterator.SetIndex(end);
            }
            return result;
        }

        // ICU4N: De-nested FieldContainer

        /*Helper functions */
        public void verify(string message, string got, double expected)
        {
            Logln(message + got + " Expected : " + (long)expected);
            String expectedStr = "";
            expectedStr = expectedStr + (long)expected;
            if (!got.Equals(expectedStr, StringComparison.Ordinal))
            {
                Errln("ERROR: Round() failed:  " + message + got + "  Expected : " + expectedStr);
            }
        }
    }

    public class FieldContainer
    {
        internal int start, end;

        internal AttributedCharacterIteratorAttribute attribute;

        internal object value;

        //         called from support_decimalformat and support_simpledateformat tests
        public FieldContainer(int start, int end,
            AttributedCharacterIteratorAttribute attribute)
            : this(start, end, attribute, attribute)
        {
        }

        //         called from support_messageformat tests
        public FieldContainer(int start, int end, AttributedCharacterIteratorAttribute attribute, int value)
            : this(start, end, attribute, (object)Integer.GetInstance(value))
        {
        }

        //         called from support_messageformat tests
        public FieldContainer(int start, int end, AttributedCharacterIteratorAttribute attribute,
            object value)
        {
            this.start = start;
            this.end = end;
            this.attribute = attribute;
            this.value = value;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is FieldContainer fc))
                return false;

            return (start == fc.start && end == fc.end
                && attribute == fc.attribute && value.Equals(fc.value));
        }

        public override int GetHashCode()
        {
            return start.GetHashCode() ^ end.GetHashCode(); // ICU4N specific: Quick comparison - check the start and end. If these match, then do the equality check.
        }
    }
}
