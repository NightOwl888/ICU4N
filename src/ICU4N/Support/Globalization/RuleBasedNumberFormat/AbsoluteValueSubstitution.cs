using System;
#nullable enable

namespace ICU4N.Globalization
{
    //===================================================================
    // AbsoluteValueSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that formats the absolute value of the number.
    /// This substitution is represented by &gt;&gt; in a negative-number rule.
    /// </summary>
    internal sealed class AbsoluteValueSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs an <see cref="AbsoluteValueSubstitution"/>. This just uses the
        /// superclass constructor.
        /// </summary>
        /// <param name="pos">The substitution's position in the owning rule's rule text.</param>
        /// <param name="ruleSet">The rule set that owns this substitution.</param>
        /// <param name="description">The substitution descriptor (i.e., the text
        /// inside the token characters)</param>
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
        internal AbsoluteValueSubstitution(int pos,
                NumberFormatRuleSet ruleSet,
                ReadOnlySpan<char> description)
            : base(pos, ruleSet, description)
        {
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns the absolute value of the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns>Abs(<paramref name="number"/>).</returns>
        public override long TransformNumber(long number)
        {
            return Math.Abs(number);
        }

        /// <summary>
        /// Returns the absolute value of the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns>Abs(<paramref name="number"/>).</returns>
        public override double TransformNumber(double number)
        {
            return Math.Abs(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns the additive inverse of the result of parsing the
        /// substitution (this supersedes the earlier partial result).
        /// </summary>
        /// <param name="newRuleValue">The result of parsing the substitution.</param>
        /// <param name="oldRuleValue">The partial parse result prior to calling
        /// this function.</param>
        /// <returns>-<paramref name="newRuleValue"/>.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return -newRuleValue;
        }

        /// <summary>
        /// Sets the upper bound beck up to consider all rules.
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
        /// The token character for an AbsoluteValueSubstitution is '&gt;'. Returns '&gt;'.
        /// </summary>
        private protected override char TokenChar => '>';
    }
}
