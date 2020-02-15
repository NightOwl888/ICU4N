using System;
using System.Collections.Generic;

namespace ICU4N.Support
{
    internal class AnonymousComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> compare;

        public AnonymousComparer(Func<T, T, int> compare)
        {
            this.compare = compare ?? throw new ArgumentNullException(nameof(compare));
        }

        public int Compare(T x, T y)
        {
            return compare(x, y);
        }
    }
}
