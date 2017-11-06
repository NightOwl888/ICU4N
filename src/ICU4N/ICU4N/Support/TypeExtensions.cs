using System;
using System.Linq;
using System.Reflection;

namespace ICU4N.Support
{
    public static class TypeExtensions
    {
        public static bool ImplementsGenericInterface(this Type target, Type interfaceType)
        {
            return target.GetTypeInfo().IsGenericType && target.GetGenericTypeDefinition().GetInterfaces().Any(
                x => x.GetTypeInfo().IsGenericType && interfaceType.IsAssignableFrom(x.GetGenericTypeDefinition())
            );
        }
    }
}
