using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Support.Collections
{
    internal static class Arrays
    {
        /// <summary>
        /// This is the same implementation of ToString from Java's AbstractCollection
        /// (the default implementation for all sets and lists)
        /// </summary>
        public static string ToString<T>(T[] array)
        {
            return CollectionUtil.ToString((IList<T>)array);
        }

        /// <summary>
        /// This is the same implementation of ToString from Java's AbstractCollection
        /// (the default implementation for all sets and lists), plus the ability
        /// to specify culture for formatting of nested numbers and dates. Note that
        /// this overload will change the culture of the current thread.
        /// </summary>
        public static string ToString<T>(T array, CultureInfo culture)
        {
            return CollectionUtil.ToString((IList<T>)array, culture);
        }

        /// <summary>
        /// Assigns the specified value to each element of the specified array.
        /// </summary>
        /// <typeparam name="T">the type of the array</typeparam>
        /// <param name="a">the array to be filled</param>
        /// <param name="val">the value to be stored in all elements of the array</param>
        public static void Fill<T>(T[] a, T val)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = val;
            }
        }

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
    }
}
