using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ICU4N.Impl
{
    public class ICULocaleService : ICUService
    {
        private UCultureInfo fallbackLocale;
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
        /// <see cref="Get(UCultureInfo, int, UCultureInfo[])"/> with <see cref="LocaleKey.KindAny"/>
        /// for kind and null for actualReturn.
        /// </summary>
        public virtual object Get(UCultureInfo locale) // ICU4N TODO: API - Use indexer? // ICU4N TODO: API Remove
        {
            return Get(locale, LocaleKey.KindAny, null);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="Get(UCultureInfo, int, UCultureInfo[])"/> with a null actualReturn.
        /// </summary>
        public virtual object Get(UCultureInfo locale, int kind) // ICU4N TODO: API Remove
        {
            return Get(locale, kind, null);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="Get(UCultureInfo, int, UCultureInfo[])"/> with <see cref="LocaleKey.KindAny"/> for kind.
        /// </summary>
        public virtual object Get(UCultureInfo locale, UCultureInfo[] actualReturn) // ICU4N TODO: API Remove
        {
            return Get(locale, LocaleKey.KindAny, actualReturn);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This uses
        /// <see cref="CreateKey(string, int)"/> to create a key, calls 
        /// <see cref="ICUService.GetKey(ICUServiceKey)"/>, and then
        /// if <paramref name="actualReturn"/> is not null, returns the actualResult from
        /// <see cref="ICUService.GetKey(ICUServiceKey)"/> (stripping any prefix) into a <see cref="UCultureInfo"/>.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="kind"></param>
        /// <param name="actualReturn"></param>
        /// <returns></returns>
        public virtual object Get(UCultureInfo locale, int kind, UCultureInfo[] actualReturn) // ICU4N TODO: API Remove
        {
            ICUServiceKey key = CreateKey(locale, kind);
            if (actualReturn == null)
            {
                return GetKey(key);
            }

            string[] temp = new string[1];
            object result = GetKey(key, temp);
            if (result != null)
            {
                int n = temp[0].IndexOf('/');
                if (n >= 0)
                {
                    temp[0] = temp[0].Substring(n + 1);
                }
                actualReturn[0] = new UCultureInfo(temp[0]);
            }
            return result;
        }


        ///// <summary>
        ///// Convenience override for callers using locales.  This calls
        ///// <see cref="Get(UCultureInfo, int, out UCultureInfo)"/> with <see cref="LocaleKey.KindAny"/> for kind.
        ///// </summary>
        //public virtual object Get(UCultureInfo locale) // ICU4N TODO: API - Use indexer?
        //{
        //    return Get(locale, LocaleKey.KindAny);
        //}

        ///// <summary>
        ///// Convenience override for callers using locales.  This calls
        ///// <see cref="Get(UCultureInfo, int, out UCultureInfo)"/> with <see cref="LocaleKey.KindAny"/> for kind.
        ///// </summary>
        //public virtual object Get(UCultureInfo locale, out UCultureInfo actualResult)
        //{
        //    return Get(locale, LocaleKey.KindAny, out actualResult);
        //}

        ///// <summary>
        ///// Convenience override for callers using locales. This uses
        ///// <see cref="CreateKey(string, int)"/> to create a key, and returns the result of
        ///// <see cref="ICUService.GetKey(ICUServiceKey)"/>.
        ///// </summary>
        ///// <param name="locale"></param>
        ///// <param name="kind"></param>
        ///// <returns></returns>
        //public virtual object Get(UCultureInfo locale, int kind)
        //{
        //    ICUServiceKey key = CreateKey(locale, kind);
        //    return GetKey(key);
        //}

        ///// <summary>
        ///// Convenience override for callers using locales.  This uses
        ///// <see cref="CreateKey(string, int)"/> to create a key, calls 
        ///// <see cref="ICUService.GetKey(ICUServiceKey)"/>, and then
        ///// returns the <paramref name="actualResult"/> from
        ///// <see cref="ICUService.GetKey(ICUServiceKey)"/> (stripping any prefix)
        ///// into a <see cref="UCultureInfo"/>.
        ///// </summary>
        ///// <param name="locale"></param>
        ///// <param name="kind"></param>
        ///// <param name="actualResult"></param>
        ///// <returns></returns>
        //public virtual object Get(UCultureInfo locale, int kind, out UCultureInfo actualResult)
        //{
        //    actualResult = null;
        //    ICUServiceKey key = CreateKey(locale, kind);

        //    string[] temp = new string[1];
        //    object result = GetKey(key, temp);
        //    if (result != null)
        //    {
        //        int n = temp[0].IndexOf('/');
        //        if (n >= 0)
        //        {
        //            temp[0] = temp[0].Substring(n + 1);
        //        }
        //        actualResult = new UCultureInfo(temp[0]);
        //    }
        //    return result;
        //}

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="RegisterObject(object, UCultureInfo, int, bool)"/>
        /// passing <see cref="LocaleKey.KindAny"/> for the kind, and true for the visibility.
        /// </summary>
        public virtual IServiceFactory RegisterObject(object obj, UCultureInfo locale)
        {
            return RegisterObject(obj, locale, LocaleKey.KindAny, true);
        }

        /// <summary>
        /// Convenience override for callers using locales.  This calls
        /// <see cref="RegisterObject(object, UCultureInfo, int, bool)"/>
        /// passing <see cref="LocaleKey.KindAny"/> for the kind.
        /// </summary>
        public virtual IServiceFactory RegisterObject(object obj, UCultureInfo locale, bool visible)
        {
            return RegisterObject(obj, locale, LocaleKey.KindAny, visible);
        }

        /// <summary>
        /// Convenience function for callers using locales.  This calls
        /// <see cref="RegisterObject(object, UCultureInfo, int, bool)"/>
        /// passing true for the visibility.
        /// </summary>
        public virtual IServiceFactory RegisterObject(object obj, UCultureInfo locale, int kind)
        {
            return RegisterObject(obj, locale, kind, true);
        }

        /// <summary>
        /// Convenience function for callers using locales.  This instantiates
        /// a <see cref="SimpleLocaleKeyFactory"/>, and registers the factory.
        /// </summary>
        public virtual IServiceFactory RegisterObject(object obj, UCultureInfo locale, int kind, bool visible)
        {
            IServiceFactory factory = new SimpleLocaleKeyFactory(obj, locale, kind, visible);
            return RegisterFactory(factory);
        }

        /// <summary>
        /// Convenience method for callers using locales.  This returns the standard
        /// <see cref="CultureInfo"/> list, built from the <see cref="ICollection{T}"/> of visible ids.
        /// </summary>
        public virtual CultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            return GetCultures(types, (id) => LocaleUtility.GetLocaleFromName(id));
        }

        /// <summary>
        /// Convenience method for callers using locales.  This returns the standard
        /// <see cref="UCultureInfo"/> list, built from the <see cref="ICollection{T}"/> of visible ids.
        /// </summary>
        public virtual UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableULocales
        {
            return GetCultures(types, (id) => new UCultureInfo(id));
        }

        private T[] GetCultures<T>(UCultureTypes types, Func<string, T> factory)
        {
            ICollection<string> visIDs = GetVisibleIDs();
            
            if (types == UCultureTypes.AllCultures)
            {
                int n = 0;
                var locales = new T[visIDs.Count];
                foreach (string id in visIDs)
                    locales[n++] = factory(id);
                return locales;
            }
            else
            {
                List<T> locales = new List<T>(visIDs.Count);
                var parser = new LocaleIDParser(string.Empty);
                foreach (string id in visIDs)
                {
                    // Filter the culture type before allocating the object
                    parser.Reset(id);
                    bool isNeutralCulture = parser.GetLocaleID().IsNeutralCulture;
                    if (isNeutralCulture && types.HasFlag(UCultureTypes.NeutralCultures)
                        || (!isNeutralCulture && types.HasFlag(UCultureTypes.SpecificCultures)))
                    {
                        locales.Add(factory(id));
                    }
                }

                return locales.ToArray();
            }
        }

        // ICU4N specific - de-nested LocaleKey

        // ICU4N specific - de-nested LocaleKeyFactory

        // ICU4N specific - de-nested SimpleLocaleKeyFactory

        // ICU4N specific - de-nested ICUResourceBundleFactory

        /// <summary>
        /// Return the name of the current fallback locale.  If it has changed since this was
        /// last accessed, the service cache is cleared.
        /// </summary>
        public virtual string ValidateFallbackLocale()
        {
            UCultureInfo loc = UCultureInfo.CurrentCulture;
            if (loc != fallbackLocale)
            {
                lock (this)
                {
                    if (loc != fallbackLocale)
                    {
                        fallbackLocale = loc;
                        fallbackLocaleName = loc.Name;
                        ClearServiceCache();
                    }
                }
            }
            return fallbackLocaleName;
        }

        public override ICUServiceKey CreateKey(string id)
        {
            return LocaleKey.CreateWithCanonicalFallback(id, ValidateFallbackLocale());
        }

        public virtual ICUServiceKey CreateKey(string id, int kind)
        {
            return LocaleKey.CreateWithCanonicalFallback(id, ValidateFallbackLocale(), kind);
        }

        public virtual ICUServiceKey CreateKey(UCultureInfo l, int kind)
        {
            return LocaleKey.CreateWithCanonical(l, ValidateFallbackLocale(), kind);
        }
    }

    /// <summary>
    /// A subclass of <see cref="ICUServiceKey"/> that implements a locale fallback mechanism.
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
    public class LocaleKey : ICUServiceKey
    {
        private int kind;
        private int varstart;
        private string primaryID;
        private string fallbackID;
        private string currentID;

        public const int KindAny = -1;

        /// <summary>
        /// Create a <see cref="LocaleKey"/> with canonical primary and fallback IDs.
        /// </summary>
        public static LocaleKey CreateWithCanonicalFallback(string primaryID, string canonicalFallbackID)
        {
            return CreateWithCanonicalFallback(primaryID, canonicalFallbackID, KindAny);
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
            string canonicalPrimaryID = UCultureInfo.GetFullName(primaryID);
            return new LocaleKey(primaryID, canonicalPrimaryID, canonicalFallbackID, kind);
        }

        /// <summary>
        /// Create a <see cref="LocaleKey"/> with canonical primary and fallback IDs.
        /// </summary>
        public static LocaleKey CreateWithCanonical(UCultureInfo locale, string canonicalFallbackID, int kind)
        {
            if (locale == null)
            {
                return null;
            }
            string canonicalPrimaryID = locale.FullName;
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
                //if (idx == 4 && canonicalPrimaryID.RegionMatches(true, 0, "root", 0, 4))
                if (idx == 4 && canonicalPrimaryID.IndexOf("root", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
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
        /// Return the prefix associated with the kind, or null if the kind is <see cref="KindAny"/>.
        /// </summary>
        public virtual string Prefix
        {
            get { return kind == KindAny ? null : Kind.ToString(CultureInfo.InvariantCulture); }
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
        public override string GetCurrentDescriptor() // ICU4N specific - added "Get"
        {
            string result = CurrentID;
            if (result != null)
            {
                StringBuilder buf = new StringBuilder(); // default capacity 16 is usually good enough
                if (kind != KindAny)
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
        /// Convenience method to return the <see cref="UCultureInfo"/> corresponding to the (canonical) original ID.
        /// </summary>
        public virtual UCultureInfo GetCanonicalCulture() // ICU4N specific - added "Get"
        {
            return new UCultureInfo(primaryID);
        }

        /// <summary>
        /// Convenience method to return the <see cref="UCultureInfo"/> corresponding to the (canonical) currentID.
        /// </summary>
        public virtual UCultureInfo GetCurrentCulture() // ICU4N specific - added "Get"
        {
            if (varstart == -1)
            {
                return new UCultureInfo(currentID);
            }
            else
            {
                return new UCultureInfo(currentID + primaryID.Substring(varstart));
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
    public abstract class LocaleKeyFactory : IServiceFactory
    {
        protected readonly string m_name;
        protected readonly bool m_visible;

        public const bool Visible = true;
        public const bool Invisible = false;

        /// <summary>
        /// Constructor used by subclasses.
        /// </summary>
        protected LocaleKeyFactory(bool visible)
        {
            this.m_visible = visible;
            this.m_name = null;
        }

        /// <summary>
        /// Constructor used by subclasses.
        /// </summary>
        protected LocaleKeyFactory(bool visible, string name)
        {
            this.m_visible = visible;
            this.m_name = name;
        }

        /// <summary>
        /// Implement superclass abstract method.  This checks the <see cref="ICUServiceKey.CurrentID"/>
        /// against the supported IDs, and passes the canonicalLocale and
        /// <see cref="LocaleKey.Kind"/> to <see cref="HandleCreate(UCultureInfo, int, ICUService)"/> (which subclasses must implement).
        /// </summary>
        public virtual object Create(ICUServiceKey key, ICUService service)
        {
            if (HandlesKey(key))
            {
                LocaleKey lkey = (LocaleKey)key;
                int kind = lkey.Kind;

                UCultureInfo uloc = lkey.GetCurrentCulture();
                return HandleCreate(uloc, kind, service);
            }
            else
            {
                // System.out.println("factory: " + this + " did not support id: " + key.currentID());
                // System.out.println("supported ids: " + getSupportedIDs());
            }
            return null;
        }

        protected virtual bool HandlesKey(ICUServiceKey key)
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
        /// Implementation of <see cref="IServiceFactory"/> method.
        /// </summary>
        public virtual void UpdateVisibleIDs(IDictionary<string, IServiceFactory> result)
        {
            ICollection<string> cache = GetSupportedIDs();
            foreach (string id in cache)
            {
                if (m_visible)
                {
                    result[id] = this;
                }
                else
                {
                    result.Remove(id);
                }
            }
        }

        public virtual string GetDisplayName(string id, UCultureInfo locale)
        {
            // assume if the user called this on us, we must have handled some fallback of this id
            //          if (isSupportedID(id)) {
            if (locale == null)
            {
                return id;
            }
            UCultureInfo loc = new UCultureInfo(id);
            return loc.GetDisplayName(locale);
            //              }
            //          return null;
        }

        //CLOVER:OFF
        /// <summary>
        /// Utility method used by <see cref="Create(ICUServiceKey, ICUService)"/>.  Subclasses can
        /// implement this instead of <see cref="Create(ICUServiceKey, ICUService)"/>.
        /// </summary>
        protected virtual object HandleCreate(UCultureInfo loc, int kind, ICUService service)
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
            if (m_name != null)
            {
                buf.Append(", name: ");
                buf.Append(m_name);
            }
            buf.Append(", visible: ");
            buf.Append(m_visible);
            return buf.ToString();
        }
    }

    /// <summary>
    /// A <see cref="LocaleKeyFactory"/> that just returns a single object for a kind/locale.
    /// </summary>
    public class SimpleLocaleKeyFactory : LocaleKeyFactory
    {
        private readonly object obj;
        private readonly string id;
        private readonly int kind;

        // TODO: remove when we no longer need this
        public SimpleLocaleKeyFactory(object obj, UCultureInfo locale, int kind, bool visible)
            : this(obj, locale, kind, visible, null)
        {
        }

        public SimpleLocaleKeyFactory(object obj, UCultureInfo locale, int kind, bool visible, string name)
            : base(visible, name)
        {
            this.obj = obj;
            this.id = locale.Name;
            this.kind = kind;
        }

        /// <summary>
        /// Returns the service object if kind/locale match.  Service is not used.
        /// </summary>
        public override object Create(ICUServiceKey key, ICUService service)
        {
            if (!(key is LocaleKey))
            {
                return null;
            }

            LocaleKey lkey = (LocaleKey)key;
            if (kind != LocaleKey.KindAny && kind != lkey.Kind)
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

        public override void UpdateVisibleIDs(IDictionary<string, IServiceFactory> result)
        {
            if (m_visible)
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
    /// IDs.  Subclasses then override <see cref="HandleCreate(UCultureInfo, int, ICUService)"/> to create the actual service
    /// object.  The default implementation returns a <see cref="System.Resources.ResourceManager"/>.
    /// </summary>
    public class ICUResourceBundleFactory : LocaleKeyFactory // ICU4N TODO: API Rename ICUResourceManagerFactory ?
    {
        protected readonly string bundleName; // ICU4N TODO: API - rename baseName ?? Need to work out how to map this to ResourceManager

        /// <summary>
        /// Convenience constructor that uses the main ICU bundle name.
        /// </summary>
        public ICUResourceBundleFactory()
            : this(ICUData.IcuBaseName)
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
        public override void UpdateVisibleIDs(IDictionary<string, IServiceFactory> result)
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
        protected override object HandleCreate(UCultureInfo loc, int kind, ICUService service)
        {
            return ICUResourceBundle.GetBundleInstance(bundleName, loc, Assembly);
        }

        protected virtual Assembly Assembly
        {
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            get { return GetType().GetTypeInfo().Assembly; }
#else
            get { return GetType().Assembly; }
#endif
        }

        public override string ToString()
        {
            return base.ToString() + ", bundle: " + bundleName;
        }
    }

}
