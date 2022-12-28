using ICU4N.Dev.Test;
using NUnit.Framework;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigInteger
    /// Method: and
    /// </summary>
    public class BigIntegerAndTest : TestFmwk
    {
        /**
         * And for zero and a positive number
         */
        [Test]
        public void testZeroPos()
        {
            byte[] aBytes = new byte[] { 0 };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 0;
            int bSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for zero and a negative number
         */
        [Test]
        public void testZeroNeg()
        {
            byte[] aBytes = new byte[] { 0 };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 0;
            int bSign = -1;
            byte[] rBytes = new byte[] { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for a positive number and zero 
         */
        [Test]
        public void testPosZero()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { 0 };
            int aSign = 1;
            int bSign = 0;
            byte[] rBytes = new byte[] { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for a negative number and zero  
         */
        [Test]
        public void testNegPos()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { 0 };
            int aSign = -1;
            int bSign = 0;
            byte[] rBytes = new byte[] { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for zero and zero
         */
        [Test]
        public void testZeroZero()
        {
            byte[] aBytes = new byte[] { 0 };
            byte[] bBytes = new byte[] { 0 };
            int aSign = 0;
            int bSign = 0;
            byte[] rBytes = new byte[] { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for zero and one
         */
        [Test]
        public void testZeroOne()
        {
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber & (bNumber);
            assertTrue("incorrect equality", result.Equals(BigInteger.Zero));
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * And for one and one
         */
        [Test]
        public void testOneOne()
        {
            BigInteger aNumber = BigInteger.One;
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber & (bNumber);
            assertTrue("equality failed", result.Equals(BigInteger.One));
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for two positive numbers of the same length
         */
        [Test]
        public void testPosPosSameLength()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -128, 56, 100, 4, 4, 17, 37, 16, 1, 64, 1, 10, 3 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for two positive numbers; the first is longer
         */
        [Test]
        public void testPosPosFirstLonger()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -2, -76, 88, 44, 1, 2, 17, 35, 16, 9, 2, 5, 6, 21 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for two positive numbers; the first is shorter
         */
        [Test]
        public void testPosPosFirstShorter()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -2, -76, 88, 44, 1, 2, 17, 35, 16, 9, 2, 5, 6, 21 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for two negative numbers of the same length
         */
        [Test]
        public void testNegNegSameLength()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { -1, 1, 2, 3, 3, 0, 65, -96, -48, -124, -60, 12, -40, -31, 97 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * And for two negative numbers; the first is longer
         */
        [Test]
        public void testNegNegFirstLonger()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { -1, 127, -10, -57, -101, 1, 2, 2, 2, -96, -16, 8, -40, -59, 68, -88, -88, 16, 73 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * And for two negative numbers; the first is shorter
         */
        [Test]
        public void testNegNegFirstShorter()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = -1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { -1, 127, -10, -57, -101, 1, 2, 2, 2, -96, -16, 8, -40, -59, 68, -88, -88, 16, 73 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * And for two numbers of different signs and the same length
         */
        [Test]
        public void testPosNegSameLength()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { 0, -6, -80, 72, 8, 75, 2, -79, 34, 16, -119 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for two numbers of different signs and the same length
         */
        [Test]
        public void testNegPosSameLength()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -2, 125, -60, -104, 1, 10, 6, 2, 32, 56, 2, 4, 4, 21 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for a negative and a positive numbers; the first is longer
         */
        [Test]
        public void testNegPosFirstLonger()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 73, -92, -48, 4, 12, 6, 4, 32, 48, 64, 0, 8, 3 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for a negative and a positive numbers; the first is shorter
         */
        [Test]
        public void testNegPosFirstShorter()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = -1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -128, 9, 56, 100, 0, 0, 1, 1, 90, 1, -32, 0, 10, -126, 21, 82, -31, -95 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for a positive and a negative numbers; the first is longer
         */
        [Test]
        public void testPosNegFirstLonger()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { 0, -128, 9, 56, 100, 0, 0, 1, 1, 90, 1, -32, 0, 10, -126, 21, 82, -31, -95 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * And for a positive and a negative numbers; the first is shorter
         */
        [Test]
        public void testPosNegFirstShorter()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = new byte[] { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = 1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { 73, -92, -48, 4, 12, 6, 4, 32, 48, 64, 0, 8, 3 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Test for a special case
         */
        [Test]
        public void testSpecialCase1()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            byte[] bBytes = new byte[] { 5, unchecked((byte)-4), unchecked((byte)-3), unchecked((byte)-2) };
            int aSign = -1;
            int bSign = -1;
            sbyte[] rBytes = new sbyte[] { -1, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Test for a special case
         */
        [Test]
        public void testSpecialCase2()
        {
            byte[] aBytes = new byte[] { unchecked((byte)-51) };
            byte[] bBytes = new byte[] { unchecked((byte)-52), unchecked((byte)-51), unchecked((byte)-50), unchecked((byte)-49), unchecked((byte)-48) };
            int aSign = -1;
            int bSign = 1;
            sbyte[] rBytes = new sbyte[] { 0, -52, -51, -50, -49, 16 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber & (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }
    }
}
