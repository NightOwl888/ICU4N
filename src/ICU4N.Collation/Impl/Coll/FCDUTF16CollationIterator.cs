using ICU4N.Support.Text;
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
            nfcImpl = d.nfcImpl;
        }

        public FCDUTF16CollationIterator(CollationData data, bool numeric, ICharSequence s, int p)
            : base(data, numeric, s, p)
        {
            rawSeq = s;
            segmentStart = p;
            rawLimit = s.Length;
            nfcImpl = data.nfcImpl;
            checkDir = 1;
        }

        public override bool Equals(object other)
        {
            // Skip the UTF16CollationIterator and call its parent.
            if (!(other is CollationIterator)
            || !((CollationIterator)this).Equals(other)
            || !(other is FCDUTF16CollationIterator))
            {
                return false;
            }
            FCDUTF16CollationIterator o = (FCDUTF16CollationIterator)other;
            // Compare the iterator state but not the text: Assume that the caller does that.
            if (checkDir != o.checkDir)
            {
                return false;
            }
            if (checkDir == 0 && (seq == rawSeq) != (o.seq == o.rawSeq))
            {
                return false;
            }
            if (checkDir != 0 || seq == rawSeq)
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
            Debug.Assert(false, "hashCode not designed");
            return 42; // any arbitrary constant will do
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
                if (checkDir != 0 || seq == rawSeq)
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

        public override void SetText(bool numeric, ICharSequence s, int p)
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
                    c = seq[pos++];
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != limit && CollationFCD.HasLccc(seq[pos])))
                        {
                            --pos;
                            NextSegment();
                            c = seq[pos++];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != limit)
                {
                    c = seq[pos++];
                    break;
                }
                else
                {
                    SwitchToForward();
                }
            }
            char trail;
            if (char.IsHighSurrogate(c) && pos != limit &&
                    char.IsLowSurrogate(trail = seq[pos]))
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
            char c;
            for (; ; )
            {
                if (checkDir < 0)
                {
                    if (pos == start)
                    {
                        return Collation.SentinelCodePoint;
                    }
                    c = seq[--pos];
                    if (CollationFCD.HasLccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != start && CollationFCD.HasTccc(seq[pos - 1])))
                        {
                            ++pos;
                            PreviousSegment();
                            c = seq[--pos];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != start)
                {
                    c = seq[--pos];
                    break;
                }
                else
                {
                    SwitchToBackward();
                }
            }
            char lead;
            if (char.IsLowSurrogate(c) && pos != start &&
                    char.IsHighSurrogate(lead = seq[pos - 1]))
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
            char c;
            for (; ; )
            {
                if (checkDir > 0)
                {
                    if (pos == limit)
                    {
                        return NO_CP_AND_CE32;
                    }
                    c = seq[pos++];
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                (pos != limit && CollationFCD.HasLccc(seq[pos])))
                        {
                            --pos;
                            NextSegment();
                            c = seq[pos++];
                        }
                    }
                    break;
                }
                else if (checkDir == 0 && pos != limit)
                {
                    c = seq[pos++];
                    break;
                }
                else
                {
                    SwitchToForward();
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
            Debug.Assert((checkDir < 0 && seq == rawSeq) || (checkDir == 0 && pos == limit));
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
                if (seq == rawSeq)
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
            Debug.Assert(checkDir > 0 && seq == rawSeq && pos != limit);
            // The input text [segmentStart..pos[ passes the FCD check.
            int p = pos;
            int prevCC = 0;
            for (; ; )
            {
                // Fetch the next character's fcd16 value.
                int q = p;
                int c = Character.CodePointAt(seq, p);
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
                        c = Character.CodePointAt(seq, p);
                        p += Character.CharCount(c);
                    } while (nfcImpl.GetFCD16(c) > 0xff);
                    Normalize(pos, q);
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
            Debug.Assert((checkDir > 0 && seq == rawSeq) || (checkDir == 0 && pos == start));
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
                if (seq == rawSeq)
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
            Debug.Assert(checkDir < 0 && seq == rawSeq && pos != start);
            // The input text [pos..segmentLimit[ passes the FCD check.
            int p = pos;
            int nextCC = 0;
            for (; ; )
            {
                // Fetch the previous character's fcd16 value.
                int q = p;
                int c = Character.CodePointBefore(seq, p);
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
                        c = Character.CodePointBefore(seq, p);
                        p -= Character.CharCount(c);
                    } while ((fcd16 = nfcImpl.GetFCD16(c)) != 0);
                    Normalize(q, pos);
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

        private void Normalize(int from, int to)
        {
            if (normalized == null)
            {
                normalized = new StringBuilder();
            }
            // NFD without argument checking.
            nfcImpl.Decompose(rawSeq, from, to, normalized, to - from);
            // Switch collation processing into the FCD buffer
            // with the result of normalizing [segmentStart, segmentLimit[.
            segmentStart = from;
            segmentLimit = to;
            seq = normalized.ToCharSequence();
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
        private ICharSequence rawSeq;
        private static readonly int rawStart = 0;
        private int segmentStart;
        private int segmentLimit;
        private int rawLimit;

        private readonly Normalizer2Impl nfcImpl;
        private StringBuilder normalized;
        // Direction of incremental FCD check. See comments before rawStart.
        private int checkDir;
    }
}
