using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Collections
{
    public static class SetExtensions
    {
        //public static T Get<T>(this ISet<T> set, int index)
        //{
        //    if (index > -1 && index < set.Count)
        //        return set[index];
        //    return default(T);
        //}

        public static ISet<T> ToUnmodifiableSet<T>(this ISet<T> set)
        {
            return new UnmodifiableSet<T>(set);
        }

        #region UnmodifiableSet

        private class UnmodifiableSet<T> : ISet<T>
        {
            private readonly ISet<T> set;
            public UnmodifiableSet(ISet<T> set)
            {
                if (set == null)
                    throw new ArgumentNullException("set");
                this.set = set;
            }
            public int Count
            {
                get
                {
                    return set.Count;
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
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public void Clear()
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public bool Contains(T item)
            {
                return set.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                set.CopyTo(array, arrayIndex);
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public IEnumerator<T> GetEnumerator()
            {
                return set.GetEnumerator();
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                return set.IsProperSubsetOf(other);
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return set.IsProperSupersetOf(other);
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return set.IsSubsetOf(other);
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return set.IsSupersetOf(other);
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                return set.Overlaps(other);
            }

            public bool Remove(T item)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                return set.SetEquals(other);
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            public void UnionWith(IEnumerable<T> other)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            bool ISet<T>.Add(T item)
            {
                throw new InvalidOperationException("Unable to modify this set.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return set.GetEnumerator();
            }
        }

        #endregion
    }
}
