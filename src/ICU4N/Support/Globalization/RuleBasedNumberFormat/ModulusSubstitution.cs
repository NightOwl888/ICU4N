using ICU4N.Support.Text;
using System;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // ModulusSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that divides the number being formatted by the its rule's
    /// divisor and formats the remainder. Represented by "&gt;&gt;" in a
    /// regular rule.
    /// </summary>
    internal sealed class ModulusSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The divisor of the rule owning this substitution
        /// </summary>
        internal long divisor; // Internal for testing

        /// <summary>
        /// If this is a &gt;&gt;&gt; substitution, the rule to use to format
        /// the substitution value.  Otherwise, <c>null</c>.
        /// </summary>
        internal readonly NumberFormatRule? ruleToUse; // Internal for testing

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="ModulusSubstitution"/>. In addition to the inherited
        /// members, a <see cref="ModulusSubstitution"/> keeps track of the divisor of the
        /// rule that owns it, and may also keep a reference to the rule
        /// that precedes the rule containing this substitution in the rule
        /// set's rule list.
        /// </summary>
        /// <param name="pos">The substitution's position in its rule's rule text.</param>
        /// <param name="rule">The rule that owns this substitution.</param>
        /// <param name="rulePredecessor">The rule that precedes this substitution's
        /// rule in its rule set's rule list.</param>
        /// <param name="ruleSet">The rule set that owns this substitution.</param>
        /// <param name="description">The description for this substitution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="description"/> length is 1.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="description"/> doesn't begin with and end with the same character.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="description"/> starts with a <see cref="char"/> other than '%', '#', '0', or '&gt;'.
        /// </exception>
        /// <exception cref="InvalidOperationException"><paramref name="rule"/>.<see cref="NumberFormatRule.Divisor"/> is 0, 
        /// which would cause infinite recursion.</exception>
        internal ModulusSubstitution(int pos,
                NumberFormatRule rule,
                NumberFormatRule? rulePredecessor,
                NumberFormatRuleSet ruleSet,
                ReadOnlySpan<char> description)
            : base(pos, ruleSet, description)
        {
            if (rule is null)
                throw new ArgumentNullException(nameof(rule));

            // the owning rule's divisor controls the behavior of this
            // substitution: rather than keeping a backpointer to the rule,
            // we keep a copy of the divisor
            this.divisor = rule.Divisor;

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException(string.Concat("Substitution with bad divisor (" + divisor + ") ", description.Slice(0, pos), // ICU4N: Checked 2nd parameter
                        " | ", description.Slice(pos)));
            }

            // the >>> token doesn't alter how this substitution calculates the
            // values it uses for formatting and parsing, but it changes
            // what's done with that value after it's obtained: >>> short-
            // circuits the rule-search process and goes straight to the
            // specified rule to format the substitution value
            if (description.Equals(">>>", StringComparison.Ordinal))
            {
                ruleToUse = rulePredecessor;
            }
            else
            {
                ruleToUse = null;
            }
        }

        /// <summary>
        /// Makes the substitution's divisor conform to that of the rule
        /// that owns it. Used when the divisor is determined after creation.
        /// </summary>
        /// <param name="radix">The radix of the divisor.</param>
        /// <param name="exponent">The exponent of the divisor.</param>
        /// <exception cref="InvalidOperationException">The calculated divisor is 0.</exception>
        public override void SetDivisor(int radix, short exponent)
        {
            divisor = NumberFormatRule.Power(radix, exponent);

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException("Substitution with bad divisor");
            }
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Augments the inherited <see cref="Equals(object?)"/> function by comparing divisors.
        /// </summary>
        /// <param name="that">The other substitution.</param>
        /// <returns><c>true</c> if the two substitutions are functionally equivalent.</returns>
        public override bool Equals(object? that)
        {
            if (base.Equals(that))
            {
                ModulusSubstitution that2 = (ModulusSubstitution)that;

                return divisor == that2.divisor;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() // ICU4N specific - need to override hash code
        {
            return base.GetHashCode() ^ divisor.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        ///// <summary>
        ///// If this is a &gt;&gt;&gt; substitution, use <see cref="ruleToUse"/> to fill in
        ///// the substitution. Otherwise, just use the superclass function.
        ///// </summary>
        ///// <param name="number">The number being formatted.</param>
        ///// <param name="toInsertInto">The string to insert the result of this substitution
        ///// into.</param>
        ///// <param name="position">The position of the rule text in <paramref name="toInsertInto"/>.</param>
        ///// <param name="recursionCount">The number of recursive calls to this method.</param>
        //public override void DoSubstitution(long number, StringBuilder toInsertInto, int position, int recursionCount)
        //{
        //    // if this isn't a >>> substitution, just use the inherited version
        //    // of this function (which uses either a rule set or a DecimalFormat
        //    // to format its substitution value)
        //    if (ruleToUse == null)
        //    {
        //        base.DoSubstitution(number, toInsertInto, position, recursionCount);

        //    }
        //    else
        //    {
        //        // a >>> substitution goes straight to a particular rule to
        //        // format the substitution value
        //        long numberToFormat = TransformNumber(number);
        //        ruleToUse.DoFormat(numberToFormat, toInsertInto, position + pos, recursionCount);
        //    }
        //}

        /// <summary>
        /// If this is a &gt;&gt;&gt; substitution, use <see cref="ruleToUse"/> to fill in
        /// the substitution. Otherwise, just use the superclass function.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string to insert the result of this substitution
        /// into.</param>
        /// <param name="position">The position of the rule text in <paramref name="toInsertInto"/>.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public override void DoSubstitution(double number, ref ValueStringBuilder toInsertInto, int position, UNumberFormatInfo info, int recursionCount)
        {
            // if this isn't a >>> substitution, just use the inherited version
            // of this function (which uses either a rule set or a DecimalFormat
            // to format its substitution value)
            if (ruleToUse == null)
            {
                base.DoSubstitution(number, ref toInsertInto, position, info, recursionCount);

            }
            else
            {
                // a >>> substitution goes straight to a particular rule to
                // format the substitution value
                double numberToFormat = TransformNumber(number);
                ruleToUse.DoFormat(numberToFormat, ref toInsertInto, position + pos, info, recursionCount);
            }
        }

        /// <summary>
        /// Divides the number being formatted by the rule's divisor and
        /// returns the remainder.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> mod divisor.</returns>
        public override long TransformNumber(long number)
        {
            return number % divisor;
        }

        /// <summary>
        /// Divides the number being formatted by the rule's divisor and
        /// returns the remainder.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> mod divisor.</returns>
        public override double TransformNumber(double number)
        {
            return Math.Floor(number % divisor);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        ///// <summary>
        ///// If this is a &gt;&gt;&gt; substitution, match only against <see cref="ruleToUse"/>.
        ///// Otherwise, use the superclass function.
        ///// </summary>
        ///// <param name="text">The string to parse.</param>
        ///// <param name="parsePosition">Ignored on entry, updated on exit to point to
        ///// the first unmatched character.</param>
        ///// <param name="baseValue">The partial parse result prior to calling this
        ///// routine.</param>
        ///// <param name="upperBound">When searching the rule set for a rule
        ///// matching the string passed in, only rules with base values
        ///// lower than this are considered.</param>
        ///// <param name="lenientParse">If <c>true</c> and matching against rules fails,
        ///// the substitution will also try matching the text against
        ///// numerals using a default-constructed <see cref="NumberFormat"/>. If <c>false</c>,
        ///// no extra work is done.  (This value is false whenever the
        ///// formatter isn't in lenient-parse mode, but is also false
        ///// under some conditions even when the formatter _is_ in
        ///// lenient-parse mode.)</param>
        ///// <returns></returns>
        //public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
        //                      double upperBound, bool lenientParse)
        //{
        //    // if this isn't a >>> substitution, we can just use the
        //    // inherited parse() routine to do the parsing
        //    if (ruleToUse == null)
        //    {
        //        return base.DoParse(text, parsePosition, baseValue, upperBound, lenientParse);

        //    }
        //    else
        //    {
        //        // but if it IS a >>> substitution, we have to do it here: we
        //        // use the specific rule's doParse() method, and then we have to
        //        // do some of the other work of NumberFormatRuleSet.parse()
        //        Number tempResult = ruleToUse.DoParse(text, parsePosition, false, upperBound);

        //        if (parsePosition.Index != 0)
        //        {
        //            double result = tempResult.ToDouble();

        //            result = ComposeRuleValue(result, baseValue);
        //            if (result == (long)result) // ICU4N: This is quite a bit faster than using result.IsInteger()
        //            {
        //                return Long.GetInstance((long)result);
        //            }
        //            else
        //            {
        //                return Double.GetInstance(result);
        //            }
        //        }
        //        else
        //        {
        //            return tempResult;
        //        }
        //    }
        //}

        /// <summary>
        /// Returns the highest multiple of the rule's divisor that its less
        /// than or equal to <paramref name="oldRuleValue"/>, plus <paramref name="newRuleValue"/>. (The result
        /// is the sum of the result of parsing the substitution plus the
        /// base value of the rule containing the substitution, but if the
        /// owning rule's base value isn't an even multiple of its divisor,
        /// we have to round it down to a multiple of the divisor, or we
        /// get unwanted digits in the result.)
        /// </summary>
        /// <param name="newRuleValue">The result of parsing the substitution.</param>
        /// <param name="oldRuleValue">The base value of the rule containing the substitution.</param>
        /// <returns></returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return (oldRuleValue - (oldRuleValue % divisor)) + newRuleValue;
        }

        /// <summary>
        /// Sets the upper bound down to the owning rule's divisor.
        /// </summary>
        /// <param name="oldUpperBound">Ignored.</param>
        /// <returns>The owning rule's divisor.</returns>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return divisor;
        }

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c>. This is a ModulusSubstitution.
        /// </summary>
        public override bool IsModulusSubstitution => true;

        /// <summary>
        /// The token character of a <see cref="ModulusSubstitution"/> is '&gt;'. Returns '&gt;'.
        /// </summary>
        private protected override char TokenChar => '>';
    }
#endif
}
