// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !FEATURE_MICROSOFT_EXTENSIONS_CONFIGURATION
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration
{
    internal interface IConfigurationProvider
    {
        //
        // Summary:
        //     Returns the immediate descendant configuration keys for a given parent path based
        //     on this Microsoft.Extensions.Configuration.IConfigurationProvider's data and
        //     the set of keys returned by all the preceding Microsoft.Extensions.Configuration.IConfigurationProviders.
        //
        // Parameters:
        //   earlierKeys:
        //     The child keys returned by the preceding providers for the same parent path.
        //
        //   parentPath:
        //     The parent path.
        //
        // Returns:
        //     The child keys.
        IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath);
        //
        // Summary:
        //     Returns a change token if this provider supports change tracking, null otherwise.
        IChangeToken GetReloadToken();
        //
        // Summary:
        //     Loads configuration values from the source represented by this Microsoft.Extensions.Configuration.IConfigurationProvider.
        void Load();
        //
        // Summary:
        //     Sets a configuration value for the specified key.
        //
        // Parameters:
        //   key:
        //     The key.
        //
        //   value:
        //     The value.
        void Set(string key, string value);
        //
        // Summary:
        //     Tries to get a configuration value for the specified key.
        //
        // Parameters:
        //   key:
        //     The key.
        //
        //   value:
        //     The value.
        //
        // Returns:
        //     True if a value for the specified key was found, otherwise false.
        bool TryGet(string key, out string value);
    }
}
#endif
