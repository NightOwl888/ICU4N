using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ICU4N.Support
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Aggressively searches for a resource and, if found, returns an open <see cref="Stream"/>
        /// where it can be read.
        /// </summary>
        /// <param name="type">a type in the same namespace as the resource</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>an open <see cref="Stream"/> that can be used to read the resource, or <c>null</c> if the resource cannot be found.</returns>
        public static Stream FindAndGetManifestResourceStream(this Type type, string name)
        {
            return type.GetTypeInfo().Assembly.FindAndGetManifestResourceStream(type, name);
        }

        /// <summary>
        /// Aggressively searches to find a resource based on a <see cref="Type"/> and resource name.
        /// </summary>
        /// <param name="type">a type in the same namespace as the resource</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>the resource, if found; if not found, returns <c>null</c></returns>
        public static string FindResource(this Type type, string name)
        {
            return type.GetTypeInfo().Assembly.FindResource(type, name);
        }

        public static bool ImplementsGenericInterface(this Type target, Type interfaceType)
        {
            return target.GetTypeInfo().IsGenericType && target.GetGenericTypeDefinition().GetInterfaces().Any(
                x => x.GetTypeInfo().IsGenericType && interfaceType.IsAssignableFrom(x.GetGenericTypeDefinition())
            );
        }
    }
}
