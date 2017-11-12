using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The <see cref="ILanguageBreakEngine"/> interface is to be used to implement any
    /// language-specific logic for break iteration.
    /// </summary>
    internal interface ILanguageBreakEngine
    {
        /**
         * @param c A Unicode codepoint value
         * @param breakType The kind of break iterator that is wanting to make use
         *  of this engine - character, word, line, sentence
         * @return true if the engine can handle this character, false otherwise
         */
        bool Handles(int c, int breakType);

        /**
         * Implements the actual breaking logic. Find any breaks within a run in the supplied text.
         * @param text The text to break over. The iterator is left at
         * the end of the run of characters which the engine has handled.
         * @param startPos The index of the beginning of the range
         * @param endPos The index of the possible end of our range. It is possible,
         *  however, that the range ends earlier
         * @param breakType The kind of break iterator that is wanting to make use
         *  of this engine - character, word, line, sentence
         * @param foundBreaks A data structure to receive the break positions.
         * @return the number of breaks found
         */
        int FindBreaks(CharacterIterator text, int startPos, int endPos,
                int breakType, DictionaryBreakEngine.DequeI foundBreaks);
    }
}
