using ICU4N.Impl;
using ICU4N.Impl.Locale;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Collections.Generic.Extensions;
using J2N.Globalization;
using J2N.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using JCG = J2N.Collections.Generic;
using UnicodeLocaleExtensionClass = ICU4N.Impl.Locale.UnicodeLocaleExtension;
#nullable enable

namespace ICU4N.Globalization
{

    // ICU4N: Ideally, we would make this into a subclass of CultureInfo, however
    // many of the exposed types such as TextInfo, DateTimeFormatInfo, NumberFormatInfo
    // have no (good) way to be extended. For now, we are keeping the API similar, but
    // not making this a subclass.
#if FEATURE_CULTUREINFO_SERIALIZABLE
    [Serializable]
#endif
    public sealed partial class UCultureInfo : /*CultureInfo,*/ IFormatProvider, IComparable<UCultureInfo>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        private const int CharStackBufferSize = 32;

        internal static readonly CacheBase<string, string> nameCache = new SoftCache<string, string>();

        /// <summary>
        /// ICU locale ID, in .NET, this is referred to as the FullName.
        /// This is the <a href="https://tools.ietf.org/html/bcp47">BCP 47</a> representation of the culture.
        /// </summary>
        internal readonly string localeID;

        /// <summary>
        /// The closest match for the current UCultureInfo as a CultureInfo
        /// </summary>
        internal readonly CultureInfo culture; // internal for testing

        // Used in both UCultureInfo and LocaleIDParser, so moved up here.
        private const char Underscore = '_';

        // special keyword key for Unicode locale attributes
        private const string LocaleAttributeKey = "attribute";

        private static readonly UCultureInfo invariantCultureInfo = new UCultureInfo(UCultureData.Invariant, isReadOnly: true);

        /// <summary>
        /// Gets the <see cref="UCultureInfo"/> object that is culture-independent (invariant).
        /// </summary>
        // ICU4N: This corresponds to the ROOT in ICU4J
        public static UCultureInfo InvariantCulture
        {
            get
            {
                Debug.Assert(invariantCultureInfo is not null);
                return invariantCultureInfo!;
            }
        }
        //public static UCultureInfo InvariantCulture { get; } = new UCultureInfo("", CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the English culture. We cache it statically to optimize the <see cref="EnglishName"/> property.
        /// </summary>
        private static readonly UCultureInfo English = new UCultureInfo("en", new CultureInfo("en"));

        private readonly LocaleID localeIdentifier;
        internal readonly bool isNeutralCulture;
        internal readonly bool isInvariantCulture;

        internal readonly UCultureData cultureData;
        internal UNumberFormatInfo? numInfo;

        private bool isReadOnly;

        /// <summary>
        /// Cache the locale data container fields.
        /// In future, we want to use them as the primary locale identifier storage.
        /// </summary>
#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
        private volatile BaseLocale? baseLocale;

#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
        private volatile string? name;

#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
        private volatile LocaleExtensions? extensions;

#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
#if FEATURE_IREADONLYCOLLECTIONS
        private volatile IReadOnlyDictionary<string, string>? keywords;
#else
        private volatile IDictionary<string, string>? keywords;
#endif

#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
#if FEATURE_IREADONLYCOLLECTIONS
        private volatile IReadOnlyDictionary<string, string>? unicodeLocales;
#else
        private volatile IDictionary<string, string>? unicodeLocales;
#endif

#if FEATURE_CULTUREINFO_SERIALIZABLE
        [NonSerialized]
#endif
        private volatile string? languageTag;

        private const string UndeterminedWithSeparator = LanguageTag.Undetermined + "-";

        /// <summary>
        /// This table lists pairs of locale ids for canonicalization.  The
        /// The 1st item is the normalized id. The 2nd item is the
        /// canonicalized id. The 3rd is the keyword. The 4th is the keyword value.
        /// </summary>
        private static readonly string?[][] CANONICALIZE_MAP = {
            new string?[] { "C",              "en_US_POSIX", null, null }, /* POSIX name */
            new string?[] { "art_LOJBAN",     "jbo", null, null }, /* registered name */
            new string?[] { "az_AZ_CYRL",     "az_Cyrl_AZ", null, null }, /* .NET name */
            new string?[] { "az_AZ_LATN",     "az_Latn_AZ", null, null }, /* .NET name */
            new string?[] { "ca_ES_PREEURO",  "ca_ES", "currency", "ESP" },
            new string?[] { "cel_GAULISH",    "cel__GAULISH", null, null }, /* registered name */
            new string?[] { "de_1901",        "de__1901", null, null }, /* registered name */
            new string?[] { "de_1906",        "de__1906", null, null }, /* registered name */
            new string?[] { "de__PHONEBOOK",  "de", "collation", "phonebook" }, /* Old ICU name */
            new string?[] { "de_AT_PREEURO",  "de_AT", "currency", "ATS" },
            new string?[] { "de_DE_PREEURO",  "de_DE", "currency", "DEM" },
            new string?[] { "de_LU_PREEURO",  "de_LU", "currency", "EUR" },
            new string?[] { "el_GR_PREEURO",  "el_GR", "currency", "GRD" },
            new string?[] { "en_BOONT",       "en__BOONT", null, null }, /* registered name */
            new string?[] { "en_SCOUSE",      "en__SCOUSE", null, null }, /* registered name */
            new string?[] { "en_BE_PREEURO",  "en_BE", "currency", "BEF" },
            new string?[] { "en_IE_PREEURO",  "en_IE", "currency", "IEP" },
            new string?[] { "es__TRADITIONAL", "es", "collation", "traditional" }, /* Old ICU name */
            new string?[] { "es_ES_PREEURO",  "es_ES", "currency", "ESP" },
            new string?[] { "eu_ES_PREEURO",  "eu_ES", "currency", "ESP" },
            new string?[] { "fi_FI_PREEURO",  "fi_FI", "currency", "FIM" },
            new string?[] { "fr_BE_PREEURO",  "fr_BE", "currency", "BEF" },
            new string?[] { "fr_FR_PREEURO",  "fr_FR", "currency", "FRF" },
            new string?[] { "fr_LU_PREEURO",  "fr_LU", "currency", "LUF" },
            new string?[] { "ga_IE_PREEURO",  "ga_IE", "currency", "IEP" },
            new string?[] { "gl_ES_PREEURO",  "gl_ES", "currency", "ESP" },
            new string?[] { "hi__DIRECT",     "hi", "collation", "direct" }, /* Old ICU name */
            new string?[] { "it_IT_PREEURO",  "it_IT", "currency", "ITL" },
            new string?[] { "ja_JP_TRADITIONAL", "ja_JP", "calendar", "japanese" },
          // new string?[] { "nb_NO_NY",       "nn_NO", null, null },
            new string?[] { "nl_BE_PREEURO",  "nl_BE", "currency", "BEF" },
            new string?[] { "nl_NL_PREEURO",  "nl_NL", "currency", "NLG" },
            new string?[] { "pt_PT_PREEURO",  "pt_PT", "currency", "PTE" },
            new string?[] { "sl_ROZAJ",       "sl__ROZAJ", null, null }, /* registered name */
            new string?[] { "sr_SP_CYRL",     "sr_Cyrl_RS", null, null }, /* .NET name */
            new string?[] { "sr_SP_LATN",     "sr_Latn_RS", null, null }, /* .NET name */
            new string?[] { "sr_YU_CYRILLIC", "sr_Cyrl_RS", null, null }, /* Linux name */
            new string?[] { "th_TH_TRADITIONAL", "th_TH", "calendar", "buddhist" }, /* Old ICU name */
            new string?[] { "uz_UZ_CYRILLIC", "uz_Cyrl_UZ", null, null }, /* Linux name */
            new string?[] { "uz_UZ_CYRL",     "uz_Cyrl_UZ", null, null }, /* .NET name */
            new string?[] { "uz_UZ_LATN",     "uz_Latn_UZ", null, null }, /* .NET name */
            new string?[] { "zh_CHS",         "zh_Hans", null, null }, /* .NET name */
            new string?[] { "zh_CHT",         "zh_Hant", null, null }, /* .NET name */
            new string?[] { "zh_GAN",         "zh__GAN", null, null }, /* registered name */
            new string?[] { "zh_GUOYU",       "zh", null, null }, /* registered name */
            new string?[] { "zh_HAKKA",       "zh__HAKKA", null, null }, /* registered name */
            new string?[] { "zh_MIN",         "zh__MIN", null, null }, /* registered name */
            new string?[] { "zh_MIN_NAN",     "zh__MINNAN", null, null }, /* registered name */
            new string?[] { "zh_WUU",         "zh__WUU", null, null }, /* registered name */
            new string?[] { "zh_XIANG",       "zh__XIANG", null, null }, /* registered name */
            new string?[] { "zh_YUE",         "zh__YUE", null, null } /* registered name */
        };

        /// <summary>
        /// This table lists pairs of locale ids for canonicalization.
        /// The first item is the normalized variant id.
        /// </summary>
        private static readonly string[][] variantsToKeywords = {
            new string[] { "EURO",   "currency", "EUR" },
            new string[] { "PINYIN", "collation", "pinyin" }, /* Solaris variant */
            new string[] { "STROKE", "collation", "stroke" }  /* Solaris variant */
        };

        /// <summary>
        /// Private constructor used by static initializers.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="culture"></param>
        private UCultureInfo(string name, CultureInfo? culture)
            //: base(name: "")
        {
            this.localeID = name ?? string.Empty;
            this.culture = culture!; // ICU4N: The constructor that calls us with null populates this value
            this.isReadOnly = false;

            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            this.localeIdentifier = parser.GetLocaleID();

            // NOTE: Invariant culture is not neutral
            this.isNeutralCulture = localeIdentifier.IsNeutralCulture;
            this.isInvariantCulture = localeIdentifier.IsInvariantCulture;

            this.cultureData = UCultureData.GetCultureData(this);
        }

        // Constructor for invariant culture property
        // ICU4N TODO: Clean up constructors. We ought to be using localeId from UCultureData, but need to
        // think through how to correctly populate UCultureData. We need a shared parser that everything
        // can use to get the bits from a localeID. Does that piece need to be used elsewhere? Possibly...
        private UCultureInfo(UCultureData cultureData, bool isReadOnly)
        {
            Debug.Assert(cultureData is not null);

            this.localeID = string.Empty;
            this.culture = CultureInfo.InvariantCulture;
            this.isReadOnly = isReadOnly;

            this.localeIdentifier = new LocaleID(string.Empty, string.Empty, string.Empty, string.Empty);

            // NOTE: Invariant culture is not neutral
            this.isNeutralCulture = localeIdentifier.IsNeutralCulture;
            this.isInvariantCulture = localeIdentifier.IsInvariantCulture;

            this.cultureData = cultureData!;
        }

        ///// <summary>
        ///// Construct a <see cref="UCultureInfo"/> from a <see cref="ToCultureInfo"/>.
        ///// </summary>
        ///// <param name="culture">A <see cref="ToCultureInfo"/>.</param>
        //private UCultureInfo(CultureInfo culture)
        //    : this(GetFullName(culture.ToUCultureInfo().ToString()), culture)
        //{
        //}

        /// <summary>
        /// <icu/> Constructs a <see cref="UCultureInfo"/> from a RFC 3066 locale ID. The <paramref name="name"/> consists
        /// of optional language, script, country, and variant fields in that order,
        /// separated by underscores, followed by an optional keyword list.  The
        /// script, if present, is four characters long-- this distinguishes it
        /// from a country code, which is two characters long.  Other fields
        /// are distinguished by position as indicated by the underscores.  The
        /// start of the keyword list is indicated by '@', and consists of two
        /// or more keyword/value pairs separated by semicolons(';').
        /// 
        /// <para/>This constructor does not canonicalize the <paramref name="name"/>. So, for
        /// example, "zh__pinyin" remains unchanged instead of converting
        /// to "zh@collation=pinyin".  By default ICU only recognizes the
        /// latter as specifying pinyin collation.  Use <see cref="CreateCanonical(string)"/>
        /// or <see cref="Canonicalize(string)"/> if you need to canonicalize the <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        public UCultureInfo(string name)
            : this(GetFullName(name), null)
        {
            this.culture = DotNetLocaleHelper.ToCultureInfo(this);
        }

        internal UCultureInfo(string name, bool isReadOnly, bool useDataCache)
        {
            Debug.Assert(name is not null);

            this.localeID = GetFullName(name) ?? string.Empty;

            this.isReadOnly = isReadOnly;

            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            this.localeIdentifier = parser.GetLocaleID();

            // NOTE: Invariant culture is not neutral
            this.isNeutralCulture = localeIdentifier.IsNeutralCulture;
            this.isInvariantCulture = localeIdentifier.IsInvariantCulture;

            this.cultureData = UCultureData.GetCultureData(this, useDataCache);

            this.culture = DotNetLocaleHelper.ToCultureInfo(this);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UCultureInfo"/> from the id by first canonicalizing the id.
        /// </summary>
        /// <param name="nonCanonicalID">The locale id to canonicalize.</param>
        /// <returns>The <see cref="UCultureInfo"/> created from the canonical version of the ID.</returns>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo CreateCanonical(string nonCanonicalID)
        {
            return new UCultureInfo(Canonicalize(nonCanonicalID)); // ICU4N: Call overload to create the culture based on canonical ID
        }

        private static string LscvToID(string? lang, string? script, string? country, string? variant)
        {
            using var buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            if (lang != null && lang.Length > 0)
            {
                buf.Append(lang);
            }
            if (script != null && script.Length > 0)
            {
                buf.Append(Underscore);
                buf.Append(script);
            }
            if (country != null && country.Length > 0)
            {
                buf.Append(Underscore);
                buf.Append(country);
            }
            if (variant != null && variant.Length > 0)
            {
                if (country == null || country.Length == 0)
                {
                    buf.Append(Underscore);
                }
                buf.Append(Underscore);
                buf.Append(variant);
            }
            return buf.ToString();
        }

        /// <summary>
        /// <icu/> Gets the corresponding <see cref="System.Globalization.CultureInfo"/>.
        /// This will be either a culture that exactly represents this one, or the closest
        /// approximation.
        /// </summary>
        /// <stable>ICU4N 60</stable>
        public CultureInfo ToCultureInfo() // ICU4N: Corresponds to toLocale() in ICU4J
        {
            //if (culture == null)
            //    culture = DotNetLocaleHelper.ToCultureInfo(this);

            return culture;
        }




        /// <summary>
        /// Compares two <see cref="UCultureInfo"/> for ordering.
        /// 
        /// <para/><b>Note:</b> The order might change in future.
        /// </summary>
        /// <param name="other">The <see cref="UCultureInfo"/> to be compared.</param>
        /// <returns>A negative integer, zero, or a positive integer as this <see cref="UCultureInfo"/>
        /// is less than, equal to, or greater than <paramref name="other"/>.</returns>
        /// <stable>ICU4N 60</stable>
        public int CompareTo(UCultureInfo? other)
        {
            if (other is null) return 1; // Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

            if (this == other)
                return 0;

            int cmp; // ICU4N: Removed unnecessary assignment

            // Language
            cmp = this.Language.CompareToOrdinal(other.Language);
            if (cmp == 0)
            {
                // Script
                cmp = this.Script.CompareToOrdinal(other.Script);
                if (cmp == 0)
                {
                    // Region
                    cmp = this.Country.CompareToOrdinal(other.Country);
                    if (cmp == 0)
                    {
                        // Variant
                        cmp = this.Variant.CompareToOrdinal(other.Variant);
                        if (cmp == 0)
                        {
                            // Keywords
                            using (var thisKwdItr = Keywords.GetEnumerator())
                            using (var otherKwdItr = other.Keywords.GetEnumerator())
                            {

                                if (thisKwdItr == null)
                                {
                                    cmp = otherKwdItr == null ? 0 : -1;
                                }
                                else if (otherKwdItr == null)
                                {
                                    cmp = 1;
                                }
                                else
                                {
                                    // Both have keywords
                                    while (cmp == 0 && thisKwdItr.MoveNext())
                                    {
                                        if (!otherKwdItr.MoveNext())
                                        {
                                            cmp = 1;
                                            break;
                                        }
                                        // Compare keyword keys
                                        string thisKey = thisKwdItr.Current.Key;
                                        string otherKey = otherKwdItr.Current.Key;
                                        cmp = thisKey.CompareToOrdinal(otherKey);
                                        if (cmp == 0)
                                        {
                                            // Compare keyword values
                                            string thisVal = thisKwdItr.Current.Value;
                                            string otherVal = otherKwdItr.Current.Value;
                                            if (thisVal == null)
                                            {
                                                cmp = otherVal == null ? 0 : -1;
                                            }
                                            else if (otherVal == null)
                                            {
                                                cmp = 1;
                                            }
                                            else
                                            {
                                                cmp = thisVal.CompareToOrdinal(otherVal);
                                            }
                                        }
                                    }
                                    if (cmp == 0 && otherKwdItr.MoveNext())
                                    {
                                        cmp = -1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Normalize the result value:
            // Note: string.compareTo() may return value other than -1, 0, 1.
            // A value other than those are OK by the definition, but we don't want
            // associate any semantics other than negative/zero/positive.
            return (cmp < 0) ? -1 : ((cmp > 0) ? 1 : 0);
        }


        /// <summary>
        /// <icunote/> Unlike the <see cref="System.Globalization.CultureInfo"/> API, this
        /// returns an array of <see cref="UCultureInfo"/>s, not <see cref="System.Globalization.CultureInfo"/>s.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>Returns a list of all installed locales.</returns>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            return ICUResourceBundle.GetUCultures(types);
        }

        internal bool IsMatch(UCultureTypes types)
        {
#if FEATURE_CULTUREINFO_GETCULTURES
            return ((int)CultureTypes & (int)types) != 0;
#else
            return isNeutralCulture && types.HasFlag(UCultureTypes.NeutralCultures)
                || !isNeutralCulture && types.HasFlag(UCultureTypes.SpecificCultures);
#endif
        }

        /// <summary>
        /// Returns a list of all 2-letter country codes defined in ISO 3166.
        /// Can be used to create Locales.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public static string[] GetISOCountries()
        {
            return LocaleIDs.GetISOCountries();
        }

        /// <summary>
        /// Returns a list of all 2-letter language codes defined in ISO 639
        /// plus additional 3-letter codes determined to be useful for locale generation as
        /// defined by Unicode CLDR.
        /// Can be used to create Locales.
        /// </summary>
        /// <remarks>
        /// [NOTE:  ISO 639 is not a stable standard-- some languages' codes have changed.
        /// The list this function returns includes both the new and the old codes for the
        /// languages whose codes have changed.]
        /// </remarks>
        /// <stable>ICU 3.0</stable>
        public static string[] GetISOLanguages()
        {
            return LocaleIDs.GetISOLanguages();
        }

        /// <summary>
        /// Returns the language code for this locale, which will either be the empty string
        /// or a lowercase ISO 639 code.
        /// </summary>
        /// <seealso cref="DisplayLanguage"/>
        /// <seealso cref="GetDisplayLanguage(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Language => Base.Language;

        /// <summary>
        /// Returns the language code for the locale ID,
        /// which will either be the empty string
        /// or a lowercase ISO 639 code.
        /// </summary>
        /// <param name="localeID"></param>
        /// <returns></returns>
        /// <seealso cref="DisplayLanguage"/>
        /// <seealso cref="GetDisplayLanguage(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public static string GetLanguage(string localeID)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.GetLanguage();
        }

        /// <summary>
        /// Returns the script code for this locale, which might be the empty string.
        /// </summary>
        /// <seealso cref="DisplayScript"/>
        /// <seealso cref="GetDisplayScript(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Script => Base.Script;

        /// <summary>
        /// <icu/> Returns the script code for the specified <paramref name="localeID"/>, 
        /// which might be the empty string.
        /// </summary>
        /// <param name="localeID"></param>
        /// <returns></returns>
        /// <seealso cref="DisplayScript"/>
        /// <seealso cref="GetDisplayScript(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public static string GetScript(string localeID)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.GetScript();
        }

        /// <summary>
        /// Returns the country/region code for this locale, which will either be the empty string
        /// or an uppercase ISO 3166 2-letter code.
        /// </summary>
        /// <seealso cref="DisplayCountry"/>
        /// <seealso cref="GetDisplayCountry(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Country => Base.Region;

        /// <summary>
        /// <icu/> Returns the country/region code for the specified <paramref name="localeID"/>,
        /// which will either be the empty string or an uppercase ISO 3166 2-letter code.
        /// </summary>
        /// <param name="localeID"></param>
        /// <returns></returns>
        /// <seealso cref="DisplayCountry"/>
        /// <seealso cref="GetDisplayCountry(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public static string GetCountry(string localeID)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.GetCountry();
        }

        /**
         * {@icu} Get the region to use for supplemental data lookup.
         * Uses
         * (1) any region specified by locale tag "rg"; if none then
         * (2) any unicode_region_tag in the locale ID; if none then
         * (3) if inferRegion is TRUE, the region suggested by
         *     getLikelySubtags on the localeID.
         * If no region is found, returns empty string ""
         *
         * @param locale
         *     The locale (includes any keywords) from which
         *     to get the region to use for supplemental data.
         * @param inferRegion
         *     If TRUE, will try to infer region from other
         *     locale elements if not found any other way.
         * @return
         *     string with region to use ("" if none found).
         * @internal ICU 57
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        internal static string GetRegionForSupplementalData(
            UCultureInfo locale, bool inferRegion) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            if (locale.Keywords.TryGetValue("rg", out string? region) && region != null && region.Length == 6)
            {
                string regionUpper = AsciiUtil.ToUpper(region);
                if (regionUpper.EndsWith("ZZZZ", StringComparison.Ordinal))
                {
                    return regionUpper.Substring(0, 2 - 0); // ICU4N: Checked 2nd parameter
                }
            }
            region = locale.Country;
            if (region.Length == 0 && inferRegion)
            {
                UCultureInfo maximized = AddLikelySubtags(locale);
                region = maximized.Country;
            }
            return region;
        }

        /// <summary>
        /// Returns the variant code for this locale, which might be the empty string.
        /// </summary>
        /// <seealso cref="DisplayVariant"/>
        /// <seealso cref="GetDisplayVariant(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Variant => Base.Variant;

        /// <summary>
        /// <icu/> Returns the variant code for the specified <paramref name="localeID"/>,
        /// which might be the empty string.
        /// </summary>
        /// <param name="localeID"></param>
        /// <returns></returns>
        /// <seealso cref="DisplayVariant"/>
        /// <seealso cref="GetDisplayVariant(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public static string GetVariant(string localeID)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.GetVariant();
        }

        /// <summary>
        /// Returns the fallback locale (parent) for this locale. If this locale is root,
        /// returns <c>null</c>.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        internal UCultureInfo? GetParent() // ICU4N: Exposed through Parent property, but in that case we never have null
        {
            if (localeID.Length == 0 || localeID[0] == '@')
            {
                return null;
            }
            return new UCultureInfo(GetParentString(localeID));
        }

        /// <summary>
        /// <icu/> Returns the fallback locale (parent) for the specified locale, which might be the
        /// empty string.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public static string GetParent(string localeID) // ICU4N - Renamed from GetFallback
        {
            return GetParentString(GetFullName(localeID));
        }

        /// <summary>
        /// Returns the given (canonical) locale id minus the last part before the tags.
        /// </summary>
        // ICU4N TODO: Should we be using - instead of _ here?
        private static string GetParentString(string fallback) // ICU4N - Renamed from GetFallbackString
        {
            int extStart = fallback.IndexOf('@');
            if (extStart == -1)
            {
                extStart = fallback.Length;
            }

            // ICU4N: Using LocaleIDParser for more accurate results
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], fallback);
            int bufferLength = fallback.Length + 5;
            bool usePool = bufferLength > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(bufferLength) : null;
            try
            {
                Span<char> result = usePool ? arrayToReturnToPool : stackalloc char[bufferLength];
                int totalLength = 0, lastLength = 0;

                parser.GetLanguage(result, out int languageLength);
                if (languageLength > 0)
                {
                    totalLength += languageLength;
                    lastLength = languageLength;
                }
                parser.GetScript(result.Slice(totalLength + 1), out int scriptLength);
                if (scriptLength > 0)
                {
                    result[totalLength] = '_';
                    totalLength += scriptLength + 1;
                    lastLength = scriptLength + 1;
                }
                parser.GetCountry(result.Slice(totalLength + 1), out int countryLength);
                if (countryLength > 0)
                {
                    result[totalLength] = '_';
                    totalLength += countryLength + 1;
                    lastLength = countryLength + 1;
                }
                parser.GetVariant(result.Slice(totalLength + 1), out int variantLength);
                if (variantLength > 0)
                {
                    result[totalLength] = '_';
                    totalLength += variantLength + 1;
                    lastLength = variantLength + 1;
                }

                totalLength -= lastLength; // Remove the last segment

                // Append the ext chars, if any
                ReadOnlySpan<char> ext = fallback.AsSpan(extStart);
                if (ext.Length > 0)
                {
                    ext.CopyTo(result.Slice(totalLength));
                    totalLength += ext.Length;
                }

                return result.Slice(0, totalLength).ToString();
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }

        /// <summary>
        /// <icu/> Returns the (normalized) base name for the specified locale,
        /// like <see cref="GetFullName(string)"/>, but without keywords.
        /// </summary>
        /// <param name="localeID">The locale ID as a string.</param>
        /// <returns>The base name as a string.</returns>
        /// <stable>ICU 3.0</stable>
        public static string GetName(string localeID) // ICU4N specific: Renamed from getBaseName()
        {
            if (localeID.IndexOf('@') == -1)
            {
                return localeID;
            }
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.GetBaseName();
        }

        /// <summary>
        /// <icu/> Returns the (normalized) full name for this culture.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        // ICU4N specific: Renamed from Name (and BaseName was renamed to Name, since it does not contain keywords the same as .NET).
        public string FullName => localeID; // always normalized

        /// <summary>
        /// Gets the shortest length subtag's size.
        /// </summary>
        /// <param name="localeID">The localeID as a string.</param>
        /// <returns>The size of the shortest length subtag.</returns>
        private static int GetShortestSubtagLength(string localeID)
        {
            int localeIDLength = localeID.Length;
            int length = localeIDLength;
            bool reset = true;
            int tmpLength = 0;

            for (int i = 0; i < localeIDLength; i++)
            {
                if (localeID[i] != '_' && localeID[i] != '-')
                {
                    if (reset)
                    {
                        reset = false;
                        tmpLength = 0;
                    }
                    tmpLength++;
                }
                else
                {
                    if (tmpLength != 0 && tmpLength < length)
                    {
                        length = tmpLength;
                    }
                    reset = true;
                }
            }

            return length;
        }

        /// <summary>
        /// <icu/> Returns the (normalized) full name for the specified locale.
        /// </summary>
        /// <param name="name">The localeID as a string.</param>
        /// <returns>The full name of the localeID.</returns>
        /// <stable>ICU 3.0</stable>
        public static string GetFullName(string? name) // ICU4N specific - renamed from GetName() in ICU4J (since this would cause a collision with BaseName that has no keywords)
        {
            string? tmpLocaleID;
            // Convert BCP47 id if necessary
            if (name != null && !name.Contains('@') && GetShortestSubtagLength(name) == 1)
            {
                tmpLocaleID = GetCultureInfoByIetfLanguageTag(name).FullName;
                if (tmpLocaleID.Length == 0)
                {
                    tmpLocaleID = name;
                }
            }
            else
            {
                tmpLocaleID = name;
            }
            return nameCache.GetOrCreate(tmpLocaleID!, (key) => GetFullName(key));

            static string GetFullName(string key)
            {
                using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], key);
                return parser.GetFullName();
            }
        }

        /// <summary>
        /// <icu/> Gets a read-only dictionary of keywords and values for this locale.
        /// </summary>
        /// <stable>ICU 60</stable>
#if FEATURE_IREADONLYCOLLECTIONS
        public IReadOnlyDictionary<string, string> Keywords
#else
        public IDictionary<string, string> Keywords
#endif
        {
            get
            {
                keywords ??= GetKeywords(localeID);
                return keywords;
            }
        }

        /// <summary>
        /// <icu/> Gets a read-only dictionary of keywords and values for the
        /// specified <paramref name="localeID"/>.
        /// </summary>
        /// <param name="localeID">The locale ID.</param>
        /// <returns>A read-only dictionary of keywords and values.</returns>
        /// <stable>ICU 60</stable>
#if FEATURE_IREADONLYCOLLECTIONS
        public static IReadOnlyDictionary<string, string> GetKeywords(string localeID)
#else
        public static IDictionary<string, string> GetKeywords(string localeID)
#endif
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            return parser.Keywords;
        }

        // ICU4N specific - Removed GetKeywordValue() methods because they are redundant with the
        // Keywords property in the current culture. For other cultures, the user can use
        // new LocaleIDParser(localeID).GetKeywordValue(keywordName);

        /// <summary>
        /// <icu/> Returns the canonical name for the specified locale ID.  This is used to
        /// convert POSIX and other grandfathered IDs to standard ICU form.
        /// </summary>
        /// <param name="localeID">The locale ID.</param>
        /// <returns>The canonicalized ID.</returns>
        /// <stable>ICU 3.0</stable>
        /// <exception cref="ArgumentNullException"><paramref name="localeID"/> is <c>null</c>.</exception>
        public static string Canonicalize(string localeID)
        {
            if (localeID is null)
                throw new ArgumentNullException(nameof(localeID));

            return Canonicalize(localeID.AsSpan());
        }

        /// <summary>
        /// <icu/> Returns the canonical name for the specified locale ID.  This is used to
        /// convert POSIX and other grandfathered IDs to standard ICU form.
        /// </summary>
        /// <param name="localeID">The locale ID.</param>
        /// <returns>The canonicalized ID.</returns>
        /// <stable>ICU 60.1</stable>
        private static string Canonicalize(ReadOnlySpan<char> localeID)
        {
            int bufferLength = localeID.Length + 10;
            bool usePool = bufferLength > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(bufferLength) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[bufferLength];

                if (TryCanonicalize(localeID, buffer, out int charsWritten))
                {
                    return buffer.Slice(0, charsWritten).ToString();
                }
                else // rare
                {
                    while (true)
                    {
                        usePool = true;
                        bufferLength += 1024;
                        buffer = arrayToReturnToPool = ArrayPool<char>.Shared.Rent(bufferLength);
                        if (TryCanonicalize(localeID, buffer, out charsWritten))
                            return buffer.Slice(0, charsWritten).ToString();
                    }
                }
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }

        /// <summary>
        /// <icu/> Copies the canonical name for the specified locale ID to <paramref name="destination"/>.
        /// This is used to convert POSIX and other grandfathered IDs to standard ICU form.
        /// </summary>
        /// <param name="localeID">The locale ID.</param>
        /// <param name="destination">The span in which to write the canonical name as a span of characters. Using a span
        /// of at least 10 more characters than <paramref name="localeID"/> is recommended.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in
        /// <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        /// <stable>ICU 60.1</stable>
        public static bool TryCanonicalize(ReadOnlySpan<char> localeID, Span<char> destination, out int charsWritten)
        {
            using LocaleIDParser parser = new LocaleIDParser(stackalloc char[CharStackBufferSize],
                localeID, canonicalize: true);
            ReadOnlySpan<char> baseName = parser.GetBaseNameAsSpan();
            bool foundVariant = false;

            // formerly, we always set to en_US_POSIX if the basename was empty, but
            // now we require that the entire id be empty, so that "@foo=bar"
            // will pass through unchanged.
            // {dlf} I'd rather keep "" unchanged.
            if (localeID.IsEmpty)
            {
                bool success = ReadOnlySpan<char>.Empty.TryCopyTo(destination);
                charsWritten = 0;
                return success;
            }

            Span<char> buffer = stackalloc char[CharStackBufferSize];

            // we have an ID in the form xx_Yyyy_ZZ_KKKKK

            /* convert the variants to appropriate ID */
            for (int i = 0; i < variantsToKeywords.Length; i++)
            {
                string[] vals = variantsToKeywords[i];
                buffer[0] = '_';
                vals[0].CopyTo(buffer.Slice(1));
                int idx = baseName.LastIndexOf(buffer.Slice(0, vals[0].Length + 1)); // ICU4N: Defaults to ordinal (overload missing in .NET Framework)
                if (idx > -1)
                {
                    foundVariant = true;

                    baseName = baseName.Slice(0, idx - 0); // ICU4N: Checked 2nd parameter
                    if (baseName.EndsWith("_", StringComparison.Ordinal))
                    {
                        baseName = baseName.Slice(0, (--idx - 0)); // ICU4N: Checked 2nd parameter
                    }
                    parser.SetBaseName(baseName);
                    parser.DefaultKeywordValue(vals[1], vals[2]);
                    break;
                }
            }

            /* See if this is an already known locale */
            for (int i = 0; i < CANONICALIZE_MAP.Length; i++)
            {
                if (baseName.Equals(CANONICALIZE_MAP[i][0]!, StringComparison.Ordinal))
                {
                    foundVariant = true;

                    string?[] vals = CANONICALIZE_MAP[i];
                    parser.SetBaseName(vals[1]);
                    if (vals[2] != null)
                    {
                        parser.DefaultKeywordValue(vals[2], vals[3]);
                    }
                    break;
                }
            }

            /* total mondo hack for Norwegian, fortunately the main NY case is handled earlier */
            if (!foundVariant)
            {
                if (parser.TryGetLanguage(buffer, out int languageLength) && buffer.Slice(0, languageLength).SequenceEqual("nb".AsSpan())
                    && parser.TryGetVariant(buffer, out int variantLength) && buffer.Slice(0, variantLength).SequenceEqual("NY".AsSpan()))
                {
                    parser.SetBaseName(LscvToID("nn", parser.GetScript(), parser.GetCountry(), null));
                }
            }
            return parser.TryGetFullName(destination, out charsWritten);
        }

        /// <summary>
        /// <icu/> Given a <paramref name="keyword"/> and a <paramref name="value"/>, return a new locale with an updated
        /// keyword and value.  If the keyword is <c>null</c>, this removes all keywords from the locale id.
        /// Otherwise, if the value is <c>null</c>, this removes the value for this keyword from the
        /// locale id.  Otherwise, this adds/replaces the value for this keyword in the locale id.
        /// The keyword and value must not be empty.
        /// 
        /// <para/>Related: <see cref="Name"/> returns the locale ID string with all keywords removed.
        /// </summary>
        /// <param name="keyword">The keyword to add/remove, or <c>null</c> to remove all keywords.</param>
        /// <param name="value">The value to add/set, or <c>null</c> to remove this particular keyword.</param>
        /// <returns>The updated locale.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyword"/> is an empty string.
        /// </exception>
        /// <stable>ICU 3.2</stable>
        public UCultureInfo SetKeywordValue(string keyword, string value)
        {
            return new UCultureInfo(SetKeywordValue(localeID, keyword, value));
        }

        /// <summary>
        /// Given a <paramref name="localeID"/>, a <paramref name="keyword"/>, and a <paramref name="value"/>,
        /// return a new locale id with an updated
        /// keyword and value.  If the keyword is <c>null</c>, this removes all keywords from the locale id.
        /// Otherwise, if the value is <c>null</c>, this removes the value for this keyword from the
        /// locale id. Otherwise, this adds/replaces the value for this keyword in the locale id.
        /// The keyword and value must not be empty.
        /// 
        /// <para/>Related: <see cref="Name"/> returns the locale ID string with all keywords removed.
        /// </summary>
        /// <param name="localeID">The locale id to modify.</param>
        /// <param name="keyword">The keyword to add/remove, or <c>null</c> to remove all keywords.</param>
        /// <param name="value">The value to add/set, or <c>null</c> to remove this particular keyword.</param>
        /// <returns>The updated locale id.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="localeID"/> or <paramref name="keyword"/> is an empty string.
        /// </exception>
        /// <stable>ICU 3.2</stable>
        public static string SetKeywordValue(string localeID, string keyword, string value)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            parser.SetKeywordValue(keyword, value);
            return parser.GetFullName();
        }

        /// <summary>
        /// <icu/> Returns a three-letter abbreviation for the specified <paramref name="localeID"/>'s language.
        /// If language is empty, returns the empty string. Otherwise, returns
        /// a lowercase ISO 639-2/T language code.
        /// </summary>
        /// <remarks>The ISO 639-2 language codes can be found on-line at
        /// <a href="ftp://dkuug.dk/i18n/iso-639-2.txt">ftp://dkuug.dk/i18n/iso-639-2.txt</a>.
        /// </remarks>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter language abbreviation is not available for this locale.</exception>
        /// <stable>ICU 3.0</stable>
        public static string GetThreeLetterISOLanguageName(string localeID)
        {
            return LocaleIDs.GetThreeLetterISOLanguageName(GetLanguage(localeID));
        }

        /// <summary>
        /// <icu/> Returns a two-letter abbreviation for the specified <paramref name="localeID"/>'s language.
        /// If language is empty, returns the empty string. Otherwise, returns
        /// a lowercase ISO 639-1/T language code.
        /// </summary>
        /// <remarks>The ISO 639-1 language codes can be found on-line at
        /// <a href="ftp://dkuug.dk/i18n/iso-639-1.txt">ftp://dkuug.dk/i18n/iso-639-1.txt</a>.
        /// </remarks>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter language abbreviation is not available for this locale.</exception>
        /// <draft>ICU 60</draft>
        public static string GetTwoLetterISOLanguageName(string localeID)
        {
            return LocaleIDs.ThreeToTwoLetterLanguage(GetThreeLetterISOLanguageName(localeID));
        }

        /// <summary>
        /// <icu/> Returns a three-letter abbreviation for this locale's country/region.  If the locale
        /// doesn't specify a country, returns the empty string. Otherwise, returns
        /// an uppercase ISO 3166-3 3-letter country code.
        /// </summary>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter country abbreviation is not available for this locale.</exception>
        /// <stable>ICU 3.0</stable>
        public string ThreeLetterISOCountryName
            => GetThreeLetterISOCountryName(localeID);

        /// <summary>
        /// <icu/> Returns a three-letter abbreviation for the specified <paramref name="localeID"/>'s country/region.
        /// If the locale doesn't specify a country, returns the empty string. Otherwise, returns
        /// an uppercase ISO 3166-3 3-letter country code.
        /// </summary>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter country abbreviation is not available for this locale.</exception>
        /// <stable>ICU 3.0</stable>
        public static string GetThreeLetterISOCountryName(string localeID)
        {
            return LocaleIDs.GetThreeLetterISOCountryName(GetCountry(localeID));
        }

        /// <summary>
        /// <icu/> Returns a three-letter abbreviation for this locale's country/region.  If the locale
        /// doesn't specify a country, returns the empty string. Otherwise, returns
        /// an uppercase ISO 3166-2 2-letter country code.
        /// </summary>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter country abbreviation is not available for this locale.</exception>
        /// <stable>ICU 60</stable>
        public string TwoLetterISOCountryName // ICU4N specific
            => GetTwoLetterISOCountryName(localeID);

        /// <summary>
        /// <icu/> Returns a three-letter abbreviation for the specified <paramref name="localeID"/>'s country/region.
        /// If the locale doesn't specify a country, returns the empty string. Otherwise, returns
        /// an uppercase ISO 3166-2 2-letter country code.
        /// </summary>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter country abbreviation is not available for this locale.</exception>
        /// <stable>ICU 60</stable>
        public static string GetTwoLetterISOCountryName(string localeID)
        {
            return LocaleIDs.ThreeToTwoLetterRegion(GetThreeLetterISOCountryName(localeID));
        }

        /// <summary>
        /// Pairs of (language subtag, + or -) for finding out fast if common languages
        /// are LTR (minus) or RTL (plus).
        /// </summary>
        private const string LANG_DIR_STRING =
                    "root-en-es-pt-zh-ja-ko-de-fr-it-ar+he+fa+ru-nl-pl-th-tr-";

        // ICU4N TODO: API - Add to UTextInfo class that is exposed through the TextInfo property
        /// <summary>
        /// <icu/> Gets whether this locale's script is written right-to-left.
        /// If there is no script subtag, then the likely script is used,
        /// see <see cref="AddLikelySubtags(UCultureInfo)"/>.
        /// If no likely script is known, then <c>false</c> is returned.
        /// 
        /// <para/>A script is right-to-left according to the CLDR script metadata
        /// which corresponds to whether the script's letters have Bidi_Class=R or AL.
        /// 
        /// <para/>Returns true for "ar" and "en-Hebr", false for "zh" and "fa-Cyrl".
        /// </summary>
        /// <stable>ICU 54</stable>
        internal bool IsRightToLeft
        {
            get
            {
                string script = Script;
                if (script.Length == 0)
                {
                    // Fastpath: We know the likely scripts and their writing direction
                    // for some common languages.
                    string lang = Language;
                    if (lang.Length == 0)
                    {
                        return false;
                    }
                    int langIndex = LANG_DIR_STRING.IndexOf(lang, StringComparison.Ordinal);
                    if (langIndex >= 0)
                    {
                        switch (LANG_DIR_STRING[langIndex + lang.Length])
                        {
                            case '-': return false;
                            case '+': return true;
                            default: break;  // partial match of a longer code
                        }
                    }
                    // Otherwise, find the likely script.
                    UCultureInfo likely = AddLikelySubtags(this);
                    script = likely.Script;
                    if (script.Length == 0)
                    {
                        return false;
                    }
                }
                int scriptCode = UScript.GetCodeFromName(script);
                return UScript.IsRightToLeft(scriptCode);
            }
        }


        // display names

        /// <summary>
        /// Gets this locale's language localized for display in the <see cref="CultureInfo.CurrentUICulture"/> locale.
        /// </summary>
        /// <seealso cref="CultureInfo.CurrentUICulture"/>
        /// <stable>ICU 3.0</stable>
        public string DisplayLanguage
            => GetDisplayLanguageInternal(this, CurrentUICulture, false);

        /// <summary>
        /// Returns this locale's language localized for display in the provided locale.
        /// </summary>
        /// <param name="displayLocale">The locale in which to display the name.</param>
        /// <returns>The localized language name.</returns>
        /// <stable>ICU 3.0</stable>
        public string GetDisplayLanguage(UCultureInfo displayLocale)
        {
            return GetDisplayLanguageInternal(this, displayLocale, false);
        }

        /**
         * {@icu} Returns a locale's language localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose language will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 3.0
         */
        public static string GetDisplayLanguage(string localeID, string displayLocaleID)
        {
            return GetDisplayLanguageInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID),
                    false);
        }

        /**
         * {@icu} Returns a locale's language localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose language will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 3.0
         */
        public static string GetDisplayLanguage(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayLanguageInternal(new UCultureInfo(localeID), displayLocale, false);
        }
        /**
         * {@icu} Returns this locale's language localized for display in the default <code>DISPLAY</code> locale.
         * If a dialect name is present in the data, then it is returned.
         * @return the localized language name.
         * @see Category#DISPLAY
         * @stable ICU 4.4
         */
        public string GetDisplayLanguageWithDialect()
        {
            return GetDisplayLanguageInternal(this, CurrentUICulture, true);
        }

        /**
         * {@icu} Returns this locale's language localized for display in the provided locale.
         * If a dialect name is present in the data, then it is returned.
         * @param displayLocale the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 4.4
         */
        public string GetDisplayLanguageWithDialect(UCultureInfo displayLocale)
        {
            return GetDisplayLanguageInternal(this, displayLocale, true);
        }

        /**
         * {@icu} Returns a locale's language localized for display in the provided locale.
         * If a dialect name is present in the data, then it is returned.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose language will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 4.4
         */
        public static string GetDisplayLanguageWithDialect(string localeID, string displayLocaleID)
        {
            return GetDisplayLanguageInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID),
                    true);
        }

        /**
         * {@icu} Returns a locale's language localized for display in the provided locale.
         * If a dialect name is present in the data, then it is returned.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose language will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 4.4
         */
        public static string GetDisplayLanguageWithDialect(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayLanguageInternal(new UCultureInfo(localeID), displayLocale, true);
        }

        private static string GetDisplayLanguageInternal(UCultureInfo locale, UCultureInfo displayLocale,
                bool useDialect)
        {
            string lang = useDialect ? locale.Name : locale.Language;
            return CultureDisplayNames.GetInstance(displayLocale).GetLanguageDisplayName(lang);
        }

        /**
         * Returns this locale's script localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized script name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string DisplayScript
            => GetDisplayScriptInternal(this, CurrentUICulture);

        /**
         * {@icu} Returns this locale's script localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized script name.
         * @see Category#DISPLAY
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //[Obsolete("This API is ICU internal only.")]
        internal string DisplayScriptInContext
            => GetDisplayScriptInContextInternal(this, CurrentUICulture);

        /**
         * Returns this locale's script localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @stable ICU 3.0
         */
        public string GetDisplayScript(UCultureInfo displayLocale)
        {
            return GetDisplayScriptInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns this locale's script localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //[Obsolete("This API is ICU internal only.")]
        internal string GetDisplayScriptInContext(UCultureInfo displayLocale)
        {
            return GetDisplayScriptInContextInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose script will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized script name.
         * @stable ICU 3.0
         */
        public static string GetDisplayScript(string localeID, string displayLocaleID)
        {
            return GetDisplayScriptInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID));
        }
        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose script will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized script name.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //[Obsolete("This API is ICU internal only.")]
        internal static string GetDisplayScriptInContext(string localeID, string displayLocaleID)
        {
            return GetDisplayScriptInContextInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID));
        }

        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * @param localeID the id of the locale whose script will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @stable ICU 3.0
         */
        public static string GetDisplayScript(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayScriptInternal(new UCultureInfo(localeID), displayLocale);
        }
        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * @param localeID the id of the locale whose script will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //[Obsolete("This API is ICU internal only.")]
        internal static string GetDisplayScriptInContext(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayScriptInContextInternal(new UCultureInfo(localeID), displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayScriptInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale)
                    .GetScriptDisplayName(locale.Script);
        }

        private static string GetDisplayScriptInContextInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
#pragma warning disable 612, 618
            return CultureDisplayNames.GetInstance(displayLocale)
                    .GetScriptDisplayNameInContext(locale.Script);
#pragma warning restore 612, 618
        }

        /**
         * Returns this locale's country localized for display in the default <code>DISPLAY</code> locale.
         * <b>Warning: </b>this is for the region part of a valid locale ID; it cannot just be the region code (like "FR").
         * To get the display name for a region alone, or for other options, use {@link LocaleDisplayNames} instead.
         * @return the localized country name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string DisplayCountry
            => GetDisplayCountryInternal(this, CurrentUICulture);


        /**
         * Returns this locale's country localized for display in the provided locale.
         * <b>Warning: </b>this is for the region part of a valid locale ID; it cannot just be the region code (like "FR").
         * To get the display name for a region alone, or for other options, use {@link LocaleDisplayNames} instead.
         * @param displayLocale the locale in which to display the name.
         * @return the localized country name.
         * @stable ICU 3.0
         */
        public string GetDisplayCountry(UCultureInfo displayLocale)
        {
            return GetDisplayCountryInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns a locale's country localized for display in the provided locale.
         * <b>Warning: </b>this is for the region part of a valid locale ID; it cannot just be the region code (like "FR").
         * To get the display name for a region alone, or for other options, use {@link LocaleDisplayNames} instead.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose country will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized country name.
         * @stable ICU 3.0
         */
        public static string GetDisplayCountry(string localeID, string displayLocaleID)
        {
            return GetDisplayCountryInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID));
        }

        /**
         * {@icu} Returns a locale's country localized for display in the provided locale.
         * <b>Warning: </b>this is for the region part of a valid locale ID; it cannot just be the region code (like "FR").
         * To get the display name for a region alone, or for other options, use {@link LocaleDisplayNames} instead.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose country will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized country name.
         * @stable ICU 3.0
         */
        public static string GetDisplayCountry(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayCountryInternal(new UCultureInfo(localeID), displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayCountryInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale)
                    .GetRegionDisplayName(locale.Country);
        }

        /**
         * Returns this locale's variant localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized variant name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string DisplayVariant
            => GetDisplayVariantInternal(this, CurrentUICulture);

        /**
         * Returns this locale's variant localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized variant name.
         * @stable ICU 3.0
         */
        public string GetDisplayVariant(UCultureInfo displayLocale)
        {
            return GetDisplayVariantInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns a locale's variant localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose variant will be displayed
         * @param displayLocaleID the id of the locale in which to display the name.
         * @return the localized variant name.
         * @stable ICU 3.0
         */
        public static string GetDisplayVariant(string localeID, string displayLocaleID)
        {
            return GetDisplayVariantInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID));
        }

        /**
         * {@icu} Returns a locale's variant localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose variant will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized variant name.
         * @stable ICU 3.0
         */
        public static string GetDisplayVariant(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayVariantInternal(new UCultureInfo(localeID), displayLocale);
        }

        private static string GetDisplayVariantInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale)
                    .GetVariantDisplayName(locale.Variant);
        }


        /// <summary>
        /// <icu/> Returns a keyword localized for display in the <see cref="UCultureInfo.CurrentUICulture"/> locale.
        /// </summary>
        /// <param name="keyword">The keyword to be displayed.</param>
        /// <returns>The localized keyword name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <seealso cref="CultureInfo.CurrentUICulture"/>
        /// <stable>ICU 3.0</stable>
        public static string GetDisplayKeyword(string keyword)
        {
            return GetDisplayKeywordInternal(keyword, CurrentUICulture);
        }

        /// <summary>
        /// <icu/> Returns a keyword localized for display in the specified locale.
        /// </summary>
        /// <param name="keyword">The keyword to be displayed.</param>
        /// <param name="displayLocaleID">The id of the locale in which to display the keyword.</param>
        /// <returns>The localized keyword name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public static string GetDisplayKeyword(string keyword, string displayLocaleID)
        {
            return GetDisplayKeywordInternal(keyword, new UCultureInfo(displayLocaleID));
        }

        /// <summary>
        /// <icu/> Returns a keyword localized for display in the specified locale.
        /// </summary>
        /// <param name="keyword">The keyword to be displayed.</param>
        /// <param name="displayLocale">The <see cref="UCultureInfo"/> in which to display the keyword.</param>
        /// <returns>The localized keyword name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public static string GetDisplayKeyword(string keyword, UCultureInfo displayLocale)
        {
            return GetDisplayKeywordInternal(keyword, displayLocale);
        }

        private static string GetDisplayKeywordInternal(string keyword, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale).GetKeyDisplayName(keyword);
        }

        /// <summary>
        /// <icu/> Returns a keyword value localized for display in the <see cref="UCultureInfo.CurrentUICulture"/> locale.
        /// </summary>
        /// <param name="keyword">The keyword whose value is to be displayed.</param>
        /// <returns>The localized value name.</returns>
        /// <seealso cref="UCultureInfo.CurrentUICulture"/>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public string GetDisplayKeywordValue(string keyword)
        {
            return GetDisplayKeywordValueInternal(this, keyword, CurrentUICulture);
        }

        /// <summary>
        /// <icu/> Returns a keyword value localized for display in the specified locale.
        /// </summary>
        /// <param name="keyword">The keyword whose value is to be displayed.</param>
        /// <param name="displayLocale">The locale in which to display the value.</param>
        /// <returns>The localized value name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public string GetDisplayKeywordValue(string keyword, UCultureInfo displayLocale)
        {
            return GetDisplayKeywordValueInternal(this, keyword, displayLocale);
        }

        /// <summary>
        /// <icu/> Returns a keyword value localized for display in the specified locale.
        /// This is a cover for the ICU4C API.
        /// </summary>
        /// <param name="localeID">The id of the locale whose keyword value is to be displayed.</param>
        /// <param name="keyword">The keyword whose value is to be displayed.</param>
        /// <param name="displayLocaleID">The id of the locale in which to display the value.</param>
        /// <returns>The localized value name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public static string GetDisplayKeywordValue(string localeID, string keyword,
                string displayLocaleID)
        {
            return GetDisplayKeywordValueInternal(new UCultureInfo(localeID), keyword,
                    new UCultureInfo(displayLocaleID));
        }

        /// <summary>
        /// <icu/> Returns a keyword value localized for display in the specified locale.
        /// This is a cover for the ICU4C API.
        /// </summary>
        /// <param name="localeID">The id of the locale whose keyword value is to be displayed.</param>
        /// <param name="keyword">The keyword whose value is to be displayed.</param>
        /// <param name="displayLocale">The <see cref="UCultureInfo"/> in which to display the value.</param>
        /// <returns>The localized value name.</returns>
        /// <seealso cref="GetKeywords(string)"/>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public static string GetDisplayKeywordValue(string localeID, string keyword,
                UCultureInfo displayLocale)
        {
            return GetDisplayKeywordValueInternal(new UCultureInfo(localeID), keyword, displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayKeywordValueInternal(UCultureInfo locale, string keyword,
                UCultureInfo displayLocale)
        {
            keyword = AsciiUtil.ToLower(keyword.Trim());
            locale.Keywords.TryGetValue(keyword, out string? value); // ICU4N TODO: If this returns a null value, the below line will throw NullReferenceException. Need to find a solution.
            return CultureDisplayNames.GetInstance(displayLocale).GetKeyValueDisplayName(keyword, value);
        }


        // ICU4N TODO: Figure out what to namme this, it behaves differently than .NET DisplayName
        //public string DisplayName2 => GetDisplayNameInternal(this, CurrentUICulture);

        /**
 * Returns this locale name localized for display in the provided locale.
 * @param displayLocale the locale in which to display the locale name.
 * @return the localized locale name.
 * @stable ICU 3.0
 */
        public string GetDisplayName(UCultureInfo displayLocale)
        {
            return GetDisplayNameInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns the locale ID localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the locale whose name is to be displayed.
         * @param displayLocaleID the id of the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 3.0
         */
        public static string GetDisplayName(string localeID, string displayLocaleID)
        {
            return GetDisplayNameInternal(new UCultureInfo(localeID), new UCultureInfo(displayLocaleID));
        }

        /**
         * {@icu} Returns the locale ID localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the locale whose name is to be displayed.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 3.0
         */
        public static string GetDisplayName(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayNameInternal(new UCultureInfo(localeID), displayLocale);
        }

        private static string GetDisplayNameInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale).GetLocaleDisplayName(locale);
        }

        /**
         * {@icu} Returns this locale name localized for display in the default <code>DISPLAY</code> locale.
         * If a dialect name is present in the locale data, then it is returned.
         * @return the localized locale name.
         * @see Category#DISPLAY
         * @stable ICU 4.4
         */
        public string DisplayNameWithDialect
            => GetDisplayNameWithDialectInternal(this, CurrentUICulture);

        /**
         * {@icu} Returns this locale name localized for display in the provided locale.
         * If a dialect name is present in the locale data, then it is returned.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 4.4
         */
        public string GetDisplayNameWithDialect(UCultureInfo displayLocale)
        {
            return GetDisplayNameWithDialectInternal(this, displayLocale);
        }

        /**
         * {@icu} Returns the locale ID localized for display in the provided locale.
         * If a dialect name is present in the locale data, then it is returned.
         * This is a cover for the ICU4C API.
         * @param localeID the locale whose name is to be displayed.
         * @param displayLocaleID the id of the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 4.4
         */
        public static string GetDisplayNameWithDialect(string localeID, string displayLocaleID)
        {
            return GetDisplayNameWithDialectInternal(new UCultureInfo(localeID),
                    new UCultureInfo(displayLocaleID));
        }

        /**
         * {@icu} Returns the locale ID localized for display in the provided locale.
         * If a dialect name is present in the locale data, then it is returned.
         * This is a cover for the ICU4C API.
         * @param localeID the locale whose name is to be displayed.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 4.4
         */
        public static string GetDisplayNameWithDialect(string localeID, UCultureInfo displayLocale)
        {
            return GetDisplayNameWithDialectInternal(new UCultureInfo(localeID), displayLocale);
        }

        private static string GetDisplayNameWithDialectInternal(UCultureInfo locale, UCultureInfo displayLocale)
        {
            return CultureDisplayNames.GetInstance(displayLocale, DialectHandling.DialectNames)
                    .GetLocaleDisplayName(locale);
        }

        /**
         * {@icu} Returns this locale's layout orientation for characters.  The possible
         * values are "left-to-right", "right-to-left", "top-to-bottom" or
         * "bottom-to-top".
         * @return The locale's layout orientation for characters.
         * @stable ICU 4.0
         */
        internal string CharacterOrientation // ICU4N TODO: API - Move to TextInfo class that is returned by TextInfo property, change to return enum value
            => ICUResourceTableAccess.GetTableString(ICUData.IcuBaseName, this,
                    "layout", "characters", "characters");

        /**
         * {@icu} Returns this locale's layout orientation for lines.  The possible
         * values are "left-to-right", "right-to-left", "top-to-bottom" or
         * "bottom-to-top".
         * @return The locale's layout orientation for lines.
         * @stable ICU 4.0
         */
        internal string LineOrientation // ICU4N TODO: API - Move to TextInfo class that is returned by TextInfo property, change to return enum value
            => ICUResourceTableAccess.GetTableString(ICUData.IcuBaseName, this,
                    "layout", "lines", "lines");

        /// <summary>
        /// <icu/> Based on an HTTP formatted list of acceptable cultures, determine an available
        /// culture for the user. <paramref name="isFallback"/> will be <c>true</c> if a
        /// fallback culture (one not in the <paramref name="acceptLanguageList"/>) was returned.
        /// The return value will be one of the cultures in <paramref name="availableCultures"/>, or
        /// <see cref="CultureInfo.InvariantCulture"/> if an invariant culture was used as a fallback
        /// (because nothing else in <paramref name="availableCultures"/> matched). No
        /// <see cref="UCultureInfo"/> in <paramref name="availableCultures"/> should be <c>null</c>;
        /// an <see cref="ArgumentException"/> will be thrown in this case.
        /// </summary>
        /// <param name="acceptLanguageList">List in HTTP "Accept-Language:" format of acceptable cultures.</param>
        /// <param name="availableCultures">List of available cultures. One of these will be returned.</param>
        /// <param name="isFallback">Returns the fallback status.</param>
        /// <returns>One of the cultures from the <paramref name="availableCultures"/> list, or <c>null</c> if none match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="availableCultures"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">One of the elements of <paramref name="availableCultures"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
        public static UCultureInfo? AcceptLanguage(string acceptLanguageList, IList<UCultureInfo> availableCultures,
            out bool isFallback) // ICU4N TODO: API - review whether returning null here makes sense
        {
            if (acceptLanguageList == null)
                throw new ArgumentNullException(nameof(acceptLanguageList));
            if (availableCultures == null)
                throw new ArgumentNullException(nameof(availableCultures));

            if (!TryParseAcceptLanguage(acceptLanguageList, true, out IList<UCultureInfo>? acceptList))
            {
                isFallback = false;
                return null;
            }
            return AcceptLanguage(acceptList, availableCultures, out isFallback);
        }

        /// <summary>
        /// <icu/> Based on an ordered list of acceptable cultures, determine an available
        /// culture for the user. <paramref name="isFallback"/> will be <c>true</c> if a
        /// fallback culture (one not in the <paramref name="acceptLanguageList"/>) was returned.
        /// The return value will be one of the cultures in <paramref name="availableCultures"/>, or
        /// <see cref="CultureInfo.InvariantCulture"/> if an invariant culture was used as a fallback
        /// (because nothing else in <paramref name="availableCultures"/> matched). No
        /// <see cref="UCultureInfo"/> in <paramref name="acceptLanguageList"/> or <paramref name="availableCultures"/>
        /// should be <c>null</c>; behavior is undefined in this case.
        /// </summary>
        /// <param name="acceptLanguageList">Ordered list of acceptable cultures (preferred are listed first).</param>
        /// <param name="availableCultures">List of available cultures. One of these will be returned.</param>
        /// <param name="isFallback">Returns the fallback status.</param>
        /// <returns>One of the cultures from the <paramref name="availableCultures"/> list, or <c>null</c> if none match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="acceptLanguageList"/> or <paramref name="availableCultures"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">One of the elements of <paramref name="acceptLanguageList"/> or
        /// <paramref name="availableCultures"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
        public static UCultureInfo? AcceptLanguage(IList<UCultureInfo> acceptLanguageList,
            IList<UCultureInfo> availableCultures, out bool isFallback) // ICU4N TODO: API - review whether returning null here makes sense
        {
            if (acceptLanguageList == null)
                throw new ArgumentNullException(nameof(acceptLanguageList));
            if (availableCultures == null)
                throw new ArgumentNullException(nameof(availableCultures));

            // fallbacklist
            int i, j;
            isFallback = true;
            for (i = 0; i < acceptLanguageList.Count; i++)
            {
                var aLocale = acceptLanguageList[i];
                if (aLocale == null)
                    throw new ArgumentException($"Element {i} of {nameof(acceptLanguageList)} is null.");
                bool setFallback = true;
                do
                {
                    for (j = 0; j < availableCultures.Count; j++)
                    {
                        var availableLocale = availableCultures[j];
                        if (availableLocale == null)
                            throw new ArgumentException($"Element {j} of {nameof(availableCultures)} is null.");
                        if (availableLocale.Equals(aLocale))
                        {
                            if (setFallback)
                            {
                                isFallback = false; // first time with this locale - not a fallback.
                            }
                            return availableLocale;
                        }
                        // compare to scriptless alias, so locales such as
                        // zh_TW, zh_CN are considered as available locales - see #7190
                        if (aLocale.Script.Length == 0
                                && availableLocale.Script.Length > 0
                                && availableLocale.Language.Equals(aLocale.Language)
                                && availableLocale.Country.Equals(aLocale.Country)
                                && availableLocale.Variant.Equals(aLocale.Variant))
                        {
                            UCultureInfo minAvail = UCultureInfo.MinimizeSubtags(availableLocale);
                            if (minAvail.Script.Length == 0)
                            {
                                if (setFallback)
                                {
                                    isFallback = false; // not a fallback.
                                }
                                return aLocale;
                            }
                        }
                    }

                    // ICU4N: In .NET we don't have fallback behavior for Accept-Language, so use the
                    // LocaleUtility to simply rewrite the string to the correct culture.
                    // This differs from UCultureInfo.GetParentString() in that it skips the script tag.
                    UCultureInfo? parent = LocaleUtility.Fallback(aLocale);
                    
                    if (parent != null)
                    {
                        aLocale = parent;
                    }
                    else
                    {
                        aLocale = null;
                    }

                    setFallback = false; // Do not set fallback in later iterations
                } while (aLocale != null);
            }
            return null;
        }

        /// <summary>
        /// <icu/> Based on an HTTP formatted list of acceptable cultures, determine an available
        /// locale for the user. <paramref name="isFallback"/> will be <c>true</c> if a
        /// fallback culture (one not in the <paramref name="acceptLanguageList"/>) was returned.
        /// The return value will be one of the cultures in <see cref="UCultureInfo.GetCultures(UCultureTypes)"/>, or
        /// <see cref="CultureInfo.InvariantCulture"/> if an invariant culture was used as a fallback
        /// (because nothing else in <see cref="UCultureInfo.GetCultures(UCultureTypes)"/> matched). No
        /// <see cref="UCultureInfo"/> in <paramref name="acceptLanguageList"/> should be <c>null</c>;
        /// behavior is undefined in this case.
        /// </summary>
        /// <param name="acceptLanguageList">List in HTTP "Accept-Language:" format of acceptable cultures.</param>
        /// <param name="isFallback">Returns the fallback status.</param>
        /// <returns>One of the cultures from the <see cref="GetCultures(UCultureTypes)"/> list, or <c>null</c> if none match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="acceptLanguageList"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
        public static UCultureInfo? AcceptLanguage(string acceptLanguageList, out bool isFallback) // ICU4N TODO: API - review whether returning null here makes sense
        {
            return AcceptLanguage(acceptLanguageList, GetCultures(UCultureTypes.AllCultures), out isFallback);
        }

        /// <summary>
        /// <icu/> Based on an ordered list of acceptable cultures, determine an available
        /// culture for the user. <paramref name="isFallback"/> will be <c>true</c> if a
        /// fallback culture (one not in the <paramref name="acceptLanguageList"/>) was returned.
        /// The return value will be one of the cultures in <see cref="GetCultures(UCultureTypes)"/>, or
        /// <see cref="CultureInfo.InvariantCulture"/> if an invariant culture was used as a fallback
        /// (because nothing else in <see cref="GetCultures(UCultureTypes)"/> matched). No
        /// <see cref="UCultureInfo"/> in <paramref name="acceptLanguageList"/> should be <c>null</c>;
        /// behavior is undefined in this case.
        /// </summary>
        /// <param name="acceptLanguageList">Ordered list of acceptable cultures (preferred are listed first).</param>
        /// <param name="isFallback">Returns the fallback status.</param>
        /// <returns>One of the cultures from the <see cref="GetCultures(UCultureTypes)"/> list, or <c>null</c> if none match.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="acceptLanguageList"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
        public static UCultureInfo? AcceptLanguage(IList<UCultureInfo> acceptLanguageList, out bool isFallback) // ICU4N TODO: API - review whether returning null here makes sense
        {
            return AcceptLanguage(acceptLanguageList, GetCultures(UCultureTypes.AllCultures), out isFallback);
        }

        private class CultureAcceptLanguageQ : IComparable<CultureAcceptLanguageQ>
        {
            private readonly double q;
            private readonly double serial;

            public CultureAcceptLanguageQ(double q, int serial)
            {
                this.q = q;
                this.serial = serial;
            }

            public int CompareTo(CultureAcceptLanguageQ? other)
            {
                if (other is null) return 1; // ICU4N: Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

                if (q > other.q)
                { // reverse - to sort in descending order
                    return -1;
                }
                else if (q < other.q)
                {
                    return 1;
                }
                if (serial < other.serial)
                {
                    return -1;
                }
                else if (serial > other.serial)
                {
                    return 1;
                }
                else
                {
                    return 0; // same object
                }
            }
        }

        /// <summary>
        /// Internal method used for parsing Accept-Language string
        /// </summary>
        internal static bool TryParseAcceptLanguage(string acceptLanguage, bool isLenient, [MaybeNullWhen(false)] out IList<UCultureInfo> result)
        {
            result = null;
            // parse out the acceptLanguage into an array
            SortedDictionary<CultureAcceptLanguageQ, UCultureInfo> map =
                    new SortedDictionary<CultureAcceptLanguageQ, UCultureInfo>();

            int state = 0;
            var languageRangeBuf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            var qvalBuf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                acceptLanguage += ","; // append comma to simplify the parsing code
                int n;
                bool subTag = false;
                bool q1 = false;
                for (n = 0; n < acceptLanguage.Length; n++)
                {
                    bool gotLanguageQ = false;
                    char c = acceptLanguage[n];
                    switch (state)
                    {
                        case 0: // before language-range start
                            if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                            {
                                // in language-range
                                languageRangeBuf.Append(c);
                                state = 1;
                                subTag = false;
                            }
                            else if (c == '*')
                            {
                                languageRangeBuf.Append(c);
                                state = 2;
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid character
                                state = -1;
                            }
                            break;
                        case 1: // in language-range
                            if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                            {
                                languageRangeBuf.Append(c);
                            }
                            else if (c == '-')
                            {
                                subTag = true;
                                languageRangeBuf.Append(c);
                            }
                            else if (c == '_')
                            {
                                if (isLenient)
                                {
                                    subTag = true;
                                    languageRangeBuf.Append(c);
                                }
                                else
                                {
                                    state = -1;
                                }
                            }
                            else if ('0' <= c && c <= '9')
                            {
                                if (subTag)
                                {
                                    languageRangeBuf.Append(c);
                                }
                                else
                                {
                                    // DIGIT is allowed only in language sub tag
                                    state = -1;
                                }
                            }
                            else if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c == ' ' || c == '\t')
                            {
                                // language-range end
                                state = 3;
                            }
                            else if (c == ';')
                            {
                                // before q
                                state = 4;
                            }
                            else
                            {
                                // invalid character for language-range
                                state = -1;
                            }
                            break;
                        case 2: // saw wild card range
                            if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c == ' ' || c == '\t')
                            {
                                // language-range end
                                state = 3;
                            }
                            else if (c == ';')
                            {
                                // before q
                                state = 4;
                            }
                            else
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 3: // language-range end
                            if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c == ';')
                            {
                                // before q
                                state = 4;
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 4: // before q
                            if (c == 'q')
                            {
                                // before equal
                                state = 5;
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 5: // before equal
                            if (c == '=')
                            {
                                // before q value
                                state = 6;
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 6: // before q value
                            if (c == '0')
                            {
                                // q value start with 0
                                q1 = false;
                                qvalBuf.Append(c);
                                state = 7;
                            }
                            else if (c == '1')
                            {
                                // q value start with 1
                                qvalBuf.Append(c);
                                state = 7;
                            }
                            else if (c == '.')
                            {
                                if (isLenient)
                                {
                                    qvalBuf.Append(c);
                                    state = 8;
                                }
                                else
                                {
                                    state = -1;
                                }
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 7: // q value start
                            if (c == '.')
                            {
                                // before q value fraction part
                                qvalBuf.Append(c);
                                state = 8;
                            }
                            else if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c == ' ' || c == '\t')
                            {
                                // after q value
                                state = 10;
                            }
                            else
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 8: // before q value fraction part
                            if ('0' <= c && c <= '9')
                            {
                                if (q1 && c != '0' && !isLenient)
                                {
                                    // if q value starts with 1, the fraction part must be 0
                                    state = -1;
                                }
                                else
                                {
                                    // in q value fraction part
                                    qvalBuf.Append(c);
                                    state = 9;
                                }
                            }
                            else
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 9: // in q value fraction part
                            if ('0' <= c && c <= '9')
                            {
                                if (q1 && c != '0')
                                {
                                    // if q value starts with 1, the fraction part must be 0
                                    state = -1;
                                }
                                else
                                {
                                    qvalBuf.Append(c);
                                }
                            }
                            else if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c == ' ' || c == '\t')
                            {
                                // after q value
                                state = 10;
                            }
                            else
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                        case 10: // after q value
                            if (c == ',')
                            {
                                // language-q end
                                gotLanguageQ = true;
                            }
                            else if (c != ' ' && c != '\t')
                            {
                                // invalid
                                state = -1;
                            }
                            break;
                    }
                    if (state == -1)
                    {
                        return false;
                    }
                    if (gotLanguageQ)
                    {
                        double q = 1.0;
                        if (qvalBuf.Length != 0)
                        {
                            if (!J2N.Numerics.Double.TryParse(qvalBuf.AsSpan(), NumberStyle.Float, CultureInfo.InvariantCulture, out q))
                            {
                                // Already validated, so it should never happen
                                q = 1.0;
                            }
                            if (q > 1.0)
                            {
                                q = 1.0;
                            }
                        }
                        if (languageRangeBuf[0] != '*')
                        {
                            int serial = map.Count;
                            CultureAcceptLanguageQ entry = new CultureAcceptLanguageQ(q, serial);
                            // sort in reverse order..   1.0, 0.9, 0.8 .. etc
                            string canonicalizedLocaleID = Canonicalize(languageRangeBuf.AsSpan());
                            map[entry] = new UCultureInfo(canonicalizedLocaleID);
                        }

                        // reset buffer and parse state
                    languageRangeBuf.Length = 0;
                    qvalBuf.Length = 0;
                        state = 0;
                    }
                }
            }
            finally
            {
                languageRangeBuf.Dispose();
                qvalBuf.Dispose();
            }
            if (state != 0)
            {
                // Well, the parser should handle all cases.  So just in case.
                return false;
            }

            // pull out the map
            result = map.Values.ToArray();
            return true;
        }

        private const string UndefinedLanguage = "und";
        private const string UndefinedScript = "Zzzz";
        private const string UndefinedRegion = "ZZ";

        /// <summary>
        /// <icu/> Adds the likely subtags for a provided locale ID, per the algorithm
        /// described in the following CLDR technical report:
        /// <a href="http://www.unicode.org/reports/tr35/#Likely_Subtags">http://www.unicode.org/reports/tr35/#Likely_Subtags</a>
        /// 
        /// <para/>If the provided <see cref="UCultureInfo"/> instance is already in the maximal form, or there is no
        /// data available available for maximization, it will be returned. For example,
        /// "und-Zzzz" cannot be maximized, since there is no reasonable maximization.
        /// Otherwise, a new <see cref="UCultureInfo"/> instance with the maximal form is returned.
        /// 
        /// <para/>Examples:
        /// <list type="bullet">
        ///     <item><description>"en" maximizes to "en_Latn_US"</description></item>
        ///     <item><description>"de" maximizes to "de_Latn_US"</description></item>
        ///     <item><description>"sr" maximizes to "sr_Cyrl_RS"</description></item>
        ///     <item><description>"sh" maximizes to "sr_Latn_RS" (Note this will not reverse.)</description></item>
        ///     <item><description>"zh_Hani" maximizes to "zh_Hans_CN" (Note this will not reverse.)</description></item>
        /// </list>
        /// </summary>
        /// <param name="culture">The <see cref="UCultureInfo"/> to maximize.</param>
        /// <returns>The maximized <see cref="UCultureInfo"/> instance.</returns>
        /// <stable>ICU 4.0</stable>
        public static UCultureInfo AddLikelySubtags(UCultureInfo culture)
        {
            string? trailing = null;

            int trailingIndex = ParseTagString(
                    culture.localeID,
                    out string language,
                    out string script,
                    out string region);

            if (trailingIndex < culture.localeID.Length)
            {
                trailing = culture.localeID.Substring(trailingIndex);
            }

            string? newLocaleID =
                    CreateLikelySubtagsString(
                            language,
                            script,
                            region,
                            trailing);

            return newLocaleID == null ? culture : new UCultureInfo(newLocaleID);
        }

        /// <summary>
        /// <icu/> Minimizes the subtags for a provided locale ID, per the algorithm described
        /// in the following CLDR technical report:
        /// <a href="http://www.unicode.org/reports/tr35/#Likely_Subtags">http://www.unicode.org/reports/tr35/#Likely_Subtags</a>
        /// 
        /// <para/>If the provided <see cref="UCultureInfo"/> instance is already in the minimal form, or there
        /// is no data available for minimization, it will be returned. Since the
        /// minimization algorithm relies on proper maximization, see the comments
        /// for <see cref="AddLikelySubtags(UCultureInfo)"/> for reasons why there might not be any data.
        /// 
        /// <para/>Examples:
        /// <list type="bullet">
        ///     <item><description>"en_Latn_US" minimizes to "en"</description></item>
        ///     <item><description>"de_Latn_US" minimizes to "de"</description></item>
        ///     <item><description>"sr_Cyrl_RS" minimizes to "sr"</description></item>
        ///     <item><description>"zh_Hant_TW" minimizes to "zh_TW" (The region is preferred to the
        ///         script, and minimizing to "zh" would imply "zh_Hans_CN".)</description></item>
        /// </list>
        /// </summary>
        /// <param name="culture">The <see cref="UCultureInfo"/> to minimize.</param>
        /// <returns>The minimized <see cref="UCultureInfo"/> instance.</returns>
        /// <stable>ICU 4.0</stable>
        public static UCultureInfo MinimizeSubtags(UCultureInfo culture)
        {
#pragma warning disable 612, 618
            return MinimizeSubtags(culture, Minimize.FavorRegion);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Options for <see cref="MinimizeSubtags(UCultureInfo, Minimize)"/>.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        internal enum Minimize // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            /// <summary>
            /// Favor including the script, when either the region <b>or</b> the script could be suppressed, but not both.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            FavorScript,

            /// <summary>
            /// Favor including the region, when either the region <b>or</b> the script could be suppressed, but not both.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            FavorRegion
        }

        /// <summary>
        /// <icu/> Minimizes the subtags for a provided locale ID, per the algorithm described
        /// in the following CLDR technical report:
        /// <a href="http://www.unicode.org/reports/tr35/#Likely_Subtags">http://www.unicode.org/reports/tr35/#Likely_Subtags</a>
        /// 
        /// <para/>If the provided <see cref="UCultureInfo"/> instance is already in the minimal form, or there
        /// is no data available for minimization, it will be returned. Since the
        /// minimization algorithm relies on proper maximization, see the comments
        /// for <see cref="AddLikelySubtags(UCultureInfo)"/> for reasons why there might not be any data.
        /// 
        /// <para/>Examples:
        /// <list type="bullet">
        ///     <item><description>"en_Latn_US" minimizes to "en"</description></item>
        ///     <item><description>"de_Latn_US" minimizes to "de"</description></item>
        ///     <item><description>"sr_Cyrl_RS" minimizes to "sr"</description></item>
        ///     <item><description>"zh_Hant_TW" minimizes to "zh_TW" if
        ///         <paramref name="fieldToFavor"/> == <see cref="Minimize.FavorRegion"/></description></item>
        ///     <item><description>"zh_Hant_TW" minimizes to "zh_Hant" if
        ///         <paramref name="fieldToFavor"/> == <see cref="Minimize.FavorScript"/></description></item>
        /// </list>
        /// <para/>
        /// The <paramref name="fieldToFavor"/> only has an effect if either the region or the script
        /// could be suppressed, but not both.
        /// </summary>
        /// <param name="loc">The <see cref="UCultureInfo"/> to minimize.</param>
        /// <param name="fieldToFavor">Indicate which should be preferred, when either the region
        /// <b>or</b> the script could be suppressed, but not both.</param>
        /// <returns>The minimized <see cref="UCultureInfo"/> instance.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static UCultureInfo MinimizeSubtags(UCultureInfo loc, Minimize fieldToFavor) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            int trailingIndex = ParseTagString(
                    loc.localeID,
                    out string originalLang,
                    out string originalScript,
                    out string originalRegion);

            string? originalTrailing = null;

            if (trailingIndex < loc.localeID.Length)
            {
                /*
                 * Create a string that contains everything
                 * after the language, script, and region.
                 */
                originalTrailing = loc.localeID.Substring(trailingIndex);
            }

            /*
             * First, we need to first get the maximization
             * by adding any likely subtags.
             **/
            string? maximizedLocaleID =
                    CreateLikelySubtagsString(
                            originalLang,
                            originalScript,
                            originalRegion,
                            null);

            /*
             * If maximization fails, there's nothing
             * we can do.
             **/
            if (string.IsNullOrEmpty(maximizedLocaleID))
            {
                return loc;
            }
            else
            {
                /*
                 * Start first with just the language.
                 **/
                string? tag =
                        CreateLikelySubtagsString(
                                originalLang,
                                null,
                                null,
                                null);

                if (tag is not null && tag.Equals(maximizedLocaleID))
                {
                    string newLocaleID =
                            CreateTagString(
                                    originalLang,
                                    null,
                                    null,
                                    originalTrailing);

                    return new UCultureInfo(newLocaleID);
                }
            }

            /*
             * Next, try the language and region.
             **/
            if (fieldToFavor == Minimize.FavorRegion)
            {
                if (originalRegion.Length != 0)
                {
                    string? tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    null,
                                    originalRegion,
                                    null);

                    if (tag is not null && tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        null,
                                        originalRegion,
                                        originalTrailing);

                        return new UCultureInfo(newLocaleID);
                    }
                }
                if (originalScript.Length != 0)
                {
                    string? tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    originalScript,
                                    null,
                                    null);

                    if (tag is not null && tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        originalScript,
                                        null,
                                        originalTrailing);

                        return new UCultureInfo(newLocaleID);
                    }
                }
            }
            else
            { // FAVOR_SCRIPT, so
                if (originalScript.Length != 0)
                {
                    string? tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    originalScript,
                                    null,
                                    null);

                    if (tag is not null && tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        originalScript,
                                        null,
                                        originalTrailing);

                        return new UCultureInfo(newLocaleID);
                    }
                }
                if (originalRegion.Length != 0)
                {
                    string? tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    null,
                                    originalRegion,
                                    null);

                    if (tag is not null && tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        null,
                                        originalRegion,
                                        originalTrailing);

                        return new UCultureInfo(newLocaleID);
                    }
                }
            }
            return loc;
        }

        /// <summary>
        /// Append a tag to a StringBuilder, adding the separator if necessary. The tag must
        /// not be a zero-length string.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        /// <param name="buffer">The output buffer.</param>
        private static void AppendTag(string tag, ref ValueStringBuilder buffer)
        {
            if (buffer.Length != 0)
            {
                buffer.Append(Underscore);
            }

            buffer.Append(tag);
        }

        /// <summary>
        /// Create a tag string from the supplied parameters. The lang, script and region
        /// parameters may be <c>null</c> references.
        /// 
        /// <para/>If any of the language, script or region parameters are empty, and the alternateTags
        /// parameter is not <c>null</c>, it will be parsed for potential language, script and region tags
        /// to be used when constructing the new tag. If the alternateTags parameter is <c>null</c>, or
        /// it contains no language tag, the default tag for the unknown language is used.
        /// </summary>
        /// <param name="lang">The language tag to use.</param>
        /// <param name="script">The script tag to use.</param>
        /// <param name="region">The region tag to use.</param>
        /// <param name="trailing">Any trailing data to append to the new tag.</param>
        /// <param name="alternateTags">A string containing any alternate tags.</param>
        /// <returns>The new tag string.</returns>
        private static string CreateTagString(string? lang, string? script, string? region,
                string? trailing, string? alternateTags)
        {
            bool regionAppended = false;
            ValueStringBuilder tag = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            using LocaleIDParser parser = !string.IsNullOrEmpty(alternateTags)
                ? new LocaleIDParser(stackalloc char[CharStackBufferSize], alternateTags)
                : default;

            if (!string.IsNullOrEmpty(lang))
            {
                AppendTag(lang!, ref tag);
            }
            else if (string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Append the value for an unknown language, if
                 * we found no language.
                 */
                AppendTag(UndefinedLanguage, ref tag);
            }
            else
            {
                string alternateLang = parser.GetLanguage();

                /*
                 * Append the value for an unknown language, if
                 * we found no language.
                 */
                AppendTag(!string.IsNullOrEmpty(alternateLang) ? alternateLang : UndefinedLanguage, ref tag);
            }

            if (!string.IsNullOrEmpty(script))
            {
                AppendTag(script!, ref tag);
            }
            else if (!string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Parse the alternateTags string for the script.
                 */
                string alternateScript = parser.GetScript();

                if (!string.IsNullOrEmpty(alternateScript))
                {
                    AppendTag(alternateScript, ref tag);
                }
            }

            if (!string.IsNullOrEmpty(region))
            {
                AppendTag(region!, ref tag);

                regionAppended = true;
            }
            else if (!string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Parse the alternateTags string for the region.
                 */
                string alternateRegion = parser.GetCountry();

                if (!string.IsNullOrEmpty(alternateRegion))
                {
                    AppendTag(alternateRegion, ref tag);

                    regionAppended = true;
                }
            }

            if (trailing != null && trailing.Length > 1)
            {
                /*
                 * The current ICU format expects two underscores
                 * will separate the variant from the preceeding
                 * parts of the tag, if there is no region.
                 */
                int separators = 0;

                if (trailing[0] == Underscore)
                {
                    if (trailing[1] == Underscore)
                    {
                        separators = 2;
                    }
                }
                else
                {
                    separators = 1;
                }

                if (regionAppended)
                {
                    /*
                     * If we appended a region, we may need to strip
                     * the extra separator from the variant portion.
                     */
                    if (separators == 2)
                    {
                        tag.Append(trailing.AsSpan(1));
                    }
                    else
                    {
                        tag.Append(trailing);
                    }
                }
                else
                {
                    /*
                     * If we did not append a region, we may need to add
                     * an extra separator to the variant portion.
                     */
                    if (separators == 1)
                    {
                        tag.Append(Underscore);
                    }
                    tag.Append(trailing);
                }
            }

            return tag.ToString();
        }

        /// <summary>
        /// Create a tag string from the supplied parameters. The lang, script and region
        /// parameters may be <c>null</c> references. If the lang parameter is an empty string, the
        /// default value for an unknown language is written to the output buffer.
        /// </summary>
        /// <param name="lang">The language tag to use.</param>
        /// <param name="script">The script tag to use.</param>
        /// <param name="region">The region tag to use.</param>
        /// <param name="trailing">Any trailing data to append to the new tag.</param>
        /// <returns>The new string.</returns>
        internal static string CreateTagString(string? lang, string? script, string? region, string? trailing)
        {
            return CreateTagString(lang, script, region, trailing, null);
        }

        /// <summary>
        /// Parse the language, script, and region subtags from a tag string, and return the results.
        /// 
        /// <para/>This function does not return the canonical strings for the unknown script and region.
        /// </summary>
        /// <param name="localeID">The locale ID to parse.</param>
        /// <param name="language">The returned language id.</param>
        /// <param name="script">The returned script id.</param>
        /// <param name="region">The returned region (country id).</param>
        /// <returns>The number of chars of the localeID parameter consumed.</returns>
        private static int ParseTagString(string localeID, out string language, out string script, out string region)
        {
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
            string lang = parser.GetLanguage();
            string scr = parser.GetScript();
            string reg = parser.GetCountry();

            if (string.IsNullOrEmpty(lang))
            {
                language = UndefinedLanguage;
            }
            else
            {
                language = lang;
            }

            if (scr.Equals(UndefinedScript))
            {
                script = "";
            }
            else
            {
                script = scr;
            }

            if (reg.Equals(UndefinedRegion))
            {
                region = "";
            }
            else
            {
                region = reg;
            }

            /*
             * Search for the variant.  If there is one, then return the index of
             * the preceeding separator.
             * If there's no variant, search for the keyword delimiter,
             * and return its index.  Otherwise, return the length of the
             * string.
             *
             * $TOTO(dbertoni) we need to take into account that we might
             * find a part of the language as the variant, since it can
             * can have a variant portion that is long enough to contain
             * the same characters as the variant.
             */
            string variant = parser.GetVariant();

            if (!string.IsNullOrEmpty(variant))
            {
                int index = localeID.IndexOf(variant, StringComparison.Ordinal);


                return index > 0 ? index - 1 : index;
            }
            else
            {
                int index = localeID.IndexOf('@');

                return index == -1 ? localeID.Length : index;
            }
        }

        private static string? LookupLikelySubtags(string localeId)
        {
            UResourceBundle bundle =
                    UResourceBundle.GetBundleInstance(
                            ICUData.IcuBaseName, "likelySubtags");
            try
            {
                return bundle.GetString(localeId);
            }
            catch (MissingManifestResourceException)
            {
                return null;
            }
        }

        private static string? CreateLikelySubtagsString(string? lang, string? script, string? region,
                string? variants)
        {

            /*
             * Try the language with the script and region first.
             */
            if (!string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(region))
            {

                string searchTag =
                        CreateTagString(
                                lang,
                                script,
                                region,
                                null);

                string? likelySubtags = LookupLikelySubtags(searchTag);

                /*
                if (likelySubtags == null) {
                    if (likelySubtags2 != null) {
                        System.err.println("Tag mismatch: \"(null)\" \"" + likelySubtags2 + "\"");
                    }
                }
                else if (likelySubtags2 == null) {
                    System.err.println("Tag mismatch: \"" + likelySubtags + "\" \"(null)\"");
                }
                else if (!likelySubtags.equals(likelySubtags2)) {
                    System.err.println("Tag mismatch: \"" + likelySubtags + "\" \"" + likelySubtags2
                        + "\"");
                }
                 */
                if (likelySubtags != null)
                {
                    // Always use the language tag from the
                    // maximal string, since it may be more
                    // specific than the one provided.
                    return CreateTagString(
                            null,
                            null,
                            null,
                            variants,
                            likelySubtags);
                }
            }

            /*
             * Try the language with just the script.
             **/
            if (!string.IsNullOrEmpty(script))
            {

                string searchTag =
                        CreateTagString(
                                lang,
                                script,
                                null,
                                null);

                string? likelySubtags = LookupLikelySubtags(searchTag);
                if (likelySubtags != null)
                {
                    // Always use the language tag from the
                    // maximal string, since it may be more
                    // specific than the one provided.
                    return CreateTagString(
                            null,
                            null,
                            region,
                            variants,
                            likelySubtags);
                }
            }

            /*
             * Try the language with just the region.
             **/
            if (!string.IsNullOrEmpty(region))
            {

                string searchTag =
                        CreateTagString(
                                lang,
                                null,
                                region,
                                null);

                string? likelySubtags = LookupLikelySubtags(searchTag);

                if (likelySubtags != null)
                {
                    // Always use the language tag from the
                    // maximal string, since it may be more
                    // specific than the one provided.
                    return CreateTagString(
                            null,
                            script,
                            null,
                            variants,
                            likelySubtags);
                }
            }

            /*
             * Finally, try just the language.
             **/
            {
                string searchTag =
                        CreateTagString(
                                lang,
                                null,
                                null,
                                null);

                string? likelySubtags = LookupLikelySubtags(searchTag);

                if (likelySubtags != null)
                {
                    // Always use the language tag from the
                    // maximal string, since it may be more
                    // specific than the one provided.
                    return CreateTagString(
                            null,
                            script,
                            region,
                            variants,
                            likelySubtags);
                }
            }

            return null;
        }


        // --------------------------------
        //      BCP47/OpenJDK APIs
        // --------------------------------

        /// <summary>
        /// The key for the private use locale extension ('x').
        /// </summary>
        /// <seealso cref="Extensions"/>
        /// <stable>ICU 4.2</stable>
        public const char PrivateUseExtension = 'x';

        /// <summary>
        /// The key for Unicode locale extension ('u').
        /// </summary>
        /// <seealso cref="Extensions"/>
        /// <stable>ICU 4.2</stable>
        public const char UnicodeLocaleExtension = 'u';


        /// <summary>
        /// Gets the set of all extension keys and values associated with this locale,
        /// or an empty dictionary if it has no extensions. The returned dictionary is unmodifiable.
        /// The keys will be all lowercase.
        /// </summary>
        /// <draft>ICU 60</draft>
#if FEATURE_IREADONLYCOLLECTIONS
        public IReadOnlyDictionary<char, string> Extensions
#else
        public IDictionary<char, string> Extensions
#endif
            => LocaleExtensions.Extensions;

        /// <summary>
        /// Gets the set of Unicode locale attributes associated with
        /// this locale, or the empty set if it has no attributes. The
        /// returned set is unmodifiable.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public ISet<string> UnicodeLocaleAttributes
            => LocaleExtensions.UnicodeLocaleAttributes;

        /// <summary>
        /// Gets the set of Unicode locale keys and types as an immutable dictionary.
        /// Keys are all lower case. Types are <see cref="string.Empty"/> for keys that are defined with no type.
        /// </summary>
        /// <draft>ICU 60</draft>
        // ICU4N: Corresponds to both GetUnicodeLocaleKeys() and GetUnicodeLocaleType(string) in ICU4J
#if FEATURE_IREADONLYCOLLECTIONS
        public IReadOnlyDictionary<string, string> UnicodeLocales
#else
        public IDictionary<string, string> UnicodeLocales
#endif
        {
            get
            {
                if (unicodeLocales == null)
                    unicodeLocales = LocaleExtensions.UnicodeLocales;

                return unicodeLocales;
            }
        }

        /**
        * Returns a well-formed IETF BCP 47 language tag representing
        * this locale.
        *
        * <para/>If this <code>UCultureInfo</code> has a language, script, country, or
        * variant that does not satisfy the IETF BCP 47 language tag
        * syntax requirements, this method handles these fields as
        * described below:
        *
        * <para/><b>Language:</b> If language is empty, or not well-formed
        * (for example "a" or "e2"), it will be emitted as "und" (Undetermined).
        *
        * <para/><b>Script:</b> If script is not well-formed (for example "12"
        * or "Latin"), it will be omitted.
        *
        * <para/><b>Country:</b> If country is not well-formed (for example "12"
        * or "USA"), it will be omitted.
        *
        * <para/><b>Variant:</b> If variant <b>is</b> well-formed, each sub-segment
        * (delimited by '-' or '_') is emitted as a subtag.  Otherwise:
        * <ul>
        *
        * <li>if all sub-segments match <code>[0-9a-zA-Z]{1,8}</code>
        * (for example "WIN" or "Oracle_JDK_Standard_Edition"), the first
        * ill-formed sub-segment and all following will be appended to
        * the private use subtag.  The first appended subtag will be
        * "lvariant", followed by the sub-segments in order, separated by
        * hyphen. For example, "x-lvariant-WIN",
        * "Oracle-x-lvariant-JDK-Standard-Edition".</li>
        *
        * <li>if any sub-segment does not match
        * <code>[0-9a-zA-Z]{1,8}</code>, the variant will be truncated
        * and the problematic sub-segment and all following sub-segments
        * will be omitted.  If the remainder is non-empty, it will be
        * emitted as a private use subtag as above (even if the remainder
        * turns out to be well-formed).  For example,
        * "Solaris_isjustthecoolestthing" is emitted as
        * "x-lvariant-Solaris", not as "solaris".</li></ul>
        *
        * <para/><b>Note:</b> Although the language tag created by this
        * method is well-formed (satisfies the syntax requirements
        * defined by the IETF BCP 47 specification), it is not
        * necessarily a valid BCP 47 language tag.  For example,
        * <pre>
        *   new Locale("xx", "YY").toLanguageTag();</pre>
        *
        * will return "xx-YY", but the language subtag "xx" and the
        * region subtag "YY" are invalid because they are not registered
        * in the IANA Language Subtag Registry.
        *
        * @return a BCP47 language tag representing the locale
        * @see #forLanguageTag(string)
        *
        * @stable ICU 4.2
        */
        internal string ToIetfLanguageTag() // ICU4N TODO: API Make this into the Name property (cached) and replace Name with BaseName property..?
        {
            BaseLocale @base = Base;
            LocaleExtensions exts = LocaleExtensions;

            if (@base.Variant.Equals("POSIX", StringComparison.OrdinalIgnoreCase))
            {
                // special handling for variant POSIX
                @base = BaseLocale.GetInstance(@base.Language, @base.Script, @base.Region, "");
                if (exts.GetUnicodeLocaleType("va") == null)
                {
                    // add va-posix
                    InternalLocaleBuilder ilocbld = new InternalLocaleBuilder();
                    try
                    {
                        ilocbld.SetLocale(BaseLocale.Root, exts);
                        ilocbld.SetUnicodeLocaleKeyword("va", "posix");
                        exts = ilocbld.GetLocaleExtensions();
                    }
                    catch (FormatException e)
                    {
                        // this should not happen
                        throw new Exception(e.ToString(), e);
                    }
                }
            }

            LanguageTag.ParseLocale(@base, exts, out LanguageTag tag);

            ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                string subtag = tag.Language;
                if (subtag.Length > 0)
                {
                    buf.Append(LanguageTag.CanonicalizeLanguage(subtag));
                }

                subtag = tag.Script;
                if (subtag.Length > 0)
                {
                    buf.Append(LanguageTag.Separator);
                    buf.Append(LanguageTag.CanonicalizeScript(subtag));
                }

                subtag = tag.Region;
                if (subtag.Length > 0)
                {
                    buf.Append(LanguageTag.Separator);
                    buf.Append(LanguageTag.CanonicalizeRegion(subtag));
                }

                IList<string> subtags = tag.Variants;
                foreach (string s in subtags)
                {
                    buf.Append(LanguageTag.Separator);
                    buf.Append(LanguageTag.CanonicalizeVariant(s));
                }

                subtags = tag.Extensions;
                foreach (string s in subtags)
                {
                    buf.Append(LanguageTag.Separator);
                    buf.Append(LanguageTag.CanonicalizeExtension(s));
                }

                subtag = tag.PrivateUse;
                if (subtag.Length > 0)
                {
                    if (buf.Length > 0)
                    {
                        buf.Append(LanguageTag.Separator);
                    }
                    buf.Append(LanguageTag.Private_Use);
                    buf.Append(LanguageTag.Separator);
                    buf.Append(LanguageTag.CanonicalizePrivateuse(subtag));
                }

                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        /**
         * Returns a locale for the specified IETF BCP 47 language tag string.
         *
         * <para/>If the specified language tag contains any ill-formed subtags,
         * the first such subtag and all following subtags are ignored.  Compare
         * to <see cref="UCultureInfoBuilder.SetLanguageTag(string)"/> which throws an exception
         * in this case.
         *
         * <para/>The following <b>conversions</b> are performed:
         * <ul>
         *
         * <li>The language code "und" is mapped to language "".</li>
         *
         * <li>The portion of a private use subtag prefixed by "lvariant",
         * if any, is removed and appended to the variant field in the
         * result locale (without case normalization).  If it is then
         * empty, the private use subtag is discarded:
         *
         * <pre>
         *     UCultureInfo loc;
         *     loc = UCultureInfo.GetCultureInfoByIetfLanguageTag("en-US-x-lvariant-icu4n);
         *     loc.Variant; // returns "ICU4N"
         *     loc.Extensions.TryGetValue("x", out string _); // returns false/null
         *
         *     loc = CultureInfo.GetCultureInfoByIetfLanguageTag("de-icu4n-x-URP-lvariant-Abc-Def");
         *     loc.Variant; // returns "ICU4N_ABC_DEF"
         *     loc.Extensions.TryGetValue("x", out string _); // returns "urp"
         * </pre></li>
         *
         * <li>When the languageTag argument contains an extlang subtag,
         * the first such subtag is used as the language, and the primary
         * language subtag and other extlang subtags are ignored:
         *
         * <pre>
         *     UCultureInfo.GetCultureInfoByIetfLanguageTag("ar-aao").Language; // returns "aao"
         *     UCultureInfo.GetCultureInfoByIetfLanguageTag("en-abc-def-us").ToString(); // returns "abc_US"
         * </pre></li>
         *
         * <li>Case is normalized. Language is normalized to lower case,
         * script to title case, country to upper case, variant to upper case,
         * and extensions to lower case.</li>
         *
         * </ul>
         *
         * <para/>This implements the 'Language-Tag' production of BCP47, and
         * so supports grandfathered (regular and irregular) as well as
         * private use language tags.  Stand alone private use tags are
         * represented as empty language and extension 'x-whatever',
         * and grandfathered tags are converted to their canonical replacements
         * where they exist.
         *
         * <para/>Grandfathered tags with canonical replacements are as follows:
         *
         * <table>
         * <tbody align="center">
         * <tr><th>grandfathered tag</th><th>&#160;</th><th>modern replacement</th></tr>
         * <tr><td>art-lojban</td><td>&#160;</td><td>jbo</td></tr>
         * <tr><td>i-ami</td><td>&#160;</td><td>ami</td></tr>
         * <tr><td>i-bnn</td><td>&#160;</td><td>bnn</td></tr>
         * <tr><td>i-hak</td><td>&#160;</td><td>hak</td></tr>
         * <tr><td>i-klingon</td><td>&#160;</td><td>tlh</td></tr>
         * <tr><td>i-lux</td><td>&#160;</td><td>lb</td></tr>
         * <tr><td>i-navajo</td><td>&#160;</td><td>nv</td></tr>
         * <tr><td>i-pwn</td><td>&#160;</td><td>pwn</td></tr>
         * <tr><td>i-tao</td><td>&#160;</td><td>tao</td></tr>
         * <tr><td>i-tay</td><td>&#160;</td><td>tay</td></tr>
         * <tr><td>i-tsu</td><td>&#160;</td><td>tsu</td></tr>
         * <tr><td>no-bok</td><td>&#160;</td><td>nb</td></tr>
         * <tr><td>no-nyn</td><td>&#160;</td><td>nn</td></tr>
         * <tr><td>sgn-BE-FR</td><td>&#160;</td><td>sfb</td></tr>
         * <tr><td>sgn-BE-NL</td><td>&#160;</td><td>vgt</td></tr>
         * <tr><td>sgn-CH-DE</td><td>&#160;</td><td>sgg</td></tr>
         * <tr><td>zh-guoyu</td><td>&#160;</td><td>cmn</td></tr>
         * <tr><td>zh-hakka</td><td>&#160;</td><td>hak</td></tr>
         * <tr><td>zh-min-nan</td><td>&#160;</td><td>nan</td></tr>
         * <tr><td>zh-xiang</td><td>&#160;</td><td>hsn</td></tr>
         * </tbody>
         * </table>
         *
         * <para/>Grandfathered tags with no modern replacement will be
         * converted as follows:
         *
         * <table>
         * <tbody align="center">
         * <tr><th>grandfathered tag</th><th>&#160;</th><th>converts to</th></tr>
         * <tr><td>cel-gaulish</td><td>&#160;</td><td>xtg-x-cel-gaulish</td></tr>
         * <tr><td>en-GB-oed</td><td>&#160;</td><td>en-GB-x-oed</td></tr>
         * <tr><td>i-default</td><td>&#160;</td><td>en-x-i-default</td></tr>
         * <tr><td>i-enochian</td><td>&#160;</td><td>und-x-i-enochian</td></tr>
         * <tr><td>i-mingo</td><td>&#160;</td><td>see-x-i-mingo</td></tr>
         * <tr><td>zh-min</td><td>&#160;</td><td>nan-x-zh-min</td></tr>
         * </tbody>
         * </table>
         *
         * <para/>For a list of all grandfathered tags, see the
         * IANA Language Subtag Registry (search for "Type: grandfathered").
         *
         * <para/><b>Note</b>: there is no guarantee that <code>toLanguageTag</code>
         * and <code>forLanguageTag</code> will round-trip.
         *
         * @param languageTag the language tag
         * @return The locale that best represents the language tag.
         * @throws NullPointerException if <code>languageTag</code> is <code>null</code>
         * @see #toLanguageTag()
         * @see UCultureInfoBuilder.SetLanguageTag(string)
         *
         * @stable ICU 4.2
         */
        /*new*/ public static UCultureInfo GetCultureInfoByIetfLanguageTag(string languageTag) // ICU4N specific - renamed from ForLanguageTag to match .NET
        {
            if (languageTag == null)
                throw new ArgumentNullException(nameof(languageTag));

            LanguageTag.TryParse(languageTag.AsSpan(), out LanguageTag tag, out _);
            InternalLocaleBuilder bldr = new InternalLocaleBuilder();
            bldr.SetLanguageTag(tag);
            return GetInstance(bldr.GetBaseLocale(), bldr.GetLocaleExtensions());
        }

        /// <summary>
        /// <icu/> Converts the specified keyword (legacy key, or BCP 47 Unicode locale
        /// extension key) to the equivalent BCP 47 Unicode locale extension key.
        /// For example, BCP 47 Unicode locale extension key "co" is returned for
        /// the input keyword "collation".
        /// <para/>
        /// When the specified keyword is unknown, but satisfies the BCP syntax,
        /// then the lower-case version of the input keyword will be returned.
        /// For example,
        /// <c>ToUnicodeLocaleKey("ZZ")</c> returns "zz".
        /// </summary>
        /// <param name="keyword">The input locale keyword (either legacy key
        /// such as "collation" or BCP 47 Unicode locale extension
        /// key such as "co").</param>
        /// <returns>The well-formed BCP 47 Unicode locale extension key,
        /// or <c>null</c> if the specified locale keyword cannot be mapped
        /// to a well-formed BCP 47 Unicode locale extension key.</returns>
        /// <seealso cref="ToLegacyKey(string)"/>
        /// <stable>ICU 54</stable>
        public static string? ToUnicodeLocaleKey(string keyword)
        {
            string? bcpKey = KeyTypeData.ToBcpKey(keyword);
            if (bcpKey == null && UnicodeLocaleExtensionClass.IsKey(keyword))
            {
                // unknown keyword, but syntax is fine..
                bcpKey = AsciiUtil.ToLower(keyword);
            }
            return bcpKey;
        }

        /// <summary>
        /// <icu/> Converts the specified keyword value (legacy type, or BCP 47
        /// Unicode locale extension type) to the well-formed BCP 47 Unicode locale
        /// extension type for the specified keyword (category). For example, BCP 47
        /// Unicode locale extension type "phonebk" is returned for the input
        /// keyword value "phonebook", with the keyword "collation" (or "co").
        /// <para/>
        /// When the specified keyword is not recognized, but the specified value
        /// satisfies the syntax of the BCP 47 Unicode locale extension type,
        /// or when the specified keyword allows 'variable' type and the specified
        /// value satisfies the syntax, the lower-case version of the input value
        /// will be returned. For example,
        /// <c>ToUnicodeLocaleType("Foo", "Bar")</c> returns "bar",
        /// <c>ToUnicodeLocaleType("variableTop", "00A4")</c> returns "00a4".
        /// </summary>
        /// <param name="keyword">The locale keyword (either legacy key such as
        /// "collation" or BCP 47 Unicode locale extension
        /// key such as "co").</param>
        /// <param name="value">The locale keyword value (either legacy type
        /// such as "phonebook" or BCP 47 Unicode locale extension
        /// type such as "phonebk").</param>
        /// <returns>The well-formed BCP47 Unicode locale extension type,
        /// or <c>null</c> if the locale keyword value cannot be mapped to
        /// a well-formed BCP 47 Unicode locale extension type.</returns>
        /// <seealso cref="ToLegacyType(string, string)"/>
        /// <stable>ICU 54</stable>
        public static string? ToUnicodeLocaleType(string keyword, string value)
        {
            string? bcpType = KeyTypeData.ToBcpType(keyword, value, isKnownKey: out _, isSpecialType: out _);
            if (bcpType == null && UnicodeLocaleExtensionClass.IsType(value))
            {
                // unknown keyword, but syntax is fine..
                bcpType = AsciiUtil.ToLower(value);
            }
            return bcpType;
        }


        private static readonly Regex legacyKeyCheck = new Regex("^[0-9a-zA-Z]+$", RegexOptions.Compiled);
        private static readonly Regex legacyTypeCheck = new Regex("^[0-9a-zA-Z]+([_/\\-][0-9a-zA-Z]+)*$", RegexOptions.Compiled);

        /// <summary>
        /// <icu/> Converts the specified keyword (BCP 47 Unicode locale extension key, or
        /// legacy key) to the legacy key. For example, legacy key "collation" is
        /// returned for the input BCP 47 Unicode locale extension key "co".
        /// </summary>
        /// <param name="keyword">The input locale keyword (either BCP 47 Unicode locale
        /// extension key or legacy key).</param>
        /// <returns>The well-formed legacy key, or <c>null</c> if the specified
        /// keyword cannot be mapped to a well-formed legacy key.</returns>
        /// <seealso cref="ToUnicodeLocaleKey(string)"/>
        /// <stable>ICU 54</stable>
        public static string? ToLegacyKey(string keyword)
        {
            string? legacyKey = KeyTypeData.ToLegacyKey(keyword);
            if (legacyKey == null)
            {
                // Checks if the specified locale key is well-formed with the legacy locale syntax.
                //
                // Note:
                //  Neither ICU nor LDML/CLDR provides the definition of keyword syntax.
                //  However, a key should not contain '=' obviously. For now, all existing
                //  keys are using ASCII alphabetic letters only. We won't add any new key
                //  that is not compatible with the BCP 47 syntax. Therefore, we assume
                //  a valid key consist from [0-9a-zA-Z], no symbols.
                if (legacyKeyCheck.IsMatch(keyword))
                {
                    legacyKey = AsciiUtil.ToLower(keyword);
                }
            }
            return legacyKey;
        }

        /// <summary>
        /// <icu/> Converts the specified keyword value (BCP 47 Unicode locale extension type,
        /// or legacy type or type alias) to the canonical legacy type. For example,
        /// the legacy type "phonebook" is returned for the input BCP 47 Unicode
        /// locale extension type "phonebk" with the keyword "collation" (or "co").
        /// <para/>
        /// When the specified keyword is not recognized, but the specified value
        /// satisfies the syntax of legacy key, or when the specified keyword
        /// allows 'variable' type and the specified value satisfies the syntax,
        /// the lower-case version of the input value will be returned.
        /// For example,
        /// <code>ToLegacyType("Foo", "Bar")</code> returns "bar",
        /// <code>ToLegacyType("vt", "00A4")</code> returns "00a4".
        /// </summary>
        /// <param name="keyword">The locale keyword (either legacy keyword such as
        /// "collation" or BCP 47 Unicode locale extension
        /// key such as "co").</param>
        /// <param name="value">The locale keyword value (either BCP 47 Unicode locale
        /// extension type such as "phonebk" or legacy keyword value
        /// such as "phonebook").</param>
        /// <returns>The well-formed legacy type, or <c>null</c> if the specified
        /// keyword value cannot be mapped to a well-formed legacy
        /// type.</returns>
        /// <seealso cref="ToUnicodeLocaleType(string, string)"/>
        /// <stable>ICU 54</stable>
        public static string? ToLegacyType(string keyword, string value)
        {
            string? legacyType = KeyTypeData.ToLegacyType(keyword, value, isKnownKey: out _, isSpecialType: out _);
            if (legacyType == null)
            {
                // Checks if the specified locale type is well-formed with the legacy locale syntax.
                //
                // Note:
                //  Neither ICU nor LDML/CLDR provides the definition of keyword syntax.
                //  However, a type should not contain '=' obviously. For now, all existing
                //  types are using ASCII alphabetic letters with a few symbol letters. We won't
                //  add any new type that is not compatible with the BCP 47 syntax except timezone
                //  IDs. For now, we assume a valid type start with [0-9a-zA-Z], but may contain
                //  '-' '_' '/' in the middle.
                if (legacyTypeCheck.IsMatch(value))
                {
                    legacyType = AsciiUtil.ToLower(value);
                }
            }
            return legacyType;
        }


        internal static UCultureInfo GetInstance(BaseLocale @base, LocaleExtensions exts)
        {
            string id = LscvToID(@base.Language, @base.Script, @base.Region,
                    @base.Variant);

            Span<char> charBuffer = stackalloc char[CharStackBufferSize];
            var extKeys = exts.Keys;
            if (extKeys.Count > 0)
            {
                // legacy locale ID assume Unicode locale keywords and
                // other extensions are at the same level.
                // e.g. @a=ext-for-aa;calendar=japanese;m=ext-for-mm;x=priv-use

                JCG.SortedDictionary<string?, string?> kwds = new JCG.SortedDictionary<string?, string?>(StringComparer.Ordinal);
                foreach (char key in extKeys)
                {
                    Extension ext = exts.GetExtension(key);
                    if (ext is UnicodeLocaleExtension uext)
                    {
                        var ukeys = uext.UnicodeLocaleKeys;
                        foreach (string bcpKey in ukeys)
                        {
                            string bcpType = uext.GetUnicodeLocaleType(bcpKey);
                            // convert to legacy key/type
                            string? lkey = ToLegacyKey(bcpKey);
                            string? ltype = ToLegacyType(bcpKey, ((bcpType.Length == 0) ? "yes" : bcpType)); // use "yes" as the value of typeless keywords
                                                                                                            // special handling for u-va-posix, since this is a variant, not a keyword
                            if ("va".Equals(lkey) && "posix".Equals(ltype) && @base.Variant.Length == 0)
                            {
                                id += "_POSIX";
                            }
                            else
                            {
                                kwds[lkey] = ltype;
                            }
                        }
                        // Mapping Unicode locale attribute to the special keyword, attribute=xxx-yyy
                        var uattributes = uext.UnicodeLocaleAttributes;
                        if (uattributes.Count > 0)
                        {
                            ValueStringBuilder attrbuf = new ValueStringBuilder(charBuffer);
                            foreach (string attr in uattributes)
                            {
                                if (attrbuf.Length > 0)
                                {
                                    attrbuf.Append('-');
                                }
                                attrbuf.Append(attr);
                            }
                            kwds[LocaleAttributeKey] = attrbuf.ToString();
                        }
                    }
                    else
                    {
                        kwds[new string(new char[] { key })] = ext.Value;
                    }
                }

                if (kwds.Count > 0)
                {
                    ValueStringBuilder buf = new ValueStringBuilder(charBuffer);
                    buf.Append(id);
                    buf.Append('@');
                    bool insertSep = false;
                    foreach (var kwd in kwds)
                    {
                        if (insertSep)
                        {
                            buf.Append(';');
                        }
                        else
                        {
                            insertSep = true;
                        }
                        buf.Append(kwd.Key);
                        buf.Append('=');
                        buf.Append(kwd.Value);
                    }

                    id = buf.ToString();
                }
            }
            return new UCultureInfo(id);
        }

        internal BaseLocale Base
        {
            get
            {
                if (baseLocale == null)
                {
                    string language, script, region, variant;
                    language = script = region = variant = string.Empty;
                    if (localeID.Length > 0) // Invariant culture
                    {
                        using var lp = new LocaleIDParser(stackalloc char[CharStackBufferSize], localeID);
                        language = lp.GetLanguage();
                        script = lp.GetScript();
                        region = lp.GetCountry();
                        variant = lp.GetVariant();
                    }
                    baseLocale = BaseLocale.GetInstance(language, script, region, variant);
                }
                return baseLocale;
            }
        }

        private static readonly Regex HyphenOrUnderscore = new Regex("[-_]", RegexOptions.Compiled);

        internal LocaleExtensions LocaleExtensions
        {
            get
            {
                if (extensions == null)
                {
                    if (Keywords.Count == 0)
                    {
                        extensions = LocaleExtensions.EmptyExtensions;
                    }
                    else
                    {
                        InternalLocaleBuilder intbld = new InternalLocaleBuilder();
                        var kwitr = Keywords.GetEnumerator();
                        while (kwitr.MoveNext())
                        {
                            var pair = kwitr.Current;
                            if (pair.Key.Equals(LocaleAttributeKey))
                            {
                                // special keyword used for representing Unicode locale attributes
                                string[] uattributes = HyphenOrUnderscore.Split(pair.Value);
                                foreach (string uattr in uattributes)
                                {
                                    // ICU4N: Proactively check the parameter going in rather than responding to exceptions
                                    if (uattr != null && UnicodeLocaleExtensionClass.IsAttribute(uattr))
                                    {
                                        intbld.AddUnicodeLocaleAttribute(uattr);
                                    }
                                }
                            }
                            else if (pair.Key.Length >= 2)
                            {
                                string? bcpKey = ToUnicodeLocaleKey(pair.Key);
                                string? bcpType = ToUnicodeLocaleType(pair.Key, pair.Value);
                                if (bcpKey != null && bcpType != null)
                                {
                                    try
                                    {
                                        intbld.SetUnicodeLocaleKeyword(bcpKey, bcpType);
                                    }
                                    catch (FormatException) // ICU4N TODO: Make a TrySet version so we don't have an expensive try catch
                                    {
                                        // ignore and fall through
                                    }
                                }
                            }
                            else if (pair.Key.Length == 1 && (pair.Key[0] != UnicodeLocaleExtension))
                            {
                                try
                                {
                                    intbld.SetExtension(pair.Key[0], pair.Value.Replace(BaseLocale.Separator,
                                            LanguageTag.Separator));
                                }
                                catch (FormatException) // ICU4N TODO: Make a TrySet version so we don't have an expensive try catch
                                {
                                    // ignore and fall through
                                }
                            }
                        }
                        extensions = intbld.GetLocaleExtensions();
                    }
                }
                return extensions;
            }
        }
    }
}
