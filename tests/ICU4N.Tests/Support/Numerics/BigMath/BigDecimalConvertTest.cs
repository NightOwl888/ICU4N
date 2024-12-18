﻿using ICU4N.Dev.Test;
using J2N;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigDecimal
    /// Methods: doubleValue, floatValue, intValue, longValue,
    /// valueOf, toString, toBigInteger
    /// </summary>
    public class BigDecimalConvertTest : TestFmwk
    {
        /**
         * Double value of a negative BigDecimal
         */
        [Test]
        public void testDoubleValueNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            double result = -1.2380964839238476E53;
            assertEquals("incorrect value", result, aNumber.ToDouble(), 0);
        }

        /**
         * Double value of a positive BigDecimal
         */
        [Test]
        public void testDoubleValuePos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            double result = 1.2380964839238476E53;
            assertEquals("incorrect value", result, aNumber.ToDouble(), 0);
        }

        /**
         * Double value of a large positive BigDecimal
         */
        [Test]
        public void testDoubleValuePosInfinity()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            double result = double.PositiveInfinity;
            assertEquals("incorrect value", result, aNumber.ToDouble(), 0);
        }

        /**
         * Double value of a large negative BigDecimal
         */
        [Test]
        public void testDoubleValueNegInfinity()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            double result = double.NegativeInfinity;
            assertEquals("incorrect value", result, aNumber.ToDouble(), 0);
        }

        /**
         * Double value of a small negative BigDecimal
         */
        [Test]
        public void testDoubleValueMinusZero()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            long minusZero = -9223372036854775808L;
            double result = aNumber.ToDouble();
            assertTrue("incorrect value", BitConversion.DoubleToInt64Bits(result) == minusZero);
        }

        /**
         * Double value of a small positive BigDecimal
         */
        [Test]
        public void testDoubleValuePlusZero()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E-400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            long zero = 0;
            double result = aNumber.ToDouble();
            assertTrue("incorrect value", BitConversion.DoubleToInt64Bits(result) == zero);
        }

        /**
         * Float value of a negative BigDecimal
         */
        [Test]
        public void testFloatValueNeg()
        {
            String a = "-1238096483923847.6356789029578E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            float result = -1.2380965E36F;
            assertTrue("incorrect value", aNumber.ToSingle() == result);
        }

        /**
         * Float value of a positive BigDecimal
         */
        [Test]
        public void testFloatValuePos()
        {
            String a = "1238096483923847.6356789029578E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            float result = 1.2380965E36F;
            assertTrue("incorrect value", aNumber.ToSingle() == result);
        }

        /**
         * Float value of a large positive BigDecimal
         */
        [Test]
        public void testFloatValuePosInfinity()
        {
            String a = "123809648373567356745735.6356789787678287E+200";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            float result = float.PositiveInfinity;
            assertTrue("incorrect value", aNumber.ToSingle() == result);
        }

        /**
         * Float value of a large negative BigDecimal
         */
        [Test]
        public void testFloatValueNegInfinity()
        {
            String a = "-123809648392384755735.63567887678287E+200";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            float result = float.NegativeInfinity;
            assertTrue("incorrect value", aNumber.ToSingle() == result);
        }

        /**
         * Float value of a small negative BigDecimal
         */
        [Test]
        public void testFloatValueMinusZero()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int minusZero = -2147483648;
            float result = aNumber.ToSingle();
            assertTrue("incorrect value", BitConversion.SingleToInt32Bits(result) == minusZero);
        }

        /**
         * Float value of a small positive BigDecimal
         */
        [Test]
        public void testFloatValuePlusZero()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E-400";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int zero = 0;
            float result = aNumber.ToSingle();
            assertTrue("incorrect value", BitConversion.SingleToInt32Bits(result) == zero);
        }

        /**
         * Integer value of a negative BigDecimal
         */
        [Test]
        public void testIntValueNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int result = 218520473;
            assertTrue("incorrect value", aNumber.ToInt32() == result);
        }

        /**
         * Integer value of a positive BigDecimal
         */
        [Test]
        public void testIntValuePos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int result = -218520473;
            assertTrue("incorrect value", aNumber.ToInt32() == result);
        }

        /**
         * Long value of a negative BigDecimal
         */
        [Test]
        public void testLongValueNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            long result = -1246043477766677607L;
            assertTrue("incorrect value", aNumber.ToInt64() == result);
        }

        /**
         * Long value of a positive BigDecimal
         */
        [Test]
        public void testLongValuePos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            long result = 1246043477766677607L;
            assertTrue("incorrect value", aNumber.ToInt64() == result);
        }

        /**
         * scaleByPowerOfTen(int n)
         */
        [Test]
        public void testScaleByPowerOfTen1()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 13;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.ScaleByPowerOfTen(aNumber, 10);
            String res = "1231212478987482988429808779810457634781384756794.987";
            int resScale = 3;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * scaleByPowerOfTen(int n)
         */
        [Test]
        public void testScaleByPowerOfTen2()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -13;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.ScaleByPowerOfTen(aNumber, 10);
            String res = "1.231212478987482988429808779810457634781384756794987E+74";
            int resScale = -23;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Convert a positive BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerPos1()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigInteger bNumber = BigInteger.Parse("123809648392384754573567356745735635678902957849027687");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a positive BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerPos2()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+15";
            BigInteger bNumber = BigInteger.Parse("123809648392384754573567356745735635678902957849");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a positive BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerPos3()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+45";
            BigInteger bNumber = BigInteger.Parse("123809648392384754573567356745735635678902957849027687876782870000000000000000");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a negative BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerNeg1()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigInteger bNumber = BigInteger.Parse("-123809648392384754573567356745735635678902957849027687");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a negative BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerNeg2()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+15";
            BigInteger bNumber = BigInteger.Parse("-123809648392384754573567356745735635678902957849");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a negative BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerNeg3()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+45";
            BigInteger bNumber = BigInteger.Parse("-123809648392384754573567356745735635678902957849027687876782870000000000000000");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * Convert a small BigDecimal to BigInteger
         */
        [Test]
        public void testToBigIntegerZero()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-500";
            BigInteger bNumber = BigInteger.Parse("0");
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            BigInteger result = aNumber.ToBigInteger();
            assertTrue("incorrect value", result.Equals(bNumber));
        }

        /**
         * toBigIntegerExact()
         */
        [Test]
        public void testToBigIntegerExact1()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+45";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String res = "-123809648392384754573567356745735635678902957849027687876782870000000000000000";
            BigInteger result = aNumber.ToBigIntegerExact();
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * toBigIntegerExact()
         */
        [Test]
        public void testToBigIntegerExactException()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-10";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            try
            {
                aNumber.ToBigIntegerExact();
                fail("java.lang.ArithmeticException has not been thrown");
            }
            catch (ArithmeticException e)
            {
                return;
            }
        }

        /**
         * Convert a positive BigDecimal to an engineering string representation
         */
        [Test]
        public void testToEngineeringStringPos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E-501";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "123.80964839238475457356735674573563567890295784902768787678287E-471";
            assertEquals("incorrect value", result, aNumber.ToEngineeringString());
        }

        /**
         * Convert a negative BigDecimal to an engineering string representation
         */
        [Test]
        public void testToEngineeringStringNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-501";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "-123.80964839238475457356735674573563567890295784902768787678287E-471";
            assertEquals("incorrect value", result, aNumber.ToEngineeringString());
        }

        /**
         * Convert a negative BigDecimal to an engineering string representation
         */
        [Test]
        public void testToEngineeringStringZeroPosExponent()
        {
            String a = "0.0E+16";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "0E+15";
            assertEquals("incorrect value", result, aNumber.ToEngineeringString());
        }

        /**
         * Convert a negative BigDecimal to an engineering string representation
         */
        [Test]
        public void testToEngineeringStringZeroNegExponent()
        {
            String a = "0.0E-16";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "0.00E-15";
            assertEquals("incorrect value", result, aNumber.ToEngineeringString());
        }

        /**
         * Convert a negative BigDecimal with a negative exponent to a plain string
         * representation; scale == 0.
         */
        [Test]
        public void testToPlainStringNegNegExp()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E-100";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "-0.000000000000000000000000000000000000000000000000000000000000000000012380964839238475457356735674573563567890295784902768787678287";
            assertTrue("incorrect value", aNumber.ToPlainString().Equals(result));
        }

        /**
         * Convert a negative BigDecimal with a positive exponent
         * to a plain string representation;
         * scale == 0.
         */
        [Test]
        public void testToPlainStringNegPosExp()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E100";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "-1238096483923847545735673567457356356789029578490276878767828700000000000000000000000000000000000000000000000000000000000000000000000";
            assertTrue("incorrect value", aNumber.ToPlainString().Equals(result));
        }

        /**
         * Convert a positive BigDecimal with a negative exponent
         * to a plain string representation;
         * scale == 0.
         */
        [Test]
        public void testToPlainStringPosNegExp()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E-100";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "0.000000000000000000000000000000000000000000000000000000000000000000012380964839238475457356735674573563567890295784902768787678287";
            assertTrue("incorrect value", aNumber.ToPlainString().Equals(result));
        }

        /**
         * Convert a negative BigDecimal with a negative exponent
         * to a plain string representation;
         * scale == 0.
         */
        [Test]
        public void testToPlainStringPosPosExp()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+100";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "1238096483923847545735673567457356356789029578490276878767828700000000000000000000000000000000000000000000000000000000000000000000000";
            assertTrue("incorrect value", aNumber.ToPlainString().Equals(result));
        }

        /**
         * Convert a BigDecimal to a string representation;
         * scale == 0.
         */
        [Test]
        public void testToStringZeroScale()
        {
            String a = "-123809648392384754573567356745735635678902957849027687876782870";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a));
            String result = "-123809648392384754573567356745735635678902957849027687876782870";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Convert a positive BigDecimal to a string representation
         */
        [Test]
        public void testToStringPos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E-500";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "1.2380964839238475457356735674573563567890295784902768787678287E-468";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Convert a negative BigDecimal to a string representation
         */
        [Test]
        public void testToStringNeg()
        {
            String a = "-123.4564563673567380964839238475457356735674573563567890295784902768787678287E-5";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "-0.001234564563673567380964839238475457356735674573563567890295784902768787678287";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a positive long value; scale == 0
         */
        [Test]
        public void testValueOfPosZeroScale()
        {
            long a = 98374823947823578L;
            BigDecimal aNumber = BigDecimal.GetInstance(a);
            String result = "98374823947823578";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative long value; scale is 0
         */
        [Test]
        public void testValueOfNegZeroScale()
        {
            long a = -98374823947823578L;
            BigDecimal aNumber = BigDecimal.GetInstance(a);
            String result = "-98374823947823578";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative long value; scale is positive
         */
        [Test]
        public void testValueOfNegScalePos()
        {
            long a = -98374823947823578L;
            int scale = 12;
            BigDecimal aNumber = BigDecimal.GetInstance(a, scale);
            String result = "-98374.823947823578";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative long value; scale is negative
         */
        [Test]
        public void testValueOfNegScaleNeg()
        {
            long a = -98374823947823578L;
            int scale = -12;
            BigDecimal aNumber = BigDecimal.GetInstance(a, scale);
            String result = "-9.8374823947823578E+28";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative long value; scale is positive
         */
        [Test]
        public void testValueOfPosScalePos()
        {
            long a = 98374823947823578L;
            int scale = 12;
            BigDecimal aNumber = BigDecimal.GetInstance(a, scale);
            String result = "98374.823947823578";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative long value; scale is negative
         */
        [Test]
        public void testValueOfPosScaleNeg()
        {
            long a = 98374823947823578L;
            int scale = -12;
            BigDecimal aNumber = BigDecimal.GetInstance(a, scale);
            String result = "9.8374823947823578E+28";
            assertTrue("incorrect value", aNumber.ToString(CultureInfo.InvariantCulture).Equals(result));
        }

        /**
         * Create a BigDecimal from a negative double value
         */
        [Test]
        public void testValueOfDoubleNeg()
        {
            double a = -65678765876567576.98788767;
            BigDecimal result = new BigDecimal(a);
            String res = "-65678765876567576";
            int resScale = 0;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Create a BigDecimal from a positive double value
         */
        [Test]
        public void testValueOfDoublePos1()
        {
            double a = 65678765876567576.98788767;
            BigDecimal result = new BigDecimal(a);
            String res = "65678765876567576";
            int resScale = 0;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Create a BigDecimal from a positive double value
         */
        [Test]
        public void testValueOfDoublePos2()
        {
            double a = 12321237576.98788767;
            BigDecimal result = new BigDecimal(a);
            //String res = "12321237576.987888";
            //int resScale = 6;

            // In Java 8, the double type was updated to use the IEEE 754-2008 standard, which introduced
            // some changes to the representation of double values. For example, the double type now supports
            // subnormal numbers, which are numbers that are very close to zero but have a non-zero value.
            string res = "12321237576.987888336181640625"; // ICU4N: Between Java 5 and Java 8 the double type changed to be more precise. This result is what JDK 8 gives.
            int resScale = 18;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Create a BigDecimal from a positive double value
         */
        [Test]
        public void testValueOfDoublePos3()
        {
            double a = 12321237576.9878838;
            BigDecimal result = new BigDecimal(a);
            //String res = "12321237576.987885";
            //int resScale = 6;

            // In Java 8, the double type was updated to use the IEEE 754-2008 standard, which introduced
            // some changes to the representation of double values. For example, the double type now supports
            // subnormal numbers, which are numbers that are very close to zero but have a non-zero value.
            // This change can affect the precision of double values, particularly when they are close to zero.
            string res = "12321237576.987884521484375"; // ICU4N: Between Java 5 and Java 8 the double type changed to be more precise. This result is what JDK 8 gives.
            int resScale = 15;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * valueOf(Double.NaN)
         */
        [Test]
        public void testValueOfDoubleNaN()
        {
            double a = Double.NaN;
            try
            {
                new BigDecimal(a);
                fail("NumberFormatException has not been thrown for Double.NaN");
            }
            catch (OverflowException e)
            {
                return;
            }
        }

        // ICU4N specific
        /// <summary>
        /// Tests converting from BigInteger in little-endian format,
        /// since .NET doesn't support exporting big endian bytes prior
        /// to .NET Core 2.1/.NET Standard 2.1.
        /// </summary>
        [Test]
        // Positive test cases
        [TestCase("400000000000030299193828888", "400000000000030299193828888")]
        [TestCase("2147483648", "2147483648")] // Produces 5 bytes
        [TestCase("12345678901234567890137641668102549", "12345678901234567890137641668102549")] // Produces 15 bytes
        [TestCase("123456789012345678904641671296476", "123456789012345678904641671296476")] // Produces 14 bytes
        [TestCase("123456789012345678901568102528", "123456789012345678901568102528")] // Produces 13 bytes

        // Negative test cases
        [TestCase("-400000000000030299193828888", "-400000000000030299193828888")]
        [TestCase("-2147483648", "-2147483648")] // Produces 5 bytes
        [TestCase("-12345678901234567890137641668102549", "-12345678901234567890137641668102549")] // Produces 15 bytes
        [TestCase("-123456789012345678904641671296476", "-123456789012345678904641671296476")] // Produces 14 bytes
        [TestCase("-123456789012345678901568102528", "-123456789012345678901568102528")] // Produces 13 bytes
        public void TestConvertFromLittleEndianByteArray(string number, string expected)
        {
            System.Numerics.BigInteger bi = System.Numerics.BigInteger.Parse(number);
            byte[] littleEndianBytes = bi.ToByteArray();

            ICU4N.Numerics.BigMath.BigInteger converted = new ICU4N.Numerics.BigMath.BigInteger(littleEndianBytes, isBigEndian: false);
            string actual = converted.ToString(CultureInfo.InvariantCulture);
            assertEquals($"incorrect conversion - expected: {expected}, actual: {actual}", expected, actual);
        }

        // ICU4N specific
        /// <summary>
        /// Tests converting from BigInteger in little-endian format,
        /// since .NET doesn't support exporting big endian bytes prior
        /// to .NET Core 2.1/.NET Standard 2.1.
        /// </summary>
        [Test]
        // Positive test cases
        [TestCase("400000000000030299193828888", "400000000000030299193828888")]
        [TestCase("2147483648", "2147483648")] // Produces 5 bytes
        [TestCase("12345678901234567890137641668102549", "12345678901234567890137641668102549")] // Produces 15 bytes
        [TestCase("123456789012345678904641671296476", "123456789012345678904641671296476")] // Produces 14 bytes
        [TestCase("123456789012345678901568102528", "123456789012345678901568102528")] // Produces 13 bytes

        // Negative test cases
        [TestCase("-400000000000030299193828888", "-400000000000030299193828888")]
        [TestCase("-2147483648", "-2147483648")] // Produces 5 bytes
        [TestCase("-12345678901234567890137641668102549", "-12345678901234567890137641668102549")] // Produces 15 bytes
        [TestCase("-123456789012345678904641671296476", "-123456789012345678904641671296476")] // Produces 14 bytes
        [TestCase("-123456789012345678901568102528", "-123456789012345678901568102528")] // Produces 13 bytes
        public void TestConvertFromBigEndianByteArray(string number, string expected)
        {
            ICU4N.Numerics.BigMath.BigInteger bi = ICU4N.Numerics.BigMath.BigInteger.Parse(number);
            byte[] bigEndianBytes = bi.ToByteArray();

            ICU4N.Numerics.BigMath.BigInteger converted = new ICU4N.Numerics.BigMath.BigInteger(bigEndianBytes, isBigEndian: true);
            string actual = converted.ToString(CultureInfo.InvariantCulture);
            assertEquals($"incorrect conversion - expected: {expected}, actual: {actual}", expected, actual);
        }
    }
}
