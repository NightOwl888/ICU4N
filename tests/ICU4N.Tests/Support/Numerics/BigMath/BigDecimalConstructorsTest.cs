using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigDecimal
    /// Methods: constructors and fields
    /// </summary>
    public class BigDecimalConstructorsTest : TestFmwk
    {
        /**
         * check ONE
         */
        [Test]
        public void testFieldONE()
        {
            String oneS = "1";
            double oneD = 1.0;
            assertEquals("incorrect string value", oneS, BigDecimal.One.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect double value", oneD, BigDecimal.One.ToDouble(), 0);
        }

        /**
         * check TEN
         */
        [Test]
        public void testFieldTEN()
        {
            String oneS = "10";
            double oneD = 10.0;
            assertEquals("incorrect string value", oneS, BigDecimal.Ten.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect double value", oneD, BigDecimal.Ten.ToDouble(), 0);
        }

        /**
         * check ZERO
         */
        [Test]
        public void testFieldZERO()
        {
            String oneS = "0";
            double oneD = 0.0;
            assertEquals("incorrect string value", oneS, BigDecimal.Zero.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect double value", oneD, BigDecimal.Zero.ToDouble(), 0);
        }

        /**
         * new BigDecimal(BigInteger value)
         */
        [Test]
        public void testConstrBI()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            BigInteger bA = BigInteger.Parse(a);
            BigDecimal aNumber = new BigDecimal(bA);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", 0, aNumber.Scale);

            try
            {
                new BigDecimal((BigInteger)null);
                fail("No NullPointerException");
            }
            catch (ArgumentNullException e)
            {
                //expected
            }
        }

        /**
         * new BigDecimal(BigInteger value, int scale)
         */
        [Test]
        public void testConstrBIScale()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            BigInteger bA = BigInteger.Parse(a);
            int aScale = 10;
            BigDecimal aNumber = new BigDecimal(bA, aScale);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(BigInteger value, MathContext)
         */
        [Test]
        public void testConstrBigIntegerMathContext()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            BigInteger bA = BigInteger.Parse(a);
            int precision = 46;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "1231212478987482988429808779810457634781384757";
            int resScale = -6;
            BigDecimal result = new BigDecimal(bA, mc);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(BigInteger value, int scale, MathContext)
         */
        [Test]
        public void testConstrBigIntegerScaleMathContext()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            BigInteger bA = BigInteger.Parse(a);
            int aScale = 10;
            int precision = 46;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "1231212478987482988429808779810457634781384757";
            int resScale = 4;
            BigDecimal result = new BigDecimal(bA, aScale, mc);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(char[] value); 
         */
        [Test]
        public void testConstrChar()
        {
            char[] value = new char[] { '-', '1', '2', '3', '8', '0', '.', '4', '7', '3', '8', 'E', '-', '4', '2', '3' };
            BigDecimal result = BigDecimal.Parse(value, CultureInfo.InvariantCulture);
            String res = "-1.23804738E-419";
            int resScale = 427;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);

            try
            {
                // Regression for HARMONY-783
                BigDecimal.Parse(new char[] { }, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(char[] value, int offset, int len); 
         */
        [Test]
        public void testConstrCharIntInt()
        {
            char[] value = new char[] { '-', '1', '2', '3', '8', '0', '.', '4', '7', '3', '8', 'E', '-', '4', '2', '3' };
            int offset = 3;
            int len = 12;
            BigDecimal result = BigDecimal.Parse(value, offset, len, CultureInfo.InvariantCulture);
            String res = "3.804738E-40";
            int resScale = 46;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);

            try
            {
                // Regression for HARMONY-783
                BigDecimal.Parse(new char[] { }, 0, 0, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(char[] value, int offset, int len, MathContext mc); 
         */
        [Test]
        public void testConstrCharIntIntMathContext()
        {
            char[] value = new char[] { '-', '1', '2', '3', '8', '0', '.', '4', '7', '3', '8', 'E', '-', '4', '2', '3' };
            int offset = 3;
            int len = 12;
            int precision = 4;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            BigDecimal result = BigDecimal.Parse(value, offset, len, mc, CultureInfo.InvariantCulture);
            String res = "3.805E-40";
            int resScale = 43;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);

            try
            {
                // Regression for HARMONY-783
                BigDecimal.Parse(new char[] { }, 0, 0, MathContext.Decimal32, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(char[] value, int offset, int len, MathContext mc); 
         */
        [Test]
        public void testConstrCharIntIntMathContextException1()
        {
            char[] value = new char[] { '-', '1', '2', '3', '8', '0', '.', '4', '7', '3', '8', 'E', '-', '4', '2', '3' };
            int offset = 3;
            int len = 120;
            int precision = 4;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            try
            {
                BigDecimal.Parse(value, offset, len, mc, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (ArgumentOutOfRangeException e) // ICU4N: We throw ArgumentOutOfRangeException in this case to match .NET
            {
            }
        }

        /**
         * new BigDecimal(char[] value, int offset, int len, MathContext mc); 
         */
        [Test]
        public void testConstrCharIntIntMathContextException2()
        {
            char[] value = new char[] { '-', '1', '2', '3', '8', '0', ',', '4', '7', '3', '8', 'E', '-', '4', '2', '3' };
            int offset = 3;
            int len = 120;
            int precision = 4;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            try
            {
                BigDecimal.Parse(value, offset, len, mc, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (ArgumentOutOfRangeException e) // ICU4N: We throw ArgumentOutOfRangeException in this case to match .NET
            {
            }
        }

        /**
         * new BigDecimal(char[] value, MathContext mc);
         */
        [Test]
        public void testConstrCharMathContext()
        {
            try
            {
                // Regression for HARMONY-783
                BigDecimal.Parse(new char[] { }, MathContext.Decimal32, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been thrown");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(double value) when value is NaN
         */
        [Test]
        public void testConstrDoubleNaN()
        {
            double a = Double.NaN;
            try
            {
                new BigDecimal(a);
                fail("NumberFormatException has not been caught");
            }
            catch (OverflowException e)
            {
                assertEquals("Improper exception message", "Infinite or NaN", e
                        .Message);
            }
        }

        /**
         * new BigDecimal(double value) when value is positive infinity
         */
        [Test]
        public void testConstrDoublePosInfinity()
        {
            double a = double.PositiveInfinity;
            try
            {
                new BigDecimal(a);
                fail("NumberFormatException has not been caught");
            }
            catch (OverflowException e)
            {
                assertEquals("Improper exception message", "Infinite or NaN",
                        e.Message);
            }
        }

        /**
         * new BigDecimal(double value) when value is positive infinity
         */
        [Test]
        public void testConstrDoubleNegInfinity()
        {
            double a = double.NegativeInfinity;
            try
            {
                new BigDecimal(a);
                fail("NumberFormatException has not been caught");
            }
            catch (OverflowException e)
            {
                assertEquals("Improper exception message", "Infinite or NaN",
                        e.Message);
            }
        }

        /**
         * new BigDecimal(double value)
         */
        [Test]
        public void testConstrDouble()
        {
            double a = 732546982374982347892379283571094797.287346782359284756;
            int aScale = 0;
            BigInteger bA = BigInteger.Parse("732546982374982285073458350476230656");
            BigDecimal aNumber = new BigDecimal(a);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(double, MathContext)
         */
        [Test]
        public void testConstrDoubleMathContext()
        {
            double a = 732546982374982347892379283571094797.287346782359284756;
            int precision = 21;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "732546982374982285074";
            int resScale = -15;
            BigDecimal result = new BigDecimal(a, mc);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(0.1)
         */
        [Test]
        public void testConstrDouble01()
        {
            // "E" represents the exponent of 10, so the value of "1.E-1" is
            // equivalent to "1 * 10^-1", which is equal to 0.1. The line of code assigns
            // the value of 0.1 to the variable "a", which is of type "double"
            // (a double-precision floating-point number).

            //double a = 1.E-1;
            double a = 0.1;
            int aScale = 55;
            BigInteger bA = BigInteger.Parse("1000000000000000055511151231257827021181583404541015625");
            BigDecimal aNumber = new BigDecimal(a);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(0.555)
         */
        [Test]
        public void testConstrDouble02()
        {
            double a = 0.555;
            int aScale = 53;
            BigInteger bA = BigInteger.Parse("55500000000000004884981308350688777863979339599609375");
            BigDecimal aNumber = new BigDecimal(a);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(-0.1)
         */
        [Test]
        public void testConstrDoubleMinus01()
        {
            // In C#, the "double" data type is used to represent a double-precision floating-point number,
            // which is a numerical data type that can store very large or very small numbers with a high
            // degree of precision. The line of code assigns the value of "-1.E - 1" to the variable "a".
            // In this case, "E" represents the exponent of 10, so the value of "-1.E - 1" is equivalent
            // to "-1 * 10^-1", which is equal to -0.1.

            //double a = -1.E - 1;
            double a = -0.1;
            int aScale = 55;
            BigInteger bA = BigInteger.Parse("-1000000000000000055511151231257827021181583404541015625");
            BigDecimal aNumber = new BigDecimal(a);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(int value)
         */
        [Test]
        public void testConstrInt()
        {
            int a = 732546982;
            String res = "732546982";
            int resScale = 0;
            BigDecimal result = new BigDecimal(a);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(int, MathContext)
         */
        [Test]
        public void testConstrIntMathContext()
        {
            int a = 732546982;
            int precision = 21;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "732546982";
            int resScale = 0;
            BigDecimal result = new BigDecimal(a, mc);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(long value)
         */
        [Test]
        public void testConstrLong()
        {
            long a = 4576578677732546982L;
            String res = "4576578677732546982";
            int resScale = 0;
            BigDecimal result = new BigDecimal(a);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(long, MathContext)
         */
        [Test]
        public void testConstrLongMathContext()
        {
            long a = 4576578677732546982L;
            int precision = 5;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "45766";
            int resScale = -14;
            BigDecimal result = new BigDecimal(a, mc);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * new BigDecimal(double value) when value is denormalized
         */
        [Test]
        public void testConstrDoubleDenormalized()
        {
            double a = 2.274341322658976E-309;
            int aScale = 1073;
            BigInteger bA = BigInteger.Parse("227434132265897633950269241702666687639731047124115603942986140264569528085692462493371029187342478828091760934014851133733918639492582043963243759464684978401240614084312038547315281016804838374623558434472007664427140169018817050565150914041833284370702366055678057809362286455237716100382057360123091641959140448783514464639706721250400288267372238950016114583259228262046633530468551311769574111763316146065958042194569102063373243372766692713192728878701004405568459288708477607744497502929764155046100964958011009313090462293046650352146796805866786767887226278836423536035611825593567576424943331337401071583562754098901412372708947790843318760718495117047155597276492717187936854356663665005157041552436478744491526494952982062613955349661409854888916015625");
            BigDecimal aNumber = new BigDecimal(a);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value)
         * when value is not a valid representation of BigDecimal.
         */
        [Test]
        public void testConstrStringException()
        {
            String a = "-238768.787678287a+10";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e) { }
        }

        /**
         * new BigDecimal(String value) when exponent is empty.
         */
        public void testConstrStringExceptionEmptyExponent1()
        {
            String a = "-238768.787678287e";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(String value) when exponent is empty.
         */
        [Test]
        public void testConstrStringExceptionEmptyExponent2()
        {
            String a = "-238768.787678287e-";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * new BigDecimal(String value) when exponent is greater than
         * Integer.MAX_VALUE.
         */
        [Test]
        public void testConstrStringExceptionExponentGreaterIntegerMax()
        {
            String a = "-238768.787678287e214748364767876";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been caught");
            }
            catch (OverflowException e) // ICU4N: We throw OverflowException here to match .NET.
            {
            }
        }

        /**
         * new BigDecimal(String value) when exponent is less than
         * Integer.MIN_VALUE.
         */
        [Test]
        public void testConstrStringExceptionExponentLessIntegerMin()
        {
            String a = "-238768.787678287e-214748364767876";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException has not been caught");
            }
            catch (OverflowException e) // ICU4N: We throw OverflowException here to match .NET.
            {
            }
        }

        /**
         * new BigDecimal(String value)
         * when exponent is Integer.MAX_VALUE.
         */
        [Test]
        public void testConstrStringExponentIntegerMax()
        {
            String a = "-238768.787678287e2147483647";
            int aScale = -2147483638;
            BigInteger bA = BigInteger.Parse("-238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value)
         * when exponent is Integer.MIN_VALUE.
         */
        [Test]
        public void testConstrStringExponentIntegerMin()
        {
            String a = ".238768e-2147483648";
            try
            {
                BigDecimal.Parse(a, CultureInfo.InvariantCulture);
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
                assertEquals("Improper exception message", "Scale out of range.",
                    e.Message);
            }
        }

        /**
         * new BigDecimal(String value); value does not contain exponent
         */
        [Test]
        public void testConstrStringWithoutExpPos1()
        {
            String a = "732546982374982347892379283571094797.287346782359284756";
            int aScale = 18;
            BigInteger bA = BigInteger.Parse("732546982374982347892379283571094797287346782359284756");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value does not contain exponent
         */
        [Test]
        public void testConstrStringWithoutExpPos2()
        {
            String a = "+732546982374982347892379283571094797.287346782359284756";
            int aScale = 18;
            BigInteger bA = BigInteger.Parse("732546982374982347892379283571094797287346782359284756");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value does not contain exponent
         */
        [Test]
        public void testConstrStringWithoutExpNeg()
        {
            String a = "-732546982374982347892379283571094797.287346782359284756";
            int aScale = 18;
            BigInteger bA = BigInteger.Parse("-732546982374982347892379283571094797287346782359284756");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value does not contain exponent
         * and decimal point
         */
        [Test]
        public void testConstrStringWithoutExpWithoutPoint()
        {
            String a = "-732546982374982347892379283571094797287346782359284756";
            int aScale = 0;
            BigInteger bA = BigInteger.Parse("-732546982374982347892379283571094797287346782359284756");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value contains exponent
         * and does not contain decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithoutPoint1()
        {
            String a = "-238768787678287e214";
            int aScale = -214;
            BigInteger bA = BigInteger.Parse("-238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value contains exponent
         * and does not contain decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithoutPoint2()
        {
            String a = "-238768787678287e-214";
            int aScale = 214;
            BigInteger bA = BigInteger.Parse("-238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value contains exponent
         * and does not contain decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithoutPoint3()
        {
            String a = "238768787678287e-214";
            int aScale = 214;
            BigInteger bA = BigInteger.Parse("238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value contains exponent
         * and does not contain decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithoutPoint4()
        {
            String a = "238768787678287e+214";
            int aScale = -214;
            BigInteger bA = BigInteger.Parse("238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); value contains exponent
         * and does not contain decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithoutPoint5()
        {
            String a = "238768787678287E214";
            int aScale = -214;
            BigInteger bA = BigInteger.Parse("238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); 
         * value contains both exponent and decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithPoint1()
        {
            String a = "23985439837984782435652424523876878.7678287e+214";
            int aScale = -207;
            BigInteger bA = BigInteger.Parse("239854398379847824356524245238768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); 
         * value contains both exponent and decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithPoint2()
        {
            String a = "238096483923847545735673567457356356789029578490276878.7678287e-214";
            int aScale = 221;
            BigInteger bA = BigInteger.Parse("2380964839238475457356735674573563567890295784902768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); 
         * value contains both exponent and decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithPoint3()
        {
            String a = "2380964839238475457356735674573563567890.295784902768787678287E+21";
            int aScale = 0;
            BigInteger bA = BigInteger.Parse("2380964839238475457356735674573563567890295784902768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); 
         * value contains both exponent and decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithPoint4()
        {
            String a = "23809648392384754573567356745735635678.90295784902768787678287E+21";
            int aScale = 2;
            BigInteger bA = BigInteger.Parse("2380964839238475457356735674573563567890295784902768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value); 
         * value contains both exponent and decimal point
         */
        [Test]
        public void testConstrStringWithExponentWithPoint5()
        {
            String a = "238096483923847545735673567457356356789029.5784902768787678287E+21";
            int aScale = -2;
            BigInteger bA = BigInteger.Parse("2380964839238475457356735674573563567890295784902768787678287");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", bA, aNumber.UnscaledValue);
            assertEquals("incorrect scale", aScale, aNumber.Scale);
        }

        /**
         * new BigDecimal(String value, MathContext)
         */
        [Test]
        public void testConstrStringMathContext()
        {
            String a = "-238768787678287e214";
            int precision = 5;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String res = "-23876";
            int resScale = -224;
            BigDecimal result = BigDecimal.Parse(a, mc, CultureInfo.InvariantCulture);
            assertEquals("incorrect value", res, result.UnscaledValue.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }
    }
}
