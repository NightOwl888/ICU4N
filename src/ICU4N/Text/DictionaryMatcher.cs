using ICU4N.Support.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The <see cref="DictionaryMatcher"/> interface is used to allow arbitrary "types" of
    /// back-end data structures to be used with the break iteration code.
    /// </summary>
    internal abstract class DictionaryMatcher
    {
        /// <summary>
        /// Find dictionary words that match the text.
        /// </summary>
        /// <param name="text">A <see cref="CharacterIterator"/> representing the text. The iterator is
        /// left after the longest prefix match in the dictionary.</param>
        /// <param name="maxLength">The maximum number of code units to match.</param>
        /// <param name="lengths">An array that is filled with the lengths of words that matched.</param>
        /// <param name="count">Filled with the number of elements output in lengths.</param>
        /// <param name="limit">The maximum amount of words to output. Must be less than or equal to lengths.Length.</param>
        /// <param name="values">Filled with the weight values associated with the various words.</param>
        /// <returns>The number of characters in text that were matched.</returns>
        public abstract int Matches(CharacterIterator text, int maxLength, int[] lengths,
                int[] count, int limit, int[] values);

        public int Matches(CharacterIterator text, int maxLength, int[] lengths,
                int[] count, int limit)
        {
            return Matches(text, maxLength, lengths, count, limit, null);
        }

        /// <summary>
        /// Gets the kind of dictionary that this matcher is using.
        /// </summary>
        public abstract int Type { get; }
    }
}
