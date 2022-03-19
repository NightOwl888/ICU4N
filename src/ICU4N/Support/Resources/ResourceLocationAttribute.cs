using ICU4N.Impl;
using System;

namespace ICU4N.Resources
{
    /// <summary>
    /// Informs the <see cref="ICUData"/> where to locate any embedded resource files
    /// for the given assembly. If not provided, it is assumed they are in the main assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ResourceLocationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLocationAttribute"/> class.
        /// </summary>
        /// <param name="location">One of the enumeration values that indicates the location from which to retrieve embedded resource files.</param>
        public ResourceLocationAttribute(ResourceLocation location)
        {
            Location = location;
        }

        /// <summary>
        /// Gets the location for the <see cref="ICUData"/> class to use to retrieve the embedded resource files.
        /// </summary>
        public ResourceLocation Location { get; private set; }
    }
}
