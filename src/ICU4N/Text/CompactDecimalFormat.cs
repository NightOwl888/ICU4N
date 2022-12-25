using ICU4N.Globalization;
using ICU4N.Numerics;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The <see cref="CompactDecimalFormat"/> produces abbreviated numbers, suitable for display in environments will
    /// limited real estate. For example, 'Hits: 1.2B' instead of 'Hits: 1,200,000,000'. The format will
    /// be appropriate for the given language, such as "1,2 Mrd." for German.
    /// <para/>
    /// For numbers under 1000 trillion (under 10^15, such as 123,456,789,012,345), the result will be
    /// short for supported languages. However, the result may sometimes exceed 7 characters, such as
    /// when there are combining marks or thin characters. In such cases, the visual width in fonts
    /// should still be short.
    /// <para/>
    /// By default, there are 2 significant digits. After creation, if more than three significant
    /// digits are set (with setMaximumSignificantDigits), or if a fixed number of digits are set (with
    /// setMaximumIntegerDigits or setMaximumFractionDigits), then result may be wider.
    /// <para/>
    /// The "short" style is also capable of formatting currency amounts, such as "$1.2M" instead of
    /// "$1,200,000.00" (English) or "5,3 Mio. €" instead of "5.300.000,00 €" (German). Localized data
    /// concerning longer formats is not available yet in the Unicode CLDR. Because of this, attempting
    /// to format a currency amount using the "long" style will produce a <see cref="NotSupportedException"/>.
    /// <para/>
    /// At this time, negative numbers and parsing are not supported, and will produce a
    /// <see cref="NotSupportedException"/>. Resetting the pattern prefixes or suffixes is not supported; the
    /// method calls are ignored.
    /// <para/>
    /// Note that important methods, like setting the number of decimals, will be moved up from
    /// <see cref="DecimalFormat"/> to <see cref="NumberFormat"/>.
    /// </summary>
    /// <author>markdavis</author>
    /// <stable>ICU 49</stable>
    internal class CompactDecimalFormat : DecimalFormat // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final long serialVersionUID = 4716293295276629682L;

        /// <summary>
        /// Style parameter for <see cref="CompactDecimalFormat"/>.
        /// </summary>
        /// <stable>ICU 50</stable>
        public enum CompactStyle
        {
            /// <summary>
            /// Short version, like "1.2T"
            /// </summary>
            /// <stable>ICU 50</stable>
            Short,

            /// <summary>
            /// Longer version, like "1.2 trillion", if available. May return same result as <see cref="Short"/> if not.
            /// </summary>
            /// <stable>ICU 50</stable>
            Long,
        }

        /// <summary>
        /// Creates a <see cref="CompactDecimalFormat"/> appropriate for a locale. The result may be affected by the
        /// number system in the locale, such as ar-u-nu-latn.
        /// </summary>
        /// <param name="locale">The desired locale.</param>
        /// <param name="style">The compact style.</param>
        /// <stable>ICU 50</stable>
        public static CompactDecimalFormat GetInstance(UCultureInfo locale, CompactStyle style)
        {
            return new CompactDecimalFormat(locale, style);
        }

        /// <summary>
        /// Creates a <see cref="CompactDecimalFormat"/> appropriate for a locale. The result may be affected by the
        /// number system in the locale, such as ar-u-nu-latn.
        /// </summary>
        /// <param name="locale">The desired locale.</param>
        /// <param name="style">The compact style.</param>
        /// <stable>ICU 50</stable>
        public static CompactDecimalFormat GetInstance(CultureInfo locale, CompactStyle style)
        {
            return new CompactDecimalFormat(locale.ToUCultureInfo(), style);
        }

        /// <summary>
        /// The public mechanism is <see cref="CompactDecimalFormat.GetInstance(UCultureInfo, CompactStyle)"/>.
        /// </summary>
        /// <param name="locale">The desired locale.</param>
        /// <param name="style">The compact style.</param>
        internal CompactDecimalFormat(UCultureInfo locale, CompactStyle style)
        {
            // Minimal properties: let the non-shim code path do most of the logic for us.
            symbols = DecimalFormatSymbols.GetInstance(locale);
            properties = new DecimalFormatProperties
            {
                CompactStyle = style,
                GroupingSize = -2, // do not forward grouping information
                MinimumGroupingDigits = 2,
            };
            exportedProperties = new DecimalFormatProperties();
            RefreshFormatter();
        }

        /// <summary>
        /// Parsing is currently unsupported, and throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parsePosition"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        /// <stable>ICU 49</stable>
        public override J2N.Numerics.Number Parse(string text, ParsePosition parsePosition)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Parsing is currently unsupported, and throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parsePosition"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        /// <stable>ICU 49</stable>
        public override CurrencyAmount ParseCurrency(ICharSequence text, ParsePosition parsePosition)
        {
            throw new NotSupportedException();
        }
    }
}
