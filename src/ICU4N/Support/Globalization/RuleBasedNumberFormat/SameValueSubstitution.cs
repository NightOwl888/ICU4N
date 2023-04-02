using System;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // SameValueSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that passes the value passed to it through unchanged.
    /// Represented by == in rule descriptions.
    /// </summary>
    internal sealed class SameValueSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="SameValueSubstitution"/>. This function just uses the
        /// superclass constructor, but it performs a check that this
        /// substitution doesn't call the rule set that owns it, since that
        /// would lead to infinite recursion.
        /// </summary>
        /// <param name="pos">The substitution's position in the owning rule's rule text.</param>
        /// <param name="ruleSet">The rule set that owns this substitution.</param>
        /// <param name="description">The substitution descriptor (i.e., the text
        /// inside the token characters).</param>
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
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="description"/> equals "==", which is not a legal token.
        /// </exception>
        internal SameValueSubstitution(int pos,
                          NumberFormatRuleSet ruleSet,
                          ReadOnlySpan<char> description)
            : base(pos, ruleSet, description)
        {
            if (description.Equals("==", StringComparison.Ordinal))
            {
                throw new ArgumentException("== is not a legal token");
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns <paramref name="number"/> unchanged.
        /// </summary>
        /// <param name="number">The number to transform.</param>
        /// <returns><paramref name="number"/> unchanged.</returns>
        public override long TransformNumber(long number)
        {
            return number;
        }

        /// <summary>
        /// Returns <paramref name="number"/> unchanged.
        /// </summary>
        /// <param name="number">The number to transform.</param>
        /// <returns><paramref name="number"/> unchanged.</returns>
        public override double TransformNumber(double number)
        {
            return number;
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns <paramref name="newRuleValue"/> and ignores <paramref name="oldRuleValue"/>. (The value we got
        /// matching the substitution supersedes the value of the rule
        /// that owns the substitution.)
        /// </summary>
        /// <param name="newRuleValue">The value resulting from matching the substitution.</param>
        /// <param name="oldRuleValue">The value of the rule containing the substitution.</param>
        /// <returns><paramref name="newRuleValue"/> unchanged.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue;
        }

        /// <summary>
        /// <see cref="SameValueSubstitution"/> doesn't change the upper bound.
        /// </summary>
        /// <param name="oldUpperBound">The current upper bound.</param>
        /// <returns><paramref name="oldUpperBound"/> unchanged.</returns>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return oldUpperBound;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /// <summary>
        /// The token character for a <see cref="SameValueSubstitution"/> is '='. Returns '='.
        /// </summary>
        private protected override char TokenChar => '=';
    }
#endif
}
