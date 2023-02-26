using ICU4N.Support.Numerics.BigMath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Numerics.BigMath
{
#if FEATURE_BIGMATH
    public
#else
    internal
#endif
        sealed partial class BigInteger
    {
        /// <summary>
        /// Computes the absolute value of the given <see cref="BigInteger"/>
        /// </summary>
        /// <returns>
        /// Returns an instance of <see cref="BigInteger"/> that represents the
        /// absolute value of this instance.
        /// </returns>
        public static BigInteger Abs(BigInteger value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return ((value.Sign < 0) ? new BigInteger(1, value.numberLength, value.digits) : value);
        }

        /// <summary>
        /// Computes the negation of this <see cref="BigInteger"/>.
        /// </summary>
        /// <returns>
        /// Returns an instance of <see cref="BigInteger"/> that is the negated value
        /// of this instance.
        /// </returns>
        public static BigInteger Negate(BigInteger value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return ((value.Sign == 0) ? value : new BigInteger(-value.Sign, value.numberLength, value.digits));
        }

        /// <summary>
        /// Computes an addition between two big integer numbers
        /// </summary>
        /// <param name="a">The first term of the addition</param>
        /// <param name="b">The second term of the addition</param>
        /// <returns>Returns a new <see cref="BigInteger"/> that
        /// is the result of the addition of the two integers specified</returns>
        public static BigInteger Add(BigInteger a, BigInteger b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            return Elementary.Add(a, b);
        }

        /// <summary>
        /// Subtracts a big integer value from another 
        /// </summary>
        /// <param name="a">The subtrahend value</param>
        /// <param name="b">The subtractor value</param>
        /// <returns>
        /// </returns>
        public static BigInteger Subtract(BigInteger a, BigInteger b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            return Elementary.Subtract(a, b);
        }

        /// <summary>
        /// Shifts the given big integer on the right by the given distance
        /// </summary>
        /// <param name="value">The integer value to shif</param>
        /// <param name="n">The shift distance</param>
        /// <remarks>
        /// <para>
        /// For negative arguments, the result is also negative.The shift distance 
        /// may be negative which means that <paramref name="value"/> is shifted left.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> Usage of this method on negative values is not recommended 
        /// as the current implementation is not efficient.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public static BigInteger ShiftRight(BigInteger value, int n)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if ((n == 0) || (value.Sign == 0))
            {
                return value;
            }
            return ((n > 0)
                ? BitLevel.ShiftRight(value, n)
                : BitLevel.ShiftLeft(
                    value, -n));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="n"></param>
        /// <remarks>
        /// <para>
        /// The result is equivalent to <c>value * 2^n</c> if n is greater 
        /// than or equal to 0.
        /// The shift distance may be negative which means that <paramref name="value"/> is 
        /// shifted right.The result then corresponds to <c>floor(value / 2 ^ (-n))</c>.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> Usage of this method on negative values is not recommended 
        /// as the current implementation is not efficient.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public static BigInteger ShiftLeft(BigInteger value, int n)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if ((n == 0) || (value.Sign == 0))
            {
                return value;
            }
            return ((n > 0) ? BitLevel.ShiftLeft(value, n) : BitLevel.ShiftRight(value, -n));
        }

        /**
         * Tests whether the bit at position n in {@code this} is set. The result is
         * equivalent to {@code this & (2^n) != 0}.
         * <para/>
         * <b>Implementation Note:</b> Usage of this method is not recommended as
         * the current implementation is not efficient.
         *
         * @param n
         *            position where the bit in {@code this} has to be inspected.
         * @return {@code this & (2^n) != 0}.
         * @throws ArithmeticException
         *             if {@code n < 0}.
         */
        public static bool TestBit(BigInteger value, int n)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (n == 0)
            {
                return ((value.digits[0] & 1) != 0);
            }
            if (n < 0)
            {
                // math.15=Negative bit address
                throw new ArithmeticException(Messages.math15); //$NON-NLS-1$
            }
            int intCount = n >> 5;
            if (intCount >= value.numberLength)
            {
                return (value.sign < 0);
            }
            int digit = value.digits[intCount];
            n = (1 << (n & 31)); // int with 1 set to the needed position
            if (value.Sign < 0)
            {
                int firstNonZeroDigit = value.FirstNonZeroDigit;
                if (intCount < firstNonZeroDigit)
                {
                    return false;
                }
                else if (firstNonZeroDigit == intCount)
                {
                    digit = -digit;
                }
                else
                {
                    digit = ~digit;
                }
            }
            return ((digit & n) != 0);
        }

        /**
        * Returns a new {@code BigInteger} which has the same binary representation
        * as {@code this} but with the bit at position n set. The result is
        * equivalent to {@code this | 2^n}.
        * <para/>
        * <b>Implementation Note:</b> Usage of this method is not recommended as
        * the current implementation is not efficient.
        *
        * @param n
        *            position where the bit in {@code this} has to be set.
        * @return {@code this | 2^n}.
        * @throws ArithmeticException
        *             if {@code n < 0}.
        */
        public static BigInteger SetBit(BigInteger value, int n)
        {
            if (!TestBit(value, n))
            {
                return BitLevel.FlipBit(value, n);
            }
            return value;
        }

        /**
        * Returns a new {@code BigInteger} which has the same binary representation
        * as {@code this} but with the bit at position n cleared. The result is
        * equivalent to {@code this & ~(2^n)}.
        * <para/>
        * <b>Implementation Note:</b> Usage of this method is not recommended as
        * the current implementation is not efficient.
        *
        * @param n
        *            position where the bit in {@code this} has to be cleared.
        * @return {@code this & ~(2^n)}.
        * @throws ArithmeticException
        *             if {@code n < 0}.
        */
        public static BigInteger ClearBit(BigInteger value, int n)
        {
            if (TestBit(value, n))
            {
                return BitLevel.FlipBit(value, n);
            }
            return value;
        }

        /**
        * Returns a new {@code BigInteger} which has the same binary representation
        * as {@code this} but with the bit at position n flipped. The result is
        * equivalent to {@code this ^ 2^n}.
        * <para/>
        * <b>Implementation Note:</b> Usage of this method is not recommended as
        * the current implementation is not efficient.
        *
        * @param n
        *            position where the bit in {@code this} has to be flipped.
        * @return {@code this ^ 2^n}.
        * @throws ArithmeticException
        *             if {@code n < 0}.
        */
        public static BigInteger FlipBit(BigInteger value, int n)
        {
            if (n < 0)
            {
                // math.15=Negative bit address
                throw new ArithmeticException(Messages.math15); //$NON-NLS-1$
            }
            return BitLevel.FlipBit(value, n);
        }


        /**
        * Returns a new {@code BigInteger} whose value is {@code ~this}. The result
        * of this operation is {@code -this-1}.
        * <para/>
        * <b>Implementation Note:</b> Usage of this method is not recommended as
        * the current implementation is not efficient.
        *
        * @return {@code ~this}.
        */
        public static BigInteger Not(BigInteger value)
        {
            return Logical.Not(value);
        }

        /// <summary>
        /// Computes the bit per bit operator between two numbers
        /// </summary>
        /// <param name="a">The first term of the operation.</param>
        /// <param name="b">The second term of the oepration</param>
        /// <remarks>
        /// <strong>Note:</strong> Usage of this method is not recommended as 
        /// the current implementation is not efficient.
        /// </remarks>
        /// <returns>
        /// Returns a new <see cref="BigInteger"/> whose value is the result
        /// of an logical and between the given numbers.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If either <paramref name="a"/> or <paramref name="b"/> is <c>null</c>.
        /// </exception>
        public static BigInteger And(BigInteger a, BigInteger b)
        {
            return Logical.And(a, b);
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this | val}.
         * <para/>
         * <b>Implementation Note:</b> Usage of this method is not recommended as
         * the current implementation is not efficient.
         *
         * @param val
         *            value to be or'ed with {@code this}.
         * @return {@code this | val}.
         * @throws NullPointerException
         *             if {@code val == null}.
         */
        public static BigInteger Or(BigInteger a, BigInteger b)
        {
            return Logical.Or(a, b);
        }


        /**
         * Returns a new {@code BigInteger} whose value is {@code this ^ val}.
         * <para/>
         * <b>Implementation Note:</b> Usage of this method is not recommended as
         * the current implementation is not efficient.
         *
         * @param val
         *            value to be xor'ed with {@code this}
         * @return {@code this ^ val}
         * @throws NullPointerException
         *             if {@code val == null}
         */
        public static BigInteger XOr(BigInteger a, BigInteger b)
        {
            return Logical.Xor(a, b);
        }

        /**
        * Returns a new {@code BigInteger} whose value is {@code this & ~val}.
        * Evaluating {@code x.andNot(val)} returns the same result as {@code
        * x.and(val.not())}.
        * <para/>
        * <b>Implementation Note:</b> Usage of this method is not recommended as
        * the current implementation is not efficient.
        *
        * @param val
        *            value to be not'ed and then and'ed with {@code this}.
        * @return {@code this & ~val}.
        * @throws NullPointerException
        *             if {@code val == null}.
        */
        public static BigInteger AndNot(BigInteger value, BigInteger other)
        {
            return Logical.AndNot(value, other);
        }


        /**
        * Returns a new {@code BigInteger} whose value is greatest common divisor
        * of {@code this} and {@code val}. If {@code this==0} and {@code val==0}
        * then zero is returned, otherwise the result is positive.
        *
        * @param val
        *            value with which the greatest common divisor is computed.
        * @return {@code gcd(this, val)}.
        * @throws NullPointerException
        *             if {@code val == null}.
        */
        public static BigInteger Gcd(BigInteger a, BigInteger b) // ICU4N TODO: API - Rename GreatestCommonDivisor to match .NET
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            BigInteger val1 = Abs(a);
            BigInteger val2 = Abs(b);
            // To avoid a possible division by zero
            if (val1.Sign == 0)
            {
                return val2;
            }
            else if (val2.Sign == 0)
            {
                return val1;
            }

            // Optimization for small operands
            // (op2.bitLength() < 64) and (op1.bitLength() < 64)
            if (((val1.numberLength == 1) || ((val1.numberLength == 2) && (val1.digits[1] > 0)))
                && (val2.numberLength == 1 || (val2.numberLength == 2 && val2.digits[1] > 0)))
            {
                return BigInteger.GetInstance(Division.GcdBinary(val1.ToInt64(), val2.ToInt64()));
            }

            return Division.GcdBinary(val1.Copy(), val2.Copy());

        }

        /// <summary>
        /// Returns a new <see cref="BigInteger"/> whose value is <paramref name="a"/> * <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static BigInteger Multiply(BigInteger a, BigInteger b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            // This let us to throw NullPointerException when val == null
            if (b.Sign == 0)
            {
                return Zero;
            }
            if (a.Sign == 0)
            {
                return Zero;
            }

            return Multiplication.Multiply(a, b);
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this ^ exp}.
         *
         * @param exp
         *            exponent to which {@code this} is raised.
         * @return {@code this ^ exp}.
         * @throws ArithmeticException
         *             if {@code exp < 0}.
         */
        public static BigInteger Pow(BigInteger value, int exp)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (exp < 0)
            {
                // math.16=Negative exponent
                throw new ArithmeticException(Messages.math16); //$NON-NLS-1$
            }
            if (exp == 0)
            {
                return One;
            }
            else if (exp == 1 ||
                       value.Equals(One) || value.Equals(Zero))
            {
                return value;
            }

            // if even take out 2^x factor which we can
            // calculate by shifting.
            if (!TestBit(value, 0))
            {
                int x = 1;
                while (!TestBit(value, x))
                {
                    x++;
                }

                return GetPowerOfTwo(x * exp) * (Pow(value >> x, exp));
            }

            return Multiplication.Pow(value, exp);
        }

        /**
         * Returns a {@code BigInteger} array which contains {@code this / divisor}
         * at index 0 and {@code this % divisor} at index 1.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return {@code [this / divisor, this % divisor]}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @see #divide
         * @see #remainder
         */
        public static BigInteger DivideAndRemainder(BigInteger dividend, BigInteger divisor, out BigInteger remainder) // ICU4N TODO: API Rename DivRem
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            int divisorSign = divisor.Sign;
            if (divisorSign == 0)
            {
                // math.17=BigInteger divide by zero
                throw new DivideByZeroException(Messages.math17); //$NON-NLS-1$
            }
            int divisorLen = divisor.numberLength;
            int[] divisorDigits = divisor.digits;
            if (divisorLen == 1)
            {
                var values = Division.DivideAndRemainderByInteger(dividend, divisorDigits[0], divisorSign);
                remainder = values[1];
                return values[0];
            }

            int[] thisDigits = dividend.digits;
            int thisLen = dividend.numberLength;
            int cmp = (thisLen != divisorLen)
                ? ((thisLen > divisorLen) ? 1 : -1)
                : Elementary.CompareArrays(thisDigits, divisorDigits, thisLen);
            if (cmp < 0)
            {
                remainder = dividend;
                return Zero;
            }
            int thisSign = dividend.Sign;
            int quotientLength = thisLen - divisorLen + 1;
            int remainderLength = divisorLen;
            int quotientSign = ((thisSign == divisorSign) ? 1 : -1);
            int[] quotientDigits = new int[quotientLength];
            int[] remainderDigits = Division.Divide(quotientDigits, quotientLength,
                thisDigits, thisLen, divisorDigits, divisorLen);

            var quotient = new BigInteger(quotientSign, quotientLength, quotientDigits);
            remainder = new BigInteger(thisSign, remainderLength, remainderDigits);
            quotient.CutOffLeadingZeroes();
            remainder.CutOffLeadingZeroes();

            return quotient;
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this / divisor}.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return {@code this / divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         */
        public static BigInteger Divide(BigInteger dividend, BigInteger divisor)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            if (divisor.Sign == 0)
            {
                // math.17=BigInteger divide by zero
                throw new DivideByZeroException(Messages.math17); //$NON-NLS-1$
            }
            int divisorSign = divisor.Sign;
            if (divisor.IsOne)
            {
                return ((divisor.Sign > 0) ? dividend : -dividend);
            }
            int thisSign = dividend.Sign;
            int thisLen = dividend.numberLength;
            int divisorLen = divisor.numberLength;
            if (thisLen + divisorLen == 2)
            {
                long val = (dividend.digits[0] & 0xFFFFFFFFL)
                           / (divisor.digits[0] & 0xFFFFFFFFL);
                if (thisSign != divisorSign)
                {
                    val = -val;
                }
                return BigInteger.GetInstance(val);
            }
            int cmp = ((thisLen != divisorLen)
                ? ((thisLen > divisorLen) ? 1 : -1)
                : Elementary.CompareArrays(dividend.digits, divisor.digits, thisLen));
            if (cmp == EQUALS)
            {
                return ((thisSign == divisorSign) ? One : MinusOne);
            }
            if (cmp == LESS)
            {
                return Zero;
            }
            int resLength = thisLen - divisorLen + 1;
            int[] resDigits = new int[resLength];
            int resSign = ((thisSign == divisorSign) ? 1 : -1);
            if (divisorLen == 1)
            {
                Division.DivideArrayByInt(resDigits, dividend.digits, thisLen,
                    divisor.digits[0]);
            }
            else
            {
                Division.Divide(resDigits, resLength, dividend.digits, thisLen,
                    divisor.digits, divisorLen);
            }
            BigInteger result = new BigInteger(resSign, resLength, resDigits);
            result.CutOffLeadingZeroes();
            return result;
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this % divisor}.
         * Regarding signs this methods has the same behavior as the % operator on
         * int's, i.e. the sign of the remainder is the same as the sign of this.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return {@code this % divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         */
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            if (divisor.Sign == 0)
            {
                // math.17=BigInteger divide by zero
                throw new DivideByZeroException(Messages.math17); //$NON-NLS-1$
            }
            int thisLen = dividend.numberLength;
            int divisorLen = divisor.numberLength;
            if (((thisLen != divisorLen)
                    ? ((thisLen > divisorLen) ? 1 : -1)
                    : Elementary.CompareArrays(dividend.digits, divisor.digits, thisLen)) == LESS)
            {
                return dividend;
            }
            int resLength = divisorLen;
            int[] resDigits = new int[resLength];
            if (resLength == 1)
            {
                resDigits[0] = Division.RemainderArrayByInt(dividend.digits, thisLen,
                    divisor.digits[0]);
            }
            else
            {
                int qLen = thisLen - divisorLen + 1;
                resDigits = Division.Divide(null, qLen, dividend.digits, thisLen,
                    divisor.digits, divisorLen);
            }
            BigInteger result = new BigInteger(dividend.Sign, resLength, resDigits);
            result.CutOffLeadingZeroes();
            return result;
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code 1/this mod m}. The
         * modulus {@code m} must be positive. The result is guaranteed to be in the
         * interval {@code [0, m)} (0 inclusive, m exclusive). If {@code this} is
         * not relatively prime to m, then an exception is thrown.
         *
         * @param m
         *            the modulus.
         * @return {@code 1/this mod m}.
         * @throws NullPointerException
         *             if {@code m == null}
         * @throws ArithmeticException
         *             if {@code m < 0 or} if {@code this} is not relatively prime
         *             to {@code m}
         */
        public static BigInteger ModInverse(BigInteger value, BigInteger m)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (m is null)
                throw new ArgumentNullException(nameof(m));

            if (m.Sign <= 0)
            {
                // math.18=BigInteger: modulus not positive
                throw new ArithmeticException(Messages.math18); //$NON-NLS-1$
            }
            // If both are even, no inverse exists
            if (!(TestBit(value, 0) ||
                  TestBit(m, 0)))
            {
                // math.19=BigInteger not invertible.
                throw new ArithmeticException(Messages.math19); //$NON-NLS-1$
            }
            if (m.IsOne)
            {
                return Zero;
            }

            // From now on: (m > 1)
            BigInteger res = Division.ModInverseMontgomery(Abs(value) % m, m);
            if (res.Sign == 0)
            {
                // math.19=BigInteger not invertible.
                throw new ArithmeticException(Messages.math19); //$NON-NLS-1$
            }

            res = ((value.Sign < 0) ? m - res : res);
            return res;
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this^exponent mod
         * m}. The modulus {@code m} must be positive. The result is guaranteed to
         * be in the interval {@code [0, m)} (0 inclusive, m exclusive). If the
         * exponent is negative, then {@code this.modInverse(m)^(-exponent) mod m)}
         * is computed. The inverse of this only exists if {@code this} is
         * relatively prime to m, otherwise an exception is thrown.
         *
         * @param exponent
         *            the exponent.
         * @param m
         *            the modulus.
         * @return {@code this^exponent mod val}.
         * @throws NullPointerException
         *             if {@code m == null} or {@code exponent == null}.
         * @throws ArithmeticException
         *             if {@code m < 0} or if {@code exponent<0} and this is not
         *             relatively prime to {@code m}.
         */
        public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger m)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (exponent is null)
                throw new ArgumentNullException(nameof(exponent));
            if (m is null)
                throw new ArgumentNullException(nameof(m));

            if (m.Sign <= 0)
            {
                // math.18=BigInteger: modulus not positive
                throw new ArithmeticException(Messages.math18); //$NON-NLS-1$
            }
            BigInteger b = value;

            if (m.IsOne || (exponent.Sign > 0 && b.Sign == 0))
            {
                return Zero;
            }
            if (b.Sign == 0 && exponent.Sign == 0)
            {
                return One;
            }
            if (exponent.Sign < 0)
            {
                b = ModInverse(value, m);
                exponent = -exponent;
            }
            // From now on: (m > 0) and (exponent >= 0)
            BigInteger res = (TestBit(m, 0))
                ? Division.OddModPow(Abs(b),
                    exponent, m)
                : Division.EvenModPow(Abs(b), exponent, m);
            if ((b.Sign < 0) && TestBit(exponent, 0))
            {
                // -b^e mod m == ((-1 mod m) * (b^e mod m)) mod m
                res = ((m - One) * res) % m;
            }
            // else exponent is even, so base^exp is positive
            return res;
        }

        /**
         * Returns a new {@code BigInteger} whose value is {@code this mod m}. The
         * modulus {@code m} must be positive. The result is guaranteed to be in the
         * interval {@code [0, m)} (0 inclusive, m exclusive). The behavior of this
         * function is not equivalent to the behavior of the % operator defined for
         * the built-in {@code int}'s.
         *
         * @param m
         *            the modulus.
         * @return {@code this mod m}.
         * @throws NullPointerException
         *             if {@code m == null}.
         * @throws ArithmeticException
         *             if {@code m < 0}.
         */
        public static BigInteger Mod(BigInteger value, BigInteger m)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (m is null)
                throw new ArgumentNullException(nameof(m));

            if (m.Sign <= 0)
            {
                // math.18=BigInteger: modulus not positive
                throw new ArithmeticException(Messages.math18); //$NON-NLS-1$
            }
            BigInteger rem = Remainder(value, m);
            return ((rem.Sign < 0) ? rem + m : rem);
        }

        /**
       * Tests whether this {@code BigInteger} is probably prime. If {@code true}
       * is returned, then this is prime with a probability beyond
       * (1-1/2^certainty). If {@code false} is returned, then this is definitely
       * composite. If the argument {@code certainty} <= 0, then this method
       * returns true.
       *
       * @param certainty
       *            tolerated primality uncertainty.
       * @return {@code true}, if {@code this} is probably prime, {@code false}
       *         otherwise.
       */
        public static bool IsProbablePrime(BigInteger value, int certainty)
        {
            return Primality.IsProbablePrime(Abs(value), certainty);
        }

        /**
        * Returns the smallest integer x > {@code this} which is probably prime as
        * a {@code BigInteger} instance. The probability that the returned {@code
        * BigInteger} is prime is beyond (1-1/2^80).
        *
        * @return smallest integer > {@code this} which is robably prime.
        * @throws ArithmeticException
        *             if {@code this < 0}.
        */
        public static BigInteger NextProbablePrime(BigInteger value)
        {
            if (value.Sign < 0)
            {
                // math.1A=start < 0: {0}
                throw new ArithmeticException(string.Format(Messages.math1A, value)); //$NON-NLS-1$
            }
            return Primality.NextProbablePrime(value);
        }

        /**
        * Returns a random positive {@code BigInteger} instance in the range [0,
        * 2^(bitLength)-1] which is probably prime. The probability that the
        * returned {@code BigInteger} is prime is beyond (1-1/2^80).
        * <para/>
        * <b>Implementation Note:</b> Currently {@code rnd} is ignored.
        *
        * @param bitLength
        *            length of the new {@code BigInteger} in bits.
        * @param rnd
        *            random generator used to generate the new {@code BigInteger}.
        * @return probably prime random {@code BigInteger} instance.
        * @throws IllegalArgumentException
        *             if {@code bitLength < 2}.
        */
        public static BigInteger ProbablePrime(int bitLength, Random rnd)
        {
            return new BigInteger(bitLength, 100, rnd);
        }
    }
}
