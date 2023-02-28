using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using static ICU4N.Numerics.Padder;
using static ICU4N.Text.PluralRules;
using Double = J2N.Numerics.Double;
using Number = J2N.Numerics.Number;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    // ICU4N specific
    /// <summary>
    /// Padding positions for <see cref="DecimalFormat.PadPosition"/>.
    /// </summary>
    public enum PadPosition
    {
        /// <summary>
        /// <icu/> Constant to specify pad characters inserted before the prefix.
        /// </summary>
        /// <seealso cref="DecimalFormat.PadPosition"/>
        /// <stable>ICU 2.0</stable>
        BeforePrefix,

        /// <summary>
        /// <icu/> Constant to specify pad characters inserted after the prefix.
        /// </summary>
        /// <seealso cref="DecimalFormat.PadPosition"/>
        /// <stable>ICU 2.0</stable>
        AfterPrefix,

        /// <summary>
        /// <icu/> Constant to specify pad characters inserted before the suffix.
        /// </summary>
        /// <seealso cref="DecimalFormat.PadPosition"/>
        /// <stable>ICU 2.0</stable>
        BeforeSuffix,

        /// <summary>
        /// <icu/> Constant to specify pad characters inserted after the suffix.
        /// </summary>
        /// <seealso cref="DecimalFormat.PadPosition"/>
        /// <stable>ICU 2.0</stable>
        AfterSuffix,
    }

    /// <summary>
    /// <see cref="DecimalFormat"/> is the primary
    /// concrete subclass of <see cref="NumberFormat"/>. It has a variety of features designed to make it
    /// possible to parse and format numbers in any locale, including support for Western, Arabic, or
    /// Indic digits. It supports different flavors of numbers, including integers ("123"), fixed-point
    /// numbers ("123.4"), scientific notation ("1.23E4"), percentages ("12%"), and currency amounts
    /// ("$123.00", "USD123.00", "123.00 US dollars"). All of these flavors can be easily localized.
    /// 
    /// <para/>To obtain a number formatter for a specific locale (including the default locale), call one of
    /// <see cref="NumberFormat"/>'s factory methods such as <see cref="NumberFormat.GetInstance()"/>. Do not call
    /// <see cref="DecimalFormat"/> constructors directly unless you know what you are doing.
    /// 
    /// <para/><see cref="DecimalFormat"/> aims to comply with the specification <a
    /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>. Read
    /// the specification for more information on how all the properties in <see cref="DecimalFormat"/> fit together.
    /// 
    /// <para/><strong>NOTE:</strong> Starting in ICU 60, there is a new set of APIs for localized number
    /// formatting that are designed to be an improvement over <see cref="DecimalFormat"/>.  New users are discouraged
    /// from using <see cref="DecimalFormat"/>.  For more information, see the package com.ibm.icu.number.
    /// 
    /// <para/>
    /// <h3>Example Usage</h3>
    /// <para/>
    /// Customize settings on a <see cref="DecimalFormat"/> instance from the <see cref="NumberFormat"/> factory:
    /// <code>
    /// NumberFormat f = NumberFormat.GetInstance(culture);
    /// if (f is DecimalFormat decimalFormat)
    /// {
    ///     decimalFormat.DecimalSeparatorAlwaysShown = true;
    ///     decimalFormat.MinimumGroupingDigits = true;
    /// }
    /// </code>
    /// 
    /// <para/>Quick and dirty print out a number using the localized number, currency, and percent format
    /// for each culture:
    /// <code>
    /// foreach (UCultureInfo culture in UCultureInfo.GetCultures(UCultureTypes.AllCultures))
    /// {
    ///     Console.Write(culture + ":\t");
    ///     Console.Write(NumberFormat.GetInstance(culture).Format(1.23));
    ///     Console.Write("\t");
    ///     Console.Write(NumberFormat.GetCurrencyInstance(culture).Format(1.23));
    ///     Console.Write("\t");
    ///     Console.Write(NumberFormat.GetPercentInstance(culture).Format(1.23));
    ///     Console.WriteLine();
    /// }
    /// </code>
    /// <para/>
    /// <h3>Properties and Symbols</h3>
    /// 
    /// <para/>A <see cref="DecimalFormat"/> object encapsulates a set of <em>properties</em> and a set of
    /// <em>symbols</em>. Grouping size, rounding mode, and affixes are examples of properties. Locale
    /// digits and the characters used for grouping and decimal separators are examples of symbols.
    /// 
    /// <para/>To set a custom set of symbols, use <see cref="SetDecimalFormatSymbols(DecimalFormatSymbols)"/>. Use the various other
    /// setters in this class to set custom values for the properties.
    /// 
    /// <h3>Rounding</h3>
    /// 
    /// <para/><see cref="DecimalFormat"/> provides three main strategies to specify the position at which numbers should
    /// be rounded:
    /// 
    /// <list type="number">
    ///     <item><description><strong>Magnitude:</strong> Display a fixed number of fraction digits; this is the most
    ///         common form.</description></item>
    ///     <item><description><strong>Increment:</strong> Round numbers to the closest multiple of a certain increment,
    ///         such as 0.05. This is common in currencies.</description></item>
    ///     <item><description><strong>Significant Digits:</strong> Round numbers such that a fixed number of nonzero
    ///         digits are shown. This is most common in scientific notation.</description></item>
    /// </list>
    /// 
    /// <para/>It is not possible to specify more than one rounding strategy. For example, setting a rounding
    /// increment in conjunction with significant digits results in undefined behavior.
    /// 
    /// <para/>It is also possible to specify the <em>rounding mode</em> to use. The default rounding mode is
    /// "half even", which rounds numbers to their closest increment, with ties broken in favor of
    /// trailing numbers being even. For more information, see <see cref="RoundingMode"/> and <a
    /// href="http://userguide.icu-project.org/formatparse/numbers/rounding-modes">the ICU User
    /// Guide</a>.
    /// 
    /// <para/>
    /// <h3>Pattern Strings</h3>
    /// 
    /// <para/>A <em>pattern string</em> is a way to serialize some of the available properties for decimal
    /// formatting. However, not all properties are capable of being serialized into a pattern string;
    /// see <see cref="ApplyPattern(string)"/> for more information.
    /// 
    /// <para/>Most users should not need to interface with pattern strings directly.
    /// 
    /// <para/>ICU DecimalFormat aims to follow the specification for pattern strings in <a
    /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>.
    /// Refer to that specification for more information on pattern string syntax.
    /// 
    /// <para/>
    /// <h4>Pattern String BNF</h4>
    /// 
    /// <para/>The following BNF is used when parsing the pattern string into property values:
    /// 
    /// <code>
    /// pattern    := subpattern (';' subpattern)?
    /// subpattern := prefix? number exponent? suffix?
    /// number     := (integer ('.' fraction)?) | sigDigits
    /// prefix     := '&#92;u0000'..'&#92;uFFFD' - specialCharacters
    /// suffix     := '&#92;u0000'..'&#92;uFFFD' - specialCharacters
    /// integer    := '#'* '0'* '0'
    /// fraction   := '0'* '#'*
    /// sigDigits  := '#'* '@' '@'* '#'*
    /// exponent   := 'E' '+'? '0'* '0'
    /// padSpec    := '*' padChar
    /// padChar    := '&#92;u0000'..'&#92;uFFFD' - quote
    /// &#32;
    /// Notation:
    ///   X*       0 or more instances of X
    ///   X?       0 or 1 instances of X
    ///   X|Y      either X or Y
    ///   C..D     any character from C up to D, inclusive
    ///   S-T      characters in S, except those in T
    /// </code>
    /// 
    /// <para/>The first subpattern is for positive numbers. The second (optional) subpattern is for negative
    /// numbers.
    /// 
    /// <para/>Not indicated in the BNF syntax above:
    /// 
    /// <list type="bullet">
    ///     <item><description>The grouping separator ',' can occur inside the integer and sigDigits elements, between any
    ///         two pattern characters of that element, as long as the integer or sigDigits element is not
    ///         followed by the exponent element.</description></item>
    ///     <item><description>Two grouping intervals are recognized: That between the decimal point and the first
    ///         grouping symbol, and that between the first and second grouping symbols. These intervals
    ///         are identical in most locales, but in some locales they differ. For example, the pattern
    ///         &quot;#,##,###&quot; formats the number 123456789 as &quot;12,34,56,789&quot;.</description></item>
    ///     <item><description>The pad specifier <c>padSpec</c> may appear before the prefix, after the prefix,
    ///         before the suffix, after the suffix, or not at all.</description></item>
    ///     <item><description>In place of '0', the digits '1' through '9' may be used to indicate a rounding increment.</description></item>
    /// </list>
    /// 
    /// <para/>
    /// <h3>Pattern Strings</h3>
    /// 
    /// <para/><see cref="DecimalFormat"/> aims to be able to parse anything that it can output as a formatted string.
    /// 
    /// <para/>There are two primary parse modes: <em>lenient</em> and <em>strict</em>. Lenient mode should
    /// be used if the goal is to parse user input to a number; strict mode should be used if the goal is
    /// validation. The default is lenient mode. For more information, see <see cref="ParseStrict"/>.
    /// 
    /// <para/><see cref="DecimalFormat"/> parses all Unicode characters that represent decimal digits, as
    /// defined by <see cref="UChar.Digit(int, int)"/>. In addition, <see cref="DecimalFormat"/> also recognizes as
    /// digits the ten consecutive characters starting with the localized zero digit defined in the
    /// <see cref="DecimalFormatSymbols"/> object. During formatting, the <see cref="DecimalFormatSymbols"/>-based
    /// digits are output.
    /// 
    /// <para/>Grouping separators are ignored in lenient mode (default). In strict mode, grouping separators
    /// must match the locale-specified grouping sizes.
    /// 
    /// <para/>When using <see cref="ParseCurrency"/>, all currencies are accepted, not just the currency
    /// currently set in the formatter. In addition, the formatter is able to parse every currency style
    /// format for a particular locale no matter which style the formatter is constructed with. For
    /// example, a formatter instance gotten from <see cref="NumberFormat.GetInstance(UCultureInfo, NumberFormatStyle)"/>
    /// with <see cref="NumberFormatStyle.CurrencyStyle"/> can parse both "USD1.00" and "3.00 US dollars".
    /// 
    /// <para/>Whitespace characters (lenient mode) and bidi control characters (lenient and strict mode),
    /// collectively called "ignorables", do not need to match in identity or quantity between the
    /// pattern string and the input string. For example, the pattern "# %" matches "35 %" (with a single
    /// space), "35%" (with no space), "35&amp;nbsp;%" (with a non-breaking space), and "35&amp;nbsp; %" (with
    /// multiple spaces). Arbitrary ignorables are also allowed at boundaries between the parts of the
    /// number: prefix, number, exponent separator, and suffix. Ignorable whitespace characters are those
    /// having the Unicode "blank" property for regular expressions, defined in UTS #18 Annex C, which is
    /// "horizontal" whitespace, like spaces and tabs, but not "vertical" whitespace, like line breaks.
    /// 
    /// <para/>If <see cref="Parse(string, ParsePosition)"/> fails to parse a string, it returns <c>null</c>
    /// and leaves the parse position unchanged. The convenience method <see cref="NumberFormat.Parse(string)"/> indicates
    /// parse failure by throwing a <see cref="FormatException"/>.
    /// 
    /// <para/>Under the hood, a state table parsing engine is used. To debug a parsing failure during
    /// development, use the following pattern to print details about the state table transitions:
    /// 
    /// <code>
    /// ICU4N.Numerics.Parser.DEBUGGING = true;
    /// df.Parse("123.45", ppos);
    /// ICU4N.Numerics.Parser.DEBUGGING = false;
    /// </code>
    /// 
    /// <para/>
    /// <h3>Thread Safety and Best Practices</h3>
    /// 
    /// <para/>Starting with ICU 59, instances of <see cref="DecimalFormat"/> are thread-safe.
    /// 
    /// <para/>Under the hood, <see cref="DecimalFormat"/> maintains an immutable formatter object that is rebuilt whenever
    /// any of the property setters are called. It is therefore best practice to call property setters
    /// only during construction and not when formatting numbers online.
    /// </summary>
    /// <seealso cref="UFormat"/>
    /// <seealso cref="NumberFormat"/>
    /// <stable>ICU 2.0</stable>
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        class DecimalFormat : NumberFormat
    {
        //// ICU4N TODO: Serialization
        ////** New serialization in ICU 59: declare different version from ICU 58. */
        ////private static final long serialVersionUID = 864413376551465018L;

        /// <summary>
        /// One non-transient field such that deserialization can determine the version of the class. This
        /// field has existed since the very earliest versions of DecimalFormat (in ICU4J).
        /// </summary>
        private readonly int serialVersionOnStream = 5;

        //=====================================================================================//
        //                                   INSTANCE FIELDS                                   //
        //=====================================================================================//

        // Fields are package-private, so that subclasses can use them.
        // properties should be final, but clone won't work if we make it final.
        // All fields are transient because custom serialization is used.

        /// <summary>
        /// The property bag corresponding to user-specified settings and settings from the pattern string.
        /// In principle this should be readonly, but serialize and clone won't work if it is readonly. Does not
        /// need to be volatile because the reference never changes.
        /// </summary>
        /* final */
        [NonSerialized]
        internal DecimalFormatProperties properties;

        /// <summary>
        /// The symbols for the current locale. Volatile because threads may read and write at the same
        /// time.
        /// </summary>
        [NonSerialized]
        internal volatile DecimalFormatSymbols symbols;

        /// <summary>
        /// The pre-computed formatter object. Setters cause this to be re-computed atomically. The
        /// <see cref="Format(long, StringBuffer, FieldPosition)"/>
        /// method uses the formatter directly without needing to synchronize. Volatile because
        /// threads may read and write at the same time.
        /// </summary>
        [NonSerialized]
        internal volatile LocalizedNumberFormatter formatter;

        /// <summary>
        /// The effective properties as exported from the formatter object. Volatile because threads may
        /// read and write at the same time.
        /// </summary>
        [NonSerialized]
        internal volatile DecimalFormatProperties exportedProperties;

        //=====================================================================================//
        //                                    CONSTRUCTORS                                     //
        //=====================================================================================//

        /// <summary>
        /// Creates a <see cref="DecimalFormat"/> based on the number pattern and symbols for <see cref="UCultureInfo.CurrentCulture"/>. This is
        /// a convenient way to obtain a <see cref="DecimalFormat"/> instance when internationalization is not the main
        /// concern.
        /// <para/>
        /// Most users should call the factory methods on <see cref="NumberFormat"/>, such as
        /// <see cref="NumberFormat.GetNumberInstance()"/>, which return localized formatter objects, instead of the
        /// <see cref="DecimalFormat"/> constructors.
        /// </summary>
        /// <seealso cref="NumberFormat.GetInstance()"/>
        /// <seealso cref="NumberFormat.GetNumberInstance()"/>
        /// <seealso cref="NumberFormat.GetCurrencyInstance()"/>
        /// <seealso cref="NumberFormat.GetPercentInstance()"/>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public DecimalFormat()
        {
            // Use the locale's default pattern
            UCultureInfo def = UCultureInfo.CurrentCulture;
            string pattern = GetPattern(def, NumberFormatStyle.NumberStyle);
            symbols = GetDefaultSymbols();
            properties = new DecimalFormatProperties();
            exportedProperties = new DecimalFormatProperties();
            // Regression: ignore pattern rounding information if the pattern has currency symbols.
            SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            RefreshFormatter();
        }

        /// <summary>
        /// Creates a <see cref="DecimalFormat"/> based on the given <paramref name="pattern"/>, using symbols for
        /// <see cref="UCultureInfo.CurrentCulture"/>. This
        /// is a convenient way to obtain a <see cref="DecimalFormat"/> instance when internationalization is not the
        /// main concern.
        /// <para/>
        /// Most users should call the factory methods on <see cref="NumberFormat"/>, such as
        /// <see cref="NumberFormat.GetNumberInstance()"/>, which return localized formatter objects, instead of the
        /// <see cref="DecimalFormat"/> constructors.
        /// </summary>
        /// <param name="pattern">A pattern string such as "#,##0.00" conforming to <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>.</param>
        /// <exception cref="FormatException"><paramref name="pattern"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        /// <seealso cref="NumberFormat.GetInstance()"/>
        /// <seealso cref="NumberFormat.GetNumberInstance()"/>
        /// <seealso cref="NumberFormat.GetCurrencyInstance()"/>
        /// <seealso cref="NumberFormat.GetPercentInstance()"/>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public DecimalFormat(string pattern)
        {
            symbols = GetDefaultSymbols();
            properties = new DecimalFormatProperties();
            exportedProperties = new DecimalFormatProperties();
            // Regression: ignore pattern rounding information if the pattern has currency symbols.
            SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            RefreshFormatter();
        }

        /// <summary>
        /// Creates a <see cref="DecimalFormat"/> based on the given <paramref name="pattern"/> and <paramref name="symbols"/>.
        /// Use this constructor if you want complete control over the behavior of the formatter.
        /// <para/>
        /// Most users should call the factory methods on <see cref="NumberFormat"/>, such as
        /// <see cref="NumberFormat.GetNumberInstance()"/>, which return localized formatter objects, instead of the
        /// <see cref="DecimalFormat"/> constructors.
        /// </summary>
        /// <param name="pattern">A pattern string such as "#,##0.00" conforming to <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>.</param>
        /// <param name="symbols">The set of symbols to be used.</param>
        /// <exception cref="FormatException"><paramref name="pattern"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> or
        /// <paramref name="symbols"/> is <c>null</c>.</exception>
        /// <seealso cref="NumberFormat.GetInstance()"/>
        /// <seealso cref="NumberFormat.GetNumberInstance()"/>
        /// <seealso cref="NumberFormat.GetCurrencyInstance()"/>
        /// <seealso cref="NumberFormat.GetPercentInstance()"/>
        /// <seealso cref="DecimalFormatSymbols"/>
        /// <stable>ICU 2.0</stable>
        public DecimalFormat(string pattern, DecimalFormatSymbols symbols)
        {
            if (symbols is null)
                throw new ArgumentNullException(nameof(symbols));

            this.symbols = (DecimalFormatSymbols)symbols.Clone();
            properties = new DecimalFormatProperties();
            exportedProperties = new DecimalFormatProperties();
            // Regression: ignore pattern rounding information if the pattern has currency symbols.
            SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            RefreshFormatter();
        }

        /// <summary>
        /// Creates a <see cref="DecimalFormat"/> based on the given <paramref name="pattern"/> and <paramref name="symbols"/>,
        /// with additional control over the behavior of currency. The style argument determines whether currency rounding rules should
        /// override the pattern, and the <see cref="CurrencyPluralInfo"/> object is used for customizing the
        /// plural forms used for currency long names.
        /// <para/>
        /// Most users should call the factory methods on <see cref="NumberFormat"/>, such as
        /// <see cref="NumberFormat.GetNumberInstance()"/>, which return localized formatter objects, instead of the
        /// <see cref="DecimalFormat"/> constructors.
        /// </summary>
        /// <param name="pattern">A non-localized pattern string.</param>
        /// <param name="symbols">The set of symbols to be used.</param>
        /// <param name="infoInput">The information used for currency plural format, including currency plural
        /// patterns and plural rules.</param>
        /// <param name="style">The decimal formatting style.</param>
        /// <exception cref="FormatException"><paramref name="pattern"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/>,
        /// <paramref name="symbols"/>, or <paramref name="infoInput"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.2</stable>
        public DecimalFormat(
            string pattern, DecimalFormatSymbols symbols, CurrencyPluralInfo infoInput, NumberFormatStyle style)
            : this(pattern, symbols, style)
        {
            properties.CurrencyPluralInfo = infoInput ?? throw new ArgumentNullException(nameof(infoInput));
            RefreshFormatter();
        }

        /// <summary>
        /// Internal constructor used by <see cref="NumberFormat"/>.
        /// </summary>
        /// <param name="pattern">A non-localized pattern string.</param>
        /// <param name="symbols">The set of symbols to be used.</param>
        /// <param name="choice">The decimal formatting style.</param>
        /// <exception cref="FormatException"><paramref name="pattern"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/>,
        /// or <paramref name="symbols"/> is <c>null</c>.</exception>
        internal DecimalFormat(string pattern, DecimalFormatSymbols symbols, NumberFormatStyle choice)
        {
            // ICU4N: Added guard clauses
            if (pattern is null)
                throw new ArgumentNullException(nameof(pattern));
            if (symbols is null)
                throw new ArgumentNullException(nameof(symbols));

            this.symbols = (DecimalFormatSymbols)symbols.Clone();
            properties = new DecimalFormatProperties();
            exportedProperties = new DecimalFormatProperties();
            // If choice is a currency type, ignore the rounding information.
            if (choice == NumberFormatStyle.CurrencyStyle
                || choice == NumberFormatStyle.ISOCurrencyStyle
                || choice == NumberFormatStyle.AccountingCurrencyStyle
                || choice == NumberFormatStyle.CashCurrencyStyle
                || choice == NumberFormatStyle.StandardCurrencyStyle
                || choice == NumberFormatStyle.PluralCurrencyStyle)
            {
                SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_ALWAYS);
            }
            else
            {
                SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            }
            RefreshFormatter();
        }

        private static DecimalFormatSymbols GetDefaultSymbols()
        {
            return DecimalFormatSymbols.GetInstance();
        }

        /// <summary>
        /// Parses the given pattern string and overwrites the settings specified in the pattern string.
        /// The properties corresponding to the following setters are overwritten, either with their
        /// default values or with the value specified in the pattern string:
        /// 
        /// <list type="bullet">
        ///     <item><description><see cref="DecimalSeparatorAlwaysShown"/></description></item>
        ///     <item><description><see cref="ExponentSignAlwaysShown"/></description></item>
        ///     <item><description><see cref="FormatWidth"/></description></item>
        ///     <item><description><see cref="GroupingSize"/></description></item>
        ///     <item><description><see cref="Multiplier"/>  (percent/permille)</description></item>
        ///     <item><description><see cref="MaximumFractionDigits"/></description></item>
        ///     <item><description><see cref="MaximumIntegerDigits"/></description></item>
        ///     <item><description><see cref="MaximumSignificantDigits"/></description></item>
        ///     <item><description><see cref="MinimumExponentDigits"/></description></item>
        ///     <item><description><see cref="MinimumFractionDigits"/></description></item>
        ///     <item><description><see cref="MinimumIntegerDigits"/></description></item>
        ///     <item><description><see cref="MinimumSignificantDigits"/></description></item>
        ///     <item><description><see cref="PadPosition"/></description></item>
        ///     <item><description><see cref="PadCharacter"/></description></item>
        ///     <item><description><see cref="RoundingIncrement"/></description></item>
        ///     <item><description><see cref="SecondaryGroupingSize"/></description></item>
        /// </list>
        /// All other settings remain untouched.
        /// 
        /// <para/>For more information on pattern strings, see <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <exception cref="FormatException">The <paramref name="pattern"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        /// <stable>ICU 2.0</stable>
        public virtual void ApplyPattern(string pattern)
        {
            lock (this) // ICU4N TODO: Create specialized lock object - note this is shared with NumberFormat
            {
                SetPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_NEVER);
                // Backwards compatibility: clear out user-specified prefix and suffix,
                // as well as CurrencyPluralInfo.
                properties.PositivePrefix = null;
                properties.NegativePrefix = null;
                properties.PositiveSuffix = null;
                properties.NegativeSuffix = null;
                properties.CurrencyPluralInfo = null;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Converts the given string to standard notation and then parses it using <see cref="ApplyPattern(string)"/>.
        /// This method is provided for backwards compatibility and should not be used in new projects.
        /// 
        /// <para/>Localized notation means that instead of using generic placeholders in the pattern, you use
        /// the corresponding locale-specific characters instead. For example, in locale <em>fr-FR</em>,
        /// the period in the pattern "0.000" means "decimal" in standard notation (as it does in every
        /// other locale), but it means "grouping" in localized notation.
        /// </summary>
        /// <param name="localizedPattern">The pattern string in localized notation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="localizedPattern"/> is <c>null</c>.</exception>
        /// <stable>ICU 2.0</stable>
        public virtual void ApplyLocalizedPattern(string localizedPattern)
        {
            lock (this) // ICU4N TODO: Create specialized lock object - note this is shared with NumberFormat
            {
                string pattern = PatternStringUtils.ConvertLocalized(localizedPattern, symbols, false);
                ApplyPattern(pattern);
            }
        }

        //=====================================================================================//
        //                                CLONE AND SERIALIZE                                  //
        //=====================================================================================//

        /// <stable>ICU 2.0</stable>
        public override object Clone()
        {
            DecimalFormat other = (DecimalFormat)base.Clone();
            other.symbols = (DecimalFormatSymbols)symbols.Clone();
            other.properties = (DecimalFormatProperties)properties.Clone();
            other.exportedProperties = new DecimalFormatProperties();
            other.RefreshFormatter();
            return other;
        }

        // ICU4N TODO: Serialization

        //        /**
        //         * Custom serialization: save property bag and symbols; the formatter object can be re-created
        //         * from just that amount of information.
        //         */
        //        private synchronized void writeObject(ObjectOutputStream oos) // throws IOException
        //        {
        //            // ICU 59 custom serialization.
        //            // Write class metadata and serialVersionOnStream field:
        //            oos.defaultWriteObject();
        //            // Extra int for possible future use:
        //            oos.writeInt(0);
        //            // 1) Property Bag
        //            oos.writeObject(properties);
        //            // 2) DecimalFormatSymbols
        //            oos.writeObject(symbols);
        //          }

        //    /**
        //     * Custom serialization: re-create object from serialized property bag and symbols. Also supports
        //     * reading from the legacy (pre-ICU4J 59) format and converting it to the new form.
        //     */
        //    private void readObject(ObjectInputStream ois) throws IOException, ClassNotFoundException {
        //    ObjectInputStream.GetField fieldGetter = ois.readFields();
        //    ObjectStreamField[] serializedFields = fieldGetter.getObjectStreamClass().getFields();
        //    int serialVersion = fieldGetter.get("serialVersionOnStream", -1);

        //    if (serialVersion > 5) {
        //      throw new IOException(
        //          "Cannot deserialize newer com.ibm.icu.text.DecimalFormat (v" + serialVersion + ")");
        //} else if (serialVersion == 5)
        //{
        //    ///// ICU 59+ SERIALIZATION FORMAT /////
        //    // We expect this field and no other fields:
        //    if (serializedFields.length > 1)
        //    {
        //        throw new IOException("Too many fields when reading serial version 5");
        //    }
        //    // Extra int for possible future use:
        //    ois.readInt();
        //    // 1) Property Bag
        //    Object serializedProperties = ois.readObject();
        //    if (serializedProperties instanceof DecimalFormatProperties) {
        //        // ICU 60+
        //        properties = (DecimalFormatProperties)serializedProperties;
        //    } else
        //    {
        //        // ICU 59
        //        properties = ((com.ibm.icu.impl.number.Properties)serializedProperties).getInstance();
        //    }
        //    // 2) DecimalFormatSymbols
        //    symbols = (DecimalFormatSymbols)ois.readObject();
        //    // Re-build transient fields
        //    exportedProperties = new DecimalFormatProperties();
        //    RefreshFormatter();
        //}
        //else
        //{
        //    ///// LEGACY SERIALIZATION FORMAT /////
        //    properties = new DecimalFormatProperties();
        //    // Loop through the fields. Not all fields necessarily exist in the serialization.
        //    String pp = null, ppp = null, ps = null, psp = null;
        //    String np = null, npp = null, ns = null, nsp = null;
        //    for (ObjectStreamField field : serializedFields)
        //    {
        //        String name = field.getName();
        //        if (name.equals("decimalSeparatorAlwaysShown"))
        //        {
        //            setDecimalSeparatorAlwaysShown(fieldGetter.get("decimalSeparatorAlwaysShown", false));
        //        }
        //        else if (name.equals("exponentSignAlwaysShown"))
        //        {
        //            setExponentSignAlwaysShown(fieldGetter.get("exponentSignAlwaysShown", false));
        //        }
        //        else if (name.equals("formatWidth"))
        //        {
        //            setFormatWidth(fieldGetter.get("formatWidth", 0));
        //        }
        //        else if (name.equals("groupingSize"))
        //        {
        //            setGroupingSize(fieldGetter.get("groupingSize", (byte)3));
        //        }
        //        else if (name.equals("groupingSize2"))
        //        {
        //            setSecondaryGroupingSize(fieldGetter.get("groupingSize2", (byte)0));
        //        }
        //        else if (name.equals("maxSignificantDigits"))
        //        {
        //            setMaximumSignificantDigits(fieldGetter.get("maxSignificantDigits", 6));
        //        }
        //        else if (name.equals("minExponentDigits"))
        //        {
        //            setMinimumExponentDigits(fieldGetter.get("minExponentDigits", (byte)0));
        //        }
        //        else if (name.equals("minSignificantDigits"))
        //        {
        //            setMinimumSignificantDigits(fieldGetter.get("minSignificantDigits", 1));
        //        }
        //        else if (name.equals("multiplier"))
        //        {
        //            setMultiplier(fieldGetter.get("multiplier", 1));
        //        }
        //        else if (name.equals("pad"))
        //        {
        //            setPadCharacter(fieldGetter.get("pad", '\u0020'));
        //        }
        //        else if (name.equals("padPosition"))
        //        {
        //            setPadPosition(fieldGetter.get("padPosition", 0));
        //        }
        //        else if (name.equals("parseBigDecimal"))
        //        {
        //            setParseBigDecimal(fieldGetter.get("parseBigDecimal", false));
        //        }
        //        else if (name.equals("parseRequireDecimalPoint"))
        //        {
        //            setDecimalPatternMatchRequired(fieldGetter.get("parseRequireDecimalPoint", false));
        //        }
        //        else if (name.equals("roundingMode"))
        //        {
        //            setRoundingMode(fieldGetter.get("roundingMode", 0));
        //        }
        //        else if (name.equals("useExponentialNotation"))
        //        {
        //            setScientificNotation(fieldGetter.get("useExponentialNotation", false));
        //        }
        //        else if (name.equals("useSignificantDigits"))
        //        {
        //            setSignificantDigitsUsed(fieldGetter.get("useSignificantDigits", false));
        //        }
        //        else if (name.equals("currencyPluralInfo"))
        //        {
        //            setCurrencyPluralInfo((CurrencyPluralInfo)fieldGetter.get("currencyPluralInfo", null));
        //        }
        //        else if (name.equals("mathContext"))
        //        {
        //            setMathContextICU((MathContext)fieldGetter.get("mathContext", null));
        //        }
        //        else if (name.equals("negPrefixPattern"))
        //        {
        //            npp = (String)fieldGetter.get("negPrefixPattern", null);
        //        }
        //        else if (name.equals("negSuffixPattern"))
        //        {
        //            nsp = (String)fieldGetter.get("negSuffixPattern", null);
        //        }
        //        else if (name.equals("negativePrefix"))
        //        {
        //            np = (String)fieldGetter.get("negativePrefix", null);
        //        }
        //        else if (name.equals("negativeSuffix"))
        //        {
        //            ns = (String)fieldGetter.get("negativeSuffix", null);
        //        }
        //        else if (name.equals("posPrefixPattern"))
        //        {
        //            ppp = (String)fieldGetter.get("posPrefixPattern", null);
        //        }
        //        else if (name.equals("posSuffixPattern"))
        //        {
        //            psp = (String)fieldGetter.get("posSuffixPattern", null);
        //        }
        //        else if (name.equals("positivePrefix"))
        //        {
        //            pp = (String)fieldGetter.get("positivePrefix", null);
        //        }
        //        else if (name.equals("positiveSuffix"))
        //        {
        //            ps = (String)fieldGetter.get("positiveSuffix", null);
        //        }
        //        else if (name.equals("roundingIncrement"))
        //        {
        //            setRoundingIncrement((java.math.BigDecimal)fieldGetter.get("roundingIncrement", null));
        //        }
        //        else if (name.equals("symbols"))
        //        {
        //            setDecimalFormatSymbols((DecimalFormatSymbols)fieldGetter.get("symbols", null));
        //        }
        //        else
        //        {
        //            // The following fields are ignored:
        //            // "PARSE_MAX_EXPONENT"
        //            // "currencySignCount"
        //            // "style"
        //            // "attributes"
        //            // "currencyChoice"
        //            // "formatPattern"
        //            // "currencyUsage" => ignore this because the old code puts currencyUsage directly into min/max fraction.
        //        }
        //    }
        //    // Resolve affixes
        //    if (npp == null)
        //    {
        //        properties.setNegativePrefix(np);
        //    }
        //    else
        //    {
        //        properties.setNegativePrefixPattern(npp);
        //    }
        //    if (nsp == null)
        //    {
        //        properties.setNegativeSuffix(ns);
        //    }
        //    else
        //    {
        //        properties.setNegativeSuffixPattern(nsp);
        //    }
        //    if (ppp == null)
        //    {
        //        properties.setPositivePrefix(pp);
        //    }
        //    else
        //    {
        //        properties.setPositivePrefixPattern(ppp);
        //    }
        //    if (psp == null)
        //    {
        //        properties.setPositiveSuffix(ps);
        //    }
        //    else
        //    {
        //        properties.setPositiveSuffixPattern(psp);
        //    }
        //    // Extract values from parent NumberFormat class.  Have to use reflection here.
        //    java.lang.reflect.Field getter;
        //    try
        //    {
        //        getter = NumberFormat.class.getDeclaredField("groupingUsed");
        //getter.setAccessible(true);
        //setGroupingUsed((Boolean)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("parseIntegerOnly");
        //getter.setAccessible(true);
        //setParseIntegerOnly((Boolean)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("maximumIntegerDigits");
        //getter.setAccessible(true);
        //setMaximumIntegerDigits((Integer)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("minimumIntegerDigits");
        //getter.setAccessible(true);
        //setMinimumIntegerDigits((Integer)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("maximumFractionDigits");
        //getter.setAccessible(true);
        //setMaximumFractionDigits((Integer)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("minimumFractionDigits");
        //getter.setAccessible(true);
        //setMinimumFractionDigits((Integer)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("currency");
        //getter.setAccessible(true);
        //setCurrency((Currency)getter.get(this));
        //getter = NumberFormat.class.getDeclaredField("parseStrict");
        //getter.setAccessible(true);
        //setParseStrict((Boolean)getter.get(this));
        //      } catch (IllegalArgumentException e)
        //{
        //    throw new IOException(e);
        //}
        //catch (IllegalAccessException e)
        //{
        //    throw new IOException(e);
        //}
        //catch (NoSuchFieldException e)
        //{
        //    throw new IOException(e);
        //}
        //catch (SecurityException e)
        //{
        //    throw new IOException(e);
        //}
        //// Finish initialization
        //if (symbols == null)
        //{
        //    symbols = getDefaultSymbols();
        //}
        //exportedProperties = new DecimalFormatProperties();
        //RefreshFormatter();
        //    }
        //  }

        //=====================================================================================//
        //                               FORMAT AND PARSE APIS                                 //
        //=====================================================================================//

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(double number, StringBuffer result, FieldPosition fieldPosition) // ICU4N TODO: API - Replace FieldPosition with ReadOnlySpan<char> ?
        {
            FormattedNumber output = formatter.Format(number);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(fieldPosition, result.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(result);
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(long number, StringBuffer result, FieldPosition fieldPosition)
        {
            FormattedNumber output = formatter.Format(number);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(fieldPosition, result.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(result);
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(Numerics.BigMath.BigInteger number, StringBuffer result, FieldPosition fieldPosition)
        {
            FormattedNumber output = formatter.Format(number);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(fieldPosition, result.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(result);
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(
            Numerics.BigMath.BigDecimal number, StringBuffer result, FieldPosition fieldPosition)
        {
            FormattedNumber output = formatter.Format(number);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(fieldPosition, result.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(result);
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(BigDecimal number, StringBuffer result, FieldPosition fieldPosition)
        {
            FormattedNumber output = formatter.Format(number);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(fieldPosition, result.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(result);
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 3.6</stable>
        public override AttributedCharacterIterator FormatToCharacterIterator(object obj)
        {
            if (!(obj is Number number)) throw new ArgumentException();
            FormattedNumber output = formatter.Format(number);
            return output.GetFieldIterator();
        }

        /// <inheritdoc/>
        /// <stable>ICU 3.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            override StringBuffer Format(CurrencyAmount currAmt, StringBuffer toAppendTo, FieldPosition pos)
        {
            FormattedNumber output = formatter.Format(currAmt);
#pragma warning disable CS0618 // Type or member is obsolete
            output.PopulateFieldPosition(pos, toAppendTo.Length);
#pragma warning restore CS0618 // Type or member is obsolete
            output.AppendTo(toAppendTo);
            return toAppendTo;
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
        public override Number Parse(string text, ParsePosition parsePosition) // ICU4N TODO: API - This should be refactored into TryParse.
        {
            DecimalFormatProperties pprops = threadLocalProperties.Value;
            lock (this)
            {
                pprops.CopyFrom(properties);
            }
            // Backwards compatibility: use currency parse mode if this is a currency instance
            Number result = Parser.Parse(text, parsePosition, pprops, symbols);
            // Backwards compatibility: return com.ibm.icu.math.BigDecimal
            if (result is Numerics.BigMath.BigDecimal bigDecimal)
            {
                result = SafeConvertBigDecimal(bigDecimal);
            }
            return result;
        }

        /// <inheritdoc/>
        /// <stable>ICU 49</stable>
#if FEATURE_LEGACY_NUMBER_FORMAT
        internal
#else
        public
#endif
            override CurrencyAmount ParseCurrency(string text, ParsePosition parsePosition) // ICU4N: Changed ICharSequence to string
        {
            try
            {
                DecimalFormatProperties pprops = threadLocalProperties.Value;
                lock (this)
                {
                    pprops.CopyFrom(properties);
                }
                CurrencyAmount result = Parser.ParseCurrency(text, parsePosition, pprops, symbols);
                if (result == null) return null;
                Number number = result.Number;
                // Backwards compatibility: return com.ibm.icu.math.BigDecimal
                if (number is Numerics.BigMath.BigDecimal bigDecimal)
                {
                    number = SafeConvertBigDecimal(bigDecimal);
                    result = new CurrencyAmount(number, result.Currency);
                }
                return result;
            }
            catch (FormatException) // ICU4N TODO: Factor out the need for this catch by making the above calls safe. Note this was ParseException in ICU4J.
            {
                return null;
            }
        }

        //=====================================================================================//
        //                                GETTERS AND SETTERS                                  //
        //=====================================================================================//

        /// <summary>
        /// Returns a copy of the decimal format symbols used by this formatter.
        /// </summary>
        /// <returns>The desired <see cref="DecimalFormatSymbols"/>.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual DecimalFormatSymbols GetDecimalFormatSymbols() // ICU4N TODO: API Make into property?
        {
            lock (this)
            {
                return (DecimalFormatSymbols)symbols.Clone();
            }
        }

        /// <summary>
        /// Sets the decimal format symbols used by this formatter. The formatter uses a copy of the
        /// provided symbols.
        /// </summary>
        /// <param name="newSymbols">The desired <see cref="DecimalFormatSymbols"/>.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols) // ICU4N TODO: API Make into property?
        {
            lock (this)
            {
                symbols = (DecimalFormatSymbols)newSymbols.Clone();
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <strong>Affixes:</strong> Gets the positive prefix string currently being used to format
        /// numbers.
        /// 
        /// <para/>If the affix was specified via the pattern, the string returned by this method will have
        /// locale symbols substituted in place of special characters according to the LDML specification.
        /// If the affix was specified via <see cref="PositivePrefix"/>, the string will be returned
        /// literally.
        /// 
        /// <para/>Using the setter overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this property will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        public virtual string PositivePrefix
        {
            get
            {
                lock (this)
#pragma warning disable CS0618 // Type or member is obsolete
                    return formatter.Format(1).GetPrefix();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            set => SetPositivePrefix(value);
        }

        /// <summary>
        /// <strong>Affixes:</strong> Sets the string to prepend to positive numbers. For example, if you
        /// set the value "#", then the number 123 will be formatted as "#123" in the locale
        /// <em>en-US</em>.
        /// 
        /// <para/>Using this method overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this method will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <param name="prefix">The literal string to prepend to positive numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        private void SetPositivePrefix(string prefix)
        {
            if (prefix is null)
                throw new ArgumentNullException(nameof(prefix));

            lock (this)
            {
                properties.PositivePrefix = prefix;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <strong>Affixes:</strong> Gets or sets the negative prefix string currently being used to format
        /// numbers. For example, if you set the value "#", then the number -123 will be formatted as "#123"
        /// in the locale <em>en-US</em> (overriding the implicit default '-' in the pattern).
        /// 
        /// <para/>If the affix was specified via the pattern, the string returned by this method will have
        /// locale symbols substituted in place of special characters according to the LDML specification.
        /// If the affix was specified via <see cref="NegativePrefix"/>, the string will be returned
        /// literally.
        /// 
        /// <para/>Using the setter overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this property will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        public virtual string NegativePrefix
        {
            get
            {
                lock (this)
#pragma warning disable CS0618 // Type or member is obsolete
                    return formatter.Format(-1).GetPrefix();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            set => SetNegativePrefix(value);
        }

        /// <summary>
        /// <strong>Affixes:</strong> Sets the string to prepend to negative numbers. For example, if you
        /// set the value "#", then the number -123 will be formatted as "#123" in the locale
        /// <em>en-US</em> (overriding the implicit default '-' in the pattern).
        /// 
        /// <para/>Using this method overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this method will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <param name="prefix">The literal string to prepend to negative numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        private void SetNegativePrefix(string prefix)
        {
            if (prefix is null)
                throw new ArgumentNullException(nameof(prefix));

            lock (this)
            {
                properties.NegativePrefix = prefix;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <strong>Affixes:</strong> Gets or sets the positive suffix string currently being used to format
        /// numbers. For example, if you set the value "#", then the number 123 will be formatted as "123#"
        /// in the locale <em>en-US</em>.
        /// 
        /// <para/>If the affix was specified via the pattern, the string returned by this method will have
        /// locale symbols substituted in place of special characters according to the LDML specification.
        /// If the affix was specified via <see cref="PositiveSuffix"/>, the string will be returned
        /// literally.
        /// 
        /// <para/>Using the setter overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this property will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        public virtual string PositiveSuffix
        {
            get
            {
                lock (this)
#pragma warning disable CS0618 // Type or member is obsolete
                    return formatter.Format(1).GetSuffix();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            set => SetPositiveSuffix(value);
        }

        /// <summary>
        /// <strong>Affixes:</strong> Sets the string to append to positive numbers. For example, if you
        /// set the value "#", then the number 123 will be formatted as "123#" in the locale
        /// <em>en-US</em>.
        /// 
        /// <para/>Using this method overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this method will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <param name="suffix">The literal string to append to positive numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="suffix"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        private void SetPositiveSuffix(string suffix)
        {
            if (suffix is null)
                throw new ArgumentNullException(nameof(suffix));

            lock (this)
            {
                properties.PositiveSuffix = suffix;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <strong>Affixes:</strong> Gets or sets the negative suffix string currently being used to format
        /// numbers.
        /// 
        /// <para/>If the affix was specified via the pattern, the string returned by this method will have
        /// locale symbols substituted in place of special characters according to the LDML specification.
        /// If the affix was specified via <see cref="NegativeSuffix"/>, the string will be returned
        /// literally.
        /// 
        /// <para/>Using the setter overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this property will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        public virtual string NegativeSuffix
        {
            get
            {
                lock (this)
#pragma warning disable CS0618 // Type or member is obsolete
                    return formatter.Format(-1).GetSuffix();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            set => SetNegativeSuffix(value);
        }

        /// <summary>
        /// <strong>Affixes:</strong> Sets the string to append to negative numbers. For example, if you
        /// set the value "#", then the number 123 will be formatted as "123#" in the locale
        /// <em>en-US</em>.
        /// 
        /// <para/>Using this method overrides the affix specified via the pattern, and unlike the pattern, the
        /// string given to this method will be interpreted literally WITHOUT locale symbol substitutions.
        /// </summary>
        /// <param name="suffix">The literal string to append to negative numbers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="suffix"/> is <c>null</c>.</exception>
        /// <category>Affixes</category>
        /// <stable>ICU 2.0</stable>
        private void SetNegativeSuffix(string suffix)
        {
            if (suffix is null)
                throw new ArgumentNullException(nameof(suffix));
            lock (this)
            {
                properties.NegativeSuffix = suffix;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether the sign ('+' in <em>en</em>) is being shown on positive numbers.
        /// The rules in UTS #35 section 3.2.1 will be followed to ensure a locale-aware placement of the sign.
        /// 
        /// <para/>More specifically, the following strategy will be used to place the plus sign:
        /// 
        /// <list type="number">
        ///     <item>
        ///         <term><em>Patterns without a negative subpattern:</em></term>
        ///         <description>The locale's plus sign will be prepended
        ///         to the positive prefix.</description>
        ///     </item>
        ///     <item>
        ///         <term><em>Patterns with a negative subpattern without a '-' sign (e.g., accounting):</em></term>
        ///         <description>The locale's plus sign will be prepended to the positive prefix, as in case 1.</description>
        ///     </item>
        ///     <item>
        ///         <term><em>Patterns with a negative subpattern that has a '-' sign:</em></term>
        ///         <description>The locale's plus sign will substitute the '-' in the negative subpattern.
        ///         The positive subpattern will be unused.</description>
        ///     </item>
        /// </list>
        /// 
        /// <para/>
        /// This property setter is designed to be used <em>instead of</em> applying a pattern containing an
        /// explicit plus sign, such as "+0;-0". The behavior when combining this setter with explicit plus
        /// signs in the pattern is undefined.
        /// </summary>
        /// <category>Affixes</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        public virtual bool SignAlwaysShown
        {
            get
            {
                lock (this)
                    // This is not in the exported properties
                    return properties.SignAlwaysShown;
            }
            set => SetSignAlwaysShown(value);
        }

        /// <summary>
        /// Sets whether to always shown the plus sign ('+' in <em>en</em>) on positive numbers. The rules
        /// in UTS #35 section 3.2.1 will be followed to ensure a locale-aware placement of the sign.
        /// 
        /// <para/>More specifically, the following strategy will be used to place the plus sign:
        /// 
        /// <list type="number">
        ///     <item>
        ///         <term><em>Patterns without a negative subpattern:</em></term>
        ///         <description>The locale's plus sign will be prepended
        ///         to the positive prefix.</description>
        ///     </item>
        ///     <item>
        ///         <term><em>Patterns with a negative subpattern without a '-' sign (e.g., accounting):</em></term>
        ///         <description>The locale's plus sign will be prepended to the positive prefix, as in case 1.</description>
        ///     </item>
        ///     <item>
        ///         <term><em>Patterns with a negative subpattern that has a '-' sign:</em></term>
        ///         <description>The locale's plus sign will substitute the '-' in the negative subpattern.
        ///         The positive subpattern will be unused.</description>
        ///     </item>
        /// </list>
        /// 
        /// <para/>
        /// This method is designed to be used <em>instead of</em> applying a pattern containing an
        /// explicit plus sign, such as "+0;-0". The behavior when combining this method with explicit plus
        /// signs in the pattern is undefined.
        /// </summary>
        /// <param name="value"><c>true</c> to always show a sign; <c>false</c> to hide the sign on positive numbers.</param>
        /// <category>Affixes</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        private void SetSignAlwaysShown(bool value)
        {
            lock (this)
            {
                properties.SignAlwaysShown = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the multiplier being applied to numbers before they are formatted. For example, when
        /// formatting percents, a multiplier of 100 can be used.
        /// 
        /// <para/>If a percent or permille sign is specified in the pattern, the multiplier is automatically
        /// set to 100 or 1000, respectively.
        /// 
        /// <para/>If the number specified here is a power of 10, a more efficient code path will be used.
        /// </summary>
        /// <exception cref="ArgumentException">Setter <paramref name="value"/> is zero.</exception>
        /// <category>Multipliers</category>
        /// <stable>ICU 2.0</stable>
        public virtual int Multiplier
        {
            get
            {
                lock (this)
                {
                    if (properties.Multiplier != null)
                    {
                        return properties.Multiplier.ToInt32();
                    }
                    else
                    {
                        return (int)Math.Pow(10, properties.MagnitudeMultiplier);
                    }
                }
            }
            set => SetMultiplier(value);
        }

        /// <summary>
        /// Sets a number that will be used to multiply all numbers prior to formatting. For example, when
        /// formatting percents, a multiplier of 100 can be used.
        /// 
        /// <para/>If a percent or permille sign is specified in the pattern, the multiplier is automatically
        /// set to 100 or 1000, respectively.
        /// 
        /// <para/>If the number specified here is a power of 10, a more efficient code path will be used.
        /// </summary>
        /// <param name="multiplier">The number by which all numbers passed to Format() overloads will be multiplied.</param>
        /// <exception cref="ArgumentException"><paramref name="multiplier"/> is zero.</exception>
        /// <category>Multipliers</category>
        /// <stable>ICU 2.0</stable>
        private void SetMultiplier(int multiplier)
        {
            if (multiplier == 0)
                throw new ArgumentException("Multiplier must be nonzero.");

            lock (this)
            {
                // Try to convert to a magnitude multiplier first
                int delta = 0;
                int value = multiplier;
                while (multiplier != 1)
                {
                    delta++;
                    int temp = value / 10;
                    if (temp * 10 != value)
                    {
                        delta = -1;
                        break;
                    }
                    value = temp;
                }
                if (delta != -1)
                {
                    properties.MagnitudeMultiplier = delta;
                }
                else
                {
                    properties.Multiplier = multiplier;
                }
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets the increment to which numbers are being rounded.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual Numerics.BigMath.BigDecimal RoundingIncrement
        {
            get
            {
                lock (this)
                    return exportedProperties.RoundingIncrement;
            }
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Sets an increment, or interval, to which
        /// numbers are rounded. For example, a rounding increment of 0.05 will cause the number 1.23 to be
        /// rounded to 1.25 in the default rounding mode.
        /// 
        /// <para/>The rounding increment can be specified via the pattern string: for example, the pattern
        /// "#,##0.05" encodes a rounding increment of 0.05.
        /// 
        /// <para/>The rounding increment is applied <em>after</em> any multipliers might take effect; for
        /// example, in scientific notation or when <see cref="Multiplier"/> is set.
        /// 
        /// <para/>See <see cref="MaximumFractionDigits"/> and <see cref="MaximumSignificantDigits"/> for two other
        /// ways of specifying rounding strategies.
        /// </summary>
        /// <param name="increment">The increment to which numbers are to be rounded.</param>
        /// <seealso cref="MaximumFractionDigits"/>
        /// <seealso cref="MaximumSignificantDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual void SetRoundingIncrement(Numerics.BigMath.BigDecimal increment)
        {
            lock (this)
            {
                // Backwards compatibility: ignore rounding increment if zero,
                // and instead set maximum fraction digits.
                if (increment != null && increment.CompareTo(Numerics.BigMath.BigDecimal.Zero) == 0)
                {
                    properties.MaximumFractionDigits = int.MaxValue;
                    return;
                }

                properties.RoundingIncrement = increment;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Overload of
        /// <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>.
        /// </summary>
        /// <param name="increment">The increment to which numbers are to be rounded.</param>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="SetRoundingIncrement(double)"/>
        /// <category>Rounding</category>
        /// <stable>ICU 3.6</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual void SetRoundingIncrement(BigDecimal increment) // ICU4N TODO: Fix ICU4N.Numerics.BigDecimal to have an explicit cast to Numerics.BigMath.BigDecimal and change this to a single property setter above
        {
            lock (this)
            {
                Numerics.BigMath.BigDecimal javaBigDecimal = (increment == null) ? null : increment.ToBigDecimal();
                SetRoundingIncrement(javaBigDecimal);
            }
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Overload of
        /// <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>.
        /// </summary>
        /// <param name="increment">The increment to which numbers are to be rounded.</param>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="SetRoundingIncrement(BigDecimal)"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        public virtual void SetRoundingIncrement(double increment) // ICU4N TODO: change this to a single property setter above ?
        {
            lock (this)
            {
                if (increment == 0)
                {
                    SetRoundingIncrement((Numerics.BigMath.BigDecimal)null);
                }
                else
                {
                    // ICU4N NOTE: BigDecimal.GetInstance() uses a string to convert the value, since doing a precise
                    // conversion will result in rounding issues.
                    Numerics.BigMath.BigDecimal javaBigDecimal = Numerics.BigMath.BigDecimal.GetInstance(increment);
                    SetRoundingIncrement(javaBigDecimal);
                }
            }
        }

        /// <summary>
        /// Gets or sets the rounding mode being used to round numbers.
        /// 
        /// <para/>The default rounding mode is <see cref="Numerics.BigMath.RoundingMode.HalfEven"/>,
        /// which rounds decimals to their closest whole, and rounds to the closest even number if at the midpoint.
        /// 
        /// <para/>For more detail on rounding modes, see <a
        /// href="http://userguide.icu-project.org/formatparse/numbers/rounding-modes">the ICU User
        /// Guide</a>.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            override Numerics.BigMath.RoundingMode RoundingMode // ICU4N TODO: API Combine rounding modes?
        {
            get
            {
                lock (this)
                {
                    Numerics.BigMath.RoundingMode? mode = exportedProperties.RoundingMode;
                    return (mode == null) ? 0 : mode.Value; //.ordinal();
                }
            }
            set => SetRoundingMode(value);
        }

        /// <summary>
        /// <strong>Rounding and Digit Limits:</strong> Sets the <see cref="Numerics.BigMath.RoundingMode"/> used to round
        /// numbers. The default rounding mode is <see cref="Numerics.BigMath.RoundingMode.HalfEven"/>,
        /// which rounds decimals to their closest whole, and rounds to the closest even number if at the midpoint.
        /// 
        /// <para/>For more detail on rounding modes, see <a
        /// href="http://userguide.icu-project.org/formatparse/numbers/rounding-modes">the ICU User
        /// Guide</a>.
        /// </summary>
        /// <param name="roundingMode">The <see cref="Numerics.BigMath.RoundingMode"/> to use when formatting numbers.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>

        private void SetRoundingMode(Numerics.BigMath.RoundingMode roundingMode)
        {
            lock (this)
            {
                properties.RoundingMode = roundingMode;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the <see cref="Numerics.BigMath.MathContext"/> being used to round numbers.
        /// A "math context" encodes both a rounding mode and a number of significant
        /// digits. Most users should set <see cref="RoundingMode"/> and/or <see cref="MaximumSignificantDigits"/>
        /// instead of this method.
        /// 
        /// <para/>When formatting, since no division is ever performed, the default <see cref="Numerics.BigMath.MathContext"/>
        /// is <see cref="Numerics.BigMath.MathContext.Unlimited"/> significant digits. However, when division occurs during
        /// parsing to correct for percentages and multipliers, a <see cref="Numerics.BigMath.MathContext"/> of 34 digits,
        /// the IEEE 754R Decimal128 standard, is used by default. If you require more than 34 digits when parsing, you
        /// can set a custom <see cref="Numerics.BigMath.MathContext"/> using this property.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 4.2</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual Numerics.BigMath.MathContext MathContext
        {
            get
            {
                lock (this)
                {
                    Numerics.BigMath.MathContext mathContext = exportedProperties.MathContext;
                    Debug.Assert(mathContext != null);
                    return mathContext;
                }
            }
            set => SetMathContext(value);
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Sets the <see cref="Numerics.BigMath.MathContext"/> used
        /// to round numbers. A "math context" encodes both a rounding mode and a number of significant
        /// digits. Most users should set <see cref="RoundingMode"/> and/or <see cref="MaximumSignificantDigits"/>
        /// instead of this method.
        /// 
        /// <para/>When formatting, since no division is ever performed, the default <see cref="Numerics.BigMath.MathContext"/>
        /// is <see cref="Numerics.BigMath.MathContext.Unlimited"/> significant digits. However, when division occurs during
        /// parsing to correct for percentages and multipliers, a <see cref="Numerics.BigMath.MathContext"/> of 34 digits,
        /// the IEEE 754R Decimal128 standard, is used by default. If you require more than 34 digits when parsing, you
        /// can set a custom <see cref="Numerics.BigMath.MathContext"/> using this property.
        /// </summary>
        /// <param name="mathContext">The <see cref="Numerics.BigMath.MathContext"/> to use when rounding numbers.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 4.2</stable>
        private void SetMathContext(Numerics.BigMath.MathContext mathContext)
        {
            lock (this)
            {
                properties.MathContext = mathContext; // ICU4N TODO: Null guard clause?
                RefreshFormatter();
            }
        }

        // Remember the ICU math context form in order to be able to return it from the API.
        // NOTE: This value is not serialized. (should it be?)
        [NonSerialized]
        private ExponentForm icuMathContextForm = ExponentForm.Plain;

        /// <summary>
        /// <icu/> Gets or sets the <see cref="MathContext"/> being used to round numbers.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 4.2</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual MathContext MathContextICU
        {
            get
            {
                lock (this)
                {
                    Numerics.BigMath.MathContext mathContext = MathContext;
                    return new MathContext(
                        mathContext.Precision,
                        icuMathContextForm,
                        false,
                        mathContext.RoundingMode.ToICURoundingMode());
                }
            }
            set => SetMathContextICU(value);
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Overload of <see cref="SetMathContext(Numerics.BigMath.MathContext)"/> for
        /// <see cref="ICU4N.Numerics.MathContext"/>.
        /// </summary>
        /// <param name="mathContextICU">The <see cref="ICU4N.Numerics.MathContext"/> to use when rounding numbers.</param>
        /// <seealso cref="SetMathContext(Numerics.BigMath.MathContext)"/>
        /// <category>Rounding</category>
        /// <stable>ICU 4.2</stable>
        private void SetMathContextICU(MathContext mathContextICU)
        {
            lock (this)
            {
                icuMathContextForm = mathContextICU.Form;
                Numerics.BigMath.MathContext mathContext;
                if (mathContextICU.LostDigits)
                {
                    // The getLostDigits() feature in ICU MathContext means "throw an ArithmeticException if
                    // rounding causes digits to be lost". That feature is called RoundingMode.UNNECESSARY in
                    // Java MathContext.
                    mathContext = new Numerics.BigMath.MathContext(mathContextICU.Digits, Numerics.BigMath.RoundingMode.Unnecessary);
                }
                else
                {
                    mathContext =
                        new Numerics.BigMath.MathContext(
                            mathContextICU.Digits, mathContextICU.RoundingMode.ToRoundingMode());
                }
                MathContext = mathContext;
            }
        }

        /// <summary>
        /// Gets or sets the effective minimum number of digits before the decimal separator.
        /// If the number has fewer than this many digits, the number is padded with zeros.
        /// 
        /// <para/>For example, if minimum integer digits is 3, the number 12.3 will be printed as "001.23".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        public override int MinimumIntegerDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MinimumIntegerDigits;
            }
            set => SetMinimumIntegerDigits(value);
        }

        /// <summary>
        /// <strong>Rounding and Digit Limits:</strong> Sets the minimum number of digits to display before
        /// the decimal separator. If the number has fewer than this many digits, the number is padded with
        /// zeros.
        /// 
        /// <para/>For example, if minimum integer digits is 3, the number 12.3 will be printed as "001.23".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <param name="value">The minimum number of digits before the decimal separator.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        private void SetMinimumIntegerDigits(int value)
        {
            lock (this)
            {
                // For backwards compatibility, conflicting min/max need to keep the most recent setting.
                int max = properties.MaximumIntegerDigits;
                if (max >= 0 && max < value)
                {
                    properties.MaximumIntegerDigits = value;
                }
                properties.MinimumIntegerDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the effective maximum number of digits before the decimal separator.
        /// If the number has more than this many digits, the number is truncated.
        /// 
        /// <para/>For example, if maximum integer digits is 3, the number 12345 will be printed as "345".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        public override int MaximumIntegerDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MaximumIntegerDigits;
            }
            set => SetMaximumIntegerDigits(value);
        }

        /// <summary>
        /// <strong>Rounding and Digit Limits:</strong> Sets the maximum number of digits to display before
        /// the decimal separator. If the number has more than this many digits, the number is truncated.
        /// 
        /// <para/>For example, if maximum integer digits is 3, the number 12345 will be printed as "345".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <param name="value">The maximum number of digits before the decimal separator.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        private void SetMaximumIntegerDigits(int value)
        {
            lock (this)
            {
                int min = properties.MinimumIntegerDigits;
                if (min >= 0 && min > value)
                {
                    properties.MinimumIntegerDigits = value;
                }
                properties.MaximumIntegerDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the effective minimum number of integer digits after the decimal separator.
        /// If the number has fewer than this many digits, the number is padded with zeros.
        /// 
        /// <para/>For example, if minimum fraction digits is 2, the number 123.4 will be printed as "123.40".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// 
        /// <para/>See <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/> and
        /// <see cref="MaximumSignificantDigits"/> for two other ways of specifying rounding strategies.
        /// </summary>
        /// <seealso cref="RoundingMode"/>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="MaximumSignificantDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        public override int MinimumFractionDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MinimumFractionDigits;
            }
            set => SetMinimumFractionDigits(value);
        }

        /// <summary>
        /// <strong>Rounding and Digit Limits:</strong> Sets the minimum number of digits to display after
        /// the decimal separator. If the number has fewer than this many digits, the number is padded with zeros.
        /// 
        /// <para/>For example, if minimum fraction digits is 2, the number 123.4 will be printed as "123.40".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// 
        /// <para/>See <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/> and
        /// <see cref="MaximumSignificantDigits"/> for two other ways of specifying rounding strategies.
        /// </summary>
        /// <param name="value">The minimum number of integer digits after the decimal separator.</param>
        /// <seealso cref="RoundingMode"/>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="MaximumSignificantDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        private void SetMinimumFractionDigits(int value)
        {
            lock (this)
            {
                int max = properties.MaximumFractionDigits;
                if (max >= 0 && max < value)
                {
                    properties.MaximumFractionDigits = value;
                }
                properties.MinimumFractionDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the effective maximum number of integer digits after the decimal separator.
        /// If the number has more than this many digits, the number is rounded
        /// according to the rounding mode.
        /// 
        /// <para/>For example, if maximum fraction digits is 2, the number 123.456 will be printed as
        /// "123.46".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <seealso cref="MaximumIntegerDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        public override int MaximumFractionDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MaximumFractionDigits;
            }
            set => SetMaximumFractionDigits(value);
        }

        /// <summary>
        /// <strong>Rounding and Digit Limits:</strong> Sets the maximum number of digits to display after
        /// the decimal separator. If the number has more than this many digits, the number is rounded
        /// according to the rounding mode.
        /// 
        /// <para/>For example, if maximum fraction digits is 2, the number 123.456 will be printed as
        /// "123.46".
        /// 
        /// <para/>Minimum integer and minimum and maximum fraction digits can be specified via the pattern
        /// string. For example, "#,#00.00#" has 2 minimum integer digits, 2 minimum fraction digits, and 3
        /// maximum fraction digits. Note that it is not possible to specify maximium integer digits in the
        /// pattern except in scientific notation.
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <param name="value">The maximum number of integer digits after the decimal separator.</param>
        /// <seealso cref="RoundingMode"/>
        /// <category>Rounding</category>
        /// <stable>ICU 2.0</stable>
        private void SetMaximumFractionDigits(int value)
        {
            lock (this)
            {
                int min = properties.MinimumFractionDigits;
                if (min >= 0 && min > value)
                {
                    properties.MinimumFractionDigits = value;
                }
                properties.MaximumFractionDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether significant digits are being used in rounding.
        /// 
        /// <para/>Calling <c>decimalFormat.AreSignificantDigitsUsed = true</c> is functionally equivalent to:
        /// 
        /// <code>
        /// decimalFormat.MinimumSignificantDigits = 1;
        /// decimalFormat.MaximumSignificantDigits = 6;
        /// </code>
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        public virtual bool AreSignificantDigitsUsed // ICU4N TODO: We can probably leave this out of UNumberFormatInfo
        {
            get
            {
                lock (this)
                {
                    return properties.MinimumSignificantDigits != -1
                        || properties.MaximumSignificantDigits != -1;
                }
            }
            set => SetSignificantDigitsUsed(value);
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Sets whether significant digits are to be
        /// used in rounding.
        /// 
        /// <para/>Calling <c>decimalFormat.AreSignificantDigitsUsed = true</c> is functionally equivalent to:
        /// 
        /// <code>
        /// decimalFormat.MinimumSignificantDigits = 1;
        /// decimalFormat.MaximumSignificantDigits = 6;
        /// </code>
        /// </summary>
        /// <param name="useSignificantDigits"><c>true</c> to enable significant digit rounding; <c>false</c> to disable it.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        private void SetSignificantDigitsUsed(bool useSignificantDigits)
        {
            lock (this)
            {
                if (useSignificantDigits)
                {
                    // These are the default values from the old implementation.
                    properties.MinimumSignificantDigits = 1;
                    properties.MaximumSignificantDigits = 6;
                }
                else
                {
                    properties.MinimumSignificantDigits = -1;
                    properties.MaximumSignificantDigits = -1;
                }
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the effective minimum number of significant digits displayed.
        /// If the number of significant digits is less than this value, the number
        /// will be padded with zeros as necessary.
        /// 
        /// <para/>For example, if minimum significant digits is 3 and the number is 1.2, the number will be
        /// printed as "1.20".
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        public virtual int MinimumSignificantDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MinimumSignificantDigits;
            }
            set => SetMinimumSignificantDigits(value);


        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Sets the minimum number of significant
        /// digits to be displayed. If the number of significant digits is less than this value, the number
        /// will be padded with zeros as necessary.
        /// 
        /// <para/>For example, if minimum significant digits is 3 and the number is 1.2, the number will be
        /// printed as "1.20".
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// </summary>
        /// <param name="value">The minimum number of significant digits to display.</param>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        private void SetMinimumSignificantDigits(int value)
        {
            lock (this)
            {
                int max = properties.MaximumSignificantDigits;
                if (max >= 0 && max < value)
                {
                    properties.MaximumSignificantDigits = value;
                }
                properties.MinimumSignificantDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the effective maximum number of significant digits displayed.
        /// If the number of significant digits in the number exceeds this value,
        /// the number will be rounded according to the current rounding mode.
        /// 
        /// <para/>For example, if maximum significant digits is 3 and the number is 12345, the number will be
        /// printed as "12300".
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// 
        /// <para/>See <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/> and <see cref="MaximumFractionDigits"/>
        /// for two other ways of specifying rounding strategies.
        /// </summary>
        /// <seealso cref="RoundingMode"/>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="MaximumFractionDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        public virtual int MaximumSignificantDigits
        {
            get
            {
                lock (this)
                    return exportedProperties.MaximumSignificantDigits;
            }
            set => SetMaximumSignificantDigits(value);
        }

        /// <summary>
        /// <icu/> <strong>Rounding and Digit Limits:</strong> Sets the maximum number of significant
        /// digits to be displayed. If the number of significant digits in the number exceeds this value,
        /// the number will be rounded according to the current rounding mode.
        /// 
        /// <para/>For example, if maximum significant digits is 3 and the number is 12345, the number will be
        /// printed as "12300".
        /// 
        /// <para/>If minimum and maximum integer, fraction, or significant digits conflict with each other,
        /// the most recently specified value is used. For example, if there is a formatter with minInt=5,
        /// and then you set maxInt=3, then minInt will be changed to 3.
        /// 
        /// <para/>See <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/> and <see cref="MaximumFractionDigits"/>
        /// for two other ways of specifying rounding strategies.
        /// </summary>
        /// <param name="value">The maximum number of significant digits to display.</param>
        /// <seealso cref="RoundingMode"/>
        /// <seealso cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>
        /// <seealso cref="MaximumFractionDigits"/>
        /// <category>Rounding</category>
        /// <stable>ICU 3.0</stable>
        private void SetMaximumSignificantDigits(int value)
        {
            lock (this)
            {
                int min = properties.MinimumSignificantDigits;
                if (min >= 0 && min > value)
                {
                    properties.MinimumSignificantDigits = value;
                }
                properties.MaximumSignificantDigits = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of characters in formatted output.
        /// For example, if padding is enabled and paddingWidth is set to 6, formatting the
        /// number "3.14159" with the pattern "0.00" will result in "··3.14" if '·' is your padding string.
        /// 
        /// <para/>If the number is longer than your padding width, the number will display as if no padding
        /// width had been specified, which may result in strings longer than the padding width.
        /// 
        /// <para/>Padding can be specified in the pattern string using the '*' symbol. For example, the format
        /// "*x######0" has a format width of 7 and a pad character of 'x'.
        /// 
        /// <para/>Padding is currently counted in UTF-16 code units; see <a
        /// href="http://bugs.icu-project.org/trac/ticket/13034">ticket #13034</a> for more information.
        /// </summary>
        /// <seealso cref="PadCharacter"/>
        /// <seealso cref="PadPosition"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        public virtual int FormatWidth
        {
            get
            {
                lock (this)
                    return properties.FormatWidth;
            }
            set => SetFormatWidth(value);
        }

        /// <summary>
        /// <strong>Padding:</strong> Sets the minimum width of the string output by the formatting
        /// pipeline. For example, if padding is enabled and paddingWidth is set to 6, formatting the
        /// number "3.14159" with the pattern "0.00" will result in "··3.14" if '·' is your padding string.
        /// 
        /// <para/>If the number is longer than your padding width, the number will display as if no padding
        /// width had been specified, which may result in strings longer than the padding width.
        /// 
        /// <para/>Padding can be specified in the pattern string using the '*' symbol. For example, the format
        /// "*x######0" has a format width of 7 and a pad character of 'x'.
        /// 
        /// <para/>Padding is currently counted in UTF-16 code units; see <a
        /// href="http://bugs.icu-project.org/trac/ticket/13034">ticket #13034</a> for more information.
        /// </summary>
        /// <param name="width">The minimum number of characters in the output.</param>
        /// <seealso cref="PadCharacter"/>
        /// <seealso cref="PadPosition"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        private void SetFormatWidth(int width)
        {
            lock (this)
            {
                properties.FormatWidth = width;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the character used for padding for numbers that are narrower than the width
        /// specified in <see cref="FormatWidth"/>.
        /// 
        /// <para/>In the pattern string, the padding character is the token that follows '*' before or after
        /// the prefix or suffix.
        /// </summary>
        /// <seealso cref="FormatWidth"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        public virtual char PadCharacter
        {
            get
            {
                lock (this)
                {
                    string paddingString = properties.PadString;
                    if (paddingString is null)
                    {
                        return '.'; // TODO: Is this the correct behavior?
                    }
                    else
                    {
                        return paddingString[0];
                    }
                }
            }
            set => SetPadCharacter(value);
        }

        /// <summary>
        /// <icu/> <strong>Padding:</strong> Sets the character used to pad numbers that are narrower than
        /// the width specified in  <see cref="FormatWidth"/>.
        /// 
        /// <para/>In the pattern string, the padding character is the token that follows '*' before or after
        /// the prefix or suffix.
        /// </summary>
        /// <param name="padChar">The character used for padding.</param>
        /// <seealso cref="FormatWidth"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        private void SetPadCharacter(char padChar)
        {
            lock (this)
            {
                properties.PadString = char.ToString(padChar);
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the position where to insert the pad character when
        /// narrower than the width specified in <see cref="FormatWidth"/>. For example, consider the pattern
        /// "P123S" with padding width 8 and padding char "*". The four positions are:
        /// 
        /// <list type="bullet">
        ///     <item><term><see cref="PadPosition.BeforePrefix"/></term><description>"***P123S"</description></item>
        ///     <item><term><see cref="PadPosition.AfterPrefix"/></term><description>"P***123S"</description></item>
        ///     <item><term><see cref="PadPosition.BeforeSuffix"/></term><description>"P123***S"</description></item>
        ///     <item><term><see cref="PadPosition.AfterSuffix"/></term><description>"P123S***"</description></item>
        /// </list>
        /// </summary>
        /// <seealso cref="FormatWidth"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        public virtual PadPosition PadPosition
        {
            get
            {
                lock (this)
                {
                    Padder.PadPosition? loc = properties.PadPosition;
                    return (loc is null) ? PadPosition.BeforePrefix : loc.Value.ToOld();
                }
            }
            set => SetPadPosition(value);
        }

        /// <summary>
        /// <icu/> <strong>Padding:</strong> Sets the position where to insert the pad character when
        /// narrower than the width specified in <see cref="FormatWidth"/>. For example, consider the pattern
        /// "P123S" with padding width 8 and padding char "*". The four positions are:
        /// 
        /// <list type="bullet">
        ///     <item><term><see cref="PadPosition.BeforePrefix"/></term><description>"***P123S"</description></item>
        ///     <item><term><see cref="PadPosition.AfterPrefix"/></term><description>"P***123S"</description></item>
        ///     <item><term><see cref="PadPosition.BeforeSuffix"/></term><description>"P123***S"</description></item>
        ///     <item><term><see cref="PadPosition.AfterSuffix"/></term><description>"P123S***"</description></item>
        /// </list>
        /// </summary>
        /// <param name="padPos">The position used for padding.</param>
        /// <seealso cref="FormatWidth"/>
        /// <category>Padding</category>
        /// <stable>ICU 2.0</stable>
        private void SetPadPosition(PadPosition padPos)
        {
            lock (this)
            {
                properties.PadPosition = padPos.ToNew();
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether scientific (exponential) notation is enabled on this formatter.
        /// For example, if scientific notation is enabled, the number
        /// 123000 will be printed as "1.23E5" in locale <em>en-US</em>. A locale-specific symbol is used
        /// as the exponent separator.
        /// 
        /// <para/>Setting <c>decimalFormat.UseScientificNotation = true</c> if functionally equivalent to setting
        /// <c>decimalFormat.MinimumExponentDigits = 1</c>.
        /// </summary>
        /// <seealso cref="MinimumExponentDigits"/>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        public virtual bool UseScientificNotation // ICU4N NOTE: We probably don't need this setting in UNumberFormatInfo. IN .NET, this is equivalent to the "e" standard format string.
        {
            get
            {
                lock (this)
                    return properties.MinimumExponentDigits != -1;
            }
            set => SetScientificNotation(value);
        }

        /// <summary>
        /// <icu/> <strong>Scientific Notation:</strong> Sets whether this formatter should print in
        /// scientific (exponential) notation. For example, if scientific notation is enabled, the number
        /// 123000 will be printed as "1.23E5" in locale <em>en-US</em>. A locale-specific symbol is used
        /// as the exponent separator.
        /// 
        /// <para/>Setting <c>decimalFormat.UseScientificNotation = true</c> if functionally equivalent to setting
        /// <c>decimalFormat.MinimumExponentDigits = 1</c>.
        /// </summary>
        /// <param name="useScientific"><c>true</c> to enable scientific notation; <c>false</c> to disable it.</param>
        /// <seealso cref="MinimumExponentDigits"/>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        private void SetScientificNotation(bool useScientific)
        {
            lock (this)
            {
                if (useScientific)
                {
                    properties.MinimumExponentDigits = 1;
                }
                else
                {
                    properties.MinimumExponentDigits = -1;
                }
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the minimum number of digits printed in the exponent in scientific notation.
        /// For example, if minimum exponent digits is 3, the number 123000 will be printed
        /// as "1.23E005".
        /// 
        /// <para/>This setting corresponds to the number of zeros after the 'E' in a pattern string such as
        /// "0.00E000".
        /// </summary>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        public virtual byte MinimumExponentDigits // ICU4N NOTE: This is equivalent to the "3" in "E3" standard pattern in .NET
        {
            get
            {
                lock (this)
                    return (byte)properties.MinimumExponentDigits;
            }
            set => SetMinimumExponentDigits(value);
        }

        /// <summary>
        /// <icu/> <strong>Scientific Notation:</strong> Sets the minimum number of digits to be printed in
        /// the exponent. For example, if minimum exponent digits is 3, the number 123000 will be printed
        /// as "1.23E005".
        /// 
        /// <para/>This setting corresponds to the number of zeros after the 'E' in a pattern string such as
        /// "0.00E000".
        /// </summary>
        /// <param name="minExpDig">The minimum number of digits in the exponent.</param>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        private void SetMinimumExponentDigits(byte minExpDig)
        {
            lock (this)
            {
                properties.MinimumExponentDigits = minExpDig;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether the sign (plus or minus) is always printed in scientific notation.
        /// For example, if this setting is enabled, the
        /// number 123000 will be printed as "1.23E+5" in locale <em>en-US</em>. The number 0.0000123 will
        /// always be printed as "1.23E-5" in locale <em>en-US</em> whether or not this setting is enabled.
        /// 
        /// <para/>This setting corresponds to the '+' in a pattern such as "0.00E+0".
        /// </summary>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        public virtual bool ExponentSignAlwaysShown // ICU4N NOTE: The only way to set this in .NET is to set the PositiveSign = ""
        {
            get
            {
                lock (this)
                    return properties.ExponentSignAlwaysShown;
            }
            set => SetExponentSignAlwaysShown(value);
        }

        /// <summary>
        /// <icu/> <strong>Scientific Notation:</strong> Sets whether the sign (plus or minus) is always to
        /// be shown in the exponent in scientific notation. For example, if this setting is enabled, the
        /// number 123000 will be printed as "1.23E+5" in locale <em>en-US</em>. The number 0.0000123 will
        /// always be printed as "1.23E-5" in locale <em>en-US</em> whether or not this setting is enabled.
        /// 
        /// <para/>This setting corresponds to the '+' in a pattern such as "0.00E+0".
        /// </summary>
        /// <param name="expSignAlways"><c>true</c> to always shown the sign in the exponent; <c>false</c> to show it for
        /// negatives but not positives.</param>
        /// <category>ScientificNotation</category>
        /// <stable>ICU 2.0</stable>
        private void SetExponentSignAlwaysShown(bool expSignAlways)
        {
            lock (this)
            {
                properties.ExponentSignAlwaysShown = expSignAlways;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets whether or not grouping separators are being printed in the output.
        /// Grouping means whether the thousands, millions, billions, and larger powers of ten should be
        /// separated by a grouping separator (a comma in <em>en-US</em>).
        /// 
        /// <para/>For example, if grouping is enabled, 12345 will be printed as "12,345" in <em>en-US</em>. If
        /// grouping were disabled, it would instead be printed as simply "12345".
        /// 
        /// <para/>Setting <c>decimalFormat.IsGroupingUsed = true</c> is functionally equivalent to setting grouping
        /// size to 3, as in <c>decimalFormat.GroupingSize = 3</c>.
        /// </summary>
        /// <seealso cref="GroupingSize"/>
        /// <seealso cref="SecondaryGroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        public override bool IsGroupingUsed // ICU4N NOTE: In .NET, grouping can be "turned off" by setting the NumberGroupSeparator, CurrencyGroupSeparater, or PercentGroupSeparator to empty string. Not all standard formats do grouping.
        {
            get
            {
                lock (this)
                    return properties.GroupingSize != -1 || properties.SecondaryGroupingSize != -1;
            }
            set => SetGroupingUsed(value);
        }

        /// <summary>
        /// <strong>Grouping:</strong> Sets whether grouping is to be used when formatting numbers.
        /// Grouping means whether the thousands, millions, billions, and larger powers of ten should be
        /// separated by a grouping separator (a comma in <em>en-US</em>).
        /// 
        /// <para/>For example, if grouping is enabled, 12345 will be printed as "12,345" in <em>en-US</em>. If
        /// grouping were disabled, it would instead be printed as simply "12345".
        /// 
        /// <para/>Setting <c>decimalFormat.IsGroupingUsed = true</c> is functionally equivalent to setting grouping
        /// size to 3, as in <c>decimalFormat.GroupingSize = 3</c>.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable grouping separators; <c>false</c> to disable them.</param>
        /// <seealso cref="GroupingSize"/>
        /// <seealso cref="SecondaryGroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        private void SetGroupingUsed(bool enabled)
        {
            lock (this)
            {
                if (enabled)
                {
                    // Set to a reasonable default value
                    properties.GroupingSize = 3;
                }
                else
                {
                    properties.GroupingSize = -1;
                    properties.SecondaryGroupingSize = -1;
                }
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the primary grouping size (distance between grouping
        /// separators) used when formatting large numbers. For most locales, this defaults to 3: the
        /// number of digits between the ones and thousands place, between thousands and millions, and so
        /// forth.
        /// 
        /// <para/>For example, with a grouping size of 3, the number 1234567 will be formatted as "1,234,567".
        /// 
        /// <para/>Grouping size can also be specified in the pattern: for example, "#,##0" corresponds to a
        /// grouping size of 3.
        /// </summary>
        /// <see cref="SecondaryGroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        public virtual int GroupingSize // ICU4N NOTE: In .NET grouping sizes are stored in an array (primary, secondary, etc).
        {
            get
            {
                lock (this)
                    return properties.GroupingSize;
            }
            set => SetGroupingSize(value);
        }

        /// <summary>
        /// <strong>Grouping:</strong> Sets the primary grouping size (distance between grouping
        /// separators) used when formatting large numbers. For most locales, this defaults to 3: the
        /// number of digits between the ones and thousands place, between thousands and millions, and so
        /// forth.
        /// 
        /// <para/>For example, with a grouping size of 3, the number 1234567 will be formatted as "1,234,567".
        /// 
        /// <para/>Grouping size can also be specified in the pattern: for example, "#,##0" corresponds to a
        /// grouping size of 3.
        /// </summary>
        /// <param name="width">The grouping size to use.</param>
        /// <see cref="SecondaryGroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        private void SetGroupingSize(int width)
        {
            lock (this)
            {
                properties.GroupingSize = width;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the secondary grouping size (distance between grouping
        /// separators after the first separator) used when formatting large numbers.
        /// In many south Asian locales, this is set to 2.
        /// 
        /// <para/>For example, with primary grouping size 3 and secondary grouping size 2, the number 1234567
        /// will be formatted as "12,34,567".
        /// 
        /// <para/>Grouping size can also be specified in the pattern: for example, "#,##,##0" corresponds to a
        /// primary grouping size of 3 and a secondary grouping size of 2.
        /// </summary>
        /// <see cref="GroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        public virtual int SecondaryGroupingSize
        {
            get
            {
                lock (this)
                {
                    int grouping1 = properties.GroupingSize;
                    int grouping2 = properties.SecondaryGroupingSize;
                    if (grouping1 == grouping2 || grouping2 < 0)
                    {
                        return 0;
                    }
                    return properties.SecondaryGroupingSize;
                }
            }
            set => SetSecondaryGroupingSize(value);
        }

        /// <summary>
        /// <icu/> <strong>Grouping:</strong> Sets the secondary grouping size (distance between grouping
        /// separators after the first separator) used when formatting large numbers.
        /// In many south Asian locales, this is set to 2.
        /// 
        /// <para/>For example, with primary grouping size 3 and secondary grouping size 2, the number 1234567
        /// will be formatted as "12,34,567".
        /// 
        /// <para/>Grouping size can also be specified in the pattern: for example, "#,##,##0" corresponds to a
        /// primary grouping size of 3 and a secondary grouping size of 2.
        /// </summary>
        /// <param name="width">The secondary grouping size to use.</param>
        /// <see cref="GroupingSize"/>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        private void SetSecondaryGroupingSize(int width)
        {
            lock (this)
            {
                properties.SecondaryGroupingSize = width;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the minimum number of digits before grouping is triggered.
        /// For example, if minimum grouping digits is set
        /// to 2, in <em>en-US</em>, 1234 will be printed as "1234" and 12345 will be printed as "12,345".
        /// </summary>
        /// <category>Separators</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        public virtual int MinimumGroupingDigits // ICU4N NOTE: This would require a custom format string in .NET.
        {
            get
            {
                lock (this)
                {
                    // Only 1 and 2 are supported right now.
                    if (properties.MinimumGroupingDigits == 2)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            set => SetMinimumGroupingDigits(value);
        }

        /// <summary>
        /// <icu/> Sets the minimum number of digits that must be before the first grouping separator in
        /// order for the grouping separator to be printed. For example, if minimum grouping digits is set
        /// to 2, in <em>en-US</em>, 1234 will be printed as "1234" and 12345 will be printed as "12,345".
        /// </summary>
        /// <param name="number">The minimum number of digits before grouping is triggered.</param>
        /// <category>Separators</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        private void SetMinimumGroupingDigits(int number)
        {
            lock (this)
            {
                properties.MinimumGroupingDigits = number;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets whether the decimal separator (a period in <em>en-US</em>) is
        /// shown on integers. For example, if this setting is turned on, formatting 123 will result in
        /// "123." with the decimal separator.
        /// 
        /// <para/>This setting can be specified in the pattern for integer formats: "#,##0." is an example.
        /// </summary>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        public virtual bool DecimalSeparatorAlwaysShown // ICU4N TODO: In .NET, this requires a custom format string?
        {
            get
            {
                lock (this)
                    return properties.DecimalSeparatorAlwaysShown;
            }
            set => SetDecimalSeparatorAlwaysShown(value);
        }

        /// <summary>
        /// <strong>Separators:</strong> Sets whether the decimal separator (a period in <em>en-US</em>) is
        /// shown on integers. For example, if this setting is turned on, formatting 123 will result in
        /// "123." with the decimal separator.
        /// 
        /// <para/>This setting can be specified in the pattern for integer formats: "#,##0." is an example.
        /// </summary>
        /// <param name="value"><c>true</c> to always show the decimal separator; false to show it only when there is a
        /// fraction part of the number.</param>
        /// <category>Separators</category>
        /// <stable>ICU 2.0</stable>
        private void SetDecimalSeparatorAlwaysShown(bool value)
        {
            lock (this)
            {
                properties.DecimalSeparatorAlwaysShown = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the user-specified currency. May be <c>null</c>. The effect is twofold:
        /// 
        /// <list type="number">
        ///     <item><description>Substitutions for currency symbols in the pattern string will use this currency</description></item>
        ///     <item><description>The rounding mode will obey the rules for this currency (see <see cref="CurrencyUsage"/>).</description></item>
        /// </list>
        /// 
        /// <strong>Important:</strong> Displaying the currency in the output requires that the pattern
        /// associated with this formatter contains a currency symbol '¤'. This will be the case if the
        /// instance was created via <see cref="NumberFormat.GetCurrencyInstance()"/> or one of its overloads.
        /// </summary>
        /// <category>Currency</category>
        /// <stable>ICU 2.2</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            override Currency Currency
        {
            get
            {
                lock (this)
                    return properties.Currency;
            }
            set => SetCurrency(value);
        }

        /// <summary>
        /// Sets the currency to be used when formatting numbers. The effect is twofold:
        /// 
        /// <list type="number">
        ///     <item><description>Substitutions for currency symbols in the pattern string will use this currency</description></item>
        ///     <item><description>The rounding mode will obey the rules for this currency (see <see cref="CurrencyUsage"/>).</description></item>
        /// </list>
        /// 
        /// <strong>Important:</strong> Displaying the currency in the output requires that the pattern
        /// associated with this formatter contains a currency symbol '¤'. This will be the case if the
        /// instance was created via <see cref="NumberFormat.GetCurrencyInstance()"/> or one of its overloads.
        /// </summary>
        /// <param name="currency">The currency to use.</param>
        /// <category>Currency</category>
        /// <stable>ICU 2.2</stable>
        private void SetCurrency(Currency currency)
        {
            lock (this)
            {
                properties.Currency = currency;
                // Backwards compatibility: also set the currency in the DecimalFormatSymbols
                if (currency != null)
                {
                    symbols.Currency = currency;
                    string symbol = currency.GetName(symbols.UCulture, CurrencyNameStyle.SymbolName, out bool _);
                    symbols.CurrencySymbol = symbol;
                }
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the currency-dependent strategy to use when rounding numbers. There are two
        /// strategies:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="CurrencyUsage.Standard"/></term>
        ///         <description>When the amount displayed is intended for banking statements or electronic
        ///         transfer.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="CurrencyUsage.Cash"/></term>
        ///         <description>When the amount displayed is intended to be representable in physical currency,
        ///         like at a cash register.</description>
        ///         </item>
        /// </list>
        /// 
        /// <see cref="CurrencyUsage.Cash"/> mode is relevant in currencies that do not have tender down to the penny. For more
        /// information on the two rounding strategies, see <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Supplemental_Currency_Data">UTS #35</a>.
        /// If omitted, the strategy defaults to <see cref="CurrencyUsage.Standard"/>. To override currency rounding
        /// altogether, use <see cref="MinimumFractionDigits"/> and <see cref="MaximumFractionDigits"/> or
        /// <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>.
        /// </summary>
        /// <category>Currency</category>
        /// <stable>ICU 54</stable>
        public virtual CurrencyUsage CurrencyUsage
        {
            get
            {
                lock (this)
                {
                    // CurrencyUsage is not exported, so we have to get it from the input property bag.
                    // TODO: Should we export CurrencyUsage instead?
                    CurrencyUsage? usage = properties.CurrencyUsage;
                    if (usage is null)
                    {
                        usage = Util.CurrencyUsage.Standard;
                    }
                    return usage.Value;
                }
            }
            set => SetCurrencyUsage(value);
        }

        /// <summary>
        /// <icu/> Sets the currency-dependent strategy to use when rounding numbers. There are two
        /// strategies:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="CurrencyUsage.Standard"/></term>
        ///         <description>When the amount displayed is intended for banking statements or electronic
        ///         transfer.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="CurrencyUsage.Cash"/></term>
        ///         <description>When the amount displayed is intended to be representable in physical currency,
        ///         like at a cash register.</description>
        ///         </item>
        /// </list>
        /// 
        /// <see cref="CurrencyUsage.Cash"/> mode is relevant in currencies that do not have tender down to the penny. For more
        /// information on the two rounding strategies, see <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Supplemental_Currency_Data">UTS #35</a>.
        /// If omitted, the strategy defaults to <see cref="CurrencyUsage.Standard"/>. To override currency rounding
        /// altogether, use <see cref="MinimumFractionDigits"/> and <see cref="MaximumFractionDigits"/> or
        /// <see cref="SetRoundingIncrement(Numerics.BigMath.BigDecimal)"/>.
        /// </summary>
        /// <param name="usage">The strategy to use when rounding in the current currency.</param>
        /// <category>Currency</category>
        /// <stable>ICU 54</stable>
        internal void SetCurrencyUsage(CurrencyUsage? usage)
        {
            lock (this)
            {
                properties.CurrencyUsage = usage;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets the current instance of <see cref="ICU4N.Text.CurrencyPluralInfo"/>.
        /// <see cref="ICU4N.Text.CurrencyPluralInfo"/> generates pattern
        /// strings for printing currency long names.
        /// 
        /// <para/><strong>Most users should not call this method directly.</strong> You should instead create
        /// your formatter via <see cref="NumberFormat.GetCurrencyInstance()"/>.
        /// </summary>
        /// <category>Currency</category>
        /// <stable>ICU 4.2</stable>
        public virtual CurrencyPluralInfo CurrencyPluralInfo
        {
            get
            {
                lock (this)
                    // CurrencyPluralInfo also is not exported.
                    return properties.CurrencyPluralInfo;
            }
            set => SetCurrencyPluralInfo(value);
        }

        /// <summary>
        /// <icu/> Sets a custom instance of <see cref="CurrencyPluralInfo"/>.
        /// <see cref="ICU4N.Text.CurrencyPluralInfo"/> generates pattern
        /// strings for printing currency long names.
        /// 
        /// <para/><strong>Most users should not call this method directly.</strong> You should instead create
        /// your formatter via <see cref="NumberFormat.GetCurrencyInstance()"/>.
        /// </summary>
        /// <param name="newInfo">The <see cref="ICU4N.Text.CurrencyPluralInfo"/> to use when printing currency long names.</param>
        /// <category>Currency</category>
        /// <stable>ICU 4.2</stable>
        private void SetCurrencyPluralInfo(CurrencyPluralInfo newInfo)
        {
            lock (this)
            {
                properties.CurrencyPluralInfo = newInfo;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// Gets or sets whether to make <see cref="Parse(string, ParsePosition)"/> prefer returning a <see cref="Numerics.BigDecimal"/> when
        /// possible. For strings corresponding to return values of Infinity, -Infinity, NaN, and -0.0, a
        /// <see cref="J2N.Numerics.Double"/> will be returned even if <see cref="ParseToBigDecimal"/> is enabled.
        /// </summary>
        /// <category>Parsing</category>
        /// <stable>ICU 3.6</stable>
        public virtual bool ParseToBigDecimal
        {
            get
            {
                lock (this)
                    return properties.ParseToBigDecimal;
            }
            set => SetParseToBigDecimal(value);
        }

        /// <summary>
        /// Whether to make <see cref="Parse(string, ParsePosition)"/> prefer returning a <see cref="Numerics.BigDecimal"/> when
        /// possible. For strings corresponding to return values of Infinity, -Infinity, NaN, and -0.0, a
        /// <see cref="J2N.Numerics.Double"/> will be returned even if <see cref="ParseToBigDecimal"/> is enabled.
        /// </summary>
        /// <param name="value"><c>true</c> to cause <see cref="Parse(string, ParsePosition)"/> to prefer
        /// <see cref="Numerics.BigDecimal"/>; <c>false</c> to let <see cref="Parse(string, ParsePosition)"/> return
        /// additional data types like <see cref="J2N.Numerics.Int64"/> or <see cref="Numerics.BigMath.BigInteger"/></param>
        /// <category>Parsing</category>
        /// <stable>ICU 3.6</stable>
        private void SetParseToBigDecimal(bool value)
        {
            lock (this)
            {
                properties.ParseToBigDecimal = value;
                // refreshFormatter() not needed
            }
        }

        /// <summary>
        /// Always returns 1000, the default prior to ICU4J 59. Setting max parse digits has no effect.
        /// </summary>
        /// <category>Parsing</category>
        [Obsolete("Setting max parse digits has no effect since ICU 59.")]
        internal virtual int ParseMaxDigits // ICU4N specifc - made internal rather than public
        {
            get => 1000;
            set { /* Intentionally blank */ }
        }

        /// <inheritdoc/>
        /// <category>Parsing</category>
        /// <stable>ICU 3.6</stable>
        public override bool ParseStrict
        {
            get
            {
                lock (this)
                    return properties.ParseMode == Parser.ParseMode.Strict;
            }
            set => SetParseStrict(value);
        }

        private void SetParseStrict(bool parseStrict)
        {
            lock (this)
            {
                Parser.ParseMode mode = parseStrict ? Parser.ParseMode.Strict : Parser.ParseMode.Lenient;
                properties.ParseMode = mode;
                // refreshFormatter() not needed
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// 
        /// <para/>This is functionally equivalent to setting <see cref="DecimalPatternMatchRequired"/> and a
        /// pattern without a decimal point.
        /// </summary>
        /// <category>Parsing</category>
        /// <stable>ICU 2.0</stable>
        public override bool ParseIntegerOnly
        {
            get
            {
                lock (this)
                    return properties.ParseIntegerOnly;
            }
            set => SetParseIntegerOnly(value);
        }

        private void SetParseIntegerOnly(bool parseIntegerOnly)
        {
            lock (this)
            {
                properties.ParseIntegerOnly = parseIntegerOnly;
                // refreshFormatter() not needed
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether the presence of a decimal point must match the pattern.
        /// This method is used to either <em>require</em> or
        /// <em>forbid</em> the presence of a decimal point in the string being parsed (disabled by
        /// default). This feature was designed to be an extra layer of strictness on top of strict
        /// parsing, although it can be used in either lenient mode or strict mode.
        /// 
        /// <para/>To <em>require</em> a decimal point, set this property in combination with either a pattern
        /// containing a decimal point or with <see cref="DecimalSeparatorAlwaysShown"/>.
        /// 
        /// <code>
        /// // Require a decimal point in the string being parsed:
        /// decimalFormat.ApplyPattern("#.");
        /// decimalFormat.DecimalPatternMatchRequired = true;
        /// 
        /// // Alternatively:
        /// decimalFormat.DecimalSeparatorAlwaysShown = true;
        /// decimalFormat.DecimalPatternMatchRequired = true;
        /// </code>
        /// 
        /// To <em>forbid</em> a decimal point, call this method in combination with a pattern containing
        /// no decimal point. Alternatively, set <see cref="ParseIntegerOnly"/> for the same behavior without
        /// depending on the contents of the pattern string.
        /// 
        /// <code>
        /// // Forbid a decimal point in the string being parsed:
        /// decimalFormat.ApplyPattern("#");
        /// decimalFormat.DecimalPatternMatchRequired = true;
        /// </code>
        /// </summary>
        /// <seealso cref="ParseIntegerOnly"/>
        /// <category>Parsing</category>
        /// <stable>ICU 54</stable>
        public virtual bool DecimalPatternMatchRequired
        {
            get
            {
                lock (this)
                    return properties.DecimalPatternMatchRequired;
            }
            set => SetDecimalPatternMatchRequired(value);
        }

        private void SetDecimalPatternMatchRequired(bool value)
        {
            lock (this)
            {
                properties.DecimalPatternMatchRequired = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether to ignore exponents when parsing. For
        /// example, parses "123E4" to 123 (with parse position 3) instead of 1230000 (with parse position
        /// 5).
        /// </summary>
        /// <category>Parsing</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        public virtual bool ParseNoExponent
        {
            get
            {
                lock (this)
                    return properties.ParseNoExponent;
            }
            set => SetParseNoExponent(value);
        }

        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        private void SetParseNoExponent(bool value)
        {
            lock (this)
            {
                properties.ParseNoExponent = value;
                RefreshFormatter();
            }
        }

        /// <summary>
        /// <icu/> Gets or sets whether to force case (uppercase/lowercase) to match when parsing.
        /// Specifies whether parsing should require cases to match in affixes, exponent separators,
        /// and currency codes. Case mapping is performed for each code point using <see cref="UChar.FoldCase(int, bool)"/>.
        /// </summary>
        /// <seealso cref="ParseNoExponent"/>
        /// <category>Parsing</category>
        /// <internal/>
        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        public virtual bool ParseCaseSensitive
        {
            get
            {
                lock (this)
                    return properties.ParseCaseSensitive;
            }
            set => SetParseCaseSensitive(value);
        }

        [Obsolete("ICU 59: This API is a technical preview. It may change in an upcoming release.")]
        private void SetParseCaseSensitive(bool value)
        {
            lock (this)
            {
                properties.ParseCaseSensitive = value;
                RefreshFormatter();
            }
        }

        // TODO(sffc): Uncomment for ICU 60 API proposal.
        //
        //  /**
        //   * {@icu} Returns the strategy used for choosing between grouping and decimal separators when
        //   * parsing.
        //   *
        //   * @see #setParseGroupingMode
        //   * @category Parsing
        //   */
        //  public synchronized GroupingMode getParseGroupingMode() {
        //    return properties.getParseGroupingMode();
        //  }
        //
        //  /**
        //   * {@icu} Sets the strategy used during parsing when a code point needs to be interpreted as
        //   * either a decimal separator or a grouping separator.
        //   *
        //   * <para/>The comma, period, space, and apostrophe have different meanings in different locales. For
        //   * example, in <em>en-US</em> and most American locales, the period is used as a decimal
        //   * separator, but in <em>es-PY</em> and most European locales, it is used as a grouping separator.
        //   *
        //   * Suppose you are in <em>fr-FR</em> the parser encounters the string "1.234".  In <em>fr-FR</em>,
        //   * the grouping is a space and the decimal is a comma.  The <em>grouping mode</em> is a mechanism
        //   * to let you specify whether to accept the string as 1234 (GroupingMode.DEFAULT) or whether to reject it since the separators
        //   * don't match (GroupingMode.RESTRICTED).
        //   *
        //   * When resolving grouping separators, it is the <em>equivalence class</em> of separators that is considered.
        //   * For example, a period is seen as equal to a fixed set of other period-like characters.
        //   *
        //   * @param groupingMode The strategy to use; either DEFAULT or RESTRICTED.
        //   * @category Parsing
        //   */
        //  public synchronized void setParseGroupingMode(GroupingMode groupingMode) {
        //    properties.setParseGroupingMode(groupingMode);
        //    refreshFormatter();
        //  }

        //=====================================================================================//
        //                                     UTILITIES                                       //
        //=====================================================================================//

        /// <summary>
        /// Tests for equality between this formatter and another formatter.
        /// 
        /// <para/>If two <see cref="DecimalFormat"/> instances are equal, then they will always produce the same output.
        /// However, the reverse is not necessarily true: if two <see cref="DecimalFormat"/> instances always produce the
        /// same output, they are not necessarily equal.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="DecimalFormat"/> and contains
        /// equivalent <see cref="DecimalFormatSymbols"/> and <see cref="DecimalFormatProperties"/>; otherwise, <c>false</c>.
        /// If <paramref name="obj"/> is <c>null</c>, the method returns <c>false</c></returns>
        public override bool Equals(object obj)
        {

            if (obj == null) return false;
            if (obj == this) return true;
            if (!(obj is DecimalFormat other)) return false;
            lock (this)
            {
                return properties.Equals(other.properties) && symbols.Equals(other.symbols);
            }
        }
    
        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
        public override int GetHashCode()
        {
            lock (this)
            {
                return properties.GetHashCode() ^ symbols.GetHashCode();
            }
        }

        /// <summary>
        /// Returns the default value of <see cref="object.ToString()"/> with extra
        /// <see cref="DecimalFormat"/>-specific information appended to the end of the string.
        /// This extra information is intended for debugging purposes, and the
        /// format is not guaranteed to be stable.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(GetType().Name);
            result.Append("@");
            result.Append(GetHashCode().ToHexString());
            result.Append(" { symbols@");
            result.Append(symbols.GetHashCode().ToHexString());
            lock (this)
            {
                properties.ToStringBare(result);
            }
            result.Append(" }");
            return result.ToString();
        }

        /// <summary>
        /// Serializes this formatter object to a decimal format pattern string. The result of this method
        /// is guaranteed to be <em>functionally</em> equivalent to the pattern string used to create this
        /// instance after incorporating values from the setter methods.
        /// 
        /// <para/>For more information on decimal format pattern strings, see <a
        /// href="http://unicode.org/reports/tr35/tr35-numbers.html#Number_Format_Patterns">UTS #35</a>.
        /// 
        /// <para/><strong>Important:</strong> Not all properties are capable of being encoded in a pattern
        /// string. See a list of properties in <see cref="ApplyPattern(string)"/>.
        /// </summary>
        /// <returns>A decimal format pattern string.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual string ToPattern()
        {
            lock (this)
            {
                // Pull some properties from exportedProperties and others from properties
                // to keep affix patterns intact.  In particular, pull rounding properties
                // so that CurrencyUsage is reflected properly.
                // TODO: Consider putting this logic in PatternString.java instead.
                DecimalFormatProperties tprops = threadLocalProperties.Value.CopyFrom(properties);
                if (UseCurrency(properties))
                {
                    tprops.MinimumFractionDigits = exportedProperties.MinimumFractionDigits;
                    tprops.MaximumFractionDigits = exportedProperties.MaximumFractionDigits;
                    tprops.RoundingIncrement = exportedProperties.RoundingIncrement;
                }
                return PatternStringUtils.PropertiesToPatternString(tprops);
            }
        }

        /// <summary>
        /// Calls <see cref="ToPattern()"/> and converts the string to localized notation. For more information on
        /// localized notation, see <see cref="ApplyLocalizedPattern(string)"/>. This method is provided for backwards
        /// compatibility and should not be used in new projects.
        /// </summary>
        /// <returns>A decimal format pattern string in localized notation.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual string ToLocalizedPattern()
        {
            lock (this)
            {
                string pattern = ToPattern();
                return PatternStringUtils.ConvertLocalized(pattern, symbols, true);
            }
        }

        /// <summary>
        /// Converts this <see cref="DecimalFormat"/> to a <see cref="NumberFormatter"/>. Starting in ICU 60,
        /// <see cref="NumberFormatter"/> is the recommended way to format numbers.
        /// </summary>
        /// <returns>An instance of <see cref="LocalizedNumberFormatter"/> with the same behavior as this instance of
        /// <see cref="DecimalFormat"/>.</returns>
        /// <seealso cref="NumberFormatter"/>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <draft>ICU 60</draft>
#if FEATURE_NUMBERFORMATTER
        public
#else
        internal
#endif
            virtual LocalizedNumberFormatter ToNumberFormatter()
        {
            return formatter;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual IFixedDecimal GetFixedDecimal(double number) // ICU4N specific - made internal instead of public
        {
            return formatter.Format(number).FixedDecimal;
        }

        private static readonly ThreadLocal<DecimalFormatProperties> threadLocalProperties =
            new ThreadLocal<DecimalFormatProperties>(() => new DecimalFormatProperties());

        /// <summary>
        /// Rebuilds the formatter object from the property bag.
        /// </summary>
        internal void RefreshFormatter()
        {
            if (exportedProperties == null)
            {
                // exportedProperties is null only when the formatter is not ready yet.
                // The only time when this happens is during legacy deserialization.
                return;
            }
            UCultureInfo locale = ActualCulture;
            if (locale == null)
            {
                // Constructor
                locale = symbols.ActualCulture;
            }
            if (locale == null)
            {
                // Deserialization
                locale = symbols.UCulture;
            }
            Debug.Assert(locale != null);
#pragma warning disable CS0618 // Type or member is obsolete
            formatter = NumberFormatter.FromDecimalFormat(properties, symbols, exportedProperties).Culture(locale);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Converta a <see cref="Numerics.BigMath.BigDecimal"/> to a <see cref="Numerics.BigDecimal"/> with fallback for numbers
        /// outside of the range supported by <see cref="Numerics.BigDecimal"/>.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="number"/> is <c>null</c>.</exception>
        private Number SafeConvertBigDecimal(Numerics.BigMath.BigDecimal number)
        {
            if (number is null)
                throw new ArgumentNullException(nameof(number));

            // ICU4N: Use TryParse so we don't have to deal with exceptions here.
            string value = number.ToString(CultureInfo.InvariantCulture);
            if (!BigDecimal.TryParse(value, CultureInfo.InvariantCulture, out BigDecimal result))
            {
                if (number.Sign > 0 && number.Scale < 0)
                {
                    return Double.GetInstance(double.PositiveInfinity);
                }
                else if (number.Scale < 0)
                {
                    return Double.GetInstance(double.NegativeInfinity);
                }
                else if (number.Sign < 0)
                {
                    return Double.GetInstance(-0.0);
                }
                else
                {
                    return Double.GetInstance(0.0);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the currency is set in the <paramref name="properties"/> or if currency symbols are present in
        /// the prefix/suffix pattern.
        /// </summary>
        private static bool UseCurrency(DecimalFormatProperties properties)
        {
            return ((properties.Currency != null)
                || properties.CurrencyPluralInfo != null
                || properties.CurrencyUsage != null
                || AffixUtils.HasCurrencySymbols(properties.PositivePrefixPattern)
                || AffixUtils.HasCurrencySymbols(properties.PositiveSuffixPattern)
                || AffixUtils.HasCurrencySymbols(properties.NegativePrefixPattern)
                || AffixUtils.HasCurrencySymbols(properties.NegativeSuffixPattern));
        }

        /// <summary>
        /// Updates the property bag with settings from the given pattern.
        /// </summary>
        /// <param name="pattern">The pattern string to parse.</param>
        /// <param name="ignoreRounding">Whether to leave out rounding information (minFrac, maxFrac, and rounding
        /// increment) when parsing the pattern. This may be desirable if a custom rounding mode, such
        /// as CurrencyUsage, is to be used instead. One of <see cref="PatternStringParser.IGNORE_ROUNDING_ALWAYS"/>,
        /// <see cref="PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY"/>, or <see cref="PatternStringParser.IGNORE_ROUNDING_NEVER"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
        /// <seealso cref="PatternStringParser.ParseToExistingProperties(string, DecimalFormatProperties, int)"/>
        internal void SetPropertiesFromPattern(string pattern, int ignoreRounding) // ICU4N TODO: API Convert ignoreRounding to an enum
        {
            if (pattern is null)
                throw new ArgumentNullException(nameof(pattern));

            PatternStringParser.ParseToExistingProperties(pattern, properties, ignoreRounding);
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual void SetProperties(IPropertySetter func) // ICU4N specific - marked internal instead of public
        {
            lock (this)
            {
                func.Set(properties);
                RefreshFormatter();
            }
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal interface IPropertySetter // ICU4N specific - marked internal instead of public
        {
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public void Set(DecimalFormatProperties props);
        }

        // ICU4N specific - moved pad position constants to new enum named PadPosition and de-nested.
    }
}
