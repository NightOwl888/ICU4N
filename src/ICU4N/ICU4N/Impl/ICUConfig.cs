using ICU4N.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text;

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

        /**
         * Get ICU configuration property value for the given name.
         * @param name The configuration property name
         * @return The configuration property value, or null if it does not exist.
         */
        public static string Get(string name)
        {
            return Get(name, null);
        }

        /**
         * Get ICU configuration property value for the given name.
         * @param name The configuration property name
         * @param def The default value
         * @return The configuration property value.  If the property does not
         * exist, <code>def</code> is returned.
         */
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
