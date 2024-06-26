using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Incrementally checks the input text for FCD and normalizes where necessary.
    /// </summary>
    public sealed class FCDUTF16CollationIterator : UTF16CollationIterator
    {
        /// <summary>
        /// Partial constructor, see <see cref="CollationIterator.CollationIterator(CollationData)"/>
        /// </summary>
        public FCDUTF16CollationIterator(CollationData d)
            : base(d)
        {
            nfcImpl = d.NfcImpl;
        }

        // ICU4N: The value for s must have a reference to it that has a lifetime longer than this class.
        public FCDUTF16CollationIterator(CollationData data, bool numeric, ReadOnlyMemory<char> s, int p)
            : base(data, numeric, s, p)
        {
            rawSeq = s;
            segmentStart = p;
            rawLimit = s.Length;
            nfcImpl = data.NfcImpl;
            checkDir = 1;
        }

        public override bool Equals(object other)
        {
            // Skip the UTF16CollationIterator and call its parent.
            if (!(other is CollationIterator otherCi)
                || !otherCi.Equals(other)
                || !(other is FCDUTF16CollationIterator o))
            {
                return false;
            }
            // Compare the iterator state but not the text: Assume that the caller does that.
            if (checkDir != o.checkDir)
            {
                return false;
            }
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            ReadOnlySpan<char> oSeqSpan = seq.Span;
            ReadOnlySpan<char> oRawSeqSpan = rawSeq.Span;
            if (checkDir == 0 && (seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal)) != (oSeqSpan.Equals(oRawSeqSpan, StringComparison.Ordinal)))
            {
                return false;
            }
            if (checkDir != 0 || seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal))
            {
                return (pos - rawStart) == (o.pos - /*o.*/ rawStart);
            }
            else
            {
                return (segmentStart - rawStart) == (o.segmentStart - /*o.*/ rawStart) &&
                        (pos - start) == (o.pos - o.start);
            }
        }

        public override int GetHashCode()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            // ICU4N specific - implemented hash code
            int hash = 17;
            unchecked // Overflow is fine, just wrap
            {
                hash = hash * 23 + checkDir.GetHashCode();
                hash = hash * 23 + ((checkDir == 0) ? StringHelper.GetHashCode(seqSpan) ^ StringHelper.GetHashCode(rawSeqSpan) : 0);
                hash = hash * 23 + ((checkDir != 0 || seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal)) ? (pos - rawStart).GetHashCode() : (segmentStart - rawStart).GetHashCode() ^ (pos - start).GetHashCode());
            }
            return hash;
        }
        public override void ResetToOffset(int newOffset)
        {
            Reset();
            seq = rawSeq;
            start = segmentStart = pos = rawStart + newOffset;
            limit = rawLimit;
            checkDir = 1;
        }

        public override int Offset
        {
            get
            {
                if (checkDir != 0 || seq.Span.Equals(rawSeq.Span, StringComparison.Ordinal))
                {
                    return pos - rawStart;
                }
                else if (pos == start)
                {
                    return segmentStart - rawStart;
                }
                else
                {
                    return segmentLimit - rawStart;
                }
            }
        }

        // ICU4N: The value for s must have a reference to it that has a lifetime longer than this class.
        public override void SetText(bool numeric, ReadOnlyMemory<char> s, int p)
        {
            base.SetText(numeric, s, p);
            rawSeq = s;
            segmentStart = p;
            rawLimit = limit = s.Length;
            checkDir = 1;
        }

        public override int NextCodePoint()
        {
            char c;
            for (; ; )
            {
                if (checkDir > 0)
                {
                    if (pos == limit)
                    {
                        return Collation.SentinelCodePoint;
                    }
                    c = seq.Span[pos++];
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != limit && CollationFCD.HasLccc(seq.Span[pos])))
                        {
                            --pos;
                            NextSegment();
                            c = seq.Span[pos++];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != limit)
                {
                    c = seq.Span[pos++];
                    break;
                }
                else
                {
                    SwitchToForward();
                }
            }
            char trail;
            if (char.IsHighSurrogate(c) && pos != limit &&
                    char.IsLowSurrogate(trail = seq.Span[pos]))
            {
                ++pos;
                return Character.ToCodePoint(c, trail);
            }
            else
            {
                return c;
            }
        }

        public override int PreviousCodePoint()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            char c;
            for (; ; )
            {
                if (checkDir < 0)
                {
                    if (pos == start)
                    {
                        return Collation.SentinelCodePoint;
                    }
                    c = seqSpan[--pos];
                    if (CollationFCD.HasLccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != start && CollationFCD.HasTccc(seqSpan[pos - 1])))
                        {
                            ++pos;
                            PreviousSegment();
                            seqSpan = seq.Span;
                            c = seqSpan[--pos];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != start)
                {
                    c = seqSpan[--pos];
                    break;
                }
                else
                {
                    SwitchToBackward();
                    seqSpan = seq.Span;
                }
            }
            char lead;
            if (char.IsLowSurrogate(c) && pos != start &&
                    char.IsHighSurrogate(lead = seqSpan[pos - 1]))
            {
                --pos;
                return Character.ToCodePoint(lead, c);
            }
            else
            {
                return c;
            }
        }

        protected override long HandleNextCE32()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            char c;
            for (; ; )
            {
                if (checkDir > 0)
                {
                    if (pos == limit)
                    {
                        return NoCodePointAndCE32;
                    }
                    c = seqSpan[pos++];
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != limit && CollationFCD.HasLccc(seqSpan[pos])))
                        {
                            --pos;
                            NextSegment();
                            seqSpan = seq.Span;
                            c = seqSpan[pos++];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != limit)
                {
                    c = seqSpan[pos++];
                    break;
                }
                else
                {
                    SwitchToForward();
                    seqSpan = seq.Span;
                }
            }
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead(c));
        }

        /* bool foundNULTerminator(); */

        protected override void ForwardNumCodePoints(int num)
        {
            // Specify the class to avoid a virtual-function indirection.
            // In Java, we would declare this class final.
            while (num > 0 && NextCodePoint() >= 0)
            {
                --num;
            }
        }

        protected override void BackwardNumCodePoints(int num)
        {
            // Specify the class to avoid a virtual-function indirection.
            // In Java, we would declare this class final.
            while (num > 0 && PreviousCodePoint() >= 0)
            {
                --num;
            }
        }

        /// <summary>
        /// Switches to forward checking if possible.
        /// To be called when checkDir &lt; 0 || (checkDir == 0 &amp;&amp; pos == limit).
        /// Returns with checkDir &gt; 0 || (checkDir == 0 &amp;&amp; pos != limit).
        /// </summary>
        private void SwitchToForward()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            Debug.Assert((checkDir < 0 && seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal)) || (checkDir == 0 && pos == limit));
            if (checkDir < 0)
            {
                // Turn around from backward checking.
                start = segmentStart = pos;
                if (pos == segmentLimit)
                {
                    limit = rawLimit;
                    checkDir = 1;  // Check forward.
                }
                else
                {  // pos < segmentLimit
                    checkDir = 0;  // Stay in FCD segment.
                }
            }
            else
            {
                // Reached the end of the FCD segment.
                if (seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal))
                {
                    // The input text segment is FCD, extend it forward.
                }
                else
                {
                    // The input text segment needed to be normalized.
                    // Switch to checking forward from it.
                    seq = rawSeq;
                    pos = start = segmentStart = segmentLimit;
                    // Note: If this segment is at the end of the input text,
                    // then it might help to return false to indicate that, so that
                    // we do not have to re-check and normalize when we turn around and go backwards.
                    // However, that would complicate the call sites for an optimization of an unusual case.
                }
                limit = rawLimit;
                checkDir = 1;
            }
        }

        /// <summary>
        /// Extend the FCD text segment forward or normalize around pos.
        /// To be called when checkDir > 0 &amp;&amp; pos != limit.
        /// Returns with checkDir == 0 and pos != limit.
        /// </summary>
        private void NextSegment()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            Debug.Assert(checkDir > 0 && seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal) && pos != limit);
            // The input text [segmentStart..pos[ passes the FCD check.
            int p = pos;
            int prevCC = 0;
            for (; ; )
            {
                // Fetch the next character's fcd16 value.
                int q = p;
                int c = Character.CodePointAt(seqSpan, p);
                p += Character.CharCount(c);
                int fcd16 = nfcImpl.GetFCD16(c);
                int leadCC = fcd16 >> 8;
                if (leadCC == 0 && q != pos)
                {
                    // FCD boundary before the [q, p[ character.
                    limit = segmentLimit = q;
                    break;
                }
                if (leadCC != 0 && (prevCC > leadCC || CollationFCD.IsFCD16OfTibetanCompositeVowel(fcd16)))
                {
                    // Fails FCD check. Find the next FCD boundary and normalize.
                    do
                    {
                        q = p;
                        if (p == rawLimit) { break; }
                        c = Character.CodePointAt(seqSpan, p);
                        p += Character.CharCount(c);
                    } while (nfcImpl.GetFCD16(c) > 0xff);
                    Normalize(pos, q);
                    seqSpan = seq.Span;
                    rawSeqSpan = rawSeq.Span;
                    pos = start;
                    break;
                }
                prevCC = fcd16 & 0xff;
                if (p == rawLimit || prevCC == 0)
                {
                    // FCD boundary after the last character.
                    limit = segmentLimit = p;
                    break;
                }
            }
            Debug.Assert(pos != limit);
            checkDir = 0;
        }

        /// <summary>
        /// Switches to backward checking.
        /// To be called when checkDir &gt; 0 || (checkDir == 0 &amp;&amp; pos == start).
        /// Returns with checkDir &lt; 0 || (checkDir == 0 &amp;&amp; pos != start).
        /// </summary>
        private void SwitchToBackward()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            Debug.Assert((checkDir > 0 && seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal)) || (checkDir == 0 && pos == start));
            if (checkDir > 0)
            {
                // Turn around from forward checking.
                limit = segmentLimit = pos;
                if (pos == segmentStart)
                {
                    start = rawStart;
                    checkDir = -1;  // Check backward.
                }
                else
                {  // pos > segmentStart
                    checkDir = 0;  // Stay in FCD segment.
                }
            }
            else
            {
                // Reached the start of the FCD segment.
                if (seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal))
                {
                    // The input text segment is FCD, extend it backward.
                }
                else
                {
                    // The input text segment needed to be normalized.
                    // Switch to checking backward from it.
                    seq = rawSeq;
                    pos = limit = segmentLimit = segmentStart;
                }
                start = rawStart;
                checkDir = -1;
            }
        }

        /// <summary>
        /// Extend the FCD text segment backward or normalize around pos.
        /// To be called when checkDir &lt; 0 &amp;&amp; pos != start.
        /// Returns with checkDir == 0 and pos != start.
        /// </summary>
        private void PreviousSegment()
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            ReadOnlySpan<char> rawSeqSpan = rawSeq.Span;
            Debug.Assert(checkDir < 0 && seqSpan.Equals(rawSeqSpan, StringComparison.Ordinal) && pos != start);
            // The input text [pos..segmentLimit[ passes the FCD check.
            int p = pos;
            int nextCC = 0;
            for (; ; )
            {
                // Fetch the previous character's fcd16 value.
                int q = p;
                int c = Character.CodePointBefore(seqSpan, p);
                p -= Character.CharCount(c);
                int fcd16 = nfcImpl.GetFCD16(c);
                int trailCC = fcd16 & 0xff;
                if (trailCC == 0 && q != pos)
                {
                    // FCD boundary after the [p, q[ character.
                    start = segmentStart = q;
                    break;
                }
                if (trailCC != 0 && ((nextCC != 0 && trailCC > nextCC) ||
                                    CollationFCD.IsFCD16OfTibetanCompositeVowel(fcd16)))
                {
                    // Fails FCD check. Find the previous FCD boundary and normalize.
                    do
                    {
                        q = p;
                        if (fcd16 <= 0xff || p == rawStart) { break; }
                        c = Character.CodePointBefore(seqSpan, p);
                        p -= Character.CharCount(c);
                    } while ((fcd16 = nfcImpl.GetFCD16(c)) != 0);
                    Normalize(q, pos);
                    seqSpan = seq.Span;
                    rawSeqSpan = rawSeq.Span;
                    pos = limit;
                    break;
                }
                nextCC = fcd16 >> 8;
                if (p == rawStart || nextCC == 0)
                {
                    // FCD boundary before the following character.
                    start = segmentStart = p;
                    break;
                }
            }
            Debug.Assert(pos != start);
            checkDir = 0;
        }

        private const int CharStackBufferSize = 64;
        private void Normalize(int from, int to)
        {
            if (normalized == null)
            {
                normalized = new OpenStringBuilder();
            }
            normalized.Length = 0;
            int estimatedLength = to - from;
            ValueStringBuilder sb = estimatedLength <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(estimatedLength);
            try
            {
                // NFD without argument checking.
                nfcImpl.Decompose(rawSeq.Span.Slice(from, to - from), ref sb, to - from); // ICU4N: Corrected 3rd parameter
                normalized.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
            // Switch collation processing into the FCD buffer
            // with the result of normalizing [segmentStart, segmentLimit[.
            segmentStart = from;
            segmentLimit = to;
            seq = normalized.AsMemory();
            start = 0;
            limit = start + normalized.Length;
        }

        // Text pointers: The input text is rawSeq[rawStart, rawLimit[.
        // (In C++, these are const UChar * pointers.
        // In Java, we use CharSequence rawSeq and the parent class' seq
        // together with int indexes.)
        //
        // checkDir > 0:
        //
        // The input text rawSeq[segmentStart..pos[ passes the FCD check.
        // Moving forward checks incrementally.
        // segmentLimit is undefined. seq == rawSeq. limit == rawLimit.
        //
        // checkDir < 0:
        // The input text rawSeq[pos..segmentLimit[ passes the FCD check.
        // Moving backward checks incrementally.
        // segmentStart is undefined. seq == rawSeq. start == rawStart.
        //
        // checkDir == 0:
        //
        // The input text rawSeq[segmentStart..segmentLimit[ is being processed.
        // These pointers are at FCD boundaries.
        // Either this text segment already passes the FCD check
        // and seq==rawSeq && segmentStart==start<=pos<=limit==segmentLimit,
        // or the current segment had to be normalized so that
        // rawSeq[segmentStart..segmentLimit[ turned into the normalized string,
        // corresponding to seq==normalized && 0==start<=pos<=limit==start+normalized.length().
        private ReadOnlyMemory<char> rawSeq; // ICU4N: The base class keeps this alive for us
        private const int rawStart = 0;
        private int segmentStart;
        private int segmentLimit;
        private int rawLimit;

        private readonly Normalizer2Impl nfcImpl;
        private OpenStringBuilder normalized;
        // Direction of incremental FCD check. See comments before rawStart.
        private int checkDir;
    }
}
