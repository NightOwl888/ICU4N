using System;
using System.Collections.Generic;
using System.Globalization;
using JCG = J2N.Collections.Generic;

// Port of text.LocaleDisplayNames from ICU4J

namespace ICU4N.Globalization
{
    /// <summary>
    /// Returns display names of <see cref="UCultureInfo"/>s and components of <see cref="UCultureInfo"/>s. For
    /// more information on language, script, region, variant, key, and
    /// values, see <see cref="UCultureInfo"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    public abstract class CultureDisplayNames
    {
        // ICU4N specific - de-nested DialectHandling

        private static ICultureDisplayNamesFactory cultureDisplayNamesFactory = new DefaultCultureDisplayNamesFactory();

        /// <summary>
        /// Sets the <see cref="ICultureDisplayNamesFactory"/> instance that is used to retrieve display names.
        /// Supplying a custom factory allows data to be provided from sources other than resource files.
        /// </summary>
        /// <param name="cultureDisplayNamesFactory">The <see cref="ICultureDisplayNamesFactory"/> that
        /// provides the display name data.</param>
        /// <draft>ICU 60</draft>
        public static void SetCultureDisplayNamesFactory(ICultureDisplayNamesFactory cultureDisplayNamesFactory)
            => CultureDisplayNames.cultureDisplayNamesFactory = cultureDisplayNamesFactory ?? throw new ArgumentNullException(nameof(cultureDisplayNamesFactory));

        /// <summary>
        /// Gets the current <see cref="ICultureDisplayNamesFactory"/> instance.
        /// </summary>
        /// <returns>The current <see cref="ICultureDisplayNamesFactory"/> instance.</returns>
        /// <draft>ICU 60</draft>
        public static ICultureDisplayNamesFactory GetCultureDisplayNamesFactory()
            => cultureDisplayNamesFactory;

        // factory methods
        /// <summary>
        /// Convenience overload of <see cref="GetInstance(CultureInfo, DialectHandling)"/> that specifies
        /// <see cref="DialectHandling.StandardNames"/> dialect handling.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <returns>A <see cref="CultureDisplayNames"/> instance.</returns>
        /// <draft>ICU 60</draft>
        public static CultureDisplayNames GetInstance(CultureInfo locale)
        {
            return GetInstance(locale, DialectHandling.StandardNames);
        }

        /// <summary>
        /// Returns an instance of <see cref="CultureDisplayNames"/> that returns names formatted for the provided <paramref name="locale"/>,
        /// using the provided <paramref name="dialectHandling"/>.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <param name="dialectHandling">How to select names for locales.</param>
        /// <returns>A <see cref="CultureDisplayNames"/> instance.</returns>
        /// <draft>ICU 60</draft>
        public static CultureDisplayNames GetInstance(CultureInfo locale, DialectHandling dialectHandling)
        {
            CultureDisplayNames result = null;
            var options = new DisplayContextOptions { DialectHandling = dialectHandling }.Freeze();
            var culture = locale.ToUCultureInfo();
            if (cultureDisplayNamesFactory != null)
            {
                result = cultureDisplayNamesFactory.GetCultureDisplayNames(culture, options);
            }
            if (result == null)
            {
                result = new LastResortCultureDisplayNames(culture, options);
            }
            return result;
        }

        /// <summary>
        /// Returns an instance of <see cref="CultureDisplayNames"/> that returns names formatted for the provided <paramref name="locale"/>,
        /// using the provided <see cref="DisplayContextOptions"/> settings.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <param name="options">One or more context settings (e.g. for dialect handling, capitalization, etc.)</param>
        /// <returns>A <see cref="CultureDisplayNames"/> instance.</returns>
        /// <draft>ICU 60</draft>
        public static CultureDisplayNames GetInstance(CultureInfo locale, DisplayContextOptions options)
        {
            CultureDisplayNames result = null;
            var culture = locale.ToUCultureInfo();
            options = options.Freeze();
            if (cultureDisplayNamesFactory != null)
            {
                result = cultureDisplayNamesFactory.GetCultureDisplayNames(culture, options);
            }
            if (result == null)
            {
                result = new LastResortCultureDisplayNames(culture, options);
            }
            return result;
        }

        // getters for state

        /// <summary>
        /// Gets the locale used to determine the display names. This is not necessarily the same
        /// locale passed to <see cref="GetInstance(CultureInfo)"/>.
        /// </summary>
        /// <returns>The display locale.</returns>
        /// <draft>ICU 60</draft>
        public abstract UCultureInfo Culture { get; }

        /// <summary>
        /// Gets the display context options.
        /// </summary>
        /// <draft>ICU 60</draft>
        public abstract DisplayContextOptions DisplayContextOptions { get; }

        // names for entire locales

        /// <summary>
        /// Returns the display name of the provided <paramref name="locale"/>.
        /// When no display names are available for all or portions
        /// of the original locale ID, those portions may be
        /// used directly (possibly in a more canonical form) as
        /// part of the  returned display name.
        /// </summary>
        /// <param name="locale">The locale whose display name to return.</param>
        /// <returns>The display name of the provided <paramref name="locale"/>.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetLocaleDisplayName(CultureInfo locale);

        /// <summary>
        /// Returns the display name of the provided <paramref name="localeId"/>.
        /// When no display names are available for all or portions
        /// of the original locale ID, those portions may be
        /// used directly (possibly in a more canonical form) as
        /// part of the  returned display name.
        /// </summary>
        /// <param name="localeId">The id of the locale whose display name to return.</param>
        /// <returns>The display name of the provided locale.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetLocaleDisplayName(string localeId);

        // names for components of a locale id

        /// <summary>
        /// Returns the display name of the provided language code.
        /// </summary>
        /// <param name="language">The language code.</param>
        /// <returns>The display name of the provided language code.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetLanguageDisplayName(string language);

        /// <summary>
        /// Returns the display name of the provided script code.
        /// </summary>
        /// <param name="script">The script code.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetScriptDisplayName(string script);

        // ICU4N specific - removed ScriptDisplayNameInContext, since it is obsolete anyway

        /// <summary>
        /// Returns the display name of the provided script code
        /// when used in the context of a full locale name.
        /// </summary>
        /// <param name="script">The script code.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <stable>ICU 49</stable>
        //[Obsolete("This API is ICU internal only.")]
        internal virtual string GetScriptDisplayNameInContext(string script)
        {
            return GetScriptDisplayName(script);
        }

        /// <summary>
        /// Returns the display name of the provided script code.  See
        /// <see cref="UScript"/> for recognized script codes.
        /// </summary>
        /// <param name="scriptCode">The script code number.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetScriptDisplayName(int scriptCode);

        /// <summary>
        /// Returns the display name of the provided region code.
        /// </summary>
        /// <param name="region">The region code.</param>
        /// <returns>The display name of the provided region code.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetRegionDisplayName(string region);

        /// <summary>
        /// Returns the display name of the provided variant.
        /// </summary>
        /// <param name="variant">The variant string.</param>
        /// <returns>The display name of the provided variant.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetVariantDisplayName(string variant);

        /// <summary>
        /// Returns the display name of the provided locale key.
        /// </summary>
        /// <param name="key">The locale key name.</param>
        /// <returns>The display name of the provided locale key.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetKeyDisplayName(string key);

        /// <summary>
        /// Returns the display name of the provided value (used with the provided key).
        /// </summary>
        /// <param name="key">The locale key name.</param>
        /// <param name="value">The locale key's value.</param>
        /// <returns>The display name of the provided value.</returns>
        /// <draft>ICU 60</draft>
        public abstract string GetKeyValueDisplayName(string key, string value);

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names.
        /// </summary>
        /// <param name="cultures">A list of locales to present in a UI list. The casing uses the settings in the <see cref="CultureDisplayNames"/> instance.</param>
        /// <param name="inSelf">
        /// If <c>true</c>, compares the <see cref=""/>nameInSelf, otherwise the nameInDisplayLocale.
        /// Set depending on which field (displayLocale vs self) is to show up in the UI.
        /// If both are to show up in the UI, then it should be the one used for the primary sort order.
        /// </param>
        /// <param name="collator">How to collate—should normally be <c>Collator.GetInstance(GetDisplayLocale())</c>.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="CultureNotFoundException">If any of the locales in <paramref name="cultures"/> are malformed.</exception>
        /// <draft>ICU 60</draft>
        public virtual IList<UiListItem> GetUiList(ICollection<CultureInfo> cultures, bool inSelf, IComparer<string> collator) // ICU4N specific - changed from IComparer<object> to IComparer<string>
        {
            return GetUiListCompareWholeItems(cultures, UiListItem.GetComparer(collator, inSelf));
        }

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names.
        /// </summary>
        /// <param name="cultures">A list of locales to present in a UI list. The casing uses the settings in the <see cref="CultureDisplayNames"/> instance.</param>
        /// <param name="inSelf">
        /// If true, compares the nameInSelf, otherwise the nameInDisplayLocale.
        /// Set depending on which field (displayLocale vs self) is to show up in the UI.
        /// If both are to show up in the UI, then it should be the one used for the primary sort order.
        /// </param>
        /// <param name="collator">How to collate—should normally be <c>Collator.GetInstance(GetDisplayLocale())</c>.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="CultureNotFoundException">If any of the locales in <paramref name="cultures"/> are malformed.</exception>
        /// <draft>ICU 60</draft>
        public virtual IList<UiListItem> GetUiList(ICollection<CultureInfo> cultures, bool inSelf, CompareInfo collator) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
        {
            return GetUiListCompareWholeItems(cultures, UiListItem.GetComparer(collator, inSelf));
        }

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names, providing more access to control the sorting.
        /// Normally use <see cref="GetUiList(ICollection{CultureInfo}, bool, IComparer{string})"/> instead.
        /// </summary>
        /// <param name="cultures">A list of locales to present in a UI list. The casing uses the settings in the <see cref="CultureDisplayNames"/> instance.</param>
        /// <param name="comparer">How to sort the UiListItems in the result.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="CultureNotFoundException">If any of the locales in <paramref name="cultures"/> are malformed.</exception>
        /// <draft>ICU 60</draft>
        public abstract IList<UiListItem> GetUiListCompareWholeItems(ICollection<CultureInfo> cultures, IComparer<UiListItem> comparer);

        // ICU4N specific - de-nested UiListItem

        /// <summary>
        /// Sole constructor.  (For invocation by subclass constructors,
        /// typically implicit.)
        /// </summary>
        /// <internal/>
        //[Obsolete("This API is ICU internal only.")]
        internal CultureDisplayNames() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
        }

        /// <summary>
        /// Minimum implementation of <see cref="CultureDisplayNames"/>
        /// </summary>
        private class LastResortCultureDisplayNames : CultureDisplayNames
        {
            private readonly UCultureInfo culture;
            private readonly DisplayContextOptions options;

            internal LastResortCultureDisplayNames(UCultureInfo culture, DisplayContextOptions options)
#pragma warning disable 612, 618
                : base()
#pragma warning restore 612, 618
            {
                this.culture = culture;
                this.options = options;
            }

            public override UCultureInfo Culture => culture;

            public override DisplayContextOptions DisplayContextOptions => options;

            public override string GetLocaleDisplayName(CultureInfo locale)
            {
                return locale.ToUCultureInfo().FullName;
            }

            public override string GetLocaleDisplayName(string localeId)
            {
                return new UCultureInfo(localeId).FullName;
            }

            public override string GetLanguageDisplayName(string language)
            {
                return language;
            }

            public override string GetScriptDisplayName(string script)
            {
                return script;
            }

            public override string GetScriptDisplayName(int scriptCode)
            {
                return UScript.GetShortName(scriptCode);
            }

            public override string GetRegionDisplayName(string region)
            {
                return region;
            }

            public override string GetVariantDisplayName(string variant)
            {
                return variant;
            }

            public override string GetKeyDisplayName(string key)
            {
                return key;
            }

            public override string GetKeyValueDisplayName(string key, string value)
            {
                return value;
            }

            public override IList<UiListItem> GetUiListCompareWholeItems(ICollection<CultureInfo> cultures, IComparer<UiListItem> comparator)
            {
                return new JCG.List<UiListItem>();
            }
        }
    }
}
