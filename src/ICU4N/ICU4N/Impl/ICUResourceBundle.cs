using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using ReaderValue = ICU4N.Impl.ICUResourceBundleReader.ReaderValue; // ICU4N TODO: API - de-nest?

namespace ICU4N.Impl
{
    public class ICUResourceBundle : UResourceBundle
    {
        /**
     * CLDR string value "∅∅∅" prevents fallback to the parent bundle.
     */
        public static readonly string NO_INHERITANCE_MARKER = "\u2205\u2205\u2205";

        /**
         * The class loader constant to be used with getBundleInstance API
         */
        public static readonly Assembly ICU_DATA_CLASS_LOADER = typeof(ICUData).GetTypeInfo().Assembly; //ClassLoaderUtil.getClassLoader(ICUData.class);

        /**
         * The name of the resource containing the installed locales
         */
        protected static readonly string INSTALLED_LOCALES = "InstalledLocales";

        /**
         * Fields for a whole bundle, rather than any specific resource in the bundle.
         * Corresponds roughly to ICU4C/source/common/uresimp.h struct UResourceDataEntry.
         */
        protected internal sealed class WholeBundle
        {
            internal WholeBundle(string baseName, string localeID, Assembly loader,
                    ICUResourceBundleReader reader)
            {
                this.baseName = baseName;
                this.localeID = localeID;
                this.ulocale = new ULocale(localeID);
                this.loader = loader;
                this.reader = reader;
            }

            internal string baseName;
            internal string localeID;
            internal ULocale ulocale;
            internal Assembly loader;

            /**
             * Access to the bits and bytes of the resource bundle.
             * Hides low-level details.
             */
            internal ICUResourceBundleReader reader;

            // TODO: Remove topLevelKeys when we upgrade to Java 6 where ResourceBundle caches the keySet().
            internal ISet<string> topLevelKeys;
        }

        internal WholeBundle wholeBundle;
        private ICUResourceBundle container;

        /** Loader for bundle instances, for caching. */
        private abstract class Loader
        {
            internal abstract ICUResourceBundle Load();
        }

        private class BundleCache : SoftCache<string, ICUResourceBundle, Loader>
        {
            protected override ICUResourceBundle CreateInstance(string unusedKey, Loader loader)
            {
                return loader.Load();
            }
        }

        private static CacheBase<String, ICUResourceBundle, Loader> BUNDLE_CACHE = new BundleCache();

        /**
         * Returns a functionally equivalent locale, considering keywords as well, for the specified keyword.
         * @param baseName resource specifier
         * @param resName top level resource to consider (such as "collations")
         * @param keyword a particular keyword to consider (such as "collation" )
         * @param locID The requested locale
         * @param isAvailable If non-null, 1-element array of fillin parameter that indicates whether the
         * requested locale was available. The locale is defined as 'available' if it physically
         * exists within the specified tree and included in 'InstalledLocales'.
         * @param omitDefault  if true, omit keyword and value if default.
         * 'de_DE\@collation=standard' -> 'de_DE'
         * @return the locale
         * @internal ICU 3.0
         */
        public static ULocale GetFunctionalEquivalent(string baseName, Assembly loader,
                string resName, string keyword, ULocale locID,
                bool[] isAvailable, bool omitDefault)
        {
            string kwVal = locID.GetKeywordValue(keyword);
            string baseLoc = locID.GetBaseName();
            string defStr = null;
            ULocale parent = new ULocale(baseLoc);
            ULocale defLoc = null; // locale where default (found) resource is
            bool lookForDefault = false; // true if kwVal needs to be set
            ULocale fullBase = null; // base locale of found (target) resource
            int defDepth = 0; // depth of 'default' marker
            int resDepth = 0; // depth of found resource;

            if ((kwVal == null) || (kwVal.Length == 0)
                    || kwVal.Equals(DEFAULT_TAG))
            {
                kwVal = ""; // default tag is treated as no keyword
                lookForDefault = true;
            }

            // Check top level locale first
            ICUResourceBundle r = null;

            r = (ICUResourceBundle)UResourceBundle.GetBundleInstance(baseName, parent);
            if (isAvailable != null)
            {
                isAvailable[0] = false;
                ULocale[] availableULocales = GetAvailEntry(baseName, loader).GetULocaleList();
                for (int i = 0; i < availableULocales.Length; i++)
                {
                    if (parent.Equals(availableULocales[i]))
                    {
                        isAvailable[0] = true;
                        break;
                    }
                }
            }
            // determine in which locale (if any) the currently relevant 'default' is
            do
            {
                try
                {
                    ICUResourceBundle irb = (ICUResourceBundle)r.Get(resName);
                    defStr = irb.GetString(DEFAULT_TAG);
                    if (lookForDefault == true)
                    {
                        kwVal = defStr;
                        lookForDefault = false;
                    }
                    defLoc = r.GetULocale();
                }
                catch (MissingManifestResourceException t)
                {
                    // Ignore error and continue search.
                }
                if (defLoc == null)
                {
                    r = r.Parent;
                    defDepth++;
                }
            } while ((r != null) && (defLoc == null));

            // Now, search for the named resource
            parent = new ULocale(baseLoc);
            r = (ICUResourceBundle)UResourceBundle.GetBundleInstance(baseName, parent);
            // determine in which locale (if any) the named resource is located
            do
            {
                try
                {
                    ICUResourceBundle irb = (ICUResourceBundle)r.Get(resName);
                    /* UResourceBundle urb = */
                    irb.Get(kwVal);
                    fullBase = irb.GetULocale();
                    // If the get() completed, we have the full base locale
                    // If we fell back to an ancestor of the old 'default',
                    // we need to re calculate the "default" keyword.
                    if ((fullBase != null) && ((resDepth) > defDepth))
                    {
                        defStr = irb.GetString(DEFAULT_TAG);
                        defLoc = r.GetULocale();
                        defDepth = resDepth;
                    }
                }
                catch (MissingManifestResourceException t)
                {
                    // Ignore error,
                }
                if (fullBase == null)
                {
                    r = r.Parent;
                    resDepth++;
                }
            } while ((r != null) && (fullBase == null));

            if (fullBase == null && // Could not find resource 'kwVal'
                    (defStr != null) && // default was defined
                    !defStr.Equals(kwVal))
            { // kwVal is not default
              // couldn't find requested resource. Fall back to default.
                kwVal = defStr; // Fall back to default.
                parent = new ULocale(baseLoc);
                r = (ICUResourceBundle)UResourceBundle.GetBundleInstance(baseName, parent);
                resDepth = 0;
                // determine in which locale (if any) the named resource is located
                do
                {
                    try
                    {
                        ICUResourceBundle irb = (ICUResourceBundle)r.Get(resName);
                        ICUResourceBundle urb = (ICUResourceBundle)irb.Get(kwVal);

                        // if we didn't fail before this..
                        fullBase = r.GetULocale();

                        // If the fetched item (urb) is in a different locale than our outer locale (r/fullBase)
                        // then we are in a 'fallback' situation. treat as a missing resource situation.
                        if (!fullBase.GetBaseName().Equals(urb.GetULocale().GetBaseName()))
                        {
                            fullBase = null; // fallback condition. Loop and try again.
                        }

                        // If we fell back to an ancestor of the old 'default',
                        // we need to re calculate the "default" keyword.
                        if ((fullBase != null) && ((resDepth) > defDepth))
                        {
                            defStr = irb.GetString(DEFAULT_TAG);
                            defLoc = r.GetULocale();
                            defDepth = resDepth;
                        }
                    }
                    catch (MissingManifestResourceException t)
                    {
                        // Ignore error, continue search.
                    }
                    if (fullBase == null)
                    {
                        r = r.Parent;
                        resDepth++;
                    }
                } while ((r != null) && (fullBase == null));
            }

            if (fullBase == null)
            {
                throw new MissingManifestResourceException(
                    "Could not find locale containing requested or default keyword." +
                    " BaseName: " + baseName + ", " + keyword + "=" + kwVal);
            }

            if (omitDefault
                && defStr.Equals(kwVal) // if default was requested and
                && resDepth <= defDepth)
            { // default was set in same locale or child
                return fullBase; // Keyword value is default - no keyword needed in locale
            }
            else
            {
                return new ULocale(fullBase.GetBaseName() + "@" + keyword + "=" + kwVal);
            }
        }

        /**
         * Given a tree path and keyword, return a string enumeration of all possible values for that keyword.
         * @param baseName resource specifier
         * @param keyword a particular keyword to consider, must match a top level resource name
         * within the tree. (i.e. "collations")
         * @internal ICU 3.0
         */
        public static string[] GetKeywordValues(string baseName, string keyword)
        {
            ISet<string> keywords = new HashSet<string>();
            ULocale[] locales = GetAvailEntry(baseName, ICU_DATA_CLASS_LOADER).GetULocaleList();
            int i;

            for (i = 0; i < locales.Length; i++)
            {
                try
                {
                    UResourceBundle b = UResourceBundle.GetBundleInstance(baseName, locales[i]); // ICU4N TODO: Pass assembly ?
                    // downcast to ICUResourceBundle?
                    ICUResourceBundle irb = (ICUResourceBundle)(b.GetObject(keyword));
                    using (var e = irb.GetKeys().GetEnumerator())
                    {
                        while (e.MoveNext())
                        {
                            string s = e.Current;
                            if (!DEFAULT_TAG.Equals(s) && !s.StartsWith("private-", StringComparison.Ordinal))
                            {
                                // don't add 'default' items, nor unlisted types
                                keywords.Add(s);
                            }
                        }
                    }
                }
                catch (Exception t)
                {
                    //System.err.println("Error in - " + new Integer(i).toString()
                    // + " - " + t.toString());
                    // ignore the err - just skip that resource
                }
            }
            return keywords.ToArray();
        }

        /**
         * This method performs multilevel fallback for fetching items from the
         * bundle e.g: If resource is in the form de__PHONEBOOK{ collations{
         * default{ "phonebook"} } } If the value of "default" key needs to be
         * accessed, then do: <code>
         *  UResourceBundle bundle = UResourceBundle.getBundleInstance("de__PHONEBOOK");
         *  ICUResourceBundle result = null;
         *  if(bundle instanceof ICUResourceBundle){
         *      result = ((ICUResourceBundle) bundle).getWithFallback("collations/default");
         *  }
         * </code>
         *
         * @param path The path to the required resource key
         * @return resource represented by the key
         * @exception MissingResourceException If a resource was not found.
         */
        public virtual ICUResourceBundle GetWithFallback(string path)
        {
            ICUResourceBundle actualBundle = this;

            // now recurse to pick up sub levels of the items
            ICUResourceBundle result = FindResourceWithFallback(path, actualBundle, null);

            if (result == null)
            {
                throw new MissingManifestResourceException(
                    "Can't find resource for bundle "
                    + this.GetType().FullName + ", key " + Type + ", path " +
                    path);
            }

            if (result.Type == STRING && result.GetString().Equals(NO_INHERITANCE_MARKER))
            {
                throw new MissingManifestResourceException(string.Format("Encountered NO_INHERITANCE_MARKER, path: {0}, key: {1}", path, Key));
            }

            return result;
        }

        public virtual ICUResourceBundle At(int index)
        {
            return (ICUResourceBundle)HandleGet(index, null, this);
        }

        public virtual ICUResourceBundle At(String key)
        {
            // don't ever presume the key is an int in disguise, like ResourceArray does.
            if (this is ICUResourceBundleImpl.ResourceTable)
            {
                return (ICUResourceBundle)HandleGet(key, null, this);
            }
            return null;
        }

        new public ICUResourceBundle FindTopLevel(int index)
        {
            return (ICUResourceBundle)base.FindTopLevel(index);
        }

        new public ICUResourceBundle FindTopLevel(string aKey)
        {
            return (ICUResourceBundle)base.FindTopLevel(aKey);
        }

        /**
         * Like getWithFallback, but returns null if the resource is not found instead of
         * throwing an exception.
         * @param path the path to the resource
         * @return the resource, or null
         */
        public virtual ICUResourceBundle FindWithFallback(string path)
        {
            return FindResourceWithFallback(path, this, null);
        }
        public virtual string FindStringWithFallback(string path)
        {
            return FindStringWithFallback(path, this, null);
        }

        // will throw type mismatch exception if the resource is not a string
        public virtual string GetStringWithFallback(string path)
        {
            // Optimized form of getWithFallback(path).getString();
            ICUResourceBundle actualBundle = this;
            string result = FindStringWithFallback(path, actualBundle, null);

            if (result == null)
            {
                throw new MissingManifestResourceException(
                    "Can't find resource for bundle "
                    + this.GetType().FullName + ", key " + Type + ", path " + path);
            }

            if (result.Equals(NO_INHERITANCE_MARKER))
            {
                throw new MissingManifestResourceException(string.Format("Encountered NO_INHERITANCE_MARKER, path: {0}, key: {1}", path, Key));
            }
            return result;
        }

        public virtual void GetAllItemsWithFallbackNoFail(string path, UResource.Sink sink) // ICU4N TODO: Change to TryGetAllItemsWithFallback
        {
            try
            {
                GetAllItemsWithFallback(path, sink);
            }
            catch (MissingManifestResourceException e)
            {
                // Quietly ignore the exception.
            }
        }

        public virtual void GetAllItemsWithFallback(string path, UResource.Sink sink)
        {
            // Collect existing and parsed key objects into an array of keys,
            // rather than assembling and parsing paths.
            int numPathKeys = CountPathKeys(path);  // How much deeper does the path go?
            ICUResourceBundle rb;
            if (numPathKeys == 0)
            {
                rb = this;
            }
            else
            {
                // Get the keys for finding the target.
                int depth = GetResDepth();  // How deep are we in this bundle?
                string[] pathKeys = new string[depth + numPathKeys];
                GetResPathKeys(path, numPathKeys, pathKeys, depth);
                rb = FindResourceWithFallback(pathKeys, depth, this, null);
                if (rb == null)
                {
                    throw new MissingManifestResourceException(
                        "Can't find resource for bundle "
                        + this.GetType().FullName + ", key " + Type + ", path " + path);
                }
            }
            UResource.Key key = new UResource.Key();
            ReaderValue readerValue = new ReaderValue();
            rb.GetAllItemsWithFallback(key, readerValue, sink);
        }

        private void GetAllItemsWithFallback(
                UResource.Key key, ReaderValue readerValue, UResource.Sink sink)
        {
            // We recursively enumerate child-first,
            // only storing parent items in the absence of child items.
            // The sink needs to store a placeholder value for the no-fallback/no-inheritance marker
            // to prevent a parent item from being stored.
            //
            // It would be possible to recursively enumerate parent-first,
            // overriding parent items with child items.
            // When the sink sees the no-fallback/no-inheritance marker,
            // then it would remove the parent's item.
            // We would deserialize parent values even though they are overridden in a child bundle.
            ICUResourceBundleImpl impl = (ICUResourceBundleImpl)this;
            readerValue.reader = impl.wholeBundle.reader;
            readerValue.res = impl.GetResource();
            key.SetString(this.key != null ? this.key : "");
            sink.Put(key, readerValue, parent == null);
            if (parent != null)
            {
                // We might try to query the sink whether
                // any fallback from the parent bundle is still possible.
                ICUResourceBundle parentBundle = (ICUResourceBundle)parent;
                ICUResourceBundle rb;
                int depth = GetResDepth();
                if (depth == 0)
                {
                    rb = parentBundle;
                }
                else
                {
                    // Re-fetch the path keys: They may differ from the original ones
                    // if we had followed an alias.
                    string[] pathKeys = new string[depth];
                    GetResPathKeys(pathKeys, depth);
                    rb = FindResourceWithFallback(pathKeys, 0, parentBundle, null);
                }
                if (rb != null)
                {
                    rb.GetAllItemsWithFallback(key, readerValue, sink);
                }
            }
        }

        /**
         * Return a set of the locale names supported by a collection of resource
         * bundles.
         *
         * @param bundlePrefix the prefix of the resource bundles to use.
         */
        public static ISet<string> GetAvailableLocaleNameSet(string bundlePrefix, Assembly loader)
        {
            return GetAvailEntry(bundlePrefix, loader).GetLocaleNameSet();
        }

        /**
         * Return a set of all the locale names supported by a collection of
         * resource bundles.
         */
        public static ISet<string> GetFullLocaleNameSet()
        {
            return GetFullLocaleNameSet(ICUData.ICU_BASE_NAME, ICU_DATA_CLASS_LOADER);
        }

        /**
         * Return a set of all the locale names supported by a collection of
         * resource bundles.
         *
         * @param bundlePrefix the prefix of the resource bundles to use.
         */
        public static ISet<String> GetFullLocaleNameSet(string bundlePrefix, Assembly loader)
        {
            return GetAvailEntry(bundlePrefix, loader).GetFullLocaleNameSet();
        }

        /**
         * Return a set of the locale names supported by a collection of resource
         * bundles.
         */
        public static ISet<string> GetAvailableLocaleNameSet()
        {
            return GetAvailableLocaleNameSet(ICUData.ICU_BASE_NAME, ICU_DATA_CLASS_LOADER);
        }

        /**
         * Get the set of Locales installed in the specified bundles.
         * @return the list of available locales
         */
        public static ULocale[] GetAvailableULocales(string baseName, Assembly loader)
        {
            return GetAvailEntry(baseName, loader).GetULocaleList();
        }

        /**
         * Get the set of ULocales installed the base bundle.
         * @return the list of available locales
         */
        public static ULocale[] GetAvailableULocales()
        {
            return GetAvailableULocales(ICUData.ICU_BASE_NAME, ICU_DATA_CLASS_LOADER);
        }

        /**
         * Get the set of Locales installed in the specified bundles.
         * @return the list of available locales
         */
        public static CultureInfo[] GetAvailableLocales(string baseName, Assembly loader)
        {
            return GetAvailEntry(baseName, loader).GetLocaleList();
        }

        /**
          * Get the set of Locales installed the base bundle.
          * @return the list of available locales
          */
        public static CultureInfo[] GetAvailableLocales()
        {
            return GetAvailEntry(ICUData.ICU_BASE_NAME, ICU_DATA_CLASS_LOADER).GetLocaleList();
        }

        /**
         * Convert a list of ULocales to a list of Locales.  ULocales with a script code will not be converted
         * since they cannot be represented as a Locale.  This means that the two lists will <b>not</b> match
         * one-to-one, and that the returned list might be shorter than the input list.
         * @param ulocales a list of ULocales to convert to a list of Locales.
         * @return the list of converted ULocales
         */
        public static CultureInfo[] GetLocaleList(ULocale[] ulocales)
        {
            List<CultureInfo> list = new List<CultureInfo>(ulocales.Length);
            HashSet<CultureInfo> uniqueSet = new HashSet<CultureInfo>();
            for (int i = 0; i < ulocales.Length; i++)
            {
                CultureInfo loc = ulocales[i].ToLocale();
                if (!uniqueSet.Contains(loc))
                {
                    list.Add(loc);
                    uniqueSet.Add(loc);
                }
            }
            return list.ToArray();
        }

        /**
         * Returns the locale of this resource bundle. This method can be used after
         * a call to getBundle() to determine whether the resource bundle returned
         * really corresponds to the requested locale or is a fallback.
         *
         * @return the locale of this resource bundle
         */
        public override CultureInfo GetLocale()
        {
            return GetULocale().ToLocale();
        }


        // ========== privates ==========
        private static readonly string ICU_RESOURCE_INDEX = "res_index";

        private static readonly string DEFAULT_TAG = "default";

        // The name of text file generated by ICU4J build script including all locale names
        // (canonical, alias and root)
        private static readonly string FULL_LOCALE_NAMES_LIST = "fullLocaleNames.lst";

        // Flag for enabling/disabling debugging code
        private static readonly bool DEBUG = ICUDebug.Enabled("localedata");

        private static ULocale[] CreateULocaleList(string baseName,
                Assembly root)
        {
            // the canned list is a subset of all the available .res files, the idea
            // is we don't export them
            // all. gotta be a better way to do this, since to add a locale you have
            // to update this list,
            // and it's embedded in our binary resources.
            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.InstantiateBundle(baseName, ICU_RESOURCE_INDEX, root, true);

            bundle = (ICUResourceBundle)bundle.Get(INSTALLED_LOCALES);
            int length = bundle.Length;
            int i = 0;
            ULocale[] locales = new ULocale[length];
            using (UResourceBundleEnumerator iter = bundle.GetEnumerator())
            {
                iter.Reset();
                while (iter.MoveNext())
                {
                    string locstr = iter.Current.Key;
                    if (locstr.Equals("root"))
                    {
                        locales[i++] = ULocale.ROOT;
                    }
                    else
                    {
                        locales[i++] = new ULocale(locstr);
                    }
                }
            }
            bundle = null;
            return locales;
        }

        // Same as createULocaleList() but catches the MissingResourceException
        // and returns the data in a different form.
        private static void AddLocaleIDsFromIndexBundle(string baseName,
                Assembly root, ISet<string> locales)
        {
            ICUResourceBundle bundle;
            try
            {
                bundle = (ICUResourceBundle)UResourceBundle.InstantiateBundle(baseName, ICU_RESOURCE_INDEX, root, true);
                bundle = (ICUResourceBundle)bundle.Get(INSTALLED_LOCALES);
            }
            catch (MissingManifestResourceException e)
            {
                if (DEBUG)
                {
                    Console.Out.WriteLine("couldn't find " + baseName + '/' + ICU_RESOURCE_INDEX + ".res");
                    //Thread.dumpStack(); // ICU4N TODO
                }
                return;
            }
            using (UResourceBundleEnumerator iter = bundle.GetEnumerator())
            {
                iter.Reset();
                while (iter.MoveNext())
                {
                    string locstr = iter.Current.Key;
                    locales.Add(locstr);
                }
            }
        }

        private static void AddBundleBaseNamesFromClassLoader(
                string bn, Assembly root, ISet<string> names)
        {
            // ICU4N: Convert to .NET style base name
            string suffix = bn.Replace('/', '.').Replace('.' + ICUData.PACKAGE_NAME, "");
            string baseName = root.GetManifestResourceBaseName(suffix);
            foreach (var s in root.GetManifestResourceNames()
                .Where(name => name.StartsWith(baseName, StringComparison.Ordinal))
                .Select(n => n.Replace(baseName, "")))
            {
                if (s.EndsWith(".res", StringComparison.Ordinal))
                {
                    string locstr = s.Substring(0, s.Length - 4);
                    names.Add(locstr);
                }
            }
        }

        private static void AddLocaleIDsFromListFile(string bn, Assembly root, ISet<string> locales)
        {
            try
            {
                using (Stream s = root.FindAndGetManifestResourceStream(bn + FULL_LOCALE_NAMES_LIST))
                {
                    if (s != null)
                    {
                        using (TextReader br = new StreamReader(s, Encoding.ASCII))
                        {
                            string line;
                            while ((line = br.ReadLine()) != null)
                            {
                                if (line.Length != 0 && !line.StartsWith("#", StringComparison.Ordinal))
                                {
                                    locales.Add(line);
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException/* ignored */)
            {
                // swallow it
            }
        }

        private static ISet<string> CreateFullLocaleNameSet(string baseName, Assembly loader)
        {
            string bn = baseName.EndsWith("/", StringComparison.Ordinal) ? baseName : baseName + "/";
            ISet<string> set = new HashSet<string>();
            string skipScan = ICUConfig.Get("ICUResourceBundle.SkipRuntimeLocaleResourceScan", "false");
            if (!skipScan.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // scan available locale resources under the base url first
                AddBundleBaseNamesFromClassLoader(bn, loader, set);
                if (baseName.StartsWith(ICUData.ICU_BASE_NAME, StringComparison.Ordinal))
                {
                    string folder;
                    if (baseName.Length == ICUData.ICU_BASE_NAME.Length)
                    {
                        folder = "";
                    }
                    else if (baseName[ICUData.ICU_BASE_NAME.Length] == '/')
                    {
                        folder = baseName.Substring(ICUData.ICU_BASE_NAME.Length + 1);
                    }
                    else
                    {
                        folder = null;
                    }
                    if (folder != null)
                    {
                        ICUBinary.AddBaseNamesInFileFolder(folder, ".res", set);
                    }
                }
                set.Remove(ICU_RESOURCE_INDEX);  // "res_index"
                                                 // HACK: TODO: Figure out how we can distinguish locale data from other data items.

                var toRemove = new List<string>();
                using (var iter = set.GetEnumerator())
                {
                    while (iter.MoveNext())
                    {
                        string name = iter.Current;
                        if ((name.Length == 1 || name.Length > 3) && name.IndexOf('_') < 0)
                        {
                            // Does not look like a locale ID.
                            //iter.remove();
                            toRemove.Add(name);
                        }
                    }
                }
                // ICU4N: Remove items outside of the enumerator loop
                set.ExceptWith(toRemove);
            }
            // look for prebuilt full locale names list next
            if (set.Count == 0)
            {
                if (DEBUG) Console.Out.WriteLine("unable to enumerate data files in " + baseName);
                AddLocaleIDsFromListFile(bn, loader, set);
            }
            if (set.Count == 0)
            {
                // Use locale name set as the last resort fallback
                AddLocaleIDsFromIndexBundle(baseName, loader, set);
            }
            // We need to have the root locale in the set, but not as "root".
            set.Remove("root");
            set.Add(ULocale.ROOT.ToString());  // ""
            return (set).ToUnmodifiableSet();
        }

        private static ISet<string> CreateLocaleNameSet(string baseName, Assembly loader)
        {
            HashSet<string> set = new HashSet<string>();
            AddLocaleIDsFromIndexBundle(baseName, loader, set);
            return (set).ToUnmodifiableSet();
        }

        /**
         * Holds the prefix, and lazily creates the Locale[] list or the locale name
         * Set as needed.
         */
        private sealed class AvailEntry
        {
            private string prefix;
            private Assembly loader;
            private volatile ULocale[] ulocales;
            private volatile CultureInfo[] locales;
            private volatile ISet<string> nameSet;
            private volatile ISet<string> fullNameSet;

            internal AvailEntry(string prefix, Assembly loader)
            {
                this.prefix = prefix;
                this.loader = loader;
            }

            internal ULocale[] GetULocaleList()
            {
                if (ulocales == null)
                {
                    lock (this)
                    {
                        if (ulocales == null)
                        {
                            ulocales = CreateULocaleList(prefix, loader);
                        }
                    }
                }
                return ulocales;
            }
            internal CultureInfo[] GetLocaleList()
            {
                if (locales == null)
                {
                    GetULocaleList();
                    lock (this)
                    {
                        if (locales == null)
                        {
                            locales = ICUResourceBundle.GetLocaleList(ulocales);
                        }
                    }
                }
                return locales;
            }
            internal ISet<string> GetLocaleNameSet()
            {
                if (nameSet == null)
                {
                    lock (this)
                    {
                        if (nameSet == null)
                        {
                            nameSet = CreateLocaleNameSet(prefix, loader);
                        }
                    }
                }
                return nameSet;
            }
            internal ISet<string> GetFullLocaleNameSet()
            {
                // When there's no prebuilt index, we iterate through the jar files
                // and read the contents to build it.  If many threads try to read the
                // same jar at the same time, java thrashes.  Synchronize here
                // so that we can avoid this problem. We don't synchronize on the
                // other methods since they don't do this.
                //
                // This is the common entry point for access into the code that walks
                // through the resources, and is cached.  So it's a good place to lock
                // access.  Locking in the URLHandler doesn't give us a common object
                // to lock.
                if (fullNameSet == null)
                {
                    lock (this)
                    {
                        if (fullNameSet == null)
                        {
                            fullNameSet = CreateFullLocaleNameSet(prefix, loader);
                        }
                    }
                }
                return fullNameSet;
            }
        }

        private class AvailableEntryCache : SoftCache<string, AvailEntry, Assembly>
        {
            protected override AvailEntry CreateInstance(string key, Assembly loader)
            {
                return new AvailEntry(key, loader);
            }
        }

        /*
         * Cache used for AvailableEntry
         */
        private static CacheBase<String, AvailEntry, Assembly> GET_AVAILABLE_CACHE = new AvailableEntryCache();


        /**
         * Stores the locale information in a cache accessed by key (bundle prefix).
         * The cached objects are AvailEntries. The cache is implemented by SoftCache
         * so it can be GC'd.
         */
        private static AvailEntry GetAvailEntry(string key, Assembly loader)
        {
            return GET_AVAILABLE_CACHE.GetInstance(key, loader);
        }

        private static ICUResourceBundle FindResourceWithFallback(string path,
                UResourceBundle actualBundle, UResourceBundle requested)
        {
            if (path.Length == 0)
            {
                return null;
            }
            ICUResourceBundle @base = (ICUResourceBundle)actualBundle;
            // Collect existing and parsed key objects into an array of keys,
            // rather than assembling and parsing paths.
            int depth = @base.GetResDepth();
            int numPathKeys = CountPathKeys(path);
            Debug.Assert(numPathKeys > 0);
            string[] keys = new string[depth + numPathKeys];
            GetResPathKeys(path, numPathKeys, keys, depth);
            return FindResourceWithFallback(keys, depth, @base, requested);
        }

        private static ICUResourceBundle FindResourceWithFallback(
                string[] keys, int depth,
                ICUResourceBundle @base, UResourceBundle requested)
        {
            if (requested == null)
            {
                requested = @base;
            }

            for (; ; )
            {  // Iterate over the parent bundles.
                for (; ; )
                {  // Iterate over the keys on the requested path, within a bundle.
                    string subKey = keys[depth++];
                    ICUResourceBundle sub = (ICUResourceBundle)@base.HandleGet(subKey, null, requested);
                    if (sub == null)
                    {
                        --depth;
                        break;
                    }
                    if (depth == keys.Length)
                    {
                        // We found it.
                        return sub;
                    }
                    @base = sub;
                }
                // Try the parent bundle of the last-found resource.
                ICUResourceBundle nextBase = @base.Parent;
                if (nextBase == null)
                {
                    return null;
                }
                // If we followed an alias, then we may have switched bundle (locale) and key path.
                // Set the lower parts of the path according to the last-found resource.
                // This relies on a resource found via alias to have its original location information,
                // rather than the location of the alias.
                int baseDepth = @base.GetResDepth();
                if (depth != baseDepth)
                {
                    string[] newKeys = new string[baseDepth + (keys.Length - depth)];
                    System.Array.Copy(keys, depth, newKeys, baseDepth, keys.Length - depth);
                    keys = newKeys;
                }
                @base.GetResPathKeys(keys, baseDepth);
                @base = nextBase;
                depth = 0;  // getParent() returned a top level table resource.
            }
        }

        /**
         * Like findResourceWithFallback(...).getString() but with minimal creation of intermediate
         * ICUResourceBundle objects.
         */
        private static string FindStringWithFallback(string path,
                UResourceBundle actualBundle, UResourceBundle requested)
        {
            if (path.Length == 0)
            {
                return null;
            }
            if (!(actualBundle is ICUResourceBundleImpl.ResourceContainer))
            {
                return null;
            }
            if (requested == null)
            {
                requested = actualBundle;
            }

            ICUResourceBundle @base = (ICUResourceBundle)actualBundle;
            ICUResourceBundleReader reader = @base.wholeBundle.reader;
            int res = RES_BOGUS;

            // Collect existing and parsed key objects into an array of keys,
            // rather than assembling and parsing paths.
            int baseDepth = @base.GetResDepth();
            int depth = baseDepth;
            int numPathKeys = CountPathKeys(path);
            Debug.Assert(numPathKeys > 0);
            string[] keys = new string[depth + numPathKeys];
            GetResPathKeys(path, numPathKeys, keys, depth);

            for (; ; )
            {  // Iterate over the parent bundles.
                for (; ; )
                {  // Iterate over the keys on the requested path, within a bundle.
                    ICUResourceBundleReader.Container readerContainer;
                    if (res == RES_BOGUS)
                    {
                        int type = @base.Type;
                        if (type == TABLE || type == ARRAY)
                        {
                            readerContainer = ((ICUResourceBundleImpl.ResourceContainer)@base).value;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        int type = ICUResourceBundleReader.RES_GET_TYPE(res);
                        if (ICUResourceBundleReader.URES_IS_TABLE(type))
                        {
                            readerContainer = reader.GetTable(res);
                        }
                        else if (ICUResourceBundleReader.URES_IS_ARRAY(type))
                        {
                            readerContainer = reader.GetArray(res);
                        }
                        else
                        {
                            res = RES_BOGUS;
                            break;
                        }
                    }
                    string subKey = keys[depth++];
                    res = readerContainer.GetResource(reader, subKey);
                    if (res == RES_BOGUS)
                    {
                        --depth;
                        break;
                    }
                    ICUResourceBundle sub;
                    if (ICUResourceBundleReader.RES_GET_TYPE(res) == ALIAS)
                    {
                        @base.GetResPathKeys(keys, baseDepth);
                        sub = GetAliasedResource(@base, keys, depth, subKey, res, null, requested);
                    }
                    else
                    {
                        sub = null;
                    }
                    if (depth == keys.Length)
                    {
                        // We found it.
                        if (sub != null)
                        {
                            return sub.GetString();  // string from alias handling
                        }
                        else
                        {
                            string s = reader.GetString(res);
                            if (s == null)
                            {
                                throw new UResourceTypeMismatchException("");
                            }
                            return s;
                        }
                    }
                    if (sub != null)
                    {
                        @base = sub;
                        reader = @base.wholeBundle.reader;
                        res = RES_BOGUS;
                        // If we followed an alias, then we may have switched bundle (locale) and key path.
                        // Reserve space for the lower parts of the path according to the last-found resource.
                        // This relies on a resource found via alias to have its original location information,
                        // rather than the location of the alias.
                        baseDepth = @base.GetResDepth();
                        if (depth != baseDepth)
                        {
                            string[] newKeys = new string[baseDepth + (keys.Length - depth)];
                            System.Array.Copy(keys, depth, newKeys, baseDepth, keys.Length - depth);
                            keys = newKeys;
                            depth = baseDepth;
                        }
                    }
                }
                // Try the parent bundle of the last-found resource.
                ICUResourceBundle nextBase = @base.Parent;
                if (nextBase == null)
                {
                    return null;
                }
                // We probably have not yet set the lower parts of the key path.
                @base.GetResPathKeys(keys, baseDepth);
                @base = nextBase;
                reader = @base.wholeBundle.reader;
                depth = baseDepth = 0;  // getParent() returned a top level table resource.
            }
        }

        private int GetResDepth()
        {
            return (container == null) ? 0 : container.GetResDepth() + 1;
        }

        /**
         * Fills some of the keys array with the keys on the path to this resource object.
         * Writes the top-level key into index 0 and increments from there.
         *
         * @param keys
         * @param depth must be {@link #getResDepth()}
         */
        private void GetResPathKeys(string[] keys, int depth)
        {
            ICUResourceBundle b = this;
            while (depth > 0)
            {
                keys[--depth] = b.Key;
                b = b.container;
                Debug.Assert((depth == 0) == (b.container == null));
            }
        }

        private static int CountPathKeys(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }
            int num = 1;
            for (int i = 0; i < path.Length; ++i)
            {
                if (path[i] == RES_PATH_SEP_CHAR)
                {
                    ++num;
                }
            }
            return num;
        }

        /**
         * Fills some of the keys array (from start) with the num keys from the path string.
         *
         * @param path path string
         * @param num must be {@link #countPathKeys(String)}
         * @param keys
         * @param start index where the first path key is stored
         */
        private static void GetResPathKeys(string path, int num, string[] keys, int start)
        {
            if (num == 0)
            {
                return;
            }
            if (num == 1)
            {
                keys[start] = path;
                return;
            }
            int i = 0;
            for (; ; )
            {
                int j = path.IndexOf(RES_PATH_SEP_CHAR, i);
                Debug.Assert(j >= i);
                keys[start++] = path.Substring(i, j - i); // ICU4N: Corrected 2nd parameter
                if (num == 2)
                {
                    Debug.Assert(path.IndexOf(RES_PATH_SEP_CHAR, j + 1) < 0);
                    keys[start] = path.Substring(j + 1);
                    break;
                }
                else
                {
                    i = j + 1;
                    --num;
                }
            }
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other is ICUResourceBundle)
            {
                ICUResourceBundle o = (ICUResourceBundle)other;
                if (GetBaseName().Equals(o.GetBaseName())
                        && GetLocaleID().Equals(o.GetLocaleID()))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            Debug.Assert(false, "hashCode not designed");
            return 42;
        }

        public enum OpenType
        {  // C++ uresbund.cpp: enum UResOpenType
           /**
            * Open a resource bundle for the locale;
            * if there is not even a base language bundle, then fall back to the default locale;
            * if there is no bundle for that either, then load the root bundle.
            *
            * <p>This is the default bundle loading behavior.
            */
            LOCALE_DEFAULT_ROOT,
            // TODO: ICU ticket #11271 "consistent default locale across locale trees"
            // Add an option to look at the main locale tree for whether to
            // fall back to root directly (if the locale has main data) or
            // fall back to the default locale first (if the locale does not even have main data).
            /**
             * Open a resource bundle for the locale;
             * if there is not even a base language bundle, then load the root bundle;
             * never fall back to the default locale.
             *
             * <p>This is used for algorithms that have good pan-Unicode default behavior,
             * such as case mappings, collation, and segmentation (BreakIterator).
             */
            LOCALE_ROOT,
            /**
             * Open a resource bundle for the locale;
             * if there is not even a base language bundle, then fail;
             * never fall back to the default locale nor to the root locale.
             *
             * <p>This is used when fallback to another language is not desired
             * and the root locale is not generally useful.
             * For example, {@link com.ibm.icu.util.LocaleData#setNoSubstitute(boolean)}
             * or currency display names for {@link com.ibm.icu.text.LocaleDisplayNames}.
             */
            LOCALE_ONLY,
            /**
             * Open a resource bundle for the exact bundle name as requested;
             * no fallbacks, do not load parent bundles.
             *
             * <p>This is used for supplemental (non-locale) data.
             */
            DIRECT
        };

        // This method is for super class's instantiateBundle method
        new public static ICUResourceBundle GetBundleInstance(string baseName, string localeID,
            Assembly root, bool disableFallback)
        {
            return GetBundleInstance(baseName, localeID, root,
                    disableFallback ? OpenType.DIRECT : OpenType.LOCALE_DEFAULT_ROOT);
        }

        public static ICUResourceBundle GetBundleInstance(
            string baseName, ULocale locale, OpenType openType)
        {
            return GetBundleInstance(baseName, locale,
                    ICUResourceBundle.ICU_DATA_CLASS_LOADER, openType);
        }

        // ICU4N specific overload so we can pass the Assembly from submodules (since 
        // the main assembly won't see submodules by default).
        public static ICUResourceBundle GetBundleInstance(
            string baseName, ULocale locale, Assembly root, OpenType openType)
        {
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            return GetBundleInstance(baseName, locale.GetBaseName(),
                    root, openType);
        }

        public static ICUResourceBundle GetBundleInstance(string baseName, string localeID,
            Assembly root, OpenType openType)
        {
            if (baseName == null)
            {
                baseName = ICUData.ICU_BASE_NAME;
            }
            localeID = ULocale.GetBaseName(localeID);
            ICUResourceBundle b;
            if (openType == OpenType.LOCALE_DEFAULT_ROOT)
            {
                b = InstantiateBundle(baseName, localeID, ULocale.GetDefault().GetBaseName(),
                        root, openType);
            }
            else
            {
                b = InstantiateBundle(baseName, localeID, null, root, openType);
            }
            if (b == null)
            {
                throw new MissingManifestResourceException(
                        "Could not find the bundle " + baseName + "/" + localeID + ".res");
            }
            return b;
        }

        private static bool LocaleIDStartsWithLangSubtag(string localeID, string lang)
        {
            return localeID.StartsWith(lang, StringComparison.Ordinal) &&
                    (localeID.Length == lang.Length || localeID[lang.Length] == '_');
        }

        //private class BundleLoader : Loader
        //{
        //    private readonly Func<ICUResourceBundle> load;
        //    public BundleLoader(Func<ICUResourceBundle> load)
        //    {
        //        this.load = load;
        //    }

        //    internal override ICUResourceBundle Load()
        //    {
        //        return load();
        //    }
        //}

        private class BundleLoader : Loader
        {
            private readonly string baseName;
            private readonly string localeID;
            private readonly string defaultID;
            private readonly Assembly root;
            private readonly OpenType openType;
            private readonly string fullName;

            public BundleLoader(string baseName, string localeID, string defaultID,
                Assembly root, OpenType openType, string fullName)
            {
                this.baseName = baseName;
                this.localeID = localeID;
                this.defaultID = defaultID;
                this.root = root;
                this.openType = openType;
                this.fullName = fullName;
            }

            internal override ICUResourceBundle Load()
            {
                if (DEBUG) Console.Out.WriteLine("Creating " + fullName);
                // here we assume that java type resource bundle organization
                // is required then the base name contains '.' else
                // the resource organization is of ICU type
                // so clients can instantiate resources of the type
                // com.mycompany.data.MyLocaleElements_en.res and
                // com.mycompany.data.MyLocaleElements.res
                //
                string rootLocale = (baseName.IndexOf('.') == -1) ? "root" : "";
                string localeName = string.IsNullOrEmpty(localeID) ? rootLocale : localeID;
                ICUResourceBundle b = ICUResourceBundle.CreateBundle(baseName, localeName, root);

                if (DEBUG) Console.Out.WriteLine("The bundle created is: " + b + " and openType=" + openType + " and bundle.getNoFallback=" + (b != null && b.NoFallback));
                if (openType == OpenType.DIRECT || (b != null && b.NoFallback))
                {
                    // no fallback because the caller said so or because the bundle says so
                    //
                    // TODO for b!=null: In C++, ures_openDirect() builds the parent chain
                    // for its bundle unless its nofallback flag is set.
                    // Otherwise we get test failures.
                    // For example, item aliases are followed via ures_openDirect(),
                    // and fail if the target bundle needs fallbacks but the chain is not set.
                    // Figure out why Java does not build the parent chain
                    // for a bundle that does not have nofallback.
                    // Are the relevant test cases just disabled?
                    // Do item aliases not get followed via "direct" loading?
                    return b;
                }

                // fallback to locale ID parent
                if (b == null)
                {
                    int i = localeName.LastIndexOf('_');
                    if (i != -1)
                    {
                        // Chop off the last underscore and the subtag after that.
                        string temp = localeName.Substring(0, i - 0); // ICU4N: Checked 2nd parameter
                        b = InstantiateBundle(baseName, temp, defaultID, root, openType);
                    }
                    else
                    {
                        // No underscore, only a base language subtag.
                        if (openType == OpenType.LOCALE_DEFAULT_ROOT &&
                                !LocaleIDStartsWithLangSubtag(defaultID, localeName))
                        {
                            // Go to the default locale before root.
                            b = InstantiateBundle(baseName, defaultID, defaultID, root, openType);
                        }
                        else if (openType != OpenType.LOCALE_ONLY && !string.IsNullOrEmpty(rootLocale))
                        {
                            // Ultimately go to root.
                            b = ICUResourceBundle.CreateBundle(baseName, rootLocale, root);
                        }
                    }
                }
                else
                {
                    UResourceBundle parent = null;
                    localeName = b.GetLocaleID();
                    int i = localeName.LastIndexOf('_');

                    // TODO: C++ uresbund.cpp also checks for %%ParentIsRoot. Why not Java?
                    string parentLocaleName = ((ICUResourceBundleImpl.ResourceTable)b).FindString("%%Parent");
                    if (parentLocaleName != null)
                    {
                        parent = InstantiateBundle(baseName, parentLocaleName, defaultID, root, openType);
                    }
                    else if (i != -1)
                    {
                        parent = InstantiateBundle(baseName, localeName.Substring(0, i - 0), defaultID, root, openType); // ICU4N: Checked 2nd parameter
                    }
                    else if (!localeName.Equals(rootLocale))
                    {
                        parent = InstantiateBundle(baseName, rootLocale, defaultID, root, openType);
                    }

                    if (!b.Equals(parent))
                    {
                        b.SetParent(parent);
                    }
                }
                return b;
            }
        }

        private static ICUResourceBundle InstantiateBundle(
            string baseName, string localeID, string defaultID,
            Assembly root, OpenType openType)
        {
            Debug.Assert(localeID.IndexOf('@') < 0);
            Debug.Assert(defaultID == null || defaultID.IndexOf('@') < 0);
            string fullName = ICUResourceBundleReader.GetFullName(baseName, localeID);
            char openTypeChar = (char)('0' + (int)openType);
            string cacheKey = openType != OpenType.LOCALE_DEFAULT_ROOT ?
                    fullName + '#' + openTypeChar :
                        fullName + '#' + openTypeChar + '#' + defaultID;
            return BUNDLE_CACHE.GetInstance(cacheKey, new BundleLoader(baseName, localeID, defaultID, root, openType, fullName));

            //return BUNDLE_CACHE.GetInstance(cacheKey, new BundleLoader(load: () =>
            //{
            //    if (DEBUG) Console.Out.WriteLine("Creating " + fullName);
            //    // here we assume that java type resource bundle organization
            //    // is required then the base name contains '.' else
            //    // the resource organization is of ICU type
            //    // so clients can instantiate resources of the type
            //    // com.mycompany.data.MyLocaleElements_en.res and
            //    // com.mycompany.data.MyLocaleElements.res
            //    //
            //    string rootLocale = (baseName.IndexOf('.') == -1) ? "root" : "";
            //    string localeName = string.IsNullOrEmpty(localeID) ? rootLocale : localeID;
            //    ICUResourceBundle b = ICUResourceBundle.CreateBundle(baseName, localeName, root);

            //    if (DEBUG) Console.Out.WriteLine("The bundle created is: " + b + " and openType=" + openType + " and bundle.getNoFallback=" + (b != null && b.NoFallback));
            //    if (openType == OpenType.DIRECT || (b != null && b.NoFallback))
            //    {
            //        // no fallback because the caller said so or because the bundle says so
            //        //
            //        // TODO for b!=null: In C++, ures_openDirect() builds the parent chain
            //        // for its bundle unless its nofallback flag is set.
            //        // Otherwise we get test failures.
            //        // For example, item aliases are followed via ures_openDirect(),
            //        // and fail if the target bundle needs fallbacks but the chain is not set.
            //        // Figure out why Java does not build the parent chain
            //        // for a bundle that does not have nofallback.
            //        // Are the relevant test cases just disabled?
            //        // Do item aliases not get followed via "direct" loading?
            //        return b;
            //    }

            //    // fallback to locale ID parent
            //    if (b == null)
            //    {
            //        int i = localeName.LastIndexOf('_');
            //        if (i != -1)
            //        {
            //            // Chop off the last underscore and the subtag after that.
            //            string temp = localeName.Substring(0, i - 0); // ICU4N: Checked 2nd parameter
            //            b = InstantiateBundle(baseName, temp, defaultID, root, openType);
            //        }
            //        else
            //        {
            //            // No underscore, only a base language subtag.
            //            if (openType == OpenType.LOCALE_DEFAULT_ROOT &&
            //                    !LocaleIDStartsWithLangSubtag(defaultID, localeName))
            //            {
            //                // Go to the default locale before root.
            //                b = InstantiateBundle(baseName, defaultID, defaultID, root, openType);
            //            }
            //            else if (openType != OpenType.LOCALE_ONLY && !string.IsNullOrEmpty(rootLocale))
            //            {
            //                // Ultimately go to root.
            //                b = ICUResourceBundle.CreateBundle(baseName, rootLocale, root);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        UResourceBundle parent = null;
            //        localeName = b.GetLocaleID();
            //        int i = localeName.LastIndexOf('_');

            //        // TODO: C++ uresbund.cpp also checks for %%ParentIsRoot. Why not Java?
            //        string parentLocaleName = ((ICUResourceBundleImpl.ResourceTable)b).FindString("%%Parent");
            //        if (parentLocaleName != null)
            //        {
            //            parent = InstantiateBundle(baseName, parentLocaleName, defaultID, root, openType);
            //        }
            //        else if (i != -1)
            //        {
            //            parent = InstantiateBundle(baseName, localeName.Substring(0, i - 0), defaultID, root, openType); // ICU4N: Checked 2nd parameter
            //        }
            //        else if (!localeName.Equals(rootLocale))
            //        {
            //            parent = InstantiateBundle(baseName, rootLocale, defaultID, root, openType);
            //        }

            //        if (!b.Equals(parent))
            //        {
            //            b.SetParent(parent);
            //        }
            //    }
            //    return b;
            //}));
        }

        internal ICUResourceBundle Get(string aKey, IDictionary<string, string> aliasesVisited, UResourceBundle requested)
        {
            ICUResourceBundle obj = (ICUResourceBundle)HandleGet(aKey, aliasesVisited, requested);
            if (obj == null)
            {
                obj = this.Parent;
                if (obj != null)
                {
                    //call the get method to recursively fetch the resource
                    obj = obj.Get(aKey, aliasesVisited, requested);
                }
                if (obj == null)
                {
                    string fullName = ICUResourceBundleReader.GetFullName(GetBaseName(), GetLocaleID());
                    throw new MissingManifestResourceException(
                            "Can't find resource for bundle " + fullName + ", key "
                                    + aKey + ", type " + this.GetType().FullName);
                }
            }
            return obj;
        }

        /** Data member where the subclasses store the key. */
        protected string key;

        /**
         * A resource word value that means "no resource".
         * Note: 0xffffffff == -1
         * This has the same value as UResourceBundle.NONE, but they are semantically
         * different and should be used appropriately according to context:
         * NONE means "no type".
         * (The type of RES_BOGUS is RES_RESERVED=15 which was defined in ICU4C ures.h.)
         */
        public static readonly int RES_BOGUS = unchecked((int)0xffffffff);
        //blic static readonly int RES_MAX_OFFSET = 0x0fffffff;

        /**
         * Resource type constant for aliases;
         * internally stores a string which identifies the actual resource
         * storing the data (can be in a different resource bundle).
         * Resolved internally before delivering the actual resource through the API.
         */
        public const int ALIAS = 3;

        /** Resource type constant for tables with 32-bit count, key offsets and values. */
        public const int TABLE32 = 4;

        /**
         * Resource type constant for tables with 16-bit count, key offsets and values.
         * All values are STRING_V2 strings.
         */
        public const int TABLE16 = 5;

        /** Resource type constant for 16-bit Unicode strings in formatVersion 2. */
        public const int STRING_V2 = 6;

        /**
         * Resource type constant for arrays with 16-bit count and values.
         * All values are STRING_V2 strings.
         */
        public const int ARRAY16 = 9;

        /* Resource type 15 is not defined but effectively used by RES_BOGUS=0xffffffff. */

        /**
        * Create a bundle using a reader.
        * @param baseName The name for the bundle.
        * @param localeID The locale identification.
        * @param root The ClassLoader object root.
        * @return the new bundle
*/
        public static ICUResourceBundle CreateBundle(string baseName, string localeID, Assembly root)
        {
            ICUResourceBundleReader reader = ICUResourceBundleReader.GetReader(baseName, localeID, root);
            if (reader == null)
            {
                // could not open the .res file
                return null;
            }
            return GetBundle(reader, baseName, localeID, root);
        }

        protected override string GetLocaleID()
        {
            return wholeBundle.localeID;
        }

        protected internal override string GetBaseName()
        {
            return wholeBundle.baseName;
        }

        public override ULocale GetULocale()
        {
            return wholeBundle.ulocale;
        }

        /**
         * Returns true if this is the root bundle, or an item in the root bundle.
         */
        public virtual bool IsRoot
        {
            get { return string.IsNullOrEmpty(wholeBundle.localeID) || wholeBundle.localeID.Equals("root"); }
        }

        //public override ICUResourceBundle GetParent()
        //{
        //    return (ICUResourceBundle)parent;
        //}

        public override void SetParent(ResourceBundle parent)
        {
            this.parent = parent;
        }

        new public ICUResourceBundle Parent
        {
            get { return (ICUResourceBundle)parent; }
            set { this.parent = value; }
        }


        public override string Key
        {
            get { return key; }
        }

        /**
         * Get the noFallback flag specified in the loaded bundle.
         * @return The noFallback flag.
         */
        private bool NoFallback
        {
            get { return wholeBundle.reader.NoFallback; }
        }

        private static ICUResourceBundle GetBundle(ICUResourceBundleReader reader,
                                                   string baseName, string localeID,
                                                   Assembly loader)
        {
            ICUResourceBundleImpl.ResourceTable rootTable;
            int rootRes = reader.RootResource;
            if (ICUResourceBundleReader.URES_IS_TABLE(ICUResourceBundleReader.RES_GET_TYPE(rootRes)))
            {
                WholeBundle wb = new WholeBundle(baseName, localeID, loader, reader);
                rootTable = new ICUResourceBundleImpl.ResourceTable(wb, rootRes);
            }
            else
            {
                throw new InvalidOperationException("Invalid format error");
            }
            string aliasString = rootTable.FindString("%%ALIAS");
            if (aliasString != null)
            {
                return (ICUResourceBundle)UResourceBundle.GetBundleInstance(baseName, aliasString);
            }
            else
            {
                return rootTable;
            }
        }
        /**
         * Constructor for the root table of a bundle.
         */
        protected ICUResourceBundle(WholeBundle wholeBundle)
        {
            this.wholeBundle = wholeBundle;
        }
        // constructor for inner classes
        protected ICUResourceBundle(ICUResourceBundle container, string key)
        {
            this.key = key;
            wholeBundle = container.wholeBundle;
            this.container = container;
            parent = container.parent;
        }

        private static readonly char RES_PATH_SEP_CHAR = '/';
        private static readonly string RES_PATH_SEP_STR = "/";
        private static readonly string ICUDATA = "ICUDATA";
        private static readonly char HYPHEN = '-';
        private static readonly string LOCALE = "LOCALE";

        /**
         * Returns the resource object referred to from the alias _resource int's path string.
         * Throws MissingResourceException if not found.
         *
         * If the alias path does not contain a key path:
         * If keys != null then keys[:depth] is used.
         * Otherwise the base key path plus the key parameter is used.
         *
         * @param base A direct or indirect container of the alias.
         * @param keys The key path to the alias, or null. (const)
         * @param depth The length of the key path, if keys != null.
         * @param key The alias' own key within this current container, if keys == null.
         * @param _resource The alias resource int.
         * @param aliasesVisited Set of alias path strings already visited, for detecting loops.
         *        We cannot change the type (e.g., to Set<String>) because it is used
         *        in protected/@stable UResourceBundle methods.
         * @param requested The original resource object from which the lookup started,
         *        which is the starting point for "/LOCALE/..." aliases.
         * @return the aliased resource object
         */
        protected static ICUResourceBundle GetAliasedResource(
                ICUResourceBundle @base, string[] keys, int depth,
                string key, int _resource,
                IDictionary<string, string> aliasesVisited,
                UResourceBundle requested)
        {
            WholeBundle wholeBundle = @base.wholeBundle;
            Assembly loaderToUse = wholeBundle.loader;
            string locale;
            string keyPath = null;
            string bundleName;
            string rpath = wholeBundle.reader.GetAlias(_resource);
            if (aliasesVisited == null)
            {
                aliasesVisited = new Dictionary<string, string>();
            }
            if (aliasesVisited.Get(rpath) != null)
            {
                throw new ArgumentException(
                        "Circular references in the resource bundles");
            }
            aliasesVisited[rpath] = "";
            if (rpath.IndexOf(RES_PATH_SEP_CHAR) == 0)
            {
                int i = rpath.IndexOf(RES_PATH_SEP_CHAR, 1);
                int j = rpath.IndexOf(RES_PATH_SEP_CHAR, i + 1);
                bundleName = rpath.Substring(1, i - 1); // ICU4N: Corrected 2nd parameter
                if (j < 0)
                {
                    locale = rpath.Substring(i + 1);
                }
                else
                {
                    locale = rpath.Substring(i + 1, j - (i + 1)); // ICU4N: Corrected 2nd parameter
                    keyPath = rpath.Substring(j + 1, rpath.Length - (j + 1)); // ICU4N: Corrected 2nd parameter
                }
                //there is a path included
                if (bundleName.Equals(ICUDATA))
                {
                    bundleName = ICUData.ICU_BASE_NAME;
                    loaderToUse = ICU_DATA_CLASS_LOADER;
                }
                else if (bundleName.IndexOf(ICUDATA) > -1)
                {
                    int idx = bundleName.IndexOf(HYPHEN);
                    if (idx > -1)
                    {
                        bundleName = ICUData.ICU_BASE_NAME + RES_PATH_SEP_STR + bundleName.Substring(idx + 1, bundleName.Length - idx + 1); // ICU4N: Corrected 2nd parameter
                        loaderToUse = ICU_DATA_CLASS_LOADER;
                    }
                }
            }
            else
            {
                //no path start with locale
                int i = rpath.IndexOf(RES_PATH_SEP_CHAR);
                if (i != -1)
                {
                    locale = rpath.Substring(0, i - 0); // ICU4N: Checked 2nd parameter
                    keyPath = rpath.Substring(i + 1);
                }
                else
                {
                    locale = rpath;
                }
                bundleName = wholeBundle.baseName;
            }
            ICUResourceBundle bundle = null;
            ICUResourceBundle sub = null;
            if (bundleName.Equals(LOCALE))
            {
                bundleName = wholeBundle.baseName;
                keyPath = rpath.Substring(LOCALE.Length + 2/* prepending and appending / */, rpath.Length - (LOCALE.Length + 2)); // ICU4N: Corrected 2nd parameter

                // Get the top bundle of the requested bundle
                bundle = (ICUResourceBundle)requested;
                while (bundle.container != null)
                {
                    bundle = bundle.container;
                }
                sub = ICUResourceBundle.FindResourceWithFallback(keyPath, bundle, null);
            }
            else
            {
                bundle = GetBundleInstance(bundleName, locale, loaderToUse, false);

                int numKeys;
                if (keyPath != null)
                {
                    numKeys = CountPathKeys(keyPath);
                    if (numKeys > 0)
                    {
                        keys = new String[numKeys];
                        GetResPathKeys(keyPath, numKeys, keys, 0);
                    }
                }
                else if (keys != null)
                {
                    numKeys = depth;
                }
                else
                {
                    depth = @base.GetResDepth();
                    numKeys = depth + 1;
                    keys = new string[numKeys];
                    @base.GetResPathKeys(keys, depth);
                    keys[depth] = key;
                }
                if (numKeys > 0)
                {
                    sub = bundle;
                    for (int i = 0; sub != null && i < numKeys; ++i)
                    {
                        sub = sub.Get(keys[i], aliasesVisited, requested);
                    }
                }
            }
            if (sub == null)
            {
                throw new MissingManifestResourceException(string.Format("culture: {0}, baseName: {1}, key: {2}", wholeBundle.localeID, wholeBundle.baseName, key));
            }
            // TODO: If we know that sub is not cached,
            // then we should set its container and key to the alias' location,
            // so that it behaves as if its value had been copied into the alias location.
            // However, findResourceWithFallback() must reroute its bundle and key path
            // to where the alias data comes from.
            return sub;
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public ISet<string> GetTopLevelKeySet()
        {
            return wholeBundle.topLevelKeys;
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public void SetTopLevelKeySet(ISet<string> keySet)
        {
            wholeBundle.topLevelKeys = keySet;
        }

        // ICU4N TODO: finish implementation
    //    // This is the worker function for the public getKeys().
    //    // TODO: Now that UResourceBundle uses handleKeySet(), this function is obsolete.
    //    // It is also not inherited from ResourceBundle, and it is not implemented
    //    // by ResourceBundleWrapper despite its documentation requiring all subclasses to
    //    // implement it.
    //    // Consider deprecating UResourceBundle.handleGetKeys(), and consider making it always return null.
    //    @Override
    //protected Enumeration<string> HandleGetKeys()
    //    {
    //        return Collections.enumeration(HandleKeySet());
    //    }

        protected override bool IsTopLevelResource
        {
            get { return container == null; }
        }
    }
}
