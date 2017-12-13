using ICU4N.Lang;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Returns display names of ULocales and components of ULocales. For
    /// more information on language, script, region, variant, key, and
    /// values, see <see cref="Util.ULocale"/>.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public abstract class LocaleDisplayNames
    {
        /**
     * Enum used in {@link #getInstance(ULocale, DialectHandling)}.
     * @stable ICU 4.4
     */
        public enum DialectHandling // ICU4N TODO: API - de-nest and rename LocaleDisplayNamesDialectHandling
        {
            /**
             * Use standard names when generating a locale name,
             * e.g. en_GB displays as 'English (United Kingdom)'.
             * @stable ICU 4.4
             */
            STANDARD_NAMES,
            /**
             * Use dialect names when generating a locale name,
             * e.g. en_GB displays as 'British English'.
             * @stable ICU 4.4
             */
            DIALECT_NAMES
        }

        // factory methods
        /**
         * Convenience overload of {@link #getInstance(ULocale, DialectHandling)} that specifies
         * STANDARD dialect handling.
         * @param locale the display locale
         * @return a LocaleDisplayNames instance
         * @stable ICU 4.4
         */
        public static LocaleDisplayNames GetInstance(ULocale locale)
        {
            return GetInstance(locale, DialectHandling.STANDARD_NAMES);
        }

        /**
         * Convenience overload of {@link #getInstance(Locale, DisplayContext...)} that specifies
         * {@link DisplayContext#STANDARD_NAMES}.
         * @param locale the display {@link java.util.Locale}
         * @return a LocaleDisplayNames instance
         * @stable ICU 54
         */
        public static LocaleDisplayNames GetInstance(CultureInfo locale)
        {
            return GetInstance(ULocale.ForLocale(locale));
        }

        /**
         * Returns an instance of LocaleDisplayNames that returns names formatted for the provided locale,
         * using the provided dialectHandling.
         * @param locale the display locale
         * @param dialectHandling how to select names for locales
         * @return a LocaleDisplayNames instance
         * @stable ICU 4.4
         */
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
                catch (TargetInvocationException e)
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
                result = new LastResortLocaleDisplayNames(locale, dialectHandling);
            }
            return result;
        }

        /**
         * Returns an instance of LocaleDisplayNames that returns names formatted for the provided locale,
         * using the provided DisplayContext settings
         * @param locale the display locale
         * @param contexts one or more context settings (e.g. for dialect
         *              handling, capitalization, etc.
         * @return a LocaleDisplayNames instance
         * @stable ICU 51
         */
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

        /**
         * Returns an instance of LocaleDisplayNames that returns names formatted for the provided
         * {@link java.util.Locale}, using the provided DisplayContext settings
         * @param locale the display {@link java.util.Locale}
         * @param contexts one or more context settings (e.g. for dialect
         *              handling, capitalization, etc.
         * @return a LocaleDisplayNames instance
         * @stable ICU 54
         */
        public static LocaleDisplayNames GetInstance(CultureInfo locale, params DisplayContext[] contexts)
        {
            return GetInstance(ULocale.ForLocale(locale), contexts);
        }

        // getters for state
        /**
         * Returns the locale used to determine the display names. This is not necessarily the same
         * locale passed to {@link #getInstance}.
         * @return the display locale
         * @stable ICU 4.4
         */
        public abstract ULocale GetLocale(); // ICU4N TODO: API - make property, rename Culture ?

        /**
         * Returns the dialect handling used in the display names.
         * @return the dialect handling enum
         * @stable ICU 4.4
         */
        public abstract DialectHandling GetDialectHandling();

        /**
         * Returns the current value for a specified DisplayContext.Type.
         * @param type the DisplayContext.Type whose value to return
         * @return the current DisplayContext setting for the specified type
         * @stable ICU 51
         */
        public abstract DisplayContext GetContext(DisplayContextType type);

        // names for entire locales
        /**
         * Returns the display name of the provided ulocale.
         * When no display names are available for all or portions
         * of the original locale ID, those portions may be
         * used directly (possibly in a more canonical form) as
         * part of the  returned display name.
         * @param locale the locale whose display name to return
         * @return the display name of the provided locale
         * @stable ICU 4.4
         */
        public abstract string LocaleDisplayName(ULocale locale);

        /**
         * Returns the display name of the provided locale.
         * When no display names are available for all or portions
         * of the original locale ID, those portions may be
         * used directly (possibly in a more canonical form) as
         * part of the  returned display name.
         * @param locale the locale whose display name to return
         * @return the display name of the provided locale
         * @stable ICU 4.4
         */
        public abstract string LocaleDisplayName(CultureInfo locale);

        /**
         * Returns the display name of the provided locale id.
         * When no display names are available for all or portions
         * of the original locale ID, those portions may be
         * used directly (possibly in a more canonical form) as
         * part of the  returned display name.
         * @param localeId the id of the locale whose display name to return
         * @return the display name of the provided locale
         * @stable ICU 4.4
         */
        public abstract string LocaleDisplayName(string localeId);

        // names for components of a locale id
        /**
         * Returns the display name of the provided language code.
         * @param lang the language code
         * @return the display name of the provided language code
         * @stable ICU 4.4
         */
        public abstract string LanguageDisplayName(string lang);

        /**
         * Returns the display name of the provided script code.
         * @param script the script code
         * @return the display name of the provided script code
         * @stable ICU 4.4
         */
        public abstract string ScriptDisplayName(string script);

        /**
         * Returns the display name of the provided script code
         * when used in the context of a full locale name.
         * @param script the script code
         * @return the display name of the provided script code
         * @internal ICU 49
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual string ScriptDisplayNameInContext(string script)
        {
            return ScriptDisplayName(script);
        }

        /**
         * Returns the display name of the provided script code.  See
         * {@link com.ibm.icu.lang.UScript} for recognized script codes.
         * @param scriptCode the script code number
         * @return the display name of the provided script code
         * @stable ICU 4.4
         */
        public abstract string ScriptDisplayName(int scriptCode);

        /**
         * Returns the display name of the provided region code.
         * @param region the region code
         * @return the display name of the provided region code
         * @stable ICU 4.4
         */
        public abstract string RegionDisplayName(string region);

        /**
         * Returns the display name of the provided variant.
         * @param variant the variant string
         * @return the display name of the provided variant
         * @stable ICU 4.4
         */
        public abstract string VariantDisplayName(string variant);

        /**
         * Returns the display name of the provided locale key.
         * @param key the locale key name
         * @return the display name of the provided locale key
         * @stable ICU 4.4
         */
        public abstract string KeyDisplayName(string key);

        /**
         * Returns the display name of the provided value (used with the provided key).
         * @param key the locale key name
         * @param value the locale key's value
         * @return the display name of the provided value
         * @stable ICU 4.4
         */
        public abstract string KeyValueDisplayName(string key, string value);


        /**
         * Return a list of information used to construct a UI list of locale names.
         * @param collator how to collate—should normally be Collator.getInstance(getDisplayLocale())
         * @param inSelf if true, compares the nameInSelf, otherwise the nameInDisplayLocale.
         * Set depending on which field (displayLocale vs self) is to show up in the UI.
         * If both are to show up in the UI, then it should be the one used for the primary sort order.
         * @param localeSet a list of locales to present in a UI list. The casing uses the settings in the LocaleDisplayNames instance.
         * @return an ordered list of UiListItems.
         * @throws IllformedLocaleException if any of the locales in localeSet are malformed.
         * @stable ICU 55
         */
        public virtual IList<UiListItem> GetUiList(ISet<ULocale> localeSet, bool inSelf, IComparer<object> collator)
        {
            return GetUiListCompareWholeItems(localeSet, UiListItem.GetComparer(collator, inSelf));
        }

        /**
         * Return a list of information used to construct a UI list of locale names, providing more access to control the sorting.
         * Normally use getUiList instead.
         * @param comparator how to sort the UiListItems in the result.
         * @param localeSet a list of locales to present in a UI list. The casing uses the settings in the LocaleDisplayNames instance.
         * @return an ordered list of UiListItems.
         * @throws IllformedLocaleException if any of the locales in localeSet are malformed.
         * @stable ICU 55
         */
        public abstract IList<UiListItem> GetUiListCompareWholeItems(ISet<ULocale> localeSet, IComparer<UiListItem> comparator);

        /**
         * Struct-like class used to return information for constructing a UI list, each corresponding to a locale.
         * @stable ICU 55
         */
        public class UiListItem
        {
            // ICU4N TODO: API - make into properties

            /**
             * Returns the minimized locale for an input locale, such as sr-Cyrl → sr
             * @stable ICU 55
             */
            public readonly ULocale minimized;
            /**
             * Returns the modified locale for an input locale, such as sr → sr-Cyrl, where there is also an sr-Latn in the list
             * @stable ICU 55
             */
            public readonly ULocale modified;
            /**
             * Returns the name of the modified locale in the display locale, such as "Englisch (VS)" (for 'en-US', where the display locale is 'de').
             * @stable ICU 55
             */
            public readonly string nameInDisplayLocale;
            /**
             * Returns the name of the modified locale in itself, such as "English (US)" (for 'en-US').
             * @stable ICU 55
             */
            public readonly string nameInSelf;

            /**
             * Constructor, normally only called internally.
             * @param minimized locale for an input locale
             * @param modified modified for an input locale
             * @param nameInDisplayLocale name of the modified locale in the display locale
             * @param nameInSelf name of the modified locale in itself
             * @stable ICU 55
             */
            public UiListItem(ULocale minimized, ULocale modified, string nameInDisplayLocale, string nameInSelf)
            {
                this.minimized = minimized;
                this.modified = modified;
                this.nameInDisplayLocale = nameInDisplayLocale;
                this.nameInSelf = nameInSelf;
            }

            /**
             * {@inheritDoc}
             *
             * @stable ICU 55
             */
            public override bool Equals(Object obj)
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
                return nameInDisplayLocale.Equals(other.nameInDisplayLocale)
                        && nameInSelf.Equals(other.nameInSelf)
                        && minimized.Equals(other.minimized)
                        && modified.Equals(other.modified);
            }

            /**
             * {@inheritDoc}
             *
             * @stable ICU 55
             */
            public override int GetHashCode()
            {
                return modified.GetHashCode() ^ nameInDisplayLocale.GetHashCode();
            }

            /**
             * {@inheritDoc}
             *
             * @stable ICU 55
             */
            public override string ToString()
            {
                return "{" + minimized + ", " + modified + ", " + nameInDisplayLocale + ", " + nameInSelf + "}";
            }

            /**
             * Return a comparator that compares the locale names for the display locale or the in-self names,
             * depending on an input parameter.
             * @param inSelf if true, compares the nameInSelf, otherwise the nameInDisplayLocale
             * @param comparator (meant for strings, but because Java Collator doesn't have &lt;string&gt;...)
             * @return UiListItem comparator
             * @stable ICU 55
             */
            public static IComparer<UiListItem> GetComparer(IComparer<object> comparer, bool inSelf)
            {
                return new UiListItemComparator(comparer, inSelf);
            }

            private class UiListItemComparator : IComparer<UiListItem>
            {
                private readonly IComparer<object> collator;
                private readonly bool useSelf;
                internal UiListItemComparator(IComparer<Object> collator, bool useSelf)
                {
                    this.collator = collator;
                    this.useSelf = useSelf;
                }

                public int Compare(UiListItem o1, UiListItem o2)
                {
                    int result = useSelf ? collator.Compare(o1.nameInSelf, o2.nameInSelf)
                            : collator.Compare(o1.nameInDisplayLocale, o2.nameInDisplayLocale);
                    return result != 0 ? result : o1.modified.CompareTo(o2.modified); // just in case
                }
            }
        }
        /**
         * Sole constructor.  (For invocation by subclass constructors,
         * typically implicit.)
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected LocaleDisplayNames()
        {
        }

        private static readonly MethodInfo FACTORY_DIALECTHANDLING;
        private static readonly MethodInfo FACTORY_DISPLAYCONTEXT;

        static LocaleDisplayNames()
        {
            string implClassName = Impl.ICUConfig.Get("ICU4N.LocaleDisplayNames.impl", "ICU4N.Impl.LocaleDisplayNamesImpl, ICU4N");

            MethodInfo factoryDialectHandling = null;
            MethodInfo factoryDisplayContext = null;

            try
            {
                Type implClass = Type.GetType(implClassName);

                if (implClass == null)
                    return;

                // ICU4N NOTE: GetMethod() doesn't throw an exception if the method
                // is not found in .NET.
                factoryDialectHandling = implClass.GetMethod("GetInstance", 
                    new Type[] { typeof(ULocale), typeof(DialectHandling) });

                factoryDisplayContext = implClass.GetMethod("GetInstance",
                    new Type[] { typeof(ULocale), typeof(DialectHandling[]) });
            }
            catch (TypeLoadException)
            {
                // fallback to last resort impl
            }

            FACTORY_DIALECTHANDLING = factoryDialectHandling;
            FACTORY_DISPLAYCONTEXT = factoryDisplayContext;
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
                DisplayContext context = (dialectHandling == DialectHandling.DIALECT_NAMES) ?
                        DisplayContext.DIALECT_NAMES : DisplayContext.STANDARD_NAMES;
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

            public override ULocale GetLocale()
            {
                return locale;
            }

            public override DialectHandling GetDialectHandling()
            {
                DialectHandling result = DialectHandling.STANDARD_NAMES;
                foreach (DisplayContext context in contexts)
                {
                    if (context.Type() == DisplayContextType.DIALECT_HANDLING)
                    {
                        if (context.Value() == (int)DisplayContext.DIALECT_NAMES)
                        {
                            result = DialectHandling.DIALECT_NAMES;
                            break;
                        }
                    }
                }
                return result;
            }

            public override DisplayContext GetContext(DisplayContextType type)
            {
                DisplayContext result = DisplayContext.STANDARD_NAMES;  // final fallback
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

            public override string LocaleDisplayName(ULocale locale)
            {
                return locale.GetName();
            }

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
