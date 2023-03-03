using ICU4N.Dev.Test;
using NUnit.Framework;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigInteger
    /// Method: or
    /// </summary>
    public class BigIntegerOrTest : TestFmwk
    {
        /**
         * Or for zero and a positive number
         */
        [Test]
        public void testZeroPos()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 0;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for zero and a negative number
         */
        [Test]
        public void testZeroNeg()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 0;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 1, 2, 3, 3, unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-24), unchecked((byte)-40), unchecked((byte)-49), unchecked((byte)-58), unchecked((byte)-67), unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-23) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for a positive number and zero 
         */
        [Test]
        public void testPosZero()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { 0 };
            int aSign = 1;
            int bSign = 0;
            byte[] rBytes = { 0, unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for a negative number and zero  
         */
        [Test]
        public void testNegPos()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { 0 };
            int aSign = -1;
            int bSign = 0;
            byte[] rBytes = { unchecked((byte)-1), 1, 2, 3, 3, unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-24), unchecked((byte)-40), unchecked((byte)-49), unchecked((byte)-58), unchecked((byte)-67), unchecked((byte)-6), unchecked((byte)-15), unchecked((byte)-23) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for zero and zero
         */
        [Test]
        public void testZeroZero()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 0 };
            int aSign = 0;
            int bSign = 0;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Or for zero and one
         */
        [Test]
        public void testZeroOne()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 1 };
            int aSign = 0;
            int bSign = 1;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for one and one
         */
        [Test]
        public void testOneOne()
        {
            byte[] aBytes = { 1 };
            byte[] bBytes = { 1 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for two positive numbers of the same length
         */
        [Test]
        public void testPosPosSameLength()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), unchecked((byte)-1), unchecked((byte)-66), 95, 47, 123, 59, unchecked((byte)-13), 39, 30, unchecked((byte)-97) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for two positive numbers; the first is longer
         */
        [Test]
        public void testPosPosFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-3), unchecked((byte)-3), 95, 15, unchecked((byte)-9), 39, 58, unchecked((byte)-69), 87, 87, unchecked((byte)-17), unchecked((byte)-73) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for two positive numbers; the first is shorter
         */
        [Test]
        public void testPosPosFirstShorter()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-3), unchecked((byte)-3), 95, 15, unchecked((byte)-9), 39, 58, unchecked((byte)-69), 87, 87, unchecked((byte)-17), unchecked((byte)-73) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Or for two negative numbers of the same length
         */
        [Test]
        public void testNegNegSameLength()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-57), unchecked((byte)-101), unchecked((byte)-5), unchecked((byte)-5), unchecked((byte)-18), unchecked((byte)-38), unchecked((byte)-17), unchecked((byte)-2), unchecked((byte)-65), unchecked((byte)-2), unchecked((byte)-11), unchecked((byte)-3) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for two negative numbers; the first is longer
         */
        [Test]
        public void testNegNegFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 1, 75, unchecked((byte)-89), unchecked((byte)-45), unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-18), unchecked((byte)-36), unchecked((byte)-17), unchecked((byte)-10), unchecked((byte)-3), unchecked((byte)-6), unchecked((byte)-7), unchecked((byte)-21) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for two negative numbers; the first is shorter
         */
        [Test]
        public void testNegNegFirstShorter()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 1, 75, unchecked((byte)-89), unchecked((byte)-45), unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-18), unchecked((byte)-36), unchecked((byte)-17), unchecked((byte)-10), unchecked((byte)-3), unchecked((byte)-6), unchecked((byte)-7), unchecked((byte)-21) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for two numbers of different signs and the same length
         */
        [Test]
        public void testPosNegSameLength()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 1, unchecked((byte)-126), 59, 103, unchecked((byte)-2), unchecked((byte)-11), unchecked((byte)-7), unchecked((byte)-3), unchecked((byte)-33), unchecked((byte)-57), unchecked((byte)-3), unchecked((byte)-5), unchecked((byte)-5), unchecked((byte)-21) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for two numbers of different signs and the same length
         */
        [Test]
        public void testNegPosSameLength()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-1), 5, 79, unchecked((byte)-73), unchecked((byte)-9), unchecked((byte)-76), unchecked((byte)-3), 78, unchecked((byte)-35), unchecked((byte)-17), 119 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for a negative and a positive numbers; the first is longer
         */
        [Test]
        public void testNegPosFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-10), unchecked((byte)-57), unchecked((byte)-101), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), unchecked((byte)-2), unchecked((byte)-91), unchecked((byte)-2), 31, unchecked((byte)-1), unchecked((byte)-11), 125, unchecked((byte)-22), unchecked((byte)-83), 30, 95 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for two negative numbers; the first is shorter
         */
        [Test]
        public void testNegPosFirstShorter()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-74), 91, 47, unchecked((byte)-5), unchecked((byte)-13), unchecked((byte)-7), unchecked((byte)-5), unchecked((byte)-33), unchecked((byte)-49), unchecked((byte)-65), unchecked((byte)-1), unchecked((byte)-9), unchecked((byte)-3) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for a positive and a negative numbers; the first is longer
         */
        [Test]
        public void testPosNegFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-74), 91, 47, unchecked((byte)-5), unchecked((byte)-13), unchecked((byte)-7), unchecked((byte)-5), unchecked((byte)-33), unchecked((byte)-49), unchecked((byte)-65), unchecked((byte)-1), unchecked((byte)-9), unchecked((byte)-3) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Or for a positive and a negative number; the first is shorter
         */
        [Test]
        public void testPosNegFirstShorter()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-10), unchecked((byte)-57), unchecked((byte)-101), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), unchecked((byte)-2), unchecked((byte)-91), unchecked((byte)-2), 31, unchecked((byte)-1), unchecked((byte)-11), 125, unchecked((byte)-22), unchecked((byte)-83), 30, 95 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber | (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        [Test]
        public void testRegression()
        {
            // Regression test for HARMONY-1996
            BigInteger x = BigInteger.Parse("-1023");
            BigInteger r1 = x & ((BigInteger.Not(BigInteger.Zero)) << (32));
            BigInteger r3 = x & (BigInteger.Not((BigInteger.Not(BigInteger.Zero)) << (32)));
            BigInteger result = r1 | (r3);
            assertEquals("incorrect result", x, result);
        }
    }
}
