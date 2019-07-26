namespace ICU4N.Globalization
{
    /// <summary>
    /// Option values for case folding
    /// </summary>
    public enum FoldCase
    {
        /// <icu/>
        /// <summary>
        /// Option value for case folding: use default mappings defined in
        /// CaseFolding.txt.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Default = 0x0000, // ICU4N specific - removed FOLD_CASE_ because it is redundant

        /// <icu/>
        /// <summary>
        /// Option value for case folding:
        /// Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
        /// and dotless i appropriately for Turkic languages (tr, az).
        /// </summary>
        /// <remarks>
        /// Before Unicode 3.2, CaseFolding.txt contains mappings marked with 'I' that
        /// are to be included for default mappings and
        /// excluded for the Turkic-specific mappings.
        /// <para/>
        /// Unicode 3.2 CaseFolding.txt instead contains mappings marked with 'T' that
        /// are to be excluded for default mappings and
        /// included for the Turkic-specific mappings.
        /// </remarks>
        /// <stable>ICU 2.6</stable>
        ExcludeSpecialI = 0x0001 // ICU4N specific - removed FOLD_CASE_ because it is redundant
    }
}
