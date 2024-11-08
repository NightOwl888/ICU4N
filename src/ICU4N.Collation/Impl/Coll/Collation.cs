using J2N;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Sort key levels.
    /// </summary>
    public enum CollationSortKeyLevel
    {
        /// <summary>Unspecified level.</summary>
        // ICU4N specific: This was NO_LEVEL in ICU4J
        Unspecified = 0,

        Primary = 1,
        Secondary = 2,
        Case = 3,
        Tertiary = 4,
        Quaternary = 5,
        Identical = 6,

        /// <summary>Beyond sort key bytes.</summary>
        Zero = 7
    }

    /// <summary>
    /// Collation v2 basic definitions and static helper functions.
    /// <para/>
    /// Data structures except for expansion tables store 32-bit CEs which are
    /// either specials (see tags below) or are compact forms of 64-bit CEs.
    /// </summary>
    public sealed class Collation
    {
        /// <summary>
        /// UChar32 U_SENTINEL.
        /// </summary>
        // TODO: Create a common, public constant?
        public const int SentinelCodePoint = -1; // ICU4N specific - renamed from SENTINEL_CP

        // ICU4C compare() API returns enum UCollationResult values (with UCOL_ prefix).
        // ICU4N just returns int. We use these constants for ease of porting.
        public const int Less = -1;
        public const int Equal = 0;
        public const int Greater = 1;

        // Special sort key bytes for all levels.
        public const int TerminatorByte = 0;
        public const int LevelSeparatorByte = 1;

        /// <summary>The secondary/tertiary lower limit for tailoring before any root elements.</summary>
        internal const int BEFORE_WEIGHT16 = 0x100;

        /// <summary>
        /// Merge-sort-key separator.
        /// Same as the unique primary and identical-level weights of U+FFFE.
        /// Must not be used as primary compression low terminator.
        /// Otherwise usable.
        /// </summary>
        public const int MergeSeparatorByte = 2;
        public const long MergeSeparatorPrimary = 0x02000000;  // U+FFFE
        internal const int MERGE_SEPARATOR_CE32 = 0x02000505;  // U+FFFE

        /// <summary>
        /// Primary compression low terminator, must be greater than <see cref="MergeSeparatorByte"/>.
        /// Reserved value in primary second byte if the lead byte is compressible.
        /// Otherwise usable in all CE weight bytes.
        /// </summary>
        public const int PrimaryCompressionLowByte = 3;
        /// <summary>
        /// Primary compression high terminator.
        /// Reserved value in primary second byte if the lead byte is compressible.
        /// Otherwise usable in all CE weight bytes.
        /// </summary>
        public const int PrimaryCompressionHighByte = 0xff;

        /// <summary>Default secondary/tertiary weight lead byte.</summary>
        internal const int COMMON_BYTE = 5;
        public const int CommonWeight16 = 0x0500;
        /// <summary>Middle 16 bits of a CE with a common secondary weight.</summary>
        internal const int COMMON_SECONDARY_CE = 0x05000000;
        /// <summary>Lower 16 bits of a CE with a common tertiary weight.</summary>
        internal const int COMMON_TERTIARY_CE = 0x0500;
        /// <summary>Lower 32 bits of a CE with common secondary and tertiary weights.</summary>
        public const int CommonSecondaryAndTertiaryCE = 0x05000500;

        internal const int SECONDARY_MASK = unchecked((int)0xffff0000);
        public const int CaseMask = 0xc000;
        internal const int SECONDARY_AND_CASE_MASK = SECONDARY_MASK | CaseMask;
        /// <summary>Only the 2*6 bits for the pure tertiary weight.</summary>
        public const int OnlyTertiaryMask = 0x3f3f;
        /// <summary>Only the secondary &amp; tertiary bits; no case, no quaternary.</summary>
        internal const int ONLY_SEC_TER_MASK = SECONDARY_MASK | OnlyTertiaryMask;
        /// <summary>Case bits and tertiary bits.</summary>
        internal const int CASE_AND_TERTIARY_MASK = CaseMask | OnlyTertiaryMask;
        public const int QuaternaryMask = 0xc0;
        /// <summary>Case bits and quaternary bits.</summary>
        public const int CaseAndQuaternaryMask = CaseMask | QuaternaryMask;

        internal const int UNASSIGNED_IMPLICIT_BYTE = 0xfe;  // compressible

        /// <summary>
        /// First unassigned: AlphabeticIndex overflow boundary.
        /// We want a 3-byte primary so that it fits into the root elements table.
        /// <para/>
        /// This 3-byte primary will not collide with
        /// any unassigned-implicit 4-byte primaries because
        /// the first few hundred Unicode code points all have real mappings.
        /// </summary>
        internal const long FIRST_UNASSIGNED_PRIMARY = 0xfe040200L;

        internal const int TRAIL_WEIGHT_BYTE = 0xff;  // not compressible
        internal const long FIRST_TRAILING_PRIMARY = 0xff020200L;  // [first trailing]
        public const long MaxPrimary = 0xffff0000L;  // U+FFFF
        internal const int MAX_REGULAR_CE32 = unchecked((int)0xffff0505);  // U+FFFF

        // CE32 value for U+FFFD as well as illegal UTF-8 byte sequences (which behave like U+FFFD).
        // We use the third-highest primary weight for U+FFFD (as in UCA 6.3+).
        public const long FFFD_Primary = MaxPrimary - 0x20000;
        internal const int FFFD_CE32 = MAX_REGULAR_CE32 - 0x20000;

        /// <summary>
        /// A CE32 is special if its low byte is this or greater.
        /// Impossible case bits 11 mark special CE32s.
        /// This value itself is used to indicate a fallback to the base collator.
        /// </summary>
        internal const int SPECIAL_CE32_LOW_BYTE = 0xc0;
        internal const int FALLBACK_CE32 = SPECIAL_CE32_LOW_BYTE;
        /// <summary>
        /// Low byte of a long-primary special CE32.
        /// </summary>
        internal const int LONG_PRIMARY_CE32_LOW_BYTE = 0xc1;  // SPECIAL_CE32_LOW_BYTE | LONG_PRIMARY_TAG

        internal const int UNASSIGNED_CE32 = unchecked((int)0xffffffff);  // Compute an unassigned-implicit CE.

        internal const int NO_CE32 = 1;

        /// <summary>No CE: End of input. Only used in runtime code, not stored in data.</summary>
        internal const long NO_CE_PRIMARY = 1;  // not a left-adjusted weight
        internal const int NO_CE_WEIGHT16 = 0x0100;  // weight of LEVEL_SEPARATOR_BYTE
        public const long NoCE = 0x101000100L;  // NO_CE_PRIMARY, NO_CE_WEIGHT16, NO_CE_WEIGHT16

        // ICU4N specific - moved sort key levels to enum named CollationSortKeyLevel

        /// <summary>
        /// Sort key level flags: xx_FLAG = 1 &lt;&lt; xx_LEVEL.
        /// In Java, use enum Level with flag() getters, or use EnumSet rather than hand-made bit sets.
        /// </summary>
        internal const int NO_LEVEL_FLAG = 1;
        internal const int PRIMARY_LEVEL_FLAG = 2;
        internal const int SECONDARY_LEVEL_FLAG = 4;
        internal const int CASE_LEVEL_FLAG = 8;
        internal const int TERTIARY_LEVEL_FLAG = 0x10;
        internal const int QUATERNARY_LEVEL_FLAG = 0x20;
        internal const int IDENTICAL_LEVEL_FLAG = 0x40;
        internal const int ZERO_LEVEL_FLAG = 0x80;

        /**
         * Special-CE32 tags, from bits 3..0 of a special 32-bit CE.
         * Bits 31..8 are available for tag-specific data.
         * Bits  5..4: Reserved. May be used in the future to indicate lccc!=0 and tccc!=0.
         */

        /// <summary>
        /// Fall back to the base collator.
        /// This is the tag value in <see cref="SPECIAL_CE32_LOW_BYTE"/> and <see cref="FALLBACK_CE32"/>.
        /// Bits 31..8: Unused, 0.
        /// </summary>
        internal const int FALLBACK_TAG = 0;
        /// <summary>
        /// Long-primary CE with <see cref="CommonSecondaryAndTertiaryCE"/>.
        /// Bits 31..8: Three-byte primary.
        /// </summary>
        internal const int LONG_PRIMARY_TAG = 1;
        /// <summary>
        /// Long-secondary CE with zero primary.
        /// Bits 31..16: Secondary weight.
        /// Bits 15.. 8: Tertiary weight.
        /// </summary>
        internal const int LONG_SECONDARY_TAG = 2;
        /// <summary>
        /// Unused.
        /// May be used in the future for single-byte secondary CEs (SHORT_SECONDARY_TAG),
        /// storing the secondary in bits 31..24, the ccc in bits 23..16,
        /// and the tertiary in bits 15..8.
        /// </summary>
        internal const int RESERVED_TAG_3 = 3;
        /// <summary>
        /// Latin mini expansions of two simple CEs [pp, 05, tt] [00, ss, 05].
        /// Bits 31..24: Single-byte primary weight pp of the first CE.
        /// Bits 23..16: Tertiary weight tt of the first CE.
        /// Bits 15.. 8: Secondary weight ss of the second CE.
        /// </summary>
        internal const int LATIN_EXPANSION_TAG = 4;
        /// <summary>
        /// Points to one or more simple/long-primary/long-secondary 32-bit CE32s.
        /// Bits 31..13: Index into int table.
        /// Bits 12.. 8: Length=1..31.
        /// </summary>
        internal const int EXPANSION32_TAG = 5;
        /// <summary>
        /// Points to one or more 64-bit CEs.
        /// Bits 31..13: Index into CE table.
        /// Bits 12.. 8: Length=1..31.
        /// </summary>
        internal const int EXPANSION_TAG = 6;
        /// <summary>
        /// Builder data, used only in the CollationDataBuilder, not in runtime data.
        /// <para/>
        /// If bit 8 is 0: Builder context, points to a list of context-sensitive mappings.
        /// Bits 31..13: Index to the builder's list of ConditionalCE32 for this character.
        /// Bits 12.. 9: Unused, 0.
        /// <para/>
        /// If bit 8 is 1 (IS_BUILDER_JAMO_CE32): Builder-only jamoCE32 value.
        /// The builder fetches the Jamo CE32 from the trie.
        /// Bits 31..13: Jamo code point.
        /// Bits 12.. 9: Unused, 0.
        /// </summary>
        internal const int BUILDER_DATA_TAG = 7;
        /// <summary>
        /// Points to prefix trie.
        /// Bits 31..13: Index into prefix/contraction data.
        /// Bits 12.. 8: Unused, 0.
        /// </summary>
        internal const int PREFIX_TAG = 8;
        /// <summary>
        /// Points to contraction data.
        /// Bits 31..13: Index into prefix/contraction data.
        /// Bits 12..11: Unused, 0.
        /// Bit      10: CONTRACT_TRAILING_CCC flag.
        /// Bit       9: CONTRACT_NEXT_CCC flag.
        /// Bit       8: CONTRACT_SINGLE_CP_NO_MATCH flag.
        /// </summary>
        internal const int CONTRACTION_TAG = 9;
        /// <summary>
        /// Decimal digit.
        /// Bits 31..13: Index into int table for non-numeric-collation CE32.
        /// Bit      12: Unused, 0.
        /// Bits 11.. 8: Digit value 0..9.
        /// </summary>
        internal const int DIGIT_TAG = 10;
        /// <summary>
        /// Tag for U+0000, for moving the NUL-termination handling
        /// from the regular fastpath into specials-handling code.
        /// Bits 31..8: Unused, 0.
        /// </summary>
        internal const int U0000_TAG = 11;
        /// <summary>
        /// Tag for a Hangul syllable.
        /// Bits 31..9: Unused, 0.
        /// Bit      8: HANGUL_NO_SPECIAL_JAMO flag.
        /// </summary>
        internal const int HANGUL_TAG = 12;
        /// <summary>
        /// Tag for a lead surrogate code unit.
        /// Optional optimization for UTF-16 string processing.
        /// Bits 31..10: Unused, 0.
        ///       9.. 8: =0: All associated supplementary code points are unassigned-implict.
        ///              =1: All associated supplementary code points fall back to the base data.
        ///              else: (Normally 2) Look up the data for the supplementary code point.
        /// </summary>
        internal const int LEAD_SURROGATE_TAG = 13;
        /// <summary>
        /// Tag for CEs with primary weights in code point order.
        /// Bits 31..13: Index into CE table, for one data "CE".
        /// Bits 12.. 8: Unused, 0.
        /// <para/>
        /// This data "CE" has the following bit fields:
        /// Bits 63..32: Three-byte primary pppppp00.
        ///      31.. 8: Start/base code point of the in-order range.
        ///           7: Flag isCompressible primary.
        ///       6.. 0: Per-code point primary-weight increment.          
        /// </summary>
        internal const int OFFSET_TAG = 14;
        /// <summary>
        /// Implicit CE tag. Compute an unassigned-implicit CE.
        /// All bits are set (UNASSIGNED_CE32=0xffffffff).
        /// </summary>
        internal const int IMPLICIT_TAG = 15;

        internal static bool IsAssignedCE32(int ce32)
        {
            return ce32 != FALLBACK_CE32 && ce32 != UNASSIGNED_CE32;
        }

        /// <summary>
        /// We limit the number of CEs in an expansion
        /// so that we can use a small number of length bits in the data structure,
        /// and so that an implementation can copy CEs at runtime without growing a destination buffer.
        /// </summary>
        internal const int MAX_EXPANSION_LENGTH = 31;
        internal const int MAX_INDEX = 0x7ffff;
        /// <summary>
        /// Set if there is no match for the single (no-suffix) character itself.
        /// This is only possible if there is a prefix.
        /// In this case, discontiguous contraction matching cannot add combining marks
        /// starting from an empty suffix.
        /// The default CE32 is used anyway if there is no suffix match.
        /// </summary>
        internal const int CONTRACT_SINGLE_CP_NO_MATCH = 0x100;
        /// <summary>Set if the first character of every contraction suffix has lccc!=0.</summary>
        internal const int CONTRACT_NEXT_CCC = 0x200;
        /// <summary>Set if any contraction suffix ends with lccc!=0.</summary>
        internal const int CONTRACT_TRAILING_CCC = 0x400;

        /// <summary>For HANGUL_TAG: None of its Jamo CE32s <see cref="IsSpecialCE32(int)"/>.</summary>
        internal const int HANGUL_NO_SPECIAL_JAMO = 0x100;

        internal const int LEAD_ALL_UNASSIGNED = 0;
        internal const int LEAD_ALL_FALLBACK = 0x100;
        internal const int LEAD_MIXED = 0x200;
        internal const int LEAD_TYPE_MASK = 0x300;

        internal static int MakeLongPrimaryCE32(long p) { return (int)(p | LONG_PRIMARY_CE32_LOW_BYTE); }

        /// <summary>Turns the long-primary CE32 into a primary weight pppppp00.</summary>
        internal static long PrimaryFromLongPrimaryCE32(int ce32)
        {
            return (long)ce32 & 0xffffff00L;
        }
        internal static long CeFromLongPrimaryCE32(int ce32)
        {
            return ((long)(ce32 & 0xffffff00) << 32) | CommonSecondaryAndTertiaryCE;
        }

        internal static int MakeLongSecondaryCE32(int lower32)
        {
            return lower32 | SPECIAL_CE32_LOW_BYTE | LONG_SECONDARY_TAG;
        }
        internal static long CeFromLongSecondaryCE32(int ce32)
        {
            return (long)ce32 & 0xffffff00L;
        }

        /// <summary>Makes a special CE32 with <paramref name="tag"/>, <paramref name="index"/> and <paramref name="length"/>.</summary>
        internal static int MakeCE32FromTagIndexAndLength(int tag, int index, int length)
        {
            return (index << 13) | (length << 8) | SPECIAL_CE32_LOW_BYTE | tag;
        }
        /// <summary>Makes a special CE32 with only <paramref name="tag"/> and <paramref name="index"/>.</summary>
        internal static int MakeCE32FromTagAndIndex(int tag, int index)
        {
            return (index << 13) | SPECIAL_CE32_LOW_BYTE | tag;
        }

        internal static bool IsSpecialCE32(int ce32)
        {
            return (ce32 & 0xff) >= SPECIAL_CE32_LOW_BYTE;
        }

        internal static int TagFromCE32(int ce32)
        {
            return ce32 & 0xf;
        }

        internal static bool HasCE32Tag(int ce32, int tag)
        {
            return IsSpecialCE32(ce32) && TagFromCE32(ce32) == tag;
        }

        internal static bool IsLongPrimaryCE32(int ce32)
        {
            return HasCE32Tag(ce32, LONG_PRIMARY_TAG);
        }

        internal static bool IsSimpleOrLongCE32(int ce32)
        {
            return !IsSpecialCE32(ce32) ||
                    TagFromCE32(ce32) == LONG_PRIMARY_TAG ||
                    TagFromCE32(ce32) == LONG_SECONDARY_TAG;
        }

        /// <returns>true if the ce32 yields one or more CEs without further data lookups.</returns>
        internal static bool IsSelfContainedCE32(int ce32)
        {
            return !IsSpecialCE32(ce32) ||
                    TagFromCE32(ce32) == LONG_PRIMARY_TAG ||
                    TagFromCE32(ce32) == LONG_SECONDARY_TAG ||
                    TagFromCE32(ce32) == LATIN_EXPANSION_TAG;
        }

        internal static bool IsPrefixCE32(int ce32)
        {
            return HasCE32Tag(ce32, PREFIX_TAG);
        }

        internal static bool IsContractionCE32(int ce32)
        {
            return HasCE32Tag(ce32, CONTRACTION_TAG);
        }

        internal static bool Ce32HasContext(int ce32)
        {
            return IsSpecialCE32(ce32) &&
                    (TagFromCE32(ce32) == PREFIX_TAG ||
                    TagFromCE32(ce32) == CONTRACTION_TAG);
        }

        /// <summary>
        /// Get the first of the two Latin-expansion CEs encoded in ce32.
        /// </summary>
        /// <seealso cref="LATIN_EXPANSION_TAG"/>
        internal static long LatinCE0FromCE32(int ce32)
        {
            return ((long)(ce32 & 0xff000000) << 32) | COMMON_SECONDARY_CE | (uint)((ce32 & 0xff0000) >> 8);
        }

        /// <summary>
        /// Get the second of the two Latin-expansion CEs encoded in ce32.
        /// </summary>
        /// <seealso cref="LATIN_EXPANSION_TAG"/>
        internal static long LatinCE1FromCE32(int ce32)
        {
            return (((long)ce32 & 0xff00) << 16) | COMMON_TERTIARY_CE;
        }

        /// <summary>
        /// Returns the data index from a special CE32.
        /// </summary>
        internal static int IndexFromCE32(int ce32)
        {
            return ce32 >>> 13;
        }

        /// <summary>
        /// Returns the data length from a ce32.
        /// </summary>
        internal static int LengthFromCE32(int ce32)
        {
            return (ce32 >> 8) & 31;
        }

        /// <summary>
        /// Returns the digit value from a <see cref="DIGIT_TAG"/> ce32.
        /// </summary>
        internal static char DigitFromCE32(int ce32)
        {
            return (char)((ce32 >> 8) & 0xf);
        }

        /// <summary>Returns a 64-bit CE from a simple CE32 (not special).</summary>
        internal static long CeFromSimpleCE32(int ce32)
        {
            // normal form ppppsstt -> pppp0000ss00tt00
            Debug.Assert((ce32 & 0xff) < SPECIAL_CE32_LOW_BYTE);
            return ((long)(ce32 & 0xffff0000) << 32) | (uint)((long)(ce32 & 0xff00) << 16) | (uint)((ce32 & 0xff) << 8);
        }

        /// <summary>Returns a 64-bit CE from a simple/long-primary/long-secondary CE32.</summary>
        internal static long CeFromCE32(int ce32)
        {
            int tertiary = ce32 & 0xff;
            if (tertiary < SPECIAL_CE32_LOW_BYTE)
            {
                // normal form ppppsstt -> pppp0000ss00tt00
                return ((long)(ce32 & 0xffff0000) << 32) | ((long)(ce32 & 0xff00) << 16) | (uint)(tertiary << 8);
            }
            else
            {
                ce32 -= tertiary;
                if ((tertiary & 0xf) == LONG_PRIMARY_TAG)
                {
                    // long-primary form ppppppC1 -> pppppp00050000500
                    return ((long)ce32 << 32) | CommonSecondaryAndTertiaryCE;
                }
                else
                {
                    // long-secondary form ssssttC2 -> 00000000sssstt00
                    Debug.Assert((tertiary & 0xf) == LONG_SECONDARY_TAG);
                    return ce32 & 0xffffffffL;
                }
            }
        }

        /// <summary>Creates a CE from a primary weight.</summary>
        public static long MakeCE(long p)
        {
            return (p << 32) | CommonSecondaryAndTertiaryCE;
        }
        /// <summary>
        /// Creates a CE from a primary weight,
        /// 16-bit secondary/tertiary weights, and a 2-bit quaternary.
        /// </summary>
        internal static long MakeCE(long p, int s, int t, int q)
        {
            return (p << 32) | ((long)s << 16) | (uint)t | (uint)(q << 6);
        }

        /// <summary>
        /// Increments a 2-byte primary by a code point offset.
        /// </summary>
        public static long IncTwoBytePrimaryByOffset(long basePrimary, bool isCompressible,
                                                  int offset)
        {
            // Extract the second byte, minus the minimum byte value,
            // plus the offset, modulo the number of usable byte values, plus the minimum.
            // Reserve the PRIMARY_COMPRESSION_LOW_BYTE and high byte if necessary.
            long primary;
            if (isCompressible)
            {
                offset += ((int)(basePrimary >> 16) & 0xff) - 4;
                primary = ((offset % 251) + 4) << 16;
                offset /= 251;
            }
            else
            {
                offset += ((int)(basePrimary >> 16) & 0xff) - 2;
                primary = ((offset % 254) + 2) << 16;
                offset /= 254;
            }
            // First byte, assume no further overflow.
            return primary | ((basePrimary & 0xff000000L) + ((long)offset << 24));
        }

        /// <summary>
        /// Increments a 3-byte primary by a code point offset.
        /// </summary>
        public static long IncThreeBytePrimaryByOffset(long basePrimary, bool isCompressible,
                                                    int offset)
        {
            // Extract the third byte, minus the minimum byte value,
            // plus the offset, modulo the number of usable byte values, plus the minimum.
            offset += ((int)(basePrimary >> 8) & 0xff) - 2;
            long primary = ((offset % 254) + 2) << 8;
            offset /= 254;
            // Same with the second byte,
            // but reserve the PRIMARY_COMPRESSION_LOW_BYTE and high byte if necessary.
            if (isCompressible)
            {
                offset += ((int)(basePrimary >> 16) & 0xff) - 4;
                primary |= (uint)((offset % 251) + 4) << 16;
                offset /= 251;
            }
            else
            {
                offset += ((int)(basePrimary >> 16) & 0xff) - 2;
                primary |= (uint)((offset % 254) + 2) << 16;
                offset /= 254;
            }
            // First byte, assume no further overflow.
            return primary | ((basePrimary & 0xff000000L) + ((long)offset << 24));
        }

        /// <summary>
        /// Decrements a 2-byte primary by one range step (1..0x7f).
        /// </summary>
        internal static long DecTwoBytePrimaryByOneStep(long basePrimary, bool isCompressible, int step)
        {
            // Extract the second byte, minus the minimum byte value,
            // minus the step, modulo the number of usable byte values, plus the minimum.
            // Reserve the PRIMARY_COMPRESSION_LOW_BYTE and high byte if necessary.
            // Assume no further underflow for the first byte.
            Debug.Assert(0 < step && step <= 0x7f);
            int byte2 = ((int)(basePrimary >> 16) & 0xff) - step;
            if (isCompressible)
            {
                if (byte2 < 4)
                {
                    byte2 += 251;
                    basePrimary -= 0x1000000;
                }
            }
            else
            {
                if (byte2 < 2)
                {
                    byte2 += 254;
                    basePrimary -= 0x1000000;
                }
            }
            return (basePrimary & 0xff000000L) | (uint)(byte2 << 16);
        }

        /// <summary>
        /// Decrements a 3-byte primary by one range step (1..0x7f).
        /// </summary>
        internal static long DecThreeBytePrimaryByOneStep(long basePrimary, bool isCompressible, int step)
        {
            // Extract the third byte, minus the minimum byte value,
            // minus the step, modulo the number of usable byte values, plus the minimum.
            Debug.Assert(0 < step && step <= 0x7f);
            int byte3 = ((int)(basePrimary >> 8) & 0xff) - step;
            if (byte3 >= 2)
            {
                return (basePrimary & 0xffff0000L) | (uint)(byte3 << 8);
            }
            byte3 += 254;
            // Same with the second byte,
            // but reserve the PRIMARY_COMPRESSION_LOW_BYTE and high byte if necessary.
            int byte2 = ((int)(basePrimary >> 16) & 0xff) - 1;
            if (isCompressible)
            {
                if (byte2 < 4)
                {
                    byte2 = 0xfe;
                    basePrimary -= 0x1000000;
                }
            }
            else
            {
                if (byte2 < 2)
                {
                    byte2 = 0xff;
                    basePrimary -= 0x1000000;
                }
            }
            // First byte, assume no further underflow.
            return (basePrimary & 0xff000000L) | (uint)(byte2 << 16) | (uint)(byte3 << 8);
        }

        /// <summary>
        /// Computes a 3-byte primary for c's OFFSET_TAG data "CE".
        /// </summary>
        internal static long GetThreeBytePrimaryForOffsetData(int c, long dataCE)
        {
            long p = dataCE >>> 32;  // three-byte primary pppppp00
            int lower32 = (int)dataCE;  // base code point b & step s: bbbbbbss (bit 7: isCompressible)
            int offset = (c - (lower32 >> 8)) * (lower32 & 0x7f);  // delta * increment
            bool isCompressible = (lower32 & 0x80) != 0;
            return Collation.IncThreeBytePrimaryByOffset(p, isCompressible, offset);
        }

        /// <summary>
        /// Returns the unassigned-character implicit primary weight for any valid code point c.
        /// </summary>
        internal static long UnassignedPrimaryFromCodePoint(int c)
        {
            // Create a gap before U+0000. Use c=-1 for [first unassigned].
            ++c;
            // Fourth byte: 18 values, every 14th byte value (gap of 13).
            long primary = 2 + (c % 18) * 14;
            c /= 18;
            // Third byte: 254 values.
            primary |= (uint)(2 + (c % 254)) << 8;
            c /= 254;
            // Second byte: 251 values 04..FE excluding the primary compression bytes.
            primary |= (uint)(4 + (c % 251)) << 16;
            // One lead byte covers all code points (c < 0x1182B4 = 1*251*254*18).
            return primary | ((long)UNASSIGNED_IMPLICIT_BYTE << 24);
        }

        internal static long UnassignedCEFromCodePoint(int c)
        {
            return MakeCE(UnassignedPrimaryFromCodePoint(c));
        }

        // private Collation()  // No instantiation.
    }
}
