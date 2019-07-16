namespace ICU4N.Text
{
    /// <summary>
    /// Interface that defines an API for forward-only iteration
    /// on text objects.
    /// </summary>
    /// <remarks>
    /// This is a minimal interface for iteration without random access
    /// or backwards iteration. It is especially useful for wrapping
    /// streams with converters into an object for collation or
    /// normalization.
    /// <para/>
    /// Characters can be accessed in two ways: as code units or as
    /// code points.
    /// Unicode code points are 21-bit integers and are the scalar values
    /// of Unicode characters. ICU uses the type <see cref="int"/> for them.
    /// Unicode code units are the storage units of a given
    /// Unicode/UCS Transformation Format (a character encoding scheme).
    /// With UTF-16, all code points can be represented with either one
    /// or two code units ("surrogates").
    /// String storage is typically based on code units, while properties
    /// of characters are typically determined using code point values.
    /// Some processes may be designed to work with sequences of code units,
    /// or it may be known that all characters that are important to an
    /// algorithm can be represented with single code units.
    /// Other processes will need to use the code point access functions.
    /// <para/>
    /// <see cref="IUForwardCharacterIterator"/> provides <see cref="MoveNext()"/> to access
    /// a code unit and advance an internal position into the text object,
    /// similar to a <c>return text[position++]</c>.
    /// It provides <see cref="MoveNextCodePoint()"/> to access a code point and advance an internal
    /// position.
    /// <para/>
    /// <see cref="MoveNextCodePoint()"/> assumes that the current position is that of
    /// the beginning of a code point, i.e., of its first code unit.
    /// After <see cref="MoveNextCodePoint()"/>, this will be true again.
    /// In general, access to code units and code points in the same
    /// iteration loop should not be mixed. In UTF-16, if the current position
    /// is on a second code unit (Low Surrogate), then only that code unit
    /// is returned even by <see cref="MoveNextCodePoint()"/>.
    /// <para/>
    /// Usage:
    /// <code>
    /// public void Function1(IUForwardCharacterIterator it)
    /// {
    ///     int c;
    ///     while ((c = it.Next()) != UForwardCharacterIterator.DONE)
    ///     {
    ///         // use c
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <stable>ICU 2.4</stable>
    public interface IUForwardCharacterIterator
    {
        /// <summary>
        /// Returns the UTF16 code unit at index, and increments to the next
        /// code unit (post-increment semantics).  If index is out of
        /// range, <see cref="UForwardCharacterIterator.Done"/> is returned, and the iterator is reset to the limit
        /// of the text.
        /// </summary>
        /// <returns>The next UTF16 code unit, or <see cref="UForwardCharacterIterator.Done"/> if the index is at the limit of the text.</returns>
        /// <stable>ICU 2.4</stable>
        int MoveNext();

        /// <summary>
        /// Returns the code point at index, and increments to the next code
        /// point (post-increment semantics).  If index does not point to a
        /// valid surrogate pair, the behavior is the same as
        /// <see cref="MoveNext()"/>.  Otherwise the iterator is incremented past
        /// the surrogate pair, and the code point represented by the pair
        /// is returned.
        /// </summary>
        /// <returns>The next codepoint in text, or <see cref="UForwardCharacterIterator.Done"/> if the index is at
        /// the limit of the text.</returns>
        /// <stable>ICU 2.4</stable>
        int MoveNextCodePoint();
    }

    /// <summary>
    /// <see cref="IUForwardCharacterIterator"/> constants.
    /// </summary>
    public static class UForwardCharacterIterator
    {
        /// <summary>
        /// Indicator that we have reached the ends of the UTF16 text.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public static readonly int Done = -1;
    }
}
