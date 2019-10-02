using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.Resources;
using Category = ICU4N.Util.ULocale.Category; // ICU4N TODO: API de-nest?

namespace ICU4N.Util
{
    /// <summary>
    /// A class for accessing miscellaneous data in the locale bundles
    /// </summary>
    /// <author>ram</author>
    /// <stable>2.8</stable>
    public sealed class LocaleData
    {
        //    private static final String EXEMPLAR_CHARS      = "ExemplarCharacters";
        private static readonly string MEASUREMENT_SYSTEM = "MeasurementSystem";
        private static readonly string PAPER_SIZE = "PaperSize";
        private static readonly string LOCALE_DISPLAY_PATTERN = "localeDisplayPattern";
        private static readonly string PATTERN = "pattern";
        private static readonly string SEPARATOR = "separator";
        private bool noSubstitute;
        private ICUResourceBundle bundle;
        private ICUResourceBundle langBundle;

        /// <summary>
        /// EXType for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// Corresponds to the 'main' (aka 'standard') CLDR exemplars in
        /// <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
        /// http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int ES_STANDARD = 0; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// EXType for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// Corresponds to the 'auxiliary' CLDR exemplars in
        /// <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
        /// http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int ES_AUXILIARY = 1; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// EXType for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// Corresponds to the 'index' CLDR exemplars in
        /// <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
        /// http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static readonly int ES_INDEX = 2; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// EXType for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// Corresponds to the 'currencySymbol' CLDR exemplars in
        /// <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
        /// http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
        /// Note: This type is no longer supported.
        /// </summary>
        [Obsolete("ICU 51 This type is no longer supported.")]
        public static readonly int ES_CURRENCY = 3; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// Corresponds to the 'punctuation' CLDR exemplars in
        /// <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
        /// http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
        /// EXType for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// </summary>
        /// <stable>ICU 49</stable>
        public static readonly int ES_PUNCTUATION = 4; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// Count of EXTypes for <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        public static readonly int ES_COUNT = 5; // ICU4N TODO: API - Make enum ExemplarSetType

        /// <summary>
        /// Delimiter type for <see cref="GetDelimiter(int)"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int QUOTATION_START = 0; // ICU4N TODO: API - Make enum

        /// <summary>
        /// Delimiter type for <see cref="GetDelimiter(int)"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int QUOTATION_END = 1; // ICU4N TODO: API - Make enum

        /// <summary>
        /// Delimiter type for <see cref="GetDelimiter(int)"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int ALT_QUOTATION_START = 2; // ICU4N TODO: API - Make enum

        /// <summary>
        /// Delimiter type for <see cref="GetDelimiter(int)"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int ALT_QUOTATION_END = 3; // ICU4N TODO: API - Make enum

        /// <summary>
        /// Count of delimiter types for <see cref="GetDelimiter(int)"/>.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        public static readonly int DELIMITER_COUNT = 4; // ICU4N TODO: API - Make enum

        // private constructor to prevent default construction
        ////CLOVER:OFF
        private LocaleData() { }
        ////CLOVER:ON

        /// <summary>
        /// Returns the set of exemplar characters for a locale. Equivalent to calling 
        /// <see cref="GetExemplarSet(ULocale, PatternOptions, int)"/> with the
        /// extype == <see cref="ES_STANDARD"/>.
        /// </summary>
        /// <param name="locale">Locale for which the exemplar character set
        /// is to be retrieved.</param>
        /// <param name="options">
        /// <see cref="PatternOptions"/> flags to apply to the exemplar pattern.
        /// Specify <see cref="PatternOptions.Default"/> to retrieve the exemplar set as it is
        /// defined in the locale data.  Specify <see cref="PatternOptions.Case"/> to retrieve a case-folded exemplar
        /// set.  See <see cref="PatternOptions"/> for a complete list of valid options.  The
        /// <see cref="PatternOptions.IgnoreSpace"/> bit is always set, regardless of the
        /// value of <paramref name="options"/>.
        /// </param>
        /// <returns>The set of exemplar characters for the given locale.</returns>
        /// <stable>ICU 3.0</stable>
        public static UnicodeSet GetExemplarSet(ULocale locale, PatternOptions options)
        {
            return LocaleData.GetInstance(locale).GetExemplarSet(options, ES_STANDARD);
        }

        /// <summary>
        /// Returns the set of exemplar characters for a <paramref name="locale"/>.
        /// Equivalent to calling <c>new LocaleData(locale)</c>. <see cref="GetExemplarSet(PatternOptions, int)"/>.
        /// </summary>
        /// <param name="locale">Locale for which the exemplar character set
        /// is to be retrieved.</param>
        /// <param name="options">
        /// <see cref="PatternOptions"/> flags to apply to the exemplar pattern.
        /// Specify <see cref="PatternOptions.Default"/> to retrieve the exemplar set as it is
        /// defined in the locale data.  Specify <see cref="PatternOptions.Case"/> to retrieve a case-folded exemplar
        /// set.  See <see cref="PatternOptions"/> for a complete list of valid options.  The
        /// <see cref="PatternOptions.IgnoreSpace"/> bit is always set, regardless of the
        /// value of <paramref name="options"/>.
        /// </param>
        /// <param name="extype">The type of exemplar character set to retrieve.</param>
        /// <returns>The set of exemplar characters for the given <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UnicodeSet GetExemplarSet(ULocale locale, PatternOptions options, int extype) // ICU4N TODO: API - make extype into enum ExemplarSetType
        {
            return LocaleData.GetInstance(locale).GetExemplarSet(options, extype);
        }

        /// <summary>
        /// Returns the set of exemplar characters for a locale.
        /// </summary>
        /// <param name="options">
        /// Bitmask for options to apply to the exemplar pattern.
        /// Specify zero to retrieve the exemplar set as it is
        /// defined in the locale data.  Specify <see cref="PatternOptions.Case"/>
        /// to retrieve a case-folded exemplar
        /// set.  See <see cref="PatternOptions"/> for a complete list of valid options.  The
        /// <see cref="PatternOptions.IgnoreSpace"/> bit is always set, regardless of the
        /// value of <paramref name="options"/>.
        /// </param>
        /// <param name="extype">The type of exemplar set to be retrieved,
        /// <see cref="ES_STANDARD"/>, <see cref="ES_INDEX"/>, <see cref="ES_AUXILIARY"/>, or <see cref="ES_PUNCTUATION"/></param>
        /// <returns>
        /// The set of exemplar characters for the given locale.
        /// If there is nothing available for the locale,
        /// then null is returned if <see cref="NoSubstitute"/> is <c>true</c>, otherwise the
        /// root value is returned (which may be <see cref="UnicodeSet.Empty"/>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="extype"/> is invalid.</exception>
        /// <stable>ICU 3.4</stable>
        public UnicodeSet GetExemplarSet(PatternOptions options, int extype) // ICU4N TODO: API - make extype into enum ExemplarSetType
        {
            string[] exemplarSetTypes = {
                    "ExemplarCharacters",
                    "AuxExemplarCharacters",
                    "ExemplarCharactersIndex",
                    "ExemplarCharactersCurrency",
                    "ExemplarCharactersPunctuation"
            };

#pragma warning disable 612, 618
            if (extype == ES_CURRENCY)
#pragma warning restore 612, 618
            {
                // currency symbol exemplar is no longer available
                return noSubstitute ? null : UnicodeSet.Empty;
            }

            try
            {
                string aKey = exemplarSetTypes[extype]; // will throw an out-of-bounds exception
                ICUResourceBundle stringBundle = (ICUResourceBundle)bundle.Get(aKey);

                if (noSubstitute && !bundle.IsRoot && stringBundle.IsRoot)
                {
                    return null;
                }
                string unicodeSetPattern = stringBundle.GetString();
                return new UnicodeSet(unicodeSetPattern, UnicodeSet.IgnoreSpace | options);
            }
            catch (IndexOutOfRangeException aiooe)
            {
                throw new ArgumentException(aiooe.Message, aiooe);
            }
            catch (Exception ex)
            {
                return noSubstitute ? null : UnicodeSet.Empty;
            }
        }

        /// <summary>
        /// Gets the <see cref="LocaleData"/> object associated with the <see cref="ULocale"/> specified in <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale"><see cref="ULocale"/> with thich the locale data object is associated.</param>
        /// <returns>A locale data object.</returns>
        /// <stable>ICU 3.4</stable>
        public static LocaleData GetInstance(ULocale locale)
        {
            LocaleData ld = new LocaleData();
            ld.bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, locale);
            ld.langBundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_LANG_BASE_NAME, locale);
            ld.noSubstitute = false;
            return ld;
        }

        /// <summary>
        /// Gets the <see cref="LocaleData"/> object associated with the default <see cref="Category.FORMAT"/> locale.
        /// </summary>
        /// <returns>A locale data object.</returns>
        /// <see cref="Category.FORMAT"/>
        /// <stable>ICU 3.4</stable>
        public static LocaleData GetInstance()
        {
            return LocaleData.GetInstance(ULocale.GetDefault(Category.FORMAT));
        }

        /// <summary>
        /// Gets or sets the "no substitute" behavior of this locale data object.
        /// <para/>
        /// If <c>true</c>, methods of this locale data object will return
        /// an error when no data is available for that method,
        /// given the locale ID supplied to the constructor.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public bool NoSubstitute
        {
            get { return noSubstitute; }
            set { noSubstitute = value; }
        }

        private static readonly string[] DELIMITER_TYPES = {
            "quotationStart",
            "quotationEnd",
            "alternateQuotationStart",
            "alternateQuotationEnd"
        };

        /// <summary>
        /// Retrieves a delimiter string from the locale data.
        /// </summary>
        /// <param name="type">
        /// The type of delimiter string desired.  Currently,
        /// the valid choices are <see cref="QUOTATION_START"/>, <see cref="QUOTATION_END"/>,
        /// <see cref="ALT_QUOTATION_START"/>, or <see cref="ALT_QUOTATION_END"/>.
        /// </param>
        /// <returns>The desired delimiter string.</returns>
        /// <stable>ICU 3.4</stable>
        public string GetDelimiter(int type)
        {
            ICUResourceBundle delimitersBundle = (ICUResourceBundle)bundle.Get("delimiters");
            // Only some of the quotation marks may be here. So we make sure that we do a multilevel fallback.
            ICUResourceBundle stringBundle = delimitersBundle.GetWithFallback(DELIMITER_TYPES[type]);

            if (noSubstitute && !bundle.IsRoot && stringBundle.IsRoot)
            {
                return null;
            }
            return stringBundle.GetString();
        }

        /// <summary>
        /// Utility for <see cref="GetMeasurementSystem(ULocale)"/> and <see cref="GetPaperSize(ULocale)"/>
        /// </summary>
        private static UResourceBundle MeasurementTypeBundleForLocale(ULocale locale, string measurementType)
        {
            // Much of this is taken from getCalendarType in impl/CalendarUtil.java
            UResourceBundle measTypeBundle = null;
#pragma warning disable 612, 618
            string region = ULocale.GetRegionForSupplementalData(locale, true);
#pragma warning restore 612, 618
            try
            {
                UResourceBundle rb = UResourceBundle.GetBundleInstance(
                        ICUData.ICU_BASE_NAME,
                        "supplementalData",
                        ICUResourceBundle.ICU_DATA_CLASS_LOADER);
                UResourceBundle measurementData = rb.Get("measurementData");
                UResourceBundle measDataBundle = null;
                try
                {
                    measDataBundle = measurementData.Get(region);
                    measTypeBundle = measDataBundle.Get(measurementType);
                }
                catch (MissingManifestResourceException mre)
                {
                    // use "001" as fallback
                    measDataBundle = measurementData.Get("001");
                    measTypeBundle = measDataBundle.Get(measurementType);
                }
            }
            catch (MissingManifestResourceException mre)
            {
                // fall through
            }
            return measTypeBundle;
        }


        /**
         * Enumeration for representing the measurement systems.
         * @stable ICU 2.8
         */
        //public enum MeasurementSystem
        //{
        //    SI,
        //    US,
        //    UK
        //}

        public sealed class MeasurementSystem
        {
            /// <summary>
            /// Measurement system specified by Le Syst&#x00E8;me International d'Unit&#x00E9;s (SI)
            /// otherwise known as Metric system.
            /// </summary>
            /// <stable>ICU 2.8</stable>
            public static readonly MeasurementSystem SI = new MeasurementSystem();
            /// <summary>
            /// Measurement system followed in the United States of America.
            /// </summary>
            /// <stable>ICU 2.8</stable>
            public static readonly MeasurementSystem US = new MeasurementSystem();

            /// <summary>
            /// Mix of metric and imperial units used in Great Britain.
            /// </summary>
            /// <stable>ICU 55</stable>
            public static readonly MeasurementSystem UK = new MeasurementSystem();

            private MeasurementSystem() { }
        }

        /// <summary>
        /// Returns the measurement system used in the locale specified by the locale.
        /// </summary>
        /// <param name="locale">The locale for which the measurement system to be retrieved.</param>
        /// <returns>The <see cref="MeasurementSystem"/> used in the locale.</returns>
        /// <stable>ICU 3.0</stable>
        public static MeasurementSystem GetMeasurementSystem(ULocale locale)
        {
            UResourceBundle sysBundle = MeasurementTypeBundleForLocale(locale, MEASUREMENT_SYSTEM);

            switch (sysBundle.GetInt32())
            {
                case 0: return MeasurementSystem.SI;
                case 1: return MeasurementSystem.US;
                case 2: return MeasurementSystem.UK;
                default:
                    // return null if the object is null or is not an instance
                    // of integer indicating an error
                    return null;
            }
        }

        /// <summary>
        /// A class that represents the size of letter head
        /// used in the country.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public sealed class PaperSize
        {
            private int height;
            private int width;

            internal PaperSize(int h, int w)
            {
                height = h;
                width = w;
            }

            /// <summary>
            /// Gets the height of the paper.
            /// </summary>
            /// <stable>ICU 2.8</stable>
            public int Height
            {
                get { return height; }
            }

            /// <summary>
            /// Gets the width of the paper.
            /// </summary>
            /// <stable>ICU 2.8</stable>
            public int Width
            {
                get { return width; }
            }
        }

        /// <summary>
        /// Returns the size of paper used in the locale. The paper sizes returned are always in
        /// <em>milli-meters</em>.
        /// </summary>
        /// <param name="locale">The locale for which the measurement system to be retrieved.</param>
        /// <returns>The paper size used in the locale.</returns>
        /// <stable>ICU 3.0</stable>
        public static PaperSize GetPaperSize(ULocale locale)
        {
            UResourceBundle obj = MeasurementTypeBundleForLocale(locale, PAPER_SIZE);
            int[] size = obj.GetInt32Vector();
            return new PaperSize(size[0], size[1]);
        }

        /// <summary>
        /// Returns LocaleDisplayPattern for this locale, e.g., {0}({1})
        /// </summary>
        /// <returns>Locale display pattern as a <see cref="string"/>.</returns>
        /// <stable>ICU 4.2</stable>
        public string GetLocaleDisplayPattern()
        {
            ICUResourceBundle locDispBundle = (ICUResourceBundle)langBundle.Get(LOCALE_DISPLAY_PATTERN);
            string localeDisplayPattern = locDispBundle.GetStringWithFallback(PATTERN);
            return localeDisplayPattern;
        }

        /// <summary>
        /// Returns LocaleDisplaySeparator for this locale.
        /// </summary>
        /// <returns>Locale display separator as a char.</returns>
        /// <stable>ICU 4.2</stable>
        public string GetLocaleSeparator()
        {
            string sub0 = "{0}";
            string sub1 = "{1}";
            ICUResourceBundle locDispBundle = (ICUResourceBundle)langBundle.Get(LOCALE_DISPLAY_PATTERN);
            string localeSeparator = locDispBundle.GetStringWithFallback(SEPARATOR);
            int index0 = localeSeparator.IndexOf(sub0, StringComparison.Ordinal);
            int index1 = localeSeparator.IndexOf(sub1, StringComparison.Ordinal);
            if (index0 >= 0 && index1 >= 0 && index0 <= index1)
            {
                return localeSeparator.Substring(index0 + sub0.Length, index1 - (index0 + sub0.Length)); // ICU4N: Corrected 2nd parameter
            }
            return localeSeparator;
        }

        private static VersionInfo gCLDRVersion = null;

        /// <summary>
        /// Returns the current CLDR version
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static VersionInfo GetCLDRVersion()
        {
            // fetching this data should be idempotent.
            if (gCLDRVersion == null)
            {
                // from ZoneMeta.java
                UResourceBundle supplementalDataBundle = UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, "supplementalData", ICUResourceBundle.ICU_DATA_CLASS_LOADER);
                UResourceBundle cldrVersionBundle = supplementalDataBundle.Get("cldrVersion");
                gCLDRVersion = VersionInfo.GetInstance(cldrVersionBundle.GetString());
            }
            return gCLDRVersion;
        }
    }
}
