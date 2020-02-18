using ICU4N.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// <see cref="UCharacterIterator"/>-based collation element and character iterator.
    /// Handles normalized text, with length or NUL-terminated.
    /// Unnormalized text is handled by a subclass.
    /// </summary>
    internal class IterCollationIterator : CollationIterator // ICU4N TODO: API Changed from public to internal until this can be converted into an enumerator
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

        public override int Offset => iter.Index;

        public override int NextCodePoint()
        {
            return iter.NextCodePoint();
        }

        public override int PreviousCodePoint()
        {
            return iter.PreviousCodePoint();
        }

        protected override long HandleNextCE32()
        {
            int c = iter.Next();
            if (c < 0)
            {
                return NoCodePointAndCE32;
            }
            return MakeCodePointAndCE32Pair(c, trie.GetFromU16SingleLead((char)c));
        }

        protected override char HandleGetTrailSurrogate()
        {
            int trail = iter.Next();
            if (!IsTrailSurrogate(trail) && trail >= 0) { iter.Previous(); }
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
