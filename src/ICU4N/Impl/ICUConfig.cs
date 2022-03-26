using ICU4N.Support;

namespace ICU4N.Impl
{
    /// <summary>
    /// ICUConfig is a class used for accessing ICU4N runtime configuration.
    /// </summary>
    internal class ICUConfig // ICU4N specific - reads configuration settings from IConfiguration
    {
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

            return SystemProperties.GetProperty(name, def);
        }
    }
}
