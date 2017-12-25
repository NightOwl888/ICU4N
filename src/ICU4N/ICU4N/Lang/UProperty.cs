using System;

namespace ICU4N.Lang
{
    /// <summary>
    /// Selection values for Unicode properties.
    /// <para/>
    /// These values are used in functions like
    /// <see cref="UCharacter.HasBinaryProperty(int, UProperty)"/> to select one of the Unicode properties.
    /// </summary>
    /// <remarks>
    /// The properties APIs are intended to reflect Unicode properties as
    /// defined in the Unicode Character Database (UCD) and Unicode Technical
    /// Reports (UTR).
    /// <para/>
    /// For details about the properties see
    /// <a href="http://www.unicode.org/reports/tr44/">UAX #44: Unicode Character Database</a>.
    /// <para/>
    /// Important: If ICU is built with UCD files from Unicode versions below
    /// 3.2, then properties marked with "new" are not or not fully
    /// available. Check <see cref="UCharacter.UnicodeVersion"/> to be sure.
    /// </remarks>
    /// <seealso cref="UCharacter"/>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.6</stable>
    public enum UProperty
    {
        /// <summary>
        /// Special value indicating undefined property.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        UNDEFINED = -1,

        /// <summary>
        /// Binary property Alphabetic.
        /// <para/>
        /// Property for <see cref="UCharacter.IsUAlphabetic(int)"/>, different from the property
        /// in <see cref="UCharacter.IsUAlphabetic(int)"/>.
        /// <para/>
        /// Lu + Ll + Lt + Lm + Lo + Nl + Other_Alphabetic.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ALPHABETIC = 0,

        /// <summary>
        /// First constant for binary Unicode properties.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        BINARY_START = ALPHABETIC,

        /// <summary>
        /// Binary property ASCII_Hex_Digit (0-9 A-F a-f).
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ASCII_HEX_DIGIT = 1,

        /// <summary>
        /// Binary property Bidi_Control.
        /// <para/>
        /// Format controls which have specific functions in the Bidi Algorithm.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        BIDI_CONTROL = 2,

        /// <summary>
        /// Binary property Bidi_Mirrored.
        /// <para/>
        /// Characters that may change display in RTL text.
        /// <para/>
        /// Property for <see cref="UCharacter.IsMirrored(int)"/>.
        /// <para/>
        /// See Bidi Algorithm, UTR 9.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        BIDI_MIRRORED = 3,

        /// <summary>
        /// Binary property Dash.
        /// <para/>
        /// Variations of dashes.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        DASH = 4,

        /// <summary>
        /// Binary property Default_Ignorable_Code_Point (new).
        /// <para/>
        /// Property that indicates codepoint is ignorable in most processing.
        /// <para/>
        /// Codepoints (2060..206F, FFF0..FFFB, E0000..E0FFF) +
        /// Other_Default_Ignorable_Code_Point + (Cf + Cc + Cs - White_Space)
        /// </summary>
        /// <stable>ICU 2.6</stable>
        DEFAULT_IGNORABLE_CODE_POINT = 5,

        /// <summary>
        /// Binary property Deprecated (new).
        /// <para/>
        /// The usage of deprecated characters is strongly discouraged.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        DEPRECATED = 6,

        /// <summary>
        /// Binary property Diacritic.
        /// <para/>
        /// Characters that linguistically modify the meaning of another
        /// character to which they apply.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        DIACRITIC = 7,

        /// <summary>
        /// Binary property Extender.
        /// <para/>
        /// Extend the value or shape of a preceding alphabetic character, e.g.
        /// length and iteration marks.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        EXTENDER = 8,

        /// <summary>
        /// Binary property Full_Composition_Exclusion.
        /// <para/>
        /// CompositionExclusions.txt + Singleton Decompositions +
        /// Non-Starter Decompositions.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        FULL_COMPOSITION_EXCLUSION = 9,

        /// <summary>
        /// Binary property Grapheme_Base (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// [0..10FFFF]-Cc-Cf-Cs-Co-Cn-Zl-Zp-Grapheme_Link-Grapheme_Extend-CGJ
        /// </summary>
        /// <stable>ICU 2.6</stable>
        GRAPHEME_BASE = 10,

        /// <summary>
        /// Binary property Grapheme_Extend (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// <para/>
        /// Me+Mn+Mc+Other_Grapheme_Extend-Grapheme_Link-CGJ
        /// </summary>
        /// <stable>ICU 2.6</stable>
        GRAPHEME_EXTEND = 11,

        /// <summary>
        /// Binary property Grapheme_Link (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        GRAPHEME_LINK = 12,

        /// <summary>
        /// Binary property Hex_Digit.
        /// <para/>
        /// Characters commonly used for hexadecimal numbers.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        HEX_DIGIT = 13,

        /// <summary>
        /// Binary property Hyphen.
        /// <para/>
        /// Dashes used to mark connections between pieces of words, plus the
        /// Katakana middle dot.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        HYPHEN = 14,

        /// <summary>
        /// Binary property ID_Continue.
        /// <para/>
        /// Characters that can continue an identifier.
        /// <para/>
        /// ID_Start+Mn+Mc+Nd+Pc
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ID_CONTINUE = 15,

        /// <summary>
        /// Binary property ID_Start.
        /// <para/>
        /// Characters that can start an identifier.
        /// <para/>
        /// Lu+Ll+Lt+Lm+Lo+Nl
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ID_START = 16,

        /// <summary>
        /// Binary property Ideographic.
        /// <para/>
        /// CJKV ideographs.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        IDEOGRAPHIC = 17,

        /// <summary>
        /// Binary property IDS_Binary_Operator (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        IDS_BINARY_OPERATOR = 18,

        /// <summary>
        /// Binary property IDS_Trinary_Operator (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        IDS_TRINARY_OPERATOR = 19,

        /// <summary>
        /// Binary property Join_Control.
        /// <para/>
        /// Format controls for cursive joining and ligation.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        JOIN_CONTROL = 20,

        /// <summary>
        /// Binary property Logical_Order_Exception (new).
        /// <para/>
        /// Characters that do not use logical order and require special
        /// handling in most processing.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        LOGICAL_ORDER_EXCEPTION = 21,

        /// <summary>
        /// Binary property Lowercase.
        /// <para/>
        /// Same as <see cref="UCharacter.IsULowercase(int)"/>, different from
        /// <see cref="UCharacter.IsLowerCase(int)"/>.
        /// <para/>
        /// Ll+Other_Lowercase
        /// </summary>
        /// <stable>ICU 2.6</stable>
        LOWERCASE = 22,

        /// <summary>
        /// Binary property Math.
        /// <para/>
        /// Sm+Other_Math
        /// </summary>
        /// <stable>ICU 2.6</stable>
        MATH = 23,

        /// <summary>
        /// Binary property Noncharacter_Code_Point.
        /// <para/>
        /// Code points that are explicitly defined as illegal for the encoding
        /// of characters.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        NONCHARACTER_CODE_POINT = 24,

        /// <summary>
        /// Binary property Quotation_Mark.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        QUOTATION_MARK = 25,

        /// <summary>
        /// Binary property Radical (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        RADICAL = 26,

        /// <summary>
        /// Binary property Soft_Dotted (new).
        /// <para/>
        /// Characters with a "soft dot", like i or j.
        /// <para/>
        /// An accent placed on these characters causes the dot to disappear.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        SOFT_DOTTED = 27,

        /// <summary>
        /// Binary property Terminal_Punctuation.
        /// <para/>
        /// Punctuation characters that generally mark the end of textual
        /// units.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        TERMINAL_PUNCTUATION = 28,

        /// <summary>
        /// Binary property Unified_Ideograph (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        UNIFIED_IDEOGRAPH = 29,

        /// <summary>
        /// Binary property Uppercase.
        /// <para/>
        /// Same as <see cref="UCharacter.IsUUppercase(int)"/>, different from
        /// <see cref="UCharacter.IsUpperCase(int)"/>.
        /// <para/>
        /// Lu+Other_Uppercase
        /// </summary>
        /// <stable>ICU 2.6</stable>
        UPPERCASE = 30,

        /// <summary>
        /// Binary property White_Space.
        /// <para/>
        /// Same as <see cref="UCharacter.IsUWhiteSpace(int)"/>, different from
        /// <see cref="UCharacter.IsSpace(int)"/> and <see cref="UCharacter.IsWhitespace(int)"/>.
        /// Space characters+TAB+CR+LF-ZWSP-ZWNBSP
        /// </summary>
        /// <stable>ICU 2.6</stable>
        WHITE_SPACE = 31,

        /// <summary>
        /// Binary property XID_Continue.
        /// <para/>
        /// ID_Continue modified to allow closure under normalization forms
        /// NFKC and NFKD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        XID_CONTINUE = 32,

        /// <summary>
        /// Binary property XID_Start.
        /// <para/>
        /// ID_Start modified to allow closure under normalization forms NFKC
        /// and NFKD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        XID_START = 33,

        /// <summary>
        /// Binary property Case_Sensitive.
        /// <para/>
        /// Either the source of a case
        /// mapping or _in_ the target of a case mapping. Not the same as
        /// the general category Cased_Letter.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        CASE_SENSITIVE = 34,

        /// <summary>
        /// Binary property STerm (new in Unicode 4.0.1).
        /// Sentence Terminal. Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>)
        /// </summary>
        /// <stable>ICU 3.0</stable>
        S_TERM = 35,

        /// <summary>
        /// Binary property Variation_Selector (new in Unicode 4.0.1).
        /// Indicates all those characters that qualify as Variation Selectors.
        /// For details on the behavior of these characters,
        /// see StandardizedVariants.html and 15.6 Variation Selectors.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        VARIATION_SELECTOR = 36,

        /// <summary>
        /// Binary property NFD_Inert.
        /// ICU-specific property for characters that are inert under NFD,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// <para/>
        /// There is one such property per normalization form.
        /// These properties are computed as follows - an inert character is:
        /// a) unassigned, or ALL of the following:
        /// b) of combining class 0.
        /// c) not decomposed by this normalization form.
        /// AND if NFC or NFKC,
        /// d) can never compose with a previous character.
        /// e) can never compose with a following character.
        /// f) can never change if another character is added.
        /// Example: a-breve might satisfy all but f, but if you
        /// add an ogonek it changes to a-ogonek + breve
        /// <para/>
        /// See also com.ibm.text.UCD.NFSkippable in the ICU4J repository,
        /// and icu/source/common/unormimp.h .
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFD_INERT = 37,

        /// <summary>
        /// Binary property NFKD_Inert.
        /// ICU-specific property for characters that are inert under NFKD,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_INERT"/>
        /// <stable>ICU 3.0</stable>
        NFKD_INERT = 38,

        /// <summary>
        /// Binary property NFC_Inert.
        /// ICU-specific property for characters that are inert under NFC,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_INERT"/>
        /// <stable>ICU 3.0</stable>
        NFC_INERT = 39,

        /// <summary>
        /// Binary property NFKC_Inert.
        /// ICU-specific property for characters that are inert under NFKC,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_INERT"/>
        /// <stable>ICU 3.0</stable>
        NFKC_INERT = 40,

        /// <summary>
        /// Binary Property Segment_Starter.
        /// ICU-specific property for characters that are starters in terms of
        /// Unicode normalization and combining character sequences.
        /// They have ccc=0 and do not occur in non-initial position of the
        /// canonical decomposition of any character
        /// (like " in NFD(a-umlaut) and a Jamo T in an NFD(Hangul LVT)).
        /// ICU uses this property for segmenting a string for generating a set of
        /// canonically equivalent strings, e.g. for canonical closure while
        /// processing collation tailoring rules.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        SEGMENT_STARTER = 41,

        /// <summary>
        /// Binary property Pattern_Syntax (new in Unicode 4.1).
        /// See UAX #31 Identifier and Pattern Syntax
        /// (<a href="http://www.unicode.org/reports/tr31/">http://www.unicode.org/reports/tr31/</a>)
        /// </summary>
        /// <stable>ICU 3.4</stable>
        PATTERN_SYNTAX = 42,

        /// <summary>
        /// Binary property Pattern_White_Space (new in Unicode 4.1).
        /// See UAX #31 Identifier and Pattern Syntax
        /// (<a href="http://www.unicode.org/reports/tr31/">http://www.unicode.org/reports/tr31/</a>)
        /// </summary>
        /// <stable>ICU 3.4</stable>
        PATTERN_WHITE_SPACE = 43,

        /// <summary>
        /// Binary property alnum (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UCharacter"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_ALNUM = 44,

        /// <summary>
        /// Binary property blank (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UCharacter"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_BLANK = 45,

        /// <summary>
        /// Binary property graph (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UCharacter"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_GRAPH = 46,

        /// <summary>
        /// Binary property print (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UCharacter"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_PRINT = 47,

        /// <summary>
        /// Binary property xdigit (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UCharacter"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_XDIGIT = 48,

        /// <summary>
        /// Binary property Cased.
        /// For Lowercase, Uppercase and Titlecase characters.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CASED = 49,

        /// <summary>
        /// Binary property Case_Ignorable.
        /// Used in context-sensitive case mappings.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CASE_IGNORABLE = 50,

        /// <summary>
        /// Binary property Changes_When_Lowercased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_LOWERCASED = 51,

        /// <summary>
        /// Binary property Changes_When_Uppercased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_UPPERCASED = 52,

        /// <summary>
        /// Binary property Changes_When_Titlecased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_TITLECASED = 53,

        /// <summary>
        /// Binary property Changes_When_Casefolded.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_CASEFOLDED = 54,

        /// <summary>
        /// Binary property Changes_When_Casemapped.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_CASEMAPPED = 55,

        /// <summary>
        /// Binary property Changes_When_NFKC_Casefolded.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        CHANGES_WHEN_NFKC_CASEFOLDED = 56,

        /// <summary>
        /// Binary property Emoji.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        EMOJI = 57,

        /// <summary>
        /// Binary property Emoji_Presentation.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        EMOJI_PRESENTATION = 58,

        /// <summary>
        /// Binary property Emoji_Modifier.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        EMOJI_MODIFIER = 59,

        /// <summary>
        /// Binary property Emoji_Modifier_Base.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        EMOJI_MODIFIER_BASE = 60,

        /// <summary>
        /// Binary property Emoji_Component.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 60</stable>
        EMOJI_COMPONENT = 61,

        /// <summary>
        /// Binary property Regional_Indicator.
        /// </summary>
        /// <stable>ICU 60</stable>
        REGIONAL_INDICATOR = 62,

        /// <summary>
        /// Binary property Prepended_Concatenation_Mark.
        /// </summary>
        /// <stable>ICU 60</stable>
        PREPENDED_CONCATENATION_MARK = 63,

        /// <summary>
        /// One more than the last constant for binary Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        BINARY_LIMIT = 64,

        /// <summary>
        /// Enumerated property Bidi_Class.
        /// Same as <see cref="UCharacter.GetDirection(int)"/>, returns <see cref="UCharacterDirection"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        BIDI_CLASS = 0x1000,

        /// <summary>
        ///  First constant for enumerated/integer Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        INT_START = BIDI_CLASS,

        /// <summary>
        /// Enumerated property Block.
        /// Same as <see cref="UCharacter.UnicodeBlock.Of(int)"/>, returns <see cref="UCharacter.UnicodeBlock"/>
        /// values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        BLOCK = 0x1001,

        /// <summary>
        /// Enumerated property Canonical_Combining_Class.
        /// Same as <see cref="UCharacter.GetCombiningClass(int)"/>, returns 8-bit numeric values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        CANONICAL_COMBINING_CLASS = 0x1002,

        /// <summary>
        /// Enumerated property Decomposition_Type.
        /// Returns <see cref="UCharacter.DecompositionType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        DECOMPOSITION_TYPE = 0x1003,

        /// <summary>
        /// Enumerated property East_Asian_Width.
        /// See <a href="http://www.unicode.org/reports/tr11/">http://www.unicode.org/reports/tr11/</a>.
        /// Returns <see cref="UCharacter.EastAsianWidth"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        EAST_ASIAN_WIDTH = 0x1004,

        /// <summary>
        /// Enumerated property General_Category.
        /// Same as <see cref="UCharacter.GetType(int)"/>, returns <see cref="System.Globalization.UnicodeCategory"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        GENERAL_CATEGORY = 0x1005,

        /// <summary>
        /// Enumerated property Joining_Group.
        /// Returns <see cref="UCharacter.JoiningGroup"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        JOINING_GROUP = 0x1006,

        /// <summary>
        /// Enumerated property Joining_Type.
        /// Returns <see cref="UCharacter.JoiningType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        JOINING_TYPE = 0x1007,

        /// <summary>
        /// Enumerated property Line_Break.
        /// Returns <see cref="UCharacter.LineBreak"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        LINE_BREAK = 0x1008,

        /// <summary>
        /// Enumerated property Numeric_Type.
        /// Returns <see cref="UCharacter.NumericType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        NUMERIC_TYPE = 0x1009,

        /// <summary>
        /// Enumerated property Script.
        /// Same as <see cref="UScript.GetScript(int)"/>, returns <see cref="UScript"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        SCRIPT = 0x100A,

        /// <summary>
        /// Enumerated property Hangul_Syllable_Type, new in Unicode 4.
        /// Returns <see cref="UCharacter.HangulSyllableType"/> values.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        HANGUL_SYLLABLE_TYPE = 0x100B,

        /// <summary>
        /// Enumerated property NFD_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.NormalizerQuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFD_QUICK_CHECK = 0x100C,

        /// <summary>
        /// Enumerated property NFKD_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.NormalizerQuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFKD_QUICK_CHECK = 0x100D,

        /// <summary>
        /// Enumerated property NFC_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.NormalizerQuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFC_QUICK_CHECK = 0x100E,

        /// <summary>
        /// Enumerated property NFKC_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.NormalizerQuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFKC_QUICK_CHECK = 0x100F,

        /// <summary>
        /// Enumerated property Lead_Canonical_Combining_Class.
        /// ICU-specific property for the ccc of the first code point
        /// of the decomposition, or lccc(c)=ccc(NFD(c)[0]).
        /// Useful for checking for canonically ordered text,
        /// see <see cref="Text.Normalizer.FCD"/> and 
        /// <a href="http://www.unicode.org/notes/tn5/#FCD">http://www.unicode.org/notes/tn5/#FCD</a>.
        /// Returns 8-bit numeric values like <see cref="CANONICAL_COMBINING_CLASS"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        LEAD_CANONICAL_COMBINING_CLASS = 0x1010,

        /// <summary>
        /// Enumerated property Trail_Canonical_Combining_Class.
        /// ICU-specific property for the ccc of the last code point
        /// of the decomposition, or lccc(c)=ccc(NFD(c)[last]).
        /// Useful for checking for canonically ordered text,
        /// see <see cref="Text.Normalizer.FCD"/> and 
        /// <a href="http://www.unicode.org/notes/tn5/#FCD">http://www.unicode.org/notes/tn5/#FCD</a>.
        /// Returns 8-bit numeric values like <see cref="CANONICAL_COMBINING_CLASS"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        TRAIL_CANONICAL_COMBINING_CLASS = 0x1011,

        /// <summary>
        /// Enumerated property Grapheme_Cluster_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UCharacter.GraphemeClusterBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        GRAPHEME_CLUSTER_BREAK = 0x1012,

        /// <summary>
        /// Enumerated property Sentence_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UCharacter.SentenceBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        SENTENCE_BREAK = 0x1013,

        /// <summary>
        /// Enumerated property Word_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UCharacter.WordBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        WORD_BREAK = 0x1014,

        /// <summary>
        /// Enumerated property Bidi_Paired_Bracket_Type (new in Unicode 6.3).
        /// Used in UAX #9: Unicode Bidirectional Algorithm
        /// (<a href="http://www.unicode.org/reports/tr9/">http://www.unicode.org/reports/tr9/</a>).
        /// Returns <see cref="UCharacter.BidiPairedBracketType"/> values.
        /// </summary>
        /// <stable>ICU 52</stable>
        BIDI_PAIRED_BRACKET_TYPE = 0x1015,

        /// <summary>
        /// One more than the last constant for enumerated/integer Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        INT_LIMIT = 0x1016,

        /// <summary>
        /// Bitmask property General_Category_Mask.
        /// This is the General_Category property returned as a bit mask.
        /// When used in <see cref="UCharacter.GetInt32PropertyValue(int, UProperty)"/>,
        /// returns bit masks for <see cref="System.Globalization.UnicodeCategory"/> values where exactly one bit is set.
        /// When used with <see cref="UCharacter.GetPropertyValueName(UProperty, int, NameChoice)"/> 
        /// and <see cref="UCharacter.GetPropertyValueEnum(UProperty, string)"/>,
        /// a multi-bit mask is used for sets of categories like "Letters".
        /// </summary>
        /// <stable>ICU 2.4</stable>
        GENERAL_CATEGORY_MASK = 0x2000,

        /// <summary>
        /// First constant for bit-mask Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        MASK_START = GENERAL_CATEGORY_MASK,

        /// <summary>
        /// One more than the last constant for bit-mask Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        MASK_LIMIT = 0x2001,

        /// <summary>
        /// Double property Numeric_Value.
        /// Corresponds to <see cref="UCharacter.GetUnicodeNumericValue(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        NUMERIC_VALUE = 0x3000,

        /// <summary>
        /// First constant for double Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        DOUBLE_START = NUMERIC_VALUE,

        /// <summary>
        /// One more than the last constant for double Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        DOUBLE_LIMIT = 0x3001,

        /// <summary>
        /// String property Age.
        /// Corresponds to <see cref="UCharacter.GetAge(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        AGE = 0x4000,

        /// <summary>
        /// First constant for string Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        STRING_START = AGE,

        /// <summary>
        /// String property Bidi_Mirroring_Glyph.
        /// Corresponds to <see cref="UCharacter.GetMirror(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        BIDI_MIRRORING_GLYPH = 0x4001,

        /// <summary>
        /// String property Case_Folding.
        /// Corresponds to <see cref="UCharacter.FoldCase(string, bool)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        CASE_FOLDING = 0x4002,

        /// <summary>
        /// Deprecated string property ISO_Comment.
        /// Corresponds to <see cref="UCharacter.GetISOComment(int)"/>.
        /// </summary>
        [Obsolete("ICU 49")]
        ISO_COMMENT = 0x4003,

        /// <summary>
        /// String property Lowercase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToLower(string)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        LOWERCASE_MAPPING = 0x4004,

        /// <summary>
        /// String property Name.
        /// Corresponds to <see cref="UCharacter.GetName(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        NAME = 0x4005,

        /// <summary>
        /// String property Simple_Case_Folding.
        /// Corresponds to <see cref="UCharacter.FoldCase(int, bool)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        SIMPLE_CASE_FOLDING = 0x4006,

        /// <summary>
        /// String property Simple_Lowercase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToLower(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        SIMPLE_LOWERCASE_MAPPING = 0x4007,

        /// <summary>
        /// String property Simple_Titlecase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToTitleCase(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        SIMPLE_TITLECASE_MAPPING = 0x4008,

        /// <summary>
        /// String property Simple_Uppercase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToUpper(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        SIMPLE_UPPERCASE_MAPPING = 0x4009,

        /// <summary>
        /// String property Titlecase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToTitleCase(string)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        TITLECASE_MAPPING = 0x400A,

        /// <summary>
        /// String property Unicode_1_Name.
        /// This property is of little practical value.
        /// Beginning with ICU 49, ICU APIs return null or an empty string for this property.
        /// Corresponds to <see cref="UCharacter.GetName1_0(int)"/>.
        /// </summary>
        [Obsolete("ICU 49")]
        UNICODE_1_NAME = 0x400B,

        /// <summary>
        /// String property Uppercase_Mapping.
        /// Corresponds to <see cref="UCharacter.ToUpper(string)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        UPPERCASE_MAPPING = 0x400C,

        /// <summary>
        /// String property Bidi_Paired_Bracket (new in Unicode 6.3).
        /// Corresponds to <see cref="UCharacter.GetBidiPairedBracket(int)"/>.
        /// </summary>
        /// <stable>ICU 52</stable>
        BIDI_PAIRED_BRACKET = 0x400D,

        /// <summary>
        /// One more than the last constant for string Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        STRING_LIMIT = 0x400E,

        /// <summary>
        /// Miscellaneous property Script_Extensions (new in Unicode 6.0).
        /// Some characters are commonly used in multiple scripts.
        /// For more information, see UAX #24: <a href="http://www.unicode.org/reports/tr24/">http://www.unicode.org/reports/tr24/</a>.
        /// Corresponds to <see cref="UScript.HasScript(int, int)"/> and <see cref="UScript.GetScriptExtensions(int, Support.Collections.BitSet)"/>.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        SCRIPT_EXTENSIONS = 0x7000,

        /// <summary>
        /// First constant for Unicode properties with unusual value types.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        OTHER_PROPERTY_START = SCRIPT_EXTENSIONS,

        /// <summary>
        /// One more than the last constant for Unicode properties with unusual value types.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        OTHER_PROPERTY_LIMIT = 0x7001,
    }

    /// <summary>
    /// Selector constants for <see cref="UCharacter.GetPropertyName(UProperty, NameChoice)"/> and
    /// <see cref="UCharacter.GetPropertyValueName(UProperty, int, NameChoice)"/>.  These selectors are used to
    /// choose which name is returned for a given property or value.
    /// All properties and values have a long name.  Most have a short
    /// name, but some do not.  Unicode allows for additional names,
    /// beyond the long and short name, which would be indicated by
    /// LONG + i, where i=1, 2,...
    /// </summary>
    /// <seealso cref="UCharacter.GetPropertyName(UProperty, NameChoice)"/>
    /// <seealso cref="UCharacter.GetPropertyValueName(UProperty, int, NameChoice)"/>
    /// <stable>ICU 2.4</stable>
    public enum NameChoice 
    {
        /// <summary>
        /// Selector for the abbreviated name of a property or value.
        /// Most properties and values have a short name, those that do
        /// not return null.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Short = 0,

        /// <summary>
        /// Selector for the long name of a property or value.  All
        /// properties and values have a long name.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Long = 1,

        /// <summary>
        /// The number of predefined property name choices.  Individual
        /// properties or values may have more than <see cref="Count"/> aliases.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Count = 2,
    }
}
