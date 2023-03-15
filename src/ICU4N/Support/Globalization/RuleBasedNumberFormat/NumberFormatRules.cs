using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.Collections.Generic;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    internal sealed class NumberFormatRules : INumberFormatRules
    {
        //-----------------------------------------------------------------------
        // constants
        //-----------------------------------------------------------------------

        // Special rules
        private const string LenientParseRuleName = "%%lenient-parse:";
        private const string PostProcessRuleName = "%%post-process:";

        // Potential default rules
        private const string SpelloutNumberingRuleName = "%spellout-numbering";
        private const string DigitsOrdinalRuleName = "%digits-ordinal";
        private const string DurationRuleName = "%duration";


        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The formatter's rule sets.
        /// </summary>
        //[NonSerialized]
        private readonly NumberFormatRuleSet[] ruleSets;

        /// <summary>
        /// The formatter's rule names mapped to rule sets.
        /// </summary>
        //[NonSerialized]
        private readonly IDictionary<string, NumberFormatRuleSet> ruleSetsMap;

        /// <summary>
        /// A pointer to the formatter's default rule set. This is always included
        /// in <see cref="ruleSets"/>.
        /// </summary>
        //[NonSerialized]
        private readonly NumberFormatRuleSet defaultRuleSet; // ICU4N TODO: API - Change to CurrentRuleSet to match .NET?

        /// <summary>
        /// If the description specifies lenient-parse rules, they're stored here until
        /// the collator is created.
        /// </summary>
        //[NonSerialized]
        private readonly string? lenientParseRules;

        /// <summary>
        /// If the description specifies post-process rules, they're stored here until
        /// post-processing is required.
        /// </summary>
        //[NonSerialized]
        private readonly string? postProcessRules; // ICU4N TODO: Do we need to lazy load this? Or is it dependent on passed in culture?

        /// <summary>
        /// The public rule set names;
        /// </summary>
        /// <serial/>
        private readonly string[] publicRuleSetNames;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        public static bool IsDefaultCandidateRule(ReadOnlySpan<char> ruleText)
            => IsNamedRule(ruleText, SpelloutNumberingRuleName) ||
            IsNamedRule(ruleText, DigitsOrdinalRuleName) ||
            IsNamedRule(ruleText, DurationRuleName);

        private static bool IsSpecialRule(ReadOnlySpan<char> ruleText)
            => IsNamedRule(ruleText, LenientParseRuleName) || IsNamedRule(ruleText, PostProcessRuleName);

        private static bool IsPrivateRule(ReadOnlySpan<char> ruleText)
            => ruleText.StartsWith("%%", StringComparison.Ordinal);

        private static bool IsNamedRule(ReadOnlySpan<char> ruleText, string ruleName)
            => ruleText.StartsWith(ruleName, StringComparison.Ordinal);

        private static ReadOnlySpan<char> ExtractSpecialRule(ReadOnlySpan<char> ruleText, string ruleName)
            => ruleText.Slice(ruleName.Length, ruleText.Length - ruleName.Length);

        public NumberFormatRules(ReadOnlySpan<char> description) // ICU4N TODO: Add a localizations parameter? We need to work out a way to allow users to supply these, but they don't matter for built-in rules. The jagged array is really ugly, but we should probably include an overload for compatibility reasons.
        {
            if (description.Length == 0)
                throw new ArgumentException("Empty rules description");

            // 1st pass: pre-flight parsing the description and count the number of
            // rule sets (";%" marks the end of one rule set and the beginning
            // of the next)
            int numRuleSets = 1;
            SplitTokenizerEnumerator ruleTokens = description.AsTokens(";%", PatternProps.WhiteSpace);
            while (ruleTokens.MoveNext())
            {
                ReadOnlySpan<char> ruleToken = ruleTokens.Current.Text;
                if (IsNamedRule(ruleToken, LenientParseRuleName))
                {
                    lenientParseRules = new string(ExtractSpecialRule(ruleToken, LenientParseRuleName));
                    continue; // Don't count this rule
                }
                if (IsNamedRule(ruleToken, PostProcessRuleName))
                {
                    lenientParseRules = new string(ExtractSpecialRule(ruleToken, PostProcessRuleName));
                    continue; // Don't count this rule
                }

                ++numRuleSets;
            }

            // our rule list is an array of the appropriate size
            ruleSets = new NumberFormatRuleSet[numRuleSets];
            ruleSetsMap = new Dictionary<string, NumberFormatRuleSet>(numRuleSets * 2 + 1);

            // Used to count the number of public rule sets
            // Public rule sets have names that begin with % instead of %%.
            int publicRuleSetCount = 0;

            // 2nd pass: divide up the descriptions into individual rule-set descriptions
            // and instantiate the NumberFormatRuleSet instances.
            // We can't actually parse
            // the rest of the descriptions and finish initializing everything
            // because we have to know the names and locations of all the rule
            // sets before we can actually set everything up
            int curRuleSet = 0;

            ruleTokens = description.AsTokens(";%", PatternProps.WhiteSpace);
            while (ruleTokens.MoveNext())
            {
                // Skip special rules
                ReadOnlySpan<char> ruleToken = ruleTokens.Current.Text;
                if (IsSpecialRule(ruleToken))
                    continue;

                var ruleSet = new NumberFormatRuleSet(this, ruleToken);
                ruleSets[curRuleSet] = ruleSet;
                string currentName = ruleSet.Name;
                ruleSetsMap[currentName] = ruleSet;
                if (!IsPrivateRule(currentName))
                {
                    ++publicRuleSetCount;
                    if (defaultRuleSet is null && IsDefaultCandidateRule(ruleToken))
                    {
                        defaultRuleSet = ruleSet;
                    }
                }
                ++curRuleSet;
            }

            // now we can take note of the formatter's default rule set, which
            // is the last public rule set in the description (it's the last
            // rather than the first so that a user can create a new formatter
            // from an existing formatter and change its default behavior just
            // by appending more rule sets to the end)

            // {dlf} Initialization of a fraction rule set requires the default rule
            // set to be known.  For purposes of initialization, this is always the
            // last public rule set, no matter what the localization data says.

            // Set the default ruleset to the last public ruleset, unless one of the predefined
            // ruleset names %spellout-numbering, %digits-ordinal, or %duration is found

            if (defaultRuleSet == null)
            {
                for (int i = ruleSets.Length - 1; i >= 0; --i)
                {
                    if (!IsPrivateRule(ruleSets[i].Name))
                    {
                        defaultRuleSet = ruleSets[i];
                        break;
                    }
                }
            }
            if (defaultRuleSet == null)
            {
                defaultRuleSet = ruleSets[ruleSets.Length - 1];
            }

            // 3rd pass: finally, we can go back through the temporary descriptions
            // list and finish setting up the substructure
            ruleTokens = description.AsTokens(";%", PatternProps.WhiteSpace);
            for (int i = 0; i < ruleSets.Length && ruleTokens.MoveNext(); i++)
            {
                // Skip special rules
                ReadOnlySpan<char> ruleToken = ruleTokens.Current.Text;
                if (IsSpecialRule(ruleToken))
                    continue;

                ruleSets[i].ParseRules(ruleToken);
            }

            // Now that the rules are initialized, the 'real' default rule
            // set can be adjusted by the localization data.

            // prepare an array of the proper size and copy the names into it
            string[] publicRuleSetTemp = new string[publicRuleSetCount];
            publicRuleSetCount = 0;
            for (int i = ruleSets.Length - 1; i >= 0; i--)
            {
                if (!IsPrivateRule(ruleSets[i].Name))
                {
                    publicRuleSetTemp[publicRuleSetCount++] = ruleSets[i].Name;
                }
            }

            if (publicRuleSetNames != null) // ICU4N TODO: This block (which was in the RuleBasedNumberFormat.Init() method) will never run in the constructor.
            {
                // confirm the names, if any aren't in the rules, that's an error
                // it is ok if the rules contain public rule sets that are not in this list
                for (int i = 0; i < publicRuleSetNames.Length; ++i)
                {
                    string name = publicRuleSetNames[i];
                    bool found = false;
                    for (int j = 0; j < publicRuleSetTemp.Length; ++j)
                    {
                        if (name.Equals(publicRuleSetTemp[j], StringComparison.Ordinal))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) throw new ArgumentException("did not find public rule set: " + name);
                }

                defaultRuleSet = FindRuleSet(publicRuleSetNames[0]); // might be different
            }
            else
            {
                publicRuleSetNames = publicRuleSetTemp;
            }
        }




        // ICU4N TODO: Implementation

        //-----------------------------------------------------------------------
        // INumberFormatRules members
        //-----------------------------------------------------------------------

        /// <summary>
        /// Gets a reference to the formatter's default rule set. The default
        /// rule set is the last public rule set in the description, or the one
        /// most recently set.
        /// </summary>
        NumberFormatRuleSet INumberFormatRules.DefaultRuleSet => defaultRuleSet; // ICU4N TODO: API - This should be passed into the Format method and if not passed, it should use the rule parsed from NumberingSystem.Description (See internal static NumberFormat CreateInstance(UCultureInfo desiredLocale, NumberFormatStyle choice))

        /// <summary>
        /// Returns the named rule set. Throws an <see cref="ArgumentException"/>
        /// if this formatter doesn't have a rule set with that name.
        /// </summary>
        /// <param name="name">The name of the desired rule set.</param>
        /// <returns>The rule set with that name.</returns>
        /// <exception cref="ArgumentException">No rule exists with the provided <paramref name="name"/>.</exception>
        private NumberFormatRuleSet FindRuleSet(string name)
        {
            if (!ruleSetsMap!.TryGetValue(name, out NumberFormatRuleSet? result) || result is null)
            {
                throw new ArgumentException("No rule set named " + name);
            }
            return result;
        }

        NumberFormatRuleSet INumberFormatRules.FindRuleSet(string name) => FindRuleSet(name);
    }
#endif
}
