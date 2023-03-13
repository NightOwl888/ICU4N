using System;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // IntegralPartSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that formats the number's integral part. This is
    /// represented by &lt;&lt; in a fraction rule.
    /// </summary>
    internal class IntegralPartSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs an <see cref="IntegralPartSubstitution"/>. This just calls
        /// the superclass constructor.
        /// </summary>
        /// <param name="pos">The substitution's position in the owning rule's rule text.</param>
        /// <param name="ruleSet">The rule set that owns this substitution.</param>
        /// <param name="description">The substitution descriptor (i.e., the text
        /// inside the token characters)</param>
        internal IntegralPartSubstitution(int pos,
                NumberFormatRuleSet ruleSet,
                ReadOnlySpan<char> description)
            : base(pos, ruleSet, description)
        {
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns the number's integral part. (For a long, that's just the
        /// number unchanged.)
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> unchanged.</returns>
        public override long TransformNumber(long number)
        {
            return number;
        }

        /// <summary>
        /// Returns the number's integral part.
        /// </summary>
        /// <param name="number">The integral part of the number being formatted.</param>
        /// <returns>Floor(<paramref name="number"/>)</returns>
        public override double TransformNumber(double number)
        {
            return Math.Floor(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns the sum of the result of parsing the substitution and the
        /// owning rule's base value.  (The owning rule, at best, has an
        /// integral-part substitution and a fractional-part substitution,
        /// so we can safely just add them.)
        /// </summary>
        /// <param name="newRuleValue">The result of matching the substitution.</param>
        /// <param name="oldRuleValue">The partial result of the parse prior to
        /// calling this function.</param>
        /// <returns><paramref name="oldRuleValue"/> + <paramref name="newRuleValue"/>.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue + oldRuleValue;
        }

        /// <summary>
        /// An <see cref="IntegralPartSubstitution"/> sets the upper bound back up so all
        /// potentially matching rules are considered.
        /// </summary>
        /// <param name="oldUpperBound">Ignored.</param>
        /// <returns><see cref="double.MaxValue"/>.</returns>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return double.MaxValue;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /// <summary>
        /// An <see cref="IntegralPartSubstitution"/>'s token character is '&lt;'. Returns '&lt;'.
        /// </summary>
        private protected override char TokenChar => '<';
    }
#endif
}
