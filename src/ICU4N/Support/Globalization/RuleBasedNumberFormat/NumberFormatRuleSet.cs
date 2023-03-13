using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    /// <summary>
    /// A collection of rules used by a <see cref="INumberFormatRules"/> to format and
    /// parse numbers. It is the responsibility of a <see cref="NumberFormatRuleSet"/> to select an
    /// appropriate rule for formatting a particular number and dispatch
    /// control to it, and to arbitrate between different rules when parsing
    /// a number.
    /// </summary>
    internal partial class NumberFormatRuleSet
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The rule set's name
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The rule set's regular rules
        /// </summary>
        private NumberFormatRule[] rules;


        /// <summary>
        /// The rule set's non-numerical rules like negative, fractions, infinity and NaN
        /// </summary>
        internal readonly NumberFormatRule[] nonNumericalRules = new NumberFormatRule[6];

        /// <summary>
        /// These are a pile of fraction rules in declared order. They may have alternate
        /// ways to represent fractions.
        /// </summary>
        private List<NumberFormatRule> fractionRules;

        /// <summary>-x</summary>
        private const int NegativeRuleIndex = 0;
        /// <summary>x.x</summary>
        private const int ImproperFractionRuleIndex = 1;
        /// <summary>0.x</summary>
        private const int ProperFractionRuleIndex = 2;
        /// <summary>x.0</summary>
        private const int MasterRuleIndex = 3;
        /// <summary>Inf</summary>
        private const int InfinityRuleIndex = 4;
        /// <summary>NaN</summary>
        private const int NaNRuleIndex = 5;

        /// <summary>
        /// The <see cref="INumberFormatRules"/> that owns this rule
        /// </summary>
        internal readonly INumberFormatRules owner;

        /// <summary>
        /// True if the rule set is a fraction rule set.  A fraction rule set
        /// is a rule set that is used to Format the fractional part of a
        /// number.  It is called from a >> substitution in another rule set's
        /// fraction rule, and is only called upon to Format values between
        /// 0 and 1.  A fraction rule set has different rule-selection
        /// behavior than a regular rule set.
        /// </summary>
        private bool isFractionRuleSet = false;

        /// <summary>
        /// True if the rule set is parseable.
        /// </summary>
        private readonly bool isParseable;

        /// <summary>
        /// Limit of recursion. It's about a 64 bit number formatted in base 2.
        /// </summary>
        private const int RecursionLimit = 64;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        /// <summary>
        /// Flags this rule set as a fraction rule set. This function is
        /// called during the construction process once we know this rule
        /// set is a fraction rule set. We don't know a rule set is a
        /// fraction rule set until we see it used somewhere. This function
        /// is not and must not be called at any time other than during
        /// construction of a <see cref="RuleBasedNumberFormat"/>.
        /// </summary>
        public void MakeIntoFractionRuleSet()
        {
            isFractionRuleSet = true;
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Compares two rule sets for equality.
        /// </summary>
        /// <param name="that">The other rule set.</param>
        /// <returns><c>true</c> if the two rule sets are equivalent.</returns>
        public override bool Equals(object that)
        {
            // if different classes, they're not equal
            if (that is NumberFormatRuleSet that2)
            {
                // otherwise, compare the members one by one...
                if (!name.Equals(that2.name, StringComparison.Ordinal)
                        || rules.Length != that2.rules.Length
                        || isFractionRuleSet != that2.isFractionRuleSet)
                {
                    return false;
                }

                // ICU4N specific - we are testing the entire rule set (rather than the current rule as was the case in ICU4J)
                // so this logic is different. We never set the "current" rule because this object is functionally immutable.
                if (fractionRules is null)
                {
                    if (that2.fractionRules is not null)
                        return false;
                }
                else if (fractionRules is not null)
                {
                    if (that2.fractionRules is null)
                        return false;

                    for (int i = 0; i < fractionRules.Count; i++)
                    {
                        if (!Equals(fractionRules[i], that2.fractionRules[i]))
                        {
                            return false;
                        }
                    }
                }

                // ...then compare the non-numerical rule lists...
                for (int i = 0; i < nonNumericalRules.Length; i++)
                {
                    if (!Equals(nonNumericalRules[i], that2.nonNumericalRules[i]))
                    {
                        return false;
                    }
                }

                // ...then compare the rule lists...
                for (int i = 0; i < rules.Length; i++)
                {
                    if (!Equals(rules[i], that2.rules[i]))
                    {
                        return false;
                    }
                }

                // ...and if we make it here, they're equal
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() // ICU4N TODO: Create real hash code - we can definitely rule out cases here.
        {
            //assert false : "hashCode not designed";
            return 42;
        }

        /// <summary>
        /// Builds a textual representation of a rule set.
        /// </summary>
        /// <returns>A textual representation of a rule set. This won't
        /// necessarily be the same description that the rule set was
        /// constructed with, but it will produce the same results.</returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            // the rule set name goes first...
            result.Append(name).Append(":\n");

            // followed by the regular rules...
            foreach (NumberFormatRule rule in rules)
            {
                result.Append(rule.ToString()).Append('\n');
            }

            // followed by the special rules (if they exist)
            foreach (NumberFormatRule rule in nonNumericalRules)
            {
                if (rule != null)
                {
                    if (fractionRules != null && // ICU4N: There was a bug in ICU4J here - this collection may be null, so we need to use the default value in those cases
                        (rule.BaseValue == NumberFormatRule.ImproperFractionRule
                        || rule.BaseValue == NumberFormatRule.ProperFractionRule
                        || rule.BaseValue == NumberFormatRule.MasterRule))
                    {
                        foreach (NumberFormatRule fractionRule in fractionRules)
                        {
                            if (fractionRule.BaseValue == rule.BaseValue)
                            {
                                result.Append(fractionRule.ToString()).Append('\n');
                            }
                        }
                    }
                    else
                    {
                        result.Append(rule.ToString()).Append('\n');
                    }
                }
            }

            return result.ToString();
        }

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /// <summary>
        /// <c>true</c> if this rule is a fraction rule set; <c>false</c> if it isn't.
        /// </summary>
        public bool IsFractionSet => isFractionRuleSet;

        /// <summary>
        /// Gets the rule set's names
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Returns <c>true</c> if the rule set is public.
        /// </summary>
        public bool IsPublic => !name.StartsWith("%%", StringComparison.Ordinal);


        /// <summary>
        /// Returns <c>true</c> if the rule set can be used for parsing.
        /// </summary>
        public bool IsParseable => isParseable;
    }
#endif
}
