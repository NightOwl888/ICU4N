using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines a rounding strategy based on a number of fraction places and optionally significant digits to be
    /// used when formatting numbers in <see cref="NumberFormatter"/>.
    /// <para/>
    /// To create a <see cref="FractionRounder"/>, use one of the factory methods on <see cref="Rounder"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal abstract class FractionRounder : Rounder
    {
        /* package-private */
        internal FractionRounder()
        {
        }

        /**
         * Ensure that no less than this number of significant digits are retained when rounding according to fraction
         * rules.
         *
         * <p>
         * For example, with integer rounding, the number 3.141 becomes "3". However, with minimum figures set to 2, 3.141
         * becomes "3.1" instead.
         *
         * <p>
         * This setting does not affect the number of trailing zeros. For example, 3.01 would print as "3", not "3.0".
         *
         * @param minSignificantDigits
         *            The number of significant figures to guarantee.
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual Rounder WithMinDigits(int minSignificantDigits)
        {
            if (minSignificantDigits > 0 && minSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructFractionSignificant(this, minSignificantDigits, -1);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: exception type
            }
        }

        /**
         * Ensure that no more than this number of significant digits are retained when rounding according to fraction
         * rules.
         *
         * <p>
         * For example, with integer rounding, the number 123.4 becomes "123". However, with maximum figures set to 2, 123.4
         * becomes "120" instead.
         *
         * <p>
         * This setting does not affect the number of trailing zeros. For example, with fixed fraction of 2, 123.4 would
         * become "120.00".
         *
         * @param maxSignificantDigits
         *            Round the number to no more than this number of significant figures.
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual Rounder WithMaxDigits(int maxSignificantDigits)
        {
            if (maxSignificantDigits > 0 && maxSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructFractionSignificant(this, -1, maxSignificantDigits);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Exception type
            }
        }
    }
}
