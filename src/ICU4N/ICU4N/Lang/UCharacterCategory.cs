using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Lang
{
    public static class UCharacterCategory // ICU4N TODO: API Make into extension methods
    {
        /**
     * Gets the name of the argument category
     * @param category to retrieve name
     * @return category name
     * @stable ICU 2.1
     */
        public static string ToString(UnicodeCategory category)
        {
            switch (category)
            {
                case UnicodeCategory.UppercaseLetter:
                    return "Letter, Uppercase";
                case UnicodeCategory.LowercaseLetter:
                    return "Letter, Lowercase";
                case UnicodeCategory.TitlecaseLetter:
                    return "Letter, Titlecase";
                case UnicodeCategory.ModifierLetter:
                    return "Letter, Modifier";
                case UnicodeCategory.OtherLetter:
                    return "Letter, Other";
                case UnicodeCategory.NonSpacingMark:
                    return "Mark, Non-Spacing";
                case UnicodeCategory.EnclosingMark:
                    return "Mark, Enclosing";
                case UnicodeCategory.SpacingCombiningMark:
                    return "Mark, Spacing Combining";
                case UnicodeCategory.DecimalDigitNumber:
                    return "Number, Decimal Digit";
                case UnicodeCategory.LetterNumber:
                    return "Number, Letter";
                case UnicodeCategory.OtherNumber:
                    return "Number, Other";
                case UnicodeCategory.SpaceSeparator:
                    return "Separator, Space";
                case UnicodeCategory.LineSeparator:
                    return "Separator, Line";
                case UnicodeCategory.ParagraphSeparator:
                    return "Separator, Paragraph";
                case UnicodeCategory.Control:
                    return "Other, Control";
                case UnicodeCategory.Format:
                    return "Other, Format";
                case UnicodeCategory.PrivateUse:
                    return "Other, Private Use";
                case UnicodeCategory.Surrogate:
                    return "Other, Surrogate";
                case UnicodeCategory.DashPunctuation:
                    return "Punctuation, Dash";
                case UnicodeCategory.OpenPunctuation:
                    return "Punctuation, Open";
                case UnicodeCategory.ClosePunctuation:
                    return "Punctuation, Close";
                case UnicodeCategory.ConnectorPunctuation:
                    return "Punctuation, Connector";
                case UnicodeCategory.OtherPunctuation:
                    return "Punctuation, Other";
                case UnicodeCategory.MathSymbol:
                    return "Symbol, Math";
                case UnicodeCategory.CurrencySymbol:
                    return "Symbol, Currency";
                case UnicodeCategory.ModifierSymbol:
                    return "Symbol, Modifier";
                case UnicodeCategory.OtherSymbol:
                    return "Symbol, Other";
                case UnicodeCategory.InitialQuotePunctuation:
                    return "Punctuation, Initial quote";
                case UnicodeCategory.FinalQuotePunctuation:
                    return "Punctuation, Final quote";
                default:
                    return "Unassigned";
            }
        }

        public static readonly int CHAR_CATEGORY_COUNT;

        static UCharacterCategory()
        {
            CHAR_CATEGORY_COUNT = Enum.GetNames(typeof(UnicodeCategory)).Length;
        }


        //public static string ToString(ECharacterCategory category)
        //{
        //    switch (category)
        //    {
        //        case ECharacterCategory.UppercaseLetter:
        //            return "Letter, Uppercase";
        //        case ECharacterCategory.LowercaseLetter:
        //            return "Letter, Lowercase";
        //        case ECharacterCategory.TitleCaseLetter:
        //            return "Letter, Titlecase";
        //        case ECharacterCategory.ModifierLetter:
        //            return "Letter, Modifier";
        //        case ECharacterCategory.OtherLetter:
        //            return "Letter, Other";
        //        case ECharacterCategory.NonSpacingMark:
        //            return "Mark, Non-Spacing";
        //        case ECharacterCategory.EnclosingMark:
        //            return "Mark, Enclosing";
        //        case ECharacterCategory.CombiningSpacingMark:
        //            return "Mark, Spacing Combining";
        //        case ECharacterCategory.DecimalDigitNumber:
        //            return "Number, Decimal Digit";
        //        case ECharacterCategory.LetterNumber:
        //            return "Number, Letter";
        //        case ECharacterCategory.OtherNumber:
        //            return "Number, Other";
        //        case ECharacterCategory.SpaceSeparator:
        //            return "Separator, Space";
        //        case ECharacterCategory.LineSeparator:
        //            return "Separator, Line";
        //        case ECharacterCategory.ParagraphSeparator:
        //            return "Separator, Paragraph";
        //        case ECharacterCategory.Control:
        //            return "Other, Control";
        //        case ECharacterCategory.Format:
        //            return "Other, Format";
        //        case ECharacterCategory.PrivateUse:
        //            return "Other, Private Use";
        //        case ECharacterCategory.Surrogate:
        //            return "Other, Surrogate";
        //        case ECharacterCategory.DashPunctuation:
        //            return "Punctuation, Dash";
        //        case ECharacterCategory.StartPunctuation:
        //            return "Punctuation, Open";
        //        case ECharacterCategory.EndPunctuation:
        //            return "Punctuation, Close";
        //        case ECharacterCategory.ConnectorPunctuation:
        //            return "Punctuation, Connector";
        //        case ECharacterCategory.OtherPunctuation:
        //            return "Punctuation, Other";
        //        case ECharacterCategory.MathSymbol:
        //            return "Symbol, Math";
        //        case ECharacterCategory.CurrencySymbol:
        //            return "Symbol, Currency";
        //        case ECharacterCategory.ModifierSymbol:
        //            return "Symbol, Modifier";
        //        case ECharacterCategory.OtherSymbol:
        //            return "Symbol, Other";
        //        case ECharacterCategory.InitialPunctuation:
        //            return "Punctuation, Initial quote";
        //        case ECharacterCategory.FinalPunctuation:
        //            return "Punctuation, Final quote";
        //        default:
        //            return "Unassigned";
        //    }
        //}
    }
}
