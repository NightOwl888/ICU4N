using ICU4N.Globalization;
using ICU4N.Support.Globalization;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Text
{
    /// <summary>
    /// Enumerator used in <see cref="LocaleDisplayNames.GetInstance(ULocale, DialectHandling)"/>.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public enum DialectHandling
    {
        /// <summary>
        /// Use standard names when generating a locale name,
        /// e.g. en_GB displays as 'English (United Kingdom)'.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        StandardNames,

        /// <summary>
        /// Use dialect names when generating a locale name,
        /// e.g. en_GB displays as 'British English'.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        DialectNames
    }

    /// <summary>
    /// Struct-like class used to return information for constructing a UI list, each corresponding to a locale.
    /// </summary>
    /// <stable>ICU 55</stable>
    public class UiListItem
    {
        /// <summary>
        /// Returns the minimized locale for an input locale, such as sr-Cyrl → sr
        /// </summary>
        /// <stable>ICU 55</stable>
        public ULocale Minimized { get; private set; }
        /// <summary>
        /// Returns the modified locale for an input locale, such as sr → sr-Cyrl, where there is also an sr-Latn in the list
        /// </summary>
        /// <stable>ICU 55</stable>
        public ULocale Modified { get; private set; }
        /// <summary>
        /// Returns the name of the modified locale in the display locale, such as "Englisch (VS)" (for 'en-US', where the display locale is 'de').
        /// </summary>
        /// <stable>ICU 55</stable>
        public string NameInDisplayLocale { get; private set; }
        /// <summary>
        /// Returns the name of the modified locale in itself, such as "English (US)" (for 'en-US').
        /// </summary>
        /// <stable>ICU 55</stable>
        public string NameInSelf { get; private set; }

        /// <summary>
        /// Constructor, normally only called internally.
        /// </summary>
        /// <param name="minimized">Locale for an input locale.</param>
        /// <param name="modified">Modified for an input locale.</param>
        /// <param name="nameInDisplayLocale">Name of the modified locale in the display locale.</param>
        /// <param name="nameInSelf">Name of the modified locale in itself.</param>
        /// <stable>ICU 55</stable>
        public UiListItem(ULocale minimized, ULocale modified, string nameInDisplayLocale, string nameInSelf)
        {
            this.Minimized = minimized;
            this.Modified = modified;
            this.NameInDisplayLocale = nameInDisplayLocale;
            this.NameInSelf = nameInSelf;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        /// <stable>ICU 55</stable>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || !(obj is UiListItem))
            {
                return false;
            }
            UiListItem other = (UiListItem)obj;
            return NameInDisplayLocale.Equals(other.NameInDisplayLocale)
                    && NameInSelf.Equals(other.NameInSelf)
                    && Minimized.Equals(other.Minimized)
                    && Modified.Equals(other.Modified);
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <stable>ICU 55</stable>
        public override int GetHashCode()
        {
            return Modified.GetHashCode() ^ NameInDisplayLocale.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <stable>ICU 55</stable>
        public override string ToString()
        {
            return "{" + Minimized + ", " + Modified + ", " + NameInDisplayLocale + ", " + NameInSelf + "}";
        }

        /// <summary>
        /// Return a comparer that compares the locale names for the display locale or the in-self names,
        /// depending on an input parameter.
        /// </summary>
        /// <param name="comparer">The string <see cref="IComparer{T}"/> to order the <see cref="UiListItem"/>s.</param>
        /// <param name="inSelf">If true, compares the nameInSelf, otherwise the <see cref="NameInDisplayLocale"/>.</param>
        /// <returns><see cref="UiListItem"/> comparer.</returns>
        /// <stable>ICU 55</stable>
        public static IComparer<UiListItem> GetComparer(IComparer<string> comparer, bool inSelf)
        {
            return new UiListItemComparer(comparer, inSelf);
        }

        /// <summary>
        /// Return a comparator that compares the locale names for the display locale or the in-self names,
        /// depending on an input parameter.
        /// </summary>
        /// <param name="comparer">The string <see cref="IComparer{T}"/> to order the <see cref="UiListItem"/>s.</param>
        /// <param name="inSelf">If true, compares the nameInSelf, otherwise the <see cref="NameInDisplayLocale"/>.</param>
        /// <returns><see cref="UiListItem"/> comparer.</returns>
        /// <stable>ICU4N 60</stable>
        public static IComparer<UiListItem> GetComparer(CompareInfo comparer, bool inSelf) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
        {
            return new UiListItemComparer(comparer, inSelf);
        }

        private class UiListItemComparer : IComparer<UiListItem>
        {
            private readonly IComparer<string> collator;
            private readonly bool useSelf;

            internal UiListItemComparer(CompareInfo collator, bool useSelf) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
                : this(collator.AsComparer(), useSelf)
            {
            }

            internal UiListItemComparer(IComparer<string> collator, bool useSelf)
            {
                this.collator = collator;
                this.useSelf = useSelf;
            }

            public int Compare(UiListItem o1, UiListItem o2)
            {
                int result = useSelf ? collator.Compare(o1.NameInSelf, o2.NameInSelf)
                        : collator.Compare(o1.NameInDisplayLocale, o2.NameInDisplayLocale);
                return result != 0 ? result : o1.Modified.CompareTo(o2.Modified); // just in case
            }
        }
    }

    /// <summary>
    /// Returns display names of ULocales and components of ULocales. For
    /// more information on language, script, region, variant, key, and
    /// values, see <see cref="Util.ULocale"/>.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public abstract class LocaleDisplayNames
    {
        // ICU4N specific - de-nested DialectHandling

        // factory methods
        /// <summary>
        /// Convenience overload of <see cref="GetInstance(ULocale, DialectHandling)"/> that specifies
        /// <see cref="DialectHandling.StandardNames"/> dialect handling.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <returns>A <see cref="LocaleDisplayNames"/> instance.</returns>
        /// <stable>ICU 4.4</stable>
        public static LocaleDisplayNames GetInstance(ULocale locale)
        {
            return GetInstance(locale, DialectHandling.StandardNames);
        }

        /// <summary>
        /// Convenience overload of <see cref="GetInstance(CultureInfo, DisplayContext[])"/> that specifies
        /// <see cref="DisplayContext.StandardNames"/>.
        /// </summary>
        /// <param name="locale">The display <see cref="CultureInfo"/>.</param>
        /// <returns>A <see cref="LocaleDisplayNames"/> instance.</returns>
        /// <stable>ICU 54</stable>
        public static LocaleDisplayNames GetInstance(CultureInfo locale)
        {
            return GetInstance(ULocale.ForLocale(locale));
        }

        /// <summary>
        /// Returns an instance of <see cref="LocaleDisplayNames"/> that returns names formatted for the provided <paramref name="locale"/>,
        /// using the provided <paramref name="dialectHandling"/>.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <param name="dialectHandling">How to select names for locales.</param>
        /// <returns>A <see cref="LocaleDisplayNames"/> instance.</returns>
        /// <stable>ICU 4.4</stable>
        public static LocaleDisplayNames GetInstance(ULocale locale, DialectHandling dialectHandling)
        {
            LocaleDisplayNames result = null;
            if (FACTORY_DIALECTHANDLING != null)
            {
                try
                {
                    result = (LocaleDisplayNames)FACTORY_DIALECTHANDLING.Invoke(null,
                        new object[] { locale, dialectHandling });
                }
                catch (TargetInvocationException)
                {
                    // fall through
                }
                //catch (IllegalAccessException)
                //{
                //    // fall through
                //}
            }
            if (result == null)
            {
                result = new LastResortLocaleDisplayNames(locale, dialectHandling);
            }
            return result;
        }

        /// <summary>
        /// Returns an instance of <see cref="LocaleDisplayNames"/> that returns names formatted for the provided <paramref name="locale"/>,
        /// using the provided <see cref="DisplayContext"/> settings.
        /// </summary>
        /// <param name="locale">The display locale.</param>
        /// <param name="contexts">One or more context settings (e.g. for dialect handling, capitalization, etc.)</param>
        /// <returns>A <see cref="LocaleDisplayNames"/> instance.</returns>
        /// <stable>ICU 51</stable>
        public static LocaleDisplayNames GetInstance(ULocale locale, params DisplayContext[] contexts)
        {
            LocaleDisplayNames result = null;
            if (FACTORY_DISPLAYCONTEXT != null)
            {
                try
                {
                    result = (LocaleDisplayNames)FACTORY_DISPLAYCONTEXT.Invoke(null,
                        new object[] { locale, contexts });
                }
                catch (TargetInvocationException)
                {
                    // fall through
                }
                //catch (IllegalAccessException e)
                //{
                //    // fall through
                //}
            }
            if (result == null)
            {
                result = new LastResortLocaleDisplayNames(locale, contexts);
            }
            return result;
        }

        /// <summary>
        /// Returns an instance of <see cref="LocaleDisplayNames"/> that returns names formatted for the provided
        /// <see cref="CultureInfo"/>, using the provided <see cref="DisplayContext"/> settings.
        /// </summary>
        /// <param name="locale">The display <see cref="CultureInfo"/>.</param>
        /// <param name="contexts">One or more context settings (e.g. for dialect handling, capitalization, etc.)</param>
        /// <returns>A <see cref="LocaleDisplayNames"/> instance.</returns>
        /// <stable>ICU 54</stable>
        public static LocaleDisplayNames GetInstance(CultureInfo locale, params DisplayContext[] contexts)
        {
            return GetInstance(ULocale.ForLocale(locale), contexts);
        }

        // getters for state

        /// <summary>
        /// Returns the locale used to determine the display names. This is not necessarily the same
        /// locale passed to <see cref="GetInstance(ULocale)"/>.
        /// </summary>
        /// <returns>The display locale.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract ULocale Locale { get; } // ICU4N TODO: API - rename Culture ?

        /// <summary>
        /// Returns the dialect handling used in the display names.
        /// </summary>
        /// <returns>The dialect handling enumeration.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract DialectHandling DialectHandling { get; }

        /// <summary>
        /// Returns the current value for a specified <see cref="DisplayContextType"/>.
        /// </summary>
        /// <param name="type">The <see cref="DisplayContextType"/> whose value to return.</param>
        /// <returns>The current DisplayContext setting for the specified type.</returns>
        /// <stable>ICU 51</stable>
        public abstract DisplayContext GetContext(DisplayContextType type);

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
        /// <stable>ICU 4.4</stable>
        public abstract string LocaleDisplayName(ULocale locale);

        /// <summary>
        /// Returns the display name of the provided <paramref name="locale"/>.
        /// When no display names are available for all or portions
        /// of the original locale ID, those portions may be
        /// used directly (possibly in a more canonical form) as
        /// part of the  returned display name.
        /// </summary>
        /// <param name="locale">The locale whose display name to return.</param>
        /// <returns>The display name of the provided <paramref name="locale"/>.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string LocaleDisplayName(CultureInfo locale);

        /// <summary>
        /// Returns the display name of the provided <paramref name="localeId"/>.
        /// When no display names are available for all or portions
        /// of the original locale ID, those portions may be
        /// used directly (possibly in a more canonical form) as
        /// part of the  returned display name.
        /// </summary>
        /// <param name="localeId">The id of the locale whose display name to return.</param>
        /// <returns>The display name of the provided locale.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string LocaleDisplayName(string localeId);

        // names for components of a locale id

        /// <summary>
        /// Returns the display name of the provided language code.
        /// </summary>
        /// <param name="lang">The language code.</param>
        /// <returns>The display name of the provided language code.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string LanguageDisplayName(string lang);

        /// <summary>
        /// Returns the display name of the provided script code.
        /// </summary>
        /// <param name="script">The script code.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string ScriptDisplayName(string script);

        /// <summary>
        /// Returns the display name of the provided script code
        /// when used in the context of a full locale name.
        /// </summary>
        /// <param name="script">The script code.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <stable>ICU 49</stable>
        [Obsolete("This API is ICU internal only.")]
        public virtual string ScriptDisplayNameInContext(string script)
        {
            return ScriptDisplayName(script);
        }

        /// <summary>
        /// Returns the display name of the provided script code.  See
        /// <see cref="UScript"/> for recognized script codes.
        /// </summary>
        /// <param name="scriptCode">The script code number.</param>
        /// <returns>The display name of the provided script code.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string ScriptDisplayName(int scriptCode);
 
        /// <summary>
        /// Returns the display name of the provided region code.
        /// </summary>
        /// <param name="region">The region code.</param>
        /// <returns>The display name of the provided region code.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string RegionDisplayName(string region);

        /// <summary>
        /// Returns the display name of the provided variant.
        /// </summary>
        /// <param name="variant">The variant string.</param>
        /// <returns>The display name of the provided variant.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string VariantDisplayName(string variant);

        /// <summary>
        /// Returns the display name of the provided locale key.
        /// </summary>
        /// <param name="key">The locale key name.</param>
        /// <returns>The display name of the provided locale key.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string KeyDisplayName(string key);

        /// <summary>
        /// Returns the display name of the provided value (used with the provided key).
        /// </summary>
        /// <param name="key">The locale key name.</param>
        /// <param name="value">The locale key's value.</param>
        /// <returns>The display name of the provided value.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract string KeyValueDisplayName(string key, string value);

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names.
        /// </summary>
        /// <param name="localeSet">A list of locales to present in a UI list. The casing uses the settings in the <see cref="LocaleDisplayNames"/> instance.</param>
        /// <param name="inSelf">
        /// If true, compares the nameInSelf, otherwise the nameInDisplayLocale.
        /// Set depending on which field (displayLocale vs self) is to show up in the UI.
        /// If both are to show up in the UI, then it should be the one used for the primary sort order.
        /// </param>
        /// <param name="collator">How to collate—should normally be <c>Collator.GetInstance(GetDisplayLocale())</c>.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="IllformedLocaleException">If any of the locales in localeSet are malformed.</exception>
        /// <stable>ICU 55</stable>
        public virtual IList<UiListItem> GetUiList(ISet<ULocale> localeSet, bool inSelf, IComparer<string> collator) // ICU4N specific - changed from IComparer<object> to IComparer<string>
        {
            return GetUiListCompareWholeItems(localeSet, UiListItem.GetComparer(collator, inSelf));
        }

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names.
        /// </summary>
        /// <param name="localeSet">A list of locales to present in a UI list. The casing uses the settings in the <see cref="LocaleDisplayNames"/> instance.</param>
        /// <param name="inSelf">
        /// If true, compares the nameInSelf, otherwise the nameInDisplayLocale.
        /// Set depending on which field (displayLocale vs self) is to show up in the UI.
        /// If both are to show up in the UI, then it should be the one used for the primary sort order.
        /// </param>
        /// <param name="collator">How to collate—should normally be <c>Collator.GetInstance(GetDisplayLocale())</c>.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="IllformedLocaleException">If any of the locales in localeSet are malformed.</exception>
        /// <draft>ICU4N 60</draft>
        public virtual IList<UiListItem> GetUiList(ISet<ULocale> localeSet, bool inSelf, CompareInfo collator) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
        {
            return GetUiListCompareWholeItems(localeSet, UiListItem.GetComparer(collator, inSelf));
        }

        /// <summary>
        /// Return a list of information used to construct a UI list of locale names, providing more access to control the sorting.
        /// Normally use <see cref="GetUiList(ISet{ULocale}, bool, IComparer{string})"/> instead.
        /// </summary>
        /// <param name="localeSet">A list of locales to present in a UI list. The casing uses the settings in the <see cref="LocaleDisplayNames"/> instance.</param>
        /// <param name="comparer">How to sort the UiListItems in the result.</param>
        /// <returns>An ordered list of <see cref="UiListItem"/>s.</returns>
        /// <exception cref="IllformedLocaleException">If any of the locales in localeSet are malformed.</exception>
        /// <stable>ICU 55</stable>
        public abstract IList<UiListItem> GetUiListCompareWholeItems(ISet<ULocale> localeSet, IComparer<UiListItem> comparer);

        // ICU4N specific - de-nested UiListItem

        /// <summary>
        /// Sole constructor.  (For invocation by subclass constructors,
        /// typically implicit.)
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal LocaleDisplayNames() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
        }

        // ICU4N: Load class name only once for both methods below
        private static readonly string implClassName = Impl.ICUConfig.Get("ICU4N.Text.LocaleDisplayNames.impl", "ICU4N.Impl.LocaleDisplayNamesImpl, ICU4N");
        // ICU4N: Avoid static constructor by initializing inline
        private static readonly MethodInfo FACTORY_DIALECTHANDLING = LoadFactoryDialectHandling();
        private static readonly MethodInfo FACTORY_DISPLAYCONTEXT = LoadFactoryDisplayContext();

        private static MethodInfo LoadFactoryDialectHandling()
        {
            MethodInfo factoryDialectHandling = null;

            try
            {
                Type implClass = Type.GetType(implClassName);

                if (implClass == null)
                    return null;

                // ICU4N NOTE: GetMethod() doesn't throw an exception if the method
                // is not found in .NET.
                factoryDialectHandling = implClass.GetMethod("GetInstance",
                    new Type[] { typeof(ULocale), typeof(DialectHandling) });
            }
            catch (TypeLoadException)
            {
                // fallback to last resort impl
            }

            return factoryDialectHandling;
        }
        private static MethodInfo LoadFactoryDisplayContext()
        {
            MethodInfo factoryDisplayContext = null;

            try
            {
                Type implClass = Type.GetType(implClassName);

                if (implClass == null)
                    return null;

                // ICU4N NOTE: GetMethod() doesn't throw an exception if the method
                // is not found in .NET.
                factoryDisplayContext = implClass.GetMethod("GetInstance",
                    new Type[] { typeof(ULocale), typeof(DialectHandling[]) });
            }
            catch (TypeLoadException)
            {
                // fallback to last resort impl
            }

            return factoryDisplayContext;
        }

        /// <summary>
        /// Minimum implementation of <see cref="LocaleDisplayNames"/>
        /// </summary>
        private class LastResortLocaleDisplayNames : LocaleDisplayNames
        {
            private ULocale locale;
            private DisplayContext[] contexts;

            internal LastResortLocaleDisplayNames(ULocale locale, DialectHandling dialectHandling)
#pragma warning disable 612, 618
                : base()
#pragma warning restore 612, 618
            {
                this.locale = locale;
                DisplayContext context = (dialectHandling == DialectHandling.DialectNames) ?
                        DisplayContext.DialectNames : DisplayContext.StandardNames;
                this.contexts = new DisplayContext[] { context };
            }

            internal LastResortLocaleDisplayNames(ULocale locale, params DisplayContext[] contexts)
#pragma warning disable 612, 618
                : base()
#pragma warning restore 612, 618
            {
                this.locale = locale;
                this.contexts = new DisplayContext[contexts.Length];
                System.Array.Copy(contexts, 0, this.contexts, 0, contexts.Length);
            }

            public override ULocale Locale => locale;

            public override DialectHandling DialectHandling
            {
                get
                {
                    DialectHandling result = DialectHandling.StandardNames;
                    foreach (DisplayContext context in contexts)
                    {
                        if (context.Type() == DisplayContextType.DialectHandling)
                        {
                            if (context.Value() == (int)DisplayContext.DialectNames)
                            {
                                result = DialectHandling.DialectNames;
                                break;
                            }
                        }
                    }
                    return result;
                }
            }

            public override DisplayContext GetContext(DisplayContextType type)
            {
                DisplayContext result = DisplayContext.StandardNames;  // final fallback
                foreach (DisplayContext context in contexts)
                {
                    if (context.Type() == type)
                    {
                        result = context;
                        break;
                    }
                }
                return result;
            }

            public override string LocaleDisplayName(ULocale locale) // ICU4N TODO: API - remove
            {
                return locale.GetName();
            }

            //public override string LocaleDisplayName(UCultureInfo locale)
            //{
            //    return locale.FullName;
            //}

            public override string LocaleDisplayName(CultureInfo locale)
            {
                return ULocale.ForLocale(locale).GetName();
            }

            public override string LocaleDisplayName(string localeId)
            {
                return new ULocale(localeId).GetName();
            }

            public override string LanguageDisplayName(string lang)
            {
                return lang;
            }

            public override string ScriptDisplayName(string script)
            {
                return script;
            }

            public override string ScriptDisplayName(int scriptCode)
            {
                return UScript.GetShortName(scriptCode);
            }

            public override string RegionDisplayName(string region)
            {
                return region;
            }

            public override string VariantDisplayName(string variant)
            {
                return variant;
            }

            public override string KeyDisplayName(string key)
            {
                return key;
            }

            public override string KeyValueDisplayName(string key, string value)
            {
                return value;
            }

            public override IList<UiListItem> GetUiListCompareWholeItems(ISet<ULocale> localeSet, IComparer<UiListItem> comparator)
            {
                return new List<UiListItem>();
            }
        }
    }
}
