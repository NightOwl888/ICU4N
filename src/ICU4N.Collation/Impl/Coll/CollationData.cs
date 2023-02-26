using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Collation data container.
    /// Immutable data created by a <see cref="CollationDataBuilder"/>, or loaded from a file,
    /// or deserialized from API-provided binary data.
    /// <para/>
    /// Includes data for the collation base (root/default), aliased if this is not the base.
    /// </summary>
    public sealed class CollationData
    {
        // Note: The ucadata.icu loader could discover the reserved ranges by setting an array
        // parallel with the ranges, and resetting ranges that are indexed.
        // The reordering builder code could clone the resulting template array.
        internal const int REORDER_RESERVED_BEFORE_LATIN = ReorderCodes.First + 14;
        internal const int REORDER_RESERVED_AFTER_LATIN = ReorderCodes.First + 15;

        internal const int MAX_NUM_SPECIAL_REORDER_CODES = 8;

        internal CollationData(Normalizer2Impl nfc)
        {
            NfcImpl = nfc;
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
            return CompressibleBytes[b];
        }

        public bool IsCompressiblePrimary(long p)
        {
            return IsCompressibleLeadByte(((int)p).TripleShift(24));
        }

        /// <summary>
        /// Returns the CE32 from two contexts words.
        /// Access to the defaultCE32 for contraction and prefix matching.
        /// </summary>
        internal int GetCE32FromContexts(int index)
        {
            return ((int)contexts[index] << 16) | contexts[index + 1];
        }

        /// <summary>
        /// Returns the CE32 for an indirect special CE32 (e.g., with DIGIT_TAG).
        /// Requires that <paramref name="ce32"/> is special.
        /// </summary>
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

        /// <summary>
        /// Returns the CE32 for an indirect special CE32 (e.g., with DIGIT_TAG),
        /// if <paramref name="ce32"/> is special.
        /// </summary>
        internal int GetFinalCE32(int ce32)
        {
            if (Collation.IsSpecialCE32(ce32))
            {
                ce32 = GetIndirectCE32(ce32);
            }
            return ce32;
        }

        /// <summary>
        /// Computes a CE from <paramref name="c"/>'s <paramref name="ce32"/> which has the <see cref="Collation.OFFSET_TAG"/>.
        /// </summary>
        internal long GetCEFromOffsetCE32(int c, int ce32)
        {
            long dataCE = ces[Collation.IndexFromCE32(ce32)];
            return Collation.MakeCE(Collation.GetThreeBytePrimaryForOffsetData(c, dataCE));
        }

        /// <summary>
        /// Returns the single CE that c maps to.
        /// Throws <see cref="NotSupportedException"/> if <paramref name="c"/> does not map to a single CE.
        /// </summary>
        internal long GetSingleCE(int c)
        {
            CollationData d;
            int ce32 = GetCE32(c);
            if (ce32 == Collation.FALLBACK_CE32)
            {
                d = Base;
                ce32 = Base.GetCE32(c);
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

        /// <summary>
        /// Returns the FCD16 value for code point <paramref name="c"/>. <paramref name="c"/> must be >= 0.
        /// </summary>
        internal int GetFCD16(int c)
        {
            return NfcImpl.GetFCD16(c);
        }

        /// <summary>
        /// Returns the first primary for the <paramref name="script"/>'s reordering group.
        /// </summary>
        /// <returns>The primary with only the first primary lead byte of the group
        /// (not necessarily an actual root collator primary weight),
        /// or 0 if the <paramref name="script"/> is unknown
        /// </returns>
        internal long GetFirstPrimaryForGroup(int script)
        {
            int index = GetScriptIndex(script);
            return index == 0 ? 0 : (long)scriptStarts[index] << 16;
        }

        /// <summary>
        /// Returns the last primary for the <paramref name="script"/>'s reordering group.
        /// </summary>
        /// <returns>The last primary of the group
        /// (not an actual root collator primary weight),
        /// or 0 if the <paramref name="script"/> is unknown.
        /// </returns>
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

        /// <summary>
        /// Finds the reordering group which contains the primary weight.
        /// </summary>
        /// <returns>The first script of the group, or -1 if the weight is beyond the last group.</returns>
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
                    return ReorderCodes.First + i;
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
            else if (script < ReorderCodes.First)
            {
                return 0;
            }
            else
            {
                script -= ReorderCodes.First;
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
            if (script >= ReorderCodes.First)
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

        /// <summary>
        /// Writes the permutation of primary-weight ranges
        /// for the given reordering of scripts and groups.
        /// The caller checks for illegal arguments and
        /// takes care of [DEFAULT] and memory allocation.
        /// <para/>
        /// Each list element will be a (limit, offset) pair as described
        /// for the <see cref="CollationSettings.reorderRanges"/>.
        /// The list will be empty if no ranges are reordered.
        /// </summary>
        /// <param name="reorder"></param>
        /// <param name="ranges"></param>
        internal void MakeReorderRanges(int[] reorder, IList<int> ranges)
        {
            MakeReorderRanges(reorder, false, ranges);
        }

        private void MakeReorderRanges(int[] reorder, bool latinMustMove, IList<int> ranges)
        {
            ranges.Clear();
            int length = reorder.Length;
            if (length == 0 || (length == 1 && reorder[0] == UScript.Unknown))
            {
                return;
            }

            // Maps each script-or-group range to a new lead byte.
            short[] table = new short[scriptStarts.Length - 1];  // C++: uint8_t[]

            {
                // Set "don't care" values for reserved ranges.
                int index = scriptsIndex[
                        numScripts + REORDER_RESERVED_BEFORE_LATIN - ReorderCodes.First];
                if (index != 0)
                {
                    table[index] = 0xff;
                }
                index = scriptsIndex[
                        numScripts + REORDER_RESERVED_AFTER_LATIN - ReorderCodes.First];
                if (index != 0)
                {
                    table[index] = 0xff;
                }
            }

            // Never reorder special low and high primary lead bytes.
            Debug.Assert(scriptStarts.Length >= 2);
            Debug.Assert(scriptStarts[0] == 0);
            int lowStart = scriptStarts[1];
            Debug.Assert(lowStart == ((Collation.MergeSeparatorByte + 1) << 8));
            int highLimit = scriptStarts[scriptStarts.Length - 1];
            Debug.Assert(highLimit == (Collation.TRAIL_WEIGHT_BYTE << 8));

            // Get the set of special reorder codes in the input list.
            // This supports a fixed number of special reorder codes;
            // it works for data with codes beyond Collator.ReorderCodes.LIMIT.
            int specials = 0;
            for (int i = 0; i < length; ++i)
            {
                int reorderCode = reorder[i] - ReorderCodes.First;
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
            if (specials == 0 && reorder[0] == UScript.Latin && !latinMustMove)
            {
                int index = scriptsIndex[UScript.Latin];
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
                if (script == UScript.Unknown)
                {
                    // Put the remaining scripts at the top.
                    hasReorderToEnd = true;
                    while (i < length)
                    {
                        script = reorder[--length];
                        if (script == UScript.Unknown)
                        {  // Must occur at most once.
                            throw new ArgumentException(
                                    "SetReorderCodes(): duplicate UScript.Unknown");
                        }
                        if (script == ReorderCodes.Default)
                        {
                            throw new ArgumentException(
                                    "SetReorderCodes(): UScript.Default together with other scripts");
                        }
                        int index2 = GetScriptIndex(script);
                        if (index2 == 0) { continue; }
                        if (table[index2] != 0)
                        {  // Duplicate or equivalent script.
                            throw new ArgumentException(
                                    "SetReorderCodes(): duplicate or equivalent script " +
                                    ScriptCodeString(script));
                        }
                        highLimit = AddHighScriptRange(table, index2, highLimit);
                    }
                    break;
                }
                if (script == ReorderCodes.Default)
                {
                    // The default code must be the only one in the list, and that is handled by the caller.
                    // Otherwise it must not be used.
                    throw new ArgumentException(
                            "SetReorderCodes(): UScript.Default together with other scripts");
                }
                int index = GetScriptIndex(script);
                if (index == 0) { continue; }
                if (table[index] != 0)
                {  // Duplicate or equivalent script.
                    throw new ArgumentException(
                            "SetReorderCodes(): duplicate or equivalent script " +
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
                        "SetReorderCodes(): reordering too many partial-primary-lead-byte scripts");
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
            return (script < ReorderCodes.First) ?
                    script.ToString(CultureInfo.InvariantCulture) : "0x" + script.ToHexString();
        }

        private static readonly int[] EMPTY_INT_ARRAY = new int[0];

        /// <seealso cref="jamoCE32s"/>
        internal const int JAMO_CE32S_LENGTH = 19 + 21 + 27;

        /// <summary>Main lookup trie.</summary>
        internal Trie2_32 trie;
        /// <summary>
        /// Array of CE32 values.
        /// At index 0 there must be CE32(U+0000)
        /// to support U+0000's special-tag for NUL-termination handling.
        /// </summary>
        internal IList<int> ce32s;
        /// <summary>Array of CE values for expansions and <see cref="Collation.OFFSET_TAG"/>.</summary>
        internal IList<long> ces;
        /// <summary>Array of prefix and contraction-suffix matching data.</summary>
        internal string contexts;

        /// <summary>Base collation data, or null if this data itself is a base.</summary>
        public CollationData Base { get; set; }

        /// <summary>
        /// Simple array of <see cref="JAMO_CE32S_LENGTH"/>=19+21+27 CE32s, one per canonical Jamo L/V/T.
        /// They are normally simple CE32s, rarely expansions.
        /// For fast handling of <see cref="Collation.HANGUL_TAG"/>.
        /// </summary>
        internal int[] jamoCE32s = new int[JAMO_CE32S_LENGTH];
        public Normalizer2Impl NfcImpl { get; set; }
        /// <summary>The single-byte primary weight (xx000000) for numeric collation.</summary>
        internal long numericPrimary = 0x12000000;

        /// <summary>256 flags for which primary-weight lead bytes are compressible.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public bool[] CompressibleBytes { get; set; }

        /// <summary>
        /// Set of code points that are unsafe for starting string comparison after an identical prefix,
        /// or in backwards CE iteration.
        /// </summary>
        internal UnicodeSet unsafeBackwardSet;

        /// <summary>
        /// Fast Latin table for common-Latin-text string comparisons.
        /// Data structure see class <see cref="CollationFastLatin"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public char[] FastLatinTable { get; set; }

        /// <summary>
        /// Header portion of the <see cref="FastLatinTable"/>.
        /// In C++, these are one array, and the header is skipped for mapping characters.
        /// In .NET, two arrays work better.
        /// </summary>
        internal char[] fastLatinTableHeader;

        /// <summary>
        /// Data for scripts and reordering groups.
        /// Uses include building a reordering permutation table and
        /// providing script boundaries to <see cref="ICU4N.Text.AlphabeticIndex{T}"/>.
        /// </summary>
        internal int numScripts;

        /// <summary>
        /// The length of scriptsIndex is <see cref="numScripts"/>+16.
        /// It maps from a Script code or a special reorder code to an entry in <see cref="scriptStarts"/>.
        /// 16 special reorder codes (not all used) are mapped starting at <see cref="numScripts"/>.
        /// Up to <see cref="MAX_NUM_SPECIAL_REORDER_CODES"/> are codes for special groups like space/punct/digit.
        /// There are special codes at the end for reorder-reserved primary ranges.
        /// <para/>
        /// Multiple scripts may share a range and index, for example Hira &amp; Kana.
        /// </summary>
        internal char[] scriptsIndex;

        /// <summary>
        /// Start primary weight (top 16 bits only) for a group/script/reserved range
        /// indexed by <see cref="scriptsIndex"/>.
        /// The first range (separators &amp; terminators) and the last range (trailing weights)
        /// are not reorderable, and no <see cref="scriptsIndex"/> entry points to them.
        /// </summary>
        internal char[] scriptStarts;

        /// <summary>
        /// Collation elements in the root collator.
        /// Used by the <see cref="CollationRootElements"/> class. The data structure is described there.
        /// null in a tailoring.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public long[] RootElements { get; set; }
    }
}
