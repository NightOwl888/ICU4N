using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using ICU4N.Text;
using ICU4N.Util;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Collation binary data reader.
    /// </summary>
    /// <since>2013feb07</since>
    /// <author>Markus W. Scherer</author>
    internal static class CollationDataReader /* all static */
    {
        // The following constants are also copied into source/common/ucol_swp.cpp.
        // Keep them in sync!
        /// <summary>
        /// Number of int indexes.
        /// <para/>
        /// Can be 2 if there are only options.
        /// Can be 7 or 8 if there are only options and a script reordering.
        /// The loader treats any index>=indexes[IX_INDEXES_LENGTH] as 0.
        /// </summary>
        internal const int IX_INDEXES_LENGTH = 0;

        /// <summary>
        /// Bits 31..24: numericPrimary, for numeric collation
        ///      23..16: fast Latin format version (0 = no fast Latin table)
        ///      15.. 0: options bit set
        /// </summary>
        internal const int IX_OPTIONS = 1;
        internal const int IX_RESERVED2 = 2;
        internal const int IX_RESERVED3 = 3;

        /// <summary>Array offset to Jamo CE32s in ce32s[], or &lt;0 if none.</summary>
        internal const int IX_JAMO_CE32S_START = 4;

        // Byte offsets from the start of the data, after the generic header.
        // The indexes[] are at byte offset 0, other data follows.
        // Each data item is aligned properly.
        // The data items should be in descending order of unit size,
        // to minimize the need for padding.
        // Each item's byte length is given by the difference between its offset and
        // the next index/offset value.
        /// <summary>Byte offset to int reorderCodes[].</summary>
        internal const int IX_REORDER_CODES_OFFSET = 5;

        /// <summary>
        /// Byte offset to uint8_t reorderTable[].
        /// Empty table if &lt;256 bytes (padding only).
        /// Otherwise 256 bytes or more (with padding).
        /// </summary>
        internal const int IX_REORDER_TABLE_OFFSET = 6;
        /// <summary>Byte offset to the collation trie. Its length is a multiple of 8 bytes.</summary>
        internal const int IX_TRIE_OFFSET = 7;

        internal const int IX_RESERVED8_OFFSET = 8;
        /// <summary>Byte offset to long ces[].</summary>
        internal const int IX_CES_OFFSET = 9;
        internal const int IX_RESERVED10_OFFSET = 10;
        /// <summary>Byte offset to int ce32s[].</summary>
        internal const int IX_CE32S_OFFSET = 11;

        /// <summary>Byte offset to uint32_t rootElements[].</summary>
        internal const int IX_ROOT_ELEMENTS_OFFSET = 12;
        /// <summary>Byte offset to UChar *contexts[].</summary>
        internal const int IX_CONTEXTS_OFFSET = 13;
        /// <summary>Byte offset to char[] with serialized unsafeBackwardSet.</summary>
        internal const int IX_UNSAFE_BWD_OFFSET = 14;
        /// <summary>Byte offset to char fastLatinTable[].</summary>
        internal const int IX_FAST_LATIN_TABLE_OFFSET = 15;

        /// <summary>Byte offset to char scripts[].</summary>
        internal const int IX_SCRIPTS_OFFSET = 16;

        /// <summary>
        /// Byte offset to boolean compressibleBytes[].
        /// Empty table if &lt;256 bytes (padding only).
        /// Otherwise 256 bytes or more (with padding).
        /// </summary>
        internal const int IX_COMPRESSIBLE_BYTES_OFFSET = 17;
        internal const int IX_RESERVED18_OFFSET = 18;
        internal const int IX_TOTAL_SIZE = 19;

        internal static void Read(CollationTailoring @base, ByteBuffer inBytes,
                         CollationTailoring tailoring)
        {
            tailoring.Version = ICUBinary.ReadHeader(inBytes, DATA_FORMAT, IS_ACCEPTABLE);
            if (@base != null && @base.GetUCAVersion() != tailoring.GetUCAVersion())
            {
                throw new ICUException("Tailoring UCA version differs from base data UCA version");
            }

            int inLength = inBytes.Remaining;
            if (inLength < 8)
            {
                throw new ICUException("not enough bytes");
            }
            int indexesLength = inBytes.GetInt32();  // inIndexes[IX_INDEXES_LENGTH]
            if (indexesLength < 2 || inLength < indexesLength * 4)
            {
                throw new ICUException("not enough indexes");
            }
            int[] inIndexes = new int[IX_TOTAL_SIZE + 1];
            inIndexes[0] = indexesLength;
            for (int i = 1; i < indexesLength && i < inIndexes.Length; ++i)
            {
                inIndexes[i] = inBytes.GetInt32();
            }
            for (int i = indexesLength; i < inIndexes.Length; ++i)
            {
                inIndexes[i] = -1;
            }
            if (indexesLength > inIndexes.Length)
            {
                ICUBinary.SkipBytes(inBytes, (indexesLength - inIndexes.Length) * 4);
            }

            // Assume that the tailoring data is in initial state,
            // with null pointers and 0 lengths.

            // Set pointers to non-empty data parts.
            // Do this in order of their byte offsets. (Should help porting to Java.)

            int index;  // one of the indexes[] slots
            int offset;  // byte offset for the index part
            int length;  // number of bytes in the index part

            if (indexesLength > IX_TOTAL_SIZE)
            {
                length = inIndexes[IX_TOTAL_SIZE];
            }
            else if (indexesLength > IX_REORDER_CODES_OFFSET)
            {
                length = inIndexes[indexesLength - 1];
            }
            else
            {
                length = 0;  // only indexes, and inLength was already checked for them
            }
            if (inLength < length)
            {
                throw new ICUException("not enough bytes");
            }

            CollationData baseData = @base == null ? null : @base.Data;
            int[] reorderCodes;
            int reorderCodesLength;
            index = IX_REORDER_CODES_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 4)
            {
                if (baseData == null)
                {
                    // We assume for collation settings that
                    // the base data does not have a reordering.
                    throw new ICUException("Collation base data must not reorder scripts");
                }
                reorderCodesLength = length / 4;
                reorderCodes = ICUBinary.GetInts(inBytes, reorderCodesLength, length & 3);

                // The reorderRanges (if any) are the trailing reorderCodes entries.
                // Split the array at the boundary.
                // Script or reorder codes do not exceed 16-bit values.
                // Range limits are stored in the upper 16 bits, and are never 0.
                int reorderRangesLength = 0;
                while (reorderRangesLength < reorderCodesLength &&
                        (reorderCodes[reorderCodesLength - reorderRangesLength - 1] & 0xffff0000) != 0)
                {
                    ++reorderRangesLength;
                }
                Debug.Assert(reorderRangesLength < reorderCodesLength);
                reorderCodesLength -= reorderRangesLength;
            }
            else
            {
                reorderCodes = new int[0];
                reorderCodesLength = 0;
                ICUBinary.SkipBytes(inBytes, length);
            }

            // There should be a reorder table only if there are reorder codes.
            // However, when there are reorder codes the reorder table may be omitted to reduce
            // the data size.
            byte[] reorderTable = null;
            index = IX_REORDER_TABLE_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 256)
            {
                if (reorderCodesLength == 0)
                {
                    throw new ICUException("Reordering table without reordering codes");
                }
                reorderTable = new byte[256];
                inBytes.Get(reorderTable);
                length -= 256;
            }
            else
            {
                // If we have reorder codes, then build the reorderTable at the end,
                // when the CollationData is otherwise complete.
            }
            ICUBinary.SkipBytes(inBytes, length);

            if (baseData != null && baseData.numericPrimary != (inIndexes[IX_OPTIONS] & 0xff000000L))
            {
                throw new ICUException("Tailoring numeric primary weight differs from base data");
            }
            CollationData data = null;  // Remains null if there are no mappings.

            index = IX_TRIE_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 8)
            {
                tailoring.EnsureOwnedData();
                data = tailoring.OwnedData;
                data.Base = baseData;
                data.numericPrimary = inIndexes[IX_OPTIONS] & 0xff000000L;
                data.trie = tailoring.Trie = Trie2_32.CreateFromSerialized(inBytes);
                int trieLength = data.trie.SerializedLength;
                if (trieLength > length)
                {
                    throw new ICUException("Not enough bytes for the mappings trie");  // No mappings.
                }
                length -= trieLength;
            }
            else if (baseData != null)
            {
                // Use the base data. Only the settings are tailored.
                tailoring.Data = baseData;
            }
            else
            {
                throw new ICUException("Missing collation data mappings");  // No mappings.
            }
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_RESERVED8_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_CES_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 8)
            {
                if (data == null)
                {
                    throw new ICUException("Tailored ces without tailored trie");
                }
                data.ces = ICUBinary.GetLongs(inBytes, length / 8, length & 7);
            }
            else
            {
                ICUBinary.SkipBytes(inBytes, length);
            }

            index = IX_RESERVED10_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_CE32S_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 4)
            {
                if (data == null)
                {
                    throw new ICUException("Tailored ce32s without tailored trie");
                }
                data.ce32s = ICUBinary.GetInts(inBytes, length / 4, length & 3);
            }
            else
            {
                ICUBinary.SkipBytes(inBytes, length);
            }

            int jamoCE32sStart = inIndexes[IX_JAMO_CE32S_START];
            if (jamoCE32sStart >= 0)
            {
                if (data == null || data.ce32s == null)
                {
                    throw new ICUException("JamoCE32sStart index into non-existent ce32s[]");
                }
                data.jamoCE32s = new int[CollationData.JAMO_CE32S_LENGTH];
                // ICU4N specific - added extension method to IList<T> to handle "copy to"
                data.ce32s.CopyTo(jamoCE32sStart, data.jamoCE32s, 0, CollationData.JAMO_CE32S_LENGTH);
            }
            else if (data == null)
            {
                // Nothing to do.
            }
            else if (baseData != null)
            {
                data.jamoCE32s = baseData.jamoCE32s;
            }
            else
            {
                throw new ICUException("Missing Jamo CE32s for Hangul processing");
            }

            index = IX_ROOT_ELEMENTS_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 4)
            {
                int rootElementsLength = length / 4;
                if (data == null)
                {
                    throw new ICUException("Root elements but no mappings");
                }
                if (rootElementsLength <= CollationRootElements.IX_SEC_TER_BOUNDARIES)
                {
                    throw new ICUException("Root elements array too short");
                }
                data.rootElements = new long[rootElementsLength];
                for (int i = 0; i < rootElementsLength; ++i)
                {
                    data.rootElements[i] = inBytes.GetInt32() & 0xffffffffL;  // unsigned int -> long
                }
                long commonSecTer = data.rootElements[CollationRootElements.IX_COMMON_SEC_AND_TER_CE];
                if (commonSecTer != Collation.CommonSecondaryAndTertiaryCE)
                {
                    throw new ICUException("Common sec/ter weights in base data differ from the hardcoded value");
                }
                long secTerBoundaries = data.rootElements[CollationRootElements.IX_SEC_TER_BOUNDARIES];
                if ((secTerBoundaries.TripleShift(24)) < CollationKeys.SEC_COMMON_HIGH)
                {
                    // [fixed last secondary common byte] is too low,
                    // and secondary weights would collide with compressed common secondaries.
                    throw new ICUException("[fixed last secondary common byte] is too low");
                }
                length &= 3;
            }
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_CONTEXTS_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 2)
            {
                if (data == null)
                {
                    throw new ICUException("Tailored contexts without tailored trie");
                }
                data.contexts = ICUBinary.GetString(inBytes, length / 2, length & 1);
            }
            else
            {
                ICUBinary.SkipBytes(inBytes, length);
            }

            index = IX_UNSAFE_BWD_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 2)
            {
                if (data == null)
                {
                    throw new ICUException("Unsafe-backward-set but no mappings");
                }
                if (baseData == null)
                {
                    // Create the unsafe-backward set for the root collator.
                    // Include all non-zero combining marks and trail surrogates.
                    // We do this at load time, rather than at build time,
                    // to simplify Unicode version bootstrapping:
                    // The root data builder only needs the new FractionalUCA.txt data,
                    // but it need not be built with a version of ICU already updated to
                    // the corresponding new Unicode Character Database.
                    //
                    // The following is an optimized version of
                    // new UnicodeSet("[[:^lccc=0:][\\udc00-\\udfff]]").
                    // It is faster and requires fewer code dependencies.
                    tailoring.UnsafeBackwardSet = new UnicodeSet(0xdc00, 0xdfff);  // trail surrogates
                    data.nfcImpl.AddLcccChars(tailoring.UnsafeBackwardSet);
                }
                else
                {
                    // Clone the root collator's set contents.
                    tailoring.UnsafeBackwardSet = baseData.unsafeBackwardSet.CloneAsThawed();
                }
                // Add the ranges from the data file to the unsafe-backward set.
                USerializedSet sset = new USerializedSet();
                char[] unsafeData = ICUBinary.GetChars(inBytes, length / 2, length & 1);
                length = 0;
                sset.GetSet(unsafeData, 0);
                int count = sset.CountRanges();
                int[] range = new int[2];
                for (int i = 0; i < count; ++i)
                {
                    sset.GetRange(i, range);
                    tailoring.UnsafeBackwardSet.Add(range[0], range[1]);
                }
                // Mark each lead surrogate as "unsafe"
                // if any of its 1024 associated supplementary code points is "unsafe".
                int c = 0x10000;
                for (int lead = 0xd800; lead < 0xdc00; ++lead, c += 0x400)
                {
                    if (!tailoring.UnsafeBackwardSet.ContainsNone(c, c + 0x3ff))
                    {
                        tailoring.UnsafeBackwardSet.Add(lead);
                    }
                }
                tailoring.UnsafeBackwardSet.Freeze();
                data.unsafeBackwardSet = tailoring.UnsafeBackwardSet;
            }
            else if (data == null)
            {
                // Nothing to do.
            }
            else if (baseData != null)
            {
                // No tailoring-specific data: Alias the root collator's set.
                data.unsafeBackwardSet = baseData.unsafeBackwardSet;
            }
            else
            {
                throw new ICUException("Missing unsafe-backward-set");
            }
            ICUBinary.SkipBytes(inBytes, length);

            // If the fast Latin format version is different,
            // or the version is set to 0 for "no fast Latin table",
            // then just always use the normal string comparison path.
            index = IX_FAST_LATIN_TABLE_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (data != null)
            {
                data.fastLatinTable = null;
                data.fastLatinTableHeader = null;
                if (((inIndexes[IX_OPTIONS] >> 16) & 0xff) == CollationFastLatin.Version)
                {
                    if (length >= 2)
                    {
                        char header0 = inBytes.GetChar();
                        int headerLength = header0 & 0xff;
                        data.fastLatinTableHeader = new char[headerLength];
                        data.fastLatinTableHeader[0] = header0;
                        for (int i = 1; i < headerLength; ++i)
                        {
                            data.fastLatinTableHeader[i] = inBytes.GetChar();
                        }
                        int tableLength = length / 2 - headerLength;
                        data.fastLatinTable = ICUBinary.GetChars(inBytes, tableLength, length & 1);
                        length = 0;
                        if ((header0 >> 8) != CollationFastLatin.Version)
                        {
                            throw new ICUException("Fast-Latin table version differs from version in data header");
                        }
                    }
                    else if (baseData != null)
                    {
                        data.fastLatinTable = baseData.fastLatinTable;
                        data.fastLatinTableHeader = baseData.fastLatinTableHeader;
                    }
                }
            }
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_SCRIPTS_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 2)
            {
                if (data == null)
                {
                    throw new ICUException("Script order data but no mappings");
                }
                int scriptsLength = length / 2;
                CharBuffer inChars = inBytes.AsCharBuffer();
                data.numScripts = inChars.Get();
                // There must be enough entries for both arrays, including more than two range starts.
                int scriptStartsLength = scriptsLength - (1 + data.numScripts + 16);
                if (scriptStartsLength <= 2)
                {
                    throw new ICUException("Script order data too short");
                }
                inChars.Get(data.scriptsIndex = new char[data.numScripts + 16]);
                inChars.Get(data.scriptStarts = new char[scriptStartsLength]);
                if (!(data.scriptStarts[0] == 0 &&
                        data.scriptStarts[1] == ((Collation.MergeSeparatorByte + 1) << 8) &&
                        data.scriptStarts[scriptStartsLength - 1] ==
                                (Collation.TRAIL_WEIGHT_BYTE << 8)))
                {
                    throw new ICUException("Script order data not valid");
                }
            }
            else if (data == null)
            {
                // Nothing to do.
            }
            else if (baseData != null)
            {
                data.numScripts = baseData.numScripts;
                data.scriptsIndex = baseData.scriptsIndex;
                data.scriptStarts = baseData.scriptStarts;
            }
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_COMPRESSIBLE_BYTES_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            if (length >= 256)
            {
                if (data == null)
                {
                    throw new ICUException("Data for compressible primary lead bytes but no mappings");
                }
                data.compressibleBytes = new bool[256];
                for (int i = 0; i < 256; ++i)
                {
                    data.compressibleBytes[i] = inBytes.Get() != 0;
                }
                length -= 256;
            }
            else if (data == null)
            {
                // Nothing to do.
            }
            else if (baseData != null)
            {
                data.compressibleBytes = baseData.compressibleBytes;
            }
            else
            {
                throw new ICUException("Missing data for compressible primary lead bytes");
            }
            ICUBinary.SkipBytes(inBytes, length);

            index = IX_RESERVED18_OFFSET;
            offset = inIndexes[index];
            length = inIndexes[index + 1] - offset;
            ICUBinary.SkipBytes(inBytes, length);

            CollationSettings ts = tailoring.Settings.ReadOnly;
            int options = inIndexes[IX_OPTIONS] & 0xffff;
            char[] fastLatinPrimaries = new char[CollationFastLatin.LatinLimit];
            int fastLatinOptions = CollationFastLatin.GetOptions(
                    tailoring.Data, ts, fastLatinPrimaries);
            if (options == ts.Options && ts.VariableTop != 0 &&
                    Arrays.Equals(reorderCodes, ts.ReorderCodes) &&
                    fastLatinOptions == ts.FastLatinOptions &&
                    (fastLatinOptions < 0 ||
                            Arrays.Equals(fastLatinPrimaries, ts.FastLatinPrimaries)))
            {
                return;
            }

            CollationSettings settings = tailoring.Settings.CopyOnWrite();
            settings.Options = options;
            // Set variableTop from options and scripts data.
            settings.VariableTop = tailoring.Data.GetLastPrimaryForGroup(
                    ReorderCodes.First + settings.MaxVariable);
            if (settings.VariableTop == 0)
            {
                throw new ICUException("The maxVariable could not be mapped to a variableTop");
            }

            if (reorderCodesLength != 0)
            {
                settings.AliasReordering(baseData, reorderCodes, reorderCodesLength, reorderTable);
            }

            settings.FastLatinOptions = CollationFastLatin.GetOptions(
                tailoring.Data, settings,
                settings.FastLatinPrimaries);
        }

        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 5;
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();
        private const int DATA_FORMAT = 0x55436f6c;  // "UCol"

        // ICU4N specific - made class static instead of having private constructor

        /*
         * Format of collation data (ucadata.icu, binary data in coll/ *.res files):
         * See ICU4C source/common/collationdatareader.h.
         */
    }
}
