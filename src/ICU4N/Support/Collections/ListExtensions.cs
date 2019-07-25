using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Collections
{
    public static class ListExtensions
    {
        /// <summary>
        /// Copies a range of elements from an <see cref="IList{T}"/> starting at the specified source index and pastes them to another <see cref="IList{T}"/> starting at the specified 
        /// destination index. The length and the indexes are specified as 32-bit integers.
        /// </summary>
        /// <typeparam name="T">The type of elements to copy.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> that contains the data to copy.</param>
        /// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceIndex"/> at which copying begins.</param>
        /// <param name="destination">The <see cref="IList{T}"/> that receives the data.</param>
        /// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destination"/> at which storing begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
        public static void CopyTo<T>(this IList<T> list, int sourceIndex, IList<T> destination, int destinationIndex, int length)
        {
            if (sourceIndex < 0 || sourceIndex > list.Count || destinationIndex < 0 || length < 0 || destinationIndex + length > destination.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            for (int i = sourceIndex, j = 0; j < length; i++, j++)
            {
                destination[j + destinationIndex] = list[i];
            }
        }

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
