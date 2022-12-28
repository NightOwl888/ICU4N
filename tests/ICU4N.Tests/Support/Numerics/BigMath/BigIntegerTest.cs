using ICU4N.Dev.Test;
using J2N;
using NUnit.Framework;
using System;

namespace ICU4N.Numerics.BigMath
{
    public class BigIntegerTest : TestFmwk
    {
        BigInteger minusTwo;

        BigInteger minusOne;

        BigInteger zero;

        BigInteger one;

        BigInteger two;

        BigInteger ten;

        BigInteger sixteen;

        BigInteger oneThousand;

        BigInteger aZillion;

        BigInteger twoToTheTen;

        BigInteger twoToTheSeventy;

        Random rand = new Randomizer();

        BigInteger bi;

        BigInteger bi1;

        BigInteger bi2;

        BigInteger bi3;

        BigInteger bi11;

        BigInteger bi22;

        BigInteger bi33;

        BigInteger bi12;

        BigInteger bi23;

        BigInteger bi13;

        BigInteger largePos;

        BigInteger smallPos;

        BigInteger largeNeg;

        BigInteger smallNeg;

        BigInteger[][] boolPairs;

        public BigIntegerTest()
        {
            minusTwo = BigInteger.Parse("-2", 10);

            minusOne = BigInteger.Parse("-1", 10);

            zero = BigInteger.Parse("0", 10);

            one = BigInteger.Parse("1", 10);

            two = BigInteger.Parse("2", 10);

            ten = BigInteger.Parse("10", 10);

            sixteen = BigInteger.Parse("16", 10);

            oneThousand = BigInteger.Parse("1000", 10);

            aZillion = BigInteger.Parse(
                    "100000000000000000000000000000000000000000000000000", 10);

            twoToTheTen = BigInteger.Parse("1024", 10);

            twoToTheSeventy = BigInteger.Pow(two, 70);
        }

        /**
         * @tests java.math.BigInteger#BigInteger(int, java.util.Random)
         */
        [Test]
        public void test_ConstructorILjava_util_Random()
        {
            // regression test for HARMONY-1047
            try
            {
                new BigInteger(int.MaxValue, (Random)null);
                fail("NegativeArraySizeException expected");
            }
            catch (OverflowException e) // .NET thows this exception
            {
                // PASSED
            }

            bi = new BigInteger(70, rand);
            bi2 = new BigInteger(70, rand);
            assertTrue("Random number is negative", bi.CompareTo(zero) >= 0);
            assertTrue("Random number is too big",
                    bi.CompareTo(twoToTheSeventy) < 0);
            assertTrue(
                    "Two random numbers in a row are the same (might not be a bug but it very likely is)",
                    !bi.Equals(bi2));
            assertTrue("Not zero", new BigInteger(0, rand).Equals(BigInteger.Zero));
        }

        /**
         * @tests java.math.BigInteger#BigInteger(int, int, java.util.Random)
         */
        [Test]
        public void test_ConstructorIILjava_util_Random()
        {
            bi = new BigInteger(10, 5, rand);
            bi2 = new BigInteger(10, 5, rand);
            assertTrue("Random number one is negative", bi.CompareTo(zero) >= 0);
            assertTrue("Random number one is too big",
                    bi.CompareTo(twoToTheTen) < 0);
            assertTrue("Random number two is negative", bi2.CompareTo(zero) >= 0);
            assertTrue("Random number two is too big",
                    bi2.CompareTo(twoToTheTen) < 0);

            {
                Random rand = new Random();
                BigInteger bi;
                int[] certainty = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                int.MinValue, int.MinValue + 1, -2, -1 };
                for (int i = 2; i <= 20; i++)
                {
                    for (int c = 0; c < certainty.Length; c++)
                    {
                        bi = new BigInteger(i, c, rand); // Create BigInteger
                        assertTrue("Bit length incorrect", bi.BitLength == i);
                    }
                }
            }
        }

        /**
         * @tests java.math.BigInteger#BigInteger(byte[])
         */
        [Test]
        public void test_Constructor_B()
        {
            byte[] myByteArray;
            myByteArray = new byte[] { (byte)0x00, (byte)0xFF, (byte)0xFE };
            bi = new BigInteger(myByteArray);
            assertTrue("Incorrect value for pos number", bi.Equals(BigInteger.SetBit(BigInteger.Zero
                    , 16) - two));
            myByteArray = new byte[] { (byte)0xFF, (byte)0xFE };
            bi = new BigInteger(myByteArray);
            assertTrue("Incorrect value for neg number", bi.Equals(minusTwo));
        }

        /**
         * @tests java.math.BigInteger#BigInteger(int, byte[])
         */
        [Test]
        public void test_ConstructorI_B()
        {
            byte[] myByteArray;
            myByteArray = new byte[] { (byte)0xFF, (byte)0xFE };
            bi = new BigInteger(1, myByteArray);
            assertTrue("Incorrect value for pos number", bi.Equals((BigInteger.SetBit(BigInteger.Zero,
                    16) - two)));
            bi = new BigInteger(-1, myByteArray);
            assertTrue("Incorrect value for neg number", bi.Equals(-(BigInteger.SetBit(BigInteger.Zero,
                    16) - two)));
            myByteArray = new byte[] { (byte)0, (byte)0 };
            bi = new BigInteger(0, myByteArray);
            assertTrue("Incorrect value for zero", bi.Equals(zero));
            myByteArray = new byte[] { (byte)1 };
            try
            {
                new BigInteger(0, myByteArray);
                fail("Failed to throw NumberFormatException");
            }
            catch (FormatException e) // ICU4N TODO: This exception seems like the wrong type
            {
                // correct
            }
        }

        /**
         * @tests java.math.BigInteger#BigInteger(java.lang.String)
         */
        [Test]
        public void test_constructor_String_empty()
        {
            try
            {
                BigInteger.Parse("");
                fail("Expected NumberFormatException for new BigInteger(\"\")");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * @tests java.math.BigInteger#toByteArray()
         */
        [Test]
        public void test_toByteArray()
        {
            byte[] myByteArray, anotherByteArray;
            myByteArray = new byte[] { 97, 33, 120, 124, 50, 2, 0, 0, 0, 12, 124,
                42 };
            anotherByteArray = new BigInteger(myByteArray).ToByteArray();
            assertTrue("Incorrect byte array returned",
                    myByteArray.Length == anotherByteArray.Length);
            for (int counter = myByteArray.Length - 1; counter >= 0; counter--)
            {
                assertTrue("Incorrect values in returned byte array",
                        myByteArray[counter] == anotherByteArray[counter]);
            }
        }

        /**
         * @tests java.math.BigInteger#isProbablePrime(int)
         */
        [Test]
        public void test_isProbablePrimeI()
        {
            int fails = 0;
            bi = new BigInteger(20, 20, rand);
            if (!BigInteger.IsProbablePrime(bi, 17))
            {
                fails++;
            }
            bi = BigInteger.Parse("4", 10);
            if (BigInteger.IsProbablePrime(bi, 17))
            {
                fail("isProbablePrime failed for: " + bi);
            }
            bi = BigInteger.FromInt64(17L * 13L);
            if (BigInteger.IsProbablePrime(bi, 17))
            {
                fail("isProbablePrime failed for: " + bi);
            }
            for (long a = 2; a < 1000; a++)
            {
                if (isPrime(a))
                {
                    assertTrue("false negative on prime number <1000", BigInteger.IsProbablePrime(BigInteger
                            .FromInt64(a), 5));
                }
                else if (BigInteger.IsProbablePrime(BigInteger.FromInt64(a), 17))
                {
                    Console.Out.WriteLine("isProbablePrime failed for: " + a);
                    fails++;
                }
            }
            for (int a = 0; a < 1000; a++)
            {
                bi = BigInteger.FromInt64(rand.Next(1000000)) * (
                        BigInteger.FromInt64(rand.Next(1000000)));
                if (BigInteger.IsProbablePrime(bi, 17))
                {
                    Console.Out.WriteLine("isProbablePrime failed for: " + bi);
                    fails++;
                }
            }
            for (int a = 0; a < 200; a++)
            {
                bi = new BigInteger(70, rand) * (new BigInteger(70, rand));
                if (BigInteger.IsProbablePrime(bi, 17))
                {
                    Console.Out.WriteLine("isProbablePrime failed for: " + bi);
                    fails++;
                }
            }
            assertTrue("Too many false positives - may indicate a problem",
                    fails <= 1);
        }

        /**
         * @tests java.math.BigInteger#equals(java.lang.Object)
         */
        [Test]
        public void test_equalsLjava_lang_Object()
        {
            assertTrue("0=0", zero.Equals(BigInteger.FromInt64(0)));
            assertTrue("-123=-123", BigInteger.FromInt64(-123).Equals(
                    BigInteger.FromInt64(-123)));
            assertTrue("0=1", !zero.Equals(one));
            assertTrue("0=-1", !zero.Equals(minusOne));
            assertTrue("1=-1", !one.Equals(minusOne));
            assertTrue("bi3=bi3", bi3.Equals(bi3));
            assertTrue("bi3=copy of bi3", bi3.Equals(-(-bi3)));
            assertTrue("bi3=bi2", !bi3.Equals(bi2));
        }

        /**
         * @tests java.math.BigInteger#compareTo(java.math.BigInteger)
         */
        [Test]
        public void test_compareToLjava_math_BigInteger()
        {
            assertTrue("Smaller number returned >= 0", one.CompareTo(two) < 0);
            assertTrue("Larger number returned >= 0", two.CompareTo(one) > 0);
            assertTrue("Equal numbers did not return 0", one.CompareTo(one) == 0);
            assertTrue("Neg number messed things up",
                    (-two).CompareTo(one) < 0);
        }

        /**
         * @tests java.math.BigInteger#intValue()
         */
        [Test]
        public void test_intValue()
        {
            assertTrue("Incorrect intValue for 2**70",
                    twoToTheSeventy.ToInt32() == 0);
            assertTrue("Incorrect intValue for 2", two.ToInt32() == 2);
        }

        /**
         * @tests java.math.BigInteger#longValue()
         */
        [Test]
        public void test_longValue()
        {
            assertTrue("Incorrect longValue for 2**70",
                    twoToTheSeventy.ToInt64() == 0);
            assertTrue("Incorrect longValue for 2", two.ToInt64() == 2);
        }

        /**
         * @tests java.math.BigInteger#valueOf(long)
         */
        [Test]
        public void test_valueOfJ()
        {
            assertTrue("Incurred number returned for 2", BigInteger.FromInt64(2L)
                    .Equals(two));
            assertTrue("Incurred number returned for 200", BigInteger.FromInt64(200L)
                    .Equals(BigInteger.FromInt64(139) + (BigInteger.FromInt64(61))));
        }

        /**
         * @tests java.math.BigInteger#add(java.math.BigInteger)
         */
        [Test]
        public void test_addLjava_math_BigInteger()
        {
            assertTrue("Incorrect sum--wanted a zillion", (aZillion + (aZillion)
                    + (-aZillion)).Equals(aZillion));
            assertTrue("0+0", (zero + zero).Equals(zero));
            assertTrue("0+1", (zero + one).Equals(one));
            assertTrue("1+0", (one + zero).Equals(one));
            assertTrue("1+1", (one + one).Equals(two));
            assertTrue("0+(-1)", (zero + minusOne).Equals(minusOne));
            assertTrue("(-1)+0", (minusOne + zero).Equals(minusOne));
            assertTrue("(-1)+(-1)", (minusOne + minusOne).Equals(minusTwo));
            assertTrue("1+(-1)", (one + minusOne).Equals(zero));
            assertTrue("(-1)+1", (minusOne + one).Equals(zero));

            for (int i = 0; i < 200; i++)
            {
                BigInteger midbit = BigInteger.SetBit(zero, i);
                assertTrue("add fails to carry on bit " + i, (midbit + midbit)
                        .Equals(BigInteger.SetBit(zero, i + 1)));
            }
            BigInteger bi2p3 = bi2 + (bi3);
            BigInteger bi3p2 = bi3 + (bi2);
            assertTrue("bi2p3=bi3p2", bi2p3.Equals(bi3p2));

            // add large positive + small positive

            // add large positive + small negative

            // add large negative + small positive

            // add large negative + small negative
        }

        /**
         * @tests java.math.BigInteger#negate()
         */
        [Test]
        public void test_negate()
        {
            assertTrue("Single negation of zero did not result in zero", (-zero
                    ).Equals(zero));
            assertTrue("Single negation resulted in original nonzero number",
                    !(-aZillion).Equals(aZillion));
            assertTrue("Double negation did not result in original number",
                    (-(-aZillion)).Equals(aZillion));

            assertTrue("0.neg", (-zero).Equals(zero));
            assertTrue("1.neg", (-one).Equals(minusOne));
            assertTrue("2.neg", (-two).Equals(minusTwo));
            assertTrue("-1.neg", (-minusOne).Equals(one));
            assertTrue("-2.neg", (-minusTwo).Equals(two));
            assertTrue("0x62EB40FEF85AA9EBL*2.neg", (-BigInteger.FromInt64(
                    unchecked(0x62EB40FEF85AA9EBL * 2))).Equals(
                    BigInteger.FromInt64(unchecked(-0x62EB40FEF85AA9EBL * 2))));
            for (int i = 0; i < 200; i++)
            {
                BigInteger midbit = BigInteger.SetBit(zero, i);
                BigInteger negate = -midbit;
                assertTrue("negate negate", (-negate).Equals(midbit));
                assertTrue("neg fails on bit " + i, ((-midbit) + midbit)
                        .Equals(zero));
            }
        }

        /**
         * @tests java.math.BigInteger#signum()
         */
        [Test]
        public void test_signum()
        {
            assertTrue("Wrong positive signum", two.Sign == 1);
            assertTrue("Wrong zero signum", zero.Sign == 0);
            assertTrue("Wrong neg zero signum", (-zero).Sign == 0);
            assertTrue("Wrong neg signum", (-two).Sign == -1);
        }

        /**
         * @tests java.math.BigInteger#abs()
         */
        [Test]
        public void test_abs()
        {
            assertTrue("Invalid number returned for zillion", BigInteger.Abs(-aZillion)
                    .Equals(BigInteger.Abs(aZillion)));
            assertTrue("Invalid number returned for zero neg", BigInteger.Abs(-zero)
                    .Equals(zero));
            assertTrue("Invalid number returned for zero", BigInteger.Abs(zero).Equals(zero));
            assertTrue("Invalid number returned for two", BigInteger.Abs(-two)
                    .Equals(two));
        }

        /**
         * @tests java.math.BigInteger#pow(int)
         */
        [Test]
        public void test_powI()
        {
            assertTrue("Incorrect exponent returned for 2**10", BigInteger.Pow(two, 10).Equals(
                    twoToTheTen));
            assertTrue("Incorrect exponent returned for 2**70", (BigInteger.Pow(two, 30)
                    * BigInteger.Pow(two, 40)).Equals(twoToTheSeventy));
            assertTrue("Incorrect exponent returned for 10**50", BigInteger.Pow(ten, 50)
                    .Equals(aZillion));
        }

        /**
         * @tests java.math.BigInteger#modInverse(java.math.BigInteger)
         */
        [Test]
        public void test_modInverseLjava_math_BigInteger()
        {
            BigInteger a = zero, mod, inv;
            for (int j = 3; j < 50; j++)
            {
                mod = BigInteger.FromInt64(j);
                for (int i = -j + 1; i < j; i++)
                {
                    try
                    {
                        a = BigInteger.FromInt64(i);
                        inv = BigInteger.ModInverse(a, mod);
                        assertTrue("bad inverse: " + a + " inv mod " + mod
                                + " equals " + inv, one.Equals(BigInteger.Mod(a * inv,
                                mod)));
                        assertTrue("inverse greater than modulo: " + a
                                + " inv mod " + mod + " equals " + inv, inv
                                .CompareTo(mod) < 0);
                        assertTrue("inverse less than zero: " + a + " inv mod "
                                + mod + " equals " + inv, inv
                                .CompareTo(BigInteger.Zero) >= 0);
                    }
                    catch (ArithmeticException e)
                    {
                        assertTrue("should have found inverse for " + a + " mod "
                                + mod, !one.Equals(BigInteger.Gcd(a, mod)));
                    }
                }
            }
            for (int j = 1; j < 10; j++)
            {
                mod = bi2 + (BigInteger.FromInt64(j));
                for (int i = 0; i < 20; i++)
                {
                    try
                    {
                        a = bi3 + (BigInteger.FromInt64(i));
                        inv = BigInteger.ModInverse(a, mod);
                        assertTrue("bad inverse: " + a + " inv mod " + mod
                                + " equals " + inv, one.Equals(BigInteger.Mod((a * inv),
                                mod)));
                        assertTrue("inverse greater than modulo: " + a
                                + " inv mod " + mod + " equals " + inv, inv
                                .CompareTo(mod) < 0);
                        assertTrue("inverse less than zero: " + a + " inv mod "
                                + mod + " equals " + inv, inv
                                .CompareTo(BigInteger.Zero) >= 0);
                    }
                    catch (ArithmeticException e)
                    {
                        assertTrue("should have found inverse for " + a + " mod "
                                + mod, !one.Equals(BigInteger.Gcd(a, mod)));
                    }
                }
            }
        }

        /**
         * @tests java.math.BigInteger#shiftRight(int)
         */
        [Test]
        public void test_shiftRightI()
        {
            assertTrue("1 >> 0", BigInteger.FromInt64(1) >> (0) == (
                    BigInteger.One));
            assertTrue("1 >> 1", BigInteger.FromInt64(1) >> (1) == (
                    BigInteger.Zero));
            assertTrue("1 >> 63", BigInteger.FromInt64(1) >> (63) == (
                    BigInteger.Zero));
            assertTrue("1 >> 64", BigInteger.FromInt64(1) >> (64) == (
                    BigInteger.Zero));
            assertTrue("1 >> 65", BigInteger.FromInt64(1) >> (65) == (
                    BigInteger.Zero));
            assertTrue("1 >> 1000", BigInteger.FromInt64(1) >> (1000) == (
                    BigInteger.Zero));
            assertTrue("-1 >> 0", BigInteger.FromInt64(-1) >> (0) == (
                    minusOne));
            assertTrue("-1 >> 1", BigInteger.FromInt64(-1) >> (1) == (
                    minusOne));
            assertTrue("-1 >> 63", BigInteger.FromInt64(-1) >> (63) == (
                    minusOne));
            assertTrue("-1 >> 64", BigInteger.FromInt64(-1) >> (64) == (
                    minusOne));
            assertTrue("-1 >> 65", BigInteger.FromInt64(-1) >> (65) == (
                    minusOne));
            assertTrue("-1 >> 1000", BigInteger.FromInt64(-1) >> (1000)
                    == (minusOne));

            BigInteger a = BigInteger.One;
            BigInteger c = bi3;
            BigInteger E = -bi3;
            BigInteger e = E;
            for (int i = 0; i < 200; i++)
            {
                BigInteger b = BigInteger.SetBit(BigInteger.Zero, i);
                assertTrue("a==b", a == (b));
                a = a << (1);
                assertTrue("a non-neg", a.Sign >= 0);

                BigInteger d = bi3 >> (i);
                assertTrue("c==d", c == (d));
                c = c >> (1);
                assertTrue(">>1 == /2", (d / two) == (c));
                assertTrue("c non-neg", c.Sign >= 0);

                BigInteger f = E >> (i);
                assertTrue("e==f", e.Equals(f));
                e = e >> (1);
                assertTrue(">>1 == /2", ((f - one) / two) == (e));
                assertTrue("e negative", e.Sign == -1);

                assertTrue("b >> i", b >> (i) == (one));
                assertTrue("b >> i+1", (b >> (i + 1)) == (zero));
                assertTrue("b >> i-1", (b >> (i - 1)) == (two));
            }
        }

        /**
         * @tests java.math.BigInteger#shiftLeft(int)
         */
        [Test]
        public void test_shiftLeftI()
        {
            assertTrue("1 << 0", one << (0) == (one));
            assertTrue("1 << 1", one << (1) == (two));
            assertTrue("1 << 63", one << (63) == (
                    BigInteger.Parse("8000000000000000", 16)));
            assertTrue("1 << 64", one << (64) == (
                    BigInteger.Parse("10000000000000000", 16)));
            assertTrue("1 << 65", one << (65) == (
                    BigInteger.Parse("20000000000000000", 16)));
            assertTrue("-1 << 0", minusOne << (0) == (minusOne));
            assertTrue("-1 << 1", minusOne << (1) == (minusTwo));
            assertTrue("-1 << 63", minusOne << (63) == (
                    BigInteger.Parse("-9223372036854775808")));
            assertTrue("-1 << 64", minusOne << (64) == (
                    BigInteger.Parse("-18446744073709551616")));
            assertTrue("-1 << 65", minusOne << (65) == (
                    BigInteger.Parse("-36893488147419103232")));

            BigInteger a = bi3;
            BigInteger c = minusOne;
            for (int i = 0; i < 200; i++)
            {
                BigInteger b = bi3 << (i);
                assertTrue("a==b", a == (b));
                assertTrue("a >> i == bi3", a >> (i) == (bi3));
                a = a << (1);
                assertTrue("<<1 == *2", b * (two) == (a));
                assertTrue("a non-neg", a.Sign >= 0);
                assertTrue("a.bitCount==b.bitCount", a.BitCount == b.BitCount);

                BigInteger d = minusOne << (i);
                assertTrue("c==d", c == (d));
                c = c << (1);
                assertTrue("<<1 == *2 negative", d * (two) == (c));
                assertTrue("c negative", c.Sign == -1);
                assertTrue("d >> i == minusOne", d >> (i) == (minusOne));
            }
        }

        /**
         * @tests java.math.BigInteger#multiply(java.math.BigInteger)
         */
        [Test]
        public void test_multiplyLjava_math_BigInteger()
        {
            assertTrue("Incorrect sum--wanted three zillion", aZillion
                    + (aZillion) + (aZillion) == (
                            aZillion * (BigInteger.Parse("3", 10))));

            assertTrue("0*0", zero * (zero) == (zero));
            assertTrue("0*1", zero * (one) == (zero));
            assertTrue("1*0", one * (zero) == (zero));
            assertTrue("1*1", one * (one) == (one));
            assertTrue("0*(-1)", zero * (minusOne) == (zero));
            assertTrue("(-1)*0", minusOne * (zero) == (zero));
            assertTrue("(-1)*(-1)", minusOne * (minusOne) == (one));
            assertTrue("1*(-1)", one * (minusOne) == (minusOne));
            assertTrue("(-1)*1", minusOne * (one) == (minusOne));

            testAllMults(bi1, bi1, bi11);
            testAllMults(bi2, bi2, bi22);
            testAllMults(bi3, bi3, bi33);
            testAllMults(bi1, bi2, bi12);
            testAllMults(bi1, bi3, bi13);
            testAllMults(bi2, bi3, bi23);
        }

        /**
         * @tests java.math.BigInteger#divide(java.math.BigInteger)
         */
        [Test]
        public void test_divideLjava_math_BigInteger()
        {
            testAllDivs(bi33, bi3);
            testAllDivs(bi22, bi2);
            testAllDivs(bi11, bi1);
            testAllDivs(bi13, bi1);
            testAllDivs(bi13, bi3);
            testAllDivs(bi12, bi1);
            testAllDivs(bi12, bi2);
            testAllDivs(bi23, bi2);
            testAllDivs(bi23, bi3);
            testAllDivs(largePos, bi1);
            testAllDivs(largePos, bi2);
            testAllDivs(largePos, bi3);
            testAllDivs(largeNeg, bi1);
            testAllDivs(largeNeg, bi2);
            testAllDivs(largeNeg, bi3);
            testAllDivs(largeNeg, largePos);
            testAllDivs(largePos, largeNeg);
            testAllDivs(bi3, bi3);
            testAllDivs(bi2, bi2);
            testAllDivs(bi1, bi1);
            testDivRanges(bi1);
            testDivRanges(bi2);
            testDivRanges(bi3);
            testDivRanges(smallPos);
            testDivRanges(largePos);
            testDivRanges(BigInteger.Parse("62EB40FEF85AA9EB", 16));
            testAllDivs(BigInteger.FromInt64(0xCC0225953CL), BigInteger
                    .FromInt64(0x1B937B765L));

            try
            {
                var _ = largePos / (zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                var _ = bi1 / (zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                var _ = (-bi3) / (zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                var _ = zero / (zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }
        }

        /**
         * @tests java.math.BigInteger#remainder(java.math.BigInteger)
         */
        [Test]
        public void test_remainderLjava_math_BigInteger()
        {
            try
            {
                BigInteger.Remainder(largePos, zero);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.Remainder(bi1, zero);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.Remainder(-bi3, zero);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.Remainder(zero, zero);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }
        }

        /**
         * @tests java.math.BigInteger#mod(java.math.BigInteger)
         */
        [Test]
        public void test_modLjava_math_BigInteger()
        {
            try
            {
                BigInteger.Mod(largePos, zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                BigInteger.Mod(bi1, zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                BigInteger.Mod(-bi3, zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }

            try
            {
                BigInteger.Mod(zero, zero);
                fail("ArithmeticException expected");
            }
            catch (ArithmeticException e)
            {
            }
        }

        /**
         * @tests java.math.BigInteger#divideAndRemainder(java.math.BigInteger)
         */
        [Test]
        public void test_divideAndRemainderLjava_math_BigInteger()
        {
            try
            {
                BigInteger.DivideAndRemainder(largePos, zero, out BigInteger _);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.DivideAndRemainder(bi1, zero, out BigInteger _);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.DivideAndRemainder(-bi3, zero, out BigInteger _);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }

            try
            {
                BigInteger.DivideAndRemainder(zero, zero, out BigInteger _);
                fail("ArithmeticException expected");
            }
            catch (DivideByZeroException e)
            {
            }
        }

        /**
         * @tests java.math.BigInteger#BigInteger(java.lang.String)
         */
        [Test]
        public void test_ConstructorLjava_lang_String()
        {
            assertTrue("new(0)", BigInteger.Parse("0") == (BigInteger.FromInt64(0)));
            assertTrue("new(1)", BigInteger.Parse("1") == (BigInteger.FromInt64(1)));
            assertTrue("new(12345678901234)", BigInteger.Parse("12345678901234")
                    == (BigInteger.FromInt64(12345678901234L)));
            assertTrue("new(-1)", BigInteger.Parse("-1") == (BigInteger
                    .FromInt64(-1)));
            assertTrue("new(-12345678901234)", BigInteger.Parse("-12345678901234")
                     == (BigInteger.FromInt64(-12345678901234L)));
        }

        /**
         * @tests java.math.BigInteger#BigInteger(java.lang.String, int)
         */
        [Test]
        public void test_ConstructorLjava_lang_StringI()
        {
            assertTrue("new(0,16)", BigInteger.Parse("0", 16) == (BigInteger
                    .FromInt64(0)));
            assertTrue("new(1,16)", BigInteger.Parse("1", 16) == (BigInteger
                    .FromInt64(1)));
            assertTrue("new(ABF345678901234,16)", BigInteger.Parse("ABF345678901234",
                    16) == (BigInteger.FromInt64(0xABF345678901234L)));
            assertTrue("new(abf345678901234,16)", BigInteger.Parse("abf345678901234",
                    16) == (BigInteger.FromInt64(0xABF345678901234L)));
            assertTrue("new(-1,16)", BigInteger.Parse("-1", 16) == (BigInteger
                    .FromInt64(-1)));
            assertTrue("new(-ABF345678901234,16)", BigInteger.Parse(
                    "-ABF345678901234", 16) == (BigInteger
                    .FromInt64(-0xABF345678901234L)));
            assertTrue("new(-abf345678901234,16)", BigInteger.Parse(
                    "-abf345678901234", 16) == (BigInteger
                    .FromInt64(-0xABF345678901234L)));
            assertTrue("new(-101010101,2)", BigInteger.Parse("-101010101", 2)
                     == (BigInteger.FromInt64(-341)));
        }

        /**
         * @tests java.math.BigInteger#toString()
         */
        [Test]
        public void test_toString()
        {
            assertTrue("0.toString", "0".Equals(BigInteger.FromInt64(0).ToString()));
            assertTrue("1.toString", "1".Equals(BigInteger.FromInt64(1).ToString()));
            assertTrue("12345678901234.toString", "12345678901234"
                    .Equals(BigInteger.FromInt64(12345678901234L).ToString()));
            assertTrue("-1.toString", "-1"
                    .Equals(BigInteger.FromInt64(-1).ToString()));
            assertTrue("-12345678901234.toString", "-12345678901234"
                    .Equals(BigInteger.FromInt64(-12345678901234L).ToString()));
        }

        /**
         * @tests java.math.BigInteger#toString(int)
         */
        [Test]
        public void test_toStringI()
        {
            assertTrue("0.toString(16)", "0".Equals(BigInteger.FromInt64(0).ToString(
                    16)));
            assertTrue("1.toString(16)", "1".Equals(BigInteger.FromInt64(1).ToString(
                    16)));
            assertTrue("ABF345678901234.toString(16)", "abf345678901234"
                    .Equals(BigInteger.FromInt64(0xABF345678901234L).ToString(16)));
            assertTrue("-1.toString(16)", "-1".Equals(BigInteger.FromInt64(-1)
                    .ToString(16)));
            assertTrue("-ABF345678901234.toString(16)", "-abf345678901234"
                    .Equals(BigInteger.FromInt64(-0xABF345678901234L).ToString(16)));
            assertTrue("-101010101.toString(2)", "-101010101".Equals(BigInteger
                    .FromInt64(-341).ToString(2)));
        }

        /**
         * @tests java.math.BigInteger#and(java.math.BigInteger)
         */
        [Test]
        public void test_andLjava_math_BigInteger()
        {
            foreach (BigInteger[] element in boolPairs)
            {
                BigInteger i1 = element[0], i2 = element[1];
                BigInteger res = i1 & i2;
                assertTrue("symmetry of and", res == (i2 & i1));
                int len = Math.Max(i1.BitLength, i2.BitLength) + 66;
                for (int i = 0; i < len; i++)
                {
                    assertTrue("and", (BigInteger.TestBit(i1, i) && BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i));
                }
            }
        }

        /**
         * @tests java.math.BigInteger#or(java.math.BigInteger)
         */
        [Test]
        public void test_orLjava_math_BigInteger()
        {
            foreach (BigInteger[] element in boolPairs)
            {
                BigInteger i1 = element[0], i2 = element[1];
                BigInteger res = i1 | i2;
                assertTrue("symmetry of or", res == (i2 | (i1)));
                int len = Math.Max(i1.BitLength, i2.BitLength) + 66;
                for (int i = 0; i < len; i++)
                {
                    assertTrue("or", (BigInteger.TestBit(i1, i) || BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i));
                }
            }
        }

        /**
         * @tests java.math.BigInteger#xor(java.math.BigInteger)
         */
        [Test]
        public void test_xorLjava_math_BigInteger()
        {
            foreach (BigInteger[] element in boolPairs)
            {
                BigInteger i1 = element[0], i2 = element[1];
                BigInteger res = i1 ^ (i2);
                assertTrue("symmetry of xor", res == (i2 ^ (i1)));
                int len = Math.Max(i1.BitLength, i2.BitLength) + 66;
                for (int i = 0; i < len; i++)
                {
                    assertTrue("xor", (BigInteger.TestBit(i1, i) ^ BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i));
                }
            }
        }

        /**
         * @tests java.math.BigInteger#not()
         */
        [Test]
        public void test_not()
        {
            foreach (BigInteger[] element in boolPairs)
            {
                BigInteger i1 = element[0];
                BigInteger res = BigInteger.Not(i1);
                int len = i1.BitLength + 66;
                for (int i = 0; i < len; i++)
                {
                    assertTrue("not", !BigInteger.TestBit(i1, i) == BigInteger.TestBit(res, i));
                }
            }
        }

        /**
         * @tests java.math.BigInteger#andNot(java.math.BigInteger)
         */
        [Test]
        public void test_andNotLjava_math_BigInteger()
        {
            foreach (BigInteger[] element in boolPairs)
            {
                BigInteger i1 = element[0], i2 = element[1];
                BigInteger res = BigInteger.AndNot(i1, i2);
                int len = Math.Max(i1.BitLength, i2.BitLength) + 66;
                for (int i = 0; i < len; i++)
                {
                    assertTrue("andNot", (BigInteger.TestBit(i1, i) && !BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i));
                }
                // asymmetrical
                i1 = element[1];
                i2 = element[0];
                res = BigInteger.AndNot(i1, i2);
                for (int i = 0; i < len; i++)
                {
                    assertTrue("andNot reversed",
                            (BigInteger.TestBit(i1, i) && !BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i));
                }
            }
            //regression for HARMONY-4653
            try
            {
                BigInteger.AndNot(BigInteger.Zero, null);
                fail("should throw NPE");
            }
            catch (Exception e)
            {
                //expected
            }
            BigInteger bi = new BigInteger(0, new byte[] { });
            assertEquals(string.Empty, BigInteger.Zero, BigInteger.AndNot(bi, BigInteger.Zero));
        }


        // ICU4N: BigInteger is sealed, so this won't work. Besides, we don't have this constructor (Parse/TryParse instead)
        //    [Test]
        //    public void testClone()
        //    {
        //        // Regression test for HARMONY-1770
        //        MyBigInteger myBigInteger = MyBigInteger("12345");
        //        myBigInteger = (MyBigInteger)myBigInteger.Clone();
        //    }

        //    static class MyBigInteger : BigInteger //, ICloneable
        //    {
        //    public MyBigInteger(String val)

        //    {
        //        super(val);
        //    }
        //    public Object clone()
        //    {
        //        try
        //        {
        //            return super.clone();
        //        }
        //        catch (CloneNotSupportedException e)
        //        {
        //            return null;
        //        }
        //    }
        //}

        public override void TestInitialize()
        {
            base.TestInitialize();
            SetUp();
        }

        protected void SetUp()
        {
            bi1 = BigInteger.Parse("2436798324768978", 16);
            bi2 = BigInteger.Parse("4576829475724387584378543764555", 16);
            bi3 = BigInteger.Parse("43987298363278574365732645872643587624387563245",
                    16);

            bi33 = BigInteger.Parse(
                    "10730846694701319120609898625733976090865327544790136667944805934175543888691400559249041094474885347922769807001",
                    10);
            bi22 = BigInteger.Parse(
                    "33301606932171509517158059487795669025817912852219962782230629632224456249",
                    10);
            bi11 = BigInteger.Parse("6809003003832961306048761258711296064", 10);
            bi23 = BigInteger.Parse(
                    "597791300268191573513888045771594235932809890963138840086083595706565695943160293610527214057",
                    10);
            bi13 = BigInteger.Parse(
                    "270307912162948508387666703213038600031041043966215279482940731158968434008",
                    10);
            bi12 = BigInteger.Parse(
                    "15058244971895641717453176477697767050482947161656458456", 10);

            largePos = BigInteger.Parse(
                    "834759814379857314986743298675687569845986736578576375675678998612743867438632986243982098437620983476924376",
                    16);
            smallPos = BigInteger.Parse("48753269875973284765874598630960986276", 16);
            largeNeg = BigInteger.Parse(
                    "-878824397432651481891353247987891423768534321387864361143548364457698487264387568743568743265873246576467643756437657436587436",
                    16);
            smallNeg = BigInteger.Parse("-567863254343798609857456273458769843", 16);
            boolPairs = new BigInteger[][] {
            new BigInteger[] { largePos, smallPos },
            new BigInteger[] { largePos, smallNeg },
            new BigInteger[] { largeNeg, smallPos },
            new BigInteger[] { largeNeg, smallNeg }
        };
        }

        private void testDiv(BigInteger i1, BigInteger i2)
        {
            BigInteger q = i1 / i2;
            BigInteger r = BigInteger.Remainder(i1, i2);
            BigInteger temp0 = BigInteger.DivideAndRemainder(i1, i2, out BigInteger temp1);

            assertTrue("divide and divideAndRemainder do not agree", q
                    .Equals(temp0));
            assertTrue("remainder and divideAndRemainder do not agree", r
                    .Equals(temp1));
            assertTrue("signum and equals(zero) do not agree on quotient", q
                    .Sign != 0
                    || q.Equals(zero));
            assertTrue("signum and equals(zero) do not agree on remainder", r
                    .Sign != 0
                    || r.Equals(zero));
            assertTrue("wrong sign on quotient", q.Sign == 0
                    || q.Sign == i1.Sign * i2.Sign);
            assertTrue("wrong sign on remainder", r.Sign == 0
                    || r.Sign == i1.Sign);
            assertTrue("remainder out of range", BigInteger.Abs(r).CompareTo(BigInteger.Abs(i2)) < 0);
            assertTrue("quotient too small", ((BigInteger.Abs(q) + one) * BigInteger.Abs(i2))
                    .CompareTo(BigInteger.Abs(i1)) > 0);
            assertTrue("quotient too large", (BigInteger.Abs(q) * BigInteger.Abs(i2)).CompareTo(
                    BigInteger.Abs(i1)) <= 0);
            BigInteger p = q * (i2);
            BigInteger a = p + (r);
            assertTrue("(a/b)*b+(a%b) != a", a == (i1));
            try
            {
                BigInteger mod = i1 % (i2);
                assertTrue("mod is negative", mod.Sign >= 0);
                assertTrue("mod out of range", BigInteger.Abs(mod).CompareTo(BigInteger.Abs(i2)) < 0);
                assertTrue("positive remainder == mod", r.Sign < 0
                        || r == (mod));
                assertTrue("negative remainder == mod - divisor", r.Sign >= 0
                        || r == (mod - i2));
            }
            catch (ArithmeticException e)
            {
                assertTrue("mod fails on negative divisor only", i2.Sign <= 0);
            }
        }

        private void testDivRanges(BigInteger i)
        {
            BigInteger bound = i * (two);
            for (BigInteger j = -bound; j.CompareTo(bound) <= 0; j = j + i)
            {
                BigInteger innerbound = j + (two);
                BigInteger k = j - (two);
                for (; k.CompareTo(innerbound) <= 0; k = k + (one))
                {
                    testDiv(k, i);
                }
            }
        }

        private bool isPrime(long b)
        {
            if (b == 2)
            {
                return true;
            }
            // check for div by 2
            if ((b & 1L) == 0)
            {
                return false;
            }
            long maxlen = ((long)Math.Sqrt(b)) + 2;
            for (long x = 3; x < maxlen; x += 2)
            {
                if (b % x == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void testAllMults(BigInteger i1, BigInteger i2, BigInteger ans)
        {
            assertTrue("i1*i2=ans", i1 * (i2) == (ans));
            assertTrue("i2*i1=ans", i2 * (i1) == (ans));
            assertTrue("-i1*i2=-ans", -i1 * i2 == -ans);
            assertTrue("-i2*i1=-ans", -i2 * i1 == -ans);
            assertTrue("i1*-i2=-ans", i1 * (-i2) == -ans);
            assertTrue("i2*-i1=-ans", i2 * (-i1) == -ans);
            assertTrue("-i1*-i2=ans", -i1 * (-i2) == ans);
            assertTrue("-i2*-i1=ans", -i2 * (-i1) == (ans));
        }

        private void testAllDivs(BigInteger i1, BigInteger i2)
        {
            testDiv(i1, i2);
            testDiv(-i1, i2);
            testDiv(i1, -i2);
            testDiv(-i1, -i2);
        }
    }
}
