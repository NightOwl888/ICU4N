using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Lang
{
    public enum UProperty
    {
        /**
         * Special value indicating undefined property.
         * @internal
         * @deprecated 
         */
        [Obsolete("This API is ICU internal only.")]
        UNDEFINED = -1,

        /**
         * <p>Binary property Alphabetic.
         * <p>Property for UCharacter.isUAlphabetic(), different from the property
         * in UCharacter.isalpha().
         * <p>Lu + Ll + Lt + Lm + Lo + Nl + Other_Alphabetic.
         * @stable ICU 2.6
         */
        ALPHABETIC = 0,

        /**
         * First constant for binary Unicode properties.
         * @stable ICU 2.6
         */
        BINARY_START = ALPHABETIC,

        /**
         * Binary property ASCII_Hex_Digit (0-9 A-F a-f).
         * @stable ICU 2.6
         */
        ASCII_HEX_DIGIT = 1,

        /**
         * <p>Binary property Bidi_Control.
         * <p>Format controls which have specific functions in the Bidi Algorithm.
         *
         * @stable ICU 2.6
         */
        BIDI_CONTROL = 2,

        /**
         * <p>Binary property Bidi_Mirrored.
         * <p>Characters that may change display in RTL text.
         * <p>Property for UCharacter.isMirrored().
         * <p>See Bidi Algorithm, UTR 9.
         * @stable ICU 2.6
         */
        BIDI_MIRRORED = 3,

        /**
         * <p>Binary property Dash.
         * <p>Variations of dashes.
         * @stable ICU 2.6
         */
        DASH = 4,

        /**
         * <p>Binary property Default_Ignorable_Code_Point (new).
         *
         * <p>Property that indicates codepoint is ignorable in most processing.
         *
         * <p>Codepoints (2060..206F, FFF0..FFFB, E0000..E0FFF) +
         * Other_Default_Ignorable_Code_Point + (Cf + Cc + Cs - White_Space)
         * @stable ICU 2.6
         */
        DEFAULT_IGNORABLE_CODE_POINT = 5,

        /**
         * <p>Binary property Deprecated (new).
         * <p>The usage of deprecated characters is strongly discouraged.
         * @stable ICU 2.6
         */
        DEPRECATED = 6,

        /**
         * <p>Binary property Diacritic.
         * <p>Characters that linguistically modify the meaning of another
         * character to which they apply.
         * @stable ICU 2.6
         */
        DIACRITIC = 7,

        /**
         * <p>Binary property Extender.
         * <p>Extend the value or shape of a preceding alphabetic character, e.g.
         * length and iteration marks.
         * @stable ICU 2.6
         */
        EXTENDER = 8,

        /**
         * <p>Binary property Full_Composition_Exclusion.
         * <p>CompositionExclusions.txt + Singleton Decompositions +
         * Non-Starter Decompositions.
         * @stable ICU 2.6
         */
        FULL_COMPOSITION_EXCLUSION = 9,

        /**
         * <p>Binary property Grapheme_Base (new).
         * <p>For programmatic determination of grapheme cluster boundaries.
         * [0..10FFFF]-Cc-Cf-Cs-Co-Cn-Zl-Zp-Grapheme_Link-Grapheme_Extend-CGJ
         * @stable ICU 2.6
         */
        GRAPHEME_BASE = 10,

        /**
         * <p>Binary property Grapheme_Extend (new).
         * <p>For programmatic determination of grapheme cluster boundaries.
         * <p>Me+Mn+Mc+Other_Grapheme_Extend-Grapheme_Link-CGJ
         * @stable ICU 2.6
         */
        GRAPHEME_EXTEND = 11,

        /**
         * <p>Binary property Grapheme_Link (new).
         * <p>For programmatic determination of grapheme cluster boundaries.
         * @stable ICU 2.6
         */
        GRAPHEME_LINK = 12,

        /**
         * <p>Binary property Hex_Digit.
         * <p>Characters commonly used for hexadecimal numbers.
         * @stable ICU 2.6
         */
        HEX_DIGIT = 13,

        /**
         * <p>Binary property Hyphen.
         * <p>Dashes used to mark connections between pieces of words, plus the
         * Katakana middle dot.
         * @stable ICU 2.6
         */
        HYPHEN = 14,

        /**
         * <p>Binary property ID_Continue.
         * <p>Characters that can continue an identifier.
         * <p>ID_Start+Mn+Mc+Nd+Pc
         * @stable ICU 2.6
         */
        ID_CONTINUE = 15,

        /**
         * <p>Binary property ID_Start.
         * <p>Characters that can start an identifier.
         * <p>Lu+Ll+Lt+Lm+Lo+Nl
         * @stable ICU 2.6
         */
        ID_START = 16,

        /**
         * <p>Binary property Ideographic.
         * <p>CJKV ideographs.
         * @stable ICU 2.6
         */
        IDEOGRAPHIC = 17,

        /**
         * <p>Binary property IDS_Binary_Operator (new).
         * <p>For programmatic determination of Ideographic Description Sequences.
         *
         * @stable ICU 2.6
         */
        IDS_BINARY_OPERATOR = 18,

        /**
         * <p>Binary property IDS_Trinary_Operator (new).
         * <p>For programmatic determination of Ideographic Description
         * Sequences.
         * @stable ICU 2.6
         */
        IDS_TRINARY_OPERATOR = 19,

        /**
         * <p>Binary property Join_Control.
         * <p>Format controls for cursive joining and ligation.
         * @stable ICU 2.6
         */
        JOIN_CONTROL = 20,

        /**
         * <p>Binary property Logical_Order_Exception (new).
         * <p>Characters that do not use logical order and require special
         * handling in most processing.
         * @stable ICU 2.6
         */
        LOGICAL_ORDER_EXCEPTION = 21,

        /**
         * <p>Binary property Lowercase.
         * <p>Same as UCharacter.isULowercase(), different from
         * UCharacter.islower().
         * <p>Ll+Other_Lowercase
         * @stable ICU 2.6
         */
        LOWERCASE = 22,

        /** <p>Binary property Math.
         * <p>Sm+Other_Math
         * @stable ICU 2.6
         */
        MATH = 23,

        /**
         * <p>Binary property Noncharacter_Code_Point.
         * <p>Code points that are explicitly defined as illegal for the encoding
         * of characters.
         * @stable ICU 2.6
         */
        NONCHARACTER_CODE_POINT = 24,

        /**
         * <p>Binary property Quotation_Mark.
         * @stable ICU 2.6
         */
        QUOTATION_MARK = 25,

        /**
         * <p>Binary property Radical (new).
         * <p>For programmatic determination of Ideographic Description
         * Sequences.
         * @stable ICU 2.6
         */
        RADICAL = 26,

        /**
         * <p>Binary property Soft_Dotted (new).
         * <p>Characters with a "soft dot", like i or j.
         * <p>An accent placed on these characters causes the dot to disappear.
         * @stable ICU 2.6
         */
        SOFT_DOTTED = 27,

        /**
         * <p>Binary property Terminal_Punctuation.
         * <p>Punctuation characters that generally mark the end of textual
         * units.
         * @stable ICU 2.6
         */
        TERMINAL_PUNCTUATION = 28,

        /**
         * <p>Binary property Unified_Ideograph (new).
         * <p>For programmatic determination of Ideographic Description
         * Sequences.
         * @stable ICU 2.6
         */
        UNIFIED_IDEOGRAPH = 29,

        /**
         * <p>Binary property Uppercase.
         * <p>Same as UCharacter.isUUppercase(), different from
         * UCharacter.isUpperCase().
         * <p>Lu+Other_Uppercase
         * @stable ICU 2.6
         */
        UPPERCASE = 30,

        /**
         * <p>Binary property White_Space.
         * <p>Same as UCharacter.isUWhiteSpace(), different from
         * UCharacter.isSpace() and UCharacter.isWhitespace().
         * Space characters+TAB+CR+LF-ZWSP-ZWNBSP
         * @stable ICU 2.6
         */
        WHITE_SPACE = 31,

        /**
         * <p>Binary property XID_Continue.
         * <p>ID_Continue modified to allow closure under normalization forms
         * NFKC and NFKD.
         * @stable ICU 2.6
         */
        XID_CONTINUE = 32,

        /**
         * <p>Binary property XID_Start.
         * <p>ID_Start modified to allow closure under normalization forms NFKC
         * and NFKD.
         * @stable ICU 2.6
         */
        XID_START = 33,

        /**
         * <p>Binary property Case_Sensitive.
         * <p>Either the source of a case
         * mapping or _in_ the target of a case mapping. Not the same as
         * the general category Cased_Letter.
         * @stable ICU 2.6
         */
        CASE_SENSITIVE = 34,

        /**
         * Binary property STerm (new in Unicode 4.0.1).
         * Sentence Terminal. Used in UAX #29: Text Boundaries
         * (http://www.unicode.org/reports/tr29/)
         * @stable ICU 3.0
         */
        S_TERM = 35,

        /**
         * Binary property Variation_Selector (new in Unicode 4.0.1).
         * Indicates all those characters that qualify as Variation Selectors.
         * For details on the behavior of these characters,
         * see StandardizedVariants.html and 15.6 Variation Selectors.
         * @stable ICU 3.0
         */
        VARIATION_SELECTOR = 36,

        /**
         * Binary property NFD_Inert.
         * ICU-specific property for characters that are inert under NFD,
         * i.e., they do not interact with adjacent characters.
         * Used for example in normalizing transforms in incremental mode
         * to find the boundary of safely normalizable text despite possible
         * text additions.
         *
         * There is one such property per normalization form.
         * These properties are computed as follows - an inert character is:
         * a) unassigned, or ALL of the following:
         * b) of combining class 0.
         * c) not decomposed by this normalization form.
         * AND if NFC or NFKC,
         * d) can never compose with a previous character.
         * e) can never compose with a following character.
         * f) can never change if another character is added.
         * Example: a-breve might satisfy all but f, but if you
         * add an ogonek it changes to a-ogonek + breve
         *
         * See also com.ibm.text.UCD.NFSkippable in the ICU4J repository,
         * and icu/source/common/unormimp.h .
         * @stable ICU 3.0
         */
        NFD_INERT = 37,

        /**
         * Binary property NFKD_Inert.
         * ICU-specific property for characters that are inert under NFKD,
         * i.e., they do not interact with adjacent characters.
         * Used for example in normalizing transforms in incremental mode
         * to find the boundary of safely normalizable text despite possible
         * text additions.
         * @see #NFD_INERT
         * @stable ICU 3.0
         */
        NFKD_INERT = 38,

        /**
         * Binary property NFC_Inert.
         * ICU-specific property for characters that are inert under NFC,
         * i.e., they do not interact with adjacent characters.
         * Used for example in normalizing transforms in incremental mode
         * to find the boundary of safely normalizable text despite possible
         * text additions.
         * @see #NFD_INERT
         * @stable ICU 3.0
         */
        NFC_INERT = 39,

        /**
         * Binary property NFKC_Inert.
         * ICU-specific property for characters that are inert under NFKC,
         * i.e., they do not interact with adjacent characters.
         * Used for example in normalizing transforms in incremental mode
         * to find the boundary of safely normalizable text despite possible
         * text additions.
         * @see #NFD_INERT
         * @stable ICU 3.0
         */
        NFKC_INERT = 40,

        /**
         * Binary Property Segment_Starter.
         * ICU-specific property for characters that are starters in terms of
         * Unicode normalization and combining character sequences.
         * They have ccc=0 and do not occur in non-initial position of the
         * canonical decomposition of any character
         * (like " in NFD(a-umlaut) and a Jamo T in an NFD(Hangul LVT)).
         * ICU uses this property for segmenting a string for generating a set of
         * canonically equivalent strings, e.g. for canonical closure while
         * processing collation tailoring rules.
         * @stable ICU 3.0
         */
        SEGMENT_STARTER = 41,

        /**
         * Binary property Pattern_Syntax (new in Unicode 4.1).
         * See UAX #31 Identifier and Pattern Syntax
         * (http://www.unicode.org/reports/tr31/)
         * @stable ICU 3.4
         */
        PATTERN_SYNTAX = 42,

        /**
         * Binary property Pattern_White_Space (new in Unicode 4.1).
         * See UAX #31 Identifier and Pattern Syntax
         * (http://www.unicode.org/reports/tr31/)
         * @stable ICU 3.4
         */
        PATTERN_WHITE_SPACE = 43,

        /**
         * Binary property alnum (a C/POSIX character class).
         * Implemented according to the UTS #18 Annex C Standard Recommendation.
         * See the UCharacter class documentation.
         * @stable ICU 3.4
         */
        POSIX_ALNUM = 44,

        /**
         * Binary property blank (a C/POSIX character class).
         * Implemented according to the UTS #18 Annex C Standard Recommendation.
         * See the UCharacter class documentation.
         * @stable ICU 3.4
         */
        POSIX_BLANK = 45,

        /**
         * Binary property graph (a C/POSIX character class).
         * Implemented according to the UTS #18 Annex C Standard Recommendation.
         * See the UCharacter class documentation.
         * @stable ICU 3.4
         */
        POSIX_GRAPH = 46,

        /**
         * Binary property print (a C/POSIX character class).
         * Implemented according to the UTS #18 Annex C Standard Recommendation.
         * See the UCharacter class documentation.
         * @stable ICU 3.4
         */
        POSIX_PRINT = 47,

        /**
         * Binary property xdigit (a C/POSIX character class).
         * Implemented according to the UTS #18 Annex C Standard Recommendation.
         * See the UCharacter class documentation.
         * @stable ICU 3.4
         */
        POSIX_XDIGIT = 48,

        /**
         * Binary property Cased.
         * For Lowercase, Uppercase and Titlecase characters.
         * @stable ICU 4.4
         */
        CASED = 49,
        /**
         * Binary property Case_Ignorable.
         * Used in context-sensitive case mappings.
         * @stable ICU 4.4
         */
        CASE_IGNORABLE = 50,
        /**
         * Binary property Changes_When_Lowercased.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_LOWERCASED = 51,
        /**
         * Binary property Changes_When_Uppercased.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_UPPERCASED = 52,
        /**
         * Binary property Changes_When_Titlecased.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_TITLECASED = 53,
        /**
         * Binary property Changes_When_Casefolded.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_CASEFOLDED = 54,
        /**
         * Binary property Changes_When_Casemapped.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_CASEMAPPED = 55,
        /**
         * Binary property Changes_When_NFKC_Casefolded.
         * @stable ICU 4.4
         */
        CHANGES_WHEN_NFKC_CASEFOLDED = 56,
        /**
         * Binary property Emoji.
         * See http://www.unicode.org/reports/tr51/#Emoji_Properties
         *
         * @stable ICU 57
         */
        EMOJI = 57,
        /**
         * Binary property Emoji_Presentation.
         * See http://www.unicode.org/reports/tr51/#Emoji_Properties
         *
         * @stable ICU 57
         */
        EMOJI_PRESENTATION = 58,
        /**
         * Binary property Emoji_Modifier.
         * See http://www.unicode.org/reports/tr51/#Emoji_Properties
         *
         * @stable ICU 57
         */
        EMOJI_MODIFIER = 59,
        /**
         * Binary property Emoji_Modifier_Base.
         * See http://www.unicode.org/reports/tr51/#Emoji_Properties
         *
         * @stable ICU 57
         */
        EMOJI_MODIFIER_BASE = 60,
        /**
         * Binary property Emoji_Component.
         * See http://www.unicode.org/reports/tr51/#Emoji_Properties
         *
         * @stable ICU 60
         */
        EMOJI_COMPONENT = 61,
        /**
         * Binary property Regional_Indicator.
         *
         * @stable ICU 60
         */
        REGIONAL_INDICATOR = 62,
        /**
         * Binary property Prepended_Concatenation_Mark.
         *
         * @stable ICU 60
         */
        PREPENDED_CONCATENATION_MARK = 63,

        /**
         * One more than the last constant for binary Unicode properties.
         * @deprecated 
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        BINARY_LIMIT = 64,

        /**
         * Enumerated property Bidi_Class.
         * Same as UCharacter.getDirection(int), returns UCharacterDirection values.
         * @stable ICU 2.4
         */
        BIDI_CLASS = 0x1000,

        /**
         * First constant for enumerated/integer Unicode properties.
         * @stable ICU 2.4
         */
        INT_START = BIDI_CLASS,

        /**
         * Enumerated property Block.
         * Same as UCharacter.UnicodeBlock.of(int), returns UCharacter.UnicodeBlock
         * values.
         * @stable ICU 2.4
         */
        BLOCK = 0x1001,

        /**
         * Enumerated property Canonical_Combining_Class.
         * Same as UCharacter.getCombiningClass(int), returns 8-bit numeric values.
         * @stable ICU 2.4
         */
        CANONICAL_COMBINING_CLASS = 0x1002,

        /**
         * Enumerated property Decomposition_Type.
         * Returns UCharacter.DecompositionType values.
         * @stable ICU 2.4
         */
        DECOMPOSITION_TYPE = 0x1003,

        /**
         * Enumerated property East_Asian_Width.
         * See http://www.unicode.org/reports/tr11/
         * Returns UCharacter.EastAsianWidth values.
         * @stable ICU 2.4
         */
        EAST_ASIAN_WIDTH = 0x1004,

        /**
         * Enumerated property General_Category.
         * Same as UCharacter.getType(int), returns UCharacterCategory values.
         * @stable ICU 2.4
         */
        GENERAL_CATEGORY = 0x1005,

        /**
         * Enumerated property Joining_Group.
         * Returns UCharacter.JoiningGroup values.
         * @stable ICU 2.4
         */
        JOINING_GROUP = 0x1006,

        /**
         * Enumerated property Joining_Type.
         * Returns UCharacter.JoiningType values.
         * @stable ICU 2.4
         */
        JOINING_TYPE = 0x1007,

        /**
         * Enumerated property Line_Break.
         * Returns UCharacter.LineBreak values.
         * @stable ICU 2.4
         */
        LINE_BREAK = 0x1008,

        /**
         * Enumerated property Numeric_Type.
         * Returns UCharacter.NumericType values.
         * @stable ICU 2.4
         */
        NUMERIC_TYPE = 0x1009,

        /**
         * Enumerated property Script.
         * Same as UScript.getScript(int), returns UScript values.
         * @stable ICU 2.4
         */
        SCRIPT = 0x100A,

        /**
         * Enumerated property Hangul_Syllable_Type, new in Unicode 4.
         * Returns UCharacter.HangulSyllableType values.
         * @stable ICU 2.6
         */
        HANGUL_SYLLABLE_TYPE = 0x100B,

        /**
         * Enumerated property NFD_Quick_Check.
         * Returns numeric values compatible with Normalizer.QuickCheckResult.
         * @stable ICU 3.0
         */
        NFD_QUICK_CHECK = 0x100C,

        /**
         * Enumerated property NFKD_Quick_Check.
         * Returns numeric values compatible with Normalizer.QuickCheckResult.
         * @stable ICU 3.0
         */
        NFKD_QUICK_CHECK = 0x100D,

        /**
         * Enumerated property NFC_Quick_Check.
         * Returns numeric values compatible with Normalizer.QuickCheckResult.
         * @stable ICU 3.0
         */
        NFC_QUICK_CHECK = 0x100E,

        /**
         * Enumerated property NFKC_Quick_Check.
         * Returns numeric values compatible with Normalizer.QuickCheckResult.
         * @stable ICU 3.0
         */
        NFKC_QUICK_CHECK = 0x100F,

        /**
         * Enumerated property Lead_Canonical_Combining_Class.
         * ICU-specific property for the ccc of the first code point
         * of the decomposition, or lccc(c)=ccc(NFD(c)[0]).
         * Useful for checking for canonically ordered text,
         * see Normalizer.FCD and http://www.unicode.org/notes/tn5/#FCD .
         * Returns 8-bit numeric values like CANONICAL_COMBINING_CLASS.
         * @stable ICU 3.0
         */
        LEAD_CANONICAL_COMBINING_CLASS = 0x1010,

        /**
         * Enumerated property Trail_Canonical_Combining_Class.
         * ICU-specific property for the ccc of the last code point
         * of the decomposition, or lccc(c)=ccc(NFD(c)[last]).
         * Useful for checking for canonically ordered text,
         * see Normalizer.FCD and http://www.unicode.org/notes/tn5/#FCD .
         * Returns 8-bit numeric values like CANONICAL_COMBINING_CLASS.
         * @stable ICU 3.0
         */
        TRAIL_CANONICAL_COMBINING_CLASS = 0x1011,

        /**
         * Enumerated property Grapheme_Cluster_Break (new in Unicode 4.1).
         * Used in UAX #29: Text Boundaries
         * (http://www.unicode.org/reports/tr29/)
         * Returns UCharacter.GraphemeClusterBreak values.
         * @stable ICU 3.4
         */
        GRAPHEME_CLUSTER_BREAK = 0x1012,

        /**
         * Enumerated property Sentence_Break (new in Unicode 4.1).
         * Used in UAX #29: Text Boundaries
         * (http://www.unicode.org/reports/tr29/)
         * Returns UCharacter.SentenceBreak values.
         * @stable ICU 3.4
         */
        SENTENCE_BREAK = 0x1013,

        /**
         * Enumerated property Word_Break (new in Unicode 4.1).
         * Used in UAX #29: Text Boundaries
         * (http://www.unicode.org/reports/tr29/)
         * Returns UCharacter.WordBreak values.
         * @stable ICU 3.4
         */
        WORD_BREAK = 0x1014,

        /**
         * Enumerated property Bidi_Paired_Bracket_Type (new in Unicode 6.3).
         * Used in UAX #9: Unicode Bidirectional Algorithm
         * (http://www.unicode.org/reports/tr9/)
         * Returns UCharacter.BidiPairedBracketType values.
         * @stable ICU 52
         */
        BIDI_PAIRED_BRACKET_TYPE = 0x1015,

        /**
         * One more than the last constant for enumerated/integer Unicode properties.
         * @deprecated .
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        INT_LIMIT = 0x1016,

        /**
         * Bitmask property General_Category_Mask.
         * This is the General_Category property returned as a bit mask.
         * When used in UCharacter.getIntPropertyValue(c),
         * returns bit masks for UCharacterCategory values where exactly one bit is set.
         * When used with UCharacter.getPropertyValueName() and UCharacter.getPropertyValueEnum(),
         * a multi-bit mask is used for sets of categories like "Letters".
         * @stable ICU 2.4
         */
        GENERAL_CATEGORY_MASK = 0x2000,

        /**
         * First constant for bit-mask Unicode properties.
         * @stable ICU 2.4
         */
        MASK_START = GENERAL_CATEGORY_MASK,

        /**
         * One more than the last constant for bit-mask Unicode properties.
         * @deprecated 
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        MASK_LIMIT = 0x2001,

        /**
         * Double property Numeric_Value.
         * Corresponds to UCharacter.getUnicodeNumericValue(int).
         * @stable ICU 2.4
         */
        NUMERIC_VALUE = 0x3000,

        /**
         * First constant for double Unicode properties.
         * @stable ICU 2.4
         */
        DOUBLE_START = NUMERIC_VALUE,

        /**
         * One more than the last constant for double Unicode properties.
         * @deprecated 
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        DOUBLE_LIMIT = 0x3001,

        /**
         * String property Age.
         * Corresponds to UCharacter.getAge(int).
         * @stable ICU 2.4
         */
        AGE = 0x4000,

        /**
         * First constant for string Unicode properties.
         * @stable ICU 2.4
         */
        STRING_START = AGE,

        /**
         * String property Bidi_Mirroring_Glyph.
         * Corresponds to UCharacter.getMirror(int).
         * @stable ICU 2.4
         */
        BIDI_MIRRORING_GLYPH = 0x4001,

        /**
         * String property Case_Folding.
         * Corresponds to UCharacter.foldCase(String, boolean).
         * @stable ICU 2.4
         */
        CASE_FOLDING = 0x4002,

        /**
         * Deprecated string property ISO_Comment.
         * Corresponds to UCharacter.getISOComment(int).
         * @deprecated 
         */
        [Obsolete("ICU 49")]
        ISO_COMMENT = 0x4003,

        /**
         * String property Lowercase_Mapping.
         * Corresponds to UCharacter.toLowerCase(String).
         * @stable ICU 2.4
         */
        LOWERCASE_MAPPING = 0x4004,

        /**
         * String property Name.
         * Corresponds to UCharacter.getName(int).
         * @stable ICU 2.4
         */
        NAME = 0x4005,

        /**
         * String property Simple_Case_Folding.
         * Corresponds to UCharacter.foldCase(int, boolean).
         * @stable ICU 2.4
         */
        SIMPLE_CASE_FOLDING = 0x4006,

        /**
         * String property Simple_Lowercase_Mapping.
         * Corresponds to UCharacter.toLowerCase(int).
         * @stable ICU 2.4
         */
        SIMPLE_LOWERCASE_MAPPING = 0x4007,

        /**
         * String property Simple_Titlecase_Mapping.
         * Corresponds to UCharacter.toTitleCase(int).
         * @stable ICU 2.4
         */
        SIMPLE_TITLECASE_MAPPING = 0x4008,

        /**
         * String property Simple_Uppercase_Mapping.
         * Corresponds to UCharacter.toUpperCase(int).
         * @stable ICU 2.4
         */
        SIMPLE_UPPERCASE_MAPPING = 0x4009,

        /**
         * String property Titlecase_Mapping.
         * Corresponds to UCharacter.toTitleCase(String).
         * @stable ICU 2.4
         */
        TITLECASE_MAPPING = 0x400A,

        /**
         * String property Unicode_1_Name.
         * This property is of little practical value.
         * Beginning with ICU 49, ICU APIs return null or an empty string for this property.
         * Corresponds to UCharacter.getName1_0(int).
         * @deprecated 
         */
        [Obsolete("ICU 49")]
        UNICODE_1_NAME = 0x400B,

        /**
         * String property Uppercase_Mapping.
         * Corresponds to UCharacter.toUpperCase(String).
         * @stable ICU 2.4
         */
        UPPERCASE_MAPPING = 0x400C,

        /**
         * String property Bidi_Paired_Bracket (new in Unicode 6.3).
         * Corresponds to UCharacter.getBidiPairedBracket.
         * @stable ICU 52
         */
        BIDI_PAIRED_BRACKET = 0x400D,

        /**
         * One more than the last constant for string Unicode properties.
         * @deprecated 
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        STRING_LIMIT = 0x400E,

        /**
         * Miscellaneous property Script_Extensions (new in Unicode 6.0).
         * Some characters are commonly used in multiple scripts.
         * For more information, see UAX #24: http://www.unicode.org/reports/tr24/.
         * Corresponds to UScript.hasScript and UScript.getScriptExtensions.
         * @stable ICU 4.6
         */
        SCRIPT_EXTENSIONS = 0x7000,
        /**
         * First constant for Unicode properties with unusual value types.
         * @stable ICU 4.6
         */
        OTHER_PROPERTY_START = SCRIPT_EXTENSIONS,
        /**
         * One more than the last constant for Unicode properties with unusual value types.
         * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        OTHER_PROPERTY_LIMIT = 0x7001,
    }

    public enum NameChoice // ICU4N TODO: API Re-nest so this is the same as the Java documentation?
    {
        /**
         * Selector for the abbreviated name of a property or value.
         * Most properties and values have a short name, those that do
         * not return null.
         * @stable ICU 2.4
         */
        Short = 0,

        /**
         * Selector for the long name of a property or value.  All
         * properties and values have a long name.
         * @stable ICU 2.4
         */
        Long = 1,

        /**
         * The number of predefined property name choices.  Individual
         * properties or values may have more than COUNT aliases.
         * @deprecated 
         */
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Count = 2,
    }
}
