using ICU4N.Support.Text;
using ICU4N.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class is a wrapper around <see cref="UCharacterIterator"/> and implements the
    /// <see cref="CharacterIterator"/> protocol
    /// </summary>
    /// <author>ram</author>
    public class UCharacterIteratorWrapper : CharacterIterator
    {
        public UCharacterIteratorWrapper(UCharacterIterator iter)
        {
            this.iterator = iter;
        }

        private UCharacterIterator iterator;


        /**
         * Sets the position to getBeginIndex() and returns the character at that
         * position.
         * @return the first character in the text, or DONE if the text is empty
         * @see #getBeginIndex()
         */

        public override char First()
        {
            //UCharacterIterator always iterates from 0 to length
            iterator.SetToStart();
            return (char)iterator.Current;
        }

        /**
         * Sets the position to getEndIndex()-1 (getEndIndex() if the text is empty)
         * and returns the character at that position.
         * @return the last character in the text, or DONE if the text is empty
         * @see #getEndIndex()
         */

        public override char Last()
        {
            iterator.SetToLimit();
            return (char)iterator.Previous();
        }

        /**
         * Gets the character at the current position (as returned by getIndex()).
         * @return the character at the current position or DONE if the current
         * position is off the end of the text.
         * @see #getIndex()
         */

        public override char Current
        {
            get { return (char)iterator.Current; }
        }

        /**
         * Increments the iterator's index by one and returns the character
         * at the new index.  If the resulting index is greater or equal
         * to getEndIndex(), the current index is reset to getEndIndex() and
         * a value of DONE is returned.
         * @return the character at the new position or DONE if the new
         * position is off the end of the text range.
         */

        public override char Next()
        {
            //pre-increment
            iterator.Next();
            return (char)iterator.Current;
        }

        /**
         * Decrements the iterator's index by one and returns the character
         * at the new index. If the current index is getBeginIndex(), the index
         * remains at getBeginIndex() and a value of DONE is returned.
         * @return the character at the new position or DONE if the current
         * position is equal to getBeginIndex().
         */

        public override char Previous()
        {
            //pre-decrement
            return (char)iterator.Previous();
        }

        /**
         * Sets the position to the specified position in the text and returns that
         * character.
         * @param position the position within the text.  Valid values range from
         * getBeginIndex() to getEndIndex().  An IllegalArgumentException is thrown
         * if an invalid value is supplied.
         * @return the character at the specified position or DONE if the specified position is equal to getEndIndex()
         */

        public override char SetIndex(int position)
        {
            iterator.Index = position;
            return (char)iterator.Current;
        }

        /**
         * Returns the start index of the text.
         * @return the index at which the text begins.
         */

        public override int BeginIndex
        {
            //UCharacterIterator always starts from 0
            get { return 0; }
        }

        /**
         * Returns the end index of the text.  This index is the index of the first
         * character following the end of the text.
         * @return the index after the last character in the text
         */

        public override int EndIndex
        {
            get { return iterator.Length; }
        }

        /**
         * Returns the current index.
         * @return the current index.
         */

        public override int Index
        {
            get { return iterator.Index; }
        }

        /**
         * Create a copy of this iterator
         * @return A copy of this
         */
        public override object Clone()
        {
            UCharacterIteratorWrapper result = (UCharacterIteratorWrapper)base.MemberwiseClone();
            result.iterator = (UCharacterIterator)this.iterator.Clone();
            return result;
        }
    }
}
