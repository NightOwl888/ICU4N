using System;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Selection values for Unicode properties.
    /// <para/>
    /// These values are used in functions like
    /// <see cref="UChar.HasBinaryProperty(int, UProperty)"/> to select one of the Unicode properties.
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
    /// available. Check <see cref="UChar.UnicodeVersion"/> to be sure.
    /// </remarks>
    /// <seealso cref="UChar"/>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.6</stable>
    public enum UProperty
    {
        /// <summary>
        /// Special value indicating undefined property.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        Undefined = -1,

        /// <summary>
        /// Binary property Alphabetic.
        /// <para/>
        /// Property for <see cref="UChar.IsUAlphabetic(int)"/>, different from the property
        /// in <see cref="UChar.IsUAlphabetic(int)"/>.
        /// <para/>
        /// Lu + Ll + Lt + Lm + Lo + Nl + Other_Alphabetic.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Alphabetic = 0,

        /// <summary>
        /// First constant for binary Unicode properties.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Binary_Start = Alphabetic,

        /// <summary>
        /// Binary property ASCII_Hex_Digit (0-9 A-F a-f).
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ASCII_Hex_Digit = 1,

        /// <summary>
        /// Binary property Bidi_Control.
        /// <para/>
        /// Format controls which have specific functions in the Bidi Algorithm.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Bidi_Control = 2, // ICU4N TODO: API Rename BiDi

        /// <summary>
        /// Binary property Bidi_Mirrored.
        /// <para/>
        /// Characters that may change display in RTL text.
        /// <para/>
        /// Property for <see cref="UChar.IsMirrored(int)"/>.
        /// <para/>
        /// See Bidi Algorithm, UTR 9.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Bidi_Mirrored = 3, // ICU4N TODO: API Rename BiDi

        /// <summary>
        /// Binary property Dash.
        /// <para/>
        /// Variations of dashes.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Dash = 4,

        /// <summary>
        /// Binary property Default_Ignorable_Code_Point (new).
        /// <para/>
        /// Property that indicates codepoint is ignorable in most processing.
        /// <para/>
        /// Codepoints (2060..206F, FFF0..FFFB, E0000..E0FFF) +
        /// Other_Default_Ignorable_Code_Point + (Cf + Cc + Cs - White_Space)
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Default_Ignorable_Code_Point = 5,

        /// <summary>
        /// Binary property Deprecated (new).
        /// <para/>
        /// The usage of deprecated characters is strongly discouraged.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Deprecated = 6,

        /// <summary>
        /// Binary property Diacritic.
        /// <para/>
        /// Characters that linguistically modify the meaning of another
        /// character to which they apply.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Diacritic = 7,

        /// <summary>
        /// Binary property Extender.
        /// <para/>
        /// Extend the value or shape of a preceding alphabetic character, e.g.
        /// length and iteration marks.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Extender = 8,

        /// <summary>
        /// Binary property Full_Composition_Exclusion.
        /// <para/>
        /// CompositionExclusions.txt + Singleton Decompositions +
        /// Non-Starter Decompositions.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Full_Composition_Exclusion = 9,

        /// <summary>
        /// Binary property Grapheme_Base (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// [0..10FFFF]-Cc-Cf-Cs-Co-Cn-Zl-Zp-Grapheme_Link-Grapheme_Extend-CGJ
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Grapheme_Base = 10,

        /// <summary>
        /// Binary property Grapheme_Extend (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// <para/>
        /// Me+Mn+Mc+Other_Grapheme_Extend-Grapheme_Link-CGJ
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Grapheme_Extend = 11,

        /// <summary>
        /// Binary property Grapheme_Link (new).
        /// <para/>
        /// For programmatic determination of grapheme cluster boundaries.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Grapheme_Link = 12,

        /// <summary>
        /// Binary property Hex_Digit.
        /// <para/>
        /// Characters commonly used for hexadecimal numbers.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Hex_Digit = 13,

        /// <summary>
        /// Binary property Hyphen.
        /// <para/>
        /// Dashes used to mark connections between pieces of words, plus the
        /// Katakana middle dot.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Hyphen = 14,

        /// <summary>
        /// Binary property ID_Continue.
        /// <para/>
        /// Characters that can continue an identifier.
        /// <para/>
        /// ID_Start+Mn+Mc+Nd+Pc
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ID_Continue = 15,

        /// <summary>
        /// Binary property ID_Start.
        /// <para/>
        /// Characters that can start an identifier.
        /// <para/>
        /// Lu+Ll+Lt+Lm+Lo+Nl
        /// </summary>
        /// <stable>ICU 2.6</stable>
        ID_Start = 16,

        /// <summary>
        /// Binary property Ideographic.
        /// <para/>
        /// CJKV ideographs.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Ideographic = 17,

        /// <summary>
        /// Binary property IDS_Binary_Operator (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        IDS_Binary_Operator = 18,

        /// <summary>
        /// Binary property IDS_Trinary_Operator (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        IDS_Trinary_Operator = 19,

        /// <summary>
        /// Binary property Join_Control.
        /// <para/>
        /// Format controls for cursive joining and ligation.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Join_Control = 20,

        /// <summary>
        /// Binary property Logical_Order_Exception (new).
        /// <para/>
        /// Characters that do not use logical order and require special
        /// handling in most processing.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Logical_Order_Exception = 21,

        /// <summary>
        /// Binary property Lowercase.
        /// <para/>
        /// Same as <see cref="UChar.IsULower(int)"/>, different from
        /// <see cref="UChar.IsLower(int)"/>.
        /// <para/>
        /// Ll+Other_Lowercase
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Lowercase = 22,

        /// <summary>
        /// Binary property Math.
        /// <para/>
        /// Sm+Other_Math
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Math = 23,

        /// <summary>
        /// Binary property Noncharacter_Code_Point.
        /// <para/>
        /// Code points that are explicitly defined as illegal for the encoding
        /// of characters.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Noncharacter_Code_Point = 24,

        /// <summary>
        /// Binary property Quotation_Mark.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Quotation_Mark = 25,

        /// <summary>
        /// Binary property Radical (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Radical = 26,

        /// <summary>
        /// Binary property Soft_Dotted (new).
        /// <para/>
        /// Characters with a "soft dot", like i or j.
        /// <para/>
        /// An accent placed on these characters causes the dot to disappear.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Soft_Dotted = 27,

        /// <summary>
        /// Binary property Terminal_Punctuation.
        /// <para/>
        /// Punctuation characters that generally mark the end of textual
        /// units.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Terminal_Punctuation = 28,

        /// <summary>
        /// Binary property Unified_Ideograph (new).
        /// <para/>
        /// For programmatic determination of Ideographic Description
        /// Sequences.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Unified_Ideograph = 29,

        /// <summary>
        /// Binary property Uppercase.
        /// <para/>
        /// Same as <see cref="UChar.IsUUpper(int)"/>, different from
        /// <see cref="UChar.IsUpper(int)"/>.
        /// <para/>
        /// Lu+Other_Uppercase
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Uppercase = 30,

        /// <summary>
        /// Binary property White_Space.
        /// <para/>
        /// Same as <see cref="UChar.IsUWhiteSpace(int)"/>, different from
        /// <see cref="UChar.IsSpace(int)"/> and <see cref="UChar.IsWhiteSpace(int)"/>.
        /// Space characters+TAB+CR+LF-ZWSP-ZWNBSP
        /// </summary>
        /// <stable>ICU 2.6</stable>
        White_Space = 31,

        /// <summary>
        /// Binary property XID_Continue.
        /// <para/>
        /// ID_Continue modified to allow closure under normalization forms
        /// NFKC and NFKD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        XID_Continue = 32,

        /// <summary>
        /// Binary property XID_Start.
        /// <para/>
        /// ID_Start modified to allow closure under normalization forms NFKC
        /// and NFKD.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        XID_Start = 33,

        /// <summary>
        /// Binary property Case_Sensitive.
        /// <para/>
        /// Either the source of a case
        /// mapping or _in_ the target of a case mapping. Not the same as
        /// the general category Cased_Letter.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Case_Sensitive = 34,

        /// <summary>
        /// Binary property STerm (new in Unicode 4.0.1).
        /// Sentence Terminal. Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>)
        /// </summary>
        /// <stable>ICU 3.0</stable>
        STerm = 35,

        /// <summary>
        /// Binary property Variation_Selector (new in Unicode 4.0.1).
        /// Indicates all those characters that qualify as Variation Selectors.
        /// For details on the behavior of these characters,
        /// see StandardizedVariants.html and 15.6 Variation Selectors.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        Variation_Selector = 36,

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
        NFD_Inert = 37,

        /// <summary>
        /// Binary property NFKD_Inert.
        /// ICU-specific property for characters that are inert under NFKD,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_Inert"/>
        /// <stable>ICU 3.0</stable>
        NFKD_Inert = 38,

        /// <summary>
        /// Binary property NFC_Inert.
        /// ICU-specific property for characters that are inert under NFC,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_Inert"/>
        /// <stable>ICU 3.0</stable>
        NFC_Inert = 39,

        /// <summary>
        /// Binary property NFKC_Inert.
        /// ICU-specific property for characters that are inert under NFKC,
        /// i.e., they do not interact with adjacent characters.
        /// Used for example in normalizing transforms in incremental mode
        /// to find the boundary of safely normalizable text despite possible
        /// text additions.
        /// </summary>
        /// <seealso cref="NFD_Inert"/>
        /// <stable>ICU 3.0</stable>
        NFKC_Inert = 40,

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
        Segment_Starter = 41,

        /// <summary>
        /// Binary property Pattern_Syntax (new in Unicode 4.1).
        /// See UAX #31 Identifier and Pattern Syntax
        /// (<a href="http://www.unicode.org/reports/tr31/">http://www.unicode.org/reports/tr31/</a>)
        /// </summary>
        /// <stable>ICU 3.4</stable>
        Pattern_Syntax = 42,

        /// <summary>
        /// Binary property Pattern_White_Space (new in Unicode 4.1).
        /// See UAX #31 Identifier and Pattern Syntax
        /// (<a href="http://www.unicode.org/reports/tr31/">http://www.unicode.org/reports/tr31/</a>)
        /// </summary>
        /// <stable>ICU 3.4</stable>
        Pattern_White_Space = 43,

        /// <summary>
        /// Binary property alnum (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UChar"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_Alnum = 44,

        /// <summary>
        /// Binary property blank (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UChar"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_Blank = 45,

        /// <summary>
        /// Binary property graph (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UChar"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_Graph = 46,

        /// <summary>
        /// Binary property print (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UChar"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_Print = 47,

        /// <summary>
        /// Binary property xdigit (a C/POSIX character class).
        /// Implemented according to the UTS #18 Annex C Standard Recommendation.
        /// See the <see cref="UChar"/> class documentation.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        POSIX_XDigit = 48,

        /// <summary>
        /// Binary property Cased.
        /// For Lowercase, Uppercase and Titlecase characters.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Cased = 49,

        /// <summary>
        /// Binary property Case_Ignorable.
        /// Used in context-sensitive case mappings.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Case_Ignorable = 50,

        /// <summary>
        /// Binary property Changes_When_Lowercased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_Lowercased = 51,

        /// <summary>
        /// Binary property Changes_When_Uppercased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_Uppercased = 52,

        /// <summary>
        /// Binary property Changes_When_Titlecased.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_Titlecased = 53,

        /// <summary>
        /// Binary property Changes_When_Casefolded.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_Casefolded = 54,

        /// <summary>
        /// Binary property Changes_When_Casemapped.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_Casemapped = 55,

        /// <summary>
        /// Binary property Changes_When_NFKC_Casefolded.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        Changes_When_NFKC_Casefolded = 56,

        /// <summary>
        /// Binary property Emoji.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        Emoji = 57,

        /// <summary>
        /// Binary property Emoji_Presentation.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        Emoji_Presentation = 58,

        /// <summary>
        /// Binary property Emoji_Modifier.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        Emoji_Modifier = 59,

        /// <summary>
        /// Binary property Emoji_Modifier_Base.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 57</stable>
        Emoji_Modifier_Base = 60,

        /// <summary>
        /// Binary property Emoji_Component.
        /// See <a href="http://www.unicode.org/reports/tr51/#Emoji_Properties">http://www.unicode.org/reports/tr51/#Emoji_Properties</a>
        /// </summary>
        /// <stable>ICU 60</stable>
        Emoji_Component = 61,

        /// <summary>
        /// Binary property Regional_Indicator.
        /// </summary>
        /// <stable>ICU 60</stable>
        Regional_Indicator = 62,

        /// <summary>
        /// Binary property Prepended_Concatenation_Mark.
        /// </summary>
        /// <stable>ICU 60</stable>
        Prepended_Concatenation_Mark = 63,

        /// <summary>
        /// One more than the last constant for binary Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Binary_Limit = 64,

        /// <summary>
        /// Enumerated property Bidi_Class.
        /// Same as <see cref="UChar.GetDirection(int)"/>, returns <see cref="UCharacterDirection"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        BiDi_Class = 0x1000,

        /// <summary>
        ///  First constant for enumerated/integer Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Int_Start = BiDi_Class,

        /// <summary>
        /// Enumerated property Block.
        /// Same as <see cref="UChar.UnicodeBlock.Of(int)"/>, returns <see cref="UChar.UnicodeBlock"/>
        /// values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Block = 0x1001,

        /// <summary>
        /// Enumerated property Canonical_Combining_Class.
        /// Same as <see cref="UChar.GetCombiningClass(int)"/>, returns 8-bit numeric values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Canonical_Combining_Class = 0x1002,

        /// <summary>
        /// Enumerated property Decomposition_Type.
        /// Returns <see cref="UChar.DecompositionType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Decomposition_Type = 0x1003,

        /// <summary>
        /// Enumerated property East_Asian_Width.
        /// See <a href="http://www.unicode.org/reports/tr11/">http://www.unicode.org/reports/tr11/</a>.
        /// Returns <see cref="UChar.EastAsianWidth"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        East_Asian_Width = 0x1004,

        /// <summary>
        /// Enumerated property General_Category.
        /// Same as <see cref="UChar.GetType(int)"/>, returns <see cref="UUnicodeCategory"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        General_Category = 0x1005,

        /// <summary>
        /// Enumerated property Joining_Group.
        /// Returns <see cref="UChar.JoiningGroup"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Joining_Group = 0x1006,

        /// <summary>
        /// Enumerated property Joining_Type.
        /// Returns <see cref="UChar.JoiningType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Joining_Type = 0x1007,

        /// <summary>
        /// Enumerated property Line_Break.
        /// Returns <see cref="UChar.LineBreak"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Line_Break = 0x1008,

        /// <summary>
        /// Enumerated property Numeric_Type.
        /// Returns <see cref="UChar.NumericType"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Numeric_Type = 0x1009,

        /// <summary>
        /// Enumerated property Script.
        /// Same as <see cref="UScript.GetScript(int)"/>, returns <see cref="UScript"/> values.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Script = 0x100A,

        /// <summary>
        /// Enumerated property Hangul_Syllable_Type, new in Unicode 4.
        /// Returns <see cref="UChar.HangulSyllableType"/> values.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        Hangul_Syllable_Type = 0x100B,

        /// <summary>
        /// Enumerated property NFD_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.QuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFD_Quick_Check = 0x100C,

        /// <summary>
        /// Enumerated property NFKD_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.QuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFKD_Quick_Check = 0x100D,

        /// <summary>
        /// Enumerated property NFC_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.QuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFC_Quick_Check = 0x100E,

        /// <summary>
        /// Enumerated property NFKC_Quick_Check.
        /// Returns numeric values compatible with <see cref="Text.QuickCheckResult"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        NFKC_Quick_Check = 0x100F,

        /// <summary>
        /// Enumerated property Lead_Canonical_Combining_Class.
        /// ICU-specific property for the ccc of the first code point
        /// of the decomposition, or lccc(c)=ccc(NFD(c)[0]).
        /// Useful for checking for canonically ordered text,
        /// see <see cref="Text.Normalizer.FCD"/> and 
        /// <a href="http://www.unicode.org/notes/tn5/#FCD">http://www.unicode.org/notes/tn5/#FCD</a>.
        /// Returns 8-bit numeric values like <see cref="Canonical_Combining_Class"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        Lead_Canonical_Combining_Class = 0x1010,

        /// <summary>
        /// Enumerated property Trail_Canonical_Combining_Class.
        /// ICU-specific property for the ccc of the last code point
        /// of the decomposition, or lccc(c)=ccc(NFD(c)[last]).
        /// Useful for checking for canonically ordered text,
        /// see <see cref="Text.Normalizer.FCD"/> and 
        /// <a href="http://www.unicode.org/notes/tn5/#FCD">http://www.unicode.org/notes/tn5/#FCD</a>.
        /// Returns 8-bit numeric values like <see cref="Canonical_Combining_Class"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        Trail_Canonical_Combining_Class = 0x1011,

        /// <summary>
        /// Enumerated property Grapheme_Cluster_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UChar.GraphemeClusterBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        Grapheme_Cluster_Break = 0x1012,

        /// <summary>
        /// Enumerated property Sentence_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UChar.SentenceBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        Sentence_Break = 0x1013,

        /// <summary>
        /// Enumerated property Word_Break (new in Unicode 4.1).
        /// Used in UAX #29: Text Boundaries
        /// (<a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>).
        /// Returns <see cref="UChar.WordBreak"/> values.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        Word_Break = 0x1014,

        /// <summary>
        /// Enumerated property Bidi_Paired_Bracket_Type (new in Unicode 6.3).
        /// Used in UAX #9: Unicode Bidirectional Algorithm
        /// (<a href="http://www.unicode.org/reports/tr9/">http://www.unicode.org/reports/tr9/</a>).
        /// Returns <see cref="UChar.BidiPairedBracketType"/> values.
        /// </summary>
        /// <stable>ICU 52</stable>
        Bidi_Paired_Bracket_Type = 0x1015, // ICU4N TODO: API Rename BiDi

        /// <summary>
        /// One more than the last constant for enumerated/integer Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Int_Limit = 0x1016,

        /// <summary>
        /// Bitmask property General_Category_Mask.
        /// This is the <see cref="General_Category"/> property returned as a bit mask.
        /// When used in <see cref="UChar.GetIntPropertyValue(int, UProperty)"/>,
        /// returns bit masks for <see cref="UUnicodeCategory"/> values where exactly one bit is set.
        /// When used with <see cref="UChar.GetPropertyValueName(UProperty, int, NameChoice)"/> 
        /// and <see cref="UChar.GetPropertyValueEnum(UProperty, string)"/>,
        /// a multi-bit mask is used for sets of categories like "Letters".
        /// </summary>
        /// <stable>ICU 2.4</stable>
        General_Category_Mask = 0x2000,

        /// <summary>
        /// First constant for bit-mask Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Mask_Start = General_Category_Mask,

        /// <summary>
        /// One more than the last constant for bit-mask Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Mask_Limit = 0x2001,

        /// <summary>
        /// Double property Numeric_Value.
        /// Corresponds to <see cref="UChar.GetUnicodeNumericValue(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Numeric_Value = 0x3000,

        /// <summary>
        /// First constant for double Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Double_Start = Numeric_Value,

        /// <summary>
        /// One more than the last constant for double Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Double_Limit = 0x3001,

        /// <summary>
        /// String property Age.
        /// Corresponds to <see cref="UChar.GetAge(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Age = 0x4000,

        /// <summary>
        /// First constant for string Unicode properties.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        String_Start = Age,

        /// <summary>
        /// String property Bidi_Mirroring_Glyph.
        /// Corresponds to <see cref="UChar.GetMirror(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Bidi_Mirroring_Glyph = 0x4001, // ICU4N TODO: API Rename BiDi

        /// <summary>
        /// String property Case_Folding.
        /// Corresponds to <see cref="UChar.FoldCase(string, bool)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Case_Folding = 0x4002,

        /// <summary>
        /// Deprecated string property ISO_Comment.
        /// Corresponds to <see cref="UChar.GetISOComment(int)"/>.
        /// </summary>
        [Obsolete("ICU 49")]
        ISO_Comment = 0x4003,

        /// <summary>
        /// String property Lowercase_Mapping.
        /// Corresponds to <see cref="UChar.ToLower(string)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Lowercase_Mapping = 0x4004,

        /// <summary>
        /// String property Name.
        /// Corresponds to <see cref="UChar.GetName(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Name = 0x4005,

        /// <summary>
        /// String property Simple_Case_Folding.
        /// Corresponds to <see cref="UChar.FoldCase(int, bool)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Simple_Case_Folding = 0x4006,

        /// <summary>
        /// String property Simple_Lowercase_Mapping.
        /// Corresponds to <see cref="UChar.ToLower(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Simple_Lowercase_Mapping = 0x4007,

        /// <summary>
        /// String property Simple_Titlecase_Mapping.
        /// Corresponds to <see cref="UChar.ToTitleCase(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Simple_Titlecase_Mapping = 0x4008,

        /// <summary>
        /// String property Simple_Uppercase_Mapping.
        /// Corresponds to <see cref="UChar.ToUpper(int)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Simple_Uppercase_Mapping = 0x4009,

        /// <summary>
        /// String property Titlecase_Mapping.
        /// Corresponds to <see cref="UChar.ToTitleCase(string, Text.BreakIterator)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Titlecase_Mapping = 0x400A,

        /// <summary>
        /// String property Unicode_1_Name.
        /// This property is of little practical value.
        /// Beginning with ICU 49, ICU APIs return null or an empty string for this property.
        /// Corresponds to <see cref="UChar.GetName1_0(int)"/>.
        /// </summary>
        [Obsolete("ICU 49")]
        Unicode_1_Name = 0x400B,

        /// <summary>
        /// String property Uppercase_Mapping.
        /// Corresponds to <see cref="UChar.ToUpper(string)"/>.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        Uppercase_Mapping = 0x400C,

        /// <summary>
        /// String property Bidi_Paired_Bracket (new in Unicode 6.3).
        /// Corresponds to <see cref="UChar.GetBidiPairedBracket(int)"/>.
        /// </summary>
        /// <stable>ICU 52</stable>
        Bidi_Paired_Bracket = 0x400D, // ICU4N TODO: API Rename BiDi

        /// <summary>
        /// One more than the last constant for string Unicode properties.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        String_Limit = 0x400E,

        /// <summary>
        /// Miscellaneous property Script_Extensions (new in Unicode 6.0).
        /// Some characters are commonly used in multiple scripts.
        /// For more information, see UAX #24: <a href="http://www.unicode.org/reports/tr24/">http://www.unicode.org/reports/tr24/</a>.
        /// Corresponds to <see cref="UScript.HasScript(int, int)"/> and <see cref="UScript.GetScriptExtensions(int, Support.Collections.BitSet)"/>.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Script_Extensions = 0x7000,

        /// <summary>
        /// First constant for Unicode properties with unusual value types.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Other_Property_Start = Script_Extensions,

        /// <summary>
        /// One more than the last constant for Unicode properties with unusual value types.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        Other_Property_Limit = 0x7001,
    }

    /// <summary>
    /// Selector constants for <see cref="UChar.GetPropertyName(UProperty, NameChoice)"/> and
    /// <see cref="UChar.GetPropertyValueName(UProperty, int, NameChoice)"/>.  These selectors are used to
    /// choose which name is returned for a given property or value.
    /// All properties and values have a long name.  Most have a short
    /// name, but some do not.  Unicode allows for additional names,
    /// beyond the long and short name, which would be indicated by
    /// LONG + i, where i=1, 2,...
    /// </summary>
    /// <seealso cref="UChar.GetPropertyName(UProperty, NameChoice)"/>
    /// <seealso cref="UChar.GetPropertyValueName(UProperty, int, NameChoice)"/>
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
