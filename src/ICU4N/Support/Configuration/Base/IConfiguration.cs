// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !FEATURE_MICROSOFT_EXTENSIONS_CONFIGURATION
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration
{
    internal interface IConfiguration
    {
        //
        // Summary:
        //     Gets or sets a configuration value.
        //
        // Parameters:
        //   key:
        //     The configuration key.
        //
        // Returns:
        //     The configuration value.
        string this[string key] { get; set; }

        //
        // Summary:
        //     Gets the immediate descendant configuration sub-sections.
        //
        // Returns:
        //     The configuration sub-sections.
        IEnumerable<IConfigurationSection> GetChildren();
        //
        // Summary:
        //     Returns a Microsoft.Extensions.Primitives.IChangeToken that can be used to observe
        //     when this configuration is reloaded.
        //
        // Returns:
        //     A Microsoft.Extensions.Primitives.IChangeToken.
        IChangeToken GetReloadToken();
        //
        // Summary:
        //     Gets a configuration sub-section with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the configuration section.
        //
        // Returns:
        //     The Microsoft.Extensions.Configuration.IConfigurationSection.
        //
        // Remarks:
        //     This method will never return null. If no matching sub-section is found with
        //     the specified key, an empty Microsoft.Extensions.Configuration.IConfigurationSection
        //     will be returned.
        IConfigurationSection GetSection(string key);
    }
}
#endif