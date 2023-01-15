﻿using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:  java.math.BigDecimal
    /// Methods: add, subtract, multiply, divide 
    /// </summary>
    public class BigDecimalArithmeticTest : TestFmwk
    {
        /**
         * Add two numbers of equal positive scales
         */
        [Test]
        public void testAddEqualScalePosPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "123121247898748373566323807282924555312937.1991359555";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber + (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of equal positive scales using MathContext
         */
        [Test]
        public void testAddMathContextEqualScalePosPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1.2313E+41";
            int cScale = -37;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(5, RoundingMode.Up);
            BigDecimal result = BigDecimal.Add(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of equal negative scales
         */
        [Test]
        public void testAddEqualScaleNegNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -10;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "1.231212478987483735663238072829245553129371991359555E+61";
            int cScale = -10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber + (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of equal negative scales using MathContext
         */
        [Test]
        public void testAddMathContextEqualScaleNegNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -10;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "1.2312E+61";
            int cScale = -57;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(5, RoundingMode.Floor);
            BigDecimal result = BigDecimal.Add(aNumber, bNumber, mc);
            assertEquals("incorrect value ", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of different scales; the first is positive
         */
        [Test]
        public void testAddDiffScalePosNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "7472334294161400358170962860775454459810457634.781384756794987";
            int cScale = 15;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber + (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of different scales using MathContext; the first is positive
         */
        [Test]
        public void testAddMathContextDiffScalePosNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "7.47233429416141E+45";
            int cScale = -31;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(15, RoundingMode.Ceiling);
            BigDecimal result = BigDecimal.Add(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, c.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two numbers of different scales; the first is negative
         */
        [Test]
        public void testAddDiffScaleNegPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1231212478987482988429808779810457634781459480137916301878791834798.7234564568";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber + (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Add two zeroes of different scales; the first is negative
         */
        [Test]
        public void testAddDiffScaleZeroZero()
        {
            String a = "0";
            int aScale = -15;
            String b = "0";
            int bScale = 10;
            String c = "0E-10";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber + (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of equal positive scales
         */
        [Test]
        public void testSubtractEqualScalePosPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "123121247898748224119637948679166971643339.7522230419";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber - (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of equal positive scales using MathContext
         */
        [Test]
        public void testSubtractMathContextEqualScalePosPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1.23121247898749E+41";
            int cScale = -27;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(15, RoundingMode.Ceiling);
            BigDecimal result = BigDecimal.Subtract(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of equal negative scales
         */
        [Test]
        public void testSubtractEqualScaleNegNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -10;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "1.231212478987482241196379486791669716433397522230419E+61";
            int cScale = -10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber - (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of different scales; the first is positive
         */
        [Test]
        public void testSubtractDiffScalePosNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "-7472334291698975400195996883915836900189542365.218615243205013";
            int cScale = 15;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber - (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of different scales using MathContext;
         *  the first is positive
         */
        [Test]
        public void testSubtractMathContextDiffScalePosNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "-7.4723342916989754E+45";
            int cScale = -29;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(17, RoundingMode.Down);
            BigDecimal result = BigDecimal.Subtract(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of different scales; the first is negative
         */
        [Test]
        public void testSubtractDiffScaleNegPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1231212478987482988429808779810457634781310033452057698121208165201.2765435432";
            int cScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber - (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Subtract two numbers of different scales using MathContext;
         *  the first is negative
         */
        [Test]
        public void testSubtractMathContextDiffScaleNegPos()
        {
            String a = "986798656676789766678767876078779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 40;
            String c = "9.867986566767897666787678760787798104576347813847567949870000000000000E+71";
            int cScale = -2;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(70, RoundingMode.HalfDown);
            BigDecimal result = BigDecimal.Subtract(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of positive scales
         */
        [Test]
        public void testMultiplyScalePosPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "92000312286217574978643009574114545567010139156902666284589309.1880727173060570190220616";
            int cScale = 25;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber * (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of positive scales using MathContext
         */
        [Test]
        public void testMultiplyMathContextScalePosPos()
        {
            String a = "97665696756578755423325476545428779810457634781384756794987";
            int aScale = -25;
            String b = "87656965586786097685674786576598865";
            int bScale = 10;
            String c = "8.561078619600910561431314228543672720908E+108";
            int cScale = -69;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(40, RoundingMode.HalfDown);
            BigDecimal result = BigDecimal.Multiply(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of negative scales
         */
        [Test]
        public void testMultiplyEqualScaleNegNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "9.20003122862175749786430095741145455670101391569026662845893091880727173060570190220616E+111";
            int cScale = -25;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber * (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of different scales
         */
        [Test]
        public void testMultiplyDiffScalePosNeg()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 10;
            String b = "747233429293018787918347987234564568";
            int bScale = -10;
            String c = "920003122862175749786430095741145455670101391569026662845893091880727173060570190220616";
            int cScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber * (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of different scales using MathContext
         */
        [Test]
        public void testMultiplyMathContextDiffScalePosNeg()
        {
            String a = "987667796597975765768768767866756808779810457634781384756794987";
            int aScale = 100;
            String b = "747233429293018787918347987234564568";
            int bScale = -70;
            String c = "7.3801839465418518653942222612429081498248509257207477E+68";
            int cScale = -16;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(53, RoundingMode.HalfUp);
            BigDecimal result = BigDecimal.Multiply(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of different scales
         */
        [Test]
        public void testMultiplyDiffScaleNegPos()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "9.20003122862175749786430095741145455670101391569026662845893091880727173060570190220616E+91";
            int cScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber * (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Multiply two numbers of different scales using MathContext
         */
        [Test]
        public void testMultiplyMathContextDiffScaleNegPos()
        {
            String a = "488757458676796558668876576576579097029810457634781384756794987";
            int aScale = -63;
            String b = "747233429293018787918347987234564568";
            int bScale = 63;
            String c = "3.6521591193960361339707130098174381429788164316E+98";
            int cScale = -52;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            MathContext mc = new MathContext(47, RoundingMode.HalfUp);
            BigDecimal result = BigDecimal.Multiply(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * pow(int)
         */
        [Test]
        public void testPow()
        {
            String a = "123121247898748298842980";
            int aScale = 10;
            int exp = 10;
            String c = "8004424019039195734129783677098845174704975003788210729597" +
                       "4875206425711159855030832837132149513512555214958035390490" +
                       "798520842025826.594316163502809818340013610490541783276343" +
                       "6514490899700151256484355936102754469438371850240000000000";
            int cScale = 100;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Pow(aNumber, exp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * pow(0)
         */
        [Test]
        public void testPow0()
        {
            String a = "123121247898748298842980";
            int aScale = 10;
            int exp = 0;
            String c = "1";
            int cScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Pow(aNumber, exp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * ZERO.pow(0)
         */
        [Test]
        public void testZeroPow0()
        {
            String c = "1";
            int cScale = 0;
            BigDecimal result = BigDecimal.Pow(BigDecimal.Zero, 0);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * pow(int, MathContext)
         */
        [Test]
        public void testPowMathContext()
        {
            String a = "123121247898748298842980";
            int aScale = 10;
            int exp = 10;
            String c = "8.0044E+130";
            int cScale = -126;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            MathContext mc = new MathContext(5, RoundingMode.HalfUp);
            BigDecimal result = BigDecimal.Pow(aNumber, exp, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", cScale, result.Scale);
        }

        /**
         * Divide by zero
         */
        [Test]
        public void testDivideByZero()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = BigDecimal.GetInstance(0L);
            try
            {
                var _ = aNumber / (bNumber);
                fail("ArithmeticException has not been caught");
            }
            catch (DivideByZeroException e)
            {
                assertEquals("Improper exception message", "Division by zero", e.Message);
            }
        }

        /**
         * Divide with ROUND_UNNECESSARY
         */
        [Test]
        public void testDivideExceptionRM()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            try
            {
                BigDecimal.Divide(aNumber, bNumber, RoundingMode.Unnecessary);
                fail("ArithmeticException has not been caught");
            }
            catch (ArithmeticException e)
            {
                assertEquals("Improper exception message", "Rounding necessary", e.Message);
            }
        }

        /**
         * Divide with invalid rounding mode
         */
        [Test]
        public void testDivideExceptionInvalidRM()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            try
            {
                BigDecimal.Divide(aNumber, bNumber, (RoundingMode)100);
                fail("IllegalArgumentException has not been caught");
            }
            catch (ArgumentOutOfRangeException e)
            {
                // ICU4N: We changed the error message. All we care about is the exception type.
                //assertEquals("Improper exception message", "Invalid rounding mode", e.Message);
            }
        }

        /**
         * Divide: local variable exponent is less than zero
         */
        [Test]
        public void testDivideExpLessZero()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = 15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1.64770E+10";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: local variable exponent is equal to zero
         */
        [Test]
        public void testDivideExpEqualsZero()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 10;
            String c = "1.64769459009933764189139568605273529E+40";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: local variable exponent is greater than zero
         */
        [Test]
        public void testDivideExpGreaterZero()
        {
            String a = "1231212478987482988429808779810457634781384756794987";
            int aScale = -15;
            String b = "747233429293018787918347987234564568";
            int bScale = 20;
            String c = "1.647694590099337641891395686052735285121058381E+50";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: remainder is zero
         */
        [Test]
        public void testDivideRemainderIsZero()
        {
            String a = "8311389578904553209874735431110";
            int aScale = -15;
            String b = "237468273682987234567849583746";
            int bScale = 20;
            String c = "3.5000000000000000000000000000000E+36";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_UP, result is negative
         */
        [Test]
        [Ignore("ICU4N TODO: This test fails due to the inaccuracy of BigDecimal.AproxPrecision(). See: https://github.com/openjdk/jdk8u-dev/blob/987c7384267be18fe86d3bd2514d389a5d62306c/jdk/src/share/classes/java/math/BigDecimal.java#L3869-L3886")]
        public void testDivideRoundUpNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_UP, result is positive
         */
        [Test]
        public void testDivideRoundUpPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Up);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_DOWN, result is negative
         */
        [Test]
        public void testDivideRoundDownNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799283E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Down);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_DOWN, result is positive
         */
        [Test]
        public void testDivideRoundDownPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799283E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Down);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_FLOOR, result is positive
         */
        [Test]
        public void testDivideRoundFloorPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799283E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Floor);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_FLOOR, result is negative
         */
        [Test]
        public void testDivideRoundFloorNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Floor);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_CEILING, result is positive
         */
        [Test]
        public void testDivideRoundCeilingPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_CEILING, result is negative
         */
        [Test]
        public void testDivideRoundCeilingNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799283E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.Ceiling);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is positive; distance = -1
         */
        [Test]
        public void testDivideRoundHalfUpPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfUp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is negative; distance = -1
         */
        [Test]
        public void testDivideRoundHalfUpNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfUp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is positive; distance = 1
         */
        [Test]
        public void testDivideRoundHalfUpPos1()
        {
            String a = "92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfUp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is negative; distance = 1
         */
        [Test]
        public void testDivideRoundHalfUpNeg1()
        {
            String a = "-92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "-1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfUp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is negative; equidistant
         */
        [Test]
        public void testDivideRoundHalfUpNeg2()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            String c = "-1E+5";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfUp);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_DOWN, result is positive; distance = -1
         */
        [Test]
        public void testDivideRoundHalfDownPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfDown);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_DOWN, result is negative; distance = -1
         */
        [Test]
        public void testDivideRoundHalfDownNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfDown);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_DOWN, result is positive; distance = 1
         */
        [Test]
        public void testDivideRoundHalfDownPos1()
        {
            String a = "92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfDown);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_DOWN, result is negative; distance = 1
         */
        [Test]
        public void testDivideRoundHalfDownNeg1()
        {
            String a = "-92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "-1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfDown);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_UP, result is negative; equidistant
         */
        [Test]
        public void testDivideRoundHalfDownNeg2()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            String c = "0E+5";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfDown);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_EVEN, result is positive; distance = -1
         */
        [Test]
        public void testDivideRoundHalfEvenPos()
        {
            String a = "92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfEven);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_EVEN, result is negative; distance = -1
         */
        [Test]
        public void testDivideRoundHalfEvenNeg()
        {
            String a = "-92948782094488478231212478987482988429808779810457634781384756794987";
            int aScale = -24;
            String b = "7472334223847623782375469293018787918347987234564568";
            int bScale = 13;
            String c = "-1.24390557635720517122423359799284E+53";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfEven);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_EVEN, result is positive; distance = 1
         */
        [Test]
        public void testDivideRoundHalfEvenPos1()
        {
            String a = "92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfEven);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_EVEN, result is negative; distance = 1
         */
        [Test]
        public void testDivideRoundHalfEvenNeg1()
        {
            String a = "-92948782094488478231212478987482988798104576347813847567949855464535634534563456";
            int aScale = -24;
            String b = "74723342238476237823754692930187879183479";
            int bScale = 13;
            String c = "-1.2439055763572051712242335979928354832010167729111113605E+76";
            int resScale = -21;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfEven);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide: rounding mode is ROUND_HALF_EVEN, result is negative; equidistant
         */
        [Test]
        public void testDivideRoundHalfEvenNeg2()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            String c = "0E+5";
            int resScale = -5;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, resScale, RoundingMode.HalfEven);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide to BigDecimal
         */
        [Test]
        public void testDivideBigDecimal1()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            String c = "-5E+4";
            int resScale = -4;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber / (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * Divide to BigDecimal
         */
        [Test]
        public void testDivideBigDecimal2()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = -15;
            String c = "-5E-26";
            int resScale = 26;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = aNumber / (bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeUP()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = -15;
            int newScale = 31;
            RoundingMode rm = RoundingMode.Up;
            String c = "-5.00000E-26";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeDOWN()
        {
            String a = "-37361671119238118911893939591735";
            int aScale = 10;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            int newScale = 31;
            RoundingMode rm = RoundingMode.Down;
            String c = "-50000.0000000000000000000000000000000";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeCEILING()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 100;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            int newScale = 45;
            RoundingMode rm = RoundingMode.Ceiling;
            String c = "1E-45";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeFLOOR()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 100;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            int newScale = 45;
            RoundingMode rm = RoundingMode.Floor;
            String c = "0E-45";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeHALF_UP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -51;
            String b = "74723342238476237823787879183470";
            int bScale = 45;
            int newScale = 3;
            RoundingMode rm = RoundingMode.HalfUp;
            String c = "50000260373164286401361913262100972218038099522752460421" +
                       "05959924024355721031761947728703598332749334086415670525" +
                       "3761096961.670";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeHALF_DOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 5;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            int newScale = 7;
            RoundingMode rm = RoundingMode.HalfDown;
            String c = "500002603731642864013619132621009722.1803810";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, scale, RoundingMode)
         */
        [Test]
        public void testDivideBigDecimalScaleRoundingModeHALF_EVEN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 5;
            String b = "74723342238476237823787879183470";
            int bScale = 15;
            int newScale = 7;
            RoundingMode rm = RoundingMode.HalfEven;
            String c = "500002603731642864013619132621009722.1803810";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, newScale, rm);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", newScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextUP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 15;
            String b = "748766876876723342238476237823787879183470";
            int bScale = 10;
            int precision = 21;
            RoundingMode rm = RoundingMode.Up;
            MathContext mc = new MathContext(precision, rm);
            String c = "49897861180.2562512996";
            int resScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextDOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 15;
            String b = "748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.Down;
            MathContext mc = new MathContext(precision, rm);
            String c = "4.98978611802562512995E+70";
            int resScale = -50;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextCEILING()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 15;
            String b = "748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.Ceiling;
            MathContext mc = new MathContext(precision, rm);
            String c = "4.98978611802562512996E+70";
            int resScale = -50;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextFLOOR()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 15;
            String b = "748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.Floor;
            MathContext mc = new MathContext(precision, rm);
            String c = "4.98978611802562512995E+70";
            int resScale = -50;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextHALF_UP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.HalfUp;
            MathContext mc = new MathContext(precision, rm);
            String c = "2.77923185514690367475E+26";
            int resScale = -6;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextHALF_DOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.HalfDown;
            MathContext mc = new MathContext(precision, rm);
            String c = "2.77923185514690367475E+26";
            int resScale = -6;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divide(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideBigDecimalScaleMathContextHALF_EVEN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 21;
            RoundingMode rm = RoundingMode.HalfEven;
            MathContext mc = new MathContext(precision, rm);
            String c = "2.77923185514690367475E+26";
            int resScale = -6;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }


        /**
         * BigDecimal.divide with a scale that's too large
         * 
         * Regression test for HARMONY-6271
         */
        [Test]
        public void testDivideLargeScale()
        {
            BigDecimal arg1 = BigDecimal.Parse("320.0E+2147483647", CultureInfo.InvariantCulture);
            BigDecimal arg2 = BigDecimal.Parse("6E-2147483647", CultureInfo.InvariantCulture);
            try
            {
                BigDecimal result = BigDecimal.Divide(arg1, arg2, int.MaxValue,
                        RoundingMode.Ceiling);
                fail("Expected ArithmeticException when dividing with a scale that's too large");
            }
            catch (ArithmeticException e)
            {
                // expected behaviour
            }
        }

        /**
         * divideToIntegralValue(BigDecimal)
         */
        [Test]
        public void testDivideToIntegralValue()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            String c = "277923185514690367474770683";
            int resScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.DivideToIntegralValue(aNumber, bNumber);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divideToIntegralValue(BigDecimal, MathContext)
         */
        [Test]
        [Ignore("ICU4N TODO: This test fails due to the inaccuracy of BigDecimal.AproxPrecision(). See: https://github.com/openjdk/jdk8u-dev/blob/987c7384267be18fe86d3bd2514d389a5d62306c/jdk/src/share/classes/java/math/BigDecimal.java#L3869-L3886")]
        public void testDivideToIntegralValueMathContextUP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 32;
            RoundingMode rm = RoundingMode.Up;
            MathContext mc = new MathContext(precision, rm);
            String c = "277923185514690367474770683";
            int resScale = 0;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Divide(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divideToIntegralValue(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideToIntegralValueMathContextDOWN()
        {
            String a = "3736186567876876578956958769675785435673453453653543654354365435675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 75;
            RoundingMode rm = RoundingMode.Down;
            MathContext mc = new MathContext(precision, rm);
            String c = "2.7792318551469036747477068339450205874992634417590178670822889E+62";
            int resScale = -1;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.DivideToIntegralValue(aNumber, bNumber, mc);
            assertEquals("incorrect value", c, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * divideAndRemainder(BigDecimal)
         */
        [Test]
        public void testDivideAndRemainder1()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            String res = "277923185514690367474770683";
            int resScale = 0;
            String rem = "1.3032693871288309587558885943391070087960319452465789990E-15";
            int remScale = 70;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result0 = BigDecimal.DivideAndRemainder(aNumber, bNumber, out BigDecimal result1);
            assertEquals("incorrect quotient value", res, result0.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result0.Scale);
            assertEquals("incorrect remainder value", rem, result1.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect remainder scale", remScale, result1.Scale);
        }

        /**
         * divideAndRemainder(BigDecimal)
         */
        [Test]
        public void testDivideAndRemainder2()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            String res = "2779231855146903674747706830969461168692256919247547952" +
                         "2608549363170374005512836303475980101168105698072946555" +
                         "6862849";
            int resScale = 0;
            String rem = "3.4935796954060524114470681810486417234751682675102093970E-15";
            int remScale = 70;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result0 = BigDecimal.DivideAndRemainder(aNumber, bNumber, out BigDecimal result1);
            assertEquals("incorrect quotient value", res, result0.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result0.Scale);
            assertEquals("incorrect remainder value", rem, result1.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect remainder scale", remScale, result1.Scale);
        }

        /**
         * divideAndRemainder(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideAndRemainderMathContextUP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 70;
            int precision = 75;
            RoundingMode rm = RoundingMode.Up;
            MathContext mc = new MathContext(precision, rm);
            String res = "277923185514690367474770683";
            int resScale = 0;
            String rem = "1.3032693871288309587558885943391070087960319452465789990E-15";
            int remScale = 70;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result0 = BigDecimal.DivideAndRemainder(aNumber, bNumber, mc, out BigDecimal result1);
            assertEquals("incorrect quotient value", res, result0.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result0.Scale);
            assertEquals("incorrect remainder value", rem, result1.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect remainder scale", remScale, result1.Scale);
        }

        /**
         * divideAndRemainder(BigDecimal, MathContext)
         */
        [Test]
        public void testDivideAndRemainderMathContextDOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 20;
            int precision = 15;
            RoundingMode rm = RoundingMode.Down;
            MathContext mc = new MathContext(precision, rm);
            String res = "0E-25";
            int resScale = 25;
            String rem = "3736186567876.876578956958765675671119238118911893939591735";
            int remScale = 45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result0 = BigDecimal.DivideAndRemainder(aNumber, bNumber, mc, out BigDecimal result1);
            assertEquals("incorrect quotient value", res, result0.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result0.Scale);
            assertEquals("incorrect remainder value", rem, result1.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect remainder scale", remScale, result1.Scale);
        }

        /**
         * remainder(BigDecimal)
         */
        [Test]
        public void testRemainder1()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 10;
            String res = "3736186567876.876578956958765675671119238118911893939591735";
            int resScale = 45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Remainder(aNumber, bNumber);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * remainder(BigDecimal)
         */
        [Test]
        public void testRemainder2()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 10;
            String res = "1149310942946292909508821656680979993738625937.2065885780";
            int resScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Remainder(aNumber, bNumber);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * remainder(BigDecimal, MathContext)
         */
        [Test]
        public void testRemainderMathContextHALF_UP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 10;
            int precision = 15;
            RoundingMode rm = RoundingMode.HalfUp;
            MathContext mc = new MathContext(precision, rm);
            String res = "3736186567876.876578956958765675671119238118911893939591735";
            int resScale = 45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Remainder(aNumber, bNumber, mc);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * remainder(BigDecimal, MathContext)
         */
        [Test]
        public void testRemainderMathContextHALF_DOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -45;
            String b = "134432345432345748766876876723342238476237823787879183470";
            int bScale = 10;
            int precision = 75;
            RoundingMode rm = RoundingMode.HalfDown;
            MathContext mc = new MathContext(precision, rm);
            String res = "1149310942946292909508821656680979993738625937.2065885780";
            int resScale = 10;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal bNumber = new BigDecimal(BigInteger.Parse(b), bScale);
            BigDecimal result = BigDecimal.Remainder(aNumber, bNumber, mc);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * round(BigDecimal, MathContext)
         */
        [Test]
        public void testRoundMathContextHALF_DOWN()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -45;
            int precision = 75;
            RoundingMode rm = RoundingMode.HalfDown;
            MathContext mc = new MathContext(precision, rm);
            String res = "3.736186567876876578956958765675671119238118911893939591735E+102";
            int resScale = -45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Round(aNumber, mc);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * round(BigDecimal, MathContext)
         */
        [Test]
        public void testRoundMathContextHALF_UP()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            int precision = 15;
            RoundingMode rm = RoundingMode.HalfUp;
            MathContext mc = new MathContext(precision, rm);
            String res = "3736186567876.88";
            int resScale = 2;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Round(aNumber, mc);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", resScale, result.Scale);
        }

        /**
         * round(BigDecimal, MathContext) when precision = 0
         */
        [Test]
        public void testRoundMathContextPrecision0()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            int precision = 0;
            RoundingMode rm = RoundingMode.HalfUp;
            MathContext mc = new MathContext(precision, rm);
            String res = "3736186567876.876578956958765675671119238118911893939591735";
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Round(aNumber, mc);
            assertEquals("incorrect quotient value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect quotient scale", aScale, result.Scale);
        }


        /**
         * ulp() of a positive BigDecimal
         */
        [Test]
        public void testUlpPos()
        {
            String a = "3736186567876876578956958765675671119238118911893939591735";
            int aScale = -45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Ulp(aNumber);
            String res = "1E+45";
            int resScale = -45;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * ulp() of a negative BigDecimal
         */
        [Test]
        public void testUlpNeg()
        {
            String a = "-3736186567876876578956958765675671119238118911893939591735";
            int aScale = 45;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Ulp(aNumber);
            String res = "1E-45";
            int resScale = 45;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }

        /**
         * ulp() of a negative BigDecimal
         */
        [Test]
        public void testUlpZero()
        {
            String a = "0";
            int aScale = 2;
            BigDecimal aNumber = new BigDecimal(BigInteger.Parse(a), aScale);
            BigDecimal result = BigDecimal.Ulp(aNumber);
            String res = "0.01";
            int resScale = 2;
            assertEquals("incorrect value", res, result.ToString(CultureInfo.InvariantCulture));
            assertEquals("incorrect scale", resScale, result.Scale);
        }
    }
}
