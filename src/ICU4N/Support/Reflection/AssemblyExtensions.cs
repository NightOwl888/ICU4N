using System.Linq;
using System.Reflection;

namespace ICU4N.Reflection
{
    internal static class AssemblyExtensions
    {
        // Support for .NET 4.0
#if NET40
        public static T GetCustomAttribute<T>(this Assembly assembly)
        {
            return (T)assembly.GetCustomAttributes(inherit: true).FirstOrDefault(a => a.GetType() is T);
        }
#endif
    }
}
