using System;
using System.Globalization;

namespace ICU4N.Lang
{
    /// <summary>
    /// Extension methods for <see cref="UnicodeCategory"/>.
    /// </summary>
    public static class UnicodeCategoryConvert
    {
        /// <summary>
        /// Converts a <see cref="UnicodeCategory"/> to the integer value that is used by
        /// ICU4N, which differs from the numeric value of the <see cref="UnicodeCategory"/> enum
        /// that is defined in System.Globalization.
        /// </summary>
        /// <param name="unicodeCategory">A <see cref="UnicodeCategory"/> value.</param>
        /// <returns>The integer associated with the <see cref="UnicodeCategory"/> for comparisions in ICU4N.</returns>
        public static int ToIcuValue(this UnicodeCategory unicodeCategory)
        {
            switch (unicodeCategory)
            {
                case UnicodeCategory.OtherNotAssigned: return 0; // UNASSIGNED / GENERAL_OTHER_TYPES
                case UnicodeCategory.UppercaseLetter: return 1;
                case UnicodeCategory.LowercaseLetter: return 2;
                case UnicodeCategory.TitlecaseLetter: return 3;
                case UnicodeCategory.ModifierLetter: return 4;
                case UnicodeCategory.OtherLetter: return 5;
                case UnicodeCategory.NonSpacingMark: return 6;
                case UnicodeCategory.EnclosingMark: return 7;
                case UnicodeCategory.SpacingCombiningMark: return 8;
                case UnicodeCategory.DecimalDigitNumber: return 9;
                case UnicodeCategory.LetterNumber: return 10;
                case UnicodeCategory.OtherNumber: return 11;
                case UnicodeCategory.SpaceSeparator: return 12;
                case UnicodeCategory.LineSeparator: return 13;
                case UnicodeCategory.ParagraphSeparator: return 14;
                case UnicodeCategory.Control: return 15;
                case UnicodeCategory.Format: return 16;
                case UnicodeCategory.PrivateUse: return 17;
                case UnicodeCategory.Surrogate: return 18;
                case UnicodeCategory.DashPunctuation: return 19;
                case UnicodeCategory.OpenPunctuation: return 20;
                case UnicodeCategory.ClosePunctuation: return 21;
                case UnicodeCategory.ConnectorPunctuation: return 22;
                case UnicodeCategory.OtherPunctuation: return 23;
                case UnicodeCategory.MathSymbol: return 24;
                case UnicodeCategory.CurrencySymbol: return 25;
                case UnicodeCategory.ModifierSymbol: return 26;
                case UnicodeCategory.OtherSymbol: return 27;
                case UnicodeCategory.InitialQuotePunctuation: return 28;
                case UnicodeCategory.FinalQuotePunctuation: return 29;
                default: return 0; // UNASSIGNED / GENERAL_OTHER_TYPES
            }
        }

        /// <summary>
        /// Converts an integer value used by ICU4N to the corresponding <see cref="UnicodeCategory"/> value.
        /// </summary>
        /// <param name="value">An integer representing an ICU4J UCategory constant.</param>
        /// <returns>The <see cref="UnicodeCategory"/> representing the passed in <paramref name="value"/>.</returns>
        public static UnicodeCategory FromIcuValue(int value)
        {
            switch (value)
            {
                case 0: return UnicodeCategory.OtherNotAssigned; // UNASSIGNED / GENERAL_OTHER_TYPES
                case 1: return UnicodeCategory.UppercaseLetter;
                case 2: return UnicodeCategory.LowercaseLetter;
                case 3: return UnicodeCategory.TitlecaseLetter;
                case 4: return UnicodeCategory.ModifierLetter;
                case 5: return UnicodeCategory.OtherLetter;
                case 6: return UnicodeCategory.NonSpacingMark;
                case 7: return UnicodeCategory.EnclosingMark;
                case 8: return UnicodeCategory.SpacingCombiningMark;
                case 9: return UnicodeCategory.DecimalDigitNumber;
                case 10: return UnicodeCategory.LetterNumber;
                case 11: return UnicodeCategory.OtherNumber;
                case 12: return UnicodeCategory.SpaceSeparator;
                case 13: return UnicodeCategory.LineSeparator;
                case 14: return UnicodeCategory.ParagraphSeparator;
                case 15: return UnicodeCategory.Control;
                case 16: return UnicodeCategory.Format;
                case 17: return UnicodeCategory.PrivateUse;
                case 18: return UnicodeCategory.Surrogate;
                case 19: return UnicodeCategory.DashPunctuation;
                case 20: return UnicodeCategory.OpenPunctuation;
                case 21: return UnicodeCategory.ClosePunctuation;
                case 22: return UnicodeCategory.ConnectorPunctuation;
                case 23: return UnicodeCategory.OtherPunctuation;
                case 24: return UnicodeCategory.MathSymbol;
                case 25: return UnicodeCategory.CurrencySymbol;
                case 26: return UnicodeCategory.ModifierSymbol;
                case 27: return UnicodeCategory.OtherSymbol;
                case 28: return UnicodeCategory.InitialQuotePunctuation;
                case 29: return UnicodeCategory.FinalQuotePunctuation;
                default: return UnicodeCategory.OtherNotAssigned; // UNASSIGNED / GENERAL_OTHER_TYPES
            }
        }
    }


    // ICU4N TODO: API - revert back to this enum so we are decoupled from the .NET framework
    // and we have the correct documentation
    //public enum ECharacterCategory : byte
    //{
    //    /**
    //     * Unassigned character type
    //     * @stable ICU 2.1
    //     */
    //    Unassigned = 0,

    //    /**
    //     * Character type Cn
    //     * Not Assigned (no characters in [UnicodeData.txt] have this property)
    //     * @stable ICU 2.6
    //     */
    //    GeneralOtherTypes = 0,

    //    /**
    //     * Character type Lu
    //     * @stable ICU 2.1
    //     */
    //    UppercaseLetter = 1,

    //    /**
    //     * Character type Ll
    //     * @stable ICU 2.1
    //     */
    //    LowercaseLetter = 2,

    //    /**
    //     * Character type Lt
    //     * @stable ICU 2.1
    //     */

    //    TitleCaseLetter = 3,

    //    /**
    //     * Character type Lm
    //     * @stable ICU 2.1
    //     */
    //    ModifierLetter = 4,

    //    /**
    //     * Character type Lo
    //     * @stable ICU 2.1
    //     */
    //    OtherLetter = 5,

    //    /**
    //     * Character type Mn
    //     * @stable ICU 2.1
    //     */
    //    NonSpacingMark = 6,

    //    /**
    //     * Character type Me
    //     * @stable ICU 2.1
    //     */
    //    EnclosingMark = 7,

    //    /**
    //     * Character type Mc
    //     * @stable ICU 2.1
    //     */
    //    CombiningSpacingMark = 8,

    //    /**
    //     * Character type Nd
    //     * @stable ICU 2.1
    //     */
    //    DecimalDigitNumber = 9,

    //    /**
    //     * Character type Nl
    //     * @stable ICU 2.1
    //     */
    //    LetterNumber = 10,

    //    /**
    //     * Character type No
    //     * @stable ICU 2.1
    //     */
    //    OtherNumber = 11,

    //    /**
    //     * Character type Zs
    //     * @stable ICU 2.1
    //     */
    //    SpaceSeparator = 12,

    //    /**
    //     * Character type Zl
    //     * @stable ICU 2.1
    //     */
    //    LineSeparator = 13,

    //    /**
    //     * Character type Zp
    //     * @stable ICU 2.1
    //     */
    //    ParagraphSeparator = 14,

    //    /**
    //     * Character type Cc
    //     * @stable ICU 2.1
    //     */
    //    Control = 15,

    //    /**
    //     * Character type Cf
    //     * @stable ICU 2.1
    //     */
    //    Format = 16,

    //    /**
    //     * Character type Co
    //     * @stable ICU 2.1
    //     */
    //    PrivateUse = 17,

    //    /**
    //     * Character type Cs
    //     * @stable ICU 2.1
    //     */
    //    Surrogate = 18,

    //    /**
    //     * Character type Pd
    //     * @stable ICU 2.1
    //     */
    //    DashPunctuation = 19,

    //    /**
    //     * Character type Ps
    //     * @stable ICU 2.1
    //     */
    //    StartPunctuation = 20,

    //    /**
    //     * Character type Pe
    //     * @stable ICU 2.1
    //     */
    //    EndPunctuation = 21,

    //    /**
    //     * Character type Pc
    //     * @stable ICU 2.1
    //     */
    //    ConnectorPunctuation = 22,

    //    /**
    //     * Character type Po
    //     * @stable ICU 2.1
    //     */
    //    OtherPunctuation = 23,

    //    /**
    //     * Character type Sm
    //     * @stable ICU 2.1
    //     */
    //    MathSymbol = 24,

    //    /**
    //     * Character type Sc
    //     * @stable ICU 2.1
    //     */
    //    CurrencySymbol = 25,

    //    /**
    //     * Character type Sk
    //     * @stable ICU 2.1
    //     */
    //    ModifierSymbol = 26,

    //    /**
    //     * Character type So
    //     * @stable ICU 2.1
    //     */
    //    OtherSymbol = 27,

    //    /**
    //     * Character type Pi
    //     * @see #INITIAL_QUOTE_PUNCTUATION
    //     * @stable ICU 2.1
    //     */
    //    InitialPunctuation = 28,

    //    /**
    //     * Character type Pi
    //     * This name is compatible with java.lang.Character's name for this type.
    //     * @see #INITIAL_PUNCTUATION
    //     * @stable ICU 2.8
    //     */
    //    InitialQuotePunctuation = 28,

    //    /**
    //     * Character type Pf
    //     * @see #FINAL_QUOTE_PUNCTUATION
    //     * @stable ICU 2.1
    //     */
    //    FinalPunctuation = 29,

    //    /**
    //     * Character type Pf
    //     * This name is compatible with java.lang.Character's name for this type.
    //     * @see #FINAL_PUNCTUATION
    //     * @stable ICU 2.8
    //     */
    //    FinalQuotePunctuation = 29,

    //    /**
    //     * One more than the highest normal ECharacterCategory value.
    //     * This numeric value is stable (will not change), see
    //     * http://www.unicode.org/policies/stability_policy.html#Property_Value
    //     * @stable ICU 2.1
    //     */
    //    CharCategoryCount = 30
    //}

    /// <summary>
    /// <see cref="Enum"/> for the CharacterDirection constants. Some constants are
    /// compatible in name <b>but not in value</b> with those defined in
    /// <see cref="Support.Text.Character"/>.
    /// </summary>
    /// <see cref="UCharacterDirection"/>
    public enum UnicodeDirection 
    {
        /// <summary>
        /// Directional type L
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LeftToRight = 0,

        /// <summary>
        /// Synonym of <see cref="LeftToRight"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityLeftToRight = LeftToRight,

        /// <summary>
        /// Directional type R
        /// </summary>
        /// <stable>ICU 2.1</stable>
        RightToLeft = 1,

        /// <summary>
        /// Synonym of <see cref="RightToLeft"/>
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityRightToLeft = RightToLeft,

        /// <summary>
        /// Directional type EN
        /// </summary>
        /// <stable>ICU 2.1</stable>
        EuropeanNumber = 2,

        /// <summary>
        /// Synonym of <see cref="EuropeanNumber"/>
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityEuropeanNumber = EuropeanNumber,

        /// <summary>
        /// Directional type ES
        /// </summary>
        /// <stable>ICU 2.1</stable>
        EuropeanNumberSeparator = 3,

        /// <summary>
        /// Synonym of <see cref="EuropeanNumberSeparator"/>
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityEuropeanNumberSeparator = EuropeanNumberSeparator,

        /// <summary>
        /// Directional type ET
        /// </summary>
        /// <stable>ICU 2.1</stable>
        EuropeanNumberTerminator = 4,

        /// <summary>
        /// Synonym of <see cref="EuropeanNumberTerminator"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityEuropeanNumberTerminator = EuropeanNumberTerminator,

        /// <summary>
        /// Directional type AN
        /// </summary>
        /// <stable>ICU 2.1</stable>
        ArabicNumber = 5,

        /// <summary>
        /// Synonym of <see cref="ArabicNumber"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityArabicNumber = ArabicNumber,

        /// <summary>
        /// Directional type CS
        /// </summary>
        /// <stable>ICU 2.1</stable>
        CommonNumberSeparator = 6,

        /// <summary>
        /// Synonym of <see cref="CommonNumberSeparator"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityCommonNumberSeparator = (byte)CommonNumberSeparator,

        /// <summary>
        /// Directional type B
        /// </summary>
        /// <stable>ICU 2.1</stable>
        BlockSeparator = 7,

        /// <summary>
        /// Synonym of <see cref="BlockSeparator"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityParagraphSeparator = BlockSeparator,

        /// <summary>
        /// Directional type S
        /// </summary>
        /// <stable>ICU 2.1</stable>
        SegmentSeparator = 8,

        /// <summary>
        /// Synonym of <see cref="SegmentSeparator"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalitySegmentSeparator = SegmentSeparator,

        /// <summary>
        /// Directional type WS
        /// </summary>
        /// <stable>ICU 2.1</stable>
        WhiteSpaceNeutral = 9,

        /// <summary>
        /// Synonym of <see cref="WhiteSpaceNeutral"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityWhiteSpace = WhiteSpaceNeutral,

        /// <summary>
        /// Directional type ON
        /// </summary>
        /// <stable>ICU 2.1</stable>
        OtherNeutral = 10,

        /// <summary>
        /// Synonym of <see cref="OtherNeutral"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityOtherNeutrals = OtherNeutral,

        /// <summary>
        /// Directional type LRE
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LeftToRightEmbedding = 11,

        /// <summary>
        /// Synonym of <see cref="LeftToRightEmbedding"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityLeftToRightEmbedding = LeftToRightEmbedding,

        /// <summary>
        /// Directional type LRO
        /// </summary>
        /// <stable>ICU 2.1</stable>
        LeftToRightOverride = 12,

        /// <summary>
        /// Synonym of <see cref="LeftToRightOverride"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityLeftToRightOverride = LeftToRightOverride,

        /// <summary>
        /// Directional type AL
        /// </summary>
        /// <stable>ICU 2.1</stable>
        RightToLeftArabic = 13,

        /// <summary>
        /// Synonym of <see cref="RightToLeftArabic"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityRightToLeftArabic = RightToLeftArabic,

        /// <summary>
        /// Directional type RLE
        /// </summary>
        /// <stable>ICU 2.1</stable>
        RightToLeftEmbedding = 14,

        /// <summary>
        /// Synonym of <see cref="RightToLeftEmbedding"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityRightToLeftEmbedding = (byte)RightToLeftEmbedding,

        /// <summary>
        /// Directional type RLO
        /// </summary>
        /// <stable>ICU 2.1</stable>
        RightToLeftOverride = 15,

        /// <summary>
        /// Synonym of <see cref="RightToLeftOverride"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityRightToLeftOverride = (byte)RightToLeftOverride,

        /// <summary>
        /// Directional type PDF
        /// </summary>
        /// <stable>ICU 2.1</stable>
        PopDirectionalFormat = 16,

        /// <summary>
        /// Synonym of <see cref="PopDirectionalFormat"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityPopDirectionalFormat = (byte)PopDirectionalFormat,

        /// <summary>
        /// Directional type NSM
        /// </summary>
        /// <stable>ICU 2.1</stable>
        DirNonSpacingMark = 17,

        /// <summary>
        /// Synonym of <see cref="DirNonSpacingMark"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityNonSpacingMark = (byte)DirNonSpacingMark,

        /// <summary>
        /// Directional type BN
        /// </summary>
        /// <stable>ICU 2.1</stable>
        BoundaryNeutral = 18,

        /// <summary>
        /// Synonym of <see cref="BoundaryNeutral"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityBoundaryNeutral = (byte)BoundaryNeutral,

        /// <summary>
        /// Directional type FSI
        /// </summary>
        /// <stable>ICU 52</stable>
        FirstStrongIsolate = 19,

        /// <summary>
        /// Directional type LRI
        /// </summary>
        /// <stable>ICU 52</stable>
        LeftToRightIsolate = 20,

        /// <summary>
        /// Directional type RLI
        /// </summary>
        /// <stable>ICU 52</stable>
        RightToLeftIsolate = 21,

        /// <summary>
        /// Directional type PDI
        /// </summary>
        /// <stable>ICU 52</stable>
        PopDirectionalIsolate = 22,

        /// <summary>
        /// One more than the highest normal <see cref="UnicodeDirection"/> value.
        /// The highest value is available via <see cref="UCharacter.GetIntPropertyMaxValue(UProperty)"/>
        /// with parameter <see cref="UProperty.BiDi_Class"/>.
        /// </summary>
        /// <stable>ICU 52</stable>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        CharDirectionCount = 23,

        /// <summary>
        /// Undefined bidirectional character type. Undefined <see cref="char"/>
        /// values have undefined directionality in the Unicode specification.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        DirectionalityUndefined = -1,
    }
}
