using System;
using System.Runtime.CompilerServices;

namespace ICU4N.Support.Collections
{
    internal static class Arrays
    {
        public static T[][] NewRectangularArray<T>(int size1, int size2)
        {
            T[][] array;
            if (size1 > -1)
            {
                array = new T[size1][];
                if (size2 > -1)
                {
                    for (int array1 = 0; array1 < size1; array1++)
                    {
                        array[array1] = new T[size2];
                    }
                }
            }
            else
                array = null;

            return array;
        }

        /// <summary>
        /// Returns an empty array.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <returns>An empty array.</returns>
        // ICU4N: Since Array.Empty<T>() doesn't exist in all supported platforms, we
        // have this wrapper method to add support.
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        public static T[] Empty<T>()
        {
#if FEATURE_ARRAYEMPTY
            return Array.Empty<T>();
#else
            return EmptyArrayHolder<T>.Empty;
#endif
        }

#if !FEATURE_ARRAYEMPTY
        private static class EmptyArrayHolder<T>
        {
            public static readonly T[] Empty = new T[0];
        }
#endif
    }
}
