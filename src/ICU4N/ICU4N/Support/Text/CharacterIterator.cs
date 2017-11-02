using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    /**
     * An interface for the bidirectional iteration over a group of characters. The
     * iteration starts at the begin index in the group of characters and continues
     * to one index before the end index.
     */
    public abstract class CharacterIterator
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {

        /**
         * A constant which indicates that there is no character at the current
         * index.
         */
        public const char DONE = '\uffff';

        /**
         * Returns a new {@code CharacterIterator} with the same properties.
         * 
         * @return a shallow copy of this character iterator.
         * 
         * @see java.lang.Cloneable
         */
        public abstract object Clone();

        /**
         * Returns the character at the current index.
         * 
         * @return the current character, or {@code DONE} if the current index is
         *         past the beginning or end of the sequence.
         */
        public abstract char Current { get; }

        /**
         * Sets the current position to the begin index and returns the character at
         * the new position.
         * 
         * @return the character at the begin index.
         */
        public abstract char First();

        /**
         * Returns the begin index.
         * 
         * @return the index of the first character of the iteration.
         */
        public abstract int BeginIndex { get; }

        /**
         * Returns the end index.
         * 
         * @return the index one past the last character of the iteration.
         */
        public abstract int EndIndex { get; }

        /**
         * Returns the current index.
         * 
         * @return the current index.
         */
        public abstract int Index { get; }

        /**
         * Sets the current position to the end index - 1 and returns the character
         * at the new position.
         * 
         * @return the character before the end index.
         */
        public abstract char Last();

        /**
         * Increments the current index and returns the character at the new index.
         * 
         * @return the character at the next index, or {@code DONE} if the next
         *         index would be past the end.
         */
        public abstract char Next();

        /**
         * Decrements the current index and returns the character at the new index.
         * 
         * @return the character at the previous index, or {@code DONE} if the
         *         previous index would be past the beginning.
         */
        public abstract char Previous();

        /**
         * Sets the current index to a new position and returns the character at the
         * new index.
         * 
         * @param location
         *            the new index that this character iterator is set to.
         * @return the character at the new index, or {@code DONE} if the index is
         *         past the end.
         * @throws IllegalArgumentException
         *         if {@code location} is less than the begin index or greater than
         *         the end index.
         */
        public abstract char SetIndex(int location);
    }
}
