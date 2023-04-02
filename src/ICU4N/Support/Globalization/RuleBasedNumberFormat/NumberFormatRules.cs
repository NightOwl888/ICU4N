using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Text;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    public sealed partial class NumberFormatRules : INumberFormatRules
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

        private const int RuleStringMaxStackBufferSize = 256;

        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The formatter's rule sets.
        /// </summary>
        //[NonSerialized]
        internal readonly NumberFormatRuleSet[] ruleSets; // Internal for testing

        /// <summary>
        /// The formatter's rule names mapped to rule sets.
        /// </summary>
        //[NonSerialized]
        internal readonly IDictionary<string, NumberFormatRuleSet> ruleSetsMap; // Internal for testing

        /// <summary>
        /// A pointer to the formatter's default rule set. This is always included
        /// in <see cref="ruleSets"/>.
        /// </summary>
        //[NonSerialized]
        internal readonly NumberFormatRuleSet defaultRuleSet; // ICU4N TODO: API - Change to CurrentRuleSet to match .NET? // Internal for testing

        /// <summary>
        /// If the description specifies lenient-parse rules, they're stored here until
        /// the collator is created.
        /// </summary>
        //[NonSerialized]
        internal readonly string? lenientParseRules; // Internal for testing

        /// <summary>
        /// If the description specifies post-process rules, they're stored here until
        /// post-processing is required.
        /// </summary>
        //[NonSerialized]
        internal readonly string? postProcessRules; // ICU4N TODO: Do we need to lazy load this? Or is it dependent on passed in culture? // Internal for testing

        /// <summary>
        /// The public rule set names;
        /// </summary>
        /// <serial/>
        internal readonly ReadOnlyCollection<string> publicRuleSetNames; // Internal for testing

        //-----------------------------------------------------------------------
        // cache
        //-----------------------------------------------------------------------

        private static readonly SoftCache<CacheKey, NumberFormatRules> rulesCache = new SoftCache<CacheKey, NumberFormatRules>();

        private struct CacheKey : IEquatable<CacheKey>
        {
            public string Name;
            public NumberPresentation NumberPresentation;

            public CacheKey(string baseName, NumberPresentation presentation)
            {
                Name = baseName ?? throw new ArgumentNullException(nameof(baseName));
                NumberPresentation = presentation;
            }

            public bool Equals(CacheKey other)
            {
                return Name.Equals(other.Name) && NumberPresentation == other.NumberPresentation;
            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is CacheKey other)
                    return Equals(other);

                return false;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ NumberPresentation.GetHashCode();
            }

            public override string ToString()
            {
                return string.Concat(Name.ToString(), ", ", NumberPresentation.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        /// <exception cref="MissingManifestResourceException">If no resource bundle for the specified
        /// <paramref name="cultureName"/> can be found.</exception>
        private static ICUResourceBundle GetBundle(string cultureName)
        {
            return (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuRuleBasedNumberFormatBaseName,
                cultureName, ICUResourceBundle.IcuDataAssembly, disableFallback: false);
        }

        private static string GetRulesForCulture(ICUResourceBundle? bundle, string cultureName, NumberPresentation format, out string[][]? localizations)
        {
            // Reuse the bundle if it doesn't exist.
            bundle ??= GetBundle(cultureName);

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
                // ICU4N: Intentionally blank
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

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        // ICU4N TODO: API Overloads of strings (cultureName) for each of these (requires a common way to validate/parse culture strings that we don't yet have)

        // ICU4N TODO: API Overload for CultureInfo

        // ICU4N TODO: API Overload for UCultureInfo

        // ICU4N TODO: API Overload for CultureInfo, NumberPresentation

        internal static NumberFormatRules GetInstance(UCultureInfo culture, NumberPresentation format) // ICU4N TODO: API - make public when we have format/parse methods that accept one of these?
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            return GetInstance(culture.Name, format);
        }

        internal static NumberFormatRules GetInstance(string cultureName, NumberPresentation format)
        {
            if (cultureName is null)
                throw new ArgumentNullException(nameof(cultureName));

            // Use the bundle to normalize cacheId based on what is available, so we only cache
            // the minimum number of instances. In ICU4J, the bundle was used to set actual/valid locale,
            // so this should be good enough for a cache key name.
            ICUResourceBundle? bundle = GetBundle(cultureName);
            string name = bundle?.UCulture.Name ?? string.Empty; // Go to invariant if there was no bundle.
            CacheKey cacheKey = new CacheKey(name, format);

            return rulesCache.GetOrCreate(cacheKey, (key) =>
            {
                string rules = GetRulesForCulture(bundle, key.Name, format, out string[][]? localizations);
                return new NumberFormatRules(rules, stripWhiteSpace: false); // ICU4N TODO: localizations
            });
        }

        private static bool IsDefaultCandidateRule(ReadOnlySpan<char> ruleText)
            => IsNamedRule(ruleText, SpelloutNumberingRuleName) ||
            IsNamedRule(ruleText, DigitsOrdinalRuleName) ||
            IsNamedRule(ruleText, DurationRuleName);

        private static bool IsSpecialRule(ReadOnlySpan<char> ruleText)
            => ruleText.StartsWith(LenientParseRuleName, StringComparison.Ordinal) || ruleText.StartsWith(PostProcessRuleName, StringComparison.Ordinal);

        private static bool IsSpecialRule(ReadOnlySpan<char> ruleText, string ruleName)
            => ruleText.StartsWith(ruleName, StringComparison.Ordinal);

        private static bool IsNamedRule(ReadOnlySpan<char> ruleText, string ruleName)
            => ruleText.Equals(ruleName, StringComparison.Ordinal);

        /// <summary>
        /// This extracts the special information from the rule sets before the
        /// main parsing starts. Extra whitespace must have already been removed
        /// from the description. If found, the special information is extracted from the
        /// description and returned. Note: the trailing semicolon at the end of the special
        /// rules is stripped. <see cref="IsSpecialRule(ReadOnlySpan{char}, string)"/> should be
        /// called prior to this method and <see cref="IsSpecialRule(ReadOnlySpan{char})"/> should be
        /// checked prior to all subsequent processing of the string to ensure the special rule is
        /// not processed further.</summary>
        /// <param name="ruleText">The rbnf description with extra whitespace removed.</param>
        /// <param name="ruleName">The name of the special rule text to extract (including the leading %%).</param>
        /// <returns>The special rule text.</returns>
        private static ReadOnlySpan<char> ExtractSpecialRule(ReadOnlySpan<char> ruleText, string ruleName)
            => ruleText.Slice(ruleName.Length).TrimStart(PatternProps.WhiteSpace).TrimEnd(';');


        internal NumberFormatRules(ReadOnlySpan<char> description)
            : this(description, stripWhiteSpace: true)
        {
        }

        private NumberFormatRules(ReadOnlySpan<char> description, bool stripWhiteSpace) // ICU4N TODO: Add a localizations parameter? We need to work out a way to allow users to supply these, but they don't matter for built-in rules. The jagged array is really ugly, but we should probably include an overload for compatibility reasons.
        {
            if (description.Length == 0)
                throw new ArgumentException("Empty rules description");

            if (stripWhiteSpace)
            {
                description = StripWhiteSpace(description);
            }

            // 1st pass: pre-flight parsing the description and count the number of
            // rule sets (";%" marks the end of one rule set and the beginning
            // of the next)
            int numRuleSets = 0; // ICU4N: Since we are counting the rule segments instead of the delimiters, we start at 0 instead of 1.
            SplitTokenizerEnumerator ruleTokens = description.AsTokens(";%", delimiterLength: 1, PatternProps.WhiteSpace, TrimBehavior.Start);
            while (ruleTokens.MoveNext())
            {
                ReadOnlySpan<char> ruleToken = ruleTokens.Current.Text;
                if (IsSpecialRule(ruleToken, LenientParseRuleName))
                {
                    lenientParseRules = new string(ExtractSpecialRule(ruleToken, LenientParseRuleName));
                    continue; // Don't count this rule
                }
                if (IsSpecialRule(ruleToken, PostProcessRuleName))
                {
                    postProcessRules = new string(ExtractSpecialRule(ruleToken, PostProcessRuleName));
                    continue; // Don't count this rule
                }

                ++numRuleSets;
            }

            // our rule list is an array of the appropriate size
            ruleSets = new NumberFormatRuleSet[numRuleSets];
            ruleSetsMap = new Dictionary<string, NumberFormatRuleSet>(numRuleSets); // ICU4N: Corrected allocation calculation

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

            ruleTokens = description.AsTokens(";%", delimiterLength: 1, PatternProps.WhiteSpace, TrimBehavior.Start);
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
                if (ruleSet.IsPublic)
                {
                    ++publicRuleSetCount;
                    if (defaultRuleSet is null && IsDefaultCandidateRule(currentName))
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

            if (defaultRuleSet is null)
            {
                for (int i = ruleSets.Length - 1; i >= 0; --i)
                {
                    if (ruleSets[i].IsPublic)
                    {
                        defaultRuleSet = ruleSets[i];
                        break;
                    }
                }
            }
            if (defaultRuleSet is null)
            {
                defaultRuleSet = ruleSets[ruleSets.Length - 1];
            }

            // 3rd pass: finally, we can go back through the descriptions
            // and finish setting up the substructure
            ruleTokens = description.AsTokens(";%", delimiterLength: 1, PatternProps.WhiteSpace, TrimBehavior.Start);
            for (int i = 0; i < ruleSets.Length && ruleTokens.MoveNext(); )
            {
                // Skip special rules
                ReadOnlySpan<char> ruleToken = ruleTokens.Current.Text;
                if (IsSpecialRule(ruleToken))
                    continue;

                ruleSets[i].ParseRules(ruleToken);
                i++;
            }

            // Now that the rules are initialized, the 'real' default rule
            // set can be adjusted by the localization data.

            // prepare an array of the proper size and copy the names into it
            string[] publicRuleSetTemp = new string[publicRuleSetCount];
            publicRuleSetCount = 0;
            for (int i = ruleSets.Length - 1; i >= 0; i--)
            {
                if (ruleSets[i].IsPublic)
                {
                    publicRuleSetTemp[publicRuleSetCount++] = ruleSets[i].Name;
                }
            }

            if (publicRuleSetNames != null) // ICU4N TODO: This block (which was in the RuleBasedNumberFormat.Init() method) will never run in the constructor.
            {
                // confirm the names, if any aren't in the rules, that's an error
                // it is ok if the rules contain public rule sets that are not in this list
                for (int i = 0; i < publicRuleSetNames.Count; ++i)
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
                publicRuleSetNames = new ReadOnlyCollection<string>(publicRuleSetTemp);
            }
        }

        /// <summary>
        /// This method is used by the constructor to strip whitespace between rules (i.e.,
        /// after semicolons).
        /// </summary>
        /// <param name="description">The formatter description.</param>
        /// <returns>The description with all the whitespace that follows semicolons
        /// taken out.</returns>
        private ReadOnlySpan<char> StripWhiteSpace(ReadOnlySpan<char> description)
        {
            int descriptionLength = description.Length;

            // since we don't have a method that deletes characters
            // create a new StringBuffer to copy the text into
            ValueStringBuilder result = descriptionLength <= RuleStringMaxStackBufferSize
                ? new ValueStringBuilder(stackalloc char[RuleStringMaxStackBufferSize])
                : new ValueStringBuilder(descriptionLength);

            // iterate through the characters...
            int start = 0;
            while (start < descriptionLength)
            {
                // seek to the first non-whitespace character...
                while (start < descriptionLength
                       && PatternProps.IsWhiteSpace(description[start]))
                {
                    ++start;
                }

                //if the first non-whitespace character is semicolon, skip it and continue
                if (start < descriptionLength && description[start] == ';')
                {
                    start += 1;
                    continue;
                }

                // locate the next semicolon in the text and copy the text from
                // our current position up to that semicolon into the result
                int p = description.Slice(start).IndexOf(';') + start;
                if (p == -1)
                {
                    // or if we don't find a semicolon, just copy the rest of
                    // the string into the result
                    result.Append(description.Slice(start));
                    break;
                }
                else if (p < descriptionLength)
                {
                    result.Append(description.Slice(start, (p + 1) - start)); // ICU4N: Corrected 2nd parameter
                    start = p + 1;
                }
                else
                {
                    // when we get here, we've seeked off the end of the string, and
                    // we terminate the loop (we continue until *start* is -1 rather
                    // than until *p* is -1, because otherwise we'd miss the last
                    // rule in the description)
                    break;
                }
            }
            // ICU4N: We must heap allocate here because the ValueStringBuilder may return from the
            // stack, which is out of scope after this point.
            return result.ToString();
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        // ICU4N specific: Being that this class is immutable, it doesn't make a lot of sense for a Clone() method.
        // We can just use this instance on any thread.

        /// <summary>
        /// Tests two <see cref="NumberFormatRules"/> instances for equality.
        /// </summary>
        /// <param name="obj">The object to compare with this one.</param>
        /// <returns><c>true</c> if the two objects contain the same behavior; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (this == obj) return true;
            if (GetType() != obj.GetType()) return false;
            NumberFormatRules other = (NumberFormatRules)obj;

            // compare their lenient-parse and pose process rules
            if (lenientParseRules != other.lenientParseRules || postProcessRules != other.postProcessRules)
                return false;

            // if that succeeds, then compare their rule set lists
            if (ruleSets.Length != other.ruleSets.Length)
            {
                return false;
            }
            for (int i = 0; i < ruleSets.Length; i++)
            {
                if (!ruleSets[i].Equals(other.ruleSets[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1;
            hashCode = 31 * hashCode + (lenientParseRules?.GetHashCode() ?? 0);
            hashCode = 31 * hashCode + (postProcessRules?.GetHashCode() ?? 0);
            hashCode = 31 * hashCode + ruleSets.Length.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Generates a textual description of these rules.
        /// </summary>
        /// <returns>A <see cref="string"/> containing a rule set that will produce a <see cref="NumberFormatRules"/>
        /// with identical behavior to this one. This won't necessarily be identical
        /// to the rule set description that it was originally built with, but will produce
        /// the same result.</returns>
        /// <stable>ICU 2.0</stable>
        public override string ToString()
        {
            // accumulate the descriptions of all the rule sets in a
            // StringBuffer, then cast it to a String and return it
            StringBuilder result = new StringBuilder(ruleSets.Length * 20);
            foreach (NumberFormatRuleSet ruleSet in ruleSets)
            {
                result.Append(ruleSet.ToString());
            }
            return result.ToString();
        }

        //-----------------------------------------------------------------------
        // public API functions
        //-----------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name of the default rule set. If the default rule set is not public, returns <see cref="string.Empty"/>.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        // ICU4N specific - removed setter because this class is immutable and the rule set name can be specified on the Format/Parse APIs
        public string DefaultRuleSetName
        {
            get
            {
                if (defaultRuleSet != null && defaultRuleSet.IsPublic)
                {
                    return defaultRuleSet.Name;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a collection of the names of all of the public rule sets.
        /// </summary>
        /// <draft>ICU 60.1</draft>
#if FEATURE_IREADONLYCOLLECTIONS
        public IReadOnlyList<string> RuleSetNames
#else
        public IList<string> RuleSetNames
#endif
            => publicRuleSetNames;

        //-----------------------------------------------------------------------
        // INumberFormatRules members
        //-----------------------------------------------------------------------

        internal NumberFormatRuleSet DefaultRuleSet => defaultRuleSet;

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
        /// <param name="throwIfNotFound"><c>true</c> to throw if not found, otherwise <c>false</c> to return <c>null</c> instead.</param>
        /// <returns>The rule set with that name.</returns>
        /// <exception cref="ArgumentException">No rule exists with the provided <paramref name="name"/>.</exception>
        internal NumberFormatRuleSet FindRuleSet(string name, bool throwIfNotFound = true)
        {
            if ((!ruleSetsMap!.TryGetValue(name, out NumberFormatRuleSet ? result) || result is null) && throwIfNotFound)
            {
                throw new ArgumentException("No rule set named " + name);
            }
            return result!; // Assume we know what we are doing if we pass throwIfNotFound
        }

        NumberFormatRuleSet INumberFormatRules.FindRuleSet(string name) => FindRuleSet(name);
    }
#endif
}
