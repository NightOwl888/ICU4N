using J2N.Collections.Generic.Extensions;
using J2N.Collections.ObjectModel;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Support.Collections
{
    internal class Collection
    {
        private static class EmptyListHolder<T>
        {
            public static readonly ReadOnlyList<T> EMPTY_LIST = new JCG.List<T>().AsReadOnly();
        }

        private static class EmptyDictionaryHolder<TKey, TValue>
        {
            public static readonly ReadOnlyDictionary<TKey, TValue> EMPTY_DICTIONARY = new JCG.Dictionary<TKey, TValue>().AsReadOnly();
        }

        private static class EmptySetHolder<T>
        {
            public static readonly ReadOnlySet<T> EMPTY_SET = new JCG.HashSet<T>().AsReadOnly();
        }

        public static ReadOnlyList<T> EmptyList<T>()
        {
            return EmptyListHolder<T>.EMPTY_LIST; // ICU4N NOTE: Enumerable.Empty<T>() fails to cast to IList<T> on .NET Core 3.x, so we just create a new list
        }

        public static ReadOnlyDictionary<TKey, TValue> EmptyDictionary<TKey, TValue>()
        {
            return EmptyDictionaryHolder<TKey, TValue>.EMPTY_DICTIONARY;
        }

        public static ReadOnlySet<T> EmptySet<T>()
        {
            return EmptySetHolder<T>.EMPTY_SET;
        }
    }
}
