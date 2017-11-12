﻿using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The DictionaryMatcher interface is used to allow arbitrary "types" of
    /// back-end data structures to be used with the break iteration code.
    /// </summary>
    internal abstract class DictionaryMatcher
    {
        /**
         * Find dictionary words that match the text.
         * 
         * @param text A CharacterIterator representing the text. The iterator is
         *            left after the longest prefix match in the dictionary.
         * @param maxLength The maximum number of code units to match.
         * @param lengths An array that is filled with the lengths of words that matched.
         * @param count Filled with the number of elements output in lengths.
         * @param limit The maximum amount of words to output. Must be less than or equal to lengths.length.
         * @param values Filled with the weight values associated with the various words.
         * @return The number of characters in text that were matched.
         */
        public abstract int Matches(CharacterIterator text, int maxLength, int[] lengths,
                int[] count, int limit, int[] values);

        public int Matches(CharacterIterator text, int maxLength, int[] lengths,
                int[] count, int limit)
        {
            return Matches(text, maxLength, lengths, count, limit, null);
        }

        /**
         * @return the kind of dictionary that this matcher is using
         */
        public abstract int Type { get; }
    }
}
