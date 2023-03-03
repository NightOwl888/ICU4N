using ICU4N.Text;
using J2N;
using J2N.Collections;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Impl.Coll
{
    // CollationSettings.cs, ported from collationsettings.h/.cpp
    // 
    // C++ version created on: 2013feb07
    // created by: Markus W. Scherer

    /// <summary>
    /// Collation settings/options/attributes.
    /// These are the values that can be changed via API.
    /// </summary>
    public sealed class CollationSettings : SharedObject
    {
        /// <summary>
        /// Options bit 0: Perform the FCD check on the input text and deliver normalized text.
        /// </summary>
        public const int CheckFCD = 1;
        /// <summary>
        /// Options bit 1: Numeric collation.
        /// Also known as CODAN = COllate Digits As Numbers.
        /// <para/>
        /// Treat digit sequences as numbers with CE sequences in numeric order,
        /// rather than returning a normal CE for each digit.
        /// </summary>
        public const int Numeric = 2;
        /// <summary>
        /// "Shifted" alternate handling, see <see cref="AlternateMask"/>.
        /// </summary>
        internal const int Shifted = 4;
        /// <summary>
        /// Options bits 3..2: Alternate-handling mask. 0 for non-ignorable.
        /// Reserve values 8 and 0xc for shift-trimmed and blanked.
        /// </summary>
        internal const int AlternateMask = 0xc;
        /// <summary>
        /// Options bits 6..4: The 3-bit maxVariable value bit field is shifted by this value.
        /// </summary>
        internal const int MaxVariableShift = 4;
        /// <summary>maxVariable options bit mask before shifting.</summary>
        internal const int MaxVariableMask = 0x70;
        // Options bit 7: Reserved/unused/0.
        /// <summary>
        /// Options bit 8: Sort uppercase first if caseLevel or caseFirst is on.
        /// </summary>
        internal const int UpperFirst = 0x100;
        /// <summary>
        /// Options bit 9: Keep the case bits in the tertiary weight (they trump other tertiary values)
        /// unless case level is on (when they are *moved* into the separate case level).
        /// By default, the case bits are removed from the tertiary weight (ignored).
        /// <para/>
        /// When <see cref="CaseFirst"/> is off, <see cref="UpperFirst"/> must be off too, corresponding to
        /// the tri-value UCOL_CASE_FIRST attribute: UCOL_OFF vs. UCOL_LOWER_FIRST vs. UCOL_UPPER_FIRST.
        /// </summary>
        public const int CaseFirst = 0x200;
        /// <summary>
        /// Options bit mask for caseFirst and upperFirst, before shifting.
        /// Same value as caseFirst==upperFirst.
        /// </summary>
        public const int CaseFirstAndUpperMask = CaseFirst | UpperFirst; // ICU4N TODO: API - convert constants to [Flags] enum? Check the C implementation for ideas.
        /// <summary>
        /// Options bit 10: Insert the case level between the secondary and tertiary levels.
        /// </summary>
        public const int CaseLevel = 0x400;
        /// <summary>
        /// Options bit 11: Compare secondary weights backwards. ("French secondary")
        /// </summary>
        public const int BackwardSecondary = 0x800;
        /// <summary>
        /// Options bits 15..12: The 4-bit strength value bit field is shifted by this value.
        /// It is the top used bit field in the options. (No need to mask after shifting.)
        /// </summary>
        internal const int StrengthShift = 12;
        /// <summary>Strength options bit mask before shifting.</summary>
        internal const int StrengthMask = 0xf000;

        /// <summary>maxVariable values</summary>
        internal const int MaxVariableSpace = 0;
        internal const int MaxVariblePunctuation = 1;
        internal const int MaxVariableSymbol = 2;
        internal const int MaxVarCurrency = 3;

        internal CollationSettings() { }

        public override object Clone()
        {
            CollationSettings newSettings = (CollationSettings)base.Clone();
            // Note: The reorderTable, reorderRanges, and reorderCodes need not be cloned
            // because, in Java, they only get replaced but not modified.
            newSettings.fastLatinPrimaries = (char[])fastLatinPrimaries.Clone();
            return newSettings;
        }

        public override bool Equals(object other)
        {
            if (other == null) { return false; }
            if (!this.GetType().Equals(other.GetType())) { return false; }
            CollationSettings o = (CollationSettings)other;
            if (options != o.options) { return false; }
            if ((options & AlternateMask) != 0 && variableTop != o.variableTop) { return false; }
            if (!ArrayEqualityComparer<int>.OneDimensional.Equals(reorderCodes, o.reorderCodes)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int h = options << 8;
            if ((options & AlternateMask) != 0) { h = (int)(h ^ variableTop); }
            h ^= reorderCodes.Length;
            for (int i = 0; i < reorderCodes.Length; ++i)
            {
                h ^= (reorderCodes[i] << i);
            }
            return h;
        }

        public void ResetReordering()
        {
            // When we turn off reordering, we want to set a null permutation
            // rather than a no-op permutation.
            reorderTable = null;
            minHighNoReorder = 0;
            reorderRanges = null;
            reorderCodes = EMPTY_INT_ARRAY;
        }

        internal void AliasReordering(CollationData data, int[] codesAndRanges, int codesLength, byte[] table)
        {
            int[] codes;
            if (codesLength == codesAndRanges.Length)
            {
                codes = codesAndRanges;
            }
            else
            {
                // TODO: Java 6: Arrays.copyOf(codes, codesLength);
                codes = new int[codesLength];
                System.Array.Copy(codesAndRanges, 0, codes, 0, codesLength);
            }
            int rangesStart = codesLength;
            int rangesLimit = codesAndRanges.Length;
            int rangesLength = rangesLimit - rangesStart;
            if (table != null &&
                    (rangesLength == 0 ?
                            !ReorderTableHasSplitBytes(table) :
                            rangesLength >= 2 &&
                            // The first offset must be 0. The last offset must not be 0.
                            (codesAndRanges[rangesStart] & 0xffff) == 0 &&
                            (codesAndRanges[rangesLimit - 1] & 0xffff) != 0))
            {
                reorderTable = table;
                reorderCodes = codes;
                // Drop ranges before the first split byte. They are reordered by the table.
                // This then speeds up reordering of the remaining ranges.
                int firstSplitByteRangeIndex = rangesStart;
                while (firstSplitByteRangeIndex < rangesLimit &&
                        (codesAndRanges[firstSplitByteRangeIndex] & 0xff0000) == 0)
                {
                    // The second byte of the primary limit is 0.
                    ++firstSplitByteRangeIndex;
                }
                if (firstSplitByteRangeIndex == rangesLimit)
                {
                    Debug.Assert(!ReorderTableHasSplitBytes(table));
                    minHighNoReorder = 0;
                    reorderRanges = null;
                }
                else
                {
                    Debug.Assert(table[codesAndRanges[firstSplitByteRangeIndex].TripleShift(24)] == 0);
                    minHighNoReorder = codesAndRanges[rangesLimit - 1] & 0xffff0000L;
                    SetReorderRanges(codesAndRanges, firstSplitByteRangeIndex,
                            rangesLimit - firstSplitByteRangeIndex);
                }
                return;
            }
            // Regenerate missing data.
            SetReordering(data, codes);
        }

        public void SetReordering(CollationData data, int[] codes)
        {
            if (codes.Length == 0 || (codes.Length == 1 && codes[0] == ICU4N.Text.ReorderCodes.None))
            {
                ResetReordering();
                return;
            }
            List<int> ranges = new List<int>();
            data.MakeReorderRanges(codes, ranges);
            int rangesLength = ranges.Count;
            if (rangesLength == 0)
            {
                ResetReordering();
                return;
            }
            // ranges[] contains at least two (limit, offset) pairs.
            // The first offset must be 0. The last offset must not be 0.
            // Separators (at the low end) and trailing weights (at the high end)
            // are never reordered.
            Debug.Assert(rangesLength >= 2);
            Debug.Assert((ranges[0] & 0xffff) == 0 && (ranges[rangesLength - 1] & 0xffff) != 0);
            minHighNoReorder = ranges[rangesLength - 1] & 0xffff0000L;

            // Write the lead byte permutation table.
            // Set a 0 for each lead byte that has a range boundary in the middle.
            byte[] table = new byte[256];
            int b = 0;
            int firstSplitByteRangeIndex = -1;
            for (int i = 0; i < rangesLength; ++i)
            {
                int pair = ranges[i];
                int limit1 = pair.TripleShift(24);
                while (b < limit1)
                {
                    table[b] = (byte)(b + pair);
                    ++b;
                }
                // Check the second byte of the limit.
                if ((pair & 0xff0000) != 0)
                {
                    table[limit1] = 0;
                    b = limit1 + 1;
                    if (firstSplitByteRangeIndex < 0)
                    {
                        firstSplitByteRangeIndex = i;
                    }
                }
            }
            while (b <= 0xff)
            {
                table[b] = (byte)b;
                ++b;
            }
            int rangesStart;
            if (firstSplitByteRangeIndex < 0)
            {
                // The lead byte permutation table alone suffices for reordering.
                rangesStart = rangesLength = 0;
            }
            else
            {
                // Remove the ranges below the first split byte.
                rangesStart = firstSplitByteRangeIndex;
                rangesLength -= firstSplitByteRangeIndex;
            }
            SetReorderArrays(codes, ranges, rangesStart, rangesLength, table);
        }

        private void SetReorderArrays(int[] codes,
                IList<int> ranges, int rangesStart, int rangesLength, byte[] table)
        {
            // Very different from C++. See the comments after the reorderCodes declaration.
            if (codes == null)
            {
                codes = EMPTY_INT_ARRAY;
            }
            Debug.Assert((codes.Length == 0) == (table == null));
            reorderTable = table;
            reorderCodes = codes;
            SetReorderRanges(ranges, rangesStart, rangesLength);
        }

        private void SetReorderRanges(IList<int> ranges, int rangesStart, int rangesLength)
        {
            if (rangesLength == 0)
            {
                reorderRanges = null;
            }
            else
            {
                reorderRanges = new long[rangesLength];
                int i = 0;
                do
                {
                    reorderRanges[i++] = ranges[rangesStart++] & 0xffffffffL;
                } while (i < rangesLength);
            }
        }

        public void CopyReorderingFrom(CollationSettings other)
        {
            if (!other.HasReordering)
            {
                ResetReordering();
                return;
            }
            minHighNoReorder = other.minHighNoReorder;
            reorderTable = other.reorderTable;
            reorderRanges = other.reorderRanges;
            reorderCodes = other.reorderCodes;
        }

        public bool HasReordering => reorderTable != null;

        private static bool ReorderTableHasSplitBytes(byte[] table)
        {
            Debug.Assert(table[0] == 0);
            for (int i = 1; i < 256; ++i)
            {
                if (table[i] == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public long Reorder(long p)
        {
            byte b = reorderTable[(int)p.TripleShift(24)];
            if (b != 0 || p <= Collation.NO_CE_PRIMARY)
            {
                return ((b & 0xffL) << 24) | (p & 0xffffff);
            }
            else
            {
                return ReorderEx(p);
            }
        }

        private long ReorderEx(long p)
        {
            Debug.Assert(minHighNoReorder > 0);
            if (p >= minHighNoReorder) { return p; }
            // Round up p so that its lower 16 bits are >= any offset bits.
            // Then compare q directly with (limit, offset) pairs.
            long q = p | 0xffff;
            long r;
            int i = 0;
            while (q >= (r = reorderRanges[i])) { ++i; }
            return p + ((long)(short)r << 24);
        }

        // In C++, we use enums for attributes and their values, with a special value for the default.
        // Combined getter/setter methods handle many attributes.
        // In Java, we have specific methods for getting, setting, and set-to-default,
        // except that this class uses bits in its own bit set for simple values.

        public CollationStrength Strength
        {
            get => GetStrength(options);
            set
            {
                int noStrength = options & ~StrengthMask;
                switch (value)
                {
                    case CollationStrength.Primary:
                    case CollationStrength.Secondary:
                    case CollationStrength.Tertiary:
                    case CollationStrength.Quaternary:
                    case CollationStrength.Identical:
                        options = noStrength | ((int)value << StrengthShift);
                        break;
                    default:
                        throw new ArgumentException("illegal strength value " + value);
                }
            }
        }

        public void SetStrengthDefault(int defaultOptions)
        {
            int noStrength = options & ~StrengthMask;
            options = noStrength | (defaultOptions & StrengthMask);
        }

        internal static CollationStrength GetStrength(int options)
        {
            return (CollationStrength)(options >> StrengthShift);
        }

        /// <summary>Sets the options bit for an on/off attribute.</summary>
        public void SetFlag(int bit, bool value)
        {
            if (value)
            {
                options |= bit;
            }
            else
            {
                options &= ~bit;
            }
        }

        public void SetFlagDefault(int bit, int defaultOptions)
        {
            options = (options & ~bit) | (defaultOptions & bit);
        }

        public bool GetFlag(int bit)
        {
            return (options & bit) != 0;
        }

        public void SetCaseFirst(int value)
        {
            Debug.Assert(value == 0 || value == CaseFirst || value == CaseFirstAndUpperMask);
            int noCaseFirst = options & ~CaseFirstAndUpperMask;
            options = noCaseFirst | value;
        }

        public void SetCaseFirstDefault(int defaultOptions)
        {
            int noCaseFirst = options & ~CaseFirstAndUpperMask;
            options = noCaseFirst | (defaultOptions & CaseFirstAndUpperMask);
        }

        public int GetCaseFirst()
        {
            return options & CaseFirstAndUpperMask;
        }

        public void SetAlternateHandlingShifted(bool value)
        {
            int noAlternate = options & ~AlternateMask;
            if (value)
            {
                options = noAlternate | Shifted;
            }
            else
            {
                options = noAlternate;
            }
        }

        public void SetAlternateHandlingDefault(int defaultOptions)
        {
            int noAlternate = options & ~AlternateMask;
            options = noAlternate | (defaultOptions & AlternateMask);
        }

        public bool AlternateHandling => (options & AlternateMask) != 0;

        public void SetMaxVariable(int value, int defaultOptions)
        {
            int noMax = options & ~MaxVariableMask;
            switch (value)
            {
                case MaxVariableSpace:
                case MaxVariblePunctuation:
                case MaxVariableSymbol:
                case MaxVarCurrency:
                    options = noMax | (value << MaxVariableShift);
                    break;
                case -1:
                    options = noMax | (defaultOptions & MaxVariableMask);
                    break;
                default:
                    throw new ArgumentException("illegal maxVariable value " + value);
            }
        }

        public int MaxVariable => (options & MaxVariableMask) >> MaxVariableShift;

        /// <summary>
        /// Include case bits in the tertiary level if caseLevel=off and caseFirst!=off.
        /// </summary>
        internal static bool IsTertiaryWithCaseBits(int options)
        {
            return (options & (CaseLevel | CaseFirst)) == CaseFirst;
        }
        internal static int GetTertiaryMask(int options)
        {
            // Remove the case bits from the tertiary weight when caseLevel is on or caseFirst is off.
            return IsTertiaryWithCaseBits(options) ?
                    Collation.CASE_AND_TERTIARY_MASK : Collation.OnlyTertiaryMask;
        }

        internal static bool SortsTertiaryUpperCaseFirst(int options)
        {
            // On tertiary level, consider case bits and sort uppercase first
            // if caseLevel is off and caseFirst==upperFirst.
            return (options & (CaseLevel | CaseFirstAndUpperMask)) == CaseFirstAndUpperMask;
        }

        public bool DontCheckFCD => (options & CheckFCD) == 0; // ICU4N TODO: API - per MSDN properties should be in the affirmative

        internal bool HasBackwardSecondary => (options & BackwardSecondary) != 0;

        public bool IsNumeric => (options & Numeric) != 0;

        /// <summary>CHECK_FCD etc.</summary>
        private int options = ((int)CollationStrength.Tertiary << StrengthShift) |  // DEFAULT_STRENGTH
                (MaxVariblePunctuation << MaxVariableShift);
        /// <summary>Variable-top primary weight.</summary>
        private long variableTop;
        /// <summary>
        /// 256-byte table for reordering permutation of primary lead bytes; null if no reordering.
        /// A 0 entry at a non-zero index means that the primary lead byte is "split"
        /// (there are different offsets for primaries that share that lead byte)
        /// and the reordering offset must be determined via the <see cref="reorderRanges"/>.
        /// </summary>
        private byte[] reorderTable;
        /// <summary>Limit of last reordered range. 0 if no reordering or no split bytes.</summary>
        internal long minHighNoReorder;
        /// <summary>
        /// Primary-weight ranges for script reordering,
        /// to be used by <see cref="Reorder(long)"/> for split-reordered primary lead bytes.
        /// <para/>
        /// Each entry is a (limit, offset) pair.
        /// The upper 16 bits of the entry are the upper 16 bits of the
        /// exclusive primary limit of a range.
        /// Primaries between the previous limit and this one have their lead bytes
        /// modified by the signed offset (-0xff..+0xff) stored in the lower 16 bits.
        /// <para/>
        /// <see cref="CollationData.MakeReorderRanges(int[], IList{int})"/> writes a full list where the first range
        /// (at least for terminators and separators) has a 0 offset.
        /// The last range has a non-zero offset.
        /// <see cref="minHighNoReorder"/> is set to the limit of that last range.
        /// <para/>
        /// In the settings object, the initial ranges before the first split lead byte
        /// are omitted for efficiency; they are handled by <see cref="Reorder(long)"/> via the <see cref="reorderTable"/>.
        /// If there are no split-reordered lead bytes, then no ranges are needed.
        /// </summary>
        internal long[] reorderRanges;
        /// <summary>Array of reorder codes; ignored if length == 0.</summary>
        private int[] reorderCodes = EMPTY_INT_ARRAY;
        // Note: In C++, we keep a memory block around for the reorder codes,
        // the ranges, and the permutation table,
        // and modify them for new codes.
        // In Java, we simply copy references and then never modify the array contents.
        // The caller must abandon the arrays.
        // Reorder codes from the public setter API must be cloned.
        private static readonly int[] EMPTY_INT_ARRAY = new int[0];

        /// <summary>Options for CollationFastLatin. Negative if disabled.</summary>
        private int fastLatinOptions = -1;
        // fastLatinPrimaries.length must be equal to CollationFastLatin.LATIN_LIMIT,
        // but we do not import CollationFastLatin to reduce circular dependencies.
        private char[] fastLatinPrimaries = new char[0x180];  // mutable contents

        public int Options
        {
            get => options;
            set => options = value;
        }

        public long VariableTop
        {
            get => variableTop;
            set => variableTop = value;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public byte[] ReorderTable
        {
            get => reorderTable;
            set => reorderTable = value;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public int[] ReorderCodes => reorderCodes;

        public int FastLatinOptions
        {
            get => fastLatinOptions;
            set => fastLatinOptions = value;
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public char[] FastLatinPrimaries => fastLatinPrimaries;
    }
}
