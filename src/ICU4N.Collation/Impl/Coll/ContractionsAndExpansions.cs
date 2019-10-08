using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    // ContractionsAndExpansions.cs, ported from collationsets.h/.cpp
    //
    // C++ version created on: 2013feb09
    // created by: Markus W. Scherer

    public interface ICESink
    {
        void HandleCE(long ce);
        void HandleExpansion(IList<long> ces, int start, int length);
    }

    public sealed class ContractionsAndExpansions
    {
        // C++: The following fields are @internal, only public for access by callback.
        private CollationData data;
        private UnicodeSet contractions;
        private UnicodeSet expansions;
        private ICESink sink;
        private bool addPrefixes;
        private int checkTailored = 0;  // -1: collected tailored  +1: exclude tailored
        private UnicodeSet tailored = new UnicodeSet();
        private UnicodeSet ranges;
        private StringBuilder unreversedPrefix = new StringBuilder();
        private string suffix;
        private long[] ces = new long[Collation.MAX_EXPANSION_LENGTH];

        // ICU4N specific - de-nested ICESink interface

        public ContractionsAndExpansions(UnicodeSet con, UnicodeSet exp, ICESink s, bool prefixes)
        {
            contractions = con;
            expansions = exp;
            sink = s;
            addPrefixes = prefixes;
        }

        public void ForData(CollationData d)
        {
            // Add all from the data, can be tailoring or base.
            if (d.Base != null) {
                checkTailored = -1;
            }
            data = d;
            using (IEnumerator<Trie2Range> trieIterator = data.trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumCnERange(range.StartCodePoint, range.EndCodePoint, range.Value, this);
                }
            }
            if (d.Base == null) {
                    return;
                }
                // Add all from the base data but only for un-tailored code points.
                tailored.Freeze();
                checkTailored = 1;
                data = d.Base;
            using (IEnumerator<Trie2Range> trieIterator = data.trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumCnERange(range.StartCodePoint, range.EndCodePoint, range.Value, this);
                }
            }
        }

        private void EnumCnERange(int start, int end, int ce32, ContractionsAndExpansions cne)
        {
            if (cne.checkTailored == 0)
            {
                // There is no tailoring.
                // No need to collect nor check the tailored set.
            }
            else if (cne.checkTailored < 0)
            {
                // Collect the set of code points with mappings in the tailoring data.
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    return; // fallback to base, not tailored
                }
                else
                {
                    cne.tailored.Add(start, end);
                }
                // checkTailored > 0: Exclude tailored ranges from the base data enumeration.
            }
            else if (start == end)
            {
                if (cne.tailored.Contains(start))
                {
                    return;
                }
            }
            else if (cne.tailored.ContainsSome(start, end))
            {
                if (cne.ranges == null)
                {
                    cne.ranges = new UnicodeSet();
                }
                cne.ranges.Set(start, end).RemoveAll(cne.tailored);
                int count = cne.ranges.RangeCount;
                for (int i = 0; i < count; ++i)
                {
                    cne.HandleCE32(cne.ranges.GetRangeStart(i), cne.ranges.GetRangeEnd(i), ce32);
                }
            }
            cne.HandleCE32(start, end, ce32);
        }

        public void ForCodePoint(CollationData d, int c)
        {
            int ce32 = d.GetCE32(c);
            if (ce32 == Collation.FALLBACK_CE32)
            {
                d = d.Base;
                ce32 = d.GetCE32(c);
            }
            data = d;
            HandleCE32(c, c, ce32);
        }

        private void HandleCE32(int start, int end, int ce32)
        {
            for (; ; )
            {
                if ((ce32 & 0xff) < Collation.SPECIAL_CE32_LOW_BYTE)
                {
                    // !isSpecialCE32()
                    if (sink != null)
                    {
                        sink.HandleCE(Collation.CeFromSimpleCE32(ce32));
                    }
                    return;
                }
                switch (Collation.TagFromCE32(ce32))
                {
                    case Collation.FALLBACK_TAG:
                        return;
                    case Collation.RESERVED_TAG_3:
                    case Collation.BUILDER_DATA_TAG:
                    case Collation.LEAD_SURROGATE_TAG:
                        // Java porting note: U_INTERNAL_PROGRAM_ERROR is set to errorCode in ICU4C.
                        throw new InvalidOperationException(
                                string.Format("Unexpected CE32 tag type {0} for ce32=0x{1:x8}",
                                        Collation.TagFromCE32(ce32), ce32));
                    case Collation.LONG_PRIMARY_TAG:
                        if (sink != null)
                        {
                            sink.HandleCE(Collation.CeFromLongPrimaryCE32(ce32));
                        }
                        return;
                    case Collation.LONG_SECONDARY_TAG:
                        if (sink != null)
                        {
                            sink.HandleCE(Collation.CeFromLongSecondaryCE32(ce32));
                        }
                        return;
                    case Collation.LATIN_EXPANSION_TAG:
                        if (sink != null)
                        {
                            ces[0] = Collation.LatinCE0FromCE32(ce32);
                            ces[1] = Collation.LatinCE1FromCE32(ce32);
                            sink.HandleExpansion(ces, 0, 2);
                        }
                        // Optimization: If we have a prefix,
                        // then the relevant strings have been added already.
                        if (unreversedPrefix.Length == 0)
                        {
                            AddExpansions(start, end);
                        }
                        return;
                    case Collation.EXPANSION32_TAG:
                        if (sink != null)
                        {
                            int idx = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            for (int i = 0; i < length; ++i)
                            {
                                ces[i] = Collation.CeFromCE32(data.ce32s[idx + i]);
                            }
                            sink.HandleExpansion(ces, 0, length);
                        }
                        // Optimization: If we have a prefix,
                        // then the relevant strings have been added already.
                        if (unreversedPrefix.Length == 0)
                        {
                            AddExpansions(start, end);
                        }
                        return;
                    case Collation.EXPANSION_TAG:
                        if (sink != null)
                        {
                            int idx = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            sink.HandleExpansion(data.ces, idx, length);
                        }
                        // Optimization: If we have a prefix,
                        // then the relevant strings have been added already.
                        if (unreversedPrefix.Length == 0)
                        {
                            AddExpansions(start, end);
                        }
                        return;
                    case Collation.PREFIX_TAG:
                        HandlePrefixes(start, end, ce32);
                        return;
                    case Collation.CONTRACTION_TAG:
                        HandleContractions(start, end, ce32);
                        return;
                    case Collation.DIGIT_TAG:
                        // Fetch the non-numeric-collation CE32 and continue.
                        ce32 = data.ce32s[Collation.IndexFromCE32(ce32)];
                        break;
                    case Collation.U0000_TAG:
                        Debug.Assert(start == 0 && end == 0);
                        // Fetch the normal ce32 for U+0000 and continue.
                        ce32 = data.ce32s[0];
                        break;
                    case Collation.HANGUL_TAG:
                        if (sink != null)
                        {
                            // TODO: This should be optimized,
                            // especially if [start..end] is the complete Hangul range. (assert that)
                            UTF16CollationIterator iter = new UTF16CollationIterator(data);
                            StringBuilderCharSequence hangul = new StringBuilderCharSequence(new StringBuilder(1));
                            for (int c = start; c <= end; ++c)
                            {
                                hangul.StringBuilder.Length=0;
                                hangul.StringBuilder.AppendCodePoint(c);
                                iter.SetText(false, hangul, 0);
                                int length = iter.FetchCEs();
                                // Ignore the terminating non-CE.
                                Debug.Assert(length >= 2 && iter.GetCE(length - 1) == Collation.NoCE);
                                sink.HandleExpansion(iter.GetCEs(), 0, length - 1);
                            }
                        }
                        // Optimization: If we have a prefix,
                        // then the relevant strings have been added already.
                        if (unreversedPrefix.Length == 0)
                        {
                            AddExpansions(start, end);
                        }
                        return;
                    case Collation.OFFSET_TAG:
                        // Currently no need to send offset CEs to the sink.
                        return;
                    case Collation.IMPLICIT_TAG:
                        // Currently no need to send implicit CEs to the sink.
                        return;
                }
            }
        }

        private void HandlePrefixes(int start, int end, int ce32)
        {
            int index = Collation.IndexFromCE32(ce32);
            ce32 = data.GetCE32FromContexts(index); // Default if no prefix match.
            HandleCE32(start, end, ce32);
            if (!addPrefixes)
            {
                return;
            }
            using (CharsTrie.Enumerator prefixes = new CharsTrie(data.contexts, index + 2).GetEnumerator())
            {
                while (prefixes.MoveNext())
                {
                    var e = prefixes.Current;
                    SetPrefix(e.Chars);
                    // Prefix/pre-context mappings are special kinds of contractions
                    // that always yield expansions.
                    AddStrings(start, end, contractions);
                    AddStrings(start, end, expansions);
                    HandleCE32(start, end, e.Value);
                }
            }
            ResetPrefix();
        }

        internal void HandleContractions(int start, int end, int ce32)
        {
            int index = Collation.IndexFromCE32(ce32);
            if ((ce32 & Collation.CONTRACT_SINGLE_CP_NO_MATCH) != 0)
            {
                // No match on the single code point.
                // We are underneath a prefix, and the default mapping is just
                // a fallback to the mappings for a shorter prefix.
                Debug.Assert(unreversedPrefix.Length != 0);
            }
            else
            {
                ce32 = data.GetCE32FromContexts(index); // Default if no suffix match.
                Debug.Assert(!Collation.IsContractionCE32(ce32));
                HandleCE32(start, end, ce32);
            }
            using (CharsTrie.Enumerator suffixes = new CharsTrie(data.contexts, index + 2).GetEnumerator())
            {
                while (suffixes.MoveNext())
                {
                    var e = suffixes.Current;
                    suffix = e.Chars.ToString();
                    AddStrings(start, end, contractions);
                    if (unreversedPrefix.Length != 0)
                    {
                        AddStrings(start, end, expansions);
                    }
                    HandleCE32(start, end, e.Value);
                }
            }
            suffix = null;
        }

        internal void AddExpansions(int start, int end)
        {
            if (unreversedPrefix.Length == 0 && suffix == null)
            {
                if (expansions != null)
                {
                    expansions.Add(start, end);
                }
            }
            else
            {
                AddStrings(start, end, expansions);
            }
        }

        internal void AddStrings(int start, int end, UnicodeSet set)
        {
            if (set == null)
            {
                return;
            }
            StringBuilder s = new StringBuilder(unreversedPrefix.ToString());
            do
            {
                s.AppendCodePoint(start);
                if (suffix != null)
                {
                    s.Append(suffix);
                }
                set.Add(s);
                s.Length=unreversedPrefix.Length;
            } while (++start <= end);
        }

        // Prefixes are reversed in the data structure.
        private void SetPrefix(ICharSequence pfx)
        {
            unreversedPrefix.Length=0;
            unreversedPrefix.Append(pfx).Reverse();
        }

        private void ResetPrefix()
        {
            unreversedPrefix.Length=0;
        }
    }
}
