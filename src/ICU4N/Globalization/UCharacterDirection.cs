using System;

namespace ICU4N.Globalization
{
    /// <summary>
    /// <see cref="Enum"/> for the CharacterDirection constants. Some constants are
    /// compatible in name <b>but not in value</b> with those defined in
    /// <see cref="Support.Text.Character"/>.
    /// </summary>
    /// <seealso cref="UCharacterDirectionExtensions"/>
    public enum UCharacterDirection
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
        /// One more than the highest normal <see cref="UCharacterDirection"/> value.
        /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
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

    /// <summary>
    /// Enumerated Unicode character linguistic direction values.
    /// Used as return results from <see cref="UChar"/>.
    /// <para/>
    /// This class is not subclassable.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.1</stable>
    public static class UCharacterDirectionExtensions 
    {
        /// <summary>
        /// Gets the name of the argument direction.
        /// </summary>
        /// <param name="dir">Direction type to retrieve name.</param>
        /// <returns>Directional name.</returns>
        /// <stable>ICU 2.1</stable>
        public static string AsString(this UCharacterDirection dir)
        {
            switch (dir)
            {
                case UCharacterDirection.LeftToRight:
                    return "Left-to-Right";
                case UCharacterDirection.RightToLeft:
                    return "Right-to-Left";
                case UCharacterDirection.EuropeanNumber:
                    return "European Number";
                case UCharacterDirection.EuropeanNumberSeparator:
                    return "European Number Separator";
                case UCharacterDirection.EuropeanNumberTerminator:
                    return "European Number Terminator";
                case UCharacterDirection.ArabicNumber:
                    return "Arabic Number";
                case UCharacterDirection.CommonNumberSeparator:
                    return "Common Number Separator";
                case UCharacterDirection.BlockSeparator:
                    return "Paragraph Separator";
                case UCharacterDirection.SegmentSeparator:
                    return "Segment Separator";
                case UCharacterDirection.WhiteSpaceNeutral:
                    return "Whitespace";
                case UCharacterDirection.OtherNeutral:
                    return "Other Neutrals";
                case UCharacterDirection.LeftToRightEmbedding:
                    return "Left-to-Right Embedding";
                case UCharacterDirection.LeftToRightOverride:
                    return "Left-to-Right Override";
                case UCharacterDirection.RightToLeftArabic:
                    return "Right-to-Left Arabic";
                case UCharacterDirection.RightToLeftEmbedding:
                    return "Right-to-Left Embedding";
                case UCharacterDirection.RightToLeftOverride:
                    return "Right-to-Left Override";
                case UCharacterDirection.PopDirectionalFormat:
                    return "Pop Directional Format";
                case UCharacterDirection.DirNonSpacingMark:
                    return "Non-Spacing Mark";
                case UCharacterDirection.BoundaryNeutral:
                    return "Boundary Neutral";
                case UCharacterDirection.FirstStrongIsolate:
                    return "First Strong Isolate";
                case UCharacterDirection.LeftToRightIsolate:
                    return "Left-to-Right Isolate";
                case UCharacterDirection.RightToLeftIsolate:
                    return "Right-to-Left Isolate";
                case UCharacterDirection.PopDirectionalIsolate:
                    return "Pop Directional Isolate";
                default:
                    return "Unassigned";
            }
        }

        /// <summary>
        /// Converts a <see cref="UCharacterDirection"/> to an <see cref="int"/>.
        /// Same as <c>(int)<paramref name="characterDirection"/></c>.
        /// </summary>
        /// <param name="characterDirection">This <see cref="UCharacterDirection"/>.</param>
        /// <returns>This direction as <see cref="int"/>.</returns>
        public static int ToInt32(this UCharacterDirection characterDirection)
        {
            return (int)characterDirection;
        }
    }
}
