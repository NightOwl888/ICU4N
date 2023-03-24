namespace ICU4N.Globalization
{
    /// <summary>
    /// Specifies the culture-specific display of digits.
    /// </summary>
    /// <remarks>
    /// A <see cref="UDigitShapes"/> value specifies that no digit shape is substituted for
    /// the Unicode input, a digit shape is substituted based on context, or a native national
    /// digit shape is substituted for the input.
    /// <para/>
    /// The Arabic, Indic, and Thai languages have classical shapes for numbers that are
    /// different from the digits 0 through 9 (Unicode U+0030 through U+0039), which are
    /// most often used on computers. The application uses the DigitShapes enumeration with
    /// the <see cref="UNumberFormatInfo.DigitSubstitution"/> property to specify how to display
    /// digits U+0030 through U+0039 in the absence of other formatting information.
    /// <para/>
    /// The <see cref="UDigitShapes"/> enumeration is primarily used by applications intended for
    /// cultures that use bidirectional scripts. For example, the reading order of Arabic and Indic
    /// scripts is bidirectional.
    /// </remarks>
    // ICU4N TODO: Microsoft seems to be looking this up from locale data in ICU, but I haven't spotted
    // it in the ICU4J bundles. ar uses "Context" (0) and dz uses "NativeNational" (2), but it will take
    // a browse through the native resources to work out where they are finding that field in icu4c.
    // Note that MS uses their own compile of icu4c, so they may even be adding it.
    internal enum UDigitShapes : int 
    {
        // ICU4N TODO: Not currently supported (should it be)?
        ///// <summary>
        ///// The digit shape depends on the previous text in the same output. European digits follow Latin scripts;
        ///// Arabic-Indic digits follow Arabic text; and Thai digits follow Thai text.
        ///// </summary>
        //Context = 0x0000,        // The shape depends on the previous text in the same output.

        /// <summary>
        /// ASCII digits from 0 through 9 are always used.
        /// </summary>
        None = 0x0001,             // Gives full Unicode compatibility.

        /// <summary>
        /// The digit shape is the native equivalent of the digits from 0 through 9. ASCII digits from 0 through 9
        /// are replaced by equivalent native national digits.
        /// </summary>
        NativeNational = 0x0002    // National shapes
    }
}
