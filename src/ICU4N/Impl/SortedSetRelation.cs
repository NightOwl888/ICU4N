using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;

namespace ICU4N.Impl
{
    /// <summary>
    /// Relations that correspond to the filters in <see cref="SortedSetRelation.HasRelation{T}(SortedSet{T}, SortedSetOperation, SortedSet{T})"/>.
    /// </summary>
    public enum SortedSetFilter
    {
        Any = SortedSetRelation.ANY,
        Contains = SortedSetRelation.CONTAINS,
        Disjoint = SortedSetRelation.DISJOINT,
        IsContained = SortedSetRelation.ISCONTAINED,
        No_B = SortedSetRelation.NO_B,
        Equals = SortedSetRelation.A_AND_B,
        No_A = SortedSetRelation.NO_A,
        None = SortedSetRelation.NONE,
    }

    /// <summary>
    /// Relations that correspond to set operations in <see cref="SortedSetRelation.DoOperation{T}(SortedSet{T}, SortedSetOperation, SortedSet{T})"/>.
    /// </summary>
    public enum SortedSetOperation
    {
        None = SortedSetRelation.NONE,
        UnionWith = SortedSetRelation.ANY,
        A = SortedSetRelation.CONTAINS,
        SymmetricExceptWith = SortedSetRelation.DISJOINT,
        B = SortedSetRelation.ISCONTAINED,
        ExceptWith = SortedSetRelation.NO_B,
        IntersectWith = SortedSetRelation.EQUALS,
        B_ExceptWith = SortedSetRelation.NO_A,
    }

    /// <summary>
    /// Computationally efficient determination of the relationship between
    /// two SortedSets.
    /// </summary>
    public static class SortedSetRelation // ICU4N specific - made class static, since it has no instance members
    {
        /// <summary>
        /// The relationship between two sets A and B can be determined by looking at:
        /// <list type="bullet">
        ///     <item><description>A - B</description></item>
        ///     <item><description>A &amp; B (intersection)</description></item>
        ///     <item><description>B - A</description></item>
        /// </list>
        /// These are represented by a set of bits.
        /// <list type="bullet">
        ///     <item><description>Bit 2 is true if A - B is not empty</description></item>
        ///     <item><description>Bit 1 is true if A &amp; B is not empty</description></item>
        ///     <item><description>BIT 0 is true if B - A is not empty</description></item>
        /// </list>
        /// </summary>
        internal const int // ICU4N specific - using SortedSetOperation enum, but left constants in for compatibility. Made internal instead of public.
            A_NOT_B = 4,
            A_AND_B = 2,
            B_NOT_A = 1;

        /// <summary>
        /// There are 8 combinations of the relationship bits. These correspond to
        /// the filters (combinations of allowed bits) in <see cref="HasRelation{T}(SortedSet{T}, SortedSetOperation, SortedSet{T})"/>. They also
        /// correspond to the modification functions, listed in comments.
        /// </summary>
        internal const int // ICU4N specific - using SortedSetOperation enum, but left constants in for compatibility. Made internal instead of public.
            ANY = A_NOT_B | A_AND_B | B_NOT_A,           // union,           addAll
            CONTAINS = A_NOT_B | A_AND_B,                // A                (unnecessary)
            DISJOINT = A_NOT_B | B_NOT_A,                // A xor B,         missing Java function
            ISCONTAINED = A_AND_B | B_NOT_A,             // B                (unnecessary)
            NO_B = A_NOT_B,                              // A setDiff B,     removeAll
            EQUALS = A_AND_B,                            // A intersect B,   retainAll
            NO_A = B_NOT_A,                              // B setDiff A,     removeAll
            NONE = 0,                                    // null             (unnecessary)

            ADDALL = ANY,                // union,           addAll
            A = CONTAINS,                // A                (unnecessary)
            COMPLEMENTALL = DISJOINT,    // A xor B,         missing Java function
            B = ISCONTAINED,             // B                (unnecessary)
            REMOVEALL = NO_B,            // A setDiff B,     removeAll
            RETAINALL = EQUALS,          // A intersect B,   retainAll
            B_REMOVEALL = NO_A;          // B setDiff A,     removeAll

        /// <summary>
        /// Utility that could be on SortedSet. Faster implementation than
        /// what is in .NET for doing contains, equals, etc.
        /// </summary>
        /// <typeparam name="T">Type of element. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="a">First set.</param>
        /// <param name="filter">Filter, using <see cref="SortedSetFilter"/>.</param>
        /// <param name="b">Second set.</param>
        /// <returns>Whether the filter relationship is true or not.</returns>
        public static bool HasRelation<T>(SortedSet<T> a, SortedSetFilter filter, SortedSet<T> b) where T : IComparable<T>
        {
            int allow = (int)filter;
            if (allow < NONE || allow > ANY)
            {
                throw new ArgumentException("Relation " + allow + " out of range");
            }

            // extract filter conditions
            // these are the ALLOWED conditions Set

            bool anb = (allow & A_NOT_B) != 0;
            bool ab = (allow & A_AND_B) != 0;
            bool bna = (allow & B_NOT_A) != 0;

            // quick check on sizes
            switch (allow)
            {
                case CONTAINS: if (a.Count < b.Count) return false; break;
                case ISCONTAINED: if (a.Count > b.Count) return false; break;
                case EQUALS: if (a.Count != b.Count) return false; break;
            }

            // check for null sets
            if (a.Count == 0)
            {
                if (b.Count == 0) return true;
                return bna;
            }
            else if (b.Count == 0)
            {
                return anb;
            }

            // pick up first strings, and start comparing
            using (var ait = a.GetEnumerator())
            using (var bit = b.GetEnumerator())
            {
                ait.MoveNext();
                bit.MoveNext();

                T aa = ait.Current;
                T bb = bit.Current;

                while (true)
                {
                    int comp = aa.CompareTo(bb);
                    if (comp == 0)
                    {
                        if (!ab) return false;
                        if (!ait.MoveNext())
                        {
                            if (!bit.MoveNext()) return true;
                            return bna;
                        }
                        else if (!bit.MoveNext())
                        {
                            return anb;
                        }
                        aa = ait.Current;
                        bb = bit.Current;
                    }
                    else if (comp < 0)
                    {
                        if (!anb) return false;
                        if (!ait.MoveNext())
                        {
                            return bna;
                        }
                        aa = ait.Current;
                    }
                    else
                    {
                        if (!bna) return false;
                        if (!bit.MoveNext())
                        {
                            return anb;
                        }
                        bb = bit.Current;
                    }
                }
            }
        }

        /// <summary>
        /// Utility that could be on SortedSet. Allows faster implementation than
        /// what is in .NET for doing UnionWith, ExceptWith, IntersectWith, (complementAll).
        /// </summary>
        /// <typeparam name="T">Type of element. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="a">First set.</param>
        /// <param name="relation">The relation filter, using <see cref="SortedSetOperation"/>.</param>
        /// <param name="b">Second set.</param>
        /// <returns>The new set.</returns>
        public static SortedSet<T> DoOperation<T>(SortedSet<T> a, SortedSetOperation relation, SortedSet<T> b) where T : IComparable<T>
        {
            // TODO: optimize this as above
            SortedSet<T> temp;
            switch (relation)
            {
                case SortedSetOperation.UnionWith:
                    a.UnionWith(b);
                    return a;
                case SortedSetOperation.A:
                    return a; // no action
                case SortedSetOperation.B:
                    a.Clear();
                    a.UnionWith(b);
                    return a;
                case SortedSetOperation.ExceptWith:
                    a.ExceptWith(b);
                    return a;
                case SortedSetOperation.IntersectWith:
                    a.IntersectWith(b);
                    return a;
                case SortedSetOperation.SymmetricExceptWith:
                    a.SymmetricExceptWith(b);
                    return a;
                // the following is the only case not really supported by .NET
                case SortedSetOperation.B_ExceptWith:
                    temp = new SortedSet<T>(b, GenericComparer.NaturalComparer<T>());
                    temp.ExceptWith(a);
                    a.Clear();
                    a.UnionWith(temp);
                    return a;
                case SortedSetOperation.None:
                    a.Clear();
                    return a;
                default:
                    throw new ArgumentException("Relation " + relation + " out of range");
            }
        }
    }
}
