using System;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    internal partial class NumberFormatRule
    {
        // ICU4N TODO: Main Formatting methods

        /// <summary>
        /// This is an equivalent to <see cref="Math.Pow(double, double)"/> that accurately works on 64-bit numbers.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>radix ** exponent.</returns>
        /// <seealso cref="Math.Pow(double, double)"/>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="exponent"/> is less than zero.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="base"/> is less than zero.
        /// </exception>
        internal static long Power(long @base, short exponent) // ICU4N TODO: Does this really belong under "formatting"?
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent can not be negative");
            if (@base < 0)
                throw new ArgumentOutOfRangeException(nameof(@base), "Base can not be negative");

            long result = 1;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                {
                    result *= @base;
                }
                @base *= @base;
                exponent >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Used by the owning rule set to determine whether to invoke the
        /// rollback rule (i.e., whether this rule or the one that precedes
        /// it in the rule set's list should be used to Format the number).
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><c>true</c> if the rule set should use the rule that precedes
        /// this one in its list; <c>false</c> if it should use this rule.</returns>
        public bool ShouldRollBack(long number)
        {
            // we roll back if the rule contains a modulus substitution,
            // the number being formatted is an even multiple of the rule's
            // divisor, and the rule's base value is NOT an even multiple
            // of its divisor
            // In other words, if the original description had
            //    100: << hundred[ >>];
            // that expands into
            //    100: << hundred;
            //    101: << hundred >>;
            // internally.  But when we're formatting 200, if we use the rule
            // at 101, which would normally apply, we get "two hundred zero".
            // To prevent this, we roll back and use the rule at 100 instead.
            // This is the logic that makes this happen: the rule at 101 has
            // a modulus substitution, its base value isn't an even multiple
            // of 100, and the value we're trying to Format _is_ an even
            // multiple of 100.  This is called the "rollback rule."
            if (!((sub1 != null && sub1.IsModulusSubstitution) || (sub2 != null && sub2.IsModulusSubstitution)))
            {
                return false;
            }
            long divisor = Power(radix, exponent);
            return (number % divisor) == 0 && (baseValue % divisor) != 0;
        }
    }
#endif
}
