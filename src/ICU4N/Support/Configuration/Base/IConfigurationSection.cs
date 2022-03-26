// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !FEATURE_MICROSOFT_EXTENSIONS_CONFIGURATION

namespace Microsoft.Extensions.Configuration
{
    internal interface IConfigurationSection : IConfiguration
    {
        //
        // Summary:
        //     Gets the key this section occupies in its parent.
        string Key { get; }
        //
        // Summary:
        //     Gets the full path to this section within the Microsoft.Extensions.Configuration.IConfiguration.
        string Path { get; }
        //
        // Summary:
        //     Gets or sets the section value.
        string Value { get; set; }
    }
}
#endif
