using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Collation data container.
    /// Immutable data created by a CollationDataBuilder, or loaded from a file,
    /// or deserialized from API-provided binary data.
    /// <para/>
    /// Includes data for the collation base (root/default), aliased if this is not the base.
    /// </summary>
    public sealed class CollationData
    {
        // ICU4N specific - We need to pass this assembly in order for its resources to load.
        public static readonly Assembly ICU_DATA_CLASS_LOADER = typeof(CollationData).GetTypeInfo().Assembly;

        // Note: The ucadata.icu loader could discover the reserved ranges by setting an array
        // parallel with the ranges, and resetting ranges that are indexed.
        // The reordering builder code could clone the resulting template array.
        internal static readonly int REORDER_RESERVED_BEFORE_LATIN = (int)ReorderCode.First + 14;
        internal static readonly int REORDER_RESERVED_AFTER_LATIN = (int)ReorderCode.First + 15;

        internal static readonly int MAX_NUM_SPECIAL_REORDER_CODES = 8;

        internal CollationData(Normalizer2Impl nfc)
        {
            nfcImpl = nfc;
        }

        public int GetCE32(int c)
        {
            return trie.Get(c);
        }

        internal int GetCE32FromSupplementary(int c)
        {
            return trie.Get(c);  // TODO: port UTRIE2_GET32_FROM_SUPP(trie, c) to Java?
        }

        internal bool IsDigit(int c)
        {
            return c < 0x660 ? c <= 0x39 && 0x30 <= c :
                    Collation.HasCE32Tag(GetCE32(c), Collation.DIGIT_TAG);
        }

        public bool IsUnsafeBackward(int c, bool numeric)
        {
            return unsafeBackwardSet.Contains(c) || (numeric && IsDigit(c));
        }

        public bool IsCompressibleLeadByte(int b)
        {
            return compressibleBytes[b];
        }

        public bool IsCompressiblePrimary(long p)
        {
            return IsCompressibleLeadByte(((int)p).TripleShift(24));
        }

        /**
         * Returns the CE32 from two contexts words.
         * Access to the defaultCE32 for contraction and prefix matching.
         */
        internal int GetCE32FromContexts(int index)
        {
            return ((int)contexts[index] << 16) | contexts[index + 1];
        }

        /**
         * Returns the CE32 for an indirect special CE32 (e.g., with DIGIT_TAG).
         * Requires that ce32 is special.
         */
        internal int GetIndirectCE32(int ce32)
        {
            Debug.Assert(Collation.IsSpecialCE32(ce32));
            int tag = Collation.TagFromCE32(ce32);
            if (tag == Collation.DIGIT_TAG)
            {
                // Fetch the non-numeric-collation CE32.
                ce32 = ce32s[Collation.IndexFromCE32(ce32)];
            }
            else if (tag == Collation.LEAD_SURROGATE_TAG)
            {
                ce32 = Collation.UNASSIGNED_CE32;
            }
            else if (tag == Collation.U0000_TAG)
            {
                // Fetch the normal ce32 for U+0000.
                ce32 = ce32s[0];
            }
            return ce32;
        }

        /**
         * Returns the CE32 for an indirect special CE32 (e.g., with DIGIT_TAG),
         * if ce32 is special.
         */
        internal int GetFinalCE32(int ce32)
        {
            if (Collation.IsSpecialCE32(ce32))
            {
                ce32 = GetIndirectCE32(ce32);
            }
            return ce32;
        }

        /**
         * Computes a CE from c's ce32 which has the OFFSET_TAG.
         */
        internal long GetCEFromOffsetCE32(int c, int ce32)
        {
            long dataCE = ces[Collation.IndexFromCE32(ce32)];
            return Collation.MakeCE(Collation.GetThreeBytePrimaryForOffsetData(c, dataCE));
        }

        /**
         * Returns the single CE that c maps to.
         * Throws UnsupportedOperationException if c does not map to a single CE.
         */
        internal long GetSingleCE(int c)
        {
            CollationData d;
            int ce32 = GetCE32(c);
            if (ce32 == Collation.FALLBACK_CE32)
            {
                d = base_;
                ce32 = base_.GetCE32(c);
            }
            else
            {
                d = this;
            }
            while (Collation.IsSpecialCE32(ce32))
            {
                switch (Collation.TagFromCE32(ce32))
                {
                    case Collation.LATIN_EXPANSION_TAG:
                    case Collation.BUILDER_DATA_TAG:
                    case Collation.PREFIX_TAG:
                    case Collation.CONTRACTION_TAG:
                    case Collation.HANGUL_TAG:
                    case Collation.LEAD_SURROGATE_TAG:
                        throw new NotSupportedException(string.Format(
                                "there is not exactly one collation element for U+{0:X4} (CE32 0x{1:x8})",
                                c, ce32));
                    case Collation.FALLBACK_TAG:
                    case Collation.RESERVED_TAG_3:
                        throw new InvalidOperationException(string.Format(
                                "unexpected CE32 tag for U+{0:X4} (CE32 0x{1:x8})", c, ce32));
                    case Collation.LONG_PRIMARY_TAG:
                        return Collation.CeFromLongPrimaryCE32(ce32);
                    case Collation.LONG_SECONDARY_TAG:
                        return Collation.CeFromLongSecondaryCE32(ce32);
                    case Collation.EXPANSION32_TAG:
                        if (Collation.LengthFromCE32(ce32) == 1)
                        {
                            ce32 = d.ce32s[Collation.IndexFromCE32(ce32)];
                            break;
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format(
                                    "there is not exactly one collation element for U+{0:X4} (CE32 0x{1:x8})",
                                    c, ce32));
                        }
                    case Collation.EXPANSION_TAG:
                        {
                            if (Collation.LengthFromCE32(ce32) == 1)
                            {
                                return d.ces[Collation.IndexFromCE32(ce32)];
                            }
                            else
                            {
                                throw new NotSupportedException(string.Format(
                                        "there is not exactly one collation element for U+{0:X4} (CE32 0x{1:x8})",
                                        c, ce32));
                            }
                        }
                    case Collation.DIGIT_TAG:
                        // Fetch the non-numeric-collation CE32 and continue.
                        ce32 = d.ce32s[Collation.IndexFromCE32(ce32)];
                        break;
                    case Collation.U0000_TAG:
                        Debug.Assert(c == 0);
                        // Fetch the normal ce32 for U+0000 and continue.
                        ce32 = d.ce32s[0];
                        break;
                    case Collation.OFFSET_TAG:
                        return d.GetCEFromOffsetCE32(c, ce32);
                    case Collation.IMPLICIT_TAG:
                        return Collation.UnassignedCEFromCodePoint(c);
                }
            }
            return Collation.CeFromSimpleCE32(ce32);
        }

        /**
         * Returns the FCD16 value for code point c. c must be >= 0.
         */
        internal int GetFCD16(int c)
        {
            return nfcImpl.GetFCD16(c);
        }

        /**
         * Returns the first primary for the script's reordering group.
         * @return the primary with only the first primary lead byte of the group
         *         (not necessarily an actual root collator primary weight),
         *         or 0 if the script is unknown
         */
        internal long GetFirstPrimaryForGroup(int script)
        {
            int index = GetScriptIndex(script);
            return index == 0 ? 0 : (long)scriptStarts[index] << 16;
        }

        /**
         * Returns the last primary for the script's reordering group.
         * @return the last primary of the group
         *         (not an actual root collator primary weight),
         *         or 0 if the script is unknown
         */
        public long GetLastPrimaryForGroup(int script)
        {
            int index = GetScriptIndex(script);
            if (index == 0)
            {
                return 0;
            }
            long limit = scriptStarts[index + 1];
            return (limit << 16) - 1;
        }

        /**
         * Finds the reordering group which contains the primary weight.
         * @return the first script of the group, or -1 if the weight is beyond the last group
         */
        public int GetGroupForPrimary(long p)
        {
            p >>= 16;
            if (p < scriptStarts[1] || scriptStarts[scriptStarts.Length - 1] <= p)
            {
                return -1;
            }
            int index = 1;
            while (p >= scriptStarts[index + 1]) { ++index; }
            for (int i = 0; i < numScripts; ++i)
            {
                if (scriptsIndex[i] == index)
                {
                    return i;
                }
            }
            for (int i = 0; i < MAX_NUM_SPECIAL_REORDER_CODES; ++i)
            {
                if (scriptsIndex[numScripts + i] == index)
                {
                    return (int)ReorderCode.First + i;
                }
            }
            return -1;
        }

        private int GetScriptIndex(int script)
        {
            if (script < 0)
            {
                return 0;
            }
            else if (script < numScripts)
            {
                return scriptsIndex[script];
            }
            else if (script < (int)ReorderCode.First)
            {
                return 0;
            }
            else
            {
                script -= (int)ReorderCode.First;
                if (script < MAX_NUM_SPECIAL_REORDER_CODES)
                {
                    return scriptsIndex[numScripts + script];
                }
                else
                {
                    return 0;
                }
            }
        }

        public int[] GetEquivalentScripts(int script)
        {
            int index = GetScriptIndex(script);
            if (index == 0) { return EMPTY_INT_ARRAY; }
            if (script >= (int)ReorderCode.First)
            {
                // Special groups have no aliases.
                return new int[] { script };
            }

            int length = 0;
            for (int i = 0; i < numScripts; ++i)
            {
                if (scriptsIndex[i] == index)
                {
                    ++length;
                }
            }
            int[] dest = new int[length];
            if (length == 1)
            {
                dest[0] = script;
                return dest;
            }
            length = 0;
            for (int i = 0; i < numScripts; ++i)
            {
                if (scriptsIndex[i] == index)
                {
                    dest[length++] = i;
                }
            }
            return dest;
        }

        /**
         * Writes the permutation of primary-weight ranges
         * for the given reordering of scripts and groups.
         * The caller checks for illegal arguments and
         * takes care of [DEFAULT] and memory allocation.
         *
         * <p>Each list element will be a (limit, offset) pair as described
         * for the CollationSettings.reorderRanges.
         * The list will be empty if no ranges are reordered.
         */
        internal void MakeReorderRanges(int[] reorder, IList<int> ranges)
        {
            MakeReorderRanges(reorder, false, ranges);
        }

        private void MakeReorderRanges(int[] reorder, bool latinMustMove, IList<int> ranges)
        {
            ranges.Clear();
            int length = reorder.Length;
            if (length == 0 || (length == 1 && reorder[0] == UScript.UNKNOWN))
            {
                return;
            }

            // Maps each script-or-group range to a new lead byte.
            short[] table = new short[scriptStarts.Length - 1];  // C++: uint8_t[]

            {
                // Set "don't care" values for reserved ranges.
                int index = scriptsIndex[
                        numScripts + REORDER_RESERVED_BEFORE_LATIN - (int)ReorderCode.First];
                if (index != 0)
                {
                    table[index] = 0xff;
                }
                index = scriptsIndex[
                        numScripts + REORDER_RESERVED_AFTER_LATIN - (int)ReorderCode.First];
                if (index != 0)
                {
                    table[index] = 0xff;
                }
            }

            // Never reorder special low and high primary lead bytes.
            Debug.Assert(scriptStarts.Length >= 2);
            Debug.Assert(scriptStarts[0] == 0);
            int lowStart = scriptStarts[1];
            Debug.Assert(lowStart == ((Collation.MERGE_SEPARATOR_BYTE + 1) << 8));
            int highLimit = scriptStarts[scriptStarts.Length - 1];
            Debug.Assert(highLimit == (Collation.TRAIL_WEIGHT_BYTE << 8));

            // Get the set of special reorder codes in the input list.
            // This supports a fixed number of special reorder codes;
            // it works for data with codes beyond Collator.ReorderCodes.LIMIT.
            int specials = 0;
            for (int i = 0; i < length; ++i)
            {
                int reorderCode = reorder[i] - (int)ReorderCode.First;
                if (0 <= reorderCode && reorderCode < MAX_NUM_SPECIAL_REORDER_CODES)
                {
                    specials |= 1 << reorderCode;
                }
            }

            // Start the reordering with the special low reorder codes that do not occur in the input.
            for (int i = 0; i < MAX_NUM_SPECIAL_REORDER_CODES; ++i)
            {
                int index = scriptsIndex[numScripts + i];
                if (index != 0 && (specials & (1 << i)) == 0)
                {
                    lowStart = AddLowScriptRange(table, index, lowStart);
                }
            }

            // Skip the reserved range before Latin if Latin is the first script,
            // so that we do not move it unnecessarily.
            int skippedReserved = 0;
            if (specials == 0 && reorder[0] == UScript.LATIN && !latinMustMove)
            {
                int index = scriptsIndex[UScript.LATIN];
                Debug.Assert(index != 0);
                int start = scriptStarts[index];
                Debug.Assert(lowStart <= start);
                skippedReserved = start - lowStart;
                lowStart = start;
            }

            // Reorder according to the input scripts, continuing from the bottom of the primary range.
            bool hasReorderToEnd = false;
            for (int i = 0; i < length;)
            {
                int script = reorder[i++];
                if (script == UScript.UNKNOWN)
                {
                    // Put the remaining scripts at the top.
                    hasReorderToEnd = true;
                    while (i < length)
                    {
                        script = reorder[--length];
                        if (script == UScript.UNKNOWN)
                        {  // Must occur at most once.
                            throw new ArgumentException(
                                    "setReorderCodes(): duplicate UScript.UNKNOWN");
                        }
                        if (script == (int)ReorderCode.Default)
                        {
                            throw new ArgumentException(
                                    "setReorderCodes(): UScript.DEFAULT together with other scripts");
                        }
                        int index2 = GetScriptIndex(script);
                        if (index2 == 0) { continue; }
                        if (table[index2] != 0)
                        {  // Duplicate or equivalent script.
                            throw new ArgumentException(
                                    "setReorderCodes(): duplicate or equivalent script " +
                                    ScriptCodeString(script));
                        }
                        highLimit = AddHighScriptRange(table, index2, highLimit);
                    }
                    break;
                }
                if (script == (int)ReorderCode.Default)
                {
                    // The default code must be the only one in the list, and that is handled by the caller.
                    // Otherwise it must not be used.
                    throw new ArgumentException(
                            "setReorderCodes(): UScript.DEFAULT together with other scripts");
                }
                int index = GetScriptIndex(script);
                if (index == 0) { continue; }
                if (table[index] != 0)
                {  // Duplicate or equivalent script.
                    throw new ArgumentException(
                            "setReorderCodes(): duplicate or equivalent script " +
                            ScriptCodeString(script));
                }
                lowStart = AddLowScriptRange(table, index, lowStart);
            }

            // Put all remaining scripts into the middle.
            for (int i = 1; i < scriptStarts.Length - 1; ++i)
            {
                int leadByte = table[i];
                if (leadByte != 0) { continue; }
                int start = scriptStarts[i];
                if (!hasReorderToEnd && start > lowStart)
                {
                    // No need to move this script.
                    lowStart = start;
                }
                lowStart = AddLowScriptRange(table, i, lowStart);
            }
            if (lowStart > highLimit)
            {
                if ((lowStart - (skippedReserved & 0xff00)) <= highLimit)
                {
                    // Try not skipping the before-Latin reserved range.
                    MakeReorderRanges(reorder, true, ranges);
                    return;
                }
                // We need more primary lead bytes than available, despite the reserved ranges.
                throw new ICUException(
                        "setReorderCodes(): reordering too many partial-primary-lead-byte scripts");
            }

            // Turn lead bytes into a list of (limit, offset) pairs.
            // Encode each pair in one list element:
            // Upper 16 bits = limit, lower 16 = signed lead byte offset.
            int offset = 0;
            for (int i = 1; ; ++i)
            {
                int nextOffset = offset;
                while (i < scriptStarts.Length - 1)
                {
                    int newLeadByte = table[i];
                    if (newLeadByte == 0xff)
                    {
                        // "Don't care" lead byte for reserved range, continue with current offset.
                    }
                    else
                    {
                        nextOffset = newLeadByte - (scriptStarts[i] >> 8);
                        if (nextOffset != offset) { break; }
                    }
                    ++i;
                }
                if (offset != 0 || i < scriptStarts.Length - 1)
                {
                    ranges.Add(((int)scriptStarts[i] << 16) | (offset & 0xffff));
                }
                if (i == scriptStarts.Length - 1) { break; }
                offset = nextOffset;
            }
        }

        private int AddLowScriptRange(short[] table, int index, int lowStart)
        {
            int start = scriptStarts[index];
            if ((start & 0xff) < (lowStart & 0xff))
            {
                lowStart += 0x100;
            }
            table[index] = (short)(lowStart >> 8);
            int limit = scriptStarts[index + 1];
            lowStart = ((lowStart & 0xff00) + ((limit & 0xff00) - (start & 0xff00))) | (limit & 0xff);
            return lowStart;
        }

        private int AddHighScriptRange(short[] table, int index, int highLimit)
        {
            int limit = scriptStarts[index + 1];
            if ((limit & 0xff) > (highLimit & 0xff))
            {
                highLimit -= 0x100;
            }
            int start = scriptStarts[index];
            highLimit = ((highLimit & 0xff00) - ((limit & 0xff00) - (start & 0xff00))) | (start & 0xff);
            table[index] = (short)(highLimit >> 8);
            return highLimit;
        }

        private static string ScriptCodeString(int script)
        {
            // Do not use the script name here: We do not want to depend on that data.
            return (script < (int)ReorderCode.First) ?
                    script.ToString(CultureInfo.InvariantCulture) : "0x" + script.ToHexString();
        }

        private static readonly int[] EMPTY_INT_ARRAY = new int[0];

        /** @see jamoCE32s */
        internal static readonly int JAMO_CE32S_LENGTH = 19 + 21 + 27;

        /** Main lookup trie. */
        internal Trie2_32 trie;
        /**
         * Array of CE32 values.
         * At index 0 there must be CE32(U+0000)
         * to support U+0000's special-tag for NUL-termination handling.
         */
        internal IList<int> ce32s;
        /** Array of CE values for expansions and OFFSET_TAG. */
        internal IList<long> ces;
        /** Array of prefix and contraction-suffix matching data. */
        internal string contexts;
        /** Base collation data, or null if this data itself is a base. */
        private CollationData base_;

        public CollationData Base
        {
            get { return base_; }
            set { base_ = value; }
        }

        // ICU4N TODO: API Make fields into properties

        /**
         * Simple array of JAMO_CE32S_LENGTH=19+21+27 CE32s, one per canonical Jamo L/V/T.
         * They are normally simple CE32s, rarely expansions.
         * For fast handling of HANGUL_TAG.
         */
        internal int[] jamoCE32s = new int[JAMO_CE32S_LENGTH];
        public Normalizer2Impl nfcImpl;
        /** The single-byte primary weight (xx000000) for numeric collation. */
        internal long numericPrimary = 0x12000000;

        /** 256 flags for which primary-weight lead bytes are compressible. */
        public bool[] compressibleBytes;
        /**
         * Set of code points that are unsafe for starting string comparison after an identical prefix,
         * or in backwards CE iteration.
         */
        internal UnicodeSet unsafeBackwardSet;

        /**
         * Fast Latin table for common-Latin-text string comparisons.
         * Data structure see class CollationFastLatin.
         */
        public char[] fastLatinTable;
        /**
         * Header portion of the fastLatinTable.
         * In C++, these are one array, and the header is skipped for mapping characters.
         * In Java, two arrays work better.
         */
        internal char[] fastLatinTableHeader;

        /**
         * Data for scripts and reordering groups.
         * Uses include building a reordering permutation table and
         * providing script boundaries to AlphabeticIndex.
         */
        internal int numScripts;
        /**
         * The length of scriptsIndex is numScripts+16.
         * It maps from a UScriptCode or a special reorder code to an entry in scriptStarts.
         * 16 special reorder codes (not all used) are mapped starting at numScripts.
         * Up to MAX_NUM_SPECIAL_REORDER_CODES are codes for special groups like space/punct/digit.
         * There are special codes at the end for reorder-reserved primary ranges.
         *
         * <p>Multiple scripts may share a range and index, for example Hira & Kana.
         */
        internal char[] scriptsIndex;
        /**
         * Start primary weight (top 16 bits only) for a group/script/reserved range
         * indexed by scriptsIndex.
         * The first range (separators & terminators) and the last range (trailing weights)
         * are not reorderable, and no scriptsIndex entry points to them.
         */
        internal char[] scriptStarts;

        /**
         * Collation elements in the root collator.
         * Used by the CollationRootElements class. The data structure is described there.
         * null in a tailoring.
         */
        public long[] rootElements;
    }
}
