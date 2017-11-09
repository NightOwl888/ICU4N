using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Low-level case mapping options and methods. Immutable.
    /// "Setters" return instances with the union of the current and new options set.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public abstract partial class CaseMap
    {
        [Obsolete("This API is ICU internal only.")]
        protected int internalOptions;

        private CaseMap(int opt) { internalOptions = opt; }

        private static int GetCaseLocale(CultureInfo locale)
        {
            if (locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            return UCaseProps.GetCaseLocale(locale);
        }

        /**
         * @return Lowercasing object with default options.
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public static Lower ToLower() { return Lower.DEFAULT; }
        /**
         * @return Uppercasing object with default options.
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public static Upper ToUpper() { return Upper.DEFAULT; }
        /**
         * @return Titlecasing object with default options.
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public static Title ToTitle() { return Title.DEFAULT; }
        /**
         * @return Case folding object with default options.
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public static Fold ToFold() { return Fold.DEFAULT; } // ICU4N specific - renamed from Fold() because of naming collision

        /**
         * Returns an instance that behaves like this one but
         * omits unchanged text when case-mapping with {@link Edits}.
         *
         * @return an options object with this option.
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public abstract CaseMap OmitUnchangedText();

        /// <summary>
        /// Lowercasing options and methods. Immutable.
        /// </summary>
        /// <seealso cref="ToLower()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public sealed partial class Lower : CaseMap
        {
            internal static readonly Lower DEFAULT = new Lower(0);
            private static readonly Lower OMIT_UNCHANGED = new Lower(CaseMapImpl.OMIT_UNCHANGED_TEXT);
            internal Lower(int opt)
                : base(opt)
            {
            }

            /**
             * {@inheritDoc}
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public override CaseMap OmitUnchangedText()
            {
                return OMIT_UNCHANGED;
            }

            // ICU4N specific - Apply(CultureInfo locale, ICharSequence src) moved to CaseMapExtension.tt

            // ICU4N specific - Apply(
            //    CultureInfo locale, string src, StringBuilder dest, Edits edits) moved to CaseMapExtension.tt
        }

        /// <summary>
        /// Uppercasing options and methods. Immutable.
        /// </summary>
        /// <seealso cref="ToUpper()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public sealed partial class Upper : CaseMap
        {
            internal static readonly Upper DEFAULT = new Upper(0);
            private static readonly Upper OMIT_UNCHANGED = new Upper(CaseMapImpl.OMIT_UNCHANGED_TEXT);
            internal Upper(int opt)
                : base(opt)
            {
            }

            /**
             * {@inheritDoc}
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public override CaseMap OmitUnchangedText()
            {
                return OMIT_UNCHANGED;
            }

            // ICU4N specific - Apply(CultureInfo locale, ICharSequence src) moved to CaseMapExtension.tt

            // ICU4N specific - Apply<T>(
            //    CultureInfo locale, string src, T dest, Edits edits) where T : IAppendable moved to CaseMapExtension.tt
        }

        /// <summary>
        /// Titlecasing options and methods. Immutable.
        /// </summary>
        /// <seealso cref="ToTitle()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public sealed partial class Title : CaseMap
        {
            internal static readonly Title DEFAULT = new Title(0);
            private static readonly Title OMIT_UNCHANGED = new Title(CaseMapImpl.OMIT_UNCHANGED_TEXT);
            private Title(int opt)
                        : base(opt)
            {
            }

            /**
             * Returns an instance that behaves like this one but
             * titlecases the string as a whole rather than each word.
             * (Titlecases only the character at index 0, possibly adjusted.)
             *
             * <p>It is an error to specify multiple titlecasing iterator options together,
             * including both an option and an explicit BreakIterator.
             *
             * @return an options object with this option.
             * @see #adjustToCased()
             * @draft ICU 60
             * @provisional This API might change or be removed in a future release.
             */
            public Title WholeString()
            {
                return new Title(CaseMapImpl.AddTitleIteratorOption(
                        internalOptions, CaseMapImpl.TITLECASE_WHOLE_STRING));
            }

            /**
             * Returns an instance that behaves like this one but
             * titlecases sentences rather than words.
             * (Titlecases only the first character of each sentence, possibly adjusted.)
             *
             * <p>It is an error to specify multiple titlecasing iterator options together,
             * including both an option and an explicit BreakIterator.
             *
             * @return an options object with this option.
             * @see #adjustToCased()
             * @draft ICU 60
             * @provisional This API might change or be removed in a future release.
             */
            public Title Sentences()
            {
                return new Title(CaseMapImpl.AddTitleIteratorOption(
                        internalOptions, CaseMapImpl.TITLECASE_SENTENCES));
            }

            /**
             * {@inheritDoc}
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public override CaseMap OmitUnchangedText()
            {
                if (internalOptions == 0 || internalOptions == CaseMapImpl.OMIT_UNCHANGED_TEXT)
                {
                    return OMIT_UNCHANGED;
                }
                return new Title(internalOptions | CaseMapImpl.OMIT_UNCHANGED_TEXT);
            }

            /**
             * Returns an instance that behaves like this one but
             * does not lowercase non-initial parts of words when titlecasing.
             *
             * <p>By default, titlecasing will titlecase the character at each
             * (possibly adjusted) BreakIterator index and
             * lowercase all other characters up to the next iterator index.
             * With this option, the other characters will not be modified.
             *
             * @return an options object with this option.
             * @see UCharacter#TITLECASE_NO_LOWERCASE
             * @see #adjustToCased()
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public Title NoLowercase()
            {
                return new Title(internalOptions | UCharacter.TITLECASE_NO_LOWERCASE);
            }

            /**
             * Returns an instance that behaves like this one but
             * does not adjust the titlecasing BreakIterator indexes;
             * titlecases exactly the characters at breaks from the iterator.
             *
             * <p>By default, titlecasing will take each break iterator index,
             * adjust it to the next relevant character (see {@link #adjustToCased()}),
             * and titlecase that one.
             *
             * <p>Other characters are lowercased.
             *
             * @return an options object with this option.
             * @see UCharacter#TITLECASE_NO_BREAK_ADJUSTMENT
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public Title NoBreakAdjustment()
            {
                return new Title(CaseMapImpl.AddTitleAdjustmentOption(
                        internalOptions, UCharacter.TITLECASE_NO_BREAK_ADJUSTMENT));
            }

            /**
             * Returns an instance that behaves like this one but
             * adjusts each titlecasing BreakIterator index to the next cased character.
             * (See the Unicode Standard, chapter 3, Default Case Conversion, R3 toTitlecase(X).)
             *
             * <p>This used to be the default index adjustment in ICU.
             * Since ICU 60, the default index adjustment is to the next character that is
             * a letter, number, symbol, or private use code point.
             * (Uncased modifier letters are skipped.)
             * The difference in behavior is small for word titlecasing,
             * but the new adjustment is much better for whole-string and sentence titlecasing:
             * It yields "49ers" and "«丰(abc)»" instead of "49Ers" and "«丰(Abc)»".
             *
             * <p>It is an error to specify multiple titlecasing adjustment options together.
             *
             * @return an options object with this option.
             * @see #noBreakAdjustment()
             * @draft ICU 60
             * @provisional This API might change or be removed in a future release.
             */
            public Title AdjustToCased()
            {
                return new Title(CaseMapImpl.AddTitleAdjustmentOption(
                        internalOptions, CaseMapImpl.TITLECASE_ADJUST_TO_CASED));
            }

            // ICU4N specific - Apply(CultureInfo locale, BreakIterator iter, ICharSequence src) moved to CaseMapExtension.tt

            // ICU4N specific - Apply<T>(
            //    CultureInfo locale, BreakIterator iter, ICharSequence src, T dest, Edits edits) where dest : IAppendable moved to CaseMapExtension.tt
        }

        /// <summary>
        /// Case folding options and methods. Immutable.
        /// </summary>
        /// <seealso cref="ToFold()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public sealed partial class Fold : CaseMap
        {
            internal static readonly Fold DEFAULT = new Fold(0);
            private static readonly Fold TURKIC = new Fold(UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I);
            private static readonly Fold OMIT_UNCHANGED = new Fold(CaseMapImpl.OMIT_UNCHANGED_TEXT);
            private static readonly Fold TURKIC_OMIT_UNCHANGED = new Fold(
                    UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I | CaseMapImpl.OMIT_UNCHANGED_TEXT);
            internal Fold(int opt)
                        : base(opt)
            {
            }

            /**
             * {@inheritDoc}
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public override CaseMap OmitUnchangedText()
            {
                return (internalOptions & UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0 ?
                        OMIT_UNCHANGED : TURKIC_OMIT_UNCHANGED;
            }

            /**
             * Returns an instance that behaves like this one but
             * handles dotted I and dotless i appropriately for Turkic languages (tr, az).
             *
             * <p>Uses the Unicode CaseFolding.txt mappings marked with 'T' that
             * are to be excluded for default mappings and
             * included for the Turkic-specific mappings.
             *
             * @return an options object with this option.
             * @see UCharacter#FOLD_CASE_EXCLUDE_SPECIAL_I
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            public Fold Turkic()
            {
                return (internalOptions & CaseMapImpl.OMIT_UNCHANGED_TEXT) == 0 ?
                        TURKIC : TURKIC_OMIT_UNCHANGED;
            }

            // ICU4N specific - Apply(ICharSequence src) moved to CaseMapExtension.tt

            // ICU4N specific - Apply(
            //    string src, StringBuilder dest, Edits edits) moved to CaseMapExtension.tt
        }
    }
}
