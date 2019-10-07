using ICU4N.Support.Text;
using System.Diagnostics;

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

        public UTF16CollationIterator(CollationData d, bool numeric, ICharSequence s, int p)
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
            UTF16CollationIterator o = (UTF16CollationIterator)other;
            // Compare the iterator state but not the text: Assume that the caller does that.
            return (pos - start) == (o.pos - o.start);
        }

        public override int GetHashCode()
        {
            Debug.Assert(false, "hashCode not designed");
            return 42; // any arbitrary constant will do
        }

        public override void ResetToOffset(int newOffset)
        {
            Reset();
            pos = start + newOffset;
        }

        public override int Offset
        {
            get { return pos - start; }
        }

        public virtual void SetText(bool numeric, ICharSequence s, int p)
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
            char c = seq[pos++];
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
            if (pos == start)
            {
                return Collation.SentinelCodePoint;
            }
            char c = seq[--pos];
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
            if (pos == limit)
            {
                return NoCodePointAndCE32;
            }
            char c = seq[pos++];
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead(c));
        }

        protected override char HandleGetTrailSurrogate()
        {
            if (pos == limit) { return (char)0; }
            char trail;
            if (char.IsLowSurrogate(trail = seq[pos])) { ++pos; }
            return trail;
        }

        /* bool foundNULTerminator(); */

        protected override void ForwardNumCodePoints(int num)
        {
            while (num > 0 && pos != limit)
            {
                char c = seq[pos++];
                --num;
                if (char.IsHighSurrogate(c) && pos != limit &&
                        char.IsLowSurrogate(seq[pos]))
                {
                    ++pos;
                }
            }
        }

        protected override void BackwardNumCodePoints(int num)
        {
            while (num > 0 && pos != start)
            {
                char c = seq[--pos];
                --num;
                if (char.IsLowSurrogate(c) && pos != start &&
                        char.IsHighSurrogate(seq[pos - 1]))
                {
                    --pos;
                }
            }
        }

        protected ICharSequence seq;
        protected int start;
        protected int pos;
        protected int limit;
    }
}
