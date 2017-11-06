using System;
using System.Collections.Generic;
using System.Reflection;

namespace ICU4N.Support.Collections
{
    public static class GenericComparer
    {
        /// <summary>
        /// Get the natural <see cref="IComparer{T}"/> for the provided object class.
        /// <para/>
        /// The comparer returned depends on the <typeparam name="T"/> argument:
        /// <list type="number">
        ///     <item><description>If the type is <see cref="string"/>, the comparer returned uses
        ///         the <see cref="string.CompareOrdinal(string, string)"/> to make the comparison
        ///         to ensure that the current culture doesn't affect the results. This is the
        ///         default string comparison used in Java.</description></item>
        ///     <item><description>If the type implements <see cref="IComparable{T}"/>, the comparer uses
        ///         <see cref="IComparable{T}.CompareTo(T)"/> for the comparison. This allows
        ///         the use of types with custom comparison schemes.</description></item>
        ///     <item><description>If neither of the above conditions are true, will default to <see cref="Comparer{T}.Default"/>.</description></item>
        /// </list>
        /// <para/>
        /// </summary>
        public static IComparer<T> NaturalComparer<T>()
        {
            Type genericClosingType = typeof(T);

            // we need to ensure that strings are compared
            // in a culture-insenitive manner.
            if (genericClosingType.Equals(typeof(string)))
            {
                return (IComparer<T>)StringComparer.Ordinal;
            }
            // Only return the NaturalComparer if the type
            // implements IComparable<T>, otherwise use Comparer<T>.Default.
            // This allows the comparison to be customized, but it is not mandatory
            // to implement IComparable<T>.
            else if (typeof(IComparable<T>).GetTypeInfo().IsAssignableFrom(genericClosingType.GetTypeInfo()))
            {
                return new NaturalComparerImpl<T>();
            }

            return Comparer<T>.Default;
        }

        // we don't have an IComparable<T> constraint -
        // the logic of GetNaturalComparer<T> handles that so we just
        // do a cast here.
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        private class NaturalComparerImpl<T> : IComparer<T> //where T : IComparable<T>
        {
            internal NaturalComparerImpl()
            {
            }

            public virtual int Compare(T o1, T o2)
            {
                return ((IComparable<T>)o1).CompareTo(o2);
            }
        }
    }
}
