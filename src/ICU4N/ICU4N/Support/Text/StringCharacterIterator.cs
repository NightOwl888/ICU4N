using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// An implementation of <see cref="CharacterIterator"/> for strings.
    /// </summary>
    public sealed class StringCharacterIterator : CharacterIterator
    {
        string str;

    int start, end, offset;

        /**
         * Constructs a new {@code StringCharacterIterator} on the specified string.
         * The begin and current indices are set to the beginning of the string, the
         * end index is set to the length of the string.
         * 
         * @param value
         *            the source string to iterate over.
         */
        public StringCharacterIterator(string value)
        {
            str = value;
            start = offset = 0;
            end = str.Length;
        }

        /**
         * Constructs a new {@code StringCharacterIterator} on the specified string
         * with the current index set to the specified value. The begin index is set
         * to the beginning of the string, the end index is set to the length of the
         * string.
         * 
         * @param value
         *            the source string to iterate over.
         * @param location
         *            the current index.
         * @throws IllegalArgumentException
         *            if {@code location} is negative or greater than the length
         *            of the source string.
         */
        public StringCharacterIterator(string value, int location)
        {
            str = value;
            start = 0;
            end = str.Length;
            if (location < 0 || location > end)
            {
                throw new ArgumentException();
            }
            offset = location;
        }

        /**
         * Constructs a new {@code StringCharacterIterator} on the specified string
         * with the begin, end and current index set to the specified values.
         * 
         * @param value
         *            the source string to iterate over.
         * @param start
         *            the index of the first character to iterate.
         * @param end
         *            the index one past the last character to iterate.
         * @param location
         *            the current index.
         * @throws IllegalArgumentException
         *            if {@code start < 0}, {@code start > end}, {@code location <
         *            start}, {@code location > end} or if {@code end} is greater
         *            than the length of {@code value}.
         */
        public StringCharacterIterator(string value, int start, int end,
                int location)
        {
            str = value;
            if (start < 0 || end > str.Length || start > end
                    || location < start || location > end)
            {
                throw new ArgumentException();
            }
            this.start = start;
            this.end = end;
            offset = location;
        }

        /**
         * Returns a new {@code StringCharacterIterator} with the same source
         * string, begin, end, and current index as this iterator.
         * 
         * @return a shallow copy of this iterator.
         * @see java.lang.Cloneable
         */
    public override object Clone()
        {
            return base.MemberwiseClone();
        }

        /**
         * Returns the character at the current index in the source string.
         * 
         * @return the current character, or {@code DONE} if the current index is
         *         past the end.
         */
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

        /**
         * Compares the specified object with this {@code StringCharacterIterator}
         * and indicates if they are equal. In order to be equal, {@code object}
         * must be an instance of {@code StringCharacterIterator} that iterates over
         * the same sequence of characters with the same index.
         * 
         * @param object
         *            the object to compare with this object.
         * @return {@code true} if the specified object is equal to this
         *         {@code StringCharacterIterator}; {@code false} otherwise.
         * @see #hashCode
         */
    public override bool Equals(object obj)
        {
            if (!(obj is StringCharacterIterator)) {
                return false;
            }
            StringCharacterIterator it = (StringCharacterIterator)obj;
            return str.Equals(it.str) && start == it.start && end == it.end
                    && offset == it.offset;
        }

        /**
         * Sets the current position to the begin index and returns the character at
         * the new position in the source string.
         * 
         * @return the character at the begin index or {@code DONE} if the begin
         *         index is equal to the end index.
         */
        public override char MoveFirst()
        {
            if (start == end)
            {
                return Done;
            }
            offset = start;
            return str[offset];
        }

        /**
         * Returns the begin index in the source string.
         * 
         * @return the index of the first character of the iteration.
         */
        public override int BeginIndex
        {
            get { return start; }
        }

        /**
         * Returns the end index in the source string.
         * 
         * @return the index one past the last character of the iteration.
         */
        public override int EndIndex
        {
            get { return end; }
        }

        /**
         * Returns the current index in the source string.
         * 
         * @return the current index.
         */
        public override int Index
        {
            get { return offset; }
        }

    public override int GetHashCode()
        {
            return str.GetHashCode() + start + end + offset;
        }

        /**
         * Sets the current position to the end index - 1 and returns the character
         * at the new position.
         * 
         * @return the character before the end index or {@code DONE} if the begin
         *         index is equal to the end index.
         */
        public override char MoveLast()
        {
            if (start == end)
            {
                return Done;
            }
            offset = end - 1;
            return str[offset];
        }

        /**
         * Increments the current index and returns the character at the new index.
         *
         * @return the character at the next index, or {@code DONE} if the next
         *         index would be past the end.
         */
        public override char MoveNext()
        {
            if (offset >= (end - 1))
            {
                offset = end;
                return Done;
            }
            return str[++offset];
        }

        /**
         * Decrements the current index and returns the character at the new index.
         * 
         * @return the character at the previous index, or {@code DONE} if the
         *         previous index would be past the beginning.
         */
        public override char MovePrevious()
        {
            if (offset == start)
            {
                return Done;
            }
            return str[--offset];
        }

        /**
         * Sets the current index in the source string.
         * 
         * @param location
         *            the index the current position is set to.
         * @return the character at the new index, or {@code DONE} if
         *         {@code location} is set to the end index.
         * @throws IllegalArgumentException
         *            if {@code location} is smaller than the begin index or greater
         *            than the end index.
         */
        public override char SetIndex(int location)
        {
            if (location < start || location > end)
            {
                throw new ArgumentException();
            }
            offset = location;
            if (offset == end)
            {
                return Done;
            }
            return str[offset];
        }

        /**
         * Sets the source string to iterate over. The begin and end positions are
         * set to the start and end of this string.
         * 
         * @param value
         *            the new source string.
         */
        public void SetText(string value)
        {
            str = value;
            start = offset = 0;
            end = value.Length;
        }
    }
}
