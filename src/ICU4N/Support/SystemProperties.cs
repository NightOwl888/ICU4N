using System;
using System.Collections.Concurrent;
using System.Security;

namespace ICU4N.Support
{
    /// <summary>
    /// Helper for environment variables. This class helps to convert the environment
    /// variables to int or bool data types and also silently handles read permission
    /// errors.
    /// <para/>
    /// For instructions how to set environment variables for your OS, see 
    /// <a href="https://my.schrodinger.com/support/article/1842">https://my.schrodinger.com/support/article/1842</a>.
    /// <para/>
    /// Set the environment variable <c>ICU4N_IGNORE_ENVVAR_SECURITY_EXCEPTIONS</c> to <c>false</c>
    /// to change the read behavior of these methods to throw the underlying exception
    /// instead of returning the default value.
    /// </summary>
    internal static class SystemProperties // ICU4N: We can probably factor this out completely once ICUConfig and ICUDebug classes are refactored to use .NET APIs
    {
        private static readonly ConcurrentDictionary<string, PropertyNames> propertyNameCache = new(StringComparer.Ordinal);

        private static PropertyNames GetPropertyNames(string logicalName)
        {
            return propertyNameCache.GetOrAdd(
                logicalName,
                static x => new PropertyNames(x));
        }

        internal sealed class PropertyNames
        {
            public PropertyNames(string logicalName)
            {
                LogicalName = logicalName;

                AppConfigurationName =
                    $"ICU4N:{logicalName.Replace('_', ':')}";

                EnvironmentVariableName =
                    $"ICU4N_{logicalName.ToUpperInvariant()}";
            }

            public string LogicalName { get; }

            public string AppConfigurationName { get; }

            public string EnvironmentVariableName { get; }
        }


        /// <summary>
        /// Retrieves the value of an environment variable from the current process.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <returns>The environment variable value.</returns>
        public static string GetProperty(string key)
        {
            return GetProperty(key, null);
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process, 
        /// with a default value if it doens't exist or the caller doesn't have 
        /// permission to read the value.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="defaultValue">The value to use if the environment variable does not exist 
        /// or the caller doesn't have permission to read the value.</param>
        /// <returns>The environment variable value.</returns>
        public static string GetProperty(string key, string defaultValue)
        {
            return GetProperty<string>(key, defaultValue,
                (str) =>
                {
                    return str;
                }
            );
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process
        /// as <see cref="bool"/>. If the value cannot be cast to <see cref="bool"/>, returns <c>false</c>.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <returns>The environment variable value.</returns>
        public static bool GetPropertyAsBoolean(string key)
        {
            return GetPropertyAsBoolean(key, false);
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process as <see cref="bool"/>, 
        /// with a default value if it doens't exist, the caller doesn't have permission to read the value, 
        /// or the value cannot be cast to a <see cref="bool"/>.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="defaultValue">The value to use if the environment variable does not exist,
        /// the caller doesn't have permission to read the value, or the value cannot be cast to <see cref="bool"/>.</param>
        /// <returns>The environment variable value.</returns>
        public static bool GetPropertyAsBoolean(string key, bool defaultValue)
        {
            return GetProperty<bool>(key, defaultValue,
                (str) =>
                {
                    return bool.TryParse(str, out bool value) ? value : defaultValue;
                }
            );
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process
        /// as <see cref="int"/>. If the value cannot be cast to <see cref="int"/>, returns <c>0</c>.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <returns>The environment variable value.</returns>
        public static int GetPropertyAsInt32(string key)
        {
            return GetPropertyAsInt32(key, 0);
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process as <see cref="int"/>, 
        /// with a default value if it doens't exist, the caller doesn't have permission to read the value, 
        /// or the value cannot be cast to a <see cref="int"/>.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="defaultValue">The value to use if the environment variable does not exist,
        /// the caller doesn't have permission to read the value, or the value cannot be cast to <see cref="int"/>.</param>
        /// <returns>The environment variable value.</returns>
        public static int GetPropertyAsInt32(string key, int defaultValue)
        {
            return GetProperty<int>(key, defaultValue,
                (str) =>
                {
                    return int.TryParse(str, out int value) ? value : defaultValue;
                }
            );
        }

        private static T GetProperty<T>(string key, T defaultValue, Func<string, T> conversionFunction)
        {
            if (key is null)
                return defaultValue;

            PropertyNames names = GetPropertyNames(key);

            string setting;
            if (ignoreSecurityExceptions)
            {
                try
                {
                    setting = Environment.GetEnvironmentVariable(names.EnvironmentVariableName);
                }
                catch (SecurityException)
                {
                    setting = null;
                }
            }
            else
            {
                setting = Environment.GetEnvironmentVariable(names.EnvironmentVariableName);
            }

            return string.IsNullOrEmpty(setting)
                ? defaultValue
                : conversionFunction(setting);
        }

        internal static bool ignoreSecurityExceptions = GetPropertyAsBoolean("ICU4N_IGNORE_ENVVAR_SECURITY_EXCEPTIONS", true);
    }
}
