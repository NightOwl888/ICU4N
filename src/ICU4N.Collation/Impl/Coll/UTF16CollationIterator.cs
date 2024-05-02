using J2N;
using J2N.Text;
using System;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// UTF-16 collation element and character iterator.
    /// Handles normalized UTF-16 text, with length or NUL-terminated.
    /// Unnormalized text is handled by a subclass.
    /// </summary>
    public class UTF16CollationIterator : CollationIterator
    {
        /// <summary>
        /// Partial constructor, see <see cref="CollationIterator.CollationIterator(CollationData)"/>
        /// </summary>
        public UTF16CollationIterator(CollationData d)
            : base(d)
        {
        }

        // ICU4N: The value for s must have a reference to it that has a lifetime longer than this class.
        public UTF16CollationIterator(CollationData d, bool numeric, ReadOnlyMemory<char> s, int p)
            : base(d, numeric)
        {
            seq = s;
            start = 0;
            pos = p;
            limit = s.Length;
        }

        public override bool Equals(object other)
        {
            if (!base.Equals(other)) { return false; }
            if (other is UTF16CollationIterator o)
            {
                // Compare the iterator state but not the text: Assume that the caller does that.
                return (pos - start) == (o.pos - o.start);
            }
            return false;
        }

        public override int GetHashCode()
        {
            // ICU4N specific - implemented hash code
            return (pos - start).GetHashCode();
        }

        public override void ResetToOffset(int newOffset)
        {
            Reset();
            pos = start + newOffset;
        }

        public override int Offset => pos - start;

        // ICU4N: The value for s must have a reference to it that has a lifetime longer than this class.
        public virtual void SetText(bool numeric, ReadOnlyMemory<char> s, int p)
        {
            Reset(numeric);
            seq = s;
            start = 0;
            pos = p;
            limit = s.Length;
        }

        public override int NextCodePoint()
        {
            if (pos == limit)
            {
                return Collation.SentinelCodePoint;
            }
            ReadOnlySpan<char> seqSpan = seq.Span;
            char c = seqSpan[pos++];
            char trail;
            if (char.IsHighSurrogate(c) && pos != limit &&
                    char.IsLowSurrogate(trail = seqSpan[pos]))
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
            if (pos == start)
            {
                return Collation.SentinelCodePoint;
            }
            ReadOnlySpan<char> seqSpan = seq.Span;
            char c = seqSpan[--pos];
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
            if (pos == limit)
            {
                return NoCodePointAndCE32;
            }
            char c = seq.Span[pos++];
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead(c));
        }

        protected override char HandleGetTrailSurrogate()
        {
            if (pos == limit) { return (char)0; }
            char trail;
            if (char.IsLowSurrogate(trail = seq.Span[pos])) { ++pos; }
            return trail;
        }

        /* bool foundNULTerminator(); */

        protected override void ForwardNumCodePoints(int num)
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            while (num > 0 && pos != limit)
            {
                char c = seqSpan[pos++];
                --num;
                if (char.IsHighSurrogate(c) && pos != limit &&
                        char.IsLowSurrogate(seqSpan[pos]))
                {
                    ++pos;
                }
            }
        }

        protected override void BackwardNumCodePoints(int num)
        {
            ReadOnlySpan<char> seqSpan = seq.Span;
            while (num > 0 && pos != start)
            {
                char c = seqSpan[--pos];
                --num;
                if (char.IsLowSurrogate(c) && pos != start &&
                        char.IsHighSurrogate(seqSpan[pos - 1]))
                {
                    --pos;
                }
            }
        }

        protected ReadOnlyMemory<char> seq;
        protected int start;
        protected int pos;
        protected int limit;
    }
}
