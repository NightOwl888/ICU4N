using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ICU4N.Impl
{
    public class ICULocaleService : ICUService
    {
        private ULocale fallbackLocale;
        private string fallbackLocaleName;

        /**
         * Construct an ICULocaleService.
         */
        public ICULocaleService()
        {
        }

        /**
         * Construct an ICULocaleService with a name (useful for debugging).
         */
        public ICULocaleService(string name)
            : base(name)
        {
        }

        /**
         * Convenience override for callers using locales.  This calls
         * get(ULocale, int, ULocale[]) with KIND_ANY for kind and null for
         * actualReturn.
         */
        public virtual object Get(ULocale locale) // ICU4N TODO: API - Use indexer?
        {
            return Get(locale, LocaleKey.KIND_ANY, null);
        }

        /**
         * Convenience override for callers using locales.  This calls
         * get(ULocale, int, ULocale[]) with a null actualReturn.
         */
        public virtual object Get(ULocale locale, int kind)
        {
            return Get(locale, kind, null);
        }

        /**
         * Convenience override for callers using locales.  This calls
         * get(ULocale, int, ULocale[]) with KIND_ANY for kind.
         */
        public virtual object Get(ULocale locale, ULocale[] actualReturn)
        {
            return Get(locale, LocaleKey.KIND_ANY, actualReturn);
        }

        /**
         * Convenience override for callers using locales.  This uses
         * createKey(ULocale.toString(), kind) to create a key, calls getKey, and then
         * if actualReturn is not null, returns the actualResult from
         * getKey (stripping any prefix) into a ULocale.
         */
        public virtual object Get(ULocale locale, int kind, ULocale[] actualReturn)
        {
            Key key = CreateKey(locale, kind);
            if (actualReturn == null)
            {
                return GetKey(key);
            }

            string[] temp = new string[1];
            object result = GetKey(key, temp);
            if (result != null)
            {
                int n = temp[0].IndexOf("/");
                if (n >= 0)
                {
                    temp[0] = temp[0].Substring(n + 1);
                }
                actualReturn[0] = new ULocale(temp[0]);
            }
            return result;
        }

        /**
         * Convenience override for callers using locales.  This calls
         * registerObject(Object, ULocale, int kind, boolean visible)
         * passing KIND_ANY for the kind, and true for the visibility.
         */
        public virtual IFactory RegisterObject(Object obj, ULocale locale)
        {
            return RegisterObject(obj, locale, LocaleKey.KIND_ANY, true);
        }

        /**
         * Convenience override for callers using locales.  This calls
         * registerObject(Object, ULocale, int kind, boolean visible)
         * passing KIND_ANY for the kind.
         */
        public virtual IFactory RegisterObject(Object obj, ULocale locale, bool visible)
        {
            return RegisterObject(obj, locale, LocaleKey.KIND_ANY, visible);
        }

        /**
         * Convenience function for callers using locales.  This calls
         * registerObject(Object, ULocale, int kind, boolean visible)
         * passing true for the visibility.
         */
        public virtual IFactory RegisterObject(Object obj, ULocale locale, int kind)
        {
            return RegisterObject(obj, locale, kind, true);
        }

        /**
         * Convenience function for callers using locales.  This  instantiates
         * a SimpleLocaleKeyFactory, and registers the factory.
         */
        public virtual IFactory RegisterObject(Object obj, ULocale locale, int kind, bool visible)
        {
            IFactory factory = new SimpleLocaleKeyFactory(obj, locale, kind, visible);
            return RegisterFactory(factory);
        }

        /**
         * Convenience method for callers using locales.  This returns the standard
         * Locale list, built from the Set of visible ids.
         */
        public virtual CultureInfo[] GetAvailableLocales()
        {
            // TODO make this wrap getAvailableULocales later
            ICollection<string> visIDs = GetVisibleIDs();
            CultureInfo[] locales = new CultureInfo[visIDs.Count];
            int n = 0;
            foreach (string id in visIDs)
            {
                CultureInfo loc = LocaleUtility.GetLocaleFromName(id);
                locales[n++] = loc;
            }
            return locales;
        }

        /**
         * Convenience method for callers using locales.  This returns the standard
         * ULocale list, built from the Set of visible ids.
         */
        public ULocale[] GetAvailableULocales()
        {
            ICollection<string> visIDs = GetVisibleIDs();
            ULocale[] locales = new ULocale[visIDs.Count];
            int n = 0;
            foreach (string id in visIDs)
            {
                locales[n++] = new ULocale(id);
            }
            return locales;
        }

        /**
         * A subclass of Key that implements a locale fallback mechanism.
         * The first locale to search for is the locale provided by the
         * client, and the fallback locale to search for is the current
         * default locale.  If a prefix is present, the currentDescriptor
         * includes it before the locale proper, separated by "/".  This
         * is the default key instantiated by ICULocaleService.</p>
         *
         * <p>Canonicalization adjusts the locale string so that the
         * section before the first understore is in lower case, and the rest
         * is in upper case, with no trailing underscores.</p>
         */
        public class LocaleKey : ICUService.Key
        {
            private int kind;
            private int varstart;
            private string primaryID;
            private string fallbackID;
            private string currentID;

            public static readonly int KIND_ANY = -1;

            /**
             * Create a LocaleKey with canonical primary and fallback IDs.
             */
            public static LocaleKey CreateWithCanonicalFallback(string primaryID, string canonicalFallbackID)
            {
                return CreateWithCanonicalFallback(primaryID, canonicalFallbackID, KIND_ANY);
            }

            /**
             * Create a LocaleKey with canonical primary and fallback IDs.
             */
            public static LocaleKey CreateWithCanonicalFallback(string primaryID, string canonicalFallbackID, int kind)
            {
                if (primaryID == null)
                {
                    return null;
                }
                string canonicalPrimaryID = ULocale.GetName(primaryID);
                return new LocaleKey(primaryID, canonicalPrimaryID, canonicalFallbackID, kind);
            }

            /**
             * Create a LocaleKey with canonical primary and fallback IDs.
             */
            public static LocaleKey createWithCanonical(ULocale locale, string canonicalFallbackID, int kind)
            {
                if (locale == null)
                {
                    return null;
                }
                string canonicalPrimaryID = locale.GetName();
                return new LocaleKey(canonicalPrimaryID, canonicalPrimaryID, canonicalFallbackID, kind);
            }

            /**
             * PrimaryID is the user's requested locale string,
             * canonicalPrimaryID is this string in canonical form,
             * fallbackID is the current default locale's string in
             * canonical form.
             */
            protected LocaleKey(string primaryID, string canonicalPrimaryID, string canonicalFallbackID, int kind)
                    : base(primaryID)
            {
                this.kind = kind;

                if (canonicalPrimaryID == null || canonicalPrimaryID.Equals("root", StringComparison.OrdinalIgnoreCase))
                {
                    this.primaryID = "";
                    this.fallbackID = null;
                }
                else
                {
                    int idx = canonicalPrimaryID.IndexOf('@');
                    if (idx == 4 && canonicalPrimaryID.RegionMatches(true, 0, "root", 0, 4))
                    {
                        this.primaryID = canonicalPrimaryID.Substring(4);
                        this.varstart = 0;
                        this.fallbackID = null;
                    }
                    else
                    {
                        this.primaryID = canonicalPrimaryID;
                        this.varstart = idx;

                        if (canonicalFallbackID == null || this.primaryID.Equals(canonicalFallbackID))
                        {
                            this.fallbackID = "";
                        }
                        else
                        {
                            this.fallbackID = canonicalFallbackID;
                        }
                    }
                }

                this.currentID = varstart == -1 ? this.primaryID : this.primaryID.Substring(0, varstart - 0); // ICU4N: Checked 2nd parameter
            }

            /**
             * Return the prefix associated with the kind, or null if the kind is KIND_ANY.
             */
            public virtual string Prefix
            {
                get { return kind == KIND_ANY ? null : Kind.ToString(CultureInfo.InvariantCulture); }
            }

            /**
             * Return the kind code associated with this key.
             */
            public virtual int Kind
            {
                get { return kind; }
            }

            /**
             * Return the (canonical) original ID.
             */
            public override string CanonicalID
            {
                get { return primaryID; }
            }

            /**
             * Return the (canonical) current ID, or null if no current id.
             */
            public override string CurrentID
            {
                get { return currentID; }
            }

            /**
             * Return the (canonical) current descriptor, or null if no current id.
             * Includes the keywords, whereas the ID does not include keywords.
             */
            public override string CurrentDescriptor()
            {
                string result = CurrentID;
                if (result != null)
                {
                    StringBuilder buf = new StringBuilder(); // default capacity 16 is usually good enough
                    if (kind != KIND_ANY)
                    {
                        buf.Append(Prefix);
                    }
                    buf.Append('/');
                    buf.Append(result);
                    if (varstart != -1)
                    {
                        buf.Append(primaryID.Substring(varstart, primaryID.Length - varstart)); // ICU4N: Corrected 2nd Substring parameter
                    }
                    result = buf.ToString();
                }
                return result;
            }

            /**
             * Convenience method to return the locale corresponding to the (canonical) original ID.
             */
            public ULocale CanonicalLocale() // ICU4N TODO: API - begin with Get ?
            {
                return new ULocale(primaryID);
            }

            /**
             * Convenience method to return the ulocale corresponding to the (canonical) currentID.
             */
            public ULocale CurrentLocale() // ICU4N TODO: API - begin with Get ?
            {
                if (varstart == -1)
                {
                    return new ULocale(currentID);
                }
                else
                {
                    return new ULocale(currentID + primaryID.Substring(varstart));
                }
            }

            /**
             * If the key has a fallback, modify the key and return true,
             * otherwise return false.</p>
             *
             * <p>First falls back through the primary ID, then through
             * the fallbackID.  The final fallback is "" (root)
             * unless the primary id was "" (root), in which case
             * there is no fallback.
             */
            public override bool Fallback()
            {
                int x = currentID.LastIndexOf('_');
                if (x != -1)
                {
                    while (--x >= 0 && currentID[x] == '_')
                    { // handle zh__PINYIN
                    }
                    currentID = currentID.Substring(0, (x + 1) - 0); // ICU4N: Checked 2nd parameter
                    return true;
                }
                if (fallbackID != null)
                {
                    currentID = fallbackID;
                    if (fallbackID.Length == 0)
                    {
                        fallbackID = null;
                    }
                    else
                    {
                        fallbackID = "";
                    }
                    return true;
                }
                currentID = null;
                return false;
            }

            /**
             * If a key created from id would eventually fallback to match the
             * canonical ID of this key, return true.
             */
            public override bool IsFallbackOf(string id)
            {
                return LocaleUtility.IsFallbackOf(CanonicalID, id);
            }
        }

        /**
         * A subclass of Factory that uses LocaleKeys.  If 'visible' the
         * factory reports its IDs.
         */
        public abstract class LocaleKeyFactory : IFactory
        {
            protected readonly string name;
            protected readonly bool visible;

            public static readonly bool VISIBLE = true;
            public static readonly bool INVISIBLE = false;

            /**
             * Constructor used by subclasses.
             */
            protected LocaleKeyFactory(bool visible)
            {
                this.visible = visible;
                this.name = null;
            }

            /**
             * Constructor used by subclasses.
             */
            protected LocaleKeyFactory(bool visible, string name)
            {
                this.visible = visible;
                this.name = name;
            }

            /**
             * Implement superclass abstract method.  This checks the currentID of
             * the key against the supported IDs, and passes the canonicalLocale and
             * kind off to handleCreate (which subclasses must implement).
             */
            public virtual object Create(Key key, ICUService service)
            {
                if (HandlesKey(key))
                {
                    LocaleKey lkey = (LocaleKey)key;
                    int kind = lkey.Kind;

                    ULocale uloc = lkey.CurrentLocale();
                    return HandleCreate(uloc, kind, service);
                }
                else
                {
                    // System.out.println("factory: " + this + " did not support id: " + key.currentID());
                    // System.out.println("supported ids: " + getSupportedIDs());
                }
                return null;
            }

            protected virtual bool HandlesKey(Key key)
            {
                if (key != null)
                {
                    string id = key.CurrentID;
                    ICollection<string> supported = GetSupportedIDs();
                    return supported.Contains(id);
                }
                return false;
            }

            /**
             * Override of superclass method.
             */
            public virtual void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                ICollection<string> cache = GetSupportedIDs();
                foreach (string id in cache)
                {
                    if (visible)
                    {
                        result[id] = this;
                    }
                    else
                    {
                        result.Remove(id);
                    }
                }
            }

            /**
             * Return a localized name for the locale represented by id.
             */
            public virtual string GetDisplayName(string id, ULocale locale)
            {
                // assume if the user called this on us, we must have handled some fallback of this id
                //          if (isSupportedID(id)) {
                if (locale == null)
                {
                    return id;
                }
                ULocale loc = new ULocale(id);
                return loc.GetDisplayName(locale);
                //              }
                //          return null;
            }

            ///CLOVER:OFF
            /**
             * Utility method used by create(Key, ICUService).  Subclasses can
             * implement this instead of create.
             */
            protected virtual object HandleCreate(ULocale loc, int kind, ICUService service)
            {
                return null;
            }
            ///CLOVER:ON

            /**
             * Return true if this id is one the factory supports (visible or
             * otherwise).
             */
            protected virtual bool IsSupportedID(string id)
            {
                return GetSupportedIDs().Contains(id);
            }

            /**
             * Return the set of ids that this factory supports (visible or
             * otherwise).  This can be called often and might need to be
             * cached if it is expensive to create.
             */
            protected virtual ICollection<string> GetSupportedIDs()
            {
                return new List<string>();
            }

            /**
             * For debugging.
             */
            public override string ToString()
            {
                StringBuilder buf = new StringBuilder(base.ToString());
                if (name != null)
                {
                    buf.Append(", name: ");
                    buf.Append(name);
                }
                buf.Append(", visible: ");
                buf.Append(visible);
                return buf.ToString();
            }
        }

        /**
         * A LocaleKeyFactory that just returns a single object for a kind/locale.
         */
        public class SimpleLocaleKeyFactory : LocaleKeyFactory
        {
            private readonly object obj;
            private readonly string id;
            private readonly int kind;

            // TODO: remove when we no longer need this
            public SimpleLocaleKeyFactory(object obj, ULocale locale, int kind, bool visible)
                            : this(obj, locale, kind, visible, null)
            {
            }

            public SimpleLocaleKeyFactory(object obj, ULocale locale, int kind, bool visible, string name)
                            : base(visible, name)
            {
                this.obj = obj;
                this.id = locale.GetBaseName();
                this.kind = kind;
            }

            /**
             * Returns the service object if kind/locale match.  Service is not used.
             */
            public override object Create(Key key, ICUService service)
            {
                if (!(key is LocaleKey))
                {
                    return null;
                }

                LocaleKey lkey = (LocaleKey)key;
                if (kind != LocaleKey.KIND_ANY && kind != lkey.Kind)
                {
                    return null;
                }
                if (!id.Equals(lkey.CurrentID))
                {
                    return null;
                }

                return obj;
            }

            protected override bool IsSupportedID(string idToCheck)
            {
                return this.id.Equals(idToCheck);
            }

            public override void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                if (visible)
                {
                    result[id] = this;
                }
                else
                {
                    result.Remove(id);
                }
            }

            public override string ToString()
            {
                StringBuilder buf = new StringBuilder(base.ToString());
                buf.Append(", id: ");
                buf.Append(id);
                buf.Append(", kind: ");
                buf.Append(kind);
                return buf.ToString();
            }
        }

        /**
         * A LocaleKeyFactory that creates a service based on the ICU locale data.
         * This is a base class for most ICU factories.  Subclasses instantiate it
         * with a constructor that takes a bundle name, which determines the supported
         * IDs.  Subclasses then override handleCreate to create the actual service
         * object.  The default implementation returns a resource bundle.
         */
        public class ICUResourceBundleFactory : LocaleKeyFactory
        {
            protected readonly string bundleName;

            /**
             * Convenience constructor that uses the main ICU bundle name.
             */
            public ICUResourceBundleFactory()
                    : this(ICUData.ICU_BASE_NAME)
            {
            }

            /**
             * A service factory based on ICU resource data in resources
             * with the given name.
             */
            public ICUResourceBundleFactory(string bundleName)
                            : base(true)
            {
                this.bundleName = bundleName;
            }

            /**
             * Return the supported IDs.  This is the set of all locale names for the bundleName.
             */
            protected override ICollection<string> GetSupportedIDs()
            {
                return ICUResourceBundle.GetFullLocaleNameSet(bundleName, Assembly);
            }

            /**
             * Override of superclass method.
             */
            public override void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                ISet<string> visibleIDs = ICUResourceBundle.GetAvailableLocaleNameSet(bundleName, Assembly); // only visible ids
                foreach (string id in visibleIDs)
                {
                    result[id] = this;
                }
            }

            /**
             * Create the service.  The default implementation returns the resource bundle
             * for the locale, ignoring kind, and service.
             */
            protected override object HandleCreate(ULocale loc, int kind, ICUService service)
            {
                return ICUResourceBundle.GetBundleInstance(bundleName, loc, Assembly);
            }

            protected virtual Assembly Assembly
            {
                get { return GetType().GetTypeInfo().Assembly; }
            }

            public override string ToString()
            {
                return base.ToString() + ", bundle: " + bundleName;
            }
        }

        /**
         * Return the name of the current fallback locale.  If it has changed since this was
         * last accessed, the service cache is cleared.
         */
        public virtual string ValidateFallbackLocale()
        {
            ULocale loc = ULocale.GetDefault();
            if (loc != fallbackLocale)
            {
                lock (this)
                {
                    if (loc != fallbackLocale)
                    {
                        fallbackLocale = loc;
                        fallbackLocaleName = loc.GetBaseName();
                        ClearServiceCache();
                    }
                }
            }
            return fallbackLocaleName;
        }

        public override Key CreateKey(string id)
        {
            return LocaleKey.CreateWithCanonicalFallback(id, ValidateFallbackLocale());
        }

        public virtual Key CreateKey(string id, int kind)
        {
            return LocaleKey.CreateWithCanonicalFallback(id, ValidateFallbackLocale(), kind);
        }

        public virtual Key CreateKey(ULocale l, int kind)
        {
            return LocaleKey.createWithCanonical(l, ValidateFallbackLocale(), kind);
        }
    }
}
