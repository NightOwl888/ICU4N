using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ICU4N.Support
{
    public static class AssemblyExtensions
    {
        private static ConcurrentDictionary<TypeAndResource, string> resourceCache = new ConcurrentDictionary<TypeAndResource, string>();

        /// <summary>
        /// Uses the assembly name + '.' + suffix to determine whether any resources begin with the concatenation.
        /// If not, the assembly name will be truncated at the '.' beginning from the right side of the string
        /// until a base name is found.
        /// </summary>
        /// <param name="assembly">This <see cref="Assembly"/>.</param>
        /// <param name="suffix">A suffix to use on the assembly name to limit the possible resource names to match. 
        /// This value can be null to match any resource name in the assembly.</param>
        /// <returns>A base name if found, otherwise <see cref="string.Empty"/>.</returns>
        public static string GetManifestResourceBaseName(this Assembly assembly, string suffix)
        {
            var resourceNames = assembly.GetManifestResourceNames();
            string assemblyName = assembly.GetName().Name;
            string baseName = string.IsNullOrEmpty(suffix) ? assemblyName : assemblyName + '.' + suffix;
            int dotIndex = -1;
            do
            {
                if (resourceNames.Any(resName => resName.StartsWith(baseName, StringComparison.Ordinal)))
                {
                    return baseName;
                }

                dotIndex = assemblyName.LastIndexOf('.');
                if (dotIndex > -1 && dotIndex < assemblyName.Length - 1)
                {
                    assemblyName = assemblyName.Substring(0, dotIndex);
                    baseName = string.IsNullOrEmpty(suffix) ? assemblyName : assemblyName + '.' + suffix;
                }
            } while (dotIndex > -1);

            // No match
            return string.Empty;
        }

        /// <summary>
        /// Aggressively searches for a resource and, if found, returns an open <see cref="Stream"/>
        /// where it can be read.
        /// </summary>
        /// <param name="assembly">this assembly</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>an open <see cref="Stream"/> that can be used to read the resource, or <c>null</c> if the resource cannot be found.</returns>
        public static Stream FindAndGetManifestResourceStream(this Assembly assembly, string name)
        {
            string resourceName = FindResource(assembly, name);
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Aggressively searches for a resource and, if found, returns an open <see cref="Stream"/>
        /// where it can be read.
        /// </summary>
        /// <param name="assembly">this assembly</param>
        /// <param name="type">a type in the same namespace as the resource</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>an open <see cref="Stream"/> that can be used to read the resource, or <c>null</c> if the resource cannot be found.</returns>
        public static Stream FindAndGetManifestResourceStream(this Assembly assembly, Type type, string name)
        {
            string resourceName = FindResource(assembly, type, name);
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Aggressively searches to find a resource based on a resource name.
        /// Attempts to find the resource file by prepending the assembly name
        /// with the resource name and then removing the segements of the
        /// name from left to right until a match is found.
        /// </summary>
        /// <param name="assembly">this assembly</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>the resource, if found; if not found, returns <c>null</c></returns>
        public static string FindResource(this Assembly assembly, string name)
        {
            name = ConvertResourceName(name);
            string resourceName;
            TypeAndResource key = new TypeAndResource(null, name);
            if (!resourceCache.TryGetValue(key, out resourceName))
            {
                string[] resourceNames = assembly.GetManifestResourceNames();
                resourceName = resourceNames.Where(x => x.Equals(name)).FirstOrDefault();

                // If resourceName is not null, we have an exact match, don't search
                if (resourceName == null)
                {
                    string assemblyName = assembly.GetName().Name;
                    int lastDot = assemblyName.LastIndexOf('.');
                    do
                    {
                        // Search by assembly name only
                        string resourceToFind = string.Concat(assemblyName, ".", name);
                        TryFindResource(resourceNames, null, resourceToFind, name, out resourceName);

                        // Continue searching by removing sections after the . from the assembly name
                        // until we have a match.
                        lastDot = assemblyName.LastIndexOf('.');
                        assemblyName = lastDot >= 0 ? assemblyName.Substring(0, lastDot) : null;

                    } while (assemblyName != null && resourceName == null);

                    if (resourceName == null)
                    {
                        // Try again without using the assembly name
                        TryFindResource(resourceNames, null, name, name, out resourceName);
                    }
                }

                resourceCache[key] = resourceName;
            }

            return resourceName;
        }

        /// <summary>
        /// Aggressively searches to find a resource based on a <see cref="Type"/> and resource name.
        /// </summary>
        /// <param name="assembly">this assembly</param>
        /// <param name="type">a type in the same namespace as the resource</param>
        /// <param name="name">the resource name to locate</param>
        /// <returns>the resource, if found; if not found, returns <c>null</c></returns>
        public static string FindResource(this Assembly assembly, Type type, string name)
        {
            name = ConvertResourceName(name);
            string resourceName;
            TypeAndResource key = new TypeAndResource(type, name);
            if (!resourceCache.TryGetValue(key, out resourceName))
            {
                string[] resourceNames = assembly.GetManifestResourceNames();
                resourceName = resourceNames.Where(x => x.Equals(name)).FirstOrDefault();

                // If resourceName is not null, we have an exact match, don't search
                if (resourceName == null)
                {
                    string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
                    string namespaceName = type.GetTypeInfo().Namespace;

                    // Search by assembly + namespace
                    string resourceToFind = string.Concat(namespaceName, ".", name);
                    if (!TryFindResource(resourceNames, assemblyName, resourceToFind, name, out resourceName))
                    {
                        string found1 = resourceName;

                        // Search by namespace only
                        if (!TryFindResource(resourceNames, null, resourceToFind, name, out resourceName))
                        {
                            string found2 = resourceName;

                            // Search by assembly name only
                            resourceToFind = string.Concat(assemblyName, ".", name);
                            if (!TryFindResource(resourceNames, null, resourceToFind, name, out resourceName))
                            {
                                // Take the first match of multiple, if there are any
                                resourceName = found1 ?? found2 ?? resourceName;
                            }
                        }
                    }
                }

                resourceCache[key] = resourceName;
            }

            return resourceName;
        }

        /// <summary>
        /// Change from JDK-style resource path (Impl/Data/icudt60b/brkitr) to .NET style (Impl.Data.brkitr).
        /// This method also removes the version number from the path so we don't have to change it internally
        /// from one release to the next.
        /// </summary>
        public static string ConvertResourceName(string name)
        {
            return name.Replace('/', '.').Replace("." + ICU4N.Impl.ICUData.PackageName, "");
        }

        private static bool TryFindResource(string[] resourceNames, string prefix, string resourceName, string exactResourceName, out string result)
        {
            if (!resourceNames.Contains(resourceName))
            {
                string nameToFind = null;
                while (resourceName.Length > 0 && resourceName.Contains('.') && (!(string.IsNullOrEmpty(prefix)) || resourceName.Equals(exactResourceName)))
                {
                    nameToFind = string.IsNullOrEmpty(prefix)
                        ? resourceName
                        : string.Concat(prefix, ".", resourceName);
                    string[] matches = resourceNames.Where(x => x.EndsWith(nameToFind, StringComparison.Ordinal)).ToArray();
                    if (matches.Length == 1)
                    {
                        result = matches[0]; // Exact match
                        return true;
                    }
                    else if (matches.Length > 1)
                    {
                        result = matches[0]; // First of many
                        return false;
                    }

                    resourceName = resourceName.Substring(resourceName.IndexOf('.') + 1);
                }
                result = null; // No match
                return false;
            }

            result = resourceName;
            return true;
        }

        private class TypeAndResource
        {
            private readonly Type type;
            private readonly string name;

            public TypeAndResource(Type type, string name)
            {
                this.type = type;
                this.name = name;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeAndResource))
                {
                    return false;
                }

                var other = obj as TypeAndResource;
                if (type != null)
                {
                    return this.type.Equals(other.type)
                        && this.name.Equals(other.name);
                }

                return this.name.Equals(other.name);
            }

            public override int GetHashCode()
            {
                if (type != null)
                {
                    return this.type.GetHashCode() ^ this.name.GetHashCode();
                }

                return this.name.GetHashCode();
            }
        }
    }
}
