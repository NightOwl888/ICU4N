using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Result values for <see cref="Normalizer.QuickCheck(string, Normalizer.Mode)"/> and
    /// <see cref="Normalizer2.QuickCheck(String)"/>.
    /// For details see Unicode Technical Report 15.
    /// </summary>
    public enum NormalizerQuickCheckResult
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


    public sealed class Normalizer
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        // The input text and our position in it
        private UCharacterIterator text;
        private Normalizer2 norm2;
        private Mode mode;
        private int options;

        // The normalization buffer is the result of normalization
        // of the source in [currentIndex..nextIndex[ .
        private int currentIndex;
        private int nextIndex;

        // A buffer for holding intermediate results
        private StringBuilder buffer;
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
            internal static readonly ModeImpl INSTANCE = new ModeImpl(Normalizer2.GetNFDInstance());
        }
        private sealed class NFKDModeImpl
        {
            internal static readonly ModeImpl INSTANCE = new ModeImpl(Normalizer2.GetNFKDInstance());
        }
        private sealed class NFCModeImpl
        {
            internal static readonly ModeImpl INSTANCE = new ModeImpl(Normalizer2.GetNFCInstance());
        }
        private sealed class NFKCModeImpl
        {
            internal static readonly ModeImpl INSTANCE = new ModeImpl(Normalizer2.GetNFKCInstance());
        }
        private sealed class FCDModeImpl
        {
            internal static readonly ModeImpl INSTANCE = new ModeImpl(Norm2AllModes.GetFCDNormalizer2());
        }

        private sealed class Unicode32
        {
            internal static readonly UnicodeSet INSTANCE = new UnicodeSet("[:age=3.2:]").Freeze();
        }
        private sealed class NFD32ModeImpl
        {
            internal static readonly ModeImpl INSTANCE =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.GetNFDInstance(),
                                                 Unicode32.INSTANCE));
        }
        private sealed class NFKD32ModeImpl
        {
            internal static readonly ModeImpl INSTANCE =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.GetNFKDInstance(),
                                                 Unicode32.INSTANCE));
        }
        private sealed class NFC32ModeImpl
        {
            internal static readonly ModeImpl INSTANCE =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.GetNFCInstance(),
                                                 Unicode32.INSTANCE));
        }
        private sealed class NFKC32ModeImpl
        {
            internal static readonly ModeImpl INSTANCE =
                new ModeImpl(new FilteredNormalizer2(Normalizer2.GetNFKCInstance(),
                                                 Unicode32.INSTANCE));
        }
        private sealed class FCD32ModeImpl
        {
            internal static readonly ModeImpl INSTANCE =
                new ModeImpl(new FilteredNormalizer2(Norm2AllModes.GetFCDNormalizer2(),
                                                 Unicode32.INSTANCE));
        }

        /**
         * Options bit set value to select Unicode 3.2 normalization
         * (except NormalizationCorrections).
         * At most one Unicode version can be selected at a time.
         *
         * @deprecated ICU 56 Use {@link FilteredNormalizer2} instead.
         */
        [Obsolete("ICU 56 Use FilteredNormalizer2 instead.")]
        public static readonly int UNICODE_3_2 = 0x20;

        /**
         * Constant indicating that the end of the iteration has been reached.
         * This is guaranteed to have the same value as {@link UCharacterIterator#DONE}.
         *
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public static readonly int DONE = UForwardCharacterIterator.DONE;

        /**
         * Constants for normalization modes.
         * <p>
         * The Mode class is not intended for public subclassing.
         * Only the Mode constants provided by the Normalizer class should be used,
         * and any fields or methods should not be called or overridden by users.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public abstract class Mode
        {
            /**
             * Sole constructor
             * @internal
             * @deprecated This API is ICU internal only.
             */
            //[Obsolete("This API is ICU internal only.")]
            protected Mode()
            {
            }

            /**
             * @internal
             * @deprecated This API is ICU internal only.
             */
            //[Obsolete("This API is ICU internal only.")]
            protected internal abstract Normalizer2 GetNormalizer2(int options);
        }

        private sealed class NONEMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options) { return Norm2AllModes.NOOP_NORMALIZER2; }
        }
        private sealed class NFDMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & UNICODE_3_2) != 0 ?
                        NFD32ModeImpl.INSTANCE.Normalizer2 : NFDModeImpl.INSTANCE.Normalizer2;
            }
        }
        private sealed class NFKDMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & UNICODE_3_2) != 0 ?
                        NFKD32ModeImpl.INSTANCE.Normalizer2 : NFKDModeImpl.INSTANCE.Normalizer2;
            }
        }
        private sealed class NFCMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & UNICODE_3_2) != 0 ?
                        NFC32ModeImpl.INSTANCE.Normalizer2 : NFCModeImpl.INSTANCE.Normalizer2;
            }
        }
        private sealed class NFKCMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & UNICODE_3_2) != 0 ?
                        NFKC32ModeImpl.INSTANCE.Normalizer2 : NFKCModeImpl.INSTANCE.Normalizer2;
            }
        }
        private sealed class FCDMode : Mode
        {
            protected internal override Normalizer2 GetNormalizer2(int options)
            {
                return (options & UNICODE_3_2) != 0 ?
                        FCD32ModeImpl.INSTANCE.Normalizer2 : FCDModeImpl.INSTANCE.Normalizer2;
            }
        }

        /**
         * No decomposition/composition.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode NONE = new NONEMode();

        /**
         * Canonical decomposition.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode NFD = new NFDMode();

        /**
         * Compatibility decomposition.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode NFKD = new NFKDMode();

        /**
         * Canonical decomposition followed by canonical composition.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode NFC = new NFCMode();

        /**
         * Default normalization.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode DEFAULT = NFC;

        /**
         * Compatibility decomposition followed by canonical composition.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode NFKC = new NFKCMode();

        /**
         * "Fast C or D" form.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly Mode FCD = new FCDMode();

        /**
         * Null operation for use with the {@link com.ibm.icu.text.Normalizer constructors}
         * and the static {@link #normalize normalize} method.  This value tells
         * the <tt>Normalizer</tt> to do nothing but return unprocessed characters
         * from the underlying String or CharacterIterator.  If you have code which
         * requires raw text at some times and normalized text at others, you can
         * use <tt>NO_OP</tt> for the cases where you want raw text, rather
         * than having a separate code path that bypasses <tt>Normalizer</tt>
         * altogether.
         * <p>
         * @see #setMode
         * @deprecated ICU 2.8. Use Nomalizer.NONE
         * @see #NONE
         */
        [Obsolete("ICU 2.8. Use Nomalizer.NONE")]
        public static readonly Mode NO_OP = NONE;

        /**
         * Canonical decomposition followed by canonical composition.  Used with the
         * {@link com.ibm.icu.text.Normalizer constructors} and the static
         * {@link #normalize normalize} method to determine the operation to be
         * performed.
         * <p>
         * If all optional features (<i>e.g.</i> {@link #IGNORE_HANGUL}) are turned
         * off, this operation produces output that is in
         * <a href=http://www.unicode.org/unicode/reports/tr15/>Unicode Canonical
         * Form</a>
         * <b>C</b>.
         * <p>
         * @see #setMode
         * @deprecated ICU 2.8. Use Normalier.NFC
         * @see #NFC
         */
        [Obsolete("ICU 2.8. Use Normalier.NFC")]
        public static readonly Mode COMPOSE = NFC;

        /**
         * Compatibility decomposition followed by canonical composition.
         * Used with the {@link com.ibm.icu.text.Normalizer constructors} and the static
         * {@link #normalize normalize} method to determine the operation to be
         * performed.
         * <p>
         * If all optional features (<i>e.g.</i> {@link #IGNORE_HANGUL}) are turned
         * off, this operation produces output that is in
         * <a href=http://www.unicode.org/unicode/reports/tr15/>Unicode Canonical
         * Form</a>
         * <b>KC</b>.
         * <p>
         * @see #setMode
         * @deprecated ICU 2.8. Use Normalizer.NFKC
         * @see #NFKC
         */
        [Obsolete("ICU 2.8. Use Normalier.NFKC")]
        public static readonly Mode COMPOSE_COMPAT = NFKC;

        /**
         * Canonical decomposition.  This value is passed to the
         * {@link com.ibm.icu.text.Normalizer constructors} and the static
         * {@link #normalize normalize}
         * method to determine the operation to be performed.
         * <p>
         * If all optional features (<i>e.g.</i> {@link #IGNORE_HANGUL}) are turned
         * off, this operation produces output that is in
         * <a href=http://www.unicode.org/unicode/reports/tr15/>Unicode Canonical
         * Form</a>
         * <b>D</b>.
         * <p>
         * @see #setMode
         * @deprecated ICU 2.8. Use Normalizer.NFD
         * @see #NFD
         */
        [Obsolete("ICU 2.8. Use Normalier.NFD")]
        public static readonly Mode DECOMP = NFD;

        /**
         * Compatibility decomposition.  This value is passed to the
         * {@link com.ibm.icu.text.Normalizer constructors} and the static
         * {@link #normalize normalize}
         * method to determine the operation to be performed.
         * <p>
         * If all optional features (<i>e.g.</i> {@link #IGNORE_HANGUL}) are turned
         * off, this operation produces output that is in
         * <a href=http://www.unicode.org/unicode/reports/tr15/>Unicode Canonical
         * Form</a>
         * <b>KD</b>.
         * <p>
         * @see #setMode
         * @deprecated ICU 2.8. Use Normalizer.NFKD
         * @see #NFKD
         */
        [Obsolete("ICU 2.8. Use Normalier.NFKD")]
        public static readonly Mode DECOMP_COMPAT = NFKD;

        /**
         * Option to disable Hangul/Jamo composition and decomposition.
         * This option applies to Korean text,
         * which can be represented either in the Jamo alphabet or in Hangul
         * characters, which are really just two or three Jamo combined
         * into one visual glyph.  Since Jamo takes up more storage space than
         * Hangul, applications that process only Hangul text may wish to turn
         * this option on when decomposing text.
         * <p>
         * The Unicode standard treates Hangul to Jamo conversion as a
         * canonical decomposition, so this option must be turned <b>off</b> if you
         * wish to transform strings into one of the standard
         * <a href="http://www.unicode.org/unicode/reports/tr15/" target="unicode">
         * Unicode Normalization Forms</a>.
         * <p>
         * @see #setOption
         * @deprecated ICU 2.8. This option is no longer supported.
         */
        [Obsolete("ICU 2.8. This option is no longer supported.")]
        public static readonly int IGNORE_HANGUL = 0x0001;

        // ICU4N specific - de-nested and renamed NormalizerQuickCheckResult
        ///**
        // * Result values for quickCheck().
        // * For details see Unicode Technical Report 15.
        // * @stable ICU 2.8
        // */
        //public sealed class QuickCheckResult
        //{
        //    //private int resultValue;
        //    internal QuickCheckResult(int value)
        //    {
        //        //resultValue=value;
        //    }
        //}
        ///**
        // * Indicates that string is not in the normalized format
        // * @stable ICU 2.8
        // */
        //public static readonly QuickCheckResult NO = new QuickCheckResult(0);

        ///**
        // * Indicates that string is in the normalized format
        // * @stable ICU 2.8
        // */
        //public static readonly QuickCheckResult YES = new QuickCheckResult(1);

        ///**
        // * Indicates it cannot be determined if string is in the normalized
        // * format without further thorough checks.
        // * @stable ICU 2.8
        // */
        //public static readonly QuickCheckResult MAYBE = new QuickCheckResult(2);

        /**
         * Option bit for compare:
         * Case sensitively compare the strings
         * @stable ICU 2.8
         */
        public static readonly int FOLD_CASE_DEFAULT = UCharacter.FOLD_CASE_DEFAULT;

        /**
         * Option bit for compare:
         * Both input strings are assumed to fulfill FCD conditions.
         * @stable ICU 2.8
         */
        public static readonly int INPUT_IS_FCD = 0x20000;

        /**
         * Option bit for compare:
         * Perform case-insensitive comparison.
         * @stable ICU 2.8
         */
        public static readonly int COMPARE_IGNORE_CASE = 0x10000;

        /**
         * Option bit for compare:
         * Compare strings in code point order instead of code unit order.
         * @stable ICU 2.8
         */
        public static readonly int COMPARE_CODE_POINT_ORDER = 0x8000;

        /**
         * Option value for case folding:
         * Use the modified set of mappings provided in CaseFolding.txt to handle dotted I
         * and dotless i appropriately for Turkic languages (tr, az).
         * @see UCharacter#FOLD_CASE_EXCLUDE_SPECIAL_I
         * @stable ICU 2.8
         */
        public static readonly int FOLD_CASE_EXCLUDE_SPECIAL_I = UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I;

        /**
         * Lowest-order bit number of compare() options bits corresponding to
         * normalization options bits.
         *
         * The options parameter for compare() uses most bits for
         * itself and for various comparison and folding flags.
         * The most significant bits, however, are shifted down and passed on
         * to the normalization implementation.
         * (That is, from compare(..., options, ...),
         * options&gt;&gt;COMPARE_NORM_OPTIONS_SHIFT will be passed on to the
         * internal normalization functions.)
         *
         * @see #compare
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static readonly int COMPARE_NORM_OPTIONS_SHIFT = 20;

        //-------------------------------------------------------------------------
        // Iterator constructors
        //-------------------------------------------------------------------------

        /**
         * Creates a new <tt>Normalizer</tt> object for iterating over the
         * normalized form of a given string.
         * <p>
         * The <tt>options</tt> parameter specifies which optional
         * <tt>Normalizer</tt> features are to be enabled for this object.
         * <p>
         * @param str  The string to be normalized.  The normalization
         *              will start at the beginning of the string.
         *
         * @param mode The normalization mode.
         *
         * @param opt Any optional features to be enabled.
         *            Currently the only available option is {@link #UNICODE_3_2}.
         *            If you want the default behavior corresponding to one of the
         *            standard Unicode Normalization Forms, use 0 for this argument.
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(string str, Mode mode, int opt)
        {
            this.text = UCharacterIterator.GetInstance(str);
            this.mode = mode;
            this.options = opt;
            norm2 = mode.GetNormalizer2(opt);
            buffer = new StringBuilder();
        }

        /**
         * Creates a new <tt>Normalizer</tt> object for iterating over the
         * normalized form of the given text.
         * <p>
         * @param iter  The input text to be normalized.  The normalization
         *              will start at the beginning of the string.
         *
         * @param mode  The normalization mode.
         *
         * @param opt Any optional features to be enabled.
         *            Currently the only available option is {@link #UNICODE_3_2}.
         *            If you want the default behavior corresponding to one of the
         *            standard Unicode Normalization Forms, use 0 for this argument.
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(CharacterIterator iter, Mode mode, int opt)
        {
            this.text = UCharacterIterator.GetInstance((CharacterIterator)iter.Clone());
            this.mode = mode;
            this.options = opt;
            norm2 = mode.GetNormalizer2(opt);
            buffer = new StringBuilder();
        }

        /**
         * Creates a new <tt>Normalizer</tt> object for iterating over the
         * normalized form of the given text.
         * <p>
         * @param iter  The input text to be normalized.  The normalization
         *              will start at the beginning of the string.
         *
         * @param mode  The normalization mode.
         * @param options The normalization options, ORed together (0 for no options).
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public Normalizer(UCharacterIterator iter, Mode mode, int options)
        {
            //try
            //{
            this.text = (UCharacterIterator)iter.Clone();
            this.mode = mode;
            this.options = options;
            norm2 = mode.GetNormalizer2(options);
            buffer = new StringBuilder();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    throw new ICUCloneNotSupportedException(e);
            //}
        }

        /**
         * Clones this <tt>Normalizer</tt> object.  All properties of this
         * object are duplicated in the new object, including the cloning of any
         * {@link CharacterIterator} that was passed in to the constructor
         * or to {@link #setText(CharacterIterator) setText}.
         * However, the text storage underlying
         * the <tt>CharacterIterator</tt> is not duplicated unless the
         * iterator's <tt>clone</tt> method does so.
         *
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public object Clone()
        {
            //try
            //{
            Normalizer copy = (Normalizer)base.MemberwiseClone();
            copy.text = (UCharacterIterator)text.Clone();
            copy.mode = mode;
            copy.options = options;
            copy.norm2 = norm2;
            copy.buffer = new StringBuilder(buffer.ToString());
            copy.bufferPos = bufferPos;
            copy.currentIndex = currentIndex;
            copy.nextIndex = nextIndex;
            return copy;
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    throw new ICUCloneNotSupportedException(e);
            //}
        }

        //--------------------------------------------------------------------------
        // Static Utility methods
        //--------------------------------------------------------------------------

        private static Normalizer2 GetComposeNormalizer2(bool compat, int options)
        {
            return (compat ? NFKC : NFC).GetNormalizer2(options);
        }
        private static Normalizer2 GetDecomposeNormalizer2(bool compat, int options)
        {
            return (compat ? NFKD : NFD).GetNormalizer2(options);
        }

        /**
         * Compose a string.
         * The string will be composed to according to the specified mode.
         * @param str        The string to compose.
         * @param compat     If true the string will be composed according to
         *                    NFKC rules and if false will be composed according to
         *                    NFC rules.
         * @return String    The composed string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(string str, bool compat)
        {
            return Compose(str, compat, 0);
        }

        /**
         * Compose a string.
         * The string will be composed to according to the specified mode.
         * @param str        The string to compose.
         * @param compat     If true the string will be composed according to
         *                    NFKC rules and if false will be composed according to
         *                    NFC rules.
         * @param options    The only recognized option is UNICODE_3_2
         * @return String    The composed string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Compose(string str, bool compat, int options)
        {
            return GetComposeNormalizer2(compat, options).Normalize(str);
        }

        /**
         * Compose a string.
         * The string will be composed to according to the specified mode.
         * @param source The char array to compose.
         * @param target A char buffer to receive the normalized text.
         * @param compat If true the char array will be composed according to
         *                NFKC rules and if false will be composed according to
         *                NFC rules.
         * @param options The normalization options, ORed together (0 for no options).
         * @return int   The total buffer size needed;if greater than length of
         *                result, the output was truncated.
         * @exception IndexOutOfBoundsException if target.length is less than the
         *             required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Compose(char[] source, char[] target, bool compat, int options)
        {
            return Compose(source, 0, source.Length, target, 0, target.Length, compat, options);
        }

        /**
         * Compose a string.
         * The string will be composed to according to the specified mode.
         * @param src       The char array to compose.
         * @param srcStart  Start index of the source
         * @param srcLimit  Limit index of the source
         * @param dest      The char buffer to fill in
         * @param destStart Start index of the destination buffer
         * @param destLimit End index of the destination buffer
         * @param compat If true the char array will be composed according to
         *                NFKC rules and if false will be composed according to
         *                NFC rules.
         * @param options The normalization options, ORed together (0 for no options).
         * @return int   The total buffer size needed;if greater than length of
         *                result, the output was truncated.
         * @exception IndexOutOfBoundsException if target.length is less than the
         *             required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Compose(char[] src, int srcStart, int srcLimit,
                              char[] dest, int destStart, int destLimit,
                              bool compat, int options)
        {
            CharBuffer srcBuffer = CharBuffer.Wrap(src, srcStart, srcLimit - srcStart);
            CharsAppendable app = new CharsAppendable(dest, destStart, destLimit);
            GetComposeNormalizer2(compat, options).Normalize(srcBuffer, app);
            return app.Length;
        }

        /**
         * Decompose a string.
         * The string will be decomposed to according to the specified mode.
         * @param str       The string to decompose.
         * @param compat    If true the string will be decomposed according to NFKD
         *                   rules and if false will be decomposed according to NFD
         *                   rules.
         * @return String   The decomposed string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(string str, bool compat)
        {
            return Decompose(str, compat, 0);
        }

        /**
         * Decompose a string.
         * The string will be decomposed to according to the specified mode.
         * @param str     The string to decompose.
         * @param compat  If true the string will be decomposed according to NFKD
         *                 rules and if false will be decomposed according to NFD
         *                 rules.
         * @param options The normalization options, ORed together (0 for no options).
         * @return String The decomposed string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Decompose(string str, bool compat, int options)
        {
            return GetDecomposeNormalizer2(compat, options).Normalize(str);
        }

        /**
         * Decompose a string.
         * The string will be decomposed to according to the specified mode.
         * @param source The char array to decompose.
         * @param target A char buffer to receive the normalized text.
         * @param compat If true the char array will be decomposed according to NFKD
         *                rules and if false will be decomposed according to
         *                NFD rules.
         * @return int   The total buffer size needed;if greater than length of
         *                result,the output was truncated.
         * @param options The normalization options, ORed together (0 for no options).
         * @exception IndexOutOfBoundsException if the target capacity is less than
         *             the required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Decompose(char[] source, char[] target, bool compat, int options)
        {
            return Decompose(source, 0, source.Length, target, 0, target.Length, compat, options);
        }

        /**
         * Decompose a string.
         * The string will be decomposed to according to the specified mode.
         * @param src       The char array to compose.
         * @param srcStart  Start index of the source
         * @param srcLimit  Limit index of the source
         * @param dest      The char buffer to fill in
         * @param destStart Start index of the destination buffer
         * @param destLimit End index of the destination buffer
         * @param compat If true the char array will be decomposed according to NFKD
         *                rules and if false will be decomposed according to
         *                NFD rules.
         * @param options The normalization options, ORed together (0 for no options).
         * @return int   The total buffer size needed;if greater than length of
         *                result,the output was truncated.
         * @exception IndexOutOfBoundsException if the target capacity is less than
         *             the required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Decompose(char[] src, int srcStart, int srcLimit,
                                char[] dest, int destStart, int destLimit,
                                bool compat, int options)
        {
            CharBuffer srcBuffer = CharBuffer.Wrap(src, srcStart, srcLimit - srcStart);
            CharsAppendable app = new CharsAppendable(dest, destStart, destLimit);
            GetDecomposeNormalizer2(compat, options).Normalize(srcBuffer, app);
            return app.Length;
        }

        /**
         * Normalizes a <tt>String</tt> using the given normalization operation.
         * <p>
         * The <tt>options</tt> parameter specifies which optional
         * <tt>Normalizer</tt> features are to be enabled for this operation.
         * Currently the only available option is {@link #UNICODE_3_2}.
         * If you want the default behavior corresponding to one of the standard
         * Unicode Normalization Forms, use 0 for this argument.
         * <p>
         * @param str       the input string to be normalized.
         * @param mode      the normalization mode
         * @param options   the optional features to be enabled.
         * @return String   the normalized string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(string str, Mode mode, int options)
        {
            return mode.GetNormalizer2(options).Normalize(str);
        }

        /**
         * Normalize a string.
         * The string will be normalized according to the specified normalization
         * mode and options.
         * @param src        The string to normalize.
         * @param mode       The normalization mode; one of Normalizer.NONE,
         *                    Normalizer.NFD, Normalizer.NFC, Normalizer.NFKC,
         *                    Normalizer.NFKD, Normalizer.DEFAULT
         * @return the normalized string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(string src, Mode mode)
        {
            return Normalize(src, mode, 0);
        }
        /**
         * Normalize a string.
         * The string will be normalized according to the specified normalization
         * mode and options.
         * @param source The char array to normalize.
         * @param target A char buffer to receive the normalized text.
         * @param mode   The normalization mode; one of Normalizer.NONE,
         *                Normalizer.NFD, Normalizer.NFC, Normalizer.NFKC,
         *                Normalizer.NFKD, Normalizer.DEFAULT
         * @param options The normalization options, ORed together (0 for no options).
         * @return int   The total buffer size needed;if greater than length of
         *                result, the output was truncated.
         * @exception    IndexOutOfBoundsException if the target capacity is less
         *                than the required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int normalize(char[] source, char[] target, Mode mode, int options)
        {
            return Normalize(source, 0, source.Length, target, 0, target.Length, mode, options);
        }

        /**
         * Normalize a string.
         * The string will be normalized according to the specified normalization
         * mode and options.
         * @param src       The char array to compose.
         * @param srcStart  Start index of the source
         * @param srcLimit  Limit index of the source
         * @param dest      The char buffer to fill in
         * @param destStart Start index of the destination buffer
         * @param destLimit End index of the destination buffer
         * @param mode      The normalization mode; one of Normalizer.NONE,
         *                   Normalizer.NFD, Normalizer.NFC, Normalizer.NFKC,
         *                   Normalizer.NFKD, Normalizer.DEFAULT
         * @param options The normalization options, ORed together (0 for no options).
         * @return int      The total buffer size needed;if greater than length of
         *                   result, the output was truncated.
         * @exception       IndexOutOfBoundsException if the target capacity is
         *                   less than the required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Normalize(char[] src, int srcStart, int srcLimit,
                                char[] dest, int destStart, int destLimit,
                                Mode mode, int options)
        {
            CharBuffer srcBuffer = CharBuffer.Wrap(src, srcStart, srcLimit - srcStart);
            CharsAppendable app = new CharsAppendable(dest, destStart, destLimit);
            mode.GetNormalizer2(options).Normalize(srcBuffer, app);
            return app.Length;
        }

        /**
         * Normalize a codepoint according to the given mode
         * @param char32    The input string to be normalized.
         * @param mode      The normalization mode
         * @param options   Options for use with exclusion set and tailored Normalization
         *                                   The only option that is currently recognized is UNICODE_3_2
         * @return String   The normalized string
         * @see #UNICODE_3_2
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(int char32, Mode mode, int options)
        {
            if (mode == NFD && options == 0)
            {
                String decomposition = Normalizer2.GetNFCInstance().GetDecomposition(char32);
                if (decomposition == null)
                {
                    decomposition = UTF16.ValueOf(char32);
                }
                return decomposition;
            }
            return Normalize(UTF16.ValueOf(char32), mode, options);
        }

        /**
         * Convenience method to normalize a codepoint according to the given mode
         * @param char32    The input string to be normalized.
         * @param mode      The normalization mode
         * @return String   The normalized string
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Normalize(int char32, Mode mode)
        {
            return Normalize(char32, mode, 0);
        }

        /**
         * Convenience method.
         *
         * @param source   string for determining if it is in a normalized format
         * @param mode     normalization format (Normalizer.NFC,Normalizer.NFD,
         *                  Normalizer.NFKC,Normalizer.NFKD)
         * @return         Return code to specify if the text is normalized or not
         *                     (Normalizer.YES, Normalizer.NO or Normalizer.MAYBE)
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static NormalizerQuickCheckResult QuickCheck(string source, Mode mode)
        {
            return QuickCheck(source, mode, 0);
        }

        /**
         * Performing quick check on a string, to quickly determine if the string is
         * in a particular normalization format.
         * Three types of result can be returned Normalizer.YES, Normalizer.NO or
         * Normalizer.MAYBE. Result Normalizer.YES indicates that the argument
         * string is in the desired normalized format, Normalizer.NO determines that
         * argument string is not in the desired normalized format. A
         * Normalizer.MAYBE result indicates that a more thorough check is required,
         * the user may have to put the string in its normalized form and compare
         * the results.
         *
         * @param source   string for determining if it is in a normalized format
         * @param mode     normalization format (Normalizer.NFC,Normalizer.NFD,
         *                  Normalizer.NFKC,Normalizer.NFKD)
         * @param options   Options for use with exclusion set and tailored Normalization
         *                                   The only option that is currently recognized is UNICODE_3_2
         * @return         Return code to specify if the text is normalized or not
         *                     (Normalizer.YES, Normalizer.NO or Normalizer.MAYBE)
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static NormalizerQuickCheckResult QuickCheck(string source, Mode mode, int options)
        {
            return mode.GetNormalizer2(options).QuickCheck(source);
        }

        /**
         * Convenience method.
         *
         * @param source Array of characters for determining if it is in a
         *                normalized format
         * @param mode   normalization format (Normalizer.NFC,Normalizer.NFD,
         *                Normalizer.NFKC,Normalizer.NFKD)
         * @param options   Options for use with exclusion set and tailored Normalization
         *                                   The only option that is currently recognized is UNICODE_3_2
         * @return       Return code to specify if the text is normalized or not
         *                (Normalizer.YES, Normalizer.NO or Normalizer.MAYBE)
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static NormalizerQuickCheckResult QuickCheck(char[] source, Mode mode, int options)
        {
            return QuickCheck(source, 0, source.Length, mode, options);
        }

        /**
         * Performing quick check on a string, to quickly determine if the string is
         * in a particular normalization format.
         * Three types of result can be returned Normalizer.YES, Normalizer.NO or
         * Normalizer.MAYBE. Result Normalizer.YES indicates that the argument
         * string is in the desired normalized format, Normalizer.NO determines that
         * argument string is not in the desired normalized format. A
         * Normalizer.MAYBE result indicates that a more thorough check is required,
         * the user may have to put the string in its normalized form and compare
         * the results.
         *
         * @param source    string for determining if it is in a normalized format
         * @param start     the start index of the source
         * @param limit     the limit index of the source it is equal to the length
         * @param mode      normalization format (Normalizer.NFC,Normalizer.NFD,
         *                   Normalizer.NFKC,Normalizer.NFKD)
         * @param options   Options for use with exclusion set and tailored Normalization
         *                                   The only option that is currently recognized is UNICODE_3_2
         * @return          Return code to specify if the text is normalized or not
         *                   (Normalizer.YES, Normalizer.NO or
         *                   Normalizer.MAYBE)
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static NormalizerQuickCheckResult QuickCheck(char[] source, int start,
                                              int limit, Mode mode, int options)
        {
            CharBuffer srcBuffer = CharBuffer.Wrap(source, start, limit - start);
            return mode.GetNormalizer2(options).QuickCheck(srcBuffer);
        }

        /**
         * Test if a string is in a given normalization form.
         * This is semantically equivalent to source.equals(normalize(source, mode)).
         *
         * Unlike quickCheck(), this function returns a definitive result,
         * never a "maybe".
         * For NFD, NFKD, and FCD, both functions work exactly the same.
         * For NFC and NFKC where quickCheck may return "maybe", this function will
         * perform further tests to arrive at a true/false result.
         * @param src       The input array of characters to be checked to see if
         *                   it is normalized
         * @param start     The strart index in the source
         * @param limit     The limit index in the source
         * @param mode      the normalization mode
         * @param options   Options for use with exclusion set and tailored Normalization
         *                                   The only option that is currently recognized is UNICODE_3_2
         * @return Boolean value indicating whether the source string is in the
         *         "mode" normalization form
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(char[] src, int start,
                                       int limit, Mode mode,
                                       int options)
        {
            CharBuffer srcBuffer = CharBuffer.Wrap(src, start, limit - start);
            return mode.GetNormalizer2(options).IsNormalized(srcBuffer);
        }

        /**
         * Test if a string is in a given normalization form.
         * This is semantically equivalent to source.equals(normalize(source, mode)).
         *
         * Unlike quickCheck(), this function returns a definitive result,
         * never a "maybe".
         * For NFD, NFKD, and FCD, both functions work exactly the same.
         * For NFC and NFKC where quickCheck may return "maybe", this function will
         * perform further tests to arrive at a true/false result.
         * @param str       the input string to be checked to see if it is
         *                   normalized
         * @param mode      the normalization mode
         * @param options   Options for use with exclusion set and tailored Normalization
         *                  The only option that is currently recognized is UNICODE_3_2
         * @see #isNormalized
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(string str, Mode mode, int options)
        {
            return mode.GetNormalizer2(options).IsNormalized(str);
        }

        /**
         * Convenience Method
         * @param char32    the input code point to be checked to see if it is
         *                   normalized
         * @param mode      the normalization mode
         * @param options   Options for use with exclusion set and tailored Normalization
         *                  The only option that is currently recognized is UNICODE_3_2
         *
         * @see #isNormalized
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static bool IsNormalized(int char32, Mode mode, int options)
        {
            return IsNormalized(UTF16.ValueOf(char32), mode, options);
        }

        /**
         * Compare two strings for canonical equivalence.
         * Further options include case-insensitive comparison and
         * code point order (as opposed to code unit order).
         *
         * Canonical equivalence between two strings is defined as their normalized
         * forms (NFD or NFC) being identical.
         * This function compares strings incrementally instead of normalizing
         * (and optionally case-folding) both strings entirely,
         * improving performance significantly.
         *
         * Bulk normalization is only necessary if the strings do not fulfill the
         * FCD conditions. Only in this case, and only if the strings are relatively
         * long, is memory allocated temporarily.
         * For FCD strings and short non-FCD strings there is no memory allocation.
         *
         * Semantically, this is equivalent to
         *   strcmp[CodePointOrder](foldCase(NFD(s1)), foldCase(NFD(s2)))
         * where code point order and foldCase are all optional.
         *
         * @param s1        First source character array.
         * @param s1Start   start index of source
         * @param s1Limit   limit of the source
         *
         * @param s2        Second source character array.
         * @param s2Start   start index of the source
         * @param s2Limit   limit of the source
         *
         * @param options A bit set of options:
         *   - FOLD_CASE_DEFAULT or 0 is used for default options:
         *     Case-sensitive comparison in code unit order, and the input strings
         *     are quick-checked for FCD.
         *
         *   - INPUT_IS_FCD
         *     Set if the caller knows that both s1 and s2 fulfill the FCD
         *     conditions.If not set, the function will quickCheck for FCD
         *     and normalize if necessary.
         *
         *   - COMPARE_CODE_POINT_ORDER
         *     Set to choose code point order instead of code unit order
         *
         *   - COMPARE_IGNORE_CASE
         *     Set to compare strings case-insensitively using case folding,
         *     instead of case-sensitively.
         *     If set, then the following case folding options are used.
         *
         *
         * @return &lt;0 or 0 or &gt;0 as usual for string comparisons
         *
         * @see #normalize
         * @see #FCD
         * @stable ICU 2.8
         */
        public static int Compare(char[] s1, int s1Start, int s1Limit,
                                  char[] s2, int s2Start, int s2Limit,
                                  int options)
        {
            if (s1 == null || s1Start < 0 || s1Limit < 0 ||
                s2 == null || s2Start < 0 || s2Limit < 0 ||
                s1Limit < s1Start || s2Limit < s2Start
            )
            {
                throw new ArgumentException();
            }
            return InternalCompare(CharBuffer.Wrap(s1, s1Start, s1Limit - s1Start),
                                   CharBuffer.Wrap(s2, s2Start, s2Limit - s2Start),
                                   options);
        }

        /**
         * Compare two strings for canonical equivalence.
         * Further options include case-insensitive comparison and
         * code point order (as opposed to code unit order).
         *
         * Canonical equivalence between two strings is defined as their normalized
         * forms (NFD or NFC) being identical.
         * This function compares strings incrementally instead of normalizing
         * (and optionally case-folding) both strings entirely,
         * improving performance significantly.
         *
         * Bulk normalization is only necessary if the strings do not fulfill the
         * FCD conditions. Only in this case, and only if the strings are relatively
         * long, is memory allocated temporarily.
         * For FCD strings and short non-FCD strings there is no memory allocation.
         *
         * Semantically, this is equivalent to
         *   strcmp[CodePointOrder](foldCase(NFD(s1)), foldCase(NFD(s2)))
         * where code point order and foldCase are all optional.
         *
         * @param s1 First source string.
         * @param s2 Second source string.
         *
         * @param options A bit set of options:
         *   - FOLD_CASE_DEFAULT or 0 is used for default options:
         *     Case-sensitive comparison in code unit order, and the input strings
         *     are quick-checked for FCD.
         *
         *   - INPUT_IS_FCD
         *     Set if the caller knows that both s1 and s2 fulfill the FCD
         *     conditions. If not set, the function will quickCheck for FCD
         *     and normalize if necessary.
         *
         *   - COMPARE_CODE_POINT_ORDER
         *     Set to choose code point order instead of code unit order
         *
         *   - COMPARE_IGNORE_CASE
         *     Set to compare strings case-insensitively using case folding,
         *     instead of case-sensitively.
         *     If set, then the following case folding options are used.
         *
         * @return &lt;0 or 0 or &gt;0 as usual for string comparisons
         *
         * @see #normalize
         * @see #FCD
         * @stable ICU 2.8
         */
        public static int Compare(string s1, string s2, int options)
        {
            return InternalCompare(s1.ToCharSequence(), s2.ToCharSequence(), options);
        }

        /**
         * Compare two strings for canonical equivalence.
         * Further options include case-insensitive comparison and
         * code point order (as opposed to code unit order).
         * Convenience method.
         *
         * @param s1 First source string.
         * @param s2 Second source string.
         *
         * @param options A bit set of options:
         *   - FOLD_CASE_DEFAULT or 0 is used for default options:
         *     Case-sensitive comparison in code unit order, and the input strings
         *     are quick-checked for FCD.
         *
         *   - INPUT_IS_FCD
         *     Set if the caller knows that both s1 and s2 fulfill the FCD
         *     conditions. If not set, the function will quickCheck for FCD
         *     and normalize if necessary.
         *
         *   - COMPARE_CODE_POINT_ORDER
         *     Set to choose code point order instead of code unit order
         *
         *   - COMPARE_IGNORE_CASE
         *     Set to compare strings case-insensitively using case folding,
         *     instead of case-sensitively.
         *     If set, then the following case folding options are used.
         *
         * @return &lt;0 or 0 or &gt;0 as usual for string comparisons
         *
         * @see #normalize
         * @see #FCD
         * @stable ICU 2.8
         */
        public static int Compare(char[] s1, char[] s2, int options)
        {
            return InternalCompare(CharBuffer.Wrap(s1), CharBuffer.Wrap(s2), options);
        }

        /**
         * Convenience method that can have faster implementation
         * by not allocating buffers.
         * @param char32a    the first code point to be checked against the
         * @param char32b    the second code point
         * @param options    A bit set of options
         * @stable ICU 2.8
         */
        public static int Compare(int char32a, int char32b, int options)
        {
            return InternalCompare(UTF16.ValueOf(char32a).ToCharSequence(), UTF16.ValueOf(char32b).ToCharSequence(), options | INPUT_IS_FCD);
        }

        /**
         * Convenience method that can have faster implementation
         * by not allocating buffers.
         * @param char32a   the first code point to be checked against
         * @param str2      the second string
         * @param options   A bit set of options
         * @stable ICU 2.8
         */
        public static int Compare(int char32a, string str2, int options)
        {
            return InternalCompare(UTF16.ValueOf(char32a).ToCharSequence(), str2.ToCharSequence(), options);
        }

        /* Concatenation of normalized strings --------------------------------- */
        /**
         * Concatenate normalized strings, making sure that the result is normalized
         * as well.
         *
         * If both the left and the right strings are in
         * the normalization form according to "mode",
         * then the result will be
         *
         * <code>
         *     dest=normalize(left+right, mode)
         * </code>
         *
         * With the input strings already being normalized,
         * this function will use next() and previous()
         * to find the adjacent end pieces of the input strings.
         * Only the concatenation of these end pieces will be normalized and
         * then concatenated with the remaining parts of the input strings.
         *
         * It is allowed to have dest==left to avoid copying the entire left string.
         *
         * @param left Left source array, may be same as dest.
         * @param leftStart start in the left array.
         * @param leftLimit limit in the left array (==length)
         * @param right Right source array.
         * @param rightStart start in the right array.
         * @param rightLimit limit in the right array (==length)
         * @param dest The output buffer; can be null if destStart==destLimit==0
         *              for pure preflighting.
         * @param destStart start in the destination array
         * @param destLimit limit in the destination array (==length)
         * @param mode The normalization mode.
         * @param options The normalization options, ORed together (0 for no options).
         * @return Length of output (number of chars) when successful or
         *          IndexOutOfBoundsException
         * @exception IndexOutOfBoundsException whose message has the string
         *             representation of destination capacity required.
         * @see #normalize
         * @see #next
         * @see #previous
         * @exception IndexOutOfBoundsException if target capacity is less than the
         *             required length
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static int Concatenate(char[] left, int leftStart, int leftLimit,
                                  char[] right, int rightStart, int rightLimit,
                                  char[] dest, int destStart, int destLimit,
                                  Normalizer.Mode mode, int options)
        {
            if (dest == null)
            {
                throw new ArgumentException();
            }

            /* check for overlapping right and destination */
            if (right == dest && rightStart < destLimit && destStart < rightLimit)
            {
                throw new ArgumentException("overlapping right and dst ranges");
            }

            /* allow left==dest */
            StringBuilder destBuilder = new StringBuilder(leftLimit - leftStart + rightLimit - rightStart + 16);
            destBuilder.Append(left, leftStart, leftLimit - leftStart);
            CharBuffer rightBuffer = CharBuffer.Wrap(right, rightStart, rightLimit - rightStart);
            mode.GetNormalizer2(options).Append(destBuilder, rightBuffer);
            int destLength = destBuilder.Length;
            if (destLength <= (destLimit - destStart))
            {
                //destBuilder.GetChars(0, destLength, dest, destStart);
                destBuilder.CopyTo(0, dest, destStart, destLength);
                return destLength;
            }
            else
            {
                throw new IndexOutOfRangeException(destLength.ToString());
            }
        }

        /**
         * Concatenate normalized strings, making sure that the result is normalized
         * as well.
         *
         * If both the left and the right strings are in
         * the normalization form according to "mode",
         * then the result will be
         *
         * <code>
         *     dest=normalize(left+right, mode)
         * </code>
         *
         * For details see concatenate
         *
         * @param left Left source string.
         * @param right Right source string.
         * @param mode The normalization mode.
         * @param options The normalization options, ORed together (0 for no options).
         * @return result
         *
         * @see #concatenate
         * @see #normalize
         * @see #next
         * @see #previous
         * @see #concatenate
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Concatenate(char[] left, char[] right, Mode mode, int options)
        {
            StringBuilder dest = new StringBuilder(left.Length + right.Length + 16).Append(left);
            return mode.GetNormalizer2(options).Append(dest, CharBuffer.Wrap(right)).ToString();
        }

        /**
         * Concatenate normalized strings, making sure that the result is normalized
         * as well.
         *
         * If both the left and the right strings are in
         * the normalization form according to "mode",
         * then the result will be
         *
         * <code>
         *     dest=normalize(left+right, mode)
         * </code>
         *
         * With the input strings already being normalized,
         * this function will use next() and previous()
         * to find the adjacent end pieces of the input strings.
         * Only the concatenation of these end pieces will be normalized and
         * then concatenated with the remaining parts of the input strings.
         *
         * @param left Left source string.
         * @param right Right source string.
         * @param mode The normalization mode.
         * @param options The normalization options, ORed together (0 for no options).
         * @return result
         *
         * @see #concatenate
         * @see #normalize
         * @see #next
         * @see #previous
         * @see #concatenate
         * @deprecated ICU 56 Use {@link Normalizer2} instead.
         */
        [Obsolete("ICU 56 Use Normalizer2 instead.")]
        public static string Concatenate(string left, string right, Mode mode, int options)
        {
            StringBuilder dest = new StringBuilder(left.Length + right.Length + 16).Append(left);
            return mode.GetNormalizer2(options).Append(dest, right).ToString();
        }

        /**
         * Gets the FC_NFKC closure value.
         * @param c The code point whose closure value is to be retrieved
         * @param dest The char array to receive the closure value
         * @return the length of the closure value; 0 if there is none
         * @deprecated ICU 56
         */
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
        /**
         * Gets the FC_NFKC closure value.
         * @param c The code point whose closure value is to be retrieved
         * @return String representation of the closure value; "" if there is none
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public static string GetFC_NFKC_Closure(int c)
        {
            // Compute the FC_NFKC_Closure on the fly:
            // We have the API for complete coverage of Unicode properties, although
            // this value by itself is not useful via API.
            // (What could be useful is a custom normalization table that combines
            // case folding and NFKC.)
            // For the derivation, see Unicode's DerivedNormalizationProps.txt.
            Normalizer2 nfkc = NFKCModeImpl.INSTANCE.Normalizer2;
            UCaseProps csp = UCaseProps.INSTANCE;
            // first: b = NFKC(Fold(a))
            StringBuilder folded = new StringBuilder();
            int folded1Length = csp.ToFullFolding(c, folded, 0);
            if (folded1Length < 0)
            {
                Normalizer2Impl nfkcImpl = ((Norm2AllModes.Normalizer2WithImpl)nfkc).impl;
                if (nfkcImpl.GetCompQuickCheck(nfkcImpl.GetNorm16(c)) != 0)
                {
                    return "";  // c does not change at all under CaseFolding+NFKC
                }
                folded.AppendCodePoint(c);
            }
            else
            {
                if (folded1Length > UCaseProps.MAX_STRING_LENGTH)
                {
                    folded.AppendCodePoint(folded1Length);
                }
            }
            string kc1 = nfkc.Normalize(folded);
            // second: c = NFKC(Fold(b))
            string kc2 = nfkc.Normalize(UCharacter.FoldCase(kc1, 0));
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

        //-------------------------------------------------------------------------
        // Iteration API
        //-------------------------------------------------------------------------

        /**
         * Return the current character in the normalized text.
         * @return The codepoint as an int
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int Current()
        {
            if (bufferPos < buffer.Length || NextNormalize())
            {
                return buffer.CodePointAt(bufferPos);
            }
            else
            {
                return DONE;
            }
        }

        /**
         * Return the next character in the normalized text and advance
         * the iteration position by one.  If the end
         * of the text has already been reached, {@link #DONE} is returned.
         * @return The codepoint as an int
         * @deprecated ICU 56
         */
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
                return DONE;
            }
        }


        /**
         * Return the previous character in the normalized text and decrement
         * the iteration position by one.  If the beginning
         * of the text has already been reached, {@link #DONE} is returned.
         * @return The codepoint as an int
         * @deprecated ICU 56
         */
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
                return DONE;
            }
        }

        /**
         * Reset the index to the beginning of the text.
         * This is equivalent to setIndexOnly(startIndex)).
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void Reset()
        {
            text.SetToStart();
            currentIndex = nextIndex = 0;
            ClearBuffer();
        }

        /**
         * Set the iteration position in the input text that is being normalized,
         * without any immediate normalization.
         * After setIndexOnly(), getIndex() will return the same index that is
         * specified here.
         *
         * @param index the desired index in the input text.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetIndexOnly(int index)
        {
            text.Index = index;  // validates index
            currentIndex = nextIndex = index;
            ClearBuffer();
        }

        /**
         * Set the iteration position in the input text that is being normalized
         * and return the first normalized character at that position.
         * <p>
         * <b>Note:</b> This method sets the position in the <em>input</em> text,
         * while {@link #next} and {@link #previous} iterate through characters
         * in the normalized <em>output</em>.  This means that there is not
         * necessarily a one-to-one correspondence between characters returned
         * by <tt>next</tt> and <tt>previous</tt> and the indices passed to and
         * returned from <tt>setIndex</tt> and {@link #getIndex}.
         * <p>
         * @param index the desired index in the input text.
         *
         * @return   the first normalized character that is the result of iterating
         *            forward starting at the given index.
         *
         * @throws IllegalArgumentException if the given index is less than
         *          {@link #getBeginIndex} or greater than {@link #getEndIndex}.
         * @deprecated ICU 3.2
         * @obsolete ICU 3.2
         */
        [Obsolete("ICU 3.2")]
        ///CLOVER:OFF
        public int SetIndex(int index)
        {
            SetIndexOnly(index);
            return Current();
        }
        ///CLOVER:ON
        /**
         * Retrieve the index of the start of the input text. This is the begin
         * index of the <tt>CharacterIterator</tt> or the start (i.e. 0) of the
         * <tt>String</tt> over which this <tt>Normalizer</tt> is iterating
         * @deprecated ICU 2.2. Use startIndex() instead.
         * @return The codepoint as an int
         * @see #startIndex
         */
        [Obsolete("ICU 2.2. Use StartIndex() instead.")]
        public int GetBeginIndex()
        {
            return 0;
        }

        /**
         * Retrieve the index of the end of the input text.  This is the end index
         * of the <tt>CharacterIterator</tt> or the length of the <tt>String</tt>
         * over which this <tt>Normalizer</tt> is iterating
         * @deprecated ICU 2.2. Use endIndex() instead.
         * @return The codepoint as an int
         * @see #endIndex
         */
        [Obsolete("ICU 2.2. Use EndIndex() instead.")]
        public int GetEndIndex()
        {
            return EndIndex;
        }
        /**
         * Return the first character in the normalized text.  This resets
         * the <tt>Normalizer's</tt> position to the beginning of the text.
         * @return The codepoint as an int
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int First()
        {
            Reset();
            return Next();
        }

        /**
         * Return the last character in the normalized text.  This resets
         * the <tt>Normalizer's</tt> position to be just before the
         * the input text corresponding to that normalized character.
         * @return The codepoint as an int
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int Last()
        {
            text.SetToLimit();
            currentIndex = nextIndex = text.Index;
            ClearBuffer();
            return Previous();
        }

        /**
         * Retrieve the current iteration position in the input text that is
         * being normalized.  This method is useful in applications such as
         * searching, where you need to be able to determine the position in
         * the input text that corresponds to a given normalized output character.
         * <p>
         * <b>Note:</b> This method sets the position in the <em>input</em>, while
         * {@link #next} and {@link #previous} iterate through characters in the
         * <em>output</em>.  This means that there is not necessarily a one-to-one
         * correspondence between characters returned by <tt>next</tt> and
         * <tt>previous</tt> and the indices passed to and returned from
         * <tt>setIndex</tt> and {@link #getIndex}.
         * @return The current iteration position
         * @deprecated ICU 56
         */
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

        /**
         * Retrieve the index of the start of the input text. This is the begin
         * index of the <tt>CharacterIterator</tt> or the start (i.e. 0) of the
         * <tt>String</tt> over which this <tt>Normalizer</tt> is iterating
         * @return The current iteration position
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int StartIndex
        {
            get { return 0; }
        }

        /**
         * Retrieve the index of the end of the input text.  This is the end index
         * of the <tt>CharacterIterator</tt> or the length of the <tt>String</tt>
         * over which this <tt>Normalizer</tt> is iterating
         * @return The current iteration position
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int EndIndex
        {
            get { return text.Length; }
        }

        //-------------------------------------------------------------------------
        // Iterator attributes
        //-------------------------------------------------------------------------
        /**
         * Set the normalization mode for this object.
         * <p>
         * <b>Note:</b>If the normalization mode is changed while iterating
         * over a string, calls to {@link #next} and {@link #previous} may
         * return previously buffers characters in the old normalization mode
         * until the iteration is able to re-sync at the next base character.
         * It is safest to call {@link #setText setText()}, {@link #first},
         * {@link #last}, etc. after calling <tt>setMode</tt>.
         * <p>
         * @param newMode the new mode for this <tt>Normalizer</tt>.
         * The supported modes are:
         * <ul>
         *  <li>{@link #NFC}    - Unicode canonical decompositiion
         *                        followed by canonical composition.
         *  <li>{@link #NFKC}   - Unicode compatibility decompositiion
         *                        follwed by canonical composition.
         *  <li>{@link #NFD}    - Unicode canonical decomposition
         *  <li>{@link #NFKD}   - Unicode compatibility decomposition.
         *  <li>{@link #NONE}   - Do nothing but return characters
         *                        from the underlying input text.
         * </ul>
         *
         * @see #getMode
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetMode(Mode newMode)
        {
            mode = newMode;
            norm2 = mode.GetNormalizer2(options);
        }
        /**
         * Return the basic operation performed by this <tt>Normalizer</tt>
         *
         * @see #setMode
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public Mode GetMode()
        {
            return mode;
        }
        /**
         * Set options that affect this <tt>Normalizer</tt>'s operation.
         * Options do not change the basic composition or decomposition operation
         * that is being performed , but they control whether
         * certain optional portions of the operation are done.
         * Currently the only available option is:
         *
         * <ul>
         *   <li>{@link #UNICODE_3_2} - Use Normalization conforming to Unicode version 3.2.
         * </ul>
         *
         * @param   option  the option whose value is to be set.
         * @param   value   the new setting for the option.  Use <tt>true</tt> to
         *                  turn the option on and <tt>false</tt> to turn it off.
         *
         * @see #getOption
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetOption(int option, bool value)
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

        /**
         * Determine whether an option is turned on or off.
         * <p>
         * @see #setOption
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int GetOption(int option)
        {
            if ((options & option) != 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /**
         * Gets the underlying text storage
         * @param fillIn the char buffer to fill the UTF-16 units.
         *         The length of the buffer should be equal to the length of the
         *         underlying text storage
         * @throws IndexOutOfBoundsException If the index passed for the array is invalid.
         * @see   #getLength
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int GetText(char[] fillIn)
        {
            return text.GetText(fillIn);
        }

        /**
         * Gets the length of underlying text storage
         * @return the length
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public int Length
        {
            get { return text.Length; }
        }

        /**
         * Returns the text under iteration as a string
         * @return a copy of the text under iteration.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public string GetText()
        {
            return text.GetText();
        }

        /**
         * Set the input text over which this <tt>Normalizer</tt> will iterate.
         * The iteration position is set to the beginning of the input text.
         * @param newText   The new string to be normalized.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetText(StringBuffer newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            if (newIter == null)
            {
                throw new InvalidOperationException("Could not create a new UCharacterIterator");
            }
            text = newIter;
            Reset();
        }

        /**
         * Set the input text over which this <tt>Normalizer</tt> will iterate.
         * The iteration position is set to the beginning of the input text.
         * @param newText   The new string to be normalized.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetText(char[] newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            if (newIter == null)
            {
                throw new InvalidOperationException("Could not create a new UCharacterIterator");
            }
            text = newIter;
            Reset();
        }

        /**
         * Set the input text over which this <tt>Normalizer</tt> will iterate.
         * The iteration position is set to the beginning of the input text.
         * @param newText   The new string to be normalized.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetText(String newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            if (newIter == null)
            {
                throw new InvalidOperationException("Could not create a new UCharacterIterator");
            }
            text = newIter;
            Reset();
        }

        /**
         * Set the input text over which this <tt>Normalizer</tt> will iterate.
         * The iteration position is set to the beginning of the input text.
         * @param newText   The new string to be normalized.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetText(CharacterIterator newText)
        {
            UCharacterIterator newIter = UCharacterIterator.GetInstance(newText);
            if (newIter == null)
            {
                throw new InvalidOperationException("Could not create a new UCharacterIterator");
            }
            text = newIter;
            Reset();
        }

        /**
         * Set the input text over which this <tt>Normalizer</tt> will iterate.
         * The iteration position is set to the beginning of the string.
         * @param newText   The new string to be normalized.
         * @deprecated ICU 56
         */
        [Obsolete("ICU 56")]
        public void SetText(UCharacterIterator newText)
        {
            //try
            //{
            UCharacterIterator newIter = (UCharacterIterator)newText.Clone();
            if (newIter == null)
            {
                throw new InvalidOperationException("Could not create a new UCharacterIterator");
            }
            text = newIter;
            Reset();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    throw new ICUCloneNotSupportedException("Could not clone the UCharacterIterator", e);
            //}
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
            StringBuilder segment = new StringBuilder().AppendCodePoint(c);
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
            norm2.Normalize(segment, buffer);
            return buffer.Length != 0;
        }

        private bool PreviousNormalize()
        {
            ClearBuffer();
            nextIndex = currentIndex;
            text.Index = currentIndex;
            StringBuilder segment = new StringBuilder();
            int c;
            while ((c = text.PreviousCodePoint()) >= 0)
            {
                if (c <= 0xffff)
                {
                    segment.Insert(0, (char)c);
                }
                else
                {
                    segment.Insert(0, Character.ToChars(c));
                }
                if (norm2.HasBoundaryBefore(c))
                {
                    break;
                }
            }
            currentIndex = text.Index;
            norm2.Normalize(segment, buffer);
            bufferPos = buffer.Length;
            return buffer.Length != 0;
        }

        /* compare canonically equivalent ------------------------------------------- */

        // TODO: Broaden the public compare(String, String, options) API like this. Ticket #7407
        private static int InternalCompare(ICharSequence s1, ICharSequence s2, int options)
        {
            int normOptions = (int)((uint)options >> COMPARE_NORM_OPTIONS_SHIFT);
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
                    n2 = NFD.GetNormalizer2(normOptions);
                }
                else
                {
                    n2 = FCD.GetNormalizer2(normOptions);
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

                if (spanQCYes1 < s1.Length)
                {
                    StringBuilder fcd1 = new StringBuilder(s1.Length + 16).Append(s1, 0, spanQCYes1);
                    s1 = n2.NormalizeSecondAndAppend(fcd1, s1.SubSequence(spanQCYes1, s1.Length)).ToCharSequence();
                }
                if (spanQCYes2 < s2.Length)
                {
                    StringBuilder fcd2 = new StringBuilder(s2.Length + 16).Append(s2, 0, spanQCYes2);
                    s2 = n2.NormalizeSecondAndAppend(fcd2, s2.SubSequence(spanQCYes2, s2.Length)).ToCharSequence();
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
         * String comparisons almost always yield results before processing both strings
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

        /* stack element for previous-level source/decomposition pointers */
        private sealed class CmpEquivLevel
        {
            public ICharSequence Cs { get; set; }
            public int S { get; set; }
        };
        private static CmpEquivLevel[] CreateCmpEquivLevelStack()
        {
            return new CmpEquivLevel[] {
            new CmpEquivLevel(), new CmpEquivLevel()
        };
        }

        /**
         * Internal option for unorm_cmpEquivFold() for decomposing.
         * If not set, just do strcasecmp().
         */
        private static readonly int COMPARE_EQUIV = 0x80000;

        /* internal function; package visibility for use by UTF16.StringComparator */
        /*package*/
        internal static int CmpEquivFold(ICharSequence cs1, ICharSequence cs2, int options)
        {
            Normalizer2Impl nfcImpl;
            UCaseProps csp;

            /* current-level start/limit - s1/s2 as current */
            int s1, s2, limit1, limit2;

            /* decomposition and case folding variables */
            int length;

            /* stacks of previous-level start/current/limit */
            CmpEquivLevel[] stack1 = null, stack2 = null;

            /* buffers for algorithmic decompositions */
            string decomp1, decomp2;

            /* case folding buffers, only use current-level start/limit */
            StringBuilder fold1, fold2;

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
                nfcImpl = Norm2AllModes.GetNFCInstance().impl;
            }
            else
            {
                nfcImpl = null;
            }
            if ((options & COMPARE_IGNORE_CASE) != 0)
            {
                csp = UCaseProps.INSTANCE;
                fold1 = new StringBuilder();
                fold2 = new StringBuilder();
            }
            else
            {
                csp = null;
                fold1 = fold2 = null;
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

                    if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c1))
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

                    if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c2))
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
                    (length = csp.ToFullFolding(cp1, fold1, options)) >= 0
                )
                {
                    /* cp1 case-folds to the code point "length" or to p[length] */
                    if (UTF16.IsSurrogate((char)c1))
                    {
                        if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c1))
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
                    if (stack1 == null)
                    {
                        stack1 = CreateCmpEquivLevelStack();
                    }
                    stack1[0].Cs = cs1;
                    stack1[0].S = s1;
                    ++level1;

                    /* copy the folding result to fold1[] */
                    /* Java: the buffer was probably not empty, remove the old contents */
                    if (length <= UCaseProps.MAX_STRING_LENGTH)
                    {
                        fold1.Delete(0, fold1.Length - length);
                    }
                    else
                    {
                        fold1.Length = 0;
                        fold1.AppendCodePoint(length);
                    }

                    /* set next level pointers to case folding */
                    cs1 = fold1.ToCharSequence();
                    s1 = 0;
                    limit1 = fold1.Length;

                    /* get ready to read from decomposition, continue with loop */
                    c1 = -1;
                    continue;
                }

                if (level2 == 0 && (options & COMPARE_IGNORE_CASE) != 0 &&
                    (length = csp.ToFullFolding(cp2, fold2, options)) >= 0
                )
                {
                    /* cp2 case-folds to the code point "length" or to p[length] */
                    if (UTF16.IsSurrogate((char)c2))
                    {
                        if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c2))
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
                    if (stack2 == null)
                    {
                        stack2 = CreateCmpEquivLevelStack();
                    }
                    stack2[0].Cs = cs2;
                    stack2[0].S = s2;
                    ++level2;

                    /* copy the folding result to fold2[] */
                    /* Java: the buffer was probably not empty, remove the old contents */
                    if (length <= UCaseProps.MAX_STRING_LENGTH)
                    {
                        fold2.Delete(0, fold2.Length - length);
                    }
                    else
                    {
                        fold2.Length = 0;
                        fold2.AppendCodePoint(length);
                    }

                    /* set next level pointers to case folding */
                    cs2 = fold2.ToCharSequence();
                    s2 = 0;
                    limit2 = fold2.Length;

                    /* get ready to read from decomposition, continue with loop */
                    c2 = -1;
                    continue;
                }

                if (level1 < 2 && (options & COMPARE_EQUIV) != 0 &&
                    (decomp1 = nfcImpl.GetDecomposition(cp1)) != null
                )
                {
                    /* cp1 decomposes into p[length] */
                    if (UTF16.IsSurrogate((char)c1))
                    {
                        if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c1))
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
                    if (stack1 == null)
                    {
                        stack1 = CreateCmpEquivLevelStack();
                    }
                    stack1[level1].Cs = cs1;
                    stack1[level1].S = s1;
                    ++level1;

                    /* set empty intermediate level if skipped */
                    if (level1 < 2)
                    {
                        stack1[level1++].Cs = null;
                    }

                    /* set next level pointers to decomposition */
                    cs1 = decomp1.ToCharSequence();
                    s1 = 0;
                    limit1 = decomp1.Length;

                    /* get ready to read from decomposition, continue with loop */
                    c1 = -1;
                    continue;
                }

                if (level2 < 2 && (options & COMPARE_EQUIV) != 0 &&
                    (decomp2 = nfcImpl.GetDecomposition(cp2)) != null
                )
                {
                    /* cp2 decomposes into p[length] */
                    if (UTF16.IsSurrogate((char)c2))
                    {
                        if (Normalizer2Impl.UTF16Plus.IsSurrogateLead(c2))
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
                    if (stack2 == null)
                    {
                        stack2 = CreateCmpEquivLevelStack();
                    }
                    stack2[level2].Cs = cs2;
                    stack2[level2].S = s2;
                    ++level2;

                    /* set empty intermediate level if skipped */
                    if (level2 < 2)
                    {
                        stack2[level2++].Cs = null;
                    }

                    /* set next level pointers to decomposition */
                    cs2 = decomp2.ToCharSequence();
                    s2 = 0;
                    limit2 = decomp2.Length;

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

        /**
         * An Appendable that writes into a char array with a capacity that may be
         * less than array.length.
         * (By contrast, CharBuffer will write beyond destLimit all the way up to array.length.)
         * <p>
         * An overflow is only reported at the end, for the old Normalizer API functions that write
         * to char arrays.
         */
        private sealed class CharsAppendable : IAppendable
        {
            public CharsAppendable(char[] dest, int destStart, int destLimit)
            {
                chars = dest;
                start = offset = destStart;
                limit = destLimit;
            }
            public int Length
            {
                get
                {
                    int len = offset - start;
                    if (offset <= limit)
                    {
                        return len;
                    }
                    else
                    {
                        throw new IndexOutOfRangeException(len.ToString());
                    }
                }
            }
            public IAppendable Append(char c)
            {
                if (offset < limit)
                {
                    chars[offset] = c;
                }
                ++offset;
                return this;
            }

            public IAppendable Append(string csq)
            {
                return Append(csq.ToCharSequence());
            }

            public IAppendable Append(string csq, int start, int end)
            {
                return Append(csq.ToCharSequence(), start, end);
            }

            public IAppendable Append(StringBuilder csq)
            {
                return Append(csq.ToCharSequence());
            }

            public IAppendable Append(StringBuilder csq, int start, int end)
            {
                return Append(csq.ToCharSequence(), start, end);
            }

            public IAppendable Append(char[] csq)
            {
                return Append(csq.ToCharSequence());
            }

            public IAppendable Append(char[] csq, int start, int end)
            {
                return Append(csq.ToCharSequence(), start, end);
            }

            public IAppendable Append(ICharSequence s)
            {
                return Append(s, 0, s.Length);
            }

            public IAppendable Append(ICharSequence s, int sStart, int sLimit)
            {
                int len = sLimit - sStart;
                if (len <= (limit - offset))
                {
                    while (sStart < sLimit)
                    {  // TODO: Is there a better way to copy the characters?
                        chars[offset++] = s[sStart++];
                    }
                }
                else
                {
                    offset += len;
                }
                return this;
            }

            private readonly char[] chars;
            private readonly int start, limit;
            private int offset;
        }
    }
}
