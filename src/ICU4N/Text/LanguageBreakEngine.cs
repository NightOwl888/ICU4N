using ICU4N.Support.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The <see cref="ILanguageBreakEngine"/> interface is to be used to implement any
    /// language-specific logic for break iteration.
    /// </summary>
    internal interface ILanguageBreakEngine
    {
        /// <param name="c">A Unicode codepoint value.</param>
        /// <param name="breakType">The kind of break iterator that is wanting to make use
        /// of this engine - character, word, line, sentence.</param>
        /// <returns>true if the engine can handle this character, false otherwise.</returns>
        bool Handles(int c, int breakType);

        /// <summary>
        /// Implements the actual breaking logic. Find any breaks within a run in the supplied text.
        /// </summary>
        /// <param name="text">The text to break over. The iterator is left at
        /// the end of the run of characters which the engine has handled.</param>
        /// <param name="startPos">The index of the beginning of the range.</param>
        /// <param name="endPos">The index of the possible end of our range. It is possible,
        /// however, that the range ends earlier.</param>
        /// <param name="breakType">The kind of break iterator that is wanting to make use
        /// of this engine - character, word, line, sentence.</param>
        /// <param name="foundBreaks">A data structure to receive the break positions.</param>
        /// <returns>The number of breaks found.</returns>
        int FindBreaks(CharacterIterator text, int startPos, int endPos,
                int breakType, DictionaryBreakEngine.DequeI foundBreaks); // ICU4N TODO: API - make breakType into an enum (constants are on BreakIterator)
    }
}
