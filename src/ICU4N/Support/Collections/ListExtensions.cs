using System;
using System.Collections.Generic;

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
    }
}
