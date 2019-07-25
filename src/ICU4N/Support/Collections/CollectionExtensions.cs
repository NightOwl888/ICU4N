using System;
using System.Collections;
using System.Collections.Generic;

namespace ICU4N.Support.Collections
{
    public static class CollectionExtensions
    {
        public static ICollection<T> ToUnmodifiableCollection<T>(this ICollection<T> collection)
        {
            return new UnmodifiableCollection<T>(collection);
        }

        #region UnmodifiableCollection

        private class UnmodifiableCollection<T> : ICollection<T>
        {
            private readonly ICollection<T> collection;

            public UnmodifiableCollection(ICollection<T> collection)
            {
                this.collection = collection;
            }

            public int Count
            {
                get { return collection.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add(T item)
            {
                throw new NotSupportedException("Unable to modifiy this collection.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Unable to modifiy this collection.");
            }

            public bool Contains(T item)
            {
                return collection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                collection.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return collection.GetEnumerator();
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException("Unable to modifiy this collection.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return collection.GetEnumerator();
            }
        }

        #endregion
    }
}
