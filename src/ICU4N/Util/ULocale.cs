using ICU4N.Impl;
using ICU4N.Impl.Locale;
using ICU4N.Globalization;
using ICU4N.Support.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using static ICU4N.Text.LocaleDisplayNames;

namespace ICU4N.Util
{
    // ICU4N TODO: API - The approach done in ULocale will not be completely compatible in .NET becauuse there is no way to apply
    // it to the current thread. Thus, using ICU functionality in a multi-threaded environment is only truely possible using
    // CultureInfo.

    // The approach we need to take is to subclass CultureInfo (preferably in an ICU4N.Globalization namespace) in order to make
    // it possible to use ICU functionality on the current thread. ULocale.Category should not be used:

    // ULocale.Category.FORMAT =>      CultureInfo.CurrentCulture
    // ULocale.Category.DISPLAY =>     CultureInfo.CurrentUICulture

    // The subclass could be made to distinguish between .NET style locale names (en-US) and ICU style locale names (en_US).
    // If the former, it could simply wrap an instance of CultureInfo. If the latter, it would act more like ULocale, overriding
    // calendars, numbering sytems, and dates for the type.

    // Should UCultureInfo also control culture sensitive info throughout the whole .NET architecture, or was ULocale intended to
    // only be used with ICU compoenents? Being that the Locale class is sealed in Java, it is difficult to tell if ULocale was
    // intended to completely override Locale, or simply to control what happens within the boundaries of ICU.
    // But the answer to this will determine if ICU should override the calendars, numbering systems, and dates, of CultureInfo 
    // or should simply add additional properties and/or extension methods to CultureInfo that only apply to ICU types.

    public sealed class ULocale : IComparable<ULocale>
    {
        private static readonly object syncLock = new object();

        private static readonly CacheBase<string, string, object> nameCache = new NameCache();

        private class NameCache : SoftCache<string, string, object>
        {
            protected override string CreateInstance(string tmpLocaleID, object unused)
            {
                return new LocaleIDParser(tmpLocaleID).GetName();
            }
        }

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale ENGLISH = new ULocale("en", new CultureInfo("en"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale FRENCH = new ULocale("fr", new CultureInfo("fr"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale GERMAN = new ULocale("de", new CultureInfo("de"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale ITALIAN = new ULocale("it", new CultureInfo("it"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale JAPANESE = new ULocale("ja", new CultureInfo("ja"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale KOREAN = new ULocale("ko", new CultureInfo("ko"));

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale CHINESE = new ULocale("zh", new CultureInfo("zh"));


        // Special note about static initializer for
        //   - SIMPLIFIED_CHINESE
        //   - TRADTIONAL_CHINESE
        //   - CHINA
        //   - TAIWAN
        //
        // Equivalent JDK Locale for ULocale.SIMPLIFIED_CHINESE is different
        // by JRE version. JRE 7 or later supports a script tag "Hans", while
        // JRE 6 or older does not. JDK's Locale.SIMPLIFIED_CHINESE is actually
        // zh_CN, not zh_Hans. This is same in Java 7 or later versions.
        //
        // ULocale#toLocale() implementation uses Java reflection to create a Locale
        // with a script tag. When a new ULocale is constructed with the single arg
        // constructor, the volatile field 'Locale locale' is initialized by
        // #toLocale() method.
        //
        // Because we cannot hardcode corresponding JDK Locale representation below,
        // SIMPLIFIED_CHINESE is constructed without JDK Locale argument, and
        // #toLocale() is used for resolving the best matching JDK Locale at runtime.
        //
        // The same thing applies to TRADITIONAL_CHINESE.

        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale SIMPLIFIED_CHINESE = new ULocale("zh_Hans");


        /**
         * Useful constant for language.
         * @stable ICU 3.0
         */
        public static readonly ULocale TRADITIONAL_CHINESE = new ULocale("zh_Hant");

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale FRANCE = new ULocale("fr_FR", new CultureInfo("fr-FR"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale GERMANY = new ULocale("de_DE", new CultureInfo("de-DE"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale ITALY = new ULocale("it_IT", new CultureInfo("it-IT"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale JAPAN = new ULocale("ja_JP", new CultureInfo("ja-JP"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale KOREA = new ULocale("ko_KR", new CultureInfo("ko-KR"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale CHINA = new ULocale("zh_Hans_CN");

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale PRC = CHINA;

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale TAIWAN = new ULocale("zh_Hant_TW");

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale UK = new ULocale("en_GB", new CultureInfo("en-GB"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale US = new ULocale("en_US", new CultureInfo("en-US"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale CANADA = new ULocale("en_CA", new CultureInfo("en-CA"));

        /**
         * Useful constant for country/region.
         * @stable ICU 3.0
         */
        public static readonly ULocale CANADA_FRENCH = new ULocale("fr_CA", new CultureInfo("fr-CA"));

        /**
         * Handy constant.
         */
        private static readonly string EMPTY_STRING = "";

        // Used in both ULocale and LocaleIDParser, so moved up here.
        private static readonly char UNDERSCORE = '_';

        // default empty locale
        private static readonly CultureInfo EMPTY_LOCALE = CultureInfo.InvariantCulture;

        // special keyword key for Unicode locale attributes
        private static readonly string LOCALE_ATTRIBUTE_KEY = "attribute";

        /**
         * The root ULocale.
         * @stable ICU 2.8
         */
        public static readonly ULocale ROOT = new ULocale("", EMPTY_LOCALE);

        /**
         * Enum for locale categories. These locale categories are used to get/set the default locale for
         * the specific functionality represented by the category.
         * @stable ICU 49
         */
        public enum Category // ICU4N TODO: API - de-nest this enum
        {
            /**
             * Category used to represent the default locale for displaying user interfaces.
             * @stable ICU 49
             */
            DISPLAY, // ICU4N TODO: API - this corresponds to CultureInfo.CurrentUICulture in .NET
            /**
             * Category used to represent the default locale for formatting date, number and/or currency.
             * @stable ICU 49
             */
            FORMAT // ICU4N TODO: API - this corresponds to CultureInfo.CurrentCulture in .NET
        }

        private static readonly SoftCache<CultureInfo, ULocale, object> CACHE = new ULocaleCache();

        private class ULocaleCache : SoftCache<CultureInfo, ULocale, object>
        {
            protected override ULocale CreateInstance(CultureInfo key, object unused)
            {
                return DotNetLocaleHelper.ToULocale(key);
            }
        }

        /**
         * Cache the locale.
         */
        private volatile CultureInfo locale;

        /**
         * The raw localeID that we were passed in.
         */
        private string localeID;

        /**
         * Cache the locale data container fields.
         * In future, we want to use them as the primary locale identifier storage.
         */
        private volatile BaseLocale baseLocale;
        private volatile LocaleExtensions extensions;

        /**
         * This table lists pairs of locale ids for canonicalization.  The
         * The 1st item is the normalized id. The 2nd item is the
         * canonicalized id. The 3rd is the keyword. The 4th is the keyword value.
         */
        private static string[][] CANONICALIZE_MAP = {
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

        /**
         * This table lists pairs of locale ids for canonicalization.
         * The first item is the normalized variant id.
         */
        private static string[][] variantsToKeywords = {
            new string[] { "EURO",   "currency", "EUR" },
            new string[] { "PINYIN", "collation", "pinyin" }, /* Solaris variant */
            new string[] { "STROKE", "collation", "stroke" }  /* Solaris variant */
        };


        /**
         * Private constructor used by static initializers.
         */
        private ULocale(string localeID, CultureInfo locale)
        {
            this.localeID = localeID;
            this.locale = locale;
        }

        /**
         * Construct a ULocale object from a {@link java.util.Locale}.
         * @param loc a {@link java.util.Locale}
         */
        private ULocale(CultureInfo loc)
        {
            this.localeID = GetName(ForLocale(loc).ToString());
            this.locale = loc;
        }

        /**
         * {@icu} Returns a ULocale object for a {@link java.util.Locale}.
         * The ULocale is canonicalized.
         * @param loc a {@link java.util.Locale}
         * @stable ICU 3.2
         */
        public static ULocale ForLocale(CultureInfo loc)
        {
            if (loc == null)
            {
                return null;
            }
            return CACHE.GetInstance(loc, null /* unused */);
        }

        /**
         * {@icu} Constructs a ULocale from a RFC 3066 locale ID. The locale ID consists
         * of optional language, script, country, and variant fields in that order,
         * separated by underscores, followed by an optional keyword list.  The
         * script, if present, is four characters long-- this distinguishes it
         * from a country code, which is two characters long.  Other fields
         * are distinguished by position as indicated by the underscores.  The
         * start of the keyword list is indicated by '@', and consists of two
         * or more keyword/value pairs separated by semicolons(';').
         *
         * <para/>This constructor does not canonicalize the localeID.  So, for
         * example, "zh__pinyin" remains unchanged instead of converting
         * to "zh@collation=pinyin".  By default ICU only recognizes the
         * latter as specifying pinyin collation.  Use {@link #createCanonical}
         * or {@link #canonicalize} if you need to canonicalize the localeID.
         *
         * @param localeID string representation of the locale, e.g:
         * "en_US", "sy_Cyrl_YU", "zh__pinyin", "es_ES@currency=EUR;collation=traditional"
         * @stable ICU 2.8
         */
        public ULocale(string localeID)
        {
            this.localeID = GetName(localeID);
        }

        /**
         * Convenience overload of ULocale(string, string, string) for
         * compatibility with java.util.Locale.
         * @see #ULocale(string, string, string)
         * @stable ICU 3.4
         */
        public ULocale(string a, string b)
                    : this(a, b, null)
        {
        }

        /**
         * Constructs a ULocale from a localeID constructed from the three 'fields' a, b, and
         * c.  These fields are concatenated using underscores to form a localeID of the form
         * a_b_c, which is then handled like the localeID passed to <code>ULocale(string
         * localeID)</code>.
         *
         * <para/>Java locale strings consisting of language, country, and
         * variant will be handled by this form, since the country code
         * (being shorter than four letters long) will not be interpreted
         * as a script code.  If a script code is present, the final
         * argument ('c') will be interpreted as the country code.  It is
         * recommended that this constructor only be used to ease porting,
         * and that clients instead use the single-argument constructor
         * when constructing a ULocale from a localeID.
         * @param a first component of the locale id
         * @param b second component of the locale id
         * @param c third component of the locale id
         * @see #ULocale(string)
         * @stable ICU 3.0
         */
        public ULocale(string a, string b, string c)
        {
            localeID = GetName(LscvToID(a, b, c, EMPTY_STRING));
        }

        /**
         * {@icu} Creates a ULocale from the id by first canonicalizing the id.
         * @param nonCanonicalID the locale id to canonicalize
         * @return the locale created from the canonical version of the ID.
         * @stable ICU 3.0
         */
        public static ULocale CreateCanonical(string nonCanonicalID)
        {
            return new ULocale(Canonicalize(nonCanonicalID), (CultureInfo)null);
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
                buf.Append(UNDERSCORE);
                buf.Append(script);
            }
            if (country != null && country.Length > 0)
            {
                buf.Append(UNDERSCORE);
                buf.Append(country);
            }
            if (variant != null && variant.Length > 0)
            {
                if (country == null || country.Length == 0)
                {
                    buf.Append(UNDERSCORE);
                }
                buf.Append(UNDERSCORE);
                buf.Append(variant);
            }
            return buf.ToString();
        }

        /**
         * {@icu} Converts this ULocale object to a {@link java.util.Locale}.
         * @return a {@link java.util.Locale} that either exactly represents this object
         * or is the closest approximation.
         * @stable ICU 2.8
         */
        public CultureInfo ToLocale() // ICU4N TODO: API - rename ToCultureInfo()
        {
            if (locale == null)
            {
                locale = DotNetLocaleHelper.ToLocale(this);
            }
            return locale;
        }

        /**
         * Keep our own default ULocale.
         */
        private static CultureInfo defaultLocale = CultureInfo.CurrentCulture;
        private static ULocale defaultULocale;

        private static CultureInfo[] defaultCategoryLocales = new CultureInfo[Enum.GetValues(typeof(Category)).Length];
        private static ULocale[] defaultCategoryULocales = new ULocale[Enum.GetValues(typeof(Category)).Length];

        static ULocale()
        {
            defaultULocale = ForLocale(defaultLocale);

            // For Java 6 or older JRE, ICU initializes the default script from
            // "user.script" system property. The system property was added
            // in Java 7. On JRE 7, Locale.getDefault() should reflect the
            // property value to the Locale's default. So ICU just relies on
            // Locale.getDefault().

            // Note: The "user.script" property is only used by initialization.
            //
            //if (DotNetLocaleHelper.HasLocaleCategories())
            //{
            foreach (Category cat in Enum.GetValues(typeof(Category)))
            {
                int idx = (int)cat;
                defaultCategoryLocales[idx] = DotNetLocaleHelper.GetDefault(cat);
                defaultCategoryULocales[idx] = ForLocale(defaultCategoryLocales[idx]);
            }
            //}
            //else
            //{
            //    // Make sure the current default Locale is original.
            //    // If not, it means that someone updated the default Locale.
            //    // In this case, user.XXX properties are already out of date
            //    // and we should not use user.script.
            //    if (DotNetLocaleHelper.IsOriginalDefaultLocale(defaultLocale))
            //    {
            //        // Use "user.script" if available
            //        string userScript = JDKLocaleHelper.GetSystemProperty("user.script");
            //        if (userScript != null && LanguageTag.IsScript(userScript))
            //        {
            //            // Note: Builder or forLanguageTag cannot be used here
            //            // when one of Locale fields is not well-formed.
            //            BaseLocale @base = defaultULocale.Base();
            //            BaseLocale newBase = BaseLocale.GetInstance(@base.GetLanguage(), userScript,
            //                    @base.GetRegion(), @base.GetVariant());
            //            defaultULocale = GetInstance(newBase, defaultULocale.Extensions());
            //        }
            //    }

            //    // Java 6 or older does not have separated category locales,
            //    // use the non-category default for all
            //    foreach (Category cat in Enum.GetValues(typeof(Category)))
            //    {
            //        int idx = (int)cat;
            //        defaultCategoryLocales[idx] = defaultLocale;
            //        defaultCategoryULocales[idx] = defaultULocale;
            //    }
            //}
        }

        /**
         * Returns the current default ULocale.
         * <para/>
         * The default ULocale is synchronized to the default Java Locale. This method checks
         * the current default Java Locale and returns an equivalent ULocale.
         * <para/>
         * <b>Note:</b> Before Java 7, the {@link java.util.Locale} was not able to represent a
         * locale's script. Therefore, the script field in the default ULocale is always empty unless
         * a ULocale with non-empty script is explicitly set by {@link #setDefault(ULocale)}
         * on Java 6 or older systems.
         * <para/>
         * <b>Note for ICU 49 or later:</b> Some JRE implementations allow users to override the default
         * {@link java.util.Locale} using system properties - <code>user.language</code>,
         * <code>user.country</code> and <code>user.variant</code>. In addition to these system
         * properties, some Java 7 implementations support <code>user.script</code> for overriding the
         * default Locale's script.
         * ICU 49 and later versions use the <code>user.script</code> system property on Java 6
         * or older systems supporting other <code>user.*</code> system properties to initialize
         * the default ULocale. The <code>user.script</code> override for default ULocale is not
         * used on Java 7, or if the current Java default Locale is changed after start up.
         *
         * @return the default ULocale.
         * @stable ICU 2.8
         */
        public static ULocale GetDefault() // ICU4N TODO: API - rename CurrentCulture?
        {
            lock (syncLock)
            {
                if (defaultULocale == null)
                {
                    // When Java's default locale has extensions (such as ja-JP-u-ca-japanese),
                    // Locale -> ULocale mapping requires BCP47 keyword mapping data that is currently
                    // stored in a resource bundle. However, UResourceBundle currently requires
                    // non-null default ULocale. For now, this implementation returns ULocale.ROOT
                    // to avoid the problem.

                    // TODO: Consider moving BCP47 mapping data out of resource bundle later.

                    return ULocale.ROOT;
                }
                CultureInfo currentDefault = CultureInfo.CurrentCulture;
                if (!defaultLocale.Equals(currentDefault))
                {
                    defaultLocale = currentDefault;
                    defaultULocale = ForLocale(currentDefault);

                    //if (!JDKLocaleHelper.HasLocaleCategories())
                    //{

                    // Detected Java default Locale change.
                    // We need to update category defaults to match the
                    // Java 7's behavior on Java 6 or older environment.
                    foreach (Category cat in Enum.GetValues(typeof(Category)))
                    {
                        int idx = (int)cat;
                        defaultCategoryLocales[idx] = currentDefault;
                        defaultCategoryULocales[idx] = ForLocale(currentDefault);
                    }

                    //}
                }
                return defaultULocale;
            }
        }

        /**
         * Sets the default ULocale.  This also sets the default Locale.
         * If the caller does not have write permission to the
         * user.language property, a security exception will be thrown,
         * and the default ULocale will remain unchanged.
         * <para/>
         * By setting the default ULocale with this method, all of the default categoy locales
         * are also set to the specified default ULocale.
         * @param newLocale the new default locale
         * @throws SecurityException if a security manager exists and its
         *        <code>checkPermission</code> method doesn't allow the operation.
         * @throws NullPointerException if <code>newLocale</code> is null
         * @see SecurityManager#checkPermission(java.security.Permission)
         * @see java.util.PropertyPermission
         * @see ULocale#setDefault(Category, ULocale)
         * @stable ICU 3.0
         */
        public static void SetDefault(ULocale newLocale)
        {
            lock (syncLock)
            {
                defaultLocale = newLocale.ToLocale();
                //Locale.setDefault(defaultLocale);
#if NETSTANDARD
                CultureInfo.CurrentCulture = defaultLocale;
#else
                System.Threading.Thread.CurrentThread.CurrentCulture = defaultLocale;
#endif

                defaultULocale = newLocale;
                // This method also updates all category default locales
                foreach (Category cat in Enum.GetValues(typeof(Category)))
                {
                    SetDefault(cat, newLocale);
                }
            }
        }

        /**
         * Returns the current default ULocale for the specified category.
         *
         * @param category the category
         * @return the default ULocale for the specified category.
         * @stable ICU 49
         */
        public static ULocale GetDefault(Category category)
        {
            lock (syncLock)
            {
                int idx = (int)category;
                if (defaultCategoryULocales[idx] == null)
                {
                    // Just in case this method is called during ULocale class
                    // initialization. Unlike getDefault(), we do not have
                    // cyclic dependency for category default.
                    return ULocale.ROOT;
                }
                //if (JDKLocaleHelper.HasLocaleCategories())
                //{
                CultureInfo currentCategoryDefault = DotNetLocaleHelper.GetDefault(category);
                if (!defaultCategoryLocales[idx].Equals(currentCategoryDefault))
                {
                    defaultCategoryLocales[idx] = currentCategoryDefault;
                    defaultCategoryULocales[idx] = ForLocale(currentCategoryDefault);
                }
                //}
                //else
                //{
                //    // java.util.Locale.setDefault(Locale) in Java 7 updates
                //    // category locale defaults. On Java 6 or older environment,
                //    // ICU4J checks if the default locale has changed and update
                //    // category ULocales here if necessary.

                //    // Note: When java.util.Locale.setDefault(Locale) is called
                //    // with a Locale same with the previous one, Java 7 still
                //    // updates category locale defaults. On Java 6 or older env,
                //    // there is no good way to detect the event, ICU4J simply
                //    // check if the default Java Locale has changed since last
                //    // time.

                //    CultureInfo currentDefault = CultureInfo.CurrentCulture;
                //    if (!defaultLocale.Equals(currentDefault))
                //    {
                //        defaultLocale = currentDefault;
                //        defaultULocale = ForLocale(currentDefault);

                //        foreach (Category cat in Enum.GetValues(typeof(Category)))
                //        {
                //            int tmpIdx = (int)cat;
                //            defaultCategoryLocales[tmpIdx] = currentDefault;
                //            defaultCategoryULocales[tmpIdx] = ForLocale(currentDefault);
                //        }
                //    }

                //    // No synchronization with JDK Locale, because category default
                //    // is not supported in Java 6 or older versions
                //}
                return defaultCategoryULocales[idx];
            }
        }

        /**
         * Sets the default <code>ULocale</code> for the specified <code>Category</code>.
         * This also sets the default <code>Locale</code> for the specified <code>Category</code>
         * of the JVM. If the caller does not have write permission to the
         * user.language property, a security exception will be thrown,
         * and the default ULocale for the specified Category will remain unchanged.
         *
         * @param category the specified category to set the default locale
         * @param newLocale the new default locale
         * @see SecurityManager#checkPermission(java.security.Permission)
         * @see java.util.PropertyPermission
         * @stable ICU 49
         */
        public static void SetDefault(Category category, ULocale newLocale)
        {
            lock (syncLock)
            {
                CultureInfo newJavaDefault = newLocale.ToLocale();
                int idx = (int)category;
                defaultCategoryULocales[idx] = newLocale;
                defaultCategoryLocales[idx] = newJavaDefault;
                DotNetLocaleHelper.SetDefault(category, newJavaDefault);
            }
        }

        /**
         * This is for compatibility with Locale-- in actuality, since ULocale is
         * immutable, there is no reason to clone it, so this API returns 'this'.
         * @stable ICU 3.0
         */
        public object Clone()
        {
            return this;
        }

        /**
         * Returns the hashCode.
         * @stable ICU 3.0
         */
        public override int GetHashCode()
        {
            return localeID.GetHashCode();
        }

        /**
         * Returns true if the other object is another ULocale with the
         * same full name.
         * Note that since names are not canonicalized, two ULocales that
         * function identically might not compare equal.
         *
         * @return true if this Locale is equal to the specified object.
         * @stable ICU 3.0
         */
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is ULocale)
            {
                return localeID.Equals(((ULocale)obj).localeID);
            }
            return false;
        }

        /**
         * Compares two ULocale for ordering.
         * <para/><b>Note:</b> The order might change in future.
         *
         * @param other the ULocale to be compared.
         * @return a negative integer, zero, or a positive integer as this ULocale is less than, equal to, or greater
         * than the specified ULocale.
         * @throws NullPointerException if <code>other</code> is null.
         *
         * @stable ICU 53
         */
        public int CompareTo(ULocale other)
        {
            if (this == other)
            {
                return 0;
            }

            int cmp = 0;

            // Language
            cmp = GetLanguage().CompareToOrdinal(other.GetLanguage());
            if (cmp == 0)
            {
                // Script
                cmp = GetScript().CompareToOrdinal(other.GetScript());
                if (cmp == 0)
                {
                    // Region
                    cmp = GetCountry().CompareToOrdinal(other.GetCountry());
                    if (cmp == 0)
                    {
                        // Variant
                        cmp = GetVariant().CompareToOrdinal(other.GetVariant());
                        if (cmp == 0)
                        {
                            // Keywords
                            using (var thisKwdItr = GetKeywords())
                            using (var otherKwdItr = other.GetKeywords())
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
                                        string thisKey = thisKwdItr.Current;
                                        string otherKey = otherKwdItr.Current;
                                        cmp = thisKey.CompareToOrdinal(otherKey);
                                        if (cmp == 0)
                                        {
                                            // Compare keyword values
                                            string thisVal = GetKeywordValue(thisKey);
                                            string otherVal = other.GetKeywordValue(otherKey);
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

        /**
         * {@icunote} Unlike the Locale API, this returns an array of <code>ULocale</code>,
         * not <code>Locale</code>.  Returns a list of all installed locales.
         * @stable ICU 3.0
         */
        public static ULocale[] GetAvailableLocales()
        {
            return ICUResourceBundle.GetAvailableULocales();
        }

        /**
         * Returns a list of all 2-letter country codes defined in ISO 3166.
         * Can be used to create Locales.
         * @stable ICU 3.0
         */
        public static string[] GetISOCountries()
        {
            return LocaleIDs.GetISOCountries();
        }

        /**
         * Returns a list of all 2-letter language codes defined in ISO 639.
         * Can be used to create Locales.
         * [NOTE:  ISO 639 is not a stable standard-- some languages' codes have changed.
         * The list this function returns includes both the new and the old codes for the
         * languages whose codes have changed.]
         * @stable ICU 3.0
         */
        public static string[] GetISOLanguages()
        {
            return LocaleIDs.GetISOLanguages();
        }

        /**
         * Returns the language code for this locale, which will either be the empty string
         * or a lowercase ISO 639 code.
         * @see #getDisplayLanguage()
         * @see #getDisplayLanguage(ULocale)
         * @stable ICU 3.0
         */
        public string GetLanguage()
        {
            return Base().GetLanguage();
        }

        /**
         * Returns the language code for the locale ID,
         * which will either be the empty string
         * or a lowercase ISO 639 code.
         * @see #getDisplayLanguage()
         * @see #getDisplayLanguage(ULocale)
         * @stable ICU 3.0
         */
        public static string GetLanguage(string localeID)
        {
            return new LocaleIDParser(localeID).GetLanguage();
        }

        /**
         * Returns the script code for this locale, which might be the empty string.
         * @see #getDisplayScript()
         * @see #getDisplayScript(ULocale)
         * @stable ICU 3.0
         */
        public string GetScript()
        {
            return Base().GetScript();
        }

        /**
         * {@icu} Returns the script code for the specified locale, which might be the empty
         * string.
         * @see #getDisplayScript()
         * @see #getDisplayScript(ULocale)
         * @stable ICU 3.0
         */
        public static string GetScript(string localeID)
        {
            return new LocaleIDParser(localeID).GetScript();
        }

        /**
         * Returns the country/region code for this locale, which will either be the empty string
         * or an uppercase ISO 3166 2-letter code.
         * @see #getDisplayCountry()
         * @see #getDisplayCountry(ULocale)
         * @stable ICU 3.0
         */
        public string GetCountry()
        {
            return Base().GetRegion();
        }

        /**
         * {@icu} Returns the country/region code for this locale, which will either be the empty string
         * or an uppercase ISO 3166 2-letter code.
         * @param localeID The locale identification string.
         * @see #getDisplayCountry()
         * @see #getDisplayCountry(ULocale)
         * @stable ICU 3.0
         */
        public static string GetCountry(string localeID)
        {
            return new LocaleIDParser(localeID).GetCountry();
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
        public static string GetRegionForSupplementalData(
                                    ULocale locale, bool inferRegion)
        {
            string region = locale.GetKeywordValue("rg");
            if (region != null && region.Length == 6)
            {
                string regionUpper = AsciiUtil.ToUpper(region);
                if (regionUpper.EndsWith("ZZZZ", StringComparison.Ordinal))
                {
                    return regionUpper.Substring(0, 2 - 0); // ICU4N: Checked 2nd parameter
                }
            }
            region = locale.GetCountry();
            if (region.Length == 0 && inferRegion)
            {
                ULocale maximized = AddLikelySubtags(locale);
                region = maximized.GetCountry();
            }
            return region;
        }

        /**
         * Returns the variant code for this locale, which might be the empty string.
         * @see #getDisplayVariant()
         * @see #getDisplayVariant(ULocale)
         * @stable ICU 3.0
         */
        public string GetVariant()
        {
            return Base().GetVariant();
        }

        /**
         * {@icu} Returns the variant code for the specified locale, which might be the empty string.
         * @see #getDisplayVariant()
         * @see #getDisplayVariant(ULocale)
         * @stable ICU 3.0
         */
        public static string GetVariant(string localeID)
        {
            return new LocaleIDParser(localeID).GetVariant();
        }

        /**
         * {@icu} Returns the fallback locale for the specified locale, which might be the
         * empty string.
         * @stable ICU 3.2
         */
        public static string GetFallback(string localeID)
        {
            return GetFallbackString(GetName(localeID));
        }

        /**
         * {@icu} Returns the fallback locale for this locale.  If this locale is root,
         * returns null.
         * @stable ICU 3.2
         */
        public ULocale GetFallback()
        {
            if (localeID.Length == 0 || localeID[0] == '@')
            {
                return null;
            }
            return new ULocale(GetFallbackString(localeID), (CultureInfo)null);
        }

        /**
         * Returns the given (canonical) locale id minus the last part before the tags.
         */
        private static string GetFallbackString(string fallback)
        {
            int extStart = fallback.IndexOf('@');
            if (extStart == -1)
            {
                extStart = fallback.Length;
            }
            int last = fallback.LastIndexOf('_', Math.Min(extStart, fallback.Length - 1)); // ICU4N: Corrected 2nd parameter so it cannot be the same or greater than fallback.Length
            if (last == -1)
            {
                last = 0;
            }
            else
            {
                // truncate empty segment
                while (last > 0)
                {
                    if (fallback[last - 1] != '_')
                    {
                        break;
                    }
                    last--;
                }
            }
            return fallback.Substring(0, last - 0) + fallback.Substring(extStart); // ICU4N: Checked 2nd parameter
        }

        /**
         * {@icu} Returns the (normalized) base name for this locale,
         * like {@link #getName()}, but without keywords.
         *
         * @return the base name as a string.
         * @stable ICU 3.0
         */
        public string GetBaseName()
        {
            return GetBaseName(localeID);
        }

        /**
         * {@icu} Returns the (normalized) base name for the specified locale,
         * like {@link #getName(string)}, but without keywords.
         *
         * @param localeID the locale ID as a string
         * @return the base name as a string.
         * @stable ICU 3.0
         */
        public static string GetBaseName(string localeID)
        {
            if (localeID.IndexOf('@') == -1)
            {
                return localeID;
            }
            return new LocaleIDParser(localeID).GetBaseName();
        }

        /**
         * {@icu} Returns the (normalized) full name for this locale.
         *
         * @return string the full name of the localeID
         * @stable ICU 3.0
         */
        public string GetName() // ICU4N TODO: API - make into a property
        {
            return localeID; // always normalized
        }

        /**
         * Gets the shortest length subtag's size.
         *
         * @param localeID
         * @return The size of the shortest length subtag
         **/
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

        /**
         * {@icu} Returns the (normalized) full name for the specified locale.
         *
         * @param localeID the localeID as a string
         * @return string the full name of the localeID
         * @stable ICU 3.0
         */
        public static string GetName(string localeID)
        {
            string tmpLocaleID;
            // Convert BCP47 id if necessary
            if (localeID != null && !localeID.Contains("@") && GetShortestSubtagLength(localeID) == 1)
            {
                tmpLocaleID = ForLanguageTag(localeID).GetName();
                if (tmpLocaleID.Length == 0)
                {
                    tmpLocaleID = localeID;
                }
            }
            else
            {
                tmpLocaleID = localeID;
            }
            return nameCache.GetInstance(tmpLocaleID, null /* unused */);
        }

        /**
         * Returns a string representation of this object.
         * @stable ICU 3.0
         */
        public override string ToString()
        {
            return localeID;
        }

        /**
         * {@icu} Returns an iterator over keywords for this locale.  If there
         * are no keywords, returns null.
         * @return iterator over keywords, or null if there are no keywords.
         * @stable ICU 3.0
         */
        public IEnumerator<string> GetKeywords()
        {
            return GetKeywords(localeID);
        }

        /**
         * {@icu} Returns an iterator over keywords for the specified locale.  If there
         * are no keywords, returns null.
         * @return an iterator over the keywords in the specified locale, or null
         * if there are no keywords.
         * @stable ICU 3.0
         */
        public static IEnumerator<string> GetKeywords(string localeID)
        {
            return new LocaleIDParser(localeID).GetKeywords();
        }

        /**
         * {@icu} Returns the value for a keyword in this locale. If the keyword is not
         * defined, returns null.
         * @param keywordName name of the keyword whose value is desired. Case insensitive.
         * @return the value of the keyword, or null.
         * @stable ICU 3.0
         */
        public string GetKeywordValue(string keywordName)
        {
            return GetKeywordValue(localeID, keywordName);
        }

        /**
         * {@icu} Returns the value for a keyword in the specified locale. If the keyword is
         * not defined, returns null.  The locale name does not need to be normalized.
         * @param keywordName name of the keyword whose value is desired. Case insensitive.
         * @return string the value of the keyword as a string
         * @stable ICU 3.0
         */
        public static string GetKeywordValue(string localeID, string keywordName)
        {
            return new LocaleIDParser(localeID).GetKeywordValue(keywordName);
        }

        /**
         * {@icu} Returns the canonical name for the specified locale ID.  This is used to
         * convert POSIX and other grandfathered IDs to standard ICU form.
         * @param localeID the locale id
         * @return the canonicalized id
         * @stable ICU 3.0
         */
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

        /**
         * {@icu} Given a keyword and a value, return a new locale with an updated
         * keyword and value.  If the keyword is null, this removes all keywords from the locale id.
         * Otherwise, if the value is null, this removes the value for this keyword from the
         * locale id.  Otherwise, this adds/replaces the value for this keyword in the locale id.
         * The keyword and value must not be empty.
         *
         * <para/>Related: {@link #getBaseName()} returns the locale ID string with all keywords removed.
         *
         * @param keyword the keyword to add/remove, or null to remove all keywords.
         * @param value the value to add/set, or null to remove this particular keyword.
         * @return the updated locale
         * @stable ICU 3.2
         */
        public ULocale SetKeywordValue(string keyword, string value)
        {
            return new ULocale(SetKeywordValue(localeID, keyword, value), (CultureInfo)null);
        }

        /**
         * Given a locale id, a keyword, and a value, return a new locale id with an updated
         * keyword and value.  If the keyword is null, this removes all keywords from the locale id.
         * Otherwise, if the value is null, this removes the value for this keyword from the
         * locale id.  Otherwise, this adds/replaces the value for this keyword in the locale id.
         * The keyword and value must not be empty.
         *
         * <para/>Related: {@link #getBaseName(string)} returns the locale ID string with all keywords removed.
         *
         * @param localeID the locale id to modify
         * @param keyword the keyword to add/remove, or null to remove all keywords.
         * @param value the value to add/set, or null to remove this particular keyword.
         * @return the updated locale id
         * @stable ICU 3.2
         */
        public static string SetKeywordValue(string localeID, string keyword, string value)
        {
            LocaleIDParser parser = new LocaleIDParser(localeID);
            parser.SetKeywordValue(keyword, value);
            return parser.GetName();
        }

        /*
         * Given a locale id, a keyword, and a value, return a new locale id with an updated
         * keyword and value, if the keyword does not already have a value.  The keyword and
         * value must not be null or empty.
         * @param localeID the locale id to modify
         * @param keyword the keyword to add, if not already present
         * @param value the value to add, if not already present
         * @return the updated locale id
         */
        /*    private static string defaultKeywordValue(string localeID, string keyword, string value) {
            LocaleIDParser parser = new LocaleIDParser(localeID);
            parser.defaultKeywordValue(keyword, value);
            return parser.getName();
        }*/

        /**
         * Returns a three-letter abbreviation for this locale's language.  If the locale
         * doesn't specify a language, returns the empty string.  Otherwise, returns
         * a lowercase ISO 639-2/T language code.
         * The ISO 639-2 language codes can be found on-line at
         *   <a href="ftp://dkuug.dk/i18n/iso-639-2.txt"><code>ftp://dkuug.dk/i18n/iso-639-2.txt</code></a>
         * @exception MissingResourceException Throws MissingResourceException if the
         * three-letter language abbreviation is not available for this locale.
         * @stable ICU 3.0
         */
        public string GetISO3Language()
        {
            return GetISO3Language(localeID);
        }

        /**
         * {@icu} Returns a three-letter abbreviation for this locale's language.  If the locale
         * doesn't specify a language, returns the empty string.  Otherwise, returns
         * a lowercase ISO 639-2/T language code.
         * The ISO 639-2 language codes can be found on-line at
         *   <a href="ftp://dkuug.dk/i18n/iso-639-2.txt"><code>ftp://dkuug.dk/i18n/iso-639-2.txt</code></a>
         * @exception MissingResourceException Throws MissingResourceException if the
         * three-letter language abbreviation is not available for this locale.
         * @stable ICU 3.0
         */
        public static string GetISO3Language(string localeID)
        {
            return LocaleIDs.GetISO3Language(GetLanguage(localeID));
        }

        /**
         * Returns a three-letter abbreviation for this locale's country/region.  If the locale
         * doesn't specify a country, returns the empty string.  Otherwise, returns
         * an uppercase ISO 3166 3-letter country code.
         * @exception MissingResourceException Throws MissingResourceException if the
         * three-letter country abbreviation is not available for this locale.
         * @stable ICU 3.0
         */
        public string GetISO3Country()
        {
            return GetISO3Country(localeID);
        }

        /**
         * {@icu} Returns a three-letter abbreviation for this locale's country/region.  If the locale
         * doesn't specify a country, returns the empty string.  Otherwise, returns
         * an uppercase ISO 3166 3-letter country code.
         * @exception MissingResourceException Throws MissingResourceException if the
         * three-letter country abbreviation is not available for this locale.
         * @stable ICU 3.0
         */
        public static string GetISO3Country(string localeID)
        {
            return LocaleIDs.GetISO3Country(GetCountry(localeID));
        }

        /**
         * Pairs of (language subtag, + or -) for finding out fast if common languages
         * are LTR (minus) or RTL (plus).
         */
        private static readonly string LANG_DIR_STRING =
                    "root-en-es-pt-zh-ja-ko-de-fr-it-ar+he+fa+ru-nl-pl-th-tr-";

        /**
         * {@icu} Returns whether this locale's script is written right-to-left.
         * If there is no script subtag, then the likely script is used,
         * see {@link #addLikelySubtags(ULocale)}.
         * If no likely script is known, then false is returned.
         *
         * <para/>A script is right-to-left according to the CLDR script metadata
         * which corresponds to whether the script's letters have Bidi_Class=R or AL.
         *
         * <para/>Returns true for "ar" and "en-Hebr", false for "zh" and "fa-Cyrl".
         *
         * @return true if the locale's script is written right-to-left
         * @stable ICU 54
         */
        public bool IsRightToLeft()
        {
            string script = GetScript();
            if (script.Length == 0)
            {
                // Fastpath: We know the likely scripts and their writing direction
                // for some common languages.
                string lang = GetLanguage();
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
                ULocale likely = AddLikelySubtags(this);
                script = likely.GetScript();
                if (script.Length == 0)
                {
                    return false;
                }
            }
            int scriptCode = UScript.GetCodeFromName(script);
            return UScript.IsRightToLeft(scriptCode);
        }

        // display names

        /**
         * Returns this locale's language localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized language name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string GetDisplayLanguage()
        {
            return GetDisplayLanguageInternal(this, GetDefault(Category.DISPLAY), false);
        }

        /**
         * Returns this locale's language localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 3.0
         */
        public string GetDisplayLanguage(ULocale displayLocale)
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
            return GetDisplayLanguageInternal(new ULocale(localeID), new ULocale(displayLocaleID),
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
        public static string GetDisplayLanguage(string localeID, ULocale displayLocale)
        {
            return GetDisplayLanguageInternal(new ULocale(localeID), displayLocale, false);
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
            return GetDisplayLanguageInternal(this, GetDefault(Category.DISPLAY), true);
        }

        /**
         * {@icu} Returns this locale's language localized for display in the provided locale.
         * If a dialect name is present in the data, then it is returned.
         * @param displayLocale the locale in which to display the name.
         * @return the localized language name.
         * @stable ICU 4.4
         */
        public string GetDisplayLanguageWithDialect(ULocale displayLocale)
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
            return GetDisplayLanguageInternal(new ULocale(localeID), new ULocale(displayLocaleID),
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
        public static string GetDisplayLanguageWithDialect(string localeID, ULocale displayLocale)
        {
            return GetDisplayLanguageInternal(new ULocale(localeID), displayLocale, true);
        }

        private static string GetDisplayLanguageInternal(ULocale locale, ULocale displayLocale,
                bool useDialect)
        {
            string lang = useDialect ? locale.GetBaseName() : locale.GetLanguage();
            return LocaleDisplayNames.GetInstance(displayLocale).LanguageDisplayName(lang);
        }

        /**
         * Returns this locale's script localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized script name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string GetDisplayScript()
        {
            return GetDisplayScriptInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * {@icu} Returns this locale's script localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized script name.
         * @see Category#DISPLAY
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public string GetDisplayScriptInContext()
        {
            return GetDisplayScriptInContextInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * Returns this locale's script localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @stable ICU 3.0
         */
        public string GetDisplayScript(ULocale displayLocale)
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
        [Obsolete("This API is ICU internal only.")]
        public string GetDisplayScriptInContext(ULocale displayLocale)
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
            return GetDisplayScriptInternal(new ULocale(localeID), new ULocale(displayLocaleID));
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
        [Obsolete("This API is ICU internal only.")]
        public static string GetDisplayScriptInContext(string localeID, string displayLocaleID)
        {
            return GetDisplayScriptInContextInternal(new ULocale(localeID), new ULocale(displayLocaleID));
        }

        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * @param localeID the id of the locale whose script will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @stable ICU 3.0
         */
        public static string GetDisplayScript(string localeID, ULocale displayLocale)
        {
            return GetDisplayScriptInternal(new ULocale(localeID), displayLocale);
        }
        /**
         * {@icu} Returns a locale's script localized for display in the provided locale.
         * @param localeID the id of the locale whose script will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized script name.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static string GetDisplayScriptInContext(string localeID, ULocale displayLocale)
        {
            return GetDisplayScriptInContextInternal(new ULocale(localeID), displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayScriptInternal(ULocale locale, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale)
                    .ScriptDisplayName(locale.GetScript());
        }

        private static string GetDisplayScriptInContextInternal(ULocale locale, ULocale displayLocale)
        {
#pragma warning disable 612, 618
            return LocaleDisplayNames.GetInstance(displayLocale)
                    .ScriptDisplayNameInContext(locale.GetScript());
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
        public string GetDisplayCountry()
        {
            return GetDisplayCountryInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * Returns this locale's country localized for display in the provided locale.
         * <b>Warning: </b>this is for the region part of a valid locale ID; it cannot just be the region code (like "FR").
         * To get the display name for a region alone, or for other options, use {@link LocaleDisplayNames} instead.
         * @param displayLocale the locale in which to display the name.
         * @return the localized country name.
         * @stable ICU 3.0
         */
        public string GetDisplayCountry(ULocale displayLocale)
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
            return GetDisplayCountryInternal(new ULocale(localeID), new ULocale(displayLocaleID));
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
        public static string GetDisplayCountry(string localeID, ULocale displayLocale)
        {
            return GetDisplayCountryInternal(new ULocale(localeID), displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayCountryInternal(ULocale locale, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale)
                    .RegionDisplayName(locale.GetCountry());
        }

        /**
         * Returns this locale's variant localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized variant name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string GetDisplayVariant()
        {
            return GetDisplayVariantInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * Returns this locale's variant localized for display in the provided locale.
         * @param displayLocale the locale in which to display the name.
         * @return the localized variant name.
         * @stable ICU 3.0
         */
        public string GetDisplayVariant(ULocale displayLocale)
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
            return GetDisplayVariantInternal(new ULocale(localeID), new ULocale(displayLocaleID));
        }

        /**
         * {@icu} Returns a locale's variant localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose variant will be displayed.
         * @param displayLocale the locale in which to display the name.
         * @return the localized variant name.
         * @stable ICU 3.0
         */
        public static string GetDisplayVariant(string localeID, ULocale displayLocale)
        {
            return GetDisplayVariantInternal(new ULocale(localeID), displayLocale);
        }

        private static string GetDisplayVariantInternal(ULocale locale, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale)
                    .VariantDisplayName(locale.GetVariant());
        }

        /**
         * {@icu} Returns a keyword localized for display in the default <code>DISPLAY</code> locale.
         * @param keyword the keyword to be displayed.
         * @return the localized keyword name.
         * @see #getKeywords()
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public static string GetDisplayKeyword(string keyword)
        {
            return GetDisplayKeywordInternal(keyword, GetDefault(Category.DISPLAY));
        }

        /**
         * {@icu} Returns a keyword localized for display in the specified locale.
         * @param keyword the keyword to be displayed.
         * @param displayLocaleID the id of the locale in which to display the keyword.
         * @return the localized keyword name.
         * @see #getKeywords(string)
         * @stable ICU 3.0
         */
        public static string GetDisplayKeyword(string keyword, string displayLocaleID)
        {
            return GetDisplayKeywordInternal(keyword, new ULocale(displayLocaleID));
        }

        /**
         * {@icu} Returns a keyword localized for display in the specified locale.
         * @param keyword the keyword to be displayed.
         * @param displayLocale the locale in which to display the keyword.
         * @return the localized keyword name.
         * @see #getKeywords(string)
         * @stable ICU 3.0
         */
        public static string GetDisplayKeyword(string keyword, ULocale displayLocale)
        {
            return GetDisplayKeywordInternal(keyword, displayLocale);
        }

        private static string GetDisplayKeywordInternal(string keyword, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale).KeyDisplayName(keyword);
        }

        /**
         * {@icu} Returns a keyword value localized for display in the default <code>DISPLAY</code> locale.
         * @param keyword the keyword whose value is to be displayed.
         * @return the localized value name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string GetDisplayKeywordValue(string keyword)
        {
            return GetDisplayKeywordValueInternal(this, keyword, GetDefault(Category.DISPLAY));
        }

        /**
         * {@icu} Returns a keyword value localized for display in the specified locale.
         * @param keyword the keyword whose value is to be displayed.
         * @param displayLocale the locale in which to display the value.
         * @return the localized value name.
         * @stable ICU 3.0
         */
        public string GetDisplayKeywordValue(string keyword, ULocale displayLocale)
        {
            return GetDisplayKeywordValueInternal(this, keyword, displayLocale);
        }

        /**
         * {@icu} Returns a keyword value localized for display in the specified locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose keyword value is to be displayed.
         * @param keyword the keyword whose value is to be displayed.
         * @param displayLocaleID the id of the locale in which to display the value.
         * @return the localized value name.
         * @stable ICU 3.0
         */
        public static string GetDisplayKeywordValue(string localeID, string keyword,
                string displayLocaleID)
        {
            return GetDisplayKeywordValueInternal(new ULocale(localeID), keyword,
                    new ULocale(displayLocaleID));
        }

        /**
         * {@icu} Returns a keyword value localized for display in the specified locale.
         * This is a cover for the ICU4C API.
         * @param localeID the id of the locale whose keyword value is to be displayed.
         * @param keyword the keyword whose value is to be displayed.
         * @param displayLocale the id of the locale in which to display the value.
         * @return the localized value name.
         * @stable ICU 3.0
         */
        public static string GetDisplayKeywordValue(string localeID, string keyword,
                ULocale displayLocale)
        {
            return GetDisplayKeywordValueInternal(new ULocale(localeID), keyword, displayLocale);
        }

        // displayLocaleID is canonical, localeID need not be since parsing will fix this.
        private static string GetDisplayKeywordValueInternal(ULocale locale, string keyword,
                ULocale displayLocale)
        {
            keyword = AsciiUtil.ToLower(keyword.Trim());
            string value = locale.GetKeywordValue(keyword);
            return LocaleDisplayNames.GetInstance(displayLocale).KeyValueDisplayName(keyword, value);
        }

        /**
         * Returns this locale name localized for display in the default <code>DISPLAY</code> locale.
         * @return the localized locale name.
         * @see Category#DISPLAY
         * @stable ICU 3.0
         */
        public string GetDisplayName()
        {
            return GetDisplayNameInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * Returns this locale name localized for display in the provided locale.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 3.0
         */
        public string GetDisplayName(ULocale displayLocale)
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
            return GetDisplayNameInternal(new ULocale(localeID), new ULocale(displayLocaleID));
        }

        /**
         * {@icu} Returns the locale ID localized for display in the provided locale.
         * This is a cover for the ICU4C API.
         * @param localeID the locale whose name is to be displayed.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 3.0
         */
        public static string GetDisplayName(string localeID, ULocale displayLocale)
        {
            return GetDisplayNameInternal(new ULocale(localeID), displayLocale);
        }

        private static string GetDisplayNameInternal(ULocale locale, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale).LocaleDisplayName(locale);
        }

        /**
         * {@icu} Returns this locale name localized for display in the default <code>DISPLAY</code> locale.
         * If a dialect name is present in the locale data, then it is returned.
         * @return the localized locale name.
         * @see Category#DISPLAY
         * @stable ICU 4.4
         */
        public string GetDisplayNameWithDialect()
        {
            return GetDisplayNameWithDialectInternal(this, GetDefault(Category.DISPLAY));
        }

        /**
         * {@icu} Returns this locale name localized for display in the provided locale.
         * If a dialect name is present in the locale data, then it is returned.
         * @param displayLocale the locale in which to display the locale name.
         * @return the localized locale name.
         * @stable ICU 4.4
         */
        public string GetDisplayNameWithDialect(ULocale displayLocale)
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
            return GetDisplayNameWithDialectInternal(new ULocale(localeID),
                    new ULocale(displayLocaleID));
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
        public static string GetDisplayNameWithDialect(string localeID, ULocale displayLocale)
        {
            return GetDisplayNameWithDialectInternal(new ULocale(localeID), displayLocale);
        }

        private static string GetDisplayNameWithDialectInternal(ULocale locale, ULocale displayLocale)
        {
            return LocaleDisplayNames.GetInstance(displayLocale, DialectHandling.DialectNames)
                    .LocaleDisplayName(locale);
        }

        /**
         * {@icu} Returns this locale's layout orientation for characters.  The possible
         * values are "left-to-right", "right-to-left", "top-to-bottom" or
         * "bottom-to-top".
         * @return The locale's layout orientation for characters.
         * @stable ICU 4.0
         */
        public string GetCharacterOrientation()
        {
            return ICUResourceTableAccess.GetTableString(ICUData.IcuBaseName, this,
                    "layout", "characters", "characters");
        }

        /**
         * {@icu} Returns this locale's layout orientation for lines.  The possible
         * values are "left-to-right", "right-to-left", "top-to-bottom" or
         * "bottom-to-top".
         * @return The locale's layout orientation for lines.
         * @stable ICU 4.0
         */
        public string GetLineOrientation()
        {
            return ICUResourceTableAccess.GetTableString(ICUData.IcuBaseName, this,
                    "layout", "lines", "lines");
        }

        /**
         * {@icu} Selector for <tt>getLocale()</tt> indicating the locale of the
         * resource containing the data.  This is always at or above the
         * valid locale.  If the valid locale does not contain the
         * specific data being requested, then the actual locale will be
         * above the valid locale.  If the object was not constructed from
         * locale data, then the valid locale is <i>null</i>.
         *
         * @draft ICU 2.8 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public static Type ACTUAL_LOCALE = new Type();

        /**
         * {@icu} Selector for <tt>getLocale()</tt> indicating the most specific
         * locale for which any data exists.  This is always at or above
         * the requested locale, and at or below the actual locale.  If
         * the requested locale does not correspond to any resource data,
         * then the valid locale will be above the requested locale.  If
         * the object was not constructed from locale data, then the
         * actual locale is <i>null</i>.
         *
         * <para/>Note: The valid locale will be returned correctly in ICU
         * 3.0 or later.  In ICU 2.8, it is not returned correctly.
         * @draft ICU 2.8 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public static Type VALID_LOCALE = new Type();

        /**
         * Opaque selector enum for <tt>getLocale()</tt>.
         * @see com.ibm.icu.util.ULocale
         * @see com.ibm.icu.util.ULocale#ACTUAL_LOCALE
         * @see com.ibm.icu.util.ULocale#VALID_LOCALE
         * @draft ICU 2.8 (retainAll)
         * @provisional This API might change or be removed in a future release.
         */
        public sealed class Type
        {
            internal Type() { }
        }

        /**
         * {@icu} Based on a HTTP formatted list of acceptable locales, determine an available
         * locale for the user.  NullPointerException is thrown if acceptLanguageList or
         * availableLocales is null.  If fallback is non-null, it will contain true if a
         * fallback locale (one not in the acceptLanguageList) was returned.  The value on
         * entry is ignored.  ULocale will be one of the locales in availableLocales, or the
         * ROOT ULocale if if a ROOT locale was used as a fallback (because nothing else in
         * availableLocales matched).  No ULocale array element should be null; behavior is
         * undefined if this is the case.
         * @param acceptLanguageList list in HTTP "Accept-Language:" format of acceptable locales
         * @param availableLocales list of available locales. One of these will be returned.
         * @param fallback if non-null, a 1-element array containing a bool to be set with
         * the fallback status
         * @return one of the locales from the availableLocales list, or null if none match
         * @stable ICU 3.4
         */
        public static ULocale AcceptLanguage(string acceptLanguageList, ULocale[] availableLocales,
                bool[] fallback)
        {
            if (acceptLanguageList == null)
            {
                throw new ArgumentNullException(nameof(acceptLanguageList));
            }
            ULocale[] acceptList = null;
            try
            {
                acceptList = ParseAcceptLanguage(acceptLanguageList, true); // ICU4N TODO: TryParseAcceptLanguage
            }
            catch (FormatException)
            {
                acceptList = null;
            }
            if (acceptList == null)
            {
                return null;
            }
            return AcceptLanguage(acceptList, availableLocales, fallback);
        }

        /**
         * {@icu} Based on a list of acceptable locales, determine an available locale for the
         * user.  NullPointerException is thrown if acceptLanguageList or availableLocales is
         * null.  If fallback is non-null, it will contain true if a fallback locale (one not
         * in the acceptLanguageList) was returned.  The value on entry is ignored.  ULocale
         * will be one of the locales in availableLocales, or the ROOT ULocale if if a ROOT
         * locale was used as a fallback (because nothing else in availableLocales matched).
         * No ULocale array element should be null; behavior is undefined if this is the case.
         * @param acceptLanguageList list of acceptable locales
         * @param availableLocales list of available locales. One of these will be returned.
         * @param fallback if non-null, a 1-element array containing a bool to be set with
         * the fallback status
         * @return one of the locales from the availableLocales list, or null if none match
         * @stable ICU 3.4
         */

        public static ULocale AcceptLanguage(ULocale[] acceptLanguageList, ULocale[]
                availableLocales, bool[] fallback)
        {
            // fallbacklist
            int i, j;
            if (fallback != null)
            {
                fallback[0] = true;
            }
            for (i = 0; i < acceptLanguageList.Length; i++)
            {
                ULocale aLocale = acceptLanguageList[i];
                bool[] setFallback = fallback;
                do
                {
                    for (j = 0; j < availableLocales.Length; j++)
                    {
                        if (availableLocales[j].Equals(aLocale))
                        {
                            if (setFallback != null)
                            {
                                setFallback[0] = false; // first time with this locale - not a fallback.
                            }
                            return availableLocales[j];
                        }
                        // compare to scriptless alias, so locales such as
                        // zh_TW, zh_CN are considered as available locales - see #7190
                        if (aLocale.GetScript().Length == 0
                                && availableLocales[j].GetScript().Length > 0
                                && availableLocales[j].GetLanguage().Equals(aLocale.GetLanguage())
                                && availableLocales[j].GetCountry().Equals(aLocale.GetCountry())
                                && availableLocales[j].GetVariant().Equals(aLocale.GetVariant()))
                        {
                            ULocale minAvail = ULocale.MinimizeSubtags(availableLocales[j]);
                            if (minAvail.GetScript().Length == 0)
                            {
                                if (setFallback != null)
                                {
                                    setFallback[0] = false; // not a fallback.
                                }
                                return aLocale;
                            }
                        }
                    }
                    CultureInfo loc = aLocale.ToLocale();
                    CultureInfo parent = null;
                    if (loc != null)
                        parent = LocaleUtility.Fallback(loc.DisplayName);
                    else
                        parent = LocaleUtility.Fallback(aLocale.GetName());


                    //                    try
                    //                    {
                    //                        CultureInfo loc = aLocale.ToLocale();
                    //#if NETSTANDARD1_3
                    //                        // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
                    //                        // to be created, but will be "unknown" languages. We need to manually
                    //                        // ignore these.
                    //                        if (!loc.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
                    //                        {
                    //#endif
                    //                            parent = LocaleUtility.Fallback(loc);
                    //#if NETSTANDARD1_3
                    //                        }
                    //#endif
                    //                    }
                    //                    // ICU4N: In .NET Framework and .NET Standard 2.x+, unknown cultures throw a 
                    //                    // CultureNotFoundException.
                    //                    catch (CultureNotFoundException)
                    //                    {
                    //                        parent = LocaleUtility.Fallback(aLocale);
                    //                    }

                    if (parent != null)
                    {
                        aLocale = new ULocale(parent);
                    }
                    else
                    {
                        aLocale = null;
                    }

                    setFallback = null; // Do not set fallback in later iterations
                } while (aLocale != null);
            }
            return null;
        }

        /**
         * {@icu} Based on a HTTP formatted list of acceptable locales, determine an available
         * locale for the user.  NullPointerException is thrown if acceptLanguageList or
         * availableLocales is null.  If fallback is non-null, it will contain true if a
         * fallback locale (one not in the acceptLanguageList) was returned.  The value on
         * entry is ignored.  ULocale will be one of the locales in availableLocales, or the
         * ROOT ULocale if if a ROOT locale was used as a fallback (because nothing else in
         * availableLocales matched).  No ULocale array element should be null; behavior is
         * undefined if this is the case.  This function will choose a locale from the
         * ULocale.getAvailableLocales() list as available.
         * @param acceptLanguageList list in HTTP "Accept-Language:" format of acceptable locales
         * @param fallback if non-null, a 1-element array containing a bool to be set with
         * the fallback status
         * @return one of the locales from the ULocale.getAvailableLocales() list, or null if
         * none match
         * @stable ICU 3.4
         */
        public static ULocale AcceptLanguage(string acceptLanguageList, bool[] fallback)
        {
            return AcceptLanguage(acceptLanguageList, ULocale.GetAvailableLocales(),
                    fallback);
        }

        /**
         * {@icu} Based on an ordered array of acceptable locales, determine an available
         * locale for the user.  NullPointerException is thrown if acceptLanguageList or
         * availableLocales is null.  If fallback is non-null, it will contain true if a
         * fallback locale (one not in the acceptLanguageList) was returned.  The value on
         * entry is ignored.  ULocale will be one of the locales in availableLocales, or the
         * ROOT ULocale if if a ROOT locale was used as a fallback (because nothing else in
         * availableLocales matched).  No ULocale array element should be null; behavior is
         * undefined if this is the case.  This function will choose a locale from the
         * ULocale.getAvailableLocales() list as available.
         * @param acceptLanguageList ordered array of acceptable locales (preferred are listed first)
         * @param fallback if non-null, a 1-element array containing a bool to be set with
         * the fallback status
         * @return one of the locales from the ULocale.getAvailableLocales() list, or null if none match
         * @stable ICU 3.4
         */
        public static ULocale AcceptLanguage(ULocale[] acceptLanguageList, bool[] fallback)
        {
            return AcceptLanguage(acceptLanguageList, ULocale.GetAvailableLocales(),
                    fallback);
        }

        private class ULocaleAcceptLanguageQ : IComparable<ULocaleAcceptLanguageQ>
        {
            private double q;
            private double serial;

            public ULocaleAcceptLanguageQ(double theq, int theserial)
            {
                q = theq;
                serial = theserial;
            }

            public int CompareTo(ULocaleAcceptLanguageQ other)
            {
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

        /**
         * Package local method used for parsing Accept-Language string
         */
        internal static ULocale[] ParseAcceptLanguage(string acceptLanguage, bool isLenient)
        {


            // parse out the acceptLanguage into an array
            SortedDictionary<ULocaleAcceptLanguageQ, ULocale> map =
                    new SortedDictionary<ULocaleAcceptLanguageQ, ULocale>();
            StringBuilder languageRangeBuf = new StringBuilder();
            StringBuilder qvalBuf = new StringBuilder();
            int state = 0;
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
                    // error state
                    throw new FormatException("Invalid Accept-Language" /*, n*/); // ICU4N TODO: Make a Try... version of this
                }
                if (gotLanguageQ)
                {
                    double q = 1.0;
                    if (qvalBuf.Length != 0)
                    {
                        if (!double.TryParse(qvalBuf.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out q))
                        {
                            // Already validated, so it should never happen
                            q = 1.0;
                        }
                        //try // ICU4N TODO: Use TryParse
                        //{
                        //    q = Double.Parse(qvalBuf.ToString(), CultureInfo.InvariantCulture);
                        //}
                        //catch (FormatException nfe)
                        //{
                        //    // Already validated, so it should never happen
                        //    q = 1.0;
                        //}
                        if (q > 1.0)
                        {
                            q = 1.0;
                        }
                    }
                    if (languageRangeBuf[0] != '*')
                    {
                        int serial = map.Count;
                        ULocaleAcceptLanguageQ entry = new ULocaleAcceptLanguageQ(q, serial);
                        // sort in reverse order..   1.0, 0.9, 0.8 .. etc
                        map[entry] = new ULocale(Canonicalize(languageRangeBuf.ToString()));
                    }

                    // reset buffer and parse state
                    languageRangeBuf.Length = 0;
                    qvalBuf.Length = 0;
                    state = 0;
                }
            }
            if (state != 0)
            {
                // Well, the parser should handle all cases.  So just in case.
                throw new FormatException("Invalid AcceptlLanguage" /*, n*/);
            }

            // pull out the map
            ULocale[] acceptList = map.Values.ToArray();
            return acceptList;
        }

        private static readonly string UNDEFINED_LANGUAGE = "und";
        private static readonly string UNDEFINED_SCRIPT = "Zzzz";
        private static readonly string UNDEFINED_REGION = "ZZ";

        /**
         * {@icu} Adds the likely subtags for a provided locale ID, per the algorithm
         * described in the following CLDR technical report:
         *
         *   http://www.unicode.org/reports/tr35/#Likely_Subtags
         *
         * If the provided ULocale instance is already in the maximal form, or there is no
         * data available available for maximization, it will be returned.  For example,
         * "und-Zzzz" cannot be maximized, since there is no reasonable maximization.
         * Otherwise, a new ULocale instance with the maximal form is returned.
         *
         * Examples:
         *
         * "en" maximizes to "en_Latn_US"
         *
         * "de" maximizes to "de_Latn_US"
         *
         * "sr" maximizes to "sr_Cyrl_RS"
         *
         * "sh" maximizes to "sr_Latn_RS" (Note this will not reverse.)
         *
         * "zh_Hani" maximizes to "zh_Hans_CN" (Note this will not reverse.)
         *
         * @param loc The ULocale to maximize
         * @return The maximized ULocale instance.
         * @stable ICU 4.0
         */
        public static ULocale AddLikelySubtags(ULocale loc)
        {
            string[] tags = new string[3];
            string trailing = null;

            int trailingIndex = ParseTagString(
                    loc.localeID,
                    tags);

            if (trailingIndex < loc.localeID.Length)
            {
                trailing = loc.localeID.Substring(trailingIndex);
            }

            string newLocaleID =
                    CreateLikelySubtagsString(
                            tags[0],
                            tags[1],
                            tags[2],
                            trailing);

            return newLocaleID == null ? loc : new ULocale(newLocaleID);
        }

        /**
         * {@icu} Minimizes the subtags for a provided locale ID, per the algorithm described
         * in the following CLDR technical report:<blockquote>
         *
         *   <a href="http://www.unicode.org/reports/tr35/#Likely_Subtags"
         *>http://www.unicode.org/reports/tr35/#Likely_Subtags</a></blockquote>
         *
         * If the provided ULocale instance is already in the minimal form, or there
         * is no data available for minimization, it will be returned.  Since the
         * minimization algorithm relies on proper maximization, see the comments
         * for addLikelySubtags for reasons why there might not be any data.
         *
         * Examples:<pre>
         *
         * "en_Latn_US" minimizes to "en"
         *
         * "de_Latn_US" minimizes to "de"
         *
         * "sr_Cyrl_RS" minimizes to "sr"
         *
         * "zh_Hant_TW" minimizes to "zh_TW" (The region is preferred to the
         * script, and minimizing to "zh" would imply "zh_Hans_CN".) </pre>
         *
         * @param loc The ULocale to minimize
         * @return The minimized ULocale instance.
         * @stable ICU 4.0
         */
        public static ULocale MinimizeSubtags(ULocale loc)
        {
#pragma warning disable 612, 618
            return MinimizeSubtags(loc, Minimize.FAVOR_REGION);
#pragma warning restore 612, 618
        }

        /**
         * Options for minimizeSubtags.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public enum Minimize
        {
            /**
             * Favor including the script, when either the region <b>or</b> the script could be suppressed, but not both.
             * @internal
             * @deprecated This API is ICU internal only.
             */
            [Obsolete("This API is ICU internal only.")]
            FAVOR_SCRIPT,
            /**
             * Favor including the region, when either the region <b>or</b> the script could be suppressed, but not both.
             * @internal
             * @deprecated This API is ICU internal only.
             */
            [Obsolete("This API is ICU internal only.")]
            FAVOR_REGION
        }

        /**
         * {@icu} Minimizes the subtags for a provided locale ID, per the algorithm described
         * in the following CLDR technical report:<blockquote>
         *
         *   <a href="http://www.unicode.org/reports/tr35/#Likely_Subtags"
         *>http://www.unicode.org/reports/tr35/#Likely_Subtags</a></blockquote>
         *
         * If the provided ULocale instance is already in the minimal form, or there
         * is no data available for minimization, it will be returned.  Since the
         * minimization algorithm relies on proper maximization, see the comments
         * for addLikelySubtags for reasons why there might not be any data.
         *
         * Examples:<pre>
         *
         * "en_Latn_US" minimizes to "en"
         *
         * "de_Latn_US" minimizes to "de"
         *
         * "sr_Cyrl_RS" minimizes to "sr"
         *
         * "zh_Hant_TW" minimizes to "zh_TW" if fieldToFavor == {@link Minimize#FAVOR_REGION}
         * "zh_Hant_TW" minimizes to "zh_Hant" if fieldToFavor == {@link Minimize#FAVOR_SCRIPT}
         * </pre>
         * The fieldToFavor only has an effect if either the region or the script could be suppressed, but not both.
         * @param loc The ULocale to minimize
         * @param fieldToFavor Indicate which should be preferred, when either the region <b>or</b> the script could be suppressed, but not both.
         * @return The minimized ULocale instance.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static ULocale MinimizeSubtags(ULocale loc, Minimize fieldToFavor)
        {
            string[] tags = new string[3];

            int trailingIndex = ParseTagString(
                    loc.localeID,
                    tags);

            string originalLang = tags[0];
            string originalScript = tags[1];
            string originalRegion = tags[2];
            string originalTrailing = null;

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
            string maximizedLocaleID =
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
                string tag =
                        CreateLikelySubtagsString(
                                originalLang,
                                null,
                                null,
                                null);

                if (tag.Equals(maximizedLocaleID))
                {
                    string newLocaleID =
                            CreateTagString(
                                    originalLang,
                                    null,
                                    null,
                                    originalTrailing);

                    return new ULocale(newLocaleID);
                }
            }

            /*
             * Next, try the language and region.
             **/
            if (fieldToFavor == Minimize.FAVOR_REGION)
            {
                if (originalRegion.Length != 0)
                {
                    string tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    null,
                                    originalRegion,
                                    null);

                    if (tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        null,
                                        originalRegion,
                                        originalTrailing);

                        return new ULocale(newLocaleID);
                    }
                }
                if (originalScript.Length != 0)
                {
                    string tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    originalScript,
                                    null,
                                    null);

                    if (tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        originalScript,
                                        null,
                                        originalTrailing);

                        return new ULocale(newLocaleID);
                    }
                }
            }
            else
            { // FAVOR_SCRIPT, so
                if (originalScript.Length != 0)
                {
                    string tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    originalScript,
                                    null,
                                    null);

                    if (tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        originalScript,
                                        null,
                                        originalTrailing);

                        return new ULocale(newLocaleID);
                    }
                }
                if (originalRegion.Length != 0)
                {
                    string tag =
                            CreateLikelySubtagsString(
                                    originalLang,
                                    null,
                                    originalRegion,
                                    null);

                    if (tag.Equals(maximizedLocaleID))
                    {
                        string newLocaleID =
                                CreateTagString(
                                        originalLang,
                                        null,
                                        originalRegion,
                                        originalTrailing);

                        return new ULocale(newLocaleID);
                    }
                }
            }
            return loc;
        }

        /////**
        //// * A trivial utility function that checks for a null
        //// * reference or checks the length of the supplied string.
        //// *
        //// *   @param string The string to check
        //// *
        //// *   @return true if the string is empty, or if the reference is null.
        //// */
        ////private static bool IsEmptyString(string str)
        ////{
        ////    return str == null || str.Length == 0;
        ////}

        /**
         * Append a tag to a StringBuilder, adding the separator if necessary.The tag must
         * not be a zero-length string.
         *
         * @param tag The tag to add.
         * @param buffer The output buffer.
         **/
        private static void AppendTag(string tag, StringBuilder buffer)
        {
            if (buffer.Length != 0)
            {
                buffer.Append(UNDERSCORE);
            }

            buffer.Append(tag);
        }

        /**
         * Create a tag string from the supplied parameters.  The lang, script and region
         * parameters may be null references.
         *
         * If any of the language, script or region parameters are empty, and the alternateTags
         * parameter is not null, it will be parsed for potential language, script and region tags
         * to be used when constructing the new tag.  If the alternateTags parameter is null, or
         * it contains no language tag, the default tag for the unknown language is used.
         *
         * @param lang The language tag to use.
         * @param script The script tag to use.
         * @param region The region tag to use.
         * @param trailing Any trailing data to append to the new tag.
         * @param alternateTags A string containing any alternate tags.
         * @return The new tag string.
         **/
        private static string CreateTagString(string lang, string script, string region,
                string trailing, string alternateTags)
        {

            LocaleIDParser parser = null;
            bool regionAppended = false;

            StringBuilder tag = new StringBuilder();

            if (!string.IsNullOrEmpty(lang))
            {
                AppendTag(
                        lang,
                        tag);
            }
            else if (string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Append the value for an unknown language, if
                 * we found no language.
                 */
                AppendTag(
                        UNDEFINED_LANGUAGE,
                        tag);
            }
            else
            {
                parser = new LocaleIDParser(alternateTags);

                string alternateLang = parser.GetLanguage();

                /*
                 * Append the value for an unknown language, if
                 * we found no language.
                 */
                AppendTag(
                        !string.IsNullOrEmpty(alternateLang) ? alternateLang : UNDEFINED_LANGUAGE,
                                tag);
            }

            if (!string.IsNullOrEmpty(script))
            {
                AppendTag(
                        script,
                        tag);
            }
            else if (!string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Parse the alternateTags string for the script.
                 */
                if (parser == null)
                {
                    parser = new LocaleIDParser(alternateTags);
                }

                string alternateScript = parser.GetScript();

                if (!string.IsNullOrEmpty(alternateScript))
                {
                    AppendTag(
                            alternateScript,
                            tag);
                }
            }

            if (!string.IsNullOrEmpty(region))
            {
                AppendTag(
                        region,
                        tag);

                regionAppended = true;
            }
            else if (!string.IsNullOrEmpty(alternateTags))
            {
                /*
                 * Parse the alternateTags string for the region.
                 */
                if (parser == null)
                {
                    parser = new LocaleIDParser(alternateTags);
                }

                string alternateRegion = parser.GetCountry();

                if (!string.IsNullOrEmpty(alternateRegion))
                {
                    AppendTag(
                            alternateRegion,
                            tag);

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

                if (trailing[0] == UNDERSCORE)
                {
                    if (trailing[1] == UNDERSCORE)
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
                        tag.Append(trailing.Substring(1));
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
                        tag.Append(UNDERSCORE);
                    }
                    tag.Append(trailing);
                }
            }

            return tag.ToString();
        }

        /**
         * Create a tag string from the supplied parameters.  The lang, script and region
         * parameters may be null references.If the lang parameter is an empty string, the
         * default value for an unknown language is written to the output buffer.
         *
         * @param lang The language tag to use.
         * @param script The script tag to use.
         * @param region The region tag to use.
         * @param trailing Any trailing data to append to the new tag.
         * @return The new string.
         **/
        internal static string CreateTagString(string lang, string script, string region, string trailing)
        {
            return CreateTagString(lang, script, region, trailing, null);
        }

        /**
         * Parse the language, script, and region subtags from a tag string, and return the results.
         *
         * This function does not return the canonical strings for the unknown script and region.
         *
         * @param localeID The locale ID to parse.
         * @param tags An array of three string references to return the subtag strings.
         * @return The number of chars of the localeID parameter consumed.
         **/
        private static int ParseTagString(string localeID, string[] tags)
        {
            LocaleIDParser parser = new LocaleIDParser(localeID);

            string lang = parser.GetLanguage();
            string script = parser.GetScript();
            string region = parser.GetCountry();

            if (string.IsNullOrEmpty(lang))
            {
                tags[0] = UNDEFINED_LANGUAGE;
            }
            else
            {
                tags[0] = lang;
            }

            if (script.Equals(UNDEFINED_SCRIPT))
            {
                tags[1] = "";
            }
            else
            {
                tags[1] = script;
            }

            if (region.Equals(UNDEFINED_REGION))
            {
                tags[2] = "";
            }
            else
            {
                tags[2] = region;
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

        private static string LookupLikelySubtags(string localeId)
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

        private static string CreateLikelySubtagsString(string lang, string script, string region,
                string variants)
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

                string likelySubtags = LookupLikelySubtags(searchTag);

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

                string likelySubtags = LookupLikelySubtags(searchTag);
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

                string likelySubtags = LookupLikelySubtags(searchTag);

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

                string likelySubtags = LookupLikelySubtags(searchTag);

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

        /**
         * The key for the private use locale extension ('x').
         *
         * @see #getExtension(char)
         * @see Builder#setExtension(char, string)
         *
         * @stable ICU 4.2
         */
        public static readonly char PRIVATE_USE_EXTENSION = 'x';

        /**
         * The key for Unicode locale extension ('u').
         *
         * @see #getExtension(char)
         * @see Builder#setExtension(char, string)
         *
         * @stable ICU 4.2
         */
        public static readonly char UNICODE_LOCALE_EXTENSION = 'u';

        /**
         * Returns the extension (or private use) value associated with
         * the specified key, or null if there is no extension
         * associated with the key. To be well-formed, the key must be one
         * of <code>[0-9A-Za-z]</code>. Keys are case-insensitive, so
         * for example 'z' and 'Z' represent the same extension.
         *
         * @param key the extension key
         * @return The extension, or null if this locale defines no
         * extension for the specified key.
         * @throws IllegalArgumentException if key is not well-formed
         * @see #PRIVATE_USE_EXTENSION
         * @see #UNICODE_LOCALE_EXTENSION
         *
         * @stable ICU 4.2
         */
        public string GetExtension(char key)
        {
            if (!LocaleExtensions.IsValidKey(key))
            {
                throw new ArgumentException("Invalid extension key: " + key);
            }
            return Extensions().GetExtensionValue(key);
        }

        /**
         * Returns the set of extension keys associated with this locale, or the
         * empty set if it has no extensions. The returned set is unmodifiable.
         * The keys will all be lower-case.
         *
         * @return the set of extension keys, or the empty set if this locale has
         * no extensions
         * @stable ICU 4.2
         */
        public ICollection<char> GetExtensionKeys() // ICU4N TODO: API make property ?
        {
            return Extensions().Keys;
        }

        /**
         * Returns the set of unicode locale attributes associated with
         * this locale, or the empty set if it has no attributes. The
         * returned set is unmodifiable.
         *
         * @return The set of attributes.
         * @stable ICU 4.6
         */
        public ISet<string> GetUnicodeLocaleAttributes()
        {
            return Extensions().GetUnicodeLocaleAttributes();
        }

        /**
         * Returns the Unicode locale type associated with the specified Unicode locale key
         * for this locale. Returns the empty string for keys that are defined with no type.
         * Returns null if the key is not defined. Keys are case-insensitive. The key must
         * be two alphanumeric characters ([0-9a-zA-Z]), or an IllegalArgumentException is
         * thrown.
         *
         * @param key the Unicode locale key
         * @return The Unicode locale type associated with the key, or null if the
         * locale does not define the key.
         * @throws IllegalArgumentException if the key is not well-formed
         * @throws NullPointerException if <code>key</code> is null
         *
         * @stable ICU 4.4
         */
        public string GetUnicodeLocaleType(string key)
        {
            if (!LocaleExtensions.IsValidUnicodeLocaleKey(key))
            {
                throw new ArgumentException("Invalid Unicode locale key: " + key);
            }
            return Extensions().GetUnicodeLocaleType(key);
        }

        /**
         * Returns the set of Unicode locale keys defined by this locale, or the empty set if
         * this locale has none.  The returned set is immutable.  Keys are all lower case.
         *
         * @return The set of Unicode locale keys, or the empty set if this locale has
         * no Unicode locale keywords.
         *
         * @stable ICU 4.4
         */
        public ICollection<string> GetUnicodeLocaleKeys()
        {
            return Extensions().GetUnicodeLocaleKeys();
        }

        /**
         * Returns a well-formed IETF BCP 47 language tag representing
         * this locale.
         *
         * <para/>If this <code>ULocale</code> has a language, script, country, or
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
        public string ToLanguageTag()
        {
            BaseLocale @base = Base();
            LocaleExtensions exts = Extensions();

            if (@base.GetVariant().Equals("POSIX", StringComparison.OrdinalIgnoreCase))
            {
                // special handling for variant POSIX
                @base = BaseLocale.GetInstance(@base.GetLanguage(), @base.GetScript(), @base.GetRegion(), "");
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

            LanguageTag tag = LanguageTag.ParseLocale(@base, exts);

            StringBuilder buf = new StringBuilder();
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
                buf.Append(LanguageTag.Private_Use).Append(LanguageTag.Separator);
                buf.Append(LanguageTag.CanonicalizePrivateuse(subtag));
            }

            return buf.ToString();
        }

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
        public static ULocale ForLanguageTag(string languageTag)
        {
            LanguageTag tag = LanguageTag.Parse(languageTag, null);
            InternalLocaleBuilder bldr = new InternalLocaleBuilder();
            bldr.SetLanguageTag(tag);
            return GetInstance(bldr.GetBaseLocale(), bldr.GetLocaleExtensions());
        }

        /**
         * {@icu} Converts the specified keyword (legacy key, or BCP 47 Unicode locale
         * extension key) to the equivalent BCP 47 Unicode locale extension key.
         * For example, BCP 47 Unicode locale extension key "co" is returned for
         * the input keyword "collation".
         * <para/>
         * When the specified keyword is unknown, but satisfies the BCP syntax,
         * then the lower-case version of the input keyword will be returned.
         * For example,
         * <code>toUnicodeLocaleKey("ZZ")</code> returns "zz".
         *
         * @param keyword       the input locale keyword (either legacy key
         *                      such as "collation" or BCP 47 Unicode locale extension
         *                      key such as "co").
         * @return              the well-formed BCP 47 Unicode locale extension key,
         *                      or null if the specified locale keyword cannot be mapped
         *                      to a well-formed BCP 47 Unicode locale extension key.
         * @see #toLegacyKey(string)
         * @stable ICU 54
         */
        public static string ToUnicodeLocaleKey(string keyword)
        {
            string bcpKey = KeyTypeData.ToBcpKey(keyword);
            if (bcpKey == null && UnicodeLocaleExtension.IsKey(keyword))
            {
                // unknown keyword, but syntax is fine..
                bcpKey = AsciiUtil.ToLower(keyword);
            }
            return bcpKey;
        }

        /**
         * {@icu} Converts the specified keyword value (legacy type, or BCP 47
         * Unicode locale extension type) to the well-formed BCP 47 Unicode locale
         * extension type for the specified keyword (category). For example, BCP 47
         * Unicode locale extension type "phonebk" is returned for the input
         * keyword value "phonebook", with the keyword "collation" (or "co").
         * <para/>
         * When the specified keyword is not recognized, but the specified value
         * satisfies the syntax of the BCP 47 Unicode locale extension type,
         * or when the specified keyword allows 'variable' type and the specified
         * value satisfies the syntax, the lower-case version of the input value
         * will be returned. For example,
         * <code>toUnicodeLocaleType("Foo", "Bar")</code> returns "bar",
         * <code>toUnicodeLocaleType("variableTop", "00A4")</code> returns "00a4".
         *
         * @param keyword       the locale keyword (either legacy key such as
         *                      "collation" or BCP 47 Unicode locale extension
         *                      key such as "co").
         * @param value         the locale keyword value (either legacy type
         *                      such as "phonebook" or BCP 47 Unicode locale extension
         *                      type such as "phonebk").
         * @return              the well-formed BCP47 Unicode locale extension type,
         *                      or null if the locale keyword value cannot be mapped to
         *                      a well-formed BCP 47 Unicode locale extension type.
         * @see #toLegacyType(string, string)
         * @stable ICU 54
         */
        public static string ToUnicodeLocaleType(string keyword, string value)
        {
            bool isKnownKey, isSpecialType;
            string bcpType = KeyTypeData.ToBcpType(keyword, value, out isKnownKey, out isSpecialType);
            if (bcpType == null && UnicodeLocaleExtension.IsType(value))
            {
                // unknown keyword, but syntax is fine..
                bcpType = AsciiUtil.ToLower(value);
            }
            return bcpType;
        }

        private static readonly Regex legacyKeyCheck = new Regex("^[0-9a-zA-Z]+$", RegexOptions.Compiled);
        private static readonly Regex legacyTypeCheck = new Regex("^[0-9a-zA-Z]+([_/\\-][0-9a-zA-Z]+)*$", RegexOptions.Compiled);

        /**
         * {@icu} Converts the specified keyword (BCP 47 Unicode locale extension key, or
         * legacy key) to the legacy key. For example, legacy key "collation" is
         * returned for the input BCP 47 Unicode locale extension key "co".
         *
         * @param keyword       the input locale keyword (either BCP 47 Unicode locale
         *                      extension key or legacy key).
         * @return              the well-formed legacy key, or null if the specified
         *                      keyword cannot be mapped to a well-formed legacy key.
         * @see #toUnicodeLocaleKey(string)
         * @stable ICU 54
         */
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

        /**
         * {@icu} Converts the specified keyword value (BCP 47 Unicode locale extension type,
         * or legacy type or type alias) to the canonical legacy type. For example,
         * the legacy type "phonebook" is returned for the input BCP 47 Unicode
         * locale extension type "phonebk" with the keyword "collation" (or "co").
         * <para/>
         * When the specified keyword is not recognized, but the specified value
         * satisfies the syntax of legacy key, or when the specified keyword
         * allows 'variable' type and the specified value satisfies the syntax,
         * the lower-case version of the input value will be returned.
         * For example,
         * <code>toLegacyType("Foo", "Bar")</code> returns "bar",
         * <code>toLegacyType("vt", "00A4")</code> returns "00a4".
         *
         * @param keyword       the locale keyword (either legacy keyword such as
         *                      "collation" or BCP 47 Unicode locale extension
         *                      key such as "co").
         * @param value         the locale keyword value (either BCP 47 Unicode locale
         *                      extension type such as "phonebk" or legacy keyword value
         *                      such as "phonebook").
         * @return              the well-formed legacy type, or null if the specified
         *                      keyword value cannot be mapped to a well-formed legacy
         *                      type.
         * @see #toUnicodeLocaleType(string, string)
         * @stable ICU 54
         */
        public static string ToLegacyType(string keyword, string value)
        {
            bool isKnownKey, isSpecialType;
            string legacyType = KeyTypeData.ToLegacyType(keyword, value, out isKnownKey, out isSpecialType);
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

        /**
         * <code>Builder</code> is used to build instances of <code>ULocale</code>
         * from values configured by the setters.  Unlike the <code>ULocale</code>
         * constructors, the <code>Builder</code> checks if a value configured by a
         * setter satisfies the syntax requirements defined by the <code>ULocale</code>
         * class.  A <code>ULocale</code> object created by a <code>Builder</code> is
         * well-formed and can be transformed to a well-formed IETF BCP 47 language tag
         * without losing information.
         *
         * <para/><b>Note:</b> The <code>ULocale</code> class does not provide any
         * syntactic restrictions on variant, while BCP 47 requires each variant
         * subtag to be 5 to 8 alphanumerics or a single numeric followed by 3
         * alphanumerics.  The method <code>setVariant</code> throws
         * <code>IllformedLocaleException</code> for a variant that does not satisfy
         * this restriction. If it is necessary to support such a variant, use a
         * ULocale constructor.  However, keep in mind that a <code>ULocale</code>
         * object created this way might lose the variant information when
         * transformed to a BCP 47 language tag.
         *
         * <para/>The following example shows how to create a <code>Locale</code> object
         * with the <code>Builder</code>.
         * <blockquote>
         * <pre>
         *     ULocale aLocale = new Builder().setLanguage("sr").setScript("Latn").setRegion("RS").build();
         * </pre>
         * </blockquote>
         *
         * <para/>Builders can be reused; <code>clear()</code> resets all
         * fields to their default values.
         *
         * @see ULocale#toLanguageTag()
         *
         * @stable ICU 4.2
         */
        public sealed class Builder
        {

            private readonly InternalLocaleBuilder _locbld;

            /**
             * Constructs an empty Builder. The default value of all
             * fields, extensions, and private use information is the
             * empty string.
             *
             * @stable ICU 4.2
             */
            public Builder()
            {
                _locbld = new InternalLocaleBuilder();
            }

            /**
             * Resets the <code>Builder</code> to match the provided
             * <code>locale</code>.  Existing state is discarded.
             *
             * <para/>All fields of the locale must be well-formed, see {@link Locale}.
             *
             * <para/>Locales with any ill-formed fields cause
             * <code>IllformedLocaleException</code> to be thrown.
             *
             * @param locale the locale
             * @return This builder.
             * @throws IllformedLocaleException if <code>locale</code> has
             * any ill-formed fields.
             * @throws NullPointerException if <code>locale</code> is null.
             *
             * @stable ICU 4.2
             */
            public Builder SetLocale(ULocale locale)
            {
                try
                {
                    _locbld.SetLocale(locale.Base(), locale.Extensions());
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.ToString(), e.HResult);
                }
                return this;
            }

            /**
             * Resets the Builder to match the provided IETF BCP 47
             * language tag.  Discards the existing state.  Null and the
             * empty string cause the builder to be reset, like {@link
             * #clear}.  Grandfathered tags (see {@link
             * ULocale#forLanguageTag}) are converted to their canonical
             * form before being processed.  Otherwise, the language tag
             * must be well-formed (see {@link ULocale}) or an exception is
             * thrown (unlike <code>ULocale.forLanguageTag</code>, which
             * just discards ill-formed and following portions of the
             * tag).
             *
             * @param languageTag the language tag
             * @return This builder.
             * @throws IllformedLocaleException if <code>languageTag</code> is ill-formed
             * @see ULocale#forLanguageTag(string)
             *
             * @stable ICU 4.2
             */
            public Builder SetLanguageTag(string languageTag)
            {
                ParseStatus sts = new ParseStatus();
                LanguageTag tag = LanguageTag.Parse(languageTag, sts);
                if (sts.IsError)
                {
                    throw new IllformedLocaleException(sts.ErrorMessage, sts.ErrorIndex);
                }
                _locbld.SetLanguageTag(tag);

                return this;
            }

            /**
             * Sets the language.  If <code>language</code> is the empty string or
             * null, the language in this <code>Builder</code> is removed.  Otherwise,
             * the language must be <a href="./Locale.html#def_language">well-formed</a>
             * or an exception is thrown.
             *
             * <para/>The typical language value is a two or three-letter language
             * code as defined in ISO639.
             *
             * @param language the language
             * @return This builder.
             * @throws IllformedLocaleException if <code>language</code> is ill-formed
             *
             * @stable ICU 4.2
             */
            public Builder SetLanguage(string language)
            {
                try
                {
                    _locbld.SetLanguage(language);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Sets the script. If <code>script</code> is null or the empty string,
             * the script in this <code>Builder</code> is removed.
             * Otherwise, the script must be well-formed or an exception is thrown.
             *
             * <para/>The typical script value is a four-letter script code as defined by ISO 15924.
             *
             * @param script the script
             * @return This builder.
             * @throws IllformedLocaleException if <code>script</code> is ill-formed
             *
             * @stable ICU 4.2
             */
            public Builder SetScript(string script)
            {
                try
                {
                    _locbld.SetScript(script);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Sets the region.  If region is null or the empty string, the region
             * in this <code>Builder</code> is removed.  Otherwise,
             * the region must be well-formed or an exception is thrown.
             *
             * <para/>The typical region value is a two-letter ISO 3166 code or a
             * three-digit UN M.49 area code.
             *
             * <para/>The country value in the <code>Locale</code> created by the
             * <code>Builder</code> is always normalized to upper case.
             *
             * @param region the region
             * @return This builder.
             * @throws IllformedLocaleException if <code>region</code> is ill-formed
             *
             * @stable ICU 4.2
             */
            public Builder SetRegion(string region)
            {
                try
                {
                    _locbld.SetRegion(region);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Sets the variant.  If variant is null or the empty string, the
             * variant in this <code>Builder</code> is removed.  Otherwise, it
             * must consist of one or more well-formed subtags, or an exception is thrown.
             *
             * <para/><b>Note:</b> This method checks if <code>variant</code>
             * satisfies the IETF BCP 47 variant subtag's syntax requirements,
             * and normalizes the value to lowercase letters.  However,
             * the <code>ULocale</code> class does not impose any syntactic
             * restriction on variant.  To set such a variant,
             * use a ULocale constructor.
             *
             * @param variant the variant
             * @return This builder.
             * @throws IllformedLocaleException if <code>variant</code> is ill-formed
             *
             * @stable ICU 4.2
             */
            public Builder SetVariant(string variant)
            {
                try
                {
                    _locbld.SetVariant(variant);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Sets the extension for the given key. If the value is null or the
             * empty string, the extension is removed.  Otherwise, the extension
             * must be well-formed or an exception is thrown.
             *
             * <para/><b>Note:</b> The key {@link ULocale#UNICODE_LOCALE_EXTENSION
             * UNICODE_LOCALE_EXTENSION} ('u') is used for the Unicode locale extension.
             * Setting a value for this key replaces any existing Unicode locale key/type
             * pairs with those defined in the extension.
             *
             * <para/><b>Note:</b> The key {@link ULocale#PRIVATE_USE_EXTENSION
             * PRIVATE_USE_EXTENSION} ('x') is used for the private use code. To be
             * well-formed, the value for this key needs only to have subtags of one to
             * eight alphanumeric characters, not two to eight as in the general case.
             *
             * @param key the extension key
             * @param value the extension value
             * @return This builder.
             * @throws IllformedLocaleException if <code>key</code> is illegal
             * or <code>value</code> is ill-formed
             * @see #setUnicodeLocaleKeyword(string, string)
             *
             * @stable ICU 4.2
             */
            public Builder SetExtension(char key, string value)
            {
                try
                {
                    _locbld.SetExtension(key, value);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Sets the Unicode locale keyword type for the given key.  If the type
             * is null, the Unicode keyword is removed.  Otherwise, the key must be
             * non-null and both key and type must be well-formed or an exception
             * is thrown.
             *
             * <para/>Keys and types are converted to lower case.
             *
             * <para/><b>Note</b>:Setting the 'u' extension via {@link #setExtension}
             * replaces all Unicode locale keywords with those defined in the
             * extension.
             *
             * @param key the Unicode locale key
             * @param type the Unicode locale type
             * @return This builder.
             * @throws IllformedLocaleException if <code>key</code> or <code>type</code>
             * is ill-formed
             * @throws NullPointerException if <code>key</code> is null
             * @see #setExtension(char, string)
             *
             * @stable ICU 4.4
             */
            public Builder SetUnicodeLocaleKeyword(string key, string type)
            {
                try
                {
                    _locbld.SetUnicodeLocaleKeyword(key, type);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Adds a unicode locale attribute, if not already present, otherwise
             * has no effect.  The attribute must not be null and must be well-formed
             * or an exception is thrown.
             *
             * @param attribute the attribute
             * @return This builder.
             * @throws NullPointerException if <code>attribute</code> is null
             * @throws IllformedLocaleException if <code>attribute</code> is ill-formed
             * @see #setExtension(char, string)
             *
             * @stable ICU 4.6
             */
            public Builder AddUnicodeLocaleAttribute(string attribute)
            {
                try
                {
                    _locbld.AddUnicodeLocaleAttribute(attribute);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Removes a unicode locale attribute, if present, otherwise has no
             * effect.  The attribute must not be null and must be well-formed
             * or an exception is thrown.
             *
             * <para/>Attribute comparision for removal is case-insensitive.
             *
             * @param attribute the attribute
             * @return This builder.
             * @throws NullPointerException if <code>attribute</code> is null
             * @throws IllformedLocaleException if <code>attribute</code> is ill-formed
             * @see #setExtension(char, string)
             *
             * @stable ICU 4.6
             */
            public Builder RemoveUnicodeLocaleAttribute(string attribute)
            {
                try
                {
                    _locbld.RemoveUnicodeLocaleAttribute(attribute);
                }
                catch (FormatException e)
                {
                    throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
                }
                return this;
            }

            /**
             * Resets the builder to its initial, empty state.
             *
             * @return this builder
             *
             * @stable ICU 4.2
             */
            public Builder Clear()
            {
                _locbld.Clear();
                return this;
            }

            /**
             * Resets the extensions to their initial, empty state.
             * Language, script, region and variant are unchanged.
             *
             * @return this builder
             * @see #setExtension(char, string)
             *
             * @stable ICU 4.2
             */
            public Builder ClearExtensions()
            {
                _locbld.ClearExtensions();
                return this;
            }

            /**
             * Returns an instance of <code>ULocale</code> created from the fields set
             * on this builder.
             *
             * @return a new Locale
             *
             * @stable ICU 4.4
             */
            public ULocale Build()
            {
                return GetInstance(_locbld.GetBaseLocale(), _locbld.GetLocaleExtensions());
            }
        }

        private static ULocale GetInstance(BaseLocale @base, LocaleExtensions exts)
        {
            string id = LscvToID(@base.GetLanguage(), @base.GetScript(), @base.GetRegion(),
                    @base.GetVariant());

            var extKeys = exts.Keys;
            if (extKeys.Any())
            {
                // legacy locale ID assume Unicode locale keywords and
                // other extensions are at the same level.
                // e.g. @a=ext-for-aa;calendar=japanese;m=ext-for-mm;x=priv-use

                SortedDictionary<string, string> kwds = new SortedDictionary<string, string>();
                foreach (char key in extKeys)
                {
                    Extension ext = exts.GetExtension(key);
                    if (ext is UnicodeLocaleExtension)
                    {
                        UnicodeLocaleExtension uext = (UnicodeLocaleExtension)ext;
                        var ukeys = uext.GetUnicodeLocaleKeys();
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
                        ISet<string> uattributes = uext.GetUnicodeLocaleAttributes();
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
                            kwds[LOCALE_ATTRIBUTE_KEY] = attrbuf.ToString();
                        }
                    }
                    else
                    {
                        kwds[new string(new char[] { key })] = ext.Value;
                    }
                }

                if (kwds.Any())
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
            return new ULocale(id);
        }

        private BaseLocale Base()
        {
            if (baseLocale == null)
            {
                string language, script, region, variant;
                language = script = region = variant = "";
                if (!Equals(ULocale.ROOT))
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

        private LocaleExtensions Extensions()
        {
            if (extensions == null)
            {
                var kwitr = GetKeywords();
                if (kwitr == null)
                {
                    extensions = LocaleExtensions.EmptyExtensions;
                }
                else
                {
                    InternalLocaleBuilder intbld = new InternalLocaleBuilder();
                    while (kwitr.MoveNext())
                    {
                        string key = kwitr.Current;
                        if (key.Equals(LOCALE_ATTRIBUTE_KEY))
                        {
                            // special keyword used for representing Unicode locale attributes
                            string[] uattributes = Regex.Split(GetKeywordValue(key), "[-_]");
                            foreach (string uattr in uattributes)
                            {
                                // ICU4N: Proactively check the parameter going in rather than responding to exceptions
                                if (uattr != null || UnicodeLocaleExtension.IsAttribute(uattr))
                                {
                                    intbld.AddUnicodeLocaleAttribute(uattr);
                                }

                                //try
                                //{
                                //    intbld.AddUnicodeLocaleAttribute(uattr);
                                //}
                                //catch (FormatException e)
                                //{
                                //    // ignore and fall through
                                //}
                            }
                        }
                        else if (key.Length >= 2)
                        {
                            string bcpKey = ToUnicodeLocaleKey(key);
                            string bcpType = ToUnicodeLocaleType(key, GetKeywordValue(key));
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
                        else if (key.Length == 1 && (key[0] != UNICODE_LOCALE_EXTENSION))
                        {
                            try
                            {
                                intbld.SetExtension(key[0], GetKeywordValue(key).Replace("_",
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

        /// <summary>
        /// .NET Locale Helper
        /// </summary>
        private sealed class DotNetLocaleHelper
        {
            /*
             *  Java
             * 6 locales.  When an ICU locale matches <minumum base> with
             * <keyword>/<value>, the ICU locale is mapped to <Java> locale.
             * For example, both ja_JP@calendar=japanese and ja@calendar=japanese
             * are mapped to Java locale "ja_JP_JP".  ICU locale "nn" is mapped
             * to Java locale "no_NO_NY".
             */

            ///// <summary>
            ///// This table is used for mapping between ICU and special .NET locales.
            ///// When an ICU locale matches &lt;minumum base&gt; with
            ///// &lt;keyword>/&tl;value>, the ICU locale is mapped to &lt;.NET> locale.
            ///// </summary>
            //private static readonly string[][] NET_MAPDATA = { // ICU4N TODO: Do we need different values for .NET Framework/.NET Standard ?
            //    //  { <Java>,       <ICU base>, <keyword>,  <value>,    <minimum base>
            //    new string[] { "ja-JP",   "ja_JP",    "calendar", "japanese", "ja"},
            //    //new string[] { "nn-NO",   "nn_NO",    null,       null,       "nn"},
            //    new string[] { "th-TH",   "th_TH",    "numbers",  "thai",     "th"},
            //};

            private static readonly string[][] NET_MAPDATA = { // ICU4N TODO: Do we need different values for .NET Framework/.NET Standard ?
                //  { <Java>,       <ICU base>, <keyword>,  <value>,    <minimum base>
                //new string[] { "ja-JP",   "ja_JP",    "calendar", "japanese", "ja"},
                //new string[] { "nn-NO",   "nn_NO",    null,       null,       "nn"},
                new string[] { "th-TH",   "th_TH",    "numbers",  "thai", "",     "th"},
            };

            private static readonly IDictionary<System.Type, string> DOTNET_CALENDARS = new Dictionary<System.Type, string>
            {
                { typeof(JapaneseCalendar), "japanese" },
                { typeof(ThaiBuddhistCalendar), "buddhist" },
                { typeof(ChineseLunisolarCalendar), "chinese" },
                { typeof(PersianCalendar), "persian" },
                { typeof(HijriCalendar), "islamic" },
                { typeof(HebrewCalendar), "hebrew" },
                { typeof(TaiwanCalendar), "taiwan" },
            };


            public static ULocale ToULocale(CultureInfo loc)
            {
                if (loc == CultureInfo.InvariantCulture)
                {
                    return new ULocale("");
                }

                var name = loc.Name;

                // Convert from RFC 4646, ISO 639, ISO 639-2, and ISO 15924 to ICU  format 


                // ICU4N TODO: Need to append currency, number, and collation data
                //name = name.Replace('-', '_');
                var segments = name.Split(new char[] { '-', '_' });
                string newName = "";
                for (int i = 0; i < segments.Length; i++)
                {
                    if (newName.Length > 0)
                        newName += '_';
                    if (i == 0)
                        newName += segments[i].ToLowerInvariant();
                    else
                        newName += segments[i].ToUpperInvariant(); // Special case - .NET makes the variant lower case, but ULocale expects upper case
                }

                // Special cases...

                string language = segments.Length > 0 ? segments[0] : "";
                string country = segments.Length > 1 ? segments[1] : "";
                string variant = segments.Length > 2 ? segments[2] : "";

                // .NET doesn't recognize no-NO-NY any more than ICU does, but if it is input,
                // we need to patch it (at least for the tests)

                if (language.Equals("no") && country.Equals("NO") && variant.Equals("NY", StringComparison.OrdinalIgnoreCase))
                {
                    newName = "nn_NO";
                }

                // Calander info
                var calandarType = loc.Calendar.GetType();
                if (!calandarType.Equals(typeof(GregorianCalendar)) && 
                    DOTNET_CALENDARS.TryGetValue(calandarType, out string calendar))
                {
                    string sep = newName.Contains("@") ? ";" : "@";
                    newName += string.Concat(sep, "calandar=", calendar);
                }

                //for (int i = 0; i < NET_MAPDATA.Length; i++)
                //{
                //    if (newName.StartsWith(NET_MAPDATA[i][1], StringComparison.Ordinal) & NET_MAPDATA[i][2] != null)
                //    {
                //        string sep = newName.Contains("@") ? ";" : "@";
                //        newName += string.Concat(sep, NET_MAPDATA[i][2], "=", NET_MAPDATA[i][3]);
                //    }
                //}


                return new ULocale(newName, loc);
            }

            public static CultureInfo ToLocale(ULocale uloc)
            {
                var name = uloc.GetName();

                if (name.Equals("root", StringComparison.OrdinalIgnoreCase) || name.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return CultureInfo.InvariantCulture;
                }

                // Strip off the config options
                int optionsIndex = name.IndexOf('@');
                if (optionsIndex > -1)
                {
                    // ICU4N TODO: Need to convert calendar, currency, number, and collation options by
                    // creating a custom CultureInfo subclass...where possible

                    name = name.Substring(0, optionsIndex); // ICU4N: Checked 2nd parameter
                }

                string newName = name.Replace('_', '-').Trim('-');

                // ICU4N special cases...

                var language = uloc.GetLanguage();
                var country = uloc.GetCountry();
                var variant = uloc.GetVariant();

                // .NET doesn't recognize no-NO-NY any more than ICU does, but if it is input,
                // we need to patch it (at least for the tests)

                if (language.Equals("no") && country.Equals("NO") && variant.Equals("NY"))
                {
                    newName = "nn-NO";
                }


                try
                {
                    CultureInfo culture = new CultureInfo(newName);

//#if NETSTANDARD1_3
//                    // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
//                    // to be created, but will be "unknown" languages. We need to manually
//                    // ignore these.
//                    if (culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
//                    {
//                        return null;
//                    }
//#endif
                    return culture;
                }
                catch (CultureNotFoundException)
                {
                    return null;
                }
            }

            public static CultureInfo GetDefault(Category category)
            {
                CultureInfo loc = CultureInfo.CurrentCulture;
                switch (category)
                {
                    case Category.DISPLAY:
                        loc = CultureInfo.CurrentUICulture;
                        break;
                    case Category.FORMAT:
                        loc = CultureInfo.CurrentCulture;
                        break;
                }
                return loc;
            }

            public static void SetDefault(Category category, CultureInfo newLocale)
            {
                switch (category)
                {
                    case Category.DISPLAY:
#if NETSTANDARD
                        CultureInfo.CurrentUICulture = newLocale;
#else
                        System.Threading.Thread.CurrentThread.CurrentUICulture = newLocale;
#endif
                        break;
                    case Category.FORMAT:
#if NETSTANDARD
                        CultureInfo.CurrentCulture = newLocale;
#else
                        System.Threading.Thread.CurrentThread.CurrentCulture = newLocale;
#endif
                        break;
                }
            }

            // Returns true if the given Locale matches the original
            // default locale initialized by JVM by checking user.XXX
            // system properties. When the system properties are not accessible,
            // this method returns false.
            public static bool IsOriginalDefaultLocale(CultureInfo loc)
            {
                return loc.Equals(CultureInfo.DefaultThreadCurrentCulture);
            }

        }

        /////*
        //// * JDK Locale Helper
        //// */
        //private sealed class JDKLocaleHelper
        //{
        //    private static bool hasScriptsAndUnicodeExtensions = false;
        //    private static bool hasLocaleCategories = false;

        //    /*
        //     * New methods in Java 7 Locale class
        //     */
        //    private static MethodInfo mGetScript;
        //    private static MethodInfo mGetExtensionKeys;
        //    private static MethodInfo mGetExtension;
        //    private static MethodInfo mGetUnicodeLocaleKeys;
        //    private static MethodInfo mGetUnicodeLocaleAttributes;
        //    private static MethodInfo mGetUnicodeLocaleType;
        //    private static MethodInfo mForLanguageTag;

        //    private static MethodInfo mGetDefault;
        //    private static MethodInfo mSetDefault;
        //    private static object eDISPLAY;
        //    private static object eFORMAT;

        //    /*
        //     * This table is used for mapping between ICU and special Java
        //     * 6 locales.  When an ICU locale matches <minumum base> with
        //     * <keyword>/<value>, the ICU locale is mapped to <Java> locale.
        //     * For example, both ja_JP@calendar=japanese and ja@calendar=japanese
        //     * are mapped to Java locale "ja_JP_JP".  ICU locale "nn" is mapped
        //     * to Java locale "no_NO_NY".
        //     */
        //    private static readonly string[][] JAVA6_MAPDATA = {
        //    //  { <Java>,       <ICU base>, <keyword>,  <value>,    <minimum base>
        //    new string[] { "ja_JP_JP",   "ja_JP",    "calendar", "japanese", "ja"},
        //    new string[] { "no_NO_NY",   "nn_NO",    null,       null,       "nn"},
        //    new string[] { "th_TH_TH",   "th_TH",    "numbers",  "thai",     "th"},
        //};

        //    static JDKLocaleHelper()
        //    {
        //        // 
        //        //            do {
        //        //                try {
        //        //                    mGetScript = Locale.class.getMethod("getScript", (Class[]) null);
        //        //    mGetExtensionKeys = Locale.class.getMethod("getExtensionKeys", (Class[]) null);
        //        //    mGetExtension = Locale.class.getMethod("getExtension", char.class);
        //        //                    mGetUnicodeLocaleKeys = Locale.class.getMethod("getUnicodeLocaleKeys", (Class[]) null);
        //        //    mGetUnicodeLocaleAttributes = Locale.class.getMethod("getUnicodeLocaleAttributes", (Class[]) null);
        //        //    mGetUnicodeLocaleType = Locale.class.getMethod("getUnicodeLocaleType", string.class);
        //        //                    mForLanguageTag = Locale.class.getMethod("forLanguageTag", string.class);

        //        //                    hasScriptsAndUnicodeExtensions = true;
        //        //                } catch (NoSuchMethodException e) {
        //        //                } catch (IllegalArgumentException e) {
        //        //                } catch (SecurityException e) {
        //        //                    // TODO : report?
        //        //                }

        //        //                try {
        //        //                    Class<?> cCategory = null;
        //        //Class<?>[] classes = Locale.class.getDeclaredClasses();
        //        //                    for (Class<?> c : classes) {
        //        //                        if (c.getName().equals("java.util.Locale$Category")) {
        //        //                            cCategory = c;
        //        //                            break;
        //        //                        }
        //        //                    }
        //        //                    if (cCategory == null) {
        //        //                        break;
        //        //                    }
        //        //                    mGetDefault = Locale.class.getDeclaredMethod("getDefault", cCategory);
        //        //mSetDefault = Locale.class.getDeclaredMethod("setDefault", cCategory, Locale.class);

        //        //                    Method mName = cCategory.getMethod("name", (Class[])null);
        //        //Object[] enumConstants = cCategory.getEnumConstants();
        //        //                    for (Object e : enumConstants) {
        //        //                        string catVal = (string)mName.invoke(e, (Object[])null);
        //        //                        if (catVal.equals("DISPLAY")) {
        //        //                            eDISPLAY = e;
        //        //                        } else if (catVal.equals("FORMAT")) {
        //        //                            eFORMAT = e;
        //        //                        }
        //        //                    }
        //        //                    if (eDISPLAY == null || eFORMAT == null) {
        //        //                        break;
        //        //                    }

        //        //                    hasLocaleCategories = true;
        //        //                } catch (NoSuchMethodException e) {
        //        //                } catch (IllegalArgumentException e) {
        //        //                } catch (IllegalAccessException e) {
        //        //                } catch (InvocationTargetException e) {
        //        //                } catch (SecurityException e) {
        //        //                    // TODO : report?
        //        //                }
        //        //            } while (false);
        //    }

        //    private JDKLocaleHelper()
        //    {
        //    }

        //    public static bool HasLocaleCategories() // ICU4N TODO: Make property
        //    {
        //        return hasLocaleCategories;
        //    }

        //    public static ULocale ToULocale(CultureInfo loc)
        //    {
        //        return hasScriptsAndUnicodeExtensions ? ToULocale7(loc) : ToULocale6(loc);
        //    }

        //    public static CultureInfo ToLocale(ULocale uloc)
        //    {
        //        return hasScriptsAndUnicodeExtensions ? ToLocale7(uloc) : ToLocale6(uloc);
        //    }

        //    private static ULocale ToULocale7(CultureInfo loc)
        //    {
        //        string language = loc.GetLanguage();
        //        string script = "";
        //        string country = loc.GetCountry();
        //        string variant = loc.GetVariant();

        //        ISet<string> attributes = null;
        //        IDictionary<string, string> keywords = null;

        //        //try
        //        //{
        //        script = (string)mGetScript.Invoke(loc, (object[])null);
        //        //@SuppressWarnings("unchecked")
        //        ISet<char> extKeys = (ISet<char>)mGetExtensionKeys.Invoke(loc, (object[])null);
        //        if (extKeys.Any())
        //        {
        //            foreach (char extKey in extKeys)
        //            {
        //                if (extKey == 'u')
        //                {
        //                    // Found Unicode locale extension

        //                    // attributes
        //                    //@SuppressWarnings("unchecked")
        //                    ISet<string> uAttributes = (ISet<string>)mGetUnicodeLocaleAttributes.Invoke(loc, (object[])null);
        //                    if (uAttributes.Any())
        //                    {
        //                        attributes = new SortedSet<string>(StringComparer.Ordinal);
        //                        foreach (string attr in uAttributes)
        //                        {
        //                            attributes.Add(attr);
        //                        }
        //                    }

        //                    // keywords
        //                    //@SuppressWarnings("unchecked")
        //                    ISet<string> uKeys = (ISet<string>)mGetUnicodeLocaleKeys.Invoke(loc, (object[])null);
        //                    foreach (string kwKey in uKeys)
        //                    {
        //                        string kwVal = (string)mGetUnicodeLocaleType.Invoke(loc, new object[] { kwKey });
        //                        if (kwVal != null)
        //                        {
        //                            if (kwKey.Equals("va"))
        //                            {
        //                                // va-* is interpreted as a variant
        //                                variant = (variant.Length == 0) ? kwVal : kwVal + "_" + variant;
        //                            }
        //                            else
        //                            {
        //                                if (keywords == null)
        //                                {
        //                                    keywords = new SortedDictionary<string, string>();
        //                                }
        //                                keywords[kwKey] = kwVal;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    string extVal = (string)mGetExtension.Invoke(loc, new object[] { extKey });
        //                    if (extVal != null)
        //                    {
        //                        if (keywords == null)
        //                        {
        //                            keywords = new SortedDictionary<string, string>();
        //                        }
        //                        keywords[new string(new char[] { extKey })] = extVal;
        //                    }
        //                }
        //            }
        //        }
        //        //}
        //        //catch (IllegalAccessException e)
        //        //{
        //        //    throw new RuntimeException(e);
        //        //}
        //        //catch (InvocationTargetException e)
        //        //{
        //        //    throw new RuntimeException(e);
        //        //}

        //        // JDK locale no_NO_NY is not interpreted as Nynorsk by ICU,
        //        // and it should be transformed to nn_NO.

        //        // Note: JDK7+ unerstand both no_NO_NY and nn_NO. When convert
        //        // ICU locale to JDK, we do not need to map nn_NO back to no_NO_NY.

        //        if (language.Equals("no") && country.Equals("NO") && variant.Equals("NY")) 
        //        {
        //            language = "nn";
        //            variant = "";
        //        }

        //        // Constructing ID
        //        StringBuilder buf = new StringBuilder(language);

        //        if (script.Length > 0)
        //        {
        //            buf.Append('_');
        //            buf.Append(script);
        //        }

        //        if (country.Length > 0)
        //        {
        //            buf.Append('_');
        //            buf.Append(country);
        //        }

        //        if (variant.Length > 0)
        //        {
        //            if (country.Length == 0)
        //            {
        //                buf.Append('_');
        //            }
        //            buf.Append('_');
        //            buf.Append(variant);
        //        }

        //        if (attributes != null)
        //        {
        //            // transform Unicode attributes into a keyword
        //            StringBuilder attrBuf = new StringBuilder();
        //            foreach (string attr in attributes)
        //            {
        //                if (attrBuf.Length != 0)
        //                {
        //                    attrBuf.Append('-');
        //                }
        //                attrBuf.Append(attr);
        //            }
        //            if (keywords == null)
        //            {
        //                keywords = new SortedDictionary<string, string>();
        //            }
        //            keywords[LOCALE_ATTRIBUTE_KEY] = attrBuf.ToString();
        //        }

        //        if (keywords != null)
        //        {
        //            buf.Append('@');
        //            bool addSep = false;
        //            foreach (var kwEntry in keywords)
        //            {
        //                string kwKey = kwEntry.Key;
        //                string kwVal = kwEntry.Value;

        //                if (kwKey.Length != 1)
        //                {
        //                    // Unicode locale key
        //                    kwKey = ToLegacyKey(kwKey);
        //                    // use "yes" as the value of typeless keywords
        //                    kwVal = ToLegacyType(kwKey, ((kwVal.Length == 0) ? "yes" : kwVal));
        //                }

        //                if (addSep)
        //                {
        //                    buf.Append(';');
        //                }
        //                else
        //                {
        //                    addSep = true;
        //                }
        //                buf.Append(kwKey);
        //                buf.Append('=');
        //                buf.Append(kwVal);
        //            }
        //        }

        //        return new ULocale(GetName(buf.ToString()), loc);
        //    }

        //    private static ULocale ToULocale6(CultureInfo loc)
        //    {
        //        ULocale uloc = null;
        //        string locStr = loc.ToString();
        //        if (locStr.Length == 0)
        //        {
        //            uloc = ULocale.ROOT;
        //        }
        //        else
        //        {
        //            for (int i = 0; i < JAVA6_MAPDATA.Length; i++)
        //            {
        //                if (JAVA6_MAPDATA[i][0].Equals(locStr))
        //                {
        //                    LocaleIDParser p = new LocaleIDParser(JAVA6_MAPDATA[i][1]);
        //                    p.SetKeywordValue(JAVA6_MAPDATA[i][2], JAVA6_MAPDATA[i][3]);
        //                    locStr = p.GetName();
        //                    break;
        //                }
        //            }
        //            uloc = new ULocale(GetName(locStr), loc);
        //        }
        //        return uloc;
        //    }

        //    private static CultureInfo ToLocale7(ULocale uloc)
        //    {
        //        CultureInfo loc = null;
        //        string ulocStr = uloc.GetName();
        //        if (uloc.GetScript().Length > 0 || ulocStr.Contains("@"))
        //        {
        //            // With script or keywords available, the best way
        //            // to get a mapped Locale is to go through a language tag.
        //            // A Locale with script or keywords can only have variants
        //            // that is 1 to 8 alphanum. If this ULocale has a variant
        //            // subtag not satisfying the criteria, the variant subtag
        //            // will be lost.
        //            string tag = uloc.ToLanguageTag();

        //            // Workaround for variant casing problem:
        //            //
        //            // The variant field in ICU is case insensitive and normalized
        //            // to upper case letters by getVariant(), while
        //            // the variant field in JDK Locale is case sensitive.
        //            // ULocale#toLanguageTag use lower case characters for
        //            // BCP 47 variant and private use x-lvariant.
        //            //
        //            // Locale#forLanguageTag in JDK preserves character casing
        //            // for variant. Because ICU always normalizes variant to
        //            // upper case, we convert language tag to upper case here.
        //            tag = AsciiUtil.ToUpperString(tag);

        //            //try
        //            //{
        //            loc = (CultureInfo)mForLanguageTag.Invoke(null, new object[] { tag });
        //            //}
        //            //catch (IllegalAccessException e)
        //            //{
        //            //    throw new RuntimeException(e);
        //            //}
        //            //catch (InvocationTargetException e)
        //            //{
        //            //    throw new RuntimeException(e);
        //            //}
        //        }
        //        if (loc == null)
        //        {
        //            // Without script or keywords, use a Locale constructor,
        //            // so we can preserve any ill-formed variants.
        //            //loc = new Locale(uloc.getLanguage(), uloc.getCountry(), uloc.getVariant());
        //            loc = new CultureInfo(uloc.GetLanguage() + "-" + uloc.GetCountry());
        //        }
        //        return loc;
        //    }

        //    private static CultureInfo ToLocale6(ULocale uloc)
        //    {
        //        string locstr = uloc.GetBaseName();
        //        for (int i = 0; i < JAVA6_MAPDATA.Length; i++)
        //        {
        //            if (locstr.Equals(JAVA6_MAPDATA[i][1]) || locstr.Equals(JAVA6_MAPDATA[i][4]))
        //            {
        //                if (JAVA6_MAPDATA[i][2] != null)
        //                {
        //                    string val = uloc.GetKeywordValue(JAVA6_MAPDATA[i][2]);
        //                    if (val != null && val.Equals(JAVA6_MAPDATA[i][3]))
        //                    {
        //                        locstr = JAVA6_MAPDATA[i][0];
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    locstr = JAVA6_MAPDATA[i][0];
        //                    break;
        //                }
        //            }
        //        }
        //        LocaleIDParser p = new LocaleIDParser(locstr);
        //        string[] names = p.GetLanguageScriptCountryVariant();
        //        //return new Locale(names[0], names[2], names[3]);
        //        return new CultureInfo(names[0] + "-" + names[2] + "-" + names[3]);
        //    }

        //    public static CultureInfo GetDefault(Category category)
        //    {
        //        CultureInfo loc = CultureInfo.CurrentCulture;
        //        if (hasLocaleCategories)
        //        {
        //            object cat = null;
        //            switch (category)
        //            {
        //                case Category.DISPLAY:
        //                    cat = eDISPLAY;
        //                    break;
        //                case Category.FORMAT:
        //                    cat = eFORMAT;
        //                    break;
        //            }
        //            if (cat != null)
        //            {
        //                //try
        //                //{
        //                loc = (CultureInfo)mGetDefault.Invoke(null, new object[] { cat });
        //                //}
        //                //catch (InvocationTargetException e)
        //                //{
        //                //    // fall through - use the base default
        //                //}
        //                //catch (IllegalArgumentException e)
        //                //{
        //                //    // fall through - use the base default
        //                //}
        //                //catch (IllegalAccessException e)
        //                //{
        //                //    // fall through - use the base default
        //                //}
        //            }
        //        }
        //        return loc;
        //    }

        //    public static void SetDefault(Category category, CultureInfo newLocale)
        //    {
        //        if (hasLocaleCategories)
        //        {
        //            object cat = null;
        //            switch (category)
        //            {
        //                case Category.DISPLAY:
        //                    cat = eDISPLAY;
        //                    break;
        //                case Category.FORMAT:
        //                    cat = eFORMAT;
        //                    break;
        //            }
        //            if (cat != null)
        //            {
        //                //try
        //                //{
        //                mSetDefault.Invoke(null, new object[] { cat, newLocale });
        //                //}
        //                //catch (InvocationTargetException e)
        //                //{
        //                //    // fall through - no effects
        //                //}
        //                //catch (IllegalArgumentException e)
        //                //{
        //                //    // fall through - no effects
        //                //}
        //                //catch (IllegalAccessException e)
        //                //{
        //                //    // fall through - no effects
        //                //}
        //            }
        //        }
        //    }

        //    // Returns true if the given Locale matches the original
        //    // default locale initialized by JVM by checking user.XXX
        //    // system properties. When the system properties are not accessible,
        //    // this method returns false.
        //    public static bool IsOriginalDefaultLocale(CultureInfo loc)
        //    {
        //        return loc.Equals(CultureInfo.DefaultThreadCurrentCulture);

        //        //if (hasScriptsAndUnicodeExtensions)
        //        //{
        //        //    string script = "";
        //        //    try
        //        //    {
        //        //        script = (string)mGetScript.Invoke(loc, (Object[])null);
        //        //    }
        //        //    catch (Exception e)
        //        //    {
        //        //        return false;
        //        //    }

        //        //    return loc.GetLanguage().Equals(GetSystemProperty("user.language"))
        //        //            && loc.GetCountry().Equals(GetSystemProperty("user.country"))
        //        //            && loc.GetVariant().Equals(GetSystemProperty("user.variant"))
        //        //            && script.Equals(GetSystemProperty("user.script"));
        //        //}
        //        //return loc.GetLanguage().Equals(GetSystemProperty("user.language"))
        //        //        && loc.GetCountry().Equals(GetSystemProperty("user.country"))
        //        //        && loc.GetVariant().Equals(GetSystemProperty("user.variant"));
        //    }

        //    public static string GetSystemProperty(string key)
        //    {
        //        return null;
        //                     //    string val = null;
        //                     //    string fkey = key;
        //                     //    if (System.getSecurityManager() != null)
        //                     //    {
        //                     //        try
        //                     //        {
        //                     //            val = AccessController.doPrivileged(new PrivilegedAction<string>() {
        //                     //                        @Override
        //                     //                        public string run()
        //                     //            {
        //                     //                return System.getProperty(fkey);
        //                     //            }
        //                     //        });
        //                     //    } catch (AccessControlException e)
        //                     //    {
        //                     //        // ignore
        //                     //    }
        //                     //} else {
        //                     //                val = System.getProperty(fkey);
        //                     //            }
        //                     //            return val;
        //    }
        //}
    }
}
