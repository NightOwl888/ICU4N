using ICU4N.Impl;
using ICU4N.Support.Numerics;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static ICU4N.Text.PluralRules;

namespace ICU4N.Numerics
{
    /// <summary>
    /// Represents numbers and digit display properties using Binary Coded Decimal (BCD).
    /// </summary>
    internal abstract class DecimalQuantity_AbstractBCD : IDecimalQuantity // ICU4N TODO: API - this was public in ICU4J
    {
        /**
          * The power of ten corresponding to the least significant digit in the BCD. For example, if this
          * object represents the number "3.14", the BCD will be "0x314" and the scale will be -2.
          *
          * <p>Note that in {@link java.math.BigDecimal}, the scale is defined differently: the number of
          * digits after the decimal place, which is the negative of our definition of scale.
          */
        protected int scale;

        /**
         * The number of digits in the BCD. For example, "1007" has BCD "0x1007" and precision 4. The
         * maximum precision is 16 since a long can hold only 16 digits.
         *
         * <p>This value must be re-calculated whenever the value in bcd changes by using {@link
         * #computePrecisionAndCompact()}.
         */
        protected int precision;

        /**
         * A bitmask of properties relating to the number represented by this object.
         *
         * @see #NEGATIVE_FLAG
         * @see #INFINITY_FLAG
         * @see #NAN_FLAG
         */
        protected byte flags;

        protected static readonly int NegativeFlag = 1;
        protected static readonly int InfinityFlag = 2;
        protected static readonly int NaNFlag = 4;

        // The following three fields relate to the double-to-ascii fast path algorithm.
        // When a double is given to DecimalQuantityBCD, it is converted to using a fast algorithm. The
        // fast algorithm guarantees correctness to only the first ~12 digits of the double. The process
        // of rounding the number ensures that the converted digits are correct, falling back to a slow-
        // path algorithm if required.  Therefore, if a DecimalQuantity is constructed from a double, it
        // is *required* that roundToMagnitude(), roundToIncrement(), or roundToInfinity() is called. If
        // you don't round, assertions will fail in certain other methods if you try calling them.

        /**
         * The original number provided by the user and which is represented in BCD. Used when we need to
         * re-compute the BCD for an exact double representation.
         */
        protected double origDouble;

        /**
         * The change in magnitude relative to the original double. Used when we need to re-compute the
         * BCD for an exact double representation.
         */
        protected int origDelta;

        /**
         * Whether the value in the BCD comes from the double fast path without having been rounded to
         * ensure correctness
         */
        protected bool isApproximate;

        // Four positions: left optional '(', left required '[', right required ']', right optional ')'.
        // These four positions determine which digits are displayed in the output string.  They do NOT
        // affect rounding.  These positions are internal-only and can be specified only by the public
        // endpoints like setFractionLength, setIntegerLength, and setSignificantDigits, among others.
        //
        //   * Digits between lReqPos and rReqPos are in the "required zone" and are always displayed.
        //   * Digits between lOptPos and rOptPos but outside the required zone are in the "optional zone"
        //     and are displayed unless they are trailing off the left or right edge of the number and
        //     have a numerical value of zero.  In order to be "trailing", the digits need to be beyond
        //     the decimal point in their respective directions.
        //   * Digits outside of the "optional zone" are never displayed.
        //
        // See the table below for illustrative examples.
        //
        // +---------+---------+---------+---------+------------+------------------------+--------------+
        // | lOptPos | lReqPos | rReqPos | rOptPos |   number   |        positions       | en-US string |
        // +---------+---------+---------+---------+------------+------------------------+--------------+
        // |    5    |    2    |   -1    |   -5    |   1234.567 |     ( 12[34.5]67  )    |   1,234.567  |
        // |    3    |    2    |   -1    |   -5    |   1234.567 |      1(2[34.5]67  )    |     234.567  |
        // |    3    |    2    |   -1    |   -2    |   1234.567 |      1(2[34.5]6)7      |     234.56   |
        // |    6    |    4    |    2    |   -5    | 123456789. |  123(45[67]89.     )   | 456,789.     |
        // |    6    |    4    |    2    |    1    | 123456789. |     123(45[67]8)9.     | 456,780.     |
        // |   -1    |   -1    |   -3    |   -4    | 0.123456   |     0.1([23]4)56       |        .0234 |
        // |    6    |    4    |   -2    |   -2    |     12.3   |     (  [  12.3 ])      |    0012.30   |
        // +---------+---------+---------+---------+------------+------------------------+--------------+
        //
        protected int lOptPos = int.MaxValue;
        protected int lReqPos = 0;
        protected int rReqPos = 0;
        protected int rOptPos = int.MinValue;

        public virtual void CopyFrom(IDecimalQuantity other)
        {
            CopyBcdFrom(other);
            if (other is DecimalQuantity_AbstractBCD _other)
            {
                lOptPos = _other.lOptPos;
                lReqPos = _other.lReqPos;
                rReqPos = _other.rReqPos;
                rOptPos = _other.rOptPos;
                scale = _other.scale;
                precision = _other.precision;
                flags = _other.flags;
                origDouble = _other.origDouble;
                origDelta = _other.origDelta;
                isApproximate = _other.isApproximate;
            }
            else
            {
                throw new ArgumentException($"{nameof(other)} must be type {typeof(DecimalQuantity_AbstractBCD)}"); // ICU4N specific - avoid type cast exception
            }
        }

        public DecimalQuantity_AbstractBCD Clear()
        {
            lOptPos = int.MaxValue;
            lReqPos = 0;
            rReqPos = 0;
            rOptPos = int.MinValue;
            flags = 0;
            SetBcdToZero(); // sets scale, precision, hasDouble, origDouble, origDelta, and BCD data
            return this;
        }

        public virtual void SetIntegerLength(int minInt, int maxInt)
        {
            // Validation should happen outside of DecimalQuantity, e.g., in the Rounder class.
            Debug.Assert(minInt >= 0);
            Debug.Assert(maxInt >= minInt); // ICU4N TODO: Make into guard clauses?

            // Save values into internal state
            // Negation is safe for minFrac/maxFrac because -Integer.MAX_VALUE > Integer.MIN_VALUE
            lOptPos = maxInt;
            lReqPos = minInt;
        }

        public virtual void SetFractionLength(int minFrac, int maxFrac)
        {
            // Validation should happen outside of DecimalQuantity, e.g., in the Rounder class.
            Debug.Assert(minFrac >= 0);
            Debug.Assert(maxFrac >= minFrac); // ICU4N TODO: Make into guard clauses?

            // Save values into internal state
            // Negation is safe for minFrac/maxFrac because -Integer.MAX_VALUE > Integer.MIN_VALUE
            rReqPos = -minFrac;
            rOptPos = -maxFrac;
        }

        public virtual long PositionFingerprint
        {
            get
            {
                long fingerprint = 0;
                fingerprint ^= lOptPos;
                fingerprint ^= (lReqPos << 16);
                fingerprint ^= ((long)rReqPos << 32);
                fingerprint ^= ((long)rOptPos << 48);
                return fingerprint;
            }
        }

        public virtual void RoundToIncrement(BigMath.BigDecimal roundingIncrement, BigMath.MathContext mathContext)
        {
            // TODO: Avoid converting back and forth to BigDecimal.
            BigMath.BigDecimal temp = ToBigDecimal();
            temp = BigMath.BigMath.Divide(roundingIncrement, 0, mathContext.RoundingMode);
            temp = temp * roundingIncrement;
            temp = BigMath.BigMath.Round(temp, mathContext);
            //temp =
            //    temp.Divide(roundingIncrement, 0, mathContext.RoundingMode)
            //        .Multiply(roundingIncrement)
            //        .Round(mathContext);
            if (temp.Sign == 0)
            {
                SetBcdToZero(); // keeps negative flag for -0.0
            }
            else
            {
                SetToBigDecimal(temp);
            }
        }

        public virtual void MultiplyBy(BigMath.BigDecimal multiplicand)
        {
            if (IsInfinity || IsZero || IsNaN)
            {
                return;
            }
            BigMath.BigDecimal temp = ToBigDecimal();
            //temp = temp.Multiply(multiplicand);
            temp = temp * multiplicand;
            SetToBigDecimal(temp);
        }

        public virtual int GetMagnitude()
        {
            if (precision == 0)
            {
                throw new ArithmeticException("Magnitude is not well-defined for zero");
            }
            else
            {
                return scale + precision - 1;
            }
        }

        public virtual void AdjustMagnitude(int delta)
        {
            if (precision != 0)
            {
                scale += delta;
                origDelta += delta;
            }
        }

        public virtual StandardPlural GetStandardPlural(PluralRules rules)
        {
            if (rules == null)
            {
                // Fail gracefully if the user didn't provide a PluralRules
                return StandardPlural.Other;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                string ruleString = rules.Select(this);
#pragma warning restore CS0618 // Type or member is obsolete
                return StandardPluralUtil.OrOtherFromString(ruleString);
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public virtual double GetPluralOperand(Operand operand)
        {
            // If this assertion fails, you need to call roundToInfinity() or some other rounding method.
            // See the comment at the top of this file explaining the "isApproximate" field.
            Debug.Assert(!isApproximate);

            return operand switch
            {
                Operand.i => ToLong(),
                Operand.f => ToFractionLong(true),
                Operand.t => ToFractionLong(false),
                Operand.v => FractionCount,
                Operand.w => FractionCountWithoutTrailingZeros,
                _ => Math.Abs(ToDouble()),
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public virtual void PopulateUFieldPosition(FieldPosition fp)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (fp is UFieldPosition uFieldPosition)
            {
                uFieldPosition.SetFractionDigits((int)GetPluralOperand(Operand.v), (long)GetPluralOperand(Operand.f));
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }


        public virtual int UpperDisplayMagnitude
        {
            get
            {
                // If this assertion fails, you need to call roundToInfinity() or some other rounding method.
                // See the comment at the top of this file explaining the "isApproximate" field.
                Debug.Assert(!isApproximate);

                int magnitude = scale + precision;
                int result = (lReqPos > magnitude) ? lReqPos : (lOptPos < magnitude) ? lOptPos : magnitude;
                return result - 1;
            }
        }

        public virtual int LowerDisplayMagnitude
        {
            get
            {
                // If this assertion fails, you need to call roundToInfinity() or some other rounding method.
                // See the comment at the top of this file explaining the "isApproximate" field.
                Debug.Assert(!isApproximate);

                int magnitude = scale;
                int result = (rReqPos < magnitude) ? rReqPos : (rOptPos > magnitude) ? rOptPos : magnitude;
                return result;
            }
        }

        public virtual byte GetDigit(int magnitude)
        {
            // If this assertion fails, you need to call roundToInfinity() or some other rounding method.
            // See the comment at the top of this file explaining the "isApproximate" field.
            Debug.Assert(!isApproximate);

            return GetDigitPos(magnitude - scale);
        }

        private int FractionCount => -LowerDisplayMagnitude;

        private int FractionCountWithoutTrailingZeros => Math.Max(-scale, 0);


        public virtual bool IsNegative => (flags & NegativeFlag) != 0;


        public virtual bool IsInfinity => (flags & InfinityFlag) != 0;


        public virtual bool IsNaN => (flags & NaNFlag) != 0;


        public virtual bool IsZero => precision == 0;

        public abstract int MaxRepresentableDigits { get; }

        public void SetToInt(int n)
        {
            SetBcdToZero();
            flags = 0;
            if (n < 0)
            {
                flags = (byte)(flags | NegativeFlag);
                n = -n;
            }
            if (n != 0)
            {
                SetToIntImpl(n);
                Compact();
            }
        }

        private void SetToIntImpl(int n)
        {
            if (n == int.MinValue)
            {
                ReadLongToBcd(-(long)n);
            }
            else
            {
                ReadIntToBcd(n);
            }
        }

        public void SetToLong(long n)
        {
            SetBcdToZero();
            flags = 0;
            if (n < 0)
            {
                flags = (byte)(flags | NegativeFlag);
                n = -n;
            }
            if (n != 0)
            {
                SetToLongImpl(n);
                Compact();
            }
        }

        private void SetToLongImpl(long n)
        {
            if (n == long.MinValue)
            {
                ReadBigIntegerToBcd(-n);
            }
            else if (n <= int.MaxValue)
            {
                ReadIntToBcd((int)n);
            }
            else
            {
                ReadLongToBcd(n);
            }
        }

        public void SetToBigInteger(BigMath.BigInteger n)
        {
            SetBcdToZero();
            flags = 0;
            if (n.Sign == -1)
            {
                flags = (byte)(flags | NegativeFlag);
                n = -n;
            }
            if (n.Sign != 0)
            {
                SetToBigIntegerImpl(n);
                Compact();
            }
        }

        private void SetToBigIntegerImpl(BigMath.BigInteger n)
        {
            int bitLength = n.ToByteArray().Length * sizeof(byte);
            if (bitLength < 32)
            {
                ReadIntToBcd((int)n);
            }
            else if (bitLength < 64)
            {
                ReadLongToBcd((long)n);
            }
            else
            {
                ReadBigIntegerToBcd(n);
            }
        }

        /**
         * Sets the internal BCD state to represent the value in the given double.
         *
         * @param n The value to consume.
         */
        public void SetToDouble(double n)
        {
            SetBcdToZero();
            flags = 0;
            // Double.compare() handles +0.0 vs -0.0
            if (Comparer<double>.Default.Compare(n, 0.0) < 0)
            {
                flags = (byte)(flags | NegativeFlag);
                n = -n;
            }
            if (double.IsNaN(n))
            {
                flags = (byte)(flags | NaNFlag);
            }
            else if (double.IsInfinity(n))
            {
                flags = (byte)(flags | InfinityFlag);
            }
            else if (n != 0)
            {
                SetToDoubleFast(n);
                Compact();
            }
        }

        private static readonly double[] DOUBLE_MULTIPLIERS = {
            1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16,
            1e17, 1e18, 1e19, 1e20, 1e21
        };

        /**
         * Uses double multiplication and division to get the number into integer space before converting
         * to digits. Since double arithmetic is inexact, the resulting digits may not be accurate.
         */
        private void SetToDoubleFast(double n)
        {
            isApproximate = true;
            origDouble = n;
            origDelta = 0;

            // NOTE: Unlike ICU4C, doubles are always IEEE 754 doubles.
            long ieeeBits = BitConversion.DoubleToInt64Bits(n);
            int exponent = (int)((ieeeBits & 0x7ff0000000000000L) >> 52) - 0x3ff;

            // Not all integers can be represented exactly for exponent > 52
            if (exponent <= 52 && (long)n == n)
            {
                SetToLongImpl((long)n);
                return;
            }

            // 3.3219... is log2(10)
            int fracLength = (int)((52 - exponent) / 3.32192809489);
            if (fracLength >= 0)
            {
                int i = fracLength;
                // 1e22 is the largest exact double.
                for (; i >= 22; i -= 22) n *= 1e22;
                n *= DOUBLE_MULTIPLIERS[i];
            }
            else
            {
                int i = fracLength;
                // 1e22 is the largest exact double.
                for (; i <= -22; i += 22) n /= 1e22;
                n /= DOUBLE_MULTIPLIERS[-i];
            }
            long result = (long)Math.Ceiling(n); // ICU4N: AwayFromZero rounding mode is implemented as the Math.Ceiling() method. See: https://stackoverflow.com/a/70596191
            if (result != 0)
            {
                SetToLongImpl(result);
                scale -= fracLength;
            }
        }

        /**
         * Uses Double.toString() to obtain an exact accurate representation of the double, overwriting it
         * into the BCD. This method can be called at any point after {@link #_setToDoubleFast} while
         * {@link #isApproximate} is still true.
         */
        private void ConvertToAccurateDouble()
        {
            double n = origDouble;
            Debug.Assert(n != 0);
            int delta = origDelta;
            SetBcdToZero();

            // Call the slow oracle function (Double.toString in Java, sprintf in C++).
            string dstr = DoubleConverter.ToExactString(n); //Double.toString(n); // ICU4N TODO: Check this.

            if (dstr.IndexOf('E') != -1)
            {
                // Case 1: Exponential notation.
                Debug.Assert(dstr.IndexOf('.') == 1);
                int expPos = dstr.IndexOf('E');
                // ICU4N TODO: Should we use TryParse?
                SetToLongImpl(long.Parse(dstr[0] + dstr.Substring(2, expPos - 2), CultureInfo.InvariantCulture)); // ICU4N: Corrected 2nd arg.
                scale += int.Parse(dstr.Substring(expPos + 1), CultureInfo.InvariantCulture) - (expPos - 1) + 1;
            }
            else if (dstr[0] == '0')
            {
                // Case 2: Fraction-only number.
                Debug.Assert(dstr.IndexOf('.') == 1);
                SetToLongImpl(long.Parse(dstr.Substring(2), CultureInfo.InvariantCulture));
                scale += 2 - dstr.Length;
            }
            else if (dstr[dstr.Length - 1] == '0')
            {
                // Case 3: Integer-only number.
                // Note: this path should not normally happen, because integer-only numbers are captured
                // before the approximate double logic is performed.
                Debug.Assert(dstr.IndexOf('.') == dstr.Length - 2);
                Debug.Assert(dstr.Length - 2 <= 18);
                SetToLongImpl(long.Parse(dstr.Substring(0, dstr.Length - 2), CultureInfo.InvariantCulture)); // ICU4N: Checked 2nd arg
                                                                                                             // no need to adjust scale
            }
            else
            {
                // Case 4: Number with both a fraction and an integer.
                int decimalPos = dstr.IndexOf('.');
                SetToLongImpl(long.Parse(dstr.Substring(0, decimalPos) + dstr.Substring(decimalPos + 1))); // ICU4N: Checked 2nd arg
                scale += decimalPos - dstr.Length + 1;
            }

            scale += delta;
            Compact();
            explicitExactDouble = true;
        }

        /**
         * Whether this {@link DecimalQuantity_DualStorageBCD} has been explicitly converted to an exact double. true if
         * backed by a double that was explicitly converted via convertToAccurateDouble; false otherwise.
         * Used for testing.
         *
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //[Obsolete("This API is ICU internal only.")] // ICU4N: Marked internal instead of public
        internal bool explicitExactDouble = false;

        /**
         * Sets the internal BCD state to represent the value in the given BigDecimal.
         *
         * @param n The value to consume.
         */
        public virtual void SetToBigDecimal(ICU4N.Numerics.BigMath.BigDecimal n)
        {
            SetBcdToZero();
            flags = 0;
            if (n.Sign == -1)
            {
                flags = (byte)(flags | NegativeFlag);
                n = -n;
            }
            if (n.Sign != 0)
            {
                SetToBigDecimalImpl(n);
                Compact();
            }
        }

        private void SetToBigDecimalImpl(ICU4N.Numerics.BigMath.BigDecimal n)
        {
            int fracLength = n.Scale;
            //n = n.ScaleByPowerOfTen(fracLength);
            n = BigMath.BigMath.ScaleByPowerOfTen(n, fracLength);
            //BigInteger bi = (BigInteger)n.ToBigInteger();
            BigMath.BigInteger bi = n.ToBigInteger();
            SetToBigInteger(bi);
            scale -= fracLength;
        }

        /**
         * Returns a long approximating the internal BCD. A long can only represent the integral part of
         * the number.
         *
         * @return A double representation of the internal BCD.
         */
        protected virtual long ToLong()
        {
            long result = 0L;
            for (int magnitude = scale + precision - 1; magnitude >= 0; magnitude--)
            {
                result = result * 10 + GetDigitPos(magnitude - scale);
            }
            return result;
        }

        /**
         * This returns a long representing the fraction digits of the number, as required by PluralRules.
         * For example, if we represent the number "1.20" (including optional and required digits), then
         * this function returns "20" if includeTrailingZeros is true or "2" if false.
         */
        protected long ToFractionLong(bool includeTrailingZeros)
        {
            long result = 0L;
            int magnitude = -1;
            for (;
                (magnitude >= scale || (includeTrailingZeros && magnitude >= rReqPos))
                    && magnitude >= rOptPos;
                magnitude--)
            {
                result = result * 10 + GetDigitPos(magnitude - scale);
            }
            return result;
        }

        /**
         * Returns a double approximating the internal BCD. The double may not retain all of the
         * information encoded in the BCD if the BCD represents a number out of range of a double.
         *
         * @return A double representation of the internal BCD.
         */
        public virtual double ToDouble()
        {
            if (isApproximate)
            {
                return ToDoubleFromOriginal();
            }

            if (IsNaN)
            {
                return double.NaN;
            }
            else if (IsInfinity)
            {
                return IsNegative ? double.NegativeInfinity : double.PositiveInfinity;
            }

            long tempLong = 0L;
            int lostDigits = precision - Math.Min(precision, 17);
            for (int shift = precision - 1; shift >= lostDigits; shift--)
            {
                tempLong = tempLong * 10 + GetDigitPos(shift);
            }
            double result = tempLong;
            int _scale = scale + lostDigits;
            if (_scale >= 0)
            {
                // 1e22 is the largest exact double.
                int i = _scale;
                for (; i >= 22; i -= 22) result *= 1e22;
                result *= DOUBLE_MULTIPLIERS[i];
            }
            else
            {
                // 1e22 is the largest exact double.
                int i = _scale;
                for (; i <= -22; i += 22) result /= 1e22;
                result /= DOUBLE_MULTIPLIERS[-i];
            }
            if (IsNegative) result = -result;
            return result;
        }

        public virtual BigMath.BigDecimal ToBigDecimal()
        {
            if (isApproximate)
            {
                // Converting to a BigDecimal requires Double.toString().
                ConvertToAccurateDouble();
            }
            return BcdToBigDecimal();
        }

        protected double ToDoubleFromOriginal()
        {
            double result = origDouble;
            int delta = origDelta;
            if (delta >= 0)
            {
                // 1e22 is the largest exact double.
                for (; delta >= 22; delta -= 22) result *= 1e22;
                result *= DOUBLE_MULTIPLIERS[delta];
            }
            else
            {
                // 1e22 is the largest exact double.
                for (; delta <= -22; delta += 22) result /= 1e22;
                result /= DOUBLE_MULTIPLIERS[-delta];
            }
            if (IsNegative) result *= -1;
            return result;
        }

        private static int SafeSubtract(int a, int b)
        {
            int diff = a - b;
            if (b < 0 && diff < a) return int.MaxValue;
            if (b > 0 && diff > a) return int.MaxValue;
            return diff;
        }

        private const int SectionLowerEdge = -1;
        private const int SectionUpperEdge = -2;

        public virtual void RoundToMagnitude(int magnitude, BigMath.MathContext mathContext)
        {
            // The position in the BCD at which rounding will be performed; digits to the right of position
            // will be rounded away.
            // TODO: Andy: There was a test failure because of integer overflow here. Should I do
            // "safe subtraction" everywhere in the code?  What's the nicest way to do it?
            int position = SafeSubtract(magnitude, scale);

            // Enforce the number of digits required by the MathContext.
            int _mcPrecision = mathContext.Precision;
            if (magnitude == int.MaxValue
                || (_mcPrecision > 0 && precision - position > _mcPrecision))
            {
                position = precision - _mcPrecision;
            }

            if (position <= 0 && !isApproximate)
            {
                // All digits are to the left of the rounding magnitude.
            }
            else if (precision == 0)
            {
                // No rounding for zero.
            }
            else
            {
                // Perform rounding logic.
                // "leading" = most significant digit to the right of rounding
                // "trailing" = least significant digit to the left of rounding
                byte leadingDigit = GetDigitPos(SafeSubtract(position, 1));
                byte trailingDigit = GetDigitPos(position);

                // Compute which section of the number we are in.
                // EDGE means we are at the bottom or top edge, like 1.000 or 1.999 (used by doubles)
                // LOWER means we are between the bottom edge and the midpoint, like 1.391
                // MIDPOINT means we are exactly in the middle, like 1.500
                // UPPER means we are between the midpoint and the top edge, like 1.916
                RoundingUtils.Section section = RoundingUtils.Section.MidPoint; // SECTION_MIDPOINT;
                if (!isApproximate)
                {
                    if (leadingDigit < 5)
                    {
                        section = RoundingUtils.Section.Lower; //.SECTION_LOWER;
                    }
                    else if (leadingDigit > 5)
                    {
                        section = RoundingUtils.Section.Upper; //.SECTION_UPPER;
                    }
                    else
                    {
                        for (int p = SafeSubtract(position, 2); p >= 0; p--)
                        {
                            if (GetDigitPos(p) != 0)
                            {
                                section = RoundingUtils.Section.Upper; //.SECTION_UPPER;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int p = SafeSubtract(position, 2);
                    int minP = Math.Max(0, precision - 14);
                    if (leadingDigit == 0)
                    {
                        section = (RoundingUtils.Section)SectionLowerEdge;
                        for (; p >= minP; p--)
                        {
                            if (GetDigitPos(p) != 0)
                            {
                                section = RoundingUtils.Section.Lower; //.SECTION_LOWER;
                                break;
                            }
                        }
                    }
                    else if (leadingDigit == 4)
                    {
                        for (; p >= minP; p--)
                        {
                            if (GetDigitPos(p) != 9)
                            {
                                section = RoundingUtils.Section.Lower; //.SECTION_LOWER;
                                break;
                            }
                        }
                    }
                    else if (leadingDigit == 5)
                    {
                        for (; p >= minP; p--)
                        {
                            if (GetDigitPos(p) != 0)
                            {
                                section = RoundingUtils.Section.Upper;
                                break;
                            }
                        }
                    }
                    else if (leadingDigit == 9)
                    {
                        section = (RoundingUtils.Section)SectionUpperEdge;
                        for (; p >= minP; p--)
                        {
                            if (GetDigitPos(p) != 9)
                            {
                                section = RoundingUtils.Section.Upper;
                                break;
                            }
                        }
                    }
                    else if (leadingDigit < 5)
                    {
                        section = RoundingUtils.Section.Lower;
                    }
                    else
                    {
                        section = RoundingUtils.Section.Upper;
                    }

                    bool roundsAtMidpoint =
                        RoundingUtils.RoundsAtMidpoint(mathContext.RoundingMode);
                    if (SafeSubtract(position, 1) < precision - 14
                        || (roundsAtMidpoint && section == RoundingUtils.Section.MidPoint) //.SECTION_MIDPOINT)
                        || (!roundsAtMidpoint && section < 0 /* i.e. at upper or lower edge */))
                    {
                        // Oops! This means that we have to get the exact representation of the double, because
                        // the zone of uncertainty is along the rounding boundary.
                        ConvertToAccurateDouble();
                        RoundToMagnitude(magnitude, mathContext); // start over
                        return;
                    }

                    // Turn off the approximate double flag, since the value is now confirmed to be exact.
                    isApproximate = false;
                    origDouble = 0.0;
                    origDelta = 0;

                    if (position <= 0)
                    {
                        // All digits are to the left of the rounding magnitude.
                        return;
                    }

                    // Good to continue rounding.
                    if (section == (RoundingUtils.Section)SectionLowerEdge) section = RoundingUtils.Section.Lower;
                    if (section == (RoundingUtils.Section)SectionUpperEdge) section = RoundingUtils.Section.Upper;
                }

                bool roundDown =
                    RoundingUtils.GetRoundingDirection(
                        (trailingDigit % 2) == 0,
                        IsNegative,
                        section,
                        mathContext.RoundingMode,
                        this);

                // Perform truncation
                if (position >= precision)
                {
                    SetBcdToZero();
                    scale = magnitude;
                }
                else
                {
                    ShiftRight(position);
                }

                // Bubble the result to the higher digits
                if (!roundDown)
                {
                    if (trailingDigit == 9)
                    {
                        int bubblePos = 0;
                        // Note: in the long implementation, the most digits BCD can have at this point is 15,
                        // so bubblePos <= 15 and getDigitPos(bubblePos) is safe.
                        for (; GetDigitPos(bubblePos) == 9; bubblePos++) { }
                        ShiftRight(bubblePos); // shift off the trailing 9s
                    }
                    byte digit0 = GetDigitPos(0);
                    Debug.Assert(digit0 != 9);
                    SetDigitPos(0, (byte)(digit0 + 1));
                    precision += 1; // in case an extra digit got added
                }

                Compact();
            }
        }

        public virtual void RoundToInfinity()
        {
            if (isApproximate)
            {
                ConvertToAccurateDouble();
            }
        }

        /**
         * Appends a digit, optionally with one or more leading zeros, to the end of the value represented
         * by this DecimalQuantity.
         *
         * <p>The primary use of this method is to construct numbers during a parsing loop. It allows
         * parsing to take advantage of the digit list infrastructure primarily designed for formatting.
         *
         * @param value The digit to append.
         * @param leadingZeros The number of zeros to append before the digit. For example, if the value
         *     in this instance starts as 12.3, and you append a 4 with 1 leading zero, the value becomes
         *     12.304.
         * @param appendAsInteger If true, increase the magnitude of existing digits to make room for the
         *     new digit. If false, append to the end like a fraction digit. If true, there must not be
         *     any fraction digits already in the number.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public void AppendDigit(byte value, int leadingZeros, bool appendAsInteger)
        {
            Debug.Assert(leadingZeros >= 0);

            // Zero requires special handling to maintain the invariant that the least-significant digit
            // in the BCD is nonzero.
            if (value == 0)
            {
                if (appendAsInteger && precision != 0)
                {
                    scale += leadingZeros + 1;
                }
                return;
            }

            // Deal with trailing zeros
            if (scale > 0)
            {
                leadingZeros += scale;
                if (appendAsInteger)
                {
                    scale = 0;
                }
            }

            // Append digit
            ShiftLeft(leadingZeros + 1);
            SetDigitPos(0, value);

            // Fix scale if in integer mode
            if (appendAsInteger)
            {
                scale += leadingZeros + 1;
            }
        }

        public virtual string ToPlainString()
        {
            // NOTE: This logic is duplicated between here and DecimalQuantity_SimpleStorage.
            StringBuilder sb = new StringBuilder();
            if (IsNegative)
            {
                sb.Append('-');
            }
            for (int m = UpperDisplayMagnitude; m >= LowerDisplayMagnitude; m--)
            {
                sb.Append(GetDigit(m));
                if (m == 0) sb.Append('.');
            }
            return sb.ToString();
        }

        /**
         * Returns a single digit from the BCD list. No internal state is changed by calling this method.
         *
         * @param position The position of the digit to pop, counted in BCD units from the least
         *     significant digit. If outside the range supported by the implementation, zero is returned.
         * @return The digit at the specified location.
         */
        protected abstract byte GetDigitPos(int position);

        /**
         * Sets the digit in the BCD list. This method only sets the digit; it is the caller's
         * responsibility to call {@link #compact} after setting the digit.
         *
         * @param position The position of the digit to pop, counted in BCD units from the least
         *     significant digit. If outside the range supported by the implementation, an AssertionError
         *     is thrown.
         * @param value The digit to set at the specified location.
         */
        protected abstract void SetDigitPos(int position, byte value);

        /**
         * Adds zeros to the end of the BCD list. This will result in an invalid BCD representation; it is
         * the caller's responsibility to do further manipulation and then call {@link #compact}.
         *
         * @param numDigits The number of zeros to add.
         */
        protected abstract void ShiftLeft(int numDigits);

        protected abstract void ShiftRight(int numDigits);

        /**
         * Sets the internal representation to zero. Clears any values stored in scale, precision,
         * hasDouble, origDouble, origDelta, and BCD data.
         */
        protected abstract void SetBcdToZero();

        /**
         * Sets the internal BCD state to represent the value in the given int. The int is guaranteed to
         * be either positive. The internal state is guaranteed to be empty when this method is called.
         *
         * @param n The value to consume.
         */
        protected abstract void ReadIntToBcd(int input);

        /**
         * Sets the internal BCD state to represent the value in the given long. The long is guaranteed to
         * be either positive. The internal state is guaranteed to be empty when this method is called.
         *
         * @param n The value to consume.
         */
        protected abstract void ReadLongToBcd(long input);

        /**
         * Sets the internal BCD state to represent the value in the given BigInteger. The BigInteger is
         * guaranteed to be positive, and it is guaranteed to be larger than Long.MAX_VALUE. The internal
         * state is guaranteed to be empty when this method is called.
         *
         * @param n The value to consume.
         */
        protected abstract void ReadBigIntegerToBcd(BigMath.BigInteger input);

        /**
         * Returns a BigDecimal encoding the internal BCD value.
         *
         * @return A BigDecimal representation of the internal BCD.
         */
        protected abstract BigMath.BigDecimal BcdToBigDecimal();

        protected abstract void CopyBcdFrom(IDecimalQuantity other);

        /**
         * Removes trailing zeros from the BCD (adjusting the scale as required) and then computes the
         * precision. The precision is the number of digits in the number up through the greatest nonzero
         * digit.
         *
         * <p>This method must always be called when bcd changes in order for assumptions to be correct in
         * methods like {@link #fractionCount()}.
         */
        protected abstract void Compact();

        /// <inheritdoc/>
        public abstract IDecimalQuantity CreateCopy(); // ICU4N specific - must implement all interface members
    }
}
