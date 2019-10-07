using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace ICU4N.Impl
{
    public class ICUCurrencyDisplayInfoProvider : ICurrencyDisplayInfoProvider
    {
        public ICUCurrencyDisplayInfoProvider()
        {
        }

        /// <summary>
        /// Single-item cache for <see cref="ICUCurrencyDisplayInfo"/> keyed by locale.
        /// </summary>
        private volatile ICUCurrencyDisplayInfo currencyDisplayInfoCache = null;

        public virtual CurrencyDisplayInfo GetInstance(ULocale locale, bool withFallback)
        {
            // Make sure the locale is non-null (this can happen during deserialization):
            if (locale == null) { locale = ULocale.ROOT; }
            ICUCurrencyDisplayInfo instance = currencyDisplayInfoCache;
            if (instance == null || !instance.locale.Equals(locale) || instance.fallback != withFallback)
            {
                ICUResourceBundle rb;
                if (withFallback)
                {
                    rb = ICUResourceBundle.GetBundleInstance(
                            ICUData.IcuCurrencyBaseName, locale, typeof(ICUCurrencyDisplayInfoProvider).GetTypeInfo().Assembly, OpenType.LocaleDefaultRoot);
                }
                else
                {
                    try
                    {
                        rb = ICUResourceBundle.GetBundleInstance(
                                ICUData.IcuCurrencyBaseName, locale, typeof(ICUCurrencyDisplayInfoProvider).GetTypeInfo().Assembly, OpenType.LocaleOnly);
                    }
                    catch (MissingManifestResourceException)
                    {
                        return null;
                    }
                }
                instance = new ICUCurrencyDisplayInfo(locale, rb, withFallback);
                currencyDisplayInfoCache = instance;
            }
            return instance;
        }

        public virtual bool HasData
        {
            get { return true; }
        }

        /// <summary>
        /// This class performs data loading for currencies and keeps data in lightweight cache.
        /// </summary>
        internal class ICUCurrencyDisplayInfo : CurrencyDisplayInfo
        {
            internal readonly ULocale locale;
            internal readonly bool fallback;
            private readonly ICUResourceBundle rb;

            /// <summary>
            /// Single-item cache for <see cref="GetName(string)"/>, <see cref="GetSymbol(string)"/>, 
            /// and <see cref="GetFormatInfo(string)"/>. Holds data for only one currency. 
            /// If another currency is requested, the old cache item is overwritten.
            /// </summary>
            private volatile FormattingData formattingDataCache = null;

            /// <summary>
            /// Single-item cache for <see cref="GetNarrowSymbol(string)"/>.
            /// Holds data for only one currency. If another currency is requested, the old cache item is overwritten.
            /// </summary>
            private volatile NarrowSymbol narrowSymbolCache = null;

            /// <summary>
            /// Single-item cache for <see cref="GetPluralName(string, string)"/>.
            /// </summary>
            /// <remarks>
            /// array[0] is the ISO code.
            /// array[1+p] is the plural name where p=(int)standardPlural
            /// <para/>
            /// Holds data for only one currency. If another currency is requested, the old cache item is overwritten.
            /// </remarks>
            private volatile String[] pluralsDataCache = null;

            /// <summary>
            /// Cache for <see cref="SymbolMap"/> and <see cref="NameMap"/>.
            /// </summary>
            private volatile SoftReference<ParsingData> parsingDataCache = new SoftReference<ParsingData>(null);

            /// <summary>
            /// Cache for <see cref="GetUnitPatterns()"/>.
            /// </summary>
            private volatile IDictionary<string, string> unitPatternsCache = null;

            /// <summary>
            /// Cache for <see cref="GetSpacingInfo()"/>.
            /// </summary>
            private volatile CurrencySpacingInfo spacingInfoCache = null;

            internal class FormattingData
            {
                internal readonly string isoCode;
                internal string displayName = null;
                internal string symbol = null;
                internal CurrencyFormatInfo formatInfo = null;

                internal FormattingData(string isoCode) { this.isoCode = isoCode; }
            }

            internal class NarrowSymbol
            {
                internal readonly string isoCode;
                internal string narrowSymbol = null;

                internal NarrowSymbol(string isoCode) { this.isoCode = isoCode; }
            }

            internal class ParsingData
            {
                internal IDictionary<string, string> symbolToIsoCode = new Dictionary<string, string>();
                internal IDictionary<string, string> nameToIsoCode = new Dictionary<string, string>();
            }

            ////////////////////////
            /// START PUBLIC API ///
            ////////////////////////

            public ICUCurrencyDisplayInfo(ULocale locale, ICUResourceBundle rb, bool fallback)
            {
                this.locale = locale;
                this.fallback = fallback;
                this.rb = rb;
            }

            public override ULocale ULocale
            {
                get { return rb.GetULocale(); }
            }

            public override string GetName(string isoCode)
            {
                FormattingData formattingData = FetchFormattingData(isoCode);

                // Fall back to ISO Code
                if (formattingData.displayName == null && fallback)
                {
                    return isoCode;
                }
                return formattingData.displayName;
            }

            public override string GetSymbol(string isoCode)
            {
                FormattingData formattingData = FetchFormattingData(isoCode);

                // Fall back to ISO Code
                if (formattingData.symbol == null && fallback)
                {
                    return isoCode;
                }
                return formattingData.symbol;
            }

            public override string GetNarrowSymbol(string isoCode)
            {
                NarrowSymbol narrowSymbol = FetchNarrowSymbol(isoCode);

                // Fall back to ISO Code
                // TODO: Should this fall back to the regular symbol instead of the ISO code?
                if (narrowSymbol.narrowSymbol == null && fallback)
                {
                    return isoCode;
                }
                return narrowSymbol.narrowSymbol;
            }

            public override string GetPluralName(string isoCode, string pluralKey)
            {
                StandardPlural? plural = StandardPluralUtil.OrNullFromString(pluralKey);
                string[] pluralsData = fetchPluralsData(isoCode);

                // See http://unicode.org/reports/tr35/#Currencies, especially the fallback rule.
                string result = null;
                if (plural != null)
                {
                    result = pluralsData[1 + (int)plural];
                }
                if (result == null && fallback)
                {
                    // First fall back to the "other" plural variant
                    // Note: If plural is already "other", this fallback is benign
                    result = pluralsData[1 + (int)StandardPlural.Other];
                }
                if (result == null && fallback)
                {
                    // If that fails, fall back to the display name
                    FormattingData formattingData = FetchFormattingData(isoCode);
                    result = formattingData.displayName;
                }
                if (result == null && fallback)
                {
                    // If all else fails, return the ISO code
                    result = isoCode;
                }
                return result;
            }

            public override IDictionary<string, string> SymbolMap
            {
                get
                {
                    ParsingData parsingData = FetchParsingData();
                    return parsingData.symbolToIsoCode;
                }
            }

            public override IDictionary<string, string> NameMap
            {
                get
                {
                    ParsingData parsingData = FetchParsingData();
                    return parsingData.nameToIsoCode;
                }
            }

            public override IDictionary<string, string> GetUnitPatterns()
            {
                // Default result is the empty map. Callers who require a pattern will have to
                // supply a default.
                IDictionary<string, string> unitPatterns = FetchUnitPatterns();
                return unitPatterns;
            }

            public override CurrencyFormatInfo GetFormatInfo(string isoCode)
            {
                FormattingData formattingData = FetchFormattingData(isoCode);
                return formattingData.formatInfo;
            }

            public override CurrencySpacingInfo GetSpacingInfo()
            {
                CurrencySpacingInfo spacingInfo = FetchSpacingInfo();

                // Fall back to DEFAULT
                if ((!spacingInfo.hasBeforeCurrency || !spacingInfo.hasAfterCurrency) && fallback)
                {
                    return CurrencySpacingInfo.DEFAULT;
                }
                return spacingInfo;
            }

            /////////////////////////////////////////////
            /// END PUBLIC API -- START DATA FRONTEND ///
            /////////////////////////////////////////////

            internal FormattingData FetchFormattingData(string isoCode)
            {
                FormattingData result = formattingDataCache;
                if (result == null || !result.isoCode.Equals(isoCode))
                {
                    result = new FormattingData(isoCode);
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.CURRENCIES);
                    sink.formattingData = result;
                    rb.GetAllItemsWithFallbackNoFail("Currencies/" + isoCode, sink);
                    formattingDataCache = result;
                }
                return result;
            }

            internal NarrowSymbol FetchNarrowSymbol(string isoCode)
            {
                NarrowSymbol result = narrowSymbolCache;
                if (result == null || !result.isoCode.Equals(isoCode))
                {
                    result = new NarrowSymbol(isoCode);
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.CURRENCY_NARROW);
                    sink.narrowSymbol = result;
                    rb.GetAllItemsWithFallbackNoFail("Currencies%narrow/" + isoCode, sink);
                    narrowSymbolCache = result;
                }
                return result;
            }

            internal string[] fetchPluralsData(string isoCode)
            {
                string[] result = pluralsDataCache;
                if (result == null || !result[0].Equals(isoCode))
                {
                    result = new string[1 + StandardPluralUtil.Count];
                    result[0] = isoCode;
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.CURRENCY_PLURALS);
                    sink.pluralsData = result;
                    rb.GetAllItemsWithFallbackNoFail("CurrencyPlurals/" + isoCode, sink);
                    pluralsDataCache = result;
                }
                return result;
            }

            internal ParsingData FetchParsingData()
            {
                ParsingData result = parsingDataCache.Get();
                if (result == null)
                {
                    result = new ParsingData();
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.TOP);
                    sink.parsingData = result;
                    rb.GetAllItemsWithFallback("", sink);
                    parsingDataCache = new SoftReference<ParsingData>(result);
                }
                return result;
            }

            internal IDictionary<string, string> FetchUnitPatterns()
            {
                IDictionary<string, string> result = unitPatternsCache;
                if (result == null)
                {
                    result = new Dictionary<string, string>();
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.CURRENCY_UNIT_PATTERNS);
                    sink.unitPatterns = result;
                    rb.GetAllItemsWithFallback("CurrencyUnitPatterns", sink);
                    unitPatternsCache = result;
                }
                return result;
            }

            internal CurrencySpacingInfo FetchSpacingInfo()
            {
                CurrencySpacingInfo result = spacingInfoCache;
                if (result == null)
                {
                    result = new CurrencySpacingInfo();
                    CurrencySink sink = new CurrencySink(!fallback, CurrencySink.EntrypointTable.CURRENCY_SPACING);
                    sink.spacingInfo = result;
                    rb.GetAllItemsWithFallback("currencySpacing", sink);
                    spacingInfoCache = result;
                }
                return result;
            }

            ////////////////////////////////////////////
            /// END DATA FRONTEND -- START DATA SINK ///
            ////////////////////////////////////////////

            private sealed class CurrencySink : UResource.Sink
            {
                internal readonly bool noRoot;
                internal readonly EntrypointTable entrypointTable;

                // The fields to be populated on this run of the data sink will be non-null.
                internal FormattingData formattingData = null;
                internal string[] pluralsData = null;
                internal ParsingData parsingData = null;
                internal IDictionary<string, string> unitPatterns = null;
                internal CurrencySpacingInfo spacingInfo = null;
                internal NarrowSymbol narrowSymbol = null;

                internal enum EntrypointTable
                {
                    // For Parsing:
                    TOP,

                    // For Formatting:
                    CURRENCIES,
                    CURRENCY_PLURALS,
                    CURRENCY_NARROW,
                    CURRENCY_SPACING,
                    CURRENCY_UNIT_PATTERNS
                }

                internal CurrencySink(bool noRoot, EntrypointTable entrypointTable)
                {
                    this.noRoot = noRoot;
                    this.entrypointTable = entrypointTable;
                }

                /// <summary>
                /// The entrypoint method delegates to helper methods for each of the types of tables
                /// found in the currency data.
                /// </summary>
                public override void Put(UResource.Key key, UResource.Value value, bool isRoot)
                {
                    if (noRoot && isRoot)
                    {
                        // Don't consume the root bundle
                        return;
                    }

                    switch (entrypointTable)
                    {
                        case EntrypointTable.TOP:
                            ConsumeTopTable(key, value);
                            break;
                        case EntrypointTable.CURRENCIES:
                            ConsumeCurrenciesEntry(key, value);
                            break;
                        case EntrypointTable.CURRENCY_PLURALS:
                            ConsumeCurrencyPluralsEntry(key, value);
                            break;
                        case EntrypointTable.CURRENCY_NARROW:
                            ConsumeCurrenciesNarrowEntry(key, value);
                            break;
                        case EntrypointTable.CURRENCY_SPACING:
                            ConsumeCurrencySpacingTable(key, value);
                            break;
                        case EntrypointTable.CURRENCY_UNIT_PATTERNS:
                            ConsumeCurrencyUnitPatternsTable(key, value);
                            break;
                    }
                }

                private void ConsumeTopTable(UResource.Key key, UResource.Value value)
                {
                    UResource.ITable table = value.GetTable();
                    for (int i = 0; table.GetKeyAndValue(i, key, value); i++)
                    {
                        if (key.ContentEquals("Currencies"))
                        {
                            ConsumeCurrenciesTable(key, value);
                        }
                        else if (key.ContentEquals("Currencies%variant"))
                        {
                            ConsumeCurrenciesVariantTable(key, value);
                        }
                        else if (key.ContentEquals("CurrencyPlurals"))
                        {
                            ConsumeCurrencyPluralsTable(key, value);
                        }
                    }
                }

                /// <summary>
                ///  Currencies{
                ///      ...
                ///      USD{
                ///          "US$",        => symbol
                ///          "US Dollar",  => display name
                ///      }
                ///      ...
                ///      ESP{
                ///          "₧",                  => symbol
                ///          "pesseta espanyola",  => display name
                ///          {
                ///              "¤ #,##0.00",     => currency-specific pattern
                ///              ",",              => currency-specific grouping separator
                ///              ".",              => currency-specific decimal separator
                ///          }
                ///      }
                ///      ...
                ///  }
                /// </summary>
                internal void ConsumeCurrenciesTable(UResource.Key key, UResource.Value value)
                {
                    // The full Currencies table is consumed for parsing only.
                    Debug.Assert(parsingData != null);
                    UResource.ITable table = value.GetTable();
                    for (int i = 0; table.GetKeyAndValue(i, key, value); i++)
                    {
                        string isoCode = key.ToString();
                        if (value.Type != UResourceType.Array)
                        {
                            throw new ICUException("Unexpected data type in Currencies table for " + isoCode);
                        }
                        UResource.IArray array = value.GetArray();

                        parsingData.symbolToIsoCode[isoCode] = isoCode; // Add the ISO code itself as a symbol
                        array.GetValue(0, value);
                        parsingData.symbolToIsoCode[value.GetString()] = isoCode;
                        array.GetValue(1, value);
                        parsingData.nameToIsoCode[value.GetString()] = isoCode;
                    }
                }

                internal void ConsumeCurrenciesEntry(UResource.Key key, UResource.Value value)
                {
                    Debug.Assert(formattingData != null);
                    string isoCode = key.ToString();
                    if (value.Type != UResourceType.Array)
                    {
                        throw new ICUException("Unexpected data type in Currencies table for " + isoCode);
                    }
                    UResource.IArray array = value.GetArray();

                    if (formattingData.symbol == null)
                    {
                        array.GetValue(0, value);
                        formattingData.symbol = value.GetString();
                    }
                    if (formattingData.displayName == null)
                    {
                        array.GetValue(1, value);
                        formattingData.displayName = value.GetString();
                    }

                    // If present, the third element is the currency format info.
                    // TODO: Write unit test to ensure that this data is being used by number formatting.
                    if (array.Length > 2 && formattingData.formatInfo == null)
                    {
                        array.GetValue(2, value);
                        UResource.IArray formatArray = value.GetArray();
                        formatArray.GetValue(0, value);
                        string formatPattern = value.GetString();
                        formatArray.GetValue(1, value);
                        string decimalSeparator = value.GetString();
                        formatArray.GetValue(2, value);
                        string groupingSeparator = value.GetString();
                        formattingData.formatInfo = new CurrencyFormatInfo(
                                isoCode, formatPattern, decimalSeparator, groupingSeparator);
                    }
                }

                /// <summary>
                ///  Currencies%narrow{
                ///      AOA{"Kz"}
                ///      ARS{"$"}
                ///      ...
                ///  }
                /// </summary>
                internal void ConsumeCurrenciesNarrowEntry(UResource.Key key, UResource.Value value)
                {
                    Debug.Assert(narrowSymbol != null);
                    // No extra structure to traverse.
                    if (narrowSymbol.narrowSymbol == null)
                    {
                        narrowSymbol.narrowSymbol = value.GetString();
                    }
                }

                /// <summary>
                ///  Currencies%variant{
                ///      TRY{"TL"}
                ///  }
                /// </summary>
                internal void ConsumeCurrenciesVariantTable(UResource.Key key, UResource.Value value)
                {
                    // Note: This data is used for parsing but not formatting.
                    Debug.Assert(parsingData != null);
                    UResource.ITable table = value.GetTable();
                    for (int i = 0; table.GetKeyAndValue(i, key, value); i++)
                    {
                        string isoCode = key.ToString();
                        parsingData.symbolToIsoCode[value.GetString()] = isoCode;
                    }
                }

                /// <summary>
                ///  CurrencyPlurals{
                ///      BYB{
                ///          one{"Belarusian new rouble (1994–1999)"}
                ///          other{"Belarusian new roubles (1994–1999)"}
                ///      }
                ///      ...
                ///  }
                /// </summary>
                internal void ConsumeCurrencyPluralsTable(UResource.Key key, UResource.Value value)
                {
                    // The full CurrencyPlurals table is consumed for parsing only.
                    Debug.Assert(parsingData != null);
                    UResource.ITable table = value.GetTable();
                    for (int i = 0; table.GetKeyAndValue(i, key, value); i++)
                    {
                        string isoCode = key.ToString();
                        UResource.ITable pluralsTable = value.GetTable();
                        for (int j = 0; pluralsTable.GetKeyAndValue(j, key, value); j++)
                        {
                            StandardPlural? plural = StandardPluralUtil.OrNullFromString(key.ToString());
                            if (plural == null)
                            {
                                throw new ICUException("Could not make StandardPlural from keyword " + key);
                            }

                            parsingData.nameToIsoCode[value.GetString()] = isoCode;
                        }
                    }
                }

                internal void ConsumeCurrencyPluralsEntry(UResource.Key key, UResource.Value value)
                {
                    Debug.Assert(pluralsData != null);
                    UResource.ITable pluralsTable = value.GetTable();
                    for (int j = 0; pluralsTable.GetKeyAndValue(j, key, value); j++)
                    {
                        StandardPlural? plural = StandardPluralUtil.OrNullFromString(key.ToString());
                        if (plural == null)
                        {
                            throw new ICUException("Could not make StandardPlural from keyword " + key);
                        }

                        if (pluralsData[1 + (int)plural] == null)
                        {
                            pluralsData[1 + (int)plural] = value.GetString();
                        }
                    }
                }

                /// <summary>
                ///  currencySpacing{
                ///      afterCurrency{
                ///          currencyMatch{"[:^S:]"}
                ///          insertBetween{" "}
                ///          surroundingMatch{"[:digit:]"}
                ///      }
                ///      beforeCurrency{
                ///          currencyMatch{"[:^S:]"}
                ///          insertBetween{" "}
                ///          surroundingMatch{"[:digit:]"}
                ///      }
                ///  }
                /// </summary>
                internal void ConsumeCurrencySpacingTable(UResource.Key key, UResource.Value value)
                {
                    Debug.Assert(spacingInfo != null);
                    UResource.ITable spacingTypesTable = value.GetTable();
                    for (int i = 0; spacingTypesTable.GetKeyAndValue(i, key, value); ++i)
                    {
                        CurrencySpacingInfo.SpacingType type;
                        if (key.ContentEquals("beforeCurrency"))
                        {
                            type = CurrencySpacingInfo.SpacingType.Before;
                            spacingInfo.hasBeforeCurrency = true;
                        }
                        else if (key.ContentEquals("afterCurrency"))
                        {
                            type = CurrencySpacingInfo.SpacingType.After;
                            spacingInfo.hasAfterCurrency = true;
                        }
                        else
                        {
                            continue;
                        }

                        UResource.ITable patternsTable = value.GetTable();
                        for (int j = 0; patternsTable.GetKeyAndValue(j, key, value); ++j)
                        {
                            CurrencySpacingInfo.SpacingPattern pattern;
                            if (key.ContentEquals("currencyMatch"))
                            {
                                pattern = CurrencySpacingInfo.SpacingPattern.CurrencyMatch;
                            }
                            else if (key.ContentEquals("surroundingMatch"))
                            {
                                pattern = CurrencySpacingInfo.SpacingPattern.SurroundingMatch;
                            }
                            else if (key.ContentEquals("insertBetween"))
                            {
                                pattern = CurrencySpacingInfo.SpacingPattern.InsertBetween;
                            }
                            else
                            {
                                continue;
                            }

                            spacingInfo.SetSymbolIfNull(type, pattern, value.GetString());
                        }
                    }
                }

                /// <summary>
                ///  CurrencyUnitPatterns{
                ///      other{"{0} {1}"}
                ///      ...
                ///  }
                /// </summary>
                internal void ConsumeCurrencyUnitPatternsTable(UResource.Key key, UResource.Value value)
                {
                    Debug.Assert(unitPatterns != null);
                    UResource.ITable table = value.GetTable();
                    for (int i = 0; table.GetKeyAndValue(i, key, value); i++)
                    {
                        String pluralKeyword = key.ToString();
                        if (unitPatterns.Get(pluralKeyword) == null)
                        {
                            unitPatterns[pluralKeyword] = value.GetString();
                        }
                    }
                }
            }
        }
    }
}
