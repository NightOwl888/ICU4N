using ICU4N.Globalization;
using ICU4N.Text;
using System;
using System.Globalization;

namespace ICU4N.Numerics
{
    /// <summary>
    /// The main entrypoint to the localized number formatting library introduced in ICU 60. Basic usage examples:
    /// 
    /// <code>
    /// // Most basic usage:
    /// NumberFormatter.WithCulture(...).Format(123).ToString();  // 1,234 in en-US
    /// 
    /// // Custom notation, unit, and rounding strategy:
    /// NumberFormatter.With()
    ///     .Notation(Notation.CompactShort)
    ///     .Unit(Currency.GetInstance("EUR"))
    ///     .Rounding(Rounder.MaxDigits(2))
    ///     .Culture(...)
    ///     .Format(1234)
    ///     .ToString();  // €1.2K in en-US
    /// 
    /// // Create a formatter in a private static final field:
    /// private static readonly LocalizedNumberFormatter formatter = NumberFormatter.WithCulture(...)
    ///     .Unit(NoUnit.Percent)
    ///     .Rounding(Rounder.FixedFraction(3));
    /// formatter.Format(5.9831).ToString();  // 5.983% in en-US
    /// 
    /// // Create a "template" in a private static final field but without setting a culture until the call site:
    /// private static readonly UnlocalizedNumberFormatter template = NumberFormatter.With()
    ///     .Sign(SignDisplay.Always)
    ///     .UnitWidth(UnitWidth.FullName);
    /// template.Culture(...).Format(new Measure(1234, MeasureUnit.Meter)).ToString();  // +1,234 meters in en-US
    /// </code>
    /// <para/>
    /// This API offers more features than <see cref="DecimalFormat"/> and is geared toward new users of ICU.
    /// <para/>
    /// <see cref="NumberFormatter"/> instances are immutable and thread safe. This means that invoking a configuration method has no
    /// effect on the receiving instance; you must store and use the new number formatter instance it returns instead.
    /// <code>
    /// UnlocalizedNumberFormatter formatter = UnlocalizedNumberFormatter.With().Notation(Notation.Scientific);
    /// formatter.Rounding(Rounder.MaxFraction(2)); // does nothing!
    /// formatter.Culture(new UCultureInfo("en")).Format(9.8765).ToString(); // prints "9.8765E0", not "9.88E0"
    /// </code>
    /// <para/>
    /// This API is based on the <em>fluent</em> design pattern popularized by libraries such as Google's Guava. For
    /// extensive details on the design of this API, read <a href="https://goo.gl/szi5VB">the design doc</a>.
    /// </summary>
    /// <author>Shane Carr</author>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    internal static class NumberFormatter
    {
        private static readonly UnlocalizedNumberFormatter BASE = new UnlocalizedNumberFormatter();

        /// <summary>
        /// An enum declaring how to render units, including currencies. Example outputs when formatting 123 USD and 123
        /// meters in <em>en-CA</em>:
        /// 
        /// <list type="bullet">
        ///     <item><term><see cref="Narrow"/></term><description>"$123.00" and "123 m"</description></item>
        ///     <item><term><see cref="Short"/></term><description>"US$ 123.00" and "123 m"</description></item>
        ///     <item><term><see cref="FullName"/></term><description>"123.00 US dollars" and "123 meters"</description></item>
        ///     <item><term><see cref="ISOCode"/></term><description>"USD 123.00" and undefined behavior</description></item>
        ///     <item><term><see cref="Hidden"/></term><description>"123.00" and "123"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// This enum is similar to <see cref="MeasureFormat.FormatWidth"/>.
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <seealso cref="NumberFormatter"/>
        public enum UnitWidth
        {
            /// <summary>
            /// Print an abbreviated version of the unit name. Similar to <see cref="Short"/>, but always use the shortest available
            /// abbreviation or symbol. This option can be used when the context hints at the identity of the unit. For more
            /// information on the difference between <see cref="Narrow"/> and <see cref="Short"/>, see <see cref="Short"/>.
            /// 
            /// <para/>
            /// In CLDR, this option corresponds to the "Narrow" format for measure units and the "¤¤¤¤¤" placeholder for
            /// currencies.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Narrow,

            /// <summary>
            /// Print an abbreviated version of the unit name. Similar to <see cref="Narrow"/>, but use a slightly wider abbreviation or
            /// symbol when there may be ambiguity. This is the default behavior.
            /// 
            /// <para/>
            /// For example, in <em>es-US</em>, the <see cref="Short"/> form for Fahrenheit is "{0} °F", but the NARROW form is "{0}°",
            /// since Fahrenheit is the customary unit for temperature in that locale.
            /// 
            /// <para/>
            /// In CLDR, this option corresponds to the "Short" format for measure units and the "¤" placeholder for
            /// currencies.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Short,

            /// <summary>
            /// Print the full name of the unit, without any abbreviations.
            /// 
            /// <para/>
            /// In CLDR, this option corresponds to the default format for measure units and the "¤¤¤" placeholder for
            /// currencies.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            FullName,

            /// <summary>
            /// Use the three-digit ISO XXX code in place of the symbol for displaying currencies. The behavior of this
            /// option is currently undefined for use with measure units.
            /// 
            /// <para/>
            /// In CLDR, this option corresponds to the "¤¤" placeholder for currencies.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            ISOCode,

            /// <summary>
            /// Format the number according to the specified unit, but do not display the unit. For currencies, apply
            /// monetary symbols and formats as with <see cref="Short"/>, but omit the currency symbol. For measure units, the behavior is
            /// equivalent to not specifying the unit at all.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Hidden,
        }

        /// <summary>
        /// An enum declaring how to denote positive and negative numbers. Example outputs when formatting 123 and -123 in
        /// <em>en-US</em>:
        /// 
        /// <list type="bullet">
        ///     <item><term><see cref="Auto"/></term><description>"123" and "-123"</description></item>
        ///     <item><term><see cref="Always"/></term><description>"+123" and "-123"</description></item>
        ///     <item><term><see cref="Never"/></term><description>"123" and "123"</description></item>
        ///     <item><term><see cref="Accounting"/></term><description>"$123" and "($123)"</description></item>
        ///     <item><term><see cref="AccountingAlways"/></term><description>"+$123" and "($123)"</description></item>
        /// </list>
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <seealso cref="NumberFormatter"/>
        public enum SignDisplay
        {
            /// <summary>
            /// Show the minus sign on negative numbers, and do not show the sign on positive numbers. This is the default
            /// behavior.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Auto,

            /// <summary>
            /// Show the minus sign on negative numbers and the plus sign on positive numbers.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Always,

            /// <summary>
            /// Do not show the sign on positive or negative numbers.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Never,

            /// <summary>
            /// Use the locale-dependent accounting format on negative numbers, and do not show the sign on positive numbers.
            /// 
            /// <para/>
            /// The accounting format is defined in CLDR and varies by locale; in many Western locales, the format is a pair
            /// of parentheses around the number.
            /// 
            /// <para/>
            /// Note: Since CLDR defines the accounting format in the monetary context only, this option falls back to the
            /// <see cref="Auto"/> sign display strategy when formatting without a currency unit. This limitation may be lifted in the
            /// future.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Accounting,

            /// <summary>
            /// Use the locale-dependent accounting format on negative numbers, and show the plus sign on positive numbers.
            /// For more information on the accounting format, see the <see cref="Accounting"/> sign display strategy.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            AccountingAlways,
        }

        /// <summary>
        /// An enum declaring how to render the decimal separator. Example outputs when formatting 1 and 1.1 in
        /// <em>en-US</em>:
        /// 
        /// <list type="bullet">
        ///     <item><term><see cref="Auto"/></term><description>"1" and "1.1"</description></item>
        ///     <item><term><see cref="Always"/></term><description>"1." and "1.1"</description></item>
        /// </list>
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <seealso cref="NumberFormatter"/>
        public enum DecimalSeparatorDisplay
        {
            /// <summary>
            /// Show the decimal separator when there are one or more digits to display after the separator, and do not show
            /// it otherwise. This is the default behavior.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Auto,

            /// <summary>
            /// Always show the decimal separator, even if there are no digits to display after the separator.
            /// </summary>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NumberFormatter"/>
            Always,
        }

        /// <summary>
        /// Use a default threshold of 3. This means that the third time .Format() is called, the data structures get built
        /// using the "safe" code path. The first two calls to .Format() will trigger the unsafe code path.
        /// </summary>
        internal const long DEFAULT_THRESHOLD = 3;

        // ICU4N specific - made the class static instead of using a private constructor

        /// <summary>
        /// Call this method at the beginning of a <see cref="NumberFormatter"/> fluent chain in which the locale is not currently known at
        /// the call site.
        /// </summary>
        /// <returns>An <see cref="UnlocalizedNumberFormatter"/>, to be used for chaining.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnlocalizedNumberFormatter With()
        {
            return BASE;
        }

        /// <summary>
        /// Call this method at the beginning of a <see cref="NumberFormatter"/> fluent chain in which the locale is known at the call
        /// site.
        /// </summary>
        /// <param name="locale">The locale from which to load formats and symbols for number formatting.</param>
        /// <returns>A <see cref="LocalizedNumberFormatter"/>, to be used for chaining.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static LocalizedNumberFormatter WithCulture(CultureInfo locale)
        {
            return BASE.Culture(locale);
        }

        /// <summary>
        /// Call this method at the beginning of a NumberFormatter fluent chain in which the locale is known at the call
        /// site.
        /// </summary>
        /// <param name="locale">The locale from which to load formats and symbols for number formatting.</param>
        /// <returns>A <see cref="LocalizedNumberFormatter"/>, to be used for chaining.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static LocalizedNumberFormatter WithCulture(UCultureInfo locale)
        {
            return BASE.Culture(locale);
        }

        /// <internal/>
        [Obsolete("ICU 60 This API is ICU internal only.")]
        public static UnlocalizedNumberFormatter FromDecimalFormat(DecimalFormatProperties properties,
            DecimalFormatSymbols symbols, DecimalFormatProperties exportedProperties)
        {
            MacroProps macros = NumberPropertyMapper.OldToNew(properties, symbols, exportedProperties);
            return NumberFormatter.With().Macros(macros);
        }
    }
}
