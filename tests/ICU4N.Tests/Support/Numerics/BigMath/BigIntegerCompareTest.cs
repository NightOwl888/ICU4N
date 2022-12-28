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
    /// Methods: abs, compareTo, equals, max, min, negate, signum
    /// </summary>
    public class BigIntegerCompareTest : TestFmwk
    {
        /**
         * abs() for a positive number
         */
        [Test]
        public void testAbsPositive()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            int aSign = 1;
            byte[] rBytes = { 1, 2, 3, 4, 5, 6, 7 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Abs(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * abs() for a negative number
         */
        [Test]
        public void testAbsNegative()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            int aSign = -1;
            byte[] rBytes = { 1, 2, 3, 4, 5, 6, 7 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.Abs(aNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * compareTo(BigInteger a).
         * Compare two positive numbers.
         * The first is greater.
         */
        [Test]
        public void testCompareToPosPos1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two positive numbers.
         * The first is less.
         */
        [Test]
        public void testCompareToPosPos2()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", -1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two equal positive numbers.
         */
        [Test]
        public void testCompareToEqualPos()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 0, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two negative numbers.
         * The first is greater in absolute value.
         */
        [Test]
        public void testCompareToNegNeg1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", -1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two negative numbers.
         * The first is less  in absolute value.
         */
        [Test]
        public void testCompareNegNeg2()
        {
            byte[] aBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two equal negative numbers.
         */
        [Test]
        public void testCompareToEqualNeg()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 0, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two numbers of different signs.
         * The first is positive.
         */
        [Test]
        public void testCompareToDiffSigns1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = 1;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare two numbers of different signs.
         * The first is negative.
         */
        [Test]
        public void testCompareToDiffSigns2()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 10, 20, 30, 40, 50, 60, 70, 10, 20, 30 };
            int aSign = -1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", -1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare a positive number to ZERO.
         */
        [Test]
        public void testCompareToPosZero()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            assertEquals("incorrect result", 1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare ZERO to a positive number.
         */
        [Test]
        public void testCompareToZeroPos()
        {
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int bSign = 1;
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", -1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare a negative number to ZERO.
         */
        [Test]
        public void testCompareToNegZero()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            assertEquals("incorrect result", -1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare ZERO to a negative number.
         */
        [Test]
        public void testCompareToZeroNeg()
        {
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int bSign = -1;
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            assertEquals("incorrect result", 1, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * compareTo(BigInteger a).
         * Compare ZERO to ZERO.
         */
        [Test]
        public void testCompareToZeroZero()
        {
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = BigInteger.Zero;
            assertEquals("incorrect result", 0, BigInteger.Compare(aNumber, bNumber));
        }

        /**
         * equals(Object obj).
         * obj is not a BigInteger
         */
        [Test]
        public void testEqualsObject()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            Object obj = new Object();
            assertFalse("incorrect result", aNumber.Equals(obj));
        }

        /**
         * equals(null).
         */
        [Test]
        public void testEqualsNull()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertFalse("incorrect result", aNumber.Equals(null));
        }

        /**
         * equals(Object obj).
         * obj is a BigInteger.
         * numbers are equal.
         */
        [Test]
        public void testEqualsBigIntegerTrue()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            Object bNumber = new BigInteger(bSign, bBytes);
            assertTrue("incorrect result", aNumber.Equals(bNumber));
        }

        /**
         * equals(Object obj).
         * obj is a BigInteger.
         * numbers are not equal.
         */
        [Test]
        public void testEqualsBigIntegerFalse()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            Object bNumber = new BigInteger(bSign, bBytes);
            assertFalse("incorrect result", aNumber.Equals(bNumber));
        }

        /**
         * max(BigInteger val).
         * the first is greater.
         */
        [Test]
        public void testMaxGreater()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 12, 56, 100, -2, -76, 89, 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Max(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 1);
        }

        /**
         * max(BigInteger val).
         * the first is less.
         */
        [Test]
        public void testMaxLess()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 12, 56, 100, -2, -76, 89, 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Max(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 1);
        }

        /**
         * max(BigInteger val).
         * numbers are equal.
         */
        [Test]
        public void testMaxEqual()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Max(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * max(BigInteger val).
         * max of negative and ZERO.
         */
        [Test]
        public void testMaxNegZero()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            BigInteger result = BigInteger.Max(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 0);
        }

        /**
         * min(BigInteger val).
         * the first is greater.
         */
        [Test]
        public void testMinGreater()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Min(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * min(BigInteger val).
         * the first is less.
         */
        [Test]
        public void testMinLess()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Min(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * min(BigInteger val).
         * numbers are equal.
         */
        [Test]
        public void testMinEqual()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] bBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            int bSign = 1;
            sbyte[] rBytes = { 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Min(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 1);
        }

        /**
         * max(BigInteger val).
         * min of positive and ZERO.
         */
        [Test]
        public void testMinPosZero()
        {
            byte[] aBytes = { 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            BigInteger result = BigInteger.Min(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 0);
        }

        /**
         * negate() a positive number.
         */
        [Test]
        public void testNegatePositive()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            sbyte[] rBytes = { -13, -57, -101, 1, 75, -90, -46, -92, -4, 14, -36, -27, -4, -91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = -aNumber;
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == -1);
        }

        /**
         * negate() a negative number.
         */
        [Test]
        public void testNegateNegative()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            sbyte[] rBytes = { 12, 56, 100, -2, -76, 89, 45, 91, 3, -15, 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = -aNumber;
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertTrue("incorrect sign", result.Sign == 1);
        }

        /**
         * negate() ZERO.
         */
        [Test]
        public void testNegateZero()
        {
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.Zero;
            BigInteger result = -aNumber;
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * signum() of a positive number.
         */
        [Test]
        public void testSignumPositive()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * signum() of a negative number.
         */
        [Test]
        public void testSignumNegative()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * signum() of ZERO.
         */
        [Test]
        public void testSignumZero()
        {
            BigInteger aNumber = BigInteger.Zero;
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }
    }
}
