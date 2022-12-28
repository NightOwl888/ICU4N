using ICU4N.Dev.Test;
using NUnit.Framework;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigInteger
    /// Methods: and, andNot
    /// </summary>
    public class BigIntegerNotTest : TestFmwk
    {
        /**
         * andNot for two positive numbers; the first is longer
         */
        [Test]
        public void testAndNotPosPosFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-128), 9, 56, 100, 0, 0, 1, 1, 90, 1, unchecked((byte)-32), 0, 10, unchecked((byte)-126), 21, 82, unchecked((byte)-31), unchecked((byte)-96) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.AndNot(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * andNot for two positive numbers; the first is shorter
         */
        [Test]
        public void testAndNotPosPosFirstShorter()
        {
            byte[] aBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            byte[] bBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 73, unchecked((byte)-92), unchecked((byte)-48), 4, 12, 6, 4, 32, 48, 64, 0, 8, 2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.AndNot(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * andNot for two negative numbers; the first is longer
         */
        [Test]
        public void testAndNotNegNegFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 73, unchecked((byte)-92), unchecked((byte)-48), 4, 12, 6, 4, 32, 48, 64, 0, 8, 2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.AndNot(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * andNot for a negative and a positive numbers; the first is longer
         */
        [Test]
        public void testNegPosFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-128), 9, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117), 23, 87, unchecked((byte)-25), unchecked((byte)-75) };
            byte[] bBytes = { unchecked((byte)-2), unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-4), 5, 14, 23, 39, 48, 57, 66, 5, 14, 23 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-10), unchecked((byte)-57), unchecked((byte)-101), 1, 2, 2, 2, unchecked((byte)-96), unchecked((byte)-16), 8, unchecked((byte)-40), unchecked((byte)-59), 68, unchecked((byte)-88), unchecked((byte)-88), 16, 72 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.AndNot(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Not for ZERO 
         */
        [Test]
        public void testNotZero()
        {
            byte[] rBytes = { unchecked((byte)-1) };
            BigInteger aNumber = BigInteger.Zero;
            BigInteger result = BigInteger.Not(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Not for ONE
         */
        [Test]
        public void testNotOne()
        {
            byte[] rBytes = { unchecked((byte)-2) };
            BigInteger aNumber = BigInteger.One;
            BigInteger result = BigInteger.Not(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Not for a positive number
         */
        [Test]
        public void testNotPos()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            int aSign = 1;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-27), 116 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Not(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Not for a negative number
         */
        [Test]
        public void testNotNeg()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-117) };
            int aSign = -1;
            byte[] rBytes = { 0, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, unchecked((byte)-118) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Not(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Not for a negative number
         */
        [Test]
        public void testNotSpecialCase()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = 1;
            byte[] rBytes = { unchecked((byte)-1), 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Not(aNumber);
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
