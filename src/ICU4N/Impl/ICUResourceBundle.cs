using ICU4N.Globalization;
using ICU4N.Reflection;
using ICU4N.Resources;
using ICU4N.Support.Globalization;
using ICU4N.Util;
using J2N;
using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using JCG = J2N.Collections.Generic;
using ReaderValue = ICU4N.Impl.ICUResourceBundleReader.ReaderValue;

namespace ICU4N.Impl
{
    public class ICUResourceBundle : UResourceBundle
    {
        /// <summary>
        /// The path to the .NET Framework 4+ GAC where we will find .resources.dll files.
        /// </summary>
        private static readonly string GlobalAssemblyCacheMSILPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\assembly\GAC_MSIL");

        /// <summary>
        /// CLDR string value "∅∅∅" prevents fallback to the parent bundle.
        /// </summary>
        public const string NoInheritanceMarker = "\u2205\u2205\u2205";

        /// <summary>
        /// The class loader constant to be used with <see cref="GetBundleInstance(string, string, Assembly, OpenType)"/> API
        /// </summary>
        public static readonly Assembly IcuDataAssembly =
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            typeof(ICUData).GetTypeInfo().Assembly; //ClassLoaderUtil.getClassLoader(ICUData.class); // ICU4N specific: This was named ICU_DATA_CLASS_LOADER in Java
#else
            typeof(ICUData).Assembly; //ClassLoaderUtil.getClassLoader(ICUData.class); // ICU4N specific: This was named ICU_DATA_CLASS_LOADER in Java
#endif

        /// <summary>
        /// The name of the resource containing the installed locales
        /// </summary>
        protected const string InstalledLocales = "InstalledLocales";

        /// <summary>
        /// Fields for a whole bundle, rather than any specific resource in the bundle.
        /// Corresponds roughly to ICU4C/source/common/uresimp.h struct UResourceDataEntry.
        /// </summary>
        protected internal sealed class WholeBundle
        {
            internal WholeBundle(string baseName, string localeID, Assembly loader,
                ICUResourceBundleReader reader)
            {
                this.baseName = baseName;
                this.localeID = localeID;
                this.uculture = new UCultureInfo(localeID);
                this.loader = loader;
                this.reader = reader;
            }

            internal string baseName;
            internal string localeID;
            internal UCultureInfo uculture;
            internal Assembly loader;

            /// <summary>
            /// Access to the bits and bytes of the resource bundle.
            /// Hides low-level details.
            /// </summary>
            internal ICUResourceBundleReader reader;

            // TODO: Remove topLevelKeys when we upgrade to Java 6 where ResourceBundle caches the keySet().
            internal ISet<string> topLevelKeys;
        }

        internal WholeBundle wholeBundle;
        private readonly ICUResourceBundle container;

        // ICU4N: Factored out Loader and BundleCache and changed to GetOrCreate() method that
        // uses a delegate to do all of this inline.

        private static readonly CacheBase<string, ICUResourceBundle> BUNDLE_CACHE = new SoftCache<string, ICUResourceBundle>();

        /// <summary>
        /// Returns a functionally equivalent locale, considering keywords as well, for the specified keyword.
        /// </summary>
        /// <param name="baseName">Resource specifier.</param>
        /// <param name="assembly"></param>
        /// <param name="resName">Top level resource to consider (such as "collations").</param>
        /// <param name="keyword">A particular keyword to consider (such as "collation" ).</param>
        /// <param name="locID">The requested locale.</param>
        /// <param name="isAvailable">If non-null, 1-element array of fillin parameter that indicates whether the
        /// requested locale was available. The locale is defined as 'available' if it physically
        /// exists within the specified tree and included in 'InstalledLocales'.</param>
        /// <param name="omitDefault">If true, omit keyword and value if default.
        /// 'de_DE\@collation=standard' -> 'de_DE'</param>
        /// <returns>The locale.</returns>
        /// <internal>ICU 3.0</internal>
        public static UCultureInfo GetFunctionalEquivalent(string baseName, Assembly assembly,
            string resName, string keyword, UCultureInfo locID,
            bool[] isAvailable, bool omitDefault)
        {
            locID.Keywords.TryGetValue(keyword, out string kwVal);
            string baseLoc = locID.Name;
            string defStr = null;
            UCultureInfo parent = new UCultureInfo(baseLoc);
            UCultureInfo defLoc = null; // locale where default (found) resource is
            bool lookForDefault = false; // true if kwVal needs to be set
            UCultureInfo fullBase = null; // base locale of found (target) resource
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
                UCultureInfo[] availableUCultures = GetAvailEntry(baseName, assembly).GetUCultureList(UCultureTypes.AllCultures);
                for (int i = 0; i < availableUCultures.Length; i++)
                {
                    if (parent.Equals(availableUCultures[i]))
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
                    defLoc = r.UCulture;
                }
                catch (MissingManifestResourceException)
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
            parent = new UCultureInfo(baseLoc);
            r = (ICUResourceBundle)UResourceBundle.GetBundleInstance(baseName, parent);
            // determine in which locale (if any) the named resource is located
            do
            {
                try
                {
                    ICUResourceBundle irb = (ICUResourceBundle)r.Get(resName);
                    /* UResourceBundle urb = */
                    irb.Get(kwVal);
                    fullBase = irb.UCulture;
                    // If the get() completed, we have the full base locale
                    // If we fell back to an ancestor of the old 'default',
                    // we need to re calculate the "default" keyword.
                    if ((fullBase != null) && ((resDepth) > defDepth))
                    {
                        defStr = irb.GetString(DEFAULT_TAG);
                        defLoc = r.UCulture;
                        defDepth = resDepth;
                    }
                }
                catch (MissingManifestResourceException)
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
                parent = new UCultureInfo(baseLoc);
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
                        fullBase = r.UCulture;

                        // If the fetched item (urb) is in a different locale than our outer locale (r/fullBase)
                        // then we are in a 'fallback' situation. treat as a missing resource situation.
                        if (!fullBase.Name.Equals(urb.UCulture.Name))
                        {
                            fullBase = null; // fallback condition. Loop and try again.
                        }

                        // If we fell back to an ancestor of the old 'default',
                        // we need to re calculate the "default" keyword.
                        if ((fullBase != null) && ((resDepth) > defDepth))
                        {
                            defStr = irb.GetString(DEFAULT_TAG);
                            defLoc = r.UCulture;
                            defDepth = resDepth;
                        }
                    }
                    catch (MissingManifestResourceException)
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
                return new UCultureInfo(fullBase.Name + "@" + keyword + "=" + kwVal);
            }
        }

        /// <summary>
        /// Given a tree path and keyword, return a string enumeration of all possible values for that keyword.
        /// </summary>
        /// <param name="baseName">Resource specifier.</param>
        /// <param name="keyword">A particular keyword to consider, must match a top level resource name
        /// within the tree. (i.e. "collations").</param>
        /// <param name="assembly">The assembly to retrieve the resources from.</param>
        /// <internal>ICU 3.0</internal>
        public static string[] GetKeywordValues(string baseName, string keyword, Assembly assembly) // ICU4N specific - passing in assembly so submodules can override
        {
            ISet<string> keywords = new JCG.HashSet<string>();
            UCultureInfo[] locales = GetAvailEntry(baseName, assembly).GetUCultureList(UCultureTypes.AllCultures); // ICU4N specific - passing in assembly so submodules can override
            int i;

            for (i = 0; i < locales.Length; i++)
            {
                try
                {
                    UResourceBundle b = UResourceBundle.GetBundleInstance(baseName, locales[i], assembly); // ICU4N specific - passing in assembly so submodules can override
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
                catch (Exception)
                {
                    //System.err.println("Error in - " + new Integer(i).toString()
                    // + " - " + t.toString());
                    // ignore the err - just skip that resource
                }
            }
            return keywords.ToArray();
        }

        // ICU4N TODO: API - change code sample UResourceBundle > UResourceManager
        /// <summary>
        /// This method performs multilevel fallback for fetching items from the
        /// bundle e.g: If resource is in the form de__PHONEBOOK{ collations{
        /// default{ "phonebook"} } } If the value of "default" key needs to be
        /// accessed, then do: 
        /// <code>
        ///     UResourceBundle bundle = UResourceBundle.GetBundleInstance("de__PHONEBOOK");
        ///     ICUResourceBundle result = null;
        ///     if (bundle is ICUResourceBundle)
        ///     {
        ///         result = ((ICUResourceBundle) bundle).GetWithFallback("collations/default");
        ///     }
        /// </code>
        /// </summary>
        /// <param name="path">The path to the required resource key.</param>
        /// <returns>Resource represented by the key.</returns>
        /// <exception cref="MissingManifestResourceException">If a resource was not found.</exception>
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

            if (result.Type == UResourceType.String && result.GetString().Equals(NoInheritanceMarker))
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
#pragma warning disable 612, 618
            return (ICUResourceBundle)base.FindTopLevel(index);
#pragma warning restore 612, 618
        }

        new public ICUResourceBundle FindTopLevel(string aKey)
        {
#pragma warning disable 612, 618
            return (ICUResourceBundle)base.FindTopLevel(aKey);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Like <see cref="GetWithFallback(string)"/>, but returns null if the resource is not found instead of
        /// throwing an exception.
        /// </summary>
        /// <param name="path">The path to the resource.</param>
        /// <returns>The resource, or null.</returns>
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

            if (result.Equals(NoInheritanceMarker))
            {
                throw new MissingManifestResourceException(string.Format("Encountered NO_INHERITANCE_MARKER, path: {0}, key: {1}", path, Key));
            }
            return result;
        }

        public virtual void GetAllItemsWithFallbackNoFail(string path, ResourceSink sink) // ICU4N TODO: API Change to TryGetAllItemsWithFallback (swap impl with below)
        {
            try
            {
                GetAllItemsWithFallback(path, sink);
            }
            catch (MissingManifestResourceException)
            {
                // Quietly ignore the exception.
            }
        }

        public virtual void GetAllItemsWithFallback(string path, ResourceSink sink)
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
            ResourceKey key = new ResourceKey();
            ReaderValue readerValue = new ReaderValue();
            rb.GetAllItemsWithFallback(key, readerValue, sink);
        }

        private void GetAllItemsWithFallback(
            ResourceKey key, ReaderValue readerValue, ResourceSink sink)
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
            key.SetString(this.m_key != null ? this.m_key : "");
            sink.Put(key, readerValue, m_parent == null);
            if (m_parent != null)
            {
                // We might try to query the sink whether
                // any fallback from the parent bundle is still possible.
                ICUResourceBundle parentBundle = (ICUResourceBundle)m_parent;
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

        /// <summary>
        /// Return a set of the locale names supported by a collection of resource
        /// bundles.
        /// </summary>
        /// <param name="bundlePrefix">The prefix of the resource bundles to use.</param>
        /// <param name="assembly"></param>
        public static ISet<string> GetAvailableLocaleNameSet(string bundlePrefix, Assembly assembly)
        {
            return GetAvailEntry(bundlePrefix, assembly).GetLocaleNameSet();
        }

        /// <summary>
        /// Return a set of all the locale names supported by a collection of
        /// resource bundles.
        /// </summary>
        public static ISet<string> GetFullLocaleNameSet()
        {
            return GetFullLocaleNameSet(ICUData.IcuBaseName, IcuDataAssembly);
        }

        /// <summary>
        /// Return a set of all the locale names supported by a collection of
        /// resource bundles.
        /// </summary>
        /// <param name="bundlePrefix">The prefix of the resource bundles to use.</param>
        /// <param name="assembly"></param>
        public static ISet<string> GetFullLocaleNameSet(string bundlePrefix, Assembly assembly)
        {
            return GetAvailEntry(bundlePrefix, assembly).GetFullLocaleNameSet();
        }

        /// <summary>
        /// Return a set of the locale names supported by a collection of resource
        /// bundles.
        /// </summary>
        /// <returns></returns>
        public static ISet<string> GetAvailableLocaleNameSet()
        {
            return GetAvailableLocaleNameSet(ICUData.IcuBaseName, IcuDataAssembly);
        }

        /// <summary>
        /// Get the set of <see cref="UCultureInfo"/>s installed in the specified bundles.
        /// </summary>
        /// <param name="baseName">The base name.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to scan for cultures.</param>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="types"/> specifies an invalid combination of <see cref="UCultureTypes"/> values.</exception>
        public static UCultureInfo[] GetUCultures(string baseName, Assembly assembly, UCultureTypes types) // ICU4N: Renamed from GetAvailableULocales
        {
            // Validate flags
            if ((int)types <= 0 || ((int)types & (int)~(UCultureTypes.NeutralCultures | UCultureTypes.SpecificCultures)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(types),
                    $"Values are between {UCultureTypes.NeutralCultures} and {UCultureTypes.SpecificCultures}, inclusive.");
            }

            return GetAvailEntry(baseName, assembly).GetUCultureList(types);
        }

        /// <summary>
        /// Get the set of <see cref="UCultureInfo"/>s installed the base bundle.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="types"/> specifies an invalid combination of <see cref="UCultureTypes"/> values.</exception>
        public static UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableULocales
        {
            return GetUCultures(ICUData.IcuBaseName, IcuDataAssembly, types);
        }

        /// <summary>
        /// Get the set of <see cref="CultureInfo"/>s installed in the specified bundles.
        /// </summary>
        /// <param name="baseName">The base name.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to scan for cultures.</param>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="types"/> specifies an invalid combination of <see cref="UCultureTypes"/> values.</exception>
        public static CultureInfo[] GetCultures(string baseName, Assembly assembly, UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            // Validate flags
            if ((int)types <= 0 || ((int)types & (int)~(UCultureTypes.NeutralCultures | UCultureTypes.SpecificCultures)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(types),
                    $"Values are between {UCultureTypes.NeutralCultures} and {UCultureTypes.SpecificCultures}, inclusive.");
            }

            return GetAvailEntry(baseName, assembly).GetCultureList(types);
        }

        /// <summary>
        /// Get the set of <see cref="CultureInfo"/>s installed the base bundle.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="types"/> specifies an invalid combination of <see cref="UCultureTypes"/> values.</exception>
        public static CultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            // Validate flags
            if ((int)types <= 0 || ((int)types & (int)~(UCultureTypes.NeutralCultures | UCultureTypes.SpecificCultures)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(types),
                    $"Values are between {UCultureTypes.NeutralCultures} and {UCultureTypes.SpecificCultures}, inclusive.");
            }

            return GetAvailEntry(ICUData.IcuBaseName, IcuDataAssembly).GetCultureList(types);
        }

        /// <summary>
        /// Convert a list of <see cref="UCultureInfo"/>s to a list of <see cref="CultureInfo"/>s.  <see cref="UCultureInfo"/>s with a script code will not be converted
        /// since they cannot be represented as a <see cref="CultureInfo"/>.  This means that the two lists will <b>not</b> match
        /// one-to-one, and that the returned list might be shorter than the input list.
        /// </summary>
        /// <param name="ulocales">A list of <see cref="UCultureInfo"/>s to convert to a list of <see cref="CultureInfo"/>s.</param>
        /// <returns>The list of converted Locales.</returns>
        public static CultureInfo[] GetCultureList(UCultureInfo[] ulocales) // ICU4N: Since this list is cached in AvailEntry, we don't need to put a filter on it here
        {
            List<CultureInfo> list = new List<CultureInfo>(ulocales.Length);
            for (int i = 0; i < ulocales.Length; i++)
            {
                CultureInfo loc = ulocales[i].ToCultureInfo();
                if (!list.Contains(loc))
                    list.Add(loc);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Returns the locale of this resource bundle. This method can be used after
        /// a call to <see cref="GetBundle(ICUResourceBundleReader, string, string, Assembly)"/>
        /// to determine whether the resource bundle returned
        /// really corresponds to the requested locale or is a fallback.
        /// </summary>
        /// <returns>The locale of this resource bundle.</returns>
        public override CultureInfo Culture
            => UCulture.ToCultureInfo();

        // ========== privates ==========
        private const string ICU_RESOURCE_INDEX = "res_index";

        private const string DEFAULT_TAG = "default";

        // The name of text file generated by ICU4J build script including all locale names
        // (canonical, alias and root)
        private const string FULL_LOCALE_NAMES_LIST = "fullLocaleNames.lst"; // ICU4N TODO: Do we need this list ?

        // Flag for enabling/disabling debugging code
        private static readonly bool DEBUG = ICUDebug.Enabled("localedata");

        private static UCultureInfo[] CreateUCultureList(string baseName,
            Assembly root)
        {
            // the canned list is a subset of all the available .res files, the idea
            // is we don't export them
            // all. gotta be a better way to do this, since to add a locale you have
            // to update this list,
            // and it's embedded in our binary resources.
            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.InstantiateBundle(baseName, ICU_RESOURCE_INDEX, root, true);

            bundle = (ICUResourceBundle)bundle.Get(InstalledLocales);
            int length = bundle.Length;
            int i = 0;
            UCultureInfo[] locales = new UCultureInfo[length];
            using UResourceBundleEnumerator iter = bundle.GetEnumerator();
            iter.Reset();
            while (iter.MoveNext())
            {
                string locstr = iter.Current.Key;
                if (locstr.Equals("root"))
                {
                    locales[i++] = UCultureInfo.InvariantCulture;
                }
                else
                {
                    locales[i++] = new UCultureInfo(locstr);
                }
            }
            bundle = null;
            return locales;
        }

        /// <summary>
        /// Gets the satellite assembly if one exists. Returns the root if one does not, so we can lookup the data via files.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
        private static Assembly GetInvariantSatelliteAssemblyOrDefault(Assembly root)
        {
            if (root is null)
                throw new ArgumentNullException(nameof(root));

            var resourceLocationAttribute = root.GetCustomAttribute<ResourceLocationAttribute>();
            if (resourceLocationAttribute != null && resourceLocationAttribute.Location == Resources.ResourceLocation.Satellite)
            {
                try
                {
                    return root.GetSatelliteAssembly(CultureInfo.InvariantCulture);
                }
                catch (FileNotFoundException) // ICU4N: This will occur if the root satellite assembly is missing from the disk/GAC
                {
                    // ignore
                }
            }
            return root;
        }

        // Same as createULocaleList() but catches the MissingResourceException
        // and returns the data in a different form.
        private static void AddLocaleIDsFromIndexBundle(string baseName,
            Assembly root, ISet<string> locales)
        {
            ICUResourceBundle bundle;
            Assembly satelliteAssembly = GetInvariantSatelliteAssemblyOrDefault(root);
                
            try
            {
                bundle = (ICUResourceBundle)UResourceBundle.InstantiateBundle(baseName, ICU_RESOURCE_INDEX, satelliteAssembly, true);
                bundle = (ICUResourceBundle)bundle.Get(InstalledLocales);
            }
            catch (MissingManifestResourceException)
            {
                if (DEBUG)
                {
                    Console.Out.WriteLine("couldn't find " + baseName + '/' + ICU_RESOURCE_INDEX + ".res");
                    //Thread.dumpStack(); // ICU4N TODO
                }
                return;
            }
            using UResourceBundleEnumerator iter = bundle.GetEnumerator();
            iter.Reset();
            while (iter.MoveNext())
            {
                string locstr = iter.Current.Key;
                locales.Add(locstr);
            }
        }

        private static void AddBundleBaseNamesFromAssembly(
            string bn, Assembly root, ISet<string> names) // ICU4N: Renamed from AddBundleBaseNamesFromClassLoader()
        {
            // ICU4N: Convert to .NET style base name
            string suffix = bn.Replace('/', '.').Replace('.' + ICUData.PackageName, "");
            string baseName = root.GetManifestResourceBaseName(suffix);
            foreach (var s in root.GetManifestResourceNames()
                .Where(name => name.StartsWith(baseName, StringComparison.Ordinal))
                .Select(n => n.Length != 0 ? n.Replace(baseName, "") : n))
            {
                if (s.EndsWith(".res", StringComparison.Ordinal))
                {
                    string locstr = s.Substring(0, s.Length - 4); // ICU4N: Checked 2nd parameter
                    names.Add(locstr);
                }
            }
        }

        // internal for testing
        internal static void AddLocaleIDsFromListFile(string bn, Assembly root, ISet<string> locales)
        {
            Assembly satelliteAssembly = GetInvariantSatelliteAssemblyOrDefault(root);
            try
            {
                using Stream s = satelliteAssembly.FindAndGetManifestResourceStream(ResourceUtil.ConvertResourceName(bn + FULL_LOCALE_NAMES_LIST));
                if (s != null)
                {
                    using TextReader br = new StreamReader(s, Encoding.ASCII);
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
            catch (IOException/* ignored */)
            {
                // swallow it
            }
        }

        // internal for testing
        internal static void AddLocaleIDsFromSatelliteAndGACFolderNames(string baseName, Assembly assembly, ISet<string> set)
        {
            if (baseName is null)
                throw new ArgumentNullException(nameof(baseName));
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            AssemblyName assemblyNameObj = assembly.GetName();
            string assemblyName = assemblyNameObj.Name;
            string satelliteAssemblyName = $"{assemblyName}.resources";
            string satelliteAssemblyDLLName = $"{satelliteAssemblyName}.dll";

            var cultureNames = new HashSet<string>();

            foreach (var file in new DirectoryInfo(PlatformDetection.BaseDirectory).GetFiles(satelliteAssemblyDLLName, SearchOption.AllDirectories))
            {
                string cultureName = file.Directory.Name;
                if (LooksLikeACultureName(cultureName))
                    cultureNames.Add(cultureName);
            }

            // Check the Global Assembly Cache (for .NET 4+ only)
            if (PlatformDetection.IsWindows)
            {
                // Example path: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\ICU4N.resources\v4.0_60.0.0.0_de--PHONEBOOK_efb17c8e4f0e291b

                string globalAssemblyCacheResourcePath = Path.Combine(GlobalAssemblyCacheMSILPath, satelliteAssemblyName);

                if (Directory.Exists(globalAssemblyCacheResourcePath))
                {
                    foreach (var file in new DirectoryInfo(globalAssemblyCacheResourcePath).GetFiles(satelliteAssemblyDLLName, SearchOption.AllDirectories))
                    {
                        string assemblyDetails = file.Directory.Name;
                        string[] parts = assemblyDetails.Split('_');
                        string cultureName = parts[2]; // 3rd segment of the folder is the culture name. Neutral resources have an empty string.

                        if (LooksLikeACultureName(cultureName))
                            cultureNames.Add(cultureName);
                    }
                }
            }

            Assembly satelliteAssembly;

            foreach (var cultureName in cultureNames)
            {
                // ICU4N TODO: For .NET Core 3+, there is a better way with AssemblyLoadContext that allows us to
                // unload the assembly again https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
                // We need to target .NET Core 3 (or higher), to pull it off.

                // ICU4N TODO: For .NET Framework, we can unload the assemblies by loading them into a temporary domain.
                // This works in most cases, but certain domains trip up due to scropt codes. Still, if we load
                // and unload the majority of them and fall back on ICUData.GetSatelliteAssemblyOrDefault, it should be more
                // efficient than using ICUData.GetSatelliteAssemblyOrDefault alone.
                //
                // We require a cultureName -> assembly path Dictionary to use this approach, though. And the same will
                // likely apply to the .NET Core 3+ unloading approach.
                //
                //            AppDomain dom = AppDomain.CreateDomain("getCultures");
                //            AssemblyName satelliteAssemblyNamer = new AssemblyName();
                //            foreach (var kvp in cultureMap)
                //            {
                //                satelliteAssemblyNamer.CodeBase = kvp.Value;
                //                var satellite = dom.Load(satelliteAssemblyNamer);

                //                string localeID = kvp.Key.Replace('-', '_');
                //                if (EmbeddedResourceFileExists(satellite, baseName, localeID))
                //                    set.Add(localeID);
                //            }
                //            AppDomain.Unload(dom);

                if ((satelliteAssembly = ICUData.GetSatelliteAssemblyOrDefault(assembly, cultureName)) != null)
                {
                    string localeID = cultureName.Replace('-', '_');
                    if (EmbeddedResourceFileExists(satelliteAssembly, baseName, localeID))
                        set.Add(localeID);
                }
            }
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool EmbeddedResourceFileExists(Assembly assembly, string baseName, string localeID)
        {
            var icuPath = Path.Combine(baseName, string.Concat(localeID, ".res"));

            // We just run a quick check to see if the file is in the resource
            return assembly.GetManifestResourceInfo(ResourceUtil.ConvertResourceName(icuPath)) != null;
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool LooksLikeACultureName(string cultureName)
                => (cultureName.Length > 1 && cultureName.Length <= 3) || cultureName.IndexOf('-') >= 2;

        private static ISet<string> CreateFullLocaleNameSet(string baseName, Assembly assembly)
        {
            string bn = baseName.EndsWith("/", StringComparison.Ordinal) ? baseName : baseName + "/";
            ISet<string> set = new JCG.HashSet<string>();
            string skipScan = ICUConfig.Get("skipRuntimeLocaleResourceScan", "false");
            if (!skipScan.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var resourceLocationAttribute = assembly.GetCustomAttribute<ResourceLocationAttribute>();
                if (resourceLocationAttribute != null && resourceLocationAttribute.Location == Resources.ResourceLocation.Satellite)
                {
                    AddLocaleIDsFromSatelliteAndGACFolderNames(baseName, assembly, set);
                }

                if (set.Count == 0)
                {
                    // scan available locale resources under the base url first
                    AddBundleBaseNamesFromAssembly(bn, assembly, set);

                    if (baseName.StartsWith(ICUData.IcuBaseName, StringComparison.Ordinal))
                    {
                        string folder;
                        if (baseName.Length == ICUData.IcuBaseName.Length)
                        {
                            folder = "";
                        }
                        else if (baseName[ICUData.IcuBaseName.Length] == '/')
                        {
                            folder = baseName.Substring(ICUData.IcuBaseName.Length + 1);
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
            }
            // look for prebuilt full locale names list next
            if (set.Count == 0)
            {
                if (DEBUG) Console.Out.WriteLine("unable to enumerate data files in " + baseName);
                AddLocaleIDsFromListFile(bn, assembly, set);
            }
            if (set.Count == 0)
            {
                // Use locale name set as the last resort fallback
                AddLocaleIDsFromIndexBundle(baseName, assembly, set);
            }
            // We need to have the root locale in the set, but not as "root".
            set.Remove("root");
            set.Add(UCultureInfo.InvariantCulture.ToString());  // ""

            // ICU4N TODO: Add extension point (something like ICUConfig) that can be used at
            // app startup to inject additional locale names to cover cases like dynamically
            // generated assemblies in the AppDomain.AssemblyLoad Event.

            return set.AsReadOnly();
        }

        private static ISet<string> CreateLocaleNameSet(string baseName, Assembly assembly)
        {
            var set = new JCG.HashSet<string>();
            AddLocaleIDsFromIndexBundle(baseName, assembly, set);
            return set.AsReadOnly();
        }

        /// <summary>
        /// Holds the prefix, and lazily creates the <see cref="T:CultureInfo[]"/> list or the locale name
        /// <see cref="ISet{T}"/> as needed.
        /// </summary>
        private sealed class AvailEntry
        {
            private readonly string prefix;
            private readonly Assembly assembly; // ICU4N specific - renamed loader to assembly
            private UCultureInfo[] uCultures; // ICU4N specific - made non-volatile
            private CultureInfo[] locales; // ICU4N specific - made non-volatile
            private ISet<string> nameSet; // ICU4N specific - made non-volatile
            private ISet<string> fullNameSet; // ICU4N specific - made non-volatile

            internal AvailEntry(string prefix, Assembly assembly)
            {
                this.prefix = prefix;
                this.assembly = assembly;
            }

            internal UCultureInfo[] GetUCultureList(UCultureTypes types)
            {
                if (uCultures == null)
                {
                    LazyInitializer.EnsureInitialized(ref uCultures,
                        () => CreateUCultureList(prefix, assembly));
                }

                return types == UCultureTypes.AllCultures ? uCultures : uCultures.Where(c => c.IsMatch(types)).ToArray();
            }
            internal CultureInfo[] GetCultureList(UCultureTypes types)
            {
                if (locales == null)
                {
                    LazyInitializer.EnsureInitialized(ref locales, () =>
                    {
                        // ICU4N: Skip the LINQ query here
                        GetUCultureList(UCultureTypes.AllCultures);
                        return ICUResourceBundle.GetCultureList(uCultures);
                    });
                }

                return types == UCultureTypes.AllCultures ? locales : locales.Where(c => c.IsMatch(types)).ToArray();
            }
            internal ISet<string> GetLocaleNameSet()
            {
                if (nameSet == null)
                {
                    return LazyInitializer.EnsureInitialized(ref nameSet, () => CreateLocaleNameSet(prefix, assembly));
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
                    return LazyInitializer.EnsureInitialized(ref fullNameSet,
                        () => CreateFullLocaleNameSet(prefix, assembly));
                }

                return fullNameSet;
            }
        }

        // ICU4N: Factored out AvailableEntryCache and changed to GetOrCreate() method that
        // uses a delegate to do all of this inline.

        /// <summary>
        /// Cache used for AvailableEntry
        /// </summary>
        private static readonly CacheBase<string, AvailEntry> GET_AVAILABLE_CACHE = new SoftCache<string, AvailEntry>();

        /// <summary>
        /// Stores the locale information in a cache accessed by key (bundle prefix).
        /// The cached objects are <see cref="AvailEntry"/>s. The cache is implemented by <see cref="SoftCache{TKey, TValue}"/>
        /// so it can be GC'd.
        /// </summary>
        private static AvailEntry GetAvailEntry(string key, Assembly assembly)
        {
            return GET_AVAILABLE_CACHE.GetOrCreate(key, (k) => new AvailEntry(k, assembly)); // ICU4N: Use a delegate to create the value inline so we don't need extra classes
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

        /// <summary>
        /// Like FindResourceWithFallback(...).GetString() but with minimal creation of intermediate
        /// <see cref="ICUResourceBundle"/> objects.
        /// </summary>
        private static string FindStringWithFallback(string path,
            UResourceBundle actualBundle, UResourceBundle requested)
        {
            if (string.IsNullOrEmpty(path)) // ICU4N: BUG: path may be null here, and it throws.
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
            int res = ResBogus;

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
                    if (res == ResBogus)
                    {
                        UResourceType type = @base.Type;
                        if (type == UResourceType.Table || type == UResourceType.Array)
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
                        UResourceType type = ICUResourceBundleReader.RES_GET_TYPE(res);
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
                            res = ResBogus;
                            break;
                        }
                    }
                    string subKey = keys[depth++];
                    res = readerContainer.GetResource(reader, subKey);
                    if (res == ResBogus)
                    {
                        --depth;
                        break;
                    }
                    ICUResourceBundle sub;
                    if (ICUResourceBundleReader.RES_GET_TYPE(res) == UResourceType.Alias)
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
                        res = ResBogus;
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

        /// <summary>
        /// Fills some of the keys array with the keys on the path to this resource object.
        /// Writes the top-level key into index 0 and increments from there.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="depth">Must be <see cref="GetResDepth()"/>.</param>
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

        /// <summary>
        /// Fills some of the keys array (from start) with the num keys from the path string.
        /// </summary>
        /// <param name="path">Path string.</param>
        /// <param name="num">Must be <see cref="CountPathKeys(string)"/>.</param>
        /// <param name="keys"></param>
        /// <param name="start">Index where the first path key is stored.</param>
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
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (other is ICUResourceBundle o)
            {
                if (BaseName.Equals(o.BaseName)
                        && LocaleID.Equals(o.LocaleID))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            // ICU4N specific - implemented hash code
            int hash = 17;
            unchecked // Overflow is fine, just wrap
            {
                hash = hash * 23 + Utility.CheckHashCode(BaseName);
                hash = hash * 23 + Utility.CheckHashCode(LocaleID);
            }
            return hash;
        }

        // ICU4N specific - de-nested OpenType enum

        // This method is for super class's instantiateBundle method
        new public static ICUResourceBundle GetBundleInstance(string baseName, string localeID,
            Assembly root, bool disableFallback)
        {
            return GetBundleInstance(baseName, localeID, root,
                    disableFallback ? OpenType.Direct : OpenType.LocaleDefaultRoot);
        }

        public static ICUResourceBundle GetBundleInstance(
            string baseName, UCultureInfo locale, OpenType openType)
        {
            return GetBundleInstance(baseName, locale,
                    ICUResourceBundle.IcuDataAssembly, openType);
        }

        // ICU4N specific overload so we can pass the Assembly from submodules (since 
        // the main assembly won't see submodules by default).
        public static ICUResourceBundle GetBundleInstance(
            string baseName, UCultureInfo locale, Assembly root, OpenType openType)
        {
            if (locale == null)
            {
                locale = UCultureInfo.CurrentCulture;
            }
            return GetBundleInstance(baseName, locale.Name,
                    root, openType);
        }

        public static ICUResourceBundle GetBundleInstance(string baseName, string localeID,
            Assembly root, OpenType openType)
        {
            return GetBundleInstance(baseName, localeID, UCultureInfo.CurrentCulture.Name, root, openType);
        }

        // ICU4N specific - added this as a means to call this within UCultureInfo.CurrentCulture without
        // infinite recursion.
        internal static ICUResourceBundle GetBundleInstance(string baseName, string localeID,
            string defaultID, Assembly root, OpenType openType)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            localeID = UCultureInfo.GetName(localeID);
            ICUResourceBundle b;
            if (openType == OpenType.LocaleDefaultRoot)
            {
                b = InstantiateBundle(baseName, localeID, defaultID,
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

        // ICU4N: Factored out BundleLoader and changed to GetOrCreate() method that
        // uses a delegate to do all of this inline.

        private static ICUResourceBundle InstantiateBundle(
            string baseName, string localeID, string defaultID,
            Assembly root, OpenType openType)
        {
            Debug.Assert(localeID.IndexOf('@') < 0);
            Debug.Assert(defaultID == null || defaultID.IndexOf('@') < 0);
            string fullName = ICUResourceBundleReader.GetFullName(baseName, localeID);
            char openTypeChar = (char)('0' + (int)openType);
            string cacheKey = openType != OpenType.LocaleDefaultRoot ?
                    fullName + '#' + openTypeChar :
                        fullName + '#' + openTypeChar + '#' + defaultID;
            return BUNDLE_CACHE.GetOrCreate(cacheKey, (key) =>
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
                if (openType == OpenType.Direct || (b != null && b.NoFallback))
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
                        if (openType == OpenType.LocaleDefaultRoot &&
                                !LocaleIDStartsWithLangSubtag(defaultID, localeName))
                        {
                            // Go to the default locale before root.
                            b = InstantiateBundle(baseName, defaultID, defaultID, root, openType);
                        }
                        else if (openType != OpenType.LocaleOnly && !string.IsNullOrEmpty(rootLocale))
                        {
                            // Ultimately go to root.
                            b = ICUResourceBundle.CreateBundle(baseName, rootLocale, root);
                        }
                    }
                }
                else
                {
                    UResourceBundle parent = null;
                    localeName = b.LocaleID;
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
            });
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
                    string fullName = ICUResourceBundleReader.GetFullName(BaseName, LocaleID);
                    throw new MissingManifestResourceException(
                            "Can't find resource for bundle " + fullName + ", key "
                                    + aKey + ", type " + this.GetType().FullName);
                }
            }
            return obj;
        }

        /// <summary>
        /// Data member where the subclasses store the key.
        /// </summary>
        protected string m_key;

        /// <summary>
        /// A resource word value that means "no resource".
        /// Note: 0xffffffff == -1
        /// <para/>
        /// This has the same value as <see cref="UResourceType.None"/>, but they are semantically
        /// different and should be used appropriately according to context:
        /// NONE means "no type".
        /// (The type of <see cref="ResBogus"/> is RES_RESERVED=15 which was defined in ICU4C ures.h.)
        /// </summary>
        public const int ResBogus = unchecked((int)0xffffffff);
        
        // ICU4N specific - moved constants to UResourceType enum

        /* Resource type 15 is not defined but effectively used by RES_BOGUS=0xffffffff. */

        /// <summary>
        /// Create a bundle using a reader.
        /// </summary>
        /// <param name="baseName">The name for the bundle.</param>
        /// <param name="localeID">The locale identification.</param>
        /// <param name="root">The <see cref="Assembly"/> object root.</param>
        /// <returns>The new bundle.</returns>
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

        protected override string LocaleID => wholeBundle.localeID;

        protected internal override string BaseName => wholeBundle.baseName;

        public override UCultureInfo UCulture => wholeBundle.uculture;

        /// <summary>
        /// Returns true if this is the root bundle, or an item in the root bundle.
        /// </summary>
        public virtual bool IsRoot
            => string.IsNullOrEmpty(wholeBundle.localeID) || wholeBundle.localeID.Equals("root");

        public override void SetParent(ResourceBundle parent)
        {
            this.m_parent = parent;
        }

        new public ICUResourceBundle Parent // ICU4N: Since the only purpose here is to cast, using the new keyword is fine
        {
            get => (ICUResourceBundle)m_parent;
            set => this.m_parent = value;
        }


        public override string Key
            => m_key;

        /// <summary>
        /// Gets the noFallback flag specified in the loaded bundle.
        /// </summary>
        private bool NoFallback
            => wholeBundle.reader.NoFallback;

        private static ICUResourceBundle GetBundle(ICUResourceBundleReader reader,
                                                   string baseName, string localeID,
                                                   Assembly assembly)
        {
            ICUResourceBundleImpl.ResourceTable rootTable;
            int rootRes = reader.RootResource;
            if (ICUResourceBundleReader.URES_IS_TABLE(ICUResourceBundleReader.RES_GET_TYPE(rootRes)))
            {
                WholeBundle wb = new WholeBundle(baseName, localeID, assembly, reader);
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
        /// <summary>
        /// Constructor for the root table of a bundle.
        /// </summary>
        protected ICUResourceBundle(WholeBundle wholeBundle)
        {
            this.wholeBundle = wholeBundle;
        }
        // constructor for inner classes
        protected ICUResourceBundle(ICUResourceBundle container, string key)
        {
            this.m_key = key;
            wholeBundle = container.wholeBundle;
            this.container = container;
            m_parent = container.m_parent;
        }

        private const char RES_PATH_SEP_CHAR = '/';
        private const string RES_PATH_SEP_STR = "/";
        private const string ICUDATA = "ICUDATA";
        private const char HYPHEN = '-';
        private const string LOCALE = "LOCALE";

        /// <summary>
        /// Returns the resource object referred to from the alias _resource int's path string.
        /// Throws <see cref="MissingManifestResourceException"/> if not found.
        /// <para/>
        /// If the alias path does not contain a key path:
        /// If keys != null then keys[:depth] is used.
        /// Otherwise the base key path plus the key parameter is used.
        /// </summary>
        /// <param name="base">A direct or indirect container of the alias.</param>
        /// <param name="keys">The key path to the alias, or null. (const)</param>
        /// <param name="depth">The length of the key path, if keys != null.</param>
        /// <param name="key">The alias' own key within this current container, if keys == null.</param>
        /// <param name="_resource">The alias resource int.</param>
        /// <param name="aliasesVisited">Set of alias path strings already visited, for detecting loops.
        /// We cannot change the type (e.g., to <see cref="T:ISet{string}"/>) because it is used
        /// in protected/@stable <see cref="UResourceBundle"/> methods.</param>
        /// <param name="requested">The original resource object from which the lookup started,
        /// which is the starting point for "/LOCALE/..." aliases.</param>
        /// <returns>The aliased resource object.</returns>
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
            if (aliasesVisited.TryGetValue(rpath, out string value) && value != null)
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
                if (bundleName.Equals(ICUDATA, StringComparison.Ordinal))
                {
                    bundleName = ICUData.IcuBaseName;
                    loaderToUse = IcuDataAssembly;
                }
                else if (bundleName.IndexOf(ICUDATA, StringComparison.Ordinal) > -1)
                {
                    int idx = bundleName.IndexOf(HYPHEN);
                    if (idx > -1)
                    {
                        bundleName = ICUData.IcuBaseName + RES_PATH_SEP_STR + bundleName.Substring(idx + 1, bundleName.Length - (idx + 1)); // ICU4N: Corrected 2nd parameter
                        loaderToUse = IcuDataAssembly;
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

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal ISet<string> TopLevelKeySet // ICU4N specific - marked internal, since the functionality is obsolete
        {
            get => wholeBundle.topLevelKeys;
            set => wholeBundle.topLevelKeys = value;
        }

        // This is the worker function for the public getKeys().
        // TODO: Now that UResourceBundle uses HandleKeySet(), this function is obsolete.
        // It is also not inherited from ResourceBundle, and it is not implemented
        // by ResourceBundleWrapper despite its documentation requiring all subclasses to
        // implement it.
        // Consider deprecating UResourceBundle.HandleGetKeys(), and consider making it always return null.
        protected override IEnumerable<string> HandleGetKeys()
        {
            return HandleKeySet();
        }

#pragma warning disable 672
        internal override bool IsTopLevelResource
#pragma warning restore 672
        {
            get { return container == null; }
        }
    }

    // C++ uresbund.cpp: enum UResOpenType
    public enum OpenType
    {
        /// <summary>
        /// Open a resource bundle for the locale;
        /// if there is not even a base language bundle, then fall back to the default locale;
        /// if there is no bundle for that either, then load the root bundle.
        /// <para/>
        /// This is the default bundle loading behavior.
        /// </summary>
        LocaleDefaultRoot,
        // TODO: ICU ticket #11271 "consistent default locale across locale trees"
        // Add an option to look at the main locale tree for whether to
        // fall back to root directly (if the locale has main data) or
        // fall back to the default locale first (if the locale does not even have main data).
        /// <summary>
        /// Open a resource bundle for the locale;
        /// if there is not even a base language bundle, then load the root bundle;
        /// never fall back to the default locale.
        /// <para/>
        /// This is used for algorithms that have good pan-Unicode default behavior,
        /// such as case mappings, collation, and segmentation (BreakIterator).
        /// </summary>
        LocaleRoot,
        /// <summary>
        /// Open a resource bundle for the locale;
        /// if there is not even a base language bundle, then fail;
        /// never fall back to the default locale nor to the root locale.
        /// <para/>
        /// This is used when fallback to another language is not desired
        /// and the root locale is not generally useful.
        /// For example, <see cref="LocaleData.NoSubstitute"/>
        /// or currency display names for <see cref="CultureDisplayNames"/>.
        /// </summary>
        LocaleOnly,
        /// <summary>
        /// Open a resource bundle for the exact bundle name as requested;
        /// no fallbacks, do not load parent bundles.
        /// <para/>
        /// This is used for supplemental (non-locale) data.
        /// </summary>
        Direct
    };
}
