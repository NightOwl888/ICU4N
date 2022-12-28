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
    /// Method: hashCode()
    /// </summary>
    public class BigIntegerHashCodeTest : TestFmwk
    {
        /**
         * Test hash codes for the same object
         */
        [Test]
        public void testSameObject()
        {
            String value1 = "12378246728727834290276457386374882976782849";
            String value2 = "-5634562095872038262928728727834290276457386374882976782849";
            BigInteger aNumber1 = BigInteger.Parse(value1);
            BigInteger aNumber2 = BigInteger.Parse(value2);
            int code1 = aNumber1.GetHashCode();
            var o1 = (aNumber1 + aNumber2) << (125);
            var o2 = (aNumber1 - aNumber2) >> (125);
            var o3 = (aNumber1 * aNumber2).ToByteArray();
            var o4 = (aNumber1 / aNumber2).BitLength;
            var o5 = (BigInteger.Pow(BigInteger.Gcd(aNumber1, aNumber2), 7));
            int code2 = aNumber1.GetHashCode();
            assertTrue("hash codes for the same object differ", code1 == code2);
        }

        /**
         * Test hash codes for equal objects.
         */
        [Test]
        public void testEqualObjects()
        {
            String value1 = "12378246728727834290276457386374882976782849";
            String value2 = "12378246728727834290276457386374882976782849";
            BigInteger aNumber1 = BigInteger.Parse(value1);
            BigInteger aNumber2 = BigInteger.Parse(value2);
            int code1 = aNumber1.GetHashCode();
            int code2 = aNumber2.GetHashCode();
            if (aNumber1.Equals(aNumber2))
            {
                assertTrue("hash codes for equal objects are unequal", code1 == code2);
            }
        }

        /**
         * Test hash codes for unequal objects.
         * The codes are unequal.
         */
        [Test]
        public void testUnequalObjectsUnequal()
        {
            String value1 = "12378246728727834290276457386374882976782849";
            String value2 = "-5634562095872038262928728727834290276457386374882976782849";
            BigInteger aNumber1 = BigInteger.Parse(value1);
            BigInteger aNumber2 = BigInteger.Parse(value2);
            int code1 = aNumber1.GetHashCode();
            int code2 = aNumber2.GetHashCode();
            if (!aNumber1.Equals(aNumber2))
            {
                assertTrue("hash codes for unequal objects are equal", code1 != code2);
            }
        }
    }
}
