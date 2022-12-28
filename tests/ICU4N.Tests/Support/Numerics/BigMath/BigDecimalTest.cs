﻿using ICU4N.Dev.Test;
using NUnit.Framework;
using System;

namespace ICU4N.Numerics.BigMath
{
    public class BigDecimalTest : TestFmwk
    {
        BigInteger value = BigInteger.Parse("12345908");

        BigInteger value2 = BigInteger.Parse("12334560000");

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.math.BigInteger)
         */
        [Test]
        public void test_ConstructorLjava_math_BigInteger()
        {
            BigDecimal big = new BigDecimal(value);
            assertTrue("the BigDecimal value is not initialized properly", big
                    .UnscaledValue.Equals(value)
                    && big.Scale == 0);
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.math.BigInteger, int)
         */
        [Test]
        public void test_ConstructorLjava_math_BigIntegerI()
        {
            BigDecimal big = new BigDecimal(value2, 5);
            assertTrue("the BigDecimal value is not initialized properly", big
                    .UnscaledValue.Equals(value2)
                    && big.Scale == 5);
            assertTrue("the BigDecimal value is not represented properly", big
                    .ToString().Equals("123345.60000"));
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(double)
         */
        [Test]
        public void test_ConstructorD()
        {
            BigDecimal big = new BigDecimal(123E04);
            assertTrue(
                    "the BigDecimal value taking a double argument is not initialized properly",
                    big.ToString().Equals("1230000"));
            big = new BigDecimal(1.2345E-12);
            assertTrue("the double representation is not correct", big
                    .ToDouble() == 1.2345E-12);
            big = new BigDecimal(-12345E-3);
            assertTrue("the double representation is not correct", big
                    .ToDouble() == -12.345);
            big = new BigDecimal(5.1234567897654321e138);
            assertTrue("the double representation is not correct", big
                    .ToDouble() == 5.1234567897654321E138
                    && big.Scale == 0);
            big = new BigDecimal(0.1);
            assertTrue(
                    "the double representation of 0.1 bigDecimal is not correct",
                    big.ToDouble() == 0.1);
            big = new BigDecimal(0.00345);
            assertTrue(
                    "the double representation of 0.00345 bigDecimal is not correct",
                    big.ToDouble() == 0.00345);
            // regression test for HARMONY-2429
            big = new BigDecimal(-0.0);
            assertTrue(
                    "the double representation of -0.0 bigDecimal is not correct",
                    big.Scale == 0);
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.lang.String)
         */
        [Test]
        public void test_ConstructorLjava_lang_String() //throws NumberFormatException
        {
            BigDecimal big = BigDecimal.Parse("345.23499600293850");
            assertTrue("the BigDecimal value is not initialized properly", big
                    .ToString().Equals("345.23499600293850")
                    && big.Scale == 14);
            big = BigDecimal.Parse("-12345");
            assertTrue("the BigDecimal value is not initialized properly", big
                    .ToString().Equals("-12345")
                    && big.Scale == 0);
            big = BigDecimal.Parse("123.");
            assertTrue("the BigDecimal value is not initialized properly", big
                    .ToString().Equals("123")
                    && big.Scale == 0);

            BigDecimal.Parse("1.234E02");
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.lang.String)
         */
        [Test]
        public void test_constructor_String_plus_exp()
        {
            /*
             * BigDecimal does not support a + sign in the exponent when converting
             * from a String
             */
            new BigDecimal(+23e-0);
            new BigDecimal(-23e+0);
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.lang.String)
         */
        [Test]
        public void test_constructor_String_empty()
        {
            try
            {
                BigDecimal.Parse("");
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(java.lang.String)
         */
        [Test]
        public void test_constructor_String_plus_minus_exp() // ICU4N TODO: Tests for TryParse
        {
            try
            {
                BigDecimal.Parse("+35e+-2");
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
            }

            try
            {
                BigDecimal.Parse("-35e-+2");
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#BigDecimal(char[])
         */
        [Test]
        public void test_constructor_CC_plus_minus_exp()
        {
            try
            {
                BigDecimal.Parse("+35e+-2".ToCharArray());
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
            }

            try
            {
                BigDecimal.Parse("-35e-+2".ToCharArray());
                fail("NumberFormatException expected");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#abs()
         */
        [Test]
        public void test_abs()
        {
            BigDecimal big = BigDecimal.Parse("-1234");
            BigDecimal bigabs = BigDecimal.Abs(big);
            assertTrue("the absolute value of -1234 is not 1234", bigabs.ToString()
                    .Equals("1234"));
            big = new BigDecimal(BigInteger.Parse("2345"), 2);
            bigabs = BigDecimal.Abs(big);
            assertTrue("the absolute value of 23.45 is not 23.45", bigabs
                    .ToString().Equals("23.45"));
        }

        /**
         * @tests java.math.BigDecimal#add(java.math.BigDecimal)
         */
        [Test]
        public void test_addLjava_math_BigDecimal()
        {
            BigDecimal add1 = BigDecimal.Parse("23.456");
            BigDecimal add2 = BigDecimal.Parse("3849.235");
            BigDecimal sum = BigDecimal.Add(add1, add2);
            assertTrue("the sum of 23.456 + 3849.235 is wrong", sum.UnscaledValue
                    .ToString().Equals("3872691")
                    && sum.Scale == 3);
            assertTrue("the sum of 23.456 + 3849.235 is not printed correctly", sum
                    .ToString().Equals("3872.691"));
            BigDecimal add3 = new BigDecimal(12.34E02D);
            assertTrue("the sum of 23.456 + 12.34E02 is not printed correctly",
                    (BigDecimal.Add(add1, add3)).ToString().Equals("1257.456"));
        }

        /**
         * @tests java.math.BigDecimal#compareTo(java.math.BigDecimal)
         */
        [Test]
        public void test_compareToLjava_math_BigDecimal()
        {
            BigDecimal comp1 = BigDecimal.Parse("1.00");
            BigDecimal comp2 = new BigDecimal(1.000000D);
            assertTrue("1.00 and 1.000000 should be equal",
                    comp1.CompareTo(comp2) == 0);
            BigDecimal comp3 = BigDecimal.Parse("1.02");
            assertTrue("1.02 should be bigger than 1.00",
                    comp3.CompareTo(comp1) == 1);
            BigDecimal comp4 = new BigDecimal(0.98D);
            assertTrue("0.98 should be less than 1.00",
                    comp4.CompareTo(comp1) == -1);
        }

        /**
         * @tests java.math.BigDecimal#divide(java.math.BigDecimal, int)
         */
        [Test]
        public void test_divideLjava_math_BigDecimalI()
        {
            BigDecimal divd1 = new BigDecimal(value, 2);
            BigDecimal divd2 = BigDecimal.Parse("2.335");
            BigDecimal divd3 = BigDecimal.Divide(divd1, divd2, RoundingMode.Up);
            assertTrue("123459.08/2.335 is not correct", divd3.ToString().Equals(
                    "52873.27")
                    && divd3.Scale == divd1.Scale);
            assertTrue(
                    "the unscaledValue representation of 123459.08/2.335 is not correct",
                    divd3.UnscaledValue.ToString().Equals("5287327"));
            divd2 = new BigDecimal(123.4D);
            divd3 = BigDecimal.Divide(divd1, divd2, RoundingMode.Down);
            assertTrue("123459.08/123.4  is not correct", divd3.ToString().Equals(
                    "1000.47")
                    && divd3.Scale == 2);
            divd2 = new BigDecimal(000D);

            try
            {
                BigDecimal.Divide(divd1, divd2, RoundingMode.Down);
                fail("divide by zero is not caught");
            }
            catch (DivideByZeroException e) // ICU4N: .NET has a specialized exception for this
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#divide(java.math.BigDecimal, int, int)
         */
        [Test]
        public void test_divideLjava_math_BigDecimalII()
        {
            BigDecimal divd1 = new BigDecimal(value2, 4);
            BigDecimal divd2 = BigDecimal.Parse("0.0023");
            BigDecimal divd3 = BigDecimal.Divide(divd1, divd2, 3, RoundingMode.HalfUp);
            assertTrue("1233456/0.0023 is not correct", divd3.ToString().Equals(
                    "536285217.391")
                    && divd3.Scale == 3);
            divd2 = new BigDecimal(1345.5E-02D);
            divd3 = BigDecimal.Divide(divd1, divd2, 0, RoundingMode.Down);
            assertTrue(
                    "1233456/13.455 is not correct or does not have the correct scale",
                    divd3.ToString().Equals("91672") && divd3.Scale == 0);
            divd2 = new BigDecimal(0000D);

            try
            {
                BigDecimal.Divide(divd1, divd2, 4, RoundingMode.Down);
                fail("divide by zero is not caught");
            }
            catch (DivideByZeroException e) // ICU4N: .NET has a specialized exception for this
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#doubleValue()
         */
        [Test]
        public void test_doubleValue()
        {
            BigDecimal bigDB = new BigDecimal(-1.234E-112);
            //		Commenting out this part because it causes an endless loop (see HARMONY-319 and HARMONY-329)
            //		assertTrue(
            //				"the double representation of this BigDecimal is not correct",
            //				bigDB.doubleValue() == -1.234E-112);
            bigDB = new BigDecimal(5.00E-324);
            assertTrue("the double representation of bigDecimal is not correct",
                    bigDB.ToDouble() == 5.00E-324);
            bigDB = new BigDecimal(1.79E308);
            assertTrue("the double representation of bigDecimal is not correct",
                    bigDB.ToDouble() == 1.79E308 && bigDB.Scale == 0);
            bigDB = new BigDecimal(-2.33E102);
            assertTrue(
                    "the double representation of bigDecimal -2.33E102 is not correct",
                    bigDB.ToDouble() == -2.33E102 && bigDB.Scale == 0);
            bigDB = new BigDecimal(double.MaxValue);
            bigDB = bigDB + bigDB;
            assertTrue(
                    "a  + number out of the double range should return infinity",
                    bigDB.ToDouble() == double.PositiveInfinity);
            bigDB = new BigDecimal(-double.MaxValue);
            bigDB = bigDB + bigDB;
            assertTrue(
                    "a  - number out of the double range should return neg infinity",
                    bigDB.ToDouble() == double.NegativeInfinity);
        }

        /**
         * @tests java.math.BigDecimal#equals(java.lang.Object)
         */
        [Test]
        public void test_equalsLjava_lang_Object()
        {
            BigDecimal equal1 = new BigDecimal(1.00D);
            BigDecimal equal2 = BigDecimal.Parse("1.0");
            assertFalse("1.00 and 1.0 should not be equal",
                    equal1.Equals(equal2));
            equal2 = new BigDecimal(1.01D);
            assertFalse("1.00 and 1.01 should not be equal",
                    equal1.Equals(equal2));
            equal2 = BigDecimal.Parse("1.00");
            assertFalse("1.00D and 1.00 should not be equal",
                    equal1.Equals(equal2));
            BigInteger val = BigInteger.Parse("100");
            equal1 = BigDecimal.Parse("1.00");
            equal2 = new BigDecimal(val, 2);
            assertTrue("1.00(string) and 1.00(bigInteger) should be equal", equal1
                    .Equals(equal2));
            equal1 = new BigDecimal(100D);
            equal2 = BigDecimal.Parse("2.34576");
            assertFalse("100D and 2.34576 should not be equal", equal1
                    .Equals(equal2));
            assertFalse("bigDecimal 100D does not equal string 23415", equal1
                    .Equals("23415"));
        }

        /**
         * @tests java.math.BigDecimal#floatValue()
         */
        [Test]
        public void test_floatValue()
        {
            BigDecimal fl1 = BigDecimal.Parse("234563782344567");
            assertTrue("the float representation of bigDecimal 234563782344567",
                    fl1.ToSingle() == 234563782344567f);
            BigDecimal fl2 = new BigDecimal(2.345E37);
            assertTrue("the float representation of bigDecimal 2.345E37", fl2
                    .ToSingle() == 2.345E37F);
            fl2 = new BigDecimal(-1.00E-44);
            assertTrue("the float representation of bigDecimal -1.00E-44", fl2
                    .ToSingle() == -1.00E-44F);
            fl2 = new BigDecimal(-3E12);
            assertTrue("the float representation of bigDecimal -3E12", fl2
                    .ToSingle() == -3E12F);
            fl2 = new BigDecimal(double.MaxValue);
            assertTrue(
                    "A number can't be represented by float should return infinity",
                    fl2.ToSingle() == float.PositiveInfinity);
            fl2 = new BigDecimal(-double.MaxValue);
            assertTrue(
                    "A number can't be represented by float should return infinity",
                    fl2.ToSingle() == float.NegativeInfinity);

        }

        /**
         * @tests java.math.BigDecimal#hashCode()
         */
        [Test]
        public void test_hashCode()
        {
            // anything that is equal must have the same hashCode
            BigDecimal hash = BigDecimal.Parse("1.00");
            BigDecimal hash2 = new BigDecimal(1.00D);
            assertTrue("the hashCode of 1.00 and 1.00D is equal",
                    hash.GetHashCode() != hash2.GetHashCode() && !hash.Equals(hash2));
            hash2 = BigDecimal.Parse("1.0");
            assertTrue("the hashCode of 1.0 and 1.00 is equal",
                    hash.GetHashCode() != hash2.GetHashCode() && !hash.Equals(hash2));
            BigInteger val = BigInteger.Parse("100");
            hash2 = new BigDecimal(val, 2);
            assertTrue("hashCode of 1.00 and 1.00(bigInteger) is not equal", hash
                    .GetHashCode() == hash2.GetHashCode()
                    && hash.Equals(hash2));
            hash = new BigDecimal(value, 2);
            hash2 = BigDecimal.Parse("-1233456.0000");
            assertTrue("hashCode of 123459.08 and -1233456.0000 is not equal", hash
                    .GetHashCode() != hash2.GetHashCode()
                    && !hash.Equals(hash2));
            hash2 = new BigDecimal(-value, 2);
            assertTrue("hashCode of 123459.08 and -123459.08 is not equal", hash
                    .GetHashCode() != hash2.GetHashCode()
                    && !hash.Equals(hash2));
        }

        /**
         * @tests java.math.BigDecimal#intValue()
         */
        [Test]
        public void test_intValue()
        {
            BigDecimal int1 = new BigDecimal(value, 3);
            assertTrue("the int value of 12345.908 is not 12345",
                    int1.ToInt32() == 12345);
            int1 = BigDecimal.Parse("1.99");
            assertTrue("the int value of 1.99 is not 1", int1.ToInt32() == 1);
            int1 = BigDecimal.Parse("23423419083091823091283933");
            // ran JDK and found representation for the above was -249268259
            assertTrue("the int value of 23423419083091823091283933 is wrong", int1
                    .ToInt32() == -249268259);
            int1 = new BigDecimal(-1235D);
            assertTrue("the int value of -1235 is not -1235",
                    int1.ToInt32() == -1235);
        }

        /**
         * @tests java.math.BigDecimal#longValue()
         */
        [Test]
        public void test_longValue()
        {
            BigDecimal long1 = new BigDecimal(-value2, 0);
            assertTrue("the long value of 12334560000 is not 12334560000", long1
                    .ToInt64() == -12334560000L);
            long1 = new BigDecimal(-1345.348E-123D);
            assertTrue("the long value of -1345.348E-123D is not zero", long1
                    .ToInt64() == 0);
            long1 = BigDecimal.Parse("31323423423419083091823091283933");
            // ran JDK and found representation for the above was
            // -5251313250005125155
            assertTrue(
                    "the long value of 31323423423419083091823091283933 is wrong",
                    long1.ToInt64() == -5251313250005125155L);
        }

        /**
         * @tests java.math.BigDecimal#max(java.math.BigDecimal)
         */
        [Test]
        public void test_maxLjava_math_BigDecimal()
        {
            BigDecimal max1 = new BigDecimal(value2, 1);
            BigDecimal max2 = new BigDecimal(value2, 4);
            assertTrue("1233456000.0 is not greater than 1233456", BigDecimal.Max(max1, max2)
                    .Equals(max1));
            max1 = new BigDecimal(-1.224D);
            max2 = new BigDecimal(-1.2245D);
            assertTrue("-1.224 is not greater than -1.2245", BigDecimal.Max(max1, max2).Equals(
                    max1));
            max1 = new BigDecimal(123E18);
            max2 = new BigDecimal(123E19);
            assertTrue("123E19 is the not the max", BigDecimal.Max(max1, max2).Equals(max2));
        }

        /**
         * @tests java.math.BigDecimal#min(java.math.BigDecimal)
         */
        [Test]
        public void test_minLjava_math_BigDecimal()
        {
            BigDecimal min1 = new BigDecimal(-12345.4D);
            BigDecimal min2 = new BigDecimal(-12345.39D);
            assertTrue("-12345.39 should have been returned", BigDecimal.Min(min1, min2)
                    .Equals(min1));
            min1 = new BigDecimal(value2, 5);
            min2 = new BigDecimal(value2, 0);
            assertTrue("123345.6 should have been returned", BigDecimal.Min(min1, min2).Equals(
                    min1));
        }

        /**
         * @tests java.math.BigDecimal#movePointLeft(int)
         */
        [Test]
        public void test_movePointLeftI()
        {
            BigDecimal movePtLeft = BigDecimal.Parse("123456265.34");
            BigDecimal alreadyMoved = BigDecimal.MovePointLeft(movePtLeft, 5);
            assertTrue("move point left 5 failed", alreadyMoved.Scale == 7
                    && alreadyMoved.ToString().Equals("1234.5626534"));
            movePtLeft = new BigDecimal(-value2, 0);
            alreadyMoved = BigDecimal.MovePointLeft(movePtLeft, 12);
            assertTrue("move point left 12 failed", alreadyMoved.Scale == 12
                    && alreadyMoved.ToString().Equals("-0.012334560000"));
            movePtLeft = new BigDecimal(123E18);
            alreadyMoved = BigDecimal.MovePointLeft(movePtLeft, 2);
            assertTrue("move point left 2 failed",
                    alreadyMoved.Scale == movePtLeft.Scale + 2
                            && alreadyMoved.ToDouble() == 1.23E18);
            movePtLeft = new BigDecimal(1.123E-12);
            alreadyMoved = BigDecimal.MovePointLeft(movePtLeft, 3);
            assertTrue("move point left 3 failed",
                    alreadyMoved.Scale == movePtLeft.Scale + 3
                            && alreadyMoved.ToDouble() == 1.123E-15);
            movePtLeft = new BigDecimal(value, 2);
            alreadyMoved = BigDecimal.MovePointLeft(movePtLeft, -2);
            assertTrue("move point left -2 failed",
                    alreadyMoved.Scale == movePtLeft.Scale - 2
                            && alreadyMoved.ToString().Equals("12345908"));
        }

        /**
         * @tests java.math.BigDecimal#movePointRight(int)
         */
        [Test]
        public void test_movePointRightI()
        {
            BigDecimal movePtRight = BigDecimal.Parse("-1.58796521458");
            BigDecimal alreadyMoved = BigDecimal.MovePointRight(movePtRight, 8);
            assertTrue("move point right 8 failed", alreadyMoved.Scale == 3
                    && alreadyMoved.ToString().Equals("-158796521.458"));
            movePtRight = new BigDecimal(value, 2);
            alreadyMoved = BigDecimal.MovePointRight(movePtRight, 4);
            assertTrue("move point right 4 failed", alreadyMoved.Scale == 0
                    && alreadyMoved.ToString().Equals("1234590800"));
            movePtRight = new BigDecimal(134E12);
            alreadyMoved = BigDecimal.MovePointRight(movePtRight, 2);
            assertTrue("move point right 2 failed", alreadyMoved.Scale == 0
                    && alreadyMoved.ToString().Equals("13400000000000000"));
            movePtRight = new BigDecimal(-3.4E-10);
            alreadyMoved = BigDecimal.MovePointRight(movePtRight, 5);
            assertTrue("move point right 5 failed",
                    alreadyMoved.Scale == movePtRight.Scale - 5
                            && alreadyMoved.ToDouble() == -0.000034);
            alreadyMoved = BigDecimal.MovePointRight(alreadyMoved, -5);
            assertTrue("move point right -5 failed", alreadyMoved
                    .Equals(movePtRight));
        }

        /**
         * @tests java.math.BigDecimal#multiply(java.math.BigDecimal)
         */
        [Test]
        public void test_multiplyLjava_math_BigDecimal()
        {
            BigDecimal multi1 = new BigDecimal(value, 5);
            BigDecimal multi2 = new BigDecimal(2.345D);
            BigDecimal result = multi1 * multi2;
            assertTrue("123.45908 * 2.345 is not correct: " + result, result
                    .ToString().StartsWith("289.51154260", StringComparison.Ordinal)
                    && result.Scale == multi1.Scale + multi2.Scale);
            multi1 = BigDecimal.Parse("34656");
            multi2 = BigDecimal.Parse("-2");
            result = multi1 * multi2;
            assertTrue("34656 * 2 is not correct", result.ToString().Equals(
                    "-69312")
                    && result.Scale == 0);
            multi1 = new BigDecimal(-2.345E-02);
            multi2 = new BigDecimal(-134E130);
            result = multi1 * multi2;
            assertTrue("-2.345E-02 * -134E130 is not correct " + result.ToDouble(),
                    result.ToDouble() == 3.1422999999999997E130
                            && result.Scale == multi1.Scale + multi2.Scale);
            multi1 = BigDecimal.Parse("11235");
            multi2 = BigDecimal.Parse("0");
            result = multi1 * multi2;
            assertTrue("11235 * 0 is not correct", result.ToDouble() == 0
                    && result.Scale == 0);
            multi1 = BigDecimal.Parse("-0.00234");
            multi2 = new BigDecimal(13.4E10);
            result = multi1 * multi2;
            assertTrue("-0.00234 * 13.4E10 is not correct",
                    result.ToDouble() == -313560000
                            && result.Scale == multi1.Scale + multi2.Scale);
        }

        /**
         * @tests java.math.BigDecimal#negate()
         */
        [Test]
        public void test_negate()
        {
            BigDecimal negate1 = new BigDecimal(value2, 7);
            assertTrue("the negate of 1233.4560000 is not -1233.4560000", (-negate1)
                    .ToString().Equals("-1233.4560000"));
            negate1 = BigDecimal.Parse("-23465839");
            assertTrue("the negate of -23465839 is not 23465839", (-negate1)
                    .ToString().Equals("23465839"));
            negate1 = new BigDecimal(-3.456E6);
            assertTrue("the negate of -3.456E6 is not 3.456E6",(-(-negate1))
                    .Equals(negate1));
        }

        /**
         * @tests java.math.BigDecimal#scale()
         */
        [Test]
        public void test_scale()
        {
            BigDecimal scale1 = new BigDecimal(value2, 8);
            assertTrue("the scale of the number 123.34560000 is wrong", scale1
                    .Scale == 8);
            BigDecimal scale2 = BigDecimal.Parse("29389.");
            assertTrue("the scale of the number 29389. is wrong",
                    scale2.Scale == 0);
            BigDecimal scale3 = new BigDecimal(3.374E13);
            assertTrue("the scale of the number 3.374E13 is wrong",
                    scale3.Scale == 0);
            BigDecimal scale4 = BigDecimal.Parse("-3.45E-203");
            // note the scale is calculated as 15 digits of 345000.... + exponent -
            // 1. -1 for the 3
            assertTrue("the scale of the number -3.45E-203 is wrong: "
                    + scale4.Scale, scale4.Scale == 205);
            scale4 = BigDecimal.Parse("-345.4E-200");
            assertTrue("the scale of the number -345.4E-200 is wrong", scale4
                    .Scale == 201);
        }

        /**
         * @tests java.math.BigDecimal#setScale(int)
         */
        [Test]
        public void test_setScaleI()
        {
            // rounding mode defaults to zero
            BigDecimal setScale1 = new BigDecimal(value, 3);
            BigDecimal setScale2 = BigDecimal.SetScale(setScale1, 5);
            BigInteger setresult = BigInteger.Parse("1234590800");
            assertTrue("the number 12345.908 after setting scale is wrong",
                    setScale2.UnscaledValue.Equals(setresult)
                            && setScale2.Scale == 5);

            try
            {
                setScale2 = BigDecimal.SetScale(setScale1, 2, RoundingMode.Unnecessary);
                fail("arithmetic Exception not caught as a result of loosing precision");
            }
            catch (ArithmeticException e)
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#setScale(int, int)
         */
        [Test]
        public void test_setScaleII()
        {
            BigDecimal setScale1 = new BigDecimal(2.323E102);
            BigDecimal setScale2 = BigDecimal.SetScale(setScale1, 4);
            assertTrue("the number 2.323E102 after setting scale is wrong",
                    setScale2.Scale == 4);
            assertTrue("the representation of the number 2.323E102 is wrong",
                    setScale2.ToDouble() == 2.323E102);
            setScale1 = BigDecimal.Parse("-1.253E-12");
            setScale2 = BigDecimal.SetScale(setScale1, 17, RoundingMode.Ceiling);
            assertTrue("the number -1.253E-12 after setting scale is wrong",
                    setScale2.Scale == 17);
            assertTrue(
                    "the representation of the number -1.253E-12 after setting scale is wrong, " + setScale2.ToString(),
                    setScale2.ToString().Equals("-1.25300E-12"));

            // testing rounding Mode ROUND_CEILING
            setScale1 = new BigDecimal(value, 4);
            setScale2 = BigDecimal.SetScale(setScale1, 1, RoundingMode.Ceiling);
            assertTrue(
                    "the number 1234.5908 after setting scale to 1/ROUND_CEILING is wrong",
                    setScale2.ToString().Equals("1234.6") && setScale2.Scale == 1);
            BigDecimal setNeg = new BigDecimal(-value, 4);
            setScale2 = BigDecimal.SetScale(setNeg, 1, RoundingMode.Ceiling);
            assertTrue(
                    "the number -1234.5908 after setting scale to 1/ROUND_CEILING is wrong",
                    setScale2.ToString().Equals("-1234.5")
                            && setScale2.Scale == 1);

            // testing rounding Mode ROUND_DOWN
            setScale2 = BigDecimal.SetScale(setNeg, 1, RoundingMode.Down);
            assertTrue(
                    "the number -1234.5908 after setting scale to 1/ROUND_DOWN is wrong",
                    setScale2.ToString().Equals("-1234.5")
                            && setScale2.Scale == 1);
            setScale1 = new BigDecimal(value, 4);
            setScale2 = BigDecimal.SetScale(setScale1, 1, RoundingMode.Down);
            assertTrue(
                    "the number 1234.5908 after setting scale to 1/ROUND_DOWN is wrong",
                    setScale2.ToString().Equals("1234.5") && setScale2.Scale == 1);

            // testing rounding Mode ROUND_FLOOR
            setScale2 = BigDecimal.SetScale(setScale1, 1, RoundingMode.Floor);
            assertTrue(
                    "the number 1234.5908 after setting scale to 1/ROUND_FLOOR is wrong",
                    setScale2.ToString().Equals("1234.5") && setScale2.Scale == 1);
            setScale2 = BigDecimal.SetScale(setNeg, 1, RoundingMode.Floor);
            assertTrue(
                    "the number -1234.5908 after setting scale to 1/ROUND_FLOOR is wrong",
                    setScale2.ToString().Equals("-1234.6")
                            && setScale2.Scale == 1);

            // testing rounding Mode ROUND_HALF_DOWN
            setScale2 = BigDecimal.SetScale(setScale1, 3, RoundingMode.HalfDown);
            assertTrue(
                    "the number 1234.5908 after setting scale to 3/ROUND_HALF_DOWN is wrong",
                    setScale2.ToString().Equals("1234.591")
                            && setScale2.Scale == 3);
            setScale1 = new BigDecimal(BigInteger.Parse("12345000"), 5);
            setScale2 = BigDecimal.SetScale(setScale1, 1, RoundingMode.HalfDown);
            assertTrue(
                    "the number 123.45908 after setting scale to 1/ROUND_HALF_DOWN is wrong",
                    setScale2.ToString().Equals("123.4") && setScale2.Scale == 1);
            setScale2 = BigDecimal.SetScale(BigDecimal.Parse("-1234.5000"), 0,
                    RoundingMode.HalfDown);
            assertTrue(
                    "the number -1234.5908 after setting scale to 0/ROUND_HALF_DOWN is wrong",
                    setScale2.ToString().Equals("-1234") && setScale2.Scale == 0);

            // testing rounding Mode ROUND_HALF_EVEN
            setScale1 = new BigDecimal(1.2345789D);
            setScale2 = BigDecimal.SetScale(setScale1, 4, RoundingMode.HalfEven);
            assertTrue(
                    "the number 1.2345789 after setting scale to 4/ROUND_HALF_EVEN is wrong",
                    setScale2.ToDouble() == 1.2346D && setScale2.Scale == 4);
            setNeg = new BigDecimal(-1.2335789D);
            setScale2 = BigDecimal.SetScale(setNeg, 2, RoundingMode.HalfEven);
            assertTrue(
                    "the number -1.2335789 after setting scale to 2/ROUND_HALF_EVEN is wrong",
                    setScale2.ToDouble() == -1.23D && setScale2.Scale == 2);
            setScale2 = BigDecimal.SetScale(BigDecimal.Parse("1.2345000"), 3,
                    RoundingMode.HalfEven);
            assertTrue(
                    "the number 1.2345789 after setting scale to 3/ROUND_HALF_EVEN is wrong",
                    setScale2.ToDouble() == 1.234D && setScale2.Scale == 3);
            setScale2 = BigDecimal.SetScale(BigDecimal.Parse("-1.2345000"), 3,
                    RoundingMode.HalfEven);
            assertTrue(
                    "the number -1.2335789 after setting scale to 3/ROUND_HALF_EVEN is wrong",
                    setScale2.ToDouble() == -1.234D && setScale2.Scale == 3);

            // testing rounding Mode ROUND_HALF_UP
            setScale1 = BigDecimal.Parse("134567.34650");
            setScale2 = BigDecimal.SetScale(setScale1, 3, RoundingMode.HalfUp);
            assertTrue(
                    "the number 134567.34658 after setting scale to 3/ROUND_HALF_UP is wrong",
                    setScale2.ToString().Equals("134567.347")
                            && setScale2.Scale == 3);
            setNeg = BigDecimal.Parse("-1234.4567");
            setScale2 = BigDecimal.SetScale(setNeg, 0, RoundingMode.HalfUp);
            assertTrue(
                    "the number -1234.4567 after setting scale to 0/ROUND_HALF_UP is wrong",
                    setScale2.ToString().Equals("-1234") && setScale2.Scale == 0);

            // testing rounding Mode ROUND_UNNECESSARY
            try
            {
                BigDecimal.SetScale(setScale1, 3, RoundingMode.Unnecessary);
                fail("arithmetic Exception not caught for round unnecessary");
            }
            catch (ArithmeticException e)
            {
            }

            // testing rounding Mode ROUND_UP
            setScale1 = BigDecimal.Parse("100000.374");
            setScale2 = BigDecimal.SetScale(setScale1, 2, RoundingMode.Up);
            assertTrue(
                    "the number 100000.374 after setting scale to 2/ROUND_UP is wrong",
                    setScale2.ToString().Equals("100000.38")
                            && setScale2.Scale == 2);
            setNeg = new BigDecimal(-134.34589D);
            setScale2 = BigDecimal.SetScale(setNeg, 2, RoundingMode.Up);
            assertTrue(
                    "the number -134.34589 after setting scale to 2/ROUND_UP is wrong",
                    setScale2.ToDouble() == -134.35D && setScale2.Scale == 2);

            // testing invalid rounding modes
            try
            {
                setScale2 = BigDecimal.SetScale(setScale1, 0, (RoundingMode)(-123));
                fail("IllegalArgumentException is not caught for wrong rounding mode");
            }
            catch (ArgumentException e)
            {
            }
        }

        /**
         * @tests java.math.BigDecimal#signum()
         */
        [Test]
        public void test_signum()
        {
            BigDecimal sign = new BigDecimal(123E-104);
            assertTrue("123E-104 is not positive in signum()", sign.Sign == 1);
            sign = BigDecimal.Parse("-1234.3959");
            assertTrue("-1234.3959 is not negative in signum()",
                    sign.Sign == -1);
            sign = new BigDecimal(000D);
            assertTrue("000D is not zero in signum()", sign.Sign == 0);
        }

        /**
         * @tests java.math.BigDecimal#subtract(java.math.BigDecimal)
         */
        public void test_subtractLjava_math_BigDecimal()
        {
            BigDecimal sub1 = BigDecimal.Parse("13948");
            BigDecimal sub2 = BigDecimal.Parse("2839.489");
            BigDecimal result = sub1 - sub2;
            assertTrue("13948 - 2839.489 is wrong: " + result, result.ToString()
                    .Equals("11108.511")
                    && result.Scale == 3);
            BigDecimal result2 = sub2 - sub1;
            assertTrue("2839.489 - 13948 is wrong", result2.ToString().Equals(
                    "-11108.511")
                    && result2.Scale == 3);
            assertTrue("13948 - 2839.489 is not the negative of 2839.489 - 13948",
                    result.Equals(-result2));
            sub1 = new BigDecimal(value, 1);
            sub2 = BigDecimal.Parse("0");
            result = sub1 - sub2;
            assertTrue("1234590.8 - 0 is wrong", result.Equals(sub1));
            sub1 = new BigDecimal(1.234E-03);
            sub2 = new BigDecimal(3.423E-10);
            result = sub1 - sub2;
            assertTrue("1.234E-03 - 3.423E-10 is wrong, " + result.ToDouble(),
                    result.ToDouble() == 0.0012339996577);
            sub1 = new BigDecimal(1234.0123);
            sub2 = new BigDecimal(1234.0123000);
            result = sub1 - sub2;
            assertTrue("1234.0123 - 1234.0123000 is wrong, " + result.ToDouble(),
                    result.ToDouble() == 0.0);
        }

        /**
         * @tests java.math.BigDecimal#toBigInteger()
         */
        [Test]
        public void test_toBigInteger()
        {
            BigDecimal sub1 = BigDecimal.Parse("-29830.989");
            BigInteger result = sub1.ToBigInteger();

            assertTrue("the bigInteger equivalent of -29830.989 is wrong", result
                    .ToString().Equals("-29830"));
            sub1 = new BigDecimal(-2837E10);
            result = sub1.ToBigInteger();
            assertTrue("the bigInteger equivalent of -2837E10 is wrong", result
                    .ToDouble() == -2837E10);
            sub1 = new BigDecimal(2.349E-10);
            result = sub1.ToBigInteger();
            assertTrue("the bigInteger equivalent of 2.349E-10 is wrong", result
                    .Equals(BigInteger.Zero));
            sub1 = new BigDecimal(value2, 6);
            result = sub1.ToBigInteger();
            assertTrue("the bigInteger equivalent of 12334.560000 is wrong", result
                    .ToString().Equals("12334"));
        }

        /**
         * @tests java.math.BigDecimal#toString()
         */
        [Test]
        public void test_toString()
        {
            BigDecimal toString1 = BigDecimal.Parse("1234.000");
            assertTrue("the toString representation of 1234.000 is wrong",
                    toString1.ToString().Equals("1234.000"));
            toString1 = BigDecimal.Parse("-123.4E-5");
            assertTrue("the toString representation of -123.4E-5 is wrong: "
                    + toString1, toString1.ToString().Equals("-0.001234"));
            toString1 = BigDecimal.Parse("-1.455E-20");
            assertTrue("the toString representation of -1.455E-20 is wrong",
                    toString1.ToString().Equals("-1.455E-20"));
            toString1 = new BigDecimal(value2, 4);
            assertTrue("the toString representation of 1233456.0000 is wrong",
                    toString1.ToString().Equals("1233456.0000"));
        }

        /**
         * @tests java.math.BigDecimal#unscaledValue()
         */
        [Test]
        public void test_unscaledValue()
        {
            BigDecimal unsVal = BigDecimal.Parse("-2839485.000");
            assertTrue("the unscaledValue of -2839485.000 is wrong", unsVal
                    .UnscaledValue.ToString().Equals("-2839485000"));
            unsVal = new BigDecimal(123E10);
            assertTrue("the unscaledValue of 123E10 is wrong", unsVal
                    .UnscaledValue.ToString().Equals("1230000000000"));
            unsVal = BigDecimal.Parse("-4.56E-13");
            assertTrue("the unscaledValue of -4.56E-13 is wrong: "
                    + unsVal.UnscaledValue, unsVal.UnscaledValue.ToString()
                    .Equals("-456"));
            unsVal = new BigDecimal(value, 3);
            assertTrue("the unscaledValue of 12345.908 is wrong", unsVal
                    .UnscaledValue.ToString().Equals("12345908"));

        }

        /**
         * @tests java.math.BigDecimal#valueOf(long)
         */
        [Test]
        public void test_valueOfJ()
        {
            BigDecimal valueOfL = BigDecimal.Create(9223372036854775806L);
            assertTrue("the bigDecimal equivalent of 9223372036854775806 is wrong",
                    valueOfL.UnscaledValue.ToString().Equals(
                            "9223372036854775806")
                            && valueOfL.Scale == 0);
            assertTrue(
                    "the toString representation of 9223372036854775806 is wrong",
                    valueOfL.ToString().Equals("9223372036854775806"));
            valueOfL = BigDecimal.Create(0L);
            assertTrue("the bigDecimal equivalent of 0 is wrong", valueOfL
                    .UnscaledValue.ToString().Equals("0")
                    && valueOfL.Scale == 0);
        }

        /**
         * @tests java.math.BigDecimal#valueOf(long, int)
         */
        [Test]
        public void test_valueOfJI()
        {
            BigDecimal valueOfJI = BigDecimal.Create(9223372036854775806L, 5);
            assertTrue(
                    "the bigDecimal equivalent of 92233720368547.75806 is wrong",
                    valueOfJI.UnscaledValue.ToString().Equals(
                            "9223372036854775806")
                            && valueOfJI.Scale == 5);
            assertTrue(
                    "the toString representation of 9223372036854775806 is wrong",
                    valueOfJI.ToString().Equals("92233720368547.75806"));
            valueOfJI = BigDecimal.Create(1234L, 8);
            assertTrue(
                    "the bigDecimal equivalent of 92233720368547.75806 is wrong",
                    valueOfJI.UnscaledValue.ToString().Equals("1234")
                            && valueOfJI.Scale == 8);
            assertTrue(
                    "the toString representation of 9223372036854775806 is wrong",
                    valueOfJI.ToString().Equals("0.00001234"));
            valueOfJI = BigDecimal.Create(0, 3);
            assertTrue(
                    "the bigDecimal equivalent of 92233720368547.75806 is wrong",
                    valueOfJI.UnscaledValue.ToString().Equals("0")
                            && valueOfJI.Scale == 3);
            assertTrue(
                    "the toString representation of 9223372036854775806 is wrong",
                    valueOfJI.ToString().Equals("0.000"));

        }

        // ICU4N TODO: Serialization
        //        [Test]
        //        public void test_BigDecimal_serialization()// throws Exception
        //    {
        //        // Regression for HARMONY-1896
        //        char[] @in = new char[] { '1', '5', '6', '7', '8', '7', '.', '0', '0' };
        //        BigDecimal bd = BigDecimal.Parse(@in, 0, 9);

        //    ByteArrayOutputStream bos = new ByteArrayOutputStream();
        //    ObjectOutputStream oos = new ObjectOutputStream(bos);
        //    oos.writeObject(bd);

        //        ByteArrayInputStream bis = new ByteArrayInputStream(bos.toByteArray());
        //    ObjectInputStream ois = new ObjectInputStream(bis);
        //    BigDecimal nbd = (BigDecimal)ois.readObject();

        //    assertEquals(bd.intValue(), nbd.intValue());
        //    assertEquals(bd.doubleValue(), nbd.doubleValue(), 0.0);
        //    assertEquals(bd.toString(), nbd.toString());
        //}

        /**
         * @tests java.math.BigDecimal#stripTrailingZero(long)
         */
        [Test]
        public void test_stripTrailingZero()
        {
            BigDecimal sixhundredtest = BigDecimal.Parse("600.0");
            assertTrue("stripTrailingZero failed for 600.0",
                    ((BigDecimal.StripTrailingZeros(sixhundredtest)).Scale == -2)
                    );

            /* Single digit, no trailing zero, odd number */
            BigDecimal notrailingzerotest = BigDecimal.Parse("1");
            assertTrue("stripTrailingZero failed for 1",
                    ((BigDecimal.StripTrailingZeros(notrailingzerotest)).Scale == 0)
                    );

            /* Zero */
            //regression for HARMONY-4623, NON-BUG DIFF with RI
            BigDecimal zerotest = BigDecimal.Parse("0.0000");
            assertTrue("stripTrailingZero failed for 0.0000",
                    ((BigDecimal.StripTrailingZeros(zerotest)).Scale == 0)
                    );
        }

        [Test]
        public void testMathContextConstruction()
        {
            String a = "-12380945E+61";
            BigDecimal aNumber = BigDecimal.Parse(a);
            int precision = 6;
            RoundingMode rm = RoundingMode.HalfDown;
            MathContext mcIntRm = new MathContext(precision, rm);
            MathContext mcStr = MathContext.Parse("precision=6 roundingMode=HalfDown"); // ICU4N TODO: Need to figure out how to deal with these
            MathContext mcInt = new MathContext(precision);
            BigDecimal res = BigDecimal.Abs(aNumber, mcInt);
            assertEquals("MathContext Constructer with int precision failed",
                    res,
                    BigDecimal.Parse("1.23809E+68"));

            assertEquals("Equal MathContexts are not Equal ",
                    mcIntRm,
                    mcStr);

            assertEquals("Different MathContext are reported as Equal ",
                    mcInt.Equals(mcStr),
                    false);

            assertEquals("Equal MathContexts have different hashcodes ",
                    mcIntRm.GetHashCode(),
                    mcStr.GetHashCode());

            assertEquals("MathContext.toString() returning incorrect value",
                    mcIntRm.ToString(),
                    "precision=6 roundingMode=HalfDown");
        }
    }
}
