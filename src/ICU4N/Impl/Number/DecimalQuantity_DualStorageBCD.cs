using J2N;
using J2N.Numerics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Double = J2N.Numerics.Double;
using Integer = J2N.Numerics.Int32;
using Long = J2N.Numerics.Int64;
using Number = J2N.Numerics.Number;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A DecimalQuantity with internal storage as a 64-bit BCD, with fallback to a byte array
    /// for numbers that don't fit into the standard BCD.
    /// </summary>
    internal sealed class DecimalQuantity_DualStorageBCD : DecimalQuantity_AbstractBCD
    {
        /**
       * The BCD of the 16 digits of the number represented by this object. Every 4 bits of the long map
       * to one digit. For example, the number "12345" in BCD is "0x12345".
       *
       * <p>Whenever bcd changes internally, {@link #compact()} must be called, except in special cases
       * like setting the digit to zero.
       */
        private byte[] bcdBytes;

        private long bcdLong = 0L;

        private bool usingBytes = false;


        public override int MaxRepresentableDigits => int.MaxValue;


        public DecimalQuantity_DualStorageBCD()
        {
            SetBcdToZero();
            flags = 0;
        }

        public DecimalQuantity_DualStorageBCD(long input)
        {
            SetToLong(input);
        }

        public DecimalQuantity_DualStorageBCD(int input)
        {
            SetToInt(input);
        }

        public DecimalQuantity_DualStorageBCD(double input)
        {
            SetToDouble(input);
        }

        public DecimalQuantity_DualStorageBCD(BigMath.BigInteger input)
        {
            SetToBigInteger(input);
        }

        public DecimalQuantity_DualStorageBCD(BigMath.BigDecimal input)
        {
            SetToBigDecimal(input);
        }

        public DecimalQuantity_DualStorageBCD(DecimalQuantity_DualStorageBCD other)
        {
            CopyFrom(other);
        }

        public DecimalQuantity_DualStorageBCD(Number number)
        {
            if (number is Long)
            {
                SetToLong(number.ToInt64());
            }
            else if (number is Integer)
            {
                SetToInt(number.ToInt32());
            }
            else if (number is Double)
            {
                SetToDouble(number.ToDouble());
            }
            else if (number is BigMath.BigInteger)
            {
                SetToBigInteger((BigMath.BigInteger)number);
            }
            else if (number is BigMath.BigDecimal bd1) // like java.math.BigDecimal
            {
                SetToBigDecimal(bd1);
            }
            else if (number is BigDecimal)
            {
                SetToBigDecimal(((BigDecimal)number).ToBigDecimal()); // This converts to java.math.BigDecimal
            }
            else
            {
                throw new ArgumentException(
                    "Number is of an unsupported type: " + number.GetType().Name);
            }
        }

        public override IDecimalQuantity CreateCopy()
        {
            return new DecimalQuantity_DualStorageBCD(this);
        }

        protected override byte GetDigitPos(int position)
        {
            if (usingBytes)
            {
                if (position < 0 || position > precision) return 0;
                return bcdBytes[position];
            }
            else
            {
                if (position < 0 || position >= 16) return 0;
                return (byte)((bcdLong.TripleShift(position * 4)) & 0xf);
            }
        }

        protected override void SetDigitPos(int position, byte value)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position)); // ICU4N TODO: Error message
            //assert position >= 0;
            if (usingBytes)
            {
                EnsureCapacity(position + 1);
                bcdBytes[position] = value;
            }
            else if (position >= 16)
            {
                SwitchStorage();
                EnsureCapacity(position + 1);
                bcdBytes[position] = value;
            }
            else
            {
                int shift = position * 4;
                bcdLong = bcdLong & ~(0xfL << shift) | ((long)value << shift);
            }
        }

        protected override void ShiftLeft(int numDigits)
        {
            if (!usingBytes && precision + numDigits > 16)
            {
                SwitchStorage();
            }
            if (usingBytes)
            {
                EnsureCapacity(precision + numDigits);
                int i = precision + numDigits - 1;
                for (; i >= numDigits; i--)
                {
                    bcdBytes[i] = bcdBytes[i - numDigits];
                }
                for (; i >= 0; i--)
                {
                    bcdBytes[i] = 0;
                }
            }
            else
            {
                bcdLong <<= (numDigits * 4);
            }
            scale -= numDigits;
            precision += numDigits;
        }

        protected override void ShiftRight(int numDigits)
        {
            if (usingBytes)
            {
                int i = 0;
                for (; i < precision - numDigits; i++)
                {
                    bcdBytes[i] = bcdBytes[i + numDigits];
                }
                for (; i < precision; i++)
                {
                    bcdBytes[i] = 0;
                }
            }
            else
            {
                //bcdLong >>>= (numDigits * 4);
                bcdLong = bcdLong.TripleShift(numDigits * 4);
            }
            scale += numDigits;
            precision -= numDigits;
        }

        protected override void SetBcdToZero()
        {
            if (usingBytes)
            {
                bcdBytes = null;
                usingBytes = false;
            }
            bcdLong = 0L;
            scale = 0;
            precision = 0;
            isApproximate = false;
            origDouble = 0;
            origDelta = 0;
        }

        protected override void ReadIntToBcd(int n)
        {
            //assert n != 0;
            if (n == 0)
                throw new ArgumentOutOfRangeException(nameof(n), "n must not be zero.");
            // ints always fit inside the long implementation.
            long result = 0L;
            int i = 16;
            for (; n != 0; n /= 10, i--)
            {
                result = (result.TripleShift(4)) + (((long)n % 10) << 60);
            }
            Debug.Assert(!usingBytes);
            bcdLong = result.TripleShift(i * 4);
            scale = 0;
            precision = 16 - i;
        }

        protected override void ReadLongToBcd(long n)
        {
            //assert n != 0;
            if (n == 0)
                throw new ArgumentOutOfRangeException(nameof(n), "n must not be zero.");
            if (n >= 10000000000000000L)
            {
                EnsureCapacity();
                int i = 0;
                for (; n != 0L; n /= 10L, i++)
                {
                    bcdBytes[i] = (byte)(n % 10);
                }
                Debug.Assert(usingBytes);
                scale = 0;
                precision = i;
            }
            else
            {
                long result = 0L;
                int i = 16;
                for (; n != 0L; n /= 10L, i--)
                {
                    result = (result.TripleShift(4)) + ((n % 10) << 60);
                }
                Debug.Assert(i >= 0);
                Debug.Assert(!usingBytes);
                bcdLong = result.TripleShift(i * 4);
                scale = 0;
                precision = 16 - i;
            }
        }

        protected override void ReadBigIntegerToBcd(BigMath.BigInteger n)
        {
            if (n.Sign == 0)
                throw new ArgumentException("BigInteger sign must not be zero.");
            //assert n.signum() != 0;
            EnsureCapacity(); // allocate initial byte array
            int i = 0;
            for (; n.Sign != 0; i++)
            {
                var result = BigMath.BigInteger.DivideAndRemainder(n, BigMath.BigInteger.Ten, out BigMath.BigInteger remainder);
                EnsureCapacity(i + 1);
                bcdBytes[i] = remainder.ToByte();
                n = result;
            }
            scale = 0;
            precision = i;
        }

        protected override BigMath.BigDecimal BcdToBigDecimal()
        {
            if (usingBytes)
            {
                // Converting to a string here is faster than doing BigInteger/BigDecimal arithmetic.
                //BigDecimal result = new BigDecimal(ToNumberString());
                BigMath.BigDecimal result = BigMath.BigDecimal.Parse(ToNumberString(), CultureInfo.InvariantCulture);
                if (IsNegative)
                {
                    result = -result; //.negate();
                }
                return result;
            }
            else
            {
                long tempLong = 0L;
                for (int shift = (precision - 1); shift >= 0; shift--)
                {
                    tempLong = tempLong * 10 + GetDigitPos(shift);
                }
                BigMath.BigDecimal result = new BigMath.BigDecimal(tempLong);
                result = BigMath.BigDecimal.ScaleByPowerOfTen(result, scale);
                if (IsNegative) result = -result;
                return result;
                //BigDecimal result = BigDecimal.GetInstance(tempLong);
                //result = result.ScaleByPowerOfTen(scale);
                //if (IsNegative) result = -result; //.negate();
                //return result;
            }
        }

        protected override void Compact()
        {
            if (usingBytes)
            {
                int delta = 0;
                for (; delta < precision && bcdBytes[delta] == 0; delta++) ;
                if (delta == precision)
                {
                    // Number is zero
                    SetBcdToZero();
                    return;
                }
                else
                {
                    // Remove trailing zeros
                    ShiftRight(delta);
                }

                // Compute precision
                int leading = precision - 1;
                for (; leading >= 0 && bcdBytes[leading] == 0; leading--) ;
                precision = leading + 1;

                // Switch storage mechanism if possible
                if (precision <= 16)
                {
                    SwitchStorage();
                }

            }
            else
            {
                if (bcdLong == 0L)
                {
                    // Number is zero
                    SetBcdToZero();
                    return;
                }

                // Compact the number (remove trailing zeros)
                int delta = bcdLong.TrailingZeroCount() / 4;
                //bcdLong >>>= delta * 4;
                bcdLong = bcdLong.TripleShift(delta * 4);
                scale += delta;

                // Compute precision
                precision = 16 - (bcdLong.LeadingZeroCount() / 4);
            }
        }

        /** Ensure that a byte array of at least 40 digits is allocated. */
        private void EnsureCapacity()
        {
            EnsureCapacity(40);
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity == 0) return;
            int oldCapacity = usingBytes ? bcdBytes.Length : 0;
            if (!usingBytes)
            {
                bcdBytes = new byte[capacity];
            }
            else if (oldCapacity < capacity)
            {
                byte[] bcd1 = new byte[capacity * 2];
                Array.Copy(bcdBytes, 0, bcd1, 0, oldCapacity);
                bcdBytes = bcd1;
            }
            usingBytes = true;
        }

        /** Switches the internal storage mechanism between the 64-bit long and the byte array. */
        private void SwitchStorage()
        {
            if (usingBytes)
            {
                // Change from bytes to long
                bcdLong = 0L;
                for (int i = precision - 1; i >= 0; i--)
                {
                    bcdLong <<= 4;
                    bcdLong |= bcdBytes[i];
                }
                bcdBytes = null;
                usingBytes = false;
            }
            else
            {
                // Change from long to bytes
                EnsureCapacity();
                for (int i = 0; i < precision; i++)
                {
                    bcdBytes[i] = (byte)(bcdLong & 0xf);
                    //bcdLong >>>= 4;
                    bcdLong = bcdLong.TripleShift(4);
                }
                Debug.Assert(usingBytes);
            }
        }

        protected override void CopyBcdFrom(IDecimalQuantity other)
        {
            DecimalQuantity_DualStorageBCD other_ = (DecimalQuantity_DualStorageBCD)other; // ICU4N TODO: Safe cast
            SetBcdToZero();
            if (other_.usingBytes)
            {
                EnsureCapacity(other_.precision);
                Array.Copy(other_.bcdBytes, 0, bcdBytes, 0, other_.precision);
            }
            else
            {
                bcdLong = other_.bcdLong;
            }
        }

        /**
         * Checks whether the bytes stored in this instance are all valid. For internal unit testing only.
         *
         * @return An error message if this instance is invalid, or null if this instance is healthy.
         * @internal
         * @deprecated This API is for ICU internal use only.
         */
        [Obsolete("This API is for ICU internal use only.")]
        public string CheckHealth()
        {
            if (usingBytes)
            {
                if (bcdLong != 0) return "Value in bcdLong but we are in byte mode";
                if (precision == 0) return "Zero precision but we are in byte mode";
                if (precision > bcdBytes.Length) return "Precision exceeds length of byte array";
                if (GetDigitPos(precision - 1) == 0) return "Most significant digit is zero in byte mode";
                if (GetDigitPos(0) == 0) return "Least significant digit is zero in long mode";
                for (int i = 0; i < precision; i++)
                {
                    if (GetDigitPos(i) >= 10) return "Digit exceeding 10 in byte array";
                    if (GetDigitPos(i) < 0) return "Digit below 0 in byte array";
                }
                for (int i = precision; i < bcdBytes.Length; i++)
                {
                    if (GetDigitPos(i) != 0) return "Nonzero digits outside of range in byte array";
                }
            }
            else
            {
                if (bcdBytes != null)
                {
                    for (int i = 0; i < bcdBytes.Length; i++)
                    {
                        if (bcdBytes[i] != 0) return "Nonzero digits in byte array but we are in long mode";
                    }
                }
                if (precision == 0 && bcdLong != 0) return "Value in bcdLong even though precision is zero";
                if (precision > 16) return "Precision exceeds length of long";
                if (precision != 0 && GetDigitPos(precision - 1) == 0)
                    return "Most significant digit is zero in long mode";
                if (precision != 0 && GetDigitPos(0) == 0)
                    return "Least significant digit is zero in long mode";
                for (int i = 0; i < precision; i++)
                {
                    if (GetDigitPos(i) >= 10) return "Digit exceeding 10 in long";
                    if (GetDigitPos(i) < 0) return "Digit below 0 in long (?!)";
                }
                for (int i = precision; i < 16; i++)
                {
                    if (GetDigitPos(i) != 0) return "Nonzero digits outside of range in long";
                }
            }

            return null;
        }

        /**
         * Checks whether this {@link DecimalQuantity_DualStorageBCD} is using its internal byte array storage mechanism.
         *
         * @return true if an internal byte array is being used; false if a long is being used.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is for ICU internal use only.")]
        public bool IsUsingBytes => usingBytes;

        public override string ToString()
        {
            return string.Format("<DecimalQuantity {0}:{1}:{2}:{3} {4} {5}>",
                (lOptPos > 1000 ? "999" : lOptPos.ToString(CultureInfo.InvariantCulture)),
                lReqPos,
                rReqPos,
                (rOptPos < -1000 ? "-999" : rOptPos.ToString(CultureInfo.InvariantCulture)),
                (usingBytes ? "bytes" : "long"),
                ToNumberString());

            //return String.format(
            //    "<DecimalQuantity %s:%d:%d:%s %s %s>",
            //    (lOptPos > 1000 ? "999" : String.valueOf(lOptPos)),
            //    lReqPos,
            //    rReqPos,
            //    (rOptPos < -1000 ? "-999" : String.valueOf(rOptPos)),
            //    (usingBytes ? "bytes" : "long"),
            //    ToNumberString());
        }

        public string ToNumberString()
        {
            StringBuilder sb = new StringBuilder();
            if (usingBytes)
            {
                for (int i = precision - 1; i >= 0; i--)
                {
                    sb.Append(bcdBytes[i]);
                }
            }
            else
            {
                sb.Append(bcdLong.ToHexString());
            }
            sb.Append("E");
            sb.Append(scale);
            return sb.ToString();
        }
    }
}
