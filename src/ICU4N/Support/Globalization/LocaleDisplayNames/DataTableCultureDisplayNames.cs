using ICU4N.Impl;
using ICU4N.Impl.Locale;
using ICU4N.Util;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using BreakIterator = ICU4N.Text.BreakIterator;
using CaseMap = ICU4N.Text.CaseMap;
using TitleCaseMap = ICU4N.Text.TitleCaseMap;

// Port of impl.LocaleDisplayNamesImpl from ICU4J

namespace ICU4N.Globalization
{
    public class DataTableCultureDisplayNames : CultureDisplayNames
    {
        internal const string DefaultLanguageDataTableProvider = "ICU4N.Globalization.ICULanguageDataTableProvider, ICU4N.LanguageData";
        internal const string DefaultRegionDataTableProvider = "ICU4N.Globalization.ICURegionDataTableProvider, ICU4N.RegionData";

        private static ILanguageDataTableProvider languageDataTableProvider = new DefaultLanguageDataTableProvider();
        private static IRegionDataTableProvider regionDataTableProvider = new DefaultRegionDataTableProvider();

        public static void SetLanguageDataTableProvider(ILanguageDataTableProvider provider)
            => languageDataTableProvider = provider;

        public static ILanguageDataTableProvider GetLanguageDataTableProvider()
            => languageDataTableProvider;

        public static void SetRegionDataTableProvider(IRegionDataTableProvider provider)
            => regionDataTableProvider = provider;

        public static IRegionDataTableProvider GetRegionDataTableProvider()
            => regionDataTableProvider;


        private readonly UCultureInfo locale;
        private readonly DisplayContextOptions displayContextOptions;
        private readonly IDataTable langData;
        private readonly IDataTable regionData;
        // Compiled SimpleFormatter patterns.
        private readonly string separatorFormat;
        private readonly string format;
        private readonly string keyTypeFormat;
        private readonly char formatOpenParen;
        private readonly char formatReplaceOpenParen;
        private readonly char formatCloseParen;
        private readonly char formatReplaceCloseParen;
        private readonly CurrencyDisplayInfo currencyDisplayInfo;

        //private static readonly Cache cache = new Cache();

        /// <summary>
        /// Capitalization context usage types for locale display names
        /// </summary>
        private enum CapitalizationContextUsage
        {
            Language,
            Script,
            Territory,
            Variant,
            Key,
            KeyValue
        }

        /// <summary>
        /// Capitalization transforms. For each usage type, indicates whether to titlecase for
        /// the context specified in capitalization (which we know at construction time).
        /// </summary>
        private bool[] capitalizationUsage = null;
        /// <summary>
        /// Map from resource key to <see cref="CapitalizationContextUsage"/> value
        /// </summary>
        private static readonly IDictionary<string, CapitalizationContextUsage> contextUsageTypeMap
            // ICU4N: Avoid static constructor and initialize inline
            = new Dictionary<string, CapitalizationContextUsage>
            {
                {"languages", CapitalizationContextUsage.Language},
                {"script",    CapitalizationContextUsage.Script},
                {"territory", CapitalizationContextUsage.Territory},
                {"variant",   CapitalizationContextUsage.Variant},
                {"key",       CapitalizationContextUsage.Key},
                {"keyValue",  CapitalizationContextUsage.KeyValue},
            };

        /// <summary>
        /// <see cref="BreakIterator"/> to use for capitalization
        /// </summary>
        private /*transient*/ BreakIterator capitalizationBrkIter = null;

        private static readonly TitleCaseMap ToTitleWholeStringNoLower =
                CaseMap.ToTitle().WholeString().NoLowercase();

        private static string ToTitleWholeStringNoLowercase(UCultureInfo culture, string s)
        {
            return ToTitleWholeStringNoLower.Apply(
                    culture, null, s, new StringBuilder(), null).ToString();
        }

        // ICU4N TODO: We need to work out what to do with this - I am not sure this needs to be public and/or static
        //new public static CultureDisplayNames GetInstance(CultureInfo culture, DisplayContextOptions options)
        //{
        //    // ICU4N: Switched to using ReaderWriterLockSlim instead of lock (cache)
        //    return cache.Get(culture.ToUCultureInfo(), options);
        //}

        private sealed class CapitalizationContextSink : ResourceSink
        {
            private readonly DataTableCultureDisplayNames dataTableCultureDisplayNames;
            internal bool hasCapitalizationUsage = false;

            public CapitalizationContextSink(DataTableCultureDisplayNames dataTableCultureDisplayNames)
            {
                this.dataTableCultureDisplayNames = dataTableCultureDisplayNames;
            }

            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable contextsTable = value.GetTable();
                for (int i = 0; contextsTable.GetKeyAndValue(i, key, value); ++i)
                {
                    if (!contextUsageTypeMap.TryGetValue(key.ToString(), out CapitalizationContextUsage usage))
                    {
                        continue;
                    }


                    int[] intVector = value.GetInt32Vector();
                    if (intVector.Length < 2) { continue; }

                    int titlecaseInt = (dataTableCultureDisplayNames.displayContextOptions.Capitalization == Capitalization.UIListOrMenu)
                            ? intVector[0] : intVector[1];
                    if (titlecaseInt == 0) { continue; }

                    dataTableCultureDisplayNames.capitalizationUsage[(int)usage] = true;
                    hasCapitalizationUsage = true;
                }
            }
        }

        public DataTableCultureDisplayNames(UCultureInfo culture, DisplayContextOptions options)
#pragma warning disable 612, 618
            : base()
#pragma warning restore 612, 618
        {
            this.displayContextOptions = options.Freeze();
            this.langData = languageDataTableProvider.GetDataTable(culture, options.SubstituteHandling == SubstituteHandling.NoSubstitute);
            this.regionData = regionDataTableProvider.GetDataTable(culture, options.SubstituteHandling == SubstituteHandling.NoSubstitute);
            this.locale = langData.CultureInfo != null && langData.CultureInfo.Equals(CultureInfo.InvariantCulture)
                ? regionData.CultureInfo.ToUCultureInfo()
                : langData.CultureInfo.ToUCultureInfo();

            // Note, by going through DataTable, this uses table lookup rather than straight lookup.
            // That should get us the same data, I think.  This way we don't have to explicitly
            // load the bundle again.  Using direct lookup didn't seem to make an appreciable
            // difference in performance.
            string sep = langData.Get("localeDisplayPattern", "separator");
            if (sep == null || "separator".Equals(sep))
            {
                sep = "{0}, {1}";
            }
            StringBuilder sb = new StringBuilder();
            this.separatorFormat = SimpleFormatterImpl.CompileToStringMinMaxArguments(sep, sb, 2, 2);

            string pattern = langData.Get("localeDisplayPattern", "pattern");
            if (pattern == null || "pattern".Equals(pattern))
            {
                pattern = "{0} ({1})";
            }
            this.format = SimpleFormatterImpl.CompileToStringMinMaxArguments(pattern, sb, 2, 2);
            if (pattern.Contains("（"))
            {
                formatOpenParen = '（';
                formatCloseParen = '）';
                formatReplaceOpenParen = '［';
                formatReplaceCloseParen = '］';
            }
            else
            {
                formatOpenParen = '(';
                formatCloseParen = ')';
                formatReplaceOpenParen = '[';
                formatReplaceCloseParen = ']';
            }

            string keyTypePattern = langData.Get("localeDisplayPattern", "keyTypePattern");
            if (keyTypePattern == null || "keyTypePattern".Equals(keyTypePattern))
            {
                keyTypePattern = "{0}={1}";
            }
            this.keyTypeFormat = SimpleFormatterImpl.CompileToStringMinMaxArguments(
                    keyTypePattern, sb, 2, 2);

            // Get values from the contextTransforms data if we need them
            // Also check whether we will need a break iterator (depends on the data)
            bool needBrkIter = false;
            if (options.Capitalization == Capitalization.UIListOrMenu ||
                    options.Capitalization == Capitalization.Standalone)
            {
                capitalizationUsage = new bool[Enum.GetValues(typeof(CapitalizationContextUsage)).Length]; // initialized to all false
                ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, locale);
                CapitalizationContextSink sink = new CapitalizationContextSink(this);
                try
                {
                    rb.GetAllItemsWithFallback("contextTransforms", sink);
                }
                catch (MissingManifestResourceException)
                {
                    // Silently ignore.  Not every locale has contextTransforms.
                }
                needBrkIter = sink.hasCapitalizationUsage;
            }
            // Get a sentence break iterator if we will need it
            if (needBrkIter || options.Capitalization == Capitalization.BeginningOfSentence)
            {
                capitalizationBrkIter = BreakIterator.GetSentenceInstance(locale);
            }

            this.currencyDisplayInfo = CurrencyData.Provider.GetInstance(locale.ToUCultureInfo(), false);
        }

        public override UCultureInfo Culture => locale;

        public override DisplayContextOptions DisplayContextOptions => displayContextOptions;

        private string AdjustForUsageAndContext(CapitalizationContextUsage usage, string name)
        {
            if (name != null && name.Length > 0 && UChar.IsLower(name.CodePointAt(0)) &&
                    (displayContextOptions.Capitalization == Capitalization.BeginningOfSentence ||
                    (capitalizationUsage != null && capitalizationUsage[(int)usage])))
            {
                // Note, won't have capitalizationUsage != null && capitalizationUsage[usage.ordinal()]
                // unless capitalization is CAPITALIZATION_FOR_UI_LIST_OR_MENU or CAPITALIZATION_FOR_STANDALONE

                // should only happen when deserializing, etc.
                LazyInitializer.EnsureInitialized(ref capitalizationBrkIter, () => BreakIterator.GetSentenceInstance(locale));
                return UChar.ToTitleCase(locale, name, capitalizationBrkIter,
                            UChar.TitleCaseNoLowerCase | UChar.TitleCaseNoBreakAdjustment);
            }
            return name;
        }

        public override string GetLocaleDisplayName(CultureInfo locale)
        {
            return GetLocaleDisplayNameInternal(locale.ToUCultureInfo());
        }

        public override string GetLocaleDisplayName(string localeId)
        {
            return GetLocaleDisplayNameInternal(new UCultureInfo(localeId));
        }

        // TODO: implement use of capitalization
        private string GetLocaleDisplayNameInternal(UCultureInfo locale)
        {
            // lang
            // lang (script, country, variant, keyword=value, ...)
            // script, country, variant, keyword=value, ...

            string resultName = null;

            string lang = locale.Language;

            // Empty basename indicates root locale (keywords are ignored for this).
            // Our data uses 'root' to access display names for the root locale in the
            // "Languages" table.
            if (locale.Name.Length == 0)
            {
                lang = "root";
            }
            string script = locale.Script;
            string country = locale.Country;
            string variant = locale.Variant;

            bool hasScript = script.Length > 0;
            bool hasCountry = country.Length > 0;
            bool hasVariant = variant.Length > 0;

            // always have a value for lang
            if (displayContextOptions.DialectHandling == DialectHandling.DialectNames)
            {
                do
                { // loop construct is so we can break early out of search
                    if (hasScript && hasCountry)
                    {
                        string langScriptCountry = lang + '_' + script + '_' + country;
                        string result = LocaleIdName(langScriptCountry);
                        if (result != null && !result.Equals(langScriptCountry))
                        {
                            resultName = result;
                            hasScript = false;
                            hasCountry = false;
                            break;
                        }
                    }
                    if (hasScript)
                    {
                        string langScript = lang + '_' + script;
                        string result = LocaleIdName(langScript);
                        if (result != null && !result.Equals(langScript))
                        {
                            resultName = result;
                            hasScript = false;
                            break;
                        }
                    }
                    if (hasCountry)
                    {
                        string langCountry = lang + '_' + country;
                        string result = LocaleIdName(langCountry);
                        if (result != null && !result.Equals(langCountry))
                        {
                            resultName = result;
                            hasCountry = false;
                            break;
                        }
                    }
                } while (false);
            }

            if (resultName == null)
            {
                string result = LocaleIdName(lang);
                if (result == null) { return null; }
                resultName = result
                        .Replace(formatOpenParen, formatReplaceOpenParen)
                        .Replace(formatCloseParen, formatReplaceCloseParen);
            }

            StringBuilder buf = new StringBuilder();
            if (hasScript)
            {
                // first element, don't need appendWithSep
                string result = GetScriptDisplayNameInContext(script, true);
                if (result == null) { return null; }
                buf.Append(result
                        .Replace(formatOpenParen, formatReplaceOpenParen)
                        .Replace(formatCloseParen, formatReplaceCloseParen));
            }
            if (hasCountry)
            {
                string result = GetRegionDisplayName(country, true);
                if (result == null) { return null; }
                AppendWithSep(result
                        .Replace(formatOpenParen, formatReplaceOpenParen)
                        .Replace(formatCloseParen, formatReplaceCloseParen), buf);
            }
            if (hasVariant)
            {
                string result = GetVariantDisplayName(variant, true);
                if (result == null) { return null; }
                AppendWithSep(result
                        .Replace(formatOpenParen, formatReplaceOpenParen)
                        .Replace(formatCloseParen, formatReplaceCloseParen), buf);
            }

            using (var pairs = locale.Keywords.GetEnumerator())
            {
                if (pairs != null)
                {
                    while (pairs.MoveNext())
                    {
                        string key = pairs.Current.Key;
                        string value = pairs.Current.Value; // locale.GetKeywordValue(key);
                        string keyDisplayName = GetKeyDisplayName(key, true);
                        if (keyDisplayName == null) { return null; }
                        keyDisplayName = keyDisplayName
                                .Replace(formatOpenParen, formatReplaceOpenParen)
                                .Replace(formatCloseParen, formatReplaceCloseParen);
                        string valueDisplayName = GetKeyValueDisplayName(key, value, true);
                        if (valueDisplayName == null) { return null; }
                        valueDisplayName = valueDisplayName
                                .Replace(formatOpenParen, formatReplaceOpenParen)
                                .Replace(formatCloseParen, formatReplaceCloseParen);
                        if (!valueDisplayName.Equals(value))
                        {
                            AppendWithSep(valueDisplayName, buf);
                        }
                        else if (!key.Equals(keyDisplayName))
                        {
                            string keyValue = SimpleFormatterImpl.FormatCompiledPattern(
                            keyTypeFormat, keyDisplayName, valueDisplayName);
                            AppendWithSep(keyValue, buf);
                        }
                        else
                        {
                            AppendWithSep(keyDisplayName, buf)
                            .Append("=")
                            .Append(valueDisplayName);
                        }
                    }
                }
            }
            string resultRemainder = null;
            if (buf.Length > 0)
            {
                resultRemainder = buf.ToString();
            }

            if (resultRemainder != null)
            {
                resultName = SimpleFormatterImpl.FormatCompiledPattern(
                        format, resultName, resultRemainder);
            }

            return AdjustForUsageAndContext(CapitalizationContextUsage.Language, resultName);
        }

        private string LocaleIdName(string localeId)
        {
            if (displayContextOptions.DisplayLength == DisplayLength.Short)
            {
                string locIdName = langData.Get("Languages%short", localeId);
                if (locIdName != null && !locIdName.Equals(localeId))
                {
                    return locIdName;
                }
            }
            return langData.Get("Languages", localeId);
        }

        public override string GetLanguageDisplayName(string language)
        {
            // Special case to eliminate non-languages, which pollute our data.
            if (language.Equals("root") || language.IndexOf('_') != -1)
            {
                return displayContextOptions.SubstituteHandling == SubstituteHandling.Substitute ? language : null;
            }
            if (displayContextOptions.DisplayLength == DisplayLength.Short)
            {
                string langName = langData.Get("Languages%short", language);
                if (langName != null && !langName.Equals(language))
                {
                    return AdjustForUsageAndContext(CapitalizationContextUsage.Language, langName);
                }
            }
            return AdjustForUsageAndContext(CapitalizationContextUsage.Language, langData.Get("Languages", language));
        }

        public override string GetScriptDisplayName(string script)
        {
            string str = langData.Get("Scripts%stand-alone", script);
            if (str == null || str.Equals(script))
            {
                if (displayContextOptions.DisplayLength == DisplayLength.Short)
                {
                    str = langData.Get("Scripts%short", script);
                    if (str != null && !str.Equals(script))
                    {
                        return AdjustForUsageAndContext(CapitalizationContextUsage.Script, str);
                    }
                }
                str = langData.Get("Scripts", script);
            }
            return AdjustForUsageAndContext(CapitalizationContextUsage.Script, str);
        }

        private string GetScriptDisplayNameInContext(string script, bool skipAdjust)
        {
            if (displayContextOptions.DisplayLength == DisplayLength.Short)
            {
                string scriptName2 = langData.Get("Scripts%short", script);
                if (scriptName2 != null && !scriptName2.Equals(script))
                {
                    return skipAdjust ? scriptName2 : AdjustForUsageAndContext(CapitalizationContextUsage.Script, scriptName2);
                }
            }
            string scriptName = langData.Get("Scripts", script);
            return skipAdjust ? scriptName : AdjustForUsageAndContext(CapitalizationContextUsage.Script, scriptName);
        }

#pragma warning disable 672
        internal override string GetScriptDisplayNameInContext(string script)
#pragma warning restore 672
        {
            return GetScriptDisplayNameInContext(script, false);
        }

        public override string GetScriptDisplayName(int scriptCode)
        {
            return GetScriptDisplayName(UScript.GetShortName(scriptCode));
        }

        private string GetRegionDisplayName(string region, bool skipAdjust)
        {
            if (displayContextOptions.DisplayLength == DisplayLength.Short)
            {
                string regionName2 = regionData.Get("Countries%short", region);
                if (regionName2 != null && !regionName2.Equals(region))
                {
                    return skipAdjust ? regionName2 : AdjustForUsageAndContext(CapitalizationContextUsage.Territory, regionName2);
                }
            }
            string regionName = regionData.Get("Countries", region);
            return skipAdjust ? regionName : AdjustForUsageAndContext(CapitalizationContextUsage.Territory, regionName);
        }

        public override string GetRegionDisplayName(string region)
        {
            return GetRegionDisplayName(region, false);
        }

        private string GetVariantDisplayName(string variant, bool skipAdjust)
        {
            // don't have a resource for short variant names
            string variantName = langData.Get("Variants", variant);
            return skipAdjust ? variantName : AdjustForUsageAndContext(CapitalizationContextUsage.Variant, variantName);
        }

        public override string GetVariantDisplayName(string variant)
        {
            return GetVariantDisplayName(variant, false);
        }

        private string GetKeyDisplayName(string key, bool skipAdjust)
        {
            // don't have a resource for short key names
            string keyName = langData.Get("Keys", key);
            return skipAdjust ? keyName : AdjustForUsageAndContext(CapitalizationContextUsage.Key, keyName);
        }

        public override string GetKeyDisplayName(string key)
        {
            return GetKeyDisplayName(key, false);
        }

        private string GetKeyValueDisplayName(string key, string value, bool skipAdjust)
        {
            string keyValueName = null;

            if (key.Equals("currency"))
            {
                keyValueName = currencyDisplayInfo.GetName(AsciiUtil.ToUpper(value));
                if (keyValueName == null)
                {
                    keyValueName = value;
                }
            }
            else
            {
                if (displayContextOptions.DisplayLength == DisplayLength.Short)
                {
                    string tmp = langData.Get("Types%short", key, value);
                    if (tmp != null && !tmp.Equals(value))
                    {
                        keyValueName = tmp;
                    }
                }
                if (keyValueName == null)
                {
                    keyValueName = langData.Get("Types", key, value);
                }
            }

            return skipAdjust ? keyValueName : AdjustForUsageAndContext(CapitalizationContextUsage.KeyValue, keyValueName);
        }

        public override string GetKeyValueDisplayName(string key, string value)
        {
            return GetKeyValueDisplayName(key, value, false);
        }

        public override IList<UiListItem> GetUiListCompareWholeItems(ICollection<CultureInfo> cultures, IComparer<UiListItem> comparer)
        {
            Capitalization capContext = displayContextOptions.Capitalization;

            List<UiListItem> result = new List<UiListItem>();

            // ICU4N TODO: Finish implementation (missing UCultureInfo.Builder dependency)

            //            IDictionary<UCultureInfo, ISet<UCultureInfo>> baseToLocales = new Dictionary<UCultureInfo, ISet<UCultureInfo>>();
            //            UCultureInfo.Builder builder = new UCultureInfo.Builder();
            //            foreach (ULocale locOriginal in localeSet)
            //            {
            //                builder.SetLocale(locOriginal); // verify well-formed. We do this here so that we consistently throw exception
            //                UCultureInfo loc = UCultureInfo.AddLikelySubtags(locOriginal);
            //                UCultureInfo @base = new UCultureInfo(loc.Language);
            //                ISet<ULocale> locales = baseToLocales.Get(@base);
            //                if (locales == null)
            //                {
            //                    baseToLocales[@base] = locales = new HashSet<UCultureInfo>();
            //                }
            //                locales.Add(loc);
            //            }
            //            foreach (var entry in baseToLocales)
            //            {
            //                UCultureInfo @base = entry.Key;
            //                ISet<UCultureInfo> values = entry.Value;
            //                if (values.Count == 1)
            //                {
            //                    ULocale locale = values.First();
            //#pragma warning disable 612, 618
            //                    result.Add(NewRow(UCultureInfo.MinimizeSubtags(locale, UCultureInfo.Minimize.FavorScript), capContext));
            //#pragma warning restore 612, 618
            //                }
            //                else
            //                {
            //                    ISet<string> scripts = new HashSet<string>();
            //                    ISet<string> regions = new HashSet<string>();
            //                    // need the follow two steps to make sure that unusual scripts or regions are displayed
            //                    ULocale maxBase = UCultureInfo.AddLikelySubtags(@base);
            //                    scripts.Add(maxBase.Script);
            //                    regions.Add(maxBase.Country);
            //                    foreach (ULocale locale in values)
            //                    {
            //                        scripts.Add(locale.Script);
            //                        regions.Add(locale.Country);
            //                    }
            //                    bool hasScripts = scripts.Count > 1;
            //                    bool hasRegions = regions.Count > 1;
            //                    foreach (UCultureInfo locale in values)
            //                    {
            //                        UCultureInfo.Builder modified = builder.SetLocale(locale);
            //                        if (!hasScripts)
            //                        {
            //                            modified.SetScript("");
            //                        }
            //                        if (!hasRegions)
            //                        {
            //                            modified.SetRegion("");
            //                        }
            //                        result.Add(NewRow(modified.Build(), capContext));
            //                    }
            //                }
            //            }
            result.Sort(comparer);
            return result;
        }

        private UiListItem NewRow(UCultureInfo modified, Capitalization capitalization)
        {
#pragma warning disable 612, 618
            UCultureInfo minimized = UCultureInfo.MinimizeSubtags(modified, UCultureInfo.Minimize.FavorScript);
#pragma warning restore 612, 618
            string tempName = modified.GetDisplayName(locale);
            bool titlecase = capitalization == Capitalization.UIListOrMenu;
            string nameInDisplayLocale =
            titlecase ? ToTitleWholeStringNoLowercase(locale, tempName) : tempName;
            tempName = modified.GetDisplayName(modified);
            string nameInSelf = capitalization ==
            Capitalization.UIListOrMenu ?
                    ToTitleWholeStringNoLowercase(modified, tempName) : tempName;
            return new UiListItem(minimized, modified, nameInDisplayLocale, nameInSelf);
        }

        public enum DataTableType
        {
            Language, Region
        }

        public static bool HasData(DataTableType type)
        {
            switch (type)
            {
                case DataTableType.Language: return languageDataTableProvider.HasData;
                case DataTableType.Region: return regionDataTableProvider.HasData;
                default:
                    throw new ArgumentException("unknown type: " + type);
            }
        }

        private StringBuilder AppendWithSep(string s, StringBuilder b)
        {
            if (b.Length == 0)
            {
                b.Append(s);
            }
            else
            {
                SimpleFormatterImpl.FormatAndReplace(separatorFormat, b, null, b.AsCharSequence(), s.AsCharSequence());
            }
            return b;
        }

        //private class Cache
        //{
        //    private UCultureInfo locale;
        //    private DisplayContextOptions displayContextOptions;
        //    private CultureDisplayNames cache;
        //    private readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        //    public CultureDisplayNames Get(UCultureInfo locale, DisplayContextOptions displayContextOptions)
        //    {
        //        syncLock.EnterUpgradeableReadLock();
        //        try
        //        {
        //            if (!(this.displayContextOptions == displayContextOptions && locale.Equals(this.locale)))
        //            {
        //                syncLock.EnterWriteLock();
        //                try
        //                {
        //                    if (!(this.displayContextOptions == displayContextOptions && locale.Equals(this.locale)))
        //                    {
        //                        this.locale = locale;
        //                        this.displayContextOptions = displayContextOptions.Freeze();
        //                        this.cache = new DataTableCultureDisplayNames(locale, displayContextOptions);
        //                    }
        //                }
        //                finally
        //                {
        //                    syncLock.ExitWriteLock();
        //                }
        //            }

        //            return cache;
        //        }
        //        finally
        //        {
        //            syncLock.ExitUpgradeableReadLock();
        //        }
        //    }
        //}
    }
}
