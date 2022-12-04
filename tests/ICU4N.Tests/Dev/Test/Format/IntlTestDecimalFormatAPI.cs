using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Ignore("ICU4N TODO: Missing dependencies DecimalFormat, BigDecimal")]
        public void TestJB1871()
        {
            //// problem 2
            //double number = 8.88885;
            //String expected = "8.8889";

            //String pat = ",##0.0000";
            //DecimalFormat dec = new DecimalFormat(pat);
            //dec.RoundingMode = (BigDecimal.ROUND_HALF_UP);
            //dec.RoundingIncrement = (new java.math.BigDecimal("0.0001"));
            //String str = dec.Format(number);
            //if (!str.Equals(expected, StringComparison.Ordinal))
            //{
            //    Errln("Fail: " + number + " x \"" + pat + "\" = \"" +
            //          str + "\", expected \"" + expected + "\"");
            //}

            //pat = ",##0.0001";
            //dec = new DecimalFormat(pat);
            //dec.RoundingMode = (BigDecimal.ROUND_HALF_UP);
            //str = dec.Format(number);
            //if (!str.Equals(expected, StringComparison.Ordinal))
            //{
            //    Errln("Fail: " + number + " x \"" + pat + "\" = \"" +
            //          str + "\", expected \"" + expected + "\"");
            //}

            //// testing 20 decimal places
            //pat = ",##0.00000000000000000001";
            //dec = new DecimalFormat(pat);
            //BigDecimal bignumber = new BigDecimal("8.888888888888888888885");
            //expected = "8.88888888888888888889";

            //dec.RoundingMode = (BigDecimal.ROUND_HALF_UP);
            //str = dec.Format(bignumber);
            //if (!str.Equals(expected, StringComparison.Ordinal))
            //{
            //    Errln("Fail: " + bignumber + " x \"" + pat + "\" = \"" +
            //          str + "\", expected \"" + expected + "\"");
            //}

        }

        /**
         * This test checks various generic API methods in DecimalFormat to achieve
         * 100% API coverage.
         */
        [Test]
        [Ignore("ICU4N TODO: Missing dependnecy DecimalFormat")]
        public void TestAPI()
        {
            //Logln("DecimalFormat API test---"); Logln("");
            //Locale.setDefault(Locale.ENGLISH);

            //// ======= Test constructors

            //Logln("Testing DecimalFormat constructors");

            //DecimalFormat def = new DecimalFormat();

            //string pattern = "#,##0.# FF";
            //DecimalFormat pat = null;
            //try
            //{
            //    pat = new DecimalFormat(pattern);
            //}
            //catch (ArgumentException e)
            //{
            //    Errln("ERROR: Could not create DecimalFormat (pattern)");
            //}

            //DecimalFormatSymbols symbols = new DecimalFormatSymbols(new CultureInfo("fr"));

            //DecimalFormat cust1 = new DecimalFormat(pattern, symbols);

            //// ======= Test clone(), assignment, and equality

            //Logln("Testing clone() and equality operators");

            //Formatter clone = (Formatter)def.Clone();
            //if (!def.Equals(clone))
            //{
            //    Errln("ERROR: Clone() failed");
            //}

            //// ======= Test various format() methods

            //Logln("Testing various format() methods");

            ////        final double d = -10456.0037; // this appears as -10456.003700000001 on NT
            ////        final double d = -1.04560037e-4; // this appears as -1.0456003700000002E-4 on NT
            //double d = -10456.00370000000000; // this works!
            //long l = 100000000;
            //Logln("" + d + " is the double value");

            //StringBuffer res1 = new StringBuffer();
            //StringBuffer res2 = new StringBuffer();
            //StringBuffer res3 = new StringBuffer();
            //StringBuffer res4 = new StringBuffer();
            //FieldPosition pos1 = new FieldPosition(0);
            //FieldPosition pos2 = new FieldPosition(0);
            //FieldPosition pos3 = new FieldPosition(0);
            //FieldPosition pos4 = new FieldPosition(0);

            //res1 = def.Format(d, res1, pos1);
            //Logln("" + d + " formatted to " + res1);

            //res2 = pat.Format(l, res2, pos2);
            //Logln("" + l + " formatted to " + res2);

            //res3 = cust1.Format(d, res3, pos3);
            //Logln("" + d + " formatted to " + res3);

            //res4 = cust1.Format(l, res4, pos4);
            //Logln("" + l + " formatted to " + res4);

            //// ======= Test parse()

            //Logln("Testing parse()");

            //String text = new String("-10,456.0037");
            //ParsePosition pos = new ParsePosition(0);
            //String patt = new String("#,##0.#");
            //pat.applyPattern(patt);
            //double d2 = pat.parse(text, pos).doubleValue();
            //if (d2 != d)
            //{
            //    Errln("ERROR: Roundtrip failed (via parse(" + d2 + " != " + d + ")) for " + text);
            //}
            //Logln(text + " parsed into " + (long)d2);

            //// ======= Test getters and setters

            //Logln("Testing getters and setters");

            //DecimalFormatSymbols syms = pat.DecimalFormatSymbols;
            //def.SetDecimalFormatSymbols(syms);
            //if (!pat.DecimalFormatSymbols.Equals(def.DecimalFormatSymbols))
            //{
            //    Errln("ERROR: set DecimalFormatSymbols() failed");
            //}

            //String posPrefix;
            //pat.PositivePrefix = ("+");
            //posPrefix = pat.PositivePrefix;
            //Logln("Positive prefix (should be +): " + posPrefix);
            //assertEquals("ERROR: setPositivePrefix() failed", "+", posPrefix);

            //String negPrefix;
            //pat.NegativePrefix = ("-");
            //negPrefix = pat.NegativePrefix;
            //Logln("Negative prefix (should be -): " + negPrefix);
            //assertEquals("ERROR: setNegativePrefix() failed", "-", negPrefix);

            //String posSuffix;
            //pat.PositiveSuffix = ("_");
            //posSuffix = pat.PositiveSuffix;
            //Logln("Positive suffix (should be _): " + posSuffix);
            //assertEquals("ERROR: setPositiveSuffix() failed", "_", posSuffix);

            //String negSuffix;
            //pat.NegativeSuffix = ("~");
            //negSuffix = pat.NegativeSuffix;
            //Logln("Negative suffix (should be ~): " + negSuffix);
            //assertEquals("ERROR: setNegativeSuffix() failed", "~", negSuffix);

            //long multiplier = 0;
            //pat.Multiplier = (8);
            //multiplier = pat.Multiplier;
            //Logln("Multiplier (should be 8): " + multiplier);
            //if (multiplier != 8)
            //{
            //    Errln("ERROR: setMultiplier() failed");
            //}

            //int groupingSize = 0;
            //pat.GroupingSize = (2);
            //groupingSize = pat.GroupingSize;
            //Logln("Grouping size (should be 2): " + (long)groupingSize);
            //if (groupingSize != 2)
            //{
            //    Errln("ERROR: setGroupingSize() failed");
            //}

            //pat.DecimalSeparatorAlwaysShown = (true);
            //bool tf = pat.DecimalSeparatorAlwaysShown;
            //Logln("DecimalSeparatorIsAlwaysShown (should be true) is " + (tf ? "true" : "false"));
            //if (tf != true)
            //{
            //    Errln("ERROR: setDecimalSeparatorAlwaysShown() failed");
            //}

            //String funkyPat;
            //funkyPat = pat.ToPattern();
            //Logln("Pattern is " + funkyPat);

            //String locPat;
            //locPat = pat.ToLocalizedPattern();
            //Logln("Localized pattern is " + locPat);

            //// ======= Test applyPattern()

            //Logln("Testing applyPattern()");

            //String p1 = new String("#,##0.0#;(#,##0.0#)");
            //Logln("Applying pattern " + p1);
            //pat.ApplyPattern(p1);
            //String s2;
            //s2 = pat.ToPattern();
            //Logln("Extracted pattern is " + s2);
            //if (!s2.Equals(p1))
            //{
            //    Errln("ERROR: toPattern() result did not match pattern applied");
            //}

            //String p2 = new String("#,##0.0# FF;(#,##0.0# FF)");
            //Logln("Applying pattern " + p2);
            //pat.ApplyLocalizedPattern(p2);
            //String s3;
            //s3 = pat.ToLocalizedPattern();
            //Logln("Extracted pattern is " + s3);
            //if (!s3.Equals(p2))
            //{
            //    Errln("ERROR: toLocalizedPattern() result did not match pattern applied");
            //}
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependency DecimalFormat")]
        public void TestJB6134()
        {
            //DecimalFormat decfmt = new DecimalFormat();
            //StringBuffer buf = new StringBuffer();

            //FieldPosition fposByInt = new FieldPosition(NumberFormat.IntegerField);
            //decfmt.Format(123, buf, fposByInt);

            //buf.Length = (0);
            //FieldPosition fposByField = new FieldPosition(NumberFormatField.Integer);
            //decfmt.Format(123, buf, fposByField);

            //if (fposByInt.EndIndex != fposByField.EndIndex)
            //{
            //    Errln("ERROR: End index for integer field - fposByInt:" + fposByInt.EndIndex +
            //        " / fposByField: " + fposByField.EndIndex);
            //}
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependencies DecimalFormat, MathContext")]
        public void TestJB4971()
        {
            //DecimalFormat decfmt = new DecimalFormat();
            //MathContext resultICU;

            //MathContext comp1 = new MathContext(0, MathContext.PLAIN, false, MathContext.ROUND_HALF_EVEN);
            //resultICU = decfmt.getMathContextICU();
            //if ((comp1.getDigits() != resultICU.getDigits()) ||
            //    (comp1.getForm() != resultICU.getForm()) ||
            //    (comp1.getLostDigits() != resultICU.getLostDigits()) ||
            //    (comp1.getRoundingMode() != resultICU.getRoundingMode()))
            //{
            //    Errln("ERROR: Math context 1 not equal - result: " + resultICU.toString() +
            //        " / expected: " + comp1.toString());
            //}

            //MathContext comp2 = new MathContext(5, MathContext.ENGINEERING, false, MathContext.ROUND_HALF_EVEN);
            //decfmt.setMathContextICU(comp2);
            //resultICU = decfmt.getMathContextICU();
            //if ((comp2.getDigits() != resultICU.getDigits()) ||
            //    (comp2.getForm() != resultICU.getForm()) ||
            //    (comp2.getLostDigits() != resultICU.getLostDigits()) ||
            //    (comp2.getRoundingMode() != resultICU.getRoundingMode()))
            //{
            //    Errln("ERROR: Math context 2 not equal - result: " + resultICU.toString() +
            //        " / expected: " + comp2.toString());
            //}

            //java.math.MathContext result;

            //java.math.MathContext comp3 = new java.math.MathContext(3, java.math.RoundingMode.DOWN);
            //decfmt.setMathContext(comp3);
            //result = decfmt.getMathContext();
            //if ((comp3.getPrecision() != result.getPrecision()) ||
            //    (comp3.getRoundingMode() != result.getRoundingMode()))
            //{
            //    Errln("ERROR: Math context 3 not equal - result: " + result.toString() +
            //        " / expected: " + comp3.toString());
            //}

        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependency DecimalFormat, BigDecimal")]
        public void TestJB6354()
        {
            //DecimalFormat pat = new DecimalFormat("#,##0.00");
            //java.math.BigDecimal r1, r2;

            //// get default rounding increment
            //r1 = pat.getRoundingIncrement();

            //// set rounding mode with zero increment.  Rounding
            //// increment should be set by this operation
            //pat.setRoundingMode(BigDecimal.ROUND_UP);
            //r2 = pat.getRoundingIncrement();

            //// check for different values
            //if ((r1 != null) && (r2 != null))
            //{
            //    if (r1.compareTo(r2) == 0)
            //    {
            //        Errln("ERROR: Rounding increment did not change");
            //    }
            //}
        }

        [Test]
        [Ignore("ICU4N TODO: Missing dependency DecimalFormat")]
        public void TestJB6648()
        {
            //DecimalFormat df = new DecimalFormat();
            //df.setParseStrict(true);

            //String numstr;

            //String[] patterns = {
            //    "0",
            //    "00",
            //    "000",
            //    "0,000",
            //    "0.0",
            //    "#000.0"
            //};

            //for (int i = 0; i < patterns.length; i++)
            //{
            //    df.applyPattern(patterns[i]);
            //    numstr = df.format(5);
            //    try
            //    {
            //        Number n = df.parse(numstr);
            //        Logln("INFO: Parsed " + numstr + " -> " + n);
            //    }
            //    catch (ParseException pe)
            //    {
            //        Errln("ERROR: Failed round trip with strict parsing.");
            //    }
            //}

            //df.applyPattern(patterns[1]);
            //numstr = "005";
            //try
            //{
            //    Number n = df.parse(numstr);
            //    Logln("INFO: Successful parse for " + numstr + " with strict parse enabled. Number is " + n);
            //}
            //catch (ParseException pe)
            //{
            //    Errln("ERROR: Parse Exception encountered in strict mode: numstr -> " + numstr);
            //}

        }
    }
}
