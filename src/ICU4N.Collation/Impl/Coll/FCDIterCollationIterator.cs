using ICU4N.Support.Text;
using ICU4N.Text;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Incrementally checks the input text for FCD and normalizes where necessary.
    /// </summary>
    public sealed class FCDIterCollationIterator : IterCollationIterator
    {
        public FCDIterCollationIterator(CollationData data, bool numeric,
            UCharacterIterator ui, int startIndex)
            : base(data, numeric, ui)
        {
            state = State.IterCheckFwd;
            start = startIndex;
            nfcImpl = data.nfcImpl;
        }

        public override void ResetToOffset(int newOffset)
        {
            base.ResetToOffset(newOffset);
            start = newOffset;
            state = State.IterCheckFwd;
        }

        public override int Offset
        {
            get
            {
                if (state.CompareTo(State.IterCheckBwd) <= 0)
                {
                    return iter.Index;
                }
                else if (state == State.IterInFCDSegment)
                {
                    return pos;
                }
                else if (pos == 0)
                {
                    return start;
                }
                else
                {
                    return limit;
                }
            }
        }

        public override int NextCodePoint()
        {
            int c;
            for (; ; )
            {
                if (state == State.IterCheckFwd)
                {
                    c = iter.Next();
                    if (c < 0)
                    {
                        return c;
                    }
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                CollationFCD.HasLccc(iter.Current))
                        {
                            iter.Previous();
                            if (!NextSegment())
                            {
                                return Collation.SentinelCodePoint;
                            }
                            continue;
                        }
                    }
                    if (IsLeadSurrogate(c))
                    {
                        int trail = iter.Next();
                        if (IsTrailSurrogate(trail))
                        {
                            return Character.ToCodePoint((char)c, (char)trail);
                        }
                        else if (trail >= 0)
                        {
                            iter.Previous();
                        }
                    }
                    return c;
                }
                else if (state == State.IterInFCDSegment && pos != limit)
                {
                    c = iter.NextCodePoint();
                    pos += Character.CharCount(c);
                    Debug.Assert(c >= 0);
                    return c;
                }
                else if (state.CompareTo(State.InNormIterAtLimit) >= 0 &&
                      pos != normalized.Length)
                {
                    c = normalized.CodePointAt(pos);
                    pos += Character.CharCount(c);
                    return c;
                }
                else
                {
                    SwitchToForward();
                }
            }
        }

        public override int PreviousCodePoint()
        {
            int c;
            for (; ; )
            {
                if (state == State.IterCheckBwd)
                {
                    c = iter.Previous();
                    if (c < 0)
                    {
                        start = pos = 0;
                        state = State.IterInFCDSegment;
                        return Collation.SentinelCodePoint;
                    }
                    if (CollationFCD.HasLccc(c))
                    {
                        int prev = Collation.SentinelCodePoint;
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                CollationFCD.HasTccc(prev = iter.Previous()))
                        {
                            iter.Next();
                            if (prev >= 0)
                            {
                                iter.Next();
                            }
                            if (!PreviousSegment())
                            {
                                return Collation.SentinelCodePoint;
                            }
                            continue;
                        }
                        // hasLccc(trail)=true for all trail surrogates
                        if (IsTrailSurrogate(c))
                        {
                            if (prev < 0)
                            {
                                prev = iter.Previous();
                            }
                            if (IsLeadSurrogate(prev))
                            {
                                return Character.ToCodePoint((char)prev, (char)c);
                            }
                        }
                        if (prev >= 0)
                        {
                            iter.Next();
                        }
                    }
                    return c;
                }
                else if (state == State.IterInFCDSegment && pos != start)
                {
                    c = iter.PreviousCodePoint();
                    pos -= Character.CharCount(c);
                    Debug.Assert(c >= 0);
                    return c;
                }
                else if (state.CompareTo(State.InNormIterAtLimit) >= 0 && pos != 0)
                {
                    c = normalized.CodePointBefore(pos);
                    pos -= Character.CharCount(c);
                    return c;
                }
                else
                {
                    SwitchToBackward();
                }
            }
        }

        protected override long HandleNextCE32()
        {
            int c;
            for (; ; )
            {
                if (state == State.IterCheckFwd)
                {
                    c = iter.Next();
                    if (c < 0)
                    {
                        return NO_CP_AND_CE32;
                    }
                    if (CollationFCD.HasTccc(c))
                    {
                        if (CollationFCD.MaybeTibetanCompositeVowel(c) ||
                                CollationFCD.HasLccc(iter.Current))
                        {
                            iter.Previous();
                            if (!NextSegment())
                            {
                                c = Collation.SentinelCodePoint;
                                return Collation.FALLBACK_CE32;
                            }
                            continue;
                        }
                    }
                    break;
                }
                else if (state == State.IterInFCDSegment && pos != limit)
                {
                    c = iter.Next();
                    ++pos;
                    Debug.Assert(c >= 0);
                    break;
                }
                else if (state.CompareTo(State.InNormIterAtLimit) >= 0 &&
                      pos != normalized.Length)
                {
                    c = normalized[pos++];
                    break;
                }
                else
                {
                    SwitchToForward();
                }
            }
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead((char)c));
        }

        protected override char HandleGetTrailSurrogate()
        {
            if (state.CompareTo(State.IterInFCDSegment) <= 0)
            {
                int trail = iter.Next();
                if (IsTrailSurrogate(trail))
                {
                    if (state == State.IterInFCDSegment) { ++pos; }
                }
                else if (trail >= 0)
                {
                    iter.Previous();
                }
                return (char)trail;
            }
            else
            {
                Debug.Assert(pos < normalized.Length);
                char trail;
                if (char.IsLowSurrogate(trail = normalized[pos])) { ++pos; }
                return trail;
            }
        }

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
        /// </summary>
        private void SwitchToForward()
        {
            Debug.Assert(state == State.IterCheckBwd ||
                    (state == State.IterInFCDSegment && pos == limit) ||
                    (state.CompareTo(State.InNormIterAtLimit) >= 0 && pos == normalized.Length));
            if (state == State.IterCheckBwd)
            {
                // Turn around from backward checking.
                start = pos = iter.Index;
                if (pos == limit)
                {
                    state = State.IterCheckFwd;  // Check forward.
                }
                else
                {  // pos < limit
                    state = State.IterInFCDSegment;  // Stay in FCD segment.
                }
            }
            else
            {
                // Reached the end of the FCD segment.
                if (state == State.IterInFCDSegment)
                {
                    // The input text segment is FCD, extend it forward.
                }
                else
                {
                    // The input text segment needed to be normalized.
                    // Switch to checking forward from it.
                    if (state == State.InNormIterAtStart)
                    {
                        iter.MoveIndex(limit - start);
                    }
                    start = limit;
                }
                state = State.IterCheckFwd;
            }
        }

        /// <summary>
        /// Extends the FCD text segment forward or normalizes around pos.
        /// </summary>
        /// <returns><c>true</c> if success.</returns>
        private bool NextSegment()
        {
            Debug.Assert(state == State.IterCheckFwd);
            // The input text [start..(iter index)[ passes the FCD check.
            pos = iter.Index;
            // Collect the characters being checked, in case they need to be normalized.
            if (s == null)
            {
                s = new StringBuilder();
            }
            else
            {
                s.Length = 0;
            }
            int prevCC = 0;
            for (; ; )
            {
                // Fetch the next character and its fcd16 value.
                int c = iter.NextCodePoint();
                if (c < 0) { break; }
                int fcd16 = nfcImpl.GetFCD16(c);
                int leadCC = fcd16 >> 8;
                if (leadCC == 0 && s.Length != 0)
                {
                    // FCD boundary before this character.
                    iter.PreviousCodePoint();
                    break;
                }
                s.AppendCodePoint(c);
                if (leadCC != 0 && (prevCC > leadCC || CollationFCD.IsFCD16OfTibetanCompositeVowel(fcd16)))
                {
                    // Fails FCD check. Find the next FCD boundary and normalize.
                    for (; ; )
                    {
                        c = iter.NextCodePoint();
                        if (c < 0) { break; }
                        if (nfcImpl.GetFCD16(c) <= 0xff)
                        {
                            iter.PreviousCodePoint();
                            break;
                        }
                        s.AppendCodePoint(c);
                    }
                    Normalize(s);
                    start = pos;
                    limit = pos + s.Length;
                    state = State.InNormIterAtLimit;
                    pos = 0;
                    return true;
                }
                prevCC = fcd16 & 0xff;
                if (prevCC == 0)
                {
                    // FCD boundary after the last character.
                    break;
                }
            }
            limit = pos + s.Length;
            Debug.Assert(pos != limit);
            iter.MoveIndex(-s.Length);
            state = State.IterInFCDSegment;
            return true;
        }

        /// <summary>
        /// Switches to backward checking.
        /// </summary>
        private void SwitchToBackward()
        {
            Debug.Assert(state == State.IterCheckFwd ||
                    (state == State.IterInFCDSegment && pos == start) ||
                    (state.CompareTo(State.InNormIterAtLimit) >= 0 && pos == 0));
            if (state == State.IterCheckFwd)
            {
                // Turn around from forward checking.
                limit = pos = iter.Index;
                if (pos == start)
                {
                    state = State.IterCheckBwd;  // Check backward.
                }
                else
                {  // pos > start
                    state = State.IterInFCDSegment;  // Stay in FCD segment.
                }
            }
            else
            {
                // Reached the start of the FCD segment.
                if (state == State.IterInFCDSegment)
                {
                    // The input text segment is FCD, extend it backward.
                }
                else
                {
                    // The input text segment needed to be normalized.
                    // Switch to checking backward from it.
                    if (state == State.InNormIterAtLimit)
                    {
                        iter.MoveIndex(start - limit);
                    }
                    limit = start;
                }
                state = State.IterCheckBwd;
            }
        }

        /// <summary>
        /// Extends the FCD text segment backward or normalizes around pos.
        /// </summary>
        /// <returns><c>true</c> if success.</returns>
        private bool PreviousSegment()
        {
            Debug.Assert(state == State.IterCheckBwd);
            // The input text [(iter index)..limit[ passes the FCD check.
            pos = iter.Index;
            // Collect the characters being checked, in case they need to be normalized.
            if (s == null)
            {
                s = new StringBuilder();
            }
            else
            {
                s.Length = 0;
            }
            int nextCC = 0;
            for (; ; )
            {
                // Fetch the previous character and its fcd16 value.
                int c = iter.PreviousCodePoint();
                if (c < 0) { break; }
                int fcd16 = nfcImpl.GetFCD16(c);
                int trailCC = fcd16 & 0xff;
                if (trailCC == 0 && s.Length != 0)
                {
                    // FCD boundary after this character.
                    iter.NextCodePoint();
                    break;
                }
                s.AppendCodePoint(c);
                if (trailCC != 0 && ((nextCC != 0 && trailCC > nextCC) ||
                                    CollationFCD.IsFCD16OfTibetanCompositeVowel(fcd16)))
                {
                    // Fails FCD check. Find the previous FCD boundary and normalize.
                    while (fcd16 > 0xff)
                    {
                        c = iter.PreviousCodePoint();
                        if (c < 0) { break; }
                        fcd16 = nfcImpl.GetFCD16(c);
                        if (fcd16 == 0)
                        {
                            iter.NextCodePoint();
                            break;
                        }
                        s.AppendCodePoint(c);
                    }
                    s.Reverse();
                    Normalize(s);
                    limit = pos;
                    start = pos - s.Length;
                    state = State.InNormIterAtStart;
                    pos = normalized.Length;
                    return true;
                }
                nextCC = fcd16 >> 8;
                if (nextCC == 0)
                {
                    // FCD boundary before the following character.
                    break;
                }
            }
            start = pos - s.Length;
            Debug.Assert(pos != start);
            iter.MoveIndex(s.Length);
            state = State.IterInFCDSegment;
            return true;
        }

        private void Normalize(StringBuilder s) // ICU4N specific - changed s parameter from ICharSequence to StringBuilder
        {
            if (normalized == null)
            {
                normalized = new StringBuilder();
            }
            // NFD without argument checking.
            nfcImpl.Decompose(s, normalized);
        }

        private enum State
        {
            /// <summary>
            /// The input text [start..(iter index)[ passes the FCD check.
            /// Moving forward checks incrementally.
            /// pos &amp; limit are undefined.
            /// </summary>
            IterCheckFwd,
            /// <summary>
            /// The input text [(iter index)..limit[ passes the FCD check.
            /// Moving backward checks incrementally.
            /// start &amp; pos are undefined.
            /// </summary>
            IterCheckBwd,
            /// <summary>
            /// The input text [start..limit[ passes the FCD check.
            /// pos tracks the current text index.
            /// </summary>
            IterInFCDSegment,
            /// <summary>
            /// The input text [start..limit[ failed the FCD check and was normalized.
            /// pos tracks the current index in the normalized string.
            /// The text iterator is at the limit index.
            /// </summary>
            InNormIterAtLimit,
            /// <summary>
            /// The input text [start..limit[ failed the FCD check and was normalized.
            /// pos tracks the current index in the normalized string.
            /// The text iterator is at the start index.
            /// </summary>
            InNormIterAtStart,
        }

        private State state;

        private int start;
        private int pos;
        private int limit;

        private readonly Normalizer2Impl nfcImpl;
        private StringBuilder s;
        private StringBuilder normalized;
    }
}
