using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Threading;
#nullable enable

namespace ICU4N.Globalization
{
    // ICU4N: Loosely modeled after System.Globalization.CultureData.
    internal class UCultureData
    {
        // Basics
        internal string localeID; // Name you passed in (ie: en-US, en, or de-DE-PHONEBOOK)
        private bool isInvariantCulture; // ICU4N TODO: Move the logic for dealing with these to UCultureData from UCultureInfo, as is the case in .NET
        private bool isNeutralCulture;

        // Identity
        internal string name; // normalized locale name (ie: en-US) - same as UCultureInfo.Name property.

        // Numbers
        private readonly string? numbersKeyword; // The numbers keyword from the original passed in string (en@numbers=thai). Copied from UCultureInfo upon creation. This is always in sync because the collection is readonly.
        internal string? positiveSign; // (user can override) positive sign
        internal string? negativeSign; // (user can override) negative sign
        // (nfi populates these 5, don't have to be = undef)
        //internal int digits; // (user can override) number of fractional digits
        //internal int iNegativeNumber; // (user can override) negative number format
        internal string? decimalFormat; // The format string for for general numbers
        internal int[]? grouping; // (user can override) grouping of digits
        internal string? decimalSeparator; // (user can override) decimal separator
        internal string? groupSeparator; // (user can override) thousands separator
        internal string? naN; // Not a Number
        internal string? positiveInfinity; // + Infinity
        internal string? negativeInfinity; // - Infinity // ICU4N: Need to combine negativesign with infinity

        internal string? patternSeparator; // Character used to separate positive and negative subpatterns in a pattern. (ICU4N: don't need yet)

        // Percent
        //internal int _iNegativePercent = undef; // Negative Percent (0-3)
        //internal int _iPositivePercent = undef; // Positive Percent (0-11)
        internal string? percent; // Percent (%) symbol
        internal string? perMille; // PerMille symbol
        internal string? percentFormat; // The format string for percent

        // Scientific
        internal string? scientificFormat; // The format string for scientific
        internal string? exponentSeparator; // The string used to separate the mantissa from the exponent. Examples: "x10^" for 1.23x10^4, "E" for 1.23E4. Used in localized patterns and formatted strings.
        internal string? exponentMultiplicationSign;


        // Currency
        internal readonly string? cfKeyword; // The currency format keyword from the original localeID
        internal string? currency; // (user can override) local monetary symbol
        internal string? intlMonetarySymbol; // international monetary symbol (RegionInfo)
        //internal string? _sEnglishCurrency; // English name for this currency
        //internal string? _sNativeCurrency; // Native name for this currency
        // (nfi populates these 4, don't have to be = undef)
        //internal int _iCurrencyDigits; // (user can override) # local monetary fractional digits
        //internal int _iCurrency; // (user can override) positive currency format
        //internal int _iNegativeCurrency; // (user can override) negative currency format
        internal int[]? monetaryGrouping; // (user can override) monetary grouping of digits
        internal string? monetaryDecimal; // (user can override) monetary decimal separator
        internal string? monetaryGroupSeparator; // (user can override) monetary thousands separator
        internal string? currencyFormat; // The format string for currency
        internal string? accountingFormat; // the format string for accounting

        // Numbering System (for base 10)
        internal string? nativeDigits; // The digits for formatting strings
        internal string? numberingSystemName; // The numbering system name
        internal bool isAlgorithmic; // This is used to decide between decimal format and RBNF for "general" formatting of a culture

        // Capitalization Settings
        internal bool capitalizationForListOrMenu;
        internal bool capitalizationForStandAlone;
        private BreakIterator? sentenceBreakIterator;
        private readonly string? lbKeyword; // Controls the resource data set that is looked up - line break behavior
        private readonly string? ssKeyword; // If "ss=standard", wraps a sentence iterator in a SimpleFilteredSentenceBreakIterator


        // IMPORTANT: There is likely a concurrency bug somewhere in the dictionary-based break iterators. This
        // lock is static and exposed publically only to work around this problem. Once the bug has been
        // tracked down, we can make this non-static and remove the SentenceBreakIteratorLock property.
        private static readonly object sentenceBreakIteratorLock = new object();

        // Plural rules
        private PluralRules? ordinalPluralRules;
        private PluralRules? cardinalPluralRules;

        public object SentenceBreakIteratorLock => sentenceBreakIteratorLock;

        // IMPORTANT: This is not threadsafe. A clone should be made to the current thread before use.
        // Do NOT call this in the constructor of UCultureInfo
        public BreakIterator SentenceBreakIterator
        //=> LazyInitializer.EnsureInitialized(ref sentenceBreakIterator, () => BreakIterator.GetSentenceInstance(culture));
        {
            get
            {
                if (sentenceBreakIterator is not null)
                    return sentenceBreakIterator;
                lock (sentenceBreakIteratorLock)
                {
                    if (sentenceBreakIterator is not null)
                        return sentenceBreakIterator;

                    sentenceBreakIterator = BreakIterator.GetSentenceInstance(name, localeID, lbKeyword, ssKeyword);
                    return sentenceBreakIterator;
                }
            }
        }

        // Do NOT call this in the constructor of UCultureInfo
        public PluralRules OrdinalPluralRules
            => LazyInitializer.EnsureInitialized(ref ordinalPluralRules, () => PluralRules.GetInstance(name, PluralType.Ordinal));

        // Do NOT call this in the constructor of UCultureInfo
        public PluralRules CardinalPluralRules
            => LazyInitializer.EnsureInitialized(ref cardinalPluralRules, () => PluralRules.GetInstance(name, PluralType.Cardinal));


        private object? nonNullIfNFIInitialized; // Marker to tell us we are "done" loading NFI data if not null.

        [MemberNotNull(nameof(positiveSign))]
        [MemberNotNull(nameof(negativeSign))]
        [MemberNotNull(nameof(decimalFormat))]
        // ICU4N TODO: grouping (need to parse decimalFormat pattern to get it)
        [MemberNotNull(nameof(decimalSeparator))]
        [MemberNotNull(nameof(groupSeparator))]
        [MemberNotNull(nameof(naN))]
        [MemberNotNull(nameof(positiveInfinity))]
        [MemberNotNull(nameof(negativeInfinity))]
        [MemberNotNull(nameof(patternSeparator))]
        [MemberNotNull(nameof(percent))]
        [MemberNotNull(nameof(perMille))]
        [MemberNotNull(nameof(percentFormat))]
        [MemberNotNull(nameof(scientificFormat))]
        [MemberNotNull(nameof(exponentSeparator))]
        [MemberNotNull(nameof(exponentMultiplicationSign))]
        // cfKeyword may be null
        [MemberNotNull(nameof(currency))]
        [MemberNotNull(nameof(intlMonetarySymbol))]
        // Need to parse monetaryGrouping from currencyFormat (and/or accountingFormat ?)
        [MemberNotNull(nameof(monetaryDecimal))]
        [MemberNotNull(nameof(monetaryGroupSeparator))]
        [MemberNotNull(nameof(currencyFormat))]
        [MemberNotNull(nameof(accountingFormat))]

        [MemberNotNull(nameof(nativeDigits))]
        [MemberNotNull(nameof(numberingSystemName))]
        private void EnsureNumberFormatInfoInitialized()
        {
            LazyInitializer.EnsureInitialized(ref nonNullIfNFIInitialized, () =>
            {
                LoadNumberFormatInfo(this);
                return new object(); // Marker to tell us we are "done" loading NFI data if not null.
            });
        }

        private static UCultureData CreateCultureWithInvariantData()
        {
            var invariant = new UCultureData();
            invariant.localeID = string.Empty;
            invariant.name = string.Empty;
            invariant.isInvariantCulture = true;
            invariant.isNeutralCulture = false;
            // The rest is lazy-loaded from resources

            return invariant;
        }

        // CONFUSING: The ctor of UCultureInfo is responsible for setting its own cultureData field.
        // So, the sequence is:
        // 1. UCultureInfo.constructor call
        // 2. UCultureInfo.InvariantCulture.cultureData is set
        // 3. UCultureInfo.invariantCultureInfo is set (backing field for UCultureInfo.InvariantCulture)
        // 4. This property is then able to return cultureData since both have been initialized.
        internal static UCultureData Invariant => s_Invariant ??= CreateCultureWithInvariantData();
        private static volatile UCultureData? s_Invariant;

        // Cache of cultures we've already looked up
        private static volatile Dictionary<string, UCultureData>? s_cachedCultures;
        private static readonly object s_lock = new object();

        // Clear our internal caches
        internal static void ClearCachedData()
        {
            s_cachedCultures = null;
            //s_cachedRegions = null;
        }

        internal static UCultureData GetCultureData(UCultureInfo cultureInfo, bool useDataCache = true)
        {
            Debug.Assert(cultureInfo is not null);

            // ICU4N: We cannot use the invariant culture if there are keywords to parse,
            // so we must use localeID here.
            if (string.IsNullOrEmpty(cultureInfo.FullName))
            {
                return Invariant;
            }

            Dictionary<string, UCultureData>? tempHashTable = s_cachedCultures;
            string? hashName = null;
            if (useDataCache)
            {
                hashName = cultureInfo.FullName; // ICU4N TODO: This will probably have more cache entries than we need, but Canonicalize() has lots of overhead - need to optimize it before using it. And we need a way to add the keywords to the canonicalized name.
                if (tempHashTable == null)
                {
                    // No table yet, make a new one
                    tempHashTable = new Dictionary<string, UCultureData>();
                }
                else
                {
                    // Check the hash table
                    bool ret;
                    UCultureData? retVal;
                    lock (s_lock)
                    {
                        ret = tempHashTable.TryGetValue(hashName, out retVal);
                    }
                    if (ret && retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            // Not found in the hash table, need to see if we can build one that works for us
            UCultureData newCulture = CreateCultureData(cultureInfo);

            if (useDataCache)
            {
                // Found one, add it to the cache
                lock (s_lock)
                {
                    tempHashTable![hashName!] = newCulture;
                }

                // Copy the hashtable to the corresponding member variables.  This will potentially overwrite
                // new tables simultaneously created by a new thread, but maximizes thread safety.
                s_cachedCultures = tempHashTable;
            }

            return newCulture;
        }

        private static UCultureData CreateCultureData(UCultureInfo cultureInfo)
        {
            return new UCultureData(cultureInfo, skipKeywords: false);
        }

        internal UCultureData(UCultureInfo cultureInfo, bool skipKeywords = false)
        {
            Debug.Assert(cultureInfo is not null);
            this.localeID = cultureInfo.localeID;
            this.isInvariantCulture = cultureInfo.isInvariantCulture;
            this.isNeutralCulture = cultureInfo.isNeutralCulture;
            this.name = cultureInfo.Name; // base name from ICU

            if (!skipKeywords)
            {
                cultureInfo.Keywords.TryGetValue("numbers", out numbersKeyword);
                cultureInfo.Keywords.TryGetValue("cf", out cfKeyword);
                cultureInfo.Keywords.TryGetValue("lb", out lbKeyword);
                cultureInfo.Keywords.TryGetValue("ss", out ssKeyword);
            }
        }

        private UCultureData()
        {
        }

        internal void GetNFIValues(UNumberFormatInfo nfi)
        {
            EnsureNumberFormatInfoInitialized();

            nfi.positiveSign = positiveSign;
            nfi.negativeSign = negativeSign;

            nfi.numberDecimalSeparator = decimalSeparator;
            nfi.numberGroupSeparator = groupSeparator;
            nfi.nanSymbol = naN;
            nfi.positiveInfinitySymbol = positiveInfinity;
            nfi.negativeInfinitySymbol = negativeInfinity;

            nfi.patternSeparator = patternSeparator[0];

            nfi.percentSymbol = percent;
            nfi.perMilleSymbol = perMille;

            nfi.exponentSeparator = exponentSeparator;
            nfi.exponentMultiplicationSign = exponentMultiplicationSign;

            nfi.currencySymbol = currency;
            nfi.currencyInternationalSymbol = intlMonetarySymbol;
            nfi.currencyDecimalSeparator = monetaryDecimal;
            nfi.currencyGroupSeparator = monetaryGroupSeparator;

            nfi.nativeDigits = ConvertDigits(nativeDigits);
            nfi.numberingSystemName = numberingSystemName;
            nfi.isAlgorithmic = isAlgorithmic;

            nfi.capitalizationForListOrMenu = capitalizationForListOrMenu;
            nfi.capitalizationForStandAlone = capitalizationForStandAlone;


            // ICU4N TODO: decimalFormat > numberGroupSizes
            // ICU4N TODO: currencyFormat > currencyGroupSizes
        }

        private static string[] ConvertDigits(string digitString)
        {
            string[] digits = new string[10];
            for (int i = 0, offset = 0; i < 10; i++)
            {
                int cp = digitString.CodePointAt(offset);
                int cpCharCount = Character.CharCount(cp);
                int nextOffset = offset + cpCharCount;
                digits[i] = cpCharCount == 1
                    ? char.ToString(digitString[offset])
                    : digitString.Substring(offset, nextOffset - offset); // ICU4N: Corrected 2nd parameter
                offset = nextOffset;
            }
            return digits;
        }

        internal static void LoadNumberFormatInfo(UCultureData cultureData)
        {
            string? nsName = null;
            // Attempt to set the decimal digits based on the numbering system for the requested locale.
            NumberingSystem ns = NumberingSystem.GetInstance(cultureData.name, cultureData.numbersKeyword);
            if (ns != null && ns.Radix == 10 && !ns.IsAlgorithmic &&
                NumberingSystem.IsValidDigitString(ns.Description))
            {
                nsName = ns.Name;
                cultureData.nativeDigits = ns.Description;
            }

            // Default numbering system
            cultureData.numberingSystemName = (nsName ??= Default.NumberingSystem);
            cultureData.nativeDigits ??= Default.NativeDigits;
            cultureData.isAlgorithmic = ns?.IsAlgorithmic ?? false;


            // Open the resource bundle and get the locale IDs.
            // TODO: Is there a better way to get the locale than making an ICUResourceBundle instance?
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, cultureData.name);
            CultureDataSink sink = new CultureDataSink();

            // *********** Symbol Data ************

            NumberElementSymbolLoader symbolLoader = new NumberElementSymbolLoader(cultureData);
            LoadDataTable(nsName, rb, string.Concat(NumberElements, nsName, Symbols), LatinSymbols, sink, symbolLoader);

            // *********** Pattern Data ************

            NumberElementPatternLoader patternLoader = new NumberElementPatternLoader(cultureData);
            LoadDataTable(nsName, rb, string.Concat(NumberElements, nsName, Patterns), LatinPatterns, sink, patternLoader);

            // *********** Capitalization Data ************

            try
            {
                ICUResourceBundle rdb = rb.GetWithFallback(CapitalizationSettings);
                int[] intVector = rdb.GetInt32Vector();
                if (intVector.Length >= 2)
                {
                    cultureData.capitalizationForListOrMenu = intVector[0] != 0;
                    cultureData.capitalizationForStandAlone = intVector[1] != 0;
                }
            }
            catch (MissingManifestResourceException)
            {
                // use default
            }

        }

        private static void LoadDataTable(string nsName, ICUResourceBundle rb, string path, string fallbackPath, CultureDataSink sink, IResourceTableLoader loader)
        {
            sink.SetLoader(loader);

            rb.GetAllItemsWithFallbackNoFail(path, sink);

            // Load the Latin fallback if necessary
            if (!loader.IsFullyPopulated && !LatinNumberingSystem.Equals(nsName, StringComparison.Ordinal))
            {
                rb.GetAllItemsWithFallbackNoFail(fallbackPath, sink);
            }

            // Fill in any remaining missing values
            loader.PostProcess();
        }

        private const string LatinNumberingSystem = "latn";
        private const string NumberElements = "NumberElements/";
        private const string Symbols = "/symbols";
        private const string Patterns = "/patterns";

        private const string LatinSymbols = NumberElements + LatinNumberingSystem + Symbols;
        private const string LatinPatterns = NumberElements + LatinNumberingSystem + Patterns;

        private const string CapitalizationSettings = "contextTransforms/number-spellout";

        private struct Default
        {
            // Symbol data

            public const string NumberingSystem = LatinNumberingSystem;
            public const string NativeDigits = "0123456789";

            public const string DecimalSeparator = ".";
            public const string GroupSeparator = ",";
            public const string PatternSeparator = ","; // ICU4N TODO: The resource name is "list". Is this the same thing as _sListSeparator in .NET?
            public const string Percent = "%";
            public const string NegativeSign = "-";
            public const string PositiveSign = "+";
            public const string ExponentSeparator = "E";
            public const string PerMille = "\x2030"; // PerMille symbol
            public const string PositiveInfinity = "∞"; // ICU4N: This differs from .NET //"Infinity";
            public static string GetNegativeInfinity(string negativeSign, string positiveInfinity) => negativeSign + positiveInfinity;

            public const string NaN = "NaN";

            public const string ExponentMultiplicationSign = "\u00D7"; // superscripting exponent

            // Pattern data
            public const string AccountingFormat = "¤ #,##0.00";
            public const string CurrencyFormat = "¤ #,##0.00";
            public const string DecimalFormat = "#,##0.###";
            public const string PercentFormat = "#,##0%";
            public const string ScientificFormat = "#E0";
        }


        private sealed class NumberElementSymbolLoader : IResourceTableLoader
        {
            private readonly UCultureData cultureData;

            public NumberElementSymbolLoader(UCultureData cultureData)
            {
                Debug.Assert(cultureData is not null);
                this.cultureData = cultureData;
            }

            public bool IsFullyPopulated => cultureData.decimalSeparator is not null &&
                cultureData.groupSeparator is not null &&
                cultureData.patternSeparator is not null &&
                cultureData.percent is not null &&
                cultureData.negativeSign is not null &&
                cultureData.positiveSign is not null &&
                cultureData.exponentSeparator is not null &&
                cultureData.perMille is not null &&
                cultureData.positiveInfinity is not null &&
                //cultureData.negativeInfinity is not null && // This is not in the resource data - we must call FillInDefaults() to fix this
                cultureData.naN is not null &&
                cultureData.monetaryDecimal is not null &&
                cultureData.monetaryGroupSeparator is not null &&
                cultureData.exponentMultiplicationSign is not null;

            public void PostProcess()
            {
                cultureData.decimalSeparator ??= Default.DecimalSeparator;
                cultureData.groupSeparator ??= Default.GroupSeparator;
                cultureData.patternSeparator ??= Default.PatternSeparator;
                cultureData.percent ??= Default.Percent;
                cultureData.negativeSign ??= Default.NegativeSign;
                cultureData.positiveSign ??= Default.PositiveSign;
                cultureData.exponentSeparator ??= Default.ExponentSeparator;
                cultureData.perMille ??= Default.PerMille;
                cultureData.positiveInfinity ??= Default.PositiveInfinity;
                cultureData.negativeInfinity ??= Default.GetNegativeInfinity(cultureData.negativeSign, cultureData.positiveInfinity); // Not in CLDR
                cultureData.naN ??= Default.NaN;
                cultureData.monetaryDecimal ??= cultureData.decimalSeparator ?? Default.DecimalSeparator;
                cultureData.monetaryGroupSeparator ??= cultureData.groupSeparator ?? Default.GroupSeparator;
                cultureData.exponentMultiplicationSign ??= Default.ExponentMultiplicationSign;
            }

            public void Put(int index, IResourceTable table, ResourceKey key, ResourceValue value, bool noFallback)
            {
                switch (key.ToString()) // ICU4N: This is the same list as DecimalFormatSymbols.SYMBOL_KEYS
                {
                    case "decimal":
                        cultureData.decimalSeparator ??= value.ToString();
                        break;
                    case "group":
                        cultureData.groupSeparator ??= value.ToString();
                        break;
                    case "list":
                        cultureData.patternSeparator ??= value.ToString();
                        break;
                    case "percentSign":
                        cultureData.percent ??= value.ToString();
                        break;
                    case "minusSign":
                        cultureData.negativeSign ??= value.ToString();
                        break;
                    case "plusSign":
                        cultureData.positiveSign ??= value.ToString();
                        break;
                    case "exponential":
                        cultureData.exponentSeparator ??= value.ToString();
                        break;
                    case "perMille":
                        cultureData.perMille ??= value.ToString();
                        break;
                    case "infinity":
                        cultureData.positiveInfinity ??= value.ToString();
                        break;
                    case "nan":
                        cultureData.naN ??= value.ToString();
                        break;
                    case "currencyDecimal":
                        cultureData.monetaryDecimal ??= value.ToString();
                        break;
                    case "currencyGroup":
                        cultureData.monetaryGroupSeparator ??= value.ToString();
                        break;
                    case "superscriptingExponent":
                        cultureData.exponentMultiplicationSign ??= value.ToString();
                        break;
                }
            }
        }

        private sealed class NumberElementPatternLoader : IResourceTableLoader
        {
            private readonly UCultureData cultureData;

            public NumberElementPatternLoader(UCultureData cultureData)
            {
                Debug.Assert(cultureData is not null);
                this.cultureData = cultureData;
            }

            public bool IsFullyPopulated => cultureData.decimalFormat is not null &&
                cultureData.currencyFormat is not null &&
                cultureData.accountingFormat is not null &&
                cultureData.percentFormat is not null &&
                cultureData.scientificFormat is not null;

            public void PostProcess()
            {
                // Note: These are the same as latn defaults. Do we need this?
                cultureData.decimalFormat ??= Default.DecimalFormat;
                cultureData.currencyFormat ??= Default.CurrencyFormat;
                cultureData.accountingFormat ??= Default.AccountingFormat;
                cultureData.percentFormat ??= Default.PercentFormat;
                cultureData.scientificFormat ??= Default.ScientificFormat;
            }

            public void Put(int index, IResourceTable table, ResourceKey key, ResourceValue value, bool noFallback)
            {
                switch (key.ToString()) // ICU4N: This is the same list as in NumberFormat.GetPatternForStyleAndNumberingSystem()
                {
                    case "decimalFormat":
                        cultureData.decimalFormat ??= value.ToString();
                        break;
                    case "currencyFormat":
                        cultureData.currencyFormat ??= value.ToString();
                        break;
                    case "accountingFormat":
                        cultureData.accountingFormat ??= value.ToString();
                        break;
                    case "percentFormat":
                        cultureData.percentFormat ??= value.ToString();
                        break;
                    case "scientificFormat":
                        cultureData.scientificFormat ??= value.ToString();
                        break;
                }
            }
        }

        internal interface IResourceTableLoader
        {
            bool IsFullyPopulated { get; }

            void Put(int index, IResourceTable table, ResourceKey key, ResourceValue value, bool noFallback);

            void PostProcess();
        }

        internal sealed class CultureDataSink : ResourceSink
        {
            private IResourceTableLoader loader;

            [MemberNotNull(nameof(loader))]
            public void SetLoader(IResourceTableLoader loader)
            {
                Debug.Assert(loader is not null);
                this.loader = loader;
            }

            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                Debug.Assert(loader is not null);
                IResourceTable symbolsTable = value.GetTable();
                for (int j = 0; symbolsTable.GetKeyAndValue(j, key, value); ++j)
                {
                    loader.Put(j, symbolsTable, key, value, noFallback);
                }
            }
        }

    }
}
