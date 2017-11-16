using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    public class AnonymousComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> compare;

        public AnonymousComparer(Func<T, T, int> compare)
        {
            if (compare == null)
                throw new ArgumentNullException(nameof(compare));
            this.compare = compare;
        }

        public int Compare(T x, T y)
        {
            return compare(x, y);
        }
    }
}
