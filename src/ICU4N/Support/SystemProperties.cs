﻿
#if !NET40
using ICU4N.Configuration;
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Security;

namespace ICU4N.Support
{
    /// <summary>
    /// Helper for environment variables. This class helps to convert the environment
    /// variables to int or bool data types and also silently handles read permission
    /// errors.
    /// <para/>
    /// For instructions how to set environment variables for your OS, see 
    /// <a href="https://www.schrodinger.com/kb/1842">https://www.schrodinger.com/kb/1842</a>.
    /// <para/>
    /// Note that if you want to load any of these settings for your application from a
    /// configuration file, it is recommended your application load them at startup and
    /// call <see cref="SystemProperties.SetProperty(string, string)"/> to set them.
    /// <para/>
    /// Set the environment variable <c>icu4n.ignoreSecurityExceptions</c> to <c>false</c>
    /// to change the read behavior of these methods to throw the underlying exception 
    /// instead of returning the default value.
    /// </summary>
    internal static class SystemProperties // ICU4N: We can probably factor this out completely once ICUConfig and ICUDebug classes are refactored to use .NET APIs
    {
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
                    bool value;
                    return bool.TryParse(str, out value) ? value : defaultValue;
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
                    int value;
                    return int.TryParse(str, out value) ? value : defaultValue;
                }
            );
        }

        internal static bool ignoreSecurityExceptions = GetPropertyAsBoolean("icu4n.ignoreSecurityExceptions", true);
#if NET40
        private static T GetProperty<T>(string key, T defaultValue, Func<string, T> conversionFunction)
        {
            string setting;
            if (ignoreSecurityExceptions)
            {
                try
                {
                    setting = Environment.GetEnvironmentVariable(key);
                }
                catch (SecurityException)
                {
                    setting = null;
                }
            }
            else
            {
                setting = Environment.GetEnvironmentVariable(key);
            }

            return string.IsNullOrEmpty(setting)
                ? defaultValue
                : conversionFunction(setting);
        }


        /// <summary>
        /// Creates, modifies, or deletes an environment variable stored in the current process.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="value">The new environment variable value.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission to perform this operation.</exception>
        public static void SetProperty(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
#else

        private static T GetProperty<T>(string key, T defaultValue, Func<string, T> conversionFunction)
        {
            IConfiguration configuration = ConfigurationSettings.GetConfigurationFactory().CreateConfiguration();
            string setting = configuration[key];

            return string.IsNullOrEmpty(setting)
                ? defaultValue
                : conversionFunction(setting);
        }

        /// <summary>
        /// Creates, modifies, or deletes an environment variable stored in the current process.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <param name="value">The new environment variable value.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission to perform this operation.</exception>
        public static void SetProperty(string key, string value)
        {
            IConfiguration configuration = ConfigurationSettings.GetConfigurationFactory().CreateConfiguration();
            configuration[key] = value;
        }
    }
#endif

}