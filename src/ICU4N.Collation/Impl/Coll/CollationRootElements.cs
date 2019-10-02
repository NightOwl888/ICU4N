using ICU4N.Support;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Container and access methods for collation elements and weights
    /// that occur in the root collator.
    /// Needed for finding boundaries for building a tailoring.
    /// <para/>
    /// This class takes and returns 16-bit secondary and tertiary weights.
    /// </summary>
    public sealed class CollationRootElements
    {
        public CollationRootElements(long[] rootElements)
        {
            elements = rootElements;
        }

        /// <summary>
        /// Higher than any root primary.
        /// </summary>
        public const long PRIMARY_SENTINEL = 0xffffff00L; // ICU4N TODO: API - Rename per .NET conventions

        /// <summary>
        /// Flag in a root element, set if the element contains secondary &amp; tertiary weights,
        /// rather than a primary.
        /// </summary>
        public const int SEC_TER_DELTA_FLAG = 0x80; // ICU4N TODO: API - Rename per .NET conventions
        /// <summary>
        /// Mask for getting the primary range step value from a primary-range-end element.
        /// </summary>
        public const int PRIMARY_STEP_MASK = 0x7f; // ICU4N TODO: API - Rename per .NET conventions

        /// <summary>
        /// Index of the first CE with a non-zero tertiary weight.
        /// Same as the start of the compact root elements table.
        /// </summary>
        public const int IX_FIRST_TERTIARY_INDEX = 0; // ICU4N TODO: API - Rename per .NET conventions
        /// <summary>
        /// Index of the first CE with a non-zero secondary weight.
        /// </summary>
        internal const int IX_FIRST_SECONDARY_INDEX = 1;
        /// <summary>
        /// Index of the first CE with a non-zero primary weight.
        /// </summary>
        internal const int IX_FIRST_PRIMARY_INDEX = 2;
        /// <summary>
        /// Must match Collation.COMMON_SEC_AND_TER_CE.
        /// </summary>
        internal const int IX_COMMON_SEC_AND_TER_CE = 3;
        /// <summary>
        /// Secondary &amp; tertiary boundaries.
        /// Bits 31..24: [fixed last secondary common byte 45]
        /// Bits 23..16: [fixed first ignorable secondary byte 80]
        /// Bits 15.. 8: reserved, 0
        /// Bits  7.. 0: [fixed first ignorable tertiary byte 3C]
        /// </summary>
        internal const int IX_SEC_TER_BOUNDARIES = 4;
        /// <summary>
        /// The current number of indexes.
        /// Currently the same as elements[IX_FIRST_TERTIARY_INDEX].
        /// </summary>
        internal const int IX_COUNT = 5;

        /// <summary>
        /// Gets the boundary between tertiary weights of primary/secondary CEs
        /// and those of tertiary CEs.
        /// This is the upper limit for tertiaries of primary/secondary CEs.
        /// This minus one is the lower limit for tertiaries of tertiary CEs.
        /// </summary>
        public int TertiaryBoundary
        {
            get { return ((int)elements[IX_SEC_TER_BOUNDARIES] << 8) & 0xff00; }
        }

        /// <summary>
        /// Gets the first assigned tertiary CE.
        /// </summary>
        internal long FirstTertiaryCE
        {
            get { return elements[(int)elements[IX_FIRST_TERTIARY_INDEX]] & ~SEC_TER_DELTA_FLAG; }
        }

        /// <summary>
        /// Gets the last assigned tertiary CE.
        /// </summary>
        internal long LastTertiaryCE
        {
            get { return elements[(int)elements[IX_FIRST_SECONDARY_INDEX] - 1] & ~SEC_TER_DELTA_FLAG; }
        }

        /// <summary>
        /// Gets the last common secondary weight.
        /// This is the lower limit for secondaries of primary CEs.
        /// </summary>
        public int LastCommonSecondary
        {
            get { return ((int)elements[IX_SEC_TER_BOUNDARIES] >> 16) & 0xff00; }
        }

        /// <summary>
        /// Gets the boundary between secondary weights of primary CEs
        /// and those of secondary CEs.
        /// This is the upper limit for secondaries of primary CEs.
        /// This minus one is the lower limit for secondaries of secondary CEs.
        /// </summary>
        public int SecondaryBoundary
        {
            get { return ((int)elements[IX_SEC_TER_BOUNDARIES] >> 8) & 0xff00; }
        }

        /// <summary>
        /// Gets the first assigned secondary CE.
        /// </summary>
        internal long FirstSecondaryCE
        {
            get { return elements[(int)elements[IX_FIRST_SECONDARY_INDEX]] & ~SEC_TER_DELTA_FLAG; }
        }

        /// <summary>
        /// Gets the last assigned secondary CE.
        /// </summary>
        internal long LastSecondaryCE
        {
            get { return elements[(int)elements[IX_FIRST_PRIMARY_INDEX] - 1] & ~SEC_TER_DELTA_FLAG; }
        }

        /// <summary>
        /// Gets the first assigned primary weight.
        /// </summary>
        internal long FirstPrimary
        {
            get { return elements[(int)elements[IX_FIRST_PRIMARY_INDEX]]; }  // step=0: cannot be a range end
        }

        /// <summary>
        /// Gets the first assigned primary CE.
        /// </summary>
        internal long FirstPrimaryCE
        {
            get { return Collation.MakeCE(FirstPrimary); }
        }

        /// <summary>
        /// Returns the last root CE with a primary weight before <paramref name="p"/>.
        /// Intended only for reordering group boundaries.
        /// </summary>
        internal long LastCEWithPrimaryBefore(long p)
        {
            if (p == 0) { return 0; }
            Debug.Assert(p > elements[(int)elements[IX_FIRST_PRIMARY_INDEX]]);
            int index = FindP(p);
            long q = elements[index];
            long secTer;
            if (p == (q & 0xffffff00L))
            {
                // p == elements[index] is a root primary. Find the CE before it.
                // We must not be in a primary range.
                Debug.Assert((q & PRIMARY_STEP_MASK) == 0);
                secTer = elements[index - 1];
                if ((secTer & SEC_TER_DELTA_FLAG) == 0)
                {
                    // Primary CE just before p.
                    p = secTer & 0xffffff00L;
                    secTer = Collation.COMMON_SEC_AND_TER_CE;
                }
                else
                {
                    // secTer = last secondary & tertiary for the previous primary
                    index -= 2;
                    for (; ; )
                    {
                        p = elements[index];
                        if ((p & SEC_TER_DELTA_FLAG) == 0)
                        {
                            p &= 0xffffff00L;
                            break;
                        }
                        --index;
                    }
                }
            }
            else
            {
                // p > elements[index] which is the previous primary.
                // Find the last secondary & tertiary weights for it.
                p = q & 0xffffff00L;
                secTer = Collation.COMMON_SEC_AND_TER_CE;
                for (; ; )
                {
                    q = elements[++index];
                    if ((q & SEC_TER_DELTA_FLAG) == 0)
                    {
                        // We must not be in a primary range.
                        Debug.Assert((q & PRIMARY_STEP_MASK) == 0);
                        break;
                    }
                    secTer = q;
                }
            }
            return (p << 32) | (secTer & ~SEC_TER_DELTA_FLAG);
        }

        /// <summary>
        /// Returns the first root CE with a primary weight of at least <paramref name="p"/>.
        /// Intended only for reordering group boundaries.
        /// </summary>
        internal long FirstCEWithPrimaryAtLeast(long p)
        {
            if (p == 0) { return 0; }
            int index = FindP(p);
            if (p != (elements[index] & 0xffffff00L))
            {
                for (; ; )
                {
                    p = elements[++index];
                    if ((p & SEC_TER_DELTA_FLAG) == 0)
                    {
                        // First primary after p. We must not be in a primary range.
                        Debug.Assert((p & PRIMARY_STEP_MASK) == 0);
                        break;
                    }
                }
            }
            // The code above guarantees that p has at most 3 bytes: (p & 0xff) == 0.
            return (p << 32) | Collation.COMMON_SEC_AND_TER_CE;
        }

        /// <summary>
        /// Returns the primary weight before <paramref name="p"/>.
        /// <paramref name="p"/> must be greater than the first root primary.
        /// </summary>
        internal long GetPrimaryBefore(long p, bool isCompressible)
        {
            int index = FindPrimary(p);
            int step;
            long q = elements[index];
            if (p == (q & 0xffffff00L))
            {
                // Found p itself. Return the previous primary.
                // See if p is at the end of a previous range.
                step = (int)q & PRIMARY_STEP_MASK;
                if (step == 0)
                {
                    // p is not at the end of a range. Look for the previous primary.
                    do
                    {
                        p = elements[--index];
                    } while ((p & SEC_TER_DELTA_FLAG) != 0);
                    return p & 0xffffff00L;
                }
            }
            else
            {
                // p is in a range, and not at the start.
                long nextElement = elements[index + 1];
                Debug.Assert(IsEndOfPrimaryRange(nextElement));
                step = (int)nextElement & PRIMARY_STEP_MASK;
            }
            // Return the previous range primary.
            if ((p & 0xffff) == 0)
            {
                return Collation.DecTwoBytePrimaryByOneStep(p, isCompressible, step);
            }
            else
            {
                return Collation.DecThreeBytePrimaryByOneStep(p, isCompressible, step);
            }
        }

        /// <summary>Returns the secondary weight before [p, s].</summary>
        internal int GetSecondaryBefore(long p, int s)
        {
            int index;
            int previousSec, sec;
            if (p == 0)
            {
                index = (int)elements[IX_FIRST_SECONDARY_INDEX];
                // Gap at the beginning of the secondary CE range.
                previousSec = 0;
                sec = (int)(elements[index] >> 16);
            }
            else
            {
                index = FindPrimary(p) + 1;
                previousSec = Collation.BEFORE_WEIGHT16;
                sec = ((int)GetFirstSecTerForPrimary(index)).TripleShift(16);
            }
            Debug.Assert(s >= sec);
            while (s > sec)
            {
                previousSec = sec;
                Debug.Assert((elements[index] & SEC_TER_DELTA_FLAG) != 0);
                sec = (int)(elements[index++] >> 16);
            }
            Debug.Assert(sec == s);
            return previousSec;
        }

        /// <summary>Returns the tertiary weight before [p, s, t].</summary>
        internal int GetTertiaryBefore(long p, int s, int t)
        {
            Debug.Assert((t & ~Collation.ONLY_TERTIARY_MASK) == 0);
            int index;
            int previousTer;
            long secTer;
            if (p == 0)
            {
                if (s == 0)
                {
                    index = (int)elements[IX_FIRST_TERTIARY_INDEX];
                    // Gap at the beginning of the tertiary CE range.
                    previousTer = 0;
                }
                else
                {
                    index = (int)elements[IX_FIRST_SECONDARY_INDEX];
                    previousTer = Collation.BEFORE_WEIGHT16;
                }
                secTer = elements[index] & ~SEC_TER_DELTA_FLAG;
            }
            else
            {
                index = FindPrimary(p) + 1;
                previousTer = Collation.BEFORE_WEIGHT16;
                secTer = GetFirstSecTerForPrimary(index);
            }
            long st = ((long)s << 16) | (uint)t;
            while (st > secTer)
            {
                if ((int)(secTer >> 16) == s) { previousTer = (int)secTer; }
                Debug.Assert((elements[index] & SEC_TER_DELTA_FLAG) != 0);
                secTer = elements[index++] & ~SEC_TER_DELTA_FLAG;
            }
            Debug.Assert(secTer == st);
            return previousTer & 0xffff;
        }

        /// <summary>
        /// Finds the index of the input primary.
        /// <paramref name="p"/> must occur as a root primary, and must not be 0.
        /// </summary>
        internal int FindPrimary(long p)
        {
            // Requirement: p must occur as a root primary.
            Debug.Assert((p & 0xff) == 0);  // at most a 3-byte primary
            int index = FindP(p);
            // If p is in a range, then we just assume that p is an actual primary in this range.
            // (Too cumbersome/expensive to check.)
            // Otherwise, it must be an exact match.
            Debug.Assert(IsEndOfPrimaryRange(elements[index + 1]) || p == (elements[index] & 0xffffff00L));
            return index;
        }

        /// <summary>
        /// Returns the primary weight after <paramref name="p"/> where index=FindPrimary(p).
        /// <paramref name="p"/> must be at least the first root primary.
        /// </summary>
        internal long GetPrimaryAfter(long p, int index, bool isCompressible)
        {
            Debug.Assert(p == (elements[index] & 0xffffff00L) || IsEndOfPrimaryRange(elements[index + 1]));
            long q = elements[++index];
            int step;
            if ((q & SEC_TER_DELTA_FLAG) == 0 && (step = (int)q & PRIMARY_STEP_MASK) != 0)
            {
                // Return the next primary in this range.
                if ((p & 0xffff) == 0)
                {
                    return Collation.IncTwoBytePrimaryByOffset(p, isCompressible, step);
                }
                else
                {
                    return Collation.IncThreeBytePrimaryByOffset(p, isCompressible, step);
                }
            }
            else
            {
                // Return the next primary in the list.
                while ((q & SEC_TER_DELTA_FLAG) != 0)
                {
                    q = elements[++index];
                }
                Debug.Assert((q & PRIMARY_STEP_MASK) == 0);
                return q;
            }
        }
        /// <summary>
        /// Returns the secondary weight after [p, s] where index=FindPrimary(p)
        /// except use index=0 for p=0.
        /// <para/>
        /// Must return a weight for every root [p, s] as well as for every weight
        /// returned by GetSecondaryBefore(). If p!=0 then s can be BEFORE_WEIGHT16.
        /// <para/>
        /// Exception: [0, 0] is handled by the CollationBuilder:
        /// Both its lower and upper boundaries are special.
        /// </summary>
        internal int GetSecondaryAfter(int index, int s)
        {
            long secTer;
            int secLimit;
            if (index == 0)
            {
                // primary = 0
                Debug.Assert(s != 0);
                index = (int)elements[IX_FIRST_SECONDARY_INDEX];
                secTer = elements[index];
                // Gap at the end of the secondary CE range.
                secLimit = 0x10000;
            }
            else
            {
                Debug.Assert(index >= (int)elements[IX_FIRST_PRIMARY_INDEX]);
                secTer = GetFirstSecTerForPrimary(index + 1);
                // If this is an explicit sec/ter unit, then it will be read once more.
                // Gap for secondaries of primary CEs.
                secLimit = SecondaryBoundary;
            }
            for (; ; )
            {
                int sec = (int)(secTer >> 16);
                if (sec > s) { return sec; }
                secTer = elements[++index];
                if ((secTer & SEC_TER_DELTA_FLAG) == 0) { return secLimit; }
            }
        }
        /// <summary>
        /// Returns the tertiary weight after [p, s, t] where index=FindPrimary(p)
        /// except use index=0 for p=0.
        /// <para/>
        /// Must return a weight for every root [p, s, t] as well as for every weight
        /// returned by GetTertiaryBefore(). If s!=0 then t can be BEFORE_WEIGHT16.
        /// <para/>
        /// Exception: [0, 0, 0] is handled by the CollationBuilder:
        /// Both its lower and upper boundaries are special.
        /// </summary>
        internal int GetTertiaryAfter(int index, int s, int t)
        {
            long secTer;
            int terLimit;
            if (index == 0)
            {
                // primary = 0
                if (s == 0)
                {
                    Debug.Assert(t != 0);
                    index = (int)elements[IX_FIRST_TERTIARY_INDEX];
                    // Gap at the end of the tertiary CE range.
                    terLimit = 0x4000;
                }
                else
                {
                    index = (int)elements[IX_FIRST_SECONDARY_INDEX];
                    // Gap for tertiaries of primary/secondary CEs.
                    terLimit = TertiaryBoundary;
                }
                secTer = elements[index] & ~SEC_TER_DELTA_FLAG;
            }
            else
            {
                Debug.Assert(index >= (int)elements[IX_FIRST_PRIMARY_INDEX]);
                secTer = GetFirstSecTerForPrimary(index + 1);
                // If this is an explicit sec/ter unit, then it will be read once more.
                terLimit = TertiaryBoundary;
            }
            long st = (((long)s & 0xffffffffL) << 16) | (uint)t;
            for (; ; )
            {
                if (secTer > st)
                {
                    Debug.Assert((secTer >> 16) == s);
                    return (int)secTer & 0xffff;
                }
                secTer = elements[++index];
                // No tertiary greater than t for this primary+secondary.
                if ((secTer & SEC_TER_DELTA_FLAG) == 0 || (secTer >> 16) > s) { return terLimit; }
                secTer &= ~SEC_TER_DELTA_FLAG;
            }
        }

        /// <summary>
        /// Returns the first secondary &amp; tertiary weights for p where index=findPrimary(p)+1.
        /// </summary>
        private long GetFirstSecTerForPrimary(int index)
        {
            long secTer = elements[index];
            if ((secTer & SEC_TER_DELTA_FLAG) == 0)
            {
                // No sec/ter delta.
                return Collation.COMMON_SEC_AND_TER_CE;
            }
            secTer &= ~SEC_TER_DELTA_FLAG;
            if (secTer > Collation.COMMON_SEC_AND_TER_CE)
            {
                // Implied sec/ter.
                return Collation.COMMON_SEC_AND_TER_CE;
            }
            // Explicit sec/ter below common/common.
            return secTer;
        }

        /// <summary>
        /// Finds the largest index i where elements[i]&lt;=p.
        /// Requires first primary&lt;=p&lt;0xffffff00 (PRIMARY_SENTINEL).
        /// Does not require that p is a root collator primary.
        /// </summary>
        private int FindP(long p)
        {
            // p need not occur as a root primary.
            // For example, it might be a reordering group boundary.
            Debug.Assert((p >> 24) != Collation.UNASSIGNED_IMPLICIT_BYTE);
            // modified binary search
            int start = (int)elements[IX_FIRST_PRIMARY_INDEX];
            Debug.Assert(p >= elements[start]);
            int limit = elements.Length - 1;
            Debug.Assert(elements[limit] >= PRIMARY_SENTINEL);
            Debug.Assert(p < elements[limit]);
            while ((start + 1) < limit)
            {
                // Invariant: elements[start] and elements[limit] are primaries,
                // and elements[start]<=p<=elements[limit].
                int i = (int)(((long)start + (long)limit) / 2);
                long q = elements[i];
                if ((q & SEC_TER_DELTA_FLAG) != 0)
                {
                    // Find the next primary.
                    int j = i + 1;
                    for (; ; )
                    {
                        if (j == limit) { break; }
                        q = elements[j];
                        if ((q & SEC_TER_DELTA_FLAG) == 0)
                        {
                            i = j;
                            break;
                        }
                        ++j;
                    }
                    if ((q & SEC_TER_DELTA_FLAG) != 0)
                    {
                        // Find the preceding primary.
                        j = i - 1;
                        for (; ; )
                        {
                            if (j == start) { break; }
                            q = elements[j];
                            if ((q & SEC_TER_DELTA_FLAG) == 0)
                            {
                                i = j;
                                break;
                            }
                            --j;
                        }
                        if ((q & SEC_TER_DELTA_FLAG) != 0)
                        {
                            // No primary between start and limit.
                            break;
                        }
                    }
                }
                if (p < (q & 0xffffff00L))
                {  // Reset the "step" bits of a range end primary.
                    limit = i;
                }
                else
                {
                    start = i;
                }
            }
            return start;
        }

        private static bool IsEndOfPrimaryRange(long q)
        {
            return (q & SEC_TER_DELTA_FLAG) == 0 && (q & PRIMARY_STEP_MASK) != 0;
        }

        /// <summary>
        /// Data structure: See ICU4C source/i18n/collationrootelements.h.
        /// </summary>
        private long[] elements;
    }
}
