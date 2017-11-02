using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Provides access to ICU data files as InputStreams.  Implements security checking.
    /// </summary>
    public sealed class ICUData
    {
        /**
     * The data path to be used with getBundleInstance API
     */
        internal static readonly string ICU_DATA_PATH = "com/ibm/icu/impl/";
        /**
         * The ICU data package name.
         * This is normally the name of the .dat package, and the prefix (plus '/')
         * of the package entry names.
         */
        internal static readonly string PACKAGE_NAME = "icudt" + VersionInfo.ICU_DATA_VERSION_PATH;
        /**
         * The data path to be used with Class.getResourceAsStream().
         */
        public static readonly string ICU_BUNDLE = "data/" + PACKAGE_NAME;

        /**
         * The base name of ICU data to be used with ClassLoader.getResourceAsStream(),
         * ICUResourceBundle.getBundleInstance() etc.
         */
        public static readonly string ICU_BASE_NAME = ICU_DATA_PATH + ICU_BUNDLE;

        /**
         * The base name of collation data to be used with getBundleInstance API
         */
        public static readonly string ICU_COLLATION_BASE_NAME = ICU_BASE_NAME + "/coll";

        /**
         * The base name of rbbi data to be used with getData API
         */
        public static readonly string ICU_BRKITR_NAME = "brkitr";

        /**
         * The base name of rbbi data to be used with getBundleInstance API
         */
        public static readonly string ICU_BRKITR_BASE_NAME = ICU_BASE_NAME + '/' + ICU_BRKITR_NAME;

        /**
         * The base name of rbnf data to be used with getBundleInstance API
         */
        public static readonly string ICU_RBNF_BASE_NAME = ICU_BASE_NAME + "/rbnf";

        /**
         * The base name of transliterator data to be used with getBundleInstance API
         */
        public static readonly string ICU_TRANSLIT_BASE_NAME = ICU_BASE_NAME + "/translit";

        public static readonly string ICU_LANG_BASE_NAME = ICU_BASE_NAME + "/lang";
        public static readonly string ICU_CURR_BASE_NAME = ICU_BASE_NAME + "/curr";
        public static readonly string ICU_REGION_BASE_NAME = ICU_BASE_NAME + "/region";
        public static readonly string ICU_ZONE_BASE_NAME = ICU_BASE_NAME + "/zone";
        public static readonly string ICU_UNIT_BASE_NAME = ICU_BASE_NAME + "/unit";

        /**
         * For testing (otherwise false): When reading an InputStream from a Class or ClassLoader
         * (that is, not from a file), log when the stream contains ICU binary data.
         *
         * This cannot be ICUConfig'ured because ICUConfig calls ICUData.getStream()
         * to read the properties file, so we would get a circular dependency
         * in the class initialization.
         */
        private static readonly bool logBinaryDataFromInputStream = false;
        //private static readonly Logger logger = logBinaryDataFromInputStream?
        //        Logger.getLogger(ICUData.class.getName()) : null;

        public static bool Exists(string resourceName)
        {
            // ICU4N TODO: finish
            //            URL i = null;
            //            if (System.getSecurityManager() != null)
            //            {
            ////                i = AccessController.doPrivileged(new PrivilegedAction<URL>() {
            ////                    @Override
            ////                    public URL run()
            ////                {
            ////                    return ICUData.class.getResource(resourceName);
            ////    }
            ////});
            //        } else {
            //            i = ICUData.class.getResource(resourceName);
            //        }
            //        return i != null;
            return false;
        }

        private static Stream GetStream(Type root, string resourceName, bool required)
        {
            throw new NotImplementedException();
            // ICU4N TODO: Finish
            //    Stream i = null;
            //    if (System.getSecurityManager() != null)
            //    {
            //        i = AccessController.doPrivileged(new PrivilegedAction<InputStream>() {
            //                    @Override
            //                    public InputStream run()
            //        {
            //            return root.getResourceAsStream(resourceName);
            //        }
            //    });
            //} else {
            //            i = root.getResourceAsStream(resourceName);
            //        }

            //        if (i == null && required) {
            //            throw new MissingResourceException("could not locate data " +resourceName, root.getPackage().getName(), resourceName);
            //        }
            //        checkStreamForBinaryData(i, resourceName);
            //        return i;
        }

        /**
         * Should be called only from ICUBinary.getData() or from convenience overloads here.
         */
        internal static Stream GetStream(/*ClassLoader*/ object loader, string resourceName, bool required)
        {
            throw new NotImplementedException();
            // ICU4N TODO: Finish
            //    InputStream i = null;
            //    if (System.getSecurityManager() != null)
            //    {
            //        i = AccessController.doPrivileged(new PrivilegedAction<InputStream>() {
            //                    @Override
            //                    public InputStream run()
            //        {
            //            return loader.getResourceAsStream(resourceName);
            //        }
            //    });
            //} else {
            //            i = loader.getResourceAsStream(resourceName);
            //        }
            //        if (i == null && required) {
            //            throw new MissingResourceException("could not locate data", loader.toString(), resourceName);
            //        }
            //        checkStreamForBinaryData(i, resourceName);
            //        return i;
        }

        //@SuppressWarnings("unused")  // used if logBinaryDataFromInputStream == true
        private static void CheckStreamForBinaryData(Stream input, String resourceName)
        {
            throw new NotImplementedException();
            // ICU4N TODO: Finish
            //if (logBinaryDataFromInputStream && is != null && resourceName.indexOf(PACKAGE_NAME) >= 0)
            //{
            //    try
            //    {
            //            is.mark(32);
            //        byte[] b = new byte[32];
            //        int len = is.read(b);
            //        if (len == 32 && b[2] == (byte)0xda && b[3] == 0x27)
            //        {
            //            String msg = String.format(
            //                    "ICU binary data file loaded from Class/ClassLoader as InputStream " +
            //                    "from %s: MappedData %02x%02x%02x%02x  dataFormat %02x%02x%02x%02x",
            //                    resourceName,
            //                    b[0], b[1], b[2], b[3],
            //                    b[12], b[13], b[14], b[15]);
            //            logger.info(msg);
            //        }
            //            is.reset();
            //    }
            //    catch (IOException ignored)
            //    {
            //    }
            //}
        }

        public static Stream GetStream(/*ClassLoader*/object loader, String resourceName)
        {
            return GetStream(loader, resourceName, false);
        }

        public static Stream GetRequiredStream(/*ClassLoader*/object loader, string resourceName)
        {
            return GetStream(loader, resourceName, true);
        }

        /**
         * Convenience override that calls getStream(ICUData.class, resourceName, false);
         * Returns null if the resource could not be found.
         */
        public static Stream GetStream(string resourceName)
        {
            return GetStream(typeof(ICUData), resourceName, false);
        }

        /**
         * Convenience method that calls getStream(ICUData.class, resourceName, true).
         * @throws MissingResourceException if the resource could not be found
         */
        public static Stream GetRequiredStream(string resourceName)
        {
            return GetStream(typeof(ICUData), resourceName, true);
        }

        /**
         * Convenience override that calls getStream(root, resourceName, false);
         * Returns null if the resource could not be found.
         */
        public static Stream GetStream(Type root, string resourceName)
        {
            return GetStream(root, resourceName, false);
        }

        /**
         * Convenience method that calls getStream(root, resourceName, true).
         * @throws MissingResourceException if the resource could not be found
         */
        public static Stream GetRequiredStream(Type root, string resourceName)
        {
            return GetStream(root, resourceName, true);
        }
    }
}
