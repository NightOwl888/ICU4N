using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:   java.math.BigInteger
    /// Methods: divide, remainder, mod, and divideAndRemainder
    /// </summary>
    public class BigIntegerDivideTest : TestFmwk
    {
        /**
         * Divide by zero
         */
        [Test]
        public void testCase1()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 0 };
            int aSign = 1;
            int bSign = 0;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            try
            {
                var _ = aNumber / bNumber;
                fail("ArithmeticException has not been caught");
            }
            catch (DivideByZeroException e)
            {
                assertEquals("Improper exception message", "BigInteger divide by zero", e.Message);
            }
        }

        /**
         * Divide by ZERO
         */
        [Test]
        public void testCase2()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            int aSign = 1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.Zero;
            try
            {
                var _ = aNumber / (bNumber);
                fail("ArithmeticException has not been caught");
            }
            catch (DivideByZeroException e)
            {
                assertEquals("Improper exception message", "BigInteger divide by zero", e.Message);
            }
        }

        /**
         * Divide two equal positive numbers
         */
        [Test]
        public void testCase3()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            byte[] bBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 1 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide two equal in absolute value numbers of different signs.
         */
        [Test]
        public void testCase4()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            byte[] bBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-1) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Divide two numbers of different length and different signs.
         * The second is longer.
         */
        [Test]
        public void testCase5()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            byte[] bBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 1, 2, 3, 4, 5 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Divide two positive numbers of the same length.
         * The second is greater.
         */
        [Test]
        public void testCase6()
        {
            byte[] aBytes = { 1, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            byte[] bBytes = { 15, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Divide two positive numbers.
         */
        [Test]
        public void testCase7()
        {
            byte[] aBytes = { 1, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 5, 6, 7, 8, 9 };
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 23, 115, 11, 78, 35, unchecked((byte)-11) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide a positive number by a negative one.
         */
        [Test]
        public void testCase8()
        {
            byte[] aBytes = { 1, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 5, 6, 7, 8, 9 };
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-24), unchecked((byte)-116), unchecked((byte)-12), unchecked((byte)-79), unchecked((byte)-36), 11 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Divide a negative number by a positive one.
         */
        [Test]
        public void testCase9()
        {
            byte[] aBytes = { 1, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 5, 6, 7, 8, 9 };
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-24), unchecked((byte)-116), unchecked((byte)-12), unchecked((byte)-79), unchecked((byte)-36), 11 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Divide two negative numbers.
         */
        [Test]
        public void testCase10()
        {
            byte[] aBytes = { 1, 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 5, 6, 7, 8, 9 };
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 23, 115, 11, 78, 35, unchecked((byte)-11) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide zero by a negative number.
         */
        [Test]
        public void testCase11()
        {
            byte[] aBytes = { 0 };
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = 0;
            int bSign = -1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Divide ZERO by a negative number.
         */
        [Test]
        public void testCase12()
        {
            byte[] bBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int bSign = -1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.Zero;
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Divide a positive number by ONE.
         */
        [Test]
        public void testCase13()
        {
            byte[] aBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            int aSign = 1;
            byte[] rBytes = { 15, 48, unchecked((byte)-29), 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide ONE by ONE.
         */
        [Test]
        public void testCase14()
        {
            byte[] rBytes = { 1 };
            BigInteger aNumber = BigInteger.One;
            BigInteger bNumber = BigInteger.One;
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Verifies the case when borrow != 0 in the private divide method.
         */
        [Test]
        public void testDivisionKnuth1()
        {
            byte[] aBytes = { unchecked((byte)-7), unchecked((byte)-6), unchecked((byte)-5), unchecked((byte)-4), unchecked((byte)-3), unchecked((byte)-2), unchecked((byte)-1), 0, 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { unchecked((byte)-3), unchecked((byte)-3), unchecked((byte)-3), unchecked((byte)-3) };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0, unchecked((byte)-5), unchecked((byte)-12), unchecked((byte)-33), unchecked((byte)-96), unchecked((byte)-36), unchecked((byte)-105), unchecked((byte)-56), 92, 15, 48, unchecked((byte)-109) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Verifies the case when the divisor is already normalized.
         */
        [Test]
        public void testDivisionKnuthIsNormalized()
        {
            byte[] aBytes = { unchecked((byte)-9), unchecked((byte)-8), unchecked((byte)-7), unchecked((byte)-6), unchecked((byte)-5), unchecked((byte)-4), unchecked((byte)-3), unchecked((byte)-2), unchecked((byte)-1), 0, 1, 2, 3, 4, 5 };
            byte[] bBytes = { unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 0, unchecked((byte)-9), unchecked((byte)-8), unchecked((byte)-7), unchecked((byte)-6), unchecked((byte)-5), unchecked((byte)-4), unchecked((byte)-3) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Verifies the case when the first digits of the dividend
         * and divisor equal.
         */
        [Test]
        public void testDivisionKnuthFirstDigitsEqual()
        {
            byte[] aBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-1), unchecked((byte)-5), unchecked((byte)-4), unchecked((byte)-3), unchecked((byte)-2), unchecked((byte)-1), 0, 1, 2, 3, 4, 5 };
            byte[] bBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1) };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { 0, unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-1), unchecked((byte)-2), unchecked((byte)-88), unchecked((byte)-60), 41 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide the number of one digit by the number of one digit 
         */
        [Test]
        public void testDivisionKnuthOneDigitByOneDigit()
        {
            byte[] aBytes = { 113, unchecked((byte)-83), 123, unchecked((byte)-5) };
            byte[] bBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-37) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Divide the number of multi digits by the number of one digit 
         */
        [Test]
        public void testDivisionKnuthMultiDigitsByOneDigit()
        {
            byte[] aBytes = { 113, unchecked((byte)-83), 123, unchecked((byte)-5), 18, unchecked((byte)-34), 67, 39, unchecked((byte)-29) };
            byte[] bBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-5) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-38), 2, 7, 30, 109, unchecked((byte)-43) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber / (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Remainder of division by zero
         */
        [Test]
        public void testCase15()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 0 };
            int aSign = 1;
            int bSign = 0;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            try
            {
                BigInteger.Remainder(aNumber, bNumber);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "BigInteger divide by zero", e.Message);
            }
        }

        /**
         * Remainder of division of equal numbers
         */
        [Test]
        public void testCase16()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            byte[] bBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, result.Sign);
        }

        /**
         * Remainder of division of two positive numbers
         */
        [Test]
        public void testCase17()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 12, unchecked((byte)-21), 73, 56, 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Remainder of division of two negative numbers
         */
        [Test]
        public void testCase18()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = -1;
            int bSign = -1;
            byte[] rBytes = { unchecked((byte)-13), 20, unchecked((byte)-74), unchecked((byte)-57), unchecked((byte)-27) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Remainder of division of two numbers of different signs.
         * The first is positive.
         */
        [Test]
        public void testCase19()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 12, unchecked((byte)-21), 73, 56, 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Remainder of division of two numbers of different signs.
         * The first is negative.
         */
        [Test]
        public void testCase20()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { unchecked((byte)-13), 20, unchecked((byte)-74), unchecked((byte)-57), unchecked((byte)-27) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, result.Sign);
        }

        /**
         * Tests the step D6 from the Knuth algorithm
         */
        [Test]
        public void testRemainderKnuth1()
        {
            byte[] aBytes = { unchecked((byte)-9), unchecked((byte)-8), unchecked((byte)-7), unchecked((byte)-6), unchecked((byte)-5), unchecked((byte)-4), unchecked((byte)-3), unchecked((byte)-2), unchecked((byte)-1), 0, 1 };
            byte[] bBytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 1, 2, 3, 4, 5, 6, 7, 7, 18, unchecked((byte)-89) };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide the number of one digit by the number of one digit 
         */
        [Test]
        public void testRemainderKnuthOneDigitByOneDigit()
        {
            byte[] aBytes = { 113, unchecked((byte)-83), 123, unchecked((byte)-5) };
            byte[] bBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-50) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 2, unchecked((byte)-9), unchecked((byte)-14), 53 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * Divide the number of multi digits by the number of one digit 
         */
        [Test]
        public void testRemainderKnuthMultiDigitsByOneDigit()
        {
            byte[] aBytes = { 113, unchecked((byte)-83), 123, unchecked((byte)-5), 18, unchecked((byte)-34), 67, 39, unchecked((byte)-29) };
            byte[] bBytes = { 2, unchecked((byte)-3), unchecked((byte)-4), unchecked((byte)-50) };
            int aSign = 1;
            int bSign = -1;
            byte[] rBytes = { 2, unchecked((byte)-37), unchecked((byte)-60), 59 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = BigInteger.Remainder(aNumber, bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * divideAndRemainder of two numbers of different signs.
         * The first is negative.
         */
        [Test]
        public void testCase21()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = -1;
            int bSign = 1;
            byte[][] rBytes = {
                new byte[] { unchecked((byte)-5), 94, unchecked((byte)-115), unchecked((byte)-74), unchecked((byte)-85), 84},
                new byte[] { unchecked((byte)-13), 20, unchecked((byte)-74), unchecked((byte)-57), unchecked((byte)-27)}
            };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result0 = BigInteger.DivideAndRemainder(aNumber, bNumber, out BigInteger result1);
            byte[] resBytes;
            resBytes = result0.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                if (resBytes[i] != rBytes[0][i])
                {
                    fail("Incorrect quotation");
                }
            }
            assertEquals("incorrect sign", -1, result0.Sign);
            resBytes = result1.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                if (resBytes[i] != rBytes[1][i])
                {
                    fail("Incorrect remainder");
                }
                assertEquals("incorrect sign", -1, result1.Sign);
            }
        }

        /**
         * mod when modulus is negative
         */
        [Test]
        public void testCase22()
        {
            byte[] aBytes = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] bBytes = { 1, 30, 40, 56, unchecked((byte)-1), 45 };
            int aSign = 1;
            int bSign = -1;
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            try
            {
                var _ = aNumber % (bNumber);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "BigInteger: modulus not positive", e.Message);
            }
        }

        /**
         * mod when a divisor is positive
         */
        [Test]
        public void testCase23()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = 1;
            int bSign = 1;
            byte[] rBytes = { 12, unchecked((byte)-21), 73, 56, 27 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber % (bNumber);
            byte[] resBytes;
            resBytes = result.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, result.Sign);
        }

        /**
         * mod when a divisor is negative
         */
        [Test]
        public void testCase24()
        {
            byte[] aBytes = { unchecked((byte)-127), 100, 56, 7, 98, unchecked((byte)-1), 39, unchecked((byte)-128), 127, 75 };
            byte[] bBytes = { 27, unchecked((byte)-15), 65, 39, 100 };
            int aSign = -1;
            int bSign = 1;
            byte[] rBytes = { 15, 5, unchecked((byte)-9), unchecked((byte)-17), 73 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            BigInteger bNumber = new BigInteger(bSign, bBytes);
            BigInteger result = aNumber % (bNumber);
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
