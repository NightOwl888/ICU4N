namespace ICU4N.Globalization
{
    /// <summary>
    /// Enum for the CharacterCategory constants.  These constants are
    /// compatible in name <b>but not in value</b> with those defined in
    /// <see cref="System.Globalization.UnicodeCategory"/>.
    /// </summary>
    /// <stable>ICU 3.0</stable>
    public enum UUnicodeCategory : byte
    {
        /// <summary>
        /// Unassigned character type.
        /// </summary>
        /// <seealso cref="OtherNotAssigned"/>
        /// <seealso cref="GeneralOtherTypes"/>
        /// <stable>ICU 2.1</stable>
        Unassigned = 0,

        /// <summary>
        /// Unassigned character type.
        /// This name is compatible with <see cref="System.Globalization.UnicodeCategory"/>'s name for this type.
        /// </summary>
        /// <seealso cref="Unassigned"/>
        /// <seealso cref="GeneralOtherTypes"/>
        /// <stable>ICU 60</stable>
        OtherNotAssigned = 0,

        /// <summary>
        /// Character type Cn.
        /// Not Assigned (no characters in [UnicodeData.txt] have this property)
        /// </summary>
        /// <seealso cref="Unassigned"/>
        /// <seealso cref="OtherNotAssigned"/>
        /// <stable>ICU 2.6</stable>
        GeneralOtherTypes = 0,

        /// <summary>
        /// Character type Lu.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        UppercaseLetter = 1,

        /// <summary>
        /// Character type Ll.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LowercaseLetter = 2,

        /// <summary>
        /// Character type Lt.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        TitlecaseLetter = 3,

        /// <summary>
        /// Character type Lm.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ModifierLetter = 4,

        /// <summary>
        /// Character type Lo.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OtherLetter = 5,

        /// <summary>
        /// Character type Mn.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        NonSpacingMark = 6,

        /// <summary>
        /// Character type Me.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        EnclosingMark = 7,

        /// <summary>
        /// Character type Mc.
        /// </summary>
        /// <seealso cref="SpacingCombiningMark"/>
        /// <stable>ICU 2.1</stable>
        CombiningSpacingMark = 8,

        /// <summary>
        /// Character type Mc.
        /// This name is compatible with <see cref="System.Globalization.UnicodeCategory"/>'s name for this type.
        /// </summary>
        /// <seealso cref="CombiningSpacingMark"/>
        /// <stable>ICU 2.1</stable>
        SpacingCombiningMark = 8,

        /// <summary>
        /// Character type Nd.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        DecimalDigitNumber = 9,

        /// <summary>
        /// Character type Nl.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LetterNumber = 10,

        /// <summary>
        /// Character type No.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OtherNumber = 11,

        /// <summary>
        /// Character type Zs.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        SpaceSeparator = 12,

        /// <summary>
        /// Character type Zl.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LineSeparator = 13,

        /// <summary>
        /// Character type Zp.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ParagraphSeparator = 14,

        /// <summary>
        /// Character type Cc.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        Control = 15,

        /// <summary>
        /// Character type Cf.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        Format = 16,

        /// <summary>
        /// Character type Co.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        PrivateUse = 17,

        /// <summary>
        /// Character type Cs.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        Surrogate = 18,

        /// <summary>
        /// Character type Pd.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        DashPunctuation = 19,

        /// <summary>
        /// Character type Ps.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OpenPunctuation = 20,

        /// <summary>
        /// Character type Pe.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ClosePunctuation = 21,

        /// <summary>
        /// Character type Pc.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ConnectorPunctuation = 22,

        /// <summary>
        /// Character type Po.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OtherPunctuation = 23,

        /// <summary>
        /// Character type Sm.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        MathSymbol = 24,

        /// <summary>
        /// Character type Sc.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        CurrencySymbol = 25,

        /// <summary>
        /// Character type Sk.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ModifierSymbol = 26,

        /// <summary>
        /// Character type So.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OtherSymbol = 27,

        /// <summary>
        /// Character type Pi.
        /// </summary>
        /// <seealso cref="InitialQuotePunctuation"/>
        /// <stable>ICU 2.1</stable>
        InitialPunctuation = 28,

        /// <summary>
        /// Character type Pi.
        /// This name is compatible with <see cref="System.Globalization.UnicodeCategory"/>'s name for this type.
        /// </summary>
        /// <seealso cref="InitialPunctuation"/>
        /// <stable>ICU 2.8</stable>
        InitialQuotePunctuation = 28,

        /// <summary>
        /// Character type Pf.
        /// </summary>
        /// <seealso cref="FinalQuotePunctuation"/>
        /// <stable>ICU 2.1</stable>
        FinalPunctuation = 29,

        /// <summary>
        /// Character type Pf.
        /// This name is compatible with <see cref="System.Globalization.UnicodeCategory"/>'s name for this type.
        /// </summary>
        /// <seealso cref="FinalPunctuation"/>
        /// <stable>ICU 2.8</stable>
        FinalQuotePunctuation = 29,
    }

    /// <summary>
    /// Extension methods for <see cref="UUnicodeCategory"/>.
    /// </summary>
    public static class UCharacterCategoryExtensions
    {
        /// <summary>
        /// Gets the name of the argument category.
        /// </summary>
        /// <param name="category">Category to retrieve name.</param>
        /// <returns>Category name.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N NOTE: Since ToString() cannot be changed from the default on an Enum,
        // we have renamed this method AsString().
        public static string AsString(this UUnicodeCategory category)
        {
            switch (category)
            {
                case UUnicodeCategory.UppercaseLetter:
                    return "Letter, Uppercase";
                case UUnicodeCategory.LowercaseLetter:
                    return "Letter, Lowercase";
                case UUnicodeCategory.TitlecaseLetter:
                    return "Letter, Titlecase";
                case UUnicodeCategory.ModifierLetter:
                    return "Letter, Modifier";
                case UUnicodeCategory.OtherLetter:
                    return "Letter, Other";
                case UUnicodeCategory.NonSpacingMark:
                    return "Mark, Non-Spacing";
                case UUnicodeCategory.EnclosingMark:
                    return "Mark, Enclosing";
                case UUnicodeCategory.SpacingCombiningMark:
                    return "Mark, Spacing Combining";
                case UUnicodeCategory.DecimalDigitNumber:
                    return "Number, Decimal Digit";
                case UUnicodeCategory.LetterNumber:
                    return "Number, Letter";
                case UUnicodeCategory.OtherNumber:
                    return "Number, Other";
                case UUnicodeCategory.SpaceSeparator:
                    return "Separator, Space";
                case UUnicodeCategory.LineSeparator:
                    return "Separator, Line";
                case UUnicodeCategory.ParagraphSeparator:
                    return "Separator, Paragraph";
                case UUnicodeCategory.Control:
                    return "Other, Control";
                case UUnicodeCategory.Format:
                    return "Other, Format";
                case UUnicodeCategory.PrivateUse:
                    return "Other, Private Use";
                case UUnicodeCategory.Surrogate:
                    return "Other, Surrogate";
                case UUnicodeCategory.DashPunctuation:
                    return "Punctuation, Dash";
                case UUnicodeCategory.OpenPunctuation:
                    return "Punctuation, Open";
                case UUnicodeCategory.ClosePunctuation:
                    return "Punctuation, Close";
                case UUnicodeCategory.ConnectorPunctuation:
                    return "Punctuation, Connector";
                case UUnicodeCategory.OtherPunctuation:
                    return "Punctuation, Other";
                case UUnicodeCategory.MathSymbol:
                    return "Symbol, Math";
                case UUnicodeCategory.CurrencySymbol:
                    return "Symbol, Currency";
                case UUnicodeCategory.ModifierSymbol:
                    return "Symbol, Modifier";
                case UUnicodeCategory.OtherSymbol:
                    return "Symbol, Other";
                case UUnicodeCategory.InitialQuotePunctuation:
                    return "Punctuation, Initial quote";
                case UUnicodeCategory.FinalQuotePunctuation:
                    return "Punctuation, Final quote";
                default:
                    return "Unassigned";
            }
        }

        /// <summary>
        /// Converts a <see cref="UUnicodeCategory"/> to an <see cref="int"/>.
        /// Same as <c>(int)<paramref name="characterCategory"/></c>.
        /// </summary>
        /// <param name="characterCategory">This <see cref="UUnicodeCategory"/>.</param>
        /// <returns>This category as <see cref="int"/>.</returns>
        public static int ToInt32(this UUnicodeCategory characterCategory) // ICU4N TODO: Add this extension to all main enums
        {
            return (int)characterCategory;
        }

        /// <summary>
        /// One more than the highest normal <see cref="UUnicodeCategory"/> value.
        /// This numeric value is stable (will not change), see
        /// <a href="http://www.unicode.org/policies/stability_policy.html#Property_Value">
        /// http://www.unicode.org/policies/stability_policy.html#Property_Value</a>
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int CharCategoryCount = 30;
    }
}
