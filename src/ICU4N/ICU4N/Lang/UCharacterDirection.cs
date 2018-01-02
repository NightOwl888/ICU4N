namespace ICU4N.Lang // ICU4N TODO: Move to Globalization namespace
{
    /// <summary>
    /// Enumerated Unicode character linguistic direction values.
    /// Used as return results from <see cref="UCharacter"/>.
    /// <para/>
    /// This class is not subclassable.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.1</stable>
    public static class UCharacterDirection // ICU4N TODO: API Make into extension methods for enum UCharacterDirection
    {
        /// <summary>
        /// Gets the name of the argument direction.
        /// </summary>
        /// <param name="dir">Direction type to retrieve name.</param>
        /// <returns>Directional name.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ToString(UnicodeDirection dir)
        {
            switch (dir)
            {
                case UnicodeDirection.LeftToRight:
                    return "Left-to-Right";
                case UnicodeDirection.RightToLeft:
                    return "Right-to-Left";
                case UnicodeDirection.EuropeanNumber:
                    return "European Number";
                case UnicodeDirection.EuropeanNumberSeparator:
                    return "European Number Separator";
                case UnicodeDirection.EuropeanNumberTerminator:
                    return "European Number Terminator";
                case UnicodeDirection.ArabicNumber:
                    return "Arabic Number";
                case UnicodeDirection.CommonNumberSeparator:
                    return "Common Number Separator";
                case UnicodeDirection.BlockSeparator:
                    return "Paragraph Separator";
                case UnicodeDirection.SegmentSeparator:
                    return "Segment Separator";
                case UnicodeDirection.WhiteSpaceNeutral:
                    return "Whitespace";
                case UnicodeDirection.OtherNeutral:
                    return "Other Neutrals";
                case UnicodeDirection.LeftToRightEmbedding:
                    return "Left-to-Right Embedding";
                case UnicodeDirection.LeftToRightOverride:
                    return "Left-to-Right Override";
                case UnicodeDirection.RightToLeftArabic:
                    return "Right-to-Left Arabic";
                case UnicodeDirection.RightToLeftEmbedding:
                    return "Right-to-Left Embedding";
                case UnicodeDirection.RightToLeftOverride:
                    return "Right-to-Left Override";
                case UnicodeDirection.PopDirectionalFormat:
                    return "Pop Directional Format";
                case UnicodeDirection.DirNonSpacingMark:
                    return "Non-Spacing Mark";
                case UnicodeDirection.BoundaryNeutral:
                    return "Boundary Neutral";
                case UnicodeDirection.FirstStrongIsolate:
                    return "First Strong Isolate";
                case UnicodeDirection.LeftToRightIsolate:
                    return "Left-to-Right Isolate";
                case UnicodeDirection.RightToLeftIsolate:
                    return "Right-to-Left Isolate";
                case UnicodeDirection.PopDirectionalIsolate:
                    return "Pop Directional Isolate";
                default:
                    return "Unassigned";
            }
        }
    }
}
