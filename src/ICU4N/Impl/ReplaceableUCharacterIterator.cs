using ICU4N.Text;
using J2N;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    // DLF docs must define behavior when Replaceable is mutated underneath
    // the iterator.
    // 
    // This and ICUCharacterIterator share some code, maybe they should share
    // an implementation, or the common state and implementation should be
    // moved up into UCharacterIterator.
    // 
    // What are first, last, and getBeginIndex doing here?!?!?!
    public class ReplaceableUCharacterIterator : UCharacterIterator
    {
        // public constructor ------------------------------------------------------

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="replaceable">Text which the iterator will be based on.</param>
        public ReplaceableUCharacterIterator(IReplaceable replaceable)
        {
            this.replaceable = replaceable ?? throw new ArgumentNullException(nameof(replaceable));
            this.currentIndex = 0;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="str">Text which the iterator will be based on.</param>
        public ReplaceableUCharacterIterator(string str)
        {
            if (str == null)
            {
                throw new ArgumentException();
            }
            this.replaceable = new ReplaceableString(str);
            this.currentIndex = 0;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="buf">Buffer of text on which the iterator will be based.</param>
        public ReplaceableUCharacterIterator(StringBuffer buf)
        {
            if (buf == null)
            {
                throw new ArgumentException();
            }
            this.replaceable = new ReplaceableString(buf);
            this.currentIndex = 0;
        }

        // public methods ----------------------------------------------------------

        /// <summary>
        /// Creates a copy of this iterator, does not clone the underlying
        /// <see cref="IReplaceable"/> object.
        /// </summary>
        /// <returns>Copy of this iterator.</returns>
        public override object Clone()
        {
            return base.MemberwiseClone();
        }

        /// <summary>
        /// Gets the current <see cref="UTF16"/> character.
        /// </summary>
        public override int Current
        {
            get
            {
                if (currentIndex < replaceable.Length)
                {
                    return replaceable[currentIndex];
                }
                return UForwardCharacterIterator.Done;
            }
        }

        /// <summary>
        /// Returns the current codepoint.
        /// </summary>
        /// <returns>Current codepoint.</returns>
        public override int CurrentCodePoint
        {
            get
            {
                // cannot use charAt due to it different
                // behaviour when index is pointing at a
                // trail surrogate, check for surrogates

                int ch = Current;
                if (UTF16.IsLeadSurrogate((char)ch))
                {
                    // advance the index to get the next code point
                    Next();
                    // due to post increment semantics current() after next()
                    // actually returns the next char which is what we want
                    int ch2 = Current;
                    // current should never change the current index so back off
                    Previous();

                    if (UTF16.IsTrailSurrogate((char)ch2))
                    {
                        // we found a surrogate pair
                        return Character.ToCodePoint((char)ch, (char)ch2);
                    }
                }
                return ch;
            }
        }

        /// <summary>
        /// Gets the length of the text.
        /// </summary>
        public override int Length => replaceable.Length;

        /// <summary>
        /// Gets or Sets the current <see cref="currentIndex"/> in text.
        /// This assumes the text is stored as 16-bit code units.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">if an invalid value is
        ///            supplied. i.e. value is out of bounds.</exception>
        public override int Index
        {
            get => currentIndex;
            set
            {
                if (value < 0 || value > replaceable.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                this.currentIndex = value;
            }
        }

        /// <summary>
        /// Returns next UTF16 character and increments the iterator's <see cref="currentIndex"/> by 1.
        /// If the resulting <see cref="currentIndex"/> is greater or equal to the text length, the
        /// <see cref="currentIndex"/> is reset to the text length and a value of <see cref="UForwardCharacterIterator.Done"/> is
        /// returned.
        /// </summary>
        /// <returns>Next UTF16 character in text or <see cref="UForwardCharacterIterator.Done"/> if the new <see cref="currentIndex"/> is off the
        ///         end of the text range.</returns>
        public override int Next()
        {
            if (currentIndex < replaceable.Length)
            {
                return replaceable[currentIndex++];
            }
            return UForwardCharacterIterator.Done;
        }

        /// <summary>
        /// Returns previous UTF16 character and decrements the iterator's <see cref="currentIndex"/> by
        /// 1.
        /// If the resulting <see cref="currentIndex"/> is less than 0, the <see cref="currentIndex"/> is reset to 0 and a
        /// value of <see cref="UForwardCharacterIterator.Done"/> is returned.
        /// </summary>
        /// <returns>Next UTF16 character in text or <see cref="UForwardCharacterIterator.Done"/> if the new <see cref="currentIndex"/> is off the
        ///         start of the text range.</returns>
        public override int Previous()
        {
            if (currentIndex > 0)
            {
                return replaceable[--currentIndex];
            }
            return UForwardCharacterIterator.Done;
        }

        // ICU4N specific - moved setter to the Index property

        public override int GetText(char[] destination, int offset)
        {
            // ICU4N: Reworked guard clauses
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            int length = replaceable.Length;
            if (offset < 0 || offset > length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > destination.Length)
                throw new ArgumentException($"Not enough space in the destination array: {length}", nameof(destination));

            replaceable.CopyTo(0, destination, offset, length);
            return length;
        }

        // private data members ----------------------------------------------------

        /// <summary>
        /// Replacable object
        /// </summary>
        private readonly IReplaceable replaceable;
        /// <summary>
        /// Current currentIndex
        /// </summary>
        private int currentIndex;
    }
}
