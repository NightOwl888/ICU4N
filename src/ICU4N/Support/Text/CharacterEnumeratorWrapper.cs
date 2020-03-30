using J2N.Text;
using System;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// This class is a wrapper around <see cref="ICharacterEnumerator"/> and implements the
    /// <see cref="CharacterIterator"/> protocol.
    /// </summary>
    internal class CharacterEnumeratorWrapper : CharacterIterator
    {
        private ICharacterEnumerator enumerator;
        // ICU4N: Since our ICharacterEnumerator's EndIndex is the end of the string
        // and in CharacterIterator it is one past the end of the string, we keep track of whether
        // we are past the end of the string with this boolean flag.
        private bool pastEnd = false;

        internal ICharacterEnumerator Enumerator => enumerator;

        public CharacterEnumeratorWrapper(ICharacterEnumerator enumerator)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        }

        /// <inheritdoc/>
        public override char Current
        {
            get
            {
                if (pastEnd)
                    return Done;
                return enumerator.Current;
            }
        }

        /// <inheritdoc/>
        public override int BeginIndex => enumerator.StartIndex;

        /// <inheritdoc/>
        public override int EndIndex => enumerator.EndIndex + 1;

        /// <inheritdoc/>
        public override int Index => enumerator.Index + (pastEnd ? 1 : 0);

        /// <inheritdoc/>
        public override char First()
        {
            pastEnd = false;
            if (enumerator.MoveFirst())
            {
                return enumerator.Current;
            }
            return CharacterIterator.Done;
        }

        /// <inheritdoc/>
        public override char Last()
        {
            pastEnd = false;
            if (enumerator.MoveLast())
            {
                return enumerator.Current;
            }
            return CharacterIterator.Done;
        }

        /// <inheritdoc/>
        public override char Next()
        {
            pastEnd = !enumerator.MoveNext();
            if (pastEnd)
                return Done;

            return enumerator.Current;
        }

        /// <inheritdoc/>
        public override char Previous()
        {
            if (pastEnd)
                pastEnd = false;
            else if (!enumerator.MovePrevious())
                return Done;
            return enumerator.Current;
        }

        /// <inheritdoc/>
        public override char SetIndex(int location)
        {
            if (location < BeginIndex || location > EndIndex + 1)
                throw new ArgumentException("Invalid index");

            pastEnd = !enumerator.TrySetIndex(location);
            if (pastEnd)
                return Done;

            return enumerator.Current;
        }

        /// <inheritdoc/>
        public override object Clone()
        {
            var result = (CharacterEnumeratorWrapper)MemberwiseClone();
            result.enumerator = (ICharacterEnumerator)enumerator.Clone();
            return result;
        }

        /// <summary>
        /// Compares the specified object with this <see cref="CharacterEnumeratorWrapper"/>
        /// and indicates if they are equal. In order to be equal, <paramref name="obj"/>
        /// must be an instance of <see cref="CharacterEnumeratorWrapper"/> that iterates over
        /// the same sequence of characters with the same index.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns><c>true</c> if the specified object is equal to this <see cref="CharacterEnumeratorWrapper"/>; <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (!(obj is CharacterEnumeratorWrapper other))
            {
                return false;
            }
            return pastEnd == other.pastEnd && this.enumerator.Equals(other.Enumerator);
        }

        /// <summary>
        /// Gets the hash code for this <see cref="StringCharacterEnumerator"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return pastEnd.GetHashCode() + this.enumerator.GetHashCode();
        }
    }
}
