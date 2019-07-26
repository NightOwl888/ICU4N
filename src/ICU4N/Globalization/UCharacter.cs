using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N
{
    /// <icuenhanced cref="System.Char"/>.<icu>_usage_</icu>
    /// <summary>
    /// The <see cref="UChar"/> class provides extensions to the <see cref="System.Char"/> class.
    /// These extensions provide support for more Unicode properties.
    /// Each ICU release supports the latest version of Unicode available at that time.
    /// </summary>
    /// <remarks>
    /// Code points are represented in these API using <see cref="int"/>s. While it would be
    /// more convenient in .NET to have a separate primitive datatype for them,
    /// <see cref="int"/>s suffice in the meantime.
    /// <para/>
    /// Aside from the additions for UTF-16 support, and the updated Unicode
    /// properties, the main differences between <see cref="UChar"/> and <see cref="Char"/> are:
    /// <list type="bullet">
    ///     <item><description>
    ///         <see cref="UChar"/> is not designed to be a struct and does not have
    ///         APIs to which involves management of that single <see cref="char"/>.
    ///         These include:
    ///         <list type="bullet">
    ///             <item><description><see cref="IConvertible"/> members.</description></item>
    ///             <item><description><see cref="Char.CompareTo(char)"/>, etc.</description></item>
    ///         </list>
    ///     </description></item>
    ///     <item><description>
    ///         <see cref="Char"/>'s API uses single <see cref="char"/> values and/or <see cref="string"/>
    ///         values to cover surrogate pairs. However, this API uses single <see cref="int"/> code points, 
    ///         which can represent the entire Unicode range of values using a single value.
    ///     </description></item>
    /// </list>
    /// <para/>
    /// In addition to .NET compatibility functions, which calculate derived properties,
    /// this API provides low-level access to the Unicode Character Database.
    /// <para/>
    /// Unicode assigns each code point (not just assigned character) values for
    /// many properties.
    /// Most of them are simple boolean flags, or constants from a small enumerated list.
    /// For some properties, values are strings or other relatively more complex types.
    /// <para/>
    /// For more information see
    /// <a href="http://www.unicode/org/ucd/">"About the Unicode Character Database"</a>
    /// (http://www.unicode.org/ucd/)
    /// and the <a href="http://www.icu-project.org/userguide/properties.html">ICU
    /// User Guide chapter on Properties</a>
    /// (http://www.icu-project.org/userguide/properties.html).
    /// <para/>
    /// There are also functions that provide easy migration from C/POSIX functions. 
    /// Their use is generally discouraged because the C/POSIX
    /// standards do not define their semantics beyond the ASCII range, which means
    /// that different implementations exhibit very different behavior.
    /// Instead, Unicode properties should be used directly.
    /// <para/>
    /// There are also only a few, broad C/POSIX character classes, and they tend
    /// to be used for conflicting purposes. For example, the "isalpha()" class
    /// is sometimes used to determine word boundaries, while a more sophisticated
    /// approach would at least distinguish initial letters from continuation
    /// characters (the latter including combining marks).
    /// (In ICU, <see cref="BreakIterator"/> is the most sophisticated API for word boundaries.)
    /// Another example: There is no "istitle()" class for titlecase characters.
    /// <para/>
    /// ICU 3.4 and later provides API access for all twelve C/POSIX character classes.
    /// ICU implements them according to the Standard Recommendations in
    /// Annex C: Compatibility Properties of UTS #18 Unicode Regular Expressions
    /// (http://www.unicode.org/reports/tr18/#Compatibility_Properties).
    /// <para/>
    /// API access for C/POSIX character classes is as follows:
    /// <code>
    /// - alpha:     IsUAlphabetic(c) or HasBinaryProperty(c, UProperty.Alphabetic)
    /// - lower:     IsULower(c) or HasBinaryProperty(c, UProperty.Lowercase)
    /// - upper:     IsUUpper(c) or HasBinaryProperty(c, UProperty.Uppercase)
    /// - punct:     ((1&lt;&lt;GetType(c)) &amp; ((1&lt;&lt;ECharacterCategory.DashPunctuation)|(1&lt;&lt;CharacterCategory.StartPunctuation)|
    ///               (1&lt;&lt;CharacterCategory.EndPunctuation)|(1&lt;&lt;CharacterCategory.ConnectorPunctuation)|(1&lt;&lt;CharacterCategory.OtherPunctuation)|
    ///               (1&lt;&lt;CharacterCategory.InitialPunctuation)|(1&lt;&lt;CharacterCategory.FinalPunctuation)))!=0
    /// - digit:     IsDigit(c) or GetType(c)==CharacterCategory.DecimalDigitNumber
    /// - xdigit:    HasBinaryProperty(c, UProperty.POSIX_XDigit)
    /// - alnum:     HasBinaryProperty(c, UProperty.POSIX_Alnum)
    /// - space:     IsUWhiteSpace(c) or HasBinaryProperty(c, UProperty.White_Space)
    /// - blank:     HasBinaryProperty(c, UProperty.POSIX_Blank)
    /// - cntrl:     GetType(c)==CharacterCategory.Control
    /// - graph:     HasBinaryProperty(c, UProperty.POSIX_Graph)
    /// - print:     HasBinaryProperty(c, UProperty.POSIX_Print)
    /// </code>
    /// <para/>
    /// The C/POSIX character classes are also available in <see cref="UnicodeSet"/> patterns,
    /// using patterns like <c>[:graph:]</c> or <c>\p{graph}</c>.
    /// <para/>
    /// <icunote>There are several ICU (and .NET) whitespace functions.
    /// Comparison:
    /// <list type="bullet">
    ///     <item><term><see cref="IsUWhiteSpace(int)"/></term><description>
    ///         Unicode White_Space property;
    ///         most of general categories "Z" (separators) + most whitespace ISO controls
    ///         (including no-break spaces, but excluding IS1..IS4 and ZWSP)
    ///     </description></item>
    ///     <item><term><see cref="IsWhitespace(int)"/></term><description>
    ///         .NET <see cref="Char.IsWhiteSpace(char)"/> or <see cref="Char.IsWhiteSpace(string, int)"/>; 
    ///         Z + whitespace ISO controls but excluding no-break spaces
    ///     </description></item>
    ///     <item><term><see cref="IsSpaceChar(int)"/></term><description>
    ///         just Z (including no-break spaces)
    ///     </description></item>
    /// </list>
    /// </icunote>
    /// <para/>
    /// This class is not subclassable.
    /// </remarks>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.1</stable>
    /// <see cref="UUnicodeCategory"/>
    /// <see cref="UCharacterDirection"/>
    // ICU4N TODO: API Add all members of System.Char to this class
    // ICU4N TODO: API Merge Support.Character with this class
    public static partial class UChar // ICU4N specific - renamed from UCharacter to match .NET and made class static because there are no instance members
    {
        // ICU4N specific - copy UNASSIGNED from UCharacterEnums.ECharacterCategory (since we cannot inherit via interface)

        /// <summary>
        /// Unassigned character type
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const byte Unassigned = (byte)UUnicodeCategory.Unassigned;

        // public inner classes ----------------------------------------------

        /// <icu>_usage_</icu>
        /// <summary>
        /// A family of character subsets representing the character blocks in the
        /// Unicode specification, generated from Unicode Data file Blocks.txt.
        /// Character blocks generally define characters used for a specific script
        /// or purpose. A character is contained by at most one Unicode block.
        /// <para/>
        /// <icunote>All fields named XXX_ID are specific to ICU.</icunote>
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public sealed class UnicodeBlock
        {
            // block id corresponding to icu4c -----------------------------------

            /// <stable>ICU 2.4</stable>
            public const int INVALID_CODE_ID = -1;
            /// <stable>ICU 2.4</stable>
            public const int BASIC_LATIN_ID = 1;
            /// <stable>ICU 2.4</stable>
            public const int LATIN_1_SUPPLEMENT_ID = 2;
            /// <stable>ICU 2.4</stable>
            public const int LATIN_EXTENDED_A_ID = 3;
            /// <stable>ICU 2.4</stable>
            public const int LATIN_EXTENDED_B_ID = 4;
            /// <stable>ICU 2.4</stable>
            public const int IPA_EXTENSIONS_ID = 5;
            /// <stable>ICU 2.4</stable>
            public const int SPACING_MODIFIER_LETTERS_ID = 6;
            /// <stable>ICU 2.4</stable>
            public const int COMBINING_DIACRITICAL_MARKS_ID = 7;
            /// <summary>
            /// Unicode 3.2 renames this block to "Greek and Coptic".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int GREEK_ID = 8;
            /// <stable>ICU 2.4</stable>
            public const int CYRILLIC_ID = 9;
            /// <stable>ICU 2.4</stable>
            public const int ARMENIAN_ID = 10;
            /// <stable>ICU 2.4</stable>
            public const int HEBREW_ID = 11;
            /// <stable>ICU 2.4</stable>
            public const int ARABIC_ID = 12;
            /// <stable>ICU 2.4</stable>
            public const int SYRIAC_ID = 13;
            /// <stable>ICU 2.4</stable>
            public const int THAANA_ID = 14;
            /// <stable>ICU 2.4</stable>
            public const int DEVANAGARI_ID = 15;
            /// <stable>ICU 2.4</stable>
            public const int BENGALI_ID = 16;
            /// <stable>ICU 2.4</stable>
            public const int GURMUKHI_ID = 17;
            /// <stable>ICU 2.4</stable>
            public const int GUJARATI_ID = 18;
            /// <stable>ICU 2.4</stable>
            public const int ORIYA_ID = 19;
            /// <stable>ICU 2.4</stable>
            public const int TAMIL_ID = 20;
            /// <stable>ICU 2.4</stable>
            public const int TELUGU_ID = 21;
            /// <stable>ICU 2.4</stable>
            public const int KANNADA_ID = 22;
            /// <stable>ICU 2.4</stable>
            public const int MALAYALAM_ID = 23;
            /// <stable>ICU 2.4</stable>
            public const int SINHALA_ID = 24;
            /// <stable>ICU 2.4</stable>
            public const int THAI_ID = 25;
            /// <stable>ICU 2.4</stable>
            public const int LAO_ID = 26;
            /// <stable>ICU 2.4</stable>
            public const int TIBETAN_ID = 27;
            /// <stable>ICU 2.4</stable>
            public const int MYANMAR_ID = 28;
            /// <stable>ICU 2.4</stable>
            public const int GEORGIAN_ID = 29;
            /// <stable>ICU 2.4</stable>
            public const int HANGUL_JAMO_ID = 30;
            /// <stable>ICU 2.4</stable>
            public const int ETHIOPIC_ID = 31;
            /// <stable>ICU 2.4</stable>
            public const int CHEROKEE_ID = 32;
            /// <stable>ICU 2.4</stable>
            public const int UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_ID = 33;
            /// <stable>ICU 2.4</stable>
            public const int OGHAM_ID = 34;
            /// <stable>ICU 2.4</stable>
            public const int RUNIC_ID = 35;
            /// <stable>ICU 2.4</stable>
            public const int KHMER_ID = 36;
            /// <stable>ICU 2.4</stable>
            public const int MONGOLIAN_ID = 37;
            /// <stable>ICU 2.4</stable>
            public const int LATIN_EXTENDED_ADDITIONAL_ID = 38;
            /// <stable>ICU 2.4</stable>
            public const int GREEK_EXTENDED_ID = 39;
            /// <stable>ICU 2.4</stable>
            public const int GENERAL_PUNCTUATION_ID = 40;
            /// <stable>ICU 2.4</stable>
            public const int SUPERSCRIPTS_AND_SUBSCRIPTS_ID = 41;
            /// <stable>ICU 2.4</stable>
            public const int CURRENCY_SYMBOLS_ID = 42;
            /// <summary>
            /// Unicode 3.2 renames this block to "Combining Diacritical Marks for
            /// Symbols".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int COMBINING_MARKS_FOR_SYMBOLS_ID = 43;
            /// <stable>ICU 2.4</stable>
            public const int LETTERLIKE_SYMBOLS_ID = 44;
            /// <stable>ICU 2.4</stable>
            public const int NUMBER_FORMS_ID = 45;
            /// <stable>ICU 2.4</stable>
            public const int ARROWS_ID = 46;
            /// <stable>ICU 2.4</stable>
            public const int MATHEMATICAL_OPERATORS_ID = 47;
            /// <stable>ICU 2.4</stable>
            public const int MISCELLANEOUS_TECHNICAL_ID = 48;
            /// <stable>ICU 2.4</stable>
            public const int CONTROL_PICTURES_ID = 49;
            /// <stable>ICU 2.4</stable>
            public const int OPTICAL_CHARACTER_RECOGNITION_ID = 50;
            /// <stable>ICU 2.4</stable>
            public const int ENCLOSED_ALPHANUMERICS_ID = 51;
            /// <stable>ICU 2.4</stable>
            public const int BOX_DRAWING_ID = 52;
            /// <stable>ICU 2.4</stable>
            public const int BLOCK_ELEMENTS_ID = 53;
            /// <stable>ICU 2.4</stable>
            public const int GEOMETRIC_SHAPES_ID = 54;
            /// <stable>ICU 2.4</stable>
            public const int MISCELLANEOUS_SYMBOLS_ID = 55;
            /// <stable>ICU 2.4</stable>
            public const int DINGBATS_ID = 56;
            /// <stable>ICU 2.4</stable>
            public const int BRAILLE_PATTERNS_ID = 57;
            /// <stable>ICU 2.4</stable>
            public const int CJK_RADICALS_SUPPLEMENT_ID = 58;
            /// <stable>ICU 2.4</stable>
            public const int KANGXI_RADICALS_ID = 59;
            /// <stable>ICU 2.4</stable>
            public const int IDEOGRAPHIC_DESCRIPTION_CHARACTERS_ID = 60;
            /// <stable>ICU 2.4</stable>
            public const int CJK_SYMBOLS_AND_PUNCTUATION_ID = 61;
            /// <stable>ICU 2.4</stable>
            public const int HIRAGANA_ID = 62;
            /// <stable>ICU 2.4</stable>
            public const int KATAKANA_ID = 63;
            /// <stable>ICU 2.4</stable>
            public const int BOPOMOFO_ID = 64;
            /// <stable>ICU 2.4</stable>
            public const int HANGUL_COMPATIBILITY_JAMO_ID = 65;
            /// <stable>ICU 2.4</stable>
            public const int KANBUN_ID = 66;
            /// <stable>ICU 2.4</stable>
            public const int BOPOMOFO_EXTENDED_ID = 67;
            /// <stable>ICU 2.4</stable>
            public const int ENCLOSED_CJK_LETTERS_AND_MONTHS_ID = 68;
            /// <stable>ICU 2.4</stable>
            public const int CJK_COMPATIBILITY_ID = 69;
            /// <stable>ICU 2.4</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A_ID = 70;
            /// <stable>ICU 2.4</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_ID = 71;
            /// <stable>ICU 2.4</stable>
            public const int YI_SYLLABLES_ID = 72;
            /// <stable>ICU 2.4</stable>
            public const int YI_RADICALS_ID = 73;
            /// <stable>ICU 2.4</stable>
            public const int HANGUL_SYLLABLES_ID = 74;
            /// <stable>ICU 2.4</stable>
            public const int HIGH_SURROGATES_ID = 75;
            /// <stable>ICU 2.4</stable>
            public const int HIGH_PRIVATE_USE_SURROGATES_ID = 76;
            /// <stable>ICU 2.4</stable>
            public const int LOW_SURROGATES_ID = 77;
            /// <summary>
            /// Same as <see cref="PRIVATE_USE"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int PRIVATE_USE_AREA_ID = 78;
            /// <summary>
            /// Same as <see cref="PRIVATE_USE_AREA"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int PRIVATE_USE_ID = PRIVATE_USE_AREA_ID;
            /// <stable>ICU 2.4</stable>
            public const int CJK_COMPATIBILITY_IDEOGRAPHS_ID = 79;
            /// <stable>ICU 2.4</stable>
            public const int ALPHABETIC_PRESENTATION_FORMS_ID = 80;
            /// <stable>ICU 2.4</stable>
            public const int ARABIC_PRESENTATION_FORMS_A_ID = 81;
            /// <stable>ICU 2.4</stable>
            public const int COMBINING_HALF_MARKS_ID = 82;
            /// <stable>ICU 2.4</stable>
            public const int CJK_COMPATIBILITY_FORMS_ID = 83;
            /// <stable>ICU 2.4</stable>
            public const int SMALL_FORM_VARIANTS_ID = 84;
            /// <stable>ICU 2.4</stable>
            public const int ARABIC_PRESENTATION_FORMS_B_ID = 85;
            /// <stable>ICU 2.4</stable>
            public const int SPECIALS_ID = 86;
            /// <stable>ICU 2.4</stable>
            public const int HALFWIDTH_AND_FULLWIDTH_FORMS_ID = 87;
            /// <stable>ICU 2.4</stable>
            public const int OLD_ITALIC_ID = 88;
            /// <stable>ICU 2.4</stable>
            public const int GOTHIC_ID = 89;
            /// <stable>ICU 2.4</stable>
            public const int DESERET_ID = 90;
            /// <stable>ICU 2.4</stable>
            public const int BYZANTINE_MUSICAL_SYMBOLS_ID = 91;
            /// <stable>ICU 2.4</stable>
            public const int MUSICAL_SYMBOLS_ID = 92;
            /// <stable>ICU 2.4</stable>
            public const int MATHEMATICAL_ALPHANUMERIC_SYMBOLS_ID = 93;
            /// <stable>ICU 2.4</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B_ID = 94;
            /// <stable>ICU 2.4</stable>
            public const int CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT_ID = 95;
            /// <stable>ICU 2.4</stable>
            public const int TAGS_ID = 96;

            // New blocks in Unicode 3.2

            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int CYRILLIC_SUPPLEMENTARY_ID = 97;
            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 3.0</stable>
            public const int CYRILLIC_SUPPLEMENT_ID = 97;
            /// <stable>ICU 2.4</stable>
            public const int TAGALOG_ID = 98;
            /// <stable>ICU 2.4</stable>
            public const int HANUNOO_ID = 99;
            /// <stable>ICU 2.4</stable>
            public const int BUHID_ID = 100;
            /// <stable>ICU 2.4</stable>
            public const int TAGBANWA_ID = 101;
            /// <stable>ICU 2.4</stable>
            public const int MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A_ID = 102;
            /// <stable>ICU 2.4</stable>
            public const int SUPPLEMENTAL_ARROWS_A_ID = 103;
            /// <stable>ICU 2.4</stable>
            public const int SUPPLEMENTAL_ARROWS_B_ID = 104;
            /// <stable>ICU 2.4</stable>
            public const int MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B_ID = 105;
            /// <stable>ICU 2.4</stable>
            public const int SUPPLEMENTAL_MATHEMATICAL_OPERATORS_ID = 106;
            /// <stable>ICU 2.4</stable>
            public const int KATAKANA_PHONETIC_EXTENSIONS_ID = 107;
            /// <stable>ICU 2.4</stable>
            public const int VARIATION_SELECTORS_ID = 108;
            /// <stable>ICU 2.4</stable>
            public const int SUPPLEMENTARY_PRIVATE_USE_AREA_A_ID = 109;
            /// <stable>ICU 2.4</stable>
            public const int SUPPLEMENTARY_PRIVATE_USE_AREA_B_ID = 110;

            /// <stable>ICU 2.6</stable>
            public const int LIMBU_ID = 111; /*[1900]*/
            /// <stable>ICU 2.6</stable>
            public const int TAI_LE_ID = 112; /*[1950]*/
            /// <stable>ICU 2.6</stable>
            public const int KHMER_SYMBOLS_ID = 113; /*[19E0]*/
            /// <stable>ICU 2.6</stable>
            public const int PHONETIC_EXTENSIONS_ID = 114; /*[1D00]*/
            /// <stable>ICU 2.6</stable>
            public const int MISCELLANEOUS_SYMBOLS_AND_ARROWS_ID = 115; /*[2B00]*/
            /// <stable>ICU 2.6</stable>
            public const int YIJING_HEXAGRAM_SYMBOLS_ID = 116; /*[4DC0]*/
            /// <stable>ICU 2.6</stable>
            public const int LINEAR_B_SYLLABARY_ID = 117; /*[10000]*/
            /// <stable>ICU 2.6</stable>
            public const int LINEAR_B_IDEOGRAMS_ID = 118; /*[10080]*/
            /// <stable>ICU 2.6</stable>
            public const int AEGEAN_NUMBERS_ID = 119; /*[10100]*/
            /// <stable>ICU 2.6</stable>
            public const int UGARITIC_ID = 120; /*[10380]*/
            /// <stable>ICU 2.6</stable>
            public const int SHAVIAN_ID = 121; /*[10450]*/
            /// <stable>ICU 2.6</stable>
            public const int OSMANYA_ID = 122; /*[10480]*/
            /// <stable>ICU 2.6</stable>
            public const int CYPRIOT_SYLLABARY_ID = 123; /*[10800]*/
            /// <stable>ICU 2.6</stable>
            public const int TAI_XUAN_JING_SYMBOLS_ID = 124; /*[1D300]*/
            /// <stable>ICU 2.6</stable>
            public const int VARIATION_SELECTORS_SUPPLEMENT_ID = 125; /*[E0100]*/

            /* New blocks in Unicode 4.1 */

            /// <stable>ICU 3.4</stable>
            public const int ANCIENT_GREEK_MUSICAL_NOTATION_ID = 126; /*[1D200]*/

            /// <stable>ICU 3.4</stable>
            public const int ANCIENT_GREEK_NUMBERS_ID = 127; /*[10140]*/

            /// <stable>ICU 3.4</stable>
            public const int ARABIC_SUPPLEMENT_ID = 128; /*[0750]*/

            /// <stable>ICU 3.4</stable>
            public const int BUGINESE_ID = 129; /*[1A00]*/

            /// <stable>ICU 3.4</stable>
            public const int CJK_STROKES_ID = 130; /*[31C0]*/

            /// <stable>ICU 3.4</stable>
            public const int COMBINING_DIACRITICAL_MARKS_SUPPLEMENT_ID = 131; /*[1DC0]*/

            /// <stable>ICU 3.4</stable>
            public const int COPTIC_ID = 132; /*[2C80]*/

            /// <stable>ICU 3.4</stable>
            public const int ETHIOPIC_EXTENDED_ID = 133; /*[2D80]*/

            /// <stable>ICU 3.4</stable>
            public const int ETHIOPIC_SUPPLEMENT_ID = 134; /*[1380]*/

            /// <stable>ICU 3.4</stable>
            public const int GEORGIAN_SUPPLEMENT_ID = 135; /*[2D00]*/

            /// <stable>ICU 3.4</stable>
            public const int GLAGOLITIC_ID = 136; /*[2C00]*/

            /// <stable>ICU 3.4</stable>
            public const int KHAROSHTHI_ID = 137; /*[10A00]*/

            /// <stable>ICU 3.4</stable>
            public const int MODIFIER_TONE_LETTERS_ID = 138; /*[A700]*/

            /// <stable>ICU 3.4</stable>
            public const int NEW_TAI_LUE_ID = 139; /*[1980]*/

            /// <stable>ICU 3.4</stable>
            public const int OLD_PERSIAN_ID = 140; /*[103A0]*/

            /// <stable>ICU 3.4</stable>
            public const int PHONETIC_EXTENSIONS_SUPPLEMENT_ID = 141; /*[1D80]*/

            /// <stable>ICU 3.4</stable>
            public const int SUPPLEMENTAL_PUNCTUATION_ID = 142; /*[2E00]*/

            /// <stable>ICU 3.4</stable>
            public const int SYLOTI_NAGRI_ID = 143; /*[A800]*/

            /// <stable>ICU 3.4</stable>
            public const int TIFINAGH_ID = 144; /*[2D30]*/

            /// <stable>ICU 3.4</stable>
            public const int VERTICAL_FORMS_ID = 145; /*[FE10]*/

            /* New blocks in Unicode 5.0 */

            /// <stable>ICU 3.6</stable>
            public const int NKO_ID = 146; /*[07C0]*/
            /// <stable>ICU 3.6</stable>
            public const int BALINESE_ID = 147; /*[1B00]*/
            /// <stable>ICU 3.6</stable>
            public const int LATIN_EXTENDED_C_ID = 148; /*[2C60]*/
            /// <stable>ICU 3.6</stable>
            public const int LATIN_EXTENDED_D_ID = 149; /*[A720]*/
            /// <stable>ICU 3.6</stable>
            public const int PHAGS_PA_ID = 150; /*[A840]*/
            /// <stable>ICU 3.6</stable>
            public const int PHOENICIAN_ID = 151; /*[10900]*/
            /// <stable>ICU 3.6</stable>
            public const int CUNEIFORM_ID = 152; /*[12000]*/
            /// <stable>ICU 3.6</stable>
            public const int CUNEIFORM_NUMBERS_AND_PUNCTUATION_ID = 153; /*[12400]*/
            /// <stable>ICU 3.6</stable>
            public const int COUNTING_ROD_NUMERALS_ID = 154; /*[1D360]*/

            /// <stable>ICU 4.0</stable>
            public const int SUNDANESE_ID = 155; /* [1B80] */

            /// <stable>ICU 4.0</stable>
            public const int LEPCHA_ID = 156; /* [1C00] */

            /// <stable>ICU 4.0</stable>
            public const int OL_CHIKI_ID = 157; /* [1C50] */

            /// <stable>ICU 4.0</stable>
            public const int CYRILLIC_EXTENDED_A_ID = 158; /* [2DE0] */

            /// <stable>ICU 4.0</stable>
            public const int VAI_ID = 159; /* [A500] */

            /// <stable>ICU 4.0</stable>
            public const int CYRILLIC_EXTENDED_B_ID = 160; /* [A640] */

            /// <stable>ICU 4.0</stable>
            public const int SAURASHTRA_ID = 161; /* [A880] */

            /// <stable>ICU 4.0</stable>
            public const int KAYAH_LI_ID = 162; /* [A900] */

            /// <stable>ICU 4.0</stable>
            public const int REJANG_ID = 163; /* [A930] */

            /// <stable>ICU 4.0</stable>
            public const int CHAM_ID = 164; /* [AA00] */

            /// <stable>ICU 4.0</stable>
            public const int ANCIENT_SYMBOLS_ID = 165; /* [10190] */

            /// <stable>ICU 4.0</stable>
            public const int PHAISTOS_DISC_ID = 166; /* [101D0] */

            /// <stable>ICU 4.0</stable>
            public const int LYCIAN_ID = 167; /* [10280] */

            /// <stable>ICU 4.0</stable>
            public const int CARIAN_ID = 168; /* [102A0] */

            /// <stable>ICU 4.0</stable>
            public const int LYDIAN_ID = 169; /* [10920] */

            /// <stable>ICU 4.0</stable>
            public const int MAHJONG_TILES_ID = 170; /* [1F000] */

            /// <stable>ICU 4.0</stable>
            public const int DOMINO_TILES_ID = 171; /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /// <stable>ICU 4.4</stable>
            public const int SAMARITAN_ID = 172; /*[0800]*/
            /// <stable>ICU 4.4</stable>
            public const int UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED_ID = 173; /*[18B0]*/
            /// <stable>ICU 4.4</stable>
            public const int TAI_THAM_ID = 174; /*[1A20]*/
            /// <stable>ICU 4.4</stable>
            public const int VEDIC_EXTENSIONS_ID = 175; /*[1CD0]*/
            /// <stable>ICU 4.4</stable>
            public const int LISU_ID = 176; /*[A4D0]*/
            /// <stable>ICU 4.4</stable>
            public const int BAMUM_ID = 177; /*[A6A0]*/
            /// <stable>ICU 4.4</stable>
            public const int COMMON_INDIC_NUMBER_FORMS_ID = 178; /*[A830]*/
            /// <stable>ICU 4.4</stable>
            public const int DEVANAGARI_EXTENDED_ID = 179; /*[A8E0]*/
            /// <stable>ICU 4.4</stable>
            public const int HANGUL_JAMO_EXTENDED_A_ID = 180; /*[A960]*/
            /// <stable>ICU 4.4</stable>
            public const int JAVANESE_ID = 181; /*[A980]*/
            /// <stable>ICU 4.4</stable>
            public const int MYANMAR_EXTENDED_A_ID = 182; /*[AA60]*/
            /// <stable>ICU 4.4</stable>
            public const int TAI_VIET_ID = 183; /*[AA80]*/
            /// <stable>ICU 4.4</stable>
            public const int MEETEI_MAYEK_ID = 184; /*[ABC0]*/
            /// <stable>ICU 4.4</stable>
            public const int HANGUL_JAMO_EXTENDED_B_ID = 185; /*[D7B0]*/
            /// <stable>ICU 4.4</stable>
            public const int IMPERIAL_ARAMAIC_ID = 186; /*[10840]*/
            /// <stable>ICU 4.4</stable>
            public const int OLD_SOUTH_ARABIAN_ID = 187; /*[10A60]*/
            /// <stable>ICU 4.4</stable>
            public const int AVESTAN_ID = 188; /*[10B00]*/
            /// <stable>ICU 4.4</stable>
            public const int INSCRIPTIONAL_PARTHIAN_ID = 189; /*[10B40]*/
            /// <stable>ICU 4.4</stable>
            public const int INSCRIPTIONAL_PAHLAVI_ID = 190; /*[10B60]*/
            /// <stable>ICU 4.4</stable>
            public const int OLD_TURKIC_ID = 191; /*[10C00]*/
            /// <stable>ICU 4.4</stable>
            public const int RUMI_NUMERAL_SYMBOLS_ID = 192; /*[10E60]*/
            /// <stable>ICU 4.4</stable>
            public const int KAITHI_ID = 193; /*[11080]*/
            /// <stable>ICU 4.4</stable>
            public const int EGYPTIAN_HIEROGLYPHS_ID = 194; /*[13000]*/
            /// <stable>ICU 4.4</stable>
            public const int ENCLOSED_ALPHANUMERIC_SUPPLEMENT_ID = 195; /*[1F100]*/
            /// <stable>ICU 4.4</stable>
            public const int ENCLOSED_IDEOGRAPHIC_SUPPLEMENT_ID = 196; /*[1F200]*/
            /// <stable>ICU 4.4</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C_ID = 197; /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /// <stable>ICU 4.6</stable>
            public const int MANDAIC_ID = 198; /*[0840]*/
            /// <stable>ICU 4.6</stable>
            public const int BATAK_ID = 199; /*[1BC0]*/
            /// <stable>ICU 4.6</stable>
            public const int ETHIOPIC_EXTENDED_A_ID = 200; /*[AB00]*/
            /// <stable>ICU 4.6</stable>
            public const int BRAHMI_ID = 201; /*[11000]*/
            /// <stable>ICU 4.6</stable>
            public const int BAMUM_SUPPLEMENT_ID = 202; /*[16800]*/
            /// <stable>ICU 4.6</stable>
            public const int KANA_SUPPLEMENT_ID = 203; /*[1B000]*/
            /// <stable>ICU 4.6</stable>
            public const int PLAYING_CARDS_ID = 204; /*[1F0A0]*/
            /// <stable>ICU 4.6</stable>
            public const int MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS_ID = 205; /*[1F300]*/
            /// <stable>ICU 4.6</stable>
            public const int EMOTICONS_ID = 206; /*[1F600]*/
            /// <stable>ICU 4.6</stable>
            public const int TRANSPORT_AND_MAP_SYMBOLS_ID = 207; /*[1F680]*/
            /// <stable>ICU 4.6</stable>
            public const int ALCHEMICAL_SYMBOLS_ID = 208; /*[1F700]*/
            /// <stable>ICU 4.6</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D_ID = 209; /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /// <stable>ICU 49</stable>
            public const int ARABIC_EXTENDED_A_ID = 210; /*[08A0]*/
            /// <stable>ICU 49</stable>
            public const int ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS_ID = 211; /*[1EE00]*/
            /// <stable>ICU 49</stable>
            public const int CHAKMA_ID = 212; /*[11100]*/
            /// <stable>ICU 49</stable>
            public const int MEETEI_MAYEK_EXTENSIONS_ID = 213; /*[AAE0]*/
            /// <stable>ICU 49</stable>
            public const int MEROITIC_CURSIVE_ID = 214; /*[109A0]*/
            /// <stable>ICU 49</stable>
            public const int MEROITIC_HIEROGLYPHS_ID = 215; /*[10980]*/
            /// <stable>ICU 49</stable>
            public const int MIAO_ID = 216; /*[16F00]*/
            /// <stable>ICU 49</stable>
            public const int SHARADA_ID = 217; /*[11180]*/
            /// <stable>ICU 49</stable>
            public const int SORA_SOMPENG_ID = 218; /*[110D0]*/
            /// <stable>ICU 49</stable>
            public const int SUNDANESE_SUPPLEMENT_ID = 219; /*[1CC0]*/
            /// <stable>ICU 49</stable>
            public const int TAKRI_ID = 220; /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /// <stable>ICU 54</stable>
            public const int BASSA_VAH_ID = 221; /*[16AD0]*/
            /// <stable>ICU 54</stable>
            public const int CAUCASIAN_ALBANIAN_ID = 222; /*[10530]*/
            /// <stable>ICU 54</stable>
            public const int COPTIC_EPACT_NUMBERS_ID = 223; /*[102E0]*/
            /// <stable>ICU 54</stable>
            public const int COMBINING_DIACRITICAL_MARKS_EXTENDED_ID = 224; /*[1AB0]*/
            /// <stable>ICU 54</stable>
            public const int DUPLOYAN_ID = 225; /*[1BC00]*/
            /// <stable>ICU 54</stable>
            public const int ELBASAN_ID = 226; /*[10500]*/
            /// <stable>ICU 54</stable>
            public const int GEOMETRIC_SHAPES_EXTENDED_ID = 227; /*[1F780]*/
            /// <stable>ICU 54</stable>
            public const int GRANTHA_ID = 228; /*[11300]*/
            /// <stable>ICU 54</stable>
            public const int KHOJKI_ID = 229; /*[11200]*/
            /// <stable>ICU 54</stable>
            public const int KHUDAWADI_ID = 230; /*[112B0]*/
            /// <stable>ICU 54</stable>
            public const int LATIN_EXTENDED_E_ID = 231; /*[AB30]*/
            /// <stable>ICU 54</stable>
            public const int LINEAR_A_ID = 232; /*[10600]*/
            /// <stable>ICU 54</stable>
            public const int MAHAJANI_ID = 233; /*[11150]*/
            /// <stable>ICU 54</stable>
            public const int MANICHAEAN_ID = 234; /*[10AC0]*/
            /// <stable>ICU 54</stable>
            public const int MENDE_KIKAKUI_ID = 235; /*[1E800]*/
            /// <stable>ICU 54</stable>
            public const int MODI_ID = 236; /*[11600]*/
            /// <stable>ICU 54</stable>
            public const int MRO_ID = 237; /*[16A40]*/
            /// <stable>ICU 54</stable>
            public const int MYANMAR_EXTENDED_B_ID = 238; /*[A9E0]*/
            /// <stable>ICU 54</stable>
            public const int NABATAEAN_ID = 239; /*[10880]*/
            /// <stable>ICU 54</stable>
            public const int OLD_NORTH_ARABIAN_ID = 240; /*[10A80]*/
            /// <stable>ICU 54</stable>
            public const int OLD_PERMIC_ID = 241; /*[10350]*/
            /// <stable>ICU 54</stable>
            public const int ORNAMENTAL_DINGBATS_ID = 242; /*[1F650]*/
            /// <stable>ICU 54</stable>
            public const int PAHAWH_HMONG_ID = 243; /*[16B00]*/
            /// <stable>ICU 54</stable>
            public const int PALMYRENE_ID = 244; /*[10860]*/
            /// <stable>ICU 54</stable>
            public const int PAU_CIN_HAU_ID = 245; /*[11AC0]*/
            /// <stable>ICU 54</stable>
            public const int PSALTER_PAHLAVI_ID = 246; /*[10B80]*/
            /// <stable>ICU 54</stable>
            public const int SHORTHAND_FORMAT_CONTROLS_ID = 247; /*[1BCA0]*/
            /// <stable>ICU 54</stable>
            public const int SIDDHAM_ID = 248; /*[11580]*/
            /// <stable>ICU 54</stable>
            public const int SINHALA_ARCHAIC_NUMBERS_ID = 249; /*[111E0]*/
            /// <stable>ICU 54</stable>
            public const int SUPPLEMENTAL_ARROWS_C_ID = 250; /*[1F800]*/
            /// <stable>ICU 54</stable>
            public const int TIRHUTA_ID = 251; /*[11480]*/
            /// <stable>ICU 54</stable>
            public const int WARANG_CITI_ID = 252; /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /// <stable>ICU 56</stable>
            public const int AHOM_ID = 253; /*[11700]*/
            /// <stable>ICU 56</stable>
            public const int ANATOLIAN_HIEROGLYPHS_ID = 254; /*[14400]*/
            /// <stable>ICU 56</stable>
            public const int CHEROKEE_SUPPLEMENT_ID = 255; /*[AB70]*/
            /// <stable>ICU 56</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E_ID = 256; /*[2B820]*/
            /// <stable>ICU 56</stable>
            public const int EARLY_DYNASTIC_CUNEIFORM_ID = 257; /*[12480]*/
            /// <stable>ICU 56</stable>
            public const int HATRAN_ID = 258; /*[108E0]*/
            /// <stable>ICU 56</stable>
            public const int MULTANI_ID = 259; /*[11280]*/
            /// <stable>ICU 56</stable>
            public const int OLD_HUNGARIAN_ID = 260; /*[10C80]*/
            /// <stable>ICU 56</stable>
            public const int SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS_ID = 261; /*[1F900]*/
            /// <stable>ICU 56</stable>
            public const int SUTTON_SIGNWRITING_ID = 262; /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /// <stable>ICU 58</stable>
            public const int ADLAM_ID = 263; /*[1E900]*/
            /// <stable>ICU 58</stable>
            public const int BHAIKSUKI_ID = 264; /*[11C00]*/
            /// <stable>ICU 58</stable>
            public const int CYRILLIC_EXTENDED_C_ID = 265; /*[1C80]*/
            /// <stable>ICU 58</stable>
            public const int GLAGOLITIC_SUPPLEMENT_ID = 266; /*[1E000]*/
            /// <stable>ICU 58</stable>
            public const int IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION_ID = 267; /*[16FE0]*/
            /// <stable>ICU 58</stable>
            public const int MARCHEN_ID = 268; /*[11C70]*/
            /// <stable>ICU 58</stable>
            public const int MONGOLIAN_SUPPLEMENT_ID = 269; /*[11660]*/
            /// <stable>ICU 58</stable>
            public const int NEWA_ID = 270; /*[11400]*/
            /// <stable>ICU 58</stable>
            public const int OSAGE_ID = 271; /*[104B0]*/
            /// <stable>ICU 58</stable>
            public const int TANGUT_ID = 272; /*[17000]*/
            /// <stable>ICU 58</stable>
            public const int TANGUT_COMPONENTS_ID = 273; /*[18800]*/

            // New blocks in Unicode 10.0

            /// <stable>ICU 60</stable>
            public const int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F_ID = 274; /*[2CEB0]*/
            /// <stable>ICU 60</stable>
            public const int KANA_EXTENDED_A_ID = 275; /*[1B100]*/
            /// <stable>ICU 60</stable>
            public const int MASARAM_GONDI_ID = 276; /*[11D00]*/
            /// <stable>ICU 60</stable>
            public const int NUSHU_ID = 277; /*[1B170]*/
            /// <stable>ICU 60</stable>
            public const int SOYOMBO_ID = 278; /*[11A50]*/
            /// <stable>ICU 60</stable>
            public const int SYRIAC_SUPPLEMENT_ID = 279; /*[0860]*/
            /// <stable>ICU 60</stable>
            public const int ZANABAZAR_SQUARE_ID = 280; /*[11A00]*/

            /// <summary>
            /// One more than the highest normal UnicodeBlock value.
            /// The highest value is available via <see cref="UChar.GetInt32PropertyValue(int, UProperty)"/> 
            /// with parameter <see cref="UProperty.Block"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int COUNT = 281;

            // blocks objects ---------------------------------------------------

            /// <summary>
            /// Array of <see cref="UnicodeBlock"/>s, for easy access in <see cref="GetInstance(int)"/>
            /// </summary>
#pragma warning disable 612, 618
            private readonly static UnicodeBlock[] BLOCKS_ = new UnicodeBlock[COUNT];
#pragma warning restore 612, 618

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock NO_BLOCK
                = new UnicodeBlock("NO_BLOCK", 0);

            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BASIC_LATIN
                = new UnicodeBlock("BASIC_LATIN", BASIC_LATIN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LATIN_1_SUPPLEMENT
                = new UnicodeBlock("LATIN_1_SUPPLEMENT", LATIN_1_SUPPLEMENT_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_A
                = new UnicodeBlock("LATIN_EXTENDED_A", LATIN_EXTENDED_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_B
                = new UnicodeBlock("LATIN_EXTENDED_B", LATIN_EXTENDED_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock IPA_EXTENSIONS
                = new UnicodeBlock("IPA_EXTENSIONS", IPA_EXTENSIONS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SPACING_MODIFIER_LETTERS
                = new UnicodeBlock("SPACING_MODIFIER_LETTERS", SPACING_MODIFIER_LETTERS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS
                = new UnicodeBlock("COMBINING_DIACRITICAL_MARKS", COMBINING_DIACRITICAL_MARKS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GREEK
                = new UnicodeBlock("GREEK", GREEK_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CYRILLIC
                = new UnicodeBlock("CYRILLIC", CYRILLIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ARMENIAN
                = new UnicodeBlock("ARMENIAN", ARMENIAN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HEBREW
                = new UnicodeBlock("HEBREW", HEBREW_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ARABIC
                = new UnicodeBlock("ARABIC", ARABIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SYRIAC
                = new UnicodeBlock("SYRIAC", SYRIAC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock THAANA
                = new UnicodeBlock("THAANA", THAANA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock DEVANAGARI
                = new UnicodeBlock("DEVANAGARI", DEVANAGARI_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BENGALI
                = new UnicodeBlock("BENGALI", BENGALI_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GURMUKHI
                = new UnicodeBlock("GURMUKHI", GURMUKHI_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GUJARATI
                = new UnicodeBlock("GUJARATI", GUJARATI_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ORIYA
                = new UnicodeBlock("ORIYA", ORIYA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TAMIL
                = new UnicodeBlock("TAMIL", TAMIL_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TELUGU
                = new UnicodeBlock("TELUGU", TELUGU_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KANNADA
                = new UnicodeBlock("KANNADA", KANNADA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MALAYALAM
                = new UnicodeBlock("MALAYALAM", MALAYALAM_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SINHALA
                = new UnicodeBlock("SINHALA", SINHALA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock THAI
                = new UnicodeBlock("THAI", THAI_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LAO
                = new UnicodeBlock("LAO", LAO_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TIBETAN
                = new UnicodeBlock("TIBETAN", TIBETAN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MYANMAR
                = new UnicodeBlock("MYANMAR", MYANMAR_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GEORGIAN
                = new UnicodeBlock("GEORGIAN", GEORGIAN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HANGUL_JAMO
                = new UnicodeBlock("HANGUL_JAMO", HANGUL_JAMO_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ETHIOPIC
                = new UnicodeBlock("ETHIOPIC", ETHIOPIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CHEROKEE
                = new UnicodeBlock("CHEROKEE", CHEROKEE_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS
                = new UnicodeBlock("UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS",
                    UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock OGHAM
                = new UnicodeBlock("OGHAM", OGHAM_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock RUNIC
                = new UnicodeBlock("RUNIC", RUNIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KHMER
                = new UnicodeBlock("KHMER", KHMER_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MONGOLIAN
                = new UnicodeBlock("MONGOLIAN", MONGOLIAN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_ADDITIONAL
                = new UnicodeBlock("LATIN_EXTENDED_ADDITIONAL", LATIN_EXTENDED_ADDITIONAL_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GREEK_EXTENDED
                = new UnicodeBlock("GREEK_EXTENDED", GREEK_EXTENDED_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GENERAL_PUNCTUATION
                = new UnicodeBlock("GENERAL_PUNCTUATION", GENERAL_PUNCTUATION_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPERSCRIPTS_AND_SUBSCRIPTS
                = new UnicodeBlock("SUPERSCRIPTS_AND_SUBSCRIPTS", SUPERSCRIPTS_AND_SUBSCRIPTS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CURRENCY_SYMBOLS
                = new UnicodeBlock("CURRENCY_SYMBOLS", CURRENCY_SYMBOLS_ID);
            /// <summary>
            /// Unicode 3.2 renames this block to "Combining Diacritical Marks for
            /// Symbols".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock COMBINING_MARKS_FOR_SYMBOLS
                = new UnicodeBlock("COMBINING_MARKS_FOR_SYMBOLS", COMBINING_MARKS_FOR_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LETTERLIKE_SYMBOLS
                = new UnicodeBlock("LETTERLIKE_SYMBOLS", LETTERLIKE_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock NUMBER_FORMS
                = new UnicodeBlock("NUMBER_FORMS", NUMBER_FORMS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ARROWS
                = new UnicodeBlock("ARROWS", ARROWS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MATHEMATICAL_OPERATORS
                = new UnicodeBlock("MATHEMATICAL_OPERATORS", MATHEMATICAL_OPERATORS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_TECHNICAL
                = new UnicodeBlock("MISCELLANEOUS_TECHNICAL", MISCELLANEOUS_TECHNICAL_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CONTROL_PICTURES
                = new UnicodeBlock("CONTROL_PICTURES", CONTROL_PICTURES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock OPTICAL_CHARACTER_RECOGNITION
                = new UnicodeBlock("OPTICAL_CHARACTER_RECOGNITION", OPTICAL_CHARACTER_RECOGNITION_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ENCLOSED_ALPHANUMERICS
                = new UnicodeBlock("ENCLOSED_ALPHANUMERICS", ENCLOSED_ALPHANUMERICS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BOX_DRAWING
                = new UnicodeBlock("BOX_DRAWING", BOX_DRAWING_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BLOCK_ELEMENTS
                = new UnicodeBlock("BLOCK_ELEMENTS", BLOCK_ELEMENTS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GEOMETRIC_SHAPES
                = new UnicodeBlock("GEOMETRIC_SHAPES", GEOMETRIC_SHAPES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS
                = new UnicodeBlock("MISCELLANEOUS_SYMBOLS", MISCELLANEOUS_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock DINGBATS
                = new UnicodeBlock("DINGBATS", DINGBATS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BRAILLE_PATTERNS
                = new UnicodeBlock("BRAILLE_PATTERNS", BRAILLE_PATTERNS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_RADICALS_SUPPLEMENT
                = new UnicodeBlock("CJK_RADICALS_SUPPLEMENT", CJK_RADICALS_SUPPLEMENT_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KANGXI_RADICALS
                = new UnicodeBlock("KANGXI_RADICALS", KANGXI_RADICALS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock IDEOGRAPHIC_DESCRIPTION_CHARACTERS
                = new UnicodeBlock("IDEOGRAPHIC_DESCRIPTION_CHARACTERS",
                    IDEOGRAPHIC_DESCRIPTION_CHARACTERS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_SYMBOLS_AND_PUNCTUATION
                = new UnicodeBlock("CJK_SYMBOLS_AND_PUNCTUATION", CJK_SYMBOLS_AND_PUNCTUATION_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HIRAGANA
                = new UnicodeBlock("HIRAGANA", HIRAGANA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KATAKANA
                = new UnicodeBlock("KATAKANA", KATAKANA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BOPOMOFO
                = new UnicodeBlock("BOPOMOFO", BOPOMOFO_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HANGUL_COMPATIBILITY_JAMO
                = new UnicodeBlock("HANGUL_COMPATIBILITY_JAMO", HANGUL_COMPATIBILITY_JAMO_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KANBUN
                = new UnicodeBlock("KANBUN", KANBUN_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BOPOMOFO_EXTENDED
                = new UnicodeBlock("BOPOMOFO_EXTENDED", BOPOMOFO_EXTENDED_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ENCLOSED_CJK_LETTERS_AND_MONTHS
                = new UnicodeBlock("ENCLOSED_CJK_LETTERS_AND_MONTHS",
                    ENCLOSED_CJK_LETTERS_AND_MONTHS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_COMPATIBILITY
                = new UnicodeBlock("CJK_COMPATIBILITY", CJK_COMPATIBILITY_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A
                = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A",
                    CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS
                = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS", CJK_UNIFIED_IDEOGRAPHS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock YI_SYLLABLES
                = new UnicodeBlock("YI_SYLLABLES", YI_SYLLABLES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock YI_RADICALS
                = new UnicodeBlock("YI_RADICALS", YI_RADICALS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HANGUL_SYLLABLES
                = new UnicodeBlock("HANGUL_SYLLABLES", HANGUL_SYLLABLES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HIGH_SURROGATES
                = new UnicodeBlock("HIGH_SURROGATES", HIGH_SURROGATES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HIGH_PRIVATE_USE_SURROGATES
                = new UnicodeBlock("HIGH_PRIVATE_USE_SURROGATES", HIGH_PRIVATE_USE_SURROGATES_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock LOW_SURROGATES
                = new UnicodeBlock("LOW_SURROGATES", LOW_SURROGATES_ID);
            /// <summary>
            /// Same as <see cref="PRIVATE_USE"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock PRIVATE_USE_AREA
                = new UnicodeBlock("PRIVATE_USE_AREA", 78);
            /// <summary>
            /// Same as <see cref="PRIVATE_USE_AREA"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock PRIVATE_USE
                = PRIVATE_USE_AREA;
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_COMPATIBILITY_IDEOGRAPHS
                = new UnicodeBlock("CJK_COMPATIBILITY_IDEOGRAPHS", CJK_COMPATIBILITY_IDEOGRAPHS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ALPHABETIC_PRESENTATION_FORMS
                = new UnicodeBlock("ALPHABETIC_PRESENTATION_FORMS", ALPHABETIC_PRESENTATION_FORMS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ARABIC_PRESENTATION_FORMS_A
                = new UnicodeBlock("ARABIC_PRESENTATION_FORMS_A", ARABIC_PRESENTATION_FORMS_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock COMBINING_HALF_MARKS
                = new UnicodeBlock("COMBINING_HALF_MARKS", COMBINING_HALF_MARKS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_COMPATIBILITY_FORMS
                = new UnicodeBlock("CJK_COMPATIBILITY_FORMS", CJK_COMPATIBILITY_FORMS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SMALL_FORM_VARIANTS
                = new UnicodeBlock("SMALL_FORM_VARIANTS", SMALL_FORM_VARIANTS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock ARABIC_PRESENTATION_FORMS_B
                = new UnicodeBlock("ARABIC_PRESENTATION_FORMS_B", ARABIC_PRESENTATION_FORMS_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SPECIALS
                = new UnicodeBlock("SPECIALS", SPECIALS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HALFWIDTH_AND_FULLWIDTH_FORMS
                = new UnicodeBlock("HALFWIDTH_AND_FULLWIDTH_FORMS", HALFWIDTH_AND_FULLWIDTH_FORMS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock OLD_ITALIC
                = new UnicodeBlock("OLD_ITALIC", OLD_ITALIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock GOTHIC
                = new UnicodeBlock("GOTHIC", GOTHIC_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock DESERET
                = new UnicodeBlock("DESERET", DESERET_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BYZANTINE_MUSICAL_SYMBOLS
                = new UnicodeBlock("BYZANTINE_MUSICAL_SYMBOLS", BYZANTINE_MUSICAL_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MUSICAL_SYMBOLS
                = new UnicodeBlock("MUSICAL_SYMBOLS", MUSICAL_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MATHEMATICAL_ALPHANUMERIC_SYMBOLS
                = new UnicodeBlock("MATHEMATICAL_ALPHANUMERIC_SYMBOLS",
                    MATHEMATICAL_ALPHANUMERIC_SYMBOLS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B
                = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B",
                    CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT
                = new UnicodeBlock("CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT",
                    CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TAGS
                = new UnicodeBlock("TAGS", TAGS_ID);

            // New blocks in Unicode 3.2

            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CYRILLIC_SUPPLEMENTARY
                = new UnicodeBlock("CYRILLIC_SUPPLEMENTARY", CYRILLIC_SUPPLEMENTARY_ID);
            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 3.0</stable>
            public static readonly UnicodeBlock CYRILLIC_SUPPLEMENT
                = new UnicodeBlock("CYRILLIC_SUPPLEMENT", CYRILLIC_SUPPLEMENT_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TAGALOG
                = new UnicodeBlock("TAGALOG", TAGALOG_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HANUNOO
                = new UnicodeBlock("HANUNOO", HANUNOO_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock BUHID
                = new UnicodeBlock("BUHID", BUHID_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock TAGBANWA
                = new UnicodeBlock("TAGBANWA", TAGBANWA_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A
                = new UnicodeBlock("MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A",
                    MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_A
                = new UnicodeBlock("SUPPLEMENTAL_ARROWS_A", SUPPLEMENTAL_ARROWS_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_B
                = new UnicodeBlock("SUPPLEMENTAL_ARROWS_B", SUPPLEMENTAL_ARROWS_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B
                = new UnicodeBlock("MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B",
                    MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_MATHEMATICAL_OPERATORS
                = new UnicodeBlock("SUPPLEMENTAL_MATHEMATICAL_OPERATORS",
                    SUPPLEMENTAL_MATHEMATICAL_OPERATORS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock KATAKANA_PHONETIC_EXTENSIONS
                = new UnicodeBlock("KATAKANA_PHONETIC_EXTENSIONS", KATAKANA_PHONETIC_EXTENSIONS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock VARIATION_SELECTORS
                = new UnicodeBlock("VARIATION_SELECTORS", VARIATION_SELECTORS_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTARY_PRIVATE_USE_AREA_A
                = new UnicodeBlock("SUPPLEMENTARY_PRIVATE_USE_AREA_A",
                    SUPPLEMENTARY_PRIVATE_USE_AREA_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTARY_PRIVATE_USE_AREA_B
                = new UnicodeBlock("SUPPLEMENTARY_PRIVATE_USE_AREA_B",
                    SUPPLEMENTARY_PRIVATE_USE_AREA_B_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock LIMBU
                = new UnicodeBlock("LIMBU", LIMBU_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock TAI_LE
                = new UnicodeBlock("TAI_LE", TAI_LE_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock KHMER_SYMBOLS
                = new UnicodeBlock("KHMER_SYMBOLS", KHMER_SYMBOLS_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock PHONETIC_EXTENSIONS
                = new UnicodeBlock("PHONETIC_EXTENSIONS", PHONETIC_EXTENSIONS_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS_AND_ARROWS
                = new UnicodeBlock("MISCELLANEOUS_SYMBOLS_AND_ARROWS",
                    MISCELLANEOUS_SYMBOLS_AND_ARROWS_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock YIJING_HEXAGRAM_SYMBOLS
                = new UnicodeBlock("YIJING_HEXAGRAM_SYMBOLS", YIJING_HEXAGRAM_SYMBOLS_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock LINEAR_B_SYLLABARY
                = new UnicodeBlock("LINEAR_B_SYLLABARY", LINEAR_B_SYLLABARY_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock LINEAR_B_IDEOGRAMS
                = new UnicodeBlock("LINEAR_B_IDEOGRAMS", LINEAR_B_IDEOGRAMS_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock AEGEAN_NUMBERS
                = new UnicodeBlock("AEGEAN_NUMBERS", AEGEAN_NUMBERS_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock UGARITIC
                = new UnicodeBlock("UGARITIC", UGARITIC_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock SHAVIAN
                = new UnicodeBlock("SHAVIAN", SHAVIAN_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock OSMANYA
                = new UnicodeBlock("OSMANYA", OSMANYA_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock CYPRIOT_SYLLABARY
                = new UnicodeBlock("CYPRIOT_SYLLABARY", CYPRIOT_SYLLABARY_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock TAI_XUAN_JING_SYMBOLS
                = new UnicodeBlock("TAI_XUAN_JING_SYMBOLS", TAI_XUAN_JING_SYMBOLS_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock VARIATION_SELECTORS_SUPPLEMENT
                = new UnicodeBlock("VARIATION_SELECTORS_SUPPLEMENT", VARIATION_SELECTORS_SUPPLEMENT_ID);

            /* New blocks in Unicode 4.1 */

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock ANCIENT_GREEK_MUSICAL_NOTATION =
                    new UnicodeBlock("ANCIENT_GREEK_MUSICAL_NOTATION",
                            ANCIENT_GREEK_MUSICAL_NOTATION_ID); /*[1D200]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock ANCIENT_GREEK_NUMBERS =
                    new UnicodeBlock("ANCIENT_GREEK_NUMBERS", ANCIENT_GREEK_NUMBERS_ID); /*[10140]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock ARABIC_SUPPLEMENT =
                    new UnicodeBlock("ARABIC_SUPPLEMENT", ARABIC_SUPPLEMENT_ID); /*[0750]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock BUGINESE =
                    new UnicodeBlock("BUGINESE", BUGINESE_ID); /*[1A00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock CJK_STROKES =
                    new UnicodeBlock("CJK_STROKES", CJK_STROKES_ID); /*[31C0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS_SUPPLEMENT =
                    new UnicodeBlock("COMBINING_DIACRITICAL_MARKS_SUPPLEMENT",
                            COMBINING_DIACRITICAL_MARKS_SUPPLEMENT_ID); /*[1DC0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock COPTIC = new UnicodeBlock("COPTIC", COPTIC_ID); /*[2C80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock ETHIOPIC_EXTENDED =
                    new UnicodeBlock("ETHIOPIC_EXTENDED", ETHIOPIC_EXTENDED_ID); /*[2D80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock ETHIOPIC_SUPPLEMENT =
                    new UnicodeBlock("ETHIOPIC_SUPPLEMENT", ETHIOPIC_SUPPLEMENT_ID); /*[1380]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock GEORGIAN_SUPPLEMENT =
                    new UnicodeBlock("GEORGIAN_SUPPLEMENT", GEORGIAN_SUPPLEMENT_ID); /*[2D00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock GLAGOLITIC =
                    new UnicodeBlock("GLAGOLITIC", GLAGOLITIC_ID); /*[2C00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock KHAROSHTHI =
                    new UnicodeBlock("KHAROSHTHI", KHAROSHTHI_ID); /*[10A00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock MODIFIER_TONE_LETTERS =
                    new UnicodeBlock("MODIFIER_TONE_LETTERS", MODIFIER_TONE_LETTERS_ID); /*[A700]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock NEW_TAI_LUE =
                    new UnicodeBlock("NEW_TAI_LUE", NEW_TAI_LUE_ID); /*[1980]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock OLD_PERSIAN =
                    new UnicodeBlock("OLD_PERSIAN", OLD_PERSIAN_ID); /*[103A0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock PHONETIC_EXTENSIONS_SUPPLEMENT =
                    new UnicodeBlock("PHONETIC_EXTENSIONS_SUPPLEMENT",
                            PHONETIC_EXTENSIONS_SUPPLEMENT_ID); /*[1D80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_PUNCTUATION =
                    new UnicodeBlock("SUPPLEMENTAL_PUNCTUATION", SUPPLEMENTAL_PUNCTUATION_ID); /*[2E00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock SYLOTI_NAGRI =
                    new UnicodeBlock("SYLOTI_NAGRI", SYLOTI_NAGRI_ID); /*[A800]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock TIFINAGH =
                    new UnicodeBlock("TIFINAGH", TIFINAGH_ID); /*[2D30]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock VERTICAL_FORMS =
                    new UnicodeBlock("VERTICAL_FORMS", VERTICAL_FORMS_ID); /*[FE10]*/

            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock NKO = new UnicodeBlock("NKO", NKO_ID); /*[07C0]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock BALINESE =
                    new UnicodeBlock("BALINESE", BALINESE_ID); /*[1B00]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_C =
                    new UnicodeBlock("LATIN_EXTENDED_C", LATIN_EXTENDED_C_ID); /*[2C60]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_D =
                    new UnicodeBlock("LATIN_EXTENDED_D", LATIN_EXTENDED_D_ID); /*[A720]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock PHAGS_PA =
                    new UnicodeBlock("PHAGS_PA", PHAGS_PA_ID); /*[A840]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock PHOENICIAN =
                    new UnicodeBlock("PHOENICIAN", PHOENICIAN_ID); /*[10900]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock CUNEIFORM =
                    new UnicodeBlock("CUNEIFORM", CUNEIFORM_ID); /*[12000]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock CUNEIFORM_NUMBERS_AND_PUNCTUATION =
                    new UnicodeBlock("CUNEIFORM_NUMBERS_AND_PUNCTUATION",
                            CUNEIFORM_NUMBERS_AND_PUNCTUATION_ID); /*[12400]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock COUNTING_ROD_NUMERALS =
                    new UnicodeBlock("COUNTING_ROD_NUMERALS", COUNTING_ROD_NUMERALS_ID); /*[1D360]*/

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock SUNDANESE =
                    new UnicodeBlock("SUNDANESE", SUNDANESE_ID); /* [1B80] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock LEPCHA =
                    new UnicodeBlock("LEPCHA", LEPCHA_ID); /* [1C00] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock OL_CHIKI =
                    new UnicodeBlock("OL_CHIKI", OL_CHIKI_ID); /* [1C50] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_A =
                    new UnicodeBlock("CYRILLIC_EXTENDED_A", CYRILLIC_EXTENDED_A_ID); /* [2DE0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock VAI = new UnicodeBlock("VAI", VAI_ID); /* [A500] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_B =
                    new UnicodeBlock("CYRILLIC_EXTENDED_B", CYRILLIC_EXTENDED_B_ID); /* [A640] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock SAURASHTRA =
                    new UnicodeBlock("SAURASHTRA", SAURASHTRA_ID); /* [A880] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock KAYAH_LI =
                    new UnicodeBlock("KAYAH_LI", KAYAH_LI_ID); /* [A900] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock REJANG =
                    new UnicodeBlock("REJANG", REJANG_ID); /* [A930] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock CHAM =
                    new UnicodeBlock("CHAM", CHAM_ID); /* [AA00] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock ANCIENT_SYMBOLS =
                    new UnicodeBlock("ANCIENT_SYMBOLS", ANCIENT_SYMBOLS_ID); /* [10190] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock PHAISTOS_DISC =
                    new UnicodeBlock("PHAISTOS_DISC", PHAISTOS_DISC_ID); /* [101D0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock LYCIAN =
                    new UnicodeBlock("LYCIAN", LYCIAN_ID); /* [10280] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock CARIAN =
                    new UnicodeBlock("CARIAN", CARIAN_ID); /* [102A0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock LYDIAN =
                    new UnicodeBlock("LYDIAN", LYDIAN_ID); /* [10920] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock MAHJONG_TILES =
                    new UnicodeBlock("MAHJONG_TILES", MAHJONG_TILES_ID); /* [1F000] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock DOMINO_TILES =
                    new UnicodeBlock("DOMINO_TILES", DOMINO_TILES_ID); /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock SAMARITAN =
                    new UnicodeBlock("SAMARITAN", SAMARITAN_ID); /*[0800]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED =
                    new UnicodeBlock("UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED",
                            UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED_ID); /*[18B0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock TAI_THAM =
                    new UnicodeBlock("TAI_THAM", TAI_THAM_ID); /*[1A20]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock VEDIC_EXTENSIONS =
                    new UnicodeBlock("VEDIC_EXTENSIONS", VEDIC_EXTENSIONS_ID); /*[1CD0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock LISU =
                    new UnicodeBlock("LISU", LISU_ID); /*[A4D0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock BAMUM =
                    new UnicodeBlock("BAMUM", BAMUM_ID); /*[A6A0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock COMMON_INDIC_NUMBER_FORMS =
                    new UnicodeBlock("COMMON_INDIC_NUMBER_FORMS", COMMON_INDIC_NUMBER_FORMS_ID); /*[A830]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock DEVANAGARI_EXTENDED =
                    new UnicodeBlock("DEVANAGARI_EXTENDED", DEVANAGARI_EXTENDED_ID); /*[A8E0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock HANGUL_JAMO_EXTENDED_A =
                    new UnicodeBlock("HANGUL_JAMO_EXTENDED_A", HANGUL_JAMO_EXTENDED_A_ID); /*[A960]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock JAVANESE =
                    new UnicodeBlock("JAVANESE", JAVANESE_ID); /*[A980]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock MYANMAR_EXTENDED_A =
                    new UnicodeBlock("MYANMAR_EXTENDED_A", MYANMAR_EXTENDED_A_ID); /*[AA60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock TAI_VIET =
                    new UnicodeBlock("TAI_VIET", TAI_VIET_ID); /*[AA80]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock MEETEI_MAYEK =
                    new UnicodeBlock("MEETEI_MAYEK", MEETEI_MAYEK_ID); /*[ABC0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock HANGUL_JAMO_EXTENDED_B =
                    new UnicodeBlock("HANGUL_JAMO_EXTENDED_B", HANGUL_JAMO_EXTENDED_B_ID); /*[D7B0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock IMPERIAL_ARAMAIC =
                    new UnicodeBlock("IMPERIAL_ARAMAIC", IMPERIAL_ARAMAIC_ID); /*[10840]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock OLD_SOUTH_ARABIAN =
                    new UnicodeBlock("OLD_SOUTH_ARABIAN", OLD_SOUTH_ARABIAN_ID); /*[10A60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock AVESTAN =
                    new UnicodeBlock("AVESTAN", AVESTAN_ID); /*[10B00]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock INSCRIPTIONAL_PARTHIAN =
                    new UnicodeBlock("INSCRIPTIONAL_PARTHIAN", INSCRIPTIONAL_PARTHIAN_ID); /*[10B40]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock INSCRIPTIONAL_PAHLAVI =
                    new UnicodeBlock("INSCRIPTIONAL_PAHLAVI", INSCRIPTIONAL_PAHLAVI_ID); /*[10B60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock OLD_TURKIC =
                    new UnicodeBlock("OLD_TURKIC", OLD_TURKIC_ID); /*[10C00]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock RUMI_NUMERAL_SYMBOLS =
                    new UnicodeBlock("RUMI_NUMERAL_SYMBOLS", RUMI_NUMERAL_SYMBOLS_ID); /*[10E60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock KAITHI =
                    new UnicodeBlock("KAITHI", KAITHI_ID); /*[11080]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock EGYPTIAN_HIEROGLYPHS =
                    new UnicodeBlock("EGYPTIAN_HIEROGLYPHS", EGYPTIAN_HIEROGLYPHS_ID); /*[13000]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock ENCLOSED_ALPHANUMERIC_SUPPLEMENT =
                    new UnicodeBlock("ENCLOSED_ALPHANUMERIC_SUPPLEMENT",
                            ENCLOSED_ALPHANUMERIC_SUPPLEMENT_ID); /*[1F100]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock ENCLOSED_IDEOGRAPHIC_SUPPLEMENT =
                    new UnicodeBlock("ENCLOSED_IDEOGRAPHIC_SUPPLEMENT",
                            ENCLOSED_IDEOGRAPHIC_SUPPLEMENT_ID); /*[1F200]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C_ID); /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock MANDAIC =
                    new UnicodeBlock("MANDAIC", MANDAIC_ID); /*[0840]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock BATAK =
                    new UnicodeBlock("BATAK", BATAK_ID); /*[1BC0]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock ETHIOPIC_EXTENDED_A =
                    new UnicodeBlock("ETHIOPIC_EXTENDED_A", ETHIOPIC_EXTENDED_A_ID); /*[AB00]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock BRAHMI =
                    new UnicodeBlock("BRAHMI", BRAHMI_ID); /*[11000]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock BAMUM_SUPPLEMENT =
                    new UnicodeBlock("BAMUM_SUPPLEMENT", BAMUM_SUPPLEMENT_ID); /*[16800]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock KANA_SUPPLEMENT =
                    new UnicodeBlock("KANA_SUPPLEMENT", KANA_SUPPLEMENT_ID); /*[1B000]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock PLAYING_CARDS =
                    new UnicodeBlock("PLAYING_CARDS", PLAYING_CARDS_ID); /*[1F0A0]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS =
                    new UnicodeBlock("MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS",
                            MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS_ID); /*[1F300]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock EMOTICONS =
                    new UnicodeBlock("EMOTICONS", EMOTICONS_ID); /*[1F600]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock TRANSPORT_AND_MAP_SYMBOLS =
                    new UnicodeBlock("TRANSPORT_AND_MAP_SYMBOLS", TRANSPORT_AND_MAP_SYMBOLS_ID); /*[1F680]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock ALCHEMICAL_SYMBOLS =
                    new UnicodeBlock("ALCHEMICAL_SYMBOLS", ALCHEMICAL_SYMBOLS_ID); /*[1F700]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D_ID); /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock ARABIC_EXTENDED_A =
                    new UnicodeBlock("ARABIC_EXTENDED_A", ARABIC_EXTENDED_A_ID); /*[08A0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS =
                    new UnicodeBlock("ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS", ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS_ID); /*[1EE00]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock CHAKMA = new UnicodeBlock("CHAKMA", CHAKMA_ID); /*[11100]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock MEETEI_MAYEK_EXTENSIONS =
                    new UnicodeBlock("MEETEI_MAYEK_EXTENSIONS", MEETEI_MAYEK_EXTENSIONS_ID); /*[AAE0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock MEROITIC_CURSIVE =
                    new UnicodeBlock("MEROITIC_CURSIVE", MEROITIC_CURSIVE_ID); /*[109A0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock MEROITIC_HIEROGLYPHS =
                    new UnicodeBlock("MEROITIC_HIEROGLYPHS", MEROITIC_HIEROGLYPHS_ID); /*[10980]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock MIAO = new UnicodeBlock("MIAO", MIAO_ID); /*[16F00]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock SHARADA = new UnicodeBlock("SHARADA", SHARADA_ID); /*[11180]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock SORA_SOMPENG =
                    new UnicodeBlock("SORA_SOMPENG", SORA_SOMPENG_ID); /*[110D0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock SUNDANESE_SUPPLEMENT =
                    new UnicodeBlock("SUNDANESE_SUPPLEMENT", SUNDANESE_SUPPLEMENT_ID); /*[1CC0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock TAKRI = new UnicodeBlock("TAKRI", TAKRI_ID); /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock BASSA_VAH = new UnicodeBlock("BASSA_VAH", BASSA_VAH_ID); /*[16AD0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock CAUCASIAN_ALBANIAN =
                    new UnicodeBlock("CAUCASIAN_ALBANIAN", CAUCASIAN_ALBANIAN_ID); /*[10530]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock COPTIC_EPACT_NUMBERS =
                    new UnicodeBlock("COPTIC_EPACT_NUMBERS", COPTIC_EPACT_NUMBERS_ID); /*[102E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS_EXTENDED =
                    new UnicodeBlock("COMBINING_DIACRITICAL_MARKS_EXTENDED", COMBINING_DIACRITICAL_MARKS_EXTENDED_ID); /*[1AB0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock DUPLOYAN = new UnicodeBlock("DUPLOYAN", DUPLOYAN_ID); /*[1BC00]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock ELBASAN = new UnicodeBlock("ELBASAN", ELBASAN_ID); /*[10500]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock GEOMETRIC_SHAPES_EXTENDED =
                    new UnicodeBlock("GEOMETRIC_SHAPES_EXTENDED", GEOMETRIC_SHAPES_EXTENDED_ID); /*[1F780]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock GRANTHA = new UnicodeBlock("GRANTHA", GRANTHA_ID); /*[11300]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock KHOJKI = new UnicodeBlock("KHOJKI", KHOJKI_ID); /*[11200]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock KHUDAWADI = new UnicodeBlock("KHUDAWADI", KHUDAWADI_ID); /*[112B0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock LATIN_EXTENDED_E =
                    new UnicodeBlock("LATIN_EXTENDED_E", LATIN_EXTENDED_E_ID); /*[AB30]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock LINEAR_A = new UnicodeBlock("LINEAR_A", LINEAR_A_ID); /*[10600]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MAHAJANI = new UnicodeBlock("MAHAJANI", MAHAJANI_ID); /*[11150]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MANICHAEAN = new UnicodeBlock("MANICHAEAN", MANICHAEAN_ID); /*[10AC0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MENDE_KIKAKUI =
                    new UnicodeBlock("MENDE_KIKAKUI", MENDE_KIKAKUI_ID); /*[1E800]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MODI = new UnicodeBlock("MODI", MODI_ID); /*[11600]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MRO = new UnicodeBlock("MRO", MRO_ID); /*[16A40]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock MYANMAR_EXTENDED_B =
                    new UnicodeBlock("MYANMAR_EXTENDED_B", MYANMAR_EXTENDED_B_ID); /*[A9E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock NABATAEAN = new UnicodeBlock("NABATAEAN", NABATAEAN_ID); /*[10880]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock OLD_NORTH_ARABIAN =
                    new UnicodeBlock("OLD_NORTH_ARABIAN", OLD_NORTH_ARABIAN_ID); /*[10A80]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock OLD_PERMIC = new UnicodeBlock("OLD_PERMIC", OLD_PERMIC_ID); /*[10350]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock ORNAMENTAL_DINGBATS =
                    new UnicodeBlock("ORNAMENTAL_DINGBATS", ORNAMENTAL_DINGBATS_ID); /*[1F650]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock PAHAWH_HMONG = new UnicodeBlock("PAHAWH_HMONG", PAHAWH_HMONG_ID); /*[16B00]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock PALMYRENE = new UnicodeBlock("PALMYRENE", PALMYRENE_ID); /*[10860]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock PAU_CIN_HAU = new UnicodeBlock("PAU_CIN_HAU", PAU_CIN_HAU_ID); /*[11AC0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock PSALTER_PAHLAVI =
                    new UnicodeBlock("PSALTER_PAHLAVI", PSALTER_PAHLAVI_ID); /*[10B80]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock SHORTHAND_FORMAT_CONTROLS =
                    new UnicodeBlock("SHORTHAND_FORMAT_CONTROLS", SHORTHAND_FORMAT_CONTROLS_ID); /*[1BCA0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock SIDDHAM = new UnicodeBlock("SIDDHAM", SIDDHAM_ID); /*[11580]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock SINHALA_ARCHAIC_NUMBERS =
                    new UnicodeBlock("SINHALA_ARCHAIC_NUMBERS", SINHALA_ARCHAIC_NUMBERS_ID); /*[111E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_C =
                    new UnicodeBlock("SUPPLEMENTAL_ARROWS_C", SUPPLEMENTAL_ARROWS_C_ID); /*[1F800]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock TIRHUTA = new UnicodeBlock("TIRHUTA", TIRHUTA_ID); /*[11480]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock WARANG_CITI = new UnicodeBlock("WARANG_CITI", WARANG_CITI_ID); /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock AHOM = new UnicodeBlock("AHOM", AHOM_ID); /*[11700]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock ANATOLIAN_HIEROGLYPHS =
                    new UnicodeBlock("ANATOLIAN_HIEROGLYPHS", ANATOLIAN_HIEROGLYPHS_ID); /*[14400]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock CHEROKEE_SUPPLEMENT =
                    new UnicodeBlock("CHEROKEE_SUPPLEMENT", CHEROKEE_SUPPLEMENT_ID); /*[AB70]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E_ID); /*[2B820]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock EARLY_DYNASTIC_CUNEIFORM =
                    new UnicodeBlock("EARLY_DYNASTIC_CUNEIFORM", EARLY_DYNASTIC_CUNEIFORM_ID); /*[12480]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock HATRAN = new UnicodeBlock("HATRAN", HATRAN_ID); /*[108E0]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock MULTANI = new UnicodeBlock("MULTANI", MULTANI_ID); /*[11280]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock OLD_HUNGARIAN =
                    new UnicodeBlock("OLD_HUNGARIAN", OLD_HUNGARIAN_ID); /*[10C80]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS =
                    new UnicodeBlock("SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS",
                            SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS_ID); /*[1F900]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock SUTTON_SIGNWRITING =
                    new UnicodeBlock("SUTTON_SIGNWRITING", SUTTON_SIGNWRITING_ID); /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock ADLAM = new UnicodeBlock("ADLAM", ADLAM_ID); /*[1E900]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock BHAIKSUKI = new UnicodeBlock("BHAIKSUKI", BHAIKSUKI_ID); /*[11C00]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_C =
                    new UnicodeBlock("CYRILLIC_EXTENDED_C", CYRILLIC_EXTENDED_C_ID); /*[1C80]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock GLAGOLITIC_SUPPLEMENT =
                    new UnicodeBlock("GLAGOLITIC_SUPPLEMENT", GLAGOLITIC_SUPPLEMENT_ID); /*[1E000]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION =
                    new UnicodeBlock("IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION", IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION_ID); /*[16FE0]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock MARCHEN = new UnicodeBlock("MARCHEN", MARCHEN_ID); /*[11C70]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock MONGOLIAN_SUPPLEMENT =
                    new UnicodeBlock("MONGOLIAN_SUPPLEMENT", MONGOLIAN_SUPPLEMENT_ID); /*[11660]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock NEWA = new UnicodeBlock("NEWA", NEWA_ID); /*[11400]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock OSAGE = new UnicodeBlock("OSAGE", OSAGE_ID); /*[104B0]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock TANGUT = new UnicodeBlock("TANGUT", TANGUT_ID); /*[17000]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock TANGUT_COMPONENTS =
                    new UnicodeBlock("TANGUT_COMPONENTS", TANGUT_COMPONENTS_ID); /*[18800]*/

            // New blocks in Unicode 10.0

            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F", CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F_ID); /*[2CEB0]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock KANA_EXTENDED_A =
                    new UnicodeBlock("KANA_EXTENDED_A", KANA_EXTENDED_A_ID); /*[1B100]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock MASARAM_GONDI =
                    new UnicodeBlock("MASARAM_GONDI", MASARAM_GONDI_ID); /*[11D00]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock NUSHU = new UnicodeBlock("NUSHU", NUSHU_ID); /*[1B170]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock SOYOMBO = new UnicodeBlock("SOYOMBO", SOYOMBO_ID); /*[11A50]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock SYRIAC_SUPPLEMENT =
                    new UnicodeBlock("SYRIAC_SUPPLEMENT", SYRIAC_SUPPLEMENT_ID); /*[0860]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock ZANABAZAR_SQUARE =
                    new UnicodeBlock("ZANABAZAR_SQUARE", ZANABAZAR_SQUARE_ID); /*[11A00]*/

            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock INVALID_CODE
                = new UnicodeBlock("INVALID_CODE", INVALID_CODE_ID);

            static UnicodeBlock()
            {
#pragma warning disable 612, 618
                for (int blockId = 0; blockId < COUNT; ++blockId)
#pragma warning restore 612, 618
                {
                    if (BLOCKS_[blockId] == null)
                    {
                        throw new InvalidOperationException(
                                "UnicodeBlock.BLOCKS_[" + blockId + "] not initialized");
                    }
                }
            }

            // public methods --------------------------------------------------

            /// <icu/>
            /// <summary>
            /// Returns the only instance of the <see cref="UnicodeBlock"/> with the argument ID.
            /// If no such ID exists, a <see cref="INVALID_CODE"/> <see cref="UnicodeBlock"/> will be returned.
            /// </summary>
            /// <param name="id"><see cref="UnicodeBlock"/> ID.</param>
            /// <returns>the only instance of the <see cref="UnicodeBlock"/> with the argument ID
            /// if it exists, otherwise a <see cref="INVALID_CODE"/> <see cref="UnicodeBlock"/> will be
            /// returned.
            /// </returns>
            /// <stable>ICU 2.4</stable>
            public static UnicodeBlock GetInstance(int id)
            {
                if (id >= 0 && id < BLOCKS_.Length)
                {
                    return BLOCKS_[id];
                }
                return INVALID_CODE;
            }

            /// <summary>
            /// Returns the Unicode allocation block that contains the code point,
            /// or null if the code point is not a member of a defined block.
            /// </summary>
            /// <param name="ch">Code point to be tested.</param>
            /// <returns>The Unicode allocation block that contains the code point.</returns>
            /// <stable>ICU 2.4</stable>
            public static UnicodeBlock Of(int ch)
            {
                if (ch > MaxValue)
                {
                    return INVALID_CODE;
                }

                return UnicodeBlock.GetInstance(
                        UCharacterProperty.Instance.GetInt32PropertyValue(ch, (int)UProperty.Block));
            }

            /// <summary>
            /// Returns the Unicode block with the given name. This matches
            /// against the official UCD name (ignoring case).
            /// </summary>
            /// <param name="blockName">The name of the block to match.</param>
            /// <returns>The <see cref="UnicodeBlock"/> with that name.</returns>
            /// <exception cref="ArgumentException">If the blockName could not be matched.</exception>
            /// <stable>ICU 3.0</stable>
            public static UnicodeBlock ForName(string blockName) // ICU4N TODO: API - rename GetInstance() ? ForName is a Java-ism.
            {
                IDictionary<string, UnicodeBlock> m = null;
                if (mref != null)
                {
                    //m = mref.Get();
                    mref.TryGetTarget(out m);
                }
                if (m == null)
                {
                    m = new Dictionary<string, UnicodeBlock>(BLOCKS_.Length);
                    for (int i = 0; i < BLOCKS_.Length; ++i)
                    {
                        UnicodeBlock b2 = BLOCKS_[i];
                        string name = TrimBlockName(
                                GetPropertyValueName(UProperty.Block, b2.ID,
                                        NameChoice.Long));
                        m[name] = b2;
                    }
                    mref = new WeakReference<IDictionary<string, UnicodeBlock>>(m);
                }
                UnicodeBlock b = m[TrimBlockName(blockName)];
                if (b == null)
                {
                    throw new ArgumentException();
                }
                return b;
            }
            private static WeakReference<IDictionary<string, UnicodeBlock>> mref;

            private static string TrimBlockName(string name)
            {
                string upper = name.ToUpperInvariant();
                StringBuilder result = new StringBuilder(upper.Length);
                for (int i = 0; i < upper.Length; i++)
                {
                    char c = upper[i];
                    if (c != ' ' && c != '_' && c != '-')
                    {
                        result.Append(c);
                    }
                }
                return result.ToString();
            }

            /// <icu/>
            /// <summary>
            /// Gets the type ID of this Unicode block.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public int ID
            {
                get { return m_id_; }
            }

            /// <summary>
            /// Returns the name of this <see cref="UnicodeBlock"/>.
            /// </summary>
            // ICU4N specific - we don't have a Character.Subset base class, so
            // this functionality was moved here.
            public override string ToString()
            {
                return name;
            }

            // private data members ---------------------------------------------

            /// <summary>
            /// Identification code for this <see cref="UnicodeBlock"/>
            /// </summary>
            private int m_id_;

            /// <summary>
            /// Name for this <see cref="UnicodeBlock"/>
            /// </summary>
            private string name;

            // private constructor ----------------------------------------------

            /// <summary>
            /// <see cref="UnicodeBlock"/> constructor.
            /// </summary>
            /// <param name="name">Name of this <see cref="UnicodeBlock"/>.</param>
            /// <param name="id">Unique id of this <see cref="UnicodeBlock"/>.</param>
            /// <exception cref="ArgumentNullException">If name is <c>null</c>.</exception>
            private UnicodeBlock(string name, int id)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                this.name = name;
                m_id_ = id;
                if (id >= 0)
                {
                    BLOCKS_[id] = this;
                }
            }
        }

        /// <summary>
        /// East Asian Width constants.
        /// </summary>
        /// <seealso cref="UProperty.East_Asian_Width"/>
        /// <seealso cref="UChar.GetInt32PropertyValue(int, UProperty)"/>
        /// <stable>ICU 2.4</stable>
        public static class EastAsianWidth
        {
            /// <stable>ICU 2.4</stable>
            public const int Neutral = 0;
            /// <stable>ICU 2.4</stable>
            public const int Ambiguous = 1;
            /// <stable>ICU 2.4</stable>
            public const int HalfWidth = 2;
            /// <stable>ICU 2.4</stable>
            public const int FullWidth = 3;
            /// <stable>ICU 2.4</stable>
            public const int Narrow = 4;
            /// <stable>ICU 2.4</stable>
            public const int Wide = 5;
            /// <summary>
            /// One more than the highest normal <see cref="EastAsianWidth"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.East_Asian_Width"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 6;
        }

        /// <summary>
        /// Decomposition Type constants.
        /// </summary>
        /// <seealso cref="UProperty.Decomposition_Type"/>
        /// <stable>ICU 2.4</stable>
        public static class DecompositionType
        {
            /// <stable>ICU 2.4</stable>
            public const int None = 0;
            /// <stable>ICU 2.4</stable>
            public const int Canonical = 1;
            /// <stable>ICU 2.4</stable>
            public const int Compat = 2;
            /// <stable>ICU 2.4</stable>
            public const int Circle = 3;
            /// <stable>ICU 2.4</stable>
            public const int Final = 4;
            /// <stable>ICU 2.4</stable>
            public const int Font = 5;
            /// <stable>ICU 2.4</stable>
            public const int Fraction = 6;
            /// <stable>ICU 2.4</stable>
            public const int Initial = 7;
            /// <stable>ICU 2.4</stable>
            public const int Isolated = 8;
            /// <stable>ICU 2.4</stable>
            public const int Medial = 9;
            /// <stable>ICU 2.4</stable>
            public const int Narrow = 10;
            /// <stable>ICU 2.4</stable>
            public const int NoBreak = 11;
            /// <stable>ICU 2.4</stable>
            public const int Small = 12;
            /// <stable>ICU 2.4</stable>
            public const int Square = 13;
            /// <stable>ICU 2.4</stable>
            public const int Sub = 14;
            /// <stable>ICU 2.4</stable>
            public const int Super = 15;
            /// <stable>ICU 2.4</stable>
            public const int Vertical = 16;
            /// <stable>ICU 2.4</stable>
            public const int Wide = 17;
            /// <summary>
            /// One more than the highest normal <see cref="DecompositionType"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Decomposition_Type"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int COUNT = 18;
        }

        /// <summary>
        /// Joining Type constants.
        /// </summary>
        /// <seealso cref="UProperty.Joining_Type"/>
        /// <stable>ICU 2.4</stable>
        public static class JoiningType
        {
            /// <stable>ICU 2.4</stable>
            public const int NonJoining = 0;
            /// <stable>ICU 2.4</stable>
            public const int JoinCausing = 1;
            /// <stable>ICU 2.4</stable>
            public const int DualJoining = 2;
            /// <stable>ICU 2.4</stable>
            public const int LeftJoining = 3;
            /// <stable>ICU 2.4</stable>
            public const int RightJoining = 4;
            /// <stable>ICU 2.4</stable>
            public const int Transparent = 5;
            /// <summary>
            /// One more than the highest normal <see cref="JoiningType"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Joining_Type"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 6;
        }

        /// <summary>
        /// Joining Group constants.
        /// </summary>
        /// <seealso cref="UProperty.Joining_Group"/>
        /// <stable>ICU 2.4</stable>
        public static class JoiningGroup
        {
            /// <stable>ICU 2.4</stable>
            public const int NoJoiningGroup = 0;
            /// <stable>ICU 2.4</stable>
            public const int Ain = 1;
            /// <stable>ICU 2.4</stable>
            public const int Alaph = 2;
            /// <stable>ICU 2.4</stable>
            public const int Alef = 3;
            /// <stable>ICU 2.4</stable>
            public const int Beh = 4;
            /// <stable>ICU 2.4</stable>
            public const int Beth = 5;
            /// <stable>ICU 2.4</stable>
            public const int Dal = 6;
            /// <stable>ICU 2.4</stable>
            public const int DalathRish = 7;
            /// <stable>ICU 2.4</stable>
            public const int E = 8;
            /// <stable>ICU 2.4</stable>
            public const int Feh = 9;
            /// <stable>ICU 2.4</stable>
            public const int FinalSemkath = 10;
            /// <stable>ICU 2.4</stable>
            public const int Gaf = 11;
            /// <stable>ICU 2.4</stable>
            public const int Gamal = 12;
            /// <stable>ICU 2.4</stable>
            public const int Hah = 13;
            /// <stable>ICU 4.6</stable>
            public const int TehMarbutaGoal = 14;
            /// <stable>ICU 2.4</stable>
            public const int HamzaOnHehGoal = TehMarbutaGoal;
            /// <stable>ICU 2.4</stable>
            public const int He = 15;
            /// <stable>ICU 2.4</stable>
            public const int Heh = 16;
            /// <stable>ICU 2.4</stable>
            public const int HehGoal = 17;
            /// <stable>ICU 2.4</stable>
            public const int Heth = 18;
            /// <stable>ICU 2.4</stable>
            public const int Kaf = 19;
            /// <stable>ICU 2.4</stable>
            public const int Kaph = 20;
            /// <stable>ICU 2.4</stable>
            public const int KnottedHeh = 21;
            /// <stable>ICU 2.4</stable>
            public const int Lam = 22;
            /// <stable>ICU 2.4</stable>
            public const int Lamadh = 23;
            /// <stable>ICU 2.4</stable>
            public const int Meem = 24;
            /// <stable>ICU 2.4</stable>
            public const int Mim = 25;
            /// <stable>ICU 2.4</stable>
            public const int Noon = 26;
            /// <stable>ICU 2.4</stable>
            public const int Nun = 27;
            /// <stable>ICU 2.4</stable>
            public const int Pe = 28;
            /// <stable>ICU 2.4</stable>
            public const int Qaf = 29;
            /// <stable>ICU 2.4</stable>
            public const int Qaph = 30;
            /// <stable>ICU 2.4</stable>
            public const int Reh = 31;
            /// <stable>ICU 2.4</stable>
            public const int ReversedPe = 32;
            /// <stable>ICU 2.4</stable>
            public const int Sad = 33;
            /// <stable>ICU 2.4</stable>
            public const int Sadhe = 34;
            /// <stable>ICU 2.4</stable>
            public const int Seen = 35;
            /// <stable>ICU 2.4</stable>
            public const int Semkath = 36;
            /// <stable>ICU 2.4</stable>
            public const int Shin = 37;
            /// <stable>ICU 2.4</stable>
            public const int SwashKaf = 38;
            /// <stable>ICU 2.4</stable>
            public const int SyriacWaw = 39;
            /// <stable>ICU 2.4</stable>
            public const int Tah = 40;
            /// <stable>ICU 2.4</stable>
            public const int Taw = 41;
            /// <stable>ICU 2.4</stable>
            public const int TehMarbuta = 42;
            /// <stable>ICU 2.4</stable>
            public const int Teth = 43;
            /// <stable>ICU 2.4</stable>
            public const int Waw = 44;
            /// <stable>ICU 2.4</stable>
            public const int Yeh = 45;
            /// <stable>ICU 2.4</stable>
            public const int YehBarree = 46;
            /// <stable>ICU 2.4</stable>
            public const int YehWithTail = 47;
            /// <stable>ICU 2.4</stable>
            public const int Yudh = 48;
            /// <stable>ICU 2.4</stable>
            public const int YudhHe = 49;
            /// <stable>ICU 2.4</stable>
            public const int Zain = 50;
            /// <stable>ICU 2.6</stable>
            public const int Fe = 51;
            /// <stable>ICU 2.6</stable>
            public const int Khaph = 52;
            /// <stable>ICU 2.6</stable>
            public const int Zhain = 53;
            /// <stable>ICU 4.0</stable>
            public const int BurushaskiYehBarree = 54;
            /// <stable>ICU 4.4</stable>
            public const int FarsiYeh = 55;
            /// <stable>ICU 4.4</stable>
            public const int Nya = 56;
            /// <stable>ICU 49</stable>
            public const int RohingyaYeh = 57;

            /// <stable>ICU 54</stable>
            public const int ManichaeanAleph = 58;
            /// <stable>ICU 54</stable>
            public const int ManichaeanAyin = 59;
            /// <stable>ICU 54</stable>
            public const int ManichaeanBeth = 60;
            /// <stable>ICU 54</stable>
            public const int ManichaeanDaleth = 61;
            /// <stable>ICU 54</stable>
            public const int ManichaeanDhamedh = 62;
            /// <stable>ICU 54</stable>
            public const int ManichaeanFive = 63;
            /// <stable>ICU 54</stable>
            public const int ManichaeanGimel = 64;
            /// <stable>ICU 54</stable>
            public const int ManichaeanHeth = 65;
            /// <stable>ICU 54</stable>
            public const int ManichaeanHundred = 66;
            /// <stable>ICU 54</stable>
            public const int ManichaeanKaph = 67;
            /// <stable>ICU 54</stable>
            public const int ManichaeanLamedh = 68;
            /// <stable>ICU 54</stable>
            public const int ManichaeanMem = 69;
            /// <stable>ICU 54</stable>
            public const int ManichaeanNun = 70;
            /// <stable>ICU 54</stable>
            public const int ManichaeanOne = 71;
            /// <stable>ICU 54</stable>
            public const int ManichaeanPe = 72;
            /// <stable>ICU 54</stable>
            public const int ManichaeanQoph = 73;
            /// <stable>ICU 54</stable>
            public const int ManichaeanResh = 74;
            /// <stable>ICU 54</stable>
            public const int ManichaeanSadhe = 75;
            /// <stable>ICU 54</stable>
            public const int ManichaeanSamekh = 76;
            /// <stable>ICU 54</stable>
            public const int ManichaeanTaw = 77;
            /// <stable>ICU 54</stable>
            public const int ManichaeanTen = 78;
            /// <stable>ICU 54</stable>
            public const int ManichaeanTeth = 79;
            /// <stable>ICU 54</stable>
            public const int ManichaeanThamedh = 80;
            /// <stable>ICU 54</stable>
            public const int ManichaeanTwenty = 81;
            /// <stable>ICU 54</stable>
            public const int ManichaeanWaw = 82;
            /// <stable>ICU 54</stable>
            public const int ManichaeanYodh = 83;
            /// <stable>ICU 54</stable>
            public const int ManichaeanZayin = 84;
            /// <stable>ICU 54</stable>
            public const int StraightWaw = 85;

            /// <stable>ICU 58</stable>
            public const int AfricanFeh = 86;
            /// <stable>ICU 58</stable>
            public const int AfricanNoon = 87;
            /// <stable>ICU 58</stable>
            public const int AfricanQaf = 88;

            /// <stable>ICU 60</stable>
            public const int MalayalamBha = 89;
            /// <stable>ICU 60</stable>
            public const int MalayalamJa = 90;
            /// <stable>ICU 60</stable>
            public const int MalayalamLla = 91;
            /// <stable>ICU 60</stable>
            public const int MalayalamLlla = 92;
            /// <stable>ICU 60</stable>
            public const int MalayalamNga = 93;
            /// <stable>ICU 60</stable>
            public const int MalayalamNna = 94;
            /// <stable>ICU 60</stable>
            public const int MalayalamNnna = 95;
            /// <stable>ICU 60</stable>
            public const int MalayalamNya = 96;
            /// <stable>ICU 60</stable>
            public const int MalayalamRa = 97;
            /// <stable>ICU 60</stable>
            public const int MalayalamSsa = 98;
            /// <stable>ICU 60</stable>
            public const int MalayalamTta = 99;

            /// <summary>
            /// One more than the highest normal <see cref="JoiningGroup"/> value.
            /// The highest value is available via <see cref="GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Joining_Group"/>
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 100;
        }

        /// <summary>
        /// Grapheme Cluster Break constants.
        /// </summary>
        /// <seealso cref="UProperty.Grapheme_Cluster_Break"/>
        /// <stable>ICU 3.4</stable>
        public static class GraphemeClusterBreak
        {
            /// <stable>ICU 3.4</stable>
            public const int Other = 0;
            /// <stable>ICU 3.4</stable>
            public const int Control = 1;
            /// <stable>ICU 3.4</stable>
            public const int Cr = 2;
            /// <stable>ICU 3.4</stable>
            public const int Extend = 3;
            /// <stable>ICU 3.4</stable>
            public const int L = 4;
            /// <stable>ICU 3.4</stable>
            public const int Lf = 5;
            /// <stable>ICU 3.4</stable>
            public const int Lv = 6;
            /// <stable>ICU 3.4</stable>
            public const int Lvt = 7;
            /// <stable>ICU 3.4</stable>
            public const int T = 8;
            /// <stable>ICU 3.4</stable>
            public const int V = 9;
            /// <stable>ICU 4.0</stable>
            public const int SpacingMark = 10;
            /// <stable>ICU 4.0</stable>
            public const int Prepend = 11;
            /// <stable>ICU 50</stable>
            public const int RegionalIndicator = 12;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
            /// <stable>ICU 58</stable>
            public const int EBase = 13;          /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
            /// <stable>ICU 58</stable>
            public const int EBaseGaz = 14;      /*[EBG]*/
            /// <stable>ICU 58</stable>
            public const int EModifier = 15;      /*[EM]*/
            /// <stable>ICU 58</stable>
            public const int GlueAfterZwj = 16;  /*[GAZ]*/
            /// <stable>ICU 58</stable>
            public const int Zwj = 17;             /*[ZWJ]*/

            /// <summary>
            /// One more than the highest normal <see cref="GraphemeClusterBreak"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Grapheme_Cluster_Break"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 18;
        }

        /// <summary>
        /// Word Break constants.
        /// </summary>
        /// <seealso cref="UProperty.Word_Break"/>
        /// <stable>ICU 3.4</stable>
        public static class WordBreak
        {
            /// <stable>ICU 3.8</stable>
            public const int Other = 0;
            /// <stable>ICU 3.8</stable>
            public const int ALetter = 1;
            /// <stable>ICU 3.8</stable>
            public const int Format = 2;
            /// <stable>ICU 3.8</stable>
            public const int Katakana = 3;
            /// <stable>ICU 3.8</stable>
            public const int MidLetter = 4;
            /// <stable>ICU 3.8</stable>
            public const int MidNum = 5;
            /// <stable>ICU 3.8</stable>
            public const int Numeric = 6;
            /// <stable>ICU 3.8</stable>
            public const int ExtendNumLet = 7;
            /// <stable>ICU 4.0</stable>
            public const int Cr = 8;
            /// <stable>ICU 4.0</stable>
            public const int Extend = 9;
            /// <stable>ICU 4.0</stable>
            public const int Lf = 10;
            /// <stable>ICU 4.0</stable>
            public const int MidNumLet = 11;
            /// <stable>ICU 4.0</stable>
            public const int Newline = 12;
            /// <stable>ICU 50</stable>
            public const int RegionalIndicator = 13;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
            /// <stable>ICU 52</stable>
            public const int HebrewLetter = 14;    /*[HL]*/ /* from here on: new in Unicode 6.3/ICU 52 */
            /// <stable>ICU 52</stable>
            public const int SingleQuote = 15;     /*[SQ]*/
            /// <stable>ICU 52</stable>
            public const int DoubleQuote = 16;     /*[DQ]*/
            /// <stable>ICU 58</stable>
            public const int EBase = 17;           /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
            /// <stable>ICU 58</stable>
            public const int EBaseGaz = 18;       /*[EBG]*/
            /// <stable>ICU 58</stable>
            public const int EModifier = 19;       /*[EM]*/
            /// <stable>ICU 58</stable>
            public const int GlueAfterZwj = 20;   /*[GAZ]*/
            /// <stable>ICU 58</stable>
            public const int Zwj = 21;              /*[ZWJ]*/

            /// <summary>
            /// One more than the highest normal <see cref="WordBreak"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Word_Break"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 22;
        }

        /// <summary>
        /// Sentence Break constants.
        /// </summary>
        /// <seealso cref="UProperty.Sentence_Break"/>
        /// <stable>ICU 3.4</stable>
        public static class SentenceBreak
        {
            /// <stable>ICU 3.8</stable>
            public const int Other = 0;
            /// <stable>ICU 3.8</stable>
            public const int ATerm = 1;
            /// <stable>ICU 3.8</stable>
            public const int Close = 2;
            /// <stable>ICU 3.8</stable>
            public const int Format = 3;
            /// <stable>ICU 3.8</stable>
            public const int Lower = 4;
            /// <stable>ICU 3.8</stable>
            public const int Numeric = 5;
            /// <stable>ICU 3.8</stable>
            public const int OLetter = 6;
            /// <stable>ICU 3.8</stable>
            public const int Sep = 7;
            /// <stable>ICU 3.8</stable>
            public const int Sp = 8;
            /// <stable>ICU 3.8</stable>
            public const int STerm = 9;
            /// <stable>ICU 3.8</stable>
            public const int Upper = 10;
            /// <stable>ICU 4.0</stable>
            public const int Cr = 11;
            /// <stable>ICU 4.0</stable>
            public const int Extend = 12;
            /// <stable>ICU 4.0</stable>
            public const int Lf = 13;
            /// <stable>ICU 4.0</stable>
            public const int SContinue = 14;

            /// <summary>
            /// One more than the highest normal <see cref="SentenceBreak"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Sentence_Break"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 15;
        }

        /// <summary>
        /// Line Break constants.
        /// </summary>
        /// <seealso cref="UProperty.Line_Break"/>
        /// <stable>ICU 2.4</stable>
        public static class LineBreak
        {
            /// <stable>ICU 2.4</stable>
            public const int Unknown = 0;
            /// <stable>ICU 2.4</stable>
            public const int Ambiguous = 1;
            /// <stable>ICU 2.4</stable>
            public const int Alphabetic = 2;
            /// <stable>ICU 2.4</stable>
            public const int BreakBoth = 3;
            /// <stable>ICU 2.4</stable>
            public const int BreakAfter = 4;
            /// <stable>ICU 2.4</stable>
            public const int BreakBefore = 5;
            /// <stable>ICU 2.4</stable>
            public const int MandatoryBreak = 6;
            /// <stable>ICU 2.4</stable>
            public const int ContingentBreak = 7;
            /// <stable>ICU 2.4</stable>
            public const int ClosePunctuation = 8;
            /// <stable>ICU 2.4</stable>
            public const int CombiningMark = 9;
            /// <stable>ICU 2.4</stable>
            public const int CarriageReturn = 10;
            /// <stable>ICU 2.4</stable>
            public const int Exclamation = 11;
            /// <stable>ICU 2.4</stable>
            public const int Glue = 12;
            /// <stable>ICU 2.4</stable>
            public const int Hyphen = 13;
            /// <stable>ICU 2.4</stable>
            public const int Ideographic = 14;
            ///// <seealso cref="Inseparable"/>
            ///// <stable>ICU 2.4</stable>
            //public const int Inseperable = 15;
            ///// <summary>
            ///// Renamed from the misspelled "inseperable" in Unicode 4.0.1.
            ///// </summary>
            /// <stable>ICU 3.0</stable>
            public const int Inseparable = 15;
            /// <stable>ICU 2.4</stable>
            public const int InfixNumeric = 16;
            /// <stable>ICU 2.4</stable>
            public const int LineFeed = 17;
            /// <stable>ICU 2.4</stable>
            public const int Nonstarter = 18;
            /// <stable>ICU 2.4</stable>
            public const int Numeric = 19;
            /// <stable>ICU 2.4</stable>
            public const int OpenPunctuation = 20;
            /// <stable>ICU 2.4</stable>
            public const int PostfixNumeric = 21;
            /// <stable>ICU 2.4</stable>
            public const int PrefixNumeric = 22;
            /// <stable>ICU 2.4</stable>
            public const int Quotation = 23;
            /// <stable>ICU 2.4</stable>
            public const int ComplexContext = 24;
            /// <stable>ICU 2.4</stable>
            public const int Surrogate = 25;
            /// <stable>ICU 2.4</stable>
            public const int Space = 26;
            /// <stable>ICU 2.4</stable>
            public const int BreakSymbols = 27;
            /// <stable>ICU 2.4</stable>
            public const int ZwSpace = 28;
            /// <stable>ICU 2.6</stable>
            public const int NextLine = 29;  /*[NL]*/ /* from here on: new in Unicode 4/ICU 2.6 */
            /// <stable>ICU 2.6</stable>
            public const int WordJoiner = 30;      /*[WJ]*/
            /// <stable>ICU 3.4</stable>
            public const int H2 = 31;  /* from here on: new in Unicode 4.1/ICU 3.4 */
            /// <stable>ICU 3.4</stable>
            public const int H3 = 32;
            /// <stable>ICU 3.4</stable>
            public const int Jl = 33;
            /// <stable>ICU 3.4</stable>
            public const int Jt = 34;
            /// <stable>ICU 3.4</stable>
            public const int Jv = 35;
            /// <stable>ICU 4.4</stable>
            public const int CloseParenthesis = 36; /*[CP]*/ /* new in Unicode 5.2/ICU 4.4 */
            /// <stable>ICU 49</stable>
            public const int ConditionalJapaneseStarter = 37;  /*[CJ]*/ /* new in Unicode 6.1/ICU 49 */
            /// <stable>ICU 49</stable>
            public const int HebrewLetter = 38;  /*[HL]*/ /* new in Unicode 6.1/ICU 49 */
            /// <stable>ICU 50</stable>
            public const int RegionalIndicator = 39;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
            /// <stable>ICU 58</stable>
            public const int EBase = 40;  /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
            /// <stable>ICU 58</stable>
            public const int EModifier = 41;  /*[EM]*/
            /// <stable>ICU 58</stable>
            public const int Zwj = 42;  /*[ZWJ]*/

            /// <summary>
            /// One more than the highest normal <see cref="LineBreak"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Line_Break"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 43;
        }

        /// <summary>
        /// Numeric Type constants.
        /// </summary>
        /// <seealso cref="UProperty.Numeric_Type"/>
        /// <stable>ICU 2.4</stable>
        public static class NumericType
        {
            /// <stable>ICU 2.4</stable>
            public const int None = 0;
            /// <stable>ICU 2.4</stable>
            public const int Decimal = 1;
            /// <stable>ICU 2.4</stable>
            public const int Digit = 2;
            /// <stable>ICU 2.4</stable>
            public const int Numeric = 3;

            /// <summary>
            /// One more than the highest normal <see cref="NumericType"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Numeric_Type"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 4;
        }

        /// <summary>
        /// Hangul Syllable Type constants.
        /// </summary>
        /// <seealso cref="UProperty.Hangul_Syllable_Type"/>
        /// <stable>ICU 2.6</stable>
        public static class HangulSyllableType
        {
            /// <stable>ICU 2.6</stable>
            public const int NotApplicable = 0;   /*[NA]*/ /*See note !!*/
            /// <stable>ICU 2.6</stable>
            public const int LeadingJamo = 1;   /*[L]*/
            /// <stable>ICU 2.6</stable>
            public const int VowelJamo = 2;   /*[V]*/
            /// <stable>ICU 2.6</stable>
            public const int TrailingJamo = 3;   /*[T]*/
            /// <stable>ICU 2.6</stable>
            public const int LvSyllable = 4;   /*[LV]*/
            /// <stable>ICU 2.6</stable>
            public const int LvtSyllable = 5;   /*[LVT]*/

            /// <summary>
            /// One more than the highest normal <see cref="HangulSyllableType"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Hangul_Syllable_Type"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int COUNT = 6;
        }

        /// <summary>
        /// Bidi Paired Bracket Type constants.
        /// </summary>
        /// <seealso cref="UProperty.Bidi_Paired_Bracket_Type"/>
        /// <stable>ICU 52</stable>
        public static class BidiPairedBracketType
        {
            /// <summary>
            /// Not a paired bracket.
            /// </summary>
            /// <stable>ICU 52</stable>
            public const int None = 0;
            /// <summary>
            /// Open paired bracket.
            /// </summary>
            /// <stable>ICU 52</stable>
            public const int Open = 1;
            /// <summary>
            /// Close paired bracket.
            /// </summary>
            /// <stable>ICU 52</stable>
            public const int Close = 2;

            /// <summary>
            /// One more than the highest normal <see cref="BidiPairedBracketType"/> value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/>
            /// with parameter <see cref="UProperty.Bidi_Paired_Bracket_Type"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 3;
        }

        // public data members -----------------------------------------------

        /// <summary>
        /// The lowest Unicode code point value, constant 0.
        /// Same as <see cref="Character.MIN_CODE_POINT"/>, same integer value as <see cref="Char.MinValue"/>.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int MinValue = Character.MIN_CODE_POINT; // ICU4N TODO: Change to Char.MinValue

        /// <summary>
        /// The highest Unicode code point value (scalar value), constant U+10FFFF (uses 21 bits).
        /// Same integer value as <see cref="Character.MAX_CODE_POINT"/>.
        /// <para/>
        /// Up-to-date Unicode implementation of <see cref="Char.MaxValue"/>
        /// which is still a char with the value U+FFFF.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int MaxValue = Character.MAX_CODE_POINT; // ICU4N TODO: Change to Char.MaxValue (and check documentation to ensure it is right)

        /// <summary>
        /// The minimum value for Supplementary code points, constant U+10000.
        /// Same as <see cref="Character.MIN_SUPPLEMENTARY_CODE_POINT"/>.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SupplementaryMinValue = Character.MIN_SUPPLEMENTARY_CODE_POINT;

        /// <summary>
        /// Unicode value used when translating into Unicode encoding form and there
        /// is no existing character.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int ReplacementChar = '\uFFFD';

        /// <summary>
        /// Special value that is returned by <see cref="GetUnicodeNumericValue(int)"/> when no
        /// numeric value is defined for a code point.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        /// <seealso cref="GetUnicodeNumericValue(int)"/>
        public const double NoNumericValue = -123456789;

        /// <summary>
        /// Compatibility constant for <see cref="Character.MIN_RADIX"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int MinRadix = Character.MIN_RADIX;

        /// <summary>
        /// Compatibility constant for <see cref="Character.MAX_RADIX"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int MaxRadix = Character.MAX_RADIX;

        /// <summary>
        /// Do not lowercase non-initial parts of words when titlecasing.
        /// Option bit for titlecasing APIs that take an options bit set.
        /// </summary>
        /// <remarks>
        /// By default, titlecasing will titlecase the first cased character
        /// of a word and lowercase all other characters.
        /// With this option, the other characters will not be modified.
        /// </remarks>
        /// <see cref="ToTitleCase(int)"/>
        /// <stable>ICU 3.8</stable>
        public const int TitleCaseNoLowerCase = 0x100; // ICU4N TODO: API make into [Flags] enum

        /// <summary>
        /// Do not adjust the titlecasing indexes from <see cref="BreakIterator.Next()"/> indexes;
        /// titlecase exactly the characters at breaks from the iterator.
        /// Option bit for titlecasing APIs that take an options bit set.
        /// </summary>
        /// <remarks>
        /// By default, titlecasing will take each break iterator index,
        /// adjust it by looking for the next cased character, and titlecase that one.
        /// Other characters are lowercased.
        /// <para/>
        /// This follows Unicode 4 &amp; 5 section 3.13 Default Case Operations:
        /// <para/>
        /// R3  ToTitleCase(X): Find the word boundaries based on Unicode Standard Annex
        /// #29, "Text Boundaries." Between each pair of word boundaries, find the first
        /// cased character F. If F exists, map F to default_title(F); then map each
        /// subsequent character C to default_lower(C).
        /// </remarks>
        /// <seealso cref="ToTitleCase(int)"/>
        /// <seealso cref="TitleCaseNoLowerCase"/>
        /// <stable>ICU 3.8</stable>
        public const int TitleCaseNoBreakAdjustment = 0x200; // ICU4N TODO: API make into [Flags] enum

        // public methods ----------------------------------------------------

        /// <summary>
        /// Returns the numeric value of a decimal digit code point.
        /// <para/>
        /// Note that this will return positive values for code points 
        /// for which <see cref="IsDigit(int)"/> returns false.
        /// </summary>
        /// <remarks>
        /// A code point is a valid digit if and only if:
        /// <list type="bullet">
        ///     <item><description><paramref name="ch"/> is a decimal digit or one of the european letters, and</description></item>
        ///     <item><description>the value of <paramref name="ch"/> is less than the specified radix.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="ch">The code point to query.</param>
        /// <param name="radix">The radix.</param>
        /// <returns>The numeric value represented by the code point in the
        /// specified radix, or -1 if the code point is not a decimal digit
        /// or if its value is too large for the radix.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        public static int Digit(int ch, int radix)
        {
            if (2 <= radix && radix <= 36)
            {
                int value = Digit(ch);
                if (value < 0)
                {
                    // ch is not a decimal digit, try latin letters
                    value = UCharacterProperty.GetEuropeanDigit(ch);
                }
                return (value < radix) ? value : -1;
            }
            else
            {
                return -1;  // invalid radix
            }
        }

        /// <summary>
        /// Returns the numeric value of a decimal digit code point.
        /// <para/>
        /// This is a convenience overload of <see cref="Digit(int, int)"/>
        /// that provides a decimal radix.
        /// </summary>
        /// <param name="ch">The code point to query.</param>
        /// <returns>The numeric value represented by the code point,
        /// or -1 if the code point is not a decimal digit or if its
        /// value is too large for a decimal radix.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        public static int Digit(int ch)
        {
            return UCharacterProperty.Instance.Digit(ch);
        }

        /// <summary>
        /// Returns the numeric value of the code point as a nonnegative
        /// integer.
        /// <para/>
        /// If the code point does not have a numeric value, then -1 is returned.
        /// <para/>
        /// If the code point has a numeric value that cannot be represented as a
        /// nonnegative integer (for example, a fractional value), then -2 is
        /// returned.
        /// </summary>
        /// <param name="ch">The code point to query.</param>
        /// <returns>The numeric value of the code point, or -1 if it has no numeric
        /// value, or -2 if it has a numeric value that cannot be represented as a
        /// nonnegative integer.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        public static int GetNumericValue(int ch)
        {
            return UCharacterProperty.Instance.GetNumericValue(ch);
        }

        /// <icu/>
        /// <summary>
        /// Returns the numeric value for a Unicode code point as defined in the
        /// Unicode Character Database.
        /// <para/>
        /// A <see cref="double"/> return type is necessary because some numeric values are
        /// fractions, negative, or too large for <see cref="int"/>.
        /// <para/>
        /// For characters without any numeric values in the Unicode Character
        /// Database, this function will return <see cref="NoNumericValue"/>.
        /// Note: This is different from the Unicode Standard which specifies NaN as the default value.
        /// <para/>
        /// This corresponds to the ICU4C function u_getNumericValue.
        /// </summary>
        /// <param name="ch">Code point to get the numeric value for.</param>
        /// <returns>Numeric value of <paramref name="ch"/>, or <see cref="NoNumericValue"/> if none is defined.</returns>
        /// <stable>ICU 2.4</stable>
        public static double GetUnicodeNumericValue(int ch)
        {
            return UCharacterProperty.Instance.GetUnicodeNumericValue(ch);
        }

        /// <summary>
        /// Compatibility override of Java deprecated method.  This
        /// method will always remain deprecated. 
        /// Same as <see cref="Character.IsSpace(char)"/>.
        /// </summary>
        /// <param name="ch">The code point.</param>
        /// <returns>true if the code point is a space character as
        /// defined by <see cref="Character.IsSpace(char)"/>.</returns>
        [Obsolete("ICU 3.4 (Java)")]
        public static bool IsSpace(int ch)
        {
            return ch <= 0x20 &&
                    (ch == 0x20 || ch == 0x09 || ch == 0x0a || ch == 0x0c || ch == 0x0d);
        }

        /// <summary>
        /// Returns a value indicating a code point's Unicode category.
        /// Up-to-date Unicode implementation of <c>char.GetUnicodeCategory(char)</c>
        /// except for the code points that had their category changed.
        /// <para/>
        /// Return results are constants from the enum <see cref="UUnicodeCategory"/>.
        /// <para/>
        /// <em>NOTE:</em> the <see cref="UUnicodeCategory"/> values are <em>not</em> compatible with
        /// those returned by <c>char.GetUnicodeCategory(char)</c> or <see cref="Character.GetType(int)"/>.
        /// <see cref="UUnicodeCategory"/> values
        /// match the ones used in ICU4C, while <see cref="Character"/> type
        /// values, though similar, skip the value 17. The <see cref="UnicodeCategory"/>
        /// numeric values are often different than with <see cref="UUnicodeCategory"/>'s values.
        /// </summary>
        /// <param name="ch">Code point whose type is to be determined.</param>
        /// <returns>Category which is a value of <see cref="UUnicodeCategory"/>.</returns>
        /// <stable>ICU 2.1</stable>
        public static UUnicodeCategory GetType(int ch) // ICU4N TODO: API - Rename GetUnicodeCategory() to cover Char, add overload for (string, int)
        {
            return (UUnicodeCategory)UCharacterProperty.Instance.GetType(ch);
        }

        /// <summary>
        /// Determines if a code point has a defined meaning in the up-to-date
        /// Unicode standard.
        /// E.g. supplementary code points though allocated space are not defined in
        /// Unicode yet.
        /// </summary>
        /// <param name="ch">Code point to be determined if it is defined in the most
        /// current version of Unicode.</param>
        /// <returns>true if this code point is defined in unicode.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsDefined(int ch)
        {
            // ICU4N specific - need to check for the value, not 0
            return GetType(ch) != UUnicodeCategory.OtherNotAssigned;
        }

        /// <summary>
        /// Determines if a code point is a .NET digit.
        /// <para/>
        /// It returns true for decimal digits only.
        /// </summary>
        /// <param name="ch">Code point to query.</param>
        /// <returns>true if this code point is a digit.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsDigit(int ch) // ICU4N TODO: API - to cover Char, add overload for (string, int)
        {
            return GetType(ch) == UUnicodeCategory.DecimalDigitNumber;
        }

        /// <summary>
        /// Determines if the specified code point is an ISO control character.
        /// A code point is considered to be an ISO control character if it is in
        /// the range &#92;u0000 through &#92;u001F or in the range &#92;u007F through
        /// &#92;u009F.
        /// </summary>
        /// <param name="ch">Code point to determine if it is an ISO control character.</param>
        /// <returns>true if code point is a ISO control character.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsISOControl(int ch) // ICU4N TODO: API - to cover Char, add overload for (string, int)
        {
            return ch >= 0 && ch <= APPLICATION_PROGRAM_COMMAND_ &&
                    ((ch <= UNIT_SEPARATOR_) || (ch >= DELETE_));
        }

        /// <summary>
        /// Determines if the specified code point is a letter.
        /// Up-to-date Unicode implementation of <see cref="char.IsLetter(char)"/>.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a letter.</param>
        /// <returns>true if code point is a letter.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLetter(int ch) // ICU4N TODO: API - to cover Char, add overload for (string, int)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32()))) != 0;
        }

        /// <summary>
        /// Determines if the specified code point is a letter or digit.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a letter or a digit.</param>
        /// <returns>true if code point is a letter or a digit.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLetterOrDigit(int ch) // ICU4N TODO: API - to cover Char, add overload for (string, int)
        {
            return ((1 << GetType(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32())
                            | (1 << UUnicodeCategory.DecimalDigitNumber.ToInt32()))) != 0;
        }

        // ICU4N: We definitely don't need any of the Java.. functions. 
        // In .NET, it is not so straightforward to determine if a character
        // is an identifier (and it is not built in).

        // ICU4N TODO: Perhaps it would make sense to duplicate these from Java,
        // since in .NET determining if a string is an identifier is difficult.

        /// <summary>
        /// Determines if the specified code point is a lowercase character.
        /// UnicodeData only contains case mappings for code points where they are
        /// one-to-one mappings; it also omits information about context-sensitive
        /// case mappings.
        /// </summary>
        /// <remarks>
        /// For more information about Unicode case mapping
        /// please refer to the
        /// <a href="http://www.unicode.org/unicode/reports/tr21/">Technical report #21</a>.
        /// <para/>
        /// Up-to-date Unicode implementation of <see cref="char.IsLower(char)"/>.
        /// </remarks>
        /// <param name="ch">Code point to determine if it is in lowercase.</param>
        /// <returns>true if code point is a lowercase character.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - IsUpperCase vs IsUUppercase vs ToUpper - drop "case" from name
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsLowerCase(int ch) 
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UUnicodeCategory.LowercaseLetter;
        }

        /// <summary>
        /// Determines if the specified code point is a white space character.
        /// </summary>
        /// <remarks>
        /// A code point is considered to be an whitespace character if and only
        /// if it satisfies one of the following criteria:
        /// <list type="bullet">
        ///     <item><description>
        ///         It is a Unicode Separator character (categories "Z" = "Zs" or "Zl" or "Zp"), but is not
        ///         also a non-breaking space (&#92;u00A0 or &#92;u2007 or &#92;u202F).
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u0009, HORIZONTAL TABULATION.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u000A, LINE FEED.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u000B, VERTICAL TABULATION.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u000C, FORM FEED.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u000D, CARRIAGE RETURN.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u001C, FILE SEPARATOR.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u001D, GROUP SEPARATOR.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u001E, RECORD SEPARATOR.
        ///     </description></item>
        ///     <item><description>
        ///         It is &#92;u001F, UNIT SEPARATOR.
        ///     </description></item>
        /// </list>
        /// <para/>
        /// This API tries to sync with the semantics of .NET's <see cref="char.IsWhiteSpace(char)"/>, 
        /// but it may not return the exact same results because of the Unicode version
        /// difference.
        /// <para/>
        /// Note: Unicode 4.0.1 changed U+200B ZERO WIDTH SPACE from a Space Separator (Zs)
        /// to a Format Control (Cf). Since then, <c>IsWhitespace(0x200b)</c> returns false.
        /// See <a href="http://www.unicode.org/versions/Unicode4.0.1/">http://www.unicode.org/versions/Unicode4.0.1/</a>.
        /// </remarks>
        /// <param name="ch">Code point to determine if it is a white space.</param>
        /// <returns>true if the specified code point is a white space character.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API Rename IsWhiteSpace (for consistency with .NET)
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsWhitespace(int ch) 
        {
            // exclude no-break spaces
            // if props == 0, it will just fall through and return false
            return ((1 << (int)GetType(ch)) &
                    ((1 << (int)UUnicodeCategory.SpaceSeparator)
                            | (1 << (int)UUnicodeCategory.LineSeparator)
                            | (1 << (int)UUnicodeCategory.ParagraphSeparator))) != 0
                            && (ch != NO_BREAK_SPACE_) && (ch != FIGURE_SPACE_) && (ch != NARROW_NO_BREAK_SPACE_)
                            // TAB VT LF FF CR FS GS RS US NL are all control characters
                            // that are white spaces.
                            || (ch >= 0x9 && ch <= 0xd) || (ch >= 0x1c && ch <= 0x1f);
        }

        /// <summary>
        /// Determines if the specified code point is a Unicode specified space
        /// character, i.e. if code point is in the category Zs, Zl and Zp.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a space.</param>
        /// <returns>true if the specified code point is a space character.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsSpaceChar(int ch)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToInt32()) & ((1 << UUnicodeCategory.SpaceSeparator.ToInt32())
                    | (1 << UUnicodeCategory.LineSeparator.ToInt32())
                    | (1 << UUnicodeCategory.ParagraphSeparator.ToInt32())))
                    != 0;
        }

        /// <summary>
        /// Determines if the specified code point is a titlecase character.
        /// UnicodeData only contains case mappings for code points where they are
        /// one-to-one mappings; it also omits information about context-sensitive
        /// case mappings.
        /// </summary>
        /// <remarks>
        /// For more information about Unicode case mapping
        /// please refer to the
        /// <a href="http://www.unicode.org/unicode/reports/tr21/">Technical report #21</a>.
        /// </remarks>
        /// <param name="ch">Code point to determine if it is in title case.</param>
        /// <returns>true if the specified code point is a titlecase character.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsTitleCase(int ch)
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UUnicodeCategory.TitlecaseLetter;
        }

        /// <summary>
        /// Determines if the specified code point may be any part of a Unicode
        /// identifier other than the starting character.
        /// </summary>
        /// <remarks>
        /// A code point may be part of a Unicode identifier if and only if it is
        /// one of the following:
        /// <list type="bullet">
        ///     <item><description> Lu Uppercase letter </description></item>
        ///     <item><description> Ll Lowercase letter </description></item>
        ///     <item><description> Lt Titlecase letter </description></item>
        ///     <item><description> Lm Modifier letter </description></item>
        ///     <item><description> Lo Other letter </description></item>
        ///     <item><description> Nl Letter number </description></item>
        ///     <item><description> Pc Connecting punctuation character </description></item>
        ///     <item><description> Nd decimal number </description></item>
        ///     <item><description> Mc Spacing combining mark </description></item>
        ///     <item><description> Mn Non-spacing mark </description></item>
        ///     <item><description> Cf formatting code </description></item>
        /// </list>
        /// <para/>
        /// See <a href="http://www.unicode.org/unicode/reports/tr8/">UTR #8</a>.
        /// </remarks>
        /// <param name="ch">code point to determine if is can be part of a Unicode
        /// identifier.</param>
        /// <returns>true if code point is any character belonging a unicode
        /// identifier suffix after the first character.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsUnicodeIdentifierPart(int ch)
        {
            // if props == 0, it will just fall through and return false
            // cat == format
            return ((1 << GetType(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32())
                            | (1 << UUnicodeCategory.LetterNumber.ToInt32())
                            | (1 << UUnicodeCategory.ConnectorPunctuation.ToInt32())
                            | (1 << UUnicodeCategory.DecimalDigitNumber.ToInt32())
                            | (1 << UUnicodeCategory.SpacingCombiningMark.ToInt32())
                            | (1 << UUnicodeCategory.NonSpacingMark.ToInt32()))) != 0
                            || IsIdentifierIgnorable(ch);
        }

        /// <summary>
        /// Determines if the specified code point is permissible as the first
        /// character in a Unicode identifier.
        /// </summary>
        /// <remarks>
        /// A code point may start a Unicode identifier if it is of type either
        /// <list type="bullet">
        ///     <item><description> Lu Uppercase letter </description></item>
        ///     <item><description> Ll Lowercase letter </description></item>
        ///     <item><description> Lt Titlecase letter </description></item>
        ///     <item><description> Lm Modifier letter </description></item>
        ///     <item><description> Lo Other letter </description></item>
        ///     <item><description> Nl Letter number </description></item>
        /// </list>
        /// <para/>
        /// See <a href="http://www.unicode.org/unicode/reports/tr8/">UTR #8</a>.
        /// </remarks>
        /// <param name="ch">code point to determine if it can start a Unicode identifier.</param>
        /// <returns>true if code point is the first character belonging a unicode
        /// identifier.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsUnicodeIdentifierStart(int ch)
        {
            /*int cat = getType(ch);*/
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32())
                            | (1 << UUnicodeCategory.LetterNumber.ToInt32()))) != 0;
        }

        /// <summary>
        /// Determines if the specified code point should be regarded as an
        /// ignorable character in a .NET identifier.
        /// </summary>
        /// <remarks>
        /// A character is .NET-identifier-ignorable if it has the general category
        /// Cf Formatting Control, or it is a non-.NET-whitespace ISO control:
        /// U+0000..U+0008, U+000E..U+001B, U+007F..U+009F.
        /// <para/>
        /// See <a href="http://www.unicode.org/unicode/reports/tr8/">UTR #8</a>.
        /// <para/>
        /// Note that Unicode just recommends to ignore Cf (format controls).
        /// </remarks>
        /// <param name="ch">Code point to be determined if it can be ignored in a Unicode
        /// identifier.</param>
        /// <returns>true if the code point is ignorable.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsIdentifierIgnorable(int ch)
        {
            // see java.lang.Character.isIdentifierIgnorable() on range of
            // ignorable characters.
            if (ch <= 0x9f)
            {
                return IsISOControl(ch)
                        && !((ch >= 0x9 && ch <= 0xd)
                                || (ch >= 0x1c && ch <= 0x1f));
            }
            return GetType(ch) == UUnicodeCategory.Format;
        }

        /// <summary>
        /// Determines if the specified code point is an uppercase character.
        /// UnicodeData only contains case mappings for code point where they are
        /// one-to-one mappings; it also omits information about context-sensitive
        /// case mappings.
        /// </summary>
        /// <remarks>
        /// For language specific case conversion behavior, use <c>CultureInfo.TextInfo.ToUpper(char)</c>.
        /// <para/>
        /// For example, the case conversion for dot-less i and dotted I in Turkish,
        /// or for final sigma in Greek.
        /// For more information about Unicode case mapping please refer to the
        /// <a href="http://www.unicode.org/unicode/reports/tr21/">
        /// Technical report #21</a>.
        /// <para/>
        /// Up-to-date Unicode implementation of <see cref="Char.IsUpper(char)"/>.
        /// </remarks>
        /// <param name="ch">Code point to determine if it is in uppercase.</param>
        /// <returns>true if the code point is an uppercase character.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static bool IsUpperCase(int ch) // ICU4N TODO: API - IsUpperCase vs IsUUppercase vs ToUpper - drop "case" from name
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UUnicodeCategory.UppercaseLetter;
        }

        /// <summary>
        /// The given code point is mapped to its lowercase equivalent; if the code
        /// point has no lowercase equivalent, the code point itself is returned.
        /// Up-to-date Unicode implementation of <see cref="Char.ToLower(char)"/>
        /// </summary>
        /// <remarks>
        /// This function only returns the simple, single-code point case mapping.
        /// Full case mappings should be used whenever possible because they produce
        /// better results by working on whole strings.
        /// They take into account the string context and the language and can map
        /// to a result string with a different length as appropriate.
        /// Full case mappings are applied by the case mapping functions
        /// that take string parameters rather than code points (int).
        /// See also the User Guide chapter on C/POSIX migration:
        /// <a href="http://www.icu-project.org/userguide/posix.html#case_mappings">
        /// http://www.icu-project.org/userguide/posix.html#case_mappings</a>
        /// </remarks>
        /// <param name="ch">Code point whose lowercase equivalent is to be retrieved.</param>
        /// <returns>The lowercase equivalent code point.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static int ToLower(int ch) // ICU4N TODO: API - should this be ToLowerInvariant? Need to figure out the context-sensitive behavior and add overload if necessary.
        {
            return UCaseProps.Instance.ToLower(ch);
        }

        /// <summary>
        /// Converts argument code point and returns a string object representing
        /// the code point's value in UTF-16 format.
        /// The result is a string whose length is 1 for BMP code points, 2 for supplementary ones.
        /// <para/>
        /// Up-to-date Unicode implementation of <see cref="char.ConvertFromUtf32(int)"/>, however
        /// this implementation differs in that it returns null rather than throwing exceptions if
        /// the input is not a valid code point.
        /// </summary>
        /// <param name="ch">Code point.</param>
        /// <returns>String representation of the code point, null if code point is not
        /// defined in unicode.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static string ToString(int ch) // ICU4N TODO: API - Rename ConvertFromUtf32 to cover Char
        {
            if (ch < MinValue || ch > MaxValue)
            {
                return null;
            }

            if (ch < SupplementaryMinValue)
            {
                return new string(new char[] { (char)ch });
            }

            return new string(Character.ToChars(ch));
        }

        /// <summary>
        /// Converts the code point argument to titlecase.
        /// If no titlecase is available, the uppercase is returned. If no uppercase
        /// is available, the code point itself is returned.
        /// Up-to-date Unicode implementation of <c>CultureInfo.TextInfo.ToTitleCase(string)</c>.
        /// </summary>
        /// <remarks>
        /// This function only returns the simple, single-code point case mapping.
        /// Full case mappings should be used whenever possible because they produce
        /// better results by working on whole strings.
        /// They take into account the string context and the language and can map
        /// to a result string with a different length as appropriate.
        /// Full case mappings are applied by the case mapping functions
        /// that take string parameters rather than code points (int).
        /// See also the User Guide chapter on C/POSIX migration:
        /// <a href="http://www.icu-project.org/userguide/posix.html#case_mappings">
        /// http://www.icu-project.org/userguide/posix.html#case_mappings</a>
        /// </remarks>
        /// <param name="ch">Code point whose title case is to be retrieved.</param>
        /// <returns>Titlecase code point.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static int ToTitleCase(int ch)
        {
            return UCaseProps.Instance.ToTitle(ch);
        }

        /// <summary>
        /// Converts the character argument to uppercase.
        /// If no uppercase is available, the character itself is returned.
        /// Up-to-date Unicode implementation of <see cref="char.ToUpper(char)"/>.
        /// </summary>
        /// <remarks>
        /// This function only returns the simple, single-code point case mapping.
        /// Full case mappings should be used whenever possible because they produce
        /// better results by working on whole strings.
        /// They take into account the string context and the language and can map
        /// to a result string with a different length as appropriate.
        /// Full case mappings are applied by the case mapping functions
        /// that take string parameters rather than code points (int).
        /// See also the User Guide chapter on C/POSIX migration:
        /// <a href="http://www.icu-project.org/userguide/posix.html#case_mappings">
        /// http://www.icu-project.org/userguide/posix.html#case_mappings</a>
        /// </remarks>
        /// <param name="ch">Code point whose uppercase is to be retrieved.</param>
        /// <returns>Uppercase code point.</returns>
        /// <stable>ICU 2.1</stable>
        // ICU4N TODO: API - to cover Char, add overload for (string, int)
        public static int ToUpper(int ch) // ICU4N TODO: API - should this be ToUpperInvariant? Need to figure out the context-sensitive behavior and add overload if necessary.
        {
            return UCaseProps.Instance.ToUpper(ch);
        }

        // ICU4N TODO: API - move functions from above not in System.Char to this section
        // extra methods not in System.Char --------------------------

        /// <icu/>
        /// <summary>
        /// Determines if the code point is a supplementary character.
        /// A code point is a supplementary character if and only if it is greater
        /// than <see cref="SupplementaryMinValue"/>.
        /// </summary>
        /// <param name="ch">Code point to be determined if it is in the supplementary plane.</param>
        /// <returns>true if code point is a supplementary character.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsSupplementary(int ch)
        {
            return ch >= UChar.SupplementaryMinValue &&
                    ch <= UChar.MaxValue;
        }

        /// <icu/>
        /// <summary>
        /// Determines if the code point is in the BMP plane.
        /// </summary>
        /// <param name="ch">Code point to be determined if it is not a supplementary character.</param>
        /// <returns>true if code point is not a supplementary character.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsBMP(int ch)
        {
            return (ch >= 0 && ch <= LAST_CHAR_MASK_);
        }

        /// <icu/>
        /// <summary>
        /// Determines whether the specified code point is a printable character
        /// according to the Unicode standard.
        /// </summary>
        /// <param name="ch">Code point to be determined if it is printable.</param>
        /// <returns>true if the code point is a printable character.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsPrintable(int ch)
        {
            UUnicodeCategory cat = GetType(ch);
            // if props == 0, it will just fall through and return false
            return (cat != UUnicodeCategory.OtherNotAssigned &&
                    cat != UUnicodeCategory.Control &&
                    cat != UUnicodeCategory.Format &&
                    cat != UUnicodeCategory.PrivateUse &&
                    cat != UUnicodeCategory.Surrogate);
        }

        /// <icu/>
        /// <summary>
        /// Determines whether the specified code point is of base form.
        /// A code point of base form does not graphically combine with preceding
        /// characters, and is neither a control nor a format character.
        /// </summary>
        /// <param name="ch">Code point to be determined if it is of base form.</param>
        /// <returns>true if the code point is of base form.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsBaseForm(int ch)
        {
            UUnicodeCategory cat = GetType(ch);
            // if props == 0, it will just fall through and return false
            return cat == UUnicodeCategory.DecimalDigitNumber ||
                    cat == UUnicodeCategory.OtherNumber ||
                    cat == UUnicodeCategory.LetterNumber ||
                    cat == UUnicodeCategory.UppercaseLetter ||
                    cat == UUnicodeCategory.LowercaseLetter ||
                    cat == UUnicodeCategory.TitlecaseLetter ||
                    cat == UUnicodeCategory.ModifierLetter ||
                    cat == UUnicodeCategory.OtherLetter ||
                    cat == UUnicodeCategory.NonSpacingMark ||
                    cat == UUnicodeCategory.EnclosingMark ||
                    cat == UUnicodeCategory.SpacingCombiningMark;
        }

        /// <icu/>
        /// <summary>
        /// Returns the Bidirection property of a code point.
        /// For example, 0x0041 (letter A) has the <see cref="UCharacterDirection.LeftToRight"/> directional
        /// property.
        /// <para/>
        /// Result returned belongs to the enum <see cref="UCharacterDirection"/>.
        /// </summary>
        /// <param name="ch">The code point to be determined its direction.</param>
        /// <returns>Direction constant from <see cref="UCharacterDirection"/>.</returns>
        /// <stable>ICU 2.1</stable>
        public static UCharacterDirection GetDirection(int ch)
        {
            return UBiDiProps.Instance.GetClass(ch);
        }

        /// <summary>
        /// Determines whether the code point has the "mirrored" property.
        /// This property is set for characters that are commonly used in
        /// Right-To-Left contexts and need to be displayed with a "mirrored"
        /// glyph.
        /// </summary>
        /// <param name="ch">Code point whose mirror is to be determined.</param>
        /// <returns>true if the code point has the "mirrored" property.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsMirrored(int ch)
        {
            return UBiDiProps.Instance.IsMirrored(ch);
        }

        /// <icu/>
        /// <summary>
        /// Maps the specified code point to a "mirror-image" code point.
        /// For code points with the "mirrored" property, implementations sometimes
        /// need a "poor man's" mapping to another code point such that the default
        /// glyph may serve as the mirror-image of the default glyph of the
        /// specified code point.
        /// <para/>
        /// This is useful for text conversion to and from codepages with visual
        /// order, and for displays without glyph selection capabilities.
        /// </summary>
        /// <param name="ch">Code point whose mirror is to be retrieved.</param>
        /// <returns>Another code point that may serve as a mirror-image substitute,
        /// or <paramref name="ch"/> itself if there is no such mapping or 
        /// <paramref name="ch"/> does not have the "mirrored" property.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        public static int GetMirror(int ch)
        {
            return UBiDiProps.Instance.GetMirror(ch);
        }

        /// <icu/>
        /// <summary>
        /// Maps the specified character to its paired bracket character.
        /// For <see cref="UProperty.Bidi_Paired_Bracket_Type"/>!=None, this 
        /// is the same as <see cref="GetMirror(int)"/>.
        /// Otherwise <paramref name="c"/> itself is returned.
        /// See <a href="http://www.unicode.org/reports/tr9/">http://www.unicode.org/reports/tr9/</a>.
        /// </summary>
        /// <param name="c">The code point to be mapped.</param>
        /// <returns>The paired bracket code point,
        /// or <paramref name="c"/> itself if there is no such mapping
        /// (<see cref="UProperty.Bidi_Paired_Bracket_Type"/>=None)
        /// </returns>
        /// <seealso cref="UProperty.Bidi_Paired_Bracket"/>
        /// <seealso cref="UProperty.Bidi_Paired_Bracket_Type"/>
        /// <seealso cref="GetMirror(int)"/>
        /// <stable>ICU 52</stable>
        public static int GetBidiPairedBracket(int c)
        {
            return UBiDiProps.Instance.GetPairedBracket(c);
        }

        /// <icu/>
        /// <summary>
        /// Returns the combining class of the argument codepoint.
        /// </summary>
        /// <param name="ch">Code point whose combining is to be retrieved.</param>
        /// <returns>The combining class of the codepoint.</returns>
        /// <stable>ICU 2.1</stable>
        public static int GetCombiningClass(int ch)
        {
            return Normalizer2.GetNFDInstance().GetCombiningClass(ch);
        }

        /// <icu/>
        /// <summary>
        /// A code point is illegal if and only if
        /// <list type="bullet">
        ///     <item><description> Out of bounds, less than 0 or greater than <see cref="UChar.MaxValue"/> </description></item>
        ///     <item><description> A surrogate value, 0xD800 to 0xDFFF </description></item>
        ///     <item><description> Not-a-character, having the form 0x xxFFFF or 0x xxFFFE </description></item>
        /// </list>
        /// Note: legal does not mean that it is assigned in this version of Unicode.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a legal code point by itself.</param>
        /// <returns>true if and only if legal.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLegal(int ch)
        {
            if (ch < MinValue)
            {
                return false;
            }
            if (ch < Character.MIN_SURROGATE)
            {
                return true;
            }
            if (ch <= Character.MAX_SURROGATE)
            {
                return false;
            }
            if (UCharacterUtility.IsNonCharacter(ch))
            {
                return false;
            }
            return (ch <= MaxValue);
        }

        /// <icu/>
        /// <summary>
        /// A string is legal iff all its code points are legal.
        /// A code point is illegal if and only if
        /// <list type="bullet">
        ///     <item><description> Out of bounds, less than 0 or greater than <see cref="UChar.MaxValue"/> </description></item>
        ///     <item><description> A surrogate value, 0xD800 to 0xDFFF </description></item>
        ///     <item><description> Not-a-character, having the form 0x xxFFFF or 0x xxFFFE </description></item>
        /// </list>
        /// Note: legal does not mean that it is assigned in this version of Unicode.
        /// </summary>
        /// <param name="str">String containing code points to examine.</param>
        /// <returns>true if and only if legal.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLegal(string str)
        {
            int size = str.Length;
            int codepoint;
            for (int i = 0; i < size; i += Character.CharCount(codepoint))
            {
                codepoint = str.CodePointAt(i);
                if (!IsLegal(codepoint))
                {
                    return false;
                }
            }
            return true;
        }

        /// <icu/>
        /// <summary>
        /// Gets the version of Unicode data used.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public static VersionInfo UnicodeVersion
        {
            get { return UCharacterProperty.Instance.UnicodeVersion; }
        }

        /// <icu/>
        /// <summary>
        /// Returns the most current Unicode name of the argument code point, or
        /// null if the character is unassigned or outside the range
        /// <see cref="UChar.MinValue"/> and <see cref="UChar.MaxValue"/>
        /// or does not have a name.
        /// <para/>
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </summary>
        /// <param name="ch">The code point for which to get the name.</param>
        /// <returns>Most current Unicode name.</returns>
        /// <stable>ICU 2.1</stable>
        public static string GetName(int ch)
        {
            return UCharacterName.Instance.GetName(ch, UCharacterNameChoice.UnicodeCharName);
        }

        /// <icu/>
        /// <summary>
        /// Returns the names for each of the characters in a string.
        /// </summary>
        /// <param name="s">String to format.</param>
        /// <param name="separator">String to go between names.</param>
        /// <returns>String of names.</returns>
        /// <stable>ICU 3.8</stable>
        public static string GetName(string s, string separator)
        {
            if (s.Length == 1)
            { // handle common case
                return GetName(s[0]);
            }
            int cp;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i += Character.CharCount(cp))
            {
                cp = s.CodePointAt(i);
                if (i != 0) sb.Append(separator);
                // ICU4N: Need to manually put the string "null"
                // here when name is null
                var name = UChar.GetName(cp);
                sb.Append(name ?? "null");
            }
            return sb.ToString();
        }

        /// <icu/>
        /// <summary>
        /// Returns null.
        /// Used to return the Unicode_1_Name property value which was of little practical value.
        /// </summary>
        /// <param name="ch">The code point for which to get the name.</param>
        /// <returns>Null.</returns>
        [Obsolete("ICU 49")]
        public static string GetName1_0(int ch)
        {
            return null;
        }

        /// <icu/>
        /// <summary>
        /// Returns a name for a valid codepoint. Unlike, <see cref="GetName(int)"/> and
        /// <see cref="GetName1_0(int)"/>, this method will return a name even for codepoints that
        /// are not assigned a name in UnicodeData.txt.
        /// </summary>
        /// <remarks>
        /// The names are returned in the following order.
        /// <list type="bullet">
        ///     <item><description>Most current Unicode name if there is any</description></item>
        ///     <item><description>Unicode 1.0 name if there is any</description></item>
        ///     <item><description>Extended name in the form of 
        ///         "&lt;codepoint_type-codepoint_hex_digits&gt;". E.g., &lt;noncharacter-fffe&gt;</description></item>
        /// </list>
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </remarks>
        /// <param name="ch">The code point for which to get the name.</param>
        /// <returns>A name for the argument codepoint.</returns>
        /// <stable>ICU 2.6</stable>
        public static string GetExtendedName(int ch)
        {
            return UCharacterName.Instance.GetName(ch, UCharacterNameChoice.ExtendedCharName);
        }

        /// <icu/>
        /// <summary>
        /// Returns the corrected name from NameAliases.txt if there is one.
        /// Returns null if the character is unassigned or outside the range
        /// <see cref="UChar.MinValue"/> and <see cref="UChar.MaxValue"/>
        /// or does not have a name.
        /// <para/>
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </summary>
        /// <param name="ch">The code point for which to get the name alias.</param>
        /// <returns>Unicode name alias, or null.</returns>
        /// <stable>ICU 4.4</stable>
        public static string GetNameAlias(int ch)
        {
            return UCharacterName.Instance.GetName(ch, UCharacterNameChoice.CharNameAlias);
        }

        /// <icu/>
        /// <summary>
        /// Returns null.
        /// Used to return the ISO 10646 comment for a character.
        /// The Unicode <see cref="UProperty.ISO_Comment"/> property is deprecated and has no values.
        /// </summary>
        /// <param name="ch">The code point for which to get the ISO comment.
        /// It must be the case that <c>0 &lt;= <paramref name="ch"/> &lt;= 0x10ffff</c>.
        /// </param>
        /// <returns>null.</returns>
        [Obsolete("ICU 49")]
        public static string GetISOComment(int ch)
        {
            return null;
        }

        /// <icu/>
        /// <summary>
        /// Finds a Unicode code point by its most current Unicode name and
        /// return its code point value. All Unicode names are in uppercase.
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </summary>
        /// <param name="name">Most current Unicode character name whose code point is to
        /// be returned.</param>
        /// <returns>Code point or -1 if name is not found.</returns>
        /// <stable>ICU 2.1</stable>
        public static int GetCharFromName(string name)
        {
            return UCharacterName.Instance.GetCharFromName(
                    UCharacterNameChoice.UnicodeCharName, name);
        }

        /// <icu/>
        /// <summary>
        /// Returns -1.
        /// <para/>
        /// Used to find a Unicode character by its version 1.0 Unicode name and return
        /// its code point value.
        /// </summary>
        /// <param name="name">Unicode 1.0 code point name whose code point is to be
        /// returned.</param>
        /// <returns>-1</returns>
        /// <seealso cref="GetName1_0(int)"/>
        [Obsolete("ICU 49")]
        public static int GetCharFromName1_0(string name)
        {
            return -1;
        }

        /// <icu/>
        /// <summary>
        /// Find a Unicode character by either its name and return its code
        /// point value. 
        /// </summary>
        /// <remarks>
        /// All Unicode names are in uppercase.
        /// Extended names are all lowercase except for numbers and are contained
        /// within angle brackets.
        /// The names are searched in the following order
        /// <list type="bullet">
        ///     <item><description>Most current Unicode name if there is any</description></item>
        ///     <item><description>Unicode 1.0 name if there is any</description></item>
        ///     <item><description>Extended name in the form of
        ///         "&lt;codepoint_type-codepoint_hex_digits&gt;". E.g. &lt;noncharacter-FFFE&gt;</description></item>
        /// </list>
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </remarks>
        /// <param name="name">Codepoint name.</param>
        /// <returns>Code point associated with the name or -1 if the name is not
        /// found.</returns>
        /// <stable>ICU 2.6</stable>
        public static int GetCharFromExtendedName(string name)
        {
            return UCharacterName.Instance.GetCharFromName(
                    UCharacterNameChoice.ExtendedCharName, name);
        }

        /// <icu/>
        /// <summary>
        /// Find a Unicode character by its corrected name alias and return
        /// its code point value. All Unicode names are in uppercase.
        /// Note calling any methods related to code point names, e.g. Get*Name*()
        /// incurs a one-time initialization cost to construct the name tables.
        /// </summary>
        /// <param name="name">Unicode name alias whose code point is to be returned.</param>
        /// <returns>Code point or -1 if name is not found.</returns>
        /// <stable>ICU 4.4</stable>
        public static int GetCharFromNameAlias(string name)
        {
            return UCharacterName.Instance.GetCharFromName(UCharacterNameChoice.CharNameAlias, name);
        }

        /// <summary>
        /// Return the Unicode name for a given property, as given in the
        /// Unicode database file PropertyAliases.txt.  Most properties
        /// have more than one name.  The <paramref name="nameChoice"/> determines which one
        /// is returned.
        /// </summary>
        /// <remarks>
        /// In addition, this function maps the property
        /// <see cref="UProperty.General_Category_Mask"/> to the synthetic names "gcm" /
        /// "General_Category_Mask".  These names are not in
        /// PropertyAliases.txt.
        /// </remarks>
        /// <param name="property"><see cref="UProperty"/> selector.</param>
        /// <param name="nameChoice">
        /// <see cref="NameChoice"/> selector for which name
        /// to get.  All properties have a long name.  Most have a short
        /// name, but some do not.  Unicode allows for additional names; if
        /// present these will be returned by <see cref="NameChoice.Long"/> + i,
        /// where i=1, 2,...
        /// </param>
        /// <returns>
        /// A name, or null if Unicode explicitly defines no name
        /// ("n/a") for a given <paramref name="property"/>/<paramref name="nameChoice"/>.  
        /// If a given <paramref name="nameChoice"/> throws an exception, then all larger 
        /// values of <paramref name="nameChoice"/> will throw an exception.  If null is 
        /// returned for a given <paramref name="nameChoice"/>, then other 
        /// <paramref name="nameChoice"/> values may return non-null results.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="property"/> or 
        /// <paramref name="nameChoice"/> are invalid.</exception>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="NameChoice"/>
        /// <stable>ICU 2.4</stable>
        public static string GetPropertyName(UProperty property,
                NameChoice nameChoice)
        {
            return UPropertyAliases.Instance.GetPropertyName(property, nameChoice);
        }

        /// <summary>
        /// Return the Unicode name for a given property, as given in the
        /// Unicode database file PropertyAliases.txt.  Most properties
        /// have more than one name.  The <paramref name="nameChoice"/> determines which one
        /// is returned.
        /// </summary>
        /// <remarks>
        /// In addition, this function maps the property
        /// <see cref="UProperty.General_Category_Mask"/> to the synthetic names "gcm" /
        /// "General_Category_Mask".  These names are not in
        /// PropertyAliases.txt.
        /// </remarks>
        /// <param name="property"><see cref="UProperty"/> selector.</param>
        /// <param name="nameChoice">
        /// <see cref="NameChoice"/> selector for which name
        /// to get.  All properties have a long name.  Most have a short
        /// name, but some do not.  Unicode allows for additional names; if
        /// present these will be returned by <see cref="NameChoice.Long"/> + i,
        /// where i=1, 2,...
        /// </param>
        /// <param name="result">
        /// A name, or null if Unicode explicitly defines no name
        /// ("n/a") for a given <paramref name="property"/>/<paramref name="nameChoice"/>.
        /// If a given <paramref name="nameChoice"/> returns false, then all larger 
        /// values of <paramref name="nameChoice"/> will return false.  If null is
        /// returned for a given <paramref name="nameChoice"/>, then other
        /// <paramref name="nameChoice"/> values may return non-null results.
        /// </param>
        /// <returns>
        /// true if both <paramref name="property"/> or 
        /// <paramref name="nameChoice"/> are valid, othewise false.
        /// </returns>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="NameChoice"/>
        /// <stable>ICU4N 60.1.0</stable>
        // ICU4N specific
        public static bool TryGetPropertyName(UProperty property,
                NameChoice nameChoice, out string result) // ICU4N TODO: Tests
        {
            return UPropertyAliases.Instance.TryGetPropertyName(property, nameChoice, out result);
        }

        // ICU4N specific - GetPropertyEnum(ICharSequence propertyAlias) moved to UCharacterExtension.tt

        /// <summary>
        /// Return the Unicode name for a given property value, as given in
        /// the Unicode database file PropertyValueAliases.txt.  Most
        /// values have more than one name.  The <paramref name="nameChoice"/> determines
        /// which one is returned.
        /// </summary>
        /// <remarks>
        /// Note: Some of the names in PropertyValueAliases.txt can only be
        /// retrieved using <see cref="UProperty.General_Category_Mask"/>, not
        /// <see cref="UProperty.General_Category"/>.  These include: "C" / "Other", "L" /
        /// "Letter", "LC" / "Cased_Letter", "M" / "Mark", "N" / "Number", "P"
        /// / "Punctuation", "S" / "Symbol", and "Z" / "Separator".
        /// </remarks>
        /// <param name="property">
        /// <see cref="UProperty"/> selector constant.
        /// <see cref="UProperty.Int_Start"/> &lt;= property &lt; <see cref="UProperty.Int_Limit"/> or
        /// <see cref="UProperty.Binary_Start"/> &lt;= property &lt; <see cref="UProperty.Binary_Limit"/> or
        /// <see cref="UProperty.Mask_Start"/> &lt; = property &lt; <see cref="UProperty.Mask_Limit"/>.
        /// If out of range, null is returned.
        /// </param>
        /// <param name="value">
        /// Selector for a value for the given property.  In
        /// general, valid values range from 0 up to some maximum.  There
        /// are a few exceptions:
        /// <list type="number">
        ///     <item><desription>
        ///         <see cref="UProperty.Block"/> values begin at the
        ///         non-zero value <see cref="UChar.UnicodeBlock.BASIC_LATIN_ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.Canonical_Combining_Class"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.General_Category_Mask"/> values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UChar.GetType(int)"/>.  This allows grouped categories such as
        ///         [:L:] to be represented.  Mask values are non-contiguous.
        ///     </desription></item>
        /// </list>
        /// </param>
        /// <param name="nameChoice">
        /// <see cref="NameChoice"/> selector for which name
        /// to get.  All values have a long name.  Most have a short name,
        /// but some do not.  Unicode allows for additional names; if
        /// present these will be returned by <see cref="NameChoice.Long"/> + i,
        /// where i=1, 2,...
        /// </param>
        /// <returns>
        /// A name, or null if Unicode explicitly defines no name
        /// ("n/a") for a given property/value/nameChoice.  If a given
        /// <paramref name="nameChoice"/> throws an exception, then all larger values of
        /// <paramref name="nameChoice"/> will throw an exception.  If null is returned for a
        /// given <paramref name="nameChoice"/>, then other <paramref name="nameChoice"/> values may return
        /// non-null results.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="property"/>, 
        /// <paramref name="value"/>, or <paramref name="nameChoice"/> are invalid.</exception>
        /// <seealso cref="TryGetPropertyValueName(UProperty, int, NameChoice, out string)"/>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="NameChoice"/>
        /// <stable>ICU 2.4</stable>
        public static string GetPropertyValueName(UProperty property,
                    int value,
                    NameChoice nameChoice)
        {
            if ((property == UProperty.Canonical_Combining_Class
                    || property == UProperty.Lead_Canonical_Combining_Class
                    || property == UProperty.Trail_Canonical_Combining_Class)
                    && value >= UChar.GetIntPropertyMinValue(
                            UProperty.Canonical_Combining_Class)
                            && value <= UChar.GetIntPropertyMaxValue(
                                    UProperty.Canonical_Combining_Class)
#pragma warning disable 612, 618
                                    && nameChoice >= 0 && nameChoice < NameChoice.Count)
#pragma warning restore 612, 618
            {
                // this is hard coded for the valid cc
                // because PropertyValueAliases.txt does not contain all of them

                // ICU4N specific - using TryGet version instead of falling back on exception
                UPropertyAliases.Instance.TryGetPropertyValueName(property, value, nameChoice, out string result);
                return result;
            }
            return UPropertyAliases.Instance.GetPropertyValueName(property, value, nameChoice);
        }

        /// <summary>
        /// Get the Unicode name for a given property value, as given in
        /// the Unicode database file PropertyValueAliases.txt.  Most
        /// values have more than one name.  The <paramref name="nameChoice"/> determines
        /// which one is returned.
        /// <para/>
        /// This is similar to <see cref="GetPropertyValueName(UProperty, int, NameChoice)"/>,
        /// but returns a true/false result if a name or value cannot be found rather
        /// than throwing exceptions.
        /// </summary>
        /// <remarks>
        /// Note: Some of the names in PropertyValueAliases.txt can only be
        /// retrieved using <see cref="UProperty.General_Category_Mask"/>, not
        /// <see cref="UProperty.General_Category"/>.  These include: "C" / "Other", "L" /
        /// "Letter", "LC" / "Cased_Letter", "M" / "Mark", "N" / "Number", "P"
        /// / "Punctuation", "S" / "Symbol", and "Z" / "Separator".
        /// </remarks>
        /// <param name="property">
        /// <see cref="UProperty"/> selector constant.
        /// <see cref="UProperty.Int_Start"/> &lt;= property &lt; <see cref="UProperty.Int_Limit"/> or
        /// <see cref="UProperty.Binary_Start"/> &lt;= property &lt; <see cref="UProperty.Binary_Limit"/> or
        /// <see cref="UProperty.Mask_Start"/> &lt; = property &lt; <see cref="UProperty.Mask_Limit"/>.
        /// If out of range, null is returned.
        /// </param>
        /// <param name="value">
        /// Selector for a value for the given property.  In
        /// general, valid values range from 0 up to some maximum.  There
        /// are a few exceptions:
        /// <list type="number">
        ///     <item><desription>
        ///         <see cref="UProperty.Block"/> values begin at the
        ///         non-zero value <see cref="UChar.UnicodeBlock.BASIC_LATIN_ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.Canonical_Combining_Class"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         UProperty.GENERAL_CATEGORY_MASK values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UChar.GetType(int)"/>.  This allows grouped categories such as
        ///         [:L:] to be represented.  Mask values are non-contiguous.
        ///     </desription></item>
        /// </list>
        /// </param>
        /// <param name="nameChoice">
        /// <see cref="NameChoice"/> selector for which name
        /// to get.  All values have a long name.  Most have a short name,
        /// but some do not.  Unicode allows for additional names; if
        /// present these will be returned by <see cref="NameChoice.Long"/> + i,
        /// where i=1, 2,...
        /// </param>
        /// <param name="result">
        /// The Unicode name for a given property value, as given in
        /// the Unicode database file PropertyValueAliases.txt, or null
        /// if the lookup failed.
        /// </param>
        /// <returns>
        /// true if the operation succeeded, or false if Unicode explicitly
        /// defines no name ("n/a") for a given property/value/nameChoice.
        /// If a given <paramref name="nameChoice"/> returns false,
        /// then all larger values of <paramref name="nameChoice"/> will return false.
        /// If null is returned for a given <paramref name="nameChoice"/>, then 
        /// other <paramref name="nameChoice"/> values may return non-null results.
        /// </returns>
        /// <seealso cref="TryGetPropertyValueName(UProperty, int, NameChoice, out string)"/>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="NameChoice"/>
        /// <stable>ICU 2.4</stable>
        public static bool TryGetPropertyValueName(UProperty property,
               int value,
               NameChoice nameChoice, out string result) // ICU4N TODO: Tests
        {
            if ((property == UProperty.Canonical_Combining_Class
                    || property == UProperty.Lead_Canonical_Combining_Class
                    || property == UProperty.Trail_Canonical_Combining_Class)
                    && value >= UChar.GetIntPropertyMinValue(
                            UProperty.Canonical_Combining_Class)
                            && value <= UChar.GetIntPropertyMaxValue(
                                    UProperty.Canonical_Combining_Class)
#pragma warning disable 612, 618
                                    && nameChoice >= 0 && nameChoice < NameChoice.Count)
#pragma warning restore 612, 618
            {
                // this is hard coded for the valid cc
                // because PropertyValueAliases.txt does not contain all of them
                if (!UPropertyAliases.Instance.TryGetPropertyValueName(property, value, nameChoice, out result))
                {
                    result = null;
                }
                return true;
            }
            return UPropertyAliases.Instance.TryGetPropertyValueName(property, value, nameChoice, out result);
        }

        // ICU4N specific - GetPropertyValueEnum(UProperty property, ICharSequence valueAlias) moved to UCharacterExtension.tt

        /// <summary>
        /// Same as <see cref="GetPropertyValueEnum(UProperty, ICharSequence)"/>, except doesn't throw exception. Instead, returns <see cref="UProperty.Undefined"/>.
        /// </summary>
        /// <param name="property">Same as <see cref="GetPropertyValueEnum(UProperty, ICharSequence)"/>.</param>
        /// <param name="valueAlias">Same as <see cref="GetPropertyValueEnum(UProperty, ICharSequence)"/>.</param>
        /// <returns>Returns <see cref="UProperty.Undefined"/> if the value is not valid, otherwise the value.</returns>
        [Obsolete("ICU4N 60.1.0 Use TryGetPropertyValueEnum(UProperty property, ICharSequence valueAlias) instead.")]
        internal static int GetPropertyValueEnumNoThrow(UProperty property, ICharSequence valueAlias)
        {
            return UPropertyAliases.Instance.GetPropertyValueEnumNoThrow((int)property, valueAlias);
        }

        /// <icu/>
        /// <summary>
        /// Returns a code point corresponding to the two surrogate code units.
        /// </summary>
        /// <param name="lead">The lead char.</param>
        /// <param name="trail">The trail char.</param>
        /// <returns>Code point if surrogate characters are valid.</returns>
        /// <exception cref="ArgumentException">Thrown when the code units do not form a valid code point.</exception>
        /// <stable>ICU 2.1</stable>
        public static int GetCodePoint(char lead, char trail) // ICU4N TODO: API - rename ConvertToUtf32 to match Char
        {
            // ICU4N TODO: Perhaps we should just call char.ConvertToUtf32, since this is a duplicate of that functionality
            if (char.IsSurrogatePair(lead, trail))
            {
                return Character.ToCodePoint(lead, trail);
            }
            throw new ArgumentException("Illegal surrogate characters");
        }

        /// <icu/>
        /// <summary>
        /// Returns the code point corresponding to the BMP code point.
        /// </summary>
        /// <param name="char16">The BMP code point.</param>
        /// <returns>Code point if argument is a valid character.</returns>
        /// <exception cref="ArgumentException">Thrown when the code units do not form a valid code point.</exception>
        /// <stable>ICU 2.1</stable>
        public static int GetCodePoint(char char16) // ICU4N TODO: API - rename ConvertToUtf32 to match Char
        {
            if (UChar.IsLegal(char16))
            {
                return char16;
            }
            throw new ArgumentException("Illegal codepoint");
        }

        /// <summary>
        /// Returns the uppercase version of the argument string.
        /// Casing is dependent on the current culture and context-sensitive.
        /// </summary>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Uppercase version of the argument string.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ToUpper(string str)
        {
            return CaseMapImpl.ToUpper(GetDefaultCaseLocale(), 0, str);
        }

        /// <summary>
        /// Returns the lowercase version of the argument string.
        /// Casing is dependent on the current culture and context-sensitive.
        /// </summary>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Lowercase version of the argument string.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ToLower(string str)
        {
            return CaseMapImpl.ToLower(GetDefaultCaseLocale(), 0, str);
        }

        /// <summary>
        /// Returns the titlecase version of the argument string.
        /// </summary>
        /// <remarks>
        /// Position for titlecasing is determined by the argument break
        /// iterator, hence the user can customize his break iterator for
        /// a specialized titlecasing. In this case only the forward iteration
        /// needs to be implemented.
        /// If the break iterator passed in is null, the default Unicode algorithm
        /// will be used to determine the titlecase positions.
        /// <para/>
        /// Only positions returned by the break iterator will be title cased,
        /// character in between the positions will all be in lower case.
        /// <para/>
        /// Casing is dependent on the current culture and context-sensitive.
        /// </remarks>
        /// <param name="str">Source string to be performed on.</param>
        /// <param name="breakiter">Break iterator to determine the positions in which
        /// the character should be title cased.</param>
        /// <returns>Lowercase version of the argument string.</returns>
        /// <stable>ICU 2.6</stable>
        public static string ToTitleCase(string str, BreakIterator breakiter) // ICU4N TODO: API - create overload with no BreakIterator (that passes null)
        {
            return ToTitleCase(CultureInfo.CurrentCulture, str, breakiter, 0);
        }

        private static int GetDefaultCaseLocale() 
        {
            return UCaseProps.GetCaseLocale(CultureInfo.CurrentCulture);
        }

        private static int GetCaseLocale(CultureInfo locale)
        {
            if (locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            return UCaseProps.GetCaseLocale(locale);
        }

        private static int GetCaseLocale(ULocale locale)
        {
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            return UCaseProps.GetCaseLocale(locale);
        }

        /// <summary>
        /// Returns the uppercase version of the argument <paramref name="str"/>.
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale"><see cref="CultureInfo"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Uppercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ToUpper(CultureInfo locale, string str)
        {
            return CaseMapImpl.ToUpper(GetCaseLocale(locale), 0, str);
        }

        /// <summary>
        /// Returns the uppercase version of the argument <paramref name="str"/>.
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale"><see cref="ULocale"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Uppercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static string ToUpper(ULocale locale, string str)
        {
            return CaseMapImpl.ToUpper(GetCaseLocale(locale), 0, str);
        }

        /// <summary>
        /// Returns the lowercase version of the argument <paramref name="str"/>.
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale"><see cref="CultureInfo"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 2.1</stable>
        public static string ToLower(CultureInfo locale, string str)
        {
            return CaseMapImpl.ToLower(GetCaseLocale(locale), 0, str);
        }

        /// <summary>
        /// Returns the lowercase version of the argument <paramref name="str"/>.
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale"><see cref="ULocale"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static string ToLower(ULocale locale, string str)
        {
            return CaseMapImpl.ToLower(GetCaseLocale(locale), 0, str);
        }

        /// <summary>
        /// Returns the titlecase version of the argument <paramref name="str"/>.
        /// </summary>
        /// <remarks>
        /// Position for titlecasing is determined by the argument break
        /// iterator, hence the user can customize his break iterator for
        /// a specialized titlecasing. In this case only the forward iteration
        /// needs to be implemented.
        /// If the break iterator passed in is null, the default Unicode algorithm
        /// will be used to determine the titlecase positions.
        /// <para/>
        /// Only positions returned by the break iterator will be title cased,
        /// character in between the positions will all be in lower case.
        /// <para/>
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </remarks>
        /// <param name="locale"><see cref="CultureInfo"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <param name="breakiter">Break iterator to determine the positions in which
        /// the character should be title cased.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 2.6</stable>
        public static string ToTitleCase(CultureInfo locale, string str,
            BreakIterator breakiter)
        {
            return ToTitleCase(locale, str, breakiter, 0);
        }

        /// <summary>
        /// Returns the titlecase version of the argument <paramref name="str"/>.
        /// </summary>
        /// <remarks>
        /// Position for titlecasing is determined by the argument break
        /// iterator, hence the user can customize his break iterator for
        /// a specialized titlecasing. In this case only the forward iteration
        /// needs to be implemented.
        /// If the break iterator passed in is null, the default Unicode algorithm
        /// will be used to determine the titlecase positions.
        /// <para/>
        /// Only positions returned by the break iterator will be title cased,
        /// character in between the positions will all be in lower case.
        /// <para/>
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </remarks>
        /// <param name="locale"><see cref="ULocale"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <param name="titleIter">Break iterator to determine the positions in which
        /// the character should be title cased.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static string ToTitleCase(ULocale locale, string str,
            BreakIterator titleIter)
        {
            return ToTitleCase(locale, str, titleIter, 0);
        }

        /// <summary>
        /// Returns the titlecase version of the argument <paramref name="str"/>.
        /// </summary>
        /// <remarks>
        /// Position for titlecasing is determined by the argument break
        /// iterator, hence the user can customize his break iterator for
        /// a specialized titlecasing. In this case only the forward iteration
        /// needs to be implemented.
        /// If the break iterator passed in is null, the default Unicode algorithm
        /// will be used to determine the titlecase positions.
        /// <para/>
        /// Only positions returned by the break iterator will be title cased,
        /// character in between the positions will all be in lower case.
        /// <para/>
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </remarks>
        /// <param name="locale"><see cref="ULocale"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <param name="titleIter">Break iterator to determine the positions in which
        /// the character should be title cased.</param>
        /// <param name="options">Bit set to modify the titlecasing operation.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <stable>ICU 3.8</stable>
        /// <seealso cref="TitleCaseNoLowerCase"/>
        /// <seealso cref="TitleCaseNoBreakAdjustment"/>
        public static string ToTitleCase(ULocale locale, string str,
            BreakIterator titleIter, int options) // ICU4N TODO: API - make options into [Flags] enum
        {
            if (titleIter == null && locale == null)
            {
                locale = ULocale.GetDefault();
            }
            titleIter = CaseMapImpl.GetTitleBreakIterator(locale, options, titleIter);
            titleIter.SetText(str);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), options, titleIter, str);
        }

        /// <summary>
        /// Return a string with just the first word titlecased, for menus and UI, etc. This does not affect most of the string,
        /// and sometimes has no effect at all; the original string is returned whenever casing
        /// would not be appropriate for the first word (such as for CJK characters or initial numbers).
        /// Initial non-letters are skipped in order to find the character to change.
        /// Characters past the first affected are left untouched: see also <see cref="TitleCaseNoLowerCase"/>.
        /// <para/>
        /// Examples:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Source</term>
        ///         <term>Result</term>
        ///         <term>Locale</term>
        ///     </listheader>
        ///     <item>
        ///         <term>anglo-American locale</term>
        ///         <term>Anglo-American locale</term>
        ///         <term></term>
        ///     </item>
        ///     <item>
        ///         <term>“contact us”</term>
        ///         <term>“Contact us”</term>
        ///         <term></term>
        ///     </item>
        ///     <item>
        ///         <term>49ers win!</term>
        ///         <term>49ers win!</term>
        ///         <term></term>
        ///     </item>
        ///     <item>
        ///         <term>丰(abc)</term>
        ///         <term>丰(abc)</term>
        ///         <term></term>
        ///     </item>
        ///     <item>
        ///         <term>«ijs»</term>
        ///         <term>«Ijs»</term>
        ///         <term></term>
        ///     </item>
        ///     <item>
        ///         <term>«ijs»</term>
        ///         <term>«IJs»</term>
        ///         <term>nl-BE</term>
        ///     </item>
        ///     <item>
        ///         <term>«ijs»</term>
        ///         <term>«İjs»</term>
        ///         <term>tr-DE</term>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="locale">The locale for accessing exceptional behavior (eg for tr).</param>
        /// <param name="str">The source string to change.</param>
        /// <returns>The modified string, or the original if no modifications were necessary.</returns>
        /// <internal/>
        [Obsolete("ICU internal only")]
        public static string ToTitleFirst(ULocale locale, string str)
        {
            // TODO: Remove this function. Inline it where it is called in CLDR.
            return TO_TITLE_WHOLE_STRING_NO_LOWERCASE.Apply(locale.ToLocale(), null, str);
        }

        private static readonly CaseMap.Title TO_TITLE_WHOLE_STRING_NO_LOWERCASE =
                CaseMap.ToTitle().WholeString().NoLowercase();

        /// <icu/>
        /// <summary>
        /// Returns the titlecase version of the argument <paramref name="str"/>.
        /// </summary>
        /// <remarks>
        /// Position for titlecasing is determined by the argument break
        /// iterator, hence the user can customize his break iterator for
        /// a specialized titlecasing. In this case only the forward iteration
        /// needs to be implemented.
        /// If the break iterator passed in is null, the default Unicode algorithm
        /// will be used to determine the titlecase positions.
        /// <para/>
        /// Only positions returned by the break iterator will be title cased,
        /// character in between the positions will all be in lower case.
        /// <para/>
        /// Casing is dependent on the argument <paramref name="locale"/>.
        /// </remarks>
        /// <param name="locale"><see cref="ULocale"/> which <paramref name="str"/> is to be converted in.</param>
        /// <param name="str">Source string to be performed on.</param>
        /// <param name="titleIter">Break iterator to determine the positions in which
        /// the character should be title cased.</param>
        /// <param name="options">Bit set to modify the titlecasing operation.</param>
        /// <returns>Lowercase version of the argument <paramref name="str"/>.</returns>
        /// <seealso cref="TitleCaseNoLowerCase"/>
        /// <seealso cref="TitleCaseNoBreakAdjustment"/>
        /// <stable>ICU 54</stable>
        public static string ToTitleCase(CultureInfo locale, string str,
            BreakIterator titleIter,
            int options) // ICU4N TODO: API - make options into [Flags] enum
        {
            if (titleIter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            titleIter = CaseMapImpl.GetTitleBreakIterator(locale, options, titleIter);
            titleIter.SetText(str);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), options, titleIter, str);
        }

        /// <icu/>
        /// <summary>
        /// The given character is mapped to its case folding equivalent according
        /// to UnicodeData.txt and CaseFolding.txt; if the character has no case
        /// folding equivalent, the character itself is returned.
        /// <para/>
        /// This function only returns the simple, single-code point case mapping.
        /// Full case mappings should be used whenever possible because they produce
        /// better results by working on whole strings.
        /// They can map to a result string with a different length as appropriate.
        /// Full case mappings are applied by the case mapping functions
        /// that take string parameters rather than code points (int).
        /// See also the User Guide chapter on C/POSIX migration:
        /// <a href="http://www.icu-project.org/userguide/posix.html#case_mappings">
        /// http://www.icu-project.org/userguide/posix.html#case_mappings</a>
        /// </summary>
        /// <param name="ch">The character to be converted.</param>
        /// <param name="defaultmapping">
        /// Indicates whether the default mappings defined in
        /// CaseFolding.txt are to be used, otherwise the
        /// mappings for dotted I and dotless i marked with
        /// 'T' in CaseFolding.txt are included.
        /// </param>
        /// <returns>The case folding equivalent of the character, if
        /// any; otherwise the character itself.</returns>
        /// <seealso cref="FoldCase(string, bool)"/>
        /// <stable>ICU 2.1</stable>
        public static int FoldCase(int ch, bool defaultmapping)
        {
            return FoldCase(ch, defaultmapping ? Globalization.FoldCase.Default : Globalization.FoldCase.ExcludeSpecialI);
        }

        /// <icu/>
        /// <summary>
        /// The given string is mapped to its case folding equivalent according to
        /// UnicodeData.txt and CaseFolding.txt; if any character has no case
        /// folding equivalent, the character itself is returned.
        /// "Full", multiple-code point case folding mappings are returned here.
        /// For "simple" single-code point mappings use the API
        /// <see cref="FoldCase(int, bool)"/>.
        /// </summary>
        /// <param name="str">The string to be converted.</param>
        /// <param name="defaultmapping">
        /// Indicates whether the default mappings defined in
        /// CaseFolding.txt are to be used, otherwise the
        /// mappings for dotted I and dotless i marked with
        /// 'T' in CaseFolding.txt are included.
        /// </param>
        /// <returns>The case folding equivalent of the character, if
        /// any; otherwise the character itself.</returns>
        /// <seealso cref="FoldCase(int, bool)"/>
        /// <stable>ICU 2.1</stable>
        public static string FoldCase(string str, bool defaultmapping) // ICU4N TODO: API - Make context-sensitive overload based on current culture
        {
            return FoldCase(str, defaultmapping ? Globalization.FoldCase.Default : Globalization.FoldCase.ExcludeSpecialI);
        }

        /// <icu/>
        /// <summary>
        /// Option value for case folding: use default mappings defined in
        /// CaseFolding.txt.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        internal const int FoldCaseDefault = (int)Globalization.FoldCase.Default; // ICU4N specific - changed from public to internal because we use enum in .NET

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
        internal const int FoldCaseExcludeSpecialI = (int)Globalization.FoldCase.ExcludeSpecialI; // ICU4N specific - changed from public to internal because we use enum in .NET

        /// <icu/>
        /// <summary>
        /// The given character is mapped to its case folding equivalent according
        /// to UnicodeData.txt and CaseFolding.txt; if the character has no case
        /// folding equivalent, the character itself is returned.
        /// <para/>
        /// This function only returns the simple, single-code point case mapping.
        /// Full case mappings should be used whenever possible because they produce
        /// better results by working on whole strings.
        /// They can map to a result string with a different length as appropriate.
        /// Full case mappings are applied by the case mapping functions
        /// that take string parameters rather than code points (int).
        /// See also the User Guide chapter on C/POSIX migration:
        /// <a href="http://www.icu-project.org/userguide/posix.html#case_mappings">
        /// http://www.icu-project.org/userguide/posix.html#case_mappings</a>
        /// </summary>
        /// <param name="ch">The character to be converted.</param>
        /// <param name="foldCase">Option for special processing. Currently the recognised options
        /// are <see cref="Globalization.FoldCase.ExcludeSpecialI"/> and <see cref="Globalization.FoldCase.Default"/>.</param>
        /// <returns>The case folding equivalent of the character, if any; otherwise the
        /// character itself.</returns>
        /// <seealso cref="FoldCase(string, FoldCase)"/>
        /// <stable>ICU 2.6</stable>
        public static int FoldCase(int ch, FoldCase foldCase)
        {
            return UCaseProps.Instance.Fold(ch, foldCase);
        }

        /// <icu/>
        /// <summary>
        /// The given string is mapped to its case folding equivalent according to
        /// UnicodeData.txt and CaseFolding.txt; if any character has no case
        /// folding equivalent, the character itself is returned.
        /// "Full", multiple-code point case folding mappings are returned here.
        /// For "simple" single-code point mappings use the API
        /// <see cref="FoldCase(int, FoldCase)"/>.
        /// </summary>
        /// <param name="str">The string to be converted.</param>
        /// <param name="foldCase">Option for special processing. Currently the recognised options
        /// are <see cref="Globalization.FoldCase.ExcludeSpecialI"/> and <see cref="Globalization.FoldCase.Default"/>.</param>
        /// <returns>The case folding equivalent of the character, if any; otherwise the
        /// character itself.</returns>
        /// <seealso cref="FoldCase(int, FoldCase)"/>
        /// <stable>ICU 2.6</stable>
        public static string FoldCase(string str, FoldCase foldCase)
        {
            return CaseMapImpl.Fold((int)foldCase, str);
        }

        /// <icu/>
        /// <summary>
        /// Returns the numeric value of a Han character.
        /// <para/>
        /// This returns the value of Han 'numeric' code points,
        /// including those for zero, ten, hundred, thousand, ten thousand,
        /// and hundred million.
        /// This includes both the standard and 'checkwriting'
        /// characters, the 'big circle' zero character, and the standard
        /// zero character.
        /// <para/>
        /// Note: The Unicode Standard has numeric values for more
        /// Han characters recognized by this method
        /// (see <see cref="GetNumericValue(int)"/> and the UCD file DerivedNumericValues.txt),
        /// and a <see cref="Text.NumberFormat"/> can be used with
        /// a Chinese <see cref="Text.NumberingSystem"/>.
        /// </summary>
        /// <param name="ch">Code point to query.</param>
        /// <returns>Value if it is a Han 'numeric character,' otherwise return -1.</returns>
        /// <stable>ICU 2.4</stable>
        public static int GetHanNumericValue(int ch)
        {
            switch (ch)
            {
                case IDEOGRAPHIC_NUMBER_ZERO_:
                case CJK_IDEOGRAPH_COMPLEX_ZERO_:
                    return 0; // Han Zero
                case CJK_IDEOGRAPH_FIRST_:
                case CJK_IDEOGRAPH_COMPLEX_ONE_:
                    return 1; // Han One
                case CJK_IDEOGRAPH_SECOND_:
                case CJK_IDEOGRAPH_COMPLEX_TWO_:
                    return 2; // Han Two
                case CJK_IDEOGRAPH_THIRD_:
                case CJK_IDEOGRAPH_COMPLEX_THREE_:
                    return 3; // Han Three
                case CJK_IDEOGRAPH_FOURTH_:
                case CJK_IDEOGRAPH_COMPLEX_FOUR_:
                    return 4; // Han Four
                case CJK_IDEOGRAPH_FIFTH_:
                case CJK_IDEOGRAPH_COMPLEX_FIVE_:
                    return 5; // Han Five
                case CJK_IDEOGRAPH_SIXTH_:
                case CJK_IDEOGRAPH_COMPLEX_SIX_:
                    return 6; // Han Six
                case CJK_IDEOGRAPH_SEVENTH_:
                case CJK_IDEOGRAPH_COMPLEX_SEVEN_:
                    return 7; // Han Seven
                case CJK_IDEOGRAPH_EIGHTH_:
                case CJK_IDEOGRAPH_COMPLEX_EIGHT_:
                    return 8; // Han Eight
                case CJK_IDEOGRAPH_NINETH_:
                case CJK_IDEOGRAPH_COMPLEX_NINE_:
                    return 9; // Han Nine
                case CJK_IDEOGRAPH_TEN_:
                case CJK_IDEOGRAPH_COMPLEX_TEN_:
                    return 10;
                case CJK_IDEOGRAPH_HUNDRED_:
                case CJK_IDEOGRAPH_COMPLEX_HUNDRED_:
                    return 100;
                case CJK_IDEOGRAPH_THOUSAND_:
                case CJK_IDEOGRAPH_COMPLEX_THOUSAND_:
                    return 1000;
                case CJK_IDEOGRAPH_TEN_THOUSAND_:
                    return 10000;
                case CJK_IDEOGRAPH_HUNDRED_MILLION_:
                    return 100000000;
                default:
                    return -1; // no value
            }

        }

        /// <icu/>
        /// <summary>
        /// Returns an iterator for character types, iterating over codepoints.
        /// <para/>
        /// Example of use:
        /// <code>
        /// IRangeValueEnumerator iterator = UChar.GetTypeEnumerator();
        /// while (iterator.MoveNext())
        /// {
        ///     Console.WriteLine("Codepoint \\u" +
        ///                         iterator.Current.Start.ToHexString() +
        ///                         " to codepoint \\u" +
        ///                         (iterator.Current.Limit - 1).ToHexString() +
        ///                         " has the character type " +
        ///                         iterator.Current.Value);
        /// }
        /// </code>
        /// </summary>
        /// <remarks>
        /// This is equivalent to getTypeIterator() in ICU4J.
        /// </remarks>
        /// <returns>An enumerator.</returns>
        /// <stable>ICU 2.6</stable>
        public static IRangeValueEnumerator GetTypeEnumerator()
        {
            return new UCharacterTypeEnumerator();
        }

        private sealed class UCharacterTypeEnumerator : IRangeValueEnumerator
        {
            private RangeValueEnumeratorElement current;

            internal UCharacterTypeEnumerator()
            {
                Reset();
            }

            public RangeValueEnumeratorElement Current => current;

            object IEnumerator.Current => current;

            // implements IRangeValueEnumerator
            public void Reset()
            {
                trieIterator = UCharacterProperty.Instance.Trie.GetEnumerator(MASK_TYPE);
            }

            // implements IRangeValueEnumerator
            public bool MoveNext()
            {
                if (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                {
                    current = new RangeValueEnumeratorElement
                    {
                        Start = range.StartCodePoint,
                        Limit = range.EndCodePoint + 1,
                        Value = range.Value
                    };
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // implements IRangeValueEnumerator
            public void Dispose()
            {
                if (trieIterator != null)
                    trieIterator.Dispose();
            }

            private IEnumerator<Trie2.Range> trieIterator;
            private Trie2.Range range;

            private sealed class MaskType : IValueMapper
            {
                // Extracts the general category ("character type") from the trie value.
                public int Map(int value)
                {
                    return value & UCharacterProperty.TYPE_MASK;
                }
            }
            private static readonly MaskType MASK_TYPE = new MaskType();
        }

        /// <icu/>
        /// <summary>
        /// Returns an enumerator for character names, iterating over codepoints.
        /// </summary>
        /// <remarks>
        /// This API only gets the iterator for the modern, most up-to-date
        /// Unicode names. For older 1.0 Unicode names use <see cref="GetName1_0Enumerator"/> or
        /// for extended names use <see cref="GetExtendedNameEnumerator()"/>.
        /// <para/>
        /// Example of use:
        /// <code>
        /// IValueEnumerator iterator = UChar.GetNameEnumerator();
        /// while (iterator.MoveNext())
        /// {
        ///     Console.WriteLine("Codepoint \\u" +
        ///                         (iterator.Current.Codepoint).ToHexString() +
        ///                         " has the name " + (string)iterator.Current.Value);
        /// }
        /// </code>
        /// <para/>
        /// The maximal range which the name iterator iterates is from
        /// <see cref="UChar.MinValue"/> to <see cref="UChar.MaxValue"/>
        /// <para/>
        /// NOTE: This is equivalent to getNameIterator() in ICU4J
        /// </remarks>
        /// <returns>An enumerator.</returns>
        /// <stable>ICU 2.6</stable>
        public static IValueEnumerator GetNameEnumerator()
        {
            return new UCharacterNameEnumerator(UCharacterName.Instance,
                    UCharacterNameChoice.UnicodeCharName);
        }

        /// <icu/>
        /// <summary>
        /// Returns an empty enumerator.
        /// <para/>
        /// Used to return an iterator for the older 1.0 Unicode character names, iterating over codepoints.
        /// </summary>
        /// <returns>An empty enumerator.</returns>
        /// <seealso cref="GetName1_0(int)"/>
        // NOTE: This is equivalent to getName1_0Iterator() in ICU4J
        [Obsolete("ICU 49")]
        public static IValueEnumerator GetName1_0Enumerator()
        {
            return new DummyValueEnumerator();
        }

        private sealed class DummyValueEnumerator : IValueEnumerator
        {
            public ValueEnumeratorElement Current => null;

            object IEnumerator.Current => null;

            public void Dispose()
            {
                // nothing to do
            }

            public bool MoveNext() { return false; }

            public void Reset() { }

            public void SetRange(int start, int limit) { }
        }

        /// <icu/>
        /// <summary>
        /// Returns an enumerator for character names, iterating over codepoints.
        /// </summary>
        /// <remarks>
        /// This API only gets the enumerator for the extended names.
        /// For modern, most up-to-date Unicode names use <see cref="GetNameEnumerator()"/> or
        /// for older 1.0 Unicode names use <see cref="GetName1_0Enumerator()"/>.
        /// <para/>
        /// Example of use:
        /// <code>
        /// IValueEnumerator iterator = UChar.GetExtendedNameIterator();
        /// while (iterator.MoveNext())
        /// {
        ///     Console.WriteLine("Codepoint \\u" +
        ///                         (iterator.Current.Codepoint).ToHexString() +
        ///                         " has the name " + (string)iterator.Current.Value);
        /// }
        /// </code>
        /// <para/>
        /// The maximal range which the name iterator iterates is from
        /// <see cref="UChar.MinValue"/> to <see cref="UChar.MaxValue"/>.
        /// <para/>
        /// NOTE: This is equivalent to getExtendedNameIterator() in ICU4J
        /// </remarks>
        /// <returns>An enumerator.</returns>
        /// <stable>ICU 2.6</stable>
        public static IValueEnumerator GetExtendedNameEnumerator()
        {
            return new UCharacterNameEnumerator(UCharacterName.Instance,
                    UCharacterNameChoice.ExtendedCharName);
        }

        /// <icu/>
        /// <summary>
        /// Returns the "age" of the code point.
        /// <para/>
        /// The "age" is the Unicode version when the code point was first
        /// designated (as a non-character or for Private Use) or assigned a
        /// character.
        /// <para/>
        /// This can be useful to avoid emitting code points to receiving
        /// processes that do not accept newer characters.
        /// <para/>
        /// The data is from the UCD file DerivedAge.txt.
        /// </summary>
        /// <param name="ch">The code point.</param>
        /// <returns>The Unicode version number.</returns>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetAge(int ch)
        {
            if (ch < MinValue || ch > MaxValue)
            {
                throw new ArgumentException("Codepoint out of bounds");
            }
            return UCharacterProperty.Instance.GetAge(ch);
        }

        /// <icu/>
        /// <summary>
        /// Check a binary Unicode property for a code point.
        /// <para/>
        /// Unicode, especially in version 3.2, defines many more properties
        /// than the original set in UnicodeData.txt.
        /// <para/>
        /// This API is intended to reflect Unicode properties as defined in
        /// the Unicode Character Database (UCD) and Unicode Technical Reports
        /// (UTR).
        /// <para/>
        /// For details about the properties see
        /// <a href="http://www.unicode.org/">http://www.unicode.org/</a>.
        /// <para/>
        /// For names of Unicode properties see the UCD file
        /// PropertyAliases.txt.
        /// <para/>
        /// This API does not check the validity of the codepoint.
        /// <para/>
        /// Important: If ICU is built with UCD files from Unicode versions
        /// below 3.2, then properties marked with "new" are not or
        /// not fully available.
        /// </summary>
        /// <param name="ch">Code point to test.</param>
        /// <param name="property">Selector constant from <see cref="UProperty"/>,
        /// identifies which binary property to check.</param>
        /// <returns>
        /// true or false according to the binary Unicode property value
        /// for <paramref name="ch"/>. Also false if <paramref name="property"/> 
        /// is out of bounds or if the Unicode version does not have data for 
        /// the property at all, or not for this code point.
        /// </returns>
        /// <seealso cref="UProperty"/>
        /// <stable>ICU 2.6</stable>
        public static bool HasBinaryProperty(int ch, UProperty property)
        {
            return UCharacterProperty.Instance.HasBinaryProperty(ch, (int)property);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Alphabetic"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Alphabetic"/>.
        /// <para/>
        /// Different from <see cref="IsLetter(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsUAlphabetic(int ch)
        {
            return HasBinaryProperty(ch, UProperty.Alphabetic);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Lowercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Lowercase"/>.
        /// <para/>
        /// This is different from <see cref="IsLowerCase(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsULowercase(int ch) // ICU4N TODO: API - IsUpperCase vs IsUUppercase vs ToUpper - drop "case" from name
        {
            return HasBinaryProperty(ch, UProperty.Lowercase);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Uppercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Uppercase"/>.
        /// <para/>
        /// This is different from <see cref="IsUpperCase(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsUUppercase(int ch) // ICU4N TODO: API - IsUpperCase vs IsUUppercase vs ToUpper - drop "case" from name
        {
            return HasBinaryProperty(ch, UProperty.Uppercase);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.White_Space"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.White_Space"/>.
        /// <para/>
        /// This is different from both <see cref="IsSpace(int)"/> and <see cref="IsWhitespace(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsUWhiteSpace(int ch)
        {
            return HasBinaryProperty(ch, UProperty.White_Space);
        }

        /// <icu/>
        /// <summary>
        /// Returns the property value for an Unicode property type of a code point.
        /// Also returns binary and mask property values. 
        /// </summary>
        /// <remarks>
        /// Unicode, especially in version 3.2, defines many more properties than
        /// the original set in UnicodeData.txt.
        /// <para/>
        /// The properties APIs are intended to reflect Unicode properties as
        /// defined in the Unicode Character Database (UCD) and Unicode Technical
        /// Reports (UTR). For details about the properties see
        /// <a href="http://www.unicode.org/">http://www.unicode.org/</a>.
        /// <para/>
        /// For names of Unicode properties see the UCD file PropertyAliases.txt.
        /// <para/>
        /// Sample usage:
        /// <code>
        /// int ea = UChar.GetInt32PropertyValue(c, UProperty.East_Asian_Width);
        /// int ideo = UChar.GetInt32PropertyValue(c, UProperty.Ideographic);
        /// bool b = (ideo == 1) ? true : false;
        /// </code>
        /// </remarks>
        /// <param name="ch">Code point to test.</param>
        /// <param name="type">UProperty selector constant, identifies which binary
        /// property to check. Must be
        /// <see cref="UProperty.Binary_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Binary_Limit"/> or
        /// <see cref="UProperty.Int_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Int_Limit"/> or
        /// <see cref="UProperty.Mask_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Mask_Limit"/>.
        /// </param>
        /// <returns>
        /// Numeric value that is directly the property value or,
        /// for enumerated properties, corresponds to the numeric value of
        /// the enumerated constant (or enum) of the respective property value
        /// enumeration type (cast to enum type if necessary).
        /// Returns 0 or 1 (for false / true) for binary Unicode properties.
        /// Returns a bit-mask for mask properties.
        /// Returns 0 if 'type' is out of bounds or if the Unicode version
        /// does not have data for the property at all, or not for this code
        /// point.
        /// </returns>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="HasBinaryProperty(int, UProperty)"/>
        /// <seealso cref="GetIntPropertyMinValue(UProperty)"/>
        /// <seealso cref="GetIntPropertyMaxValue(UProperty)"/>
        /// <seealso cref="UnicodeVersion"/>
        /// <stable>ICU 2.4</stable>
        public static int GetInt32PropertyValue(int ch, UProperty type) // ICU4N TODO: API - rename back to GetIntPropertyValue (we don't have to discern between different data types)
        {
            return UCharacterProperty.Instance.GetInt32PropertyValue(ch, (int)type);
        }

        /// <icu/>
        /// <summary>
        /// Returns a string version of the property value.
        /// </summary>
        /// <param name="propertyEnum">The property enum value.</param>
        /// <param name="codepoint">The codepoint value.</param>
        /// <param name="nameChoice">The choice of the name.</param>
        /// <returns>Value as string.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        //CLOVER:OFF
        public static string GetStringPropertyValue(UProperty propertyEnum, int codepoint, NameChoice nameChoice)
        {
            if ((propertyEnum >= UProperty.Binary_Start && propertyEnum < UProperty.Binary_Limit) ||
                    (propertyEnum >= UProperty.Int_Start && propertyEnum < UProperty.Int_Limit))
            {
                return GetPropertyValueName(propertyEnum, GetInt32PropertyValue(codepoint, propertyEnum),
                        nameChoice);
            }
            if (propertyEnum == UProperty.Numeric_Value)
            {
                return GetUnicodeNumericValue(codepoint).ToString(CultureInfo.InvariantCulture);
            }
            // otherwise must be string property
            switch (propertyEnum)
            {
                case UProperty.Age: return GetAge(codepoint).ToString();
                case UProperty.ISO_Comment: return GetISOComment(codepoint);
                case UProperty.Bidi_Mirroring_Glyph: return ToString(GetMirror(codepoint));
                case UProperty.Case_Folding: return ToString(FoldCase(codepoint, true));
                case UProperty.Lowercase_Mapping: return ToString(ToLower(codepoint));
                case UProperty.Name: return GetName(codepoint);
                case UProperty.Simple_Case_Folding: return ToString(FoldCase(codepoint, true));
                case UProperty.Simple_Lowercase_Mapping: return ToString(ToLower(codepoint));
                case UProperty.Simple_Titlecase_Mapping: return ToString(ToTitleCase(codepoint));
                case UProperty.Simple_Uppercase_Mapping: return ToString(ToUpper(codepoint));
                case UProperty.Titlecase_Mapping: return ToString(ToTitleCase(codepoint));
                case UProperty.Unicode_1_Name: return GetName1_0(codepoint);
                case UProperty.Uppercase_Mapping: return ToString(ToUpper(codepoint));
            }
            throw new ArgumentException("Illegal Property Enum");
        }
        //CLOVER:ON

        /// <icu/>
        /// <summary>
        /// Returns the minimum value for an integer/binary Unicode property type.
        /// Can be used together with <see cref="GetIntPropertyMaxValue(UProperty)"/>
        /// to allocate arrays of <see cref="Text.UnicodeSet"/> or similar.
        /// </summary>
        /// <param name="type"><see cref="UProperty"/> selector constant, identifies which binary
        /// property to check. Must be
        /// <see cref="UProperty.Binary_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Binary_Limit"/> or
        /// <see cref="UProperty.Int_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Int_Limit"/>.
        /// </param>
        /// <returns>
        /// Minimum value returned by <see cref="GetInt32PropertyValue(int, UProperty)"/>
        /// for a Unicode property. 0 if the property
        /// selector 'type' is out of range.
        /// </returns>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="HasBinaryProperty(int, UProperty)"/>
        /// <seealso cref="UnicodeVersion"/>
        /// <seealso cref="GetIntPropertyMaxValue(UProperty)"/>
        /// <seealso cref="GetInt32PropertyValue(int, UProperty)"/>
        /// <stable>ICU 2.4</stable>
        public static int GetIntPropertyMinValue(UProperty type)
        {
            return 0; // undefined; and: all other properties have a minimum value of 0
        }

        /// <icu/>
        /// <summary>
        /// Returns the maximum value for an integer/binary Unicode property.
        /// Can be used together with <see cref="GetIntPropertyMinValue(UProperty)"/>
        /// to allocate arrays of <see cref="Text.UnicodeSet"/> or similar.
        /// </summary>
        /// <remarks>
        /// Examples for min/max values (for Unicode 3.2):
        /// <list type="bullet">
        ///     <item><description>
        ///         <see cref="UProperty.BiDi_Class"/>:    0/18
        ///         (<see cref="UCharacterDirection.LeftToRight"/>/<see cref="UCharacterDirection.BoundaryNeutral"/>)
        ///     </description></item>
        ///     <item><description>
        ///         <see cref="UProperty.Script"/>:        0/45 (<see cref="UScript.Common"/>/<see cref="UScript.Tagbanwa"/>)
        ///     </description></item>
        ///     <item><description>
        ///         <see cref="UProperty.Ideographic"/>:   0/1  (false/true)
        ///     </description></item>
        /// </list>
        /// For undefined <see cref="UProperty"/> enum values, min/max values will be 0/-1.
        /// </remarks>
        /// <param name="type">
        /// <see cref="UProperty"/> selector constant, identifies which binary
        /// property to check. Must be
        /// <see cref="UProperty.Binary_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Binary_Limit"/> or
        /// <see cref="UProperty.Int_Start"/> &lt;= <paramref name="type"/> &lt; <see cref="UProperty.Int_Limit"/>.
        /// </param>
        /// <returns>
        /// Maximum value returned by <see cref="GetInt32PropertyValue(int, UProperty)"/> for a Unicode
        /// property. &lt;= 0 if the property selector '<paramref name="type"/>' is out of range.
        /// </returns>
        public static int GetIntPropertyMaxValue(UProperty type)
        {
            return UCharacterProperty.Instance.GetIntPropertyMaxValue((int)type);
        }

        /// <summary>
        /// Provide the <see cref="Character.ForDigit(int, int)"/> API, for convenience.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public static char ForDigit(int digit, int radix)
        {
            return Character.ForDigit(digit, radix);
        }

        // JDK 1.5 API coverage

        /// <summary>
        /// Constant U+D800, same as <see cref="Character.MIN_HIGH_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinHighSurrogate = Character.MIN_HIGH_SURROGATE;

        /// <summary>
        /// Constant U+DBFF, same as <see cref="Character.MAX_HIGH_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxHighSurrogate = Character.MAX_HIGH_SURROGATE;

        /// <summary>
        /// Constant U+DC00, same as <see cref="Character.MIN_LOW_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinLowSurrogate = Character.MIN_LOW_SURROGATE;

        /// <summary>
        /// Constant U+DFFF, same as <see cref="Character.MAX_LOW_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxLowSurrogate = Character.MAX_LOW_SURROGATE;

        /// <summary>
        /// Constant U+D800, same as <see cref="Character.MIN_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinSurrogate = Character.MIN_SURROGATE;

        /// <summary>
        /// Constant U+DFFF, same as <see cref="Character.MAX_SURROGATE"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxSurrogate = Character.MAX_SURROGATE;

        /// <summary>
        /// Constant U+10000, same as <see cref="Character.MIN_SUPPLEMENTARY_CODE_POINT"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MinSupplementaryCodePoint = Character.MIN_SUPPLEMENTARY_CODE_POINT;

        /// <summary>
        /// Constant U+10FFFF, same as <see cref="Character.MAX_CODE_POINT"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MaxCodePoint = Character.MAX_CODE_POINT;

        /// <summary>
        /// Constant U+0000, same as <see cref="Character.MIN_CODE_POINT"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MinCodePoint = Character.MIN_CODE_POINT;

        /// <summary>
        /// Equivalent to <see cref="Character.IsValidCodePoint(int)"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <returns>true if <paramref name="cp"/> is a valid code point.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsValidCodePoint(int cp)
        {
            return cp >= 0 && cp <= MaxCodePoint;
        }

        /// <summary>
        /// Same as <see cref="Character.IsSupplementaryCodePoint(int)"/>.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <returns>true if <paramref name="cp"/> is a supplementary code point.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSupplementaryCodePoint(int cp)
        {
            return Character.IsSupplementaryCodePoint(cp);
        }

        /// <summary>
        /// Same as <see cref="Char.IsHighSurrogate(char)"/>.
        /// </summary>
        /// <param name="ch">The <see cref="char"/> to check.</param>
        /// <returns>true if <paramref name="ch"/> is a high (lead) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsHighSurrogate(char ch) // ICU4N TODO: API Add overload for string, int
        {
            return char.IsHighSurrogate(ch);
        }

        /// <summary>
        /// Same as <see cref="Char.IsLowSurrogate(char)"/>.
        /// </summary>
        /// <param name="ch">The <see cref="char"/> to check.</param>
        /// <returns>true if <paramref name="ch"/> is a low (trail) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsLowSurrogate(char ch) // ICU4N TODO: API Add overload for string, int
        {
            return char.IsLowSurrogate(ch);
        }

        /// <summary>
        /// Same as <see cref="Char.IsSurrogatePair(char, char)"/>.
        /// </summary>
        /// <param name="high">The high (lead) <see cref="char"/>.</param>
        /// <param name="low">The low (trail) <see cref="char"/>.</param>
        /// <returns>true if <paramref name="high"/>, <paramref name="low"/> form a surrogate pair.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSurrogatePair(char high, char low) // ICU4N TODO: API Add overload for string, int
        {
            return char.IsSurrogatePair(high, low);
        }

        /// <summary>
        /// Same as <see cref="Character.CharCount(int)"/>.
        /// Returns the number of chars needed to represent the code point (1 or 2).
        /// This does not check the code point for validity.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <returns>The number of chars needed to represent the code point.</returns>
        /// <stable>ICU 3.0</stable>
        public static int CharCount(int cp)
        {
            return Character.CharCount(cp);
        }

        /// <summary>
        /// Similar to <see cref="char.ConvertToUtf32(char, char)"/>.
        /// Returns the code point represented by the two surrogate code units.
        /// However, this does not check the surrogate pair for validity.
        /// </summary>
        /// <param name="high">The high (lead) surrogate.</param>
        /// <param name="low">The low (trail) surrogate.</param>
        /// <returns>The code point formed by the surrogate pair.</returns>
        /// <stable>ICU 3.0</stable>
        public static int ToCodePoint(char high, char low) // ICU4N TODO: API - rename ConvertToUtf32 to match Char, add overload for (string, int)
        {
            return Character.ToCodePoint(high, low);
        }


        // ICU4N specific - CodePointAt(ICharSequence seq, int index) moved to UCharacterExtension.tt

        // ICU4N specific - CodePointAt(char[] seq, int index) moved to UCharacterExtension.tt

        /// <summary>
        /// Returns the code point at index.
        /// This examines only the characters at index and index+1.
        /// </summary>
        /// <param name="text">The characters to check.</param>
        /// <param name="index">The index of the first or only char forming the code point.</param>
        /// <param name="limit">The limit of the valid text.</param>
        /// <returns>The code point at the index.</returns>
        /// <stable>ICU 3.0</stable>
        public static int CodePointAt(char[] text, int index, int limit)
        {
            if (index >= limit || limit > text.Length)
            {
                throw new IndexOutOfRangeException();
            }
            char c1 = text[index++];
            if (IsHighSurrogate(c1))
            {
                if (index < limit)
                {
                    char c2 = text[index];
                    if (IsLowSurrogate(c2))
                    {
                        return ToCodePoint(c1, c2);
                    }
                }
            }
            return c1;
        }

        // ICU4N specific - CodePointBefore(ICharSequence seq, int index) moved to UCharacterExtension.tt

        // ICU4N specific - CodePointBefore(char[] seq, int index) moved to UCharacterExtension.tt

        /// <summary>
        /// Return the code point before index.
        /// This examines only the characters at index-1 and index-2.
        /// </summary>
        /// <param name="text">The characters to check.</param>
        /// <param name="index">The index after the last or only char forming the code point.</param>
        /// <param name="limit">The start of the valid text.</param>
        /// <returns>The code point before the index.</returns>
        /// <stable>ICU 3.0</stable>
        public static int CodePointBefore(char[] text, int index, int limit)
        {
            if (index <= limit || limit < 0)
            {
                throw new IndexOutOfRangeException();
            }
            char c2 = text[--index];
            if (IsLowSurrogate(c2))
            {
                if (index > limit)
                {
                    char c1 = text[--index];
                    if (IsHighSurrogate(c1))
                    {
                        return ToCodePoint(c1, c2);
                    }
                }
            }
            return c2;
        }

        /// <summary>
        /// Writes the chars representing the
        /// code point into the destination at the given index.
        /// </summary>
        /// <param name="cp">The code point to convert.</param>
        /// <param name="dst">The destination array into which to put the char(s) representing the code point.</param>
        /// <param name="dstIndex">The index at which to put the first (or only) char.</param>
        /// <returns>The count of the number of chars written (1 or 2).</returns>
        /// <exception cref="ArgumentException">If <paramref name="cp"/> is not a valid code point.</exception>
        /// <stable>ICU 3.0</stable>
        public static int ToChars(int cp, char[] dst, int dstIndex)
        {
            return Character.ToChars(cp, dst, dstIndex);
        }

        /// <summary>
        /// Returns a char array representing the code point.
        /// </summary>
        /// <param name="cp">The code point to convert.</param>
        /// <returns>An array containing the char(s) representing the code point.</returns>
        /// <exception cref="ArgumentException">If <paramref name="cp"/> is not a valid code point.</exception>
        /// <stable>ICU 3.0</stable>
        public static char[] ToChars(int cp)
        {
            return Character.ToChars(cp);
        }

        /// <summary>
        /// Returns a value representing the directionality of the character.
        /// </summary>
        /// <param name="cp">The code point to check.</param>
        /// <returns>The directionality of the code point.</returns>
        /// <see cref="GetDirection(int)"/>
        /// <stable>ICU 3.0</stable>
        // ICU4N TODO: API This is exactly the same as GetDirection() (except for the return type). Do we really need it?
        public static byte GetDirectionality(int cp) // ICU4N TODO: API return UCharacterDirection type
        {
            return (byte)GetDirection(cp);
        }

        // ICU4N specific - CodePointCount(ICharSequence text, int start, int limit) moved to UCharacterExtension.tt

        // ICU4N specific - CodePointCount(char[] text, int start, int limit) moved to UCharacterExtension.tt

        // ICU4N specific - OffsetByCodePoints(ICharSequence text, int index, int codePointOffset) moved to UCharacterExtension.tt

        /// <summary>
        /// Adjusts the char index by a code point offset.
        /// </summary>
        /// <param name="text">The characters to check.</param>
        /// <param name="start">The start of the range to check.</param>
        /// <param name="count">The length of the range to check.</param>
        /// <param name="index">The index to adjust.</param>
        /// <param name="codePointOffset">The number of code points by which to offset the index.</param>
        /// <returns>The adjusted index.</returns>
        /// <stable>ICU 3.0</stable>
        public static int OffsetByCodePoints(char[] text, int start, int count, int index,
            int codePointOffset)
        {
            // ICU4N specific - throw ArgumentNullException rather than falling back on NullReferenceException
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int limit = start + count;
            if (start < 0 || limit < start || limit > text.Length || index < start || index > limit)
            {
                throw new IndexOutOfRangeException("index ( " + index +
                        ") out of range " + start +
                        ", " + limit +
                        " in array 0, " + text.Length);
            }

            if (codePointOffset < 0)
            {
                while (++codePointOffset <= 0)
                {
                    char ch = text[--index];
                    if (index < start)
                    {
                        throw new IndexOutOfRangeException("index ( " + index +
                                ") < start (" + start +
                                ")");
                    }
                    while (ch >= MinLowSurrogate && ch <= MaxLowSurrogate && index > start)
                    {
                        ch = text[--index];
                        if (ch < MinHighSurrogate || ch > MaxHighSurrogate)
                        {
                            if (++codePointOffset > 0)
                            {
                                return index + 1;
                            }
                        }
                    }
                }
            }
            else
            {
                while (--codePointOffset >= 0)
                {
                    char ch = text[index++];
                    if (index > limit)
                    {
                        throw new IndexOutOfRangeException("index ( " + index +
                                ") > limit (" + limit +
                                ")");
                    }
                    while (ch >= MinHighSurrogate && ch <= MaxHighSurrogate && index < limit)
                    {
                        ch = text[index++];
                        if (ch < MinLowSurrogate || ch > MaxLowSurrogate)
                        {
                            if (--codePointOffset < 0)
                            {
                                return index - 1;
                            }
                        }
                    }
                }
            }

            return index;
        }

        // private variables -------------------------------------------------

        /// <summary>
        /// To get the last character out from a data type
        /// </summary>
        private const int LAST_CHAR_MASK_ = 0xFFFF;

        ///// <summary>
        ///// To get the last byte out from a data type
        ///// </summary>
        //private const int LAST_BYTE_MASK_ = 0xFF;

        ///// <summary>
        ///// Shift 16 bits
        ///// </summary>
        //private const int SHIFT_16_ = 16;

        ///// <summary>
        ///// Shift 24 bits
        ///// </summary>
        //private const int SHIFT_24_ = 24;

        ///// <summary>
        ///// Decimal radix
        ///// </summary>
        //private const int DECIMAL_RADIX_ = 10;

        /// <summary>
        /// No break space code point
        /// </summary>
        private const int NO_BREAK_SPACE_ = 0xA0;

        /// <summary>
        /// Figure space code point
        /// </summary>
        private const int FIGURE_SPACE_ = 0x2007;

        /// <summary>
        /// Narrow no break space code point
        /// </summary>
        private const int NARROW_NO_BREAK_SPACE_ = 0x202F;

        /// <summary>
        /// Ideographic number zero code point
        /// </summary>
        private const int IDEOGRAPHIC_NUMBER_ZERO_ = 0x3007;

        /// <summary>
        /// CJK Ideograph, First code point
        /// </summary>
        private const int CJK_IDEOGRAPH_FIRST_ = 0x4e00;

        /// <summary>
        /// CJK Ideograph, Second code point
        /// </summary>
        private const int CJK_IDEOGRAPH_SECOND_ = 0x4e8c;

        /// <summary>
        /// CJK Ideograph, Third code point
        /// </summary>
        private const int CJK_IDEOGRAPH_THIRD_ = 0x4e09;

        /// <summary>
        /// CJK Ideograph, Fourth code point
        /// </summary>
        private const int CJK_IDEOGRAPH_FOURTH_ = 0x56db;

        /// <summary>
        /// CJK Ideograph, FIFTH code point
        /// </summary>
        private const int CJK_IDEOGRAPH_FIFTH_ = 0x4e94;

        /// <summary>
        /// CJK Ideograph, Sixth code point
        /// </summary>
        private const int CJK_IDEOGRAPH_SIXTH_ = 0x516d;

        /// <summary>
        /// CJK Ideograph, Seventh code point
        /// </summary>
        private const int CJK_IDEOGRAPH_SEVENTH_ = 0x4e03;

        /// <summary>
        /// CJK Ideograph, Eighth code point
        /// </summary>
        private const int CJK_IDEOGRAPH_EIGHTH_ = 0x516b;

        /// <summary>
        /// CJK Ideograph, Nineth code point
        /// </summary>
        private const int CJK_IDEOGRAPH_NINETH_ = 0x4e5d;

        /// <summary>
        /// Application Program command code point
        /// </summary>
        private const int APPLICATION_PROGRAM_COMMAND_ = 0x009F;

        /// <summary>
        /// Unit separator code point
        /// </summary>
        private const int UNIT_SEPARATOR_ = 0x001F;

        /// <summary>
        /// Delete code point
        /// </summary>
        private const int DELETE_ = 0x007F;

        /**
         * Han digit characters
         */
        private const int CJK_IDEOGRAPH_COMPLEX_ZERO_ = 0x96f6;
        private const int CJK_IDEOGRAPH_COMPLEX_ONE_ = 0x58f9;
        private const int CJK_IDEOGRAPH_COMPLEX_TWO_ = 0x8cb3;
        private const int CJK_IDEOGRAPH_COMPLEX_THREE_ = 0x53c3;
        private const int CJK_IDEOGRAPH_COMPLEX_FOUR_ = 0x8086;
        private const int CJK_IDEOGRAPH_COMPLEX_FIVE_ = 0x4f0d;
        private const int CJK_IDEOGRAPH_COMPLEX_SIX_ = 0x9678;
        private const int CJK_IDEOGRAPH_COMPLEX_SEVEN_ = 0x67d2;
        private const int CJK_IDEOGRAPH_COMPLEX_EIGHT_ = 0x634c;
        private const int CJK_IDEOGRAPH_COMPLEX_NINE_ = 0x7396;
        private const int CJK_IDEOGRAPH_TEN_ = 0x5341;
        private const int CJK_IDEOGRAPH_COMPLEX_TEN_ = 0x62fe;
        private const int CJK_IDEOGRAPH_HUNDRED_ = 0x767e;
        private const int CJK_IDEOGRAPH_COMPLEX_HUNDRED_ = 0x4f70;
        private const int CJK_IDEOGRAPH_THOUSAND_ = 0x5343;
        private const int CJK_IDEOGRAPH_COMPLEX_THOUSAND_ = 0x4edf;
        private const int CJK_IDEOGRAPH_TEN_THOUSAND_ = 0x824c;
        private const int CJK_IDEOGRAPH_HUNDRED_MILLION_ = 0x5104;

        // private constructor -----------------------------------------------

        // ICU4N spcicific - Made class static, so we cannot have constructors
    }
}