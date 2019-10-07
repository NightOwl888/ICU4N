using ICU4N.Support.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Collation root provider.
    /// </summary>
    /// <since>2012dec17</since>
    /// <author>Markus W. Scherer</author>
    public static class CollationRoot // purely static
    {
        private static readonly CollationTailoring rootSingleton;
        private static readonly Exception exception;

        public static CollationTailoring Root
        {
            get
            {
                if (exception != null)
                {
                    throw exception;
                }
                return rootSingleton;
            }
        }
        public static CollationData Data
        {
            get
            {
                CollationTailoring root = Root;
                return root.Data;
            }
        }
        internal static CollationSettings Settings
        {
            get
            {
                CollationTailoring root = Root;
                return root.Settings.ReadOnly;
            }
        }

        static CollationRoot()
        {  // Corresponds to C++ load() function.
            CollationTailoring t = null;
            Exception e2 = null;
            try
            {
                // ICU4N specific - passing in assembly name so we resolve to this assembly rather than ICU4N.dll
                ByteBuffer bytes = ICUBinary.GetRequiredData(typeof(CollationRoot).GetTypeInfo().Assembly, null, "coll/ucadata.icu");
                CollationTailoring t2 = new CollationTailoring(null);
                CollationDataReader.Read(null, bytes, t2);
                // Keep t=null until after the root data has been read completely.
                // Otherwise we would set a non-null root object if the data reader throws an exception.
                t = t2;
            }
            catch (IOException e)
            {
                e2 = new MissingManifestResourceException(
                        "IOException while reading CLDR root data, " +
                        "type: CollationRoot, bundle: " + ICUData.IcuBundle + "/coll/ucadata.icu", e);
            }
            catch (Exception e)
            {
                e2 = e;
            }
            rootSingleton = t;
            exception = e2;
        }
    }
}
