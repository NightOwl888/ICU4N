// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NET40
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;

namespace ICU4N.Configuration
{
    /// <summary>
    /// An environment variable based <see cref="ConfigurationProvider"/>.
    /// </summary>
    internal class ICU4NDefaultConfigurationProvider : IConfigurationProvider
    {
        private readonly bool ignoreSecurityExceptionsOnRead;
        private readonly string _prefix;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ICU4NDefaultConfigurationProvider(bool ignoreSecurityExceptionsOnRead) : this(string.Empty, ignoreSecurityExceptionsOnRead)
        { }

        /// <summary>
        /// Initializes a new instance with the specified prefix.
        /// </summary>
        /// <param name="prefix">A prefix used to filter the environment variables.</param>
        public ICU4NDefaultConfigurationProvider(string prefix, bool ignoreSecurityExceptionsOnRead = true)
        {
            _prefix = prefix ?? string.Empty;
            this.ignoreSecurityExceptionsOnRead = ignoreSecurityExceptionsOnRead;
        }

        /// <summary>
        /// Loads the environment variables.
        /// </summary>
        public void Load()
        {
            Data = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// The configuration key value pairs for this provider.
        /// </summary>
        protected ConcurrentDictionary<string, string> Data { get; set; }

        public virtual bool TryGet(string key, out string value)
        {
            value = Data.GetOrAdd(key, (x) =>
            {
                if (ignoreSecurityExceptionsOnRead)
                {
                    try
                    {
                        return Environment.GetEnvironmentVariable(key);
                    }
                    catch (SecurityException)
                    {
                        return null;
                    }
                }
                else
                {
                    return Environment.GetEnvironmentVariable(key);
                }
            });
            return (!string.IsNullOrEmpty(value));
        }


        /// <summary>
        /// Sets a value for a given key.
        /// </summary>
        /// <param name="key">The configuration key to set.</param>
        /// <param name="value">The value to set.</param>
        public virtual void Set(string key, string value)
            => Data[key] = value;
        /// <summary>
        /// Returns the list of keys that this provider has.
        /// </summary>
        /// <param name="earlierKeys">The earlier keys that other providers contain.</param>
        /// <param name="parentPath">The path for the parent IConfiguration.</param>
        /// <returns>The list of keys for this provider.</returns>
        public virtual IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            return Data
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(kv => Segment(kv.Key, prefix.Length))
                .Concat(earlierKeys)
                .OrderBy(k => k);
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        private IChangeToken _reloadToken = new ConfigurationReloadToken();

        /// <summary>
        /// Returns a <see cref="IChangeToken"/> that can be used to listen when this provider is reloaded.
        /// </summary>
        /// <returns></returns>
        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }
    }
}

#endif