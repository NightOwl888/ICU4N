using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Collections
{
    public static class ArrayExtensions
    {
        public static T[] CopyOf<T>(this T[] original, int newLength)
        {
            T[] newArray = new T[newLength];

            for (int i = 0; i < Math.Min(original.Length, newLength); i++)
            {
                newArray[i] = original[i];
            }

            return newArray;
        }
    }
}
