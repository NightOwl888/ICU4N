using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Globalization;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:   java.math.BigInteger
    /// Methods: bitLength, shiftLeft, shiftRight,
    /// clearBit, flipBit, setBit, testBit
    /// </summary>
    public class BigIntegerOperateBitsTest : TestFmwk
    {
        /**
         * bitCount() of zero.
         */
        [Test]
        public void testBitCountZero()
        {
            BigInteger aNumber = BigInteger.Parse("0");
            assertEquals("incorrect result", 0, aNumber.BitCount);
        }

        /**
         * bitCount() of a negative number.
         */
        [Test]
        public void testBitCountNeg()
        {
            BigInteger aNumber = BigInteger.Parse("-12378634756382937873487638746283767238657872368748726875");
            assertEquals("incorrect result", 87, aNumber.BitCount);
        }

        /**
         * bitCount() of a negative number.
         */
        [Test]
        public void testBitCountPos()
        {
            BigInteger aNumber = BigInteger.Parse("12378634756343564757582937873487638746283767238657872368748726875");
            assertEquals("incorrect result", 107, aNumber.BitCount);
        }

        /**
         * bitLength() of zero.
         */
        [Test]
        public void testBitLengthZero()
        {
            BigInteger aNumber = BigInteger.Parse("0");
            assertEquals("incorrect result", 0, aNumber.BitLength);
        }

        /**
         * bitLength() of a positive number.
         */
        [Test]
        public void testBitLengthPositive1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 108, aNumber.BitLength);
        }

        /**
         * bitLength() of a positive number with the leftmost bit set
         */
        [Test]
        public void testBitLengthPositive2()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 96, aNumber.BitLength);
        }

        /**
         * bitLength() of a positive number which is a power of 2
         */
        [Test]
        public void testBitLengthPositive3()
        {
            byte[] aBytes = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 81, aNumber.BitLength);
        }

        /**
         * bitLength() of a negative number.
         */
        [Test]
        public void testBitLengthNegative1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            int aSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 108, aNumber.BitLength);
        }

        /**
         * bitLength() of a negative number with the leftmost bit set
         */
        [Test]
        public void testBitLengthNegative2()
        {
            byte[] aBytes = { unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 96, aNumber.BitLength);
        }

        /**
         * bitLength() of a negative number which is a power of 2
         */
        [Test]
        public void testBitLengthNegative3()
        {
            byte[] aBytes = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertEquals("incorrect result", 80, aNumber.BitLength);
        }

        /**
         * clearBit(int n) of a negative n
         */
        [Test]
        public void testClearBitException()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            try
            {
                BigInteger.ClearBit(aNumber, number);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Negative bit address", e.Message);
            }
        }

        /**
         * clearBit(int n) outside zero
         */
        [Test]
        public void testClearBitZero()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 0;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * clearBit(int n) outside zero
         */
        [Test]
        public void testClearBitZeroOutside1()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 95;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * clearBit(int n) inside a negative number
         */
        [Test]
        public void testClearBitNegativeInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 15;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, 92, unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * clearBit(int n) inside a negative number
         */
        [Test]
        public void testClearBitNegativeInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 44;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-62), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * clearBit(2) in the negative number with all ones in bit representation
         */
        [Test]
        public void testClearBitNegativeInside3()
        {
            String @as = "-18446744073709551615";
            int number = 2;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            assertEquals("incorrect result", @as, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * clearBit(0) in the negative number of length 1
         * with all ones in bit representation.
         * the resulting number's length is 2.
         */
        [Test]
        public void testClearBitNegativeInside4()
        {
            String @as = "-4294967295";
            String res = "-4294967296";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * clearBit(0) in the negative number of length 2
         * with all ones in bit representation.
         * the resulting number's length is 3.
         */
        [Test]
        public void testClearBitNegativeInside5()
        {
            String @as = "-18446744073709551615";
            String res = "-18446744073709551616";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * clearBit(int n) outside a negative number
         */
        [Test]
        public void testClearBitNegativeOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 150;
            byte[] rBytes = { unchecked((byte)-65), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * clearBit(int n) outside a negative number
         */
        [Test]
        public void testClearBitNegativeOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 165;
            byte[] rBytes = { unchecked((byte)-33), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * clearBit(int n) inside a positive number
         */
        [Test]
        public void testClearBitPositiveInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 20;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-31), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) inside a positive number
         */
        [Test]
        public void testClearBitPositiveInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 17;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) inside a positive number
         */
        [Test]
        public void testClearBitPositiveInside3()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 13, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) inside a positive number
         */
        [Test]
        public void testClearBitPositiveInside4()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 50;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) inside a positive number
         */
        [Test]
        public void testClearBitPositiveInside5()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 63;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), 52, 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) outside a positive number
         */
        [Test]
        public void testClearBitPositiveOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 150;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) outside a positive number
         */
        [Test]
        public void testClearBitPositiveOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 191;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * clearBit(int n) the leftmost bit in a negative number
         */
        [Test]
        public void testClearBitTopNegative()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 63;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.ClearBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) of a negative n
         */
        [Test]
        public void testFlipBitException()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            try
            {
                BigInteger.FlipBit(aNumber, number);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Negative bit address", e.Message);
            }
        }

        /**
         * flipBit(int n) zero
         */
        [Test]
        public void testFlipBitZero()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 0;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) outside zero
         */
        [Test]
        public void testFlipBitZeroOutside1()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 62;
            byte[] rBytes = { 64, 0, 0, 0, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) outside zero
         */
        [Test]
        public void testFlipBitZeroOutside2()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 63;
            byte[] rBytes = { 0, unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) the leftmost bit in a negative number
         */
        [Test]
        public void testFlipBitLeftmostNegative()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 48;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-57), unchecked((byte)-101), 14, unchecked((byte)-36), unchecked((byte)-26), 49 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) the leftmost bit in a positive number
         */
        [Test]
        public void testFlipBitLeftmostPositive()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 48;
            byte[] rBytes = { 0, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) inside a negative number
         */
        [Test]
        public void testFlipBitNegativeInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 15;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, 92, unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) inside a negative number
         */
        [Test]
        public void testFlipBitNegativeInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 45;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-14), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) inside a negative number with all ones in bit representation 
         */
        [Test]
        public void testFlipBitNegativeInside3()
        {
            String @as = "-18446744073709551615";
            String res = "-18446744073709551611";
            int number = 2;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * flipBit(0) in the negative number of length 1
         * with all ones in bit representation.
         * the resulting number's length is 2.
         */
        [Test]
        public void testFlipBitNegativeInside4()
        {
            String @as = "-4294967295";
            String res = "-4294967296";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * flipBit(0) in the negative number of length 2
         * with all ones in bit representation.
         * the resulting number's length is 3.
         */
        [Test]
        public void testFlipBitNegativeInside5()
        {
            String @as = "-18446744073709551615";
            String res = "-18446744073709551616";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * flipBit(int n) outside a negative number
         */
        [Test]
        public void testFlipBitNegativeOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 150;
            byte[] rBytes = { unchecked((byte)-65), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) outside a negative number
         */
        [Test]
        public void testFlipBitNegativeOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 191;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * flipBit(int n) inside a positive number
         */
        [Test]
        public void testFlipBitPositiveInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 15;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), unchecked((byte)-93), 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) inside a positive number
         */
        [Test]
        public void testFlipBitPositiveInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 13, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) outside a positive number
         */
        [Test]
        public void testFlipBitPositiveOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 150;
            byte[] rBytes = { 64, 0, 0, 0, 0, 0, 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * flipBit(int n) outside a positive number
         */
        [Test]
        public void testFlipBitPositiveOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 191;
            byte[] rBytes = { 0, unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.FlipBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) of a negative n
         */
        [Test]
        public void testSetBitException()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            try
            {
                BigInteger.SetBit(aNumber, number);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Negative bit address", e.Message);
            }
        }

        /**
         * setBit(int n) outside zero
         */
        [Test]
        public void testSetBitZero()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 0;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) outside zero
         */
        [Test]
        public void testSetBitZeroOutside1()
        {
            byte[] aBytes = { 0 };
            int aSign = 0;
            int number = 95;
            byte[] rBytes = { 0, unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) inside a positive number
         */
        [Test]
        public void testSetBitPositiveInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 20;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) inside a positive number
         */
        [Test]
        public void testSetBitPositiveInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 17;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-13), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) inside a positive number
         */
        [Test]
        public void testSetBitPositiveInside3()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) inside a positive number
         */
        [Test]
        public void testSetBitPositiveInside4()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 50;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 93, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) outside a positive number
         */
        [Test]
        public void testSetBitPositiveOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 150;
            byte[] rBytes = { 64, 0, 0, 0, 0, 0, 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) outside a positive number
         */
        [Test]
        public void testSetBitPositiveOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 223;
            byte[] rBytes = { 0, unchecked((byte)-128), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) the leftmost bit in a positive number
         */
        [Test]
        public void testSetBitTopPositive()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 63;
            byte[] rBytes = { 0, unchecked((byte)-128), 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * setBit(int n) the leftmost bit in a negative number
         */
        [Test]
        public void testSetBitLeftmostNegative()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 48;
            byte[] rBytes = { unchecked((byte)-1), 127, unchecked((byte)-57), unchecked((byte)-101), 14, unchecked((byte)-36), unchecked((byte)-26), 49 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * setBit(int n) inside a negative number
         */
        [Test]
        public void testSetBitNegativeInside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 15;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * setBit(int n) inside a negative number
         */
        [Test]
        public void testSetBitNegativeInside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 44;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * setBit(int n) inside a negative number with all ones in bit representation
         */
        [Test]
        public void testSetBitNegativeInside3()
        {
            String @as = "-18446744073709551615";
            String res = "-18446744073709551611";
            int number = 2;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            assertEquals("incorrect result", res, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * setBit(0) in the negative number of length 1
         * with all ones in bit representation.
         * the resulting number's length is 2.
         */
        [Test]
        public void testSetBitNegativeInside4()
        {
            String @as = "-4294967295";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            assertEquals("incorrect result", @as, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * setBit(0) in the negative number of length 2
         * with all ones in bit representation.
         * the resulting number's length is 3.
         */
        [Test]
        public void testSetBitNegativeInside5()
        {
            String @as = "-18446744073709551615";
            int number = 0;
            BigInteger aNumber = BigInteger.Parse(@as);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            assertEquals("incorrect result", @as, result.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * setBit(int n) outside a negative number
         */
        [Test]
        public void testSetBitNegativeOutside1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 150;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * setBit(int n) outside a negative number
         */
        [Test]
        public void testSetBitNegativeOutside2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 191;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92), unchecked((byte)-4), 14, unchecked((byte)-36), unchecked((byte)-26) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = BigInteger.SetBit(aNumber, number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * setBit: check the case when the number of bit to be set can be
         * represented as n * 32 + 31, where n is an arbitrary integer.
         * Here 191 = 5 * 32 + 31 
         */
        [Test]
        public void testSetBitBug1331()
        {
            BigInteger result = BigInteger.SetBit(BigInteger.GetInstance(0L), 191);
            assertEquals("incorrect value", "3138550867693340381917894711603833208051177722232017256448", result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftLeft(int n), n = 0
         */
        [Test]
        public void testShiftLeft1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 0;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber << (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftLeft(int n), n < 0
         */
        [Test]
        public void testShiftLeft2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -27;
            byte[] rBytes = { 48, 7, 12, unchecked((byte)-97), unchecked((byte)-42), unchecked((byte)-117), 37, unchecked((byte)-85), 96 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber << (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftLeft(int n) a positive number, n > 0
         */
        [Test]
        public void testShiftLeft3()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 27;
            byte[] rBytes = { 12, 1, unchecked((byte)-61), 39, unchecked((byte)-11), unchecked((byte)-94), unchecked((byte)-55), 106, unchecked((byte)-40), 31, unchecked((byte)-119), 24, unchecked((byte)-48), 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber << (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftLeft(int n) a positive number, n > 0
         */
        [Test]
        public void testShiftLeft4()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            byte[] rBytes = { 48, 7, 12, unchecked((byte)-97), unchecked((byte)-42), unchecked((byte)-117), 37, unchecked((byte)-85), 96, 126, 36, 99, 64, 0, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber << (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftLeft(int n) a negative number, n > 0
         */
        [Test]
        public void testShiftLeft5()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 45;
            byte[] rBytes = { unchecked((byte)-49), unchecked((byte)-8), unchecked((byte)-13), 96, 41, 116, unchecked((byte)-38), 84, unchecked((byte)-97), unchecked((byte)-127), unchecked((byte)-37), unchecked((byte)-100), unchecked((byte)-64), 0, 0, 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber << (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * shiftRight(int n), n = 0
         */
        [Test]
        public void testShiftRight1()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 0;
            byte[] rBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftRight(int n), n < 0
         */
        [Test]
        public void testShiftRight2()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -27;
            byte[] rBytes = { 12, 1, unchecked((byte)-61), 39, unchecked((byte)-11), unchecked((byte)-94), unchecked((byte)-55), 106, unchecked((byte)-40), 31, unchecked((byte)-119), 24, unchecked((byte)-48), 0, 0, 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftRight(int n), 0 < n < 32
         */
        [Test]
        public void testShiftRight3()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 27;
            byte[] rBytes = { 48, 7, 12, unchecked((byte)-97), unchecked((byte)-42), unchecked((byte)-117), 37, unchecked((byte)-85), 96 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftRight(int n), n > 32
         */
        [Test]
        public void testShiftRight4()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            byte[] rBytes = { 12, 1, unchecked((byte)-61), 39, unchecked((byte)-11), unchecked((byte)-94), unchecked((byte)-55) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * shiftRight(int n), n is greater than bitLength()
         */
        [Test]
        public void testShiftRight5()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 300;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * shiftRight a negative number;
         * shift distance is multiple of 32;
         * shifted bits are NOT zeroes. 
         */
        [Test]
        public void testShiftRightNegNonZeroesMul32()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 1, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            int number = 64;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-92) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * shiftRight a negative number;
         * shift distance is NOT multiple of 32;
         * shifted bits are NOT zeroes. 
         */
        [Test]
        public void testShiftRightNegNonZeroes()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            int number = 68;
            byte[] rBytes = { unchecked((byte)-25), unchecked((byte)-4), 121, unchecked((byte)-80), 20, unchecked((byte)-70), 109, 42 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * shiftRight a negative number;
         * shift distance is NOT multiple of 32;
         * shifted bits are zeroes. 
         */
        [Test]
        public void testShiftRightNegZeroes()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            int number = 68;
            byte[] rBytes = { unchecked((byte)-25), unchecked((byte)-4), 121, unchecked((byte)-80), 20, unchecked((byte)-70), 109, 48 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * shiftRight a negative number;
         * shift distance is multiple of 32;
         * shifted bits are zeroes. 
         */
        [Test]
        public void testShiftRightNegZeroesMul32()
        {
            byte[] aBytes = { 1, unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 0, 0, 0, 0, 0, 0, 0, 0 };
            int aSign = -1;
            int number = 64;
            byte[] rBytes = { unchecked((byte)-2), 127, unchecked((byte)-57), unchecked((byte)-101), 1, 75, unchecked((byte)-90), unchecked((byte)-46), unchecked((byte)-91) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger result = aNumber >> (number);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * testBit(int n) of a negative n
         */
        [Test]
        public void testTestBitException()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = -7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            try
            {
                BigInteger.TestBit(aNumber, number);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Negative bit address", e.Message);
            }
        }

        /**
         * testBit(int n) of a positive number
         */
        [Test]
        public void testTestBitPositive1()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", !BigInteger.TestBit(aNumber, number));
        }

        /**
         * testBit(int n) of a positive number
         */
        [Test]
        public void testTestBitPositive2()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 45;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", BigInteger.TestBit(aNumber, number));
        }

        /**
         * testBit(int n) of a positive number, n > bitLength()
         */
        [Test]
        public void testTestBitPositive3()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = 1;
            int number = 300;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", !BigInteger.TestBit(aNumber, number));
        }

        /**
         * testBit(int n) of a negative number
         */
        [Test]
        public void testTestBitNegative1()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 7;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", BigInteger.TestBit(aNumber, number));
        }

        /**
         * testBit(int n) of a positive n
         */
        [Test]
        public void testTestBitNegative2()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 45;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", !BigInteger.TestBit(aNumber, number));
        }

        /**
         * testBit(int n) of a positive n, n > bitLength()
         */
        [Test]
        public void testTestBitNegative3()
        {
            byte[] aBytes = { unchecked((byte)-1), unchecked((byte)-128), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26 };
            int aSign = -1;
            int number = 300;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            assertTrue("incorrect value", BigInteger.TestBit(aNumber, number));
        }
    }
}
