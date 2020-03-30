using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Text;
using System;
using System.Collections;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class is a wrapper around <see cref="UCharacterIterator"/> and implements the
    /// <see cref="ICharacterEnumerator"/> protocol.
    /// </summary>
    /// <author>ram</author>
    internal class UCharacterEnumeratorWrapper : ICharacterEnumerator // ICU4N TODO: API Changed from public to internal until UCharacterIterator can be converted into an enumerator
    {
        public UCharacterEnumeratorWrapper(UCharacterIterator iter)
        {
            this.iterator = iter;
        }

        private UCharacterIterator iterator;

        /// <summary>
        /// Gets the start index of the text.
        /// </summary>
        public int StartIndex => 0; //UCharacterIterator always starts from 0

        /// <summary>
        /// Gets the end index of the text. This index is the index of the end of the text.
        /// </summary>
        public int EndIndex => Math.Max(iterator.Length - 1, 0);

        /// <inheritdoc/>
        public int Length => iterator.Length;

        /// <summary>
        /// Gets or sets the current index.
        /// </summary>
        public int Index
        {
            get => iterator.Index;
            set => iterator.Index = value;
        }

        /// <summary>
        /// Gets the character at the current position (as returned by <see cref="Index"/>).
        /// </summary>
        /// <returns>
        /// The character at the current position or <see cref="UCharacterIterator.Done"/> if the current
        /// position is off the end of the text.
        /// </returns>
        /// <seealso cref="Index"/>
        public char Current => (char)iterator.Current;

        object IEnumerator.Current => Current;

        /// <summary>
        /// Sets the position to <see cref="StartIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if successful; <c>false</c> if the text is empty.</returns>
        /// <seealso cref="StartIndex"/>
        public bool MoveFirst()
        {
            //UCharacterIterator always iterates from 0 to length
            iterator.SetToStart();
            return iterator.Current != UCharacterIterator.Done;
        }

        /// <summary>
        /// Sets the position to <see cref="EndIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if successful; <c>false</c> if the text is empty.</returns>
        /// <seealso cref="EndIndex"/>
        public bool MoveLast()
        {
            iterator.SetToLimit();
            return iterator.Previous() != UCharacterIterator.Done;
        }

        /// <summary>
        /// Increments the iterator's index by one.
        /// </summary>
        /// <returns><c>true</c> if the index was incremented; <c>false</c> if the current index is <see cref="EndIndex"/>.</returns>
        public bool MoveNext()
        {
            //pre-increment
            iterator.Next();
            return iterator.Current != UCharacterIterator.Done;
        }

        /// <summary>
        /// Decrements the iterator's index by one.
        /// </summary>
        /// <returns><c>true</c> if the index was decremented; <c>false</c> if the current index is <see cref="StartIndex"/>.</returns>
        public bool MovePrevious()
        {
            //pre-decrement
            return iterator.Previous() != UCharacterIterator.Done;
        }

        /// <inheritdoc/>
        public bool TrySetIndex(int value)
        {
            if (value < StartIndex)
            {
                iterator.Index = StartIndex;
                return false;
            }
            if (value > EndIndex)
            {
                iterator.Index = EndIndex;
                return false;
            }
            iterator.Index = value;
            return true;
        }

        /// <summary>
        /// Create a copy of this enumerator.
        /// </summary>
        /// <returns>A copy of this.</returns>
        public object Clone()
        {
            UCharacterEnumeratorWrapper result = (UCharacterEnumeratorWrapper)base.MemberwiseClone();
            result.iterator = (UCharacterIterator)this.iterator.Clone();
            return result;
        }

        void IEnumerator.Reset()
        {
            iterator.Index = StartIndex;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
