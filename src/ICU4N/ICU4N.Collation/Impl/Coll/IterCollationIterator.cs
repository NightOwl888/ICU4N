using ICU4N.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// <see cref="UCharacterIterator"/>-based collation element and character iterator.
    /// Handles normalized text, with length or NUL-terminated.
    /// Unnormalized text is handled by a subclass.
    /// </summary>
    public class IterCollationIterator : CollationIterator
    {
        public IterCollationIterator(CollationData d, bool numeric, UCharacterIterator ui)
            : base(d, numeric)
        {
            iter = ui;
        }

        public override void ResetToOffset(int newOffset)
        {
            Reset();
            iter.Index = newOffset;
        }

        public override int Offset
        {
            get { return iter.Index; }
        }

        public override int MoveNextCodePoint()
        {
            return iter.MoveNextCodePoint();
        }

        public override int MovePreviousCodePoint()
        {
            return iter.MovePreviousCodePoint();
        }

        protected override long HandleNextCE32()
        {
            int c = iter.MoveNext();
            if (c < 0)
            {
                return NO_CP_AND_CE32;
            }
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead((char)c));
        }

        protected override char HandleGetTrailSurrogate()
        {
            int trail = iter.MoveNext();
            if (!IsTrailSurrogate(trail) && trail >= 0) { iter.MovePrevious(); }
            return (char)trail;
        }

        protected override void ForwardNumCodePoints(int num)
        {
            iter.MoveCodePointIndex(num);
        }

        protected override void BackwardNumCodePoints(int num)
        {
            iter.MoveCodePointIndex(-num);
        }

        protected UCharacterIterator iter;
    }
}
