using ICU4N.Text;
using System;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // MultiplierSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that divides the number being formatted by the rule's
    /// divisor and formats the quotient. Represented by &lt;&lt; in normal
    /// rules.
    /// </summary>
    internal sealed class MultiplierSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The divisor of the rule that owns this substitution.
        /// </summary>
        internal long divisor; // Internal for testing

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="MultiplierSubstitution"/>.  This uses the superclass
        /// constructor to initialize most members, but this substitution
        /// also maintains its own copy of its rule's divisor.
        /// </summary>
        /// <param name="pos">The substitution's position in its rule's rule text.</param>
        /// <param name="rule">The rule that owns this substitution.</param>
        /// <param name="ruleSet">The ruleSet this substitution uses to format its result.</param>
        /// <param name="description">The description describing this substitution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="rule"/>.<see cref="NumberFormatRule.Divisor"/> is 0,
        /// which would cause infinite recursion.</exception>
        internal MultiplierSubstitution(int pos,
                               NumberFormatRule rule,
                               NumberFormatRuleSet ruleSet,
                               ReadOnlySpan<char> description)
            : base(pos, ruleSet, description)
        {
            if (rule is null)
                throw new ArgumentNullException(nameof(rule));

            // the owning rule's divisor affects the behavior of this
            // substitution.  Rather than keeping a back-pointer to the
            // rule, we keep a copy of the divisor
            this.divisor = rule.Divisor;

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException(string.Concat("Substitution with divisor 0 ", description.Slice(0, pos), // ICU4N: Checked 2nd parameter
                             " | ", description.Slice(pos)));
            }
        }

        /// <summary>
        /// Sets the substitution's divisor based on the values passed in.
        /// </summary>
        /// <param name="radix">The radix of the divisor.</param>
        /// <param name="exponent">The exponent of the divisor.</param>
        /// <exception cref="InvalidOperationException">The calculated divisor is 0.</exception>
        public override void SetDivisor(int radix, short exponent)
        {
            divisor = NumberFormatRule.Power(radix, exponent);

            if (divisor == 0)
            {
                throw new InvalidOperationException("Substitution with divisor 0");
            }
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Augments the superclass's <see cref="Equals(object?)"/> function by comparing divisors.
        /// </summary>
        /// <param name="that">The other substitution</param>
        /// <returns><c>true</c> if the two substitutions are functionally equal.</returns>
        public override bool Equals(object? that)
        {
            return base.Equals(that) && divisor == ((MultiplierSubstitution)that).divisor;
        }

        /// <inheritdoc/>
        public override int GetHashCode() // ICU4N specific - need to override hash code
        {
            return base.GetHashCode() ^ divisor.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Divides the number by the rule's divisor and returns the quotient.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> divided by the rule's divisor.</returns>
        public override long TransformNumber(long number)
        {
            //return (long)Math.Floor((double)(number / divisor));
            // ICU4N TODO: Tests for this logic (need to find some edge cases that cause the number to flip in Java)
            // source: https://stackoverflow.com/a/28060018
            return (number / divisor - Convert.ToInt32(((number < 0) ^ (divisor < 0)) && (number % divisor != 0)));
        }

        /// <summary>
        /// Divides the number by the rule's divisor and returns the quotient.
        /// This is an integral quotient if we're filling in the substitution
        /// using another rule set, but it's the full quotient (integral and
        /// fractional parts) if we're filling in the substitution using
        /// a <see cref="DecimalFormat"/>. (This allows things such as "1.2 million".)
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> divided by the rule's divisor.</returns>
        public override double TransformNumber(double number)
        {
            if (ruleSet == null)
            {
                return number / divisor;
            }
            else
            {
                return Math.Floor(number / divisor);
            }
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns <paramref name="newRuleValue"/> times the divisor. Ignores <paramref name="oldRuleValue"/>.
        /// (The result of matching a &lt;&lt; substitution supersedes the base
        /// value of the rule that contains it.)
        /// </summary>
        /// <param name="newRuleValue">The result of matching the substitution.</param>
        /// <param name="oldRuleValue">The base value of the rule containing the substitution.</param>
        /// <returns><paramref name="newRuleValue"/> * divisor.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue * divisor;
        }

        /// <summary>
        /// Sets the upper bound down to the rule's divisor.
        /// </summary>
        /// <param name="oldUpperBound">Ignored.</param>
        /// <returns>The rule's divisor.</returns>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return divisor;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /// <summary>
        /// The token character for a multiplier substitution is &lt;.Returns '&lt;'.
        /// </summary>
        private protected override char TokenChar => '<';
    }
#endif
}
