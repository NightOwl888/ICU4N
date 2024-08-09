using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using J2N;
using J2N.IO;
using J2N.Text;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Result values for <see cref="Normalizer.QuickCheck(string, NormalizerMode)"/> and
    /// <see cref="Normalizer2.QuickCheck(string)"/>.
    /// For details see Unicode Technical Report 15.
    /// </summary>
    public enum QuickCheckResult
    {
        /// <summary>
        /// Indicates that string is not in the normalized format.
        /// </summary>
        No = 0,

        /// <summary>
        /// Indicates that string is in the normalized format.
        /// </summary>
        Yes = 1,

        /// <summary>
        /// Indicates it cannot be determined if string is in the normalized
        /// format without further thorough checks.
        /// </summary>
        Maybe = 2
    }

    /// <summary>
    /// <see cref="Normalizer"/> Unicode version options.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlagsAttribute", Justification = "This enum contains options that can only be selected individually")]
    public enum NormalizerUnicodeVersion
    {
        /// <summary>
        /// Optionto select the default Unicode normalization
        /// (except NormalizationCorrections).
        /// </summary>
        Default = 0x00,

        /// <summary>
        /// Option to select Unicode 3.2 normalization
        /// (except NormalizationCorrections).
        /// </summary>
        [Obsolete("ICU 56 Use FilteredNormalizer2 instead.")]
        Unicode3_2 = 0x20
    }

    /// <summary>
    /// Options for <see cref="Normalizer"/> text comparison
    /// </summary>
    [Flags]
    public enum NormalizerComparison
    {
        /// <summary>
        /// Option bit for compare:
        /// Use when no other option is required. This has no effect if it is ORed with other options.
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        Default = 0,

        /// <summary>
        /// Option bit for compare:
        /// Both input strings are assumed to fulfill FCD conditions.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        InputIsFCD = 0x20000,

        /// <summary>
        /// Option bit for compare:
        /// Perform case-insensitive comparison.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        IgnoreCase = 0x10000, // ICU4N specific - removed COMPARE_

        /// <summary>
        /// Option bit for compare:
        /// Compare strings in code point order instead of code unit order.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        CodePointOrder = 0x8000, // ICU4N specific - removed COMPARE_
    }

    /// <summary>
    /// Normalization mode constants.
    /// </summary>
    public enum NormalizerMode
    {
        /// <summary>
        /// No decomposition/composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        None = 1,

        /// <summary>
        /// Canonical decomposition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        NFD = 2,

        /// <summary>
        /// Compatibility decomposition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        NFKD = 3,

        /// <summary>
        /// Canonical decomposition followed by canonical composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        NFC = 4,

        /// <summary>
        /// Default normalization.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        Default = 0,

        /// <summary>
        /// Compatibility decomposition followed by canonical composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        NFKC = 5,

        /// <summary>
        /// "Fast C or D" form.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        FCD = 6
    }

    /// <summary>
    /// Old Unicode normalization API.
    /// <para/>
    /// This API has been replaced by the <see cref="Normalizer2"/> class and is only available
    /// for backward compatibility. This class simply delegates to the Normalizer2 class.
    /// There are two exceptions: The new API does not provide a replacement for
    /// <see cref="QuickCheckResult"/> and <see cref="Compare(string, string)"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> transforms Unicode text into an equivalent composed or
    /// decomposed form, allowing for easier sorting and searching of text.
    /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> supports the standard normalization forms described in
    /// <a href="http://www.unicode.org/unicode/reports/tr15/" target="unicode">
    /// Unicode Standard Annex #15 &amp;mdash; Unicode Normalization Forms</a>.
    /// <para/>
    /// Characters with accents or other adornments can be encoded in
    /// several different ways in Unicode.  For example, take the character A-acute.
    /// In Unicode, this can be encoded as a single character (the
    /// "composed" form):
    /// <code>
    ///      00C1    LATIN CAPITAL LETTER A WITH ACUTE
    /// </code>
    /// or as two separate characters (the "decomposed" form):
    /// <code>
    ///      0041    LATIN CAPITAL LETTER A
    ///      0301    COMBINING ACUTE ACCENT
    /// </code>
    /// <para/>
    /// To a user of your program, however, both of these sequences should be
    /// treated as the same "user-level" character "A with acute accent".  When you
    /// are searching or comparing text, you must ensure that these two sequences are
    /// treated equivalently.  In addition, you must handle characters with more than
    /// one accent.  Sometimes the order of a character's combining accents is
    /// significant, while in other cases accent sequences in different orders are
    /// really equivalent.
    /// <para/>
    /// Similarly, the string "ffi" can be encoded as three separate letters:
    /// <code>
    ///      0066    LATIN SMALL LETTER F
    ///      0066    LATIN SMALL LETTER F
    ///      0069    LATIN SMALL LETTER I
    /// </code>
    /// or as the single character
    /// <code>
    ///      FB03    LATIN SMALL LIGATURE FFI
    /// </code>
    /// <para/>
    /// The ffi ligature is not a distinct semantic character, and strictly speaking
    /// it shouldn't be in Unicode at all, but it was included for compatibility
    /// with existing character sets that already provided it.  The Unicode standard
    /// identifies such characters by giving them "compatibility" decompositions
    /// into the corresponding semantic characters.  When sorting and searching, you
    /// will often want to use these mappings.
    /// <para/>
    /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> helps solve these problems by transforming text into
    /// the canonical composed and decomposed forms as shown in the first example
    /// above. In addition, you can have it perform compatibility decompositions so
    /// that you can treat compatibility characters the same as their equivalents.
    /// Finally, <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> rearranges accents into the proper canonical
    /// order, so that you do not have to worry about accent rearrangement on your
    /// own.
    /// <para/>
    /// Form FCD, "Fast C or D", is also designed for collation.
    /// It allows to work on strings that are not necessarily normalized
    /// with an algorithm (like in collation) that works under "canonical closure",
    /// i.e., it treats precomposed characters and their decomposed equivalents the
    /// same.
    /// <para/>
    /// It is not a normalization form because it does not provide for uniqueness of
    /// representation. Multiple strings may be canonically equivalent (their NFDs
    /// are identical) and may all conform to FCD without being identical themselves.
    /// The form is defined such that the "raw decomposition", the recursive
    /// canonical decomposition of each character, results in a string that is
    /// canonically ordered. This means that precomposed characters are allowed for
    /// as long as their decompositions do not need canonical reordering.
    /// <para/>
    /// Its advantage for a process like collation is that all NFD and most NFC texts
    /// - and many unnormalized texts - already conform to FCD and do not need to be
    /// normalized (NFD) for such a process. The FCD quick check will return YES for
    /// most strings in practice.
    /// <para/>
    /// Normalize(FCD) may be implemented with NFD.
    /// <para/>
    /// For more details on FCD see Unicode Technical Note #5 (Canonical Equivalence in Applications):
    /// <a href="http://www.unicode.org/notes/tn5/#FCD">http://www.unicode.org/notes/tn5/#FCD</a>
    /// <para/>
    /// ICU collation performs either NFD or FCD normalization automatically if
    /// normalization is turned on for the collator object. Beyond collation and
    /// string search, normalized strings may be useful for string equivalence
    /// comparisons, transliteration/transcription, unique representations, etc.
    /// <para/>
    /// The W3C generally recommends to exchange texts in NFC.
    /// Note also that most legacy character encodings use only precomposed forms and
    /// often do not encode any combining marks by themselves. For conversion to such
    /// character encodings the Unicode text needs to be normalized to NFC.
    /// For more usage examples, see the Unicode Standard Annex.
    /// <para/>
    /// Note: The <see cref="Normalizer"/> class also provides API for iterative normalization.
    /// While the <see cref="SetIndex(int)"/> and <see cref="Index"/> refer to indices in the
    /// underlying Unicode input text, the <see cref="Next()"/> and <see cref="Previous()"/> methods
    /// iterate through characters in the normalized output.
    /// This means that there is not necessarily a one-to-one correspondence
    /// between characters returned by <see cref="Next()"/> and <see cref="Previous()"/> and the indices
    /// passed to and returned from <see cref="SetIndex(int)"/> and <see cref="Index"/>.
    /// It is for this reason that <see cref="Normalizer"/> does not implement the <see cref="CharacterIterator"/> interface.
    /// </remarks>
    public sealed partial class Normalizer
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        private const int CharStackBufferSize = 64;

        // The input text and our position in it
        private UCharacterIterator text;
        private Normalizer2 norm2;
#pragma warning disable 612, 618
        private Mode mode;
#pragma warning restore 612, 618
        private int options; // ICU4N specific - we don't need to bit shift using COMPARE_NORM_OPTIONS_SHIFT because it only applies to the static Compare methods and our instance no longer uses other options than NormalizerUnicodeVersion

        // The normalization buffer is the result of normalization
        // of the source in [currentIndex..nextIndex[ .
        private int currentIndex;
        private int nextIndex;

        // A buffer for holding intermediate results
        private OpenStringBuilder buffer;
        private int bufferPos;

        // Helper classes to defer loading of normalization data.
        private sealed class ModeImpl
        {
            internal ModeImpl(Normalizer2 n2)
            {
                Normalizer2 = n2;
            }

            internal Normalizer2 Normalizer2 { get; private set; }
        }
        private sealed class NFDModeImpl
        {
            private static readonly ModeImpl instance = new ModeImpl(Normalizer2.NFDInstance);
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFKDModeImpl
        {
            private static readonly ModeImpl instance = new ModeImpl(Normalizer2.NFKDInstance);
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFCModeImpl
        {
            private static readonly ModeImpl instance = new ModeImpl(Normalizer2.NFCInstance);
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFKCModeImpl
        {
            private static readonly ModeImpl instance = new ModeImpl(Normalizer2.NFKCInstance);
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class FCDModeImpl
        {
            private static readonly ModeImpl instance = new ModeImpl(Norm2AllModes.FCDNormalizer2);
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }

        private sealed class Unicode32
        {
            private static readonly UnicodeSet instance = new UnicodeSet("[:age=3.2:]").Freeze();
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static UnicodeSet Instance => instance;
        }
        private sealed class NFD32ModeImpl
        {
            private static readonly ModeImpl instance =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.NFDInstance,
                                                 Unicode32.Instance));
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFKD32ModeImpl
        {
            private static readonly ModeImpl instance =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.NFKDInstance,
                                                 Unicode32.Instance));
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFC32ModeImpl
        {
            private static readonly ModeImpl instance =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.NFCInstance,
                                                 Unicode32.Instance));
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class NFKC32ModeImpl
        {
            private static readonly ModeImpl instance =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.NFKCInstance,
                                                 Unicode32.Instance));
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }
        private sealed class FCD32ModeImpl
        {
            private static readonly ModeImpl instance =
                new ModeImpl(new FilteredNormalizer2(Norm2AllModes.FCDNormalizer2,
                                                 Unicode32.Instance));
            /// <summary>
            /// public singleton instance
            /// </summary>
            public static ModeImpl Instance => instance;
        }

        /// <summary>
        /// Options bit set value to select Unicode 3.2 normalization
        /// (except NormalizationCorrections).
        /// At most one Unicode version can be selected at a time.
        /// </summary>
        [Obsolete("ICU 56 Use FilteredNormalizer2 instead.")]
        internal const int Unicode3_2 = (int)NormalizerUnicodeVersion.Unicode3_2; // ICU4N specific - changed from public to internal, as we have an enum to supply the value

        /// <summary>
        /// Constant indicating that the end of the iteration has been reached.
        /// This is guaranteed to have the same value as <see cref="UCharacterIterator.Done"/>.
        /// </summary>
        [Obsolete("ICU 56")]
        public const int Done = UForwardCharacterIterator.Done;

        /// <summary>
        /// Constants for normalization modes.
        /// <para/>
        /// The <see cref="Mode"/> class is not intended for public subclassing.
        /// Only the <see cref="Mode"/> constants provided by the Normalizer class should be used,
        /// and any fields or methods should not be called or overridden by users.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private abstract class Mode // ICU4N specific - changed from public to private because it is obsolete and all subclasses are private.
        {
            internal NormalizerMode NormalizerMode { get; private set; }

            /// <summary>
            /// Sole constructor.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            protected Mode(NormalizerMode normalizerMode)
            {
                this.NormalizerMode = normalizerMode;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            protected internal abstract Normalizer2 GetNormalizer2(int options);
        }

#pragma warning disable 612, 618, 672
        private sealed class NONEMode : Mode
        {
            public NONEMode()
                : base(NormalizerMode.None)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options) { return Norm2AllModes.NoopNormalizer2; }
        }
        private sealed class NFDMode : Mode
        {
            public NFDMode()
                : base(NormalizerMode.NFD)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & Unicode3_2) != 0 ?
                        NFD32ModeImpl.Instance.Normalizer2 : NFDModeImpl.Instance.Normalizer2;
            }
        }
        private sealed class NFKDMode : Mode
        {
            public NFKDMode()
                : base(NormalizerMode.NFKD)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & Unicode3_2) != 0 ?
                        NFKD32ModeImpl.Instance.Normalizer2 : NFKDModeImpl.Instance.Normalizer2;
            }
        }
        private sealed class NFCMode : Mode
        {
            public NFCMode()
                : base(NormalizerMode.NFC)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & Unicode3_2) != 0 ?
                        NFC32ModeImpl.Instance.Normalizer2 : NFCModeImpl.Instance.Normalizer2;
            }
        }
        private sealed class NFKCMode : Mode
        {
            public NFKCMode()
                : base(NormalizerMode.NFKC)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & Unicode3_2) != 0 ?
                        NFKC32ModeImpl.Instance.Normalizer2 : NFKCModeImpl.Instance.Normalizer2;
            }
        }
        private sealed class FCDMode : Mode
        {
            public FCDMode()
                : base(NormalizerMode.FCD)
            { }
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & Unicode3_2) != 0 ?
                        FCD32ModeImpl.Instance.Normalizer2 : FCDModeImpl.Instance.Normalizer2;
            }
        }

        /// <summary>
        /// No decomposition/composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode NONE = new NONEMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Canonical decomposition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode NFD = new NFDMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Compatibility decomposition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode NFKD = new NFKDMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Canonical decomposition followed by canonical composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode NFC = new NFCMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Default normalization.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode DEFAULT = NFC; // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Compatibility decomposition followed by canonical composition.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode NFKC = new NFKCMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// "Fast C or D" form.
        /// </summary>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        private static readonly Mode FCD = new FCDMode(); // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Null operation for use with the <see cref="Normalizer(string, NormalizerMode, NormalizerUnicodeVersion)"/> constructors.
        /// and the static <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> method.  This value tells
        /// the <see cref="Normalizer"/> to do nothing but return unprocessed characters
        /// from the underlying string or <see cref="CharacterIterator"/>.  If you have code which
        /// requires raw text at some times and normalized text at others, you can
        /// use <c>NO_OP</c> for the cases where you want raw text, rather
        /// than having a separate code path that bypasses <see cref="Normalizer"/>
        /// altogether.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        /// <seealso cref="NormalizerMode.None"/>
        [Obsolete("ICU 2.8. Use Nomalizer.NONE")]
        private static readonly Mode NO_OP = NONE; // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Canonical decomposition followed by canonical composition.  Used 
        /// with the <see cref="Normalizer(string, NormalizerMode, NormalizerUnicodeVersion)"/> constructors.
        /// and the static <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> method
        /// to determine the operation to be performed.
        /// <para/>
        /// If all optional features (<i>e.g.</i> <see cref="IGNORE_HANGUL"/>) are turned
        /// off, this operation produces output that is in
        /// <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Canonical
        /// Form</a> <b>C</b>.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        /// <seealso cref="NormalizerMode.NFC"/>
        [Obsolete("ICU 2.8. Use Normalier.NFC")]
        private static readonly Mode COMPOSE = NFC; // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Compatibility decomposition followed by canonical composition.
        /// Used 
        /// with the <see cref="Normalizer(string, NormalizerMode, NormalizerUnicodeVersion)"/> constructors.
        /// and the static <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> method
        /// to determine the operation to be performed.
        /// <para/>
        /// If all optional features (<i>e.g.</i> <see cref="IGNORE_HANGUL"/>) are turned
        /// off, this operation produces output that is in
        /// <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Canonical
        /// Form</a> <b>KC</b>.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        /// <seealso cref="NormalizerMode.NFKC"/>
        [Obsolete("ICU 2.8. Use Normalier.NFKC")]
        private static readonly Mode COMPOSE_COMPAT = NFKC; // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Canonical decomposition.  This value is passed to the
        /// <see cref="Normalizer(string, NormalizerMode, NormalizerUnicodeVersion)"/> constructors.
        /// and the static <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> method
        /// to determine the operation to be performed.
        /// <para/>
        /// If all optional features (<i>e.g.</i> <see cref="IGNORE_HANGUL"/>) are turned
        /// off, this operation produces output that is in
        /// <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Canonical
        /// Form</a> <b>D</b>.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        /// <seealso cref="NFD"/>
        [Obsolete("ICU 2.8. Use Normalier.NFD")]
        private static readonly Mode DECOMP = NFD; // ICU4N specific - made private because we don't need access out of this class

        /// <summary>
        /// Compatibility decomposition.  This value is passed to the
        /// <see cref="Normalizer(string, NormalizerMode, NormalizerUnicodeVersion)"/> constructors.
        /// and the static <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/> method
        /// to determine the operation to be performed.
        /// <para/>
        /// If all optional features (<i>e.g.</i> <see cref="IGNORE_HANGUL"/>) are turned
        /// off, this operation produces output that is in
        /// <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Canonical
        /// Form</a> <b>KD</b>.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        /// <seealso cref="NormalizerMode.NFKD"/>
        [Obsolete("ICU 2.8. Use Normalier.NFKD")]
        private static readonly Mode DECOMP_COMPAT = NFKD; // ICU4N specific - made private because we don't need access out of this class
#pragma warning restore 612, 618, 672

        /// <summary>
        /// Option to disable Hangul/Jamo composition and decomposition.
        /// This option applies to Korean text,
        /// which can be represented either in the Jamo alphabet or in Hangul
        /// characters, which are really just two or three Jamo combined
        /// into one visual glyph.  Since Jamo takes up more storage space than
        /// Hangul, applications that process only Hangul text may wish to turn
        /// this option on when decomposing text.
        /// <para/>
        /// The Unicode standard treates Hangul to Jamo conversion as a
        /// canonical decomposition, so this option must be turned <b>off</b> if you
        /// wish to transform strings into one of the standard
        /// <a href="http://www.unicode.org/unicode/reports/tr15/" target="unicode">
        /// Unicode Normalization Forms</a>.
        /// </summary>
        /// <see cref="SetOption(int, bool)"/>
        [Obsolete("ICU 2.8. This option is no longer supported.")]
        internal const int IGNORE_HANGUL = 0x0001; // ICU4N specific - this option is not supported anywhere, so making it internal

        // ICU4N specific - de-nested QuickCheckResult

        /// <summary>
        /// Indicates that string is not in the normalized format
        /// </summary>
        /// <stable>ICU 2.8</stable>
        private const QuickCheckResult NO = QuickCheckResult.No; // ICU4N specific - marked internal for testing (we use the enum in .NET)

        /// <summary>
        /// Indicates that string is in the normalized format
        /// </summary>
        /// <stable>ICU 2.8</stable>
        private const QuickCheckResult YES = QuickCheckResult.Yes; // ICU4N specific - marked internal for testing (we use the enum in .NET)

        /// <summary>
        /// Indicates it cannot be determined if string is in the normalized
        /// format without further thorough checks.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        private const QuickCheckResult MAYBE = QuickCheckResult.Maybe; // ICU4N specific - marked internal for testing (we use the enum in .NET)

        /// <summary>
        /// Option bit for compare:
        /// Case sensitively compare the strings
        /// </summary>
        /// <stable>ICU 2.8</stable>
        internal const int FOLD_CASE_DEFAULT = (int)FoldCase.Default; // ICU4N specific - marked internal (we use the enum in .NET)

        /// <summary>
        /// Option bit for compare:
        /// Both input strings are assumed to fulfill FCD conditions.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        internal const int INPUT_IS_FCD = (int)NormalizerComparison.InputIsFCD; // ICU4N specific - marked internal (we use the enum in .NET)

        /// <summary>
        /// Option bit for compare:
        /// Perform case-insensitive comparison.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        internal const int COMPARE_IGNORE_CASE = (int)NormalizerComparison.IgnoreCase; // ICU4N specific - marked internal (we use the enum in .NET)

        /// <summary>
        /// Option bit for compare:
        /// Compare strings in code point order instead of code unit order.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        internal const int COMPARE_CODE_POINT_ORDER = (int)NormalizerComparison.CodePointOrder; // ICU4N specific - marked internal (we use the enum in .NET)

        /// <summary>
        /// Option value for case folding:
        /// Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
        /// and dotless i appropriately for Turkic languages (tr, az).
        /// </summary>
        /// <seealso cref="UChar.FoldCaseExcludeSpecialI"/>
        /// <stable>ICU 2.8</stable>
        internal const int FOLD_CASE_EXCLUDE_SPECIAL_I = (int)FoldCase.ExcludeSpecialI; // ICU4N specific - marked internal (we use the enum in .NET)

        /// <summary>
        /// Lowest-order bit number of <see cref="Compare(string, string)"/> options bits corresponding to
        /// normalization options bits.
        /// <para/>
        /// The options parameter for <see cref="Compare(string, string)"/> uses most bits for
        /// itself and for various comparison and folding flags.
        /// The most significant bits, however, are shifted down and passed on
        /// to the normalization implementation.
        /// (That is, from compare(..., options, ...),
        /// options&gt;&gt;COMPARE_NORM_OPTIONS_SHIFT will be passed on to the
        /// internal normalization functions.)
        /// </summary>
        /// <seealso cref="Compare(string, string)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        internal const int COMPARE_NORM_OPTIONS_SHIFT = 20; // ICU4N specific - We don't need to shift bits anymore because we use a separate enum for UnicodeVersion, so that option is passed where it needs to be

        //-------------------------------------------------------------------------
        // Iterator constructors
        //-------------------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of a given string.
        /// </summary>
        /// <param name="str">The string to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(string str, NormalizerMode mode)
            : this(str, mode, NormalizerUnicodeVersion.Default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of a given string.
        /// </summary>
        /// <param name="str">The string to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(ReadOnlySpan<char> str, NormalizerMode mode)
            : this(str, mode, NormalizerUnicodeVersion.Default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of a given string.
        /// <para/>
        /// The <paramref name="unicodeVersion"/> parameter specifies which optional
        /// <see cref="Normalizer"/> features are to be enabled for this object.
        /// </summary>
        /// <param name="str">The string to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(string str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            this.text = UCharacterIterator.GetInstance(str);
            this.mode = GetModeInstance(mode);
            this.options = (int)unicodeVersion;
            norm2 = this.mode.GetNormalizer2(this.options);
            buffer = new OpenStringBuilder();
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of a given string.
        /// <para/>
        /// The <paramref name="unicodeVersion"/> parameter specifies which optional
        /// <see cref="Normalizer"/> features are to be enabled for this object.
        /// </summary>
        /// <param name="str">The string to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(ReadOnlySpan<char> str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            this.text = UCharacterIterator.GetInstance(str);
            this.mode = GetModeInstance(mode);
            this.options = (int)unicodeVersion;
            norm2 = this.mode.GetNormalizer2(this.options);
            buffer = new OpenStringBuilder();
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of the given text.
        /// </summary>
        /// <param name="iter">The input text to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(CharacterIterator iter, NormalizerMode mode)
            : this(iter, mode, NormalizerUnicodeVersion.Default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of the given text.
        /// </summary>
        /// <param name="iter">The input text to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(CharacterIterator iter, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            this.text = UCharacterIterator.GetInstance((CharacterIterator)iter.Clone());
            this.mode = GetModeInstance(mode);
            this.options = (int)unicodeVersion;
            norm2 = this.mode.GetNormalizer2(this.options);
            buffer = new OpenStringBuilder();
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of the given text.
        /// </summary>
        /// <param name="iter">The input text to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(UCharacterIterator iter, NormalizerMode mode)
            : this(iter, mode, NormalizerUnicodeVersion.Default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Normalizer"/> object for iterating over the
        /// normalized form of the given text.
        /// </summary>
        /// <param name="iter">The input text to be normalized.  The normalization
        /// will start at the beginning of the string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(UCharacterIterator iter, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            this.text = (UCharacterIterator)iter.Clone();
            this.mode = GetModeInstance(mode);
            this.options = (int)unicodeVersion;
            norm2 = this.mode.GetNormalizer2(this.options);
            buffer = new OpenStringBuilder();
        }

        /// <summary>
        /// Clones this <see cref="Normalizer"/> object.  All properties of this
        /// object are duplicated in the new object, including the cloning of any
        /// <see cref="CharacterIterator"/> that was passed in to the constructor
        /// or to <see cref="SetText(CharacterIterator)"/>.
        /// However, the text storage underlying
        /// the <see cref="CharacterIterator"/> is not duplicated unless the
        /// <see cref="CharacterIterator.Clone()"/> method does so.
        /// </summary>
        /// <returns></returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public object Clone()
        {
            Normalizer copy = (Normalizer)base.MemberwiseClone();
            copy.text = (UCharacterIterator)text.Clone();
            copy.mode = mode;
            copy.options = options;
            copy.norm2 = norm2;
            copy.buffer = new OpenStringBuilder(buffer.AsSpan());
            copy.bufferPos = bufferPos;
            copy.currentIndex = currentIndex;
            copy.nextIndex = nextIndex;
            return copy;
        }

        //--------------------------------------------------------------------------
        // Static Utility methods
        //--------------------------------------------------------------------------

        private static Normalizer2 GetComposeNormalizer2(bool compat, int options)
        {
#pragma warning disable 612, 618
            return (compat ? NFKC : NFC).GetNormalizer2(options);
#pragma warning restore 612, 618
        }
        private static Normalizer2 GetDecomposeNormalizer2(bool compat, int options)
        {
#pragma warning disable 612, 618
            return (compat ? NFKD : NFD).GetNormalizer2(options);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Compose a string.
        /// The string will be composed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to compose.</param>
        /// <param name="compat">If true the string will be composed according to
        /// <see cref="NormalizerMode.NFKC"/> rules and if false will be composed according to
        /// <see cref="NormalizerMode.NFC"/> rules.</param>
        /// <returns>The composed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(string str, bool compat)
        {
            return Compose(str.AsSpan(), compat, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compose a string.
        /// The string will be composed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to compose.</param>
        /// <param name="compat">If true the string will be composed according to
        /// <see cref="NormalizerMode.NFKC"/> rules and if false will be composed according to
        /// <see cref="NormalizerMode.NFC"/> rules.</param>
        /// <returns>The composed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(scoped ReadOnlySpan<char> str, bool compat)
        {
            return Compose(str, compat, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compose a string.
        /// The string will be composed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to compose.</param>
        /// <param name="compat">If true the string will be composed according to
        /// <see cref="NormalizerMode.NFKC"/> rules and if false will be composed according to
        /// <see cref="NormalizerMode.NFC"/> rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The composed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(string str, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetComposeNormalizer2(compat, (int)unicodeVersion).Normalize(str.AsSpan());
        }

        /// <summary>
        /// Compose a string.
        /// The string will be composed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to compose.</param>
        /// <param name="compat">If true the string will be composed according to
        /// <see cref="NormalizerMode.NFKC"/> rules and if false will be composed according to
        /// <see cref="NormalizerMode.NFC"/> rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The composed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(scoped ReadOnlySpan<char> str, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetComposeNormalizer2(compat, (int)unicodeVersion).Normalize(str);
        }

        /// <summary>
        /// Compose a string.
        /// The string will be composed to according to the specified mode.
        /// </summary>
        /// <param name="source">The string to compose.</param>
        /// <param name="destination">A span to receive the normalized text.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <param name="compat">If <c>true</c> the string will be composed according to
        /// <see cref="NormalizerMode.NFKC"/> rules and if <c>false</c> will be composed according to
        /// <see cref="NormalizerMode.NFC"/> rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool TryCompose(scoped ReadOnlySpan<char> source, Span<char> destination, out int charsLength, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetComposeNormalizer2(compat, (int)unicodeVersion).TryNormalize(source, destination, out charsLength);
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to decompose.</param>
        /// <param name="compat">If true the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if false will be decomposed according to <see cref="NormalizerMode.NFD"/>
        /// rules.
        /// </param>
        /// <returns>The decomposed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(string str, bool compat)
        {
            return Decompose(str.AsSpan(), compat, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to decompose.</param>
        /// <param name="compat">If true the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if false will be decomposed according to <see cref="NormalizerMode.NFD"/>
        /// rules.
        /// </param>
        /// <param name="destination">The buffer to write the decomposed string to.</param>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        internal static void Decompose(scoped ReadOnlySpan<char> str, bool compat, ref ValueStringBuilder destination) // ICU4N: Added to support StringSearch
        {
            GetDecomposeNormalizer2(compat, (int)NormalizerUnicodeVersion.Default).Normalize(str, ref destination);
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to decompose.</param>
        /// <param name="compat">If true the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if false will be decomposed according to <see cref="NormalizerMode.NFD"/>
        /// rules.
        /// </param>
        /// <returns>The decomposed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(scoped ReadOnlySpan<char> str, bool compat)
        {
            return Decompose(str, compat, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to decompose.</param>
        /// <param name="compat">If true the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if false will be decomposed according to <see cref="NormalizerMode.NFD"/>
        /// rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The decomposed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(string str, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetDecomposeNormalizer2(compat, (int)unicodeVersion).Normalize(str.AsSpan());
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="str">The string to decompose.</param>
        /// <param name="compat">If true the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if false will be decomposed according to <see cref="NormalizerMode.NFD"/>
        /// rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The decomposed string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(scoped ReadOnlySpan<char> str, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetDecomposeNormalizer2(compat, (int)unicodeVersion).Normalize(str);
        }

        /// <summary>
        /// Decompose a string.
        /// The string will be decomposed to according to the specified mode.
        /// </summary>
        /// <param name="source">The string to decompose.</param>
        /// <param name="destination">A span to receive the normalized text.</param>
        /// When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <param name="compat">If <c>true</c> the string will be decomposed according to <see cref="NormalizerMode.NFKD"/>
        /// rules and if <c>false</c> will be decomposed according to
        /// <see cref="NormalizerMode.NFD"/> rules.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool TryDecompose(scoped ReadOnlySpan<char> source, Span<char> destination, out int charsLength, bool compat, NormalizerUnicodeVersion unicodeVersion)
        {
            var sb = new ValueStringBuilder(destination);
            try
            {
                GetDecomposeNormalizer2(compat, (int)unicodeVersion).Normalize(source, ref sb);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Normalizes a <see cref="string"/> using the given normalization operation.
        /// </summary>
        /// <param name="str">The input string to be normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(string str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).Normalize(str);
        }

        /// <summary>
        /// Normalizes a <see cref="string"/> using the given normalization operation.
        /// </summary>
        /// <param name="str">The input string to be normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(scoped ReadOnlySpan<char> str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).Normalize(str);
        }

        /// <summary>
        /// Normalizes a <see cref="string"/> using the given normalization operation.
        /// </summary>
        /// <param name="str">The input string to be normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <param name="destination">The <see cref="ValueStringBuilder"/> in which to append the result.</param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        // ICU4N overload to support StringPrep
        internal static void Normalize(scoped ReadOnlySpan<char> str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion, ref ValueStringBuilder destination)
        {
#pragma warning disable 612, 618
            GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).Normalize(str, ref destination);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Normalize a string.
        /// The string will be normalized according to the specified normalization
        /// mode and options.
        /// </summary>
        /// <param name="src">The string to normalize.</param>
        /// <param name="mode">The normalization mode; one of <see cref="NormalizerMode.None"/>,
        /// <see cref="NormalizerMode.NFD"/>, <see cref="NormalizerMode.NFC"/>, <see cref="NormalizerMode.NFKC"/>,
        /// <see cref="NormalizerMode.NFKD"/>, <see cref="NormalizerMode.Default"/>.
        /// </param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(string src, NormalizerMode mode)
        {
            return Normalize(src, mode, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Normalize a string.
        /// The string will be normalized according to the specified normalization
        /// mode and options.
        /// </summary>
        /// <param name="src">The string to normalize.</param>
        /// <param name="mode">The normalization mode; one of <see cref="NormalizerMode.None"/>,
        /// <see cref="NormalizerMode.NFD"/>, <see cref="NormalizerMode.NFC"/>, <see cref="NormalizerMode.NFKC"/>,
        /// <see cref="NormalizerMode.NFKD"/>, <see cref="NormalizerMode.Default"/>.
        /// </param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(scoped ReadOnlySpan<char> src, NormalizerMode mode)
        {
            return Normalize(src, mode, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Normalize a string.
        /// The string will be normalized according to the specified normalization
        /// mode and options.
        /// </summary>
        /// <param name="source">The string to normalize.</param>
        /// <param name="target">A char buffer to receive the normalized text.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <param name="mode">The normalization mode; one of <see cref="NormalizerMode.None"/>,
        /// <see cref="NormalizerMode.NFD"/>, <see cref="NormalizerMode.NFC"/>, <see cref="NormalizerMode.NFKC"/>,
        /// <see cref="NormalizerMode.NFKD"/>, <see cref="NormalizerMode.Default"/>.
        /// </param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool TryNormalize(scoped ReadOnlySpan<char> source, Span<char> target, out int charsLength, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).TryNormalize(source, target, out charsLength);
        }

        /// <summary>
        /// Normalize a codepoint according to the given mode.
        /// </summary>
        /// <param name="char32">The input string to be normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(int char32, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            if (mode == NormalizerMode.NFD && unicodeVersion == 0)
            {
                string decomposition = Normalizer2.NFCInstance.GetDecomposition(char32);
                if (decomposition == null)
                {
                    decomposition = UTF16.ValueOf(char32);
                }
                return decomposition;
            }
            return Normalize(UTF16.ValueOf(char32, stackalloc char[2]), mode, unicodeVersion);
        }

        /// <summary>
        /// Convenience method to normalize a codepoint according to the given mode.
        /// </summary>
        /// <param name="char32">The input string to be normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <returns>The normalized string.</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(int char32, NormalizerMode mode)
        {
            return Normalize(char32, mode, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method.
        /// </summary>
        /// <param name="source">String for determining if it is in a normalized format.</param>
        /// <param name="mode">Normalization format (<see cref="NormalizerMode.NFC"/>,<see cref="NormalizerMode.NFD"/>,
        /// <see cref="NormalizerMode.NFKC"/>,<see cref="NormalizerMode.NFKD"/>).</param>
        /// <returns>Return code to specify if the text is normalized or not
        /// (<see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or <see cref="QuickCheckResult.Maybe"/>)</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static QuickCheckResult QuickCheck(string source, NormalizerMode mode)
        {
            return QuickCheck(source.AsSpan(), mode, 0);
        }

        /// <summary>
        /// Convenience method.
        /// </summary>
        /// <param name="source">String for determining if it is in a normalized format.</param>
        /// <param name="mode">Normalization format (<see cref="NormalizerMode.NFC"/>,<see cref="NormalizerMode.NFD"/>,
        /// <see cref="NormalizerMode.NFKC"/>,<see cref="NormalizerMode.NFKD"/>).</param>
        /// <returns>Return code to specify if the text is normalized or not
        /// (<see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or <see cref="QuickCheckResult.Maybe"/>)</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static QuickCheckResult QuickCheck(scoped ReadOnlySpan<char> source, NormalizerMode mode)
        {
            return QuickCheck(source, mode, 0);
        }

        /// <summary>
        /// Performing quick check on a string, to quickly determine if the string is
        /// in a particular normalization format.
        /// Three types of result can be returned <see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or
        /// <see cref="QuickCheckResult.Maybe"/>. Result <see cref="QuickCheckResult.Yes"/> indicates that the argument
        /// string is in the desired normalized format, <see cref="QuickCheckResult.No"/> determines that
        /// argument string is not in the desired normalized format. A
        /// <see cref="QuickCheckResult.Maybe"/> result indicates that a more thorough check is required,
        /// the user may have to put the string in its normalized form and compare
        /// the results.
        /// </summary>
        /// <param name="source">String for determining if it is in a normalized format.</param>
        /// <param name="mode">Normalization format (<see cref="NormalizerMode.NFC"/>,<see cref="NormalizerMode.NFD"/>,
        /// <see cref="NormalizerMode.NFKC"/>,<see cref="NormalizerMode.NFKD"/>).</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Return code to specify if the text is normalized or not
        /// (<see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or <see cref="QuickCheckResult.Maybe"/>)</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static QuickCheckResult QuickCheck(string source, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).QuickCheck(source.AsSpan());
        }

        /// <summary>
        /// Performing quick check on a string, to quickly determine if the string is
        /// in a particular normalization format.
        /// Three types of result can be returned <see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or
        /// <see cref="QuickCheckResult.Maybe"/>. Result <see cref="QuickCheckResult.Yes"/> indicates that the argument
        /// string is in the desired normalized format, <see cref="QuickCheckResult.No"/> determines that
        /// argument string is not in the desired normalized format. A
        /// <see cref="QuickCheckResult.Maybe"/> result indicates that a more thorough check is required,
        /// the user may have to put the string in its normalized form and compare
        /// the results.
        /// </summary>
        /// <param name="source">String for determining if it is in a normalized format.</param>
        /// <param name="mode">Normalization format (<see cref="NormalizerMode.NFC"/>,<see cref="NormalizerMode.NFD"/>,
        /// <see cref="NormalizerMode.NFKC"/>,<see cref="NormalizerMode.NFKD"/>).</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Return code to specify if the text is normalized or not
        /// (<see cref="QuickCheckResult.Yes"/>, <see cref="QuickCheckResult.No"/> or <see cref="QuickCheckResult.Maybe"/>)</returns>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static QuickCheckResult QuickCheck(scoped ReadOnlySpan<char> source, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).QuickCheck(source);
        }

        /// <summary>
        /// Test if a string is in a given normalization form.
        /// This is semantically equivalent to source.Equals(Normalize(source, mode)).
        /// Unlike <see cref="QuickCheck(string, NormalizerMode)"/>, this function returns a definitive result,
        /// never a "maybe".
        /// For <see cref="NormalizerMode.NFD"/>, <see cref="NormalizerMode.NFKD"/>, and <see cref="NormalizerMode.FCD"/>, both functions work exactly the same.
        /// For <see cref="NormalizerMode.NFC"/> and<see cref="NormalizerMode.NFKC"/> where <see cref="QuickCheck(string, NormalizerMode)"/> may return "maybe", this function will
        /// perform further tests to arrive at a true/false result.
        /// </summary>
        /// <param name="str">The input string to be checked to see if it is normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Boolean value indicating whether the source string is in the
        /// "<paramref name="mode"/>" normalization form.</returns>
        /// <seealso cref="IsNormalized(ReadOnlySpan{char}, NormalizerMode, NormalizerUnicodeVersion)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(string str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).IsNormalized(str.AsSpan());
        }

        /// <summary>
        /// Test if a string is in a given normalization form.
        /// This is semantically equivalent to source.Equals(Normalize(source, mode)).
        /// Unlike <see cref="QuickCheck(string, NormalizerMode)"/>, this function returns a definitive result,
        /// never a "maybe".
        /// For <see cref="NormalizerMode.NFD"/>, <see cref="NormalizerMode.NFKD"/>, and <see cref="NormalizerMode.FCD"/>, both functions work exactly the same.
        /// For <see cref="NormalizerMode.NFC"/> and<see cref="NormalizerMode.NFKC"/> where <see cref="QuickCheck(string, NormalizerMode)"/> may return "maybe", this function will
        /// perform further tests to arrive at a true/false result.
        /// </summary>
        /// <param name="str">The input string to be checked to see if it is normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Boolean value indicating whether the source string is in the
        /// "<paramref name="mode"/>" normalization form.</returns>
        /// <seealso cref="IsNormalized(ReadOnlySpan{char}, NormalizerMode, NormalizerUnicodeVersion)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(scoped ReadOnlySpan<char> str, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).IsNormalized(str);
        }

        /// <summary>
        /// Convenience Method.
        /// </summary>
        /// <param name="char32">The input code point to be checked to see if it is normalized.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Boolean value indicating whether the source string is in the
        /// "<paramref name="mode"/>" normalization form.</returns>
        /// <seealso cref="IsNormalized(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(int char32, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return IsNormalized(UTF16.ValueOf(char32, stackalloc char[2]), mode, unicodeVersion);
        }

        // ---------------------------------

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// </summary>
        /// <remarks>
        /// Canonical equivalence between two strings is defined as their normalized
        /// forms (<see cref="NormalizerMode.NFD"/> or <see cref="NormalizerMode.NFC"/>) being identical.
        /// This function compares strings incrementally instead of normalizing
        /// (and optionally case-folding) both strings entirely,
        /// improving performance significantly.
        /// <para/>
        /// Bulk normalization is only necessary if the strings do not fulfill the
        /// <see cref="NormalizerMode.FCD"/> conditions. Only in this case, and only if the strings are relatively
        /// long, is memory allocated temporarily.
        /// For <see cref="NormalizerMode.FCD"/> strings and short non-<see cref="NormalizerMode.FCD"/> strings there is no memory allocation.
        /// <para/>
        /// Semantically, this is equivalent to
        ///   strcmp[CodePointOrder](FoldCase(NFD(s1)), FoldCase(NFD(s2)))
        /// where code point order and foldCase are all optional.
        /// </remarks>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(string s1, string s2) // ICU4N specific overload
        {
            return Compare(s1, s2, NormalizerComparison.Default, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// </summary>
        /// <remarks>
        /// Canonical equivalence between two strings is defined as their normalized
        /// forms (<see cref="NormalizerMode.NFD"/> or <see cref="NormalizerMode.NFC"/>) being identical.
        /// This function compares strings incrementally instead of normalizing
        /// (and optionally case-folding) both strings entirely,
        /// improving performance significantly.
        /// <para/>
        /// Bulk normalization is only necessary if the strings do not fulfill the
        /// <see cref="NormalizerMode.FCD"/> conditions. Only in this case, and only if the strings are relatively
        /// long, is memory allocated temporarily.
        /// For <see cref="NormalizerMode.FCD"/> strings and short non-<see cref="NormalizerMode.FCD"/> strings there is no memory allocation.
        /// <para/>
        /// Semantically, this is equivalent to
        ///   strcmp[CodePointOrder](FoldCase(NFD(s1)), FoldCase(NFD(s2)))
        /// where code point order and foldCase are all optional.
        /// </remarks>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(string s1, string s2, NormalizerComparison comparison) // ICU4N specific overload
        {
            return Compare(s1, s2, comparison, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// </summary>
        /// <remarks>
        /// Canonical equivalence between two strings is defined as their normalized
        /// forms (<see cref="NormalizerMode.NFD"/> or <see cref="NormalizerMode.NFC"/>) being identical.
        /// This function compares strings incrementally instead of normalizing
        /// (and optionally case-folding) both strings entirely,
        /// improving performance significantly.
        /// <para/>
        /// Bulk normalization is only necessary if the strings do not fulfill the
        /// <see cref="NormalizerMode.FCD"/> conditions. Only in this case, and only if the strings are relatively
        /// long, is memory allocated temporarily.
        /// For <see cref="NormalizerMode.FCD"/> strings and short non-<see cref="NormalizerMode.FCD"/> strings there is no memory allocation.
        /// <para/>
        /// Semantically, this is equivalent to
        ///   strcmp[CodePointOrder](FoldCase(NFD(s1)), FoldCase(NFD(s2)))
        /// where code point order and foldCase are all optional.
        /// </remarks>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(string s1, string s2, NormalizerComparison comparison, FoldCase foldCase) // ICU4N specific overload
        {
            return Compare(s1, s2, comparison, foldCase, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// </summary>
        /// <remarks>
        /// Canonical equivalence between two strings is defined as their normalized
        /// forms (<see cref="NormalizerMode.NFD"/> or <see cref="NormalizerMode.NFC"/>) being identical.
        /// This function compares strings incrementally instead of normalizing
        /// (and optionally case-folding) both strings entirely,
        /// improving performance significantly.
        /// <para/>
        /// Bulk normalization is only necessary if the strings do not fulfill the
        /// <see cref="NormalizerMode.FCD"/> conditions. Only in this case, and only if the strings are relatively
        /// long, is memory allocated temporarily.
        /// For <see cref="NormalizerMode.FCD"/> strings and short non-<see cref="NormalizerMode.FCD"/> strings there is no memory allocation.
        /// <para/>
        /// Semantically, this is equivalent to
        ///   strcmp[CodePointOrder](FoldCase(NFD(s1)), FoldCase(NFD(s2)))
        /// where code point order and foldCase are all optional.
        /// </remarks>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <stable>ICU 2.8</stable>
        public static int Compare(string s1, string s2, NormalizerComparison comparison, FoldCase foldCase, NormalizerUnicodeVersion unicodeVersion)
        {
            return InternalCompare(s1.AsSpan(), s2.AsSpan(), (int)comparison | (int)foldCase, (int)unicodeVersion);
        }

        // ---------------------------------

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// Convenience method.
        /// </summary>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(scoped ReadOnlySpan<char> s1, scoped ReadOnlySpan<char> s2) // ICU4N specific overload
        {
            return Compare(s1, s2, NormalizerComparison.Default, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// Convenience method.
        /// </summary>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(scoped ReadOnlySpan<char> s1, scoped ReadOnlySpan<char> s2, NormalizerComparison comparison) // ICU4N specific overload
        {
            return Compare(s1, s2, comparison, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// Convenience method.
        /// </summary>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(scoped ReadOnlySpan<char> s1, scoped ReadOnlySpan<char> s2, NormalizerComparison comparison, FoldCase foldCase) // ICU4N specific overload
        {
            return Compare(s1, s2, comparison, foldCase, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Compare two strings for canonical equivalence.
        /// Further options include case-insensitive comparison and
        /// code point order (as opposed to code unit order).
        /// Convenience method.
        /// </summary>
        /// <param name="s1">First source string.</param>
        /// <param name="s2">Second source string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// <list type="table">
        ///     <item><term><see cref="NormalizerComparison.InputIsFCD"/></term><description>
        ///         Set if the caller knows that both <paramref name="s1"/> and <paramref name="s2"/> fulfill the FCD
        ///         conditions. If not set, the function will quickCheck for FCD
        ///         and normalize if necessary.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.CodePointOrder"/></term><description>
        ///         Set to choose code point order instead of code unit order.
        ///     </description></item>
        ///     <item><term><see cref="NormalizerComparison.IgnoreCase"/></term><description>
        ///         Set to compare strings case-insensitively using case folding,
        ///         instead of case-sensitively.
        ///         If set, then the following case folding options are used.
        ///     </description></item>
        /// </list>
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>&lt;0 or 0 or &gt;0 as usual for string comparisons.</returns>
        /// <seealso cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <seealso cref="NormalizerMode.FCD"/>
        /// <stable>ICU 2.8</stable>
        public static int Compare(scoped ReadOnlySpan<char> s1, scoped ReadOnlySpan<char> s2, NormalizerComparison comparison, FoldCase foldCase, NormalizerUnicodeVersion unicodeVersion)
        {
            return InternalCompare(s1, s2, (int)comparison | (int)foldCase, (int)unicodeVersion);
        }

        // ---------------------------------

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="char32b">The second code point.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, int char32b) // ICU4N specific overload
        {
            return Compare(char32a, char32b, NormalizerComparison.Default, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="char32b">The second code point.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, int char32b, NormalizerComparison comparison) // ICU4N specific overload
        {
            return Compare(char32a, char32b, comparison, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="char32b">The second code point.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, int char32b, NormalizerComparison comparison, FoldCase foldCase) // ICU4N specific overload
        {
            return Compare(char32a, char32b, comparison, foldCase, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="char32b">The second code point.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <stable>ICU 2.8</stable>
        public static int Compare(int char32a, int char32b, NormalizerComparison comparison, FoldCase foldCase, NormalizerUnicodeVersion unicodeVersion)
        {
            return InternalCompare(UTF16.ValueOf(char32a, stackalloc char[2]), UTF16.ValueOf(char32b, stackalloc char[2]), (int)comparison | (int)foldCase | INPUT_IS_FCD, (int)unicodeVersion);
        }

        // ---------------------------------

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, string str2) // ICU4N specific overload
        {
            return Compare(char32a, str2, NormalizerComparison.Default, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, string str2, NormalizerComparison comparison) // ICU4N specific overload
        {
            return Compare(char32a, str2, comparison, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, string str2, NormalizerComparison comparison, FoldCase foldCase) // ICU4N specific overload
        {
            return Compare(char32a, str2, comparison, foldCase, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <stable>ICU 2.8</stable>
        public static int Compare(int char32a, string str2, NormalizerComparison comparison, FoldCase foldCase, NormalizerUnicodeVersion unicodeVersion)
        {
            return InternalCompare(UTF16.ValueOf(char32a, stackalloc char[2]), str2.AsSpan(), (int)comparison | (int)foldCase, (int)unicodeVersion);
        }

        // ---------------------------------

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, scoped ReadOnlySpan<char> str2) // ICU4N specific overload
        {
            return Compare(char32a, str2, NormalizerComparison.Default, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, scoped ReadOnlySpan<char> str2, NormalizerComparison comparison) // ICU4N specific overload
        {
            return Compare(char32a, str2, comparison, FoldCase.Default, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        public static int Compare(int char32a, scoped ReadOnlySpan<char> str2, NormalizerComparison comparison, FoldCase foldCase) // ICU4N specific overload
        {
            return Compare(char32a, str2, comparison, foldCase, NormalizerUnicodeVersion.Default);
        }

        /// <summary>
        /// Convenience method that can have faster implementation
        /// by not allocating buffers.
        /// </summary>
        /// <param name="char32a">The first code point to be checked against the.</param>
        /// <param name="str2">The second string.</param>
        /// <param name="comparison"><see cref="NormalizerComparison"/> flags to control the text comparison.
        /// </param>
        /// <param name="foldCase"><see cref="FoldCase"/> option, such as 
        /// <see cref="FoldCase.ExcludeSpecialI"/> or <see cref="FoldCase.Default"/>.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <stable>ICU 2.8</stable>
        public static int Compare(int char32a, scoped ReadOnlySpan<char> str2, NormalizerComparison comparison, FoldCase foldCase, NormalizerUnicodeVersion unicodeVersion)
        {
            return InternalCompare(UTF16.ValueOf(char32a, stackalloc char[2]), str2, (int)comparison | (int)foldCase, (int)unicodeVersion);
        }

        /* Concatenation of normalized strings --------------------------------- */

        /// <summary>
        /// Concatenate normalized strings, making sure that the result is normalized
        /// as well.
        /// </summary>
        /// <remarks>
        /// If both the left and the right strings are in
        /// the normalization form according to "mode",
        /// then the result will be
        ///
        /// <code>
        ///     destination=Normalize(left+right, mode)
        /// </code>
        ///
        /// <para/>
        /// It is allowed to have destination==left to avoid copying the entire left string.
        /// </remarks>
        /// <param name="left">Left source string, may be same as <paramref name="destination"/>.</param>
        /// <param name="right">Right source string.</param>
        /// <param name="destination">The output buffer.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool TryConcat(ReadOnlySpan<char> left, ReadOnlySpan<char> right, Span<char> destination, out int charsLength, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).TryConcat(left, right, destination, out charsLength);
        }

        /// <summary>
        /// Concatenate normalized strings, making sure that the result is normalized
        /// as well.
        /// </summary>
        /// <remarks>
        /// If both the left and the right strings are in
        /// the normalization form according to "mode",
        /// then the result will be
        ///
        /// <code>
        ///     dest=Normalize(left+right, mode)
        /// </code>
        /// 
        /// <para/>
        /// For details see <see cref="TryConcat(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int, NormalizerMode, NormalizerUnicodeVersion)"/>.
        /// </remarks>
        /// <param name="left">Left source string.</param>
        /// <param name="right">Right source string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Result.</returns>
        /// <exception cref="IndexOutOfRangeException">If target capacity is less than the required length.</exception>
        /// <see cref="TryConcat(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <see cref="Next()"/>
        /// <see cref="Previous()"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Concat(ReadOnlySpan<char> left, ReadOnlySpan<char> right, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            int length = left.Length + right.Length + 16;
            ValueStringBuilder dest = length < CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);
            try
            {
                dest.Append(left);
                GetModeInstance(mode).GetNormalizer2((int)unicodeVersion).Append(ref dest, right);
                return dest.ToString();
            }
            finally
            {
                dest.Dispose();
            }
        }

        /// <summary>
        /// Concatenate normalized strings, making sure that the result is normalized
        /// as well.
        /// </summary>
        /// <remarks>
        /// If both the left and the right strings are in
        /// the normalization form according to "mode",
        /// then the result will be
        ///
        /// <code>
        ///     dest=Normalize(left+right, mode)
        /// </code>
        ///
        /// With the input strings already being normalized,
        /// this function will use <see cref="Next()"/> and <see cref="Previous()"/>
        /// to find the adjacent end pieces of the input strings.
        /// Only the concatenation of these end pieces will be normalized and
        /// then concatenated with the remaining parts of the input strings.
        /// </remarks>
        /// <param name="left">Left source string.</param>
        /// <param name="right">Right source string.</param>
        /// <param name="mode">The normalization mode.</param>
        /// <param name="unicodeVersion">The Unicode version to use.
        /// Currently the only available option is <see cref="NormalizerUnicodeVersion.Unicode3_2"/>.
        /// If you want the default behavior corresponding to one of the
        /// standard Unicode Normalization Forms, use <see cref="NormalizerUnicodeVersion.Default"/> for this argument.
        /// </param>
        /// <returns>Result.</returns>
        /// <exception cref="IndexOutOfRangeException">If target capacity is less than the required length.</exception>
        /// <see cref="TryConcat(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <see cref="Normalize(string, NormalizerMode, NormalizerUnicodeVersion)"/>
        /// <see cref="Next()"/>
        /// <see cref="Previous()"/>
        /// <see cref="Concat(ReadOnlySpan{Char}, ReadOnlySpan{Char}, NormalizerMode, NormalizerUnicodeVersion)"/>
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Concat(string left, string right, NormalizerMode mode, NormalizerUnicodeVersion unicodeVersion)
        {
            return Concat(left.AsSpan(), right.AsSpan(), mode, unicodeVersion);
        }

        /// <summary>
        /// Gets the FC_NFKC closure value.
        /// </summary>
        /// <param name="c">The code point whose closure value is to be retrieved.</param>
        /// <param name="dest">The char array to receive the closure value.</param>
        /// <returns>The length of the closure value; 0 if there is none.</returns>
        [Obsolete("ICU 56")]
        public static int GetFC_NFKC_Closure(int c, char[] dest)
        {
            string closure = GetFC_NFKC_Closure(c);
            int length = closure.Length;
            if (length != 0 && dest != null && length <= dest.Length)
            {
                //closure.getChars(0, length, dest, 0);
                closure.CopyTo(0, dest, 0, length);
            }
            return length;
        }

        /// <summary>
        /// Gets the FC_NFKC closure value.
        /// </summary>
        /// <param name="c">The code point whose closure value is to be retrieved.</param>
        /// <returns>String representation of the closure value; "" if there is none.</returns>
        [Obsolete("ICU 56")]
        public static string GetFC_NFKC_Closure(int c)
        {
            // Compute the FC_NFKC_Closure on the fly:
            // We have the API for complete coverage of Unicode properties, although
            // this value by itself is not useful via API.
            // (What could be useful is a custom normalization table that combines
            // case folding and NFKC.)
            // For the derivation, see Unicode's DerivedNormalizationProps.txt.
            Normalizer2 nfkc = NFKCModeImpl.Instance.Normalizer2;
            UCaseProperties csp = UCaseProperties.Instance;
            // first: b = NFKC(Fold(a))
            ValueStringBuilder folded = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int folded1Length = csp.ToFullFolding(c, ref folded, 0);
                if (folded1Length < 0)
                {
                    Normalizer2Impl nfkcImpl = ((Normalizer2WithImpl)nfkc).Impl;
                    if (nfkcImpl.GetCompQuickCheck(nfkcImpl.GetNorm16(c)) != 0)
                    {
                        return "";  // c does not change at all under CaseFolding+NFKC
                    }
                    folded.AppendCodePoint(c);
                }
                else
                {
                    if (folded1Length > UCaseProperties.MaxStringLength)
                    {
                        folded.AppendCodePoint(folded1Length);
                    }
                }
                string kc1 = nfkc.Normalize(folded.AsSpan());
                // second: c = NFKC(Fold(b))
                string kc2 = nfkc.Normalize(UChar.FoldCase(kc1, 0));
                // if (c != b) add the mapping from a to c
                if (kc1.Equals(kc2))
                {
                    return "";
                }
                else
                {
                    return kc2;
                }
            }
            finally
            {
                folded.Dispose();
            }
        }

        //-------------------------------------------------------------------------
        // Iteration API
        //-------------------------------------------------------------------------

        /// <summary>
        /// Gets the current character in the normalized text.
        /// Returns the codepoint as an int.
        /// </summary>
        [Obsolete("ICU 56")]
        public int Current
        {
            get
            {
                if (bufferPos < buffer.Length || NextNormalize())
                {
                    return buffer.CodePointAt(bufferPos);
                }
                else
                {
                    return Done;
                }
            }
        }

        /// <summary>
        /// Return the next character in the normalized text and advance
        /// the iteration position by one.  If the end
        /// of the text has already been reached, <see cref="Done"/> is returned.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        [Obsolete("ICU 56")]
        public int Next()
        {
            if (bufferPos < buffer.Length || NextNormalize())
            {
                int c = buffer.CodePointAt(bufferPos);
                bufferPos += Character.CharCount(c);
                return c;
            }
            else
            {
                return Done;
            }
        }

        /// <summary>
        /// Return the previous character in the normalized text and decrement
        /// the iteration position by one.  If the beginning
        /// of the text has already been reached, <see cref="Done"/> is returned.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        [Obsolete("ICU 56")]
        public int Previous()
        {
            if (bufferPos > 0 || PreviousNormalize())
            {
                int c = buffer.CodePointBefore(bufferPos);
                bufferPos -= Character.CharCount(c);
                return c;
            }
            else
            {
                return Done;
            }
        }

        /// <summary>
        /// Reset the index to the beginning of the text.
        /// This is equivalent to <c>SetIndexOnly(startIndex)</c>.
        /// </summary>
        [Obsolete("ICU 56")]
        public void Reset()
        {
            text.SetToStart();
            currentIndex = nextIndex = 0;
            ClearBuffer();
        }

        /// <summary>
        /// Set the iteration position in the input text that is being normalized,
        /// without any immediate normalization.
        /// After <see cref="SetIndexOnly(int)"/>, <see cref="Index"/> will return the same index that is
        /// specified here.
        /// </summary>
        /// <param name="index">The desired index in the input text.</param>
        [Obsolete("ICU 56")]
        public void SetIndexOnly(int index)
        {
            text.Index = index;  // validates index
            currentIndex = nextIndex = index;
            ClearBuffer();
        }

        /// <summary>
        /// Set the iteration position in the input text that is being normalized
        /// and return the first normalized character at that position.
        /// <para/>
        /// <b>Note:</b> This method sets the position in the <em>input</em> text,
        /// while <see cref="Next()"/> and <see cref="Previous()"/> iterate through characters
        /// in the normalized <em>output</em>.  This means that there is not
        /// necessarily a one-to-one correspondence between characters returned
        /// by see <see cref="Next()"/> and <see cref="Previous()"/> and the indices passed to and
        /// returned from <see cref="SetIndex(int)"/> and <see cref="Index"/>.
        /// </summary>
        /// <param name="index">The desired index in the input text.</param>
        /// <returns>The first normalized character that is the result of iterating
        /// forward starting at the given index.</returns>
        /// <exception cref="ArgumentException">if the given index is less than
        /// <see cref="GetBeginIndex()"/> or greater than <see cref="GetEndIndex()"/>.</exception>
        [Obsolete("ICU 3.2")]
        //CLOVER:OFF
        public int SetIndex(int index)
        {
            SetIndexOnly(index);
            return Current;
        }
        //CLOVER:ON

        /// <summary>
        /// Retrieve the index of the start of the input text. This is the begin
        /// index of the <see cref="CharacterIterator"/> or the start (i.e. 0) of the
        /// <see cref="string"/> over which this <see cref="Normalizer"/> is iterating.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        /// <seealso cref="StartIndex"/>
        [Obsolete("ICU 2.2. Use StartIndex instead.")]
        internal int GetBeginIndex() // ICU4N specific - marked internal instead of public, since the functionality is obsolete and duplicated
        {
            return 0;
        }

        /// <summary>
        /// Retrieve the index of the end of the input text.  This is the end index
        /// of the <see cref="CharacterIterator"/> or the length of the <see cref="string"/>
        /// over which this <see cref="Normalizer"/> is iterating.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        /// <seealso cref="EndIndex"/>
        [Obsolete("ICU 2.2. Use EndIndex instead.")]
        internal int GetEndIndex() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete and duplicated
        {
            return EndIndex;
        }

        /// <summary>
        /// Return the first character in the normalized text.  This resets
        /// the <see cref="Normalizer"/>'s position to the beginning of the text.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        [Obsolete("ICU 56")]
        public int First()
        {
            Reset();
            return Next();
        }

        /// <summary>
        /// Return the last character in the normalized text.  This resets
        /// the <see cref="Normalizer"/>'s position to be just before the
        /// the input text corresponding to that normalized character.
        /// </summary>
        /// <returns>The codepoint as an int.</returns>
        [Obsolete("ICU 56")]
        public int Last()
        {
            text.SetToLimit();
            currentIndex = nextIndex = text.Index;
            ClearBuffer();
            return Previous();
        }

        /// <summary>
        /// Retrieve the current iteration position in the input text that is
        /// being normalized.  This method is useful in applications such as
        /// searching, where you need to be able to determine the position in
        /// the input text that corresponds to a given normalized output character.
        /// <para/>
        /// <b>Note:</b> This method sets the position in the <em>input</em>, while
        /// <see cref="Next()"/> and <see cref="Previous()"/> iterate through characters in the
        /// <em>output</em>.  This means that there is not necessarily a one-to-one
        /// correspondence between characters returned by <see cref="Next()"/> and <see cref="Previous()"/>
        /// and the indices passed to and returned from <see cref="SetIndex(int)"/> and <see cref="Index"/>.
        /// </summary>
        /// <returns>The current iteration position.</returns>
        [Obsolete("ICU 56")]
        public int Index
        {
            get
            {
                if (bufferPos < buffer.Length)
                {
                    return currentIndex;
                }
                else
                {
                    return nextIndex;
                }
            }
        }

        /// <summary>
        /// Retrieve the index of the start of the input text. This is the begin
        /// index of the <see cref="CharacterIterator"/> or the start (i.e. 0) of the
        /// <see cref="string"/> over which this <see cref="Normalizer"/> is iterating.
        /// </summary>
        /// <returns>The current iteration position.</returns>
        [Obsolete("ICU 56")]
        public int StartIndex => 0;

        /// <summary>
        /// Retrieve the index of the end of the input text.  This is the end index
        /// of the <see cref="CharacterIterator"/> or the length of the <see cref="string"/>
        /// over which this <see cref="Normalizer"/> is iterating.
        /// </summary>
        /// <returns>The current iteration position.</returns>
        [Obsolete("ICU 56")]
        public int EndIndex => text.Length;

        //-------------------------------------------------------------------------
        // Iterator attributes
        //-------------------------------------------------------------------------

        /// <summary>
        /// Set the normalization mode for this object.
        /// <para/>
        /// <b>Note:</b>If the normalization mode is changed while iterating
        /// over a string, calls to <see cref="Next()"/> and <see cref="Previous()"/> may
        /// return previously buffers characters in the old normalization mode
        /// until the iteration is able to re-sync at the next base character.
        /// It is safest to call <see cref="SetText(string)"/>, <see cref="First()"/>,
        /// <see cref="Last()"/>, etc. after calling <see cref="SetMode(NormalizerMode)"/>.
        /// </summary>
        /// <param name="newMode">The new mode for this <see cref="Normalizer"/>.
        /// The supported modes are:
        /// <list type="table">
        ///     <item><term><see cref="NormalizerMode.NFC"/></term><description>Unicode canonical decompositiion followed by canonical composition.</description></item>
        ///     <item><term><see cref="NormalizerMode.NFKC"/></term><description>Unicode compatibility decompositiion follwed by canonical composition.</description></item>
        ///     <item><term><see cref="NormalizerMode.NFD"/></term><description>Unicode canonical decomposition.</description></item>
        ///     <item><term><see cref="NormalizerMode.NFKD"/></term><description>Unicode compatibility decomposition.</description></item>
        ///     <item><term><see cref="NormalizerMode.None"/></term><description>Do nothing but return characters from the underlying input text.</description></item>
        /// </list>
        /// </param>
        /// <see cref="GetMode()"/>
        [Obsolete("ICU 56")]
        public void SetMode(NormalizerMode newMode)
        {
            mode = GetModeInstance(newMode);
            norm2 = mode.GetNormalizer2(this.options);
        }

        /// <summary>
        /// Return the basic operation performed by this <see cref="Normalizer"/>.
        /// </summary>
        /// <seealso cref="SetMode(NormalizerMode)"/>
        [Obsolete("ICU 56")]
        public NormalizerMode GetMode() // ICU4N: Property would conflict with the nested Mode class, and both are obsolete anyway
        {
            return mode.NormalizerMode;
        }

        [Obsolete("ICU 56")]
        private static Mode GetModeInstance(NormalizerMode mode)
        {
            switch (mode)
            {
                case NormalizerMode.None:
                    return NONE;
                case NormalizerMode.NFD:
                    return NFD;
                case NormalizerMode.NFKD:
                    return NFKD;
                case NormalizerMode.NFC:
                    return NFC;
                case NormalizerMode.NFKC:
                    return NFKC;
                case NormalizerMode.FCD:
                    return FCD;
                case NormalizerMode.Default:
                    return NFC;
                default:
                    return NFC;
            }
        }

        /// <summary>
        /// Set options that affect this <tt>Normalizer</tt>'s operation.
        /// Options do not change the basic composition or decomposition operation
        /// that is being performed , but they control whether
        /// certain optional portions of the operation are done.
        /// Currently the only available option is:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="NormalizerUnicodeVersion.Unicode3_2"/></term>
        ///         <description>Use Normalization conforming to Unicode version 3.2.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="option">The option whose value is to be set.</param>
        /// <param name="value">the new setting for the option.  Use <c>true</c> to
        /// turn the option on and <c>false</c> to turn it off.</param>
        /// <seealso cref="GetOption(int)"/>
        /// <seealso cref="UnicodeVersion"/>
        [Obsolete("ICU 56")]
        internal void SetOption(int option, bool value) // ICU4N specific - retained this method for testing purposes, but made it internal
        {
            if (value)
            {
                options |= option;
            }
            else
            {
                options &= (~option);
            }
            norm2 = mode.GetNormalizer2(options);
        }

        /// <summary>
        /// Determine whether an option is turned on or off.
        /// </summary>
        /// <seealso cref="SetOption(int, bool)"/>
        /// <seealso cref="UnicodeVersion"/>
        [Obsolete("ICU 56")]
        internal int GetOption(int option) // ICU4N specific - retained this method for testing purposes, but made it internal
        {
            if ((this.options & option) != 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets whether to use Normalization conforming to Unicode version 3.2
        /// or default behavior corresponding to one of the standard Unicode Normalization Forms
        /// </summary>
        [Obsolete("ICU 56")]
        public NormalizerUnicodeVersion UnicodeVersion // ICU4N specific - converted GetOption and SetOption methods to a property that sets the one possible option. If options that belong to other enums were possible, we would add a similar property for each enum.
        {
            get => this.options.AsFlagsToEnum<NormalizerUnicodeVersion>();
            set
            {
                // Only one version allowed at a time - clear all previously selected versions
                foreach (int option in Enum.GetValues(typeof(NormalizerUnicodeVersion)))
                    this.options &= (~option);
                this.options |= (int)value;
                norm2 = mode.GetNormalizer2(this.options);
            }
        }

        /// <summary>
        /// Gets the underlying text storage.
        /// </summary>
        /// <param name="destination">The char buffer to fill the UTF-16 units.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public bool TryGetText(Span<char> destination, out int charsLength)
            => text.TryGetText(destination, out charsLength);

        /// <summary>
        /// Gets the length of underlying text storage.
        /// </summary>
        [Obsolete("ICU 56")]
        public int Length => text.Length;

        /// <summary>
        /// Returns the text under iteration as a string.
        /// </summary>
        /// <returns>A copy of the text under iteration.</returns>
        [Obsolete("ICU 56")]
        public string GetText()
        {
            return text.GetText();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the input text.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(StringBuffer newText) // ICU4N TODO: API - Factor out
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the input text.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(char[] newText) // ICU4N TODO: API - Factor out
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the input text.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(string newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the input text.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(ReadOnlySpan<char> newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the input text.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(CharacterIterator newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        /// <summary>
        /// Set the input text over which this <see cref="Normalizer"/> will iterate.
        /// The iteration position is set to the beginning of the string.
        /// </summary>
        /// <param name="newText">The new string to be normalized.</param>
        [Obsolete("ICU 56")]
        public void SetText(UCharacterIterator newText)
        {
            UCharacterIterator newIter = (UCharacterIterator)newText.Clone();
            text = newIter ?? throw new InvalidOperationException("Could not create a new UCharacterIterator");
            Reset();
        }

        private void ClearBuffer()
        {
            buffer.Length = 0;
            bufferPos = 0;
        }

        private bool NextNormalize()
        {
            ClearBuffer();
            currentIndex = nextIndex;
            text.Index = nextIndex;
            // Skip at least one character so we make progress.
            int c = text.NextCodePoint();
            if (c < 0)
            {
                return false;
            }
            using ValueStringBuilder segment = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            segment.AppendCodePoint(c);
            while ((c = text.NextCodePoint()) >= 0)
            {
                if (norm2.HasBoundaryBefore(c))
                {
                    text.MoveCodePointIndex(-1);
                    break;
                }
                segment.AppendCodePoint(c);
            }
            nextIndex = text.Index;
            norm2.Normalize(segment.AsSpan(), buffer);
            return buffer.Length != 0;
        }

        private bool PreviousNormalize()
        {
            ClearBuffer();
            nextIndex = currentIndex;
            text.Index = currentIndex;
            using ValueStringBuilder segment = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            int c;
            while ((c = text.PreviousCodePoint()) >= 0)
            {
                if (c <= 0xffff)
                {
                    segment.Insert(0, (char)c);
                }
                else
                {
                    segment.InsertCodePoint(0, c); // ICU4N: Optimized insertion of code point without allocating
                }
                if (norm2.HasBoundaryBefore(c))
                {
                    break;
                }
            }
            currentIndex = text.Index;
            norm2.Normalize(segment.AsSpan(), buffer);
            bufferPos = buffer.Length;
            return buffer.Length != 0;
        }

        /* compare canonically equivalent ------------------------------------------- */

        // TODO: Broaden the public compare(string, string, options) API like this. Ticket #7407
        private static int InternalCompare(scoped ReadOnlySpan<char> s1, scoped ReadOnlySpan<char> s2, int options, int normOptions)
        {
//#pragma warning disable 612, 618
//            int normOptions = options.TripleShift(COMPARE_NORM_OPTIONS_SHIFT);
//#pragma warning restore 612, 618
            options |= COMPARE_EQUIV;

            /*
             * UAX #21 Case Mappings, as fixed for Unicode version 4
             * (see Jitterbug 2021), defines a canonical caseless match as
             *
             * A string X is a canonical caseless match
             * for a string Y if and only if
             * NFD(toCasefold(NFD(X))) = NFD(toCasefold(NFD(Y)))
             *
             * For better performance, we check for FCD (or let the caller tell us that
             * both strings are in FCD) for the inner normalization.
             * BasicNormalizerTest::FindFoldFCDExceptions() makes sure that
             * case-folding preserves the FCD-ness of a string.
             * The outer normalization is then only performed by NormalizerImpl.cmpEquivFold()
             * when there is a difference.
             *
             * Exception: When using the Turkic case-folding option, we do perform
             * full NFD first. This is because in the Turkic case precomposed characters
             * with 0049 capital I or 0069 small i fold differently whether they
             * are first decomposed or not, so an FCD check - a check only for
             * canonical order - is not sufficient.
             */
            if ((options & INPUT_IS_FCD) == 0 || (options & FOLD_CASE_EXCLUDE_SPECIAL_I) != 0)
            {
                Normalizer2 n2;
                if ((options & FOLD_CASE_EXCLUDE_SPECIAL_I) != 0)
                {
#pragma warning disable 612, 618
                    n2 = NFD.GetNormalizer2(normOptions);
                }
                else
                {
                    n2 = FCD.GetNormalizer2(normOptions);
#pragma warning restore 612, 618
                }

                // check if s1 and/or s2 fulfill the FCD conditions
                int spanQCYes1 = n2.SpanQuickCheckYes(s1);
                int spanQCYes2 = n2.SpanQuickCheckYes(s2);

                /*
                 * ICU 2.4 had a further optimization:
                 * If both strings were not in FCD, then they were both NFD'ed,
                 * and the COMPARE_EQUIV option was turned off.
                 * It is not entirely clear that this is valid with the current
                 * definition of the canonical caseless match.
                 * Therefore, ICU 2.6 removes that optimization.
                 */


                bool s1Normalize = spanQCYes1 < s1.Length;
                bool s2Normalize = spanQCYes2 < s2.Length;
                int s1BufferLength = s1.Length + 16;
                int s2BufferLength = s2.Length + 16;

                // ICU4N: We need to declare these at the same stack level so they remain in
                // scope for the duration of the CmpEquivFold call.
                ValueStringBuilder fcd1 = !s1Normalize ? default :
                    (s1BufferLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                        : new ValueStringBuilder(s1BufferLength));
                ValueStringBuilder fcd2 = !s2Normalize ? default :
                    (s2BufferLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                        : new ValueStringBuilder(s2BufferLength));

                try
                {
                    if (s1Normalize)
                    {
                        fcd1.Append(s1.Slice(0, spanQCYes1 - 0)); // ICU4N: Checked 3rd parameter math
                        n2.NormalizeSecondAndAppend(ref fcd1, s1.Slice(spanQCYes1, s1.Length - spanQCYes1)); // ICU4N: Checked 2nd parameter math
                                                                                                             //s1String = fcd1.ToString();
                    }
                    if (s2Normalize)
                    {
                        fcd2.Append(s2.Slice(0, spanQCYes2 - 0)); // ICU4N: Checked 3rd parameter math
                        n2.NormalizeSecondAndAppend(ref fcd2, s2.Slice(spanQCYes2, s2.Length - spanQCYes2)); // ICU4N: Checked 2nd parameter math
                                                                                                             //s2String = fcd2.ToString();
                    }

                    return CmpEquivFold(
                        s1Normalize ? fcd1.AsSpan() : s1,
                        s2Normalize ? fcd2.AsSpan() : s2,
                        options);
                }
                finally
                {
                    fcd1.Dispose();
                    fcd2.Dispose();
                }
            }

            return CmpEquivFold(s1, s2, options);
        }

        /*
         * Compare two strings for canonical equivalence.
         * Further options include case-insensitive comparison and
         * code point order (as opposed to code unit order).
         *
         * In this function, canonical equivalence is optional as well.
         * If canonical equivalence is tested, then both strings must fulfill
         * the FCD check.
         *
         * Semantically, this is equivalent to
         *   strcmp[CodePointOrder](NFD(foldCase(s1)), NFD(foldCase(s2)))
         * where code point order, NFD and foldCase are all optional.
         *
         * string comparisons almost always yield results before processing both strings
         * completely.
         * They are generally more efficient working incrementally instead of
         * performing the sub-processing (strlen, normalization, case-folding)
         * on the entire strings first.
         *
         * It is also unnecessary to not normalize identical characters.
         *
         * This function works in principle as follows:
         *
         * loop {
         *   get one code unit c1 from s1 (-1 if end of source)
         *   get one code unit c2 from s2 (-1 if end of source)
         *
         *   if(either string finished) {
         *     return result;
         *   }
         *   if(c1==c2) {
         *     continue;
         *   }
         *
         *   // c1!=c2
         *   try to decompose/case-fold c1/c2, and continue if one does;
         *
         *   // still c1!=c2 and neither decomposes/case-folds, return result
         *   return c1-c2;
         * }
         *
         * When a character decomposes, then the pointer for that source changes to
         * the decomposition, pushing the previous pointer onto a stack.
         * When the end of the decomposition is reached, then the code unit reader
         * pops the previous source from the stack.
         * (Same for case-folding.)
         *
         * This is complicated further by operating on variable-width UTF-16.
         * The top part of the loop works on code units, while lookups for decomposition
         * and case-folding need code points.
         * Code points are assembled after the equality/end-of-source part.
         * The source pointer is only advanced beyond all code units when the code point
         * actually decomposes/case-folds.
         *
         * If we were on a trail surrogate unit when assembling a code point,
         * and the code point decomposes/case-folds, then the decomposition/folding
         * result must be compared with the part of the other string that corresponds to
         * this string's lead surrogate.
         * Since we only assemble a code point when hitting a trail unit when the
         * preceding lead units were identical, we back up the other string by one unit
         * in such a case.
         *
         * The optional code point order comparison at the end works with
         * the same fix-up as the other code point order comparison functions.
         * See ustring.c and the comment near the end of this function.
         *
         * Assumption: A decomposition or case-folding result string never contains
         * a single surrogate. This is a safe assumption in the Unicode Standard.
         * Therefore, we do not need to check for surrogate pairs across
         * decomposition/case-folding boundaries.
         *
         * Further assumptions (see verifications tstnorm.cpp):
         * The API function checks for FCD first, while the core function
         * first case-folds and then decomposes. This requires that case-folding does not
         * un-FCD any strings.
         *
         * The API function may also NFD the input and turn off decomposition.
         * This requires that case-folding does not un-NFD strings either.
         *
         * TODO If any of the above two assumptions is violated,
         * then this entire code must be re-thought.
         * If this happens, then a simple solution is to case-fold both strings up front
         * and to turn off UNORM_INPUT_IS_FCD.
         * We already do this when not both strings are in FCD because makeFCD
         * would be a partial NFD before the case folding, which does not work.
         * Note that all of this is only a problem when case-folding _and_
         * canonical equivalence come together.
         * (Comments in unorm_compare() are more up to date than this TODO.)
         */

        // ICU4N: Refactored the "stack" to use ref structs
        /* stack element for previous-level source/decomposition pointers */
        private unsafe ref struct CmpEquivLevel
        {
            public CmpEquivLevel()
            {
                Cs = default;
                S = default;
            }

            public ReadOnlySpan<char> Cs { get; set; }
            public int S { get; set; }
        }

        private unsafe ref struct StackContainer
        {
            private CmpEquivLevel level0;
            private CmpEquivLevel level1;

            public StackContainer()
            {
                level0 = new CmpEquivLevel();
                level1 = new CmpEquivLevel();
            }

            public ref CmpEquivLevel this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
                            return ref level0;
                        case 1:
                            return ref level1;
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
            }
        }

        /// <summary>
        /// Internal option for unorm_cmpEquivFold() for decomposing.
        /// If not set, just do strcasecmp().
        /// </summary>
        private const int COMPARE_EQUIV = 0x80000;

        /// <summary>
        /// internal function; package visibility for use by <see cref="UTF16.StringComparer"/>
        /// </summary>
        internal static int CmpEquivFold(ReadOnlySpan<char> cs1, ReadOnlySpan<char> cs2, int options)
        {
            Normalizer2Impl nfcImpl;
            UCaseProperties csp;

            /* current-level start/limit - s1/s2 as current */
            int s1, s2, limit1, limit2;

            /* decomposition and case folding variables */
            int length;

            /* stacks of previous-level start/current/limit */
            StackContainer stack1 = new StackContainer();
            StackContainer stack2 = new StackContainer();

            /* buffers for algorithmic decompositions */
            const int DecompositionCharStackBufferSize = 16; // maximum length of 6, but need to be safe because it will fail if not enough
            Span<char> decomp1 = stackalloc char[DecompositionCharStackBufferSize];
            Span<char> decomp2 = stackalloc char[DecompositionCharStackBufferSize];

            /* case folding buffers, only use current-level start/limit */
            const int FoldCharStackBufferSize = 8; // maximum length of 3
            var fold1 = new ValueStringBuilder(stackalloc char[FoldCharStackBufferSize]);
            var fold2 = new ValueStringBuilder(stackalloc char[FoldCharStackBufferSize]);
            try
            {

                /* track which is the current level per string */
                int level1, level2;

                /* current code units, and code points for lookups */
                int c1, c2, cp1, cp2;

                /* no argument error checking because this itself is not an API */

                /*
                 * assume that at least one of the options _COMPARE_EQUIV and U_COMPARE_IGNORE_CASE is set
                 * otherwise this function must behave exactly as uprv_strCompare()
                 * not checking for that here makes testing this function easier
                 */

                /* normalization/properties data loaded? */
                if ((options & COMPARE_EQUIV) != 0)
                {
                    nfcImpl = Norm2AllModes.NFCInstance.Impl;
                }
                else
                {
                    nfcImpl = null;
                }
                if ((options & COMPARE_IGNORE_CASE) != 0)
                {
                    csp = UCaseProperties.Instance;
                    // ICU4N: fold1, fold2 instantiated on stack above
                }
                else
                {
                    csp = null;
                }

                /* initialize */
                s1 = 0;
                limit1 = cs1.Length;
                s2 = 0;
                limit2 = cs2.Length;

                level1 = level2 = 0;
                c1 = c2 = -1;

                /* comparison loop */
                for (; ; )
                {
                    /*
                     * here a code unit value of -1 means "get another code unit"
                     * below it will mean "this source is finished"
                     */

                    if (c1 < 0)
                    {
                        /* get next code unit from string 1, post-increment */
                        for (; ; )
                        {
                            if (s1 == limit1)
                            {
                                if (level1 == 0)
                                {
                                    c1 = -1;
                                    break;
                                }
                            }
                            else
                            {
                                c1 = cs1[s1++];
                                break;
                            }

                            /* reached end of level buffer, pop one level */
                            do
                            {
                                --level1;
                                cs1 = stack1[level1].Cs;
                            } while (cs1 == null);
                            s1 = stack1[level1].S;
                            limit1 = cs1.Length;
                        }
                    }

                    if (c2 < 0)
                    {
                        /* get next code unit from string 2, post-increment */
                        for (; ; )
                        {
                            if (s2 == limit2)
                            {
                                if (level2 == 0)
                                {
                                    c2 = -1;
                                    break;
                                }
                            }
                            else
                            {
                                c2 = cs2[s2++];
                                break;
                            }

                            /* reached end of level buffer, pop one level */
                            do
                            {
                                --level2;
                                cs2 = stack2[level2].Cs;
                            } while (cs2 == null);
                            s2 = stack2[level2].S;
                            limit2 = cs2.Length;
                        }
                    }

                    /*
                     * compare c1 and c2
                     * either variable c1, c2 is -1 only if the corresponding string is finished
                     */
                    if (c1 == c2)
                    {
                        if (c1 < 0)
                        {
                            return 0;   /* c1==c2==-1 indicating end of strings */
                        }
                        c1 = c2 = -1;       /* make us fetch new code units */
                        continue;
                    }
                    else if (c1 < 0)
                    {
                        return -1;      /* string 1 ends before string 2 */
                    }
                    else if (c2 < 0)
                    {
                        return 1;       /* string 2 ends before string 1 */
                    }
                    /* c1!=c2 && c1>=0 && c2>=0 */

                    /* get complete code points for c1, c2 for lookups if either is a surrogate */
                    cp1 = c1;
                    if (UTF16.IsSurrogate((char)c1))
                    {
                        char c;

                        if (UTF16Plus.IsSurrogateLead(c1))
                        {
                            if (s1 != limit1 && char.IsLowSurrogate(c = cs1[s1]))
                            {
                                /* advance ++s1; only below if cp1 decomposes/case-folds */
                                cp1 = Character.ToCodePoint((char)c1, c);
                            }
                        }
                        else /* isTrail(c1) */
                        {
                            if (0 <= (s1 - 2) && char.IsHighSurrogate(c = cs1[s1 - 2]))
                            {
                                cp1 = Character.ToCodePoint(c, (char)c1);
                            }
                        }
                    }

                    cp2 = c2;
                    if (UTF16.IsSurrogate((char)c2))
                    {
                        char c;

                        if (UTF16Plus.IsSurrogateLead(c2))
                        {
                            if (s2 != limit2 && char.IsLowSurrogate(c = cs2[s2]))
                            {
                                /* advance ++s2; only below if cp2 decomposes/case-folds */
                                cp2 = Character.ToCodePoint((char)c2, c);
                            }
                        }
                        else /* isTrail(c2) */
                        {
                            if (0 <= (s2 - 2) && char.IsHighSurrogate(c = cs2[s2 - 2]))
                            {
                                cp2 = Character.ToCodePoint(c, (char)c2);
                            }
                        }
                    }

                    /*
                     * go down one level for each string
                     * continue with the main loop as soon as there is a real change
                     */

                    if (level1 == 0 && (options & COMPARE_IGNORE_CASE) != 0 &&
                        (length = csp.ToFullFolding(cp1, ref fold1, options)) >= 0)
                    {
                        /* cp1 case-folds to the code point "length" or to p[length] */
                        if (UTF16.IsSurrogate((char)c1))
                        {
                            if (UTF16Plus.IsSurrogateLead(c1))
                            {
                                /* advance beyond source surrogate pair if it case-folds */
                                ++s1;
                            }
                            else /* isTrail(c1) */
                            {
                                /*
                                    * we got a supplementary code point when hitting its trail surrogate,
                                    * therefore the lead surrogate must have been the same as in the other string;
                                    * compare this decomposition with the lead surrogate in the other string
                                    * remember that this simulates bulk text replacement:
                                    * the decomposition would replace the entire code point
                                    */
                                --s2;
                                c2 = cs2[s2 - 1];
                            }
                        }

                        /* push current level pointers */
                        stack1[0].Cs = cs1;
                        stack1[0].S = s1;
                        ++level1;

                        /* copy the folding result to fold1[] */
                        /* Java: the buffer was probably not empty, remove the old contents */
                        if (length <= UCaseProperties.MaxStringLength)
                        {
                            fold1.Delete(0, (fold1.Length - length) - 0); // ICU4N: Corrected 2nd parameter of Delete
                        }
                        else
                        {
                            fold1.Length = 0;
                            fold1.AppendCodePoint(length);
                        }

                        /* set next level pointers to case folding */
                        unsafe
                        {
                            cs1 = new ReadOnlySpan<char>(fold1.GetCharsPointer(), fold1.Length);
                        }
                        s1 = 0;
                        limit1 = fold1.Length;

                        /* get ready to read from decomposition, continue with loop */
                        c1 = -1;
                        continue;
                    }

                    if (level2 == 0 && (options & COMPARE_IGNORE_CASE) != 0 &&
                        (length = csp.ToFullFolding(cp2, ref fold2, options)) >= 0
                    )
                    {
                        /* cp2 case-folds to the code point "length" or to p[length] */
                        if (UTF16.IsSurrogate((char)c2))
                        {
                            if (UTF16Plus.IsSurrogateLead(c2))
                            {
                                /* advance beyond source surrogate pair if it case-folds */
                                ++s2;
                            }
                            else /* isTrail(c2) */
                            {
                                /*
                                 * we got a supplementary code point when hitting its trail surrogate,
                                 * therefore the lead surrogate must have been the same as in the other string;
                                 * compare this decomposition with the lead surrogate in the other string
                                 * remember that this simulates bulk text replacement:
                                 * the decomposition would replace the entire code point
                                 */
                                --s1;
                                c1 = cs1[s1 - 1];
                            }
                        }

                        /* push current level pointers */
                        stack2[0].Cs = cs2;
                        stack2[0].S = s2;
                        ++level2;

                        /* copy the folding result to fold2[] */
                        /* Java: the buffer was probably not empty, remove the old contents */
                        if (length <= UCaseProperties.MaxStringLength)
                        {
                            fold2.Delete(0, (fold2.Length - length) - 0); // ICU4N: Corrected 2nd parameter of Delete
                        }
                        else
                        {
                            fold2.Length = 0;
                            fold2.AppendCodePoint(length);
                        }

                        /* set next level pointers to case folding */
                        unsafe
                        {
                            cs2 = new ReadOnlySpan<char>(fold2.GetCharsPointer(), fold2.Length);
                        }
                        s2 = 0;
                        limit2 = fold2.Length;

                        /* get ready to read from decomposition, continue with loop */
                        c2 = -1;
                        continue;
                    }

                    if (level1 < 2 && (options & COMPARE_EQUIV) != 0 &&
                        nfcImpl.TryGetDecomposition(cp1, decomp1, out int decomp1Length)
                    )
                    {
                        /* cp1 decomposes into p[length] */
                        if (UTF16.IsSurrogate((char)c1))
                        {
                            if (UTF16Plus.IsSurrogateLead(c1))
                            {
                                /* advance beyond source surrogate pair if it decomposes */
                                ++s1;
                            }
                            else /* isTrail(c1) */
                            {
                                /*
                                 * we got a supplementary code point when hitting its trail surrogate,
                                 * therefore the lead surrogate must have been the same as in the other string;
                                 * compare this decomposition with the lead surrogate in the other string
                                 * remember that this simulates bulk text replacement:
                                 * the decomposition would replace the entire code point
                                 */
                                --s2;
                                c2 = cs2[s2 - 1];
                            }
                        }

                        /* push current level pointers */
                        stack1[level1].Cs = cs1;
                        stack1[level1].S = s1;
                        ++level1;

                        /* set empty intermediate level if skipped */
                        if (level1 < 2)
                        {
                            stack1[level1++].Cs = null;
                        }

                        /* set next level pointers to decomposition */
                        unsafe
                        {
                            cs1 = new ReadOnlySpan<char>((char*)Unsafe.AsPointer(ref decomp1[0]), decomp1Length);
                        }
                        s1 = 0;
                        limit1 = decomp1Length;

                        /* get ready to read from decomposition, continue with loop */
                        c1 = -1;
                        continue;
                    }

                    if (level2 < 2 && (options & COMPARE_EQUIV) != 0 &&
                        nfcImpl.TryGetDecomposition(cp2, decomp2, out int decomp2Length)
                    )
                    {
                        /* cp2 decomposes into p[length] */
                        if (UTF16.IsSurrogate((char)c2))
                        {
                            if (UTF16Plus.IsSurrogateLead(c2))
                            {
                                /* advance beyond source surrogate pair if it decomposes */
                                ++s2;
                            }
                            else /* isTrail(c2) */
                            {
                                /*
                                 * we got a supplementary code point when hitting its trail surrogate,
                                 * therefore the lead surrogate must have been the same as in the other string;
                                 * compare this decomposition with the lead surrogate in the other string
                                 * remember that this simulates bulk text replacement:
                                 * the decomposition would replace the entire code point
                                 */
                                --s1;
                                c1 = cs1[s1 - 1];
                            }
                        }

                        /* push current level pointers */
                        stack2[level2].Cs = cs2;
                        stack2[level2].S = s2;
                        ++level2;

                        /* set empty intermediate level if skipped */
                        if (level2 < 2)
                        {
                            stack2[level2++].Cs = null;
                        }

                        /* set next level pointers to decomposition */
                        unsafe
                        {
                            cs2 = new ReadOnlySpan<char>((char*)Unsafe.AsPointer(ref decomp2[0]), decomp2Length);
                        }
                        s2 = 0;
                        limit2 = decomp2Length;

                        /* get ready to read from decomposition, continue with loop */
                        c2 = -1;
                        continue;
                    }

                    /*
                     * no decomposition/case folding, max level for both sides:
                     * return difference result
                     *
                     * code point order comparison must not just return cp1-cp2
                     * because when single surrogates are present then the surrogate pairs
                     * that formed cp1 and cp2 may be from different string indexes
                     *
                     * example: { d800 d800 dc01 } vs. { d800 dc00 }, compare at second code units
                     * c1=d800 cp1=10001 c2=dc00 cp2=10000
                     * cp1-cp2>0 but c1-c2<0 and in fact in UTF-32 it is { d800 10001 } < { 10000 }
                     *
                     * therefore, use same fix-up as in ustring.c/uprv_strCompare()
                     * except: uprv_strCompare() fetches c=*s while this functions fetches c=*s++
                     * so we have slightly different pointer/start/limit comparisons here
                     */

                    if (c1 >= 0xd800 && c2 >= 0xd800 && (options & COMPARE_CODE_POINT_ORDER) != 0)
                    {
                        /* subtract 0x2800 from BMP code points to make them smaller than supplementary ones */
                        if (
                            (c1 <= 0xdbff && s1 != limit1 && char.IsLowSurrogate(cs1[s1])) ||
                            (char.IsLowSurrogate((char)c1) && 0 != (s1 - 1) && char.IsHighSurrogate(cs1[s1 - 2]))
                        )
                        {
                            /* part of a surrogate pair, leave >=d800 */
                        }
                        else
                        {
                            /* BMP code point - may be surrogate code point - make <d800 */
                            c1 -= 0x2800;
                        }

                        if (
                            (c2 <= 0xdbff && s2 != limit2 && char.IsLowSurrogate(cs2[s2])) ||
                            (char.IsLowSurrogate((char)c2) && 0 != (s2 - 1) && char.IsHighSurrogate(cs2[s2 - 2]))
                        )
                        {
                            /* part of a surrogate pair, leave >=d800 */
                        }
                        else
                        {
                            /* BMP code point - may be surrogate code point - make <d800 */
                            c2 -= 0x2800;
                        }
                    }

                    return c1 - c2;
                }
            }
            finally
            {
                fold1.Dispose();
                fold2.Dispose();
            }
        }

        // ICU4N: Factored out CharsAppendable by making Try... versions of methods instead of throwing when the buffer is full
    }
}
