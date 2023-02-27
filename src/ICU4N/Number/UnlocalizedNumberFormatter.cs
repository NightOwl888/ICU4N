using ICU4N.Globalization;
using System.Globalization;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A <see cref="NumberFormatter"/> that does not yet have a locale. In order to format numbers, a locale must be specified.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class UnlocalizedNumberFormatter : NumberFormatterSettings<UnlocalizedNumberFormatter> // ICU4N TODO: API - this was public in ICU4J
    {
        /// <summary>
        /// Base constructor; called during startup only. Sets the threshold to the default value of 3.
        /// </summary>
        internal UnlocalizedNumberFormatter()
            : base(null, KEY_THRESHOLD, Long.GetInstance(3))
        {
        }

        internal UnlocalizedNumberFormatter(NumberFormatterSettings/*<?>*/ parent, int key, object value)
            : base(parent, key, value)
        {
        }

        /// <summary>
        /// Associate the given locale with the number formatter. The locale is used for picking the appropriate symbols,
        /// formats, and other data for number display.
        /// 
        /// <para/>
        /// To use <see cref="CultureInfo.CurrentCulture"/>, use <see cref="CultureInfo.CurrentCulture"/>.
        /// 
        /// <code>
        /// NumberFormatter.With(). ... .Culture(CultureInfo.CurrentCulture)
        /// </code>
        /// </summary>
        /// <param name="locale">The locale to use when loading data for number formatting.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public LocalizedNumberFormatter Culture(CultureInfo locale)
        {
            return new LocalizedNumberFormatter(this, KEY_LOCALE, locale.ToUCultureInfo());
        }

        /// <summary>
        /// <see cref="UCultureInfo"/> version of the <see cref="Culture(CultureInfo)"/> setter above.
        /// </summary>
        /// <param name="locale">The locale to use when loading data for number formatting.</param>
        /// <returns>The fluent chain.</returns>
        /// <seealso cref="Culture(CultureInfo)"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public LocalizedNumberFormatter Culture(UCultureInfo locale)
        {
            return new LocalizedNumberFormatter(this, KEY_LOCALE, locale);
        }

        internal override UnlocalizedNumberFormatter Create(int key, object value)
        {
            return new UnlocalizedNumberFormatter(this, key, value);
        }
    }
}
