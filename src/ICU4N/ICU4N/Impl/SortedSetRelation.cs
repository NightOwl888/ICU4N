using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Computationally efficient determination of the relationship between
    /// two SortedSets.
    /// </summary>
    public class SortedSetRelation
    {
        /**
         * The relationship between two sets A and B can be determined by looking at:
         * A - B
         * A & B (intersection)
         * B - A
         * These are represented by a set of bits.
         * Bit 2 is true if A - B is not empty
         * Bit 1 is true if A & B is not empty
         * BIT 0 is true if B - A is not empty
         */
        public const int
            A_NOT_B = 4,
            A_AND_B = 2,
            B_NOT_A = 1;

        /**
         * There are 8 combinations of the relationship bits. These correspond to
         * the filters (combinations of allowed bits) in hasRelation. They also
         * correspond to the modification functions, listed in comments.
         */
        public const int
           ANY = A_NOT_B | A_AND_B | B_NOT_A,    // union,           addAll
           CONTAINS = A_NOT_B | A_AND_B,                // A                (unnecessary)
           DISJOINT = A_NOT_B | B_NOT_A,    // A xor B,         missing Java function
           ISCONTAINED = A_AND_B | B_NOT_A,    // B                (unnecessary)
           NO_B = A_NOT_B,                            // A setDiff B,     removeAll
           EQUALS = A_AND_B,                // A intersect B,   retainAll
           NO_A = B_NOT_A,    // B setDiff A,     removeAll
           NONE = 0,                                  // null             (unnecessary)

           ADDALL = ANY,                // union,           addAll
           A = CONTAINS,                // A                (unnecessary)
           COMPLEMENTALL = DISJOINT,    // A xor B,         missing Java function
           B = ISCONTAINED,             // B                (unnecessary)
           REMOVEALL = NO_B,            // A setDiff B,     removeAll
           RETAINALL = EQUALS,          // A intersect B,   retainAll
           B_REMOVEALL = NO_A;          // B setDiff A,     removeAll


        /**
         * Utility that could be on SortedSet. Faster implementation than
         * what is in Java for doing contains, equals, etc.
         * @param a first set
         * @param allow filter, using ANY, CONTAINS, etc.
         * @param b second set
         * @return whether the filter relationship is true or not.
         */
        public static bool HasRelation<T>(SortedSet<T> a, int allow, SortedSet<T> b) where T : IComparable<T>
        {
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

        /**
         * Utility that could be on SortedSet. Allows faster implementation than
         * what is in Java for doing addAll, removeAll, retainAll, (complementAll).
         * @param a first set
         * @param relation the relation filter, using ANY, CONTAINS, etc.
         * @param b second set
         * @return the new set
         */
        public static SortedSet<T> DoOperation<T>(SortedSet<T> a, int relation, SortedSet<T> b) where T : IComparable<T>
        {
            // TODO: optimize this as above
            SortedSet<T> temp;
            switch (relation)
            {
                case ADDALL:
                    a.UnionWith(b);
                    return a;
                case A:
                    return a; // no action
                case B:
                    a.Clear();
                    a.UnionWith(b);
                    return a;
                case REMOVEALL:
                    a.ExceptWith(b);
                    return a;
                case RETAINALL:
                    a.IntersectWith(b);
                    return a;
                // the following is the only case not really supported by Java
                // although all could be optimized
                case COMPLEMENTALL:
                    temp = new SortedSet<T>(b, GenericComparer.NaturalComparer<T>());
                    temp.ExceptWith(a);
                    a.ExceptWith(b);
                    a.UnionWith(temp);
                    return a;
                case B_REMOVEALL:
                    temp = new SortedSet<T>(b, GenericComparer.NaturalComparer<T>());
                    temp.ExceptWith(a);
                    a.Clear();
                    a.UnionWith(temp);
                    return a;
                case NONE:
                    a.Clear();
                    return a;
                default:
                    throw new ArgumentException("Relation " + relation + " out of range");
            }
        }
    }
}
