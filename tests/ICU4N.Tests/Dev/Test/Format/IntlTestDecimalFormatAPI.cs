using ICU4N.Numerics;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Globalization;
using J2N.Numerics;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using StringBuffer = System.Text.StringBuilder;

/**
 * Port From:   JDK 1.4b1 : java.text.Format.IntlTestDecimalFormatAPI
 * Source File: java/text/format/IntlTestDecimalFormatAPI.java
 **/

/*
    [Test] 1.4 98/03/06
    @summary test International Decimal Format API
*/

namespace ICU4N.Dev.Test.Format
{
    public class IntlTestDecimalFormatAPI : TestFmwk
    {
        /**
         * Problem 1: simply running
         * decF4.setRoundingMode(java.math.BigDecimal.ROUND_HALF_UP) does not work
         * as decF4.setRoundingIncrement(.0001) must also be run.
         * Problem 2: decF4.format(8.88885) does not return 8.8889 as expected.
         * You must run decF4.format(new BigDecimal(Double.valueOf(8.88885))) in
         * order for this to work as expected.
         * Problem 3: There seems to be no way to set half up to be the default
         * rounding mode.
         * We solved the problem with the code at the bottom of this page however
         * this is not quite general purpose enough to include in icu4j. A static
         * setDefaultRoundingMode function would solve the problem nicely. Also
         * decimal places past 20 are not handled properly. A small ammount of work
         * would make bring this up to snuff.
         */
        [Test]
        public void TestJB1871()
        {
            // problem 2
            double number = 8.88885;
            String expected = "8.8889";

            String pat = ",##0.0000";
            DecimalFormat dec = new DecimalFormat(pat);
            dec.RoundingMode = ICU4N.Numerics.BigMath.RoundingMode.HalfUp;
            dec.SetRoundingIncrement(ICU4N.Numerics.BigMath.BigDecimal.Parse("0.0001", CultureInfo.InvariantCulture));
            String str = dec.Format(number);
            if (!str.Equals(expected, StringComparison.Ordinal))
            {
                Errln("Fail: " + number + " x \"" + pat + "\" = \"" +
                      str + "\", expected \"" + expected + "\"");
            }

            pat = ",##0.0001";
            dec = new DecimalFormat(pat);
            dec.RoundingMode = ICU4N.Numerics.BigMath.RoundingMode.HalfUp;
            str = dec.Format(number);
            if (!str.Equals(expected, StringComparison.Ordinal))
            {
                Errln("Fail: " + number + " x \"" + pat + "\" = \"" +
                      str + "\", expected \"" + expected + "\"");
            }

            // testing 20 decimal places
            pat = ",##0.00000000000000000001";
            dec = new DecimalFormat(pat);
            BigDecimal bignumber = BigDecimal.Parse("8.888888888888888888885", CultureInfo.InvariantCulture);
            expected = "8.88888888888888888889";

            dec.RoundingMode = ICU4N.Numerics.BigMath.RoundingMode.HalfUp;
            str = dec.Format(bignumber);
            if (!str.Equals(expected, StringComparison.Ordinal))
            {
                Errln("Fail: " + bignumber + " x \"" + pat + "\" = \"" +
                      str + "\", expected \"" + expected + "\"");
            }

        }

        /**
         * This test checks various generic API methods in DecimalFormat to achieve
         * 100% API coverage.
         */
        [Test]
        public void TestAPI()
        {
            Logln("DecimalFormat API test---"); Logln("");
            //Locale.setDefault(Locale.ENGLISH);
            using var context = new CultureContext("en"); // ICU4N specific: Use CultureContext to ensure culture is reset if the test fails.


            // ======= Test constructors

            Logln("Testing DecimalFormat constructors");

            DecimalFormat def = new DecimalFormat();

            string pattern = "#,##0.# FF";
            DecimalFormat pat = null;
            try
            {
                pat = new DecimalFormat(pattern);
            }
            catch (ArgumentException e)
            {
                Errln("ERROR: Could not create DecimalFormat (pattern)");
            }

            DecimalFormatSymbols symbols = new DecimalFormatSymbols(new CultureInfo("fr"));

            DecimalFormat cust1 = new DecimalFormat(pattern, symbols);

            // ======= Test clone(), assignment, and equality

            Logln("Testing clone() and equality operators");

            Formatter clone = (Formatter)def.Clone();
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
            Logln("" + d + " is the double value");

            StringBuffer res1 = new StringBuffer();
            StringBuffer res2 = new StringBuffer();
            StringBuffer res3 = new StringBuffer();
            StringBuffer res4 = new StringBuffer();
            FieldPosition pos1 = new FieldPosition(0);
            FieldPosition pos2 = new FieldPosition(0);
            FieldPosition pos3 = new FieldPosition(0);
            FieldPosition pos4 = new FieldPosition(0);

            res1 = def.Format(d, res1, pos1);
            Logln("" + d + " formatted to " + res1);

            res2 = pat.Format(l, res2, pos2);
            Logln("" + l + " formatted to " + res2);

            res3 = cust1.Format(d, res3, pos3);
            Logln("" + d + " formatted to " + res3);

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
                Errln("ERROR: Roundtrip failed (via parse(" + d2 + " != " + d + ")) for " + text);
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
            Logln("DecimalSeparatorIsAlwaysShown (should be true) is " + (tf ? "true" : "false"));
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
                Errln("ERROR: toPattern() result did not match pattern applied");
            }

            String p2 = "#,##0.0# FF;(#,##0.0# FF)";
            Logln("Applying pattern " + p2);
            pat.ApplyLocalizedPattern(p2);
            String s3;
            s3 = pat.ToLocalizedPattern();
            Logln("Extracted pattern is " + s3);
            if (!s3.Equals(p2, StringComparison.Ordinal))
            {
                Errln("ERROR: toLocalizedPattern() result did not match pattern applied");
            }
        }

        [Test]
        public void TestJB6134()
        {
            DecimalFormat decfmt = new DecimalFormat();
            StringBuffer buf = new StringBuffer();

            FieldPosition fposByInt = new FieldPosition(NumberFormat.IntegerField);
            decfmt.Format(123, buf, fposByInt);

            buf.Length = (0);
            FieldPosition fposByField = new FieldPosition(NumberFormatField.Integer);
            decfmt.Format(123, buf, fposByField);

            if (fposByInt.EndIndex != fposByField.EndIndex)
            {
                Errln("ERROR: End index for integer field - fposByInt:" + fposByInt.EndIndex +
                    " / fposByField: " + fposByField.EndIndex);
            }
        }

        [Test]
        public void TestJB4971()
        {
            DecimalFormat decfmt = new DecimalFormat();
            MathContext resultICU;

            MathContext comp1 = new MathContext(0, MathContext.Plain, false, MathContext.RoundHalfEven);
            resultICU = decfmt.MathContextICU;
            if ((comp1.Digits != resultICU.Digits) ||
                (comp1.Form != resultICU.Form) ||
                (comp1.LostDigits != resultICU.LostDigits) ||
                (comp1.RoundingMode != resultICU.RoundingMode))
            {
                Errln("ERROR: Math context 1 not equal - result: " + resultICU.ToString() +
                    " / expected: " + comp1.ToString());
            }

            MathContext comp2 = new MathContext(5, MathContext.Engineering, false, MathContext.RoundHalfEven);
            decfmt.MathContextICU = (comp2);
            resultICU = decfmt.MathContextICU;
            if ((comp2.Digits != resultICU.Digits) ||
                (comp2.Form != resultICU.Form) ||
                (comp2.LostDigits != resultICU.LostDigits) ||
                (comp2.RoundingMode != resultICU.RoundingMode))
            {
                Errln("ERROR: Math context 2 not equal - result: " + resultICU.ToString() +
                    " / expected: " + comp2.ToString());
            }

            ICU4N.Numerics.BigMath.MathContext result;

            ICU4N.Numerics.BigMath.MathContext comp3 = new ICU4N.Numerics.BigMath.MathContext(3, ICU4N.Numerics.BigMath.RoundingMode.Down);
            decfmt.MathContext = (comp3);
            result = decfmt.MathContext;
            if ((comp3.Precision != result.Precision) ||
                (comp3.RoundingMode != result.RoundingMode))
            {
                Errln("ERROR: Math context 3 not equal - result: " + result.ToString() +
                    " / expected: " + comp3.ToString());
            }

        }

        [Test]
        public void TestJB6354()
        {
            DecimalFormat pat = new DecimalFormat("#,##0.00");
            ICU4N.Numerics.BigMath.BigDecimal r1, r2;

            // get default rounding increment
            r1 = pat.RoundingIncrement;

            // set rounding mode with zero increment.  Rounding
            // increment should be set by this operation
            pat.RoundingMode = ICU4N.Numerics.BigMath.RoundingMode.Up;
            r2 = pat.RoundingIncrement;

            // check for different values
            if ((r1 != null) && (r2 != null))
            {
                if (r1.CompareTo(r2) == 0)
                {
                    Errln("ERROR: Rounding increment did not change");
                }
            }
        }

        [Test]
        public void TestJB6648()
        {
            DecimalFormat df = new DecimalFormat();
            df.ParseStrict = (true);

            String numstr;

            String[] patterns = {
                "0",
                "00",
                "000",
                "0,000",
                "0.0",
                "#000.0"
            };

            for (int i = 0; i < patterns.Length; i++)
            {
                df.ApplyPattern(patterns[i]);
                numstr = df.Format(5);
                try
                {
                    Number n = df.Parse(numstr);
                    Logln("INFO: Parsed " + numstr + " -> " + n);
                }
                catch (FormatException pe)
                {
                    Errln("ERROR: Failed round trip with strict parsing.");
                }
            }

            df.ApplyPattern(patterns[1]);
            numstr = "005";
            try
            {
                Number n = df.Parse(numstr);
                Logln("INFO: Successful parse for " + numstr + " with strict parse enabled. Number is " + n);
            }
            catch (FormatException pe)
            {
                Errln("ERROR: Parse Exception encountered in strict mode: numstr -> " + numstr);
            }

        }
    }
}
