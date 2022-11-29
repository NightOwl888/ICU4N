using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Used by RBNF to leniently parse a string.
    /// </summary>
    [Obsolete("ICU 54")]
    internal interface IRbnfLenientScanner
    {
        /// <summary>
        /// Returns <c>true</c> if a string consists entirely of ignorable
        /// characters.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns><c>true</c> if the string is empty or consists entirely of
        /// characters that are ignorable.</returns>
        [Obsolete("ICU 54")]
        bool AllIgnorable(string s);

        /// <summary>
        /// Matches characters in a string against a prefix and return
        /// the number of chars that matched, or 0 if no match.  Only
        /// primary-order differences are significant in determining
        /// whether there's a match.  This means that the returned
        /// value need not be the same as the length of the prefix.
        /// </summary>
        /// <param name="str">The string being tested.</param>
        /// <param name="prefix">The text we're hoping to see at the beginning of <paramref name="str"/>.</param>
        /// <returns>The number of characters in <paramref name="str"/> that were matched.</returns>
        [Obsolete("ICU 54")]
        int PrefixLength(string str, string prefix);

        /// <summary>
        /// Searches a string for another string.  This might use a
        /// Collator to compare strings, or just do a simple match.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="key">The string to search <paramref name="str"/> for.</param>
        /// <param name="startingAt">The index into <paramref name="str"/> where the search is to begin.</param>
        /// <returns>A two-element array of ints.  Element 0 is the position
        /// of the match, or -1 if there was no match.  Element 1 is the
        /// number of characters in <paramref name="str"/> that matched (which isn't necessarily
        /// the same as the length of <paramref name="key"/>).
        /// </returns>
        [Obsolete("ICU 54")]
        int[] FindText(string str, string key, int startingAt);
    }
}
