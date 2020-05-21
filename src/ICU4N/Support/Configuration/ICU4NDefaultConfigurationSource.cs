// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NET40
using Microsoft.Extensions.Configuration;

namespace ICU4N.Configuration
{
    /// <summary>
    /// Represents environment variables as an <see cref="IConfigurationSource"/>.
    /// </summary>
    internal class ICU4NDefaultConfigurationSource : IConfigurationSource
    {

        /// <summary>
        /// A prefix used to filter environment variables.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Set to true by default - used to prevent any security exceptions thrown when reading environment variables
        /// </summary>
        public bool IgnoreSecurityExceptionsOnRead { get; set; }

        /// <summary>
        /// Builds the <see cref="LuceneDefaultConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="LuceneDefaultConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ICU4NDefaultConfigurationProvider(Prefix, IgnoreSecurityExceptionsOnRead);
        }
    }
}
#endif