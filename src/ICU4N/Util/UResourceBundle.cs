using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Collections;

namespace ICU4N.Util
{
    /// <summary>
    /// <icuenhanced/>A class representing a collection of resource information pertaining to a given
    /// locale. A resource bundle provides a way of accessing locale- specific information in a
    /// data file. You create a resource bundle that manages the resources for a given locale
    /// and then ask it for individual resources.
    ///
    /// <para/>In ResourceBundle, an object is created and the sub-items are fetched using the
    /// getString and getObject methods.  In UResourceBundle, each individual element of a
    /// resource is a resource by itself.
    ///
    /// <para/>Resource bundles in ICU are currently defined using text files that conform to the
    /// following <a
    /// href="http://source.icu-project.org/repos/icu/icuhtml/trunk/design/bnf_rb.txt">BNF
    /// definition</a>.  More on resource bundle concepts and syntax can be found in the <a
    /// href="http://www.icu-project.org/userguide/ResourceManagement.html">Users Guide</a>.
    ///
    /// <para/>The packaging of ICU ///.res files can be of two types
    /// ICU4C:
    /// <pre>
    ///       root.res
    ///         |
    ///      --------
    ///     |        |
    ///   fr.res  en.res
    ///     |
    ///   --------
    ///  |        |
    /// fr_CA.res fr_FR.res
    /// </pre>
    /// JAVA/JDK:
    /// <pre>
    ///    LocaleElements.res
    ///         |
    ///      -------------------
    ///     |                   |
    /// LocaleElements_fr.res  LocaleElements_en.res
    ///     |
    ///   ---------------------------
    ///  |                            |
    /// LocaleElements_fr_CA.res   LocaleElements_fr_FR.res
    /// </pre>
    ///
    /// Depending on the organization of your resources, the syntax to GetBundleInstance will
    /// change.  To open ICU style organization use:
    ///
    /// <pre>
    ///      UResourceBundle bundle =
    ///          UResourceBundle.GetBundleInstance("com/mycompany/resources",
    ///                                            "en_US", myAssembly);
    /// </pre>
    /// To open Java/JDK style organization use:
    /// <pre>
    ///      UResourceBundle bundle =
    ///          UResourceBundle.GetBundleInstance("com.mycompany.resources.LocaleElements",
    ///                                            "en-US", myAssembly);
    /// </pre>
    ///
    /// <para/>Note: Please use pass an <see cref="Assembly"/> for loading non-ICU resources. .NET does not
    /// allow loading of resources across assembly files. You must provide your <see cref="Assembly"/> reference
    /// to load the resources.
    /// </summary>
    /// <stable>ICU 3.0</stable>
    /// <author>ram</author>
    public abstract class UResourceBundle : ResourceBundle, IEnumerable<UResourceBundle> //: ResourceManager
    {
        /// <summary>
        /// <icu/> Creates a resource bundle using the specified base name and locale.
        /// <see cref="ICUResourceBundle.IcuDataAssembly"/> is used as the default root.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="localeName">The locale for which a resource bundle is desired.</param>
        /// <exception cref="MissingManifestResourceException">If no resource bundle for the specified <paramref name="baseName"/> can be found.</exception>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="localeName"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(string baseName, string localeName)
        {
            return GetBundleInstance(baseName, localeName, GetAssembly(baseName),
                                     false);
        }

        // ICU4N TODO: Our main assembly won't be able to load any LanguageData, RegionData, etc.
        // Need to come up with a better way to retrieve these values
        private static Assembly GetAssembly(string baseName)
        {
            if (baseName.EndsWith("/lang", StringComparison.Ordinal) || baseName.Contains("/lang/"))
                return LocaleDisplayNamesImpl.LangDataTables.impl.GetType().GetTypeInfo().Assembly;
            if (baseName.EndsWith("/region", StringComparison.Ordinal) || baseName.Contains("/region/"))
                return LocaleDisplayNamesImpl.RegionDataTables.impl.GetType().GetTypeInfo().Assembly;

            return ICUResourceBundle.IcuDataAssembly;
        }

        /// <summary>
        /// <icu/> Creates a resource bundle using the specified <paramref name="baseName"/>, <paramref name="localeName"/>, 
        /// and assembly <paramref name="root"/>.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="localeName">The locale for which a resource bundle is desired.</param>
        /// <param name="root">The <see cref="Assembly"/> from which to load the resource bundle.</param>
        /// <exception cref="MissingManifestResourceException">If no resource bundle for the specified <paramref name="baseName"/> can be found.</exception>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="localeName"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(string baseName, string localeName,
                                                        Assembly root)
        {
            return GetBundleInstance(baseName, localeName, root, false);
        }

        /// <summary>
        /// <icu/> Creates a resource bundle using the specified <paramref name="baseName"/>, <paramref name="localeName"/>, and assembly
        /// <paramref name="root"/>.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="localeName">The locale for which a resource bundle is desired.</param>
        /// <param name="root">The <see cref="Assembly"/> from which to load the resource bundle.</param>
        /// <param name="disableFallback">Option to disable locale inheritence.
        /// If true the fallback chain will not be built.</param>
        /// <exception cref="MissingManifestResourceException">If no resource bundle for the specified <paramref name="baseName"/> can be found.</exception>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="localeName"/>.</returns>
        /// <stable>ICU 3.0</stable>
        protected static UResourceBundle GetBundleInstance(string baseName, string localeName,
                                                           Assembly root, bool disableFallback)
        {
            return InstantiateBundle(baseName, localeName, root, disableFallback);
        }

        /// <summary>
        /// <icu/> Sole constructor.  (For invocation by subclass constructors, typically
        /// implicit.)
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public UResourceBundle()
        {
        }

        /// <summary>
        /// <icu/> Creates a UResourceBundle for the locale specified, from which users can extract
        /// resources by using their corresponding keys.
        /// </summary>
        /// <param name="locale">Specifies the locale for which we want to open the resource.
        /// If null the bundle for default locale is opened.</param>
        /// <returns>A resource bundle for the given <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(ULocale locale)
        {
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            return GetBundleInstance(ICUData.IcuBaseName, locale.GetBaseName(),
                                     ICUResourceBundle.IcuDataAssembly, false);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UResourceBundle"/> for the default locale and specified <paramref name="baseName"/>,
        /// from which users can extract resources by using their corresponding keys.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and default locale.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(string baseName)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            ULocale uloc = ULocale.GetDefault();
            //return GetBundleInstance(baseName, uloc.GetBaseName(), ICUResourceBundle.ICU_DATA_CLASS_LOADER,
            //                         false);
            return GetBundleInstance(baseName, uloc.GetBaseName(), GetAssembly(baseName),
                                     false);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UResourceBundle"/> for the specified <paramref name="locale"/>locale and specified <paramref name="baseName"/>,
        /// from which users can extract resources by using their corresponding keys.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="locale">Specifies the locale for which we want to open the resource.
        /// If null the bundle for default locale is opened.</param>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(string baseName, CultureInfo locale)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            ULocale uloc = locale == null ? ULocale.GetDefault() : ULocale.ForLocale(locale);

            //return GetBundleInstance(baseName, uloc.GetBaseName(),
            //                         ICUResourceBundle.ICU_DATA_CLASS_LOADER, false);
            return GetBundleInstance(baseName, uloc.GetBaseName(),
                GetAssembly(baseName), false);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UResourceBundle"/>, from which users can extract resources by using
        /// their corresponding keys.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="locale">Specifies the locale for which we want to open the resource.
        /// If null the bundle for default locale is opened.</param>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public static UResourceBundle GetBundleInstance(string baseName, ULocale locale)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            //return GetBundleInstance(baseName, locale.GetBaseName(),
            //                         ICUResourceBundle.ICU_DATA_CLASS_LOADER, false);
            return GetBundleInstance(baseName, locale.GetBaseName(),
                GetAssembly(baseName), false);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UResourceBundle"/> for the specified locale and specified base name,
        /// from which users can extract resources by using their corresponding keys.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="locale">Specifies the locale for which we want to open the resource.
        /// If null the bundle for default locale is opened.</param>
        /// <param name="assembly">The assembly to use.</param>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.8</stable>
        public static UResourceBundle GetBundleInstance(string baseName, CultureInfo locale,
                                                        Assembly assembly)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            ULocale uloc = locale == null ? ULocale.GetDefault() : ULocale.ForLocale(locale);
            return GetBundleInstance(baseName, uloc.GetBaseName(), assembly, false);
        }

        /// <summary>
        /// <icu/> Creates a <see cref="UResourceBundle"/>, from which users can extract resources by using
        /// their corresponding keys.
        /// <para/>
        /// Note: Please use this API for loading non-ICU resources. .NET does not
        /// allow loading of resources across assemblies. You must provide your assembly
        /// to load the resources.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        ///  If null the default ICU package name is used.</param>
        /// <param name="locale">Specifies the locale for which we want to open the resource.
        /// If null the bundle for default locale is opened.</param>
        /// <param name="assembly">The assembly to use.</param>
        /// <returns>A resource bundle for the given <paramref name="baseName"/> and <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.8</stable>
        public static UResourceBundle GetBundleInstance(string baseName, ULocale locale,
                                                        Assembly assembly)
        {
            if (baseName == null)
            {
                baseName = ICUData.IcuBaseName;
            }
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            return GetBundleInstance(baseName, locale.GetBaseName(), assembly, false);
        }

        /// <summary>
        /// <icu/> Returns the RFC 3066 conformant locale id of this resource bundle.
        /// This method can be used after a call to <see cref="GetBundleInstance(string)"/> to
        /// determine whether the resource bundle returned really
        /// corresponds to the requested locale or is a fallback.
        /// </summary>
        /// <returns>The locale of this resource bundle.</returns>
        /// <stable>ICU 3.0</stable>
        public abstract ULocale GetULocale();

        /// <summary>
        /// <icu/> Returns the localeID.
        /// </summary>
        /// <returns>The string representation of the localeID.</returns>
        /// <stable>ICU 3.0</stable>
        protected abstract string GetLocaleID();

        /// <summary>
        /// <icu/> Returns the base name of the resource bundle.
        /// </summary>
        /// <returns>The string representation of the base name.</returns>
        /// <stable>ICU 3.0</stable>
        protected internal abstract string GetBaseName();

        /// <summary>
        /// <icu/> Gets the parent bundle, as <see cref="UResourceBundle"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        new public virtual UResourceBundle Parent
        {
            get { return (UResourceBundle)base.m_parent; }
        }

        /// <summary>
        /// Returns the locale of this bundle.
        /// </summary>
        /// <returns>The locale of this resource bundle.</returns>
        /// <stable>ICU 3.0</stable>
        public override CultureInfo GetLocale() // ICU4N TODO: API - rename GetCulture()
        {
            return GetULocale().ToLocale();
        }

        private enum RootType { MISSING, ICU, JAVA } // ICU4N TODO: API - rename from JAVA

        private static IDictionary<string, RootType> ROOT_CACHE = new ConcurrentDictionary<string, RootType>();

        private static RootType GetRootType(string baseName, Assembly root)
        {
            RootType rootType;

            if (!ROOT_CACHE.TryGetValue(baseName, out rootType))
            {
                string rootLocale = (baseName.IndexOf('.') == -1) ? "root" : "";
                try
                {
                    ICUResourceBundle.GetBundleInstance(baseName, rootLocale, root, true);
                    rootType = RootType.ICU;
                }
                catch (MissingManifestResourceException)
                {
                    try
                    {
                        ResourceBundleWrapper.GetBundleInstance(baseName, rootLocale, root, true);
                        rootType = RootType.JAVA;
                    }
                    catch (MissingManifestResourceException)
                    {
                        //throw away the exception
                        rootType = RootType.MISSING;
                    }
                }

                ROOT_CACHE[baseName] = rootType;
            }

            return rootType;
        }

        private static void SetRootType(string baseName, RootType rootType)
        {
            ROOT_CACHE[baseName] = rootType;
        }

        /// <summary>
        /// <icu/> Loads a new resource bundle for the given base name, locale and assembly.
        /// Optionally will disable loading of fallback bundles.
        /// </summary>
        /// <param name="baseName">String containing the name of the data package.
        /// If null the default ICU package name is used.</param>
        /// <param name="localeName">The locale for which a resource bundle is desired.</param>
        /// <param name="root">The class object from which to load the resource bundle.</param>
        /// <param name="disableFallback">Disables loading of fallback lookup chain.</param>
        /// <exception cref="MissingManifestResourceException">If no resource bundle for the specified base name can be found.</exception>
        /// <returns>A resource bundle for the given base name and locale.</returns>
        /// <stable>ICU 3.0</stable>
        protected static UResourceBundle InstantiateBundle(string baseName, string localeName,
                                                           Assembly root, bool disableFallback)
        {
            RootType rootType = GetRootType(baseName, root);

            switch (rootType)
            {
                case RootType.ICU:
                    return ICUResourceBundle.GetBundleInstance(baseName, localeName, root, disableFallback);

                case RootType.JAVA:
                    return ResourceBundleWrapper.GetBundleInstance(baseName, localeName, root,
                                                                   disableFallback);

                case RootType.MISSING:
                default:
                    UResourceBundle b;
                    try
                    {
                        b = ICUResourceBundle.GetBundleInstance(baseName, localeName, root,
                                                                disableFallback);
                        SetRootType(baseName, RootType.ICU);
                    }
                    catch (MissingManifestResourceException)
                    {
                        b = ResourceBundleWrapper.GetBundleInstance(baseName, localeName, root,
                                                                    disableFallback);
                        SetRootType(baseName, RootType.JAVA);
                    }
                    return b;
            }
        }

        /// <summary>
        /// <icu/> Returns a binary data item from a binary resource, as a read-only <see cref="ByteBuffer"/>.
        /// </summary>
        /// <returns>A pointer to a chunk of unsigned bytes which live in a memory mapped/DLL
        /// file.</returns>
        /// <seealso cref="GetInt32Vector()"/>
        /// <seealso cref="GetInt32()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual ByteBuffer GetBinary() // ICU4N TODO: API - change return type to byte[]
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// Returns a string from a string resource type.
        /// </summary>
        /// <returns>A string.</returns>
        /// <seealso cref="GetBinary()"/>
        /// <seealso cref="GetInt32Vector()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual string GetString()
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// Returns a string array from a array resource type.
        /// </summary>
        /// <returns>A string array.</returns>
        /// <seealso cref="GetString()"/>
        /// <seealso cref="GetInt32Vector()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual string[] GetStringArray()
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns a binary data from a binary resource, as a byte array with a copy
        /// of the bytes from the resource bundle.
        /// </summary>
        /// <param name="ba">The byte array to write the bytes to. A null variable is OK.</param>
        /// <returns>An array of bytes containing the binary data from the resource.</returns>
        /// <seealso cref="GetInt32Vector()"/>
        /// <seealso cref="GetInt32()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual byte[] GetBinary(byte[] ba)
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns a 32 bit integer array from a resource.
        /// </summary>
        /// <returns>A pointer to a chunk of unsigned bytes which live in a memory mapped/DLL file.</returns>
        /// <seealso cref="GetBinary()"/>
        /// <seealso cref="GetInt32()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual int[] GetInt32Vector()
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns a signed integer from a resource.
        /// </summary>
        /// <returns>An <see cref="int"/> value.</returns>
        /// <seealso cref="GetInt32Vector()"/>
        /// <seealso cref="GetBinary()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual int GetInt32()
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns a unsigned integer from a resource.
        /// This integer is originally 28 bit and the sign gets propagated.
        /// </summary>
        /// <returns>An <see cref="int"/> value.</returns>
        /// <seealso cref="GetInt32Vector()"/>
        /// <seealso cref="GetBinary()"/>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual int GetUInt32() // ICU4N TODO: API - return type should be uint (and marked CLSCompliant(false))
        {
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns a resource in a given resource that has a given key.
        /// </summary>
        /// <param name="aKey">A key associated with the wanted resource.</param>
        /// <returns>A resource bundle object representing the resource.</returns>
        /// <exception cref="MissingManifestResourceException">If resource bundle is missing.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual UResourceBundle Get(string aKey) // ICU4N TODO: API make into indexer property
        {
#pragma warning disable 612, 618
            UResourceBundle obj = FindTopLevel(aKey);
#pragma warning restore 612, 618
            if (obj == null)
            {
                string fullName = ICUResourceBundleReader.GetFullName(GetBaseName(), GetLocaleID());
                throw new MissingManifestResourceException(
                        "Can't find resource for bundle " + fullName + ", key "
                        + aKey + "Type: " + this.GetType().FullName + " Key: " + aKey);
            }
            return obj;
        }

        /// <summary>
        /// Returns a resource in a given resource that has a given key, or null if the
        /// resource is not found.
        /// </summary>
        /// <param name="aKey">The key associated with the wanted resource.</param>
        /// <returns>The resource, or null.</returns>
        /// <seealso cref="Get(string)"/>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected virtual UResourceBundle FindTopLevel(string aKey)
        {
            // NOTE: this only works for top-level resources.  For resources at lower
            // levels, it fails when you fall back to the parent, since you're now
            // looking at root resources, not at the corresponding nested resource.
            for (UResourceBundle res = this; res != null; res = res.Parent)
            {
                UResourceBundle obj = res.HandleGet(aKey, null, this);
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the string in a given resource at the specified index.
        /// </summary>
        /// <param name="index">An index to the wanted string.</param>
        /// <returns>A string which lives in the resource.</returns>
        /// <exception cref="IndexOutOfRangeException">If the index value is out of bounds of accepted values.</exception>
        /// <exception cref="UResourceTypeMismatchException">If resource bundle type mismatch.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual string GetString(int index)
        {
            ICUResourceBundle temp = (ICUResourceBundle)Get(index);
            if (temp.Type == UResourceType.String)
            {
                return temp.GetString();
            }
            throw new UResourceTypeMismatchException("");
        }

        /// <summary>
        /// <icu/> Returns the resource in a given resource at the specified index.
        /// </summary>
        /// <param name="index">An index to the wanted resource.</param>
        /// <returns>The sub resource <see cref="UResourceBundle"/> object.</returns>
        /// <exception cref="IndexOutOfRangeException">If the index value is out of bounds of accepted values.</exception>
        /// <exception cref="MissingManifestResourceException">If the resource bundle is missing.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual UResourceBundle Get(int index) // ICU4N TODO: API make into indexer property
        {
            UResourceBundle obj = HandleGet(index, null, this);
            if (obj == null)
            {
                obj = Parent;
                if (obj != null)
                {
                    obj = obj.Get(index);
                }
                if (obj == null)
                    throw new MissingManifestResourceException(
                            "Can't find resource for bundle "
                                    + this.GetType().FullName + ", key "
                                    + Key);
            }
            return obj;
        }

        /// <summary>
        /// Returns a resource in a given resource that has a given index, or null if the
        /// resource is not found.
        /// </summary>
        /// <param name="index">The index of the resource.</param>
        /// <returns>The resource, or null.</returns>
        /// <seealso cref="Get(int)"/>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected virtual UResourceBundle FindTopLevel(int index)
        {
            // NOTE: this _barely_ works for top-level resources.  For resources at lower
            // levels, it fails when you fall back to the parent, since you're now
            // looking at root resources, not at the corresponding nested resource.
            // Not only that, but unless the indices correspond 1-to-1, the index will
            // lose meaning.  Essentially this only works if the child resource arrays
            // are prefixes of their parent arrays.
            for (UResourceBundle res = this; res != null; res = res.Parent)
            {
                UResourceBundle obj = res.HandleGet(index, null, this);
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the keys in this bundle as an <see cref="IEnumerable{String}"/>,
        /// which is empty if this is not a bundle or a table resource.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public override IEnumerable<string> GetKeys() // ICU4N TODO: API - change to Keys property
        {
            return KeySet();
        }

        /// <summary>
        /// Returns a <see cref="ISet{String}"/> of all keys contained in this ResourceBundle and its parent bundles.
        /// </summary>
        /// <returns>a <see cref="ISet{String}"/> of all keys contained in this ResourceBundle and its parent bundles,
        /// which is empty if this is not a bundle or a table resource.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override ISet<string> KeySet() // ICU4N TODO: API - change to KeySet property
#pragma warning disable 809
        {
            // TODO: Java 6 ResourceBundle has keySet() which calls handleKeySet()
            // and caches the results.
            // When we upgrade to Java 6, we still need to check for isTopLevelResource().
            // Keep the else branch as is. The if body should just return super.keySet().
            // Remove then-redundant caching of the keys.
            ISet<string> keys = null;
            ICUResourceBundle icurb = null;
            if (IsTopLevelResource && this is ICUResourceBundle)
            {
                // We do not cache the top-level keys in this base class so that
                // not every string/int/binary... resource has to have a keys cache field.
                icurb = (ICUResourceBundle)this;
                keys = icurb.TopLevelKeySet;
            }
            if (keys == null)
            {
                if (IsTopLevelResource)
                {
                    SortedSet<string> newKeySet;
                    if (m_parent == null)
                    {
                        newKeySet = new SortedSet<string>();
                    }
                    else if (m_parent is UResourceBundle)
                    {
                        newKeySet = new SortedSet<string>(((UResourceBundle)m_parent).KeySet());
                    }
                    else
                    {
                        // TODO: Java 6 ResourceBundle has keySet(); use it when we upgrade to Java 6
                        // and remove this else branch.
                        newKeySet = new SortedSet<string>();
                        using (var parentKeys = Parent.GetKeys().GetEnumerator())
                        {
                            while (parentKeys.MoveNext())
                            {
                                newKeySet.Add(parentKeys.Current);
                            }
                        }
                    }
                    newKeySet.UnionWith(HandleKeySet());
                    keys = (newKeySet).ToUnmodifiableSet();
                    if (icurb != null)
                    {
                        icurb.TopLevelKeySet = keys;
                    }
                }
                else
                {
                    return HandleKeySet();
                }
            }
            return keys;
        }

        /// <summary>
        /// Returns a <see cref="ISet{String}"/> of the keys contained <i>only</i> in this ResourceBundle.
        /// This does not include further keys from parent bundles.
        /// </summary>
        /// <returns>A <see cref="ISet{String}"/> of the keys contained only in this ResourceBundle,
        /// which is empty if this is not a bundle or a table resource.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        protected override ISet<string> HandleKeySet()
#pragma warning disable 809
        {
            return new HashSet<string>();
        }

        /// <summary>
        /// <icu/> Gets the number of resources in a given resource. Number for scalar types is always 1, and for
        /// vector/table types is the number of child resources.
        /// <para/>
        /// <b>Note:</b> Integer array is treated as a scalar type. There are no APIs to
        /// access individual members of an integer array. It is always returned as a whole.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual int Length
        {
            get { return 1; }
        }

        /**
         * {@icu} Returns the type of a resource.
         * Available types are {@link #INT INT}, {@link #ARRAY ARRAY},
         * {@link #BINARY BINARY}, {@link #INT_VECTOR INT_VECTOR},
         * {@link #STRING STRING}, {@link #TABLE TABLE}.
         *
         * @return type of the given resource.
         * @stable ICU 3.8
         */
        public virtual UResourceType Type
        {
            get { return UResourceType.None; }
        }

        /// <summary>
        /// <icu/> Gets the version number associated with this <see cref="UResourceBundle"/> as an
        /// <see cref="VersionInfo"/> object.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public VersionInfo Version
        {
            get { return null; }
        }

        /// <summary>
        /// <icu/> Returns the enumerator which iterates over this
        /// resource bundle
        /// </summary>.
        /// <returns><see cref="UResourceBundleEnumerator"/> that iterates over the resources in the bundle.</returns>
        /// <stable>ICU 3.8</stable>
        public UResourceBundleEnumerator GetEnumerator()
        {
            return new UResourceBundleEnumerator(this);
        }

        #region .NET Compatibility
        IEnumerator<UResourceBundle> IEnumerable<UResourceBundle>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// <icu/>Gets the key associated with a given resource. Not all the resources have
        /// a key - only those that are members of a table. Returns <c>null</c> if it doesn't have a key.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual string Key
        {
            get { return null; }
        }

        // ICU4N specific - moved constants to enum named UResourceType and de-nested


        //====== protected members ==============

        /// <summary>
        /// <icu/> Actual worker method for fetching a resource based on the given key.
        /// Sub classes must override this method if they support resources with keys.
        /// </summary>
        /// <param name="aKey">The key string of the resource to be fetched.</param>
        /// <param name="aliasesVisited"><see cref="IDictionary{String, String}"/> to hold references of resources already seen.</param>
        /// <param name="requested">The original resource bundle object on which the get method was invoked.
        /// The requested bundle and the bundle on which this method is invoked
        /// are the same, except in the cases where aliases are involved.</param>
        /// <returns><see cref="UResourceBundle"/> resource associated with the index.</returns>
        /// <stable>ICU 3.8</stable>
        protected virtual UResourceBundle HandleGet(string aKey, IDictionary<string, string> aliasesVisited,
                                            UResourceBundle requested)
        {
            return null;
        }

        /// <summary>
        /// <icu/> Actual worker method for fetching a resource based on the given index.
        /// Sub classes must override this method if they support arrays of resources.
        /// </summary>
        /// <param name="index">The index of the resource to be fetched.</param>
        /// <param name="aliasesVisited"><see cref="IDictionary{String, String}"/> to hold references of resources already seen.</param>
        /// <param name="requested">The original resource bundle object on which the get method was invoked.
        /// The requested bundle and the bundle on which this method is invoked
        /// are the same, except in the cases where aliases are involved.</param>
        /// <returns><see cref="UResourceBundle"/> resource associated with the index.</returns>
        /// <stable>ICU 3.8</stable>
        protected virtual UResourceBundle HandleGet(int index, IDictionary<string, string> aliasesVisited,
                                            UResourceBundle requested)
        {
            return null;
        }

        /// <summary>
        /// <icu/> Actual worker method for fetching the array of strings in a resource.
        /// Sub classes must override this method if they support arrays of strings.
        /// </summary>
        /// <returns><see cref="T:string[]"/> array containing resource strings.</returns>
        /// <stable>ICU 3.8</stable>
        protected virtual string[] HandleGetStringArray()
        {
            return null;
        }

        /// <summary>
        /// <icu/> Actual worker method for fetching the keys of resources contained in the resource.
        /// Sub classes must override this method if they support keys and associated resources.
        /// </summary>
        /// <returns><see cref="IEnumerable{String}"/> of all the keys in this resource.</returns>
        /// <stable>ICU 3.8</stable>
        protected virtual IEnumerable<string> HandleGetKeys()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <stable>ICU 3.8</stable>
        // {@inheritDoc}

        // this method is declared in ResourceBundle class
        // so cannot change the signature
        // Override this method
        protected override object HandleGetObject(string aKey)
        {
            return HandleGetObjectImpl(aKey, this);
        }

        /// <summary>
        /// Override the superclass method
        /// </summary>
        // To facilitate XPath style aliases we need a way to pass the reference
        // to requested locale. The only way I could figure out is to implement
        // the look up logic here. This has a disadvantage that if the client
        // loads an ICUResourceBundle, calls ResourceBundle.getObject method
        // with a key that does not exist in the bundle then the lookup is
        // done twice before throwing a MissingResourceExpection.
        private object HandleGetObjectImpl(string aKey, UResourceBundle requested)
        {
            object obj = ResolveObject(aKey, requested);
            if (obj == null)
            {
                UResourceBundle parentBundle = Parent;
                if (parentBundle != null)
                {
                    obj = parentBundle.HandleGetObjectImpl(aKey, requested);
                }
                if (obj == null)
                    throw new MissingManifestResourceException(
                        "Can't find resource for bundle "
                        + this.GetType().FullName + ", key " + aKey);
            }
            return obj;
        }

        // Routine for figuring out the type of object to be returned
        // string or string array
        private object ResolveObject(string aKey, UResourceBundle requested)
        {
            if (Type == UResourceType.String)
            {
                return GetString();
            }
            UResourceBundle obj = HandleGet(aKey, null, requested);
            if (obj != null)
            {
                if (obj.Type == UResourceType.String)
                {
                    return obj.GetString();
                }
                try
                {
                    if (obj.Type == UResourceType.Array)
                    {
                        return obj.HandleGetStringArray();
                    }
                }
                catch (UResourceTypeMismatchException)
                {
                    return obj;
                }
            }
            return obj;
        }

        /// <summary>
        /// Is this a top-level resource, that is, a whole bundle?
        /// <para/>
        /// Returns <c>true</c> if this is a top-level resource.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected virtual bool IsTopLevelResource
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Resource type constants
    /// </summary>
    // ICU4N: These constants were combined from UResourceBundle and ICUResourceBundle in ICU4J
    public enum UResourceType
    {
        /// <summary>
        /// <icu/> Resource type constant for "no resource".
        /// </summary>
        /// <stable>ICU 3.8</stable>
        None = -1,

        /// <summary>
        /// <icu/> Resource type constant for strings.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        String = 0,

        /// <summary>
        /// <icu/> Resource type constant for binary data.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        Binary = 1,

        /// <summary>
        /// <icu/> Resource type constant for tables of key-value pairs.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        Table = 2,

        /// <summary>
        /// Resource type constant for aliases;
        /// internally stores a string which identifies the actual resource
        /// storing the data (can be in a different resource bundle).
        /// Resolved internally before delivering the actual resource through the API.
        /// </summary>
        Alias = 3,

        /// <summary>
        /// Resource type constant for tables with 32-bit count, key offsets and values.
        /// </summary>
        Table32 = 4,

        /// <summary>
        /// Resource type constant for tables with 16-bit count, key offsets and values.
        /// All values are <see cref="UResourceType.StringV2"/> strings.
        /// </summary>
        Table16 = 5,

        /// <summary>
        /// Resource type constant for 16-bit Unicode strings in formatVersion 2.
        /// </summary>
        StringV2 = 6,

        /// <summary>
        /// Resource type constant for arrays with 16-bit count and values.
        /// All values are <see cref="UResourceType.StringV2"/> strings.
        /// </summary>
        Array16 = 9,

        /// <summary>
        /// <icu/> Resource type constant for a single 28-bit integer, interpreted as
        /// signed or unsigned by the <see cref="UResourceBundle.GetInt32()"/> function.
        /// </summary>
        /// <seealso cref="UResourceBundle.GetInt32()"/>
        /// <stable>ICU 3.8</stable>
        Int32 = 7,

        /// <summary>
        /// <icu/> Resource type constant for arrays of resources.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        Array = 8,

        /// <summary>
        /// Resource type constant for vectors of 32-bit integers.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        Int32Vector = 14
    }


    // ICU4N: temporary stub until we work out how to implement ResourceBundle
    public abstract class ResourceBundle
    {
        /**
         * The parent bundle of this bundle.
         * The parent bundle is searched by {@link #getObject getObject}
         * when this bundle does not contain a particular resource.
         */
        protected internal ResourceBundle m_parent = null;

        public virtual ResourceBundle Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        public virtual void SetParent(ResourceBundle parent)
        {
            this.m_parent = parent;
        }

        public abstract CultureInfo GetLocale();


        public object GetObject(string key)
        {
            object obj = HandleGetObject(key);
            if (obj == null)
            {
                if (m_parent != null)
                {
                    obj = m_parent.GetObject(key);
                }
                if (obj == null)
                {
                    throw new MissingManifestResourceException("Can't find resource for bundle " +
                        this.GetType().Name + ", key " + key);
                }
            }
            return obj;
        }

        public string GetString(string key)
        {
            return (string)GetObject(key);
        }

        public string[] GetStringArray(string key)
        {
            return (string[])GetObject(key);
        }

        public abstract IEnumerable<string> GetKeys();

        public virtual ISet<string> KeySet()
        {
            ISet<string> keys = new HashSet<string>();
            for (ResourceBundle rb = this; rb != null; rb = rb.m_parent)
            {
                keys.UnionWith(rb.HandleKeySet());
            }
            return keys;
        }

        protected ISet<string> m_keySet = null;

        protected virtual ISet<string> HandleKeySet()
        {
            if (m_keySet == null)
            {
                lock(this) {
                    if (m_keySet == null)
                    {
                        ISet<string> keys = new HashSet<string>();
                        using (var enumKeys = GetKeys().GetEnumerator())
                        {
                            while (enumKeys.MoveNext())
                            {
                                string key = enumKeys.Current;
                                if (HandleGetObject(key) != null)
                                {
                                    keys.Add(key);
                                }
                            }
                        }
                        m_keySet = keys;
                    }
                }
            }
            return m_keySet;
        }

        protected abstract object HandleGetObject(string key);
    }
}
