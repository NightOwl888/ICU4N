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
    /// - punct:     ((1&lt;&lt;GetUnicodeCategory(c)) &amp; ((1&lt;&lt;ECharacterCategory.DashPunctuation)|(1&lt;&lt;CharacterCategory.StartPunctuation)|
    ///               (1&lt;&lt;CharacterCategory.EndPunctuation)|(1&lt;&lt;CharacterCategory.ConnectorPunctuation)|(1&lt;&lt;CharacterCategory.OtherPunctuation)|
    ///               (1&lt;&lt;CharacterCategory.InitialPunctuation)|(1&lt;&lt;CharacterCategory.FinalPunctuation)))!=0
    /// - digit:     IsDigit(c) or GetUnicodeCategory(c)==CharacterCategory.DecimalDigitNumber
    /// - xdigit:    HasBinaryProperty(c, UProperty.POSIX_XDigit)
    /// - alnum:     HasBinaryProperty(c, UProperty.POSIX_Alnum)
    /// - space:     IsUWhiteSpace(c) or HasBinaryProperty(c, UProperty.White_Space)
    /// - blank:     HasBinaryProperty(c, UProperty.POSIX_Blank)
    /// - cntrl:     GetUnicodeCategory(c)==CharacterCategory.Control
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
    ///     <item><term><see cref="IsWhiteSpace(int)"/></term><description>
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
            public const int Invalid_Code_ID = -1;
            /// <stable>ICU 2.4</stable>
            public const int Basic_Latin_ID = 1;
            /// <stable>ICU 2.4</stable>
            public const int Latin_1_Supplement_ID = 2;
            /// <stable>ICU 2.4</stable>
            public const int Latin_Extended_A_ID = 3;
            /// <stable>ICU 2.4</stable>
            public const int Latin_Extended_B_ID = 4;
            /// <stable>ICU 2.4</stable>
            public const int IPA_Extensions_ID = 5;
            /// <stable>ICU 2.4</stable>
            public const int Spacing_Modifier_Letters_ID = 6;
            /// <stable>ICU 2.4</stable>
            public const int Combining_Diacritical_Marks_ID = 7;
            /// <summary>
            /// Unicode 3.2 renames this block to "Greek and Coptic".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int Greek_ID = 8;
            /// <stable>ICU 2.4</stable>
            public const int Cyrillic_ID = 9;
            /// <stable>ICU 2.4</stable>
            public const int Armenian_ID = 10;
            /// <stable>ICU 2.4</stable>
            public const int Hebrew_ID = 11;
            /// <stable>ICU 2.4</stable>
            public const int Arabic_ID = 12;
            /// <stable>ICU 2.4</stable>
            public const int Syriac_ID = 13;
            /// <stable>ICU 2.4</stable>
            public const int Thaana_ID = 14;
            /// <stable>ICU 2.4</stable>
            public const int Devanagari_ID = 15;
            /// <stable>ICU 2.4</stable>
            public const int Bengali_ID = 16;
            /// <stable>ICU 2.4</stable>
            public const int Gurmukhi_ID = 17;
            /// <stable>ICU 2.4</stable>
            public const int Gujarati_ID = 18;
            /// <stable>ICU 2.4</stable>
            public const int Oriya_ID = 19;
            /// <stable>ICU 2.4</stable>
            public const int Tamil_ID = 20;
            /// <stable>ICU 2.4</stable>
            public const int Telugu_ID = 21;
            /// <stable>ICU 2.4</stable>
            public const int Kannada_ID = 22;
            /// <stable>ICU 2.4</stable>
            public const int Malayalam_ID = 23;
            /// <stable>ICU 2.4</stable>
            public const int Sinhala_ID = 24;
            /// <stable>ICU 2.4</stable>
            public const int Thai_ID = 25;
            /// <stable>ICU 2.4</stable>
            public const int Lao_ID = 26;
            /// <stable>ICU 2.4</stable>
            public const int Tibetan_ID = 27;
            /// <stable>ICU 2.4</stable>
            public const int Myanmar_ID = 28;
            /// <stable>ICU 2.4</stable>
            public const int Georgian_ID = 29;
            /// <stable>ICU 2.4</stable>
            public const int Hangul_Jamo_ID = 30;
            /// <stable>ICU 2.4</stable>
            public const int Ethiopic_ID = 31;
            /// <stable>ICU 2.4</stable>
            public const int Cherokee_ID = 32;
            /// <stable>ICU 2.4</stable>
            public const int Unified_Canadian_Aboriginal_Syllabics_ID = 33;
            /// <stable>ICU 2.4</stable>
            public const int Ogham_ID = 34;
            /// <stable>ICU 2.4</stable>
            public const int Runic_ID = 35;
            /// <stable>ICU 2.4</stable>
            public const int Khmer_ID = 36;
            /// <stable>ICU 2.4</stable>
            public const int Mongolian_ID = 37;
            /// <stable>ICU 2.4</stable>
            public const int Latin_Extended_Additional_ID = 38;
            /// <stable>ICU 2.4</stable>
            public const int Greek_Extended_ID = 39;
            /// <stable>ICU 2.4</stable>
            public const int General_Punctuation_ID = 40;
            /// <stable>ICU 2.4</stable>
            public const int Superscripts_And_Subscripts_ID = 41;
            /// <stable>ICU 2.4</stable>
            public const int Currency_Symbols_ID = 42;
            /// <summary>
            /// Unicode 3.2 renames this block to "Combining Diacritical Marks for
            /// Symbols".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int Combining_Marks_For_Symbols_ID = 43;
            /// <stable>ICU 2.4</stable>
            public const int Letterlike_Symbols_ID = 44;
            /// <stable>ICU 2.4</stable>
            public const int Number_Forms_ID = 45;
            /// <stable>ICU 2.4</stable>
            public const int Arrows_ID = 46;
            /// <stable>ICU 2.4</stable>
            public const int Mathematical_Operators_ID = 47;
            /// <stable>ICU 2.4</stable>
            public const int Miscellaneous_Technical_ID = 48;
            /// <stable>ICU 2.4</stable>
            public const int Control_Pictures_ID = 49;
            /// <stable>ICU 2.4</stable>
            public const int Optical_Character_Recognition_ID = 50;
            /// <stable>ICU 2.4</stable>
            public const int Enclosed_Alphanumerics_ID = 51;
            /// <stable>ICU 2.4</stable>
            public const int Box_Drawing_ID = 52;
            /// <stable>ICU 2.4</stable>
            public const int Block_Elements_ID = 53;
            /// <stable>ICU 2.4</stable>
            public const int Geometric_Shapes_ID = 54;
            /// <stable>ICU 2.4</stable>
            public const int Miscellaneous_Symbols_ID = 55;
            /// <stable>ICU 2.4</stable>
            public const int Dingbats_ID = 56;
            /// <stable>ICU 2.4</stable>
            public const int Braille_Patterns_ID = 57;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Radicals_Supplement_ID = 58;
            /// <stable>ICU 2.4</stable>
            public const int Kangxi_Radicals_ID = 59;
            /// <stable>ICU 2.4</stable>
            public const int Ideographic_Description_Characters_ID = 60;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Symbols_And_Punctuation_ID = 61;
            /// <stable>ICU 2.4</stable>
            public const int Hiragana_ID = 62;
            /// <stable>ICU 2.4</stable>
            public const int Katakana_ID = 63;
            /// <stable>ICU 2.4</stable>
            public const int Bopomofo_ID = 64;
            /// <stable>ICU 2.4</stable>
            public const int Hangul_Compatibility_Jamo_ID = 65;
            /// <stable>ICU 2.4</stable>
            public const int Kanbun_ID = 66;
            /// <stable>ICU 2.4</stable>
            public const int Bopomofo_Extended_ID = 67;
            /// <stable>ICU 2.4</stable>
            public const int Enclosed_CJK_Letters_And_Months_ID = 68;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Compatibility_ID = 69;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Unified_Ideographs_Extension_A_ID = 70;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Unified_Ideographs_ID = 71;
            /// <stable>ICU 2.4</stable>
            public const int Yi_Syllables_ID = 72;
            /// <stable>ICU 2.4</stable>
            public const int Yi_Radicals_ID = 73;
            /// <stable>ICU 2.4</stable>
            public const int Hangul_Syllables_ID = 74;
            /// <stable>ICU 2.4</stable>
            public const int High_Surrogates_ID = 75;
            /// <stable>ICU 2.4</stable>
            public const int High_Private_Use_Surrogates_ID = 76;
            /// <stable>ICU 2.4</stable>
            public const int Low_Surrogates_ID = 77;
            /// <summary>
            /// Same as <see cref="Private_Use"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int Private_Use_Area_ID = 78;
            /// <summary>
            /// Same as <see cref="Private_Use_Area"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int Private_Use_ID = Private_Use_Area_ID;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Compatibility_Ideographs_ID = 79;
            /// <stable>ICU 2.4</stable>
            public const int Alphabetic_Presentation_Forms_ID = 80;
            /// <stable>ICU 2.4</stable>
            public const int Arabic_Presentation_Forms_A_ID = 81;
            /// <stable>ICU 2.4</stable>
            public const int Combining_Half_Marks_ID = 82;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Compatibility_Forms_ID = 83;
            /// <stable>ICU 2.4</stable>
            public const int Small_Form_Variants_ID = 84;
            /// <stable>ICU 2.4</stable>
            public const int Arabic_Presentation_Forms_B_ID = 85;
            /// <stable>ICU 2.4</stable>
            public const int Specials_ID = 86;
            /// <stable>ICU 2.4</stable>
            public const int HalfWidth_And_FullWidth_Forms_ID = 87;
            /// <stable>ICU 2.4</stable>
            public const int Old_Italic_ID = 88;
            /// <stable>ICU 2.4</stable>
            public const int Gothic_ID = 89;
            /// <stable>ICU 2.4</stable>
            public const int Deseret_ID = 90;
            /// <stable>ICU 2.4</stable>
            public const int Byzantine_Musical_Symbols_ID = 91;
            /// <stable>ICU 2.4</stable>
            public const int Musical_Symbols_ID = 92;
            /// <stable>ICU 2.4</stable>
            public const int Mathematical_Alphanumeric_Symbols_ID = 93;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Unified_Ideographs_Extension_B_ID = 94;
            /// <stable>ICU 2.4</stable>
            public const int CJK_Compatibility_Ideographs_Supplement_ID = 95;
            /// <stable>ICU 2.4</stable>
            public const int Tags_ID = 96;

            // New blocks in Unicode 3.2

            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public const int Cyrillic_Supplementary_ID = 97;
            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 3.0</stable>
            public const int Cyrillic_Supplement_ID = 97;
            /// <stable>ICU 2.4</stable>
            public const int Tagalog_ID = 98;
            /// <stable>ICU 2.4</stable>
            public const int Hanunoo_ID = 99;
            /// <stable>ICU 2.4</stable>
            public const int Buhid_ID = 100;
            /// <stable>ICU 2.4</stable>
            public const int Tagbanwa_ID = 101;
            /// <stable>ICU 2.4</stable>
            public const int Miscellaneous_Mathematical_Symbols_A_ID = 102;
            /// <stable>ICU 2.4</stable>
            public const int Supplemental_Arrows_A_ID = 103;
            /// <stable>ICU 2.4</stable>
            public const int Supplemental_Arrows_B_ID = 104;
            /// <stable>ICU 2.4</stable>
            public const int Miscellaneous_Mathematical_Symbols_B_ID = 105;
            /// <stable>ICU 2.4</stable>
            public const int Supplemental_Mathematical_Operators_ID = 106;
            /// <stable>ICU 2.4</stable>
            public const int Katakana_Phonetic_Extensions_ID = 107;
            /// <stable>ICU 2.4</stable>
            public const int Variation_Selectors_ID = 108;
            /// <stable>ICU 2.4</stable>
            public const int Supplementary_Private_Use_Area_A_ID = 109;
            /// <stable>ICU 2.4</stable>
            public const int Supplementary_Private_Use_Area_B_ID = 110;

            /// <stable>ICU 2.6</stable>
            public const int Limbu_ID = 111; /*[1900]*/
            /// <stable>ICU 2.6</stable>
            public const int Tai_Le_ID = 112; /*[1950]*/
            /// <stable>ICU 2.6</stable>
            public const int Khmer_Symbols_ID = 113; /*[19E0]*/
            /// <stable>ICU 2.6</stable>
            public const int Phonetic_Extensions_ID = 114; /*[1D00]*/
            /// <stable>ICU 2.6</stable>
            public const int Miscellaneous_Symbols_And_Arrows_ID = 115; /*[2B00]*/
            /// <stable>ICU 2.6</stable>
            public const int Yijing_Hexagram_Symbols_ID = 116; /*[4DC0]*/
            /// <stable>ICU 2.6</stable>
            public const int Linear_B_Syllabary_ID = 117; /*[10000]*/
            /// <stable>ICU 2.6</stable>
            public const int Linear_B_Ideograms_ID = 118; /*[10080]*/
            /// <stable>ICU 2.6</stable>
            public const int Aegean_Numbers_ID = 119; /*[10100]*/
            /// <stable>ICU 2.6</stable>
            public const int Ugaritic_ID = 120; /*[10380]*/
            /// <stable>ICU 2.6</stable>
            public const int Shavian_ID = 121; /*[10450]*/
            /// <stable>ICU 2.6</stable>
            public const int Osmanya_ID = 122; /*[10480]*/
            /// <stable>ICU 2.6</stable>
            public const int Cypriot_Syllabary_ID = 123; /*[10800]*/
            /// <stable>ICU 2.6</stable>
            public const int Tai_Xuan_Jing_Symbols_ID = 124; /*[1D300]*/
            /// <stable>ICU 2.6</stable>
            public const int Variation_Selectors_Supplement_ID = 125; /*[E0100]*/

            /* New blocks in Unicode 4.1 */

            /// <stable>ICU 3.4</stable>
            public const int Ancient_Greek_Musical_Notation_ID = 126; /*[1D200]*/

            /// <stable>ICU 3.4</stable>
            public const int Ancient_Greek_Numbers_ID = 127; /*[10140]*/

            /// <stable>ICU 3.4</stable>
            public const int Arabic_Supplement_ID = 128; /*[0750]*/

            /// <stable>ICU 3.4</stable>
            public const int Buginese_ID = 129; /*[1A00]*/

            /// <stable>ICU 3.4</stable>
            public const int CJK_Strokes_ID = 130; /*[31C0]*/

            /// <stable>ICU 3.4</stable>
            public const int Combining_Diacritical_Marks_Supplement_ID = 131; /*[1DC0]*/

            /// <stable>ICU 3.4</stable>
            public const int Coptic_ID = 132; /*[2C80]*/

            /// <stable>ICU 3.4</stable>
            public const int Ethiopic_Extended_ID = 133; /*[2D80]*/

            /// <stable>ICU 3.4</stable>
            public const int Ethiopic_Supplement_ID = 134; /*[1380]*/

            /// <stable>ICU 3.4</stable>
            public const int Georgian_Supplement_ID = 135; /*[2D00]*/

            /// <stable>ICU 3.4</stable>
            public const int Glagolitic_ID = 136; /*[2C00]*/

            /// <stable>ICU 3.4</stable>
            public const int Kharoshthi_ID = 137; /*[10A00]*/

            /// <stable>ICU 3.4</stable>
            public const int Modifier_Tone_Letters_ID = 138; /*[A700]*/

            /// <stable>ICU 3.4</stable>
            public const int New_Tai_Lue_ID = 139; /*[1980]*/

            /// <stable>ICU 3.4</stable>
            public const int Old_Persian_ID = 140; /*[103A0]*/

            /// <stable>ICU 3.4</stable>
            public const int Phonetic_Extensions_Supplement_ID = 141; /*[1D80]*/

            /// <stable>ICU 3.4</stable>
            public const int Supplemental_Punctuation_ID = 142; /*[2E00]*/

            /// <stable>ICU 3.4</stable>
            public const int Syloti_Nagri_ID = 143; /*[A800]*/

            /// <stable>ICU 3.4</stable>
            public const int Tifinagh_ID = 144; /*[2D30]*/

            /// <stable>ICU 3.4</stable>
            public const int Vertical_Forms_ID = 145; /*[FE10]*/

            /* New blocks in Unicode 5.0 */

            /// <stable>ICU 3.6</stable>
            public const int Nko_ID = 146; /*[07C0]*/
            /// <stable>ICU 3.6</stable>
            public const int Balinese_ID = 147; /*[1B00]*/
            /// <stable>ICU 3.6</stable>
            public const int Latin_Extended_C_ID = 148; /*[2C60]*/
            /// <stable>ICU 3.6</stable>
            public const int Latin_Extended_D_ID = 149; /*[A720]*/
            /// <stable>ICU 3.6</stable>
            public const int Phags_Pa_ID = 150; /*[A840]*/
            /// <stable>ICU 3.6</stable>
            public const int Phoenician_ID = 151; /*[10900]*/
            /// <stable>ICU 3.6</stable>
            public const int Cuneiform_ID = 152; /*[12000]*/
            /// <stable>ICU 3.6</stable>
            public const int Cuneiform_Numbers_And_Punctuation_ID = 153; /*[12400]*/
            /// <stable>ICU 3.6</stable>
            public const int Counting_Rod_Numerals_ID = 154; /*[1D360]*/

            /// <stable>ICU 4.0</stable>
            public const int Sundanese_ID = 155; /* [1B80] */

            /// <stable>ICU 4.0</stable>
            public const int Lepcha_ID = 156; /* [1C00] */

            /// <stable>ICU 4.0</stable>
            public const int Ol_Chiki_ID = 157; /* [1C50] */

            /// <stable>ICU 4.0</stable>
            public const int Cyrillic_Extended_A_ID = 158; /* [2DE0] */

            /// <stable>ICU 4.0</stable>
            public const int Vai_ID = 159; /* [A500] */

            /// <stable>ICU 4.0</stable>
            public const int Cyrillic_Extended_B_ID = 160; /* [A640] */

            /// <stable>ICU 4.0</stable>
            public const int Saurashtra_ID = 161; /* [A880] */

            /// <stable>ICU 4.0</stable>
            public const int Kayah_Li_ID = 162; /* [A900] */

            /// <stable>ICU 4.0</stable>
            public const int Rejang_ID = 163; /* [A930] */

            /// <stable>ICU 4.0</stable>
            public const int Cham_ID = 164; /* [AA00] */

            /// <stable>ICU 4.0</stable>
            public const int Ancient_Symbols_ID = 165; /* [10190] */

            /// <stable>ICU 4.0</stable>
            public const int Phaistos_Disc_ID = 166; /* [101D0] */

            /// <stable>ICU 4.0</stable>
            public const int Lycian_ID = 167; /* [10280] */

            /// <stable>ICU 4.0</stable>
            public const int Carian_ID = 168; /* [102A0] */

            /// <stable>ICU 4.0</stable>
            public const int Lydian_ID = 169; /* [10920] */

            /// <stable>ICU 4.0</stable>
            public const int Mahjong_Tiles_ID = 170; /* [1F000] */

            /// <stable>ICU 4.0</stable>
            public const int Domino_Tiles_ID = 171; /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /// <stable>ICU 4.4</stable>
            public const int Samaritan_ID = 172; /*[0800]*/
            /// <stable>ICU 4.4</stable>
            public const int Unified_Canadian_Aboriginal_Syllabics_Extended_ID = 173; /*[18B0]*/
            /// <stable>ICU 4.4</stable>
            public const int Tai_Tham_ID = 174; /*[1A20]*/
            /// <stable>ICU 4.4</stable>
            public const int Vedic_Extensions_ID = 175; /*[1CD0]*/
            /// <stable>ICU 4.4</stable>
            public const int Lisu_ID = 176; /*[A4D0]*/
            /// <stable>ICU 4.4</stable>
            public const int Bamum_ID = 177; /*[A6A0]*/
            /// <stable>ICU 4.4</stable>
            public const int Common_Indic_Number_Forms_ID = 178; /*[A830]*/
            /// <stable>ICU 4.4</stable>
            public const int Devanagari_Extended_ID = 179; /*[A8E0]*/
            /// <stable>ICU 4.4</stable>
            public const int Hangul_Jamo_Extended_A_ID = 180; /*[A960]*/
            /// <stable>ICU 4.4</stable>
            public const int Javanese_ID = 181; /*[A980]*/
            /// <stable>ICU 4.4</stable>
            public const int Myanmar_Extended_A_ID = 182; /*[AA60]*/
            /// <stable>ICU 4.4</stable>
            public const int Tai_Viet_ID = 183; /*[AA80]*/
            /// <stable>ICU 4.4</stable>
            public const int Meetei_Mayek_ID = 184; /*[ABC0]*/
            /// <stable>ICU 4.4</stable>
            public const int Hangul_Jamo_Extended_B_ID = 185; /*[D7B0]*/
            /// <stable>ICU 4.4</stable>
            public const int Imperial_Aramaic_ID = 186; /*[10840]*/
            /// <stable>ICU 4.4</stable>
            public const int Old_South_Arabian_ID = 187; /*[10A60]*/
            /// <stable>ICU 4.4</stable>
            public const int Avestan_ID = 188; /*[10B00]*/
            /// <stable>ICU 4.4</stable>
            public const int Inscriptional_Parthian_ID = 189; /*[10B40]*/
            /// <stable>ICU 4.4</stable>
            public const int Inscriptional_Pahlavi_ID = 190; /*[10B60]*/
            /// <stable>ICU 4.4</stable>
            public const int Old_Turkic_ID = 191; /*[10C00]*/
            /// <stable>ICU 4.4</stable>
            public const int Rumi_Numeral_Symbols_ID = 192; /*[10E60]*/
            /// <stable>ICU 4.4</stable>
            public const int Kaithi_ID = 193; /*[11080]*/
            /// <stable>ICU 4.4</stable>
            public const int Egyptian_Hieroglyphs_ID = 194; /*[13000]*/
            /// <stable>ICU 4.4</stable>
            public const int Enclosed_Alphanumeric_Supplement_ID = 195; /*[1F100]*/
            /// <stable>ICU 4.4</stable>
            public const int Enclosed_Ideographic_Supplement_ID = 196; /*[1F200]*/
            /// <stable>ICU 4.4</stable>
            public const int CJK_Unified_Ideographs_Extension_C_ID = 197; /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /// <stable>ICU 4.6</stable>
            public const int Mandaic_ID = 198; /*[0840]*/
            /// <stable>ICU 4.6</stable>
            public const int Batak_ID = 199; /*[1BC0]*/
            /// <stable>ICU 4.6</stable>
            public const int Ethiopic_Extended_A_ID = 200; /*[AB00]*/
            /// <stable>ICU 4.6</stable>
            public const int Brahmi_ID = 201; /*[11000]*/
            /// <stable>ICU 4.6</stable>
            public const int Bamum_Supplement_ID = 202; /*[16800]*/
            /// <stable>ICU 4.6</stable>
            public const int Kana_Supplement_ID = 203; /*[1B000]*/
            /// <stable>ICU 4.6</stable>
            public const int Playing_Cards_ID = 204; /*[1F0A0]*/
            /// <stable>ICU 4.6</stable>
            public const int Miscellaneous_Symbols_And_Pictographs_ID = 205; /*[1F300]*/
            /// <stable>ICU 4.6</stable>
            public const int Emoticons_ID = 206; /*[1F600]*/
            /// <stable>ICU 4.6</stable>
            public const int Transport_And_Map_Symbols_ID = 207; /*[1F680]*/
            /// <stable>ICU 4.6</stable>
            public const int Alchemical_Symbols_ID = 208; /*[1F700]*/
            /// <stable>ICU 4.6</stable>
            public const int CJK_Unified_Ideographs_Extension_D_ID = 209; /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /// <stable>ICU 49</stable>
            public const int Arabic_Extended_A_ID = 210; /*[08A0]*/
            /// <stable>ICU 49</stable>
            public const int Arabic_Mathematical_Alphabetic_Symbols_ID = 211; /*[1EE00]*/
            /// <stable>ICU 49</stable>
            public const int Chakma_ID = 212; /*[11100]*/
            /// <stable>ICU 49</stable>
            public const int Meetei_Mayek_Extensions_ID = 213; /*[AAE0]*/
            /// <stable>ICU 49</stable>
            public const int Meroitic_Cursive_ID = 214; /*[109A0]*/
            /// <stable>ICU 49</stable>
            public const int Meroitic_Hieroglyphs_ID = 215; /*[10980]*/
            /// <stable>ICU 49</stable>
            public const int Miao_ID = 216; /*[16F00]*/
            /// <stable>ICU 49</stable>
            public const int Sharada_ID = 217; /*[11180]*/
            /// <stable>ICU 49</stable>
            public const int Sora_Sompeng_ID = 218; /*[110D0]*/
            /// <stable>ICU 49</stable>
            public const int Sundanese_Supplement_ID = 219; /*[1CC0]*/
            /// <stable>ICU 49</stable>
            public const int Takri_ID = 220; /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /// <stable>ICU 54</stable>
            public const int Bassa_Vah_ID = 221; /*[16AD0]*/
            /// <stable>ICU 54</stable>
            public const int Caucasian_Albanian_ID = 222; /*[10530]*/
            /// <stable>ICU 54</stable>
            public const int Coptic_Epact_Numbers_ID = 223; /*[102E0]*/
            /// <stable>ICU 54</stable>
            public const int Combining_Diacritical_Marks_Extended_ID = 224; /*[1AB0]*/
            /// <stable>ICU 54</stable>
            public const int Duployan_ID = 225; /*[1BC00]*/
            /// <stable>ICU 54</stable>
            public const int Elbasan_ID = 226; /*[10500]*/
            /// <stable>ICU 54</stable>
            public const int Geometric_Shapes_Extended_ID = 227; /*[1F780]*/
            /// <stable>ICU 54</stable>
            public const int Grantha_ID = 228; /*[11300]*/
            /// <stable>ICU 54</stable>
            public const int Khojki_ID = 229; /*[11200]*/
            /// <stable>ICU 54</stable>
            public const int Khudawadi_ID = 230; /*[112B0]*/
            /// <stable>ICU 54</stable>
            public const int Latin_Extended_E_ID = 231; /*[AB30]*/
            /// <stable>ICU 54</stable>
            public const int Linear_A_ID = 232; /*[10600]*/
            /// <stable>ICU 54</stable>
            public const int Mahajani_ID = 233; /*[11150]*/
            /// <stable>ICU 54</stable>
            public const int Manichaean_ID = 234; /*[10AC0]*/
            /// <stable>ICU 54</stable>
            public const int Mende_Kikakui_ID = 235; /*[1E800]*/
            /// <stable>ICU 54</stable>
            public const int Modi_ID = 236; /*[11600]*/
            /// <stable>ICU 54</stable>
            public const int Mro_ID = 237; /*[16A40]*/
            /// <stable>ICU 54</stable>
            public const int Myanmar_Extended_B_ID = 238; /*[A9E0]*/
            /// <stable>ICU 54</stable>
            public const int Nabataean_ID = 239; /*[10880]*/
            /// <stable>ICU 54</stable>
            public const int Old_North_Arabian_ID = 240; /*[10A80]*/
            /// <stable>ICU 54</stable>
            public const int Old_Permic_ID = 241; /*[10350]*/
            /// <stable>ICU 54</stable>
            public const int Ornamental_Dingbats_ID = 242; /*[1F650]*/
            /// <stable>ICU 54</stable>
            public const int Pahawh_Hmong_ID = 243; /*[16B00]*/
            /// <stable>ICU 54</stable>
            public const int Palmyrene_ID = 244; /*[10860]*/
            /// <stable>ICU 54</stable>
            public const int Pau_Cin_Hau_ID = 245; /*[11AC0]*/
            /// <stable>ICU 54</stable>
            public const int Psalter_Pahlavi_ID = 246; /*[10B80]*/
            /// <stable>ICU 54</stable>
            public const int Shorthand_Format_Controls_ID = 247; /*[1BCA0]*/
            /// <stable>ICU 54</stable>
            public const int Siddham_ID = 248; /*[11580]*/
            /// <stable>ICU 54</stable>
            public const int Sinhala_Archaic_Numbers_ID = 249; /*[111E0]*/
            /// <stable>ICU 54</stable>
            public const int Supplemental_Arrows_C_ID = 250; /*[1F800]*/
            /// <stable>ICU 54</stable>
            public const int Tirhuta_ID = 251; /*[11480]*/
            /// <stable>ICU 54</stable>
            public const int Warang_Citi_ID = 252; /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /// <stable>ICU 56</stable>
            public const int Ahom_ID = 253; /*[11700]*/
            /// <stable>ICU 56</stable>
            public const int Anatolian_Hieroglyphs_ID = 254; /*[14400]*/
            /// <stable>ICU 56</stable>
            public const int Cherokee_Supplement_ID = 255; /*[AB70]*/
            /// <stable>ICU 56</stable>
            public const int CJK_Unified_Ideographs_Extension_E_ID = 256; /*[2B820]*/
            /// <stable>ICU 56</stable>
            public const int Early_Dynastic_Cuneiform_ID = 257; /*[12480]*/
            /// <stable>ICU 56</stable>
            public const int Hatran_ID = 258; /*[108E0]*/
            /// <stable>ICU 56</stable>
            public const int Multani_ID = 259; /*[11280]*/
            /// <stable>ICU 56</stable>
            public const int Old_Hungarian_ID = 260; /*[10C80]*/
            /// <stable>ICU 56</stable>
            public const int Supplemental_Symbols_And_Pictographs_ID = 261; /*[1F900]*/
            /// <stable>ICU 56</stable>
            public const int Sutton_Signwriting_ID = 262; /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /// <stable>ICU 58</stable>
            public const int Adlam_ID = 263; /*[1E900]*/
            /// <stable>ICU 58</stable>
            public const int Bhaiksuki_ID = 264; /*[11C00]*/
            /// <stable>ICU 58</stable>
            public const int Cyrillic_Extended_C_ID = 265; /*[1C80]*/
            /// <stable>ICU 58</stable>
            public const int Glagolitic_Supplement_ID = 266; /*[1E000]*/
            /// <stable>ICU 58</stable>
            public const int Ideographic_Symbols_And_Punctuation_ID = 267; /*[16FE0]*/
            /// <stable>ICU 58</stable>
            public const int Marchen_ID = 268; /*[11C70]*/
            /// <stable>ICU 58</stable>
            public const int Mongolian_Supplement_ID = 269; /*[11660]*/
            /// <stable>ICU 58</stable>
            public const int Newa_ID = 270; /*[11400]*/
            /// <stable>ICU 58</stable>
            public const int Osage_ID = 271; /*[104B0]*/
            /// <stable>ICU 58</stable>
            public const int Tangut_ID = 272; /*[17000]*/
            /// <stable>ICU 58</stable>
            public const int Tangut_Components_ID = 273; /*[18800]*/

            // New blocks in Unicode 10.0

            /// <stable>ICU 60</stable>
            public const int CJK_Unified_Ideographs_Extension_F_ID = 274; /*[2CEB0]*/
            /// <stable>ICU 60</stable>
            public const int Kana_Extended_A_ID = 275; /*[1B100]*/
            /// <stable>ICU 60</stable>
            public const int Masaram_Gondi_ID = 276; /*[11D00]*/
            /// <stable>ICU 60</stable>
            public const int Nushu_ID = 277; /*[1B170]*/
            /// <stable>ICU 60</stable>
            public const int Soyombo_ID = 278; /*[11A50]*/
            /// <stable>ICU 60</stable>
            public const int Syriac_Supplement_ID = 279; /*[0860]*/
            /// <stable>ICU 60</stable>
            public const int Zanabazar_Square_ID = 280; /*[11A00]*/

            /// <summary>
            /// One more than the highest normal UnicodeBlock value.
            /// The highest value is available via <see cref="UChar.GetIntPropertyValue(int, UProperty)"/> 
            /// with parameter <see cref="UProperty.Block"/>.
            /// </summary>
            [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
            public const int Count = 281;

            // blocks objects ---------------------------------------------------

            /// <summary>
            /// Array of <see cref="UnicodeBlock"/>s, for easy access in <see cref="GetInstance(int)"/>
            /// </summary>
#pragma warning disable 612, 618
            private readonly static UnicodeBlock[] BLOCKS_ = new UnicodeBlock[Count];
#pragma warning restore 612, 618

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock NoBlock
                = new UnicodeBlock(nameof(NoBlock), 0);

            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Basic_Latin
                = new UnicodeBlock(nameof(Basic_Latin), Basic_Latin_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Latin_1_Supplement
                = new UnicodeBlock(nameof(Latin_1_Supplement), Latin_1_Supplement_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Latin_Extended_A
                = new UnicodeBlock(nameof(Latin_Extended_A), Latin_Extended_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Latin_Extended_B
                = new UnicodeBlock(nameof(Latin_Extended_B), Latin_Extended_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock IPA_Extensions
                = new UnicodeBlock(nameof(IPA_Extensions), IPA_Extensions_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Spacing_Modifier_Letters
                = new UnicodeBlock(nameof(Spacing_Modifier_Letters), Spacing_Modifier_Letters_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Combining_Diacritical_Marks
                = new UnicodeBlock(nameof(Combining_Diacritical_Marks), Combining_Diacritical_Marks_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Greek
                = new UnicodeBlock(nameof(Greek), Greek_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Cyrillic
                = new UnicodeBlock(nameof(Cyrillic), Cyrillic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Armenian
                = new UnicodeBlock(nameof(Armenian), Armenian_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hebrew
                = new UnicodeBlock(nameof(Hebrew), Hebrew_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Arabic
                = new UnicodeBlock(nameof(Arabic), Arabic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Syriac
                = new UnicodeBlock(nameof(Syriac), Syriac_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Thaana
                = new UnicodeBlock(nameof(Thaana), Thaana_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Devanagari
                = new UnicodeBlock(nameof(Devanagari), Devanagari_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Bengali
                = new UnicodeBlock(nameof(Bengali), Bengali_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Gurmukhi
                = new UnicodeBlock(nameof(Gurmukhi), Gurmukhi_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Gujarati
                = new UnicodeBlock(nameof(Gujarati), Gujarati_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Oriya
                = new UnicodeBlock(nameof(Oriya), Oriya_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Tamil
                = new UnicodeBlock(nameof(Tamil), Tamil_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Telugu
                = new UnicodeBlock(nameof(Telugu), Telugu_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Kannada
                = new UnicodeBlock(nameof(Kannada), Kannada_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Malayalam
                = new UnicodeBlock(nameof(Malayalam), Malayalam_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Sinhala
                = new UnicodeBlock(nameof(Sinhala), Sinhala_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Thai
                = new UnicodeBlock(nameof(Thai), Thai_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Lao
                = new UnicodeBlock(nameof(Lao), Lao_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Tibetan
                = new UnicodeBlock(nameof(Tibetan), Tibetan_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Myanmar
                = new UnicodeBlock(nameof(Myanmar), Myanmar_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Georgian
                = new UnicodeBlock(nameof(Georgian), Georgian_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hangul_Jamo
                = new UnicodeBlock(nameof(Hangul_Jamo), Hangul_Jamo_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Ethiopic
                = new UnicodeBlock(nameof(Ethiopic), Ethiopic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Cherokee
                = new UnicodeBlock(nameof(Cherokee), Cherokee_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Unified_Canadian_Aboriginal_Syllabics
                = new UnicodeBlock(nameof(Unified_Canadian_Aboriginal_Syllabics),
                    Unified_Canadian_Aboriginal_Syllabics_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Ogham
                = new UnicodeBlock(nameof(Ogham), Ogham_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Runic
                = new UnicodeBlock(nameof(Runic), Runic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Khmer
                = new UnicodeBlock(nameof(Khmer), Khmer_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Mongolian
                = new UnicodeBlock(nameof(Mongolian), Mongolian_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Latin_Extended_Additional
                = new UnicodeBlock(nameof(Latin_Extended_Additional), Latin_Extended_Additional_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Greek_Extended
                = new UnicodeBlock(nameof(Greek_Extended), Greek_Extended_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock General_Punctuation
                = new UnicodeBlock(nameof(General_Punctuation), General_Punctuation_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Superscripts_And_Subscripts
                = new UnicodeBlock(nameof(Superscripts_And_Subscripts), Superscripts_And_Subscripts_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Currency_Symbols
                = new UnicodeBlock(nameof(Currency_Symbols), Currency_Symbols_ID);
            /// <summary>
            /// Unicode 3.2 renames this block to "Combining Diacritical Marks for
            /// Symbols".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Combining_Marks_For_Symbols
                = new UnicodeBlock(nameof(Combining_Marks_For_Symbols), Combining_Marks_For_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Letterlike_Symbols
                = new UnicodeBlock(nameof(Letterlike_Symbols), Letterlike_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Number_Forms
                = new UnicodeBlock(nameof(Number_Forms), Number_Forms_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Arrows
                = new UnicodeBlock(nameof(Arrows), Arrows_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Mathematical_Operators
                = new UnicodeBlock(nameof(Mathematical_Operators), Mathematical_Operators_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Miscellaneous_Technical
                = new UnicodeBlock(nameof(Miscellaneous_Technical), Miscellaneous_Technical_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Control_Pictures
                = new UnicodeBlock(nameof(Control_Pictures), Control_Pictures_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Optical_Character_Recognition
                = new UnicodeBlock(nameof(Optical_Character_Recognition), Optical_Character_Recognition_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Enclosed_Alphanumerics
                = new UnicodeBlock(nameof(Enclosed_Alphanumerics), Enclosed_Alphanumerics_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Box_Drawing
                = new UnicodeBlock(nameof(Box_Drawing), Box_Drawing_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Block_Elements
                = new UnicodeBlock(nameof(Block_Elements), Block_Elements_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Geometric_Shapes
                = new UnicodeBlock(nameof(Geometric_Shapes), Geometric_Shapes_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Miscellaneous_Symbols
                = new UnicodeBlock(nameof(Miscellaneous_Symbols), Miscellaneous_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Dingbats
                = new UnicodeBlock(nameof(Dingbats), Dingbats_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Braille_Patterns
                = new UnicodeBlock(nameof(Braille_Patterns), Braille_Patterns_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Radicals_Supplement
                = new UnicodeBlock(nameof(CJK_Radicals_Supplement), CJK_Radicals_Supplement_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Kangxi_Radicals
                = new UnicodeBlock(nameof(Kangxi_Radicals), Kangxi_Radicals_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Ideographic_Description_Characters
                = new UnicodeBlock(nameof(Ideographic_Description_Characters),
                    Ideographic_Description_Characters_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Symbols_And_Punctuation
                = new UnicodeBlock(nameof(CJK_Symbols_And_Punctuation), CJK_Symbols_And_Punctuation_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hiragana
                = new UnicodeBlock(nameof(Hiragana), Hiragana_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Katakana
                = new UnicodeBlock(nameof(Katakana), Katakana_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Bopomofo
                = new UnicodeBlock(nameof(Bopomofo), Bopomofo_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hangul_Compatibility_Jamo
                = new UnicodeBlock(nameof(Hangul_Compatibility_Jamo), Hangul_Compatibility_Jamo_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Kanbun
                = new UnicodeBlock(nameof(Kanbun), Kanbun_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Bopomofo_Extended
                = new UnicodeBlock(nameof(Bopomofo_Extended), Bopomofo_Extended_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Enclosed_CJK_Letters_And_Months
                = new UnicodeBlock(nameof(Enclosed_CJK_Letters_And_Months),
                    Enclosed_CJK_Letters_And_Months_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Compatibility
                = new UnicodeBlock(nameof(CJK_Compatibility), CJK_Compatibility_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_A
                = new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_A),
                    CJK_Unified_Ideographs_Extension_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs
                = new UnicodeBlock(nameof(CJK_Unified_Ideographs), CJK_Unified_Ideographs_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Yi_Syllables
                = new UnicodeBlock(nameof(Yi_Syllables), Yi_Syllables_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Yi_Radicals
                = new UnicodeBlock(nameof(Yi_Radicals), Yi_Radicals_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hangul_Syllables
                = new UnicodeBlock(nameof(Hangul_Syllables), Hangul_Syllables_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock High_Surrogates
                = new UnicodeBlock(nameof(High_Surrogates), High_Surrogates_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock High_Private_Use_Surrogates
                = new UnicodeBlock(nameof(High_Private_Use_Surrogates), High_Private_Use_Surrogates_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Low_Surrogates
                = new UnicodeBlock(nameof(Low_Surrogates), Low_Surrogates_ID);
            /// <summary>
            /// Same as <see cref="Private_Use"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Private_Use_Area
                = new UnicodeBlock(nameof(Private_Use_Area), 78);
            /// <summary>
            /// Same as <see cref="Private_Use_Area"/>.
            /// Until Unicode 3.1.1; the corresponding block name was "Private Use";
            /// and multiple code point ranges had this block.
            /// Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
            /// and adds separate blocks for the supplementary PUAs.
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Private_Use
                = Private_Use_Area;
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Compatibility_Ideographs
                = new UnicodeBlock(nameof(CJK_Compatibility_Ideographs), CJK_Compatibility_Ideographs_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Alphabetic_Presentation_Forms
                = new UnicodeBlock(nameof(Alphabetic_Presentation_Forms), Alphabetic_Presentation_Forms_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Arabic_Presentation_Forms_A
                = new UnicodeBlock(nameof(Arabic_Presentation_Forms_A), Arabic_Presentation_Forms_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Combining_Half_Marks
                = new UnicodeBlock(nameof(Combining_Half_Marks), Combining_Half_Marks_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Compatibility_Forms
                = new UnicodeBlock(nameof(CJK_Compatibility_Forms), CJK_Compatibility_Forms_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Small_Form_Variants
                = new UnicodeBlock(nameof(Small_Form_Variants), Small_Form_Variants_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Arabic_Presentation_Forms_B
                = new UnicodeBlock(nameof(Arabic_Presentation_Forms_B), Arabic_Presentation_Forms_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Specials
                = new UnicodeBlock(nameof(Specials), Specials_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock HalfWidth_And_FullWidth_Forms
                = new UnicodeBlock(nameof(HalfWidth_And_FullWidth_Forms), HalfWidth_And_FullWidth_Forms_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Old_Italic
                = new UnicodeBlock(nameof(Old_Italic), Old_Italic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Gothic
                = new UnicodeBlock(nameof(Gothic), Gothic_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Deseret
                = new UnicodeBlock(nameof(Deseret), Deseret_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Byzantine_Musical_Symbols
                = new UnicodeBlock(nameof(Byzantine_Musical_Symbols), Byzantine_Musical_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Musical_Symbols
                = new UnicodeBlock(nameof(Musical_Symbols), Musical_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Mathematical_Alphanumeric_Symbols
                = new UnicodeBlock(nameof(Mathematical_Alphanumeric_Symbols),
                    Mathematical_Alphanumeric_Symbols_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_B
                = new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_B),
                    CJK_Unified_Ideographs_Extension_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock CJK_Compatibility_Ideographs_Supplement
                = new UnicodeBlock(nameof(CJK_Compatibility_Ideographs_Supplement),
                    CJK_Compatibility_Ideographs_Supplement_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Tags
                = new UnicodeBlock(nameof(Tags), Tags_ID);

            // New blocks in Unicode 3.2

            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Cyrillic_Supplementary
                = new UnicodeBlock(nameof(Cyrillic_Supplementary), Cyrillic_Supplementary_ID);
            /// <summary>
            /// Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
            /// </summary>
            /// <stable>ICU 3.0</stable>
            public static readonly UnicodeBlock Cyrillic_Supplement
                = new UnicodeBlock(nameof(Cyrillic_Supplement), Cyrillic_Supplement_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Tagalog
                = new UnicodeBlock(nameof(Tagalog), Tagalog_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Hanunoo
                = new UnicodeBlock(nameof(Hanunoo), Hanunoo_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Buhid
                = new UnicodeBlock(nameof(Buhid), Buhid_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Tagbanwa
                = new UnicodeBlock(nameof(Tagbanwa), Tagbanwa_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Miscellaneous_Mathematical_Symbols_A
                = new UnicodeBlock(nameof(Miscellaneous_Mathematical_Symbols_A),
                    Miscellaneous_Mathematical_Symbols_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Supplemental_Arrows_A
                = new UnicodeBlock(nameof(Supplemental_Arrows_A), Supplemental_Arrows_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Supplemental_Arrows_B
                = new UnicodeBlock(nameof(Supplemental_Arrows_B), Supplemental_Arrows_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Miscellaneous_Mathematical_Symbols_B
                = new UnicodeBlock(nameof(Miscellaneous_Mathematical_Symbols_B),
                    Miscellaneous_Mathematical_Symbols_B_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Supplemental_Mathematical_Operators
                = new UnicodeBlock(nameof(Supplemental_Mathematical_Operators),
                    Supplemental_Mathematical_Operators_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Katakana_Phonetic_Extensions
                = new UnicodeBlock(nameof(Katakana_Phonetic_Extensions), Katakana_Phonetic_Extensions_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Variation_Selectors
                = new UnicodeBlock(nameof(Variation_Selectors), Variation_Selectors_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Supplementary_Private_Use_Area_A
                = new UnicodeBlock(nameof(Supplementary_Private_Use_Area_A),
                    Supplementary_Private_Use_Area_A_ID);
            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Supplementary_Private_Use_Area_B
                = new UnicodeBlock(nameof(Supplementary_Private_Use_Area_B),
                    Supplementary_Private_Use_Area_B_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Limbu
                = new UnicodeBlock(nameof(Limbu), Limbu_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Tai_Le
                = new UnicodeBlock(nameof(Tai_Le), Tai_Le_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Khmer_Symbols
                = new UnicodeBlock(nameof(Khmer_Symbols), Khmer_Symbols_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Phonetic_Extensions
                = new UnicodeBlock(nameof(Phonetic_Extensions), Phonetic_Extensions_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Miscellaneous_Symbols_And_Arrows
                = new UnicodeBlock(nameof(Miscellaneous_Symbols_And_Arrows),
                    Miscellaneous_Symbols_And_Arrows_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Yijing_Hexagram_Symbols
                = new UnicodeBlock(nameof(Yijing_Hexagram_Symbols), Yijing_Hexagram_Symbols_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Linear_B_Syllabary
                = new UnicodeBlock(nameof(Linear_B_Syllabary), Linear_B_Syllabary_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Linear_B_Ideograms
                = new UnicodeBlock(nameof(Linear_B_Ideograms), Linear_B_Ideograms_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Aegean_Numbers
                = new UnicodeBlock(nameof(Aegean_Numbers), Aegean_Numbers_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Ugaritic
                = new UnicodeBlock(nameof(Ugaritic), Ugaritic_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Shavian
                = new UnicodeBlock(nameof(Shavian), Shavian_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Osmanya
                = new UnicodeBlock(nameof(Osmanya), Osmanya_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Cypriot_Syllabary
                = new UnicodeBlock(nameof(Cypriot_Syllabary), Cypriot_Syllabary_ID);
            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Tai_Xuan_Jing_Symbols
                = new UnicodeBlock(nameof(Tai_Xuan_Jing_Symbols), Tai_Xuan_Jing_Symbols_ID);

            /// <stable>ICU 2.6</stable>
            public static readonly UnicodeBlock Variation_Selectors_Supplement
                = new UnicodeBlock(nameof(Variation_Selectors_Supplement), Variation_Selectors_Supplement_ID);

            /* New blocks in Unicode 4.1 */

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Ancient_Greek_Musical_Notation =
                    new UnicodeBlock(nameof(Ancient_Greek_Musical_Notation),
                            Ancient_Greek_Musical_Notation_ID); /*[1D200]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Ancient_Greek_Numbers =
                    new UnicodeBlock(nameof(Ancient_Greek_Numbers), Ancient_Greek_Numbers_ID); /*[10140]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Arabic_Supplement =
                    new UnicodeBlock(nameof(Arabic_Supplement), Arabic_Supplement_ID); /*[0750]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Buginese =
                    new UnicodeBlock(nameof(Buginese), Buginese_ID); /*[1A00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock CJK_Strokes =
                    new UnicodeBlock(nameof(CJK_Strokes), CJK_Strokes_ID); /*[31C0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Combining_Diacritical_Marks_Supplement =
                    new UnicodeBlock(nameof(Combining_Diacritical_Marks_Supplement),
                            Combining_Diacritical_Marks_Supplement_ID); /*[1DC0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Coptic = new UnicodeBlock(nameof(Coptic), Coptic_ID); /*[2C80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Ethiopic_Extended =
                    new UnicodeBlock(nameof(Ethiopic_Extended), Ethiopic_Extended_ID); /*[2D80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Ethiopic_Supplement =
                    new UnicodeBlock(nameof(Ethiopic_Supplement), Ethiopic_Supplement_ID); /*[1380]*/

            /// <stable>ICU 3.4</stable>
            public 
                static readonly UnicodeBlock Georgian_Supplement =
                    new UnicodeBlock(nameof(Georgian_Supplement), Georgian_Supplement_ID); /*[2D00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Glagolitic =
                    new UnicodeBlock(nameof(Glagolitic), Glagolitic_ID); /*[2C00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Kharoshthi =
                    new UnicodeBlock(nameof(Kharoshthi), Kharoshthi_ID); /*[10A00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Modifier_Tone_Letters =
                    new UnicodeBlock(nameof(Modifier_Tone_Letters), Modifier_Tone_Letters_ID); /*[A700]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock New_Tai_Lue =
                    new UnicodeBlock(nameof(New_Tai_Lue), New_Tai_Lue_ID); /*[1980]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Old_Persian =
                    new UnicodeBlock(nameof(Old_Persian), Old_Persian_ID); /*[103A0]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Phonetic_Extensions_Supplement =
                    new UnicodeBlock(nameof(Phonetic_Extensions_Supplement),
                            Phonetic_Extensions_Supplement_ID); /*[1D80]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Supplemental_Punctuation =
                    new UnicodeBlock(nameof(Supplemental_Punctuation), Supplemental_Punctuation_ID); /*[2E00]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Syloti_Nagri =
                    new UnicodeBlock(nameof(Syloti_Nagri), Syloti_Nagri_ID); /*[A800]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Tifinagh =
                    new UnicodeBlock(nameof(Tifinagh), Tifinagh_ID); /*[2D30]*/

            /// <stable>ICU 3.4</stable>
            public static readonly UnicodeBlock Vertical_Forms =
                    new UnicodeBlock(nameof(Vertical_Forms), Vertical_Forms_ID); /*[FE10]*/

            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Nko = new UnicodeBlock(nameof(Nko), Nko_ID); /*[07C0]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Balinese =
                    new UnicodeBlock(nameof(Balinese), Balinese_ID); /*[1B00]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Latin_Extended_C =
                    new UnicodeBlock(nameof(Latin_Extended_C), Latin_Extended_C_ID); /*[2C60]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Latin_Extended_D =
                    new UnicodeBlock(nameof(Latin_Extended_D), Latin_Extended_D_ID); /*[A720]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Phags_Pa =
                    new UnicodeBlock(nameof(Phags_Pa), Phags_Pa_ID); /*[A840]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Phoenician =
                    new UnicodeBlock(nameof(Phoenician), Phoenician_ID); /*[10900]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Cuneiform =
                    new UnicodeBlock(nameof(Cuneiform), Cuneiform_ID); /*[12000]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Cuneiform_Numbers_And_Punctuation =
                    new UnicodeBlock(nameof(Cuneiform_Numbers_And_Punctuation),
                            Cuneiform_Numbers_And_Punctuation_ID); /*[12400]*/
            /// <stable>ICU 3.6</stable>
            public static readonly UnicodeBlock Counting_Rod_Numerals =
                    new UnicodeBlock(nameof(Counting_Rod_Numerals), Counting_Rod_Numerals_ID); /*[1D360]*/

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Sundanese =
                    new UnicodeBlock(nameof(Sundanese), Sundanese_ID); /* [1B80] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Lepcha =
                    new UnicodeBlock(nameof(Lepcha), Lepcha_ID); /* [1C00] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Ol_Chiki =
                    new UnicodeBlock(nameof(Ol_Chiki), Ol_Chiki_ID); /* [1C50] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Cyrillic_Extended_A =
                    new UnicodeBlock(nameof(Cyrillic_Extended_A), Cyrillic_Extended_A_ID); /* [2DE0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Vai = new UnicodeBlock(nameof(Vai), Vai_ID); /* [A500] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Cyrillic_Extended_B =
                    new UnicodeBlock(nameof(Cyrillic_Extended_B), Cyrillic_Extended_B_ID); /* [A640] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Saurashtra =
                    new UnicodeBlock(nameof(Saurashtra), Saurashtra_ID); /* [A880] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Kayah_Li =
                    new UnicodeBlock(nameof(Kayah_Li), Kayah_Li_ID); /* [A900] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Rejang =
                    new UnicodeBlock(nameof(Rejang), Rejang_ID); /* [A930] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Cham =
                    new UnicodeBlock(nameof(Cham), Cham_ID); /* [AA00] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Ancient_Symbols =
                    new UnicodeBlock(nameof(Ancient_Symbols), Ancient_Symbols_ID); /* [10190] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Phaistos_Disc =
                    new UnicodeBlock(nameof(Phaistos_Disc), Phaistos_Disc_ID); /* [101D0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Lycian =
                    new UnicodeBlock(nameof(Lycian), Lycian_ID); /* [10280] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Carian =
                    new UnicodeBlock(nameof(Carian), Carian_ID); /* [102A0] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Lydian =
                    new UnicodeBlock(nameof(Lydian), Lydian_ID); /* [10920] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Mahjong_Tiles =
                    new UnicodeBlock(nameof(Mahjong_Tiles), Mahjong_Tiles_ID); /* [1F000] */

            /// <stable>ICU 4.0</stable>
            public static readonly UnicodeBlock Domino_Tiles =
                    new UnicodeBlock(nameof(Domino_Tiles), Domino_Tiles_ID); /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Samaritan =
                    new UnicodeBlock(nameof(Samaritan), Samaritan_ID); /*[0800]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Unified_Canadian_Aboriginal_Syllabics_Extended =
                    new UnicodeBlock(nameof(Unified_Canadian_Aboriginal_Syllabics_Extended),
                            Unified_Canadian_Aboriginal_Syllabics_Extended_ID); /*[18B0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Tai_Tham =
                    new UnicodeBlock(nameof(Tai_Tham), Tai_Tham_ID); /*[1A20]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Vedic_Extensions =
                    new UnicodeBlock(nameof(Vedic_Extensions), Vedic_Extensions_ID); /*[1CD0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Lisu =
                    new UnicodeBlock(nameof(Lisu), Lisu_ID); /*[A4D0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Bamum =
                    new UnicodeBlock(nameof(Bamum), Bamum_ID); /*[A6A0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Common_Indic_Number_Forms =
                    new UnicodeBlock(nameof(Common_Indic_Number_Forms), Common_Indic_Number_Forms_ID); /*[A830]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Devanagari_Extended =
                    new UnicodeBlock(nameof(Devanagari_Extended), Devanagari_Extended_ID); /*[A8E0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Hangul_Jamo_Extended_A =
                    new UnicodeBlock(nameof(Hangul_Jamo_Extended_A), Hangul_Jamo_Extended_A_ID); /*[A960]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Javanese =
                    new UnicodeBlock(nameof(Javanese), Javanese_ID); /*[A980]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Myanmar_Extended_A =
                    new UnicodeBlock(nameof(Myanmar_Extended_A), Myanmar_Extended_A_ID); /*[AA60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Tai_Viet =
                    new UnicodeBlock(nameof(Tai_Viet), Tai_Viet_ID); /*[AA80]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Meetei_Mayek =
                    new UnicodeBlock(nameof(Meetei_Mayek), Meetei_Mayek_ID); /*[ABC0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Hangul_Jamo_Extended_B =
                    new UnicodeBlock(nameof(Hangul_Jamo_Extended_B), Hangul_Jamo_Extended_B_ID); /*[D7B0]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Imperial_Aramaic =
                    new UnicodeBlock(nameof(Imperial_Aramaic), Imperial_Aramaic_ID); /*[10840]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Old_South_Arabian =
                    new UnicodeBlock(nameof(Old_South_Arabian), Old_South_Arabian_ID); /*[10A60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Avestan =
                    new UnicodeBlock(nameof(Avestan), Avestan_ID); /*[10B00]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Inscriptional_Parthian =
                    new UnicodeBlock(nameof(Inscriptional_Parthian), Inscriptional_Parthian_ID); /*[10B40]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Inscriptional_Pahlavi =
                    new UnicodeBlock(nameof(Inscriptional_Pahlavi), Inscriptional_Pahlavi_ID); /*[10B60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Old_Turkic =
                    new UnicodeBlock(nameof(Old_Turkic), Old_Turkic_ID); /*[10C00]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Rumi_Numeral_Symbols =
                    new UnicodeBlock(nameof(Rumi_Numeral_Symbols), Rumi_Numeral_Symbols_ID); /*[10E60]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Kaithi =
                    new UnicodeBlock(nameof(Kaithi), Kaithi_ID); /*[11080]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Egyptian_Hieroglyphs =
                    new UnicodeBlock(nameof(Egyptian_Hieroglyphs), Egyptian_Hieroglyphs_ID); /*[13000]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Enclosed_Alphanumeric_Supplement =
                    new UnicodeBlock(nameof(Enclosed_Alphanumeric_Supplement),
                            Enclosed_Alphanumeric_Supplement_ID); /*[1F100]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock Enclosed_Ideographic_Supplement =
                    new UnicodeBlock(nameof(Enclosed_Ideographic_Supplement),
                            Enclosed_Ideographic_Supplement_ID); /*[1F200]*/
            /// <stable>ICU 4.4</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_C =
                    new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_C),
                            CJK_Unified_Ideographs_Extension_C_ID); /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Mandaic =
                    new UnicodeBlock(nameof(Mandaic), Mandaic_ID); /*[0840]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Batak =
                    new UnicodeBlock(nameof(Batak), Batak_ID); /*[1BC0]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Ethiopic_Extended_A =
                    new UnicodeBlock(nameof(Ethiopic_Extended_A), Ethiopic_Extended_A_ID); /*[AB00]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Brahmi =
                    new UnicodeBlock(nameof(Brahmi), Brahmi_ID); /*[11000]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Bamum_Supplement =
                    new UnicodeBlock(nameof(Bamum_Supplement), Bamum_Supplement_ID); /*[16800]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Kana_Supplement =
                    new UnicodeBlock(nameof(Kana_Supplement), Kana_Supplement_ID); /*[1B000]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Playing_Cards =
                    new UnicodeBlock(nameof(Playing_Cards), Playing_Cards_ID); /*[1F0A0]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Miscellaneous_Symbols_And_Pictographs =
                    new UnicodeBlock(nameof(Miscellaneous_Symbols_And_Pictographs),
                            Miscellaneous_Symbols_And_Pictographs_ID); /*[1F300]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Emoticons =
                    new UnicodeBlock(nameof(Emoticons), Emoticons_ID); /*[1F600]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Transport_And_Map_Symbols =
                    new UnicodeBlock(nameof(Transport_And_Map_Symbols), Transport_And_Map_Symbols_ID); /*[1F680]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock Alchemical_Symbols =
                    new UnicodeBlock(nameof(Alchemical_Symbols), Alchemical_Symbols_ID); /*[1F700]*/
            /// <stable>ICU 4.6</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_D =
                    new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_D),
                            CJK_Unified_Ideographs_Extension_D_ID); /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Arabic_Extended_A =
                    new UnicodeBlock(nameof(Arabic_Extended_A), Arabic_Extended_A_ID); /*[08A0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Arabic_Mathematical_Alphabetic_Symbols =
                    new UnicodeBlock(nameof(Arabic_Mathematical_Alphabetic_Symbols), Arabic_Mathematical_Alphabetic_Symbols_ID); /*[1EE00]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Chakma = new UnicodeBlock(nameof(Chakma), Chakma_ID); /*[11100]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Meetei_Mayek_Extensions =
                    new UnicodeBlock(nameof(Meetei_Mayek_Extensions), Meetei_Mayek_Extensions_ID); /*[AAE0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Meroitic_Cursive =
                    new UnicodeBlock(nameof(Meroitic_Cursive), Meroitic_Cursive_ID); /*[109A0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Meroitic_Hieroglyphs =
                    new UnicodeBlock(nameof(Meroitic_Hieroglyphs), Meroitic_Hieroglyphs_ID); /*[10980]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Miao = new UnicodeBlock(nameof(Miao), Miao_ID); /*[16F00]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Sharada = new UnicodeBlock(nameof(Sharada), Sharada_ID); /*[11180]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Sora_Sompeng =
                    new UnicodeBlock(nameof(Sora_Sompeng), Sora_Sompeng_ID); /*[110D0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Sundanese_Supplement =
                    new UnicodeBlock(nameof(Sundanese_Supplement), Sundanese_Supplement_ID); /*[1CC0]*/
            /// <stable>ICU 49</stable>
            public static readonly UnicodeBlock Takri = new UnicodeBlock(nameof(Takri), Takri_ID); /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Bassa_Vah = new UnicodeBlock(nameof(Bassa_Vah), Bassa_Vah_ID); /*[16AD0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Caucasian_Albanian =
                    new UnicodeBlock(nameof(Caucasian_Albanian), Caucasian_Albanian_ID); /*[10530]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Coptic_Epact_Numbers =
                    new UnicodeBlock(nameof(Coptic_Epact_Numbers), Coptic_Epact_Numbers_ID); /*[102E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Combining_Diacritical_Marks_Extended =
                    new UnicodeBlock(nameof(Combining_Diacritical_Marks_Extended), Combining_Diacritical_Marks_Extended_ID); /*[1AB0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Duployan = new UnicodeBlock(nameof(Duployan), Duployan_ID); /*[1BC00]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Elbasan = new UnicodeBlock(nameof(Elbasan), Elbasan_ID); /*[10500]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Geometric_Shapes_Extended =
                    new UnicodeBlock(nameof(Geometric_Shapes_Extended), Geometric_Shapes_Extended_ID); /*[1F780]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Grantha = new UnicodeBlock(nameof(Grantha), Grantha_ID); /*[11300]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Khojki = new UnicodeBlock(nameof(Khojki), Khojki_ID); /*[11200]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Khudawadi = new UnicodeBlock(nameof(Khudawadi), Khudawadi_ID); /*[112B0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Latin_Extended_E =
                    new UnicodeBlock(nameof(Latin_Extended_E), Latin_Extended_E_ID); /*[AB30]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Linear_A = new UnicodeBlock(nameof(Linear_A), Linear_A_ID); /*[10600]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Mahajani = new UnicodeBlock(nameof(Mahajani), Mahajani_ID); /*[11150]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Manichaean = new UnicodeBlock(nameof(Manichaean), Manichaean_ID); /*[10AC0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Mende_Kikakui =
                    new UnicodeBlock(nameof(Mende_Kikakui), Mende_Kikakui_ID); /*[1E800]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Modi = new UnicodeBlock(nameof(Modi), Modi_ID); /*[11600]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Mro = new UnicodeBlock(nameof(Mro), Mro_ID); /*[16A40]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Myanmar_Extended_B =
                    new UnicodeBlock(nameof(Myanmar_Extended_B), Myanmar_Extended_B_ID); /*[A9E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Nabataean = new UnicodeBlock(nameof(Nabataean), Nabataean_ID); /*[10880]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Old_North_Arabian =
                    new UnicodeBlock(nameof(Old_North_Arabian), Old_North_Arabian_ID); /*[10A80]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Old_Permic = new UnicodeBlock(nameof(Old_Permic), Old_Permic_ID); /*[10350]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Ornamental_Dingbats =
                    new UnicodeBlock(nameof(Ornamental_Dingbats), Ornamental_Dingbats_ID); /*[1F650]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Pahawh_Hmong = new UnicodeBlock(nameof(Pahawh_Hmong), Pahawh_Hmong_ID); /*[16B00]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Palmyrene = new UnicodeBlock(nameof(Palmyrene), Palmyrene_ID); /*[10860]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Pau_Cin_Hau = new UnicodeBlock(nameof(Pau_Cin_Hau), Pau_Cin_Hau_ID); /*[11AC0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Psalter_Pahlavi =
                    new UnicodeBlock(nameof(Psalter_Pahlavi), Psalter_Pahlavi_ID); /*[10B80]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Shorthand_Format_Controls =
                    new UnicodeBlock(nameof(Shorthand_Format_Controls), Shorthand_Format_Controls_ID); /*[1BCA0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Siddham = new UnicodeBlock(nameof(Siddham), Siddham_ID); /*[11580]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Sinhala_Archaic_Numbers =
                    new UnicodeBlock(nameof(Sinhala_Archaic_Numbers), Sinhala_Archaic_Numbers_ID); /*[111E0]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Supplemental_Arrows_C =
                    new UnicodeBlock(nameof(Supplemental_Arrows_C), Supplemental_Arrows_C_ID); /*[1F800]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Tirhuta = new UnicodeBlock(nameof(Tirhuta), Tirhuta_ID); /*[11480]*/
            /// <stable>ICU 54</stable>
            public static readonly UnicodeBlock Warang_Citi = new UnicodeBlock(nameof(Warang_Citi), Warang_Citi_ID); /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Ahom = new UnicodeBlock(nameof(Ahom), Ahom_ID); /*[11700]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Anatolian_Hieroglyphs =
                    new UnicodeBlock(nameof(Anatolian_Hieroglyphs), Anatolian_Hieroglyphs_ID); /*[14400]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Cherokee_Supplement =
                    new UnicodeBlock(nameof(Cherokee_Supplement), Cherokee_Supplement_ID); /*[AB70]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_E =
                    new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_E),
                            CJK_Unified_Ideographs_Extension_E_ID); /*[2B820]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Early_Dynastic_Cuneiform =
                    new UnicodeBlock(nameof(Early_Dynastic_Cuneiform), Early_Dynastic_Cuneiform_ID); /*[12480]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Hatran = new UnicodeBlock(nameof(Hatran), Hatran_ID); /*[108E0]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Multani = new UnicodeBlock(nameof(Multani), Multani_ID); /*[11280]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Old_Hungarian =
                    new UnicodeBlock(nameof(Old_Hungarian), Old_Hungarian_ID); /*[10C80]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Supplemental_Symbols_And_Pictographs =
                    new UnicodeBlock(nameof(Supplemental_Symbols_And_Pictographs),
                            Supplemental_Symbols_And_Pictographs_ID); /*[1F900]*/
            /// <stable>ICU 56</stable>
            public static readonly UnicodeBlock Sutton_Signwriting =
                    new UnicodeBlock(nameof(Sutton_Signwriting), Sutton_Signwriting_ID); /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Adlam = new UnicodeBlock(nameof(Adlam), Adlam_ID); /*[1E900]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Bhaiksuki = new UnicodeBlock(nameof(Bhaiksuki), Bhaiksuki_ID); /*[11C00]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Cyrillic_Extended_C =
                    new UnicodeBlock(nameof(Cyrillic_Extended_C), Cyrillic_Extended_C_ID); /*[1C80]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Glagolitic_Supplement =
                    new UnicodeBlock(nameof(Glagolitic_Supplement), Glagolitic_Supplement_ID); /*[1E000]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Ideographic_Symbols_And_Punctuation =
                    new UnicodeBlock(nameof(Ideographic_Symbols_And_Punctuation), Ideographic_Symbols_And_Punctuation_ID); /*[16FE0]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Marchen = new UnicodeBlock(nameof(Marchen), Marchen_ID); /*[11C70]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Mongolian_Supplement =
                    new UnicodeBlock(nameof(Mongolian_Supplement), Mongolian_Supplement_ID); /*[11660]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Newa = new UnicodeBlock(nameof(Newa), Newa_ID); /*[11400]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Osage = new UnicodeBlock(nameof(Osage), Osage_ID); /*[104B0]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Tangut = new UnicodeBlock(nameof(Tangut), Tangut_ID); /*[17000]*/
            /// <stable>ICU 58</stable>
            public static readonly UnicodeBlock Tangut_Components =
                    new UnicodeBlock(nameof(Tangut_Components), Tangut_Components_ID); /*[18800]*/

            // New blocks in Unicode 10.0

            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock CJK_Unified_Ideographs_Extension_F =
                    new UnicodeBlock(nameof(CJK_Unified_Ideographs_Extension_F), CJK_Unified_Ideographs_Extension_F_ID); /*[2CEB0]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Kana_Extended_A =
                    new UnicodeBlock(nameof(Kana_Extended_A), Kana_Extended_A_ID); /*[1B100]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Masaram_Gondi =
                    new UnicodeBlock(nameof(Masaram_Gondi), Masaram_Gondi_ID); /*[11D00]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Nushu = new UnicodeBlock(nameof(Nushu), Nushu_ID); /*[1B170]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Soyombo = new UnicodeBlock(nameof(Soyombo), Soyombo_ID); /*[11A50]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Syriac_Supplement =
                    new UnicodeBlock(nameof(Syriac_Supplement), Syriac_Supplement_ID); /*[0860]*/
            /// <stable>ICU 60</stable>
            public static readonly UnicodeBlock Zanabazar_Square =
                    new UnicodeBlock(nameof(Zanabazar_Square), Zanabazar_Square_ID); /*[11A00]*/

            /// <stable>ICU 2.4</stable>
            public static readonly UnicodeBlock Invalid_Code
                = new UnicodeBlock(nameof(Invalid_Code), Invalid_Code_ID);

            static UnicodeBlock()
            {
#pragma warning disable 612, 618
                for (int blockId = 0; blockId < Count; ++blockId)
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
            /// If no such ID exists, a <see cref="Invalid_Code"/> <see cref="UnicodeBlock"/> will be returned.
            /// </summary>
            /// <param name="id"><see cref="UnicodeBlock"/> ID.</param>
            /// <returns>the only instance of the <see cref="UnicodeBlock"/> with the argument ID
            /// if it exists, otherwise a <see cref="Invalid_Code"/> <see cref="UnicodeBlock"/> will be
            /// returned.
            /// </returns>
            /// <stable>ICU 2.4</stable>
            public static UnicodeBlock GetInstance(int id)
            {
                if (id >= 0 && id < BLOCKS_.Length)
                {
                    return BLOCKS_[id];
                }
                return Invalid_Code;
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
                    return Invalid_Code;
                }

                return UnicodeBlock.GetInstance(
                        UCharacterProperty.Instance.GetIntPropertyValue(ch, (int)UProperty.Block));
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
        /// <seealso cref="UChar.GetIntPropertyValue(int, UProperty)"/>
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
            public const int Count = 18;
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
            public const int Count = 6;
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
        /// Same as <see cref="Character.MinCodePoint"/>, same integer value as <see cref="Char.MinValue"/>.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int MinValue = Character.MinCodePoint;

        /// <summary>
        /// The highest Unicode code point value (scalar value), constant U+10FFFF (uses 21 bits).
        /// Same integer value as <see cref="Character.MaxCodePoint"/>.
        /// <para/>
        /// Up-to-date Unicode implementation of <see cref="Char.MaxValue"/>
        /// which is still a char with the value U+FFFF.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int MaxValue = Character.MaxCodePoint;

        /// <summary>
        /// The minimum value for Supplementary code points, constant U+10000.
        /// Same as <see cref="Character.MinSupplementaryCodePoint"/>.
        /// </summary>
        /// <stable>ICU 2.1</stable>
        public const int SupplementaryMinValue = Character.MinSupplementaryCodePoint;

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
        /// Compatibility constant for <see cref="Character.MinRadix"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int MinRadix = Character.MinRadix;

        /// <summary>
        /// Compatibility constant for <see cref="Character.MaxRadix"/>.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int MaxRadix = Character.MaxRadix;

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
        internal static bool IsSpace(int ch) // ICU4N specific - marked this java-ism internal instead of public
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
        /// <param name="ch">Code point whose category is to be determined.</param>
        /// <returns>Category which is a value of <see cref="UUnicodeCategory"/>.</returns>
        /// <stable>ICU 2.1</stable>
        public static UUnicodeCategory GetUnicodeCategory(int ch) // ICU4N specific - renamed from GetType() to cover System.Char.GetUnicodeCategory()
        {
            return (UUnicodeCategory)UCharacterProperty.Instance.GetType(ch);
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
        /// <param name="c"><see cref="char"/> whose category is to be determined.</param>
        /// <returns>Category which is a value of <see cref="UUnicodeCategory"/>.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static UUnicodeCategory GetUnicodeCategory(char c) // ICU4N specific overload to cover System.Char
        {
            return GetUnicodeCategory((int)c);
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
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The character position in <paramref name="s"/>.</param>
        /// <returns>Category which is a value of <see cref="UUnicodeCategory"/>.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static UUnicodeCategory GetUnicodeCategory(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return GetUnicodeCategory((int)s[index]);
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
            return GetUnicodeCategory(ch) != UUnicodeCategory.OtherNotAssigned;
        }

        /// <summary>
        /// Determines if a code point has a defined meaning in the up-to-date
        /// Unicode standard.
        /// E.g. supplementary code points though allocated space are not defined in
        /// Unicode yet.
        /// </summary>
        /// <param name="c"><see cref="char"/> to be determined if it is defined in the most
        /// current version of Unicode.</param>
        /// <returns>true if this code point is defined in unicode.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsDefined(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsDefined((int)c);
        }

        /// <summary>
        /// Determines if a code point has a defined meaning in the up-to-date
        /// Unicode standard.
        /// E.g. supplementary code points though allocated space are not defined in
        /// Unicode yet.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if this code point is defined in unicode.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsDefined(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsDefined((int)s[index]);
        }

        /// <summary>
        /// Determines if a code point is a .NET digit.
        /// <para/>
        /// It returns true for decimal digits only.
        /// </summary>
        /// <param name="ch">Code point to query.</param>
        /// <returns>true if this code point is a digit.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsDigit(int ch)
        {
            return GetUnicodeCategory(ch) == UUnicodeCategory.DecimalDigitNumber;
        }

        /// <summary>
        /// Determines if a code point is a .NET digit.
        /// <para/>
        /// It returns true for decimal digits only.
        /// </summary>
        /// <param name="c"><see cref="char"/> to query.</param>
        /// <returns>true if this code point is a digit.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsDigit(char c) // ICU4N specific overload to cover System.Char
        {
            return IsDigit((int)c);
        }

        /// <summary>
        /// Determines if a code point is a .NET digit.
        /// <para/>
        /// It returns true for decimal digits only.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if this code point is a digit.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsDigit(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsDigit((int)s[index]);
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
        public static bool IsISOControl(int ch)
        {
            return ch >= 0 && ch <= APPLICATION_PROGRAM_COMMAND_ &&
                    ((ch <= UNIT_SEPARATOR_) || (ch >= DELETE_));
        }

        /// <summary>
        /// Determines if the specified code point is an ISO control character.
        /// A code point is considered to be an ISO control character if it is in
        /// the range &#92;u0000 through &#92;u001F or in the range &#92;u007F through
        /// &#92;u009F.
        /// </summary>
        /// <param name="c"><see cref="char"/> to determine if it is an ISO control character.</param>
        /// <returns>true if code point is a ISO control character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsISOControl(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsISOControl((int)c);
        }

        /// <summary>
        /// Determines if the specified code point is an ISO control character.
        /// A code point is considered to be an ISO control character if it is in
        /// the range &#92;u0000 through &#92;u001F or in the range &#92;u007F through
        /// &#92;u009F.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index"></param>
        /// <returns>true if code point is a ISO control character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsISOControl(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsISOControl((int)s[index]);
        }

        /// <summary>
        /// Determines if the specified code point is a letter.
        /// Up-to-date Unicode implementation of <see cref="char.IsLetter(char)"/>.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a letter.</param>
        /// <returns>true if code point is a letter.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLetter(int ch)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetUnicodeCategory(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32()))) != 0;
        }

        /// <summary>
        /// Determines if the specified code point is a letter.
        /// Up-to-date Unicode implementation of <see cref="char.IsLetter(char)"/>.
        /// </summary>
        /// <param name="c"><see cref="char"/> to determine if it is a letter.</param>
        /// <returns>true if code point is a letter.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLetter(char c) // ICU4N specific overload to cover System.Char
        {
            return IsLetter((int)c);
        }

        /// <summary>
        /// Determines if the specified code point is a letter.
        /// Up-to-date Unicode implementation of <see cref="char.IsLetter(string, int)"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is a letter.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLetter(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsLetter((int)s[index]);
        }

        /// <summary>
        /// Determines if the specified code point is a letter or digit.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a letter or a digit.</param>
        /// <returns>true if code point is a letter or a digit.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsLetterOrDigit(int ch)
        {
            return ((1 << GetUnicodeCategory(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32())
                            | (1 << UUnicodeCategory.DecimalDigitNumber.ToInt32()))) != 0;
        }

        /// <summary>
        /// Determines if the specified code point is a letter or digit.
        /// </summary>
        /// <param name="c"><see cref="char"/> to determine if it is a letter or a digit.</param>
        /// <returns>true if code point is a letter or a digit.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLetterOrDigit(char c) // ICU4N specific overload to cover System.Char
        {
            return IsLetterOrDigit((int)c);
        }

        /// <summary>
        /// Determines if the specified code point is a letter or digit.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is a letter or a digit.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLetterOrDigit(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsLetterOrDigit((int)s[index]);
        }

        // ICU4N: We definitely don't need any of the Java.. functions. 
        // In .NET, it is not so straightforward to determine if a character
        // is an identifier (and it is not built in).

        // ICU4N TODO: Perhaps it would make sense to duplicate these from Java,
        // since in .NET determining if a string is an identifier is difficult.
        // Note we will need to do this for C# and VB at least (possibly others).
        // Here are the exact rules for C#: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure#identifiers

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
        public static bool IsLower(int ch) // ICU4N specific - renamed from IsLowerCase
        {
            // if props == 0, it will just fall through and return false
            return GetUnicodeCategory(ch) == UUnicodeCategory.LowercaseLetter;
        }

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
        /// <param name="c"><see cref="char"/> to determine if it is in lowercase.</param>
        /// <returns>true if code point is a lowercase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLower(char c) // ICU4N specific overload to cover System.Char
        {
            return IsLower((int)c);
        }

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
        /// Up-to-date Unicode implementation of <see cref="char.IsLower(string, int)"/>.
        /// </remarks>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is a lowercase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsLower(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsLower((int)s[index]);
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
        public static bool IsWhiteSpace(int ch) // ICU4N specific - renamed from IsWhitespace (lowercase s)
        {
            // exclude no-break spaces
            // if props == 0, it will just fall through and return false
            return ((1 << (int)GetUnicodeCategory(ch)) &
                    ((1 << (int)UUnicodeCategory.SpaceSeparator)
                            | (1 << (int)UUnicodeCategory.LineSeparator)
                            | (1 << (int)UUnicodeCategory.ParagraphSeparator))) != 0
                            && (ch != NO_BREAK_SPACE_) && (ch != FIGURE_SPACE_) && (ch != NARROW_NO_BREAK_SPACE_)
                            // TAB VT LF FF CR FS GS RS US NL are all control characters
                            // that are white spaces.
                            || (ch >= 0x9 && ch <= 0xd) || (ch >= 0x1c && ch <= 0x1f);
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
        /// <param name="c"><see cref="char"/> to determine if it is a white space.</param>
        /// <returns>true if the specified code point is a white space character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsWhiteSpace(char c) // ICU4N specific overload to cover System.Char
        {
            return IsWhiteSpace(c);
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
        /// This API tries to sync with the semantics of .NET's <see cref="char.IsWhiteSpace(string, int)"/>, 
        /// but it may not return the exact same results because of the Unicode version
        /// difference.
        /// <para/>
        /// Note: Unicode 4.0.1 changed U+200B ZERO WIDTH SPACE from a Space Separator (Zs)
        /// to a Format Control (Cf). Since then, <c>IsWhitespace(0x200b)</c> returns false.
        /// See <a href="http://www.unicode.org/versions/Unicode4.0.1/">http://www.unicode.org/versions/Unicode4.0.1/</a>.
        /// </remarks>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the specified code point is a white space character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsWhiteSpace(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsWhiteSpace((int)s[index]);
        }

        /// <summary>
        /// Determines if the specified code point is a Unicode specified space
        /// character, i.e. if code point is in the category Zs, Zl and Zp.
        /// </summary>
        /// <param name="ch">Code point to determine if it is a space.</param>
        /// <returns>true if the specified code point is a space character.</returns>
        /// <stable>ICU 2.1</stable>
        public static bool IsSpaceChar(int ch)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetUnicodeCategory(ch).ToInt32()) & ((1 << UUnicodeCategory.SpaceSeparator.ToInt32())
                    | (1 << UUnicodeCategory.LineSeparator.ToInt32())
                    | (1 << UUnicodeCategory.ParagraphSeparator.ToInt32())))
                    != 0;
        }

        /// <summary>
        /// Determines if the specified code point is a Unicode specified space
        /// character, i.e. if code point is in the category Zs, Zl and Zp.
        /// </summary>
        /// <param name="c"><see cref="char"/> to determine if it is a space.</param>
        /// <returns>true if the specified code point is a space character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsSpaceChar(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsSpaceChar((int)c);
        }

        /// <summary>
        /// Determines if the specified code point is a Unicode specified space
        /// character, i.e. if code point is in the category Zs, Zl and Zp.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the specified code point is a space character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsSpaceChar(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsSpaceChar((int)s[index]);
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
        public static bool IsTitleCase(int ch)
        {
            // if props == 0, it will just fall through and return false
            return GetUnicodeCategory(ch) == UUnicodeCategory.TitlecaseLetter;
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
        /// <param name="c"><see cref="char"/> to determine if it is in title case.</param>
        /// <returns>true if the specified code point is a titlecase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsTitleCase(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsTitleCase((int)c);
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
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the specified code point is a titlecase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsTitleCase(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsTitleCase((int)s[index]);
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
        public static bool IsUnicodeIdentifierPart(int ch)
        {
            // if props == 0, it will just fall through and return false
            // cat == format
            return ((1 << GetUnicodeCategory(ch).ToInt32())
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
        /// <param name="c"><see cref="char"/> to determine if is can be part of a Unicode
        /// identifier.</param>
        /// <returns>true if code point is any character belonging a unicode
        /// identifier suffix after the first character.
        /// </returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUnicodeIdentifierPart(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsUnicodeIdentifierPart((int)c);
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
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is any character belonging a unicode
        /// identifier suffix after the first character.
        /// </returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUnicodeIdentifierPart(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUnicodeIdentifierPart((int)s[index]);
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
        public static bool IsUnicodeIdentifierStart(int ch)
        {
            /*int cat = getType(ch);*/
            // if props == 0, it will just fall through and return false
            return ((1 << GetUnicodeCategory(ch).ToInt32())
                    & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.LowercaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.TitlecaseLetter.ToInt32())
                            | (1 << UUnicodeCategory.ModifierLetter.ToInt32())
                            | (1 << UUnicodeCategory.OtherLetter.ToInt32())
                            | (1 << UUnicodeCategory.LetterNumber.ToInt32()))) != 0;
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
        /// <param name="c"><see cref="char"/> to determine if it can start a Unicode identifier.</param>
        /// <returns>true if code point is the first character belonging a unicode
        /// identifier.
        /// </returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUnicodeIdentifierStart(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsUnicodeIdentifierStart((int)c);
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
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is the first character belonging a unicode
        /// identifier.
        /// </returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUnicodeIdentifierStart(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUnicodeIdentifierStart((int)s[index]);
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
            return GetUnicodeCategory(ch) == UUnicodeCategory.Format;
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
        /// <param name="c"><see cref="char"/> to be determined if it can be ignored in a Unicode
        /// identifier.</param>
        /// <returns>true if the code point is ignorable.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsIdentifierIgnorable(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsIdentifierIgnorable((int)c);
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
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the code point is ignorable.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsIdentifierIgnorable(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsIdentifierIgnorable((int)s[index]);
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
        public static bool IsUpper(int ch) // ICU4N specific - renamed from IsUpperCase
        {
            // if props == 0, it will just fall through and return false
            return GetUnicodeCategory(ch) == UUnicodeCategory.UppercaseLetter;
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
        /// <param name="c"><see cref="char"/> to determine if it is in uppercase.</param>
        /// <returns>true if the <see cref="char"/> is an uppercase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUpper(char c) // ICU4N specific overload to cover System.Char
        {
            return IsUpper((int)c);
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
        /// Up-to-date Unicode implementation of <see cref="Char.IsUpper(string, int)"/>.
        /// </remarks>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the <see cref="char"/> is an uppercase character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUpper(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUpper((int)s[index]);
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
        /// <param name="utf32">Code point.</param>
        /// <returns>String representation of the code point, null if code point is not
        /// defined in unicode.
        /// </returns>
        /// <stable>ICU 2.1</stable>
        public static string ConvertFromUtf32(int utf32) // ICU4N: Renamed from ToString to cover System.Char API
        {
            if (utf32 < MinValue || utf32 > MaxValue)
            {
                return null;
            }

            if (utf32 < SupplementaryMinValue)
            {
                return new string(new char[] { (char)utf32 });
            }

            return new string(Character.ToChars(utf32));
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
        /// Determines if the code point is a supplementary character.
        /// A code point is a supplementary character if and only if it is greater
        /// than <see cref="SupplementaryMinValue"/>.
        /// </summary>
        /// <param name="c"><see cref="char"/> to be determined if it is in the supplementary plane.</param>
        /// <returns>true if code point is a supplementary character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsSupplementary(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsSupplementary((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Determines if the code point is a supplementary character.
        /// A code point is a supplementary character if and only if it is greater
        /// than <see cref="SupplementaryMinValue"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is a supplementary character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsSupplementary(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsSupplementary((int)s[index]);
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
        /// Determines if the code point is in the BMP plane.
        /// </summary>
        /// <param name="c"><see cref="char"/> to be determined if it is not a supplementary character.</param>
        /// <returns>true if code point is not a supplementary character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsBMP(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsBMP((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Determines if the code point is in the BMP plane.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if code point is not a supplementary character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsBMP(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsBMP((int)s[index]);
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
            UUnicodeCategory cat = GetUnicodeCategory(ch);
            // if props == 0, it will just fall through and return false
            return (cat != UUnicodeCategory.OtherNotAssigned &&
                    cat != UUnicodeCategory.Control &&
                    cat != UUnicodeCategory.Format &&
                    cat != UUnicodeCategory.PrivateUse &&
                    cat != UUnicodeCategory.Surrogate);
        }

        /// <icu/>
        /// <summary>
        /// Determines whether the specified code point is a printable character
        /// according to the Unicode standard.
        /// </summary>
        /// <param name="c"><see cref="char"/> to be determined if it is printable.</param>
        /// <returns>true if the code point is a printable character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsPrintable(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsPrintable((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Determines whether the specified code point is a printable character
        /// according to the Unicode standard.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the code point is a printable character.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsPrintable(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsPrintable((int)s[index]);
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
            UUnicodeCategory cat = GetUnicodeCategory(ch);
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
        /// Determines whether the specified code point is of base form.
        /// A code point of base form does not graphically combine with preceding
        /// characters, and is neither a control nor a format character.
        /// </summary>
        /// <param name="c"><see cref="char"/> to be determined if it is of base form.</param>
        /// <returns>true if the code point is of base form.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsBaseForm(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsBaseForm((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Determines whether the specified code point is of base form.
        /// A code point of base form does not graphically combine with preceding
        /// characters, and is neither a control nor a format character.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the code point is of base form.</returns>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsBaseForm(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsBaseForm((int)s[index]);
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
            if (ch < Character.MinSurrogate)
            {
                return true;
            }
            if (ch <= Character.MaxSurrogate)
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
        /// A string is legal if and only if all its code points are legal.
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
        ///         non-zero value <see cref="UChar.UnicodeBlock.Basic_Latin_ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.Canonical_Combining_Class"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.General_Category_Mask"/> values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UChar.GetUnicodeCategory(int)"/>.  This allows grouped categories such as
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
        ///         non-zero value <see cref="UChar.UnicodeBlock.Basic_Latin_ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.Canonical_Combining_Class"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.General_Category_Mask"/> values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UChar.GetUnicodeCategory(int)"/>.  This allows grouped categories such as
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
        /// <param name="highSurrogate">The lead char (high surrogate).</param>
        /// <param name="lowSurrogate">The trail char (low surrogate).</param>
        /// <returns>Code point if surrogate characters are valid.</returns>
        /// <exception cref="ArgumentException">Thrown when the code units do not form a valid code point.</exception>
        /// <stable>ICU 2.1</stable>
        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate) // ICU4N specific - renamed from GetCodePoint() to match System.Char
        {
            if (char.IsSurrogatePair(highSurrogate, lowSurrogate))
            {
                return Character.ToCodePoint(highSurrogate, lowSurrogate);
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
        public static int ConvertToUtf32(char char16) // ICU4N specific - renamed from GetCodePoint() to match System.Char
        {
            if (UChar.IsLegal(char16))
            {
                return char16;
            }
            throw new ArgumentException("Illegal codepoint");
        }

        /// <icu/>
        /// <summary>
        /// Returns the code point corresponding to the BMP code point.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>Code point if char at <paramref name="index"/> is a valid character.</returns>
        /// <exception cref="ArgumentException">Thrown when the code units do not form a valid code point.</exception>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static int ConvertToUtf32(string s, int index) // ICU4N specific overload to cover System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ConvertToUtf32(s[index]);
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

        private static CaseLocale GetDefaultCaseLocale() 
        {
            return UCaseProps.GetCaseLocale(CultureInfo.CurrentCulture);
        }

        private static CaseLocale GetCaseLocale(CultureInfo locale)
        {
            if (locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            return UCaseProps.GetCaseLocale(locale);
        }

        private static CaseLocale GetCaseLocale(ULocale locale)
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

        private static readonly TitleCaseMap TO_TITLE_WHOLE_STRING_NO_LOWERCASE =
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
                if (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
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

            private IEnumerator<Trie2Range> trieIterator;
            private Trie2Range range;

            private sealed class MaskType : IValueMapper
            {
                // Extracts the general category ("character type") from the trie value.
                public int Map(int value)
                {
                    return value & UCharacterProperty.TypeMask;
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
        /// Check if a code point has the <see cref="UProperty.Alphabetic"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Alphabetic"/>.
        /// <para/>
        /// Different from <see cref="IsLetter(char)"/>!
        /// </summary>
        /// <param name="c"><see cref="char"/> to be tested.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUAlphabetic(char c)
        {
            return IsUAlphabetic((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Alphabetic"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Alphabetic"/>.
        /// <para/>
        /// Different from <see cref="IsLetter(string, int)"/>!
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUAlphabetic(string s, int index)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUAlphabetic((int)s[index]);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Lowercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Lowercase"/>.
        /// <para/>
        /// This is different from <see cref="IsLower(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsULower(int ch) // ICU4N specific - renamed from IsULowercase
        {
            return HasBinaryProperty(ch, UProperty.Lowercase);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Lowercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Lowercase"/>.
        /// <para/>
        /// This is different from <see cref="IsLower(char)"/>!
        /// </summary>
        /// <param name="c"><see cref="char"/> to be tested.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsULower(char c) // ICU4N specific - renamed from IsULowercase
        {
            return IsULower((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Lowercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Lowercase"/>.
        /// <para/>
        /// This is different from <see cref="IsLower(string, int)"/>!
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsULower(string s, int index) // ICU4N specific - renamed from IsULowercase
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsULower((int)s[index]);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Uppercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Uppercase"/>.
        /// <para/>
        /// This is different from <see cref="IsUpper(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsUUpper(int ch) // ICU4N specific - renamed from IsUUppercase
        {
            return HasBinaryProperty(ch, UProperty.Uppercase);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Uppercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Uppercase"/>.
        /// <para/>
        /// This is different from <see cref="IsUpper(char)"/>!
        /// </summary>
        /// <param name="c"><see cref="char"/> to be tested.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUUpper(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsUUpper((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.Uppercase"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.Uppercase"/>.
        /// <para/>
        /// This is different from <see cref="IsUpper(string, int)"/>!
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N TODO: Tests
        public static bool IsUUpper(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUUpper((int)s[index]);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.White_Space"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.White_Space"/>.
        /// <para/>
        /// This is different from <see cref="IsWhiteSpace(int)"/>!
        /// </summary>
        /// <param name="ch">Codepoint to be tested.</param>
        /// <stable>ICU 2.6</stable>
        public static bool IsUWhiteSpace(int ch)
        {
            return HasBinaryProperty(ch, UProperty.White_Space);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.White_Space"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.White_Space"/>.
        /// <para/>
        /// This is different from <see cref="IsWhiteSpace(int)"/>!
        /// </summary>
        /// <param name="c"><see cref="char"/> to be tested.</param>
        /// <draft>ICU4N 60.1</draft>
        public static bool IsUWhiteSpace(char c) // ICU4N specific overload to mimic System.Char
        {
            return IsUWhiteSpace((int)c);
        }

        /// <icu/>
        /// <summary>
        /// Check if a code point has the <see cref="UProperty.White_Space"/> Unicode property.
        /// <para/>
        /// Same as <see cref="HasBinaryProperty(int, UProperty)"/> with <see cref="UProperty.White_Space"/>.
        /// <para/>
        /// This is different from <see cref="IsWhiteSpace(int)"/>!
        /// </summary>
        /// <param name="s">A string.</param>
        /// <param name="index"></param>
        /// <draft>ICU4N 60.1</draft>
        public static bool IsUWhiteSpace(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsUWhiteSpace((int)s[index]);
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
        public static int GetIntPropertyValue(int ch, UProperty type)
        {
            return UCharacterProperty.Instance.GetIntPropertyValue(ch, (int)type);
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
                return GetPropertyValueName(propertyEnum, GetIntPropertyValue(codepoint, propertyEnum),
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
                case UProperty.Bidi_Mirroring_Glyph: return ConvertFromUtf32(GetMirror(codepoint));
                case UProperty.Case_Folding: return ConvertFromUtf32(FoldCase(codepoint, true));
                case UProperty.Lowercase_Mapping: return ConvertFromUtf32(ToLower(codepoint));
                case UProperty.Name: return GetName(codepoint);
                case UProperty.Simple_Case_Folding: return ConvertFromUtf32(FoldCase(codepoint, true));
                case UProperty.Simple_Lowercase_Mapping: return ConvertFromUtf32(ToLower(codepoint));
                case UProperty.Simple_Titlecase_Mapping: return ConvertFromUtf32(ToTitleCase(codepoint));
                case UProperty.Simple_Uppercase_Mapping: return ConvertFromUtf32(ToUpper(codepoint));
                case UProperty.Titlecase_Mapping: return ConvertFromUtf32(ToTitleCase(codepoint));
                case UProperty.Unicode_1_Name: return GetName1_0(codepoint);
                case UProperty.Uppercase_Mapping: return ConvertFromUtf32(ToUpper(codepoint));
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
        /// Minimum value returned by <see cref="GetIntPropertyValue(int, UProperty)"/>
        /// for a Unicode property. 0 if the property
        /// selector 'type' is out of range.
        /// </returns>
        /// <seealso cref="UProperty"/>
        /// <seealso cref="HasBinaryProperty(int, UProperty)"/>
        /// <seealso cref="UnicodeVersion"/>
        /// <seealso cref="GetIntPropertyMaxValue(UProperty)"/>
        /// <seealso cref="GetIntPropertyValue(int, UProperty)"/>
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
        ///         <see cref="UProperty.Bidi_Class"/>:    0/18
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
        /// Maximum value returned by <see cref="GetIntPropertyValue(int, UProperty)"/> for a Unicode
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
        /// Constant U+D800, same as <see cref="Character.MinHighSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinHighSurrogate = Character.MinHighSurrogate;

        /// <summary>
        /// Constant U+DBFF, same as <see cref="Character.MaxHighSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxHighSurrogate = Character.MaxHighSurrogate;

        /// <summary>
        /// Constant U+DC00, same as <see cref="Character.MinLowSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinLowSurrogate = Character.MinLowSurrogate;

        /// <summary>
        /// Constant U+DFFF, same as <see cref="Character.MaxLowSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxLowSurrogate = Character.MaxLowSurrogate;

        /// <summary>
        /// Constant U+D800, same as <see cref="Character.MinSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MinSurrogate = Character.MinSurrogate;

        /// <summary>
        /// Constant U+DFFF, same as <see cref="Character.MaxSurrogate"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const char MaxSurrogate = Character.MaxSurrogate;

        /// <summary>
        /// Constant U+10000, same as <see cref="Character.MinSupplementaryCodePoint"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MinSupplementaryCodePoint = Character.MinSupplementaryCodePoint;

        /// <summary>
        /// Constant U+10FFFF, same as <see cref="Character.MaxCodePoint"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MaxCodePoint = Character.MaxCodePoint;

        /// <summary>
        /// Constant U+0000, same as <see cref="Character.MinCodePoint"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int MinCodePoint = Character.MinCodePoint;

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
        /// Same as <see cref="Character.IsSupplementaryCodePoint(int)"/>.
        /// </summary>
        /// <param name="c">The <see cref="char"/> to check.</param>
        /// <returns>true if <paramref name="c"/> is a supplementary code point.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSupplementaryCodePoint(char c)
        {
            return IsSupplementaryCodePoint((int)c);
        }

        /// <summary>
        /// Same as <see cref="Character.IsSupplementaryCodePoint(int)"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the <see cref="char"/> in <paramref name="s"/> at <paramref name="index"/> is a supplementary code point.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSupplementaryCodePoint(string s, int index) // ICU4N specific overload to mimic System.Char
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (((uint)index) >= ((uint)s.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return IsSupplementaryCodePoint((int)s[index]);
        }

        /// <summary>
        /// Same as <see cref="Char.IsHighSurrogate(char)"/>.
        /// </summary>
        /// <param name="ch">The <see cref="char"/> to check.</param>
        /// <returns>true if <paramref name="ch"/> is a high (lead) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsHighSurrogate(char ch)
        {
            return char.IsHighSurrogate(ch);
        }

        /// <summary>
        /// Same as <see cref="Char.IsHighSurrogate(string, int)"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the character in <paramref name="s"/> at <paramref name="index"/> is a high (lead) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsHighSurrogate(string s, int index) // ICU4N specific overload to cover System.Char
        {
            return char.IsHighSurrogate(s, index);
        }

        /// <summary>
        /// Same as <see cref="Char.IsLowSurrogate(char)"/>.
        /// </summary>
        /// <param name="ch">The <see cref="char"/> to check.</param>
        /// <returns>true if <paramref name="ch"/> is a low (trail) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsLowSurrogate(char ch)
        {
            return char.IsLowSurrogate(ch);
        }

        /// <summary>
        /// Same as <see cref="Char.IsLowSurrogate(string, int)"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The position of the character to evaluate in <paramref name="s"/>.</param>
        /// <returns>true if the character in <paramref name="s"/> at <paramref name="index"/> is a low (trail) surrogate.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsLowSurrogate(string s, int index) // ICU4N specific overload to cover System.Char
        {
            return char.IsLowSurrogate(s, index);
        }

        /// <summary>
        /// Same as <see cref="Char.IsSurrogatePair(char, char)"/>.
        /// </summary>
        /// <param name="high">The high (lead) <see cref="char"/>.</param>
        /// <param name="low">The low (trail) <see cref="char"/>.</param>
        /// <returns>true if <paramref name="high"/>, <paramref name="low"/> form a surrogate pair.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSurrogatePair(char high, char low)
        {
            return char.IsSurrogatePair(high, low);
        }

        /// <summary>
        /// Same as <see cref="Char.IsSurrogatePair(string, int)"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/>.</param>
        /// <param name="index">The starting position of the pair of characters to evaluate within <paramref name="s"/>.</param>
        /// <returns><c>true</c> if the <paramref name="s"/> parameter includes adjacent characters at positions index and <paramref name="index"/> + 1, and the 
        /// numeric value of the character at position <paramref name="index"/> ranges from U+D800 through U+DBFF, and the numeric 
        /// value of the character at position <paramref name="index"/> + 1 ranges from U+DC00 through U+DFFF; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 3.0</stable>
        public static bool IsSurrogatePair(string s, int index) // ICU4N specific overload to cover System.Char
        {
            return char.IsSurrogatePair(s, index);
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