using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using StringBuffer = System.Text.StringBuilder;

/**
 * Port From:   ICU4C v1.8.1 : format : IntlTestDecimalFormatSymbols
 * Source File: $ICU4CRoot/source/test/intltest/tsdcfmsy.cpp
 **/

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// Tests for DecimalFormatSymbols
    /// </summary>
    public class IntlTestDecimalFormatSymbolsC : TestFmwk
    {
        /**
         * Test the API of DecimalFormatSymbols; primarily a simple get/set set.
         */
        [Test]
        public void TestSymbols()
        {
            DecimalFormatSymbols fr = new DecimalFormatSymbols(new CultureInfo("fr"));
            DecimalFormatSymbols en = new DecimalFormatSymbols(new CultureInfo("en"));

            if (en.Equals(fr))
            {
                Errln("ERROR: English DecimalFormatSymbols equal to French");
            }

            // just do some VERY basic tests to make sure that get/set work

            char zero = en.ZeroDigit;
            fr.ZeroDigit = (zero);
            if (fr.ZeroDigit != en.ZeroDigit)
            {
                Errln("ERROR: get/set ZeroDigit failed");
            }

            char group = en.GroupingSeparator;
            fr.GroupingSeparator = (group);
            if (fr.GroupingSeparator != en.GroupingSeparator)
            {
                Errln("ERROR: get/set GroupingSeparator failed");
            }

            char @decimal = en.DecimalSeparator;
            fr.DecimalSeparator = (@decimal);
            if (fr.DecimalSeparator != en.DecimalSeparator)
            {
                Errln("ERROR: get/set DecimalSeparator failed");
            }

            char perMill = en.PerMill;
            fr.PerMill = (perMill);
            if (fr.PerMill != en.PerMill)
            {
                Errln("ERROR: get/set PerMill failed");
            }

            char percent = en.Percent;
            fr.Percent = (percent);
            if (fr.Percent != en.Percent)
            {
                Errln("ERROR: get/set Percent failed");
            }

            char digit = en.Digit;
            fr.Digit = (digit);
            if (fr.Percent != en.Percent)
            {
                Errln("ERROR: get/set Percent failed");
            }

            char patternSeparator = en.PatternSeparator;
            fr.PatternSeparator = (patternSeparator);
            if (fr.PatternSeparator != en.PatternSeparator)
            {
                Errln("ERROR: get/set PatternSeparator failed");
            }

            String infinity = en.Infinity;
            fr.Infinity = (infinity);
            String infinity2 = fr.Infinity;
            if (!infinity.Equals(infinity2, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set Infinity failed");
            }

            String nan = en.NaN;
            fr.NaN = (nan);
            String nan2 = fr.NaN;
            if (!nan.Equals(nan2, StringComparison.Ordinal))
            {
                Errln("ERROR: get/set NaN failed");
            }

            char minusSign = en.MinusSign;
            fr.MinusSign = (minusSign);
            if (fr.MinusSign != en.MinusSign)
            {
                Errln("ERROR: get/set MinusSign failed");
            }

            //        char exponential = en.getExponentialSymbol();
            //        fr.setExponentialSymbol(exponential);
            //        if(fr.getExponentialSymbol() != en.getExponentialSymbol()) {
            //            errln("ERROR: get/set Exponential failed");
            //        }

            //DecimalFormatSymbols foo = new DecimalFormatSymbols(); //The variable is never used

            en = (DecimalFormatSymbols)fr.Clone();

            if (!en.Equals(fr))
            {
                Errln("ERROR: Clone failed");
            }

            DecimalFormatSymbols sym = new DecimalFormatSymbols(new CultureInfo("en-US"));

            verify(34.5, "00.00", sym, "34.50");
            sym.DecimalSeparator = ('S');
            verify(34.5, "00.00", sym, "34S50");
            sym.Percent = ('P');
            verify(34.5, "00 %", sym, "3450 P");
            sym.CurrencySymbol = ("D");
            verify(34.5, "\u00a4##.##", sym, "D 34.50");
            sym.GroupingSeparator = ('|');
            verify(3456.5, "0,000.##", sym, "3|456S5");
        }

        /** helper functions**/
        internal void verify(double value, string pattern, DecimalFormatSymbols sym, string expected)
        {
            // ICU4N TODO: Verify pattern works
            //DecimalFormat df = new DecimalFormat(pattern, sym);
            //StringBuffer buffer = new StringBuffer("");
            //FieldPosition pos = new FieldPosition(-1);
            //buffer = df.Format(value, buffer, pos);
            //if (!buffer.ToString().Equals(expected, StringComparison.Ordinal))
            //{
            //    Errln("ERROR: format failed after setSymbols()\n Expected" +
            //        expected + ", Got " + buffer);
            //}
        }
    }
}
