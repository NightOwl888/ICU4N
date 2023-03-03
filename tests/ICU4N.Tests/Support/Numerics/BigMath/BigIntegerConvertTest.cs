using ICU4N.Dev.Test;
using NUnit.Framework;
using System;

/**
 * @author Elena Semukhina
 */


namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:   java.math.BigInteger
    /// Methods: intValue, longValue, toByteArray(), valueOf(long val),
    /// floatValue(), doubleValue()
    /// </summary>
    public class BigIntegerConvertTest : TestFmwk
    {
        /**
         * Return the double value of ZERO. 
         */
        [Test]
        public void testDoubleValueZero()
        {
            String a = "0";
            double result = 0.0;
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * The number's length is less than 64 bits.
         */
        [Test]
        public void testDoubleValuePositive1()
        {
            String a = "27467238945";
            double result = 2.7467238945E10;
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * The number's bit length is inside [63, 1024].
         */
        [Test]
        public void testDoubleValuePositive2()
        {
            String a = "2746723894572364578265426346273456972";
            double result = 2.7467238945723645E36;
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a negative number to a double value. 
         * The number's bit length is less than 64 bits.
         */
        [Test]
        public void testDoubleValueNegative1()
        {
            String a = "-27467238945";
            double result = -2.7467238945E10;
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a negative number to a double value. 
         * The number's bit length is inside [63, 1024].
         */
        [Test]
        public void testDoubleValueNegative2()
        {
            String a = "-2746723894572364578265426346273456972";
            double result = -2.7467238945723645E36;
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 1.
         */
        [Test]
        public void testDoubleValuePosRounded1()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            double result = 1.54747264387948E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 0
         * but some of dropped bits are 1s.
         */
        [Test]
        public void testDoubleValuePosRounded2()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 36, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            double result = 1.547472643879479E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }
        /**
         * Convert a positive number to a double value. 
         * Rounding is NOT needed.
         */
        [Test]
        public void testDoubleValuePosNotRounded()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, unchecked((byte)-128), 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            double result = 1.5474726438794828E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * Rounding is needed.
         */
        [Test]
        public void testDoubleValueNegRounded1()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            double result = -1.54747264387948E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 0
         * but some of dropped bits are 1s.
         */
        [Test]
        public void testDoubleValueNegRounded2()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 36, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            double result = -1.547472643879479E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * Rounding is NOT needed.
         */
        [Test]
        public void testDoubleValueNegNotRounded()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, unchecked((byte)-128), 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            double result = -1.5474726438794828E26;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The rounding bit is 0.
         * The result is Double.MAX_VALUE.
         */
        [Test]
        public void testDoubleValuePosMaxValue()
        {
            byte[] a = {0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1)
            };
            int aSign = 1;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == double.MaxValue);
        }

        /**
         * Convert a negative number to a double value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The result is -Double.MAX_VALUE.
         */
        [Test]
        public void testDoubleValueNegMaxValue()
        {
            byte[] a = {0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1),
                unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1)
            };
            int aSign = -1;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == -double.MaxValue);
        }

        /**
         * Convert a positive number to a double value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The rounding bit is 1.
         * The result is Double.POSITIVE_INFINITY.
         */
        [Test]
        public void testDoubleValuePositiveInfinity1()
        {
            byte[] a = {unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            int aSign = 1;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == double.PositiveInfinity);
        }

        /**
         * Convert a positive number to a double value. 
         * The number's bit length is greater than 1024.
         */
        [Test]
        public void testDoubleValuePositiveInfinity2()
        {
            String a = "2746723894572364578265426346273456972283746872364768676747462342342342342342342342323423423423423423426767456345745293762384756238475634563456845634568934568347586346578648576478568456457634875673845678456786587345873645767456834756745763457863485768475678465783456702897830296720476846578634576384567845678346573465786457863";
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == double.PositiveInfinity);
        }

        /**
         * Convert a negative number to a double value. 
         * The number's bit length is greater than 1024.
         */
        [Test]
        public void testDoubleValueNegativeInfinity1()
        {
            String a = "-2746723894572364578265426346273456972283746872364768676747462342342342342342342342323423423423423423426767456345745293762384756238475634563456845634568934568347586346578648576478568456457634875673845678456786587345873645767456834756745763457863485768475678465783456702897830296720476846578634576384567845678346573465786457863";
            double aNumber = BigInteger.Parse(a).ToDouble();
            assertTrue("incorrect result", aNumber == double.NegativeInfinity);
        }

        /**
         * Convert a negative number to a double value. 
         * The exponent is 1023 and the mantissa is all 0s.
         * The rounding bit is 0.
         * The result is Double.NEGATIVE_INFINITY.
         */
        [Test]
        public void testDoubleValueNegativeInfinity2()
        {
            byte[] a = {unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            int aSign = -1;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == double.NegativeInfinity);
        }

        /**
         * Convert a positive number to a double value. 
         * The exponent is 1023 and the mantissa is all 0s
         * but the 54th bit (implicit) is 1.
         */
        [Test]
        public void testDoubleValuePosMantissaIsZero()
        {
            byte[] a = {unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            int aSign = 1;
            double result = 8.98846567431158E307;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * The exponent is 1023 and the mantissa is all 0s
         * but the 54th bit (implicit) is 1.
         */
        [Test]
        public void testDoubleValueNegMantissaIsZero()
        {
            byte[] a = {unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            int aSign = -1;
            double aNumber = new BigInteger(aSign, a).ToDouble();
            assertTrue("incorrect result", aNumber == -8.98846567431158E307);
        }

        /**
         * Return the float value of ZERO. 
         */
        [Test]
        public void testFloatValueZero()
        {
            String a = "0";
            float result = 0.0f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * The number's length is less than 32 bits.
         */
        [Test]
        public void testFloatValuePositive1()
        {
            String a = "27467238";
            float result = 2.7467238E7f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * The number's bit length is inside [32, 127].
         */
        [Test]
        public void testFloatValuePositive2()
        {
            String a = "27467238945723645782";
            float result = 2.7467239E19f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a negative number to a float value. 
         * The number's bit length is less than 32 bits.
         */
        [Test]
        public void testFloatValueNegative1()
        {
            String a = "-27467238";
            float result = -2.7467238E7f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a negative number to a doufloatble value. 
         * The number's bit length is inside [63, 1024].
         */
        [Test]
        public void testFloatValueNegative2()
        {
            String a = "-27467238945723645782";
            float result = -2.7467239E19f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 1.
         */
        [Test]
        public void testFloatValuePosRounded1()
        {
            byte[] a = { unchecked((byte)-128), 1, unchecked((byte)-1), unchecked((byte)-4), 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            float result = 1.5475195E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 0
         * but some of dropped bits are 1s.
         */
        [Test]
        public void testFloatValuePosRounded2()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, unchecked((byte)-128), 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            float result = 1.5474728E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }
        /**
         * Convert a positive number to a float value. 
         * Rounding is NOT needed.
         */
        [Test]
        public void testFloatValuePosNotRounded()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = 1;
            float result = 1.5474726E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * Rounding is needed.
         */
        [Test]
        public void testFloatValueNegRounded1()
        {
            byte[] a = { unchecked((byte)-128), 1, unchecked((byte)-1), unchecked((byte)-4), 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            float result = -1.5475195E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * Rounding is needed.
         * The rounding bit is 1 and the next bit to the left is 0
         * but some of dropped bits are 1s.
         */
        [Test]
        public void testFloatValueNegRounded2()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, unchecked((byte)-128), 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            float result = -1.5474728E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * Rounding is NOT needed.
         */
        [Test]
        public void testFloatValueNegNotRounded()
        {
            byte[] a = { unchecked((byte)-128), 1, 2, 3, 4, 5, 60, 23, 1, unchecked((byte)-3), unchecked((byte)-5) };
            int aSign = -1;
            float result = -1.5474726E26f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a float value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The rounding bit is 0.
         * The result is Float.MAX_VALUE.
         */
        [Test]
        public void testFloatValuePosMaxValue()
        {
            byte[] a = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = 1;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == float.MaxValue);
        }

        /**
         * Convert a negative number to a float value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The rounding bit is 0.
         * The result is -Float.MAX_VALUE.
         */
        [Test]
        public void testFloatValueNegMaxValue()
        {
            byte[] a = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-8), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = -1;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == -float.MaxValue);
        }

        /**
         * Convert a positive number to a float value. 
         * The exponent is 1023 and the mantissa is all 1s.
         * The rounding bit is 1.
         * The result is Float.POSITIVE_INFINITY.
         */
        [Test]
        public void testFloatValuePositiveInfinity1()
        {
            byte[] a = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = 1;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == float.PositiveInfinity);
        }

        /**
         * Convert a positive number to a float value. 
         * The number's bit length is greater than 127.
         */
        [Test]
        public void testFloatValuePositiveInfinity2()
        {
            String a = "2746723894572364578265426346273456972283746872364768676747462342342342342342342342323423423423423423426767456345745293762384756238475634563456845634568934568347586346578648576478568456457634875673845678456786587345873645767456834756745763457863485768475678465783456702897830296720476846578634576384567845678346573465786457863";
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == float.PositiveInfinity);
        }

        /**
         * Convert a negative number to a float value. 
         * The number's bit length is greater than 127.
         */
        [Test]
        public void testFloatValueNegativeInfinity1()
        {
            String a = "-2746723894572364578265426346273456972283746872364768676747462342342342342342342342323423423423423423426767456345745293762384756238475634563456845634568934568347586346578648576478568456457634875673845678456786587345873645767456834756745763457863485768475678465783456702897830296720476846578634576384567845678346573465786457863";
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == float.NegativeInfinity);
        }

        /**
         * Convert a negative number to a float value. 
         * The exponent is 1023 and the mantissa is all 0s.
         * The rounding bit is 0.
         * The result is Float.NEGATIVE_INFINITY.
         */
        [Test]
        public void testFloatValueNegativeInfinity2()
        {
            byte[] a = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = -1;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == float.NegativeInfinity);
        }

        /**
         * Convert a positive number to a float value. 
         * The exponent is 1023 and the mantissa is all 0s
         * but the 54th bit (implicit) is 1.
         */
        [Test]
        public void testFloatValuePosMantissaIsZero()
        {
            byte[] a = { unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = 1;
            float result = 1.7014118E38f;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive number to a double value. 
         * The exponent is 1023 and the mantissa is all 0s
         * but the 54th bit (implicit) is 1.
         */
        [Test]
        public void testFloatValueNegMantissaIsZero()
        {
            byte[] a = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            float aNumber = new BigInteger(aSign, a).ToSingle();
            assertTrue("incorrect result", aNumber == float.NegativeInfinity);
        }

        /**
         * Convert a negative number to a float value. 
         * The number's bit length is less than 32 bits.
         */
        [Test]
        public void testFloatValueBug2482()
        {
            String a = "2147483649";
            float result = 2.14748365E9f;
            float aNumber = BigInteger.Parse(a).ToSingle();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a positive BigInteger to an integer value. 
         * The low digit is positive
         */
        [Test]
        public void testIntValuePositive1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3 };
            int resInt = 1496144643;
            int aNumber = new BigInteger(aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == resInt);
        }

        /**
         * Convert a positive BigInteger to an integer value. 
         * The low digit is positive
         */
        [Test]
        public void testIntValuePositive2()
        {
            byte[] aBytes = { 12, 56, 100 };
            int resInt = 800868;
            int aNumber = new BigInteger(aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == resInt);
        }

        /**
         * Convert a positive BigInteger to an integer value. 
         * The low digit is negative.
         */
        [Test]
        public void testIntValuePositive3()
        {
            byte[] aBytes = { 56, 13, 78, unchecked((byte)-12), unchecked((byte)-5), 56, 100 };
            int sign = 1;
            int resInt = -184862620;
            int aNumber = new BigInteger(sign, aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == resInt);
        }

        /**
         * Convert a negative BigInteger to an integer value.
         * The low digit is negative.
         */
        [Test]
        public void testIntValueNegative1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), unchecked((byte)-128), 45, 91, 3 };
            int sign = -1;
            int resInt = 2144511229;
            int aNumber = new BigInteger(sign, aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == resInt);
        }

        /**
         * Convert a negative BigInteger to an integer value.
         * The low digit is negative.
         */
        [Test]
        public void testIntValueNegative2()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100 };
            int result = -771996;
            int aNumber = new BigInteger(aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a negative BigInteger to an integer value. 
         * The low digit is positive.
         */
        [Test]
        public void testIntValueNegative3()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 127, 45, 91, 3 };
            int sign = -1;
            int resInt = -2133678851;
            int aNumber = new BigInteger(sign, aBytes).ToInt32();
            assertTrue("incorrect result", aNumber == resInt);
        }

        /**
         * Convert a BigInteger to a positive long value
         * The BigInteger is longer than int.
         */
        [Test]
        public void testLongValuePositive1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, 120, unchecked((byte)-34), unchecked((byte)-12), 45, 98 };
            long result = 3268209772258930018L;
            long aNumber = new BigInteger(aBytes).ToInt64();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a number to a positive long value
         * The number fits in a long.
         */
        [Test]
        public void testLongValuePositive2()
        {
            byte[] aBytes = { 12, 56, 100, 18, unchecked((byte)-105), 34, unchecked((byte)-18), 45 };
            long result = 880563758158769709L;
            long aNumber = new BigInteger(aBytes).ToInt64();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a number to a negative long value
         * The BigInteger is longer than int.
         */
        [Test]
        public void testLongValueNegative1()
        {
            byte[] aBytes = { 12, unchecked((byte)-1), 100, unchecked((byte)-2), unchecked((byte)-76), unchecked((byte)-128), 45, 91, 3 };
            long result = -43630045168837885L;
            long aNumber = new BigInteger(aBytes).ToInt64();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * Convert a number to a negative long value
         * The number fits in a long.
         */
        [Test]
        public void testLongValueNegative2()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, 45, unchecked((byte)-101), 45, 98 };
            long result = -3315696807498398L;
            long aNumber = new BigInteger(aBytes).ToInt64();
            assertTrue("incorrect result", aNumber == result);
        }

        /**
         * valueOf (long val): convert Integer.MAX_VALUE to a BigInteger.
         */
        [Test]
        public void testValueOfIntegerMax()
        {
            long longVal = int.MaxValue;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { 127, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert Integer.MIN_VALUE to a BigInteger.
         */
        [Test]
        public void testValueOfIntegerMin()
        {
            long longVal = int.MinValue;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { unchecked((byte)-128), 0, 0, 0 };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert Long.MAX_VALUE to a BigInteger.
         */
        [Test]
        public void testValueOfLongMax()
        {
            long longVal = long.MaxValue;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { 127, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert Long.MIN_VALUE to a BigInteger.
         */
        [Test]
        public void testValueOfLongMin()
        {
            long longVal = long.MinValue;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0 };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert a positive long value to a BigInteger.
         */
        [Test]
        public void testValueOfLongPositive1()
        {
            long longVal = 268209772258930018L;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { 3, unchecked((byte)-72), unchecked((byte)-33), 93, unchecked((byte)-24), unchecked((byte)-56), 45, 98 };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert a positive long value to a BigInteger.
         * The long value fits in integer.
         */
        [Test]
        public void testValueOfLongPositive2()
        {
            long longVal = 58930018L;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { 3, unchecked((byte)-125), 51, 98 };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert a negative long value to a BigInteger.
         */
        [Test]
        public void testValueOfLongNegative1()
        {
            long longVal = -268209772258930018L;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { unchecked((byte)-4), 71, 32, unchecked((byte)-94), 23, 55, unchecked((byte)-46), unchecked((byte)-98) };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * valueOf (long val): convert a negative long value to a BigInteger.
         * The long value fits in integer.
         */
        [Test]
        public void testValueOfLongNegative2()
        {
            long longVal = -58930018L;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { unchecked((byte)-4), 124, unchecked((byte)-52), unchecked((byte)-98) };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }
        /**
         * valueOf (long val): convert a zero long value to a BigInteger.
         */
        [Test]
        public void testValueOfLongZero()
        {
            long longVal = 0L;
            BigInteger aNumber = BigInteger.GetInstance(longVal);
            byte[] rBytes = { 0 };
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect result", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }
    }
}
