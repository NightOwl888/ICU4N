using System;
using System.Collections.Generic;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    internal class NumberFormatRules : INumberFormatRules
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The formatter's rule sets.
        /// </summary>
        [NonSerialized]
        private NumberFormatRuleSet[] ruleSets = null;

        /// <summary>
        /// The formatter's rule names mapped to rule sets.
        /// </summary>
        [NonSerialized]
        private IDictionary<string, NumberFormatRuleSet> ruleSetsMap = null;

        ///// <summary>
        ///// A pointer to the formatter's default rule set. This is always included
        ///// in <see cref="ruleSets"/>.
        ///// </summary>
        //[NonSerialized]
        //private NumberFormatRuleSet defaultRuleSet = null; // ICU4N TODO: API - Change to CurrentRuleSet to match .NET?

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        //-----------------------------------------------------------------------
        // INumberFormatRules members
        //-----------------------------------------------------------------------

        ///// <summary>
        ///// Gets a reference to the formatter's default rule set. The default
        ///// rule set is the last public rule set in the description, or the one
        ///// most recently set.
        ///// </summary>
        //NumberFormatRuleSet INumberFormatRules.DefaultRuleSet => defaultRuleSet; // ICU4N TODO: API - This should be passed into the Format method and if not passed, it should use the rule parsed from NumberingSystem.Description (See internal static NumberFormat CreateInstance(UCultureInfo desiredLocale, NumberFormatStyle choice))

        /// <summary>
        /// Returns the named rule set. Throws an <see cref="ArgumentException"/>
        /// if this formatter doesn't have a rule set with that name.
        /// </summary>
        /// <param name="name">The name of the desired rule set.</param>
        /// <returns>The rule set with that name.</returns>
        /// <exception cref="ArgumentException">No rule exists with the provided <paramref name="name"/>.</exception>
        NumberFormatRuleSet INumberFormatRules.FindRuleSet(string name)
        {
            if (!ruleSetsMap.TryGetValue(name, out NumberFormatRuleSet result) || result is null)
            {
                throw new ArgumentException("No rule set named " + name);
            }
            return result;
        }
    }
#endif
}
