using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace ICU4N.Impl
{
    /// <summary>
    /// just a wrapper for Java ListResourceBundles and
    /// </summary>
    /// <author>ram</author>
    public sealed class ResourceBundleWrapper : UResourceBundle
    {
        private ResourceBundle bundle = null;
        private string localeID = null;
        private string baseName = null;
        private IList<string> keys = null;

        /// <summary>Loader for bundle instances, for caching.</summary>
        private abstract class Loader
        {
            internal abstract ResourceBundleWrapper Load();
        }

        private class BundleCache : SoftCache<string, ResourceBundleWrapper, Loader>
        {
            protected override ResourceBundleWrapper CreateInstance(string unusedKey, Loader loader)
            {
                return loader.Load();
            }
        }

        private static CacheBase<string, ResourceBundleWrapper, Loader> BUNDLE_CACHE = new BundleCache();


        private ResourceBundleWrapper(ResourceBundle bundle)
        {
            this.bundle = bundle;
        }

        protected override object HandleGetObject(string aKey)
        {
            ResourceBundleWrapper current = this;
            object obj = null;
            while (current != null)
            {
                try
                {
                    obj = current.bundle.GetObject(aKey);
                    break;
                }
                catch (MissingManifestResourceException ex)
                {
                    current = (ResourceBundleWrapper)current.Parent;
                }
            }
            if (obj == null)
            {
                throw new MissingManifestResourceException("Can't find resource for bundle "
                                                   + baseName
                                                   + ", key " + aKey
                                                   + ", type " + this.GetType().FullName);
            }
            return obj;
        }

        public override IEnumerable<string> GetKeys()
        {
            return keys;
        }

        private void InitKeysVector()
        {
            ResourceBundleWrapper current = this;
            keys = new List<string>();
            while (current != null)
            {
                using (var e = current.bundle.GetKeys().GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        string elem = e.Current;
                        if (!keys.Contains(elem))
                        {
                            keys.Add(elem);
                        }
                    }
                }
                current = (ResourceBundleWrapper)current.Parent;
            }
        }

        protected override string GetLocaleID()
        {
            return localeID;
        }

        protected internal override string GetBaseName()
        {
            return bundle.GetType().FullName.Replace('.', '/');
        }

        public override ULocale GetULocale()
        {
            return new ULocale(localeID);
        }

        new public UResourceBundle Parent
        {
            get { return (UResourceBundle)parent; }
        }

        // Flag for enabling/disabling debugging code
        private static readonly bool DEBUG = ICUDebug.Enabled("resourceBundleWrapper");

        // This method is for super class's instantiateBundle method
        new public static ResourceBundleWrapper GetBundleInstance(string baseName, string localeID,
            Assembly root, bool disableFallback)
        {
            if (root == null)
            {
                // ICU4N TODO: Check this
                root = typeof(ICUData).GetTypeInfo().Assembly; //ClassLoaderUtil.getClassLoader();
            }
            ResourceBundleWrapper b;
            if (disableFallback)
            {
                b = InstantiateBundle(baseName, localeID, null, root, disableFallback);
            }
            else
            {
                b = InstantiateBundle(baseName, localeID, ULocale.GetDefault().GetBaseName(),
                        root, disableFallback);
            }
            if (b == null)
            {
                string separator = "_";
                if (baseName.IndexOf('/') >= 0)
                {
                    separator = "/";
                }
                throw new MissingManifestResourceException("Could not find the bundle " + baseName + separator + localeID);
            }
            return b;
        }

        private static bool LocaleIDStartsWithLangSubtag(string localeID, string lang)
        {
            return localeID.StartsWith(lang, StringComparison.Ordinal) &&
                    (localeID.Length == lang.Length || localeID[lang.Length] == '_');
        }

        private class BundleCacheLoader : Loader
        {
            private readonly Func<ResourceBundleWrapper> load;

            public BundleCacheLoader(Func<ResourceBundleWrapper> load)
            {
                this.load = load;
            }

            internal override ResourceBundleWrapper Load()
            {
                return load();
            }
        }

        private static ResourceBundleWrapper InstantiateBundle(
                 string baseName, string localeID, string defaultID,
                 Assembly root, bool disableFallback)
        {
            string name = string.IsNullOrEmpty(localeID) ? baseName : baseName + '_' + localeID;
            string cacheKey = disableFallback ? name : name + '#' + defaultID;
            return BUNDLE_CACHE.GetInstance(cacheKey, new BundleCacheLoader(load: () =>
            {
                ResourceBundleWrapper parent = null;
                int i = localeID.LastIndexOf('_');

                bool loadFromProperties = false;
                bool parentIsRoot = false;
                if (i != -1)
                {
                    string locName = localeID.Substring(0, i - 0); // ICU4N: Checked 2nd parameter
                    parent = InstantiateBundle(baseName, locName, defaultID, root, disableFallback);
                }
                else if (!string.IsNullOrEmpty(localeID))
                {
                    parent = InstantiateBundle(baseName, "", defaultID, root, disableFallback);
                    parentIsRoot = true;
                }
                ResourceBundleWrapper b = null;
                try
                {
                    Type cls = root.GetType(name);
                    ResourceBundle bx = (ResourceBundle)Activator.CreateInstance(cls);

                    b = new ResourceBundleWrapper(bx);
                    if (parent != null)
                    {
                        b.SetParent(parent);
                    }
                    b.baseName = baseName;
                    b.localeID = localeID;
                }
                catch (TargetInvocationException e)
                {
                    loadFromProperties = true;
                }
                catch (MissingMethodException e)
                {
                    loadFromProperties = true;
                }
                catch (Exception e)
                {
                    if (DEBUG)
                        Console.Out.WriteLine("failure");
                    if (DEBUG)
                        Console.Out.WriteLine(e);
                }

                if (loadFromProperties)
                {
                    // ICU4N TODO: finish implementation
                    throw new NotImplementedException();
                    //                try {
                    //                    string resName = name.Replace('.', '/') + ".properties";
                    //                    InputStream stream = java.security.AccessController.doPrivileged(
                    //                        new java.security.PrivilegedAction<InputStream>() {
                    //                            @Override
                    //                            public InputStream run()
                    //{
                    //    return root.getResourceAsStream(resName);
                    //}
                    //                        }
                    //                    );
                    //                    if (stream != null) {
                    //                        // make sure it is buffered
                    //                        stream = new java.io.BufferedInputStream(stream);
                    //                        try {
                    //                            b = new ResourceBundleWrapper(new PropertyResourceBundle(stream));
                    //                            if (parent != null) {
                    //                                b.setParent(parent);
                    //                            }
                    //                            b.baseName=baseName;
                    //                            b.localeID=localeID;
                    //                        } catch (Exception ex) {
                    //                            // throw away exception
                    //                        } finally {
                    //                            try {
                    //                                stream.close();
                    //                            } catch (Exception ex) {
                    //                                // throw away exception
                    //                            }
                    //                        }
                    //                    }

                    //                    // if a bogus locale is passed then the parent should be
                    //                    // the default locale not the root locale!
                    //                    if (b == null && !disableFallback &&
                    //                            !localeID.isEmpty() && localeID.indexOf('_') < 0 &&
                    //                            !localeIDStartsWithLangSubtag(defaultID, localeID)) {
                    //                        // localeID is only a language subtag, different from the default language.
                    //                        b = instantiateBundle(baseName, defaultID, defaultID, root, disableFallback);
                    //                    }
                    //                    // if still could not find the bundle then return the parent
                    //                    if(b==null && (!parentIsRoot || !disableFallback)){
                    //                        b=parent;
                    //                    }
                    //                } catch (Exception e) {
                    //                    if (DEBUG)
                    //                        Console.Out.WriteLine("failure");
                    //                    if (DEBUG)
                    //                        Console.Out.WriteLine(e);
                    //                }
                }
                if (b != null)
                {
                    b.InitKeysVector();
                }
                else
                {
                    if (DEBUG) Console.Out.WriteLine("Returning null for " + baseName + "_" + localeID);
                }
                return b;
            }));
        }
    }
}
