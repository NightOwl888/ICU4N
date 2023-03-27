using ICU4N.Support.Text;
using System;
using System.Diagnostics;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    internal partial class NumberFormatRule
    {
        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Formats the <paramref name="number"/>, and inserts the resulting text into <paramref name="toInsertInto"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the resultant text should be inserted.</param>
        /// <param name="pos">The position in toInsertInto where the resultant text should be inserted.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public void DoFormat(long number, ref ValueStringBuilder toInsertInto, int pos, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

            // first, insert the rule's rule text into toInsertInto at the
            // specified position, then insert the results of the substitutions
            // into the right places in toInsertInto (notice we do the
            // substitutions in reverse order so that the offsets don't get
            // messed up)
            int pluralRuleStart = ruleText!.Length;
            int lengthOffset = 0;
            if (pluralMessagePattern is null)
            {
                toInsertInto.Insert(pos, ruleText);
            }
            else
            {
                pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
                int pluralRuleEnd = ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal);
                int initialLength = toInsertInto.Length;
                if (pluralRuleEnd < ruleText.Length - 1)
                {
#if FEATURE_SPAN
                    toInsertInto.Insert(pos, ruleText.AsSpan(pluralRuleEnd + 2));
#else
                    toInsertInto.Insert(pos, ruleText.Substring(pluralRuleEnd + 2));
#endif
                }
                toInsertInto.Insert(pos, IcuNumber.FormatPlural((double)number / Power(radix, exponent), null, pluralMessagePattern, info));
                if (pluralRuleStart > 0)
                {
#if FEATURE_SPAN
                    toInsertInto.Insert(pos, ruleText.AsSpan(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
#else
                    toInsertInto.Insert(pos, ruleText.Substring(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
#endif
                }
                lengthOffset = ruleText.Length - (toInsertInto.Length - initialLength);
            }
            sub2?.DoSubstitution(number, ref toInsertInto, pos - (sub2.Pos > pluralRuleStart ? lengthOffset : 0), info, recursionCount);
            sub1?.DoSubstitution(number, ref toInsertInto, pos - (sub1.Pos > pluralRuleStart ? lengthOffset : 0), info, recursionCount);
        }

        /// <summary>
        /// Formats the <paramref name="number"/>, and inserts the resulting text into <paramref name="toInsertInto"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the resultant text should be inserted.</param>
        /// <param name="pos">The position in toInsertInto where the resultant text should be inserted.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public void DoFormat(double number, ref ValueStringBuilder toInsertInto, int pos, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

            // first, insert the rule's rule text into toInsertInto at the
            // specified position, then insert the results of the substitutions
            // into the right places in toInsertInto
            // [again, we have two copies of this routine that do the same thing
            // so that we don't sacrifice precision in a long by casting it
            // to a double]
            int pluralRuleStart = ruleText!.Length; // ICU4N: This is always populated during construction in the ExtractSubstitutions method.
            int lengthOffset = 0;
            if (pluralMessagePattern is null)
            {
                toInsertInto.Insert(pos, ruleText);
            }
            else
            {
                pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
                int pluralRuleEnd = ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal);
                int initialLength = toInsertInto.Length;
                if (pluralRuleEnd < ruleText.Length - 1)
                {
#if FEATURE_SPAN
                    toInsertInto.Insert(pos, ruleText.AsSpan(pluralRuleEnd + 2));
#else
                    toInsertInto.Insert(pos, ruleText.Substring(pluralRuleEnd + 2));
#endif
                }
                double pluralVal = number;
                if (0 <= pluralVal && pluralVal < 1)
                {
                    // We're in a fractional rule, and we have to match the NumeratorSubstitution behavior.
                    // 2.3 can become 0.2999999999999998 for the fraction due to rounding errors.
                    pluralVal = Math.Round(pluralVal * Power(radix, exponent)); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
                }
                else
                {
                    pluralVal = pluralVal / Power(radix, exponent);
                }
                toInsertInto.Insert(pos, IcuNumber.FormatPlural((long)pluralVal, null, pluralMessagePattern, info));
                if (pluralRuleStart > 0)
                {
#if FEATURE_SPAN
                    toInsertInto.Insert(pos, ruleText.AsSpan(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
#else
                    toInsertInto.Insert(pos, ruleText.Substring(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
#endif
                }
                lengthOffset = ruleText.Length - (toInsertInto.Length - initialLength);
            }
            sub2?.DoSubstitution(number, ref toInsertInto, pos - (sub2.Pos > pluralRuleStart ? lengthOffset : 0), info, recursionCount);
            sub1?.DoSubstitution(number, ref toInsertInto, pos - (sub1.Pos > pluralRuleStart ? lengthOffset : 0), info, recursionCount);
        }

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
