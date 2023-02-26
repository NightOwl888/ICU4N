using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using J2N.Collections.Generic.Extensions;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
using Microsoft.Extensions.Caching.Memory;
#else
using System.Runtime.Caching;
#endif
using JCG = J2N.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// A class encapsulating a currency, as defined by ISO 4217.  A
    /// <see cref="Currency"/> object can be created given a <see cref="CultureInfo"/> or
    /// given an ISO 4217 code.  Once created, the <see cref="Currency"/> object
    /// can return various data necessary to its proper display:
    /// 
    /// <list type="bullet">
    ///     <item><description>A display symbol, for a specific locale.</description></item>
    ///     <item><description>The number of fraction digits to display.</description></item>
    ///     <item><description>A rounding increment.</description></item>
    /// </list>
    /// <para/>
    /// The <see cref="DecimalFormat"/> class uses these data to display
    /// currencies.
    /// <para/>
    /// Note: This class deliberately resembles <c>java.util.Currency</c>
    /// but it has a completely independent
    /// implementation, and adds features not present in the JDK.
    /// </summary>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.2</stable>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class Currency : MeasureUnit // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final long serialVersionUID = -5839973855554750484L;
        private static readonly bool DEBUG = ICUDebug.Enabled("currency");

        private static readonly TimeSpan SlidingExpiration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);

        // Cache to save currency name trie
#pragma warning disable CS0618 // Type or member is obsolete
        private static IICUCache<UCultureInfo, IList<TextTrieMap<CurrencyStringInfo>>> CURRENCY_NAME_CACHE =
#pragma warning restore CS0618 // Type or member is obsolete
        new SimpleCache<UCultureInfo, IList<TextTrieMap<CurrencyStringInfo>>>();

        /// <summary>
        /// Selector for <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating a symbolic name for a
        /// currency, such as "$" for USD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const CurrencyNameStyle SymbolName = CurrencyNameStyle.SymbolName;

        /// <summary>
        /// Selector for <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the long name for a
        /// currency, such as "US Dollar" for USD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const CurrencyNameStyle LongName = CurrencyNameStyle.LongName;

        /// <summary>
        /// Selector for <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the plural long name for a
        /// currency, such as "US dollar" for USD in "1 US dollar",
        /// and "US dollars" for USD in "2 US dollars".
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public const CurrencyNameStyle PluralLongName = CurrencyNameStyle.PluralLongName;

        /// <summary>
        /// Selector for <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the narrow currency symbol.
        /// The narrow currency symbol is similar to the regular currency
        /// symbol, but it always takes the shortest form: for example,
        /// "$" instead of "US$".
        /// <para/>
        /// This method assumes that the currency data provider is the ICU4N
        /// built-in data provider. If it is not, an exception is thrown.
        /// </summary>
        /// <internal/>
        [Obsolete("ICU 60: This API is ICU internal only.")]
        public const CurrencyNameStyle NarrowSymbolName = CurrencyNameStyle.NarrowSymbolName;

        private static readonly EquivalenceRelation<string> EQUIVALENT_CURRENCY_SYMBOLS =
                new EquivalenceRelation<string>()
                .Add("\u00a5", "\uffe5")
                .Add("$", "\ufe69", "\uff04")
                .Add("\u20a8", "\u20b9")
                .Add("\u00a3", "\u20a4");

        // ICU4N specific: de-nested CurrencyUsage enum.



        // begin registry stuff

        // shim for service code
        /* package */
        internal abstract class ServiceShim
        {
            internal abstract UCultureInfo[] GetUCultures(UCultureTypes types); // ICU4N: Renamed from GetAvailableULocales
            internal abstract CultureInfo[] GetCultures(UCultureTypes types);
            internal abstract Currency CreateInstance(UCultureInfo l);
            internal abstract object RegisterInstance(Currency c, UCultureInfo l);
            internal abstract bool Unregister(object f);
        }

        private static ServiceShim shim;
        private static ServiceShim GetShim()
        {
            // Note: this instantiation is safe on loose-memory-model configurations
            // despite lack of synchronization, since the shim instance has no state--
            // it's all in the class init.  The worst problem is we might instantiate
            // two shim instances, but they'll share the same state so that's ok.
            if (shim == null)
            {
                try
                {
                    //Class <?> cls = Class.forName("com.ibm.icu.util.CurrencyServiceShim");
                    //shim = (ServiceShim)cls.newInstance();
                    Type cls = System.Type.GetType("ICU4N.Util.CurrencyServiceShim, ICU4N"); // ICU4N TODO: API Set statically on Currency class so it can be injected (this won't allow external injection)
                    shim = (CurrencyServiceShim)Activator.CreateInstance(cls);
                }
                catch (Exception e)
                {
                    if (DEBUG)
                    {
                        e.PrintStackTrace();
                    }
                    throw;
                    //throw new RuntimeException(e.getMessage());
                }
            }
            return shim;
        }

        /// <summary>
        /// Returns a currency object for the default currency in the given
        /// locale.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns>The currency object for this locale.</returns>
        /// <stable>ICU 2.2</stable>
        public static Currency GetInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo());
        }

        /// <summary>
        /// Returns a currency object for the default currency in the given
        /// locale.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public static Currency GetInstance(UCultureInfo locale)
        {
            locale ??= UCultureInfo.CurrentCulture; // ICU4N: Enusure the culture is set.

            // ICU4N: We should be running new LocaleIDParser("zh_Hans_CN").GetKeywordValue("currency").
            // However, the UCultureInfo should be doing this already so we just need to check whether it exists in the Keywords property.
            //string currency = locale.GetDisplayKeywordValue("currency");
            //if (currency != null)
            if (locale.Keywords.TryGetValue("currency", out string currency) && currency != null)
            {
                return GetInstance(currency);
            }

            if (shim == null)
            {
                return CreateCurrency(locale);
            }

            return shim.CreateInstance(locale);
        }

        /// <summary>
        /// Returns an array of <see cref="string"/>s which contain the currency
        /// identifiers that are valid for the given locale on the
        /// given date.  If there are no such identifiers, returns <c>null</c>.
        /// Returned identifiers are in preference order.
        /// </summary>
        /// <param name="loc">The locale for which to retrieve currency codes.</param>
        /// <param name="d">The date for which to retrieve currency codes for the given locale.</param>
        /// <returns>The array of ISO currency codes.</returns>
        /// <stable>ICU 4.0</stable>
        public static string[] GetAvailableCurrencyCodes(UCultureInfo loc, DateTime d)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            string region = UCultureInfo.GetRegionForSupplementalData(loc, false);
#pragma warning restore CS0618 // Type or member is obsolete
            CurrencyFilter filter = CurrencyFilter.OnDate(d).WithRegion(region);
            IList<string> list = GetTenderCurrencies(filter);
            // Note: Prior to 4.4 the spec didn't say that we return null if there are no results, but
            // the test assumed it did.  Kept the behavior and amended the spec.
            if (list.Count == 0)
            {
                return null;
            }
            return list.ToArray();
        }

        /// <summary>
        /// Returns an array of <see cref="string"/>s which contain the currency
        /// identifiers that are valid for the given <see cref="CultureInfo"/> on the
        /// given date.  If there are no such identifiers, returns <c>null</c>.
        /// Returned identifiers are in preference order.
        /// </summary>
        /// <param name="loc">The <see cref="CultureInfo"/> for which to retrieve currency codes.</param>
        /// <param name="d">The date for which to retrieve currency codes for the given locale.</param>
        /// <returns>The array of ISO currency codes.</returns>
        /// <stable>ICU 54</stable>
        public static string[] GetAvailableCurrencyCodes(CultureInfo loc, DateTime d) // ICU4N TODO: API - name to match GetCultures()?
        {
            return GetAvailableCurrencyCodes(loc.ToUCultureInfo(), d);
        }

        /// <summary>
        /// Returns the set of available currencies. The returned set of currencies contains all of the
        /// available currencies, including obsolete ones. The result set can be modified without
        /// affecting the available currencies in the runtime.
        /// </summary>
        /// <returns>
        /// The set of available currencies. The returned set could be empty if there is no
        /// currency data available.
        /// </returns>
        /// <stable>ICU 49</stable>
        public static ISet<Currency> GetAvailableCurrencies() // ICU4N TODO: API - name to match GetCultures()?
        {
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
            IList<string> list = info.Currencies(CurrencyFilter.All);
            JCG.HashSet<Currency> resultSet = new JCG.HashSet<Currency>(list.Count);
            foreach (string code in list)
            {
                resultSet.Add(GetInstance(code));
            }
            return resultSet;
        }

        private const string EUR_STR = "EUR";
        private static readonly CacheBase<string, Currency> regionCurrencyCache = new CurrencySoftCache();

        private class CurrencySoftCache : SoftCache<string, Currency>
        {
            public override Currency GetOrCreate(string key, Func<string, Currency> valueFactory)
            {
                return LoadCurrency(key);
            }
        }

        /// <summary>
        /// Instantiate a currency from resource data.
        /// </summary>
        /* package */
        internal static Currency CreateCurrency(UCultureInfo loc)
        {
            string variant = loc.Variant;
            if ("EURO".Equals(variant, StringComparison.Ordinal))
            {
                return GetInstance(EUR_STR);
            }

            // Cache the currency by region, and whether variant=PREEURO.
            // Minimizes the size of the cache compared with caching by ULocale.
#pragma warning disable CS0618 // Type or member is obsolete
            string key = UCultureInfo.GetRegionForSupplementalData(loc, false);
#pragma warning restore CS0618 // Type or member is obsolete
            if ("PREEURO".Equals(variant, StringComparison.Ordinal))
            {
                key = key + '-';
            }
            return regionCurrencyCache.GetOrCreate(key, null);
        }

        private static Currency LoadCurrency(string key)
        {
            string region;
            bool isPreEuro;
            if (key.EndsWith("-", StringComparison.Ordinal))
            {
                region = key.Substring(0, key.Length - 1); // ICU4N: Checked 2nd arg
                isPreEuro = true;
            }
            else
            {
                region = key;
                isPreEuro = false;
            }
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
            IList<string> list = info.Currencies(CurrencyFilter.OnRegion(region));
            if (list.Count != 0)
            {
                string code = list[0];
                if (isPreEuro && EUR_STR.Equals(code, StringComparison.Ordinal))
                {
                    if (list.Count < 2)
                    {
                        return null;
                    }
                    code = list[1];
                }
                return GetInstance(code);
            }
            return null;
        }

        /// <summary>
        /// Returns a currency object given an ISO 4217 3-letter code.
        /// </summary>
        /// <param name="theISOCode">The iso code.</param>
        /// <returns>The currency for this iso code.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="theISOCode"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="theISOCode"/> is not a 3-letter alpha code.</exception>
        /// <stable>ICU 2.2</stable>
        public static Currency GetInstance(string theISOCode)
        {
            if (theISOCode is null)
                throw new ArgumentNullException(nameof(theISOCode), "The input currency code is null.");

            if (!IsAlpha3Code(theISOCode))
            {
                throw new ArgumentException(
                        "The input currency code is not 3-letter alphabetic code.");
            }
#pragma warning disable CS0618 // Type or member is obsolete
            //return (Currency)MeasureUnit.InternalGetInstance("currency", theISOCode.ToUpperInvariant()  /*toUpperCase(Locale.ENGLISH)*/);
            return (Currency)MeasureUnit.InternalGetInstance("currency", theISOCode.ToUpper(English));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static readonly CultureInfo English = new CultureInfo("en");


        private static bool IsAlpha3Code(string code)
        {
            if (code.Length != 3)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    char ch = code[i];
                    if (ch < 'A' || (ch > 'Z' && ch < 'a') || ch > 'z')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /////**
        //// * Returns a Currency object based on the currency represented by the given java.util.Currency.
        //// *
        //// * @param currency The Java currency object to convert.
        //// * @return An equivalent ICU currency object.
        //// * @draft ICU 60
        //// */
        ////public static Currency fromJavaCurrency(java.util.Currency currency)
        ////{
        ////    return getInstance(currency.getCurrencyCode());
        ////}

        /////**
        //// * Returns a java.util.Currency object based on the currency represented by this Currency.
        //// *
        //// * @return An equivalent Java currency object.
        //// * @draft ICU 60
        //// */
        ////public java.util.Currency toJavaCurrency()
        ////{
        ////    return java.util.Currency.getInstance(getCurrencyCode());
        ////}

        /// <summary>
        /// Registers a new currency for the provided locale.  The returned object
        /// is a key that can be used to unregister this currency object.
        /// <para/>
        /// Because ICU may choose to cache <see cref="Currency"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="GetInstance(UCultureInfo)"/> overloads to avoid undefined behavior.
        /// </summary>
        /// <param name="currency">The currency to register.</param>
        /// <param name="locale">The ulocale under which to register the currency.</param>
        /// <returns>A registry key that can be used to unregister this currency.</returns>
        /// <seealso cref="Unregister(object)"/>
        /// <stable>ICU 3.2</stable>
        public static object RegisterInstance(Currency currency, UCultureInfo locale)
        {
            return GetShim().RegisterInstance(currency, locale);
        }

        /// <summary>
        /// Unregister the currency associated with this key (obtained from
        /// <see cref="RegisterInstance(Currency, UCultureInfo)"/>).
        /// </summary>
        /// <param name="registryKey">The registry key returned from <see cref="RegisterInstance(Currency, UCultureInfo)"/>.</param>
        /// <returns><c>true</c> if the instance was successfully unregistered; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="registryKey"/> is <c>null</c>.</exception>
        /// <seealso cref="RegisterInstance(Currency, UCultureInfo)"/>
        /// <stable>ICU 2.6</stable>
        public static bool Unregister(object registryKey)
        {
            if (registryKey is null)
                throw new ArgumentNullException(nameof(registryKey), "registryKey must not be null");

            if (shim is null)
            {
                return false;
            }
            return shim.Unregister(registryKey);
        }

        /// <summary>
        /// Return an array of the locales for which a currency
        /// is defined within the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>An array of the available <see cref="CultureInfo"/>s.</returns>
        /// <stable>ICU 2.2</stable>
        public static CultureInfo[] GetCultures(UCultureTypes types)
        {
            if (shim is null)
            {
                return ICUResourceBundle.GetCultures(types);
            }
            else
            {
                return shim.GetCultures(types);
            }
        }

        /// <summary>
        /// Return an array of the ulocales for which a currency
        /// is defined within the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>An array of the available <see cref="UCultureInfo"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static UCultureInfo[] GetUCultures(UCultureTypes types)
        {
            if (shim == null)
            {
                return ICUResourceBundle.GetUCultures(types);
            }
            else
            {
                return shim.GetUCultures(types);
            }
        }

        // end registry stuff

        /// <summary>
        /// Given a key and a locale, returns an array of values for the key for which data
        /// exists.  If commonlyUsed is true, these are the values that typically are used
        /// with this locale, otherwise these are all values for which data exists.
        /// This is a common service API.
        /// <para/>
        /// The only supported key is "currency", other values return an empty array.
        /// <para/>
        /// Currency information is based on the region of the <paramref name="locale"/>.  If the <paramref name="locale"/> does not
        /// indicate a region, <see cref="UCultureInfo.AddLikelySubtags(UCultureInfo)"/> is used to infer a region,
        /// except for the 'und' locale.
        /// <para/>
        /// If <paramref name="commonlyUsed"/> is <c>true</c>, only the currencies known to be in use as of the current date
        /// are returned.  When there are more than one, these are returned in preference order
        /// (typically, this occurs when a country is transitioning to a new currency, and the
        /// newer currency is preferred), see
        /// <a href="http://unicode.org/reports/tr35/#Supplemental_Currency_Data">Unicode TR#35 Sec. C1</a>.
        /// If <paramref name="commonlyUsed"/> is <c>false</c>, all currencies ever used in any locale are returned, in no
        /// particular order.
        /// </summary>
        /// <param name="key">Key whose values to look up.  the only recognized key is "currency".</param>
        /// <param name="locale">The locale.</param>
        /// <param name="commonlyUsed">
        /// If <c>true</c>, return only values that are currently used in the locale.
        /// Otherwise returns all values.
        /// </param>
        /// <returns>
        /// An array of values for the given key and the locale. If there is no data, the
        /// array will be empty.
        /// </returns>
        /// <stable>ICU 4.2</stable>
        public static string[] GetKeywordValuesForLocale(string key, UCultureInfo locale,
                bool commonlyUsed) // ICU4N TODO: API - name to Culture
        {

            // The only keyword we recognize is 'currency'
            if (!"currency".Equals(key, StringComparison.Ordinal))
            {
                return EMPTY_STRING_ARRAY;
            }

            if (!commonlyUsed)
            {
                // Behavior change from 4.3.3, no longer sort the currencies
                return GetAllTenderCurrencies().ToArray();
            }

            // Don't resolve region if the requested locale is 'und', it will resolve to US
            // which we don't want.
            if (UND.Equals(locale))
            {
                return EMPTY_STRING_ARRAY;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            string prefRegion = UCultureInfo.GetRegionForSupplementalData(locale, true);
#pragma warning restore CS0618 // Type or member is obsolete

            CurrencyFilter filter = CurrencyFilter.Now().WithRegion(prefRegion);

            // currencies are in region's preferred order when we're filtering on region, which
            // matches our spec
            IList<string> result = GetTenderCurrencies(filter);

            // No fallback anymore (change from 4.3.3)
            if (result.Count == 0)
            {
                return EMPTY_STRING_ARRAY;
            }

            return result.ToArray();
        }

        private static readonly UCultureInfo UND = new UCultureInfo("und");
        private static readonly string[] EMPTY_STRING_ARRAY = new string[0];

        /// <summary>
        /// Returns the ISO 4217 3-letter code for this currency object.
        /// </summary>
        /// <stable>ICU 2.2</stable>
        public virtual string CurrencyCode
#pragma warning disable CS0618 // Type or member is obsolete
            => subType;
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Returns the ISO 4217 numeric code for this currency object.
        /// <para/>
        /// Note: If the ISO 4217 numeric code is not assigned for the currency or
        /// the currency is unknown, this method returns 0.
        /// </summary>
        /// <returns>The ISO 4217 numeric code of this currency.</returns>
        /// <stable>ICU 49</stable>
        public virtual int GetNumericCode()
        {
            int result = 0;
            try
            {
                UResourceBundle bundle = UResourceBundle.GetBundleInstance(
                        ICUData.IcuBaseName,
                        "currencyNumericCodes",
                        ICUResourceBundle.IcuDataAssembly);
                UResourceBundle codeMap = bundle.Get("codeMap");
#pragma warning disable CS0618 // Type or member is obsolete
                UResourceBundle numCode = codeMap.Get(subType);
#pragma warning restore CS0618 // Type or member is obsolete
                result = numCode.GetInt32();
            }
            catch (MissingManifestResourceException)
            {
                // fall through
            }
            return result;
        }

        /// <summary>
        /// Convenience and compatibility override of <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> that
        /// requests the symbol name for the <see cref="UCultureInfo.CurrentUICulture"/>.
        /// </summary>
        /// <seealso cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/>
        /// <seealso cref="GetName(UCultureInfo, CurrencyNameStyle, out bool)"/>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, string, out bool)"/>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, out bool)"/>
        /// <seealso cref="CultureInfo.CurrentUICulture"/>
        /// <stable>ICU 3.4</stable>
        public virtual string GetSymbol()
        {
            return GetSymbol(UCultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Convenience and compatibility override of <see cref="GetName(CultureInfo, CurrencyNameStyle, string, out bool)"/> that
        /// requests the symbol name.
        /// </summary>
        /// <param name="loc">The <see cref="CultureInfo"/> for the symbol.</param>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, string, out bool)"/>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, out bool)"/>
        /// <stable>ICU 3.4</stable>
        public virtual string GetSymbol(CultureInfo loc)
        {
            return GetSymbol(loc.ToUCultureInfo());
        }

        /// <summary>
        /// Convenience and compatibility override of <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> that
        /// requests the symbol name.
        /// </summary>
        /// <param name="uloc">The <see cref="UCultureInfo"/> for the symbol.</param>
        /// <seealso cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/>
        /// <seealso cref="GetName(UCultureInfo, CurrencyNameStyle, out bool)"/>
        /// <stable>ICU 3.4</stable>
        public virtual string GetSymbol(UCultureInfo uloc)
        {
            return GetName(uloc, CurrencyNameStyle.SymbolName, out bool _);
        }

        /// <summary>
        /// Returns the display name for the given currency in the
        /// given <paramref name="locale"/>. This is a convenience method for
        /// <see cref="GetName(UCultureInfo, CurrencyNameStyle, out bool)"/>
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public virtual string GetName(CultureInfo locale,
                          CurrencyNameStyle nameStyle,
                          out bool isChoiceFormat)
        {
            return GetName(locale.ToUCultureInfo(), nameStyle, out isChoiceFormat);
        }

        /// <summary>
        /// Returns the display name for the given currency in the
        /// given <paramref name="locale"/>.  For example, the display name for the USD
        /// currency object in the en_US locale is "$".
        /// </summary>
        /// <param name="locale">Locale in which to display currency.</param>
        /// <param name="nameStyle">Selector for which kind of name to return. The <paramref name="nameStyle"/>
        /// should be <see cref="CurrencyNameStyle.SymbolName"/>, <see cref="CurrencyNameStyle.LongName"/>, or <see cref="CurrencyNameStyle.NarrowSymbolName"/>;
        /// otherwise an <see cref="ArgumentException"/> is thrown.</param>
        /// <param name="isChoiceFormat">Output parameter that is set to <c>true</c> if the returned value is ChoiceFormat pattern;
        /// othwise it is set to <c>false</c>.</param>
        /// <returns>Display string for this currency.  If the resource data
        /// contains no entry for this currency, then the ISO 4217 code is
        /// returned.  If <paramref name="isChoiceFormat"/> is <c>true</c>, then the result is a
        /// ChoiceFormat pattern.  Otherwise it is a static string. <b>Note:</b>
        /// as of ICU 4.4, choice formats are not used, and the value returned
        /// in <paramref name="isChoiceFormat"/> is always <c>false</c>.
        /// </returns>
        /// <exception cref="NotSupportedException"><paramref name="nameStyle"/> is set to <see cref="CurrencyNameStyle.NarrowSymbolName"/>
        /// and a custom <see cref="CurrencyDisplayNames"/> implementation is registed for <paramref name="locale"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="nameStyle"/> is not one of
        /// <see cref="CurrencyNameStyle.SymbolName"/>, <see cref="CurrencyNameStyle.LongName"/>, or <see cref="CurrencyNameStyle.NarrowSymbolName"/>.</exception>
        /// <seealso cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/>
        /// <stable>ICU 3.2</stable>
        public virtual string GetName(UCultureInfo locale, CurrencyNameStyle nameStyle, out bool isChoiceFormat)
        {
            // We no longer support choice format data in names.  Data should not contain
            // choice patterns.
            isChoiceFormat = false;

            CurrencyDisplayNames names = CurrencyDisplayNames.GetInstance(locale);
#pragma warning disable CS0618 // Type or member is obsolete
            switch (nameStyle)
            {
                case CurrencyNameStyle.SymbolName:
                    return names.GetSymbol(subType);
                case CurrencyNameStyle.NarrowSymbolName:
                    // CurrencyDisplayNames is the public interface.
                    // CurrencyDisplayInfo is ICU's standard implementation.
                    if (!(names is CurrencyDisplayInfo currencyDisplayInfo))
                    {
                        throw new NotSupportedException(
                                "Cannot get narrow symbol from custom currency display name provider");
                    }
                    return currencyDisplayInfo.GetNarrowSymbol(subType);
                case CurrencyNameStyle.LongName:
                    return names.GetName(subType);
                default:
                    throw new ArgumentException("bad name style: " + nameStyle);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the display name for the given currency in the given <paramref name="locale"/>.
        /// <para/>
        /// This is a convenience overload of <see cref="GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/>.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual string GetName(CultureInfo locale, CurrencyNameStyle nameStyle, string pluralCount,
            out bool isChoiceFormat)
        {
            return GetName(locale.ToUCultureInfo(), nameStyle, pluralCount, out isChoiceFormat);
        }

        /// <summary>
        /// Returns the display name for the given currency in the
        /// given <paramref name="locale"/>.  For example, the <see cref="CurrencyNameStyle.SymbolName"/> for the USD
        /// currency object in the en_US locale is "$".
        /// The <see cref="CurrencyNameStyle.PluralLongName"/> for the USD currency object when the currency
        /// amount is plural is "US dollars", such as in "3.00 US dollars";
        /// while the <see cref="CurrencyNameStyle.PluralLongName"/> for the USD currency object when the currency
        /// amount is singular is "US dollar", such as in "1.00 US dollar".
        /// </summary>
        /// <param name="locale">Locale in which to display currency.</param>
        /// <param name="nameStyle">Selector for which kind of name to return.</param>
        /// <param name="pluralCount">Plural count string for this locale.</param>
        /// <param name="isChoiceFormat">Output parameter. Returns <c>true</c> if the returned value is a
        /// ChoiceFormat pattern; otherwise it is set to <c>false</c>.</param>
        /// <returns>Display string for this currency.  If the resource data
        /// contains no entry for this currency, then the ISO 4217 code is
        /// returned. If <paramref name="isChoiceFormat"/> is <c>true</c>, then the result is a
        /// ChoiceFormat pattern. Otherwise it is a static string. <b>Note:</b>
        /// as of ICU 4.4, choice formats are not used, and the value returned
        /// in <paramref name="isChoiceFormat"/> is always <c>false</c>.
        /// </returns>
        /// <exception cref="NotSupportedException"><paramref name="nameStyle"/> is set to <see cref="CurrencyNameStyle.NarrowSymbolName"/>
        /// and a custom <see cref="CurrencyDisplayNames"/> implementation is registed for <paramref name="locale"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="nameStyle"/> is not one of <see cref="CurrencyNameStyle.SymbolName"/>,
        /// <see cref="CurrencyNameStyle.LongName"/>, <see cref="CurrencyNameStyle.PluralLongName"/> or <see cref="CurrencyNameStyle.NarrowSymbolName"/>.</exception>
        /// <stable>ICU 4.2</stable>
        public virtual string GetName(UCultureInfo locale, CurrencyNameStyle nameStyle, string pluralCount,
            out bool isChoiceFormat)
        {
            if (nameStyle != CurrencyNameStyle.PluralLongName)
            {
                return GetName(locale, nameStyle, out isChoiceFormat);
            }

            // We no longer support choice format
            isChoiceFormat = false;

            CurrencyDisplayNames names = CurrencyDisplayNames.GetInstance(locale);
#pragma warning disable CS0618 // Type or member is obsolete
            return names.GetPluralName(subType, pluralCount);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the display name for this currency in the <see cref="CultureInfo.CurrentCulture"/>.
        /// If the resource data for the <see cref="CultureInfo.CurrentCulture"/> contains no entry for this currency,
        /// then the ISO 4217 code is returned.
        /// <para/>
        /// Note: This method is a convenience equivalent for
        /// <see cref="Currency.GetDisplayName()"/> and is equivalent to
        /// <c>GetName(CultureInfo.CurrentCulture, CurrencyNameStyle.LongName, out bool _)</c>.
        /// </summary>
        /// <returns>The display name of this currency.</returns>
        /// <seealso cref="GetDisplayName(CultureInfo)"/>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, out bool)"/>
        /// <stable>ICU 49</stable>
        public virtual string GetDisplayName()
        {
            return GetName(CultureInfo.CurrentCulture, CurrencyNameStyle.LongName, out bool _);
        }

        /// <summary>
        /// Returns the display name for this currency in the given <paramref name="locale"/>.
        /// If the resource data for the given <paramref name="locale"/> contains no entry for this currency,
        /// then the ISO 4217 code is returned.
        /// <para/>
        /// Note: This method is a convenience equivalent for
        /// <see cref="GetDisplayName(CultureInfo)"/> and is equivalent to
        /// <c>GetName(locale, CurrencyNameStyle.LongName, out bool _)</c>.
        /// </summary>
        /// <param name="locale">Locale in which to display currency.</param>
        /// <returns>The display name of this currency for the specified locale.</returns>
        /// <seealso cref="GetDisplayName(CultureInfo)"/>
        /// <seealso cref="GetName(CultureInfo, CurrencyNameStyle, out bool)"/>
        /// <stable>ICU 49</stable>
        public virtual string GetDisplayName(CultureInfo locale)
        {
            return GetName(locale, CurrencyNameStyle.LongName, out bool _);
        }

        /// <summary>
        /// Attempt to parse the given string as a currency, either as a
        /// display name in the given locale, or as a 3-letter ISO 4217
        /// code.  If multiple display names match, then the longest one is
        /// selected.  If both a display name and a 3-letter ISO code
        /// match, then the display name is preferred, unless it's length
        /// is less than 3.
        /// </summary>
        /// <param name="locale">The locale of the display names to match.</param>
        /// <param name="text">The text to parse.</param>
        /// <param name="type">Parse against currency type: <see cref="CurrencyNameStyle.LongName"/> only or not.</param>
        /// <param name="pos">Input-output position; on input, the position within
        /// text to match; must have 0 &lt;= pos.Index &lt; text.Length;
        /// on output, the position after the last matched character. If
        /// the parse fails, the position in unchanged upon output.</param>
        /// <returns>The ISO 4217 code, as a string, of the best match, or
        /// <c>null</c> if there is no match.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static string Parse(UCultureInfo locale, string text, CurrencyNameStyle type, ParsePosition pos) // ICU4N specific - made internal rather than public
        {
            IList<TextTrieMap<CurrencyStringInfo>> currencyTrieVec = GetCurrencyTrieVec(locale);
            int maxLength = 0;
            string isoResult = null;

            // look for the names
            TextTrieMap<CurrencyStringInfo> currencyNameTrie = currencyTrieVec[1];
            CurrencyNameResultHandler handler = new CurrencyNameResultHandler();
            currencyNameTrie.Find(text, pos.Index, handler);
            isoResult = handler.BestCurrencyISOCode;
            maxLength = handler.BestMatchLength;

            if (type != CurrencyNameStyle.LongName)
            {  // not long name only
                TextTrieMap<CurrencyStringInfo> currencySymbolTrie = currencyTrieVec[0];
                handler = new CurrencyNameResultHandler();
                currencySymbolTrie.Find(text, pos.Index, handler);
                if (handler.BestMatchLength > maxLength)
                {
                    isoResult = handler.BestCurrencyISOCode;
                    maxLength = handler.BestMatchLength;
                }
            }
            int start = pos.Index;
            pos.Index = (start + maxLength);
            return isoResult;
        }

        /// <internal/>
        //[Obsolete("This API is ICU internal only.")]
        internal static TextTrieMap<CurrencyStringInfo>.ParseState OpenParseState(
            UCultureInfo locale, int startingCp, CurrencyNameStyle type) // ICU4N specific - made internal rather than public
        {
            IList<TextTrieMap<CurrencyStringInfo>> currencyTrieVec = GetCurrencyTrieVec(locale);
            if (type == CurrencyNameStyle.LongName)
            {
                return currencyTrieVec[0].OpenParseState(startingCp);
            }
            else
            {
                return currencyTrieVec[1].OpenParseState(startingCp);
            }
        }

        private static IList<TextTrieMap<CurrencyStringInfo>> GetCurrencyTrieVec(UCultureInfo locale)
        {
            return CURRENCY_NAME_CACHE.GetOrAdd(locale, (loc) =>
            {
                TextTrieMap<CurrencyStringInfo> currencyNameTrie =
                    new TextTrieMap<CurrencyStringInfo>(true);
                TextTrieMap<CurrencyStringInfo> currencySymbolTrie =
                    new TextTrieMap<CurrencyStringInfo>(false);
                var currencyTrieVec = new List<TextTrieMap<CurrencyStringInfo>> { currencySymbolTrie, currencyNameTrie };
                SetupCurrencyTrieVec(locale, currencyTrieVec);
                return currencyTrieVec;
            });
        }

        private static void SetupCurrencyTrieVec(UCultureInfo locale,
                IList<TextTrieMap<CurrencyStringInfo>> trieVec)
        {

            TextTrieMap<CurrencyStringInfo> symTrie = trieVec[0];
            TextTrieMap<CurrencyStringInfo> trie = trieVec[1];

            CurrencyDisplayNames names = CurrencyDisplayNames.GetInstance(locale);
            foreach (var e in names.SymbolMap)
            {
                string symbol = e.Key;
                string isoCode = e.Value;

                // Register under not just symbol, but under every equivalent symbol as well
                // e.g short width yen and long width yen.
                foreach (string equivalentSymbol in EQUIVALENT_CURRENCY_SYMBOLS.Get(symbol))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    symTrie.Put(equivalentSymbol, new CurrencyStringInfo(isoCode, symbol));
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
            foreach (var e in names.NameMap)
            {
                string name = e.Key;
                string isoCode = e.Value;
#pragma warning disable CS0618 // Type or member is obsolete
                trie.Put(name, new CurrencyStringInfo(isoCode, name));
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /// <internal/>
        //[Obsolete("his API is ICU internal only.")]
        internal sealed class CurrencyStringInfo // ICU4N specific - made internal rather than public
        {
            private readonly string isoCode;
            private readonly string currencyString;

            /// <internal/>
            [Obsolete("his API is ICU internal only.")]
            public CurrencyStringInfo(string isoCode, string currencyString)
            {
                this.isoCode = isoCode ?? throw new ArgumentNullException(nameof(isoCode));
                this.currencyString = currencyString ?? throw new ArgumentNullException(nameof(currencyString));
            }

            /// <internal/>
            [Obsolete("his API is ICU internal only.")]
            public string ISOCode => isoCode;

            /// <internal/>
            [Obsolete("his API is ICU internal only.")]
            public string CurrencyString => currencyString;
        }

        private sealed class CurrencyNameResultHandler : IResultHandler<CurrencyStringInfo>
        {
            // The length of longest matching key
            private int bestMatchLength;
            // The currency ISO code of longest matching key
            private string bestCurrencyISOCode;

            // As the trie is traversed, handlePrefixMatch is called at each node. matchLength is the
            // length length of the key at the current node; values is the list of all the values mapped to
            // that key. matchLength increases with each call as trie is traversed.
            public bool HandlePrefixMatch(int matchLength, IEnumerator<CurrencyStringInfo> values)
            {
                if (values.MoveNext())
                {
                    // Since the best match criteria is only based on length of key in trie and since all the
                    // values are mapped to the same key, we only need to examine the first value.
#pragma warning disable CS0618 // Type or member is obsolete
                    bestCurrencyISOCode = values.Current.ISOCode;
#pragma warning restore CS0618 // Type or member is obsolete
                    bestMatchLength = matchLength;
                }
                return true;
            }

            public string BestCurrencyISOCode => bestCurrencyISOCode;

            public int BestMatchLength => bestMatchLength;
        }

        /// <summary>
        /// Returns the number of the number of fraction digits that should
        /// be displayed for this currency.
        /// This is equivalent to <c>GetDefaultFractionDigits(CurrencyUsage.Standard)</c>.
        /// </summary>
        /// <returns>A non-negative number of fraction digits to be displayed.</returns>
        /// <stable>ICU 2.2</stable>
        public virtual int GetDefaultFractionDigits()
        {
            return GetDefaultFractionDigits(CurrencyUsage.Standard);
        }

        /// <summary>
        /// Returns the number of the number of fraction digits that should
        /// be displayed for this currency with <paramref name="usage"/>.
        /// </summary>
        /// <param name="usage">The usage of currency (<see cref="CurrencyUsage.Standard"/> or <see cref="CurrencyUsage.Cash"/>).</param>
        /// <returns>A non-negative number of fraction digits to be displayed.</returns>
        /// <stable>ICU 54</stable>
        public virtual int GetDefaultFractionDigits(CurrencyUsage usage)
        {
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
#pragma warning disable CS0618 // Type or member is obsolete
            CurrencyDigits digits = info.CurrencyDigits(subType, usage);
#pragma warning restore CS0618 // Type or member is obsolete
            return digits.FractionDigits;
        }

        /// <summary>
        /// Returns the rounding increment for this currency, or 0.0 if no
        /// rounding is done by this currency.
        /// This is equivalent to <c>GetRoundingIncrement(CurrencyUsage.Standard)</c>.
        /// </summary>
        /// <returns>The non-negative rounding increment, or 0.0 if none.</returns>
        /// <stable>ICU 2.2</stable>
        public virtual double GetRoundingIncrement()
        {
            return GetRoundingIncrement(CurrencyUsage.Standard);
        }

        /// <summary>
        /// Returns the rounding increment for this currency, or 0.0 if no
        /// rounding is done by this currency with the <paramref name="usage"/>.
        /// </summary>
        /// <param name="usage">The usage of currency (<see cref="CurrencyUsage.Standard"/> or <see cref="CurrencyUsage.Cash"/>).</param>
        /// <returns>The non-negative rounding increment, or 0.0 if none.</returns>
        /// <stable>ICU 54</stable>
        public virtual double GetRoundingIncrement(CurrencyUsage usage)
        {
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
#pragma warning disable CS0618 // Type or member is obsolete
            CurrencyDigits digits = info.CurrencyDigits(subType, usage);
#pragma warning restore CS0618 // Type or member is obsolete

            int data1 = digits.RoundingIncrement;

            // If there is no rounding return 0.0 to indicate no rounding.
            // This is the high-runner case, by far.
            if (data1 == 0)
            {
                return 0.0;
            }

            int data0 = digits.FractionDigits;

            // If the meta data is invalid, return 0.0 to indicate no rounding.
            if (data0 < 0 || data0 >= POW10.Length)
            {
                return 0.0;
            }

            // Return data[1] / 10^(data[0]). The only actual rounding data,
            // as of this writing, is CHF { 2, 25 }.
            return (double)data1 / POW10[data0];
        }

        /// <summary>
        /// Returns the ISO 4217 code for this currency.
        /// </summary>
        /// <stable>ICU 2.2</stable>
        public override string ToString()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return subType;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Constructs a currency object for the given ISO 4217 3-letter
        /// code. This constructor assumes that the code is valid.
        /// </summary>
        /// <param name="theISOCode">The iso code used to construct the currency.</param>
        /// <stable>ICU 3.4</stable>
        protected internal Currency(string theISOCode)
#pragma warning disable CS0618 // Type or member is obsolete
            : base("currency", theISOCode)
#pragma warning restore CS0618 // Type or member is obsolete
        {

            // isoCode is kept for readResolve() and Currency class no longer
            // use it. So this statement actually does not have any effect.
            isoCode = theISOCode;
        }

        // POW10[i] = 10^i
        private static readonly int[] POW10 = {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000
        };


        private static SoftReference<IList<string>> ALL_TENDER_CODES;
        private static SoftReference<ISet<string>> ALL_CODES_AS_SET;

        /// <summary>
        /// Returns an unmodifiable string list including all known tender currency codes.
        /// </summary>
        private static IList<string> GetAllTenderCurrencies()
        {
            lock (cacheLock) // ICU4N: cacheLock synchronizes with MeasureUnit
            {
                if (ALL_TENDER_CODES == null || ALL_TENDER_CODES.TryGetValue(out IList<string> all) || all == null)
                {
                    // Filter out non-tender currencies which have "from" date set to 9999-12-31
                    // CurrencyFilter has "to" value set to 9998-12-31 in order to exclude them
                    //CurrencyFilter filter = CurrencyFilter.onDateRange(null, new Date(253373299200000L));
                    CurrencyFilter filter = CurrencyFilter.All;
                    all = GetTenderCurrencies(filter).AsReadOnly();
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                    ALL_TENDER_CODES = new SoftReference<IList<string>>(all, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
#else
                    ALL_TENDER_CODES = new SoftReference<IList<string>>(all, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
#endif
                }
                return all;
            }
        }

        private static ISet<string> GetAllCurrenciesAsSet()
        {
            lock (cacheLock)
            {
                if (ALL_CODES_AS_SET == null || !ALL_CODES_AS_SET.TryGetValue(out ISet<string> all) || all == null)
                {
                    CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
                    all = new HashSet<string>(info.Currencies(CurrencyFilter.All)).AsReadOnly();
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                    ALL_CODES_AS_SET = new SoftReference<ISet<string>>(all, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
#else
                    ALL_CODES_AS_SET = new SoftReference<ISet<string>>(all, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
#endif
                }
                return all;
            }
        }

        /// <summary>
        /// Queries if the given ISO 4217 3-letter code is available on the specified date range.
        /// <para/>
        /// Note: For checking availability of a currency on a specific date, specify the date on both <paramref name="from"/>
        /// and <paramref name="to"/>. When both <paramref name="from"/> and <paramref name="to"/> are <c>null</c>, this method checks if the specified
        /// currency is available all time.
        /// </summary>
        /// <param name="code">The ISO 4217 3-letter code.</param>
        /// <param name="from">The lower bound of the date range, inclusive. When <paramref name="from"/> is <c>null</c>, check the availability
        /// of the currency any date before <paramref name="to"/>.</param>
        /// <param name="to">The upper bound of the date range, inclusive. When <paramref name="to"/> is <c>null</c>, check the availability of
        /// of the currency any date after <paramref name="from"/>.</param>
        /// <returns><c>true</c> if the given ISO 4217 3-letter code is supported on the specified date range.</returns>
        /// <exception cref="ArgumentException"><paramref name="to"/> is before <paramref name="from"/>.</exception>
        /// <stable>ICU 4.6</stable>
        public static bool IsAvailable(string code, DateTime? from, DateTime? to)
        {
            if (!IsAlpha3Code(code))
            {
                return false;
            }

            if (from != null && to != null && from > to)
            {
                throw new ArgumentException("To is before from");
            }

            code = code.ToUpper(English);
            bool isKnown = GetAllCurrenciesAsSet().Contains(code);
            if (isKnown == false)
            {
                return false;
            }
            else if (from == null && to == null)
            {
                return true;
            }

            // If caller passed a date range, we cannot rely solely on the cache
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
            IList<string> allActive = info.Currencies(
                    CurrencyFilter.OnDateRange(from, to).WithCurrency(code));
            return allActive.Contains(code);
        }

        /// <summary>
        /// Returns the list of remaining tender currencies after a filter is applied.
        /// </summary>
        /// <param name="filter">The filter to apply to the tender currencies.</param>
        /// <returns>A list of tender currencies.</returns>
        private static IList<string> GetTenderCurrencies(CurrencyFilter filter)
        {
            CurrencyMetaInfo info = CurrencyMetaInfo.GetInstance();
            return info.Currencies(filter.WithTender());
        }

        private sealed class EquivalenceRelation<T>
        {

            private IDictionary<T, ISet<T>> data = new Dictionary<T, ISet<T>>();

            //@SuppressWarnings("unchecked")  // See ticket #11395, this is safe.
            public EquivalenceRelation<T> Add(params T[] items)
            {
                ISet<T> group = new HashSet<T>();
                foreach (T item in items)
                {
                    if (data.ContainsKey(item))
                    {
                        throw new ArgumentException("All groups passed to add must be disjoint.");
                    }
                    group.Add(item);
                }
                foreach (T item in items)
                {
                    data[item] = group;
                }
                return this;
            }

            public ISet<T> Get(T item)
            {
                if (!data.TryGetValue(item, out ISet<T> result) || result == null)
                {
                    return new HashSet<T> { item }.AsReadOnly();
                }
                return result.AsReadOnly();
            }
        }

        // ICU4N TODO: Serialization
        //private Object writeReplace() throws ObjectStreamException
        //{
        //        return new MeasureUnitProxy(type, subType);
        //    }

        // For backward compatibility only
        /// <summary>
        /// ISO 4217 3-letter code.
        /// </summary>
        private readonly string isoCode;

        // ICU4N TODO: Serialization
        //private Object readResolve() throws ObjectStreamException
        //{
        //        // The old isoCode field used to determine the currency.
        //        return Currency.getInstance(isoCode);
        //}
    }

    /// <summary>
    /// Currency Usage used for <see cref="DecimalFormat"/>
    /// </summary>
    /// <stable>ICU 54</stable>
    public enum CurrencyUsage
    {
        /// <summary>
        /// A setting to specify currency usage which determines currency digit and rounding
        /// for standard usage, for example: "50.00 NT$"
        /// </summary>
        /// <stable>ICU 54</stable>
        Standard,

        /// <summary>
        /// A setting to specify currency usage which determines currency digit and rounding
        /// for cash usage, for example: "50 NT$"
        /// </summary>
        /// <stable>ICU 54</stable>
        Cash
    }

    /// <summary>
    /// Selectors for <see cref="Currency.GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads
    /// indicating the naming style for a currency.
    /// </summary>
    /// <draft>ICU 60</draft>
    // ICU4N specific
    public enum CurrencyNameStyle
    {
        /// <summary>
        /// Selector for <see cref="Currency.GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating a symbolic name for a
        /// currency, such as "$" for USD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        SymbolName = 0,

        /// <summary>
        /// Selector for <see cref="Currency.GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the long name for a
        /// currency, such as "US Dollar" for USD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        LongName = 1,

        /// <summary>
        /// Selector for <see cref="Currency.GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the plural long name for a
        /// currency, such as "US dollar" for USD in "1 US dollar",
        /// and "US dollars" for USD in "2 US dollars".
        /// </summary>
        /// <stable>ICU 4.2</stable>
        PluralLongName = 2,

        /// <summary>
        /// Selector for <see cref="Currency.GetName(UCultureInfo, CurrencyNameStyle, string, out bool)"/> overloads indicating the narrow currency symbol.
        /// The narrow currency symbol is similar to the regular currency
        /// symbol, but it always takes the shortest form: for example,
        /// "$" instead of "US$".
        /// <para/>
        /// This method assumes that the currency data provider is the ICU4N
        /// built-in data provider. If it is not, an exception is thrown.
        /// </summary>
        /// <internal/>
        [Obsolete("ICU 60: This API is ICU internal only.")]
        NarrowSymbolName = 3,
    }
}
