﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support
{
    /// <summary>
    /// A simple class for number conversions.
    /// </summary>
    public static class Number
    {
        //        /// <summary>
        //        /// Min radix value.
        //        /// </summary>
        //        public const int MIN_RADIX = 2;

        //        /// <summary>
        //        /// Max radix value.
        //        /// </summary>
        //        public const int MAX_RADIX = 36;

        //        /*public const int CHAR_MIN_CODE_POINT =
        //        public const int CHAR_MAX_CODE_POINT = */

        //        private const string digits = "0123456789abcdefghijklmnopqrstuvwxyz";

        //        /// <summary>
        //        /// Converts a number to System.String.
        //        /// </summary>
        //        /// <param name="number"></param>
        //        /// <returns></returns>
        //        public static string ToString(long number)
        //        {
        //            var s = new System.Text.StringBuilder();

        //            if (number == 0)
        //            {
        //                s.Append("0");
        //            }
        //            else
        //            {
        //                if (number < 0)
        //                {
        //                    s.Append("-");
        //                    number = -number;
        //                }

        //                while (number > 0)
        //                {
        //                    char c = digits[(int)number % 36];
        //                    s.Insert(0, c);
        //                    number = number / 36;
        //                }
        //            }

        //            return s.ToString();
        //        }

        /// <summary>
        /// Converts a number to System.String.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static System.String ToString(double d)
        {
            if ((double)(int)d == d)
            {
                // Special case: When we have an integer value,
                // the standard .NET formatting removes the decimal point
                // and everything to the right. But we need to always
                // have at least decimal place to match Java.
                return d.ToString("0.0", CultureInfo.InvariantCulture);
            }
            else
            {
                // Although the MSDN documentation says that 
                // round-trip on float will be limited to 7 decimals, it appears
                // not to be the case. Also, when specifying "0.0######", we only
                // get a result to 6 decimal places maximum. So, we must round before
                // doing a round-trip format to guarantee 7 decimal places.
                return Math.Round(d, 7).ToString("R", CultureInfo.InvariantCulture);
            }
        }

        //        /// <summary>
        //        /// Converts a number to System.String in the specified radix.
        //        /// </summary>
        //        /// <param name="i">A number to be converted.</param>
        //        /// <param name="radix">A radix.</param>
        //        /// <returns>A System.String representation of the number in the specified redix.</returns>
        //        public static String ToString(long i, int radix)
        //        {
        //            if (radix < MIN_RADIX || radix > MAX_RADIX)
        //                radix = 10;

        //            var buf = new char[65];
        //            int charPos = 64;
        //            bool negative = (i < 0);

        //            if (!negative)
        //            {
        //                i = -i;
        //            }

        //            while (i <= -radix)
        //            {
        //                buf[charPos--] = digits[(int)(-(i % radix))];
        //                i = i / radix;
        //            }
        //            buf[charPos] = digits[(int)(-i)];

        //            if (negative)
        //            {
        //                buf[--charPos] = '-';
        //            }

        //            return new System.String(buf, charPos, (65 - charPos));
        //        }

        //        /// <summary>
        //        /// Parses a number in the specified radix.
        //        /// </summary>
        //        /// <param name="s">An input System.String.</param>
        //        /// <param name="radix">A radix.</param>
        //        /// <returns>The parsed number in the specified radix.</returns>
        //        public static long Parse(System.String s, int radix)
        //        {
        //            if (s == null)
        //            {
        //                throw new ArgumentException("null");
        //            }

        //            if (radix < MIN_RADIX)
        //            {
        //                throw new NotSupportedException("radix " + radix +
        //                                                " less than Number.MIN_RADIX");
        //            }
        //            if (radix > MAX_RADIX)
        //            {
        //                throw new NotSupportedException("radix " + radix +
        //                                                " greater than Number.MAX_RADIX");
        //            }

        //            long result = 0;
        //            long mult = 1;

        //            s = s.ToLowerInvariant(); // TODO: Do we need to deal with Turkish? If so, this won't work right...

        //            for (int i = s.Length - 1; i >= 0; i--)
        //            {
        //                int weight = digits.IndexOf(s[i]);
        //                if (weight == -1)
        //                    throw new FormatException("Invalid number for the specified radix");

        //                result += (weight * mult);
        //                mult *= radix;
        //            }

        //            return result;
        //        }

        //        /// <summary>
        //        /// Performs an unsigned bitwise right shift with the specified number
        //        /// </summary>
        //        /// <param name="number">Number to operate on</param>
        //        /// <param name="bits">Ammount of bits to shift</param>
        //        /// <returns>The resulting number from the shift operation</returns>
        //        public static int URShift(int number, int bits)
        //        {
        //            return (int)(((uint)number) >> bits);
        //        }

        //        /// <summary>
        //        /// Performs an unsigned bitwise right shift with the specified number
        //        /// </summary>
        //        /// <param name="number">Number to operate on</param>
        //        /// <param name="bits">Ammount of bits to shift</param>
        //        /// <returns>The resulting number from the shift operation</returns>
        //        public static long URShift(long number, int bits)
        //        {
        //            return (long)(((ulong)number) >> bits);
        //        }

        //        /// <summary>
        //        /// Converts a System.String number to long.
        //        /// </summary>
        //        /// <param name="s"></param>
        //        /// <returns></returns>
        //        public static long ToInt64(System.String s)
        //        {
        //            long number = 0;
        //            long factor;

        //            // handle negative number
        //            if (s.StartsWith("-", StringComparison.Ordinal))
        //            {
        //                s = s.Substring(1);
        //                factor = -1;
        //            }
        //            else
        //            {
        //                factor = 1;
        //            }

        //            // generate number
        //            for (int i = s.Length - 1; i > -1; i--)
        //            {
        //                int n = digits.IndexOf(s[i]);

        //                // not supporting fractional or scientific notations
        //                if (n < 0)
        //                    throw new System.ArgumentException("Invalid or unsupported character in number: " + s[i]);

        //                number += (n * factor);
        //                factor *= 36;
        //            }

        //            return number;
        //        }

        //        public static int NumberOfLeadingZeros(int num)
        //        {
        //            if (num == 0)
        //                return 32;

        //            uint unum = (uint)num;
        //            int count = 0;
        //            int i;

        //            for (i = 0; i < 32; ++i)
        //            {
        //                if ((unum & 0x80000000) == 0x80000000)
        //                    break;

        //                count++;
        //                unum <<= 1;
        //            }

        //            return count;
        //        }

        //        public static int NumberOfLeadingZeros(long num)
        //        {
        //            if (num == 0)
        //                return 64;

        //            ulong unum = (ulong)num;
        //            int count = 0;
        //            int i;

        //            for (i = 0; i < 64; ++i)
        //            {
        //                if ((unum & 0x8000000000000000L) == 0x8000000000000000L)
        //                    break;

        //                count++;
        //                unum <<= 1;
        //            }

        //            return count;
        //        }

        //        public static int NumberOfTrailingZeros(int num)
        //        {
        //            if (num == 0)
        //                return 32;

        //            uint unum = (uint)num;
        //            int count = 0;
        //            int i;

        //            for (i = 0; i < 32; ++i)
        //            {
        //                if ((unum & 1) == 1)
        //                    break;

        //                count++;
        //                unum >>= 1;
        //            }

        //            return count;
        //        }

        //        public static int NumberOfTrailingZeros(long num)
        //        {
        //            if (num == 0)
        //                return 64;

        //            ulong unum = (ulong)num;
        //            int count = 0;
        //            int i;

        //            for (i = 0; i < 64; ++i)
        //            {
        //                if ((unum & 1L) == 1L)
        //                    break;

        //                count++;
        //                unum >>= 1;
        //            }

        //            return count;
        //        }

        //        // Returns the number of 1-bits in the number
        //        public static int BitCount(long num)
        //        {
        //            int bitcount = 0;
        //            // To use the > 0 condition
        //            ulong nonNegNum = (ulong)num;
        //            while (nonNegNum > 0)
        //            {
        //                bitcount += (int)(nonNegNum & 0x1);
        //                nonNegNum >>= 1;
        //            }
        //            return bitcount;
        //        }

        //        public static int Signum(long a)
        //        {
        //            return a == 0 ? 0 : (int)(a / Math.Abs(a));
        //        }

        //        // Returns the number of 1-bits in the number
        //        public static int BitCount(int num)
        //        {
        //            int bitcount = 0;
        //            while (num > 0)
        //            {
        //                bitcount += (num & 1);
        //                num >>= 1;
        //            }
        //            return bitcount;
        //        }

        //        public static int RotateLeft(int i, int reps)
        //        {
        //            uint val = (uint)i;
        //            return (int)((val << reps) | (val >> (32 - reps)));
        //        }

        //        public static int RotateRight(int i, int reps)
        //        {
        //            uint val = (uint)i;
        //            return (int)((val >> reps) | (val << (32 - reps)));
        //        }

        //        public static string ToBinaryString(int value)
        //        {
        //            // Convert to base 2 string with no leading 0's
        //            return Convert.ToString(value, 2);
        //        }


        /// <summary>
        /// Returns the signum function of the specified <see cref="int"/> value.  (The
        /// return value is -1 if the specified value is negative; 0 if the
        /// specified value is zero; and 1 if the specified value is positive.)
        /// <para/>
        /// This can be useful for testing the results of two <see cref="IComparable{T}.CompareTo(T)"/>
        /// methods against each other, since only the sign is guaranteed to be the same between impementations.
        /// </summary>
        /// <returns>the signum function of the specified <see cref="int"/> value.</returns>
        public static int Signum(int i)
        {
            // HD, Section 2-7
            return (i >> 31) | (-i.TripleShift(31));
        }


        /// <summary>
        /// NOTE: This was intBitsToFloat() in the JDK
        /// </summary>
        public static float Int32BitsToSingle(int value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        //        /// <summary>
        //        /// NOTE: This was floatToRawIntBits() in the JDK
        //        /// </summary>
        //        public static int SingleToRawInt32Bits(float value)
        //        {
        //            // TODO: does this handle NaNs the same?
        //            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        //        }

        /// <summary>
        /// NOTE: This was floatToIntBits() in the JDK
        /// </summary>
        public static int SingleToInt32Bits(float value)
        {
            if (float.IsNaN(value))
            {
                return 0x7fc00000;
            }

            // TODO it is claimed that this could be faster
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        //        /// <summary>
        //        /// NOTE: This was floatToLongBits() in the JDK
        //        /// </summary>
        //        public static long SingleToInt64Bits(float value)
        //        {
        //            return BitConverter.ToInt64(BitConverter.GetBytes(value), 0);
        //        }

        /// <summary>
        /// NOTE: This was doubleToRawLongBits() in the JDK
        /// </summary>
        public static long DoubleToRawInt64Bits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        /// <summary>
        /// NOTE: This was doubleToLongBits() in the JDK
        /// </summary>
        public static long DoubleToInt64Bits(double value)
        {
            if (double.IsNaN(value))
            {
                return 0x7ff8000000000000L;
            }

            return BitConverter.DoubleToInt64Bits(value);
        }

        /// <summary>
        /// Replacement for Java triple shift (>>>) operator.
        /// </summary>
        public static long TripleShift(this long number, int shift)
        {
            return (long)((ulong)number >> shift);
        }

        /// <summary>
        /// Replacement for Java triple shift (>>>) operator.
        /// See http://stackoverflow.com/a/6625912
        /// </summary>
        public static int TripleShift(this int number, int shift)
        {
            if (number >= 0)
                return number >> shift;
            return (number >> shift) + (2 << ~shift);
        }

        /// <summary>
        /// Replacement for Java triple shift (>>>) operator.
        /// See http://stackoverflow.com/a/6625912
        /// </summary>
        public static int TripleShift(this char number, int shift)
        {
            if (number >= 0)
                return number >> shift;
            return (number >> shift) + (2 << ~shift);
        }



        //        //Flips the endianness from Little-Endian to Big-Endian

        //        //2 bytes
        //        public static char FlipEndian(char x)
        //        {
        //            return (char)((x & 0xFFU) << 8 | (x & 0xFF00U) >> 8);
        //        }

        //        //2 bytes
        //        public static short FlipEndian(short x)
        //        {
        //            return (short)((x & 0xFFU) << 8 | (x & 0xFF00U) >> 8);
        //        }

        //        //4 bytes
        //        public static int FlipEndian(int x)
        //        {
        //            return (int)((x & 0x000000FFU) << 24 | (x & 0x0000FF00U) << 8 | (x & 0x00FF0000U) >> 8 | (x & 0xFF000000U) >> 24);
        //        }

        //        //8 bytes
        //        public static long FlipEndian(long x)
        //        {
        //            ulong y = (ulong)x;
        //            return (long)(
        //                (y & 0x00000000000000FFUL) << 56 | (y & 0x000000000000FF00UL) << 40 |
        //                (y & 0x0000000000FF0000UL) << 24 | (y & 0x00000000FF000000UL) << 8 |
        //                (y & 0x000000FF00000000UL) >> 8 | (y & 0x0000FF0000000000UL) >> 24 |
        //                (y & 0x00FF000000000000UL) >> 40 | (y & 0xFF00000000000000UL) >> 56);
        //        }

        //        //4 bytes
        //        public static float FlipEndian(float f)
        //        {
        //            int x = SingleToInt32Bits(f);
        //            return Int32BitsToSingle(FlipEndian(x));
        //        }

        //        //8 bytes
        //        public static double FlipEndian(double d)
        //        {
        //            long x = BitConverter.DoubleToInt64Bits(d);
        //            return BitConverter.Int64BitsToDouble(FlipEndian(x));
        //        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}
