namespace ICU4N.Support.Text
{
    /// <summary>
    /// An interface for the bidirectional iteration over a group of characters. The
    /// iteration starts at the begin index in the group of characters and continues
    /// to one index before the end index.
    /// </summary>
    // ICU4N TODO: API - more analysis is needed to determine the impact, but it might make sense to make this
    // more like System.Globalization.TextElementEnumerator. It seems we should be able
    // to make these MoveNext(), MovePrevious(), MoveFirst(), and MoveLast() that return
    // bool, and then use the Current property to read the char (as is typically done in .NET).
    // This would allow us to implement the regular IEnumerble<char> interface in order to support
    // LINQ.
    // Alternatively, just implement that interface here without changing this from "iterator" to "enumerator" - call Next()
    // within MoveNext() and check whether Done is returned to provide the result OR alternatively check the EndIndex against Index for the true/false
    // result of MoveNext().
    // We may be able to do something like that with BreakIterator as well.
    public abstract class CharacterIterator
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /// <summary>
        /// A constant which indicates that there is no character at the current
        /// index.
        /// </summary>
        public const char Done = '\uffff';

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
        public abstract object Clone();

        /// <summary>
        /// Returns the character at the current index, or <see cref="CharacterIterator.Done"/> if the current index is
        /// past the beginning or end of the sequence.
        /// </summary>
        public abstract char Current { get; }

        /// <summary>
        /// Sets the current position to the begin index and returns the character at
        /// the new position.
        /// </summary>
        /// <returns>The character at the begin index.</returns>
        public abstract char First();

        /// <summary>
        /// Gets the begin index. Returns the index of the first character of the iteration.
        /// </summary>
        public abstract int BeginIndex { get; }

        /// <summary>
        /// Gets the end index. Returns the index one past the last character of the iteration.
        /// </summary>
        public abstract int EndIndex { get; }

        /// <summary>
        /// Gets the current index.
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Sets the current position to the end index - 1 and returns the character
        /// at the new position.
        /// </summary>
        /// <returns>The character before the end index.</returns>
        public abstract char Last();

        /// <summary>
        /// Increments the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the next index, or <see cref="CharacterIterator.Done"/> if the next
        /// index would be past the end.</returns>
        public abstract char Next();

        /// <summary>
        /// Decrements the current index and returns the character at the new index.
        /// </summary>
        /// <returns>The character at the previous index, or <see cref="CharacterIterator.Done"/> if the
        /// previous index would be past the beginning.</returns>
        public abstract char Previous();

        /// <summary>
        /// Sets the current index to a new position and returns the character at the
        /// new index.
        /// </summary>
        /// <param name="location">The new index that this character iterator is set to.</param>
        /// <returns>The character at the new index, or <see cref="CharacterIterator.Done"/> if the index is
        /// past the end.</returns>
        /// <exception cref="System.ArgumentException">If <paramref name="location"/> is less than 
        /// the begin index or greater than the end index.</exception>
        public abstract char SetIndex(int location);
    }
}
