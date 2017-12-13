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

        /// <summary>
        /// Construct an <see cref="ICULocaleService"/>.
        /// </summary>
        public ICULocaleService()
        {
        }

        /// <summary>
        /// Construct an <see cref="ICULocaleService"/> with a <paramref name="name"/> (useful for debugging).
        /// </summary>
        public ICULocaleService(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="Get(ULocale, int, ULocale[])"/> with <see cref="LocaleKey.KIND_ANY"/>
        /// for kind and null for actualReturn.
        /// </summary>
        public virtual object Get(ULocale locale) // ICU4N TODO: API - Use indexer?
        {
            return Get(locale, LocaleKey.KIND_ANY, null);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="Get(ULocale, int, ULocale[])"/> with a null actualReturn.
        /// </summary>
        public virtual object Get(ULocale locale, int kind)
        {
            return Get(locale, kind, null);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="Get(ULocale, int, ULocale[])"/> with <see cref="LocaleKey.KIND_ANY"/> for kind.
        /// </summary>
        public virtual object Get(ULocale locale, ULocale[] actualReturn)
        {
            return Get(locale, LocaleKey.KIND_ANY, actualReturn);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This uses
        /// <see cref="CreateKey(string, int)"/> to create a key, calls 
        /// <see cref="ICUService.GetKey(Key)"/>, and then
        /// if <paramref name="actualReturn"/> is not null, returns the actualResult from
        /// <see cref="ICUService.GetKey(Key)"/> (stripping any prefix) into a <see cref="ULocale"/>.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="kind"></param>
        /// <param name="actualReturn"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="RegisterObject(object, ULocale, int, bool)"/>
        /// passing <see cref="LocaleKey.KIND_ANY"/> for the kind, and true for the visibility.
        /// </summary>
        public virtual IFactory RegisterObject(object obj, ULocale locale)
        {
            return RegisterObject(obj, locale, LocaleKey.KIND_ANY, true);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="RegisterObject(object, ULocale, int, bool)"/>
        /// passing <see cref="LocaleKey.KIND_ANY"/> for the kind.
        /// </summary>
        public virtual IFactory RegisterObject(Object obj, ULocale locale, bool visible)
        {
            return RegisterObject(obj, locale, LocaleKey.KIND_ANY, visible);
        }

        /// <summary>
        /// Convenience function for callers using locales.  This calls
        /// <see cref="RegisterObject(object, ULocale, int, bool)"/>
        /// passing true for the visibility.
        /// </summary>
        public virtual IFactory RegisterObject(Object obj, ULocale locale, int kind)
        {
            return RegisterObject(obj, locale, kind, true);
        }

        /// <summary>
        /// Convenience function for callers using locales.  This instantiates
        /// a <see cref="SimpleLocaleKeyFactory"/>, and registers the factory.
        /// </summary>
        public virtual IFactory RegisterObject(Object obj, ULocale locale, int kind, bool visible)
        {
            IFactory factory = new SimpleLocaleKeyFactory(obj, locale, kind, visible);
            return RegisterFactory(factory);
        }

        /// <summary>
        /// Convenience method for callers using locales.  This returns the standard
        /// <see cref="CultureInfo"/> list, built from the <see cref="ICollection{T}"/> of visible ids.
        /// </summary>
        public virtual CultureInfo[] GetAvailableLocales() // ICU4N TODO: API - rename GetCultures (just like CultureInfo) - consider adding a CultureTypes filter
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

        /// <summary>
        /// Convenience method for callers using locales.  This returns the standard
        /// <see cref="ULocale"/> list, built from the <see cref="ICollection{T}"/> of visible ids.
        /// </summary>
        public virtual ULocale[] GetAvailableULocales() // ICU4N TODO: API - rename GetUCultures (just like CultureInfo) - consider adding a CultureTypes filter
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

        /// <summary>
        /// A subclass of <see cref="ICUService.Key"/> that implements a locale fallback mechanism.
        /// </summary>
        /// <remarks>
        /// The first locale to search for is the locale provided by the
        /// client, and the fallback locale to search for is the current
        /// default locale.  If a prefix is present, the currentDescriptor
        /// includes it before the locale proper, separated by "/".  This
        /// is the default key instantiated by ICULocaleService.
        /// <para/>
        /// Canonicalization adjusts the locale string so that the
        /// section before the first understore is in lower case, and the rest
        /// is in upper case, with no trailing underscores.
        /// </remarks>
        public class LocaleKey : ICUService.Key // ICU4N TODO: API de-nest ?
        {
            private int kind;
            private int varstart;
            private string primaryID;
            private string fallbackID;
            private string currentID;

            public static readonly int KIND_ANY = -1; // ICU4N TODO: API rename to follow .NET Conventions

            /// <summary>
            /// Create a <see cref="LocaleKey"/> with canonical primary and fallback IDs.
            /// </summary>
            public static LocaleKey CreateWithCanonicalFallback(string primaryID, string canonicalFallbackID)
            {
                return CreateWithCanonicalFallback(primaryID, canonicalFallbackID, KIND_ANY);
            }

            /// <summary>
            /// Create a <see cref="LocaleKey"/> with canonical primary and fallback IDs.
            /// </summary>
            public static LocaleKey CreateWithCanonicalFallback(string primaryID, string canonicalFallbackID, int kind)
            {
                if (primaryID == null)
                {
                    return null;
                }
                string canonicalPrimaryID = ULocale.GetName(primaryID);
                return new LocaleKey(primaryID, canonicalPrimaryID, canonicalFallbackID, kind);
            }

            /// <summary>
            /// Create a <see cref="LocaleKey"/> with canonical primary and fallback IDs.
            /// </summary>
            public static LocaleKey CreateWithCanonical(ULocale locale, string canonicalFallbackID, int kind)
            {
                if (locale == null)
                {
                    return null;
                }
                string canonicalPrimaryID = locale.GetName();
                return new LocaleKey(canonicalPrimaryID, canonicalPrimaryID, canonicalFallbackID, kind);
            }

            /// <param name="primaryID">The user's requested locale string.</param>
            /// <param name="canonicalPrimaryID"><paramref name="primaryID"/> string in canonical form.</param>
            /// <param name="canonicalFallbackID">The current default locale's string in canonical form.</param>
            /// <param name="kind"></param>
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

            /// <summary>
            /// Return the prefix associated with the kind, or null if the kind is <see cref="KIND_ANY"/>.
            /// </summary>
            public virtual string Prefix
            {
                get { return kind == KIND_ANY ? null : Kind.ToString(CultureInfo.InvariantCulture); }
            }

            /// <summary>
            /// Return the kind code associated with this key.
            /// </summary>
            public virtual int Kind
            {
                get { return kind; }
            }

            /// <summary>
            /// Return the (canonical) original ID.
            /// </summary>
            public override string CanonicalID
            {
                get { return primaryID; }
            }

            /// <summary>
            /// Return the (canonical) current ID, or null if no current id.
            /// </summary>
            public override string CurrentID
            {
                get { return currentID; }
            }

            /// <summary>
            /// Return the (canonical) current descriptor, or null if no current id.
            /// Includes the keywords, whereas the ID does not include keywords.
            /// </summary>
            public override string CurrentDescriptor() // ICU4N TODO: API - begin with Get ?
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

            /// <summary>
            /// Convenience method to return the locale corresponding to the (canonical) original ID.
            /// </summary>
            public virtual ULocale CanonicalLocale() // ICU4N TODO: API - begin with Get ?
            {
                return new ULocale(primaryID);
            }

            /// <summary>
            /// Convenience method to return the ulocale corresponding to the (canonical) currentID.
            /// </summary>
            public virtual ULocale CurrentLocale() // ICU4N TODO: API - begin with Get ?
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

            /// <summary>
            /// If the key has a fallback, modify the key and return true,
            /// otherwise return false.
            /// <para/>
            /// First falls back through the primary ID, then through
            /// the fallbackID.  The final fallback is "" (root)
            /// unless the primary id was "" (root), in which case
            /// there is no fallback.
            /// </summary>
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

            /// <summary>
            /// If a key created from id would eventually fallback to match the
            /// canonical ID of this key, return true.
            /// </summary>
            public override bool IsFallbackOf(string id)
            {
                return LocaleUtility.IsFallbackOf(CanonicalID, id);
            }
        }

        /// <summary>
        /// A subclass of Factory that uses LocaleKeys.  If 'visible' the
        /// factory reports its IDs.
        /// </summary>
        public abstract class LocaleKeyFactory : IFactory // ICU4N TODO: API de-nest ?
        {
            protected readonly string name;
            protected readonly bool visible;

            public static readonly bool VISIBLE = true; // ICU4N TODO: API - rename to match .NET Conventions
            public static readonly bool INVISIBLE = false; // ICU4N TODO: API - rename to match .NET Conventions

            /// <summary>
            /// Constructor used by subclasses.
            /// </summary>
            protected LocaleKeyFactory(bool visible)
            {
                this.visible = visible;
                this.name = null;
            }

            /// <summary>
            /// Constructor used by subclasses.
            /// </summary>
            protected LocaleKeyFactory(bool visible, string name)
            {
                this.visible = visible;
                this.name = name;
            }

            /// <summary>
            /// Implement superclass abstract method.  This checks the <see cref="ICUService.Key.CurrentID"/>
            /// against the supported IDs, and passes the canonicalLocale and
            /// <see cref="LocaleKey.Kind"/> to <see cref="HandleCreate(ULocale, int, ICUService)"/> (which subclasses must implement).
            /// </summary>
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

            /// <summary>
            /// Implementation of <see cref="ICUService.IFactory"/> method.
            /// </summary>
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

            /// <summary>
            /// Return a localized name for the locale represented by id.
            /// </summary>
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

            //CLOVER:OFF
            /// <summary>
            /// Utility method used by <see cref="Create(Key, ICUService)"/>.  Subclasses can
            /// implement this instead of <see cref="Create(Key, ICUService)"/>.
            /// </summary>
            protected virtual object HandleCreate(ULocale loc, int kind, ICUService service)
            {
                return null;
            }
            //CLOVER:ON

            /// <summary>
            /// Return true if this id is one the factory supports (visible or
            /// otherwise).
            /// </summary>
            protected virtual bool IsSupportedID(string id)
            {
                return GetSupportedIDs().Contains(id);
            }

            /// <summary>
            /// Return the set of ids that this factory supports (visible or
            /// otherwise).  This can be called often and might need to be
            /// cached if it is expensive to create.
            /// </summary>
            protected virtual ICollection<string> GetSupportedIDs()
            {
                return new List<string>();
            }

            /// <summary>
            /// For debugging.
            /// </summary>
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

        /// <summary>
        /// A <see cref="LocaleKeyFactory"/> that just returns a single object for a kind/locale.
        /// </summary>
        public class SimpleLocaleKeyFactory : LocaleKeyFactory // ICU4N TODO: API de-nest ?
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

            /// <summary>
            /// Returns the service object if kind/locale match.  Service is not used.
            /// </summary>
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

        /// <summary>
        /// A <see cref="LocaleKeyFactory"/> that creates a service based on the ICU locale data.
        /// This is a base class for most ICU factories.  Subclasses instantiate it
        /// with a constructor that takes a bundle name, which determines the supported
        /// IDs.  Subclasses then override <see cref="HandleCreate(ULocale, int, ICUService)"/> to create the actual service
        /// object.  The default implementation returns a <see cref="System.Resources.ResourceManager"/>.
        /// </summary>
        public class ICUResourceBundleFactory : LocaleKeyFactory // ICU4N TODO: API de-nest ? Rename ICUResourceManagerFactory
        {
            protected readonly string bundleName; // ICU4N TODO: API - rename baseName ?? Need to work out how to map this to ResourceManager

            /// <summary>
            /// Convenience constructor that uses the main ICU bundle name.
            /// </summary>
            public ICUResourceBundleFactory()
                : this(ICUData.ICU_BASE_NAME)
            {
            }

            /// <summary>
            /// A service factory based on ICU resource data in resources
            /// with the given name.
            /// </summary>
            public ICUResourceBundleFactory(string bundleName)
                : base(true)
            {
                this.bundleName = bundleName;
            }

            /// <summary>
            /// Return the supported IDs.  This is the set of all locale names for the bundleName.
            /// </summary>
            protected override ICollection<string> GetSupportedIDs()
            {
                return ICUResourceBundle.GetFullLocaleNameSet(bundleName, Assembly);
            }

            /// <summary>
            /// Override of superclass method.
            /// </summary>
            public override void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                ISet<string> visibleIDs = ICUResourceBundle.GetAvailableLocaleNameSet(bundleName, Assembly); // only visible ids
                foreach (string id in visibleIDs)
                {
                    result[id] = this;
                }
            }

            /// <summary>
            /// Create the service.  The default implementation returns the resource bundle
            /// for the locale, ignoring kind, and service.
            /// </summary>
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

        /// <summary>
        /// Return the name of the current fallback locale.  If it has changed since this was
        /// last accessed, the service cache is cleared.
        /// </summary>
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
            return LocaleKey.CreateWithCanonical(l, ValidateFallbackLocale(), kind);
        }
    }
}
