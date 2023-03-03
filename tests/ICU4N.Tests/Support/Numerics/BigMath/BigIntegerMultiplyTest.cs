using ICU4N.Dev.Test;
using NUnit.Framework;
using System;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigInteger
    /// Method: multiply
    /// </summary>
    public class BigIntegerMultiplyTest : TestFmwk
    {
        /**
         * Multiply two negative numbers of the same length
         */
        [Test]
        public void testCase1()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 10, 40, 100, unchecked((byte)-55), 96, 51, 76, 40, unchecked((byte)-45), 85, 105, 4, 28, unchecked((byte)-86), unchecked((byte)-117), unchecked((byte)-52), 100, 120, 90 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Multiply two numbers of the same length and different signs.
         * The first is negative.
         */
        [Test]
        public void testCase2()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-11), unchecked((byte)-41), unchecked((byte)-101), 54, unchecked((byte)-97), unchecked((byte)-52), unchecked((byte)-77), unchecked((byte)-41), 44, unchecked((byte)-86), unchecked((byte)-106), unchecked((byte)-5), unchecked((byte)-29), 85, 116, 51, unchecked((byte)-101), unchecked((byte)-121), unchecked((byte)-90) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Multiply two positive numbers of different length.
         * The first is longer.
         */
        [Test]
        public void testCase3()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = {10, 40, 100, unchecked((byte)-55), 96, 51, 76, 40, unchecked((byte)-45), 85, 115, 44, unchecked((byte)-127),
                         115, unchecked((byte)-21), unchecked((byte)-62), unchecked((byte)-15), 85, 64, unchecked((byte)-87), unchecked((byte)-2), unchecked((byte)-36), unchecked((byte)-36), unchecked((byte)-106)};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Multiply two positive numbers of different length.
         * The second is longer.
         */
        [Test]
        public void testCase4()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = {10, 40, 100, unchecked((byte)-55), 96, 51, 76, 40, unchecked((byte)-45), 85, 115, 44, unchecked((byte)-127),
                         115, unchecked((byte)-21), unchecked((byte)-62), unchecked((byte)-15), 85, 64, unchecked((byte)-87), unchecked((byte)-2), unchecked((byte)-36), unchecked((byte)-36), unchecked((byte)-106)};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Multiply two numbers of different length and different signs.
         * The first is positive.
         * The first is longer.
         */
        [Test]
        public void testCase5()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = {unchecked((byte)-11), unchecked((byte)-41), unchecked((byte)-101), 54, unchecked((byte)-97), unchecked((byte)-52), unchecked((byte)-77), unchecked((byte)-41), 44, unchecked((byte)-86), unchecked((byte)-116), unchecked((byte)-45), 126,
                         unchecked((byte)-116), 20, 61, 14, unchecked((byte)-86), unchecked((byte)-65), 86, 1, 35, 35, 106};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Multiply two numbers of different length and different signs.
         * The first is positive.
         * The second is longer.
         */
        [Test]
        public void testCase6()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = {unchecked((byte)-11), unchecked((byte)-41), unchecked((byte)-101), 54, unchecked((byte)-97), unchecked((byte)-52), unchecked((byte)-77), unchecked((byte)-41), 44, unchecked((byte)-86), unchecked((byte)-116), unchecked((byte)-45), 126,
                         unchecked((byte)-116), 20, 61, 14, unchecked((byte)-86), unchecked((byte)-65), 86, 1, 35, 35, 106};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Multiply a number by zero.
         */
        [Test]
        public void testCase7()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            byte[] bBytes = { 0 };
            int aSign = 1;
            int bSign = 0;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Multiply a number by ZERO.
         */
        [Test]
        public void testCase8()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            int aSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Multiply a positive number by ONE.
         */
        [Test]
        public void testCase9()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            int aSign = 1;
            byte[] rBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Multiply a negative number by ONE.
         */
        [Test]
        public void testCase10()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 1, 2, 3, 4, 5 };
            int aSign = -1;
            byte[] rBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-6), unchecked((byte)-7), unchecked((byte)-8), unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-5) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Multiply two numbers of 4 bytes length.
         */
        [Test]
        public void testIntbyInt1()
        {
            byte[] aBytes = { 10, 20, 30, 40 };
            byte[] bBytes = { 1, 2, 3, 4 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-11), unchecked((byte)-41), unchecked((byte)-101), 55, 5, 15, 96 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Multiply two numbers of 4 bytes length.
         */
        [Test]
        public void testIntbyInt2()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            byte[] bBytes = { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), 0, 0, 0, 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber * (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Negative exponent.
         */
        [Test]
        public void testPowException()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            int aSign = 1;
            int exp = -5;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            try
            {
                BigInteger.Pow(aNumber, exp);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Negative exponent", e.Message);
            }
        }

        /**
         * Exponentiation of a negative number to an odd exponent.
         */
        [Test]
        public void testPowNegativeNumToOddExp()
        {
            byte[] aBytes = { 50, unchecked((byte)-26), 90, 69, 120, 32, 63, unchecked((byte)-103), unchecked((byte)-14), 35 };
            int aSign = -1;
            int exp = 5;
            byte[] rBytes = {unchecked((byte)-21), unchecked((byte)-94), unchecked((byte)-42), unchecked((byte)-15), unchecked((byte)-127), 113, unchecked((byte)-50), unchecked((byte)-88), 115, unchecked((byte)-35), 3,
            59, unchecked((byte)-92), 111, unchecked((byte)-75), 103, unchecked((byte)-42), 41, 34, unchecked((byte)-114), 99, unchecked((byte)-32), 105, unchecked((byte)-59), 127,
            45, 108, 74, unchecked((byte)-93), 105, 33, 12, unchecked((byte)-5), unchecked((byte)-20), 17, unchecked((byte)-21), unchecked((byte)-119), unchecked((byte)-127), unchecked((byte)-115),
            27, unchecked((byte)-122), 26, unchecked((byte)-67), 109, unchecked((byte)-125), 16, 91, unchecked((byte)-70), 109};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Pow(aNumber, exp);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Exponentiation of a negative number to an even exponent.
         */
        [Test]
        public void testPowNegativeNumToEvenExp()
        {
            byte[] aBytes = { 50, unchecked((byte)-26), 90, 69, 120, 32, 63, unchecked((byte)-103), unchecked((byte)-14), 35 };
            int aSign = -1;
            int exp = 4;
            byte[] rBytes = {102, 107, unchecked((byte)-122), unchecked((byte)-43), unchecked((byte)-52), unchecked((byte)-20), unchecked((byte)-27), 25, unchecked((byte)-9), 88, unchecked((byte)-13),
            75, 78, 81, unchecked((byte)-33), unchecked((byte)-77), 39, 27, unchecked((byte)-37), 106, 121, unchecked((byte)-73), 108, unchecked((byte)-47), unchecked((byte)-101),
            80, unchecked((byte)-25), 71, 13, 94, unchecked((byte)-7), unchecked((byte)-33), 1, unchecked((byte)-17), unchecked((byte)-65), unchecked((byte)-70), unchecked((byte)-61), unchecked((byte)-3), unchecked((byte)-47)};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Pow(aNumber, exp);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Exponentiation of a negative number to zero exponent.
         */
        [Test]
        public void testPowNegativeNumToZeroExp()
        {
            byte[] aBytes = { 50, unchecked((byte)-26), 90, 69, 120, 32, 63, unchecked((byte)-103), unchecked((byte)-14), 35 };
            int aSign = -1;
            int exp = 0;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Pow(aNumber, exp);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Exponentiation of a positive number.
         */
        [Test]
        public void testPowPositiveNum()
        {
            byte[] aBytes = { 50, unchecked((byte)-26), 90, 69, 120, 32, 63, unchecked((byte)-103), unchecked((byte)-14), 35 };
            int aSign = 1;
            int exp = 5;
            byte[] rBytes = {20, 93, 41, 14, 126, unchecked((byte)-114), 49, 87, unchecked((byte)-116), 34, unchecked((byte)-4), unchecked((byte)-60),
            91, unchecked((byte)-112), 74, unchecked((byte)-104), 41, unchecked((byte)-42), unchecked((byte)-35), 113, unchecked((byte)-100), 31, unchecked((byte)-106), 58, unchecked((byte)-128),
            unchecked((byte)-46), unchecked((byte)-109), unchecked((byte)-75), 92, unchecked((byte)-106), unchecked((byte)-34), unchecked((byte)-13), 4, 19, unchecked((byte)-18), 20, 118, 126, 114,
            unchecked((byte)-28), 121, unchecked((byte)-27), 66, unchecked((byte)-110), 124, unchecked((byte)-17), unchecked((byte)-92), 69, unchecked((byte)-109)};
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Pow(aNumber, exp);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Exponentiation of a negative number to zero exponent.
         */
        [Test]
        public void testPowPositiveNumToZeroExp()
        {
            byte[] aBytes = { 50, unchecked((byte)-26), 90, 69, 120, 32, 63, unchecked((byte)-103), unchecked((byte)-14), 35 };
            int aSign = 1;
            int exp = 0;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Pow(aNumber, exp);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }
    }
}
