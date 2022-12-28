using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigDecimal
    /// Methods: movePointLeft, movePointRight, scale, setScale, unscaledValue *
    /// </summary>
    public class BigDecimalScaleOperationsTest : TestFmwk
    {
        /**
         * Check the default scale
         */
        [Test]
        public void testScaleDefault()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int cScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a));
            assertTrue("incorrect scale", aNumber.Scale == cScale);
        }

        /**
         * Check a negative scale
         */
        [Test]
        public void testScaleNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -10;
            int cScale = -10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertTrue("incorrect scale", aNumber.Scale == cScale);
        }

        /**
         * Check a positive scale
         */
        [Test]
        public void testScalePos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertTrue("incorrect scale", aNumber.Scale == cScale);
        }

        /**
         * Check the zero scale
         */
        [Test]
        public void testScaleZero()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 0;
            int cScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            assertTrue("incorrect scale", aNumber.Scale == cScale);
        }

        /**
         * Check the unscaled value
         */
        [Test]
        public void testUnscaledValue()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 100;
            BigInteger bNumber = BigInteger.Parse(a);
            BigDecimal aNumber = new BigDecimal(bNumber, aScale);
            assertTrue("incorrect unscaled value", aNumber.UnscaledValue.Equals(bNumber));
        }

        /**
         * Set a greater new scale
         */
        [Test]
        public void testSetScaleGreater()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 18;
            int newScale = 28;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertEquals("incorrect value", 0, bNumber.CompareTo(aNumber));
        }

        /**
         * Set a less new scale; this.scale == 8; newScale == 5.
         */
        [Test]
        public void testSetScaleLess()
        {
            String a = "2.345726458768760000E+10";
            int newScale = 5;
            BigDecimal aNumber = BigDecimal.Parse(a);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertEquals("incorrect value", 0, bNumber.CompareTo(aNumber));
        }

        /**
         * Verify an exception when setting a new scale
         */
        [Test]
        public void testSetScaleException()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            try
            {
                BigDecimal.SetScale(aNumber, newScale);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Rounding necessary", e.Message);
            }
        }

        /**
         * Set the same new scale
         */
        [Test]
        public void testSetScaleSame()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 18;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.Equals(aNumber));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundUp()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478139";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.Up);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundDown()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.Down);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundCeiling()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478139";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.Ceiling);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundFloor()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.Floor);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundHalfUp()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.HalfUp);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundHalfDown()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.HalfDown);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Set a new scale
         */
        [Test]
        public void testSetScaleRoundHalfEven()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.SetScale(aNumber, newScale, RoundingMode.HalfEven);
            assertTrue("incorrect scale", bNumber.Scale == newScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * SetScale(int, RoundingMode)
         */
        [Test]
        public void testSetScaleIntRoundingMode()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int newScale = 18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.SetScale(aNumber, newScale, RoundingMode.HalfEven);
            String res = "123121247898748298842980.877981045763478138";
            int resScale = 18;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Move the decimal point to the left; the shift value is positive
         */
        [Test]
        public void testMovePointLeftPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int shift = 18;
            int resScale = 46;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.MovePointLeft(aNumber, shift);
            assertTrue("incorrect scale", bNumber.Scale == resScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(a));
        }

        /**
         * Move the decimal point to the left; the shift value is positive
         */
        [Test]
        public void testMovePointLeftNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int shift = -18;
            int resScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.MovePointLeft(aNumber, shift);
            assertTrue("incorrect scale", bNumber.Scale == resScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(a));
        }

        /**
         * Move the decimal point to the right; the shift value is positive
         */
        [Test]
        public void testMovePointRightPosGreater()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int shift = 18;
            int resScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.MovePointRight(aNumber, shift);
            assertTrue("incorrect scale", bNumber.Scale == resScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(a));
        }

        /**
         * Move the decimal point to the right; the shift value is positive
         */
        [Test]
        public void testMovePointRightPosLess()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            String b = "123121247898748298842980877981045763478138475679498700";
            int aScale = 28;
            int shift = 30;
            int resScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.MovePointRight(aNumber, shift);
            assertTrue("incorrect scale", bNumber.Scale == resScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(b));
        }

        /**
         * Move the decimal point to the right; the shift value is positive
         */
        [Test]
        public void testMovePointRightNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 28;
            int shift = -18;
            int resScale = 46;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.MovePointRight(aNumber, shift);
            assertTrue("incorrect scale", bNumber.Scale == resScale);
            assertTrue("incorrect value", bNumber.UnscaledValue.ToString(CultureInfo.InvariantCulture).Equals(a));
        }

        /**
         * Move the decimal point to the right when the scale overflows
         */
        [Test]
        public void testMovePointRightException()
        {
            String a = "12312124789874829887348723648726347429808779810457634781384756794987";
            int aScale = int.MaxValue; //2147483647
            int shift = -18;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            try
            {
                BigDecimal.MovePointRight(aNumber, shift);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Underflow", e.Message);
            }
        }

        /**
         * precision()
         */
        [Test]
        public void testPrecision()
        {
            String a = "12312124789874829887348723648726347429808779810457634781384756794987";
            int aScale = 14;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            int prec = aNumber.Precision;
            assertEquals("incorrect value", 68, prec);
        }
    }
}
