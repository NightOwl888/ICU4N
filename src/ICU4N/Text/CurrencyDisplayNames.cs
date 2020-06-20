using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Text
{
    /// <summary>
    /// Returns currency names localized for a locale.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public abstract class CurrencyDisplayNames
    {
        /// <summary>
        /// Return an instance of <see cref="CurrencyDisplayNames"/> that provides information
        /// localized for display in the provided locale.  If there is no data for the
        /// provided locale, this falls back to the current default locale; if there
        /// is no data for that either, it falls back to the root locale.  Substitute
        /// values are returned from APIs when there is no data for the requested ISO
        /// code.
        /// </summary>
        /// <param name="locale">The locale into which to localize the names.</param>
        /// <returns>A <see cref="CurrencyDisplayNames"/>.</returns>
        /// <stable>ICU 4.4</stable>
        public static CurrencyDisplayNames GetInstance(ULocale locale)
        {
            return CurrencyData.Provider.GetInstance(locale, true);
        }

        /// <summary>
        /// Return an instance of CurrencyDisplayNames that provides information
        /// localized for display in the provided locale.  If there is no data for the
        /// provided locale, this falls back to the current default locale; if there
        /// is no data for that either, it falls back to the root locale.  Substitute
        /// values are returned from APIs when there is no data for the requested ISO
        /// code.
        /// </summary>
        /// <param name="locale">The locale into which to localize the names.</param>
        /// <returns>A CurrencyDisplayNames.</returns>
        /// <stable>ICU 54</stable>
        public static CurrencyDisplayNames GetInstance(CultureInfo locale)
        {
            return GetInstance(locale, true);
        }

        /// <summary>
        /// Return an instance of <see cref="CurrencyDisplayNames"/> that provides information
        /// localized for display in the provided locale.  If noSubstitute is false,
        /// this behaves like <see cref="GetInstance(ULocale)"/>.  Otherwise, 1) if there
        /// is no supporting data for the locale at all, there is no fallback through
        /// the default locale or root, and null is returned, and 2) if there is data
        /// for the locale, but not data for the requested ISO code, null is returned
        /// from those APIs instead of a substitute value.
        /// </summary>
        /// <param name="locale">The locale into which to localize the names.</param>
        /// <param name="noSubstitute">If true, do not return substitute values.</param>
        /// <returns>A <see cref="CurrencyDisplayNames"/>.</returns>
        /// <stable>ICU 49</stable>
        public static CurrencyDisplayNames GetInstance(ULocale locale, bool noSubstitute)
        {
            return CurrencyData.Provider.GetInstance(locale, !noSubstitute);
        }

        /// <summary>
        /// Return an instance of CurrencyDisplayNames that provides information
        /// localized for display in the provided locale.  If noSubstitute is false,
        /// this behaves like <see cref="GetInstance(CultureInfo)"/>.  Otherwise, 1) if there
        /// is no supporting data for the locale at all, there is no fallback through
        /// the default locale or root, and null is returned, and 2) if there is data
        /// for the locale, but not data for the requested ISO code, null is returned
        /// from those APIs instead of a substitute value.
        /// </summary>
        /// <param name="locale">The <see cref="CultureInfo"/> into which to localize the names.</param>
        /// <param name="noSubstitute">If true, do not return substitute values.</param>
        /// <returns>A <see cref="CurrencyDisplayNames"/>.</returns>
        /// <stable>ICU 54</stable>
        public static CurrencyDisplayNames GetInstance(CultureInfo locale, bool noSubstitute)
        {
            return GetInstance(ULocale.ForLocale(locale), noSubstitute);
        }

        /// <summary>
        /// Returns true if currency display name data is available.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool HasData
        {
            get { return CurrencyData.Provider.HasData; }
        }

        /// <summary>
        /// Returns the locale used to determine how to translate the currency names.
        /// This is not necessarily the same locale passed to <see cref="GetInstance(ULocale)"/>.
        /// </summary>
        /// <stable>ICU 49</stable>
        public abstract ULocale ULocale { get; } // ICU4N TODO: API Remove

        /// <summary>
        /// Returns the locale used to determine how to translate the currency names.
        /// This is not necessarily the same locale passed to <see cref="GetInstance(ULocale)"/>.
        /// </summary>
        /// <stable>ICU 49</stable>
        public abstract UCultureInfo UCultureInfo { get; }

        /// <summary>
        /// Returns the symbol for the currency with the provided ISO code.  If
        /// there is no data for the ISO code, substitutes isoCode or returns null.
        /// </summary>
        /// <param name="isoCode">The three-letter ISO code.</param>
        /// <returns>The display name.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string GetSymbol(string isoCode);

        /// <summary>
        /// Returns the 'long name' for the currency with the provided ISO code.
        /// If there is no data for the ISO code, substitutes isoCode or returns null.
        /// </summary>
        /// <param name="isoCode">The three-letter ISO code.</param>
        /// <returns>The display name.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string GetName(string isoCode);

        /// <summary>
        /// Returns a 'plural name' for the currency with the provided ISO code corresponding to
        /// the <paramref name="pluralKey"/>.  If there is no data for the ISO code, substitutes <paramref name="isoCode"/> or
        /// returns null.  If there is data for the ISO code but no data for the plural key,
        /// substitutes the 'other' value (and failing that the <paramref name="isoCode"/>) or returns null.
        /// </summary>
        /// <param name="isoCode">The three-letter ISO code.</param>
        /// <param name="pluralKey">The plural key, for example "one", "other".</param>
        /// <returns>The display name.</returns>
        /// <seealso cref="PluralRules"/>
        /// <stable>ICU 4.4</stable>
        public abstract string GetPluralName(string isoCode, string pluralKey);

        /// <summary>
        /// Returns a mapping from localized symbols and currency codes to currency codes.
        /// The returned map is unmodifiable.
        /// </summary>
        /// <returns>The map.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract IDictionary<string, string> SymbolMap { get; }

        /// <summary>
        /// Returns a mapping from localized names (standard and plural) to currency codes.
        /// The returned map is unmodifiable.
        /// </summary>
        /// <returns>The map.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract IDictionary<string, string> NameMap { get; }

        /// <summary>
        /// Sole constructor.  (For invocation by subclass constructors,
        /// typically implicit.)
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        internal CurrencyDisplayNames() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
        }
    }
}
