using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
