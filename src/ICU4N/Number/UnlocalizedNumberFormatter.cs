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
        /** Base constructor; called during startup only. Sets the threshold to the default value of 3. */
        internal UnlocalizedNumberFormatter()
            : base(null, KEY_THRESHOLD, Long.GetInstance(3))
        {
        }

        internal UnlocalizedNumberFormatter(NumberFormatterSettings/*<?>*/ parent, int key, object value)
            : base(parent, key, value)
        {
        }

        /**
         * Associate the given locale with the number formatter. The locale is used for picking the appropriate symbols,
         * formats, and other data for number display.
         *
         * <p>
         * To use the Java default locale, call Locale.getDefault():
         *
         * <pre>
         * NumberFormatter.with(). ... .locale(Locale.getDefault())
         * </pre>
         *
         * @param locale
         *            The locale to use when loading data for number formatting.
         * @return The fluent chain
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         */
        public LocalizedNumberFormatter Culture(CultureInfo locale)
        {
            return new LocalizedNumberFormatter(this, KEY_LOCALE, locale.ToUCultureInfo());
        }

        /**
         * ULocale version of the {@link #locale(Locale)} setter above.
         *
         * @param locale
         *            The locale to use when loading data for number formatting.
         * @return The fluent chain
         * @see #locale(Locale)
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         */
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
