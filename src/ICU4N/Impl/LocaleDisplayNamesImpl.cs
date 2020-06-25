// ICU4N: Replaced with DataTableCultureDisplayNames in ICU4N.Globalization

//using ICU4N.Globalization;
//using ICU4N.Impl.Locale;
//using ICU4N.Support.Collections;
//using ICU4N.Text;
//using ICU4N.Util;
//using J2N;
//using J2N.Text;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Resources;
//using System.Text;
//using System.Threading;
//using DialectHandling = ICU4N.Text.DialectHandling;
//using UiListItem = ICU4N.Text.UiListItem;

//namespace ICU4N.Impl
//{
//    public class LocaleDisplayNamesImpl : LocaleDisplayNames // ICU4N TODO: Remove this, as it is only to support ULocale, and we have made a new DataTableCultureDisplayNames class to replace this
//    {
//        private readonly ULocale locale;
//        private readonly DialectHandling dialectHandling;
//        private readonly DisplayContext capitalization;
//        private readonly DisplayContext nameLength;
//        private readonly DisplayContext substituteHandling;
//        private readonly DataTable langData;
//        private readonly DataTable regionData;
//        // Compiled SimpleFormatter patterns.
//        private readonly string separatorFormat;
//        private readonly string format;
//        private readonly string keyTypeFormat;
//        private readonly char formatOpenParen;
//        private readonly char formatReplaceOpenParen;
//        private readonly char formatCloseParen;
//        private readonly char formatReplaceCloseParen;
//        private readonly CurrencyDisplayInfo currencyDisplayInfo;

//        private static readonly Cache cache = new Cache();

//        /// <summary>
//        /// Capitalization context usage types for locale display names
//        /// </summary>
//        private enum CapitalizationContextUsage
//        {
//            Language,
//            Script,
//            Territory,
//            Variant,
//            Key,
//            KeyValue
//        }

//        /// <summary>
//        /// Capitalization transforms. For each usage type, indicates whether to titlecase for
//        /// the context specified in capitalization (which we know at construction time).
//        /// </summary>
//        private bool[] capitalizationUsage = null;
//        /// <summary>
//        /// Map from resource key to <see cref="CapitalizationContextUsage"/> value
//        /// </summary>
//        private static readonly IDictionary<string, CapitalizationContextUsage> contextUsageTypeMap 
//            // ICU4N: Avoid static constructor and initialize inline
//            = new Dictionary<string, CapitalizationContextUsage>
//            {
//                {"languages", CapitalizationContextUsage.Language},
//                {"script",    CapitalizationContextUsage.Script},
//                {"territory", CapitalizationContextUsage.Territory},
//                {"variant",   CapitalizationContextUsage.Variant},
//                {"key",       CapitalizationContextUsage.Key},
//                {"keyValue",  CapitalizationContextUsage.KeyValue},
//            };

//        /// <summary>
//        /// <see cref="BreakIterator"/> to use for capitalization
//        /// </summary>
//        private /*transient*/ BreakIterator capitalizationBrkIter = null;

//        private static readonly TitleCaseMap TO_TITLE_WHOLE_STRING_NO_LOWERCASE =
//                CaseMap.ToTitle().WholeString().NoLowercase();

//        private static string ToTitleWholeStringNoLowercase(ULocale locale, string s)
//        {
//            return TO_TITLE_WHOLE_STRING_NO_LOWERCASE.Apply(
//                    locale.ToLocale(), null, s, new StringBuilder(), null).ToString();
//        }

//        new public static LocaleDisplayNames GetInstance(ULocale locale, DialectHandling dialectHandling)
//        {
//            // ICU4N: Switched to using ReaderWriterLockSlim instead of lock (cache)
//            return cache.Get(locale, dialectHandling);
//        }

//        new public static LocaleDisplayNames GetInstance(ULocale locale, params DisplayContext[] contexts)
//        {
//            // ICU4N: Switched to using ReaderWriterLockSlim instead of lock (cache)
//            return cache.Get(locale, contexts);
//        }

//        private sealed class CapitalizationContextSink : ResourceSink
//        {
//            private readonly LocaleDisplayNamesImpl outerInstance;
//            internal bool hasCapitalizationUsage = false;

//            public CapitalizationContextSink(LocaleDisplayNamesImpl outerInstance)
//            {
//                this.outerInstance = outerInstance;
//            }

//            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
//            {
//                IResourceTable contextsTable = value.GetTable();
//                for (int i = 0; contextsTable.GetKeyAndValue(i, key, value); ++i)
//                {
//                    CapitalizationContextUsage usage;
//                    if (!contextUsageTypeMap.TryGetValue(key.ToString(), out usage))
//                    {
//                        continue;
//                    }


//                    int[] intVector = value.GetInt32Vector();
//                    if (intVector.Length < 2) { continue; }

//                    int titlecaseInt = (outerInstance.capitalization == DisplayContext.CapitalizationForUIListOrMenu)
//                            ? intVector[0] : intVector[1];
//                    if (titlecaseInt == 0) { continue; }

//                    outerInstance.capitalizationUsage[(int)usage] = true;
//                    hasCapitalizationUsage = true;
//                }
//            }
//        }

//        public LocaleDisplayNamesImpl(ULocale locale, DialectHandling dialectHandling)
//                    : this(locale, (dialectHandling == DialectHandling.StandardNames) ? DisplayContext.StandardNames : DisplayContext.DialectNames,
//                    DisplayContext.CapitalizationNone)
//        {
//        }

//        public LocaleDisplayNamesImpl(ULocale locale, params DisplayContext[] contexts)
//#pragma warning disable 612, 618
//            : base()
//#pragma warning restore 612, 618
//        {
//            DialectHandling dialectHandling = DialectHandling.StandardNames;
//            DisplayContext capitalization = DisplayContext.CapitalizationNone;
//            DisplayContext nameLength = DisplayContext.LengthFull;
//            DisplayContext substituteHandling = DisplayContext.Substitute;
//            foreach (DisplayContext contextItem in contexts)
//            {
//                switch (contextItem.Type())
//                {
//                    case DisplayContextType.DialectHandling:
//                        dialectHandling = (contextItem.Value() == DisplayContext.StandardNames.Value()) ?
//                                DialectHandling.StandardNames : DialectHandling.DialectNames;
//                        break;
//                    case DisplayContextType.Capitalization:
//                        capitalization = contextItem;
//                        break;
//                    case DisplayContextType.DisplayLength:
//                        nameLength = contextItem;
//                        break;
//                    case DisplayContextType.SubstituteHandling:
//                        substituteHandling = contextItem;
//                        break;
//                    default:
//                        break;
//                }
//            }

//            this.dialectHandling = dialectHandling;
//            this.capitalization = capitalization;
//            this.nameLength = nameLength;
//            this.substituteHandling = substituteHandling;
//            this.langData = LangDataTables.impl.Get(locale, substituteHandling == DisplayContext.NoSubstitute);
//            this.regionData = RegionDataTables.impl.Get(locale, substituteHandling == DisplayContext.NoSubstitute);
//            this.locale = ULocale.ROOT.Equals(langData.GetLocale()) ? regionData.GetLocale() :
//                langData.GetLocale();

//            // Note, by going through DataTable, this uses table lookup rather than straight lookup.
//            // That should get us the same data, I think.  This way we don't have to explicitly
//            // load the bundle again.  Using direct lookup didn't seem to make an appreciable
//            // difference in performance.
//            string sep = langData.Get("localeDisplayPattern", "separator");
//            if (sep == null || "separator".Equals(sep))
//            {
//                sep = "{0}, {1}";
//            }
//            StringBuilder sb = new StringBuilder();
//            this.separatorFormat = SimpleFormatterImpl.CompileToStringMinMaxArguments(sep, sb, 2, 2);

//            string pattern = langData.Get("localeDisplayPattern", "pattern");
//            if (pattern == null || "pattern".Equals(pattern))
//            {
//                pattern = "{0} ({1})";
//            }
//            this.format = SimpleFormatterImpl.CompileToStringMinMaxArguments(pattern, sb, 2, 2);
//            if (pattern.Contains("（"))
//            {
//                formatOpenParen = '（';
//                formatCloseParen = '）';
//                formatReplaceOpenParen = '［';
//                formatReplaceCloseParen = '］';
//            }
//            else
//            {
//                formatOpenParen = '(';
//                formatCloseParen = ')';
//                formatReplaceOpenParen = '[';
//                formatReplaceCloseParen = ']';
//            }

//            string keyTypePattern = langData.Get("localeDisplayPattern", "keyTypePattern");
//            if (keyTypePattern == null || "keyTypePattern".Equals(keyTypePattern))
//            {
//                keyTypePattern = "{0}={1}";
//            }
//            this.keyTypeFormat = SimpleFormatterImpl.CompileToStringMinMaxArguments(
//                    keyTypePattern, sb, 2, 2);

//            // Get values from the contextTransforms data if we need them
//            // Also check whether we will need a break iterator (depends on the data)
//            bool needBrkIter = false;
//            if (capitalization == DisplayContext.CapitalizationForUIListOrMenu ||
//                    capitalization == DisplayContext.CapitalizationForStandalone)
//            {
//                capitalizationUsage = new bool[Enum.GetValues(typeof(CapitalizationContextUsage)).Length]; // initialized to all false
//                ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, locale);
//                CapitalizationContextSink sink = new CapitalizationContextSink(this);
//                try
//                {
//                    rb.GetAllItemsWithFallback("contextTransforms", sink);
//                }
//                catch (MissingManifestResourceException)
//                {
//                    // Silently ignore.  Not every locale has contextTransforms.
//                }
//                needBrkIter = sink.hasCapitalizationUsage;
//            }
//            // Get a sentence break iterator if we will need it
//            if (needBrkIter || capitalization == DisplayContext.CapitalizationForBeginningOfSentence)
//            {
//                capitalizationBrkIter = BreakIterator.GetSentenceInstance(locale);
//            }

//            this.currencyDisplayInfo = CurrencyData.Provider.GetInstance(locale, false);
//        }

//        public override ULocale Locale => locale;

//        public override DialectHandling DialectHandling => dialectHandling;

//        public override DisplayContext GetContext(DisplayContextType type)
//        {
//            DisplayContext result;
//            switch (type)
//            {
//                case DisplayContextType.DialectHandling:
//                    result = (dialectHandling == DialectHandling.StandardNames) ? DisplayContext.StandardNames : DisplayContext.DialectNames;
//                    break;
//                case DisplayContextType.Capitalization:
//                    result = capitalization;
//                    break;
//                case DisplayContextType.DisplayLength:
//                    result = nameLength;
//                    break;
//                case DisplayContextType.SubstituteHandling:
//                    result = substituteHandling;
//                    break;
//                default:
//                    result = DisplayContext.StandardNames; // hmm, we should do something else here
//                    break;
//            }
//            return result;
//        }

//        private string AdjustForUsageAndContext(CapitalizationContextUsage usage, String name)
//        {
//            if (name != null && name.Length > 0 && UChar.IsLower(name.CodePointAt(0)) &&
//                    (capitalization == DisplayContext.CapitalizationForBeginningOfSentence ||
//                    (capitalizationUsage != null && capitalizationUsage[(int)usage])))
//            {
//                // Note, won't have capitalizationUsage != null && capitalizationUsage[usage.ordinal()]
//                // unless capitalization is CAPITALIZATION_FOR_UI_LIST_OR_MENU or CAPITALIZATION_FOR_STANDALONE
//                lock (this)
//                {
//                    if (capitalizationBrkIter == null)
//                    {
//                        // should only happen when deserializing, etc.
//                        capitalizationBrkIter = BreakIterator.GetSentenceInstance(locale);
//                    }
//                    return UChar.ToTitleCase(locale, name, capitalizationBrkIter,
//                            UChar.TitleCaseNoLowerCase | UChar.TitleCaseNoBreakAdjustment);
//                }
//            }
//            return name;
//        }

//        public override string LocaleDisplayName(ULocale locale) // ICU4N TODO: API - remove
//        {
//            return LocaleDisplayNameInternal(locale);
//        }

//        //public override string LocaleDisplayName(UCultureInfo locale)
//        //{
//        //    return LocaleDisplayNameInternal(locale);
//        //}

//        public override string LocaleDisplayName(CultureInfo locale)
//        {
//            return LocaleDisplayNameInternal(ULocale.ForLocale(locale));
//        }

//        public override string LocaleDisplayName(string localeId)
//        {
//            return LocaleDisplayNameInternal(new UCultureInfo(localeId));
//        }

//        // TODO: implement use of capitalization
//        private string LocaleDisplayNameInternal(ULocale locale)
//        {
//            // lang
//            // lang (script, country, variant, keyword=value, ...)
//            // script, country, variant, keyword=value, ...

//            string resultName = null;

//            string lang = locale.GetLanguage();

//            // Empty basename indicates root locale (keywords are ignored for this).
//            // Our data uses 'root' to access display names for the root locale in the
//            // "Languages" table.
//            if (locale.GetBaseName().Length == 0)
//            {
//                lang = "root";
//            }
//            string script = locale.GetScript();
//            string country = locale.GetCountry();
//            string variant = locale.GetVariant();

//            bool hasScript = script.Length > 0;
//            bool hasCountry = country.Length > 0;
//            bool hasVariant = variant.Length > 0;

//            // always have a value for lang
//            if (dialectHandling == DialectHandling.DialectNames)
//            {
//                do
//                { // loop construct is so we can break early out of search
//                    if (hasScript && hasCountry)
//                    {
//                        string langScriptCountry = lang + '_' + script + '_' + country;
//                        string result = LocaleIdName(langScriptCountry);
//                        if (result != null && !result.Equals(langScriptCountry))
//                        {
//                            resultName = result;
//                            hasScript = false;
//                            hasCountry = false;
//                            break;
//                        }
//                    }
//                    if (hasScript)
//                    {
//                        string langScript = lang + '_' + script;
//                        string result = LocaleIdName(langScript);
//                        if (result != null && !result.Equals(langScript))
//                        {
//                            resultName = result;
//                            hasScript = false;
//                            break;
//                        }
//                    }
//                    if (hasCountry)
//                    {
//                        string langCountry = lang + '_' + country;
//                        string result = LocaleIdName(langCountry);
//                        if (result != null && !result.Equals(langCountry))
//                        {
//                            resultName = result;
//                            hasCountry = false;
//                            break;
//                        }
//                    }
//                } while (false);
//            }

//            if (resultName == null)
//            {
//                string result = LocaleIdName(lang);
//                if (result == null) { return null; }
//                resultName = result
//                        .Replace(formatOpenParen, formatReplaceOpenParen)
//                        .Replace(formatCloseParen, formatReplaceCloseParen);
//            }

//            StringBuilder buf = new StringBuilder();
//            if (hasScript)
//            {
//                // first element, don't need appendWithSep
//                string result = ScriptDisplayNameInContext(script, true);
//                if (result == null) { return null; }
//                buf.Append(result
//                        .Replace(formatOpenParen, formatReplaceOpenParen)
//                        .Replace(formatCloseParen, formatReplaceCloseParen));
//            }
//            if (hasCountry)
//            {
//                string result = RegionDisplayName(country, true);
//                if (result == null) { return null; }
//                AppendWithSep(result
//                        .Replace(formatOpenParen, formatReplaceOpenParen)
//                        .Replace(formatCloseParen, formatReplaceCloseParen), buf);
//            }
//            if (hasVariant)
//            {
//                string result = VariantDisplayName(variant, true);
//                if (result == null) { return null; }
//                AppendWithSep(result
//                        .Replace(formatOpenParen, formatReplaceOpenParen)
//                        .Replace(formatCloseParen, formatReplaceCloseParen), buf);
//            }

//            using (IEnumerator<string> keys = locale.GetKeywords())
//            {
//                if (keys != null)
//                {
//                    while (keys.MoveNext())
//                    {
//                        string key = keys.Current;
//                        string value = locale.GetKeywordValue(key);
//                        string keyDisplayName = KeyDisplayName(key, true);
//                        if (keyDisplayName == null) { return null; }
//                        keyDisplayName = keyDisplayName
//                                .Replace(formatOpenParen, formatReplaceOpenParen)
//                                .Replace(formatCloseParen, formatReplaceCloseParen);
//                        string valueDisplayName = KeyValueDisplayName(key, value, true);
//                        if (valueDisplayName == null) { return null; }
//                        valueDisplayName = valueDisplayName
//                                .Replace(formatOpenParen, formatReplaceOpenParen)
//                                .Replace(formatCloseParen, formatReplaceCloseParen);
//                        if (!valueDisplayName.Equals(value))
//                        {
//                            AppendWithSep(valueDisplayName, buf);
//                        }
//                        else if (!key.Equals(keyDisplayName))
//                        {
//                            string keyValue = SimpleFormatterImpl.FormatCompiledPattern(
//                            keyTypeFormat, keyDisplayName, valueDisplayName);
//                            AppendWithSep(keyValue, buf);
//                        }
//                        else
//                        {
//                            AppendWithSep(keyDisplayName, buf)
//                            .Append("=")
//                            .Append(valueDisplayName);
//                        }
//                    }
//                }
//            }
//            string resultRemainder = null;
//            if (buf.Length > 0)
//            {
//                resultRemainder = buf.ToString();
//            }

//            if (resultRemainder != null)
//            {
//                resultName = SimpleFormatterImpl.FormatCompiledPattern(
//                        format, resultName, resultRemainder);
//            }

//            return AdjustForUsageAndContext(CapitalizationContextUsage.Language, resultName);
//        }

//        private string LocaleIdName(string localeId)
//        {
//            if (nameLength == DisplayContext.LengthShort)
//            {
//                string locIdName = langData.Get("Languages%short", localeId);
//                if (locIdName != null && !locIdName.Equals(localeId))
//                {
//                    return locIdName;
//                }
//            }
//            return langData.Get("Languages", localeId);
//        }

//        public override string LanguageDisplayName(string lang)
//        {
//            // Special case to eliminate non-languages, which pollute our data.
//            if (lang.Equals("root") || lang.IndexOf('_') != -1)
//            {
//                return substituteHandling == DisplayContext.Substitute ? lang : null;
//            }
//            if (nameLength == DisplayContext.LengthShort)
//            {
//                string langName = langData.Get("Languages%short", lang);
//                if (langName != null && !langName.Equals(lang))
//                {
//                    return AdjustForUsageAndContext(CapitalizationContextUsage.Language, langName);
//                }
//            }
//            return AdjustForUsageAndContext(CapitalizationContextUsage.Language, langData.Get("Languages", lang));
//        }

//        public override string ScriptDisplayName(string script)
//        {
//            string str = langData.Get("Scripts%stand-alone", script);
//            if (str == null || str.Equals(script))
//            {
//                if (nameLength == DisplayContext.LengthShort)
//                {
//                    str = langData.Get("Scripts%short", script);
//                    if (str != null && !str.Equals(script))
//                    {
//                        return AdjustForUsageAndContext(CapitalizationContextUsage.Script, str);
//                    }
//                }
//                str = langData.Get("Scripts", script);
//            }
//            return AdjustForUsageAndContext(CapitalizationContextUsage.Script, str);
//        }

//        private string ScriptDisplayNameInContext(string script, bool skipAdjust)
//        {
//            if (nameLength == DisplayContext.LengthShort)
//            {
//                string scriptName2 = langData.Get("Scripts%short", script);
//                if (scriptName2 != null && !scriptName2.Equals(script))
//                {
//                    return skipAdjust ? scriptName2 : AdjustForUsageAndContext(CapitalizationContextUsage.Script, scriptName2);
//                }
//            }
//            string scriptName = langData.Get("Scripts", script);
//            return skipAdjust ? scriptName : AdjustForUsageAndContext(CapitalizationContextUsage.Script, scriptName);
//        }

//#pragma warning disable 672
//        public override string ScriptDisplayNameInContext(string script)
//#pragma warning restore 672
//        {
//            return ScriptDisplayNameInContext(script, false);
//        }

//        public override string ScriptDisplayName(int scriptCode)
//        {
//            return ScriptDisplayName(UScript.GetShortName(scriptCode));
//        }

//        private string RegionDisplayName(string region, bool skipAdjust)
//        {
//            if (nameLength == DisplayContext.LengthShort)
//            {
//                string regionName2 = regionData.Get("Countries%short", region);
//                if (regionName2 != null && !regionName2.Equals(region))
//                {
//                    return skipAdjust ? regionName2 : AdjustForUsageAndContext(CapitalizationContextUsage.Territory, regionName2);
//                }
//            }
//            string regionName = regionData.Get("Countries", region);
//            return skipAdjust ? regionName : AdjustForUsageAndContext(CapitalizationContextUsage.Territory, regionName);
//        }

//        public override string RegionDisplayName(string region)
//        {
//            return RegionDisplayName(region, false);
//        }

//        private string VariantDisplayName(string variant, bool skipAdjust)
//        {
//            // don't have a resource for short variant names
//            string variantName = langData.Get("Variants", variant);
//            return skipAdjust ? variantName : AdjustForUsageAndContext(CapitalizationContextUsage.Variant, variantName);
//        }

//        public override string VariantDisplayName(string variant)
//        {
//            return VariantDisplayName(variant, false);
//        }

//        private string KeyDisplayName(string key, bool skipAdjust)
//        {
//            // don't have a resource for short key names
//            string keyName = langData.Get("Keys", key);
//            return skipAdjust ? keyName : AdjustForUsageAndContext(CapitalizationContextUsage.Key, keyName);
//        }

//        public override string KeyDisplayName(string key)
//        {
//            return KeyDisplayName(key, false);
//        }

//        private string KeyValueDisplayName(string key, string value, bool skipAdjust)
//        {
//            string keyValueName = null;

//            if (key.Equals("currency"))
//            {
//                keyValueName = currencyDisplayInfo.GetName(AsciiUtil.ToUpper(value));
//                if (keyValueName == null)
//                {
//                    keyValueName = value;
//                }
//            }
//            else
//            {
//                if (nameLength == DisplayContext.LengthShort)
//                {
//                    string tmp = langData.Get("Types%short", key, value);
//                    if (tmp != null && !tmp.Equals(value))
//                    {
//                        keyValueName = tmp;
//                    }
//                }
//                if (keyValueName == null)
//                {
//                    keyValueName = langData.Get("Types", key, value);
//                }
//            }

//            return skipAdjust ? keyValueName : AdjustForUsageAndContext(CapitalizationContextUsage.KeyValue, keyValueName);
//        }

//        public override string KeyValueDisplayName(string key, string value)
//        {
//            return KeyValueDisplayName(key, value, false);
//        }

//        public override IList<UiListItem> GetUiListCompareWholeItems(ISet<ULocale> localeSet, IComparer<UiListItem> comparer)
//        {
//            DisplayContext capContext = GetContext(DisplayContextType.Capitalization);

//            List<UiListItem> result = new List<UiListItem>();
//            IDictionary<ULocale, ISet<ULocale>> baseToLocales = new Dictionary<ULocale, ISet<ULocale>>();
//            ULocale.Builder builder = new ULocale.Builder();
//            foreach (ULocale locOriginal in localeSet)
//            {
//                builder.SetLocale(locOriginal); // verify well-formed. We do this here so that we consistently throw exception
//                ULocale loc = ULocale.AddLikelySubtags(locOriginal);
//                ULocale @base = new UCultureInfo(loc.GetLanguage());
//                ISet<ULocale> locales = baseToLocales.Get(@base);
//                if (locales == null)
//                {
//                    baseToLocales[@base] = locales = new HashSet<ULocale>();
//                }
//                locales.Add(loc);
//            }
//            foreach (var entry in baseToLocales)
//            {
//                ULocale @base = entry.Key;
//                ISet<ULocale> values = entry.Value;
//                if (values.Count == 1)
//                {
//                    ULocale locale = values.First();
//#pragma warning disable 612, 618
//                    result.Add(NewRow(ULocale.MinimizeSubtags(locale, ULocale.Minimize.FAVOR_SCRIPT), capContext));
//#pragma warning restore 612, 618
//                }
//                else
//                {
//                    ISet<string> scripts = new HashSet<string>();
//                    ISet<string> regions = new HashSet<string>();
//                    // need the follow two steps to make sure that unusual scripts or regions are displayed
//                    ULocale maxBase = ULocale.AddLikelySubtags(@base);
//                    scripts.Add(maxBase.GetScript());
//                    regions.Add(maxBase.GetCountry());
//                    foreach (ULocale locale in values)
//                    {
//                        scripts.Add(locale.GetScript());
//                        regions.Add(locale.GetCountry());
//                    }
//                    bool hasScripts = scripts.Count > 1;
//                    bool hasRegions = regions.Count > 1;
//                    foreach (ULocale locale in values)
//                    {
//                        ULocale.Builder modified = builder.SetLocale(locale);
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
//            result.Sort(comparer);
//            return result;
//        }

//        private UiListItem NewRow(ULocale modified, DisplayContext capContext)
//        {
//#pragma warning disable 612, 618
//            ULocale minimized = ULocale.MinimizeSubtags(modified, ULocale.Minimize.FAVOR_SCRIPT);
//#pragma warning restore 612, 618
//            string tempName = modified.GetDisplayName(locale);
//            bool titlecase = capContext == DisplayContext.CapitalizationForUIListOrMenu;
//            string nameInDisplayLocale =
//            titlecase ? ToTitleWholeStringNoLowercase(locale, tempName) : tempName;
//            tempName = modified.GetDisplayName(modified);
//            string nameInSelf = capContext ==
//            DisplayContext.CapitalizationForUIListOrMenu ?
//                    ToTitleWholeStringNoLowercase(modified, tempName) : tempName;
//            return new UiListItem(minimized, modified, nameInDisplayLocale, nameInSelf);
//        }

//        public class DataTable
//        {
//            protected bool nullIfNotFound;

//            internal DataTable(bool nullIfNotFound)
//            {
//                this.nullIfNotFound = nullIfNotFound;
//            }

//            public virtual ULocale GetLocale()
//            {
//                return ULocale.ROOT;
//            }

//            public virtual string Get(string tableName, string code)
//            {
//                return Get(tableName, null, code);
//            }

//            public virtual string Get(string tableName, string subTableName, string code)
//            {
//                return nullIfNotFound ? null : code;
//            }
//        }

//        internal class ICUDataTable : DataTable
//        {
//            private readonly ICUResourceBundle bundle;

//            public ICUDataTable(string path, ULocale locale, bool nullIfNotFound)
//                    : base(nullIfNotFound)
//            {
//                this.bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
//                        path, locale.GetBaseName());
//            }

//            public override ULocale GetLocale()
//            {
//                return bundle.GetULocale();
//            }

//            public override string Get(string tableName, string subTableName, string code)
//            {
//                return ICUResourceTableAccess.GetTableString(bundle, tableName, subTableName,
//                        code, nullIfNotFound ? null : code);
//            }
//        }

//        internal class DefaultDataTables : DataTables
//        {
//            public override DataTable Get(ULocale locale, bool nullIfNotFound)
//            {
//                return new DataTable(nullIfNotFound);
//            }
//        }

//        public abstract class DataTables
//        {
//            public abstract DataTable Get(ULocale locale, bool nullIfNotFound);
//            public static DataTables Load(string className)
//            {
//                try
//                {
//                    Type type = Type.GetType(className);
//                    return (DataTables)Activator.CreateInstance(type);
//                }
//                catch (Exception)
//                {
//                    return new DefaultDataTables();
//                }
//            }
//        }

//        public abstract class ICUDataTables : DataTables
//        {
//            private readonly string path;

//            protected ICUDataTables(string path)
//            {
//                this.path = path;
//            }

//            public override DataTable Get(ULocale locale, bool nullIfNotFound)
//            {
//                return new ICUDataTable(path, locale, nullIfNotFound);
//            }
//        }

//        internal static class LangDataTables
//        {
//            // ICU4N TODO: API - create abstract factory so the type to load the tables can be customized. 
//            // In .NET, this doesn't work so well anyway because the name of the assembly must match, not just the namespace.
//            internal static readonly DataTables impl = DataTables.Load("ICU4N.Impl.ICULangDataTables, ICU4N.LanguageData");
//        }

//        internal static class RegionDataTables
//        {
//            // ICU4N TODO: API - create abstract factory so the type to load the tables can be customized. 
//            // In .NET, this doesn't work so well anyway because the name of the assembly must match, not just the namespace.
//            internal static readonly DataTables impl = DataTables.Load("ICU4N.Impl.ICURegionDataTables, ICU4N.RegionData");
//        }

//        public enum DataTableType
//        {
//            Language, Region
//        }

//        public static bool HaveData(DataTableType type)
//        {
//            switch (type)
//            {
//                case DataTableType.Language: return LangDataTables.impl is ICUDataTables;
//                case DataTableType.Region: return RegionDataTables.impl is ICUDataTables;
//                default:
//                    throw new ArgumentException("unknown type: " + type);
//            }
//        }

//        private StringBuilder AppendWithSep(string s, StringBuilder b)
//        {
//            if (b.Length == 0)
//            {
//                b.Append(s);
//            }
//            else
//            {
//                // ICU4N TODO: Does it make sense to call this twice, once for StringBuilder and once for String?
//                SimpleFormatterImpl.FormatAndReplace(separatorFormat, b, null, b.AsCharSequence(), s.AsCharSequence());
//            }
//            return b;
//        }

//        private class Cache
//        {
//            private ULocale locale;
//            private DialectHandling dialectHandling;
//            private DisplayContext capitalization;
//            private DisplayContext nameLength;
//            private DisplayContext substituteHandling;
//            private LocaleDisplayNames cache;
//            private readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

//            public LocaleDisplayNames Get(ULocale locale, DialectHandling dialectHandling)
//            {
//                syncLock.EnterUpgradeableReadLock();
//                try
//                {
//                    if (!(dialectHandling == this.dialectHandling && DisplayContext.CapitalizationNone == this.capitalization &&
//                            DisplayContext.LengthFull == this.nameLength && DisplayContext.Substitute == this.substituteHandling &&
//                            locale.Equals(this.locale)))
//                    {
//                        syncLock.EnterWriteLock();
//                        try
//                        {
//                            if (!(dialectHandling == this.dialectHandling && DisplayContext.CapitalizationNone == this.capitalization &&
//                                DisplayContext.LengthFull == this.nameLength && DisplayContext.Substitute == this.substituteHandling &&
//                                locale.Equals(this.locale)))
//                            {
//                                this.locale = locale;
//                                this.dialectHandling = dialectHandling;
//                                this.capitalization = DisplayContext.CapitalizationNone;
//                                this.nameLength = DisplayContext.LengthFull;
//                                this.substituteHandling = DisplayContext.Substitute;
//                                this.cache = new LocaleDisplayNamesImpl(locale, dialectHandling);
//                            }
//                        }
//                        finally
//                        {
//                            syncLock.ExitWriteLock();
//                        }
//                    }
//                    return cache;
//                }
//                finally
//                {
//                    syncLock.ExitUpgradeableReadLock();
//                }
//            }
//            public LocaleDisplayNames Get(ULocale locale, params DisplayContext[] contexts)
//            {
//                DialectHandling dialectHandlingIn = DialectHandling.StandardNames;
//                DisplayContext capitalizationIn = DisplayContext.CapitalizationNone;
//                DisplayContext nameLengthIn = DisplayContext.LengthFull;
//                DisplayContext substituteHandling = DisplayContext.Substitute;
//                foreach (DisplayContext contextItem in contexts)
//                {
//                    switch (contextItem.Type())
//                    {
//                        case DisplayContextType.DialectHandling:
//                            dialectHandlingIn = (contextItem.Value() == DisplayContext.StandardNames.Value()) ?
//                                    DialectHandling.StandardNames : DialectHandling.DialectNames;
//                            break;
//                        case DisplayContextType.Capitalization:
//                            capitalizationIn = contextItem;
//                            break;
//                        case DisplayContextType.DisplayLength:
//                            nameLengthIn = contextItem;
//                            break;
//                        case DisplayContextType.SubstituteHandling:
//                            substituteHandling = contextItem;
//                            break;
//                        default:
//                            break;
//                    }
//                }
//                syncLock.EnterUpgradeableReadLock();
//                try
//                {
//                    if (!(dialectHandlingIn == this.dialectHandling && capitalizationIn == this.capitalization &&
//                        nameLengthIn == this.nameLength && substituteHandling == this.substituteHandling &&
//                        locale.Equals(this.locale)))
//                    {
//                        syncLock.EnterWriteLock();
//                        try
//                        {
//                            if (!(dialectHandlingIn == this.dialectHandling && capitalizationIn == this.capitalization &&
//                                nameLengthIn == this.nameLength && substituteHandling == this.substituteHandling &&
//                                locale.Equals(this.locale)))
//                            {

//                                this.locale = locale;
//                                this.dialectHandling = dialectHandlingIn;
//                                this.capitalization = capitalizationIn;
//                                this.nameLength = nameLengthIn;
//                                this.substituteHandling = substituteHandling;
//                                this.cache = new LocaleDisplayNamesImpl(locale, contexts);
//                            }
//                        }
//                        finally
//                        {
//                            syncLock.ExitWriteLock();
//                        }
//                    }
//                    return cache;
//                }
//                finally
//                {
//                    syncLock.ExitUpgradeableReadLock();
//                }
//            }
//        }
//    }
//}
