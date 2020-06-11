using ICU4N.Impl;
using ICU4N.Impl.Locale;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ICU4N.Globalization
{
#if FEATURE_CULTUREINFO_SERIALIZABLE
    [Serializable]
#endif
    public partial class UCultureInfo : CultureInfo, IComparable<UCultureInfo>
    {
        private static readonly CacheBase<string, string> nameCache = new SoftCache<string, string>();

        /// <summary>
        /// ICU locale ID, in .NET, this is referred to as the FullName.
        /// This is the <a href="https://tools.ietf.org/html/bcp47">BCP 47</a> representation of the culture.
        /// </summary>
        private readonly string localeID;

        // Used in both UCultureInfo and LocaleIDParser, so moved up here.
        private const char Underscore = '_';

        // special keyword key for Unicode locale attributes
        private const string LocaleAttributeKey = "attribute";

        /// <summary>
        /// Cache the locale data container fields.
        /// In future, we want to use them as the primary locale identifier storage.
        /// </summary>
        private volatile BaseLocale baseLocale;

        /// <summary>
        /// This table lists pairs of locale ids for canonicalization.  The
        /// The 1st item is the normalized id. The 2nd item is the
        /// canonicalized id. The 3rd is the keyword. The 4th is the keyword value.
        /// </summary>
        private static readonly string[][] CANONICALIZE_MAP = {
            new string[] { "C",              "en_US_POSIX", null, null }, /* POSIX name */
            new string[] { "art_LOJBAN",     "jbo", null, null }, /* registered name */
            new string[] { "az_AZ_CYRL",     "az_Cyrl_AZ", null, null }, /* .NET name */
            new string[] { "az_AZ_LATN",     "az_Latn_AZ", null, null }, /* .NET name */
            new string[] { "ca_ES_PREEURO",  "ca_ES", "currency", "ESP" },
            new string[] { "cel_GAULISH",    "cel__GAULISH", null, null }, /* registered name */
            new string[] { "de_1901",        "de__1901", null, null }, /* registered name */
            new string[] { "de_1906",        "de__1906", null, null }, /* registered name */
            new string[] { "de__PHONEBOOK",  "de", "collation", "phonebook" }, /* Old ICU name */
            new string[] { "de_AT_PREEURO",  "de_AT", "currency", "ATS" },
            new string[] { "de_DE_PREEURO",  "de_DE", "currency", "DEM" },
            new string[] { "de_LU_PREEURO",  "de_LU", "currency", "EUR" },
            new string[] { "el_GR_PREEURO",  "el_GR", "currency", "GRD" },
            new string[] { "en_BOONT",       "en__BOONT", null, null }, /* registered name */
            new string[] { "en_SCOUSE",      "en__SCOUSE", null, null }, /* registered name */
            new string[] { "en_BE_PREEURO",  "en_BE", "currency", "BEF" },
            new string[] { "en_IE_PREEURO",  "en_IE", "currency", "IEP" },
            new string[] { "es__TRADITIONAL", "es", "collation", "traditional" }, /* Old ICU name */
            new string[] { "es_ES_PREEURO",  "es_ES", "currency", "ESP" },
            new string[] { "eu_ES_PREEURO",  "eu_ES", "currency", "ESP" },
            new string[] { "fi_FI_PREEURO",  "fi_FI", "currency", "FIM" },
            new string[] { "fr_BE_PREEURO",  "fr_BE", "currency", "BEF" },
            new string[] { "fr_FR_PREEURO",  "fr_FR", "currency", "FRF" },
            new string[] { "fr_LU_PREEURO",  "fr_LU", "currency", "LUF" },
            new string[] { "ga_IE_PREEURO",  "ga_IE", "currency", "IEP" },
            new string[] { "gl_ES_PREEURO",  "gl_ES", "currency", "ESP" },
            new string[] { "hi__DIRECT",     "hi", "collation", "direct" }, /* Old ICU name */
            new string[] { "it_IT_PREEURO",  "it_IT", "currency", "ITL" },
            new string[] { "ja_JP_TRADITIONAL", "ja_JP", "calendar", "japanese" },
          // new string[] { "nb_NO_NY",       "nn_NO", null, null },
            new string[] { "nl_BE_PREEURO",  "nl_BE", "currency", "BEF" },
            new string[] { "nl_NL_PREEURO",  "nl_NL", "currency", "NLG" },
            new string[] { "pt_PT_PREEURO",  "pt_PT", "currency", "PTE" },
            new string[] { "sl_ROZAJ",       "sl__ROZAJ", null, null }, /* registered name */
            new string[] { "sr_SP_CYRL",     "sr_Cyrl_RS", null, null }, /* .NET name */
            new string[] { "sr_SP_LATN",     "sr_Latn_RS", null, null }, /* .NET name */
            new string[] { "sr_YU_CYRILLIC", "sr_Cyrl_RS", null, null }, /* Linux name */
            new string[] { "th_TH_TRADITIONAL", "th_TH", "calendar", "buddhist" }, /* Old ICU name */
            new string[] { "uz_UZ_CYRILLIC", "uz_Cyrl_UZ", null, null }, /* Linux name */
            new string[] { "uz_UZ_CYRL",     "uz_Cyrl_UZ", null, null }, /* .NET name */
            new string[] { "uz_UZ_LATN",     "uz_Latn_UZ", null, null }, /* .NET name */
            new string[] { "zh_CHS",         "zh_Hans", null, null }, /* .NET name */
            new string[] { "zh_CHT",         "zh_Hant", null, null }, /* .NET name */
            new string[] { "zh_GAN",         "zh__GAN", null, null }, /* registered name */
            new string[] { "zh_GUOYU",       "zh", null, null }, /* registered name */
            new string[] { "zh_HAKKA",       "zh__HAKKA", null, null }, /* registered name */
            new string[] { "zh_MIN",         "zh__MIN", null, null }, /* registered name */
            new string[] { "zh_MIN_NAN",     "zh__MINNAN", null, null }, /* registered name */
            new string[] { "zh_WUU",         "zh__WUU", null, null }, /* registered name */
            new string[] { "zh_XIANG",       "zh__XIANG", null, null }, /* registered name */
            new string[] { "zh_YUE",         "zh__YUE", null, null } /* registered name */
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
        private UCultureInfo(string name, CultureInfo culture)
            : base(name: "")
        {
            this.localeID = name;
            this.culture = culture;
        }

        /// <summary>
        /// Construct a <see cref="UCultureInfo"/> from a <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/>.</param>
        private UCultureInfo(CultureInfo culture)
            : base(name: "")
        {
            this.localeID = GetFullName(culture.ToUCultureInfo().ToString());
            this.culture = culture;
        }

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
        /// <para/>This constructor does not canonicalize the <paramref name="name"/>.  So, for
        /// example, "zh__pinyin" remains unchanged instead of converting
        /// to "zh@collation=pinyin".  By default ICU only recognizes the
        /// latter as specifying pinyin collation.  Use <see cref="CreateCanonical(string)"/>
        /// or <see cref="Canonicalize(string)"/> if you need to canonicalize the <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        public UCultureInfo(string name)
            : base(name: "")
        {
            this.localeID = GetFullName(name);
            culture = DotNetLocaleHelper.ToCultureInfo(this);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UCultureInfo"/> from the id by first canonicalizing the id.
        /// </summary>
        /// <param name="nonCanonicalID">The locale id to canonicalize.</param>
        /// <returns>The <see cref="UCultureInfo"/> created from the canonical version of the ID.</returns>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo CreateCanonical(string nonCanonicalID)
        {
            return new UCultureInfo(Canonicalize(nonCanonicalID), (CultureInfo)null);
        }

        private static string LscvToID(string lang, string script, string country, string variant)
        {
            StringBuilder buf = new StringBuilder();

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
        public CultureInfo CultureInfo => culture; // ICU4N specific: Corresponds to toLocale() in ICU4J




        /// <summary>
        /// Compares two <see cref="UCultureInfo"/> for ordering.
        /// 
        /// <para/><b>Note:</b> The order might change in future.
        /// </summary>
        /// <param name="other">The <see cref="UCultureInfo"/> to be compared.</param>
        /// <returns>A negative integer, zero, or a positive integer as this <see cref="UCultureInfo"/>
        /// is less than, equal to, or greater than <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
        /// <stable>ICU4N 60</stable>
        public int CompareTo(UCultureInfo other)
        {
            // ICU4N TODO: Finish keyword API

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (this == other)
                return 0;

            int cmp = 0;

            //// Language
            //cmp = this.GetLanguage().CompareToOrdinal(other.GetLanguage());
            //if (cmp == 0)
            //{
            //    // Script
            //    cmp = this.GetScript().CompareToOrdinal(other.GetScript());
            //    if (cmp == 0)
            //    {
            //        // Region
            //        cmp = this.GetCountry().CompareToOrdinal(other.GetCountry());
            //        if (cmp == 0)
            //        {
            //            // Variant
            //            cmp = this.GetVariant().CompareToOrdinal(other.GetVariant());
            //            if (cmp == 0)
            //            {
            //                // Keywords
            //                using (var thisKwdItr = GetKeywords())
            //                using (var otherKwdItr = other.GetKeywords())
            //                {

            //                    if (thisKwdItr == null)
            //                    {
            //                        cmp = otherKwdItr == null ? 0 : -1;
            //                    }
            //                    else if (otherKwdItr == null)
            //                    {
            //                        cmp = 1;
            //                    }
            //                    else
            //                    {
            //                        // Both have keywords
            //                        while (cmp == 0 && thisKwdItr.MoveNext())
            //                        {
            //                            if (!otherKwdItr.MoveNext())
            //                            {
            //                                cmp = 1;
            //                                break;
            //                            }
            //                            // Compare keyword keys
            //                            string thisKey = thisKwdItr.Current;
            //                            string otherKey = otherKwdItr.Current;
            //                            cmp = thisKey.CompareToOrdinal(otherKey);
            //                            if (cmp == 0)
            //                            {
            //                                // Compare keyword values
            //                                string thisVal = GetKeywordValue(thisKey);
            //                                string otherVal = other.GetKeywordValue(otherKey);
            //                                if (thisVal == null)
            //                                {
            //                                    cmp = otherVal == null ? 0 : -1;
            //                                }
            //                                else if (otherVal == null)
            //                                {
            //                                    cmp = 1;
            //                                }
            //                                else
            //                                {
            //                                    cmp = thisVal.CompareToOrdinal(otherVal);
            //                                }
            //                            }
            //                        }
            //                        if (cmp == 0 && otherKwdItr.MoveNext())
            //                        {
            //                            cmp = -1;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            // Normalize the result value:
            // Note: string.compareTo() may return value other than -1, 0, 1.
            // A value other than those are OK by the definition, but we don't want
            // associate any semantics other than negative/zero/positive.
            return (cmp < 0) ? -1 : ((cmp > 0) ? 1 : 0);
        }

        /// <summary>
        /// Returns the language code for this locale, which will either be the empty string
        /// or a lowercase ISO 639 code.
        /// </summary>
        /// <seealso cref="DisplayLanguage"/>
        /// <seealso cref="GetDisplayLanguage(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Language => Base.GetLanguage();

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
            return new LocaleIDParser(localeID).GetLanguage();
        }

        /// <summary>
        /// Returns the script code for this locale, which might be the empty string.
        /// </summary>
        /// <seealso cref="DisplayScript"/>
        /// <seealso cref="GetDisplayScript(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Script => Base.GetScript();

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
            return new LocaleIDParser(localeID).GetScript();
        }

        /// <summary>
        /// Returns the country/region code for this locale, which will either be the empty string
        /// or an uppercase ISO 3166 2-letter code.
        /// </summary>
        /// <seealso cref="DisplayCountry"/>
        /// <seealso cref="GetDisplayCountry(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Country => Base.GetRegion();

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
            return new LocaleIDParser(localeID).GetCountry();
        }

        /// <summary>
        /// Returns the variant code for this locale, which might be the empty string.
        /// </summary>
        /// <seealso cref="DisplayVariant"/>
        /// <seealso cref="GetDisplayVariant(UCultureInfo)"/>
        /// <stable>ICU4N 60</stable>
        public string Variant => Base.GetVariant();

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
            return new LocaleIDParser(localeID).GetVariant();
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
            return new LocaleIDParser(localeID).GetBaseName();
        }

        /// <summary>
        /// <icu/> Returns the (normalized) full name for this culture.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        // ICU4N specific: Renamed from Name (and BaseName was renamed to Name, since it does not contain keywords the same as .NET).
        public virtual string FullName => localeID; // always normalized

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
        public static string GetFullName(string name) // ICU4N specific - renamed from GetName() in ICU4J (since this would cause a collision with BaseName that has no keywords)
        {
            string tmpLocaleID;
            // Convert BCP47 id if necessary
            if (name != null && !name.Contains("@") && GetShortestSubtagLength(name) == 1)
            {
                tmpLocaleID = ForLanguageTag(name).FullName;
                if (tmpLocaleID.Length == 0)
                {
                    tmpLocaleID = name;
                }
            }
            else
            {
                tmpLocaleID = name;
            }
            return nameCache.GetOrCreate(tmpLocaleID, (key) => new LocaleIDParser(key).GetName());
        }

        /// <summary>
        /// <icu/> Returns the canonical name for the specified locale ID.  This is used to
        /// convert POSIX and other grandfathered IDs to standard ICU form.
        /// </summary>
        /// <param name="localeID">The locale ID.</param>
        /// <returns>The canonicalized ID.</returns>
        /// <stable>ICU 3.0</stable>
        public static string Canonicalize(string localeID)
        {
            LocaleIDParser parser = new LocaleIDParser(localeID, true);
            string baseName = parser.GetBaseName();
            bool foundVariant = false;

            // formerly, we always set to en_US_POSIX if the basename was empty, but
            // now we require that the entire id be empty, so that "@foo=bar"
            // will pass through unchanged.
            // {dlf} I'd rather keep "" unchanged.
            if (localeID.Equals(""))
            {
                return "";
                //              return "en_US_POSIX";
            }

            // we have an ID in the form xx_Yyyy_ZZ_KKKKK

            /* convert the variants to appropriate ID */
            for (int i = 0; i < variantsToKeywords.Length; i++)
            {
                string[] vals = variantsToKeywords[i];
                int idx = baseName.LastIndexOf("_" + vals[0], StringComparison.Ordinal);
                if (idx > -1)
                {
                    foundVariant = true;

                    baseName = baseName.Substring(0, idx - 0); // ICU4N: Checked 2nd parameter
                    if (baseName.EndsWith("_", StringComparison.Ordinal))
                    {
                        baseName = baseName.Substring(0, (--idx - 0)); // ICU4N: Checked 2nd parameter
                    }
                    parser.SetBaseName(baseName);
                    parser.DefaultKeywordValue(vals[1], vals[2]);
                    break;
                }
            }

            /* See if this is an already known locale */
            for (int i = 0; i < CANONICALIZE_MAP.Length; i++)
            {
                if (CANONICALIZE_MAP[i][0].Equals(baseName))
                {
                    foundVariant = true;

                    string[] vals = CANONICALIZE_MAP[i];
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
                if (parser.GetLanguage().Equals("nb") && parser.GetVariant().Equals("NY"))
                {
                    parser.SetBaseName(LscvToID("nn", parser.GetScript(), parser.GetCountry(), null));
                }
            }

            return parser.GetName();
        }


        /////**
        ////* Returns a well-formed IETF BCP 47 language tag representing
        ////* this locale.
        ////*
        ////* <para/>If this <code>ULocale</code> has a language, script, country, or
        ////* variant that does not satisfy the IETF BCP 47 language tag
        ////* syntax requirements, this method handles these fields as
        ////* described below:
        ////*
        ////* <para/><b>Language:</b> If language is empty, or not well-formed
        ////* (for example "a" or "e2"), it will be emitted as "und" (Undetermined).
        ////*
        ////* <para/><b>Script:</b> If script is not well-formed (for example "12"
        ////* or "Latin"), it will be omitted.
        ////*
        ////* <para/><b>Country:</b> If country is not well-formed (for example "12"
        ////* or "USA"), it will be omitted.
        ////*
        ////* <para/><b>Variant:</b> If variant <b>is</b> well-formed, each sub-segment
        ////* (delimited by '-' or '_') is emitted as a subtag.  Otherwise:
        ////* <ul>
        ////*
        ////* <li>if all sub-segments match <code>[0-9a-zA-Z]{1,8}</code>
        ////* (for example "WIN" or "Oracle_JDK_Standard_Edition"), the first
        ////* ill-formed sub-segment and all following will be appended to
        ////* the private use subtag.  The first appended subtag will be
        ////* "lvariant", followed by the sub-segments in order, separated by
        ////* hyphen. For example, "x-lvariant-WIN",
        ////* "Oracle-x-lvariant-JDK-Standard-Edition".</li>
        ////*
        ////* <li>if any sub-segment does not match
        ////* <code>[0-9a-zA-Z]{1,8}</code>, the variant will be truncated
        ////* and the problematic sub-segment and all following sub-segments
        ////* will be omitted.  If the remainder is non-empty, it will be
        ////* emitted as a private use subtag as above (even if the remainder
        ////* turns out to be well-formed).  For example,
        ////* "Solaris_isjustthecoolestthing" is emitted as
        ////* "x-lvariant-Solaris", not as "solaris".</li></ul>
        ////*
        ////* <para/><b>Note:</b> Although the language tag created by this
        ////* method is well-formed (satisfies the syntax requirements
        ////* defined by the IETF BCP 47 specification), it is not
        ////* necessarily a valid BCP 47 language tag.  For example,
        ////* <pre>
        ////*   new Locale("xx", "YY").toLanguageTag();</pre>
        ////*
        ////* will return "xx-YY", but the language subtag "xx" and the
        ////* region subtag "YY" are invalid because they are not registered
        ////* in the IANA Language Subtag Registry.
        ////*
        ////* @return a BCP47 language tag representing the locale
        ////* @see #forLanguageTag(string)
        ////*
        ////* @stable ICU 4.2
        ////*/
        ////internal string ToLanguageTag() // ICU4N TODO: API Make this into the Name property (cached) and replace Name with BaseName property..?
        ////{
        ////    BaseLocale @base = Base;
        ////    LocaleExtensions exts = Extensions;

        ////    if (@base.GetVariant().Equals("POSIX", StringComparison.OrdinalIgnoreCase))
        ////    {
        ////        // special handling for variant POSIX
        ////        @base = BaseLocale.GetInstance(@base.GetLanguage(), @base.GetScript(), @base.GetRegion(), "");
        ////        if (exts.GetUnicodeLocaleType("va") == null)
        ////        {
        ////            // add va-posix
        ////            InternalLocaleBuilder ilocbld = new InternalLocaleBuilder();
        ////            try
        ////            {
        ////                ilocbld.SetLocale(BaseLocale.Root, exts);
        ////                ilocbld.SetUnicodeLocaleKeyword("va", "posix");
        ////                exts = ilocbld.GetLocaleExtensions();
        ////            }
        ////            catch (FormatException e)
        ////            {
        ////                // this should not happen
        ////                throw new Exception(e.ToString(), e);
        ////            }
        ////        }
        ////    }

        ////    LanguageTag tag = LanguageTag.ParseLocale(@base, exts);

        ////    StringBuilder buf = new StringBuilder();
        ////    string subtag = tag.Language;
        ////    if (subtag.Length > 0)
        ////    {
        ////        buf.Append(LanguageTag.CanonicalizeLanguage(subtag));
        ////    }

        ////    subtag = tag.Script;
        ////    if (subtag.Length > 0)
        ////    {
        ////        buf.Append(LanguageTag.Separator);
        ////        buf.Append(LanguageTag.CanonicalizeScript(subtag));
        ////    }

        ////    subtag = tag.Region;
        ////    if (subtag.Length > 0)
        ////    {
        ////        buf.Append(LanguageTag.Separator);
        ////        buf.Append(LanguageTag.CanonicalizeRegion(subtag));
        ////    }

        ////    IList<string> subtags = tag.Variants;
        ////    foreach (string s in subtags)
        ////    {
        ////        buf.Append(LanguageTag.Separator);
        ////        buf.Append(LanguageTag.CanonicalizeVariant(s));
        ////    }

        ////    subtags = tag.Extensions;
        ////    foreach (string s in subtags)
        ////    {
        ////        buf.Append(LanguageTag.Separator);
        ////        buf.Append(LanguageTag.CanonicalizeExtension(s));
        ////    }

        ////    subtag = tag.PrivateUse;
        ////    if (subtag.Length > 0)
        ////    {
        ////        if (buf.Length > 0)
        ////        {
        ////            buf.Append(LanguageTag.Separator);
        ////        }
        ////        buf.Append(LanguageTag.Private_Use).Append(LanguageTag.Separator);
        ////        buf.Append(LanguageTag.CanonicalizePrivateuse(subtag));
        ////    }

        ////    return buf.ToString();
        ////}

        /**
         * Returns a locale for the specified IETF BCP 47 language tag string.
         *
         * <para/>If the specified language tag contains any ill-formed subtags,
         * the first such subtag and all following subtags are ignored.  Compare
         * to {@link ULocale.Builder#setLanguageTag} which throws an exception
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
         *     ULocale loc;
         *     loc = ULocale.forLanguageTag("en-US-x-lvariant-icu4j);
         *     loc.getVariant(); // returns "ICU4J"
         *     loc.getExtension('x'); // returns null
         *
         *     loc = Locale.forLanguageTag("de-icu4j-x-URP-lvariant-Abc-Def");
         *     loc.getVariant(); // returns "ICU4J_ABC_DEF"
         *     loc.getExtension('x'); // returns "urp"
         * </pre></li>
         *
         * <li>When the languageTag argument contains an extlang subtag,
         * the first such subtag is used as the language, and the primary
         * language subtag and other extlang subtags are ignored:
         *
         * <pre>
         *     ULocale.forLanguageTag("ar-aao").getLanguage(); // returns "aao"
         *     ULocale.forLanguageTag("en-abc-def-us").toString(); // returns "abc_US"
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
         * @see ULocale.Builder#setLanguageTag(string)
         *
         * @stable ICU 4.2
         */
        public static UCultureInfo ForLanguageTag(string languageTag)
        {
            if (languageTag == null)
                throw new ArgumentNullException(nameof(languageTag));

            LanguageTag tag = LanguageTag.Parse(languageTag, null);
            InternalLocaleBuilder bldr = new InternalLocaleBuilder();
            bldr.SetLanguageTag(tag);
            return GetInstance(bldr.GetBaseLocale(), bldr.GetLocaleExtensions());
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
        public static string ToLegacyKey(string keyword)
        {
            string legacyKey = KeyTypeData.ToLegacyKey(keyword);
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
        /// <seealso cref="ToUnicodeLocaleType(string)"/>
        /// <stable>ICU 54</stable>
        public static string ToLegacyType(string keyword, string value)
        {
            string legacyType = KeyTypeData.ToLegacyType(keyword, value, isKnownKey: out _, isSpecialType: out _);
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


        private static UCultureInfo GetInstance(BaseLocale @base, LocaleExtensions exts)
        {
            string id = LscvToID(@base.GetLanguage(), @base.GetScript(), @base.GetRegion(),
                    @base.GetVariant());

            var extKeys = exts.Keys;
            if (extKeys.Count > 0)
            {
                // legacy locale ID assume Unicode locale keywords and
                // other extensions are at the same level.
                // e.g. @a=ext-for-aa;calendar=japanese;m=ext-for-mm;x=priv-use

                SortedDictionary<string, string> kwds = new SortedDictionary<string, string>();
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
                            string lkey = ToLegacyKey(bcpKey);
                            string ltype = ToLegacyType(bcpKey, ((bcpType.Length == 0) ? "yes" : bcpType)); // use "yes" as the value of typeless keywords
                                                                                                            // special handling for u-va-posix, since this is a variant, not a keyword
                            if (lkey.Equals("va") && ltype.Equals("posix") && @base.GetVariant().Length == 0)
                            {
                                id = id + "_POSIX";
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
                            StringBuilder attrbuf = new StringBuilder();
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
                    StringBuilder buf = new StringBuilder(id);
                    buf.Append("@");
                    bool insertSep = false;
                    foreach (var kwd in kwds)
                    {
                        if (insertSep)
                        {
                            buf.Append(";");
                        }
                        else
                        {
                            insertSep = true;
                        }
                        buf.Append(kwd.Key);
                        buf.Append("=");
                        buf.Append(kwd.Value);
                    }

                    id = buf.ToString();
                }
            }
            return new UCultureInfo(id);
        }

        private BaseLocale Base
        {
            get
            {
                if (baseLocale == null)
                {
                    string language, script, region, variant;
                    language = script = region = variant = string.Empty;
                    if (!localeID.Equals(string.Empty)) // Invariant culture
                    {
                        LocaleIDParser lp = new LocaleIDParser(localeID);
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

        //private LocaleExtensions Extensions
        //{
        //    get
        //    {
        //        if (extensions == null)
        //        {
        //            var kwitr = GetKeywords();
        //            if (kwitr == null)
        //            {
        //                extensions = LocaleExtensions.EmptyExtensions;
        //            }
        //            else
        //            {
        //                InternalLocaleBuilder intbld = new InternalLocaleBuilder();
        //                while (kwitr.MoveNext())
        //                {
        //                    string key = kwitr.Current;
        //                    if (key.Equals(LOCALE_ATTRIBUTE_KEY))
        //                    {
        //                        // special keyword used for representing Unicode locale attributes
        //                        string[] uattributes = Regex.Split(GetKeywordValue(key), "[-_]");
        //                        foreach (string uattr in uattributes)
        //                        {
        //                            // ICU4N: Proactively check the parameter going in rather than responding to exceptions
        //                            if (uattr != null || UnicodeLocaleExtension.IsAttribute(uattr))
        //                            {
        //                                intbld.AddUnicodeLocaleAttribute(uattr);
        //                            }

        //                            //try
        //                            //{
        //                            //    intbld.AddUnicodeLocaleAttribute(uattr);
        //                            //}
        //                            //catch (FormatException e)
        //                            //{
        //                            //    // ignore and fall through
        //                            //}
        //                        }
        //                    }
        //                    else if (key.Length >= 2)
        //                    {
        //                        string bcpKey = ToUnicodeLocaleKey(key);
        //                        string bcpType = ToUnicodeLocaleType(key, GetKeywordValue(key));
        //                        if (bcpKey != null && bcpType != null)
        //                        {
        //                            try
        //                            {
        //                                intbld.SetUnicodeLocaleKeyword(bcpKey, bcpType);
        //                            }
        //                            catch (FormatException) // ICU4N TODO: Make a TrySet version so we don't have an expensive try catch
        //                            {
        //                                // ignore and fall through
        //                            }
        //                        }
        //                    }
        //                    else if (key.Length == 1 && (key[0] != UNICODE_LOCALE_EXTENSION))
        //                    {
        //                        try
        //                        {
        //                            intbld.SetExtension(key[0], GetKeywordValue(key).Replace("_",
        //                                    LanguageTag.Separator));
        //                        }
        //                        catch (FormatException) // ICU4N TODO: Make a TrySet version so we don't have an expensive try catch
        //                        {
        //                            // ignore and fall through
        //                        }
        //                    }
        //                }
        //                extensions = intbld.GetLocaleExtensions();
        //            }
        //        }
        //        return extensions;
        //    }
        //}
    }
}
