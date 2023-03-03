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
    /// Method: toString(int radix)
    /// </summary>
    public class BigIntegerToStringTest : TestFmwk
    {
        /**
         * If 36 < radix < 2 it should be set to 10
         */
        [Test]
        public void testRadixOutOfRange()
        {
            String value = "442429234853876401";
            int radix = 10;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(45);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test negative number of radix 2
         */
        [Test]
        public void testRadix2Neg()
        {
            String value = "-101001100010010001001010101110000101010110001010010101010101010101010101010101010101010101010010101";
            int radix = 2;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test positive number of radix 2
         */
        [Test]
        public void testRadix2Pos()
        {
            String value = "101000011111000000110101010101010101010001001010101010101010010101010101010000100010010";
            int radix = 2;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test negative number of radix 10
         */
        [Test]
        public void testRadix10Neg()
        {
            String value = "-2489756308572364789878394872984";
            int radix = 16;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test positive number of radix 10
         */
        [Test]
        public void testRadix10Pos()
        {
            String value = "2387627892347567398736473476";
            int radix = 16;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test negative number of radix 16
         */
        [Test]
        public void testRadix16Neg()
        {
            String value = "-287628a883451b800865c67e8d7ff20";
            int radix = 16;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test positive number of radix 16
         */
        [Test]
        public void testRadix16Pos()
        {
            String value = "287628a883451b800865c67e8d7ff20";
            int radix = 16;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test negative number of radix 24
         */
        [Test]
        public void testRadix24Neg()
        {
            String value = "-287628a88gmn3451b8ijk00865c67e8d7ff20";
            int radix = 24;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test positive number of radix 24
         */
        [Test]
        public void testRadix24Pos()
        {
            String value = "287628a883451bg80ijhk0865c67e8d7ff20";
            int radix = 24;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test negative number of radix 24
         */
        [Test]
        public void testRadix36Neg()
        {
            String value = "-uhguweut98iu4h3478tq3985pq98yeiuth33485yq4aiuhalai485yiaehasdkr8tywi5uhslei8";
            int radix = 36;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }

        /**
         * test positive number of radix 24
         */
        [Test]
        public void testRadix36Pos()
        {
            String value = "23895lt45y6vhgliuwhgi45y845htsuerhsi4586ysuerhtsio5y68peruhgsil4568ypeorihtse48y6";
            int radix = 36;
            BigInteger aNumber = BigInteger.Parse(value, radix);
            String result = aNumber.ToString(radix);
            assertTrue("incorrect result", result.Equals(value));
        }
    }
}
