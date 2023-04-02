using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;
#nullable enable

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
    internal sealed partial class NumberFormatRuleSet
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
        internal NumberFormatRule[]? rules; // Internal for testing

        /// <summary>
        /// The rule set's non-numerical rules like negative, fractions, infinity and NaN
        /// </summary>
        internal readonly NumberFormatRule?[] nonNumericalRules = new NumberFormatRule[6];

        /// <summary>
        /// These are a pile of fraction rules in declared order. They may have alternate
        /// ways to represent fractions.
        /// </summary>
        internal List<NumberFormatRule>? fractionRules; // Internal for testing

        /// <summary>-x</summary>
        internal const int NegativeRuleIndex = 0; // Internal for testing
        /// <summary>x.x</summary>
        internal const int ImproperFractionRuleIndex = 1; // Internal for testing
        /// <summary>0.x</summary>
        internal const int ProperFractionRuleIndex = 2; // Internal for testing
        /// <summary>x.0</summary>
        internal const int MasterRuleIndex = 3; // Internal for testing
        /// <summary>Inf</summary>
        internal const int InfinityRuleIndex = 4; // Internal for testing
        /// <summary>NaN</summary>
        internal const int NaNRuleIndex = 5; // Internal for testing

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

        /// <summary>
        /// Constructs a rule set.
        /// </summary>
        /// <param name = "owner" > The <see cref= "INumberFormatRules" /> that owns this rule set.</param>
        /// <param name = "description" > The description of this rule set.</param>
        /// <exception cref="ArgumentNullException"><paramref name= "owner" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name= "description" /> is zero length.
        /// <para/>
        /// -or-
        /// <para/>
        /// The rule set name within<paramref name="description"/> doesn't end in a colon (:).
        /// </exception>
        public NumberFormatRuleSet(INumberFormatRules owner, ReadOnlySpan<char> description)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            ExtractRuleSetName(description, out ReadOnlySpan<char> name, out isParseable);
            this.name = new string(name);

            // all of the other members of NumberFormatRuleSet are initialized
            // by ParseRules()
        }

        /// <summary>
        /// Processes a rule set description, separating name from description as appropriate.
        /// </summary>
        /// <param name="description">The rule set text.</param>
        /// <param name="name">Upon return, contains the name of the rule set.</param>
        /// <param name="isParseable">Upon return, contains a value indicating whether the rule set allows parsing.</param>
        /// <returns>The description minus any <paramref name="name"/> token, for further processing.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="description"/> is zero length.
        /// <para/>
        /// -or-
        /// <para/>
        /// The rule set name within <paramref name="description"/> doesn't end in a colon (:).
        /// </exception>
        private static ReadOnlySpan<char> ExtractRuleSetName(ReadOnlySpan<char> description, out ReadOnlySpan<char> name, out bool isParseable)
        {
            if (description.Length == 0)
                throw new ArgumentException("Empty rule set description");

            // if the description begins with a rule set name (the rule set
            // name can be omitted in formatter descriptions that consist
            // of only one rule set), copy it out into our "name" member
            // and delete it from the description
            if (description[0] == '%')
            {
                int pos = description.IndexOf(':');
                if (pos == -1)
                {
                    throw new ArgumentException("Rule set name doesn't end in colon");
                }
                else
                {
                    name = description.Slice(0, pos); // ICU4N: Checked 2nd parameter
                    isParseable = !name.EndsWith("@noparse", StringComparison.Ordinal);
                    if (!isParseable)
                    {
                        name = name.Slice(0, name.Length - 8); // Remove the @noparse from the name
                    }

                    while (pos < description.Length && PatternProps.IsWhiteSpace(description[++pos]))
                    {
                        // ICU4N: Intentionally empty
                    }

                    // Remove the name from the description and return it.
                    return description.Slice(pos);
                }
            }
            else
            {
                // if the description doesn't begin with a rule set name, its
                // name is "%default"
                name = "%default";
                isParseable = true;
                return description;
            }
        }

        /// <summary>
        /// Construct the subordinate data structures used by this object.
        /// This function is called by the <see cref="RuleBasedNumberFormat"/> constructor
        /// after all the rule sets have been created to actually parse
        /// the description and build rules from it. Since any rule set
        /// can refer to any other rule set, we have to have created all of
        /// them before we can create anything else.
        /// </summary>
        /// <param name="description">The textual description of this rule set.</param>
        /// <exception cref="ArgumentException">
        /// The rules in <paramref name="description"/> is zero length.
        /// <para/>
        /// -or-
        /// <para/>
        /// The rules in <paramref name="description"/> are not specified in the correct order.
        /// <para/>
        /// -or-
        /// <para/>
        /// A rule within <paramref name="description"/> does not have a defined type ("ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// A rule within <paramref name="description"/> has an invalid type (one other than "ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="description"/> starts with '&lt;'
        /// and its base value is <see cref="NumberFormatRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="description"/> starts with '&gt;'
        /// and <see cref="NumberFormatRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="description"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        public void ParseRules(ReadOnlySpan<char> description)
        {
            if (description.Length == 0)
                throw new ArgumentException("Empty rule set description");

            // This method is expected to be called within a separate loop than the constructor call. So,
            // we must account for the fact that the new loop will contain the ruleset name, stripping it off here.
            // We already stored the name and isParseable values in the constructor, so we ignore them here.
            description = ExtractRuleSetName(description, out var _, out var _);

            // Check again to ensure our description without a name is not empty.
            if (description.Length == 0)
                throw new ArgumentException("Empty rule set description");

            // (the number of elements in the description list isn't necessarily
            // the number of rules-- some descriptions may expend into two rules)
            LinkedList<NumberFormatRule> tempRules = new LinkedList<NumberFormatRule>();

            // we keep track of the rule before the one we're currently working
            // on solely to support >>> substitutions
            NumberFormatRule? predecessor = null;

            // Iterate through the rules.  The rules
            // are separated by semicolons (there's no escape facility: ALL
            // semicolons are rule delimiters)
            var ruleTokens = description.AsTokens(';', PatternProps.WhiteSpace, TrimBehavior.Start);
            while (ruleTokens.MoveNext())
            {
                // makeRules (a factory method on NumberFormatRule) will return either
                // a single rule or an array of rules.  Either way, add them
                // to our rule vector
                NumberFormatRule.MakeRules(ruleTokens.Current.Text, this, predecessor, owner, tempRules);
                if (tempRules.Count != 0)
                {
                    predecessor = tempRules.Last!.Value;
                }
            }

            // for rules that didn't specify a base value, their base values
            // were initialized to 0.  Make another pass through the list and
            // set all those rules' base values.  We also remove any special
            // rules from the list and put them into their own member variables
            long defaultBaseValue = 0;

            foreach (NumberFormatRule rule in tempRules)
            {
                long baseValue = rule.BaseValue;
                if (baseValue == 0)
                {
                    // if the rule's base value is 0, fill in a default
                    // base value (this will be 1 plus the preceding
                    // rule's base value for regular rule sets, and the
                    // same as the preceding rule's base value in fraction
                    // rule sets)
                    rule.SetBaseValue(defaultBaseValue);
                }
                else
                {
                    // if it's a regular rule that already knows its base value,
                    // check to make sure the rules are in order, and update
                    // the default base value for the next rule
                    if (baseValue < defaultBaseValue)
                    {
                        throw new ArgumentException("Rules are not in order, base: " +
                                baseValue + " < " + defaultBaseValue);
                    }
                    defaultBaseValue = baseValue;
                }
                if (!isFractionRuleSet)
                {
                    ++defaultBaseValue;
                }
            }

            // finally, we can copy the rules from the vector into a
            // fixed-length array
            rules = new NumberFormatRule[tempRules.Count];
            tempRules.CopyTo(rules, 0);
        }

        /// <summary>
        /// Set one of the non-numerical rules.
        /// </summary>
        /// <param name="rule">The rule to set.</param>
        internal void SetNonNumericalRule(NumberFormatRule rule)
        {
            long baseValue = rule.BaseValue;
            if (baseValue == NumberFormatRule.NegativeNumberRule)
            {
                nonNumericalRules[NumberFormatRuleSet.NegativeRuleIndex] = rule;
            }
            else if (baseValue == NumberFormatRule.ImproperFractionRule)
            {
                SetFractionRule(rule); // Lookup with NumberFormatRuleSet.ImproperFractionRuleIndex
            }
            else if (baseValue == NumberFormatRule.ProperFractionRule)
            {
                SetFractionRule(rule); // Lookup with NumberFormatRuleSet.ProperFractionRuleIndex
            }
            else if (baseValue == NumberFormatRule.MasterRule)
            {
                SetFractionRule(rule); // Lookup with NumberFormatRuleSet.MasterRuleIndex
            }
            else if (baseValue == NumberFormatRule.InfinityRule)
            {
                nonNumericalRules[NumberFormatRuleSet.InfinityRuleIndex] = rule;
            }
            else if (baseValue == NumberFormatRule.NaNRule)
            {
                nonNumericalRules[NumberFormatRuleSet.NaNRuleIndex] = rule;
            }
        }

        /// <summary>
        /// Determine the best fraction rule to use. Rules matching the decimal point from
        /// <see cref="DecimalFormatSymbols"/> become the main set of rules to use.
        /// </summary>
        /// <param name="newRule">The new rule to consider.</param>
        private void SetFractionRule(NumberFormatRule newRule)
        {
            if (fractionRules is null)
            {
                fractionRules = new List<NumberFormatRule>();
            }
            fractionRules.Add(newRule);
        }

        /// <summary>
        /// Gets the best fraction rule for the supplied <paramref name="info"/> or <c>null</c>
        /// if no rule was found.
        /// <para/>
        /// This is purely a runtime method. It should always be used when parsing or formatting non-numerical rules.
        /// </summary>
        /// <param name="originalIndex">The identifier for the type of fraction rule, one of
        /// <see cref="NumberFormatRule.ImproperFractionRule"/>, <see cref="NumberFormatRule.ProperFractionRule"/> or
        /// <see cref="NumberFormatRule.MasterRule"/>.</param>
        /// <param name="info">The decimal format symbols for the current request.</param>
        /// <returns>The best rule that was registered during the construction of this class or <c>null</c> if no rule was found.</returns>
        public NumberFormatRule? GetBestFractionRule(int originalIndex, UNumberFormatInfo info)
        {
            if (fractionRules is not null)
            {
                NumberFormatRule? first = null;
                foreach (var rule in fractionRules)
                {
                    if (rule.BaseValue != originalIndex)
                        continue;

                    first ??= rule;
                    if (info.NumberDecimalSeparator == rule.DecimalPoint) // ICU4N NOTE: If we wanted to support formatting for percent, currency, etc, we would need to add a parameter to make a choice here...not sure if ICU4J does that.
                    {
                        return rule;
                    }
                }
                return first;
            }
            return null;
        }

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
        public override bool Equals(object? that)
        {
            // if different classes, they're not equal
            if (that is NumberFormatRuleSet that2)
            {
                // otherwise, compare the members one by one...
                if (!name.Equals(that2.name, StringComparison.Ordinal)
                        || rules!.Length != that2.rules!.Length // This is never null after construction
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
            foreach (NumberFormatRule rule in rules!)
            {
                result.Append(rule.ToString()).Append('\n');
            }

            // followed by the special rules (if they exist)
            foreach (NumberFormatRule? rule in nonNumericalRules)
            {
                if (rule is not null)
                {
                    if (fractionRules is not null && // ICU4N: There was a bug in ICU4J here - this collection may be null, so we need to use the default value in those cases
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
