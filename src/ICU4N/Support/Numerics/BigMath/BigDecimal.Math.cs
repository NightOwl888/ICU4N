using ICU4N.Support.Numerics.BigMath;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICU4N.Numerics.BigMath
{
#if FEATURE_BIGMATH
    public
#else
    internal
#endif
        sealed partial class BigDecimal
    {
        /// <summary>
        /// Adds a value to the current instance of <see cref="BigDecimal"/>,
        /// rounding the result according to the provided context.
        /// </summary>
        /// <param name="value">The value to add to.</param>
        /// <param name="augend">The value to be added to this instance.</param>
        /// <param name="mc">The rounding mode and precision for the result of 
        /// this operation.</param>
        /// <returns>
        /// Returns a new <see cref="BigDecimal"/> whose value is <c>this + <paramref name="augend"/></c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the given <paramref name="augend"/> or <paramref name="mc"/> is <c>null</c>.
        /// </exception>
        public static BigDecimal Add(BigDecimal value, BigDecimal augend, MathContext mc)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (augend is null)
                throw new ArgumentNullException(nameof(augend));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            BigDecimal larger; // operand with the largest unscaled value
            BigDecimal smaller; // operand with the smallest unscaled value
            BigInteger tempBi;
            long diffScale = (long)value.Scale - augend.Scale;

            // Some operand is zero or the precision is infinity  
            if ((augend.IsZero) || (value.IsZero) || (mc.Precision == 0))
            {
                return RoundImpl(Add(value, augend), mc);
            }
            // Cases where there is room for optimizations
            if (value.AproxPrecision() < diffScale - 1)
            {
                larger = augend;
                smaller = value;
            }
            else if (augend.AproxPrecision() < -diffScale - 1)
            {
                larger = value;
                smaller = augend;
            }
            else
            {
                // No optimization is done 
                return RoundImpl(Add(value, augend), mc);
            }
            if (mc.Precision >= larger.AproxPrecision())
            {
                // No optimization is done
                return RoundImpl(Add(value, augend), mc);
            }

            // Cases where it's unnecessary to add two numbers with very different scales 
            var largerSignum = larger.Sign;
            if (largerSignum == smaller.Sign)
            {
                tempBi = Multiplication.MultiplyByPositiveInt(larger.UnscaledValue, 10) +
                         BigInteger.GetInstance(largerSignum);
            }
            else
            {
                tempBi = larger.UnscaledValue - BigInteger.GetInstance(largerSignum);
                tempBi = Multiplication.MultiplyByPositiveInt(tempBi, 10) +
                         BigInteger.GetInstance(largerSignum * 9);
            }
            // Rounding the improved adding 
            larger = new BigDecimal(tempBi, larger.Scale + 1);
            return RoundImpl(larger, mc);
        }

        /// <summary>
        /// Adds a value to the current instance of <see cref="BigDecimal"/>.
        /// The scale of the result is the maximum of the scales of the two arguments.
        /// </summary>
        /// <param name="value">The value to add to.</param>
        /// <param name="augend">The value to be added to this instance.</param>
        /// <returns>
        /// Returns a new {@code BigDecimal} whose value is <c>this + <paramref name="augend"/></c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the given <paramref name="augend"/> is <c>null</c>.
        /// </exception>
        public static BigDecimal Add(BigDecimal value, BigDecimal augend)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (augend is null)
                throw new ArgumentNullException(nameof(augend));

            int diffScale = value.Scale - augend.Scale;
            // Fast return when some operand is zero
            if (value.IsZero)
            {
                if (diffScale <= 0)
                    return augend;
                if (augend.IsZero)
                    return value;
            }
            else if (augend.IsZero)
            {
                if (diffScale >= 0)
                    return value;
            }
            // Let be:  this = [u1,s1]  and  augend = [u2,s2]
            if (diffScale == 0)
            {
                // case s1 == s2: [u1 + u2 , s1]
                if (System.Math.Max(value.BitLength, augend.BitLength) + 1 < 64)
                {
                    return BigDecimal.GetInstance(value.SmallValue + augend.SmallValue, value.Scale);
                }
                return new BigDecimal(value.UnscaledValue + augend.UnscaledValue, value.Scale);
            }
            if (diffScale > 0)
                // case s1 > s2 : [(u1 + u2) * 10 ^ (s1 - s2) , s1]
                return AddAndMult10(value, augend, diffScale);

            // case s2 > s1 : [(u2 + u1) * 10 ^ (s2 - s1) , s2]
            return AddAndMult10(augend, value, -diffScale);
        }

        /// <summary>
        /// Subtracts the given value from this instance of <see cref="BigDecimal"/>.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="value">The value to subtract from.</param>
        /// <param name="subtrahend">The value to be subtracted from this <see cref="BigDecimal"/>.</param>
        /// <returns>
        /// Returns an instance of <see cref="BigDecimal"/> that is the result of the
        /// subtraction of the given <paramref name="subtrahend"/> from this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the given <paramref name="subtrahend"/> is <c>null</c>.
        /// </exception>
        public static BigDecimal Subtract(BigDecimal value, BigDecimal subtrahend)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (subtrahend is null)
                throw new ArgumentNullException(nameof(subtrahend));

            int diffScale = value.Scale - subtrahend.Scale;

            // Fast return when some operand is zero
            if (value.IsZero)
            {
                if (diffScale <= 0)
                {
                    return -subtrahend;
                }
                if (subtrahend.IsZero)
                {
                    return value;
                }
            }
            else if (subtrahend.IsZero)
            {
                if (diffScale >= 0)
                {
                    return value;
                }
            }
            // Let be: this = [u1,s1] and subtrahend = [u2,s2] so:
            if (diffScale == 0)
            {
                // case s1 = s2 : [u1 - u2 , s1]
                if (System.Math.Max(value.BitLength, subtrahend.BitLength) + 1 < 64)
                {
                    return BigDecimal.GetInstance(value.SmallValue - subtrahend.SmallValue, value.Scale);
                }
                return new BigDecimal(value.UnscaledValue - subtrahend.UnscaledValue, value.Scale);
            }
            if (diffScale > 0)
            {
                // case s1 > s2 : [ u1 - u2 * 10 ^ (s1 - s2) , s1 ]
                if (diffScale < BigDecimal.LongTenPow.Length &&
                    System.Math.Max(value.BitLength, subtrahend.BitLength + BigDecimal.LongTenPowBitLength[diffScale]) + 1 < 64)
                {
                    return BigDecimal.GetInstance(value.SmallValue - subtrahend.SmallValue * BigDecimal.LongTenPow[diffScale], value.Scale);
                }
                return new BigDecimal(
                    value.UnscaledValue - Multiplication.MultiplyByTenPow(subtrahend.UnscaledValue, diffScale),
                    value.Scale);
            }

            // case s2 > s1 : [ u1 * 10 ^ (s2 - s1) - u2 , s2 ]
            diffScale = -diffScale;
            if (diffScale < BigDecimal.LongTenPow.Length &&
                System.Math.Max(value.BitLength + BigDecimal.LongTenPowBitLength[diffScale], subtrahend.BitLength) + 1 < 64)
            {
                return BigDecimal.GetInstance(value.SmallValue * BigDecimal.LongTenPow[diffScale] - subtrahend.SmallValue, subtrahend.Scale);
            }

            return new BigDecimal(Multiplication.MultiplyByTenPow(value.UnscaledValue, diffScale) -
                                  subtrahend.UnscaledValue, subtrahend.Scale);
        }

        /// <summary>
        /// Subtracts the given value from this instance of <see cref="BigDecimal"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This overload rounds the result of the operation to the <paramref name="mc">context</paramref>
        /// provided as argument.
        /// </para>
        /// </remarks>
        /// <param name="value">The value to subtract from.</param>
        /// <param name="subtrahend">The value to be subtracted from this <see cref="BigDecimal"/>.</param>
        /// <param name="mc">The context used to round the result of this operation.</param>
        /// <returns>
        /// Returns an instance of <see cref="BigDecimal"/> that is the result of the
        /// subtraction of the given <paramref name="subtrahend"/> from this instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If either of the given <paramref name="subtrahend"/> or <paramref name="mc"/> are <c>null</c>.
        /// </exception>
        public static BigDecimal Subtract(BigDecimal value, BigDecimal subtrahend, MathContext mc)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (subtrahend is null)
                throw new ArgumentNullException(nameof(subtrahend));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            long diffScale = subtrahend.Scale - (long)value.Scale;

            // Some operand is zero or the precision is infinity  
            if ((subtrahend.IsZero) || (value.IsZero) || (mc.Precision == 0))
                return RoundImpl(Subtract(value, subtrahend), mc);

            // Now:   this != 0   and   subtrahend != 0
            if (subtrahend.AproxPrecision() < diffScale - 1)
            {
                // Cases where it is unnecessary to subtract two numbers with very different scales
                if (mc.Precision < value.AproxPrecision())
                {
                    var thisSignum = value.Sign;
                    BigInteger tempBI;
                    if (thisSignum != subtrahend.Sign)
                    {
                        tempBI = Multiplication.MultiplyByPositiveInt(value.UnscaledValue, 10) +
                                 BigInteger.GetInstance(thisSignum);
                    }
                    else
                    {
                        tempBI = value.UnscaledValue - BigInteger.GetInstance(thisSignum);
                        tempBI = Multiplication.MultiplyByPositiveInt(tempBI, 10) +
                                 BigInteger.GetInstance(thisSignum * 9);
                    }
                    // Rounding the improved subtracting
                    var leftOperand = new BigDecimal(tempBI, value.Scale + 1); // it will be only the left operand (this) 
                    return RoundImpl(leftOperand, mc);
                }
            }

            // No optimization is done
            return RoundImpl(Subtract(value, subtrahend), mc);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this *
         * multiplicand}. The scale of the result is the sum of the scales of the
         * two arguments.
         *
         * @param multiplicand
         *            value to be multiplied with {@code this}.
         * @return {@code this * multiplicand}.
         * @throws NullPointerException
         *             if {@code multiplicand == null}.
         */
        public static BigDecimal Multiply(BigDecimal value, BigDecimal multiplicand)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (multiplicand is null)
                throw new ArgumentNullException(nameof(multiplicand));

            long newScale = (long)value.Scale + multiplicand.Scale;

            if ((value.IsZero) || (multiplicand.IsZero))
            {
                return GetZeroScaledBy(newScale);
            }

            // ICU4N specific: In .NET multiplying long.MinValue by -1 results in a negative number.
            // So, for this case, we need to use BigMath to arrive at the correct (positive) result.
            /* Let be: this = [u1,s1] and multiplicand = [u2,s2] so:
             * this x multiplicand = [ s1 * s2 , s1 + s2 ] */
            if (value.BitLength + multiplicand.BitLength < 64 && value.SmallValue != long.MinValue && multiplicand.SmallValue != long.MinValue)
            {
                return BigDecimal.GetInstance(value.SmallValue * multiplicand.SmallValue, ToIntScale(newScale));
            }
            return new BigDecimal(value.UnscaledValue * multiplicand.UnscaledValue, ToIntScale(newScale));
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this *
         * multiplicand}. The result is rounded according to the passed context
         * {@code mc}.
         *
         * @param multiplicand
         *            value to be multiplied with {@code this}.
         * @param mc
         *            rounding mode and precision for the result of this operation.
         * @return {@code this * multiplicand}.
         * @throws NullPointerException
         *             if {@code multiplicand == null} or {@code mc == null}.
         */
        public static BigDecimal Multiply(BigDecimal value, BigDecimal multiplicand, MathContext mc)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (multiplicand is null)
                throw new ArgumentNullException(nameof(multiplicand));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            BigDecimal result = Multiply(value, multiplicand);

            result.InplaceRound(mc);
            return result;
        }


        /**
        * Returns a new {@code BigDecimal} whose value is {@code this / divisor}.
        * As scale of the result the parameter {@code scale} is used. If rounding
        * is required to meet the specified scale, then the specified rounding mode
        * {@code roundingMode} is applied.
        *
        * @param divisor
        *            value by which {@code this} is divided.
        * @param scale
        *            the scale of the result returned.
        * @param roundingMode
        *            rounding mode to be used to round the result.
        * @return {@code this / divisor} rounded according to the given rounding
        *         mode.
        * @throws NullPointerException
        *             if {@code divisor == null} or {@code roundingMode == null}.
        * @throws ArithmeticException
        *             if {@code divisor == 0}.
        * @throws ArithmeticException
        *             if {@code roundingMode == RoundingMode.UNNECESSAR}Y and
        *             rounding is necessary according to the given scale and given
        *             precision.
        */
        public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor, int scale, RoundingMode roundingMode)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));
            if (!roundingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(roundingMode), string.Format(Messages.ArgumentOutOfRange_Enum, roundingMode, nameof(RoundingMode)));

            // Let be: this = [u1,s1]  and  divisor = [u2,s2]
            if (divisor.IsZero)
            {
                // math.04=Division by zero
                throw new DivideByZeroException(Messages.math04); //$NON-NLS-1$
            }

            long diffScale = ((long)dividend.Scale - divisor.Scale) - scale;
            if (dividend.BitLength < 64 && divisor.BitLength < 64)
            {
                if (diffScale == 0)
                    return DividePrimitiveLongs(dividend.SmallValue, divisor.SmallValue, scale, roundingMode);
                if (diffScale > 0)
                {
                    if (diffScale < LongTenPow.Length &&
                        divisor.BitLength + LongTenPowBitLength[(int)diffScale] < 64)
                    {
                        return DividePrimitiveLongs(dividend.SmallValue, divisor.SmallValue * LongTenPow[(int)diffScale], scale, roundingMode);
                    }
                }
                else
                {
                    // diffScale < 0
                    if (-diffScale < LongTenPow.Length &&
                        dividend.BitLength + LongTenPowBitLength[(int)-diffScale] < 64)
                    {
                        return DividePrimitiveLongs(dividend.SmallValue * LongTenPow[(int)-diffScale], divisor.SmallValue, scale, roundingMode);
                    }

                }
            }
            BigInteger scaledDividend = dividend.UnscaledValue;
            BigInteger scaledDivisor = divisor.UnscaledValue; // for scaling of 'u2'

            if (diffScale > 0)
            {
                // Multiply 'u2'  by:  10^((s1 - s2) - scale)
                scaledDivisor = Multiplication.MultiplyByTenPow(scaledDivisor, (int)diffScale);
            }
            else if (diffScale < 0)
            {
                // Multiply 'u1'  by:  10^(scale - (s1 - s2))
                scaledDividend = Multiplication.MultiplyByTenPow(scaledDividend, (int)-diffScale);
            }
            return DivideBigIntegers(scaledDividend, scaledDivisor, scale, roundingMode);
        }

        public static BigDecimal DivideBigIntegers(BigInteger scaledDividend, BigInteger scaledDivisor, int scale,
            RoundingMode roundingMode)
        {
            if (scaledDividend is null)
                throw new ArgumentNullException(nameof(scaledDividend));
            if (scaledDivisor is null)
                throw new ArgumentNullException(nameof(scaledDivisor));
            if (!roundingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(roundingMode), string.Format(Messages.ArgumentOutOfRange_Enum, roundingMode, nameof(RoundingMode)));

            BigInteger quotient = BigInteger.DivideAndRemainder(scaledDividend, scaledDivisor, out BigInteger remainder);
            if (remainder.Sign == 0)
            {
                return new BigDecimal(quotient, scale);
            }
            int sign = scaledDividend.Sign * scaledDivisor.Sign;
            int compRem; // 'compare to remainder'
            if (scaledDivisor.BitLength < 63)
            {
                // 63 in order to avoid out of long after <<1
                long rem = remainder.ToInt64();
                long divisor = scaledDivisor.ToInt64();
                compRem = LongCompareTo(System.Math.Abs(rem) << 1, System.Math.Abs(divisor));
                // To look if there is a carry
                compRem = RoundingBehavior(BigInteger.TestBit(quotient, 0) ? 1 : 0,
                    sign * (5 + compRem), roundingMode);

            }
            else
            {
                // Checking if:  remainder * 2 >= scaledDivisor 
                compRem = BigInteger.Abs(remainder).ShiftLeftOneBit().CompareTo(BigInteger.Abs(scaledDivisor));
                compRem = RoundingBehavior(BigInteger.TestBit(quotient, 0) ? 1 : 0,
                    sign * (5 + compRem), roundingMode);
            }
            if (compRem != 0)
            {
                if (quotient.BitLength < 63)
                {
                    return BigDecimal.GetInstance(quotient.ToInt64() + compRem, scale);
                }
                quotient += BigInteger.GetInstance(compRem);
                return new BigDecimal(quotient, scale);
            }
            // Constructing the result with the appropriate unscaled value
            return new BigDecimal(quotient, scale);
        }

        public static BigDecimal DividePrimitiveLongs(long scaledDividend, long scaledDivisor, int scale,
            RoundingMode roundingMode)
        {
            long quotient = scaledDividend / scaledDivisor;
            long remainder = scaledDividend % scaledDivisor;
            int sign = Math.Sign(scaledDividend) * Math.Sign(scaledDivisor);
            if (remainder != 0)
            {
                // Checking if:  remainder * 2 >= scaledDivisor
                int compRem; // 'compare to remainder'
                compRem = LongCompareTo(Math.Abs(remainder) << 1, Math.Abs(scaledDivisor));
                // To look if there is a carry
                quotient += RoundingBehavior(((int)quotient) & 1, sign * (5 + compRem), roundingMode);
            }
            // Constructing the result with the appropriate unscaled value
            return BigDecimal.GetInstance(quotient, scale);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this / divisor}.
         * The scale of the result is the scale of {@code this}. If rounding is
         * required to meet the specified scale, then the specified rounding mode
         * {@code roundingMode} is applied.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @param roundingMode
         *            rounding mode to be used to round the result.
         * @return {@code this / divisor} rounded according to the given rounding
         *         mode.
         * @throws NullPointerException
         *             if {@code divisor == null} or {@code roundingMode == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @throws ArithmeticException
         *             if {@code roundingMode == RoundingMode.UNNECESSARY} and
         *             rounding is necessary according to the scale of this.
         */
        public static BigDecimal Divide(BigDecimal a, BigDecimal b, RoundingMode roundingMode)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));

            return Divide(a, b, a.Scale, roundingMode);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this / divisor}.
         * The scale of the result is the difference of the scales of {@code this}
         * and {@code divisor}. If the exact result requires more digits, then the
         * scale is adjusted accordingly. For example, {@code 1/128 = 0.0078125}
         * which has a scale of {@code 7} and precision {@code 5}.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return {@code this / divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @throws ArithmeticException
         *             if the result cannot be represented exactly.
         */
        public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            BigInteger p = dividend.UnscaledValue;
            BigInteger q = divisor.UnscaledValue;
            BigInteger gcd; // greatest common divisor between 'p' and 'q'
            BigInteger quotient;
            BigInteger remainder;
            long diffScale = (long)dividend.Scale - divisor.Scale;
            int newScale; // the new scale for final quotient
            int k; // number of factors "2" in 'q'
            int l = 0; // number of factors "5" in 'q'
            int i = 1;
            int lastPow = FivePow.Length - 1;

            if (divisor.IsZero)
            {
                // math.04=Division by zero
                throw new DivideByZeroException(Messages.math04); //$NON-NLS-1$
            }
            if (p.Sign == 0)
            {
                return GetZeroScaledBy(diffScale);
            }
            // To divide both by the GCD
            gcd = BigInteger.Gcd(p, q);
            p = p / gcd;
            q = q / gcd;
            // To simplify all "2" factors of q, dividing by 2^k
            k = q.LowestSetBit;
            q = q >> k;
            // To simplify all "5" factors of q, dividing by 5^l
            do
            {
                quotient = BigInteger.DivideAndRemainder(q, FivePow[i], out remainder);
                if (remainder.Sign == 0)
                {
                    l += i;
                    if (i < lastPow)
                    {
                        i++;
                    }
                    q = quotient;
                }
                else
                {
                    if (i == 1)
                    {
                        break;
                    }
                    i = 1;
                }
            } while (true);
            // If  abs(q) != 1  then the quotient is periodic
            if (!BigInteger.Abs(q).Equals(BigInteger.One))
            {
                // math.05=Non-terminating decimal expansion; no exact representable decimal result.
                throw new ArithmeticException(Messages.math05); //$NON-NLS-1$
            }
            // The sign of the is fixed and the quotient will be saved in 'p'
            if (q.Sign < 0)
            {
                p = -p;
            }
            // Checking if the new scale is out of range
            newScale = ToIntScale(diffScale + Math.Max(k, l));
            // k >= 0  and  l >= 0  implies that  k - l  is in the 32-bit range
            i = k - l;

            p = (i > 0)
                ? Multiplication.MultiplyByFivePow(p, i)
                : p << -i;
            return new BigDecimal(p, newScale);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this / divisor}.
         * The result is rounded according to the passed context {@code mc}. If the
         * passed math context specifies precision {@code 0}, then this call is
         * equivalent to {@code this.divide(divisor)}.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @param mc
         *            rounding mode and precision for the result of this operation.
         * @return {@code this / divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null} or {@code mc == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @throws ArithmeticException
         *             if {@code mc.getRoundingMode() == UNNECESSARY} and rounding
         *             is necessary according {@code mc.getPrecision()}.
         */
        public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor, MathContext mc)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            /* Calculating how many zeros must be append to 'dividend'
             * to obtain a  quotient with at least 'mc.precision()' digits */
            long traillingZeros = mc.Precision + 2L
                                  + divisor.AproxPrecision() - dividend.AproxPrecision();
            long diffScale = (long)dividend.Scale - divisor.Scale;
            long newScale = diffScale; // scale of the final quotient
            int compRem; // to compare the remainder
            int i = 1; // index   
            int lastPow = BigDecimal.TenPow.Length - 1; // last power of ten
            BigInteger integerQuot; // for temporal results
            BigInteger quotient = dividend.UnscaledValue;
            BigInteger remainder;
            // In special cases it reduces the problem to call the dual method
            if ((mc.Precision == 0) || (dividend.IsZero) || (divisor.IsZero))
                return Divide(dividend, divisor);

            if (traillingZeros > 0)
            {
                // To append trailing zeros at end of dividend
                quotient = dividend.UnscaledValue * Multiplication.PowerOf10(traillingZeros);
                newScale += traillingZeros;
            }
            quotient = BigInteger.DivideAndRemainder(quotient, divisor.UnscaledValue, out remainder);
            integerQuot = quotient;
            // Calculating the exact quotient with at least 'mc.precision()' digits
            if (remainder.Sign != 0)
            {
                // Checking if:   2 * remainder >= divisor ?
                compRem = remainder.ShiftLeftOneBit().CompareTo(divisor.UnscaledValue);
                // quot := quot * 10 + r;     with 'r' in {-6,-5,-4, 0,+4,+5,+6}
                integerQuot = (integerQuot * BigInteger.Ten) +
                              BigInteger.GetInstance(quotient.Sign * (5 + compRem));
                newScale++;
            }
            else
            {
                // To strip trailing zeros until the preferred scale is reached
                while (!BigInteger.TestBit(integerQuot, 0))
                {
                    quotient = BigInteger.DivideAndRemainder(integerQuot, TenPow[i], out remainder);
                    if ((remainder.Sign == 0)
                        && (newScale - i >= diffScale))
                    {
                        newScale -= i;
                        if (i < lastPow)
                        {
                            i++;
                        }
                        integerQuot = quotient;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            break;
                        }
                        i = 1;
                    }
                }
            }
            // To perform rounding
            return new BigDecimal(integerQuot, ToIntScale(newScale), mc);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is the integral part of
         * {@code this / divisor}. The quotient is rounded down towards zero to the
         * next integer. For example, {@code 0.5/0.2 = 2}.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return integral part of {@code this / divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         */
        public static BigDecimal DivideToIntegralValue(BigDecimal dividend, BigDecimal divisor)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            BigInteger integralValue; // the integer of result
            BigInteger powerOfTen; // some power of ten
            BigInteger quotient;
            BigInteger remainder;
            long newScale = (long)dividend.Scale - divisor.Scale;
            long tempScale = 0;
            int i = 1;
            int lastPow = TenPow.Length - 1;

            if (divisor.IsZero)
            {
                // math.04=Division by zero
                throw new DivideByZeroException(Messages.math04); //$NON-NLS-1$
            }
            if ((divisor.AproxPrecision() + newScale > dividend.AproxPrecision() + 1L)
                || (dividend.IsZero))
            {
                /* If the divisor's integer part is greater than this's integer part,
                 * the result must be zero with the appropriate scale */
                integralValue = BigInteger.Zero;
            }
            else if (newScale == 0)
            {
                integralValue = dividend.UnscaledValue / divisor.UnscaledValue;
            }
            else if (newScale > 0)
            {
                powerOfTen = Multiplication.PowerOf10(newScale);
                integralValue = dividend.UnscaledValue / (divisor.UnscaledValue * powerOfTen);
                integralValue = integralValue * powerOfTen;
            }
            else
            {
                // (newScale < 0)
                powerOfTen = Multiplication.PowerOf10(-newScale);
                integralValue = (dividend.UnscaledValue * powerOfTen) / divisor.UnscaledValue;
                // To strip trailing zeros approximating to the preferred scale
                while (!BigInteger.TestBit(integralValue, 0))
                {
                    quotient = BigInteger.DivideAndRemainder(integralValue, TenPow[i], out remainder); // ICU4N NOTE: Implicit conversion to BigInteger - double check this.
                    if ((remainder.Sign == 0)
                        && (tempScale - i >= newScale))
                    {
                        tempScale -= i;
                        if (i < lastPow)
                        {
                            i++;
                        }
                        integralValue = quotient;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            break;
                        }
                        i = 1;
                    }
                }
                newScale = tempScale;
            }
            return ((integralValue.Sign == 0)
                ? BigDecimal.GetZeroScaledBy(newScale)
                : new BigDecimal(integralValue, BigDecimal.ToIntScale(newScale)));
        }

        /**
         * Returns a new {@code BigDecimal} whose value is the integral part of
         * {@code this / divisor}. The quotient is rounded down towards zero to the
         * next integer. The rounding mode passed with the parameter {@code mc} is
         * not considered. But if the precision of {@code mc > 0} and the integral
         * part requires more digits, then an {@code ArithmeticException} is thrown.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @param mc
         *            math context which determines the maximal precision of the
         *            result.
         * @return integral part of {@code this / divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null} or {@code mc == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @throws ArithmeticException
         *             if {@code mc.getPrecision() > 0} and the result requires more
         *             digits to be represented.
         */
        public static BigDecimal DivideToIntegralValue(BigDecimal dividend, BigDecimal divisor, MathContext mc)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            int mcPrecision = mc.Precision;
            int diffPrecision = dividend.Precision - divisor.Precision;
            int lastPow = TenPow.Length - 1;
            long diffScale = (long)dividend.Scale - divisor.Scale;
            long newScale = diffScale;
            long quotPrecision = diffPrecision - diffScale + 1;
            BigInteger quotient;
            BigInteger remainder;
            // In special cases it call the dual method
            if ((mcPrecision == 0) || (dividend.IsZero) || (divisor.IsZero))
            {
                return DivideToIntegralValue(dividend, divisor);
            }
            // Let be:   this = [u1,s1]   and   divisor = [u2,s2]
            if (quotPrecision <= 0)
            {
                quotient = BigInteger.Zero;
            }
            else if (diffScale == 0)
            {
                // CASE s1 == s2:  to calculate   u1 / u2 
                quotient = dividend.UnscaledValue / divisor.UnscaledValue;
            }
            else if (diffScale > 0)
            {
                // CASE s1 >= s2:  to calculate   u1 / (u2 * 10^(s1-s2)  
                quotient = dividend.UnscaledValue / (divisor.UnscaledValue * Multiplication.PowerOf10(diffScale));
                // To chose  10^newScale  to get a quotient with at least 'mc.precision()' digits
                newScale = Math.Min(diffScale, Math.Max(mcPrecision - quotPrecision + 1, 0));
                // To calculate: (u1 / (u2 * 10^(s1-s2)) * 10^newScale
                quotient = quotient * Multiplication.PowerOf10(newScale);
            }
            else
            {
                // CASE s2 > s1:   
                /* To calculate the minimum power of ten, such that the quotient 
                 *   (u1 * 10^exp) / u2   has at least 'mc.precision()' digits. */
                long exp = Math.Min(-diffScale, Math.Max((long)mcPrecision - diffPrecision, 0));
                long compRemDiv;
                // Let be:   (u1 * 10^exp) / u2 = [q,r]  
                quotient = BigInteger.DivideAndRemainder(dividend.UnscaledValue * Multiplication.PowerOf10(exp),
                    divisor.UnscaledValue, out remainder);
                newScale += exp; // To fix the scale
                exp = -newScale; // The remaining power of ten
                                 // If after division there is a remainder...
                if ((remainder.Sign != 0) && (exp > 0))
                {
                    // Log10(r) + ((s2 - s1) - exp) > mc.precision ?
                    compRemDiv = (new BigDecimal(remainder)).Precision
                                 + exp - divisor.Precision;
                    if (compRemDiv == 0)
                    {
                        // To calculate:  (r * 10^exp2) / u2
                        remainder = (remainder * Multiplication.PowerOf10(exp)) / divisor.UnscaledValue;
                        compRemDiv = Math.Abs(remainder.Sign);
                    }
                    if (compRemDiv > 0)
                    {
                        // The quotient won't fit in 'mc.precision()' digits
                        // math.06=Division impossible
                        throw new ArithmeticException(Messages.math06); //$NON-NLS-1$
                    }
                }
            }
            // Fast return if the quotient is zero
            if (quotient.Sign == 0)
            {
                return GetZeroScaledBy(diffScale);
            }
            BigInteger strippedBI = quotient;
            BigDecimal integralValue = new BigDecimal(quotient);
            long resultPrecision = integralValue.Precision;
            int i = 1;
            // To strip trailing zeros until the specified precision is reached
            while (!BigInteger.TestBit(strippedBI, 0))
            {
                quotient = BigInteger.DivideAndRemainder(strippedBI, TenPow[i], out remainder);
                if ((remainder.Sign == 0) &&
                    ((resultPrecision - i >= mcPrecision)
                     || (newScale - i >= diffScale)))
                {
                    resultPrecision -= i;
                    newScale -= i;
                    if (i < lastPow)
                    {
                        i++;
                    }
                    strippedBI = quotient;
                }
                else
                {
                    if (i == 1)
                    {
                        break;
                    }
                    i = 1;
                }
            }
            // To check if the result fit in 'mc.precision()' digits
            if (resultPrecision > mcPrecision)
            {
                // math.06=Division impossible
                throw new ArithmeticException(Messages.math06); //$NON-NLS-1$
            }
            integralValue.Scale = ToIntScale(newScale);
            integralValue.SetUnscaledValue(strippedBI);
            return integralValue;
        }

        /**
        * Returns a new {@code BigDecimal} whose value is {@code this % divisor}.
        * <para/>
        * The remainder is defined as {@code this -
        * this.divideToIntegralValue(divisor) * divisor}.
        *
        * @param divisor
        *            value by which {@code this} is divided.
        * @return {@code this % divisor}.
        * @throws NullPointerException
        *             if {@code divisor == null}.
        * @throws ArithmeticException
        *             if {@code divisor == 0}.
        */
        public static BigDecimal Remainder(BigDecimal a, BigDecimal b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            DivideAndRemainder(a, b, out BigDecimal remainder);
            return remainder;
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this % divisor}.
         * <para/>
         * The remainder is defined as {@code this -
         * this.divideToIntegralValue(divisor) * divisor}.
         * <para/>
         * The specified rounding mode {@code mc} is used for the division only.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @param mc
         *            rounding mode and precision to be used.
         * @return {@code this % divisor}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @throws ArithmeticException
         *             if {@code mc.getPrecision() > 0} and the result of {@code
         *             this.divideToIntegralValue(divisor, mc)} requires more digits
         *             to be represented.
         */
        public static BigDecimal Remainder(BigDecimal a, BigDecimal b, MathContext context)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));

            DivideAndRemainder(a, b, context, out BigDecimal remainder);
            return remainder;

        }

        /**
         * Returns a {@code BigDecimal} array which contains the integral part of
         * {@code this / divisor} at index 0 and the remainder {@code this %
         * divisor} at index 1. The quotient is rounded down towards zero to the
         * next integer.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @return {@code [this.divideToIntegralValue(divisor),
         *         this.remainder(divisor)]}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @see #divideToIntegralValue
         * @see #remainder
         */
        public static BigDecimal DivideAndRemainder(BigDecimal dividend, BigDecimal divisor, out BigDecimal remainder)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));

            var quotient = DivideToIntegralValue(dividend, divisor);
            remainder = Subtract(dividend, Multiply(quotient, divisor));
            return quotient;
        }

        /**
         * Returns a {@code BigDecimal} array which contains the integral part of
         * {@code this / divisor} at index 0 and the remainder {@code this %
         * divisor} at index 1. The quotient is rounded down towards zero to the
         * next integer. The rounding mode passed with the parameter {@code mc} is
         * not considered. But if the precision of {@code mc > 0} and the integral
         * part requires more digits, then an {@code ArithmeticException} is thrown.
         *
         * @param divisor
         *            value by which {@code this} is divided.
         * @param mc
         *            math context which determines the maximal precision of the
         *            result.
         * @return {@code [this.divideToIntegralValue(divisor),
         *         this.remainder(divisor)]}.
         * @throws NullPointerException
         *             if {@code divisor == null}.
         * @throws ArithmeticException
         *             if {@code divisor == 0}.
         * @see #divideToIntegralValue
         * @see #remainder
         */
        public static BigDecimal DivideAndRemainder(BigDecimal dividend, BigDecimal divisor, MathContext mc, out BigDecimal remainder)
        {
            if (dividend is null)
                throw new ArgumentNullException(nameof(dividend));
            if (divisor is null)
                throw new ArgumentNullException(nameof(divisor));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            var quotient = DivideToIntegralValue(dividend, divisor, mc);
            remainder = Subtract(dividend, Multiply(quotient, divisor));
            return quotient;
        }

        /**
        * Returns a new {@code BigDecimal} whose value is {@code this ^ n}. The
        * scale of the result is {@code n} times the scales of {@code this}.
        * <para/>
        * {@code x.pow(0)} returns {@code 1}, even if {@code x == 0}.
        * <para/>
        * Implementation Note: The implementation is based on the ANSI standard
        * X3.274-1996 algorithm.
        *
        * @param n
        *            exponent to which {@code this} is raised.
        * @return {@code this ^ n}.
        * @throws ArithmeticException
        *             if {@code n &lt; 0} or {@code n &gt; 999999999}.
        */
        public static BigDecimal Pow(BigDecimal number, int n)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));

            if (n == 0)
            {
                return One;
            }
            if ((n < 0) || (n > 999999999))
            {
                // math.07=Invalid Operation
                throw new ArithmeticException(Messages.math07); //$NON-NLS-1$
            }
            long newScale = number.Scale * (long)n;
            // Let be: this = [u,s]   so:  this^n = [u^n, s*n]
            return ((number.IsZero)
                ? GetZeroScaledBy(newScale)
                : new BigDecimal(BigInteger.Pow(number.UnscaledValue, n), ToIntScale(newScale)));
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this ^ n}. The
         * result is rounded according to the passed context {@code mc}.
         * <para/>
         * Implementation Note: The implementation is based on the ANSI standard
         * X3.274-1996 algorithm.
         *
         * @param n
         *            exponent to which {@code this} is raised.
         * @param mc
         *            rounding mode and precision for the result of this operation.
         * @return {@code this ^ n}.
         * @throws ArithmeticException
         *             if {@code n &lt; 0} or {@code n &gt; 999999999}.
         */
        public static BigDecimal Pow(BigDecimal number, int n, MathContext mc)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            // The ANSI standard X3.274-1996 algorithm
            int m = Math.Abs(n);
            int mcPrecision = mc.Precision;
            int elength = (int)Math.Log10(m) + 1; // decimal digits in 'n'
            int oneBitMask; // mask of bits
            BigDecimal accum; // the single accumulator
            MathContext newPrecision = mc; // MathContext by default

            // In particular cases, it reduces the problem to call the other 'pow()'
            if ((n == 0) || ((number.IsZero) && (n > 0)))
            {
                return Pow(number, n);
            }
            if ((m > 999999999) || ((mcPrecision == 0) && (n < 0))
                || ((mcPrecision > 0) && (elength > mcPrecision)))
            {
                // math.07=Invalid Operation
                throw new ArithmeticException(Messages.math07); //$NON-NLS-1$
            }
            if (mcPrecision > 0)
            {
                newPrecision = new MathContext(mcPrecision + elength + 1,
                    mc.RoundingMode);
            }
            // The result is calculated as if 'n' were positive
            accum = RoundImpl(number, newPrecision);
            oneBitMask = m.HighestOneBit() >> 1;

            while (oneBitMask > 0)
            {
                accum = Multiply(accum, accum, newPrecision);
                if ((m & oneBitMask) == oneBitMask)
                {
                    accum = Multiply(accum, number, newPrecision);
                }
                oneBitMask >>= 1;
            }
            // If 'n' is negative, the value is divided into 'ONE'
            if (n < 0)
            {
                accum = Divide(One, accum, newPrecision);
            }
            // The final value is rounded to the destination precision
            accum.InplaceRound(mc);
            return accum;
        }

        /**
        * Returns a new {@code BigDecimal} whose value is the absolute value of
        * {@code this}. The scale of the result is the same as the scale of this.
        *
        * @return {@code abs(this)}
        */
        public static BigDecimal Abs(BigDecimal value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return ((value.Sign < 0) ? -value : value);
        }

        /**
        * Returns a new {@code BigDecimal} whose value is the absolute value of
        * {@code this}. The result is rounded according to the passed context
        * {@code mc}.
        *
        * @param mc
        *            rounding mode and precision for the result of this operation.
        * @return {@code abs(this)}
        */
        public static BigDecimal Abs(BigDecimal value, MathContext mc)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            return Abs(RoundImpl(value, mc));
        }

        /**
         * Returns a new {@code BigDecimal} whose value is the {@code -this}. The
         * scale of the result is the same as the scale of this.
         *
         * @return {@code -this}
         */
        public static BigDecimal Negate(BigDecimal number)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));

            if (number.BitLength < 63 || (number.BitLength == 63 && number.SmallValue != long.MinValue))
            {
                return BigDecimal.GetInstance(-number.SmallValue, number.Scale);
            }

            return new BigDecimal(-number.UnscaledValue, number.Scale);
        }

        /**
        * Returns a new {@code BigDecimal} whose value is the {@code -this}. The
        * result is rounded according to the passed context {@code mc}.
        *
        * @param mc
        *            rounding mode and precision for the result of this operation.
        * @return {@code -this}
        */
        public static BigDecimal Negate(BigDecimal number, MathContext mc)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            return Negate(RoundImpl(number, mc));
        }

        /**
        * Returns a new {@code BigDecimal} whose value is {@code +this}. The scale
        * of the result is the same as the scale of this.
        *
        * @return {@code this}
        */
        public static BigDecimal Plus(BigDecimal number)
        {
            return number; // ICU4N TODO: This doesn't look right
        }

        /// <remarks>
        /// Returns a new <see cref="BigDecimal"/> whose value is <c>+this</c>.
        /// </remarks>
        /// <param name="number"></param>
        /// <param name="mc">Rounding mode and precision for the result of this operation.</param>
        /// <remarks>
        /// The result is rounded according to the passed context <paramref name="mc"/>.
        /// </remarks>
        /// <returns>
        /// Returns this decimal value rounded.
        /// </returns>
        public static BigDecimal Plus(BigDecimal number, MathContext mc)
        {
            return Round(number, mc); // ICU4N TODO: This doesn't look right
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this}, rounded
         * according to the passed context {@code mc}.
         * <para/>
         * If {@code mc.precision = 0}, then no rounding is performed.
         * <para/>
         * If {@code mc.precision > 0} and {@code mc.roundingMode == UNNECESSARY},
         * then an {@code ArithmeticException} is thrown if the result cannot be
         * represented exactly within the given precision.
         *
         * @param mc
         *            rounding mode and precision for the result of this operation.
         * @return {@code this} rounded according to the passed context.
         * @throws ArithmeticException
         *             if {@code mc.precision > 0} and {@code mc.roundingMode ==
         *             UNNECESSARY} and this cannot be represented within the given
         *             precision.
         */
        public static BigDecimal Round(BigDecimal value, MathContext mc)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (mc is null)
                throw new ArgumentNullException(nameof(mc));

            return RoundImpl(value, mc);
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static BigDecimal RoundImpl(BigDecimal value, MathContext mc)
        {
            var thisBD = new BigDecimal(value.UnscaledValue, value.Scale);

            thisBD.InplaceRound(mc);
            return thisBD;
        }

        /**
         * Returns a new {@code BigDecimal} instance with the specified scale.
         * <para/>
         * If the new scale is greater than the old scale, then additional zeros are
         * added to the unscaled value. In this case no rounding is necessary.
         * <para/>
         * If the new scale is smaller than the old scale, then trailing digits are
         * removed. If these trailing digits are not zero, then the remaining
         * unscaled value has to be rounded. For this rounding operation the
         * specified rounding mode is used.
         *
         * @param newScale
         *            scale of the result returned.
         * @param roundingMode
         *            rounding mode to be used to round the result.
         * @return a new {@code BigDecimal} instance with the specified scale.
         * @throws NullPointerException
         *             if {@code roundingMode == null}.
         * @throws ArithmeticException
         *             if {@code roundingMode == ROUND_UNNECESSARY} and rounding is
         *             necessary according to the given scale.
         */
        public static BigDecimal SetScale(BigDecimal number, int newScale, RoundingMode roundingMode)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));
            if (!roundingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(roundingMode), string.Format(Messages.ArgumentOutOfRange_Enum, roundingMode, nameof(RoundingMode)));

            long diffScale = newScale - (long)number.Scale;
            // Let be:  'number' = [u,s]
            if (diffScale == 0)
            {
                return number;
            }
            if (diffScale > 0)
            {
                // return  [u * 10^(s2 - s), newScale]
                if (diffScale < LongTenPow.Length &&
                    (number.BitLength + LongTenPowBitLength[(int)diffScale]) < 64)
                {
                    return BigDecimal.GetInstance(number.SmallValue * LongTenPow[(int)diffScale], newScale);
                }
                return new BigDecimal(Multiplication.MultiplyByTenPow(number.UnscaledValue, (int)diffScale), newScale);
            }
            // diffScale < 0
            // return  [u,s] / [1,newScale]  with the appropriate scale and rounding
            if (number.BitLength < 64 && -diffScale < LongTenPow.Length)
            {
                return DividePrimitiveLongs(number.SmallValue, LongTenPow[(int)-diffScale], newScale, roundingMode);
            }

            return DivideBigIntegers(number.UnscaledValue, Multiplication.PowerOf10(-diffScale), newScale, roundingMode);
        }


        /**
         * Returns a new {@code BigDecimal} instance with the specified scale. If
         * the new scale is greater than the old scale, then additional zeros are
         * added to the unscaled value. If the new scale is smaller than the old
         * scale, then trailing zeros are removed. If the trailing digits are not
         * zeros then an ArithmeticException is thrown.
         * <para/>
         * If no exception is thrown, then the following equation holds: {@code
         * x.setScale(s).compareTo(x) == 0}.
         *
         * @param newScale
         *            scale of the result returned.
         * @return a new {@code BigDecimal} instance with the specified scale.
         * @throws ArithmeticException
         *             if rounding would be necessary.
         */
        public static BigDecimal SetScale(BigDecimal number, int newScale)
        {
            return SetScale(number, newScale, RoundingMode.Unnecessary);
        }

        /**
        * Returns a new {@code BigDecimal} instance where the decimal point has
        * been moved {@code n} places to the left. If {@code n &lt; 0} then the
        * decimal point is moved {@code -n} places to the right.
        * <para/>
        * The result is obtained by changing its scale. If the scale of the result
        * becomes negative, then its precision is increased such that the scale is
        * zero.
        * <para/>
        * Note, that {@code movePointLeft(0)} returns a result which is
        * mathematically equivalent, but which has {@code scale &gt;= 0}.
        *
        * @param n
        *            number of placed the decimal point has to be moved.
        * @return {@code this * 10^(-n}).
        */
        public static BigDecimal MovePointLeft(BigDecimal number, int n)
        {
            return MovePoint(number, number.Scale + (long)n);
        }

        public static BigDecimal MovePoint(BigDecimal number, long newScale)
        {
            if (number.IsZero)
            {
                return GetZeroScaledBy(Math.Max(newScale, 0));
            }
            /* When:  'n'== Integer.MIN_VALUE  isn't possible to call to movePointRight(-n)  
             * since  -Integer.MIN_VALUE == Integer.MIN_VALUE */
            if (newScale >= 0)
            {
                if (number.BitLength < 64)
                {
                    return BigDecimal.GetInstance(number.SmallValue, ToIntScale(newScale));
                }
                return new BigDecimal(number.UnscaledValue, ToIntScale(newScale));
            }
            if (-newScale < LongTenPow.Length &&
                number.BitLength + LongTenPowBitLength[(int)-newScale] < 64)
            {
                return BigDecimal.GetInstance(number.SmallValue * LongTenPow[(int)-newScale], 0);
            }
            return new BigDecimal(Multiplication.MultiplyByTenPow(number.UnscaledValue, (int)-newScale), 0);
        }

        /**
         * Returns a new {@code BigDecimal} instance where the decimal point has
         * been moved {@code n} places to the right. If {@code n &lt; 0} then the
         * decimal point is moved {@code -n} places to the left.
         * <para/>
         * The result is obtained by changing its scale. If the scale of the result
         * becomes negative, then its precision is increased such that the scale is
         * zero.
         * <para/>
         * Note, that {@code movePointRight(0)} returns a result which is
         * mathematically equivalent, but which has scale &gt;= 0.
         *
         * @param n
         *            number of placed the decimal point has to be moved.
         * @return {@code this * 10^n}.
         */
        public static BigDecimal MovePointRight(BigDecimal number, int n)
        {
            return MovePoint(number, number.Scale - (long)n);
        }

        /**
         * Returns a new {@code BigDecimal} whose value is {@code this} 10^{@code n}.
         * The scale of the result is {@code this.scale()} - {@code n}.
         * The precision of the result is the precision of {@code this}.
         * <para/>
         * This method has the same effect as {@link #movePointRight}, except that
         * the precision is not changed.
         *
         * @param n
         *            number of places the decimal point has to be moved.
         * @return {@code this * 10^n}
         */
        public static BigDecimal ScaleByPowerOfTen(BigDecimal number, int n)
        {
            long newScale = number.Scale - (long)n;
            if (number.BitLength < 64)
            {
                //Taking care when a 0 is to be scaled
                if (number.SmallValue == 0)
                {
                    return GetZeroScaledBy(newScale);
                }

                return BigDecimal.GetInstance(number.SmallValue, ToIntScale(newScale));
            }

            return new BigDecimal(number.UnscaledValue, ToIntScale(newScale));
        }

        /**
         * Returns a new {@code BigDecimal} instance with the same value as {@code
         * this} but with a unscaled value where the trailing zeros have been
         * removed. If the unscaled value of {@code this} has n trailing zeros, then
         * the scale and the precision of the result has been reduced by n.
         *
         * @return a new {@code BigDecimal} instance equivalent to this where the
         *         trailing zeros of the unscaled value have been removed.
         */
        public static BigDecimal StripTrailingZeros(BigDecimal value) // ICU4N TODO: Change back to instance method? This seems inconvenient and there is no operator for this.
        {
            int i = 1; // 1 <= i <= 18
            int lastPow = TenPow.Length - 1;
            long newScale = value.Scale;

            if (value.IsZero)
            {
                return BigDecimal.Parse("0", CultureInfo.InvariantCulture); // ICU4N TODO: Why not use Zero here?
            }
            BigInteger strippedBI = value.UnscaledValue;
            BigInteger quotient;
            BigInteger remainder;

            // while the number is even...
            while (!BigInteger.TestBit(strippedBI, 0))
            {
                // To divide by 10^i
                quotient = BigInteger.DivideAndRemainder(strippedBI, TenPow[i], out remainder);
                // To look the remainder
                if (remainder.Sign == 0)
                {
                    // To adjust the scale
                    newScale -= i;
                    if (i < lastPow)
                    {
                        // To set to the next power
                        i++;
                    }
                    strippedBI = quotient;
                }
                else
                {
                    if (i == 1)
                    {
                        // 'this' has no more trailing zeros
                        break;
                    }
                    // To set to the smallest power of ten
                    i = 1;
                }
            }
            return new BigDecimal(strippedBI, ToIntScale(newScale));
        }


        private static BigDecimal AddAndMult10(BigDecimal thisValue, BigDecimal augend, int diffScale)
        {
            if (diffScale < LongTenPow.Length &&
                Math.Max(thisValue.BitLength, augend.BitLength + LongTenPowBitLength[diffScale]) + 1 < 64)
            {
                return BigDecimal.GetInstance(thisValue.SmallValue + augend.SmallValue * LongTenPow[diffScale], thisValue.Scale);
            }
            return new BigDecimal(
                thisValue.UnscaledValue + Multiplication.MultiplyByTenPow(augend.UnscaledValue, diffScale),
                thisValue.Scale);
        }


        /**
         * Returns the unit in the last place (ULP) of this {@code BigDecimal}
         * instance. An ULP is the distance to the nearest big decimal with the same
         * precision.
         * <para/>
         * The amount of a rounding error in the evaluation of a floating-point
         * operation is often expressed in ULPs. An error of 1 ULP is often seen as
         * a tolerable error.
         * <para/>
         * For class {@code BigDecimal}, the ULP of a number is simply 10^(-scale).
         * <para/>
         * For example, {@code new BigDecimal(0.1).ulp()} returns {@code 1E-55}.
         *
         * @return unit in the last place (ULP) of this {@code BigDecimal} instance.
         */
        public static BigDecimal Ulp(BigDecimal value)
        {
            return BigDecimal.GetInstance(1, value.Scale);
        }
    }
}
