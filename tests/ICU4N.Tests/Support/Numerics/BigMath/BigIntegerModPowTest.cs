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
    /// Methods: modPow, modInverse, and gcd 
    /// </summary>
    public class BigIntegerModPowTest : TestFmwk
    {
        /**
        * modPow: non-positive modulus
        */
        [Test]
        public void testModPowException()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] eBytes = { 1, 2, 3, 4, 5 };
            byte[] mBytes = { 1, 2, 3 };
            int aSign = 1;
            int eSign = 1;
            int mSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger exp = new BigInteger(eSign, eBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            try
            {
                BigInteger.ModPow(aNumber, exp, modulus);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "BigInteger: modulus not positive", e.Message);
            }

            try
            {
                BigInteger.ModPow(BigInteger.Zero, BigInteger.Parse("-1"), BigInteger.Parse("10"));
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                // expected
            }
        }

        /**
         * modPow: positive exponent
         */
        [Test]
        public void testModPowPosExp()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75, 48, unchecked((byte)-7) };
            byte[] eBytes = { 27, unchecked((byte)-15), 65, 39 };
            byte[] mBytes = { unchecked((byte)-128), 2, 3, 4, 5 };
            int aSign = 1;
            int eSign = 1;
            int mSign = 1;
            byte[] rBytes = { 113, 100, unchecked((byte)-84), unchecked((byte)-28), unchecked((byte)-85) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger exp = new BigInteger(eSign, eBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            BigInteger result = BigInteger.ModPow(aNumber, exp, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * modPow: negative exponent
         */
        [Test]
        public void testModPowNegExp()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75, 48, unchecked((byte)-7) };
            byte[] eBytes = { 27, unchecked((byte)-15), 65, 39 };
            byte[] mBytes = { unchecked((byte)-128), 2, 3, 4, 5 };
            int aSign = 1;
            int eSign = -1;
            int mSign = 1;
            byte[] rBytes = { 12, 118, 46, 86, 92 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger exp = new BigInteger(eSign, eBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            BigInteger result = BigInteger.ModPow(aNumber, exp, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        [Test]
        [Ignore("ICU4N TODO: This test fails because 2147483648 is out of range for creating an int array (it overflows to negative). This can be fixed by changing the internal array to uint[], but will require a lot of updates to do. It would probably be better to use System.Numerics.BigInteger and Singulink.Numerics.BigDecimal and simply make this into a wrapper class for BigInteger than to go that route.")]
        public void testModPowZeroExp()
        {
            BigInteger exp = BigInteger.Parse("0");
            BigInteger[] @base = new BigInteger[] { BigInteger.Parse("-1"), BigInteger.Parse("0"), BigInteger.Parse("1") };
            BigInteger[] mod = new BigInteger[] { BigInteger.Parse("2"), BigInteger.Parse("10"), BigInteger.Parse("2147483648") };

            for (int i = 0; i < @base.Length; ++i)
            {
                for (int j = 0; j < mod.Length; ++j)
                {
                    assertEquals(@base[i] + " modePow(" + exp + ", " + mod[j]
                            + ") should be " + BigInteger.One, BigInteger.One,
                            BigInteger.ModPow(@base[i], exp, mod[j]));
                }
            }

            mod = new BigInteger[] { BigInteger.Parse("1") };
            for (int i = 0; i < @base.Length; ++i)
            {
                for (int j = 0; j < mod.Length; ++j)
                {
                    assertEquals(@base[i] + " modePow(" + exp + ", " + mod[j]
                            + ") should be " + BigInteger.Zero, BigInteger.Zero,
                            BigInteger.ModPow(@base[i], exp, mod[j]));
                }
            }
        }

        /**
         * modInverse: non-positive modulus
         */
        [Test]
        public void testmodInverseException()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] mBytes = { 1, 2, 3 };
            int aSign = 1;
            int mSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            try
            {
                BigInteger.ModInverse(aNumber, modulus);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "BigInteger: modulus not positive", e.Message);
            }
        }

        /**
         * modInverse: non-invertible number
         */
        [Test]
        public void testmodInverseNonInvertible()
        {
            byte[] aBytes = { unchecked((byte)-15), 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            byte[] mBytes = { unchecked((byte)-12), 1, 0, 0, 0, 23, 44, 55, 66 };
            int aSign = 1;
            int mSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            try
            {
                BigInteger.ModInverse(aNumber, modulus);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "BigInteger not invertible.", e.Message);
            }
        }

        /**
         * modInverse: positive number
         */
        [Test]
        public void testmodInversePos1()
        {
            byte[] aBytes = { 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            byte[] mBytes = { 122, 45, 36, 100, 122, 45 };
            int aSign = 1;
            int mSign = 1;
            byte[] rBytes = { 47, 3, 96, 62, 87, 19 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            BigInteger result = BigInteger.ModInverse(aNumber, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * modInverse: positive number (another case: a < 0)
         */
        [Test]
        public void testmodInversePos2()
        {
            byte[] aBytes = { 15, 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            byte[] mBytes = { 2, 122, 45, 36, 100 };
            int aSign = 1;
            int mSign = 1;
            byte[] rBytes = { 1, unchecked((byte)-93), 40, 127, 73 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            BigInteger result = BigInteger.ModInverse(aNumber, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * modInverse: negative number
         */
        [Test]
        public void testmodInverseNeg1()
        {
            byte[] aBytes = { 15, 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            byte[] mBytes = { 2, 122, 45, 36, 100 };
            int aSign = -1;
            int mSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-41), 4, unchecked((byte)-91), 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger modulus = new BigInteger(mSign, mBytes);
            BigInteger result = BigInteger.ModInverse(aNumber, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * modInverse: negative number (another case: x < 0)
         */
        [Test]
        public void testmodInverseNeg2()
        {
            byte[] aBytes = { unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            byte[] mBytes = { 122, 2, 4, 122, 2, 4 };
            byte[] rBytes = { 85, 47, 127, 4, unchecked((byte)-128), 45 };
            BigInteger aNumber = new BigInteger(aBytes);
            BigInteger modulus = new BigInteger(mBytes);
            BigInteger result = BigInteger.ModInverse(aNumber, modulus);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * gcd: the second number is zero
         */
        [Test]
        public void testGcdSecondZero()
        {
            byte[] aBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            byte[] bBytes = { 0 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * gcd: the first number is zero
         */
        [Test]
        public void testGcdFirstZero()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * gcd: the first number is ZERO
         */
        [Test]
        public void testGcdFirstZERO()
        {
            byte[] bBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            int bSign = 1;
            byte[] rBytes = { 15, 24, 123, 57, unchecked((byte)-15), 24, 123, 57, unchecked((byte)-15), 24, 123, 57 };
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * gcd: both numbers are zeros
         */
        [Test]
        public void testGcdBothZeros()
        {
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.Parse("0");
            BigInteger bNumber = BigInteger.GetInstance(0L);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
            byte[] resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * gcd: the first number is longer
         */
        [Test]
        public void testGcdFirstLonger()
        {
            byte[] aBytes = { unchecked((byte)-15), 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            byte[] bBytes = { unchecked((byte)-12), 1, 0, 0, 0, 23, 44, 55, 66 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 7 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * gcd: the second number is longer
         */
        [Test]
        public void testGcdSecondLonger()
        {
            byte[] aBytes = { unchecked((byte)-12), 1, 0, 0, 0, 23, 44, 55, 66 };
            byte[] bBytes = { unchecked((byte)-15), 24, 123, 56, unchecked((byte)-11), unchecked((byte)-112), unchecked((byte)-34), unchecked((byte)-98), 8, 10, 12, 14, 25, 125, unchecked((byte)-15), 28, unchecked((byte)-127) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 7 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Gcd(aNumber, bNumber);
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
