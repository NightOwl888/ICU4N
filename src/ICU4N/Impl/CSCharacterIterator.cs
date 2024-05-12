using ICU4N.Support.Text;
using ICU4N.Text;
using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// Implement the <see cref="CharacterIterator"/> abstract class on a <see cref="ReadOnlyMemory{Char}"/>.
    /// Intended for internal use by ICU only.
    /// </summary>
    internal class ReadOnlyMemoryCharacterIterator : CharacterIterator // ICU4N TODO: API - convert to an enumerator..? and move to J2N
    {
        private int index;
        private readonly ReadOnlyMemory<char> seq;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The <see cref="ReadOnlyMemory{Char}"/> to iterate over.</param>
        public ReadOnlyMemoryCharacterIterator(ReadOnlyMemory<char> text)
        {
            seq = text;
            index = 0;
        }

        /// <summary>
        /// Sets the current position to the begin index and returns the character at
        /// the new position.
        /// </summary>
        /// <returns>The character at the begin index.</returns>
        public override char First()
        {
            index = 0;
            return Current;
        }

        /// <summary>
        /// Sets the current position to the end index - 1 and returns the character
        /// at the new position.
        /// </summary>
        /// <returns>The character before the end index.</returns>
        public override char Last()
        {
            index = seq.Length;
            return Previous();
        }

        /// <summary>
        /// Returns the character at the current index, or <see cref="CharacterIterator.Done"/> if the current index is
        /// past the beginning or end of the sequence.
        /// </summary>
        public override char Current
        {
            get
            {
                if (index == seq.Length)
                {
                    return Done;
                }
                return seq.Span[index];
            }
        }

        /// <summary>
        /// Increments the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the next index, or <see cref="CharacterIterator.Done"/> if the next
        /// index would be past the end.</returns>
        public override char Next()
        {
            if (index < seq.Length)
            {
                ++index;
            }
            return Current;
        }

        /// <summary>
        /// Decrements the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the previous index, or <see cref="CharacterIterator.Done"/> if the
        /// previous index would be past the beginning.</returns>
        public override char Previous()
        {
            if (index == 0)
            {
                return Done;
            }
            --index;
            return Current;
        }

        /// <summary>
        /// Sets the current index to a new position and returns the character at the
        /// new index.
        /// </summary>
        /// <param name="position">The new index that this character iterator is set to.</param>
        /// <returns>The character at the new index, or <see cref="CharacterIterator.Done"/> if the index is
        /// past the end.</returns>
        /// <exception cref="System.ArgumentException">If <paramref name="position"/> is less than 
        /// the begin index or greater than the end index.</exception>
        public override char SetIndex(int position)
        {
            if (position < 0 || position > seq.Length)
            {
                throw new ArgumentException();
            }
            index = position;
            return Current;
        }

        /// <summary>
        /// Gets the begin index. Returns the index of the first character of the iteration.
        /// </summary>
        public override int BeginIndex => 0;

        /// <summary>
        /// Gets the end index. Returns the index one past the last character of the iteration.
        /// </summary>
        public override int EndIndex => seq.Length;

        /// <summary>
        /// Gets the current index.
        /// </summary>
        public override int Index => index;

#if FEATURE_CLONEABLE
        /// <summary>
        /// Returns a new <see cref="CharacterIterator"/> with the same properties.
        /// </summary>
        /// <returns>A shallow copy of this character iterator.</returns>
        /// <seealso cref="ICloneable"/>
#else
        /// <summary>
        /// Returns a new <see cref="CharacterIterator"/> with the same properties.
        /// </summary>
        /// <returns>A shallow copy of this character iterator.</returns>
#endif
        public override object Clone()
        {
            ReadOnlyMemoryCharacterIterator copy = new ReadOnlyMemoryCharacterIterator(seq);
            copy.SetIndex(index);
            return copy;
        }

        // ICU4N specific: Added Equality checking
        /// <summary>
        /// Compares the specified object with this <see cref="ReadOnlyMemoryCharacterIterator"/>
        /// and indicates if they are equal. In order to be equal, <paramref name="obj"/>
        /// must be an instance of <see cref="ReadOnlyMemoryCharacterIterator"/> that iterates over
        /// the same sequence of characters with the same index.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns><c>true</c> if the specified object is equal to this <see cref="ReadOnlyMemoryCharacterIterator"/>; <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemoryCharacterIterator it)
            {
                return seq.Span.Equals(it.seq.Span, StringComparison.Ordinal) && index == it.index;
            }
            // ICU4N TODO: We need all iterator types to be able to compare equal if they have the same string, bounds, and position.
            return false;
        }

        public override int GetHashCode()
        {
            return StringHelper.GetHashCode(seq.Span) ^ index;
        }
    }
}
