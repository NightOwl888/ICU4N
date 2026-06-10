using ICU4N.Support;
using J2N;
using System.Collections.Generic;
using System.IO;

namespace ICU4N.Impl
{
    /// <summary>
    /// ICUConfig is a class used for accessing ICU4N runtime configuration.
    /// </summary>
    internal class ICUConfig // ICU4N TODO: Refactor to use IConfiguration from .NET and make public
    {
        private const string ConfigPropsFile = "ICU4N.ICUConfig.properties";
        private static readonly Dictionary<string, string> ConfigProps = LoadConfigProps();

        private static Dictionary<string, string> LoadConfigProps()
        {
            Dictionary<string, string> props = [];
            try
            {
                using Stream input = typeof(ICUConfig).Assembly.GetManifestResourceStream(ConfigPropsFile);
                if (input != null)
                {
                    props.LoadProperties(input);
                }
            }
            catch (IOException)
            {
                // Any IO errors, ignore
            }
            return props;
        }

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
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The configuration property value.  If the property does not
        /// exist, <paramref name="defaultValue"/> is returned.</returns>
        public static string Get(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(name))
                return defaultValue;

            System.Diagnostics.Debug.Assert(!name.Contains('.'), $"ICUConfig property names should not contain '.'. These should be converted to '_' without namespaces for .NET. Value: {name}");

            // Try to get an environment variable first
            string value = SystemProperties.GetProperty(name, null);
            if (value != null)
                return value;

            if (ConfigProps.TryGetValue(name, out value))
                return value;

            return defaultValue;
        }
    }
}
