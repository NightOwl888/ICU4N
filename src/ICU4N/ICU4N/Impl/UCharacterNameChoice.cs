namespace ICU4N.Impl
{
    public enum UCharacterNameChoice // ICU4N TODO: API Rename to follow .NET Conventions
    {
        // public variables =============================================

        UNICODE_CHAR_NAME = 0,
        OBSOLETE_UNUSED_UNICODE_10_CHAR_NAME = 1,
        EXTENDED_CHAR_NAME = 2,
        /* Corrected name from NameAliases.txt. */
        CHAR_NAME_ALIAS = 3,
        CHAR_NAME_CHOICE_COUNT = 4,
        ISO_COMMENT_ = CHAR_NAME_CHOICE_COUNT,
    }
}
