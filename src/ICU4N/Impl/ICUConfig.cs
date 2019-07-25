using ICU4N.Support;
using System.Resources;

namespace ICU4N.Impl
{
    /// <summary>
    /// ICUConfig is a class used for accessing ICU4N runtime configuration.
    /// </summary>
    public class ICUConfig
    {
        //public static readonly string CONFIG_PROPS_FILE = "/com/ibm/icu/ICUConfig.properties";
        //private static readonly IDictionary<string, string> CONFIG_PROPS;

        //static ICUConfig()
        //{
        //    CONFIG_PROPS = new Dictionary<string, string>();
        //    try
        //    {
        //        using (Stream input = ICUData.GetStream(CONFIG_PROPS_FILE))
        //        {
        //            if (input != null)
        //            {
        //                // ICU4N TODO: Go with some other type of "properties" in .NET?
        //                //CONFIG_PROPS.Load(input);
        //            }
        //        }
        //        //} catch (MissingResourceException mre) {
        //        // If it does not exist, ignore.
        //    }
        //    catch (IOException ioe)
        //    {
        //        // Any IO errors, ignore
        //    }
        //}

        /// <summary>
        /// Get ICU configuration property value for the given name.
        /// </summary>
        /// <param name="name">The configuration property name.</param>
        /// <returns>The configuration property value, or null if it does not exist.</returns>
        public static string Get(string name)
        {
            return Get(name, null);
        }

        /// <summary>
        /// Get ICU configuration property value for the given name.
        /// </summary>
        /// <param name="name">The configuration property name.</param>
        /// <param name="def">The default value.</param>
        /// <returns>The configuration property value.  If the property does not
        /// exist, <paramref name="def"/> is returned.</returns>
        public static string Get(string name, string def)
        {
            if (string.IsNullOrEmpty(name))
                return def;

            string value = null;

            // Try to get an environment variable first
            value = SystemProperties.GetProperty(name, null);
            if (value != null)
                return value;

            try
            {
                value =  global::ICU4N.ICUConfig.ResourceManager.GetString(name.Replace(".", "_"));
                if (value != null)
                    return value;
            }
            catch (MissingManifestResourceException)
            {
            }

            return def;
        }
    }
}
