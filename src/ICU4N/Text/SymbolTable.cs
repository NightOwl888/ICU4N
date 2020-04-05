using J2N.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// An interface that defines both lookup protocol and parsing of
    /// symbolic names.
    /// </summary>
    /// <remarks>
    /// This interface is used by <see cref="UnicodeSet"/> to resolve $Variable style
    /// references that appear in set patterns.  RBBI and Transliteration
    /// both independently implement this interface.
    /// <para/>
    /// A symbol table maintains two kinds of mappings.  The first is
    /// between symbolic names and their values.  For example, if the
    /// variable with the name "start" is set to the value "alpha"
    /// (perhaps, though not necessarily, through an expression such as
    /// "$start=alpha"), then the call lookup("start") will return the
    /// char[] array ['a', 'l', 'p', 'h', 'a'].
    /// <para/>
    /// The second kind of mapping is between character values and
    /// <see cref="IUnicodeMatcher"/> objects.  This is used by RuleBasedTransliterator,
    /// which uses characters in the private use area to represent objects
    /// such as <see cref="UnicodeSet"/>s.  If U+E015 is mapped to the <see cref="UnicodeSet"/> [a-z],
    /// then lookupMatcher(0xE015) will return the <see cref="UnicodeSet"/> [a-z].
    /// <para/>
    /// Finally, a symbol table defines parsing behavior for symbolic
    /// names.  All symbolic names start with the <see cref="SymbolTable.SymbolReference"/> character.
    /// When a parser encounters this character, it calls <see cref="ParseReference(string, ParsePosition, int)"/>
    /// with the position immediately following the <see cref="SymbolTable.SymbolReference"/>.  The symbol
    /// table parses the name, if there is one, and returns it.
    /// </remarks>
    /// <stable>ICU 2.8</stable>
    public interface ISymbolTable
    {
        /// <summary>
        /// Lookup the characters associated with this string and return it.
        /// Return <c>null</c> if no such name exists.  The resultant
        /// array may have length zero.
        /// </summary>
        /// <param name="s">The symbolic name to lookup.</param>
        /// <returns>A char array containing the name's value, or null if
        /// there is no mapping for s.</returns>
        /// <stable>ICU 2.8</stable>
        char[] Lookup(string s);

        /// <summary>
        /// Lookup the <see cref="IUnicodeMatcher"/> associated with the given character, and
        /// return it.  Return <c>null</c> if not found.
        /// </summary>
        /// <param name="ch">A 32-bit code point from 0 to 0x10FFFF inclusive.</param>
        /// <returns>The <see cref="IUnicodeMatcher"/> object represented by the given
        /// character, or null if there is no mapping for ch.</returns>
        /// <stable>ICU 2.8</stable>
        IUnicodeMatcher LookupMatcher(int ch);

        /// <summary>
        /// Parse a symbol reference name from the given string, starting
        /// at the given position.  If no valid symbol reference name is
        /// found, return null and leave <paramref name="pos"/> unchanged.  That is, if the
        /// character at <paramref name="pos"/> cannot start a name, or if <paramref name="pos"/> is at or after
        /// text.Length, then return null.  This indicates an isolated
        /// <see cref="SymbolTable.SymbolReference"/> character.
        /// </summary>
        /// <param name="text">The text to parse for the name.</param>
        /// <param name="pos">Position on entry, the index of the first character to parse.
        /// This is the character following the <see cref="SymbolTable.SymbolReference"/> character.  On
        /// exit, the index after the last parsed character.  If the parse
        /// failed, pos is unchanged on exit.
        /// </param>
        /// <param name="limit">The index after the last character to be parsed.</param>
        /// <returns>The parsed name, or null if there is no valid symbolic name at the given position.</returns>
        /// <stable>ICU 2.8</stable>
        string ParseReference(string text, ParsePosition pos, int limit);
    }

    /// <summary>
    /// <see cref="ISymbolTable"/> constants.
    /// </summary>
    internal static class SymbolTable
    {
        /// <summary>
        /// The character preceding a symbol reference name.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        internal const char SymbolReference = '$';
    }
}
