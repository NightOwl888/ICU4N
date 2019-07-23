using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <since>2013aug09</since>
    /// <author>Markus W. Scherer</author>
    internal sealed class CollationFastLatinBuilder
    {
        // #define DEBUG_COLLATION_FAST_LATIN_BUILDER 0  // 0 or 1 or 2

        /**
         * Compare two signed long values as if they were unsigned.
         */
        private static int CompareInt64AsUnsigned(long a, long b)
        {
            a += unchecked((long)0x8000000000000000L);
            b += unchecked((long)0x8000000000000000L);
            if (a < b)
            {
                return -1;
            }
            else if (a > b)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /**
         * Like Java Collections.binarySearch(List, String, Comparator).
         *
         * @return the index>=0 where the item was found,
         *         or the index<0 for inserting the string at ~index in sorted order
         */
        private static int BinarySearch(IList<long> list, int limit, long ce)
        {
            if (limit == 0) { return ~0; }
            int start = 0;
            for (; ; )
            {
                int i = (int)(((long)start + (long)limit) / 2);
                int cmp = CompareInt64AsUnsigned(ce, list[i]);
                if (cmp == 0)
                {
                    return i;
                }
                else if (cmp < 0)
                {
                    if (i == start)
                    {
                        return ~start;  // insert ce before i
                    }
                    limit = i;
                }
                else
                {
                    if (i == start)
                    {
                        return ~(start + 1);  // insert ce after i
                    }
                    start = i;
                }
            }
        }

        internal CollationFastLatinBuilder()
        {
            ce0 = 0;
            ce1 = 0;
            contractionCEs = new List<long>(32);
            uniqueCEs = new List<long>(32);
            miniCEs = null;
            firstDigitPrimary = 0;
            firstLatinPrimary = 0;
            lastLatinPrimary = 0;
            firstShortPrimary = 0;
            shortPrimaryOverflow = false;
            headerLength = 0;
        }

        internal bool ForData(CollationData data)
        {
            if (result.Length != 0)
            {  // This builder is not reusable.
                throw new InvalidOperationException("attempt to reuse a CollationFastLatinBuilder");
            }
            if (!LoadGroups(data)) { return false; }

            // Fast handling of digits.
            firstShortPrimary = firstDigitPrimary;
            GetCEs(data);
            EncodeUniqueCEs();
            if (shortPrimaryOverflow)
            {
                // Give digits long mini primaries,
                // so that there are more short primaries for letters.
                firstShortPrimary = firstLatinPrimary;
                ResetCEs();
                GetCEs(data);
                EncodeUniqueCEs();
            }
            // Note: If we still have a short-primary overflow but not a long-primary overflow,
            // then we could calculate how many more long primaries would fit,
            // and set the firstShortPrimary to that many after the current firstShortPrimary,
            // and try again.
            // However, this might only benefit the en_US_POSIX tailoring,
            // and it is simpler to suppress building fast Latin data for it in genrb,
            // or by returning false here if shortPrimaryOverflow.

            bool ok = !shortPrimaryOverflow;
            if (ok)
            {
                EncodeCharCEs();
                EncodeContractions();
            }
            contractionCEs.Clear();  // might reduce heap memory usage
            uniqueCEs.Clear();
            return ok;
        }

        // C++ returns one combined array with the contents of the result buffer.
        // Java returns two arrays (header & table) because we cannot use pointer arithmetic,
        // and we do not want to index into the table with an offset.
        internal char[] GetHeader()
        {
            char[] resultArray = new char[headerLength];
            //result.GetChars(0, headerLength, resultArray, 0);
            result.CopyTo(0, resultArray, 0, headerLength - 0);
            return resultArray;
        }

        internal char[] GetTable()
        {
            char[] resultArray = new char[result.Length - headerLength];
            //result.getChars(headerLength, result.Length, resultArray, 0);
            result.CopyTo(headerLength, resultArray, 0, result.Length - headerLength); // ICU4N TODO: check this
            return resultArray;
        }

        private bool LoadGroups(CollationData data)
        {
            headerLength = 1 + NUM_SPECIAL_GROUPS;
            int r0 = (CollationFastLatin.VERSION << 8) | headerLength;
            result.Append((char)r0);
            // The first few reordering groups should be special groups
            // (space, punct, ..., digit) followed by Latn, then Grek and other scripts.
            for (int i = 0; i < NUM_SPECIAL_GROUPS; ++i)
            {
                lastSpecialPrimaries[i] = data.GetLastPrimaryForGroup(ReorderCodes.First + i);
                if (lastSpecialPrimaries[i] == 0)
                {
                    // missing data
                    return false;
                }
                // ICU4N TODO: Check this (not sure about char data type)
                result.Append((char)0);  // reserve a slot for this group
            }

            firstDigitPrimary = data.GetFirstPrimaryForGroup(ReorderCodes.Digit);
            firstLatinPrimary = data.GetFirstPrimaryForGroup(UScript.Latin);
            lastLatinPrimary = data.GetLastPrimaryForGroup(UScript.Latin);
            if (firstDigitPrimary == 0 || firstLatinPrimary == 0)
            {
                // missing data
                return false;
            }
            return true;
        }

        private bool InSameGroup(long p, long q)
        {
            // Both or neither need to be encoded as short primaries,
            // so that we can test only one and use the same bit mask.
            if (p >= firstShortPrimary)
            {
                return q >= firstShortPrimary;
            }
            else if (q >= firstShortPrimary)
            {
                return false;
            }
            // Both or neither must be potentially-variable,
            // so that we can test only one and determine if both are variable.
            long lastVariablePrimary = lastSpecialPrimaries[NUM_SPECIAL_GROUPS - 1];
            if (p > lastVariablePrimary)
            {
                return q > lastVariablePrimary;
            }
            else if (q > lastVariablePrimary)
            {
                return false;
            }
            // Both will be encoded with long mini primaries.
            // They must be in the same special reordering group,
            // so that we can test only one and determine if both are variable.
            Debug.Assert(p != 0 && q != 0);
            for (int i = 0; ; ++i)
            {  // will terminate
                long lastPrimary = lastSpecialPrimaries[i];
                if (p <= lastPrimary)
                {
                    return q <= lastPrimary;
                }
                else if (q <= lastPrimary)
                {
                    return false;
                }
            }
        }

        private void ResetCEs()
        {
            contractionCEs.Clear();
            uniqueCEs.Clear();
            shortPrimaryOverflow = false;
            result.Length = headerLength;
        }

        private void GetCEs(CollationData data)
        {
            int i = 0;
            for (char c = (char)0; ; ++i, ++c)
            {
                if (c == CollationFastLatin.LATIN_LIMIT)
                {
                    c = (char)CollationFastLatin.PUNCT_START;
                }
                else if (c == CollationFastLatin.PUNCT_LIMIT)
                {
                    break;
                }
                CollationData d;
                int ce32 = data.GetCE32(c);
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    d = data.Base;
                    ce32 = d.GetCE32(c);
                }
                else
                {
                    d = data;
                }
                if (GetCEsFromCE32(d, c, ce32))
                {
                    charCEs[i][0] = ce0;
                    charCEs[i][1] = ce1;
                    AddUniqueCE(ce0);
                    AddUniqueCE(ce1);
                }
                else
                {
                    // bail out for c
                    charCEs[i][0] = ce0 = Collation.NO_CE;
                    charCEs[i][1] = ce1 = 0;
                }
                if (c == 0 && !IsContractionCharCE(ce0))
                {
                    // Always map U+0000 to a contraction.
                    // Write a contraction list with only a default value if there is no real contraction.
                    Debug.Assert(contractionCEs.Count == 0);
                    AddContractionEntry(CollationFastLatin.CONTR_CHAR_MASK, ce0, ce1);
                    charCEs[0][0] = (Collation.NO_CE_PRIMARY << 32) | CONTRACTION_FLAG;
                    charCEs[0][1] = 0;
                }
            }
            // Terminate the last contraction list.
            contractionCEs.Add(CollationFastLatin.CONTR_CHAR_MASK);
        }

        private bool GetCEsFromCE32(CollationData data, int c, int ce32)
        {
            ce32 = data.GetFinalCE32(ce32);
            ce1 = 0;
            if (Collation.IsSimpleOrLongCE32(ce32))
            {
                ce0 = Collation.CeFromCE32(ce32);
            }
            else
            {
                switch (Collation.TagFromCE32(ce32))
                {
                    case Collation.LATIN_EXPANSION_TAG:
                        ce0 = Collation.LatinCE0FromCE32(ce32);
                        ce1 = Collation.LatinCE1FromCE32(ce32);
                        break;
                    case Collation.EXPANSION32_TAG:
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            if (length <= 2)
                            {
                                ce0 = Collation.CeFromCE32(data.ce32s[index]);
                                if (length == 2)
                                {
                                    ce1 = Collation.CeFromCE32(data.ce32s[index + 1]);
                                }
                                break;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case Collation.EXPANSION_TAG:
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            if (length <= 2)
                            {
                                ce0 = data.ces[index];
                                if (length == 2)
                                {
                                    ce1 = data.ces[index + 1];
                                }
                                break;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    // Note: We could support PREFIX_TAG (assert c>=0)
                    // by recursing on its default CE32 and checking that none of the prefixes starts
                    // with a fast Latin character.
                    // However, currently (2013) there are only the L-before-middle-dot
                    // prefix mappings in the Latin range, and those would be rejected anyway.
                    case Collation.CONTRACTION_TAG:
                        Debug.Assert(c >= 0);
                        return GetCEsFromContractionCE32(data, ce32);
                    case Collation.OFFSET_TAG:
                        Debug.Assert(c >= 0);
                        ce0 = data.GetCEFromOffsetCE32(c, ce32);
                        break;
                    default:
                        return false;
                }
            }
            // A mapping can be completely ignorable.
            if (ce0 == 0) { return ce1 == 0; }
            // We do not support an ignorable ce0 unless it is completely ignorable.
            long p0 = ce0.TripleShift(32);
            if (p0 == 0) { return false; }
            // We only support primaries up to the Latin script.
            if (p0 > lastLatinPrimary) { return false; }
            // We support non-common secondary and case weights only together with short primaries.
            int lower32_0 = (int)ce0;
            if (p0 < firstShortPrimary)
            {
                int sc0 = lower32_0 & Collation.SECONDARY_AND_CASE_MASK;
                if (sc0 != Collation.COMMON_SECONDARY_CE) { return false; }
            }
            // No below-common tertiary weights.
            if ((lower32_0 & Collation.ONLY_TERTIARY_MASK) < Collation.COMMON_WEIGHT16) { return false; }
            if (ce1 != 0)
            {
                // Both primaries must be in the same group,
                // or both must get short mini primaries,
                // or a short-primary CE is followed by a secondary CE.
                // This is so that we can test the first primary and use the same mask for both,
                // and determine for both whether they are variable.
                long p1 = ce1.TripleShift(32);
                if (p1 == 0 ? p0 < firstShortPrimary : !InSameGroup(p0, p1)) { return false; }
                int lower32_1 = (int)ce1;
                // No tertiary CEs.
                if ((lower32_1.TripleShift(16)) == 0) { return false; }
                // We support non-common secondary and case weights
                // only for secondary CEs or together with short primaries.
                if (p1 != 0 && p1 < firstShortPrimary)
                {
                    int sc1 = lower32_1 & Collation.SECONDARY_AND_CASE_MASK;
                    if (sc1 != Collation.COMMON_SECONDARY_CE) { return false; }
                }
                // No below-common tertiary weights.
                if ((lower32_0 & Collation.ONLY_TERTIARY_MASK) < Collation.COMMON_WEIGHT16) { return false; }
            }
            // No quaternary weights.
            if (((ce0 | ce1) & Collation.QUATERNARY_MASK) != 0) { return false; }
            return true;
        }

        private bool GetCEsFromContractionCE32(CollationData data, int ce32)
        {
            int trieIndex = Collation.IndexFromCE32(ce32);
            ce32 = data.GetCE32FromContexts(trieIndex);  // Default if no suffix match.
                                                         // Since the original ce32 is not a prefix mapping,
                                                         // the default ce32 must not be another contraction.
            Debug.Assert(!Collation.IsContractionCE32(ce32));
            int contractionIndex = contractionCEs.Count;
            if (GetCEsFromCE32(data, Collation.SENTINEL_CP, ce32))
            {
                AddContractionEntry(CollationFastLatin.CONTR_CHAR_MASK, ce0, ce1);
            }
            else
            {
                // Bail out for c-without-contraction.
                AddContractionEntry(CollationFastLatin.CONTR_CHAR_MASK, Collation.NO_CE, 0);
            }
            // Handle an encodable contraction unless the next contraction is too long
            // and starts with the same character.
            int prevX = -1;
            bool addContraction = false;
            using (CharsTrie.Enumerator suffixes = CharsTrie.GetEnumerator(data.contexts, trieIndex + 2, 0))
            {
                while (suffixes.MoveNext())
                {
                    CharsTrie.Entry entry = suffixes.Current;
                    ICharSequence suffix = entry.Chars;
                    int x = CollationFastLatin.GetCharIndex(suffix[0]);
                    if (x < 0) { continue; }  // ignore anything but fast Latin text
                    if (x == prevX)
                    {
                        if (addContraction)
                        {
                            // Bail out for all contractions starting with this character.
                            AddContractionEntry(x, Collation.NO_CE, 0);
                            addContraction = false;
                        }
                        continue;
                    }
                    if (addContraction)
                    {
                        AddContractionEntry(prevX, ce0, ce1);
                    }
                    ce32 = entry.Value;
                    if (suffix.Length == 1 && GetCEsFromCE32(data, Collation.SENTINEL_CP, ce32))
                    {
                        addContraction = true;
                    }
                    else
                    {
                        AddContractionEntry(x, Collation.NO_CE, 0);
                        addContraction = false;
                    }
                    prevX = x;
                }
            }
            if (addContraction)
            {
                AddContractionEntry(prevX, ce0, ce1);
            }
            // Note: There might not be any fast Latin contractions, but
            // we need to enter contraction handling anyway so that we can bail out
            // when there is a non-fast-Latin character following.
            // For example: Danish &Y<<u+umlaut, when we compare Y vs. u\u0308 we need to see the
            // following umlaut and bail out, rather than return the difference of Y vs. u.
            ce0 = (Collation.NO_CE_PRIMARY << 32) | CONTRACTION_FLAG | (uint)contractionIndex;
            ce1 = 0;
            return true;
        }

        private void AddContractionEntry(int x, long cce0, long cce1)
        {
            contractionCEs.Add(x);
            contractionCEs.Add(cce0);
            contractionCEs.Add(cce1);
            AddUniqueCE(cce0);
            AddUniqueCE(cce1);
        }

        private void AddUniqueCE(long ce)
        {
            if (ce == 0 || (ce.TripleShift(32)) == Collation.NO_CE_PRIMARY) { return; }
            ce &= ~(long)Collation.CASE_MASK;  // blank out case bits
            int i = BinarySearch(uniqueCEs, uniqueCEs.Count, ce);
            if (i < 0)
            {
                uniqueCEs.Insert(~i, ce);
            }
        }

        private int GetMiniCE(long ce)
        {
            ce &= ~(long)Collation.CASE_MASK;  // blank out case bits
            int index = BinarySearch(uniqueCEs, uniqueCEs.Count, ce);
            Debug.Assert(index >= 0);
            return miniCEs[index];
        }

        private void EncodeUniqueCEs()
        {
            miniCEs = new char[uniqueCEs.Count];
            int group = 0;
            long lastGroupPrimary = lastSpecialPrimaries[group];
            // The lowest unique CE must be at least a secondary CE.
            Debug.Assert((((int)uniqueCEs[0]).TripleShift(16)) != 0);
            long prevPrimary = 0;
            int prevSecondary = 0;
            int pri = 0;
            int sec = 0;
            int ter = CollationFastLatin.COMMON_TER;
            for (int i = 0; i < uniqueCEs.Count; ++i)
            {
                long ce = uniqueCEs[i];
                // Note: At least one of the p/s/t weights changes from one unique CE to the next.
                // (uniqueCEs does not store case bits.)
                long p = ce.TripleShift(32);
                if (p != prevPrimary)
                {
                    while (p > lastGroupPrimary)
                    {
                        Debug.Assert(pri <= CollationFastLatin.MAX_LONG);
                        // Set the group's header entry to the
                        // last "long primary" in or before the group.
                        result[1 + group] = (char)pri;
                        if (++group < NUM_SPECIAL_GROUPS)
                        {
                            lastGroupPrimary = lastSpecialPrimaries[group];
                        }
                        else
                        {
                            lastGroupPrimary = 0xffffffffL;
                            break;
                        }
                    }
                    if (p < firstShortPrimary)
                    {
                        if (pri == 0)
                        {
                            pri = CollationFastLatin.MIN_LONG;
                        }
                        else if (pri < CollationFastLatin.MAX_LONG)
                        {
                            pri += CollationFastLatin.LONG_INC;
                        }
                        else
                        {
                            /* #if DEBUG_COLLATION_FAST_LATIN_BUILDER
                                                printf("long-primary overflow for %08x\n", p);
                            #endif */
                            miniCEs[i] = CollationFastLatin.BAIL_OUT;
                            continue;
                        }
                    }
                    else
                    {
                        if (pri < CollationFastLatin.MIN_SHORT)
                        {
                            pri = CollationFastLatin.MIN_SHORT;
                        }
                        else if (pri < (CollationFastLatin.MAX_SHORT - CollationFastLatin.SHORT_INC))
                        {
                            // Reserve the highest primary weight for U+FFFF.
                            pri += CollationFastLatin.SHORT_INC;
                        }
                        else
                        {
                            /* #if DEBUG_COLLATION_FAST_LATIN_BUILDER
                                                printf("short-primary overflow for %08x\n", p);
                            #endif */
                            shortPrimaryOverflow = true;
                            miniCEs[i] = CollationFastLatin.BAIL_OUT;
                            continue;
                        }
                    }
                    prevPrimary = p;
                    prevSecondary = Collation.COMMON_WEIGHT16;
                    sec = CollationFastLatin.COMMON_SEC;
                    ter = CollationFastLatin.COMMON_TER;
                }
                int lower32 = (int)ce;
                int s = lower32.TripleShift(16);
                if (s != prevSecondary)
                {
                    if (pri == 0)
                    {
                        if (sec == 0)
                        {
                            sec = CollationFastLatin.MIN_SEC_HIGH;
                        }
                        else if (sec < CollationFastLatin.MAX_SEC_HIGH)
                        {
                            sec += CollationFastLatin.SEC_INC;
                        }
                        else
                        {
                            miniCEs[i] = CollationFastLatin.BAIL_OUT;
                            continue;
                        }
                        prevSecondary = s;
                        ter = CollationFastLatin.COMMON_TER;
                    }
                    else if (s < Collation.COMMON_WEIGHT16)
                    {
                        if (sec == CollationFastLatin.COMMON_SEC)
                        {
                            sec = CollationFastLatin.MIN_SEC_BEFORE;
                        }
                        else if (sec < CollationFastLatin.MAX_SEC_BEFORE)
                        {
                            sec += CollationFastLatin.SEC_INC;
                        }
                        else
                        {
                            miniCEs[i] = CollationFastLatin.BAIL_OUT;
                            continue;
                        }
                    }
                    else if (s == Collation.COMMON_WEIGHT16)
                    {
                        sec = CollationFastLatin.COMMON_SEC;
                    }
                    else
                    {
                        if (sec < CollationFastLatin.MIN_SEC_AFTER)
                        {
                            sec = CollationFastLatin.MIN_SEC_AFTER;
                        }
                        else if (sec < CollationFastLatin.MAX_SEC_AFTER)
                        {
                            sec += CollationFastLatin.SEC_INC;
                        }
                        else
                        {
                            miniCEs[i] = (char)CollationFastLatin.BAIL_OUT;
                            continue;
                        }
                    }
                    prevSecondary = s;
                    ter = CollationFastLatin.COMMON_TER;
                }
                Debug.Assert((lower32 & Collation.CASE_MASK) == 0);  // blanked out in uniqueCEs
                int t = lower32 & Collation.ONLY_TERTIARY_MASK;
                if (t > Collation.COMMON_WEIGHT16)
                {
                    if (ter < CollationFastLatin.MAX_TER_AFTER)
                    {
                        ++ter;
                    }
                    else
                    {
                        miniCEs[i] = CollationFastLatin.BAIL_OUT;
                        continue;
                    }
                }
                if (CollationFastLatin.MIN_LONG <= pri && pri <= CollationFastLatin.MAX_LONG)
                {
                    Debug.Assert(sec == CollationFastLatin.COMMON_SEC);
                    miniCEs[i] = (char)(pri | ter);
                }
                else
                {
                    miniCEs[i] = (char)(pri | sec | ter);
                }
            }
            /* #if DEBUG_COLLATION_FAST_LATIN_BUILDER
                printf("last mini primary: %04x\n", pri);
            #endif */
            /* #if DEBUG_COLLATION_FAST_LATIN_BUILDER >= 2
                for(int i = 0; i < uniqueCEs.size(); ++i) {
                    long ce = uniqueCEs.elementAti(i);
                    printf("unique CE 0x%016lx -> 0x%04x\n", ce, miniCEs[i]);
                }
            #endif */
        }

        private void EncodeCharCEs()
        {
            int miniCEsStart = result.Length;
            for (int i = 0; i < CollationFastLatin.NUM_FAST_CHARS; ++i)
            {
                // ICU4N TODO: Check this (not sure if this should be char)
                result.Append((char)0);  // initialize to completely ignorable
            }
            int indexBase = result.Length;
            for (int i = 0; i < CollationFastLatin.NUM_FAST_CHARS; ++i)
            {
                long ce = charCEs[i][0];
                if (IsContractionCharCE(ce)) { continue; }  // defer contraction
                int miniCE = EncodeTwoCEs(ce, charCEs[i][1]);
                if ((miniCE.TripleShift(16)) > 0)
                {   // if ((unsigned)miniCE > 0xffff)
                    // Note: There is a chance that this new expansion is the same as a previous one,
                    // and if so, then we could reuse the other expansion.
                    // However, that seems unlikely.
                    int expansionIndex = result.Length - indexBase;
                    if (expansionIndex > CollationFastLatin.INDEX_MASK)
                    {
                        miniCE = CollationFastLatin.BAIL_OUT;
                    }
                    else
                    {
                        result.Append((char)(miniCE >> 16)).Append((char)miniCE);
                        miniCE = CollationFastLatin.EXPANSION | expansionIndex;
                    }
                }
                result[miniCEsStart + i] = (char)miniCE;
            }
        }

        private void EncodeContractions()
        {
            // We encode all contraction lists so that the first word of a list
            // terminates the previous list, and we only need one additional terminator at the end.
            int indexBase = headerLength + CollationFastLatin.NUM_FAST_CHARS;
            int firstContractionIndex = result.Length;
            for (int i = 0; i < CollationFastLatin.NUM_FAST_CHARS; ++i)
            {
                long ce = charCEs[i][0];
                if (!IsContractionCharCE(ce)) { continue; }
                int contractionIndex = result.Length - indexBase;
                if (contractionIndex > CollationFastLatin.INDEX_MASK)
                {
                    result[headerLength + i] = (char)CollationFastLatin.BAIL_OUT;
                    continue;
                }
                bool firstTriple = true;
                for (int index = (int)ce & 0x7fffffff; ; index += 3)
                {
                    long x = contractionCEs[index];
                    if (x == CollationFastLatin.CONTR_CHAR_MASK && !firstTriple) { break; }
                    long cce0 = contractionCEs[index + 1];
                    long cce1 = contractionCEs[index + 2];
                    int miniCE = EncodeTwoCEs(cce0, cce1);
                    if (miniCE == CollationFastLatin.BAIL_OUT)
                    {
                        result.Append((char)(x | (uint)(1 << CollationFastLatin.CONTR_LENGTH_SHIFT)));
                    }
                    else if ((miniCE.TripleShift(16)) == 0)
                    {  // if ((unsigned)miniCE <= 0xffff)
                        result.Append((char)(x | (uint)(2 << CollationFastLatin.CONTR_LENGTH_SHIFT)));
                        result.Append((char)miniCE);
                    }
                    else
                    {
                        result.Append((char)(x | (uint)(3 << CollationFastLatin.CONTR_LENGTH_SHIFT)));
                        result.Append((char)(miniCE >> 16)).Append((char)miniCE);
                    }
                    firstTriple = false;
                }
                // Note: There is a chance that this new contraction list is the same as a previous one,
                // and if so, then we could truncate the result and reuse the other list.
                // However, that seems unlikely.
                result[headerLength + i] =
                                (char)(CollationFastLatin.CONTRACTION | contractionIndex);
            }
            if (result.Length > firstContractionIndex)
            {
                // Terminate the last contraction list.
                result.Append((char)CollationFastLatin.CONTR_CHAR_MASK);
            }
            /* #if DEBUG_COLLATION_FAST_LATIN_BUILDER
                printf("** fast Latin %d * 2 = %d bytes\n", result.Length, result.Length * 2);
                puts("   header & below-digit groups map");
                int i = 0;
                for(; i < headerLength; ++i) {
                    printf(" %04x", result[i]);
                }
                printf("\n   char mini CEs");
                assert(CollationFastLatin.NUM_FAST_CHARS % 16 == 0);
                for(; i < indexBase; i += 16) {
                    int c = i - headerLength;
                    if(c >= CollationFastLatin.LATIN_LIMIT) {
                        c = CollationFastLatin.PUNCT_START + c - CollationFastLatin.LATIN_LIMIT;
                    }
                    printf("\n %04x:", c);
                    for(int j = 0; j < 16; ++j) {
                        printf(" %04x", result[i + j]);
                    }
                }
                printf("\n   expansions & contractions");
                for(; i < result.Length; ++i) {
                    if((i - indexBase) % 16 == 0) { puts(""); }
                    printf(" %04x", result[i]);
                }
                puts("");
            #endif */
        }

        private int EncodeTwoCEs(long first, long second)
        {
            if (first == 0)
            {
                return 0;  // completely ignorable
            }
            if (first == Collation.NO_CE)
            {
                return CollationFastLatin.BAIL_OUT;
            }
            Debug.Assert((first.TripleShift(32)) != Collation.NO_CE_PRIMARY);

            int miniCE = GetMiniCE(first);
            if (miniCE == CollationFastLatin.BAIL_OUT) { return miniCE; }
            if (miniCE >= CollationFastLatin.MIN_SHORT)
            {
                // Extract & copy the case bits.
                // Shift them from normal CE bits 15..14 to mini CE bits 4..3.
                int c = (((int)first & Collation.CASE_MASK) >> (14 - 3));
                // Only in mini CEs: Ignorable case bits = 0, lowercase = 1.
                c += CollationFastLatin.LOWER_CASE;
                miniCE |= c;
            }
            if (second == 0) { return miniCE; }

            int miniCE1 = GetMiniCE(second);
            if (miniCE1 == CollationFastLatin.BAIL_OUT) { return miniCE1; }

            int case1 = (int)second & Collation.CASE_MASK;
            if (miniCE >= CollationFastLatin.MIN_SHORT &&
                    (miniCE & CollationFastLatin.SECONDARY_MASK) == CollationFastLatin.COMMON_SEC)
            {
                // Try to combine the two mini CEs into one.
                int sec1 = miniCE1 & CollationFastLatin.SECONDARY_MASK;
                int ter1 = miniCE1 & CollationFastLatin.TERTIARY_MASK;
                if (sec1 >= CollationFastLatin.MIN_SEC_HIGH && case1 == 0 &&
                        ter1 == CollationFastLatin.COMMON_TER)
                {
                    // sec1>=sec_high implies pri1==0.
                    return (miniCE & ~CollationFastLatin.SECONDARY_MASK) | sec1;
                }
            }

            if (miniCE1 <= CollationFastLatin.SECONDARY_MASK || CollationFastLatin.MIN_SHORT <= miniCE1)
            {
                // Secondary CE, or a CE with a short primary, copy the case bits.
                case1 = (case1 >> (14 - 3)) + CollationFastLatin.LOWER_CASE;
                miniCE1 |= case1;
            }
            return (miniCE << 16) | miniCE1;
        }

        private static bool IsContractionCharCE(long ce)
        {
            return (ce.TripleShift(32)) == Collation.NO_CE_PRIMARY && ce != Collation.NO_CE;
        }

        // space, punct, symbol, currency (not digit)
        private static readonly int NUM_SPECIAL_GROUPS =
                ReorderCodes.Currency - ReorderCodes.First + 1;

        private static readonly long CONTRACTION_FLAG = 0x80000000L;

        // temporary "buffer"
        private long ce0, ce1;

        private long[][] charCEs = Arrays.NewRectangularArray<long>(CollationFastLatin.NUM_FAST_CHARS, 2); //new long[CollationFastLatin.NUM_FAST_CHARS][2];

        private IList<long> contractionCEs;
        private IList<long> uniqueCEs;

        /** One 16-bit mini CE per unique CE. */
        private char[] miniCEs;

        // These are constant for a given root collator.
        long[] lastSpecialPrimaries = new long[NUM_SPECIAL_GROUPS];
        private long firstDigitPrimary;
        private long firstLatinPrimary;
        private long lastLatinPrimary;
        // This determines the first normal primary weight which is mapped to
        // a short mini primary. It must be >=firstDigitPrimary.
        private long firstShortPrimary;

        private bool shortPrimaryOverflow;

        private StringBuilder result = new StringBuilder();
        private int headerLength;
    }
}
