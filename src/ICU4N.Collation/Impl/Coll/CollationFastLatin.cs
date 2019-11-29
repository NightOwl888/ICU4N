using ICU4N.Globalization;
using ICU4N.Text;
using J2N;
using J2N.Numerics;
using J2N.Text;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <since>2013aug09</since>
    /// <author>Markus W. Scherer</author>
    public static class CollationFastLatin /* all static */
    {
        /// <summary>
        /// Fast Latin format version (one byte 1..FF).
        /// Must be incremented for any runtime-incompatible changes,
        /// in particular, for changes to any of the following constants.
        /// <para/>
        /// When the major version number of the main data format changes,
        /// we can reset this fast Latin version to 1.
        /// </summary>
        public const int Version = 2;

        public const int LatinMax = 0x17f;
        public const int LatinLimit = LatinMax + 1;

        internal const int LATIN_MAX_UTF8_LEAD = 0xc5;  // UTF-8 lead byte of LATIN_MAX

        internal const int PUNCT_START = 0x2000;
        internal const int PUNCT_LIMIT = 0x2040;

        // excludes U+FFFE & U+FFFF
        internal const int NUM_FAST_CHARS = LatinLimit + (PUNCT_LIMIT - PUNCT_START);

        // Note on the supported weight ranges:
        // Analysis of UCA 6.3 and CLDR 23 non-search tailorings shows that
        // the CEs for characters in the above ranges, excluding expansions with length >2,
        // excluding contractions of >2 characters, and other restrictions
        // (see the builder's getCEsFromCE32()),
        // use at most about 150 primary weights,
        // where about 94 primary weights are possibly-variable (space/punct/symbol/currency),
        // at most 4 secondary before-common weights,
        // at most 4 secondary after-common weights,
        // at most 16 secondary high weights (in secondary CEs), and
        // at most 4 tertiary after-common weights.
        // The following ranges are designed to support slightly more weights than that.
        // (en_US_POSIX is unusual: It creates about 64 variable + 116 Latin primaries.)

        // Digits may use long primaries (preserving more short ones)
        // or short primaries (faster) without changing this data structure.
        // (If we supported numeric collation, then digits would have to have long primaries
        // so that special handling does not affect the fast path.)

        internal const int SHORT_PRIMARY_MASK = 0xfc00;  // bits 15..10
        internal const int INDEX_MASK = 0x3ff;  // bits 9..0 for expansions & contractions
        internal const int SECONDARY_MASK = 0x3e0;  // bits 9..5
        internal const int CASE_MASK = 0x18;  // bits 4..3
        internal const int LONG_PRIMARY_MASK = 0xfff8;  // bits 15..3
        internal const int TERTIARY_MASK = 7;  // bits 2..0
        internal const int CASE_AND_TERTIARY_MASK = CASE_MASK | TERTIARY_MASK;

        internal const int TWO_SHORT_PRIMARIES_MASK =
                (SHORT_PRIMARY_MASK << 16) | SHORT_PRIMARY_MASK;  // 0xfc00fc00
        internal const int TWO_LONG_PRIMARIES_MASK =
                (LONG_PRIMARY_MASK << 16) | LONG_PRIMARY_MASK;  // 0xfff8fff8
        internal const int TWO_SECONDARIES_MASK =
                (SECONDARY_MASK << 16) | SECONDARY_MASK;  // 0x3e003e0
        internal const int TWO_CASES_MASK =
                (CASE_MASK << 16) | CASE_MASK;  // 0x180018
        internal const int TWO_TERTIARIES_MASK =
                (TERTIARY_MASK << 16) | TERTIARY_MASK;  // 0x70007

        /// <summary>
        /// Contraction with one fast Latin character.
        /// Use INDEX_MASK to find the start of the contraction list after the fixed table.
        /// The first entry contains the default mapping.
        /// Otherwise use CONTR_CHAR_MASK for the contraction character index
        /// (in ascending order).
        /// Use CONTR_LENGTH_SHIFT for the length of the entry
        /// (1=BAIL_OUT, 2=one CE, 3=two CEs).
        /// <para/>
        /// Also, U+0000 maps to a contraction entry, so that the fast path need not
        /// check for NUL termination.
        /// It usually maps to a contraction list with only the completely ignorable default value.
        /// </summary>
        internal const int CONTRACTION = 0x400;
        /// <summary>
        /// An expansion encodes two CEs.
        /// Use <see cref="INDEX_MASK"/> to find the pair of CEs after the fixed table.
        /// <para/>
        /// The higher a mini CE value, the easier it is to process.
        /// For expansions and higher, no context needs to be considered.
        /// </summary>
        internal const int EXPANSION = 0x800;
        /// <summary>
        /// Encodes one CE with a long/low mini primary (there are 128).
        /// All potentially-variable primaries must be in this range,
        /// to make the short-primary path as fast as possible.
        /// </summary>
        internal const int MIN_LONG = 0xc00;
        internal const int LONG_INC = 8;
        internal const int MAX_LONG = 0xff8;
        /// <summary>
        /// Encodes one CE with a short/high primary (there are 60),
        /// plus a secondary CE if the secondary weight is high.
        /// Fast handling: At least all letter primaries should be in this range.
        /// </summary>
        internal const int MIN_SHORT = 0x1000;
        internal const int SHORT_INC = 0x400;
        /// <summary>The highest primary weight is reserved for U+FFFF.</summary>
        internal const int MAX_SHORT = SHORT_PRIMARY_MASK;

        internal const int MIN_SEC_BEFORE = 0;  // must add SEC_OFFSET
        internal const int SEC_INC = 0x20;
        internal const int MAX_SEC_BEFORE = MIN_SEC_BEFORE + 4 * SEC_INC;  // 5 before common
        internal const int COMMON_SEC = MAX_SEC_BEFORE + SEC_INC;
        internal const int MIN_SEC_AFTER = COMMON_SEC + SEC_INC;
        internal const int MAX_SEC_AFTER = MIN_SEC_AFTER + 5 * SEC_INC;  // 6 after common
        internal const int MIN_SEC_HIGH = MAX_SEC_AFTER + SEC_INC;  // 20 high secondaries
        internal const int MAX_SEC_HIGH = SECONDARY_MASK;

        /// <summary>
        /// Lookup: Add this offset to secondary weights, except for completely ignorable CEs.
        /// Must be greater than any special value, e.g., <see cref="MERGE_WEIGHT"/>.
        /// The exact value is not relevant for the format version.
        /// </summary>
        internal const int SEC_OFFSET = SEC_INC;
        internal const int COMMON_SEC_PLUS_OFFSET = COMMON_SEC + SEC_OFFSET;

        internal const int TWO_SEC_OFFSETS =
                (SEC_OFFSET << 16) | SEC_OFFSET;  // 0x200020
        internal const int TWO_COMMON_SEC_PLUS_OFFSET =
                (COMMON_SEC_PLUS_OFFSET << 16) | COMMON_SEC_PLUS_OFFSET;

        internal const int LOWER_CASE = 8;  // case bits include this offset
        internal const int TWO_LOWER_CASES = (LOWER_CASE << 16) | LOWER_CASE;  // 0x80008

        internal const int COMMON_TER = 0;  // must add TER_OFFSET
        internal const int MAX_TER_AFTER = 7;  // 7 after common

        /// <summary>
        /// Lookup: Add this offset to tertiary weights, except for completely ignorable CEs.
        /// Must be greater than any special value, e.g., <see cref="MERGE_WEIGHT"/>.
        /// Must be greater than case bits as well, so that with combined case+tertiary weights
        /// plus the offset the tertiary bits does not spill over into the case bits.
        /// The exact value is not relevant for the format version.
        /// </summary>
        internal const int TER_OFFSET = SEC_OFFSET;
        internal const int COMMON_TER_PLUS_OFFSET = COMMON_TER + TER_OFFSET;

        internal const int TWO_TER_OFFSETS = (TER_OFFSET << 16) | TER_OFFSET;
        internal const int TWO_COMMON_TER_PLUS_OFFSET =
                (COMMON_TER_PLUS_OFFSET << 16) | COMMON_TER_PLUS_OFFSET;

        internal const int MERGE_WEIGHT = 3;
        internal const int EOS = 2;  // end of string
        internal const char BAIL_OUT = (char)1;

        /// <summary>
        /// Contraction result first word bits 8..0 contain the
        /// second contraction character, as a char index 0..<see cref="NUM_FAST_CHARS"/>-1.
        /// Each contraction list is terminated with a word containing <see cref="CONTR_CHAR_MASK"/>.
        /// </summary>
        internal const int CONTR_CHAR_MASK = 0x1ff;

        /// <summary>
        /// Contraction result first word bits 10..9 contain the result length:
        /// 1=bail out, 2=one mini CE, 3=two mini CEs
        /// </summary>
        internal const int CONTR_LENGTH_SHIFT = 9;

        /// <summary>
        /// Comparison return value when the regular comparison must be used.
        /// The exact value is not relevant for the format version.
        /// </summary>
        public const int BailOutResult = -2;

        internal static int GetCharIndex(char c)
        {
            if (c <= LatinMax)
            {
                return c;
            }
            else if (PUNCT_START <= c && c < PUNCT_LIMIT)
            {
                return c - (PUNCT_START - LatinLimit);
            }
            else
            {
                // Not a fast Latin character.
                // Note: U+FFFE & U+FFFF are forbidden in tailorings
                // and thus do not occur in any contractions.
                return -1;
            }
        }

        /// <summary>
        /// Computes the options value for the compare functions
        /// and writes the precomputed primary weights.
        /// Returns -1 if the Latin fastpath is not supported for the data and settings.
        /// The capacity must be <see cref="LatinLimit"/>.
        /// </summary>
        public static int GetOptions(CollationData data, CollationSettings settings,
                char[] primaries)
        {
            char[] header = data.fastLatinTableHeader;
            if (header == null) { return -1; }
            Debug.Assert((header[0] >> 8) == Version);
            if (primaries.Length != LatinLimit)
            {
                Debug.Assert(false);
                return -1;
            }

            int miniVarTop;
            if ((settings.Options & CollationSettings.AlternateMask) == 0)
            {
                // No mini primaries are variable, set a variableTop just below the
                // lowest long mini primary.
                miniVarTop = MIN_LONG - 1;
            }
            else
            {
                int headerLength = header[0] & 0xff;
                int i = 1 + settings.MaxVariable;
                if (i >= headerLength)
                {
                    return -1;  // variableTop >= digits, should not occur
                }
                miniVarTop = header[i];
            }

            bool digitsAreReordered = false;
            if (settings.HasReordering)
            {
                long prevStart = 0;
                long beforeDigitStart = 0;
                long digitStart = 0;
                long afterDigitStart = 0;
                for (int group = ReorderCodes.First;
                        group < ReorderCodes.First + CollationData.MAX_NUM_SPECIAL_REORDER_CODES;
                        ++group)
                {
                    long start = data.GetFirstPrimaryForGroup(group);
                    start = settings.Reorder(start);
                    if (group == ReorderCodes.Digit)
                    {
                        beforeDigitStart = prevStart;
                        digitStart = start;
                    }
                    else if (start != 0)
                    {
                        if (start < prevStart)
                        {
                            // The permutation affects the groups up to Latin.
                            return -1;
                        }
                        // In the future, there might be a special group between digits & Latin.
                        if (digitStart != 0 && afterDigitStart == 0 && prevStart == beforeDigitStart)
                        {
                            afterDigitStart = start;
                        }
                        prevStart = start;
                    }
                }
                long latinStart = data.GetFirstPrimaryForGroup(UScript.Latin);
                latinStart = settings.Reorder(latinStart);
                if (latinStart < prevStart)
                {
                    return -1;
                }
                if (afterDigitStart == 0)
                {
                    afterDigitStart = latinStart;
                }
                if (!(beforeDigitStart < digitStart && digitStart < afterDigitStart))
                {
                    digitsAreReordered = true;
                }
            }

            char[] table = data.FastLatinTable;  // skip the header
            for (int c = 0; c < LatinLimit; ++c)
            {
                int p = table[c];
                if (p >= MIN_SHORT)
                {
                    p &= SHORT_PRIMARY_MASK;
                }
                else if (p > miniVarTop)
                {
                    p &= LONG_PRIMARY_MASK;
                }
                else
                {
                    p = 0;
                }
                primaries[c] = (char)p;
            }
            if (digitsAreReordered || (settings.Options & CollationSettings.Numeric) != 0)
            {
                // Bail out for digits.
                for (int c = 0x30; c <= 0x39; ++c) { primaries[c] = (char)0; }
            }

            // Shift the miniVarTop above other options.
            return (miniVarTop << 16) | settings.Options;
        }

        public static int CompareUTF16(char[] table, char[] primaries, int options,
                ICharSequence left, ICharSequence right, int startIndex)
        {
            // This is a modified copy of CollationCompare.compareUpToQuaternary(),
            // optimized for common Latin text.
            // Keep them in sync!

            int variableTop = options >> 16;  // see getOptions()
            options &= 0xffff;  // needed for CollationSettings.getStrength() to work

            // Check for supported characters, fetch mini CEs, and compare primaries.
            int leftIndex = startIndex, rightIndex = startIndex;

            // Single mini CE or a pair.
            // The current mini CE is in the lower 16 bits, the next one is in the upper 16 bits.
            // If there is only one, then it is in the lower bits, and the upper bits are 0.
            int leftPair = 0, rightPair = 0;
            for (; ; )
            {
                // We fetch CEs until we get a non-ignorable primary or reach the end.
                while (leftPair == 0)
                {
                    if (leftIndex == left.Length)
                    {
                        leftPair = EOS;
                        break;
                    }
                    int c = left[leftIndex++];
                    if (c <= LatinMax)
                    {
                        leftPair = primaries[c];
                        if (leftPair != 0) { break; }
                        if (c <= 0x39 && c >= 0x30 && (options & CollationSettings.Numeric) != 0)
                        {
                            return BailOutResult;
                        }
                        leftPair = table[c];
                    }
                    else if (PUNCT_START <= c && c < PUNCT_LIMIT)
                    {
                        leftPair = table[c - PUNCT_START + LatinLimit];
                    }
                    else
                    {
                        leftPair = Lookup(table, c);
                    }
                    if (leftPair >= MIN_SHORT)
                    {
                        leftPair &= SHORT_PRIMARY_MASK;
                        break;
                    }
                    else if (leftPair > variableTop)
                    {
                        leftPair &= LONG_PRIMARY_MASK;
                        break;
                    }
                    else
                    {
                        long pairAndInc = NextPair(table, c, leftPair, left, leftIndex);
                        if (pairAndInc < 0)
                        {
                            ++leftIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        leftPair = (int)pairAndInc;
                        if (leftPair == BAIL_OUT) { return BailOutResult; }
                        leftPair = GetPrimaries(variableTop, leftPair);
                    }
                }

                while (rightPair == 0)
                {
                    if (rightIndex == right.Length)
                    {
                        rightPair = EOS;
                        break;
                    }
                    int c = right[rightIndex++];
                    if (c <= LatinMax)
                    {
                        rightPair = primaries[c];
                        if (rightPair != 0) { break; }
                        if (c <= 0x39 && c >= 0x30 && (options & CollationSettings.Numeric) != 0)
                        {
                            return BailOutResult;
                        }
                        rightPair = table[c];
                    }
                    else if (PUNCT_START <= c && c < PUNCT_LIMIT)
                    {
                        rightPair = table[c - PUNCT_START + LatinLimit];
                    }
                    else
                    {
                        rightPair = Lookup(table, c);
                    }
                    if (rightPair >= MIN_SHORT)
                    {
                        rightPair &= SHORT_PRIMARY_MASK;
                        break;
                    }
                    else if (rightPair > variableTop)
                    {
                        rightPair &= LONG_PRIMARY_MASK;
                        break;
                    }
                    else
                    {
                        long pairAndInc = NextPair(table, c, rightPair, right, rightIndex);
                        if (pairAndInc < 0)
                        {
                            ++rightIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        rightPair = (int)pairAndInc;
                        if (rightPair == BAIL_OUT) { return BailOutResult; }
                        rightPair = GetPrimaries(variableTop, rightPair);
                    }
                }

                if (leftPair == rightPair)
                {
                    if (leftPair == EOS) { break; }
                    leftPair = rightPair = 0;
                    continue;
                }
                int leftPrimary = leftPair & 0xffff;
                int rightPrimary = rightPair & 0xffff;
                if (leftPrimary != rightPrimary)
                {
                    // Return the primary difference.
                    return (leftPrimary < rightPrimary) ? Collation.Less : Collation.Greater;
                }
                if (leftPair == EOS) { break; }
                //leftPair >>>= 16;
                //rightPair >>>= 16;
                leftPair = leftPair.TripleShift(16);
                rightPair = rightPair.TripleShift(16);
            }
            // In the following, we need to re-fetch each character because we did not buffer the CEs,
            // but we know that the string is well-formed and
            // only contains supported characters and mappings.

            // We might skip the secondary level but continue with the case level
            // which is turned on separately.
            if (CollationSettings.GetStrength(options) >= CollationStrength.Secondary)
            {
                leftIndex = rightIndex = startIndex;
                leftPair = rightPair = 0;
                for (; ; )
                {
                    while (leftPair == 0)
                    {
                        if (leftIndex == left.Length)
                        {
                            leftPair = EOS;
                            break;
                        }
                        int c = left[leftIndex++];
                        if (c <= LatinMax)
                        {
                            leftPair = table[c];
                        }
                        else if (PUNCT_START <= c && c < PUNCT_LIMIT)
                        {
                            leftPair = table[c - PUNCT_START + LatinLimit];
                        }
                        else
                        {
                            leftPair = Lookup(table, c);
                        }
                        if (leftPair >= MIN_SHORT)
                        {
                            leftPair = GetSecondariesFromOneShortCE(leftPair);
                            break;
                        }
                        else if (leftPair > variableTop)
                        {
                            leftPair = COMMON_SEC_PLUS_OFFSET;
                            break;
                        }
                        else
                        {
                            long pairAndInc = NextPair(table, c, leftPair, left, leftIndex);
                            if (pairAndInc < 0)
                            {
                                ++leftIndex;
                                pairAndInc = ~pairAndInc;
                            }
                            leftPair = GetSecondaries(variableTop, (int)pairAndInc);
                        }
                    }

                    while (rightPair == 0)
                    {
                        if (rightIndex == right.Length)
                        {
                            rightPair = EOS;
                            break;
                        }
                        int c = right[rightIndex++];
                        if (c <= LatinMax)
                        {
                            rightPair = table[c];
                        }
                        else if (PUNCT_START <= c && c < PUNCT_LIMIT)
                        {
                            rightPair = table[c - PUNCT_START + LatinLimit];
                        }
                        else
                        {
                            rightPair = Lookup(table, c);
                        }
                        if (rightPair >= MIN_SHORT)
                        {
                            rightPair = GetSecondariesFromOneShortCE(rightPair);
                            break;
                        }
                        else if (rightPair > variableTop)
                        {
                            rightPair = COMMON_SEC_PLUS_OFFSET;
                            break;
                        }
                        else
                        {
                            long pairAndInc = NextPair(table, c, rightPair, right, rightIndex);
                            if (pairAndInc < 0)
                            {
                                ++rightIndex;
                                pairAndInc = ~pairAndInc;
                            }
                            rightPair = GetSecondaries(variableTop, (int)pairAndInc);
                        }
                    }

                    if (leftPair == rightPair)
                    {
                        if (leftPair == EOS) { break; }
                        leftPair = rightPair = 0;
                        continue;
                    }
                    int leftSecondary = leftPair & 0xffff;
                    int rightSecondary = rightPair & 0xffff;
                    if (leftSecondary != rightSecondary)
                    {
                        if ((options & CollationSettings.BackwardSecondary) != 0)
                        {
                            // Full support for backwards secondary requires backwards contraction matching
                            // and moving backwards between merge separators.
                            return BailOutResult;
                        }
                        return (leftSecondary < rightSecondary) ? Collation.Less : Collation.Greater;
                    }
                    if (leftPair == EOS) { break; }
                    //leftPair >>>= 16;
                    //rightPair >>>= 16;
                    leftPair = leftPair.TripleShift(16);
                    rightPair = rightPair.TripleShift(16);
                }
            }

            if ((options & CollationSettings.CaseLevel) != 0)
            {
                bool strengthIsPrimary = CollationSettings.GetStrength(options) == CollationStrength.Primary;
                leftIndex = rightIndex = startIndex;
                leftPair = rightPair = 0;
                for (; ; )
                {
                    while (leftPair == 0)
                    {
                        if (leftIndex == left.Length)
                        {
                            leftPair = EOS;
                            break;
                        }
                        int c = left[leftIndex++];
                        leftPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                        if (leftPair < MIN_LONG)
                        {
                            long pairAndInc = NextPair(table, c, leftPair, left, leftIndex);
                            if (pairAndInc < 0)
                            {
                                ++leftIndex;
                                pairAndInc = ~pairAndInc;
                            }
                            leftPair = (int)pairAndInc;
                        }
                        leftPair = GetCases(variableTop, strengthIsPrimary, leftPair);
                    }

                    while (rightPair == 0)
                    {
                        if (rightIndex == right.Length)
                        {
                            rightPair = EOS;
                            break;
                        }
                        int c = right[rightIndex++];
                        rightPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                        if (rightPair < MIN_LONG)
                        {
                            long pairAndInc = NextPair(table, c, rightPair, right, rightIndex);
                            if (pairAndInc < 0)
                            {
                                ++rightIndex;
                                pairAndInc = ~pairAndInc;
                            }
                            rightPair = (int)pairAndInc;
                        }
                        rightPair = GetCases(variableTop, strengthIsPrimary, rightPair);
                    }

                    if (leftPair == rightPair)
                    {
                        if (leftPair == EOS) { break; }
                        leftPair = rightPair = 0;
                        continue;
                    }
                    int leftCase = leftPair & 0xffff;
                    int rightCase = rightPair & 0xffff;
                    if (leftCase != rightCase)
                    {
                        if ((options & CollationSettings.UpperFirst) == 0)
                        {
                            return (leftCase < rightCase) ? Collation.Less : Collation.Greater;
                        }
                        else
                        {
                            return (leftCase < rightCase) ? Collation.Greater : Collation.Less;
                        }
                    }
                    if (leftPair == EOS) { break; }
                    //leftPair >>>= 16;
                    //rightPair >>>= 16;
                    leftPair = leftPair.TripleShift(16);
                    rightPair = rightPair.TripleShift(16);
                }
            }
            if (CollationSettings.GetStrength(options) <= CollationStrength.Secondary) { return Collation.Equal; }

            // Remove the case bits from the tertiary weight when caseLevel is on or caseFirst is off.
            bool withCaseBits = CollationSettings.IsTertiaryWithCaseBits(options);

            leftIndex = rightIndex = startIndex;
            leftPair = rightPair = 0;
            for (; ; )
            {
                while (leftPair == 0)
                {
                    if (leftIndex == left.Length)
                    {
                        leftPair = EOS;
                        break;
                    }
                    int c = left[leftIndex++];
                    leftPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                    if (leftPair < MIN_LONG)
                    {
                        long pairAndInc = NextPair(table, c, leftPair, left, leftIndex);
                        if (pairAndInc < 0)
                        {
                            ++leftIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        leftPair = (int)pairAndInc;
                    }
                    leftPair = GetTertiaries(variableTop, withCaseBits, leftPair);
                }

                while (rightPair == 0)
                {
                    if (rightIndex == right.Length)
                    {
                        rightPair = EOS;
                        break;
                    }
                    int c = right[rightIndex++];
                    rightPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                    if (rightPair < MIN_LONG)
                    {
                        long pairAndInc = NextPair(table, c, rightPair, right, rightIndex);
                        if (pairAndInc < 0)
                        {
                            ++rightIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        rightPair = (int)pairAndInc;
                    }
                    rightPair = GetTertiaries(variableTop, withCaseBits, rightPair);
                }

                if (leftPair == rightPair)
                {
                    if (leftPair == EOS) { break; }
                    leftPair = rightPair = 0;
                    continue;
                }
                int leftTertiary = leftPair & 0xffff;
                int rightTertiary = rightPair & 0xffff;
                if (leftTertiary != rightTertiary)
                {
                    if (CollationSettings.SortsTertiaryUpperCaseFirst(options))
                    {
                        // Pass through EOS and MERGE_WEIGHT
                        // and keep real tertiary weights larger than the MERGE_WEIGHT.
                        // Tertiary CEs (secondary ignorables) are not supported in fast Latin.
                        if (leftTertiary > MERGE_WEIGHT)
                        {
                            leftTertiary ^= CASE_MASK;
                        }
                        if (rightTertiary > MERGE_WEIGHT)
                        {
                            rightTertiary ^= CASE_MASK;
                        }
                    }
                    return (leftTertiary < rightTertiary) ? Collation.Less : Collation.Greater;
                }
                if (leftPair == EOS) { break; }
                //leftPair >>>= 16;
                //rightPair >>>= 16;
                leftPair = leftPair.TripleShift(16);
                rightPair = rightPair.TripleShift(16);
            }
            if (CollationSettings.GetStrength(options) <= CollationStrength.Tertiary) { return Collation.Equal; }

            leftIndex = rightIndex = startIndex;
            leftPair = rightPair = 0;
            for (; ; )
            {
                while (leftPair == 0)
                {
                    if (leftIndex == left.Length)
                    {
                        leftPair = EOS;
                        break;
                    }
                    int c = left[leftIndex++];
                    leftPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                    if (leftPair < MIN_LONG)
                    {
                        long pairAndInc = NextPair(table, c, leftPair, left, leftIndex);
                        if (pairAndInc < 0)
                        {
                            ++leftIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        leftPair = (int)pairAndInc;
                    }
                    leftPair = GetQuaternaries(variableTop, leftPair);
                }

                while (rightPair == 0)
                {
                    if (rightIndex == right.Length)
                    {
                        rightPair = EOS;
                        break;
                    }
                    int c = right[rightIndex++];
                    rightPair = (c <= LatinMax) ? table[c] : Lookup(table, c);
                    if (rightPair < MIN_LONG)
                    {
                        long pairAndInc = NextPair(table, c, rightPair, right, rightIndex);
                        if (pairAndInc < 0)
                        {
                            ++rightIndex;
                            pairAndInc = ~pairAndInc;
                        }
                        rightPair = (int)pairAndInc;
                    }
                    rightPair = GetQuaternaries(variableTop, rightPair);
                }

                if (leftPair == rightPair)
                {
                    if (leftPair == EOS) { break; }
                    leftPair = rightPair = 0;
                    continue;
                }
                int leftQuaternary = leftPair & 0xffff;
                int rightQuaternary = rightPair & 0xffff;
                if (leftQuaternary != rightQuaternary)
                {
                    return (leftQuaternary < rightQuaternary) ? Collation.Less : Collation.Greater;
                }
                if (leftPair == EOS) { break; }
                //leftPair >>>= 16;
                //rightPair >>>= 16;
                leftPair = leftPair.TripleShift(16);
                rightPair = rightPair.TripleShift(16);
            }
            return Collation.Equal;
        }

        private static int Lookup(char[] table, int c)
        {
            Debug.Assert(c > LatinMax);
            if (PUNCT_START <= c && c < PUNCT_LIMIT)
            {
                return table[c - PUNCT_START + LatinLimit];
            }
            else if (c == 0xfffe)
            {
                return MERGE_WEIGHT;
            }
            else if (c == 0xffff)
            {
                return MAX_SHORT | COMMON_SEC | LOWER_CASE | COMMON_TER;
            }
            else
            {
                return BAIL_OUT;
            }
        }

        /// <summary>
        /// .NET returns a negative result (use the '~' operator) if sIndex is to be incremented.
        /// C++ modifies sIndex.
        /// </summary>
        private static long NextPair(char[] table, int c, int ce, ICharSequence s16, int sIndex)
        {
            if (ce >= MIN_LONG || ce < CONTRACTION)
            {
                return ce;  // simple or special mini CE
            }
            else if (ce >= EXPANSION)
            {
                int index = NUM_FAST_CHARS + (ce & INDEX_MASK);
                return ((long)table[index + 1] << 16) | table[index];
            }
            else /* ce >= CONTRACTION */
            {
                // Contraction list: Default mapping followed by
                // 0 or more single-character contraction suffix mappings.
                int index = NUM_FAST_CHARS + (ce & INDEX_MASK);
                bool inc = false;  // true if the next char is consumed.
                if (sIndex != s16.Length)
                {
                    // Read the next character.
                    int c2;
                    int nextIndex = sIndex;
                    c2 = s16[nextIndex++];
                    if (c2 > LatinMax)
                    {
                        if (PUNCT_START <= c2 && c2 < PUNCT_LIMIT)
                        {
                            c2 = c2 - PUNCT_START + LatinLimit;  // 2000..203F -> 0180..01BF
                        }
                        else if (c2 == 0xfffe || c2 == 0xffff)
                        {
                            c2 = -1;  // U+FFFE & U+FFFF cannot occur in contractions.
                        }
                        else
                        {
                            return BAIL_OUT;
                        }
                    }
                    // Look for the next character in the contraction suffix list,
                    // which is in ascending order of single suffix characters.
                    int i = index;
                    int head = table[i];  // first skip the default mapping
                    int x;
                    do
                    {
                        i += head >> CONTR_LENGTH_SHIFT;
                        head = table[i];
                        x = head & CONTR_CHAR_MASK;
                    } while (x < c2);
                    if (x == c2)
                    {
                        index = i;
                        inc = true;
                    }
                }
                // Return the CE or CEs for the default or contraction mapping.
                int length = table[index] >> CONTR_LENGTH_SHIFT;
                if (length == 1)
                {
                    return BAIL_OUT;
                }
                ce = table[index + 1];
                long result;
                if (length == 2)
                {
                    result = ce;
                }
                else
                {
                    result = ((long)table[index + 2] << 16) | (uint)ce;
                }
                return inc ? ~result : result;
            }
        }

        private static int GetPrimaries(int variableTop, int pair)
        {
            int ce = pair & 0xffff;
            if (ce >= MIN_SHORT) { return pair & TWO_SHORT_PRIMARIES_MASK; }
            if (ce > variableTop) { return pair & TWO_LONG_PRIMARIES_MASK; }
            if (ce >= MIN_LONG) { return 0; }  // variable
            return pair;  // special mini CE
        }

        private static int GetSecondariesFromOneShortCE(int ce)
        {
            ce &= SECONDARY_MASK;
            if (ce < MIN_SEC_HIGH)
            {
                return ce + SEC_OFFSET;
            }
            else
            {
                return ((ce + SEC_OFFSET) << 16) | COMMON_SEC_PLUS_OFFSET;
            }
        }

        private static int GetSecondaries(int variableTop, int pair)
        {
            if (pair <= 0xffff)
            {
                // one mini CE
                if (pair >= MIN_SHORT)
                {
                    pair = GetSecondariesFromOneShortCE(pair);
                }
                else if (pair > variableTop)
                {
                    pair = COMMON_SEC_PLUS_OFFSET;
                }
                else if (pair >= MIN_LONG)
                {
                    pair = 0;  // variable
                }
                // else special mini CE
            }
            else
            {
                int ce = pair & 0xffff;
                if (ce >= MIN_SHORT)
                {
                    pair = (pair & TWO_SECONDARIES_MASK) + TWO_SEC_OFFSETS;
                }
                else if (ce > variableTop)
                {
                    pair = TWO_COMMON_SEC_PLUS_OFFSET;
                }
                else
                {
                    Debug.Assert(ce >= MIN_LONG);
                    pair = 0;  // variable
                }
            }
            return pair;
        }

        private static int GetCases(int variableTop, bool strengthIsPrimary, int pair)
        {
            // Primary+caseLevel: Ignore case level weights of primary ignorables.
            // Otherwise: Ignore case level weights of secondary ignorables.
            // For details see the comments in the CollationCompare class.
            // Tertiary CEs (secondary ignorables) are not supported in fast Latin.
            if (pair <= 0xffff)
            {
                // one mini CE
                if (pair >= MIN_SHORT)
                {
                    // A high secondary weight means we really have two CEs,
                    // a primary CE and a secondary CE.
                    int ce = pair;
                    pair &= CASE_MASK;  // explicit weight of primary CE
                    if (!strengthIsPrimary && (ce & SECONDARY_MASK) >= MIN_SEC_HIGH)
                    {
                        pair |= LOWER_CASE << 16;  // implied weight of secondary CE
                    }
                }
                else if (pair > variableTop)
                {
                    pair = LOWER_CASE;
                }
                else if (pair >= MIN_LONG)
                {
                    pair = 0;  // variable
                }
                // else special mini CE
            }
            else
            {
                // two mini CEs, same primary groups, neither expands like above
                int ce = pair & 0xffff;
                if (ce >= MIN_SHORT)
                {
                    if (strengthIsPrimary && (pair & (SHORT_PRIMARY_MASK << 16)) == 0)
                    {
                        pair &= CASE_MASK;
                    }
                    else
                    {
                        pair &= TWO_CASES_MASK;
                    }
                }
                else if (ce > variableTop)
                {
                    pair = TWO_LOWER_CASES;
                }
                else
                {
                    Debug.Assert(ce >= MIN_LONG);
                    pair = 0;  // variable
                }
            }
            return pair;
        }

        private static int GetTertiaries(int variableTop, bool withCaseBits, int pair)
        {
            if (pair <= 0xffff)
            {
                // one mini CE
                if (pair >= MIN_SHORT)
                {
                    // A high secondary weight means we really have two CEs,
                    // a primary CE and a secondary CE.
                    int ce = pair;
                    if (withCaseBits)
                    {
                        pair = (pair & CASE_AND_TERTIARY_MASK) + TER_OFFSET;
                        if ((ce & SECONDARY_MASK) >= MIN_SEC_HIGH)
                        {
                            pair |= (LOWER_CASE | COMMON_TER_PLUS_OFFSET) << 16;
                        }
                    }
                    else
                    {
                        pair = (pair & TERTIARY_MASK) + TER_OFFSET;
                        if ((ce & SECONDARY_MASK) >= MIN_SEC_HIGH)
                        {
                            pair |= COMMON_TER_PLUS_OFFSET << 16;
                        }
                    }
                }
                else if (pair > variableTop)
                {
                    pair = (pair & TERTIARY_MASK) + TER_OFFSET;
                    if (withCaseBits)
                    {
                        pair |= LOWER_CASE;
                    }
                }
                else if (pair >= MIN_LONG)
                {
                    pair = 0;  // variable
                }
                // else special mini CE
            }
            else
            {
                // two mini CEs, same primary groups, neither expands like above
                int ce = pair & 0xffff;
                if (ce >= MIN_SHORT)
                {
                    if (withCaseBits)
                    {
                        pair &= TWO_CASES_MASK | TWO_TERTIARIES_MASK;
                    }
                    else
                    {
                        pair &= TWO_TERTIARIES_MASK;
                    }
                    pair += TWO_TER_OFFSETS;
                }
                else if (ce > variableTop)
                {
                    pair = (pair & TWO_TERTIARIES_MASK) + TWO_TER_OFFSETS;
                    if (withCaseBits)
                    {
                        pair |= TWO_LOWER_CASES;
                    }
                }
                else
                {
                    Debug.Assert(ce >= MIN_LONG);
                    pair = 0;  // variable
                }
            }
            return pair;
        }

        private static int GetQuaternaries(int variableTop, int pair)
        {
            // Return the primary weight of a variable CE,
            // or the maximum primary weight for a non-variable, not-completely-ignorable CE.
            if (pair <= 0xffff)
            {
                // one mini CE
                if (pair >= MIN_SHORT)
                {
                    // A high secondary weight means we really have two CEs,
                    // a primary CE and a secondary CE.
                    if ((pair & SECONDARY_MASK) >= MIN_SEC_HIGH)
                    {
                        pair = TWO_SHORT_PRIMARIES_MASK;
                    }
                    else
                    {
                        pair = SHORT_PRIMARY_MASK;
                    }
                }
                else if (pair > variableTop)
                {
                    pair = SHORT_PRIMARY_MASK;
                }
                else if (pair >= MIN_LONG)
                {
                    pair &= LONG_PRIMARY_MASK;  // variable
                }
                // else special mini CE
            }
            else
            {
                // two mini CEs, same primary groups, neither expands like above
                int ce = pair & 0xffff;
                if (ce > variableTop)
                {
                    pair = TWO_SHORT_PRIMARIES_MASK;
                }
                else
                {
                    Debug.Assert(ce >= MIN_LONG);
                    pair &= TWO_LONG_PRIMARIES_MASK;  // variable
                }
            }
            return pair;
        }

        //private CollationFastLatin() { }  // no constructor
    }
}
