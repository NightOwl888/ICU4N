using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Collections
{
    public static class ListExtensions
    {
        public static IList<T> ToUnmodifiableList<T>(this IList<T> list)
        {
            return new UnmodifiableList<T>(list);
        }

        #region UnmodifiableList

        private class UnmodifiableList<T> : IList<T>
        {
            private readonly IList<T> list;

            public UnmodifiableList(IList<T> list)
            {
                if (list == null)
                    throw new ArgumentNullException("list");
                this.list = list;
            }

            public T this[int index]
            {
                get
                {
                    return list[index];
                }
                set
                {
                    throw new InvalidOperationException("Unable to modify this list.");
                }
            }

            public int Count
            {
                get
                {
                    return list.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public void Add(T item)
            {
                throw new InvalidOperationException("Unable to modify this list.");
            }

            public void Clear()
            {
                throw new InvalidOperationException("Unable to modify this list.");
            }

            public bool Contains(T item)
            {
                return list.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                list.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return list.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                throw new InvalidOperationException("Unable to modify this list.");
            }

            public bool Remove(T item)
            {
                throw new InvalidOperationException("Unable to modify this list.");
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException("Unable to modify this list.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }
        }

        #endregion UnmodifiableListImpl
    }
}
