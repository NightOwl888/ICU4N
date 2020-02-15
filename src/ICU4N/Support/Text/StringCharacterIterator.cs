using System;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// An implementation of <see cref="CharacterIterator"/> for strings.
    /// </summary>
    public sealed class StringCharacterIterator : CharacterIterator
    {
        private string str;

        private int start, end, offset;

        /// <summary>
        /// Constructs a new <see cref="StringCharacterIterator"/> on the specified string.
        /// The begin and current indices are set to the beginning of <paramref name="value"/>, the
        /// end index is set to the length of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The source string to iterate over.</param>
        public StringCharacterIterator(string value)
            : this(value, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="StringCharacterIterator"/> on the specified string
        /// with the current index set to the specified <paramref name="value"/>. The begin index is set
        /// to the beginning of <paramref name="value"/>, the end index is set to the length of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The source string to iterate over.</param>
        /// <param name="location">The current index.</param>
        /// <exception cref="ArgumentException">If <paramref name="location"/> is negative or greater than the length
        /// of <paramref name="value"/>.</exception>
        public StringCharacterIterator(string value, int location)
            : this(value, 0, value != null ? value.Length : 0, location)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="StringCharacterIterator"/> on the specified string
        /// with the begin, end and current index set to the specified values.
        /// </summary>
        /// <param name="value">The source string to iterate over.</param>
        /// <param name="start">The index of the first character to iterate.</param>
        /// <param name="end">The index one past the last character to iterate.</param>
        /// <param name="location">The current index.</param>
        /// <exception cref="ArgumentException">If <c>start &lt; 0</c>, <c>start &gt; end</c>, <c>location &lt; start</c>, <c>location &gt; end</c>
        /// or if <paramref name="end"/> is greater than the length of <paramref name="value"/>.</exception>
        public StringCharacterIterator(string value, int start, int end,
                int location)
        {
            str = value ?? throw new ArgumentNullException(nameof(value));
            if (start < 0 || end > str.Length || start > end
                    || location < start || location > end)
            {
                throw new ArgumentException();
            }
            this.start = start;
            this.end = end;
            offset = location;
        }

        /// <summary>
        /// Returns a new <see cref="StringCharacterIterator"/> with the same source
        /// string, begin, end, and current index as this iterator.
        /// </summary>
        /// <returns>A shallow copy of this iterator.</returns>
        public override object Clone()
        {
            return base.MemberwiseClone();
        }

        /// <summary>
        /// Gets the character at the current index in the source string.
        /// <para/>
        /// Returns the current character, or <see cref="CharacterIterator.Done"/> if the current index is
        /// past the end.
        /// </summary>
        public override char Current
        {
            get
            {
                if (offset == end)
                {
                    return Done;
                }
                return str[offset];
            }
        }

        /// <summary>
        /// Compares the specified object with this <see cref="StringCharacterIterator"/>
        /// and indicates if they are equal. In order to be equal, <paramref name="obj"/>
        /// must be an instance of <see cref="StringCharacterIterator"/> that iterates over
        /// the same sequence of characters with the same index.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns><c>true</c> if the specified object is equal to this <see cref="StringCharacterIterator"/>; <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (!(obj is StringCharacterIterator)) {
                return false;
            }
            StringCharacterIterator it = (StringCharacterIterator)obj;
            return str.Equals(it.str) && start == it.start && end == it.end
                    && offset == it.offset;
        }

        /// <summary>
        /// Sets the current position to the begin index and returns the character at
        /// the new position in the source string.
        /// </summary>
        /// <returns>The character at the begin index or <see cref="CharacterIterator.Done"/> if the begin
        /// index is equal to the end index.</returns>
        public override char First()
        {
            if (start == end)
            {
                return Done;
            }
            offset = start;
            return str[offset];
        }

        /// <summary>
        /// Gets the begin index in the source string.
        /// <para/>
        /// Returns the index of the first character of the iteration.
        /// </summary>
        public override int BeginIndex => start;

        /// <summary>
        /// Gets the end index in the source string.
        /// <para/>
        /// Returns the index one past the last character of the iteration.
        /// </summary>
        public override int EndIndex => end;

        /// <summary>
        /// Gets the current index in the source string.
        /// </summary>
        public override int Index => offset;

        public override int GetHashCode()
        {
            return str.GetHashCode() ^ start ^ end ^ offset;
        }

        /// <summary>
        /// Sets the current position to the end index - 1 and returns the character
        /// at the new position.
        /// </summary>
        /// <returns>The character before the end index or <see cref="CharacterIterator.Done"/> if the begin
        /// index is equal to the end index.</returns>
        public override char Last()
        {
            if (start == end)
            {
                return Done;
            }
            offset = end - 1;
            return str[offset];
        }

        /// <summary>
        /// Increments the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the next index, or <see cref="CharacterIterator.Done"/> if the
        /// next index would be past the end.</returns>
        public override char Next()
        {
            if (offset >= (end - 1))
            {
                offset = end;
                return Done;
            }
            return str[++offset];
        }

        /// <summary>
        /// Decrements the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the previous index, or <see cref="CharacterIterator.Done"/> if the
        /// previous index would be past the beginning.</returns>
        public override char Previous()
        {
            if (offset == start)
            {
                return Done;
            }
            return str[--offset];
        }

        /// <summary>
        /// Sets the current index in the source string.
        /// </summary>
        /// <param name="location">The index the current position is set to.</param>
        /// <returns>The character at the new index, or <see cref="CharacterIterator.Done"/> if
        /// <paramref name="location"/> is set to the end index.</returns>
        /// <exception cref="ArgumentException">If <paramref name="location"/> is smaller than the begin index or greater
        /// than the end index.</exception>
        public override char SetIndex(int location)
        {
            if (location < start || location > end)
            {
                throw new ArgumentException("Invalid index");
            }
            offset = location;
            if (offset == end)
            {
                return Done;
            }
            return str[offset];
        }

        /// <summary>
        /// Sets the source string to iterate over. The begin and end positions are
        /// set to the start and end of this string.
        /// </summary>
        /// <param name="value">The new source string.</param>
        public void SetText(string value)
        {
            str = value ?? throw new ArgumentNullException(nameof(value));
            start = offset = 0;
            end = value.Length;
        }
    }
}
