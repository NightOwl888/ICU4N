using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
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

        /**
         * EXType for {@link #getExemplarSet(int, int)}.
         * Corresponds to the 'main' (aka 'standard') CLDR exemplars in
         * <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
         *   http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
         * @stable ICU 3.4
         */
        public static readonly int ES_STANDARD = 0;

        /**
         * EXType for {@link #getExemplarSet(int, int)}.
         * Corresponds to the 'auxiliary' CLDR exemplars in
         * <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
         *   http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
         * @stable ICU 3.4
         */
        public static readonly int ES_AUXILIARY = 1;

        /**
         * EXType for {@link #getExemplarSet(int, int)}.
         * Corresponds to the 'index' CLDR exemplars in
         * <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
         *   http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
         * @stable ICU 4.4
         */
        public static readonly int ES_INDEX = 2;

        /**
         * EXType for {@link #getExemplarSet(int, int)}.
         * Corresponds to the 'currencySymbol' CLDR exemplars in
         * <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
         *   http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
         * Note: This type is no longer supported.
         * @deprecated ICU 51
         */
        [Obsolete("ICU 51 This type is no longer supported.")]
        public static readonly int ES_CURRENCY = 3;

        /**
         * Corresponds to the 'punctuation' CLDR exemplars in
         * <a href="http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements">
         *   http://www.unicode.org/reports/tr35/tr35-general.html#Character_Elements</a>.
         * EXType for {@link #getExemplarSet(int, int)}.
         * @stable ICU 49
         */
        public static readonly int ES_PUNCTUATION = 4;

        /**
         * Count of EXTypes for {@link #getExemplarSet(int, int)}.
         * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        public static readonly int ES_COUNT = 5;

        /**
         * Delimiter type for {@link #getDelimiter(int)}.
         * @stable ICU 3.4
         */
        public static readonly int QUOTATION_START = 0;

        /**
         * Delimiter type for {@link #getDelimiter(int)}.
         * @stable ICU 3.4
         */
        public static readonly int QUOTATION_END = 1;

        /**
         * Delimiter type for {@link #getDelimiter(int)}.
         * @stable ICU 3.4
         */
        public static readonly int ALT_QUOTATION_START = 2;

        /**
         * Delimiter type for {@link #getDelimiter(int)}.
         * @stable ICU 3.4
         */
        public static readonly int ALT_QUOTATION_END = 3;

        /**
         * Count of delimiter types for {@link #getDelimiter(int)}.
         * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        public static readonly int DELIMITER_COUNT = 4;

        // private constructor to prevent default construction
        ///CLOVER:OFF
        private LocaleData() { }
        ///CLOVER:ON

        /**
         * Returns the set of exemplar characters for a locale. Equivalent to calling {@link #getExemplarSet(ULocale, int, int)} with
         * the extype == {@link #ES_STANDARD}.
         *
         * @param locale    Locale for which the exemplar character set
         *                  is to be retrieved.
         * @param options   Bitmask for options to apply to the exemplar pattern.
         *                  Specify zero to retrieve the exemplar set as it is
         *                  defined in the locale data.  Specify
         *                  UnicodeSet.CASE to retrieve a case-folded exemplar
         *                  set.  See {@link UnicodeSet#applyPattern(String,
         *                  int)} for a complete list of valid options.  The
         *                  IGNORE_SPACE bit is always set, regardless of the
         *                  value of 'options'.
         * @return          The set of exemplar characters for the given locale.
         * @stable ICU 3.0
         */
        public static UnicodeSet GetExemplarSet(ULocale locale, int options)
        {
            return LocaleData.GetInstance(locale).GetExemplarSet(options, ES_STANDARD);
        }

        /**
         * Returns the set of exemplar characters for a locale.
         * Equivalent to calling new LocaleData(locale).{@link #getExemplarSet(int, int)}.
         *
         * @param locale    Locale for which the exemplar character set
         *                  is to be retrieved.
         * @param options   Bitmask for options to apply to the exemplar pattern.
         *                  Specify zero to retrieve the exemplar set as it is
         *                  defined in the locale data.  Specify
         *                  UnicodeSet.CASE to retrieve a case-folded exemplar
         *                  set.  See {@link UnicodeSet#applyPattern(String,
         *                  int)} for a complete list of valid options.  The
         *                  IGNORE_SPACE bit is always set, regardless of the
         *                  value of 'options'.
         * @param extype    The type of exemplar character set to retrieve.
         * @return          The set of exemplar characters for the given locale.
         * @stable ICU 3.0
         */
        public static UnicodeSet GetExemplarSet(ULocale locale, int options, int extype)
        {
            return LocaleData.GetInstance(locale).GetExemplarSet(options, extype);
        }

        /**
         * Returns the set of exemplar characters for a locale.
         *
         * @param options   Bitmask for options to apply to the exemplar pattern.
         *                  Specify zero to retrieve the exemplar set as it is
         *                  defined in the locale data.  Specify
         *                  UnicodeSet.CASE to retrieve a case-folded exemplar
         *                  set.  See {@link UnicodeSet#applyPattern(String,
         *                  int)} for a complete list of valid options.  The
         *                  IGNORE_SPACE bit is always set, regardless of the
         *                  value of 'options'.
         * @param extype    The type of exemplar set to be retrieved,
         *                  ES_STANDARD, ES_INDEX, ES_AUXILIARY, or ES_PUNCTUATION
         * @return          The set of exemplar characters for the given locale.
         *                  If there is nothing available for the locale,
         *                  then null is returned if {@link #getNoSubstitute()} is true, otherwise the
         *                  root value is returned (which may be UnicodeSet.EMPTY).
         * @exception       RuntimeException if the extype is invalid.
         * @stable ICU 3.4
         */
        public UnicodeSet GetExemplarSet(int options, int extype)
        {
            string[] exemplarSetTypes = {
                    "ExemplarCharacters",
                    "AuxExemplarCharacters",
                    "ExemplarCharactersIndex",
                    "ExemplarCharactersCurrency",
                    "ExemplarCharactersPunctuation"
            };

            if (extype == ES_CURRENCY)
            {
                // currency symbol exemplar is no longer available
                return noSubstitute ? null : UnicodeSet.EMPTY;
            }

            try
            {
                string aKey = exemplarSetTypes[extype]; // will throw an out-of-bounds exception
                ICUResourceBundle stringBundle = (ICUResourceBundle)bundle.Get(aKey);

                if (noSubstitute && !bundle.IsRoot && stringBundle.IsRoot)
                {
                    return null;
                }
                String unicodeSetPattern = stringBundle.GetString();
                return new UnicodeSet(unicodeSetPattern, UnicodeSet.IGNORE_SPACE | options);
            }
            catch (IndexOutOfRangeException aiooe)
            {
                throw new ArgumentException(aiooe.Message, aiooe);
            }
            catch (Exception ex)
            {
                return noSubstitute ? null : UnicodeSet.EMPTY;
            }
        }

        /**
         * Gets the LocaleData object associated with the ULocale specified in locale
         *
         * @param locale    Locale with thich the locale data object is associated.
         * @return          A locale data object.
         * @stable ICU 3.4
         */
        public static LocaleData GetInstance(ULocale locale)
        {
            LocaleData ld = new LocaleData();
            ld.bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, locale);
            ld.langBundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_LANG_BASE_NAME, locale);
            ld.noSubstitute = false;
            return ld;
        }

        /**
         * Gets the LocaleData object associated with the default <code>FORMAT</code> locale
         *
         * @return          A locale data object.
         * @see Category#FORMAT
         * @stable ICU 3.4
         */
        public static LocaleData GetInstance()
        {
            return LocaleData.GetInstance(ULocale.GetDefault(Category.FORMAT));
        }

        ///**
        // * Sets the "no substitute" behavior of this locale data object.
        // *
        // * @param setting   Value for the no substitute behavior.  If TRUE,
        // *                  methods of this locale data object will return
        // *                  an error when no data is available for that method,
        // *                  given the locale ID supplied to the constructor.
        // * @stable ICU 3.4
        // */
        //public void SetNoSubstitute(bool setting)
        //{
        //    noSubstitute = setting;
        //}

        ///**
        // * Gets the "no substitute" behavior of this locale data object.
        // *
        // * @return          Value for the no substitute behavior.  If TRUE,
        // *                  methods of this locale data object will return
        // *                  an error when no data is available for that method,
        // *                  given the locale ID supplied to the constructor.
        // * @stable ICU 3.4
        // */
        //public bool GetNoSubstitute()
        //{
        //    return noSubstitute;
        //}

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

        /**
         * Retrieves a delimiter string from the locale data.
         *
         * @param type      The type of delimiter string desired.  Currently,
         *                  the valid choices are QUOTATION_START, QUOTATION_END,
         *                  ALT_QUOTATION_START, or ALT_QUOTATION_END.
         * @return          The desired delimiter string.
         * @stable ICU 3.4
         */
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

        /**
         * Utility for getMeasurementSystem and getPaperSize
         */
        private static UResourceBundle MeasurementTypeBundleForLocale(ULocale locale, String measurementType)
        {
            // Much of this is taken from getCalendarType in impl/CalendarUtil.java
            UResourceBundle measTypeBundle = null;
            string region = ULocale.GetRegionForSupplementalData(locale, true);
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
            /**
             * Measurement system specified by Le Syst&#x00E8;me International d'Unit&#x00E9;s (SI)
             * otherwise known as Metric system.
             * @stable ICU 2.8
             */
            public static readonly MeasurementSystem SI = new MeasurementSystem();

            /**
             * Measurement system followed in the United States of America.
             * @stable ICU 2.8
             */
            public static readonly MeasurementSystem US = new MeasurementSystem();

            /**
             * Mix of metric and imperial units used in Great Britain.
             * @stable ICU 55
             */
            public static readonly MeasurementSystem UK = new MeasurementSystem();

            private MeasurementSystem() { }
        }

        /**
         * Returns the measurement system used in the locale specified by the locale.
         *
         * @param locale      The locale for which the measurement system to be retrieved.
         * @return MeasurementSystem the measurement system used in the locale.
         * @stable ICU 3.0
         */
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

        /**
         * A class that represents the size of letter head
         * used in the country
         * @stable ICU 2.8
         */
        public sealed class PaperSize
        {
            private int height;
            private int width;

            internal PaperSize(int h, int w)
            {
                height = h;
                width = w;
            }
            /**
             * Retruns the height of the paper
             * @return the height
             * @stable ICU 2.8
             */
            public int Height
            {
                get { return height; }
            }
            /**
             * Returns the width of the paper
             * @return the width
             * @stable ICU 2.8
             */
            public int Width
            {
                get { return width; }
            }
        }

        /**
         * Returns the size of paper used in the locale. The paper sizes returned are always in
         * <em>milli-meters</em>.
         * @param locale The locale for which the measurement system to be retrieved.
         * @return The paper size used in the locale
         * @stable ICU 3.0
         */
        public static PaperSize GetPaperSize(ULocale locale)
        {
            UResourceBundle obj = MeasurementTypeBundleForLocale(locale, PAPER_SIZE);
            int[] size = obj.GetInt32Vector();
            return new PaperSize(size[0], size[1]);
        }

        /**
         * Returns LocaleDisplayPattern for this locale, e.g., {0}({1})
         * @return locale display pattern as a String.
         * @stable ICU 4.2
         */
        public string GetLocaleDisplayPattern()
        {
            ICUResourceBundle locDispBundle = (ICUResourceBundle)langBundle.Get(LOCALE_DISPLAY_PATTERN);
            string localeDisplayPattern = locDispBundle.GetStringWithFallback(PATTERN);
            return localeDisplayPattern;
        }

        /**
         * Returns LocaleDisplaySeparator for this locale.
         * @return locale display separator as a char.
         * @stable ICU 4.2
         */
        public string GetLocaleSeparator()
        {
            string sub0 = "{0}";
            string sub1 = "{1}";
            ICUResourceBundle locDispBundle = (ICUResourceBundle)langBundle.Get(LOCALE_DISPLAY_PATTERN);
            string localeSeparator = locDispBundle.GetStringWithFallback(SEPARATOR);
            int index0 = localeSeparator.IndexOf(sub0);
            int index1 = localeSeparator.IndexOf(sub1);
            if (index0 >= 0 && index1 >= 0 && index0 <= index1)
            {
                return localeSeparator.Substring(index0 + sub0.Length, index1 - (index0 + sub0.Length)); // ICU4N: Corrected 2nd parameter
            }
            return localeSeparator;
        }

        private static VersionInfo gCLDRVersion = null;

        /**
         * Returns the current CLDR version
         * @stable ICU 4.2
         */
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
