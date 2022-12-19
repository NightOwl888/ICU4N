using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICU4N.Numerics
{
    internal partial class BigDecimal
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Conversions to BigDecimal

        public static implicit operator BigDecimal(BigInteger value) => new BigDecimal(value, 0);

        public static implicit operator BigDecimal(byte value) => new BigDecimal(value);

        public static implicit operator BigDecimal(sbyte value) => new BigDecimal(value);

        public static implicit operator BigDecimal(short value) => new BigDecimal(value);

        public static implicit operator BigDecimal(ushort value) => new BigDecimal(value);

        public static implicit operator BigDecimal(int value) => new BigDecimal(value);

        public static implicit operator BigDecimal(uint value) => new BigDecimal(value);

        public static implicit operator BigDecimal(long value) => new BigDecimal(value);

        public static implicit operator BigDecimal(ulong value) => new BigDecimal((BigInteger)value);

        public static implicit operator BigDecimal(decimal value)
        {
            return new BigDecimal(value.ToString(CultureInfo.InvariantCulture));

            //ref var decimalData = ref Unsafe.As<decimal, DecimalData>(ref value);

            //var mantissa = (new BigInteger(decimalData.Hi) << 64) + decimalData.Lo;

            //if (!decimalData.IsPositive)
            //    mantissa = -mantissa;

            //return new BigDecimal(mantissa, -decimalData.Scale);
        }

        #endregion

        #region Conversions from BigDecimal

        public static explicit operator float(BigDecimal value)
        {
            return value.ToSingle();  // float.Parse(value.ToString(ToDecimalOrFloatFormat), ToDecimalOrFloatStyle, CultureInfo.InvariantCulture);
        }

        public static explicit operator double(BigDecimal value)
        {
            return value.ToDouble(); // double.Parse(value.ToString(ToDecimalOrFloatFormat), ToDecimalOrFloatStyle, CultureInfo.InvariantCulture);
        }

        //public static explicit operator decimal(BigDecimal value)
        //{
        //    return value. //decimal.Parse(value.ToString(ToDecimalOrFloatFormat), ToDecimalOrFloatStyle, CultureInfo.InvariantCulture);
        //}

        public static explicit operator BigInteger(BigDecimal value)
        {
            return value.ToBigInteger(); //value._exponent < 0 ? value._mantissa / BigIntegerPow10.Get(-value._exponent) : value._mantissa * BigIntegerPow10.Get(value._exponent);
        }

        public static explicit operator byte(BigDecimal value) => (byte)(BigInteger)value;

        public static explicit operator sbyte(BigDecimal value) => (sbyte)(BigInteger)value;

        public static explicit operator short(BigDecimal value) => (short)(BigInteger)value;

        public static explicit operator ushort(BigDecimal value) => (ushort)(BigInteger)value;

        public static explicit operator int(BigDecimal value) => (int)(BigInteger)value;

        public static explicit operator uint(BigDecimal value) => (uint)(BigInteger)value;

        public static explicit operator long(BigDecimal value) => (long)(BigInteger)value;

        public static explicit operator ulong(BigDecimal value) => (ulong)(BigInteger)value;

        #endregion

        #region Mathematical Operators

        public static BigDecimal operator +(BigDecimal value) => value;

        public static BigDecimal operator -(BigDecimal value) => value.Negate(); //new(BigInteger.Negate(value._mantissa), value._exponent, value._precision);

        public static BigDecimal operator ++(BigDecimal value) => value + One;

        public static BigDecimal operator --(BigDecimal value) => value - One;

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return left.Add(right);

            //if (left.IsZero)
            //    return right;

            //if (right.IsZero)
            //    return left;

            //return left._exponent > right._exponent
            //    ? new BigDecimal(AlignMantissa(left, right) + right._mantissa, right._exponent)
            //    : new BigDecimal(AlignMantissa(right, left) + left._mantissa, left._exponent);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right) => left + (-right);

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return left.Subtract(right);
            //if (left.IsZero || right.IsZero)
            //    return Zero;

            //if (left.IsOne)
            //    return right;

            //if (right.IsOne)
            //    return left;

            //return new BigDecimal(left._mantissa * right._mantissa, left._exponent + right._exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            return dividend.Divide(divisor);

            //if (TryDivideExact(dividend, divisor, out var result))
            //    return result;

            //return Divide(dividend, divisor, MaxExtendedDivisionPrecision);
        }

        // ICU4N TODO:
        //public static BigDecimal operator %(BigDecimal left, BigDecimal right) => left - (right * Floor(left / right));

        // ICU4N TODO: This is comparing exact value equality, however the Equals() method returns false if the scale is different.
        // See: https://blogs.oracle.com/javamagazine/post/four-common-pitfalls-of-the-bigdecimal-class-and-how-to-avoid-them
        public static bool operator ==(BigDecimal left, BigDecimal right) => left.CompareTo(right) == 0; //left.Equals(right); //left._exponent == right._exponent && left._mantissa == right._mantissa;

        public static bool operator !=(BigDecimal left, BigDecimal right) => left.CompareTo(right) != 0; //!left.Equals(right); //left._exponent != right._exponent || left._mantissa != right._mantissa;

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) < 0; //left._exponent > right._exponent ? AlignMantissa(left, right) < right._mantissa : left._mantissa < AlignMantissa(right, left);
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) > 0; //left._exponent > right._exponent ? AlignMantissa(left, right) > right._mantissa : left._mantissa > AlignMantissa(right, left);
        }

        // ICU4N TODO: 
        //public static bool operator <=(BigDecimal left, BigDecimal right)
        //{
        //    return left._exponent > right._exponent ? AlignMantissa(left, right) <= right._mantissa : left._mantissa <= AlignMantissa(right, left);
        //}

        //public static bool operator >=(BigDecimal left, BigDecimal right)
        //{
        //    return left._exponent > right._exponent ? AlignMantissa(left, right) >= right._mantissa : left._mantissa >= AlignMantissa(right, left);
        //}

        #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
