using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Lang
{
    public sealed partial class UCharacter
    {
        // ICU4N specific - copy UNASSIGNED from UCharacterEnums.ECharacterCategory (since we cannot inherit via interface)
        /**
         * Unassigned character type
         * @stable ICU 2.1
         */
        public const byte UNASSIGNED = 0;

        // public inner classes ----------------------------------------------

        /**
         * {@icuenhanced java.lang.Character.UnicodeBlock}.{@icu _usage_}
         *
         * A family of character subsets representing the character blocks in the
         * Unicode specification, generated from Unicode Data file Blocks.txt.
         * Character blocks generally define characters used for a specific script
         * or purpose. A character is contained by at most one Unicode block.
         *
         * {@icunote} All fields named XXX_ID are specific to ICU.
         *
         * @stable ICU 2.4
         */
        public sealed class UnicodeBlock //extends Character.Subset // ICU4N TODO: Base class
        {
            // block id corresponding to icu4c -----------------------------------

            /**
             * @stable ICU 2.4
             */
            public static readonly int INVALID_CODE_ID = -1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BASIC_LATIN_ID = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LATIN_1_SUPPLEMENT_ID = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LATIN_EXTENDED_A_ID = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LATIN_EXTENDED_B_ID = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int IPA_EXTENSIONS_ID = 5;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SPACING_MODIFIER_LETTERS_ID = 6;
            /**
             * @stable ICU 2.4
             */
            public static readonly int COMBINING_DIACRITICAL_MARKS_ID = 7;
            /**
             * Unicode 3.2 renames this block to "Greek and Coptic".
             * @stable ICU 2.4
             */
            public static readonly int GREEK_ID = 8;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CYRILLIC_ID = 9;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ARMENIAN_ID = 10;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HEBREW_ID = 11;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ARABIC_ID = 12;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SYRIAC_ID = 13;
            /**
             * @stable ICU 2.4
             */
            public static readonly int THAANA_ID = 14;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DEVANAGARI_ID = 15;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BENGALI_ID = 16;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GURMUKHI_ID = 17;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GUJARATI_ID = 18;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ORIYA_ID = 19;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAMIL_ID = 20;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TELUGU_ID = 21;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KANNADA_ID = 22;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MALAYALAM_ID = 23;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SINHALA_ID = 24;
            /**
             * @stable ICU 2.4
             */
            public static readonly int THAI_ID = 25;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LAO_ID = 26;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TIBETAN_ID = 27;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MYANMAR_ID = 28;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GEORGIAN_ID = 29;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HANGUL_JAMO_ID = 30;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ETHIOPIC_ID = 31;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CHEROKEE_ID = 32;
            /**
             * @stable ICU 2.4
             */
            public static readonly int UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_ID = 33;
            /**
             * @stable ICU 2.4
             */
            public static readonly int OGHAM_ID = 34;
            /**
             * @stable ICU 2.4
             */
            public static readonly int RUNIC_ID = 35;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KHMER_ID = 36;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MONGOLIAN_ID = 37;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LATIN_EXTENDED_ADDITIONAL_ID = 38;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GREEK_EXTENDED_ID = 39;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GENERAL_PUNCTUATION_ID = 40;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPERSCRIPTS_AND_SUBSCRIPTS_ID = 41;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CURRENCY_SYMBOLS_ID = 42;
            /**
             * Unicode 3.2 renames this block to "Combining Diacritical Marks for
             * Symbols".
             * @stable ICU 2.4
             */
            public static readonly int COMBINING_MARKS_FOR_SYMBOLS_ID = 43;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LETTERLIKE_SYMBOLS_ID = 44;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NUMBER_FORMS_ID = 45;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ARROWS_ID = 46;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MATHEMATICAL_OPERATORS_ID = 47;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MISCELLANEOUS_TECHNICAL_ID = 48;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CONTROL_PICTURES_ID = 49;
            /**
             * @stable ICU 2.4
             */
            public static readonly int OPTICAL_CHARACTER_RECOGNITION_ID = 50;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ENCLOSED_ALPHANUMERICS_ID = 51;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BOX_DRAWING_ID = 52;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BLOCK_ELEMENTS_ID = 53;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GEOMETRIC_SHAPES_ID = 54;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MISCELLANEOUS_SYMBOLS_ID = 55;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DINGBATS_ID = 56;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BRAILLE_PATTERNS_ID = 57;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_RADICALS_SUPPLEMENT_ID = 58;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KANGXI_RADICALS_ID = 59;
            /**
             * @stable ICU 2.4
             */
            public static readonly int IDEOGRAPHIC_DESCRIPTION_CHARACTERS_ID = 60;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_SYMBOLS_AND_PUNCTUATION_ID = 61;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HIRAGANA_ID = 62;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KATAKANA_ID = 63;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BOPOMOFO_ID = 64;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HANGUL_COMPATIBILITY_JAMO_ID = 65;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KANBUN_ID = 66;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BOPOMOFO_EXTENDED_ID = 67;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ENCLOSED_CJK_LETTERS_AND_MONTHS_ID = 68;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_COMPATIBILITY_ID = 69;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A_ID = 70;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_ID = 71;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YI_SYLLABLES_ID = 72;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YI_RADICALS_ID = 73;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HANGUL_SYLLABLES_ID = 74;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HIGH_SURROGATES_ID = 75;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HIGH_PRIVATE_USE_SURROGATES_ID = 76;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LOW_SURROGATES_ID = 77;
            /**
             * Same as public static readonly int PRIVATE_USE.
             * Until Unicode 3.1.1; the corresponding block name was "Private Use";
             * and multiple code point ranges had this block.
             * Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
             * and adds separate blocks for the supplementary PUAs.
             * @stable ICU 2.4
             */
            public static readonly int PRIVATE_USE_AREA_ID = 78;
            /**
             * Same as public static readonly int PRIVATE_USE_AREA.
             * Until Unicode 3.1.1; the corresponding block name was "Private Use";
             * and multiple code point ranges had this block.
             * Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
             * and adds separate blocks for the supplementary PUAs.
             * @stable ICU 2.4
             */
            public static readonly int PRIVATE_USE_ID = PRIVATE_USE_AREA_ID;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_COMPATIBILITY_IDEOGRAPHS_ID = 79;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ALPHABETIC_PRESENTATION_FORMS_ID = 80;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ARABIC_PRESENTATION_FORMS_A_ID = 81;
            /**
             * @stable ICU 2.4
             */
            public static readonly int COMBINING_HALF_MARKS_ID = 82;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_COMPATIBILITY_FORMS_ID = 83;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SMALL_FORM_VARIANTS_ID = 84;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ARABIC_PRESENTATION_FORMS_B_ID = 85;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SPECIALS_ID = 86;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HALFWIDTH_AND_FULLWIDTH_FORMS_ID = 87;
            /**
             * @stable ICU 2.4
             */
            public static readonly int OLD_ITALIC_ID = 88;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GOTHIC_ID = 89;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DESERET_ID = 90;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BYZANTINE_MUSICAL_SYMBOLS_ID = 91;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MUSICAL_SYMBOLS_ID = 92;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MATHEMATICAL_ALPHANUMERIC_SYMBOLS_ID = 93;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B_ID = 94;
            /**
             * @stable ICU 2.4
             */
            public static readonly int
            CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT_ID = 95;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAGS_ID = 96;

            // New blocks in Unicode 3.2

            /**
             * Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
             * @stable ICU 2.4
             */
            public static readonly int CYRILLIC_SUPPLEMENTARY_ID = 97;
            /**
             * Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
             * @stable ICU 3.0
             */

            public static readonly int CYRILLIC_SUPPLEMENT_ID = 97;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAGALOG_ID = 98;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HANUNOO_ID = 99;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BUHID_ID = 100;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAGBANWA_ID = 101;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A_ID = 102;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPPLEMENTAL_ARROWS_A_ID = 103;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPPLEMENTAL_ARROWS_B_ID = 104;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B_ID = 105;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPPLEMENTAL_MATHEMATICAL_OPERATORS_ID = 106;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KATAKANA_PHONETIC_EXTENSIONS_ID = 107;
            /**
             * @stable ICU 2.4
             */
            public static readonly int VARIATION_SELECTORS_ID = 108;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPPLEMENTARY_PRIVATE_USE_AREA_A_ID = 109;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPPLEMENTARY_PRIVATE_USE_AREA_B_ID = 110;

            /**
             * @stable ICU 2.6
             */
            public static readonly int LIMBU_ID = 111; /*[1900]*/
                                                       /**
                                                        * @stable ICU 2.6
                                                        */
            public static readonly int TAI_LE_ID = 112; /*[1950]*/
                                                        /**
                                                         * @stable ICU 2.6
                                                         */
            public static readonly int KHMER_SYMBOLS_ID = 113; /*[19E0]*/
                                                               /**
                                                                * @stable ICU 2.6
                                                                */
            public static readonly int PHONETIC_EXTENSIONS_ID = 114; /*[1D00]*/
                                                                     /**
                                                                      * @stable ICU 2.6
                                                                      */
            public static readonly int MISCELLANEOUS_SYMBOLS_AND_ARROWS_ID = 115; /*[2B00]*/
                                                                                  /**
                                                                                   * @stable ICU 2.6
                                                                                   */
            public static readonly int YIJING_HEXAGRAM_SYMBOLS_ID = 116; /*[4DC0]*/
                                                                         /**
                                                                          * @stable ICU 2.6
                                                                          */
            public static readonly int LINEAR_B_SYLLABARY_ID = 117; /*[10000]*/
                                                                    /**
                                                                     * @stable ICU 2.6
                                                                     */
            public static readonly int LINEAR_B_IDEOGRAMS_ID = 118; /*[10080]*/
                                                                    /**
                                                                     * @stable ICU 2.6
                                                                     */
            public static readonly int AEGEAN_NUMBERS_ID = 119; /*[10100]*/
                                                                /**
                                                                 * @stable ICU 2.6
                                                                 */
            public static readonly int UGARITIC_ID = 120; /*[10380]*/
                                                          /**
                                                           * @stable ICU 2.6
                                                           */
            public static readonly int SHAVIAN_ID = 121; /*[10450]*/
                                                         /**
                                                          * @stable ICU 2.6
                                                          */
            public static readonly int OSMANYA_ID = 122; /*[10480]*/
                                                         /**
                                                          * @stable ICU 2.6
                                                          */
            public static readonly int CYPRIOT_SYLLABARY_ID = 123; /*[10800]*/
                                                                   /**
                                                                    * @stable ICU 2.6
                                                                    */
            public static readonly int TAI_XUAN_JING_SYMBOLS_ID = 124; /*[1D300]*/
                                                                       /**
                                                                        * @stable ICU 2.6
                                                                        */
            public static readonly int VARIATION_SELECTORS_SUPPLEMENT_ID = 125; /*[E0100]*/

            /* New blocks in Unicode 4.1 */

            /**
             * @stable ICU 3.4
             */
            public static readonly int ANCIENT_GREEK_MUSICAL_NOTATION_ID = 126; /*[1D200]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int ANCIENT_GREEK_NUMBERS_ID = 127; /*[10140]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int ARABIC_SUPPLEMENT_ID = 128; /*[0750]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int BUGINESE_ID = 129; /*[1A00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int CJK_STROKES_ID = 130; /*[31C0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int COMBINING_DIACRITICAL_MARKS_SUPPLEMENT_ID = 131; /*[1DC0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int COPTIC_ID = 132; /*[2C80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int ETHIOPIC_EXTENDED_ID = 133; /*[2D80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int ETHIOPIC_SUPPLEMENT_ID = 134; /*[1380]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int GEORGIAN_SUPPLEMENT_ID = 135; /*[2D00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int GLAGOLITIC_ID = 136; /*[2C00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int KHAROSHTHI_ID = 137; /*[10A00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int MODIFIER_TONE_LETTERS_ID = 138; /*[A700]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int NEW_TAI_LUE_ID = 139; /*[1980]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int OLD_PERSIAN_ID = 140; /*[103A0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int PHONETIC_EXTENSIONS_SUPPLEMENT_ID = 141; /*[1D80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int SUPPLEMENTAL_PUNCTUATION_ID = 142; /*[2E00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int SYLOTI_NAGRI_ID = 143; /*[A800]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int TIFINAGH_ID = 144; /*[2D30]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly int VERTICAL_FORMS_ID = 145; /*[FE10]*/

            /* New blocks in Unicode 5.0 */

            /**
             * @stable ICU 3.6
             */
            public static readonly int NKO_ID = 146; /*[07C0]*/
                                                     /**
                                                      * @stable ICU 3.6
                                                      */
            public static readonly int BALINESE_ID = 147; /*[1B00]*/
                                                          /**
                                                           * @stable ICU 3.6
                                                           */
            public static readonly int LATIN_EXTENDED_C_ID = 148; /*[2C60]*/
                                                                  /**
                                                                   * @stable ICU 3.6
                                                                   */
            public static readonly int LATIN_EXTENDED_D_ID = 149; /*[A720]*/
                                                                  /**
                                                                   * @stable ICU 3.6
                                                                   */
            public static readonly int PHAGS_PA_ID = 150; /*[A840]*/
                                                          /**
                                                           * @stable ICU 3.6
                                                           */
            public static readonly int PHOENICIAN_ID = 151; /*[10900]*/
                                                            /**
                                                             * @stable ICU 3.6
                                                             */
            public static readonly int CUNEIFORM_ID = 152; /*[12000]*/
                                                           /**
                                                            * @stable ICU 3.6
                                                            */
            public static readonly int CUNEIFORM_NUMBERS_AND_PUNCTUATION_ID = 153; /*[12400]*/
                                                                                   /**
                                                                                    * @stable ICU 3.6
                                                                                    */
            public static readonly int COUNTING_ROD_NUMERALS_ID = 154; /*[1D360]*/

            /**
             * @stable ICU 4.0
             */
            public static readonly int SUNDANESE_ID = 155; /* [1B80] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int LEPCHA_ID = 156; /* [1C00] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int OL_CHIKI_ID = 157; /* [1C50] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int CYRILLIC_EXTENDED_A_ID = 158; /* [2DE0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int VAI_ID = 159; /* [A500] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int CYRILLIC_EXTENDED_B_ID = 160; /* [A640] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int SAURASHTRA_ID = 161; /* [A880] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int KAYAH_LI_ID = 162; /* [A900] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int REJANG_ID = 163; /* [A930] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int CHAM_ID = 164; /* [AA00] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int ANCIENT_SYMBOLS_ID = 165; /* [10190] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int PHAISTOS_DISC_ID = 166; /* [101D0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int LYCIAN_ID = 167; /* [10280] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int CARIAN_ID = 168; /* [102A0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int LYDIAN_ID = 169; /* [10920] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int MAHJONG_TILES_ID = 170; /* [1F000] */

            /**
             * @stable ICU 4.0
             */
            public static readonly int DOMINO_TILES_ID = 171; /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /** @stable ICU 4.4 */
            public static readonly int SAMARITAN_ID = 172; /*[0800]*/
                                                           /** @stable ICU 4.4 */
            public static readonly int UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED_ID = 173; /*[18B0]*/
                                                                                                /** @stable ICU 4.4 */
            public static readonly int TAI_THAM_ID = 174; /*[1A20]*/
                                                          /** @stable ICU 4.4 */
            public static readonly int VEDIC_EXTENSIONS_ID = 175; /*[1CD0]*/
                                                                  /** @stable ICU 4.4 */
            public static readonly int LISU_ID = 176; /*[A4D0]*/
                                                      /** @stable ICU 4.4 */
            public static readonly int BAMUM_ID = 177; /*[A6A0]*/
                                                       /** @stable ICU 4.4 */
            public static readonly int COMMON_INDIC_NUMBER_FORMS_ID = 178; /*[A830]*/
                                                                           /** @stable ICU 4.4 */
            public static readonly int DEVANAGARI_EXTENDED_ID = 179; /*[A8E0]*/
                                                                     /** @stable ICU 4.4 */
            public static readonly int HANGUL_JAMO_EXTENDED_A_ID = 180; /*[A960]*/
                                                                        /** @stable ICU 4.4 */
            public static readonly int JAVANESE_ID = 181; /*[A980]*/
                                                          /** @stable ICU 4.4 */
            public static readonly int MYANMAR_EXTENDED_A_ID = 182; /*[AA60]*/
                                                                    /** @stable ICU 4.4 */
            public static readonly int TAI_VIET_ID = 183; /*[AA80]*/
                                                          /** @stable ICU 4.4 */
            public static readonly int MEETEI_MAYEK_ID = 184; /*[ABC0]*/
                                                              /** @stable ICU 4.4 */
            public static readonly int HANGUL_JAMO_EXTENDED_B_ID = 185; /*[D7B0]*/
                                                                        /** @stable ICU 4.4 */
            public static readonly int IMPERIAL_ARAMAIC_ID = 186; /*[10840]*/
                                                                  /** @stable ICU 4.4 */
            public static readonly int OLD_SOUTH_ARABIAN_ID = 187; /*[10A60]*/
                                                                   /** @stable ICU 4.4 */
            public static readonly int AVESTAN_ID = 188; /*[10B00]*/
                                                         /** @stable ICU 4.4 */
            public static readonly int INSCRIPTIONAL_PARTHIAN_ID = 189; /*[10B40]*/
                                                                        /** @stable ICU 4.4 */
            public static readonly int INSCRIPTIONAL_PAHLAVI_ID = 190; /*[10B60]*/
                                                                       /** @stable ICU 4.4 */
            public static readonly int OLD_TURKIC_ID = 191; /*[10C00]*/
                                                            /** @stable ICU 4.4 */
            public static readonly int RUMI_NUMERAL_SYMBOLS_ID = 192; /*[10E60]*/
                                                                      /** @stable ICU 4.4 */
            public static readonly int KAITHI_ID = 193; /*[11080]*/
                                                        /** @stable ICU 4.4 */
            public static readonly int EGYPTIAN_HIEROGLYPHS_ID = 194; /*[13000]*/
                                                                      /** @stable ICU 4.4 */
            public static readonly int ENCLOSED_ALPHANUMERIC_SUPPLEMENT_ID = 195; /*[1F100]*/
                                                                                  /** @stable ICU 4.4 */
            public static readonly int ENCLOSED_IDEOGRAPHIC_SUPPLEMENT_ID = 196; /*[1F200]*/
                                                                                 /** @stable ICU 4.4 */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C_ID = 197; /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /** @stable ICU 4.6 */
            public static readonly int MANDAIC_ID = 198; /*[0840]*/
                                                         /** @stable ICU 4.6 */
            public static readonly int BATAK_ID = 199; /*[1BC0]*/
                                                       /** @stable ICU 4.6 */
            public static readonly int ETHIOPIC_EXTENDED_A_ID = 200; /*[AB00]*/
                                                                     /** @stable ICU 4.6 */
            public static readonly int BRAHMI_ID = 201; /*[11000]*/
                                                        /** @stable ICU 4.6 */
            public static readonly int BAMUM_SUPPLEMENT_ID = 202; /*[16800]*/
                                                                  /** @stable ICU 4.6 */
            public static readonly int KANA_SUPPLEMENT_ID = 203; /*[1B000]*/
                                                                 /** @stable ICU 4.6 */
            public static readonly int PLAYING_CARDS_ID = 204; /*[1F0A0]*/
                                                               /** @stable ICU 4.6 */
            public static readonly int MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS_ID = 205; /*[1F300]*/
                                                                                       /** @stable ICU 4.6 */
            public static readonly int EMOTICONS_ID = 206; /*[1F600]*/
                                                           /** @stable ICU 4.6 */
            public static readonly int TRANSPORT_AND_MAP_SYMBOLS_ID = 207; /*[1F680]*/
                                                                           /** @stable ICU 4.6 */
            public static readonly int ALCHEMICAL_SYMBOLS_ID = 208; /*[1F700]*/
                                                                    /** @stable ICU 4.6 */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D_ID = 209; /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /** @stable ICU 49 */
            public static readonly int ARABIC_EXTENDED_A_ID = 210; /*[08A0]*/
                                                                   /** @stable ICU 49 */
            public static readonly int ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS_ID = 211; /*[1EE00]*/
                                                                                        /** @stable ICU 49 */
            public static readonly int CHAKMA_ID = 212; /*[11100]*/
                                                        /** @stable ICU 49 */
            public static readonly int MEETEI_MAYEK_EXTENSIONS_ID = 213; /*[AAE0]*/
                                                                         /** @stable ICU 49 */
            public static readonly int MEROITIC_CURSIVE_ID = 214; /*[109A0]*/
                                                                  /** @stable ICU 49 */
            public static readonly int MEROITIC_HIEROGLYPHS_ID = 215; /*[10980]*/
                                                                      /** @stable ICU 49 */
            public static readonly int MIAO_ID = 216; /*[16F00]*/
                                                      /** @stable ICU 49 */
            public static readonly int SHARADA_ID = 217; /*[11180]*/
                                                         /** @stable ICU 49 */
            public static readonly int SORA_SOMPENG_ID = 218; /*[110D0]*/
                                                              /** @stable ICU 49 */
            public static readonly int SUNDANESE_SUPPLEMENT_ID = 219; /*[1CC0]*/
                                                                      /** @stable ICU 49 */
            public static readonly int TAKRI_ID = 220; /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /** @stable ICU 54 */
            public static readonly int BASSA_VAH_ID = 221; /*[16AD0]*/
                                                           /** @stable ICU 54 */
            public static readonly int CAUCASIAN_ALBANIAN_ID = 222; /*[10530]*/
                                                                    /** @stable ICU 54 */
            public static readonly int COPTIC_EPACT_NUMBERS_ID = 223; /*[102E0]*/
                                                                      /** @stable ICU 54 */
            public static readonly int COMBINING_DIACRITICAL_MARKS_EXTENDED_ID = 224; /*[1AB0]*/
                                                                                      /** @stable ICU 54 */
            public static readonly int DUPLOYAN_ID = 225; /*[1BC00]*/
                                                          /** @stable ICU 54 */
            public static readonly int ELBASAN_ID = 226; /*[10500]*/
                                                         /** @stable ICU 54 */
            public static readonly int GEOMETRIC_SHAPES_EXTENDED_ID = 227; /*[1F780]*/
                                                                           /** @stable ICU 54 */
            public static readonly int GRANTHA_ID = 228; /*[11300]*/
                                                         /** @stable ICU 54 */
            public static readonly int KHOJKI_ID = 229; /*[11200]*/
                                                        /** @stable ICU 54 */
            public static readonly int KHUDAWADI_ID = 230; /*[112B0]*/
                                                           /** @stable ICU 54 */
            public static readonly int LATIN_EXTENDED_E_ID = 231; /*[AB30]*/
                                                                  /** @stable ICU 54 */
            public static readonly int LINEAR_A_ID = 232; /*[10600]*/
                                                          /** @stable ICU 54 */
            public static readonly int MAHAJANI_ID = 233; /*[11150]*/
                                                          /** @stable ICU 54 */
            public static readonly int MANICHAEAN_ID = 234; /*[10AC0]*/
                                                            /** @stable ICU 54 */
            public static readonly int MENDE_KIKAKUI_ID = 235; /*[1E800]*/
                                                               /** @stable ICU 54 */
            public static readonly int MODI_ID = 236; /*[11600]*/
                                                      /** @stable ICU 54 */
            public static readonly int MRO_ID = 237; /*[16A40]*/
                                                     /** @stable ICU 54 */
            public static readonly int MYANMAR_EXTENDED_B_ID = 238; /*[A9E0]*/
                                                                    /** @stable ICU 54 */
            public static readonly int NABATAEAN_ID = 239; /*[10880]*/
                                                           /** @stable ICU 54 */
            public static readonly int OLD_NORTH_ARABIAN_ID = 240; /*[10A80]*/
                                                                   /** @stable ICU 54 */
            public static readonly int OLD_PERMIC_ID = 241; /*[10350]*/
                                                            /** @stable ICU 54 */
            public static readonly int ORNAMENTAL_DINGBATS_ID = 242; /*[1F650]*/
                                                                     /** @stable ICU 54 */
            public static readonly int PAHAWH_HMONG_ID = 243; /*[16B00]*/
                                                              /** @stable ICU 54 */
            public static readonly int PALMYRENE_ID = 244; /*[10860]*/
                                                           /** @stable ICU 54 */
            public static readonly int PAU_CIN_HAU_ID = 245; /*[11AC0]*/
                                                             /** @stable ICU 54 */
            public static readonly int PSALTER_PAHLAVI_ID = 246; /*[10B80]*/
                                                                 /** @stable ICU 54 */
            public static readonly int SHORTHAND_FORMAT_CONTROLS_ID = 247; /*[1BCA0]*/
                                                                           /** @stable ICU 54 */
            public static readonly int SIDDHAM_ID = 248; /*[11580]*/
                                                         /** @stable ICU 54 */
            public static readonly int SINHALA_ARCHAIC_NUMBERS_ID = 249; /*[111E0]*/
                                                                         /** @stable ICU 54 */
            public static readonly int SUPPLEMENTAL_ARROWS_C_ID = 250; /*[1F800]*/
                                                                       /** @stable ICU 54 */
            public static readonly int TIRHUTA_ID = 251; /*[11480]*/
                                                         /** @stable ICU 54 */
            public static readonly int WARANG_CITI_ID = 252; /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /** @stable ICU 56 */
            public static readonly int AHOM_ID = 253; /*[11700]*/
                                                      /** @stable ICU 56 */
            public static readonly int ANATOLIAN_HIEROGLYPHS_ID = 254; /*[14400]*/
                                                                       /** @stable ICU 56 */
            public static readonly int CHEROKEE_SUPPLEMENT_ID = 255; /*[AB70]*/
                                                                     /** @stable ICU 56 */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E_ID = 256; /*[2B820]*/
                                                                                    /** @stable ICU 56 */
            public static readonly int EARLY_DYNASTIC_CUNEIFORM_ID = 257; /*[12480]*/
                                                                          /** @stable ICU 56 */
            public static readonly int HATRAN_ID = 258; /*[108E0]*/
                                                        /** @stable ICU 56 */
            public static readonly int MULTANI_ID = 259; /*[11280]*/
                                                         /** @stable ICU 56 */
            public static readonly int OLD_HUNGARIAN_ID = 260; /*[10C80]*/
                                                               /** @stable ICU 56 */
            public static readonly int SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS_ID = 261; /*[1F900]*/
                                                                                      /** @stable ICU 56 */
            public static readonly int SUTTON_SIGNWRITING_ID = 262; /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /** @stable ICU 58 */
            public static readonly int ADLAM_ID = 263; /*[1E900]*/
                                                       /** @stable ICU 58 */
            public static readonly int BHAIKSUKI_ID = 264; /*[11C00]*/
                                                           /** @stable ICU 58 */
            public static readonly int CYRILLIC_EXTENDED_C_ID = 265; /*[1C80]*/
                                                                     /** @stable ICU 58 */
            public static readonly int GLAGOLITIC_SUPPLEMENT_ID = 266; /*[1E000]*/
                                                                       /** @stable ICU 58 */
            public static readonly int IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION_ID = 267; /*[16FE0]*/
                                                                                     /** @stable ICU 58 */
            public static readonly int MARCHEN_ID = 268; /*[11C70]*/
                                                         /** @stable ICU 58 */
            public static readonly int MONGOLIAN_SUPPLEMENT_ID = 269; /*[11660]*/
                                                                      /** @stable ICU 58 */
            public static readonly int NEWA_ID = 270; /*[11400]*/
                                                      /** @stable ICU 58 */
            public static readonly int OSAGE_ID = 271; /*[104B0]*/
                                                       /** @stable ICU 58 */
            public static readonly int TANGUT_ID = 272; /*[17000]*/
                                                        /** @stable ICU 58 */
            public static readonly int TANGUT_COMPONENTS_ID = 273; /*[18800]*/

            // New blocks in Unicode 10.0

            /** @stable ICU 60 */
            public static readonly int CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F_ID = 274; /*[2CEB0]*/
                                                                                    /** @stable ICU 60 */
            public static readonly int KANA_EXTENDED_A_ID = 275; /*[1B100]*/
                                                                 /** @stable ICU 60 */
            public static readonly int MASARAM_GONDI_ID = 276; /*[11D00]*/
                                                               /** @stable ICU 60 */
            public static readonly int NUSHU_ID = 277; /*[1B170]*/
                                                       /** @stable ICU 60 */
            public static readonly int SOYOMBO_ID = 278; /*[11A50]*/
                                                         /** @stable ICU 60 */
            public static readonly int SYRIAC_SUPPLEMENT_ID = 279; /*[0860]*/
                                                                   /** @stable ICU 60 */
            public static readonly int ZANABAZAR_SQUARE_ID = 280; /*[11A00]*/

            /**
             * One more than the highest normal UnicodeBlock value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.BLOCK).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 281;

            // blocks objects ---------------------------------------------------

            /**
             * Array of UnicodeBlocks, for easy access in getInstance(int)
             */
            private readonly static UnicodeBlock[] BLOCKS_ = new UnicodeBlock[COUNT];

            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock NO_BLOCK
            = new UnicodeBlock("NO_BLOCK", 0);

            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BASIC_LATIN
            = new UnicodeBlock("BASIC_LATIN", BASIC_LATIN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LATIN_1_SUPPLEMENT
            = new UnicodeBlock("LATIN_1_SUPPLEMENT", LATIN_1_SUPPLEMENT_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LATIN_EXTENDED_A
            = new UnicodeBlock("LATIN_EXTENDED_A", LATIN_EXTENDED_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LATIN_EXTENDED_B
            = new UnicodeBlock("LATIN_EXTENDED_B", LATIN_EXTENDED_B_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock IPA_EXTENSIONS
            = new UnicodeBlock("IPA_EXTENSIONS", IPA_EXTENSIONS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SPACING_MODIFIER_LETTERS
            = new UnicodeBlock("SPACING_MODIFIER_LETTERS", SPACING_MODIFIER_LETTERS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS
            = new UnicodeBlock("COMBINING_DIACRITICAL_MARKS", COMBINING_DIACRITICAL_MARKS_ID);
            /**
             * Unicode 3.2 renames this block to "Greek and Coptic".
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GREEK
            = new UnicodeBlock("GREEK", GREEK_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CYRILLIC
            = new UnicodeBlock("CYRILLIC", CYRILLIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ARMENIAN
            = new UnicodeBlock("ARMENIAN", ARMENIAN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HEBREW
            = new UnicodeBlock("HEBREW", HEBREW_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ARABIC
            = new UnicodeBlock("ARABIC", ARABIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SYRIAC
            = new UnicodeBlock("SYRIAC", SYRIAC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock THAANA
            = new UnicodeBlock("THAANA", THAANA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock DEVANAGARI
            = new UnicodeBlock("DEVANAGARI", DEVANAGARI_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BENGALI
            = new UnicodeBlock("BENGALI", BENGALI_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GURMUKHI
            = new UnicodeBlock("GURMUKHI", GURMUKHI_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GUJARATI
            = new UnicodeBlock("GUJARATI", GUJARATI_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ORIYA
            = new UnicodeBlock("ORIYA", ORIYA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TAMIL
            = new UnicodeBlock("TAMIL", TAMIL_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TELUGU
            = new UnicodeBlock("TELUGU", TELUGU_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KANNADA
            = new UnicodeBlock("KANNADA", KANNADA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MALAYALAM
            = new UnicodeBlock("MALAYALAM", MALAYALAM_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SINHALA
            = new UnicodeBlock("SINHALA", SINHALA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock THAI
            = new UnicodeBlock("THAI", THAI_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LAO
            = new UnicodeBlock("LAO", LAO_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TIBETAN
            = new UnicodeBlock("TIBETAN", TIBETAN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MYANMAR
            = new UnicodeBlock("MYANMAR", MYANMAR_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GEORGIAN
            = new UnicodeBlock("GEORGIAN", GEORGIAN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HANGUL_JAMO
            = new UnicodeBlock("HANGUL_JAMO", HANGUL_JAMO_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ETHIOPIC
            = new UnicodeBlock("ETHIOPIC", ETHIOPIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CHEROKEE
            = new UnicodeBlock("CHEROKEE", CHEROKEE_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS
            = new UnicodeBlock("UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS",
                    UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock OGHAM
            = new UnicodeBlock("OGHAM", OGHAM_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock RUNIC
            = new UnicodeBlock("RUNIC", RUNIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KHMER
            = new UnicodeBlock("KHMER", KHMER_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MONGOLIAN
            = new UnicodeBlock("MONGOLIAN", MONGOLIAN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LATIN_EXTENDED_ADDITIONAL
            = new UnicodeBlock("LATIN_EXTENDED_ADDITIONAL", LATIN_EXTENDED_ADDITIONAL_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GREEK_EXTENDED
            = new UnicodeBlock("GREEK_EXTENDED", GREEK_EXTENDED_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GENERAL_PUNCTUATION
            = new UnicodeBlock("GENERAL_PUNCTUATION", GENERAL_PUNCTUATION_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPERSCRIPTS_AND_SUBSCRIPTS
            = new UnicodeBlock("SUPERSCRIPTS_AND_SUBSCRIPTS", SUPERSCRIPTS_AND_SUBSCRIPTS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CURRENCY_SYMBOLS
            = new UnicodeBlock("CURRENCY_SYMBOLS", CURRENCY_SYMBOLS_ID);
            /**
             * Unicode 3.2 renames this block to "Combining Diacritical Marks for
             * Symbols".
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock COMBINING_MARKS_FOR_SYMBOLS
            = new UnicodeBlock("COMBINING_MARKS_FOR_SYMBOLS", COMBINING_MARKS_FOR_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LETTERLIKE_SYMBOLS
            = new UnicodeBlock("LETTERLIKE_SYMBOLS", LETTERLIKE_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock NUMBER_FORMS
            = new UnicodeBlock("NUMBER_FORMS", NUMBER_FORMS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ARROWS
            = new UnicodeBlock("ARROWS", ARROWS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MATHEMATICAL_OPERATORS
            = new UnicodeBlock("MATHEMATICAL_OPERATORS", MATHEMATICAL_OPERATORS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MISCELLANEOUS_TECHNICAL
            = new UnicodeBlock("MISCELLANEOUS_TECHNICAL", MISCELLANEOUS_TECHNICAL_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CONTROL_PICTURES
            = new UnicodeBlock("CONTROL_PICTURES", CONTROL_PICTURES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock OPTICAL_CHARACTER_RECOGNITION
            = new UnicodeBlock("OPTICAL_CHARACTER_RECOGNITION", OPTICAL_CHARACTER_RECOGNITION_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ENCLOSED_ALPHANUMERICS
            = new UnicodeBlock("ENCLOSED_ALPHANUMERICS", ENCLOSED_ALPHANUMERICS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BOX_DRAWING
            = new UnicodeBlock("BOX_DRAWING", BOX_DRAWING_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BLOCK_ELEMENTS
            = new UnicodeBlock("BLOCK_ELEMENTS", BLOCK_ELEMENTS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GEOMETRIC_SHAPES
            = new UnicodeBlock("GEOMETRIC_SHAPES", GEOMETRIC_SHAPES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS
            = new UnicodeBlock("MISCELLANEOUS_SYMBOLS", MISCELLANEOUS_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock DINGBATS
            = new UnicodeBlock("DINGBATS", DINGBATS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BRAILLE_PATTERNS
            = new UnicodeBlock("BRAILLE_PATTERNS", BRAILLE_PATTERNS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_RADICALS_SUPPLEMENT
            = new UnicodeBlock("CJK_RADICALS_SUPPLEMENT", CJK_RADICALS_SUPPLEMENT_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KANGXI_RADICALS
            = new UnicodeBlock("KANGXI_RADICALS", KANGXI_RADICALS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock IDEOGRAPHIC_DESCRIPTION_CHARACTERS
            = new UnicodeBlock("IDEOGRAPHIC_DESCRIPTION_CHARACTERS",
                    IDEOGRAPHIC_DESCRIPTION_CHARACTERS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_SYMBOLS_AND_PUNCTUATION
            = new UnicodeBlock("CJK_SYMBOLS_AND_PUNCTUATION", CJK_SYMBOLS_AND_PUNCTUATION_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HIRAGANA
            = new UnicodeBlock("HIRAGANA", HIRAGANA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KATAKANA
            = new UnicodeBlock("KATAKANA", KATAKANA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BOPOMOFO
            = new UnicodeBlock("BOPOMOFO", BOPOMOFO_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HANGUL_COMPATIBILITY_JAMO
            = new UnicodeBlock("HANGUL_COMPATIBILITY_JAMO", HANGUL_COMPATIBILITY_JAMO_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KANBUN
            = new UnicodeBlock("KANBUN", KANBUN_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BOPOMOFO_EXTENDED
            = new UnicodeBlock("BOPOMOFO_EXTENDED", BOPOMOFO_EXTENDED_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ENCLOSED_CJK_LETTERS_AND_MONTHS
            = new UnicodeBlock("ENCLOSED_CJK_LETTERS_AND_MONTHS",
                    ENCLOSED_CJK_LETTERS_AND_MONTHS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_COMPATIBILITY
            = new UnicodeBlock("CJK_COMPATIBILITY", CJK_COMPATIBILITY_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A
            = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A",
                    CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS
            = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS", CJK_UNIFIED_IDEOGRAPHS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock YI_SYLLABLES
            = new UnicodeBlock("YI_SYLLABLES", YI_SYLLABLES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock YI_RADICALS
            = new UnicodeBlock("YI_RADICALS", YI_RADICALS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HANGUL_SYLLABLES
            = new UnicodeBlock("HANGUL_SYLLABLES", HANGUL_SYLLABLES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HIGH_SURROGATES
            = new UnicodeBlock("HIGH_SURROGATES", HIGH_SURROGATES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HIGH_PRIVATE_USE_SURROGATES
            = new UnicodeBlock("HIGH_PRIVATE_USE_SURROGATES", HIGH_PRIVATE_USE_SURROGATES_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock LOW_SURROGATES
            = new UnicodeBlock("LOW_SURROGATES", LOW_SURROGATES_ID);
            /**
             * Same as public static readonly int PRIVATE_USE.
             * Until Unicode 3.1.1; the corresponding block name was "Private Use";
             * and multiple code point ranges had this block.
             * Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
             * and adds separate blocks for the supplementary PUAs.
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock PRIVATE_USE_AREA
            = new UnicodeBlock("PRIVATE_USE_AREA", 78);
            /**
             * Same as public static readonly int PRIVATE_USE_AREA.
             * Until Unicode 3.1.1; the corresponding block name was "Private Use";
             * and multiple code point ranges had this block.
             * Unicode 3.2 renames the block for the BMP PUA to "Private Use Area"
             * and adds separate blocks for the supplementary PUAs.
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock PRIVATE_USE
            = PRIVATE_USE_AREA;
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_COMPATIBILITY_IDEOGRAPHS
            = new UnicodeBlock("CJK_COMPATIBILITY_IDEOGRAPHS", CJK_COMPATIBILITY_IDEOGRAPHS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ALPHABETIC_PRESENTATION_FORMS
            = new UnicodeBlock("ALPHABETIC_PRESENTATION_FORMS", ALPHABETIC_PRESENTATION_FORMS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ARABIC_PRESENTATION_FORMS_A
            = new UnicodeBlock("ARABIC_PRESENTATION_FORMS_A", ARABIC_PRESENTATION_FORMS_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock COMBINING_HALF_MARKS
            = new UnicodeBlock("COMBINING_HALF_MARKS", COMBINING_HALF_MARKS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_COMPATIBILITY_FORMS
            = new UnicodeBlock("CJK_COMPATIBILITY_FORMS", CJK_COMPATIBILITY_FORMS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SMALL_FORM_VARIANTS
            = new UnicodeBlock("SMALL_FORM_VARIANTS", SMALL_FORM_VARIANTS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock ARABIC_PRESENTATION_FORMS_B
            = new UnicodeBlock("ARABIC_PRESENTATION_FORMS_B", ARABIC_PRESENTATION_FORMS_B_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SPECIALS
            = new UnicodeBlock("SPECIALS", SPECIALS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HALFWIDTH_AND_FULLWIDTH_FORMS
            = new UnicodeBlock("HALFWIDTH_AND_FULLWIDTH_FORMS", HALFWIDTH_AND_FULLWIDTH_FORMS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock OLD_ITALIC
            = new UnicodeBlock("OLD_ITALIC", OLD_ITALIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock GOTHIC
            = new UnicodeBlock("GOTHIC", GOTHIC_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock DESERET
            = new UnicodeBlock("DESERET", DESERET_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BYZANTINE_MUSICAL_SYMBOLS
            = new UnicodeBlock("BYZANTINE_MUSICAL_SYMBOLS", BYZANTINE_MUSICAL_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MUSICAL_SYMBOLS
            = new UnicodeBlock("MUSICAL_SYMBOLS", MUSICAL_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MATHEMATICAL_ALPHANUMERIC_SYMBOLS
            = new UnicodeBlock("MATHEMATICAL_ALPHANUMERIC_SYMBOLS",
                    MATHEMATICAL_ALPHANUMERIC_SYMBOLS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B
            = new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B",
                    CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock
            CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT
            = new UnicodeBlock("CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT",
                    CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TAGS
            = new UnicodeBlock("TAGS", TAGS_ID);

            // New blocks in Unicode 3.2

            /**
             * Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock CYRILLIC_SUPPLEMENTARY
            = new UnicodeBlock("CYRILLIC_SUPPLEMENTARY", CYRILLIC_SUPPLEMENTARY_ID);
            /**
             * Unicode 4.0.1 renames the "Cyrillic Supplementary" block to "Cyrillic Supplement".
             * @stable ICU 3.0
             */
            public static readonly UnicodeBlock CYRILLIC_SUPPLEMENT
            = new UnicodeBlock("CYRILLIC_SUPPLEMENT", CYRILLIC_SUPPLEMENT_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TAGALOG
            = new UnicodeBlock("TAGALOG", TAGALOG_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock HANUNOO
            = new UnicodeBlock("HANUNOO", HANUNOO_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock BUHID
            = new UnicodeBlock("BUHID", BUHID_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock TAGBANWA
            = new UnicodeBlock("TAGBANWA", TAGBANWA_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A
            = new UnicodeBlock("MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A",
                    MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_A
            = new UnicodeBlock("SUPPLEMENTAL_ARROWS_A", SUPPLEMENTAL_ARROWS_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_B
            = new UnicodeBlock("SUPPLEMENTAL_ARROWS_B", SUPPLEMENTAL_ARROWS_B_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B
            = new UnicodeBlock("MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B",
                    MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPPLEMENTAL_MATHEMATICAL_OPERATORS
            = new UnicodeBlock("SUPPLEMENTAL_MATHEMATICAL_OPERATORS",
                    SUPPLEMENTAL_MATHEMATICAL_OPERATORS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock KATAKANA_PHONETIC_EXTENSIONS
            = new UnicodeBlock("KATAKANA_PHONETIC_EXTENSIONS", KATAKANA_PHONETIC_EXTENSIONS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock VARIATION_SELECTORS
            = new UnicodeBlock("VARIATION_SELECTORS", VARIATION_SELECTORS_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPPLEMENTARY_PRIVATE_USE_AREA_A
            = new UnicodeBlock("SUPPLEMENTARY_PRIVATE_USE_AREA_A",
                    SUPPLEMENTARY_PRIVATE_USE_AREA_A_ID);
            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock SUPPLEMENTARY_PRIVATE_USE_AREA_B
            = new UnicodeBlock("SUPPLEMENTARY_PRIVATE_USE_AREA_B",
                    SUPPLEMENTARY_PRIVATE_USE_AREA_B_ID);

            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock LIMBU
            = new UnicodeBlock("LIMBU", LIMBU_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock TAI_LE
            = new UnicodeBlock("TAI_LE", TAI_LE_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock KHMER_SYMBOLS
            = new UnicodeBlock("KHMER_SYMBOLS", KHMER_SYMBOLS_ID);

            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock PHONETIC_EXTENSIONS
            = new UnicodeBlock("PHONETIC_EXTENSIONS", PHONETIC_EXTENSIONS_ID);

            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS_AND_ARROWS
            = new UnicodeBlock("MISCELLANEOUS_SYMBOLS_AND_ARROWS",
                    MISCELLANEOUS_SYMBOLS_AND_ARROWS_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock YIJING_HEXAGRAM_SYMBOLS
            = new UnicodeBlock("YIJING_HEXAGRAM_SYMBOLS", YIJING_HEXAGRAM_SYMBOLS_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock LINEAR_B_SYLLABARY
            = new UnicodeBlock("LINEAR_B_SYLLABARY", LINEAR_B_SYLLABARY_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock LINEAR_B_IDEOGRAMS
            = new UnicodeBlock("LINEAR_B_IDEOGRAMS", LINEAR_B_IDEOGRAMS_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock AEGEAN_NUMBERS
            = new UnicodeBlock("AEGEAN_NUMBERS", AEGEAN_NUMBERS_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock UGARITIC
            = new UnicodeBlock("UGARITIC", UGARITIC_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock SHAVIAN
            = new UnicodeBlock("SHAVIAN", SHAVIAN_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock OSMANYA
            = new UnicodeBlock("OSMANYA", OSMANYA_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock CYPRIOT_SYLLABARY
            = new UnicodeBlock("CYPRIOT_SYLLABARY", CYPRIOT_SYLLABARY_ID);
            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock TAI_XUAN_JING_SYMBOLS
            = new UnicodeBlock("TAI_XUAN_JING_SYMBOLS", TAI_XUAN_JING_SYMBOLS_ID);

            /**
             * @stable ICU 2.6
             */
            public static readonly UnicodeBlock VARIATION_SELECTORS_SUPPLEMENT
            = new UnicodeBlock("VARIATION_SELECTORS_SUPPLEMENT", VARIATION_SELECTORS_SUPPLEMENT_ID);

            /* New blocks in Unicode 4.1 */

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock ANCIENT_GREEK_MUSICAL_NOTATION =
                    new UnicodeBlock("ANCIENT_GREEK_MUSICAL_NOTATION",
                            ANCIENT_GREEK_MUSICAL_NOTATION_ID); /*[1D200]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock ANCIENT_GREEK_NUMBERS =
                    new UnicodeBlock("ANCIENT_GREEK_NUMBERS", ANCIENT_GREEK_NUMBERS_ID); /*[10140]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock ARABIC_SUPPLEMENT =
                    new UnicodeBlock("ARABIC_SUPPLEMENT", ARABIC_SUPPLEMENT_ID); /*[0750]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock BUGINESE =
                    new UnicodeBlock("BUGINESE", BUGINESE_ID); /*[1A00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock CJK_STROKES =
                    new UnicodeBlock("CJK_STROKES", CJK_STROKES_ID); /*[31C0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS_SUPPLEMENT =
                    new UnicodeBlock("COMBINING_DIACRITICAL_MARKS_SUPPLEMENT",
                            COMBINING_DIACRITICAL_MARKS_SUPPLEMENT_ID); /*[1DC0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock COPTIC = new UnicodeBlock("COPTIC", COPTIC_ID); /*[2C80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock ETHIOPIC_EXTENDED =
                    new UnicodeBlock("ETHIOPIC_EXTENDED", ETHIOPIC_EXTENDED_ID); /*[2D80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock ETHIOPIC_SUPPLEMENT =
                    new UnicodeBlock("ETHIOPIC_SUPPLEMENT", ETHIOPIC_SUPPLEMENT_ID); /*[1380]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock GEORGIAN_SUPPLEMENT =
                    new UnicodeBlock("GEORGIAN_SUPPLEMENT", GEORGIAN_SUPPLEMENT_ID); /*[2D00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock GLAGOLITIC =
                    new UnicodeBlock("GLAGOLITIC", GLAGOLITIC_ID); /*[2C00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock KHAROSHTHI =
                    new UnicodeBlock("KHAROSHTHI", KHAROSHTHI_ID); /*[10A00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock MODIFIER_TONE_LETTERS =
                    new UnicodeBlock("MODIFIER_TONE_LETTERS", MODIFIER_TONE_LETTERS_ID); /*[A700]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock NEW_TAI_LUE =
                    new UnicodeBlock("NEW_TAI_LUE", NEW_TAI_LUE_ID); /*[1980]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock OLD_PERSIAN =
                    new UnicodeBlock("OLD_PERSIAN", OLD_PERSIAN_ID); /*[103A0]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock PHONETIC_EXTENSIONS_SUPPLEMENT =
                    new UnicodeBlock("PHONETIC_EXTENSIONS_SUPPLEMENT",
                            PHONETIC_EXTENSIONS_SUPPLEMENT_ID); /*[1D80]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock SUPPLEMENTAL_PUNCTUATION =
                    new UnicodeBlock("SUPPLEMENTAL_PUNCTUATION", SUPPLEMENTAL_PUNCTUATION_ID); /*[2E00]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock SYLOTI_NAGRI =
                    new UnicodeBlock("SYLOTI_NAGRI", SYLOTI_NAGRI_ID); /*[A800]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock TIFINAGH =
                    new UnicodeBlock("TIFINAGH", TIFINAGH_ID); /*[2D30]*/

            /**
             * @stable ICU 3.4
             */
            public static readonly UnicodeBlock VERTICAL_FORMS =
                    new UnicodeBlock("VERTICAL_FORMS", VERTICAL_FORMS_ID); /*[FE10]*/

            /**
             * @stable ICU 3.6
             */
            public static readonly UnicodeBlock NKO = new UnicodeBlock("NKO", NKO_ID); /*[07C0]*/
                                                                                       /**
                                                                                        * @stable ICU 3.6
                                                                                        */
            public static readonly UnicodeBlock BALINESE =
                    new UnicodeBlock("BALINESE", BALINESE_ID); /*[1B00]*/
                                                               /**
                                                                * @stable ICU 3.6
                                                                */
            public static readonly UnicodeBlock LATIN_EXTENDED_C =
                    new UnicodeBlock("LATIN_EXTENDED_C", LATIN_EXTENDED_C_ID); /*[2C60]*/
                                                                               /**
                                                                                * @stable ICU 3.6
                                                                                */
            public static readonly UnicodeBlock LATIN_EXTENDED_D =
                    new UnicodeBlock("LATIN_EXTENDED_D", LATIN_EXTENDED_D_ID); /*[A720]*/
                                                                               /**
                                                                                * @stable ICU 3.6
                                                                                */
            public static readonly UnicodeBlock PHAGS_PA =
                    new UnicodeBlock("PHAGS_PA", PHAGS_PA_ID); /*[A840]*/
                                                               /**
                                                                * @stable ICU 3.6
                                                                */
            public static readonly UnicodeBlock PHOENICIAN =
                    new UnicodeBlock("PHOENICIAN", PHOENICIAN_ID); /*[10900]*/
                                                                   /**
                                                                    * @stable ICU 3.6
                                                                    */
            public static readonly UnicodeBlock CUNEIFORM =
                    new UnicodeBlock("CUNEIFORM", CUNEIFORM_ID); /*[12000]*/
                                                                 /**
                                                                  * @stable ICU 3.6
                                                                  */
            public static readonly UnicodeBlock CUNEIFORM_NUMBERS_AND_PUNCTUATION =
                    new UnicodeBlock("CUNEIFORM_NUMBERS_AND_PUNCTUATION",
                            CUNEIFORM_NUMBERS_AND_PUNCTUATION_ID); /*[12400]*/
                                                                   /**
                                                                    * @stable ICU 3.6
                                                                    */
            public static readonly UnicodeBlock COUNTING_ROD_NUMERALS =
                    new UnicodeBlock("COUNTING_ROD_NUMERALS", COUNTING_ROD_NUMERALS_ID); /*[1D360]*/

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock SUNDANESE =
                    new UnicodeBlock("SUNDANESE", SUNDANESE_ID); /* [1B80] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock LEPCHA =
                    new UnicodeBlock("LEPCHA", LEPCHA_ID); /* [1C00] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock OL_CHIKI =
                    new UnicodeBlock("OL_CHIKI", OL_CHIKI_ID); /* [1C50] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_A =
                    new UnicodeBlock("CYRILLIC_EXTENDED_A", CYRILLIC_EXTENDED_A_ID); /* [2DE0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock VAI = new UnicodeBlock("VAI", VAI_ID); /* [A500] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_B =
                    new UnicodeBlock("CYRILLIC_EXTENDED_B", CYRILLIC_EXTENDED_B_ID); /* [A640] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock SAURASHTRA =
                    new UnicodeBlock("SAURASHTRA", SAURASHTRA_ID); /* [A880] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock KAYAH_LI =
                    new UnicodeBlock("KAYAH_LI", KAYAH_LI_ID); /* [A900] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock REJANG =
                    new UnicodeBlock("REJANG", REJANG_ID); /* [A930] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock CHAM =
                    new UnicodeBlock("CHAM", CHAM_ID); /* [AA00] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock ANCIENT_SYMBOLS =
                    new UnicodeBlock("ANCIENT_SYMBOLS", ANCIENT_SYMBOLS_ID); /* [10190] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock PHAISTOS_DISC =
                    new UnicodeBlock("PHAISTOS_DISC", PHAISTOS_DISC_ID); /* [101D0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock LYCIAN =
                    new UnicodeBlock("LYCIAN", LYCIAN_ID); /* [10280] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock CARIAN =
                    new UnicodeBlock("CARIAN", CARIAN_ID); /* [102A0] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock LYDIAN =
                    new UnicodeBlock("LYDIAN", LYDIAN_ID); /* [10920] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock MAHJONG_TILES =
                    new UnicodeBlock("MAHJONG_TILES", MAHJONG_TILES_ID); /* [1F000] */

            /**
             * @stable ICU 4.0
             */
            public static readonly UnicodeBlock DOMINO_TILES =
                    new UnicodeBlock("DOMINO_TILES", DOMINO_TILES_ID); /* [1F030] */

            /* New blocks in Unicode 5.2 */

            /** @stable ICU 4.4 */
            public static readonly UnicodeBlock SAMARITAN =
                    new UnicodeBlock("SAMARITAN", SAMARITAN_ID); /*[0800]*/
                                                                 /** @stable ICU 4.4 */
            public static readonly UnicodeBlock UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED =
                    new UnicodeBlock("UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED",
                            UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_EXTENDED_ID); /*[18B0]*/
                                                                                /** @stable ICU 4.4 */
            public static readonly UnicodeBlock TAI_THAM =
                    new UnicodeBlock("TAI_THAM", TAI_THAM_ID); /*[1A20]*/
                                                               /** @stable ICU 4.4 */
            public static readonly UnicodeBlock VEDIC_EXTENSIONS =
                    new UnicodeBlock("VEDIC_EXTENSIONS", VEDIC_EXTENSIONS_ID); /*[1CD0]*/
                                                                               /** @stable ICU 4.4 */
            public static readonly UnicodeBlock LISU =
                    new UnicodeBlock("LISU", LISU_ID); /*[A4D0]*/
                                                       /** @stable ICU 4.4 */
            public static readonly UnicodeBlock BAMUM =
                    new UnicodeBlock("BAMUM", BAMUM_ID); /*[A6A0]*/
                                                         /** @stable ICU 4.4 */
            public static readonly UnicodeBlock COMMON_INDIC_NUMBER_FORMS =
                    new UnicodeBlock("COMMON_INDIC_NUMBER_FORMS", COMMON_INDIC_NUMBER_FORMS_ID); /*[A830]*/
                                                                                                 /** @stable ICU 4.4 */
            public static readonly UnicodeBlock DEVANAGARI_EXTENDED =
                    new UnicodeBlock("DEVANAGARI_EXTENDED", DEVANAGARI_EXTENDED_ID); /*[A8E0]*/
                                                                                     /** @stable ICU 4.4 */
            public static readonly UnicodeBlock HANGUL_JAMO_EXTENDED_A =
                    new UnicodeBlock("HANGUL_JAMO_EXTENDED_A", HANGUL_JAMO_EXTENDED_A_ID); /*[A960]*/
                                                                                           /** @stable ICU 4.4 */
            public static readonly UnicodeBlock JAVANESE =
                    new UnicodeBlock("JAVANESE", JAVANESE_ID); /*[A980]*/
                                                               /** @stable ICU 4.4 */
            public static readonly UnicodeBlock MYANMAR_EXTENDED_A =
                    new UnicodeBlock("MYANMAR_EXTENDED_A", MYANMAR_EXTENDED_A_ID); /*[AA60]*/
                                                                                   /** @stable ICU 4.4 */
            public static readonly UnicodeBlock TAI_VIET =
                    new UnicodeBlock("TAI_VIET", TAI_VIET_ID); /*[AA80]*/
                                                               /** @stable ICU 4.4 */
            public static readonly UnicodeBlock MEETEI_MAYEK =
                    new UnicodeBlock("MEETEI_MAYEK", MEETEI_MAYEK_ID); /*[ABC0]*/
                                                                       /** @stable ICU 4.4 */
            public static readonly UnicodeBlock HANGUL_JAMO_EXTENDED_B =
                    new UnicodeBlock("HANGUL_JAMO_EXTENDED_B", HANGUL_JAMO_EXTENDED_B_ID); /*[D7B0]*/
                                                                                           /** @stable ICU 4.4 */
            public static readonly UnicodeBlock IMPERIAL_ARAMAIC =
                    new UnicodeBlock("IMPERIAL_ARAMAIC", IMPERIAL_ARAMAIC_ID); /*[10840]*/
                                                                               /** @stable ICU 4.4 */
            public static readonly UnicodeBlock OLD_SOUTH_ARABIAN =
                    new UnicodeBlock("OLD_SOUTH_ARABIAN", OLD_SOUTH_ARABIAN_ID); /*[10A60]*/
                                                                                 /** @stable ICU 4.4 */
            public static readonly UnicodeBlock AVESTAN =
                    new UnicodeBlock("AVESTAN", AVESTAN_ID); /*[10B00]*/
                                                             /** @stable ICU 4.4 */
            public static readonly UnicodeBlock INSCRIPTIONAL_PARTHIAN =
                    new UnicodeBlock("INSCRIPTIONAL_PARTHIAN", INSCRIPTIONAL_PARTHIAN_ID); /*[10B40]*/
                                                                                           /** @stable ICU 4.4 */
            public static readonly UnicodeBlock INSCRIPTIONAL_PAHLAVI =
                    new UnicodeBlock("INSCRIPTIONAL_PAHLAVI", INSCRIPTIONAL_PAHLAVI_ID); /*[10B60]*/
                                                                                         /** @stable ICU 4.4 */
            public static readonly UnicodeBlock OLD_TURKIC =
                    new UnicodeBlock("OLD_TURKIC", OLD_TURKIC_ID); /*[10C00]*/
                                                                   /** @stable ICU 4.4 */
            public static readonly UnicodeBlock RUMI_NUMERAL_SYMBOLS =
                    new UnicodeBlock("RUMI_NUMERAL_SYMBOLS", RUMI_NUMERAL_SYMBOLS_ID); /*[10E60]*/
                                                                                       /** @stable ICU 4.4 */
            public static readonly UnicodeBlock KAITHI =
                    new UnicodeBlock("KAITHI", KAITHI_ID); /*[11080]*/
                                                           /** @stable ICU 4.4 */
            public static readonly UnicodeBlock EGYPTIAN_HIEROGLYPHS =
                    new UnicodeBlock("EGYPTIAN_HIEROGLYPHS", EGYPTIAN_HIEROGLYPHS_ID); /*[13000]*/
                                                                                       /** @stable ICU 4.4 */
            public static readonly UnicodeBlock ENCLOSED_ALPHANUMERIC_SUPPLEMENT =
                    new UnicodeBlock("ENCLOSED_ALPHANUMERIC_SUPPLEMENT",
                            ENCLOSED_ALPHANUMERIC_SUPPLEMENT_ID); /*[1F100]*/
                                                                  /** @stable ICU 4.4 */
            public static readonly UnicodeBlock ENCLOSED_IDEOGRAPHIC_SUPPLEMENT =
                    new UnicodeBlock("ENCLOSED_IDEOGRAPHIC_SUPPLEMENT",
                            ENCLOSED_IDEOGRAPHIC_SUPPLEMENT_ID); /*[1F200]*/
                                                                 /** @stable ICU 4.4 */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C_ID); /*[2A700]*/

            /* New blocks in Unicode 6.0 */

            /** @stable ICU 4.6 */
            public static readonly UnicodeBlock MANDAIC =
                    new UnicodeBlock("MANDAIC", MANDAIC_ID); /*[0840]*/
                                                             /** @stable ICU 4.6 */
            public static readonly UnicodeBlock BATAK =
                    new UnicodeBlock("BATAK", BATAK_ID); /*[1BC0]*/
                                                         /** @stable ICU 4.6 */
            public static readonly UnicodeBlock ETHIOPIC_EXTENDED_A =
                    new UnicodeBlock("ETHIOPIC_EXTENDED_A", ETHIOPIC_EXTENDED_A_ID); /*[AB00]*/
                                                                                     /** @stable ICU 4.6 */
            public static readonly UnicodeBlock BRAHMI =
                    new UnicodeBlock("BRAHMI", BRAHMI_ID); /*[11000]*/
                                                           /** @stable ICU 4.6 */
            public static readonly UnicodeBlock BAMUM_SUPPLEMENT =
                    new UnicodeBlock("BAMUM_SUPPLEMENT", BAMUM_SUPPLEMENT_ID); /*[16800]*/
                                                                               /** @stable ICU 4.6 */
            public static readonly UnicodeBlock KANA_SUPPLEMENT =
                    new UnicodeBlock("KANA_SUPPLEMENT", KANA_SUPPLEMENT_ID); /*[1B000]*/
                                                                             /** @stable ICU 4.6 */
            public static readonly UnicodeBlock PLAYING_CARDS =
                    new UnicodeBlock("PLAYING_CARDS", PLAYING_CARDS_ID); /*[1F0A0]*/
                                                                         /** @stable ICU 4.6 */
            public static readonly UnicodeBlock MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS =
                    new UnicodeBlock("MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS",
                            MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS_ID); /*[1F300]*/
                                                                       /** @stable ICU 4.6 */
            public static readonly UnicodeBlock EMOTICONS =
                    new UnicodeBlock("EMOTICONS", EMOTICONS_ID); /*[1F600]*/
                                                                 /** @stable ICU 4.6 */
            public static readonly UnicodeBlock TRANSPORT_AND_MAP_SYMBOLS =
                    new UnicodeBlock("TRANSPORT_AND_MAP_SYMBOLS", TRANSPORT_AND_MAP_SYMBOLS_ID); /*[1F680]*/
                                                                                                 /** @stable ICU 4.6 */
            public static readonly UnicodeBlock ALCHEMICAL_SYMBOLS =
                    new UnicodeBlock("ALCHEMICAL_SYMBOLS", ALCHEMICAL_SYMBOLS_ID); /*[1F700]*/
                                                                                   /** @stable ICU 4.6 */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D_ID); /*[2B740]*/

            /* New blocks in Unicode 6.1 */

            /** @stable ICU 49 */
            public static readonly UnicodeBlock ARABIC_EXTENDED_A =
                    new UnicodeBlock("ARABIC_EXTENDED_A", ARABIC_EXTENDED_A_ID); /*[08A0]*/
                                                                                 /** @stable ICU 49 */
            public static readonly UnicodeBlock ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS =
                    new UnicodeBlock("ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS", ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS_ID); /*[1EE00]*/
                                                                                                                           /** @stable ICU 49 */
            public static readonly UnicodeBlock CHAKMA = new UnicodeBlock("CHAKMA", CHAKMA_ID); /*[11100]*/
                                                                                                /** @stable ICU 49 */
            public static readonly UnicodeBlock MEETEI_MAYEK_EXTENSIONS =
                    new UnicodeBlock("MEETEI_MAYEK_EXTENSIONS", MEETEI_MAYEK_EXTENSIONS_ID); /*[AAE0]*/
                                                                                             /** @stable ICU 49 */
            public static readonly UnicodeBlock MEROITIC_CURSIVE =
                    new UnicodeBlock("MEROITIC_CURSIVE", MEROITIC_CURSIVE_ID); /*[109A0]*/
                                                                               /** @stable ICU 49 */
            public static readonly UnicodeBlock MEROITIC_HIEROGLYPHS =
                    new UnicodeBlock("MEROITIC_HIEROGLYPHS", MEROITIC_HIEROGLYPHS_ID); /*[10980]*/
                                                                                       /** @stable ICU 49 */
            public static readonly UnicodeBlock MIAO = new UnicodeBlock("MIAO", MIAO_ID); /*[16F00]*/
                                                                                          /** @stable ICU 49 */
            public static readonly UnicodeBlock SHARADA = new UnicodeBlock("SHARADA", SHARADA_ID); /*[11180]*/
                                                                                                   /** @stable ICU 49 */
            public static readonly UnicodeBlock SORA_SOMPENG =
                    new UnicodeBlock("SORA_SOMPENG", SORA_SOMPENG_ID); /*[110D0]*/
                                                                       /** @stable ICU 49 */
            public static readonly UnicodeBlock SUNDANESE_SUPPLEMENT =
                    new UnicodeBlock("SUNDANESE_SUPPLEMENT", SUNDANESE_SUPPLEMENT_ID); /*[1CC0]*/
                                                                                       /** @stable ICU 49 */
            public static readonly UnicodeBlock TAKRI = new UnicodeBlock("TAKRI", TAKRI_ID); /*[11680]*/

            /* New blocks in Unicode 7.0 */

            /** @stable ICU 54 */
            public static readonly UnicodeBlock BASSA_VAH = new UnicodeBlock("BASSA_VAH", BASSA_VAH_ID); /*[16AD0]*/
                                                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock CAUCASIAN_ALBANIAN =
                    new UnicodeBlock("CAUCASIAN_ALBANIAN", CAUCASIAN_ALBANIAN_ID); /*[10530]*/
                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock COPTIC_EPACT_NUMBERS =
                    new UnicodeBlock("COPTIC_EPACT_NUMBERS", COPTIC_EPACT_NUMBERS_ID); /*[102E0]*/
                                                                                       /** @stable ICU 54 */
            public static readonly UnicodeBlock COMBINING_DIACRITICAL_MARKS_EXTENDED =
                    new UnicodeBlock("COMBINING_DIACRITICAL_MARKS_EXTENDED", COMBINING_DIACRITICAL_MARKS_EXTENDED_ID); /*[1AB0]*/
                                                                                                                       /** @stable ICU 54 */
            public static readonly UnicodeBlock DUPLOYAN = new UnicodeBlock("DUPLOYAN", DUPLOYAN_ID); /*[1BC00]*/
                                                                                                      /** @stable ICU 54 */
            public static readonly UnicodeBlock ELBASAN = new UnicodeBlock("ELBASAN", ELBASAN_ID); /*[10500]*/
                                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock GEOMETRIC_SHAPES_EXTENDED =
                    new UnicodeBlock("GEOMETRIC_SHAPES_EXTENDED", GEOMETRIC_SHAPES_EXTENDED_ID); /*[1F780]*/
                                                                                                 /** @stable ICU 54 */
            public static readonly UnicodeBlock GRANTHA = new UnicodeBlock("GRANTHA", GRANTHA_ID); /*[11300]*/
                                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock KHOJKI = new UnicodeBlock("KHOJKI", KHOJKI_ID); /*[11200]*/
                                                                                                /** @stable ICU 54 */
            public static readonly UnicodeBlock KHUDAWADI = new UnicodeBlock("KHUDAWADI", KHUDAWADI_ID); /*[112B0]*/
                                                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock LATIN_EXTENDED_E =
                    new UnicodeBlock("LATIN_EXTENDED_E", LATIN_EXTENDED_E_ID); /*[AB30]*/
                                                                               /** @stable ICU 54 */
            public static readonly UnicodeBlock LINEAR_A = new UnicodeBlock("LINEAR_A", LINEAR_A_ID); /*[10600]*/
                                                                                                      /** @stable ICU 54 */
            public static readonly UnicodeBlock MAHAJANI = new UnicodeBlock("MAHAJANI", MAHAJANI_ID); /*[11150]*/
                                                                                                      /** @stable ICU 54 */
            public static readonly UnicodeBlock MANICHAEAN = new UnicodeBlock("MANICHAEAN", MANICHAEAN_ID); /*[10AC0]*/
                                                                                                            /** @stable ICU 54 */
            public static readonly UnicodeBlock MENDE_KIKAKUI =
                    new UnicodeBlock("MENDE_KIKAKUI", MENDE_KIKAKUI_ID); /*[1E800]*/
                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock MODI = new UnicodeBlock("MODI", MODI_ID); /*[11600]*/
                                                                                          /** @stable ICU 54 */
            public static readonly UnicodeBlock MRO = new UnicodeBlock("MRO", MRO_ID); /*[16A40]*/
                                                                                       /** @stable ICU 54 */
            public static readonly UnicodeBlock MYANMAR_EXTENDED_B =
                    new UnicodeBlock("MYANMAR_EXTENDED_B", MYANMAR_EXTENDED_B_ID); /*[A9E0]*/
                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock NABATAEAN = new UnicodeBlock("NABATAEAN", NABATAEAN_ID); /*[10880]*/
                                                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock OLD_NORTH_ARABIAN =
                    new UnicodeBlock("OLD_NORTH_ARABIAN", OLD_NORTH_ARABIAN_ID); /*[10A80]*/
                                                                                 /** @stable ICU 54 */
            public static readonly UnicodeBlock OLD_PERMIC = new UnicodeBlock("OLD_PERMIC", OLD_PERMIC_ID); /*[10350]*/
                                                                                                            /** @stable ICU 54 */
            public static readonly UnicodeBlock ORNAMENTAL_DINGBATS =
                    new UnicodeBlock("ORNAMENTAL_DINGBATS", ORNAMENTAL_DINGBATS_ID); /*[1F650]*/
                                                                                     /** @stable ICU 54 */
            public static readonly UnicodeBlock PAHAWH_HMONG = new UnicodeBlock("PAHAWH_HMONG", PAHAWH_HMONG_ID); /*[16B00]*/
                                                                                                                  /** @stable ICU 54 */
            public static readonly UnicodeBlock PALMYRENE = new UnicodeBlock("PALMYRENE", PALMYRENE_ID); /*[10860]*/
                                                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock PAU_CIN_HAU = new UnicodeBlock("PAU_CIN_HAU", PAU_CIN_HAU_ID); /*[11AC0]*/
                                                                                                               /** @stable ICU 54 */
            public static readonly UnicodeBlock PSALTER_PAHLAVI =
                    new UnicodeBlock("PSALTER_PAHLAVI", PSALTER_PAHLAVI_ID); /*[10B80]*/
                                                                             /** @stable ICU 54 */
            public static readonly UnicodeBlock SHORTHAND_FORMAT_CONTROLS =
                    new UnicodeBlock("SHORTHAND_FORMAT_CONTROLS", SHORTHAND_FORMAT_CONTROLS_ID); /*[1BCA0]*/
                                                                                                 /** @stable ICU 54 */
            public static readonly UnicodeBlock SIDDHAM = new UnicodeBlock("SIDDHAM", SIDDHAM_ID); /*[11580]*/
                                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock SINHALA_ARCHAIC_NUMBERS =
                    new UnicodeBlock("SINHALA_ARCHAIC_NUMBERS", SINHALA_ARCHAIC_NUMBERS_ID); /*[111E0]*/
                                                                                             /** @stable ICU 54 */
            public static readonly UnicodeBlock SUPPLEMENTAL_ARROWS_C =
                    new UnicodeBlock("SUPPLEMENTAL_ARROWS_C", SUPPLEMENTAL_ARROWS_C_ID); /*[1F800]*/
                                                                                         /** @stable ICU 54 */
            public static readonly UnicodeBlock TIRHUTA = new UnicodeBlock("TIRHUTA", TIRHUTA_ID); /*[11480]*/
                                                                                                   /** @stable ICU 54 */
            public static readonly UnicodeBlock WARANG_CITI = new UnicodeBlock("WARANG_CITI", WARANG_CITI_ID); /*[118A0]*/

            /* New blocks in Unicode 8.0 */

            /** @stable ICU 56 */
            public static readonly UnicodeBlock AHOM = new UnicodeBlock("AHOM", AHOM_ID); /*[11700]*/
                                                                                          /** @stable ICU 56 */
            public static readonly UnicodeBlock ANATOLIAN_HIEROGLYPHS =
                    new UnicodeBlock("ANATOLIAN_HIEROGLYPHS", ANATOLIAN_HIEROGLYPHS_ID); /*[14400]*/
                                                                                         /** @stable ICU 56 */
            public static readonly UnicodeBlock CHEROKEE_SUPPLEMENT =
                    new UnicodeBlock("CHEROKEE_SUPPLEMENT", CHEROKEE_SUPPLEMENT_ID); /*[AB70]*/
                                                                                     /** @stable ICU 56 */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E",
                            CJK_UNIFIED_IDEOGRAPHS_EXTENSION_E_ID); /*[2B820]*/
                                                                    /** @stable ICU 56 */
            public static readonly UnicodeBlock EARLY_DYNASTIC_CUNEIFORM =
                    new UnicodeBlock("EARLY_DYNASTIC_CUNEIFORM", EARLY_DYNASTIC_CUNEIFORM_ID); /*[12480]*/
                                                                                               /** @stable ICU 56 */
            public static readonly UnicodeBlock HATRAN = new UnicodeBlock("HATRAN", HATRAN_ID); /*[108E0]*/
                                                                                                /** @stable ICU 56 */
            public static readonly UnicodeBlock MULTANI = new UnicodeBlock("MULTANI", MULTANI_ID); /*[11280]*/
                                                                                                   /** @stable ICU 56 */
            public static readonly UnicodeBlock OLD_HUNGARIAN =
                    new UnicodeBlock("OLD_HUNGARIAN", OLD_HUNGARIAN_ID); /*[10C80]*/
                                                                         /** @stable ICU 56 */
            public static readonly UnicodeBlock SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS =
                    new UnicodeBlock("SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS",
                            SUPPLEMENTAL_SYMBOLS_AND_PICTOGRAPHS_ID); /*[1F900]*/
                                                                      /** @stable ICU 56 */
            public static readonly UnicodeBlock SUTTON_SIGNWRITING =
                    new UnicodeBlock("SUTTON_SIGNWRITING", SUTTON_SIGNWRITING_ID); /*[1D800]*/

            /* New blocks in Unicode 9.0 */

            /** @stable ICU 58 */
            public static readonly UnicodeBlock ADLAM = new UnicodeBlock("ADLAM", ADLAM_ID); /*[1E900]*/
                                                                                             /** @stable ICU 58 */
            public static readonly UnicodeBlock BHAIKSUKI = new UnicodeBlock("BHAIKSUKI", BHAIKSUKI_ID); /*[11C00]*/
                                                                                                         /** @stable ICU 58 */
            public static readonly UnicodeBlock CYRILLIC_EXTENDED_C =
                    new UnicodeBlock("CYRILLIC_EXTENDED_C", CYRILLIC_EXTENDED_C_ID); /*[1C80]*/
                                                                                     /** @stable ICU 58 */
            public static readonly UnicodeBlock GLAGOLITIC_SUPPLEMENT =
                    new UnicodeBlock("GLAGOLITIC_SUPPLEMENT", GLAGOLITIC_SUPPLEMENT_ID); /*[1E000]*/
                                                                                         /** @stable ICU 58 */
            public static readonly UnicodeBlock IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION =
                    new UnicodeBlock("IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION", IDEOGRAPHIC_SYMBOLS_AND_PUNCTUATION_ID); /*[16FE0]*/
                                                                                                                     /** @stable ICU 58 */
            public static readonly UnicodeBlock MARCHEN = new UnicodeBlock("MARCHEN", MARCHEN_ID); /*[11C70]*/
                                                                                                   /** @stable ICU 58 */
            public static readonly UnicodeBlock MONGOLIAN_SUPPLEMENT =
                    new UnicodeBlock("MONGOLIAN_SUPPLEMENT", MONGOLIAN_SUPPLEMENT_ID); /*[11660]*/
                                                                                       /** @stable ICU 58 */
            public static readonly UnicodeBlock NEWA = new UnicodeBlock("NEWA", NEWA_ID); /*[11400]*/
                                                                                          /** @stable ICU 58 */
            public static readonly UnicodeBlock OSAGE = new UnicodeBlock("OSAGE", OSAGE_ID); /*[104B0]*/
                                                                                             /** @stable ICU 58 */
            public static readonly UnicodeBlock TANGUT = new UnicodeBlock("TANGUT", TANGUT_ID); /*[17000]*/
                                                                                                /** @stable ICU 58 */
            public static readonly UnicodeBlock TANGUT_COMPONENTS =
                    new UnicodeBlock("TANGUT_COMPONENTS", TANGUT_COMPONENTS_ID); /*[18800]*/

            // New blocks in Unicode 10.0

            /** @stable ICU 60 */
            public static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F =
                    new UnicodeBlock("CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F", CJK_UNIFIED_IDEOGRAPHS_EXTENSION_F_ID); /*[2CEB0]*/
                                                                                                                   /** @stable ICU 60 */
            public static readonly UnicodeBlock KANA_EXTENDED_A =
                    new UnicodeBlock("KANA_EXTENDED_A", KANA_EXTENDED_A_ID); /*[1B100]*/
                                                                             /** @stable ICU 60 */
            public static readonly UnicodeBlock MASARAM_GONDI =
                    new UnicodeBlock("MASARAM_GONDI", MASARAM_GONDI_ID); /*[11D00]*/
                                                                         /** @stable ICU 60 */
            public static readonly UnicodeBlock NUSHU = new UnicodeBlock("NUSHU", NUSHU_ID); /*[1B170]*/
                                                                                             /** @stable ICU 60 */
            public static readonly UnicodeBlock SOYOMBO = new UnicodeBlock("SOYOMBO", SOYOMBO_ID); /*[11A50]*/
                                                                                                   /** @stable ICU 60 */
            public static readonly UnicodeBlock SYRIAC_SUPPLEMENT =
                    new UnicodeBlock("SYRIAC_SUPPLEMENT", SYRIAC_SUPPLEMENT_ID); /*[0860]*/
                                                                                 /** @stable ICU 60 */
            public static readonly UnicodeBlock ZANABAZAR_SQUARE =
                    new UnicodeBlock("ZANABAZAR_SQUARE", ZANABAZAR_SQUARE_ID); /*[11A00]*/

            /**
             * @stable ICU 2.4
             */
            public static readonly UnicodeBlock INVALID_CODE
            = new UnicodeBlock("INVALID_CODE", INVALID_CODE_ID);

            static UnicodeBlock()
            {
                for (int blockId = 0; blockId < COUNT; ++blockId)
                {
                    if (BLOCKS_[blockId] == null)
                    {
                        throw new InvalidOperationException(
                                "UnicodeBlock.BLOCKS_[" + blockId + "] not initialized");
                    }
                }
            }

            // public methods --------------------------------------------------

            /**
             * {@icu} Returns the only instance of the UnicodeBlock with the argument ID.
             * If no such ID exists, a INVALID_CODE UnicodeBlock will be returned.
             * @param id UnicodeBlock ID
             * @return the only instance of the UnicodeBlock with the argument ID
             *         if it exists, otherwise a INVALID_CODE UnicodeBlock will be
             *         returned.
             * @stable ICU 2.4
             */
            public static UnicodeBlock GetInstance(int id)
            {
                if (id >= 0 && id < BLOCKS_.Length)
                {
                    return BLOCKS_[id];
                }
                return INVALID_CODE;
            }

            /**
             * Returns the Unicode allocation block that contains the code point,
             * or null if the code point is not a member of a defined block.
             * @param ch code point to be tested
             * @return the Unicode allocation block that contains the code point
             * @stable ICU 2.4
             */
            public static UnicodeBlock Of(int ch)
            {
                if (ch > MAX_VALUE)
                {
                    return INVALID_CODE;
                }

                return UnicodeBlock.GetInstance(
                        UCharacterProperty.INSTANCE.GetInt32PropertyValue(ch, (int)UProperty.BLOCK));
            }

            /**
             * Alternative to the {@link java.lang.Character.UnicodeBlock#forName(string)} method.
             * Returns the Unicode block with the given name. {@icunote} Unlike
             * {@link java.lang.Character.UnicodeBlock#forName(string)}, this only matches
             * against the official UCD name and the Java block name
             * (ignoring case).
             * @param blockName the name of the block to match
             * @return the UnicodeBlock with that name
             * @throws IllegalArgumentException if the blockName could not be matched
             * @stable ICU 3.0
             */
            public static UnicodeBlock ForName(string blockName)
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
                                GetPropertyValueName(UProperty.BLOCK, b2.ID,
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

            /**
             * {icu} Returns the type ID of this Unicode block
             * @return integer type ID of this Unicode block
             * @stable ICU 2.4
             */
            public int ID
            {
                get { return m_id_; }
            }

            /// <summary>
            /// Returns the name of this UnicodeBlock.
            /// </summary>
            // ICU4N specific - we don't have a Character.Subset base class, so
            // this functionality was moved here.
            public override string ToString()
            {
                return name;
            }

            // private data members ---------------------------------------------

            /**
             * Identification code for this UnicodeBlock
             */
            private int m_id_;

            /// <summary>
            /// Name for this <see cref="UnicodeBlock"/>
            /// </summary>
            private string name;

            // private constructor ----------------------------------------------

            /**
             * UnicodeBlock constructor
             * @param name name of this UnicodeBlock
             * @param id unique id of this UnicodeBlock
             * @exception NullPointerException if name is <code>null</code>
             */
            private UnicodeBlock(string name, int id)
            //: base(name)
            {
                this.name = name;
                m_id_ = id;
                if (id >= 0)
                {
                    BLOCKS_[id] = this;
                }
            }
        }

        /**
         * East Asian Width constants.
         * @see UProperty#EAST_ASIAN_WIDTH
         * @see UCharacter#getIntPropertyValue
         * @stable ICU 2.4
         */
        public static class EastAsianWidth
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int NEUTRAL = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int AMBIGUOUS = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HALFWIDTH = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FULLWIDTH = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NARROW = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int WIDE = 5;
            /**
             * One more than the highest normal EastAsianWidth value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.EAST_ASIAN_WIDTH).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 6;
        }

        /**
         * Decomposition Type constants.
         * @see UProperty#DECOMPOSITION_TYPE
         * @stable ICU 2.4
         */
        public static class DecompositionType
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int NONE = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CANONICAL = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int COMPAT = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CIRCLE = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FINAL = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FONT = 5;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FRACTION = 6;
            /**
             * @stable ICU 2.4
             */
            public static readonly int INITIAL = 7;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ISOLATED = 8;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MEDIAL = 9;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NARROW = 10;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NOBREAK = 11;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SMALL = 12;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SQUARE = 13;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUB = 14;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SUPER = 15;
            /**
             * @stable ICU 2.4
             */
            public static readonly int VERTICAL = 16;
            /**
             * @stable ICU 2.4
             */
            public static readonly int WIDE = 17;
            /**
             * One more than the highest normal DecompositionType value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.DECOMPOSITION_TYPE).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 18;
        }

        /**
         * Joining Type constants.
         * @see UProperty#JOINING_TYPE
         * @stable ICU 2.4
         */
        public static class JoiningType
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int NON_JOINING = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int JOIN_CAUSING = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DUAL_JOINING = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LEFT_JOINING = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int RIGHT_JOINING = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TRANSPARENT = 5;
            /**
             * One more than the highest normal JoiningType value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.JOINING_TYPE).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 6;
        }

        /**
         * Joining Group constants.
         * @see UProperty#JOINING_GROUP
         * @stable ICU 2.4
         */
        public static class JoiningGroup
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int NO_JOINING_GROUP = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int AIN = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ALAPH = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ALEF = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BEH = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BETH = 5;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DAL = 6;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DALATH_RISH = 7;
            /**
             * @stable ICU 2.4
             */
            public static readonly int E = 8;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FEH = 9;
            /**
             * @stable ICU 2.4
             */
            public static readonly int FINAL_SEMKATH = 10;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GAF = 11;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GAMAL = 12;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HAH = 13;
            /** @stable ICU 4.6 */
            public static readonly int TEH_MARBUTA_GOAL = 14;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HAMZA_ON_HEH_GOAL = TEH_MARBUTA_GOAL;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HE = 15;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HEH = 16;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HEH_GOAL = 17;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HETH = 18;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KAF = 19;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KAPH = 20;
            /**
             * @stable ICU 2.4
             */
            public static readonly int KNOTTED_HEH = 21;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LAM = 22;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LAMADH = 23;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MEEM = 24;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MIM = 25;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NOON = 26;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NUN = 27;
            /**
             * @stable ICU 2.4
             */
            public static readonly int PE = 28;
            /**
             * @stable ICU 2.4
             */
            public static readonly int QAF = 29;
            /**
             * @stable ICU 2.4
             */
            public static readonly int QAPH = 30;
            /**
             * @stable ICU 2.4
             */
            public static readonly int REH = 31;
            /**
             * @stable ICU 2.4
             */
            public static readonly int REVERSED_PE = 32;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SAD = 33;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SADHE = 34;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SEEN = 35;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SEMKATH = 36;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SHIN = 37;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SWASH_KAF = 38;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SYRIAC_WAW = 39;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAH = 40;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TAW = 41;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TEH_MARBUTA = 42;
            /**
             * @stable ICU 2.4
             */
            public static readonly int TETH = 43;
            /**
             * @stable ICU 2.4
             */
            public static readonly int WAW = 44;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YEH = 45;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YEH_BARREE = 46;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YEH_WITH_TAIL = 47;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YUDH = 48;
            /**
             * @stable ICU 2.4
             */
            public static readonly int YUDH_HE = 49;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ZAIN = 50;
            /**
             * @stable ICU 2.6
             */
            public static readonly int FE = 51;
            /**
             * @stable ICU 2.6
             */
            public static readonly int KHAPH = 52;
            /**
             * @stable ICU 2.6
             */
            public static readonly int ZHAIN = 53;
            /**
             * @stable ICU 4.0
             */
            public static readonly int BURUSHASKI_YEH_BARREE = 54;
            /** @stable ICU 4.4 */
            public static readonly int FARSI_YEH = 55;
            /** @stable ICU 4.4 */
            public static readonly int NYA = 56;
            /** @stable ICU 49 */
            public static readonly int ROHINGYA_YEH = 57;

            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_ALEPH = 58;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_AYIN = 59;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_BETH = 60;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_DALETH = 61;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_DHAMEDH = 62;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_FIVE = 63;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_GIMEL = 64;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_HETH = 65;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_HUNDRED = 66;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_KAPH = 67;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_LAMEDH = 68;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_MEM = 69;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_NUN = 70;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_ONE = 71;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_PE = 72;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_QOPH = 73;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_RESH = 74;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_SADHE = 75;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_SAMEKH = 76;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_TAW = 77;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_TEN = 78;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_TETH = 79;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_THAMEDH = 80;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_TWENTY = 81;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_WAW = 82;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_YODH = 83;
            /** @stable ICU 54 */
            public static readonly int MANICHAEAN_ZAYIN = 84;
            /** @stable ICU 54 */
            public static readonly int STRAIGHT_WAW = 85;

            /** @stable ICU 58 */
            public static readonly int AFRICAN_FEH = 86;
            /** @stable ICU 58 */
            public static readonly int AFRICAN_NOON = 87;
            /** @stable ICU 58 */
            public static readonly int AFRICAN_QAF = 88;

            /** @stable ICU 60 */
            public static readonly int MALAYALAM_BHA = 89;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_JA = 90;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_LLA = 91;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_LLLA = 92;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_NGA = 93;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_NNA = 94;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_NNNA = 95;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_NYA = 96;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_RA = 97;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_SSA = 98;
            /** @stable ICU 60 */
            public static readonly int MALAYALAM_TTA = 99;

            /**
             * One more than the highest normal JoiningGroup value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.JoiningGroup).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 100;
        }

        /**
         * Grapheme Cluster Break constants.
         * @see UProperty#GRAPHEME_CLUSTER_BREAK
         * @stable ICU 3.4
         */
        public static class GraphemeClusterBreak
        {
            /**
             * @stable ICU 3.4
             */
            public static readonly int OTHER = 0;
            /**
             * @stable ICU 3.4
             */
            public static readonly int CONTROL = 1;
            /**
             * @stable ICU 3.4
             */
            public static readonly int CR = 2;
            /**
             * @stable ICU 3.4
             */
            public static readonly int EXTEND = 3;
            /**
             * @stable ICU 3.4
             */
            public static readonly int L = 4;
            /**
             * @stable ICU 3.4
             */
            public static readonly int LF = 5;
            /**
             * @stable ICU 3.4
             */
            public static readonly int LV = 6;
            /**
             * @stable ICU 3.4
             */
            public static readonly int LVT = 7;
            /**
             * @stable ICU 3.4
             */
            public static readonly int T = 8;
            /**
             * @stable ICU 3.4
             */
            public static readonly int V = 9;
            /**
             * @stable ICU 4.0
             */
            public static readonly int SPACING_MARK = 10;
            /**
             * @stable ICU 4.0
             */
            public static readonly int PREPEND = 11;
            /** @stable ICU 50 */
            public static readonly int REGIONAL_INDICATOR = 12;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
                                                                          /** @stable ICU 58 */
            public static readonly int E_BASE = 13;          /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
                                                                      /** @stable ICU 58 */
            public static readonly int E_BASE_GAZ = 14;      /*[EBG]*/
                                                             /** @stable ICU 58 */
            public static readonly int E_MODIFIER = 15;      /*[EM]*/
                                                             /** @stable ICU 58 */
            public static readonly int GLUE_AFTER_ZWJ = 16;  /*[GAZ]*/
                                                             /** @stable ICU 58 */
            public static readonly int ZWJ = 17;             /*[ZWJ]*/
                                                             /**
                                                              * One more than the highest normal GraphemeClusterBreak value.
                                                              * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.GRAPHEME_CLUSTER_BREAK).
                                                              *
                                                              * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
                                                              */
            [Obsolete]
            public static readonly int COUNT = 18;
        }

        /**
         * Word Break constants.
         * @see UProperty#WORD_BREAK
         * @stable ICU 3.4
         */
        public static class WordBreak
        {
            /**
             * @stable ICU 3.8
             */
            public static readonly int OTHER = 0;
            /**
             * @stable ICU 3.8
             */
            public static readonly int ALETTER = 1;
            /**
             * @stable ICU 3.8
             */
            public static readonly int FORMAT = 2;
            /**
             * @stable ICU 3.8
             */
            public static readonly int KATAKANA = 3;
            /**
             * @stable ICU 3.8
             */
            public static readonly int MIDLETTER = 4;
            /**
             * @stable ICU 3.8
             */
            public static readonly int MIDNUM = 5;
            /**
             * @stable ICU 3.8
             */
            public static readonly int NUMERIC = 6;
            /**
             * @stable ICU 3.8
             */
            public static readonly int EXTENDNUMLET = 7;
            /**
             * @stable ICU 4.0
             */
            public static readonly int CR = 8;
            /**
             * @stable ICU 4.0
             */
            public static readonly int EXTEND = 9;
            /**
             * @stable ICU 4.0
             */
            public static readonly int LF = 10;
            /**
             * @stable ICU 4.0
             */
            public static readonly int MIDNUMLET = 11;
            /**
             * @stable ICU 4.0
             */
            public static readonly int NEWLINE = 12;
            /** @stable ICU 50 */
            public static readonly int REGIONAL_INDICATOR = 13;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
                                                                          /** @stable ICU 52 */
            public static readonly int HEBREW_LETTER = 14;    /*[HL]*/ /* from here on: new in Unicode 6.3/ICU 52 */
                                                                       /** @stable ICU 52 */
            public static readonly int SINGLE_QUOTE = 15;     /*[SQ]*/
                                                              /** @stable ICU 52 */
            public static readonly int DOUBLE_QUOTE = 16;     /*[DQ]*/
                                                              /** @stable ICU 58 */
            public static readonly int E_BASE = 17;           /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
                                                                       /** @stable ICU 58 */
            public static readonly int E_BASE_GAZ = 18;       /*[EBG]*/
                                                              /** @stable ICU 58 */
            public static readonly int E_MODIFIER = 19;       /*[EM]*/
                                                              /** @stable ICU 58 */
            public static readonly int GLUE_AFTER_ZWJ = 20;   /*[GAZ]*/
                                                              /** @stable ICU 58 */
            public static readonly int ZWJ = 21;              /*[ZWJ]*/
                                                              /**
                                                               * One more than the highest normal WordBreak value.
                                                               * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.WORD_BREAK).
                                                               *
                                                               * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
                                                               */
            [Obsolete]
            public static readonly int COUNT = 22;
        }

        /**
         * Sentence Break constants.
         * @see UProperty#SENTENCE_BREAK
         * @stable ICU 3.4
         */
        public static class SentenceBreak
        {
            /**
             * @stable ICU 3.8
             */
            public static readonly int OTHER = 0;
            /**
             * @stable ICU 3.8
             */
            public static readonly int ATERM = 1;
            /**
             * @stable ICU 3.8
             */
            public static readonly int CLOSE = 2;
            /**
             * @stable ICU 3.8
             */
            public static readonly int FORMAT = 3;
            /**
             * @stable ICU 3.8
             */
            public static readonly int LOWER = 4;
            /**
             * @stable ICU 3.8
             */
            public static readonly int NUMERIC = 5;
            /**
             * @stable ICU 3.8
             */
            public static readonly int OLETTER = 6;
            /**
             * @stable ICU 3.8
             */
            public static readonly int SEP = 7;
            /**
             * @stable ICU 3.8
             */
            public static readonly int SP = 8;
            /**
             * @stable ICU 3.8
             */
            public static readonly int STERM = 9;
            /**
             * @stable ICU 3.8
             */
            public static readonly int UPPER = 10;
            /**
             * @stable ICU 4.0
             */
            public static readonly int CR = 11;
            /**
             * @stable ICU 4.0
             */
            public static readonly int EXTEND = 12;
            /**
             * @stable ICU 4.0
             */
            public static readonly int LF = 13;
            /**
             * @stable ICU 4.0
             */
            public static readonly int SCONTINUE = 14;
            /**
             * One more than the highest normal SentenceBreak value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.SENTENCE_BREAK).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 15;
        }

        /**
         * Line Break constants.
         * @see UProperty#LINE_BREAK
         * @stable ICU 2.4
         */
        public static class LineBreak
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int UNKNOWN = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int AMBIGUOUS = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ALPHABETIC = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BREAK_BOTH = 3;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BREAK_AFTER = 4;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BREAK_BEFORE = 5;
            /**
             * @stable ICU 2.4
             */
            public static readonly int MANDATORY_BREAK = 6;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CONTINGENT_BREAK = 7;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CLOSE_PUNCTUATION = 8;
            /**
             * @stable ICU 2.4
             */
            public static readonly int COMBINING_MARK = 9;
            /**
             * @stable ICU 2.4
             */
            public static readonly int CARRIAGE_RETURN = 10;
            /**
             * @stable ICU 2.4
             */
            public static readonly int EXCLAMATION = 11;
            /**
             * @stable ICU 2.4
             */
            public static readonly int GLUE = 12;
            /**
             * @stable ICU 2.4
             */
            public static readonly int HYPHEN = 13;
            /**
             * @stable ICU 2.4
             */
            public static readonly int IDEOGRAPHIC = 14;
            /**
             * @see #INSEPARABLE
             * @stable ICU 2.4
             */
            public static readonly int INSEPERABLE = 15;
            /**
             * Renamed from the misspelled "inseperable" in Unicode 4.0.1.
             * @stable ICU 3.0
             */
            public static readonly int INSEPARABLE = 15;
            /**
             * @stable ICU 2.4
             */
            public static readonly int INFIX_NUMERIC = 16;
            /**
             * @stable ICU 2.4
             */
            public static readonly int LINE_FEED = 17;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NONSTARTER = 18;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NUMERIC = 19;
            /**
             * @stable ICU 2.4
             */
            public static readonly int OPEN_PUNCTUATION = 20;
            /**
             * @stable ICU 2.4
             */
            public static readonly int POSTFIX_NUMERIC = 21;
            /**
             * @stable ICU 2.4
             */
            public static readonly int PREFIX_NUMERIC = 22;
            /**
             * @stable ICU 2.4
             */
            public static readonly int QUOTATION = 23;
            /**
             * @stable ICU 2.4
             */
            public static readonly int COMPLEX_CONTEXT = 24;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SURROGATE = 25;
            /**
             * @stable ICU 2.4
             */
            public static readonly int SPACE = 26;
            /**
             * @stable ICU 2.4
             */
            public static readonly int BREAK_SYMBOLS = 27;
            /**
             * @stable ICU 2.4
             */
            public static readonly int ZWSPACE = 28;
            /**
             * @stable ICU 2.6
             */
            public static readonly int NEXT_LINE = 29;  /*[NL]*/ /* from here on: new in Unicode 4/ICU 2.6 */
                                                                 /**
                                                                  * @stable ICU 2.6
                                                                  */
            public static readonly int WORD_JOINER = 30;      /*[WJ]*/
                                                              /**
                                                               * @stable ICU 3.4
                                                               */
            public static readonly int H2 = 31;  /* from here on: new in Unicode 4.1/ICU 3.4 */
                                                 /**
                                                  * @stable ICU 3.4
                                                  */
            public static readonly int H3 = 32;
            /**
             * @stable ICU 3.4
             */
            public static readonly int JL = 33;
            /**
             * @stable ICU 3.4
             */
            public static readonly int JT = 34;
            /**
             * @stable ICU 3.4
             */
            public static readonly int JV = 35;
            /** @stable ICU 4.4 */
            public static readonly int CLOSE_PARENTHESIS = 36; /*[CP]*/ /* new in Unicode 5.2/ICU 4.4 */
                                                                        /** @stable ICU 49 */
            public static readonly int CONDITIONAL_JAPANESE_STARTER = 37;  /*[CJ]*/ /* new in Unicode 6.1/ICU 49 */
                                                                                    /** @stable ICU 49 */
            public static readonly int HEBREW_LETTER = 38;  /*[HL]*/ /* new in Unicode 6.1/ICU 49 */
                                                                     /** @stable ICU 50 */
            public static readonly int REGIONAL_INDICATOR = 39;  /*[RI]*/ /* new in Unicode 6.2/ICU 50 */
                                                                          /** @stable ICU 58 */
            public static readonly int E_BASE = 40;  /*[EB]*/ /* from here on: new in Unicode 9.0/ICU 58 */
                                                              /** @stable ICU 58 */
            public static readonly int E_MODIFIER = 41;  /*[EM]*/
                                                         /** @stable ICU 58 */
            public static readonly int ZWJ = 42;  /*[ZWJ]*/
                                                  /**
                                                   * One more than the highest normal LineBreak value.
                                                   * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.LINE_BREAK).
                                                   *
                                                   * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
                                                   */
            [Obsolete]
            public static readonly int COUNT = 43;
        }

        /**
         * Numeric Type constants.
         * @see UProperty#NUMERIC_TYPE
         * @stable ICU 2.4
         */
        public static class NumericType
        {
            /**
             * @stable ICU 2.4
             */
            public static readonly int NONE = 0;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DECIMAL = 1;
            /**
             * @stable ICU 2.4
             */
            public static readonly int DIGIT = 2;
            /**
             * @stable ICU 2.4
             */
            public static readonly int NUMERIC = 3;
            /**
             * One more than the highest normal NumericType value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.NUMERIC_TYPE).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 4;
        }

        /**
         * Hangul Syllable Type constants.
         *
         * @see UProperty#HANGUL_SYLLABLE_TYPE
         * @stable ICU 2.6
         */
        public static class HangulSyllableType
        {
            /**
             * @stable ICU 2.6
             */
            public static readonly int NOT_APPLICABLE = 0;   /*[NA]*/ /*See note !!*/
                                                                      /**
                                                                       * @stable ICU 2.6
                                                                       */
            public static readonly int LEADING_JAMO = 1;   /*[L]*/
                                                           /**
                                                            * @stable ICU 2.6
                                                            */
            public static readonly int VOWEL_JAMO = 2;   /*[V]*/
                                                         /**
                                                          * @stable ICU 2.6
                                                          */
            public static readonly int TRAILING_JAMO = 3;   /*[T]*/
                                                            /**
                                                             * @stable ICU 2.6
                                                             */
            public static readonly int LV_SYLLABLE = 4;   /*[LV]*/
                                                          /**
                                                           * @stable ICU 2.6
                                                           */
            public static readonly int LVT_SYLLABLE = 5;   /*[LVT]*/
                                                           /**
                                                            * One more than the highest normal HangulSyllableType value.
                                                            * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.HANGUL_SYLLABLE_TYPE).
                                                            *
                                                            * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
                                                            */
            [Obsolete]
            public static readonly int COUNT = 6;
        }

        /**
         * Bidi Paired Bracket Type constants.
         *
         * @see UProperty#BIDI_PAIRED_BRACKET_TYPE
         * @stable ICU 52
         */
        public static class BidiPairedBracketType
        {
            /**
             * Not a paired bracket.
             * @stable ICU 52
             */
            public static readonly int NONE = 0;
            /**
             * Open paired bracket.
             * @stable ICU 52
             */
            public static readonly int OPEN = 1;
            /**
             * Close paired bracket.
             * @stable ICU 52
             */
            public static readonly int CLOSE = 2;
            /**
             * One more than the highest normal BidiPairedBracketType value.
             * The highest value is available via UCharacter.getIntPropertyMaxValue(UProperty.BIDI_PAIRED_BRACKET_TYPE).
             *
             * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
             */
            [Obsolete]
            public static readonly int COUNT = 3;
        }

        // public data members -----------------------------------------------

        /**
         * The lowest Unicode code point value, constant 0.
         * Same as {@link Character#MIN_CODE_POINT}, same integer value as {@link Character#MIN_VALUE}.
         *
         * @stable ICU 2.1
         */
        public static readonly int MIN_VALUE = Character.MIN_CODE_POINT;

        /**
         * The highest Unicode code point value (scalar value), constant U+10FFFF (uses 21 bits).
         * Same as {@link Character#MAX_CODE_POINT}.
         *
         * <p>Up-to-date Unicode implementation of {@link Character#MAX_VALUE}
         * which is still a char with the value U+FFFF.
         *
         * @stable ICU 2.1
         */
        public static readonly int MAX_VALUE = Character.MAX_CODE_POINT;

        /**
         * The minimum value for Supplementary code points, constant U+10000.
         * Same as {@link Character#MIN_SUPPLEMENTARY_CODE_POINT}.
         *
         * @stable ICU 2.1
         */
        public static readonly int SUPPLEMENTARY_MIN_VALUE = Character.MIN_SUPPLEMENTARY_CODE_POINT;

        /**
         * Unicode value used when translating into Unicode encoding form and there
         * is no existing character.
         * @stable ICU 2.1
         */
        public static readonly int REPLACEMENT_CHAR = '\uFFFD';

        /**
         * Special value that is returned by getUnicodeNumericValue(int) when no
         * numeric value is defined for a code point.
         * @stable ICU 2.4
         * @see #getUnicodeNumericValue
         */
        public static readonly double NO_NUMERIC_VALUE = -123456789;

        /**
         * Compatibility constant for Java Character's MIN_RADIX.
         * @stable ICU 3.4
         */
        public static readonly int MIN_RADIX = Character.MIN_RADIX;

        /**
         * Compatibility constant for Java Character's MAX_RADIX.
         * @stable ICU 3.4
         */
        public static readonly int MAX_RADIX = Character.MAX_RADIX;

        /**
         * Do not lowercase non-initial parts of words when titlecasing.
         * Option bit for titlecasing APIs that take an options bit set.
         *
         * By default, titlecasing will titlecase the first cased character
         * of a word and lowercase all other characters.
         * With this option, the other characters will not be modified.
         *
         * @see #toTitleCase
         * @stable ICU 3.8
         */
        public static readonly int TITLECASE_NO_LOWERCASE = 0x100;

        /**
         * Do not adjust the titlecasing indexes from BreakIterator::next() indexes;
         * titlecase exactly the characters at breaks from the iterator.
         * Option bit for titlecasing APIs that take an options bit set.
         *
         * By default, titlecasing will take each break iterator index,
         * adjust it by looking for the next cased character, and titlecase that one.
         * Other characters are lowercased.
         *
         * This follows Unicode 4 &amp; 5 section 3.13 Default Case Operations:
         *
         * R3  toTitlecase(X): Find the word boundaries based on Unicode Standard Annex
         * #29, "Text Boundaries." Between each pair of word boundaries, find the first
         * cased character F. If F exists, map F to default_title(F); then map each
         * subsequent character C to default_lower(C).
         *
         * @see #toTitleCase
         * @see #TITLECASE_NO_LOWERCASE
         * @stable ICU 3.8
         */
        public static readonly int TITLECASE_NO_BREAK_ADJUSTMENT = 0x200;

        // public methods ----------------------------------------------------

        /**
         * Returnss the numeric value of a decimal digit code point.
         * <br>This method observes the semantics of
         * <code>java.lang.Character.digit()</code>.  Note that this
         * will return positive values for code points for which isDigit
         * returns false, just like java.lang.Character.
         * <br><em>Semantic Change:</em> In release 1.3.1 and
         * prior, this did not treat the European letters as having a
         * digit value, and also treated numeric letters and other numbers as
         * digits.
         * This has been changed to conform to the java semantics.
         * <br>A code point is a valid digit if and only if:
         * <ul>
         *   <li>ch is a decimal digit or one of the european letters, and
         *   <li>the value of ch is less than the specified radix.
         * </ul>
         * @param ch the code point to query
         * @param radix the radix
         * @return the numeric value represented by the code point in the
         * specified radix, or -1 if the code point is not a decimal digit
         * or if its value is too large for the radix
         * @stable ICU 2.1
         */
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

        /**
         * Returnss the numeric value of a decimal digit code point.
         * <br>This is a convenience overload of <code>digit(int, int)</code>
         * that provides a decimal radix.
         * <br><em>Semantic Change:</em> In release 1.3.1 and prior, this
         * treated numeric letters and other numbers as digits.  This has
         * been changed to conform to the java semantics.
         * @param ch the code point to query
         * @return the numeric value represented by the code point,
         * or -1 if the code point is not a decimal digit or if its
         * value is too large for a decimal radix
         * @stable ICU 2.1
         */
        public static int Digit(int ch)
        {
            return UCharacterProperty.INSTANCE.Digit(ch);
        }

        /**
         * Returns the numeric value of the code point as a nonnegative
         * integer.
         * <br>If the code point does not have a numeric value, then -1 is returned.
         * <br>
         * If the code point has a numeric value that cannot be represented as a
         * nonnegative integer (for example, a fractional value), then -2 is
         * returned.
         * @param ch the code point to query
         * @return the numeric value of the code point, or -1 if it has no numeric
         * value, or -2 if it has a numeric value that cannot be represented as a
         * nonnegative integer
         * @stable ICU 2.1
         */
        public static int GetNumericValue(int ch)
        {
            return UCharacterProperty.INSTANCE.GetNumericValue(ch);
        }

        /**
         * {@icu} Returns the numeric value for a Unicode code point as defined in the
         * Unicode Character Database.
         * <p>A "double" return type is necessary because some numeric values are
         * fractions, negative, or too large for int.
         * <p>For characters without any numeric values in the Unicode Character
         * Database, this function will return NO_NUMERIC_VALUE.
         * Note: This is different from the Unicode Standard which specifies NaN as the default value.
         * <p><em>API Change:</em> In release 2.2 and prior, this API has a
         * return type int and returns -1 when the argument ch does not have a
         * corresponding numeric value. This has been changed to synch with ICU4C
         *
         * This corresponds to the ICU4C function u_getNumericValue.
         * @param ch Code point to get the numeric value for.
         * @return numeric value of ch, or NO_NUMERIC_VALUE if none is defined.
         * @stable ICU 2.4
         */
        public static double GetUnicodeNumericValue(int ch)
        {
            return UCharacterProperty.INSTANCE.GetUnicodeNumericValue(ch);
        }

        /**
         * Compatibility override of Java deprecated method.  This
         * method will always remain deprecated.
         * Same as java.lang.Character.isSpace().
         * @param ch the code point
         * @return true if the code point is a space character as
         * defined by java.lang.Character.isSpace.
         * @deprecated ICU 3.4 (Java)
         */
        [Obsolete]
        public static bool IsSpace(int ch)
        {
            return ch <= 0x20 &&
                    (ch == 0x20 || ch == 0x09 || ch == 0x0a || ch == 0x0c || ch == 0x0d);
        }

        /**
         * Returns a value indicating a code point's Unicode category.
         * Up-to-date Unicode implementation of java.lang.Character.getType()
         * except for the above mentioned code points that had their category
         * changed.<br>
         * Return results are constants from the interface
         * <a href=UCharacterCategory.html>UCharacterCategory</a><br>
         * <em>NOTE:</em> the UCharacterCategory values are <em>not</em> compatible with
         * those returned by java.lang.Character.getType.  UCharacterCategory values
         * match the ones used in ICU4C, while java.lang.Character type
         * values, though similar, skip the value 17.
         * @param ch code point whose type is to be determined
         * @return category which is a value of UCharacterCategory
         * @stable ICU 2.1
         */
        public static UnicodeCategory GetType(int ch)
        {
            return UnicodeCategoryConvert.FromIcuValue(UCharacterProperty.INSTANCE.GetType(ch));
        }

        /**
         * Determines if a code point has a defined meaning in the up-to-date
         * Unicode standard.
         * E.g. supplementary code points though allocated space are not defined in
         * Unicode yet.<br>
         * Up-to-date Unicode implementation of java.lang.Character.isDefined()
         * @param ch code point to be determined if it is defined in the most
         *        current version of Unicode
         * @return true if this code point is defined in unicode
         * @stable ICU 2.1
         */
        public static bool IsDefined(int ch)
        {
            // ICU4N specific - need to check for the value, not 0
            return GetType(ch) != UnicodeCategory.OtherNotAssigned;
        }

        /**
         * Determines if a code point is a Java digit.
         * <br>This method observes the semantics of
         * <code>java.lang.Character.isDigit()</code>. It returns true for decimal
         * digits only.
         * <br><em>Semantic Change:</em> In release 1.3.1 and prior, this treated
         * numeric letters and other numbers as digits.
         * This has been changed to conform to the java semantics.
         * @param ch code point to query
         * @return true if this code point is a digit
         * @stable ICU 2.1
         */
        public static bool IsDigit(int ch)
        {
            return GetType(ch) == UnicodeCategory.DecimalDigitNumber;
        }

        /**
         * Determines if the specified code point is an ISO control character.
         * A code point is considered to be an ISO control character if it is in
         * the range &#92;u0000 through &#92;u001F or in the range &#92;u007F through
         * &#92;u009F.<br>
         * Up-to-date Unicode implementation of java.lang.Character.isISOControl()
         * @param ch code point to determine if it is an ISO control character
         * @return true if code point is a ISO control character
         * @stable ICU 2.1
         */
        public static bool IsISOControl(int ch)
        {
            return ch >= 0 && ch <= APPLICATION_PROGRAM_COMMAND_ &&
                    ((ch <= UNIT_SEPARATOR_) || (ch >= DELETE_));
        }

        /**
         * Determines if the specified code point is a letter.
         * Up-to-date Unicode implementation of java.lang.Character.isLetter()
         * @param ch code point to determine if it is a letter
         * @return true if code point is a letter
         * @stable ICU 2.1
         */
        public static bool IsLetter(int ch)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToIcuValue())
                    & ((1 << UnicodeCategory.UppercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LowercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.TitlecaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.ModifierLetter.ToIcuValue())
                            | (1 << UnicodeCategory.OtherLetter.ToIcuValue()))) != 0;
        }

        /**
         * Determines if the specified code point is a letter or digit.
         * {@icunote} This method, unlike java.lang.Character does not regard the ascii
         * characters 'A' - 'Z' and 'a' - 'z' as digits.
         * @param ch code point to determine if it is a letter or a digit
         * @return true if code point is a letter or a digit
         * @stable ICU 2.1
         */
        public static bool IsLetterOrDigit(int ch)
        {
            return ((1 << GetType(ch).ToIcuValue())
                    & ((1 << UnicodeCategory.UppercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LowercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.TitlecaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.ModifierLetter.ToIcuValue())
                            | (1 << UnicodeCategory.OtherLetter.ToIcuValue())
                            | (1 << UnicodeCategory.DecimalDigitNumber.ToIcuValue()))) != 0;
        }

        // ICU4N: We definitely don't need any of the Java.. functions. 
        // In .NET, it is not so straightforward

        ///**
        // * Compatibility override of Java deprecated method.  This
        // * method will always remain deprecated.  Delegates to
        // * java.lang.Character.isJavaIdentifierStart.
        // * @param cp the code point
        // * @return true if the code point can start a java identifier.
        // * @deprecated ICU 3.4 (Java)
        // */
        //[Obsolete]
        //public static bool IsJavaLetter(int cp)
        //{
        //    return IsJavaIdentifierStart(cp);
        //}

        ///**
        // * Compatibility override of Java deprecated method.  This
        // * method will always remain deprecated.  Delegates to
        // * java.lang.Character.isJavaIdentifierPart.
        // * @param cp the code point
        // * @return true if the code point can continue a java identifier.
        // * @deprecated ICU 3.4 (Java)
        // */
        //[Obsolete]
        //public static bool IsJavaLetterOrDigit(int cp)
        //{
        //    throw new NotImplementedException();
        //    //return isJavaIdentifierPart(cp);
        //}

        ///**
        // * Compatibility override of Java method, delegates to
        // * java.lang.Character.isJavaIdentifierStart.
        // * @param cp the code point
        // * @return true if the code point can start a java identifier.
        // * @stable ICU 3.4
        // */
        //public static bool IsJavaIdentifierStart(int cp)
        //{
        //    throw new NotImplementedException();
        //    // note, downcast to char for jdk 1.4 compatibility
        //    //return Character.IsJavaIdentifierStart((char)cp);
        //}

        ///**
        // * Compatibility override of Java method, delegates to
        // * java.lang.Character.isJavaIdentifierPart.
        // * @param cp the code point
        // * @return true if the code point can continue a java identifier.
        // * @stable ICU 3.4
        // */
        //public static bool IsJavaIdentifierPart(int cp)
        //{
        //    throw new NotImplementedException();
        //    // note, downcast to char for jdk 1.4 compatibility
        //    //return Character.IsJavaIdentifierPart((char)cp);
        //}

        /**
         * Determines if the specified code point is a lowercase character.
         * UnicodeData only contains case mappings for code points where they are
         * one-to-one mappings; it also omits information about context-sensitive
         * case mappings.<br> For more information about Unicode case mapping
         * please refer to the
         * <a href=http://www.unicode.org/unicode/reports/tr21/>Technical report
         * #21</a>.<br>
         * Up-to-date Unicode implementation of java.lang.Character.isLowerCase()
         * @param ch code point to determine if it is in lowercase
         * @return true if code point is a lowercase character
         * @stable ICU 2.1
         */
        public static bool IsLowerCase(int ch)
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UnicodeCategory.LowercaseLetter;
        }

        /**
         * Determines if the specified code point is a white space character.
         * A code point is considered to be an whitespace character if and only
         * if it satisfies one of the following criteria:
         * <ul>
         * <li> It is a Unicode Separator character (categories "Z" = "Zs" or "Zl" or "Zp"), but is not
         *      also a non-breaking space (&#92;u00A0 or &#92;u2007 or &#92;u202F).
         * <li> It is &#92;u0009, HORIZONTAL TABULATION.
         * <li> It is &#92;u000A, LINE FEED.
         * <li> It is &#92;u000B, VERTICAL TABULATION.
         * <li> It is &#92;u000C, FORM FEED.
         * <li> It is &#92;u000D, CARRIAGE RETURN.
         * <li> It is &#92;u001C, FILE SEPARATOR.
         * <li> It is &#92;u001D, GROUP SEPARATOR.
         * <li> It is &#92;u001E, RECORD SEPARATOR.
         * <li> It is &#92;u001F, UNIT SEPARATOR.
         * </ul>
         *
         * This API tries to sync with the semantics of Java's
         * java.lang.Character.isWhitespace(), but it may not return
         * the exact same results because of the Unicode version
         * difference.
         * <p>Note: Unicode 4.0.1 changed U+200B ZERO WIDTH SPACE from a Space Separator (Zs)
         * to a Format Control (Cf). Since then, isWhitespace(0x200b) returns false.
         * See http://www.unicode.org/versions/Unicode4.0.1/
         * @param ch code point to determine if it is a white space
         * @return true if the specified code point is a white space character
         * @stable ICU 2.1
         */
        public static bool IsWhitespace(int ch) // ICU4N TODO: API Rename IsWhiteSpace (for consistency with .NET)
        {
            // exclude no-break spaces
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToIcuValue()) &
                    ((1 << UnicodeCategory.SpaceSeparator.ToIcuValue())
                            | (1 << UnicodeCategory.LineSeparator.ToIcuValue())
                            | (1 << UnicodeCategory.ParagraphSeparator.ToIcuValue()))) != 0
                            && (ch != NO_BREAK_SPACE_) && (ch != FIGURE_SPACE_) && (ch != NARROW_NO_BREAK_SPACE_)
                            // TAB VT LF FF CR FS GS RS US NL are all control characters
                            // that are white spaces.
                            || (ch >= 0x9 && ch <= 0xd) || (ch >= 0x1c && ch <= 0x1f);
        }

        /**
         * Determines if the specified code point is a Unicode specified space
         * character, i.e. if code point is in the category Zs, Zl and Zp.
         * Up-to-date Unicode implementation of java.lang.Character.isSpaceChar().
         * @param ch code point to determine if it is a space
         * @return true if the specified code point is a space character
         * @stable ICU 2.1
         */
        public static bool IsSpaceChar(int ch)
        {
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToIcuValue()) & ((1 << UnicodeCategory.SpaceSeparator.ToIcuValue())
                    | (1 << UnicodeCategory.LineSeparator.ToIcuValue())
                    | (1 << UnicodeCategory.ParagraphSeparator.ToIcuValue())))
                    != 0;
        }

        /**
         * Determines if the specified code point is a titlecase character.
         * UnicodeData only contains case mappings for code points where they are
         * one-to-one mappings; it also omits information about context-sensitive
         * case mappings.<br>
         * For more information about Unicode case mapping please refer to the
         * <a href=http://www.unicode.org/unicode/reports/tr21/>
         * Technical report #21</a>.<br>
         * Up-to-date Unicode implementation of java.lang.Character.isTitleCase().
         * @param ch code point to determine if it is in title case
         * @return true if the specified code point is a titlecase character
         * @stable ICU 2.1
         */
        public static bool IsTitleCase(int ch)
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UnicodeCategory.TitlecaseLetter;
        }

        /**
         * Determines if the specified code point may be any part of a Unicode
         * identifier other than the starting character.
         * A code point may be part of a Unicode identifier if and only if it is
         * one of the following:
         * <ul>
         * <li> Lu Uppercase letter
         * <li> Ll Lowercase letter
         * <li> Lt Titlecase letter
         * <li> Lm Modifier letter
         * <li> Lo Other letter
         * <li> Nl Letter number
         * <li> Pc Connecting punctuation character
         * <li> Nd decimal number
         * <li> Mc Spacing combining mark
         * <li> Mn Non-spacing mark
         * <li> Cf formatting code
         * </ul>
         * Up-to-date Unicode implementation of
         * java.lang.Character.isUnicodeIdentifierPart().<br>
         * See <a href=http://www.unicode.org/unicode/reports/tr8/>UTR #8</a>.
         * @param ch code point to determine if is can be part of a Unicode
         *        identifier
         * @return true if code point is any character belonging a unicode
         *         identifier suffix after the first character
         * @stable ICU 2.1
         */
        public static bool IsUnicodeIdentifierPart(int ch)
        {
            // if props == 0, it will just fall through and return false
            // cat == format
            return ((1 << GetType(ch).ToIcuValue())
                    & ((1 << UnicodeCategory.UppercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LowercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.TitlecaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.ModifierLetter.ToIcuValue())
                            | (1 << UnicodeCategory.OtherLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LetterNumber.ToIcuValue())
                            | (1 << UnicodeCategory.ConnectorPunctuation.ToIcuValue())
                            | (1 << UnicodeCategory.DecimalDigitNumber.ToIcuValue())
                            | (1 << UnicodeCategory.SpacingCombiningMark.ToIcuValue())
                            | (1 << UnicodeCategory.NonSpacingMark.ToIcuValue()))) != 0
                            || IsIdentifierIgnorable(ch);
        }

        /**
         * Determines if the specified code point is permissible as the first
         * character in a Unicode identifier.
         * A code point may start a Unicode identifier if it is of type either
         * <ul>
         * <li> Lu Uppercase letter
         * <li> Ll Lowercase letter
         * <li> Lt Titlecase letter
         * <li> Lm Modifier letter
         * <li> Lo Other letter
         * <li> Nl Letter number
         * </ul>
         * Up-to-date Unicode implementation of
         * java.lang.Character.isUnicodeIdentifierStart().<br>
         * See <a href=http://www.unicode.org/unicode/reports/tr8/>UTR #8</a>.
         * @param ch code point to determine if it can start a Unicode identifier
         * @return true if code point is the first character belonging a unicode
         *              identifier
         * @stable ICU 2.1
         */
        public static bool IsUnicodeIdentifierStart(int ch)
        {
            /*int cat = getType(ch);*/
            // if props == 0, it will just fall through and return false
            return ((1 << GetType(ch).ToIcuValue())
                    & ((1 << UnicodeCategory.UppercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LowercaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.TitlecaseLetter.ToIcuValue())
                            | (1 << UnicodeCategory.ModifierLetter.ToIcuValue())
                            | (1 << UnicodeCategory.OtherLetter.ToIcuValue())
                            | (1 << UnicodeCategory.LetterNumber.ToIcuValue()))) != 0;
        }

        /**
         * Determines if the specified code point should be regarded as an
         * ignorable character in a Java identifier.
         * A character is Java-identifier-ignorable if it has the general category
         * Cf Formatting Control, or it is a non-Java-whitespace ISO control:
         * U+0000..U+0008, U+000E..U+001B, U+007F..U+009F.<br>
         * Up-to-date Unicode implementation of
         * java.lang.Character.isIdentifierIgnorable().<br>
         * See <a href=http://www.unicode.org/unicode/reports/tr8/>UTR #8</a>.
         * <p>Note that Unicode just recommends to ignore Cf (format controls).
         * @param ch code point to be determined if it can be ignored in a Unicode
         *        identifier.
         * @return true if the code point is ignorable
         * @stable ICU 2.1
         */
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
            return GetType(ch) == UnicodeCategory.Format;
        }

        /**
         * Determines if the specified code point is an uppercase character.
         * UnicodeData only contains case mappings for code point where they are
         * one-to-one mappings; it also omits information about context-sensitive
         * case mappings.<br>
         * For language specific case conversion behavior, use
         * toUpperCase(locale, str). <br>
         * For example, the case conversion for dot-less i and dotted I in Turkish,
         * or for final sigma in Greek.
         * For more information about Unicode case mapping please refer to the
         * <a href=http://www.unicode.org/unicode/reports/tr21/>
         * Technical report #21</a>.<br>
         * Up-to-date Unicode implementation of java.lang.Character.isUpperCase().
         * @param ch code point to determine if it is in uppercase
         * @return true if the code point is an uppercase character
         * @stable ICU 2.1
         */
        public static bool IsUpperCase(int ch)
        {
            // if props == 0, it will just fall through and return false
            return GetType(ch) == UnicodeCategory.UppercaseLetter;
        }

        /**
         * The given code point is mapped to its lowercase equivalent; if the code
         * point has no lowercase equivalent, the code point itself is returned.
         * Up-to-date Unicode implementation of java.lang.Character.toLowerCase()
         *
         * <p>This function only returns the simple, single-code point case mapping.
         * Full case mappings should be used whenever possible because they produce
         * better results by working on whole strings.
         * They take into account the string context and the language and can map
         * to a result string with a different length as appropriate.
         * Full case mappings are applied by the case mapping functions
         * that take string parameters rather than code points (int).
         * See also the User Guide chapter on C/POSIX migration:
         * http://www.icu-project.org/userguide/posix.html#case_mappings
         *
         * @param ch code point whose lowercase equivalent is to be retrieved
         * @return the lowercase equivalent code point
         * @stable ICU 2.1
         */
        public static int ToLower(int ch)
        {
            return UCaseProps.INSTANCE.ToLower(ch);
        }

        /**
         * Converts argument code point and returns a string object representing
         * the code point's value in UTF-16 format.
         * The result is a string whose length is 1 for BMP code points, 2 for supplementary ones.
         *
         * <p>Up-to-date Unicode implementation of java.lang.Character.toString().
         *
         * @param ch code point
         * @return string representation of the code point, null if code point is not
         *         defined in unicode
         * @stable ICU 2.1
         */
        public static string ToString(int ch)
        {
            if (ch < MIN_VALUE || ch > MAX_VALUE)
            {
                return null;
            }

            if (ch < SUPPLEMENTARY_MIN_VALUE)
            {
                return new string(new char[] { (char)ch });
            }

            return new string(Character.ToChars(ch));
        }

        /**
         * Converts the code point argument to titlecase.
         * If no titlecase is available, the uppercase is returned. If no uppercase
         * is available, the code point itself is returned.
         * Up-to-date Unicode implementation of java.lang.Character.toTitleCase()
         *
         * <p>This function only returns the simple, single-code point case mapping.
         * Full case mappings should be used whenever possible because they produce
         * better results by working on whole strings.
         * They take into account the string context and the language and can map
         * to a result string with a different length as appropriate.
         * Full case mappings are applied by the case mapping functions
         * that take string parameters rather than code points (int).
         * See also the User Guide chapter on C/POSIX migration:
         * http://www.icu-project.org/userguide/posix.html#case_mappings
         *
         * @param ch code point  whose title case is to be retrieved
         * @return titlecase code point
         * @stable ICU 2.1
         */
        public static int ToTitleCase(int ch)
        {
            return UCaseProps.INSTANCE.ToTitle(ch);
        }

        /**
         * Converts the character argument to uppercase.
         * If no uppercase is available, the character itself is returned.
         * Up-to-date Unicode implementation of java.lang.Character.toUpperCase()
         *
         * <p>This function only returns the simple, single-code point case mapping.
         * Full case mappings should be used whenever possible because they produce
         * better results by working on whole strings.
         * They take into account the string context and the language and can map
         * to a result string with a different length as appropriate.
         * Full case mappings are applied by the case mapping functions
         * that take string parameters rather than code points (int).
         * See also the User Guide chapter on C/POSIX migration:
         * http://www.icu-project.org/userguide/posix.html#case_mappings
         *
         * @param ch code point whose uppercase is to be retrieved
         * @return uppercase code point
         * @stable ICU 2.1
         */
        public static int ToUpper(int ch)
        {
            return UCaseProps.INSTANCE.ToUpper(ch);
        }

        // extra methods not in java.lang.Character --------------------------

        /**
         * {@icu} Determines if the code point is a supplementary character.
         * A code point is a supplementary character if and only if it is greater
         * than <a href=#SUPPLEMENTARY_MIN_VALUE>SUPPLEMENTARY_MIN_VALUE</a>
         * @param ch code point to be determined if it is in the supplementary
         *        plane
         * @return true if code point is a supplementary character
         * @stable ICU 2.1
         */
        public static bool IsSupplementary(int ch)
        {
            return ch >= UCharacter.SUPPLEMENTARY_MIN_VALUE &&
                    ch <= UCharacter.MAX_VALUE;
        }

        /**
         * {@icu} Determines if the code point is in the BMP plane.
         * @param ch code point to be determined if it is not a supplementary
         *        character
         * @return true if code point is not a supplementary character
         * @stable ICU 2.1
         */
        public static bool IsBMP(int ch)
        {
            return (ch >= 0 && ch <= LAST_CHAR_MASK_);
        }

        /**
         * {@icu} Determines whether the specified code point is a printable character
         * according to the Unicode standard.
         * @param ch code point to be determined if it is printable
         * @return true if the code point is a printable character
         * @stable ICU 2.1
         */
        public static bool IsPrintable(int ch)
        {
            UnicodeCategory cat = GetType(ch);
            // if props == 0, it will just fall through and return false
            return (cat != UnicodeCategory.OtherNotAssigned &&
                    cat != UnicodeCategory.Control &&
                    cat != UnicodeCategory.Format &&
                    cat != UnicodeCategory.PrivateUse &&
                    cat != UnicodeCategory.Surrogate);
        }

        /**
         * {@icu} Determines whether the specified code point is of base form.
         * A code point of base form does not graphically combine with preceding
         * characters, and is neither a control nor a format character.
         * @param ch code point to be determined if it is of base form
         * @return true if the code point is of base form
         * @stable ICU 2.1
         */
        public static bool IsBaseForm(int ch)
        {
            UnicodeCategory cat = GetType(ch);
            // if props == 0, it will just fall through and return false
            return cat == UnicodeCategory.DecimalDigitNumber ||
                    cat == UnicodeCategory.OtherNumber ||
                    cat == UnicodeCategory.LetterNumber ||
                    cat == UnicodeCategory.UppercaseLetter ||
                    cat == UnicodeCategory.LowercaseLetter ||
                    cat == UnicodeCategory.TitlecaseLetter ||
                    cat == UnicodeCategory.ModifierLetter ||
                    cat == UnicodeCategory.OtherLetter ||
                    cat == UnicodeCategory.NonSpacingMark ||
                    cat == UnicodeCategory.EnclosingMark ||
                    cat == UnicodeCategory.SpacingCombiningMark;
        }

        /**
         * {@icu} Returns the Bidirection property of a code point.
         * For example, 0x0041 (letter A) has the LEFT_TO_RIGHT directional
         * property.<br>
         * Result returned belongs to the interface
         * <a href=UCharacterDirection.html>UCharacterDirection</a>
         * @param ch the code point to be determined its direction
         * @return direction constant from UCharacterDirection.
         * @stable ICU 2.1
         */
        public static int GetDirection(int ch)
        {
            return UBiDiProps.INSTANCE.GetClass(ch);
        }

        /**
         * Determines whether the code point has the "mirrored" property.
         * This property is set for characters that are commonly used in
         * Right-To-Left contexts and need to be displayed with a "mirrored"
         * glyph.
         * @param ch code point whose mirror is to be determined
         * @return true if the code point has the "mirrored" property
         * @stable ICU 2.1
         */
        public static bool IsMirrored(int ch)
        {
            return UBiDiProps.INSTANCE.IsMirrored(ch);
        }

        /**
         * {@icu} Maps the specified code point to a "mirror-image" code point.
         * For code points with the "mirrored" property, implementations sometimes
         * need a "poor man's" mapping to another code point such that the default
         * glyph may serve as the mirror-image of the default glyph of the
         * specified code point.<br>
         * This is useful for text conversion to and from codepages with visual
         * order, and for displays without glyph selection capabilities.
         * @param ch code point whose mirror is to be retrieved
         * @return another code point that may serve as a mirror-image substitute,
         *         or ch itself if there is no such mapping or ch does not have the
         *         "mirrored" property
         * @stable ICU 2.1
         */
        public static int GetMirror(int ch)
        {
            return UBiDiProps.INSTANCE.GetMirror(ch);
        }

        /**
         * {@icu} Maps the specified character to its paired bracket character.
         * For Bidi_Paired_Bracket_Type!=None, this is the same as getMirror(int).
         * Otherwise c itself is returned.
         * See http://www.unicode.org/reports/tr9/
         *
         * @param c the code point to be mapped
         * @return the paired bracket code point,
         *         or c itself if there is no such mapping
         *         (Bidi_Paired_Bracket_Type=None)
         *
         * @see UProperty#BIDI_PAIRED_BRACKET
         * @see UProperty#BIDI_PAIRED_BRACKET_TYPE
         * @see #getMirror(int)
         * @stable ICU 52
         */
        public static int GetBidiPairedBracket(int c)
        {
            return UBiDiProps.INSTANCE.GetPairedBracket(c);
        }

        /**
         * {@icu} Returns the combining class of the argument codepoint
         * @param ch code point whose combining is to be retrieved
         * @return the combining class of the codepoint
         * @stable ICU 2.1
         */
        public static int GetCombiningClass(int ch)
        {
            return Normalizer2.GetNFDInstance().GetCombiningClass(ch);
        }

        /**
         * {@icu} A code point is illegal if and only if
         * <ul>
         * <li> Out of bounds, less than 0 or greater than UCharacter.MAX_VALUE
         * <li> A surrogate value, 0xD800 to 0xDFFF
         * <li> Not-a-character, having the form 0x xxFFFF or 0x xxFFFE
         * </ul>
         * Note: legal does not mean that it is assigned in this version of Unicode.
         * @param ch code point to determine if it is a legal code point by itself
         * @return true if and only if legal.
         * @stable ICU 2.1
         */
        public static bool IsLegal(int ch)
        {
            if (ch < MIN_VALUE)
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
            return (ch <= MAX_VALUE);
        }

        /**
         * {@icu} A string is legal iff all its code points are legal.
         * A code point is illegal if and only if
         * <ul>
         * <li> Out of bounds, less than 0 or greater than UCharacter.MAX_VALUE
         * <li> A surrogate value, 0xD800 to 0xDFFF
         * <li> Not-a-character, having the form 0x xxFFFF or 0x xxFFFE
         * </ul>
         * Note: legal does not mean that it is assigned in this version of Unicode.
         * @param str containing code points to examin
         * @return true if and only if legal.
         * @stable ICU 2.1
         */
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

        /**
         * {@icu} Returns the version of Unicode data used.
         * @return the unicode version number used
         * @stable ICU 2.1
         */
        public static VersionInfo GetUnicodeVersion()
        {
            return UCharacterProperty.INSTANCE.UnicodeVersion;
        }

        /**
         * {@icu} Returns the most current Unicode name of the argument code point, or
         * null if the character is unassigned or outside the range
         * UCharacter.MIN_VALUE and UCharacter.MAX_VALUE or does not have a name.
         * <br>
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param ch the code point for which to get the name
         * @return most current Unicode name
         * @stable ICU 2.1
         */
        public static string GetName(int ch)
        {
            return UCharacterName.INSTANCE.GetName(ch, UCharacterNameChoice.UNICODE_CHAR_NAME);
        }

        /**
         * {@icu} Returns the names for each of the characters in a string
         * @param s string to format
         * @param separator string to go between names
         * @return string of names
         * @stable ICU 3.8
         */
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
                var name = UCharacter.GetName(cp);
                sb.Append(name == null ? "null" : name);
            }
            return sb.ToString();
        }

        /**
         * {@icu} Returns null.
         * Used to return the Unicode_1_Name property value which was of little practical value.
         * @param ch the code point for which to get the name
         * @return null
         * @deprecated ICU 49
         */
        [Obsolete]
        public static string GetName1_0(int ch)
        {
            return null;
        }

        /**
         * {@icu} Returns a name for a valid codepoint. Unlike, getName(int) and
         * getName1_0(int), this method will return a name even for codepoints that
         * are not assigned a name in UnicodeData.txt.
         *
         * <p>The names are returned in the following order.
         * <ul>
         * <li> Most current Unicode name if there is any
         * <li> Unicode 1.0 name if there is any
         * <li> Extended name in the form of
         *      "&lt;codepoint_type-codepoint_hex_digits&gt;". E.g., &lt;noncharacter-fffe&gt;
         * </ul>
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param ch the code point for which to get the name
         * @return a name for the argument codepoint
         * @stable ICU 2.6
         */
        public static string GetExtendedName(int ch)
        {
            return UCharacterName.INSTANCE.GetName(ch, UCharacterNameChoice.EXTENDED_CHAR_NAME);
        }

        /**
         * {@icu} Returns the corrected name from NameAliases.txt if there is one.
         * Returns null if the character is unassigned or outside the range
         * UCharacter.MIN_VALUE and UCharacter.MAX_VALUE or does not have a name.
         * <br>
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param ch the code point for which to get the name alias
         * @return Unicode name alias, or null
         * @stable ICU 4.4
         */
        public static string GetNameAlias(int ch)
        {
            return UCharacterName.INSTANCE.GetName(ch, UCharacterNameChoice.CHAR_NAME_ALIAS);
        }

        /**
         * {@icu} Returns null.
         * Used to return the ISO 10646 comment for a character.
         * The Unicode ISO_Comment property is deprecated and has no values.
         *
         * @param ch The code point for which to get the ISO comment.
         *           It must be the case that {@code 0 <= ch <= 0x10ffff}.
         * @return null
         * @deprecated ICU 49
         */
        [Obsolete]
        public static string GetISOComment(int ch)
        {
            return null;
        }

        /**
         * {@icu} <p>Finds a Unicode code point by its most current Unicode name and
         * return its code point value. All Unicode names are in uppercase.
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param name most current Unicode character name whose code point is to
         *        be returned
         * @return code point or -1 if name is not found
         * @stable ICU 2.1
         */
        public static int GetCharFromName(string name)
        {
            return UCharacterName.INSTANCE.GetCharFromName(
                    UCharacterNameChoice.UNICODE_CHAR_NAME, name);
        }

        /**
         * {@icu} Returns -1.
         * <p>Used to find a Unicode character by its version 1.0 Unicode name and return
         * its code point value.
         * @param name Unicode 1.0 code point name whose code point is to be
         *             returned
         * @return -1
         * @deprecated ICU 49
         * @see #getName1_0(int)
         */
        [Obsolete]
        public static int GetCharFromName1_0(string name)
        {
            return -1;
        }

        /**
         * {@icu} <p>Find a Unicode character by either its name and return its code
         * point value. All Unicode names are in uppercase.
         * Extended names are all lowercase except for numbers and are contained
         * within angle brackets.
         * The names are searched in the following order
         * <ul>
         * <li> Most current Unicode name if there is any
         * <li> Unicode 1.0 name if there is any
         * <li> Extended name in the form of
         *      "&lt;codepoint_type-codepoint_hex_digits&gt;". E.g. &lt;noncharacter-FFFE&gt;
         * </ul>
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param name codepoint name
         * @return code point associated with the name or -1 if the name is not
         *         found.
         * @stable ICU 2.6
         */
        public static int GetCharFromExtendedName(string name)
        {
            return UCharacterName.INSTANCE.GetCharFromName(
                    UCharacterNameChoice.EXTENDED_CHAR_NAME, name);
        }

        /**
         * {@icu} <p>Find a Unicode character by its corrected name alias and return
         * its code point value. All Unicode names are in uppercase.
         * Note calling any methods related to code point names, e.g. get*Name*()
         * incurs a one-time initialisation cost to construct the name tables.
         * @param name Unicode name alias whose code point is to be returned
         * @return code point or -1 if name is not found
         * @stable ICU 4.4
         */
        public static int GetCharFromNameAlias(string name)
        {
            return UCharacterName.INSTANCE.GetCharFromName(UCharacterNameChoice.CHAR_NAME_ALIAS, name);
        }

        /// <summary>
        /// Return the Unicode name for a given property, as given in the
        /// Unicode database file PropertyAliases.txt.  Most properties
        /// have more than one name.  The <paramref name="nameChoice"/> determines which one
        /// is returned.
        /// </summary>
        /// <remarks>
        /// In addition, this function maps the property
        /// <see cref="UProperty.GENERAL_CATEGORY_MASK"/> to the synthetic names "gcm" /
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
            return UPropertyAliases.INSTANCE.GetPropertyName(property, nameChoice);
        }

        /// <summary>
        /// Return the Unicode name for a given property, as given in the
        /// Unicode database file PropertyAliases.txt.  Most properties
        /// have more than one name.  The <paramref name="nameChoice"/> determines which one
        /// is returned.
        /// </summary>
        /// <remarks>
        /// In addition, this function maps the property
        /// <see cref="UProperty.GENERAL_CATEGORY_MASK"/> to the synthetic names "gcm" /
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
            return UPropertyAliases.INSTANCE.TryGetPropertyName(property, nameChoice, out result);
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
        /// retrieved using <see cref="UProperty.GENERAL_CATEGORY_MASK"/>, not
        /// <see cref="UProperty.GENERAL_CATEGORY"/>.  These include: "C" / "Other", "L" /
        /// "Letter", "LC" / "Cased_Letter", "M" / "Mark", "N" / "Number", "P"
        /// / "Punctuation", "S" / "Symbol", and "Z" / "Separator".
        /// </remarks>
        /// <param name="property">
        /// <see cref="UProperty"/> selector constant.
        /// <see cref="UProperty.INT_START"/> &lt;= property &lt; <see cref="UProperty.INT_LIMIT"/> or
        /// <see cref="UProperty.BINARY_START"/> &lt;= property &lt; <see cref="UProperty.BINARY_LIMIT"/> or
        /// <see cref="UProperty.MASK_START"/> &lt; = property &lt; <see cref="UProperty.MASK_LIMIT"/>.
        /// If out of range, null is returned.
        /// </param>
        /// <param name="value">
        /// Selector for a value for the given property.  In
        /// general, valid values range from 0 up to some maximum.  There
        /// are a few exceptions:
        /// <list type="number">
        ///     <item><desription>
        ///         <see cref="UProperty.BLOCK"/> values begin at the
        ///         non-zero value <see cref="UCharacter.UnicodeBlock.BASIC_LATIN.ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.CANONICAL_COMBINING_CLASS"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.GENERAL_CATEGORY_MASK"/> values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UCharacter.GetType(int)"/>.GetIcuValue().  This allows grouped categories such as
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
            if ((property == UProperty.CANONICAL_COMBINING_CLASS
                    || property == UProperty.LEAD_CANONICAL_COMBINING_CLASS
                    || property == UProperty.TRAIL_CANONICAL_COMBINING_CLASS)
                    && value >= UCharacter.GetIntPropertyMinValue(
                            UProperty.CANONICAL_COMBINING_CLASS)
                            && value <= UCharacter.GetIntPropertyMaxValue(
                                    UProperty.CANONICAL_COMBINING_CLASS)
                                    && nameChoice >= 0 && nameChoice < NameChoice.Count)
            {
                // this is hard coded for the valid cc
                // because PropertyValueAliases.txt does not contain all of them
                try
                {
                    return UPropertyAliases.INSTANCE.GetPropertyValueName(property, value,
                            nameChoice);
                }
                catch (ArgumentException e)
                {
                    return null;
                }
            }
            return UPropertyAliases.INSTANCE.GetPropertyValueName(property, value, nameChoice);
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
        /// retrieved using <see cref="UProperty.GENERAL_CATEGORY_MASK"/>, not
        /// <see cref="UProperty.GENERAL_CATEGORY"/>.  These include: "C" / "Other", "L" /
        /// "Letter", "LC" / "Cased_Letter", "M" / "Mark", "N" / "Number", "P"
        /// / "Punctuation", "S" / "Symbol", and "Z" / "Separator".
        /// </remarks>
        /// <param name="property">
        /// <see cref="UProperty"/> selector constant.
        /// <see cref="UProperty.INT_START"/> &lt;= property &lt; <see cref="UProperty.INT_LIMIT"/> or
        /// <see cref="UProperty.BINARY_START"/> &lt;= property &lt; <see cref="UProperty.BINARY_LIMIT"/> or
        /// <see cref="UProperty.MASK_START"/> &lt; = property &lt; <see cref="UProperty.MASK_LIMIT"/>.
        /// If out of range, null is returned.
        /// </param>
        /// <param name="value">
        /// Selector for a value for the given property.  In
        /// general, valid values range from 0 up to some maximum.  There
        /// are a few exceptions:
        /// <list type="number">
        ///     <item><desription>
        ///         <see cref="UProperty.BLOCK"/> values begin at the
        ///         non-zero value <see cref="UCharacter.UnicodeBlock.BASIC_LATIN.ID"/>.
        ///     </desription></item>
        ///     <item><desription>
        ///         <see cref="UProperty.CANONICAL_COMBINING_CLASS"/> values are not contiguous
        ///         and range from 0..240.
        ///     </desription></item>
        ///     <item><desription>
        ///         UProperty.GENERAL_CATEGORY_MASK values
        ///         are mask values produced by left-shifting 1 by
        ///         <see cref="UCharacter.GetType(int)"/>.  This allows grouped categories such as
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
            if ((property == UProperty.CANONICAL_COMBINING_CLASS
                    || property == UProperty.LEAD_CANONICAL_COMBINING_CLASS
                    || property == UProperty.TRAIL_CANONICAL_COMBINING_CLASS)
                    && value >= UCharacter.GetIntPropertyMinValue(
                            UProperty.CANONICAL_COMBINING_CLASS)
                            && value <= UCharacter.GetIntPropertyMaxValue(
                                    UProperty.CANONICAL_COMBINING_CLASS)
                                    && nameChoice >= 0 && nameChoice < NameChoice.Count)
            {
                // this is hard coded for the valid cc
                // because PropertyValueAliases.txt does not contain all of them
                if (!UPropertyAliases.INSTANCE.TryGetPropertyValueName(property, value, nameChoice, out result))
                {
                    result = null;
                }
                return true;
            }
            return UPropertyAliases.INSTANCE.TryGetPropertyValueName(property, value, nameChoice, out result);
        }

        // ICU4N specific - GetPropertyValueEnum(UProperty property, ICharSequence valueAlias) moved to UCharacterExtension.tt

        /// <summary>
        /// Same as <see cref="GetPropertyValueEnum(int, ICharSequence)"/>, except doesn't throw exception. Instead, returns <see cref="UProperty.UNDEFINED"/>.
        /// </summary>
        /// <param name="property">Same as <see cref="GetPropertyValueEnum(int, ICharSequence)"/>.</param>
        /// <param name="valueAlias">Same as <see cref="GetPropertyValueEnum(int, ICharSequence)"/>.</param>
        /// <returns>Returns <see cref="UProperty.UNDEFINED"/> if the value is not valid, otherwise the value.</returns>
        [Obsolete("ICU4N 60.1.0 Use TryGetPropertyValueEnum(UProperty property, ICharSequence valueAlias) instead.")]
        internal static int GetPropertyValueEnumNoThrow(UProperty property, ICharSequence valueAlias)
        {
            return UPropertyAliases.INSTANCE.GetPropertyValueEnumNoThrow((int)property, valueAlias);
        }


        /**
         * {@icu} Returns a code point corresponding to the two surrogate code units.
         *
         * @param lead the lead char
         * @param trail the trail char
         * @return code point if surrogate characters are valid.
         * @exception IllegalArgumentException thrown when the code units do
         *            not form a valid code point
         * @stable ICU 2.1
         */
        public static int GetCodePoint(char lead, char trail)
        {
            if (char.IsSurrogatePair(lead, trail))
            {
                return Character.ToCodePoint(lead, trail);
            }
            throw new ArgumentException("Illegal surrogate characters");
        }

        /**
         * {@icu} Returns the code point corresponding to the BMP code point.
         *
         * @param char16 the BMP code point
         * @return code point if argument is a valid character.
         * @exception IllegalArgumentException thrown when char16 is not a valid
         *            code point
         * @stable ICU 2.1
         */
        public static int GetCodePoint(char char16)
        {
            if (UCharacter.IsLegal(char16))
            {
                return char16;
            }
            throw new ArgumentException("Illegal codepoint");
        }

        /**
         * Returns the uppercase version of the argument string.
         * Casing is dependent on the default locale and context-sensitive.
         * @param str source string to be performed on
         * @return uppercase version of the argument string
         * @stable ICU 2.1
         */
        public static string ToUpper(string str)
        {
            return CaseMapImpl.ToUpper(GetDefaultCaseLocale(), 0, str);
        }

        /**
         * Returns the lowercase version of the argument string.
         * Casing is dependent on the default locale and context-sensitive
         * @param str source string to be performed on
         * @return lowercase version of the argument string
         * @stable ICU 2.1
         */
        public static string ToLower(string str)
        {
            return CaseMapImpl.ToLower(GetDefaultCaseLocale(), 0, str);
        }

        /**
         * <p>Returns the titlecase version of the argument string.
         * <p>Position for titlecasing is determined by the argument break
         * iterator, hence the user can customize his break iterator for
         * a specialized titlecasing. In this case only the forward iteration
         * needs to be implemented.
         * If the break iterator passed in is null, the default Unicode algorithm
         * will be used to determine the titlecase positions.
         *
         * <p>Only positions returned by the break iterator will be title cased,
         * character in between the positions will all be in lower case.
         * <p>Casing is dependent on the default locale and context-sensitive
         * @param str source string to be performed on
         * @param breakiter break iterator to determine the positions in which
         *        the character should be title cased.
         * @return lowercase version of the argument string
         * @stable ICU 2.6
         */
        public static string ToTitleCase(string str, BreakIterator breakiter)
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

        /**
         * Returns the uppercase version of the argument string.
         * Casing is dependent on the argument locale and context-sensitive.
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @return uppercase version of the argument string
         * @stable ICU 2.1
         */
        public static string ToUpper(CultureInfo locale, string str)
        {
            return CaseMapImpl.ToUpper(GetCaseLocale(locale), 0, str);
        }

        /**
         * Returns the uppercase version of the argument string.
         * Casing is dependent on the argument locale and context-sensitive.
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @return uppercase version of the argument string
         * @stable ICU 3.2
         */
        public static string ToUpper(ULocale locale, string str)
        {
            return CaseMapImpl.ToUpper(GetCaseLocale(locale), 0, str);
        }

        /**
         * Returns the lowercase version of the argument string.
         * Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @return lowercase version of the argument string
         * @stable ICU 2.1
         */
        public static string ToLower(CultureInfo locale, string str)
        {
            return CaseMapImpl.ToLower(GetCaseLocale(locale), 0, str);
        }

        /**
         * Returns the lowercase version of the argument string.
         * Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @return lowercase version of the argument string
         * @stable ICU 3.2
         */
        public static string ToLower(ULocale locale, string str)
        {
            return CaseMapImpl.ToLower(GetCaseLocale(locale), 0, str);
        }

        /**
         * <p>Returns the titlecase version of the argument string.
         * <p>Position for titlecasing is determined by the argument break
         * iterator, hence the user can customize his break iterator for
         * a specialized titlecasing. In this case only the forward iteration
         * needs to be implemented.
         * If the break iterator passed in is null, the default Unicode algorithm
         * will be used to determine the titlecase positions.
         *
         * <p>Only positions returned by the break iterator will be title cased,
         * character in between the positions will all be in lower case.
         * <p>Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @param breakiter break iterator to determine the positions in which
         *        the character should be title cased.
         * @return lowercase version of the argument string
         * @stable ICU 2.6
         */
        public static string ToTitleCase(CultureInfo locale, string str,
                BreakIterator breakiter)
        {
            return ToTitleCase(locale, str, breakiter, 0);
        }

        /**
         * <p>Returns the titlecase version of the argument string.
         * <p>Position for titlecasing is determined by the argument break
         * iterator, hence the user can customize his break iterator for
         * a specialized titlecasing. In this case only the forward iteration
         * needs to be implemented.
         * If the break iterator passed in is null, the default Unicode algorithm
         * will be used to determine the titlecase positions.
         *
         * <p>Only positions returned by the break iterator will be title cased,
         * character in between the positions will all be in lower case.
         * <p>Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @param titleIter break iterator to determine the positions in which
         *        the character should be title cased.
         * @return lowercase version of the argument string
         * @stable ICU 3.2
         */
        public static string ToTitleCase(ULocale locale, string str,
                BreakIterator titleIter)
        {
            return ToTitleCase(locale, str, titleIter, 0);
        }

        /**
         * <p>Returns the titlecase version of the argument string.
         * <p>Position for titlecasing is determined by the argument break
         * iterator, hence the user can customize his break iterator for
         * a specialized titlecasing. In this case only the forward iteration
         * needs to be implemented.
         * If the break iterator passed in is null, the default Unicode algorithm
         * will be used to determine the titlecase positions.
         *
         * <p>Only positions returned by the break iterator will be title cased,
         * character in between the positions will all be in lower case.
         * <p>Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @param titleIter break iterator to determine the positions in which
         *        the character should be title cased.
         * @param options bit set to modify the titlecasing operation
         * @return lowercase version of the argument string
         * @stable ICU 3.8
         * @see #TITLECASE_NO_LOWERCASE
         * @see #TITLECASE_NO_BREAK_ADJUSTMENT
         */
        public static string ToTitleCase(ULocale locale, string str,
                BreakIterator titleIter, int options)
        {
            if (titleIter == null && locale == null)
            {
                locale = ULocale.GetDefault();
            }
            titleIter = CaseMapImpl.GetTitleBreakIterator(locale, options, titleIter);
            titleIter.SetText(str);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), options, titleIter, str);
        }

        /**
         * Return a string with just the first word titlecased, for menus and UI, etc. This does not affect most of the string,
         * and sometimes has no effect at all; the original string is returned whenever casing
         * would not be appropriate for the first word (such as for CJK characters or initial numbers).
         * Initial non-letters are skipped in order to find the character to change.
         * Characters past the first affected are left untouched: see also TITLECASE_NO_LOWERCASE.
         * <p>Examples:
         * <table border='1'><tr><th>Source</th><th>Result</th><th>Locale</th></tr>
         * <tr><td>anglo-American locale</td><td>Anglo-American locale</td></tr>
         * <tr><td>“contact us”</td><td>“Contact us”</td></tr>
         * <tr><td>49ers win!</td><td>49ers win!</td></tr>
         * <tr><td>丰(abc)</td><td>丰(abc)</td></tr>
         * <tr><td>«ijs»</td><td>«Ijs»</td></tr>
         * <tr><td>«ijs»</td><td>«IJs»</td><td>nl-BE</td></tr>
         * <tr><td>«ijs»</td><td>«İjs»</td><td>tr-DE</td></tr>
         * </table>
         * @param locale the locale for accessing exceptional behavior (eg for tr).
         * @param str the source string to change
         * @return the modified string, or the original if no modifications were necessary.
         * @internal
         * @deprecated 
         */
        [Obsolete("ICU internal only")]
        public static string ToTitleFirst(ULocale locale, string str)
        {
            // TODO: Remove this function. Inline it where it is called in CLDR.
            return TO_TITLE_WHOLE_STRING_NO_LOWERCASE.Apply(locale.ToLocale(), null, str);
        }

        private static readonly CaseMap.Title TO_TITLE_WHOLE_STRING_NO_LOWERCASE =
                CaseMap.ToTitle().WholeString().NoLowercase();

        /**
         * {@icu} <p>Returns the titlecase version of the argument string.
         * <p>Position for titlecasing is determined by the argument break
         * iterator, hence the user can customize his break iterator for
         * a specialized titlecasing. In this case only the forward iteration
         * needs to be implemented.
         * If the break iterator passed in is null, the default Unicode algorithm
         * will be used to determine the titlecase positions.
         *
         * <p>Only positions returned by the break iterator will be title cased,
         * character in between the positions will all be in lower case.
         * <p>Casing is dependent on the argument locale and context-sensitive
         * @param locale which string is to be converted in
         * @param str source string to be performed on
         * @param titleIter break iterator to determine the positions in which
         *        the character should be title cased.
         * @param options bit set to modify the titlecasing operation
         * @return lowercase version of the argument string
         * @see #TITLECASE_NO_LOWERCASE
         * @see #TITLECASE_NO_BREAK_ADJUSTMENT
         * @stable ICU 54
         */
        public static string ToTitleCase(CultureInfo locale, string str,
                BreakIterator titleIter,
                int options)
        {
            if (titleIter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            titleIter = CaseMapImpl.GetTitleBreakIterator(locale, options, titleIter);
            titleIter.SetText(str);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), options, titleIter, str);
        }

        /**
         * {@icu} The given character is mapped to its case folding equivalent according
         * to UnicodeData.txt and CaseFolding.txt; if the character has no case
         * folding equivalent, the character itself is returned.
         *
         * <p>This function only returns the simple, single-code point case mapping.
         * Full case mappings should be used whenever possible because they produce
         * better results by working on whole strings.
         * They can map to a result string with a different length as appropriate.
         * Full case mappings are applied by the case mapping functions
         * that take string parameters rather than code points (int).
         * See also the User Guide chapter on C/POSIX migration:
         * http://www.icu-project.org/userguide/posix.html#case_mappings
         *
         * @param ch             the character to be converted
         * @param defaultmapping Indicates whether the default mappings defined in
         *                       CaseFolding.txt are to be used, otherwise the
         *                       mappings for dotted I and dotless i marked with
         *                       'T' in CaseFolding.txt are included.
         * @return               the case folding equivalent of the character, if
         *                       any; otherwise the character itself.
         * @see                  #foldCase(string, boolean)
         * @stable ICU 2.1
         */
        public static int FoldCase(int ch, bool defaultmapping)
        {
            return FoldCase(ch, defaultmapping ? FOLD_CASE_DEFAULT : FOLD_CASE_EXCLUDE_SPECIAL_I);
        }

        /**
         * {@icu} The given string is mapped to its case folding equivalent according to
         * UnicodeData.txt and CaseFolding.txt; if any character has no case
         * folding equivalent, the character itself is returned.
         * "Full", multiple-code point case folding mappings are returned here.
         * For "simple" single-code point mappings use the API
         * foldCase(int ch, boolean defaultmapping).
         * @param str            the string to be converted
         * @param defaultmapping Indicates whether the default mappings defined in
         *                       CaseFolding.txt are to be used, otherwise the
         *                       mappings for dotted I and dotless i marked with
         *                       'T' in CaseFolding.txt are included.
         * @return               the case folding equivalent of the character, if
         *                       any; otherwise the character itself.
         * @see                  #foldCase(int, boolean)
         * @stable ICU 2.1
         */
        public static string FoldCase(string str, bool defaultmapping)
        {
            return FoldCase(str, defaultmapping ? FOLD_CASE_DEFAULT : FOLD_CASE_EXCLUDE_SPECIAL_I);
        }

        /**
         * {@icu} Option value for case folding: use default mappings defined in
         * CaseFolding.txt.
         * @stable ICU 2.6
         */
        public static readonly int FOLD_CASE_DEFAULT = 0x0000;
        /**
         * {@icu} Option value for case folding:
         * Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
         * and dotless i appropriately for Turkic languages (tr, az).
         *
         * <p>Before Unicode 3.2, CaseFolding.txt contains mappings marked with 'I' that
         * are to be included for default mappings and
         * excluded for the Turkic-specific mappings.
         *
         * <p>Unicode 3.2 CaseFolding.txt instead contains mappings marked with 'T' that
         * are to be excluded for default mappings and
         * included for the Turkic-specific mappings.
         *
         * @stable ICU 2.6
         */
        public static readonly int FOLD_CASE_EXCLUDE_SPECIAL_I = 0x0001;

        /**
         * {@icu} The given character is mapped to its case folding equivalent according
         * to UnicodeData.txt and CaseFolding.txt; if the character has no case
         * folding equivalent, the character itself is returned.
         *
         * <p>This function only returns the simple, single-code point case mapping.
         * Full case mappings should be used whenever possible because they produce
         * better results by working on whole strings.
         * They can map to a result string with a different length as appropriate.
         * Full case mappings are applied by the case mapping functions
         * that take string parameters rather than code points (int).
         * See also the User Guide chapter on C/POSIX migration:
         * http://www.icu-project.org/userguide/posix.html#case_mappings
         *
         * @param ch the character to be converted
         * @param options A bit set for special processing. Currently the recognised options
         * are FOLD_CASE_EXCLUDE_SPECIAL_I and FOLD_CASE_DEFAULT
         * @return the case folding equivalent of the character, if any; otherwise the
         * character itself.
         * @see #foldCase(string, boolean)
         * @stable ICU 2.6
         */
        public static int FoldCase(int ch, int options)
        {
            return UCaseProps.INSTANCE.Fold(ch, options);
        }

        /**
         * {@icu} The given string is mapped to its case folding equivalent according to
         * UnicodeData.txt and CaseFolding.txt; if any character has no case
         * folding equivalent, the character itself is returned.
         * "Full", multiple-code point case folding mappings are returned here.
         * For "simple" single-code point mappings use the API
         * foldCase(int ch, boolean defaultmapping).
         * @param str the string to be converted
         * @param options A bit set for special processing. Currently the recognised options
         *                are FOLD_CASE_EXCLUDE_SPECIAL_I and FOLD_CASE_DEFAULT
         * @return the case folding equivalent of the character, if any; otherwise the
         *         character itself.
         * @see #foldCase(int, boolean)
         * @stable ICU 2.6
         */
        public static string FoldCase(string str, int options)
        {
            return CaseMapImpl.Fold(options, str);
        }

        /**
         * {@icu} Returns the numeric value of a Han character.
         *
         * <p>This returns the value of Han 'numeric' code points,
         * including those for zero, ten, hundred, thousand, ten thousand,
         * and hundred million.
         * This includes both the standard and 'checkwriting'
         * characters, the 'big circle' zero character, and the standard
         * zero character.
         *
         * <p>Note: The Unicode Standard has numeric values for more
         * Han characters recognized by this method
         * (see {@link #getNumericValue(int)} and the UCD file DerivedNumericValues.txt),
         * and a {@link com.ibm.icu.text.NumberFormat} can be used with
         * a Chinese {@link com.ibm.icu.text.NumberingSystem}.
         *
         * @param ch code point to query
         * @return value if it is a Han 'numeric character,' otherwise return -1.
         * @stable ICU 2.4
         */
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

        /**
         * {@icu} <p>Returns an iterator for character types, iterating over codepoints.
         * <p>Example of use:<br>
         * <pre>
         * RangeValueIterator iterator = UCharacter.getTypeIterator();
         * RangeValueIterator.Element element = new RangeValueIterator.Element();
         * while (iterator.next(element)) {
         *     System.out.println("Codepoint \\u" +
         *                        Integer.toHexString(element.start) +
         *                        " to codepoint \\u" +
         *                        Integer.toHexString(element.limit - 1) +
         *                        " has the character type " +
         *                        element.value);
         * }
         * </pre>
         * @return an iterator
         * @stable ICU 2.6
         */
        public static IRangeValueIterator GetTypeIterator()
        {
            return new UCharacterTypeIterator();
        }

        private sealed class UCharacterTypeIterator : IRangeValueIterator
        {
            internal UCharacterTypeIterator()
            {
                Reset();
            }

            // implements RangeValueIterator
            public bool Next(RangeValueIteratorElement element)
            {
                if (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                {
                    element.Start = range.StartCodePoint;
                    element.Limit = range.EndCodePoint + 1;
                    element.Value = range.Value;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // implements RangeValueIterator
            public void Reset()
            {
                trieIterator = UCharacterProperty.INSTANCE.Trie.GetEnumerator(MASK_TYPE);
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

        /**
         * {@icu} <p>Returns an iterator for character names, iterating over codepoints.
         * <p>This API only gets the iterator for the modern, most up-to-date
         * Unicode names. For older 1.0 Unicode names use get1_0NameIterator() or
         * for extended names use getExtendedNameIterator().
         * <p>Example of use:<br>
         * <pre>
         * ValueIterator iterator = UCharacter.getNameIterator();
         * ValueIterator.Element element = new ValueIterator.Element();
         * while (iterator.next(element)) {
         *     System.out.println("Codepoint \\u" +
         *                        Integer.toHexString(element.codepoint) +
         *                        " has the name " + (string)element.value);
         * }
         * </pre>
         * <p>The maximal range which the name iterator iterates is from
         * UCharacter.MIN_VALUE to UCharacter.MAX_VALUE.
         * @return an iterator
         * @stable ICU 2.6
         */
        public static IValueIterator GetNameIterator()
        {
            return new UCharacterNameIterator(UCharacterName.INSTANCE,
                    UCharacterNameChoice.UNICODE_CHAR_NAME);
        }

        /**
         * {@icu} Returns an empty iterator.
         * <p>Used to return an iterator for the older 1.0 Unicode character names, iterating over codepoints.
         * @return an empty iterator
         * @deprecated 
         * @see #getName1_0(int)
         */
        [Obsolete("ICU 49")]
        public static IValueIterator GetName1_0Iterator()
        {
            return new DummyValueIterator();
        }

        private sealed class DummyValueIterator : IValueIterator
        {

            public bool Next(ValueIteratorElement element) { return false; }

            public void Reset() { }

            public void SetRange(int start, int limit) { }
        }

        /**
         * {@icu} <p>Returns an iterator for character names, iterating over codepoints.
         * <p>This API only gets the iterator for the extended names.
         * For modern, most up-to-date Unicode names use getNameIterator() or
         * for older 1.0 Unicode names use get1_0NameIterator().
         * <p>Example of use:<br>
         * <pre>
         * ValueIterator iterator = UCharacter.getExtendedNameIterator();
         * ValueIterator.Element element = new ValueIterator.Element();
         * while (iterator.next(element)) {
         *     System.out.println("Codepoint \\u" +
         *                        Integer.toHexString(element.codepoint) +
         *                        " has the name " + (string)element.value);
         * }
         * </pre>
         * <p>The maximal range which the name iterator iterates is from
         * @return an iterator
         * @stable ICU 2.6
         */
        public static IValueIterator GetExtendedNameIterator()
        {
            return new UCharacterNameIterator(UCharacterName.INSTANCE,
                    UCharacterNameChoice.EXTENDED_CHAR_NAME);
        }

        /**
         * {@icu} Returns the "age" of the code point.
         * <p>The "age" is the Unicode version when the code point was first
         * designated (as a non-character or for Private Use) or assigned a
         * character.
         * <p>This can be useful to avoid emitting code points to receiving
         * processes that do not accept newer characters.
         * <p>The data is from the UCD file DerivedAge.txt.
         * @param ch The code point.
         * @return the Unicode version number
         * @stable ICU 2.6
         */
        public static VersionInfo GetAge(int ch)
        {
            if (ch < MIN_VALUE || ch > MAX_VALUE)
            {
                throw new ArgumentException("Codepoint out of bounds");
            }
            return UCharacterProperty.INSTANCE.GetAge(ch);
        }

        /**
         * {@icu} <p>Check a binary Unicode property for a code point.
         * <p>Unicode, especially in version 3.2, defines many more properties
         * than the original set in UnicodeData.txt.
         * <p>This API is intended to reflect Unicode properties as defined in
         * the Unicode Character Database (UCD) and Unicode Technical Reports
         * (UTR).
         * <p>For details about the properties see
         * <a href=http://www.unicode.org/>http://www.unicode.org/</a>.
         * <p>For names of Unicode properties see the UCD file
         * PropertyAliases.txt.
         * <p>This API does not check the validity of the codepoint.
         * <p>Important: If ICU is built with UCD files from Unicode versions
         * below 3.2, then properties marked with "new" are not or
         * not fully available.
         * @param ch code point to test.
         * @param property selector constant from com.ibm.icu.lang.UProperty,
         *        identifies which binary property to check.
         * @return true or false according to the binary Unicode property value
         *         for ch. Also false if property is out of bounds or if the
         *         Unicode version does not have data for the property at all, or
         *         not for this code point.
         * @see com.ibm.icu.lang.UProperty
         * @stable ICU 2.6
         */
        public static bool HasBinaryProperty(int ch, UProperty property)
        {
            return UCharacterProperty.INSTANCE.HasBinaryProperty(ch, (int)property);
        }

        /**
         * {@icu} <p>Check if a code point has the Alphabetic Unicode property.
         * <p>Same as UCharacter.hasBinaryProperty(ch, UProperty.ALPHABETIC).
         * <p>Different from UCharacter.isLetter(ch)!
         * @stable ICU 2.6
         * @param ch codepoint to be tested
         */
        public static bool IsUAlphabetic(int ch)
        {
            return HasBinaryProperty(ch, UProperty.ALPHABETIC);
        }

        /**
         * {@icu} <p>Check if a code point has the Lowercase Unicode property.
         * <p>Same as UCharacter.hasBinaryProperty(ch, UProperty.LOWERCASE).
         * <p>This is different from UCharacter.isLowerCase(ch)!
         * @param ch codepoint to be tested
         * @stable ICU 2.6
         */
        public static bool IsULowercase(int ch)
        {
            return HasBinaryProperty(ch, UProperty.LOWERCASE);
        }

        /**
         * {@icu} <p>Check if a code point has the Uppercase Unicode property.
         * <p>Same as UCharacter.hasBinaryProperty(ch, UProperty.UPPERCASE).
         * <p>This is different from UCharacter.isUpperCase(ch)!
         * @param ch codepoint to be tested
         * @stable ICU 2.6
         */
        public static bool IsUUppercase(int ch)
        {
            return HasBinaryProperty(ch, UProperty.UPPERCASE);
        }

        /**
         * {@icu} <p>Check if a code point has the White_Space Unicode property.
         * <p>Same as UCharacter.hasBinaryProperty(ch, UProperty.WHITE_SPACE).
         * <p>This is different from both UCharacter.isSpace(ch) and
         * UCharacter.isWhitespace(ch)!
         * @param ch codepoint to be tested
         * @stable ICU 2.6
         */
        public static bool IsUWhiteSpace(int ch)
        {
            return HasBinaryProperty(ch, UProperty.WHITE_SPACE);
        }

        /**
         * {@icu} <p>Returns the property value for an Unicode property type of a code point.
         * Also returns binary and mask property values.
         * <p>Unicode, especially in version 3.2, defines many more properties than
         * the original set in UnicodeData.txt.
         * <p>The properties APIs are intended to reflect Unicode properties as
         * defined in the Unicode Character Database (UCD) and Unicode Technical
         * Reports (UTR). For details about the properties see
         * http://www.unicode.org/.
         * <p>For names of Unicode properties see the UCD file PropertyAliases.txt.
         *
         * <pre>
         * Sample usage:
         * int ea = UCharacter.getIntPropertyValue(c, UProperty.EAST_ASIAN_WIDTH);
         * int ideo = UCharacter.getIntPropertyValue(c, UProperty.IDEOGRAPHIC);
         * boolean b = (ideo == 1) ? true : false;
         * </pre>
         * @param ch code point to test.
         * @param type UProperty selector constant, identifies which binary
         *        property to check. Must be
         *        UProperty.BINARY_START &lt;= type &lt; UProperty.BINARY_LIMIT or
         *        UProperty.INT_START &lt;= type &lt; UProperty.INT_LIMIT or
         *        UProperty.MASK_START &lt;= type &lt; UProperty.MASK_LIMIT.
         * @return numeric value that is directly the property value or,
         *         for enumerated properties, corresponds to the numeric value of
         *         the enumerated constant of the respective property value
         *         enumeration type (cast to enum type if necessary).
         *         Returns 0 or 1 (for false / true) for binary Unicode properties.
         *         Returns a bit-mask for mask properties.
         *         Returns 0 if 'type' is out of bounds or if the Unicode version
         *         does not have data for the property at all, or not for this code
         *         point.
         * @see UProperty
         * @see #hasBinaryProperty
         * @see #getIntPropertyMinValue
         * @see #getIntPropertyMaxValue
         * @see #getUnicodeVersion
         * @stable ICU 2.4
         */
        public static int GetInt32PropertyValue(int ch, UProperty type)
        {
            return UCharacterProperty.INSTANCE.GetInt32PropertyValue(ch, (int)type);
        }
        /**
         * {@icu} Returns a string version of the property value.
         * @param propertyEnum The property enum value.
         * @param codepoint The codepoint value.
         * @param nameChoice The choice of the name.
         * @return value as string
         * @internal
         * @deprecated 
         */
        [Obsolete("This API is ICU internal only.")]
        ///CLOVER:OFF
        public static string GetStringPropertyValue(UProperty propertyEnum, int codepoint, NameChoice nameChoice)
        {
            if ((propertyEnum >= UProperty.BINARY_START && propertyEnum < UProperty.BINARY_LIMIT) ||
                    (propertyEnum >= UProperty.INT_START && propertyEnum < UProperty.INT_LIMIT))
            {
                return GetPropertyValueName(propertyEnum, GetInt32PropertyValue(codepoint, propertyEnum),
                        nameChoice);
            }
            if (propertyEnum == UProperty.NUMERIC_VALUE)
            {
                return GetUnicodeNumericValue(codepoint).ToString(CultureInfo.InvariantCulture);
            }
            // otherwise must be string property
            switch (propertyEnum)
            {
                case UProperty.AGE: return GetAge(codepoint).ToString();
                case UProperty.ISO_COMMENT: return GetISOComment(codepoint);
                case UProperty.BIDI_MIRRORING_GLYPH: return ToString(GetMirror(codepoint));
                case UProperty.CASE_FOLDING: return ToString(FoldCase(codepoint, true));
                case UProperty.LOWERCASE_MAPPING: return ToString(ToLower(codepoint));
                case UProperty.NAME: return GetName(codepoint);
                case UProperty.SIMPLE_CASE_FOLDING: return ToString(FoldCase(codepoint, true));
                case UProperty.SIMPLE_LOWERCASE_MAPPING: return ToString(ToLower(codepoint));
                case UProperty.SIMPLE_TITLECASE_MAPPING: return ToString(ToTitleCase(codepoint));
                case UProperty.SIMPLE_UPPERCASE_MAPPING: return ToString(ToUpper(codepoint));
                case UProperty.TITLECASE_MAPPING: return ToString(ToTitleCase(codepoint));
                case UProperty.UNICODE_1_NAME: return GetName1_0(codepoint);
                case UProperty.UPPERCASE_MAPPING: return ToString(ToUpper(codepoint));
            }
            throw new ArgumentException("Illegal Property Enum");
        }
        ///CLOVER:ON

        /**
         * {@icu} Returns the minimum value for an integer/binary Unicode property type.
         * Can be used together with UCharacter.getIntPropertyMaxValue(int)
         * to allocate arrays of com.ibm.icu.text.UnicodeSet or similar.
         * @param type UProperty selector constant, identifies which binary
         *        property to check. Must be
         *        UProperty.BINARY_START &lt;= type &lt; UProperty.BINARY_LIMIT or
         *        UProperty.INT_START &lt;= type &lt; UProperty.INT_LIMIT.
         * @return Minimum value returned by UCharacter.getIntPropertyValue(int)
         *         for a Unicode property. 0 if the property
         *         selector 'type' is out of range.
         * @see UProperty
         * @see #hasBinaryProperty
         * @see #getUnicodeVersion
         * @see #getIntPropertyMaxValue
         * @see #getIntPropertyValue
         * @stable ICU 2.4
         */
        public static int GetIntPropertyMinValue(UProperty type) // ICU4N TODO: API Rename GetInt32PropertyMinValue
        {

            return 0; // undefined; and: all other properties have a minimum value of 0
        }


        /**
         * {@icu} Returns the maximum value for an integer/binary Unicode property.
         * Can be used together with UCharacter.getIntPropertyMinValue(int)
         * to allocate arrays of com.ibm.icu.text.UnicodeSet or similar.
         * Examples for min/max values (for Unicode 3.2):
         * <ul>
         * <li> UProperty.BIDI_CLASS:    0/18
         * (UCharacterDirection.LEFT_TO_RIGHT/UCharacterDirection.BOUNDARY_NEUTRAL)
         * <li> UProperty.SCRIPT:        0/45 (UScript.COMMON/UScript.TAGBANWA)
         * <li> UProperty.IDEOGRAPHIC:   0/1  (false/true)
         * </ul>
         * For undefined UProperty constant values, min/max values will be 0/-1.
         * @param type UProperty selector constant, identifies which binary
         *        property to check. Must be
         *        UProperty.BINARY_START &lt;= type &lt; UProperty.BINARY_LIMIT or
         *        UProperty.INT_START &lt;= type &lt; UProperty.INT_LIMIT.
         * @return Maximum value returned by u_getIntPropertyValue for a Unicode
         *         property. &lt;= 0 if the property selector 'type' is out of range.
         * @see UProperty
         * @see #hasBinaryProperty
         * @see #getUnicodeVersion
         * @see #getIntPropertyMaxValue
         * @see #getIntPropertyValue
         * @stable ICU 2.4
         */
        public static int GetIntPropertyMaxValue(UProperty type) // ICU4N TODO: API Rename GetInt32PropertyMaxValue
        {
            return UCharacterProperty.INSTANCE.GetIntPropertyMaxValue((int)type);
        }

        /**
         * Provide the java.lang.Character forDigit API, for convenience.
         * @stable ICU 3.0
         */
        public static char ForDigit(int digit, int radix)
        {
            return Character.ForDigit(digit, radix);
        }

        // JDK 1.5 API coverage

        /**
         * Constant U+D800, same as {@link Character#MIN_HIGH_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MIN_HIGH_SURROGATE = Character.MIN_HIGH_SURROGATE;

        /**
         * Constant U+DBFF, same as {@link Character#MAX_HIGH_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MAX_HIGH_SURROGATE = Character.MAX_HIGH_SURROGATE;

        /**
         * Constant U+DC00, same as {@link Character#MIN_LOW_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MIN_LOW_SURROGATE = Character.MIN_LOW_SURROGATE;

        /**
         * Constant U+DFFF, same as {@link Character#MAX_LOW_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MAX_LOW_SURROGATE = Character.MAX_LOW_SURROGATE;

        /**
         * Constant U+D800, same as {@link Character#MIN_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MIN_SURROGATE = Character.MIN_SURROGATE;

        /**
         * Constant U+DFFF, same as {@link Character#MAX_SURROGATE}.
         *
         * @stable ICU 3.0
         */
        public static readonly char MAX_SURROGATE = Character.MAX_SURROGATE;

        /**
         * Constant U+10000, same as {@link Character#MIN_SUPPLEMENTARY_CODE_POINT}.
         *
         * @stable ICU 3.0
         */
        public static readonly int MIN_SUPPLEMENTARY_CODE_POINT = Character.MIN_SUPPLEMENTARY_CODE_POINT;

        /**
         * Constant U+10FFFF, same as {@link Character#MAX_CODE_POINT}.
         *
         * @stable ICU 3.0
         */
        public static readonly int MAX_CODE_POINT = Character.MAX_CODE_POINT;

        /**
         * Constant U+0000, same as {@link Character#MIN_CODE_POINT}.
         *
         * @stable ICU 3.0
         */
        public static readonly int MIN_CODE_POINT = Character.MIN_CODE_POINT;

        /**
         * Equivalent to {@link Character#isValidCodePoint}.
         *
         * @param cp the code point to check
         * @return true if cp is a valid code point
         * @stable ICU 3.0
         */
        public static bool IsValidCodePoint(int cp)
        {
            return cp >= 0 && cp <= MAX_CODE_POINT;
        }

        /**
         * Same as {@link Character#isSupplementaryCodePoint}.
         *
         * @param cp the code point to check
         * @return true if cp is a supplementary code point
         * @stable ICU 3.0
         */
        public static bool IsSupplementaryCodePoint(int cp)
        {
            return Character.IsSupplementaryCodePoint(cp);
        }

        /**
         * Same as {@link Character#isHighSurrogate}.
         *
         * @param ch the char to check
         * @return true if ch is a high (lead) surrogate
         * @stable ICU 3.0
         */
        public static bool IsHighSurrogate(char ch)
        {
            return char.IsHighSurrogate(ch);
        }

        /**
         * Same as {@link Character#isLowSurrogate}.
         *
         * @param ch the char to check
         * @return true if ch is a low (trail) surrogate
         * @stable ICU 3.0
         */
        public static bool IsLowSurrogate(char ch)
        {
            return char.IsLowSurrogate(ch);
        }

        /**
         * Same as {@link Character#isSurrogatePair}.
         *
         * @param high the high (lead) char
         * @param low the low (trail) char
         * @return true if high, low form a surrogate pair
         * @stable ICU 3.0
         */
        public static bool IsSurrogatePair(char high, char low)
        {
            return char.IsSurrogatePair(high, low);
        }

        /**
         * Same as {@link Character#charCount}.
         * Returns the number of chars needed to represent the code point (1 or 2).
         * This does not check the code point for validity.
         *
         * @param cp the code point to check
         * @return the number of chars needed to represent the code point
         * @stable ICU 3.0
         */
        public static int CharCount(int cp)
        {
            return Character.CharCount(cp);
        }

        /**
         * Same as {@link Character#toCodePoint}.
         * Returns the code point represented by the two surrogate code units.
         * This does not check the surrogate pair for validity.
         *
         * @param high the high (lead) surrogate
         * @param low the low (trail) surrogate
         * @return the code point formed by the surrogate pair
         * @stable ICU 3.0
         */
        public static int ToCodePoint(char high, char low)
        {
            return Character.ToCodePoint(high, low);
        }


        // ICU4N specific - CodePointAt(ICharSequence seq, int index) moved to UCharacterExtension.tt

        // ICU4N specific - CodePointAt(char[] seq, int index) moved to UCharacterExtension.tt

        /**
         * Same as {@link Character#codePointAt(char[], int, int)}.
         * Returns the code point at index.
         * This examines only the characters at index and index+1.
         *
         * @param text the characters to check
         * @param index the index of the first or only char forming the code point
         * @param limit the limit of the valid text
         * @return the code point at the index
         * @stable ICU 3.0
         */
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

        /**
         * Same as {@link Character#codePointBefore(char[], int, int)}.
         * Return the code point before index.
         * This examines only the characters at index-1 and index-2.
         *
         * @param text the characters to check
         * @param index the index after the last or only char forming the code point
         * @param limit the start of the valid text
         * @return the code point before the index
         * @stable ICU 3.0
         */
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

        /**
         * Same as {@link Character#toChars(int, char[], int)}.
         * Writes the chars representing the
         * code point into the destination at the given index.
         *
         * @param cp the code point to convert
         * @param dst the destination array into which to put the char(s) representing the code point
         * @param dstIndex the index at which to put the first (or only) char
         * @return the count of the number of chars written (1 or 2)
         * @throws IllegalArgumentException if cp is not a valid code point
         * @stable ICU 3.0
         */
        public static int ToChars(int cp, char[] dst, int dstIndex)
        {
            return Character.ToChars(cp, dst, dstIndex);
        }

        /**
         * Same as {@link Character#toChars(int)}.
         * Returns a char array representing the code point.
         *
         * @param cp the code point to convert
         * @return an array containing the char(s) representing the code point
         * @throws IllegalArgumentException if cp is not a valid code point
         * @stable ICU 3.0
         */
        public static char[] ToChars(int cp)
        {
            return Character.ToChars(cp);
        }

        /**
         * Equivalent to the {@link Character#getDirectionality(char)} method, for
         * convenience. Returns a byte representing the directionality of the
         * character.
         *
         * {@icunote} Unlike {@link Character#getDirectionality(char)}, this returns
         * DIRECTIONALITY_LEFT_TO_RIGHT for undefined or out-of-bounds characters.
         *
         * {@icunote} The return value must be tested using the constants defined in {@link
         * UCharacterDirection} and its interface {@link
         * UCharacterEnums.ECharacterDirection} since the values are different from the ones
         * defined by <code>java.lang.Character</code>.
         * @param cp the code point to check
         * @return the directionality of the code point
         * @see #getDirection
         * @stable ICU 3.0
         */
        public static byte GetDirectionality(int cp)
        {
            return (byte)GetDirection(cp);
        }

        // ICU4N specific - CodePointCount(ICharSequence text, int start, int limit) moved to UCharacterExtension.tt

        // ICU4N specific - CodePointCount(char[] text, int start, int limit) moved to UCharacterExtension.tt

        // ICU4N specific - OffsetByCodePoints(ICharSequence text, int index, int codePointOffset) moved to UCharacterExtension.tt

        /**
         * Equivalent to the
         * {@link Character#offsetByCodePoints(char[], int, int, int, int)}
         * method, for convenience.  Adjusts the char index by a code point offset.
         * @param text the characters to check
         * @param start the start of the range to check
         * @param count the length of the range to check
         * @param index the index to adjust
         * @param codePointOffset the number of code points by which to offset the index
         * @return the adjusted index
         * @stable ICU 3.0
         */
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
                    while (ch >= MIN_LOW_SURROGATE && ch <= MAX_LOW_SURROGATE && index > start)
                    {
                        ch = text[--index];
                        if (ch < MIN_HIGH_SURROGATE || ch > MAX_HIGH_SURROGATE)
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
                    while (ch >= MIN_HIGH_SURROGATE && ch <= MAX_HIGH_SURROGATE && index < limit)
                    {
                        ch = text[index++];
                        if (ch < MIN_LOW_SURROGATE || ch > MAX_LOW_SURROGATE)
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

        /**
         * To get the last character out from a data type
         */
        private const int LAST_CHAR_MASK_ = 0xFFFF;

        //    /**
        //     * To get the last byte out from a data type
        //     */
        //    private const int LAST_BYTE_MASK_ = 0xFF;
        //
        //    /**
        //     * Shift 16 bits
        //     */
        //    private const int SHIFT_16_ = 16;
        //
        //    /**
        //     * Shift 24 bits
        //     */
        //    private const int SHIFT_24_ = 24;
        //
        //    /**
        //     * Decimal radix
        //     */
        //    private const int DECIMAL_RADIX_ = 10;

        /**
         * No break space code point
         */
        private const int NO_BREAK_SPACE_ = 0xA0;

        /**
         * Figure space code point
         */
        private const int FIGURE_SPACE_ = 0x2007;

        /**
         * Narrow no break space code point
         */
        private const int NARROW_NO_BREAK_SPACE_ = 0x202F;

        /**
         * Ideographic number zero code point
         */
        private const int IDEOGRAPHIC_NUMBER_ZERO_ = 0x3007;

        /**
         * CJK Ideograph, First code point
         */
        private const int CJK_IDEOGRAPH_FIRST_ = 0x4e00;

        /**
         * CJK Ideograph, Second code point
         */
        private const int CJK_IDEOGRAPH_SECOND_ = 0x4e8c;

        /**
         * CJK Ideograph, Third code point
         */
        private const int CJK_IDEOGRAPH_THIRD_ = 0x4e09;

        /**
         * CJK Ideograph, Fourth code point
         */
        private const int CJK_IDEOGRAPH_FOURTH_ = 0x56db;

        /**
         * CJK Ideograph, FIFTH code point
         */
        private const int CJK_IDEOGRAPH_FIFTH_ = 0x4e94;

        /**
         * CJK Ideograph, Sixth code point
         */
        private const int CJK_IDEOGRAPH_SIXTH_ = 0x516d;

        /**
         * CJK Ideograph, Seventh code point
         */
        private const int CJK_IDEOGRAPH_SEVENTH_ = 0x4e03;

        /**
         * CJK Ideograph, Eighth code point
         */
        private const int CJK_IDEOGRAPH_EIGHTH_ = 0x516b;

        /**
         * CJK Ideograph, Nineth code point
         */
        private const int CJK_IDEOGRAPH_NINETH_ = 0x4e5d;

        /**
         * Application Program command code point
         */
        private const int APPLICATION_PROGRAM_COMMAND_ = 0x009F;

        /**
         * Unit separator code point
         */
        private const int UNIT_SEPARATOR_ = 0x001F;

        /**
         * Delete code point
         */
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
        ///CLOVER:OFF
        /**
         * Private constructor to prevent instantiation
         */
        private UCharacter()
        {
        }
        ///CLOVER:ON
    }
}