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
    public abstract class CaseMap
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

        /**
         * Lowercasing options and methods. Immutable.
         *
         * @see #toLower()
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public sealed class Lower : CaseMap
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

            /// <summary>
            /// Lowercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, string src)
            {
                return Apply(locale, src.ToCharSequence());
            } // ICU4N TODO: Add overloads with no culture

            /// <summary>
            /// Lowercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, StringBuilder src)
            {
                return Apply(locale, src.ToCharSequence());
            }

            /// <summary>
            /// Lowercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, char[] src)
            {
                return Apply(locale, src.ToCharSequence());
            }

            /// <summary>
            /// Lowercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal string Apply(CultureInfo locale, ICharSequence src)
            {
                return CaseMapImpl.ToLower(GetCaseLocale(locale), internalOptions, src);
            }

            /// <summary>
            /// Lowercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, string src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Lowercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, StringBuilder src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Lowercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, char[] src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Lowercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToLower(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal StringBuilder Apply(
                CultureInfo locale, ICharSequence src, StringBuilder dest, Edits edits)
            {
                return CaseMapImpl.ToLower(GetCaseLocale(locale), internalOptions, src, dest, edits);
            }
        }

        /**
         * Uppercasing options and methods. Immutable.
         *
         * @see #toUpper()
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public sealed class Upper : CaseMap
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

            /// <summary>
            /// Uppercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, string src)
            {
                return Apply(locale, src.ToCharSequence());
            }

            /// <summary>
            /// Uppercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, StringBuilder src)
            {
                return Apply(locale, src.ToCharSequence());
            }

            /// <summary>
            /// Uppercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, char[] src)
            {
                return Apply(locale, src.ToCharSequence());
            }

            /// <summary>
            /// Uppercases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal string Apply(CultureInfo locale, ICharSequence src)
            {
                return CaseMapImpl.ToUpper(GetCaseLocale(locale), internalOptions, src);
            }

            /// <summary>
            /// Uppercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, string src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Uppercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, StringBuilder src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Uppercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, char[] src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Uppercases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. edits can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal StringBuilder Apply(
            CultureInfo locale, ICharSequence src, StringBuilder dest, Edits edits)
            {
                return CaseMapImpl.ToUpper(GetCaseLocale(locale), internalOptions, src, dest, edits);
            }
        }

        /**
         * Titlecasing options and methods. Immutable.
         *
         * @see #toTitle()
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public sealed class Title : CaseMap
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

            /// <summary>
            /// Titlecases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, BreakIterator iter, string src)
            {
                return Apply(locale, iter, src.ToCharSequence());
            }

            /// <summary>
            /// Titlecases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, BreakIterator iter, StringBuilder src)
            {
                return Apply(locale, iter, src.ToCharSequence());
            }

            /// <summary>
            /// Titlecases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToUpper(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(CultureInfo locale, BreakIterator iter, char[] src)
            {
                return Apply(locale, iter, src.ToCharSequence());
            }

            /// <summary>
            /// Titlecases a string.
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <seealso cref="UCharacter.ToTitleCase(CultureInfo, String)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal string Apply(CultureInfo locale, BreakIterator iter, ICharSequence src)
            {
                if (iter == null && locale == null)
                {
                    locale = CultureInfo.CurrentCulture;
                }
                iter = CaseMapImpl.GetTitleBreakIterator(locale, internalOptions, iter);
                iter.SetText(src);
                return CaseMapImpl.ToTitle(GetCaseLocale(locale), internalOptions, iter, src);
            }

            /// <summary>
            /// Titlecases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToTitleCase(CultureInfo, String, BreakIterator, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, BreakIterator iter, string src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, iter, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Titlecases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToTitleCase(CultureInfo, String, BreakIterator, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, BreakIterator iter, StringBuilder src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, iter, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Titlecases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToTitleCase(CultureInfo, String, BreakIterator, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(
                CultureInfo locale, BreakIterator iter, char[] src, StringBuilder dest, Edits edits)
            {
                return Apply(locale, iter, src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Titlecases a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// Casing is locale-dependent and context-sensitive.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Titlecasing uses a break iterator to find the first characters of words
            /// that are to be titlecased. It titlecases those characters and lowercases
            /// all others. (This can be modified with options bits.)
            /// </remarks>
            /// <param name="locale">The locale ID. Can be null for <see cref="CultureInfo.CurrentCulture"/>.</param>
            /// <param name="iter">
            /// A break iterator to find the first characters of words that are to be titlecased.
            /// It is set to the source string (SetText())
            /// and used one or more times for iteration (First() and Next()).
            /// If null, then a word break iterator for the locale is used
            /// (or something equivalent).
            /// </param>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <seealso cref="UCharacter.ToTitleCase(CultureInfo, String, BreakIterator, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal StringBuilder Apply(
                    CultureInfo locale, BreakIterator iter, ICharSequence src, StringBuilder dest, Edits edits)
            {
                if (iter == null && locale == null)
                {
                    locale = CultureInfo.CurrentCulture;
                }
                iter = CaseMapImpl.GetTitleBreakIterator(locale, internalOptions, iter);
                iter.SetText(src);
                return CaseMapImpl.ToTitle(
                        GetCaseLocale(locale), internalOptions, iter, src, dest, edits);
            }
        }

        /**
         * Case folding options and methods. Immutable.
         *
         * @see #fold()
         * @draft ICU 59
         * @provisional This API might change or be removed in a future release.
         */
        public sealed class Fold : CaseMap
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

            /// <summary>
            /// Case-folds a string.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(string src)
            {
                return Apply(src.ToCharSequence());
            }

            /// <summary>
            /// Case-folds a string.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(StringBuilder src)
            {
                return Apply(src.ToCharSequence());
            }

            /// <summary>
            /// Case-folds a string.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public string Apply(char[] src)
            {
                return Apply(src.ToCharSequence());
            }

            /// <summary>
            /// Case-folds a string.
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <returns>The result string.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal string Apply(ICharSequence src)
            {
                return CaseMapImpl.Fold(internalOptions, src);
            }

            /// <summary>
            /// Case-folds a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(string src, StringBuilder dest, Edits edits)
            {
                return Apply(src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Case-folds a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(StringBuilder src, StringBuilder dest, Edits edits)
            {
                return Apply(src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Case-folds a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public StringBuilder Apply(char[] src, StringBuilder dest, Edits edits)
            {
                return Apply(src.ToCharSequence(), dest, edits);
            }

            /// <summary>
            /// Case-folds a string and optionally records edits (see <see cref="OmitUnchangedText"/>).
            /// The result may be longer or shorter than the original.
            /// </summary>
            /// <remarks>
            /// Case-folding is locale-independent and not context-sensitive,
            /// but there is an option for whether to include or exclude mappings for dotted I
            /// and dotless i that are marked with 'T' in CaseFolding.txt.
            /// </remarks>
            /// <param name="src">The original string.</param>
            /// <param name="dest">A buffer for the result string. Must not be null.</param>
            /// <param name="edits">
            /// Records edits for index mapping, working with styled text,
            /// and getting only changes (if any).
            /// This function calls edits.Reset() first. <paramref name="edits"/> can be null.
            /// </param>
            /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
            /// <see cref="UCharacter.FoldCase(string, int)"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            internal StringBuilder Apply(ICharSequence src, StringBuilder dest, Edits edits)
            {
                return CaseMapImpl.Fold(internalOptions, src, dest, edits);
            }
        }
    }
}
