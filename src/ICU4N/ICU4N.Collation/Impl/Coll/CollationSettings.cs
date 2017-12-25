using ICU4N.Text;
using ICU4N.Support;
using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ICU4N.Impl.Coll
{
    public sealed class CollationSettings : SharedObject
    {
        /**
         * Options bit 0: Perform the FCD check on the input text and deliver normalized text.
         */
        public const int CHECK_FCD = 1;
        /**
         * Options bit 1: Numeric collation.
         * Also known as CODAN = COllate Digits As Numbers.
         *
         * Treat digit sequences as numbers with CE sequences in numeric order,
         * rather than returning a normal CE for each digit.
         */
        public const int NUMERIC = 2;
        /**
         * "Shifted" alternate handling, see ALTERNATE_MASK.
         */
        internal const int SHIFTED = 4;
        /**
         * Options bits 3..2: Alternate-handling mask. 0 for non-ignorable.
         * Reserve values 8 and 0xc for shift-trimmed and blanked.
         */
        internal const int ALTERNATE_MASK = 0xc;
        /**
         * Options bits 6..4: The 3-bit maxVariable value bit field is shifted by this value.
         */
        internal const int MAX_VARIABLE_SHIFT = 4;
        /** maxVariable options bit mask before shifting. */
        internal const int MAX_VARIABLE_MASK = 0x70;
        /** Options bit 7: Reserved/unused/0. */
        /**
         * Options bit 8: Sort uppercase first if caseLevel or caseFirst is on.
         */
        internal const int UPPER_FIRST = 0x100;
        /**
         * Options bit 9: Keep the case bits in the tertiary weight (they trump other tertiary values)
         * unless case level is on (when they are *moved* into the separate case level).
         * By default, the case bits are removed from the tertiary weight (ignored).
         *
         * When CASE_FIRST is off, UPPER_FIRST must be off too, corresponding to
         * the tri-value UCOL_CASE_FIRST attribute: UCOL_OFF vs. UCOL_LOWER_FIRST vs. UCOL_UPPER_FIRST.
         */
        public const int CASE_FIRST = 0x200;
        /**
         * Options bit mask for caseFirst and upperFirst, before shifting.
         * Same value as caseFirst==upperFirst.
         */
        public const int CASE_FIRST_AND_UPPER_MASK = CASE_FIRST | UPPER_FIRST;
        /**
         * Options bit 10: Insert the case level between the secondary and tertiary levels.
         */
        public const int CASE_LEVEL = 0x400;
        /**
         * Options bit 11: Compare secondary weights backwards. ("French secondary")
         */
        public const int BACKWARD_SECONDARY = 0x800;
        /**
         * Options bits 15..12: The 4-bit strength value bit field is shifted by this value.
         * It is the top used bit field in the options. (No need to mask after shifting.)
         */
        internal const int STRENGTH_SHIFT = 12;
        /** Strength options bit mask before shifting. */
        internal const int STRENGTH_MASK = 0xf000;

        /** maxVariable values */
        internal const int MAX_VAR_SPACE = 0;
        internal const int MAX_VAR_PUNCT = 1;
        internal const int MAX_VAR_SYMBOL = 2;
        internal const int MAX_VAR_CURRENCY = 3;

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
            if ((options & ALTERNATE_MASK) != 0 && variableTop != o.variableTop) { return false; }
            if (!Arrays.Equals(reorderCodes, o.reorderCodes)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            int h = options << 8;
            if ((options & ALTERNATE_MASK) != 0) { h = (int)(h ^ variableTop); }
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
            if (codes.Length == 0 || (codes.Length == 1 && codes[0] == Text.ReorderCodes.None))
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

        public bool HasReordering { get { return reorderTable != null; } }

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

        //public void setStrength(int value)
        //{
        //    int noStrength = options & ~STRENGTH_MASK;
        //    switch (value)
        //    {
        //        case Collator.PRIMARY:
        //        case Collator.SECONDARY:
        //        case Collator.TERTIARY:
        //        case Collator.QUATERNARY:
        //        case Collator.IDENTICAL:
        //            options = noStrength | (value << STRENGTH_SHIFT);
        //            break;
        //        default:
        //            throw new IllegalArgumentException("illegal strength value " + value);
        //    }
        //}

        public CollationStrength Strength
        {
            get { return GetStrength(options); }
            set
            {
                int noStrength = options & ~STRENGTH_MASK;
                switch (value)
                {
                    case CollationStrength.Primary:
                    case CollationStrength.Secondary:
                    case CollationStrength.Tertiary:
                    case CollationStrength.Quaternary:
                    case CollationStrength.Identical:
                        options = noStrength | ((int)value << STRENGTH_SHIFT);
                        break;
                    default:
                        throw new ArgumentException("illegal strength value " + value);
                }
            }
        }

        public void SetStrengthDefault(int defaultOptions)
        {
            int noStrength = options & ~STRENGTH_MASK;
            options = noStrength | (defaultOptions & STRENGTH_MASK);
        }

        internal static CollationStrength GetStrength(int options)
        {
            return (CollationStrength)(options >> STRENGTH_SHIFT);
        }

        //public int GetStrength()
        //{
        //    return GetStrength(options);
        //}

        /** Sets the options bit for an on/off attribute. */
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

        //public void setCaseFirst(int value)
        //{
        //    Debug.Assert(value == 0 || value == CASE_FIRST || value == CASE_FIRST_AND_UPPER_MASK);
        //    int noCaseFirst = options & ~CASE_FIRST_AND_UPPER_MASK;
        //    options = noCaseFirst | value;
        //}

        public void SetCaseFirstDefault(int defaultOptions)
        {
            int noCaseFirst = options & ~CASE_FIRST_AND_UPPER_MASK;
            options = noCaseFirst | (defaultOptions & CASE_FIRST_AND_UPPER_MASK);
        }

        //public int getCaseFirst()
        //{
        //    return options & CASE_FIRST_AND_UPPER_MASK;
        //}

        public int CaseFirst
        {
            get { return options & CASE_FIRST_AND_UPPER_MASK; }
            set
            {
                Debug.Assert(value == 0 || value == CASE_FIRST || value == CASE_FIRST_AND_UPPER_MASK);
                int noCaseFirst = options & ~CASE_FIRST_AND_UPPER_MASK;
                options = noCaseFirst | value;
            }
        }

        public void SetAlternateHandlingShifted(bool value)
        {
            int noAlternate = options & ~ALTERNATE_MASK;
            if (value)
            {
                options = noAlternate | SHIFTED;
            }
            else
            {
                options = noAlternate;
            }
        }

        public void SetAlternateHandlingDefault(int defaultOptions)
        {
            int noAlternate = options & ~ALTERNATE_MASK;
            options = noAlternate | (defaultOptions & ALTERNATE_MASK);
        }

        public bool AlternateHandling
        {
            get { return (options & ALTERNATE_MASK) != 0; }
        }

        public void SetMaxVariable(int value, int defaultOptions)
        {
            int noMax = options & ~MAX_VARIABLE_MASK;
            switch (value)
            {
                case MAX_VAR_SPACE:
                case MAX_VAR_PUNCT:
                case MAX_VAR_SYMBOL:
                case MAX_VAR_CURRENCY:
                    options = noMax | (value << MAX_VARIABLE_SHIFT);
                    break;
                case -1:
                    options = noMax | (defaultOptions & MAX_VARIABLE_MASK);
                    break;
                default:
                    throw new ArgumentException("illegal maxVariable value " + value);
            }
        }

        //public int getMaxVariable()
        //{
        //    return (options & MAX_VARIABLE_MASK) >> MAX_VARIABLE_SHIFT;
        //}

        public int MaxVariable
        {
            get { return (options & MAX_VARIABLE_MASK) >> MAX_VARIABLE_SHIFT; }
        }

        /**
         * Include case bits in the tertiary level if caseLevel=off and caseFirst!=off.
         */
        internal static bool IsTertiaryWithCaseBits(int options)
        {
            return (options & (CASE_LEVEL | CASE_FIRST)) == CASE_FIRST;
        }
        internal static int GetTertiaryMask(int options)
        {
            // Remove the case bits from the tertiary weight when caseLevel is on or caseFirst is off.
            return IsTertiaryWithCaseBits(options) ?
                    Collation.CASE_AND_TERTIARY_MASK : Collation.ONLY_TERTIARY_MASK;
        }

        internal static bool SortsTertiaryUpperCaseFirst(int options)
        {
            // On tertiary level, consider case bits and sort uppercase first
            // if caseLevel is off and caseFirst==upperFirst.
            return (options & (CASE_LEVEL | CASE_FIRST_AND_UPPER_MASK)) == CASE_FIRST_AND_UPPER_MASK;
        }

        public bool DontCheckFCD // ICU4N TODO: API - per MSDN properties should be in the affirmative
        {
            get { return (options & CHECK_FCD) == 0; }
        }

        internal bool HasBackwardSecondary
        {
            get { return (options & BACKWARD_SECONDARY) != 0; }
        }

        public bool IsNumeric
        {
            get { return (options & NUMERIC) != 0; }
        }

        /** CHECK_FCD etc. */
        private int options = ((int)CollationStrength.Tertiary << STRENGTH_SHIFT) |  // DEFAULT_STRENGTH
                (MAX_VAR_PUNCT << MAX_VARIABLE_SHIFT);
        /** Variable-top primary weight. */
        private long variableTop;
        /**
         * 256-byte table for reordering permutation of primary lead bytes; null if no reordering.
         * A 0 entry at a non-zero index means that the primary lead byte is "split"
         * (there are different offsets for primaries that share that lead byte)
         * and the reordering offset must be determined via the reorderRanges.
         */
        private byte[] reorderTable;
        /** Limit of last reordered range. 0 if no reordering or no split bytes. */
        internal long minHighNoReorder;
        /**
         * Primary-weight ranges for script reordering,
         * to be used by reorder(p) for split-reordered primary lead bytes.
         *
         * <p>Each entry is a (limit, offset) pair.
         * The upper 16 bits of the entry are the upper 16 bits of the
         * exclusive primary limit of a range.
         * Primaries between the previous limit and this one have their lead bytes
         * modified by the signed offset (-0xff..+0xff) stored in the lower 16 bits.
         *
         * <p>CollationData.makeReorderRanges() writes a full list where the first range
         * (at least for terminators and separators) has a 0 offset.
         * The last range has a non-zero offset.
         * minHighNoReorder is set to the limit of that last range.
         *
         * <p>In the settings object, the initial ranges before the first split lead byte
         * are omitted for efficiency; they are handled by reorder(p) via the reorderTable.
         * If there are no split-reordered lead bytes, then no ranges are needed.
         */
        internal long[] reorderRanges;
        /** Array of reorder codes; ignored if length == 0. */
        private int[] reorderCodes = EMPTY_INT_ARRAY;
        // Note: In C++, we keep a memory block around for the reorder codes,
        // the ranges, and the permutation table,
        // and modify them for new codes.
        // In Java, we simply copy references and then never modify the array contents.
        // The caller must abandon the arrays.
        // Reorder codes from the public setter API must be cloned.
        private static readonly int[] EMPTY_INT_ARRAY = new int[0];

        /** Options for CollationFastLatin. Negative if disabled. */
        private int fastLatinOptions = -1;
        // fastLatinPrimaries.length must be equal to CollationFastLatin.LATIN_LIMIT,
        // but we do not import CollationFastLatin to reduce circular dependencies.
        private char[] fastLatinPrimaries = new char[0x180];  // mutable contents

        public int Options
        {
            get { return options; }
            set { options = value; }
        }

        public long VariableTop
        {
            get { return variableTop; }
            set { variableTop = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public byte[] ReorderTable
        {
            get { return reorderTable; }
            set { reorderTable = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public int[] ReorderCodes { get { return reorderCodes; } }
        public int FastLatinOptions
        {
            get { return fastLatinOptions; }
            set { fastLatinOptions = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public char[] FastLatinPrimaries { get { return fastLatinPrimaries; } }
    }
}
