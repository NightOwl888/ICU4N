using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Finds the set of characters and strings that sort differently in the tailoring
    /// from the base data.
    ///
    /// <para/>Every mapping in the tailoring needs to be compared to the base,
    /// because some mappings are copied for optimization, and
    /// all contractions for a character are copied if any contractions for that character
    /// are added, modified or removed.
    ///
    /// <para/>It might be simpler to re-parse the rule string, but:
    /// <list type="bullet">
    ///     <item><description>That would require duplicating some of the from-rules builder code.</description></item>
    ///     <item><description>That would make the runtime code depend on the builder.</description></item>
    ///     <item><description>That would only work if we have the rule string, and we allow users to
    ///                        omit the rule string from data files.</description></item>
    /// </list>
    /// </summary>
    public sealed class TailoredSet
    {
        private const int CharStackBufferSize = 32;

        private CollationData data;
        private CollationData baseData;
        private UnicodeSet tailored;
        private OpenStringBuilder unreversedPrefix = new OpenStringBuilder();
        private ReadOnlyMemory<char> suffix;

        public TailoredSet(UnicodeSet t)
        {
            tailored = t;
        }

        public void ForData(CollationData d)
        {
            data = d;
            baseData = d.Base;
            Debug.Assert(baseData != null);
            // utrie2_enum(data->trie, NULL, enumTailoredRange, this);
            using (IEnumerator<Trie2Range> trieIterator = data.trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumTailoredRange(range.StartCodePoint, range.EndCodePoint, range.Value, this);
                }
            }
        }

        private void EnumTailoredRange(int start, int end, int ce32, TailoredSet ts)
        {
            if (ce32 == Collation.FALLBACK_CE32)
            {
                return; // fallback to base, not tailored
            }
            ts.HandleCE32(start, end, ce32);
        }

        // Java porting note: ICU4C returns U_SUCCESS(error) and it's not applicable to ICU4J.
        //  Also, ICU4C requires handleCE32() to be public because it is used by the callback
        //  function (enumTailoredRange()). This is not necessary for Java implementation.
        private void HandleCE32(int start, int end, int ce32)
        {
            Debug.Assert(ce32 != Collation.FALLBACK_CE32);
            if (Collation.IsSpecialCE32(ce32))
            {
                ce32 = data.GetIndirectCE32(ce32);
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    return;
                }
            }
            do
            {
                int baseCE32 = baseData.GetFinalCE32(baseData.GetCE32(start));
                // Do not just continue if ce32 == baseCE32 because
                // contractions and expansions in different data objects
                // normally differ even if they have the same data offsets.
                if (Collation.IsSelfContainedCE32(ce32) && Collation.IsSelfContainedCE32(baseCE32))
                {
                    // fastpath
                    if (ce32 != baseCE32)
                    {
                        tailored.Add(start);
                    }
                }
                else
                {
                    Compare(start, ce32, baseCE32);
                }
            } while (++start <= end);
        }

        private void Compare(int c, int ce32, int baseCE32)
        {
            if (Collation.IsPrefixCE32(ce32))
            {
                int dataIndex = Collation.IndexFromCE32(ce32);
                ce32 = data.GetFinalCE32(data.GetCE32FromContexts(dataIndex));
                if (Collation.IsPrefixCE32(baseCE32))
                {
                    int baseIndex = Collation.IndexFromCE32(baseCE32);
                    baseCE32 = baseData.GetFinalCE32(baseData.GetCE32FromContexts(baseIndex));
                    ComparePrefixes(c, data.contexts.AsMemory(), dataIndex + 2, baseData.contexts.AsMemory(), baseIndex + 2);
                }
                else
                {
                    AddPrefixes(data, c, data.contexts.AsMemory(), dataIndex + 2);
                }
            }
            else if (Collation.IsPrefixCE32(baseCE32))
            {
                int baseIndex = Collation.IndexFromCE32(baseCE32);
                baseCE32 = baseData.GetFinalCE32(baseData.GetCE32FromContexts(baseIndex));
                AddPrefixes(baseData, c, baseData.contexts.AsMemory(), baseIndex + 2);
            }

            if (Collation.IsContractionCE32(ce32))
            {
                int dataIndex = Collation.IndexFromCE32(ce32);
                if ((ce32 & Collation.CONTRACT_SINGLE_CP_NO_MATCH) != 0)
                {
                    ce32 = Collation.NO_CE32;
                }
                else
                {
                    ce32 = data.GetFinalCE32(data.GetCE32FromContexts(dataIndex));
                }
                if (Collation.IsContractionCE32(baseCE32))
                {
                    int baseIndex = Collation.IndexFromCE32(baseCE32);
                    if ((baseCE32 & Collation.CONTRACT_SINGLE_CP_NO_MATCH) != 0)
                    {
                        baseCE32 = Collation.NO_CE32;
                    }
                    else
                    {
                        baseCE32 = baseData.GetFinalCE32(baseData.GetCE32FromContexts(baseIndex));
                    }
                    CompareContractions(c, data.contexts.AsMemory(), dataIndex + 2, baseData.contexts.AsMemory(), baseIndex + 2);
                }
                else
                {
                    AddContractions(c, data.contexts.AsMemory(), dataIndex + 2);
                }
            }
            else if (Collation.IsContractionCE32(baseCE32))
            {
                int baseIndex = Collation.IndexFromCE32(baseCE32);
                baseCE32 = baseData.GetFinalCE32(baseData.GetCE32FromContexts(baseIndex));
                AddContractions(c, baseData.contexts.AsMemory(), baseIndex + 2);
            }

            int tag;
            if (Collation.IsSpecialCE32(ce32))
            {
                tag = Collation.TagFromCE32(ce32);
                Debug.Assert(tag != Collation.PREFIX_TAG);
                Debug.Assert(tag != Collation.CONTRACTION_TAG);
                // Currently, the tailoring data builder does not write offset tags.
                // They might be useful for saving space,
                // but they would complicate the builder,
                // and in tailorings we assume that performance of tailored characters is more important.
                Debug.Assert(tag != Collation.OFFSET_TAG);
            }
            else
            {
                tag = -1;
            }
            int baseTag;
            if (Collation.IsSpecialCE32(baseCE32))
            {
                baseTag = Collation.TagFromCE32(baseCE32);
                Debug.Assert(baseTag != Collation.PREFIX_TAG);
                Debug.Assert(baseTag != Collation.CONTRACTION_TAG);
            }
            else
            {
                baseTag = -1;
            }

            // Non-contextual mappings, expansions, etc.
            if (baseTag == Collation.OFFSET_TAG)
            {
                // We might be comparing a tailoring CE which is a copy of
                // a base offset-tag CE, via the [optimize [set]] syntax
                // or when a single-character mapping was copied for tailored contractions.
                // Offset tags always result in long-primary CEs,
                // with common secondary/tertiary weights.
                if (!Collation.IsLongPrimaryCE32(ce32))
                {
                    Add(c);
                    return;
                }
                long dataCE = baseData.ces[Collation.IndexFromCE32(baseCE32)];
                long p = Collation.GetThreeBytePrimaryForOffsetData(c, dataCE);
                if (Collation.PrimaryFromLongPrimaryCE32(ce32) != p)
                {
                    Add(c);
                    return;
                }
            }

            if (tag != baseTag)
            {
                Add(c);
                return;
            }

            if (tag == Collation.EXPANSION32_TAG)
            {
                int length = Collation.LengthFromCE32(ce32);
                int baseLength = Collation.LengthFromCE32(baseCE32);

                if (length != baseLength)
                {
                    Add(c);
                    return;
                }

                int idx0 = Collation.IndexFromCE32(ce32);
                int idx1 = Collation.IndexFromCE32(baseCE32);

                for (int i = 0; i < length; ++i)
                {
                    if (data.ce32s[idx0 + i] != baseData.ce32s[idx1 + i])
                    {
                        Add(c);
                        break;
                    }
                }
            }
            else if (tag == Collation.EXPANSION_TAG)
            {
                int length = Collation.LengthFromCE32(ce32);
                int baseLength = Collation.LengthFromCE32(baseCE32);

                if (length != baseLength)
                {
                    Add(c);
                    return;
                }

                int idx0 = Collation.IndexFromCE32(ce32);
                int idx1 = Collation.IndexFromCE32(baseCE32);

                for (int i = 0; i < length; ++i)
                {
                    if (data.ces[idx0 + i] != baseData.ces[idx1 + i])
                    {
                        Add(c);
                        break;
                    }
                }
            }
            else if (tag == Collation.HANGUL_TAG)
            {
                StringBuilder jamos = new StringBuilder();
                int length = jamos.AppendHangulDecomposition(c); // ICU4N: Renamed from Hangul.Decompose()
                if (tailored.Contains(jamos[0]) || tailored.Contains(jamos[1])
                        || (length == 3 && tailored.Contains(jamos[2])))
                {
                    Add(c);
                }
            }
            else if (ce32 != baseCE32)
            {
                Add(c);
            }
        }

        private void ComparePrefixes(int c, ReadOnlyMemory<char> p, int pidx, ReadOnlyMemory<char> q, int qidx)
        {
            // Parallel iteration over prefixes of both tables.
            using (CharsTrieEnumerator prefixes = new CharsTrie(p, pidx).GetEnumerator())
            using (CharsTrieEnumerator basePrefixes = new CharsTrie(q, qidx).GetEnumerator())
            {
                ReadOnlyMemory<char> tp = default; // Tailoring prefix.
                ReadOnlyMemory<char> bp = default; // Base prefix.
                                                   // Use a string with a U+FFFF as the limit sentinel.
                                                   // U+FFFF is untailorable and will not occur in prefixes.
                ReadOnlyMemory<char> none = "\uffff".AsMemory();
                CharsTrieEntry te = null, be = null;
                for (; ; )
                {
                    if (tp.IsEmpty)
                    {
                        if (prefixes.MoveNext())
                        {
                            te = prefixes.Current;
                            tp = te.Chars;
                        }
                        else
                        {
                            te = null;
                            tp = none;
                        }
                    }
                    if (bp.IsEmpty)
                    {
                        if (basePrefixes.MoveNext())
                        {
                            be = basePrefixes.Current;
                            bp = be.Chars;
                        }
                        else
                        {
                            be = null;
                            bp = none;
                        }
                    }
                    ReadOnlySpan<char> tpSpan = tp.Span, bpSpan = bp.Span, noneSpan = none.Span;
                    if (tpSpan.Equals(noneSpan, StringComparison.Ordinal) && bpSpan.Equals(noneSpan, StringComparison.Ordinal))
                    {
                        break;
                    }
                    int cmp = tpSpan.CompareTo(bpSpan, StringComparison.Ordinal);
                    if (cmp < 0)
                    {
                        // tp occurs in the tailoring but not in the base.
                        Debug.Assert(te != null);
                        AddPrefix(data, tpSpan, c, te.Value);
                        te = null;
                        tp = null;
                    }
                    else if (cmp > 0)
                    {
                        // bp occurs in the base but not in the tailoring.
                        Debug.Assert(be != null);
                        AddPrefix(baseData, bpSpan, c, be.Value);
                        be = null;
                        bp = null;
                    }
                    else
                    {
                        SetPrefix(tpSpan);
                        Debug.Assert(te != null && be != null);
                        Compare(c, te.Value, be.Value);
                        ResetPrefix();
                        te = be = null;
                        tp = bp = null;
                    }
                }
            }
        }

        private void CompareContractions(int c, ReadOnlyMemory<char> p, int pidx, ReadOnlyMemory<char> q, int qidx)
        {
            // Parallel iteration over suffixes of both tables.
            using (CharsTrieEnumerator suffixes = new CharsTrie(p, pidx).GetEnumerator())
            using (CharsTrieEnumerator baseSuffixes = new CharsTrie(q, qidx).GetEnumerator())
            {
                ReadOnlyMemory<char> ts = default; // Tailoring suffix.
                ReadOnlyMemory<char> bs = default; // Base suffix.
                                  // Use a string with two U+FFFF as the limit sentinel.
                                  // U+FFFF is untailorable and will not occur in contractions except maybe
                                  // as a single suffix character for a root-collator boundary contraction.
                ReadOnlyMemory<char> none = "\uffff\uffff".AsMemory();
                CharsTrieEntry te = null, be = null;
                for (; ; )
                {
                    if (ts.IsEmpty)
                    {
                        if (suffixes.MoveNext())
                        {
                            te = suffixes.Current;
                            ts = te.Chars;
                        }
                        else
                        {
                            te = null;
                            ts = none;
                        }
                    }
                    if (bs.IsEmpty)
                    {
                        if (baseSuffixes.MoveNext())
                        {
                            be = baseSuffixes.Current;
                            bs = be.Chars;
                        }
                        else
                        {
                            be = null;
                            bs = none;
                        }
                    }
                    ReadOnlySpan<char> tsSpan = ts.Span, bsSpan = bs.Span, noneSpan = none.Span;
                    if (tsSpan.Equals(noneSpan, StringComparison.Ordinal) && bsSpan.Equals(noneSpan, StringComparison.Ordinal))
                    {
                        break;
                    }
                    int cmp = tsSpan.CompareTo(bsSpan, StringComparison.Ordinal);
                    if (cmp < 0)
                    {
                        // ts occurs in the tailoring but not in the base.
                        AddSuffix(c, tsSpan);
                        te = null;
                        ts = null;
                    }
                    else if (cmp > 0)
                    {
                        // bs occurs in the base but not in the tailoring.
                        AddSuffix(c, bsSpan);
                        be = null;
                        bs = null;
                    }
                    else
                    {
                        suffix = ts;
                        Compare(c, te.Value, be.Value);
                        suffix = null;
                        te = be = null;
                        ts = bs = null;
                    }
                }
            }
        }

        private void AddPrefixes(CollationData d, int c, ReadOnlyMemory<char> p, int pidx)
        {
            using (CharsTrieEnumerator prefixes = new CharsTrie(p, pidx).GetEnumerator())
            {
                while (prefixes.MoveNext())
                {
                    var e = prefixes.Current;
                    AddPrefix(d, e.Chars.Span, c, e.Value);
                }
            }
        }

        private void AddPrefix(CollationData d, ReadOnlySpan<char> pfx, int c, int ce32)
        {
            SetPrefix(pfx);
            ce32 = d.GetFinalCE32(ce32);
            if (Collation.IsContractionCE32(ce32))
            {
                int idx = Collation.IndexFromCE32(ce32);
                AddContractions(c, d.contexts.AsMemory(), idx + 2);
            }
            tailored.Add(unreversedPrefix.AppendCodePoint(c).ToString());
            ResetPrefix();
        }

        private void AddContractions(int c, ReadOnlyMemory<char> p, int pidx)
        {
            using (CharsTrieEnumerator suffixes = new CharsTrie(p, pidx).GetEnumerator())
            {
                while (suffixes.MoveNext())
                {
                    var e = suffixes.Current;
                    AddSuffix(c, e.Chars.Span);
                }
            }
        }

        private void AddSuffix(int c, ReadOnlySpan<char> sfx)
        {
            int length = unreversedPrefix.Length + 2 + sfx.Length;
            using var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[length])
                : new ValueStringBuilder(length);
            sb.Append(unreversedPrefix.AsSpan());
            sb.AppendCodePoint(c);
            sb.Append(sfx);
            tailored.Add(sb.AsSpan());
        }

        private void Add(int c)
        {
            if (unreversedPrefix.Length == 0 && suffix.IsEmpty)
            {
                tailored.Add(c);
            }
            else
            {
                int length = unreversedPrefix.Length + 2 + suffix.Length;
                using var s = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[length])
                    : new ValueStringBuilder(length);
                s.Append(unreversedPrefix.AsSpan());
                s.AppendCodePoint(c);
                if (!suffix.IsEmpty)
                {
                    s.Append(suffix.Span);
                }
                tailored.Add(s.AsSpan());
            }
        }

        // Prefixes are reversed in the data structure.
        private void SetPrefix(ReadOnlySpan<char> pfx)
        {
            unreversedPrefix.Length = 0;
            unreversedPrefix.Append(pfx).Reverse();
        }

        private void ResetPrefix()
        {
            unreversedPrefix.Length = 0;
        }
    }
}
