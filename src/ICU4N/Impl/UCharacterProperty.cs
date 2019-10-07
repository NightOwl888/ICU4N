using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using static ICU4N.UChar;

namespace ICU4N.Impl
{
    /// <summary>
    /// Internal class used for Unicode character property database.
    /// </summary>
    /// <remarks>
    /// This classes store binary data read from uprops.icu.
    /// It does not have the capability to parse the data into more high-level
    /// information. It only returns bytes of information when required.
    /// <para/>
    /// Due to the form most commonly used for retrieval, array of char is used
    /// to store the binary data.
    /// <para/>
    /// UCharacterPropertyDB also contains information on accessing indexes to
    /// significant points in the binary data.
    /// <para/>
    /// Responsibility for molding the binary data into more meaning form lies on
    /// <see cref="UChar"/>.
    /// </remarks>
    /// <author>Syn Wee Quek</author>
    /// <since>release 2.1, february 1st 2002</since>
    public sealed class UCharacterProperty
    {
        // public data members -----------------------------------------------

        private static readonly UCharacterProperty instance;

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static UCharacterProperty Instance
        {
            get { return instance; }
        }

        private Trie2_16 m_trie_;
        /// <summary>
        /// Trie data.
        /// </summary>
        public Trie2_16 Trie { get { return m_trie_; } }


        private VersionInfo m_unicodeVersion_;
        /// <summary>
        /// Unicode version.
        /// </summary>
        public VersionInfo UnicodeVersion { get { return m_unicodeVersion_; } }

        /// <summary>
        /// Latin capital letter i with dot above
        /// </summary>
        public const char LATIN_CAPITAL_LETTER_I_WITH_DOT_ABOVE_ = (char)0x130;
        /// <summary>
        /// Latin small letter i with dot above
        /// </summary>
        public const char LATIN_SMALL_LETTER_DOTLESS_I_ = (char)0x131;
        /// <summary>
        /// Latin lowercase i
        /// </summary>
        public const char LATIN_SMALL_LETTER_I_ = (char)0x69;
        /// <summary>
        /// Character type mask
        /// </summary>
        public const int TYPE_MASK = 0x1F;

        // uprops.h enum UPropertySource --------------------------------------- ***

        // ICU4N TODO: API - Make into enum?

        /// <summary>No source, not a supported property.</summary>
        public const int SRC_NONE = 0;
        /// <summary>From uchar.c/uprops.icu main trie</summary>
        public const int SRC_CHAR = 1;
        /// <summary>From uchar.c/uprops.icu properties vectors trie</summary>
        public const int SRC_PROPSVEC = 2;
        /// <summary>From unames.c/unames.icu</summary>
        public const int SRC_NAMES = 3;
        /// <summary>From ucase.c/ucase.icu</summary>
        public const int SRC_CASE = 4;
        /// <summary>From ubidi_props.c/ubidi.icu</summary>
        public const int SRC_BIDI = 5;
        /// <summary>From uchar.c/uprops.icu main trie as well as properties vectors trie</summary>
        public const int SRC_CHAR_AND_PROPSVEC = 6;
        /// <summary>From ucase.c/ucase.icu as well as unorm.cpp/unorm.icu</summary>
        public const int SRC_CASE_AND_NORM = 7;
        /// <summary>From normalizer2impl.cpp/nfc.nrm</summary>
        public const int SRC_NFC = 8;
        /// <summary>From normalizer2impl.cpp/nfkc.nrm</summary>
        public const int SRC_NFKC = 9;
        /// <summary>From normalizer2impl.cpp/nfkc_cf.nrm</summary>
        public const int SRC_NFKC_CF = 10;
        /// <summary>From normalizer2impl.cpp/nfc.nrm canonical iterator data</summary>
        public const int SRC_NFC_CANON_ITER = 11;
        /// <summary>One more than the highest UPropertySource (SRC_) constant.</summary>
        public const int SRC_COUNT = 12;

        // public methods ----------------------------------------------------

        /// <summary>
        /// Gets the main property value for code point <paramref name="ch"/>.
        /// </summary>
        /// <param name="ch">Code point whose property value is to be retrieved.</param>
        /// <returns>Property value of code point.</returns>
        public int GetProperty(int ch)
        {
            return m_trie_.Get(ch);
        }

        /// <summary>
        /// Gets the unicode additional properties.
        /// .NET version of C u_getUnicodeProperties().
        /// </summary>
        /// <param name="codepoint">Codepoint whose additional properties is to be retrieved.</param>
        /// <param name="column">The column index.</param>
        /// <returns>Unicode properties.</returns>
        public int GetAdditional(int codepoint, int column)
        {
            Debug.Assert(column >= 0);
            if (column >= m_additionalColumnsCount_)
            {
                return 0;
            }
            return m_additionalVectors_[m_additionalTrie_.Get(codepoint) + column];
        }

        internal static readonly int MY_MASK = UCharacterProperty.TYPE_MASK
            & ((1 << UUnicodeCategory.UppercaseLetter.ToInt32()) |
                (1 << UUnicodeCategory.LowercaseLetter.ToInt32()) |
                (1 << UUnicodeCategory.TitlecaseLetter.ToInt32()) |
                (1 << UUnicodeCategory.ModifierLetter.ToInt32()) |
                (1 << UUnicodeCategory.OtherLetter.ToInt32()));

        /// <summary>
        /// Get the "age" of the code point.
        /// </summary>
        /// <remarks>
        /// The "age" is the Unicode version when the code point was first
        /// designated (as a non-character or for Private Use) or assigned a
        /// character.
        /// <para/>
        /// This can be useful to avoid emitting code points to receiving
        /// processes that do not accept newer characters.
        /// <para/>
        /// The data is from the UCD file DerivedAge.txt.
        /// <para/>
        /// This API does not check the validity of the codepoint.
        /// </remarks>
        /// <param name="codepoint">The code point.</param>
        /// <returns>The Unicode version number.</returns>
        public VersionInfo GetAge(int codepoint)
        {
            int version = GetAdditional(codepoint, 0) >> AGE_SHIFT_;
            return VersionInfo.GetInstance(
                               (version >> FIRST_NIBBLE_SHIFT_) & LAST_NIBBLE_MASK_,
                               version & LAST_NIBBLE_MASK_, 0, 0);
        }

        private static readonly int GC_CN_MASK = GetMask(UUnicodeCategory.OtherNotAssigned.ToInt32());
        private static readonly int GC_CC_MASK = GetMask(UUnicodeCategory.Control.ToInt32());
        private static readonly int GC_CS_MASK = GetMask(UUnicodeCategory.Surrogate.ToInt32());
        private static readonly int GC_ZS_MASK = GetMask(UUnicodeCategory.SpaceSeparator.ToInt32());
        private static readonly int GC_ZL_MASK = GetMask(UUnicodeCategory.LineSeparator.ToInt32());
        private static readonly int GC_ZP_MASK = GetMask(UUnicodeCategory.ParagraphSeparator.ToInt32());
        /// <summary>Mask constant for multiple UCharCategory bits (Z Separators).</summary>
        private static readonly int GC_Z_MASK = GC_ZS_MASK | GC_ZL_MASK | GC_ZP_MASK;

        /// <summary>
        /// Checks if <paramref name="c"/> is in
        /// [^\p{space}\p{gc=Control}\p{gc=Surrogate}\p{gc=Unassigned}]
        /// with space=\p{Whitespace} and Control=Cc.
        /// Implements UCHAR_POSIX_GRAPH.
        /// </summary>
        /// <internal/>
        private static bool IsgraphPOSIX(int c)
        {
            /* \p{space}\p{gc=Control} == \p{gc=Z}\p{Control} */
            /* comparing ==0 returns FALSE for the categories mentioned */
            return (GetMask(UChar.GetUnicodeCategory(c).ToInt32()) &
                    (GC_CC_MASK | GC_CS_MASK | GC_CN_MASK | GC_Z_MASK))
                   == 0;
        }

        // binary properties --------------------------------------------------- ***

        private class BinaryProperty
        {
            private readonly UCharacterProperty outerInstance;
            int column;  // SRC_PROPSVEC column, or "source" if mask==0
            int mask;
            internal BinaryProperty(UCharacterProperty outerInstance, int column, int mask)
            {
                this.outerInstance = outerInstance;
                this.column = column;
                this.mask = mask;
            }
            internal BinaryProperty(UCharacterProperty outerInstance, int source)
            {
                this.outerInstance = outerInstance;
                this.column = source;
                this.mask = 0;
            }
            internal int GetSource()
            {
                return mask == 0 ? column : SRC_PROPSVEC;
            }
            internal virtual bool Contains(int c)
            {
                // systematic, directly stored properties
                return (outerInstance.GetAdditional(c, column) & mask) != 0;
            }
        }

        private class CaseBinaryProperty : BinaryProperty
        {  // case mapping properties
            int which;
            internal CaseBinaryProperty(UCharacterProperty outerInstance, int which)
                    : base(outerInstance, SRC_CASE)
            {
                this.which = which;
            }
            internal override bool Contains(int c)
            {
                return UCaseProps.Instance.HasBinaryProperty(c, (UProperty)which);
            }
        }

        private class NormInertBinaryProperty : BinaryProperty
        {  // UCHAR_NF*_INERT properties
            int which;
            internal NormInertBinaryProperty(UCharacterProperty outerInstance, int source, int which)
                    : base(outerInstance, source)
            {
                this.which = which;
            }
            internal override bool Contains(int c)
            {
                return Norm2AllModes.GetN2WithImpl(which - (int)UProperty.NFD_Inert).IsInert(c);
            }
        }

        // ICU4N specific class for building BinaryProperties on the fly
        private class AnonymousBinaryProperty : BinaryProperty
        {
            private readonly Func<int, bool> contains;

            internal AnonymousBinaryProperty(UCharacterProperty outerInstance, int source, Func<int, bool> contains)
                : base(outerInstance, source)
            {
                this.contains = contains;
            }

            internal override bool Contains(int c)
            {
                return contains(c);
            }
        }

        private void Init()
        {
            binProps = new BinaryProperty[] {
                /*
                 * Binary-property implementations must be in order of corresponding UProperty,
                 * and there must be exactly one entry per binary UProperty.
                 */
                new BinaryProperty(this, 1, (1 << ALPHABETIC_PROPERTY_)),
                new BinaryProperty(this, 1, (1 << ASCII_HEX_DIGIT_PROPERTY_)),
                new AnonymousBinaryProperty(this, SRC_BIDI, contains: (c) =>
                    {
                        return UBiDiProps.Instance.IsBidiControl(c);
                    }),
                new AnonymousBinaryProperty(this, SRC_BIDI, contains: (c) =>
                    {
                        return UBiDiProps.Instance.IsMirrored(c);
                    }),
                new BinaryProperty(this, 1, (1<<DASH_PROPERTY_)),
                new BinaryProperty(this, 1, (1<<DEFAULT_IGNORABLE_CODE_POINT_PROPERTY_)),
                new BinaryProperty(this, 1, (1<<DEPRECATED_PROPERTY_)),
                new BinaryProperty(this, 1, (1<<DIACRITIC_PROPERTY_)),
                new BinaryProperty(this, 1, (1<<EXTENDER_PROPERTY_)),
                new AnonymousBinaryProperty(this, SRC_NFC, contains: (c) =>
                    {// UCHAR_FULL_COMPOSITION_EXCLUSION
                        // By definition, Full_Composition_Exclusion is the same as NFC_QC=No.
                        Normalizer2Impl impl = Norm2AllModes.GetNFCInstance().Impl;
                        return impl.IsCompNo(impl.GetNorm16(c));
                    }),
                new BinaryProperty(this,1, (1<<GRAPHEME_BASE_PROPERTY_)),
                new BinaryProperty(this,1, (1<<GRAPHEME_EXTEND_PROPERTY_)),
                new BinaryProperty(this,1, (1<<GRAPHEME_LINK_PROPERTY_)),
                new BinaryProperty(this,1, (1<<HEX_DIGIT_PROPERTY_)),
                new BinaryProperty(this,1, (1<<HYPHEN_PROPERTY_)),
                new BinaryProperty(this,1, (1<<ID_CONTINUE_PROPERTY_)),
                new BinaryProperty(this,1, (1<<ID_START_PROPERTY_)),
                new BinaryProperty(this,1, (1<<IDEOGRAPHIC_PROPERTY_)),
                new BinaryProperty(this,1, (1<<IDS_BINARY_OPERATOR_PROPERTY_)),
                new BinaryProperty(this,1, (1<<IDS_TRINARY_OPERATOR_PROPERTY_)),
                new AnonymousBinaryProperty(this, SRC_BIDI, contains: (c) =>
                    { // UCHAR_JOIN_CONTROL
                        return UBiDiProps.Instance.IsJoinControl(c);
                    }),
                new BinaryProperty(this,1, (1<<LOGICAL_ORDER_EXCEPTION_PROPERTY_)),
                new CaseBinaryProperty(this, (int)UProperty.Lowercase),
                new BinaryProperty(this,1, (1<<MATH_PROPERTY_)),
                new BinaryProperty(this,1, (1<<NONCHARACTER_CODE_POINT_PROPERTY_)),
                new BinaryProperty(this,1, (1<<QUOTATION_MARK_PROPERTY_)),
                new BinaryProperty(this,1, (1<<RADICAL_PROPERTY_)),
                new CaseBinaryProperty(this, (int)UProperty.Soft_Dotted),
                new BinaryProperty(this,1, (1<<TERMINAL_PUNCTUATION_PROPERTY_)),
                new BinaryProperty(this,1, (1<<UNIFIED_IDEOGRAPH_PROPERTY_)),
                new CaseBinaryProperty(this, (int)UProperty.Uppercase),
                new BinaryProperty(this,1, (1<<WHITE_SPACE_PROPERTY_)),
                new BinaryProperty(this,1, (1<<XID_CONTINUE_PROPERTY_)),
                new BinaryProperty(this,1, (1<<XID_START_PROPERTY_)),
                new CaseBinaryProperty(this, (int)UProperty.Case_Sensitive),
                new BinaryProperty(this,1, (1<<S_TERM_PROPERTY_)),
                new BinaryProperty(this,1, (1<<VARIATION_SELECTOR_PROPERTY_)),
                new NormInertBinaryProperty(this,SRC_NFC, (int)UProperty.NFD_Inert),
                new NormInertBinaryProperty(this,SRC_NFKC, (int)UProperty.NFKD_Inert),
                new NormInertBinaryProperty(this,SRC_NFC, (int)UProperty.NFC_Inert),
                new NormInertBinaryProperty(this,SRC_NFKC, (int)UProperty.NFKC_Inert),
                new AnonymousBinaryProperty(this, SRC_NFC_CANON_ITER, contains: (c) =>
                    {  // UCHAR_SEGMENT_STARTER
                        return Norm2AllModes.GetNFCInstance().Impl.
                            EnsureCanonIterData().IsCanonSegmentStarter(c);
                    }),
                new BinaryProperty(this, 1, (1<<PATTERN_SYNTAX)),
                new BinaryProperty(this, 1, (1<<PATTERN_WHITE_SPACE)),
                new AnonymousBinaryProperty(this, SRC_CHAR_AND_PROPSVEC, contains: (c) =>
                    {  // UCHAR_POSIX_ALNUM
                        return UChar.IsUAlphabetic(c) || UChar.IsDigit(c);
                    }),
                new AnonymousBinaryProperty(this, SRC_CHAR, contains: (c) =>
                    {  // UCHAR_POSIX_BLANK
                        // "horizontal space"
                        if (c <= 0x9f)
                        {
                            return c == 9 || c == 0x20; /* TAB or SPACE */
                        }
                        else
                        {
                            /* Zs */
                            return UChar.GetUnicodeCategory(c) == UUnicodeCategory.SpaceSeparator;
                        }
                    }),
                new AnonymousBinaryProperty(this, SRC_CHAR, contains: (c) =>
                    {  // UCHAR_POSIX_GRAPH
                        return IsgraphPOSIX(c);
                    }),
                new AnonymousBinaryProperty(this, SRC_CHAR, contains: (c) =>
                    {  // UCHAR_POSIX_PRINT
                        /*
                        * Checks if codepoint is in \p{graph}\p{blank} - \p{cntrl}.
                        *
                        * The only cntrl character in graph+blank is TAB (in blank).
                        * Here we implement (blank-TAB)=Zs instead of calling u_isblank().
                        */
                        return (UChar.GetUnicodeCategory(c) == UUnicodeCategory.SpaceSeparator) || IsgraphPOSIX(c);
                    }),
                new AnonymousBinaryProperty(this, SRC_CHAR, contains: (c) =>
                    {  // UCHAR_POSIX_XDIGIT
                        /* check ASCII and Fullwidth ASCII a-fA-F */
                        if (
                            (c <= 0x66 && c >= 0x41 && (c <= 0x46 || c >= 0x61)) ||
                            (c >= 0xff21 && c <= 0xff46 && (c <= 0xff26 || c >= 0xff41))
                        )
                        {
                            return true;
                        }
                        return UChar.GetUnicodeCategory(c) == UUnicodeCategory.DecimalDigitNumber;
                    }),
                new CaseBinaryProperty(this, (int)UProperty.Cased),
                new CaseBinaryProperty(this, (int)UProperty.Case_Ignorable),
                new CaseBinaryProperty(this, (int)UProperty.Changes_When_Lowercased),
                new CaseBinaryProperty(this, (int)UProperty.Changes_When_Uppercased),
                new CaseBinaryProperty(this, (int)UProperty.Changes_When_Titlecased),
                new AnonymousBinaryProperty(this, SRC_CASE_AND_NORM, contains: (c) =>
                    {  // UCHAR_CHANGES_WHEN_CASEFOLDED
                        string nfd = Norm2AllModes.GetNFCInstance().Impl.GetDecomposition(c);
                        if (nfd != null)
                        {
                            /* c has a decomposition */
                            c = nfd.CodePointAt(0);
                            if (Character.CharCount(c) != nfd.Length)
                            {
                                /* multiple code points */
                                c = -1;
                            }
                        }
                        else if (c < 0)
                        {
                            return false;  /* protect against bad input */
                        }
                        if (c >= 0)
                        {
                            /* single code point */
                            UCaseProps csp = UCaseProps.Instance;
                            UCaseProps.DummyStringBuilder.Length = 0;
                            return csp.ToFullFolding(c, UCaseProps.DummyStringBuilder,
                                                        UChar.FoldCaseDefault) >= 0;
                        }
                        else
                        {
                            string folded = UChar.FoldCase(nfd, true);
                            return !folded.Equals(nfd);
                        }
                    }),
                new CaseBinaryProperty(this, (int)UProperty.Changes_When_Casemapped),
                new AnonymousBinaryProperty(this, SRC_NFKC_CF, contains: (c) =>
                    {  // UCHAR_CHANGES_WHEN_NFKC_CASEFOLDED
                        Normalizer2Impl kcf = Norm2AllModes.GetNFKC_CFInstance().Impl;
                        string src = UTF16.ValueOf(c);
                        StringBuilder dest = new StringBuilder();
                        // Small destCapacity for NFKC_CF(c).
                        ReorderingBuffer buffer = new ReorderingBuffer(kcf, dest, 5);
                        kcf.Compose(src, 0, src.Length, false, true, buffer);
                        return !UTF16Plus.Equal(dest, src);
                    }),
                new BinaryProperty(this, 2, 1<<PROPS_2_EMOJI),
                new BinaryProperty(this, 2, 1<<PROPS_2_EMOJI_PRESENTATION),
                new BinaryProperty(this, 2, 1<<PROPS_2_EMOJI_MODIFIER),
                new BinaryProperty(this, 2, 1<<PROPS_2_EMOJI_MODIFIER_BASE),
                new BinaryProperty(this, 2, 1<<PROPS_2_EMOJI_COMPONENT),
                new AnonymousBinaryProperty(this, SRC_PROPSVEC, contains: (c) =>
                    {  // REGIONAL_INDICATOR
                        // Property starts are a subset of lb=RI etc.
                        return 0x1F1E6 <= c && c <= 0x1F1FF;
                    }),
                new BinaryProperty(this, 1, 1<<PREPENDED_CONCATENATION_MARK),
            };

            intProps = new IntProperty[]
            {
                new BiDiIntProperty(this, getValue: (c) =>
                    {
                        return UBiDiProps.Instance.GetClass(c).ToInt32();
                    }),
                new IntProperty(this, 0, BLOCK_MASK_, BLOCK_SHIFT_),
                new CombiningClassIntProperty(this, SRC_NFC, getValue: (c) =>
                    { // CANONICAL_COMBINING_CLASS
                        return Normalizer2.GetNFDInstance().GetCombiningClass(c);
                    }),
                new IntProperty(this, 2, DECOMPOSITION_TYPE_MASK_, 0),
                new IntProperty(this, 0, EAST_ASIAN_MASK_, EAST_ASIAN_SHIFT_),
                new AnonymousIntProperty(this, SRC_CHAR, getValue: (c) =>
                    {  // GENERAL_CATEGORY
                        return GetType(c);
                    }, getMaxValue: (which) =>
                    {
                        return UCharacterCategoryExtensions.CharCategoryCount - 1;
                    }),
                new BiDiIntProperty(this, getValue: (c) =>
                    {  // JOINING_GROUP
                        return UBiDiProps.Instance.GetJoiningGroup(c);
                    }),
                new BiDiIntProperty(this, getValue: (c) =>
                    {  // JOINING_TYPE
                        return UBiDiProps.Instance.GetJoiningType(c);
                    }),
                new IntProperty(this, 2, LB_MASK, LB_SHIFT),  // LINE_BREAK
                new AnonymousIntProperty(this, SRC_CHAR, getValue: (c) =>
                    {  // NUMERIC_TYPE
                        return NtvGetType(GetNumericTypeValue(GetProperty(c)));
                    }, getMaxValue: (which) =>
                    {
#pragma warning disable 612, 618
                        return NumericType.Count - 1;
#pragma warning restore 612, 618
                    }),
                new AnonymousIntProperty(this, 0, SCRIPT_MASK_, 0, getValue: (c) =>
                    {
                        return (int)UScript.GetScript(c);
                    }, getMaxValue: null),
                new AnonymousIntProperty(this, SRC_PROPSVEC, getValue: (c) =>
                    {  // HANGUL_SYLLABLE_TYPE
                        /* see comments on gcbToHst[] above */
                        int gcb = (GetAdditional(c, 2) & GCB_MASK).TripleShift(GCB_SHIFT);
                        if (gcb < gcbToHst.Length)
                        {
                            return gcbToHst[gcb];
                        }
                        else
                        {
                            return HangulSyllableType.NotApplicable;
                        }
                    }, getMaxValue: (which) =>
                    {
#pragma warning disable 612, 618
                        return HangulSyllableType.COUNT - 1;
#pragma warning restore 612, 618
                    }),
                // max=1=YES -- these are never "maybe", only "no" or "yes"
                new NormQuickCheckIntProperty(this, SRC_NFC, (int)UProperty.NFD_Quick_Check, 1),
                new NormQuickCheckIntProperty(this, SRC_NFKC, (int)UProperty.NFKD_Quick_Check, 1),
                // max=2=MAYBE
                new NormQuickCheckIntProperty(this, SRC_NFC, (int)UProperty.NFC_Quick_Check, 2),
                new NormQuickCheckIntProperty(this, SRC_NFKC, (int)UProperty.NFKC_Quick_Check, 2),
                new CombiningClassIntProperty(this, SRC_NFC, getValue: (c) =>
                    {  // LEAD_CANONICAL_COMBINING_CLASS
                        return Norm2AllModes.GetNFCInstance().Impl.GetFCD16(c) >> 8;
                    }),
                new CombiningClassIntProperty(this, SRC_NFC, getValue: (c) =>
                    {  // TRAIL_CANONICAL_COMBINING_CLASS
                        return Norm2AllModes.GetNFCInstance().Impl.GetFCD16(c) & 0xff;
                    }),
                new IntProperty(this, 2, GCB_MASK, GCB_SHIFT),  // GRAPHEME_CLUSTER_BREAK
                new IntProperty(this, 2, SB_MASK, SB_SHIFT),  // SENTENCE_BREAK
                new IntProperty(this, 2, WB_MASK, WB_SHIFT),  // WORD_BREAK
                new BiDiIntProperty(this, getValue: (c) =>
                    {  // BIDI_PAIRED_BRACKET_TYPE
                        return UBiDiProps.Instance.GetPairedBracketType(c);
                    }),
            };
        }


        private BinaryProperty[] binProps;


        public bool HasBinaryProperty(int c, int which)
        {
            if (which < (int)UProperty.Binary_Start
#pragma warning disable 612, 618
                || (int)UProperty.Binary_Limit <= which)
#pragma warning restore 612, 618
            {
                // not a known binary property
                return false;
            }
            else
            {
                return binProps[which].Contains(c);
            }
        }

        // int-value and enumerated properties --------------------------------- ***

        public int GetType(int c) // ICU4N TODO: API - Return UCharacterCategory type
        {
            return GetProperty(c) & TYPE_MASK;
        }

        /// <summary>
        /// Map some of the Grapheme Cluster Break values to Hangul Syllable Types.
        /// Hangul_Syllable_Type is fully redundant with a subset of Grapheme_Cluster_Break.
        /// </summary>
        private static readonly int[] /* UHangulSyllableType */ gcbToHst ={
            HangulSyllableType.NotApplicable,   /* U_GCB_OTHER */
            HangulSyllableType.NotApplicable,   /* U_GCB_CONTROL */
            HangulSyllableType.NotApplicable,   /* U_GCB_CR */
            HangulSyllableType.NotApplicable,   /* U_GCB_EXTEND */
            HangulSyllableType.LeadingJamo,     /* U_GCB_L */
            HangulSyllableType.NotApplicable,   /* U_GCB_LF */
            HangulSyllableType.LvSyllable,      /* U_GCB_LV */
            HangulSyllableType.LvtSyllable,     /* U_GCB_LVT */
            HangulSyllableType.TrailingJamo,    /* U_GCB_T */
            HangulSyllableType.VowelJamo        /* U_GCB_V */
            /*
             * Omit GCB values beyond what we need for hst.
             * The code below checks for the array length.
             */
        };

        private class IntProperty
        {
            internal readonly UCharacterProperty outerInstance;
            int column;  // SRC_PROPSVEC column, or "source" if mask==0
            int mask;
            int shift;
            internal IntProperty(UCharacterProperty outerInstance, int column, int mask, int shift)
            {
                this.outerInstance = outerInstance;
                this.column = column;
                this.mask = mask;
                this.shift = shift;
            }
            internal IntProperty(UCharacterProperty outerInstance, int source)
            {
                this.outerInstance = outerInstance;
                this.column = source;
                this.mask = 0;
            }
            internal int GetSource()
            {
                return mask == 0 ? column : SRC_PROPSVEC;
            }
            internal virtual int GetValue(int c)
            {
                // systematic, directly stored properties
                return (outerInstance.GetAdditional(c, column) & mask).TripleShift(shift);
            }
            internal virtual int GetMaxValue(int which)
            {
                return (outerInstance.GetMaxValues(column) & mask).TripleShift(shift);
            }
        }

        private class AnonymousIntProperty : IntProperty
        {
            private readonly Func<int, int> getValue;
            private readonly Func<int, int> getMaxValue;

            internal AnonymousIntProperty(UCharacterProperty outerInstance, int source, Func<int, int> getValue, Func<int, int> getMaxValue)
            : base(outerInstance, source)
            {
                this.getValue = getValue;
                this.getMaxValue = getMaxValue;
            }

            internal AnonymousIntProperty(UCharacterProperty outerInstance, int column, int mask, int shift, Func<int, int> getValue, Func<int, int> getMaxValue)
                : base(outerInstance, column, mask, shift)
            {
                this.getValue = getValue;
                this.getMaxValue = getMaxValue;
            }

            internal override int GetValue(int c)
            {
                return getValue == null ? base.GetValue(c) : getValue(c);
            }

            internal override int GetMaxValue(int which)
            {
                return getMaxValue == null ? base.GetMaxValue(which) : getMaxValue(which);
            }
        }


        private class BiDiIntProperty : IntProperty
        {
            private readonly Func<int, int> getValue;

            internal BiDiIntProperty(UCharacterProperty outerInstance, Func<int, int> getValue)
                        : base(outerInstance, SRC_BIDI)
            {
                this.getValue = getValue;
            }

            internal override int GetValue(int c)
            {
                return getValue == null ? base.GetValue(c) : getValue(c);
            }

            internal override int GetMaxValue(int which)
            {
                return UBiDiProps.Instance.GetMaxValue(which);
            }
        }

        private class CombiningClassIntProperty : IntProperty
        {
            private readonly Func<int, int> getValue;

            internal CombiningClassIntProperty(UCharacterProperty outerInstance, int source, Func<int, int> getValue)
                        : base(outerInstance, source)
            {
                this.getValue = getValue;
            }

            internal override int GetValue(int c)
            {
                return getValue == null ? base.GetValue(c) : getValue(c);
            }

            internal override int GetMaxValue(int which)
            {
                return 0xff;
            }
        }

        private class NormQuickCheckIntProperty : IntProperty
        {  // UCHAR_NF*_QUICK_CHECK properties
            int which;
            int max;
            internal NormQuickCheckIntProperty(UCharacterProperty outerInstance, int source, int which, int max)
                : base(outerInstance, source)
            {
                this.which = which;
                this.max = max;
            }

            internal override int GetValue(int c)
            {
                return Norm2AllModes.GetN2WithImpl(which - (int)UProperty.NFD_Quick_Check).GetQuickCheck(c);
            }

            internal override int GetMaxValue(int which)
            {
                return max;
            }
        }

        private IntProperty[] intProps;

        // ICU4N TODO: API - change which to UProperty
        public int GetIntPropertyValue(int c, int which) // ICU4N TODO: API - rename back to GetIntPropertyValue (we don't have to discern between different data types)
        {
            if (which < (int)UProperty.Int_Start)
            {
                if ((int)UProperty.Binary_Start <= which
#pragma warning disable 612, 618
                    && which < (int)UProperty.Binary_Limit)
#pragma warning restore 612, 618
                {
                    return binProps[which].Contains(c) ? 1 : 0;
                }
            }
#pragma warning disable 612, 618
            else if (which < (int)UProperty.Int_Limit)
#pragma warning restore 612, 618
            {
                return intProps[which - (int)UProperty.Int_Start].GetValue(c);
            }
            else if (which == (int)UProperty.General_Category_Mask)
            {
                return GetMask(GetType(c));
            }
            return 0; // undefined
        }

        // ICU4N TODO: API - change which to UProperty
        public int GetIntPropertyMaxValue(int which)
        {
            if (which < (int)UProperty.Int_Start)
            {
                if ((int)UProperty.Binary_Start <= which
#pragma warning disable 612, 618
                    && which < (int)UProperty.Binary_Limit)
#pragma warning restore 612, 618
                {
                    return 1;  // maximum TRUE for all binary properties
                }
            }
#pragma warning disable 612, 618
            else if (which < (int)UProperty.Int_Limit)
#pragma warning restore 612, 618
            {
                return intProps[which - (int)UProperty.Int_Start].GetMaxValue(which);
            }
            return -1; // undefined
        }

        public int GetSource(UProperty which)
        {
            if (which < UProperty.Binary_Start)
            {
                return SRC_NONE; /* undefined */
            }
#pragma warning disable 612, 618
            else if (which < UProperty.Binary_Limit)
#pragma warning restore 612, 618
            {
                return binProps[(int)which].GetSource();
            }
            else if (which < UProperty.Int_Start)
            {
                return SRC_NONE; /* undefined */
            }
#pragma warning disable 612, 618
            else if (which < UProperty.Int_Limit)
#pragma warning restore 612, 618
            {
                return intProps[which - UProperty.Int_Start].GetSource();
            }
            else if (which < UProperty.String_Start)
            {
                switch (which)
                {
                    case UProperty.General_Category_Mask:
                    case UProperty.Numeric_Value:
                        return SRC_CHAR;

                    default:
                        return SRC_NONE;
                }
            }
#pragma warning disable 612, 618
            else if (which < UProperty.String_Limit)
#pragma warning restore 612, 618
            {
                switch (which)
                {
                    case UProperty.Age:
                        return SRC_PROPSVEC;

                    case UProperty.Bidi_Mirroring_Glyph:
                        return SRC_BIDI;

                    case UProperty.Case_Folding:
                    case UProperty.Lowercase_Mapping:
                    case UProperty.Simple_Case_Folding:
                    case UProperty.Simple_Lowercase_Mapping:
                    case UProperty.Simple_Titlecase_Mapping:
                    case UProperty.Simple_Uppercase_Mapping:
                    case UProperty.Titlecase_Mapping:
                    case UProperty.Uppercase_Mapping:
                        return SRC_CASE;

#pragma warning disable 612, 618
                    case UProperty.ISO_Comment:
                    case UProperty.Name:
                    case UProperty.Unicode_1_Name:
#pragma warning restore 612, 618
                        return SRC_NAMES;

                    default:
                        return SRC_NONE;
                }
            }
            else
            {
                switch (which)
                {
                    case UProperty.Script_Extensions:
                        return SRC_PROPSVEC;
                    default:
                        return SRC_NONE; /* undefined */
                }
            }
        }

        ///// <summary>
        ///// Unicode property names and property value names are compared
        ///// "loosely". Property[Value]Aliases.txt say:
        ///// <quote>
        /////   "With loose matching of property names, the case distinctions,
        /////   whitespace, and '_' are ignored."
        ///// </quote>
        ///// <para/>
        ///// This function does just that, for ASCII (char *) name strings.
        ///// It is almost identical to ucnv_compareNames() but also ignores
        ///// ASCII White_Space characters (U+0009..U+000d).
        ///// </summary>
        ///// <param name="name1">Name to compare.</param>
        ///// <param name="name2">Name to compare.</param>
        ///// <returns>0 if names are equal, &lt; 0 if name1 is less than name2 and &gt; 0
        ///// if name1 is greater than name2.</returns>
        //// to be implemented in 2.4
        //public static int ComparePropertyNames(string name1, string name2)
        //{
        //    int result = 0;
        //    int i1 = 0;
        //    int i2 = 0;
        //    while (true) {
        //        char ch1 = (char)0;
        //        char ch2 = (char)0;
        //        // Ignore delimiters '-', '_', and ASCII White_Space
        //        if (i1 < name1.Length) {
        //            ch1 = name1[i1++];
        //        }
        //        while (ch1 == '-' || ch1 == '_' || ch1 == ' ' || ch1 == '\t'
        //               || ch1 == '\n' // synwee what is || ch1 == '\v'
        //               || ch1 == '\f' || ch1=='\r') {
        //            if (i1 < name1.Length) {
        //                ch1 = name1[i1++];
        //            }
        //            else {
        //                ch1 = (char)0;
        //            }
        //        }
        //        if (i2 < name2.Length) {
        //            ch2 = name2[i2++];
        //        }
        //        while (ch2 == '-' || ch2 == '_' || ch2 == ' ' || ch2 == '\t'
        //               || ch2 == '\n' // synwee what is || ch1 == '\v'
        //               || ch2 == '\f' || ch2=='\r') {
        //            if (i2 < name2.Length) {
        //                ch2 = name2[i2++];
        //            }
        //            else {
        //                ch2 = (char)0;
        //            }
        //        }

        //        // If we reach the ends of both strings then they match
        //        if (ch1 == 0 && ch2 == 0) {
        //            return 0;
        //        }

        //        // Case-insensitive comparison
        //        if (ch1 != ch2) {
        //            result = Character.ToLower(ch1)
        //                                            - Character.ToLower(ch2);
        //            if (result != 0) {
        //                return result;
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Get the the maximum values for some enum/int properties.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Maximum values for the integer properties.</returns>
        public int GetMaxValues(int column)
        {
            // return m_maxBlockScriptValue_;

            switch (column)
            {
                case 0:
                    return m_maxBlockScriptValue_;
                case 2:
                    return m_maxJTGValue_;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the type mask.
        /// </summary>
        /// <param name="type">Character type.</param>
        /// <returns>Mask.</returns>
        public static int GetMask(int type)
        {
            return 1 << type;
        }

        /// <summary>
        /// Returns the digit values of characters like 'A' - 'Z', normal,
        /// half-width and full-width. This method assumes that the other digit
        /// characters are checked by the calling method.
        /// </summary>
        /// <param name="ch">Character to test.</param>
        /// <returns>-1 if ch is not a character of the form 'A' - 'Z', otherwise
        /// its corresponding digit will be returned.</returns>
        public static int GetEuropeanDigit(int ch)
        {
            if ((ch > 0x7a && ch < 0xff21)
                || ch < 0x41 || (ch > 0x5a && ch < 0x61)
                || ch > 0xff5a || (ch > 0xff3a && ch < 0xff41))
            {
                return -1;
            }
            if (ch <= 0x7a)
            {
                // ch >= 0x41 or ch < 0x61
                return ch + 10 - ((ch <= 0x5a) ? 0x41 : 0x61);
            }
            // ch >= 0xff21
            if (ch <= 0xff3a)
            {
                return ch + 10 - 0xff21;
            }
            // ch >= 0xff41 && ch <= 0xff5a
            return ch + 10 - 0xff41;
        }

        public int Digit(int c)
        {
            int value = GetNumericTypeValue(GetProperty(c)) - NTV_DECIMAL_START_;
            if (value <= 9)
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        public int GetNumericValue(int c)
        {
            // slightly pruned version of getUnicodeNumericValue(), plus getEuropeanDigit()
            int ntv = GetNumericTypeValue(GetProperty(c));

            if (ntv == NTV_NONE_)
            {
                return GetEuropeanDigit(c);
            }
            else if (ntv < NTV_DIGIT_START_)
            {
                /* decimal digit */
                return ntv - NTV_DECIMAL_START_;
            }
            else if (ntv < NTV_NUMERIC_START_)
            {
                /* other digit */
                return ntv - NTV_DIGIT_START_;
            }
            else if (ntv < NTV_FRACTION_START_)
            {
                /* small integer */
                return ntv - NTV_NUMERIC_START_;
            }
            else if (ntv < NTV_LARGE_START_)
            {
                /* fraction */
                return -2;
            }
            else if (ntv < NTV_BASE60_START_)
            {
                /* large, single-significant-digit integer */
                int mant = (ntv >> 5) - 14;
                int exp = (ntv & 0x1f) + 2;
                if (exp < 9 || (exp == 9 && mant <= 2))
                {
                    int numValue = mant;
                    do
                    {
                        numValue *= 10;
                    } while (--exp > 0);
                    return numValue;
                }
                else
                {
                    return -2;
                }
            }
            else if (ntv < NTV_FRACTION20_START_)
            {
                /* sexagesimal (base 60) integer */
                int numValue = (ntv >> 2) - 0xbf;
                int exp = (ntv & 3) + 1;

                switch (exp)
                {
                    case 4:
                        numValue *= 60 * 60 * 60 * 60;
                        break;
                    case 3:
                        numValue *= 60 * 60 * 60;
                        break;
                    case 2:
                        numValue *= 60 * 60;
                        break;
                    case 1:
                        numValue *= 60;
                        break;
                    case 0:
                    default:
                        break;
                }

                return numValue;
            }
            else if (ntv < NTV_RESERVED_START_)
            {
                // fraction-20 e.g. 3/80
                return -2;
            }
            else
            {
                /* reserved */
                return -2;
            }
        }

        public double GetUnicodeNumericValue(int c)
        {
            // equivalent to c version double u_getNumericValue(UChar32 c)
            int ntv = GetNumericTypeValue(GetProperty(c));

            if (ntv == NTV_NONE_)
            {
                return UChar.NoNumericValue;
            }
            else if (ntv < NTV_DIGIT_START_)
            {
                /* decimal digit */
                return ntv - NTV_DECIMAL_START_;
            }
            else if (ntv < NTV_NUMERIC_START_)
            {
                /* other digit */
                return ntv - NTV_DIGIT_START_;
            }
            else if (ntv < NTV_FRACTION_START_)
            {
                /* small integer */
                return ntv - NTV_NUMERIC_START_;
            }
            else if (ntv < NTV_LARGE_START_)
            {
                /* fraction */
                int numerator = (ntv >> 4) - 12;
                int denominator = (ntv & 0xf) + 1;
                return (double)numerator / denominator;
            }
            else if (ntv < NTV_BASE60_START_)
            {
                /* large, single-significant-digit integer */
                double numValue;
                int mant = (ntv >> 5) - 14;
                int exp = (ntv & 0x1f) + 2;
                numValue = mant;

                /* multiply by 10^exp without math.h */
                while (exp >= 4)
                {
                    numValue *= 10000;
                    exp -= 4;
                }
                switch (exp)
                {
                    case 3:
                        numValue *= 1000;
                        break;
                    case 2:
                        numValue *= 100;
                        break;
                    case 1:
                        numValue *= 10;
                        break;
                    case 0:
                    default:
                        break;
                }

                return numValue;
            }
            else if (ntv < NTV_FRACTION20_START_)
            {
                /* sexagesimal (base 60) integer */
                int numValue = (ntv >> 2) - 0xbf;
                int exp = (ntv & 3) + 1;

                switch (exp)
                {
                    case 4:
                        numValue *= 60 * 60 * 60 * 60;
                        break;
                    case 3:
                        numValue *= 60 * 60 * 60;
                        break;
                    case 2:
                        numValue *= 60 * 60;
                        break;
                    case 1:
                        numValue *= 60;
                        break;
                    case 0:
                    default:
                        break;
                }

                return numValue;
            }
            else if (ntv < NTV_RESERVED_START_)
            {
                // fraction-20 e.g. 3/80
                int frac20 = ntv - NTV_FRACTION20_START_;  // 0..0x17
                int numerator = 2 * (frac20 & 3) + 1;
                int denominator = 20 << (frac20 >> 2);
                return (double)numerator / denominator;
            }
            else
            {
                /* reserved */
                return UChar.NoNumericValue;
            }
        }

        // protected variables -----------------------------------------------

        /// <summary>
        /// Extra property trie
        /// </summary>
        private Trie2_16 m_additionalTrie_;
        /// <summary>
        /// Extra property vectors, 1st column for age and second for binary
        /// properties.
        /// </summary>
        private int[] m_additionalVectors_;
        /// <summary>
        /// Number of additional columns
        /// </summary>
        private int m_additionalColumnsCount_;
        /// <summary>
        /// Maximum values for block, bits used as in vector word
        /// 0
        /// </summary>
        private int m_maxBlockScriptValue_;
        /// <summary>
        /// Maximum values for script, bits used as in vector word
        /// 0
        /// </summary>
        private int m_maxJTGValue_;

        /// <summary>
        /// Script_Extensions data
        /// </summary>
        public char[] m_scriptExtensions_; // ICU4N TODO: API - make property

        // private variables -------------------------------------------------

        /// <summary>
        /// Default name of the datafile
        /// </summary>
        private static readonly string DATA_FILE_NAME_ = "uprops.icu";

        // property data constants -------------------------------------------------

        /// <summary>
        /// Numeric types and values in the main properties words.
        /// </summary>
        private static readonly int NUMERIC_TYPE_VALUE_SHIFT_ = 6;
        private static int GetNumericTypeValue(int props)
        {
            return props >> NUMERIC_TYPE_VALUE_SHIFT_;
        }
        /* constants for the storage form of numeric types and values */
        /// <summary> No numeric value.</summary>
        private static readonly int NTV_NONE_ = 0;
        /// <summary>Decimal digits: nv=0..9</summary>
        private static readonly int NTV_DECIMAL_START_ = 1;
        /// <summary>Other digits: nv=0..9</summary>
        private static readonly int NTV_DIGIT_START_ = 11;
        /// <summary>Small integers: nv=0..154</summary>
        private static readonly int NTV_NUMERIC_START_ = 21;
        /// <summary>Fractions: ((ntv>>4)-12) / ((ntv&amp;0xf)+1) = -1..17 / 1..16</summary>
        private static readonly int NTV_FRACTION_START_ = 0xb0;

        /// <summary>
        /// Large integers:
        /// <code>
        /// ((ntv>>5)-14) * 10^((ntv&amp;0x1f)+2) = (1..9)*(10^2..10^33)
        /// (only one significant decimal digit)
        /// </code>
        /// </summary>
        private static readonly int NTV_LARGE_START_ = 0x1e0;
        /// <summary>
        /// Sexagesimal numbers:
        /// <code>
        /// ((ntv>>2)-0xbf) * 60^((ntv&amp;3)+1) = (1..9)*(60^1..60^4)
        /// </code>
        /// </summary>
        private static readonly int NTV_BASE60_START_ = 0x300;
        /// <summary>
        /// Fraction-20 values:
        /// <code>
        /// frac20 = ntv-0x324 = 0..0x17 -> 1|3|5|7 / 20|40|80|160|320|640
        /// numerator: num = 2*(frac20&amp;3)+1
        /// denominator: den = 20&lt;&lt;(frac20>>2)
        /// </code>
        /// </summary>
        private static readonly int NTV_FRACTION20_START_ = NTV_BASE60_START_ + 36;  // 0x300+9*4=0x324
                                                                                     /** No numeric value (yet). */
        private static readonly int NTV_RESERVED_START_ = NTV_FRACTION20_START_ + 24;  // 0x324+6*4=0x34c

        private static int NtvGetType(int ntv)
        {
            return
                (ntv == NTV_NONE_) ? NumericType.None :
                (ntv < NTV_DIGIT_START_) ? NumericType.Decimal :
                (ntv < NTV_NUMERIC_START_) ? NumericType.Digit :
                NumericType.Numeric;
        }

        /*
         * Properties in vector word 0
         * Bits
         * 31..24   DerivedAge version major/minor one nibble each
         * 23..22   3..1: Bits 7..0 = Script_Extensions index
         *             3: Script value from Script_Extensions
         *             2: Script=Inherited
         *             1: Script=Common
         *             0: Script=bits 7..0
         * 21..20   reserved
         * 19..17   East Asian Width
         * 16.. 8   UBlockCode
         *  7.. 0   UScriptCode
         */

        /// <summary>
        /// Script_Extensions: mask includes Script
        /// </summary>
        public static readonly int SCRIPT_X_MASK = 0x00c000ff;
        //private static final int SCRIPT_X_SHIFT = 22;
        /// <summary>
        /// Integer properties mask and shift values for East Asian cell width.
        /// Equivalent to icu4c UPROPS_EA_MASK
        /// </summary>
        private static readonly int EAST_ASIAN_MASK_ = 0x000e0000;
        /// <summary>
        /// Integer properties mask and shift values for East Asian cell width.
        /// Equivalent to icu4c UPROPS_EA_SHIFT
        /// </summary>
        private static readonly int EAST_ASIAN_SHIFT_ = 17;
        /// <summary>
        /// Integer properties mask and shift values for blocks.
        /// Equivalent to icu4c UPROPS_BLOCK_MASK
        /// </summary>
        private static readonly int BLOCK_MASK_ = 0x0001ff00;
        /// <summary>
        /// Integer properties mask and shift values for blocks.
        /// Equivalent to icu4c UPROPS_BLOCK_SHIFT
        /// </summary>
        private static readonly int BLOCK_SHIFT_ = 8;
        /// <summary>
        /// Integer properties mask and shift values for scripts.
        /// Equivalent to icu4c UPROPS_SHIFT_MASK
        /// </summary>
        public static readonly int SCRIPT_MASK_ = 0x000000ff; // ICU4N TODO: API - rename according to .NET conventions

        /* SCRIPT_X_WITH_COMMON must be the lowest value that involves Script_Extensions. */
        public static readonly int SCRIPT_X_WITH_COMMON = 0x400000; // ICU4N TODO: API - rename according to .NET conventions
        public static readonly int SCRIPT_X_WITH_INHERITED = 0x800000; // ICU4N TODO: API - rename according to .NET conventions
        public static readonly int SCRIPT_X_WITH_OTHER = 0xc00000; // ICU4N TODO: API - rename according to .NET conventions

        /**
         * Additional properties used in internal trie data
         */
        /*
         * Properties in vector word 1
         * Each bit encodes one binary property.
         * The following constants represent the bit number, use 1<<UPROPS_XYZ.
         * UPROPS_BINARY_1_TOP<=32!
         *
         * Keep this list of property enums in sync with
         * propListNames[] in icu/source/tools/genprops/props2.c!
         *
         * ICU 2.6/uprops format version 3.2 stores full properties instead of "Other_".
         */
        private static readonly int WHITE_SPACE_PROPERTY_ = 0;
        private static readonly int DASH_PROPERTY_ = 1;
        private static readonly int HYPHEN_PROPERTY_ = 2;
        private static readonly int QUOTATION_MARK_PROPERTY_ = 3;
        private static readonly int TERMINAL_PUNCTUATION_PROPERTY_ = 4;
        private static readonly int MATH_PROPERTY_ = 5;
        private static readonly int HEX_DIGIT_PROPERTY_ = 6;
        private static readonly int ASCII_HEX_DIGIT_PROPERTY_ = 7;
        private static readonly int ALPHABETIC_PROPERTY_ = 8;
        private static readonly int IDEOGRAPHIC_PROPERTY_ = 9;
        private static readonly int DIACRITIC_PROPERTY_ = 10;
        private static readonly int EXTENDER_PROPERTY_ = 11;
        private static readonly int NONCHARACTER_CODE_POINT_PROPERTY_ = 12;
        private static readonly int GRAPHEME_EXTEND_PROPERTY_ = 13;
        private static readonly int GRAPHEME_LINK_PROPERTY_ = 14;
        private static readonly int IDS_BINARY_OPERATOR_PROPERTY_ = 15;
        private static readonly int IDS_TRINARY_OPERATOR_PROPERTY_ = 16;
        private static readonly int RADICAL_PROPERTY_ = 17;
        private static readonly int UNIFIED_IDEOGRAPH_PROPERTY_ = 18;
        private static readonly int DEFAULT_IGNORABLE_CODE_POINT_PROPERTY_ = 19;
        private static readonly int DEPRECATED_PROPERTY_ = 20;
        private static readonly int LOGICAL_ORDER_EXCEPTION_PROPERTY_ = 21;
        private static readonly int XID_START_PROPERTY_ = 22;
        private static readonly int XID_CONTINUE_PROPERTY_ = 23;
        private static readonly int ID_START_PROPERTY_ = 24;
        private static readonly int ID_CONTINUE_PROPERTY_ = 25;
        private static readonly int GRAPHEME_BASE_PROPERTY_ = 26;
        private static readonly int S_TERM_PROPERTY_ = 27;
        private static readonly int VARIATION_SELECTOR_PROPERTY_ = 28;
        private static readonly int PATTERN_SYNTAX = 29;                   /* new in ICU 3.4 and Unicode 4.1 */
        private static readonly int PATTERN_WHITE_SPACE = 30;
        private static readonly int PREPENDED_CONCATENATION_MARK = 31;     // new in ICU 60 and Unicode 10

        /*
         * Properties in vector word 2
         * Bits
         * 31..27   http://www.unicode.org/reports/tr51/#Emoji_Properties
         *     26   reserved
         * 25..20   Line Break
         * 19..15   Sentence Break
         * 14..10   Word Break
         *  9.. 5   Grapheme Cluster Break
         *  4.. 0   Decomposition Type
         */
        private static readonly int PROPS_2_EMOJI_COMPONENT = 27;
        private static readonly int PROPS_2_EMOJI = 28;
        private static readonly int PROPS_2_EMOJI_PRESENTATION = 29;
        private static readonly int PROPS_2_EMOJI_MODIFIER = 30;
        private static readonly int PROPS_2_EMOJI_MODIFIER_BASE = 31;

        private static readonly int LB_MASK = 0x03f00000;
        private static readonly int LB_SHIFT = 20;

        private static readonly int SB_MASK = 0x000f8000;
        private static readonly int SB_SHIFT = 15;

        private static readonly int WB_MASK = 0x00007c00;
        private static readonly int WB_SHIFT = 10;

        private static readonly int GCB_MASK = 0x000003e0;
        private static readonly int GCB_SHIFT = 5;

        /// <summary>
        /// Integer properties mask for decomposition type.
        /// Equivalent to icu4c UPROPS_DT_MASK.
        /// </summary>
        private static readonly int DECOMPOSITION_TYPE_MASK_ = 0x0000001f;

        /// <summary>
        /// First nibble shift
        /// </summary>
        private static readonly int FIRST_NIBBLE_SHIFT_ = 0x4;
        /// <summary>
        /// Second nibble mask
        /// </summary>
        private static readonly int LAST_NIBBLE_MASK_ = 0xF;
        /// <summary>
        /// Age value shift
        /// </summary>
        private static readonly int AGE_SHIFT_ = 24;


        // private constructors --------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <exception cref="IOException">Thrown when data reading fails or data corrupted.</exception>
        private UCharacterProperty()
        {
            Init();

            // consistency check
#pragma warning disable 612, 618
            if (binProps.Length != (int)UProperty.Binary_Limit)
            {
                throw new ICUException("binProps.length!=UProperty.BINARY_LIMIT");
            }
            if (intProps.Length != ((int)UProperty.Int_Limit - (int)UProperty.Int_Start))
            {
                throw new ICUException("intProps.length!=(UProperty.INT_LIMIT-UProperty.INT_START)");
            }
#pragma warning restore 612, 618

            // jar access
            ByteBuffer bytes = ICUBinary.GetRequiredData(DATA_FILE_NAME_);
            m_unicodeVersion_ = ICUBinary.ReadHeaderAndDataVersion(bytes, DATA_FORMAT, new IsAcceptable());
            // Read or skip the 16 indexes.
            int propertyOffset = bytes.GetInt32();
            /* exceptionOffset = */
            bytes.GetInt32();
            /* caseOffset = */
            bytes.GetInt32();
            int additionalOffset = bytes.GetInt32();
            int additionalVectorsOffset = bytes.GetInt32();
            m_additionalColumnsCount_ = bytes.GetInt32();
            int scriptExtensionsOffset = bytes.GetInt32();
            int reservedOffset7 = bytes.GetInt32();
            /* reservedOffset8 = */
            bytes.GetInt32();
            /* dataTopOffset = */
            bytes.GetInt32();
            m_maxBlockScriptValue_ = bytes.GetInt32();
            m_maxJTGValue_ = bytes.GetInt32();
            ICUBinary.SkipBytes(bytes, (16 - 12) << 2);

            // read the main properties trie
            m_trie_ = Trie2_16.CreateFromSerialized(bytes);
            int expectedTrieLength = (propertyOffset - 16) * 4;
            int trieLength = m_trie_.GetSerializedLength();
            if (trieLength > expectedTrieLength)
            {
                throw new IOException("uprops.icu: not enough bytes for main trie");
            }
            // skip padding after trie bytes
            ICUBinary.SkipBytes(bytes, expectedTrieLength - trieLength);

            // skip unused intervening data structures
            ICUBinary.SkipBytes(bytes, (additionalOffset - propertyOffset) * 4);

            if (m_additionalColumnsCount_ > 0)
            {
                // reads the additional property block
                m_additionalTrie_ = Trie2_16.CreateFromSerialized(bytes);
                expectedTrieLength = (additionalVectorsOffset - additionalOffset) * 4;
                trieLength = m_additionalTrie_.GetSerializedLength();
                if (trieLength > expectedTrieLength)
                {
                    throw new IOException("uprops.icu: not enough bytes for additional-properties trie");
                }
                // skip padding after trie bytes
                ICUBinary.SkipBytes(bytes, expectedTrieLength - trieLength);

                // additional properties
                int size = scriptExtensionsOffset - additionalVectorsOffset;
                m_additionalVectors_ = ICUBinary.GetInts(bytes, size, 0);
            }

            // Script_Extensions
            int numChars = (reservedOffset7 - scriptExtensionsOffset) * 2;
            if (numChars > 0)
            {
                m_scriptExtensions_ = ICUBinary.GetChars(bytes, numChars, 0);
            }
        }

        private sealed class IsAcceptable : IAuthenticate
        {
            // @Override when we switch to Java 6
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 7;
            }
        }
        private static readonly int DATA_FORMAT = 0x5550726F;  // "UPro"

        // private methods -------------------------------------------------------

        /*
         * Compare additional properties to see if it has argument type
         * @param property 32 bit properties
         * @param type character type
         * @return true if property has type
         */
        /*private boolean compareAdditionalType(int property, int type)
        {
            return (property & (1 << type)) != 0;
        }*/

        // property starts for UnicodeSet -------------------------------------- ***

        private static readonly int TAB = 0x0009;
        //private static readonly int LF      = 0x000a;
        //private static readonly int FF      = 0x000c;
        private static readonly int CR = 0x000d;
        private static readonly int U_A = 0x0041;
        private static readonly int U_F = 0x0046;
        private static readonly int U_Z = 0x005a;
        private static readonly int U_a = 0x0061;
        private static readonly int U_f = 0x0066;
        private static readonly int U_z = 0x007a;
        private static readonly int DEL = 0x007f;
        private static readonly int NL = 0x0085;
        private static readonly int NBSP = 0x00a0;
        private static readonly int CGJ = 0x034f;
        private static readonly int FIGURESP = 0x2007;
        private static readonly int HAIRSP = 0x200a;
        //private static readonly int ZWNJ    = 0x200c;
        //private static readonly int ZWJ     = 0x200d;
        private static readonly int RLM = 0x200f;
        private static readonly int NNBSP = 0x202f;
        private static readonly int WJ = 0x2060;
        private static readonly int INHSWAP = 0x206a;
        private static readonly int NOMDIG = 0x206f;
        private static readonly int U_FW_A = 0xff21;
        private static readonly int U_FW_F = 0xff26;
        private static readonly int U_FW_Z = 0xff3a;
        private static readonly int U_FW_a = 0xff41;
        private static readonly int U_FW_f = 0xff46;
        private static readonly int U_FW_z = 0xff5a;
        private static readonly int ZWNBSP = 0xfeff;

        public UnicodeSet AddPropertyStarts(UnicodeSet set)
        {
            /* add the start code point of each same-value range of the main trie */
            using (var trieIterator = m_trie_.GetEnumerator())
            {
                Trie2.Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                {
                    set.Add(range.StartCodePoint);
                }
            }

            /* add code points with hardcoded properties, plus the ones following them */

            /* add for u_isblank() */
            set.Add(TAB);
            set.Add(TAB + 1);

            /* add for IS_THAT_CONTROL_SPACE() */
            set.Add(CR + 1); /* range TAB..CR */
            set.Add(0x1c);
            set.Add(0x1f + 1);
            set.Add(NL);
            set.Add(NL + 1);

            /* add for u_isIDIgnorable() what was not added above */
            set.Add(DEL); /* range DEL..NBSP-1, NBSP added below */
            set.Add(HAIRSP);
            set.Add(RLM + 1);
            set.Add(INHSWAP);
            set.Add(NOMDIG + 1);
            set.Add(ZWNBSP);
            set.Add(ZWNBSP + 1);

            /* add no-break spaces for u_isWhitespace() what was not added above */
            set.Add(NBSP);
            set.Add(NBSP + 1);
            set.Add(FIGURESP);
            set.Add(FIGURESP + 1);
            set.Add(NNBSP);
            set.Add(NNBSP + 1);

            /* add for u_charDigitValue() */
            // TODO remove when UChar.getHanNumericValue() is changed to just return
            // Unicode numeric values
            set.Add(0x3007);
            set.Add(0x3008);
            set.Add(0x4e00);
            set.Add(0x4e01);
            set.Add(0x4e8c);
            set.Add(0x4e8d);
            set.Add(0x4e09);
            set.Add(0x4e0a);
            set.Add(0x56db);
            set.Add(0x56dc);
            set.Add(0x4e94);
            set.Add(0x4e95);
            set.Add(0x516d);
            set.Add(0x516e);
            set.Add(0x4e03);
            set.Add(0x4e04);
            set.Add(0x516b);
            set.Add(0x516c);
            set.Add(0x4e5d);
            set.Add(0x4e5e);

            /* add for u_digit() */
            set.Add(U_a);
            set.Add(U_z + 1);
            set.Add(U_A);
            set.Add(U_Z + 1);
            set.Add(U_FW_a);
            set.Add(U_FW_z + 1);
            set.Add(U_FW_A);
            set.Add(U_FW_Z + 1);

            /* add for u_isxdigit() */
            set.Add(U_f + 1);
            set.Add(U_F + 1);
            set.Add(U_FW_f + 1);
            set.Add(U_FW_F + 1);

            /* add for UCHAR_DEFAULT_IGNORABLE_CODE_POINT what was not added above */
            set.Add(WJ); /* range WJ..NOMDIG */
            set.Add(0xfff0);
            set.Add(0xfffb + 1);
            set.Add(0xe0000);
            set.Add(0xe0fff + 1);

            /* add for UCHAR_GRAPHEME_BASE and others */
            set.Add(CGJ);
            set.Add(CGJ + 1);

            return set; // for chaining
        }

        public void upropsvec_addPropertyStarts(UnicodeSet set) // ICU4N TODO: API - rename to use .NET Conventions
        {
            /* add the start code point of each same-value range of the properties vectors trie */
            if (m_additionalColumnsCount_ > 0)
            {
                /* if m_additionalColumnsCount_==0 then the properties vectors trie may not be there at all */
                using (var trieIterator = m_additionalTrie_.GetEnumerator())
                {
                    Trie2.Range range;
                    while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                    {
                        set.Add(range.StartCodePoint);
                    }
                }
            }
        }

        // This static initializer block must be placed after
        // other static member initialization
        static UCharacterProperty()
        {
            try
            {
                instance = new UCharacterProperty();
            }
            catch (IOException e)
            {
                throw new MissingManifestResourceException(e.ToString(), e);
            }
        }

        /*----------------------------------------------------------------
         * Inclusions list
         *----------------------------------------------------------------*/

        /*
         * Return a set of characters for property enumeration.
         * The set implicitly contains 0x110000 as well, which is one more than the highest
         * Unicode code point.
         *
         * This set is used as an ordered list - its code points are ordered, and
         * consecutive code points (in Unicode code point order) in the set define a range.
         * For each two consecutive characters (start, limit) in the set,
         * all of the UCD/normalization and related properties for
         * all code points start..limit-1 are all the same,
         * except for character names and ISO comments.
         *
         * All Unicode code points U+0000..U+10ffff are covered by these ranges.
         * The ranges define a partition of the Unicode code space.
         * ICU uses the inclusions set to enumerate properties for generating
         * UnicodeSets containing all code points that have a certain property value.
         *
         * The Inclusion List is generated from the UCD. It is generated
         * by enumerating the data tries, and code points for hardcoded properties
         * are added as well.
         *
         * --------------------------------------------------------------------------
         *
         * The following are ideas for getting properties-unique code point ranges,
         * with possible optimizations beyond the current implementation.
         * These optimizations would require more code and be more fragile.
         * The current implementation generates one single list (set) for all properties.
         *
         * To enumerate properties efficiently, one needs to know ranges of
         * repetitive values, so that the value of only each start code point
         * can be applied to the whole range.
         * This information is in principle available in the uprops.icu/unorm.icu data.
         *
         * There are two obstacles:
         *
         * 1. Some properties are computed from multiple data structures,
         *    making it necessary to get repetitive ranges by intersecting
         *    ranges from multiple tries.
         *
         * 2. It is not economical to write code for getting repetitive ranges
         *    that are precise for each of some 50 properties.
         *
         * Compromise ideas:
         *
         * - Get ranges per trie, not per individual property.
         *   Each range contains the same values for a whole group of properties.
         *   This would generate currently five range sets, two for uprops.icu tries
         *   and three for unorm.icu tries.
         *
         * - Combine sets of ranges for multiple tries to get sufficient sets
         *   for properties, e.g., the uprops.icu main and auxiliary tries
         *   for all non-normalization properties.
         *
         * Ideas for representing ranges and combining them:
         *
         * - A UnicodeSet could hold just the start code points of ranges.
         *   Multiple sets are easily combined by or-ing them together.
         *
         * - Alternatively, a UnicodeSet could hold each even-numbered range.
         *   All ranges could be enumerated by using each start code point
         *   (for the even-numbered ranges) as well as each limit (end+1) code point
         *   (for the odd-numbered ranges).
         *   It should be possible to combine two such sets by xor-ing them,
         *   but no more than two.
         *
         * The second way to represent ranges may(?!) yield smaller UnicodeSet arrays,
         * but the first one is certainly simpler and applicable for combining more than
         * two range sets.
         *
         * It is possible to combine all range sets for all uprops/unorm tries into one
         * set that can be used for all properties.
         * As an optimization, there could be less-combined range sets for certain
         * groups of properties.
         * The relationship of which less-combined range set to use for which property
         * depends on the implementation of the properties and must be hardcoded
         * - somewhat error-prone and higher maintenance but can be tested easily
         * by building property sets "the simple way" in test code.
         *
         * ---
         *
         * Do not use a UnicodeSet pattern because that causes infinite recursion;
         * UnicodeSet depends on the inclusions set.
         *
         * ---
         *
         * getInclusions() is commented out starting 2005-feb-12 because
         * UnicodeSet now calls the uxyz_addPropertyStarts() directly,
         * and only for the relevant property source.
         */
        /*
        public UnicodeSet getInclusions() {
            UnicodeSet set = new UnicodeSet();
            NormalizerImpl.addPropertyStarts(set);
            addPropertyStarts(set);
            return set;
        }
        */
    }
}
