using ICU4N.Support.Text;

namespace ICU4N.Text
{
    public interface ISymbolTable
    {
        /**
         * Lookup the characters associated with this string and return it.
         * Return <tt>null</tt> if no such name exists.  The resultant
         * array may have length zero.
         * @param s the symbolic name to lookup
         * @return a char array containing the name's value, or null if
         * there is no mapping for s.
         * @stable ICU 2.8
         */
        char[] Lookup(string s);

        /**
         * Lookup the UnicodeMatcher associated with the given character, and
         * return it.  Return <tt>null</tt> if not found.
         * @param ch a 32-bit code point from 0 to 0x10FFFF inclusive.
         * @return the UnicodeMatcher object represented by the given
         * character, or null if there is no mapping for ch.
         * @stable ICU 2.8
         */
        IUnicodeMatcher LookupMatcher(int ch);

        /**
         * Parse a symbol reference name from the given string, starting
         * at the given position.  If no valid symbol reference name is
         * found, return null and leave pos unchanged.  That is, if the
         * character at pos cannot start a name, or if pos is at or after
         * text.length(), then return null.  This indicates an isolated
         * SYMBOL_REF character.
         * @param text the text to parse for the name
         * @param pos on entry, the index of the first character to parse.
         * This is the character following the SYMBOL_REF character.  On
         * exit, the index after the last parsed character.  If the parse
         * failed, pos is unchanged on exit.
         * @param limit the index after the last character to be parsed.
         * @return the parsed name, or null if there is no valid symbolic
         * name at the given position.
         * @stable ICU 2.8
         */
        string ParseReference(string text, ParsePosition pos, int limit);
    }

    internal static class SymbolTable
    {
        /**
         * The character preceding a symbol reference name.
         * @stable ICU 2.8
         */
        internal const char SYMBOL_REF = '$';
    }
}
