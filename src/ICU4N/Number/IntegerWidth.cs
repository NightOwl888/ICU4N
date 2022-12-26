﻿using System;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines the strategy for padding and truncating integers before the decimal separator.
    /// <para/>
    /// To create an <see cref="IntegerWidth"/>, use one of the factory methods.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class IntegerWidth // ICU4N TODO: API - this was public in ICU4J
    {
        /* package-private */
        internal static readonly IntegerWidth DEFAULT = new IntegerWidth(1, -1);

        internal readonly int minInt;
        internal readonly int maxInt;

        private IntegerWidth(int minInt, int maxInt)
        {
            this.minInt = minInt;
            this.maxInt = maxInt;
        }

        /**
         * Pad numbers at the beginning with zeros to guarantee a certain number of numerals before the decimal separator.
         *
         * <p>
         * For example, with minInt=3, the number 55 will get printed as "055".
         *
         * @param minInt
         *            The minimum number of places before the decimal separator.
         * @return An IntegerWidth for chaining or passing to the NumberFormatter integerWidth() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static IntegerWidth ZeroFillTo(int minInt)
        {
            if (minInt == 1)
            {
                return DEFAULT;
            }
            else if (minInt >= 0 && minInt < RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return new IntegerWidth(minInt, -1);
            }
            else
            {
                throw new ArgumentException(
                        "Integer digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Exception type
            }
        }

        /**
         * Truncate numbers exceeding a certain number of numerals before the decimal separator.
         *
         * For example, with maxInt=3, the number 1234 will get printed as "234".
         *
         * @param maxInt
         *            The maximum number of places before the decimal separator.
         * @return An IntegerWidth for passing to the NumberFormatter integerWidth() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual IntegerWidth TruncateAt(int maxInt)
        {
            if (maxInt == this.maxInt)
            {
                return this;
            }
            else if (maxInt >= 0 && maxInt < RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return new IntegerWidth(minInt, maxInt);
            }
            else if (maxInt == -1)
            {
                return new IntegerWidth(minInt, maxInt);
            }
            else
            {
                throw new ArgumentException(
                        "Integer digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Exception type
            }
        }
    }
}
