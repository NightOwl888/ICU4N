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
    /// Class:  java.math.BigInteger
    /// Method: xor
    /// </summary>
    public class BigIntegerXorTest : TestFmwk
    {
        /**
         * Xor for zero and a positive number
         */
        [Test]
        public void testZeroPos()
        {
            String numA = "0";
            String numB = "27384627835298756289327365";
            String res = "27384627835298756289327365";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for zero and a negative number
         */
        [Test]
        public void testZeroNeg()
        {
            String numA = "0";
            String numB = "-27384627835298756289327365";
            String res = "-27384627835298756289327365";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for a positive number and zero 
         */
        [Test]
        public void testPosZero()
        {
            String numA = "27384627835298756289327365";
            String numB = "0";
            String res = "27384627835298756289327365";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for a negative number and zero  
         */
        [Test]
        public void testNegPos()
        {
            String numA = "-27384627835298756289327365";
            String numB = "0";
            String res = "-27384627835298756289327365";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for zero and zero
         */
        [Test]
        public void testZeroZero()
        {
            String numA = "0";
            String numB = "0";
            String res = "0";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for zero and one
         */
        [Test]
        public void testZeroOne()
        {
            String numA = "0";
            String numB = "1";
            String res = "1";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for one and one
         */
        [Test]
        public void testOneOne()
        {
            String numA = "1";
            String numB = "1";
            String res = "0";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two positive numbers of the same length
         */
        [Test]
        public void testPosPosSameLength()
        {
            String numA = "283746278342837476784564875684767";
            String numB = "293478573489347658763745839457637";
            String res = "71412358434940908477702819237626";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two positive numbers; the first is longer
         */
        [Test]
        public void testPosPosFirstLonger()
        {
            String numA = "2837462783428374767845648748973847593874837948575684767";
            String numB = "293478573489347658763745839457637";
            String res = "2837462783428374767845615168483972194300564226167553530";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two positive numbers; the first is shorter
         */
        [Test]
        public void testPosPosFirstShorter()
        {
            String numA = "293478573489347658763745839457637";
            String numB = "2837462783428374767845648748973847593874837948575684767";
            String res = "2837462783428374767845615168483972194300564226167553530";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two negative numbers of the same length
         */
        [Test]
        public void testNegNegSameLength()
        {
            String numA = "-283746278342837476784564875684767";
            String numB = "-293478573489347658763745839457637";
            String res = "71412358434940908477702819237626";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two negative numbers; the first is longer
         */
        [Test]
        public void testNegNegFirstLonger()
        {
            String numA = "-2837462783428374767845648748973847593874837948575684767";
            String numB = "-293478573489347658763745839457637";
            String res = "2837462783428374767845615168483972194300564226167553530";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two negative numbers; the first is shorter
         */
        [Test]
        public void testNegNegFirstShorter()
        {
            String numA = "293478573489347658763745839457637";
            String numB = "2837462783428374767845648748973847593874837948575684767";
            String res = "2837462783428374767845615168483972194300564226167553530";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two numbers of different signs and the same length
         */
        [Test]
        public void testPosNegSameLength()
        {
            String numA = "283746278342837476784564875684767";
            String numB = "-293478573489347658763745839457637";
            String res = "-71412358434940908477702819237628";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two numbers of different signs and the same length
         */
        [Test]
        public void testNegPosSameLength()
        {
            String numA = "-283746278342837476784564875684767";
            String numB = "293478573489347658763745839457637";
            String res = "-71412358434940908477702819237628";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for a negative and a positive numbers; the first is longer
         */
        [Test]
        public void testNegPosFirstLonger()
        {
            String numA = "-2837462783428374767845648748973847593874837948575684767";
            String numB = "293478573489347658763745839457637";
            String res = "-2837462783428374767845615168483972194300564226167553532";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for two negative numbers; the first is shorter
         */
        [Test]
        public void testNegPosFirstShorter()
        {
            String numA = "-293478573489347658763745839457637";
            String numB = "2837462783428374767845648748973847593874837948575684767";
            String res = "-2837462783428374767845615168483972194300564226167553532";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for a positive and a negative numbers; the first is longer
         */
        [Test]
        public void testPosNegFirstLonger()
        {
            String numA = "2837462783428374767845648748973847593874837948575684767";
            String numB = "-293478573489347658763745839457637";
            String res = "-2837462783428374767845615168483972194300564226167553532";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }

        /**
         * Xor for a positive and a negative number; the first is shorter
         */
        [Test]
        public void testPosNegFirstShorter()
        {
            String numA = "293478573489347658763745839457637";
            String numB = "-2837462783428374767845648748973847593874837948575684767";
            String res = "-2837462783428374767845615168483972194300564226167553532";
            BigInteger aNumber = BigInteger.Parse(numA);
            BigInteger bNumber = BigInteger.Parse(numB);
            BigInteger result = aNumber ^ (bNumber);
            assertTrue("invalid result", res.Equals(result.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
