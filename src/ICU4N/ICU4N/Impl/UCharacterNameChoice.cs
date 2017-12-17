namespace ICU4N.Impl
{
    /// <summary>
    /// Enum containing selector constants for the unicode character names.
    /// Constants representing the "modern" name of a Unicode character or the name 
    /// that was defined in Unicode version 1.0, before the Unicode standard
    /// merged with ISO-10646.
    /// <para/>
    /// Arguments for <see cref="UCharacterName"/>.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>oct0600</since>
    public enum UCharacterNameChoice
    {
        // public variables =============================================

        UnicodeCharName = 0,
        ObsoleteUnusedUnicode10CharName = 1,
        ExtendedCharName = 2,
        /* Corrected name from NameAliases.txt. */
        CharNameAlias = 3,
        CharNameChoiceCount = 4,
        IsoComment = CharNameChoiceCount,
    }
}
