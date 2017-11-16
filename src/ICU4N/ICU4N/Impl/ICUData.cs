using ICU4N.Logging;
using ICU4N.Support;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
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
        internal static readonly string ICU_DATA_PATH = "ICU4N.Impl."; // ICU4N TODO: Perhaps leave off the assembly name and make a more aggressive resource scan that matches the end of the path loosely?
        /**
         * The ICU data package name.
         * This is normally the name of the .dat package, and the prefix (plus '/')
         * of the package entry names.
         */
        internal static readonly string PACKAGE_NAME = "icudt" + VersionInfo.ICU_DATA_VERSION_PATH;
        /**
         * The data path to be used with Class.getResourceAsStream().
         */
        public static readonly string ICU_BUNDLE = "Data"; //"Data." + PACKAGE_NAME;

        /**
         * The base name of ICU data to be used with ClassLoader.getResourceAsStream(),
         * ICUResourceBundle.getBundleInstance() etc.
         */
        public static readonly string ICU_BASE_NAME = ICU_DATA_PATH + ICU_BUNDLE;

        /**
         * The base name of collation data to be used with getBundleInstance API
         */
        public static readonly string ICU_COLLATION_BASE_NAME = ICU_BASE_NAME + ".coll";

        /**
         * The base name of rbbi data to be used with getData API
         */
        public static readonly string ICU_BRKITR_NAME = "brkitr";

        /**
         * The base name of rbbi data to be used with getBundleInstance API
         */
        public static readonly string ICU_BRKITR_BASE_NAME = ICU_BASE_NAME + '.' + ICU_BRKITR_NAME;

        /**
         * The base name of rbnf data to be used with getBundleInstance API
         */
        public static readonly string ICU_RBNF_BASE_NAME = ICU_BASE_NAME + ".rbnf";

        /**
         * The base name of transliterator data to be used with getBundleInstance API
         */
        public static readonly string ICU_TRANSLIT_BASE_NAME = ICU_BASE_NAME + ".translit";

        public static readonly string ICU_LANG_BASE_NAME = ICU_BASE_NAME + ".lang";
        public static readonly string ICU_CURR_BASE_NAME = ICU_BASE_NAME + ".curr";
        public static readonly string ICU_REGION_BASE_NAME = ICU_BASE_NAME + ".region";
        public static readonly string ICU_ZONE_BASE_NAME = ICU_BASE_NAME + ".zone";
        public static readonly string ICU_UNIT_BASE_NAME = ICU_BASE_NAME + ".unit";

        /**
         * For testing (otherwise false): When reading an InputStream from a Class or ClassLoader
         * (that is, not from a file), log when the stream contains ICU binary data.
         *
         * This cannot be ICUConfig'ured because ICUConfig calls ICUData.getStream()
         * to read the properties file, so we would get a circular dependency
         * in the class initialization.
         */
        private static readonly bool logBinaryDataFromInputStream = SystemProperties.GetPropertyAsBoolean("ICU4N.LogBinaryDataFromInputStream", false);
        private static readonly ILog logger = logBinaryDataFromInputStream ? LogProvider.For<ICUData>() : null;

        public static bool Exists(string resourceName)
        {
            return typeof(ICUData).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName) != null;
        }

        private static Stream GetStream(Type root, string resourceName, bool required)
        {
            Stream i;
            i = root.GetTypeInfo().Assembly.GetManifestResourceStream(resourceName);
            if (i == null && required)
            {
                throw new MissingManifestResourceException("could not locate data " + resourceName +
                    " Assembly: " + root.GetTypeInfo().Assembly.FullName + " Resource: " + resourceName);
            }
            CheckStreamForBinaryData(i, resourceName);
            return i;
        }

        /**
         * Should be called only from ICUBinary.getData() or from convenience overloads here.
         */
        internal static Stream GetStream(Assembly loader, string resourceName, bool required)
        {
            Stream i = null;
            i = loader.GetManifestResourceStream(resourceName);
            if (i == null && required)
            {
                throw new MissingManifestResourceException("could not locate data " + loader.ToString() + " Resource: " + resourceName);
            }
            CheckStreamForBinaryData(i, resourceName);
            return i;
        }

        private static void CheckStreamForBinaryData(Stream input, string resourceName)
        {
            // ICU4N specific - only check the data if the stream is seekable
            if (logBinaryDataFromInputStream && input != null && input.CanSeek && resourceName.IndexOf(PACKAGE_NAME) >= 0)
            {
                try
                {
                    byte[] b = new byte[32];
                    int len = input.Read(b, 0, b.Length);
                    if (len == 32 && b[2] == (byte)0xda && b[3] == 0x27)
                    {
                        string msg = string.Format(
                                "ICU binary data file loaded from Assembly as Stream " +
                                "from {0}: MappedData {1:x2}{2:x2}{3:x2}{4:x2}  dataFormat {5:x2}{6:x2}{7:x2}{8:x2}",
                                resourceName,
                                b[0], b[1], b[2], b[3],
                                b[12], b[13], b[14], b[15]);
                        logger.Info(msg);
                    }
                    input.Seek(b.Length, SeekOrigin.Current);
                }
                catch (IOException)
                {
                    // ignored
                }
            }
        }

        public static Stream GetStream(Assembly loader, string resourceName)
        {
            return GetStream(loader, resourceName, false);
        }

        public static Stream GetRequiredStream(Assembly loader, string resourceName)
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
