using System;

namespace ICU4N.Numerics
{
    /// <author>sffc</author>
    internal static class RoundingUtils // ICU4N TODO: API - this was public in ICU4J
    {
        public enum Section
        {
            Lower = 1,
            MidPoint = 2,
            Upper = 3,
        }

        //public const int SECTION_LOWER = 1;
        //public const int SECTION_MIDPOINT = 2;
        //public const int SECTION_UPPER = 3;

        /**
         * The default rounding mode.
         */
        public const BigMath.RoundingMode DefaultRoundingMode = BigMath.RoundingMode.HalfEven;

        /**
         * The maximum number of fraction places, integer numerals, or significant digits.
         * TODO: This does not feel like the best home for this value.
         */
        public const int MAX_INT_FRAC_SIG = 100;

        /**
         * Converts a rounding mode and metadata about the quantity being rounded to a boolean determining
         * whether the value should be rounded toward infinity or toward zero.
         *
         * <para/>The parameters are of type int because benchmarks on an x86-64 processor against OpenJDK
         * showed that ints were demonstrably faster than enums in switch statements.
         *
         * @param isEven Whether the digit immediately before the rounding magnitude is even.
         * @param isNegative Whether the quantity is negative.
         * @param section Whether the part of the quantity to the right of the rounding magnitude is
         *     exactly halfway between two digits, whether it is in the lower part (closer to zero), or
         *     whether it is in the upper part (closer to infinity). See {@link #SECTION_LOWER}, {@link
         *     #SECTION_MIDPOINT}, and {@link #SECTION_UPPER}.
         * @param roundingMode The integer version of the {@link RoundingMode}, which you can get via
         *     {@link RoundingMode#ordinal}.
         * @param reference A reference object to be used when throwing an ArithmeticException.
         * @return true if the number should be rounded toward zero; false if it should be rounded toward
         *     infinity.
         */
        public static bool GetRoundingDirection(
            bool isEven, bool isNegative, Section section, BigMath.RoundingMode roundingMode, object reference)
        {
            switch (roundingMode)
            {
                case BigMath.RoundingMode.Up:
                    // round away from zero
                    return false;

                case BigMath.RoundingMode.Down:
                    // round toward zero
                    return true;

                case BigMath.RoundingMode.Ceiling:
                    // round toward positive infinity
                    return isNegative;

                case BigMath.RoundingMode.Floor:
                    // round toward negative infinity
                    return !isNegative;

                case BigMath.RoundingMode.HalfUp:
                    switch (section)
                    {
                        case Section.MidPoint:
                            return false;
                        case Section.Lower:
                            return true;
                        case Section.Upper:
                            return false;
                    }
                    break;

                case BigMath.RoundingMode.HalfDown:
                    switch (section)
                    {
                        case Section.MidPoint:
                            return true;
                        case Section.Lower:
                            return true;
                        case Section.Upper:
                            return false;
                    }
                    break;

                case BigMath.RoundingMode.HalfEven:
                    switch (section)
                    {
                        case Section.MidPoint:
                            return isEven;
                        case Section.Lower:
                            return true;
                        case Section.Upper:
                            return false;
                    }
                    break;
            }

            // Rounding mode UNNECESSARY
            throw new ArithmeticException("Rounding is required on " + reference.ToString());
        }

        /**
         * Gets whether the given rounding mode's rounding boundary is at the midpoint. The rounding
         * boundary is the point at which a number switches from being rounded down to being rounded up.
         * For example, with rounding mode HALF_EVEN, HALF_UP, or HALF_DOWN, the rounding boundary is at
         * the midpoint, and this function would return true. However, for UP, DOWN, CEILING, and FLOOR,
         * the rounding boundary is at the "edge", and this function would return false.
         *
         * @param roundingMode The integer version of the {@link RoundingMode}.
         * @return true if rounding mode is HALF_EVEN, HALF_UP, or HALF_DOWN; false otherwise.
         */
        public static bool RoundsAtMidpoint(BigMath.RoundingMode roundingMode)
        {
            switch (roundingMode)
            {
                case BigMath.RoundingMode.Up:
                case BigMath.RoundingMode.Down:
                case BigMath.RoundingMode.Ceiling:
                case BigMath.RoundingMode.Floor:
                    return false;

                default:
                    return true;
            }
        }

        private static readonly BigMath.MathContext[] MATH_CONTEXT_BY_ROUNDING_MODE_UNLIMITED = LoadUnlimited();

        private static readonly BigMath.MathContext[] MATH_CONTEXT_BY_ROUNDING_MODE_34_DIGITS = Load34Digits();

        private static BigMath.MathContext[] LoadUnlimited() // ICU4N: This logic depends on the RoundingMode being zero based and sequential.
        {
            int length = Enum.GetValues(typeof(BigMath.RoundingMode)).Length;
            var result = new BigMath.MathContext[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = new BigMath.MathContext(0, (BigMath.RoundingMode)i);
            }
            return result;
        }

        private static BigMath.MathContext[] Load34Digits() // ICU4N: This logic depends on the RoundingMode being zero based and sequential.
        {
            int length = Enum.GetValues(typeof(BigMath.RoundingMode)).Length;
            var result = new BigMath.MathContext[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = new BigMath.MathContext(34);
            }
            return result;
        }

        //  static {
        //    for (int i = 0; i<MATH_CONTEXT_BY_ROUNDING_MODE_34_DIGITS.length; i++) {
        //      MATH_CONTEXT_BY_ROUNDING_MODE_UNLIMITED[i] = new MathContext(0, RoundingMode.valueOf(i));
        //        MATH_CONTEXT_BY_ROUNDING_MODE_34_DIGITS[i] = new MathContext(34);
        //    }
        //}

        /**
         * Gets the user-specified math context out of the property bag. If there is none, falls back to a
         * math context with unlimited precision and the user-specified rounding mode, which defaults to
         * HALF_EVEN (the IEEE 754R default).
         *
         * @param properties The property bag.
         * @return A {@link MathContext}. Never null.
         */
        public static BigMath.MathContext GetMathContextOrUnlimited(DecimalFormatProperties properties)
        {
            BigMath.MathContext mathContext = properties.MathContext;
            if (mathContext == null)
            {
                BigMath.RoundingMode? roundingMode = properties.RoundingMode;
                if (roundingMode == null) roundingMode = BigMath.RoundingMode.HalfEven;
                mathContext = MATH_CONTEXT_BY_ROUNDING_MODE_UNLIMITED[(int)roundingMode];
            }
            return mathContext;
        }

        /**
         * Gets the user-specified math context out of the property bag. If there is none, falls back to a
         * math context with 34 digits of precision (the 128-bit IEEE 754R default) and the user-specified
         * rounding mode, which defaults to HALF_EVEN (the IEEE 754R default).
         *
         * @param properties The property bag.
         * @return A {@link MathContext}. Never null.
         */
        public static BigMath.MathContext GetMathContextOr34Digits(DecimalFormatProperties properties)
        {
            BigMath.MathContext mathContext = properties.MathContext;
            if (mathContext == null)
            {
                BigMath.RoundingMode? roundingMode = properties.RoundingMode;
                if (roundingMode == null) roundingMode = BigMath.RoundingMode.HalfEven;
                mathContext = MATH_CONTEXT_BY_ROUNDING_MODE_34_DIGITS[(int)roundingMode];
            }
            return mathContext;
        }

        /**
         * Gets a MathContext with unlimited precision and the specified RoundingMode. Equivalent to "new
         * MathContext(0, roundingMode)", but pulls from a singleton to prevent object thrashing.
         *
         * @param roundingMode The {@link RoundingMode} to use.
         * @return The corresponding {@link MathContext}.
         */
        public static BigMath.MathContext MathContextUnlimited(BigMath.RoundingMode roundingMode)
        {
            return MATH_CONTEXT_BY_ROUNDING_MODE_UNLIMITED[(int)roundingMode];
        }
    }
}
