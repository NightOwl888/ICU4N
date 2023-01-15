using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigDecimal
    /// Methods: abs, compareTo, equals, hashCode, 
    /// max, min, negate, signum
    /// </summary>
    public class BigDecimalCompareTest : TestFmwk
    {
        /**
         * Abs() of a negative BigDecimal
         */
        [Test]
        public void testAbsNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "123809648392384754573567356745735635678902957849027687.87678287";
            assertEquals("incorrect value", result, BigDecimal.Abs(aNumber).ToString(CultureInfo.InvariantCulture));
        }

        /**
         * Abs() of a positive BigDecimal
         */
        [Test]
        public void testAbsPos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            String result = "123809648392384754573567356745735635678902957849027687.87678287";
            assertEquals("incorrect value", result, BigDecimal.Abs(aNumber).ToString(CultureInfo.InvariantCulture));
        }

        /**
         * Abs(MathContext) of a negative BigDecimal
         */
        [Test]
        public void testAbsMathContextNeg()
        {
            String a = "-123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int precision = 15;
            RoundingMode rm = RoundingMode.HalfDown;
            MathContext mc = new MathContext(precision, rm);
            String result = "1.23809648392385E+53";
            int resScale = -39;
            BigDecimal res = BigDecimal.Abs(aNumber, mc);
            assertEquals("incorrect value", result, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, res.Scale);
        }

        /**
         * Abs(MathContext) of a positive BigDecimal
         */
        [Test]
        public void testAbsMathContextPos()
        {
            String a = "123809648392384754573567356745735.63567890295784902768787678287E+21";
            BigDecimal aNumber = BigDecimal.Parse(a, CultureInfo.InvariantCulture);
            int precision = 41;
            RoundingMode rm = RoundingMode.HalfEven;
            MathContext mc = new MathContext(precision, rm);
            String result = "1.2380964839238475457356735674573563567890E+53";
            int resScale = -13;
            BigDecimal res = BigDecimal.Abs(aNumber, mc);
            assertEquals("incorrect value", result, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, res.Scale);
        }

        /**
         * Compare to a number of an equal scale
         */
        [Test]
        public void testCompareEqualScale1()
        {
            String a = "12380964839238475457356735674573563567890295784902768787678287";
            int aScale = 18;
            String b = "4573563567890295784902768787678287";
            int bScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = 1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Compare to a number of an equal scale
         */
        [Test]
        public void testCompareEqualScale2()
        {
            String a = "12380964839238475457356735674573563567890295784902768787678287";
            int aScale = 18;
            String b = "4573563923487289357829759278282992758247567890295784902768787678287";
            int bScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = -1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Compare to a number of an greater scale
         */
        [Test]
        public void testCompareGreaterScale1()
        {
            String a = "12380964839238475457356735674573563567890295784902768787678287";
            int aScale = 28;
            String b = "4573563567890295784902768787678287";
            int bScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = 1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Compare to a number of an greater scale
         */
        [Test]
        public void testCompareGreaterScale2()
        {
            String a = "12380964839238475457356735674573563567890295784902768787678287";
            int aScale = 48;
            String b = "4573563567890295784902768787678287";
            int bScale = 2;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = -1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Compare to a number of an less scale
         */
        [Test]
        public void testCompareLessScale1()
        {
            String a = "12380964839238475457356735674573563567890295784902768787678287";
            int aScale = 18;
            String b = "4573563567890295784902768787678287";
            int bScale = 28;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = 1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Compare to a number of an less scale
         */
        [Test]
        public void testCompareLessScale2()
        {
            String a = "12380964839238475457356735674573";
            int aScale = 36;
            String b = "45735635948573894578349572001798379183767890295784902768787678287";
            int bScale = 48;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            int result = -1;
            assertEquals("incorrect result", result, aNumber.CompareTo(bNumber));
        }

        /**
         * Equals() for unequal BigDecimals
         */
        [Test]
        public void testEqualsUnequal1()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            assertFalse("incorrect value", aNumber.Equals(bNumber));
        }

        /**
         * Equals() for unequal BigDecimals
         */
        [Test]
        public void testEqualsUnequal2()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = 13;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            assertFalse("incorrect value", aNumber.Equals(bNumber));
        }

        /**
         * Equals() for unequal BigDecimals
         */
        [Test]
        public void testEqualsUnequal3()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertFalse("incorrect value", aNumber.Equals(b));
        }

        /**
         * equals() for equal BigDecimals
         */
        [Test]
        public void testEqualsEqual()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = -24;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            assertEquals("incorrect value", aNumber, bNumber);
        }

        /**
         * equals() for equal BigDecimals
         */
        [Test]
        public void testEqualsNull()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertFalse("incorrect value", aNumber.Equals(null));
        }

        /**
         * hashCode() for equal BigDecimals
         */
        [Test]
        public void testHashCodeEqual()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = -24;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            assertEquals("incorrect value", aNumber.GetHashCode(), bNumber.GetHashCode());
        }

        /**
         * hashCode() for unequal BigDecimals
         */
        [Test]
        public void testHashCodeUnequal()
        {
            String a = "8478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = -24;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            assertTrue("incorrect value", aNumber.GetHashCode() != bNumber.GetHashCode());
        }

        /**
         * max() for equal BigDecimals
         */
        [Test]
        public void testMaxEqual()
        {
            String a = "8478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String b = "8478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "8478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Max(aNumber, bNumber));
        }

        /**
         * max() for unequal BigDecimals
         */
        [Test]
        public void testMaxUnequal1()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 24;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Max(aNumber, bNumber));
        }

        /**
         * max() for unequal BigDecimals
         */
        [Test]
        public void testMaxUnequal2()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String b = "94488478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Max(aNumber, bNumber));
        }

        /**
         * min() for equal BigDecimals
         */
        [Test]
        public void testMinEqual()
        {
            String a = "8478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String b = "8478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "8478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Min(aNumber, bNumber));
        }

        /**
         * min() for unequal BigDecimals
         */
        [Test]
        public void testMinUnequal1()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 24;
            String b = "92948782094488478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Min(aNumber, bNumber));
        }

        /**
         * min() for unequal BigDecimals
         */
        [Test]
        public void testMinUnequal2()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String b = "94488478231212478987482988429808779810457634781384756794987";
            int bScale = 41;
            String c = "94488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Min(aNumber, bNumber));
        }

        /**
         * plus() for a positive BigDecimal
         */
        [Test]
        public void testPlusPositive()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String c = "92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Plus(aNumber));
        }

        /**
         * plus(MathContext) for a positive BigDecimal
         */
        [Test]
        public void testPlusMathContextPositive()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            int precision = 37;
            RoundingMode rm = RoundingMode.Floor;
            MathContext mc = new MathContext(precision, rm);
            String c = "929487820944884782312124789.8748298842";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal res = BigDecimal.Plus(aNumber, mc);
            assertEquals("incorrect value", c, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, res.Scale);
        }

        /**
         * plus() for a negative BigDecimal
         */
        [Test]
        public void testPlusNegative()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String c = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, BigDecimal.Plus(aNumber));
        }

        /**
         * plus(MathContext) for a negative BigDecimal
         */
        [Test]
        public void testPlusMathContextNegative()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 49;
            int precision = 46;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String c = "-9294878209448847823.121247898748298842980877981";
            int cScale = 27;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal res = BigDecimal.Plus(aNumber, mc);
            assertEquals("incorrect value", c, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, res.Scale);
        }

        /**
         * negate() for a positive BigDecimal
         */
        [Test]
        public void testNegatePositive()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String c = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, -aNumber);
        }

        /**
         * negate(MathContext) for a positive BigDecimal
         */
        [Test]
        public void testNegateMathContextPositive()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            int precision = 37;
            RoundingMode rm = RoundingMode.Floor;
            MathContext mc = new MathContext(precision, rm);
            String c = "-929487820944884782312124789.8748298842";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal res = BigDecimal.Negate(aNumber, mc);
            assertEquals("incorrect value", c, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, res.Scale);
        }

        /**
         * negate() for a negative BigDecimal
         */
        [Test]
        public void testNegateNegative()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            String c = "92948782094488478231212478987482988429808779810457634781384756794987";
            int cScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal cNumber = new BigDecimal(BigInteger.Parse(c), cScale);
            assertEquals("incorrect value", cNumber, -aNumber);
        }

        /**
         * negate(MathContext) for a negative BigDecimal
         */
        [Test]
        public void testNegateMathContextNegative()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 49;
            int precision = 46;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String c = "9294878209448847823.121247898748298842980877981";
            int cScale = 27;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal res = BigDecimal.Negate(aNumber, mc);
            assertEquals("incorrect value", c, res.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, res.Scale);
        }

        /**
         * signum() for a positive BigDecimal
         */
        [Test]
        public void testSignumPositive()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertEquals("incorrect value", 1, aNumber.Sign);
        }

        /**
         * signum() for a negative BigDecimal
         */
        [Test]
        public void testSignumNegative()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertEquals("incorrect value", -1, aNumber.Sign);
        }

        /**
         * signum() for zero
         */
        [Test]
        public void testSignumZero()
        {
            String a = "0";
            int aScale = 41;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertEquals("incorrect value", 0, aNumber.Sign);
        }

        /*
         * Regression test for HARMONY-6406
         */
        [Test]
        [Ignore("ICU4N TODO: This test fails due to the inaccuracy of BigDecimal.AproxPrecision(). See: https://github.com/openjdk/jdk8u-dev/blob/987c7384267be18fe86d3bd2514d389a5d62306c/jdk/src/share/classes/java/math/BigDecimal.java#L3869-L3886")]
        public void testApproxPrecision()
        {
            BigDecimal testInstance = BigDecimal.Ten * BigDecimal.Parse("0.1", CultureInfo.InvariantCulture);
            int result = testInstance.CompareTo(BigDecimal.Parse("1.00", CultureInfo.InvariantCulture));
            assertEquals("incorrect value", 0, result);
        }
    }
}
