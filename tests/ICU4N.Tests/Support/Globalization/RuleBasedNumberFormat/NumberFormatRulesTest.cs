using ICU4N.Dev.Test;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;

namespace ICU4N.Globalization
{
    internal class NumberFormatRulesTest : TestFmwk
    {
#if FEATURE_SPAN

        //[Test]
        ////[Ignore("This is just to try to work out what to cache")]
        //public void TestRuleSizes_AllCultures()
        //{
        //    Dictionary<string, Tuple<string, string, string, string>> sizes = new Dictionary<string, Tuple<string, string, string, string>>();
        //    var sw = new System.Diagnostics.Stopwatch();
        //    TimeSpan rulesResourceLookupTime;
        //    TimeSpan rulesParseTime;

        //    string[] names = {
        //        " (spellout) ",
        //        " (ordinal) ",
        //        " (duration) ", // English only
        //        " (numbering system)",
        //    };

        //    foreach (var culture in UCultureInfo.GetCultures(UCultureTypes.AllCultures))
        //    {
        //        foreach (NumberPresentation numberPresentation in Enum.GetValues<NumberPresentation>())
        //        {
        //            RuleBasedNumberFormat expected = new RuleBasedNumberFormat(culture, numberPresentation);

        //            sw.Restart();
        //            string rules = GetRulesForCulture(culture, numberPresentation, out string[][] localizations);
        //            sw.Stop();
        //            rulesResourceLookupTime = sw.Elapsed;

        //            sw.Restart();
        //            NumberFormatRules actual = new NumberFormatRules(rules);
        //            sw.Stop();
        //            rulesParseTime = sw.Elapsed;

        //            if (!sizes.ContainsKey(culture.ToString()))
        //                sizes.Add(culture.ToString(), new Tuple<string, string, string, string>(Lucene.Net.Util.RamUsageEstimator.HumanSizeOf(rules), rulesResourceLookupTime.ToString(), Lucene.Net.Util.RamUsageEstimator.HumanSizeOf(actual), rulesParseTime.ToString()));

        //            //AssertEquivalentRulesEngines(culture, expected, actual);
        //        }
        //    }
        //}

        /// <summary>
        /// Spot check for invariant culture
        /// </summary>
        [Test]
        public void TestRuleParsing_Invariant()
        {
            var culture = UCultureInfo.InvariantCulture;
            var numberPresentation = NumberPresentation.NumberingSystem;

            RuleBasedNumberFormat expected = new RuleBasedNumberFormat(culture, numberPresentation);

            string rules = GetRulesForCulture(culture, numberPresentation, out string[][] localizations);

            var actual = new NumberFormatRules(rules.AsSpan());

            AssertEquivalentRulesEngines(culture, expected, actual);
        }

        [Test]
        [Ignore("Slow - Run manually to debug")]
        public void TestRuleParsing_AllCultures()
        {
            string[] names = {
                " (spellout) ",
                " (ordinal) ",
                " (duration) ", // English only
                " (numbering system)",
            };

            foreach (var culture in UCultureInfo.GetCultures(UCultureTypes.AllCultures))
            {
                foreach (NumberPresentation numberPresentation in Enum.GetValues(typeof(NumberPresentation)))
                {
                    RuleBasedNumberFormat expected = new RuleBasedNumberFormat(culture, numberPresentation);

                    string rules = GetRulesForCulture(culture, numberPresentation, out string[][] localizations);
                    NumberFormatRules actual = new NumberFormatRules(rules.AsSpan());

                    AssertEquivalentRulesEngines(culture, expected, actual);
                }
            }
        }

        private void AssertEquivalentRulesEngines(UCultureInfo culture, RuleBasedNumberFormat expected, NumberFormatRules actual)
        {
            assertEquals($"{culture}: '{nameof(actual.ruleSets)}' had a length mismatch", expected.RuleSets.Length, actual.ruleSets.Length);
            for (int i = 0; i < actual.ruleSets.Length; i++)
            {
                AssertEquivalentRuleSets(culture, expected.RuleSets[i], actual.ruleSets[i]);
            }

            assertEquals($"{culture}: '{nameof(actual.ruleSetsMap)}' had a count mismatch", expected.RuleSetsMap.Count, actual.ruleSetsMap.Count);
            foreach (var kvp in expected.RuleSetsMap)
            {
                assertTrue($"{culture}: '{nameof(actual.ruleSetsMap)}' is missing key {kvp.Key}", actual.ruleSetsMap.TryGetValue(kvp.Key, out NumberFormatRuleSet value));
                assertEquals($"{culture}: '{nameof(actual.ruleSetsMap)}' had a value '{nameof(actual.defaultRuleSet)}' with a name mismatch", kvp.Value?.Name, value?.Name);
            }

            assertEquals($"{culture}: '{nameof(actual.defaultRuleSet)}' had a name mismatch", expected.DefaultRuleSet?.Name, actual.defaultRuleSet?.Name);
            assertEquals($"{culture}: '{nameof(actual.lenientParseRules)}' had a mismatch", expected.LenientParseRules, actual.lenientParseRules);
            assertEquals($"{culture}: '{nameof(actual.postProcessRules)}' had a mismatch", expected.PostProcessRules, actual.postProcessRules);
            assertEquals($"{culture}: '{nameof(actual.publicRuleSetNames)}' had a mismatch", expected.PublicRuleSetNames, actual.publicRuleSetNames);

        }

        private void AssertEquivalentRuleSets(UCultureInfo culture, NFRuleSet expected, NumberFormatRuleSet actual)
        {
            assertEquals($"{culture}: Rule set {actual} had a null mismatch in rule set '{actual?.Name}'", expected is null, actual is null);
            if (expected is null || actual is null) return;

            assertEquals($"{culture}: '{nameof(actual.Name)}' had a mismatch", expected.Name, actual.Name);

            assertEquals($"{culture}: Rule set {actual.rules} had a null mismatch in rule set '{actual?.Name}'", expected.Rules is null, actual.rules is null);
            if (expected.Rules is not null && actual.rules is not null)
            {
                assertEquals($"{culture}: '{nameof(actual.rules)}' had a mismatch in rule set '{actual?.Name}'", expected.Rules.Length, actual.rules.Length);
                for (int i = 0; i < actual.rules.Length; i++)
                {
                    AssertEquivalentRules(culture, expected.Rules[i], actual.rules[i], actual.Name);
                }
            }

            AssertEquivalentRules(culture,
                expected.nonNumericalRules[NFRuleSet.NegativeRuleIndex],
                actual.nonNumericalRules[NumberFormatRuleSet.NegativeRuleIndex],
                expected.Name, $" for {nameof(NumberFormatRuleSet.NegativeRuleIndex)}");

            AssertEquivalentRules(culture,
                expected.nonNumericalRules[NFRuleSet.InfinityRuleIndex],
                actual.nonNumericalRules[NumberFormatRuleSet.InfinityRuleIndex],
                expected.Name, $" for {nameof(NumberFormatRuleSet.InfinityRuleIndex)}");

            AssertEquivalentRules(culture,
                expected.nonNumericalRules[NFRuleSet.NaNRuleIndex],
                actual.nonNumericalRules[NumberFormatRuleSet.NaNRuleIndex],
                expected.Name, $" for {nameof(NumberFormatRuleSet.NaNRuleIndex)}");

            assertEquals($"{culture}: Rule set {actual.fractionRules} had a null mismatch in rule set '{actual.Name}'", expected.FractionRules is null, actual.fractionRules is null);
            if (expected.FractionRules is not null && actual.fractionRules is not null)
            {
                assertEquals($"{culture}: Rule set {actual.fractionRules} had a count mismatch in rule set '{actual.Name}'", expected.FractionRules.Count, actual.fractionRules.Count);
                for (int i = 0; i < actual.fractionRules.Count; i++)
                {
                    AssertEquivalentRules(culture, expected.FractionRules[i], actual.fractionRules[i], actual.Name, $" in fractionRules[{i}]");
                }
            }

            assertEquals($"{culture}: Rule set {actual.IsFractionSet} had a mismatch in rule set '{actual.Name}'", expected.IsFractionSet, actual.IsFractionSet);
            assertEquals($"{culture}: Rule set {actual.IsParseable} had a mismatch in rule set '{actual.Name}'", expected.IsParseable, actual.IsParseable);
        }

        private void AssertEquivalentRules(UCultureInfo culture, NFRule expected, NumberFormatRule actual, string ruleSetName, string context = "")
        {
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a null mismatch in rule with ruleText '{actual?.ruleText}'{context}", expected is null, actual is null);
            if (expected is null || actual is null) return;

            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.BaseValue} mismatch in rule with ruleText '{actual.ruleText}'{context}", expected.BaseValue, actual.BaseValue);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.radix} mismatch in rule with ruleText '{actual.ruleText}'{context}", expected.Radix, actual.radix);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.exponent} mismatch in rule with ruleText '{actual.ruleText}'{context}", expected.Exponent, actual.exponent);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.DecimalPoint} mismatch in rule with ruleText '{actual.ruleText}'{context}",
                char.ToString(expected.DecimalPoint),
                string.IsNullOrEmpty(actual.DecimalPoint) ? "\0" : actual.DecimalPoint);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.ruleText} mismatch'{context}", expected.RuleText, actual.ruleText);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.pluralRulesText} mismatch in rule with ruleText '{actual.ruleText}'{context}", expected.RulePatternFormat?.ToPattern(), actual.pluralRulesText);

            AssertEquivalentSubstitutions(culture, expected.Sub1, actual.sub1, ruleSetName, actual.ruleText, context + " in sub1");
            AssertEquivalentSubstitutions(culture, expected.Sub2, actual.sub2, ruleSetName, actual.ruleText, context + " in sub2");
        }

        private void AssertEquivalentSubstitutions(UCultureInfo culture, NFSubstitution expected, NumberFormatSubstitution actual, string ruleSetName, string ruleText, string context = "")
        {
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a null mismatch in rule with ruleText '{ruleText}'{context}", expected is null, actual is null);
            if (expected is null || actual is null) return;
            
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.Pos} mismatch in rule with ruleText '{ruleText}'{context}", expected.Pos, actual.Pos);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.ruleSet} mismatch in rule with ruleText '{ruleText}'{context}", expected.RuleSet?.Name, actual.ruleSet?.Name);
            assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actual.numberFormatPattern} mismatch in rule with ruleText '{ruleText}'{context}", expected.NumberFormat?.ToPattern(), actual.numberFormatPattern);

            if (expected is ICU4N.Text.FractionalPartSubstitution expectedFPS)
            {
                if (actual is ICU4N.Globalization.FractionalPartSubstitution actualFPS)
                {
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualFPS.byDigits} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.FractionalPartSubstitution)}",
                        expectedFPS.ByDigits, actualFPS.byDigits);
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualFPS.useSpaces} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.FractionalPartSubstitution)}",
                        expectedFPS.UseSpaces, actualFPS.useSpaces);
                }
                else
                    fail($"actual was not a {nameof(ICU4N.Globalization.FractionalPartSubstitution)}");
            }

            if (expected is ICU4N.Text.ModulusSubstitution expectedModulusSub)
            {
                if (actual is ICU4N.Globalization.ModulusSubstitution actualModulusSub)
                {
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualModulusSub.divisor} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.ModulusSubstitution)}",
                        expectedModulusSub.Divisor, actualModulusSub.divisor);
                    AssertEquivalentRules(culture, expectedModulusSub.RuleToUse, actualModulusSub.ruleToUse, ruleSetName, context + $" inside {nameof(ICU4N.Globalization.ModulusSubstitution)}");
                }
                else
                    fail($"actual was not a {nameof(ICU4N.Globalization.ModulusSubstitution)}");
            }

            if (expected is ICU4N.Text.MultiplierSubstitution expectedMultiplierSub)
            {
                if (actual is ICU4N.Globalization.MultiplierSubstitution actualMultiplierSub)
                {
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualMultiplierSub.divisor} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.MultiplierSubstitution)}",
                        expectedMultiplierSub.Divisor, actualMultiplierSub.divisor);
                }
                else
                    fail($"actual was not a {nameof(ICU4N.Globalization.MultiplierSubstitution)}");
            }

            if (expected is ICU4N.Text.NumeratorSubstitution expectedNumeratorSub)
            {
                if (actual is ICU4N.Globalization.NumeratorSubstitution actualNumeratorSub)
                {
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualNumeratorSub.denominator} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.NumeratorSubstitution)}",
                        expectedNumeratorSub.Denominator, actualNumeratorSub.denominator);
                    assertEquals($"{culture}: Rule set '{ruleSetName}' had a {actualNumeratorSub.withZeros} mismatch in rule with ruleText '{ruleText}'{context} as a {nameof(ICU4N.Globalization.NumeratorSubstitution)}",
                        expectedNumeratorSub.WithZeros, actualNumeratorSub.withZeros);
                }
                else
                    fail($"actual was not a {nameof(ICU4N.Globalization.NumeratorSubstitution)}");
            }
        }

#endif



        // Pulled from constructor of RuleBasedNumberFormat.
        // ICU4N TODO: Move this to CultureData and use the solution from there for this test.
        private static string GetRulesForCulture(UCultureInfo locale, NumberPresentation format, out string[][] localizations)
        {
            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.
                            GetBundleInstance(ICUData.IcuRuleBasedNumberFormatBaseName, locale);

            //// TODO: determine correct actual/valid locale.  Note ambiguity
            //// here -- do actual/valid refer to pattern, DecimalFormatSymbols,
            //// or Collator?
            //UCultureInfo uloc = bundle.UCulture;
            //SetCulture(uloc, uloc);

            StringBuilder description = new StringBuilder();
            localizations = null;

            try
            {
                ICUResourceBundle rules = bundle.GetWithFallback(format.ToRuleNameKey());
                UResourceBundleEnumerator it = rules.GetEnumerator();
                while (it.MoveNext())
                {
                    description.Append(it.Current.GetString());
                }
            }
            catch (MissingManifestResourceException)
            {
            }

            // We use findTopLevel() instead of get() because
            // it's faster when we know that it's usually going to fail.
            UResourceBundle locNamesBundle = bundle.FindTopLevel(format.ToRuleLocalizationsKey());
            if (locNamesBundle != null)
            {
                localizations = new string[locNamesBundle.Length][];
                for (int i = 0; i < localizations.Length; ++i)
                {
                    localizations[i] = locNamesBundle.Get(i).GetStringArray();
                }
            }
            // else there are no localized names. It's not that important.

            return description.ToString();
        }
    }
}
