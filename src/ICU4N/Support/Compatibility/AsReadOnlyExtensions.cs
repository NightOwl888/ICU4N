using JCG = J2N.Collections.Generic;
using SCG = System.Collections.Generic;

namespace ICU4N
{
    /// <summary>
    /// Since both .NET and J2N supply <c>AsReadOnly()</c> extension methods with the same signature for collections,
    /// they cause ambiguous matches when importing both namespaces. This class is to resolve which of
    /// those methods to use when both are available.
    /// </summary>
    internal static class AsReadOnlyExtensions
    {
        public static SCG.ISet<T> AsReadOnly<T>(this SCG.ISet<T> set)
        {
#if FEATURE_ISET_ASREADONLY
            return SCG.CollectionExtensions.AsReadOnly(set);
#else
            return JCG.Extensions.SetExtensions.AsReadOnly(set);
#endif
        }

        public static SCG.IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary)
        {
#if FEATURE_IDICTIONARY_ASREADONLY
            return SCG.CollectionExtensions.AsReadOnly(dictionary);
#else
            return JCG.Extensions.DictionaryExtensions.AsReadOnly(dictionary);
#endif
        }

        public static SCG.IList<T> AsReadOnly<T>(this SCG.IList<T> list)
        {
#if FEATURE_ILIST_ASREADONLY
            return SCG.CollectionExtensions.AsReadOnly(list);
#else
            return JCG.Extensions.ListExtensions.AsReadOnly(list);
#endif
        }

        public static SCG.ICollection<T> AsReadOnly<T>(this SCG.ICollection<T> collection)
        {
            return JCG.Extensions.CollectionExtensions.AsReadOnly(collection);
        }
    }
}
