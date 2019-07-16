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

        /// <summary>
        /// Sets the position to <see cref="BeginIndex"/> and returns the character at that
        /// position.
        /// </summary>
        /// <returns>The first character in the text, or <see cref="UCharacterIterator.DONE"/> if the text is empty.</returns>
        /// <seealso cref="BeginIndex"/>
        public override char MoveFirst()
        {
            //UCharacterIterator always iterates from 0 to length
            iterator.SetToStart();
            return (char)iterator.Current;
        }

        /// <summary>
        /// Sets the position to <see cref="EndIndex"/>-1 (<see cref="EndIndex"/> if the text is empty)
        /// and returns the character at that position.
        /// </summary>
        /// <returns>The last character in the text, or <see cref="UCharacterIterator.DONE"/> if the text is empty.</returns>
        /// <seealso cref="EndIndex"/>
        public override char MoveLast()
        {
            iterator.SetToLimit();
            return (char)iterator.MovePrevious();
        }

        /// <summary>
        /// Gets the character at the current position (as returned by <see cref="Index"/>).
        /// </summary>
        /// <returns>
        /// the character at the current position or <see cref="UCharacterIterator.DONE"/> if the current
        /// position is off the end of the text.
        /// </returns>
        /// <seealso cref="Index"/>
        public override char Current
        {
            get { return (char)iterator.Current; }
        }

        /// <summary>
        /// Increments the iterator's index by one and returns the character
        /// at the new index.  If the resulting index is greater or equal
        /// to <see cref="EndIndex"/>, the current index is reset to <see cref="EndIndex"/> and
        /// a value of <see cref="UCharacterIterator.DONE"/> is returned.
        /// </summary>
        /// <returns>The character at the new position or <see cref="UCharacterIterator.DONE"/> if the new
        /// position is off the end of the text range.</returns>
        public override char MoveNext()
        {
            //pre-increment
            iterator.MoveNext();
            return (char)iterator.Current;
        }

        /// <summary>
        /// Decrements the iterator's index by one and returns the character
        /// at the new index. If the current index is <see cref="BeginIndex"/>, the index
        /// remains at <see cref="BeginIndex"/> and a value of <see cref="UCharacterIterator.DONE"/> is returned.
        /// </summary>
        /// <returns>the character at the new position or <see cref="UCharacterIterator.DONE"/> if the current
        /// position is equal to <see cref="BeginIndex"/>.</returns>
        public override char MovePrevious()
        {
            //pre-decrement
            return (char)iterator.MovePrevious();
        }

        /// <summary>
        /// Sets the position to the specified position in the text and returns that
        /// character.
        /// </summary>
        /// <param name="position">The position within the text.  Valid values range from
        /// <see cref="BeginIndex"/> to <see cref="EndIndex"/>. An <see cref="System.ArgumentException"/> is thrown
        /// if an invalid value is supplied.</param>
        /// <returns>The character at the specified position or <see cref="UCharacterIterator.DONE"/> if 
        /// the specified position is equal to <see cref="EndIndex"/>.</returns>
        public override char SetIndex(int position)
        {
            iterator.Index = position;
            return (char)iterator.Current;
        }

        /// <summary>
        /// Gets the start index of the text.
        /// </summary>
        public override int BeginIndex
        {
            //UCharacterIterator always starts from 0
            get { return 0; }
        }

        /// <summary>
        /// Gets the end index of the text.  This index is the index of the first
        /// character following the end of the text.
        /// </summary>
        public override int EndIndex
        {
            get { return iterator.Length; }
        }

        /// <summary>
        /// Gets the current index.
        /// </summary>
        public override int Index
        {
            get { return iterator.Index; }
        }

        /// <summary>
        /// Create a copy of this iterator.
        /// </summary>
        /// <returns>A copy of this.</returns>
        public override object Clone()
        {
            UCharacterIteratorWrapper result = (UCharacterIteratorWrapper)base.MemberwiseClone();
            result.iterator = (UCharacterIterator)this.iterator.Clone();
            return result;
        }
    }
}
