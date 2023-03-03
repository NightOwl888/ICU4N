using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Collections.Generic.Extensions;
using J2N.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// Loader for plural rules data.
    /// </summary>
    internal class PluralRulesLoader // ICU4N: Marked internal until implementation is completed
#pragma warning disable 612, 618
        : PluralRulesFactory
#pragma warning restore 612, 618
    {
        private readonly ConcurrentDictionary<string, PluralRules> rulesIdToRules = new ConcurrentDictionary<string, PluralRules>();
        // lazy init, use getLocaleIdToRulesIdMap to access
        private IDictionary<string, string> localeIdToCardinalRulesId;
        private IDictionary<string, string> localeIdToOrdinalRulesId;
        private IDictionary<string, UCultureInfo> rulesIdToEquivalentULocale;
#pragma warning disable 612, 618
        private static readonly IDictionary<string, PluralRanges> localeIdToPluralRanges = LoadLocaleIdToPluralRanges();
#pragma warning restore 612, 618

        private object syncLock = new object();
        private bool localeRulesInitialized;

        /// <summary>
        /// Access through singleton.
        /// </summary>
#pragma warning disable 612, 618
        private PluralRulesLoader()
        {
        }
#pragma warning restore 612, 618

        /// <summary>
        /// Returns the locales for which we have plurals data. Utility for testing.
        /// </summary>
#pragma warning disable 672
        public override UCultureInfo[] GetUCultures() // ICU4N: Renamed from GetAvailableLocales // ICU4N TODO: API Add UCultureTypes enum ?
#pragma warning restore 672
        {
            ICollection<string> keys = GetLocaleIdToRulesIdMap(PluralType.Cardinal).Keys;
            UCultureInfo[] locales = new UCultureInfo[keys.Count];
            int n = 0;
            foreach (var key in keys)
            {
                locales[n++] = UCultureInfo.CreateCanonical(key);
            }
            return locales;
        }

        /// <summary>
        /// Returns the functionally equivalent locale.
        /// </summary>
#pragma warning disable CS0672 // Type or member is obsolete
        public override UCultureInfo GetFunctionalEquivalent(UCultureInfo locale, out bool isAvailable)
#pragma warning restore CS0672 // Type or member is obsolete
        {
            string localeId = UCultureInfo.Canonicalize(locale.Name);
            IDictionary<string, string> idMap = GetLocaleIdToRulesIdMap(PluralType.Cardinal);
            isAvailable = idMap.ContainsKey(localeId);
#pragma warning disable CS0618 // Type or member is obsolete
            return GetFunctionalEquivalent(locale);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the functionally equivalent locale.
        /// </summary>
#pragma warning disable CS0672 // Type or member is obsolete
        public override UCultureInfo GetFunctionalEquivalent(UCultureInfo locale) // ICU4N specific: Added overload so we don't need to pass a null parameter
#pragma warning restore CS0672 // Type or member is obsolete
        {
            string rulesId = GetRulesIdForLocale(locale, PluralType.Cardinal);
            if (rulesId is null || rulesId.Trim().Length == 0)
            {
                return UCultureInfo.InvariantCulture; // ultimate fallback
            }

            if (!RulesIdToEquivalentULocaleMap.TryGetValue(rulesId, out UCultureInfo result) || result is null)
            {
                return UCultureInfo.InvariantCulture; // ultimate fallback
            }

            return result;
        }

        /// <summary>
        /// Returns the lazily-constructed map.
        /// </summary>
        private IDictionary<string, string> GetLocaleIdToRulesIdMap(PluralType type)
        {
            CheckBuildRulesIdMaps();
            return (type == PluralType.Cardinal) ? localeIdToCardinalRulesId : localeIdToOrdinalRulesId;
        }

        /// <summary>
        /// Returns the lazily-constructed map.
        /// </summary>
        private IDictionary<string, UCultureInfo> RulesIdToEquivalentULocaleMap
        {
            get
            {
                CheckBuildRulesIdMaps();
                return rulesIdToEquivalentULocale;
            }
        }

        /// <summary>
        /// Lazily constructs the localeIdToRulesId and rulesIdToEquivalentULocale
        /// maps if necessary. These exactly reflect the contents of the locales
        /// resource in plurals.res.
        /// </summary>
        private void CheckBuildRulesIdMaps()
        {
            LazyInitializer.EnsureInitialized(ref rulesIdToEquivalentULocale, ref localeRulesInitialized, ref syncLock, LoadRulesIdMaps);
        }

        /// <summary>
        /// constructs the localeIdToRulesId and rulesIdToEquivalentULocale
        /// maps. These exactly reflect the contents of the locales
        /// resource in plurals.res.
        /// </summary>
        /// <returns>The rulesIdToEquivalentULocale map.</returns>
        private IDictionary<string, UCultureInfo> LoadRulesIdMaps()
        {
            // ICU4N: Simplified this using LazyInitializer and an external lock.
            try
            {
                UResourceBundle pluralb = GetPluralBundle();
                // Read cardinal-number rules.
                UResourceBundle localeb = pluralb.Get("locales");

                // sort for convenience of getAvailableULocales
                localeIdToCardinalRulesId = new SortedDictionary<string, string>(StringComparer.Ordinal);
                // not visible
                rulesIdToEquivalentULocale = new Dictionary<string, UCultureInfo>();

                for (int i = 0; i < localeb.Length; ++i)
                {
                    UResourceBundle b = localeb.Get(i);
                    string id = b.Key;
                    string value = b.GetString().Intern();
                    localeIdToCardinalRulesId[id] = value;

                    if (!rulesIdToEquivalentULocale.ContainsKey(value))
                    {
                        rulesIdToEquivalentULocale[value] = new UCultureInfo(id);
                    }
                }

                // Read ordinal-number rules.
                localeb = pluralb.Get("locales_ordinals");
                localeIdToOrdinalRulesId = new SortedDictionary<string, string>(StringComparer.Ordinal);
                for (int i = 0; i < localeb.Length; ++i)
                {
                    UResourceBundle b = localeb.Get(i);
                    string id = b.Key;
                    string value = b.GetString().Intern();
                    localeIdToOrdinalRulesId[id] = value;
                }
            }
            catch (MissingManifestResourceException)
            {
                // dummy so we don't try again
                localeIdToCardinalRulesId = new Dictionary<string, string>();
                localeIdToOrdinalRulesId = new Dictionary<string, string>();
                rulesIdToEquivalentULocale = new Dictionary<string, UCultureInfo>();
            }
            return rulesIdToEquivalentULocale;
        }

        /// <summary>
        /// Gets the rulesId from the locale,with locale fallback. If there is no
        /// rulesId, return null. The rulesId might be the empty string if the rule
        /// is the default rule.
        /// </summary>
        public virtual string GetRulesIdForLocale(UCultureInfo locale, PluralType type)
        {
            IDictionary<string, string> idMap = GetLocaleIdToRulesIdMap(type);
            string localeId = UCultureInfo.Canonicalize(locale.Name);
            string rulesId = null;
            while (!idMap.TryGetValue(localeId, out rulesId) || null == rulesId)
            {
                int ix = localeId.LastIndexOf('_');
                if (ix == -1)
                {
                    break;
                }
                localeId = localeId.Substring(0, ix); // ICU4N: Checked 2nd substring arg
            }
            return rulesId;
        }

        /// <summary>
        /// Gets the rule from the rulesId. If there is no rule for this rulesId,
        /// return null.
        /// </summary>
        public virtual PluralRules GetRulesForRulesId(string rulesId)
        {
            return rulesIdToRules.GetOrAdd(rulesId, (key) => {

                UResourceBundle setb;
                try
                {
                    UResourceBundle pluralb = GetPluralBundle();
                    UResourceBundle rulesb = pluralb.Get("rules");
                    setb = rulesb.Get(key);
                }
                catch (MissingManifestResourceException)
                {
                    return null; // can be null
                }

                var ruleList = new PluralRules.RuleList();
                bool parseFailure = false;
                for (int i = 0; i < setb.Length; ++i)
                {
                    UResourceBundle b = setb.Get(i);
                    if (PluralRules.TryParseRule(b.Key, b.GetString(), out PluralRules.Rule rule))
                    {
                        ruleList.AddRule(rule);
                    }
                    else
                    {
                        parseFailure = true;
                    }
                }
                if (parseFailure || ruleList.Count == 0)
                    return null; // can be null

                return new PluralRules(ruleList.Finish());
            });
        }

        /// <summary>
        /// Return the plurals resource. Note <see cref="MissingManifestResourceException"/> is unchecked,
        /// listed here for clarity. Callers should handle this exception.
        /// </summary>
        public virtual UResourceBundle GetPluralBundle()
        {
            return ICUResourceBundle.GetBundleInstance(
                    ICUData.IcuBaseName, "plurals",
                    ICUResourceBundle.IcuDataAssembly, true);
        }

        /// <summary>
        /// Returns the plural rules for the the locale. If we don't have data,
        /// <see cref="PluralRules.Default"/> is returned.
        /// </summary>
#pragma warning disable 672
        public override PluralRules ForLocale(UCultureInfo locale, PluralType type)
#pragma warning restore 672
        {
            string rulesId = GetRulesIdForLocale(locale, type);
            if (rulesId == null || rulesId.Trim().Length == 0)
            {
                return PluralRules.Default;
            }
            PluralRules rules = GetRulesForRulesId(rulesId);
            if (rules == null)
            {
                rules = PluralRules.Default;
            }
            return rules;
        }

        /// <summary>
        /// The only instance of the loader.
        /// </summary>
        private static readonly PluralRulesLoader loader = new PluralRulesLoader();

        /// <summary>
        /// The only instance of the loader.
        /// </summary>
        public static PluralRulesLoader Loader => loader; // ICU4N specific - property accessor for local member

        /// <seealso cref="PluralRulesFactory.HasOverride(UCultureInfo)"/>
#pragma warning disable 672
        public override bool HasOverride(UCultureInfo locale)
#pragma warning restore 672
        {
            return false;
        }

#pragma warning disable 612, 618
        private static readonly PluralRanges UnknownRange 
           = new PluralRanges().Freeze();
#pragma warning restore 612, 618

#pragma warning disable 1591 // No doc comments available
#pragma warning disable 612, 618
        public virtual PluralRanges GetPluralRanges(UCultureInfo locale)
#pragma warning restore 612, 618
        {
            // TODO markdavis Fix the bad fallback, here and elsewhere in this file.
            string localeId = UCultureInfo.Canonicalize(locale.Name);
#pragma warning disable 612, 618
            PluralRanges result;
#pragma warning restore 612, 618
            while (!localeIdToPluralRanges.TryGetValue(localeId, out result) || null == result)
            {
                int ix = localeId.LastIndexOf('_');
                if (ix == -1)
                {
                    result = UnknownRange;
                    break;
                }
                localeId = localeId.Substring(0, ix); // ICU4N: Checked 2nd arg
            }
            return result;
        }

        public virtual bool IsPluralRangesAvailable(UCultureInfo locale)
        {
            return GetPluralRanges(locale) == UnknownRange;
        }
#pragma warning restore 1591

        // TODO markdavis FIX HARD-CODED HACK once we have data from CLDR in the bundles

        // ICU4N: Avoid static constructor by initializing inline
#pragma warning disable 612, 618
        private static IDictionary<string, PluralRanges> LoadLocaleIdToPluralRanges()
#pragma warning restore 612, 618
        {
            string[][] pluralRangeData = new string[][] {
                new string[] {"locales", "id ja km ko lo ms my th vi zh"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "am bn fr gu hi hy kn mr pa zu"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "fa"},
                new string[] {"one", "one", "other"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "ka"},
                new string[] {"one", "other", "one"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "az de el gl hu it kk ky ml mn ne nl pt sq sw ta te tr ug uz"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "af bg ca en es et eu fi nb sv ur"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "da fil is"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "si"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "mk"},
                new string[] {"one", "one", "other"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "lv"},
                new string[] {"zero", "zero", "other"},
                new string[] {"zero", "one", "one"},
                new string[] {"zero", "other", "other"},
                new string[] {"one", "zero", "other"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "other", "other"},
                new string[] {"other", "zero", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "ro"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "other", "other"},
                new string[] {"few", "one", "few"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "other", "other"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "hr sr bs"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "other", "other"},
                new string[] {"few", "one", "one"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "sl"},
                new string[] {"one", "one", "few"},
                new string[] {"one", "two", "two"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "other", "other"},
                new string[] {"two", "one", "few"},
                new string[] {"two", "two", "two"},
                new string[] {"two", "few", "few"},
                new string[] {"two", "other", "other"},
                new string[] {"few", "one", "few"},
                new string[] {"few", "two", "two"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "other", "other"},
                new string[] {"other", "one", "few"},
                new string[] {"other", "two", "two"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "he"},
                new string[] {"one", "two", "other"},
                new string[] {"one", "many", "many"},
                new string[] {"one", "other", "other"},
                new string[] {"two", "many", "other"},
                new string[] {"two", "other", "other"},
                new string[] {"many", "many", "many"},
                new string[] {"many", "other", "many"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "two", "other"},
                new string[] {"other", "many", "many"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "cs pl sk"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "many", "many"},
                new string[] {"one", "other", "other"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "many", "many"},
                new string[] {"few", "other", "other"},
                new string[] {"many", "one", "one"},
                new string[] {"many", "few", "few"},
                new string[] {"many", "many", "many"},
                new string[] {"many", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "many", "many"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "lt ru uk"},
                new string[] {"one", "one", "one"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "many", "many"},
                new string[] {"one", "other", "other"},
                new string[] {"few", "one", "one"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "many", "many"},
                new string[] {"few", "other", "other"},
                new string[] {"many", "one", "one"},
                new string[] {"many", "few", "few"},
                new string[] {"many", "many", "many"},
                new string[] {"many", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "many", "many"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "cy"},
                new string[] {"zero", "one", "one"},
                new string[] {"zero", "two", "two"},
                new string[] {"zero", "few", "few"},
                new string[] {"zero", "many", "many"},
                new string[] {"zero", "other", "other"},
                new string[] {"one", "two", "two"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "many", "many"},
                new string[] {"one", "other", "other"},
                new string[] {"two", "few", "few"},
                new string[] {"two", "many", "many"},
                new string[] {"two", "other", "other"},
                new string[] {"few", "many", "many"},
                new string[] {"few", "other", "other"},
                new string[] {"many", "other", "other"},
                new string[] {"other", "one", "one"},
                new string[] {"other", "two", "two"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "many", "many"},
                new string[] {"other", "other", "other"},

                new string[] {"locales", "ar"},
                new string[] {"zero", "one", "zero"},
                new string[] {"zero", "two", "zero"},
                new string[] {"zero", "few", "few"},
                new string[] {"zero", "many", "many"},
                new string[] {"zero", "other", "other"},
                new string[] {"one", "two", "other"},
                new string[] {"one", "few", "few"},
                new string[] {"one", "many", "many"},
                new string[] {"one", "other", "other"},
                new string[] {"two", "few", "few"},
                new string[] {"two", "many", "many"},
                new string[] {"two", "other", "other"},
                new string[] {"few", "few", "few"},
                new string[] {"few", "many", "many"},
                new string[] {"few", "other", "other"},
                new string[] {"many", "few", "few"},
                new string[] {"many", "many", "many"},
                new string[] {"many", "other", "other"},
                new string[] {"other", "one", "other"},
                new string[] {"other", "two", "other"},
                new string[] {"other", "few", "few"},
                new string[] {"other", "many", "many"},
                new string[] {"other", "other", "other"},
            };
#pragma warning disable 612, 618
            PluralRanges pr = null;
            string[] locales = null;
            IDictionary<string, PluralRanges> tempLocaleIdToPluralRanges = new Dictionary<string, PluralRanges>();
#pragma warning restore 612, 618
            foreach (string[] row in pluralRangeData)
            {
                if (row[0].Equals("locales"))
                {
                    if (pr != null)
                    {
#pragma warning disable 612, 618
                        pr.Freeze();
#pragma warning restore 612, 618
                        foreach (string locale in locales)
                        {
                            tempLocaleIdToPluralRanges[locale] = pr;
                        }
                    }
                    locales = row[1].Split(' ');
#pragma warning disable 612, 618
                    pr = new PluralRanges();
#pragma warning restore 612, 618
                }
                else
                {
                    StandardPluralUtil.TryFromString(row[0], out StandardPlural start);
                    StandardPluralUtil.TryFromString(row[1], out StandardPlural end);
                    StandardPluralUtil.TryFromString(row[2], out StandardPlural result);
#pragma warning disable 612, 618
                    pr.Add(start, end, result);
#pragma warning restore 612, 618
                }
            }
            // do last one
            foreach (string locale in locales)
            {
                tempLocaleIdToPluralRanges[locale] = pr;
            }
            // now make whole thing immutable
            return tempLocaleIdToPluralRanges.AsReadOnly();
        }
    }
}
