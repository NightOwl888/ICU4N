using ICU4N.Dev.Test;
using NUnit.Framework;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigInteger
    /// Method: subtract
    /// </summary>
    public class BigIntegerSubtractTest : TestFmwk
    {
        /**
         * Subtract two positive numbers of the same length.
         * The first is greater.
         */
        [Test]
        public void testCase1()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 9, 18, 27, 36, 45, 54, 63, 9, 18, 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two positive numbers of the same length.
         * The second is greater.
         */
        [Test]
        public void testCase2()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-10), unchecked((byte)-19), unchecked((byte)-28), unchecked((byte)-37), unchecked((byte)-46), unchecked((byte)-55), unchecked((byte)-64), unchecked((byte)-10), unchecked((byte)-19), unchecked((byte)-27) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is positive.
         * The first is greater in absolute value.
         */
        [Test]
        public void testCase3()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 11, 22, 33, 44, 55, 66, 77, 11, 22, 33 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is positive.
         * The second is greater in absolute value.
         */
        [Test]
        public void testCase4()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 11, 22, 33, 44, 55, 66, 77, 11, 22, 33 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two negative numbers of the same length.
         * The first is greater in absolute value.
         */
        [Test]
        public void testCase5()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-10), unchecked((byte)-19), unchecked((byte)-28), unchecked((byte)-37), unchecked((byte)-46), unchecked((byte)-55), unchecked((byte)-64), unchecked((byte)-10), unchecked((byte)-19), unchecked((byte)-27) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two negative numbers of the same length.
         * The second is greater in absolute value.
         */
        [Test]
        public void testCase6()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 9, 18, 27, 36, 45, 54, 63, 9, 18, 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is negative.
         * The first is greater in absolute value.
         */
        [Test]
        public void testCase7()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-12), unchecked((byte)-23), unchecked((byte)-34), unchecked((byte)-45), unchecked((byte)-56), unchecked((byte)-67), unchecked((byte)-78), unchecked((byte)-12), unchecked((byte)-23), unchecked((byte)-33) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is negative.
         * The second is greater in absolute value.
         */
        [Test]
        public void testCase8()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-12), unchecked((byte)-23), unchecked((byte)-34), unchecked((byte)-45), unchecked((byte)-56), unchecked((byte)-67), unchecked((byte)-78), unchecked((byte)-12), unchecked((byte)-23), unchecked((byte)-33) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two positive numbers of different length.
         * The first is longer.
         */
        [Test]
        public void testCase9()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 1, 2, 3, 3, unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-24), unchecked((byte)-40), unchecked((byte)-49), unchecked((byte)-58), unchecked((byte)-67), unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-23) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two positive numbers of different length.
         * The second is longer.
         */
        [Test]
        public void testCase10()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two numbers of different length and different signs.
         * The first is positive.
         * The first is greater in absolute value.
         */
        [Test]
        public void testCase11()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 1, 2, 3, 4, 15, 26, 37, 41, 52, 63, 74, 15, 26, 37 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is positive.
         * The second is greater in absolute value.
         */
        [Test]
        public void testCase12()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 1, 2, 3, 4, 15, 26, 37, 41, 52, 63, 74, 15, 26, 37 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two numbers of different length and different signs.
         * The first is negative.
         * The first is longer.
         */
        [Test]
        public void testCase13()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-16), unchecked((byte)-27), unchecked((byte)-38), unchecked((byte)-42), unchecked((byte)-53), unchecked((byte)-64), unchecked((byte)-75), unchecked((byte)-16), unchecked((byte)-27), unchecked((byte)-37) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two numbers of the same length and different signs.
         * The first is negative.
         * The second is longer.
         */
        [Test]
        public void testCase14()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-16), unchecked((byte)-27), unchecked((byte)-38), unchecked((byte)-42), unchecked((byte)-53), unchecked((byte)-64), unchecked((byte)-75), unchecked((byte)-16), unchecked((byte)-27), unchecked((byte)-37) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two negative numbers of different length.
         * The first is longer.
         */
        [Test]
        public void testCase15()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Subtract two negative numbers of different length.
         * The second is longer.
         */
        [Test]
        public void testCase16()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 1, 2, 3, 3, unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-24), unchecked((byte)-40), unchecked((byte)-49), unchecked((byte)-58), unchecked((byte)-67), unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-23) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract two positive equal in absolute value numbers.
         */
        [Test]
        public void testCase17()
        {
            byte[] aBytes = { unchecked((byte)-120), 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] bBytes = { unchecked((byte)-120), 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] rBytes = { 0 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Subtract zero from a number.
         * The number is positive.
         */
        [Test]
        public void testCase18()
        {
            byte[] aBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] bBytes = { 0 };
            byte[] rBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            int aSign = 1;
            int bSign = 0;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract a number from zero.
         * The number is negative.
         */
        [Test]
        public void testCase19()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] rBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            int aSign = 0;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract zero from zero.
         */
        [Test]
        public void testCase20()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 0 };
            byte[] rBytes = { 0 };
            int aSign = 0;
            int bSign = 0;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Subtract ZERO from a number.
         * The number is positive.
         */
        [Test]
        public void testCase21()
        {
            byte[] aBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] rBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract a number from ZERO.
         * The number is negative.
         */
        [Test]
        public void testCase22()
        {
            byte[] bBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            byte[] rBytes = { 120, 34, 78, unchecked((byte)-23), unchecked((byte)-111), 45, 127, 23, 45, unchecked((byte)-3) };
            int bSign = -1;
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Subtract ZERO from ZERO.
         */
        [Test]
        public void testCase23()
        {
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = BigInteger.Zero;
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Subtract ONE from ONE.
         */
        [Test]
        public void testCase24()
        {
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.One;
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Subtract two numbers so that borrow is 1.
         */
        [Test]
        public void testCase25()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            byte[] bBytes = { unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128), unchecked((byte)-128) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-128), 127, 127, 127, 127, 127, 127, 127, 127 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber - (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }
    }
}
